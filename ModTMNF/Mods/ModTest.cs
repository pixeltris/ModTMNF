using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModTMNF.Game;
using System.Runtime.InteropServices;

namespace ModTMNF.Mods
{
    public class ModTest : Mod
    {
        Hook<FT.CHmsZoneDynamic.Del_PhysicsStep2> PhysicsStep2HK;
        Hook<FT.CTrackManiaMenus.Del_MenuMain_Init> MenuMain_InitHK;
        Hook<FT.CTrackManiaMenus.Del_MenuSolo> MenuSoloHK;
        Hook<FT.Globals.Del_WinMainInternal> WinMainInternal;
        CTrackManiaMenus menusPtr;

        protected override void OnApply()
        {
            PhysicsStep2HK = Hook<FT.CHmsZoneDynamic.Del_PhysicsStep2>.Create(FT.CHmsZoneDynamic.Addresses.PhysicsStep2, PhysicsStep2);
            MenuMain_InitHK = Hook<FT.CTrackManiaMenus.Del_MenuMain_Init>.Create(FT.CTrackManiaMenus.Addresses.MenuMain_Init, MenuMain_Init);
            MenuSoloHK = Hook<FT.CTrackManiaMenus.Del_MenuSolo>.Create(FT.CTrackManiaMenus.Addresses.MenuSolo, MenuSolo);
            WinMainInternal = Hook<FT.Globals.Del_WinMainInternal>.Create(FT.Globals.Addresses.WinMainInternal, OnWinMainInternal);
        }

        protected override void OnRemove()
        {
            PhysicsStep2HK.Disable();
            MenuMain_InitHK.Disable();
            WinMainInternal.Disable();
        }

        private int OnWinMainInternal(IntPtr hInstance, int nCmdShow)
        {
            Program.Log("WinMainInternal");
            return WinMainInternal.OriginalFunc(hInstance, nCmdShow);
        }

        private void PhysicsStep2(CHmsZoneDynamic thisPtr)
        {
            // Making physics changes here will break validation of a run in the regular client, but it can validate itself (as validation calls this func).
            PhysicsStep2HK.OriginalFunc(thisPtr);
            PhysicsStep2HK.OriginalFunc(thisPtr);
            //PhysicsStep2HK.OriginalFunc(thisPtr);

            //CMwCmdBufferCore.TheCoreCmdBuffer.StopSimulation();
            //CMwCmdBufferCore.TheCoreCmdBuffer.SetSimulationRelativeSpeed(3.0f);
        }

        /// <summary>
        /// Called when clicking to play solo
        /// </summary>
        private void MenuSolo(CTrackManiaMenus thisPtr)
        {
            Program.Log("MenuSolo");
            MenuSoloHK.OriginalFunc(thisPtr);
        }

        /// <summary>
        /// Called when the main menu screen gets focus (which happens at various points)
        /// </summary>
        private void MenuMain_Init(CTrackManiaMenus thisPtr)
        {
            menusPtr = thisPtr;

            Program.Log(StackWalk64.GetCallstack());

            // Example of reflection to get a param value (slow, at least cache the param if doing this often).
            CMwClassInfo classInfo = ((CMwNod)thisPtr.Address).MwGetClassInfo();
            SMwParamInfo param = classInfo.GetMwParamFromName("TimeLimit", 0);
            int timeLimitReflected = ((CMwNod)thisPtr.Address).Param_GetInt(param);
            int timeLimitMem = Marshal.ReadInt32(thisPtr.Address, OT.CTrackManiaMenus.TimeLimit);
            Program.Log("MenuMain_Init TimeLimit(reflection):" + timeLimitReflected + " TimeLimit(memory):" + timeLimitMem);

            // Used to generate runtime docs when needed. TODO: Move this somewhere more permanent and make toggleable.
            //Analysis.SymbolsHelper.GenerateRuntimeDocs();
        }
    }
}
