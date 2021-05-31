using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Vec4
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Vec4(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Vec4(IntPtr address)
        {
            return new SMwParamInfo_Vec4(address);
        }

        /// <summary>
        /// Name of the first variable (e.g. "x")
        /// </summary>
        public string Var1Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec4.Var1Name)); }
        }

        /// <summary>
        /// Name of the second variable (e.g. "y")
        /// </summary>
        public string Var2Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec4.Var2Name)); }
        }

        /// <summary>
        /// Name of the second variable (e.g. "z")
        /// </summary>
        public string Var3Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec4.Var3Name)); }
        }

        /// <summary>
        /// Name of the second variable (e.g. "w")
        /// </summary>
        public string Var4Name
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Vec4.Var4Name)); }
        }
    }
}
