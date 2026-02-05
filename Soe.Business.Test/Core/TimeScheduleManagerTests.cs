using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeScheduleManagerTests : TestBase
    {
        [TestMethod()]
        public void CreateTimeScheduleTemplateHeadTest()
        {
            TimeScheduleManager m = new TimeScheduleManager(null);
            var head = m.CreateScheduleFromEmployeePost(1562, 5, new DateTime(2017, 02, 27));
            Assert.IsTrue(head != null);
        }


        [TestMethod()]
        public void CreateTimeScheduleTemplateHeadsTest()
        {
            TimeScheduleManager m = new TimeScheduleManager(null);
            List<int> employeePostIds = new List<int>();
            employeePostIds.Add(54);
            employeePostIds.Add(53);
            SoeProgressInfo info = new SoeProgressInfo(Guid.NewGuid());
            var head = m.CreateScheduleFromEmployeePosts(90, employeePostIds, new DateTime(2017, 02, 27), ref info);
            Assert.IsTrue(head != null);
        }

        [TestMethod()]
        public void MergeShiftsOnDemoCompany()
        {
            TimeScheduleManager m = new TimeScheduleManager(null);
            Random random = new Random();
            ConfigurationSetupUtil.Init();
            List<Employee> employees = new List<Employee>();
            using (CompEntities entities = new CompEntities())
            {
                employees = entities.Employee.Where(w => w.ActorCompanyId == 1750).ToList();
            }
            DateTime dateFrom = DateTime.Today;
            DateTime dateTo = new DateTime(2021, 12, 31);
            int count = 0;

            foreach (var employee in employees)
            {
                using (CompEntities entities = new CompEntities())
                {
                    count++;
                    var shifts = m.GetShifts(employee.EmployeeId, CalendarUtility.GetDatesInInterval(dateFrom, dateTo));
                    var changedShifts = new List<TimeScheduleTemplateBlock>();

                    var periodIds = shifts.Where(w => w.TimeScheduleTemplatePeriodId.HasValue).Select(s => s.TimeScheduleTemplatePeriodId.Value).Distinct().ToList();
                    var templateBlocks = entities.TimeScheduleTemplateBlock.Where(w => w.TimeScheduleTemplatePeriodId.HasValue && periodIds.Contains(w.TimeScheduleTemplatePeriodId.Value)).ToList();
                    templateBlocks = templateBlocks.Where(w => !w.Date.HasValue && w.ModifiedBy != "mergejob").ToList();

                    foreach (var date in CalendarUtility.GetDatesInInterval(dateFrom, dateTo))
                    {
                        var onDate = shifts.Where(w => w.Date == date).ToList();

                        if (onDate.Where(w => !w.IsBreak).Count() > 2)
                        {
                            var templatePeriodOnDate = onDate.First().TimeScheduleTemplatePeriodId.Value;

                            var firstShift = onDate.OrderBy(o => o.StartTime).First();
                            var lastShift = onDate.OrderBy(o => o.StopTime).Last();
                            var inBetweenShifts = onDate.Where(w => !w.IsBreak && w.StartTime != firstShift.StartTime && w.StartTime != lastShift.StartTime).ToList();

                            if (!inBetweenShifts.IsNullOrEmpty())
                            {
                                var templatesOndate = templateBlocks.Where(w => w.TimeScheduleTemplatePeriodId == templatePeriodOnDate);

                                var middleTime = CalendarUtility.GetMiddleTime(firstShift.StopTime, lastShift.StartTime);

                                middleTime = middleTime.AddMinutes(new Random().Next(-100, 100));
                                middleTime = CalendarUtility.AdjustAccordingToInterval(middleTime, 0, 15);

                                if (templatesOndate.Any(a => a.CreatedBy == "middletime"))
                                    middleTime = templatesOndate.First(a => a.CreatedBy == "middletime").StopTime;

                                if (middleTime > firstShift.StartTime && middleTime < lastShift.StartTime)
                                {
                                    var firstTemplate = templateBlocks.FirstOrDefault(w => !w.IsBreak && w.TimeScheduleTemplatePeriodId == firstShift.TimeScheduleTemplatePeriodId && w.StartTime == firstShift.StartTime);
                                    var lastTemplate = templateBlocks.FirstOrDefault(w => !w.IsBreak && w.TimeScheduleTemplatePeriodId == lastShift.TimeScheduleTemplatePeriodId && w.StopTime == lastShift.StopTime);

                                    if (firstTemplate == null || lastTemplate == null)
                                        continue;

                                    var inBetweenTemplates = templateBlocks.Where(w => inBetweenShifts.Select(s => s.TimeScheduleTemplatePeriodId).Contains(w.TimeScheduleTemplatePeriodId) && !w.IsBreak && w.StartTime != firstTemplate.StartTime && w.StartTime != lastTemplate.StartTime).ToList();
                                    firstShift.StopTime = middleTime;
                                    lastShift.StartTime = middleTime;
                                    firstTemplate.StopTime = middleTime;
                                    firstTemplate.CreatedBy = "middletime";
                                    lastTemplate.StartTime = middleTime;

                                    firstShift.ModifiedBy = "mergejob";
                                    lastShift.ModifiedBy = "mergejob";
                                    firstTemplate.ModifiedBy = "mergejob";
                                    lastTemplate.ModifiedBy = "mergejob";

                                    changedShifts.Add(firstShift);
                                    if (!changedShifts.Any(a => a == firstTemplate))
                                        changedShifts.Add(firstTemplate);
                                    changedShifts.Add(lastShift);
                                    if (!changedShifts.Any(a => a == lastTemplate))
                                        changedShifts.Add(lastTemplate);

                                    inBetweenShifts.ForEach(f => f.State = (int)SoeEntityState.Deleted);
                                    inBetweenShifts.ForEach(f => f.ModifiedBy = "mergejob");
                                    inBetweenTemplates.ForEach(f => f.State = (int)SoeEntityState.Deleted);
                                    inBetweenTemplates.ForEach(f => f.ModifiedBy = "mergejob");
                                    changedShifts.AddRange(inBetweenShifts.Where(w => !changedShifts.Contains(w)));
                                    changedShifts.AddRange(inBetweenTemplates.Where(w => !changedShifts.Contains(w)));
                                }
                            }
                        }
                    }

                    entities.BulkUpdate(changedShifts);
                }
                GC.Collect();
            }

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void GetUnscheduledStaffingNeedsTaskTest()
        {
            List<int> shiftTypeIds = new List<int>();
            DateTime fromDate = new DateTime(2021, 10, 01);
            DateTime toDate = new DateTime(2021, 10, 30);
            int actorCompanyId = 1992; //ICA
            TimeScheduleManager m = new TimeScheduleManager(null);
            var tasks = m.GetUnscheduledStaffingNeedsTasks(actorCompanyId, shiftTypeIds, fromDate, toDate, SoeStaffingNeedType.Employee);
            Assert.IsTrue(tasks != null);
        }

        [TestMethod()]
        public void GetUnscheduledStaffingNeedsTaskDatesTest()
        {
            List<int> shiftTypeIds = new List<int>();
            DateTime fromDate = new DateTime(2017, 06, 01);
            DateTime toDate = new DateTime(2017, 06, 30);
            int actorCompanyId = 1562;
            TimeScheduleManager m = new TimeScheduleManager(null);
            var tasks = m.GetUnscheduledStaffingNeedsTaskDates(actorCompanyId, shiftTypeIds, fromDate, toDate, SoeStaffingNeedType.Employee);
            Assert.IsTrue(tasks != null);
        }

        [TestMethod()]
        public void CreateAndSaveBreaksFromTemplatesForEmployeesTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            int actorCompanyId = 90;
            int userId = 4;
            var m = new TimeScheduleManager(GetParameterObject(actorCompanyId, userId));
            var tasks = m.CreateAndSaveBreaksFromTemplatesForEmployees(new DateTime(2018, 9, 14), new DateTime(2018, 9, 14), new List<int>() { 94 }, actorCompanyId);
            Assert.IsTrue(tasks != null);
        }

        [TestMethod()]
        public void ResetAccountingAccordingToShiftTypeTest()
        {
            int actorCompanyId = 1292;
            TimeScheduleManager tsm = new TimeScheduleManager(null);
            var result = tsm.ResetAccountingAccordingToShiftType(actorCompanyId, new DateTime(2018, 1, 1), new DateTime(2020, 9, 30));
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ValidateShiftTypeAccountInternalForHierachyTest()
        {
            int actorCompanyId = 701609;
            TimeScheduleManager tsm = new TimeScheduleManager(null);
            tsm.ValidateShiftTypeAccountInternalForHierachy(actorCompanyId, tsm.GetTimeStaffingShiftAccountDimId(701609));
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void CreateFakeStaffingneedsFrequancyTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            int actorCompanyId = 1750;
            int userId = 4;
            var m = new TimeScheduleManager(GetParameterObject(actorCompanyId, userId));
            var result = m.CreateFakeStaffingneedsFrequancy(actorCompanyId, new DateTime(2024, 04, 01), new DateTime(2024, 04, 10));
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetTimeEmployeeScheduleSmallDTOForReportTest()
        {
            int actorCompanyId = 1292;
            TimeScheduleManager tsm = new TimeScheduleManager(null);
            EmployeeManager em = new EmployeeManager(null);
            var employees = em.GetAllEmployees(actorCompanyId, true, true);
            var data = tsm.GetTimeEmployeeScheduleSmallDTODictForReport(new DateTime(2019, 1, 1), new DateTime(2019, 12, 31), employees, actorCompanyId, 0, null);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetPreAnalysisInformationTest()
        {           
            DateTime fromDate = new DateTime(2020, 3, 23);
            int actorCompanyId = 451;
            int userId = 322;
            var param = GetParameterObject(actorCompanyId, userId);
            var tcm = new TimeCodeManager(param);
            var tsm = new TimeScheduleManager(param);
            var cm = new CalendarManager(param);
            var sm = new SettingManager(param);
            var info = new SoeProgressInfo(Guid.NewGuid(), SoeProgressInfoType.ScheduleEmployeePost, actorCompanyId);
            var userMock = ObjectDumper.Dump(param.SoeUser, DumpStyle.CSharp);
            var companyMock = ObjectDumper.Dump(param.SoeCompany, DumpStyle.CSharp);
            var groups = tcm.GetTimeCodeBreakGroups(param.ActorCompanyId).ToDTOs();
            var timeCodeBreakGroupsMock = ObjectDumper.Dump(groups, DumpStyle.CSharp);
            var allEmployeePosts = tsm.GetEmployeePosts(actorCompanyId, true, loadRelations: true).ToDTOs(true).ToList();
            var allEmployeePostMock = ObjectDumper.Dump(allEmployeePosts, new DumpOptions() { DumpStyle = DumpStyle.CSharp, SetPropertiesOnly = true });
            var breakTemplates = tsm.GetTimeBreakTemplates(actorCompanyId).ToDTOs().ToList();
            var breakTemplateMock = ObjectDumper.Dump(breakTemplates, DumpStyle.CSharp);
            var timeCodeBreaks = tcm.GetTimeCodes(actorCompanyId, SoeTimeCodeType.Break, loadPayrollProducts: false).OfType<TimeCode>().ToList().ToBreakDTOs();
            var timeCodeBreakMock = ObjectDumper.Dump(timeCodeBreaks, DumpStyle.CSharp);
            var ruleDTOs = tsm.GetScheduleCycleWithRulesAndRuleTypesFromCompany(actorCompanyId).ToDTOs().SelectMany(s => s.ScheduleCycleRuleDTOs).Distinct().ToList();
            var ruleDTOMock = ObjectDumper.Dump(ruleDTOs, DumpStyle.CSharp);
            var staffingNeedsCalculationStartEngine = new StaffingNeedsCalculationStartEngine(breakTemplates, timeCodeBreaks, ruleDTOs);
            int firstCommonNumberOfWeeks = StaffingNeedsCalculationEngine.GetFirstCommonNumberOfWeeks(allEmployeePosts);
            int daysForward = firstCommonNumberOfWeeks * 7;
            var shiftTypes = tsm.GetShiftTypes(actorCompanyId, loadAccounts: true, loadSkills: true, loadEmployeeStatisticsTargets: false, setTimeScheduleTemplateBlockTypeName: false, setCategoryNames: false).ToDTOs(includeAccounts: true, includeSkills: true, includeShiftTypeLinkIds: true).ToList();
            var shiftTypeMock = ObjectDumper.Dump(shiftTypes, DumpStyle.CSharp);
            var timeScheduleTasks = tsm.GetTimeScheduleTasks(actorCompanyId, fromDate, fromDate.AddDays(daysForward), loadType: true, loadShiftType: true, doIncludeRemovedDates: true, loadAccounting: true).ToDTOs(true).ToList();
            var timeScheduleTaskMock = ObjectDumper.Dump(timeScheduleTasks, DumpStyle.CSharp);
            var openingHours = cm.GetOpeningHoursForCompany(actorCompanyId).ToDTOs();
            var openingHoursMock = ObjectDumper.Dump(openingHours, DumpStyle.CSharp);
            var incomingDeliveryHeads = tsm.GetIncomingDeliveries(actorCompanyId, fromDate, fromDate.AddDays(daysForward), doIncludeRemovedDates: true, loadRows: true, loadAccounting: true).ToDTOs(true, true).ToList();
            var incomingDeliveryHeadMock = ObjectDumper.Dump(incomingDeliveryHeads, DumpStyle.CSharp);
            var shifts = tsm.GetTemplateShiftsForEmployeePost(actorCompanyId, 0, 0, fromDate, fromDate.AddDays(daysForward), false, allEmployeePosts.Select(i => i.EmployeePostId).ToList(), false);
            var shiftMock = ObjectDumper.Dump(shifts, new DumpOptions() { DumpStyle = DumpStyle.CSharp, SetPropertiesOnly = true });
            var shiftsStaffingNeedsRowPeriodId = shifts.Where(w => w.StaffingNeedsRowPeriodId.HasValue).Select(s => s.StaffingNeedsRowPeriodId.Value).ToList();
            int timeCodeId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
            int interval = tsm.GetStaffingNeedsIntervalMinutes(actorCompanyId, 1);

            #region Get StaffingNeedsHead for all WeekDays

            var needHeads = tsm.GenerateStaffingNeedsHeads(TermGroup_StaffingNeedHeadsFilterType.BaseNeed, StaffingNeedsHeadType.NeedsPlanning, fromDate, fromDate.AddDays(6));
            var needHeadsMock = ObjectDumper.Dump(needHeads, new DumpOptions() { DumpStyle = DumpStyle.CSharp, SetPropertiesOnly = true });
            var needHeads2 = tsm.GetStaffingNeedsHeads(actorCompanyId, StaffingNeedsHeadType.NeedsPlanning).ToDTOs(true, true, true, true).ToList();
            var needHeadsMock2 = ObjectDumper.Dump(needHeads2, new DumpOptions() { DumpStyle = DumpStyle.CSharp, SetPropertiesOnly = true });
            var shiftHeads = tsm.GetStaffingNeedsHeads(actorCompanyId, StaffingNeedsHeadType.NeedsShifts).ToDTOs(true, true, true, true).ToList();
            var shiftHeadsMock = ObjectDumper.Dump(shiftHeads, new DumpOptions() { DumpStyle = DumpStyle.CSharp, SetPropertiesOnly = true });

            // Will pick all StaffingNeedsHeads for every day of the week.
            var validNeedHeads = needHeads.ToList();
            var validNeedHeadsOld = new List<StaffingNeedsHeadDTO>();
            var validNeedShiftHeads = new List<StaffingNeedsHeadDTO>();
            foreach (DayOfWeek dayOfWeek in EnumUtility.GetValues<DayOfWeek>())
            {
                var validNeedHeadForDay = needHeads2.Filter(dayOfWeek, fromDate).OrderByDescending(i => i.FromDate).ThenByDescending(i => i.Created).FirstOrDefault();
                if (validNeedHeadForDay != null)
                    validNeedHeadsOld.Add(validNeedHeadForDay);

                validNeedShiftHeads.AddRange(shiftHeads.Filter(dayOfWeek, fromDate).OrderBy(h => h.FromDate).ToList());
            }

            List<int> filteredStaffingNeedsRowPeriodId = new List<int>();
            foreach (var shiftHead in validNeedShiftHeads)
            {
                foreach (var row in shiftHead.Rows.Where(i => i.State == SoeEntityState.Active))
                {
                    foreach (var period in row.Periods.Where(i => i.State == SoeEntityState.Active))
                    {
                        if (shiftsStaffingNeedsRowPeriodId.Contains(period.StaffingNeedsRowPeriodId))
                            filteredStaffingNeedsRowPeriodId.Add(period.StaffingNeedsRowPeriodId);
                    }
                }
            }

            #endregion

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void UpdateScheduledTimeSummaryTest()
        {
            List<int> shiftTypeIds = new List<int>();
            DateTime fromDate = new DateTime(2022, 03, 01);
            DateTime toDate = new DateTime(2022, 03, 31);
            int actorCompanyId = 1300;
            int employeeId = 22842;

            TimeScheduleManager m = new TimeScheduleManager(null);
            using (CompEntities entities = new CompEntities())
            {
                var company = entities.Company.Where(w => w.ActorCompanyId == actorCompanyId).ToList();
                DateTime start = DateTime.Now;
                var valueNew = m.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId, fromDate, toDate, true);
                Console.WriteLine(valueNew + " " + (DateTime.Now - start).TotalMilliseconds);
                start = DateTime.Now;
                var valueOld = m.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId, fromDate, toDate, true);
                Console.WriteLine(valueOld + " " + (DateTime.Now - start).TotalMilliseconds);
            }

            Assert.IsTrue(true);
        }
    }
}