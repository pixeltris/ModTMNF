using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwCmdContainer
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwCmdContainer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdContainer(IntPtr address)
        {
            return new CMwCmdContainer(address);
        }

        public bool Installed
        {
            get { return *(BOOL*)(Address + OT.CMwCmdContainer.Installed); }
        }

        public CFastBuffer<CMwCmd> Cmds
        {
            get { return new CFastBuffer<CMwCmd>(Address + OT.CMwCmdContainer.Cmds); }
        }

        public void Install()
        {
            VT.Get<VT.CMwCmdContainer>(Address).Install(this);
        }

        public void Uninstall()
        {
            VT.Get<VT.CMwCmdContainer>(Address).Uninstall(this);
        }

        public void AddCmd(CMwCmd cmd, int location)
        {
            FT.CMwCmdContainer.AddCmd(this, cmd, location);
        }

        public CMwCmd AddFastCall(IntPtr objPtr, IntPtr funcPtr, int location)
        {
            return FT.CMwCmdContainer.AddFastCall(this, objPtr, funcPtr, location);
        }
    }
}
