using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Data
{
    public partial class TaskWatchLog
    {
        public int DurationPerRecord
        {
            get
            {
                return Convert.ToInt32(Decimal.Divide(Decimal.Divide((decimal)Duration, (IdCount.HasValue && IdCount.Value != 0 ? IdCount.Value : 1)), (IntervalCount.HasValue && IntervalCount.Value != 0 ? IntervalCount.Value : 1)));
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region TaskWatchLog

        public static TaskWatchLog FromDTO(this TaskWatchLogDTO dto)
        {
            if (dto == null)
                return null;

            TaskWatchLog e = new TaskWatchLog()
            {
                ActorCompanyId = dto.ActorCompanyId,
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                SupportActorCompanyId = dto.SupportActorCompanyId,
                SupportUserId = dto.SupportUserId,
                SupportRoleId = dto.SupportRoleId,
                Start = dto.Start,
                Stop = dto.Stop,
                Duration = (int)dto.Duration.TotalMilliseconds,
                Batch = StringUtility.Left(dto.Batch, 50),
                Name = StringUtility.Left(dto.Name, 100),
                ClassName = StringUtility.Left(dto.ClassName, 100),
                Parameters = StringUtility.Left(dto.Parameters, 512),
                IdCount = dto.IdCount,
                IntervalCount = dto.IntervalCount,
            };

            return e;
        }

        public static void SetCompleted(this TaskWatchLog e, TaskWatchLogDTO dto)
        {
            if (e == null || dto == null)
                return;

            e.Duration = (int)dto.Duration.TotalMilliseconds;
            e.Stop = dto.Stop;
        }

        #endregion
    }
}
