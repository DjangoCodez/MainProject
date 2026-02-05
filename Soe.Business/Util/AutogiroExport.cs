using SoftOne.Soe.Business.Core.PaymentIO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public class AutogiroExport : AutogiroExportBase
    {
        private readonly List<string> exportRecords = new List<string>();
        private readonly int companyBg;
        private readonly int clientId;

        public AutogiroExport(int companyBg, int clientId)
        {
            this.companyBg = companyBg;
            this.clientId = clientId;
            Tk01Post tk01 = new Tk01Post();
            exportRecords.Add(tk01.ToString(companyBg, clientId));
        }

        public bool AddForExport(InvoiceExportIODTO invoice)
        {
            Tk82Post tk82 = new Tk82Post();
            exportRecords.Add(tk82.ToString(invoice, companyBg, clientId));
            return true;
        }

      

        public List<string> GenerateExportFile()
        {
            return exportRecords;
        }
    }

    public class Tk01Post : AutogiroExportBase
    {

        public string ToString(int companyBg, int clientId)
        {
            StringBuilder sb = new StringBuilder(LINE_MAX_LENGTH);
            sb.Append(TK01);
            sb.Append(DateTime.Now.ToString("yyyyMMdd"));
            sb.Append(AUTOGIRO);
            sb.Append(' ', 44);
            sb.Append(clientId.ToString("000000"));
            sb.Append(companyBg.ToString("0000000000"));
            sb.Append(' ', 2);
            return sb.ToString();

        }

    }

    public class Tk82Post : AutogiroExportBase
    {

        public string ToString(InvoiceExportIODTO item, int companyBg, int clientId)
        {
            string payerNr = String.Empty;
            if (item.BankAccount.Length > 9)
                payerNr = Utilities.PaymentNumberLong(item.PayerId);
            else
                payerNr = Utilities.PaymentNumberLong(item.BankAccount);
            DateTime now = DateTime.Now;
            DateTime dueDate = (DateTime)item.DueDate;
            int compareResult = DateTime.Compare(dueDate, now);
            if (compareResult < 0)
                dueDate = now;
            StringBuilder sb = new StringBuilder(LINE_MAX_LENGTH);
            if (item.InvoiceType == TermGroup_BillingType.Credit)
                sb.Append(TK32);
            else
                sb.Append(TK82);
            sb.Append(dueDate.ToString("yyyyMMdd"));
            sb.Append("0");
            sb.Append("    ");
            sb.Append(payerNr.AddLeadingZeros(16));
            sb.Append((Math.Abs(item.InvoiceAmount.Value) * 100).ToString("000000000000"));
            sb.Append(companyBg.ToString("0000000000"));
            sb.Append(item.InvoiceNr.PadRight(16));
            sb.Append(' ', 11);
            return sb.ToString();

        }
    }

    public class AutogiroExportBase
    {
        public const int LINE_MAX_LENGTH = 80;
        public const String TK01 = "01";
        public const String TK82 = "82";
        public const String TK32 = "32"; 
        public const String AUTOGIRO = "AUTOGIRO";

        public String GetValue(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString(CalendarUtility.SHORTDATEMASK);
            else
                return "";
        }

        public String GetValue(String value, int length)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (value.Length > length)
                    return value.Substring(0, length);
                else
                    return value;
            }
            else
                return "";
        }

        public String GetValue(decimal? value, int nrOfDec)
        {
            if (value.HasValue)
            {
                value = Math.Round(value.Value, nrOfDec);
                return value.ToString().Replace(".", ",");
            }
            else
                return "";
        }
    }

}
