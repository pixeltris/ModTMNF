using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public unsafe struct SMwTimedValueInstant
    {
        /// <summary>
        /// Time that the input event was stored
        /// </summary>
        [FieldOffset(0)]
        public uint Time;
        
        /// <summary>
        /// SInputActionDesc
        /// </summary>
        [FieldOffset(4)]
        public IntPtr ObjPtr;
        
        /// <summary>
        /// The value of the input
        /// </summary>
        [FieldOffset(8)]
        public int Value;
        
        /// <summary>
        /// The value of the input (floating point)
        /// </summary>
        [FieldOffset(8)]
        public float FloatValue;
    }
}
