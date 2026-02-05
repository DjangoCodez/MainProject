using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace SoftOne.Soe.Business.Util.Azure
{
    public class BlobUtil
    {
        public const string CONTAINER_TEMP = "tempfiles"; //SAS TTL = 5 min
        public const string CONTAINER_LONG_TEMP = "uploadedfile"; //SAS TTL = 48h 
        public const string CONTAINER_DATASTORAGE = "datastorage"; //SAS TTL = 48h

        public BlobServiceClient blobClient = null;
        public BlobContainerClient container = null;

        public BlobUtil()
        {
        }
       
        public string Init(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                return "containerName is null or empty";

            containerName = containerName.ToLower();
            blobClient = new BlobServiceClient(ConfigurationSetupUtil.GetBlobStorageConnectionString());
            container = blobClient.GetBlobContainerClient(containerName);
            if (container == null)
                return "container is null";

            container.CreateIfNotExists();

            return "done";
        }

        public ActionResult UploadFile(Guid guid, string filePath)
        {
            ActionResult result = new ActionResult();

            try
            {
                var blockBlob = container.GetBlobClient(guid.ToString());
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    blockBlob.Upload(fileStream);
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public ActionResult UploadData(Guid guid, byte[] data, string fileName, string contentType)
        {
            ActionResult result = new ActionResult();
            string path = ConfigSettings.SOE_SERVER_DIR_TEMP_UPLOADEDFILES_PHYSICAL + @"\" + guid.ToString();

            File.WriteAllBytes(path, data);

            try
            {

                var blockBlob = container.GetBlobClient(guid.ToString());
                var blobHttpHeader = new BlobHttpHeaders();
                blobHttpHeader.ContentDisposition = "attachment; filename=" + FixFileName(fileName);
                blobHttpHeader.ContentType = contentType;
                var uploadedBlob = blockBlob.Upload(path, blobHttpHeader);

                File.Delete(path);

            }
            catch
            {
                File.Delete(path);
                result.Success = false;
            }

            return result;
        }

        public ActionResult UploadData(string fileName, byte[] data)
        {
            ActionResult result = new ActionResult();

            try
            {
                var blockBlob = container.GetBlobClient(fileName);
                using (var ms = new MemoryStream(data))
                {
                    blockBlob.Upload(ms);
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public ActionResult UploadFile(string filePath, string fileName)
        {
            ActionResult result = new ActionResult();

            try
            {
                var blockBlob = container.GetBlobClient(fileName);

                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    if (fileStream.Length > 0)
                        blockBlob.Upload(fileStream);
                    else
                        result.Success = false;
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public ActionResult DownloadFile(Guid guid, string filePath)
        {
            ActionResult result = new ActionResult();

            try
            {
                var blockBlob = container.GetBlobClient(guid.ToString());

                using (var fileStream = System.IO.File.OpenWrite(filePath))
                {
                    blockBlob.DownloadTo(fileStream);
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public byte[] DownloadFile(string fileName, string filePath)
        {
            byte[] byteArray = null;

            try
            {
                var blockBlob = container.GetBlobClient(fileName);

                blockBlob.DownloadTo(filePath);
                byteArray = File.ReadAllBytes(filePath);
                File.Delete(filePath);
            }
            catch
            {
                return null;
            }

            return byteArray;
        }

        public string GetDownloadLink(string guid, string fileName = "")
        {
            try
            {
                var blockBlob = container.GetBlobClient(guid.ToString());

                if (blockBlob.CanGenerateSasUri)
                {
                    if (!blockBlob.Exists())
                        return "";

                    if (string.IsNullOrEmpty(fileName))
                    {
                        blockBlob.GetProperties();
                    }

                    var blobHttpHeader = new BlobHttpHeaders();
                    blobHttpHeader.ContentDisposition = string.Format("attachment;filename=\"{0}\"", fileName);
                    blobHttpHeader.ContentType = GetContentType(fileName);

                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = container.Name,
                        BlobName = blockBlob.Name,
                        Resource = "b"
                    };
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(this.GetBlobSASTTL());
                    sasBuilder.ContentDisposition = string.Format("attachment;filename=\"{0}\"", fileName);
                    sasBuilder.ContentType = GetContentType(fileName);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                    var policy = container.GetAccessPolicy();
                    Uri sasUri = container.Uri;
                    return blockBlob.GenerateSasUri(sasBuilder).AbsoluteUri;

                }
            }
            catch
            {
                return "";
            }

            return "";


        }

        public byte[] DownloadArray(Guid guid)
        {
            var blockBlob = container.GetBlobClient(guid.ToString());
            string path = ConfigSettings.SOE_SERVER_DIR_TEMP_UPLOADEDFILES_PHYSICAL + @"\" + guid.ToString();


            blockBlob.DownloadTo(path);
            byte[] blobArray = File.ReadAllBytes(path);
            File.Delete(path);

            return blobArray;
        }
        public ActionResult DeleteFile(Guid guid)
        {
            ActionResult result = new ActionResult();

            try
            {
                var blockBlob = container.GetBlobClient(guid.ToString());
                blockBlob.Delete();
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public bool fileExist(string guid)
        {
            var blockBlob = container.GetBlobClient(guid.ToString());

            return blockBlob.Exists();
        }

        private static string FixFileName(string filename)
        {
            return WebUtility.HtmlEncode(filename);
        }

        private int GetBlobSASTTL()
        {
            if (container.Name == CONTAINER_TEMP)
                return 5;
            else
                return 48 * 60;
        }

        private static string GetContentType(string filename)
        {
            return WebUtil.GetContentType(filename);
        }

    }
}

