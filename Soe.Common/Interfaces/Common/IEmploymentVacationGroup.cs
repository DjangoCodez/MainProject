using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IEmploymentVacationGroup
    {
        DateTime? FromDate { get; set; }
        int VacationGroupId { get; set; } 
    }
}
