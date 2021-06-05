using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaControlPlayerInput
    {
        public IntPtr Address;

        public CGameControlPlayerInput Base
        {
            get { return new CGameControlPlayerInput(Address); }
        }

        public CTrackManiaControlPlayerInput(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaControlPlayerInput(IntPtr address)
        {
            return new CTrackManiaControlPlayerInput(address);
        }

        public void UpdateVehicleStateFromInputs(ref SRaceInputs inputs)
        {
            FT.CTrackManiaControlPlayerInput.UpdateVehicleStateFromInputs(this, ref inputs);
        }

        public static void UpdateVehicleStateFromInputs(ref SRaceInputs inputs, CSceneMobil mobil)
        {
            FT.CTrackManiaControlPlayerInput.UpdateVehicleStateFromInputsImpl(ref inputs, mobil);
        }

        public unsafe struct SRaceInputs
        {
            public SMwTimedValueInstant SteerLeft;
            public SMwTimedValueInstant SteerRight;
            public SMwTimedValueInstant Steer;
            public SMwTimedValueInstant Accelerate;
            public SMwTimedValueInstant Brake;
            public SMwTimedValueInstant Gas;
        }
    }
}
