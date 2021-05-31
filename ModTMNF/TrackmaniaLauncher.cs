using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace ModTMNF
{
    // Suspended state idea is taken / modified from the following
    // https://github.com/pixeltris/SonyAlphaUSB/blob/6b459641a2b7fa778e2a8acfa9067c841aca5f96/SonyAlphaUSB/WIALogger.cs#L861
    // NOTE: The above is an x64 loader, we are doing x86 here. Possible struct differences.

    /* LaunchAlt:
     * - Running `ModTMNF.exe` creates a new OS process `TmForever.exe` with a debugger attatched (execution blocked by the debugger).
     * - Using debugger events it waits for the main module `TmForever.exe` to be loaded into memory. It then modifies the entry point (`OEP`) of the module into an infinite loop (`JMP -2` / `EB FE`).
     * - `GetThreadContext` is used to check when the `EIP` is at the entry point address. Once there it detatches the debugger, injects `ModTMNF_ManagedLoader.dll` which then loads the .NET runtime and `ModTMNF.exe` into the process.
     * - `ModTMNF.exe` (injected) is then responsible for hooking functions / applying mods.
     * - Once the hooks/mod are set up, the entry point is then restored and execution continues (the game opens).
     */

    // The benefit of LaunchAlt is that we can hook early functions such as WinMain.
    // TODO: Improve Launch so that we can hook early functions, then remove LaunchAlt.
    // NOTE: LaunchAlt can sometimes instantly crash the game on launch. TODO: Fix this or remove LaunchAlt.

    /// <summary>
    /// Launches TmForever.exe and injects ModTMNF.
    /// </summary>
    static unsafe class TrackmaniaLauncher
    {
        public const string LoaderDll = "ModTMNF_ManagedLoader.dll";
        private static string LoaderDllPath = Path.Combine("ModTMNF", LoaderDll);

        public static bool Launch(out string launchError)
        {
            string exeName = "TmForever.exe";
            if (!File.Exists(exeName))
            {
                launchError = "Couldn't find '" + exeName + "'";
                return false;
            }
            if (!File.Exists(LoaderDllPath))
            {
                launchError = "Couldn't find '" + LoaderDll + "'";
                return false;
            }

            STARTUPINFO si = default(STARTUPINFO);
            PROCESS_INFORMATION pi = default(PROCESS_INFORMATION);

            if (DetourCreateProcessWithDll_Exported(exeName, null, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref si, out pi, LoaderDllPath, IntPtr.Zero) == 0)
            {
                launchError = "DetourCreateProcessWithDll failed " + Marshal.GetLastWin32Error();
            }

            launchError = null;
            return true;
        }

        public static bool LaunchAlt(out string launchError)
        {
            string exeName = "TmForever.exe";
            if (!File.Exists(exeName))
            {
                launchError = "Couldn't find '" + exeName + "'";
                return false;
            }
            if (!File.Exists(LoaderDllPath))
            {
                launchError = "Couldn't find '" + LoaderDll + "'";
                return false;
            }

            STARTUPINFO si = default(STARTUPINFO);
            PROCESS_INFORMATION pi = default(PROCESS_INFORMATION);

            try
            {
                bool success = CreateProcess(exeName, null, IntPtr.Zero, IntPtr.Zero, false, DEBUG_ONLY_THIS_PROCESS, IntPtr.Zero, null, ref si, out pi);
                if (!success)
                {
                    launchError = "CreateProcess failed. ErrorCode: " + Marshal.GetLastWin32Error();
                    return false;
                }

                IntPtr entryPoint = IntPtr.Zero;
                byte[] entryPointInst = new byte[2];

                success = false;
                bool complete = false;
                while (!complete)
                {
                    DEBUG_EVENT debugEvent;
                    if (!WaitForDebugEvent(out debugEvent, 5000))
                    {
                        break;
                    }

                    switch (debugEvent.dwDebugEventCode)
                    {
                        case CREATE_PROCESS_DEBUG_EVENT:
                            {
                                IntPtr hFile = debugEvent.CreateProcessInfo.hFile;
                                if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
                                {
                                    CloseHandle(hFile);
                                }
                            }
                            break;
                        case EXIT_PROCESS_DEBUG_EVENT:
                            complete = true;
                            break;
                        case LOAD_DLL_DEBUG_EVENT:
                            {
                                LOAD_DLL_DEBUG_INFO loadDll = debugEvent.LoadDll;

                                StealEntryPointResult stealResult = TryStealEntryPoint(exeName, ref pi, ref entryPoint, entryPointInst);
                                switch (stealResult)
                                {
                                    case StealEntryPointResult.FailGetModules:
                                        // Need to wait for more modules to load
                                        break;
                                    case StealEntryPointResult.FailAlloc:
                                    case StealEntryPointResult.FailRead:
                                    case StealEntryPointResult.FailWrite:
                                    case StealEntryPointResult.FailFindTargetModule:
                                        complete = true;
                                        entryPoint = IntPtr.Zero;
                                        break;
                                    case StealEntryPointResult.Success:
                                        complete = true;
                                        break;
                                }

                                IntPtr hFile = loadDll.hFile;
                                if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
                                {
                                    CloseHandle(hFile);
                                }
                            }
                            break;
                    }

                    ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                }

                if (entryPoint != IntPtr.Zero)
                {
                    CONTEXT32 context32 = default(CONTEXT32);
                    context32.ContextFlags = (uint)CONTEXT_FLAGS.CONTROL;
                    GetThreadContext(pi.hThread, ref context32);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                    {
                        DEBUG_EVENT debugEvent;
                        if (WaitForDebugEvent(out debugEvent, 1))
                        {
                            ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                        }
                        if (context32.Eip == (ulong)entryPoint)
                        {
                            // It appears we HAVE to detatch the debugger before injecting (otherwise we crash).
                            // This means there is a small window where the process could get stuck in an infinite loop hogging CPU.
                            // TODO: Come up with a better process hanging method or introduce some fail safes in the event that our current process is closed.
                            DebugSetProcessKillOnExit(false);
                            DebugActiveProcessStop((int)pi.dwProcessId);

                            // If we are at the entry point inject the dll and then restore the entry point instructions
                            if (DllInjector.Inject(pi.hProcess, LoaderDllPath))
                            {
                                // TODO: Change this to some sort of a OS/process signal once hooks are set up.
                                SuspendThread(pi.hThread);
                                Thread.Sleep(500);//add a delay as our C# code gets loaded on a seperate thread which can delay hooks

                                IntPtr byteCount;
                                if (!WriteProcessMemory(pi.hProcess, entryPoint, entryPointInst, (IntPtr)2, out byteCount) && (int)byteCount == 2)
                                {
                                    launchError = "Failed to fix entry point";
                                    return false;
                                }

                                ResumeThread(pi.hThread);

                                launchError = null;
                                return true;
                            }
                            else
                            {
                                launchError = "Failed to inject managed binary";
                                return false;
                            }
                        }
                        context32.ContextFlags = (uint)CONTEXT_FLAGS.CONTROL;
                        GetThreadContext(pi.hThread, ref context32);
                    }
                    launchError = "Entry point timed out";
                    return false;
                }
                else
                {
                    launchError = "No entry point";
                    return false;
                }
            }
            catch (Exception e)
            {
                if (pi.hProcess != IntPtr.Zero)
                {
                    TerminateProcess(pi.hProcess, 0);
                }
                launchError = "Exception: " + e;
                return false;
            }
            finally
            {
                if (pi.hThread != IntPtr.Zero)
                {
                    CloseHandle(pi.hThread);
                }
                if (pi.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pi.hProcess);
                }
            }
        }

        private static unsafe StealEntryPointResult TryStealEntryPoint(string exeName, ref PROCESS_INFORMATION pi, ref IntPtr entryPoint, byte[] entryPointInst)
        {
            int modSize = IntPtr.Size * 1024;
            IntPtr hMods = Marshal.AllocHGlobal(modSize);

            try
            {
                if (hMods == IntPtr.Zero)
                {
                    return StealEntryPointResult.FailAlloc;
                }

                int modsNeeded;
                bool gotZeroMods = false;
                while (!EnumProcessModulesEx(pi.hProcess, hMods, modSize, out modsNeeded, LIST_MODULES_ALL) || modsNeeded == 0)
                {
                    if (modsNeeded == 0)
                    {
                        if (!gotZeroMods)
                        {
                            Thread.Sleep(100);
                            gotZeroMods = true;
                            continue;
                        }
                        else
                        {
                            // process has exited?
                            return StealEntryPointResult.FailGetModules;
                        }
                    }

                    // try again w/ more space...
                    Marshal.FreeHGlobal(hMods);
                    hMods = Marshal.AllocHGlobal(modsNeeded);
                    if (hMods == IntPtr.Zero)
                    {
                        return StealEntryPointResult.FailGetModules;
                    }
                    modSize = modsNeeded;
                }

                int totalNumberofModules = (int)(modsNeeded / IntPtr.Size);
                for (int i = 0; i < totalNumberofModules; i++)
                {
                    IntPtr hModule = Marshal.ReadIntPtr(hMods, i * IntPtr.Size);

                    MODULEINFO moduleInfo;
                    if (GetModuleInformation(pi.hProcess, hModule, out moduleInfo, sizeof(MODULEINFO)))
                    {
                        StringBuilder moduleNameSb = new StringBuilder(1024);
                        if (GetModuleFileNameEx(pi.hProcess, hModule, moduleNameSb, moduleNameSb.Capacity) != 0)
                        {
                            try
                            {
                                string moduleName = Path.GetFileName(moduleNameSb.ToString());
                                if (moduleName.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                                {
                                    IntPtr byteCount;
                                    if (ReadProcessMemory(pi.hProcess, moduleInfo.EntryPoint, entryPointInst, (IntPtr)2, out byteCount) && (int)byteCount == 2)
                                    {
                                        // TODO: We should probably use VirtualProtect here to ensure read/write/execute

                                        byte[] infLoop = { 0xEB, 0xFE };// JMP -2
                                        if (WriteProcessMemory(pi.hProcess, moduleInfo.EntryPoint, infLoop, (IntPtr)infLoop.Length, out byteCount) &&
                                            (int)byteCount == infLoop.Length)
                                        {
                                            entryPoint = moduleInfo.EntryPoint;
                                            return StealEntryPointResult.Success;
                                        }
                                        else
                                        {
                                            return StealEntryPointResult.FailWrite;
                                        }
                                    }
                                    else
                                    {
                                        return StealEntryPointResult.FailRead;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                return StealEntryPointResult.FailFindTargetModule;
            }
            finally
            {
                if (hMods != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(hMods);
                }
            }
        }

        enum StealEntryPointResult
        {
            FailAlloc,
            FailGetModules,
            FailFindTargetModule,
            FailRead,
            FailWrite,
            Success,
        }

        [DllImport(TrackmaniaLauncher.LoaderDll)]
        static extern int DetourCreateProcessWithDll_Exported(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation, string lpDllName, IntPtr pfCreateProcessA);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern bool TerminateProcess(IntPtr hProcess, uint exitCode);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        static extern bool EnumProcessModulesEx([In] IntPtr hProcess, IntPtr lphModule, int cb, [Out] out int lpcbNeeded, int dwFilterFlag);

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, int cb);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        static extern bool WaitForDebugEvent(out DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ContinueDebugEvent(int processId, int threadId, uint continuteStatus);

        [DllImport("kernel32.dll")]
        static extern void DebugSetProcessKillOnExit(bool killOnExit);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DebugActiveProcessStop(int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr size, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static unsafe extern bool GetThreadContext(IntPtr hThread, ref CONTEXT32 lpContext);

        [Flags]
        enum ThreadAccess : uint
        {
            Terminate = 0x00001,
            SuspendResume = 0x00002,
            GetContext = 0x00008,
            SetContext = 0x00010,
            SetInformation = 0x00020,
            QueryInformation = 0x00040,
            SetThreadToken = 0x00080,
            Impersonate = 0x00100,
            DirectImpersonation = 0x00200,
            All = 0x1F03FF
        }

        const int DEBUG_ONLY_THIS_PROCESS = 0x00000002;
        const int CREATE_SUSPENDED = 0x00000004;

        const int LIST_MODULES_DEFAULT = 0x00;
        const int LIST_MODULES_32BIT = 0x01;
        const int LIST_MODULES_64BIT = 0x02;
        const int LIST_MODULES_ALL = 0x03;

        const uint CREATE_PROCESS_DEBUG_EVENT = 3;
        const uint EXIT_PROCESS_DEBUG_EVENT = 5;
        const uint LOAD_DLL_DEBUG_EVENT = 6;

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        const uint DBG_CONTINUE = 0x00010002;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DEBUG_EVENT
        {
            [FieldOffset(0)]
            public uint dwDebugEventCode;
            [FieldOffset(4)]
            public int dwProcessId;
            [FieldOffset(8)]
            public int dwThreadId;

            // x64(offset:16, size:164)
            // x86(offset:12, size:86)
            [FieldOffset(12)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
            public byte[] debugInfo;

            public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
            {
                get { return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>(); }
            }

            public LOAD_DLL_DEBUG_INFO LoadDll
            {
                get { return GetDebugInfo<LOAD_DLL_DEBUG_INFO>(); }
            }

            private T GetDebugInfo<T>() where T : struct
            {
                GCHandle handle = GCHandle.Alloc(this.debugInfo, GCHandleType.Pinned);
                T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return result;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LOAD_DLL_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr lpBaseOfDll;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CREATE_PROCESS_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr hProcess;
            public IntPtr hThread;
            public IntPtr lpBaseOfImage;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpThreadLocalBase;
            public IntPtr lpStartAddress;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FLOATING_SAVE_AREA
        {
            public uint Control;
            public uint Status;
            public uint Tag;
            public uint ErrorO;
            public uint ErrorS;
            public uint DataO;
            public uint DataS;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=80)]
            public byte[] RegisterArea;
            public uint State;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CONTEXT32
        {
            public uint ContextFlags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public FLOATING_SAVE_AREA FloatSave;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;
        }

        [Flags]
        enum CONTEXT_FLAGS : uint
        {
            i386 = 0x10000,
            i486 = 0x10000,   //  same as i386
            CONTROL = i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
            INTEGER = i386 | 0x02, // AX, BX, CX, DX, SI, DI
            SEGMENTS = i386 | 0x04, // DS, ES, FS, GS
            FLOATING_POINT = i386 | 0x08, // 387 state
            DEBUG_REGISTERS = i386 | 0x10, // DB 0-3,6,7
            EXTENDED_REGISTERS = i386 | 0x20, // cpu specific extensions
            FULL = CONTROL | INTEGER | SEGMENTS,
            ALL = CONTROL | INTEGER | SEGMENTS | FLOATING_POINT | DEBUG_REGISTERS | EXTENDED_REGISTERS
        }

        static class DllInjector
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern Int32 CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr size, out IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            const uint MEM_COMMIT = 0x1000;
            const uint MEM_RESERVE = 0x2000;
            const uint MEM_RELEASE = 0x8000;

            const uint PAGE_EXECUTE = 0x10;
            const uint PAGE_EXECUTE_READ = 0x20;
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint PAGE_EXECUTE_WRITECOPY = 0x80;
            const uint PAGE_NOACCESS = 0x01;

            public static bool Inject(Process process, string dllPath)
            {
                bool result = false;
                IntPtr hProcess = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, process.Id);
                if (hProcess != IntPtr.Zero)
                {
                    result = Inject(hProcess, dllPath);
                    CloseHandle(hProcess);
                }
                return result;
            }

            public static bool Inject(IntPtr process, string dllPath)
            {
                if (process == IntPtr.Zero)
                {
                    LogError("Process handle is 0");
                    return false;
                }

                if (!File.Exists(dllPath))
                {
                    LogError("Couldn't find the dll to inject (" + dllPath + ")");
                    return false;
                }

                //dllPath = Path.GetFullPath(dllPath);
                byte[] buffer = Encoding.ASCII.GetBytes(dllPath);

                IntPtr libAddr = IntPtr.Zero;
                IntPtr memAddr = IntPtr.Zero;
                IntPtr threadAddr = IntPtr.Zero;

                try
                {
                    if (process == IntPtr.Zero)
                    {
                        LogError("Unable to attach to process");
                        return false;
                    }

                    libAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    if (libAddr == IntPtr.Zero)
                    {
                        LogError("Unable to find address of LoadLibraryA");
                        return false;
                    }

                    memAddr = VirtualAllocEx(process, IntPtr.Zero, (IntPtr)buffer.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                    if (memAddr == IntPtr.Zero)
                    {
                        LogError("Unable to allocate memory in the target process");
                        return false;
                    }

                    IntPtr bytesWritten;
                    if (!WriteProcessMemory(process, memAddr, buffer, (IntPtr)buffer.Length, out bytesWritten) ||
                        (int)bytesWritten != buffer.Length)
                    {
                        LogError("Unable to write to target process memory");
                        return false;
                    }

                    IntPtr thread = CreateRemoteThread(process, IntPtr.Zero, IntPtr.Zero, libAddr, memAddr, 0, IntPtr.Zero);
                    if (thread == IntPtr.Zero)
                    {
                        LogError("Unable to start thread in target process");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    if (threadAddr != IntPtr.Zero)
                    {
                        CloseHandle(threadAddr);
                    }
                    if (memAddr != IntPtr.Zero)
                    {
                        VirtualFreeEx(process, memAddr, IntPtr.Zero, MEM_RELEASE);
                    }
                }
            }

            private static void LogError(string str)
            {
                string error = "DllInjector error: " + str + " - ErrorCode: " + Marshal.GetLastWin32Error();
                Console.WriteLine(error);
                System.Diagnostics.Debug.WriteLine(error);
            }
        }
    }
}
