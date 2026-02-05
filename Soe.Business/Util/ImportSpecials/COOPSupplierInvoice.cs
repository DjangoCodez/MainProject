using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class CoopSupplierInvoice
    {
        public string ApplyCoopFakturan(string content, string konto2442 = null)
        {

            string modifiedContent = string.Empty;
            string shopNr = " ";
            string kst = " ";
            List<XElement> voucherHeads = new List<XElement>();
            XElement voucherHeadsElement = new XElement("Verifikat");
            XElement voucherHead = null;
            string voucherDate = string.Empty;
            string voucherText = string.Empty;
            Decimal attBetala = 0;
            string fakturaNr = string.Empty;
            string kategori = string.Empty;
            Decimal radSumma = 0;
            Decimal totaltMoms = 0;
            Decimal beloppKategori = 0;
            Decimal beloppKto4050 = 0;
            Decimal beloppKto4053 = 0;
            Decimal beloppKto4111 = 0;
            Decimal beloppMoms = 0;
            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> fakturor = xml.Elements("faktura").ToList();

            foreach (XElement faktura in fakturor)
            {
                beloppKto4050 = 0;
                beloppKto4053 = 0;
                beloppKto4111 = 0;
                beloppMoms = 0;
                List<XElement> huvud = faktura.Elements("huvud").ToList();
                foreach (XElement subElement in huvud.Elements())
                {
                    if (subElement.Name.ToString().ToLower().Equals("varumottaggbregnr"))
                        shopNr = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("fakturadatum"))
                        voucherDate = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("fakturanummer"))
                        fakturaNr = "Fakturanr:" + subElement.Value;
                }
                List<XElement> rader = faktura.Elements("rad").ToList();
                foreach (XElement rad in rader)
                {
                    beloppKategori = 0;
                    foreach (XElement subElement in rad.Elements())
                    {
                        if (subElement.Name.ToString().ToLower().Equals("kategori"))
                            kategori = subElement.Value;
                        if (subElement.Name.ToString().ToLower().Equals("radbeloppdec"))
                            beloppKategori = NumberUtility.ToDecimal(subElement.Value, 2);
                        if (subElement.Name.ToString().ToLower().Equals("momsbeloppdec"))
                            beloppMoms = NumberUtility.ToDecimal(subElement.Value, 2);
                    }
                    if (beloppMoms != 0)
                    {
                        continue;
                    }
                    //Create kto
                    switch (kategori)
                    {
                        case "1225": beloppKto4111 = beloppKto4111 + beloppKategori; break;
                        case "1248": beloppKto4111 = beloppKto4111 + beloppKategori; break;
                        case "1352": beloppKto4050 = beloppKto4050 + beloppKategori; break;
                        case "1357": beloppKto4050 = beloppKto4050 + beloppKategori; break;
                        case "1358": beloppKto4050 = beloppKto4050 + beloppKategori; break;
                        case "1359": beloppKto4050 = beloppKto4050 + beloppKategori; break;
                        case "1375": beloppKto4053 = beloppKto4053 + beloppKategori; break;
                        case "4013": beloppKto4053 = beloppKto4053 + beloppKategori; break;
                    }
                }
                //Create kst
                switch (shopNr)
                {
                    case "26737": kst = "410"; break;  //Stenhamra
                    case "26711": kst = "510"; break;  //Kungsberga
                    case "196231": kst = "410"; break; //Bjursås
                    case "196235": kst = "510"; break; //Grycksbo
                    case "146475": kst = "410"; break; //Dalsjöfors
                    case "205200": kst = "410"; break; //Forsbackaexploe
                    case "76528": kst = "410"; break;  //Älghult
                    case "26731": kst = "410"; break;  //Berg
                    case "26732": kst = "510"; break;  //Långvik
                    case "26733": kst = "610"; break;  //Ingmarsö
                    case "26734": kst = "710"; break;  //Bränsleavdelning
                    case "116385": kst = "410"; break;  //Getinge
                    case "116418": kst = "410"; break;  //Knäred
                    case "58500": kst = "110"; break;  //Fastighet Stora Coop Finspång
                    case "736512": kst = "310"; break;  //Föreningsledning
                    case "54000": kst = "410"; break;  //Stora Coop Finspång
                    case "54029": kst = "420"; break;  //Cafe(Stora Coop)
                    case "54091": kst = "430"; break;  //Restaurang(Stora Coop)
                    case "54050": kst = "440"; break;  //Bageri
                    case "56301": kst = "510"; break;  //Bergslagshallen
                    case "56309": kst = "610"; break;  //Coop Rejmyre
                    case "56310": kst = "710"; break;  //Coop Skärblacka
                    case "56313": kst = "810"; break;  //Östermalmshallen
                }
                voucherText = fakturaNr + " " + shopNr;
                //Create Head
                if (!string.IsNullOrEmpty(voucherDate))
                {
                    voucherHead = new XElement("Verhuvud",
                    new XElement("Datum", voucherDate),
                    new XElement("Text", voucherText));
                }
                List<XElement> total = faktura.Elements("total").ToList();
                foreach (XElement subElement in total.Elements())
                {
                    if (subElement.Name.ToString().ToLower().Equals("attbetaladec"))
                        attBetala = NumberUtility.ToDecimal(subElement.Value, 2);
                    if (subElement.Name.ToString().ToLower().Equals("radsummadec"))
                        radSumma = NumberUtility.ToDecimal(subElement.Value, 2);
                    if (subElement.Name.ToString().ToLower().Equals("totaltmomsdec"))
                        totaltMoms = NumberUtility.ToDecimal(subElement.Value, 2);
                }

                //  Create Rows
                attBetala = (attBetala * -1);
                radSumma = (radSumma - beloppKto4050);
                radSumma = (radSumma - beloppKto4053);
                radSumma = (radSumma - beloppKto4111);
                if (konto2442 != null)
                {
                    voucherHead.Add(CreateVoucherRow("2442", voucherText, attBetala, kst));
                }
                else
                {
                    voucherHead.Add(CreateVoucherRow("1930", voucherText, attBetala, kst));
                }
                if (totaltMoms != 0)
                {
                    voucherHead.Add(CreateVoucherRow("2641", voucherText, totaltMoms, kst));
                    voucherHead.Add(CreateVoucherRow("4111", voucherText, radSumma, kst));
                }
                if (beloppKto4050 != 0)
                {
                    voucherHead.Add(CreateVoucherRow("4050", voucherText, beloppKto4050, kst));
                }
                if (beloppKto4053 != 0)
                {
                    voucherHead.Add(CreateVoucherRow("4053", voucherText, beloppKto4053, kst));
                }
                if (beloppKto4111 != 0)
                {
                    voucherHead.Add(CreateVoucherRow("4111", voucherText, beloppKto4111, kst));
                }


                if (voucherHead != null)
                    voucherHeads.Add(voucherHead);
            }

            voucherHeadsElement.Add(voucherHeads);

            modifiedContent = voucherHeadsElement.ToString();

            return modifiedContent;
        }
        private static XElement CreateVoucherRow(String account, String text, decimal amount, string kst)
        {
            XElement voucherrow = new XElement("Verrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Kst", kst),
                                new XElement("Belopp", amount));
            return voucherrow;
        }
    }
}
