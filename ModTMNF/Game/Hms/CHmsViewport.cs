using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CHmsViewport
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CHmsViewport(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CHmsViewport(IntPtr address)
        {
            return new CHmsViewport(address);
        }

        public bool IsFullScreen
        {
            get { return *(BOOL*)(Address + OT.CHmsViewport.IsFullScreen); }
        }
    }
}
