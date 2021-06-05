using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameNetPlayerInfo
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameNetPlayerInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameNetPlayerInfo(IntPtr address)
        {
            return new CGameNetPlayerInfo(address);
        }
    }
}
