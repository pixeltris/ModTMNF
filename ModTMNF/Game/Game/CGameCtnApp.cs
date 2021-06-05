using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CGameCtnApp
    {
        public IntPtr Address;

        public CGameApp Base
        {
            get { return new CGameApp(Address); }
        }

        public CGameCtnApp(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CGameCtnApp(IntPtr address)
        {
            return new CGameCtnApp(address);
        }

        public CGameCtnReplayRecord ReplayRecord
        {
            get { return *(IntPtr*)(Address + OT.CGameCtnApp.ReplayRecord); }
        }
    }
}
