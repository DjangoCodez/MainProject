namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class ResidualCodeCopyItem
    {
        public int MatchCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public int? VatAccountId { get; set; }
        public string VatAccountNr { get; set; }
        public int State { get; set; }
    }
}
