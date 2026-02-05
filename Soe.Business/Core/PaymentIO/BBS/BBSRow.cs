using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.PaymentIO.BBS
{
    public static class BBSFormats
    {
        public static string DateFormat => "ddMMyy";
    }
    public class BBSRow
    {
        /**         Type                Format    Position
         *      03  FILLER              PIC X(4). 2-5
         *      03  NORGE-TRANSTYP      PIC X(2). 6-7
         *      03  NORGE-POSTTYP       PIC X(2). 8-9
         *      03  NORGE-TRANSNR       PIC 9(7). 9-15
         *      03  NORGE-SKRIVDAG      PIC 9(6). 16-21
         *      03  NORGE-BANKCENTRAL   PIC X(2). 22-23
         *      03  NORGE-DAG           PIC 9(2). 24-25
         *      03  NORGE-DEL-AVRAK     PIC 9(1). 26
         *      03  NORGE-DEL-LOPNR     PIC 9(5). 27-31
         *      03  FILLER              PIC X(1). 32
         *      03  NORGE-BELOPP        PIC 9(17).33-49
         *      03  NORGE-KIDKOD        PIC X(25).50-74
         *      03  FILLER              PIC X(6). 75-80
         */

        private int rowNr;
        private string row;
        public BBSRow(int rowNr, string row)
        {
            this.rowNr = rowNr;
            this.row = row;
        }

        //Properties
        public bool IsPaymentRecord => GetRecordType == "30";
        public string GetRecordType => row.Substring(6, 2);
        public decimal Amount => (decimal.Parse(GetPaymentAmountRaw()) / 100) * GetMultiplier();
        public string GetInvoiceType => GetKIDKOD().Substring(0, 1);
        public bool IsDebit => GetInvoiceType == "1";
        public bool IsCredit => !IsDebit;
        public string InvoiceNumber => GetInvoiceNumber();
        public DateTime PaymentDate => CalendarUtility.GetDateTime(row.Substring(15, 6), BBSFormats.DateFormat);
        
        //Helpers
        public string GetKIDKOD() => row.Substring(49, 25).Trim();
        private string GetPaymentAmountRaw() => row.Substring(32, 17);
        private string GetInvoiceNumber() {
            var kidKod = GetKIDKOD();
            // Handle dynamic length of invoice number.
            // The last character is omitted due to how the example file was structured.
            var invoiceNr = kidKod.Substring(1, kidKod.Length - 2); 
            return invoiceNr;
        }
        private int GetMultiplier() => IsCredit ? -1 : 1;
    }
}
