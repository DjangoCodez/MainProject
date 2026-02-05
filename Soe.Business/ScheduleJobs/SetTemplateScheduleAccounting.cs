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
    public class SetTemplateScheduleAccounting : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            var tsm = new TimeScheduleManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    var companies = base.GetCompanies(paramCompanyId);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar kontering på grundschema");

                    foreach (var company in companies)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Konterar grundschema för '{0}' ({1})", company.Name, company.ActorCompanyId));

                        List<int> blocks = tsm.GetTimeScheduleTemplateBlocksWithoutAccounting(company.ActorCompanyId);
                        tsm.SetAccountingFromShiftType(blocks, company.ActorCompanyId);
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
