using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwCmd
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwCmd(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmd(IntPtr address)
        {
            return new CMwCmd(address);
        }

        /// <summary>
        /// 0xFFFFFFFC &= default
        /// 0xFFFFFFFD | 1 = installed
        /// 0xFFFFFFFE | 2 = uninstalled
        /// </summary>
        public int State
        {
            get { return *(int*)(Address + OT.CMwCmd.State); }
        }

        public CMwCmdBuffer Buffer
        {
            get { return *(IntPtr*)(Address + OT.CMwCmd.Buffer); }
        }

        public int SetSchemeLocation(int location)
        {
            // ( | 0x80) = some other thing?
            // see CMwCmd::SetSchemeLocation
            return FT.CMwCmd.SetSchemeLocation(this, location);
        }

        public void SetCmdBuffer(CMwCmdBuffer buffer)
        {
            FT.CMwCmd.SetCmdBuffer(this, buffer);
        }

        public void Run()
        {
            VT.Get<VT.CMwCmd>(Address).Run(this);
        }

        public void Install()
        {
            VT.Get<VT.CMwCmd>(Address).Install(this);
        }

        public void Uninstall()
        {
            VT.Get<VT.CMwCmd>(Address).Uninstall(this);
        }
    }
}
