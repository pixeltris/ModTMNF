using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using ModTMNF.Game;
using System.Diagnostics;

namespace ModTMNF.Analysis
{
    static class SymbolsHelper
    {
        public static string DocsDir = Path.Combine("ModTMNF", "Generated");// Make this folder manually before running this code
        public const string MapName = "TmForever.map";
        public const string ExeName = "TmForever.exe";
        static List<SymbolInfo> symbolsCached;

        public static void GenerateDocs()
        {
            GenerateOllyDbg();
            //GenerateMissingCtorList();// One time thing to see what the struct size helper will be able to find
            GenerateObjListSimple();
            GenerateObjListByNamespace();
            GenerateNamespaceList();
            GenerateClassListByEngine();
            GenerateVTableAddressList();
            GenerateFuncListNonCMwNodVirtual(shortName: true);
            GenerateFuncListNonCMwNodVirtual(shortName: false);
            GenerateFuncListNonCMwNodAll(shortName: true);
            GenerateFuncListNonCMwNodAll(shortName: false);
            GenerateFuncListNonCMwNodAllUnscoped(shortName: true);
            GenerateFuncListNonCMwNodAllUnscoped(shortName: false);
            GenerateFuncListAllShort();
            GenerateTypeList();
            GenerateTypeListFlat();
            GenerateVirtualParamGetList();
            Asm.AsmHelper.GenerateDocs();
        }

        public static void GenerateRuntimeDocs()
        {
            //RuntimeGenerateClassList();// Not very useful
            RuntimeGenerateClassHierarchy();
            RuntimeGenerateClassMembers();
            RuntimeGenerateVTableAddressCode();
            RuntimeGenerateClassFuncList(shortName: true);
            RuntimeGenerateClassFuncList(shortName: false);
            RuntimeGenerateClassFuncListAll(shortName: true);
            RuntimeGenerateClassFuncListAll(shortName: false);
            RuntimeGenerateClassFuncListVTables(shortName: true);
            RuntimeGenerateClassFuncListVTables(shortName: false);
            Asm.AsmHelper.RuntimeGenerateDocs();
        }

        public enum SymbolType
        {
            Unused,
            VTable,
            String,
            Function,
            GlobalVariableFuncPtr,
            GlobalVariable,
            External
        }

        public enum SymbolAccess
        {
            Undefined,
            Private,
            Protected,
            Public
        }

        public enum SymbolModifier
        {
            None,
            Virtual,
            Static
        }

        public enum SymbolCallingConvention
        {
            None,
            Cdecl,
            StdCall,
            ThisCall
        }

        public class SymbolInfo
        {
            public SymbolType Type;
            public SymbolAccess Access;
            public SymbolModifier Modifier;// virtual / static
            public SymbolCallingConvention CallingConvention;
            public uint Address;
            public string MangledName;
            public string Name;
            public string Namespace;
            public string ObjName;

            // For functions
            public SymbolComponentInfo FuncCompReturn;// return type
            public SymbolComponentInfo FuncComp;// function name
            public List<SymbolComponentInfo> FuncCompArgList = new List<SymbolComponentInfo>();// arg list

            // For statics / vtable
            public SymbolComponentInfo CompType;// var type
            public SymbolComponentInfo Comp;// var name
        }

        public class SymbolComponentInfo
        {
            public string Name;
            public int PointerLevel;
            public SymbolTypeSpecifier TypeSpecifier;
            public bool IsUnsigned;
            public bool IsPointer
            {
                get { return PointerLevel > 0; }
            }
            public bool IsFunctionPointer;
            public bool IsGenericType;
            public bool IsOperator;
            public bool IsDestructor;
            public bool IsConstructor;
            /// <summary>
            /// More of a "left hand" e.g. Left1::Left2::Left3::Right (where all the "LeftX" values would appear here).
            /// </summary>
            public SymbolComponentInfo Outer;
            public List<SymbolComponentInfo> OuterCollection = new List<SymbolComponentInfo>();

            public string FullName { get; private set; }
            public string FullNameOuter { get; private set; }
            public string RootName { get; private set; }

            public void PostLoad()
            {
                SymbolComponentInfo comp = Outer;
                while (comp != null)
                {
                    OuterCollection.Add(comp);
                    comp = comp.Outer;
                }
                OuterCollection.Reverse();
                if (!string.IsNullOrEmpty(Name) && OuterCollection.Count > 0 && Name.StartsWith("operator"))
                {
                    IsOperator = true;
                }
                FullName = OuterCollection.Count == 0 ? Name : string.Join("::", OuterCollection.Select(x => x.Name)) + "::" + Name;
                RootName = OuterCollection.Count == 0 ? Name : OuterCollection[0].Name;
                FullNameOuter = OuterCollection.Count == 0 ? null : string.Join("::", OuterCollection.Select(x => x.Name));
                IsDestructor = Name.Contains("~");
                IsConstructor = Name == FullNameOuter;
            }
        }

        public enum SymbolTypeSpecifier
        {
            None,
            Enum,
            Struct,
            Union,
            Class
        }

        /// <summary>
        /// Not super accurate at the momemnt due to the bad generics detection...
        /// </summary>
        private static HashSet<string> GetGenericTypeNames(List<SymbolInfo> symbols)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (SymbolInfo sym in symbols)
            {
                if (sym.FuncComp != null && sym.FuncComp.IsGenericType)
                {
                    result.Add(sym.FuncComp.RootName);
                }
                if (sym.FuncCompArgList != null)
                {
                    foreach (SymbolComponentInfo arg in sym.FuncCompArgList)
                    {
                        GetGenericTypeNames(result, arg);
                    }
                }
                GetGenericTypeNames(result, sym.FuncCompReturn);
                GetGenericTypeNames(result, sym.CompType);
            }
            return result;
        }

        private static void GetGenericTypeNames(HashSet<string> names, SymbolComponentInfo comp)
        {
            if (comp == null)
            {
                return;
            }
            if (comp.IsGenericType)
            {
                names.Add(comp.Name);
            }
            foreach (SymbolComponentInfo outer in comp.OuterCollection)
            {
                GetGenericTypeNames(names, outer);
            }
        }

