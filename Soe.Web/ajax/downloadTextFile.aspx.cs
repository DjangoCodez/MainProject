using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace SoftOne.Soe.Web.ajax
{
    public partial class downloadTextFile : JsonBase
    {
        #region Variables

        private ReportGenManager rgm;
        

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Int32.TryParse(QS["id"], out int id))
                throw new SoeGeneralException("ID", this.ToString());

            string table = QS["table"]?.ToString();
            if (table != null)
            {
                SoeExportFormat exportFormat;
                bool showFile = true;                
                byte[] data;
                string fileName;

                switch (table.ToLower())
                {
                    case "editransfer":
                        #region editransfer
                        data = new byte[0];
                        exportFormat = SoeExportFormat.Xml;
                        fileName = "editransfer.txt";
                        #endregion
                        break;
                    case "edirecivedmsg":
                        #region edirecivedmsg
                        data = new byte[0];
                        exportFormat = SoeExportFormat.Text;
                        fileName = "edirecivedmsg.txt";
                        #endregion
                        break;
                    case "datastoragerecord":
                        #region datastoragerecord
                        if (QS["useedi"].HasValue() && Convert.ToBoolean(QS["useedi"]))
                        {
                            if (!int.TryParse(QS["cid"], out _))
                                throw new SoeGeneralException("COMPANYID", this.ToString());

                            showFile = false;
                            var image = GetSupplierInvoiceImage(0, ediEntryId: id);     
                            data = image.Image;
                            exportFormat = SoeExportFormat.Unknown;
                            fileName = "";
                            this.OpenInvoiceImage(data, image.Filename);
                        }
                        else
                        {
                            var gm = new GeneralManager(ParameterObject);
                            var dsr = gm.GetDataStorageRecord(ParameterObject.ActorCompanyId, id, ParameterObject.RoleId);
                            if (dsr == null)
                                throw new SoeGeneralException("DATASTORAGERECORD", this.ToString());
                            data = dsr.ToDTO().Data;
                            fileName = dsr.DataStorage.FileName ?? dsr.DataStorage.Description;
                            exportFormat = SoeExportFormat.Unknown;
                        }
                        #endregion
                        break;
                    case "invoiceimage":
                        #region invoiceimage
                        if (!Int32.TryParse(QS["cid"], out _))
                            throw new SoeGeneralException("COMPANYID", this.ToString());
                        if (!Int32.TryParse(QS["type"], out _))
                            throw new SoeGeneralException("DATASTORAGERECORTYPE", this.ToString());

                        showFile = false;
                        if (QS["useedi"].HasValue() && Convert.ToBoolean(QS["useedi"]))
                        {
                            var image = GetSupplierInvoiceImage(0, ediEntryId: id);     
                            data = image.Image;
                            exportFormat = SoeExportFormat.Unknown;
                            fileName = "";
                            this.OpenInvoiceImage(data, image.Filename);
                        }
                        else
                        {
                            var image = GetSupplierInvoiceImage(id);    
                            data = image.Image;
                            exportFormat = SoeExportFormat.Unknown;
                            fileName = "";
                            this.OpenInvoiceImage(data, image.Filename);
                        }
                        #endregion
                        break;
                    case "icafile":
                        #region icafile
                        var guid = QS["guid"].ToString();
                        var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_ICABALANCE_PHYSICAL + guid);
                        var filePath = StringUtility.GetValidFilePath(directory.FullName) + "KundSaldo.dat";
                        exportFormat = SoeExportFormat.Text;
                        fileName = "kund.dat";
                        data = File.ReadAllBytes(filePath);
                        #endregion
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (showFile)
                    this.OpenTextFile(data, exportFormat, fileName);
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void OpenTextFile(byte[] data, SoeExportFormat exportFormat, string fileName)
        {
            //Response type settings
            rgm = new ReportGenManager(null);
            if (exportFormat == SoeExportFormat.Unknown)
                exportFormat = rgm.ConvertToSoeExportFormat(fileName.Split('.').LastOrDefault());

            rgm.GetResponseContentType(exportFormat, out string contentType, out _, out _);

            //Response settings
            HttpContext.Current.Response.ContentType = contentType;
            HttpContext.Current.Response.Expires = Constants.SOE_SESSION_TIMEOUT_MINUTES;
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Cache.SetNoServerCaching();
            HttpContext.Current.Response.AddHeader("Content-Type", contentType);
            HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + fileName);
            if (data != null)
                HttpContext.Current.Response.BinaryWrite(data);
            HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void OpenInvoiceImage(byte[] data, string invoiceNumber, int type)
        {
            //string contentType = "application/jpg"; //"application/bmp"
            string fileExtension = "jpg";
            switch (type)
            {
                case (int)SoeDataStorageRecordType.InvoiceBitmap:
                    //      contentType = "application/jpeg";
                    fileExtension = "jpg";
                    break;
                case (int)SoeDataStorageRecordType.InvoicePdf:
                    //    contentType = "application/pdf";
                    fileExtension = "pdf";
                    break;
            }

            //Response settings            
            HttpContext.Current.Response.Expires = Constants.SOE_SESSION_TIMEOUT_MINUTES;
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Cache.SetNoServerCaching();
            HttpContext.Current.Response.AddHeader("Content-Type", "application/bmp");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + invoiceNumber + "." + fileExtension);
            if (data != null)
                HttpContext.Current.Response.BinaryWrite(data);
            HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void OpenInvoiceImage(byte[] data, string filename)
        {
            //Response settings            
            HttpContext.Current.Response.Expires = Constants.SOE_SESSION_TIMEOUT_MINUTES;
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Cache.SetNoServerCaching();
            HttpContext.Current.Response.AddHeader("Content-Type", "application/bmp");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);
            if (data != null)
                HttpContext.Current.Response.BinaryWrite(data);
            HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private GenericImageDTO GetSupplierInvoiceImage(int id, int? ediEntryId = null)
        {
            var sim = new SupplierInvoiceManager(ParameterObject);
            return sim.GetSupplierInvoiceImage(base.SoeActorCompanyId.Value, id, ediEntryId: ediEntryId);
        }
    }
}
