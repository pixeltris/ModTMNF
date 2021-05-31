using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Bea;

namespace ModTMNF.Analysis.Asm
{
    static class ExeInfo
    {
        public static int MaxFunctionLength = 50000;

        public static UnmanagedBuffer GetBuffer(string file)
        {
            UnmanagedBuffer buffer = new UnmanagedBuffer();
            PeHeaderReader peReader = new PeHeaderReader(file);
            if (peReader.BaseOfCode > 0)
            {
                buffer.ImageBase = (uint)peReader.ImageBase;
                buffer.CodeOffset = peReader.BaseOfCode;
                buffer.CodeSize = peReader.ImageSectionHeaders.FirstOrDefault(x => x.VirtualAddress == buffer.CodeOffset).VirtualSize;
                //buffer.CodeSize = peReader.OptionalHeader32.SizeOfCode;// Seems less than the actual code section size, look into
            }
            else
            {
                // Assume first section is code section
                buffer.CodeOffset = peReader.ImageSectionHeaders[0].VirtualAddress;
                buffer.CodeSize = peReader.ImageSectionHeaders[0].VirtualSize;
            }
            ulong totalSize = 0;
            for (int i = 1; i < peReader.ImageSectionHeaders.Length; i++)
                totalSize += peReader.ImageSectionHeaders[i].VirtualSize;

            using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
            {
                br.BaseStream.Position = buffer.CodeOffset;
                buffer.SetData(br.ReadBytes((int)buffer.CodeSize));
            }

            return buffer;
        }

        public static Dictionary<uint, FunctionInfo> ParseFile(string file, List<SymbolsHelper.SymbolInfo> symbols)
        {
            using (UnmanagedBuffer buffer = GetBuffer(file))
            {
                Dictionary<uint, FunctionInfo> functions = new Dictionary<uint, FunctionInfo>();
                foreach (SymbolsHelper.SymbolInfo symbol in symbols)
                {
                    if (symbol.FuncComp != null)
                    {
                        // NOTE: Multiple functions often share the same address / asm (compiler optimization)
                        // NOTE: Function length can be wrong, and functions might appear to overlap. This will break finding function references.
                        uint len = FunctionLengthHelper.GetFunctionLength(symbol.Address, buffer);
                        functions[symbol.Address] = new FunctionInfo(symbol, len);
                    }
                }

                unsafe
                {
                    DisasmUnsafe disasm;
                    disasm.EIP = (uint)buffer.Ptr;
                    disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);
                    FindFunctionReferences(ref functions, buffer, &disasm);
                }

                return functions;
            }
        }

        public static unsafe uint GetCallAddress(UnmanagedBuffer buffer, DisasmUnsafe* disasm)
        {
            uint functionAddress = 0;
            if (disasm->Instruction.AddrValue != 0 && disasm->Instruction.AddrValue >= buffer.CodeSectionStart && disasm->Instruction.AddrValue <= buffer.CodeSectionEnd)
            {
                functionAddress = (uint)disasm->Instruction.AddrValue;
            }
            else if (disasm->Argument1.Memory.Displacement != 0 && disasm->Argument1.Memory.IndexRegister == 0)
            {
                uint displacement = (uint)(disasm->Argument1.Memory.Displacement >> 32);
                if (displacement >= buffer.CodeSectionStart && displacement <= buffer.CodeSectionEnd)
                {
                    functionAddress = (uint)System.Runtime.InteropServices.Marshal.ReadInt32((IntPtr)(buffer.Ptr.ToInt32() + (displacement - buffer.CodeSectionStart)));

                    if (functionAddress < buffer.CodeSectionStart || functionAddress > buffer.CodeSectionEnd)
                        functionAddress = 0;
                }
            }
            return functionAddress;
        }

