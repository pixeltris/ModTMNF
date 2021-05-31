using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    /// <summary>
    /// new/delete must be used to avoid having mismatched allocators with the game
    /// </summary>
    public static class Memory
    {
        public static IntPtr Copy(IntPtr dst, IntPtr src, int size)
        {
            //0040C730 _memcpy
            return FT.Memory._memcpy(dst, src, size);
        }

        public static IntPtr Move(IntPtr dst, IntPtr src, int size)
        {
            //0040A8C0 _memmove
            return FT.Memory._memmove(dst, src, size);
        }

        public static void Set(IntPtr ptr, int value, int num)
        {
            //0040AF70 _memset
            FT.Memory._memset(ptr, value, num);
        }

        public static IntPtr New(int size)
        {
            //00403079 void * __cdecl operator new(unsigned int)
            return FT.Memory.new_(size);
        }

        public static void Delete(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                return;
            }
            //00402F70 void __cdecl operator delete(void *)
            FT.Memory.delete(address);
        }
    }
}
