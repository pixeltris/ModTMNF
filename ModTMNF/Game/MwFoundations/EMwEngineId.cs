using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// Also see EMwClassId
    /// 
    /// These are essentially bit flags for EMwClassId
    /// 
    /// These were dumped by traversing engines in CMwEngineManager::Engines
    /// </summary>
    public enum EMwEngineId : uint
    {
        Invalid = 0x00000000,// This is an actual entry in memory, but has no name. I assume this is a null/invalid value.
        MwFoundations = 0x01000000,
        Game = 0x03000000,
        Graphic = 0x04000000,
        Function = 0x05000000,
        Hms = 0x06000000,
        Control = 0x07000000,
        Motion = 0x08000000,
        Plug = 0x09000000,
        Scene = 0x0A000000,
        System = 0x0B000000,
        Vision = 0x0C000000,
        Audio = 0x10000000,
        Net = 0x12000000,
        Input = 0x13000000,
        Xml = 0x14000000,
        TrackMania = 0x24000000,
    }
}
