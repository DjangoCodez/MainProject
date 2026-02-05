using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.BBS
{
    public class BBSFile
    {
        /**
         * The BBS file is a Norwegian payment file format.
         * The development was ordered by the company Safilo to handle their Norwegian payment.
         */
        public List<BBSRow> Rows { get; private set; }
        public List<BBSRow> PaymentRows => Rows.Where(r => r.IsPaymentRecord).ToList();

        public BBSFile(StreamReader sr) 
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            Rows = new List<BBSRow>();
            int index = 0;

            while (!sr.EndOfStream)
                Rows.Add(new BBSRow(index++, sr.ReadLine()));
        }
    }
}
