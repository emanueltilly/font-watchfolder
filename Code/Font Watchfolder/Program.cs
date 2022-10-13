using Font_Watchfolder;
using System;
using System.Globalization;
using System.Timers;

namespace Font_Watchfolder
{

    static class Program
    {
        static string[] arguments = Environment.GetCommandLineArgs();
        static bool scanEnabled = false;
        static string userSourceFolderPath = null;
        static System.Timers.Timer t = new System.Timers.Timer();

        static void Main(string[] args)
        {
            

            Console.WriteLine(""); Console.WriteLine("");
            Console.WriteLine("Font Watchfolder \nVersion 0.1");
            Console.WriteLine("");
            Console.WriteLine("https://github.com/emanueltilly/font-watchfolder");
            Console.WriteLine("This application is ");
            Console.WriteLine(""); Console.WriteLine("");

            //Check if running as administrator
            if (!Administrator.IsAdministrator())
            {
                Console.WriteLine("Application do not have correct privileges.\nPlease restart as administrator.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            //Check arguments
            if (arguments.Length >= 3)
            { 
                if (arguments[1] != "--watchfolder") { stopIncorrectArguments("Could not find --watchfolder flag."); }
                try {
                    if (arguments[2].EndsWith('"')) {
                        arguments[2] = arguments[2].Remove(arguments[2].Length - 1);
                    }
                    Directory.GetFiles(arguments[2]);
                } catch {
                    stopIncorrectArguments("There is a problem reading files in watchfolder: " + arguments[2]);
                }

            } else
            {
                stopIncorrectArguments("");
            }
            //Set watchfolder to argument path
            userSourceFolderPath = arguments[2];
            //Trim last slash from watchfolder argument if present
            if (userSourceFolderPath.EndsWith("\\")) { userSourceFolderPath = userSourceFolderPath.Remove(userSourceFolderPath.Length - 1); }


            //Setup timer
            t.Interval = 60000; // In milliseconds
            t.AutoReset = true; // Stops it from repeating
            t.Elapsed += new ElapsedEventHandler(TimerElapsed);


            Console.WriteLine("Watchfolder: " + userSourceFolderPath + "\n");


            Console.WriteLine("Performing initial scan...");
            RunScan();



            //Start Watchfolder Service, and start timer for scanning

            Console.WriteLine("\n\nWatchfolder running...\n\n");
            t.Start();

            using var watcher = new FileSystemWatcher(userSourceFolderPath);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += TriggerScan;
            watcher.Created += TriggerScan;
            watcher.Deleted += TriggerScan;
            watcher.Renamed += TriggerScan;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            //Make sure user do not accidentally exit
            while (true)
            {
                Console.ReadLine();
            }
        }




        static void TriggerScan(object sender, FileSystemEventArgs e)
        {
            if (scanEnabled == false)
            {
                //Restart timer
                t.Stop();
                t.Start();
                scanEnabled = true;
                Console.WriteLine(DateTime.Now.ToString() + " - Watchfolder change detetcted.");
            }

        }

        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (scanEnabled) { RunScan(); }
        }

        static void RunScan()
        {
            t.Stop();

            List<fontfile> missingFont = FontHandling.Compare(userSourceFolderPath);

            if (missingFont == null) { Console.WriteLine(DateTime.Now.ToString() + " - No missing fonts"); }
            else
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Scan found " + missingFont.Count.ToString() + " missing fonts");

                //Install missing fonts
                foreach (fontfile font in missingFont)
                {
                    FontHandling.RegisterFont(font.filepath);
                }
            }

            t.Start();
            scanEnabled = false;
        }

        static void stopIncorrectArguments(string explain)
        {
            string example = "--watchfolder \"\\\\192.168.1.20\\Shared Folder\\Fonts\"";
            Console.WriteLine("\n\nERROR!\nFont Watchfolder was started with incorrect arguments!");
            Console.WriteLine("Arguments should be formatted like in the following example.\nMapped network drives are not supported. Use full networkpath like in the example:\n");
            Console.WriteLine("\n" + example);
            Console.WriteLine("\n" + explain);
            Console.ReadKey();
            Environment.Exit(160);
        }
    }
}


