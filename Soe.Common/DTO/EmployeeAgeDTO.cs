using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeAgeDTO
    {
        public readonly int EmployeeId;
        public readonly string Name;
        public readonly DateTime? BirthDate;
        public readonly DateTime? EmploymentEndDate;
        public bool IsMinor { get { return this.IsAgeYoungerThan18; } } //for clarity
        public bool IsAgeYoungerThan18 { get; private set; }
        public bool IsAgeBetween16To18 { get; private set; }
        public bool IsAgeBetween13To15 { get; private set; }
        public bool IsAgeYoungerThan13 { get; private set; }
        public bool CheckHanHandleMoney { get { return this.IsAgeBetween13To15 || this.IsAgeYoungerThan13; } }
        public bool CheckHandleMoneyAlone { get { return this.IsAgeBetween16To18; } }

        public EmployeeAgeDTO(int employeeId, string name, DateTime? birthDate, DateTime? employmentEndDate)
        {
            this.EmployeeId = employeeId;
            this.Name = name;
            this.BirthDate = birthDate;
            this.EmploymentEndDate = employmentEndDate;
            this.CalculateMinor();
        }

        private void CalculateMinor()
        {
            if (!this.BirthDate.HasValue)
                return;

            this.IsAgeYoungerThan18 = CalendarUtility.IsAgeYoungerThan18(this.BirthDate.Value);
            this.IsAgeBetween16To18 = CalendarUtility.IsAgeBetween16To18(this.BirthDate.Value);
            this.IsAgeBetween13To15 = CalendarUtility.IsAgeBetween13To15(this.BirthDate.Value);
            this.IsAgeYoungerThan13 = CalendarUtility.IsAgeYoungerThan13(this.BirthDate.Value);
        }

        public bool HasEmployment(DateTime date)
        {
            return !this.EmploymentEndDate.HasValue || this.EmploymentEndDate.Value >= date;
        }
    }
}
