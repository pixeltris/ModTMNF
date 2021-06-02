using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CPlug
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CPlug(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CPlug(IntPtr address)
        {
            return new CPlug(address);
        }
    }
}
