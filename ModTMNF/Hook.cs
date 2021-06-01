using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace ModTMNF
{
    public class Hook<T>
    {
        /// <summary>
        /// The address of the function to hook.
        /// </summary>
        public IntPtr Address;
        /// <summary>
        /// The managed function hook handler.
        /// </summary>
        public T Func;
        /// <summary>
        /// The address of Func.
        /// </summary>
        public IntPtr FuncPtr;
        /// <summary>
        /// A delegate for calling the original function.
        /// This points to a small stub containing the overwritten function bytes, followed by a JMP to the rest of the original function.
        /// </summary>
        public T OriginalFunc;
        /// <summary>
        /// The address of OriginalFunc.
        /// </summary>
        public IntPtr OriginalFuncPtr;

        public bool Enabled { get; private set; }

        /// <summary>
        /// Helper to get a delegate from an address (not really hook related).
        /// </summary>
        public static T GetFunc(IntPtr addr)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(addr, typeof(T));
        }

        /// <summary>
        /// Declaring is something that isn't ever designed to be hooked. This just to reuse code and keep the delegate alive.
        /// </summary>
        public static Hook<T> Declare(IntPtr address, Type func)
        {
            Hook<T> hook = new Hook<T>();
            hook.OriginalFunc = (T)(object)Marshal.GetDelegateForFunctionPointer(address, func);
            return hook;
        }

        /// <summary>
        /// Defer initializes a Hook instance, but doesn't apply it.
        /// </summary>
        public static Hook<T> Defer(IntPtr address, T func)
        {
            return Create(address, func, false);
        }

        /// <summary>
        /// Creates and applies a hook.
        /// </summary>
        public static Hook<T> Create(IntPtr address, T func)
        {
            return Create(address, func, true);
        }

        private static Hook<T> Create(IntPtr address, T func, bool enable)
        {
            Hook<T> hook = new Hook<T>();
            hook.Func = func;
            hook.FuncPtr = Marshal.GetFunctionPointerForDelegate((Delegate)(object)hook.Func);
            NativeDll.WL_CreateHook(address, hook.FuncPtr, ref hook.OriginalFuncPtr);
            hook.OriginalFunc = (T)(object)Marshal.GetDelegateForFunctionPointer(hook.OriginalFuncPtr, func.GetType());
            if (enable)
            {
                NativeDll.WL_EnableHook(address);
                hook.Enabled = true;
            }
            return hook;
        }

        public void Enable()
        {
            if (!Enabled)
            {
                NativeDll.WL_EnableHook(Address);
                Enabled = true;
            }
        }

        public void Disable()
        {
            if (Enabled)
            {
                NativeDll.WL_DisableHook(Address);
                Enabled = false;
            }
        }
    }

    public static class NativeDll
    {
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_InitHooks();
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_HookFunction(IntPtr target, IntPtr detour, ref IntPtr original);
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_CreateHook(IntPtr target, IntPtr detour, ref IntPtr original);
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_RemoveHook(IntPtr target);
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_EnableHook(IntPtr target);
        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern int WL_DisableHook(IntPtr target);

        [DllImport(TrackmaniaLauncher.LoaderDll)]
        public static extern Game.EMwClassId GetMwClassId(IntPtr address);
    }
}
