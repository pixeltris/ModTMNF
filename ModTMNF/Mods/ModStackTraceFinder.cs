using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Mods
{
    /// <summary>
    /// Trash class used to hook functions and dump stack traces when you're not sure of a given call chain.
    /// Put whatever hooks you want here to check stack traces, but remove it / comment it out after you're done.
    /// </summary>
    class ModStackTraceFinder : Mod
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void Del_Func1(IntPtr thisPtr, IntPtr player, IntPtr block);

        Hook<Del_Func1> hook1;

        protected override void OnApply()
        {
            //hook1 = Hook<Del_Func1>.Create((IntPtr)0x004023E0, OnFunc1);//CGbxGame::StartApp
            //hook1 = Hook<Del_Func1>.Create((IntPtr)0x00472300, OnFunc1);//CTrackManiaRace1P::OnFinishLine
        }

        void OnFunc1(IntPtr thisPtr, IntPtr player, IntPtr block)
        {
            Program.DebugBreak();
            Program.Log("OnFunc1");
            Program.Log(StackWalk64.GetCallstack());
            hook1.OriginalFunc(thisPtr, player, block);
        }
    }
}
