namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class EmployeeCollectiveAgreementCopyItem
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
    }
}
