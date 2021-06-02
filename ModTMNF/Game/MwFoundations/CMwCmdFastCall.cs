using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwCmdFastCall
    {
        public IntPtr Address;

        public CMwCmd Base
        {
            get { return new CMwCmd(Address); }
        }

        public CMwCmdFastCall(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdFastCall(IntPtr address)
        {
            return new CMwCmdFastCall(address);
        }

        public IntPtr ObjPtr
        {
            get { return *(IntPtr*)(Address + OT.CMwCmdFastCall.ObjPtr); }
        }

        public IntPtr FuncPtr
        {
            get { return *(IntPtr*)(Address + OT.CMwCmdFastCall.FuncPtr); }
        }

        public static CMwCmdFastCall New(IntPtr objPtr, IntPtr funcPtr)
        {
            CMwCmdFastCall result = Memory.New(OT.CMwCmdFastCall.StructSize);
            FT.CMwCmdFastCall.Ctor1(result, objPtr, funcPtr);
            return result;
        }

        public static CMwCmdFastCall New(IntPtr objPtr, IntPtr funcPtr, int location)
        {
            CMwCmdFastCall result = Memory.New(OT.CMwCmdFastCall.StructSize);
            FT.CMwCmdFastCall.Ctor2(result, objPtr, funcPtr, location);
            return result;
        }
    }
}
