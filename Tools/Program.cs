using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {

            WATERMARK();
            string[] A_mainMenuOptions = new string[]
            { "Logging", "N_Pst hunt", "N_DriveMap", "B_Powershell", "Python scripts", "T_Networking Tools", "Silent mode" }; // B_ = broken, N_ = not yet complete, T_ To be implemented
            Menu M_Main = new Menu(A_mainMenuOptions);
            Worker workerObject = new Worker();
            Thread WorkerThread = new Thread(() => workerObject.Logging());
            ConsoleKeyInfo vki;
            bool run = true;
            string input = string.Empty;



            //Hidden cred = new Hidden(); // If using advanced credentials is needed

            do
            {
                // Print all options from an array 
                // Run the corresponding command
                Menu.PrintMenu(M_Main);
                Console.WriteLine("Please select an option... ");
                vki = Console.ReadKey(true);
                switch (vki.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        if (!WorkerThread.IsAlive) { WorkerThread.Start(); Console.WriteLine("Log started at C:\\Health.txt"); }
                        else { Console.WriteLine("Thread is already logging"); }; continue;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Functionality.PST_OST_Location(); continue;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("Please enter the net use file path containing the drives");
                        input = Console.ReadLine(); Functionality.DRIVE_MAPPING(input); continue;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Py.PySwitchMenu(); continue;

                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                         continue;
                    case ConsoleKey.Escape:
                        run = false; break;
                    default:
                        Console.WriteLine("Invalid entry... "); continue;
                }
            } while (run);
            if (WorkerThread.IsAlive || WorkerThread.ThreadState == System.Threading.ThreadState.Running)
                workerObject.RequestStop();
        }
        static void WATERMARK()
        {
            string bank = ("National Bank IT TOOL");
            string name = ("By Mathieu Robitaille");
            Console.CursorLeft = (Console.WindowWidth / 2) - (bank.Length / 2);
            Console.WriteLine(bank);
            Console.CursorLeft = (Console.WindowWidth / 2) - (name.Length / 2);
            Console.WriteLine(name);
            Console.CursorLeft = (Console.WindowWidth/2) - (AppDomain.CurrentDomain.BaseDirectory.Length/2);
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}