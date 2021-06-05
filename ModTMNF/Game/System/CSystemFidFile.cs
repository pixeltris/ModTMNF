using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSystemFidFile
    {
        public IntPtr Address;

        public CSystemFid Base
        {
            get { return new CSystemFid(Address); }
        }

        public CSystemFidFile(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemFidFile(IntPtr address)
        {
            return new CSystemFidFile(address);
        }

        public string GetFullName(int unk1 = 0, int unk2 = 0)
        {
            CFastStringInt str = CFastStringInt.Empty;
            FT.CSystemFidFile.GetFullName(this, ref str, unk1, unk2);
            string result = str.Value;
            return result;
        }
    }
}
