using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Shared.Util
{
    public static class SharedExtensions
    {        
        #region EmployeeChangeIODTO

        public static int GetNrOfReceivedEmployees(this List<EmployeeChangeIODTO> l)
        {
            return l?.Count() ?? 0;
        }

        public static bool HasRows(this EmployeeChangeIODTO e)
        {
            return e.EmployeeChangeRowIOs != null && e.EmployeeChangeRowIOs.Any();
        }

        public static int GetNrOfCommittedEmployees(this List<EmployeeChangeIODTO> l)
        {
            return l?.Count(e => e.HasChanges && e.HasRows() && e.EmployeeChangeRowIOs.TrueForAll(r => !r.HasValidationErrors())) ?? 0;
        }

        public static int GetNrOfPartlyCommittedEmployees(this List<EmployeeChangeIODTO> l)
        {
            return l?.Count(e => e.HasChanges && e.HasRows() && e.EmployeeChangeRowIOs.Exists(r => !r.HasValidationErrors()) && e.EmployeeChangeRowIOs.Exists(r => r.HasValidationErrors())) ?? 0;
        }

        public static int GetNrOfUnCommittedEmployees(this List<EmployeeChangeIODTO> l)
        {
            return l?.Count(e => !e.HasChanges && (!e.HasRows() || e.EmployeeChangeRowIOs.TrueForAll(r => !r.HasValidationErrors()))) ?? 0;
        }

        public static bool IsNullOrEmpty(this int? value)
        {
            return !value.HasValue || value.Value == 0;
        }

        #endregion

        #region EmployeeChangeRowIODTO

        public static List<EmployeeChangeType> GetEmployeeChangeTypes(this EmployeeChangeIODTO e)
        {
            return e?.EmployeeChangeRowIOs?.Select(row => row.EmployeeChangeType).Distinct().ToList() ?? new List<EmployeeChangeType>();
        }

        public static List<EmployeeChangeRowIODTO> SortByValueAsDate(this IEnumerable<EmployeeChangeRowIODTO> l)
        {
            return l
                .Where(row => !row.Value.IsNullOrEmpty())
                .OrderBy(row => row.GetValueAsDate())
                .Concat(l.Where(row => row.Value.IsNullOrEmpty()))
                .ToList();
        }

        public static bool IsAlternatingBetweenTypes(this List<EmployeeChangeRowIODTO> l, EmployeeChangeType type1, EmployeeChangeType type2)
        {
            if (l.IsNullOrEmpty() || l.Count < 2)
                return false;

            bool startsWithType1 = l[0].EmployeeChangeType == type1;

            for (int i = 0; i < l.Count; i++)
            {
                if (startsWithType1)
                {
                    if ((i % 2 == 0 && l[i].EmployeeChangeType != type1) || (i % 2 != 0 && l[i].EmployeeChangeType != type2))
                        return false;
                }
                else
                {
                    if ((i % 2 == 0 && l[i].EmployeeChangeType != type2) || (i % 2 != 0 && l[i].EmployeeChangeType != type1))
                        return false;
                }
            }
            return true;
        }

        public static bool HasValidationErrors(this EmployeeChangeRowIODTO e)
        {
            return e.ValidationErrors != null && e.ValidationErrors.Any();
        }

        public static bool IsNewFromDate(this EmployeeChangeRowIODTO e, DateTime? newFromDate, out DateTime? fromDate)
        {
            fromDate = e.GetFromDateOrValue();
            return fromDate.HasValue && fromDate != newFromDate;
        }

        public static bool DoCreateNewEmployment(this List<EmployeeChangeRowIODTO> l, List<EmploymentDTO> employments, EmploymentDTO currentEmployment, out DateTime? newEmploymentFromDate, out DateTime? newEmploymentToDate)
        {
            newEmploymentFromDate = null;
            newEmploymentToDate = null;

            //Must be existing Employment with DateTo
            if (l.IsNullOrEmpty() || currentEmployment == null || currentEmployment.EmploymentId == 0 || !currentEmployment.DateTo.HasValue)
                return false;

            foreach (EmployeeChangeRowIODTO e in l.Where(i => i.EmployeeChangeType == EmployeeChangeType.EmploymentStartDateChange))
            {
                //Must be a future Employment that doesnt exists
                if (e.IsNewFromDate(currentEmployment.DateFrom, out DateTime? fromDate) && fromDate >= currentEmployment.DateTo && !employments.Any(emp => emp.DateFrom == fromDate && emp.State == SoeEntityState.Active))
                {
                    newEmploymentFromDate = fromDate;
                    newEmploymentToDate = e.ToDate;
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAnyType(this EmployeeChangeIODTO e, params EmployeeChangeType[] types)
        {
            return e.ContainsAnyType(types?.ToList());
        }

        public static bool ContainsAnyType(this EmployeeChangeIODTO e, IEnumerable<EmployeeChangeType> types)
        {
            if (e == null || types.IsNullOrEmpty())
                return false;
            
            return e.GetEmployeeChangeTypes().Any(employeeChangeType => types.Contains(employeeChangeType));
        }

        public static bool ContainsAnyInvalidStartDateChange(this IEnumerable<EmployeeChangeRowIODTO> l)
        {
            return l?.Any(row => row.EmployeeChangeType == EmployeeChangeType.EmploymentStartDateChange && !row.GetValueAsDate().HasValue) ?? false;
        }

        public static bool ContainsConnectingStartChange(this IEnumerable<EmployeeChangeRowIODTO> l, EmployeeChangeRowIODTO prevRow)
        {
            if (prevRow == null || !prevRow.TryParseValueAsDate(out DateTime? date, false))
                return false;
            return l?.Any(row => row.EmployeeChangeType == EmployeeChangeType.EmploymentStartDateChange && row.GetValueAsDate() == date.Value.AddDays(1)) ?? false;
        }

        public static bool IsInvalidToSaveFromEmployeeTemplateWithErrors(this EmployeeChangeRowIODTO e)
        {
            return e.EmployeeChangeType == EmployeeChangeType.SocialSec || e.EmployeeChangeType == EmployeeChangeType.DisbursementAccountNr || e.EmployeeChangeType == EmployeeChangeType.Email;
        }

        public static bool TryParseValueAsDate(this EmployeeChangeRowIODTO e, out DateTime? date, bool acceptNullOrEmpty)
        {
            date = null;
            if (e == null)
                return false;
            if (string.IsNullOrEmpty(e.Value))
                return acceptNullOrEmpty;

            if (DateTime.TryParse(e.Value, out DateTime d))
            {
                date = d.Date;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static DateTime? GetValueAsDate(this EmployeeChangeRowIODTO e)
        {
            if (TryParseValueAsDate(e, out DateTime? date, false))
                return date;
            return null;
        }

        public static DateTime? GetFromDateOrValue(this EmployeeChangeRowIODTO e)
        {
            if (e != null)
            {
                //FromDate
                if (e.FromDate.HasValue && e.FromDate.Value != DateTime.MinValue && e.FromDate.Value != CalendarUtility.DATETIME_DEFAULT)
                    return e.FromDate.Value;

                //Value
                if (DateTime.TryParse(e.Value, out DateTime fromDate))
                    return fromDate;
            }
            return null;
        }

        #endregion
    }
}
