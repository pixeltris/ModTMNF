using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using ModTMNF.Game;

namespace ModTMNF
{
    public class Program
    {
        public const string BaseDir = "ModTMNF";

        [STAThread]
        unsafe static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "../");
   
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "gendocs":
                        // Generates all docs based on the .map file
                        Analysis.SymbolsHelper.GenerateDocs();
                        return;
                    case "cs":
                        // Fixes up copied from Visual Studio "Call Stack" window (vs has better results than StackWalk64 - TODO: use better a stack walker)
                        string str = Analysis.SymbolsHelper.FixVsCallstack(File.ReadAllText(Path.Combine(BaseDir, "callstack.txt")));
                        Console.WriteLine(str);
                        System.Windows.Forms.Clipboard.SetText(str);
                        return;
                }
            }

            Console.WriteLine("Launching Trackmania...");

            string error;
            if (!TrackmaniaLauncher.Launch(out error))
            {
                Console.WriteLine(error);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Entry point invoked by the .NET loader (ModTMNF-ManagedLoader.cpp)
        /// </summary>
        public static int DllMain(string arg)
        {
            try
            {
                FT.Init();
                VT.Init();
                Mods.ModManager.Add(new Mods.ModCore());
                Mods.ModManager.Add(new Mods.ModStackTraceFinder());
                Mods.ModManager.Add(new Mods.ModTest());
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }

            return 0;
        }

        public static void DebugBreak()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            Debugger.Break();
        }

        /// <summary>
        /// // TODO: Move logging somewhere else
        /// </summary>
        public static void Log(string str)
        {
            try
            {
                lock (logLocker)
                {
                    if (!logCheckedDir)
                    {
                        logCheckedDir = true;
                        Directory.CreateDirectory(LogsDir);
                    }
                    File.AppendAllText(LogFile, "[" + DateTime.Now + "] " + str + Environment.NewLine);
                }
            }
            catch
            {
            }
        }

        static bool logCheckedDir;
        static object logLocker = new object();
        static readonly string LogsDir = Path.Combine("ModTMNF", "Logs");
        public static readonly string LogFile = Path.Combine(LogsDir, "Log-" + (uint)Environment.TickCount + ".txt");
    }
}
