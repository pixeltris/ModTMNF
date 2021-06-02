using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ModTMNF.Game;
using Bea;
using System.Runtime.InteropServices;

namespace ModTMNF.Analysis.Asm
{
    class AsmHelper
    {
        private static void LogSlowWarning()
        {
            Console.WriteLine("Running ASM helper doc gen (slow pre-processing step)");
        }

        public static void RuntimeGenerateDocs()
        {
            LogSlowWarning();

            List<SymbolsHelper.SymbolInfo> symbols = SymbolsHelper.LoadSymbols();
            string newFuncStr = "void * __cdecl operator new(unsigned int)";
            SymbolsHelper.SymbolInfo newFunc = symbols.FirstOrDefault(x => x.Name == newFuncStr);
            Debug.Assert(newFunc != null);

            // TODO: Dump the function length list / reference list from IDA which would be a lot more accurate
            Dictionary<uint, FunctionInfo> functions = ExeInfo.ParseFile(SymbolsHelper.ExeName, SymbolsHelper.LoadSymbols());
            using (UnmanagedBuffer buffer = ExeInfo.GetBuffer(SymbolsHelper.ExeName))
            {
                //RuntimeGenerateStructSizes(buffer, functions, newFunc);
                RuntimeGenerateVTableList(functions);
            }
        }

