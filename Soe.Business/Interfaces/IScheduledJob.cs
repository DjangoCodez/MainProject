using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface IScheduledJob
    {
        void Execute(SysScheduledJobDTO scheduledJob, int batchNr);
    }
}
