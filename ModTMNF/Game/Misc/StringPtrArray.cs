using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace ModTMNF.Game
{
    /// <summary>
    /// Struct for handling char**
    /// </summary>
    public unsafe struct StringPtrArray : IEnumerable<string>
    {
        public IntPtr Address;
        public int Count;

        public StringPtrArray(IntPtr address, int count)
        {
            Address = address;
            Count = count;
        }

        public string this[int index]
        {
            get
            {
                if (index < 0 || index > Count)
                {
                    return null;
                }
                return Marshal.PtrToStringAnsi(*(IntPtr*)(Address + (index * IntPtr.Size)));
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
