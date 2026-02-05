using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Text;
namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    /**
    * Implementation for BGC Salary, http://www.bgc.se/tjanster/utbetalningar/loner/teknisk-information/
    *      
    * */
    class BGCKIAdapter : SalaryPaymentBaseAdapter
    {

        #region Variables

        private string senderCustomerId;
        private string senderBankGiro;
        readonly private string currency;
        private decimal totalNetAmount = 0;
        private int paymentEntryTransactionsCount = 0;

        #endregion

        #region Constructor
        
        public BGCKIAdapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod, string senderCustomerId, string senderBankGiro, string currency)
            : base(employees, transactions, timePeriod)
        {
            base.ExportFormat = SoeTimeSalaryPaymentExportFormat.Text;
            base.ExportType = TermGroup_TimeSalaryPaymentExportType.BGCKI;

            this.senderCustomerId = senderCustomerId.Trim().Replace("-", "");
            this.senderBankGiro = senderBankGiro.Trim().Replace("-", "");
            this.currency = currency;
        }

        #endregion

        #region Methods

        #region Public
        
        public override byte[] CreateFile()
        {
            string doc = string.Empty;            
            doc = CreateOpeningEntry();
            doc += CreatePaymentEntries();
            doc += CreateSumPaymentEntry();
            return Constants.ENCODING_LATIN1.GetBytes(doc);            
        }

        #endregion

        #region Private
        
        private string CreateOpeningEntry()
        {
            StringBuilder sb = new StringBuilder();

            if (senderCustomerId.Length > 6)
                senderCustomerId = senderCustomerId.Left(6);

            if (senderBankGiro.Length > 10)
                senderBankGiro = senderBankGiro.Left(10);

            sb.Append("01");                                                //1-2
            sb.Append(base.GetExportDateYYMMDD());                          //3-8
            sb.Append(' ', 2);                                              //9-10
            sb.Append("LON");                                               //11-13
            sb.Append(' ', 46);                                             //14-59
            sb.Append(currency);                                            //60-62
            sb.Append(senderCustomerId.AddLeadingZeros(6));                 //63-68
            sb.Append(senderBankGiro.AddLeadingZeros(10));                  //69-78
            sb.Append(' ', 2);                                              //79-80
            
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

                paymentEntryTransactionsCount++;

                #region NetAmount

                decimal netAmount = decimal.Round(employeeItem.GetNetAmount(), 2, MidpointRounding.AwayFromZero); //Amount is already rounded to 2 decimals, but to bee sure...
                string netAmountStr = netAmount.ToString().Replace(".","").Replace(",","");
                totalNetAmount += netAmount;

                #endregion

                #region ClearingNr and AccountNr

                string clearingNr = employeeItem.GetClearingNr();
                string accountNr = employeeItem.GetAccountNr();

                if (clearingNr.Length > 4)
                    clearingNr = clearingNr.Left(4);

                clearingNr = clearingNr.AddLeadingZeros(4);

                if (accountNr.Length > 12)
                    accountNr = accountNr.Left(12);

                accountNr = accountNr.AddLeadingZeros(12);

                employeeItem.FormattedRecieverAccountNr = clearingNr + accountNr;

                #endregion

                sb.Append("35");                                                                //1-2
                sb.Append(base.GetPaymentDateYYMMDD());                                         //3-8
                sb.Append(' ', 4 );                                                             //9-12
                sb.Append(employeeItem.FormattedRecieverAccountNr);                             //13-28
                sb.Append(netAmountStr.AddLeadingZeros(12));                                    //29-40
                sb.Append(' ', 18);                                                             //41-58
                sb.Append(employeeItem.GetSocialSec(base.GetExportType()).AddLeadingZeros(10)); //59-68
                sb.Append("Lon".AddTrailingBlanks(12));                                         //69-80

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private string CreateSumPaymentEntry()
        {
            StringBuilder sb = new StringBuilder();
            string totalNetAmountStr = totalNetAmount.ToString().Replace(".","").Replace(",","");

            sb.Append("09");                                                        //1-2
            sb.Append(base.GetExportDateYYMMDD());                                  //3-8
            sb.Append(' ', 20);                                                     //9-28
            sb.Append(totalNetAmountStr.AddLeadingZeros(12));                       //29-40
            sb.Append(paymentEntryTransactionsCount.ToString().AddLeadingZeros(6)); //41-46
            sb.Append('0', 34);                                                     //47-80

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        #endregion

        #endregion
    }
}
