using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.AccountDistribution
{
    internal static class AccountDistributionUtility
    {
        public static DateTime GetEntryDate(int dayNumber, DateTime period)
        {
            var maxValidDayNumber = DateTime.DaysInMonth(period.Year, period.Month);

            if (dayNumber < 1)
                dayNumber = 1;
            else if (dayNumber > maxValidDayNumber)
                dayNumber = maxValidDayNumber;

            return new DateTime(
                period.Year,
                period.Month,
                dayNumber);
        }

        public static List<MinimalAccountingRow> GetRowsMatchingExpression(AccountDistributionHead head, int accountStdDimId, IEnumerable<MinimalAccountingRow> accountingRows)
        {
            var matchingRows = new List<MinimalAccountingRow>();
            var expressions = GetExpressionsFromHead(head);

            foreach (var row in accountingRows) 
                if (IsRowMatchingExpression(accountStdDimId, expressions, row))
                    matchingRows.Add(row);

            return matchingRows;
        }

        private static List<(int AccountDimId, Regex)> GetExpressionsFromHead(AccountDistributionHead head)
        {
            return head.AccountDistributionHeadAccountDimMapping
                .Select(m => (m.AccountDimId, GetRegexFromExpression(m)))
                .ToList();
        }

        private static bool IsRowMatchingExpression(int accountStdDimId, List<(int AccountDimId, Regex)> regexs, MinimalAccountingRow row)
        {
            // The default is NO match.
            if (regexs.IsNullOrEmpty())
                return false;

            foreach (var (dimId, regex) in regexs)
            {
                if (dimId == accountStdDimId)
                {
                    if (!regex.IsMatch(row.AccountNr))
                        return false;

                    continue;
                }

                var accountInternal = row.Accounts.FirstOrDefault(a => a.AccountDimId == dimId);
                var accountNr = accountInternal?.AccountNr ?? string.Empty;

                if (!regex.IsMatch(accountNr))
                    return false;
            }
            return true;
        }

        public static Regex GetRegexFromExpression(AccountDistributionHeadAccountDimMapping mapping)
        {
            var expression = mapping.AccountExpression;
            return new Regex(StringUtility.WildCardToRegEx(expression));
        }
    }
}
