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

        public static class CGbxApp
        {
            public static IntPtr TheApp = (IntPtr)0x00D66FF8;
        }

        public static class CGameApp
        {
            public static IntPtr s_TheGame = (IntPtr)0x00D68C44;
        }

        public static class CSystemConfig
        {
            public static IntPtr s_SystemConfig = (IntPtr)0x00D54380;
        }

        public static class CMwEngineMain
        {
            public static IntPtr TheMainEngine = (IntPtr)0x00D73300;
        }

        public static class CSystemWindow
        {
            public static IntPtr s_ScreenSizeX = (IntPtr)0x00D558D8;
            public static IntPtr s_ScreenSizeY = (IntPtr)0x00D558DC;
        }

        public static class CMwCmdBufferCore
        {
            public static IntPtr TheCoreCmdBuffer = (IntPtr)0x00D731E0;
        }
    }
}
