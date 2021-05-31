using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    // NOTE: For parameters with an offset of -1 they are implemented in their class VirtualParam_Get (this also applies for actions / procs). Example:
    // CTrackManiaMenus::VirtualParam_Get - 0046BEE0
    //  - NetworkGameMode (index:35, offset:-1) - calls CTrackManiaNetwork::GetServerInfo, then CTrackManiaNetworkServerInfo::GetConvertedNetworkNextGameMode
    // See Docs/Generated/VirtualParam_Get.txt

    public unsafe struct SMwParamInfo
    {
        public IntPtr Address;

        public SMwParamInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator SMwParamInfo(IntPtr address)
        {
            return new SMwParamInfo(address);
        }
        
        /// <summary>
        /// The type of param
        /// </summary>
        public EMwParamType Type
        {
            get { return *(EMwParamType*)(Address + OT.SMwParamInfo.Type); }
        }

        /// <summary>
        /// This Id is based on the class id, with increments to the class id value.
        /// The first param always has the same value as the class id, and it's incremented from there for each subsequent param.
        /// 
        /// This can be used to get the param index by doing a bitwise AND of 0x000000FF
        /// </summary>
        public int Id
        {
            get { return *(int*)(Address + OT.SMwParamInfo.Id); }
        }

        /// <summary>
        /// Id converted to an index (member index of the containing structure)
        /// </summary>
        public int Index
        {
            get { return Id & 0x000000FF; }
        }

        /// <summary>
        /// The param instance for accessing the param
        /// </summary>
        public CMwParam Param
        {
            get { return *(IntPtr*)(Address + OT.SMwParamInfo.Param); }
        }

        /// <summary>
        /// The offset in the struct (true memory struct offset)
        /// </summary>
        public int Offset
        {
            get { return *(int*)(Address + OT.SMwParamInfo.Offset); }
        }

        /// <summary>
        /// The name of the param
        /// </summary>
        public string Name
        {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(Address, OT.SMwParamInfo.Name)); }
        }

        public int Flags1
        {
            get { return *(int*)(Address + OT.SMwParamInfo.Flags1); }
        }

        public int Flags2
        {
            get { return *(int*)(Address + OT.SMwParamInfo.Flags2); }
        }

        public SMwParamInfo Next
        {
            get
            {
                if (IsArray)
                {
                    return Address + OT.SMwParamInfo_Array.StructSize;
                }
                if (IsRange)
                {
                    return Address + OT.SMwParamInfo_Range.StructSize;
                }
                switch (Type)
                {
                    case EMwParamType.Vec2: return Address + OT.SMwParamInfo_Vec2.StructSize;
                    case EMwParamType.Vec3: return Address + OT.SMwParamInfo_Vec3.StructSize;
                    case EMwParamType.Vec4: return Address + OT.SMwParamInfo_Vec4.StructSize;
                    case EMwParamType.Action: return Address + OT.SMwParamInfo_Action.StructSize;
                    case EMwParamType.Proc: return Address + OT.SMwParamInfo_Proc.StructSize;
                    case EMwParamType.Class: return Address + OT.SMwParamInfo_Class.StructSize;
                    case EMwParamType.Enum: return Address + OT.SMwParamInfo_Enum.StructSize;
                    default: return Address + OT.SMwParamInfo.StructSize;
                }
            }
        }

        public bool IsArray
        {
            get
            {
                switch (Type)
                {
                    // Array
                    case EMwParamType.BoolArray:
                    case EMwParamType.ClassArray:
                    case EMwParamType.ColorArray:
                    case EMwParamType.IdArray:
                    case EMwParamType.Int3Array:
                    case EMwParamType.IntArray:
                    case EMwParamType.Iso3Array:
                    case EMwParamType.Iso4Array:
                    case EMwParamType.NaturalArray:
                    case EMwParamType.RealArray:
                    case EMwParamType.StringArray:
                    case EMwParamType.StringIntArray:
                    case EMwParamType.Vec2Array:
                    case EMwParamType.Vec3Array:
                    case EMwParamType.Vec4Array:
                    // Buffer
                    case EMwParamType.BoolBuffer:
                    case EMwParamType.ClassBuffer:
                    case EMwParamType.ColorBuffer:
                    case EMwParamType.IdBuffer:
                    case EMwParamType.Int3Buffer:
                    case EMwParamType.IntBuffer:
                    case EMwParamType.Iso3Buffer:
                    case EMwParamType.Iso4Buffer:
                    case EMwParamType.NaturalBuffer:
                    case EMwParamType.RealBuffer:
                    case EMwParamType.StringBuffer:
                    case EMwParamType.StringIntBuffer:
                    case EMwParamType.Vec2Buffer:
                    case EMwParamType.Vec3Buffer:
                    case EMwParamType.Vec4Buffer:
                    // BufferCat
                    case EMwParamType.BoolBufferCat:
                    case EMwParamType.ClassBufferCat:
                    case EMwParamType.ColorBufferCat:
                    case EMwParamType.IdBufferCat:
                    case EMwParamType.Int3BufferCat:
                    case EMwParamType.IntBufferCat:
                    case EMwParamType.Iso3BufferCat:
                    case EMwParamType.Iso4BufferCat:
                    case EMwParamType.NaturalBufferCat:
                    case EMwParamType.RealBufferCat:
                    case EMwParamType.StringBufferCat:
                    case EMwParamType.StringIntBufferCat:
                    case EMwParamType.Vec2BufferCat:
                    case EMwParamType.Vec3BufferCat:
                    case EMwParamType.Vec4BufferCat:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsRange
        {
            get
            {
                switch (Type)
                {
                    case EMwParamType.IntRange:
                    case EMwParamType.NaturalRange:
                    case EMwParamType.RealRange:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public SMwParamInfo_Enum AsEnum
        {
            get { return Address; }
        }

        public SMwParamInfo_Class AsClass
        {
            get { return Address; }
        }

        public SMwParamInfo_Array AsArray
        {
            get { return Address; }
        }

        public SMwParamInfo_Action AsAction
        {
            get { return Address; }
        }

        public SMwParamInfo_Range AsRange
        {
            get { return Address; }
        }

        public SMwParamInfo_Vec2 AsVec2
        {
            get { return Address; }
        }

        public SMwParamInfo_Vec3 AsVec3
        {
            get { return Address; }
        }

        public SMwParamInfo_Vec4 AsVec4
        {
            get { return Address; }
        }

        public SMwParamInfo_Proc AsProc
        {
            get { return Address; }
        }
    }
}
