using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameCtnGhost
    {
        public IntPtr Address;

        public CGameGhost Base
        {
            get { return new CGameGhost(Address); }
        }

        public CGameCtnGhost(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameCtnGhost(IntPtr address)
        {
            return new CGameCtnGhost(address);
        }
    }
}
