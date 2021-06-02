using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// Used by UI controls (CControlTools::ControlBindEvent / CControlBase::BindEvent)
    /// </summary>
    public unsafe struct CMwCmdFastCallUser
    {
        public IntPtr Address;

        public CMwCmd Base
        {
            get { return new CMwCmd(Address); }
        }

        public CMwCmdFastCallUser(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdFastCallUser(IntPtr address)
        {
            return new CMwCmdFastCallUser(address);
        }

        public IntPtr ObjPtr
        {
            get { return *(IntPtr*)(Address + OT.CMwCmdFastCallUser.ObjPtr); }
        }

        public IntPtr FuncPtr
        {
            get { return *(IntPtr*)(Address + OT.CMwCmdFastCallUser.FuncPtr); }
        }

        /// <summary>
        /// Some sort of call type?
        /// </summary>
        public int Unk1
        {
            get { return *(int*)(Address + OT.CMwCmdFastCallUser.Unk1); }
        }

        public static CMwCmdFastCallUser New(IntPtr objPtr, IntPtr funcPtr, int unk1)
        {
            CMwCmdFastCallUser result = Memory.New(OT.CMwCmdFastCallUser.StructSize);
            FT.CMwCmdFastCallUser.Ctor(result, objPtr, funcPtr, unk1);
            return result;
        }
    }
}
