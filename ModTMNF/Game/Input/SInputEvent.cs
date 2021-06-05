using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SInputEvent
    {
        public SInputActionDesc Action;
        public int Value;

        public SInputEvent(SInputActionDesc action, int value)
        {
            Action = action;
            Value = value;
        }

        /// <summary>
        /// Use this when referencing a global static SInputActionDesc address (SInputActionDesc**)
        /// </summary>
        public static SInputEvent CreateStaticPtr(IntPtr staticActionPtrPtr, int value)
        {
            return new SInputEvent(*(IntPtr*)staticActionPtrPtr, value);
        }
    }
}
