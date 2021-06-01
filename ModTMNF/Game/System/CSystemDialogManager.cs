using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // CGbxApp::EngineInitEnd
    //  auto mgr = new CSystemDialogManagerWin32();
    //  ((CSystemEngine*)TheMainEngine->Engines[EMwEngineId.System])->SystemDialogManager = mgr;

    // CSystemDialogManager holds just a vtable, and 1 other unknown value
    // CSystemDialogManagerWin32 isn't any larger

    public unsafe struct CSystemDialogManager
    {
        public IntPtr Address;

        public CSystemDialogManager(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CSystemDialogManager(IntPtr address)
        {
            return new CSystemDialogManager(address);
        }
    }
}
