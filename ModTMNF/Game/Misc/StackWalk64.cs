using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF
{
    sealed class StackWalk64
    {
        private static bool SymbolsLoaded = false;

        public static bool LoadSymbols()
        {
            IntPtr process = Native.GetCurrentProcess();
            return Native.SymInitialize(process, null, true);
        }

        private static Native.IMAGEHLP_MODULE64_SHORT GetModuleInfo(IntPtr process, ulong address)
        {
            Native.IMAGEHLP_MODULE64_SHORT moduleInfo = new Native.IMAGEHLP_MODULE64_SHORT();
            moduleInfo.SizeOfStruct = (uint)Marshal.SizeOf(typeof(Native.IMAGEHLP_MODULE64_SHORT));
            Native.SymGetModuleInfo64(process, address, ref moduleInfo);
            return moduleInfo;
        }

        public static string GetCallstack()
        {
            return GetCallstack(false, false);//true, true);
        }

        public static string GetCallstack(bool skipManaged, bool skipFirstFrame)
        {
            Native.STACKFRAME64 stackFrame = new Native.STACKFRAME64();
            Native.CONTEXT context = new Native.CONTEXT();
            context.ContextFlags = (uint)Native.CONTEXT_FLAGS.CONTEXT_FULL;

            IntPtr process = Native.GetCurrentProcess();
            IntPtr thread = Native.GetCurrentThread();

            Native.RtlCaptureContext(ref context);
            context.ContextFlags = (uint)Native.CONTEXT_FLAGS.CONTEXT_FULL;

            if (!SymbolsLoaded)
            {
                LoadSymbols();
            }

            stackFrame.AddrPC.Offset = context.Eip;
            stackFrame.AddrPC.Mode = Native.ADDRESS_MODE.AddrModeFlat;
            stackFrame.AddrStack.Offset = context.Esp;
            stackFrame.AddrStack.Mode = Native.ADDRESS_MODE.AddrModeFlat;

            Native.IMAGEHLP_SYMBOL64 symbolInfo = new Native.IMAGEHLP_SYMBOL64();
            symbolInfo.MaxNameLen = Native.MAX_SYMBOL_NAME;
            symbolInfo.SizeOfStruct = (uint)(Marshal.SizeOf(typeof(Native.SYMBOL_INFO)) - Native.MAX_SYMBOL_NAME);

            Program.DebugBreak();
            StringBuilder callstack = new StringBuilder();
            bool foundManagedDll = false;
            bool foundUnmanagedDll = false;

            for (int frame = 0; ; frame++)
            {
                if (!Native.StackWalk64(Native.IMAGE_FILE_MACHINE_I386, process, thread, ref stackFrame, ref context, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                {
                    break;
                }

                string moduleName = GetModuleInfo(process, stackFrame.AddrPC.Offset).ModuleName;

                if (skipManaged)
                {
                    if (!foundManagedDll)
                    {
                        if (moduleName == "mscorwks")
                        {
                            foundManagedDll = true;
                        }
                        if (stackFrame.AddrPC.Offset == 0)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (!foundUnmanagedDll)
                    {
                        if (moduleName != "mscorwks")
                        {
                            foundUnmanagedDll = true;
                            frame = 0;
                        }
                        else if (stackFrame.AddrPC.Offset == 0)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                if (skipFirstFrame && frame == 0)
                {
                    if (stackFrame.AddrPC.Offset == 0)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                ulong displacement = 0;
                Native.SetLastError(0);
                Analysis.SymbolsHelper.SymbolInfo sym = Analysis.SymbolsHelper.ResolveFunction((uint)stackFrame.AddrPC.Offset);
                if (sym != null)
                {
                    callstack.AppendLine(string.Format("{0}: {1} ({2}) ({3} - {4})", frame, stackFrame.AddrPC.Offset.ToString("X8"), moduleName, sym.FuncComp.FullName, sym.Address.ToString("X8")));
                }
                else if (Native.SymGetSymFromAddr64(process, stackFrame.AddrPC.Offset, ref displacement, ref symbolInfo))
                {
                    callstack.AppendLine(string.Format("{0}: {1} ({2}) ({3})", frame, stackFrame.AddrPC.Offset.ToString("X8"), moduleName, symbolInfo.Name));
                }
                else
                {
                    int error = Native.GetLastError();
                    if (error != 0)
                    {
                        callstack.AppendLine(string.Format("{0}: {1} ({2}) (Error: {3})", frame, stackFrame.AddrPC.Offset.ToString("X8"), moduleName, error));
                    }
                    else
                    {
                        callstack.AppendLine(string.Format("{0}: {1} ({2}) (Unknown)", frame, stackFrame.AddrPC.Offset.ToString("X8"), moduleName));
                    }
                }

                if (stackFrame.AddrReturn.Offset == 0)
                {
                    break;
                }
            }
            return callstack.ToString();
        }

        class Native
        {
            public const int MAX_SYMBOL_NAME = 1024;
            public const uint IMAGE_FILE_MACHINE_I386 = 0x014c;

            public struct STACKFRAME64
            {
                public ADDRESS64 AddrPC;
                public ADDRESS64 AddrReturn;
                public ADDRESS64 AddrFrame;
                public ADDRESS64 AddrStack;
                public ADDRESS64 AddrBStore;
                public IntPtr FuncTableEntry;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public ulong[] Params;
                public bool Far;
                public bool Virtual;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public ulong[] Reserved;
                public KDHELP64 KdHelp;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ADDRESS64
            {
                public ulong Offset;
                public ushort Segment;
                public ADDRESS_MODE Mode;
            }

            public enum ADDRESS_MODE
            {
                AddrMode1616,
                AddrMode1632,
                AddrModeReal,
                AddrModeFlat
            }

            public struct KDHELP64
            {
                public ulong Thread;
                public uint ThCallbackStack;
                public uint ThCallbackBStore;
                public uint NextCallback;
                public uint FramePointer;
                public ulong KiCallUserMode;
                public ulong KeUserCallbackDispatcher;
                public ulong SystemRangeStart;
                public ulong KiUserExceptionDispatcher;
                public ulong StackBase;
                public ulong StackLimit;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
                public ulong[] Reserved;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CONTEXT
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

            [StructLayout(LayoutKind.Sequential)]
            public struct FLOATING_SAVE_AREA
            {
                public uint ControlWord;
                public uint StatusWord;
                public uint TagWord;
                public uint ErrorOffset;
                public uint ErrorSelector;
                public uint DataOffset;
                public uint DataSelector;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
                public byte[] RegisterArea;
                public uint Cr0NpxState;
            }

            public enum CONTEXT_FLAGS : long
            {
                CONTEXT_i386 = 0x00010000,    // this assumes that i386 and
                CONTEXT_i486 = 0x00010000,    // i486 have identical context records

                // end_wx86

                CONTEXT_CONTROL = (CONTEXT_i386 | 0x00000001L), // SS:SP, CS:IP, FLAGS, BP
                CONTEXT_INTEGER = (CONTEXT_i386 | 0x00000002L), // AX, BX, CX, DX, SI, DI
                CONTEXT_SEGMENTS = (CONTEXT_i386 | 0x00000004L), // DS, ES, FS, GS
                CONTEXT_FLOATING_POINT = (CONTEXT_i386 | 0x00000008L), // 387 state
                CONTEXT_DEBUG_REGISTERS = (CONTEXT_i386 | 0x00000010L), // DB 0-3,6,7
                CONTEXT_EXTENDED_REGISTERS = (CONTEXT_i386 | 0x00000020L), // cpu specific extensions

                CONTEXT_FULL = (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS),

                CONTEXT_ALL = (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS)
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SYMBOL_INFO
            {
                public uint SizeOfStruct;
                public uint TypeIndex;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public ulong[] Reserved;
                public uint Index;
                public uint Size;
                public ulong ModBase;
                public uint Flags;
                public ulong Value;
                public ulong Address;
                public uint Register;
                public uint Scope;
                public uint Tag;
                public uint NameLen;
                public uint MaxNameLen;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Native.MAX_SYMBOL_NAME)]//temp fixed length
                public string Name;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEHLP_SYMBOL64
            {
                public uint SizeOfStruct;
                public ulong Address;
                public uint Size;
                public uint Flags;
                public uint MaxNameLen;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Native.MAX_SYMBOL_NAME)]//temp fixed length
                public string Name;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEHLP_MODULE64_SHORT
            {
                public uint SizeOfStruct;
                public ulong BaseOfImage;
                public uint ImageSize;
                public uint TimeDateStamp;
                public uint CheckSum;
                public uint NumSyms;
                public SYM_TYPE SymType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string ModuleName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
                public string ImageName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
                public string LoadedImageName;
            }

            public enum SYM_TYPE
            {
                SymNone = 0,
                SymCoff,
                SymCv,
                SymPdb,
                SymExport,
                SymDeferred,
                SymSym,
                SymDia,
                SymVirtual,
                NumSymTypes
            }

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentProcess();

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentThread();

            [DllImport("kernel32.dll")]
            public static extern void RtlCaptureContext(ref CONTEXT context);

            [DllImport("Dbghelp.dll")]
            public static extern bool SymFromAddr(IntPtr hProcess, ulong address, ref ulong displacement, ref SYMBOL_INFO symbol);

            [DllImport("Dbghelp.dll")]
            public static extern bool SymInitialize(IntPtr hProcess, string userSearchPath, bool fInvadeProcess);

            [DllImport("Dbghelp.dll")]
            public static extern bool StackWalk64(uint machineType, IntPtr hProcess, IntPtr hThread, ref STACKFRAME64 stackFrame, ref CONTEXT contextRecord, IntPtr readMemoryRoutine, IntPtr functionTableAccessRoutine, IntPtr getModuleBaseRoutine, IntPtr translateAddress);

            [DllImport("Dbghelp.dll")]
            public static extern bool SymGetSymFromAddr64(IntPtr hProcess, ulong address, ref ulong displacement, ref IMAGEHLP_SYMBOL64 symbol);

            [DllImport("Dbghelp.dll")]
            public static extern bool SymGetModuleInfo64(IntPtr hProcess, ulong address, ref IMAGEHLP_MODULE64_SHORT moduleInfo);

            [DllImport("kernel32.dll")]
            public static extern Int32 GetLastError();

            [DllImport("kernel32.dll")]
            public static extern void SetLastError(int dwErrCode);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string lpFileName);
        }
    }
}