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

        public CSystemConfig SystemConfig
        {
            get { return *(IntPtr*)(OT.CGameApp.SystemConfig); }
        }

        public void ShowMainMenu()
        {
            FT.CGameApp.ShowMainMenu(this);
        }

        public void HideMainMenu()
        {
            FT.CGameApp.HideMainMenu(this);
        }

        public void ShowMenu()
        {
            FT.CGameApp.ShowMenu(this);
        }

        public void HideMenu()
        {
            FT.CGameApp.HideMenu(this);
        }

        public void EnablePick()
        {
            FT.CGameApp.EnablePick(this);
        }

        public void DisablePick()
        {
            FT.CGameApp.DisablePick(this);
        }

        public int AvatarGetCount()
        {
            return FT.CGameApp.AvatarGetCount(this);
        }

        public void Exit()
        {
            FT.CGameApp.Exit(this);
        }

        public void ToggleFullScreen()
        {
            FT.CGameApp.ToggleFullScreen(this);
        }

        public void SetCursorPos(GmVec2 pos)
        {
            FT.CGameApp.SetCursorPos(this, ref pos);
        }

        public CHmsPicker GetPicker()
        {
            return FT.CGameApp.GetPicker(this);
        }

        public GmVec2 GetMousePos()
        {
            GmVec2* result = FT.CGameApp.GetMousePos(this);
            return *result;
        }

        public void SetCursorPosX(float posX)
        {
            FT.CGameApp.SetCursorPosX(this, posX);
        }

        public void SetCursorPosY(float posX)
        {
            FT.CGameApp.SetCursorPosY(this, posX);
        }

        public void StopMusics()
        {
            FT.CGameApp.StopMusics(this);
        }

        public bool IsPayingInstall()
        {
            return FT.CGameApp.IsPayingInstall(this);
        }

        public EAccountType GetPayingAccountType()
        {
            return FT.CGameApp.GetPayingAccountType(this);
        }

        public bool IsPayingSolo()
        {
            return FT.CGameApp.IsPayingSolo(this);
        }

        public void SaveSystemConfig()
        {
            FT.CGameApp.SaveSystemConfig(this);
        }

        public CGameMenu GetCurrentMenu()
        {
            return FT.CGameApp.GetCurrentMenu(this);
        }

        public void MenusClear()
        {
            FT.CGameApp.MenusClear(this);
        }

        public CGameDialogs GetBasicDialogs()
        {
            return FT.CGameApp.GetBasicDialogs(this);
        }

        public CGameSystemOverlay GetSystemOverlay()
        {
            return FT.CGameApp.GetSystemOverlay(this);
        }

        public CGameManialinkBrowser GetManialinkBrowser()
        {
            return FT.CGameApp.GetManialinkBrowser(this);
        }

        public bool IsPickEnabled()
        {
            return FT.CGameApp.IsPickEnabled(this);
        }

        public CAudioSound GetSound(EInterfaceSound sound)
        {
            return FT.CGameApp.GetSound(this, sound);
        }

        public void PlaySound(EInterfaceSound sound, int unk1 = -1)
        {
            FT.CGameApp.PlaySound(this, sound, unk1);
        }

        public CPlugMusic GetNextMusic(EInterfaceMusic music)
        {
            return FT.CGameApp.GetNextMusic(this, music);
        }

        public void PlayCustomMusic(CPlugMusic music)
        {
            FT.CGameApp.PlayCustomMusic(this, music);
        }

        public void PlayMusic(EInterfaceMusic music, int unk1 = -1)
        {
            FT.CGameApp.PlayMusic(this, music, unk1);
        }

        public void UpdateMusic()
        {
            FT.CGameApp.UpdateMusic(this);
        }

        public bool Profile_IsChatEnabled()
        {
            return FT.CGameApp.Profile_IsChatEnabled(this);
        }

        public bool Profile_IsAvatarsEnabled()
        {
            return FT.CGameApp.Profile_IsAvatarsEnabled(this);
        }

        public void UnregisterPlayer(CGamePlayer player)
        {
            FT.CGameApp.UnregisterPlayer(this, player);
        }

        public bool CanUseMessenger()
        {
            return VT.Get<VT.CGameApp>(Address).CanUseMessenger(this);
        }

        public bool CanUseMoney()
        {
            return VT.Get<VT.CGameApp>(Address).CanUseMoney(this);
        }

        public CGameCtnChallenge GetChallenge()
        {
            return VT.Get<VT.CGameApp>(Address).GetChallenge(this);
        }

        public void QuitGame()
        {
            VT.Get<VT.CGameApp>(Address).QuitGame(this);
        }

        public CGameNetwork GetNetwork()
        {
            return VT.Get<VT.CGameApp>(Address).GetNetwork(this);
        }

        public CGameInterface GetInterface()
        {
            return VT.Get<VT.CGameApp>(Address).GetInterface(this);
        }

        public bool CanUseManialinkAnywhere()
        {
            return VT.Get<VT.CGameApp>(Address).CanUseManialinkAnywhere(this);
        }

        public void ResetGame(bool resetTime)
        {
            VT.Get<VT.CGameApp>(Address).ResetGame(this, resetTime);
        }

        public void ApplySystemConfig(CSystemConfig systemConfig)
        {
            VT.Get<VT.CGameApp>(Address).ApplySystemConfig(this, systemConfig);
        }

        public int RegisterPlayer(CGamePlayer player)
        {
            return VT.Get<VT.CGameApp>(Address).RegisterPlayer(this, player);
        }

        public void UnregisterPlayers()
        {
            VT.Get<VT.CGameApp>(Address).UnregisterPlayers(this);
        }
    }
}
