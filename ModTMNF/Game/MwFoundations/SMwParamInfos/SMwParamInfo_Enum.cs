using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Enum
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Enum(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Enum(IntPtr address)
        {
            return new SMwParamInfo_Enum(address);
        }

        /// <summary>
        /// The C++ name of the enum (often null / empty)
        /// Includes the scope e.g. EGxBlendFactor::
        /// </summary>
        public string CppName
        {
            get { return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + OT.SMwParamInfo_Enum.CppName)); }
        }

        /// <summary>
        /// Enum entries. These names can break the standards of C++ coding conventions.
        /// </summary>
        public StringPtrArray ValueNames
        {
            get
            {
                return new StringPtrArray(
                    *(IntPtr*)(Address + OT.SMwParamInfo_Enum.ValueNames),
                    *(int*)(Address + OT.SMwParamInfo_Enum.ValueNamesCount));
            }
        }
    }
}
