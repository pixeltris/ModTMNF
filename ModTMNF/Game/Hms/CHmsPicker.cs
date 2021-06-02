using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CHmsPicker
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CHmsPicker(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CHmsPicker(IntPtr address)
        {
            return new CHmsPicker(address);
        }
    }
}
