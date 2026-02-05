using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.ajax
{
    public partial class downloadReport : JsonBase
    {
        #region Variables

        private EdiManager edim;
        private GeneralManager gm;
        private ReportGenManager rgm;
        private ImportExportManager iem;
        private ElectronicInvoiceMananger eim;

        private Dictionary<string, string> parametersDict;
        private int sysReportTemplateTypeId;
        private List<int> reportIds = new List<int>();

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            edim = new EdiManager(PageBase.ParameterObject);
            gm = new GeneralManager(PageBase.ParameterObject);
            rgm = new ReportGenManager(PageBase.ParameterObject);
            iem = new ImportExportManager(PageBase.ParameterObject);
            eim = new ElectronicInvoiceMananger(PageBase.ParameterObject);

            if (!Int32.TryParse(QS["templatetype"], out sysReportTemplateTypeId))
                return;

            if (!String.IsNullOrEmpty(QS["reportidlist"]))
                reportIds.AddRange(StringUtility.SplitNumericList(QS["reportidlist"]));

            this.parametersDict = QS.ConvertToDict();

            #endregion

            #region Parse selection

            switch (sysReportTemplateTypeId)
            {
                #region Pre-generated reports

                case (int)SoeReportTemplateType.SymbrioEdiSupplierInvoice:
                    ExportEdiSupplierInvoice();
                    break;
                case (int)SoeReportTemplateType.ReadSoftScanningSupplierInvoice:
                    ExportScanningSupplierInvoice();
                    break;
                case (int)SoeReportTemplateType.SupplierInvoiceImage:
                    if (!String.IsNullOrEmpty(QS["invoiceId"]))
                    {
                        int invoiceId = 0;
                        Int32.TryParse(QS["invoiceId"], out invoiceId);
                        if (invoiceId > 0)
                            ExportSupplierInvoiceImage(invoiceId);
                    }
                    else if (!String.IsNullOrEmpty(QS["invoiceIds"]))
                    {
                        var ids = StringUtility.SplitNumericList(QS["invoiceIds"]);
                        if (ids.Count > 0)
                            ExportSupplierInvoiceImages(ids);
                    }
                    break;
                case (int)SoeReportTemplateType.Finvoice:
                    ExportFinvoice();
                    break;
                case (int)SoeReportTemplateType.EfhInvoice:
                    ExportEfhInvoice();
                    break;
                case (int)SoeReportTemplateType.Svefaktura:
                    ExportSvefaktura();
                    break;
                case (int)SoeReportTemplateType.CSR:
                    ExportCSR();
                    break;
                case (int)SoeReportTemplateType.EmployeeVacationDebtReport:
                    ExportEmployeeVacationDebtReport();
                    break;
                case (int)SoeReportTemplateType.TimeSaumaSalarySpecificationReport:
                    ExportTimeSalarySpecificationReport();
                    break;
                case (int)SoeReportTemplateType.ReportTransfer:
                    ExportReportTransferFile();
                    break;
                case (int)SoeReportTemplateType.Unknown:
                    int reportprintoutid = 0;
                    Int32.TryParse(QS["reportprintoutid"], out reportprintoutid);
                    int userId = 0;
                    Int32.TryParse(QS["reportuserid"], out userId);
                    if (reportprintoutid != 0 && userId != 0)
                        DownLoadReport(reportprintoutid, userId);
                    break;
                    #endregion
            }

            #endregion

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }

        #region Pre-generated reports

        private void ExportEdiSupplierInvoice()
        {
            var reportItem = new EdiSupplierInvoiceReportItem(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.SymbrioEdiSupplierInvoice, this.parametersDict);

            EdiEntry ediEntry = edim.GetEdiEntry(reportItem.EdiEntryId, SoeCompany.ActorCompanyId, ignoreState: true);
            if (ediEntry != null)
            {
                string fileName = GetFileName(GetText(5373, "Lev.faktura EDI"), ediEntry.InvoiceNr, Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                OpenGeneratedReport(ediEntry.PDF, SoeExportFormat.Pdf, fileName);
            }
        }

        private void DownLoadReport(int reportPrintOutId, int userId)
        {
            ReportManager rm = new ReportManager(PageBase.ParameterObject);

            if (PageBase.ParameterObject == null || PageBase.ParameterObject.UserId != userId)
                return;

            var report = rm.GetReportPrintout(reportPrintOutId, PageBase.ParameterObject.ActorCompanyId, userId);

            var timestampString = report.DeliveredTime.HasValue ?  " " + report.DeliveredTime.Value.ToString("yyyyMMddHHmmss") : "";  

            if (report != null)
                OpenGeneratedReport(report.Data, (SoeExportFormat)report.ExportFormat, report.ReportName + timestampString, addExtension: true);
        }

        private void ExportScanningSupplierInvoice()
        {
            var reportItem = new EdiScanningSupplierInvoiceImageDTO(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.ReadSoftScanningSupplierInvoice, this.parametersDict);

            EdiEntry ediEntry = reportItem.EdiEntryId > 0 ? edim.GetEdiEntry(reportItem.EdiEntryId, SoeCompany.ActorCompanyId) : null;

            if (ediEntry != null && ediEntry.UsesDataStorage)
            {
                var image = edim.GetEdiInvoiceImageFromDataStorage(SoeCompany.ActorCompanyId, ediEntry.EdiEntryId);
                OpenGeneratedReport(image.Image, SoeExportFormat.Pdf, image.Filename);
                return;
            }

            if (ediEntry != null && ediEntry.PDF != null && ediEntry.PDF.Length > 0)
            {
                var fileName = GetFileName(GetText(5500, "Lev.faktura scanning"), ediEntry.InvoiceNr, Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                OpenGeneratedReport(ediEntry.PDF, SoeExportFormat.Pdf, fileName);
                return;
            }

            ScanningEntry scanningEntry = edim.GetScanningEntry(reportItem.ScanningEntryId, SoeCompany.ActorCompanyId, ignoreState: true);
            if (scanningEntry != null)
            {
                string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                byte[] data = PDFUtility.CreatePdfFromTif(scanningEntry.Image, destinationFileName, true);
                if (data != null)
                {
                    string fileName = "";
                    if (scanningEntry.MessageType == (int)TermGroup_ScanningMessageType.Arrival)
                    {
                        fileName = GetFileName(GetText(5499, "Ankomstregistrering scanning"), null, Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                    }
                    else if (scanningEntry.MessageType == (int)TermGroup_ScanningMessageType.SupplierInvoice)
                    {
                        fileName = GetFileName(GetText(5500, "Lev.faktura scanning"), (ediEntry != null ? ediEntry.InvoiceNr : null), Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                    }

                    OpenGeneratedReport(data, SoeExportFormat.Pdf, fileName);
                }
            }
        }

        private void ExportSupplierInvoiceImage(int invoiceId)
        {
            var invoiceManager = new SupplierInvoiceManager(PageBase.ParameterObject);
            var image = invoiceManager.GetSupplierInvoiceImage(SoeCompany.ActorCompanyId, invoiceId);
            if (image != null)
            {
                string fileName = GetFileName(GetText(31, "Leverantörsfaktura"), null, Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                if (image.ImageFormatType == SoeDataStorageRecordType.InvoicePdf)
                {
                    OpenGeneratedReport(image.Image, SoeExportFormat.Pdf, fileName);
                }
                else
                {
                    string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                    byte[] data = PDFUtility.CreatePdfFromTif(image.Image, destinationFileName, true);
                    if (data != null)
                    {
                        OpenGeneratedReport(data, SoeExportFormat.Pdf, fileName);
                    }
                }
            }
        }



        private void ExportSupplierInvoiceImages(List<int> invoiceIds)
        {
            List<Tuple<string, byte[]>> files = new List<Tuple<string, byte[]>>();
            var invoiceManager = new SupplierInvoiceManager(PageBase.ParameterObject);
            string fileName = GetFileName(GetText(1768, "Leverantörsfakturor"), null, Constants.SOE_SERVER_FILE_ZIP_SUFFIX);
            foreach (var id in invoiceIds)
            {
                var image = invoiceManager.GetSupplierInvoiceImage(SoeCompany.ActorCompanyId, id);
                var invoice = invoiceManager.GetSupplierInvoice(id, false, false, false, false, false, false, false, false);
                if (image != null && invoice != null)
                {
                    if (image.ImageFormatType == SoeDataStorageRecordType.InvoicePdf)
                    {
                        files.Add(new Tuple<string, byte[]>(invoice.InvoiceNr + Constants.SOE_SERVER_FILE_PDF_SUFFIX, image.Image));
                    }
                    else
                    {
                        string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                        byte[] data = PDFUtility.CreatePdfFromTif(image.Image, destinationFileName, true);
                        if (data != null)
                        {
                            files.Add(new Tuple<string, byte[]>(invoice.InvoiceNr + Constants.SOE_SERVER_FILE_PDF_SUFFIX, data));
                        }
                    }
                }
            }

            //Set path
            string guid = base.SoeUserId + DateTime.Now.Millisecond.ToString();
            string tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL;
            string zippedpath = $@"{tempfolder}\{guid}.zip";

            if (ZipUtility.ZipFiles(zippedpath, files))
            {
                var data = File.ReadAllBytes(zippedpath);
                File.Delete(zippedpath);

                OpenGeneratedReport(data, SoeExportFormat.Zip, fileName);
            }
        }

        private void ExportFinvoice()
        {
            var reportItem = new FinvoiceReportDTO(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.ReadSoftScanningSupplierInvoice, this.parametersDict);

            List<FileDataItem> fileList;
            eim.CreateFinvoice(SoeCompany.ActorCompanyId, reportItem.InvoiceId, reportItem.Original, out fileList);
            var file = fileList.FirstOrDefault();
            if (file != null)
            {
                string fileName = GetFileName(GetText(8172, "Finvoice"), reportItem.InvoiceNr, Constants.SOE_SERVER_FILE_XML_SUFFIX);
                OpenGeneratedReport(file.FileData, SoeExportFormat.Xml, fileName);
            }
        }

        private void ExportEfhInvoice()
        {
            var reportItem = new EfhInvoiceReportDTO(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.EfhInvoice, this.parametersDict);

            byte[] data = iem.CreateEHFInvoice(SoeCompany.ActorCompanyId, reportItem.InvoiceId);
            if (data != null)
            {
                string fileName = GetFileName(GetText(7146, "EHFfaktura"), reportItem.InvoiceNr, Constants.SOE_SERVER_FILE_XML_SUFFIX);
                OpenGeneratedReport(data, SoeExportFormat.Xml, fileName);
            }
        }

        private void ExportSvefaktura()
        {
            var reportItem = new SvefakturaReportDTO(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.Svefaktura, this.parametersDict);

            var result = eim.CreatePeppolSveFaktura(SoeCompany.ActorCompanyId, UserId, reportItem.InvoiceId, String.Empty, null, null, TermGroup_EInvoiceFormat.Svefaktura, out byte[] data);
            if (data != null)
            {
                string fileName = GetFileName(GetText(8199, "Svefaktura"), reportItem.InvoiceNr, Constants.SOE_SERVER_FILE_XML_SUFFIX);
                OpenGeneratedReport(data, SoeExportFormat.Xml, fileName);
            }
        }

        private void ExportCSR()
        {
            Dictionary<string, string> employeeCsrData = Session[Constants.SESSION_DOWNLOAD_SCR_ITEM] as Dictionary<string, string>;
            if (employeeCsrData != null)
            {
                byte[] data = iem.CreateCSRExportFile(employeeCsrData, PageBase.SoeCompany.ActorCompanyId, PageBase.UserId, DateTime.Now.Year.ToString());
                if (data != null)
                {
                    string fileName = GetFileName("CSRExport", null, Constants.SOE_SERVER_FILE_XML_SUFFIX);
                    OpenGeneratedReport(data, SoeExportFormat.Xml, fileName);
                }

                Session[Constants.SESSION_DOWNLOAD_SCR_ITEM] = null;
            }
        }

        private void ExportReportTransferFile()
        {
            byte[] data = iem.createReportTransferFile(reportIds, SoeCompany.ActorCompanyId);
            if (data != null)
            {
                string fileName = GetFileName("ReportExport", null, Constants.SOE_SERVER_FILE_XML_SUFFIX);
                OpenGeneratedReport(data, SoeExportFormat.Xml, fileName);
            }
        }

        private void ExportEmployeeVacationDebtReport()
        {
            var reportItem = new EmployeeVacationDebtReport(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.EmployeeVacationDebtReport, this.parametersDict);

            DataStorage dataStorage = gm.GetDataStorage(reportItem.DataStorageId, SoeCompany.ActorCompanyId);
            if (dataStorage != null)
            {
                string fileName = GetFileName(TextService.GetText(64, (int)TermGroup.SysReportTemplateType), dataStorage.Description.Replace(',', '_'), Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                OpenGeneratedReport(dataStorage.Data, SoeExportFormat.Pdf, fileName);
            }
        }

        private void ExportTimeSalarySpecificationReport()
        {
            var reportItem = new TimeSaumaSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, (int)SoeReportTemplateType.TimeSalarySpecificationReport, this.parametersDict);

            byte[] data = iem.CreateSaumaTimeSalarySpecificationReport(SoeCompany.ActorCompanyId, reportItem.DataStorageId);
            if (data != null)
            {
                string fileName = GetFileName("palkkalaskelma", null, Constants.SOE_SERVER_FILE_PDF_SUFFIX);
                OpenGeneratedReport(data, SoeExportFormat.Pdf, fileName);
            }
        }

        private string GetFileName(string name, string extension, string suffix)
        {
            if (!String.IsNullOrEmpty(extension))
                return name + " " + extension + suffix;
            else
                return name + suffix;
        }


        private void OpenGeneratedReport(byte[] data, SoeExportFormat exportFormat, string filename, bool addExtension = false)
        {
            if (!String.IsNullOrEmpty(filename))
                filename = filename.Replace(',', ' ');

            //Response type settings
            rgm.GetResponseContentType(exportFormat, out string contentType, out string fileType, out _);

            //Response settings
            HttpContext.Current.Response.ContentType = contentType;
            HttpContext.Current.Response.Expires = Constants.SOE_SESSION_TIMEOUT_MINUTES;
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Cache.SetNoServerCaching();
            HttpContext.Current.Response.AddHeader("Content-Type", contentType);
            HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + filename + (addExtension ? $"{fileType}" : string.Empty));
            if (data != null)
                HttpContext.Current.Response.BinaryWrite(data);
            HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        #endregion
    }
}