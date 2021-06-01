using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CSystemEngine
    {
        public IntPtr Address;

        public CMwEngine Base
        {
            get { return new CMwEngine(Address); }
        }

        public CSystemEngine(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemEngine(IntPtr address)
        {
            return new CSystemEngine(address);
        }

        public CSystemDialogManager DialogManager
        {
            get { return *(IntPtr*)(Address + OT.CSystemEngine.DialogManager); }
        }
    }
}
