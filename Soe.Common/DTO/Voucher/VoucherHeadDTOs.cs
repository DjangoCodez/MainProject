using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SoftOne.Soe.Common.DTO
{
    [DebuggerDisplay("VoucherNr = {VoucherNr}")]
    [TSInclude]
    public class VoucherHeadDTO
    {
        public int VoucherHeadId { get; set; }
        public int VoucherSeriesId { get; set; }
        public int AccountPeriodId { get; set; }
        public int ActorCompanyId { get; set; }

        public long VoucherNr { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public bool Template { get; set; }
        public bool TypeBalance { get; set; }
        public bool VatVoucher { get; set; }
        public bool CompanyGroupVoucher { get; set; }
        public TermGroup_AccountStatus Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string Note { get; set; }
        public TermGroup_VoucherHeadSourceType SourceType { get; set; }

        // Extensions
        public int VoucherSeriesTypeId { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public string SourceTypeName { get; set; }
        public List<VoucherRowDTO> Rows { get; set; }
        public bool IsSelected { get; set; }

        // Used in BalanceResultSRUReport
        public List<int> AccountIds { get; set; }
        public bool AccountIdsHandled { get; set; }
        public int AccountYearId { get; set; }
        public int BudgetAccountId { get; set; }
    }
    [TSInclude]
    public class VoucherHeadIODTO
    {
        public int VoucherHeadIOId { get; set; }

        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public TermGroup_IOStatus Status { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public string BatchId { get; set; }
        public string ErrorMessage { get; set; }

        public int VoucherHeadId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherNr { get; set; }
        public string Text { get; set; }
        public string Note { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public bool? IsVatVoucher { get; set; }
        public string TransferType { get; set; }
        public int? VoucherSeriesTypeNr { get; set; }

        #region Partial comp

        public string StatusName { get; set; }

        #endregion

        #region Extensions

        public int AccountYearId { get; set; }
        public int AccountPeriodId { get; set; }
        public int VoucherSeriesId { get; set; }

        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }

        public string VoucherSeriesName { get; set; }
        public string IsVatVoucherText { get; set; }

        public List<VoucherRowIODTO> Rows { get; set; }

        #endregion
    }
    [TSInclude]
    public class VoucherGridDTO
    {
        public int VoucherHeadId { get; set; }
        public long VoucherNr { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public bool VatVoucher { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public int SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public DateTime? Modified { get; set; }
        public bool HasHistoryRows { get; set; }

        public bool IsSelected { get; set; }
        public bool HasDocuments { get; set; }
        public bool HasNoRows { get; set; }
        public bool HasUnbalancedRows { get; set; }
    }

    public class VoucherHeadFilter
    {
        public long? VoucherNrFrom { get; set; }
        public long? VoucherNrTo { get; set; }
        public int? VoucherHeadId { get; set; }
        public int? VoucherSeriesId { get; set; }
        public int? AccountYearId { get; set; }
    }
}
