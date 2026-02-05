using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.AccountDistribution
{
    /// <summary>
    /// Pure calculation logic for accrual generation - easily testable without database dependencies
    /// </summary>
    internal static class AccrualCalculator
    {
        /// <summary>
        /// Calculates debit and credit amounts based on calculation type and distribution row configuration
        /// </summary>
        public static (decimal debit, decimal credit) CalculateAmounts(
            TermGroup_AccountDistributionCalculationType calculationType,
            decimal sourceAmount,
            decimal sameBalance,
            decimal oppositeBalance)
        {
            if (calculationType == TermGroup_AccountDistributionCalculationType.Amount)
            {
                return (sameBalance, oppositeBalance);
            }
            else
            {
                // Percentage-based calculation
                if (sourceAmount > 0)
                {
                    var creditAmount = Decimal.Round((oppositeBalance / 100) * sourceAmount, 2);
                    var debitAmount = Decimal.Round((sameBalance / 100) * sourceAmount, 2);
                    return (debitAmount, creditAmount);
                }
                else
                {
                    // Negative amounts swap the balance fields
                    var creditAmount = Decimal.Round((sameBalance / 100) * (sourceAmount * -1), 2);
                    var debitAmount = Decimal.Round((oppositeBalance / 100) * (sourceAmount * -1), 2);
                    return (debitAmount, creditAmount);
                }
            }
        }

        /// <summary>
        /// Groups accounting rows by their account composition and sums amounts
        /// </summary>
        public static IEnumerable<(MinimalAccountingRow representative, decimal totalAmount)> GroupAndSumByAccount(
            IEnumerable<MinimalAccountingRow> rows)
        {
            var groupedRows = rows.GroupBy(m => m.GetAccountCompositionKey());

            foreach (var group in groupedRows)
            {
                var representative = group.FirstOrDefault();
                var totalAmount = group.Sum(r => r.Amount);

                if (totalAmount != 0) // Skip zero-sum groups
                {
                    yield return (representative, totalAmount);
                }
            }
        }

        /// <summary>
        /// Determines the date interval for voucher queries based on period type
        /// </summary>
        public static (DateTime from, DateTime to) GetVoucherDateInterval(
            TermGroup_AccountDistributionPeriodType periodType,
            DateTime periodStartDate,
            DateTime yearStartDate)
        {
            var periodStart = new DateTime(periodStartDate.Year, periodStartDate.Month, 1);
            var periodEnd = periodStart.AddMonths(1);

            if (periodType == TermGroup_AccountDistributionPeriodType.Period)
            {
                return (periodStart, periodEnd);
            }
            else // Year type
            {
                return (yearStartDate, periodEnd);
            }
        }

        /// <summary>
        /// Retrieves the effective start and end dates from the AccountDistributionHead.
        /// Defaults the EndDate to DateTime.Today if it is null.
        /// </summary>
        public static (DateTime Start, DateTime End) GetHeadDuration(AccountDistributionHead head)
        {
            DateTime start = head.StartDate.GetValueOrDefault(DateTime.Today);
            DateTime end = head.EndDate.GetValueOrDefault(DateTime.Today);

            return (start, end);
        }

        /// <summary>
        /// Calculates the total number of months to iterate over based on the duration and the specific distribution day.
        /// </summary>
        public static int GetMonthCount(AccountDistributionHead head)
        {
            DateTime start = head.StartDate.GetValueOrDefault(DateTime.Today);
            DateTime end = head.EndDate.GetValueOrDefault(DateTime.Today);

            DateTime firstDayOfEndMonth = new DateTime(end.Year, end.Month, 1);
            DateTime lastDayOfEndMonth = firstDayOfEndMonth.AddMonths(1).AddDays(-1);

            int months = ((end.Year - start.Year) * 12) + end.Month - start.Month;

            if (end.Day == lastDayOfEndMonth.Day || end.Day >= head.DayNumber)
            {
                months++;
            }

            return months;
        }
    }
}
