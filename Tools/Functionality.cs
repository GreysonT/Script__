using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Tools
{
    class Functionality
    {
        public static void PST_OST_Location()
        {
            bool backup = false;                                            //-------------
            string basePath;                                                //
            string username;                                                //
            ConsoleKeyInfo cki;                                             // Setting vars
            List<int> remove = new List<int>();                             //
            //List<string> ext = new List<string> { ".pst", ".ost" };    -- // Obsolete code
            List<string> ext = ListBuilder(4);
            string[] drives = Directory.GetLogicalDrives();                 //
                                                                            //-------------
            Console.WriteLine();
            Console.WriteLine("Would you like to back up the files? : Y/N ");
            cki = Console.ReadKey();
            if (cki.Key == ConsoleKey.Y)                                    //Enabling backup
                backup = true;                                              //
            Console.WriteLine();
            for (int i = 0; i < drives.Length; i++)                         //-------------
            {                                                               //
                drives[i] = drives[i].EndsWith(@"\") ?                      //Formatting drive paths
                    drives[i].Substring(0, drives[i].Length - 1)            //And printing drives
                        : drives[i];                                        //
                Console.WriteLine((i + 1 + " - ") + drives[i]);             //
            }                                                               //-------------
            Console.WriteLine("Select drives you want to" 
                + " exclude from the search." 
                + "\nPress enter to end entry.");
            for (int i = 0; i < drives.Length; i++)                         //-------------
            {                                                               //
                cki = Console.ReadKey();
                Console.WriteLine();
                int value;                                                  //
                if (int.TryParse(cki.KeyChar.ToString(), out value)         //Input handling
                    && value < drives.Length + 1)                           //For drive selection
                    remove.Add(value);                                      //
                if (cki.Key == ConsoleKey.Enter)                            //
                    break;                                                  //
                remove.TrimExcess();                                        //Sets cap to real number of elements
                if (remove.Count == drives.Length)                          // so that the loop will break if it exceeds the drives
                    break;                                                  //
            }                                                               //-------------
            List<string> files = new List<string>();
            foreach (int item in remove)                                    //-------------
            {                                                               //Removing the drives
                drives[item - 1] = null;                                    // so they dont get searched
            }                                                               //-------------
            files.TrimExcess();
            username = (Environment.UserName == ("s0001")) ? 
                Console.ReadLine() : Environment.UserName;
            try
            {
                foreach (var item in drives)                                //-------------
                {                                                           //
                    if (item != null)                                       //
                    {                                                       //
                        basePath = item + @"\Users\" + username;            // - constructing the base path to be the users directory
                        files = CheckFiles(basePath, ext);                  // - Heavy searching of all directories
                    }                                                       //
                }                                                           //-------------

                write_loc(files, "OUTLOOK_ORIGINALS", "OUTLOOK_END");
                if (backup)
                {
                    string loc = "C:\\Backup\\OutlookFiles";
                    List<string> newFiles = BACKUP_FILES(loc, files);
                    write_loc(newFiles, "BACKUP_LOCATIONS", "BACKUP_END");
                    // Change profile
                
                    if (RESTORE_FILES())
                    {
                        Console.WriteLine();
                        Console.WriteLine("Restoration of files complete!");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("The operation failed...");
                        Console.WriteLine("The log is located at : {0}", 
                            (Directory.GetDirectoryRoot                  
                                (AppDomain.CurrentDomain.BaseDirectory)  
                                    + "LOG.txt")) ;
                        Console.WriteLine();
                        
                    }
                }
            }

            catch(Exception w)
            {
                Console.WriteLine(w.Message);
            }
        }
        static List<string> BACKUP_FILES(string path, List<string> files)
        {
            List<string> result = new List<string>();
            if (!Directory.Exists(@"C:\Backup"))
            {
                Directory.CreateDirectory(@"C:\Backup");
                Directory.CreateDirectory(@"C:\Backup\Outlook");
                Directory.CreateDirectory(@"C:\Backup\Perso");
            }
            foreach (var item in files)
            {
                //async
                string newFile = item.Substring
                    (item.LastIndexOf("\\") + 1);
                newFile = path + "\\" + newFile;
                result.Add(newFile);                                
                Console.WriteLine(newFile);
                //File.Copy(item, path + newFile); // enable later
            }
            return result;
        }

        static bool RESTORE_FILES()
        {

            List<string> original_Locations = new List<string>();
            string outlook_start = "OUTLOOK_ORIGINALS";
            string outlook_end = "OUTLOOK_END";

            List<string> backup_destinations = new List<string>();
            string backup_start = "BACKUP_LOCATIONS";
            string backup_end = "BACKUP_END";

            try
            {
                original_Locations = File_reader(outlook_start, outlook_end);
                backup_destinations = File_reader(backup_start, backup_end);
                // After this the lists contain the original locations and 
                // the locations the files are in now
                //
                //
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
                

            return true;
        }

        static List<string> File_reader(string startpoint, string endpoint)
        {
            string handle;
            List<string> result = new List<string>();
            try
            {
                using (StreamReader StR = new StreamReader
                    (Directory.GetDirectoryRoot
                        (AppDomain.CurrentDomain.BaseDirectory)
                            + "LOG.txt"))
                {
                    do // Grab all backups
                    {
                        handle = StR.ReadLine();
                        if (handle == startpoint)
                        {
                            string input;
                            while ((input = StR.ReadLine()) != endpoint)
                            {
                                result.Add(input);
                            }
                            handle = input;
                        }
                    } while (handle != endpoint || StR.EndOfStream == false); // End loop at end of locations
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }



            return result;
        }

        static void write_loc(List<string> files, string start, string end)
        {
            using (TextWriter sw = new StreamWriter
                (Directory.GetDirectoryRoot
                    (AppDomain.CurrentDomain.BaseDirectory)
                        + "LOG.txt"))
            {
                sw.WriteLine(start);
                foreach (var item in files)
                { sw.WriteLine(item); }
                sw.WriteLine(end);
            }
        }

        static List<string> CheckFiles(string folder, List<string> extentions)
        {
            List<string> result = new List<string>();
            foreach (string file in Directory.GetFiles(folder))
                foreach (string ext in extentions)
                    if (file.Contains(ext))
                        result.Add(file);

            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                { result.AddRange(CheckFiles(subDir, extentions)); }

                catch (Exception e) when (!e.Message.StartsWith("Access to the path"))
                { Console.WriteLine(e.Message); }
            }
            return result;
        }

        static List<string> ListBuilder(int max)
        {
            List<string> result = new List<string>();
            string input;
            string ext = "";
            if (max < 5)
            {
                Console.WriteLine("Please enter the extentions you would like to search for");
                Console.WriteLine("Example : pst, txt, pdf, ps1");
                Console.WriteLine("Enter a blank line when your are done.");
                ext = ".";
            }
            do
            {

                input = Console.ReadLine();
                if (input == string.Empty)
                    break;
                if (input?.Length <= max)
                    result.Add(ext + input);
                else
                    Console.WriteLine("that extention was over {0} characters", max);
            } while (true);


            return result;
        }

        public static void TREE_SIZE()
        {
            // incorporate a treesize
        }
        public static void DRIVE_MAPPING(string driveFileLoc) // in format of C:\Subdir\Subdir
        {
            string line = string.Empty;
            string driveLetter = string.Empty;
            string drivePath = string.Empty;
            string command = string.Empty;
            int i = 0;
            List<string> junk = new List<string>() { "Microsoft Windows Network", "Non disponib", "OK" };
            List<Thread> threads = new List<Thread>(); // Start a thread for each map?
            CommandPrompt cmd = new CommandPrompt();
            using (StreamReader sr = new StreamReader(driveFileLoc))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    foreach (string item in junk)
                    {
                        line = line?.Replace(item, "");
                    }
                    line = line?.TrimStart();
                    if (line == string.Empty || line.StartsWith("-----"))
                        continue;
                    driveLetter = line.Substring(0, 2);
                    drivePath = line = line.Remove(0, 3).TrimStart();
                    command = string.Format("net use {driveLetter} {drivePath} /user:{Hidden.u} {Hidden.p}");
                    Thread test = new Thread(() => cmd.DriveMap(command));
                    test.Name = ("T" + i);
                    //test.Start();

                }

            }
        }

    }
}
