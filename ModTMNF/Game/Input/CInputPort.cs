using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CInputPort
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CInputPort(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CInputPort(IntPtr address)
        {
            return new CInputPort(address);
        }
    }
}
