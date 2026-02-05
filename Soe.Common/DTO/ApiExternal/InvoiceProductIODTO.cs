using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SoftOne.Soe.Common.DTO
{
    public class InvoiceProductSearchResultIODTO
    {
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EAN { get; set; }
    }

    [DebuggerDisplay("NumberAndName = {Number}/{Name}")]
    public class InvoiceProductIODTO
    {
        public int? ProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public bool ShowAsTextRow { get; set; }
        public string EAN { get; set; }
        public string MaterialCode { get; set; }
        public int VatType { get; set; }
        public decimal Weight { get; set; }
        
        public string VatCodeNr { get; set; }
        public string ProductGroupCode { get; set; }
        public int? ProductGroupId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public string ClaimAccountNr { get; set; }
        public string ClaimAccountDim2Nr { get; set; }
        public string ClaimAccountDim3Nr { get; set; }
        public string ClaimAccountDim4Nr { get; set; }
        public string ClaimAccountDim5Nr { get; set; }
        public string ClaimAccountDim6Nr { get; set; }
        public string ClaimAccountSieDim1 { get; set; }
        public string ClaimAccountSieDim6 { get; set; }
        public string SalesAccountNr { get; set; }
        public string SalesAccountDim2Nr { get; set; }
        public string SalesAccountDim3Nr { get; set; }
        public string SalesAccountDim4Nr { get; set; }
        public string SalesAccountDim5Nr { get; set; }
        public string SalesAccountDim6Nr { get; set; }
        public string SalesAccountSieDim1 { get; set; }
        public string SalesAccountSieDim6 { get; set; }
        public string ReversedVatSalesAccountNr { get; set; }
        public string ReversedVatSalesAccountDim2Nr { get; set; }
        public string ReversedVatSalesAccountDim3Nr { get; set; }
        public string ReversedVatSalesAccountDim4Nr { get; set; }
        public string ReversedVatSalesAccountDim5Nr { get; set; }
        public string ReversedVatSalesAccountDim6Nr { get; set; }
        public string ReversedVatSalesAccountSieDim1 { get; set; }
        public string ReversedVatSalesAccountSieDim6 { get; set; }
        public string VatFreeSalesAccountNr { get; set; }
        public string VatAccountNr { get; set; }
        public string ExtraField1 { get; set; }
        public string ExtraField2 { get; set; }
        public string ExtraField3 { get; set; }
        public string CategoryCode1 { get; set; }
        public string CategoryCode2 { get; set; }
        public string CategoryCode3 { get; set; }
        public string CategoryCode4 { get; set; }
        public string CategoryCode5 { get; set; }
        public List<int> CategoryIds { get; set; }
        public int State { get; set; }
        public List<InvoiceProductPriceIODTO> PriceDTOs { get; set; }
        public int Type { get; set; }
        public bool DontUseDiscountPercent { get; set; }
        public bool? IsStockProduct { get; set; }
        public decimal AvgPriceStockProduct { get; set; }
        public string IntrastatCode { get; set; }
        public bool External { get; set; }
    }

    public class InvoiceProductPriceIODTO
    {
        public int? PriceListTypeId { get; set; }
        public string PriceListCode { get; set; }
        public decimal Price { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime StopDate { get; set; }
        public bool InclusiveVat { get; set; }
        public decimal Quantity { get; set; }
    }

    public class InvoiceProductFilterIODTO
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public List<int> ProductGroupIds { get; set; }
        public DateTime? ModifiedSince { get; set; }
        public bool IncludeInactive { get; set; }
        public int PageNumber { get; set; }
        public int PageNrOfRecords { get; set; }

        public bool IncludePriceList { get; set; }
    }

    public class InvoiceProductSearchIODTO
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string EAN { get; set; }
        public bool External { get; set; }
        public DateTime? ModifiedSince { get; set; }
    }
}
