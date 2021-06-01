using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ModTMNF.Game
{
    public unsafe struct CMwStack
    {
        public IntPtr Address;

        public CMwStack(IntPtr address, bool setDeleteMemory = true, bool setVTable = true)
        {
            Address = address;
            if (setVTable)
            {
                *(IntPtr*)(Address + OT.CMwStack.VTable) = *(IntPtr*)ST.CMwStack.VTable;
            }
            if (setDeleteMemory)
            {
                DeleteMemory = true;
            }
        }

        public static implicit operator CMwStack(IntPtr address)
        {
            return new CMwStack(address, false, false);
        }

        public int WriteOffset
        {
            get { return *(int*)(Address + OT.CMwStack.WriteOffset); }
            set { *(int*)(Address + OT.CMwStack.WriteOffset) = value; }
        }

        public int ReadOffset
        {
            get { return *(int*)(Address + OT.CMwStack.ReadOffset); }
            set { *(int*)(Address + OT.CMwStack.ReadOffset) = value; }
        }

        public IntPtr* Values
        {
            get { return *(IntPtr**)(Address + OT.CMwStack.Values); }
            set { *(IntPtr**)(Address + OT.CMwStack.Values) = value; }
        }

        public EStackType* Types
        {
            get { return *(EStackType**)(Address + OT.CMwStack.Types); }
            set { *(EStackType**)(Address + OT.CMwStack.Types) = value; }
        }

        public int SizeDirect
        {
            get { return *(int*)(Address + OT.CMwStack.Size); }
            set { *(int*)(Address + OT.CMwStack.Size) = value; }
        }

        public int Size
        {
            get { return *(int*)(Address + OT.CMwStack.Size); }
            private set
            {
                if (value > 0)
                {
                    IntPtr newValues = Memory.New(value * 4);
                    IntPtr newTypes = Memory.New(value * 4);
                    Debug.Assert(newValues != IntPtr.Zero && newTypes != IntPtr.Zero);
                    if (Size > 0)
                    {
                        Memory.Copy(newValues, (IntPtr)Values, WriteOffset * 4);
                        Memory.Copy(newTypes, (IntPtr)Types, WriteOffset * 4);
                        Memory.Delete((IntPtr)Values);
                        Memory.Delete((IntPtr)Types);
                    }
                    else
                    {
                        WriteOffset = 0;
                    }
                    Values = (IntPtr*)newValues;
                    Types = (EStackType*)newTypes;
                    //ReadOffset = WriteOffset - 1;// The original C++ code does this, removed for now...
                }
                else
                {
                    // Maybe check DeleteMemory? The original C++ code doesn't...
                    Memory.Delete((IntPtr)Values);
                    Memory.Delete((IntPtr)Types);
                    Values = null;
                    Types = null;
                    ReadOffset = 0;
                    WriteOffset = 0;
                }
                *(int*)(Address + OT.CMwStack.Size) = value;
            }
        }

        /// <summary>
        /// Should the memory of the stack be deleted (memory owned by the stack, not anything else. should always be true).
        /// </summary>
        public bool DeleteMemory
        {
            get { return *(BOOL*)(Address + OT.CMwStack.DeleteMemory); }
            set { *(BOOL*)(Address + OT.CMwStack.DeleteMemory) = value; }
        }

        public void Delete()
        {
            if (DeleteMemory)
            {
                Memory.Delete((IntPtr)Values);
                Memory.Delete((IntPtr)Types);
                Values = null;
                Types = null;
            }
        }

        public void PushParam(SMwParamInfo value)
        {
            PushItem(value.Address, EStackType.Param);
        }

        public void PushBool(BOOL* value)
        {
            PushItem((IntPtr)value, EStackType.Bool);
        }

        public void PushObject(CMwNod value)
        {
            PushItem(value.Address, EStackType.Object);
        }

        public void PushEnum(int* value)
        {
            PushItem((IntPtr)value, EStackType.Enum);
        }

        /*public void PushIso4(GmIso4* value)
        {
            PushItem((IntPtr)value, EStackType.Iso4);
        }

        public void PushVec2(GmVec2* value)
        {
            PushItem((IntPtr)value, EStackType.Vec2);
        }

        public void PushVec3(GmVec3* value)
        {
            PushItem((IntPtr)value, EStackType.Vec3);
        }

        public void PushInt3(GmInt3* value)
        {
            PushItem((IntPtr)value, EStackType.Int3);
        }

        public void PushUInt3(GmInt3* value)
        {
            PushItem((IntPtr)value, EStackType.UInt3);
        }*/

        public void PushInt(int* value)
        {
            PushItem((IntPtr)value, EStackType.Int);
        }

        public void PushUInt(uint* value)
        {
            PushItem((IntPtr)value, EStackType.UInt);
        }

        public void PushFloat(float* value)
        {
            PushItem((IntPtr)value, EStackType.Float);
        }

        public void PushString(CString* value)
        {
            PushItem((IntPtr)value, EStackType.String);
        }

        public void PushStringInt(CStringInt* value)
        {
            PushItem((IntPtr)value, EStackType.StringInt);
        }

        public void PushItem(IntPtr value, EStackType type)
        {
            if (WriteOffset > Size - 1)
            {
                Size++;
            }
            Values[WriteOffset] = value;
            Types[WriteOffset] = type;
            WriteOffset++;
            // Might also have to increase the read pos? As it seems to decrease read pos when getting values? Not increase?
        }
    }

    // ManiaPlanet
    /*public static int Items = 0;// type:CMwStackItem(16)
    public static int ExtraItems = 16;
    public static int Size = 20;// type:int16
    public static int ExtraItemsCapacity = 22;// type:int16
    public static int CurrentPos = 24;*/
    /*public unsafe struct CMwStack
    {
        public IntPtr Address;

        public CMwStack(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwStack(IntPtr address)
        {
            return new CMwStack(address);
        }

        public int Size
        {
            get { return *(int*)(Address + OT.CMwStack.Size); }
            private set
            {
                if (value <= 2 + ExtraItemsCapacity)
                {
                    *(int*)(Address + OT.CMwStack.Size) = value;
                    return;
                }
                IntPtr newExtraItems = Memory.New((value - 2) * OT.CMwStackItem.StructSize);
                if (newExtraItems != IntPtr.Zero)
                {
                    if (ExtraItems != IntPtr.Zero)
                    {
                        Memory.Copy(newExtraItems, ExtraItems, (Size - 2) * OT.CMwStackItem.StructSize);
                        Memory.Delete(ExtraItems);
                    }
                    ExtraItems = newExtraItems;
                    ExtraItemsCapacity = value - 2;
                    *(int*)(Address + OT.CMwStack.Size) = value;
                }
            }
        }

        public IntPtr ExtraItems
        {
            get { return *(IntPtr*)(Address + OT.CMwStack.ExtraItems); }
            private set { *(IntPtr*)(Address + OT.CMwStack.ExtraItems) = value; }
        }

        public int ExtraItemsCapacity
        {
            get { return *(int*)(Address + OT.CMwStack.ExtraItemsCapacity); }
            private set { *(int*)(Address + OT.CMwStack.ExtraItemsCapacity) = value; }
        }

        public int CurrentPos
        {
            get { return *(int*)(Address + OT.CMwStack.CurrentPos); }
            private set { *(int*)(Address + OT.CMwStack.CurrentPos) = value; }
        }

        public CMwStackItem* GetItem(int index)
        {
            if (index < 2)
            {
                return (CMwStackItem*)Address + (index * OT.CMwStackItem.StructSize);
            }
            else if (ExtraItems != IntPtr.Zero)
            {
                return (CMwStackItem*)Address + ((index - 2) * OT.CMwStackItem.StructSize);
            }
            return (CMwStackItem*)IntPtr.Zero;
        }

        public void PushParam(SMwParamInfo value)
        {
            PushItem(new CMwStackItem(value.Address, EStackType.Param));
        }

        /// <summary>
        /// Assumes bool is 4 bytes in size (not always true for Mono)
        /// </summary>
        public void PushBool(BOOL* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Bool));
        }

        public void PushObject(CMwNod value)
        {
            PushItem(new CMwStackItem(value.Address, EStackType.Object));
        }

        public void PushEnum(int* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Enum));
        }

        /*public void PushIso4(GmIso4* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Iso4));
        }

        public void PushVec2(GmVec2* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Vec2));
        }

        public void PushVec3(GmVec3* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Vec3));
        }

        public void PushInt3(GmInt3* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.Int3));
        }

        public void PushUInt3(GmInt3* value)
        {
            PushItem(new CMwStackItem((IntPtr)value, EStackType.UInt3));
        }*/
    /*

 public void PushInt(int* value)
 {
     PushItem(new CMwStackItem((IntPtr)value, EStackType.Int));
 }

 public void PushUInt(uint* value)
 {
     PushItem(new CMwStackItem((IntPtr)value, EStackType.UInt));
 }

 public void PushFloat(float* value)
 {
     PushItem(new CMwStackItem((IntPtr)value, EStackType.Float));
 }

 public void PushString(CString* value)
 {
     PushItem(new CMwStackItem((IntPtr)value, EStackType.String));
 }

 public void PushStringInt(CStringInt* value)
 {
     PushItem(new CMwStackItem((IntPtr)value, EStackType.StringInt));
 }

 public void PushItem(CMwStackItem item)
 {
     Size++;
     *GetItem(Size - 1) = item;
 }
}

[StructLayout(LayoutKind.Sequential)]
public struct CMwStackItem
{
 public IntPtr Value;
 public EStackType Type;

 public CMwStackItem(IntPtr value, EStackType type)
 {
     Value = value;
     Type = type;
 }
}*/
}
