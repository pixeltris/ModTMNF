using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CAudioSound
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CAudioSound(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CAudioSound(IntPtr address)
        {
            return new CAudioSound(address);
        }
    }
}