        private static void RuntimeGenerateVTableList(Dictionary<uint, FunctionInfo> functions)
        {
            List<SymbolsHelper.SymbolInfo> syms = SymbolsHelper.LoadSymbols();
            HashSet<uint> vtableAddrs = new HashSet<uint>();
            Dictionary<SymbolsHelper.SymbolInfo, List<SymbolsHelper.SymbolInfo>> vtables = new Dictionary<SymbolsHelper.SymbolInfo, List<SymbolsHelper.SymbolInfo>>();
            Dictionary<uint, SymbolsHelper.SymbolInfo> funcSyms = new Dictionary<uint, SymbolsHelper.SymbolInfo>();
            Dictionary<uint, SymbolsHelper.SymbolInfo> allSyms = new Dictionary<uint, SymbolsHelper.SymbolInfo>();
            foreach (SymbolsHelper.SymbolInfo sym in syms)
            {
                allSyms[sym.Address] = sym;
                if (sym.FuncComp != null)
                {
                    funcSyms[sym.Address] = sym;
                }
                if (sym.Type == SymbolsHelper.SymbolType.VTable)
                {
                    vtableAddrs.Add(sym.Address);
                }
            }
            foreach (SymbolsHelper.SymbolInfo sym in syms)
            {
                if (sym.Type == SymbolsHelper.SymbolType.VTable)
                {
                    List<SymbolsHelper.SymbolInfo> table = new List<SymbolsHelper.SymbolInfo>();
                    vtables[sym] = table;
                    for (int i = 0; i < 1000; i++)
                    {
                        uint addr = (uint)(sym.Address + (i * 4));
                        if (i > 0 && vtableAddrs.Contains(addr))
                        {
                            // vtable overlap
                            break;
                        }
                        try
                        {
                            addr = (uint)Marshal.ReadInt32((IntPtr)addr);
                        }
                        catch
                        {
                            // Invalid read
                            break;
                        }
                        SymbolsHelper.SymbolInfo targetSym;
                        if (allSyms.TryGetValue(addr, out targetSym))
                        {
                            table.Add(targetSym);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            RuntimeGenerateVTableList(vtables, trimmedNodTypes: true);
            RuntimeGenerateVTableList(vtables, trimmedNodTypes: false);
        }

        private static void RuntimeGenerateVTableList(Dictionary<SymbolsHelper.SymbolInfo, List<SymbolsHelper.SymbolInfo>> vtables, bool trimmedNodTypes)
        {
            Dictionary<EMwClassId, List<SymbolsHelper.SymbolInfo>> nodVTables = null;
            if (trimmedNodTypes)
            {
                nodVTables = new Dictionary<EMwClassId, List<SymbolsHelper.SymbolInfo>>();
                foreach (KeyValuePair<SymbolsHelper.SymbolInfo, List<SymbolsHelper.SymbolInfo>> vtable in vtables)
                {
                    EMwClassId classId;
                    if (Enum.TryParse<EMwClassId>(vtable.Key.CompType.Name, out classId))
                    {
                        nodVTables[classId] = vtable.Value;
                    }
                }
            }
            using (TextWriter tw = File.CreateText(Path.Combine(SymbolsHelper.DocsDir, "VTablesEx" + (trimmedNodTypes ? "Nod" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List of vtables for " + (trimmedNodTypes ?
                    " all CMwNod types (and duplicate vtable entries from child classes removed)" : " all types"));
                tw.WriteLine("TODO: Fix trash entries (due to shared function addresses)");
                tw.WriteLine();
                foreach (KeyValuePair<SymbolsHelper.SymbolInfo, List<SymbolsHelper.SymbolInfo>> vtable in vtables.OrderBy(x => x.Key.CompType.Name))
                {
                    string baseClassStr = "";
                    int baseFunctionCount = 0;
                    if (trimmedNodTypes)
                    {
                        EMwClassId classId;
                        if (!Enum.TryParse<EMwClassId>(vtable.Key.CompType.Name, out classId))
                        {
                            // Not a nod type
                            continue;
                        }
                        CMwClassInfo classInfo = CMwNod.StaticGetClassInfo(classId);
                        if (classInfo.Address == IntPtr.Zero)
                        {
                            continue;
                        }
                        classInfo = classInfo.Parent;
                        if (classInfo.Address != IntPtr.Zero)
                        {
                            baseClassStr = " baseClass:" + classInfo.Id;
                        }
                        while (classInfo.Address != IntPtr.Zero)
                        {
                            List<SymbolsHelper.SymbolInfo> baseFunctions;
                            if (nodVTables.TryGetValue(classInfo.Id, out baseFunctions))
                            {
                                baseFunctionCount = Math.Max(baseFunctionCount, baseFunctions.Count);
                            }
                            classInfo = classInfo.Parent;
                        }
                    }
                    if (!trimmedNodTypes || vtable.Value.Count > baseFunctionCount)
                    {
                        tw.WriteLine(vtable.Key.CompType.Name + " - entries:" + vtable.Value.Count + " addr:" + vtable.Key.Address.ToString("X8") + baseClassStr);
                        int cnt = 0;
                        foreach (SymbolsHelper.SymbolInfo entry in vtable.Value)
                        {
                            if (cnt++ < baseFunctionCount)
                            {
                                continue;
                            }
                            tw.WriteLine(" " + entry.Address.ToString("X8") + " " + (cnt-1).ToString().PadLeft(3, '0') + " " +
                                ((cnt - 1) * 4).ToString().PadLeft(4, '0') + " " +
                                (entry.FuncComp != null ? entry.FuncComp.FullName : entry.CompType != null ? entry.CompType.FullName : entry.Name));
                        }
                        tw.WriteLine();
                    }
                }
            }
        }

        private static void RuntimeGenerateStructSizes(UnmanagedBuffer buffer, Dictionary<uint, FunctionInfo> functions, SymbolsHelper.SymbolInfo newFuncSym)
        {
            // <ctorAddr, list<sizes>>
            Dictionary<uint, HashSet<int>> foundStructSizes = new Dictionary<uint, HashSet<int>>();

            List<SymbolsHelper.SymbolInfo> symbols = SymbolsHelper.LoadSymbols();
            Dictionary<uint, SymbolsHelper.SymbolInfo> allCtors = new Dictionary<uint, SymbolsHelper.SymbolInfo>();
            foreach (SymbolsHelper.SymbolInfo symbol in symbols)
            {
                if (symbol.FuncComp != null && symbol.FuncComp.IsConstructor)
                {
                    allCtors[symbol.Address] = symbol;
                }
            }
            Debug.Assert(functions.ContainsKey(newFuncSym.Address));
            FunctionInfo newFunc = functions[newFuncSym.Address];
            foreach (KeyValuePair<uint, FunctionInfo> funcRef in newFunc.References)
            {
                if (funcRef.Value.Symbol.FuncComp.IsConstructor)
                {
                    // This might miss some real values, but often this is a base ctor invoking a ctor and the size will be the parent ctor.
                    // We can't rely on this value for anything as we can't confirm the relationship.
                    continue;
                }
                SymbolsHelper.SymbolInfo refSym = funcRef.Value.Symbol;

                unsafe
                {
                    DisasmUnsafe disasm;

                    // This isn't going to be 100% accurate, but it should be fairly accurate.
                    // Checking "PUSH XX" first helps as a "PUSH XXXXXXXX" is unlikely to be "XX XX 6A YY" (large)
                    int push = -1;
                    disasm.EIP = buffer.AddressToEip(funcRef.Key - 2);
                    disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);
                    int len = BeaEngine.Disasm(&disasm);
                    if (len > 0)
                    {
                        if (disasm.Instruction.Opcode == 0x6A)// PUSH XX
                        {
                            push = (int)disasm.Instruction.Immediat;
                        }
                    }
                    disasm.EIP = buffer.AddressToEip(funcRef.Key - 5);
                    disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);
                    len = BeaEngine.Disasm(&disasm);
                    if (len > 0 && push == -1)
                    {
                        if (disasm.Instruction.Opcode == 0x68)// PUSH XXXXXXXX
                        {
                            push = (int)disasm.Instruction.Immediat;
                        }
                    }
                    if (push > 0)
                    {
                        bool finish = false;
                        bool hasJe = false;
                        uint callTarget = 0;

                        disasm.EIP = buffer.AddressToEip(funcRef.Key + 5);
                        disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);
                        for (int i = 0; i < 100 && !finish; i++)
                        {
                            len = BeaEngine.Disasm(&disasm);
                            if (len <= 0)
                            {
                                break;
                            }
                            if (disasm.Instruction.BranchType > 0)
                            {
                                switch ((BeaConstants.BranchType)disasm.Instruction.BranchType)
                                {
                                    case BeaConstants.BranchType.CallType:
                                        callTarget = (uint)disasm.Instruction.AddrValue;
                                        finish = true;
                                        break;
                                    case BeaConstants.BranchType.JmpType:
                                        finish = true;
                                        break;
                                    default:
                                        if (hasJe)
                                        {
                                            finish = true;
                                        }
                                        hasJe = true;
                                        break;
                                }
                            }
                            disasm.EIP = (uint)(disasm.EIP + len);
                            disasm.VirtualAddr = buffer.EipToAddress(disasm.EIP);
                        }
                        if (callTarget > 0)
                        {
                            SymbolsHelper.SymbolInfo targetSym;
                            if (allCtors.TryGetValue(callTarget, out targetSym))
                            {
                                HashSet<int> sizeList;
                                if (!foundStructSizes.TryGetValue(callTarget, out sizeList))
                                {
                                    foundStructSizes[callTarget] = sizeList = new HashSet<int>();
                                }
                                sizeList.Add(push);
                            }
                        }
                    }
                }
            }

            // The compiler used seems to use ESI for offsets in ctors. I'm not sure if this is the case for all ctors
            // and I'm sure that pattern is broken in many places, but we could use it to gather some useful info.
            // TODO: Look a pattern of most MOVed target register pointer offsets
            // -----
            // This finds all referenced offsets in ctors by looking for "MOV [ESI+XXX],YYY"
            Dictionary<uint, HashSet<uint>> ctorStructOffsets = new Dictionary<uint, HashSet<uint>>();
            Dictionary<uint, uint> ctorPossibleBaseCtor = new Dictionary<uint, uint>();
            foreach (KeyValuePair<uint, SymbolsHelper.SymbolInfo> ctor in allCtors)
            {
                HashSet<uint> offsets = new HashSet<uint>();
                ctorStructOffsets[ctor.Key] = offsets;
                unsafe
                {
                    FunctionInfo ctorFuncInfo;
                    if (functions.TryGetValue(ctor.Key, out ctorFuncInfo))
                    {
                        uint firstCall = 0;
                        DisasmUnsafe disasm;
                        disasm.EIP = buffer.AddressToEip(ctor.Key);
                        disasm.VirtualAddr = buffer.EipToAddress((uint)disasm.EIP);
                        if (ctor.Key == 0x004406D0)
                        {
                        }
                        while (disasm.VirtualAddr < ctor.Key + ctorFuncInfo.Length)
                        {
                            int len = BeaEngine.Disasm(&disasm);
                            if (len <= 0)
                            {
                                break;
                            }
                            if (disasm.Instruction.BranchType == (int)BeaConstants.BranchType.CallType && firstCall == 0)
                            {
                                if (disasm.Instruction.AddrValue > 0 && allCtors.ContainsKey((uint)disasm.Instruction.AddrValue))
                                {
                                    ctorPossibleBaseCtor[ctor.Key] = (uint)disasm.Instruction.AddrValue;
                                }
                                else
                                {
                                    firstCall = 1;// Dummy value
                                }
                            }
                            // Check for MOV [ESI+XXX],YYY
                            if (disasm.Mnemonic == Mnemonic.Mov && disasm.Argument1.Memory.Displacement > 0 &&
                                ((BeaConstants.ArgumentType)disasm.Argument1.Memory.BaseRegister & BeaConstants.ArgumentType.REG6) == BeaConstants.ArgumentType.REG6)
                            {
                                offsets.Add((uint)disasm.Argument1.Memory.Displacement);
                            }
                            disasm.EIP = (uint)(disasm.EIP + len);
                            disasm.VirtualAddr = buffer.EipToAddress(disasm.EIP);
                        }
                    }
                }
            }

            // Very inaccurate results with this ctor -> base ctor code
            /*foreach (KeyValuePair<uint, uint> ctor in ctorPossibleBaseCtor)
            {
                FunctionInfo ctorFunc;
                FunctionInfo baseCtorFunc;
                if (functions.TryGetValue(ctor.Key, out ctorFunc) &&
                    functions.TryGetValue(ctor.Value, out baseCtorFunc))
                {
                    Debug.WriteLine(ctorFunc.Symbol.FuncComp.FullName + " : " + baseCtorFunc.Symbol.FuncComp.FullName);
                }
            }*/

            GenerateStructSizes(false, false, ctorStructOffsets, functions, foundStructSizes);//all
            GenerateStructSizes(true, false, ctorStructOffsets, functions, foundStructSizes);//nod types
            GenerateStructSizes(true, true, ctorStructOffsets, functions, foundStructSizes);//nod types (code gen)
        }

        private static void GenerateStructSizes(bool nodTypes, bool code,
            Dictionary<uint, HashSet<uint>> ctorStructOffsets,
            Dictionary<uint, FunctionInfo> functions,
            Dictionary<uint, HashSet<int>> foundStructSizes)
        {
            using (TextWriter tw = File.CreateText(Path.Combine(SymbolsHelper.DocsDir,
                "StructSizes" + (!nodTypes ? "_All" : string.Empty) +
                (code ? "_Code" : string.Empty) +
                (code ? ".cs" : ".txt"))))
            {
                tw.WriteLine(
                    (code ? "//" : string.Empty) +
                    "Struct / class sizes determined by looking at ASM" +
                    (nodTypes ? " (CMwNod types)" : string.Empty));
                tw.WriteLine();
                Dictionary<string, uint> classCtorNames = new Dictionary<string, uint>();
                foreach (KeyValuePair<uint, HashSet<uint>> structOffsetInfo in ctorStructOffsets)
                {
                    FunctionInfo ctorFunc;
                    if (functions.TryGetValue(structOffsetInfo.Key, out ctorFunc))
                    {
                        EMwClassId classId;
                        if (!nodTypes || Enum.TryParse<EMwClassId>(ctorFunc.Symbol.FuncComp.Name, out classId))
                        {
                            classCtorNames[ctorFunc.Symbol.FuncComp.Name] = structOffsetInfo.Key;
                        }
                    }
                }
                foreach (KeyValuePair<string, uint> ctorNameAddr in classCtorNames.OrderBy(x => x.Key))
                {
                    FunctionInfo ctorFunc;
                    if (functions.TryGetValue(ctorNameAddr.Value, out ctorFunc))
                    {
                        HashSet<uint> offsets = ctorStructOffsets[ctorNameAddr.Value];

                        int minOffset = int.MaxValue;
                        int maxOffset = int.MinValue;
                        foreach (uint offset in offsets)
                        {
                            minOffset = Math.Min((int)offset, minOffset);
                            maxOffset = Math.Max((int)offset, maxOffset);
                        }

                        HashSet<int> sizes;
                        foundStructSizes.TryGetValue(ctorNameAddr.Value, out sizes);
                        if (code)
                        {
                            tw.WriteLine("public static class " + ctorNameAddr.Key);
                            tw.WriteLine("{");
                            if (nodTypes)
                            {
                                EMwClassId classId;
                                if (Enum.TryParse(ctorNameAddr.Key, out classId))
                                {
                                    CMwClassInfo classInfo = CMwNod.StaticGetClassInfo(classId);
                                    if (classInfo.Address != IntPtr.Zero && classInfo.Parent.Address != IntPtr.Zero)
                                    {
                                        tw.WriteLine("    public static Type BaseType = typeof(OT." +
                                            classInfo.Parent.Id + ");");
                                    }
                                }
                                string sizeInfoStr = null;
                                if (sizes != null)
                                {
                                    sizeInfoStr = sizes.ElementAt(0) + ";";
                                    if (sizes.Count > 1)
                                    {
                                        sizeInfoStr += "// (" + string.Join(",", sizes) + ")";
                                    }
                                }
                                else
                                {
                                    sizeInfoStr = "0;// UNKNOWN (fixme)";
                                }
                                tw.WriteLine("    public static int StructSize = " + sizeInfoStr);
                            }
                            tw.WriteLine("}");
                        }
                        else
                        {
                            tw.WriteLine(ctorFunc.Symbol.FuncComp.Name +
                                    " | addr:" + ctorNameAddr.Value.ToString("X8") +
                                    " | minOffset:" + (offsets.Count > 0 ? minOffset : -1) +
                                    " | maxOffset:" + (offsets.Count > 0 ? maxOffset : -1) +
                                    " | offsets:" + offsets.Count +
                                    " | size:" + (sizes != null ? string.Join(",", sizes) : "UNKNOWN"));
                        }
                    }
                }
            }
        }
    }
}
