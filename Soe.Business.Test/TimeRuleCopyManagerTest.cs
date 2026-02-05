using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Soe.Business.Test
{
    [TestClass]
    public class TimeRuleCopyManagerTest
    {
        #region Constants

        protected const string THREAD = "Test";

        #endregion

        #region Variables

        //private int userId;
        private int actorCompanyId;
        private IEnumerable<int> selectedEmployeeGroupIds;
        private IEnumerable<SelectedItemDTO> selectedTimeRules;
        private IEnumerable<SelectedItemDTO> selectedTimeAbsenceRules;
        private IEnumerable<SelectedItemDTO> selectedAttestRules;
        private IEnumerable<MatchedItemDTO> matchedDayTypes;
        private IEnumerable<MatchedItemDTO> matchedTimeDeviationCauses;
        private IEnumerable<MatchedItemDTO> matchedTimeCodes;
        private IEnumerable<MatchedItemDTO> matchedPayrollProducts;
        private IEnumerable<MatchedItemDTO> dayTypesToCopy;
        private IEnumerable<MatchedItemDTO> timeDeviationCausesToCopy;
        private IEnumerable<MatchedItemDTO> timeCodesToCopy;
        private IEnumerable<MatchedItemDTO> payrollProductsToCopy;

        #endregion

        #region Ctor

        public TimeRuleCopyManagerTest()
        {
            this.SetupTest();
        }

        #endregion

        #region Setup

        protected void SetupTest()
        {
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, THREAD, true);
        }

        #endregion

        #region Tests

        [TestMethod]
        public void TestCopyOneGroupedTimeRule()
        {
            SetupTestCopyOneGroupedTimeRule();
            ActionResult result = CopyRulesInTransaction();
            Assert.AreEqual(true, result.Success);
        }

        private void SetupTestCopyOneGroupedTimeRule()
        {
            //this.userId = 10;
            this.actorCompanyId = 8711;
            this.selectedEmployeeGroupIds = new List<int> { 12, 11 };
            this.selectedTimeRules = new List<SelectedItemDTO>();
            this.selectedTimeAbsenceRules = new List<SelectedItemDTO>();
            this.selectedAttestRules = new List<SelectedItemDTO>();
            this.matchedDayTypes = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 166,
					TargetItemId = 21
				}
			};
            this.matchedTimeDeviationCauses = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 209,
					TargetItemId = 455
				}
			};
            this.matchedTimeCodes = Enumerable.Empty<MatchedItemDTO>();
            this.matchedPayrollProducts = Enumerable.Empty<MatchedItemDTO>();
            this.dayTypesToCopy = Enumerable.Empty<MatchedItemDTO>();
            this.timeDeviationCausesToCopy = Enumerable.Empty<MatchedItemDTO>();
            this.timeCodesToCopy = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 586
				},
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 601
				},
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 602
				},
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 1022
				}
			};
            this.payrollProductsToCopy = Enumerable.Empty<MatchedItemDTO>();
        }

        [TestMethod]
        public void TestCopySecondGroupedTimeRule()
        {
            SetupTestCopySecondGroupedTimeRule();
            ActionResult result = CopyRulesInTransaction();
            Assert.AreEqual(true, result.Success);
        }

        private void SetupTestCopySecondGroupedTimeRule()
        {
            //this.userId = 10;
            this.actorCompanyId = 8711;
            this.selectedEmployeeGroupIds = new List<int> { 12, 11 };
            this.selectedTimeRules = new List<SelectedItemDTO>();
            this.selectedTimeAbsenceRules = new List<SelectedItemDTO>();
            this.selectedAttestRules = new List<SelectedItemDTO>();
            this.matchedDayTypes = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 166,
					TargetItemId = 21
				}
			};
            this.matchedTimeDeviationCauses = new List<MatchedItemDTO>
            {
            };
            this.matchedTimeCodes = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 601,
					TargetItemId = 1301
				},
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 602,
					TargetItemId = 1302
				}
			};
            this.matchedPayrollProducts = Enumerable.Empty<MatchedItemDTO>();
            this.dayTypesToCopy = Enumerable.Empty<MatchedItemDTO>();
            this.timeDeviationCausesToCopy = new List<MatchedItemDTO>
			{
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 210
				}
			};
            this.timeCodesToCopy = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 587
				}
			};
            this.payrollProductsToCopy = Enumerable.Empty<MatchedItemDTO>();
        }

        [TestMethod]
        public void TestCopyRemainingRules()
        {
            SetupTestCopyRemainingRules();
            ActionResult result = CopyRulesInTransaction();
            Assert.AreEqual(true, result.Success);
        }

        private void SetupTestCopyRemainingRules()
        {
            //this.userId = 10;
            this.actorCompanyId = 8711;
            this.selectedEmployeeGroupIds = new List<int> { 12, 11 };
            this.selectedTimeRules = new List<SelectedItemDTO>
			{
				new SelectedItemDTO{CompanyId = 30449, ItemId=438},
				new SelectedItemDTO{CompanyId = 30449, ItemId=439},
				new SelectedItemDTO{CompanyId = 30449, ItemId=440},
			};
            this.selectedTimeAbsenceRules = new List<SelectedItemDTO>
			{
				new SelectedItemDTO{CompanyId = 30449, ItemId=7},
			};
            this.selectedAttestRules = new List<SelectedItemDTO>
			{
				new SelectedItemDTO{CompanyId = 30449, ItemId=7},
				new SelectedItemDTO{CompanyId = 30449, ItemId=8},
			};
            this.matchedDayTypes = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 167,
					TargetItemId = 22
				},
			};
            this.matchedTimeDeviationCauses = new List<MatchedItemDTO>
			{
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 209,
					TargetItemId = 455
				}
			};
            this.matchedTimeCodes = new List<MatchedItemDTO> {
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 586,
					TargetItemId = 1300
				},
				new MatchedItemDTO {
					CompanyId = 30449,
					SourceItemId = 588,
					TargetItemId = 61
				}
			};
            this.matchedPayrollProducts = Enumerable.Empty<MatchedItemDTO>();
            this.dayTypesToCopy = new List<MatchedItemDTO>
			{
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 168},
			};
            this.timeDeviationCausesToCopy = new List<MatchedItemDTO>
			{
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 210},
			};
            this.timeCodesToCopy = new List<MatchedItemDTO> {
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 603},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 587},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 703},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 591},
			};
            this.payrollProductsToCopy = new List<MatchedItemDTO>
			{
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66578},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66579},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66580},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66581},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66582},
				new MatchedItemDTO {CompanyId = 30449, SourceItemId = 66583},
			};
        }

        #endregion

        #region Help-methods

        private ActionResult CopyRulesInTransaction()
        {
            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        try
                        {
                            return CopyRules(entities, transaction);
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
                finally
                {
                    entities.Connection.Close();
                }
            }
        }

        private ActionResult CopyRules(CompEntities entities, TransactionScope transaction)
        {
            TimeRuleCopyManager copyManager = new TimeRuleCopyManager(
                null,
                entities,
                transaction,
                this.actorCompanyId,
                this.selectedEmployeeGroupIds,
                this.selectedTimeRules,
                this.selectedTimeAbsenceRules,
                this.selectedAttestRules,
                this.matchedDayTypes,
                this.matchedTimeDeviationCauses,
                this.matchedTimeCodes,
                this.matchedPayrollProducts,
                this.dayTypesToCopy,
                this.timeDeviationCausesToCopy,
                this.timeCodesToCopy,
                this.payrollProductsToCopy);

            return copyManager.Copy(this.actorCompanyId);
        }

        #endregion
    }
}
