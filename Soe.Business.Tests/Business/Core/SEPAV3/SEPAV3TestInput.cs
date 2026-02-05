namespace SoftOne.Soe.Business.Core.Tests
{
    public class SEPAV3TestInput
    {
        public int ActorCompanyId { get; set; }
        public string PaymentOriginType { get; set; }
        public int PaymentImportId { get; set; }
        public int BatchId { get; set; }
        public string ImportType { get; set; }

        /// <summary>
        /// Path to CAMT XML file to parse.
        /// </summary>
        public string XmlFile { get; set; }
    }
}
