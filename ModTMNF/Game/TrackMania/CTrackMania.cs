using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackMania
    {
        public IntPtr Address;

        public CGameApp Base
        {
            get { return new CGameApp(Address); }
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
    }
}
