using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeFactor : ICreatedModified, IState
    {
        public string TypeName { get; set; }

        public static EmployeeFactor Create(VacationYearEndRow vacationYearEndRow, VacationGroup vacationGroup, TermGroup_EmployeeFactorType type, DateTime fromDate, decimal factor, string createdBy, DateTime? created = null)
        {
            if (vacationYearEndRow == null || vacationGroup == null)
                return null;
            EmployeeFactor employeeFactor = Create(vacationYearEndRow.EmployeeId, vacationYearEndRow.VacationYearEndRowId, vacationGroup.VacationGroupId, null, type, fromDate, factor, createdBy, created);
            vacationYearEndRow.EmployeeFactor.Add(employeeFactor);
            return employeeFactor;
        }
        public static EmployeeFactor Create(TimeWorkAccountYearEmployee timeWorkAccountYearEmployee, TermGroup_EmployeeFactorType type, DateTime fromDate, decimal factor, string createdBy, DateTime? created = null)
        {
            if (timeWorkAccountYearEmployee == null)
                return null;
            EmployeeFactor employeeFactor = Create(timeWorkAccountYearEmployee.EmployeeId, null, null, timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId, type, fromDate, factor, createdBy, created);
            timeWorkAccountYearEmployee.EmployeeFactor.Add(employeeFactor);
            return employeeFactor;
        }
        private static EmployeeFactor Create(int employeeId, int? vacationYearEndRowId, int? vacationGroupId, int? timeWorkAccountYearEmployeeId, TermGroup_EmployeeFactorType type, DateTime fromDate, decimal factor, string createdBy, DateTime? created = null)
        {
            EmployeeFactor employeeFactor = new EmployeeFactor()
            {
                Type = (int)type,
                FromDate = fromDate,
                Factor = factor,
                State = (int)SoeEntityState.Active,

                //Set FK
                EmployeeId = employeeId,
                VacationGroupId = vacationGroupId,
                VacationYearEndRowId = vacationYearEndRowId,
                TimeWorkAccountYearEmployeeId = timeWorkAccountYearEmployeeId,
            };
            employeeFactor.SetCreated(created ?? DateTime.Now, createdBy);
            return employeeFactor;
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeFactor

        public static EmployeeFactorDTO ToDTO(this EmployeeFactor e)
        {
            if (e == null)
                return null;

            return new EmployeeFactorDTO()
            {
                EmployeeFactorId = e.EmployeeFactorId,
                Type = (TermGroup_EmployeeFactorType)e.Type,
                VacationGroupId = e.VacationGroupId,
                FromDate = e.FromDate,
                Factor = e.Factor,
                TypeName = e.TypeName,
                VacationGroupName = e.VacationGroup?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<EmployeeFactorDTO> ToDTOs(this IEnumerable<EmployeeFactor> l)
        {
            var dtos = new List<EmployeeFactorDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeFactor GetEmployeeFactor(this List<EmployeeFactor> l, DateTime date)
        {
            return l?.Where(e => !e.FromDate.HasValue || e.FromDate.Value <= date).OrderByDescending(e => e.FromDate).FirstOrDefault();
        }

        #endregion
    }
}
