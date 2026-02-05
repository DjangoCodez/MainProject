using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    #region EmployeeCollectiveAgreement

    [TSInclude]
    public class EmployeeCollectiveAgreementDTO
    {
        public int EmployeeCollectiveAgreementId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int EmployeeGroupId { get; set; }
        public int? PayrollGroupId { get; set; }
        public int? VacationGroupId { get; set; }
        public int? AnnualLeaveGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeGroupName { get; set; }
        public string PayrollGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public string AnnualLeaveGroupName { get; set; }
    }

    [TSInclude]
    public class EmployeeCollectiveAgreementGridDTO
    {
        public int EmployeeCollectiveAgreementId { get; set; }
        public string EmployeeGroupName { get; set; }
        public string PayrollGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public string AnnualLeaveGroupName { get; set; }
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EmployeeTemplateNames { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion 
}
