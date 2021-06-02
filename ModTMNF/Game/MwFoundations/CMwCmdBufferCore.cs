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

        public static void DestroyCoreCmdBuffer()
        {
            FT.CMwCmdBufferCore.DestroyCoreCmdBuffer();
        }

        public static void CreateCoreCmdBuffer()
        {
            FT.CMwCmdBufferCore.CreateCoreCmdBuffer();
        }

        public static void ForceFpuCwForSimulationX86(int flags)
        {
            FT.CMwCmdBufferCore.ForceFpuCwForSimulationX86(flags);
        }

        public void StopSimulation()
        {
            FT.CMwCmdBufferCore.StopSimulation(this);
        }

        public void SetIsSimulationOnly(bool value)
        {
            FT.CMwCmdBufferCore.SetIsSimulationOnly(this, value);
        }

        /// <summary>
        /// Speed up / slow down the game
        /// </summary>
        public void SetSimulationRelativeSpeed(float speed)
        {
            FT.CMwCmdBufferCore.SetSimulationRelativeSpeed(this, speed);
        }

        public void EnableFixedTickTime(int unk1, int unk2, int unk3)
        {
            FT.CMwCmdBufferCore.EnableFixedTickTime(this, unk1, unk2, unk3);
        }

        public void HighFrequencyAddCmd(IntPtr objPtr, IntPtr funcPtr)
        {
            FT.CMwCmdBufferCore.HighFrequencyAddCmd(this, objPtr, funcPtr);
        }

        public void HighFrequencyRun(int unk1)
        {
            FT.CMwCmdBufferCore.HighFrequencyRun(this, unk1);
        }

        public void HighFrequencyEnterSafeSection(int unk1)
        {
            FT.CMwCmdBufferCore.HighFrequencyEnterSafeSection(this, unk1);
        }

        public void HighFrequencyLeaveSafeSection()
        {
            FT.CMwCmdBufferCore.HighFrequencyLeaveSafeSection(this);
        }

        public void HighFrequencyYield(int unk1)
        {
            FT.CMwCmdBufferCore.HighFrequencyYield(this, unk1);
        }

        // TODO:
        // SetSchemePatternsProperties
        // GetSchemeProperies
        // GetSchemePeriod / EMwSchemeTimedPatterns
        // SnapTimeToSchemePeriod
        // SnapTimeToPreviousSchemePeriod

        public void Run()
        {
            FT.CMwCmdBufferCore.Run(this);
        }

        public void Enable()
        {
            FT.CMwCmdBufferCore.Enable(this);
        }

        public void StartSimulation(int unk1, int unk2, float speed)
        {
            FT.CMwCmdBufferCore.StartSimulation(this, unk1, unk2, speed);
        }

        public void SetSimulationCurrentTime(int tickMs)
        {
            FT.CMwCmdBufferCore.SetSimulationCurrentTime(this, tickMs);
        }

        public void EnableFixedTickFrequency(int unk1, int unk2)
        {
            FT.CMwCmdBufferCore.EnableFixedTickFrequency(this, unk1, unk2);
        }

        public void InitCmdBuffer()
        {
            FT.CMwCmdBufferCore.InitCmdBuffer(this);
        }

        public CMwCmdFastCall AddNotifySetTime(IntPtr objPtr, IntPtr funcPtr)
        {
            return FT.CMwCmdBufferCore.AddNotifySetTime(this, objPtr, funcPtr);
        }

        public void SubNotifySetTime(IntPtr objPtr, IntPtr funcPtr)
        {
            FT.CMwCmdBufferCore.SubNotifySetTime(this, objPtr, funcPtr);
        }

        public void HighFrequencySubCmd(IntPtr objPtr, IntPtr funcPtr)
        {
            FT.CMwCmdBufferCore.HighFrequencySubCmd(this, objPtr, funcPtr);
        }
    }
}
