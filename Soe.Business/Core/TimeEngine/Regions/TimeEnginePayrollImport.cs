using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Import schedules and transactions from PayrollImport
        /// </summary>
        /// <returns></returns>
        private PayrollImportOutputDTO TaskPayrollImport()
        {
            var (iDTO, oDTO) = InitTask<PayrollImportInputDTO, PayrollImportOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.PayrollImportHeadId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "PayrollImportHead");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                PayrollImportHeadDTO importHead = PayrollManager.GetPayrollImportHead(iDTO.PayrollImportHeadId, payrollImportEmployeeIds: iDTO.PayrollImportEmployeeIds, loadSchedule: true, loadTransactions: true, loadTransactionAccounts: true, loadTransactionLinks: true);
                if (importHead == null)
                    return new PayrollImportOutputDTO() { Result = new ActionResult((int)ActionResultSave.EntityIsNull, "PayrollImportHead") };
                if (importHead.Employees.IsNullOrEmpty())
                    return new PayrollImportOutputDTO() { Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(5018, "Anställda")) };

                TimeScheduleTemplateHead templateHead = TimeScheduleManager.GetCompanyZeroTemplateHead(entities, ActorCompanyId);
                if (templateHead == null)
                {
                    int timeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
                    if (timeCodeId == 0)
                        return new PayrollImportOutputDTO() { Result = new ActionResult(GetText(12102, "Företagsinställning för standard tidkod saknas")) };

                    templateHead = CreateEmptyTimeScheduleTemplateHead(CalendarUtility.GetFirstDateOfWeek(new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1)), timeCodeId);
                    if (templateHead == null)
                        return new PayrollImportOutputDTO() { Result = new ActionResult(GetText(12103, "En tom schemamall kunde inte hittas eller skapas")) };
                }

                List<int> employeeIds = importHead.Employees.Select(s => s.EmployeeId).Distinct().ToList();
                List<Employee> employees = GetEmployeesWithEmployment(employeeIds, true);
                List<TimeCodeBreak> timeCodeBreaks = TimeCodeManager.GetTimeCodes(taskEntities, actorCompanyId, SoeTimeCodeType.Break, loadPayrollProducts: false).OfType<TimeCodeBreak>().ToList();
                List<TimeCodeBreakGroup> timeCodeBreakGroups = TimeCodeManager.GetTimeCodeBreakGroups(taskEntities, actorCompanyId);
                Dictionary<int, List<TimeScheduleTemplateBlock>> existingScheduleByEmployee = GetScheduleBlocksForEmployeesWithTask(null, employeeIds, importHead.DateFrom, importHead.DateTo).Where(x => x.EmployeeId.HasValue).GroupBy(g => g.EmployeeId.Value).ToDictionary(k => k.Key, v => v.ToList());
                int? defaultAccountStdId = GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);
                AddEmployeeSchedulesToCache(GetEmployeeSchedules(employeeIds));
                AddTimeScheduleTemplateHeadsToCache(GetPersonalTemplateHeads(employeeIds));

                bool reCalculate = iDTO.ReCalculatePayroll;

                #endregion

                #region Process

                try
                {
                    using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromSeconds(1000), System.Transactions.IsolationLevel.ReadCommitted))
                    {

                        InitTransaction(transaction);
                        foreach (int employeeId in employeeIds)
                        {
                            Employee employee = employees.FirstOrDefault(f => f.EmployeeId == employeeId);
                            if (employee == null)
                                continue;

                            PayrollImportEmployeeDTO importEmployee = importHead.Employees.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId);
                            if (importEmployee == null)
                                continue;

                            oDTO.Result = ValidateDates(importEmployee, importHead, out DateTime dateFrom, out DateTime dateTo);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            List<PayrollImportEmployeeScheduleDTO> importSchedules;
                            List<PayrollImportEmployeeTransactionDTO> importTransactions;

                            var firstEmployment = employee.GetFirstEmployment();
                            if (firstEmployment.DateFrom > dateFrom)
                            {
                                importSchedules = importEmployee.GetScheduleToProcess().Where(w => w.Date >= firstEmployment.DateFrom).ToList();
                                importTransactions = importEmployee.GetTransactionsToProcess().Where(w => w.Date >= firstEmployment.DateFrom).ToList();
                            }
                            else
                            {
                                importSchedules = importEmployee.GetScheduleToProcess();
                                importTransactions = importEmployee.GetTransactionsToProcess();
                            }   


                            if (!importSchedules.Any() && !importTransactions.Any())
                                reCalculate = false;

                            var (placementsFromImport, scheduleBlocksFromImport) = CreateEmployeeScheduleFromPayrollImport(employee, templateHead, importSchedules, dateFrom, dateTo, timeCodeBreaks, timeCodeBreakGroups);
                            if (firstEmployment.DateFrom > dateFrom)
                                scheduleBlocksFromImport = scheduleBlocksFromImport.Where(w => w.Date >= firstEmployment.DateFrom).ToList();

                            oDTO.Result = DeletePayrollImportOverlappingSchedule(employeeId, existingScheduleByEmployee, scheduleBlocksFromImport);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SavePayrollImportPlacement(placementsFromImport);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            //Must be done after placement is finished
                            AddTimeScheduleTemplatePeriodsToCache(employeeId, dateFrom, dateTo);

                            oDTO.Result = SavePayrollImportSchedules(importSchedules, scheduleBlocksFromImport);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SavePayrollImportTransactionsPresence(importTransactions, employee, importHead.PaymentDate, defaultAccountStdId);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SavePayrollImportTransactionsAbsence(importEmployee, importTransactions);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SavePayrollImportAddedTransactions(importTransactions, employee, importHead.PaymentDate, defaultAccountStdId);
                            if (!oDTO.Result.Success)
                                return oDTO;

                        }

                        TryCommit(oDTO);
                    }

                    //Starts a new transactionscope
                    if (oDTO.Result.Success && reCalculate && importHead.PaymentDate.HasValue)
                        oDTO.Result = CalculatePayrollOnPayrollImportChange(employees, importHead.PaymentDate.Value);
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }

                #endregion
            }

            return oDTO;
        }

        /// <summary>
        /// Undo import of schedules and transactions
        /// </summary>
        /// <returns></returns>
        private RollbackPayrollImportOutputDTO TaskRollbackPayrollImport()
        {
            var (iDTO, oDTO) = InitTask<RollbackPayrollImportInputDTO, RollbackPayrollImportOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.PayrollImportHeadId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "PayrollImportHead");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                try
                {
                    InitContext(taskEntities);

                    PayrollImportHead importHead = GetPayrollImportHead(iDTO.PayrollImportHeadId);
                    if (importHead == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "PayrollImportHead");
                        return oDTO;
                    }

                    List<PayrollImportEmployee> importEmployees = GetPayrollImportEmployeesForRollback(iDTO.PayrollImportHeadId, iDTO.PayrollImportEmployeeIds, iDTO.RollbackFileContentForAllEmployees, iDTO.RollbackOutcomeForAllEmployees);

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Load

                        Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleByEmployee = GetScheduleBlocksForEmployeesWithTask(null, importEmployees.Select(x => x.EmployeeId).ToList(), importHead.DateFrom, importHead.DateTo).Where(x => x.EmployeeId.HasValue).GroupBy(g => g.EmployeeId.Value).ToDictionary(k => k.Key, v => v.ToList());
                        Dictionary<int, List<TimeBlock>> timeBlocksByEmployee = GetTimeBlocksFromPayrollImportLinks(importEmployees).GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
                        Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsByEmployee = GetTimePayrollTransactionsFromPayrollImportLinks(importEmployees).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                        Dictionary<int, List<TimePayrollScheduleTransaction>> timeScheduleTransactionsByEmployee = GetTimePayrollScheduleTransactions(importEmployees.Select(x => x.EmployeeId).ToList(), importHead.DateFrom, importHead.DateTo, null, SoeTimePayrollScheduleTransactionType.Absence, false).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

                        #endregion

                        #region Perform

                        if (iDTO.IsRollbackFileContentMode && iDTO.RollbackFileContentForAllEmployees)
                        {
                            importHead.State = (int)SoeEntityState.Deleted;
                            SetModifiedProperties(importHead);
                        }

                        foreach (PayrollImportEmployee payrollImportEmployee in importEmployees)
                        {
                            if (iDTO.IsRollbackFileContentMode)
                            {
                                payrollImportEmployee.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(payrollImportEmployee);
                            }

                            foreach (PayrollImportEmployeeTransaction payrollImportEmployeeTransaction in payrollImportEmployee.PayrollImportEmployeeTransaction.Where(x => x.State == (int)SoeEntityState.Active))
                            {
                                if (iDTO.IsRollbackFileContentMode)
                                    payrollImportEmployeeTransaction.State = (int)SoeEntityState.Deleted;
                                else
                                    payrollImportEmployeeTransaction.Status = (int)TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed;

                                SetModifiedProperties(payrollImportEmployeeTransaction);

                                foreach (var payrollImportEmployeeTransactionLink in payrollImportEmployeeTransaction.PayrollImportEmployeeTransactionLink.Where(x => x.State == (int)SoeEntityState.Active))
                                {
                                    payrollImportEmployeeTransactionLink.State = (int)SoeEntityState.Deleted;
                                    SetModifiedProperties(payrollImportEmployeeTransactionLink);
                                }
                            }

                            foreach (PayrollImportEmployeeSchedule payrollImportEmployeeSchedule in payrollImportEmployee.PayrollImportEmployeeSchedule.Where(x => x.State == (int)SoeEntityState.Active))
                            {
                                if (iDTO.IsRollbackFileContentMode)
                                    payrollImportEmployeeSchedule.State = (int)SoeEntityState.Deleted;
                                else
                                    payrollImportEmployeeSchedule.Status = (int)TermGroup_PayrollImportEmployeeScheduleStatus.Unprocessed;

                                SetModifiedProperties(payrollImportEmployeeSchedule);
                            }

                            oDTO.Result = SetScheduleToDeleted(scheduleByEmployee.GetList(payrollImportEmployee.EmployeeId), saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SetTimePayrollTransactionsToDeleted(timePayrollTransactionsByEmployee.GetList(payrollImportEmployee.EmployeeId), saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SetTimeBlocksAndTransactionsToDeleted(timeBlocksByEmployee.GetList(payrollImportEmployee.EmployeeId), saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SetTimePayrollScheduleTransactionsToDeleted(timeScheduleTransactionsByEmployee.GetList(payrollImportEmployee.EmployeeId), saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        #endregion

                        TrySaveAndCommit(oDTO);
                    }

                    //Starts a new transactionscope
                    if (oDTO.Result.Success && iDTO.ReCalculatePayroll && importHead.PaymentDate.HasValue)
                        oDTO.Result = CalculatePayrollOnPayrollImportChange(GetEmployeesWithEmployment(importEmployees.Select(s => s.EmployeeId).Distinct().ToList(), true), importHead.PaymentDate.Value);

                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Validates the PayrollImport before it is called
        /// </summary>
        /// <returns>Output DTO</returns>
        private ValidatePayrollImportOutputDTO TaskValidatePayrollImport()
        {
            var (iDTO, oDTO) = InitTask<ValidatePayrollImportInputDTO, ValidatePayrollImportOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<string> employeesAndDatesWithSchedule = new List<string>();
            List<string> employeesAndDatesWithAbsence = new List<string>();
            List<string> employeesAndDatesWithPresence = new List<string>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                PayrollImportHeadDTO importHead = PayrollManager.GetPayrollImportHead(iDTO.PayrollImportHeadId, payrollImportEmployeeIds: iDTO.PayrollImportEmployeeIds, loadSchedule: true, loadTransactions: true);
                if (importHead != null && !importHead.Employees.IsNullOrEmpty())
                {
                    foreach (PayrollImportEmployeeDTO importEmployee in importHead.Employees)
                    {
                        oDTO.Result = ValidateDates(importEmployee, importHead, out DateTime dateFrom, out DateTime dateTo);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        List<PayrollImportEmployeeScheduleDTO> importSchedule = importEmployee.GetScheduleToProcess();
                        if (importSchedule.Any())
                        {
                            List<TimeScheduleTemplateBlock> existingSchedule = GetScheduleBlocksForEmployeeWithoutScenario(importEmployee.EmployeeId, dateFrom, dateTo);
                            if (!existingSchedule.Any())
                                continue;

                            List<DateTime> importScheduleDates = importSchedule.Select(i => i.Date).Distinct().ToList();
                            if (importScheduleDates.Any())
                            {
                                List<DateTime> existingScheduleDates = existingSchedule.Select(i => i.Date.Value).Distinct().ToList();
                                if (existingScheduleDates.Any())
                                    AddOverlappingScheduleDays(importEmployee.EmployeeId, importScheduleDates.Intersect(existingScheduleDates).ToList());
                            }
                        }

                        List<PayrollImportEmployeeTransactionDTO> importTransactions = importEmployee.GetTransactionsToProcess();
                        if (importTransactions.Any())
                        {
                            List<TimePayrollTransaction> existingTransactions = GetTimePayrollTransactionsWithTimeBlockDate(importEmployee.EmployeeId, dateFrom, dateTo);
                            if (!existingTransactions.Any())
                                continue;

                            List<DateTime> importPresenceDates = importTransactions.GetPresence().Select(i => i.Date).Distinct().ToList();
                            if (importPresenceDates.Any())
                            {
                                List<DateTime> existingPresenceDates = existingTransactions.Where(tpt => !tpt.IsAbsence()).Select(i => i.TimeBlockDate.Date).Distinct().ToList();
                                if (existingPresenceDates.Any())
                                    AddOverlappingPresenceDays(importEmployee.EmployeeId, existingPresenceDates.Intersect(importPresenceDates).ToList());
                            }

                            List<DateTime> importAbsenceDates = importTransactions.GetAbsence().Select(i => i.Date).Distinct().ToList();
                            if (importAbsenceDates.Any())
                            {
                                List<DateTime> existingAbsenceDates = existingTransactions.Where(tpt => tpt.IsAbsence()).Select(i => i.TimeBlockDate.Date).Distinct().ToList();
                                if (existingAbsenceDates.Any())
                                    AddOverlappingAbsencedays(importEmployee.EmployeeId, existingAbsenceDates.Intersect(importAbsenceDates).ToList());
                            }
                        }
                    }
                }
            }

            oDTO.Result.ErrorMessage = null;
            oDTO.Result.CanUserOverride = true;

            if (employeesAndDatesWithSchedule.Any())
                AddErrorMessage(GetText(12113, "Schema finns redan på"), employeesAndDatesWithSchedule);
            if (employeesAndDatesWithAbsence.Any())
                AddErrorMessage(GetText(12110, "Frånvaro finns redan på"), employeesAndDatesWithAbsence, canUserOverride: false);
            else if (employeesAndDatesWithPresence.Any())
                AddErrorMessage(GetText(12111, "Närvaro finns redan på"), employeesAndDatesWithPresence);

            if (oDTO.Result.CanUserOverride)
                AddQuestionMessage();

            return oDTO;

            void AddOverlappingScheduleDays(int employeeId, List<DateTime> dates)
            {
                if (dates.Any())
                    employeesAndDatesWithSchedule.Add(GetMessage(employeeId, dates));
            }
            void AddOverlappingPresenceDays(int employeeId, List<DateTime> dates)
            {
                if (dates.Any())
                    employeesAndDatesWithPresence.Add(GetMessage(employeeId, dates));
            }
            void AddOverlappingAbsencedays(int employeeId, List<DateTime> dates)
            {
                if (dates.Any())
                    employeesAndDatesWithAbsence.Add(GetMessage(employeeId, dates));
            }
            string GetMessage(int employeeId, List<DateTime> dates)
            {
                return $"{(GetEmployeeWithContactPersonFromCache(employeeId)?.Name ?? employeeId.ToString())} ({dates.Select(d => d.ToShortDateString()).ToCommaSeparated()})";
            }
            void AddErrorMessage(string header, List<string> messages, bool canUserOverride = true)
            {
                StringBuilder sb = new StringBuilder();
                if (!oDTO.Result.ErrorMessage.IsNullOrEmpty())
                    sb.Append("\r\n");
                sb.Append(header);
                sb.Append(":");
                sb.Append("\r\n");
                sb.Append("\r\n");
                foreach (string message in messages)
                {
                    sb.Append(message);
                    sb.Append(". ");
                    sb.Append("\r\n");
                }

                oDTO.Result.ErrorMessage += sb.ToString();
                oDTO.Result.Success = false;
                if (!canUserOverride)
                    oDTO.Result.CanUserOverride = false;
            }
            void AddQuestionMessage()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\r\n");
                sb.Append(GetText(8494, "Vill du fortsätta?"));
                oDTO.Result.ErrorMessage += sb.ToString();
            }
        }

        #endregion

        #region Create schedule

        private (List<SaveEmployeeSchedulePlacementItem> placements, List<TimeScheduleTemplateBlockDTO> scheduleBlocks) CreateEmployeeScheduleFromPayrollImport(Employee employee, TimeScheduleTemplateHead templateHead, List<PayrollImportEmployeeScheduleDTO> importSchedulesForEmployee, DateTime startDate, DateTime stopDate, List<TimeCodeBreak> timeCodeBreaks, List<TimeCodeBreakGroup> timeCodeBreakGroups)
        {
            List<SaveEmployeeSchedulePlacementItem> placements = new List<SaveEmployeeSchedulePlacementItem>();
            List<TimeScheduleTemplateBlockDTO> scheduleBlocks = new List<TimeScheduleTemplateBlockDTO>();

            if (employee == null || templateHead == null)
                return (placements, scheduleBlocks);

            List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedulesForEmployee(employee.EmployeeId);
            DateTime firstEmployeeScheduleDate = startDate;

            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                Guid link = Guid.NewGuid();

                List<PayrollImportEmployeeScheduleDTO> importSchedulesForEmployeeAndDate = importSchedulesForEmployee?.Where(w => w.Date == currentDate).ToList() ?? new List<PayrollImportEmployeeScheduleDTO>();
                if (importSchedulesForEmployeeAndDate.Any() && importSchedulesForEmployeeAndDate.All(a => a.Quantity > 0))
                {
                    foreach (PayrollImportEmployeeScheduleDTO importSchedule in importSchedulesForEmployeeAndDate)
                    {
                        TimeCodeBreak timeCode = null;
                        if (importSchedule.IsBreak)
                        {
                            timeCode = TimeCodeManager.GetTimeCodeBreakForLength(actorCompanyId, employee.GetEmployeeGroupId(currentDate), Convert.ToInt32(importSchedule.Quantity), timeCodeBreaks, timeCodeBreakGroups);
                            if (timeCode == null)
                                continue;
                        }

                        scheduleBlocks.Add(importSchedule.CreateScheduleBlock(employee.EmployeeId, timeCode?.TimeCodeId ?? 0, link));
                    }
                }
                else
                {
                    bool hasSchedule = importSchedulesForEmployeeAndDate.Any() || HasScheduleBlocksForEmployeeWithoutScenario(employee.EmployeeId, currentDate);
                    if (!hasSchedule)
                    {
                        PayrollImportEmployeeScheduleDTO importSchedule = new PayrollImportEmployeeScheduleDTO()
                        {
                            Date = currentDate,
                            StartTime = CalendarUtility.DATETIME_DEFAULT,
                            StopTime = CalendarUtility.DATETIME_DEFAULT,
                            Quantity = 0
                        };
                        scheduleBlocks.Add(importSchedule.CreateScheduleBlock(employee.EmployeeId, 0, link));
                    }
                }

                if (HasEmployeePlacement(employee.EmployeeId, currentDate, currentDate, employeeSchedules))
                    firstEmployeeScheduleDate = currentDate.AddDays(1);

                if (currentDate != stopDate)
                    currentDate = currentDate.AddDays(1);
                else
                    break;
            }

            if (firstEmployeeScheduleDate <= stopDate)
            {
                scheduleBlocks = scheduleBlocks.Where(w => w.Date >= firstEmployeeScheduleDate).ToList();

                DateTime dayBefore = firstEmployeeScheduleDate.AddDays(-1);
                EmployeeSchedule dockedEmployeeSchedule = entities.EmployeeSchedule.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId && f.StopDate == dayBefore && f.TimeScheduleTemplateHeadId == templateHead.TimeScheduleTemplateHeadId);

                ActivateScheduleGridDTO activateSchedule = new ActivateScheduleGridDTO()
                {
                    EmployeeScheduleId = dockedEmployeeSchedule?.EmployeeScheduleId ?? 0,
                    IsPlaced = false,
                    IsPreliminary = false,
                    EmployeeScheduleStartDate = dockedEmployeeSchedule?.StartDate ?? firstEmployeeScheduleDate,
                    EmployeeScheduleStopDate = stopDate,
                    EmployeeScheduleStartDayNumber = dockedEmployeeSchedule?.StartDayNumber ?? 0,
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = employee.EmployeeNr,
                    EmployeeName = employee.Name,
                    TimeScheduleTemplateHeadId = templateHead.TimeScheduleTemplateHeadId,
                    TimeScheduleTemplateHeadName = templateHead.Name,
                    TemplateEmployeeId = 0,
                    TemplateStartDate = null,
                    EmployeeGroupId = 0,
                    EmployeeGroupName = "",
                };

                SaveEmployeeSchedulePlacementItem placement = SaveEmployeeSchedulePlacementItem.Create(
                    activateSchedule,
                    dockedEmployeeSchedule == null ? TermGroup_TemplateScheduleActivateFunctions.NewPlacement : TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate,
                    templateHead.TimeScheduleTemplateHeadId,
                    0,
                    dockedEmployeeSchedule == null ? firstEmployeeScheduleDate : (DateTime?)null,
                    stopDate,
                    false,
                    activateSchedule.EmployeeId,
                    createTimeBlocksAndTransactionsAsync: false);
                placements.Add(placement);

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, currentDate, true);
            }

            return (placements, scheduleBlocks);
        }

        private TimeScheduleTemplateHead CreateEmptyTimeScheduleTemplateHead(DateTime startDate, int timeCodeId)
        {
            TimeScheduleTemplateHead timeScheduleTemplateHead = new TimeScheduleTemplateHead
            {
                Name = "Empty",
                NoOfDays = 1,
                ActorCompanyId = actorCompanyId,
                StartDate = CalendarUtility.GetBeginningOfDay(startDate),
                FirstMondayOfCycle = CalendarUtility.GetBeginningOfDay(startDate),
                StartOnFirstDayOfWeek = false,
                SimpleSchedule = false,
                FlexForceSchedule = false,
                Locked = false,
                State = (int)SoeEntityState.Active,
            };
            entities.TimeScheduleTemplateHead.AddObject(timeScheduleTemplateHead);

            TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = new TimeScheduleTemplatePeriod
            {
                DayNumber = 1,
                TimeScheduleTemplateHead = timeScheduleTemplateHead,
            };
            entities.TimeScheduleTemplatePeriod.AddObject(timeScheduleTemplatePeriod); 
            TimeScheduleTemplateBlock timeScheduleTemplateBlock = new TimeScheduleTemplateBlock
            {
                TimeScheduleTemplatePeriod = timeScheduleTemplatePeriod,
                TimeCodeId = timeCodeId,
                AccountId = null,
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                Type = (int)TermGroup_TimeScheduleTemplateBlockType.Schedule,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                BreakNumber = 0,
                ShiftStatus = (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned,
                NbrOfSuggestionsInQueue = 0,
                NbrOfWantedInQueue = 0,
                State = (int)SoeEntityState.Active,
                TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.None,
            };
            entities.TimeScheduleTemplateBlock.AddObject(timeScheduleTemplateBlock);
            ActionResult result = Save();
            if (!result.Success)
                return null;

            return timeScheduleTemplateHead;
        }

        #endregion

        #region PayrollImportHead

        private PayrollImportHead GetPayrollImportHead(int payrollImportHeadId)
        {
            return (from pih in entities.PayrollImportHead
                    where pih.ActorCompanyId == actorCompanyId &&
                    pih.PayrollImportHeadId == payrollImportHeadId &&
                    pih.State == (int)SoeEntityState.Active
                    select pih).FirstOrDefault();
        }

        #endregion

        #region PayrollImportEmployee

        private List<PayrollImportEmployee> GetPayrollImportEmployeesForRollback(int payrollImportHeadId, List<int> payrollImportEmployeeIds, bool rollbackFileContentForAllEmployees, bool rollbackOutcomeForAllEmployees)
        {
            IQueryable<PayrollImportEmployee> oQuery = entities.PayrollImportEmployee;
            oQuery = oQuery.Include("PayrollImportEmployeeSchedule")
                           .Include("PayrollImportEmployeeTransaction")
                           .Include("PayrollImportEmployeeTransaction.PayrollImportEmployeeTransactionAccountInternal")
                           .Include("PayrollImportEmployeeTransaction.PayrollImportEmployeeTransactionLink");

            IQueryable<PayrollImportEmployee> query = (from h in oQuery
                                                       where h.PayrollImportHeadId == payrollImportHeadId &&
                                                       h.State == (int)SoeEntityState.Active
                                                       select h);

            if (!rollbackFileContentForAllEmployees && !rollbackOutcomeForAllEmployees)
                query = query.Where(pie => payrollImportEmployeeIds.Contains(pie.PayrollImportEmployeeId));

            return query.ToList();
        }

        private ActionResult ValidateDates(PayrollImportEmployeeDTO importEmployee, PayrollImportHeadDTO importHead, out DateTime validDateFrom, out DateTime validDateTo)
        {
            int maxDays = 365;
            DateTime? startDate = importEmployee.GetDateFrom(importHead);
            DateTime? stopDate = importEmployee.GetDateTo(importHead);
            if (!startDate.HasValue || !stopDate.HasValue || (stopDate.Value - startDate.Value).TotalDays > maxDays)
            {
                validDateFrom = CalendarUtility.DATETIME_DEFAULT;
                validDateTo = CalendarUtility.DATETIME_DEFAULT;
                return new ActionResult($"{GetText(12104, "Datumintervallet i filen överstiger max antal dagar: ")} {maxDays}");
            }
            else
            {
                validDateFrom = startDate.Value;
                validDateTo = stopDate.Value;
                return new ActionResult(true);
            }
        }

        private ActionResult DeletePayrollImportOverlappingSchedule(int employeeId, Dictionary<int, List<TimeScheduleTemplateBlock>> existingScheduleByEmployee, List<TimeScheduleTemplateBlockDTO> scheduleBlocksFromImport)
        {
            if (existingScheduleByEmployee.IsNullOrEmpty() || !existingScheduleByEmployee.ContainsKey(employeeId) || scheduleBlocksFromImport.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimeScheduleTemplateBlock> existingScheduleForEmployee = existingScheduleByEmployee[employeeId].Where(s => s.Date.HasValue).ToList();
            foreach (var existingSchedulesOnEmployeeByDate in existingScheduleForEmployee.GroupBy(g => g.Date.Value))
            {
                DateTime date = existingSchedulesOnEmployeeByDate.Key;
                if (!scheduleBlocksFromImport.Any(a => a.Date == date))
                    continue;

                ActionResult result = SetScheduleToDeleted(existingSchedulesOnEmployeeByDate.ToList(), saveChanges: false);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        private ActionResult CalculatePayrollOnPayrollImportChange(List<Employee> employees, DateTime paymentDate)
        {
            var companyDTO = new ReCalculatePayrollPeriodCompanyDTO();

            var result = SetUpReCalculatePayrollPeriodCompanyDTO(companyDTO, employeeIds: employees?.Select(e => e.EmployeeId).ToList());
            if (!result.Success)
                return result;

            List<int> timePeriodIds = entities.TimePeriod.Where(w => w.TimePeriodHead.ActorCompanyId == ActorCompanyId && w.TimePeriodHead.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active && w.PaymentDate == paymentDate).Select(s => s.TimePeriodId).ToList();
            Dictionary<int, List<int>> validPeriods = PayrollManager.GetValidEmployeesForTimePeriod(entities, ActorCompanyId, timePeriodIds, employees, GetPayrollGroupsFromCache(), false);

            foreach (var periodItem in validPeriods.Where(w => w.Value.Any()))
            {
                result = ReCalculatePayrollPeriod(companyDTO, periodItem.Value, periodItem.Key, false, true, false, out string baseMessage, out string errorMessage);
                if (!result.Success)
                    return result;
            }

            return result;
        }

        private ActionResult SavePayrollImportPlacement(List<SaveEmployeeSchedulePlacementItem> placementsFromImport)
        {
            if (placementsFromImport.IsNullOrEmpty())
                return new ActionResult(true);

            var validationResult = ValidateSchedulePlacement(placementsFromImport);
            if (!validationResult.Result.Success)
                return validationResult.Result;

            List<TimeScheduleTemplateBlock> asyncTemplateBlocks = null;
            return SaveEmployeeSchedulePlacement(validationResult, ref asyncTemplateBlocks);
        }

        private ActionResult SavePayrollImportSchedules(List<PayrollImportEmployeeScheduleDTO> importSchedules, List<TimeScheduleTemplateBlockDTO> scheduleBlocksFromImport)
        {
            List<TimeSchedulePlanningDayDTO> planningDays = scheduleBlocksFromImport?.ToTimeSchedulePlanningDayDTOs(groupOnDateAndEmployeeInsteadOfPeriod: true);
            if (planningDays.IsNullOrEmpty())
                return new ActionResult(true);

            ActionResult result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.ImportPayroll, planningDays, true, true, false, 0, null);
            if (result.Success)
                result = UpdatePayrollImportEmployeeScheduleStatus(importSchedules, TermGroup_PayrollImportEmployeeScheduleStatus.Processed);

            return result;
        }

        private ActionResult SavePayrollImportTransactionsPresence(List<PayrollImportEmployeeTransactionDTO> importTransactions, Employee employee, DateTime? paymentDate, int? defaultAccountStdId)
        {
            List<PayrollImportEmployeeTransactionDTO> importTransactionsPresnce = importTransactions.GetPresence();
            List<AttestPayrollTransactionDTO> payrollTransactions = importTransactionsPresnce?.CreateTransactionItems(employee.EmployeeId, defaultAccountStdId);
            if (payrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            if (paymentDate.HasValue && paymentDate != DateTime.MinValue)
                SetTimePeriodOnPayrollTransactions(payrollTransactions, employee, paymentDate.Value);

            ActionResult result = SaveTimePayrollTransactions(employee, payrollTransactions, keepExistingTransactions: true);
            if (result.Success)
                result = UpdatePayrollImportEmployeeTransactionStatus(importTransactions, TermGroup_PayrollImportEmployeeTransactionStatus.Processed);

            return result;
        }
        
        private ActionResult SavePayrollImportAddedTransactions(List<PayrollImportEmployeeTransactionDTO> importTransactions, Employee employee, DateTime? paymentDate, int? defaultAccountStdId)
        {
            List<PayrollImportEmployeeTransactionDTO> importTransactionsPresnce = importTransactions.FixedAmount();
            List<AttestPayrollTransactionDTO> payrollTransactions = importTransactionsPresnce?.CreateTransactionItems(employee.EmployeeId, defaultAccountStdId);
            TimePeriod period = null;
            if (payrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            if (paymentDate.HasValue && paymentDate != DateTime.MinValue)
            {
                List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache();
                var lastDate = payrollTransactions.OrderBy(g => g.Date).LastOrDefault()?.Date;
                PayrollGroup payrollGroup = null;

                if (lastDate.HasValue)
                {
                    payrollGroup = employee.GetPayrollGroup(lastDate.Value, payrollGroups: payrollGroups);
                    if (payrollGroup?.TimePeriodHeadId != null)
                    {
                        List<TimePeriod> periods = GetTimePeriodsFromCache(payrollGroup.TimePeriodHeadId.Value);
                        period = periods?.FirstOrDefault(tp => tp.PaymentDate.HasValue && tp.PaymentDate == paymentDate);
                    }
                }
            }
            ActionResult result = new ActionResult();

            if (period == null)
            {
                result.Success = false;
                result.ErrorMessage = GetText(8250, "Löneperiod hittades inte");
                return result;
            }

            foreach (AttestPayrollTransactionDTO payrollTransaction in payrollTransactions) //NOSONAR
            {
                payrollTransaction.IsAdded = true;
                payrollTransaction.IsSpecifiedUnitPrice = true;
                payrollTransaction.AddedDateFrom = payrollTransaction.AddedDateTo = payrollTransaction.Date;
                payrollTransaction.UnitPrice = payrollTransaction.Amount;
                payrollTransaction.TimePeriodId = period.TimePeriodId;
                result = SaveAddedTransaction(payrollTransaction, null, payrollTransaction.AccountingSettings, payrollTransaction.EmployeeId, payrollTransaction.TimePeriodId.Value);

                if (!result.Success)
                    return result;
            }
            
            if (result.Success)
                result = UpdatePayrollImportEmployeeTransactionStatus(importTransactions, TermGroup_PayrollImportEmployeeTransactionStatus.Processed);

            return result;
        }
        
        private ActionResult SavePayrollImportTransactionsAbsence(PayrollImportEmployeeDTO importEmployee, List<PayrollImportEmployeeTransactionDTO> importTransactions)
        {
            List<PayrollImportEmployeeTransactionDTO> importTransactionsAbsence = importTransactions.GetAbsence();
            if (importTransactionsAbsence.IsNullOrEmpty())
                return new ActionResult(true);

            return SaveAbsenceFromPayrollImport(importEmployee);
        }

        private ActionResult SaveAbsenceFromPayrollImport(PayrollImportEmployeeDTO importEmployee)
        {
            ActionResult result = new ActionResult(true);

            if (importEmployee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PayrollImportEmployeeDTO");

            List<PayrollImportEmployeeTransactionDTO> absenceTransactions = importEmployee.Transactions?.Where(t => t.TimeDeviationCauseId.HasValue).ToList();
            if (absenceTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(importEmployee.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            foreach (var absenceTransactionsByDate in absenceTransactions.GroupBy(g => g.Date))
            {
                DateTime date = absenceTransactionsByDate.Key;
                List<TimeEngineDay> days = new List<TimeEngineDay>();

                #region Schedule

                List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksFromCache(employee.EmployeeId, date).ToList();
                if (templateBlocks.IsNullOrEmpty())
                    continue;

                int scheduleMinutes = templateBlocks.Where(w => !w.IsBreak).Sum(s => s.TotalMinutes);
                int transactionMinutes = Convert.ToInt32(Decimal.Floor(absenceTransactionsByDate.Sum(s => s.Quantity)));
                if (transactionMinutes > scheduleMinutes)
                    return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(12082, "Dagen innehåller mer frånvaro än schema") + $" ({date.ToShortDateString()}: {transactionMinutes}>{scheduleMinutes})");

                int? templatePeriodId = GetTimeScheduleTemplatePeriodIdFromCache(employee.EmployeeId, date);

                #endregion

                #region TimeBlockDate

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date, true);

                result = Save();
                if (!result.Success)
                    return result;

                #endregion

                #region Absence

                DateTime startTime = templateBlocks.First().StartTime;

                foreach (var absenceTransactionsByDeviationCause in absenceTransactionsByDate.GroupBy(i => i.TimeDeviationCauseId.Value))
                {
                    TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(absenceTransactionsByDeviationCause.Key);
                    if (timeDeviationCause == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8813, "Orsak kunde inte hittas"));

                    List<TimeBlock> absenceTimeBlocks = new List<TimeBlock>();
                    foreach (var absenceTransaction in absenceTransactionsByDeviationCause)
                    {
                        DateTime stopTime = startTime.AddMinutes(Convert.ToDouble(absenceTransaction.Quantity));
                        stopTime = stopTime.AddMinutes(templateBlocks.Where(w => w.IsBreak && w.StartTime > startTime && w.StopTime < stopTime.AddMinutes(w.TotalMinutes)).Sum(s => s.TotalMinutes));
                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());

                        TimeBlock timeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, startTime, stopTime, timeBlockDate, employeeGroup, templatePeriodId, templateBlocks, timeDeviationCause);
                        if (timeBlock == null)
                            continue;

                        timeBlock.PayrollImportEmployeeTransactionId = absenceTransaction.PayrollImportEmployeeTransactionId;
                        timeBlock.AccountStdId = absenceTransaction.AccountStdId ?? (GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost) ?? 0);
                        AddAccountInternalsToTimeBlock(timeBlock, absenceTransaction.AccountInternals.Where(a => a.AccountId.HasValue).Select(a => a.AccountId.Value));

                        absenceTimeBlocks.Add(timeBlock);
                        startTime = stopTime;
                    }

                    foreach (var dateGroup in absenceTimeBlocks.GroupBy(g => g.TimeBlockDate))
                    {
                        days.AddDay(
                            templatePeriodId: dateGroup.First().TimeScheduleTemplatePeriodId,
                            timeBlockDate: dateGroup.Key, 
                            timeBlocks: absenceTimeBlocks
                            );
                    }
                }

                if (!days.Any())
                    continue;

                result = Save();
                if (!result.Success)
                    return result;

                #endregion

                #region PayrollImportEmployeeTransactionLink

                result = SavePayrollImportEmployeeTransactionLinks(date, importEmployee.EmployeeId, days.SelectMany(d => d.TimeBlocks));
                if (!result.Success)
                    return result;

                #endregion

                #region Transactions

                result = SaveTransactionsForPeriods(days);
                if (result.Success)
                    result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Apply, employee.EmployeeId);

                #endregion
            }

            return result;
        }

        private ActionResult UpdatePayrollImportEmployeeScheduleStatus(List<PayrollImportEmployeeScheduleDTO> importSchedules, TermGroup_PayrollImportEmployeeScheduleStatus status)
        {
            List<int> ids = importSchedules?.Select(s => s.PayrollImportEmployeeScheduleId).ToList() ?? new List<int>();
            if (ids.IsNullOrEmpty())
                return new ActionResult(true);

            List<PayrollImportEmployeeSchedule> schedules = entities.PayrollImportEmployeeSchedule.Where(f => ids.Contains(f.PayrollImportEmployeeScheduleId)).ToList();
            if (schedules.IsNullOrEmpty())
                return new ActionResult(true);

            foreach (PayrollImportEmployeeSchedule schedule in schedules)
            {
                schedule.Status = (int)status;
                SetModifiedProperties(schedule);
            }

            return Save();
        }

        private ActionResult UpdatePayrollImportEmployeeTransactionStatus(List<PayrollImportEmployeeTransactionDTO> importTransactions, TermGroup_PayrollImportEmployeeTransactionStatus status)
        {
            List<int> ids = importTransactions?.Select(s => s.PayrollImportEmployeeTransactionId).ToList() ?? new List<int>();
            if (ids.IsNullOrEmpty())
                return new ActionResult(true);

            List<PayrollImportEmployeeTransaction> transactions = entities.PayrollImportEmployeeTransaction.Where(f => ids.Contains(f.PayrollImportEmployeeTransactionId)).ToList();
            if (transactions.IsNullOrEmpty())
                return new ActionResult(true);

            foreach (PayrollImportEmployeeTransaction transaction in transactions)
            {
                transaction.Status = (int)status;
                SetModifiedProperties(transaction);
            }

            return Save();
        }

        private void SetTimePeriodOnPayrollTransactions(List<AttestPayrollTransactionDTO> payrollTransactions, Employee employee, DateTime paymentDate)
        {
            List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache();
            var lastDate = payrollTransactions.OrderBy(g => g.Date).LastOrDefault()?.Date;
            PayrollGroup payrollGroup = null;

            if (lastDate.HasValue)
            {
                payrollGroup = employee.GetPayrollGroup(lastDate.Value, payrollGroups: payrollGroups);
                if (payrollGroup?.TimePeriodHeadId == null)
                    return;

                foreach (var transactionsOnDate in payrollTransactions.GroupBy(i => i.Date))
                {
                    DateTime date = transactionsOnDate.Key;
                    List<TimePeriod> periods = GetTimePeriodsFromCache(payrollGroup.TimePeriodHeadId.Value);
                    TimePeriod period = periods?.FirstOrDefault(tp => tp.PaymentDate.HasValue && tp.PaymentDate == paymentDate);
                    if (period == null)
                        continue;
                    if (period.StartDate <= date && period.StopDate >= date)
                        continue;

                    transactionsOnDate.ToList().ForEach(f => f.TimePeriodId = period.TimePeriodId);
                }
            }
        }

        #endregion

        #region PayrollImportEmployeeTransactionLink

        private List<TimePayrollTransaction> GetTimePayrollTransactionsFromPayrollImportLinks(List<PayrollImportEmployee> payrollImportEmployees)
        {
            List<TimePayrollTransaction> transactions = new List<TimePayrollTransaction>();

            foreach (PayrollImportEmployee payrollImportEmployee in payrollImportEmployees)
            {
                List<int> timepayrollTransactionIds = payrollImportEmployee.PayrollImportEmployeeTransaction.SelectMany(s => s.PayrollImportEmployeeTransactionLink.Where(w => w.State == (int)SoeEntityState.Active && w.TimePayrollTransactionId.HasValue).Select(ss => ss.TimePayrollTransactionId.Value)).ToList();
                transactions.AddRange(entities.TimePayrollTransaction.Where(w => w.EmployeeId == payrollImportEmployee.EmployeeId && w.State == (int)SoeEntityState.Active && timepayrollTransactionIds.Contains(w.TimePayrollTransactionId)).ToList());
            }

            return transactions;
        }

        private List<TimeBlock> GetTimeBlocksFromPayrollImportLinks(List<PayrollImportEmployee> payrollImportEmployees)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();

            foreach (PayrollImportEmployee payrollImportEmployee in payrollImportEmployees)
            {
                List<int> timeBlockIds = payrollImportEmployee.PayrollImportEmployeeTransaction.SelectMany(s => s.PayrollImportEmployeeTransactionLink.Where(w => w.State == (int)SoeEntityState.Active && w.TimeBlockId.HasValue).Select(ss => ss.TimeBlockId.Value)).ToList();
                timeBlocks.AddRange(entities.TimeBlock.Include("TimeCodeTransaction.TimePayrollTransaction").Where(w => w.EmployeeId == payrollImportEmployee.EmployeeId && w.State == (int)SoeEntityState.Active && timeBlockIds.Contains(w.TimeBlockId)).ToList());
            }

            return timeBlocks;
        }

        private ActionResult SavePayrollImportEmployeeTransactionLinks(DateTime date, int employeeId, IEnumerable<TimeBlock> timeBlocks)
        {
            List<TimeBlock> validTimeBlocks = timeBlocks?.Where(tb => tb.PayrollImportEmployeeTransactionId.HasValue && tb.TimeBlockId > 0).ToList() ?? new List<TimeBlock>();
            if (!validTimeBlocks.Any())
                return new ActionResult(true);

            foreach (TimeBlock timeBlock in validTimeBlocks)
            {
                AddPayrollImportEmployeeTransactionAccountLink(date, employeeId, timeBlock.PayrollImportEmployeeTransactionId.Value, timeBlockId: timeBlock.TimeBlockId);
            }

            return Save();
        }

        private ActionResult SavePayrollImportEmployeeTransactionLinks(DateTime date, int employeeId, IEnumerable<TimePayrollTransaction> timePayrollTransactions)
        {
            List<TimePayrollTransaction> validTimePayrollTransactions = timePayrollTransactions?.Where(tb => tb.PayrollImportEmployeeTransactionId.HasValue && tb.TimePayrollTransactionId > 0).ToList() ?? new List<TimePayrollTransaction>();
            if (!validTimePayrollTransactions.Any())
                return new ActionResult(true);

            foreach (TimePayrollTransaction timePayrollTransaction in validTimePayrollTransactions)
            {
                AddPayrollImportEmployeeTransactionAccountLink(date, employeeId, timePayrollTransaction.PayrollImportEmployeeTransactionId.Value, timePayrollTransactionId: timePayrollTransaction.TimePayrollTransactionId);
            }

            return Save();
        }

        private void AddPayrollImportEmployeeTransactionAccountLink(DateTime date, int employeeId, int payrollImportEmployeeTransactionId, int? timePayrollTransactionId = null, int? timeBlockId = null)
        {
            entities.PayrollImportEmployeeTransactionLink.AddObject(new PayrollImportEmployeeTransactionLink()
            {
                Date = date,
                EmployeeId = employeeId,
                PayrollImportEmployeeTransactionId = payrollImportEmployeeTransactionId,
                TimeBlockId = timeBlockId,
                TimePayrollTransactionId = timePayrollTransactionId,
                State = (int)SoeEntityState.Active,
            });
        }

        #endregion
    }
}
