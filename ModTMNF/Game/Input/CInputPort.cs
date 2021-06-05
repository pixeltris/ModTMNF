using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CInputPort
    {
        public IntPtr Address;

        public CMwNod Base
        {
            get { return new CMwNod(Address); }
        }

        public CInputPort(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CInputPort(IntPtr address)
        {
            return new CInputPort(address);
        }
    }
}

/*
int __thiscall CInputPort::ClearInputs(CInputPort *this, int a2)
{
  CInputPort *v2; // esi@1
  CMwTimer **v3; // eax@1
  int result; // eax@3

  v2 = this;
  (*(void (**)(void))(*(_DWORD *)this + 164))();
  CInputEventsStore::ClearStore((CInputPort *)((char *)v2 + 64));
  CInputEventsStore::Lock((CInputPort *)((char *)v2 + 64), 0);
  *((_DWORD *)v2 + 14) = 1;
  v3 = *(CMwTimer ***)(dword_D731E0 + 20);
  if ( !v3 )
    v3 = (CMwTimer **)(dword_D731E0 + 160);
  result = CMwTimerAdapter::GetTime(v3);
  *((_DWORD *)v2 + 50) = result;
  if ( a2 || *((_DWORD *)v2 + 35) )
    result = CInputPort::ReadCurMapLatestEventsFromHarware(v2, 1);
  return result;
}*/