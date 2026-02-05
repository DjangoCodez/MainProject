using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SysEdiImportEdiMessageHeadsJob : ScheduledJobBase, IScheduledJob
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
                    SysServiceManager ssm = new SysServiceManager(parameterObject);
                    result = ssm.ImportEdiMessageHeads();

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Error Logg: {result.ErrorMessage}" );
                    }

                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Logg: {result.InfoMessage}");
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: {result.ErrorMessage}");
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
