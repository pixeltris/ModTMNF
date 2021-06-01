using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    // See CFastStringBase<T>

    // NOTE: We really aren't doing this right. This seems to be a packed bit structure.

    public unsafe struct CFastString
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
                return Marshal.PtrToStringAnsi(CharPtr);
            }
            set
            {
                /*Delete();
                StringPtr = Memory.New(sizeof(CString));
                ((CString*)StringPtr)->Length = 0;
                ((CString*)StringPtr)->CharPtr = IntPtr.Zero;
                ((CString*)StringPtr)->Value = value;*/

                /*if (StringPtr != IntPtr.Zero)
                {
                    Delete();
                }*/
                // Duplicate of CString code
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
        }

        public static implicit operator string(CFastString str)
        {
            return str.Value;
        }

        public CFastString(string value)
        {
            this = default(CFastString);
            Value = value;
        }

        public void Delete()
        {
            Memory.Delete(CharPtr);
            /*if (StringPtr != IntPtr.Zero)
            {
                ((CString*)StringPtr)->Delete();
            }*/
            CharPtr = IntPtr.Zero;
            Length = 0;
            //StringPtr = IntPtr.Zero;
        }
    }

    public unsafe struct CFastStringPtr
    {
        public IntPtr Address;

        private CFastString* ptr
        {
            get { return (CFastString*)Address; }
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

        public static implicit operator CFastString(CFastStringPtr fastStringPtr)
        {
            return *fastStringPtr.ptr;
        }
    }
}
