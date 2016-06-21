using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    class Menu
    {
        public List<string> options = new List<string>();
        public Menu(string[] constructorStrings)
        {
            foreach (var item in constructorStrings)
            {
                options.Add(item);
            }
        }
        public static void PrintMenu(Menu menu)
        {
            int i = 0;
            foreach (var item in menu.options)
            {
                i++;
                Console.WriteLine(i +". " + item);
            }
        }

    }
}
