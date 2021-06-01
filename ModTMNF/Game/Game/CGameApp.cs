using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameApp
    {
        public IntPtr Address;

        public CGameProcess Base
        {
            get { return new CGameProcess(Address); }
        }

        public CGameApp(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameApp(IntPtr address)
        {
            return new CGameApp(address);
        }

        /// <summary>
        /// CGameApp* CGameApp::s_TheGame
        /// </summary>
        public static CGameApp TheGame
        {
            get { return *(IntPtr*)ST.CGameApp.s_TheGame; }
        }
    }
}
