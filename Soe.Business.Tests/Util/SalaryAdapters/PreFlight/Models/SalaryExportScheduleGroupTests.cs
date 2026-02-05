using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models.Tests
{
    [TestClass()]
    public class SalaryExportScheduleGroupTests
    {
        [TestMethod]
        public void MergeSchedules_ValidInput_MergesSchedulesByMonth()
        {
            // Arrange
            var schedules = new List<SalaryExportSchedule>
            {
                new SalaryExportSchedule { Date = new DateTime(2022, 1, 15), ScheduleHours = 8 },
                new SalaryExportSchedule { Date = new DateTime(2022, 1, 20), ScheduleHours = 6 },
                new SalaryExportSchedule { Date = new DateTime(2022, 2, 5), ScheduleHours = 7 }
            };

            var input = new ScheduleMergeInput(SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Month, schedules);

            var expectedMergedSchedules = new List<SalaryExportSchedule>
            {
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 1, 1),
                    ScheduleHours = 14,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedules[0], schedules[1] },
                    MergeType = SalaryExportTransactionDateMergeType.Month
                },
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 2, 1),
                    ScheduleHours = 7,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedules[2] },
                    MergeType = SalaryExportTransactionDateMergeType.Month
                }
            };

            var salaryExportScheduleGroup = new SalaryExportScheduleGroup(input);

            // Act
            salaryExportScheduleGroup.MergeSchedules(SalaryExportTransactionGroupType.ByCode);
            var mergedSchedules = salaryExportScheduleGroup.MergedSchedules;

            // Assert
            Assert.AreEqual(expectedMergedSchedules.Count, mergedSchedules.Count);
            for (int i = 0; i < expectedMergedSchedules.Count; i++)
            {
                Assert.AreEqual(expectedMergedSchedules[i].Date, mergedSchedules[i].Date);
                Assert.AreEqual(expectedMergedSchedules[i].ScheduleHours, mergedSchedules[i].ScheduleHours);
                Assert.AreEqual(expectedMergedSchedules[i].CostAllocation, mergedSchedules[i].CostAllocation);
                Assert.AreEqual(expectedMergedSchedules[i].Children.Count, mergedSchedules[i].Children.Count);
                for (int j = 0; j < expectedMergedSchedules[i].Children.Count; j++)
                {
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].Date, mergedSchedules[i].Children[j].Date);
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].ScheduleHours, mergedSchedules[i].Children[j].ScheduleHours);
                }
                Assert.AreEqual(expectedMergedSchedules[i].MergeType, mergedSchedules[i].MergeType);
            }
        }

        [TestMethod]
        public void MergeSchedules_ValidInput_MergesSchedulesByWeek()
        {
            // Arrange
            var input = new ScheduleMergeInput(SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Week, new List<SalaryExportSchedule>());
            var schedule1 = new SalaryExportSchedule { Date = new DateTime(2022, 1, 15), ScheduleHours = 8 }; // this is a Saturday
            var schedule2 = new SalaryExportSchedule { Date = new DateTime(2022, 1, 16), ScheduleHours = 6 }; // this is a Sunday
            var schedule3 = new SalaryExportSchedule { Date = new DateTime(2022, 2, 5), ScheduleHours = 7 }; // this is a Saturday but another week
            input.SalaryExportSchedules.Add(schedule1);
            input.SalaryExportSchedules.Add(schedule2);
            input.SalaryExportSchedules.Add(schedule3);

            var expectedMergedSchedules = new List<SalaryExportSchedule>
            {
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 1, 15),
                    ScheduleHours = 14,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedule1, schedule2 },
                    MergeType = SalaryExportTransactionDateMergeType.Week
                },
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 2, 5),
                    ScheduleHours = 7,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedule3 },
                    MergeType = SalaryExportTransactionDateMergeType.Week
                }
            };

            var salaryExportScheduleGroup = new SalaryExportScheduleGroup(input);

            // Act
            salaryExportScheduleGroup.MergeSchedules(SalaryExportTransactionGroupType.ByCode);
            var mergedSchedules = salaryExportScheduleGroup.MergedSchedules;

            // Assert
            Assert.AreEqual(expectedMergedSchedules.Count, mergedSchedules.Count);
            for (int i = 0; i < expectedMergedSchedules.Count; i++)
            {
                Assert.AreEqual(expectedMergedSchedules[i].Date, mergedSchedules[i].Date);
                Assert.AreEqual(expectedMergedSchedules[i].ScheduleHours, mergedSchedules[i].ScheduleHours);
                Assert.AreEqual(expectedMergedSchedules[i].CostAllocation, mergedSchedules[i].CostAllocation);
                Assert.AreEqual(expectedMergedSchedules[i].Children.Count, mergedSchedules[i].Children.Count);
                for (int j = 0; j < expectedMergedSchedules[i].Children.Count; j++)
                {
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].Date, mergedSchedules[i].Children[j].Date);
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].ScheduleHours, mergedSchedules[i].Children[j].ScheduleHours);
                }
                Assert.AreEqual(expectedMergedSchedules[i].MergeType, mergedSchedules[i].MergeType);
            }
        }

        [TestMethod]
        public void MergeSchedules_ValidInput_MergesSchedulesByDay()
        {
            // Arrange
            var input = new ScheduleMergeInput(SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Day, new List<SalaryExportSchedule>());
            var schedule1 = new SalaryExportSchedule { Date = new DateTime(2022, 1, 15), ScheduleHours = 8 };
            var schedule2 = new SalaryExportSchedule { Date = new DateTime(2022, 1, 20), ScheduleHours = 6 };
            var schedule3 = new SalaryExportSchedule { Date = new DateTime(2022, 2, 5), ScheduleHours = 7 };
            input.SalaryExportSchedules.Add(schedule1);
            input.SalaryExportSchedules.Add(schedule2);
            input.SalaryExportSchedules.Add(schedule3);

            var expectedMergedSchedules = new List<SalaryExportSchedule>
            {
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 1, 15),
                    ScheduleHours = 8,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedule1 },
                    MergeType = SalaryExportTransactionDateMergeType.Day
                },
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 1, 20),
                    ScheduleHours = 6,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedule2 },
                    MergeType = SalaryExportTransactionDateMergeType.Day
                },
                new SalaryExportSchedule
                {
                    Date = new DateTime(2022, 2, 5),
                    ScheduleHours = 7,
                    CostAllocation = null,
                    Children = new List<SalaryExportSchedule> { schedule3 },
                    MergeType = SalaryExportTransactionDateMergeType.Day
                }
            };

            var salaryExportScheduleGroup = new SalaryExportScheduleGroup(input);

            // Act
            salaryExportScheduleGroup.MergeSchedules(SalaryExportTransactionGroupType.ByCode);
            var mergedSchedules = salaryExportScheduleGroup.MergedSchedules;

            // Assert
            Assert.AreEqual(expectedMergedSchedules.Count, mergedSchedules.Count);
            for (int i = 0; i < expectedMergedSchedules.Count; i++)
            {
                Assert.AreEqual(expectedMergedSchedules[i].Date, mergedSchedules[i].Date);
                Assert.AreEqual(expectedMergedSchedules[i].ScheduleHours, mergedSchedules[i].ScheduleHours);
                Assert.AreEqual(expectedMergedSchedules[i].CostAllocation, mergedSchedules[i].CostAllocation);
                Assert.AreEqual(expectedMergedSchedules[i].Children.Count, mergedSchedules[i].Children.Count);
                for (int j = 0; j < expectedMergedSchedules[i].Children.Count; j++)
                {
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].Date, mergedSchedules[i].Children[j].Date);
                    Assert.AreEqual(expectedMergedSchedules[i].Children[j].ScheduleHours, mergedSchedules[i].Children[j].ScheduleHours);
                }
                Assert.AreEqual(expectedMergedSchedules[i].MergeType, mergedSchedules[i].MergeType);
            }
        }

    }
}