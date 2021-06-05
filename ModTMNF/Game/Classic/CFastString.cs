using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    // See CFastStringBase<T>

    public unsafe struct CFastString
    {
        public int Length;
        public IntPtr CharPtr;

        public static CFastString Empty
        {
            get
            {
                return new CFastString()
                {
                    Length = 0,
                    CharPtr = ST.CFastStringBase.s_EmptyChars
                };
            }
        }

        public string Value
        {
            get
            {
                return Marshal.PtrToStringAnsi(CharPtr);
            }
            set
            {
                if (CharPtr != IntPtr.Zero)
                {
                    Delete();
                }
                FT.CFastString.Ctor(ref this, value);
            }
        }

        public static implicit operator string(CFastString str)
        {
            return str.Value;
        }

        public CFastString(string value)
        {
            this = Empty;
            Value = value;
        }

        public void Delete()
        {
            FT.CFastString.Dtor(ref this);
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
