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

        public static CFastStringInt Empty
        {
            get
            {
                return new CFastStringInt()
                {
                    Length = 0,
                    CharPtr = ST.CFastStringBase.s_EmptyCharsInt
                };
            }
        }

        public string Value
        {
            get
            {
                return Marshal.PtrToStringUni(CharPtr);
            }
            set
            {
                if (CharPtr != IntPtr.Zero)
                {
                    Delete();
                }
                FT.CFastStringInt.Ctor(ref this, value);
            }
        }

        public static implicit operator string(CFastStringInt str)
        {
            return str.Value;
        }

        public CFastStringInt(string value)
        {
            this = Empty;
            Value = value;
        }

        public void Delete()
        {
            FT.CFastStringInt.Dtor(ref this);
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
