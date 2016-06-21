using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Tools
{
    class Worker
    {
        // loading icon code here
        // call with 
        //--- Worker workerObject = new Worker();
        //--- Thread workerThread = new Thread(workerObject.loadingIcon);

        // stop with
        //--- workerObject.RequestStop();

        // Use the Join method to block the current thread  
        // until the object's thread terminates.
        //--- workerThread.Join();

            // ADD A GRAPH VERSION

        public void Logging(int timeOut = 3600) // char verbose v for verbose logging
        {
            int counter = 0;
            PerformanceCounter cpuCounter, ramCounter, diskUsage, pageFile;
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            diskUsage = new PerformanceCounter("LogicalDisk", "Avg. Disk Bytes/Transfer", "C:");
            pageFile = new PerformanceCounter("Paging File", "% Usage", "_Total");
            string error = string.Empty;
            using (StreamWriter sw = new StreamWriter(Directory.GetDirectoryRoot
                    (AppDomain.CurrentDomain.BaseDirectory)
                        + "Health.txt"))
            {
                while (!_shouldStop || counter <= timeOut)
                {
                    error += cpuCounter.NextValue() >= 75 ? " [ CPU over 75% ] " : " [ CPU is OK ] ";
                    error += ramCounter.NextValue() <= 2048 
                        ? "[ Available RAM is under 2048MB ] " : "[ RAM is OK ] ";
                    sw.WriteLine(DateTime.Now.ToString() + error 
                        + " -D [" + diskUsage.NextValue() + " Avg. Bytes/sec ]" 
                            + " -P " + pageFile.NextValue());
                    error = string.Empty;
                    Thread.Sleep(1000);
                }
            }  
        }
        public void RequestStop()
        {
            _shouldStop = true;
            IsWorking = false;
            Console.WriteLine("Thread stopped");
        }
        private volatile bool _shouldStop;
        public volatile bool IsWorking = true;
    }
}
