using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.ScheduledJobs
{
    public class UpdateScheduledTimeSummaryJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);
            TimePeriodManager tpm = new TimePeriodManager(parameterObject);

            #endregion

            // Get companies to update
            List<int> companyIds = tpm.GetActorCompanyIdsWithValidRuleWorkTimePeriodSettings();


            if (companyIds.IsNullOrEmpty())
            {
                // No companies to run
                return;
            }

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    DateTime startDate = CalendarUtility.GetFirstDateOfYear(DateTime.Today);
                    DateTime stopDate = CalendarUtility.GetLastDateOfYear(DateTime.Today);

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar uppdatering av periodsammanställning för {0} företag", companyIds.Count));
                    foreach (int actorCompanyId in companyIds)
                    {
                        // Get company name for clearer logging
                        string companyName = cm.GetCompanyName(actorCompanyId);

                        // Get all employees for current company
                        List<int> employeeIds = em.GetAllEmployeeIds(actorCompanyId);
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Uppdaterar periodsammanställning för {0} anställda på '{1}' ({2})", employeeIds.Count, companyName, actorCompanyId));
                        result = tsm.UpdateScheduledTimeSummary(actorCompanyId, employeeIds, startDate, stopDate, deleteRecords: true);
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid uppdatering av '{0}': {1}", companyName, result.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                if (!result.Success)
                {
                    #region Logging

                    switch (result.ErrorNumber)
                    {
                        default:
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                            break;
                    }

                    #endregion
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
