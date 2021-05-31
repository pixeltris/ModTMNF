using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Vec2
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Vec2(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Vec2(IntPtr address)
        {
            return new SMwParamInfo_Vec2(address);
        }

        /// <summary>
        /// Name of the first variable (e.g. "x")
        /// </summary>
        public string Var1Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec2.Var1Name)); }
        }

        /// <summary>
        /// Name of the second variable (e.g. "y")
        /// </summary>
        public string Var2Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec2.Var2Name)); }
        }
    }
}
