using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.Tests
{
    [TestClass()]
    public class StaffingNeedsCalculationStartEngineTests
    {
        [TestMethod()]
        public void GenerateScheduleForEmployeePostsTest()
        {
            StaffingNeedMockScenarioDTO mock = new StaffingNeedMockScenarioDTO(StaffingNeedMockScenario.All);
            SoeProgressInfo info = new SoeProgressInfo(Guid.NewGuid(), SoeProgressInfoType.ScheduleEmployeePost, mock.Company.ActorCompanyId);
            StaffingNeedsCalculationStartEngine staffingNeedsCalculationStartEngine = new StaffingNeedsCalculationStartEngine(mock.TimeBreakTemplates, mock.TimeCodeBreaks, mock.ScheduleCycleRules);
            EmployeePostCalculationOutput calculationOutput = staffingNeedsCalculationStartEngine.GenerateScheduleForEmployeePosts(mock.Company.ActorCompanyId, mock.StaffingNeedsHeadsFromTasksAndDeliveries, mock.StaffingNeedsHeadsFromShifts, mock.TimeSchedulePlanningDays, mock.SelectedEmployeePosts, mock.EmployeePosts, mock.ShiftTypes, mock.TimeCodeId, mock.FromDate, mock.Interval, ref info, mock.TimeScheduleTasks, mock.IncomingDeliveryHeads, null, parameterObject: mock.ParameterObject, openingHours: mock.OpeningHours, timeCodeBreakGroups:mock.TimeCodeBreakGroups, connectedToDatabase: false);
            Assert.IsTrue(calculationOutput.Percent > new decimal(94));
        }  
    }
}