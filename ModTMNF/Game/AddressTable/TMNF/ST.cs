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
            public static IntPtr s_WindowClassName = (IntPtr)0x00D67008;// CFastStringInt
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

        public static class SMwSchemeTimedProperties
        {
            // CMwCmdBufferCore::SetSchemePatternsProperties. offset +200 of CMwCmdBufferCore
            public const int StructSize = 56;
            public static IntPtr Instance = (IntPtr)0x00B3DC40;
        }

        public static class SInputActionDesc
        {
            /// <summary>
            /// First/last action address for each action list
            /// </summary>
            public static KeyValuePair<IntPtr, IntPtr>[] AllActions = new KeyValuePair<IntPtr, IntPtr>[]
            {
                new KeyValuePair<IntPtr, IntPtr>((IntPtr)0x00CD3F58, (IntPtr)0x00CD4010),// CTrackManiaRace
                new KeyValuePair<IntPtr, IntPtr>((IntPtr)0x00D1FC58, (IntPtr)0x00D1FEAC),// SInputConstants
                new KeyValuePair<IntPtr, IntPtr>((IntPtr)0x00D71408, (IntPtr)0x00D71444),// SInputConstants (second list)
            };
            // CTrackManiaRace actions we need for injecting input
            public static IntPtr ActionReset = (IntPtr)0x00CD3F5C;
            public static IntPtr ActionRespawn_1 = (IntPtr)0x00CD3F60;
            public static IntPtr ActionVehicleAccelerate_1 = (IntPtr)0x00CD3F64;
            public static IntPtr ActionVehicleBrake_1 = (IntPtr)0x00CD3F68;
            public static IntPtr ActionVehicleGas_1 = (IntPtr)0x00CD3F6C;
            public static IntPtr ActionVehicleSteerLeft_1 = (IntPtr)0x00CD3F70;
            public static IntPtr ActionVehicleSteerRight_1 = (IntPtr)0x00CD3F74;
            public static IntPtr ActionVehicleSteer_1 = (IntPtr)0x00CD3F78;
            public static IntPtr ActionVehicleHorn_1 = (IntPtr)0x00CD3F7C;
        }

        public static class CFastStringBase
        {
            public static IntPtr s_EmptyChars = (IntPtr)0x00BBF7D8;
            public static IntPtr s_EmptyCharsInt = (IntPtr)0x00BBF7DC;
        }
    }
}
