using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Array
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Array(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Array(IntPtr address)
        {
            return new SMwParamInfo_Array(address);
        }

        public int Unk1
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Array.Unk1); }
        }

        /// <summary>
        /// Type name. For classes this differs to the C++ class name e.g. "CMwCmdBlockCast"->"Block"
        /// </summary>
        public string TypeName
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Array.TypeName)); }
        }

        public int Unk2
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Array.Unk2); }
        }

        public CMwClassInfo ClassInfo
        {
            get { return *(IntPtr*)(Address + OT.SMwParamInfo_Array.ClassInfo); }
        }
    }
}
