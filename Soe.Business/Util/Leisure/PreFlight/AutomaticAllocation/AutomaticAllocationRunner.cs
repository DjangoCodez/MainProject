using SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation
{
    public class AutomaticAllocationRunner
    {
        private readonly List<AutomaticAllocationEmployee> employees;
        private readonly List<AutomaticAllocationLeisureCode> leisureCodes;

        public AutomaticAllocationRunner(List<AutomaticAllocationEmployee> employees, List<AutomaticAllocationLeisureCode> leisureCodes)
        {
            this.employees = employees;
            this.leisureCodes = leisureCodes;
        }

        public List<AutomaticAllocationLeisureCode> GetSortedCodes()
        {
            // Sort leisure codes by type and then by required days per week
            return leisureCodes
                .Where(w => w.Type != LeisureCodeType.None)
                .OrderBy(o => o.Type)
                .ThenBy(t => t.RequiredDaysPerWeek)
                .ToList();
        }

        public AutomaticAllocationOutput Run()
        {
            // Run X -day allocation (LeisureCodeType.X) then run V -day allocation (LeisureCodeType.V)

            var sortedCodes = GetSortedCodes();

            foreach (var employee in employees)
            {
                var sortedCodesForEmployee = sortedCodes.Where(w => w.EmployeeGroupId == employee.EmployeeGroupId);
                if (sortedCodesForEmployee == null)
                    sortedCodesForEmployee = sortedCodes.Where(w => w.EmployeeGroupId == null);

                AutomaticAllocationEmployeeWeek previousWeek = null;
                foreach (var week in employee.GetAllocationEmployeeWeeksInNumberOfWorkDaysOrder())
                {
                    var usedTypes = new List<LeisureCodeType>();
                    foreach (var leisureCode in GetSortedCodes().Where(w => w.EmployeeGroupId == employee.EmployeeGroupId))
                    {
                        if (!usedTypes.Contains(leisureCode.Type))
                            week.AllocateLeisureDays(leisureCode, previousWeek?.GetMoveableUnAssignedRestDays() ?? new List<AutomaticAllocationUnAssignedRestDay>(), employees.Where(e => e.EmployeeId != employee.EmployeeId).ToList());

                        usedTypes.Add(leisureCode.Type);
                    }

                    previousWeek = week;
                }
                // still unallocated days left - add invalidation to last week
                if (previousWeek.UnAssignedRestDays.Any(d => !d.HasBeenAssigned))
                {
                    previousWeek.WeekInvalidations.Add(LeisureCodeAllocationWeekInvalidation.UnassignedAllocationsLeft);
                }
            }

            return new AutomaticAllocationOutput
            {
                Employees = employees.Select(s => s.Output).ToList()
            };
        }
    }
}
