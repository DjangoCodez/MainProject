using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    abstract class SalaryPaymentBaseAdapter
    {
        public List<EmployeeSalaryPaymentAdapterItem> EmployeeItems= new List<EmployeeSalaryPaymentAdapterItem>();        
        public SoeTimeSalaryPaymentExportFormat ExportFormat = SoeTimeSalaryPaymentExportFormat.Text;
        public TermGroup_TimeSalaryPaymentExportType ExportType = TermGroup_TimeSalaryPaymentExportType.Undefined;
        public string Extension = "txt";
        public string UniqueMsgKey;
        public string UniquePaymentKey;
        public decimal? CurrencyRate;

        readonly private TimePeriod timePeriod;

        public SalaryPaymentBaseAdapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod)
        {
            this.timePeriod = timePeriod;

            foreach (var employee in employees)
            {
                EmployeeItems.Add(new EmployeeSalaryPaymentAdapterItem(employee, transactions.Where(x => x.EmployeeId == employee.EmployeeId).ToList()));
            }
        }

        #region Abstract
        
        abstract public byte[] CreateFile();
        
        #endregion

        #region Public 
        
        public TermGroup_TimeSalaryPaymentExportType GetExportType()
        {
            return this.ExportType;
        }
        
        public string GetPaymentDateYYMMDD()
        {
            return this.timePeriod != null && this.timePeriod.PaymentDate.HasValue ? this.timePeriod.PaymentDate.Value.Date.ToString("yyMMdd") : CalendarUtility.DATETIME_DEFAULT.Date.ToString("yyMMdd");
        }

        public string GetPaymentDateYYYYMMDD()
        {
            return this.timePeriod != null && this.timePeriod.PaymentDate.HasValue ? this.timePeriod.PaymentDate.Value.Date.ToString("yyyyMMdd") : CalendarUtility.DATETIME_DEFAULT.Date.ToString("yyyyMMdd");
        }

        public string GetPaymentDateYYYY_MM_DD(int addDays)
        {            
            return this.timePeriod != null && this.timePeriod.PaymentDate.HasValue ? this.GetPaymentDateYYYY_MM_DD(this.timePeriod.PaymentDate.Value.AddDays(addDays).Date) : this.GetPaymentDateYYYY_MM_DD(CalendarUtility.DATETIME_DEFAULT.Date);
        }
        public string GetPaymentDateYYYY_MM_DD(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd");
        }

        public string GetExportDateYYMMDD()
        {
            return DateTime.Now.ToString("yyMMdd");
        }

        public string GetExportDateYYYYMMDD()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }
      
        public string GetISO20222ExportDate()
        {
            return DateTime.Now.ToString("yyyy'-'MM'-'dd") + "T" + DateTime.Now.ToString("hh':'mm':'ss");
        }
        public string CreateUniqueId()
        {
            return Guid.NewGuid().ToString().Replace("-", ""); //needs to stay below 35 chars 
        }

        #endregion
    }

    public class EmployeeSalaryPaymentAdapterItem
    {
        readonly private Employee Employee;
        public List<TimePayrollTransaction> Transactions;
        public string UniquePaymentRowKey;

        #region Contructor

        public EmployeeSalaryPaymentAdapterItem(Employee employee, List<TimePayrollTransaction> transactions)
        {
            this.Employee = employee;
            this.Transactions = transactions;
        }

        #endregion

        #region Properties

        public string FormattedRecieverAccountNr { get; set; }

        public int EmployeeId
        {
            get { return Employee.EmployeeId; }
        }

        public string Name
        {
            get { return Employee.Name; }
        }

        public string EmployeeNr
        {
            get { return Employee.EmployeeNr; }
        }

        public bool IsSE_PersonAccount
        {
            get { return Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_PersonAccount; }
        }

        public bool IsSE_CashDeposit
        {
            get { return Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit; }
        }

        public bool IsIBAN
        {
            get { return false; }
            //get { return (int)Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_IBAN; }
        }

        public int DisbursementMethod
        {
            get { return Employee.DisbursementMethod; }
        }

        #endregion

        #region Public methods

        public string GetClearingNr(bool removeHyphen = true)
        {
            if (removeHyphen)
                return this.Employee.DisbursementClearingNr.Replace("-", "").Replace(" ","");
            else
                return this.Employee.DisbursementClearingNr.Replace(" ", "");
        }

        public string GetAccountNr(bool removeHyphen = true)
        {
            if (removeHyphen)
                return this.Employee.DisbursementAccountNr.Replace("-", "").Replace(" ", "");
            else
                return this.Employee.DisbursementAccountNr.Replace(" ", "");
        }
        public string GetBIC()
        {
            return this.Employee.DisbursementBIC;
        }
        public string GetIBAN()
        {
            return this.Employee.DisbursementIBAN;
        }
        public string GetCountryCode()
        {
            return this.Employee.DisbursementCountryCode;
        }

        public decimal GetNetAmount(decimal? currencyRate = null)
        {
            decimal netAmount = this.Transactions.Where(x => x.IsNetSalaryPaid() && x.Amount.HasValue).Sum(x => x.Amount.Value);
            if (currencyRate.HasValue)
                netAmount = decimal.Round(currencyRate.Value * netAmount, 2, MidpointRounding.AwayFromZero);

            return netAmount;
        }

        public bool IsZeroNetAmount
        {
            get
            {
                return this.GetNetAmount() == 0;
            }           
        }

        public string GetSocialSec(TermGroup_TimeSalaryPaymentExportType exportType)
        {
            if (this.Employee.ContactPerson == null || string.IsNullOrEmpty(this.Employee.ContactPerson.SocialSec))
                return "";

            string socialSec = this.Employee.ContactPerson.SocialSec.Trim();
            socialSec = socialSec.RemoveWhiteSpace('-');

            if (exportType == TermGroup_TimeSalaryPaymentExportType.BGCKI && socialSec.Length == 12)                            
                socialSec = socialSec.Substring(2);
            
            return socialSec;
        }
        
        #endregion
    }
}
