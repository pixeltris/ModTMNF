using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwValueStd
    {
        public IntPtr Address;

        public CMwValueStd(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwValueStd(IntPtr address)
        {
            return new CMwValueStd(address);
        }
    }
}
