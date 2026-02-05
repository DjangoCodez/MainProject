using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class HouseholdTaxDeductionRowForFileDTO: HouseholdTaxDeductionRowDTO
    {
        public int CustomerInvoiceRowId { get; set; }
        public int InvoiceId { get; set; }
        public int? ProductId { get; set; }
        public int HouseHoldTaxDeductionType { get; set; }
        public string InvoiceNr { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public string Comment { get; set; }
    }

    [TSInclude]
    public class HouseholdTaxDeductionFileRowDTO
    {
        public int CustomerInvoiceRowId { get; set; }

        public string InvoiceNr { get; set; }
        public string Name { get; set; }
        public string SocialSecNr { get; set; }
        public string Property { get; set; }
        public string ApartmentNr { get; set; }
        public string CooperativeOrgNr { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal WorkAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal AppliedAmount { get; set; }
        public decimal NonValidAmount { get; set; }
        public string Comment { get; set; }
        public DateTime? PaidDate { get; set; }
        public TermGroup_HouseHoldTaxDeductionType HouseHoldTaxDeductionType { get; set; }

        public List<HouseholdTaxDeductionFileRowTypeDTO> Types { get; set; }
    }

    [TSInclude]
    public class HouseholdTaxDeductionFileRowTypeDTO
    {
        public int SysHouseholdTypeId { get; set; }
        public string Text { get; set; }
        public string XMLTag { get; set; }
        public decimal Hours { get; set; }
        public decimal Amount { get; set; }
    }
}
