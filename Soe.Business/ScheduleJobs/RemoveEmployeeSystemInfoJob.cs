using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class RemoveEmployeeSystemInfoJob : ScheduledJobBase, IScheduledJob
    {
        #region Variables

        private readonly bool useRemoveEmployeeInfoJob = false;
        private GeneralManager gm = null;

        #endregion

        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            gm = new GeneralManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    if (useRemoveEmployeeInfoJob)
                    {
                        result = gm.RemoveEmployeeInfoJob();
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"RemoveEmployeeInfoJob misslyckades: '{result.ErrorMessage}'");
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: '{result.ErrorMessage}'");
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
