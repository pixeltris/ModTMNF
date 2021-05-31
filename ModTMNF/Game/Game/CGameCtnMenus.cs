using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CGameCtnMenus
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameCtnMenus(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameCtnMenus(IntPtr address)
        {
            return new CGameCtnMenus(address);
        }
    }
}
