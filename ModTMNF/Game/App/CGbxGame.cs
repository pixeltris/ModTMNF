using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGbxGame
    {
        public IntPtr Address;

        public CGbxApp Base
        {
            get { return new CGbxApp(Address); }
        }

        public CGbxGame(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGbxGame(IntPtr address)
        {
            return new CGbxGame(address);
        }
        
        /// <summary>
        /// CGbxApp* CGbxApp::TheApp
        /// </summary>
        public static CGbxGame TheApp
        {
            get { return *(IntPtr*)ST.CGbxApp.TheApp; }
        }
    }
}
