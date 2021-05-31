using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public struct CMwParam
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwParam(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwParam(IntPtr address)
        {
            return new CMwParam(address);
        }

        public bool IsIndexed
        {
            get { return FT.CMwParam.IsIndexed(this); }
        }

        public bool IsArray
        {
            get { return VT.Get<VT.CMwParam>(Address).IsArray(this); }
        }

        public bool IsBuffer
        {
            get { return VT.Get<VT.CMwParam>(Address).IsBuffer(this); }
        }

        public bool IsBufferCat
        {
            get { return VT.Get<VT.CMwParam>(Address).IsBufferCat(this); }
        }

        public bool IsRefBuffer
        {
            get { return VT.Get<VT.CMwParam>(Address).IsRefBuffer(this); }
        }

        public bool IsStruct
        {
            get { return VT.Get<VT.CMwParam>(Address).IsStruct(this); }
        }

        public int IndexedGetCount(CMwValueStd value)
        {
            return VT.Get<VT.CMwParam>(Address).IndexedGetCount(this, value);
        }

        public int StructGetCount(CMwValueStd value)
        {
            return VT.Get<VT.CMwParam>(Address).StructGetCount(this, value);
        }

        public int VGetValue(CMwStack stack, CMwValueStd value)
        {
            return VT.Get<VT.CMwParam>(Address).VGetValue(this, stack, value);
        }

        public int VSetValue(CMwStack stack, IntPtr value)
        {
            return VT.Get<VT.CMwParam>(Address).VSetValue(this, stack, value);
        }

        public int VAddValue(CMwStack stack, IntPtr value)
        {
            return VT.Get<VT.CMwParam>(Address).VAddValue(this, stack, value);
        }

        public int VSubValue(CMwStack stack, IntPtr value)
        {
            return VT.Get<VT.CMwParam>(Address).VSubValue(this, stack, value);
        }

        public int VGetNextNod(CMwStack stack, ref CMwNod value)
        {
            return VT.Get<VT.CMwParam>(Address).VGetNextNod(this, stack, ref value);
        }

        public bool VCanGetValueFromString()
        {
            return VT.Get<VT.CMwParam>(Address).VCanGetValueFromString(this);
        }

        public bool VCanGetStringFromValue()
        {
            return VT.Get<VT.CMwParam>(Address).VCanGetStringFromValue(this);
        }

        public void VGetValueFromString(CMwValueStd value, ref CFastStringInt str, SMwParamInfo paramInfo)
        {
            VT.Get<VT.CMwParam>(Address).VGetValueFromString(this,  value, ref str, paramInfo);
        }

        public void VGetStringFromValue(ref CFastStringInt str, IntPtr value, SMwParamInfo paramInfo)
        {
            VT.Get<VT.CMwParam>(Address).VGetStringFromValue(this, ref str, value, paramInfo);
        }

        public void GetIcon(ref EMwIconList unk1, ref EMwIconList unk2)
        {
            VT.Get<VT.CMwParam>(Address).GetIcon(this, ref unk1, ref unk2);
        }

        public void ParseSubParams(CMwNod unk1, CMwStack unk2, CMwNod unk3, IntPtr unk4FuncPtr, IntPtr unk5, uint unk6)
        {
            VT.Get<VT.CMwParam>(Address).ParseSubParams(this, unk1, unk2, unk3, unk4FuncPtr, unk5, unk6);
        }
    }
}
