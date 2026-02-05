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
    public class AutoAttestJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            #endregion

            // Get parameters
            int daysBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "daysback" && s.IntData.HasValue).Select(s => s.IntData.Value).FirstOrDefault();

            // Get companies to run auto attest on
            List<int> actorCompanyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.TimeAutoAttestRunService);
            if (actorCompanyIds.IsNullOrEmpty())
                return;

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                List<int> validActorCompanyIds = new List<int>();
                List<int> actorCompanyIdsWithSetting = sm.GetCompanyIdsWithCompanyIntSetting(CompanySettingType.TimeAutoAttestSourceAttestStateId);

                foreach (int actorCompanyId in actorCompanyIds)
                {
                    if (actorCompanyIdsWithSetting.Any(id => id == actorCompanyId))
                        validActorCompanyIds.Add(actorCompanyId);
                    else
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Attestnivå för automatattest saknas för '{0}' ({1})", cm.GetCompanyName(actorCompanyId), actorCompanyId));
                }

                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar automatattest för {0} företag", validActorCompanyIds.Count));

                    foreach (int actorCompanyId in validActorCompanyIds)
                    {
                        Company company = cm.GetCompany(actorCompanyId);
                        if (company == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Företag {0} hittades inte", actorCompanyId));
                            continue;
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Kör automatattest för företag {0}.{1}", company.ActorCompanyId, company.Name));

                        User user = um.GetAdminUser(actorCompanyId) ?? um.GetUser(scheduledJob.ExecuteUserId);
                        parameterObject.SetSoeUser(um.GetSoeUser(actorCompanyId, user));
                        parameterObject.SetSoeCompany(cm.GetSoeCompany(company));

                        TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, user?.UserId ?? 0);
                        result = tem.RunAutoAttest(em.GetAllEmployeeIds(actorCompanyId, getVacant: false), DateTime.Today.AddDays(-daysBack), DateTime.Today.AddDays(-1), autoAttestJob: true);
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid körning av automatattest: '{0}'", result.ErrorMessage));
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
