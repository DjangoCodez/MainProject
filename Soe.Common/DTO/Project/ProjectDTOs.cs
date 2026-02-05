using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProjectDTO
    {
        public int ProjectId { get; set; }
        public TermGroup_ProjectType Type { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentProjectId { get; set; }
        public int? CustomerId { get; set; }
        public TermGroup_ProjectStatus Status { get; set; }
        public TermGroup_ProjectAllocationType AllocationType { get; set; }
        public int? InvoiceId { get; set; }
        public int? BudgetId { get; set; }

        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string Note { get; set; }
        public bool UseAccounting { get; set; }
        public int? PriceListTypeId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string WorkSiteKey { get; set; }
        public string WorkSiteNumber { get; set; }
        public int? AttestWorkFlowHeadId { get; set; }
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }

        // Extensions
        public string StatusName { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> CreditAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> DebitAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> SalesNoVatAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> SalesContractorAccounts { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }

        public BudgetHeadDTO BudgetHead { get; set; }
    }

    [TSInclude]
    public class ProjectGridDTO
    {
        public int ProjectId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public TermGroup_ProjectStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Categories { get; set; }
        public string ChildProjects { get; set; }
        public SoeEntityState State { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }

        public string DefaultDim2AccountName { get; set; }
        public string DefaultDim3AccountName { get; set; }
        public string DefaultDim4AccountName { get; set; }
        public string DefaultDim5AccountName { get; set; }
        public string DefaultDim6AccountName { get; set; }
        // Extensions
        public string ManagerName { get; set; }
        public int? ManagerUserId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public int? ParentProjectId { get; set; }

        //Flags
        public bool ProjectsWithoutCustomer { get; set; }

        public string OrderNr { get; set; }

        public bool LoadOrders { get; set; }
        public bool LoadOnlyPlannedAndActive { get; set; }
    }

    [TSInclude]
    public class ProjectSearchResultDTO
    {
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public TermGroup_ProjectStatus Status { get; set; }
        public string StatusName { get; set; }
        public string ManagerName { get; set; }
        public int? ManagerUserId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string OrderNr { get; set; }

        public List<string> OrderNbrs { get; set; }
    }

    [TSInclude]
    public class ProjectInvoiceSmallDTO
    {
        public int ProjectId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string CustomerName { get; set; }
        public string NumberName { get; set; }
    }

    [TSInclude]
    public class ProjectSmallDTO
    {
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string NumberName { get; set; }

        // Extensions
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { set; get; }
        public int? TimeCodeId { get; set; }
        public TermGroup_ProjectAllocationType AllocationType { get; set; }
        public List<int> ProjectUsers { get; set; }
        public List<int> ProjectEmployees { get; set; }
        public List<ProjectInvoiceSmallDTO> Invoices { get; set; }
    }

    [TSInclude]
    public class ProjectTinyDTO
    {
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public TermGroup_ProjectStatus Status { get; set; }
        public int? ParentProjectId { get; set; }
        public bool UseAccounting { get; set; }
    }

    [TSInclude]
    public class EmployeeProjectInvoiceDTO
    {
        public int EmployeeId { get; set; }
        public int DefaultTimeCodeId { get; set; }
        public List<ProjectSmallDTO> Projects { get; set; }
        public List<ProjectInvoiceSmallDTO> Invoices { get; set; }
    }
    [TSInclude]
    public class ProjectIODTO
    {
        #region Extensions

        public bool IsSelected { get; set; }

        [SoeGridAttribute(isReadOnly: true)]
        public string StatusName { get; set; }

        [XmlElement]
        [SoeGridAttribute(isReadOnly: true)]
        public string ErrorMessage { get; set; }

        public int ProjectId { get; set; }

        #endregion

        #region Properties
        [XmlElement]
        public string ProjectNr { get; set; }
        [XmlElement]
        public string ParentProjectNr { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public DateTime? StartDate { get; set; }
        [XmlElement]
        public DateTime? StopDate { get; set; }
        [XmlElement]
        public string AccountNr { get; set; }
        [XmlElement]
        public string AccountDim2Nr { get; set; }
        [XmlElement]
        public string AccountDim3Nr { get; set; }
        [XmlElement]
        public string AccountDim4Nr { get; set; }
        [XmlElement]
        public string AccountDim5Nr { get; set; }
        [XmlElement]
        public string AccountDim6Nr { get; set; }
        [XmlElement]
        public string AccountNrSieDim1 { get; set; }
        [XmlElement]
        public string AccountNrSieDim6 { get; set; }
        [XmlElement]
        public int ActorCompanyId { get; set; }
        [XmlElement]
        public int? AllocationType { get; set; }
        [XmlElement]
        [SoeGridAttribute(true)]
        public string BatchId { get; set; }
        [XmlElement]
        public bool? BookAccordingToThisProject { get; set; }
        [XmlElement]
        public string CategoryCode1 { get; set; }
        [XmlElement]
        public string CategoryCode2 { get; set; }
        [XmlElement]
        public string CategoryCode3 { get; set; }
        [XmlElement]
        public string CategoryCode4 { get; set; }
        [XmlElement]
        public string CategoryCode5 { get; set; }
        [XmlElement]
        public string CategoryCode6 { get; set; }
        [XmlElement]
        public string CategoryCode7 { get; set; }
        [XmlElement]
        public string CategoryCode8 { get; set; }
        [XmlElement]
        public string CategoryCode9 { get; set; }
        [XmlElement]
        public List<int> CategoryIds { get; set; }
        public string CustomerNr { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [SoeGridAttribute(true)]
        [XmlElement]
        public bool Import { get; set; }
        [XmlElement]
        public string Note { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr1 { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr2 { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr3 { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr4 { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr5 { get; set; }
        [XmlElement]
        public string ParticipantEmployeeNr6 { get; set; }
        [XmlElement]
        [SoeGridAttribute(true)]
        public int ProjectIOId { get; set; }
        [SoeGridAttribute(true)]
        [XmlElement]
        public int Source { get; set; }
        [SoeGridAttribute(true)]
        [XmlElement]
        public int State { get; set; }
        [SoeGridAttribute(true)]
        [XmlElement]
        public int Status { get; set; }
        [SoeGridAttribute(true)]
        [XmlElement]
        public int Type { get; set; }
        #endregion

        [SoeGridAttribute(true)]
        public bool IsModified { get; set; }
        [SoeGridAttribute(true)]
        public DateTime? Modified { get; set; }
        [SoeGridAttribute(true)]
        public string ModifiedBy { get; set; }
        [SoeGridAttribute(true)]
        public DateTime? Created { get; set; }
        [SoeGridAttribute(true)]
        public string CreatedBy { get; set; }
    }

    public class ProjectTotalsDTO
    {
        public int InvoiceTime { get; set; }
        public int WorkTime { get; set; }
        public int OtherTime { get; set; }
    }

    public class InvoiceDataForProjectStatisticsReportDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }

}
