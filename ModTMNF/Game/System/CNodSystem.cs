using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CNodSystem
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CNodSystem(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CNodSystem(IntPtr address)
        {
            return new CNodSystem(address);
        }
    }
}