        public static List<SymbolInfo> LoadSymbols()
        {
            if (symbolsCached != null)
            {
                return symbolsCached;
            }
            symbolsCached = new List<SymbolInfo>();
            string[] lines = File.ReadAllLines(MapName);
            bool isContent = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("Publics by Value"))// Also see "Static symbols"
                {
                    isContent = true;
                    continue;
                }
                else if (!isContent || string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (!line.StartsWith("0"))
                {
                    // Hack due to the split of static variable symbol data
                    continue;
                }

                SymbolInfo symbolInfo = new SymbolInfo();

                int idx = line.IndexOf(' ');
                line = line.Substring(idx).Trim();

                idx = line.IndexOf(' ');
                symbolInfo.MangledName = line.Substring(0, idx);
                line = line.Substring(idx).Trim();

                idx = line.IndexOf(' ');
                symbolInfo.Address = uint.Parse(line.Substring(0, idx), System.Globalization.NumberStyles.HexNumber);
                line = line.Substring(idx + 4);

                idx = line.IndexOf(':');
                if (idx > 0)
                {
                    symbolInfo.Namespace = line.Substring(0, idx).Trim();
                    symbolInfo.ObjName = line.Substring(idx + 1).Trim();
                }
                else
                {
                    symbolInfo.ObjName = line.Trim();
                }

                string str = Demangle(symbolInfo.MangledName);
                symbolInfo.Name = str;
                if (str.IndexOf("`string'") >= 0)
                {
                    symbolInfo.Type = SymbolType.String;
                }
                else if (str.IndexOf("`scalar") >= 0 ||
                         str.IndexOf("`vector") >= 0 ||
                         str.IndexOf("`default") >= 0 ||
                         str.IndexOf("`dynamic") >= 0 ||
                         str.IndexOf("`vcall'") >= 0 ||
                         str.IndexOf("`RTTI") >= 0 ||
                         str.IndexOf("`eh") >= 0 ||
                         str.IndexOf("`public") >= 0 ||
                         str.IndexOf("`private") >= 0 ||
                         str.IndexOf("`protected") >= 0 ||
                         str.StartsWith("__unwindfunclet") ||
                         str.StartsWith("__ehhandler") ||
                         str.StartsWith("__catch") ||
                         str.Contains("std::"))
                {
                    symbolInfo.Type = SymbolType.Unused;
                }
                else if ((idx = str.IndexOf("`vftable'")) >= 0)
                {
                    int scopeIdx = str.LastIndexOf("::");
                    if (scopeIdx > 0)
                    {
                        symbolInfo.Type = SymbolType.VTable;
                        str = str.Substring(0, scopeIdx);
                        symbolInfo.CompType = GetSymbolComponent(ref str);
                        Debug.Assert(string.IsNullOrEmpty(str));
                    }
                }
                else
                {
                    // Access specifier
                    if (str.StartsWith("public:"))
                    {
                        symbolInfo.Access = SymbolAccess.Public;
                        str = str.Substring(8);
                    }
                    else if (str.StartsWith("protected:"))
                    {
                        symbolInfo.Access = SymbolAccess.Protected;
                        str = str.Substring(11);
                    }
                    else if (str.StartsWith("private:"))
                    {
                        symbolInfo.Access = SymbolAccess.Private;
                        str = str.Substring(9);
                    }

                    if (str.StartsWith("virtual"))
                    {
                        symbolInfo.Modifier = SymbolModifier.Virtual;
                        str = str.Substring(8);
                    }
                    else if (str.StartsWith("static"))
                    {
                        symbolInfo.Modifier = SymbolModifier.Static;
                        str = str.Substring(7);
                    }

                    string returnTypeStr = null;

                    // Calling convention (and detect global function pointers)
                    // TODO: Handle cases where there's a global static variable which is a function pointer (as this code will break)
                    SymbolCallingConvention firstCc = SymbolCallingConvention.None;
                    int firstCcIdx = int.MaxValue;
                    int firstCcLen = 0;
                    if ((idx = str.IndexOf("__thiscall")) >= 0 && idx < firstCcIdx)
                    {
                        firstCcIdx = idx;
                        firstCc = SymbolCallingConvention.ThisCall;
                        firstCcLen = "__thiscall".Length;
                    }
                    if ((idx = str.IndexOf("__cdecl")) >= 0 && idx < firstCcIdx)
                    {
                        firstCcIdx = idx;
                        firstCc = SymbolCallingConvention.Cdecl;
                        firstCcLen = "__cdecl".Length;
                    }
                    if ((idx = str.IndexOf("__stdcall")) >= 0 && idx < firstCcIdx)
                    {
                        firstCcIdx = idx;
                        firstCc = SymbolCallingConvention.StdCall;
                        firstCcLen = "__stdcall".Length;
                    }
                    if (firstCc != SymbolCallingConvention.None)
                    {
                        symbolInfo.CallingConvention = firstCc;
                        if (str[firstCcIdx + firstCcLen] == '*' || str.IndexOf('(') < firstCcIdx)
                        {
                            symbolInfo.Type = SymbolType.GlobalVariableFuncPtr;
                        }
                        else
                        {
                            if (firstCcIdx > 0)
                            {
                                returnTypeStr = str.Substring(0, firstCcIdx);
                            }
                            str = str.Substring(firstCcIdx + firstCcLen + 1);
                        }
                    }

                    // Special case statics
                    if (symbolInfo.Type == SymbolType.Unused &&
                        symbolInfo.CallingConvention == SymbolCallingConvention.None &&
                        symbolInfo.Modifier != SymbolModifier.Static &&
                        (str.Contains("__NatBidon_") || 
                        str.Contains("class CMwIdStatic")))
                    {
                        symbolInfo.Modifier = SymbolModifier.Static;
                        // NOTE: There are more which don't follow a common syntax. Interesting ones:
                        //struct SDx9Static s_Dx9
                        //class CFastBuffer<class CSystemFid *> s_CallStackFids
                        //class CFastBuffer<class CSystemFile *> s_SystemFile_Read_InCalls
                        //class CFastString IdName
                        //class CClassicLog TheClassicLog
                        //class CFastBuffer<class CClassicBufferMemory *> s_UnusedBufferMemorys
                        //struct HINSTANCE__ * e_FastAssert_hInstance
                        //struct HWND__ * e_FastAssert_hMainWnd
                        //struct SHmsViewport_InputMousePointerInfo * s_HmsViewport_InputMousePointerInfo
                        //char const * const s_CSystemFids_GetAllChildFidByClassId_Name
                        //class CFixedArray<wchar_t const *,6,unsigned long> const s_MedalFullNames
                    }

                    if (symbolInfo.Type == SymbolType.GlobalVariableFuncPtr)
                    {
                        // There are only a few of these, and nothing too interesting (other than maybe m_CollisionFuncTable)
                        //Debug.WriteLine(symbolInfo.Name);
                    }
                    else if (symbolInfo.CallingConvention != SymbolCallingConvention.None)
                    {
                        symbolInfo.Type = SymbolType.Function;

                        if (!string.IsNullOrEmpty(returnTypeStr))
                        {
                            symbolInfo.FuncCompReturn = GetSymbolComponent(ref returnTypeStr);
                            Debug.Assert(string.IsNullOrEmpty(returnTypeStr) || returnTypeStr == "const");
                        }

                        string funcNameStr = null;
                        List<string> funcComponentsStr = new List<string>();
                        int braceDepth1 = 0;
                        int braceDepth2 = 0;
                        int componentStartIndex = -1;
                        for (int j = 0; j < str.Length; j++)
                        {
                            if (str[j] == '<')
                            {
                                braceDepth2++;
                            }
                            else if (str[j] == '>')
                            {
                                braceDepth2--;
                            }
                            if (str[j] == '(')
                            {
                                braceDepth1++;
                                if (braceDepth1 == 1)
                                {
                                    funcNameStr = str.Substring(0, j);
                                    componentStartIndex = j + 1;
                                }
                            }
                            else if (str[j] == ')')
                            {
                                braceDepth1--;
                                if (braceDepth1 == 0 && braceDepth2 == 0)
                                {
                                    string compName = str.Substring(componentStartIndex, j - componentStartIndex).Trim();
                                    funcComponentsStr.Add(compName);
                                }
                            }
                            if (str[j] == ',' && braceDepth1 == 1 && braceDepth2 == 0)
                            {
                                string compName = str.Substring(componentStartIndex, j - componentStartIndex).Trim();
                                funcComponentsStr.Add(compName);
                                componentStartIndex = j + 1;
                            }
                        }

                        symbolInfo.FuncComp = GetSymbolComponent(ref funcNameStr);
                        Debug.Assert(string.IsNullOrEmpty(funcNameStr));

                        foreach (string funcCompStr in funcComponentsStr)
                        {
                            string funcCompStr2 = funcCompStr;
                            symbolInfo.FuncCompArgList.Add(GetSymbolComponent(ref funcCompStr2));
                            if (funcCompStr.Contains('('))
                            {
                                symbolInfo.FuncCompArgList.Last().IsFunctionPointer = true;
                                //Debug.WriteLine(symbolInfo.Name);
                            }
                            else
                            {
                                Debug.Assert(string.IsNullOrEmpty(funcCompStr2) || funcCompStr2 == "const");
                            }
                        }
                    }
                    else if (symbolInfo.Modifier == SymbolModifier.Static)
                    {
                        if (str.Contains('[') || str.Contains('('))
                        {
                            // TODO: Handle static array types/func ptrs. There's only three of these:
                            //unsigned long (* CDx9StateBlock::s_SamplerStates)[14]
                            //unsigned long (* CDx9StateBlock::s_TexStageStates)[33]
                            //class GxTexCoord * (* CCrystalQuadEqui::s_GeomCoords)[6]
                        }
                        else
                        {
                            symbolInfo.Type = SymbolType.GlobalVariable;
                            symbolInfo.CompType = GetSymbolComponent(ref str);
                            symbolInfo.Comp = GetSymbolComponent(ref str);
                            Debug.Assert(!string.IsNullOrWhiteSpace(symbolInfo.CompType.Name));
                            Debug.Assert(!string.IsNullOrWhiteSpace(symbolInfo.Comp.Name));
                            Debug.Assert(string.IsNullOrWhiteSpace(str));
                        }
                    }
                    else
                    {
                        // Likely an external (e.g. _malloc)
                        symbolInfo.Type = SymbolType.External;
                    }
                }
                symbolsCached.Add(symbolInfo);
            }
            return symbolsCached;
        }

        private static SymbolComponentInfo GetSymbolComponent(ref string str)
        {
            SymbolComponentInfo comp = GetSymbolComponentImpl(ref str);
            comp.PostLoad();
            return comp;
        }

        private static SymbolComponentInfo GetSymbolComponentImpl(ref string str)
        {
            SymbolComponentInfo result = new SymbolComponentInfo();
            str = str.Trim();
            int startIndex = 0;
            bool isLookingForConstOrPointer = false;
            int lastIndex = -1;
            for (int i = startIndex; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case ' ':
                        string content = str.Substring(startIndex, i - startIndex).Replace("*", string.Empty).Trim();
                        if (isLookingForConstOrPointer)
                        {
                            switch (content)
                            {
                                case "const":
                                    lastIndex = i;
                                    break;
                                default:
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        str = str.Substring(lastIndex).Trim();
                                        return result;
                                    }
                                    break;
                            }
                        }
                        switch (content)
                        {
                            case "const":
                                break;
                            case "class":
                                result.TypeSpecifier = SymbolTypeSpecifier.Class;
                                break;
                            case "struct":
                                result.TypeSpecifier = SymbolTypeSpecifier.Struct;
                                break;
                            case "union":
                                result.TypeSpecifier = SymbolTypeSpecifier.Struct;
                                break;
                            case "enum":
                                result.TypeSpecifier = SymbolTypeSpecifier.Enum;
                                break;
                            case "unsigned":
                                result.IsUnsigned = true;
                                break;
                            case "operator":
                                result.IsOperator = true;
                                break;
                            default:
                                if (!string.IsNullOrEmpty(content))
                                {
                                    result.Name = content.Trim();
                                    isLookingForConstOrPointer = true;
                                    lastIndex = i;
                                }
                                break;
                        }
                        startIndex = i;
                        break;
                    case '&':
                    case '*':
                        result.PointerLevel++;
                        if (isLookingForConstOrPointer)
                        {
                            lastIndex = i + 1;
                        }
                        startIndex = i;
                        break;
                    case ':':
                        if (isLookingForConstOrPointer)
                        {
                            str = str.Substring(lastIndex).Trim();
                            return result;
                        }
                        if (str[i + 1] == ':')
                        {
                            SymbolComponentInfo outer = result;
                            result = new SymbolComponentInfo();
                            result.TypeSpecifier = outer.TypeSpecifier;
                            result.Outer = outer;
                            if (string.IsNullOrWhiteSpace(outer.Name))
                            {
                                outer.Name = str.Substring(startIndex, i - startIndex).Trim();
                            }
                            outer.TypeSpecifier = SymbolTypeSpecifier.None;
                            startIndex = i + 2;
                        }
                        break;
                    case '<':
                    case '(':
                    case '[':
                        if (str.Substring(startIndex).StartsWith("operator"))
                        {
                            result.IsOperator = true;
                        }
                        else
                        {
                            // TODO: Handle generics
                            char startBraceChar = str[i];
                            char endBraceChar = (char)0;
                            switch (str[i])
                            {
                                case '<':
                                    endBraceChar = '>';
                                    result.IsGenericType = true;
                                    break;
                                case '(':
                                    endBraceChar = ')';
                                    break;
                                case '[':
                                    endBraceChar = ']';
                                    break;
                            }
                            int braceCnt = 1;
                            int braceEndIndex = i + 1;
                            for (; braceEndIndex < str.Length && braceCnt > 0; braceEndIndex++)
                            {
                                if (str[braceEndIndex] == endBraceChar)
                                {
                                    braceCnt--;
                                }
                                else if (str[braceEndIndex] == startBraceChar)
                                {
                                    braceCnt++;
                                }
                            }
                            str = str.Remove(i, braceEndIndex - i);
                            i--;// To get back to a space
                        }
                        break;
                }
            }
            if (isLookingForConstOrPointer)
            {
                str = str.Substring(lastIndex).Trim();
            }
            else
            {
                string remain = str.Substring(startIndex).Trim();
                switch (remain)
                {
                    case "const":
                    case "class":
                    case "struct":
                    case "union":
                    case "enum":
                    case "unsigned":
                    case "operator":
                        break;
                    default:
                        if (!string.IsNullOrEmpty(remain))
                        {
                            result.Name = remain.Trim();
                            str = string.Empty;
                        }
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Generates "an.txt" from TmForever.map which can be loaded by AnOlly.dll plugin for OllyDbg
        /// </summary>
        public static void GenerateOllyDbg()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "an.txt")))
            {
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo symbol in symbols)
                {
                    tw.WriteLine("|AnL|" + symbol.Address.ToString("X8") + " " + symbol.Name);
                }
            }
        }

        public static void GenerateMissingCtorList()
        {
            Dictionary<string, SymbolInfo> symbolsWithCtors = new Dictionary<string, SymbolInfo>();
            List<SymbolInfo> symbols = LoadSymbols();
            foreach (SymbolInfo symbol in symbols)
            {
                if (symbol.FuncComp != null && symbol.FuncComp.IsConstructor)
                {
                    symbolsWithCtors[symbol.FuncComp.Name] = symbol;
                }
            }
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "MissingCtorList.txt")))
            {
                int numFound = 0;
                tw.WriteLine("List of CMwNod types without ctors");
                tw.WriteLine();
                foreach (EMwClassId classId in Enum.GetValues(typeof(EMwClassId)))
                {
                    SymbolInfo symbolInfo;
                    if (!symbolsWithCtors.TryGetValue(classId.ToString(), out symbolInfo))
                    {
                        tw.WriteLine(classId);
                    }
                    else
                    {
                        numFound++;
                    }
                }
                tw.WriteLine();
                tw.WriteLine("Total found: " + numFound);
            }
        }

        public static void GenerateObjListSimple()
        {
            HashSet<string> objNamesSet = new HashSet<string>();
            List<SymbolInfo> symbols = LoadSymbols();
            foreach (SymbolInfo symbol in symbols)
            {
                objNamesSet.Add(symbol.ObjName);
            }
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ObjList.txt")))
            {
                tw.WriteLine("List of all .obj files referenced in the .map");
                tw.WriteLine();
                foreach (string objName in objNamesSet.ToList().OrderBy(x => x))
                {
                    tw.WriteLine(objName);
                }
            }
        }

        public static void GenerateObjListByNamespace()
        {
            Dictionary<string, HashSet<string>> namespaceObjs = new Dictionary<string, HashSet<string>>();
            List<SymbolInfo> symbols = LoadSymbols();
            foreach (SymbolInfo symbol in symbols)
            {
                string ns = symbol.Namespace;
                if (string.IsNullOrWhiteSpace(ns))
                {
                    ns = "(null)";
                }
                HashSet<string> objList;
                if (!namespaceObjs.TryGetValue(ns, out objList))
                {
                    namespaceObjs.Add(ns, objList = new HashSet<string>());
                }
                objList.Add(symbol.ObjName);
            }
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ObjListByNamespace.txt")))
            {
                tw.WriteLine("List of all .obj files by namespace referenced in the .map");
                tw.WriteLine();
                foreach (KeyValuePair<string, HashSet<string>> kv in namespaceObjs.OrderBy(x => x.Key))
                {
                    tw.WriteLine("- " + kv.Key);
                    foreach (string ns in kv.Value.ToList().OrderBy(x => x))
                    {
                        tw.WriteLine("  - " + ns);
                    }
                }
            }
        }

        public static void GenerateNamespaceList()
        {
            HashSet<string> namespaces = new HashSet<string>();
            List<SymbolInfo> symbols = LoadSymbols();
            foreach (SymbolInfo symbol in symbols)
            {
                if (!string.IsNullOrWhiteSpace(symbol.Namespace))
                {
                    namespaces.Add(symbol.Namespace);
                }
            }
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "NamespaceList.txt")))
            {
                tw.WriteLine("List of all namespaces referenced in the .map");
                tw.WriteLine();
                foreach (string ns in namespaces.ToList().OrderBy(x => x))
                {
                    tw.WriteLine(ns);
                }
            }
        }

        public static void GenerateClassListByEngine()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassListByEngine.txt")))
            {
                tw.WriteLine("List of all class names grouped into their respective engine");
                tw.WriteLine();
                foreach (EMwEngineId engineId in Enum.GetValues(typeof(EMwEngineId)))
                {
                    List<string> classNames = new List<string>();
                    foreach (EMwClassId classId in Enum.GetValues(typeof(EMwClassId)))
                    {
                        if (CMwEngineManager.GetEngineIdFromClassId(classId) == engineId)
                        {
                            classNames.Add(classId.ToString());
                        }
                    }
                    classNames.Sort();
                    if (classNames.Count > 0)
                    {
                        tw.WriteLine(engineId);
                        foreach (string className in classNames)
                        {
                            tw.WriteLine(" " + className);
                        }
                        tw.WriteLine();
                    }
                }
            }
        }

        public static void GenerateTypeList()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "TypeList.txt")))
            {
                tw.WriteLine("List of all types");
                tw.WriteLine();
                HashSet<string> unknowns = new HashSet<string>();
                HashSet<string> enums = new HashSet<string>();
                HashSet<string> structs = new HashSet<string>();
                HashSet<string> unions = new HashSet<string>();
                HashSet<string> classes = new HashSet<string>();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo symbol in symbols)
                {
                    if (symbol.FuncComp != null)
                    {
                        List<SymbolComponentInfo> comps = new List<SymbolComponentInfo>(symbol.FuncCompArgList);
                        if (symbol.FuncCompReturn != null)
                        {
                            comps.Add(symbol.FuncCompReturn);
                        }
                        unknowns.Add(symbol.FuncComp.FullNameOuter);
                        foreach (SymbolComponentInfo comp in comps)
                        {
                            switch (comp.TypeSpecifier)
                            {
                                case SymbolTypeSpecifier.Enum:
                                    enums.Add(comp.FullName);
                                    break;
                                case SymbolTypeSpecifier.Class:
                                    classes.Add(comp.FullName);
                                    break;
                                case SymbolTypeSpecifier.Struct:
                                    structs.Add(comp.FullName);
                                    break;
                                case SymbolTypeSpecifier.Union:
                                    unions.Add(comp.FullName);
                                    break;
                            }
                        }
                    }
                }
                foreach (string unk in new HashSet<string>(unknowns))
                {
                    if (enums.Contains(unk) ||
                        structs.Contains(unk) ||
                        unions.Contains(unk) ||
                        classes.Contains(unk))
                    {
                        unknowns.Remove(unk);
                    }
                }
                Dictionary<string, HashSet<string>> collections = new Dictionary<string, HashSet<string>>()
                {
                    { "Enums", enums },
                    { "Structs", structs },
                    { "Unions", unions },
                    { "Classes", classes },
                    { "Unknowns", unknowns },
                };
                foreach (KeyValuePair<string, HashSet<string>> collection in collections)
                {
                    tw.WriteLine("================================");
                    tw.WriteLine(collection.Key);
                    tw.WriteLine("================================");
                    foreach (string str in collection.Value.OrderBy(x => x))
                    {
                        tw.WriteLine(str);
                    }
                    tw.WriteLine();
                }
            }
        }

        public static void GenerateTypeListFlat()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "TypeListFlat.txt")))
            {
                tw.WriteLine("List of all types (flat list)");
                tw.WriteLine();
                HashSet<string> names = new HashSet<string>();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo symbol in symbols)
                {
                    if (symbol.FuncComp != null)
                    {
                        List<SymbolComponentInfo> comps = new List<SymbolComponentInfo>(symbol.FuncCompArgList);
                        if (symbol.FuncCompReturn != null)
                        {
                            comps.Add(symbol.FuncCompReturn);
                        }
                        names.Add(symbol.FuncComp.FullNameOuter);
                        foreach (SymbolComponentInfo comp in comps)
                        {
                            names.Add(comp.FullName);
                        }
                    }
                }
                foreach (string name in names.OrderBy(x => x))
                {
                    tw.WriteLine(name);
                }
            }
        }

        class ClassHierarchyHelper
        {
            public CMwClassInfo Class;
            public HashSet<ClassHierarchyHelper> Children = new HashSet<ClassHierarchyHelper>();
        }

        private static void WriteClassHierarchy(TextWriter tw, ClassHierarchyHelper classInfo, int depth)
        {
            tw.WriteLine(string.Empty.PadLeft(depth, ' ') + classInfo.Class.Name);
            foreach (ClassHierarchyHelper child in classInfo.Children.ToList().OrderBy(x => x.Class.Name))
            {
                WriteClassHierarchy(tw, child, depth + 1);
            }
        }

        public static void RuntimeGenerateClassHierarchy()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassHierarchy.txt")))
            {
                tw.WriteLine("Class hierarchy for all classes which inherit from CMwNod");
                tw.WriteLine();

                CMwClassInfo rootClass = default(CMwClassInfo);
                Dictionary<CMwClassInfo, ClassHierarchyHelper> classes = new Dictionary<CMwClassInfo, ClassHierarchyHelper>();

                CMwClassInfo classInfo = CMwClassInfo.FirstClass;
                while (classInfo.Address != IntPtr.Zero)
                {
                    ClassHierarchyHelper helper = new ClassHierarchyHelper();
                    helper.Class = classInfo;
                    classes[classInfo] = helper;
                    classInfo = classInfo.Next;
                }

                List<CMwClassInfo> classesWithoutParent = new List<CMwClassInfo>();
                foreach (ClassHierarchyHelper classHelper in classes.Values)
                {
                    if (classHelper.Class.Parent.Address == IntPtr.Zero)
                    {
                        classesWithoutParent.Add(classHelper.Class);
                    }
                    else
                    {
                        classes[classHelper.Class.Parent].Children.Add(classHelper);
                    }
                }

                if (classesWithoutParent.Count != 1)
                {
                    tw.WriteLine("[WARNING] Invalid number of classes without base class. Expected:1, found:" +
                        classesWithoutParent + ", values:" + string.Join(",", classesWithoutParent.Select(x => x.Id)));
                    tw.WriteLine();
                }
                else
                {
                    rootClass = classesWithoutParent[0];
                }

                if (rootClass.Address != IntPtr.Zero)
                {
                    foreach (ClassHierarchyHelper child in classes[rootClass].Children.ToList().OrderBy(x => x.Class.Name))
                    {
                        WriteClassHierarchy(tw, child, 0);
                    }
                }
            }
        }

        public static void RuntimeGenerateClassList()
        {
            List<CMwClassInfo> classes = new List<CMwClassInfo>();
            CMwClassInfo classInfo = CMwClassInfo.FirstClass;
            while (classInfo.Address != IntPtr.Zero)
            {
                classes.Add(classInfo);
                classInfo = classInfo.Next;
            }
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassList.txt")))
            {
                tw.WriteLine("Class list for all classes which inherit from CMwNod");
                tw.WriteLine();
                foreach (CMwClassInfo ci in classes.OrderBy(x => x.Name))
                {
                    tw.WriteLine(ci.Name);
                }
            }
        }

        public static void RuntimeGenerateClassMembers()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassMembers.txt")))
            {
                tw.WriteLine("Class list with members (including script callable functions)");
                tw.WriteLine();
                CMwClassInfo classInfo = CMwClassInfo.FirstClass;
                while (classInfo.Address != IntPtr.Zero)
                {
                    tw.WriteLine(classInfo.Name + " (HasNew:" + classInfo.HasNew + ")");// + " ClassaAddr:" + classInfo.Address.ToInt32().ToString("X8"));
                    SMwParamInfo paramInfo = classInfo.ParamInfos;
                    for (int i = 0; i < classInfo.ParamCount; i++)
                    {
                        string additionalInfo = string.Empty;
                        switch (paramInfo.Type)
                        {
                            case EMwParamType.Proc:
                                {
                                    var args = paramInfo.AsProc.Args;
                                    additionalInfo = " Args:" + paramInfo.AsProc.NumArgs + " params:" + string.Join(",", args);
                                }
                                break;
                            case EMwParamType.Action:
                                additionalInfo = " Unk1:" + paramInfo.AsAction.Unk1;
                                break;
                            case EMwParamType.Enum:
                                additionalInfo = " EnumName:" + paramInfo.AsEnum.CppName + " EnumEntries:" + string.Join(",", paramInfo.AsEnum.ValueNames);
                                break;
                            case EMwParamType.Class:
                                additionalInfo = " Class:" + paramInfo.AsClass.ClassInfo.Id + " unk1:" + paramInfo.AsClass.Unk1;
                                break;
                            case EMwParamType.Vec2:
                                additionalInfo = " v1:" + paramInfo.AsVec2.Var1Name + " v2:" + paramInfo.AsVec2.Var2Name;
                                break;
                            case EMwParamType.Vec3:
                                additionalInfo = " v1:" + paramInfo.AsVec3.Var1Name + " v2:" + paramInfo.AsVec3.Var2Name + " v3:" + paramInfo.AsVec3.Var3Name;
                                break;
                            case EMwParamType.Vec4:
                                additionalInfo = " v1:" + paramInfo.AsVec4.Var1Name + " v2:" + paramInfo.AsVec4.Var2Name + " v3:" + paramInfo.AsVec4.Var3Name + " v4:" + paramInfo.AsVec4.Var4Name;
                                break;
                        }
                        if (paramInfo.IsArray)
                        {
                            additionalInfo = " TypeName:" + paramInfo.AsArray.TypeName + " Class:" + (paramInfo.AsArray.ClassInfo.Address == IntPtr.Zero ? "(null)" : paramInfo.AsArray.ClassInfo.Id.ToString()) + " Unk1:" + paramInfo.AsArray.Unk1 + " Unk2:" + paramInfo.AsArray.Unk2;
                        }
                        else if (paramInfo.IsRange)
                        {
                            additionalInfo = " Unk1:" + paramInfo.AsRange.Unk1.ToString("X8") + " Unk2:" + paramInfo.AsRange.Unk2.ToString("X8");
                        }
                        tw.WriteLine(" Name:" + paramInfo.Name + " Type:" + paramInfo.Type + " Index:" + paramInfo.Index + " Offset:" + paramInfo.Offset + " Flag1:" + paramInfo.Flags1 + " Flag2:" + paramInfo.Flags2 + additionalInfo);
                        tw.Flush();
                        paramInfo = paramInfo.Next;
                    }
                    classInfo = classInfo.Next;
                }
            }
        }

        public static void GenerateVirtualParamGetList()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "VirtualParam_Get.txt")))
            {
                tw.WriteLine("List of all classes with VirtualParam_Get implementations (and lists the function addresses). RE required to reimplement these (without the additional overhead).");
                tw.WriteLine();
                List<SymbolInfo> foundSyms = new List<SymbolInfo>();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo sym in symbols)
                {
                    if (sym.FuncComp != null && sym.FuncComp.Name == "VirtualParam_Get")
                    {
                        foundSyms.Add(sym);
                    }
                }
                foreach (SymbolInfo sym in foundSyms.OrderBy(x => x.FuncComp.FullName))
                {
                    tw.WriteLine(sym.FuncComp.FullName + " - " + sym.Address.ToString("X8"));
                }
            }
        }

        public static void GenerateFuncListAllShort()
        {
            int numDuplicates = 0;
            Dictionary<string, SymbolInfo> funcNames = new Dictionary<string, SymbolInfo>();
            HashSet<string> duplicates = new HashSet<string>();
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "FuncAll_SHORT.txt")))
            {
                tw.WriteLine("List all functions (unique function names only)");
                tw.WriteLine();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo sym in symbols)
                {
                    if (sym.FuncComp != null)
                    {
                        string className = sym.FuncComp.FullNameOuter;
                        if (!funcNames.ContainsKey(sym.FuncComp.Name))
                        {
                            funcNames[sym.FuncComp.Name] = sym;
                        }
                        else
                        {
                            numDuplicates++;
                        }
                    }
                }
                tw.WriteLine("Duplicates: " + numDuplicates);
                tw.WriteLine();
                foreach (SymbolInfo sym in funcNames.Values.OrderBy(x => x.FuncComp.FullName))
                {
                    tw.WriteLine(sym.FuncComp.FullName + " - " + sym.Address.ToString("X8"));
                }
            }
        }

        public static void GenerateFuncListNonCMwNodAllUnscoped(bool shortName)
        {
            GenerateFuncListNonCMwNodAll(shortName, true);
        }

        public static void GenerateFuncListNonCMwNodAll(bool shortName, bool unscopedOnly = false)
        {
            int numDuplicates = 0;
            int nonVirtualDuplicates = 0;
            Dictionary<string, SymbolInfo> funcNames = new Dictionary<string, SymbolInfo>();
            HashSet<string> duplicates = new HashSet<string>();
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "FuncMisc" +
                (unscopedOnly ? "Unscoped" : "All") +
                (shortName ? "_SHORT" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List all functions excluding CMwNod types (unique function names only)");
                tw.WriteLine();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo sym in symbols)
                {
                    if (sym.FuncComp != null && (!unscopedOnly || !sym.Name.Contains("::")))
                    {
                        string className = sym.FuncComp.FullNameOuter;
                        EMwClassId classId;
                        if (!Enum.TryParse(className, out classId))
                        {
                            if (!funcNames.ContainsKey(sym.FuncComp.Name))
                            {
                                funcNames[sym.FuncComp.Name] = sym;
                            }
                            else
                            {
                                numDuplicates++;
                                if (sym.Modifier != SymbolModifier.Virtual)
                                {
                                    nonVirtualDuplicates++;
                                    duplicates.Add(sym.FuncComp.Name);
                                }
                            }
                        }
                    }
                }
                tw.WriteLine("Duplicates: " + numDuplicates + " nonVirtualDuplicates: " + nonVirtualDuplicates);
                tw.WriteLine();
                foreach (SymbolInfo sym in funcNames.Values.OrderBy(x => x.FuncComp.FullName))
                {
                    tw.WriteLine((shortName ? sym.FuncComp.FullName : sym.Name) + " - " + sym.Address.ToString("X8"));
                }
            }
            using (TextWriter tw = File.CreateText("FuncMiscAll_NonVirtualDuplicates.txt"))
            {
                foreach (string str in duplicates)
                {
                    tw.WriteLine(str);
                }
            }
        }

        public static void GenerateFuncListNonCMwNodVirtual(bool shortName)
        {
            int numDuplicates = 0;
            Dictionary<string, SymbolInfo> funcNames = new Dictionary<string, SymbolInfo>();
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "FuncMiscVirtual" + (shortName ? "_SHORT" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List all virtual functions excluding CMwNod types");
                tw.WriteLine();
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo sym in symbols)
                {
                    if (sym.FuncComp != null && sym.Modifier == SymbolModifier.Virtual && !sym.FuncComp.IsDestructor)
                    {
                        string className = sym.FuncComp.FullNameOuter;
                        EMwClassId classId;
                        if (!Enum.TryParse(className, out classId))
                        {
                            if (!funcNames.ContainsKey(sym.FuncComp.Name))
                            {
                                funcNames[sym.FuncComp.Name] = sym;
                            }
                            else
                            {
                                numDuplicates++;
                            }
                        }
                    }
                }
                tw.WriteLine("Duplicates: " + numDuplicates);
                tw.WriteLine();
                foreach (SymbolInfo sym in funcNames.Values.OrderBy(x => x.FuncComp.FullName))
                {
                    tw.WriteLine((shortName ? sym.FuncComp.FullName : sym.Name) + " - " + sym.Address.ToString("X8"));
                }
            }
        }

        public static void RuntimeGenerateClassFuncListAll(bool shortName)
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassFuncList" + (shortName ? "_SHORT" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List of all functions for CMwNod types");
                tw.WriteLine();
                RuntimeGenerateListFuncs(null, tw, IntPtr.Zero, null, false, false, shortName);
            }
        }

        public static void RuntimeGenerateClassFuncList(bool shortName)
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassFuncListNonVirtual" + (shortName ? "_SHORT" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List of functions (excluding vtables) for CMwNod types");
                tw.WriteLine();
                RuntimeGenerateListFuncs(null, tw, IntPtr.Zero, null, false, true, shortName);
            }
        }

        public static void RuntimeGenerateClassFuncListVTables(bool shortName)
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "ClassFuncListVirtual" + (shortName ? "_SHORT" : string.Empty) + ".txt")))
            {
                tw.WriteLine("List of virtual functions for CMwNod types");
                tw.WriteLine();
                RuntimeGenerateListFuncs(null, tw, IntPtr.Zero, null, true, false, shortName);
            }
        }

        private static void RuntimeGenerateListFuncs(Dictionary<EMwClassId, List<SymbolInfo>> symbolsByClass, TextWriter tw, CMwClassInfo classInfo, HashSet<string> parentFuncs, bool virtualOnly, bool noVirtuals, bool shortName)
        {
            if (classInfo.Address == IntPtr.Zero)
            {
                // Creating this dictionary is important, as itterating 100-200k entries for each class is VERY slow
                symbolsByClass = new Dictionary<EMwClassId, List<SymbolInfo>>();
                List<SymbolInfo> allSymbols = LoadSymbols();
                foreach (SymbolInfo sym in allSymbols)
                {
                    EMwClassId classId;
                    if ((virtualOnly && sym.Modifier != SymbolModifier.Virtual) ||
                        (noVirtuals && sym.Modifier == SymbolModifier.Virtual) ||
                        sym.FuncComp == null || sym.FuncComp.IsDestructor ||
                        !Enum.TryParse(sym.FuncComp.FullNameOuter, out classId))
                    {
                        continue;
                    }
                    List<SymbolInfo> syms;
                    if (!symbolsByClass.TryGetValue(classId, out syms))
                    {
                        symbolsByClass[classId] = syms = new List<SymbolInfo>();
                    }
                    syms.Add(sym);
                }
                RuntimeGenerateListFuncs(symbolsByClass, tw, CMwNod.StaticGetClassInfo(EMwClassId.CMwNod), new HashSet<string>(), virtualOnly, noVirtuals, shortName);
                return;
            }

            bool hasNewFuncs = false;
            HashSet<string> funcNames = new HashSet<string>(parentFuncs);
            List<SymbolInfo> symbols;
            if (symbolsByClass.TryGetValue(classInfo.Id, out symbols))
            {
                foreach (SymbolInfo sym in symbols)
                {
                    if (!parentFuncs.Contains(sym.FuncComp.Name))
                    {
                        funcNames.Add(sym.FuncComp.Name);
                        if (!hasNewFuncs)
                        {
                            hasNewFuncs = true;
                            tw.WriteLine("[" + classInfo.Id + "]" +
                                (classInfo.Parent.Address == IntPtr.Zero ? string.Empty :
                                " - " + classInfo.Parent.Id));
                        }
                        tw.WriteLine((shortName ? sym.FuncComp.FullName : sym.Name) + " - " + sym.Address.ToString("X8"));
                    }
                }
            }
            if (hasNewFuncs)
            {
                tw.WriteLine();
            }
            for (int i = 0; i < classInfo.Children.Count; i++)
            {
                CMwClassInfo childClass = classInfo.Children[i];
                RuntimeGenerateListFuncs(symbolsByClass, tw, childClass, funcNames, virtualOnly, noVirtuals, shortName);
            }
        }

        public static void RuntimeGenerateVTableAddressCode()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "VTablesAddressCodeGen.cs")))
            {
                tw.WriteLine("// These are just the vtable addresses for all CMwNod classes in the game. Required to access the vtable of a given class from C#.");
                tw.WriteLine("// There are currently a few missing addresses. For now manually create these with a value of 0, and fix when possible.");
                tw.WriteLine();
                Dictionary<EMwClassId, uint> vtables = new Dictionary<EMwClassId, uint>();
                foreach (EMwClassId cid in Enum.GetValues(typeof(EMwClassId)))
                {
                    if (cid != 0)
                    {
                        vtables[cid] = 0;
                    }
                }
                List<SymbolInfo> symbols = LoadSymbols();
                foreach (SymbolInfo symbol in symbols)
                {
                    if (symbol.Type == SymbolType.VTable)
                    {
                        EMwClassId classId;
                        if (Enum.TryParse<EMwClassId>(symbol.CompType.FullName, out classId))
                        {
                            vtables[classId] = symbol.Address;
                        }
                    }
                }
                foreach (KeyValuePair<EMwClassId, uint> entry in vtables.OrderBy(x => x.Key.ToString()))
                {
                    EMwClassId classId = entry.Key;
                    CMwClassInfo classInfo = CMwNod.StaticGetClassInfo(classId);
                    if (classInfo.Parent.Address != IntPtr.Zero)
                    {
                        tw.WriteLine("public class " + classId + " : VT." + classInfo.Parent.Id +
                            " { public static class Offsets { public static IntPtr VTable = (IntPtr)0x" +
                            entry.Value.ToString("X8") + "; } }");
                    }
                }
            }
        }

        public static void GenerateVTableAddressList()
        {
            using (TextWriter tw = File.CreateText(Path.Combine(DocsDir, "VTables.txt")))
            {
                tw.WriteLine("List of classes with vtable addresses");
                tw.WriteLine();
                List<SymbolInfo> symbols = LoadSymbols();
                HashSet<EMwClassId> classesWithVtables = new HashSet<EMwClassId>();
                foreach (SymbolInfo symbol in symbols)
                {
                    if (symbol.Type == SymbolType.VTable)
                    {
                        EMwClassId classId;
                        if (Enum.TryParse<EMwClassId>(symbol.CompType.FullName, out classId))
                        {
                            classesWithVtables.Add(classId);
                        }
                    }
                }
                tw.WriteLine("Missing CMwNod vtables (these MUST be fixed to access their virtuals from C#):");
                tw.WriteLine();
                foreach (EMwClassId classId in Enum.GetValues(typeof(EMwClassId)))
                {
                    if (!classesWithVtables.Contains(classId))
                    {
                        tw.WriteLine(classId);
                    }
                }
                tw.WriteLine();
                tw.WriteLine("Full list:");
                tw.WriteLine();
                foreach (SymbolInfo symbol in symbols)
                {
                    if (symbol.Type == SymbolType.VTable)
                    {
                        tw.WriteLine(symbol.Address.ToString("X8") + " " + symbol.CompType.FullName +
                            (symbol.CompType.IsGenericType ? " (generic)" : string.Empty));
                    }
                }
            }
        }

        public static string Demangle(string str)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            StringBuilder nativeDemangled = new StringBuilder(1024);
            UnDecorateSymbolName(str, nativeDemangled, nativeDemangled.Capacity, UnDecorateFlags.UNDNAME_COMPLETE);
            result = nativeDemangled.ToString();
            return result;
        }

        [Flags]
        enum UnDecorateFlags
        {
            UNDNAME_COMPLETE = (0x0000),  // Enable full undecoration
            UNDNAME_NO_LEADING_UNDERSCORES = (0x0001),  // Remove leading underscores from MS extended keywords
            UNDNAME_NO_MS_KEYWORDS = (0x0002),  // Disable expansion of MS extended keywords
            UNDNAME_NO_FUNCTION_RETURNS = (0x0004),  // Disable expansion of return type for primary declaration
            UNDNAME_NO_ALLOCATION_MODEL = (0x0008),  // Disable expansion of the declaration model
            UNDNAME_NO_ALLOCATION_LANGUAGE = (0x0010),  // Disable expansion of the declaration language specifier
            UNDNAME_NO_MS_THISTYPE = (0x0020),  // NYI Disable expansion of MS keywords on the 'this' type for primary declaration
            UNDNAME_NO_CV_THISTYPE = (0x0040),  // NYI Disable expansion of CV modifiers on the 'this' type for primary declaration
            UNDNAME_NO_THISTYPE = (0x0060),  // Disable all modifiers on the 'this' type
            UNDNAME_NO_ACCESS_SPECIFIERS = (0x0080),  // Disable expansion of access specifiers for members
            UNDNAME_NO_THROW_SIGNATURES = (0x0100),  // Disable expansion of 'throw-signatures' for functions and pointers to functions
            UNDNAME_NO_MEMBER_TYPE = (0x0200),  // Disable expansion of 'static' or 'virtual'ness of members
            UNDNAME_NO_RETURN_UDT_MODEL = (0x0400),  // Disable expansion of MS model for UDT returns
            UNDNAME_32_BIT_DECODE = (0x0800),  // Undecorate 32-bit decorated names
            UNDNAME_NAME_ONLY = (0x1000),  // Crack only the name for primary declaration;
            // return just [scope::]name.  Does expand template params
            UNDNAME_NO_ARGUMENTS = (0x2000),  // Don't undecorate arguments to function
            UNDNAME_NO_SPECIAL_SYMS = (0x4000),  // Don't undecorate special names (v-table, vcall, vector xxx, metatype, etc)
        }

        [DllImport("dbghelp.dll", SetLastError = true, PreserveSig = true)]
        static extern int UnDecorateSymbolName(
            [In] [MarshalAs(UnmanagedType.LPStr)] string DecoratedName,
            [Out] StringBuilder UnDecoratedName,
            [In] [MarshalAs(UnmanagedType.U4)] int UndecoratedLength,
            [In] [MarshalAs(UnmanagedType.U4)] UnDecorateFlags Flags);
    }
}
