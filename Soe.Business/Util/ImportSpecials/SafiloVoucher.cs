using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SoftOne.Soe.Common.Util;
using System.Xml.Linq;
using System.Globalization;


namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SafiloVoucher
    {
        private readonly CultureInfo cultureInfo = new CultureInfo("sv-SE");
        public string ApplySafiloVoucherSpecialModification(string content, int actorCompanyId)
        {
            //Method to create voucher based on Safilo format
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
 
            string kst= " ";
            string kto = " ";
            string voucherDate= " ";
            string date = " ";
            string text= " ";
            string prj= " ";
            decimal amount=0;
            int vnr = 0;
 

            line = reader.ReadLine();
            while (line != null)
            {
               
            if (line == "") continue;
 
             string[] inputRow = line.Split(delimiter);

            // bonusfil
            if (inputRow.Length == 6)
            {
                voucherDate = inputRow[0] != null ? inputRow[0] : string.Empty;
                date = CalendarUtility.GetDateTime(voucherDate).ToString();
                kto = inputRow[1] != null ? inputRow[1] : string.Empty;
                kst = inputRow[2] != null ? inputRow[2] : string.Empty;
                prj = inputRow[3] != null ? inputRow[3] : string.Empty;
                text = inputRow[4] != null ? inputRow[4] : string.Empty;
                amount = inputRow[5] != string.Empty ? Convert.ToDecimal(inputRow[5], cultureInfo) : 0;
            }
             // lönefil
            if (inputRow.Length == 5)
            {
                kto = inputRow[0] != null ? inputRow[0] : string.Empty;
                kst = inputRow[1] != null ? inputRow[1] : string.Empty;
                amount = inputRow[2] != string.Empty ? Convert.ToDecimal(inputRow[2], cultureInfo) : 0;
                voucherDate = inputRow[3] != null ? inputRow[3] : string.Empty;
                text = inputRow[4] != null ? inputRow[4] : string.Empty;
             }
            if (!string.IsNullOrEmpty(voucherDate) && vnr == 0)
            {
                voucher.Add(
                           new XElement("Namn", text),
                           new XElement("Datum", voucherDate.Trim()));
                vnr = 1;
            }
            voucher.Add(CreateVoucherRow(kto, text, amount, kst, prj));

            line = reader.ReadLine();
            }
            vouchers.Add(voucher);
            voucherHeadElement.Add(vouchers);

            modifiedContent = voucherHeadElement.ToString();

         return modifiedContent;
        }


        private static XElement CreateVoucherRow(String account, String text, decimal amount, string kst, string prj)
        {
            XElement voucherrow = new XElement("Verifikatrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Kst", kst),
                                new XElement("Prj", prj),
                                new XElement("Belopp", amount));
            return voucherrow;
        }
     }
    
    
}
