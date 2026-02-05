using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class AccountDistribitionCopyItem
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TriggerType { get; set; }
        public int CalculationType { get; set; }
        public decimal Calculate { get; set; }
        public int PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public int Sort { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DayNumber { get; set; }
        public decimal Amount { get; set; }
        public int AmountOperator { get; set; }
        public bool KeepRow { get; set; }
        public bool UseInVoucher { get; set; }
        public bool UseInSupplierInvoice { get; set; }
        public bool UseInCustomerInvoice { get; set; }
        public bool UseInImport { get; set; }
        public bool UseInPayrollVoucher { get; set; }
        public bool UseInPayrollVacationVoucher { get; set; }
        public int? VoucherSeriesTypeId { get; set; }   

        public List<AccountDistribitionRowCopyItem> AccountDistribitionRowCopyItems { get; set; }
        public List<AccountDistributionHeadAccountDimMappingCopyItem> AccountDistributionHeadAccountDimMappingCopyItems { get; set; }
    }

    public class AccountDistributionHeadAccountDimMappingCopyItem
    {
        public int AccountDimId { get; set; }
        public string AccountExpression { get; set; }
    }

    public class AccountDistribitionRowCopyItem
    {
       public int RowNbr { get; set; }
        public int CalculateRowNbr { get; set; }
        public decimal SameBalance { get; set; }
        public decimal OppositeBalance { get; set; }
        public string Description { get; set; }
        public int? AccountId { get; set; }
        
        public List<AccountDistributionRowAccountCopyItem> AccountDistributionRowAccountCopyItems { get; set; } 
    }

    public class AccountDistributionRowAccountCopyItem
    {
        public int DimNr { get; set; }
        public int AccountId { get; set; }
        public bool KeepSourceRowAccount { get; set; }
    }
}
