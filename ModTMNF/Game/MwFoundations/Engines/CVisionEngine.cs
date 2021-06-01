using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CVisionEngine
    {
        public IntPtr Address;

        public CMwEngine Base
        {
            get { return new CMwEngine(Address); }
        }

        public CVisionEngine(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CVisionEngine(IntPtr address)
        {
            return new CVisionEngine(address);
        }

        /// <summary>
        /// Copied from CGbxApp inside CGbxApp::InitSystem
        /// </summary>
        public IntPtr WindowHandle
        {
            get { return *(IntPtr*)(Address + OT.CVisionEngine.WindowHandle); }
        }

        /// <summary>
        /// Added to by the call to CVisionEngine::FindOrCreateViewport (which is called by CGbxApp::Start)
        /// 
        /// CVisionViewportDx9
        /// CVisionViewportNull
        /// </summary>
        public CFastBuffer<CVisionViewport> Viewports
        {
            get { return new CFastBuffer<CVisionViewport>(Address + OT.CVisionEngine.Viewports); }
        }
    }
}
