using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // TODO: Make IEnumerable. Would make to make all classes IConvertable from IntPtr to avoid boxing.

    public unsafe struct StructArray<T>
    {
        public IntPtr Address;
        public int Count;

        public StructArray(IntPtr address, int count)
        {
            Address = address;
            Count = count;
        }

        public static implicit operator StructArray<T>(IntPtr address)
        {
            return new StructArray<T>(IntPtr.Zero, 0);
        }

        public IntPtr this[int index]
        {
            get { return Address + (StructInfo<T>.Size * index); }
        }
    }
}
