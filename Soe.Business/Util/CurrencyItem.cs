namespace SoftOne.Soe.Business.Util
{
    public class CurrencyItem
    {
        public decimal BaseAmount { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal EntAmount { get; set; }
        public decimal LedgerAmount { get; set; }

        public decimal TransactionRate { get; set; }

    }
}
