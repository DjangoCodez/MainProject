using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class PublishPreliminarySchedulesJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            // Get parameters
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            //Get companies
            List<int> companyIds = new List<int>();
            if (paramCompanyId.HasValue)
                companyIds.Add(paramCompanyId.Value);
            else
                companyIds = sm.GetCompanyIdsWithBoolInfoSetting(SystemInfoType.PublishScheduleAutomaticly_Use);

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar publicering av preliminära scheman");
                    foreach (int companyId in companyIds)
                    {
                        // Get company name for clearer logging
                        string companyName = cm.GetCompanyName(companyId);

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Publicerar scheman för '{0}' ({1})", companyName, companyId));

                        //Check setting
                        SystemInfoSetting setting = sm.GetSystemInfoSetting((int)SystemInfoType.PublishScheduleAutomaticly, companyId);
                        if (!setting.IntData.HasValue || setting.IntData.Value < 1)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Inställning för 'Dagar i förväg' är mindre än 1 dag för '{0}' ({1})", companyName, companyId));
                            continue;
                        }

                        DateTime startDate = DateTime.Today;
                        DateTime stopDate = DateTime.Today.AddDays(setting.IntData.Value);

                        //Check employees with preliminary schedules
                        List<int> employeeIds = tsm.GetEmployeeIdsWithPreliminarySchedule(startDate, stopDate, companyId);
                        if (employeeIds == null || employeeIds.Count < 1)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Inga scheman att publicera för '{0}' ({1})", companyName, companyId));
                            continue;
                        }
                        User user = um.GetAdminUser(companyId);
                        if (user == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("GetAdminUser misslyckades för '{0}' ({1})", companyName, companyId));
                            continue;
                        }
                        TimeEngineManager tem = new TimeEngineManager(parameterObject, companyId, user.UserId);
                        result = tem.SaveShiftPrelToDef(employeeIds, startDate, stopDate, true, true);
                        if (result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Publicering av scheman klar för '{0}' ({1}). Scheman för {2} anställda publicerade", companyName, companyId, employeeIds.Count));
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Publicering av scheman misslyckades för '{0}' ({1})", companyName, companyId));
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

                    string prefix = "Conversion failed. ";
                    switch (result.ErrorNumber)
                    {
                        case (int)ActionResultSave.EntityNotFound:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + "EntityNotFound [" + result.ErrorMessage + "]");
                            break;
                        case (int)ActionResultSave.Unknown:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + result.ErrorMessage);
                            break;
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
