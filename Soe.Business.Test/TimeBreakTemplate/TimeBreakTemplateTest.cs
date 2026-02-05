using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SoftOne.Soe.Data;
using System.Linq;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Test.TimeBreakTemplateTest
{
    [TestClass]
    public class TimeBreakTemplateTest
    {
        private int actorCompanyId = 291;

        #region Breaks

        [TestMethod]
        public void TestBreakTemplate8to17_1major60_2minor15()
        {
            bool success = RunTest(
                new TimeBreakTemplateEvaluationInput(
                    SoeTimeBreakTemplateEvaluation.Test,
                    CalendarUtility.DATETIME_DEFAULT.AddHours(8),
                    CalendarUtility.DATETIME_DEFAULT.AddHours(17),
                    debugParameters: new TimeBreakTemplateDebugParameters(30, false, 1, 60, 60, 60, 2, 15, 60, 60))
                );
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestBreakTemplate8to17_1major60_2minor15_max2hr()
        {
            bool success = RunTest(
                new TimeBreakTemplateEvaluationInput(
                    SoeTimeBreakTemplateEvaluation.Test,
                    CalendarUtility.DATETIME_DEFAULT.AddHours(8),
                    CalendarUtility.DATETIME_DEFAULT.AddHours(17),
                    debugParameters: new TimeBreakTemplateDebugParameters(30, true, 1, 60, 60, 60, 2, 15, 60, 60))
                );
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestBreakTemplate8to17_1major60_2minor15_locked9to10()
        {
            bool success = RunTest(
                new TimeBreakTemplateEvaluationInput(
                    SoeTimeBreakTemplateEvaluation.Test,
                    CalendarUtility.DATETIME_DEFAULT.AddHours(8),
                    CalendarUtility.DATETIME_DEFAULT.AddHours(17),
                    debugParameters: new TimeBreakTemplateDebugParameters(30, false, 1, 60, 60, 60, 2, 15, 60, 60),
                    lockedTimeSlots: new List<TimeBreakTemplateTimeSlot>() { new TimeBreakTemplateTimeSlot(CalendarUtility.DATETIME_DEFAULT.AddHours(9), CalendarUtility.DATETIME_DEFAULT.AddHours(10))})
                );
            Assert.IsTrue(success);
        }

        private bool RunTest(TimeBreakTemplateEvaluationInput input)
        {
            TimeBreakTemplateEvaluation breakEvaluation = new TimeBreakTemplateEvaluation();
            TimeBreakTemplateEvaluationOutput output = breakEvaluation.Evaluate(input, GetTemplates());
            if (output.Success)
            {
                List<TimeBreakTemplateBreakSlot> breaks = output.BreakSlots;
                Debug.WriteLine(output.ToString());
            }
            else
            {
                Debug.WriteLine(Boolean.FalseString);
            }

            return output.Success;
        }

        private List<TimeBreakTemplateDTO> GetTemplates()
        {
            TimeScheduleManager tsm = new TimeScheduleManager(null);
            return tsm.GetTimeBreakTemplates(actorCompanyId).ToDTOs().ToList();
        }

        #endregion

        #region Help functions

        [TestMethod]
        public void TestOverlappingMinutes_BreakWithin()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 9, 0, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 60);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestOverlappingMinutes_BreakOverlapStart()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 7, 30, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 9, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 60);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestOverlappingMinutes_BreakOverlapStop()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 11, 0, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 13, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 60);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestOverlappingMinutes_BreakOverlapWhole()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 7, 0, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 13, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 240);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestOverlappingMinutes_BreakBeforeStart()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 7, 0, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 0);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestOverlappingMinutes_BreakAfterStop()
        {
            DateTime today = DateTime.Today;
            DateTime breakStart = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);
            DateTime breakStop = new DateTime(today.Year, today.Month, today.Day, 13, 0, 0);
            DateTime rangeStart = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0);
            DateTime rangeStop = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0);

            bool success = GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop, 0);
            Assert.IsTrue(success);
        }

        private bool GetOverlappingMinutes(DateTime breakStart, DateTime breakStop, DateTime rangeStart, DateTime rangeStop, int expectedMinutes)
        {
            int minutes = CalendarUtility.GetOverlappingMinutes(breakStart, breakStop, rangeStart, rangeStop);
            return minutes == expectedMinutes;
        }

        #endregion
    }
}
