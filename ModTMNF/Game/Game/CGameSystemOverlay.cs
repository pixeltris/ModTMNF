using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameSystemOverlay
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameSystemOverlay(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameSystemOverlay(IntPtr address)
        {
            return new CGameSystemOverlay(address);
        }
    }
}
