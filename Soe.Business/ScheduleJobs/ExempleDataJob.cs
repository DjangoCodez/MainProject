using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class ExampleDataJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            if (!scheduledJob.SysJobSettings.Any())
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "No settings found for scheduled job");
                return;
            }
            string actorcompanyIdsString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyids").Select(s => s.StrData).FirstOrDefault();
            string stampDateFromString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stampdatefrom").Select(s => s.StrData).FirstOrDefault();
            string stampDateToString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stampdateto").Select(s => s.StrData).FirstOrDefault();
            bool recalc = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "recalc").Select(s => s.BoolData).FirstOrDefault() ?? false;
            bool merge = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "merge").Select(s => s.BoolData).FirstOrDefault() ?? false;
            bool stamps = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stamps").Select(s => s.BoolData).FirstOrDefault() ?? false;
            bool mergeTrans = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "mergetrans").Select(s => s.BoolData).FirstOrDefault() ?? false;

            List<int> actorCompanyIds = new List<int>();

            foreach (var acid in actorcompanyIdsString.Split(','))
            {
                int id = 0;
                int.TryParse(acid, out id);

                if (id != 0)
                    actorCompanyIds.Add(id);
            }

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {

                if (actorCompanyIds.Count == 0)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "actorcompanyids is incorrect");
                    return;
                }

                try
                {

                    foreach (var actorCompanyId in actorCompanyIds)
                    {
                        UserManager um = new UserManager(null);

                        var adminUser = um.GetAdminUser(actorCompanyId);

                        if (adminUser == null)
                            adminUser = um.GetUsersByCompany(actorCompanyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();

                        ParameterObject param = null;

                        CreateLogEntry(ScheduledJobLogLevel.Information, "started actorCompanyId " + actorCompanyId.ToString());

                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, "adminUser is null" + actorCompanyId.ToString());
                            param = GetParameterObject(actorCompanyId, 4);
                        }
                        else
                        {
                            param = GetParameterObject(actorCompanyId, adminUser.UserId);
                        }

                        TimeStampManager tsm = new TimeStampManager(param);
                        EmployeeManager em = new EmployeeManager(param);
                        TimeScheduleManager timeScheduleManager = new TimeScheduleManager(param);

                        if (!DateTime.TryParse(stampDateFromString, out DateTime dateFrom))
                            dateFrom = DateTime.Today.AddDays(-2);
                        if (!DateTime.TryParse(stampDateToString, out DateTime dateTo))
                            dateTo = DateTime.Today.AddDays(-1);

                        if (merge && actorCompanyId == 1750)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "MergeShiftsOnDemoCompany started actorCompanyId " + actorCompanyId.ToString());
                            MergeShiftsOnDemoCompany();
                            CreateLogEntry(ScheduledJobLogLevel.Information, "MergeShiftsOnDemoCompany dont actorCompanyId " + actorCompanyId.ToString());
                        }
                        if (mergeTrans)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "MergeTransactionsOlderThanOneYear started actorCompanyId " + actorCompanyId.ToString());
                            MergeTransactionsOlderThanTwoYears(actorCompanyId);
                            CreateLogEntry(ScheduledJobLogLevel.Information, "MergeTransactionsOlderThanOneYear done actorCompanyId " + actorCompanyId.ToString());
                        }

                        if (stamps)
                        {

                            CreateLogEntry(ScheduledJobLogLevel.Information, "stamps started actorCompanyId " + actorCompanyId.ToString());

                            result = tsm.ConvertTimeStampsToTimeBlocks(actorCompanyId, dateFrom);

                            CreateLogEntry(ScheduledJobLogLevel.Information, "stamps done actorCompanyId " + actorCompanyId.ToString());

                        }

                        if (recalc)
                        {

                            List<Employee> employees1;
                            using (CompEntities entities = new CompEntities())
                            {
                                employees1 = entities.Employee.Where(w => w.ActorCompanyId == actorCompanyId).ToList();
                            }

                            int countEmployee = 0;
                            foreach (var employee in employees1)
                            {
                                countEmployee++;
                                using (CompEntities entities = new CompEntities())
                                {
                                    DateTime from = dateFrom;
                                    List<int> ids = entities.TimeBlockDate.Where(w => w.EmployeeId == employee.EmployeeId && w.Date >= from).Select(s => s.TimeBlockDateId).ToList();

                                    var timeBlockDateIdsOnTimeBlokc = entities.TimeBlock.Where(w2 => w2.EmployeeId == employee.EmployeeId && ids.Contains(w2.TimeBlockDateId)).Select(s => s.TimeBlockDateId).ToList();
                                    ids = ids.Where(w => !timeBlockDateIdsOnTimeBlokc.Contains(w)).ToList();

                                    if (ids.Any())
                                    {
                                        var timestampEntries = tsm.GetTimeStampEntries(entities, ids);
                                        TimeEngineManager tem = new TimeEngineManager(param, actorCompanyId, adminUser.UserId);
                                        tem.ReGenerateDayBasedOnTimeStamps(timestampEntries);
                                        CreateLogEntry(ScheduledJobLogLevel.Information, "started calc employee  " + employee.EmployeeNr + "timeBlockDateIds: " + ids.Count.ToString() + " count:" + countEmployee.ToString());
                                    }
                                    else
                                        CreateLogEntry(ScheduledJobLogLevel.Information, "nothing to calc employee  " + employee.EmployeeNr + " count:" + countEmployee.ToString());

                                }
                            }
                        }

                        tsm.CreateFakeTimeStamps(actorCompanyId, dateFrom, dateTo);
                        dateFrom = dateFrom > DateTime.Today.AddMonths(-2) ? DateTime.Today.AddMonths(-2) : dateFrom;
                        tsm.ConvertTimeStampsToTimeBlocks(actorCompanyId, dateFrom);

                        Random random = new Random();

                        using (CompEntities entities = new CompEntities())
                        {
                            int year = DateTime.Now.Year;

                            var employeestest = entities.Employee.Where(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active).ToList();
                            var taxes = entities.EmployeeTaxSE.Where(w => w.Employee.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active && w.Employee.State == (int)SoeEntityState.Active).ToList();

                            foreach (var employee in employeestest)
                            {
                                var currentTax = taxes.FirstOrDefault(f => f.Year == year && f.EmployeeId == employee.EmployeeId);

                                if (currentTax == null)
                                {
                                    var previous = taxes.FirstOrDefault(f => f.Year == (year - 1) && f.EmployeeId == employee.EmployeeId);
                                    EmployeeTaxSE tax;
                                    if (random.Next(1, 500) > 3)
                                        tax = em.CreateEmployeeTaxSE(entities, employee, TermGroup_EmployeeTaxType.TableTax, year, random.Next(30, 34), 1, null);
                                    else
                                    {
                                        tax = em.CreateEmployeeTaxSE(entities, employee, TermGroup_EmployeeTaxType.TableTax, year, random.Next(30, 34), 1, null);
                                        tax.SalaryDistressReservedAmount = random.Next(3000, 8000);
                                        tax.SalaryDistressAmountType = (int)TermGroup_EmployeeTaxSalaryDistressAmountType.FixedAmount;
                                        tax.SalaryDistressAmount = decimal.Divide(tax.SalaryDistressReservedAmount.Value, 10);
                                        tax.SalaryDistressCase = employee.EmployeeId.ToString();
                                    }

                                    if (previous != null && previous.SalaryDistressReservedAmount.HasValue && previous.SalaryDistressReservedAmount > 0)
                                    {
                                        tax.SalaryDistressReservedAmount = previous.SalaryDistressReservedAmount;
                                        tax.SalaryDistressAmountType = previous.SalaryDistressAmountType;
                                        tax.SalaryDistressAmount = previous.SalaryDistressAmount;
                                        tax.SalaryDistressCase = previous.SalaryDistressCase;
                                    }

                                    entities.SaveChanges();
                                }
                            }
                        }

                        List<Employee> employees;
                        List<EmployeeSchedule> employeeSchedules;
                        ImportExportManager ex = new ImportExportManager(param);
                        ConfigurationSetupUtil.Init();

                        List<int> employeeIds = new List<int>();

                        using (CompEntities entities = new CompEntities())
                        {
                            employeeIds = entities.Employee.Where(w => w.ActorCompanyId == actorCompanyId).Select(s => s.EmployeeId).ToList();
                            var withEmployeeSchedule = entities.EmployeeSchedule.Where(w => employeeIds.Contains(w.EmployeeId)).Select(s => s.EmployeeId).ToList();
                            employees = entities.Employee.Include("TimeScheduleTemplateHead").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
                            employees = employees.Where(w => w.TimeScheduleTemplateHead != null).ToList();
                            employeeSchedules = entities.EmployeeSchedule.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
                        }
                        DateTime endOfYear = new DateTime(DateTime.Now.Year, 12, 31);

                        foreach (var group in employees.Where(w => !w.TimeScheduleTemplateHead.IsNullOrEmpty()).GroupBy(s => s.TimeScheduleTemplateHead.LastOrDefault()?.StartDate))
                        {
                            var eIds = group.Select(s => s.EmployeeId).ToList();

                            while (eIds.Any())
                            {
                                var ids = eIds.Take(5).ToList();
                                var empSchedules = employeeSchedules.Where(w => ids.Contains(w.EmployeeId));
                                var batch = group.Where(w => ids.Contains(w.EmployeeId)).ToList();
                                List<ActivateScheduleGridDTO> placements = new List<ActivateScheduleGridDTO>();
                                List<ActivateScheduleGridDTO> newPlacements = new List<ActivateScheduleGridDTO>();
                                TimeEngineManager tem = new TimeEngineManager(param, actorCompanyId, adminUser.UserId);

                                foreach (var emp in batch)
                                {
                                    var employeeSchedule = empSchedules.OrderByDescending(o => o.StartDate).FirstOrDefault();

                                    if (employeeSchedule != null && employeeSchedule.StopDate == endOfYear)
                                        continue;

                                    ActivateScheduleGridDTO dto = new ActivateScheduleGridDTO()
                                    {
                                        EmployeeId = emp.EmployeeId
                                    };

                                    if (employeeSchedule != null)
                                    {
                                        dto = new ActivateScheduleGridDTO()
                                        {
                                            EmployeeScheduleId = employeeSchedule?.EmployeeScheduleId ?? 0,
                                            IsPlaced = false,
                                            IsPreliminary = employeeSchedule?.IsPreliminary ?? false,
                                            EmployeeScheduleStartDate = employeeSchedule?.StartDate,
                                            EmployeeScheduleStopDate = employeeSchedule?.StopDate,
                                            EmployeeScheduleStartDayNumber = employeeSchedule?.StartDayNumber ?? 0,
                                            EmployeeId = emp.EmployeeId,
                                            EmployeeNr = "9999",
                                            EmployeeName = "",
                                            TimeScheduleTemplateHeadId = employeeSchedule?.TimeScheduleTemplateHeadId ?? 0,
                                            TimeScheduleTemplateHeadName = employeeSchedule?.TimeScheduleTemplateHead?.Name ?? String.Empty,
                                            TemplateEmployeeId = employeeSchedule?.TimeScheduleTemplateHead?.EmployeeId ?? 0,
                                            TemplateStartDate = employeeSchedule?.TimeScheduleTemplateHead?.StartDate,
                                            EmployeeGroupId = 0,
                                            EmployeeGroupName = "",
                                        };

                                        placements.Add(dto);
                                    }
                                    else
                                    {
                                        newPlacements.Add(dto);
                                    }

                                }

                                foreach (var groupOnDate in placements.GroupBy(g => g.EmployeeScheduleStopDate))
                                    tem.SaveEmployeeSchedulePlacement(null, groupOnDate.ToList(), TermGroup_TemplateScheduleActivateFunctions.NewPlacement, groupOnDate.Key.Value.AddDays(1), endOfYear, useBulk: true);

                                tem.SaveEmployeeSchedulePlacement(null, newPlacements.ToList(), TermGroup_TemplateScheduleActivateFunctions.NewPlacement, group.Key.Value, endOfYear);

                                eIds = eIds.Where(w => !ids.Contains(w)).ToList();
                            }
                        }

                        timeScheduleManager.CreateFakeStaffingneedsFrequancy(actorCompanyId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
                    }

                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }

        private void MergeTransactionsOlderThanTwoYears(int actorCompanyId)
        {
            List<Employee> employees = new List<Employee>();
            using (CompEntities entities = new CompEntities())
            {
                employees = entities.Employee.Where(w => w.ActorCompanyId == actorCompanyId).ToList();
            }

            int count = 0;
            int countEmployees = employees.Count;
            foreach (var employee in employees)
            {
                using (CompEntities entities = new CompEntities())
                {

                    count++;
                    List<TimePayrollTransaction> batchTimePayrollTransactions = new List<TimePayrollTransaction>();
                    DateTime dateFrom = DateTime.Today.AddYears(-2);
                    var transactions = entities.TimePayrollTransaction.Include("AccountInternal").Include("TimePayrollTransactionExtended").Where(w => w.EmployeeId == employee.EmployeeId && w.ActorCompanyId == actorCompanyId && w.TimeBlockDate.Date < dateFrom && w.State == (int)SoeEntityState.Active).ToList();
                    if (transactions.Count > 0)
                    {
                        var groupedTransactions = transactions.GroupBy(g => g.TimeBlockDateId.ToString() + "_" + g.ProductId.ToString() + "_" + g.GetAccountingIdString() + "_" + g.IsRetroactive + "_" + g.IsExtended);

                        foreach (var group in groupedTransactions)
                        {
                            if (group.Count() == 1)
                                continue;

                            var firstTransaction = group.FirstOrDefault();
                            var otherTransactions = group.Skip(1).ToList();

                            firstTransaction.Amount = group.Sum(a => a.Amount);
                            firstTransaction.AmountCurrency = group.Sum(a => a.AmountCurrency);
                            firstTransaction.AmountEntCurrency = group.Sum(a => a.AmountEntCurrency);
                            firstTransaction.AmountLedgerCurrency = group.Sum(a => a.AmountLedgerCurrency);
                            firstTransaction.VatAmount = group.Sum(a => a.VatAmount);
                            firstTransaction.VatAmountCurrency = group.Sum(a => a.VatAmountCurrency);
                            firstTransaction.VatAmountEntCurrency = group.Sum(a => a.VatAmountEntCurrency);
                            firstTransaction.VatAmountLedgerCurrency = group.Sum(a => a.VatAmountLedgerCurrency);
                            firstTransaction.Quantity = group.Sum(a => a.Quantity);
                            otherTransactions.ForEach(f => f.State = (int)SoeEntityState.Deleted);
                            batchTimePayrollTransactions.AddRange(group);
                        }

                        entities.BulkUpdate(batchTimePayrollTransactions);

                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Merged transactions for employeeNr {employee.EmployeeNr} {count}/{countEmployees} timestamp {DateTime.Now}");
                    }
                }
            }
        }

        private void MergeShiftsOnDemoCompany()
        {
            TimeScheduleManager m = new TimeScheduleManager(null);
            Random random = new Random();
            ConfigurationSetupUtil.Init();
            List<Employee> employees = new List<Employee>();
            using (CompEntities entities = new CompEntities())
            {
                employees = entities.Employee.Where(w => w.ActorCompanyId == 1750).ToList();
            }
            DateTime dateFrom = DateTime.Today;
            DateTime dateTo = new DateTime(2021, 12, 31);
            int count = 0;

            foreach (var employee in employees)
            {
                using (CompEntities entities = new CompEntities())
                {
                    count++;
                    CreateLogEntry(ScheduledJobLogLevel.Information, "MergeShiftsOnDemoCompany started EmployeeId " + employee.EmployeeId.ToString() + " " + DateTime.Now.ToShortDateTime());
                    var shifts = m.GetShifts(employee.EmployeeId, CalendarUtility.GetDatesInInterval(dateFrom, dateTo));
                    var changedShifts = new List<TimeScheduleTemplateBlock>();

                    var periodIds = shifts.Where(w => w.TimeScheduleTemplatePeriodId.HasValue).Select(s => s.TimeScheduleTemplatePeriodId.Value).Distinct().ToList();
                    var templateBlocks = entities.TimeScheduleTemplateBlock.Where(w => w.TimeScheduleTemplatePeriodId.HasValue && periodIds.Contains(w.TimeScheduleTemplatePeriodId.Value)).ToList();
                    templateBlocks = templateBlocks.Where(w => !w.Date.HasValue && w.ModifiedBy != "mergejob").ToList();

                    foreach (var date in CalendarUtility.GetDatesInInterval(dateFrom, dateTo))
                    {
                        var onDate = shifts.Where(w => w.Date == date).ToList();

                        if (onDate.Count(w => !w.IsBreak) > 2)
                        {
                            var templatePeriodOnDate = onDate.First().TimeScheduleTemplatePeriodId.Value;

                            var firstShift = onDate.OrderBy(o => o.StartTime).First();
                            var lastShift = onDate.OrderBy(o => o.StopTime).Last();
                            var inBetweenShifts = onDate.Where(w => !w.IsBreak && w.StartTime != firstShift.StartTime && w.StartTime != lastShift.StartTime).ToList();

                            if (!inBetweenShifts.IsNullOrEmpty())
                            {
                                var templatesOndate = templateBlocks.Where(w => w.TimeScheduleTemplatePeriodId == templatePeriodOnDate);

                                var middleTime = CalendarUtility.GetMiddleTime(firstShift.StopTime, lastShift.StartTime);

                                middleTime = middleTime.AddMinutes(new Random().Next(-100, 100));
                                middleTime = CalendarUtility.AdjustAccordingToInterval(middleTime, 0, 15);

                                if (templatesOndate.Any(a => a.CreatedBy == "middletime"))
                                    middleTime = templatesOndate.First(a => a.CreatedBy == "middletime").StopTime;

                                if (middleTime > firstShift.StartTime && middleTime < lastShift.StartTime)
                                {
                                    var firstTemplate = templateBlocks.FirstOrDefault(w => !w.IsBreak && w.TimeScheduleTemplatePeriodId == firstShift.TimeScheduleTemplatePeriodId && w.StartTime == firstShift.StartTime);
                                    var lastTemplate = templateBlocks.FirstOrDefault(w => !w.IsBreak && w.TimeScheduleTemplatePeriodId == lastShift.TimeScheduleTemplatePeriodId && w.StopTime == lastShift.StopTime);

                                    if (firstTemplate == null || lastTemplate == null)
                                        continue;

                                    var inBetweenTemplates = templateBlocks.Where(w => inBetweenShifts.Select(s => s.TimeScheduleTemplatePeriodId).Contains(w.TimeScheduleTemplatePeriodId) && !w.IsBreak && w.StartTime != firstTemplate.StartTime && w.StartTime != lastTemplate.StartTime).ToList();
                                    firstShift.StopTime = middleTime;
                                    lastShift.StartTime = middleTime;
                                    firstTemplate.StopTime = middleTime;
                                    firstTemplate.CreatedBy = "middletime";
                                    lastTemplate.StartTime = middleTime;

                                    firstShift.ModifiedBy = "mergejob";
                                    lastShift.ModifiedBy = "mergejob";
                                    firstTemplate.ModifiedBy = "mergejob";
                                    lastTemplate.ModifiedBy = "mergejob";

                                    changedShifts.Add(firstShift);
                                    if (!changedShifts.Any(a => a == firstTemplate))
                                        changedShifts.Add(firstTemplate);
                                    changedShifts.Add(lastShift);
                                    if (!changedShifts.Any(a => a == lastTemplate))
                                        changedShifts.Add(lastTemplate);

                                    inBetweenShifts.ForEach(f => f.State = (int)SoeEntityState.Deleted);
                                    inBetweenShifts.ForEach(f => f.ModifiedBy = "mergejob");
                                    inBetweenTemplates.ForEach(f => f.State = (int)SoeEntityState.Deleted);
                                    inBetweenTemplates.ForEach(f => f.ModifiedBy = "mergejob");
                                    changedShifts.AddRange(inBetweenShifts.Where(w => !changedShifts.Contains(w)));
                                    changedShifts.AddRange(inBetweenTemplates.Where(w => !changedShifts.Contains(w)));
                                }
                            }
                        }
                    }

                    entities.BulkUpdate(changedShifts);
                }
                GC.Collect();
            }
        }

    }
}
