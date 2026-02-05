using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.RSK;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class RSKUpdateCreateProductGroupsJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            var rskManager = new RSKManager(parameterObject);
            var sysPLManager = new SysPriceListManager(parameterObject);

            int? noOfProducts = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "noofproducts").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "RSK update or create product groups job start");

                    int created = 0;
                    int updated = 0;
                    int notUpdated = 0;

                    result = rskManager.UpdateCreateSysProductGroups(ref created, ref updated, ref notUpdated);

                    CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("RSK update/create product groups job finished. {0} groups created, {1} groups updated, {2} groups not updated.", created, updated, notUpdated));
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, string.Format("Exception vid exekvering av jobb: '{0}'", result.ErrorMessage));
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
                        CreateLogEntry(ScheduledJobLogLevel.Error, string.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        break;
                }

                #endregion
            }

        }
    }
}
