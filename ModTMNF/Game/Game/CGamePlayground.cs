using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGamePlayground
    {
        public IntPtr Address;

        public CGameNod Base
        {
            get { return new CGameNod(Address); }
        }

        public CGamePlayground(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGamePlayground(IntPtr address)
        {
            return new CGamePlayground(address);
        }
    }
}
