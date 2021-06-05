using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameGhost
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameGhost(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameGhost(IntPtr address)
        {
            return new CGameGhost(address);
        }
    }
}
