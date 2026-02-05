using SoftOne.EdiAdmin.Business.Core;
using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business
{
    public sealed class EdiAdminFolderWatcherManager : EdiAdminManagerBase, IEdiAdminManagerSingelton
    {
        //#region Singelton implimentation
        //private static EdiAdminFolderWatcherManager instance;

        //public static EdiAdminFolderWatcherManager Instance
        //{
        //    get 
        //    {
        //        if (instance == null)
        //        {
        //            instance = new EdiAdminFolderWatcherManager();
        //        }
        //        return instance;
        //    }
        //}

        //#endregion

        private IEdiFetcher fetcher;
        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        public bool IsInitialized {get; private set;}

        public EdiAdminFolderWatcherManager(IMessageParser parser) : base(parser) 
        {
            this.fetcher = new EdiFetcherFile();
        }

        public override void Setup(bool redirectOutPut, int? sysScheduledJobId = null, int? batchNr = null)
        {
            if (this.IsInitialized)
                return;

            base.Setup(redirectOutPut, sysScheduledJobId, batchNr);

            this.IsInitialized = true;

            var wholesellerEdis = this.EdiSysManager.GetSysWholesellerEDIs(SysWholesellerEdiManagerType.EdiAdminManagerFileWatch);
            foreach (var ws in wholesellerEdis)
            {
                this.AddWatch(ws);
            }
        }

        public override string GetStatusMessage()
        {
            string msg = string.Empty;
            if (this.IsInitialized && this.watchers.All(w => w.EnableRaisingEvents == true))
                msg = string.Format("Filewatcher is running and watching {0} folders", this.watchers.Count) + Environment.NewLine;
            else
                msg = string.Format("Filewatcher is stopped") + Environment.NewLine;

            return msg + base.GetStatusMessage();
        }

        public void StartWatch(int sysScheduledJobId)
        {
            if (!this.IsInitialized)
                throw new Exception("Must call Setup before StartWatch");
                
            foreach (var item in this.watchers)
            {
                // Begin watching.
                item.EnableRaisingEvents = true;
            }
        }

        protected bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            
            //file is not locked
            return false;
        }

        private void OnFileAdded(object sender, FileSystemEventArgs e, SysWholesellerEdi sysWholesellerEdi)
        {
            Console.Out.WriteLine("Folderwatcher found new file: {0}, path: {1} ws:[{2}]", e.Name, e.FullPath, sysWholesellerEdi.SenderId);
            string errorMessage = string.Empty;
            try
            {
                if (e.ChangeType != WatcherChangeTypes.Created)
                    return;

                // Validate
                var fileInfo = new FileInfo(e.FullPath);
                int maxCount = 20;
                while (this.IsFileLocked(fileInfo))
                {
                    // We will retry for 2 seconds (100 * 20)
                    System.Threading.Thread.Sleep(100);
                    if (--maxCount == 0)
                        throw new Exception("File is locked, cannot continue");
                }

                var result = this.messageParser.ParseMessageFromWholeseller(fetcher, e.FullPath, sysWholesellerEdi.SysWholesellerEdiId, 0);
                if (!result.Success)
                {
                    Console.Error.WriteLine("Error when parsing input file {0} from wholeseller {1}. ErrorMsg: {2}",
                    e == null ? "unknown" : e.Name,
                    sysWholesellerEdi == null ? "unknown" : sysWholesellerEdi.SenderName,
                    result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unhandled error when parsing input file {0} from wholeseller {1}. Exception: {2}",
                    e == null ? "unknown" : e.Name,
                    sysWholesellerEdi == null ? "unknown" : sysWholesellerEdi.SenderName,
                    ex.GetInnerExceptionMessages().JoinToString(". Inner: "));
            }
        }

        private void AddWatch(SysWholesellerEdi ws)
        {
            if (!string.IsNullOrEmpty(ws.EdiFolder))
            {
                string path = Constants.SOE_EDI_FOLDERWATCH_BASEPATH.TrimEnd('/', '\\') + "/" + ws.EdiFolder;
                // Validation and Error check
                #region Validation
                if (!Directory.Exists(path))
                {
                    var msg = string.Format("Warning, folder {0} did not exist for wholeseller {1}, if this wholeseller is sending by ftp this is most likely an error.", path, ws.SenderName);

#if RELEASE
                    throw new DirectoryNotFoundException(msg);
#endif

                    Directory.CreateDirectory(path);
                    Console.Error.WriteLine();
                }

                if (watchers.Any(w => w.Path == path))
                {
                    const string MSG = "Cannot have multiple wholesellers targeting against the same folder, please check the syswholeselleredi table";
                    Console.Error.WriteLine(MSG);
                    throw new Exception(MSG);
                }

                // NOT WORKING
                //if (!this.CanDelete(ws.EdiFolder))
                //    return;

                #endregion

                // Create a new FileSystemWatcher and set its properties.
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = path;
                watcher.Created += (s, e) =>
                {
                    OnFileAdded(s, e, ws);
                };
                watcher.Renamed += (s, e) =>
                {
                    OnFileAdded(s, e, ws);
                };
                watchers.Add(watcher);
            }
        }

        private bool CanDelete(string directory)
        {
            var accessControlList = Directory.GetAccessControl(directory);
            var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
    
            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Delete & rule.FileSystemRights) != FileSystemRights.Read) continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    return true;
                else
                    return false;
            }

            return false;
        }
    }
}
