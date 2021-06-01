using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSystemConfig
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CSystemConfig(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemConfig(IntPtr address)
        {
            return new CSystemConfig(address);
        }

        /// <summary>
        /// Assigned by CGbxApp::Init
        /// </summary>
        public static CSystemConfig Instance
        {
            get { return *(IntPtr*)ST.CSystemConfig.s_SystemConfig; }
        }
    }
}
