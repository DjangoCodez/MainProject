using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
public interface IVoucherRowDTO
    {
        int? AccountDistributionHeadId { get; set; }
        List<AccountInternalDTO> AccountInternalDTO_forReports { get; set; }
        decimal Amount { get; set; }
        decimal AmountEntCurrency { get; set; }
        DateTime? Date { get; set; }
        int Dim1AmountStop { get; set; }
        int Dim1Id { get; set; }
        string Dim1Name { get; set; }
        string Dim1Nr { get; set; }
        bool Dim1UnitStop { get; set; }
        int Dim2Id { get; set; }
        string Dim2Name { get; set; }
        string Dim2Nr { get; set; }
        int Dim3Id { get; set; }
        string Dim3Name { get; set; }
        string Dim3Nr { get; set; }
        int Dim4Id { get; set; }
        string Dim4Name { get; set; }
        string Dim4Nr { get; set; }
        int Dim5Id { get; set; }
        string Dim5Name { get; set; }
        string Dim5Nr { get; set; }
        int Dim6Id { get; set; }
        string Dim6Name { get; set; }
        string Dim6Nr { get; set; }
        bool Merged { get; set; }
        int? ParentRowId { get; set; }
        decimal? Quantity { get; set; }
        int? RowNr { get; set; }
        SoeEntityState State { get; set; }
        int TempRowId { get; set; }
        string Text { get; set; }
        int VoucherHeadId { get; set; }
        long VoucherNr { get; set; }
        int VoucherRowId { get; set; }
        string VoucherSeriesTypeName { get; set; }
        int VoucherSeriesTypeNr { get; set; }
    }

    [TSInclude]
    public class VoucherRowDTO : IVoucherRowDTO
    {
        public VoucherRowDTO()
        {
            this.AccountInternalDTO_forReports = new List<AccountInternalDTO>();
        }
        public int VoucherRowId { get; set; }
        public int VoucherHeadId { get; set; }
        public int? ParentRowId { get; set; }
        public int? AccountDistributionHeadId { get; set; }

        public DateTime? Date { get; set; }
        public string Text { get; set; }
        public decimal? Quantity { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountEntCurrency { get; set; }

        public bool Merged { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public long VoucherNr { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public int TempRowId { get; set; }

        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1UnitStop { get; set; }
        public int Dim1AmountStop { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public DateTime? StartDate { get; set; }
        public int? NumberOfPeriods { get; set; }

        public int? RowNr { get; set; }

        public int? SysVatAccountId { get; set; }
        public int Dim1AccountType { get; set; }

        //For  reports
        public List<AccountInternalDTO> AccountInternalDTO_forReports { get; set; }
    }
    [TSInclude]
    public class VoucherRowIODTO
    {
        public int VoucherRowIOId { get; set; }
        public int VoucherHeadIOId { get; set; }

        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public TermGroup_IOStatus Status { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public string BatchId { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public int VoucherRowId { get; set; }
        public string Text { get; set; }
        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountSieDim1 { get; set; }
        public string AccountSieDim3 { get; set; }
        public string AccountSieDim6 { get; set; }

        public decimal? Amount { get; set; }
        public decimal? DebetAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? Quantity { get; set; }

        #region Partial comp

        public string StatusName { get; set; }

        #endregion

        #region Extensions

        public int AccountId { get; set; }
        public string AccountName { get; set; }

        public int AccountDim2Id { get; set; }
        public string AccountDim2Name { get; set; }

        public int AccountDim3Id { get; set; }
        public string AccountDim3Name { get; set; }

        public int AccountDim4Id { get; set; }
        public string AccountDim4Name { get; set; }

        public int AccountDim5Id { get; set; }
        public string AccountDim5Name { get; set; }

        public int AccountDim6Id { get; set; }
        public string AccountDim6Name { get; set; }

        #endregion
    }

    public class VoucherRowSAFTDTO
    {
        public int VoucherRowId { get; set; }
        public int VoucherHeadId { get; set; }
        public long VoucherNr { get; set; }
        public DateTime HeadDate { get; set; }
        public string HeadText { get; set; }
        public DateTime? HeadCreated { get; set; }
        public int VoucherSeriesId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public decimal Amount { get; set; }

        public int RowNr { get; set; }
        public int? SysVatAccountId { get; set; }
        public string AccountDim1Nr { get; set; }
        public string AccountDim1Name { get; set; }
    }
}
