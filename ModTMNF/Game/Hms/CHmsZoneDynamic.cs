using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public struct CHmsZoneDynamic
    {
        public IntPtr Address;

        public CHmsZoneDynamic(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CHmsZoneDynamic(IntPtr address)
        {
            return new CHmsZoneDynamic(address);
        }

        public void PhysicsStep2()
        {
            FT.CHmsZoneDynamic.PhysicsStep2(this);
        }
    }
}
