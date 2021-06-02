using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using ModTMNF.Game;

#pragma warning disable 649
#pragma warning disable 108

namespace ModTMNF
{
    // "VT.XXXX.Offsets.VTable" is REQUIRED for EVERY CMwNod type

    // TODO add missing vtables:
    //CGameCtnMediaBlockEditor
    //CGameRaceInterface
    //CGameCalendar
    //CCurveInterface

    /// <summary>
    /// Virtual function tables
    /// </summary>
    public unsafe static class VT
    {
        // We're doing some funky stuff here to cache vtable delegates without using a dictionary.
        // Idealy we would store additional managed info in CMwClassInfo, but there's no room.
        // NOTE: We could potentially add a fake CMwClassInfo::Children entry. We could reallocate
        //       the buffer and place a pointer to a managed object there. Giving us a managed object
        //       for every class without having to do array lookups (or the interoped call).
        static VT.CMwNod[][] vtables;

        /// <summary>
        /// Use this lookup where there isn't a unique class id for a given class.
        /// Slower than the array lookup. Might need to think of another method if this gets too big.
        /// vtablePtr->vtableObj
        /// </summary>
        static Dictionary<IntPtr, VT.CMwNod> vtablesSlowLookup = new Dictionary<IntPtr, VT.CMwNod>();

        // Add this to any types which fail to resolve due to not having a class id (CGbxApp/CGbxGame).
        class AddSlowLookupAttribute : Attribute { }

        public static bool Init()
        {
            Dictionary<int, int> maxClassId = new Dictionary<int, int>();
            int highestEngineId = 0;
            foreach (EMwEngineId engineId in Enum.GetValues(typeof(EMwEngineId)))
            {
                int eeid = CMwEngineManager.LongToShortEngineId((int)engineId);
                if (eeid > highestEngineId)
                {
                    highestEngineId = eeid;
                }
                maxClassId[eeid] = 0;
            }
            vtables = new VT.CMwNod[highestEngineId + 1][];
            foreach (EMwClassId classId in Enum.GetValues(typeof(EMwClassId)))
            {
                int cid = CMwEngineManager.LongToShortClassId((int)classId);
                int engineId = CMwEngineManager.LongToShortEngineId((int)CMwEngineManager.GetEngineIdFromClassId(classId));
                if (cid > maxClassId[engineId])
                {
                    maxClassId[engineId] = cid;
                }
            }
            foreach (KeyValuePair<int, int> kvp in maxClassId)
            {
                vtables[kvp.Key] = new VT.CMwNod[kvp.Value + 1];
            }
            foreach (Type type in typeof(VT).GetNestedTypes())
            {
                if (typeof(VT.CMwNod).IsAssignableFrom(type))
                {
                    VT.CMwNod obj = (VT.CMwNod)Activator.CreateInstance(type);
                    InitVTable(obj, type);

                    // NOTE: A few classes don't have a class id... (CGbxApp/CGbxGame)
                    EMwClassId classId;
                    if (Enum.TryParse<EMwClassId>(type.Name, out classId))
                    {
                        EMwEngineId engineId = CMwEngineManager.GetEngineIdFromClassId(classId);
                        int eeid = CMwEngineManager.LongToShortEngineId((int)engineId);
                        int cid = CMwEngineManager.LongToShortClassId((int)classId);
                        try
                        {
                            vtables[eeid][cid] = obj;
                        }
                        catch (Exception e)
                        {
                            Program.Log(eeid + " " + cid);
                            Program.Log(e.ToString());
                            throw;
                        }
                    }
                }
                else
                {
                    // TODO: Handle other vtable types
                }
            }
            return true;
        }

        private static void InitVTable(VT.CMwNod obj, Type type)
        {
            Type offsetsType = type.GetNestedType("Offsets");
            if (offsetsType != null)
            {
                FieldInfo vtablePtrField = offsetsType.GetField("VTable");
                if (vtablePtrField != null)
                {
                    IntPtr vtablePtr = (IntPtr)vtablePtrField.GetValue(null);
                    if (vtablePtr != IntPtr.Zero)
                    {
                        if (vtablePtrField.GetCustomAttributes(typeof(AddSlowLookupAttribute), false).Length > 0)
                        {
                            vtablesSlowLookup[vtablePtr] = obj;
                        }
                        InitVTable(obj, type, vtablePtr);
                    }
                }
            }
        }

        private static void InitVTable(VT.CMwNod obj, Type type, IntPtr vtablePtr)
        {
            Type offsetsType = type.GetNestedType("Offsets");
            foreach (FieldInfo fieldInfo in offsetsType.GetFields())
            {
                FieldInfo delegateFieldInfo = type.GetField(fieldInfo.Name);
                if (delegateFieldInfo != null && typeof(Delegate).IsAssignableFrom(delegateFieldInfo.FieldType))
                {
                    int offset = (int)fieldInfo.GetValue(null);
                    IntPtr funcPtr = Marshal.ReadIntPtr(vtablePtr, offset);
                    delegateFieldInfo.SetValue(obj, Marshal.GetDelegateForFunctionPointer(funcPtr, delegateFieldInfo.FieldType));
                }
            }
            if (typeof(VT.CMwNod).IsAssignableFrom(type.BaseType))
            {
                InitVTable(obj, type.BaseType, vtablePtr);
            }
        }

        public static TTable Get<TTable>(IntPtr nodAddress) where TTable : VT.CMwNod
        {
            EMwClassId classId = NativeDll.GetMwClassId(nodAddress);
            if (classId == EMwClassId.CMwNod)
            {
                // CMwNod is an abstract type. This is possible a non-registered class.
                VT.CMwNod result;
                vtablesSlowLookup.TryGetValue(*(IntPtr*)nodAddress, out result);
                if (result == null)
                {
                    Program.Log("ERROR - no vtable found for ptr " + nodAddress.ToInt32().ToString("X8"));
                }
                return (TTable)result;
            }
            EMwEngineId engineId = CMwEngineManager.GetEngineIdFromClassId(classId);
            int eeid = CMwEngineManager.LongToShortEngineId((int)engineId);
            int cid = CMwEngineManager.LongToShortClassId((int)classId);
            return (TTable)vtables[eeid][cid];
        }

