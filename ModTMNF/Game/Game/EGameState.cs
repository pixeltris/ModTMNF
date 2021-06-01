using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// CTrackMania::GetCurrentState
    /// TmDbg_GameStateToText
    /// </summary>
    public enum EGameState
    {
        None = 0,
        GameNet_Menus = 0x1,
        GameNet_RoundPrepare = 0x2,
        GameNet_RoundPlay = 0x4,
        GameNet_RoundExit = 0x8,
        StartUp = 0x10,
        Menus = 0x20,
        Quit = 0x40,
        Local_Init = 0x80,
        Local_Editor = 0x100,
        Local_Race = 0x200,
        Local_RaceEndDialog = 0x400,
        //??? = 0x800,
        Local_Replay = 0x1000,
        Local_End = 0x2000,
        Net_Sync = 0x4000,
        Net_Playing = 0x8000,
        Net_ExitRound = 0x10000,
    }
}
