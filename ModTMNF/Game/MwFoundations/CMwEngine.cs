using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwEngine
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CMwEngine(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwEngine(IntPtr address)
        {
            return new CMwEngine(address);
        }

        public EMwEngineId Id
        {
            get { return *(EMwEngineId*)(Address + OT.CMwEngine.Id); }
        }

        /// <summary>
        /// Classes belonging to an engine are referred to as "Groups" here.
        /// See CMwEngine::CreateInstance (virtual function, implemented for each engine).
        /// </summary>
        public CFastBuffer Groups
        {
            get { return new CFastBuffer(Address + OT.CMwEngine.Groups); }
        }
    }
}
