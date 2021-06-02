using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameMenu
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameMenu(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameMenu(IntPtr address)
        {
            return new CGameMenu(address);
        }
    }
}
