using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.AnnualLeave.PreFlight;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoftOne.Soe.Business.Core.TimeEngine.TimeEngine;

namespace SoftOne.Soe.Business.Util.AnnualLeave.PreFlight.Tests
{
    [TestClass()]
    public class AnnualLeaveCalculationTests
    {
        [TestMethod()]
        public void GetAnnualLeaveGroupLimits_Commercial_ReturnsExpectedLimits()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();

            // Act
            var limits = annualLeaveCalculation.GetAnnualLeaveGroupLimits(TermGroup_AnnualLeaveGroupType.Commercial);

            // Assert
            Assert.HasCount(9, limits);
            Assert.AreEqual(200 * 60, limits[0].WorkedMinutes);
            Assert.AreEqual(1, limits[0].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(7.5 * 60), limits[0].NbrOfMinutesAnnualLeave);
            Assert.AreEqual(1560 * 60, limits[8].WorkedMinutes);
            Assert.AreEqual(9, limits[8].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(67.5 * 60), limits[8].NbrOfMinutesAnnualLeave);
        }

        [TestMethod()]
        public void GetAnnualLeaveGroupLimits_HotelRestaurant_ReturnsExpectedLimits()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();

            // Act
            var limits = annualLeaveCalculation.GetAnnualLeaveGroupLimits(TermGroup_AnnualLeaveGroupType.HotelRestaurant);

