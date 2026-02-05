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
    public class ReCalculatePayrollDaysJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            string paramEmployeeNrs = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "employeenrs").Select(s => s.StrData).FirstOrDefault();
            int? paramUserId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "userid").Select(s => s.IntData).FirstOrDefault();
            int? paramTimePeriodId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "timeperiodid").Select(s => s.IntData).FirstOrDefault();
            DateTime? fromDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "fromdate").Select(s => s.DateData).FirstOrDefault();
            DateTime? toDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "todate").Select(s => s.DateData).FirstOrDefault();
            int? paramMode = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "mode").Select(s => s.IntData).FirstOrDefault();
            bool? paramIgnoreResultingAttestStateId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "ignore").Select(s => s.BoolData).FirstOrDefault();

            if (paramUserId == null)
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "UserId saknas");
                return;
            }

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                // Execute job
                CreateLogEntry(ScheduledJobLogLevel.Information, "Startar omräkning av semester 30000");

                try
                {
                    #region Prereq

                    DateTime limitStartDate = new DateTime(2019, 05, 01);
                    if (fromDate.HasValue)
                        limitStartDate = fromDate.Value;

                    DateTime limitStopDate = new DateTime(2020, 03, 31);
                    if (toDate.HasValue)
                        limitStopDate = toDate.Value;

                    #endregion

                    #region Perform

                    List<int> actorCompanyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.UsePayroll, true);
                    foreach (int actorCompanyId in actorCompanyIds)
                    {
                        #region Company

                        if (paramCompanyId.HasValue && paramCompanyId.Value != actorCompanyId)
                            continue;

                        parameterObject.SetSoeUser(um.GetSoeUser(actorCompanyId, scheduledJob.ExecuteUserId));
                        parameterObject.SetSoeCompany(cm.GetSoeCompany(actorCompanyId));
                        if (parameterObject.SoeCompany == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Företag {0} hittades inte", actorCompanyId));
                            continue;
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Räknar om företag {0}.{1}", parameterObject.SoeCompany.ActorCompanyId, parameterObject.SoeCompany.Name));

                        TimeEngineManager tem = new TimeEngineManager(parameterObject, parameterObject.SoeCompany.ActorCompanyId, parameterObject.SoeUser?.UserId ?? 0);
                        result = tem.ReGenerateTransactionsDiscardAttest(new List<AttestEmployeeDaySmallDTO>(), doNotRecalculateAmounts: true, vacationOnly: true, /*vacationResetLeaveOfAbsence: true,*/ vacationReset3000: true, limitStartDate: limitStartDate, limitStopDate: limitStopDate);
                            
                        foreach (var item in result.StrDict)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, item.Value);
                        }

                        if (result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Omräkning klar");
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Omräkning misslyckades {0}", result.ErrorMessage));

                        #endregion
                    }

                    #endregion
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
