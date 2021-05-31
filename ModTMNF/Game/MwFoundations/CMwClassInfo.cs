using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ModTMNF.Game
{
    /// <summary>
    /// Defines class info for classes which inherit from the base class CMwNod.
    /// 
    /// NOTE: This class itself doesn't inherit from CMwNod.
    /// </summary>
    public unsafe struct CMwClassInfo
    {
        public IntPtr Address;

        public CMwClassInfo(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwClassInfo(IntPtr address)
        {
            return new CMwClassInfo(address);
        }

        public EMwClassId Id
        {
            get { return (EMwClassId)(*(int*)(Address + OT.CMwClassInfo.Id)); }
        }

        public CMwClassInfo Parent
        {
            get
            {
                return *(IntPtr*)(Address + OT.CMwClassInfo.Parent);
            }
        }

        public CFastArray<CMwClassInfo> Children
        {
            get { return new CFastArray<CMwClassInfo>(Address + OT.CMwClassInfo.Children); }
        }

        /// <summary>
        /// This is set up by the call to MwClassLink from within CMwClassInfo::CMwClassInfo (this is a linked list)
        /// - See CMwNod::MwBuildClassInfoTree / CMwClassInfo::FindFromClassName for traversing the linked list at 0xD73BA8
        /// </summary>
        public CMwClassInfo Next
        {
            get
            {
                int addr = *(int*)(Address.ToInt64() + OT.CMwClassInfo.Next);
                return new CMwClassInfo((IntPtr)addr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Del_MwNew();
        static Dictionary<IntPtr, Del_MwNew> newFuncs = new Dictionary<IntPtr, Del_MwNew>();// Cache to keep the delegates alive
        /// <summary>
        /// Used to create a new instance of the given class.
        /// 
        /// Most classes in the game has their own "MwNew" function e.g. "CMwEngine::MwNewCMwEngine".
        /// This gets registered with the class info (CMwClassInfo::CMwClassInfo).
        /// </summary>
        public Del_MwNew New
        {
            get
            {
                IntPtr funcAddr = Marshal.ReadIntPtr(Address, OT.CMwClassInfo.New);
                if (funcAddr == IntPtr.Zero)
                {
                    return null;
                }
                Del_MwNew result;
                if (!newFuncs.TryGetValue(funcAddr, out result))
                {
                    result = (Del_MwNew)Marshal.GetDelegateForFunctionPointer(funcAddr, typeof(Del_MwNew));
                }
                return result;
            }
        }

        /// <summary>
        /// Does this class have a "MwNew" allocator function
        /// </summary>
        public bool HasNew
        {
            get { return Marshal.ReadIntPtr(Address, OT.CMwClassInfo.New) != IntPtr.Zero; }
        }

        /// <summary>
        /// The name of the class (not all classes have a name defined, so we will default to our hard coded value for those)
        /// </summary>
        public string Name
        {
            get
            {
                string name = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(Address, OT.CMwClassInfo.Name));
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Id.ToString();
                }
                return name;
            }
        }

        public int ParamCount
        {
            get { return *((int*)(Address + OT.CMwClassInfo.ParamCount)); }
        }

        public SMwParamInfo ParamInfos
        {
            get
            {
                int addr = *((int*)(Address + OT.CMwClassInfo.ParamInfos));
                if (addr == 0)
                {
                    return IntPtr.Zero;
                }
                return (IntPtr)(*(int*)addr);
            }
        }

        /// <summary>
        /// CMwEngineManager::First
        /// </summary>
        public static CMwClassInfo FirstClass
        {
            get { return CMwEngineManager.First; }
        }

        public int MwGetNearestFather(int unk1, ref int unk2)
        {
            return FT.CMwClassInfo.MwGetNearestFather(this, unk1, ref unk2);
        }

        public bool IsMwParamIdEqualName(int unk1, string name, int unk2)
        {
            return FT.CMwClassInfo.IsMwParamIdEqualName(this, unk1, name, unk2);
        }

        public uint GetMwParamIdFromName(string name, int unk1)
        {
            return FT.CMwClassInfo.GetMwParamIdFromName(this, name, unk1);
        }

        public SMwParamInfo GetMwParamFromName(string name, int unk1)
        {
            return FT.CMwClassInfo.GetMwParamFromName(this, name, unk1);
        }

        public SMwParamInfo GetMwParamFromNameRecursive(string name)
        {
            return FT.CMwClassInfo.GetMwParamFromNameRecursive(this, name);
        }

        public uint GetMwParamIdRecursive_FromIndex(int index)
        {
            return FT.CMwClassInfo.GetMwParamIdRecursive_FromIndex(this, index);
        }

        public int GetMwParamIdRecursive_Count()
        {
            return FT.CMwClassInfo.GetMwParamIdRecursive_Count(this);
        }

        public void AddChild(CMwClassInfo child)
        {
            FT.CMwClassInfo.AddChild(this, child);
        }

        public void BuildTree()
        {
            FT.CMwClassInfo.BuildTree(this);
        }

        public static CMwClassInfo FindFromClassName(string name)
        {
            CFastString str = new CFastString(name);
            CMwClassInfo classInfo = FT.CMwClassInfo.FindFromClassName(ref str);
            str.Delete();
            return classInfo;
        }
    }
}
