using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Class
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Class(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Class(IntPtr address)
        {
            return new SMwParamInfo_Class(address);
        }

        public CMwClassInfo ClassInfo
        {
            get { return *(IntPtr*)(Address + OT.SMwParamInfo_Class.ClassInfo); }
        }

        /// <summary>
        /// This seems to be a boolean? (only seen this as 0 or 1)
        /// </summary>
        public int Unk1
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Class.Unk1); }
        }
    }
}
