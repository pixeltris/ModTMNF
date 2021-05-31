using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // This list was obtained from https://wiki.xaseco.org/wiki/ManiaPlanet_internals
    // The names here seem logical and likely official, but Google doesn't show any other results.
    // Is there another symbols leaks somewhere?

    // Action = function with no params / return value
    // Proc = function with params
    // Real = float/single
    // Natural = uint
    // StringInt = wide string (wstring / UTF16)

    // NOTE: TMNF doesn't include collection variants of Enum (as such these values differ to the TM2)
    // NOTE: TMNF also doesn't include Nat3 / Quat values

    /// <summary>
    /// This is assumed to match up with SMwParamInfo
    /// 
    /// Not to be confused with CSystemFidParameters::EParamType (which I assume is different)
    /// </summary>
    public enum EMwParamType : int
    {
        // Function with no params / return value
        Action = 0,

        Bool = 1,
        BoolArray = 2,
        BoolBuffer = 3,
        BoolBufferCat = 4,
        
        Class = 5,
        ClassArray = 6,
        ClassBuffer = 7,
        ClassBufferCat = 8,
        
        Color = 9,
        ColorArray = 10,
        ColorBuffer = 11,
        ColorBufferCat = 12,

        Enum = 13,

        Int = 14,
        IntArray = 15,
        IntBuffer = 16,
        IntBufferCat = 17,
        IntRange = 18,

        Iso4 = 19,
        Iso4Array = 20,
        Iso4Buffer = 21,
        Iso4BufferCat = 22,

        Iso3 = 23,
        Iso3Array = 24,
        Iso3Buffer = 25,
        Iso3BufferCat = 26,

        // CMwId
        Id = 27,
        IdArray = 28,
        IdBuffer = 29,
        IdBufferCat = 30,

        // uint
        Natural = 31,
        NaturalArray = 32,
        NaturalBuffer = 33,
        NaturalBufferCat = 34,
        NaturalRange = 35,

        // float/single
        Real = 36,
        RealArray = 37,
        RealBuffer = 38,
        RealBufferCat = 39,
        RealRange = 40,

        String = 41,
        StringArray = 42,
        StringBuffer = 43,
        StringBufferCat = 44,

        // wide string (wstring / UTF16)
        StringInt = 45,
        StringIntArray = 46,
        StringIntBuffer = 47,
        StringIntBufferCat = 48,

        Vec2 = 49,
        Vec2Array = 50,
        Vec2Buffer = 51,
        Vec2BufferCat = 52,

        Vec3 = 53,
        Vec3Array = 54,
        Vec3Buffer = 55,
        Vec3BufferCat = 56,

        Vec4 = 57,
        Vec4Array = 58,
        Vec4Buffer = 59,
        Vec4BufferCat = 60,

        Int3 = 61,// UNCONFIRMED
        Int3Array = 62,
        Int3Buffer = 63,
        Int3BufferCat = 64,

        Proc = 65,
    }
}
