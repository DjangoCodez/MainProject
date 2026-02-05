using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.ScheduledJobs
{
    public class FlexForceSyncJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            base.Init(scheduledJob, batchNr);
            ActionResult result = CheckOutScheduledJob();
            CreateLogEntry(ScheduledJobLogLevel.Information, "Detta jobb används ej längre, ta bort!");
            CheckInScheduledJob(result.Success);
        }
    }
}
