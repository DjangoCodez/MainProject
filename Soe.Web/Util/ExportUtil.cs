using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.PaymentIO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace SoftOne.Soe.Web.Util
{
    public static class ExportUtil
    {
        public static void Export(string guid, int paymentIOType)
        {
            string filePathOnServer = "";
            string fileNameOnClient = "";
            if (paymentIOType == (int)TermGroup_SysPaymentMethod.LB)
            {
                filePathOnServer = Utilities.GetLBFilePathOnServer(guid);
                fileNameOnClient = Utilities.GetLBFileNameOnClient();
            }
            else if (paymentIOType == (int)TermGroup_SysPaymentMethod.PG)
            {
                filePathOnServer = Utilities.GetPGFilePathOnServer(guid);
                fileNameOnClient = Utilities.GetPGFileNameOnClient();
            }
            else
                return;

            FileStream fs = null;

            try
            {
                if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + @"export/payment/") || !File.Exists(filePathOnServer))
                    return;

                fs = File.OpenRead(filePathOnServer);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + fileNameOnClient);
                HttpContext.Current.Response.BinaryWrite(data);

                try
                {
                    HttpContext.Current.Response.End();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception e)
            {
                e.ToString();
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static void DownloadFile(string fileNameOnClient, string xml, bool addXmlDeclaration)
        {
            DownloadFile(fileNameOnClient, null, true, xml, addXmlDeclaration);
        }

        public static void DownloadFile(string fileNameOnClient, byte[] data, bool convertToText, string xml = null, bool addXmlDeclaration = false)
        {
            FileStream fs = null;

            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + fileNameOnClient);
                if (convertToText)
                {
                    if (xml != null)
                    {
                        if (addXmlDeclaration)
                            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine + xml;

                        //Used for formatting xml with indention
                        XDocument doc = XDocument.Parse(xml);
                        xml = doc.ToString();

                        HttpContext.Current.Response.ContentType = ReturnExtension(".xml") + "; charset=UTF-8";
                        HttpContext.Current.Response.Write(xml);
                    }
                    else
                        HttpContext.Current.Response.Write(Encoding.UTF8.GetString(data));
                }
                else
                {
                    HttpContext.Current.Response.BinaryWrite(data);
                }

                try
                {
                    HttpContext.Current.Response.End();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception e)
            {
                 e.ToString();
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static void DownloadFile(string fileNameOnClient, string extension, byte[] data)
        {
            FileStream fs = null;

            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ContentType = ReturnExtension(extension);
                HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + fileNameOnClient);

                HttpContext.Current.Response.BinaryWrite(data);

                try
                {
                    HttpContext.Current.Response.End();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception e)
            {
                e.ToString();
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                int length = (int)fileStream.Length;
                buffer = new byte[length];

                int count;
                int sum = 0;
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }

            return buffer;
        }

        public static void ExportPaymentFromDatabase(int paymentExportId, int sysCountryId)
        {
            PaymentManager pm = new PaymentManager(null);
            PaymentExport pe = pm.GetPaymentExport(paymentExportId);

            if (pe == null)
            {
                return;
            }

            string fileName = "betalning.txt";
            if (pe.Type == (int)TermGroup_SysPaymentMethod.NordeaCA || pe.Type == (int)TermGroup_SysPaymentMethod.SEPA)
            {
                if (sysCountryId == 0)
                {
                    fileName = TermCacheManager.Instance.GetText(11936, 1, "SepaBetalfil");
                }
                else
                {
                    fileName = TermCacheManager.Instance.GetText(11936, 1, "SepaBetalfil", sysCountryId);
                }

                fileName = fileName + "_" + DateTime.Now.ToString(sysCountryId == 3 ? "ddMMyyyy_HHmm" : "yyyyMMdd_HH.mm") + ".xml";
            }
            else if (pe.Type == (int)TermGroup_SysPaymentMethod.ISO20022)
            {
                fileName = "ISO20022_" + DateTime.Now.ToString(sysCountryId == 3 ? "ddMMyyyy_HHmm" : "yyyyMMdd_HH.mm") + ".xml";
            }
                
            DownloadFile(fileName, pe.Data, false);
        }

        public static void DownloadAttachment(int attachmentId)
        {
            GeneralManager gm = new GeneralManager(null);

            MessageAttachmentDTO dto = gm.GetAttachment(attachmentId);

            if (dto.MessageAttachmentId != 0)
            {
                if (dto.Name == null)
                    dto.Name = "attachment";

                string[] split = dto.Name.Split(new char[] { '.' });

                DownloadFile(dto.Name, split.Last(), dto.Data);
            }
        }

        public static void DownladDataStorageFromDb(int actorCompanyId, int recordId)
        {
            GeneralManager gm = new GeneralManager(null);

            var dto = gm.GetDataStorageRecord(actorCompanyId, recordId);

            if (dto != null)
            {
                var extension = dto.RecordNumber.ToLower().TrimEnd().Split('.').LastOrDefault();
                extension = extension == null ? string.Empty : "." + extension;
                DownloadFile(dto.RecordNumber, extension, dto.DataStorage.Data);
            }
        }

        private static string ReturnExtension(string fileExtension)
        {
            if (fileExtension != null && !fileExtension.StartsWith("."))
                fileExtension = "." + fileExtension;

            switch (fileExtension)
            {
                case ".htm":
                case ".html":
                case ".log":
                    return "text/HTML";
                case ".txt":
                    return "text/plain";
                case ".doc":
                case ".docx":
                    return "application/ms-word";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                case ".asf":
                    return "video/x-ms-asf";
                case ".avi":
                    return "video/avi";
                case ".zip":
                    return "application/zip";
                case ".xls":
                case ".csv":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case "jpeg":
                    return "image/jpeg";
                case ".bmp":
                    return "image/bmp";
                case ".wav":
                    return "audio/wav";
                case ".mp3":
                    return "audio/mpeg3";
                case ".mpg":
                case "mpeg":
                    return "video/mpeg";
                case ".rtf":
                    return "application/rtf";
                case ".asp":
                    return "text/asp";
                case ".pdf":
                    return "application/pdf";
                case ".fdf":
                    return "application/vnd.fdf";
                case ".ppt":
                    return "application/mspowerpoint";
                case ".dwg":
                    return "image/vnd.dwg";
                case ".msg":
                    return "application/msoutlook";
                case ".xml":
                case ".sdxl":
                    return "application/xml";
                case ".xdp":
                    return "application/vnd.adobe.xdp+xml";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
