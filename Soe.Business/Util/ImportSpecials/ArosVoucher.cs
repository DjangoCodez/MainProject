using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ArosVoucher
    {
        public string ArosVoucherSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            AccountManager accountManager = new AccountManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';
            var defaultCreditAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesVat, 0, actorCompanyId, 0);
            var defaultCreditaccount = accountManager.GetAccount(actorCompanyId, defaultCreditAccountId);
            var defaultNoVatAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesNoVat, 0, actorCompanyId, 0);
            var defaultNoVatAccount = accountManager.GetAccount(actorCompanyId, defaultNoVatAccountId);
            var defaultVatAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
            var defaultVatAccount = accountManager.GetAccount(actorCompanyId, defaultVatAccountId);

            XElement voucherHeadElement = new XElement("Verifikat");

            List<XElement> vouchers = new List<XElement>();
            XElement voucher = new XElement("Verifikathuvud");
            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            string line;
            bool write02 = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string[] inputRow = line.Split(delimiter);

                string konto = " ";
                string kst = " ";
                string text = " ";
                string belopp = " ";
                string voucherDate = " ";
 


                // 0 Vocherdate
                voucherDate = inputRow[0] != null ? inputRow[0].Replace(".", "") : string.Empty;
                voucherDate = voucherDate.Trim();
                //  text
                text = inputRow[2] != null ? inputRow[2].Replace(".", "") : string.Empty;
                text = text.Trim();
                //decimal amount;
                decimal temptotalBruttoAmount = 0;
                belopp = inputRow[3] != null ? inputRow[3].Replace(".", ",") : string.Empty;
                if (!String.IsNullOrWhiteSpace(belopp))
                {
                    temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp.Replace('.', ',')), 2);
                }

                //  konto
                konto = inputRow[4] != null ? inputRow[4].Replace(".", "") : string.Empty;
                konto = konto.Trim();
                //  kst
                kst = inputRow[5] != null ? inputRow[5].Replace(".", "") : string.Empty;
                kst = kst.Trim();

                if (write02)
                {
                    voucher.Add(
                    new XElement("Namn", "Verifikat Aros"),
                    new XElement("Datum", voucherDate));
                    write02 = false;
                }
                voucher.Add(CreateVoucherRow(konto, text, temptotalBruttoAmount, kst));

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

        