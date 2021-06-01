using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // CGbxApp::Init
    //  CMwCmdBufferCore::CreateCoreCmdBuffer
    //  CMwCmdBufferCore::InitCmdBuffer
    public unsafe struct CMwCmdBufferCore
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwCmdBufferCore(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdBufferCore(IntPtr address)
        {
            return new CMwCmdBufferCore(address);
        }

        public static CMwCmdBufferCore TheCoreCmdBuffer
        {
            get { return *(IntPtr*)ST.CMwCmdBufferCore.TheCoreCmdBuffer; }
        }

        public void StopSimulation()
        {
            FT.CMwCmdBufferCore.StopSimulation(this);
        }

        /// <summary>
        /// Speed up / slow down the game
        /// </summary>
        public void SetSimulationRelativeSpeed(float speed)
        {
            FT.CMwCmdBufferCore.SetSimulationRelativeSpeed(this, speed);
        }
    }
}
