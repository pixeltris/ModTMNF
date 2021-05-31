using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// Reference to a CMwNod object. When the ref is deleted the ref counter of the object is decremented.
    /// </summary>
    public struct CMwNodRef
    {
        public IntPtr NodPtr;

        public CMwNodRef(CMwNod nod, bool addRef = true)
        {
            NodPtr = nod.Address;
            if (addRef)
            {
                nod.MwAddRef();
            }
        }

        public CMwNodRef(IntPtr nod, bool addRef = true)
            : this(new CMwNod(), addRef)
        {
        }

        public void Delete()
        {
            CMwNod nod = NodPtr;
            nod.MwRelease();
        }
    }

    /// <summary>
    /// CMwNodRef*
    /// </summary>
    public unsafe struct CMwNodRefPtr
    {
        public IntPtr Address;

        public IntPtr NodPtr
        {
            get { return *(IntPtr*)Address; }
        }

        public void Delete()
        {
            ((CMwNodRef*)Address)->Delete();
        }
    }
}
