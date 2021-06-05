using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaPlayer
    {
        public IntPtr Address;

        public CGamePlayer Base
        {
            get { return new CGamePlayer(Address); }
        }

        public CTrackManiaPlayer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaPlayer(IntPtr address)
        {
            return new CTrackManiaPlayer(address);
        }
    }
}
