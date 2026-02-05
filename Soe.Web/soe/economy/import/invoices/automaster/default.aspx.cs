using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;

namespace SoftOne.Soe.Web.soe.economy.import.invoices.automaster
{
    public partial class _default : PageBase
    {
        #region Variables

        private ImportExportManager iem;
        private ReportManager rm;
        private ReportDataManager rdm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Import_Invoices_Automaster;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            iem = new ImportExportManager(ParameterObject);
            rm = new ReportManager(ParameterObject);
            rdm = new ReportDataManager(ParameterObject);

            AutomasterForm.Visible = true;

            AutomasterForm.SetTabHeaderText(1, GetText(4682, "Automaster import"));

            #region Actions

            if (Request.Form["action"] == "upload")
            {
                HttpPostedFile file = Request.Files["File"];
                if (file != null && file.ContentLength > 0)
                {
                    string pathOnServer = string.Empty;
                    try
                    {
                        #region Parse filename

                        string fileName = file.FileName;
                        if (fileName.Contains(@"\"))
                        {
                            fileName = file.FileName.Substring(file.FileName.LastIndexOf(@"\"));
                            fileName = fileName.Replace("\\", "");
                        }
                        if (fileName.Contains(@"/"))
                        {
                            fileName = file.FileName.Substring(file.FileName.LastIndexOf(@"/"));
                            fileName = fileName.Replace(@"/", "");
                        }
                        if (!fileName.Contains("."))
                        {
                            //Files do not always have extension, need to add
                            fileName = fileName + ".txt";
                        }

                        String serverfileName = fileName.Insert(fileName.LastIndexOf("."), Guid.NewGuid().ToString());
                        //pathOnServer = SaveFileToServer(file.InputStream, serverfileName);

                        #endregion

                        #region Parse XML

                        var result = iem.ImportAutomaster(file.InputStream, SoeCompany.ActorCompanyId);
                        if (result.Success)
                            AutomasterForm.MessageSuccess = GetText(4684, "Import klar");
                        else
                        {
                            if (string.IsNullOrEmpty(result.ErrorMessage))
                                AutomasterForm.MessageSuccess = GetText(4684, "Import klar");
                            else
                                AutomasterForm.MessageError = result.ErrorMessage;
                        }
                        #endregion

                        PrintImportReport(result.StringValue);
                    }
                    catch (Exception ex)
                    {
                        AutomasterForm.MessageError = GetText(4683, "Import misslyckades");
                        ex.ToString(); //prevent compiler warning
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(pathOnServer))
                            RemoveFileFromServer(pathOnServer);
                    }
                }
                else
                    AutomasterForm.MessageError = GetText(1179, "Filen hittades inte");
            }

            #endregion
        }

        #region Action-methods

        private string SaveFileToServer(Stream stream, string fileName)
        {
            var pathOnServer = ConfigSettings.SOE_SERVER_DIR_TEMP_AUTOMASTER_PHYSICAL + fileName;
            RemoveFileFromServer(pathOnServer);

            var file = new FileStream(pathOnServer, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

            MemoryStream ms = null;
            try
            {
                //stream not seekable convert to memory stream
                ms = new MemoryStream();
                byte[] buffer = new byte[2048];
                int bytesRead = 0;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
                stream.Close();

                //reset stream position
                ms.Position = 0;

                //convert to byte[]
                byte[] result = new byte[(int)ms.Length];
                ms.Read(result, 0, (int)ms.Length);
                if (DefenderUtil.IsVirus(pathOnServer))
                    LogCollector.LogError($"Automaster SaveFileToServer Virus detected {pathOnServer}");
                else
                    file.Write(result, 0, result.Length);
            }
            finally
            {
                ms.Close();
                ms.Dispose();
                file.Close();
            }
            return pathOnServer;
        }

        private void RemoveFileFromServer(string pathOnServer)
        {
            if (System.IO.File.Exists(pathOnServer))
                System.IO.File.Delete(pathOnServer);
        }

        private void PrintImportReport(string batchId)
        {
            int InvoiceImportReportId = rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.IOCustomerInvoice)?.FirstOrDefault().Key ?? 0;
            Report report = rm.GetReport(InvoiceImportReportId, SoeCompany.ActorCompanyId);

            List<int> customerInvoiceHeadIOIds = iem.GetCustomerInvoiceHeadIOResult(SoeCompany.ActorCompanyId, TermGroup_IOType.Unknown, TermGroup_IOSource.Unknown, batchId).Select(i => i.CustomerInvoiceHeadIOId).ToList();

            if (report != null)
            {
                #region ReportItem

                int sysReportTemplateTypeId = (int)SoeReportTemplateType.SEPAPaymentImportReport;
                CustomerInvoiceIOReportDTO reportItem = new CustomerInvoiceIOReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, null, webBaseUrl: true);

                #endregion

                #region Selection

                int exportType = report.ExportType != (int)TermGroup_ReportExportType.Unknown ? report.ExportType : (int)TermGroup_ReportExportType.Pdf;
                Selection selection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, report: report.ToDTO(), isMainReport: true, exportType: exportType);
                selection.SelectionLedger = new SelectionLedger();
                selection.Evaluated.Evaluate(reportItem);

                string dateFrom = F["FromDate"];
                bool hasFromDate = false;
                bool hasToDate = false;
                DateTime? fromDate = null;
                if (!String.IsNullOrEmpty(dateFrom))
                {
                    fromDate = CalendarUtility.GetDateTime(dateFrom);
                    if (fromDate != null)
                    {
                        selection.Evaluated.DateFrom = fromDate.Value;
                        hasFromDate = true;
                    }
                }

                string dateTo = F["ToDate"];
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(dateTo))
                {
                    toDate = CalendarUtility.GetDateTime(dateTo);
                    if (fromDate != null)
                    {
                        selection.Evaluated.DateTo = toDate.Value;
                        hasToDate = true;
                    }
                }

                if (hasFromDate || hasToDate)
                    selection.Evaluated.HasDateInterval = true;

                selection.Evaluated.SI_CustomerInvoiceHeadIOIds = customerInvoiceHeadIOIds;

                #endregion

                #region Print

                int reportPrintoutId = 0;

                if (UseCrystalService())
                {
                    try
                    {
                        string culture = Thread.CurrentThread.CurrentCulture.Name;
                        var channel = GetCrystalServiceChannel();
                        reportPrintoutId = channel.PrintReport(selection.Evaluated, SoeCompany.ActorCompanyId, UserId, culture);
                    }
                    catch (Exception ex)
                    {
                        SysLogManager.LogError<_default>(ex);
                    }
                }
                else if (UseWebApiInternal())
                {
                    try
                    {
                        string culture = Thread.CurrentThread.CurrentCulture.Name;
                        var connector = new ReportConnector();
                        reportPrintoutId = connector.PrintReport(selection.Evaluated, SoeCompany.ActorCompanyId, UserId, culture);
                    }
                    catch (Exception ex)
                    {
                        SysLogManager.LogError<_default>(ex);
                    }
                }
                else
                {
                    rdm = new ReportDataManager(base.ParameterObject);
                    reportPrintoutId = rdm.PrintReportId(selection.Evaluated);
                }

                ReportPrintout reportPrintout = rm.GetReportPrintout(reportPrintoutId, SoeCompany.ActorCompanyId);
                if (rm.DoShowReportPrintoutErrorMessage(reportPrintout))
                {
                    RedirectToSelf(GetText(5971, "Rapport kunde inte skrivas ut"), true);
                    return;
                }

                #endregion

                #region Download

                string guid = Guid.NewGuid().ToString();
                Session[Constants.SESSION_DOWNLOAD_REPORT_ITEM + guid] = reportPrintout;
                Response.Redirect(String.Format("/soe/common/distribution/reports/selection/download/?c={0}&r={1}&email={2}&guid={3}", SoeCompany.ActorCompanyId, RoleId, String.Empty, guid));

                #endregion
            }

        }
        #endregion
    }
}