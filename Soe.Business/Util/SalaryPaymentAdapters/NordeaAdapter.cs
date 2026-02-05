using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    class NordeaAdapter: SalaryPaymentBaseAdapter
    {
        #region Variables

        readonly private string employersId;
        
        #endregion

        #region Constructor

        public NordeaAdapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod, string employersId )
            : base(employees, transactions, timePeriod)
        {
            base.ExportFormat = SoeTimeSalaryPaymentExportFormat.Text;
            base.ExportType = TermGroup_TimeSalaryPaymentExportType.Nordea;

            this.employersId = employersId.Trim().Replace("-", "");
            if (this.employersId.Length > 6)
                this.employersId = this.employersId.Left(6);
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

            return Encoding.UTF8.GetBytes(doc);
        }

        #endregion

        #region Private

        private string CreateOpeningEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("I00");                                                //1-3
            sb.Append(this.employersId.AddLeadingZeros(6));                  //4-9
            sb.Append('0', 3);                                               //10-12

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

                #endregion

                //The recievers social security number is mapped to a account in a registry at Nordea => Nordea is registerholder
                employeeItem.FormattedRecieverAccountNr = employeeItem.GetSocialSec(base.GetExportType()).AddLeadingZeros(12);

                sb.Append("L00");                                                               //1-3
                sb.Append(this.employersId.AddLeadingZeros(6));                                 //4-9
                sb.Append('0', 3);                                                              //10-12
                sb.Append(employeeItem.FormattedRecieverAccountNr);                             //13-24
                sb.Append('0', 10);                                                             //25-34     
                sb.Append('0', 11);                                                             //35-45
                sb.Append(netAmountStr.AddLeadingZeros(11));                                    //46-56
                sb.Append("J");                                                                 //57
                sb.Append("00350");                                                             //58-62
                sb.Append(base.GetPaymentDateYYYYMMDD());                                       //63-70
                sb.Append(' ', 3);                                                              //71-73
                sb.Append(' ', 1);                                                              //74

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private string CreateSumPaymentEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("S99");                                                               //1-3
            sb.Append(this.employersId.AddLeadingZeros(6));                                 //4-9
            sb.Append('0', 3);                                                              //10-12

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        #endregion

        #endregion    
    }
}
