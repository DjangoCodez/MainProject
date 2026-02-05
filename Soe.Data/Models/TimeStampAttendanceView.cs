using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeStampAttendanceView
    {
        public string TypeName { get; set; }
        public string Name
        {
            get { return String.Format("({0}) {1} {2}", this.EmployeeNr, this.FirstName, this.LastName); }
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeStampAttendanceView

        public static TimeStampAttendanceViewDTO ToDTO(this TimeStampAttendanceView e)
        {
            return new TimeStampAttendanceViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Time = e.Time,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Type = (TimeStampEntryType)e.Type,
                TimeTerminalName = e.TimeTerminalName,
                TimeDeviationCauseName = e.TimeDeviationCauseName,
                Name = e.Name,
                AccountName = e.AccountName,
                TypeName = e.TypeName,
            };
        }

        public static IEnumerable<TimeStampAttendanceViewDTO> ToDTOs(this IEnumerable<TimeStampAttendanceView> l)
        {
            var dtos = new List<TimeStampAttendanceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeStampAttendanceGaugeDTO ToGaugeDTO(this TimeStampAttendanceView e)
        {
            return new TimeStampAttendanceGaugeDTO()
            {
                EmployeeId = e.EmployeeId,
                Time = e.Time,
                TypeName = e.TypeName,
                Type = (TimeStampEntryType)e.Type,
                IsBreak = e.IsBreak,
                Name = e.Name,
                TimeTerminalName = e.TimeTerminalName,
                TimeDeviationCauseName = e.TimeDeviationCauseName,
                AccountName = e.AccountName
            };
        }

        public static IEnumerable<TimeStampAttendanceGaugeDTO> ToGaugeDTOs(this IEnumerable<TimeStampAttendanceView> l)
        {
            var dtos = new List<TimeStampAttendanceGaugeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGaugeDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
