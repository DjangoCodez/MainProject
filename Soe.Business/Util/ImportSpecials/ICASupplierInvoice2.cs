using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ICASupplierInvoice2
    {
        
        public XName getElementName(XElement xml, string localName)
        {
              return XName.Get(localName, xml.GetDefaultNamespace().ToString());
        }
   
        public string ApplyICAFakturan2(string content, int actorCompanyId, Account differenceAccount)
        {
  

            string modifiedContent = string.Empty;
 
            List<XElement> voucherHeads = new List<XElement>();
            XElement voucherHeadsElement = new XElement("Verifikat");
            string voucherNo = string.Empty;
            string CompanyCode = string.Empty;
            string voucherDate = string.Empty;
            string transDate = string.Empty;
            string voucherText = string.Empty;
            string invoiceNo = string.Empty;

            string fakturaNr = string.Empty;
            string radKonto = string.Empty;
            string transType = string.Empty;
            string kst = string.Empty;
            string category = string.Empty;

            Decimal beloppKonto = 0;
 

            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> vouchers = xml.Elements(getElementName(xml, "Voucher")).OrderBy(e => (DateTime)e.Element(getElementName(e, "VoucherDate"))).ToList();

            foreach (XElement Voucher in vouchers)
            {
                bool invoiceNbrSet = false;
                XElement voucherHead = null;

                foreach (XElement subElement in Voucher.Elements())
                {
                    /*if (subElement.Name.LocalName.ToString().Equals("VoucherNo"))
                        voucherNo = subElement.Value;*/
                    if (subElement.Name.LocalName.ToString().Equals("CompanyCode"))
                        CompanyCode = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("VoucherDate"))
                        voucherDate = subElement.Value;
                }
                
                List<XElement> Transaktions = Voucher.Elements(getElementName(xml, "Transaction")).ToList();
                foreach (XElement transaktion in Transaktions)
                {
                    transType = "";
                    voucherText = "";
                    transDate = "";
                    fakturaNr = "";
                    radKonto = "";
                    kst = " ";
                    category = " ";
                    invoiceNo = "";
                    foreach (XElement subElement in transaktion.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("TransType"))
                            transType = subElement.Value;
                        if (subElement.Name.LocalName.ToString().Equals("Description"))
                            voucherText = subElement.Value;
                        if (subElement.Name.LocalName.ToString().Equals("TransDate"))
                            transDate = subElement.Value;
                        if (subElement.Name.LocalName.ToString().Equals("ExternalRef"))
                            fakturaNr = "Fakturanr:" + subElement.Value;
                    }

                    if (!string.IsNullOrEmpty(voucherDate) && voucherHead == null)
                    {
                        voucherHead = new XElement("Verhuvud",
                        new XElement("Datum", voucherDate),
                        new XElement("Text", voucherText));
                    }

                    List<XElement> belopper = transaktion.Elements(getElementName(transaktion, "Amounts")).ToList();
                    foreach (XElement belopp in belopper)
                    {
                        beloppKonto = 0;
                        foreach (XElement subElement in belopp.Elements())
                        {
                            if(subElement.Name.LocalName.ToString().ToLower().Equals("amount"))
                                beloppKonto = NumberUtility.ToDecimal(subElement.Value, 2);
                        }
                    }
                    List<XElement> konton = transaktion.Elements(getElementName(transaktion, "GLAnalysis")).ToList();
                    foreach (XElement konto in konton)
                    {
                        foreach (XElement subElement in konto.Elements())
                        {
                            if (subElement.Name.LocalName.ToString().ToLower().Equals("account"))
                                radKonto = subElement.Value;
                            if (subElement.Name.LocalName.ToString().ToLower().Equals("dim1"))
                                kst = subElement.Value;
                            if (subElement.Name.LocalName.ToString().ToLower().Equals("dim3"))
                                category = subElement.Value;
                        }
                    }

                    if (voucherHead.Elements(getElementName(voucherHead, "Fakturanr")).Count() == 0)
                    {
                        var invoiceNoTag = Transaktions.Elements().SelectMany(x => x.Elements()).FirstOrDefault(z => z.Name.LocalName.ToLower().Equals("invoiceno") && !string.IsNullOrWhiteSpace(z.Value));
                        if (invoiceNoTag != null)
                        {
                            voucherHead.Add(new XElement("Fakturanr", invoiceNoTag.Value));
                            invoiceNbrSet = true;
                        }

                        if (!invoiceNbrSet)
                        {
                            var descriptionTag = Transaktions.SelectMany(x => x.Elements()).FirstOrDefault(z => z.Name.LocalName.ToLower().Equals("description") && !string.IsNullOrWhiteSpace(z.Value));
                            if (descriptionTag != null)
                            {
                                voucherHead.Add(new XElement("Fakturanr", descriptionTag.Value));
                            }
                        }
                    }

                    if (radKonto.IsNullOrEmpty())
                        radKonto = SetMissingAccountNr(transType, differenceAccount);

                    if (radKonto != null)
                    {
                        voucherHead.Add(CreateVoucherRow(radKonto, voucherText, beloppKonto, kst, category));
                    }
                }

                if (voucherHead != null)
                    voucherHeads.Add(voucherHead);
            }

            // sort
            /*voucherHeads = (from el in voucherHeads.Elements("Verhuvud")
                                   let date = (DateTime)el.Element("Datum")
                                   orderby date
                                   select el).ToList();*/

            voucherHeadsElement.Add(voucherHeads);

            modifiedContent = voucherHeadsElement.ToString();

            return modifiedContent;
        }
        
        private string SetMissingAccountNr(string transType, Account defaultAccount)
        {
            switch (transType)
            {
                case "AP":
                    return defaultAccount?.AccountNr ?? "2440";
                case "AR":
                    return defaultAccount?.AccountNr ?? "1510";
                default:
                    return string.Empty;
            }
        }

        private static XElement CreateVoucherRow(String account, String text, decimal amount, string kst, string category)
        {
            XElement voucherrow = new XElement("Verrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Kst", kst),
                                new XElement("Kat", category),
                                new XElement("Belopp", amount));
            return voucherrow;
        }
    }
}
