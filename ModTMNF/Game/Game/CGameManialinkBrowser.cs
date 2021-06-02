using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameManialinkBrowser
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameManialinkBrowser(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameManialinkBrowser(IntPtr address)
        {
            return new CGameManialinkBrowser(address);
        }
    }
}
