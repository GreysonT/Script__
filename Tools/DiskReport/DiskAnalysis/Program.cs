using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.AccessControl;
using Microsoft.AnalysisServices;

namespace DiskAnalysis
{
   
    class Program
    {
        public static Semaphore _pool;
        public static System.Data.SqlClient.SqlConnectionStringBuilder sb;

        #region Main
        static void Main(string[] args)
        {
            string servername = "", database = "";
            string olapServer = "", appli = "";
            int level = 1, maxThread = 0;

            foreach (string param in args)
            {
                if (param.Contains(':'))
                {
                    string name, value;
                    name = param.ToLower().Split(':')[0];
                    value = param.ToLower().Split(':')[1];
                    switch (name)
                    {
                        case "-s":
                        case "/s":
                            servername = value;
                            break;
                        case "-d":
                        case "/d":
                            database = value;
                            break;
                        case "-o":
                        case "/o":
                            olapServer = value;
                            break;
                        case "-a":
                        case "/a":
                            appli = value;
                            break;
                        case "-l":
                        case "/l":
                            level = Convert.ToInt16(value);
                            break;
                        case "-t":
                        case "/t":
                            maxThread = Convert.ToInt16(value);
                            break;
                    }
                }
            }
            Boolean testOK = (servername != "") && (database != "") && (olapServer!="") && (appli !="");

            if (testOK)
            {
                sb = new System.Data.SqlClient.SqlConnectionStringBuilder();
                sb.DataSource = servername;
                sb.InitialCatalog = database;
                sb.IntegratedSecurity = true;

                start(level, maxThread);

                updateACL();

                updateSID();

                processCube(olapServer, appli);
            }
            else
                usage();
#if DEBUG 
            Console.ReadKey();
#endif

        }
        #endregion

        #region usage
        public static void usage()
        {
            string text;
            text = "Command line application analysing disk space usage.\n\n" + 
                    "USAGE : \n" +
                    "   DiskAnalysis.exe <-s:<sqlServer>> <-d:<DatabaseName>>\n" +
                    "           <-o:<olapServer>> <-a:<ApplicationName>>\n" +                     
                    "           [-t:<maxThread>] [-l:<numLevel>]\n" +                     
                    "\n" +
                    "SQLServer : The Name or IP Address of the SQL Server.\n" +
                    "DatabaseName : The name of the database on the SQLServer.\n" +
                    "olapServer : The Name or IP Address of the Analysis Service Server.\n" +
                    "ApplicationName : The name of the database on the AS Server.\n" + 
                    "maxThread : number of paralel thread running, by default 0=unlimited.\n" +
                    "numLevel : The number of folder level to display, if 0 then every folders are shown, by default = 1\n" ;
            Console.Write(text);            
        }
#endregion

        #region clear DB
        
        public static Boolean clearDB(DiskReport.DiskReportDataContext dc)
        {
            Boolean error = false;
            Console.WriteLine("Clearing Database Details");
            Console.WriteLine("- Rebuild Tables and Indexes");
            try
            {   
                dc.sp_DELETE_TreeDetail();
            }
            catch (Exception ex)
            {
                Console.WriteLine("-- 003.1 : " + ex.Message);
                error = true;
            }

            try
            {            
                dc.sp_CREATE_TreeDetail();
            }
            catch (Exception ex)
            {
                Console.WriteLine("-- 003.2 : " + ex.Message);
                error = true;
            }
            return !error;
        }
        #endregion

        #region Scan multithread
        [MTAThread]
        private static void start(int level, int maxThread)
        {
            if(maxThread>0) _pool = new Semaphore(maxThread, maxThread);
            DateTime begin, end;
            begin = DateTime.Now;

            DiskReport.DiskReportDataContext dc = new DiskReport.DiskReportDataContext(sb.ConnectionString);
            
            if (!clearDB(dc)) return;

            var q = from t in dc.TreeDetails
                    where t.Enabled == true
                    select new { t.id, t.RootFolder };

            Tree[] tt = new Tree[q.Count()];
            ThreadStart[] tts = new ThreadStart[q.Count()];
            Thread[] tth = new Thread[q.Count()];
            ManualResetEvent[] events = new ManualResetEvent[q.Count()];
            int i=0;            

            foreach (var line in q)            
            {                
                if(line.RootFolder!="")
                {
                    events[i] = new ManualResetEvent(false);             
                    tt[i] = new Tree(line.RootFolder, line.id, level, events[i]);
                    tts[i] = new ThreadStart(tt[i].startProcess);
                    tth[i] = new Thread(tts[i]);

                    tth[i].Name = line.RootFolder;

                    tth[i].Start();

                    i++;
                    Thread.Sleep(5000);
                }
            }
            if (i == 0) return;
            WaitHandle.WaitAll(events);

            foreach (Tree t in tt)
            {
                string str;
                str = "Root " + t.rootFolder + ". Folders : " + t.foldercount +
                    ". Files : " + t.filecount + ". Process time = " + t.end.Subtract(t.start);
                
                Console.WriteLine(str);
            }

            end = DateTime.Now;

            Console.WriteLine("Scan started at " + begin + " and ended at " + end);
            Console.WriteLine("Scan time = " + end.Subtract(begin));
        }
        #endregion

        #region ACLs and SIDs
        private static void updateACL()
        {
            Console.WriteLine("- Update ACLs");
            
            DiskReport.DiskReportDataContext dc = new DiskReport.DiskReportDataContext(sb.ConnectionString);
            
            var rights = (from r in dc.FoldersACLs
                         select r.Rights).Distinct();

            foreach (Int32 right in rights)
            {
                FileSystemRights fsr = (FileSystemRights)right;
                string hex = fsr.ToString("X");
                string str = fsr.ToString();
                if (right.ToString() == str) str = hex;

                dc.sp_InsertRight(right, str);

                Console.WriteLine("insert {0} as {1}", right, str);
            }
        }

        private static void updateSID()
        {
            Console.WriteLine("- Update SIDs");
            System.Security.Principal.SecurityIdentifier sid;

            DiskReport.DiskReportDataContext dc = new DiskReport.DiskReportDataContext(sb.ConnectionString);
            dc.CommandTimeout = 6000;
            dc.sp_UpdateSIDList();

            var SSDLs = from s in dc.SIDs select s.SID1;
            foreach (string ssdl in SSDLs)
            {
                sid = new System.Security.Principal.SecurityIdentifier(ssdl);

                string ntAccount = "";

                try
                {
                    ntAccount = sid.Translate(typeof(System.Security.Principal.NTAccount)).Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} {1}", ssdl, ex.Message);
                }
                if (ntAccount == "") ntAccount = "<Unknow>";

                var s = (from a in dc.SIDs
                        where a.SID1 == ssdl
                        select a).Single();
                s.Name = ntAccount;
                dc.SubmitChanges();
            }
        }

        #endregion

        #region Process OLAP
        private static void processCube(string olapServer, string application)
        {
            Server s = new Server();
            s.Connect(olapServer);
            DateTime begin, end;
            begin = DateTime.Now;
            Console.WriteLine("- Starting cube calculation process on server " + olapServer + " application " + application);
            try{

                s.Databases[application].Process(ProcessType.ProcessFull);
            }
            catch(Exception ex)
            {
                Console.WriteLine("-- Error Processing cube :");
                Console.WriteLine(ex.Message);
                Console.WriteLine("-- Exiting now.");
            }
            finally
            {
                end = DateTime.Now;
                Console.WriteLine("- Cube processed successfully. Duration : " +end.Subtract(begin));
            }

        }
        #endregion
    }
}
