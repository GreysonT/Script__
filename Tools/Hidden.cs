using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    class Hidden
    {
        public static void Logon()
        {
            // add logon function
            Console.WriteLine("Please enter username");
            u = Console.ReadLine();
            Console.WriteLine("Please enter your password");
            while (key.Key != ConsoleKey.Enter)
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
        }
        static ConsoleKeyInfo key;
        public static string u { get; private set; }
        public static string pass { get; private set; }
    }
}
