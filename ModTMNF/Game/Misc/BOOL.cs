using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF
{
    /// <summary>
    /// In C# sizeof(bool) is 1. In msvc++ sizeof(bool) is 4. This type represents the win32 BOOL type.
    /// </summary>
    public struct BOOL
    {
        public int ValueI32;
        public bool Value
        {
            get { return ValueI32 != 0; }
            set { ValueI32 = value ? 1 : 0; }
        }

        public static implicit operator bool(BOOL value)
        {
            return value.Value;
        }

        public static implicit operator BOOL(bool value)
        {
            return new BOOL() { Value = value };
        }
    }
}
