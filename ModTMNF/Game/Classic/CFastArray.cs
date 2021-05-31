using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    public unsafe struct CFastArray
    {
        public IntPtr Address;

        public CFastArray(IntPtr address)
        {
            Address = address;
        }

        public int Count
        {
            get { return *(int*)(Address); }
            set { *(int*)(Address) = value; }
        }

        public IntPtr DataPtr
        {
            get { return *(IntPtr*)(Address + 4); }
            set { *(IntPtr*)(Address + 4) = value; }
        }
    }

    public unsafe struct CFastArray<T>
    {
        public IntPtr Address;

        public CFastArray(IntPtr address)
        {
            Address = address;
        }

        public int Count
        {
            get { return *(int*)(Address); }
            set { *(int*)(Address) = value; }
        }

        public IntPtr DataPtr
        {
            get { return *(IntPtr*)(Address + 4); }
            set { *(IntPtr*)(Address + 4) = value; }
        }

        /// <summary>
        /// This indexer is of size pointer
        /// </summary>
        public IntPtr this[int index]
        {
            get { return *(IntPtr*)(DataPtr + (sizeof(IntPtr) * index)); }
            set { *(IntPtr*)(DataPtr + (sizeof(IntPtr) * index)) = value; }
        }
    }
}
