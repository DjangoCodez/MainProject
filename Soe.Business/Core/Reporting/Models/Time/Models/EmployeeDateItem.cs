using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeDateItem : EmploymentCalenderDTO
    {
        public EmployeeDateItem() { }

        public EmployeeDateItem(EmploymentCalenderDTO employmentCalenderDTO) : base()
        {
            EmployeeId = employmentCalenderDTO.EmployeeId;
            Date = employmentCalenderDTO.Date;
            EmploymentId = employmentCalenderDTO.EmploymentId;
            EmployeeGroupId = employmentCalenderDTO.EmployeeGroupId;
            PayrollGroupId = employmentCalenderDTO.PayrollGroupId;
            VacationGroupId = employmentCalenderDTO.VacationGroupId;
            DayTypeId = employmentCalenderDTO.DayTypeId;
            Percent = employmentCalenderDTO.Percent;
            EmployeeNr = employmentCalenderDTO.EmployeeNr;
            EmployeeName = employmentCalenderDTO.EmployeeName;
            DayTypeName = employmentCalenderDTO.DayTypeName;
        }

        public string EmployeeGroupName { get; set; }
        public string PayrollGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public decimal ScheduleTime { get; set; }
        public decimal ScheduleAbsenceTime { get; set; }
        public decimal PercentScheduleAbsenceTime { get; set; }
        public decimal FTE { get; set; }

        public string GroupOn(List<TermGroup_EmployeeDateMatrixColumns> columns)
        {
            string value = string.Empty;

            foreach (var column in columns)
            {
                switch (column)
                {
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeNr:
                        value += $"#{this.EmployeeNr}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeName:
                        value += $"#{this.EmployeeNr}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.Date:
                        value += $"#{this.Date}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.DateTypeName:
                        value += $"#{this.DayTypeName}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeGroupName:
                        value += $"#{this.EmployeeGroupId}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.PayrollGroupName:
                        value += $"#{this.PayrollGroupId}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.VacationGroupName:
                        value += $"#{this.VacationGroupId}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.EmploymentPercent:
                        value += $"#{this.Percent}";
                        break;
                    case TermGroup_EmployeeDateMatrixColumns.ScheduleTime:
                    case TermGroup_EmployeeDateMatrixColumns.ScheduleAbsenceTime:
                    case TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime:
                        break;
                    default:
                        break;
                }
            }

            return value;
        }
    }
}
