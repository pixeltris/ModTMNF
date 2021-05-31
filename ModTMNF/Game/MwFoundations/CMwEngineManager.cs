using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// Manages all engine infos (CMwEngineInfo) which itself is just a collection of classes belonging to the engine.
    /// See CMwEngineInfo, EMwEngineId, EMwClassId
    /// </summary>
    public struct CMwEngineManager
    {
        public IntPtr Address;

        public CMwEngineManager(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwEngineManager(IntPtr address)
        {
            return new CMwEngineManager(address);
        }

        public CFastArray<CMwEngineInfo> Engines
        {
            get { return new CFastArray<CMwEngineInfo>(Address + OT.CMwEngineManager.m_Engines); }
        }

        public static CMwEngineManager Instance
        {
            get { return new CMwEngineManager(ST.CMwEngineManager.Instance); }
        }

        /// <summary>
        /// static class CMwClassInfo* CMwEngineManager::First
        /// </summary>
        public static CMwClassInfo First
        {
            get { return new CMwClassInfo(Marshal.ReadIntPtr(ST.CMwEngineManager.First)); }
        }

        /// <summary>
        /// Interesting invokers:
        /// CMwNod::AddClass
        /// </summary>
        public CMwClassInfo AddClass(CMwClassInfo classInfo)
        {
            return FT.CMwEngineManager.AddClass(this, classInfo);
        }

        /// <summary>
        /// Interesting invokers:
        /// CMwNod::CreateByMwClassId
        /// CMwNod::StaticGetClassInfo
        /// CMwNod::GetParamInfoFromParamId
        /// </summary>
        public CMwClassInfo GetClassInfo(EMwClassId classId)
        {
            return FT.CMwEngineManager.GetClassInfo(this, classId);
        }

        public CMwEngineInfo GetEngineInfo(EMwEngineId engineId)
        {
            int count = Engines.Count;
            for (int i = 0; i < count; i++)
            {
                CMwEngineInfo engine = Engines[i];
                if (engine.Address != IntPtr.Zero && engine.Id == engineId)
                {
                    return engine;
                }
            }
            return IntPtr.Zero;
        }

        public static EMwEngineId GetEngineIdFromClassId(EMwClassId classId)
        {
            return (EMwEngineId)((uint)classId & 0xFF000000);
        }

        public static int ShortToLongEngineId(int engineId)
        {
            return engineId << 24;
        }

        public static int LongToShortEngineId(int engineId)
        {
            return engineId >> 24;
        }

        public static int LongToShortClassId(int classId)
        {
            return ((int)classId & 0x00FFF000) >> 12;
        }
    }
}
