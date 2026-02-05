using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ProductStockBalanceIODTO
    {
        public int ProductId { get; set; }
        public List<StockBalanceIODTO> StockBalance { get; set; }
}

    public class StockBalanceIODTO
    {
        public int StockId { get; set; }
        public string StockCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal ReservedQuantity { get; set; }
    }

    public class StockIODTO
    {
        public int StockId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
