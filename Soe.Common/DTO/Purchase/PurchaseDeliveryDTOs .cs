using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PurchaseDeliveryGridDTO
    {
        public int PurchaseDeliveryId { get; set; }
        public int DeliveryNr { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime? Created { get; set; }
        public string PurchaseNr { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliveryDTO
    {
        public int PurchaseDeliveryId { get; set; }
        public int DeliveryNr { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string PurchaseNr { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliveryRowDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public int TempRowId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public int PurchaseDeliveryRowId { get; set; }
        public int PurchaseDeliveryId { get; set; }
        public int PurchaseRowId { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? PurchasePriceCurrency { get; set; }
        public decimal PurchaseQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public bool? IsLocked { get; set; }

        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string StockCode { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliverySaveDTO
    {
        public int PurchaseDeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int SupplierId { get; set; }
        public List<PurchaseDeliverySaveRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliverySaveRowDTO
    {
        public int PurchaseDeliveryRowId { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PurchasePriceCurrency { get; set; }
        public int PurchaseRowId { get; set; }
        public bool IsModified { get; set; }
        public bool SetRowAsDelivered { get; set; }
        public string PurchaseNr { get; set; }
    }
}