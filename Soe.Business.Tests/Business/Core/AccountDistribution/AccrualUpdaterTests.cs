using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Business.Tests.Business.Core.AccountDistribution.Mocks;
using Soe.Business.Tests.Business.Core.AccountDistribution.Stubs;
using SoftOne.Soe.Business.Core.AccountDistribution.Accrual;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution
{
    [TestClass]
    public class AccrualUpdaterTests
    {
        private StubQueryService _stubQuery;
        private StubStateUtility _stubState;
        private StubDbService _stubDb;
        private AccrualUpdaterParameters _params;
        private const int TestCompanyId = 1;
        private const int TestHeadId = 100;

        [TestInitialize]
        public void Setup()
        {
            _stubQuery = new StubQueryService();
            _stubState = new StubStateUtility();
            _stubDb = new StubDbService();

            _params = new AccrualUpdaterParameters(
                TestCompanyId, TestHeadId,
                datesChanged: false,
                rowsChanged: false,
                accountDimStd: new AccountDim(),
                accountYear: AccrualMockFactory.MockAccountYear(2025)
            );
        }

        [TestMethod]
        public void PerformUpdate_HeadNotFound_ReturnsEntityNotFound()
        {
            _stubQuery.HeadToReturn = null;
            var updater = CreateUpdater();

            var result = updater.PerformUpdate();

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual((int)ActionResultSave.EntityNotFound, result.ErrorNumber);
        }

        [TestMethod]
        public void PerformUpdate_IncorrectCalculationType_ReturnsError()
        {
            _stubQuery.HeadToReturn = AccrualMockFactory.MockDistributionHead(TestHeadId, (int)TermGroup_AccountDistributionCalculationType.Percent, 0, DateTime.Today);
            var updater = CreateUpdater();

            var result = updater.PerformUpdate();

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual((int)ActionResultSave.IncorrectInput, result.ErrorNumber);
        }

        [TestMethod]
        public void PerformUpdate_AmountType_CreatesNewEntry()
        {
            var startDate = new DateTime(2025, 1, 1);
            var head = AccrualMockFactory.MockDistributionHead(TestHeadId, (int)TermGroup_AccountDistributionCalculationType.Amount, (int)TermGroup_AccountDistributionPeriodType.Amount, startDate);
            head.AddRow(100, 500m); // Add 500 amount to distribute

            _stubQuery.HeadToReturn = head;
            _stubQuery.EntriesToReturn = new List<AccountDistributionEntry>();

            var updater = CreateUpdater();

            var result = updater.PerformUpdate();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.HasCount(1, _stubState.AddedObjects,  "Should create 1 entry for 1 month duration");

            var createdEntry = _stubState.AddedObjects.First() as AccountDistributionEntry;
            Assert.AreEqual(startDate, createdEntry.Date);
            Assert.IsTrue(_stubDb.SaveCalled);
        }

        [TestMethod]
        public void PerformUpdate_ExistingEntry_UpdatesRows_WhenFlagIsSet()
        {
            _params.DistributionRowsChanged = true;
            var startDate = new DateTime(2025, 1, 1);

            var dim = new AccountDim
            {
                ActorCompanyId = TestCompanyId,
                Account = new EntityCollection<Account>()
            };
            dim.Account.Add(new Account { AccountId = 100, AccountNr = "100", State = (int)SoeEntityState.Active });
            dim.Account.Add(new Account { AccountId = 200, AccountNr = "200", State = (int)SoeEntityState.Active });
            _params.AccountDimStd = dim;

            var updater = CreateUpdater();

            var head = AccrualMockFactory.MockDistributionHead(TestHeadId, (int)TermGroup_AccountDistributionCalculationType.Amount, (int)TermGroup_AccountDistributionPeriodType.Amount, startDate);

            head.AddRow(100, 999m);
            head.AddRow(200, -999m);

            _stubQuery.HeadToReturn = head;

            var existingEntry = AccrualMockFactory.MockEntry(TestHeadId, startDate);
            var oldRow = new AccountDistributionEntryRow { AccountId = 100, DebitAmount = 100m };
            existingEntry.AccountDistributionEntryRow.Add(oldRow);

            _stubQuery.EntriesToReturn = new List<AccountDistributionEntry> { existingEntry };

            var result = updater.PerformUpdate();

            // Assert
            Assert.IsTrue(result.Success);

            // Check Old Row Deletion
            Assert.Contains(oldRow, _stubState.DeletedObjects, "Old row should be deleted");

            // Check New Rows
            var rows = existingEntry.AccountDistributionEntryRow;
            Assert.HasCount(2, rows, "Should generate exactly 2 rows (Debit and Credit).");

            var debitRow = rows.FirstOrDefault(r => r.AccountId == 100);
            var creditRow = rows.FirstOrDefault(r => r.AccountId == 200);

            Assert.IsNotNull(debitRow, "Debit row (Acct 100) missing");
            Assert.IsNotNull(creditRow, "Credit row (Acct 200) missing");

            Assert.AreEqual(999m, debitRow.DebitAmount - debitRow.CreditAmount);
            Assert.AreEqual(-999m, creditRow.DebitAmount - creditRow.CreditAmount);
        }

        [TestMethod]
        public void PerformUpdate_DatesChanged_DeletesOutdatedEntries()
        {
            _params.DatesChanged = true; 
            var startDate = new DateTime(2025, 1, 1);

            var head = AccrualMockFactory.MockDistributionHead(TestHeadId, (int)TermGroup_AccountDistributionCalculationType.Amount, (int)TermGroup_AccountDistributionPeriodType.Amount, startDate);
            _stubQuery.HeadToReturn = head;

            var validEntry = AccrualMockFactory.MockEntry(TestHeadId, startDate);
            var outdatedEntry = AccrualMockFactory.MockEntry(TestHeadId, startDate.AddMonths(1));

            _stubQuery.EntriesToReturn = new List<AccountDistributionEntry> { validEntry, outdatedEntry };

            var updater = CreateUpdater();

            var result = updater.PerformUpdate();

            Assert.IsTrue(result.Success);

            Assert.Contains(outdatedEntry, _stubState.DeletedObjects);
            Assert.DoesNotContain(validEntry, _stubState.DeletedObjects);
        }

        private AccrualUpdater CreateUpdater()
        {
            return new AccrualUpdater(
                _params,
                _stubQuery,
                new StubCurrencySetter(),
                _stubDb,
                _stubState
            );
        }
    }
}
