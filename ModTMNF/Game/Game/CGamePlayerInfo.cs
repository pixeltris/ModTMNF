using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGamePlayerInfo
    {
        public IntPtr Address;

        public CGameNetPlayerInfo Base
        {
            get { return new CGameNetPlayerInfo(Address); }
        }

        public CGamePlayerInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGamePlayerInfo(IntPtr address)
        {
            return new CGamePlayerInfo(address);
        }
    }
}
