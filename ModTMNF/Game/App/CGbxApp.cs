using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // c:\\codebase\\nadeo\\tools\\gbxwin32app\\source\\gbxapp.cpp
    // c:\\codebase\\nadeo\\tools\\gbxwin32app\\source\\win32main.cpp
    public unsafe struct CGbxApp
    {
        public IntPtr Address;

        /// <summary>
        /// Inherits from CMwNod, but isn't registered in the reflection system?
        /// </summary>
        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGbxApp(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGbxApp(IntPtr address)
        {
            return new CGbxApp(address);
        }

        /// <summary>
        /// CGbxApp* CGbxApp::TheApp
        /// 
        /// CGbxApp::CreateStaticInstance() creates this (called at the start of WinMainInternal)
        /// </summary>
        public static CGbxApp TheApp
        {
            get { return *(IntPtr*)ST.CGbxApp.TheApp; }
        }

        /// <summary>
        /// Holds the window handle
        /// </summary>
        public CSystemWindow SystemWindow
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.SystemWindow); }
        }

        /// <summary>
        /// Holds the window size
        /// </summary>
        public CSystemWindow SystemWindow2
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.SystemWindow2); }
        }

        public CSystemConfig SystemConfig
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.SystemConfig); }
        }

        /// <summary>
        /// Initialized by:
        /// CGbxApp::Start
        ///  - CInputEngine::GetOrCreateInputPort
        /// </summary>
        public CInputPort InputPort
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.InputPort); }
        }

        /// <summary>
        /// Initialized by:
        /// CGbxApp::Start
        ///  - CVisionEngine::FindOrCreateViewport
        /// </summary>
        public CHmsViewport Viewport
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.Viewport); }
        }

        public IntPtr WindowHandle
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.WindowHandle); }
        }

        /// <summary>
        /// Focus the existing TmForever window when launching (if no existing window exists, launch as normal).
        /// ... doesn't seem to work?
        /// </summary>
        public bool WindowFocusIfExisting
        {
            get { return *(bool*)(Address + OT.CGbxApp.WindowFocusIfExisting); }
            set { *(bool*)(Address + OT.CGbxApp.WindowFocusIfExisting) = value; }
        }

        /// <summary>
        /// Copied from CSystemEngine::s_GameWindowTitle
        /// </summary>
        public string WindowTitle
        {
            get { return ((CFastStringInt*)(Address + OT.CGbxApp.WindowTitle))->Value; }
        }

        /// <summary>
        /// "Software\\Nadeo\\TmForever"
        /// </summary>
        public string RegistryKeyName
        {
            get { return ((CFastString*)(Address + OT.CGbxApp.RegistryKeyName))->Value; }
        }

        public bool ConsoleEnabled
        {
            get { return *(bool*)(Address + OT.CGbxApp.ConsoleEnabled); }
            set { *(bool*)(Address + OT.CGbxApp.ConsoleEnabled) = value; }
        }
    }
}
