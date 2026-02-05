using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Soe.Business.Test.RecurrencePattern
{
    [TestClass]
    public class RecurrencePatternTest
    {
        [TestMethod]
        public void TestPattern()
        {
            DailyRecurrencePatternDTO dto = new DailyRecurrencePatternDTO();
            dto.Type = DailyRecurrencePatternType.Daily;
            dto.Interval = 1;
            dto.DayOfMonth = 1;
            dto.Month = 3;
            dto.DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday };
            dto.FirstDayOfWeek = DayOfWeek.Monday;
            dto.WeekIndex = DailyRecurrencePatternWeekIndex.Second;

            string str = dto.ToString();
            Debug.WriteLine(str);

            var dto2 = DailyRecurrencePatternDTO.Parse(str);
            Debug.WriteLine(dto2);
            Assert.IsTrue(dto2 != null);
        }

        [TestMethod]
        public void TestRange()
        {
            DailyRecurrenceRangeDTO dto = new DailyRecurrenceRangeDTO();
            dto.Type = DailyRecurrenceRangeType.NoEnd;
            dto.StartDate = DateTime.Today;
            dto.EndDate = DateTime.Now.AddMonths(1);
            dto.NumberOfOccurrences = 14;

            string str = dto.ToString();
            Debug.WriteLine(str);

            var dto2 = DailyRecurrenceRangeDTO.Parse(str);
            Debug.WriteLine(dto2);
            Assert.IsTrue(dto2 != null);
        }
    }
}
