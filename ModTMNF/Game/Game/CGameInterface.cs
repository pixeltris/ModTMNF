using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameInterface
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameInterface(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameInterface(IntPtr address)
        {
            return new CGameInterface(address);
        }
    }
}
