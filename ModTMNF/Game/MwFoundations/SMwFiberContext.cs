using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SMwFiberContext
    {
        public IntPtr Address;

        public SMwFiberContext(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwFiberContext(IntPtr address)
        {
            return new SMwFiberContext(address);
        }
    }
}
