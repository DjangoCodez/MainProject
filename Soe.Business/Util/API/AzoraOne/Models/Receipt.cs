using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOReceipt
    {
        public string Description { get; set; }
        public string VerificationSeries { get; set; }
        public string ReceiptDate { get; set; }
        public List<AOAccountingRow> Accounts { get; set; }
    }
}
