using Renci.SshNet;
using Renci.SshNet.Sftp;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SshUtil
    {
        private ConnectionInfo connectionInfo;
        private SysServiceManager ssm;

        public SshUtil(string address, string user, string password)
        {


            KeyboardInteractiveAuthenticationMethod kauth = new KeyboardInteractiveAuthenticationMethod(user);
            PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod(user, password);

            connectionInfo = new ConnectionInfo(address, user, pauth, kauth);
            this.ssm = new SysServiceManager(null);
        }

        public SshUtil(bool isAzureFTPserver)
        {
            if (isAzureFTPserver)
            {
                string address = "sftp.softone.se";
                string user = "soe";
                string password = $"jha5a##a!hwe2fff9d4s!!";

                KeyboardInteractiveAuthenticationMethod kauth = new KeyboardInteractiveAuthenticationMethod(user);
                PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod(user, password);
                connectionInfo = new ConnectionInfo(address, user, pauth, kauth);
                this.ssm = new SysServiceManager(null);
            }
        }

        public ActionResult Upload(byte[] data, string path)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    using (var client = new SftpClient(connectionInfo))
                    {
                        client.Connect();
                        client.UploadFile(stream, path, null);
                    }
                }

            }
            catch (Exception ex)
            {
                result = new ActionResult(ex, "Sftp Upload file");
            }

            return result;
        }

        public byte[] DownLoad(string path, bool deleteFileAfter = false)
        {
            using (SftpClient client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return DownLoad(client, path, deleteFileAfter);
            }
        }

        public void Rename(string oldPath, string newpath)
        {
            using (SftpClient client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.RenameFile(oldPath, newpath);
            }
        }

        public byte[] DownLoad(SftpClient client, string path, bool deleteFileAfter = false)
        {
            byte[] data = null;

            try
            {
                using (var stream = new MemoryStream())
                {
                    client.DownloadFile(path, stream);
                    data = stream.ToArray();

                    if (data != null && deleteFileAfter)
                        client.DeleteFile(path);
                }
            }
            catch (Exception ex)
            {
                ssm.LogError(ex.ToString());
            }

            if (DefenderUtil.IsVirus(data))
            {
                LogCollector.LogCollector.LogError($"Virus Detected on path {path}");
                return null;
            }
            else
                return data;
        }


        public ActionResult Delete(string path, bool deleteFileAfter = false)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (var stream = new MemoryStream())
                {
                    using (SftpClient client = new SftpClient(connectionInfo))
                    {
                        client.Connect();
                        client.DeleteFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex, "Sftp Delete file");
            }

            return result;
        }
        public Dictionary<string, byte[]> DownloadDirectory(string path)
        {
            Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                var files = ListFiles(path, client);
                if (!files.IsNullOrEmpty())
                {
                    foreach (var file in files.Where(w => w.Length != 0))
                    {
                        var data = DownLoad(client, $"{path}/{file.Name}");

                        if (data != null)
                            dict.Add(file.Name, data);
                    }
                }
            }

            return dict;
        }

        public List<SftpFile> ListFiles(string path)
        {
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return ListFiles(path, client);
            }
        }

        private List<SftpFile> ListFiles(string path, SftpClient client)
        {
            List<SftpFile> files = new List<SftpFile>();

            try
            {
                files = client.ListDirectory(path).OfType<SftpFile>().ToList();
            }
            catch (Exception ex)
            {
                ssm.LogError(ex.ToString());
            }

            return files;
        }
    }
}
