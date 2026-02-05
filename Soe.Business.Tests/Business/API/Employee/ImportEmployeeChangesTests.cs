using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Shared.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.API.Employee
{
    [TestClass()]
    public class ImportEmployeeChangesTests : TestBase
    {
        #region 1 Active (TODO)

        #endregion

        #region 2 FirstName

        [TestMethod()]
        public void FirstNameUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.FirstName,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void FirstNamesUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.FirstName,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void FirstNameUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.FirstName,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void FirstNamesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.FirstName,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region  3 LastName

        [TestMethod()]
        public void LastNameUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.LastName,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void LastNamesUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.LastName,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void LastNameUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.LastName,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void LastNamesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.LastName,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 4 SocialSec

        [TestMethod()]
        public void SocialSecUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SocialSec,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void SocialSecsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SocialSec,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void SocialSecUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SocialSec,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void SocialSecsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SocialSec,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 5-6 Disbursement (TODO)

        #endregion

        #region 7 ExternalCode

        [TestMethod()]
        public void ExternalCodeUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalCode,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void ExternalCodedsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalCode,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void ExternalCodeUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalCode,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void ExternalCodesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalCode,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 50 HierarchicalAccount

        [TestMethod()]
        public void HierchalAccountUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }


        [TestMethod()]
        public void HierchalAccountsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountUpdateDateFromTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountUpdateDateFromsTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountUpdateDateToTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountDateTosUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void HierchalAccountsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.HierarchicalAccount,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 70 AccountNrSieDim (TODO)

        #endregion

        #region 100 Email

        [TestMethod()]
        public void EmailUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.Email,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmailsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.Email,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmailUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.Email,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmailsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.Email,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 101-103 Phone (TODO)

        #endregion

        #region 104-111 Closest relative (TODO)

        #endregion

        #region 112-117 Adress (TODO)

        #endregion

        #region 118 Extra Field (TODO)

        #endregion

        #region 119 ExcludeFromPayroll

        [TestMethod()]
        public void EmployeeExcludeFromPayrollUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExcludeFromPayroll,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeExcludeFromPayrollsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExcludeFromPayroll,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeExcludeFromPayrollIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExcludeFromPayroll,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmployeeExcludeFromPayrollInvalidTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExcludeFromPayroll,
                new ImportEmployeeChangesTestsInputParameters(forceInvalid: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 121 ExcludeFromPayroll

        [TestMethod()]
        public void EmployeeWantsExtraShiftUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WantsExtraShifts,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeWantsExtraShiftsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WantsExtraShifts,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeWantsExtraShiftdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WantsExtraShifts,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmployeeWantsExtraShiftInvalidTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WantsExtraShifts,
                new ImportEmployeeChangesTestsInputParameters(forceInvalid: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 200-201 Employment

        [TestMethod()]
        public void EmploymentCreateNewTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentStartDateChange,
                new ImportEmployeeChangesTestsInputParameters(newFromDate: true), "2"); //must be closed
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentCreateNewOverlappingTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentStartDateChange,
                new ImportEmployeeChangesTestsInputParameters(newFromDate: false), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentUpdateStartDateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentStartDateChange,
                new ImportEmployeeChangesTestsInputParameters(newFromDate: true, customFlag: true), "2"); //must be closed
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentUpdateStopDateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentStopDateChange,
                new ImportEmployeeChangesTestsInputParameters(newToDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        #endregion

        #region 202 EmpoyeeGroup

        [TestMethod()]
        public void EmployeeGroupUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmployeeGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeGroupsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmployeeGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmployeeGroupUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmployeeGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmployeeGroupsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmployeeGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 203 PayrollGroup

        [TestMethod()]
        public void PayrollGroupUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.PayrollGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void PayrollGroupsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.PayrollGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void PayrollGroupUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.PayrollGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void PayrollGroupdsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.PayrollGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 204 VacationGroup

        [TestMethod()]
        public void VacationGroupUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void VacationGroupsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationGroup,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void VacationGroupIdenticalUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void VacationGroupsIdenticalUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationGroup,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 205 WorkTimeWeek

        [TestMethod()]
        public void EmploymentWorkTimeWeekUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTimeWeekMinutes,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTimeWeeksUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTimeWeekMinutes,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTimeWeekUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTimeWeekMinutes,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTimeWeeksUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTimeWeekMinutes,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 206 EmploymentPercent

        [TestMethod()]
        public void EmploymentPercentUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPercent,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPercentsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPercent,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPercentIdenticalUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPercent,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPercentsIdenticalUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPercent,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 207 EmploymentExternalCode

        [TestMethod()]
        public void EmploymentExternalCodeUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentExternalCode,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExternalCodedsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentExternalCode,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExternalCodeUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentExternalCode,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExternalCodesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentExternalCode,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 208 EmploymentType (TODO)

        #endregion

        #region 212 SecondaryEmployment

        [TestMethod()]
        public void EmploymentIsSecondaryEmploymentUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.IsSecondaryEmployment,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentIsSecondaryEmploymentdsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.IsSecondaryEmployment,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "0");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentIsSecondaryEmploymentUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.IsSecondaryEmployment,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentIsSecondaryEmploymentsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.IsSecondaryEmployment,
                new ImportEmployeeChangesTestsInputParameters(), "1", "0");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 220 ExperienceMonths

        [TestMethod()]
        public void EmploymentExperienceMonthsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceMonths,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceMonthsdsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceMonths,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceMonthsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceMonths,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceMonthssUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceMonths,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 221 ExperienceAgreedOrEstablished

        [TestMethod()]
        public void EmploymentExperienceAgreedOrEstablishedUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceAgreedOrEstablished,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceAgreedOrEstablisheddsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceAgreedOrEstablished,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "0");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceAgreedOrEstablishedUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceAgreedOrEstablished,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentExperienceAgreedOrEstablishedsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExperienceAgreedOrEstablished,
                new ImportEmployeeChangesTestsInputParameters(), "1", "0");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 223 EmploymentWorkTasks

        [TestMethod()]
        public void EmploymentWorkTasksUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTasks,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTasksdsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTasks,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTasksUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTasks,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkTaskssUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkTasks,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 222 EmploymentSpecialConditions

        [TestMethod()]
        public void EmploymentSpecialConditionsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SpecialConditions,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSpecialConditionsdsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SpecialConditions,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSpecialConditionsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SpecialConditions,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSpecialConditionssUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SpecialConditions,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 225 EmploymentWorkPlace

        [TestMethod()]
        public void EmploymentWorkPlaceUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkPlace,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkPlacedsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkPlace,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkPlaceUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkPlace,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentWorkPlacesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.WorkPlace,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 226 SubstituteFor

        [TestMethod()]
        public void EmploymentSubstituteForUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteFor,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteFordsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteFor,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteForUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteFor,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteForsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteFor,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 227 SubstituteForDueTo

        [TestMethod()]
        public void EmploymentSubstituteForDueToUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteForDueTo,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteForDueTodsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteForDueTo,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteForDueToUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteForDueTo,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentSubstituteForDueTosUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.SubstituteForDueTo,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 228 EmploymentEndReason

        [TestMethod()]
        public void EmploymentEndReasonSysUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentEndReason,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentEndReasonCompUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentEndReason,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "4");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentEndReasonUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentEndReason,
                new ImportEmployeeChangesTestsInputParameters(), "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentEndReasonInvalidTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentEndReason,
                new ImportEmployeeChangesTestsInputParameters(forceInvalid: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 229 BaseWorkTimeWeek

        [TestMethod()]
        public void EmploymentBaseWorkTimeWeekUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BaseWorkTimeWeek,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentBaseWorkTimeWeeksUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BaseWorkTimeWeek,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentBaseWorkTimeWeekUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BaseWorkTimeWeek,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentBaseWorkTimeWeeksUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BaseWorkTimeWeek,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 300 EmploymentPriceType

        [TestMethod()]
        public void EmploymentPriceTypeAddTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPriceType,
                new ImportEmployeeChangesTestsInputParameters(newOptionalExternalCode1: true, newOptionalExternalCode2: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPriceTypeUpdateAmountTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPriceType,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPriceTypeUpdatePriceLevelTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPriceType,
                new ImportEmployeeChangesTestsInputParameters(newOptionalExternalCode2: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPriceTypeUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPriceType,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void EmploymentPriceTypeInvalidTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.EmploymentPriceType,
                new ImportEmployeeChangesTestsInputParameters(forceInvalid: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 400+403 UserRole

        [TestMethod()]
        public void UserRoleUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRolesUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRolesUpdateDateFromTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRoleUpdateDateFromsTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRoleUpdateDateToTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRoleDateTosUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void UserRoleUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void UserRolesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.UserRole,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 401 AttestRole

        [TestMethod()]
        public void AttestRoleUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRolesUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRolesUpdateDateFromTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRoleUpdateDateFromsTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newFromDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRoletUpdateDateToTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRoleDateTosUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(newValue: false, newToDate: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void AttestRoleUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void AttestRolesUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.AttestRole,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 402 BlockedFromDate

        [TestMethod()]
        public void BlockedFromDateUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BlockedFromDate,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void BlockedFromDatesUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BlockedFromDate,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void BlockedFromDateUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.BlockedFromDate,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }


        #endregion

        #region 500 ExternalAuthId

        [TestMethod()]
        public void ExternalAuthIdUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalAuthId,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void ExternalAuthIdsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalAuthId,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void ExternalAuthIdUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalAuthId,
                new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        [TestMethod()]
        public void ExternalAuthIdsUpdateIdenticalTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.ExternalAuthId,
                new ImportEmployeeChangesTestsInputParameters(), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsFalse(hasChanges);
        }

        #endregion

        #region 600 EmployeeReports

        #region 601 PayrollStatisticsPersonalCategory

        [TestMethod()]
        public void PayrollStatisticsPersonalCategoryUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.PayrollStatisticsPersonalCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsPersonalCategoryUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.PayrollStatisticsPersonalCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsPersonalCategoryIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.PayrollStatisticsPersonalCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsPersonalCategoryDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.PayrollStatisticsPersonalCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsPersonalCategoryInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.PayrollStatisticsPersonalCategory));
        }

        #endregion

        #region 602 PayrollStatisticsWorkTimeCategory

        [TestMethod()]
        public void PayrollStatisticsWorkTimeCategoryUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.PayrollStatisticsWorkTimeCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkTimeCategoryUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.PayrollStatisticsWorkTimeCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkTimeCategoryIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.PayrollStatisticsWorkTimeCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkTimeCategoryDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.PayrollStatisticsWorkTimeCategory));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkTimeCategoryInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.PayrollStatisticsWorkTimeCategory));
        }

        #endregion

        #region 603 PayrollStatisticsSalaryType

        [TestMethod()]
        public void PayrollStatisticsSalaryTypeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.PayrollStatisticsSalaryType));
        }

        [TestMethod()]
        public void PayrollStatisticsSalaryTypeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.PayrollStatisticsSalaryType));
        }

        [TestMethod()]
        public void PayrollStatisticsSalaryTypeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.PayrollStatisticsSalaryType));
        }

        [TestMethod()]
        public void PayrollStatisticsSalaryTypeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.PayrollStatisticsSalaryType));
        }

        [TestMethod()]
        public void PayrollStatisticsSalaryTypeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.PayrollStatisticsSalaryType));
        }

        #endregion

        #region 604 PayrollStatisticsWorkPlaceNumber

        [TestMethod()]
        public void PayrollStatisticsWorkPlaceNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.PayrollStatisticsWorkPlaceNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkPlaceNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.PayrollStatisticsWorkPlaceNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkPlaceNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.PayrollStatisticsWorkPlaceNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkPlaceNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.PayrollStatisticsWorkPlaceNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsWorkPlaceNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.PayrollStatisticsWorkPlaceNumber));
        }

        #endregion

        #region 605 PayrollStatisticsWorkPlaceNumber

        [TestMethod()]
        public void PayrollStatisticsCFARNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.PayrollStatisticsCFARNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsCFARNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.PayrollStatisticsCFARNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsCFARNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.PayrollStatisticsCFARNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsCFARNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.PayrollStatisticsCFARNumber));
        }

        [TestMethod()]
        public void PayrollStatisticsCFARNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.PayrollStatisticsCFARNumber));
        }

        #endregion

        #region 611 ControlTaskWorkPlacSCB

        [TestMethod()]
        public void ControlTaskWorkPlacSCBUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.ControlTaskWorkPlaceSCB));
        }

        [TestMethod()]
        public void ControlTaskWorkPlacSCBUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.ControlTaskWorkPlaceSCB));
        }

        [TestMethod()]
        public void ControlTaskWorkPlacSCBIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.ControlTaskWorkPlaceSCB));
        }

        [TestMethod()]
        public void ControlTaskWorkPlacSCBDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.ControlTaskWorkPlaceSCB));
        }

        [TestMethod()]
        public void ControlTaskWorkPlacSCBInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.ControlTaskWorkPlaceSCB));
        }

        #endregion

        #region 612 ControlTaskPartnerInCloseCompany

        [TestMethod()]
        public void ControlTaskPartnerInCloseCompanyUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.ControlTaskPartnerInCloseCompany));
        }

        [TestMethod()]
        public void ControlTaskPartnerInCloseCompanyUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.ControlTaskPartnerInCloseCompany));
        }

        [TestMethod()]
        public void ControlTaskPartnerInCloseCompanyIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.ControlTaskPartnerInCloseCompany));
        }

        [TestMethod()]
        public void ControlTaskPartnerInCloseCompanyDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.ControlTaskPartnerInCloseCompany));
        }

        [TestMethod()]
        public void ControlTaskPartnerInCloseCompanyInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.ControlTaskPartnerInCloseCompany));
        }

        #endregion

        #region 613 ControlTaskBenefitAsPension

        [TestMethod()]
        public void ControlTaskBenefitAsPensionUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.ControlTaskBenefitAsPension));
        }

        [TestMethod()]
        public void ControlTaskBenefitAsPensionUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.ControlTaskBenefitAsPension));
        }

        [TestMethod()]
        public void ControlTaskBenefitAsPensionIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.ControlTaskBenefitAsPension));
        }

        [TestMethod()]
        public void ControlTaskBenefitAsPensionDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.ControlTaskBenefitAsPension));
        }

        [TestMethod()]
        public void ControlTaskBenefitAsPensionInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.ControlTaskBenefitAsPension));
        }

        #endregion

        #region 621 AFACategory

        [TestMethod()]
        public void AFACategoryUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AFACategory));
        }

        [TestMethod()]
        public void AFACategoryUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AFACategory));
        }

        [TestMethod()]
        public void AFACategoryIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AFACategory));
        }

        [TestMethod()]
        public void AFACategoryDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AFACategory));
        }

        [TestMethod()]
        public void AFACategoryInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AFACategory));
        }

        #endregion

        #region 622 AFASpecialAgreement

        [TestMethod()]
        public void AFASpecialAgreementUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AFASpecialAgreement));
        }

        [TestMethod()]
        public void AFASpecialAgreementUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AFASpecialAgreement));
        }

        [TestMethod()]
        public void AFASpecialAgreementIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AFASpecialAgreement));
        }

        [TestMethod()]
        public void AFASpecialAgreementDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AFASpecialAgreement));
        }

        [TestMethod()]
        public void AFASpecialAgreementInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AFASpecialAgreement));
        }

        #endregion

        #region 623 AFAWorkplaceNr

        [TestMethod()]
        public void AFAWorkplaceNrUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AFAWorkplaceNr));
        }

        [TestMethod()]
        public void AFAWorkplaceNrUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AFAWorkplaceNr));
        }

        [TestMethod()]
        public void AFAWorkplaceNrIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AFAWorkplaceNr));
        }

        [TestMethod()]
        public void AFAWorkplaceNrDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AFAWorkplaceNr));
        }

        [TestMethod()]
        public void AFAWorkplaceNrInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AFAWorkplaceNr));
        }

        #endregion

        #region 624 AFAParttimePensionCode

        [TestMethod()]
        public void AFAParttimePensionCodeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AFAParttimePensionCode));
        }

        [TestMethod()]
        public void AFAParttimePensionCodeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AFAParttimePensionCode));
        }

        [TestMethod()]
        public void AFAParttimePensionCodeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AFAParttimePensionCode));
        }

        [TestMethod()]
        public void AFAParttimePensionCodeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AFAParttimePensionCode));
        }

        [TestMethod()]
        public void AFAParttimePensionCodeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AFAParttimePensionCode));
        }

        #endregion

        #region 631 CollectumITPPlan 

        [TestMethod()]
        public void CollectumITPPlanUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.CollectumITPPlan));
        }

        [TestMethod()]
        public void CollectumITPPlanUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumITPPlan));
        }

        [TestMethod()]
        public void CollectumITPPlanIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumITPPlan));
        }

        [TestMethod()]
        public void CollectumITPPlanDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumITPPlan));
        }

        [TestMethod()]
        public void CollectumITPPlanInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumITPPlan));
        }

        #endregion

        #region 632 CollectumAgreedOnProduct  

        [TestMethod()]
        public void CollectumAgreedOnProductUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.CollectumAgreedOnProduct));
        }

        [TestMethod()]
        public void CollectumAgreedOnProductUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumAgreedOnProduct));
        }

        [TestMethod()]
        public void CollectumAgreedOnProductIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumAgreedOnProduct));
        }

        [TestMethod()]
        public void CollectumAgreedOnProductDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumAgreedOnProduct));
        }

        [TestMethod()]
        public void CollectumAgreedOnProductInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumAgreedOnProduct));
        }

        #endregion

        #region 633 CollectumCostPlace  

        [TestMethod()]
        public void CollectumCostPlaceUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.CollectumCostPlace));
        }

        [TestMethod()]
        public void CCollectumCostPlaceUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumCostPlace));
        }

        [TestMethod()]
        public void CollectumCostPlaceIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumCostPlace));
        }

        [TestMethod()]
        public void CollectumCostPlaceDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumCostPlace));
        }

        [TestMethod()]
        public void CollectumCostPlaceInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumCostPlace));
        }

        #endregion

        #region 634 CollectumCancellationDate  

        [TestMethod()]
        public void CollectumCancellationDateUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.CollectumCancellationDate));
        }

        [TestMethod()]
        public void CollectumCancellationDateUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumCancellationDate));
        }

        [TestMethod()]
        public void CollectumCancellationDateIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumCancellationDate));
        }

        [TestMethod()]
        public void CollectumCancellationDateDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumCancellationDate));
        }

        [TestMethod()]
        public void CollectumCancellationDateInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumCancellationDate));
        }

        #endregion

        #region 635 CollectumCancellationDateIsLeaveOfAbsence  

        [TestMethod()]
        public void CollectumCancellationDateIsLeaveOfAbsenceUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void CollectumCancellationDateIsLeaveOfAbsenceUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void CollectumCancellationDateIsLeaveOfAbsenceIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void CollectumCancellationDateIsLeaveOfAbsenceDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void CollectumCancellationDateIsLeaveOfAbsenceInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        #endregion

        #region 641 KPARetirementAge  

        [TestMethod()]
        public void KPARetirementAgeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.KPARetirementAge));
        }

        [TestMethod()]
        public void KPARetirementAgeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.KPARetirementAge));
        }

        [TestMethod()]
        public void KPARetirementAgeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.KPARetirementAge));
        }

        [TestMethod()]
        public void KPARetirementAgeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.KPARetirementAge));
        }

        [TestMethod()]
        public void KPARetirementAgeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.KPARetirementAge));
        }

        #endregion

        #region 642 KPABelonging  

        [TestMethod()]
        public void KPABelongingUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.KPABelonging));
        }

        [TestMethod()]
        public void KPABelongingUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.KPABelonging));
        }

        [TestMethod()]
        public void KPABelongingIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.KPABelonging));
        }

        [TestMethod()]
        public void KPABelongingDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.KPABelonging));
        }

        [TestMethod()]
        public void KPABelongingInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.KPABelonging));
        }

        #endregion

        #region 643 KPAEndCode  

        [TestMethod()]
        public void KPAEndCodeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.KPAEndCode));
        }

        [TestMethod()]
        public void KPAEndCodeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.KPAEndCode));
        }

        [TestMethod()]
        public void KPAEndCodeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.KPAEndCode));
        }

        [TestMethod()]
        public void KPAEndCodeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.KPAEndCode));
        }

        [TestMethod()]
        public void KPAEndCodeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.KPAEndCode));
        }

        #endregion

        #region 644 KPAAgreementType  

        [TestMethod()]
        public void KPAAgreementTypeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.KPAAgreementType));
        }

        [TestMethod()]
        public void KPAAgreementTypeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.KPAAgreementType));
        }

        [TestMethod()]
        public void KPAAgreementTypeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.KPAAgreementType));
        }

        [TestMethod()]
        public void KPAAgreementTypeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.KPAAgreementType));
        }

        [TestMethod()]
        public void KPAAgreementTypeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.KPAAgreementType));
        }

        #endregion

        #region 651 BygglosenAgreementArea  

        [TestMethod()]
        public void BygglosenAgreementAreaUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenAgreementArea));
        }

        [TestMethod()]
        public void BygglosenAgreementAreaUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenAgreementArea));
        }

        [TestMethod()]
        public void BygglosenAgreementAreaIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenAgreementArea));
        }

        [TestMethod()]
        public void BygglosenAgreementAreaDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenAgreementArea));
        }

        [TestMethod()]
        public void BygglosenAgreementAreaInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenAgreementArea));
        }

        #endregion

        #region 652 BygglosenAllocationNumber  

        [TestMethod()]
        public void BygglosenAllocationNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenAllocationNumber));
        }

        [TestMethod()]
        public void BygglosenAllocationNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenAllocationNumber));
        }

        [TestMethod()]
        public void BygglosenAllocationNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenAllocationNumber));
        }

        [TestMethod()]
        public void BygglosenAllocationNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenAllocationNumber));
        }

        [TestMethod()]
        public void BygglosenAllocationNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenAllocationNumber));
        }

        #endregion

        #region 653 BygglosenSalaryFormula  

        [TestMethod()]
        public void BygglosenSalaryFormulaUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenSalaryFormula));
        }

        [TestMethod()]
        public void BygglosenSalaryFormulaUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenSalaryFormula));
        }

        [TestMethod()]
        public void BygglosenSalaryFormulaIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenSalaryFormula));
        }

        [TestMethod()]
        public void BygglosenSalaryFormulaDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenSalaryFormula));
        }

        [TestMethod()]
        public void BygglosenSalaryFormulaInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenSalaryFormula));
        }

        #endregion

        #region 654 BygglosenMunicipalCode  

        [TestMethod()]
        public void BygglosenMunicipalCodeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenMunicipalCode));
        }

        [TestMethod()]
        public void BygglosenMunicipalCodeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenMunicipalCode));
        }

        [TestMethod()]
        public void BygglosenMunicipalCodeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenMunicipalCode));
        }

        [TestMethod()]
        public void BygglosenMunicipalCodeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenMunicipalCode));
        }

        [TestMethod()]
        public void BygglosenMunicipalCodeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenMunicipalCode));
        }

        #endregion

        #region 655 BygglosenProfessionCategory

        [TestMethod()]
        public void BygglosenProfessionCategoryUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenProfessionCategory));
        }

        [TestMethod()]
        public void BygglosenProfessionCategoryUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenProfessionCategory));
        }

        [TestMethod()]
        public void BygglosenProfessionCategoryIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenProfessionCategory));
        }

        [TestMethod()]
        public void BygglosenProfessionCategoryDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenProfessionCategory));
        }

        [TestMethod()]
        public void BygglosenProfessionCategoryInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenProfessionCategory));
        }

        #endregion

        #region 656 BygglosenSalaryType  

        [TestMethod()]
        public void BygglosenSalaryTypeUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenSalaryType));
        }

        [TestMethod()]
        public void BygglosenSalaryTypeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenSalaryType));
        }

        [TestMethod()]
        public void BygglosenSalaryTypeIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenSalaryType));
        }

        [TestMethod()]
        public void BygglosenSalaryTypeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenSalaryType));
        }

        [TestMethod()]
        public void BygglosenSalaryTypeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenSalaryType));
        }

        #endregion

        #region 657 BygglosenWorkPlaceNumber  

        [TestMethod()]
        public void BygglosenWorkPlaceNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenWorkPlaceNumber));
        }

        [TestMethod()]
        public void BygglosenWorkPlaceNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenWorkPlaceNumber));
        }

        [TestMethod()]
        public void BygglosenWorkPlaceNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenWorkPlaceNumber));
        }

        [TestMethod()]
        public void BygglosenWorkPlaceNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenWorkPlaceNumber));
        }

        [TestMethod()]
        public void BygglosenWorkPlaceNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenWorkPlaceNumber));
        }

        #endregion

        #region 658 BygglosenLendedToOrgNr  

        [TestMethod()]
        public void BygglosenLendedToOrgNrUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenLendedToOrgNr));
        }

        [TestMethod()]
        public void BygglosenLendedToOrgNrUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenLendedToOrgNr));
        }

        [TestMethod()]
        public void BygglosenLendedToOrgNrIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenLendedToOrgNr));
        }

        [TestMethod()]
        public void BygglosenLendedToOrgNrDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenLendedToOrgNr));
        }

        [TestMethod()]
        public void BygglosenLendedToOrgNrInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenLendedToOrgNr));
        }

        #endregion

        #region 659 BygglosenAgreedHourlyPayLevel  

        [TestMethod()]
        public void BygglosenAgreedHourlyPayLevelUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.BygglosenAgreedHourlyPayLevel));
        }

        [TestMethod()]
        public void BygglosenAgreedHourlyPayLevelUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.BygglosenAgreedHourlyPayLevel));
        }

        [TestMethod()]
        public void BygglosenAgreedHourlyPayLevelIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.BygglosenAgreedHourlyPayLevel));
        }
        [TestMethod()]
        public void BygglosenAgreedHourlyPayLevelDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.BygglosenAgreedHourlyPayLevel));
        }
        [TestMethod()]
        public void BygglosenAgreedHourlyPayLevelInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.BygglosenAgreedHourlyPayLevel));
        }

        #endregion

        #region 661 GTPAgreementNumber

        [TestMethod()]
        public void FolksamGTPAgreementNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.GTPAgreementNumber));
        }

        [TestMethod()]
        public void FolksamGTPAgreementNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.GTPAgreementNumber));
        }

        [TestMethod()]
        public void FolksamGTPAgreementNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.GTPAgreementNumber));
        }

        [TestMethod()]
        public void FolksamGTPAgreementNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.GTPAgreementNumber));
        }

        [TestMethod()]
        public void FolksamGTPAgreementNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.GTPAgreementNumber));
        }

        #endregion

        #region 662 GTPExcluded

        [TestMethod()]
        public void FolksamGTPExcludedUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.GTPExcluded));
        }

        [TestMethod()]
        public void FolksamGTPExcludedUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.GTPExcluded));
        }

        [TestMethod()]
        public void FolksamGTPExcludedIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.GTPExcluded));
        }

        [TestMethod()]
        public void FolksamGTPExcludedDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.GTPExcluded));
        }

        [TestMethod()]
        public void FolksamGTPExcludedInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.GTPExcluded));
        }

        #endregion

        #region 671 AGIPlaceOfEmploymentAddress

        [TestMethod()]
        public void AGIPlaceOfEmploymentCityUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AGIPlaceOfEmploymentCity));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentCityUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AGIPlaceOfEmploymentCity));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentCityIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AGIPlaceOfEmploymentCity));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentCityDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AGIPlaceOfEmploymentCity));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentCityInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AGIPlaceOfEmploymentCity));
        }

        #endregion

        #region 672 AGIPlaceOfEmploymentCity

        [TestMethod()]
        public void AGIPlaceOfEmploymentAddressUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AGIPlaceOfEmploymentAddress));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentAddressUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.AGIPlaceOfEmploymentAddress));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentAddressIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.AGIPlaceOfEmploymentAddress));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentAddressDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.AGIPlaceOfEmploymentAddress));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentAddressInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.AGIPlaceOfEmploymentAddress));
        }

        #endregion

        #region 673 AGIPlaceOfEmploymentIgnore

        [TestMethod()]
        public void AGIPlaceOfEmploymentIgnoreUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.AGIPlaceOfEmploymentIgnore));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentIgnoreUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentIgnoreIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentIgnoreDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        [TestMethod()]
        public void AGIPlaceOfEmploymentIgnoreInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence));
        }

        #endregion

        #region 681 AGIPlaceOfEmploymentAddress

        [TestMethod()]
        public void IFAssociationNumberUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.IFAssociationNumber));
        }

        [TestMethod()]
        public void IFAssociationNumberUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.IFAssociationNumber));
        }

        [TestMethod()]
        public void IFAssociationNumberIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.IFAssociationNumber));
        }

        [TestMethod()]
        public void IFAssociationNumberDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.IFAssociationNumber));
        }

        [TestMethod()]
        public void IFAssociationNumberInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.IFAssociationNumber));
        }

        #endregion

        #region 682 IFPaymentCode

        [TestMethod()]
        public void IFPaymentCoderUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.IFPaymentCode));
        }

        [TestMethod()]
        public void IFPaymentCodeUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.IFPaymentCode));
        }

        [TestMethod()]
        public void IFPaymentCodedenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.IFPaymentCode));
        }

        [TestMethod()]
        public void IFPaymentCodeDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.IFPaymentCode));
        }

        [TestMethod()]
        public void IFPaymentCodeInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.IFPaymentCode));
        }

        #endregion

        #region 683 IFPaymentCode

        [TestMethod()]
        public void IFWorkPlaceUpdateTest()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTest(EmployeeChangeType.IFWorkPlace));
        }

        [TestMethod()]
        public void IFWorkPlaceUpdateTests()
        {
            Assert.IsTrue(RunEmployeeReportUpdateTests(EmployeeChangeType.IFWorkPlace));
        }

        [TestMethod()]
        public void IFWorkPlaceIdenticalTest()
        {
            Assert.IsTrue(RunEmployeeReportIdenticalTest(EmployeeChangeType.IFWorkPlace));
        }

        [TestMethod()]
        public void IFWorkPlaceDeleteTest()
        {
            Assert.IsTrue(RunEmployeeReportDeleteTest(EmployeeChangeType.IFWorkPlace));
        }

        [TestMethod()]
        public void IFWorkPlaceInvalidTest()
        {
            Assert.IsTrue(RunEmployeeReportInvalidTest(EmployeeChangeType.IFWorkPlace));
        }

        #endregion

        #region 700 Vacation

        [TestMethod()]
        public void VacationDaysPaidUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationDaysPaid,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        [TestMethod()]
        public void EmploymentVacationDaysPaidsUpdateTest()
        {
            var input = new ImportEmployeeChangesTestsInput(EmployeeChangeType.VacationDaysPaid,
                new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            Assert.IsTrue(hasChanges);
        }

        #endregion

        public bool RunEmployeeReportUpdateTest(EmployeeChangeType employeeChangeType)
        {
            var input = new ImportEmployeeChangesTestsInput(employeeChangeType, new ImportEmployeeChangesTestsInputParameters(newValue: true), "1");
            bool hasChanges = RunTest(input);
            return hasChanges;
        }

        public bool RunEmployeeReportUpdateTests(EmployeeChangeType employeeChangeType)
        {
            var input = new ImportEmployeeChangesTestsInput(employeeChangeType, new ImportEmployeeChangesTestsInputParameters(newValue: true), "1", "2");
            bool hasChanges = RunTest(input);
            return hasChanges;
        }

        public bool RunEmployeeReportIdenticalTest(EmployeeChangeType employeeChangeType)
        {
            var input = new ImportEmployeeChangesTestsInput(employeeChangeType, new ImportEmployeeChangesTestsInputParameters(), "1");
            bool hasChanges = RunTest(input);
            return !hasChanges;
        }

        public bool RunEmployeeReportDeleteTest(EmployeeChangeType employeeChangeType)
        {
            var input = new ImportEmployeeChangesTestsInput(employeeChangeType, new ImportEmployeeChangesTestsInputParameters(delete: true), "1");
            bool hasChanges = RunTest(input);
            return hasChanges;
        }

        public bool RunEmployeeReportInvalidTest(EmployeeChangeType employeeChangeType)
        {
            var input = new ImportEmployeeChangesTestsInput(employeeChangeType, new ImportEmployeeChangesTestsInputParameters(forceInvalid: true), "1");
            bool hasChanges = RunTest(input);
            return !hasChanges;
        }

        #endregion

        #region Help-methods

        private bool RunTest(ImportEmployeeChangesTestsInput input)
        {
            ImportEmployeeChangesScenario scenario = ImportEmployeeChangesScenario.CreateScenario(base.GetParameterObject(0, 0));

            List<EmployeeUserDTO> employees = new List<EmployeeUserDTO>();
            List<TestImportRow> rows = new List<TestImportRow>();
            foreach (string employeeNr in input.EmployeeNrs)
            {
                EmployeeUserDTO employee = scenario.GetEmployee(employeeNr);
                if (employee == null)
                    continue;

                employees.Add(employee);
                rows.Add(scenario.GenerateChange(input, employee));
            }

            EmployeeUserImportBatch batch = scenario.ImportChangesToEmployee(employees, rows);
            return batch.HasValidChanges();
        }

        #endregion
    }
}
