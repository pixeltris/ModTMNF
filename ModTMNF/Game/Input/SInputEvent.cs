using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SInputEvent
    {
        public IntPtr Address;

        public SInputEvent(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SInputEvent(IntPtr address)
        {
            return new SInputEvent(address);
        }
    }
}
