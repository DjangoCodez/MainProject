using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{

    public class AzureSearchJob : ScheduledJobBase, IScheduledJob
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

                    result = ssm.PopulateAzureSearch();
                    if (!result.Strings.IsNullOrEmpty())
                    {
                        foreach (var item in result.Strings)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Logg: '{0}'", item));
                        }
                    }

                    if (result.Success)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, "Azure Search job executed successfully.");
                    }
                    else
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Error: '{0}'", result.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Exception: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
