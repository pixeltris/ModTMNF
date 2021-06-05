using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using ModTMNF.Game;

#pragma warning disable 649 // Field is never assigned (because of all the runtime assigned delegates)
#pragma warning disable 108 // Use the new keyword if hiding was intended (for the Offset classes in VT)

namespace ModTMNF
{
    // Things flagged as SHARED_ADDRESS use a shared address and aren't safe to hook without impacting other functions.

    /// <summary>
    /// Function tables
    /// </summary>
    unsafe static class FT
    {
        // NOTE: Rename overloaded functions! This code doesn't compare signatures when matching functions.
        public static void Init()
        {
            foreach (Type type in typeof(FT).GetNestedTypes())
            {
                Dictionary<string, IntPtr> addressTable = new Dictionary<string, IntPtr>();
                HashSet<Type> delegateTypes = new HashSet<Type>();
                foreach (Type subType in type.GetNestedTypes())
                {
                    if (typeof(Delegate).IsAssignableFrom(subType))
                    {
                        delegateTypes.Add(subType);
                    }
                    if (subType.Name == "Addresses")
                    {
                        foreach (System.Reflection.FieldInfo field in subType.GetFields())
                        {
                            if (field.IsStatic)
                            {
                                IntPtr address = (IntPtr)field.GetValue(null);
                                if (address != IntPtr.Zero)
                                {
                                    addressTable[field.Name] = address;
                                }
                            }
                        }
                    }
                }
                foreach (System.Reflection.FieldInfo field in type.GetFields())
                {
                    IntPtr address;
                    if (delegateTypes.Contains(field.FieldType) && addressTable.TryGetValue(field.Name, out address))
                    {
                        field.SetValue(null, Marshal.GetDelegateForFunctionPointer(address, field.FieldType));
                    }
                }
            }
        }

        public static class Globals
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate int Del_WinMain(IntPtr hInstance, IntPtr hPrevInstance, [MarshalAs(UnmanagedType.LPStr)] string lpCmdLine, int nCmdShow);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int Del_WinMainInternal(IntPtr hInstance, int nCmdShow);

            public static Del_WinMain WinMain;
            public static Del_WinMainInternal WinMainInternal;

            public static class Addresses
            {
                public static IntPtr WinMain = (IntPtr)0x0052A310;
                public static IntPtr WinMainInternal = (IntPtr)0x00529C00;
            }
        }

