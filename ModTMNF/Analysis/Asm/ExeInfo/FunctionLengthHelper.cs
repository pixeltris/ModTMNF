using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Bea;

namespace ModTMNF.Analysis.Asm
{
    unsafe class FunctionLengthHelper
    {
        const byte INSTR_NEAR_PREFIX = 0x0F;
        const byte INSTR_SHORTJCC_BEGIN = 0x70;
        const byte INSTR_SHORTJCC_END = 0x7F;
        const byte INSTR_NEARJCC_BEGIN = 0x80;	//	Near's are prefixed with a 0x0F byte
        const byte INSTR_NEARJCC_END = 0x8F;
        const byte INSTR_RET = 0xC2;
        const byte INSTR_RETN = 0xC3;
        const byte INSTR_RETFN = 0xCA;
        const byte INSTR_RETF = 0xCB;
        const byte INSTR_RELJCX = 0xE3;
        public const byte INSTR_RELJMP = 0xE9;
        const byte INSTR_SHORTJMP = 0xEB;
        const byte INSTR_CALL = 0xE8;
        public const byte INSTR_INT3 = 0xCC;

        public static uint GetFunctionLength(uint address, UnmanagedBuffer buffer)
        {
            DisasmUnsafe disasm;
            uint end = GetFunctionEnd(buffer.AddressToEip(address), &disasm, buffer);
            if (end == uint.MaxValue) return 0;
            disasm.EIP = end;
            //return buffer.EipToAddress(end + (uint)BeaEngine.Disasm(&disasm)) - address;
            uint len = buffer.EipToAddress(end + (uint)BeaEngine.Disasm(&disasm)) - address;
            uint jmpTableLen = GetFunctionLengthFromJmpTable(&disasm, buffer, address, len);
            if (jmpTableLen != 0)// && jmpTableLen < len)
                return jmpTableLen;
            return len;
        }


        /// <summary>
        /// Gets function length for a given function which has a jmptable
        /// </summary>
        /// <returns>The function length if it has a jmptable, 0 if it doesn't</returns>
        private static uint GetFunctionLengthFromJmpTable(DisasmUnsafe* disasm, UnmanagedBuffer buffer, uint address, uint functionLength)
        {
            disasm->EIP = buffer.AddressToEip(address);
            disasm->VirtualAddr = buffer.EipToAddress(disasm->EIP);

            uint minEndAddress = uint.MaxValue;
            int len = 0;

            while (disasm->VirtualAddr < address + functionLength)
            {
                len = BeaEngine.Disasm(disasm);
                if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return 0;
                else if (disasm->Instruction.BranchType == (int)BeaConstants.BranchType.JmpType && disasm->Argument1.Memory.Scale == 4)
                {
                    uint jmpTable = (uint)disasm->Argument1.Memory.Displacement;
                    if (jmpTable < minEndAddress)
                        minEndAddress = jmpTable;
                }
                disasm->EIP = (uint)(disasm->EIP + len);
                disasm->VirtualAddr = buffer.EipToAddress(disasm->EIP);
            }
            if (minEndAddress == uint.MaxValue)
                return 0;
            return minEndAddress - address;
        }

        private static uint GetFunctionEnd(uint address, DisasmUnsafe* disasm, UnmanagedBuffer buffer)
        {
            List<uint> branches = new List<uint>();
            uint end = GetBranchList(address, disasm, buffer, ref branches);
            if (branches.Count == 0) return end;

            uint prev = 0;
            for (int i = 0; i < branches.Count; i++)
            {
                if (branches[i] < end || end == prev)
                    continue;
                end = GetFunctionEnd(branches[i], disasm, buffer);
                if (end == uint.MaxValue) return end;
                prev = branches[i];
            }
            
            return end;
        }

        private static uint GetBranchList(uint block, DisasmUnsafe* disasm, UnmanagedBuffer buffer, ref List<uint> branches)
        {
            disasm->EIP = block;
            disasm->VirtualAddr = 0;

            while (disasm->EIP < buffer.BufferCodeSectionEnd)
            {
                int len = BeaEngine.Disasm(disasm);
                if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    len = 1;
                else
                {
                    if (IsEndPoint(disasm, block))
                        return (uint)disasm->EIP;

                    uint branchAddress = GetBranchAddress(disasm, buffer, true);
                    if (branchAddress != 0)
                    {
                        branches.Add(branchAddress);
                    }
                }

                disasm->EIP = (uint)(disasm->EIP + len);
            }

            return uint.MaxValue;
        }

