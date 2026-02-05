using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Nalen
    {
        public string ApplyNalen(string content)
        {
            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            List<NalenVoucherRow> nalenVoucherRows = new List<NalenVoucherRow>();
            string line;
            XElement VoucherHeads = new XElement("Verifikat");

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string date = "20" + line.Substring(2, 6);
                nalenVoucherRows.Add(new NalenVoucherRow() { key = date, line = line });
            }
            int rowlength = 0;
            foreach (var voucherRows in nalenVoucherRows.GroupBy(f => f.key))
            {
                var voucherHead = new XElement("Verifikathuvud",
                                      new XElement("Vernr", string.Empty),
                                      new XElement("Verserie", string.Empty),
                                      new XElement("VerText", "Dagskassa"),
                                      new XElement("Datum", voucherRows.Key));

                foreach (var row in voucherRows)
                {
                    rowlength = row.line.Length;
                    string konto = row.line.Substring(10, 4).Trim();
                    string kst = row.line.Substring(18, 2).Trim();
                    string text = row.line.Substring(37, rowlength - 37).Trim();
                    string belopp = row.line.Substring(26, 10).Trim();
                    string tecken = row.line.Substring(20, 1).Trim();
                    decimal amount = 0;

                    decimal.TryParse(belopp.Replace('.', ','), out amount);

                    belopp = amount.ToString();

                    var voucherRow = new XElement("Verifikatrad",
                                   new XElement("Konto", konto),
                                   new XElement("Kst", kst),
                                   new XElement("Text", text),
                                   new XElement("Belopp", (tecken.Equals("-") ? "-" + belopp : belopp)));

                    voucherHead.Add(voucherRow);                   
                }
                VoucherHeads.Add(voucherHead);
            }
             return VoucherHeads.ToString();
        }
    }

    public class NalenVoucherRow
    {
        public string key { get; set; }
        public string line { get; set; }
    }
}
