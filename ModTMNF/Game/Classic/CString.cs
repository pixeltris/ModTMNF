using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// char* (with length info, and null terminated)
    /// </summary>
    public unsafe struct CString
    {
        public int Length;// Number of characters, excluding terminating 0
        public IntPtr CharPtr;

        public string Value
        {
            get { return Marshal.PtrToStringAnsi(CharPtr); }
            set
            {
                if (value.Length > Length || CharPtr == IntPtr.Zero)
                {
                    Delete();
                    CharPtr = Memory.New(value.Length + sizeof(byte));
                    if (CharPtr == IntPtr.Zero)
                    {
                        return;
                    }
                    *(byte*)(CharPtr) = 0;
                }
                byte[] buff = Encoding.ASCII.GetBytes(value == null ? string.Empty : value);
                Marshal.Copy(buff, 0, CharPtr, buff.Length);
                *(byte*)(CharPtr + value.Length) = 0;
                Length = value.Length;
            }
        }

        public void Delete()
        {
            Memory.Delete(CharPtr);
            CharPtr = IntPtr.Zero;
            Length = 0;
        }
    }
}
