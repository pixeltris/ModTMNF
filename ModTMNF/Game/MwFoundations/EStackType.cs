using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// CMwStack::EStackType
    /// </summary>
    public enum EStackType : uint
    {
        Param = 0,//SMwParamInfo*
        Bool = 0x10000001,
        Object = 0x10000002,//CMwNod*
        Enum = 0x10000003,
        Iso4 = 0x10000004,
        Vec2 = 0x10000005,
        Vec3 = 0x10000006,
        Int3 = 0x10000007,
        UInt3 = 0x10000008,
        Int = 0x10000009,
        UInt = 0x1000000A,
        Float = 0x1000000B,
        String = 0x1000000C,//CString*
        StringInt = 0x1000000D,//CStringInt*
    }
}
