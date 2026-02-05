using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Momentum
    {
        public string ApplyMomentum(string content)
        {
            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            List<MomentumVoucherRow> momentumVoucherRows = new List<MomentumVoucherRow>();
            string line;
            string text = "";
            XElement VoucherHeads = new XElement("Verifikat");

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string date = 20 + line.Substring(0, 6);
                text = line.Substring(29, 30);

                momentumVoucherRows.Add(new MomentumVoucherRow() { mkey = date, mline = line });
            }

            foreach (var voucherRows in momentumVoucherRows.GroupBy(f => f.mkey))
            {
                var voucherHead = new XElement("Verifikathuvud",
                                      new XElement("Vernr", string.Empty),
                                      new XElement("Verserie", string.Empty),
                                      new XElement("Text", text),
                                      new XElement("Datum", voucherRows.Key));

                foreach (var row in voucherRows)
                {
                    string konto = row.mline.Substring(9, 4).Trim();
                    string kst = row.mline.Substring(15, 4).Trim();
                    string projekt = row.mline.Substring(21, 7).Trim();
                    string belopp = row.mline.Substring(59, 10).Trim();
                    decimal amount = 0;

                    decimal.TryParse(belopp, out amount);

                    belopp = decimal.Divide(amount, 100).ToString();

                    var voucherRow = new XElement("Verifikatrad",
                                   new XElement("Konto", konto),
                                   new XElement("Kst", kst),
                                   new XElement("Projekt", projekt),
                                   new XElement("Text", text),
                                   new XElement("Belopp",  belopp));
 //                   new XElement("Belopp", (tecken.Equals("-") ? "-" + belopp : belopp)));

                    voucherHead.Add(voucherRow);                   
                }
                VoucherHeads.Add(voucherHead);
            }
            var h = VoucherHeads.ToString();
            return VoucherHeads.ToString();
        }
    }

    public class MomentumVoucherRow
    {
        public string mkey { get; set; }
        public string mline { get; set; }
    }
}
