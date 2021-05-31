using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CClassicArchive
    {
        public IntPtr Address;

        public CClassicArchive(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CClassicArchive(IntPtr address)
        {
            return new CClassicArchive(address);
        }
    }
}
