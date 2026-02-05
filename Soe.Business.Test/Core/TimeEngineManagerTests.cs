using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeEngineManagerTests : TestBase
    {
        [TestMethod()]
        public void RecalculatePayrollPeriodTest()
        {

            TimeEngineManager tem = new TimeEngineManager(GetParameterObject(291, 193), 291, 193);
            ActionResult result = tem.RecalculatePayrollPeriod(217, 280, false, false);
            // ActionResult result = tem.RecalculatePayrollPeriod(195, 280, false, false);

            List<TaskWatchLogDTO> watchLogs = result.Value as List<TaskWatchLogDTO>;
            watchLogs.ForEach(watchLog => System.Diagnostics.Debug.WriteLine(watchLog.ToString()));

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SaveOrderAssignmentsTest()
        {
            TimeEngineManager tem = new TimeEngineManager(GetParameterObject(7, 72), 7, 72);

            ActionResult result = tem.SaveOrderAssignments(127, 646, 25, new DateTime(2018, 5, 28, 10, 0, 0), new DateTime(2018, 6, 30, 10, 0, 0), TermGroup_AssignmentTimeAdjustmentType.FillToZeroRemaining, true);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SaveEmployeeSchedulePlacementTest()
        {
            ImportExportManager ex = new ImportExportManager(GetParameterObject(1750, 1211));
            ConfigurationSetupUtil.Init();
            TimeEngineManager tem = new TimeEngineManager(GetParameterObject(1750, 1211), 1750, 1211);
            List<int> employeeIds = new List<int>();
            List<Employee> employees = new List<Employee>();
            using (CompEntities entities = new CompEntities())
            {
                employeeIds = entities.Employee.Where(w => w.ActorCompanyId == 1750).Select(s => s.EmployeeId).ToList();
                var withEmployeeSchedule = entities.EmployeeSchedule.Where(w => employeeIds.Contains(w.EmployeeId)).Select(s => s.EmployeeId).ToList();
                employeeIds = employeeIds.Where(w => !withEmployeeSchedule.Contains(w)).ToList();
                employees = entities.Employee.Include("TimeScheduleTemplateHead").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

            }

            foreach (var group in employees.GroupBy(s => s.TimeScheduleTemplateHead.First().StartDate))
            {
                var eIds = group.Select(s => s.EmployeeId).ToList();

                while (eIds.Any())
                {
                    var ids = eIds.Take(5).ToList();
                    var batch = group.Where(w => ids.Contains(w.EmployeeId)).ToList();
                    List<ActivateScheduleGridDTO> placements = new List<ActivateScheduleGridDTO>();

                    foreach (var emp in batch)
                    {
                        ActivateScheduleGridDTO dto = new ActivateScheduleGridDTO()
                        {
                            EmployeeId = emp.EmployeeId
                        };

                        placements.Add(dto);
                    }

                    tem.SaveEmployeeSchedulePlacement(null, placements, TermGroup_TemplateScheduleActivateFunctions.NewPlacement, group.Key.Value, new DateTime(2020, 12, 31));

                    eIds = eIds.Where(w => !ids.Contains(w)).ToList();
                }
            }

            Assert.IsTrue(!employees.IsNullOrEmpty());
        }

    }
}
