using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace DiskAnalysis
{    
    class Tree
    {
        public string rootFolder;
        public DateTime start, end;
        public Int32 filecount, foldercount;

        private int rootID, level;
        private ManualResetEvent ev;
        private NetworkDrive drive = null;        
        private DiskReport.DiskReportDataContext dc;
        private string processedFolder;

        public Tree(string rootFolder, int rootID, int level, ManualResetEvent ev)
        {
            dc = new DiskReport.DiskReportDataContext(Program.sb.ConnectionString);
            dc.ObjectTrackingEnabled = false;

            this.rootFolder = rootFolder;
            this.level = level;
            this.rootID = rootID;

            this.ev = ev;

            filecount = 0;
            foldercount = 0;
        }

        [MTAThread]
        public void  startProcess()
        {
            if(Program._pool!=null) Program._pool.WaitOne();

            initProcess();

            this.start = DateTime.Now;
            Console.WriteLine(Thread.CurrentThread.Name + " start " + start);

            Int32 idfolder = insertFolder(this.rootFolder, null, 0);

            processFolder(processedFolder + @"\", 0, idfolder);

            this.end = DateTime.Now;
            Console.WriteLine(Thread.CurrentThread.Name + " end " + end);
            
            if (this.drive != null)
            {
                try
                {
                    Console.WriteLine("- Disconnecting drive " + drive.LocalDrive + " from " + this.rootFolder);
                    drive.UnMapDrive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-- Disconnecting drive Error! " + ex.Message);
                }
            }
            if (Program._pool != null) Program._pool.Release();
            ev.Set();
        }

        #region InitProcess
        /// <summary>
        /// Analyze wether the root folder is a UNC Path or a local drive
        /// if it is a UNC path it will map it on first available drive.
        /// if a drive is not available it waits until one is available
        /// remove ending backslash '\' for formating and consistency purpose 
        /// </summary>
        private void initProcess()
        {
            if (this.rootFolder.StartsWith(@"\\"))
            {
                string tmp = NetworkDrive.nextFreeDrive();
                while (tmp == "")
                {
                    Thread.Sleep(10000);
                    tmp = NetworkDrive.nextFreeDrive();
                    Console.WriteLine("- " + this.rootFolder + " waiting for free drive");
                }
                try
                {
                    drive = new NetworkDrive();
                    drive.MapDrive(tmp, this.rootFolder, false);
                    Console.WriteLine("- Connecting drive " + drive.LocalDrive + " on " + this.rootFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-- Connection error ! " + ex.Message);
                }
            }

            if (this.rootFolder.EndsWith(@"\"))
                this.rootFolder = this.rootFolder.Substring(0, this.rootFolder.Length - 1);
            
            this.processedFolder = (this.drive != null) ? drive.LocalDrive : this.rootFolder;
        }

        #endregion

        #region Process Folders
        private void  processFolder(string foldername, int lvl, Int32 idParent)
        {            
            if (lvl <= this.level) Console.WriteLine("- Folder : " + this.rootFolder + foldername.Substring(2));            
           
            try
            {
                foreach (string filename in Directory.GetFiles(foldername))
                    try
                    {
                        FileInfo attr = new FileInfo(filename);
                        this.filecount++;
                        dc.sp_InsertFile(attr.Name,
                            attr.Extension,
                            attr.Length,
                            getFileOwner(filename),
                            (attr.LastWriteTime.Year > 1950) ? attr.LastWriteTime : attr.LastAccessTime,
                            (attr.LastAccessTime.Year > 1950) ? attr.LastAccessTime : attr.LastWriteTime,
                            (attr.CreationTime.Year > 1950) ? attr.CreationTime : attr.LastAccessTime,
                            rootID,
                            idParent);
                    }
                    catch (Exception ex)
                    {
                        insert_Error("File", filename, ex, idParent);
                    }
            }
            catch (Exception ex)
            {
                insert_Error("Files", foldername, ex, idParent);                
            }
            try
            {
                foreach (string folder in Directory.GetDirectories(foldername))
                    try
                    {
                        foldercount++;
                        Int32 idfolder = insertFolder(folder, idParent, lvl + 1);
                        StoreACL(idfolder, folder);
                        processFolder(folder, lvl + 1, idfolder);
                    }
                    catch (Exception ex)
                    {
                        insert_Error("SubFolder", folder, ex, idParent);                        
                    }
            }
            catch (Exception ex)
            {
                insert_Error("Folder", foldername, ex, idParent);
                Console.WriteLine("- Folder Error : " + idParent + " : " + ex.Message);
            }                
        }
#endregion
        
        #region Insert DB

        private void insert_Error(string type, string objSource, Exception ex2, Int32 idParent)
        {
            dc.sp_InsertTreeDetailError(DateTime.Now,
                type,
                this.rootFolder + objSource.Substring(2),
                ex2.Source + "---" + ex2.Message,
                idParent);
        }

        private void StoreACL(Int32 idFolder, string folder)
        {   
            DirectoryInfo dinfo = new DirectoryInfo(folder);
            DirectorySecurity dSecurity = dinfo.GetAccessControl();
            AuthorizationRuleCollection returninfo = dSecurity.GetAccessRules(true, true, System.Type.GetType("System.Security.Principal.SecurityIdentifier"));
                        
            foreach (FileSystemAccessRule fsa in returninfo)
            {                
                try
                {
                    dc.sp_InsertFolderACL(idFolder, fsa.IdentityReference.Value,
                        (int)fsa.FileSystemRights,
                        (fsa.IsInherited) ? 1 : 0,
                        (int)fsa.InheritanceFlags,
                        (int)fsa.PropagationFlags);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }         
        }
        
        private Int32 insertFolder(string folder, Int32? idparent ,int lvl)
        {
            int? id=0;
            
            try
            {
                string tmp = "<Unknown>";
                try
                {
                    tmp = getFolderOwner(folder);
                }
                catch { }
                string name = (idparent.HasValue) ? folder.Substring(folder.LastIndexOf(@"\") + 1):folder;
                
                dc.sp_InsertFolder(name,
                    0,
                    level,
                    tmp,
                    idparent,
                    rootID,
                    this.rootFolder + folder.Substring(2),
                    ref id);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fatal Error. " + ex.Message);
                Thread.CurrentThread.Abort();
            }
            return (Int32)id;                
        }
        #endregion

        #region get owner
        private string getFileOwner(string filename)
        {
            FileSecurity tmp = new FileSecurity(filename, AccessControlSections.Owner);
            string owner = "<unknown>";
            try
            {
                owner = tmp.GetOwner(System.Type.GetType("System.Security.Principal.SecurityIdentifier")).Value;
            }
            catch { }
            return owner;
        }

        private string getFolderOwner(string path)
        {
            DirectorySecurity tmp = new DirectorySecurity(path, AccessControlSections.Owner);
            string owner = "<unknown>";
            try
            {
                owner = tmp.GetOwner(System.Type.GetType("System.Security.Principal.SecurityIdentifier")).Value;
            }
            catch{}
            return owner;
        }
        #endregion
    }  
}
