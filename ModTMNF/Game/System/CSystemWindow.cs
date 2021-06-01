using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSystemWindow
    {
        public IntPtr Address;

        public CNodSystem Base
        {
            get { return new CNodSystem(Address); }
        }

        public CSystemWindow(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemWindow(IntPtr address)
        {
            return new CSystemWindow(address);
        }

        public static int ScreenSizeX
        {
            get { return *(int*)ST.CSystemWindow.s_ScreenSizeX; }
        }

        public static int ScreenSizeY
        {
            get { return *(int*)ST.CSystemWindow.s_ScreenSizeY; }
        }

        /// <summary>
        /// Assigned by CSystemWindow::SetWindow() which is invoked by CGbxApp::InitSystem()
        /// </summary>
        public IntPtr WindowHandle
        {
            get { return *(IntPtr*)(Address + OT.CSystemWindow.WindowHandle); }
        }

        public CSystemWindow SubWindow
        {
            get { return *(IntPtr*)(Address + OT.CSystemWindow.SubWindow); }
        }

        public int SizeX
        {
            get { return *(int*)(Address + OT.CSystemWindow.SizeX); }
        }

        public int SizeY
        {
            get { return *(int*)(Address + OT.CSystemWindow.SizeY); }
        }
    }
}