        static unsafe void FindFunctionReferences(ref Dictionary<uint, FunctionInfo> functions, UnmanagedBuffer buffer, DisasmUnsafe* disasm)
        {
            /*
             * To find all function references loop through each function and search for JMP and CALL branches, check if these JMP/ CALL opcodes
             * point to an address outside of the currnt function. If they do then do if(functions.ContainsKey(functionAddress)) checking if
             * the function exists then do functions[functionAddress].AddReference(referenceAddress, currentFunction); to add the reference
             * currentFunction will be the current function in the dictionary loop
             */

            int brokenFunctionCount = 0;
            int totalReference = 0;

            foreach (FunctionInfo function in functions.Values)
            {
                disasm->EIP = buffer.AddressToEip(function.Address);
                disasm->VirtualAddr = buffer.EipToAddress((uint)disasm->EIP);
                int len = 0;

                while (disasm->VirtualAddr < function.Address + function.Length)
                {
                    len = BeaEngine.Disasm(disasm);

                    if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    {
                        // Probably shouldn't have any unknown opcodes in known functions so stop scanning the function
                        brokenFunctionCount++;
                        Console.WriteLine("Broken function {0} len {1}", function.Address.ToString("X8"), function.Length);
                        break;
                    }
                    else if (disasm->Instruction.BranchType == (int)BeaConstants.BranchType.CallType || disasm->Instruction.Opcode == FunctionLengthHelper.INSTR_RELJMP)
                    {
                        uint functionAddress = GetCallAddress(buffer, disasm);
                        if (functionAddress != 0 && (functionAddress < function.Address || functionAddress > function.Address + function.Length) && functions.ContainsKey(functionAddress))
                        {
                            functions[functionAddress].AddReference(buffer.EipToAddress(disasm->EIP), function);
                            totalReference++;
                        }
                    }
                    disasm->EIP = (uint)(disasm->EIP + len);
                    disasm->VirtualAddr = buffer.EipToAddress((uint)disasm->EIP);
                }
            }
            Console.WriteLine("Found {0} broken functions when finding function references", brokenFunctionCount);
        }

        /// <summary>
        /// Finds all function references for a given function rather than for all functions
        /// </summary>
        /// <param name="function"></param>
        /// <param name="functions"></param>
        /// <param name="buffer"></param>
        /// <param name="disasm"></param>
        public static unsafe void FindFunctionReferences(FunctionInfo function, ref Dictionary<uint, FunctionInfo> functions, UnmanagedBuffer buffer, DisasmUnsafe* disasm)
        {
            foreach (FunctionInfo func in functions.Values)
            {
                if (func == function)
                    continue;

                disasm->EIP = buffer.AddressToEip(func.Address);
                disasm->VirtualAddr = buffer.EipToAddress((uint)disasm->EIP);
                int len = 0;

                while (disasm->VirtualAddr < func.Address + func.Length)
                {
                    len = BeaEngine.Disasm(disasm);

                    if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                        break;
                    else if (disasm->Instruction.BranchType == (int)BeaConstants.BranchType.CallType || disasm->Instruction.Opcode == FunctionLengthHelper.INSTR_RELJMP)
                    {
                        uint functionAddress = GetCallAddress(buffer, disasm);
                        if (functionAddress != 0 && functionAddress == function.Address)
                            functions[functionAddress].AddReference(buffer.EipToAddress(disasm->EIP), func);
                    }
                    disasm->EIP = (uint)(disasm->EIP + len);
                    disasm->VirtualAddr = buffer.EipToAddress((uint)disasm->EIP);
                }
            }
        }

        public static List<uint> FindFunctionReferences(FunctionInfo function, UnmanagedBuffer buffer)
        {
            List<uint> references = new List<uint>();
            int len = 0;

            unsafe
            {
                DisasmUnsafe disasm;
                disasm.EIP = (uint)buffer.Ptr;
                disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);

                while (disasm.EIP < buffer.BufferCodeSectionEnd)
                {
                    len = BeaEngine.Disasm(&disasm);

                    if (len == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    {
                        len = 1;
                    }
                    else if (disasm.Instruction.BranchType == (int)BeaConstants.BranchType.CallType)
                    {
                        uint functionAddress = 0;
                        if (disasm.Instruction.AddrValue != 0 && disasm.Instruction.AddrValue >= buffer.CodeSectionStart && disasm.Instruction.AddrValue <= buffer.CodeSectionEnd)
                        {
                            functionAddress = (uint)disasm.Instruction.AddrValue;
                        }
                        else if (disasm.Argument1.Memory.Displacement != 0 && disasm.Argument1.Memory.IndexRegister == 0)
                        {
                            uint displacement = (uint)(disasm.Argument1.Memory.Displacement >> 32);
                            if (displacement >= buffer.CodeSectionStart && displacement <= buffer.CodeSectionEnd)
                            {
                                functionAddress = (uint)System.Runtime.InteropServices.Marshal.ReadInt32((IntPtr)(buffer.Ptr.ToInt32() + (displacement - buffer.CodeSectionStart)));

                                if (functionAddress < buffer.CodeSectionStart || functionAddress > buffer.CodeSectionEnd)
                                    functionAddress = 0;
                            }
                        }
                        if (functionAddress == function.Address && !references.Contains(disasm.VirtualAddress))
                        {
                            references.Add(disasm.VirtualAddress);
                        }
                    }

                    disasm.EIP = (uint)(disasm.EIP + len);
                    disasm.VirtualAddr = buffer.EipToAddress(disasm.EIP);
                }
            }

            return references;
        }
    }
}