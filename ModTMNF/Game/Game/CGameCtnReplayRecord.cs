using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameCtnReplayRecord
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameCtnReplayRecord(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameCtnReplayRecord(IntPtr address)
        {
            return new CGameCtnReplayRecord(address);
        }

        public CFastArray<CGameCtnGhost> Ghosts
        {
            get { return new CFastArray<CGameCtnGhost>(Address + OT.CGameCtnReplayRecord.Ghosts); }
        }

        public CGameCtnChallenge Challenge
        {
            get { return *(IntPtr*)(Address + OT.CGameCtnReplayRecord.Challenge); }
        }

        public CFastArray<CSystemFidFile> ReplayFiles
        {
            get { return new CFastArray<CSystemFidFile>(Address + OT.CGameCtnReplayRecord.ReplayFiles); }
        }
    }
}
