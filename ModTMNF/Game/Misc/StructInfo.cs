using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ModTMNF.Game
{
    public static class StructInfo<T>
    {
        public static readonly int Size;

        static StructInfo()
        {
            Type offsetInfoType = typeof(OT).GetNestedType(typeof(T).Name);
            if (offsetInfoType != null)
            {
                FieldInfo field = offsetInfoType.GetField("StructSize");
                if (field != null)
                {
                    Size = (int)field.GetValue(null);
                }
            }
        }
    }
}
