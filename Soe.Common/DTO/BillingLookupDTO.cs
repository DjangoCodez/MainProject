using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class BillingLookupDTO
    {
        public List<PaymentConditionDTO> PaymentConditions { get; set; }
        public Dictionary<int, string> DeliveryConditionDict { get; set; }
        public Dictionary<int, string> DeliveryTypeDict { get; set; }
        public Dictionary<int, string> CustomerDict { get; set; }
        public Dictionary<int, string> OrderTypeDict { get; set; }
        public Dictionary<int, string> FixedPriceOrderTypeDict { get; set; }
        public Dictionary<int, string> VatTypeDict { get; set; }
        public List<VatCodeDTO> VatCodes { get; set; }
        public Dictionary<int, string> OurReferensDict { get; set; }
        public Dictionary<int, string> WholesellerDict { get; set; }
        public Dictionary<int, string> PriceListTypeDict { get; set; }
        public List<CompCurrencyDTO> Currencies { get; set; }
    }
}
