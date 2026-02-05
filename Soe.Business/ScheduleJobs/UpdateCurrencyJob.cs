using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class UpdateCurrencyJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CountryCurrencyManager ccm = new CountryCurrencyManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar uppdatering av valutakurser");
                    result = ccm.SaveSysCurrencyRates(true);
                    if (result.Success)
                        CreateLogEntry(ScheduledJobLogLevel.Success, "Uppdatering av valutakurser klar: CompanyCurrencyCount=" + result.IntegerValue.ToString());
                    else
                        CreateLogEntry(ScheduledJobLogLevel.Error, "Uppdatering av valutakurser misslyckades: " + result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                if (!result.Success)
                {
                    #region Logging

                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));

                    #endregion
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
