using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.ScheduledJobs
{
    public class MigrateToEmployeeGroupTimeDeviationCausesJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            //Deprecated
        }
    }
}
