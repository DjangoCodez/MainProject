using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models
{
    public class InventoryItem
    {
        public string InventoryNumber { get; set; }
        public string InventoryName { get; set; }
        public string InventoryNumberName { get; set; }
        public string InventoryStatus { get; set; }
        public string InventoryDescription { get; set; }
        public string InventoryAccount { get; set; }
        public DateTime? AcquisitionDate { get; set; }
        public decimal AcquisitionValue { get; set; }
        public decimal DepreciationValue { get; set; }
        public decimal AcquisitionsForThePeriod { get; set; }
        public decimal BookValue { get; set; }
        public decimal DepreciationForThePeriod { get; set; }
        public decimal Disposals { get; set; }
        public decimal Scrapped { get; set; }
        public decimal AccumulatedDepreciationTotal { get; set; }
        public string DepriciationMethod { get; set; }
        public string InventoryCategories { get; set; }
    }
}
