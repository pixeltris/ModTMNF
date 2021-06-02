using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CAudioMusic
    {
        public IntPtr Address;

        public CAudioSound Base
        {
            get { return new CAudioSound(Address); }
        }

        public CAudioMusic(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CAudioMusic(IntPtr address)
        {
            return new CAudioMusic(address);
        }
    }
}
