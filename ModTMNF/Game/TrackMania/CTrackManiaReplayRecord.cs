using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaReplayRecord
    {
        public IntPtr Address;

        public CGameCtnReplayRecord Base
        {
            get { return new CGameCtnReplayRecord(Address); }
        }

        public CTrackManiaReplayRecord(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaReplayRecord(IntPtr address)
        {
            return new CTrackManiaReplayRecord(address);
        }
    }
}
