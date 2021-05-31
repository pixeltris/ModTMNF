using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CFastBuffer
    {
        public IntPtr Address;

        public CFastBuffer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CFastBuffer(IntPtr address)
        {
            return new CFastBuffer(address);
        }
    }

    public unsafe struct CFastBuffer<T>
    {
        public IntPtr Address;

        public CFastBuffer(IntPtr address)
        {
            Address = address;
        }
    }
}
