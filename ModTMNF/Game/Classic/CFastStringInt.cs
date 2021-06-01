using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    // See CFastStringBase<T>

    /// <summary>
    /// UTF16
    /// </summary>
    public unsafe struct CFastStringInt
    {
        public int Length;
        public IntPtr CharPtr;
        //public IntPtr StringPtr;

        public string Value
        {
            get
            {
                /*if (StringPtr != IntPtr.Zero)
                {
                    return ((CString*)StringPtr)->Value;
                }*/
                return Marshal.PtrToStringUni(CharPtr);
            }
            set
            {
                /*if (StringPtr != IntPtr.Zero)
                {
                    Delete();
                }*/
                // Duplicate of CStringInt code
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
        }

        public static implicit operator string(CFastStringInt str)
        {
            return str.Value;
        }

        public void Delete()
        {
            Memory.Delete(CharPtr);
            /*if (StringPtr != IntPtr.Zero)
            {
                ((CStringInt*)StringPtr)->Delete();
            }*/
            CharPtr = IntPtr.Zero;
            Length = 0;
            //StringPtr = IntPtr.Zero;
        }
    }

    public unsafe struct CFastStringIntPtr
    {
        public IntPtr Address;

        private CFastStringInt* ptr
        {
            get { return (CFastStringInt*)Address; }
        }

        public IntPtr CharPtr
        {
            get { return ptr->CharPtr; }
            set { ptr->CharPtr = value; }
        }

        public int Length
        {
            get { return ptr->Length; }
            set { ptr->Length = value; }
        }

        /*public IntPtr StringPtr
        {
            get { return ptr->StringPtr; }
            set { ptr->StringPtr = value; }
        }*/

        public string Value
        {
            get { return ptr->Value; }
        }

        public static implicit operator CFastStringInt(CFastStringIntPtr fastStringPtr)
        {
            return *fastStringPtr.ptr;
        }
    }
}
