using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class FastNet
    {
        public string ApplyFastNet(string content)
        {
            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            List<FastNetVoucherRow> fastNetVoucherRows = new List<FastNetVoucherRow>();
            string line;
            XElement VoucherHeads = new XElement("Verifikat");

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string date = line.Substring(36, 10);
                fastNetVoucherRows.Add(new FastNetVoucherRow() { key = date, line = line });
            }

            foreach (var voucherRows in fastNetVoucherRows.GroupBy(f => f.key))
            {
                var voucherHead = new XElement("Verifikathuvud",
                                      new XElement("Vernr", string.Empty),
                                      new XElement("Verserie", string.Empty),
                                      new XElement("Datum", voucherRows.Key));

                foreach (var row in voucherRows)
                {
                    string konto = row.line.Substring(46, 4).Trim();
                    string kst = row.line.Substring(66, 3).Trim();
                    string projekt = row.line.Substring(69, 3).Trim();
                    string text = row.line.Substring(11, 25).Trim();
                    string belopp = row.line.Substring(53, 12).Trim();
                    string tecken = row.line.Substring(65, 1).Trim();
                    decimal amount = 0;

                    decimal.TryParse(belopp, out amount);

                    belopp = decimal.Divide(amount, 100).ToString();

                    var voucherRow = new XElement("Verifikatrad",
                                   new XElement("Konto", konto),
                                   new XElement("Kst", kst),
                                   new XElement("Projekt", projekt),
                                   new XElement("Text", text),
                                   new XElement("Belopp", (tecken.Equals("-") ? "-" + belopp : belopp)));

                    voucherHead.Add(voucherRow);                   
                }
                VoucherHeads.Add(voucherHead);
            }
             return VoucherHeads.ToString();
        }
    }

    public class FastNetVoucherRow
    {
        public string key { get; set; }
        public string line { get; set; }
    }
}
