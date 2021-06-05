using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameControlPlayerInput
    {
        public IntPtr Address;

        public CGameControlPlayer Base
        {
            get { return new CGameControlPlayer(Address); }
        }

        public CGameControlPlayerInput(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameControlPlayerInput(IntPtr address)
        {
            return new CGameControlPlayerInput(address);
        }
    }
}
