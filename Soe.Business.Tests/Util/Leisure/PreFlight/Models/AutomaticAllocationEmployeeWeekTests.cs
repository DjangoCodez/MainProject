using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation;
using SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Util.Leisure.PreFlight.Models
{
    [TestClass]
    public class AutomaticAllocationEmployeeWeekTests
    {
        [TestMethod]
        public void TestAllocateOffDays()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay  (){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6)} // Free day
                }
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            week.TryAllocateLeisureDays(leisureCodeV, new List<AutomaticAllocationEmployee>());

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            week.TryAllocateLeisureDays(leisureCodeX, new List<AutomaticAllocationEmployee>());

            var restDaysV = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "V").ToList();
            Assert.AreEqual(1, restDaysV.Count);
            Assert.IsTrue(restDaysV.All(d => d.StartTime == d.StopTime));
            var restDaysX = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "X").ToList();
            Assert.AreEqual(1, restDaysX.Count);
            Assert.IsTrue(restDaysX.All(d => d.StartTime == d.StopTime));
        }

        private AutomaticAllocationEmployeeWeek CreateWeek(DateTime monday, List<TestDay> workDays)
        {
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = monday,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>()
            };

            var date = monday;

            for (int i = 0; i < 7; i++)
            {
                var testDay = workDays.FirstOrDefault(w => w.StartTime.Date == date.Date);
                if (testDay != null)
                {
                    week.EmployeeDays.Add(new AutomaticAllocationEmployeeDay
                    {
                        Date = testDay.StartTime,
                        StartTime = testDay.StartTime,
                        StopTime = testDay.StopTime
                    });
                }
                else
                {
                    week.EmployeeDays.Add(new AutomaticAllocationEmployeeDay()
                    {
                        Date = date,
                        StartTime = date,
                        StopTime = date
                    });
                }
                date = date.AddDays(1);
            }
            return week;
        }

        private class TestDay
        {
            public TestDay(DateTime monday, DayOfWeek day, decimal startHour, decimal stopHour)
            {
                int addDays = 0;

                if (day == DayOfWeek.Tuesday)
                    addDays = 1;
                else if (day == DayOfWeek.Wednesday)
                    addDays = 2;
                else if (day == DayOfWeek.Thursday)
                    addDays = 3;
                else if (day == DayOfWeek.Friday)
                    addDays = 4;
                else if (day == DayOfWeek.Saturday)
                    addDays = 5;
                else if (day == DayOfWeek.Sunday)
                    addDays = 6;

                StartTime = monday.AddDays(addDays).AddMinutes(Convert.ToInt32(decimal.Multiply(startHour, 60)));
                StopTime = monday.AddDays(addDays).AddMinutes(Convert.ToInt32(decimal.Multiply(stopHour, 60)));

                if (StopTime < StartTime)
                {
                    StopTime = StopTime.AddDays(1);
                }
            }
            public DateTime StartTime { get; set; }
            public DateTime StopTime { get; set; }
            public override string ToString()
            {
                return $"{StartTime} - {StopTime} {StartTime.DayOfWeek}";
            }
        }

        [TestMethod]
        public void TestAllocationForStevensSimon()
        {
            // Stevens Simon's schedule: X-day on Monday, V-day on Sunday
            var monday = new DateTime(2024, 9, 23);
            var week1Days = new List<TestDay>
        {
            new TestDay(monday, DayOfWeek.Monday, 6, 14.30m),
            new TestDay(monday, DayOfWeek.Tuesday, 6, 14.30m),
            new TestDay(monday, DayOfWeek.Wednesday, 6, 14.30m),
            new TestDay(monday, DayOfWeek.Thursday, 6, 14.30m),
            new TestDay(monday, DayOfWeek.Friday, 6, 14.30m)
        };

            var week1 = CreateWeek(monday, week1Days);

            var week2Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(7), DayOfWeek.Monday, 9, 17),
                new TestDay(monday.AddDays(7), DayOfWeek.Tuesday, 9, 17),
                new TestDay(monday.AddDays(7), DayOfWeek.Wednesday, 9, 17),
                new TestDay(monday.AddDays(7), DayOfWeek.Thursday, 9, 14),
                new TestDay(monday.AddDays(7), DayOfWeek.Friday, 9, 17)
            };

            var week2 = CreateWeek(monday.AddDays(7), week2Days);

            var week3Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(14), DayOfWeek.Monday, 6, 14),
                new TestDay(monday.AddDays(14), DayOfWeek.Tuesday, 6, 14),
                new TestDay(monday.AddDays(14), DayOfWeek.Wednesday, 6, 14),
                new TestDay(monday.AddDays(14), DayOfWeek.Thursday, 6, 14),
                new TestDay(monday.AddDays(14), DayOfWeek.Friday, 6, 14)
            };

            var week3 = CreateWeek(monday.AddDays(14), week3Days);

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Extra Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Saturday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            AutomaticAllocationEmployee employee = new AutomaticAllocationEmployee
            {
                EmployeeGroupId = 1,
                EmployeeName = "Stevens Simon",
                AllocationEmployeeWeeks = new List<AutomaticAllocationEmployeeWeek>
                {
                    week1,
                    week2,
                    week3
                },
            };

            AutomaticAllocationRunner runner = new AutomaticAllocationRunner(new List<AutomaticAllocationEmployee> { employee }, new List<AutomaticAllocationLeisureCode> { leisureCodeX, leisureCodeV });

            var output = runner.Run();

            var employeeOutput = output.Employees.FirstOrDefault(e => e.EmployeeName == "Stevens Simon");
            Assert.IsNotNull(employeeOutput, "Employee output should not be null");
            var outputFirstSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(5));
            Assert.IsNotNull(outputFirstSaturday, "First Saturday output should not be null");
            Assert.AreEqual("V", outputFirstSaturday.LeisureCode.Code, "First Saturday should be allocated as V-day");
            var outputFirstSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(6));
            Assert.IsNotNull(outputFirstSunday, "First Sunday output should not be null");
            Assert.AreEqual("X", outputFirstSunday.LeisureCode.Code, "First Sunday should be allocated as X-day");
            var outputSecondSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(12));
            Assert.IsNotNull(outputSecondSaturday, "Second Saturday output should not be null");
            Assert.AreEqual("V", outputSecondSaturday.LeisureCode.Code, "Second Saturday should be allocated as V-day");
            var outputSecondSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(13));
            Assert.IsNotNull(outputSecondSunday, "Second Sunday output should not be null");
            Assert.AreEqual("X", outputSecondSunday.LeisureCode.Code, "Second Sunday should be allocated as X-day");
            var outputThirdSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(19));
            Assert.IsNotNull(outputThirdSaturday, "Third Saturday output should not be null");
            Assert.AreEqual("V", outputThirdSaturday.LeisureCode.Code, "Third Saturday should be allocated as V-day");
            var outputThirdSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(20));
            Assert.IsNotNull(outputThirdSunday, "Third Sunday output should not be null");
            Assert.AreEqual("X", outputThirdSunday.LeisureCode.Code, "Third Sunday should be allocated as X-day");
        }

        [TestMethod]
        public void TestAllocationForFroggattGraham()
        {
            // Stevens Simon's schedule: X-day on Monday, V-day on Sunday
            var monday = new DateTime(2024, 9, 23);
            var week1Days = new List<TestDay>
            {
            new TestDay(monday, DayOfWeek.Monday, 14,22),
            new TestDay(monday, DayOfWeek.Wednesday, 6, 14),
            new TestDay(monday, DayOfWeek.Thursday, 6, 14),
            new TestDay(monday, DayOfWeek.Saturday, 10.5m, 18),
            new TestDay(monday, DayOfWeek.Sunday, 8, 16)
            };

            var week1 = CreateWeek(monday, week1Days);

            var week2Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(7), DayOfWeek.Monday, 14,22),
                new TestDay(monday.AddDays(7), DayOfWeek.Tuesday, 12,10),
                new TestDay(monday.AddDays(7), DayOfWeek.Wednesday, 8,16),
                new TestDay(monday.AddDays(7), DayOfWeek.Thursday, 6,16.5m),
                new TestDay(monday.AddDays(7), DayOfWeek.Saturday, 6, 15),
                new TestDay(monday.AddDays(7), DayOfWeek.Sunday, 6, 14),
            };

            var week2 = CreateWeek(monday.AddDays(7), week2Days);

            var week3Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(14), DayOfWeek.Monday, 14, 7),
                new TestDay(monday.AddDays(14), DayOfWeek.Tuesday, 23, 7),
                new TestDay(monday.AddDays(14), DayOfWeek.Wednesday, 23, 7),
                new TestDay(monday.AddDays(14), DayOfWeek.Thursday, 23, 7),
            };

            var week3 = CreateWeek(monday.AddDays(14), week3Days);

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Extra Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Monday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            AutomaticAllocationEmployee employee = new AutomaticAllocationEmployee
            {
                EmployeeGroupId = 1,
                EmployeeName = "Froggatt Graham",
                AllocationEmployeeWeeks = new List<AutomaticAllocationEmployeeWeek>
                {
                    week1,
                    week2,
                    week3
                },
            };

            AutomaticAllocationRunner runner = new AutomaticAllocationRunner(new List<AutomaticAllocationEmployee> { employee }, new List<AutomaticAllocationLeisureCode> { leisureCodeX, leisureCodeV });

            var output = runner.Run();

            var employeeOutput = output.Employees.FirstOrDefault(e => e.EmployeeName == "Froggatt Graham");
            Assert.IsNotNull(employeeOutput, "Employee output should not be null");
            var outputFirstTuesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(1));
            Assert.IsNotNull(outputFirstTuesday, "First Tuesday output should not be null");
            Assert.AreEqual("V", outputFirstTuesday.LeisureCode.Code, "First Tuesday should be allocated as V-day");
            var outputFirstFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(4));
            Assert.IsNotNull(outputFirstFriday, "First Friday output should not be null");
            Assert.AreEqual("X", outputFirstFriday.LeisureCode.Code, "First Friday should be allocated as X-day");
            var outputSecondFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(11));
            Assert.IsNotNull(outputSecondFriday, "Second Friday output should not be null");
            Assert.AreEqual("V", outputSecondFriday.LeisureCode.Code, "Second Friday should be allocated as V-day");
            var outputThirdFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(18));
            Assert.IsNotNull(outputThirdFriday, "Third Friday output should not be null");
            Assert.AreEqual("V", outputThirdFriday.LeisureCode.Code, "Third Friday should be allocated as V-day");
            var outputThirdSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(19));
            Assert.IsNotNull(outputThirdSaturday, "Third Saturday output should not be null");
            Assert.AreEqual("X", outputThirdSaturday.LeisureCode.Code, "Third Saturday should be allocated as X-day");
            var outputThirdSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(20));
            Assert.IsNotNull(outputThirdSunday, "Third Sunday output should not be null");
            Assert.AreEqual("X", outputThirdSunday.LeisureCode.Code, "Third Sunday should be allocated as X-day");
        }

        [TestMethod]
        public void TestAllocationForLevyJane()
        {
            // Levy Jane's schedule: X-day on Monday and Tuesday, V-day on Friday
            var monday = new DateTime(2024, 9, 23);
            var week1Days = new List<TestDay>
            {
            new TestDay(monday, DayOfWeek.Wednesday, 4.5m, 12.5m),
            new TestDay(monday, DayOfWeek.Thursday, 10, 18),
            new TestDay(monday, DayOfWeek.Saturday, 14.5m, 12.5m),
            new TestDay(monday, DayOfWeek.Sunday, 10, 18)
            };

            var week1 = CreateWeek(monday, week1Days);

            var week2Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(7), DayOfWeek.Monday, 6,14),
                new TestDay(monday.AddDays(7), DayOfWeek.Tuesday, 5,13),
                new TestDay(monday.AddDays(7), DayOfWeek.Wednesday, 4.5m, 12.5m),
                new TestDay(monday.AddDays(7), DayOfWeek.Thursday, 6, 14.5m),
                new TestDay(monday.AddDays(7), DayOfWeek.Saturday, 4.5m, 13),
                new TestDay(monday.AddDays(7), DayOfWeek.Sunday, 6, 14),
            };

            var week2 = CreateWeek(monday.AddDays(7), week2Days);

            var week3Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(14), DayOfWeek.Monday, 9, 17),
                new TestDay(monday.AddDays(14), DayOfWeek.Tuesday, 8, 17), // This day should be an Annual Leave Day which is considered as work day
                new TestDay(monday.AddDays(14), DayOfWeek.Wednesday, 4.5m, 12.5m),
                new TestDay(monday.AddDays(14), DayOfWeek.Thursday, 17, 23.99m),
                new TestDay(monday.AddDays(14), DayOfWeek.Friday, 16, 23.99m),
            };

            var week3 = CreateWeek(monday.AddDays(14), week3Days);

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Extra Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Monday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            AutomaticAllocationEmployee employee = new AutomaticAllocationEmployee
            {
                EmployeeGroupId = 1,
                EmployeeName = "Levy Jane",
                AllocationEmployeeWeeks = new List<AutomaticAllocationEmployeeWeek>
                {
                    week1,
                    week2,
                    week3
                },
            };

            AutomaticAllocationRunner runner = new AutomaticAllocationRunner(new List<AutomaticAllocationEmployee> { employee }, new List<AutomaticAllocationLeisureCode> { leisureCodeX, leisureCodeV });

            var output = runner.Run();

            var employeeOutput = output.Employees.FirstOrDefault(e => e.EmployeeName == "Levy Jane");
            Assert.IsNotNull(employeeOutput, "Employee output should not be null");
            var outputFirstMonday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday);
            Assert.IsNotNull(outputFirstMonday, "First Tuesday output should not be null");
            Assert.AreEqual("X", outputFirstMonday.LeisureCode.Code, "First Monday should be allocated as X-day");
            var outputFirstTuesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(1));
            Assert.IsNotNull(outputFirstTuesday, "First Tuesday output should not be null");
            Assert.AreEqual("X", outputFirstTuesday.LeisureCode.Code, "First Tuesday should be allocated as X-day");
            var outputFirstFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(4));
            Assert.IsNotNull(outputFirstFriday, "First Friday output should not be null");
            Assert.AreEqual("V", outputFirstFriday.LeisureCode.Code, "First Friday should be allocated as V-day");
            var outputSecondFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(11));
            Assert.IsNotNull(outputSecondFriday, "Second Friday output should not be null");
            Assert.AreEqual("V", outputSecondFriday.LeisureCode.Code, "Second Friday should be allocated as V-day");
            var outputThirdSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(19));
            Assert.IsNotNull(outputThirdSaturday, "Third Saturday output should not be null");
            Assert.AreEqual("V", outputThirdSaturday.LeisureCode.Code, "Third Saturday should be allocated as V-day");
            var outputThirdSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(20));
            Assert.IsNotNull(outputThirdSunday, "Third Sunday output should not be null");
            Assert.AreEqual("X", outputThirdSunday.LeisureCode.Code, "Third Sunday should be allocated as X-day");
        }

        [TestMethod]
        public void TestAllocationForBrookerHillary()
        {
            // Brooker Hillary's schedule: V-day on Wednesday
            var monday = new DateTime(2024, 9, 23);
            var week1Days = new List<TestDay>
            {
                new TestDay(monday, DayOfWeek.Monday, 23, 7),
                new TestDay(monday, DayOfWeek.Tuesday, 23, 7),
                new TestDay(monday, DayOfWeek.Thursday, 15.5m, 23.99m),
                new TestDay(monday, DayOfWeek.Friday, 16, 23.99m),
                new TestDay(monday, DayOfWeek.Saturday, 13.5m, 22),
                new TestDay(monday, DayOfWeek.Sunday, 16, 23.99m)
            };

            var week1 = CreateWeek(monday, week1Days);

            var week2Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(7), DayOfWeek.Tuesday, 23, 7),
                new TestDay(monday.AddDays(7), DayOfWeek.Wednesday, 22, 6),
                new TestDay(monday.AddDays(7), DayOfWeek.Thursday, 23, 7),
                new TestDay(monday.AddDays(7), DayOfWeek.Saturday, 11, 19),
                new TestDay(monday.AddDays(7), DayOfWeek.Sunday, 11, 19),
            };

            var week2 = CreateWeek(monday.AddDays(7), week2Days);

            var week3Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(14), DayOfWeek.Monday, 6, 14.5m),
                new TestDay(monday.AddDays(14), DayOfWeek.Tuesday, 6, 12),
                new TestDay(monday.AddDays(14), DayOfWeek.Saturday, 8, 17),  // This day should be an Annual Leave Day which is considered as work day
                new TestDay(monday.AddDays(14), DayOfWeek.Sunday, 8, 17),  // This day should be an Annual Leave Day which is considered as work day
            };

            var week3 = CreateWeek(monday.AddDays(14), week3Days);

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Extra Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Monday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            AutomaticAllocationEmployee employee = new AutomaticAllocationEmployee
            {
                EmployeeGroupId = 1,
                EmployeeName = "Brooker Hillary",
                AllocationEmployeeWeeks = new List<AutomaticAllocationEmployeeWeek>
                {
                    week1,
                    week2,
                    week3
                },
            };

            AutomaticAllocationRunner runner = new AutomaticAllocationRunner(new List<AutomaticAllocationEmployee> { employee }, new List<AutomaticAllocationLeisureCode> { leisureCodeX, leisureCodeV });

            var output = runner.Run();

            var employeeOutput = output.Employees.FirstOrDefault(e => e.EmployeeName == "Brooker Hillary");
            Assert.IsNotNull(employeeOutput, "Employee output should not be null");
            var outputFirstWednesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(2));
            Assert.IsNotNull(outputFirstWednesday, "First Wednesday output should not be null");
            Assert.AreEqual("V", outputFirstWednesday.LeisureCode.Code, "First Wednesday should be allocated as V-day");
            var outputSecondMonday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(7));
            Assert.IsNotNull(outputSecondMonday, "Second Monday output should not be null");
            Assert.AreEqual("V", outputSecondMonday.LeisureCode.Code, "Second Monday should be allocated as V-day");
            var outputSecondFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(11));
            Assert.IsNotNull(outputSecondFriday, "Second Friday output should not be null");
            Assert.AreEqual("X", outputSecondFriday.LeisureCode.Code, "Second Friday should be allocated as X-day");
            var outputThirdWednesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(16));
            Assert.IsNotNull(outputThirdWednesday, "Third Wednesday output should not be null");
            Assert.AreEqual("X", outputThirdWednesday.LeisureCode.Code, "Third Wednesday should be allocated as X-day");
            var outputThirdThursday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(17));
            Assert.IsNotNull(outputThirdThursday, "Third Thursday output should not be null");
            Assert.AreEqual("V", outputThirdThursday.LeisureCode.Code, "Third Thursday should be allocated as V-day");
            var outputThirdFriday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(18));
            Assert.IsNotNull(outputThirdFriday, "Third Friday output should not be null");
            Assert.AreEqual("X", outputThirdFriday.LeisureCode.Code, "Third Friday should be allocated as X-day");
        }

        [TestMethod]
        public void TestAllocationForMohonPete()
        {
            // Mohon Pete's schedule: V-day on Tuesday
            var monday = new DateTime(2024, 9, 23);
            var week1Days = new List<TestDay>
            {
                new TestDay(monday, DayOfWeek.Monday, 15, 23),
                new TestDay(monday, DayOfWeek.Wednesday, 11, 19),
                new TestDay(monday, DayOfWeek.Thursday, 16, 23.99m),
                new TestDay(monday, DayOfWeek.Friday, 16, 23.99m),
                new TestDay(monday, DayOfWeek.Saturday, 15, 23),
                new TestDay(monday, DayOfWeek.Sunday, 15, 23)
            };

            var week1 = CreateWeek(monday, week1Days);

            var week2Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(7), DayOfWeek.Tuesday, 16, 23.99m),
                new TestDay(monday.AddDays(7), DayOfWeek.Thursday, 16, 23.99m),
                new TestDay(monday.AddDays(7), DayOfWeek.Friday, 16, 23.99m),
            };

            var week2 = CreateWeek(monday.AddDays(7), week2Days);

            var week3Days = new List<TestDay>
            {
                new TestDay(monday.AddDays(14), DayOfWeek.Monday, 8, 16),
                new TestDay(monday.AddDays(14), DayOfWeek.Tuesday, 10, 18),
                new TestDay(monday.AddDays(14), DayOfWeek.Wednesday, 19, 2),
                new TestDay(monday.AddDays(14), DayOfWeek.Friday, 22.5m, 7),
                new TestDay(monday.AddDays(14), DayOfWeek.Saturday, 22, 7),
                new TestDay(monday.AddDays(14), DayOfWeek.Sunday, 18.5m, 2),
            };

            var week3 = CreateWeek(monday.AddDays(14), week3Days);

            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Extra Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Monday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };

            AutomaticAllocationEmployee employee = new AutomaticAllocationEmployee
            {
                EmployeeGroupId = 1,
                EmployeeName = "Mohon Pete",
                AllocationEmployeeWeeks = new List<AutomaticAllocationEmployeeWeek>
                {
                    week1,
                    week2,
                    week3
                },
            };

            AutomaticAllocationRunner runner = new AutomaticAllocationRunner(new List<AutomaticAllocationEmployee> { employee }, new List<AutomaticAllocationLeisureCode> { leisureCodeX, leisureCodeV });

            var output = runner.Run();

            var employeeOutput = output.Employees.FirstOrDefault(e => e.EmployeeName == "Mohon Pete");
            Assert.IsNotNull(employeeOutput, "Employee output should not be null");
            var outputFirstTuesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(1));
            Assert.IsNotNull(outputFirstTuesday, "First Tuesday output should not be null");
            Assert.AreEqual("V", outputFirstTuesday.LeisureCode.Code, "First Wednesday should be allocated as V-day");
            var outputSecondMonday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(7));
            Assert.IsNotNull(outputSecondMonday, "Second Monday output should not be null");
            Assert.AreEqual("X", outputSecondMonday.LeisureCode.Code, "Second Monday should be allocated as X-day");
            var outputSecondWednesday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(9));
            Assert.IsNotNull(outputSecondWednesday, "Second Wednesday output should not be null");
            Assert.AreEqual("X", outputSecondWednesday.LeisureCode.Code, "Second Friday should be allocated as X-day");
            var outputSecondSaturday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(12));
            Assert.IsNotNull(outputSecondSaturday, "Second Saturday output should not be null");
            Assert.AreEqual("V", outputSecondSaturday.LeisureCode.Code, "Second Friday should be allocated as V-day");
            var outputSecondSunday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(13));
            Assert.IsNotNull(outputSecondSunday, "Second Sunday output should not be null");
            Assert.AreEqual("X", outputSecondSunday.LeisureCode.Code, "Second Sunday should be allocated as X-day");
            var outputThirdThursday = employeeOutput.AllocationEmployeeDayOutputs.FirstOrDefault(d => d.Date == monday.AddDays(17));
            Assert.IsNotNull(outputThirdThursday, "Third Thursday output should not be null");
            Assert.AreEqual("V", outputThirdThursday.LeisureCode.Code, "Third Thursday should be allocated as V-day");
        }

        [TestMethod]
        public void TestAllocationOfSimpleWeek()
        {
            var monday = new DateTime(2025, 3, 31);
            var testDays = new List<TestDay>
            {
                new TestDay(monday, DayOfWeek.Monday, 9, 17),
                new TestDay(monday, DayOfWeek.Tuesday, 9, 17),
                new TestDay(monday, DayOfWeek.Wednesday, 9, 17),
                new TestDay(monday, DayOfWeek.Thursday, 9, 17),
                new TestDay(monday, DayOfWeek.Friday, 9, 17),
                new TestDay(monday, DayOfWeek.Saturday, 0, 0), // Free day
                new TestDay(monday, DayOfWeek.Sunday, 0, 0) // Free day
            };

            var week = CreateWeek(monday, testDays);
            var leisureCode = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };
            week.TryAllocateLeisureDays(leisureCode, new List<AutomaticAllocationEmployee>());
            Assert.AreEqual(1, week.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "V"));
        }

        [TestMethod]
        public void TestAllocationOfThreeWeeks()
        {
            var m = new DateTime(2025, 3, 31);
            var week1 = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5).AddHours(9), StopTime = m.AddDays(5).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6)} // Free day
                }
            };
            m = m.AddDays(7);
            var week2 = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4), StopTime = m.AddDays(4) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6)} // Free day
                }
            };
            m = m.AddDays(7);
            var week3 = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6)} // Free day
                }
            };
            var leisureCodeV = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1,
                EmployeeGroupId = 1,
            };
            var leisureCodeX = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.X,
                Code = "X",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Monday,
                RequiredDaysPerWeek = 3,
                EmployeeGroupId = 1,
            };
            week1.AllocateLeisureDays(leisureCodeV, new List<AutomaticAllocationUnAssignedRestDay>(), new List<AutomaticAllocationEmployee>());
            week1.AllocateLeisureDays(leisureCodeX, new List<AutomaticAllocationUnAssignedRestDay>(), new List<AutomaticAllocationEmployee>());
            Assert.AreEqual(1, week1.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "V"));
            Assert.AreEqual(0, week1.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "X"));
            week2.AllocateLeisureDays(leisureCodeV, week1.GetMoveableUnAssignedRestDays(), new List<AutomaticAllocationEmployee>());
            week2.AllocateLeisureDays(leisureCodeX, week1.GetMoveableUnAssignedRestDays(), new List<AutomaticAllocationEmployee>());
            Assert.AreEqual(1, week2.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "V"));
            Assert.AreEqual(2, week2.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "X"));
            week3.AllocateLeisureDays(leisureCodeV, week2.GetMoveableUnAssignedRestDays(), new List<AutomaticAllocationEmployee>());
            week3.AllocateLeisureDays(leisureCodeX, week2.GetMoveableUnAssignedRestDays(), new List<AutomaticAllocationEmployee>());
            Assert.AreEqual(1, week3.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "V"));
            Assert.AreEqual(1, week3.EmployeeDays.Count(d => d.Output != null && d.Output.LeisureCode.Code == "X"));
        }

        [TestMethod]
        public void TestAllocateOffDays_NoFreeDays()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5).AddHours(9), StopTime = m.AddDays(5).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6).AddHours(9), StopTime = m.AddDays(6).AddHours(17) }
                }
            };

            var leisureCode = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1
            };

            week.TryAllocateLeisureDays(leisureCode, new List<AutomaticAllocationEmployee>());

            var restDays = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "V").ToList();
            Assert.AreEqual(0, restDays.Count);
        }

        [TestMethod]
        public void TestAllocateOffDays_PreferredDayNotAvailable()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6).AddHours(9), StopTime = m.AddDays(6).AddHours(17) }
                }
            };

            var leisureCode = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1
            };

            week.TryAllocateLeisureDays(leisureCode, new List<AutomaticAllocationEmployee>());

            var restDays = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "V").ToList();
            Assert.AreEqual(1, restDays.Count);
            Assert.AreEqual(DayOfWeek.Saturday, restDays.First().Date.DayOfWeek);
        }

        [TestMethod]
        public void TestAllocateOffDays_PreferredDayAvailable()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6) } // Free day
                }
            };

            var leisureCode = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1
            };

            week.TryAllocateLeisureDays(leisureCode, new List<AutomaticAllocationEmployee>());

            var restDays = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "V").ToList();
            Assert.AreEqual(1, restDays.Count);
            Assert.AreEqual(DayOfWeek.Sunday, restDays.First().Date.DayOfWeek);
        }

        [TestMethod]
        public void TestAllocateOffDays_EvenDistribution()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6) } // Free day
                }
            };

            var leisureCode = new AutomaticAllocationLeisureCode
            {
                Type = LeisureCodeType.V,
                Code = "V",
                Name = "Weekly Rest",
                PreferredDay = DayOfWeek.Sunday,
                RequiredDaysPerWeek = 1
            };

            week.TryAllocateLeisureDays(leisureCode, new List<AutomaticAllocationEmployee>());

            var restDays = week.EmployeeDays.Where(d => d.Output != null && d.Output.LeisureCode.Code == "V").ToList();
            Assert.AreEqual(1, restDays.Count);
            Assert.AreEqual(DayOfWeek.Sunday, restDays.First().Date.DayOfWeek);
        }
    }
}
