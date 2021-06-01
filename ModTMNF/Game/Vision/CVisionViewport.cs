using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CVisionViewport
    {
        public IntPtr Address;

        public CHmsViewport Base
        {
            get { return new CHmsViewport(Address); }
        }

        public CVisionViewport(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CVisionViewport(IntPtr address)
        {
            return new CVisionViewport(address);
        }
    }
}
