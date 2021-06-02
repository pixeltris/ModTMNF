using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModTMNF.Game;

namespace ModTMNF.Mods
{
    // Info for changing the game name...
    // Nadeo.init - "WindowTitle" will set the window title (fetched in CSystemEngine::InitForGbxGame)
    // CSystemEngine::s_GameWindowTitle - has a few hard coded locations where it's set to "TmForever"
    // CGbxGame::CGbxGame
    //  CSystemEngine::s_GameName = "TmForever" (00D54310)

    // See CGbxApp::WindowedSetWindowTitle / CGameApp::CallbackSetWindowTitle_Set

    class ModCore : Mod
    {
        Hook<FT.CGbxApp.Del_CreateStaticInstance> CreateStaticInstance;

        protected override void OnApply()
        {
            CreateStaticInstance = Hook<FT.CGbxApp.Del_CreateStaticInstance>.Create(FT.CGbxApp.Addresses.CreateStaticInstance, OnCreateStaticInstance);
        }

        private void OnCreateStaticInstance()
        {
            //Program.DebugBreak();
            Program.Log("OnCreateStaticInstance");
            CreateStaticInstance.OriginalFunc();
            CGbxApp app = CGbxApp.TheApp;
            //app.ConsoleEnabled = true;
        }
    }
}
