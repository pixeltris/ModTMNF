using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSceneObject
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CSceneObject(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSceneObject(IntPtr address)
        {
            return new CSceneObject(address);
        }
    }
}
