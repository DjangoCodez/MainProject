using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IEmployment
    {
        int EmploymentId { get; }
        int EmployeeId { get; }
        DateTime? DateFrom { get; }
        DateTime? DateTo { get; }
        int StateId { get; }
        int FinalSalaryStatusId { get; }
        bool IsSecondaryEmployment { get; }
        bool IsTemporaryPrimary { get; }
        List<DateRangeDTO> HibernatingPeriods { get; set; }
    }
}
