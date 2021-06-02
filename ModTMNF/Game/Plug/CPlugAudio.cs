using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CPlugAudio
    {
        public IntPtr Address;

        public CPlug Base
        {
            get { return new CPlug(Address); }
        }

        public CPlugAudio(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CPlugAudio(IntPtr address)
        {
            return new CPlugAudio(address);
        }
    }
}