            // Assert
            Assert.HasCount(9, limits);
            Assert.AreEqual(200 * 60, limits[0].WorkedMinutes);
            Assert.AreEqual(1, limits[0].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(7.5 * 60), limits[0].NbrOfMinutesAnnualLeave);
            Assert.AreEqual(1640 * 60, limits[8].WorkedMinutes);
            Assert.AreEqual(9, limits[8].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(67.5 * 60), limits[8].NbrOfMinutesAnnualLeave);
        }

        [TestMethod()]
        public void GetAnnualLeaveGroupLimits_HotelRestaurantNight_ReturnsExpectedLimits()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();

            // Act
            var limits = annualLeaveCalculation.GetAnnualLeaveGroupLimits(TermGroup_AnnualLeaveGroupType.HotelRestaurantNight);

            // Assert
            Assert.HasCount(8, limits);
            Assert.AreEqual(160 * 60, limits[0].WorkedMinutes);
            Assert.AreEqual(1, limits[0].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(6 * 60), limits[0].NbrOfMinutesAnnualLeave);
            Assert.AreEqual(1280 * 60, limits[7].WorkedMinutes);
            Assert.AreEqual(8, limits[7].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(48 * 60), limits[7].NbrOfMinutesAnnualLeave);
        }

        [TestMethod()]
        public void AddLimitToList_AddsCorrectLimit()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var limits = new List<AnnualLeaveGroupLimitDTO>();

            // Act
            annualLeaveCalculation.AddLimitToList(limits, 100, 2, 8.5);

            // Assert
            Assert.HasCount(1, limits);
            Assert.AreEqual(100 * 60, limits[0].WorkedMinutes);
            Assert.AreEqual(2, limits[0].NbrOfDaysAnnualLeave);
            Assert.AreEqual((int)(8.5 * 60), limits[0].NbrOfMinutesAnnualLeave);
        }
        [TestMethod()]
        public void GetAnnualLeaveTransactionsEarned_NoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var employee = new Employee
            {
                EmployeeId = 1,
            };
            var employmentCalendar = new List<EmploymentCalenderDTO>
            {
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 1, 1),
                    AnnualLeaveGroupId = null // No annual leave group for this test
                }
            };
            
            var employeeData = new EmployeePeriodData
            {
                Employee = employee,
                EmploymentCalendar = employmentCalendar,
                Transactions = new List<TimePayrollTransaction>()
            };
            var annualLeaveGroups = new List<AnnualLeaveGroupDTO>();

            // Act
            var result = annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups);

            // Assert
            Assert.HasCount(0, result);
        }

        [TestMethod()]
        public void GetAnnualLeaveTransactionsEarned_OnlyAbsenceTransactions_ReturnsEmptyList()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var employee = new Employee
            {
                EmployeeId = 2,
                //Employments = new List<Employment>() 
            };
            var employmentCalendar = new List<EmploymentCalenderDTO>
            {
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 1, 1),
                    AnnualLeaveGroupId = null // No annual leave group for this test
                }
            };
            var employeeData = new EmployeePeriodData
            {
                Employee = employee,
                EmploymentCalendar = employmentCalendar,
                Transactions = new List<TimePayrollTransaction>
                {
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 1, 1) },
                        Quantity = 8,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence
                    }
                }
            };
            var annualLeaveGroups = new List<AnnualLeaveGroupDTO>();

            // Act
            var result = annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups);

            // Assert
            Assert.HasCount(0, result);
        }

        [TestMethod()]
        public void GetAnnualLeaveTransactionsEarned_SingleTransaction_OneLevelReached()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var employment = new Employment { OriginalAnnualLeaveGroupId = 1 };
            var employee = new Employee
            {
                EmployeeId = 3,
            };
            var employmentCalendar = new List<EmploymentCalenderDTO>
            {
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 1, 1),
                    AnnualLeaveGroupId = 1
                }
            };
            var employeeData = new EmployeePeriodData
            {
                Employee = employee,
                EmploymentCalendar = employmentCalendar,
                Transactions = new List<TimePayrollTransaction>
                {
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 1, 1) },
                        Quantity = 200,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime,
                        //SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        //SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary
                    }
                }
            };
            var annualLeaveGroups = new List<AnnualLeaveGroupDTO>
            {
                new AnnualLeaveGroupDTO { AnnualLeaveGroupId = 1, Type = TermGroup_AnnualLeaveGroupType.Commercial }
            };

            // Act
            var result = annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups);

            // Assert
            Assert.HasCount(1, result);
            Assert.AreEqual(3, result[0].EmployeeId);
            Assert.AreEqual(new DateTime(2024, 1, 1), result[0].Date);
            Assert.AreEqual(450, result[0].Minutes); // 7.5 * 60
            Assert.AreEqual(1, result[0].Days);
            Assert.AreEqual(1, result[0].Level);
        }

        [TestMethod()]
        public void GetAnnualLeaveTransactionsEarned_MultipleTransactions_MultipleLevelsReached()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var employment = new Employment { OriginalAnnualLeaveGroupId = 2 };
            var employee = new Employee
            {
                EmployeeId = 4,
            };
            var employmentCalendar = new List<EmploymentCalenderDTO>
            {
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 1, 1),
                    AnnualLeaveGroupId = 2
                },
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 2, 1),
                    AnnualLeaveGroupId = 2
                },
                new EmploymentCalenderDTO
                {
                    EmployeeId = employee.EmployeeId,
                    Date = new DateTime(2024, 3, 1),
                    AnnualLeaveGroupId = 2
                }
            };
            var employeeData = new EmployeePeriodData
            {
                Employee = employee,
                EmploymentCalendar = employmentCalendar,
                Transactions = new List<TimePayrollTransaction>
                {
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 1, 1) },
                        Quantity = 200,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime,
                        //SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        //SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary 
                    },
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 2, 1) },
                        Quantity = 200,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime,
                        //SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        //SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary
                    },
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 3, 1) },
                        Quantity = 200,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime,
                        //SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        //SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary
                    }
                }
            };
            var annualLeaveGroups = new List<AnnualLeaveGroupDTO>
            {
                new AnnualLeaveGroupDTO { AnnualLeaveGroupId = 2, Type = TermGroup_AnnualLeaveGroupType.Commercial }
            };

            // Act
            var result = annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups);

            // Assert
            Assert.HasCount(3, result);
            Assert.AreEqual(1, result[0].Level);
            Assert.AreEqual(2, result[1].Level);
            Assert.AreEqual(3, result[2].Level);
        }

        [TestMethod()]
        public void GetAnnualLeaveTransactionsEarned_EmploymentWithoutAnnualLeaveGroupId_NoTransactionsEarned()
        {
            // Arrange
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            var employment = new Employment { OriginalAnnualLeaveGroupId = null };
            var employee = new Employee
            {
                EmployeeId = 5,
            };

            var employeeData = new EmployeePeriodData
            {
                Employee = employee,
                Transactions = new List<TimePayrollTransaction>
                {
                    new TimePayrollTransaction
                    {
                        TimeBlockDate = new TimeBlockDate { Date = new DateTime(2024, 1, 1) },
                        Quantity = 200,
                        SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time,
                        SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime,
                        //SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary,
                        //SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary
                    }
                }
            };
            var annualLeaveGroups = new List<AnnualLeaveGroupDTO>();

            // Act
            var result = annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups);

            // Assert
            Assert.HasCount(0, result);
        }
    }
}