using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CGameProcess
    {
        public IntPtr Address;

        public CMwCmdContainer Base
        {
            get { return new CMwCmdContainer(Address); }
        }

        public CGameProcess(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameProcess(IntPtr address)
        {
            return new CGameProcess(address);
        }
    }
}
