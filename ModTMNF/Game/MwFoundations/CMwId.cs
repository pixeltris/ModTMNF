using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwId
    {
        public int Value;

        public CMwId(int value)
        {
            Value = value;
        }

        public static implicit operator int(CMwId id)
        {
            return id.Value;
        }

        public static implicit operator CMwId(int id)
        {
            return new CMwId(id);
        }

        public static implicit operator CMwId(IntPtr address)
        {
            return new CMwId(*(int*)address);
        }

        /// <summary>
        /// SMwIdInternal*
        /// Initialized in CMwNod::StaticInit
        /// </summary>
        public static IntPtr s_NameTable
        {
            get { return ST.CMwId.s_NameTable; }
        }
    }

    public unsafe struct CMwIdPtr
    {
        public IntPtr Address;

        public int Value
        {
            get { return ((CMwId*)Address)->Value; }
        }

        public static implicit operator CMwId(CMwIdPtr idPtr)
        {
            return idPtr.Address;
        }
    }
}
