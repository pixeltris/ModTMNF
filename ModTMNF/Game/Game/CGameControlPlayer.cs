using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameControlPlayer
    {
        public IntPtr Address;

        public CGameRule Base
        {
            get { return new CGameRule(Address); }
        }

        public CGameControlPlayer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameControlPlayer(IntPtr address)
        {
            return new CGameControlPlayer(address);
        }
    }
}
