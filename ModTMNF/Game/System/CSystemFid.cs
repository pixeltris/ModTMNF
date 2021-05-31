using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CSystemFid
    {
        public IntPtr Address;

        public CSystemFid(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemFid(IntPtr address)
        {
            return new CSystemFid(address);
        }
    }
}
