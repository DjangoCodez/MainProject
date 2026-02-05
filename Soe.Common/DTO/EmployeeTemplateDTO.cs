using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    #region EmployeeTemplate

    public class EmployeeTemplateDTO
    {
        public EmployeeTemplateDTO()
        {
            EmployeeTemplateGroups = new List<EmployeeTemplateGroupDTO>();
        }
        public int EmployeeTemplateId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeCollectiveAgreementId { get; set; }
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string Title { get; set; }
        public List<EmployeeTemplateGroupDTO> EmployeeTemplateGroups { get; set; }
    }

    public class EmployeeTemplateGridDTO
    {
        public int EmployeeTemplateId { get; set; }
        public string EmployeeCollectiveAgreementName { get; set; }
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }
    }

    #region Components

    public class EmployeeTemplateDisbursementAccountDTO
    {
        public int Method { get; set; }
        public string ClearingNr { get; set; }
        public string AccountNr { get; set; }
        public bool DontValidateAccountNr { get; set; }
    }

    public class EmployeeTemplateEmployeeAccountDTO
    {
        public int? AccountDimId { get; set; }
        public int? AccountId { get; set; }
        public int? ChildAccountId { get; set; }
        public int? SubChildAccountId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string DateFromString { get; set; }
        public string DateToString { get; set; }
        public bool MainAllocation { get; set; }
        public bool Default { get; set; }
    }

    public class EmployeeTemplateEmploymentPriceTypeDTO
    {
        public int PayrollPriceTypeId { get; set; }
        public string PayrollPriceTypeName { get; set; }
        public decimal PayrollGroupAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime? FromDate { get; set; }
        public string FromDateString { get; set; }
        public int? PayrollLevelId { get; set; }
    }

    public class EmployeeTemplatePositionDTO
    {
        public int PositionId { get; set; }
        public bool Default { get; set; }
    }

    #endregion

    #endregion

    #region EmployeeTemplateGroup

    public class EmployeeTemplateGroupDTO
    {
        public EmployeeTemplateGroupDTO()
        {
            EmployeeTemplateGroupRows = new List<EmployeeTemplateGroupRowDTO>();
        }
        public int EmployeeTemplateGroupId { get; set; }
        public int EmployeeTemplateId { get; set; }
        public TermGroup_EmployeeTemplateGroupType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public bool NewPageBefore { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<EmployeeTemplateGroupRowDTO> EmployeeTemplateGroupRows { get; set; }
    }

    #endregion

    #region EmployeeTemplateGroupRow

    public class EmployeeTemplateGroupRowDTO
    {
        public int EmployeeTemplateGroupRowId { get; set; }
        public int EmployeeTemplateGroupId { get; set; }
        public TermGroup_EmployeeTemplateGroupRowType Type { get; set; }
        public int MandatoryLevel { get; set; }
        public int RegistrationLevel { get; set; }
        public string Title { get; set; }
        public string DefaultValue { get; set; }
        public string Comment { get; set; }
        public int Row { get; set; }
        public int StartColumn { get; set; }
        public int SpanColumns { get; set; }
        public string Format { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool HideInReport { get; set; }
        public bool HideInReportIfEmpty { get; set; }
        public bool HideInRegistration { get; set; }
        public bool HideInEmploymentRegistration { get; set; }
        public SoeEntityType? Entity { get; set; }
        public int? RecordId { get; set; }
    }

    #endregion

    #region SaveEmployeeFromTemplate

    public class SaveEmployeeFromTemplateHeadDTO
    {
        public int EmployeeTemplateId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public bool PrintEmploymentContract { get; set; }

        public List<SaveEmployeeFromTemplateRowDTO> Rows { get; set; }

        public bool HasExtraFields
        {
            get
            {
                return this.Rows != null && this.Rows.Any(a => a.IsExtraField);
            }
        }
    }

    public class SaveEmployeeFromTemplateRowDTO
    {
        public TermGroup_EmployeeTemplateGroupRowType Type { get; set; }
        public string Value { get; set; }
        public string ExtraValue { get; set; }
        public SoeEntityType? Entity { get; set; }
        public int? RecordId { get; set; }
        public int Sort { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsExtraField
        {
            get
            {
                return Type == TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee || Type == TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount;
            }
        }
    }

    public class PrintEmploymentContractFromTemplateDTO
    {
        public int EmployeeId { get; set; }
        public int EmployeeTemplateId { get; set; }
        public List<DateTime> SubstituteDates { get; set; }
    }

    #endregion

    #region EmployeeTemplateEdit

    public class EmployeeTemplateEditDTO
    {
        public EmployeeTemplateEditDTO(int employeeId, int employmentId, string employeeNr, string socialSec, string employeeName, string title)
        {
            EmployeeTemplateEditGroups = new List<EmployeeTemplateEditGroupDTO>();
            EmployeeId = employeeId;
            EmploymentId = employmentId;
            EmployeeNr = employeeNr;
            EmployeeName = employeeName;
            Title = title;
            SocialSec= socialSec;
        }

        public int EmployeeId { get; set; }
        public int EmploymentId { get; set; }
        public string SocialSec { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string Title { get; set; }
        public List<EmployeeTemplateEditGroupDTO> EmployeeTemplateEditGroups { get; set; }

    }

    public class EmployeeTemplateEditGroupDTO
    {
        public EmployeeTemplateEditGroupDTO(EmployeeTemplateGroupDTO employeeTemplateGroup)
        {
            Type = employeeTemplateGroup.Type;
            EmployeeTemplateEditRows = new List<EmployeeTemplateEditFieldDTO>();
            EmployeeTemplateGroup = employeeTemplateGroup;
            SubstituteShiftsTuples = new List<Tuple<DateTime, bool, string, SubstituteShiftDTO>>();
        }

        public TermGroup_EmployeeTemplateGroupType Type { get; set; }
        public List<EmployeeTemplateEditFieldDTO> EmployeeTemplateEditRows { get; set; }
        public EmployeeTemplateGroupDTO EmployeeTemplateGroup { get; set; }
        public List<Tuple<DateTime, bool, string, SubstituteShiftDTO>> SubstituteShiftsTuples { get; set; }
    }

    public class EmployeeTemplateEditFieldDTO
    {
        public EmployeeTemplateEditFieldDTO(EmployeeTemplateGroupRowDTO employeeTemplateGroupRow, string value, string title, string description)
        {
            EmployeeTemplateGroupRowDTO = employeeTemplateGroupRow;
            InitialValue = value;
            CurrentValue = null;
            Title = title;
            Description = description;
            EmployeeTemplateEditFieldReportOptions = new EmployeeTemplateEditFieldReportOptions();
        }

        public EmployeeTemplateGroupRowDTO EmployeeTemplateGroupRowDTO { get; set; }
        public TermGroup_EmployeeTemplateGroupRowType Type { get { return this.EmployeeTemplateGroupRowDTO.Type; } }
        public string InitialValue { get; set; }
        public string CurrentValue { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public EmployeeTemplateEditFieldReportOptions EmployeeTemplateEditFieldReportOptions { get; set; }

        public bool CanGrow
        {
            get
            {
                return
                    Type == TermGroup_EmployeeTemplateGroupRowType.GeneralText ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.SpecialConditions;
            }
        }

        public bool IsValidBoolType
        {
            get
            {
                return
                    Type == TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.GTPExcluded ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.GeneralText ||
                    Type == TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee;
            }
        }
    }


    public class EmployeeTemplateEditFieldReportOptions
    {
        public bool Hide { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool AlignRight { get; set; }
    }

    #endregion  
}
