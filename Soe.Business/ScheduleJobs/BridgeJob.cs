using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class BridgeJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            CompanyManager cm = new CompanyManager(parameterObject);
            UserManager um = new UserManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    var companyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.UseBridge);
                    var companies = cm.GetCompanies(companyIds);

                    companies = companies.OrderBy(o => o.ActorCompanyId).ToList();

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar beräkning av årsskiftesberäkning för saldon för {0} företag", companies.Count));
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
