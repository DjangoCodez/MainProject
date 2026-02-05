using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
 
    [TSInclude]
    public class PurchaseCartDTO
    {
        public int PurchaseCartId { get; set; }
        public int SeqNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceStrategy { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int Status { get; set; }
        public List<int> SelectedWholesellerIds { get; set; }
        public SoeEntityState State { get; set; }
        public List<PurchaseCartRowDTO> PurchaseCartRows { get; set; }
    }

    [TSInclude]
    public class ChangeCartStateModel
    {
        public List<int> Ids { get; set; }
        public TermGroup_PurchaseCartStatus StateTo { get; set; }
    }

    [TSInclude]
    public class PurchaseCartRowDTO
    {
        public int PurchaseCartRowId { get; set; }
        public int PurchaseCartId { get; set; }
        public int SysProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductInfo { get; set; }
        public string ImageUrl { get; set; }
        public int? ExternalId { get; set; }
        public int Type { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal SysPricelistHeadId { get; set; }
        public decimal WholesellerNetPriceId { get; set; }
        public decimal Quantity { get; set; }
        public int SysWholesellerId { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

    }

    [TSInclude]
    public class TransferInvoiceDTO
    {
        public int InvoiceId { get; set; }
        public int PurchaseCartId { get; set; }
    }



}