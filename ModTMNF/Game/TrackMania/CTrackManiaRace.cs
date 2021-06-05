using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaRace
    {
        public IntPtr Address;

        public CGameRace Base
        {
            get { return new CGameRace(Address); }
        }

        public CTrackManiaRace(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaRace(IntPtr address)
        {
            return new CTrackManiaRace(address);
        }

        public CTrackManiaPlayerInfo GetPlayingPlayerInfo()
        {
            return FT.CTrackManiaRace.GetPlayingPlayerInfo(this);
        }

        public CTrackManiaPlayer GetPlayingPlayer()
        {
            return FT.CTrackManiaRace.GetPlayingPlayer(this);
        }

        public const uint CoundownTime = 2600;
    }
}
