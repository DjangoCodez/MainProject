using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeSchedule : ICreatedModified, IModifiedWithNoCheckes, IState
    {
        public bool? OverridedPreliminary { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeSchedule

        public static EmployeeScheduleDTO ToDTO(this EmployeeSchedule e)
        {
            if (e == null)
                return null;

            return new EmployeeScheduleDTO()
            {
                EmployeeScheduleId = e.EmployeeScheduleId,
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                EmployeeId = e.EmployeeId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                StartDayNumber = e.StartDayNumber,
                IsPreliminary = e.IsPreliminary,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<EmployeeScheduleDTO> ToDTOs(this IEnumerable<EmployeeSchedule> l)
        {
            var dtos = new List<EmployeeScheduleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeSchedule Get(this IEnumerable<EmployeeSchedule> l, DateTime date)
        {
            return l?.FirstOrDefault(es => es.StartDate <= date && es.StopDate >= date && es.State == (int)SoeEntityState.Active);
        }

        public static List<EmployeeSchedule> Filter(this List<EmployeeSchedule> l, DateTime startDate, DateTime? stopDate)
        {
            var validPlacements = new List<EmployeeSchedule>();

            if (l.IsNullOrEmpty())
                return validPlacements;

            if (!stopDate.HasValue)
                stopDate = DateTime.MaxValue;

            foreach (EmployeeSchedule e in l.OrderBy(i => i.StartDate))
            {
                if (CalendarUtility.IsDatesOverlapping(startDate, stopDate.Value, e.StartDate, e.StopDate))
                    validPlacements.Add(e);
            }

            return validPlacements;
        }

        public static bool IsAnEmployeeScheduleOverlapping(this List<EmployeeSchedule> l1, List<EmployeeSchedule> l2)
        {
            foreach (EmployeeSchedule e1 in l1)
            {
                foreach (EmployeeSchedule e2 in l2)
                {
                    if (CalendarUtility.IsDatesOverlapping(e1.StartDate, e1.StopDate, e2.StartDate, e2.StopDate, validateDatesAreTouching: true))
                        return true;
                }
            }

            return false;
        }

        public static bool HasOverlapping(this List<EmployeeSchedule> l)
        {
            var previous = new List<EmployeeSchedule>();
            foreach (EmployeeSchedule e in l)
            {
                foreach (EmployeeSchedule validPlacement in previous)
                {
                    if (CalendarUtility.IsDatesOverlapping(e.StartDate, e.StopDate, validPlacement.StartDate, validPlacement.StopDate))
                        return true;
                }

                previous.Add(e);
            }

            return false;
        }

        public static void UpdateStart(this EmployeeSchedule e, DateTime newStart)
        {
            if (e != null && newStart <= e.StopDate)
                e.StartDate = newStart;
        }

        public static void UpdateStop(this EmployeeSchedule e, DateTime newStop)
        {
            if (e != null && newStop >= e.StartDate)
                e.StopDate = newStop;
        }

        #endregion
    }
}
