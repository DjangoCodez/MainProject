using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class EmploymentTaxTimePeriodHeadItemDTO
    {
        public EmploymentTaxTimePeriodHeadItemDTO(int employeeId, int year, List<EmploymentTaxTimePeriodRowItemDTO> periods, decimal startValueEmploymentTaxBasis, decimal startValueEmploymentTaxCredit)
        {
            this.EmployeeId = employeeId;
            this.Year = year;
            this.Periods = periods;
            this.StartValueEmploymentTaxBasis = startValueEmploymentTaxBasis;
            this.StartValueEmploymentTaxCredit = startValueEmploymentTaxCredit;
        }

        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public List<EmploymentTaxTimePeriodRowItemDTO> Periods { get; set; }
        public decimal StartValueEmploymentTaxBasis { get; set; }
        public decimal StartValueEmploymentTaxCredit { get; set; }

        public bool IsEmploymentTaxMinimumLimitReachedBeforeGivenPeriod(DateTime currentPaymentDate, bool checkMonth = false)
        {
            return IsEmploymentTaxMinimumLimitReached(GetEmploymentTaxBasisBeforeGivenPeriod(currentPaymentDate, checkMonth));
        }

        public decimal GetEmploymentTaxBasisBeforeGivenPeriod(DateTime currentPaymentDate, bool checkMonth = false)
        {
            var pastPeriods = this.GetPastPeriods(currentPaymentDate, checkMonth);
            return pastPeriods.Sum(x => x.EmploymentTaxBasis) + StartValueEmploymentTaxBasis;
        }

        public bool IsEmploymentTaxMinimumLimitReachedIncludingGivenPeriod(DateTime currentPaymentDate, decimal currentEmploymentTaxBasis)
        {
            var pastPeriods = this.GetPastPeriods(currentPaymentDate);
            return IsEmploymentTaxMinimumLimitReached(Decimal.Add(pastPeriods.Sum(x => x.EmploymentTaxBasis), currentEmploymentTaxBasis) + StartValueEmploymentTaxBasis);
        }

        public List<EmploymentTaxTimePeriodRowItemDTO> GetPastPeriods(DateTime currentPaymentDate, bool checkMonth = false)
        {
            if(checkMonth)
                return this.Periods.Where(x => x.PaymentDate.Month < currentPaymentDate.Month).ToList();
            else
                return this.Periods.Where(x => x.PaymentDate < currentPaymentDate).ToList();
        }

        public bool IsEmploymentTaxMinimumLimitReached(decimal amount)
        {
            return amount >= 1000;
        }

    }

    public class EmploymentTaxTimePeriodRowItemDTO
    {
        public int TimePeriodId { get; set; }
        public decimal EmploymentTaxBasis { get; set; }
        public DateTime PaymentDate { get; set; }

        public EmployeeTimePeriodDTO EmployeeTimePeriodDTO { get; set; }

    }
}
