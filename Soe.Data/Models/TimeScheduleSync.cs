using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleSyncBatch : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string StatusName { get; set; }
    }

    public partial class TimeScheduleSyncEntry
    {
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public string EmployeeNrSort { get; set; }
        public DateTime ScheduleStart { get; set; }
        public DateTime ScheduleStop { get; set; }
    }

    public partial class TimeScheduleSyncLeaveApplication
    {
        public string EmployeeNrSort { get; set; }
        public string StatusName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleSyncBatch

        public static TimeScheduleSyncBatchDTO ToDTO(this TimeScheduleSyncBatch e)
        {
            if (e == null)
                return null;

            return new TimeScheduleSyncBatchDTO()
            {
                TimeScheduleSyncBatchId = e.TimeScheduleSyncBatchId,
                ActorCompanyId = e.ActorCompanyId,
                Source = (TermGroup_IOSource)e.Source,
                Type = (TermGroup_TimeScheduleSyncBatchType)e.Type,
                ConditionalSync = e.ConditionalSync,
                SyncDate = e.SyncDate,
                PresenceDaysSynced = e.PresenceDaysSynced,
                AbsenceDaysSynced = e.AbsenceDaysSynced,
                LeaveApplicationsSynced = e.LeaveApplicationsSynced,
                EmployeesSynced = e.EmployeesSynced,
                Status = (TermGroup_TimeScheduleSyncBatchStatus)e.Status,
                ErrorMessage = e.ErrorMessage,
                TypeName = e.TypeName,
                StatusName = e.StatusName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<TimeScheduleSyncBatchDTO> ToDTOs(this IEnumerable<TimeScheduleSyncBatch> l)
        {
            var dtos = new List<TimeScheduleSyncBatchDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static void WriteError(this TimeScheduleSyncBatch e, string message)
        {
            if (String.IsNullOrEmpty(e.ErrorMessage))
                e.ErrorMessage = message;
            else
                e.ErrorMessage += "\n" + message;
        }

        #endregion

        #region TimeScheduleSyncEntry

        public static TimeScheduleSyncEntryDTO ToDTO(this TimeScheduleSyncEntry e)
        {
            if (e == null)
                return null;

            return new TimeScheduleSyncEntryDTO()
            {
                TimeScheduleSyncEntryId = e.TimeScheduleSyncEntryId,
                TimeScheduleSyncBatchId = e.TimeScheduleSyncBatchId,
                ScheduleId = e.ScheduleId,
                Type = (TermGroup_TimeScheduleSyncEntryType)e.Type,
                TypeName = e.TypeName,
                StatusName = e.StatusName,
                EmployeeNr = e.EmployeeNr,
                ScheduleStartDate = e.ScheduleStartDate,
                ScheduleStartTime = e.ScheduleStartTime,
                ScheduleStopDate = e.ScheduleStopDate,
                ScheduleStopTime = e.ScheduleStopTime,
                Break1Start = e.Break1Start,
                Break1Stop = e.Break1Stop,
                Break2Start = e.Break2Start,
                Break2Stop = e.Break2Stop,
                Break3Start = e.Break3Start,
                Break3Stop = e.Break3Stop,
                Break4Start = e.Break4Start,
                Break4Stop = e.Break4Stop,
                CategoryId = e.CategoryId,
                CategoryName = e.CategoryName,
                CostCentre = e.CostCentre,
                CostCentreExtCode = e.CostCentreExtCode,
                Section = e.Section,
                SectionName = e.SectionName,
                SalaryType = e.SalaryType,
                SalaryTypeExtCode = e.SalaryTypeExtCode,
                AccountNo = e.AccountNo,
                AccountNoExtCode = e.AccountNoExtCode,
                ProjectNo = e.ProjectNo,
                ProjectNoExtCode = e.ProjectNoExtCode,
                LeaveReason = e.LeaveReason,
                TimeStamp = e.TimeStamp,
                Status = (TermGroup_TimeScheduleSyncEntryStatus)e.Status,
                ErrorMessage = e.ErrorMessage,
                EmployeeNrSort = e.EmployeeNr.PadLeft(50, '0'),
                ScheduleStart = CalendarUtility.MergeDateAndTime(e.ScheduleStartDate, e.ScheduleStartTime),
                ScheduleStop = CalendarUtility.MergeDateAndTime(e.ScheduleStopDate, e.ScheduleStopTime),
            };
        }

        public static IEnumerable<TimeScheduleSyncEntryDTO> ToDTOs(this IEnumerable<TimeScheduleSyncEntry> l)
        {
            List<TimeScheduleSyncEntryDTO> dtos = new List<TimeScheduleSyncEntryDTO>();
            foreach (var e in l)
            {
                dtos.Add(e.ToDTO());
            }

            return dtos.OrderBy(t => t.TimeScheduleSyncBatchId).ThenBy(t => t.EmployeeNrSort).ThenBy(t => t.ScheduleStart);
        }

        #endregion

        #region TimeScheduleSyncLeaveApplication

        public static TimeScheduleSyncLeaveApplicationDTO ToDTO(this TimeScheduleSyncLeaveApplication e)
        {
            if (e == null)
                return null;

            return new TimeScheduleSyncLeaveApplicationDTO()
            {
                TimeScheduleSyncLeaveApplicationId = e.TimeScheduleSyncLeaveApplicationId,
                TimeScheduleSyncBatchId = e.TimeScheduleSyncBatchId,
                EmployeeNr = e.EmployeeNr,
                FromDate = CalendarUtility.MergeDateAndTime(e.FromDate, e.FromTime),
                ToDate = CalendarUtility.MergeDateAndTime(e.ToDate, e.ToTime),
                IsPreliminary = e.IsPreliminary,
                ExtCode = e.ExtCode,
                LeaveReason = e.LeaveReason,
                BodyText = e.BodyText,
                SickLevel = e.SickLevel,
                TimeStamp = e.TimeStamp,
                Status = (TermGroup_TimeScheduleSyncLeaveApplicationStatus)e.Status,
                ErrorMessage = e.ErrorMessage,
                StatusName = e.StatusName,
            };
        }

        public static IEnumerable<TimeScheduleSyncLeaveApplicationDTO> ToDTOs(this IEnumerable<TimeScheduleSyncLeaveApplication> l)
        {
            List<TimeScheduleSyncLeaveApplicationDTO> dtos = new List<TimeScheduleSyncLeaveApplicationDTO>();
            foreach (var e in l)
            {
                dtos.Add(e.ToDTO());
            }

            return dtos.OrderBy(t => t.TimeScheduleSyncBatchId).ThenBy(t => t.EmployeeNrSort).ThenBy(t => t.FromDate);
        }

        #endregion
    }
}
