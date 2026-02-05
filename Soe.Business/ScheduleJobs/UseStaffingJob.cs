using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class UseStaffingJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            int? paramEmployeeId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "employeeid").Select(s => s.IntData).FirstOrDefault();
            DateTime? paramdate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "date").Select(s => s.DateData).FirstOrDefault();
            DateTime? paramFromDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "fromdate").Select(s => s.DateData).FirstOrDefault();
            DateTime? paramToDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "todate").Select(s => s.DateData).FirstOrDefault();
            bool? createPeriod = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "createperiod").Select(s => s.BoolData).FirstOrDefault();
            bool? checkDuplicatePeriods = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "checkduplicateperiods").Select(s => s.BoolData).FirstOrDefault();
            //duplicateSchedule1: Pass som är aktiva och det finns bara en borttagen period och ingen aktiv period --> Sätt perioden till aktiv
            bool? checkDuplicateSchedule1 = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "checkduplicateschedule1").Select(s => s.BoolData).FirstOrDefault();
            //duplicateSchedule2: Borttagen period har aktiva raster enbart -> Ta bort rasterna
            bool? checkDuplicateSchedule2 = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "checkduplicateschedule2").Select(s => s.BoolData).FirstOrDefault();
            //duplicateSchedule3: Om det finns dubbla nollpass aktiva, ta bort det senaste skapade blocket
            bool? checkDuplicateSchedule3 = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "checkduplicateschedule3").Select(s => s.BoolData).FirstOrDefault();
            //duplicateSchedule4: Om det finns dubbla perioder aktiva, ta bort det som inte har nåt aktivt block
            bool? checkDuplicateSchedule4 = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "checkduplicateschedule4").Select(s => s.BoolData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                DateTime fromDate = paramFromDate ?? new DateTime(2020, 1, 1);
                DateTime toDate = paramToDate ?? fromDate.AddMonths(1);

                #region createPeriod

                if (createPeriod.HasValue && createPeriod.Value)
                {
                    try
                    {
                        // Get companies
                        var companyIdsNotUsingStaffing = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.TimeUseStaffing, false);

                        using (CompEntities entities = new CompEntities())
                        {
                            var activeCompanies = entities.Company.Where(w => w.License.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active).Select(s => s.ActorCompanyId).ToList();
                            companyIdsNotUsingStaffing = companyIdsNotUsingStaffing.Where(w => activeCompanies.Contains(w)).ToList();
                            var transationsCompanyIds = entities.TimePayrollTransaction.Where(w => activeCompanies.Contains(w.ActorCompanyId)).Select(s => s.ActorCompanyId).Distinct().ToList();
                            companyIdsNotUsingStaffing = companyIdsNotUsingStaffing.Where(w => transationsCompanyIds.Contains(w)).ToList();
                            companyIdsNotUsingStaffing = companyIdsNotUsingStaffing.Distinct().ToList();

                            if (paramCompanyId.HasValue)
                                companyIdsNotUsingStaffing = new List<int>() { paramCompanyId.Value };

                            entities.CommandTimeout = 30000;
                            int count = 1;
                            int total = companyIdsNotUsingStaffing.Count;

                            foreach (var companyId in companyIdsNotUsingStaffing.OrderBy(o => o))
                            {
                                List<TimeScheduleTemplateBlock> updatedBlocks = new List<TimeScheduleTemplateBlock>();
                                var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                                var employeeIds = entities.Employee.Where(w => w.ActorCompanyId == companyId && w.State == (int)SoeEntityState.Active).Select(s => s.EmployeeId).ToList();

                                if (paramEmployeeId.HasValue)
                                    employeeIds = new List<int>() { paramEmployeeId.Value };

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name} med {employeeIds.Count} anställda. {count}/{total}");
                                count++;
                                if (!entities.EmployeeSchedule.Any(w => employeeIds.Contains(w.EmployeeId)))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga aktiveringar funna");
                                    continue;
                                }

                                var templateBlocks = entities.TimeScheduleTemplateBlock.Where(tb => tb.State == (int)SoeEntityState.Active && tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) && tb.Date.HasValue && tb.Date.Value >= fromDate && tb.Date.Value <= toDate && !tb.TimeScheduleEmployeePeriodId.HasValue && !tb.TimeScheduleScenarioHeadId.HasValue).ToList();
                                if (paramdate.HasValue)
                                    templateBlocks = templateBlocks.Where(w => w.Date == paramdate.Value).ToList();

                                if (!templateBlocks.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga block funna");
                                    continue;
                                }

                                var existingScheduleEmployeePeriods = entities.TimeScheduleEmployeePeriod.Where(w => w.ActorCompanyId == companyId && employeeIds.Contains(w.EmployeeId) && w.Date >= fromDate && w.Date <= toDate).ToList();
                                if (paramdate.HasValue)
                                    existingScheduleEmployeePeriods = existingScheduleEmployeePeriods.Where(w => w.Date == paramdate.Value).ToList();

                                var groupByDateAndEmployee = templateBlocks.GroupBy(g => $"{g.EmployeeId.Value}#{g.Date}");
                                int createdPeriods = 0;

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name}");

                                foreach (var group in groupByDateAndEmployee)
                                {
                                    var first = group.First();

                                    if (existingScheduleEmployeePeriods.Any(a => a.EmployeeId == first.EmployeeId && a.Date == first.Date.Value))
                                        continue;

                                    TimeScheduleEmployeePeriod period = new TimeScheduleEmployeePeriod()
                                    {
                                        Date = first.Date.Value,
                                        EmployeeId = first.EmployeeId.Value,
                                        ActorCompanyId = companyId,
                                        State = 0,
                                        CreatedBy = "job4547",                                        
                                    };
                                    createdPeriods++;

                                    foreach (var item in group)
                                    {
                                        item.TimeScheduleEmployeePeriod = period;
                                        updatedBlocks.Add(item);
                                    }
                                }

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Skapade perioder: {createdPeriods} på {company.ActorCompanyId} {company.Name}");
                                var periods = updatedBlocks.Select(s => s.TimeScheduleEmployeePeriod).Distinct().ToList();

                                try
                                {
                                    entities.BulkInsert(periods);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb BulkInsert: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    continue;
                                }

                                foreach (var block in updatedBlocks)
                                    block.TimeScheduleEmployeePeriodId = block.TimeScheduleEmployeePeriod.TimeScheduleEmployeePeriodId;

                                try
                                {
                                    entities.BulkUpdate(updatedBlocks);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb BulkUpdate: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    continue;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = new ActionResult(ex);
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        base.LogError(ex);
                    }
                }
                else
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Set bool createPeriod in order to run creation of missing periods.");
                }

                #endregion

                #region checkDuplicatePeriods

                if (checkDuplicatePeriods.HasValue && checkDuplicatePeriods.Value)
                {
                    try
                    {
                        using (CompEntities entities = new CompEntities())
                        {
                            // Get companies
                            var companyIdswithPeriods = entities.TimeScheduleEmployeePeriod.Select(s => s.ActorCompanyId).Distinct().ToList();
                            var activeCompanies = entities.Company.Where(w => w.License.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active).Select(s => s.ActorCompanyId).ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Where(w => activeCompanies.Contains(w)).ToList();
                            var transationsCompanyIds = entities.TimePayrollTransaction.Where(w => activeCompanies.Contains(w.ActorCompanyId)).Select(s => s.ActorCompanyId).Distinct().ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Where(w => transationsCompanyIds.Contains(w)).ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Distinct().ToList();

                            if (paramCompanyId.HasValue)
                                companyIdswithPeriods = new List<int>() { paramCompanyId.Value };

                            entities.CommandTimeout = 30000;
                            int count = 1;
                            int total = companyIdswithPeriods.Count;

                            foreach (var companyId in companyIdswithPeriods.OrderBy(o => o))
                            {
                                List<TimeScheduleTemplateBlock> updatedBlocks = new List<TimeScheduleTemplateBlock>();
                                List<TimeScheduleEmployeePeriod> updatedPeriods = new List<TimeScheduleEmployeePeriod>();
                                var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                                var employeeIds = entities.Employee.Where(w => w.ActorCompanyId == companyId && w.State == (int)SoeEntityState.Active).Select(s => s.EmployeeId).ToList();

                                if (paramEmployeeId.HasValue)
                                    employeeIds = new List<int>() { paramEmployeeId.Value };

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name} med {employeeIds.Count} anställda. {count}/{total}");
                                count++;
                                if (!entities.EmployeeSchedule.Any(w => employeeIds.Contains(w.EmployeeId)))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga aktiveringar funna");
                                    continue;
                                }

                                var templateBlocks = entities.TimeScheduleTemplateBlock.Where(tb => tb.State == (int)SoeEntityState.Active && tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) && tb.Date.HasValue && tb.Date.Value >= fromDate && tb.Date.Value <= toDate && !tb.TimeScheduleScenarioHeadId.HasValue).ToList();
                                if (paramdate.HasValue)
                                    templateBlocks = templateBlocks.Where(w => w.Date == paramdate.Value).ToList();

                                if (!templateBlocks.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga block funna");
                                    continue;
                                }

                                var existingTimeScheduleEmployeePeriods = entities.TimeScheduleEmployeePeriod.Where(w => w.ActorCompanyId == companyId && employeeIds.Contains(w.EmployeeId) && w.Date >= fromDate && w.Date <= toDate).ToList();
                                if (paramdate.HasValue)
                                    existingTimeScheduleEmployeePeriods = existingTimeScheduleEmployeePeriods.Where(w => w.Date == paramdate.Value).ToList();

                                if (!existingTimeScheduleEmployeePeriods.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga perioder funna");
                                    continue;
                                }

                                var groupByDateAndEmployee = templateBlocks.GroupBy(g => $"{g.EmployeeId.Value}#{g.Date}");

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name}");

                                foreach (var group in groupByDateAndEmployee)
                                {
                                    var first = group.First();
                                    var blocks = group.ToList();

                                    var periodsOnDateAndEmployee = existingTimeScheduleEmployeePeriods.Where(w => w.EmployeeId == first.EmployeeId && w.Date == first.Date.Value).ToList();

                                    foreach (var item in blocks.GroupBy(g => g.TimeScheduleEmployeePeriodId).Where(a => a.All(aa => aa.IsBreak)))
                                    {
                                        if (periodsOnDateAndEmployee.Count > 1)
                                        {
                                            var period = periodsOnDateAndEmployee.First(f => f.TimeScheduleEmployeePeriodId == item.Key);
                                            period.State = 2;
                                            period.ModifiedBy = "jjh44a_2";
                                            updatedPeriods.Add(period);
                                        }
                                        blocks.ForEach(f => { f.State = 2; f.ModifiedBy = "jjh44a_2"; });
                                        updatedBlocks.AddRange(blocks);
                                    }

                                    periodsOnDateAndEmployee = periodsOnDateAndEmployee.Where(w => w.State == 0).ToList();

                                    if (periodsOnDateAndEmployee.Count > 1)
                                    {
                                        var firstPeriod = periodsOnDateAndEmployee.First();
                                        firstPeriod.ModifiedBy = "jjh44a_1";
                                        firstPeriod.State = 0;

                                        foreach (var ePeriod in periodsOnDateAndEmployee.Where(w => w.TimeScheduleEmployeePeriodId != firstPeriod.TimeScheduleEmployeePeriodId))
                                        {
                                            ePeriod.ModifiedBy = "jjh44a_2";
                                            ePeriod.State = 2;
                                        }

                                        updatedPeriods.AddRange(periodsOnDateAndEmployee);

                                        foreach (var item in group)
                                        {
                                            item.TimeScheduleEmployeePeriod = firstPeriod;
                                            updatedBlocks.Add(item);
                                        }
                                    }
                                }

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"updaterade perioder: {updatedPeriods.Count} på {company.ActorCompanyId} {company.Name}");

                                try
                                {
                                    entities.BulkUpdate(updatedPeriods);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb BulkInsert: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    continue;
                                }

                                foreach (var block in updatedBlocks)
                                    block.TimeScheduleEmployeePeriodId = block.TimeScheduleEmployeePeriod.TimeScheduleEmployeePeriodId;

                                try
                                {
                                    entities.BulkUpdate(updatedBlocks);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb BulkUpdate: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    continue;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = new ActionResult(ex);
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        base.LogError(ex);
                    }
                }
                else
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Set bool checkDuplicatePeriods in order to run check for duplicates");
                }

                #endregion

                #region checkDuplicateSchedule

                if (checkDuplicateSchedule1 == true || checkDuplicateSchedule2 == true || checkDuplicateSchedule3 == true || checkDuplicateSchedule4 == true)
                {
                    try
                    {
                        using (CompEntities entities = new CompEntities())
                        {
                            #region Prereq

                            var companyIdswithPeriods = entities.TimeScheduleEmployeePeriod.Select(s => s.ActorCompanyId).Distinct().ToList();
                            var activeCompanies = entities.Company.Where(w => w.License.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active).Select(s => s.ActorCompanyId).ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Where(w => activeCompanies.Contains(w)).ToList();
                            var transationsCompanyIds = entities.TimePayrollTransaction.Where(w => activeCompanies.Contains(w.ActorCompanyId)).Select(s => s.ActorCompanyId).Distinct().ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Where(w => transationsCompanyIds.Contains(w)).ToList();
                            companyIdswithPeriods = companyIdswithPeriods.Distinct().ToList();

                            if (paramCompanyId.HasValue)
                                companyIdswithPeriods = new List<int>() { paramCompanyId.Value };

                            entities.CommandTimeout = 30000;
                            int count = 1;
                            int total = companyIdswithPeriods.Count;

                            #endregion

                            foreach (var companyId in companyIdswithPeriods.OrderBy(o => o))
                            {
                                #region Prereq

                                var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                                if (company == null || company.State != (int)SoeEntityState.Active)
                                    continue;

                                var employeeIds = entities.Employee.Where(w => w.ActorCompanyId == companyId && w.State == (int)SoeEntityState.Active).Select(s => s.EmployeeId).ToList();
                                if (paramEmployeeId.HasValue)
                                    employeeIds = new List<int>() { paramEmployeeId.Value };

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name} med {employeeIds.Count} anställda. {count}/{total}");
                                count++;

                                if (!entities.EmployeeSchedule.Any(w => employeeIds.Contains(w.EmployeeId)))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga aktiveringar funna");
                                    continue;
                                }

                                var templateBlocks = entities.TimeScheduleTemplateBlock.Where(tb => tb.State == (int)SoeEntityState.Active && tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) && tb.Date.HasValue && tb.Date.Value >= fromDate && tb.Date.Value <= toDate && !tb.TimeScheduleScenarioHeadId.HasValue).ToList();
                                if (paramdate.HasValue)
                                    templateBlocks = templateBlocks.Where(w => w.Date == paramdate.Value).ToList();
                                if (!templateBlocks.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga block funna");
                                    continue;
                                }

                                var existingTimeScheduleEmployeePeriods = entities.TimeScheduleEmployeePeriod.Where(w => w.ActorCompanyId == companyId && employeeIds.Contains(w.EmployeeId) && w.Date >= fromDate && w.Date <= toDate).ToList();
                                if (paramdate.HasValue)
                                    existingTimeScheduleEmployeePeriods = existingTimeScheduleEmployeePeriods.Where(w => w.Date == paramdate.Value).ToList();
                                if (!existingTimeScheduleEmployeePeriods.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga perioder funna");
                                    continue;
                                }

                                #endregion

                                #region checkDuplicateSchedule1

                                if (checkDuplicateSchedule1 == true)
                                {
                                    try
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar checkDuplicateSchedule1: {company.ActorCompanyId} {company.Name}");
                                        int counter = 0;

                                        foreach (var templateBlocksByEmployee in templateBlocks.Where(i => i.EmployeeId.HasValue).GroupBy(i => i.EmployeeId.Value))
                                        {
                                            Employee employee = null;
                                            int employeeId = templateBlocksByEmployee.Key;
                                            var periodsByEmployee = existingTimeScheduleEmployeePeriods.Where(w => w.EmployeeId == employeeId).ToList();
                                            bool hasChanges = false;

                                            foreach (var templateBlocksByDate in templateBlocksByEmployee.Where(i => i.Date.HasValue && i.StartTime < i.StopTime).GroupBy(i => i.Date.Value))
                                            {
                                                counter++;
                                                DateTime date = templateBlocksByDate.Key;

                                                var periodsOnDate = periodsByEmployee.Where(w => w.Date == date).ToList();
                                                int all = periodsOnDate.Count;
                                                int deleted = periodsOnDate.Count(i => i.State == (int)SoeEntityState.Deleted);
                                                if (all == deleted && all == 1)
                                                {
                                                    var period = periodsOnDate.First();
                                                    period.State = (int)SoeEntityState.Active;
                                                    period.Modified = DateTime.Now;
                                                    period.ModifiedBy = "checkDuplicateSchedule1";

                                                    if (employee == null)
                                                        employee = entities.Employee.FirstOrDefault(i => i.EmployeeId == employeeId);
                                                    string employeeNr = employee?.EmployeeNr ?? string.Empty;

                                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;checkDuplicateSchedule1;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()}");
                                                    hasChanges = true;
                                                }
                                            }

                                            if (hasChanges)
                                                entities.SaveChanges();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb checkDuplicateSchedule1: '{0}'", ex.ToString()));
                                        base.LogError(ex);
                                        continue;
                                    }
                                }

                                #endregion

                                #region checkDuplicateSchedule2

                                if (checkDuplicateSchedule2 == true)
                                {
                                    try
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar checkDuplicateSchedule2: {company.ActorCompanyId} {company.Name}");
                                        int counter = 0;

                                        foreach (var templateBlocksByEmployee in templateBlocks.Where(i => i.EmployeeId.HasValue && i.StartTime < i.StopTime).GroupBy(i => i.EmployeeId.Value))
                                        {
                                            int employeeId = templateBlocksByEmployee.Key;
                                            var employee = entities.Employee.FirstOrDefault(i => i.EmployeeId == employeeId);
                                            string employeeNr = employee?.EmployeeNr ?? string.Empty;
                                            var periodsByEmployee = existingTimeScheduleEmployeePeriods.Where(w => w.EmployeeId == employeeId).ToList();
                                            bool hasChanges = false;

                                            foreach (var templateBlocksByDate in templateBlocksByEmployee.Where(i => i.Date.HasValue).GroupBy(i => i.Date.Value))
                                            {
                                                if (templateBlocksByDate.Any(i => !i.TimeScheduleEmployeePeriodId.HasValue))
                                                    continue;

                                                DateTime date = templateBlocksByDate.Key;
                                                var periodsOnDate = periodsByEmployee.Where(w => w.Date == date).ToList();

                                                //Nothing to do
                                                if (periodsOnDate.Count <= 1)
                                                    continue;

                                                var periodsOnDateActive = periodsOnDate.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                                                var periodOnDateActiveIds = periodsOnDateActive.Select(i => i.TimeScheduleEmployeePeriodId).ToList();
                                                var periodsOnDateDeleted = periodsOnDate.Where(i => i.State == (int)SoeEntityState.Deleted).ToList();

                                                //Has active blocks connected on different periods
                                                var activeWorkOnDay = templateBlocksByDate.Where(i => i.BreakType == 0 && i.State == (int)SoeEntityState.Active).ToList();
                                                var activeWorkOnDayPeriodIds = activeWorkOnDay.Select(i => i.TimeScheduleEmployeePeriodId.Value).Distinct().ToList();
                                                if (activeWorkOnDayPeriodIds.Count > 1)
                                                {
                                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;hasactiveblocksondifferentperiods;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()};timeScheduleTemplateBlockId");
                                                    continue;
                                                }

                                                //Has active breaks connected to different active periods
                                                var activeBreakOnDay = templateBlocksByDate.Where(i => i.BreakType > 0 && i.State == (int)SoeEntityState.Active).ToList();
                                                var activeBreakOnDayAndActivePeriodIds = activeBreakOnDay.Where(i => periodOnDateActiveIds.Contains(i.TimeScheduleEmployeePeriodId.Value)).Select(i => i.TimeScheduleEmployeePeriodId.Value).Distinct().ToList();
                                                if (activeBreakOnDayAndActivePeriodIds.Count > 1)
                                                {
                                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;hasactivebreaksondifferentactiveperiods;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()};timeScheduleTemplateBlockId");
                                                    continue;
                                                }

                                                //Cannot handle different active periods
                                                if (periodsOnDate.Count(i => i.State == (int)SoeEntityState.Active) > 1)
                                                {
                                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;hasmultipleactiveperiods;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()};timeScheduleTemplateBlockId");
                                                    continue;
                                                }

                                                int absence = 0;
                                                int totalBlocks = activeWorkOnDay.Count;
                                                int abseenceBlocks = activeWorkOnDay.Count(i => i.TimeDeviationCauseId.HasValue);
                                                if (totalBlocks > 0 && totalBlocks == abseenceBlocks)
                                                    absence = 2; //whole absence
                                                else if (abseenceBlocks > 0)
                                                    absence = 1; //partly absence

                                                #region Active breaks on deleted period

                                                foreach (var period in periodsOnDateDeleted)
                                                {
                                                    counter++;

                                                    var templateBlocksByPeriod = templateBlocksByDate.Where(i => i.State == (int)SoeEntityState.Active && i.TimeScheduleEmployeePeriodId == period.TimeScheduleEmployeePeriodId).ToList();
                                                    if (!templateBlocksByPeriod.Any())
                                                        continue;

                                                    bool hasWork = templateBlocksByPeriod.Any(i => i.BreakType == 0);
                                                    if (!hasWork)
                                                    {
                                                        bool deleteBreak = true;
                                                        int? periodId = null;

                                                        if (activeWorkOnDayPeriodIds.Count == 1)
                                                        {
                                                            periodId = activeWorkOnDayPeriodIds.First();
                                                            if (!templateBlocksByDate.Any(i => i.BreakType > 0 && i.TimeScheduleEmployeePeriodId == periodId && i.State == (int)SoeEntityState.Active))
                                                                deleteBreak = false;
                                                        }

                                                        List<int> changedBlockIds = new List<int>();
                                                        foreach (var templateBlock in templateBlocksByPeriod)
                                                        {
                                                            if (deleteBreak)
                                                            {
                                                                templateBlock.State = (int)SoeEntityState.Deleted;
                                                                templateBlock.Modified = DateTime.Now;
                                                                templateBlock.ModifiedBy = "checkDuplicateSchedule2del";
                                                                changedBlockIds.Add(templateBlock.TimeScheduleTemplateBlockId);
                                                            }
                                                            else if (periodId.HasValue)
                                                            {
                                                                templateBlock.TimeScheduleEmployeePeriodId = periodId.Value;
                                                                templateBlock.Modified = DateTime.Now;
                                                                templateBlock.ModifiedBy = "checkDuplicateSchedule2move";
                                                                changedBlockIds.Add(templateBlock.TimeScheduleTemplateBlockId);
                                                            }

                                                        }

                                                        if (deleteBreak)
                                                            CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;checkDuplicateSchedule2del;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()};timeScheduleTemplateBlockId;{changedBlockIds.ToCommaSeparated()};absence;{absence}");
                                                        else
                                                            CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;checkDuplicateSchedule2move;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employeeNr};date;{date.ToShortDateString()};timeScheduleTemplateBlockId;{changedBlockIds.ToCommaSeparated()};absence;{absence}");
                                                        hasChanges = true;
                                                    }
                                                }

                                                #endregion
                                            }

                                            if (hasChanges)
                                                entities.SaveChanges();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb checkDuplicateSchedule2: '{0}'", ex.ToString()));
                                        base.LogError(ex);
                                        continue;
                                    }
                                }

                                #endregion

                                #region checkDuplicateSchedule3

                                if (checkDuplicateSchedule3 == true)
                                {
                                    var templateBlocksZero = templateBlocks.Where(i => i.EmployeeId.HasValue && i.Date.HasValue && i.StartTime == i.StopTime && i.State == (int)SoeEntityState.Active).ToList();
                                    foreach (var templateBlocksByEmployee in templateBlocksZero.GroupBy(i => i.EmployeeId.Value))
                                    {
                                        int employeeId = templateBlocksByEmployee.Key;
                                        Employee employee = null;

                                        var periodsByEmployee = existingTimeScheduleEmployeePeriods.Where(w => w.EmployeeId == employeeId).ToList();
                                        bool hasChanges = false;

                                        foreach (var templateBlocksByDate in templateBlocksByEmployee.GroupBy(i => i.Date.Value))
                                        {
                                            if (templateBlocksByDate.Count() <= 1)
                                                continue;

                                            DateTime date = templateBlocksByDate.Key;
                                            var periodsByDate = periodsByEmployee.Where(w => w.Date == date && w.State == (int)SoeEntityState.Active).ToList();

                                            int? periodIdActive = null;
                                            foreach (var templateBlock in templateBlocksByDate)
                                            {
                                                var period = periodsByDate.FirstOrDefault(i => i.TimeScheduleEmployeePeriodId == templateBlock.TimeScheduleEmployeePeriodId);
                                                if (period != null)
                                                {
                                                    periodIdActive = period.TimeScheduleEmployeePeriodId;
                                                    break;
                                                }
                                            }

                                            if (employee == null)
                                                employee = entities.Employee.FirstOrDefault(i => i.EmployeeId == employeeId);

                                            if (!periodIdActive.HasValue)
                                            {
                                                CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;hasmultiplezeroblockswithdeletedperiods;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employee.GetEmployeeNr()};date;{date.ToShortDateString()};timeScheduleTemplateBlockId");
                                                continue;
                                            }

                                            List<int> changedBlockIds = new List<int>();
                                            if (templateBlocksByDate.All(i => i.TimeScheduleEmployeePeriodId == periodIdActive))
                                            {
                                                foreach (var templateBlock in templateBlocksByDate.Skip(1))
                                                {
                                                    templateBlock.State = (int)SoeEntityState.Deleted;
                                                    templateBlock.Modified = DateTime.Now;
                                                    templateBlock.ModifiedBy = "checkDuplicateSchedule3a";
                                                    changedBlockIds.Add(templateBlock.TimeScheduleTemplateBlockId);
                                                }
                                            }
                                            else
                                            {
                                                foreach (var templateBlock in templateBlocksByDate)
                                                {
                                                    if (templateBlock.TimeScheduleEmployeePeriodId != periodIdActive.Value)
                                                    {
                                                        templateBlock.State = (int)SoeEntityState.Deleted;
                                                        templateBlock.Modified = DateTime.Now;
                                                        templateBlock.ModifiedBy = "checkDuplicateSchedule3b";
                                                        changedBlockIds.Add(templateBlock.TimeScheduleTemplateBlockId);
                                                    }
                                                }
                                            }

                                            hasChanges = changedBlockIds.Count > 0;
                                            if (hasChanges)
                                                CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;checkDuplicateSchedule3;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employee.GetEmployeeNr()};date;{date.ToShortDateString()};timeScheduleTemplateBlockId;{changedBlockIds.ToCommaSeparated()}");
                                        }

                                        if (hasChanges)
                                            entities.SaveChanges();
                                    }
                                }

                                #endregion

                                #region checkDuplicateSchedule4

                                if (checkDuplicateSchedule4 == true)
                                {
                                    var activePeriods = existingTimeScheduleEmployeePeriods.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                                    foreach (var activePeriodsByEmployee in activePeriods.GroupBy(i => i.EmployeeId))
                                    {
                                        int employeeId = activePeriodsByEmployee.Key;
                                        Employee employee = null;

                                        var templateBlocksByEmployee = templateBlocks.Where(w => w.EmployeeId == employeeId && w.State == (int)SoeEntityState.Active).ToList();
                                        bool hasChanges = false;

                                        foreach (var activePeriodsByDate in activePeriodsByEmployee.GroupBy(i => i.Date))
                                        {
                                            if (activePeriodsByDate.Count() <= 1)
                                                continue;

                                            DateTime date = activePeriodsByDate.Key;
                                            var templateBlocksByDate = templateBlocksByEmployee.Where(i => i.Date == date).ToList(); 
                                            
                                            List<int> periodIdsWithBlocks = new List<int>();
                                            foreach (var activePeriod in activePeriodsByDate)
                                            {
                                                if (templateBlocksByDate.Any(i => i.TimeScheduleEmployeePeriodId == activePeriod.TimeScheduleEmployeePeriodId))
                                                    periodIdsWithBlocks.Add(activePeriod.TimeScheduleEmployeePeriodId);
                                            }

                                            if (employee == null)
                                                employee = entities.Employee.FirstOrDefault(i => i.EmployeeId == employeeId);

                                            if (periodIdsWithBlocks.Count > 1)
                                            {
                                                CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;hasmultipleactiveperiodswithactiveblocks;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employee.GetEmployeeNr()};date;{date.ToShortDateString()};timeScheduleTemplateBlockId");
                                                continue;
                                            }

                                            List<int> changedPeriodIds = new List<int>();
                                            foreach (var period in activePeriodsByDate)
                                            {
                                                if (periodIdsWithBlocks.Contains(period.TimeScheduleEmployeePeriodId))
                                                    continue;

                                                period.State = (int)SoeEntityState.Deleted;
                                                period.Modified = DateTime.Now;
                                                period.ModifiedBy = "checkDuplicateSchedule4";
                                                changedPeriodIds.Add(period.TimeScheduleEmployeePeriodId);
                                            }

                                            hasChanges = changedPeriodIds.Count > 0;
                                            if (hasChanges)
                                                CreateLogEntry(ScheduledJobLogLevel.Information, $"scenario;checkDuplicateSchedule4;db={scheduledJob.DatabaseName};actorCompanyId;{companyId};employeeId;{employeeId};employeeNr;{employee.GetEmployeeNr()};date;{date.ToShortDateString()};timeScheduleEmployeePeriodId;{changedPeriodIds.ToCommaSeparated()}");
                                        }

                                        if (hasChanges)
                                            entities.SaveChanges();
                                    }
                                }

                                #endregion
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = new ActionResult(ex);
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        base.LogError(ex);
                    }
                }
                else
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Set bool checkDuplicatePeriods in order to run check for duplicates");
                }

                #endregion

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
