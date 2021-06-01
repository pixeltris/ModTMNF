using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public struct CMwCmdContainer
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwCmdContainer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwCmdContainer(IntPtr address)
        {
            return new CMwCmdContainer(address);
        }
    }
}
