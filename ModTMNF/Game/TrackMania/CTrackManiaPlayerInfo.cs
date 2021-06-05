using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaPlayerInfo
    {
        public IntPtr Address;

        public CGamePlayerInfo Base
        {
            get { return new CGamePlayerInfo(Address); }
        }

        public CTrackManiaPlayerInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaPlayerInfo(IntPtr address)
        {
            return new CTrackManiaPlayerInfo(address);
        }

        public void AddInputEvent(SInputEvent inputEvent, uint time)
        {
            FT.CTrackManiaPlayerInfo.AddInputEvent(this, ref inputEvent, time);
        }

        public void SetInputState(SInputEvent inputEvent, uint time)
        {
            FT.CTrackManiaPlayerInfo.SetInputState(this, ref inputEvent, time);
        }

        public int GetInputState(SInputActionDesc action, uint time)
        {
            SMwTimedValueInstant result = default(SMwTimedValueInstant);
            FT.CTrackManiaPlayerInfo.GetInputState(this, action, time, ref result);
            return result.Value;
        }

        public float GetInputStateF(SInputActionDesc action, uint time)
        {
            SMwTimedValueInstant result = default(SMwTimedValueInstant);
            FT.CTrackManiaPlayerInfo.GetInputState(this, action, time, ref result);
            return result.FloatValue;
        }

        public void LockInputs(uint time)
        {
            FT.CTrackManiaPlayerInfo.LockInputs(this, time);
        }

        public void UnlockInptus()
        {
            LockInputs(uint.MaxValue);
        }
    }
}
