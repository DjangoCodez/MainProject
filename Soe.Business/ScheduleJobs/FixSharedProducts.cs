using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class FixSharedProducts : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar fix för delade lönearter");

                    ProductManager pm = new ProductManager(parameterObject);
                    result = pm.FixSharedPayrollProducts();
                    if (!result.Success)
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Startar fix för delade lönearter misslyckades : {0}", result.ErrorMessage));
                    else
                        CreateLogEntry(ScheduledJobLogLevel.Information, result.StringValue);
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
