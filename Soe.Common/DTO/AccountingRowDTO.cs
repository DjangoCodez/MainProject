using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class AccountDimensionsDTO
    {
        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }
        public bool Dim1Stop { get; set; }
        public bool Dim1ManuallyChanged { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }
        public bool Dim2Stop { get; set; }
        public bool Dim2ManuallyChanged { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }
        public bool Dim3Stop { get; set; }
        public bool Dim3ManuallyChanged { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }
        public bool Dim4Stop { get; set; }
        public bool Dim4ManuallyChanged { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }
        public bool Dim5Stop { get; set; }
        public bool Dim5ManuallyChanged { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }
        public bool Dim6Stop { get; set; }
        public bool Dim6ManuallyChanged { get; set; }
    }
    [TSInclude]
    public class AccountingRowDTO : AccountDimensionsDTO
    {
        // Keys
        public AccountingRowType Type { get; set; }
        public int InvoiceRowId { get; set; }
        public int InvoiceAccountRowId { get; set; }
        public int TempRowId { get; set; }
        public int TempInvoiceRowId { get; set; }
        public int ParentRowId { get; set; }
        public int? VoucherRowId { get; set; }
        public int InvoiceId { get; set; }

        // Row
        public int RowNr { get; set; }
        public int ProductRowNr { get; set; }
        public string ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public string Text { get; set; }
        public DateTime? Date { get; set; }
        public string Unit { get; set; }
        public bool QuantityStop { get; set; }
        public bool RowTextStop { get; set; }
        public int AmountStop { get; set; }
        public DateTime? StartDate { get; set; }
        public int? NumberOfPeriods { get; set; }

        // Amounts
        public decimal DebitAmount { get; set; }
        public decimal DebitAmountCurrency { get; set; }
        public decimal DebitAmountEntCurrency { get; set; }
        public decimal DebitAmountLedgerCurrency { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal CreditAmountCurrency { get; set; }
        public decimal CreditAmountEntCurrency { get; set; }
        public decimal CreditAmountLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal Balance { get; set; }

        // Split
        public int SplitType { get; set; }
        public decimal SplitValue { get; set; }
        public decimal SplitPercent { get; set; }

        // Account distribution / inventory
        public int AccountDistributionHeadId { get; set; }
        public int AccountDistributionNbrOfPeriods { get; set; }
        public DateTime? AccountDistributionStartDate { get; set; }

        public int InventoryId { get; set; }

        // Attest
        public int AttestStatus { get; set; }
        public int? AttestUserId { get; set; }
        public string AttestUserName { get; set; }

        // Flags
        public bool IsCreditRow { get; set; }
        public bool IsDebitRow { get; set; }
        public bool IsVatRow { get; set; }
        public bool IsContractorVatRow { get; set; }
        public bool IsCentRoundingRow { get; set; }
        public bool IsInterimRow { get; set; }
        public bool IsTemplateRow { get; set; }
        public bool IsClaimRow { get; set; }
        public bool IsHouseholdRow { get; set; }
        public VoucherRowMergeType VoucherRowMergeType { get; set; }
        public int MergeSign
        {
            get
            {
                if (VoucherRowMergeType == VoucherRowMergeType.MergeDebitCredit)
                    return 0;
                else if (VoucherRowMergeType == VoucherRowMergeType.Merge)
                    return this.Amount <= 0 ? -1 : 1;

                return 0;
            }
        }

        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsManuallyAdjusted { get; set; }

        public SoeEntityState State { get; set; }
    }
}
