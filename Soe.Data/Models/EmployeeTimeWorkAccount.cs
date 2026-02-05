using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeTimeWorkAccount : ICreatedModified, IState
    {
        public DateTime DateFromValue => this.DateFrom ?? DateTime.MinValue;
        public DateTime DateToValue => this.DateTo ?? DateTime.MaxValue;
    }

    public static partial class EntityExtensions
    {
        #region EmployeeTimeWorkAccount

        public static IEnumerable<EmployeeTimeWorkAccountDTO> ToDTOs(this IEnumerable<EmployeeTimeWorkAccount> l)
        {
            var dtos = new List<EmployeeTimeWorkAccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeTimeWorkAccountDTO ToDTO(this EmployeeTimeWorkAccount e)
        {
            if (e == null)
                return null;

            return new EmployeeTimeWorkAccountDTO()
            {
                EmployeeTimeWorkAccountId = e.EmployeeTimeWorkAccountId,
                TimeWorkAccountId = e.TimeWorkAccountId,
                TimeWorkAccountName = e.TimeWorkAccount?.Name ?? string.Empty,
                EmployeeId = e.EmployeeId,
                ActorCompanyId = e.ActorCompanyId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Key = Guid.NewGuid(),
                State = (SoeEntityState)e.State,
            };
        }

        public static EmployeeTimeWorkAccount GetLatest(this IEnumerable<EmployeeTimeWorkAccount> l, DateTime date)
        {
            return l?
                .OrderByDescending(e => e.DateToValue)
                .ThenByDescending(e => e.DateFrom)
                .FirstOrDefault(e => CalendarUtility.IsDateInRange(date, e.DateFrom, e.DateTo));
        }

        public static bool IsValid(this EmployeeTimeWorkAccount e, out DateTime dateFrom, out DateTime dateTo, Employee employee, TimeWorkAccountYear year, DateTime? optionalDateFrom = null, DateTime? optionalDateTo = null)
        {
            dateFrom = CalendarUtility.DATETIME_DEFAULT;
            dateTo = CalendarUtility.DATETIME_DEFAULT;

            if (e == null || e.State != (int)SoeEntityState.Active)
                return false;
            if (year == null || year.State != (int)SoeEntityState.Active)
                return false;
            if (!CalendarUtility.GetOverlappingDates(year.EarningStart, year.EarningStop, e.DateFrom, e.DateTo, out DateTime overlappingDateFrom, out DateTime overlappingDateTo))
                return false;
            if (optionalDateFrom.HasValue && optionalDateTo.HasValue && !CalendarUtility.GetOverlappingDates(optionalDateFrom.Value, optionalDateTo.Value, overlappingDateFrom, overlappingDateTo, out overlappingDateFrom, out overlappingDateTo))
                return false;

            employee?.AdjustDatesAfterEmploymentEnd(ref overlappingDateFrom, ref overlappingDateTo);

            dateFrom = overlappingDateFrom.Date;
            dateTo = overlappingDateTo.Date;
            return dateFrom <= dateTo;
        }

        #endregion
    }
}
