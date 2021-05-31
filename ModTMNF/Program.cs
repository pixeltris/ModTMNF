using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using ModTMNF.Game;
using System.Diagnostics;

namespace ModTMNF
{
    public class Program
    {
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
                        break;
                }
            }

            Console.WriteLine("Launching Trackmania...");

            // If you want to hook earlier than "Launch" allows you to then use "LaunchAlt" (which can hook as early as CRT startup / WinMain).
            string error;
            if (!TrackmaniaLauncher.LaunchAlt(out error))
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
            NativeDll.WL_InitHooks();

            try
            {
                FT.Init();
                VT.Init();
                Mods.ModManager.Add(new Mods.TestMod());
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
