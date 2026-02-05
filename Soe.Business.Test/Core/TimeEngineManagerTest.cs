using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Tests;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soe.Business.Test.Core
{
    [TestClass]
    public class TimeEngineManagerTest : TestBase
    {
        [TestMethod]
        public void SaveWholedayDeviations()
        {
            //Change config files på to point to devaxf
            //företag: Lisaberg
            //användare: b1 - regionschef(lisaberg AB)
            //Göteborg gamlestaden
            //anställd 85423, april

            ConfigurationSetupUtil.Init();

            //Setup language cache
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);

            TimeScheduleManager tsm1 = new TimeScheduleManager(GetParameterObject(1300,15109));
            var firstEmployeeDates = CalendarUtility.GetDates(new DateTime(2021, 4, 1), new DateTime(2021, 4, 30));
            List<TimeSchedulePlanningDayDTO> firstEmployeeShifts = tsm1.GetAbsenceAffectedShifts(1300, 15109, 10727, null, 354, firstEmployeeDates).ToList();
            foreach (var item in firstEmployeeShifts)
            {
                item.ApprovalTypeId = 1;
                item.EmployeeId = -1;
                    
            }

            TimeScheduleManager tsm2 = new TimeScheduleManager(GetParameterObject(1300, 15109));
            var secondEmployeeDates = CalendarUtility.GetDates(new DateTime(2021, 4, 1), new DateTime(2021, 4, 30));
            List<TimeSchedulePlanningDayDTO> secondEmployeeShifts = tsm2.GetAbsenceAffectedShifts(1300, 15109, 6697, null, 354, secondEmployeeDates).ToList();
            foreach (var item in secondEmployeeShifts)
            {
                item.ApprovalTypeId = 1;
                item.EmployeeId = -1;

            }

            Parallel.Invoke(() =>
            {                
                TimeEngineManager tem = new TimeEngineManager(GetParameterObject(1300, 15109), 1300, 15109);
                tem.GenerateAndSaveAbsenceFromStaffing(new EmployeeRequestDTO(6697, 354), secondEmployeeShifts, true, false, null);
                
            },
            () =>
            {
                Thread.Sleep(100);
                TimeEngineManager tem = new TimeEngineManager(GetParameterObject(1300, 15109), 1300, 15109);
                tem.GenerateAndSaveAbsenceFromStaffing(new EmployeeRequestDTO(10727, 354), firstEmployeeShifts, true, false, null);
            }
            );

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ConcurrentRecalculationTest()
        {
            ConfigurationSetupUtil.Init();

            //Setup language cache
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);

            int actorCompanyId = 241324;
            int userId = 18521;
            int employeeId = 19815;
            DateTime dateFrom = new DateTime(2022, 02, 01);
            DateTime dateTo = new DateTime(2022, 02, 28);
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, userId);

            TimeBlockManager tbm = new TimeBlockManager(parameterObject);
            List<TimeBlockDate> timeBlockDates = tbm.GetTimeBlockDates(employeeId, dateFrom, dateTo);

            List<AttestEmployeeDaySmallDTO> items = new List<AttestEmployeeDaySmallDTO>();
            foreach (TimeBlockDate timeBlockDate in timeBlockDates)
            {
                items.Add(new AttestEmployeeDaySmallDTO
                {
                    EmployeeId = employeeId,
                    Date = timeBlockDate.Date,
                    TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                });
            }            

            Parallel.Invoke(() =>
            {
                TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, userId);
                tem.ApplyCalculationFunctionForEmployee(items, SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps, null);
            },
            () =>
            {
                Thread.Sleep(2000);
                TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, userId);
                tem.ApplyCalculationFunctionForEmployee(items, SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest, null);
            });

            Assert.IsTrue(true);
        }
    }
}
