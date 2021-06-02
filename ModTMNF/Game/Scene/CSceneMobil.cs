using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSceneMobil
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CSceneMobil(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSceneMobil(IntPtr address)
        {
            return new CSceneMobil(address);
        }
    }
}
