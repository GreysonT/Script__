using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    class QueuePrinter
    {
        //Add a thread-safe queue that will print out 
        // its contents at a set interval
        //  
        //  Something like, queue.add(string)
        //  then have a thread lock everything and print the screen
        //  30x per second?
        //
        // TextManager.Add(string Text)
        // TextManager.Add(String Text,




        /* Concepts
          
          Thread manager?
          - entry point of program which will start threads on a switch-case
          - will allow console printing to be managed by one main thread

          [Working...] - > [OK]
          - Much simpler
          - Wont need to implement a thread safe anything
          - console.readline will still work
          
          
          
         */ 
    }
}
