using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ModTMNF.Game;
using System.Reflection;

#pragma warning disable 649 // Field is never assigned (because of all the runtime assigned delegates)
#pragma warning disable 108 // Use the new keyword if hiding was intended (for the Offset classes in VT)

namespace ModTMNF
{
    // This file should be the ONLY file with addresses / offsets. This makes it "easier" to support
    // another binary without having to make changes all throughout the codebase. The only exception
    // to this is are structures which are fully defined in C# (such as CString) as their offsets
    // are embedded into the struct itself.

    // "VT.XXXX.Offsets.VTable" is REQUIRED  for EVERY type

    // FT = function table
    // OT = offset table
    // ST = statics tables (static variables / misc addresss)
    // VT = virtual function table

    // TODO add missing vtables:
    //CGameCtnMediaBlockEditor
    //CGameRaceInterface
    //CGameCalendar
    //CCurveInterface

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
            public delegate bool Del_IsIndexed(Game.CMwParam thisPtr);

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
            public delegate bool Del_StaticMwIsKindOf(Game.EMwClassId classIdA, Game.EMwClassId classIdB);

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
            public delegate bool Del_IsMwParamIdEqualName(Game.CMwClassInfo thisPtr, int unk1, [MarshalAs(UnmanagedType.LPStr)] string name, int unk2);
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
    
    /// <summary>
    /// Statics tables (static variables / misc addresss)
    /// </summary>
    static class ST
    {
        public static class CMwEngineManager
        {
            public static IntPtr Instance = (IntPtr)0x00D73344;
            public static IntPtr First = (IntPtr)0x00D73BA8;
        }

        public static class CMwId
        {
            public static IntPtr s_NameTable = (IntPtr)0x00D739E0;
        }

        public static class CMwStack
        {
            public static IntPtr VTable = (IntPtr)0x00BC9C18;
        }
    }

    /// <summary>
    /// Offset tables
    /// 
    /// Each offset is by default treated as 4 bytes in size. If larger / smaller comment the struct type.
    /// e.g. public static int Children = 12;// type:CFastArray
    /// 
    /// Also to define the structure size where possible (StructSize).
    /// 
    /// Both of these things are important as they will be used by a simple C# code parser to show missing fields.
    /// </summary>
    static class OT
    {
        public static class CMwNod
        {
            public static Type BaseType = null;
            public static int StructSize = 20;
            public static int RefCount = 4;
            public static int Fid = 8;
            public static int Dependants = 12;
            public static int Unk1 = 16;
        }

        public static class CGameCtnMenus
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 2112;// GUESSED (last accessed offset in ctor is 2108, first offset accessed in CTrackManiaMenus is 2168)
        }

        public static class CTrackManiaMenus
        {
            public static Type BaseType = typeof(OT.CGameCtnMenus);
            public static int StructSize = 2920;
            public static int NbPlayers = 2272;// type:Enum
            public static int TimeLimit = 2276;// default:15000 (15 seconds)
            public static int Rounds = 2280;// default:5
        }

        ///////////////////////////////////////////////////////////
        // All non CMwNod types should go below here
        ///////////////////////////////////////////////////////////

        public static class CMwClassInfo
        {
            public static Type BaseType = null;
            public static int StructSize = 40;
            public static int Id = 4;
            public static int Parent = 8;
            public static int Children = 12;// type:CFastArray
            public static int Name = 20;
            public static int Next = 24;
            public static int New = 28;
            public static int ParamInfos = 32;
            public static int ParamCount = 36;
        }

        public static class CMwEngineInfo
        {
            public static Type BaseType = null;
            public static int Id = 4;
            public static int Name = 8;
            public static int Classes = 12;
        }

        public static class CMwEngineManager
        {
            public static Type BaseType = null;
            public static int m_Engines = 4;
        }

        public static class CMwStack
        {
            public static int StructSize = 28;
            public static int VTable = 0;
            public static int WriteOffset = 4;
            public static int Size = 8;
            public static int DeleteMemory = 12;
            public static int Values = 16;
            public static int Types = 20;
            public static int ReadOffset = 24;
        }

        public static class CMwStackItem// Not a real type
        {
            public static Type BaseType = null;
            public static int StructSize = 16;
            public static int Value = 0;
            public static int Type = 4;// type:CMwStack::EStackType
        }

        public static class SMwParamInfo
        {
            public static Type BaseType = null;
            public static int StructSize = 28;
            public static int Type = 0;
            public static int Id = 4;
            public static int Param = 8;
            public static int Offset = 12;
            public static int Name = 16;
            public static int Flags1 = 20;//flag(0x10) == implemented as VirtualParam_Get?
            public static int Flags2 = 24;
        }

