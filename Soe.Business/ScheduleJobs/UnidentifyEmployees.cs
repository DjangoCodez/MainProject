using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class UnidentifyEmployees : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            EmployeeManager em = new EmployeeManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar avidentifiering av anställda");
                    result = em.UnidentifyEmployees();

                    foreach (int actorCompanyId in result.IntDict.Keys)
                    {
                        Company company = cm.GetCompany(actorCompanyId);
                        if (company != null)
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Avidentifierat {0} anställd(a) på {1}", result.IntDict[actorCompanyId], company.Name));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
