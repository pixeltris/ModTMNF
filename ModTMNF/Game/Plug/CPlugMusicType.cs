using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CPlugMusicType
    {
        public IntPtr Address;

        public CPlugSound Base
        {
            get { return new CPlugSound(Address); }
        }

        public CPlugMusicType(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CPlugMusicType(IntPtr address)
        {
            return new CPlugMusicType(address);
        }
    }
}
