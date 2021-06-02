using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CPlugSound
    {
        public IntPtr Address;

        public CPlugAudio Base
        {
            get { return new CPlugAudio(Address); }
        }

        public CPlugSound(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CPlugSound(IntPtr address)
        {
            return new CPlugSound(address);
        }
    }
}