        public static uint GetBranchAddress(DisasmUnsafe* disasm, UnmanagedBuffer buffer, bool checkJmpInt3)
        {
            int offset = 0;
            byte opcode = Marshal.ReadByte((IntPtr)disasm->EIP);
            //	This code will determine what type of branch it is, and
            //	determine the address it will branch to.
            switch (opcode)
            {
                case INSTR_SHORTJMP:
                case INSTR_RELJCX:
                    offset = (sbyte)Marshal.ReadByte((IntPtr)disasm->EIP, 1);
                    offset += 2;
                    break;
                case INSTR_RELJMP:
                    offset = Marshal.ReadInt32((IntPtr)disasm->EIP, 1);
                    offset += 5;

                    // Make sure the offset is within the bounds of the code section
                    if (offset > buffer.BufferCodeSectionEnd || disasm->EIP + offset > buffer.BufferCodeSectionEnd)
                        return 0;

                    // Make sure the location where the JMP is going to doesn't have
                    // an INT3 directly above that address as we can assume the JMP is being
                    // treated as a CALL
                    if (checkJmpInt3 && Marshal.ReadByte((IntPtr)(disasm->EIP + offset - 1)) == INSTR_INT3)
                        return 0;

                    break;
                case INSTR_NEAR_PREFIX:
                    if (Marshal.ReadByte((IntPtr)disasm->EIP, 1) >= INSTR_NEARJCC_BEGIN && Marshal.ReadByte((IntPtr)disasm->EIP, 1) <= INSTR_NEARJCC_END)
                    {
                        offset = Marshal.ReadInt32((IntPtr)disasm->EIP, 2);
                        offset += 6;
                    }
                    break;
                default:
                    //	Check to see if it's in the valid range of JCC values.
                    //	e.g. ja, je, jne, jb, etc..
                    if (Marshal.ReadByte((IntPtr)disasm->EIP) >= INSTR_SHORTJCC_BEGIN && Marshal.ReadByte((IntPtr)disasm->EIP) <= INSTR_SHORTJCC_END)
                    {
                        offset = (sbyte)Marshal.ReadByte((IntPtr)disasm->EIP, 1);
                        offset += 2;
                    }
                    break;
            }

            if (offset == 0) return 0;
            return (uint)(disasm->EIP + offset);
        }

