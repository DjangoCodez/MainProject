using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models
{
    public class SalaryExportSchedule
    {
        public string EmployeeCode { get; set; }
        public DateTime Date { get; set; }
        public decimal ScheduleHours { get; set; }
        public SalaryExportCostAllocation CostAllocation { get; set; }
        public SalaryExportTransactionDateMergeType MergeType { get; set; }
        public List<SalaryExportSchedule> Children { get; set; } = new List<SalaryExportSchedule>();

        public SalaryExportSchedule Clone()
        {
            return new SalaryExportSchedule
            {
                Date = Date,
                ScheduleHours = ScheduleHours,
                CostAllocation = CostAllocation.Clone()
            };
        }

        public string AllocateCost(SalaryExportTransactionGroupType salaryExportTransactionGroupType, bool useCostCenter, bool useProject, bool useDepartment)
        {
            if (salaryExportTransactionGroupType != SalaryExportTransactionGroupType.ByCostAllocation || CostAllocation == null)
                return "";

            string groupCode = "";

            if (useCostCenter) groupCode += CostAllocation.Costcenter;
            if (useProject) groupCode += CostAllocation.Project;
            if (useDepartment) groupCode += CostAllocation.Department;
            return salaryExportTransactionGroupType == SalaryExportTransactionGroupType.ByCostAllocation ? groupCode : "";
        }
    }

    public class ScheduleMergeInput
    {
        public ScheduleMergeInput(SalaryExportTransactionGroupType salaryExportTransactionGroupType, SalaryExportTransactionDateMergeType salaryExportTransactionDateMergeType, List<SalaryExportSchedule> salaryExportSchedule,
            bool useCostCenter = false, bool useProject = false, bool department = false)
        {
            SalaryExportTransactionGroupType = salaryExportTransactionGroupType;
            SalaryExportTransactionDateMergeType = salaryExportTransactionDateMergeType;
            SalaryExportSchedules = salaryExportSchedule;
            this.UseCostCenter = useCostCenter;
            this.UseProject = useProject;
            this.UseDepartment = department;
        }
        public bool UseCostCenter { get; private set; }
        public bool UseProject { get; private set; }
        public bool UseDepartment { get; private set; }
        public SalaryExportTransactionGroupType SalaryExportTransactionGroupType { get; set; } = SalaryExportTransactionGroupType.ByCode;
        public SalaryExportTransactionDateMergeType SalaryExportTransactionDateMergeType { get; set; } = SalaryExportTransactionDateMergeType.Day;
        public List<SalaryExportSchedule> SalaryExportSchedules { get; set; } = new List<SalaryExportSchedule>();
    }

    public class SalaryExportScheduleGroup
    {
        public List<string> ValidationErrors { get; private set; } = new List<string>();
        public ScheduleMergeInput input { get; private set; }
        public List<SalaryExportSchedule> InitialSchedules => input.SalaryExportSchedules;
        public List<SalaryExportSchedule> MergedSchedules { get; private set; } = new List<SalaryExportSchedule>();

        public SalaryExportScheduleGroup(ScheduleMergeInput input)
        {
            this.input = input;

        }

        private bool Valid()
        {
            return ValidateDates() && ValidateOnlyOneEmployeeInGroup();
        }

        private bool ValidateDates()
        {
            var rangeStart = DateTime.Now.AddYears(-10);
            var rangeEnd = DateTime.Now.AddYears(10);
            if (!InitialSchedules.All(a => a.Date > rangeStart && a.Date < rangeEnd))
            {
                ValidationErrors.Add("Invalid dates");
                return false;
            }

            return true;
        }

        private bool ValidateOnlyOneEmployeeInGroup()
        {
            if (InitialSchedules.GroupBy(g => g.EmployeeCode).Count() != 1)
            {
                ValidationErrors.Add("More than one employee Code");
                return false;
            }

            return true;
        }

        public void MergeSchedules(SalaryExportTransactionGroupType salaryExportTransactionGroupType)
        {
            if (Valid())
            {

                switch (input.SalaryExportTransactionDateMergeType)
                {
                    case SalaryExportTransactionDateMergeType.Month:
                        var byMonth = InitialSchedules.GroupBy(g => new { m = g.Date.Month, y = g.Date.Year, c = $"{g.AllocateCost(input.SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}" });

                        foreach (var group in byMonth)
                        {
                            MergedSchedules.Add(new SalaryExportSchedule
                            {
                                Date = new DateTime(group.First().Date.Year, group.First().Date.Month, 1, 0, 0, 0, DateTimeKind.Local),
                                ScheduleHours = group.Sum(a => a.ScheduleHours),
                                CostAllocation = group.First().CostAllocation?.Clone(),
                                Children = group.ToList(),
                                MergeType = input.SalaryExportTransactionDateMergeType
                            });
                        }
                        break;
                    case SalaryExportTransactionDateMergeType.Week:
                        var byWeek = InitialSchedules.GroupBy(g => new { w = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(g.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday), y = g.Date.Year, c = $"{g.AllocateCost(input.SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}" });

                        foreach (var group in byWeek)
                        {
                            MergedSchedules.Add(new SalaryExportSchedule
                            {
                                Date = group.First().Date,
                                ScheduleHours = group.Sum(a => a.ScheduleHours),
                                CostAllocation = group.First().CostAllocation?.Clone(),
                                Children = group.ToList(),
                                MergeType = input.SalaryExportTransactionDateMergeType
                            });
                        }
                        break;
                    case SalaryExportTransactionDateMergeType.Day:
                        var byDay = InitialSchedules.GroupBy(g => new { d = g.Date, c = $"{g.AllocateCost(input.SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}" });
                        foreach (var group in byDay)
                        {
                            MergedSchedules.Add(new SalaryExportSchedule
                            {
                                Date = group.First().Date,
                                ScheduleHours = group.Sum(a => a.ScheduleHours),
                                CostAllocation = group.First().CostAllocation?.Clone(),
                                Children = group.ToList(),
                                MergeType = input.SalaryExportTransactionDateMergeType
                            });
                        }
                        break;
                }
            }
        }
    }
}
