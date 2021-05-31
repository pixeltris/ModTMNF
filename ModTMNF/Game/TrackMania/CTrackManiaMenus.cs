using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct CTrackManiaMenus
    {
        public IntPtr Address;

        public CGameCtnMenus Base
        {
            get { return new CGameCtnMenus(Address); }
        }

        public CTrackManiaMenus(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CTrackManiaMenus(IntPtr address)
        {
            return new CTrackManiaMenus(address);
        }
    }
}
