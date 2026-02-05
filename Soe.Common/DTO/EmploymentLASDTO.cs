using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class EmploymentLASDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public EmploymentLASType EmploymentLASType { get; set; }
        public TermGroup_EmploymentType EmploymentType { get; set; }
        public int EmploymentTypeId { get; set; }
        public string EmploymentTypeName { get; set; }
        public string Info { get; set; }
        public int NumberOfLASDays { get; set; }
        public int NumberOfCalenderDays { get; set; }
        public bool CountOnlyOnSpecificType { get; set; }
        public int EmploymentId { get; set; }

        public bool IsFixedButNotSubstitute
        {
            get
            {
                return EmploymentType == TermGroup_EmploymentType.SE_FixedTerm || EmploymentType == TermGroup_EmploymentType.SE_SpecialFixedTerm || EmploymentType == TermGroup_EmploymentType.SE_CallContract;
            }
        }

        public override string ToString()
        {
            return $"{StartDate.ToShortDateString()} {StopDate.ToShortDateString()} {EnumUtility.GetName<EmploymentLASType>(EmploymentLASType)} {NumberOfLASDays}  {EnumUtility.GetName<TermGroup_EmploymentType>(EmploymentType)} {Info}";
        }
    }

    public enum EmploymentLASType
    {
        Unknown = 0,
        Ava = 1,
        Sva = 2,
        Vik = 3
    }
}