        public static class SMwParamInfo_Class
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 36;
            public static int ClassInfo = 28;
            public static int Unk1 = 32;
        }

        public static class SMwParamInfo_Enum
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 40;
            public static int CppName = 28;
            public static int ValueNamesCount = 32;
            public static int ValueNames = 36;
        }

        public static class SMwParamInfo_Array
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 44;
            public static int Unk1 = 28;
            public static int TypeName = 32;
            public static int Unk2 = 36;
            public static int ClassInfo = 40;
        }

        public static class SMwParamInfo_Action
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 32;
            public static int Unk1 = 28;
        }

        public static class SMwParamInfo_Proc
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 48;
            public static int FuncPtr = 28;
            public static int ArgCount = 32;
            public static int ArgClassIds = 36;
            public static int ArgNames = 40;
            public static int ArgFlags = 44;
        }

        public static class SMwParamInfo_Range
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 36;
            public static int Unk1 = 28;
            public static int Unk2 = 32;
        }

        public static class SMwParamInfo_Vec2
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 36;
            public static int Var1Name = 28;
            public static int Var2Name = 32;
        }

        public static class SMwParamInfo_Vec3
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 40;
            public static int Var1Name = 28;
            public static int Var2Name = 32;
            public static int Var3Name = 36;
        }

        public static class SMwParamInfo_Vec4
        {
            public static Type BaseType = typeof(OT.SMwParamInfo);
            public static int StructSize = 44;
            public static int Var1Name = 28;
            public static int Var2Name = 32;
            public static int Var3Name = 36;
            public static int Var4Name = 40;
        }
    }

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
        static CMwNod[][] vtables;

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
            vtables = new CMwNod[highestEngineId + 1][];
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
                vtables[kvp.Key] = new CMwNod[kvp.Value + 1];
            }
            foreach (Type type in typeof(VT).GetNestedTypes())
            {
                EMwClassId classId;
                if (Enum.TryParse<EMwClassId>(type.Name, out classId))
                {
                    EMwEngineId engineId = CMwEngineManager.GetEngineIdFromClassId(classId);
                    int eeid = CMwEngineManager.LongToShortEngineId((int)engineId);
                    int cid  = CMwEngineManager.LongToShortClassId((int)classId);
                    CMwNod obj = (CMwNod)Activator.CreateInstance(type);
                    InitVTable(obj, type);
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
                else
                {
                    // TODO: Handle other vtable types
                }
            }
            return true;
        }

        private static void InitVTable(CMwNod obj, Type type)
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
                        InitVTable(obj, type, vtablePtr);
                    }
                }
            }
        }

        private static void InitVTable(CMwNod obj, Type type, IntPtr vtablePtr)
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
            if (typeof(CMwNod).IsAssignableFrom(type.BaseType))
            {
                InitVTable(obj, type.BaseType, vtablePtr);
            }
        }

        public static TTable Get<TTable>(IntPtr nodAddress) where TTable : CMwNod
        {
            EMwClassId classId = NativeDll.GetMwClassId(nodAddress);
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
            public delegate bool Del_MwIsKindOf(Game.CMwNod thisPtr, EMwClassId classId);
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
            public delegate void Del_HeaderChunk(Game.CMwNod thisPtr, Game.CClassicArchive archive, ref int chunkId, ref bool light);
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

        public class CMwParam : CMwNod
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_IsArray(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_IsBuffer(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_IsBufferCat(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_IsRefBuffer(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int Del_IndexedGetCount(Game.CMwParam thisPtr, Game.CMwValueStd value);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_IsStruct(Game.CMwParam thisPtr);
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
            public delegate bool Del_VCanGetValueFromString(Game.CMwParam thisPtr);
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate bool Del_VCanGetStringFromValue(Game.CMwParam thisPtr);
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

        public class CAudioBufferKeeper : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A93C; } }
        public class CAudioMusic : CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A4FC; } }
        public class CAudioPort : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A0DC; } }
        public class CAudioPortNull : CAudioPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9CDCC; } }
        public class CAudioSound : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A364; } }
        public class CAudioSoundEngine : CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A84C; } }
        public class CAudioSoundMulti : CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A5AC; } }
        public class CAudioSoundSurface : CAudioSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9A784; } }
        public class CBlockVariable : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCADC4; } }
        public class CBoatParam : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3F1C; } }
        public class CBoatSail : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAAE1C; } }
        public class CBoatSailState : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAAB84; } }
        public class CBoatTeamActionDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA45D4; } }
        public class CBoatTeamDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA46BC; } }
        public class CBoatTeamMateActionDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA43F4; } }
        public class CBoatTeamMateLocationDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA450C; } }
        public class CControlBase : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92314; } }
        public class CControlButton : CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93614; } }
        public class CControlColorChooser : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96324; } }
        public class CControlColorChooser2 : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97124; } }
        public class CControlContainer : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92834; } }
        public class CControlCredit : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B965F4; } }
        public class CControlCurve : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B989E4; } }
        public class CControlDisplayGraph : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9572C; } }
        public class CControlEffect : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B979F4; } }
        public class CControlEffectCombined : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99644; } }
        public class CControlEffectMaster : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B951BC; } }
        public class CControlEffectMotion : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99B9C; } }
        public class CControlEffectMoveFrame : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99524; } }
        public class CControlEffectSimi : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97454; } }
        public class CControlEffectSwitchStyle : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99D24; } }
        public class CControlEntry : CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94594; } }
        public class CControlEnum : CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94054; } }
        public class CControlField2 : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B95AA4; } }
        public class CControlForm : CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9991C; } }
        public class CControlFrame : CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92B3C; } }
        public class CControlFrameAnimated : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9537C; } }
        public class CControlFrameStyled : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B960AC; } }
        public class CControlGrid : CControlContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9328C; } }
        public class CControlIconIndex : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98804; } }
        public class CControlImage : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9835C; } }
        public class CControlLabel : CControlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B92DB4; } }
        public class CControlLayout : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97694; } }
        public class CControlList : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94DA4; } }
        public class CControlListItem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B942F4; } }
        public class CControlListMap : CControlList { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98E14; } }
        public class CControlListMap2 : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9912C; } }
        public class CControlMediaItem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98264; } }
        public class CControlMediaPlayer : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97774; } }
        public class CControlOverlay : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B98064; } }
        public class CControlPager : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96914; } }
        public class CControlQuad : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93004; } }
        public class CControlRadar : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B985AC; } }
        public class CControlSimi2 : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B975C4; } }
        public class CControlSlider : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97ADC; } }
        public class CControlStyle : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B93ABC; } }
        public class CControlStyleSheet : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B943DC; } }
        public class CControlText : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B94ABC; } }
        public class CControlTimeLine : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9971C; } }
        public class CControlTimeLine2 : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B96CBC; } }
        public class CControlTrackManiaTeamCard : CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B35364; } }
        public class CControlUiDockable : CControlUiElement { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B97D7C; } }
        public class CControlUiElement : CControlForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B99E1C; } }
        public class CControlUiRange : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9487C; } }
        public class CControlUrlLinks : CControlBase { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B95E0C; } }
        public class CCtnMediaBlockEventTrackMania : CGameCtnMediaBlockEvent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3720C; } }
        public class CCtnMediaBlockUiTMSimpleEvtsDisplay : CGameCtnMediaBlockUiSimpleEvtsDisplay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3567C; } }
        public class CCurveInterface : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CDx9DeviceCaps : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD2A1C; } }
        public class CFunc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F12C; } }
        public class CFuncClouds : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F42C; } }
        public class CFuncColor : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60174; } }
        public class CFuncColorGradient : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DD6C; } }
        public class CFuncCurves2Real : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F314; } }
        public class CFuncCurvesReal : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F5B4; } }
        public class CFuncEnum : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DBBC; } }
        public class CFuncEnvelope : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DE9C; } }
        public class CFuncFullColorGradient : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F674; } }
        public class CFuncGroup : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F9BC; } }
        public class CFuncGroupElem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FA6C; } }
        public class CFuncKeys : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F074; } }
        public class CFuncKeysCmd : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EF04; } }
        public class CFuncKeysNatural : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F8F4; } }
        public class CFuncKeysPath : CFuncKeysTransQuat { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E64C; } }
        public class CFuncKeysReal : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D9DC; } }
        public class CFuncKeysReals : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FC44; } }
        public class CFuncKeysSkel : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EE34; } }
        public class CFuncKeysSound : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EFB4; } }
        public class CFuncKeysTrans : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EC0C; } }
        public class CFuncKeysTransQuat : CFuncKeysTrans { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ECE4; } }
        public class CFuncKeysVisual : CFuncKeys { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60764; } }
        public class CFuncLight : CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EB44; } }
        public class CFuncLightColor : CFuncLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B600C4; } }
        public class CFuncLightIntensity : CFuncLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B609AC; } }
        public class CFuncManagerCharacter : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E944; } }
        public class CFuncManagerCharacterAdv : CFuncManagerCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FAFC; } }
        public class CFuncNoise : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F1E4; } }
        public class CFuncPathMesh : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E77C; } }
        public class CFuncPathMeshLocation : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FCFC; } }
        public class CFuncPlug : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DAC4; } }
        public class CFuncPuffLull : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5DFBC; } }
        public class CFuncSegment : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C40DE4; } }
        public class CFuncShader : CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60A74; } }
        public class CFuncShaderFxFactor : CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6059C; } }
        public class CFuncShaderLayerUV : CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60334; } }
        public class CFuncShaders : CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6069C; } }
        public class CFuncShaderTweakKeysTranss : CFuncShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F734; } }
        public class CFuncSin : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6081C; } }
        public class CFuncSkel : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CFBC; } }
        public class CFuncSkelValues : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EDA4; } }
        public class CFuncTree : CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E9F4; } }
        public class CFuncTreeBend : CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FDD4; } }
        public class CFuncTreeElevator : CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5F814; } }
        public class CFuncTreeRotate : CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FE84; } }
        public class CFuncTreeSubVisualSequence : CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E84C; } }
        public class CFuncTreeTranslate : CFuncTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B608CC; } }
        public class CFuncVisual : CFuncPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5EA8C; } }
        public class CFuncVisualBlendShapeSequence : CFuncVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FB9C; } }
        public class CFuncVisualShiver : CFuncVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5FF84; } }
        public class CFuncWeather : CFunc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5E22C; } }
        public class CGameAdvertising : CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83844; } }
        public class CGameAdvertisingElement : CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C574; } }
        public class CGameAnalyzer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85BD4; } }
        public class CGameApp : CGameProcess { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B60DFC; } }
        public class CGameAvatar : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6531C; } }
        public class CGameBulletModel : CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90F94; } }
        public class CGameCalendar : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameCalendarEvent : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B921B4; } }
        public class CGameCamera : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CB2C; } }
        public class CGameCampaignPlayerScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81D8C; } }
        public class CGameCampaignScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81E0C; } }
        public class CGameCampaignsScoresManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81F0C; } }
        public class CGameChallengeScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81E8C; } }
        public class CGameChampionship : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87AC4; } }
        public class CGameControlCamera : CSceneController { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65614; } }
        public class CGameControlCameraEffect : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9136C; } }
        public class CGameControlCameraEffectAdaptativeNearZ : CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89C7C; } }
        public class CGameControlCameraEffectGroup : CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B870D4; } }
        public class CGameControlCameraEffectShake : CGameControlCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91A54; } }
        public class CGameControlCameraFollowAboveWater : CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B503AC; } }
        public class CGameControlCameraFree : CGameControlCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7C984; } }
        public class CGameControlCameraMaster : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CBEC; } }
        public class CGameControlCameraOrbital3d : CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CE3C; } }
        public class CGameControlCameraTarget : CGameControlCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B653D4; } }
        public class CGameControlCameraTrackManiaRace : CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3735C; } }
        public class CGameControlCameraTrackManiaRace2 : CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B36AF4; } }
        public class CGameControlCameraTrackManiaRace3 : CGameControlCameraTarget { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B361A4; } }
        public class CGameControlCard : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B61174; } }
        public class CGameControlCardCalendar : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8EC94; } }
        public class CGameControlCardCalendarEvent : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8EFB4; } }
        public class CGameControlCardChampionship : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8DAD4; } }
        public class CGameControlCardCtnArticle : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E6B4; } }
        public class CGameControlCardCtnCampaign : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E3C4; } }
        public class CGameControlCardCtnChallengeInfo : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6488C; } }
        public class CGameControlCardCtnChapter : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8DDC4; } }
        public class CGameControlCardCtnGhost : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E084; } }
        public class CGameControlCardCtnGhostInfo : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D7E4; } }
        public class CGameControlCardCtnNetServerInfo : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B727CC; } }
        public class CGameControlCardCtnReplayRecordInfo : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8E974; } }
        public class CGameControlCardCtnVehicle : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D52C; } }
        public class CGameControlCardGeneric : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B738EC; } }
        public class CGameControlCardLadderRanking : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F364; } }
        public class CGameControlCardLeague : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8918C; } }
        public class CGameControlCardManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B638A4; } }
        public class CGameControlCardMessage : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B5FC; } }
        public class CGameControlCardNetOnlineEvent : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CF3C; } }
        public class CGameControlCardNetOnlineNews : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88B7C; } }
        public class CGameControlCardNetTeamInfo : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8D264; } }
        public class CGameControlCardProfile : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8577C; } }
        public class CGameControlDataType : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8701C; } }
        public class CGameControlGrid : CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7412C; } }
        public class CGameControlGridCard : CGameControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B634EC; } }
        public class CGameControlGridCtnCampaign : CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CC4C; } }
        public class CGameControlGridCtnChallengeGroup : CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C8EC; } }
        public class CGameControlPlayer : CGameRule { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85F44; } }
        public class CGameControlPlayerInput : CGameControlPlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85E74; } }
        public class CGameControlPlayerNet : CGameControlPlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90E84; } }
        public class CGameCtnApp : CGameApp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6890C; } }
        public class CGameCtnArticle : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7DB24; } }
        public class CGameCtnBlock : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6950C; } }
        public class CGameCtnBlockInfo : CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B78044; } }
        public class CGameCtnBlockInfoClassic : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FAFC; } }
        public class CGameCtnBlockInfoClip : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B772B4; } }
        public class CGameCtnBlockInfoFlat : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FD84; } }
        public class CGameCtnBlockInfoFrontier : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FC04; } }
        public class CGameCtnBlockInfoPylon : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B864EC; } }
        public class CGameCtnBlockInfoRectAsym : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84114; } }
        public class CGameCtnBlockInfoRoad : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83F54; } }
        public class CGameCtnBlockInfoSlope : CGameCtnBlockInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C684; } }
        public class CGameCtnBlockSkin : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83DC4; } }
        public class CGameCtnBlockUnit : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84524; } }
        public class CGameCtnBlockUnitInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B843C4; } }
        public class CGameCtnCampaign : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B63FCC; } }
        public class CGameCtnCatalog : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B614AC; } }
        public class CGameCtnChallenge : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B61AFC; } }
        public class CGameCtnChallengeGroup : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76834; } }
        public class CGameCtnChallengeInfo : CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B64234; } }
        public class CGameCtnChallengeParameters : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65FCC; } }
        public class CGameCtnChapter : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76934; } }
        public class CGameCtnCollection : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69844; } }
        public class CGameCtnCollector : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8629C; } }
        public class CGameCtnCollectorList : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83E7C; } }
        public class CGameCtnCollectorVehicle : CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7705C; } }
        public class CGameCtnCursor : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B77CDC; } }
        public class CGameCtnDecoration : CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86384; } }
        public class CGameCtnDecorationAudio : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90AEC; } }
        public class CGameCtnDecorationMood : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90944; } }
        public class CGameCtnDecorationSize : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90834; } }
        public class CGameCtnDecorationTerrainModifier : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8888C; } }
        public class CGameCtnEdControlCam : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91C14; } }
        public class CGameCtnEdControlCamCustom : CGameCtnEdControlCam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B234; } }
        public class CGameCtnEdControlCamPath : CGameCtnEdControlCam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B15C; } }
        public class CGameCtnEditor : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B615E4; } }
        public class CGameCtnEditorScenePocLink : CSceneObjectLink { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B77E24; } }
        public class CGameCtnGhost : CGameGhost { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B660E4; } }
        public class CGameCtnGhostInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76F34; } }
        public class CGameCtnMasterServer : CGameMasterServer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8181C; } }
        public class CGameCtnMediaBlock : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65034; } }
        public class CGameCtnMediaBlock3dStereo : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AD0C; } }
        public class CGameCtnMediaBlockCamera : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B40C; } }
        public class CGameCtnMediaBlockCameraCustom : CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8994C; } }
        public class CGameCtnMediaBlockCameraEffect : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9191C; } }
        public class CGameCtnMediaBlockCameraEffectShake : CGameCtnMediaBlockCameraEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AB5C; } }
        public class CGameCtnMediaBlockCameraGame : CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89764; } }
        public class CGameCtnMediaBlockCameraOrbital : CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D46C; } }
        public class CGameCtnMediaBlockCameraPath : CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89B0C; } }
        public class CGameCtnMediaBlockCameraSimple : CGameCtnMediaBlockCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D2B4; } }
        public class CGameCtnMediaBlockEditor : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameCtnMediaBlockEditorTriangles : CGameCtnMediaBlockEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90664; } }
        public class CGameCtnMediaBlockEvent : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6592C; } }
        public class CGameCtnMediaBlockFx : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AF14; } }
        public class CGameCtnMediaBlockFxBloom : CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A7FC; } }
        public class CGameCtnMediaBlockFxBlur : CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91804; } }
        public class CGameCtnMediaBlockFxBlurDepth : CGameCtnMediaBlockFxBlur { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A65C; } }
        public class CGameCtnMediaBlockFxBlurMotion : CGameCtnMediaBlockFxBlur { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F7AC; } }
        public class CGameCtnMediaBlockFxColors : CGameCtnMediaBlockFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A4A4; } }
        public class CGameCtnMediaBlockGhost : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89DC4; } }
        public class CGameCtnMediaBlockImage : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87C2C; } }
        public class CGameCtnMediaBlockMusicEffect : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A9A4; } }
        public class CGameCtnMediaBlockSound : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B875AC; } }
        public class CGameCtnMediaBlockText : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B89F94; } }
        public class CGameCtnMediaBlockTime : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A30C; } }
        public class CGameCtnMediaBlockTrails : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F65C; } }
        public class CGameCtnMediaBlockTransition : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B916EC; } }
        public class CGameCtnMediaBlockTransitionFade : CGameCtnMediaBlockTransition { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8A16C; } }
        public class CGameCtnMediaBlockTriangles : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91D1C; } }
        public class CGameCtnMediaBlockTriangles2D : CGameCtnMediaBlockTriangles { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FE8C; } }
        public class CGameCtnMediaBlockTriangles3D : CGameCtnMediaBlockTriangles { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90014; } }
        public class CGameCtnMediaBlockUi : CGameCtnMediaBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6519C; } }
        public class CGameCtnMediaBlockUiSimpleEvtsDisplay : CGameCtnMediaBlockUi { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B64EE4; } }
        public class CGameCtnMediaClip : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7A98C; } }
        public class CGameCtnMediaClipGroup : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84E6C; } }
        public class CGameCtnMediaClipPlayer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7CCF4; } }
        public class CGameCtnMediaClipViewer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7A854; } }
        public class CGameCtnMediaTrack : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D17C; } }
        public class CGameCtnMediaTracker : CGameCtnEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B78BD4; } }
        public class CGameCtnMediaVideoParams : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B835CC; } }
        public class CGameCtnMenus : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6E11C; } }
        public class CGameCtnNetForm : CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8520C; } }
        public class CGameCtnNetServerInfo : CGameNetServerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84B7C; } }
        public class CGameCtnNetwork : CGameNetwork { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7B81C; } }
        public class CGameCtnObjectInfo : CGameCtnCollector { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90BF4; } }
        public class CGameCtnPainter : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B80F54; } }
        public class CGameCtnPainterSetting : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BB5C; } }
        public class CGameCtnParticleParam : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90764; } }
        public class CGameCtnPylonColumn : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86704; } }
        public class CGameCtnReplayRecord : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65B4C; } }
        public class CGameCtnReplayRecordInfo : CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76E54; } }
        public class CGameCtnZone : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8896C; } }
        public class CGameCtnZoneFlat : CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88794; } }
        public class CGameCtnZoneFrontier : CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8F914; } }
        public class CGameCtnZoneTest : CGameCtnZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8FA04; } }
        public class CGameDialogs : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69234; } }
        public class CGameDialogShootVideo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B834FC; } }
        public class CGameEngine : CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8613C; } }
        public class CGameEnvironmentManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86094; } }
        public class CGameFid : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84D8C; } }
        public class CGameGeneralScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81D0C; } }
        public class CGameGhost : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7AAA4; } }
        public class CGameHighScore : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84F44; } }
        public class CGameInterface : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87E0C; } }
        public class CGameLadderRanking : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8772C; } }
        public class CGameLadderRankingCtnChallengeAchievement : CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BA54; } }
        public class CGameLadderRankingLeague : CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B9A4; } }
        public class CGameLadderRankingPlayer : CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B802CC; } }
        public class CGameLadderRankingSkill : CGameLadderRanking { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B8DC; } }
        public class CGameLadderScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B855A4; } }
        public class CGameLadderScoresComputer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85524; } }
        public class CGameLeague : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B73BDC; } }
        public class CGameLeagueManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B819A4; } }
        public class CGameLoadProgress : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8213C; } }
        public class CGameManialinkBrowser : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B73DAC; } }
        public class CGameManialinkEntry : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9142C; } }
        public class CGameManialinkFileEntry : CGameManialinkEntry { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B914E4; } }
        public class CGameManiaNetResource : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B87914; } }
        public class CGameMasterServer : CNetMasterServer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6203C; } }
        public class CGameMenu : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B76B74; } }
        public class CGameMenuColorEffect : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9118C; } }
        public class CGameMenuFrame : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8945C; } }
        public class CGameMenuScaleEffect : CControlEffect { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B90CEC; } }
        public class CGameMobil : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B7D0C4; } }
        public class CGameNetDataDownload : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B33C; } }
        public class CGameNetFileTransfer : CNetFileTransfer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86E1C; } }
        public class CGameNetForm : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B69C1C; } }
        public class CGameNetFormAdmin : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85374; } }
        public class CGameNetFormBuddy : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B84FDC; } }
        public class CGameNetFormCallVote : CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8514C; } }
        public class CGameNetFormGameSync : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8542C; } }
        public class CGameNetFormTimeSync : CNetFormTimed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B852BC; } }
        public class CGameNetFormTunnel : CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B85094; } }
        public class CGameNetOnlineEvent : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86A8C; } }
        public class CGameNetOnlineMessage : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86994; } }
        public class CGameNetOnlineNews : CGameNetOnlineEvent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86B84; } }
        public class CGameNetOnlineNewsReply : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86C0C; } }
        public class CGameNetPlayerInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8469C; } }
        public class CGameNetServerInfo : CNetMasterHost { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8489C; } }
        public class CGameNetTeamInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8782C; } }
        public class CGameNetwork : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B62C64; } }
        public class CGameNod : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86004; } }
        public class CGameOutlineBox : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B773AC; } }
        public class CGamePlayer : CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B664C4; } }
        public class CGamePlayerAttributesLiving : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8CB6C; } }
        public class CGamePlayerCameraSet : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6658C; } }
        public class CGamePlayerInfo : CGameNetPlayerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6447C; } }
        public class CGamePlayerOfficialScores : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B81F8C; } }
        public class CGamePlayerProfile : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B639EC; } }
        public class CGamePlayerScore : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B63D14; } }
        public class CGamePlayerScoresShooter : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C7FC; } }
        public class CGamePlayground : CGameNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6664C; } }
        public class CGamePlaygroundInterface : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B663DC; } }
        public class CGamePopUp : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8AE6C; } }
        public class CGameProcess : CMwCmdContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B861DC; } }
        public class CGameRace : CGamePlayground { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B65D84; } }
        public class CGameRaceInterface : CGamePlaygroundInterface { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00000000; } }
        public class CGameRemoteBuffer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B86F14; } }
        public class CGameRemoteBufferDataInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B867CC; } }
        public class CGameRemoteBufferDataInfoFinds : CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C184; } }
        public class CGameRemoteBufferDataInfoRankings : CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BD8C; } }
        public class CGameRemoteBufferDataInfoSearchs : CGameRemoteBufferDataInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8BFA4; } }
        public class CGameRemoteBufferPool : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B868CC; } }
        public class CGameRule : CGameProcess { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8C75C; } }
        public class CGameSafeFrame : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B8B08C; } }
        public class CGameSafeFrameConfig : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B91B6C; } }
        public class CGameScene : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B6587C; } }
        public class CGameSkillScoreComputer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B88564; } }
        public class CGameSkin : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B83C8C; } }
        public class CGameSystemOverlay : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B72ACC; } }
        public class CGameTournament : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B879CC; } }
        public class CHdrComment : CXmlComment { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5754; } }
        public class CHdrDeclaration : CXmlDeclaration { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC57F4; } }
        public class CHdrDocument : CXmlDocument { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5894; } }
        public class CHdrElement : CXmlElement { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC568C; } }
        public class CHdrText : CXmlText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC554C; } }
        public class CHdrUnknown : CXmlUnknown { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC55EC; } }
        public class CHmsAmbientOcc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58524; } }
        public class CHmsCamera : CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5655C; } }
        public class CHmsCollisionManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B55DAC; } }
        public class CHmsConfig : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57684; } }
        public class CHmsCorpus : CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56E2C; } }
        public class CHmsCorpus2d : CHmsCorpus { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B595B4; } }
        public class CHmsCorpusLight : CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58EF4; } }
        public class CHmsFogPlane : CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5965C; } }
        public class CHmsForceField : CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59494; } }
        public class CHmsForceFieldBall : CHmsForceField { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B597D4; } }
        public class CHmsForceFieldUniform : CHmsForceField { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B596FC; } }
        public class CHmsItem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56064; } }
        public class CHmsItemShadow : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5872C; } }
        public class CHmsLight : CHmsPocEmitter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B588F4; } }
        public class CHmsListener : CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59A3C; } }
        public class CHmsPackLightMap : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B568DC; } }
        public class CHmsPackLightMapAlloc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58E4C; } }
        public class CHmsPackLightMapCache : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58A2C; } }
        public class CHmsPackLightMapMood : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58DAC; } }
        public class CHmsPicker : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58604; } }
        public class CHmsPoc : CHmsZoneElem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58804; } }
        public class CHmsPocEmitter : CHmsPoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B572F4; } }
        public class CHmsPortal : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57484; } }
        public class CHmsPortalProperty : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B57274; } }
        public class CHmsPrecalcRender : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B583FC; } }
        public class CHmsShadowGroup : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59074; } }
        public class CHmsSoundSource : CHmsPocEmitter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B598DC; } }
        public class CHmsViewport : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5574C; } }
        public class CHmsZone : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5795C; } }
        public class CHmsZoneDynamic : CHmsZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B56FBC; } }
        public class CHmsZoneElem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B593E4; } }
        public class CHmsZoneOverlay : CHmsZone { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B58224; } }
        public class CInputBindingsConfig : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEA74; } }
        public class CInputDevice : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF004; } }
        public class CInputDeviceDx8Keyboard : CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF3CC; } }
        public class CInputDeviceDx8Mouse : CInputDeviceMouse { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF304; } }
        public class CInputDeviceDx8Pad : CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF24C; } }
        public class CInputDeviceMouse : CInputDevice { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF55C; } }
        public class CInputPort : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEB94; } }
        public class CInputPortDx8 : CInputPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBEE84; } }
        public class CInputPortNull : CInputPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBF48C; } }
        public class CManoeuvre : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB33C; } }
        public class CMotion : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AC1C; } }
        public class CMotionBone : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C264; } }
        public class CMotionCmdBase : CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59EBC; } }
        public class CMotionCmdBaseParams : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B08C; } }
        public class CMotionDayTime : CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B3CC; } }
        public class CMotionEmitterLeaves : CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C134; } }
        public class CMotionEmitterParticles : CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59CEC; } }
        public class CMotionFunc : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D3EC; } }
        public class CMotionGroupPlayers : CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C344; } }
        public class CMotionLight : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CC44; } }
        public class CMotionManaged : CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ACF4; } }
        public class CMotionManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AB6C; } }
        public class CMotionManagerCharacter : CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C78C; } }
        public class CMotionManagerCharacterAdv : CMotionManagerCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C464; } }
        public class CMotionManagerLeaves : CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5BF8C; } }
        public class CMotionManagerMeteo : CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B924; } }
        public class CMotionManagerMeteoPuffLull : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B304; } }
        public class CMotionManagerParticles : CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59BDC; } }
        public class CMotionManagerWeathers : CMotionManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B5FC; } }
        public class CMotionParticleEmitterModel : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5ADE4; } }
        public class CMotionParticleType : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5A4D4; } }
        public class CMotionPath : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D164; } }
        public class CMotionPlay : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D7DC; } }
        public class CMotionPlayCmd : CMotionPlay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D05C; } }
        public class CMotionPlayer : CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5AEEC; } }
        public class CMotionPlaySound : CMotionPlay { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D304; } }
        public class CMotionPlaySoundMobil : CMotionPlaySound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D22C; } }
        public class CMotions : CMotion { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D4AC; } }
        public class CMotionShader : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B204; } }
        public class CMotionSkel : CMotionSkelSimple { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CE3C; } }
        public class CMotionSkelSimple : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CF14; } }
        public class CMotionTeamAction : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D684; } }
        public class CMotionTeamActionInfo : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D8BC; } }
        public class CMotionTeamManager : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C654; } }
        public class CMotionTimerLoop : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5D5C4; } }
        public class CMotionTrack : CMwCmdContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B144; } }
        public class CMotionTrackMobilMove : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CD1C; } }
        public class CMotionTrackMobilPitchin : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CA3C; } }
        public class CMotionTrackMobilRotate : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B59FDC; } }
        public class CMotionTrackMobilScale : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C05C; } }
        public class CMotionTrackTree : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5C8F4; } }
        public class CMotionTrackVisual : CMotionTrack { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5CB6C; } }
        public class CMotionWeather : CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5B73C; } }
        public class CMotionWindBlocker : CMotionManaged { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5BB94; } }
        public class CMwClassInfoViewer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0614; } }
        public class CMwCmd : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC646C; } }
        public class CMwCmdAffectIdent : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD054C; } }
        public class CMwCmdAffectIdentBool : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD7EC; } }
        public class CMwCmdAffectIdentClass : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD8AC; } }
        public class CMwCmdAffectIdentEnum : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB3A4; } }
        public class CMwCmdAffectIdentIso4 : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD684; } }
        public class CMwCmdAffectIdentNum : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD504; } }
        public class CMwCmdAffectIdentString : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD5C4; } }
        public class CMwCmdAffectIdentVec2 : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD444; } }
        public class CMwCmdAffectIdentVec3 : CMwCmdAffectIdent { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD384; } }
        public class CMwCmdAffectParam : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0484; } }
        public class CMwCmdAffectParamBool : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD2C4; } }
        public class CMwCmdAffectParamClass : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD084; } }
        public class CMwCmdAffectParamEnum : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD144; } }
        public class CMwCmdAffectParamIso4 : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD204; } }
        public class CMwCmdAffectParamNum : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCFC4; } }
        public class CMwCmdAffectParamString : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCE44; } }
        public class CMwCmdAffectParamVec2 : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCF04; } }
        public class CMwCmdAffectParamVec3 : CMwCmdAffectParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCD84; } }
        public class CMwCmdArrayAdd : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB534; } }
        public class CMwCmdArrayRemove : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB5EC; } }
        public class CMwCmdBlock : CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCACEC; } }
        public class CMwCmdBlockCast : CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDEF4; } }
        public class CMwCmdBlockFunction : CMwCmdBlockProcedure { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBD04; } }
        public class CMwCmdBlockMain : CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9E0C; } }
        public class CMwCmdBlockProcedure : CMwCmdBlock { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAE54; } }
        public class CMwCmdBreak : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBE84; } }
        public class CMwCmdBuffer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA5DC; } }
        public class CMwCmdBufferCore : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5FA4; } }
        public class CMwCmdCall : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD03D4; } }
        public class CMwCmdContainer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA0BC; } }
        public class CMwCmdContinue : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBDCC; } }
        public class CMwCmdExp : CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAC1C; } }
        public class CMwCmdExpAdd : CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD00A4; } }
        public class CMwCmdExpAnd : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFF0C; } }
        public class CMwCmdExpBool : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFFDC; } }
        public class CMwCmdExpBoolBin : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEB3C; } }
        public class CMwCmdExpBoolFunction : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBB84; } }
        public class CMwCmdExpBoolIdent : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFE34; } }
        public class CMwCmdExpBoolParam : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFD64; } }
        public class CMwCmdExpClass : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9D14; } }
        public class CMwCmdExpClassFunction : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB9E4; } }
        public class CMwCmdExpClassIdent : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEA64; } }
        public class CMwCmdExpClassParam : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE8CC; } }
        public class CMwCmdExpClassThis : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB6A4; } }
        public class CMwCmdExpDiff : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFC9C; } }
        public class CMwCmdExpDiv : CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFAFC; } }
        public class CMwCmdExpEgal : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFBD4; } }
        public class CMwCmdExpEnum : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE99C; } }
        public class CMwCmdExpEnumCastedNum : CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAFBC; } }
        public class CMwCmdExpEnumIdent : CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB2D4; } }
        public class CMwCmdExpEnumParam : CMwCmdExpEnum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE7FC; } }
        public class CMwCmdExpInf : CMwCmdExpBoolBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCFA2C; } }
        public class CMwCmdExpInfEgal : CMwCmdExpBoolBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF95C; } }
        public class CMwCmdExpIso4 : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE22C; } }
        public class CMwCmdExpIso4Function : CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBAB4; } }
        public class CMwCmdExpIso4Ident : CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE08C; } }
        public class CMwCmdExpIso4Inverse : CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC18C; } }
        public class CMwCmdExpIso4Mult : CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC25C; } }
        public class CMwCmdExpIso4Param : CMwCmdExpIso4 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDFBC; } }
        public class CMwCmdExpMult : CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF88C; } }
        public class CMwCmdExpNeg : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF6F4; } }
        public class CMwCmdExpNot : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF7C4; } }
        public class CMwCmdExpNum : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF624; } }
        public class CMwCmdExpNumBin : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCED2C; } }
        public class CMwCmdExpNumCastedEnum : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB08C; } }
        public class CMwCmdExpNumDotProduct2 : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBFEC; } }
        public class CMwCmdExpNumDotProduct3 : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC0BC; } }
        public class CMwCmdExpNumFunction : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEC1C; } }
        public class CMwCmdExpNumIdent : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF54C; } }
        public class CMwCmdExpNumParam : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF47C; } }
        public class CMwCmdExpOr : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF214; } }
        public class CMwCmdExpPower : CMwCmdExpNumBin { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF2DC; } }
        public class CMwCmdExpString : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF074; } }
        public class CMwCmdExpStringConcat : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD06AC; } }
        public class CMwCmdExpStringFunction : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB914; } }
        public class CMwCmdExpStringIdent : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEECC; } }
        public class CMwCmdExpStringParam : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEDFC; } }
        public class CMwCmdExpStringTrunc : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB464; } }
        public class CMwCmdExpStringUpDownCase : CMwCmdExpString { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB15C; } }
        public class CMwCmdExpSub : CMwCmdExpNum { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF3AC; } }
        public class CMwCmdExpSup : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCF13C; } }
        public class CMwCmdExpSupEgal : CMwCmdExpBool { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCEF9C; } }
        public class CMwCmdExpVec2 : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE664; } }
        public class CMwCmdExpVec2Add : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCBFC; } }
        public class CMwCmdExpVec2Function : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB844; } }
        public class CMwCmdExpVec2Ident : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE3C4; } }
        public class CMwCmdExpVec2Mult : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC8DC; } }
        public class CMwCmdExpVec2Neg : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC80C; } }
        public class CMwCmdExpVec2Param : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE494; } }
        public class CMwCmdExpVec2Sub : CMwCmdExpVec2 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC9AC; } }
        public class CMwCmdExpVec3 : CMwCmdExp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE574; } }
        public class CMwCmdExpVec3Add : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC73C; } }
        public class CMwCmdExpVec3Function : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB774; } }
        public class CMwCmdExpVec3Ident : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE2F4; } }
        public class CMwCmdExpVec3Mult : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC3FC; } }
        public class CMwCmdExpVec3MultIso : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC4CC; } }
        public class CMwCmdExpVec3Neg : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC59C; } }
        public class CMwCmdExpVec3Param : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE15C; } }
        public class CMwCmdExpVec3Product : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC32C; } }
        public class CMwCmdExpVec3Sub : CMwCmdExpVec3 { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCC66C; } }
        public class CMwCmdFastCall : CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC6334; } }
        public class CMwCmdFastCallUser : CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC63BC; } }
        public class CMwCmdFiber : CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA154; } }
        public class CMwCmdFor : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0184; } }
        public class CMwCmdIf : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0254; } }
        public class CMwCmdInst : CMwCmdScript { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9C64; } }
        public class CMwCmdLog : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCB4C; } }
        public class CMwCmdProc : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBC54; } }
        public class CMwCmdReturn : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCBF34; } }
        public class CMwCmdScript : CMwCmd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9F6C; } }
        public class CMwCmdScriptVar : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCAF24; } }
        public class CMwCmdScriptVarBool : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDD74; } }
        public class CMwCmdScriptVarClass : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDCC4; } }
        public class CMwCmdScriptVarEnum : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCB23C; } }
        public class CMwCmdScriptVarFloat : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDB6C; } }
        public class CMwCmdScriptVarInt : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDC1C; } }
        public class CMwCmdScriptVarIso4 : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDAC4; } }
        public class CMwCmdScriptVarString : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDA1C; } }
        public class CMwCmdScriptVarVec2 : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD974; } }
        public class CMwCmdScriptVarVec3 : CMwCmdScriptVar { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCD74C; } }
        public class CMwCmdSleep : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCCD4; } }
        public class CMwCmdSwitch : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCE73C; } }
        public class CMwCmdSwitchType : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCDE1C; } }
        public class CMwCmdWait : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCCA84; } }
        public class CMwCmdWhile : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0324; } }
        public class CMwEngine : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC628C; } }
        public class CMwParamAction : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8944; } }
        public class CMwParamBool : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8A0C; } }
        public class CMwParamClass : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8AD4; } }
        public class CMwParamColor : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8D2C; } }
        public class CMwParamEnum : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8DF4; } }
        public class CMwParamInteger : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC904C; } }
        public class CMwParamIntegerRange : CMwParamInteger { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9114; } }
        public class CMwParamIso3 : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC968C; } }
        public class CMwParamIso4 : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC95C4; } }
        public class CMwParamMwId : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC94FC; } }
        public class CMwParamNatural : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8EBC; } }
        public class CMwParamNaturalRange : CMwParamNatural { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8F84; } }
        public class CMwParamProc : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9A74; } }
        public class CMwParamQuat : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC99AC; } }
        public class CMwParamReal : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC91DC; } }
        public class CMwParamRealRange : CMwParamReal { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC92A4; } }
        public class CMwParamRefBuffer : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8B9C; } }
        public class CMwParamString : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC936C; } }
        public class CMwParamStringInt : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9434; } }
        public class CMwParamStruct : CMwParam { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC8C64; } }
        public class CMwParamVec2 : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC98E4; } }
        public class CMwParamVec3 : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC981C; } }
        public class CMwParamVec4 : CMwParamStruct { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC9754; } }
        public class CMwRefBuffer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA484; } }
        public class CMwStatsValue : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BCA24C; } }
        public class CNetClient : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5441C; } }
        public class CNetClientInfo : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B539D4; } }
        public class CNetConnection : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53B34; } }
        public class CNetFileTransfer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53104; } }
        public class CNetFileTransferDownload : CNetFileTransferNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5244C; } }
        public class CNetFileTransferForm : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5273C; } }
        public class CNetFileTransferNod : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53514; } }
        public class CNetFileTransferUpload : CNetFileTransferNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53FA4; } }
        public class CNetFormConnectionAdmin : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5281C; } }
        public class CNetFormEnumSessions : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B528EC; } }
        public class CNetFormPing : CNetFormTimed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B48B24; } }
        public class CNetFormQuerrySessions : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B529A4; } }
        public class CNetFormRpcCall : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5268C; } }
        public class CNetFormTimed : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B48A6C; } }
        public class CNetHttpClient : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52A9C; } }
        public class CNetHttpResult : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B516FC; } }
        public class CNetIPC : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C40654; } }
        public class CNetIPSource : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52DBC; } }
        public class CNetMasterHost : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5220C; } }
        public class CNetMasterServer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B51C6C; } }
        public class CNetMasterServerInfo : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C407EC; } }
        public class CNetMasterServerUptoDateCheck : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B52BF4; } }
        public class CNetNod : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5152C; } }
        public class CNetServer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5479C; } }
        public class CNetServerInfo : CNetNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B522B4; } }
        public class CNetSource : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B537FC; } }
        public class CNetUPnP : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C408A4; } }
        public class CNetURLSource : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B53624; } }
        public class CNodSystem : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31C14; } }
        public class COalAudioBufferKeeper : CAudioBufferKeeper { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9CFD4; } }
        public class COalAudioPort : CAudioPort { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9B13C; } }
        public class COalDevice : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9AF44; } }
        public class CPlug : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE7FC; } }
        public class CPlugAudio : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD304; } }
        public class CPlugAudioEnvironment : CPlugAudio { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB24F4; } }
        public class CPlugBitmap : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BACF14; } }
        public class CPlugBitmapAddress : CPlugBitmapSampler { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB48B4; } }
        public class CPlugBitmapApply : CPlugBitmapAddress { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC8A4; } }
        public class CPlugBitmapHighLevel : CSystemNodWrapper { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBCB6C; } }
        public class CPlugBitmapPack : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBA84; } }
        public class CPlugBitmapPackElem : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB7CC; } }
        public class CPlugBitmapPacker : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBDD4; } }
        public class CPlugBitmapPackInput : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB704; } }
        public class CPlugBitmapRender : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB054C; } }
        public class CPlugBitmapRenderCamera : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADF8C; } }
        public class CPlugBitmapRenderCubeMap : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB884C; } }
        public class CPlugBitmapRenderHemisphere : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB2E4; } }
        public class CPlugBitmapRenderLightFromMap : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5C9C; } }
        public class CPlugBitmapRenderLightOcc : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB78B4; } }
        public class CPlugBitmapRenderOverlay : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB976C; } }
        public class CPlugBitmapRenderPlaneR : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAF54; } }
        public class CPlugBitmapRenderPortal : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAE54; } }
        public class CPlugBitmapRenderScene3d : CPlugBitmapRenderCamera { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA4B4; } }
        public class CPlugBitmapRenderShadow : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB0294; } }
        public class CPlugBitmapRenderSolid : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB01B4; } }
        public class CPlugBitmapRenderSub : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA244; } }
        public class CPlugBitmapRenderVDepPlaneY : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA40C; } }
        public class CPlugBitmapRenderWater : CPlugBitmapRender { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB85EC; } }
        public class CPlugBitmapSampler : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE65C; } }
        public class CPlugBitmapShader : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB848C; } }
        public class CPlugBlendShapes : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB474C; } }
        public class CPlugCrystal : CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBCC94; } }
        public class CPlugDecoratorSolid : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4BC4; } }
        public class CPlugDecoratorTree : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9614; } }
        public class CPlugFile : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC2AC; } }
        public class CPlugFileAvi : CPlugFileVideo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBFFC; } }
        public class CPlugFileBink : CPlugFileVideo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC54C; } }
        public class CPlugFileDds : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFE5C; } }
        public class CPlugFileFidCache : CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4D44; } }
        public class CPlugFileFidContainer : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAEE44; } }
        public class CPlugFileFont : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC0D4; } }
        public class CPlugFileGen : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFD84; } }
        public class CPlugFileGPU : CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB45D4; } }
        public class CPlugFileGpuFx : CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD564; } }
        public class CPlugFileGpuFxD3d : CPlugFileGpuFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA184; } }
        public class CPlugFileGPUP : CPlugFileGPU { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB218C; } }
        public class CPlugFileGPUV : CPlugFileGPU { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB099C; } }
        public class CPlugFileI18n : CPlugFileText { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB3C4; } }
        public class CPlugFileImg : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAD95C; } }
        public class CPlugFileJpg : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4AD4; } }
        public class CPlugFileModel : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD43C; } }
        public class CPlugFileModel3ds : CPlugFileModel { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9C9C; } }
        public class CPlugFileModelObj : CPlugFileModel { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9E34; } }
        public class CPlugFileOggVorbis : CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB0E4; } }
        public class CPlugFilePack : CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAECF4; } }
        public class CPlugFilePHlsl : CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB605C; } }
        public class CPlugFilePng : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBB6C; } }
        public class CPlugFilePsh : CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB50EC; } }
        public class CPlugFilePso : CPlugFileGPUP { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9D5C; } }
        public class CPlugFileSnd : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2E34; } }
        public class CPlugFileSndGen : CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB58F4; } }
        public class CPlugFileText : CPlugFile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4C84; } }
        public class CPlugFileTga : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB535C; } }
        public class CPlugFileVHlsl : CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB105C; } }
        public class CPlugFileVideo : CPlugFileImg { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5734; } }
        public class CPlugFileVsh : CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB8A4; } }
        public class CPlugFileVso : CPlugFileGPUV { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA0B4; } }
        public class CPlugFileWav : CPlugFileSnd { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5A7C; } }
        public class CPlugFileZip : CPlugFileFidContainer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC73C; } }
        public class CPlugFont : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADB8C; } }
        public class CPlugFontBitmap : CPlugFont { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB584; } }
        public class CPlugFurWind : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF04C; } }
        public class CPlugIndexBuffer : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5844; } }
        public class CPlugLight : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4E6C; } }
        public class CPlugMaterial : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE504; } }
        public class CPlugMaterialCustom : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAFADC; } }
        public class CPlugMaterialFx : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBD394; } }
        public class CPlugMaterialFxDynaBump : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA64C; } }
        public class CPlugMaterialFxDynaMobil : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA584; } }
        public class CPlugMaterialFxFlags : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA72C; } }
        public class CPlugMaterialFxFur : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA834; } }
        public class CPlugMaterialFxGenCV : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA314; } }
        public class CPlugMaterialFxGenUvProj : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA4D4; } }
        public class CPlugMaterialFxs : CPlugMaterialFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8EFC; } }
        public class CPlugModel : CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7D54; } }
        public class CPlugModelFences : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9304; } }
        public class CPlugModelFur : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9944; } }
        public class CPlugModelLodMesh : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB801C; } }
        public class CPlugModelMesh : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB826C; } }
        public class CPlugModelShell : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB01C; } }
        public class CPlugModelTree : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7E8C; } }
        public class CPlugMusic : CPlugMusicType { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE1CC; } }
        public class CPlugMusicType : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8E54; } }
        public class CPlugPointsInSphereOpt : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB0094; } }
        public class CPlugRessourceStrings : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBB484; } }
        public class CPlugShader : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF704; } }
        public class CPlugShaderApply : CPlugShaderGeneric { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF1BC; } }
        public class CPlugShaderGeneric : CPlugShader { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE314; } }
        public class CPlugShaderPass : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBABF4; } }
        public class CPlugShaderSprite : CPlugShaderApply { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAA5C; } }
        public class CPlugShaderSpritePath : CPlugShaderSprite { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBA95C; } }
        public class CPlugSolid : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BADD6C; } }
        public class CPlugSound : CPlugAudio { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2324; } }
        public class CPlugSoundEngine : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB59E4; } }
        public class CPlugSoundEngineComponent : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5B8C; } }
        public class CPlugSoundMood : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC314; } }
        public class CPlugSoundMulti : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBACD4; } }
        public class CPlugSoundSurface : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBAD9C; } }
        public class CPlugSoundVideo : CPlugSound { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4A34; } }
        public class CPlugSurface : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8CC4; } }
        public class CPlugSurfaceGeom : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8A44; } }
        public class CPlugTree : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAD5FC; } }
        public class CPlugTreeFrustum : CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7A34; } }
        public class CPlugTreeGenerator : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8D7C; } }
        public class CPlugTreeGenSolid : CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9F3C; } }
        public class CPlugTreeGenText : CPlugTreeGenerator { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE104; } }
        public class CPlugTreeLight : CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2A7C; } }
        public class CPlugTreeViewDep : CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2974; } }
        public class CPlugTreeVisualMip : CPlugTree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAF38C; } }
        public class CPlugVertexStream : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB20DC; } }
        public class CPlugViewDepLocator : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9484; } }
        public class CPlugVisual : CPlug { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAE87C; } }
        public class CPlugVisual2D : CPlugVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB9134; } }
        public class CPlugVisual3D : CPlugVisual { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2BEC; } }
        public class CPlugVisualGrid : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB7B8C; } }
        public class CPlugVisualHeightField : CPlugVisualGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC194; } }
        public class CPlugVisualIndexed : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8B4C; } }
        public class CPlugVisualIndexedLines : CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB2804; } }
        public class CPlugVisualIndexedStrip : CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB8FBC; } }
        public class CPlugVisualIndexedTriangles : CPlugVisualIndexed { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB1CE4; } }
        public class CPlugVisualIndexedTriangles2D : CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC9AC; } }
        public class CPlugVisualLines : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB4F7C; } }
        public class CPlugVisualLines2D : CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB5444; } }
        public class CPlugVisualOctree : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB276C; } }
        public class CPlugVisualPath : CPlugVisualGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBBC24; } }
        public class CPlugVisualQuads : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB25DC; } }
        public class CPlugVisualQuads2D : CPlugVisual2D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAEA7C; } }
        public class CPlugVisualSprite : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB1E9C; } }
        public class CPlugVisualStrip : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC3D4; } }
        public class CPlugVisualTriangles : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BB559C; } }
        public class CPlugVisualVertexs : CPlugVisual3D { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BBC544; } }
        public class CScene : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9ECDC; } }
        public class CScene2d : CScene { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E424; } }
        public class CScene3d : CScene { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EAC4; } }
        public class CSceneCamera : CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F4DC; } }
        public class CSceneConfig : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1984; } }
        public class CSceneConfigVision : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8B74; } }
        public class CSceneController : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0ECC; } }
        public class CSceneEngine : CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F63C; } }
        public class CSceneExtraFlocking : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7224; } }
        public class CSceneExtraFlockingCharacters : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA748C; } }
        public class CSceneField : CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1A84; } }
        public class CSceneFx : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA273C; } }
        public class CSceneFxBloom : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA90E4; } }
        public class CSceneFxBloomData : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8EF4; } }
        public class CSceneFxCameraBlend : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA52BC; } }
        public class CSceneFxColors : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1CD4; } }
        public class CSceneFxCompo : CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA77C; } }
        public class CSceneFxDepthOfField : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5F04; } }
        public class CSceneFxDistor2d : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F714; } }
        public class CSceneFxFlares : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA9B64; } }
        public class CSceneFxGrayAccum : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA53F4; } }
        public class CSceneFxHeadTrack : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8A94; } }
        public class CSceneFxMotionBlur : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA58BC; } }
        public class CSceneFxNod : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EBDC; } }
        public class CSceneFxOccZCmp : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4FE4; } }
        public class CSceneFxOverlay : CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5D74; } }
        public class CSceneFxStereoscopy : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA09CC; } }
        public class CSceneFxSuperSample : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA246C; } }
        public class CSceneFxToneMapping : CSceneFxCompo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8C14; } }
        public class CSceneFxVisionK : CSceneFx { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA64D4; } }
        public class CSceneGate : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2D44; } }
        public class CSceneLight : CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FA7C; } }
        public class CSceneListener : CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1894; } }
        public class CSceneLocation : CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA321C; } }
        public class CSceneLocationCamera : CSceneLocation { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA9EF4; } }
        public class CSceneMessageHandler : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2A6C; } }
        public class CSceneMobil : CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E5D4; } }
        public class CSceneMobilClouds : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FBB4; } }
        public class CSceneMobilFlockAttractor : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7664; } }
        public class CSceneMobilLeaves : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA04B4; } }
        public class CSceneMobilSnow : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9FE84; } }
        public class CSceneMobilTraffic : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA2E4C; } }
        public class CSceneMood : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA30E4; } }
        public class CSceneMoods : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA84FC; } }
        public class CSceneMotorbikeEnvMaterial : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB4D4; } }
        public class CSceneObject : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E814; } }
        public class CSceneObjectLink : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9E944; } }
        public class CScenePath : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F9E4; } }
        public class CScenePickerManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0834; } }
        public class CScenePoc : CSceneObject { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0FAC; } }
        public class CSceneSector : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA29B4; } }
        public class CSceneSoundManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3314; } }
        public class CSceneSoundSource : CScenePoc { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EDD4; } }
        public class CSceneToy : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA27F4; } }
        public class CSceneToyBird : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6D44; } }
        public class CSceneToyBoat : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA1464; } }
        public class CSceneToyBroomstick : CSceneToyCharacter { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA154; } }
        public class CSceneToyCharacter : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3B4C; } }
        public class CSceneToyCharacterDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA634; } }
        public class CSceneToyCharacterTuning : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA51A4; } }
        public class CSceneToyCharacterTunings : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA5234; } }
        public class CSceneToyDisplayGraph : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA872C; } }
        public class CSceneToyDisplayHistogram : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA858C; } }
        public class CSceneToyDisplayProgress : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA88D4; } }
        public class CSceneToyFilaments : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA784C; } }
        public class CSceneToyLeash : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6724; } }
        public class CSceneToyMotorbike : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7EDC; } }
        public class CSceneToyRock : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA99AC; } }
        public class CSceneToySea : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA0114; } }
        public class CSceneToySeaHoule : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA9AC; } }
        public class CSceneToySeaHouleFixe : CSceneToySeaHoule { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB5C4; } }
        public class CSceneToySeaHouleTable : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA884; } }
        public class CSceneToyStem : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA481C; } }
        public class CSceneToySubway : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6934; } }
        public class CSceneToyTrain : CSceneToy { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4B54; } }
        public class CSceneTrafficGraph : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA317C; } }
        public class CSceneTrafficPath : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA301C; } }
        public class CSceneVehicle : CSceneMobil { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9F314; } }
        public class CSceneVehicleBall : CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA7A54; } }
        public class CSceneVehicleBallTuning : CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA3E4; } }
        public class CSceneVehicleCar : CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B9EFFC; } }
        public class CSceneVehicleCarTuning : CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA376C; } }
        public class CSceneVehicleEmitter : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA6F4; } }
        public class CSceneVehicleEnvironment : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3A6C; } }
        public class CSceneVehicleGlider : CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA4DE4; } }
        public class CSceneVehicleGliderTuning : CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA8FC4; } }
        public class CSceneVehicleMaterial : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA39DC; } }
        public class CSceneVehicleMaterialGroup : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA59C; } }
        public class CSceneVehicleSpeedBoat : CSceneVehicle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA6FD4; } }
        public class CSceneVehicleSpeedBoatTuning : CSceneVehicleTuning { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAA07C; } }
        public class CSceneVehicleStruct : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3884; } }
        public class CSceneVehicleTuning : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB434; } }
        public class CSceneVehicleTunings : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BA3924; } }
        public class CSystemCmdAssert : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B318AC; } }
        public class CSystemCmdDuplicateNod : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31A1C; } }
        public class CSystemCmdExec : CMwCmdInst { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31964; } }
        public class CSystemCmdLoadNod : CMwCmdExpClass { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31B1C; } }
        public class CSystemConfig : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2F76C; } }
        public class CSystemConfigDisplay : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31324; } }
        public class CSystemData : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00C404B4; } }
        public class CSystemEngine : CMwEngine { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2E60C; } }
        public class CSystemFid : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2FCFC; } }
        public class CSystemFidBuffer : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31754; } }
        public class CSystemFidFile : CSystemFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B301BC; } }
        public class CSystemFidMemory : CSystemFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B300BC; } }
        public class CSystemFids : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B30544; } }
        public class CSystemFidsDrive : CSystemFidsFolder { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3067C; } }
        public class CSystemFidsFolder : CSystemFids { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B2FFB4; } }
        public class CSystemKeyboard : CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B321B4; } }
        public class CSystemMouse : CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31DDC; } }
        public class CSystemNodWrapper : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3180C; } }
        public class CSystemPackDesc : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B32EE4; } }
        public class CSystemPackManager : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B32E0C; } }
        public class CSystemWindow : CNodSystem { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B31CCC; } }
        public class CTrackMania : CGameCtnApp { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3E074; } }
        public class CTrackManiaControlCard : CGameControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3503C; } }
        public class CTrackManiaControlCheckPointList : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B378EC; } }
        public class CTrackManiaControlMatchSettingsCard : CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33154; } }
        public class CTrackManiaControlPlayerInfoCard : CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B35CAC; } }
        public class CTrackManiaControlPlayerInput : CGameControlPlayerInput { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50154; } }
        public class CTrackManiaControlRaceScoreCard : CTrackManiaControlCard { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B349F4; } }
        public class CTrackManiaControlScores : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33FFC; } }
        public class CTrackManiaControlScores2 : CControlFrame { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B33814; } }
        public class CTrackManiaEditor : CGameCtnEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4114C; } }
        public class CTrackManiaEditorCatalog : CTrackManiaEditorFree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B38734; } }
        public class CTrackManiaEditorFree : CTrackManiaEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CD94; } }
        public class CTrackManiaEditorIcon : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50E94; } }
        public class CTrackManiaEditorIconPage : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50AF4; } }
        public class CTrackManiaEditorInterface : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B47EF4; } }
        public class CTrackManiaEditorPuzzle : CTrackManiaEditorFree { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3C86C; } }
        public class CTrackManiaEditorSimple : CTrackManiaEditorPuzzle { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3348C; } }
        public class CTrackManiaEditorTerrain : CTrackManiaEditor { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CBF4; } }
        public class CTrackManiaEnvironmentManager : CGameEnvironmentManager { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5095C; } }
        public class CTrackManiaMatchSettings : CGameFid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B42DC4; } }
        public class CTrackManiaMatchSettingsControlGrid : CControlGrid { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B34CEC; } }
        public class CTrackManiaMenus : CGameCtnMenus { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3A64C; } }
        public class CTrackManiaNetForm : CGameNetForm { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B388EC; } }
        public class CTrackManiaNetwork : CGameCtnNetwork { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4935C; } }
        public class CTrackManiaNetworkServerInfo : CGameCtnNetServerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4231C; } }
        public class CTrackManiaPlayer : CGamePlayer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B5021C; } }
        public class CTrackManiaPlayerCameraSet : CGamePlayerCameraSet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B504C4; } }
        public class CTrackManiaPlayerInfo : CGamePlayerInfo { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B41FC4; } }
        public class CTrackManiaPlayerProfile : CGamePlayerProfile { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B346DC; } }
        public class CTrackManiaRace : CGameRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3D0D4; } }
        public class CTrackManiaRace1P : CTrackManiaRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CA0C; } }
        public class CTrackManiaRace1PGhosts : CTrackManiaRace1P { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37C2C; } }
        public class CTrackManiaRace2PTurnBased : CTrackManiaRace1P { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B383B4; } }
        public class CTrackManiaRaceAnalyzer : CGameAnalyzer { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B50084; } }
        public class CTrackManiaRaceInterface : CGameRaceInterface { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4586C; } }
        public class CTrackManiaRaceNet : CTrackManiaRace { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B4345C; } }
        public class CTrackManiaRaceNetLaps : CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3760C; } }
        public class CTrackManiaRaceNetRounds : CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B389CC; } }
        public class CTrackManiaRaceNetTimeAttack : CTrackManiaRaceNet { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37E04; } }
        public class CTrackManiaRaceScore : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3451C; } }
        public class CTrackManiaReplayRecord : CGameCtnReplayRecord { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B37544; } }
        public class CTrackManiaSwitcher : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00B3CF2C; } }
        public class CVisionResourceFile : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0B64; } }
        public class CVisionViewport : CHmsViewport { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD0904; } }
        public class CVisionViewportDx9 : CVisionViewport { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BD10BC; } }
        public class CXmlAttribute : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5D64; } }
        public class CXmlComment : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5DFC; } }
        public class CXmlDeclaration : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5BFC; } }
        public class CXmlDocument : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC59EC; } }
        public class CXmlElement : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5A9C; } }
        public class CXmlNod : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5CB4; } }
        public class CXmlText : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC5B4C; } }
        public class CXmlUnknown : CXmlNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BC594C; } }
        public class GxFog : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABA64; } }
        public class GxFogBlender : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABB2C; } }
        public class GxLight : CMwNod { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB6DC; } }
        public class GxLightAmbient : GxLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABBEC; } }
        public class GxLightBall : GxLightPoint { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABCF4; } }
        public class GxLightDirectional : GxLightNotAmbient { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB89C; } }
        public class GxLightFrustum : GxLightBall { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BABF44; } }
        public class GxLightNotAmbient : GxLight { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAB924; } }
        public class GxLightPoint : GxLightNotAmbient { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC1DC; } }
        public class GxLightSpot : GxLightBall { public static class Offsets { public static IntPtr VTable = (IntPtr)0x00BAC0B4; } }
    }
}
