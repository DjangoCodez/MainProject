using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class UpdateAccountBalanceJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);

            #endregion

            // Get required parameters
            List<int> companyIds = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid" && s.IntData.HasValue).Select(s => s.IntData.Value).ToList();
            if (companyIds == null || companyIds.Count == 0)
            {
                // No company ids specified, get all active companies
                companyIds = cm.GetActiveCompanyIds();
            }

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar uppdatering av kontosaldon för {0} företag", companyIds.Count));
                    foreach (int actorCompanyId in companyIds)
                    {
                        // Get company name for clearer logging
                        string companyName = cm.GetCompanyName(actorCompanyId);

                        // Update account balance for all accounts in current account year
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Uppdaterar kontosaldon för '{0}' ({1})", companyName, actorCompanyId));
                        AccountBalanceManager abm = new AccountBalanceManager(null, actorCompanyId);
                        result = abm.CalculateAccountBalanceForAccounts(actorCompanyId, null);
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
