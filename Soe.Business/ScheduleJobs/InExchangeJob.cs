using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class InExchangeJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init
            bool releaseModeAPI = true;

            base.Init(scheduledJob, batchNr);

            #endregion

            var idm = new InvoiceDistributionManager(parameterObject);

            bool settingAPITestMode = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "testmode" && s.BoolData.HasValue).Select(s => s.BoolData.Value).FirstOrDefault();
                      
            if (settingAPITestMode)
                releaseModeAPI = false;

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    result = idm.GetAndUpdateStatusesFromInExchange(releaseModeAPI);
                    if (!result.Success)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Exception vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
            else
            {
                #region Logging

                switch (result.ErrorNumber)
                {
                    default:
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        break;
                }

                #endregion
            }

        }
    }
}
