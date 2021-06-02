using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CPlugMusic
    {
        public IntPtr Address;

        public CPlugMusicType Base
        {
            get { return new CPlugMusicType(Address); }
        }

        public CPlugMusic(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CPlugMusic(IntPtr address)
        {
            return new CPlugMusic(address);
        }
    }
}
