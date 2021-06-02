using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameNetwork
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameNetwork(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameNetwork(IntPtr address)
        {
            return new CGameNetwork(address);
        }
    }
}
