using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ICASupplierInvoice
    {
        public string ApplyICAFakturan(string content)
        {
            //<NewDataSet>
            //   <Blad1>
            //     <Datum>20150101</Datum>
            //     <Faktnr xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">67980343</Faktnr>
            //     <Konto xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">2416</Konto>
            //     <Debet xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">26277.86</Debet>
            //     <Kredit xsi:type="xs:string" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
            //   </Blad1>
            //   <Blad1>
            //     <Faktnr xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">67980345</Faktnr>
            //     <Konto xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">2416</Konto>
            //     <Debet xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">9661.03</Debet>
            //     <Kredit xsi:type="xs:string" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
            //   </Blad1>
            //   <Blad1>

            string modifiedContent = string.Empty;
            string voucherNumber = string.Empty;
            string voucherSeries = string.Empty;
            List<XElement> voucherHeads = new List<XElement>();
            XElement voucherHeadsElement = new XElement("Verifikat");
            XElement voucherHead = null;
            string voucherDate = string.Empty;
            string previousVoucherDate = string.Empty;
            XElement xml = null;
            content = content.Replace("Kst_x0020_ställe", "Kst");

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> rows = xml.Elements("Blad1").ToList();
            
            if(rows.Count == 0)
            {
                 rows = xml.Elements("Sheet1").ToList();
            }

            foreach (XElement voucherRow in rows)
            {
                voucherDate = voucherRow.Element("Datum") != null && voucherRow.Element("Datum").Value != null ? voucherRow.Element("Datum").Value : string.Empty;

                //If new voucherDate
                if (!string.IsNullOrEmpty(voucherDate) && !string.IsNullOrEmpty(previousVoucherDate) && previousVoucherDate != voucherDate)
                {
                    voucherHeads.Add(voucherHead);
                    voucherHead = null;
                }

                //Create Head
                if (!string.IsNullOrEmpty(voucherDate))
                {
                    voucherHead = new XElement("Verhuvud",
                    new XElement("Datum", voucherDate));
                }

                //Set Date
                previousVoucherDate = voucherDate;

                //Create Row
                XElement row = new XElement("Verrad",
                    new XElement("Faktnr", voucherRow.Element("Faktnr") != null && voucherRow.Element("Faktnr").Value != null ? voucherRow.Element("Faktnr").Value : string.Empty),
                    new XElement("Text", voucherRow.Element("Text") != null && voucherRow.Element("Text").Value != null ? voucherRow.Element("Text").Value : string.Empty),
                    new XElement("Konto", voucherRow.Element("Konto") != null && voucherRow.Element("Konto").Value != null ? voucherRow.Element("Konto").Value : string.Empty),
                    new XElement("Kst", voucherRow.Element("Kst") != null && voucherRow.Element("Kst").Value != null ? voucherRow.Element("Kst").Value : string.Empty),
                    new XElement("Projekt", voucherRow.Element("Projekt") != null && voucherRow.Element("Projekt").Value != null ? voucherRow.Element("Projekt").Value : string.Empty),
                    new XElement("Debet", voucherRow.Element("Debet") != null && voucherRow.Element("Debet").Value != null ? voucherRow.Element("Debet").Value : string.Empty),
                    new XElement("Kredit", voucherRow.Element("Kredit") != null && voucherRow.Element("Kredit").Value != null ? voucherRow.Element("Kredit").Value : string.Empty));

                voucherHead.Add(row);

            }

            if (voucherHead != null)
                voucherHeads.Add(voucherHead);

            voucherHeadsElement.Add(voucherHeads);

            modifiedContent = voucherHeadsElement.ToString();

            return modifiedContent;
        }
    }
}
