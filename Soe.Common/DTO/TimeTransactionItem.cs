using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeTransactionItem : IPayrollType
    {
        #region IPayrollType implementation

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion

        #region Properties

        //Keys
        public Guid? GuidTimeBlockFK { get; set; }
        public Guid? GuidInternalPK { get; set; }
        public Guid? GuidInternalFK { get; set; }
        public Guid? GuidId { get; set; }
        public Guid? ParentGuidId { get; set; }

        public int TimeTransactionId { get; set; }
        public SoeTimeTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public string Comment { get; set; }
        public bool ManuallyAdded { get; set; }
        public bool IsAdded { get; set; }
        public bool IsFixed { get; set; }
        public bool IsReversed { get; set; }
        public bool IsCentRounding { get; set; }
        public bool IsQuantityRounding { get; set; }
        public bool IsScheduleTransaction { get; set; }
        public bool IsVacationReplacement { get; set; }
        public bool IncludedInPayrollProductChain { get; set; }
        public DateTime? ReversedDate { get; set; }
        public SoeTimePayrollScheduleTransactionType ScheduleTransactionType { get; set; }
        public int? TransactionSysPayrollTypeLevel1 { get; set; }
        public int? TransactionSysPayrollTypeLevel2 { get; set; }
        public int? TransactionSysPayrollTypeLevel3 { get; set; }
        public int? TransactionSysPayrollTypeLevel4 { get; set; }
        public bool IsQuantityOrMerchandise
        {
            get
            {
                if (this.TimeCodeRegistrationType == TermGroup_TimeCodeRegistrationType.Quantity || this.IsFixed || this.ProductVatType == TermGroup_InvoiceProductVatType.Merchandise)
                    return true;
                if (this.TransactionSysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_TableTax || this.TransactionSysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_OneTimeTax || this.TransactionSysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_SINK || this.TransactionSysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_ASINK)
                    return true;
                if (this.TransactionSysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit)
                    return true;
                if (this.TransactionSysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit)
                    return true;
                if (this.TransactionSysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit)
                    return true;
                if (this.TransactionSysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit)
                    return true;
                return false;
            }
        }
        public bool IsRounding
        {
            get
            {
                return this.IsCentRounding || this.IsQuantityRounding;
            }
        }
        public bool IsPayrollProductChainMainParent
        {
            get
            {
                return this.IncludedInPayrollProductChain && !this.ParentGuidId.HasValue;
            }
        }

        //Employee
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int? EmployeeChildId { get; set; }
        public string EmployeeChildName { get; set; }

        //Product
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public TermGroup_InvoiceProductVatType ProductVatType { get; set; }
        public int? PayrollProductSysPayrollTypeLevel1 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel2 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel3 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel4 { get; set; }

        //TimeCode
        public int TimeCodeId { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public DateTime? TimeCodeStart { get; set; }
        public DateTime? TimeCodeStop { get; set; }
        public SoeTimeCodeType TimeCodeType { get; set; }
        public TermGroup_TimeCodeRegistrationType TimeCodeRegistrationType { get; set; }

        //TimeBlock
        public int? TimeBlockId { get; set; }

        //TimeBlockDate
        public int? TimeBlockDateId { get; set; }
        public DateTime? Date { get; set; }

        //TimeRule
        public int TimeRuleId { get; set; }
        public string TimeRuleName { get; set; }
        public int? TimeRuleSort { get; set; }
        public string TimeRuleDescription
        {
            get
            {
                return this.TimeRuleId + ". " + this.TimeRuleName;
            }
        }

        //Attest
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public bool AttestStateInitial { get; set; }
        public string AttestStateColor { get; set; }
        public int AttestStateSort { get; set; }

        //SupplierInvoice
        public int SupplierInvoiceId { get; set; }

        //Accounting
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }

        #endregion

        #region Ctor

        public TimeTransactionItem()
        {
            TransactionType = SoeTimeTransactionType.Unknown;
            GuidId = Guid.NewGuid();
        }

        #endregion

        #region Public methods

        public void GetDimValues(int id, out int accountId, out string accountNr, out string accountName)
        {
            switch (id)
            {
                case 2:
                    accountId = this.Dim2Id;
                    accountNr = this.Dim2Nr;
                    accountName = this.Dim2Name;
                    break;
                case 3:
                    accountId = this.Dim3Id;
                    accountNr = this.Dim3Nr;
                    accountName = this.Dim3Name;
                    break;
                case 4:
                    accountId = this.Dim4Id;
                    accountNr = this.Dim4Nr;
                    accountName = this.Dim4Name;
                    break;
                case 5:
                    accountId = this.Dim5Id;
                    accountNr = this.Dim5Nr;
                    accountName = this.Dim5Name;
                    break;
                case 6:
                    accountId = this.Dim6Id;
                    accountNr = this.Dim6Nr;
                    accountName = this.Dim6Name;
                    break;
                default:
                    accountId = 0;
                    accountNr = "";
                    accountName = "";
                    break;
            }
        }

        #endregion
    }
}
