using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameRace
    {
        public IntPtr Address;

        public CGamePlayground Base
        {
            get { return new CGamePlayground(Address); }
        }

        public CGameRace(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameRace(IntPtr address)
        {
            return new CGameRace(address);
        }
    }
}
