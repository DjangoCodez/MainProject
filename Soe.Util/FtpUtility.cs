using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SoftOne.Soe.Util
{
    public class FtpUtility
    {
        /// <summary>
        /// Returns all files in given FTP.
        /// Throws Exception if occurred.
        /// </summary>
        /// <param name="uri">The file URI</param>
        /// <param name="ftpUserName">The UserName</param>
        /// <param name="ftpPassword">The password</param>
        /// <returns>A list of filenames</returns>
        public static List<string> GetFileList(Uri uri, string ftpUserName, string ftpPassword, bool ignoreFolders = false, bool ignoreFilesStartingWithDot = false, bool onlyFolders = false, params string[] ignoreFilesList)
        {
            List<string> files = new List<string>();

            FtpWebRequest ftpRequest = null;
            WebResponse ftpResponse = null;
            Stream responseStream = null;
            StreamReader responseReader = null;

            try
            {
                //Connect
                ftpRequest = FtpWebRequest.Create(uri) as FtpWebRequest;
                if (!String.IsNullOrEmpty(ftpUserName) && !String.IsNullOrEmpty(ftpPassword))
                    ftpRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.UseBinary = true;

                //Get data
                ftpResponse = ftpRequest.GetResponse() as FtpWebResponse;
                responseStream = ftpResponse.GetResponseStream();
                responseReader = new StreamReader(responseStream);
                while (!responseReader.EndOfStream)
                {
                    string file = responseReader.ReadLine();
                    if (ignoreFolders && !file.Contains("."))
                        continue;

                    // Do not allow files starting with "." since this can be a directory or .ftpquota file
                    if (ignoreFilesStartingWithDot && file.StartsWith("."))
                        continue;

                    if (onlyFolders && file.Contains("."))
                        continue;
                    

                    files.Add(file);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (responseReader != null)
                    responseReader.Close();
                if (responseStream != null)
                    responseStream.Close();
                if (ftpResponse != null)
                    ftpResponse.Close();
            }

            return files;
        }

        /// <summary>
        /// Upload data to given FTP.
        /// Throws Exception if occurred.
        /// </summary>
        /// <param name="uri">The file URI</param>
        /// <param name="data">The data to upload</param>
        /// <param name="ftpUserName">The UserName</param>
        /// <param name="ftpPassword">The password</param>
        /// <returns>True if the data was uploaded, otherwise false</returns>
        public static byte[] UploadData(Uri uri, byte[] data, string ftpUserName, string ftpPassword)
        {
            if (uri.Scheme != Uri.UriSchemeFtp)
                return null;

            byte[] uploadedData = null;
            WebClient client = null;

            try
            {

                // Client
                client = new WebClient();
                
                // Credentials
                if (!String.IsNullOrEmpty(ftpUserName) && !String.IsNullOrEmpty(ftpPassword))
                    client.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                // Perform
                uploadedData = client.UploadData(uri, data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Dispose();
            }

            return uploadedData;
        }

        public static bool UploadData(string ftpAddress, string fileName, string ftpUserName, string ftpPassword)
        {
            FileInfo fileInfo = new FileInfo(fileName);

            if (!ftpAddress.EndsWith(@"\") && !ftpAddress.EndsWith(@"/"))
                ftpAddress += @"\";
            string uri = ftpAddress + fileInfo.Name;

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            request.KeepAlive = false;
            request.UseBinary = true;
            request.ContentLength = fileInfo.Length;

            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = null;
            Stream stream = null;

            try
            {
                fs = fileInfo.OpenRead();
                if (fs != null)
                {
                    stream = request.GetRequestStream();
                    contentLen = fs.Read(buff, 0, buffLength);
                    while (contentLen != 0)
                    {
                        stream.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if(stream != null)
                    stream.Close();
                if (fs != null)
                    fs.Close();
            }
        }

        /// <summary>
        /// Downloads data from given FTP.
        /// Throws Exception if occurred.
        /// </summary>
        /// <param name="fileUri">The file URI</param>
        /// <param name="ftpUserName">The UserName</param>
        /// <param name="ftpPassword">The password</param>
        /// <returns>True if the data was downloaded, otherwise false</returns>
        public static byte[] DownloadData(Uri fileUri, string ftpUserName, string ftpPassword)
        {
            if (fileUri.Scheme != Uri.UriSchemeFtp)
                return null;

            byte[] downloadedData = null;
            WebClient client = null;

            try
            {
                // Client
                client = new WebClient();

                // Credentials
                if (!String.IsNullOrEmpty(ftpUserName) && !String.IsNullOrEmpty(ftpPassword))
                    client.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                // Perform
                downloadedData = client.DownloadData(fileUri.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Dispose();
            }

            return downloadedData;
        }

        /// <summary>
        /// Deletes a file on given FTP.
        /// </summary>
        /// <param name="uri">The file URI</param>
        /// <param name="ftpUserName">The UserName</param>
        /// <param name="ftpPassword">The password</param>
        /// <param name="ex">The Exception if method caused one</param>
        /// <returns>True if the file was deleted, otherwise false</returns>
        public static bool DeleteFile(Uri uri, string ftpUserName, string ftpPassword, out Exception exception)
        {
            exception = null;

            bool success = false;
            if (uri.Scheme != Uri.UriSchemeFtp)
                return success;

            FtpWebRequest ftpRequest = null;
            FtpWebResponse ftpResponse = null;

            try
            {
                //Connect
                ftpRequest = WebRequest.Create(uri) as FtpWebRequest;
                ftpRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                //Perform
                ftpResponse = ftpRequest.GetResponse() as FtpWebResponse;
                string status = ftpResponse.StatusDescription;

                success = true;
            }
            catch (Exception ex)
            {
                exception = ex;
                success = false;
            }
            finally
            {
                if(ftpResponse != null)
                   ftpResponse.Close();
            }           
            
            return success;
        }

        public static bool MakeDirectory(Uri uri, string ftpUser, string ftpPassword, out Exception exception)
        {
            exception = null;

            bool success = false;
            if (uri.Scheme != Uri.UriSchemeFtp)
                return success;

            FtpWebRequest ftpRequest = null;
            FtpWebResponse ftpResponse = null;

            try
            {
                //Connect
                ftpRequest = WebRequest.Create(uri) as FtpWebRequest;
                ftpRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                //Perform
                ftpResponse = ftpRequest.GetResponse() as FtpWebResponse;
                string status = ftpResponse.StatusDescription;

                success = true;
            }
            catch (Exception ex)
            {
                exception = ex;
                success = false;
            }
            finally
            {
                if (ftpResponse != null)
                    ftpResponse.Close();
            }

            return success;
        }
    }
}
