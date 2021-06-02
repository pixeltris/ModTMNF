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
    /// <summary>
    /// Function tables
    /// </summary>
    static class FT
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
                                addressTable[field.Name] = address;
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
            public delegate void Del_WindowedSetWindowTitle(Game.CGbxApp thisPtr, ref CFastStringInt title);
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

        public static class CMwCmdBufferCore
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_SetSimulationRelativeSpeed(Game.CMwCmdBufferCore thisPtr, float speed);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void Del_StopSimulation(Game.CMwCmdBufferCore thisPtr);

            public static Del_SetSimulationRelativeSpeed SetSimulationRelativeSpeed;
            public static Del_StopSimulation StopSimulation;

            public static class Addresses
            {
                public static IntPtr SetSimulationRelativeSpeed = (IntPtr)0x00922A00;
                public static IntPtr StopSimulation = (IntPtr)0x009229D0;
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
            public delegate Game.CMwClassInfo Del_FindFromClassName(ref CFastString str);
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
    }
}
