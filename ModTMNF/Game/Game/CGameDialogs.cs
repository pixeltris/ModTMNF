using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameDialogs
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameDialogs(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameDialogs(IntPtr address)
        {
            return new CGameDialogs(address);
        }
    }
}
