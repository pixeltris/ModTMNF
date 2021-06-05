using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    // Also see SInputActionDesc::EActionType
    public unsafe struct SInputActionDesc
    {
        public IntPtr Address;

        public SInputActionDesc(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SInputActionDesc(IntPtr address)
        {
            return new SInputActionDesc(address);
        }

        public int Unk_0
        {
            get { return *(int*)(Address + OT.SInputActionDesc.Unk_0); }
        }

        /// <summary>
        /// Type?
        /// </summary>
        public int Unk_4
        {
            get { return *(int*)(Address + OT.SInputActionDesc.Unk_4); }
        }

        public string Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SInputActionDesc.Name)); }
        }

        /// <summary>
        /// Often -1
        /// </summary>
        public int Unk_12
        {
            get { return *(int*)(Address + OT.SInputActionDesc.Unk_12); }
        }

        /// <summary>
        /// 0?
        /// </summary>
        public int Unk_16
        {
            get { return *(int*)(Address + OT.SInputActionDesc.Unk_16); }
        }

        public static SInputActionDesc ActionReset { get { return *(IntPtr*)ST.SInputActionDesc.ActionReset; } }
        public static SInputActionDesc ActionRespawn_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionRespawn_1; } }
        public static SInputActionDesc ActionVehicleAccelerate_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleAccelerate_1; } }
        public static SInputActionDesc ActionVehicleBrake_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleBrake_1; } }
        public static SInputActionDesc ActionVehicleGas_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleGas_1; } }
        public static SInputActionDesc ActionVehicleSteerLeft_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleSteerLeft_1; } }
        public static SInputActionDesc ActionVehicleSteerRight_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleSteerRight_1; } }
        public static SInputActionDesc ActionVehicleSteer_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleSteer_1; } }
        public static SInputActionDesc ActionVehicleHorn_1 { get { return *(IntPtr*)ST.SInputActionDesc.ActionVehicleHorn_1; } }
    }
}
