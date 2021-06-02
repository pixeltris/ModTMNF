using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGamePlayer
    {
        public IntPtr Address;

        public CGameNod Base
        {
            get { return new CGameNod(Address); }
        }

        public CGamePlayer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGamePlayer(IntPtr address)
        {
            return new CGamePlayer(address);
        }
    }
}
