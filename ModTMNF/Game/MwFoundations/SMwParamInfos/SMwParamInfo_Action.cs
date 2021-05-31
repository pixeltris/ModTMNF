using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Action
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Action(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Action(IntPtr address)
        {
            return new SMwParamInfo_Action(address);
        }

        public int Unk1
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Action.Unk1); }
        }
    }
}
