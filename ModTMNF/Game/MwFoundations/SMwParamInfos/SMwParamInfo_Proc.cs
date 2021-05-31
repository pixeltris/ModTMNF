using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct SMwParamInfo_Proc
    {
        public IntPtr Address;

        public SMwParamInfo Base
        {
            get { return Address; }
        }

        public SMwParamInfo_Proc(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo_Proc(IntPtr address)
        {
            return new SMwParamInfo_Proc(address);
        }

        public IntPtr FuncPtr
        {
            get { return *(IntPtr*)(Address + OT.SMwParamInfo_Proc.FuncPtr); }
        }

        public int NumArgs
        {
            get { return *(int*)(Address + OT.SMwParamInfo_Proc.ArgCount); }
        }

        public Arg[] Args
        {
            get
            {
                IntPtr classIdsPtr = *(IntPtr*)(Address + OT.SMwParamInfo_Proc.ArgClassIds);
                IntPtr flagsPtr = *(IntPtr*)(Address + OT.SMwParamInfo_Proc.ArgFlags);
                IntPtr namesPtr = *(IntPtr*)(Address + OT.SMwParamInfo_Proc.ArgNames);
                Arg[] result = new Arg[NumArgs];
                if (classIdsPtr != IntPtr.Zero)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i].ClassId = *(EMwClassId*)(classIdsPtr + (i * sizeof(int)));
                    }
                }
                if (flagsPtr != IntPtr.Zero)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i].Flags = *(int*)(flagsPtr + (i * sizeof(int)));
                    }
                }
                if (namesPtr != IntPtr.Zero)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i].Name = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(namesPtr + (i * sizeof(IntPtr))));
                    }
                }
                return result;
            }
        }

        public struct Arg
        {
            public EMwClassId ClassId;
            public string Name;
            public int Flags;//SMwParamInfo::SProcArgType::EType_Param

            public override string ToString()
            {
                return "{name:" + Name + ",classid:" + ClassId + ",flags:" + Flags + "}";
            }
        }
    }
}
