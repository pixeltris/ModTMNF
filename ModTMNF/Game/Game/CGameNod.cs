using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameNod
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameNod(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameNod(IntPtr address)
        {
            return new CGameNod(address);
        }
    }
}
