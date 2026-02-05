using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class RecalculateTimeHead : ICreatedModified, IState
    {
        public string StatusName { get; set; }
    }

    public partial class RecalculateTimeRecord
    {
        public string StatusName { get; set; }
        public Guid PlacementId { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region RecalculateTimeHead

        public static RecalculateTimeHeadDTO ToDTO(this RecalculateTimeHead e)
        {
            if (e == null)
                return null;

            RecalculateTimeHeadDTO dto = new RecalculateTimeHeadDTO()
            {
                RecalculateTimeHeadId = e.RecalculateTimeHeadId,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                Action = (SoeRecalculateTimeHeadAction)e.Action,
                Status = (TermGroup_RecalculateTimeHeadStatus)e.Status,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                ExcecutedStartTime = e.ExcecutedStartTime,
                ExcecutedStopTime = e.ExcecutedStopTime,
                StatusName = e.StatusName,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            if (!e.RecalculateTimeRecord.IsNullOrEmpty())
                dto.Records = e.RecalculateTimeRecord.OrderBy(r => r.Employee.EmployeeNrSort).ToDTOs();

            return dto;
        }

        public static IEnumerable<RecalculateTimeHeadDTO> ToDTOs(this IEnumerable<RecalculateTimeHead> l)
        {
            var dtos = new List<RecalculateTimeHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region RecalculateTimeRecord

        public static RecalculateTimeRecordDTO ToDTO(this RecalculateTimeRecord e)
        {
            if (e == null)
                return null;

            return new RecalculateTimeRecordDTO()
            {
                RecalculateTimeRecordId = e.RecalculateTimeRecordId,
                RecalculateTimeHeadId = e.RecalculateTimeHeadId,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee?.EmployeeNrAndName ?? string.Empty,
                RecalculateTimeRecordStatus = (TermGroup_RecalculateTimeRecordStatus)e.Status,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                ErrorMsg = e.ErrorMsg,
                WarningMsg = e.WarningMsg,
                StatusName = e.StatusName,
            };
        }

        public static List<RecalculateTimeRecordDTO> ToDTOs(this IEnumerable<RecalculateTimeRecord> l)
        {
            var dtos = new List<RecalculateTimeRecordDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static DateTime? GetScheduledStartDate(this RecalculateTimeRecord e)
        {
            if (e == null || e.TimeScheduleTemplateBlock.IsNullOrEmpty() || !e.TimeScheduleTemplateBlock.Any(i => i.Date.HasValue))
                return null;
            return e.TimeScheduleTemplateBlock.Min(i => i.Date.Value);
        }

        public static List<RecalculateTimeRecord> Filter(this IEnumerable<RecalculateTimeRecord> l, DateTime? startDate, DateTime? stopDate)
        {
            if (l.IsNullOrEmpty() || (!startDate.HasValue && !stopDate.HasValue))
                return new List<RecalculateTimeRecord>();

            return l.Where(i => CalendarUtility.IsDatesOverlappingNullable(i.StartDate, i.StopDate, startDate, stopDate)).ToList();
        }

        public static List<RecalculateTimeRecord> Filter(this IEnumerable<RecalculateTimeRecord> l, TermGroup_RecalculateTimeRecordStatus status)
        {
            return l?.Where(record => record.Status == (int)status).ToList() ?? new List<RecalculateTimeRecord>();
        }

        public static void SetStatus(this List<RecalculateTimeRecord> l, TermGroup_RecalculateTimeRecordStatus status)
        {
            if (!l.IsNullOrEmpty())
                l.ForEach(record => record.Status = (int)status);
        }

        public static bool Exists(this List<RecalculateTimeRecord> l, DateTime date)
        {
            if (l.IsNullOrEmpty())
                return false;

            return l.Any(i => i.StartDate <= date && i.StopDate >= date);
        }

        #endregion
    }
}
