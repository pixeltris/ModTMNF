using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// Holds class info for all classes which belong to the given engine.
    /// </summary>
    public struct CMwEngineInfo
    {
        public IntPtr Address;

        public CMwEngineInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwEngineInfo(IntPtr address)
        {
            return new CMwEngineInfo(address);
        }

        public EMwEngineId Id
        {
            get { return (EMwEngineId)Marshal.ReadInt32(Address + OT.CMwEngineInfo.Id); }
        }

        public string Name
        {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(Address + OT.CMwEngineInfo.Name)); }
        }

        public CFastArray<CMwClassInfo> Classes
        {
            get { return new CFastArray<CMwClassInfo>(Address + OT.CMwEngineInfo.Classes); }
        }

        /// <summary>
        /// Interesting invokers:
        /// CMwEngineManager::AddClass
        /// </summary>
        public CMwClassInfo AddClass(CMwClassInfo classInfo)
        {
            return FT.CMwEngineInfo.AddClass(this, classInfo);
        }
    }
}
