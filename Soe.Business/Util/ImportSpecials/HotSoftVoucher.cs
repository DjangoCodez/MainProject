using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class HotSoftVoucher
    {
        public string ApplyHotSoftVoucherSpecialModification(string content, int actorCompanyId)
        {
            //Method to create voucher based on COOP:S different formats
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;

            //First we sort the file based on customer number and put them in a list

            XElement voucherHeadElement = new XElement("Verifikat");
            
            List<XElement> vouchers = new List<XElement>();
            XElement voucher = new XElement("Verifikathuvud");
  
            string konto = " ";
            string kst= " ";
            string text = " ";
            string belopp = " ";
            string voucherDate= " ";     
 
            decimal amount;
  
 
  

            bool write02 = true;


            line = reader.ReadLine();
            voucher = new XElement("Verifikathuvud");
            while (line != null)
            {

                if (line == "")
                {
                    line = reader.ReadLine();
                    continue;
                }


                voucherDate = line.Substring(13, 8);
                konto = line.Substring(63, 5);
                kst = line.Substring(68, 5);
                text = line.Substring(21, 30);
                belopp = line.Substring(51, 12);
                //amount = Convert.ToDecimal(line.Substring(51, 12));
                decimal.TryParse(line.Substring(51, 12),out amount);
                if (write02)
                {
                    voucher.Add(
                  new XElement("Namn", "Verifikat Topsoft"),
                  new XElement("Datum", voucherDate));
                    write02 = false;
                }
                voucher.Add(CreateVoucherRow(konto, text, amount, kst));
  
   
                line = reader.ReadLine();
            }
            vouchers.Add(voucher);
            voucherHeadElement.Add(vouchers);

            modifiedContent = voucherHeadElement.ToString();

         return modifiedContent;
        }


        private static XElement CreateVoucherRow(String account, String text, decimal amount, string kst)
        {
            XElement voucherrow = new XElement("Verifikatrad");
            voucherrow.Add(
                                new XElement("Konto", account.Trim()),
                                new XElement("Text", text.Trim()),
                                new XElement("Kst", kst.Trim()),
                                new XElement("Belopp", amount));
            return voucherrow;
        }
        private static XElement CreateVoucherRow(String account, String text, decimal amount, decimal quant)
        {
            XElement voucherrow = new XElement("Verifikatrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Belopp", amount),
                                new XElement("Kvantitet", quant));
            return voucherrow;
        }
    }
    
    
}
