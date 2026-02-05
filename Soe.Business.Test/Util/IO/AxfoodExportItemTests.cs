using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.IO.Tests
{
    [TestClass()]
    public class AxfoodExportItemTests
    {
        [TestMethod]
        public void TestSingleExtraShiftDayWithinDateRange()
        {
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = null;
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                    {
                        {
                            employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                            {
                                new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = startDate, StartTime = CalendarUtility.DATETIME_DEFAULT, StopTime = CalendarUtility.DATETIME_DEFAULT, Modified = modified }, // No shift
                                new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime =CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified } // Extra shift
                            }
                        }
                    };
            }

            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, new Dictionary<int, List<SubstituteShiftDTO>>(), start, end, employees, externalExportId, ignoreEmploymentPercent: true);

            // Assert
            // Kolla att den innehåller 19
            Assert.IsTrue(result.Contains("  19  "));
            // Kolla innehåller rätt antal rader
            Assert.AreEqual(1, result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [TestMethod]
        public void TestConsecutiveExtraShiftsShouldNotOutput()
        {
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = null;
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                {
                    {
                        employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                        {
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-1), StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified }, // Shift outside of date range to check how the first day is handled
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified }, // Extra shift yesterday
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified } // Extra shift today
                        }
                    }
                };
            }

            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, new Dictionary<int, List<SubstituteShiftDTO>>(), start, end, employees, externalExportId, ignoreEmploymentPercent: true); ;

            // Assert
            // Kolla att den inte innehåller 19 eller någon annan typ
            Assert.IsFalse(result.Contains("  19  "));
            // Kolla att resultatet är tomt
            Assert.AreEqual(0, result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [TestMethod]
        public void TestConsecutiveReplacementShiftsShouldNotOutput()
        {
            // Detta test validerar att om det är vikarietpass två dagar i rad så ska ingen rad genereras eftersom det inte har skett någon förändring i schemat.
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);
            Dictionary<int, List<SubstituteShiftDTO>> substituteShifts = new Dictionary<int, List<SubstituteShiftDTO>>();

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = null;
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                {
                    {
                        employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                        {
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-1), StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = false, Modified = modified }, // Shift outside of date range to check how the first day is handled
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = false, Modified = modified }, // Replacement shift yesterday
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = false, Modified = modified } // Replacement shift today
                        }
                    }
                };
                substituteShifts = GetSubstituteShifts(employee.EmployeeId, new List<DateTime>() { start.AddDays(-1), start, end });
            }
            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, substituteShifts, start, end, employees, externalExportId, ignoreEmploymentPercent: true);

            // Assert
            // Kolla att den inte innehåller 30 eller någon annan typ
            Assert.IsFalse(result.Contains("  30  "));
            // Kolla att resultatet är tomt
            Assert.AreEqual(0, result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [TestMethod]
        public void TestDifferentSchedulesEachDayShouldOutput()
        {
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);
            Dictionary<int, List<SubstituteShiftDTO>> substituteShifts = new Dictionary<int, List<SubstituteShiftDTO>>();

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = null;
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                {
                    {
                        employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                        {
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-1), StartTime = CalendarUtility.DATETIME_DEFAULT, StopTime = CalendarUtility.DATETIME_DEFAULT, ExtraShift = false, Modified = modified }, // No shift
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified }, // Extra shift
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(10), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(18), ExtraShift = false, Modified = modified } // Replacement shift
                        }
                    }
                };
                substituteShifts = GetSubstituteShifts(employee.EmployeeId, new List<DateTime>() { end });
            }

            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, substituteShifts, start, end, employees, externalExportId, ignoreEmploymentPercent: true);

            // Assert
            // Kolla att den innehåller 19 och 30
            Assert.IsTrue(result.Contains("  19  "));
            Assert.IsTrue(result.Contains("  30  "));
            // Kolla att resultatet innehåller två rader
            Assert.AreEqual(2, result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [TestMethod]
        public void TestThreeDifferentSchedulesShouldOutputTwoRows()
        {
            // Detta test ska validera att om vi har tre dagar med olika scheman, och den första dagen är utanför tidsintervallet, så ska de två dagarna inom intervallet generera rader.
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);
            Dictionary<int, List<SubstituteShiftDTO>> substituteShifts = new Dictionary<int, List<SubstituteShiftDTO>>();

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = null;
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                {
                    {
                        employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                        {
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-1), StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified }, // Extra shift outside of date range
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(10), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(18), ExtraShift = false, Modified = modified }, // Replacement shift within date range
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(11), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(19), ExtraShift = true, Modified = modified } // Extra shift within date range
                        }
                    }
                };

                substituteShifts = GetSubstituteShifts(employee.EmployeeId, new List<DateTime>() { start });
            }

            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, substituteShifts, start, end, employees, externalExportId, ignoreEmploymentPercent: true);

            // Assert
            // Kolla att resultatet innehåller två rader
            var resultLines = result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, resultLines.Length);
            // Kolla att den första raden är typ 30 (första dagen inom intervallet)
            Assert.IsTrue(resultLines[0].Contains("  30  "));
            // Kolla att den andra raden är typ 19 (andra dagen inom intervallet)
            Assert.IsTrue(resultLines[1].Contains("  19  "));
        }

        [TestMethod]
        public void TestOldScheduleHasBeenModifiedYesterdayResendDays()
        {
            // Detta test ska validera att om vi har tre dagar med olika scheman, och den första dagen är utanför tidsintervallet, så ska de två dagarna inom intervallet generera rader.
            // Arrange
            var employees = GetEmployees(1);
            var timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            DateTime start = DateTime.Today.AddDays(-2);
            DateTime end = DateTime.Today.AddDays(-1);
            Dictionary<int, List<SubstituteShiftDTO>> substituteShifts = new Dictionary<int, List<SubstituteShiftDTO>>();

            foreach (var employee in employees)
            {
                var startDate = start;
                DateTime? modified = DateTime.Now.AddDays(-1);
                timeEmployeeScheduleDataSmallDTODict = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>()
                {
                    {
                        employee.EmployeeId, new List<TimeEmployeeScheduleDataSmallDTO>
                        {
                                          new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-2), StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = false, Modified = null }, // shift outside of date range, not modified since last job
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start.AddDays(-1), StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(9), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(17), ExtraShift = true, Modified = modified }, // Extra shift outside of date range, should be added since it has been modified.
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = start, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(10), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(18), ExtraShift = false, Modified = modified }, // Replacement shift within date range
                            new TimeEmployeeScheduleDataSmallDTO { EmployeeId = employee.EmployeeId, Date = end, StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(11), StopTime = CalendarUtility.DATETIME_DEFAULT.AddHours(19), ExtraShift = true, Modified = modified } // Extra shift within date range
                        }
                    }
                };

                substituteShifts = GetSubstituteShifts(employee.EmployeeId, new List<DateTime>() { start.AddDays(-2), start });
            }

            string externalExportId = "1111";

            // Act
            var result = AxfoodExportItem.GetLasForOneEmploymentPercent(timeEmployeeScheduleDataSmallDTODict, substituteShifts, start, end, employees, externalExportId, ignoreEmploymentPercent: true);

            // Assert
            // Kolla att resultatet innehåller tre rader
            var resultLines = result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, resultLines.Length);
            // Kolla att den första raden är typ 30 (första dagen inom intervallet)
            Assert.IsTrue(resultLines[0].Contains("  19  "));
            // Kolla att den andra raden är typ 30 (första dagen inom intervallet)
            Assert.IsTrue(resultLines[1].Contains("  30  "));
            // Kolla att den tredje raden är typ 19 (andra dagen inom intervallet)
            Assert.IsTrue(resultLines[2].Contains("  19  "));
        }


        private List<EmployeeDTO> GetEmployees(int numberOfEmployees = 1)
        {
            List<EmployeeDTO> employees = new List<EmployeeDTO>();
            for (int i = 0; i < numberOfEmployees; i++)
            {
                var employee = new EmployeeDTO() { EmployeeId = i + 1 };
                var employment = new EmploymentDTO() { EmployeeId = i + 1, Percent = 1, DateFrom = DateTime.Now.AddDays(-10) };

                employee.Employments = new List<EmploymentDTO>() { employment };
                employees.Add(employee);
            }

            return employees;
        }

        private Dictionary<int, List<SubstituteShiftDTO>> GetSubstituteShifts(int employeeId, List<DateTime> dates)
        {
            var dict = new Dictionary<int, List<SubstituteShiftDTO>>();
            var shifts = new List<SubstituteShiftDTO>();
            foreach (var date in dates)
            {
                shifts.Add(new SubstituteShiftDTO() { EmployeeId = employeeId, Date = date });
            }

            dict.Add(employeeId, shifts);
            return dict;
        }
    }
}