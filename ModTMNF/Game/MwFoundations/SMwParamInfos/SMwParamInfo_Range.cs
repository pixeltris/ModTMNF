using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Range
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Range(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Range(IntPtr address)
        {
            return new SMwParamInfo_Range(address);
        }

        public int Unk1
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Range.Unk1); }
        }

        public int Unk2
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Range.Unk2); }
        }
    }
}
