using System;
using System.Runtime.InteropServices;
using ModTMNF.Analysis.Asm;

namespace Bea
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct REX_StructUnsafe
    {
        public byte W_;
        public byte R_;
        public byte X_;
        public byte B_;
        public byte state;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PrefixInfoUnsafe
    {
        public int Number;
        public int NbUndefined;
        public byte LockPrefix;
        public byte OperandSize;
        public byte AddressSize;
        public byte RepnePrefix;
        public byte RepPrefix;
        public byte FSPrefix;
        public byte SSPrefix;
        public byte GSPrefix;
        public byte ESPrefix;
        public byte CSPrefix;
        public byte DSPrefix;
        public byte BranchTaken;
        public byte BranchNotTaken;
        public REX_StructUnsafe REX;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EFLStructUnsafe
    {
        public byte OF_;
        public byte SF_;
        public byte ZF_;
        public byte AF_;
        public byte PF_;
        public byte CF_;
        public byte TF_;
        public byte IF_;
        public byte DF_;
        public byte NT_;
        public byte RF_;
        public byte alignment;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MemoryTypeUnsafe
    {
        public Int32 BaseRegister;
        public Int32 IndexRegister;
        public Int32 Scale;
        public Int64 Displacement;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InstructionTypeUnsafe
    {
        public Int32 Category;
        public Int32 Opcode;
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        unsafe fixed sbyte mnemonic[16];
        public Int32 BranchType;
        public EFLStructUnsafe Flags;
        public UInt64 AddrValue;
        public Int64 Immediat;
        public UInt32 ImplicitModifiedRegs;

        public string Mnemonic
        {
            get
            {
                fixed (sbyte* str = mnemonic)
                {
                    return new string(str);
                }
            }
        }

        public BeaConstants.BranchType Branch
        {
            get { return (BeaConstants.BranchType)BranchType; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ArgumentTypeUnsafe
    {
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        unsafe fixed sbyte argMnemonic[32];
        public Int32 ArgType;
        public Int32 ArgSize;
        public Int32 ArgPosition;
        public UInt32 AccessMode;
        public MemoryTypeUnsafe Memory;
        public UInt32 SegmentReg;

        public string ArgMnemonicStr
        {
            get
            {
                fixed (sbyte* str = argMnemonic)
                {
                    return new string(str);
                }
            }
        }

        public bool HasDisplacement
        {
            get { return Memory.Displacement != 0; }
        }

        public bool IsConstant
        {
            get
            {
                return (Type & BeaConstants.ArgumentType.CONSTANT_TYPE) == BeaConstants.ArgumentType.CONSTANT_TYPE;
            }
        }

        public bool IsRegister
        {
            get
            {
                return (Type & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE ||
                    Memory.BaseRegister != 0 || Memory.IndexRegister != 0;
            }
        }

        public bool IsGeneralRegister
        {
            get
            {
                return (Type & BeaConstants.ArgumentType.GENERAL_REG) == BeaConstants.ArgumentType.GENERAL_REG;
            }
        }

        public bool IsDisplacement
        {
            get
            {
                return (Type & BeaConstants.ArgumentType.MEMORY_TYPE) == BeaConstants.ArgumentType.MEMORY_TYPE;
            }
        }

        public bool IsFixedDisplacement
        {
            get { return Memory.Displacement > 0 && IsDisplacement && !IsRegister; }
        }

        public BeaConstants.ArgumentType Type
        {
            get { return (BeaConstants.ArgumentType)ArgType; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct DisasmUnsafe
    {
        public UInt32 EIP;
        public UInt64 VirtualAddr;
        public UInt32 SecurityBlock;
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        unsafe fixed sbyte completeInstr[64];
        public UInt32 Archi;
        public UInt64 Options;
        public InstructionTypeUnsafe Instruction;
        public ArgumentTypeUnsafe Argument1;
        public ArgumentTypeUnsafe Argument2;
        public ArgumentTypeUnsafe Argument3;
        public PrefixInfoUnsafe Prefix;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40, ArraySubType = UnmanagedType.U4)]
        unsafe fixed UInt32 Reserved_[40];

        public string CompleteInstr
        {
            get
            {
                fixed (sbyte* str = completeInstr)
                {
                    return new string(str);
                }
            }
        }

        public uint VirtualAddress
        {
            get { return (uint)VirtualAddr; }
        }

        public bool RegisterLeftConstantRight()
        {
            return ((BeaConstants.ArgumentType)Argument1.ArgType & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE &&
                   ((BeaConstants.ArgumentType)Argument2.ArgType & BeaConstants.ArgumentType.CONSTANT_TYPE) == BeaConstants.ArgumentType.CONSTANT_TYPE;
        }

        public bool RegisterLeftMemoryRight()
        {
            return ((BeaConstants.ArgumentType)Argument1.ArgType & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE &&
                   ((BeaConstants.ArgumentType)Argument2.ArgType & BeaConstants.ArgumentType.MEMORY_TYPE) == BeaConstants.ArgumentType.MEMORY_TYPE;
        }

        public bool RegisterLeftRegisterRight()
        {
            return ((BeaConstants.ArgumentType)Argument1.ArgType & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE &&
                   ((BeaConstants.ArgumentType)Argument2.ArgType & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE;
        }

        public bool MemoryLeftConstantRight()
        {
            return ((BeaConstants.ArgumentType)Argument1.ArgType & BeaConstants.ArgumentType.MEMORY_TYPE) == BeaConstants.ArgumentType.MEMORY_TYPE &&
                   ((BeaConstants.ArgumentType)Argument2.ArgType & BeaConstants.ArgumentType.CONSTANT_TYPE) == BeaConstants.ArgumentType.CONSTANT_TYPE;
        }

        public uint GetCallAddress(UnmanagedBuffer buffer)
        {
            uint functionAddress = 0;
            if (Instruction.AddrValue != 0 && Instruction.AddrValue >= buffer.CodeSectionStart && Instruction.AddrValue <= buffer.CodeSectionEnd)
            {
                functionAddress = (uint)Instruction.AddrValue;
            }
            else if (Argument1.Memory.Displacement != 0 && Argument1.Memory.IndexRegister == 0)
            {
                uint displacement = (uint)(Argument1.Memory.Displacement >> 32);
                if (displacement >= buffer.CodeSectionStart && displacement <= buffer.CodeSectionEnd)
                {
                    functionAddress = (uint)System.Runtime.InteropServices.Marshal.ReadInt32((IntPtr)(buffer.Ptr.ToInt32() + (displacement - buffer.CodeSectionStart)));

                    if (functionAddress < buffer.CodeSectionStart || functionAddress > buffer.CodeSectionEnd)
                        functionAddress = 0;
                }
            }
            return functionAddress;
        }

        public bool IsRegister(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.GENERAL_REG) == BeaConstants.ArgumentType.GENERAL_REG;
        }

        public bool HasArgument(int argType)
        {
            return (BeaConstants.ArgumentType)argType != BeaConstants.ArgumentType.NO_ARGUMENT &&
                   (BeaConstants.ArgumentType)argType != BeaConstants.ArgumentType.NONE;
        }

        public bool IsEAX(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG0) == BeaConstants.ArgumentType.REG0;
        }

        public bool IsECX(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG1) == BeaConstants.ArgumentType.REG1;
        }

        public bool IsEDX(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG2) == BeaConstants.ArgumentType.REG2;
        }

        public bool IsEBX(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG3) == BeaConstants.ArgumentType.REG3;
        }

        public bool IsESP(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG4) == BeaConstants.ArgumentType.REG4;
        }

        public bool IsEBP(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG5) == BeaConstants.ArgumentType.REG5;
        }

        public bool IsESI(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG6) == BeaConstants.ArgumentType.REG6;
        }

        public bool IsEDI(int argType)
        {
            return ((BeaConstants.ArgumentType)argType & BeaConstants.ArgumentType.REG7) == BeaConstants.ArgumentType.REG7;
        }

        public bool HasDisplacement()
        {
            return Argument1.HasDisplacement || Argument2.HasDisplacement || Argument3.HasDisplacement;
        }

        public bool HasConstant()
        {
            return Argument1.IsConstant || Argument2.IsConstant || Argument3.IsConstant;
        }

        public long GetDisplacement()
        {
            long d1 = Argument1.Memory.Displacement;
            long d2 = Argument2.Memory.Displacement;
            long d3 = Argument2.Memory.Displacement;
            return Math.Max(Math.Max(d1, d2), d3);
        }

        public bool HasRegister()
        {
            return
                Argument1.Memory.BaseRegister != 0 || Argument1.Memory.IndexRegister != 0 ||
                Argument2.Memory.BaseRegister != 0 || Argument2.Memory.IndexRegister != 0 ||
                Argument3.Memory.BaseRegister != 0 || Argument3.Memory.IndexRegister != 0;
        }

        public bool HasSIB()
        {
            return
                Argument1.Memory.Scale != 0 ||
                Argument2.Memory.Scale != 0 ||
                Argument3.Memory.Scale != 0;
        }

        public Mnemonic Mnemonic
        {
            get
            {
                switch (Instruction.Mnemonic)
                {
                    case "push ": return Mnemonic.Push;
                    case "pop ": return Mnemonic.Pop;
                    case "add ": return Mnemonic.Add;
                    case "cmp ": return Mnemonic.Cmp;
                    case "mov ": return Mnemonic.Mov;
                    case "lea ": return Mnemonic.Lea;
                    case "test ": return Mnemonic.Test;
                    case "call": return Mnemonic.Call;
                    default: return Mnemonic.Unknown;
                }
            }
        }
    }
}