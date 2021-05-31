using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// Base class for all classes in the game.
    /// </summary>
    public unsafe struct CMwNod
    {
        public IntPtr Address;

        public CMwNod(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwNod(IntPtr address)
        {
            return new CMwNod(address);
        }

        /// <summary>
        /// See CMwNod::MwAddRef / CMwNod::MwForceRef
        /// </summary>
        public int RefCount
        {
            get { return *(int*)(Address + OT.CMwNod.RefCount); }
        }

        public CSystemFid Fid
        {
            get { return *(IntPtr*)(Address + OT.CMwNod.Fid); }
        }

        public CFastBuffer<CMwNod> Dependants
        {
            get { return new CFastBuffer<CMwNod>(Address + OT.CMwNod.Dependants); }
        }

        public int Unk1
        {
            get { return *(int*)(Address + OT.CMwNod.Unk1); }
        }

        public CMwClassInfo MwGetClassInfo()
        {
            return VT.Get<VT.CMwNod>(Address).MwGetClassInfo(this);
        }

        public EMwClassId GetMwClassId()
        {
            return VT.Get<VT.CMwNod>(Address).GetMwClassId(this);
        }

        public bool MwIsKindOf(EMwClassId classId)
        {
            return VT.Get<VT.CMwNod>(Address).MwIsKindOf(this, classId);
        }

        public CMwIdPtr MwGetId()
        {
            return VT.Get<VT.CMwNod>(Address).MwGetId(this);
        }

        public void SetIdName(string idName)
        {
            VT.Get<VT.CMwNod>(Address).SetIdName(this, idName);
        }

        public void CopyFrom(CMwNod other)
        {
            VT.Get<VT.CMwNod>(Address).CopyFrom(this, other);
        }

        public int Param_Get(CMwStack stack, CMwValueStd value)
        {
            return FT.CMwNod.Param_Get(this, stack, value);
        }

        public int Param_Set(CMwStack stack, IntPtr value)
        {
            return FT.CMwNod.Param_Set(this, stack, value);
        }

        public int Param_Set(ref CFastString unk1, ref CFastStringInt unk2)
        {
            return FT.CMwNod.Param_SetStr(this, ref unk1, ref unk2);
        }

        public int Param_Add(CMwStack stack, IntPtr value)
        {
            return FT.CMwNod.Param_Add(this, stack, value);
        }

        public int Param_Sub(CMwStack stack, IntPtr value)
        {
            return FT.CMwNod.Param_Sub(this, stack, value);
        }

        public int Param_Check(CMwStack stack, IntPtr value)
        {
            return FT.CMwNod.Param_Check(this, stack);
        }

        public int MwAddRef()
        {
            return FT.CMwNod.MwAddRef(this);
        }

        public int MwRelease()
        {
            return FT.CMwNod.MwRelease(this);
        }

        public int MwForceRef(int newRefCount)
        {
            return FT.CMwNod.MwForceRef(this, newRefCount);
        }

        public void MwAddDependant(CMwNod other)
        {
            FT.CMwNod.MwAddDependant(this, other);
        }

        public void MwAddReceiver(CMwNod other)
        {
            FT.CMwNod.MwAddReceiver(this, other);
        }

        public void MwSubDependant(CMwNod other)
        {
            FT.CMwNod.MwSubDependant(this, other);
        }

        public void MwSubDependantSafe(CMwNod other)
        {
            FT.CMwNod.MwSubDependantSafe(this, other);
        }

        public void MwFinalSubDependant(CMwNod other)
        {
            FT.CMwNod.MwSubDependant(this, other);
        }

        public void DependantSendMwIsKilled(CMwNod other)
        {
            FT.CMwNod.DependantSendMwIsKilled(this, other);
        }

        public uint GetMwParamIdForRecursiveIndex(int index)
        {
            return FT.CMwNod.GetMwParamIdForRecursiveIndex(this, index);
        }

        public int GetMwParamIdRecursiveCount(int paramId)
        {
            return FT.CMwNod.GetMwParamIdRecursiveCount(this, paramId);
        }

        public int MwGetNearestFather(int unk1, ref int unk2)
        {
            return FT.CMwNod.MwGetNearestFather(this, unk1, ref unk2);
        }

        public static SMwParamInfo GetParamInfoFromParamId(uint paramId)
        {
            return FT.CMwNod.GetParamInfoFromParamId(paramId);
        }

        public static CMwNod CreateByMwClassId(EMwClassId classId)
        {
            return FT.CMwNod.CreateByMwClassId(classId);
        }

        public static CMwClassInfo StaticGetClassInfo(EMwClassId classId)
        {
            return FT.CMwNod.StaticGetClassInfo(classId);
        }

        public static bool StaticMwIsKindOf(EMwClassId classA, EMwClassId classB)
        {
            return FT.CMwNod.StaticMwIsKindOf(classA, classB);
        }

        //////////////////////////////////////////////////////////
        // More param functions (see EMwParamType)
        // TODO: Finish these (including Set,Sub,Add,etc)
        //////////////////////////////////////////////////////////

        public bool Param_GetBool(SMwParamInfo param)
        {
            return Param_GetInt(param) != 0;
        }

        public int Param_GetInt(SMwParamInfo param)
        {
            int result;
            Param_GetX(param, &result, sizeof(int));
            return result;
        }

        public uint Param_GetUInt(SMwParamInfo param)
        {
            return (uint)Param_GetInt(param);
        }

        public float Param_GetFloat(SMwParamInfo param)
        {
            float result;
            Param_GetX(param, &result, sizeof(float));
            return result;
        }

        public string Param_GetString(SMwParamInfo param)
        {
            CString result;
            Param_GetX(param, &result, sizeof(CString));
            string str = result.Value;
            //result.Delete();// Not a copy of the string I guess? Freeing crashes.
            return str;
        }

        public string Param_GetStringInt(SMwParamInfo param)
        {
            CStringInt result;
            Param_GetX(param, &result, sizeof(CStringInt));
            string str = result.Value;
            //result.Delete();// Not a copy of the string I guess? Freeing crashes.
            return str;
        }

        public void Param_GetX(SMwParamInfo param, void* ptr, int ptrSize)
        {
            byte* result = stackalloc byte[4 + ptrSize];//pointer to data + data (this is the CMwValueStd)
            byte* stackPtr = stackalloc byte[OT.CMwStack.StructSize];
            IntPtr* stackInValues = stackalloc IntPtr[1];
            EStackType* stackInTypes = stackalloc EStackType[1];
            CMwStack stack = new CMwStack((IntPtr)stackPtr, false);
            stack.Values = stackInValues;
            stack.Types = stackInTypes;
            stack.SizeDirect = 1;
            stack.PushParam(param);
            Param_Get(stack, (IntPtr)(result));
            switch (ptrSize)
            {
                case 1:
                    *(byte*)ptr = *(byte*)*(IntPtr*)result;
                    break;
                case 2:
                    *(short*)ptr = *(short*)*(IntPtr*)result;
                    break;
                case 4:
                    *(int*)ptr = *(int*)*(IntPtr*)result;
                    break;
                case 8:
                    *(long*)ptr = *(long*)*(IntPtr*)result;
                    break;
                default:
                    Memory.Copy((IntPtr)ptr, *(IntPtr*)result, ptrSize);
                    break;
            }
            stack.Delete();// Call not really needed as this is on the stack
        }
    }
}
