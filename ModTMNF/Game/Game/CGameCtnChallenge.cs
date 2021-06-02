using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameCtnChallenge
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CGameCtnChallenge(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameCtnChallenge(IntPtr address)
        {
            return new CGameCtnChallenge(address);
        }
    }
}
