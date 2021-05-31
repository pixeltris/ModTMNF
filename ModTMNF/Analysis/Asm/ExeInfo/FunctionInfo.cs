using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Analysis.Asm
{
    class FunctionInfo
    {
        public SymbolsHelper.SymbolInfo Symbol { get; set; }
        public uint Address
        {
            get { return Symbol.Address; }
        }
        public uint Length { get; set; }
        public Dictionary<uint, FunctionInfo> References { get; private set; }// All functions which reference this function


        public FunctionInfo(SymbolsHelper.SymbolInfo symbol, uint length)
        {
            References = new Dictionary<uint, FunctionInfo>();
            Symbol = symbol;
            Length = length;
        }

        public void AddReference(uint reference, FunctionInfo functionInfo)
        {
            if (References.ContainsKey(reference))
                References[reference] = functionInfo;
            else
                References.Add(reference, functionInfo);
        }

        public uint GetFirstReferenceAddress()
        {
            if (References != null && References.Count > 0)
                return References.ElementAt(0).Key;
            return 0;
        }

        public byte[] ReadBytes(UnmanagedBuffer buffer)
        {
            return buffer.Read(Address, Length);
        }
    }
}
