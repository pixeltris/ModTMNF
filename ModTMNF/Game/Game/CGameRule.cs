using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameRule
    {
        public IntPtr Address;

        public CGameProcess Base
        {
            get { return new CGameProcess(Address); }
        }

        public CGameRule(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameRule(IntPtr address)
        {
            return new CGameRule(address);
        }
    }
}
