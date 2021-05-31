using System.Runtime.InteropServices;

namespace Bea
{
    public class BeaEngine
    {
        [DllImport("ModTMNF/BeaEngine.dll")]
        public static extern unsafe int Disasm(DisasmUnsafe *disasm);

        [DllImport("ModTMNF/BeaEngine.dll")]
        public static extern int Disasm([In, Out, MarshalAs(UnmanagedType.LPStruct)] Disasm disasm);

        [DllImport("ModTMNF/BeaEngine.dll")]
        private static extern string BeaEngineVersion();

        [DllImport("ModTMNF/BeaEngine.dll")]
        private static extern string BeaEngineRevision();

        public static string Version
        {
            get
            {
                return BeaEngineVersion();
            }
        }

        public static string Revision
        {
            get
            {
                return BeaEngineRevision();
            }
        }
    }
}
