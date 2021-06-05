using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModTMNF.Game;
using System.Runtime.InteropServices;
using ModTMNF.Mods.UI;
using System.IO;

namespace ModTMNF.Mods
{
    public class ModTest : Mod
    {
        Hook<FT.CHmsZoneDynamic.Del_PhysicsStep2> PhysicsStep2HK;
        Hook<FT.CTrackManiaMenus.Del_MenuMain_Init> MenuMain_InitHK;
        Hook<FT.CTrackManiaMenus.Del_MenuSolo> MenuSoloHK;
        Hook<FT.Globals.Del_WinMainInternal> WinMainInternal;
        CTrackManiaMenus menusPtr;

        Hook<FT.CMwTimer.Del_SimulateDeltaTime> SimulateDeltaTimeHK;
        Hook<FT.CMwTimer.Del_Tick> TickHK;
        Hook<FT.CMwCmdBufferCore.Del_Run> RunHK;
        Hook<FT.CTrackManiaRace.Del_InputRace> InputRaceHK;
        Hook<FT.CTrackManiaControlPlayerInput.Del_UpdateVehicleStateFromInputsImpl> UpdateVehicleStateFromInputsImplHK;
        StatsForm form;
        GbxReplayFile replayFile;

        protected override void OnApply()
        {
            PhysicsStep2HK = Hook<FT.CHmsZoneDynamic.Del_PhysicsStep2>.Create(FT.CHmsZoneDynamic.Addresses.PhysicsStep2, PhysicsStep2);
            MenuMain_InitHK = Hook<FT.CTrackManiaMenus.Del_MenuMain_Init>.Create(FT.CTrackManiaMenus.Addresses.MenuMain_Init, MenuMain_Init);
            MenuSoloHK = Hook<FT.CTrackManiaMenus.Del_MenuSolo>.Create(FT.CTrackManiaMenus.Addresses.MenuSolo, MenuSolo);
            WinMainInternal = Hook<FT.Globals.Del_WinMainInternal>.Create(FT.Globals.Addresses.WinMainInternal, OnWinMainInternal);

            RunHK = Hook<FT.CMwCmdBufferCore.Del_Run>.Create(FT.CMwCmdBufferCore.Addresses.Run, Run);
            SimulateDeltaTimeHK = Hook<FT.CMwTimer.Del_SimulateDeltaTime>.Create(FT.CMwTimer.Addresses.SimulateDeltaTime, SimulateDeltaTime);
            TickHK = Hook<FT.CMwTimer.Del_Tick>.Create(FT.CMwTimer.Addresses.Tick, Tick);
            InputRaceHK = Hook<FT.CTrackManiaRace.Del_InputRace>.Create(FT.CTrackManiaRace.Addresses.InputRace, InputRace);
            UpdateVehicleStateFromInputsImplHK = Hook<FT.CTrackManiaControlPlayerInput.Del_UpdateVehicleStateFromInputsImpl>.Create(FT.CTrackManiaControlPlayerInput.Addresses.UpdateVehicleStateFromInputsImpl, UpdateVehicleStateFromInputsImpl);

            form = new StatsForm();
            form.Show();
        }

        protected override void OnRemove()
        {
            PhysicsStep2HK.Disable();
            MenuMain_InitHK.Disable();
            WinMainInternal.Disable();
        }

