using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CSystemFidParameters
    {
        public IntPtr Address;

        public CSystemFidParameters(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemFidParameters(IntPtr address)
        {
            return new CSystemFidParameters(address);
        }
    }
}
