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
            public static int StructSize = 2168;// GUESSED (last accessed offset in ctor is 2108, first offset accessed in CTrackManiaMenus is 2168)
        }

        public static class CTrackManiaMenus
        {
            public static Type BaseType = typeof(OT.CGameCtnMenus);
            public static int StructSize = 2920;
            public static int NbPlayers = 2272;// type:Enum
            public static int TimeLimit = 2276;// default:15000 (15 seconds)
            public static int Rounds = 2280;// default:5
        }

        public static class CGbxApp
        {
            public static Type BaseType = typeof(OT.CMwNod);// Not part of reflection system
            public static int SystemConfig = 48;// type:CSystemConfig
            public static int UnkString52 = 52;// type:CFastStringInt
            public static int UnkString60 = 60;// type:CFastString
            public static int UnkString68 = 68;// type:CFastStringInt
            public static int WindowHandle = 80;
            public static int SystemWindow = 88;// type:CSystemWindow
            public static int SystemWindow2 = 92;// type:CSystemWindow
            public static int Viewport = 96;// type:CHmsViewport
            public static int InputPort = 100;// type:CInputPort
            public static int WindowTitle = 124;// type:CFastStringInt
            public static int ConsoleEnabled = 172;// type:bool
            public static int LoggingDisabled = 180;// type:bool
            public static int WindowFocusIfExisting = 184;// type:bool
            public static int RegistryKeyName = 188;// type:CFastString
            public static int UnkString196 = 196;// type:CFastString
            public static int UnkString204 = 204;// type:CFastStringInt
            public static int UnkString212 = 212;// type:CFastString
            public static int UnkString220 = 220;// type:CFastStringInt
            public static int UnkString228 = 228;// type:CFastStringInt
            public static int FileLog = 236;// type:CSystemFile
            public static int GameApp = 252;// type:CGameApp
        }

        public static class CGbxGame
        {
            public static Type BaseType = typeof(OT.CGbxApp);
            public static int StructSize = 324;
            public static int UnkString264 = 264;// type:CFastStringInt
            public static int UnkString272 = 272;// type:CFastStringInt
            public static int UnkString280 = 280;// type:CFastStringInt
            public static int UnkString288 = 288;// type:CFastStringInt
            public static int UnkString296 = 296;// type:CFastStringInt
            public static int UnkString304 = 304;// type:CFastStringInt
            public static int UnkString316 = 316;// type:CFastString
        }

        public static class CMwCmd
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 28;
            public static int Buffer = 20;// type:CMwCmdBuffer
            public static int State = 24;
        }

        public static class CMwCmdFastCall
        {
            public static Type BaseType = typeof(OT.CMwCmd);
            public static int StructSize = 36;
            public static int ObjPtr = 28;
            public static int FuncPtr = 32;
        }

        public static class CMwCmdFastCallUser
        {
            public static Type BaseType = typeof(OT.CMwCmd);
            public static int StructSize = 40;
            public static int ObjPtr = 28;
            public static int FuncPtr = 32;
            public static int Unk1 = 36;
        }

        public static class CMwCmdContainer
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 36;
            public static int Cmds = 20;// type:CFastBuffer
            public static int Installed = 32;
        }

        public static class CGameProcess
        {
            public static Type BaseType = typeof(OT.CMwCmdContainer);
            public static int StructSize = 36;
        }

        public static class CGameApp
        {
            public static Type BaseType = typeof(OT.CGameProcess);
            public static int StructSize = 404;
            public static int SystemConfig = 120;// type:CSystemConfig
        }

        public static class CSystemConfig
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 464;
        }

        public static class CHmsViewport
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 1384;// GUESSED (last accessed offset in ctor is 1368 (CFastBuffer), first offset accessed in CVisionViewport is 1384)
            public static int IsFullScreen = 668;
        }

        public static class CVisionViewport
        {
            public static Type BaseType = typeof(OT.CHmsViewport);
            public static int StructSize = 2040;
        }

        public static class CMwEngine
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 32;
            public static int Id = 20;
            public static int Groups = 24;// type:CFastBuffer
        }

        public static class CMwEngineMain
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 52;
            public static int Engines = 32;// type:CFastArray
            public static int UnkBuffer = 40;// type:CFastBuffer
        }

        public static class CVisionEngine
        {
            public static Type BaseType = typeof(OT.CMwEngine);
            public static int StructSize = 48;
            public static int WindowHandle = 32;
            public static int Viewports = 36;// type:CFastBuffer
        }

        public static class CSystemEngine
        {
            public static Type BaseType = typeof(OT.CMwEngine);
            public static int StructSize = 112;
            public static int DialogManager = 108;// type:CSystemDialogManager
        }

        public static class CNodSystem
        {
            public static Type BaseType = typeof(OT.CMwNod);
            public static int StructSize = 20;
        }

        public static class CSystemWindow
        {
            public static Type BaseType = typeof(OT.CNodSystem);
            public static int StructSize = 148;
            public static int SubWindow = 28;// type:CSystemWindow
            public static int SizeX = 32;
            public static int SizeY = 36;
            public static int WindowHandle = 144;
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
}