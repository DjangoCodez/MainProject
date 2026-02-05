
namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class VatCodeIODTO
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Percent { get; set; }
    }

    public class PaymentMethodIODTO
    {
        public string Name { get; set; }
        public bool UseInCashSales { get; set; }
        public bool UseRoundingInCashSales { get; set; }
        public int PaymentType { get; set; }
    }

    public class CurrencyIODTO
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
    
}
