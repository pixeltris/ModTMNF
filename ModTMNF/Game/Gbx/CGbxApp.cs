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

        public CGameApp GameApp
        {
            get { return *(IntPtr*)(Address + OT.CGbxApp.GameApp); }
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
            get { return *(BOOL*)(Address + OT.CGbxApp.WindowFocusIfExisting); }
            set { *(BOOL*)(Address + OT.CGbxApp.WindowFocusIfExisting) = value; }
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
            get { return *(BOOL*)(Address + OT.CGbxApp.ConsoleEnabled); }
            set { *(BOOL*)(Address + OT.CGbxApp.ConsoleEnabled) = value; }
        }

        public void TerminateApp()
        {
            FT.CGbxApp.TerminateApp(this);
        }

        public bool IsFullScreen()
        {
            return FT.CGbxApp.IsFullScreen(this);
        }

        public void InitSystem()
        {
            FT.CGbxApp.InitSystem(this);
        }

        public void Start()
        {
            FT.CGbxApp.Start(this);
        }

        public void MainLoop()
        {
            FT.CGbxApp.MainLoop(this);
        }

        public void GoFullScreen()
        {
            FT.CGbxApp.GoFullScreen(this);
        }

        public void GoWindowed()
        {
            FT.CGbxApp.GoWindowed(this);
        }

        public void LogAppInfo()
        {
            FT.CGbxApp.LogAppInfo(this);
        }

        public void Destroy()
        {
            VT.Get<VT.CGbxApp>(Address).Destroy(this);
        }

        public void Init()
        {
            VT.Get<VT.CGbxApp>(Address).Init(this);
        }

        public void StartApp()
        {
            VT.Get<VT.CGbxApp>(Address).StartApp(this);
        }

        public void StopApp()
        {
            VT.Get<VT.CGbxApp>(Address).StopApp(this);
        }

        public bool IsForceWindowed()
        {
            return VT.Get<VT.CGbxApp>(Address).IsForceWindowed(this);
        }

        public void RenderWaitingFrame()
        {
            VT.Get<VT.CGbxApp>(Address).RenderWaitingFrame(this);
        }

        public void EngineInitEnd()
        {
            VT.Get<VT.CGbxApp>(Address).EngineInitEnd(this);
        }

        public int SetCmdLineUrl(string value)
        {
            CFastString str = new CFastString(value);
            int ret = VT.Get<VT.CGbxApp>(Address).SetCmdLineUrl(this, ref str);
            str.Delete();
            return ret;
        }

        public int SetCmdLineFile(string value)
        {
            CFastStringInt str = new CFastStringInt(value);
            int ret = VT.Get<VT.CGbxApp>(Address).SetCmdLineFile(this, ref str);
            str.Delete();
            return ret;
        }

        public int ApplyCommandLineArgs()
        {
            return VT.Get<VT.CGbxApp>(Address).ApplyCommandLineArgs(this);
        }
    }
}
