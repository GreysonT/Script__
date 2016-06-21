using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tools
{
    class CommandPrompt
    {
        public void DriveMap(string arg)
        {
            bool result = false;
            Process proc = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = arg;
            proc.StartInfo = startInfo;
            proc.Start();
            
        }
        // thread.join();
        //Fire event?



    }
}
