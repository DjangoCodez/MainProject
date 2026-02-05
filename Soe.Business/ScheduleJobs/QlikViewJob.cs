using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.ScheduledJobs
{
    public class QlikViewJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            ReportManager rm = new ReportManager(parameterObject);
            ReportDataManager rdm = new ReportDataManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            // Get mandatory parameters
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            int? paramReportId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "reportid").Select(s => s.IntData).FirstOrDefault();
            string paramFtpAddress = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "ftpaddress").Select(s => s.StrData).FirstOrDefault();
            string paramFtpLogin = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "ftplogin").Select(s => s.StrData).FirstOrDefault();
            string paramFtpPassword = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "ftppassword").Select(s => s.StrData).FirstOrDefault();

            // Get optional parameters
            int? paramDaysBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "daysback").Select(s => s.IntData).FirstOrDefault();
            if (!paramDaysBack.HasValue)
                paramDaysBack = 40; //default
            DateTime? paramStopdate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stopdate").Select(s => s.DateData).FirstOrDefault();
            if (!paramStopdate.HasValue)
                paramStopdate = DateTime.Today; //default
            string paramFolder = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "folder").Select(s => s.StrData).FirstOrDefault();
            if (String.IsNullOrEmpty(paramFolder))
                paramFolder = @"c:\temp\"; //default
            if (!paramFolder.EndsWith(@"\") && !paramFolder.EndsWith(@"/"))
                paramFolder += @"\"; //default
            string paramFilenamePrefix = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "filename").Select(s => s.StrData).FirstOrDefault();
            if (String.IsNullOrEmpty(paramFilenamePrefix))
                paramFilenamePrefix = "mathem"; //default
            bool? localhost = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "localhost").Select(s => s.BoolData).FirstOrDefault();
            if (!localhost.HasValue)
                localhost = false; //default

            string fileName = "";
            string physicalPath = "";

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar QlikView jobb");

                    #region Prereq

                    bool failed = false;
                    ReportPrintout reportPrintout = null;
                    DateTime stopDate = CalendarUtility.GetBeginningOfDay(paramStopdate.Value);
                    DateTime startDate = CalendarUtility.GetEndOfDay(stopDate.AddDays(-paramDaysBack.Value));

                    Company company = paramCompanyId.HasValue ? cm.GetCompany(paramCompanyId.Value) : null;
                    if (company == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "QlikView fil kunde ej skapas, företaget hittades inte");
                        failed = true;
                    }

                    Report report = paramReportId.HasValue ? rm.GetReport(paramReportId.Value, company.ActorCompanyId, loadSysReportTemplateType: true) : null;
                    if (report == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "QlikView fil kunde ej skapas, rapporten hittades inte");
                        failed = true;
                    }

                    if (String.IsNullOrEmpty(paramFtpAddress) || String.IsNullOrEmpty(paramFtpLogin) || String.IsNullOrEmpty(paramFtpPassword))
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "QlikView fil kunde ej skapas, fpt information saknas");
                        failed = true;
                    }

                    #endregion

                    #region Create report

                    if (!failed)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, "Skapar rapport");

                        var selection = new Selection(company.ActorCompanyId, 0, 0, "", report: report.ToDTO(), isMainReport: true, exportType: (int)TermGroup_ReportExportType.File, exportFileType: (int)TermGroup_ReportExportFileType.QlikViewType1);
                        var reportItem = new TimeEmployeeReportDTO(company.ActorCompanyId, report.ReportId, report.SysReportTemplateTypeId.Value, startDate, stopDate, exportTypeId: (int)TermGroup_ReportExportType.File, exportFileTypeId: (int)TermGroup_ReportExportFileType.QlikViewType1);
                        reportItem.ShowAllEmployees = true;
                        if (selection.Evaluate(reportItem, null))
                        {                          
                            if (UseCrystalService(sm, localhost.HasValue && localhost.Value))
                            {
                                var channel = GetCrystalServiceChannel(sm);
                                string culture = Thread.CurrentThread.CurrentCulture.Name;
                                int reportPrintoutId = channel.PrintReport(selection.Evaluated, company.ActorCompanyId, 0, culture);
                                reportPrintout = rm.GetReportPrintout(reportPrintoutId, company.ActorCompanyId);
                            }
                            else
                            {
                                reportPrintout = rdm.PrintReport(selection.Evaluated);
                            }
                        }

                        if (reportPrintout == null || reportPrintout.Status == (int)TermGroup_ReportPrintoutStatus.Error)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, "QlikView fil kunde ej skapas, rapport misslyckades");
                            failed = true;
                        }
                    }

                    #endregion

                    #region Save to disc

                    if (!failed)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, "Sparar rapport till disk");

                        FileStream fileStream = null;

                        try
                        {
                            if (!Directory.Exists(paramFolder))
                                Directory.CreateDirectory(paramFolder);

                            fileName = String.Format("{0}_{1}{2}", paramFilenamePrefix, DateTime.Now.ToString("yyyyMMddhhmmss"), ".txt");
                            physicalPath = String.Format("{0}{1}", paramFolder, fileName);

                            fileStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                            fileStream.Write(reportPrintout.Data, 0, reportPrintout.Data.Length);
                        }
                        finally
                        {
                            if (fileStream != null)
                                fileStream.Close();
                        }
                    }

                    #endregion

                    #region Upload to FTP

                    if (!failed)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Flyttar fil till FTP {0}", paramFtpAddress));

                        if (FtpUtility.UploadData(paramFtpAddress, physicalPath, paramFtpLogin, paramFtpPassword))
                            CreateLogEntry(ScheduledJobLogLevel.Information, "QlikView fil uppladdad till FTP");
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Error, "QlikView fil kunde inte laddas upp till FTP");
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Fel vid exekvering av jobb: {0}. {1}", ex.Message, SoftOne.Soe.Util.Exceptions.SoeException.GetStackTrace());
                    if (ex.InnerException != null)
                        msg += "\n" + ex.InnerException.Message;
                    CreateLogEntry(ScheduledJobLogLevel.Error, msg);
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }

        private bool UseCrystalService(SettingManager sm, bool isLocalHost)
        {
            bool useWebService = sm.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseWebService, 0, 0, 0);
            return !isLocalHost && useWebService;
        }

        private ICrystalChannel GetCrystalServiceChannel(SettingManager sm)
        {
            var binding = new System.ServiceModel.BasicHttpBinding();
            binding.Name = "crystalChannel_basic";
            binding.SendTimeout = new TimeSpan(1, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);
            binding.OpenTimeout = new TimeSpan(0, 5, 0);
            binding.CloseTimeout = new TimeSpan(0, 5, 0);
            binding.MaxBufferSize = Int32.MaxValue - 1;
            binding.MaxReceivedMessageSize = Int32.MaxValue - 1;
            string address = sm.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.CrystalServiceUrl, 0, 0, 0);
            var endpoint = new System.ServiceModel.EndpointAddress(address);
            var channelFactory = new System.ServiceModel.ChannelFactory<ICrystalChannel>(binding, endpoint);
            var channel = channelFactory.CreateChannel();

            return channel;
        }
    }
}
