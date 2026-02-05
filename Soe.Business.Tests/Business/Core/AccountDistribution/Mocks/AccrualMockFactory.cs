using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution.Mocks
{
    internal static class AccrualMockFactory
    {
        public static AccountDistributionHead MockDistributionHead(int headId, int calcType, int periodType, DateTime startDate)
        {
            var head = new AccountDistributionHead
            {
                AccountDistributionHeadId = headId,
                CalculationType = calcType,
                PeriodType = periodType,
                StartDate = startDate,
                EndDate = startDate.AddMonths(1).AddDays(-1), // Default 1 Month
                DayNumber = 1,
                State = (int)SoeEntityState.Active,
                AccountDistributionRow = new EntityCollection<AccountDistributionRow>()
            };
            return head;
        }

        public static AccountDistributionEntry MockEntry(int headId, DateTime date, bool isTransferred = false)
        {
            var entry = new AccountDistributionEntry
            {
                AccountDistributionHeadId = headId,
                Date = date,
                State = (int)SoeEntityState.Active,
                VoucherHeadId = isTransferred ? (int?)12345 : null,
                AccountDistributionEntryRow = new EntityCollection<AccountDistributionEntryRow>()
            };
            return entry;
        }

        public static AccountYear MockAccountYear(int year)
        {
            return new AccountYear
            {
                AccountYearId = year,
                From = new DateTime(year, 1, 1),
                To = new DateTime(year, 12, 31)
            };
        }

        public static VoucherRow MockVoucherRow(decimal amount, int accountId)
        {
            return new VoucherRow
            {
                Amount = amount,
                VoucherHead = new VoucherHead { Date = DateTime.Today }
            };
        }

        public static void AddRow(this AccountDistributionHead head, int accountId, decimal amount)
        {
            head.AccountDistributionRow.Add(new AccountDistributionRow
            {
                AccountId = accountId,
                SameBalance = amount,
                AccountDistributionRowAccount = new EntityCollection<AccountDistributionRowAccount>()
            });
        }
    }
}
