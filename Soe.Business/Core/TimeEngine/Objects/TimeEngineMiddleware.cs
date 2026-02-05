using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    /// <summary>
    ///  Middleware to TimeEngine
    /// </summary>
    public sealed partial class TimeEngineManager : TimeEngine
    {
        #region Accounting

        public ActionResult RecalculateAccountingFromPayroll(List<int> employeeIds, int timePeriodId)
        {
            CreateTaskInput(new RecalculateAccountingFromPayrollInputDTO(employeeIds, timePeriodId));
            return PerformTask(SoeTimeEngineTask.RecalculateAccountingFromPayroll).Result;
        }
        public ActionResult RecalculateAccounting(List<AttestEmployeeDaySmallDTO> items, SoeRecalculateAccountingMode mode)
        {
            CreateTaskInput(new RecalculateAccountingInputDTO(items, mode));
            return PerformTask(SoeTimeEngineTask.RecalculateAccounting).Result;
        }
        public ActionResult SaveAccountProvisionBase(List<AccountProvisionBaseDTO> provisions)
        {
            CreateTaskInput(new SaveAccountProvisionBaseInputDTO(provisions));
            return PerformTask(SoeTimeEngineTask.SaveAccountProvisionBase).Result;
        }
        public ActionResult LockAccountProvisionBase(int timePeriodId)
        {
            CreateTaskInput(new LockUnlockAccountProvisionBaseInputDTO(timePeriodId));
            return PerformTask(SoeTimeEngineTask.LockAccountProvisionBase).Result;
        }
        public ActionResult UnLockAccountProvisionBase(int timePeriodId)
        {
            CreateTaskInput(new LockUnlockAccountProvisionBaseInputDTO(timePeriodId));
            return PerformTask(SoeTimeEngineTask.UnLockAccountProvisionBase).Result;
        }
        public ActionResult UpdateAccountProvisionTransactions(List<AccountProvisionTransactionGridDTO> transactions)
        {
            CreateTaskInput(new UpdateAccountProvisionTransactionsInputDTO(transactions));
            return PerformTask(SoeTimeEngineTask.UpdateAccountProvisionTransactions).Result;
        }

        #endregion

        #region Attest

        public ActionResult SaveAttestForEmployee(List<SaveAttestEmployeeDayDTO> items, int employeeId, int attestStateId, bool isMySelf = false, bool isPayrollAttest = false, bool forceWholeDay = false)
        {
            CreateTaskInput(new SaveAttestForEmployeeInputDTO(items, employeeId, attestStateId, isMySelf, isPayrollAttest, forceWholeDay));
            return PerformTask(SoeTimeEngineTask.SaveAttestForEmployee).Result;
        }
        public ActionResult SaveAttestForEmployees(int currentEmployeeId, List<int> employeeIds, int attestStateId, int timePeriodId, bool isPayrollAttest)
        {
            CreateTaskInput(new SaveAttestForEmployeesInputDTO(currentEmployeeId, employeeIds, attestStateId, timePeriodId, isPayrollAttest));
            return PerformTask(SoeTimeEngineTask.SaveAttestForEmployees).Result;
        }
        public ActionResult SaveAttestForEmployees(int currentEmployeeId, List<int> employeeIds, int attestStateId, DateTime startDate, DateTime stopDate, bool isPayrollAttest)
        {
            CreateTaskInput(new SaveAttestForEmployeesInputDTO(currentEmployeeId, employeeIds, attestStateId, startDate, stopDate, isPayrollAttest));
            return PerformTask(SoeTimeEngineTask.SaveAttestForEmployees).Result;
        }
        public ActionResult SaveAttestForTransactions(List<SaveAttestTransactionDTO> items, int attestStateId, bool isMySelf)
        {
            CreateTaskInput(new SaveAttestForTransactionsInputDTO(items, attestStateId, isMySelf));
            return PerformTask(SoeTimeEngineTask.SaveAttestForTransactions).Result;
        }
        public ActionResult SaveAttestForAccountProvision(List<AccountProvisionTransactionGridDTO> transactions)
        {
            CreateTaskInput(new SaveAttestForAccountProvisionInputDTO(transactions));
            return PerformTask(SoeTimeEngineTask.SaveAttestForAccountProvision).Result;
        }
        public ActionResult RunAutoAttest(List<int> employeeIds, DateTime startDate, DateTime stopDate, List<int> scheduleJobHeadIds = null, bool autoAttestJob = false)
        {
            CreateTaskInput(new RunAutoAttestInputDTO(employeeIds, scheduleJobHeadIds, startDate, stopDate, autoAttestJob));
            return PerformTask(SoeTimeEngineTask.RunAutoAttest).Result;
        }
        public ActionResult SendAttestReminder(List<int> employeeIds, DateTime startDate, DateTime stopDate, bool doSendToExecutive, bool doSendToEmployee)
        {
            CreateTaskInput(new SendAttestReminderInputDTO(employeeIds, startDate, stopDate, doSendToExecutive, doSendToEmployee));
            return PerformTask(SoeTimeEngineTask.SendAttestReminder).Result;
        }

        #endregion

        #region Calendar

        public ActionResult ImportHolidays(List<HolidayDTO> holidays, bool updateSchedules)
        {
            CreateTaskInput(new ImportHolidaysInputDTO(holidays, updateSchedules));
            return PerformTask(SoeTimeEngineTask.ImportHolidays).Result;
        }
        public Dictionary<DateTime, DayType> CalculateDayTypesForEmployee(List<DateTime> dates, int employeeId, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null)
        {
            CreateTaskInput(new CalculateDayTypesForEmployeeInputDTO(dates, employeeId, doNotCheckHoliday, companyHolidays));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CalculateDayTypesForEmployee);
            return oDTO.Result.Success && oDTO is CalculateDayTypesForEmployeeOutputDTO output ? output.DayTypes : null;
        }
        public DayType CalculateDayTypeForEmployee(DateTime date, int employeeId, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null)
        {
            CreateTaskInput(new CalculateDayTypeForEmployeeInputDTO(date, employeeId, doNotCheckHoliday, companyHolidays));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CalculateDayTypeForEmployee);
            return oDTO.Result.Success && oDTO is CalculateDayTypeForEmployeeOutputDTO output ? output.DayType : null;
        }
        public void CalculateDayTypeForEmployees(List<GrossNetCostDTO> dtos, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null, List<DayType> companyDayTypes = null, Employee employee = null)
        {
            CreateTaskInput(new CalculateDayTypeForEmployeesInputDTO(dtos, doNotCheckHoliday, companyHolidays, companyDayTypes, employee));
            PerformTask(SoeTimeEngineTask.CalculateDayTypeForEmployees);
        }
        public ActionResult SaveUniqueday(List<Tuple<int, int, bool>> tuples)
        {
            CreateTaskInput(new SaveUniqueDayInputDTO(tuples));
            return PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;
        }
        public TimeEngineOutputDTO UpdateUniqueDayFromHalfDay(int timeHalfdayId, int dayTypeId)
        {
            CreateTaskInput(new UpdateUniqueDayFromHalfDayInputDTO(timeHalfdayId, dayTypeId));
            return PerformTask(SoeTimeEngineTask.UpdateUniqueDayFromHalfDay);
        }
        public TimeEngineOutputDTO SaveUniqueDayFromHalfDay(int timeHalfdayId, bool removeOnly)
        {
            CreateTaskInput(new SaveUniqueDayFromHalfDayInputDTO(timeHalfdayId, removeOnly));
            return PerformTask(SoeTimeEngineTask.SaveUniqueDayFromHalfDay);
        }
        public TimeEngineOutputDTO AddUniqueDayFromHoliday(int holidayId, int dayTypeId)
        {
            CreateTaskInput(new AddUniqueDayFromHolidayInputDTO(holidayId, dayTypeId));
            return PerformTask(SoeTimeEngineTask.AddUniqueDayFromHoliday);
        }
        public TimeEngineOutputDTO DeleteUniqueDayFromHoliday(int holidayId, int dayTypeId, DateTime? oldDate, bool createTimeBlocksAndTransactionsAsync = true)
        {
            CreateTaskInput(new DeleteUniqueDayFromHolidayInputDTO(holidayId, dayTypeId, oldDate, createTimeBlocksAndTransactionsAsync));
            return PerformTask(SoeTimeEngineTask.DeleteUniqueDayFromHoliday);
        }
        public TimeEngineOutputDTO UpdateUniqueDayFromHoliday(int holidayId, int dayTypeId, DateTime? oldDateToDelete)
        {
            CreateTaskInput(new UpdateUniqueDayFromHolidayInputDTO(holidayId, dayTypeId, oldDateToDelete));
            return PerformTask(SoeTimeEngineTask.UpdateUniqueDayFromHoliday);
        }
        public ActionResult CreateTransctionsForEarnedHoliday(int holidayId, List<int> employeeIds, int year)
        {
            CreateTaskInput(new CreateTransactionsForEarnedHolidayInputDTO(holidayId, employeeIds, year));
            return PerformTask(SoeTimeEngineTask.CreateTransactionsForEarnedHoliday).Result;
        }
        public ActionResult DeleteTransctionsForEarnedHoliday(int holidayId, List<int> employeeIds, int year)
        {
            CreateTaskInput(new DeleteTransactionsForEarnedHolidayInputDTO(holidayId, employeeIds, year));
            return PerformTask(SoeTimeEngineTask.DeleteTransactionsForEarnedHoliday).Result;
        }

        #endregion

        #region Time

        public ActionResult ApplyCalculationFunctionForEmployee(List<AttestEmployeeDaySmallDTO> days, SoeTimeAttestFunctionOption restoreOption, int? timeScheduleScenarioHeadId)
        {
            ActionResult result = new ActionResult(true);

            if (days.IsNullOrEmpty())
                return result;
            if (timeScheduleScenarioHeadId.HasValue && timeScheduleScenarioHeadId > 0 && restoreOption != SoeTimeAttestFunctionOption.ScenarioRemoveAbsence)
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(10117, "Otillåten ändring"));

            base.InitiateCalculationDates(days.Select(day => day.Date).ToList());

            switch (restoreOption)
            {
                case SoeTimeAttestFunctionOption.RestoreToSchedule:
                    #region RestoreToSchedule
                    result = RestoreDaysToSchedule(days);
                    if (result.Success && !result.Dates.IsNullOrEmpty())
                        SetCompletedWithErrors(TermCacheManager.Instance.GetText(91911, (int)TermGroup.General, "Vissa dagar återställda till aktivt schema. Kunde inte återställa"));
                    else if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8808, (int)TermGroup.General, "Återställt till aktivt schema"));
                    else
                        SetFailed(TermCacheManager.Instance.GetText(91915, (int)TermGroup.General, "Dagar kunde inte återställas"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations:
                    #region RestoreToScheduleDiscardDeviations
                    result = RestoreToScheduleDiscardDeviations(days);
                    if (result.Success && !result.Dates.IsNullOrEmpty())
                        SetCompletedWithErrors(TermCacheManager.Instance.GetText(91939, (int)TermGroup.General, "Vissa dagar hoppades över"));
                    else if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8808, (int)TermGroup.General, "Återställt till aktivt schema"));
                    else
                        SetFailed(TermCacheManager.Instance.GetText(91915, (int)TermGroup.General, "Dagar kunde inte återställas"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.RestoreScheduleToTemplate:
                    #region RestoreScheduleToTemplate
                    result = RestoreDaysToTemplateSchedule(days);
                    if (result.Success && !result.Dates.IsNullOrEmpty())
                        SetCompletedWithErrors(TermCacheManager.Instance.GetText(91912, (int)TermGroup.General, "Vissa dagar återställda till grundschema. Kunde inte återställa"));
                    else if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8809, (int)TermGroup.General, "Återställt till grundschema"));
                    else
                        SetFailed(TermCacheManager.Instance.GetText(91915, (int)TermGroup.General, "Dagar kunde inte återställas"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps:
                    #region ReGenerateDaysBasedOnTimeStamps
                    result = ReGenerateDaysForEmployeeBasedOnTimeStamps(days);
                    if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8810, (int)TermGroup.General, "Återställt enligt stämplingar"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions:
                    #region DeleteTimeBlocksAndTransactions
                    result = CleanDays(days);
                    if (result.Success && !result.Dates.IsNullOrEmpty())
                        SetCompletedWithErrors(TermCacheManager.Instance.GetText(91914, (int)TermGroup.General, "Vissa dagar har rensats. Kunde inte rensa"));
                    else if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8812, (int)TermGroup.General, "Valda dagar har rensats"));
                    else
                        SetFailed(TermCacheManager.Instance.GetText(91916, (int)TermGroup.General, "Dagar kunde inte rensas"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest:
                    #region ReGenerateTransactionsDiscardAttest
                    result = ReGenerateTransactionsDiscardAttest(days);
                    if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8811, (int)TermGroup.General, "Valda dagar har räknats om"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest:
                    #region ReGenerateVacationsTransactionsDiscardAttest
                    result = ReGenerateTransactionsDiscardAttest(days, doNotRecalculateAmounts: true, vacationOnly: true);
                    if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8811, (int)TermGroup.General, "Valda dagar har räknats om"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.ScenarioRemoveAbsence:
                    #region ScenarioRemoveAbsence
                    if (timeScheduleScenarioHeadId.HasValue)
                        result = RemoveAbsenceInScenario(days, timeScheduleScenarioHeadId.Value);
                    if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(8921, (int)TermGroup.General, "Frånvaron har tagits borts"));
                    #endregion
                    break;
                case SoeTimeAttestFunctionOption.RecalculateAccounting:
                    #region RecalculateAccounting
                    result = RecalculateAccounting(days, SoeRecalculateAccountingMode.FromShiftType);
                    if (result.Success && !result.Dates.IsNullOrEmpty())
                        SetCompletedWithErrors(TermCacheManager.Instance.GetText(91919, (int)TermGroup.General, "Kontering omräknad för vissa dagar. Följande dagar kunde inte räknas om"));
                    else if (result.Success)
                        SetCompleted(TermCacheManager.Instance.GetText(91918, (int)TermGroup.General, "Kontering omräknad"));
                    else
                        SetFailed(TermCacheManager.Instance.GetText(91920, (int)TermGroup.General, "Kontering kunde inte räknas om"));
                    #endregion
                    break;
            }

            void SetCompleted(string message)
            {
                result.SuccessNumber = (int)ActionResultSave.Unknown;
                result.InfoMessage = message;
            }
            void SetCompletedWithErrors(string message)
            {
                result.SuccessNumber = (int)ActionResultSave.CompletedWithWarnings;
                result.InfoMessage = $"{message} {GetCoherentDateRangeText()}";
            }
            void SetFailed(string message)
            {
                StringBuilder sb = new StringBuilder();
                if (!result.ErrorMessage.IsNullOrEmpty())
                {
                    sb.Append(result.ErrorMessage);
                    sb.Append(". ");
                }
                sb.Append($"{message} {GetCoherentDateRangeText()}");
                result.ErrorMessage = sb.ToString();
            }
            string GetCoherentDateRangeText()
            {
                return result.Dates?.GetCoherentDateRangeText() ?? string.Empty;
            }

            return result;
        }
        public ActionResult ApplyCalculationFunctionForEmployees(List<AttestEmployeesDaySmallDTO> days, SoeTimeAttestFunctionOption restoreOption, int? timeScheduleScenarioHeadId)
        {
            ActionResult result = new ActionResult(true);
            if (days.IsNullOrEmpty())
                return result;

            List<AttestEmployeeDaySmallDTO> convertedItems = new List<AttestEmployeeDaySmallDTO>();

            foreach (var daysByEmployee in days.GroupBy(i => i.EmployeeId))
            {
                foreach (var day in daysByEmployee.OrderBy(i => i.DateFrom))
                {
                    DateTime currentDate = day.DateFrom;
                    while (currentDate <= day.DateTo)
                    {
                        if (!convertedItems.Any(i => i.EmployeeId == day.EmployeeId && i.Date == currentDate))
                            convertedItems.Add(new AttestEmployeeDaySmallDTO(daysByEmployee.Key, currentDate, 0));
                        currentDate = currentDate.AddDays(1);
                    }
                }
            }

            return ApplyCalculationFunctionForEmployee(convertedItems, restoreOption, timeScheduleScenarioHeadId);
        }
        public ActionResult RestoreDaysToSchedule(List<AttestEmployeeDaySmallDTO> days)
        {
            CreateTaskInput(new RestoreDaysToScheduleInputDTO(days));
            return PerformTask(SoeTimeEngineTask.RestoreDaysToSchedule).Result;
        }
        public ActionResult RestoreToScheduleDiscardDeviations(List<AttestEmployeeDaySmallDTO> days)
        {
            CreateTaskInput(new RestoreDaysToScheduleDiscardDeviationsInputDTO(days));
            return PerformTask(SoeTimeEngineTask.RestoreToScheduleDiscardDeviations).Result;
        }
        public ActionResult RestoreDaysToTemplateSchedule(List<AttestEmployeeDaySmallDTO> days)
        {
            CreateTaskInput(new RestoreDaysToTemplateScheduleInputDTO(days));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.RestoreDaysToTemplateSchedule);
            if (oDTO.Result.Success && oDTO is RestoreDaysToTemplateScheduleOutputDTO output)
            {
                List<int> stampingTimeBlockDateIds = output.StampingTimeBlockDateIds;
                if (!stampingTimeBlockDateIds.IsNullOrEmpty())
                {
                    List<DateTime> dates = oDTO.Result.Dates;
                    oDTO.Result = ReGenerateDaysForEmployeeBasedOnTimeStamps(stampingTimeBlockDateIds);
                    oDTO.Result.Dates = dates;
                }
            }
            return oDTO.Result;
        }
        public ActionResult ReGenerateTransactionsDiscardAttest(List<AttestEmployeeDaySmallDTO> days, bool doNotRecalculateAmounts = false, bool vacationOnly = false, bool vacationResetLeaveOfAbsence = false, bool vacationReset3000 = false, DateTime? limitStartDate = null, DateTime? limitStopDate = null)
        {
            CreateTaskInput(new ReGenerateTransactionsDiscardAttestInputDTO(days, doNotRecalculateAmounts, vacationOnly, vacationResetLeaveOfAbsence, vacationReset3000, limitStartDate, limitStopDate));
            var taskResult = PerformTask(SoeTimeEngineTask.ReGenerateTransactionsDiscardAttest);
            var result = taskResult.Result;
            if (!result.Success)
                return taskResult.Result;

            if (taskResult is ReGenerateTransactionsDiscardAttestOutputDTO && !(taskResult as ReGenerateTransactionsDiscardAttestOutputDTO).StampingDates.IsNullOrEmpty())
            {
                List<DateTime> stampingDates = (taskResult as ReGenerateTransactionsDiscardAttestOutputDTO).StampingDates;
                List<AttestEmployeeDaySmallDTO> stampingDays = days.Where(d => stampingDates.Contains(d.Date)).ToList();
                result = ReGenerateDaysForEmployeeBasedOnTimeStamps(stampingDays);
            }

            return result;
        }
        public ActionResult CleanDays(List<AttestEmployeeDaySmallDTO> days)
        {
            CreateTaskInput(new CleanDaysInputDTO(days));
            return PerformTask(SoeTimeEngineTask.CleanDays).Result;
        }
        public ActionResult SaveTimeCodeTransactions(List<TimeCodeTransactionDTO> timeCodeTransactionDTOs)
        {
            CreateTaskInput(new SaveTimeCodeTransactionsInputDTO(timeCodeTransactionDTOs));
            return PerformTask(SoeTimeEngineTask.SaveTimeCodeTransactions).Result;
        }
        public ActionResult CreateTransactionsForPlannedPeriodCalculation(int employeeId, int timePeriodId)
        {
            CreateTaskInput(new CreateTransactionsForPlannedPeriodCalculationInputDTO(employeeId, timePeriodId));
            return PerformTask(SoeTimeEngineTask.CreateTransactionsForPlannedPeriodCalculation).Result;
        }

        #endregion

        #region Deviations

        public ActionResult GenerateDeviationsFromInterval(DateTime start, DateTime stop, DateTime? displayedDate, String comment, int timeScheduleTemplatePeriodId, int timeDeviationCauseStartId, int timeDeviationCauseStopId, int? employeeChildId, int employeeId, TermGroup_TimeDeviationCauseType devCauseType)
        {
            CreateTaskInput(new GenerateDeviationsFromTimeIntervalInputDTO(start, stop, displayedDate, comment, timeScheduleTemplatePeriodId, timeDeviationCauseStartId, timeDeviationCauseStopId, employeeChildId, employeeId, devCauseType));
            return PerformTask(SoeTimeEngineTask.GenerateDeviationsFromTimeInterval).Result;
        }
        public ActionResult SaveGeneratedDeviations(List<AttestEmployeeDayTimeBlockDTO> timeBlocks, List<AttestEmployeeDayTimeCodeTransactionDTO> timeCodeTransactions, List<AttestPayrollTransactionDTO> timePayrollTransactions, List<ApplyAbsenceDTO> applyAbsenceItems, int timeBlockDateId, int timeSchedulePeriodId, int employeeId, List<int> payrollImportEmployeeTransactionIds)
        {
            CreateTaskInput(new SaveGeneratedDeviationsInputDTO(timeBlocks, timeCodeTransactions, timePayrollTransactions, applyAbsenceItems, timeBlockDateId, timeSchedulePeriodId, employeeId, payrollImportEmployeeTransactionIds));
            return PerformTask(SoeTimeEngineTask.SaveGeneratedDeviations).Result;
        }
        public ActionResult SaveWholedayDeviations(List<TimeBlockDTO> timeBlockInputs, String comment, int timeDeviationCauseStartId, int timeDeviationCauseStopId, int? employeeChildId, int employeeId)
        {
            CreateTaskInput(new SaveWholedayDeviationsInputDTO(timeBlockInputs, comment, timeDeviationCauseStartId, timeDeviationCauseStopId, employeeChildId, employeeId));
            return PerformTask(SoeTimeEngineTask.SaveWholedayDeviations).Result;
        }
        public ValidateDeviationChangeResult ValidateDeviationChange(int employeeId, int timeBlockId, int timeScheduleTemplatePeriodId, string timeBlockGuidId, SoeTimeBlockClientChange clientChange, DateTime date, DateTime startTime, DateTime stopTime, List<AttestEmployeeDayTimeBlockDTO> timeBlocks, bool onlyUseInTimeTerminal, int? timeDeviationCauseId = null, int? employeeChildId = null, string comment = "", AccountingSettingsRowDTO accountSetting = null)
        {
            CreateTaskInput(new ValidateDeviationChangeInputDTO(employeeId, timeBlockId, timeScheduleTemplatePeriodId, timeDeviationCauseId, employeeChildId, timeBlockGuidId, clientChange, date, startTime, stopTime, timeBlocks, onlyUseInTimeTerminal, comment, accountSetting));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.ValidateDeviationChange);
            return oDTO is ValidateDeviationChangeOutputDTO output ? output.ValidationResult : null;
        }
        public ActionResult RecalculateUnhandledShiftChanges(List<TimeUnhandledShiftChangesEmployeeDTO> unhandledShiftChanges, bool doRecalculateShifts, bool doRecalculateExtraShifts)
        {
            CreateTaskInput(new RecalculateUnhandledShiftChangesInputDTO(unhandledShiftChanges, doRecalculateShifts, doRecalculateExtraShifts));
            return PerformTask(SoeTimeEngineTask.RecalculateUnhandledShiftChanges).Result;
        }
        public (int dayOfAbsenceNumber, DateTime QualifyingDate) GetDayOfAbsenceNumber(int employeeId, DateTime date, TermGroup_SysPayrollType sysPayrollTypeLevel3, int maxDays, int interval)
        {
            CreateTaskInput(new GetDayOfAbsenceNumberInputDTO(employeeId, date, sysPayrollTypeLevel3, maxDays, interval));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetDayOfAbsenceNumber);
            return oDTO.Result.Success ? (oDTO.Result.IntegerValue, oDTO.Result.DateTimeValue) : (0, CalendarUtility.DATETIME_DEFAULT);
        }
        public List<CreateAbsenceDetailResultDTO> CreateAbsenceDetails(int batchInterval, DateTime startDate, DateTime stopDate, int? employeeId)
        {
            CreateTaskInput(new CreateAbsenceDetailsInputDTO(batchInterval, startDate, stopDate, employeeId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CreateAbsenceDetails);
            return oDTO is CreateAbsenceDetailsOutputDTO output ? output.AbsenceResults : null;
        }
        public ActionResult SaveAbsenceDetailsRatio(int employeeId, List<TimeAbsenceDetailDTO> timeAbsenceDetails)
        {
            CreateTaskInput(new SaveAbsenceDetailsRatioInputDTO(employeeId, timeAbsenceDetails));
            return PerformTask(SoeTimeEngineTask.SaveAbsenceDetailsRatio).Result;
        }
        public List<EmployeeDeviationAfterEmploymentDTO> GetDeviationsAfterEmployment(List<int> employeeIds = null)
        {
            CreateTaskInput(new GetDeviationsAfterEmploymentInputDTO(employeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetDeviationsAfterEmployment);
            return oDTO is GetDeviationsAfterEmploymentOutputDTO output ? output.Deviations : null;
        }
        public ActionResult DeleteDeviationsDaysAfterEmployment(List<EmployeeDeviationAfterEmploymentDTO> deviations)
        {
            CreateTaskInput(new DeleteDeviationsDaysAfterEmploymentInputDTO(deviations));
            return PerformTask(SoeTimeEngineTask.DeleteDeviationsDaysAfterEmployment).Result;
        }

        #endregion

        #region Expense

        public SaveExpenseValidationDTO SaveExpenseValidation(ExpenseRowDTO expenseRow)
        {
            CreateTaskInput(new TaskSaveExpenseInputDTO(expenseRow));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.SaveExpenseValidation);
            return oDTO is TaskSaveExpenseValidationOutputDTO output  ? output.ValidationOutput : new SaveExpenseValidationDTO
            {
                Success = false,
                CanOverride = false,
                Title = "Error",
                Message = "Error"
            };
        }
        public ActionResult SaveExpense(ExpenseRowDTO expenseRow, int? customerInvoiceId = null, bool returnEntity = false)
        {
            CreateTaskInput(new TaskSaveExpenseInputDTO(expenseRow, customerInvoiceId, returnEntity));
            return PerformTask(SoeTimeEngineTask.SaveExpense).Result;
        }
        public ActionResult DeleteExpense(int expenseRowId, bool noErrorIfExpenseRowNotFound = false)
        {
            CreateTaskInput(new DeleteExpenseInputDTO(expenseRowId, noErrorIfExpenseRowNotFound));
            return PerformTask(SoeTimeEngineTask.DeleteExpense).Result;
        }

        #endregion

        #region Mobile

        public ActionResult MobileModifyBreak(DateTime date, int scheduleBreakBlockId, int employeeId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, int totalMinutes)
        {
            CreateTaskInput(new MobileModifyBreakInputDTO(date, scheduleBreakBlockId, employeeId, timeScheduleTemplatePeriodId, timeCodeBreakId, totalMinutes));
            return PerformTask(SoeTimeEngineTask.MobileModifyBreak).Result;
        }
        public ActionResult AddModifyTimeBlocks(List<TimeBlock> timeBlocks, DateTime date, int timeScheduleTemplatePeriodId, int employeeId, int? timeDeviationCauseId)
        {
            CreateTaskInput(new AddModifyTimeBlocksInputDTO(timeBlocks, date, timeScheduleTemplatePeriodId, employeeId, timeDeviationCauseId));
            return PerformTask(SoeTimeEngineTask.AddModifyTimeBlocks).Result;
        }

        #endregion

        #region PayrollEnd

        public VacationYearEndResultDTO SaveVacationYearEnd(TermGroup_VacationYearEndHeadContentType contentType, List<int> contentTypeIds, DateTime date)
        {
            CreateTaskInput(new SaveVacationYearEndInputDTO(contentType, contentTypeIds, date));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.SaveVacationYearEnd);
            return oDTO is SaveVacationYearEndOutputDTO output ? output.Details : null;

        }
        public ActionResult DeleteVacationYearEnd(int vacationYearEndHeadId)
        {
            CreateTaskInput(new DeleteVacationYearEndInputDTO(vacationYearEndHeadId));
            return PerformTask(SoeTimeEngineTask.DeleteVacationYearEnd).Result;
        }
        public ActionResult CreateFinalSalary(int employeeId, int? timePeriodId, bool createReport)
        {
            CreateTaskInput(new CreateFinalSalaryInputDTO(employeeId, timePeriodId, createReport));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CreateFinalSalary);
            if (oDTO.Result.Success && !oDTO.Result.Keys.IsNullOrEmpty() && oDTO.Result.Keys.Contains(employeeId) && timePeriodId.HasValue)
                this.RecalculatePayrollPeriod(employeeId, timePeriodId.Value, false, true); //Do not return result from RecalculatePayrollPeriod as it changes the result.Strings property
            return oDTO.Result;
        }
        public ActionResult CreateFinalSalaries(List<int> employeeIds, int? timePeriodId, bool createReport)
        {
            CreateTaskInput(new CreateFinalSalaryInputDTO(employeeIds, timePeriodId, createReport));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CreateFinalSalary);
            if (oDTO.Result.Success && !oDTO.Result.Keys.IsNullOrEmpty() && timePeriodId.HasValue)
                this.RecalculatePayrollPeriod(oDTO.Result.Keys, timePeriodId.Value.ObjToList(), false, true); //Do not return result from RecalculatePayrollPeriod as it changes the result.Strings property
            return oDTO.Result;
        }
        public ActionResult DeleteFinalSalary(int employeeId, int timePeriodId)
        {
            CreateTaskInput(new DeleteFinalSalaryInputDTO(employeeId, timePeriodId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.DeleteFinalSalary);
            if (oDTO.Result.Success && !oDTO.Result.Keys.IsNullOrEmpty() && oDTO.Result.Keys.Contains(employeeId))
                this.RecalculatePayrollPeriod(employeeId, timePeriodId, false, true); //Do not return result from RecalculatePayrollPeriod as it changes the result.Strings property
            return oDTO.Result;
        }
        public ActionResult DeleteFinalSalaries(List<int> employeeIds, int timePeriodId)
        {
            CreateTaskInput(new DeleteFinalSalaryInputDTO(employeeIds, timePeriodId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.DeleteFinalSalary);
            if (oDTO.Result.Success && !oDTO.Result.Keys.IsNullOrEmpty())
                this.RecalculatePayrollPeriod(oDTO.Result.Keys, timePeriodId.ObjToList(), false, true); //Do not return result from RecalculatePayrollPeriod as it changes the result.Strings property
            return oDTO.Result;
        }
        public ActionResult ClearPayrollCalculation(int employeeId, int timePeriodId)
        {
            CreateTaskInput(new ClearPayrollCalculationInputDTO(employeeId, timePeriodId));
            return PerformTask(SoeTimeEngineTask.ClearPayrollCalculation).Result;

        }
        public ActionResult ValidateVacationYearEnd(DateTime date, List<int> vacationGroupIds, List<int> employeeIds)
        {
            CreateTaskInput(new ValidateVacationYearEndInputDTO(date, vacationGroupIds, employeeIds));
            return PerformTask(SoeTimeEngineTask.ValidateVacationYearEnd).Result;
        }

        #endregion

        #region PayrollImport

        public ActionResult PayrollImport(int payrollImportHeadId, List<int> payrollImportEmployeeIds)
        {
            CreateTaskInput(new PayrollImportInputDTO(payrollImportHeadId, payrollImportEmployeeIds));
            return PerformTask(SoeTimeEngineTask.PayrollImport).Result;
        }
        public ActionResult PayrollImportRollback(int payrollImportHeadId, List<int> payrollImportEmployeeIds, bool isRollbackFileContentMode, bool rollbackOutcomeForAllEmployees, bool rollbackFileContentForAllEmployees)
        {
            CreateTaskInput(new RollbackPayrollImportInputDTO(payrollImportHeadId, payrollImportEmployeeIds, isRollbackFileContentMode, rollbackOutcomeForAllEmployees, rollbackFileContentForAllEmployees));
            return PerformTask(SoeTimeEngineTask.RollbackPayrollImport).Result;
        }
        public ActionResult ValidatePayrollImport(int payrollImportHeadId, List<int> payrollImportEmployeeIds)
        {
            CreateTaskInput(new ValidatePayrollImportInputDTO(payrollImportHeadId, payrollImportEmployeeIds));
            return PerformTask(SoeTimeEngineTask.ValidatePayrollImport).Result;
        }

        #endregion

        #region PayrollFunctions

        public ActionResult LockPayrollPeriod(List<int> employeeIds, int timePeriodId, int roleId, bool ignoreResultingAttestStateId)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeIds, timePeriodId, roleId, ignoreResultingAttestStateId));
            return PerformTask(SoeTimeEngineTask.LockPayrollPeriod).Result;
        }
        public ActionResult LockPayrollPeriod(int employeeId, int timePeriodId, int roleId, bool ignoreResultingAttestStateId)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeId, timePeriodId, roleId, ignoreResultingAttestStateId));
            return PerformTask(SoeTimeEngineTask.LockPayrollPeriod).Result;
        }
        public ActionResult LockPayrollPeriod(List<int> employeeIds, List<int> timePeriodIds, int roleId, bool ignoreResultingAttestStateId)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeIds, timePeriodIds, roleId, ignoreResultingAttestStateId));
            return PerformTask(SoeTimeEngineTask.LockPayrollPeriod).Result;
        }
        public ActionResult UnLockPayrollPeriod(List<int> employeeIds, int timePeriodId)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeIds, timePeriodId, 0, false));
            return PerformTask(SoeTimeEngineTask.UnLockPayrollPeriod).Result;
        }
        public ActionResult UnLockPayrollPeriod(int employeeId, int timePeriodId)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeId, timePeriodId, 0, false));
            return PerformTask(SoeTimeEngineTask.UnLockPayrollPeriod).Result;
        }
        public ActionResult UnLockPayrollPeriod(List<int> employeeIds, List<int> timePeriodIds)
        {
            CreateTaskInput(new LockUnlockPayrollPeriodInputDTO(employeeIds, timePeriodIds, 0, false));
            return PerformTask(SoeTimeEngineTask.UnLockPayrollPeriod).Result;
        }
        public ActionResult RecalculatePayrollPeriod(int employeeId, int timePeriodId, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded, SoeProgressInfo info = null, SoeMonitor monitor = null)
        {
            CreateTaskInput(new RecalculatePayrollPeriodInputDTO(Guid.NewGuid(), employeeId, timePeriodId, includeScheduleTransactions, ignoreEmploymentHasEnded, info: info, monitor: monitor));
            return PerformTask(SoeTimeEngineTask.RecalculatePayrollPeriod).Result;
        }
        public ActionResult RecalculatePayrollPeriod(Guid key, List<int> employeeIds, int timePeriodId, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded, SoeProgressInfo info = null, SoeMonitor monitor = null)
        {
            CreateTaskInput(new RecalculatePayrollPeriodInputDTO(key, employeeIds, timePeriodId, includeScheduleTransactions, ignoreEmploymentHasEnded, info, monitor));
            return PerformTask(SoeTimeEngineTask.RecalculatePayrollPeriod).Result;
        }
        public ActionResult RecalculatePayrollPeriod(List<int> employeeIds, List<int> timePeriodIds, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded)
        {
            CreateTaskInput(new RecalculatePayrollPeriodInputDTO(Guid.NewGuid(), employeeIds, timePeriodIds, includeScheduleTransactions, ignoreEmploymentHasEnded));
            return PerformTask(SoeTimeEngineTask.RecalculatePayrollPeriod).Result;
        }
        public List<AttestPayrollTransactionDTO> GetUnhandledPayrollTransactions(int employeeId, DateTime? startDate, DateTime? stopDate, bool isBackwards)
        {
            if (!startDate.HasValue || !stopDate.HasValue)
                return new List<AttestPayrollTransactionDTO>();

            CreateTaskInput(new GetUnhandledPayrollTransactionsInputDTO(employeeId, startDate.Value, stopDate.Value, isBackwards));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetUnhandledPayrollTransactions);
            return oDTO.Result.Success && oDTO is GetUnhandledPayrollTransactionsOutputDTO output
                ? output.TimePayrollTransactionItems
                : new List<AttestPayrollTransactionDTO>();
        }
        public ActionResult RecalculateExportedEmploymentTaxJOB(List<int> employeeIds, int timePeriodId)
        {
            CreateTaskInput(new RecalculateExportedEmploymentTaxJOBInputDTO(employeeIds, timePeriodId));
            return PerformTask(SoeTimeEngineTask.RecalculateExportedEmploymentTaxJOB).Result;
        }
        public ActionResult RecalculateExportedEmploymentTaxJOB(List<int> employeeIds, List<int> timePeriodIds)
        {
            CreateTaskInput(new RecalculateExportedEmploymentTaxJOBInputDTO(employeeIds, timePeriodIds));
            return PerformTask(SoeTimeEngineTask.RecalculateExportedEmploymentTaxJOB).Result;
        }
        public ActionResult AssignPayrollTransactionsToTimePeriod(List<AttestPayrollTransactionDTO> transactionItems, List<AttestPayrollTransactionDTO> scheduleTransactionItems, TimePeriodDTO timePeriodItem, TermGroup_TimePeriodType periodType, int employeeId)
        {
            CreateTaskInput(new AssignPayrollTransactionsToTimePeriodInputDTO(transactionItems, scheduleTransactionItems, timePeriodItem, periodType, employeeId));
            return PerformTask(SoeTimeEngineTask.AssignPayrollTransactionsToTimePeriod).Result;
        }
        public ReverseTransactionsValidationDTO ReverseTransactionsValidation(int employeeId, List<DateTime> dates)
        {
            CreateTaskInput(new ReverseTransactionsValidationInputDTO(employeeId, dates));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.ReverseTransactionsValidation);
            return oDTO is ReverseTransactionsValidationOutputDTO ? (oDTO as ReverseTransactionsValidationOutputDTO).ValidationOutput : new ReverseTransactionsValidationDTO
            {
                Success = false,
                CanContinue = false,
                Title = "Error",
                Message = "Error"
            };
        }
        public ActionResult ReverseTransactionsAngular(int employeeId, List<DateTime> dates, int? timeDeviationCauseId, int? timePeriodId, int? employeeChildId)
        {
            CreateTaskInput(new ReverseTransactionsAngularInputDTO(employeeId, dates, timeDeviationCauseId, timePeriodId, employeeChildId));
            return PerformTask(SoeTimeEngineTask.ReverseTransactions).Result;
        }
        public ActionResult SaveFixedPayrollRows(List<FixedPayrollRowDTO> inputItems, int employeeId)
        {
            CreateTaskInput(new SaveFixedPayrollRowsInputDTO(inputItems.Where(x => !x.IsReadOnly).ToList(), employeeId));
            return PerformTask(SoeTimeEngineTask.SaveFixedPayrollRows).Result;
        }
        public ActionResult SaveAddedTransaction(AttestPayrollTransactionDTO inputItem, List<AccountingSettingDTO> accountingSettings, List<AccountingSettingsRowDTO> accountingSettingsAngular, int employeeId, int? timePeriodId, bool ignoreEmploymentHasEnded)
        {
            CreateTaskInput(new SaveAddedTransactionInputDTO(inputItem, accountingSettings, accountingSettingsAngular, employeeId, timePeriodId, ignoreEmploymentHasEnded));
            return PerformTask(SoeTimeEngineTask.SaveAddedTransaction).Result;
        }
        public ActionResult CreateAddedTransactionsFromTemplate(MassRegistrationTemplateHeadDTO templateDTO)
        {
            CreateTaskInput(new CreateAddedTransactionsFromTemplateInputDTO(templateDTO));
            return PerformTask(SoeTimeEngineTask.CreateAddedTransactionsFromTemplate).Result;
        }
        public ActionResult SavePayrollScheduleTransactions(Dictionary<int, List<DateTime>> employeeDates)
        {
            CreateTaskInput(new SavePayrollScheduleTransactionsInputDTO(employeeDates));
            return PerformTask(SoeTimeEngineTask.SavePayrollScheduleTransactions).Result;
        }
        public ActionResult RecalculatePayrollControll(List<int> employeeIds, int timePeriod)
        {
            CreateTaskInput(new RecalculatePayrollControllInputDTO(employeeIds, timePeriod));
            return PerformTask(SoeTimeEngineTask.RecalculatePayrollControllResult).Result;

        }

        #endregion

        #region PayrollRetro

        public ActionResult SaveRetroactivePayroll(RetroactivePayrollDTO retroactivePayroll)
        {
            CreateTaskInput(new SaveRetroactivePayrollInputDTO(retroactivePayroll));
            return PerformTask(SoeTimeEngineTask.SaveRetroactivePayroll).Result;
        }
        public ActionResult SaveRetroactivePayrollOutcome(int retroactivePayrollId, int employeeId, List<RetroactivePayrollOutcomeDTO> retroOutcomesInput)
        {
            CreateTaskInput(new SaveRetroactivePayrollOutcomeInputDTO(retroactivePayrollId, employeeId, retroOutcomesInput));
            return PerformTask(SoeTimeEngineTask.SaveRetroactivePayrollOutcome).Result;
        }
        public ActionResult DeleteRetroactivePayroll(int retroactivePayrollId)
        {
            CreateTaskInput(new DeleteRetroactivePayrollInputDTO(retroactivePayrollId));
            return PerformTask(SoeTimeEngineTask.DeleteRetroactivePayroll).Result;
        }
        public ActionResult CalculateRetroactivePayroll(RetroactivePayrollDTO retroactivePayrollInput, bool includeAlreadyCalculated, List<int> filterEmployeeIds = null)
        {
            CreateTaskInput(new CalculateRetroactivePayrollInputDTO(retroactivePayrollInput, includeAlreadyCalculated, filterEmployeeIds));
            return PerformTask(SoeTimeEngineTask.CalculateRetroactivePayroll).Result;
        }
        public ActionResult DeleteRetroactivePayrollOutcomes(RetroactivePayrollDTO retroactivePayrollInput)
        {
            CreateTaskInput(new DeleteRetroactivePayrollOutcomesInputDTO(retroactivePayrollInput));
            return PerformTask(SoeTimeEngineTask.DeleteRetroactivePayrollOutcomes).Result;
        }
        public ActionResult CreateRetroactivePayrollTransactions(RetroactivePayrollDTO retroactivePayrollInput, List<int> filterEmployeeIds = null)
        {
            CreateTaskInput(new CreateRetroactivePayrollTransactionsInputDTO(retroactivePayrollInput, filterEmployeeIds));
            return PerformTask(SoeTimeEngineTask.CreateRetroactivePayrollTransactions).Result;
        }
        public ActionResult DeleteRetroactivePayrollTransactions(RetroactivePayrollDTO retroactivePayrollInput, List<int> filterEmployeeIds = null)
        {
            CreateTaskInput(new DeleteRetroactivePayrollTransactionsInputDTO(retroactivePayrollInput, filterEmployeeIds));
            return PerformTask(SoeTimeEngineTask.DeleteRetroactivePayrollTransactions).Result;
        }

        #endregion

        #region PayrollStartValue

        public ActionResult SavePayrollStartValues(List<PayrollStartValueRowDTO> startValueRows, int payrollStartValueHeadId)
        {
            CreateTaskInput(new SavePayrollStartValuesInputDTO(startValueRows, payrollStartValueHeadId));
            return PerformTask(SoeTimeEngineTask.SavePayrollStartValues).Result;
        }
        public ActionResult SaveTransactionsForPayrollStartValue(int? employeeId, int payrollStartValueHeadId)
        {
            CreateTaskInput(new SaveTransactionsForPayrollStartValuesInputDTO(employeeId, payrollStartValueHeadId));
            return PerformTask(SoeTimeEngineTask.SaveTransactionsForPayrollStartValues).Result;
        }
        public ActionResult DeleteTransactionsForPayrollStartValue(int? employeeId, int payrollStartValueHeadId)
        {
            CreateTaskInput(new DeleteTransactionsForPayrollStartValuesInputDTO(employeeId, payrollStartValueHeadId));
            return PerformTask(SoeTimeEngineTask.DeleteTransactionsForPayrollStartValues).Result;
        }
        public ActionResult DeletePayrollStartValueHead(int payrollStartValueHeadId)
        {
            CreateTaskInput(new DeletePayrollStartValueHeadInputDTO(payrollStartValueHeadId));
            return PerformTask(SoeTimeEngineTask.DeletePayrollStartValueHead).Result;
        }

        #endregion

        #region Stamping

        public Dictionary<int, int> SynchTimeStamps(List<TSTimeStampEntryItem> timeStampEntryItems, int timeTerminalId, int accountDimId)
        {
            CreateTaskInput(new SynchTimeStampsInputDTO(timeStampEntryItems, timeTerminalId, accountDimId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.SynchTimeStamps);
            return oDTO.Result.Success && oDTO is SynchTimeStampsOutputDTO output
                ? output.GetUpdatedTimeStampEntries()
                : new Dictionary<int, int>();
        }
        public List<GoTimeStampEmployeeStampStatus> SynchGTSTimeStamps(List<GoTimeStampTimeStamp> timeStampEntryItems, int timeTerminalId)
        {
            CreateTaskInput(new SynchGTSTimeStampsInputDTO(timeStampEntryItems, timeTerminalId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.SynchGTSTimeStamps);
            return oDTO is SynchGTSTimeStampsOutputDTO output ? output.EmployeeStampStatuses : null;
        }
        public ActionResult SaveTimeStampsFromJob(List<TimeStampEntry> timeStampEntries, bool? discardBreakEvaluation = null)
        {
            CreateTaskInput(new SaveDeviationsFromStampingJobInputDTO(timeStampEntries, discardBreakEvaluation));
            return PerformTask(SoeTimeEngineTask.SaveTimeStampsFromJob).Result;
        }
        public ActionResult ReGenerateDaysForEmployeeBasedOnTimeStamps(List<AttestEmployeeDaySmallDTO> items)
        {
            ActionResult result = new ActionResult(true);
            if (!items.IsNullOrEmpty())
            {
                foreach (var itemsByEmployee in items.GroupBy(i => i.EmployeeId))
                {
                    List<int> timeBlockDateIds;
                    if (items.Any(i => i.TimeBlockDateId == 0))
                        timeBlockDateIds = TimeBlockManager.GetTimeBlockDates(itemsByEmployee.Key, itemsByEmployee.Select(i => i.Date).Distinct().ToList()).Select(t => t.TimeBlockDateId).Distinct().ToList();
                    else
                        timeBlockDateIds = itemsByEmployee.Select(i => i.TimeBlockDateId).Distinct().ToList();

                    result = ReGenerateDaysForEmployeeBasedOnTimeStamps(timeBlockDateIds, discardAttestState: true);
                    if (!result.Success)
                        return result;
                }
            }
            return result;
        }
        public ActionResult ReGenerateDaysForEmployeeBasedOnTimeStamps(List<int> timeBlockDateIds, bool discardAttestState = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeStampEntry> timeStampEntries = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entitiesReadOnly, base.ActorCompanyId, timeBlockDateIds), TimeStampManager.GetTimeStampEntriesForRecalculation);
            if (timeStampEntries.IsNullOrEmpty())
                return new ActionResult(true);

            return ReGenerateDayBasedOnTimeStamps(timeStampEntries, discardAttestState: discardAttestState);
        }
        public ActionResult ReGenerateDayBasedOnTimeStamps(List<TimeStampEntry> timeStampEntries, bool? discardBreakEvaluation = null, bool discardAttestState = false)
        {
            CreateTaskInput(new SaveDeviationsFromStampingInputDTO(timeStampEntries, discardBreakEvaluation, discardAttestState));
            return PerformTask(SoeTimeEngineTask.ReGenerateDayBasedOnTimeStamps).Result;
        }

        #endregion

        #region Schedule

        //Template
        public TimeScheduleTemplateHead GetTimeScheduleTemplate(int templateHeadId, bool loadEmployeeSchedule, bool loadAccounts)
        {
            CreateTaskInput(new GetTimeScheduleTemplateInputDTO(templateHeadId, loadEmployeeSchedule, loadAccounts));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetTimeScheduleTemplate);
            return oDTO.Result.Success && oDTO is GetTimeScheduleTemplateOutputDTO output ? output.TemplateHead : null;
        }
        public ActionResult SaveTimeScheduleTemplate(TimeScheduleTemplateHead templateHead, List<TimeScheduleTemplateBlockDTO> templateBlockItems)
        {
            CreateTaskInput(new SaveTimeScheduleTemplateInputDTO(templateHead, templateBlockItems));
            return PerformTask(SoeTimeEngineTask.SaveTimeScheduleTemplate).Result;
        }
        public ActionResult SaveTimeScheduleTemplateStaffing(List<TimeSchedulePlanningDayDTO> shifts, int timeScheduleTemplateHeadId, int noOfDays, DateTime startDate, DateTime? stopDate, DateTime? firstMondayOfCycle, DateTime currentDate, bool simpleSchedule, bool startOnFirstDayOfWeek, bool locked, int employeeId, int? employeePostId = null, int? copyFromTimeScheduleTemplateHeadId = null, bool useAccountingFromSourceSchedule = true)
        {
            CreateTaskInput(new SaveTimeScheduleTemplateStaffingInputDTO(shifts, timeScheduleTemplateHeadId, noOfDays, startDate, stopDate, firstMondayOfCycle, currentDate, simpleSchedule, startOnFirstDayOfWeek, locked, employeeId, employeePostId, copyFromTimeScheduleTemplateHeadId, useAccountingFromSourceSchedule));
            return PerformTask(SoeTimeEngineTask.SaveTimeScheduleTemplateStaffing).Result;
        }
        public ActionResult UpdateTimeScheduleTemplateStaffing(List<TimeSchedulePlanningDayDTO> shifts, int employeeId, int timeScheduleTemplateHeadId, int dayNumberFrom, int dayNumberTo, DateTime currentDate, List<DateTime> activateDates = null, int? activateDayNumber = null, bool skipXEMailOnChanges = false)
        {
            CreateTaskInput(new UpdateTimeScheduleTemplateStaffingInputDTO(shifts, employeeId, timeScheduleTemplateHeadId, dayNumberFrom, dayNumberTo, currentDate, activateDates, activateDayNumber.ToNullable(), skipXEMailOnChanges));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.UpdateTimeScheduleTemplateStaffing);
            if (oDTO.Result.Success && oDTO is UpdateTimeScheduleTemplateStaffingOutputDTO output)
            {
                List<int> stampingTimeBlockDateIds = output.StampingTimeBlockDateIds;
                if (!stampingTimeBlockDateIds.IsNullOrEmpty())
                    oDTO.Result = ReGenerateDaysForEmployeeBasedOnTimeStamps(stampingTimeBlockDateIds);
            }
            return oDTO.Result;
        }
        public ActionResult DeleteTimeScheduleTemplate(int templateHeadId)
        {
            CreateTaskInput(new DeleteTimeScheduleTemplateInputDTO(templateHeadId));
            return PerformTask(SoeTimeEngineTask.DeleteTimeScheduleTemplate).Result;
        }
        public ActionResult RemoveEmployeeFromTimeScheduleTemplate(int timeScheduleTemplateHeadId)
        {
            CreateTaskInput(new RemoveEmployeeFromTimeScheduleTemplateInputDTO(timeScheduleTemplateHeadId));
            return PerformTask(SoeTimeEngineTask.RemoveEmployeeFromTimeScheduleTemplate).Result;
        }
        public ActionResult AssignTimeScheduleTemplateToEmployee(int timeScheduleTemplateHeadId, int employeeId, DateTime startDate)
        {
            CreateTaskInput(new AssignTimeScheduleTemplateToEmployeeInputDTO(timeScheduleTemplateHeadId, employeeId, startDate));
            return PerformTask(SoeTimeEngineTask.AssignTimeScheduleTemplateToEmployee).Result;
        }

        //Schedule
        public TimeScheduleTemplatePeriod GetSequentialSchedule(DateTime date, int timeSchedulePeriodId, int employeeId, bool includeStandby = false)
        {
            CreateTaskInput(new GetSequentialScheduleInputDTO(date, timeSchedulePeriodId, employeeId, includeStandby));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetSequentialSchedule);
            return oDTO.Result.Success && oDTO is GetSequentialScheduleOutputDTO output ? output.TemplatePeriod : null;
        }
        public ActionResult SaveShiftPrelToDef(List<int> employeeIds, DateTime startDate, DateTime stopDate, bool includeScheduleShifts, bool includeStandbyShifts)
        {
            CreateTaskInput(new SaveShiftPrelDefInputDTO(employeeIds.CreateEmployeeDates(startDate, stopDate), includeScheduleShifts, includeStandbyShifts));
            return PerformTask(SoeTimeEngineTask.SaveShiftPrelToDef).Result;
        }
        public ActionResult SaveShiftDefToPrel(List<int> employeeIds, DateTime startDate, DateTime stopDate, bool includeScheduleShifts, bool includeStandbyShifts)
        {
            CreateTaskInput(new SaveShiftPrelDefInputDTO(employeeIds.CreateEmployeeDates(startDate, stopDate), includeScheduleShifts, includeStandbyShifts));
            return PerformTask(SoeTimeEngineTask.SaveShiftDefToPrel).Result;
        }
        public ActionResult CopySchedule(int sourceEmployeeId, int targetEmployeeId, DateTime? sourceDateStop, DateTime targetDateStart, DateTime? targetDateStop, bool useAccountingFromSourceSchedule, bool createTimeBlocksAndTransactionsAsync)
        {
            CreateTaskInput(new CopyScheduleInputDTO(sourceEmployeeId, targetEmployeeId, sourceDateStop, targetDateStart, targetDateStop, useAccountingFromSourceSchedule, createTimeBlocksAndTransactionsAsync));
            return PerformTask(SoeTimeEngineTask.CopySchedule).Result;
        }
        public ActionResult GenerateAndSaveAbsenceFromStaffing(EmployeeRequestDTO employeeRequest, List<TimeSchedulePlanningDayDTO> shifts, bool scheduledAbsence, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new GenerateAndSaveAbsenceFromStaffingInputDTO(employeeRequest, shifts, scheduledAbsence, skipXEMailOnShiftChanges, timeScheduleScenarioHeadId));
            return PerformTask(SoeTimeEngineTask.GenerateAndSaveAbsenceFromStaffing).Result;
        }

        //Breaks
        public List<TimeScheduleTemplateBlock> GetBreaksForScheduleBlock(TimeScheduleTemplateBlock scheduleBlock)
        {
            CreateTaskInput(new GetBreaksForScheduleBlockInputDTO(scheduleBlock));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetBreaksForScheduleBlock);
            return oDTO.Result.Success && oDTO is GetBreaksForScheduleBlockOutputDTO output ? output.ScheduleBlockBreaks : new List<TimeScheduleTemplateBlock>();
        }
        public ActionResult HasEmployeeValidTimeCodeBreak(DateTime date, int timeCodeId, int employeeId)
        {
            CreateTaskInput(new HasEmployeeValidTimeCodeBreakInputDTO(date, timeCodeId, employeeId));
            return PerformTask(SoeTimeEngineTask.HasEmployeeValidTimeCodeBreak).Result;
        }
        public ValidateBreakChangeResult ValidateBreakChange(int employeeId, int timeScheduleTemplateBlockId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, DateTime startTime, int breakLength, bool isTemplate, int? timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new ValidateBreakChangeInputDTO(employeeId, timeScheduleTemplateBlockId, timeScheduleTemplatePeriodId, timeCodeBreakId, startTime, breakLength, isTemplate, timeScheduleScenarioHeadId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.ValidateBreakChange);
            return oDTO is ValidateBreakChangeOutputDTO output ? output.ValidationResult : null;
        }

        //Scenario
        public ActionResult SaveTimeScheduleScenarioHead(TimeScheduleScenarioHeadDTO scenarioHeadInput, int? timeScheduleScenarioHeadId, bool includeAbsence, int dateFunction)
        {
            CreateTaskInput(new SaveTimeScheduleScenarioHeadInputDTO(scenarioHeadInput, timeScheduleScenarioHeadId, includeAbsence, dateFunction));
            return PerformTask(SoeTimeEngineTask.SaveTimeScheduleScenarioHead).Result;
        }
        public ActionResult RemoveAbsenceInScenario(List<AttestEmployeeDaySmallDTO> items, int timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new RemoveAbsenceInScenarioInputDTO(items, timeScheduleScenarioHeadId));
            return PerformTask(SoeTimeEngineTask.RemoveAbsenceInScenario).Result;
        }
        public ActionResult ActivateScenario(ActivateScenarioDTO input)
        {
            CreateTaskInput(new ActivateScenarioInputDTO(input));
            return PerformTask(SoeTimeEngineTask.ActivateScenario).Result;
        }
        public ActionResult CreateTemplateFromScenario(CreateTemplateFromScenarioDTO input)
        {
            CreateTaskInput(new CreateTemplateFromScenarioInputDTO(input));
            return PerformTask(SoeTimeEngineTask.CreateTemplateFromScenario).Result;
        }

        //Placement
        public ActionResult SaveEmployeeSchedulePlacement(ActivateScheduleControlDTO control, List<ActivateScheduleGridDTO> placements, TermGroup_TemplateScheduleActivateFunctions function, DateTime? startDate, DateTime stopDate, int timeScheduleTemplateHeadId = 0, int timeScheduleTemplatePeriodId = 0, bool preliminary = false, bool useBulk = true)
        {
            var items = SaveEmployeeSchedulePlacementItem.Create(placements, function, timeScheduleTemplateHeadId, timeScheduleTemplatePeriodId, startDate, stopDate, preliminary).ToList();
            return SaveEmployeeSchedulePlacement(control, items, useBulk);
        }
        public ActionResult SaveEmployeeSchedulePlacement(ActivateScheduleControlDTO control, List<SaveEmployeeSchedulePlacementItem> items, bool useBulk = true)
        {
            CreateTaskInput(new SaveEmployeeSchedulePlacementInputDTO(control, items, useBulk));
            return PerformTask(SoeTimeEngineTask.SaveEmployeeSchedulePlacement).Result;
        }
        public ActionResult SaveEmployeeSchedulePlacementFromJob(int recalculateTimeHeadId)
        {
            CreateTaskInput(new SaveEmployeeSchedulePlacementFromJobInputDTO(recalculateTimeHeadId));
            return PerformTask(SoeTimeEngineTask.SaveEmployeeSchedulePlacementFromJob).Result;
        }
        public ActionResult SaveEmployeeSchedulePlacementStaffing(ActivateScheduleControlDTO control, EmployeeSchedulePlacementGridViewDTO placement, int employeeId, DateTime? startDate, DateTime stopDate, bool preliminary, bool createTimeBlocksAndTransactionsAsync = true)
        {
            var item = SaveEmployeeSchedulePlacementItem.Create(placement, startDate, stopDate, preliminary, true, !startDate.HasValue, employeeId, createTimeBlocksAndTransactionsAsync: createTimeBlocksAndTransactionsAsync);
            return SaveEmployeeSchedulePlacementStaffing(control, item);
        }
        public ActionResult SaveEmployeeSchedulePlacementStaffing(ActivateScheduleControlDTO control, SaveEmployeeSchedulePlacementItem item)
        {
            CreateTaskInput(new SaveEmployeeSchedulePlacementStaffingInputDTO(control, item));
            return PerformTask(SoeTimeEngineTask.SaveEmployeeSchedulePlacementStaffing).Result;
        }
        public ActionResult SaveTimeScheduleTemplateAndPlacement(bool saveTemplate, bool savePlacement, ActivateScheduleControlDTO control, List<TimeSchedulePlanningDayDTO> shifts, int timeScheduleTemplateHeadId, int templateNoOfDays, DateTime templateStartDate, DateTime? templateStopDate, DateTime? firstMondayOfCycle, DateTime? placementDateFrom, DateTime? placementDateTo, DateTime currentDate, int employeeId = 0, int? employeePostId = null, int? copyFromTimeScheduleTemplateHeadId = null, bool simpleSchedule = false, bool startOnFirstDayOfWeek = false, bool preliminary = false, bool locked = false, bool createTimeBlocksAndTransactionsAsync = true, bool useAccountingFromSourceSchedule = true)
        {
            ActionResult result = new ActionResult(true);
            if (shifts == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeSchedulePlanningDayDTO");

            bool templateSaved = false;
            if (saveTemplate)
            {
                result = SaveTimeScheduleTemplateStaffing(shifts, timeScheduleTemplateHeadId, templateNoOfDays, templateStartDate, templateStopDate, firstMondayOfCycle, currentDate, simpleSchedule, startOnFirstDayOfWeek, locked, employeeId, employeePostId, copyFromTimeScheduleTemplateHeadId, useAccountingFromSourceSchedule);
                if (!result.Success)
                    return result;
                templateSaved = true;
            }
            if (savePlacement)
            {
                EmployeeSchedulePlacementGridViewDTO placement = TimeScheduleManager.GetLastPlacementForEmployee(employeeId, actorCompanyId).ToDTO();
                if (placement == null)
                    return new ActionResult((int)ActionResultSave.SaveTemplateSchedule_PlacementNotValid);
                if (!placementDateTo.HasValue)
                    return new ActionResult((int)ActionResultSave.SaveTemplateSchedule_PlacementNotValid); //Must have date to               
                if (placement.EmployeeScheduleId == 0 && !placementDateFrom.HasValue)
                    return new ActionResult((int)ActionResultSave.SaveTemplateSchedule_PlacementNotValid); //Must have date from if no placement exists (i.e. EmployeeScheduleId == 0)

                result = SaveEmployeeSchedulePlacementStaffing(control, placement, employeeId, placementDateFrom, placementDateTo.Value, preliminary, createTimeBlocksAndTransactionsAsync);
                if (!result.Success)
                    return result;
            }
            if (templateSaved)
                result.SuccessNumber = (int)ActionResultSave.SaveTemplateSchedule_TemplateSaved;

            return result;
        }
        public ActionResult DeleteEmployeeSchedulePlacement(ActivateScheduleGridDTO placement, ActivateScheduleControlDTO control)
        {
            CreateTaskInput(new DeleteEmployeeSchedulePlacementInputDTO(placement, control));
            return PerformTask(SoeTimeEngineTask.DeleteEmployeeSchedulePlacement).Result;
        }
        public ActivateScheduleControlDTO ControlEmployeeSchedulePlacement(int employeeId, DateTime? employeeScheduleStartDate, DateTime? employeeScheduleStopDate, DateTime? startDate, DateTime? stopDate, bool isDelete)
        {
            var item = ActivateScheduleGridDTO.Create(employeeId, employeeScheduleStartDate, employeeScheduleStopDate);
            return ControlEmployeeSchedulePlacements(item.ObjToList(), startDate, stopDate, isDelete);
        }
        public ActivateScheduleControlDTO ControlEmployeeSchedulePlacements(List<ActivateScheduleGridDTO> items, DateTime? startDate, DateTime? stopDate, bool isDelete)
        {
            CreateTaskInput(new ControlEmployeeSchedulePlacementInputDTO(items, startDate, stopDate, isDelete));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.ControlEmployeeSchedulePlacement);
            if (oDTO.Result.Success && oDTO is ControlEmployeeSchedulePlacementOutputDTO)
                return (oDTO as ControlEmployeeSchedulePlacementOutputDTO).Control;
            else
                return new ActivateScheduleControlDTO();
        }

        //EmployeeRequest
        public IEnumerable<EmployeeRequestDTO> GetEmployeeRequestsDTOs(int? employeeId, List<TermGroup_EmployeeRequestType> requestTypes, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return this.GetEmployeeRequests(employeeId ?? 0, null, requestTypes, dateFrom, dateTo).ToDTOs();
        }
        public List<EmployeeRequest> GetEmployeeRequests(int employeeId, int? employeeRequestId, List<TermGroup_EmployeeRequestType> requestTypes, DateTime? dateFrom = null, DateTime? dateTo = null, bool ignoreState = false)
        {
            CreateTaskInput(new GetEmployeeRequestsInputDTO(employeeId, employeeRequestId, requestTypes, dateFrom, dateTo, ignoreState));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetEmployeeRequests);
            return oDTO.Result.Success && oDTO is GetEmployeeRequestsOutputDTO output
                ? output.EmployeeRequests
                : new List<EmployeeRequest>();
        }
        public EmployeeRequest LoadEmployeeRequest(int employeeRequestId)
        {
            CreateTaskInput(new LoadEmployeeRequestInputDTO(employeeRequestId, 0, null, null));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.LoadEmployeeRequest);
            return oDTO.Result.Success && oDTO is LoadEmployeeRequestOutputDTO output
                ? output.EmployeeRequest
                : new EmployeeRequest();
        }
        public EmployeeRequest LoadEmployeeRequest(int employeeId, DateTime start, DateTime stop, TermGroup_EmployeeRequestType requestType)
        {
            CreateTaskInput(new LoadEmployeeRequestInputDTO(null, employeeId, start, stop, requestType));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.LoadEmployeeRequest);
            return oDTO.Result.Success && oDTO is LoadEmployeeRequestOutputDTO output
                ? output.EmployeeRequest
                : new EmployeeRequest();
        }
        public ActionResult SaveEmployeeRequest(EmployeeRequest employeeRequest, int employeeId, TermGroup_EmployeeRequestType requestType, bool skipXEMailOnChanges, bool isForcedDefinitive)
        {
            CreateTaskInput(new SaveEmployeeRequestInputDTO(employeeRequest, employeeId, requestType, skipXEMailOnChanges, isForcedDefinitive));
            return PerformTask(SoeTimeEngineTask.SaveEmployeeRequest).Result;
        }
        public ActionResult SaveEmployeeRequest(int employeeId, List<EmployeeRequestDTO> deletedEmployeeRequests, List<EmployeeRequestDTO> editedOrNewRequests)
        {
            CreateTaskInput(new SaveOrDeleteEmployeeRequestInputDTO(employeeId, deletedEmployeeRequests, editedOrNewRequests));
            return PerformTask(SoeTimeEngineTask.SaveOrDeleteEmployeeRequest).Result;
        }
        public ActionResult DeleteEmployeeRequest(int employeeRequestId)
        {
            CreateTaskInput(new DeleteEmployeeRequestInputDTO(employeeRequestId));
            return PerformTask(SoeTimeEngineTask.DeleteEmployeeRequest).Result;
        }
        public ActionResult PerformAbsenceRequestPlanningAction(int employeeRequestId, IEnumerable<TimeSchedulePlanningDayDTO> shifts, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new PerformAbsenceRequestPlanningActionInputDTO(employeeRequestId, shifts, skipXEMailOnShiftChanges, timeScheduleScenarioHeadId));
            return PerformTask(SoeTimeEngineTask.PerformAbsenceRequestPlanningAction).Result;
        }

        //Shift
        public List<AvailableEmployeesDTO> GetAvailableEmployees(List<int> timeScheduleTemplateBlockIds, List<int> employeeIds, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, int? filterOnMessageGroupId, bool useExistingScheduleBlocks, List<TimeScheduleTemplateBlockDTO> timeScheduleTemplateBlockDTOs, bool getHidden, bool getVacant)
        {
            CreateTaskInput(new GetAvailableEmployeesInputDTO(timeScheduleTemplateBlockIds, employeeIds, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, filterOnMessageGroupId, useExistingScheduleBlocks, timeScheduleTemplateBlockDTOs, getHidden, getVacant));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetAvailableEmployees);
            return oDTO is GetAvailableEmployeesOutputDTO output ? output.AvailableEmployees : null;
        }

        public GetAvailableTimeOutputDTO GetAvailableTime(int employeeId, DateTime startTime, DateTime stopTime)
        {
            CreateTaskInput(new GetAvailableTimeInputDTO(employeeId, startTime, stopTime));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.GetAvailableTime);
            return oDTO is GetAvailableTimeOutputDTO output ? output : null;
        }
        public ActionResult InitiateScheduleSwap(int initiatorEmployeeId, DateTime initiatorShiftDate, List<int> initiatorShiftIds, int swapWithEmployeeId, DateTime swapShiftDate, List<int> swapWithShiftIds, string comment)
        {
            CreateTaskInput(new InitiateScheduleSwapInputDTO(initiatorEmployeeId, initiatorShiftDate, initiatorShiftIds, swapWithEmployeeId, swapShiftDate, swapWithShiftIds, comment));
            return PerformTask(SoeTimeEngineTask.InitiateScheduleSwap).Result;
        }
        public ActionResult ApproveScheduleSwap(int userId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            CreateTaskInput(new ApproveScheduleSwapInputDTO(userId, timeScheduleSwapRequestId, approved, comment));
            return PerformTask(SoeTimeEngineTask.ApproveScheduleSwap).Result;
        }
        public ActionResult SaveTimeScheduleShift(string source, List<TimeSchedulePlanningDayDTO> shifts, bool updateBreaks, bool skipXEMailOnChanges, bool adjustTasks, int minutesMoved, int? timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new SaveTimeScheduleShiftInputDTO(source, shifts, updateBreaks, skipXEMailOnChanges, adjustTasks, minutesMoved, timeScheduleScenarioHeadId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.SaveTimeScheduleShift);
            if (oDTO.Result.Success && oDTO is SaveTimeScheduleShiftOutputDTO output && !timeScheduleScenarioHeadId.HasValue)
            {
                List<int> stampingTimeBlockDateIds = output.StampingTimeBlockDateIds;
                if (!stampingTimeBlockDateIds.IsNullOrEmpty())
                    oDTO.Result = ReGenerateDaysForEmployeeBasedOnTimeStamps(stampingTimeBlockDateIds);
            }
            return oDTO.Result;
        }
        public ActionResult DeleteTimeScheduleShifts(List<int> timeScheduleTemplateBlockIds, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, List<int> includedOnDutyShiftIds)
        {
            CreateTaskInput(new DeleteTimeScheduleShiftInputDTO(timeScheduleTemplateBlockIds, skipXEMailOnChanges, timeScheduleScenarioHeadId, includedOnDutyShiftIds));
            return PerformTask(SoeTimeEngineTask.DeleteTimeScheduleShift).Result;
        }
        public ActionResult HandleTimeScheduleShift(HandleShiftAction action, int timeScheduleTemplateBlockId, int timeDeviationCauseId, int employeeId, int swapTimeScheduleTemplateBlockId, int roleId, bool preventAutoPermissions)
        {
            CreateTaskInput(new HandleTimeScheduleShiftInputDTO(action, timeScheduleTemplateBlockId, timeDeviationCauseId, employeeId, swapTimeScheduleTemplateBlockId, roleId, preventAutoPermissions));
            return PerformTask(SoeTimeEngineTask.HandleTimeScheduleShift).Result;
        }
        public ActionResult SplitTimeScheduleShift(TimeSchedulePlanningDayDTO shift, DateTime splitTime, int employeeId1, int employeeId2, bool keepShiftsTogether, bool isPersonalScheduleTemplate, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId)
        {
            CreateTaskInput(new SplitTimeScheduleShiftInputDTO(shift, splitTime, employeeId1, employeeId2, keepShiftsTogether, isPersonalScheduleTemplate, skipXEMailOnChanges, timeScheduleScenarioHeadId));
            return PerformTask(SoeTimeEngineTask.SplitTimeScheduleShift).Result;
        }
        public ActionResult SplitTemplateTimeScheduleShift(TimeSchedulePlanningDayDTO sourceShift, int sourceTemplateHeadId, DateTime splitTime, int? employeeId1, int? employeePostId1, int targetTemplateHeadId1, int? employeeId2, int? employeePostId2, int targetTemplateHeadId2, bool keepShiftsTogether)
        {
            CreateTaskInput(new SplitTemplateTimeScheduleShiftInputDTO(sourceShift, sourceTemplateHeadId, splitTime, employeeId1, employeePostId1, targetTemplateHeadId1, employeeId2, employeePostId2, targetTemplateHeadId2, keepShiftsTogether));
            return PerformTask(SoeTimeEngineTask.SplitTemplateTimeScheduleShift).Result;
        }
        public ActionResult DragTimeScheduleShift(DragShiftAction action, int sourceShiftId, int targetShiftId, DateTime start, DateTime end, int employeeId, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, Guid? targetLink, bool updateLinkOnTarget, int timeDeviationCauseId, int? employeeChildId, bool wholeDayAbsence, int? messageId, bool skipXEMailOnChanges, bool copyTaskWithShift, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, bool includeOnDutyShifts, List<int> includedOnDutyShiftIds)
        {
            if (action == DragShiftAction.CopyWithCycle || action == DragShiftAction.MoveWithCycle)
            {
                var shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(sourceShiftId);
                if (shift == null || !shift.ActualStartTime.HasValue)
                    return new ActionResult(false);

                int offsetDays = start.DayOfYear - shift.ActualStartTime.Value.DayOfYear;
                return DragTimeScheduleShiftMultipel(action, new List<int> { sourceShiftId }, offsetDays, employeeId, true, skipXEMailOnChanges, copyTaskWithShift, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, includeOnDutyShifts, includedOnDutyShiftIds);
            }
            else
            {
                CreateTaskInput(new DragTimeScheduleShiftInputDTO(action, sourceShiftId, targetShiftId, start, end, employeeId, keepSourceShiftsTogether, keepTargetShiftsTogether, targetLink, updateLinkOnTarget, timeDeviationCauseId, employeeChildId, wholeDayAbsence, messageId, skipXEMailOnChanges, copyTaskWithShift, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, includeOnDutyShifts, includedOnDutyShiftIds));
                return PerformTask(SoeTimeEngineTask.DragTimeScheduleShift).Result;
            }
        }
        public ActionResult DragTemplateTimeScheduleShift(DragShiftAction action, int sourceShiftId, int sourceTemplateHeadId, DateTime sourceDate, int targetShiftId, int targetTemplateHeadId, DateTime targetStart, DateTime targetEnd, int? employeeId, int? employeePostId, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, Guid? targetLink, bool updateLinkOnTarget, bool copyTaskWithShift)
        {
            CreateTaskInput(new DragTemplateTimeScheduleShiftInputDTO(action, sourceShiftId, sourceTemplateHeadId, sourceDate, targetShiftId, targetTemplateHeadId, targetStart, targetEnd, employeeId, employeePostId, keepSourceShiftsTogether, keepTargetShiftsTogether, targetLink, updateLinkOnTarget, copyTaskWithShift));
            return PerformTask(SoeTimeEngineTask.DragTemplateTimeScheduleShift).Result;
        }
        public ActionResult DragTimeScheduleShiftMultipel(DragShiftAction action, List<int> sourceShiftIds, int offsetDays, int destinationEmployeeId, bool linkWithExistingShiftsIfPossible, bool skipXEMailOnChanges, bool copyTaskWithShift, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, bool includeOnDutyShifts, List<int> includedOnDutyShiftIds)
        {
            CreateTaskInput(new DragTimeScheduleShiftMultipelInputDTO(action, sourceShiftIds, offsetDays, destinationEmployeeId, linkWithExistingShiftsIfPossible, skipXEMailOnChanges, copyTaskWithShift, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, includeOnDutyShifts, includedOnDutyShiftIds));
            return PerformTask(SoeTimeEngineTask.DragTimeScheduleShiftMultipel).Result;
        }
        public ActionResult DragTemplateTimeScheduleShiftMultipel(DragShiftAction action, List<int> sourceShiftIds, int sourceTemplateHeadId, DateTime firstSourceDate, int offsetDays, DateTime firstTargetDate, int? targetEmployeeId, int? targetEmployeePostId, int targetTimeScheduleTemplateHeadId, bool linkWithExistingShiftsIfPossible, bool copyTaskWithShift)
        {
            CreateTaskInput(new DragTemplateTimeScheduleShiftMultipelInputDTO(action, sourceShiftIds, sourceTemplateHeadId, firstSourceDate, offsetDays, firstTargetDate, targetEmployeeId, targetEmployeePostId, targetTimeScheduleTemplateHeadId, linkWithExistingShiftsIfPossible, copyTaskWithShift));
            return PerformTask(SoeTimeEngineTask.DragTemplateTimeScheduleShiftMultipel).Result;
        }
        public ActionResult RemoveEmployeeFromShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int timeScheduleTemplateBlockId, int employeeId)
        {
            CreateTaskInput(new RemoveEmployeeFromShiftQueueInputDTO(type, timeScheduleTemplateBlockId, employeeId));
            return PerformTask(SoeTimeEngineTask.RemoveEmployeeFromShiftQueue).Result;
        }
        public ActionResult AssignTaskToEmployee(int employeeId, DateTime date, List<StaffingNeedsTaskDTO> taskDTOs, bool skipXEMailOnShiftChanges)
        {
            CreateTaskInput(new AssignTaskToEmployeeInputDTO(employeeId, date, taskDTOs, skipXEMailOnShiftChanges));
            return PerformTask(SoeTimeEngineTask.AssignTaskToEmployee).Result;
        }
        public List<TimeSchedulePlanningDayDTO> AssignTemplateShiftTask(List<StaffingNeedsTaskDTO> tasks, DateTime date, int timeScheduleTemplateHeadId)
        {
            CreateTaskInput(new AssignTemplateShiftTaskInputDTO(tasks, date, timeScheduleTemplateHeadId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.AssignTemplateShiftTask);
            return oDTO is AssignTemplatShiftTaskOutputDTO output ? output.Shifts : null;
        }
        public ActionResult PerformRestoreAbsenceRequestedShifts(int employeeRequestId, bool setRequestAsPending)
        {
            CreateTaskInput(new PerformRestoreAbsenceRequestedShiftsInputDTO(employeeRequestId, setRequestAsPending));
            return PerformTask(SoeTimeEngineTask.PerformRestoreAbsenceRequestedShifts).Result;
        }

        //Employee Active Schedule import

        public ActionResult EmployeeActiveScheduleImport(List<EmployeeActiveScheduleIO> employeeActiveSchedules)
        {
            CreateTaskInput(new EmployeeActiveScheduleImportInputDTO(employeeActiveSchedules));
            return PerformTask(SoeTimeEngineTask.EmployeeActiveScheduleImport).Result;
        }

        #endregion

        #region Project

        public ActionResult SaveOrderShift(List<TimeSchedulePlanningDayDTO> shifts, bool skipXEMailOnChanges)
        {
            CreateTaskInput(new SaveOrderShiftInputDTO(shifts, skipXEMailOnChanges));
            return PerformTask(SoeTimeEngineTask.SaveOrderShift).Result;
        }
        public ActionResult SaveOrderAssignments(int employeeId, int orderId, int? shiftTypeId, DateTime startTime, DateTime? stopTime, TermGroup_AssignmentTimeAdjustmentType assignmentTimeAdjustmentType, bool skipXEMailOnChanges)
        {
            CreateTaskInput(new SaveOrderAssignmentInputDTO(employeeId, orderId, shiftTypeId, startTime, stopTime, assignmentTimeAdjustmentType, skipXEMailOnChanges));
            return PerformTask(SoeTimeEngineTask.SaveOrderAssignments).Result;
        }
        public ActionResult GenerateTimeBlocksBasedOnProjectTimeBlocks(List<ProjectTimeBlock> projectTimeBlocks, bool autoGenTimeAndBreakForProject)
        {
            CreateTaskInput(new SaveTimeBlocksFromProjectTimeBlockInputDTO(projectTimeBlocks, autoGenTimeAndBreakForProject));
            return PerformTask(SoeTimeEngineTask.SaveTimeBlocksFromProjectTimeBlock).Result;
        }

        #endregion

        #region WorkRules

        public ActionResult SaveEvaluateAllWorkRulesByPass(EvaluateWorkRulesActionResult result, int employeeId)
        {
            CreateTaskInput(new SaveEvaluateAllWorkRulesByPassInputDTO(result, employeeId));
            return PerformTask(SoeTimeEngineTask.SaveEvaluateAllWorkRulesByPass).Result;
        }
        public EvaluateAllWorkRulesActionResult EvaluateAllWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, List<int> employeeIds, DateTime startDate, DateTime stopDate, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            CreateTaskInput(new EvaluateAllWorkRulesInputDTO(plannedShifts, employeeIds, startDate, stopDate, isPersonalScheduleTemplate, timeScheduleScenarioHeadId, rules, planningPeriodStartDate, planningPeriodStopDate));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateAllWorkRules);
            return oDTO is EvaluateAllWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluatePlannedShiftsAgainstWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, List<DateTime> dates = null, List<SoeScheduleWorkRules> rules = null, List<SoeScheduleWorkRules> rulesToSkip = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            CreateTaskInput(new EvaluatePlannedShiftsAgainstWorkRulesInputDTO(plannedShifts, isPersonalScheduleTemplate, timeScheduleScenarioHeadId, dates, rules, rulesToSkip, planningPeriodStartDate, planningPeriodStopDate));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluatePlannedShiftsAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateEmployeePostPlannedShiftsAgainstWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, List<SoeScheduleWorkRules> rules = null)
        {
            CreateTaskInput(new EvaluatePlannedShiftsAgainstWorkRulesInputDTO(plannedShifts, false, null, rules: rules));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluatePlannedShiftsAgainstWorkRulesEmployeePost);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, int employeeId, int? timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules = null)
        {
            CreateTaskInput(new EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesInputDTO(plannedShifts, employeeId, timeScheduleScenarioHeadId, rules));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules);
            return oDTO is EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateScheduleSwapAgainstRules(int timeScheduleSwapRequestId)
        {
            CreateTaskInput(new EvaluateScheduleSwapAgainstWorkRulesInputDTO(timeScheduleSwapRequestId));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateScheduleSwapAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateDragShiftAgainstWorkRules(DragShiftAction action, int sourceShiftId, int targetShiftId, DateTime start, DateTime end, int destinationEmployeeId, bool isPersonalScheduleTemplate, bool wholeDayAbsence, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, List<SoeScheduleWorkRules> rules = null, bool keepSourceShiftsTogether = true, bool keepTargetShiftsTogether = true, bool fromQueue = false, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            if (action == DragShiftAction.CopyWithCycle || action == DragShiftAction.MoveWithCycle)
            {
                TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(sourceShiftId);
                if (shift == null || !shift.ActualStartTime.HasValue)
                    return new EvaluateWorkRulesActionResult(ActionResultSave.InsufficientInput);

                int offsetDays = start.DayOfYear - shift.ActualStartTime.Value.DayOfYear;
                return EvaluateDragShiftMultipelAgainstWorkRules(action, sourceShiftId.ObjToList(), offsetDays, destinationEmployeeId, isPersonalScheduleTemplate, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, rules, planningPeriodStartDate, planningPeriodStopDate);
            }
            else
            {
                CreateTaskInput(new EvaluateDragShiftAgainstWorkRulesInputDTO(action, sourceShiftId, targetShiftId, start, end, destinationEmployeeId, isPersonalScheduleTemplate, wholeDayAbsence, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, rules, keepSourceShiftsTogether, keepTargetShiftsTogether, fromQueue, planningPeriodStartDate, planningPeriodStopDate));
                TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateDragShiftAgainstWorkRules);
                return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
            }
        }
        public EvaluateWorkRulesActionResult EvaluateDragTemplateShiftAgainstWorkRules(DragShiftAction action, int sourceShiftId, int sourceTemplateHeaId, DateTime sourceDate, int targetShiftId, int targetTemplateHeadId, DateTime start, DateTime end, int? destinationEmployeeId, int? destinationEmployeePostId, List<SoeScheduleWorkRules> rules = null, bool keepSourceShiftsTogether = true, bool keepTargetShiftsTogether = true)
        {
            CreateTaskInput(new EvaluateDragTemplateShiftAgainstWorkRulesInputDTO(action, sourceShiftId, sourceTemplateHeaId, sourceDate, targetShiftId, targetTemplateHeadId, start, end, destinationEmployeeId, destinationEmployeePostId, rules, keepSourceShiftsTogether, keepTargetShiftsTogether));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateDragTemplateShiftAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateDragShiftMultipelAgainstWorkRules(DragShiftAction action, List<int> sourceShiftIds, int offsetDays, int destinationEmployeeId, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, List<SoeScheduleWorkRules> rules = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            CreateTaskInput(new EvaluateDragShiftMultipelAgainstWorkRulesInputDTO(action, sourceShiftIds, offsetDays, destinationEmployeeId, isPersonalScheduleTemplate, timeScheduleScenarioHeadId, standbyCycleWeek, standbyCycleDateFrom, standbyCycleDateTo, isStandByView, rules, planningPeriodStartDate, planningPeriodStopDate));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateDragShiftAgainstWorkRulesMultipel);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateDragTemplateShiftMultipelAgainstWorkRules(DragShiftAction action, List<int> sourceShiftIds, int sourceTemplateHeadId, DateTime firstSourceDate, int offsetDays, int? targetEmployeeId, int? targetEmployeePostId, int targetTemplateHeadId, DateTime firstTargetDate, List<SoeScheduleWorkRules> rules = null)
        {
            CreateTaskInput(new EvaluateDragTemplateShiftMultipelAgainstWorkRulesInputDTO(action, sourceShiftIds, sourceTemplateHeadId, firstSourceDate, offsetDays, targetEmployeeId, targetEmployeePostId, targetTemplateHeadId, firstTargetDate, rules));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateDragTemplateShiftAgainstWorkRulesMultipel);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateAssignTaskToEmployeeAgainstWorkRules(int destinationEmployeeId, DateTime destinationDate, List<StaffingNeedsTaskDTO> taskDTOs, List<SoeScheduleWorkRules> rules)
        {
            CreateTaskInput(new EvaluateAssignTaskToEmployeeAgainstWorkRulesInputDTO(destinationEmployeeId, destinationDate, taskDTOs, rules));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateAssignTaskToEmployeeAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateActivateScenarioAgainstWorkRules(int timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules, DateTime? preliminaryDateFrom)
        {
            CreateTaskInput(new EvaluateActivateScenarioAgainstWorkRulesInputDTO(timeScheduleScenarioHeadId, rules, preliminaryDateFrom));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateActivateScenarioAgainstWorkRules);
            return oDTO is EvaluateScenarioAgainstWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateScenarioToTemplateAgainstWorkRules(int timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules, List<TimeSchedulePlanningDayDTO> shifts, int numberOfMovedDays)
        {
            CreateTaskInput(new EvaluateScenarioToTemplateAgainstWorkRulesInputDTO(timeScheduleScenarioHeadId, rules, shifts, numberOfMovedDays));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateScenarioToTemplateAgainstWorkRules);
            return oDTO is EvaluateScenarioAgainstWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateDeviationsAgainstWorkRules EvaluateDeviationsAgainstWorkRules(int employeeId, DateTime date)
        {
            CreateTaskInput(new EvaluateDeviationsAgainstWorkRulesInputDTO(employeeId, date));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateDeviationsAgainstWorkRulesAndSendXEMail);
            return oDTO is EvaluateDeviationsAgainstWorkRulesOutputDTO output ? output.EvaluateDeviationsAgainstWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateSplitShiftAgainstWorkRules(TimeSchedulePlanningDayDTO shift, DateTime splitTime, int employeeId1, int employeeId2, bool keepShiftsTogether, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            CreateTaskInput(new SplitTimeScheduleShiftInputDTO(shift, splitTime, employeeId1, employeeId2, keepShiftsTogether, isPersonalScheduleTemplate, false, timeScheduleScenarioHeadId, planningPeriodStartDate, planningPeriodStopDate));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateSplitShiftAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public EvaluateWorkRulesActionResult EvaluateSplitTemplateShiftAgainstWorkRules(TimeSchedulePlanningDayDTO sourceShift, int sourceTemplateHeadId, DateTime splitTime, int? employeeId1, int? employeePostId1, int templateHeadId1, int? employeeId2, int? employeePostId2, int templateHeadId2, bool keepShiftsTogether)
        {
            CreateTaskInput(new SplitTemplateTimeScheduleShiftInputDTO(sourceShift, sourceTemplateHeadId, splitTime, employeeId1, employeePostId1, templateHeadId1, employeeId2, employeePostId2, templateHeadId2, keepShiftsTogether));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.EvaluateSplitTemplateShiftAgainstWorkRules);
            return oDTO is EvaluateScheduleWorkRulesOutputDTO output ? output.EvaluateWorkRulesResult : null;
        }
        public bool IsDayAttested(int employeeId, DateTime date)
        {
            CreateTaskInput(new IsDayAttestedInputDTO(employeeId, date));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.IsDayAttested);
            return oDTO is IsDayAttestedOutputDTO output && output.IsDayAttestedResult;
        }

        #endregion

        #region TimeWorkAccount

        public TimeWorkAccountYearEmployeeResultDTO CalculateTimeWorkAccountYearEmployee(int workTimeAccountId, int workTimeAccountYearId, List<int> timeWorkAccountYearEmployeeIds, List<int> employeeIds)
        {
            CreateTaskInput(new TimeWorkAccountYearEmployeeInputDTO(workTimeAccountId, workTimeAccountYearId, timeWorkAccountYearEmployeeIds, employeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CalculateTimeWorkAccountYearEmployee);
            return oDTO is CalculateTimeWorkAccountYearEmployeeOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountYearEmployeeResultDTO(oDTO.Result);
        }
        public List<TimeWorkAccountYearEmployeeCalculation> CalculateTimeWorkAccountYearEmployeeBasis(List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            CreateTaskInput(new TaskCalculateTimeWorkAccountYearEmployeeBasisInputDTO(employeeIds, dateFrom, dateTo));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CalculateTimeWorkAccountYearEmployeeBasis);
            return oDTO is CalculateTimeWorkAccountYearEmployeeBasisOutputDTO output
                ? output.Basis
                : new List<TimeWorkAccountYearEmployeeCalculation>();
        }
        public TimeWorkAccountChoiceResultDTO TimeWorkAccountChoiceSendXEMail(int workTimeAccountId, int workTimeAccountYearId, List<int> timeWorkAccountYearEmployeeIds)
        {
            CreateTaskInput(new TimeWorkAccountYearEmployeeInputDTO(workTimeAccountId, workTimeAccountYearId, timeWorkAccountYearEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkAccountChoiceSendXEMail);
            return oDTO is TimeWorkAccountChoiceSendXEMailOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountChoiceResultDTO(oDTO.Result);
        }
        public TimeWorkAccountGenerateOutcomeResultDTO TimeWorkAccountYearGenerateOutcome(int workTimeAccountId, int workTimeAccountYearId, bool overrideChoosen, DateTime paymentDate, List<int> timeWorkAccountYearEmployeeIds)
        {
            CreateTaskInput(new TimeWorkAccountGenerateOutcomeInputDTO(workTimeAccountId, workTimeAccountYearId, overrideChoosen, paymentDate, timeWorkAccountYearEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkAccountGenerateOutcome);
            return oDTO is TimeWorkAccountTransactionOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountGenerateOutcomeResultDTO(oDTO.Result);
        }
        public TimeWorkAccountGenerateOutcomeResultDTO TimeWorkAccountYearReverseTransaction(int workTimeAccountId, int workTimeAccountYearId, List<int> timeWorkAccountYearEmployeeIds)
        {
            CreateTaskInput(new TimeWorkAccountYearEmployeeInputDTO(workTimeAccountId, workTimeAccountYearId, timeWorkAccountYearEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkAccountReverseTransaction);
            return oDTO is TimeWorkAccountTransactionOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountGenerateOutcomeResultDTO(oDTO.Result);
        }
        public TimeWorkAccountGenerateOutcomeResultDTO TimeWorkAccountGenerateUnusedPaidBalance(int workTimeAccountId, int workTimeAccountYearId, DateTime paymentDate, List<int> timeWorkAccountYearEmployeeIds)
        {
            CreateTaskInput(new TimeWorkAccountGenerateOutcomeInputDTO(workTimeAccountId, workTimeAccountYearId, false, paymentDate, timeWorkAccountYearEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkAccountGenerateUnusedPaidBalance);
            return oDTO is TimeWorkAccountTransactionOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountGenerateOutcomeResultDTO(oDTO.Result);
        }
        public TimeWorkAccountGenerateOutcomeResultDTO TimeWorkAccountYearReversePaidBalance(int workTimeAccountId, int workTimeAccountYearId, List<int> timeWorkAccountYearEmployeeIds)
        {
            CreateTaskInput(new TimeWorkAccountYearEmployeeInputDTO(workTimeAccountId, workTimeAccountYearId, timeWorkAccountYearEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkAccountYearReversePaidBalance);
            return oDTO is TimeWorkAccountTransactionOutputDTO output
                ? output.FunctionResult
                : new TimeWorkAccountGenerateOutcomeResultDTO(oDTO.Result);
        }

        #endregion

        #region TimeWorkReduction

        public TimeWorkReductionReconciliationYearEmployeeResultDTO CalculateTimeWorkReductionReconciliationYearEmployee(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationEmployeeIds, List<int> employeeIds)
        {
            CreateTaskInput(new CalculateTimeWorkReductionReconciliationYearEmployeeInputDTO(timeWorkReductionReconciliationId, timeWorkReductionReconciliationYearId, timeWorkReductionReconciliationEmployeeIds, employeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.CalculateTimeWorkReductionReconciliationYearEmployee);
            return oDTO is CalculateTimeWorkReductionReconciliationYearEmployeeOutputDTO output 
                ?  output.FunctionResult
                : new TimeWorkReductionReconciliationYearEmployeeResultDTO(oDTO.Result);
        }

        public TimeWorkReductionReconciliationYearEmployeeResultDTO TimeWorkReductionReconciliationYearEmployeeGenerateOutcome(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, bool overrideChoosen, DateTime? paymentDate, List<int> timeWorkReductionReconciliationEmployeeIds)
        {
            CreateTaskInput(new TimeWorkReductionReconciliationYearEmployeeGenerateOutcomeInputDTO(timeWorkReductionReconciliationId, timeWorkReductionReconciliationYearId, paymentDate, overrideChoosen, timeWorkReductionReconciliationEmployeeIds));
            TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkReductionReconciliationYearEmployeeGenerateOutcome);
            if (oDTO is TimeWorkReductionReconciliationTransactionResultRowDTO)
                return (oDTO as TimeWorkReductionReconciliationTransactionResultRowDTO).FunctionResult;
            return new TimeWorkReductionReconciliationYearEmployeeResultDTO();
        }
         public TimeWorkReductionReconciliationYearEmployeeResultDTO TimeWorkReductionReconciliationYearEmployeeReverseTransactions(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, bool overrideChoosen, List<int> timeWorkReductionReconciliationEmployeeIds)
        {
            CreateTaskInput(new TimeWorkReductionReconciliationYearEmployeeReverseTransactionsInputDTO(timeWorkReductionReconciliationId, timeWorkReductionReconciliationYearId, overrideChoosen, timeWorkReductionReconciliationEmployeeIds));
             TimeEngineOutputDTO oDTO = PerformTask(SoeTimeEngineTask.TimeWorkReductionReconciliationYearEmployeeReverseTransactions);
             if (oDTO is TimeWorkReductionReconciliationTransactionResultRowDTO)
                 return (oDTO as TimeWorkReductionReconciliationTransactionResultRowDTO).FunctionResult;
             return new TimeWorkReductionReconciliationYearEmployeeResultDTO();
        }
        

        #endregion
    }
}
