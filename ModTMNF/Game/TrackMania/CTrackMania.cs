using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackMania
    {
        public IntPtr Address;

        public CGameCtnApp Base
        {
            get { return new CGameCtnApp(Address); }
        }

        public CTrackMania(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackMania(IntPtr address)
        {
            return new CTrackMania(address);
        }

        /// <summary>
        /// CGameApp* CGameApp::s_TheGame
        /// </summary>
        public static CTrackMania TheGame
        {
            get { return *(IntPtr*)ST.CGameApp.s_TheGame; }
        }

        /// <summary>
        /// CTrackMania::SetRace
        /// </summary>
        public CTrackManiaRace Race
        {
            //1108
            get { return *(IntPtr*)(Address + OT.CTrackMania.Race); }
        }

        /// <summary>
        /// Provides access to the currently active replay (including in the "Launch" game menu).
        /// 
        /// Changed as soon as you press "Launch" in the game menu. Active whilst againt playing the replay.
        /// Set back to nullptr when going back to the main menu and when closing the "Launch" menu.
        /// </summary>
        public CGameCtnReplayRecord ReplayRecord
        {
            get { return Base.ReplayRecord.Address; }
        }
    }
}
