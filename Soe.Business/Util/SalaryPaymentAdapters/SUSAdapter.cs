using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    class SUSAdapter: SalaryPaymentBaseAdapter
    {
        #region Variables

        private decimal totalNetAmount = 0;
        readonly private string senderContractNr;
        readonly private string senderOrgNr;                
        readonly private bool isSenderRegisterHolder = false;

        #endregion

        public SUSAdapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod, bool isSenderRegisterHolder, string senderContractNr, string senderOrgNr)
            : base(employees, transactions, timePeriod)
        {
            base.ExportFormat = SoeTimeSalaryPaymentExportFormat.Text;
            base.ExportType = TermGroup_TimeSalaryPaymentExportType.SUS;

            this.senderContractNr = senderContractNr.Trim().Replace("-", "");
            this.senderOrgNr = senderOrgNr.Trim().Replace("-", "");            
            this.isSenderRegisterHolder = isSenderRegisterHolder;

            if (this.senderContractNr.Length > 6)
                this.senderContractNr = this.senderContractNr.Left(6);

            if (this.senderOrgNr.Length > 10)
                this.senderOrgNr = this.senderOrgNr.Left(10);
        }

        public override byte[] CreateFile()
        {
            string doc = string.Empty;
            doc = CreateOpeningEntry();
            doc += CreatePaymentEntries();
            doc += CreateSumPaymentEntry();
            
            return Encoding.GetEncoding(1252).GetBytes(doc);

            //Godkända format (inget av dessa fungerar nu)
            // - ISO-8859-1  
            // - Codepage 850
            // - UniCode (UTF-16)
            // - UniCode LittleEndian (UTF-16LE)
            // https://www.swedbank.se/idc/demo/demo_e-cm/demo_foretag/betala_overfora/filoverforing_skicka_ny_fil.html

        }

        private string CreateOpeningEntry()
        {
            StringBuilder sb = new StringBuilder();
            
            string senderContracrNrReReport = senderContractNr;
            if(senderContracrNrReReport.Length > 5)
                senderContracrNrReReport = senderContracrNrReReport.Left(5);

            sb.Append("05");                                                //1-2
            sb.Append(senderContractNr.AddLeadingZeros(6));                 //3-8
            sb.Append(' ', 50);                                             //9-58
            sb.Append(base.GetPaymentDateYYYYMMDD());                       //59-66
            sb.Append(senderContracrNrReReport.AddLeadingZeros(5));         //67-71
            sb.Append(senderOrgNr.AddLeadingZeros(10));                     //72-71
            sb.Append(' ', 99);                                             //82-180

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        private string CreatePaymentEntries()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var employeeItem in base.EmployeeItems)
            {
                if (employeeItem.IsSE_CashDeposit)
                    continue;

                if (employeeItem.IsZeroNetAmount)
                    continue;

                #region NetAmount

                decimal netAmount = decimal.Round(employeeItem.GetNetAmount(), 2, MidpointRounding.AwayFromZero); //Amount is already rounded to 2 decimals, but to bee sure...
                string netAmountStr = netAmount.ToString().Replace(".", "").Replace(",", "");
                totalNetAmount += netAmount;

                #endregion

                #region PaymentId
                
                string paymentId = employeeItem.GetSocialSec(base.GetExportType()) + DateTime.Now.ToShortDateShortTimeString();
                paymentId = paymentId.Replace("-", "").Replace(":", "").Replace(" ", "").Trim();

                #endregion

                #region ClearingNr/AccountNr/AccountCode

                string clearingNr = string.Empty;
                string accountNr = string.Empty;
                string accountCode = string.Empty;

                if (this.isSenderRegisterHolder)
                {
                    clearingNr = employeeItem.GetClearingNr();
                    accountNr = employeeItem.GetAccountNr();
                    
                    if (clearingNr.Length > 5)
                        clearingNr = clearingNr.Left(5);

                    if (accountNr.Length > 10)
                        accountNr = accountNr.Left(10);

                    if (employeeItem.IsSE_PersonAccount)
                    {
                        accountCode = "PK"; 
                        clearingNr = "00000"; //clearingNr is not used when accountCode is used                        
                    }
                    else
                    {
                        accountCode = "  ";
                    }
                }
                else
                {
                    clearingNr = "00000";
                    accountNr = "0000000000";
                    accountCode = "  ";
                }

                employeeItem.FormattedRecieverAccountNr = clearingNr + accountNr;

                #endregion

                sb.Append("30");                                                                        //1-2
                sb.Append(senderContractNr.AddLeadingZeros(6));                                         //3-8
                sb.Append(paymentId.AddTrailingBlanks(44));                                             //9-52
                sb.Append('0', 6);                                                                      //53-58
                sb.Append(' ', 3);                                                                      //59-61
                sb.Append(employeeItem.GetSocialSec(base.GetExportType()).AddTrailingBlanks(12));       //62-73 
                sb.Append(employeeItem.Name.AddTrailingBlanks(36));                                     //74-109
                sb.Append(' ', 10);                                                                     //110-119
                sb.Append(' ', 1);                                                                      //120
                sb.Append("01"); //01=LÖN                                                               //121-122
                sb.Append(clearingNr.AddLeadingZeros(5));                                               //123-127
                sb.Append(accountNr.AddLeadingZeros(10));                                               //128-137
                sb.Append(accountCode);                                                                 //138-139
                sb.Append('0', 2);                                                                      //140-141
                sb.Append(netAmountStr.AddLeadingZeros(15));                                            //142-156
                sb.Append('0', 15);                                                                     //157-171
                sb.Append(' ', 6);                                                                      //172-177
                sb.Append('0', 3);                                                                      //178-180

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();

        }

        private string CreateSumPaymentEntry()
        {
            StringBuilder sb = new StringBuilder();

            #region TotalNetAmount

            string totalNetAmountStr = totalNetAmount.ToString().Replace(".", "").Replace(",", "");

            #endregion

            sb.Append("80");                                                //1-2
            sb.Append(senderContractNr.AddLeadingZeros(6));                 //3-8
            sb.Append(' ', 50);                                             //9-58
            sb.Append(totalNetAmountStr.AddLeadingZeros(15));               //59-73  
            sb.Append('0', 15);                                             //74-88
            sb.Append('0', 15);                                             //89-103
            sb.Append('0', 15);                                             //104-118
            sb.Append('0', 15);                                             //119-133
            sb.Append('0', 15);                                             //134-148
            sb.Append(' ', 32);                                             //149-180

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
