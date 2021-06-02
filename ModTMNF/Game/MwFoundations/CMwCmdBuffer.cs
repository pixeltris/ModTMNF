using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwCmdBuffer
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwCmdBuffer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdBuffer(IntPtr address)
        {
            return new CMwCmdBuffer(address);
        }
    }
}
