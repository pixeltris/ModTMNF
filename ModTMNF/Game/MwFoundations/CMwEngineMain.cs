using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// Manages all engines in the game. CMwEngine instances are used to help instantiate classes for their engine.
    /// </summary>
    public unsafe struct CMwEngineMain
    {
        public IntPtr Address;

        public CMwEngine Base
        {
            get { return new CMwEngine(Address); }
        }

        public CMwEngineMain(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwEngineMain(IntPtr address)
        {
            return new CMwEngineMain(address);
        }

        /// <summary>
        /// CMwEngineMain* CMwEngineMain::TheMainEngine
        /// 
        /// CGbxApp::Init() creates this
        /// </summary>
        public static CMwEngineMain TheMainEngine
        {
            get { return *(IntPtr*)ST.CMwEngineMain.TheMainEngine; }
        }

        public CFastArray<CMwEngine> Engines
        {
            get { return new CFastArray<CMwEngine>(Address + OT.CMwEngineMain.Engines); }
        }

        public CFastBuffer UnkBuffer
        {
            get { return new CFastBuffer(Address + OT.CMwEngineMain.UnkBuffer); }
        }

        public CMwEngine GetEngine(EMwEngineId engineId)
        {
            int index = CMwEngineManager.LongToShortEngineId((int)engineId);
            return Engines[index];
        }
    }
}
