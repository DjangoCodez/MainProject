
using Microsoft.Azure.Amqp.Framing;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    public class PaymentExportSettings
    {
        public string HeaderSignerId { get; set; }
        public string HeaderSignerName { get; set; }
        public string HeaderSignerSchemaName { get; set; } // BANK or CUST
        public bool AggregatePayments { get; set; }
        public int ForeignBank { get; set; }
        public string FileName { get; set; }
        public readonly string MessageGuid;

        public PaymentExportSettings(string messageGuid)
        {
            MessageGuid = messageGuid;
        }
    }
}