        public static class CGbxApp
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_CreateStaticInstance();
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GetWindowSizeFromClientSize(Game.CGbxApp thisPtr, ref GmNat2 unk1, ref GmNat2 unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_WindowedSetClientSize(Game.CGbxApp thisPtr, ref GmNat2 size);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_TerminateApp(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_WindowedSetWindowTitle(Game.CGbxApp thisPtr, ref Game.CFastStringInt title);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsFullScreen(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_InitSystem(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Start(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MainLoop(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GoFullScreen(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GoWindowed(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_LogAppInfo(Game.CGbxApp thisPtr);

            public static Del_CreateStaticInstance CreateStaticInstance;
            public static Del_GetWindowSizeFromClientSize GetWindowSizeFromClientSize;
            public static Del_WindowedSetClientSize WindowedSetClientSize;
            public static Del_TerminateApp TerminateApp;
            public static Del_WindowedSetWindowTitle WindowedSetWindowTitle;
            public static Del_IsFullScreen IsFullScreen;
            public static Del_InitSystem InitSystem;
            public static Del_Start Start;
            public static Del_MainLoop MainLoop;
            public static Del_GoFullScreen GoFullScreen;
            public static Del_GoWindowed GoWindowed;
            public static Del_LogAppInfo LogAppInfo;

            public static class Addresses
            {
                public static IntPtr CreateStaticInstance = (IntPtr)0x004029C0;
                public static IntPtr GetWindowSizeFromClientSize = (IntPtr)0x00528BB0;
                public static IntPtr WindowedSetClientSize = (IntPtr)0x00528C00;
                public static IntPtr TerminateApp = (IntPtr)0x00528C80;
                public static IntPtr WindowedSetWindowTitle = (IntPtr)0x0052A3F0;
                public static IntPtr IsFullScreen = (IntPtr)0x0052A4B0;
                public static IntPtr InitSystem = (IntPtr)0x0052A670;
                public static IntPtr Start = (IntPtr)0x0052A7B0;
                public static IntPtr MainLoop = (IntPtr)0x0052A8B0;
                public static IntPtr GoFullScreen = (IntPtr)0x0052A9A0;
                public static IntPtr GoWindowed = (IntPtr)0x0052A9E0;
                public static IntPtr LogAppInfo = (IntPtr)0x0052AD20;
            }
        }

        public static class CGbxGame
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_CheckNetwork(Game.CGbxGame thisPtr);

            public static Del_CheckNetwork CheckNetwork;

            public static class Addresses
            {
                public static IntPtr CheckNetwork = (IntPtr)0x00401E80;
            }
        }

        public static class CGameApp
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ShowMainMenu(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HideMainMenu(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ShowMenu(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HideMenu(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_EnablePick(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_DisablePick(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_AvatarGetCount(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Exit(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ToggleFullScreen(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetCursorPos(Game.CGameApp thisPtr, ref GmVec2 pos);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CHmsPicker Del_GetPicker(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate GmVec2* Del_GetMousePos(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetCursorPosX(Game.CGameApp thisPtr, float posX);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetCursorPosY(Game.CGameApp thisPtr, float posY);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StopMusics(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsPayingInstall(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate EAccountType Del_GetPayingAccountType(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsPayingSolo(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SaveSystemConfig(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameMenu Del_GetCurrentMenu(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MenusClear(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameDialogs Del_GetBasicDialogs(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameSystemOverlay Del_GetSystemOverlay(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameManialinkBrowser Del_GetManialinkBrowser(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsPickEnabled(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CSceneMobil Del_GetPickedMobil(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AvatarGetFromIndex(Game.CGameApp thisPtr, int index, ref CMwNodRef result);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AvatarGetRand(Game.CGameApp thisPtr, ref CMwNodRef result);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AvatarGetRandName(Game.CGameApp thisPtr, ref Game.CFastStringInt result);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_AvatarIndexFromName(Game.CGameApp thisPtr, ref Game.CFastStringInt name);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate CAudioSound Del_GetSound(Game.CGameApp thisPtr, EInterfaceSound sound);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_PlaySound(Game.CGameApp thisPtr, EInterfaceSound sound, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CPlugMusic Del_GetNextMusic(Game.CGameApp thisPtr, EInterfaceMusic music);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_PlayCustomMusic(Game.CGameApp thisPtr, Game.CPlugMusic music);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_PlayMusic(Game.CGameApp thisPtr, EInterfaceMusic music, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_UpdateMusic(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_Profile_IsChatEnabled(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_Profile_IsAvatarsEnabled(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_UnregisterPlayer(Game.CGameApp thisPtr, Game.CGamePlayer player);

            public static Del_ShowMainMenu ShowMainMenu;
            public static Del_HideMainMenu HideMainMenu;
            public static Del_ShowMenu ShowMenu;
            public static Del_HideMenu HideMenu;
            public static Del_EnablePick EnablePick;
            public static Del_DisablePick DisablePick;
            public static Del_AvatarGetCount AvatarGetCount;
            public static Del_Exit Exit;
            public static Del_ToggleFullScreen ToggleFullScreen;
            public static Del_SetCursorPos SetCursorPos;
            public static Del_GetPicker GetPicker;
            public static Del_GetMousePos GetMousePos;
            public static Del_SetCursorPosX SetCursorPosX;
            public static Del_SetCursorPosY SetCursorPosY;
            public static Del_StopMusics StopMusics;
            public static Del_IsPayingInstall IsPayingInstall;
            public static Del_GetPayingAccountType GetPayingAccountType;
            public static Del_IsPayingSolo IsPayingSolo;
            public static Del_SaveSystemConfig SaveSystemConfig;
            public static Del_GetCurrentMenu GetCurrentMenu;
            public static Del_MenusClear MenusClear;
            public static Del_GetBasicDialogs GetBasicDialogs;
            public static Del_GetSystemOverlay GetSystemOverlay;
            public static Del_GetManialinkBrowser GetManialinkBrowser;
            public static Del_IsPickEnabled IsPickEnabled;
            public static Del_GetPickedMobil GetPickedMobil;
            public static Del_AvatarGetFromIndex AvatarGetFromIndex;
            public static Del_AvatarGetRand AvatarGetRand;
            public static Del_AvatarGetRandName AvatarGetRandName;
            public static Del_AvatarIndexFromName AvatarIndexFromName;
            public static Del_GetSound GetSound;
            public static Del_PlaySound PlaySound;
            public static Del_GetNextMusic GetNextMusic;
            public static Del_PlayCustomMusic PlayCustomMusic;
            public static Del_PlayMusic PlayMusic;
            public static Del_UpdateMusic UpdateMusic;
            public static Del_Profile_IsChatEnabled Profile_IsChatEnabled;
            public static Del_Profile_IsAvatarsEnabled Profile_IsAvatarsEnabled;
            public static Del_UnregisterPlayer UnregisterPlayer;

            public static class Addresses
            {
                public static IntPtr ShowMainMenu = (IntPtr)0x0059EFC0;
                public static IntPtr HideMainMenu = (IntPtr)0x0059EFE0;
                public static IntPtr ShowMenu = (IntPtr)0x0059D110;
                public static IntPtr HideMenu = (IntPtr)0x0059D160;
                public static IntPtr EnablePick = (IntPtr)0x0059D4C0;
                public static IntPtr DisablePick = (IntPtr)0x0059D500;
                public static IntPtr AvatarGetCount = (IntPtr)0x00676270;// SHARED_ADDRESS
                public static IntPtr Exit = (IntPtr)0x0059B060;
                public static IntPtr ToggleFullScreen = (IntPtr)0x0059B0C0;
                public static IntPtr SetCursorPos = (IntPtr)0x0059B140;
                public static IntPtr GetPicker = (IntPtr)0x0059B210;
                public static IntPtr GetMousePos = (IntPtr)0x0059B220;
                public static IntPtr SetCursorPosX = (IntPtr)0x0059B230;
                public static IntPtr SetCursorPosY = (IntPtr)0x0059B260;
                public static IntPtr StopMusics = (IntPtr)0x0059B300;
                public static IntPtr IsPayingInstall = (IntPtr)0x0059B340;
                public static IntPtr GetPayingAccountType = (IntPtr)0x0059B350;
                public static IntPtr IsPayingSolo = (IntPtr)0x0059B390;
                public static IntPtr SaveSystemConfig = (IntPtr)0x0059B4B0;
                public static IntPtr GetCurrentMenu = (IntPtr)0x0059B7F0;
                public static IntPtr MenusClear = (IntPtr)0x0059B820;
                public static IntPtr GetBasicDialogs = (IntPtr)0x0059BCA0;
                public static IntPtr GetSystemOverlay = (IntPtr)0x0059BCB0;
                public static IntPtr GetManialinkBrowser = (IntPtr)0x0059BCC0;
                public static IntPtr IsPickEnabled = (IntPtr)0x0059BCD0;
                public static IntPtr GetPickedMobil = (IntPtr)0x0059BCE0;
                public static IntPtr AvatarGetFromIndex = (IntPtr)0x0059BD00;
                public static IntPtr AvatarGetRand = (IntPtr)0x0059BD70;
                public static IntPtr AvatarGetRandName = (IntPtr)0x0059BDC0;
                public static IntPtr AvatarIndexFromName = (IntPtr)0x0059D540;
                public static IntPtr RemoteControlInit = (IntPtr)0x0059BEA0;
                public static IntPtr GetSound = (IntPtr)0x0059C0B0;
                public static IntPtr PlaySound = (IntPtr)0x0059C130;
                public static IntPtr GetNextMusic = (IntPtr)0x0059C170;
                public static IntPtr PlayCustomMusic = (IntPtr)0x0059C250;
                public static IntPtr PlayMusic = (IntPtr)0x0059D880;
                public static IntPtr UpdateMusic = (IntPtr)0x0059DB20;
                public static IntPtr Profile_IsChatEnabled = (IntPtr)0x0059C2D0;
                public static IntPtr Profile_IsAvatarsEnabled = (IntPtr)0x0059C300;
                public static IntPtr UnregisterPlayer = (IntPtr)0x0059CC60;
            }
        }

        public static class CMwCmd
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_SetSchemeLocation(Game.CMwCmd thisPtr, int location);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetCmdBuffer(Game.CMwCmd thisPtr, Game.CMwCmdBuffer buffer);

            public static Del_SetSchemeLocation SetSchemeLocation;
            public static Del_SetCmdBuffer SetCmdBuffer;

            public static class Addresses
            {
                public static IntPtr SetSchemeLocation = (IntPtr)0x00925790;
                public static IntPtr SetCmdBuffer = (IntPtr)0x00848550;// SHARED_ADDRESS
            }
        }

        public static class CMwCmdFastCall
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwCmdFastCall Del_Ctor1(Game.CMwCmdFastCall thisPtr, IntPtr objPtr, IntPtr funcPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwCmdFastCall Del_Ctor2(Game.CMwCmdFastCall thisPtr, IntPtr objPtr, IntPtr funcPtr, int location);

            public static Del_Ctor1 Ctor1;
            public static Del_Ctor2 Ctor2;

            public static class Addresses
            {
                public static IntPtr Ctor1 = (IntPtr)0x009255C0;
                public static IntPtr Ctor2 = (IntPtr)0x00925650;
            }
        }

        public static class CMwCmdFastCallUser
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwCmdFastCallUser Del_Ctor(Game.CMwCmdFastCallUser thisPtr, IntPtr objPtr, IntPtr funcPtr, int unk1);

            public static Del_Ctor Ctor;

            public static class Addresses
            {
                public static IntPtr Ctor = (IntPtr)0x009254E0;
            }
        }

        public static class CMwCmdContainer
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AddCmd(Game.CMwCmdContainer thisPtr, Game.CMwCmd cmd, int location);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwCmd Del_AddFastCall(Game.CMwCmdContainer thisPtr, IntPtr objPtr, IntPtr funcPtr, int location);

            public static Del_AddCmd AddCmd;
            public static Del_AddFastCall AddFastCall;

            public static class Addresses
            {
                public static IntPtr AddCmd = (IntPtr)0x0093A990;
                public static IntPtr AddFastCall = (IntPtr)0x0093A9D0;
            }
        }

        public static class CMwCmdBufferCore
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_DestroyCoreCmdBuffer();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_CreateCoreCmdBuffer();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_ForceFpuCwForSimulationX86(int flags);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StopSimulation(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetIsSimulationOnly(Game.CMwCmdBufferCore thisPtr, BOOL value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetSimulationRelativeSpeed(Game.CMwCmdBufferCore thisPtr, float speed);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_EnableFixedTickTime(Game.CMwCmdBufferCore thisPtr, int unk1, int unk2, int unk3);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencyAddCmd(Game.CMwCmdBufferCore thisPtr, IntPtr objPtr, IntPtr funcPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencyRun(Game.CMwCmdBufferCore thisPtr, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencyEnterSafeSection(Game.CMwCmdBufferCore thisPtr, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencyLeaveSafeSection(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencyYield(Game.CMwCmdBufferCore thisPtr, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Run(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Enable(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Disable(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StartSimulation(Game.CMwCmdBufferCore thisPtr, int unk1, int unk2, float speed);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetSimulationCurrentTime(Game.CMwCmdBufferCore thisPtr, uint time);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_EnableFixedTickFrequency(Game.CMwCmdBufferCore thisPtr, int unk1, int unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_InitCmdBuffer(Game.CMwCmdBufferCore thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwCmdFastCall Del_AddNotifySetTime(Game.CMwCmdBufferCore thisPtr, IntPtr objPtr, IntPtr funcPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SubNotifySetTime(Game.CMwCmdBufferCore thisPtr, IntPtr objPtr, IntPtr funcPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HighFrequencySubCmd(Game.CMwCmdBufferCore thisPtr, IntPtr objPtr, IntPtr funcPtr);

            public static Del_DestroyCoreCmdBuffer DestroyCoreCmdBuffer;
            public static Del_CreateCoreCmdBuffer CreateCoreCmdBuffer;
            public static Del_ForceFpuCwForSimulationX86 ForceFpuCwForSimulationX86;
            public static Del_StopSimulation StopSimulation;
            public static Del_SetIsSimulationOnly SetIsSimulationOnly;
            public static Del_SetSimulationRelativeSpeed SetSimulationRelativeSpeed;
            public static Del_EnableFixedTickTime EnableFixedTickTime;
            public static Del_HighFrequencyAddCmd HighFrequencyAddCmd;
            public static Del_HighFrequencyRun HighFrequencyRun;
            public static Del_HighFrequencyEnterSafeSection HighFrequencyEnterSafeSection;
            public static Del_HighFrequencyLeaveSafeSection HighFrequencyLeaveSafeSection;
            public static Del_HighFrequencyYield HighFrequencyYield;
            public static Del_Run Run;
            public static Del_Enable Enable;
            public static Del_Disable Disable;
            public static Del_StartSimulation StartSimulation;
            public static Del_SetSimulationCurrentTime SetSimulationCurrentTime;
            public static Del_EnableFixedTickFrequency EnableFixedTickFrequency;
            public static Del_InitCmdBuffer InitCmdBuffer;
            public static Del_AddNotifySetTime AddNotifySetTime;
            public static Del_SubNotifySetTime SubNotifySetTime;
            public static Del_HighFrequencySubCmd HighFrequencySubCmd;

            public static class Addresses
            {
                public static IntPtr DestroyCoreCmdBuffer = (IntPtr)0x00922970;
                public static IntPtr CreateCoreCmdBuffer = (IntPtr)0x00923430;
                public static IntPtr ForceFpuCwForSimulationX86 = (IntPtr)0x00922990;
                public static IntPtr StopSimulation = (IntPtr)0x009229D0;
                public static IntPtr SetIsSimulationOnly = (IntPtr)0x009229F0;
                public static IntPtr SetSimulationRelativeSpeed = (IntPtr)0x00922A00;
                public static IntPtr EnableFixedTickTime = (IntPtr)0x00922A40;
                public static IntPtr HighFrequencyAddCmd = (IntPtr)0x00922A80;
                public static IntPtr HighFrequencyRun = (IntPtr)0x00922B00;
                public static IntPtr HighFrequencyEnterSafeSection = (IntPtr)0x00922BE0;
                public static IntPtr HighFrequencyLeaveSafeSection = (IntPtr)0x00922BF0;
                public static IntPtr HighFrequencyYield = (IntPtr)0x00922C00;
                public static IntPtr Run = (IntPtr)0x00922DB0;
                public static IntPtr Enable = (IntPtr)0x00923000;
                public static IntPtr Disable = (IntPtr)0x00923020;
                public static IntPtr StartSimulation = (IntPtr)0x00923040;
                public static IntPtr SetSimulationCurrentTime = (IntPtr)0x00923130;
                public static IntPtr EnableFixedTickFrequency = (IntPtr)0x009231C0;
                public static IntPtr InitCmdBuffer = (IntPtr)0x009234B0;
                public static IntPtr AddNotifySetTime = (IntPtr)0x00923670;
                public static IntPtr SubNotifySetTime = (IntPtr)0x009237A0;
                public static IntPtr HighFrequencySubCmd = (IntPtr)0x00923850;
            }
        }

        public static class CMwTimer
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_InitTimer(Game.CMwTimer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_ChopTime(Game.CMwTimer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetElapsedTimeSinceInit(Game.CMwTimer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint* Del_GetTickTime(Game.CMwTimer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SimulateDeltaTime(Game.CMwTimer thisPtr, uint deltaTime);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Tick(Game.CMwTimer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint Del_SecondsToMwTime(float seconds);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_GetHhMmSsTime24StringFromMwTime(uint time, ref Game.CFastString result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_GetHhMmSsTimeStringFromMwTime(uint time, ref Game.CFastString result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_GetHhMmTimeStringFromMwTime(uint time, ref Game.CFastString result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_GetMmSsCcTimeStringFromMwTime(uint time, ref Game.CFastString result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_GetMmSsTimeStringFromMwTime(uint time, ref Game.CFastString result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_GetMwTimeFromHhMmSsTimeString([MarshalAs(UnmanagedType.LPStr)] string str, out uint result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_GetMwTimeFromHhMmTimeString([MarshalAs(UnmanagedType.LPStr)] string str, out uint result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_GetMwTimeFromMmSsCcTimeString([MarshalAs(UnmanagedType.LPStr)] string str, out uint result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_GetMwTimeFromMmSsTimeString([MarshalAs(UnmanagedType.LPStr)] string str, out uint result);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_CMwTimer_CalibrateEnd_ShouldSwitchOff();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_CMwTimer_CalibrateStart();

            public static Del_InitTimer InitTimer;
            public static Del_ChopTime ChopTime;
            public static Del_GetElapsedTimeSinceInit GetElapsedTimeSinceInit;
            public static Del_GetTickTime GetTickTime;
            public static Del_SimulateDeltaTime SimulateDeltaTime;
            public static Del_Tick Tick;
            public static Del_SecondsToMwTime SecondsToMwTime;
            public static Del_GetHhMmSsTime24StringFromMwTime GetHhMmSsTime24StringFromMwTime;
            public static Del_GetHhMmSsTimeStringFromMwTime GetHhMmSsTimeStringFromMwTime;
            public static Del_GetHhMmTimeStringFromMwTime GetHhMmTimeStringFromMwTime;
            public static Del_GetMmSsCcTimeStringFromMwTime GetMmSsCcTimeStringFromMwTime;
            public static Del_GetMmSsTimeStringFromMwTime GetMmSsTimeStringFromMwTime;
            public static Del_GetMwTimeFromHhMmSsTimeString GetMwTimeFromHhMmSsTimeString;
            public static Del_GetMwTimeFromHhMmTimeString GetMwTimeFromHhMmTimeString;
            public static Del_GetMwTimeFromMmSsCcTimeString GetMwTimeFromMmSsCcTimeString;
            public static Del_GetMwTimeFromMmSsTimeString GetMwTimeFromMmSsTimeString;
            public static Del_CMwTimer_CalibrateEnd_ShouldSwitchOff CMwTimer_CalibrateEnd_ShouldSwitchOff;
            public static Del_CMwTimer_CalibrateStart CMwTimer_CalibrateStart;
            //DecomposeMwTime
            //DecomposeMwTime_24h
            //MwSystemTimerDestroy
            //MwSystemTimerInit

            public static class Addresses
            {
                public static IntPtr InitTimer = (IntPtr)0x00939E20;
                public static IntPtr ChopTime = (IntPtr)0x00939EA0;
                public static IntPtr GetElapsedTimeSinceInit = (IntPtr)0x00939E60;
                public static IntPtr GetTickTime = (IntPtr)0x00939E90;
                public static IntPtr SimulateDeltaTime = (IntPtr)0x0093A580;
                public static IntPtr Tick = (IntPtr)0x0093A670;
                public static IntPtr SecondsToMwTime = (IntPtr)0x004AEB50;
                public static IntPtr GetHhMmSsTime24StringFromMwTime = (IntPtr)0x0093A2C0;
                public static IntPtr GetHhMmSsTimeStringFromMwTime = (IntPtr)0x0093A100;
                public static IntPtr GetHhMmTimeStringFromMwTime = (IntPtr)0x0093A1F0;
                public static IntPtr GetMmSsCcTimeStringFromMwTime = (IntPtr)0x00939F90;
                public static IntPtr GetMmSsTimeStringFromMwTime = (IntPtr)0x0093A330;
                public static IntPtr GetMwTimeFromHhMmSsTimeString = (IntPtr)0x0093A170;
                public static IntPtr GetMwTimeFromHhMmTimeString = (IntPtr)0x0093A260;
                public static IntPtr GetMwTimeFromMmSsCcTimeString = (IntPtr)0x0093A050;
                public static IntPtr GetMwTimeFromMmSsTimeString = (IntPtr)0x0093A3A0;
                public static IntPtr CMwTimer_CalibrateEnd_ShouldSwitchOff = (IntPtr)0x00434100;
                public static IntPtr CMwTimer_CalibrateStart = (IntPtr)0x004328A0;
            }
        }

        public static class CMwTimerAdapter
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_InitTimer(Game.CMwTimerAdapter thisPtr, float relativeSpeed);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Resync(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate float Del_GetRelativeSpeed(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetRelativeSpeed(Game.CMwTimerAdapter thisPtr, float relativeSpeed);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ComputeTimeAtHumanTick(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_ConvertHumanToGame(Game.CMwTimerAdapter thisPtr, uint time);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_ConvertSystemToGame(Game.CMwTimerAdapter thisPtr, uint time);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate float Del_GetAsyncPeriod(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetAsyncPeriodMwTime(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetSchemePeriod(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint* Del_GetTickTime(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetTime(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint* Del_GetTimeAtHumanTick(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetTimeAtPreviousHumanTick(Game.CMwTimerAdapter thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetCurrentTimeAtHumanTick(Game.CMwTimerAdapter thisPtr, uint time);

            public static Del_InitTimer InitTimer;
            public static Del_Resync Resync;
            public static Del_GetRelativeSpeed GetRelativeSpeed;
            public static Del_SetRelativeSpeed SetRelativeSpeed;
            public static Del_ComputeTimeAtHumanTick ComputeTimeAtHumanTick;
            public static Del_ConvertHumanToGame ConvertHumanToGame;
            public static Del_ConvertSystemToGame ConvertSystemToGame;
            public static Del_GetAsyncPeriod GetAsyncPeriod;
            public static Del_GetAsyncPeriodMwTime GetAsyncPeriodMwTime;
            public static Del_GetSchemePeriod GetSchemePeriod;
            public static Del_GetTickTime GetTickTime;
            public static Del_GetTime GetTime;
            public static Del_GetTimeAtHumanTick GetTimeAtHumanTick;
            public static Del_GetTimeAtPreviousHumanTick GetTimeAtPreviousHumanTick;
            public static Del_SetCurrentTimeAtHumanTick SetCurrentTimeAtHumanTick;

            public static class Addresses
            {
                public static IntPtr InitTimer = (IntPtr)0x0093A400;
                public static IntPtr Resync = (IntPtr)0x0093A840;
                public static IntPtr GetRelativeSpeed = (IntPtr)0x0093A4B0;
                public static IntPtr SetRelativeSpeed = (IntPtr)0x0093A7F0;
                public static IntPtr ComputeTimeAtHumanTick = (IntPtr)0x0093A890;
                public static IntPtr ConvertHumanToGame = (IntPtr)0x0093A4C0;
                public static IntPtr ConvertSystemToGame = (IntPtr)0x0093A520;
                public static IntPtr GetAsyncPeriod = (IntPtr)0x0093A440;
                public static IntPtr GetAsyncPeriodMwTime = (IntPtr)0x0093A460;
                public static IntPtr GetSchemePeriod = (IntPtr)0x0075CC40;
                public static IntPtr GetTickTime = (IntPtr)0x0093A4A0;
                public static IntPtr GetTime = (IntPtr)0x0093A870;
                public static IntPtr GetTimeAtHumanTick = (IntPtr)0x00808AA0;// SHARED_ADDRESS
                public static IntPtr GetTimeAtPreviousHumanTick = (IntPtr)0x0093A8B0;
                public static IntPtr SetCurrentTimeAtHumanTick = (IntPtr)0x0093A8D0;
            }
        }

        public static class CHmsZoneDynamic
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_PhysicsStep2(Game.CHmsZoneDynamic thisPtr);

            public static Del_PhysicsStep2 PhysicsStep2;

            public static class Addresses
            {
                public static IntPtr PhysicsStep2 = (IntPtr)0x00549C90;
            }
        }

        public static class CTrackManiaRace
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_InputRace(Game.CTrackManiaRace thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CTrackManiaPlayerInfo Del_GetPlayingPlayerInfo(Game.CTrackManiaRace thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CTrackManiaPlayer Del_GetPlayingPlayer(Game.CTrackManiaRace thisPtr);

            public static Del_InputRace InputRace;
            public static Del_GetPlayingPlayerInfo GetPlayingPlayerInfo;
            public static Del_GetPlayingPlayer GetPlayingPlayer;

            public static class Addresses
            {
                public static IntPtr InputRace = (IntPtr)0x0047CF80;
                public static IntPtr GetPlayingPlayerInfo = (IntPtr)0x0047BB00;
                public static IntPtr GetPlayingPlayer = (IntPtr)0x0047BB10;
            }
        }

        public static class CTrackManiaControlPlayerInput
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_UpdateVehicleStateFromInputs(Game.CTrackManiaControlPlayerInput thisPtr, ref Game.CTrackManiaControlPlayerInput.SRaceInputs inputs);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_UpdateVehicleStateFromInputsImpl(ref Game.CTrackManiaControlPlayerInput.SRaceInputs inputs, Game.CSceneMobil mobil);

            public static Del_UpdateVehicleStateFromInputs UpdateVehicleStateFromInputs;
            public static Del_UpdateVehicleStateFromInputsImpl UpdateVehicleStateFromInputsImpl;

            public static class Addresses
            {
                public static IntPtr UpdateVehicleStateFromInputs = (IntPtr)0x004FE7E0;
                public static IntPtr UpdateVehicleStateFromInputsImpl = (IntPtr)0x004FE500;
            }
        }

        public static class CTrackManiaPlayerInfo
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AddInputEvent(Game.CTrackManiaPlayerInfo thisPtr, ref Game.SInputEvent inputEvent, uint time);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetInputState(Game.CTrackManiaPlayerInfo thisPtr, ref Game.SInputEvent inputEvent, uint time);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GetInputState(Game.CTrackManiaPlayerInfo thisPtr, SInputActionDesc action, uint time, ref SMwTimedValueInstant result);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_LockInputs(Game.CTrackManiaPlayerInfo thisPtr, uint time);

            public static Del_AddInputEvent AddInputEvent;
            public static Del_SetInputState SetInputState;
            public static Del_GetInputState GetInputState;
            public static Del_LockInputs LockInputs;

            public static class Addresses
            {
                public static IntPtr AddInputEvent = (IntPtr)0x004AEA60;
                public static IntPtr SetInputState = (IntPtr)0x004AEA90;
                public static IntPtr GetInputState = (IntPtr)0x004AEB00;
                public static IntPtr LockInputs = (IntPtr)0x004AEAC0;
            }
        }

        public static class CTrackManiaMenus
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MenuMain_Init(Game.CTrackManiaMenus thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MenuSolo(Game.CTrackManiaMenus thisPtr);

            public static Del_MenuMain_Init MenuMain_Init;
            public static Del_MenuSolo MenuSolo;

            public static class Addresses
            {
                public static IntPtr MenuMain_Init = (IntPtr)0x0046C330;
                public static IntPtr MenuSolo = (IntPtr)0x004ED5F0;
            }
        }

        public static class CMwEngineManager
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwClassInfo Del_AddClass(Game.CMwEngineManager thisPtr, Game.CMwClassInfo classInfo);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwClassInfo Del_GetClassInfo(Game.CMwEngineManager thisPtr, Game.EMwClassId classId);

            public static Del_AddClass AddClass;
            public static Del_GetClassInfo GetClassInfo;

            public static class Addresses
            {
                public static IntPtr AddClass = (IntPtr)0x0093E7D0;
                public static IntPtr GetClassInfo = (IntPtr)0x0093E920;
            }
        }

        public static class CMwEngineInfo
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwClassInfo Del_AddClass(Game.CMwEngineInfo thisPtr, Game.CMwClassInfo classInfo);

            public static Del_AddClass AddClass;

            public static class Addresses
            {
                public static IntPtr AddClass = (IntPtr)0x00955410;
            }
        }

        public static class CMwParam
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsIndexed(Game.CMwParam thisPtr);

            public static Del_IsIndexed IsIndexed;

            public static class Addresses
            {
                public static IntPtr IsIndexed = (IntPtr)0x0042BD70;
            }
        }

        public static class CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_Get(Game.CMwNod thisPtr, Game.CMwStack stack, Game.CMwValueStd value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_Set(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_Add(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_Sub(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_Check(Game.CMwNod thisPtr, Game.CMwStack stack);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Param_SetStr(Game.CMwNod thisPtr, ref Game.CFastString unk1, ref Game.CFastStringInt unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_MwAddRef(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_MwRelease(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_MwForceRef(Game.CMwNod thisPtr, int newRefCount);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwAddDependant(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwAddReceiver(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwSubDependant(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwSubDependantSafe(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwFinalSubDependant(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_DependantSendMwIsKilled(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetMwParamIdForRecursiveIndex(Game.CMwNod thisPtr, int index);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetMwParamIdRecursiveCount(Game.CMwNod thisPtr, int paramId);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_MwGetNearestFather(Game.CMwNod thisPtr, int unk1, ref int unk2);
            // Statics
            // CMwNod::DumpNodToString - RETN
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Game.SMwParamInfo Del_GetParamInfoFromParamId(uint paramId);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_StaticInit();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_AddClass(Game.CMwClassInfo classInfo);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_MwBuildClassInfoTree();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Game.CMwNod Del_CreateByMwClassId(Game.EMwClassId classId);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Game.CMwClassInfo Del_StaticGetClassInfo(Game.EMwClassId classId);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate BOOL Del_StaticMwIsKindOf(Game.EMwClassId classIdA, Game.EMwClassId classIdB);

            public static Del_Param_Get Param_Get;
            public static Del_Param_Set Param_Set;
            public static Del_Param_Add Param_Add;
            public static Del_Param_Sub Param_Sub;
            public static Del_Param_Check Param_Check;
            public static Del_Param_SetStr Param_SetStr;
            public static Del_MwAddRef MwAddRef;
            public static Del_MwRelease MwRelease;
            public static Del_MwForceRef MwForceRef;
            public static Del_MwAddDependant MwAddDependant;
            public static Del_MwAddReceiver MwAddReceiver;
            public static Del_MwSubDependant MwSubDependant;
            public static Del_MwSubDependantSafe MwSubDependantSafe;
            public static Del_MwFinalSubDependant MwFinalSubDependant;
            public static Del_DependantSendMwIsKilled DependantSendMwIsKilled;
            public static Del_GetMwParamIdForRecursiveIndex GetMwParamIdForRecursiveIndex;
            public static Del_GetMwParamIdRecursiveCount GetMwParamIdRecursiveCount;
            public static Del_MwGetNearestFather MwGetNearestFather;
            // Statics
            public static Del_GetParamInfoFromParamId GetParamInfoFromParamId;
            public static Del_StaticInit StaticInit;
            public static Del_AddClass AddClass;
            public static Del_MwBuildClassInfoTree MwBuildClassInfoTree;
            public static Del_CreateByMwClassId CreateByMwClassId;
            public static Del_StaticGetClassInfo StaticGetClassInfo;
            public static Del_StaticMwIsKindOf StaticMwIsKindOf;

            public static class Addresses
            {
                public static IntPtr Param_Get = (IntPtr)0x009243F0;
                public static IntPtr Param_Set = (IntPtr)0x00924500;
                public static IntPtr Param_Add = (IntPtr)0x00924580;
                public static IntPtr Param_Sub = (IntPtr)0x009245E0;
                public static IntPtr Param_Check = (IntPtr)0x00924640;
                public static IntPtr Param_SetStr = (IntPtr)0x00924CB0;// overload (Param_Set)
                public static IntPtr MwAddRef = (IntPtr)0x00923E60;
                public static IntPtr MwRelease = (IntPtr)0x00924910;
                public static IntPtr MwForceRef = (IntPtr)0x00923E70;
                public static IntPtr MwAddDependant = (IntPtr)0x00924060;
                public static IntPtr MwAddReceiver = (IntPtr)0x009240A0;
                public static IntPtr MwSubDependant = (IntPtr)0x00924020;
                public static IntPtr MwSubDependantSafe = (IntPtr)0x00924030;
                public static IntPtr MwFinalSubDependant = (IntPtr)0x00924830;
                public static IntPtr DependantSendMwIsKilled = (IntPtr)0x00923F90;
                public static IntPtr GetMwParamIdForRecursiveIndex = (IntPtr)0x00923E80;
                public static IntPtr GetMwParamIdRecursiveCount = (IntPtr)0x00923EA0;
                public static IntPtr MwGetNearestFather = (IntPtr)0x00923EC0;
                // Statics
                public static IntPtr GetParamInfoFromParamId = (IntPtr)0x00923EE0;
                public static IntPtr StaticInit = (IntPtr)0x00923F30;
                public static IntPtr AddClass = (IntPtr)0x00923D60;
                public static IntPtr MwBuildClassInfoTree = (IntPtr)0x00923D70;
                public static IntPtr CreateByMwClassId = (IntPtr)0x00923D30;
                public static IntPtr StaticGetClassInfo = (IntPtr)0x00923D50;
                public static IntPtr StaticMwIsKindOf = (IntPtr)0x00923DB0;
            }
        }

        public static class CMwClassInfo
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_MwGetNearestFather(Game.CMwClassInfo thisPtr, int unk1, ref int unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsMwParamIdEqualName(Game.CMwClassInfo thisPtr, int unk1, [MarshalAs(UnmanagedType.LPStr)] string name, int unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetMwParamIdFromName(Game.CMwClassInfo thisPtr, [MarshalAs(UnmanagedType.LPStr)] string name, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.SMwParamInfo Del_GetMwParamFromName(Game.CMwClassInfo thisPtr, [MarshalAs(UnmanagedType.LPStr)] string name, int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.SMwParamInfo Del_GetMwParamFromNameRecursive(Game.CMwClassInfo thisPtr, [MarshalAs(UnmanagedType.LPStr)] string name);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetMwParamIdRecursive_FromIndex(Game.CMwClassInfo thisPtr, int index);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetMwParamIdRecursive_Count(Game.CMwClassInfo thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Game.CMwClassInfo Del_FindFromClassName(ref Game.CFastString str);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_AddChild(Game.CMwClassInfo thisPtr, Game.CMwClassInfo child);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_BuildTree(Game.CMwClassInfo thisPtr);

            public static Del_MwGetNearestFather MwGetNearestFather;
            public static Del_IsMwParamIdEqualName IsMwParamIdEqualName;
            public static Del_GetMwParamIdFromName GetMwParamIdFromName;
            public static Del_GetMwParamFromName GetMwParamFromName;
            public static Del_GetMwParamFromNameRecursive GetMwParamFromNameRecursive;
            public static Del_GetMwParamIdRecursive_FromIndex GetMwParamIdRecursive_FromIndex;
            public static Del_GetMwParamIdRecursive_Count GetMwParamIdRecursive_Count;
            public static Del_FindFromClassName FindFromClassName;
            public static Del_AddChild AddChild;
            public static Del_BuildTree BuildTree;

            public static class Addresses
            {
                public static IntPtr MwGetNearestFather = (IntPtr)0x00925850;
                public static IntPtr IsMwParamIdEqualName = (IntPtr)0x00925890;
                public static IntPtr GetMwParamIdFromName = (IntPtr)0x00925900;
                public static IntPtr GetMwParamFromName = (IntPtr)0x00925980;
                public static IntPtr GetMwParamFromNameRecursive = (IntPtr)0x009259B0;
                public static IntPtr GetMwParamIdRecursive_FromIndex = (IntPtr)0x00925A30;
                public static IntPtr GetMwParamIdRecursive_Count = (IntPtr)0x00925A50;
                public static IntPtr FindFromClassName = (IntPtr)0x00925A70;
                public static IntPtr AddChild = (IntPtr)0x00925AE0;
                public static IntPtr BuildTree = (IntPtr)0x00925B70;
            }
        }

        public static class Memory
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr Del_new_(int size);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del_delete(IntPtr address);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr Del__memcpy(IntPtr dst, IntPtr src, int size);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr Del__memmove(IntPtr dst, IntPtr src, int size);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Del__memset(IntPtr ptr, int value, int num);

            public static Del_new_ new_;
            public static Del_delete delete;
            public static Del__memcpy _memcpy;
            public static Del__memmove _memmove;
            public static Del__memset _memset;

            public static class Addresses
            {
                public static IntPtr new_ = (IntPtr)0x00403079;
                public static IntPtr delete = (IntPtr)0x00403079;
                public static IntPtr _memcpy = (IntPtr)0x0040C730;
                public static IntPtr _memmove = (IntPtr)0x0040A8C0;
                public static IntPtr _memset = (IntPtr)0x0040AF70;
            }
        }

        public static class CFastString
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Ctor(ref Game.CFastString thisPtr, [MarshalAs(UnmanagedType.LPStr)] string str);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Dtor(ref Game.CFastString thisPtr);

            public static Del_Ctor Ctor;
            public static Del_Dtor Dtor;

            public static class Addresses
            {
                public static IntPtr Ctor = (IntPtr)0x00401650;
                public static IntPtr Dtor = (IntPtr)0x005DB600;// SHARED_ADDRESS
            }
        }

        public static class CFastStringInt
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Ctor(ref Game.CFastStringInt thisPtr, [MarshalAs(UnmanagedType.LPWStr)] string str);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Dtor(ref Game.CFastStringInt thisPtr);

            public static Del_Ctor Ctor;
            public static Del_Dtor Dtor;

            public static class Addresses
            {
                public static IntPtr Ctor = (IntPtr)0x00401740;
                public static IntPtr Dtor = (IntPtr)0x005F4320;// SHARED_ADDRESS
            }
        }

        public static class CSystemFidFile
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetFullName(Game.CSystemFidFile thisPtr, ref Game.CFastStringInt str, int unk1, int unk2);

            public static Del_GetFullName GetFullName;

            public static class Addresses
            {
                public static IntPtr GetFullName = (IntPtr)0x0042B590;
            }
        }
    }
}
