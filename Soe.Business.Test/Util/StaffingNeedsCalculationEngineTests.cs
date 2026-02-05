using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.Tests
{
    [TestClass()]
    public class StaffingNeedsCalculationEngineTests
    {
        [TestMethod()]
        public void FallLikeTetrisTest()
        {
            StaffingNeedsCalculationEngine staffingNeedsCalculationEngine = new StaffingNeedsCalculationEngine(new List<TimeBreakTemplateDTO>(), null);
            List<CalculationPeriodItem> fixedRows = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> flexibleRows = new List<CalculationPeriodItem>();

            int items = 0;

            while (items < 200)
            {
                items++;

                Random r = new Random();
                int rStartMinutes = r.Next(-600, 600);
                int rStopMinutes = r.Next(0, 300);
                DateTime startTime = DateTime.Now.AddMinutes(rStartMinutes);
                DateTime stopTime = startTime.AddMinutes(rStopMinutes);
                StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(startTime, stopTime, startTime, stopTime);

                CalculationPeriodItem row = new CalculationPeriodItem()
                {
                    CalculationGuid = Guid.NewGuid(),
                    Interval = 15,
                    ShiftTypeId = 0,
                    StaffingNeedsRowGuid = Guid.NewGuid(),
                    StaffingNeedsRowPeriodId = 0,
                    StartTime = startTime,
                    Value = 1,
                    TimeSlot = timeSlot,
                    PeriodState = SoeEntityState.Active,
                    RowState = SoeEntityState.Active,
                    Name = string.Empty,
                    StaffingNeedsHeadId = 0,
                    StaffingNeedsRowId = 0,
                    OriginType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData,
                    Type = StaffingNeedsRowType.Normal,
                    Date = startTime.Date,
                    Weekday = null,
                    DayTypeId = 0,
                };

                fixedRows.Add(row);
            }

            while (items < 400)
            {
                items++;

                Random r = new Random();
                int rStartMinutes = r.Next(-600, 600);
                int rStopMinutes = r.Next(0, 300);
                DateTime startTime = DateTime.Now.AddMinutes(rStartMinutes);
                DateTime stopTime = startTime.AddMinutes(rStopMinutes);
                StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(startTime.AddMinutes(Convert.ToInt16(-120)), stopTime.AddMinutes(Convert.ToInt16(120)), startTime, stopTime);

                CalculationPeriodItem row = new CalculationPeriodItem()
                {
                    CalculationGuid = Guid.NewGuid(),
                    Interval = 15,
                    ShiftTypeId = 0,
                    StaffingNeedsRowGuid = Guid.NewGuid(),
                    StaffingNeedsRowPeriodId = 0,
                    StartTime = startTime,
                    Value = 1,
                    TimeSlot = timeSlot,
                    PeriodState = SoeEntityState.Active,
                    RowState = SoeEntityState.Active,
                    Name = string.Empty,
                    StaffingNeedsHeadId = 0,
                    StaffingNeedsRowId = 0,
                    OriginType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData,
                    Type = StaffingNeedsRowType.Normal,
                    Date = startTime.Date,
                    Weekday = null,
                    DayTypeId = 0,
                };

                flexibleRows.Add(row);
            }

            int count = flexibleRows.Where(t => t.TimeSlot.To > DateTime.Now.AddYears(10)).Count();

            fixedRows = fixedRows.OrderByDescending(o => o.TimeSlot.From).ToList();
            fixedRows.AddRange(flexibleRows);
            fixedRows = staffingNeedsCalculationEngine.FallLikeTetris(flexibleRows);

            Assert.IsTrue(items > 0);
        }

        [TestMethod()]
        public void GetRemainingPeriodItemsTest_Same()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(17, 0, 0);

            //bool valid = newPeriodItems.Count == expectedNrOfShifts;
            //if (valid && !isFixed && newPeriodItems.Count == 1 && CalendarUtility.IsNewSameAsCurrent(newPeriodItems[0].TimeSlot.From, newPeriodItems[0].TimeSlot.To, shiftPeriodItems[0].TimeSlot.From, shiftPeriodItems[0].TimeSlot.To))
            //    valid = false;
            //return valid;

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(0, needFrom, needTo, shiftFrom, shiftTo);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 0);
        }

        [TestMethod()]
        public void GetRemainingPeriodItemsTest_ShiftInsideNeed()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(10, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(14, 0, 0);

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(3, needFrom, needTo, shiftFrom, shiftTo);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 2 &&
                periodItems[0].TimeSlot.From == needFrom &&
                periodItems[0].TimeSlot.To == shiftFrom &&
                periodItems[1].TimeSlot.From == shiftTo &&
                periodItems[1].TimeSlot.To == needTo);
        }

        [TestMethod()]
        public void GetRemainingPeriodItemsTest_NeedBeforeShift()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(10, 0, 0);

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(1, needFrom, needTo, shiftFrom, shiftTo);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 1 &&
                periodItems[0].TimeSlot.From == shiftTo &&
                periodItems[0].TimeSlot.To == needTo);
        }

        [TestMethod()]
        public void GetRemainingPeriodItemsTest_NeedAfterShift()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(15, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(17, 0, 0);

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(1, needFrom, needTo, shiftFrom, shiftTo);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 1 &&
                periodItems[0].TimeSlot.From == needFrom &&
                periodItems[0].TimeSlot.To == shiftFrom);
        }

        [TestMethod()]
        public void GetRemainingPeriodItemsTest_3shiftsInNeed()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(9, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(10, 0, 0);
            DateTime shiftFrom2 = CalendarUtility.GetDateTime(12, 0, 0);
            DateTime shiftTo2 = CalendarUtility.GetDateTime(13, 0, 0);
            DateTime shiftFrom3 = CalendarUtility.GetDateTime(15, 0, 0);
            DateTime shiftTo3 = CalendarUtility.GetDateTime(17, 0, 0);

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(1, needFrom, needTo, shiftFrom, shiftTo, shiftFrom2, shiftTo2, shiftFrom3, shiftTo3);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 3 &&
                periodItems[0].TimeSlot.From == needFrom &&
                periodItems[0].TimeSlot.To == shiftFrom &&
                periodItems[1].TimeSlot.From == shiftTo &&
                periodItems[1].TimeSlot.To == shiftFrom2 &&
                periodItems[2].TimeSlot.From == shiftTo2 &&
                periodItems[2].TimeSlot.To == shiftFrom3);
        }

        [TestMethod()]
        public void GetRemainingNeedTest_NotFixed()
        {
            DateTime needFrom = CalendarUtility.GetDateTime(10, 0, 0);
            DateTime needTo = CalendarUtility.GetDateTime(14, 0, 0);
            DateTime shiftFrom = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime shiftTo = CalendarUtility.GetDateTime(10, 0, 0);

            List<CalculationPeriodItem> periodItems = GetRemainingPeriodItemsTest(1, needFrom, needTo, shiftFrom, shiftTo, isFixed: false);
            Assert.IsTrue(
                periodItems != null &&
                periodItems.Count == 1 &&
                periodItems[0].TimeSlot.From == needFrom &&
                periodItems[0].TimeSlot.To == needFrom.AddMinutes(120));
        }

        [TestMethod()]
        public void IsShiftValidAgainstScheduleCycleRulesTest()
        {
            TimeScheduleManager tsm = new TimeScheduleManager(null);
            List<ScheduleRuleEvaluationItem> previousShifts = new List<ScheduleRuleEvaluationItem>();
            ScheduleCycleDTO cycle = tsm.GetScheduleCycleWithRulesAndRuleTypes(6).ToDTO();
            EmployeePostCycle employeePostCycle = new EmployeePostCycle(null, new List<ScheduleCycleRuleDTO>());
            employeePostCycle.EmployeePost = new EmployeePostDTO();
            employeePostCycle.EmployeePost.ScheduleCycleDTO = cycle;

            ScheduleRuleEvaluationItem shiftToEvaluate = new ScheduleRuleEvaluationItem();
            shiftToEvaluate.DayOfWeek = DayOfWeek.Monday;
            shiftToEvaluate.StartTime = new DateTime(2017, 8, 14, 11, 0, 0);
            shiftToEvaluate.StopTime = new DateTime(2017, 8, 14, 16, 0, 0);

            bool firstTry = employeePostCycle.IsShiftValidAgainstScheduleCycleRules(previousShifts, shiftToEvaluate);

            previousShifts.Add(shiftToEvaluate);

            shiftToEvaluate = new ScheduleRuleEvaluationItem();
            shiftToEvaluate.DayOfWeek = DayOfWeek.Tuesday;
            shiftToEvaluate.StartTime = new DateTime(2017, 8, 14, 11, 0, 0);
            shiftToEvaluate.StopTime = new DateTime(2017, 8, 14, 16, 0, 0);

            bool secondTry = employeePostCycle.IsShiftValidAgainstScheduleCycleRules(previousShifts, shiftToEvaluate);

            Assert.IsTrue(firstTry && secondTry);
        }

        [TestMethod()]
        public void EvaluateRestTimeDayTest_SucceedPrevDay()
        {
            DateTime currentDayStart = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime currentDayStop = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime prevDayStop = CalendarUtility.GetDateTime(21, 0, 0, daysOffset: -1);
            DateTime nextDayStart = CalendarUtility.GetDateTime(3, 0, 0, daysOffset: 1);

            bool valid = EvaluateRestTimeDayTest(currentDayStart, currentDayStop, prevDayStop, nextDayStart);
            Assert.IsTrue(valid);
        }

        [TestMethod()]
        public void EvaluateRestTimeDayTest_SucceedNextDay()
        {
            DateTime currentDayStart = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime currentDayStop = CalendarUtility.GetDateTime(21, 0, 0);
            DateTime prevDayStop = CalendarUtility.GetDateTime(23, 0, 0, daysOffset: -1);
            DateTime nextDayStart = CalendarUtility.GetDateTime(8, 0, 0, daysOffset: 1);

            bool valid = EvaluateRestTimeDayTest(currentDayStart, currentDayStop, prevDayStop, nextDayStart);
            Assert.IsTrue(valid);
        }

        [TestMethod()]
        public void EvaluateRestTimeDayTest_Fail()
        {
            DateTime currentDayStart = CalendarUtility.GetDateTime(8, 0, 0);
            DateTime currentDayStop = CalendarUtility.GetDateTime(17, 0, 0);
            DateTime prevDayStop = CalendarUtility.GetDateTime(23, 0, 0, daysOffset: -1);
            DateTime nextDayStart = CalendarUtility.GetDateTime(3, 0, 0, daysOffset: 1);

            bool valid = EvaluateRestTimeDayTest(currentDayStart, currentDayStop, prevDayStop, nextDayStart);
            Assert.IsTrue(!valid);
        }

        [TestMethod()]
        public void EvaluateRestTimeWeekTest_Succeed()
        {
            DateTime weekStart = new DateTime(2017, 08, 21, 0, 0, 0);
            DateTime weekStop = new DateTime(2017, 08, 27, 23, 59, 59);
            List<ScheduleRuleEvaluationItem> shifts = new List<ScheduleRuleEvaluationItem>();
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(0), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(0), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(1), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(1), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(2), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(2), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(3), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(3), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(4), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(4), 17, 0, 0)));

            bool valid = EvaluateRestTimeWeekTest(shifts, weekStart, weekStop);
            Assert.IsTrue(valid);
        }

        [TestMethod()]
        public void EvaluateRestTimeWeekTest_Failed()
        {
            DateTime weekStart = new DateTime(2017, 08, 21, 0, 0, 0);
            DateTime weekStop = new DateTime(2017, 08, 27, 23, 59, 59);
            List<ScheduleRuleEvaluationItem> shifts = new List<ScheduleRuleEvaluationItem>();
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(0), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(0), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(1), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(1), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(2), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(2), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(3), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(3), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(4), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(4), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(5), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(5), 17, 0, 0)));
            shifts.Add(new ScheduleRuleEvaluationItem(CalendarUtility.GetDateTime(weekStart.AddDays(6), 8, 0, 0), CalendarUtility.GetDateTime(weekStart.AddDays(6), 17, 0, 0)));

            bool valid = EvaluateRestTimeWeekTest(shifts, weekStart, weekStop);
            Assert.IsTrue(!valid);
        }

        #region Help-methods

        private bool EvaluateRestTimeDayTest(DateTime currentDayStart, DateTime currentDayStop, DateTime prevDayStop, DateTime nextDayStart, int limitRestTimeDayMinutes = 660)
        {
            ScheduleRuleEvaluationItem currentDayShift = new ScheduleRuleEvaluationItem(currentDayStart, currentDayStop);
            ScheduleRuleEvaluationItem prevDayShift = new ScheduleRuleEvaluationItem(prevDayStop.AddHours(-8), prevDayStop);
            ScheduleRuleEvaluationItem nextDayShift = new ScheduleRuleEvaluationItem(nextDayStart, nextDayStart.AddHours(8));

            int restSincePrevDayBreachMinutes;
            int restToNextDayBreachMinutes;
            EmployeePostCycle employeePostCycle = new EmployeePostCycle(null, new List<ScheduleCycleRuleDTO>());
            return employeePostCycle.EvaluateRestTimeDay(limitRestTimeDayMinutes, currentDayShift, prevDayShift, nextDayShift, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes);
        }

        private bool EvaluateRestTimeWeekTest(List<ScheduleRuleEvaluationItem> shifts, DateTime weekStart, DateTime weekStop, int limitRestTimeWeekMinutes = 1800)
        {
            int maxRestTime;
            DateTime maxRestTimeStarts;
            EmployeePostCycle employeePostCycle = new EmployeePostCycle(null, new List<ScheduleCycleRuleDTO>());
            return employeePostCycle.EvaluateRestTimeWeek(limitRestTimeWeekMinutes, shifts, weekStart, weekStop, out maxRestTime, out maxRestTimeStarts);
        }

        private List<CalculationPeriodItem> GetRemainingPeriodItemsTest(int expectedNrOfShifts, DateTime needFrom, DateTime needTo, DateTime shiftFrom, DateTime shiftTo, DateTime? shiftFrom2 = null, DateTime? shiftTo2 = null, DateTime? shiftFrom3 = null, DateTime? shiftTo3 = null, bool isFixed = true)
        {
            StaffingNeedsCalculationEngine calculation = new StaffingNeedsCalculationEngine(null);

            //Need
            List<CalculationPeriodItem> needPeriodItems = new List<CalculationPeriodItem>();
            needPeriodItems.Add(new CalculationPeriodItem()
            {
                TimeSlot = new StaffingNeedsCalculationTimeSlot()
                {
                    From = needFrom,
                    To = needTo,
                    MinFrom = isFixed ? needFrom : CalendarUtility.GetDateTime(needFrom.Date, 0, 0, 0),
                    MaxTo = isFixed ? needTo : CalendarUtility.GetDateTime(needTo.Date, 23, 59, 59),
                },
                ShiftTypeId = 1,
                TimeScheduleTaskId = 1,
            });

            //Shift
            List<CalculationPeriodItem> shiftPeriodItems = new List<CalculationPeriodItem>();
            shiftPeriodItems.Add(new CalculationPeriodItem()
            {
                TimeSlot = new StaffingNeedsCalculationTimeSlot()
                {
                    From = shiftFrom,
                    To = shiftTo,
                },
                ShiftTypeId = 1,
                TimeScheduleTaskId = 1,
            });
            if (shiftFrom2.HasValue && shiftTo2.HasValue)
            {
                shiftPeriodItems.Add(new CalculationPeriodItem()
                {
                    TimeSlot = new StaffingNeedsCalculationTimeSlot()
                    {
                        From = shiftFrom2.Value,
                        To = shiftTo2.Value,
                    },
                    ShiftTypeId = 1,
                    TimeScheduleTaskId = 1,
                });
                if (shiftFrom3.HasValue && shiftTo3.HasValue)
                {
                    shiftPeriodItems.Add(new CalculationPeriodItem()
                    {
                        TimeSlot = new StaffingNeedsCalculationTimeSlot()
                        {
                            From = shiftFrom3.Value,
                            To = shiftTo3.Value,
                        },
                        ShiftTypeId = 1,
                        TimeScheduleTaskId = 1,
                    });
                }
            }

            bool isAnyNeedChanged;
            List<CalculationPeriodItem> newPeriodItems = calculation.GetRemainingPeriodItems(needPeriodItems, shiftPeriodItems, out isAnyNeedChanged);
            return newPeriodItems;
        }

        #endregion

        [TestMethod()]
        public void SolveKnapSackTest()
        {
            StaffingNeedsCalculationEngine staffingNeedsCalculationEngine = new StaffingNeedsCalculationEngine(null);
            List<Tuple<DateTime, int, Guid>> list = new List<Tuple<DateTime, int, Guid>>();
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 23), 7 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 21), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 13 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 22), 8 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 4 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 24), 5 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));
            list.Add(Tuple.Create(new DateTime(2017, 8, 26), 3 * 60, Guid.NewGuid()));

            var items = staffingNeedsCalculationEngine.SolveKnapSack(40 * 60, list);
            Assert.Fail();
        }
    }
}