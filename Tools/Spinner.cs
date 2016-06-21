using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    //Spinner spinnerObject = new Spinner();
    //Thread spinnerThread = new Thread(spinnerObject.Turn);
    //spinnerThread.Start();
    //spinnerObject.RequestStop();
    // ?? spinnerThread.dispose && spinnerObject.dispose ??
    public class Spinner
    {
        int counter;
        bool running;
        public Spinner()
        {
            counter = 0;
            running = true;
        }
        public void Turn()
        {
            while (running)
            {
                counter++;
                switch (counter % 4)
                {
                    case 0: Console.Write("/"); break;
                    case 1: Console.Write("-"); break;
                    case 2: Console.Write(@"\"); break;
                    case 3: Console.Write("|"); break;
                }
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            }
        }
        public void RequestStop()
        {
            running = false;
        }
    }
}
