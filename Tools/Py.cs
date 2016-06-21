using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Tools
{
    static class Py
    {
        static readonly string currentRoot = getWorkingDir();
        static readonly string pythonEXE = currentRoot + "Py\\PoPy\\App\\python.exe";
        static readonly string[] APyMenuOptions = new string[] { "PortScanner", "TcpServer", "Example2" };
        public static void PySwitchMenu()
        {
            // Same idea as the main program menu
            ConsoleKeyInfo cki;
            bool alive = true;
            Menu pyMenu = new Menu(APyMenuOptions);
            while (alive)
            {
                Menu.PrintMenu(pyMenu);
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        pyCommand("portscan"); continue;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        pyCommand("tcpserver"); continue;

                    case ConsoleKey.Escape:
                        goto default;
                    default:
                        alive = false; break;
                }
            }
        }

        static void pyCommand(string command)
        {
            // Set up process info
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = pythonEXE;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            // Find the working dir (The usb stick)
            string cmd = currentRoot + "Scripts\\Python\\";
            string args = string.Empty;

            // Arg checks
            bool ipAddress = false;
            bool port = false;

            // Available scripts
            string[] scripts = new string[]
                {"PortScanner.py" , "TcpServer.py"};
            switch (command)
            {
                case "portscan":
                    cmd += scripts[0]; ipAddress = port = true; break;
                case "tcpserver":
                    cmd += scripts[1]; port = true; break;                
                default:
                    return;
            }
            // Arg checks and inputs
            if (ipAddress)
            {
                Console.WriteLine("please enter the IP");
                args += " -a " + Console.ReadLine();
            }
            if (port)
            {
                Console.WriteLine("Please enter the ports, separated by commas");
                args += " -p " + Console.ReadLine();
            }
            
            // Finish process info
            info.Arguments = string.Format("{0} {1}", cmd, args);
            
            // Setting up proc
            using (Process proc = Process.Start(info))
            {
                do
                {
                    // Collect all output of the script and write it to the console
                    Thread.Sleep(1);
                    Console.Out.Write(proc?.StandardOutput.ReadToEnd());
                } while (!proc.HasExited);

                // Catch any leftovers in redirected stdout
                Console.Out.Write(proc.StandardOutput.ReadToEnd());
            }
        }

        #region oldscan
        static void portScan()
        {
            string cmd = currentRoot + "Scripts\\Python\\PortScanner.py";
            Console.WriteLine("please enter the IP");
            string args = " -a " + Console.ReadLine();
            Console.WriteLine("Please enter the ports, separated by commas");
            args += " -p " + Console.ReadLine();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = pythonEXE;
            info.Arguments = string.Format("{0} {1}", cmd, args);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            using (Process proc = Process.Start(info))
            {
                using (StreamReader sr = proc.StandardOutput)
                {
                    string result = sr.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
        }
        #endregion oldscan

        public static string getWorkingDir()
        {
            // This function will return the current working base directory
            // it should be a USB stick
            // The base directory needs to contain confirm.tools for this to work
            string label = File.Exists(AppDomain.CurrentDomain.BaseDirectory + "confirm.tools") 
                ? AppDomain.CurrentDomain.BaseDirectory : string.Empty ;
            if (label == string.Empty)
            {
                var drives = DriveInfo.GetDrives();
                foreach (var item in drives)
                {
                    if (item.IsReady)
                        if (File.Exists(item.Name + "confirm.tools"))
                            label = item.Name;
                }
            }
            return label;
        }
    }
}