        private void UpdateVehicleStateFromInputsImpl(ref CTrackManiaControlPlayerInput.SRaceInputs inputs, CSceneMobil mobil)
        {
            if (!form.DriveReplayFile)
            {
                UpdateVehicleStateFromInputsImplHK.OriginalFunc(ref inputs, mobil);
                return;
            }

            uint time = CMwCmdBufferCore.TheCoreCmdBuffer.ActiveTimerAdapter.GetTickTime();
            if (time == 0)
            {
                if (form.StartAtTime > 0)
                {
                    CMwCmdBufferCore.TheCoreCmdBuffer.ActiveTimerAdapter.SetRelativeSpeed(100000);
                }
                form.UpdateFetchReplayFile();
                if (replayFile == null || replayFile.FilePath != form.ReplayFilePath)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(form.ReplayFilePath) || !File.Exists(form.ReplayFilePath))
                        {
                            return;
                        }
                        replayFile = GbxReplayFile.Load(form.ReplayFilePath);
                    }
                    catch (Exception e)
                    {
                        Program.Log(e.ToString());
                    }
                }
                if (replayFile != null)
                {
                    replayFile.ActiveInputEventIndex = 0;
                    for (int i = 0; i < replayFile.ActiveInputEvents.Length; i++)
                    {
                        replayFile.ActiveInputEvents[i] = null;
                    }
                }
            }
            if (replayFile != null)
            {
                if (time >= CTrackManiaRace.CoundownTime)
                {
                    uint actualRaceTime = time - CTrackManiaRace.CoundownTime;
                    if (form.StartAtTime > 0)
                    {
                        if (time >= form.StartAtTime)
                        {
                            CMwCmdBufferCore.TheCoreCmdBuffer.ActiveTimerAdapter.SetRelativeSpeed(1);
                            if (form.StartAtTimeRegularInput)
                            {
                                UpdateVehicleStateFromInputsImplHK.OriginalFunc(ref inputs, mobil);
                                return;
                            }
                        }
                    }

                    inputs.Steer.FloatValue = 0;
                    inputs.Accelerate.Value = 0;
                    inputs.SteerLeft.Value = 0;
                    inputs.SteerRight.Value = 0;
                    inputs.Brake.Value = 0;
                    inputs.Gas.FloatValue = 0;

                    for (int i = replayFile.ActiveInputEventIndex; i < replayFile.InputEvents.Count; i++)
                    {
                        if (replayFile.InputEvents[i].Time > actualRaceTime)
                        {
                            break;
                        }
                        replayFile.ActiveInputEvents[(int)replayFile.InputEvents[i].Type] = replayFile.InputEvents[i];
                        replayFile.ActiveInputEventIndex = i;
                    }

                    inputs.Steer.FloatValue = replayFile.GetValueF(GbxReplayFile.InputType.Steer);
                    inputs.Accelerate.Value = replayFile.GetValue(GbxReplayFile.InputType.Accelerate);
                    inputs.SteerLeft.Value = replayFile.GetValue(GbxReplayFile.InputType.SteerLeft);
                    inputs.SteerRight.Value = replayFile.GetValue(GbxReplayFile.InputType.SteerRight);
                    inputs.Brake.Value = replayFile.GetValue(GbxReplayFile.InputType.Brake);
                    inputs.Gas.FloatValue = replayFile.GetValueF(GbxReplayFile.InputType.Gas);

                    // TODO: Register input events with the store, and handle respawns

                    /*CTrackManiaPlayerInfo playerInfo = CTrackMania.TheGame.Race.GetPlayingPlayerInfo();
                    SInputEvent respawnIE = new SInputEvent(SInputActionDesc.ActionRespawn_1, 1);
                    playerInfo.AddInputEvent(respawnIE, CMwCmdBufferCore.TheCoreCmdBuffer.ActiveTimerAdapter.GetTickTime());*/
                }
            }

            UpdateVehicleStateFromInputsImplHK.OriginalFunc(ref inputs, mobil);
        }

        private void InputRace(CTrackManiaRace thisPtr)
        {
            CMwTimerAdapter timerAdapter = CMwCmdBufferCore.TheCoreCmdBuffer.ActiveTimerAdapter;
            uint tickTime = timerAdapter.GetTickTime();
            uint deltaTime = timerAdapter.DeltaTime;
            InputRaceHK.OriginalFunc(thisPtr);
        }

        private void Run(CMwCmdBufferCore thisPtr)
        {
            if (!form.PauseCoreRun || form.ShouldStepCore)
            {
                form.ShouldStepCore = false;
                RunHK.OriginalFunc(thisPtr);
            }
        }

        private void SimulateDeltaTime(CMwTimer thisPtr, uint time)
        {
            form.NumSimulateDeltaTime++;
            form.UpdateTimerInfo();
            SimulateDeltaTimeHK.OriginalFunc(thisPtr, time);
        }

        private void Tick(CMwTimer thisPtr)
        {
            form.NumTick++;
            form.UpdateTimerInfo();
            TickHK.OriginalFunc(thisPtr);
        }

        private int OnWinMainInternal(IntPtr hInstance, int nCmdShow)
        {
            Program.Log("WinMainInternal");
            return WinMainInternal.OriginalFunc(hInstance, nCmdShow);
        }

        private void PhysicsStep2(CHmsZoneDynamic thisPtr)
        {
            // Making physics changes here will break validation of a run in the regular client, but it can validate itself (as validation calls this func).
            if (!form.PausePhysics)
            {
                for (int i = 0; i < form.PhysicsStep; i++)
                {
                    PhysicsStep2HK.OriginalFunc(thisPtr);
                }
            }
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

            //Program.Log(StackWalk64.GetCallstack());

            // Useful globals:
            //CGameApp.TheGame
            //CGbxApp.TheApp <--- has InputPort,SystemConfig,SystemWindow,GameApp,etc
            //CMwCmdBufferCore.TheCoreCmdBuffer <--- handles invokers for the game loop and manages game time
            //CMwEngineManager.Instance
            //CTrackMania.TheGame.Race <--- CTrackManiaRace
            //CTrackMania.TheGame.Race.GetPlayingPlayerInfo() <--- CTrackManiaPlayer
            //CTrackMania.TheGame.Race.GetPlayingPlayer() <--- CTrackManiaPlayerInfo

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
