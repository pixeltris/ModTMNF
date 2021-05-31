using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// UTF16 / wchar* (with length info, and null terminated)
    /// </summary>
    public unsafe struct CStringInt
    {
        public int Length;
        public IntPtr CharPtr;

        public string Value
        {
            get { return Marshal.PtrToStringUni(CharPtr); }
            set
            {
                if (value.Length > Length || CharPtr == IntPtr.Zero)
                {
                    Delete();
                    CharPtr = Memory.New(value.Length + sizeof(char));
                    if (CharPtr == IntPtr.Zero)
                    {
                        return;
                    }
                    *(char*)(CharPtr) = '\0';
                }
                byte[] buff = Encoding.Unicode.GetBytes(value == null ? string.Empty : value);
                Marshal.Copy(buff, 0, CharPtr, buff.Length);
                *(char*)(CharPtr + buff.Length) = '\0';
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
