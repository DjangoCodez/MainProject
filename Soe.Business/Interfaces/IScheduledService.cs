using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface IScheduledService
    {
        void ExecuteService(SysScheduledJobDTO scheduledJob);
    }
}