        private static bool IsEndPoint(DisasmUnsafe* disasm, uint curblock)
        {
            int offset;
            switch (disasm->Instruction.Opcode)
            {
                case INSTR_RET:
                case INSTR_RETN:
                case INSTR_RETFN:
                case INSTR_RETF:
                    return true;

                //	The following two checks, look for an instance in which
                //	an unconditional jump returns us to a previous block,
                //	thus creating a pseudo-endpoint.
                case INSTR_SHORTJMP:
                    offset = (sbyte)Marshal.ReadByte((IntPtr)disasm->EIP, 1);
                    return disasm->EIP + offset <= curblock;
                case INSTR_RELJMP:
                    offset = Marshal.ReadInt32((IntPtr)disasm->EIP, 1);
                    if (disasm->EIP + offset <= curblock) return true;

                    // Look for an instance in which a JMP is made followed
                    // directly by an INT3 opcode as we can assume this is the end
                    // of the function
                    else if (Marshal.ReadByte((IntPtr)disasm->EIP, 5) == INSTR_INT3)
                        return true;
                    return false;

                // Look for an instance in which a call is made followed
                // by an INT3 opcode
                case INSTR_CALL:
                    return Marshal.ReadByte((IntPtr)disasm->EIP, 5) == INSTR_INT3;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds the first RET followed by INT3 opcodes after a given address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="disasm"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint FirstRetInt3(uint address, DisasmUnsafe* disasm, UnmanagedBuffer buffer)
        {
            int len = 0;
            disasm->EIP = buffer.AddressToEip(address);
            while (disasm->EIP < buffer.BufferCodeSectionEnd)
            {
                len = BeaEngine.Disasm(disasm);                

                switch (disasm->Instruction.Opcode)
                {
                    case INSTR_RET:
                    case INSTR_RETN:
                    case INSTR_RETFN:
                    case INSTR_RETF:
                        disasm->EIP = (uint)(disasm->EIP + len);
                        len = BeaEngine.Disasm(disasm);
                        if (len != (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE && disasm->Instruction.Opcode == INSTR_INT3)
                            return buffer.EipToAddress(disasm->EIP);
                        break;
                }

                if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    len = 1;

                disasm->EIP = (uint)(disasm->EIP + len);
            }
            return uint.MaxValue;
        }

        /// <summary>
        /// Finds the first JMP followed by INT3 opcodes after a given address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="disasm"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint FirstJmpInt3(uint address, DisasmUnsafe* disasm, UnmanagedBuffer buffer)
        {
            int len = 0;
            disasm->EIP = buffer.AddressToEip(address);
            while (disasm->EIP < buffer.BufferCodeSectionEnd)
            {
                len = BeaEngine.Disasm(disasm);                

                switch (disasm->Instruction.Opcode)
                {
                    case INSTR_RELJMP:
                        disasm->EIP = (uint)(disasm->EIP + len);
                        len = BeaEngine.Disasm(disasm);
                        if (len != (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE && disasm->Instruction.Opcode == INSTR_INT3)
                            return buffer.EipToAddress(disasm->EIP);
                        break;
                }

                if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    len = 1;

                disasm->EIP = (uint)(disasm->EIP + len);
            }
            return uint.MaxValue;
        }        

        /// <summary>
        /// Checks if the given address's opcode is an INT3 opcode
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool IsAddressInt3(uint address, UnmanagedBuffer buffer)
        {
            return Marshal.ReadByte((IntPtr)buffer.AddressToEip(address)) == INSTR_INT3;
        }

        /// <summary>
        /// Finds the first address before the given address which isn't an INT3 opcode
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint FindFirstNonInt3BeforeAddress(uint address, UnmanagedBuffer buffer)
        {
            int i = 0;
            while (IsPreviousOpcodeInt3((uint)(address + i), buffer))
                i--;
            return (uint)(address + i);
        }

        /// <summary>
        /// Checks if the previous opcode is a RET opcode for the given address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool IsPreviousOpcodeRet(uint address, UnmanagedBuffer buffer)
        {
            byte opcode = Marshal.ReadByte((IntPtr)buffer.AddressToEip(address - 1));
            if (opcode == INSTR_RETN || opcode == INSTR_RETF)
                return true;

            opcode = Marshal.ReadByte((IntPtr)buffer.AddressToEip(address - 3));
            return opcode == INSTR_RET || opcode == INSTR_RETFN;
        }

        public static bool IsPreviousOpcodeJmp(uint address, UnmanagedBuffer buffer)
        {
            return Marshal.ReadByte((IntPtr)buffer.AddressToEip(address - 5)) == INSTR_RELJMP;
        }

        public static bool IsPreviousOpcodeCall(uint address, UnmanagedBuffer buffer)
        {
            return Marshal.ReadByte((IntPtr)buffer.AddressToEip(address - 5)) == INSTR_CALL;
        }

        /// <summary>
        /// Checks if there is an INT3 opcode directly before a given address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool IsPreviousOpcodeInt3(uint address, UnmanagedBuffer buffer)
        {
            return Marshal.ReadByte((IntPtr)(buffer.AddressToEip(address) - 1)) == INSTR_INT3;
        }

        public static bool IsOpcodeInt3(uint address, UnmanagedBuffer buffer)
        {
            return Marshal.ReadByte((IntPtr)(buffer.AddressToEip(address))) == INSTR_INT3;
        }

        public static bool IsOpcodeRet(int opcode)
        {
            return opcode == INSTR_RET || opcode == INSTR_RETF || opcode == INSTR_RETFN || opcode == INSTR_RETN;
        }
    }
}