        public class CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Dtor(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate CMwClassInfo Del_MwGetClassInfo(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate EMwClassId Del_GetMwClassId(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_MwIsKindOf(Game.CMwNod thisPtr, EMwClassId classId);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate CMwIdPtr Del_MwGetId(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetIdName(Game.CMwNod thisPtr, string idName);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwIsKilled(Game.CMwNod thisPtr, Game.CMwNod unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwIsUnreferenced(Game.CMwNod thisPtr, Game.CMwNod unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_VirtualParam_Get(Game.CMwNod thisPtr, Game.CMwStack stack, Game.CMwStack valuePtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_VirtualParam_Set(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr valuePtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_VirtualParam_Add(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr valuePtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_VirtualParam_Sub(Game.CMwNod thisPtr, Game.CMwStack stack, IntPtr valuePtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CMwNod Del_Archive(Game.CMwNod thisPtr, Game.CClassicArchive archive);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Chunk(Game.CMwNod thisPtr, Game.CClassicArchive archive, int chunkId);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetChunkInfo(Game.CMwNod thisPtr, int chunkId);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetUidChunkFromIndex(Game.CMwNod thisPtr, int index);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate uint Del_GetChunkCount(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_OnNodLoaded(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_CreateDefaultData(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_OnPackAvailable(Game.CMwNod thisPtr, Game.CSystemFid unk1, Game.CSystemFid unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ApplyFidParameters(Game.CMwNod thisPtr, Game.CSystemFidParameters unk1, Game.CSystemFidParameters unk2, Game.CFastBuffer unk3);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_OnCrashDump(Game.CMwNod thisPtr, ref Game.CFastString unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_CopyFrom(Game.CMwNod thisPtr, Game.CMwNod other);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_HeaderChunk(Game.CMwNod thisPtr, Game.CClassicArchive archive, ref int chunkId, ref BOOL light);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GetUidHeaderChunkFromIndex(Game.CMwNod thisPtr, int index);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetHeaderChunkCount(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetUidMessageFromIndex(Game.CMwNod thisPtr, int index);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_GetMessageCount(Game.CMwNod thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_MwReceiveMessage(Game.CMwNod thisPtr, ref int unk1, Game.CMwNod unk2, ref int unk3);

            public Del_Dtor Dtor;
            public Del_MwGetClassInfo MwGetClassInfo;
            public Del_GetMwClassId GetMwClassId;
            public Del_MwIsKindOf MwIsKindOf;
            public Del_MwGetId MwGetId;
            public Del_SetIdName SetIdName;
            public Del_MwIsKilled MwIsKilled;
            public Del_MwIsUnreferenced MwIsUnreferenced;
            public Del_VirtualParam_Get VirtualParam_Get;
            public Del_VirtualParam_Set VirtualParam_Set;
            public Del_VirtualParam_Add VirtualParam_Add;
            public Del_VirtualParam_Sub VirtualParam_Sub;
            public Del_Archive Archive;
            public Del_Chunk Chunk;
            public Del_GetChunkInfo GetChunkInfo;
            public Del_GetUidChunkFromIndex GetUidChunkFromIndex;
            public Del_GetChunkCount GetChunkCount;
            public Del_OnNodLoaded OnNodLoaded;
            public Del_CreateDefaultData CreateDefaultData;
            public Del_OnPackAvailable OnPackAvailable;
            public Del_ApplyFidParameters ApplyFidParameters;
            public Del_OnCrashDump OnCrashDump;
            public Del_CopyFrom CopyFrom;
            public Del_HeaderChunk HeaderChunk;
            public Del_GetUidHeaderChunkFromIndex GetUidHeaderChunkFromIndex;
            public Del_GetHeaderChunkCount GetHeaderChunkCount;
            public Del_GetUidMessageFromIndex GetUidMessageFromIndex;
            public Del_GetMessageCount GetMessageCount;
            public Del_MwReceiveMessage MwReceiveMessage;

            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00BC61D4;
                public static int CMwNod_FirstVirtualMethodVTable_DoNotDerive = 0;// We might be able to use this entry for storage (per-class), could directly point to the C# obj.
                public static int Dtor = 4;
                public static int MwGetClassInfo = 8;
                public static int GetMwClassId = 12;
                public static int MwIsKindOf = 16;
                public static int MwGetId = 20;
                public static int SetIdName = 24;
                public static int MwIsKilled = 28;
                public static int MwIsUnreferenced = 32;
                public static int VirtualParam_Get = 36;
                public static int VirtualParam_Set = 40;
                public static int VirtualParam_Add = 44;
                public static int VirtualParam_Sub = 48;
                public static int Archive = 52;
                public static int Chunk = 56;
                public static int GetChunkInfo = 60;
                public static int GetUidChunkFromIndex = 64;
                public static int GetChunkCount = 68;
                public static int OnNodLoaded = 72;
                public static int CreateDefaultData = 76;
                public static int OnPackAvailable = 80;
                public static int ApplyFidParameters = 84;
                public static int OnCrashDump = 88;
                public static int CopyFrom = 92;
                public static int HeaderChunk = 96;
                public static int GetUidHeaderChunkFromIndex = 100;
                public static int GetHeaderChunkCount = 104;
                public static int GetUidMessageFromIndex = 108;
                public static int GetMessageCount = 112;
                public static int MwReceiveMessage = 116;
            }
        }

        public class CMwParam : VT.CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsArray(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsBuffer(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsBufferCat(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsRefBuffer(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_IndexedGetCount(Game.CMwParam thisPtr, Game.CMwValueStd value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsStruct(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_StructGetCount(Game.CMwParam thisPtr, Game.CMwValueStd value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_VGetValue(Game.CMwParam thisPtr, Game.CMwStack stack, Game.CMwValueStd value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_VSetValue(Game.CMwParam thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_VAddValue(Game.CMwParam thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_VSubValue(Game.CMwParam thisPtr, Game.CMwStack stack, IntPtr value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_VGetNextNod(Game.CMwParam thisPtr, Game.CMwStack stack, ref Game.CMwNod value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_VCanGetValueFromString(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_VCanGetStringFromValue(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_VGetValueFromString(Game.CMwParam thisPtr, Game.CMwValueStd value, ref CFastStringInt str, Game.SMwParamInfo paramInfo);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_VGetStringFromValue(Game.CMwParam thisPtr, ref Game.CFastStringInt str, IntPtr value, Game.SMwParamInfo paramInfo);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_GetIcon(Game.CMwParam thisPtr, ref EMwIconList unk1, ref EMwIconList unk2);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ParseSubParams(Game.CMwParam thisPtr, Game.CMwNod unk1, Game.CMwStack unk2, Game.CMwNod unk3, IntPtr unk4FuncPtr, IntPtr unk5, uint unk6);

            public Del_IsArray IsArray;
            public Del_IsBuffer IsBuffer;
            public Del_IsBufferCat IsBufferCat;
            public Del_IsRefBuffer IsRefBuffer;
            public Del_IndexedGetCount IndexedGetCount;
            public Del_IsStruct IsStruct;
            public Del_StructGetCount StructGetCount;
            public Del_VGetValue VGetValue;
            public Del_VSetValue VSetValue;
            public Del_VAddValue VAddValue;
            public Del_VSubValue VSubValue;
            public Del_VGetNextNod VGetNextNod;
            public Del_VCanGetValueFromString VCanGetValueFromString;
            public Del_VCanGetStringFromValue VCanGetStringFromValue;
            public Del_VGetValueFromString VGetValueFromString;
            public Del_VGetStringFromValue VGetStringFromValue;
            public Del_GetIcon GetIcon;
            public Del_ParseSubParams ParseSubParams;

            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00BCA844;
                public static int IsArray = 120;
                public static int IsBuffer = 124;
                public static int IsBufferCat = 128;
                public static int IsRefBuffer = 132;
                public static int IndexedGetCount = 136;
                public static int IsStruct = 140;
                public static int StructGetCount = 144;
                public static int VGetValue = 148;
                public static int VSetValue = 152;
                public static int VAddValue = 156;
                public static int VSubValue = 160;
                public static int VGetNextNod = 164;
                public static int VCanGetValueFromString = 168;
                public static int VCanGetStringFromValue = 172;
                public static int VGetValueFromString = 176;
                public static int VGetStringFromValue = 180;
                public static int GetIcon = 184;
                public static int ParseSubParams = 188;
            }
        }

        public class CGbxApp : VT.CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Destroy(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_Init(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StartApp(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ExitGame(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StopApp(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_OnViewportCreation(Game.CGbxApp thisPtr, Game.CHmsViewport viewport, ref int unk1);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_IsForceWindowed(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_RenderWaitingFrame(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_EngineInitEnd(Game.CGbxApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_SetCmdLineUrl(Game.CGbxApp thisPtr, ref CFastString value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_SetCmdLineFile(Game.CGbxApp thisPtr, ref CFastStringInt value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_ApplyCommandLineArgs(Game.CGbxApp thisPtr);

            public Del_Destroy Destroy;
            public Del_Init Init;
            public Del_StartApp StartApp;
            public Del_ExitGame ExitGame;
            public Del_StopApp StopApp;
            public Del_OnViewportCreation OnViewportCreation;
            public Del_IsForceWindowed IsForceWindowed;
            public Del_RenderWaitingFrame RenderWaitingFrame;
            public Del_EngineInitEnd EngineInitEnd;
            public Del_SetCmdLineUrl SetCmdLineUrl;
            public Del_SetCmdLineFile SetCmdLineFile;
            public Del_ApplyCommandLineArgs ApplyCommandLineArgs;

            public static class Offsets
            {
                [AddSlowLookup]
                public static IntPtr VTable = (IntPtr)0x00B5518C;
                public static int Destroy = 120;
                public static int Init = 124;
                public static int StartApp = 128;
                public static int ExitGame = 132;
                public static int StopApp = 136;
                public static int OnViewportCreation = 140;
                public static int IsForceWindowed = 144;
                public static int RenderWaitingFrame = 148;
                public static int EngineInitEnd = 152;
                public static int SetCmdLineUrl = 156;
                public static int SetCmdLineFile = 160;
                public static int ApplyCommandLineArgs = 164;
            }
        }

        public class CGbxGame : VT.CGbxApp
        {
            public static class Offsets
            {
                [AddSlowLookup]
                public static IntPtr VTable = (IntPtr)0x00B2BCEC;
            }
        }

        public class CGameApp : VT.CGameProcess
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameCtnChallenge Del_GetChallenge(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_CanUseManialinkAnywhere(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_CanUseMessenger(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate BOOL Del_CanUseMoney(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_QuitGame(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_RegisterPlayer(Game.CGameApp thisPtr, Game.CGamePlayer player);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_UnregisterPlayers(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ResetGame(Game.CGameApp thisPtr, BOOL resetTime);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_ApplySystemConfig(Game.CGameApp thisPtr, Game.CSystemConfig systemConfig);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameNetwork Del_GetNetwork(Game.CGameApp thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate Game.CGameInterface Del_GetInterface(Game.CGameApp thisPtr);

            public Del_GetChallenge GetChallenge;
            public Del_CanUseManialinkAnywhere CanUseManialinkAnywhere;
            public Del_CanUseMessenger CanUseMessenger;
            public Del_CanUseMessenger CanUseMoney;
            public Del_QuitGame QuitGame;
            public Del_RegisterPlayer RegisterPlayer;
            public Del_UnregisterPlayers UnregisterPlayers;
            public Del_ResetGame ResetGame;
            public Del_ApplySystemConfig ApplySystemConfig;
            public Del_GetNetwork GetNetwork;
            public Del_GetInterface GetInterface;

            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00B60DFC;
                public static int Unk_128 = 128;
                public static int GetChallenge = 132;
                public static int CameraSetCreate = 136;
                public static int VehicleSetVisibility = 140;
                public static int CreatePlayerMobilInstance = 144;
                public static int ReleasePlayerMobilInstance = 148;
                public static int UpdateBlockMobils = 152;
                public static int GetCollectorVehicle = 156;
                public static int Start = 160;
                public static int OnStartGame = 164;
                public static int QuitGame = 168;
                public static int Destroy = 172;
                public static int RegisterPlayer = 176;
                public static int UnregisterPlayers = 180;
                public static int ResetGame = 184;
                public static int ApplySystemConfig = 188;
                public static int OpenMessenger = 192;
                public static int OnGraphicSettings = 196;
                public static int UrlLinkProcess = 200;
                public static int UrlLinkTransform = 204;
                public static int CanUseManialinkAnywhere = 208;
                public static int CanUseMessenger = 212;
                public static int CanUseMoney = 216;
                public static int OnMenuDisplayed = 220;
                public static int OnMenuHiden = 224;
                public static int Unk_228 = 228;
                public static int ProcessMenuInput = 232;
                public static int CmdLineUrlTmtp_JoinServer = 236;
                public static int CmdLineUrlTmtp_AddBuddy = 240;
                public static int CmdLineUrlTmtp_InviteBuddy = 244;
                public static int CmdLineUrlTmtp_AddFavourite = 248;
                public static int CmdLineUrlTmtp_StartDirectScore = 252;
                public static int CmdLineFile_Open = 256;
                public static int Unk_260 = 260;
                public static int Unk_264 = 264;
                public static int Unk_268 = 268;
                public static int Unk_272 = 272;
                public static int UpdatePacksAvailabilityAndUse = 276;
                public static int Unk_280 = 280;
            }
        }

        public class CGameCtnApp : VT.CGameApp
        {
            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00B6890C;
                public static int StartUpShowIntroSlidesAndRollingDemo = 284;
                public static int QuitGameAndExit = 288;
                public static int QuitGameInternal = 292;
                public static int InitAfterProfiles = 296;
                public static int AskConnectionType = 300;
                public static int ConnectToInternetAccount = 304;
                public static int InitShadows = 308;
                public static int SetMessengerActivated = 312;
                public static int CloseMessenger = 316;
                public static int LoadChallenge = 320;
                public static int LoadChallenge_PostLoading = 324;
                public static int LaunchMoveFromLeagueRequest = 328;
                public static int ScanDiskForChallenges = 332;
                public static int ScanDiskForCampaigns = 336;
                public static int ScanDiskForCollectors = 340;
                public static int ScanDiskForProfilesAndScores = 344;
                public static int ScanDiskForMatchSettings = 348;
                public static int InitCampaigns = 352;
                public static int OnChallengeInfoDeleted = 356;
                public static int SaveProfileAndScore = 360;
                public static int GetPlayground = 364;
                public static int GetVehicleNameFont = 368;
                public static int PlayerMobilDecorateMobil = 372;
                public static int GetDefaultVehicleIdentifier = 376;
                public static int ChallengeCreateSceneGraph = 380;
                public static int ExitChallenge = 384;
                public static int PrepareEditorChallengeSave = 388;
                public static int MediaTrackerGhostLoad = 392;
                public static int MediaTrackerGhostSave = 396;
                public static int OnPlayFieldModified = 400;
                public static int PlayerMobilInstanceFillUpStruct = 404;
                public static int FillAllowedCtnMediaBlockClassIds = 408;
                public static int GetChallengesScoresAndPlayerRecords_OnSuccess = 412;
                public static int GetChallengesScoresAndPlayerRecords_OnFailure = 416;
                public static int GetAllSoloCampaigns = 420;
                public static int AssociateCampaignOfficialRecords = 424;
                public static int OnTrackReceivedForManiaCodePlayTrack1 = 428;
                public static int OnTrackReceivedForManiaCodePlayTrack2 = 432;
                public static int OnReplayReceivedForManiaCodePlayReplay = 436;
                public static int OnReplayReceivedForManiaCodeViewReplay = 440;
                public static int OnManiaCodeJoinServer = 444;
                public static int OnManiaCodeAddBuddy = 448;
                public static int OnManiaCodeInviteBuddy = 452;
                public static int OnManiaCodeAddFavourite = 456;
                public static int OnManiaCodePlayScore = 460;
            }
        }

        public class CTrackMania : VT.CGameCtnApp
        {
            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00B3E074;
                public static int StopOfficialRecord_OnSuccess = 464;
                public static int OfficialRecord_OnFailure = 468;
                public static int OfficialRecord_OnFailure2 = 472;
                public static int StartOfficialRecord_OnFailureImmediate = 476;
                public static int StartOfficialRecord_OnFailureImmediate2 = 480;
            }
        }

        public class CMwCmd : VT.CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Run(Game.CMwCmd thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Install(Game.CMwCmd thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Uninstall(Game.CMwCmd thisPtr);

            public Del_Run Run;
            public Del_Install Install;
            public Del_Uninstall Uninstall;

            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00BC646C;
                public static int Run = 120;
                public static int Install = 124;
                public static int Uninstall = 128;
            }
        }

        public class CMwCmdContainer : VT.CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Install(Game.CMwCmdContainer thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_Uninstall(Game.CMwCmdContainer thisPtr);

            public Del_Install Install;
            public Del_Uninstall Uninstall;

            public static class Offsets
            {
                public static IntPtr VTable = (IntPtr)0x00BCA0BC;
                public static int Install = 120;
                public static int Uninstall = 124;
            }
        }

        public class CAudioBufferKeeper : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A93C; } }
        public class CAudioMusic : VT.CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A4FC; } }
        public class CAudioPort : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A0DC; } }
        public class CAudioPortNull : VT.CAudioPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9CDCC; } }
        public class CAudioSound : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A364; } }
        public class CAudioSoundEngine : VT.CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A84C; } }
        public class CAudioSoundMulti : VT.CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A5AC; } }
        public class CAudioSoundSurface : VT.CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A784; } }
        public class CBlockVariable : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCADC4; } }
        public class CBoatParam : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3F1C; } }
        public class CBoatSail : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAAE1C; } }
        public class CBoatSailState : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAAB84; } }
        public class CBoatTeamActionDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA45D4; } }
        public class CBoatTeamDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA46BC; } }
        public class CBoatTeamMateActionDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA43F4; } }
        public class CBoatTeamMateLocationDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA450C; } }
        public class CControlBase : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92314; } }
        public class CControlButton : VT.CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93614; } }
        public class CControlColorChooser : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96324; } }
        public class CControlColorChooser2 : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97124; } }
        public class CControlContainer : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92834; } }
        public class CControlCredit : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B965F4; } }
        public class CControlCurve : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B989E4; } }
        public class CControlDisplayGraph : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9572C; } }
        public class CControlEffect : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B979F4; } }
        public class CControlEffectCombined : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99644; } }
        public class CControlEffectMaster : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B951BC; } }
        public class CControlEffectMotion : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99B9C; } }
        public class CControlEffectMoveFrame : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99524; } }
        public class CControlEffectSimi : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97454; } }
        public class CControlEffectSwitchStyle : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99D24; } }
        public class CControlEntry : VT.CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94594; } }
        public class CControlEnum : VT.CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94054; } }
        public class CControlField2 : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B95AA4; } }
        public class CControlForm : VT.CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9991C; } }
        public class CControlFrame : VT.CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92B3C; } }
        public class CControlFrameAnimated : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9537C; } }
        public class CControlFrameStyled : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B960AC; } }
        public class CControlGrid : VT.CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9328C; } }
        public class CControlIconIndex : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98804; } }
        public class CControlImage : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9835C; } }
        public class CControlLabel : VT.CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92DB4; } }
        public class CControlLayout : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97694; } }
        public class CControlList : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94DA4; } }
        public class CControlListItem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B942F4; } }
        public class CControlListMap : VT.CControlList { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98E14; } }
        public class CControlListMap2 : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9912C; } }
        public class CControlMediaItem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98264; } }
        public class CControlMediaPlayer : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97774; } }
        public class CControlOverlay : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98064; } }
        public class CControlPager : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96914; } }
        public class CControlQuad : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93004; } }
        public class CControlRadar : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B985AC; } }
        public class CControlSimi2 : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B975C4; } }
        public class CControlSlider : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97ADC; } }
        public class CControlStyle : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93ABC; } }
        public class CControlStyleSheet : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B943DC; } }
        public class CControlText : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94ABC; } }
        public class CControlTimeLine : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9971C; } }
        public class CControlTimeLine2 : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96CBC; } }
        public class CControlTrackManiaTeamCard : VT.CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B35364; } }
        public class CControlUiDockable : VT.CControlUiElement { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97D7C; } }
        public class CControlUiElement : VT.CControlForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99E1C; } }
        public class CControlUiRange : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9487C; } }
        public class CControlUrlLinks : VT.CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B95E0C; } }
        public class CCtnMediaBlockEventTrackMania : VT.CGameCtnMediaBlockEvent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3720C; } }
        public class CCtnMediaBlockUiTMSimpleEvtsDisplay : VT.CGameCtnMediaBlockUiSimpleEvtsDisplay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3567C; } }
        public class CCurveInterface : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CDx9DeviceCaps : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD2A1C; } }
        public class CFunc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F12C; } }
        public class CFuncClouds : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F42C; } }
        public class CFuncColor : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60174; } }
        public class CFuncColorGradient : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DD6C; } }
        public class CFuncCurves2Real : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F314; } }
        public class CFuncCurvesReal : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F5B4; } }
        public class CFuncEnum : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DBBC; } }
        public class CFuncEnvelope : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DE9C; } }
        public class CFuncFullColorGradient : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F674; } }
        public class CFuncGroup : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F9BC; } }
        public class CFuncGroupElem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FA6C; } }
        public class CFuncKeys : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F074; } }
        public class CFuncKeysCmd : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EF04; } }
        public class CFuncKeysNatural : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F8F4; } }
        public class CFuncKeysPath : VT.CFuncKeysTransQuat { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E64C; } }
        public class CFuncKeysReal : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D9DC; } }
        public class CFuncKeysReals : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FC44; } }
        public class CFuncKeysSkel : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EE34; } }
        public class CFuncKeysSound : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EFB4; } }
        public class CFuncKeysTrans : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EC0C; } }
        public class CFuncKeysTransQuat : VT.CFuncKeysTrans { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ECE4; } }
        public class CFuncKeysVisual : VT.CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60764; } }
        public class CFuncLight : VT.CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EB44; } }
        public class CFuncLightColor : VT.CFuncLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B600C4; } }
        public class CFuncLightIntensity : VT.CFuncLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B609AC; } }
        public class CFuncManagerCharacter : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E944; } }
        public class CFuncManagerCharacterAdv : VT.CFuncManagerCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FAFC; } }
        public class CFuncNoise : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F1E4; } }
        public class CFuncPathMesh : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E77C; } }
        public class CFuncPathMeshLocation : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FCFC; } }
        public class CFuncPlug : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DAC4; } }
        public class CFuncPuffLull : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DFBC; } }
        public class CFuncSegment : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C40DE4; } }
        public class CFuncShader : VT.CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60A74; } }
        public class CFuncShaderFxFactor : VT.CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6059C; } }
        public class CFuncShaderLayerUV : VT.CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60334; } }
        public class CFuncShaders : VT.CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6069C; } }
        public class CFuncShaderTweakKeysTranss : VT.CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F734; } }
        public class CFuncSin : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6081C; } }
        public class CFuncSkel : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CFBC; } }
        public class CFuncSkelValues : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EDA4; } }
        public class CFuncTree : VT.CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E9F4; } }
        public class CFuncTreeBend : VT.CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FDD4; } }
        public class CFuncTreeElevator : VT.CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F814; } }
        public class CFuncTreeRotate : VT.CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FE84; } }
        public class CFuncTreeSubVisualSequence : VT.CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E84C; } }
        public class CFuncTreeTranslate : VT.CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B608CC; } }
        public class CFuncVisual : VT.CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EA8C; } }
        public class CFuncVisualBlendShapeSequence : VT.CFuncVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FB9C; } }
        public class CFuncVisualShiver : VT.CFuncVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FF84; } }
        public class CFuncWeather : VT.CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E22C; } }
        public class CGameAdvertising : VT.CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83844; } }
        public class CGameAdvertisingElement : VT.CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C574; } }
        public class CGameAnalyzer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85BD4; } }
        public class CGameAvatar : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6531C; } }
        public class CGameBulletModel : VT.CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90F94; } }
        public class CGameCalendar : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameCalendarEvent : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B921B4; } }
        public class CGameCamera : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CB2C; } }
        public class CGameCampaignPlayerScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81D8C; } }
        public class CGameCampaignScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81E0C; } }
        public class CGameCampaignsScoresManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81F0C; } }
        public class CGameChallengeScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81E8C; } }
        public class CGameChampionship : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87AC4; } }
        public class CGameControlCamera : VT.CSceneController { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65614; } }
        public class CGameControlCameraEffect : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9136C; } }
        public class CGameControlCameraEffectAdaptativeNearZ : VT.CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89C7C; } }
        public class CGameControlCameraEffectGroup : VT.CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B870D4; } }
        public class CGameControlCameraEffectShake : VT.CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91A54; } }
        public class CGameControlCameraFollowAboveWater : VT.CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B503AC; } }
        public class CGameControlCameraFree : VT.CGameControlCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7C984; } }
        public class CGameControlCameraMaster : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CBEC; } }
        public class CGameControlCameraOrbital3d : VT.CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CE3C; } }
        public class CGameControlCameraTarget : VT.CGameControlCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B653D4; } }
        public class CGameControlCameraTrackManiaRace : VT.CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3735C; } }
        public class CGameControlCameraTrackManiaRace2 : VT.CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B36AF4; } }
        public class CGameControlCameraTrackManiaRace3 : VT.CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B361A4; } }
        public class CGameControlCard : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B61174; } }
        public class CGameControlCardCalendar : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8EC94; } }
        public class CGameControlCardCalendarEvent : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8EFB4; } }
        public class CGameControlCardChampionship : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8DAD4; } }
        public class CGameControlCardCtnArticle : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E6B4; } }
        public class CGameControlCardCtnCampaign : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E3C4; } }
        public class CGameControlCardCtnChallengeInfo : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6488C; } }
        public class CGameControlCardCtnChapter : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8DDC4; } }
        public class CGameControlCardCtnGhost : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E084; } }
        public class CGameControlCardCtnGhostInfo : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D7E4; } }
        public class CGameControlCardCtnNetServerInfo : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B727CC; } }
        public class CGameControlCardCtnReplayRecordInfo : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E974; } }
        public class CGameControlCardCtnVehicle : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D52C; } }
        public class CGameControlCardGeneric : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B738EC; } }
        public class CGameControlCardLadderRanking : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F364; } }
        public class CGameControlCardLeague : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8918C; } }
        public class CGameControlCardManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B638A4; } }
        public class CGameControlCardMessage : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B5FC; } }
        public class CGameControlCardNetOnlineEvent : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CF3C; } }
        public class CGameControlCardNetOnlineNews : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88B7C; } }
        public class CGameControlCardNetTeamInfo : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D264; } }
        public class CGameControlCardProfile : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8577C; } }
        public class CGameControlDataType : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8701C; } }
        public class CGameControlGrid : VT.CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7412C; } }
        public class CGameControlGridCard : VT.CGameControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B634EC; } }
        public class CGameControlGridCtnCampaign : VT.CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CC4C; } }
        public class CGameControlGridCtnChallengeGroup : VT.CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C8EC; } }
        public class CGameControlPlayer : VT.CGameRule { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85F44; } }
        public class CGameControlPlayerInput : VT.CGameControlPlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85E74; } }
        public class CGameControlPlayerNet : VT.CGameControlPlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90E84; } }
        public class CGameCtnArticle : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7DB24; } }
        public class CGameCtnBlock : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6950C; } }
        public class CGameCtnBlockInfo : VT.CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B78044; } }
        public class CGameCtnBlockInfoClassic : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FAFC; } }
        public class CGameCtnBlockInfoClip : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B772B4; } }
        public class CGameCtnBlockInfoFlat : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FD84; } }
        public class CGameCtnBlockInfoFrontier : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FC04; } }
        public class CGameCtnBlockInfoPylon : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B864EC; } }
        public class CGameCtnBlockInfoRectAsym : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84114; } }
        public class CGameCtnBlockInfoRoad : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83F54; } }
        public class CGameCtnBlockInfoSlope : VT.CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C684; } }
        public class CGameCtnBlockSkin : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83DC4; } }
        public class CGameCtnBlockUnit : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84524; } }
        public class CGameCtnBlockUnitInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B843C4; } }
        public class CGameCtnCampaign : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B63FCC; } }
        public class CGameCtnCatalog : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B614AC; } }
        public class CGameCtnChallenge : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B61AFC; } }
        public class CGameCtnChallengeGroup : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76834; } }
        public class CGameCtnChallengeInfo : VT.CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B64234; } }
        public class CGameCtnChallengeParameters : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65FCC; } }
        public class CGameCtnChapter : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76934; } }
        public class CGameCtnCollection : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69844; } }
        public class CGameCtnCollector : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8629C; } }
        public class CGameCtnCollectorList : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83E7C; } }
        public class CGameCtnCollectorVehicle : VT.CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7705C; } }
        public class CGameCtnCursor : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B77CDC; } }
        public class CGameCtnDecoration : VT.CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86384; } }
        public class CGameCtnDecorationAudio : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90AEC; } }
        public class CGameCtnDecorationMood : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90944; } }
        public class CGameCtnDecorationSize : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90834; } }
        public class CGameCtnDecorationTerrainModifier : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8888C; } }
        public class CGameCtnEdControlCam : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91C14; } }
        public class CGameCtnEdControlCamCustom : VT.CGameCtnEdControlCam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B234; } }
        public class CGameCtnEdControlCamPath : VT.CGameCtnEdControlCam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B15C; } }
        public class CGameCtnEditor : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B615E4; } }
        public class CGameCtnEditorScenePocLink : VT.CSceneObjectLink { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B77E24; } }
        public class CGameCtnGhost : VT.CGameGhost { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B660E4; } }
        public class CGameCtnGhostInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76F34; } }
        public class CGameCtnMasterServer : VT.CGameMasterServer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8181C; } }
        public class CGameCtnMediaBlock : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65034; } }
        public class CGameCtnMediaBlock3dStereo : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AD0C; } }
        public class CGameCtnMediaBlockCamera : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B40C; } }
        public class CGameCtnMediaBlockCameraCustom : VT.CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8994C; } }
        public class CGameCtnMediaBlockCameraEffect : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9191C; } }
        public class CGameCtnMediaBlockCameraEffectShake : VT.CGameCtnMediaBlockCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AB5C; } }
        public class CGameCtnMediaBlockCameraGame : VT.CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89764; } }
        public class CGameCtnMediaBlockCameraOrbital : VT.CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D46C; } }
        public class CGameCtnMediaBlockCameraPath : VT.CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89B0C; } }
        public class CGameCtnMediaBlockCameraSimple : VT.CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D2B4; } }
        public class CGameCtnMediaBlockEditor : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameCtnMediaBlockEditorTriangles : VT.CGameCtnMediaBlockEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90664; } }
        public class CGameCtnMediaBlockEvent : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6592C; } }
        public class CGameCtnMediaBlockFx : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AF14; } }
        public class CGameCtnMediaBlockFxBloom : VT.CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A7FC; } }
        public class CGameCtnMediaBlockFxBlur : VT.CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91804; } }
        public class CGameCtnMediaBlockFxBlurDepth : VT.CGameCtnMediaBlockFxBlur { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A65C; } }
        public class CGameCtnMediaBlockFxBlurMotion : VT.CGameCtnMediaBlockFxBlur { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F7AC; } }
        public class CGameCtnMediaBlockFxColors : VT.CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A4A4; } }
        public class CGameCtnMediaBlockGhost : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89DC4; } }
        public class CGameCtnMediaBlockImage : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87C2C; } }
        public class CGameCtnMediaBlockMusicEffect : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A9A4; } }
        public class CGameCtnMediaBlockSound : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B875AC; } }
        public class CGameCtnMediaBlockText : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89F94; } }
        public class CGameCtnMediaBlockTime : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A30C; } }
        public class CGameCtnMediaBlockTrails : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F65C; } }
        public class CGameCtnMediaBlockTransition : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B916EC; } }
        public class CGameCtnMediaBlockTransitionFade : VT.CGameCtnMediaBlockTransition { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A16C; } }
        public class CGameCtnMediaBlockTriangles : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91D1C; } }
        public class CGameCtnMediaBlockTriangles2D : VT.CGameCtnMediaBlockTriangles { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FE8C; } }
        public class CGameCtnMediaBlockTriangles3D : VT.CGameCtnMediaBlockTriangles { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90014; } }
        public class CGameCtnMediaBlockUi : VT.CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6519C; } }
        public class CGameCtnMediaBlockUiSimpleEvtsDisplay : VT.CGameCtnMediaBlockUi { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B64EE4; } }
        public class CGameCtnMediaClip : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7A98C; } }
        public class CGameCtnMediaClipGroup : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84E6C; } }
        public class CGameCtnMediaClipPlayer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CCF4; } }
        public class CGameCtnMediaClipViewer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7A854; } }
        public class CGameCtnMediaTrack : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D17C; } }
        public class CGameCtnMediaTracker : VT.CGameCtnEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B78BD4; } }
        public class CGameCtnMediaVideoParams : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B835CC; } }
        public class CGameCtnMenus : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6E11C; } }
        public class CGameCtnNetForm : VT.CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8520C; } }
        public class CGameCtnNetServerInfo : VT.CGameNetServerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84B7C; } }
        public class CGameCtnNetwork : VT.CGameNetwork { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7B81C; } }
        public class CGameCtnObjectInfo : VT.CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90BF4; } }
        public class CGameCtnPainter : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B80F54; } }
        public class CGameCtnPainterSetting : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BB5C; } }
        public class CGameCtnParticleParam : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90764; } }
        public class CGameCtnPylonColumn : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86704; } }
        public class CGameCtnReplayRecord : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65B4C; } }
        public class CGameCtnReplayRecordInfo : VT.CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76E54; } }
        public class CGameCtnZone : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8896C; } }
        public class CGameCtnZoneFlat : VT.CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88794; } }
        public class CGameCtnZoneFrontier : VT.CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F914; } }
        public class CGameCtnZoneTest : VT.CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FA04; } }
        public class CGameDialogs : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69234; } }
        public class CGameDialogShootVideo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B834FC; } }
        public class CGameEngine : VT.CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8613C; } }
        public class CGameEnvironmentManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86094; } }
        public class CGameFid : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84D8C; } }
        public class CGameGeneralScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81D0C; } }
        public class CGameGhost : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7AAA4; } }
        public class CGameHighScore : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84F44; } }
        public class CGameInterface : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87E0C; } }
        public class CGameLadderRanking : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8772C; } }
        public class CGameLadderRankingCtnChallengeAchievement : VT.CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BA54; } }
        public class CGameLadderRankingLeague : VT.CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B9A4; } }
        public class CGameLadderRankingPlayer : VT.CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B802CC; } }
        public class CGameLadderRankingSkill : VT.CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B8DC; } }
        public class CGameLadderScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B855A4; } }
        public class CGameLadderScoresComputer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85524; } }
        public class CGameLeague : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B73BDC; } }
        public class CGameLeagueManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B819A4; } }
        public class CGameLoadProgress : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8213C; } }
        public class CGameManialinkBrowser : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B73DAC; } }
        public class CGameManialinkEntry : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9142C; } }
        public class CGameManialinkFileEntry : VT.CGameManialinkEntry { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B914E4; } }
        public class CGameManiaNetResource : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87914; } }
        public class CGameMasterServer : VT.CNetMasterServer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6203C; } }
        public class CGameMenu : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76B74; } }
        public class CGameMenuColorEffect : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9118C; } }
        public class CGameMenuFrame : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8945C; } }
        public class CGameMenuScaleEffect : VT.CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90CEC; } }
        public class CGameMobil : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D0C4; } }
        public class CGameNetDataDownload : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B33C; } }
        public class CGameNetFileTransfer : VT.CNetFileTransfer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86E1C; } }
        public class CGameNetForm : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69C1C; } }
        public class CGameNetFormAdmin : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85374; } }
        public class CGameNetFormBuddy : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84FDC; } }
        public class CGameNetFormCallVote : VT.CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8514C; } }
        public class CGameNetFormGameSync : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8542C; } }
        public class CGameNetFormTimeSync : VT.CNetFormTimed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B852BC; } }
        public class CGameNetFormTunnel : VT.CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85094; } }
        public class CGameNetOnlineEvent : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86A8C; } }
        public class CGameNetOnlineMessage : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86994; } }
        public class CGameNetOnlineNews : VT.CGameNetOnlineEvent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86B84; } }
        public class CGameNetOnlineNewsReply : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86C0C; } }
        public class CGameNetPlayerInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8469C; } }
        public class CGameNetServerInfo : VT.CNetMasterHost { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8489C; } }
        public class CGameNetTeamInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8782C; } }
        public class CGameNetwork : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B62C64; } }
        public class CGameNod : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86004; } }
        public class CGameOutlineBox : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B773AC; } }
        public class CGamePlayer : VT.CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B664C4; } }
        public class CGamePlayerAttributesLiving : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CB6C; } }
        public class CGamePlayerCameraSet : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6658C; } }
        public class CGamePlayerInfo : VT.CGameNetPlayerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6447C; } }
        public class CGamePlayerOfficialScores : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81F8C; } }
        public class CGamePlayerProfile : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B639EC; } }
        public class CGamePlayerScore : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B63D14; } }
        public class CGamePlayerScoresShooter : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C7FC; } }
        public class CGamePlayground : VT.CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6664C; } }
        public class CGamePlaygroundInterface : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B663DC; } }
        public class CGamePopUp : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AE6C; } }
        public class CGameProcess : VT.CMwCmdContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B861DC; } }
        public class CGameRace : VT.CGamePlayground { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65D84; } }
        public class CGameRaceInterface : VT.CGamePlaygroundInterface { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameRemoteBuffer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86F14; } }
        public class CGameRemoteBufferDataInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B867CC; } }
        public class CGameRemoteBufferDataInfoFinds : VT.CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C184; } }
        public class CGameRemoteBufferDataInfoRankings : VT.CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BD8C; } }
        public class CGameRemoteBufferDataInfoSearchs : VT.CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BFA4; } }
        public class CGameRemoteBufferPool : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B868CC; } }
        public class CGameRule : VT.CGameProcess { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C75C; } }
        public class CGameSafeFrame : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B08C; } }
        public class CGameSafeFrameConfig : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91B6C; } }
        public class CGameScene : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6587C; } }
        public class CGameSkillScoreComputer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88564; } }
        public class CGameSkin : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83C8C; } }
        public class CGameSystemOverlay : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B72ACC; } }
        public class CGameTournament : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B879CC; } }
        public class CHdrComment : VT.CXmlComment { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5754; } }
        public class CHdrDeclaration : VT.CXmlDeclaration { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC57F4; } }
        public class CHdrDocument : VT.CXmlDocument { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5894; } }
        public class CHdrElement : VT.CXmlElement { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC568C; } }
        public class CHdrText : VT.CXmlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC554C; } }
        public class CHdrUnknown : VT.CXmlUnknown { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC55EC; } }
        public class CHmsAmbientOcc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58524; } }
        public class CHmsCamera : VT.CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5655C; } }
        public class CHmsCollisionManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B55DAC; } }
        public class CHmsConfig : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57684; } }
        public class CHmsCorpus : VT.CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56E2C; } }
        public class CHmsCorpus2d : VT.CHmsCorpus { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B595B4; } }
        public class CHmsCorpusLight : VT.CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58EF4; } }
        public class CHmsFogPlane : VT.CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5965C; } }
        public class CHmsForceField : VT.CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59494; } }
        public class CHmsForceFieldBall : VT.CHmsForceField { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B597D4; } }
        public class CHmsForceFieldUniform : VT.CHmsForceField { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B596FC; } }
        public class CHmsItem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56064; } }
        public class CHmsItemShadow : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5872C; } }
        public class CHmsLight : VT.CHmsPocEmitter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B588F4; } }
        public class CHmsListener : VT.CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59A3C; } }
        public class CHmsPackLightMap : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B568DC; } }
        public class CHmsPackLightMapAlloc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58E4C; } }
        public class CHmsPackLightMapCache : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58A2C; } }
        public class CHmsPackLightMapMood : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58DAC; } }
        public class CHmsPicker : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58604; } }
        public class CHmsPoc : VT.CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58804; } }
        public class CHmsPocEmitter : VT.CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B572F4; } }
        public class CHmsPortal : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57484; } }
        public class CHmsPortalProperty : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57274; } }
        public class CHmsPrecalcRender : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B583FC; } }
        public class CHmsShadowGroup : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59074; } }
        public class CHmsSoundSource : VT.CHmsPocEmitter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B598DC; } }
        public class CHmsViewport : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5574C; } }
        public class CHmsZone : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5795C; } }
        public class CHmsZoneDynamic : VT.CHmsZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56FBC; } }
        public class CHmsZoneElem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B593E4; } }
        public class CHmsZoneOverlay : VT.CHmsZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58224; } }
        public class CInputBindingsConfig : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEA74; } }
        public class CInputDevice : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF004; } }
        public class CInputDeviceDx8Keyboard : VT.CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF3CC; } }
        public class CInputDeviceDx8Mouse : VT.CInputDeviceMouse { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF304; } }
        public class CInputDeviceDx8Pad : VT.CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF24C; } }
        public class CInputDeviceMouse : VT.CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF55C; } }
        public class CInputPort : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEB94; } }
        public class CInputPortDx8 : VT.CInputPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEE84; } }
        public class CInputPortNull : VT.CInputPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF48C; } }
        public class CManoeuvre : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB33C; } }
        public class CMotion : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AC1C; } }
        public class CMotionBone : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C264; } }
        public class CMotionCmdBase : VT.CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59EBC; } }
        public class CMotionCmdBaseParams : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B08C; } }
        public class CMotionDayTime : VT.CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B3CC; } }
        public class CMotionEmitterLeaves : VT.CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C134; } }
        public class CMotionEmitterParticles : VT.CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59CEC; } }
        public class CMotionFunc : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D3EC; } }
        public class CMotionGroupPlayers : VT.CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C344; } }
        public class CMotionLight : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CC44; } }
        public class CMotionManaged : VT.CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ACF4; } }
        public class CMotionManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AB6C; } }
        public class CMotionManagerCharacter : VT.CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C78C; } }
        public class CMotionManagerCharacterAdv : VT.CMotionManagerCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C464; } }
        public class CMotionManagerLeaves : VT.CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5BF8C; } }
        public class CMotionManagerMeteo : VT.CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B924; } }
        public class CMotionManagerMeteoPuffLull : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B304; } }
        public class CMotionManagerParticles : VT.CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59BDC; } }
        public class CMotionManagerWeathers : VT.CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B5FC; } }
        public class CMotionParticleEmitterModel : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ADE4; } }
        public class CMotionParticleType : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5A4D4; } }
        public class CMotionPath : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D164; } }
        public class CMotionPlay : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D7DC; } }
        public class CMotionPlayCmd : VT.CMotionPlay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D05C; } }
        public class CMotionPlayer : VT.CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AEEC; } }
        public class CMotionPlaySound : VT.CMotionPlay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D304; } }
        public class CMotionPlaySoundMobil : VT.CMotionPlaySound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D22C; } }
        public class CMotions : VT.CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D4AC; } }
        public class CMotionShader : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B204; } }
        public class CMotionSkel : VT.CMotionSkelSimple { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CE3C; } }
        public class CMotionSkelSimple : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CF14; } }
        public class CMotionTeamAction : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D684; } }
        public class CMotionTeamActionInfo : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D8BC; } }
        public class CMotionTeamManager : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C654; } }
        public class CMotionTimerLoop : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D5C4; } }
        public class CMotionTrack : VT.CMwCmdContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B144; } }
        public class CMotionTrackMobilMove : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CD1C; } }
        public class CMotionTrackMobilPitchin : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CA3C; } }
        public class CMotionTrackMobilRotate : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59FDC; } }
        public class CMotionTrackMobilScale : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C05C; } }
        public class CMotionTrackTree : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C8F4; } }
        public class CMotionTrackVisual : VT.CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CB6C; } }
        public class CMotionWeather : VT.CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B73C; } }
        public class CMotionWindBlocker : VT.CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5BB94; } }
        public class CMwClassInfoViewer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0614; } }
        public class CMwCmdAffectIdent : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD054C; } }
        public class CMwCmdAffectIdentBool : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD7EC; } }
        public class CMwCmdAffectIdentClass : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD8AC; } }
        public class CMwCmdAffectIdentEnum : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB3A4; } }
        public class CMwCmdAffectIdentIso4 : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD684; } }
        public class CMwCmdAffectIdentNum : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD504; } }
        public class CMwCmdAffectIdentString : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD5C4; } }
        public class CMwCmdAffectIdentVec2 : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD444; } }
        public class CMwCmdAffectIdentVec3 : VT.CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD384; } }
        public class CMwCmdAffectParam : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0484; } }
        public class CMwCmdAffectParamBool : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD2C4; } }
        public class CMwCmdAffectParamClass : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD084; } }
        public class CMwCmdAffectParamEnum : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD144; } }
        public class CMwCmdAffectParamIso4 : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD204; } }
        public class CMwCmdAffectParamNum : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCFC4; } }
        public class CMwCmdAffectParamString : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCE44; } }
        public class CMwCmdAffectParamVec2 : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCF04; } }
        public class CMwCmdAffectParamVec3 : VT.CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCD84; } }
        public class CMwCmdArrayAdd : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB534; } }
        public class CMwCmdArrayRemove : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB5EC; } }
        public class CMwCmdBlock : VT.CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCACEC; } }
        public class CMwCmdBlockCast : VT.CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDEF4; } }
        public class CMwCmdBlockFunction : VT.CMwCmdBlockProcedure { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBD04; } }
        public class CMwCmdBlockMain : VT.CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9E0C; } }
        public class CMwCmdBlockProcedure : VT.CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAE54; } }
        public class CMwCmdBreak : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBE84; } }
        public class CMwCmdBuffer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA5DC; } }
        public class CMwCmdBufferCore : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5FA4; } }
        public class CMwCmdCall : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD03D4; } }
        public class CMwCmdContinue : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBDCC; } }
        public class CMwCmdExp : VT.CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAC1C; } }
        public class CMwCmdExpAdd : VT.CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD00A4; } }
        public class CMwCmdExpAnd : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFF0C; } }
        public class CMwCmdExpBool : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFFDC; } }
        public class CMwCmdExpBoolBin : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEB3C; } }
        public class CMwCmdExpBoolFunction : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBB84; } }
        public class CMwCmdExpBoolIdent : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFE34; } }
        public class CMwCmdExpBoolParam : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFD64; } }
        public class CMwCmdExpClass : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9D14; } }
        public class CMwCmdExpClassFunction : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB9E4; } }
        public class CMwCmdExpClassIdent : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEA64; } }
        public class CMwCmdExpClassParam : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE8CC; } }
        public class CMwCmdExpClassThis : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB6A4; } }
        public class CMwCmdExpDiff : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFC9C; } }
        public class CMwCmdExpDiv : VT.CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFAFC; } }
        public class CMwCmdExpEgal : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFBD4; } }
        public class CMwCmdExpEnum : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE99C; } }
        public class CMwCmdExpEnumCastedNum : VT.CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAFBC; } }
        public class CMwCmdExpEnumIdent : VT.CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB2D4; } }
        public class CMwCmdExpEnumParam : VT.CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE7FC; } }
        public class CMwCmdExpInf : VT.CMwCmdExpBoolBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFA2C; } }
        public class CMwCmdExpInfEgal : VT.CMwCmdExpBoolBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF95C; } }
        public class CMwCmdExpIso4 : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE22C; } }
        public class CMwCmdExpIso4Function : VT.CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBAB4; } }
        public class CMwCmdExpIso4Ident : VT.CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE08C; } }
        public class CMwCmdExpIso4Inverse : VT.CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC18C; } }
        public class CMwCmdExpIso4Mult : VT.CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC25C; } }
        public class CMwCmdExpIso4Param : VT.CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDFBC; } }
        public class CMwCmdExpMult : VT.CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF88C; } }
        public class CMwCmdExpNeg : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF6F4; } }
        public class CMwCmdExpNot : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF7C4; } }
        public class CMwCmdExpNum : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF624; } }
        public class CMwCmdExpNumBin : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCED2C; } }
        public class CMwCmdExpNumCastedEnum : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB08C; } }
        public class CMwCmdExpNumDotProduct2 : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBFEC; } }
        public class CMwCmdExpNumDotProduct3 : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC0BC; } }
        public class CMwCmdExpNumFunction : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEC1C; } }
        public class CMwCmdExpNumIdent : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF54C; } }
        public class CMwCmdExpNumParam : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF47C; } }
        public class CMwCmdExpOr : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF214; } }
        public class CMwCmdExpPower : VT.CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF2DC; } }
        public class CMwCmdExpString : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF074; } }
        public class CMwCmdExpStringConcat : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD06AC; } }
        public class CMwCmdExpStringFunction : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB914; } }
        public class CMwCmdExpStringIdent : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEECC; } }
        public class CMwCmdExpStringParam : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEDFC; } }
        public class CMwCmdExpStringTrunc : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB464; } }
        public class CMwCmdExpStringUpDownCase : VT.CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB15C; } }
        public class CMwCmdExpSub : VT.CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF3AC; } }
        public class CMwCmdExpSup : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF13C; } }
        public class CMwCmdExpSupEgal : VT.CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEF9C; } }
        public class CMwCmdExpVec2 : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE664; } }
        public class CMwCmdExpVec2Add : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCBFC; } }
        public class CMwCmdExpVec2Function : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB844; } }
        public class CMwCmdExpVec2Ident : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE3C4; } }
        public class CMwCmdExpVec2Mult : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC8DC; } }
        public class CMwCmdExpVec2Neg : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC80C; } }
        public class CMwCmdExpVec2Param : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE494; } }
        public class CMwCmdExpVec2Sub : VT.CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC9AC; } }
        public class CMwCmdExpVec3 : VT.CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE574; } }
        public class CMwCmdExpVec3Add : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC73C; } }
        public class CMwCmdExpVec3Function : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB774; } }
        public class CMwCmdExpVec3Ident : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE2F4; } }
        public class CMwCmdExpVec3Mult : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC3FC; } }
        public class CMwCmdExpVec3MultIso : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC4CC; } }
        public class CMwCmdExpVec3Neg : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC59C; } }
        public class CMwCmdExpVec3Param : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE15C; } }
        public class CMwCmdExpVec3Product : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC32C; } }
        public class CMwCmdExpVec3Sub : VT.CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC66C; } }
        public class CMwCmdFastCall : VT.CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC6334; } }
        public class CMwCmdFastCallUser : VT.CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC63BC; } }
        public class CMwCmdFiber : VT.CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA154; } }
        public class CMwCmdFor : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0184; } }
        public class CMwCmdIf : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0254; } }
        public class CMwCmdInst : VT.CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9C64; } }
        public class CMwCmdLog : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCB4C; } }
        public class CMwCmdProc : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBC54; } }
        public class CMwCmdReturn : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBF34; } }
        public class CMwCmdScript : VT.CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9F6C; } }
        public class CMwCmdScriptVar : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAF24; } }
        public class CMwCmdScriptVarBool : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDD74; } }
        public class CMwCmdScriptVarClass : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDCC4; } }
        public class CMwCmdScriptVarEnum : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB23C; } }
        public class CMwCmdScriptVarFloat : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDB6C; } }
        public class CMwCmdScriptVarInt : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDC1C; } }
        public class CMwCmdScriptVarIso4 : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDAC4; } }
        public class CMwCmdScriptVarString : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDA1C; } }
        public class CMwCmdScriptVarVec2 : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD974; } }
        public class CMwCmdScriptVarVec3 : VT.CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD74C; } }
        public class CMwCmdSleep : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCCD4; } }
        public class CMwCmdSwitch : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE73C; } }
        public class CMwCmdSwitchType : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDE1C; } }
        public class CMwCmdWait : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCA84; } }
        public class CMwCmdWhile : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0324; } }
        public class CMwEngine : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC628C; } }
        public class CMwParamAction : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8944; } }
        public class CMwParamBool : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8A0C; } }
        public class CMwParamClass : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8AD4; } }
        public class CMwParamColor : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8D2C; } }
        public class CMwParamEnum : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8DF4; } }
        public class CMwParamInteger : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC904C; } }
        public class CMwParamIntegerRange : VT.CMwParamInteger { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9114; } }
        public class CMwParamIso3 : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC968C; } }
        public class CMwParamIso4 : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC95C4; } }
        public class CMwParamMwId : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC94FC; } }
        public class CMwParamNatural : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8EBC; } }
        public class CMwParamNaturalRange : VT.CMwParamNatural { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8F84; } }
        public class CMwParamProc : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9A74; } }
        public class CMwParamQuat : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC99AC; } }
        public class CMwParamReal : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC91DC; } }
        public class CMwParamRealRange : VT.CMwParamReal { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC92A4; } }
        public class CMwParamRefBuffer : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8B9C; } }
        public class CMwParamString : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC936C; } }
        public class CMwParamStringInt : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9434; } }
        public class CMwParamStruct : VT.CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8C64; } }
        public class CMwParamVec2 : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC98E4; } }
        public class CMwParamVec3 : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC981C; } }
        public class CMwParamVec4 : VT.CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9754; } }
        public class CMwRefBuffer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA484; } }
        public class CMwStatsValue : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA24C; } }
        public class CNetClient : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5441C; } }
        public class CNetClientInfo : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B539D4; } }
        public class CNetConnection : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53B34; } }
        public class CNetFileTransfer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53104; } }
        public class CNetFileTransferDownload : VT.CNetFileTransferNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5244C; } }
        public class CNetFileTransferForm : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5273C; } }
        public class CNetFileTransferNod : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53514; } }
        public class CNetFileTransferUpload : VT.CNetFileTransferNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53FA4; } }
        public class CNetFormConnectionAdmin : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5281C; } }
        public class CNetFormEnumSessions : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B528EC; } }
        public class CNetFormPing : VT.CNetFormTimed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B48B24; } }
        public class CNetFormQuerrySessions : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B529A4; } }
        public class CNetFormRpcCall : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5268C; } }
        public class CNetFormTimed : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B48A6C; } }
        public class CNetHttpClient : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52A9C; } }
        public class CNetHttpResult : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B516FC; } }
        public class CNetIPC : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C40654; } }
        public class CNetIPSource : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52DBC; } }
        public class CNetMasterHost : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5220C; } }
        public class CNetMasterServer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B51C6C; } }
        public class CNetMasterServerInfo : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C407EC; } }
        public class CNetMasterServerUptoDateCheck : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52BF4; } }
        public class CNetNod : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5152C; } }
        public class CNetServer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5479C; } }
        public class CNetServerInfo : VT.CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B522B4; } }
        public class CNetSource : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B537FC; } }
        public class CNetUPnP : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C408A4; } }
        public class CNetURLSource : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53624; } }
        public class CNodSystem : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31C14; } }
        public class COalAudioBufferKeeper : VT.CAudioBufferKeeper { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9CFD4; } }
        public class COalAudioPort : VT.CAudioPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9B13C; } }
        public class COalDevice : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9AF44; } }
        public class CPlug : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE7FC; } }
        public class CPlugAudio : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD304; } }
        public class CPlugAudioEnvironment : VT.CPlugAudio { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB24F4; } }
        public class CPlugBitmap : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BACF14; } }
        public class CPlugBitmapAddress : VT.CPlugBitmapSampler { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB48B4; } }
        public class CPlugBitmapApply : VT.CPlugBitmapAddress { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC8A4; } }
        public class CPlugBitmapHighLevel : VT.CSystemNodWrapper { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBCB6C; } }
        public class CPlugBitmapPack : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBA84; } }
        public class CPlugBitmapPackElem : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB7CC; } }
        public class CPlugBitmapPacker : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBDD4; } }
        public class CPlugBitmapPackInput : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB704; } }
        public class CPlugBitmapRender : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB054C; } }
        public class CPlugBitmapRenderCamera : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADF8C; } }
        public class CPlugBitmapRenderCubeMap : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB884C; } }
        public class CPlugBitmapRenderHemisphere : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB2E4; } }
        public class CPlugBitmapRenderLightFromMap : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5C9C; } }
        public class CPlugBitmapRenderLightOcc : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB78B4; } }
        public class CPlugBitmapRenderOverlay : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB976C; } }
        public class CPlugBitmapRenderPlaneR : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAF54; } }
        public class CPlugBitmapRenderPortal : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAE54; } }
        public class CPlugBitmapRenderScene3d : VT.CPlugBitmapRenderCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA4B4; } }
        public class CPlugBitmapRenderShadow : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB0294; } }
        public class CPlugBitmapRenderSolid : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB01B4; } }
        public class CPlugBitmapRenderSub : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA244; } }
        public class CPlugBitmapRenderVDepPlaneY : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA40C; } }
        public class CPlugBitmapRenderWater : VT.CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB85EC; } }
        public class CPlugBitmapSampler : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE65C; } }
        public class CPlugBitmapShader : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB848C; } }
        public class CPlugBlendShapes : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB474C; } }
        public class CPlugCrystal : VT.CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBCC94; } }
        public class CPlugDecoratorSolid : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4BC4; } }
        public class CPlugDecoratorTree : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9614; } }
        public class CPlugFile : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC2AC; } }
        public class CPlugFileAvi : VT.CPlugFileVideo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBFFC; } }
        public class CPlugFileBink : VT.CPlugFileVideo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC54C; } }
        public class CPlugFileDds : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFE5C; } }
        public class CPlugFileFidCache : VT.CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4D44; } }
        public class CPlugFileFidContainer : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAEE44; } }
        public class CPlugFileFont : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC0D4; } }
        public class CPlugFileGen : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFD84; } }
        public class CPlugFileGPU : VT.CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB45D4; } }
        public class CPlugFileGpuFx : VT.CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD564; } }
        public class CPlugFileGpuFxD3d : VT.CPlugFileGpuFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA184; } }
        public class CPlugFileGPUP : VT.CPlugFileGPU { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB218C; } }
        public class CPlugFileGPUV : VT.CPlugFileGPU { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB099C; } }
        public class CPlugFileI18n : VT.CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB3C4; } }
        public class CPlugFileImg : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAD95C; } }
        public class CPlugFileJpg : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4AD4; } }
        public class CPlugFileModel : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD43C; } }
        public class CPlugFileModel3ds : VT.CPlugFileModel { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9C9C; } }
        public class CPlugFileModelObj : VT.CPlugFileModel { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9E34; } }
        public class CPlugFileOggVorbis : VT.CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB0E4; } }
        public class CPlugFilePack : VT.CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAECF4; } }
        public class CPlugFilePHlsl : VT.CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB605C; } }
        public class CPlugFilePng : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBB6C; } }
        public class CPlugFilePsh : VT.CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB50EC; } }
        public class CPlugFilePso : VT.CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9D5C; } }
        public class CPlugFileSnd : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2E34; } }
        public class CPlugFileSndGen : VT.CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB58F4; } }
        public class CPlugFileText : VT.CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4C84; } }
        public class CPlugFileTga : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB535C; } }
        public class CPlugFileVHlsl : VT.CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB105C; } }
        public class CPlugFileVideo : VT.CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5734; } }
        public class CPlugFileVsh : VT.CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB8A4; } }
        public class CPlugFileVso : VT.CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA0B4; } }
        public class CPlugFileWav : VT.CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5A7C; } }
        public class CPlugFileZip : VT.CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC73C; } }
        public class CPlugFont : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADB8C; } }
        public class CPlugFontBitmap : VT.CPlugFont { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB584; } }
        public class CPlugFurWind : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF04C; } }
        public class CPlugIndexBuffer : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5844; } }
        public class CPlugLight : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4E6C; } }
        public class CPlugMaterial : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE504; } }
        public class CPlugMaterialCustom : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFADC; } }
        public class CPlugMaterialFx : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD394; } }
        public class CPlugMaterialFxDynaBump : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA64C; } }
        public class CPlugMaterialFxDynaMobil : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA584; } }
        public class CPlugMaterialFxFlags : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA72C; } }
        public class CPlugMaterialFxFur : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA834; } }
        public class CPlugMaterialFxGenCV : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA314; } }
        public class CPlugMaterialFxGenUvProj : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA4D4; } }
        public class CPlugMaterialFxs : VT.CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8EFC; } }
        public class CPlugModel : VT.CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7D54; } }
        public class CPlugModelFences : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9304; } }
        public class CPlugModelFur : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9944; } }
        public class CPlugModelLodMesh : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB801C; } }
        public class CPlugModelMesh : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB826C; } }
        public class CPlugModelShell : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB01C; } }
        public class CPlugModelTree : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7E8C; } }
        public class CPlugMusic : VT.CPlugMusicType { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE1CC; } }
        public class CPlugMusicType : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8E54; } }
        public class CPlugPointsInSphereOpt : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB0094; } }
        public class CPlugRessourceStrings : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB484; } }
        public class CPlugShader : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF704; } }
        public class CPlugShaderApply : VT.CPlugShaderGeneric { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF1BC; } }
        public class CPlugShaderGeneric : VT.CPlugShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE314; } }
        public class CPlugShaderPass : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBABF4; } }
        public class CPlugShaderSprite : VT.CPlugShaderApply { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAA5C; } }
        public class CPlugShaderSpritePath : VT.CPlugShaderSprite { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA95C; } }
        public class CPlugSolid : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADD6C; } }
        public class CPlugSound : VT.CPlugAudio { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2324; } }
        public class CPlugSoundEngine : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB59E4; } }
        public class CPlugSoundEngineComponent : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5B8C; } }
        public class CPlugSoundMood : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC314; } }
        public class CPlugSoundMulti : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBACD4; } }
        public class CPlugSoundSurface : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAD9C; } }
        public class CPlugSoundVideo : VT.CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4A34; } }
        public class CPlugSurface : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8CC4; } }
        public class CPlugSurfaceGeom : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8A44; } }
        public class CPlugTree : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAD5FC; } }
        public class CPlugTreeFrustum : VT.CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7A34; } }
        public class CPlugTreeGenerator : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8D7C; } }
        public class CPlugTreeGenSolid : VT.CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9F3C; } }
        public class CPlugTreeGenText : VT.CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE104; } }
        public class CPlugTreeLight : VT.CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2A7C; } }
        public class CPlugTreeViewDep : VT.CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2974; } }
        public class CPlugTreeVisualMip : VT.CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF38C; } }
        public class CPlugVertexStream : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB20DC; } }
        public class CPlugViewDepLocator : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9484; } }
        public class CPlugVisual : VT.CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE87C; } }
        public class CPlugVisual2D : VT.CPlugVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9134; } }
        public class CPlugVisual3D : VT.CPlugVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2BEC; } }
        public class CPlugVisualGrid : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7B8C; } }
        public class CPlugVisualHeightField : VT.CPlugVisualGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC194; } }
        public class CPlugVisualIndexed : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8B4C; } }
        public class CPlugVisualIndexedLines : VT.CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2804; } }
        public class CPlugVisualIndexedStrip : VT.CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8FBC; } }
        public class CPlugVisualIndexedTriangles : VT.CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB1CE4; } }
        public class CPlugVisualIndexedTriangles2D : VT.CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC9AC; } }
        public class CPlugVisualLines : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4F7C; } }
        public class CPlugVisualLines2D : VT.CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5444; } }
        public class CPlugVisualOctree : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB276C; } }
        public class CPlugVisualPath : VT.CPlugVisualGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBC24; } }
        public class CPlugVisualQuads : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB25DC; } }
        public class CPlugVisualQuads2D : VT.CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAEA7C; } }
        public class CPlugVisualSprite : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB1E9C; } }
        public class CPlugVisualStrip : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC3D4; } }
        public class CPlugVisualTriangles : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB559C; } }
        public class CPlugVisualVertexs : VT.CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC544; } }
        public class CScene : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9ECDC; } }
        public class CScene2d : VT.CScene { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E424; } }
        public class CScene3d : VT.CScene { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EAC4; } }
        public class CSceneCamera : VT.CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F4DC; } }
        public class CSceneConfig : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1984; } }
        public class CSceneConfigVision : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8B74; } }
        public class CSceneController : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0ECC; } }
        public class CSceneEngine : VT.CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F63C; } }
        public class CSceneExtraFlocking : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7224; } }
        public class CSceneExtraFlockingCharacters : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA748C; } }
        public class CSceneField : VT.CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1A84; } }
        public class CSceneFx : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA273C; } }
        public class CSceneFxBloom : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA90E4; } }
        public class CSceneFxBloomData : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8EF4; } }
        public class CSceneFxCameraBlend : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA52BC; } }
        public class CSceneFxColors : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1CD4; } }
        public class CSceneFxCompo : VT.CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA77C; } }
        public class CSceneFxDepthOfField : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5F04; } }
        public class CSceneFxDistor2d : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F714; } }
        public class CSceneFxFlares : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA9B64; } }
        public class CSceneFxGrayAccum : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA53F4; } }
        public class CSceneFxHeadTrack : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8A94; } }
        public class CSceneFxMotionBlur : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA58BC; } }
        public class CSceneFxNod : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EBDC; } }
        public class CSceneFxOccZCmp : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4FE4; } }
        public class CSceneFxOverlay : VT.CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5D74; } }
        public class CSceneFxStereoscopy : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA09CC; } }
        public class CSceneFxSuperSample : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA246C; } }
        public class CSceneFxToneMapping : VT.CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8C14; } }
        public class CSceneFxVisionK : VT.CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA64D4; } }
        public class CSceneGate : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2D44; } }
        public class CSceneLight : VT.CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FA7C; } }
        public class CSceneListener : VT.CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1894; } }
        public class CSceneLocation : VT.CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA321C; } }
        public class CSceneLocationCamera : VT.CSceneLocation { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA9EF4; } }
        public class CSceneMessageHandler : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2A6C; } }
        public class CSceneMobil : VT.CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E5D4; } }
        public class CSceneMobilClouds : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FBB4; } }
        public class CSceneMobilFlockAttractor : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7664; } }
        public class CSceneMobilLeaves : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA04B4; } }
        public class CSceneMobilSnow : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FE84; } }
        public class CSceneMobilTraffic : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2E4C; } }
        public class CSceneMood : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA30E4; } }
        public class CSceneMoods : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA84FC; } }
        public class CSceneMotorbikeEnvMaterial : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB4D4; } }
        public class CSceneObject : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E814; } }
        public class CSceneObjectLink : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E944; } }
        public class CScenePath : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F9E4; } }
        public class CScenePickerManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0834; } }
        public class CScenePoc : VT.CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0FAC; } }
        public class CSceneSector : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA29B4; } }
        public class CSceneSoundManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3314; } }
        public class CSceneSoundSource : VT.CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EDD4; } }
        public class CSceneToy : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA27F4; } }
        public class CSceneToyBird : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6D44; } }
        public class CSceneToyBoat : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1464; } }
        public class CSceneToyBroomstick : VT.CSceneToyCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA154; } }
        public class CSceneToyCharacter : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3B4C; } }
        public class CSceneToyCharacterDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA634; } }
        public class CSceneToyCharacterTuning : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA51A4; } }
        public class CSceneToyCharacterTunings : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5234; } }
        public class CSceneToyDisplayGraph : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA872C; } }
        public class CSceneToyDisplayHistogram : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA858C; } }
        public class CSceneToyDisplayProgress : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA88D4; } }
        public class CSceneToyFilaments : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA784C; } }
        public class CSceneToyLeash : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6724; } }
        public class CSceneToyMotorbike : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7EDC; } }
        public class CSceneToyRock : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA99AC; } }
        public class CSceneToySea : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0114; } }
        public class CSceneToySeaHoule : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA9AC; } }
        public class CSceneToySeaHouleFixe : VT.CSceneToySeaHoule { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB5C4; } }
        public class CSceneToySeaHouleTable : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA884; } }
        public class CSceneToyStem : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA481C; } }
        public class CSceneToySubway : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6934; } }
        public class CSceneToyTrain : VT.CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4B54; } }
        public class CSceneTrafficGraph : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA317C; } }
        public class CSceneTrafficPath : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA301C; } }
        public class CSceneVehicle : VT.CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F314; } }
        public class CSceneVehicleBall : VT.CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7A54; } }
        public class CSceneVehicleBallTuning : VT.CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA3E4; } }
        public class CSceneVehicleCar : VT.CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EFFC; } }
        public class CSceneVehicleCarTuning : VT.CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA376C; } }
        public class CSceneVehicleEmitter : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA6F4; } }
        public class CSceneVehicleEnvironment : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3A6C; } }
        public class CSceneVehicleGlider : VT.CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4DE4; } }
        public class CSceneVehicleGliderTuning : VT.CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8FC4; } }
        public class CSceneVehicleMaterial : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA39DC; } }
        public class CSceneVehicleMaterialGroup : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA59C; } }
        public class CSceneVehicleSpeedBoat : VT.CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6FD4; } }
        public class CSceneVehicleSpeedBoatTuning : VT.CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA07C; } }
        public class CSceneVehicleStruct : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3884; } }
        public class CSceneVehicleTuning : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB434; } }
        public class CSceneVehicleTunings : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3924; } }
        public class CSystemCmdAssert : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B318AC; } }
        public class CSystemCmdDuplicateNod : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31A1C; } }
        public class CSystemCmdExec : VT.CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31964; } }
        public class CSystemCmdLoadNod : VT.CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31B1C; } }
        public class CSystemConfig : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2F76C; } }
        public class CSystemConfigDisplay : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31324; } }
        public class CSystemData : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C404B4; } }
        public class CSystemEngine : VT.CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2E60C; } }
        public class CSystemFid : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2FCFC; } }
        public class CSystemFidBuffer : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31754; } }
        public class CSystemFidFile : VT.CSystemFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B301BC; } }
        public class CSystemFidMemory : VT.CSystemFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B300BC; } }
        public class CSystemFids : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B30544; } }
        public class CSystemFidsDrive : VT.CSystemFidsFolder { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3067C; } }
        public class CSystemFidsFolder : VT.CSystemFids { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2FFB4; } }
        public class CSystemKeyboard : VT.CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B321B4; } }
        public class CSystemMouse : VT.CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31DDC; } }
        public class CSystemNodWrapper : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3180C; } }
        public class CSystemPackDesc : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B32EE4; } }
        public class CSystemPackManager : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B32E0C; } }
        public class CSystemWindow : VT.CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31CCC; } }
        public class CTrackManiaControlCard : VT.CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3503C; } }
        public class CTrackManiaControlCheckPointList : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B378EC; } }
        public class CTrackManiaControlMatchSettingsCard : VT.CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33154; } }
        public class CTrackManiaControlPlayerInfoCard : VT.CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B35CAC; } }
        public class CTrackManiaControlPlayerInput : VT.CGameControlPlayerInput { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50154; } }
        public class CTrackManiaControlRaceScoreCard : VT.CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B349F4; } }
        public class CTrackManiaControlScores : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33FFC; } }
        public class CTrackManiaControlScores2 : VT.CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33814; } }
        public class CTrackManiaEditor : VT.CGameCtnEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4114C; } }
        public class CTrackManiaEditorCatalog : VT.CTrackManiaEditorFree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B38734; } }
        public class CTrackManiaEditorFree : VT.CTrackManiaEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CD94; } }
        public class CTrackManiaEditorIcon : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50E94; } }
        public class CTrackManiaEditorIconPage : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50AF4; } }
        public class CTrackManiaEditorInterface : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B47EF4; } }
        public class CTrackManiaEditorPuzzle : VT.CTrackManiaEditorFree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3C86C; } }
        public class CTrackManiaEditorSimple : VT.CTrackManiaEditorPuzzle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3348C; } }
        public class CTrackManiaEditorTerrain : VT.CTrackManiaEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CBF4; } }
        public class CTrackManiaEnvironmentManager : VT.CGameEnvironmentManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5095C; } }
        public class CTrackManiaMatchSettings : VT.CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B42DC4; } }
        public class CTrackManiaMatchSettingsControlGrid : VT.CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B34CEC; } }
        public class CTrackManiaMenus : VT.CGameCtnMenus { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3A64C; } }
        public class CTrackManiaNetForm : VT.CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B388EC; } }
        public class CTrackManiaNetwork : VT.CGameCtnNetwork { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4935C; } }
        public class CTrackManiaNetworkServerInfo : VT.CGameCtnNetServerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4231C; } }
        public class CTrackManiaPlayer : VT.CGamePlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5021C; } }
        public class CTrackManiaPlayerCameraSet : VT.CGamePlayerCameraSet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B504C4; } }
        public class CTrackManiaPlayerInfo : VT.CGamePlayerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B41FC4; } }
        public class CTrackManiaPlayerProfile : VT.CGamePlayerProfile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B346DC; } }
        public class CTrackManiaRace : VT.CGameRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3D0D4; } }
        public class CTrackManiaRace1P : VT.CTrackManiaRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CA0C; } }
        public class CTrackManiaRace1PGhosts : VT.CTrackManiaRace1P { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37C2C; } }
        public class CTrackManiaRace2PTurnBased : VT.CTrackManiaRace1P { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B383B4; } }
        public class CTrackManiaRaceAnalyzer : VT.CGameAnalyzer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50084; } }
        public class CTrackManiaRaceInterface : VT.CGameRaceInterface { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4586C; } }
        public class CTrackManiaRaceNet : VT.CTrackManiaRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4345C; } }
        public class CTrackManiaRaceNetLaps : VT.CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3760C; } }
        public class CTrackManiaRaceNetRounds : VT.CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B389CC; } }
        public class CTrackManiaRaceNetTimeAttack : VT.CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37E04; } }
        public class CTrackManiaRaceScore : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3451C; } }
        public class CTrackManiaReplayRecord : VT.CGameCtnReplayRecord { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37544; } }
        public class CTrackManiaSwitcher : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CF2C; } }
        public class CVisionResourceFile : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0B64; } }
        public class CVisionViewport : VT.CHmsViewport { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0904; } }
        public class CVisionViewportDx9 : VT.CVisionViewport { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD10BC; } }
        public class CXmlAttribute : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5D64; } }
        public class CXmlComment : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5DFC; } }
        public class CXmlDeclaration : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5BFC; } }
        public class CXmlDocument : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC59EC; } }
        public class CXmlElement : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5A9C; } }
        public class CXmlNod : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5CB4; } }
        public class CXmlText : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5B4C; } }
        public class CXmlUnknown : VT.CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC594C; } }
        public class GxFog : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABA64; } }
        public class GxFogBlender : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABB2C; } }
        public class GxLight : VT.CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB6DC; } }
        public class GxLightAmbient : VT.GxLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABBEC; } }
        public class GxLightBall : VT.GxLightPoint { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABCF4; } }
        public class GxLightDirectional : VT.GxLightNotAmbient { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB89C; } }
        public class GxLightFrustum : VT.GxLightBall { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABF44; } }
        public class GxLightNotAmbient : VT.GxLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB924; } }
        public class GxLightPoint : VT.GxLightNotAmbient { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC1DC; } }
        public class GxLightSpot : VT.GxLightBall { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC0B4; } }
    }
}
