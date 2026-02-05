using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiFetcherFTP : Interfaces.IEdiFetcher
    {
        private string userName;
        private string password;

        public EdiFetcherFTP(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }

        public ActionResult GetContent(string source, OnFileFetchedDelegate onFileFetched)
        {
            ActionResult result = new ActionResult();
            try
            {
                #region Prereq

                //Download from Nelfo FTP
                Uri baseUriDownload = null;
                result = this.Validate(source);
                if (!result.Success)
                    return result;

                Uri uri = new Uri(source);

                // Get filenames from FTP
                List<string> fileNames = this.GetFilesFromFtp(uri);

                int noOfFilesProcessed = 0;
                int noOfFiles = fileNames.Count;
                if (noOfFiles == 0)
                    return result;

                #endregion

                #region Process

                foreach (string fn in fileNames)
                {
                    try
                    {
                        string fileName = fn;
                        // We want only the filename, the folder should be specified in CompanyEdi
                        if (fileName.Contains('/'))
                            fileName = fileName.Split('/').Last();

                        Uri downloadUri = new Uri(baseUriDownload.ToString().TrimEnd('/') + "/" + fileName);

                        //Download file
                        byte[] downloadedData = FtpUtility.DownloadData(downloadUri, this.userName, this.password);
                        if (downloadedData != null && downloadedData.Length > 0)
                        {
                            onFileFetched(fileName, uri.ToString(), downloadedData);
                            noOfFilesProcessed++;
                        }
                        else
                        {
                            Console.Out.WriteLine("File found but data size was zero, filename: " + fn);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Unhandled exception from {0} in GetContent(), fileName: {1}, Exception messages: {2}", this.GetType().Name, fn, ex.GetInnerExceptionMessages().JoinToString(", Inner:"));
                    }
                }

                #endregion

                #region Finalize

                //Set information to service job
                result.IntegerValue = noOfFiles;
                result.IntegerValue2 = noOfFilesProcessed;

                #endregion
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult DeleteFile(string fullPath)
        {
            ActionResult result = new ActionResult();
            var uri = new Uri(fullPath);
            Exception ex;
            FtpUtility.DeleteFile(uri, this.userName, this.password, out ex);

            if (ex != null)
            {
                result.Success = false;
                result.Exception = ex;
            }

            return result;
        }

        private List<string> GetFilesFromFtp(Uri uri)
        {
            return FtpUtility.GetFileList(uri, this.userName, this.password, ignoreFolders: true, ignoreFilesStartingWithDot: true);
        }

        private ActionResult Validate(string source)
        {
            try
            {
                var uri = new Uri(source);
            }
            catch (Exception ex)
            {
                SharedProperties.LogError(ex, "Must provide a valid ftp address for the EdiFetcherFTP {0}", source);
                return new ActionResult(false);
            }
            return new ActionResult();
        }
    }
}
