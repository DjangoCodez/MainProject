using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CleanReportPrintoutJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            var rm  = new ReportManager(parameterObject);
            var sm = new SettingManager(parameterObject);

            // Get parameters
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    var companies = base.GetCompanies(paramCompanyId);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar rensning av rapporter");

                    foreach (var company in companies)
                    {
                        int nrOfDaysToKeepReports = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CleanReportPrintoutAfterNrOfDays, 0, company.ActorCompanyId, 0);
                        if (nrOfDaysToKeepReports <= 1)
                            nrOfDaysToKeepReports = 7; //default is one week
                        if (nrOfDaysToKeepReports > 180)
                            nrOfDaysToKeepReports = 180;
                        DateTime cleanToDate = DateTime.Today.AddDays(-nrOfDaysToKeepReports);

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Rensning av rapporter för '{0}' ({1})", company.Name, company.ActorCompanyId));
                        result = rm.CleanReportPrintouts(company.ActorCompanyId, cleanToDate);
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Rensning misslyckades : {0}", result.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }


                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }    
    }
}
