using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private CreateTransactionsForPlannedPeriodCalculationOutputDTO TaskCreateTransactionsForPlannedPeriodCalculation()
        {
            var (iDTO, oDTO) = InitTask<CreateTransactionsForPlannedPeriodCalculationInputDTO, CreateTransactionsForPlannedPeriodCalculationOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            ClearCachedContent();

            List<SysExtraField> sysExtraFields = ExtraFieldManager.GetSysExtraFields(SoeEntityType.PayrollProductSetting);

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        Employee employee = GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId);
                        if (employee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));
                            return oDTO;
                        }

                        oDTO.Result = GetPlanningPeriodsForPeriodCalculation(iDTO.TimePeriodId, out var childPeriod, out var parentPeriod);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = DeleteTransactionsForPlanningPeriodCalculation(employee, childPeriod, parentPeriod);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = ApplyPlanningPeriodCalculation(employee, childPeriod, parentPeriod, sysExtraFields, out var childSummary, out var parentSummary);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        var products = ApplyPlannedPeriodCalculationRules(employee, childSummary, parentSummary, childPeriod, parentPeriod);
                        oDTO.Result = CreateTimePayrollTransactions(employee, products);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        TryCommit(oDTO);
                    }
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
            }

            return oDTO;
        }

        #endregion

        #region TimeRule evaluation

        private List<TimeChunk> EvaluateRule(TimeEngineTemplate template, TimeChunk inputTimeChunk, TimeRule timeRule, TimeCode timeCode, List<int> timeDeviationCauseIdsOvertime, List<int> timeScheduleTypeIdsIsNotScheduleTime, List<TimeScheduleTemplateBlock> scheduleBlocks, ref List<TimeScheduleTemplateBlock> scheduleBlocksForOvertimePeriod, List<TimeBlock> presenceTimeBlocks, ref List<TimeBlock> presenceTimeBlocksForOvertimePeriod, List<TimeCodeTransaction> previousTimeCodeTransactions = null)
        {
            if (template == null)
                return new List<TimeChunk>();
            return EvaluateRule(inputTimeChunk, template.Date, template.Employee, template.EmployeeGroup, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, scheduleBlocks, ref scheduleBlocksForOvertimePeriod, presenceTimeBlocks, ref presenceTimeBlocksForOvertimePeriod, previousTimeCodeTransactions);
        }

        private List<TimeChunk> EvaluateRule(TimeChunk inputTimeChunk, DateTime date, Employee employee, EmployeeGroup employeeGroup, TimeRule timeRule, TimeCode timeCode, List<int> timeDeviationCauseIdsOvertime, List<int> timeScheduleTypeIdsIsNotScheduleTime, List<TimeScheduleTemplateBlock> scheduleBlocks, ref List<TimeScheduleTemplateBlock> scheduleBlocksForOvertimePeriod, List<TimeBlock> presenceTimeBlocks, ref List<TimeBlock> presenceTimeBlocksForOvertimePeriod, List<TimeCodeTransaction> previousTimeCodeTransactions = null)
        {
            if (timeRule.ContainsOvertimeOperand())
            {
                bool doSetTimeBlockTimeScheduleTypes = false;
                if (scheduleBlocksForOvertimePeriod == null)
                {
                    scheduleBlocksForOvertimePeriod = GetScheduleBlocksInOvertimePeriod(employee.EmployeeId, date);
                    doSetTimeBlockTimeScheduleTypes = true;
                }
                if (presenceTimeBlocksForOvertimePeriod == null)
                {
                    presenceTimeBlocksForOvertimePeriod = GetTimeBlocksInOvertimePeriod(employee.EmployeeId, employeeGroup, date, presenceTimeBlocks, timeDeviationCauseIdsOvertime);
                    doSetTimeBlockTimeScheduleTypes = true;
                }
                if (doSetTimeBlockTimeScheduleTypes)
                    SetTimeBlockTypes(employee.EmployeeId, date, presenceTimeBlocksForOvertimePeriod, scheduleBlocksForOvertimePeriod, setScheduleTypeOnConnectedTimeBlocksOutsideSchedule: false);
            }

            TimeEngineRuleEvaluatorParam param = new TimeEngineRuleEvaluatorParam(date, inputTimeChunk, employee, employeeGroup, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, scheduleBlocks, scheduleBlocksForOvertimePeriod, presenceTimeBlocks, presenceTimeBlocksForOvertimePeriod, previousTimeCodeTransactions);
            return TimeEngineRuleEvaluator.EvaluateRule(param);
        }

        #endregion

        #region AbsenceRule evaluation

        private (ApplyAbsenceDay absenceDay, bool valid) ApplyAbsence(TimeEngineTemplate template, ref PayrollProduct payrollProduct, ref TimeCode timeCode, TimeCodeTransaction timeCodeTransaction, TimeBlock timeBlock, Employee employee, EmployeeChild employeeChild, List<ApplyAbsenceDayBase> absenceDays, List<ApplyAbsenceResult> applyAbsenceResults, bool isScheduleTransaction, ref bool? hasCheckAbsenceOnReversedDay)
        {
            ApplyAbsenceDay absenceDay = null;
            int? newProductId = payrollProduct.ProductId;
            int? replaceStandbyProductId = null;

            #region Absence sick iwh/standby

            if (timeCodeTransaction.IsSickDuringIwhOrStandbyTransaction)
            {
                //Unique day
                template.Identity.IsUniqueDay = true;

                //TimeAbsenceRules
                ApplyAbsenceSickIwhStandbyDay absenceSickIwhStandbyDay = ApplyAbsenceRulesSickIwhOrStandBy(template, timeCodeTransaction, payrollProduct.ProductId);
                if (absenceSickIwhStandbyDay == null || !absenceSickIwhStandbyDay.NewProductId.HasValue)
                {
                    //No PayrollTransaction should be generated. Detach and remove TimeCodeTransaction
                    base.TryDetachEntity(entities, timeCodeTransaction);
                    return (null, false);
                }

                newProductId = absenceSickIwhStandbyDay.NewProductId;
                absenceDays.Add(absenceSickIwhStandbyDay);

                //Set new TimeCode on TimeCodeTransaction
                if (absenceSickIwhStandbyDay.NewTimeCodeId.HasValue && absenceSickIwhStandbyDay.NewTimeCodeId.Value != timeCodeTransaction.TimeCodeId)
                    timeCodeTransaction.TimeCode = GetTimeCodeFromCache(absenceSickIwhStandbyDay.NewTimeCodeId.Value);
            }
            else if (template.Outcome.UseStandby && !absenceDays.IsNullOrEmpty())
            {
                replaceStandbyProductId = GetStandbyReplaceProductId(absenceDays, timeCodeTransaction);
                if (replaceStandbyProductId.HasValue)
                    newProductId = replaceStandbyProductId.Value;
            }

            #endregion

            #region Regular absence

            if (!replaceStandbyProductId.HasValue && payrollProduct.IsAbsence())
            {
                VacationGroupDTO currentVacationGroup = GetVacationGroupFromCache(employee.EmployeeId, template.Date);
                if (currentVacationGroup != null)
                {
                    if (!hasCheckAbsenceOnReversedDay.HasValue)
                        hasCheckAbsenceOnReversedDay = CheckAbsenceOnReversedDay(payrollProduct, template.Identity.TimeBlockDate, template.EmployeeId);

                    ApplyAbsenceResult applyAbsenceResult = ApplyAbsenceResult.GetResult(applyAbsenceResults, payrollProduct);
                    if (applyAbsenceResult != null)
                    {
                        absenceDay = applyAbsenceResult.AbsenceDay;
                    }
                    else
                    {
                        absenceDay = ApplyAbsenceRules(template, currentVacationGroup, timeBlock, employeeChild, isScheduleTransaction, ref timeCode, ref payrollProduct);
                        if (absenceDay != null)
                        {
                            if (absenceDay.NewProductId.HasValue)
                                applyAbsenceResults.Add(new ApplyAbsenceResult(absenceDay, payrollProduct));

                            AddOrUpdateDaysToApplyAbsenceTracker(absenceDay);
                            absenceDays.Add(absenceDay);
                        }
                        else if (payrollProduct.IsAbsence())
                        {
                            //Add absence day to tracker even if no rules matched, to ensure that absence is tracked
                            TryAddAbsenceDayIfNotExistsToApplyAbsenceTracker(new ApplyAbsenceDay(template.Date, payrollProduct));
                        }
                    }

                    if (absenceDay != null)
                        newProductId = absenceDay.NewProductId;
                }
            }

            #endregion

            if (newProductId.HasValue && newProductId.Value != payrollProduct.ProductId)
                payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(newProductId.Value);

            UpdateAbsenceDay(template.EmployeeId, template.Date, timeBlock, payrollProduct);

            return (absenceDay, true);
        }

        private ApplyAbsenceSickIwhStandbyDay ApplyAbsenceRulesSickIwhOrStandBy(TimeEngineTemplate template, TimeCodeTransaction timeCodeTransaction, int productId)
        {
            if (template == null)
                return null;
            if (!HasEmployeeRightToSicknessSalaryFromCache(template.Employee, template.Date))
                return null;

            ApplyAbsenceSickIwhStandbyDay oDTO = new ApplyAbsenceSickIwhStandbyDay(template.Date, productId);

            if (!template.HasValidIdentity() || template.Outcome.TimeAbsenceRules.IsNullOrEmpty() || timeCodeTransaction == null)
                return oDTO;

            #region Evaluate TimeAbsenceRuleHead

            foreach (TimeAbsenceRuleHead timeAbsenceRule in template.Outcome.TimeAbsenceRules)
            {
                if (timeCodeTransaction.IsSickDuringIwhTransaction && !timeAbsenceRule.IsSickDuringIwh)
                    continue;
                if (timeCodeTransaction.IsAbsenceDuringStandbyTransaction && !timeAbsenceRule.IsSickDuringStandby)
                    continue;

                List<TimeAbsenceRuleRow> absenceRuleRows = timeAbsenceRule.GetRows(TermGroup_TimeAbsenceRuleRowType.CalendarDay);
                if (absenceRuleRows.IsNullOrEmpty())
                    continue;

                int maxDays = absenceRuleRows.GetMaxDays();
                int interval = Constants.SICKNESS_RELAPSEDAYS;
                int absenceDayNumber = GetDayOfAbsenceNumberCoherent(template.EmployeeId, template.Date, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick, maxDays, interval, out DateTime qualifyingDate, out int _, out int _);
                if (absenceDayNumber == 0)
                    continue;

                TimeAbsenceRuleRow absenceRuleRow = absenceRuleRows.GetRow(absenceDayNumber);
                if (absenceRuleRow == null)
                    return new ApplyAbsenceSickIwhStandbyDay(template.Date, null); //No TimeAbsenceRuleRow found, i.e. illnessDayNumber has past last rule. Indicate that the TimePayrollTransaction should be removed

                if (!absenceRuleRow.TimeAbsenceRuleRowPayrollProducts.IsLoaded)
                    absenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Load();

                int? replaceRelatedProductId = timeAbsenceRule.IsSickDuringStandby ? CalculateStandbyReplaceProductId(timeAbsenceRule, absenceRuleRow) : null;

                foreach (TimeAbsenceRuleRowPayrollProducts timeAbsenceRuleRowPayrollProducts in absenceRuleRow.TimeAbsenceRuleRowPayrollProducts)
                {
                    if (timeAbsenceRuleRowPayrollProducts.SourcePayrollProductId != productId || !timeAbsenceRuleRowPayrollProducts.TargetPayrollProductId.HasValue)
                        continue;

                    oDTO.Update(
                        timeAbsenceRule,
                        absenceRuleRow,
                        absenceDayNumber: absenceDayNumber,
                        qualifyingDate: qualifyingDate,
                        newProductId: timeAbsenceRuleRowPayrollProducts.TargetPayrollProductId.Value,
                        newTimeCodeId: timeAbsenceRule.TimeCodeId,
                        replaceRelatedProductId: replaceRelatedProductId,
                        replaceStartTime: timeCodeTransaction.Start,
                        replaceStopTime: timeCodeTransaction.Stop);
                }

                if (replaceRelatedProductId.HasValue && !oDTO.ReplaceRelatedProductId.HasValue)
                {
                    oDTO.Update(
                        timeAbsenceRule,
                        absenceRuleRow,
                        absenceDayNumber: absenceDayNumber,
                        qualifyingDate: qualifyingDate,
                        replaceRelatedProductId: replaceRelatedProductId.Value,
                        replaceStartTime: timeCodeTransaction.Start,
                        replaceStopTime: timeCodeTransaction.Stop);
                }
            }

            #endregion

            return oDTO;
        }

        private ApplyAbsenceDay ApplyAbsenceRules(TimeEngineTemplate template, VacationGroupDTO currentVacationGroup, TimeBlock timeBlock, EmployeeChild employeeChild, bool isScheduleTransaction, ref TimeCode timeCode, ref PayrollProduct payrollProduct)
        {
            ApplyAbsenceDay absenceDay = null;

            #region Prereq

            if (!template.HasValidIdentity() || !template.HasAnyRuleExceptSickDuringIwhOrStandby() || currentVacationGroup == null || timeCode == null || payrollProduct?.SysPayrollTypeLevel3 == null || !payrollProduct.IsAbsence())
                return absenceDay;

            currentVacationGroup.RealDateFrom = currentVacationGroup.CalculateFromDate(template.Date);
            bool isAbsenceVacation = payrollProduct.IsAbsenceVacation();
            bool isOtherAbsence = !isAbsenceVacation;
            bool isAbsenceTotal = payrollProduct.IsAbsenceMilitaryServiceTotal();
            bool isParentalLeaveWithChild = payrollProduct.IsParentalLeave() && employeeChild != null;
            bool discardCalculationStartDate = isParentalLeaveWithChild || payrollProduct.IsAbsenceMilitaryServiceTotal();
            bool isVacationReplacement = false;
            DateTime calculateToDate = template.Date.AddDays(-1);
            int? sysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;

            #endregion

            #region Vacation

            if (isAbsenceVacation)
            {
                absenceDay = ApplyVacationRules(template, payrollProduct, currentVacationGroup, calculateToDate, isScheduleTransaction, out isOtherAbsence);
                if (absenceDay == null)
                    return null;

                if (isOtherAbsence)
                {
                    var (timeCodeFromVacationGroup, payrollProductFromVacationGroup) = GetTimeCodeAndPayrollProductFromVacationGroupFromCache(currentVacationGroup);
                    if (timeCodeFromVacationGroup != null && payrollProductFromVacationGroup != null)
                    {
                        isVacationReplacement = true;
                        timeCode = timeCodeFromVacationGroup;
                        payrollProduct = payrollProductFromVacationGroup;
                    }
                    else
                        isOtherAbsence = false;
                }
            }

            #endregion

            #region Other absence (other than vacation or vacation replaced with other absence, normally leave of absence)

            if (isOtherAbsence)
            {
                absenceDay = new ApplyAbsenceDay(template.Date, payrollProduct, isVacationReplacement: isVacationReplacement, otherApplyAbsenceDTO: absenceDay);

                #region Check payed/unpayed

                int? payedDaysForTypeAccordingToRule = GetAbsencePayedDays(template.EmployeeId, payrollProduct.SysPayrollTypeLevel3.Value, employeeChild);
                if (!payedDaysForTypeAccordingToRule.HasValue)
                    return absenceDay;

                bool isPayedDay = true;
                int nrOfAbsenceDays = 0;

                if (payedDaysForTypeAccordingToRule <= 0)
                {
                    isPayedDay = false;
                }
                else if (
                    payrollProduct.IsParentalLeave() ||
                    isAbsenceTotal ||
                    payedDaysForTypeAccordingToRule <= GetAbsenceElapsedDays(currentVacationGroup.RealDateFrom, calculateToDate))
                {
                    if (payrollProduct.IsParentalLeave() && employeeChild != null)
                        nrOfAbsenceDays += employeeChild.OpeningBalanceUsedDays;

                    if (nrOfAbsenceDays < payedDaysForTypeAccordingToRule)
                    {
                        DateTime? calculationFromDate = discardCalculationStartDate ? (DateTime?)null : currentVacationGroup.RealDateFrom;
                        List<TimePayrollTransaction> timePayrollTransactionsCurrentToDate = GetTransactions(payrollProduct, calculationFromDate, calculateToDate);
                        foreach (var timePayrollTransactionsByTimeBlockDateId in timePayrollTransactionsCurrentToDate.GroupBy(i => i.TimeBlockDateId))
                        {
                            decimal quantity = timePayrollTransactionsByTimeBlockDateId.Sum(i => i.Quantity);
                            if (quantity > 0)
                                nrOfAbsenceDays++;
                            else if (quantity == 0 && !timePayrollTransactionsByTimeBlockDateId.Any(i => i.IsReversed))
                                nrOfAbsenceDays++;
                        }
                    }

                    if (nrOfAbsenceDays >= payedDaysForTypeAccordingToRule)
                        isPayedDay = false;
                }

                if (isPayedDay && payrollProduct.IsAbsenceSick())
                    isPayedDay = IsHealthyEarningYear(template.EmployeeId, payrollProduct.SysPayrollTypeLevel3.Value, template.Date, currentVacationGroup);

                #endregion

                #region Evaluate TimeAbsenceRuleHead

                TermGroup_TimeAbsenceRuleType absenceRuleType = GetAbsenceRuleType(payrollProduct.SysPayrollTypeLevel3.Value, isPayedDay);

                bool timeAbsenceRuleHeadResulted = false;
                foreach (TimeAbsenceRuleHead absenceRule in template.Outcome.TimeAbsenceRules.Filter(absenceRuleType, timeCode.TimeCodeId, template.EmployeeGroupId))
                {
                    if (timeAbsenceRuleHeadResulted || absenceRule.IsSickDuringIwhOrStandBy)
                        break;

                    TermGroup_TimeAbsenceRuleRowScope absenceRuleRowScope = GetAbsenceRuleScope(absenceRule);

                    List<TimeAbsenceRuleRow> absenceRuleRows = absenceRule.GetRows(scope: absenceRuleRowScope);
                    if (absenceRuleRows.IsNullOrEmpty())
                        continue;

                    int absenceDayNumber = GetDayOfAbsenceNumber(template, payrollProduct, absenceRuleRows, absenceRuleRowScope, out DateTime qualifyingDate, out int checkDaysBack, out int checkDaysForward, isAbsenceTotal, nrOfAbsenceDays, employeeChild);
                    if (absenceDayNumber == 0)
                        continue;

                    TermGroup_TimeAbsenceRuleRowType absenceRuleRowType = GetAbsenceRuleRowType(template, absenceRule, payrollProduct.SysPayrollTypeLevel3.Value, timeBlock, absenceDayNumber, forceWholeday: isAbsenceVacation);
                    if (absenceRuleRowType == TermGroup_TimeAbsenceRuleRowType.Unknown)
                        continue;

                    TimeAbsenceRuleRow timeAbsenceRuleRow = absenceRuleRows.GetRow(absenceDayNumber, absenceRuleRowType);
                    if (timeAbsenceRuleRow?.PayrollProductId != null && GetPayrollProductFromCache(timeAbsenceRuleRow.PayrollProductId.Value) == null)
                        continue;

                    if (timeAbsenceRuleRow != null)
                    {
                        var (absenceDayNumberInSequence, absenceRowInSequence) = payrollProduct.IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave() ? GetAbsenceRowSequence(template, absenceRule, absenceRuleRows, payrollProduct, absenceRuleRowType, absenceRuleRowScope, forceWholeday: isAbsenceVacation, employeeChild: employeeChild) : (null, null);
                        if (absenceRowInSequence != null && absenceRowInSequence.TimeAbsenceRuleRowId != timeAbsenceRuleRow.TimeAbsenceRuleRowId)
                        {
                            if (absenceRowInSequence.PayrollProductId.HasValue)
                            {
                                timeAbsenceRuleHeadResulted = true;
                                absenceDay.Update(
                                    absenceRule,
                                    absenceRowInSequence,
                                    absenceDayNumber: absenceDayNumberInSequence.Value,
                                    qualifyingDate: qualifyingDate,
                                    newProductId: absenceRowInSequence.PayrollProductId.Value,
                                    hasAbsenceDayNumberFromSequence: true);
                            }
                        }
                        else if (timeAbsenceRuleRow.PayrollProductId.HasValue)
                        {
                            timeAbsenceRuleHeadResulted = true;
                            absenceDay.Update(
                                absenceRule,
                                timeAbsenceRuleRow,
                                absenceDayNumber: absenceDayNumber,
                                qualifyingDate: qualifyingDate,
                                newProductId: timeAbsenceRuleRow.PayrollProductId.Value);
                        }
                    }

                    if (DoCollectDaysForRecalculation())
                    {
                        bool doFetchBackwards = payrollProduct.IsLeaveOfAbsence() || payrollProduct.IsAbsenceParentalLeaveOrTemporaryParentalLeave();
                        if (doFetchBackwards)
                            TryLoadTransactionsBack(payrollProduct, checkDaysBack);

                        bool doFetchForward = doFetchBackwards || payrollProduct.IsAbsenceSickOrWorkInjury();
                        if (doFetchForward && isParentalLeaveWithChild)
                            TryLoadTransactionsForwardWithChild(payrollProduct);
                        else if (doFetchForward)
                            TryLoadTransactionsForward(payrollProduct, checkDaysForward);
                    }
                }

                #endregion
            }

            #endregion

            void TryLoadTransactionsBack(PayrollProduct pp, int checkDaysBack)
            {
                if (absenceDay.TrySetFetchBack(template.Date, checkDaysBack, HadSameAbsenceBefore(template.EmployeeId, template.Date, sysPayrollTypeLevel3)))
                    absenceDay.SetTimePayrollTransactionsToRecalculate(GetTransactions(pp, absenceDay.FetchBackwardStartDate.Value, absenceDay.FetchBackwardStopDate.Value));
            }
            void TryLoadTransactionsForward(PayrollProduct pp, int checkDaysForward)
            {
                if (absenceDay.TrySetFetchForward(template.Date, checkDaysForward, HadSameAbsenceBefore(template.EmployeeId, template.Date, sysPayrollTypeLevel3)))
                    absenceDay.SetTimePayrollTransactionsToRecalculate(GetTransactions(pp, absenceDay.FetchForwardStartDate.Value, absenceDay.FetchForwardStopDate.Value));
            }
            void TryLoadTransactionsForwardWithChild(PayrollProduct pp)
            {
                //Pass null as start to trigger that cache not going to read all TimeBlockDates for range. Then filter start after
                List<TimePayrollTransaction> transactions = GetTransactions(pp, null, DateTime.MaxValue).GreaterThan(template.Date);
                if (absenceDay.TrySetFetchForward(transactions.GetDates(), HadSameAbsenceBefore(template.EmployeeId, template.Date, sysPayrollTypeLevel3)))
                    absenceDay.SetTimePayrollTransactionsToRecalculate(transactions);
            }
            List<TimePayrollTransaction> GetTransactions(PayrollProduct pp, DateTime? dateFrom, DateTime dateTo)
            {
                return GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(template.EmployeeId, dateFrom, dateTo, pp.SysPayrollTypeLevel3.Value, employeeChild: employeeChild);
            }

            return absenceDay;
        }

        private ApplyAbsenceDay ApplyVacationRules(TimeEngineTemplate template, PayrollProduct payrollProduct, VacationGroupDTO currentVacationGroup, DateTime calculateToDate, bool isScheduleTransaction, out bool isOtherAbsence)
        {
            isOtherAbsence = false; //Will be true when vacation days are used and should be leave of absence instead
            if (!template.HasValidIdentity() || currentVacationGroup == null || payrollProduct == null || !payrollProduct.IsAbsence())
                return null;

            #region Prereq

            ApplyAbsenceDay absenceDay = new ApplyAbsenceDay(template.Date, payrollProduct, isVacation: true);
            UpdateAbsenceDay(template.EmployeeId, template.Date, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation);

            int scheduleMinutes = template.Identity.ScheduleBlocks.GetWorkMinutes();
            if (IsVacationFiveDaysPerWeekFromCache(currentVacationGroup, template.EmployeeId, template.Date))
                SetVacationFiveDays(currentVacationGroup, absenceDay, template.EmployeeId, template.Date, scheduleMinutes);

            DateTime? usedPayrollSince = GetCompanyDateTimeSettingFromCache(CompanySettingType.UsedPayrollSince);
            if (usedPayrollSince <= CalendarUtility.DATETIME_DEFAULT)
                usedPayrollSince = null;

            VacationYearEndRow vacationYearEndRow = GetLatestVacationYearEndRowWithHead(template.EmployeeId);
            bool hasVacationYearEndOrFinalSalaryEmployment = vacationYearEndRow != null || template.Employee.HasFinalSalaryEmployments(currentVacationGroup.RealDateFrom.AddDays(-1), template.Date);
            bool isPayrollSinceOutsideVacationGroup = !usedPayrollSince.HasValue || usedPayrollSince.Value < currentVacationGroup.RealDateFrom || usedPayrollSince.Value > currentVacationGroup.RealDateTo;

            //Year 1. If started to use Payroll this year, continue evaluating vacation even thu a vacationyearend isnt created. If passed VacationYear date, do not evaluate vacation next year
            if (!hasVacationYearEndOrFinalSalaryEmployment && isPayrollSinceOutsideVacationGroup)
            {
                VacationGroupDTO vacationGroupForStartOfYear = GetVacationGroupFromCache(template.EmployeeId, CalendarUtility.GetBeginningOfYear(DateTime.Today));
                if (vacationGroupForStartOfYear != null && vacationGroupForStartOfYear.RealDateFrom < currentVacationGroup.RealDateFrom)
                    return absenceDay;
            }
            //Year > 1. If VacationYearEnd is not created for last year, do not evaluate vacation next year
            if (vacationYearEndRow?.VacationYearEndHead != null && vacationYearEndRow.VacationYearEndHead.Date.AddDays(1) < currentVacationGroup.RealDateFrom)
            {
                Employment vacationYearEndEmployment = template.Employee.GetEmployment(currentVacationGroup.RealDateFrom.AddDays(-1));
                Employment currentEmployment = template.Employee.GetEmployment(template.Date);
                bool isNewEmploymentAfterVacationYearEnd = vacationYearEndEmployment == null && currentEmployment?.DateFrom > currentVacationGroup.RealDateFrom.AddDays(-1);
                if (!isNewEmploymentAfterVacationYearEnd)
                    return absenceDay;
            }

            EmployeeVacationSE employeeVacationSE = GetEmployeeVacationSEFromCache(template.EmployeeId);
            int? sysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;

            #endregion

            #region Decide SysPayrollType

            TermGroup_SysPayrollType sysPayrollTypeLevel4;
            int checkDaysBack = 0;

            if (absenceDay.VacationFiveDaysPerWeekIsWeekend == true && employeeVacationSE != null)
            {
                #region Set SysPayrollTypeLevel4 hard-coded

                sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_NoVacationDaysDeducted;

                #endregion
            }
            else
            {
                #region Check preliminary vacation days

                List<int> lockedAttestStateIds = new List<int>()
                {
                    GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentLockedAttestStateId),
                    GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved1AttestStateId),
                    GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved2AttestStateId),
                    GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId),
                };

                List<TimeBlockDate> timeBlockdates = GetTimeBlockDatesFromCache(template.EmployeeId, currentVacationGroup.RealDateFrom, calculateToDate);

                List<TimePayrollTransaction> timePayrollTransactionsCurrentToDate = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(template.EmployeeId, timeBlockdates, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation);
                timePayrollTransactionsCurrentToDate = timePayrollTransactionsCurrentToDate.Where(i => !i.IsAbsenceVacationNoVacationDaysDeducted()).ToList();
                timePayrollTransactionsCurrentToDate.AddRange(GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(template.EmployeeId, timeBlockdates));
                timePayrollTransactionsCurrentToDate = timePayrollTransactionsCurrentToDate.Where(i => !i.RetroactivePayrollOutcomeId.HasValue).ToList();

                List<TimePayrollTransaction> timePayrollTransactionsCurrentToDatePrel = timePayrollTransactionsCurrentToDate.Where(i => i.SysPayrollTypeLevel4.HasValue && !lockedAttestStateIds.Contains(i.AttestStateId)).ToList();
                if (usedPayrollSince.HasValue)
                    timePayrollTransactionsCurrentToDatePrel = timePayrollTransactionsCurrentToDatePrel.Where(i => i.TimeBlockDate.Date >= usedPayrollSince.Value).ToList();

                List<TimePayrollTransaction> timePayrollTransactionsVacationYearEnd = GetTransactions(currentVacationGroup.RealDateFrom, currentVacationGroup.RealDateTo, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1);

                List<TimePayrollTransaction> timePayrollTransactionsVacationYearEndPrel = timePayrollTransactionsVacationYearEnd.Where(i => !lockedAttestStateIds.Contains(i.AttestStateId) && i.VacationYearEndRowId.HasValue).ToList();
                if (usedPayrollSince.HasValue)
                    timePayrollTransactionsVacationYearEndPrel = timePayrollTransactionsVacationYearEndPrel.Where(i => i.TimeBlockDate.Date >= usedPayrollSince.Value).ToList();

                List<EmployeeFactor> vacationCoefficients = GetEmployeeFactorsFromCache(template.EmployeeId, TermGroup_EmployeeFactorType.VacationCoefficient);
                List<EmployeeFactor> vacationFactorNet = GetEmployeeFactorsFromCache(template.EmployeeId, TermGroup_EmployeeFactorType.Net);
                TermGroup_VacationGroupCalculationType calculationType = currentVacationGroup?.VacationGroupSE?.CalculationType ?? TermGroup_VacationGroupCalculationType.Unknown;
                bool sammanfallande = calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement || calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;

                #endregion

                #region EmployeeVacationSE values

                decimal prelPaidInVacationYearEnd = timePayrollTransactionsVacationYearEndPrel.Sum(i => i.Quantity); //Sum Quantity because we dont have TimePayrollTransactionExtended loaded (should be QuantityVacationDays, but this will work until it dont :) )
                decimal remainingDaysPaid = 0;
                if (sammanfallande && employeeVacationSE != null && (employeeVacationSE.RemainingDaysPaid ?? 0) == 0 && (employeeVacationSE.EarnedDaysPaid ?? 0) > 0) //sammanfallande and first year where remainingdays is not set yet.
                {
                    remainingDaysPaid = decimal.Subtract(employeeVacationSE.EarnedDaysPaid.Value, employeeVacationSE.UsedDaysPaid ?? 0);

                    if (remainingDaysPaid < 0)
                        remainingDaysPaid = 0;
                }
                else
                {
                    remainingDaysPaid = employeeVacationSE?.RemainingDaysPaid ?? 0;
                }

                decimal remainingDaysAdvance = employeeVacationSE?.RemainingDaysAdvance ?? 0;
                decimal remainingDaysOverdue = employeeVacationSE?.RemainingDaysOverdue ?? 0;
                decimal remainingDaysYear5 = employeeVacationSE?.RemainingDaysYear5 ?? 0;
                decimal remainingDaysYear4 = employeeVacationSE?.RemainingDaysYear4 ?? 0;
                decimal remainingDaysYear3 = employeeVacationSE?.RemainingDaysYear3 ?? 0;
                decimal remainingDaysYear2 = employeeVacationSE?.RemainingDaysYear2 ?? 0;
                decimal remainingDaysYear1 = employeeVacationSE?.RemainingDaysYear1 ?? 0;
                remainingDaysYear1 -= prelPaidInVacationYearEnd;
                decimal remainingDaysUnpaid = employeeVacationSE?.RemainingDaysUnpaid ?? 0;

                #endregion

                #region Calculate transactions

                foreach (var timePayrollTransactionsGrouping in timePayrollTransactionsCurrentToDatePrel.GroupBy(i => i.TimeBlockDateId))
                {
                    DateTime date = timePayrollTransactionsGrouping.First().TimeBlockDate.Date;

                    #region Calculate quantity

                    EmployeeFactor vacationCoefficient = vacationCoefficients.GetEmployeeFactor(date);
                    decimal quantityDays = vacationCoefficient != null ? (1 * vacationCoefficient.Factor) : 1;
                    if (quantityDays <= 0)
                        continue;

                    #endregion

                    #region Hours

                    //VacationGroupVacationHandleRule
                    if (currentVacationGroup.VacationGroupSE.VacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Hours)
                    {
                        int workTimeWeek = template.Employee.GetEmployment(date)?.GetWorkTimeWeek() ?? 0;
                        if (workTimeWeek == 0)
                            continue;

                        EmployeeFactor vacationNet = vacationFactorNet.GetEmployeeFactor(date);
                        decimal netFactor = vacationNet?.Factor ?? 5;
                        decimal dayFactor = decimal.Divide(decimal.Divide(workTimeWeek, new decimal(60)), netFactor);
                        decimal hoursOnDay = decimal.Divide(timePayrollTransactionsGrouping.Sum(s => s.Quantity), 60);

                        quantityDays = decimal.Divide(hoursOnDay, dayFactor);
                        if (quantityDays <= 0)
                            continue;
                    }

                    #endregion

                    #region Reduce payed/advance/overdue/saved/unpaid (Can be less than zero)

                    //Paid
                    if (remainingDaysPaid > 0)
                        remainingDaysPaid -= quantityDays;
                    //Advance
                    else if (remainingDaysAdvance > 0)
                        remainingDaysAdvance -= quantityDays;
                    //Overdue
                    else if (remainingDaysOverdue > 0)
                        remainingDaysOverdue -= quantityDays;
                    //SavedYear5
                    else if (remainingDaysYear5 > 0)
                        remainingDaysYear5 -= quantityDays;
                    //SavedYear4
                    else if (remainingDaysYear4 > 0)
                        remainingDaysYear4 -= quantityDays;
                    //SavedYear3
                    else if (remainingDaysYear3 > 0)
                        remainingDaysYear3 -= quantityDays;
                    //SavedYear2
                    else if (remainingDaysYear2 > 0)
                        remainingDaysYear2 -= quantityDays;
                    //SavedYear1
                    else if (remainingDaysYear1 > 0)
                        remainingDaysYear1 -= quantityDays;
                    //Unpaid
                    else if (remainingDaysUnpaid > 0)
                        remainingDaysUnpaid -= quantityDays;

                    #endregion
                }

                #endregion

                #region Set SysPayrollTypeLevel4 based on days

                //Paid
                if (remainingDaysPaid > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid;
                //Advance
                else if (remainingDaysAdvance > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Advance;
                //Overdue
                else if (remainingDaysOverdue > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedOverdue;
                //SavedYear5
                else if (remainingDaysYear5 > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear5;
                //SavedYear4
                else if (remainingDaysYear4 > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear4;
                //SavedYear3
                else if (remainingDaysYear3 > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear3;
                //SavedYear2
                else if (remainingDaysYear2 > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear2;
                //SavedYear1
                else if (remainingDaysYear1 > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear1;
                //Unpaid
                else if (remainingDaysUnpaid > 0)
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid;
                //Leave of absence
                else
                {
                    sysPayrollTypeLevel4 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid;
                    isOtherAbsence = true; //Flag calling method to try to replace vacation with leave of absence, if everything is correctly setuped (if not, unpaid will remain)
                }

                //Only resume if leave of absence should be inferred
                if (!isOtherAbsence && !absenceDay.IsVacationFiveDaysPerWeek && scheduleMinutes <= 0)
                    return absenceDay;

                #endregion
            }

            #endregion

            #region Find PayrollProduct

            if (sysPayrollTypeLevel4 != TermGroup_SysPayrollType.None)
            {
                PayrollProduct newPayrollProduct = GetPayrollProductFromCache(sysPayrollTypeLevel4: (int)sysPayrollTypeLevel4);
                if (newPayrollProduct != null)
                {
                    if (!isScheduleTransaction)
                        newPayrollProduct = GetTurnedPayrollProductForAbsenceVacation(template, newPayrollProduct, out checkDaysBack) ?? newPayrollProduct;
                    absenceDay.Update(newProductId: newPayrollProduct.ProductId);
                }
            }

            #endregion

            #region Recalculate

            if (DoCollectDaysForRecalculation())
            {
                bool doFetchBackwards = TryAdjustDaysBackAfterInitiatedAbsence(template.EmployeeId, template.Date, ref checkDaysBack);
                if (doFetchBackwards)
                    TryLoadTransactionsBack();

                bool doFetchForwards = !TryUpdateExistingAbsenceDayForward();
                if (doFetchForwards)
                    TryLoadTransactionsForward();
            }

            #endregion

            void TryLoadTransactionsBack()
            {
                if (absenceDay.TrySetFetchBack(template.Date, checkDaysBack, HadSameAbsenceBefore(template.EmployeeId, template.Date, sysPayrollTypeLevel3)))
                    absenceDay.SetTimePayrollTransactionsToRecalculate(GetTransactions(absenceDay.FetchBackwardStartDate.Value, absenceDay.FetchBackwardStopDate.Value, TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation));
            }
            bool TryUpdateExistingAbsenceDayForward()
            {
                //Forward - check if any other absence item has included current day
                ApplyAbsenceDay existingAbsenceDay = GetDaysFromApplyAbsenceTracker().FirstOrDefault(i => i.IsDateIncludedInForwardTransactions(template.Date));
                if (existingAbsenceDay != null)
                {
                    //Exclude current day from being recalculated later
                    existingAbsenceDay.SetFetchForward(startDate: template.Date.AddDays(1));
                    return true;
                }
                return false;
            }
            void TryLoadTransactionsForward()
            {
                absenceDay.SetFetchForward(template.Date.AddDays(1), currentVacationGroup.RealDateTo);
                absenceDay.SetTimePayrollTransactionsToRecalculate(GetTransactions(absenceDay.FetchForwardStartDate.Value, absenceDay.FetchForwardStopDate.Value, TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation));
                absenceDay.SetTimePayrollTransactionsToRecalculate(GetReplacementTransactions(absenceDay.FetchForwardStartDate.Value, absenceDay.FetchForwardStopDate.Value));
            }
            List<TimePayrollTransaction> GetTransactions(DateTime dateFrom, DateTime dateTo, TermGroup_SysPayrollType sysPayrollType)
            {
                return GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(template.EmployeeId, dateFrom, dateTo, (int)sysPayrollType);
            }
            List<TimePayrollTransaction> GetReplacementTransactions(DateTime dateFrom, DateTime dateTo)
            {
                return GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(template.EmployeeId, dateFrom, dateTo);
            }

            return absenceDay;
        }

        private PayrollProduct GetTurnedPayrollProductForAbsenceScheduleTransaction(TimeEngineTemplate template, List<ApplyAbsenceResult> applyAbsenceResults, TimePayrollScheduleTransaction scheduleTransaction, TimeCodeTransaction timeCodeTransaction, PayrollProduct payrollProduct)
        {
            if (applyAbsenceResults.IsNullOrEmpty() || timeCodeTransaction == null || scheduleTransaction == null || (!scheduleTransaction.IsAddedTimeCompensation() && !scheduleTransaction.IsOvertimeCompensation() && !scheduleTransaction.IsOBAddition() && !scheduleTransaction.IsDuty()))
                return null;

            List<TimePayrollTransaction> timePayrollTransactionsForTimeBlock = template.GetTimePayrollTransactions(timeCodeTransaction);
            if (!timePayrollTransactionsForTimeBlock.IsNullOrEmpty())
            {
                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsForTimeBlock)
                {
                    ApplyAbsenceResult applyAbsenceDayResult = applyAbsenceResults.FirstOrDefault(i => i.NewProductId == timePayrollTransaction.ProductId);
                    if (applyAbsenceDayResult?.AbsenceDay != null)
                    {
                        if (timePayrollTransaction.IsAbsenceVacation() || timePayrollTransaction.IsVacationReplacement)
                            return GetTurnedPayrollProductForAbsenceVacation(template, scheduleTransaction.PayrollProduct ?? GetPayrollProductFromCache(scheduleTransaction.ProductId), out _, useHardcodedRulesAsFallback: true);
                        else
                            return GetTurnedPayrollProductForAbsenceNoneVacation(applyAbsenceDayResult, payrollProduct);
                    }
                }
            }

            return null;
        }

        private PayrollProduct GetTurnedPayrollProductForAbsenceVacation(TimeEngineTemplate template, PayrollProduct sourcePayrollProduct, out int checkDaysBack, bool useHardcodedRulesAsFallback = false)
        {
            checkDaysBack = 0;
            if (sourcePayrollProduct != null)
            {
                TimeAbsenceRuleHead absenceRule = template.GetTimeAbsenceRule(TermGroup_TimeAbsenceRuleType.Vacation);
                if (absenceRule != null)
                {
                    TimeAbsenceRuleRow timeAbsenceRuleRow = GetVacationAbsenceRuleRow(template, absenceRule, sourcePayrollProduct, out checkDaysBack);
                    return GetTurnedPayrollProductFromAbsenceRuleRow(sourcePayrollProduct, timeAbsenceRuleRow);
                }
                else if (useHardcodedRulesAsFallback)
                    return GetTurnedPayrollProductFromVacationHardcodedRules(sourcePayrollProduct);
            }
            return null;
        }

        private PayrollProduct GetTurnedPayrollProductFromAbsenceRuleRow(PayrollProduct sourcePayrollProduct, TimeAbsenceRuleRow timeAbsenceRuleRow)
        {
            if (timeAbsenceRuleRow == null || sourcePayrollProduct == null)
                return null;

            if (!timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.IsLoaded && base.CanEntityLoadReferences(entities, timeAbsenceRuleRow))
                timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Load();

            TimeAbsenceRuleRowPayrollProducts ruleRowPayrollProducts = timeAbsenceRuleRow.GetSourceProductRow(sourcePayrollProduct.ProductId);
            return ruleRowPayrollProducts?.TargetPayrollProductId != null ? GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(ruleRowPayrollProducts.TargetPayrollProductId.Value) : null;
        }

        private PayrollProduct GetTurnedPayrollProductFromVacationHardcodedRules(PayrollProduct source)
        {
            string productNr = source.GetTurnedNumberForVacation();
            return GetPayrollProductFromCache(productNr);
        }

        private PayrollProduct GetTurnedPayrollProductForAbsenceNoneVacation(ApplyAbsenceResult productAbsenceTuple, PayrollProduct sourcePayrollProduct)
        {
            if (productAbsenceTuple?.AbsenceDay?.TimeAbsenceRuleRow != null && sourcePayrollProduct != null)
            {
                if (!productAbsenceTuple.AbsenceDay.TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.IsLoaded)
                    productAbsenceTuple.AbsenceDay.TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Load();

                TimeAbsenceRuleRowPayrollProducts mapping = productAbsenceTuple.AbsenceDay.TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.FirstOrDefault(i => i.SourcePayrollProductId == sourcePayrollProduct.ProductId);
                if (mapping != null && mapping.TargetPayrollProductId.HasValue)
                    return GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(mapping.TargetPayrollProductId.Value);
            }
            return null;
        }

        private TermGroup_TimeAbsenceRuleType GetAbsenceRuleType(int sysPayrollTypeLevel3, bool isPayedDay)
        {
            TermGroup_TimeAbsenceRuleType type = TermGroup_TimeAbsenceRuleType.None;

            switch (sysPayrollTypeLevel3)
            {
                //Sjuk (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.Sick_PAID : TermGroup_TimeAbsenceRuleType.Sick_UNPAID;
                    break;
                //Arbetsskada (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury:
                    type = TermGroup_TimeAbsenceRuleType.WorkInjury_PAID;
                    break;
                //Tillfällig föräldrarledig (120/180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_PAID : TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_UNPAID;
                    break;
                //Graviditetspenning (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PregnancyCompensation:
                    type = TermGroup_TimeAbsenceRuleType.PregnancyMoney_PAID;
                    break;
                //Föräldrarledig (120/180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.ParentalLeave_PAID : TermGroup_TimeAbsenceRuleType.ParentalLeave_UNPAID;
                    break;
                //Överföring av smitta (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TransmissionOfInfection:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.DiseaseCarrier_PAID : TermGroup_TimeAbsenceRuleType.DiseaseCarrier_UNPAID;
                    break;
                //Facklig utbildning (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_UnionEduction:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.UnionEducation_PAID : TermGroup_TimeAbsenceRuleType.UnionEducation_UNPAID;
                    break;
                //Totalförsvarsplikt (60)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.MilitaryService_PAID : TermGroup_TimeAbsenceRuleType.MilitaryService_UNPAID;
                    break;
                //Totalförsvarsplikt total (60)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService_Total:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.MilitaryService_Total_PAID : TermGroup_TimeAbsenceRuleType.MilitaryService_Total_UNPAID;
                    break;
                // Svenska för invandrare (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_SwedishForImmigrants:
                    type = TermGroup_TimeAbsenceRuleType.SwedishForImmigrants_PAID;
                    break;
                // Betald frånvaro (Permittering/Permission) (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Permission:
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PayedAbsence:
                    type = TermGroup_TimeAbsenceRuleType.PayedAbsence_PAID;
                    break;
                // Ledighet utan lön (Tjänstledighet) (Alltid obetald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence:
                    type = TermGroup_TimeAbsenceRuleType.LeaveOfAbsence_UNPAID;
                    break;
                //Närståendevård (45)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_RelativeCare:
                    type = isPayedDay ? TermGroup_TimeAbsenceRuleType.RelativeCare_PAID : TermGroup_TimeAbsenceRuleType.RelativeCare_UNPAID;
                    break;
            }

            return type;
        }

        private TermGroup_TimeAbsenceRuleRowScope GetAbsenceRuleScope(TimeAbsenceRuleHead timeAbsenceRule)
        {
            return timeAbsenceRule.IsScopeCalendarYear() ? TermGroup_TimeAbsenceRuleRowScope.Calendaryear : TermGroup_TimeAbsenceRuleRowScope.Coherent;
        }

        private TermGroup_TimeAbsenceRuleRowType GetAbsenceRuleRowType(TimeEngineTemplate template, TimeAbsenceRuleHead timeAbsenceRule, int sysPayrollTypeLevel3, TimeBlock timeBlock = null, int? absenceDayNumber = null, bool forceWholeday = false)
        {
            TermGroup_TimeAbsenceRuleRowType type = TermGroup_TimeAbsenceRuleRowType.Unknown;

            if (!template.HasValidIdentity() || timeAbsenceRule == null)
                return type;

            var (isPartOfDay, scheduleMinutes) = IsAbsencePartOfDay(template, timeAbsenceRule, timeBlock, sysPayrollTypeLevel3, forceWholeday);
            if (absenceDayNumber.HasValue && absenceDayNumber.Value == 1 && PayrollRulesUtil.IsAbsenceSickOrWorkInjury(sysPayrollTypeLevel3))
            {
                if (HasEmployeeHighRiskProtectionFromCache(template.Employee, template.Date))
                    type = isPartOfDay ? TermGroup_TimeAbsenceRuleRowType.PartOfDay_QualifyingDayHighRiskProtection : TermGroup_TimeAbsenceRuleRowType.WholeDay_QualifyingDayHighRiskProtection;
                else
                    type = isPartOfDay ? TermGroup_TimeAbsenceRuleRowType.PartOfDay_QualifyingDay : TermGroup_TimeAbsenceRuleRowType.WholeDay_QualifyingDay;
            }
            else
            {
                if (scheduleMinutes == 0 && !isPartOfDay)
                    type = TermGroup_TimeAbsenceRuleRowType.CalendarDay;
                else
                    type = isPartOfDay ? TermGroup_TimeAbsenceRuleRowType.PartOfDay : TermGroup_TimeAbsenceRuleRowType.CalendarDay;
            }

            return type;
        }

        private TimeAbsenceRuleRow GetVacationAbsenceRuleRow(TimeEngineTemplate template, TimeAbsenceRuleHead absenceRule, PayrollProduct payrollProduct, out int checkDaysBack)
        {
            checkDaysBack = 0;
            List<TimeAbsenceRuleRow> absenceRuleRows = absenceRule.GetRows();
            if (absenceRuleRows.Count > 1)
            {
                TermGroup_TimeAbsenceRuleRowType absenceRuleRowType = TermGroup_TimeAbsenceRuleRowType.CalendarDay;
                TermGroup_TimeAbsenceRuleRowScope absenceRuleRowScope = TermGroup_TimeAbsenceRuleRowScope.Coherent;

                int absenceDayNumber = GetDayOfAbsenceNumber(template, payrollProduct, absenceRuleRows, absenceRuleRowScope, out _, out checkDaysBack, out _);
                if (absenceDayNumber == 0)
                    absenceDayNumber = 1;

                TimeAbsenceRuleRow timeAbsenceRuleRow = absenceRuleRows.GetRow(absenceDayNumber, absenceRuleRowType);
                if (timeAbsenceRuleRow == null)
                    return null;

                var (_, absenceRowInSequence) = GetAbsenceRowSequence(template, absenceRule, absenceRuleRows, payrollProduct, absenceRuleRowType, TermGroup_TimeAbsenceRuleRowScope.Coherent, forceWholeday: true);
                return absenceRowInSequence ?? timeAbsenceRuleRow;
            }
            else
                return absenceRuleRows.FirstOrDefault();
        }

        private (int?, TimeAbsenceRuleRow) GetAbsenceRowSequence(TimeEngineTemplate template, TimeAbsenceRuleHead absenceRule, List<TimeAbsenceRuleRow> absenceRuleRows, PayrollProduct payrollProduct, TermGroup_TimeAbsenceRuleRowType absenceRuleRowType, TermGroup_TimeAbsenceRuleRowScope absenceRuleRowScope, bool forceWholeday, EmployeeChild employeeChild = null)
        {
            int? absenceDayNumberInSequence = GetAbsenceDayNumberInSequence(template, absenceRule, payrollProduct, absenceRuleRowType, absenceRuleRowScope, forceWholeday, employeeChild);
            TimeAbsenceRuleRow absenceRowInSequence = absenceDayNumberInSequence.HasValue ? absenceRuleRows.GetRow(absenceDayNumberInSequence.Value, absenceRuleRowType) : null;
            return (absenceDayNumberInSequence, absenceRowInSequence);
        }

        private int? GetAbsenceDayNumberInSequence(TimeEngineTemplate template, TimeAbsenceRuleHead absenceRule, PayrollProduct payrollProduct, TermGroup_TimeAbsenceRuleRowType absenceRuleRowType, TermGroup_TimeAbsenceRuleRowScope absenceRuleRowScope, bool forceWholeday, EmployeeChild employeeChild = null)
        {
            if (absenceRuleRowScope != TermGroup_TimeAbsenceRuleRowScope.Coherent || payrollProduct?.SysPayrollTypeLevel3 == null)
                return null;
            return GetDayOfAbsenceNumberInSequence(template, absenceRule, payrollProduct.SysPayrollTypeLevel3.Value, absenceRuleRowType, forceWholeday: forceWholeday, employeeChild: employeeChild);
        }

        private (bool isPartOfDay, int scheduleMinutes) IsAbsencePartOfDay(TimeEngineTemplate template, TimeAbsenceRuleHead timeAbsenceRule, TimeBlock timeBlock, int sysPayrollTypeLevel3, bool forceWholeday)
        {
            if (template.Identity.ScheduleBlocks == null)
                template.Identity.ScheduleBlocks = GetScheduleBlocksFromCache(template.EmployeeId, template.Date);

            int scheduleMinutes = template.Identity.ScheduleBlocks.GetWorkMinutes();
            if (scheduleMinutes == 0 && HasCompanyPayrollStartValueFromCache(template.Date))
                scheduleMinutes = GetPayrollStartValueRowScheduleMinutesFromCache(template.EmployeeId, template.Date, sysPayrollTypeLevel3);

            decimal timeCodeMinutes = forceWholeday ? scheduleMinutes : template.Outcome.TimeCodeTransactions.SumQuantity(timeAbsenceRule.TimeCodeId);
            bool isPartOfDay = scheduleMinutes > timeCodeMinutes;
            if (this.useInitiatedAbsenceDays && !isPartOfDay && scheduleMinutes == 0)
                isPartOfDay = IsAbsencePartOfDayFromRatio(template.TimeBlockDate, timeBlock, sysPayrollTypeLevel3);

            return (isPartOfDay, scheduleMinutes);
        }

        private bool IsAbsencePartOfDayFromRatio(TimeBlockDate timeBlockDate, TimeBlock timeBlock, int sysPayrollTypeLevel3)
        {
            if (timeBlockDate == null)
                return false;

            decimal? ratio = null;
            if (timeBlock?.TimeDeviationCauseStartId != null)
            {
                //Is generating absence with TimeDeviationCause and ratio. Should use that ratio
                ratio = GetInitiatedAbsenceDaysRatio(timeBlockDate.EmployeeId, timeBlockDate.Date, timeBlock);
            }
            if (!ratio.HasValue && !HasInitiatedAbsenceDays(timeBlockDate.EmployeeId, timeBlockDate.Date))
            {
                //Is not generating absence with TimeDeviationCause and ratio. Should use saved ration from TimeBlockDateDetail
                ratio = timeBlockDate.GetTimeBlockDateDetailRatioFromPayrollType(sysPayrollTypeLevel3);
            }

            return ratio.HasValue && ratio.Value < 100;
        }

        private int? CalculateStandbyReplaceProductId(TimeAbsenceRuleHead timeAbsenceRule, TimeAbsenceRuleRow timeAbsenceRuleRow)
        {
            if (timeAbsenceRule == null || timeAbsenceRuleRow == null || !timeAbsenceRule.IsSickDuringStandby)
                return null;

            TimeCode timeCode = GetTimeCodeWithProductsFromCache(timeAbsenceRule.TimeCodeId);
            if (timeCode?.TimeCodePayrollProduct == null || timeCode.TimeCodePayrollProduct.Count != 1)
                return null;

            TimeCodePayrollProduct timeCodePayrollProduct = timeCode.TimeCodePayrollProduct.FirstOrDefault();
            if (timeCodePayrollProduct == null)
                return null;

            return timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts?.FirstOrDefault(i => i.SourcePayrollProductId == timeCodePayrollProduct.ProductId)?.TargetPayrollProductId;
        }

        private int? GetStandbyReplaceProductId(List<ApplyAbsenceDayBase> absenceDays, TimeCodeTransaction timeCodeTransaction)
        {
            if (timeCodeTransaction == null)
                return null;

            var (_, absenceSickIwhStandbyDays) = ApplyAbsenceDayBase.Parse(absenceDays);
            if (absenceSickIwhStandbyDays.IsNullOrEmpty())
                return null;

            int? productId = FindStandbyReplaceProductId(absenceSickIwhStandbyDays, timeCodeTransaction);
            if (!productId.HasValue)
                productId = FindStandbyReplaceProductId(ApplyAbsenceDayBase.Merge(absenceSickIwhStandbyDays), timeCodeTransaction);
            return productId;
        }

        private int? FindStandbyReplaceProductId(List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays, TimeCodeTransaction timeCodeTransaction)
        {
            if (timeCodeTransaction == null)
                return null;

            return absenceSickIwhStandbyDays?.FirstOrDefault(i => i.IsMatch(timeCodeTransaction.TimeCodeId, timeCodeTransaction.Start, timeCodeTransaction.Stop))?.ReplaceRelatedProductId;
        }

        private bool IsHealthyEarningYear(int employeeId, int sysPayrollTypeLevel3, DateTime currentDate, VacationGroupDTO currentVacationGroup)
        {
            if (currentVacationGroup == null)
                return true;

            //Current VacationGroup
            currentVacationGroup.RealDateFrom = currentVacationGroup.CalculateFromDate(currentDate);

            //Earning VacationGroup (must convert to DTO to be able to handle different VacationGroups that really are the same entity)
            VacationGroupDTO earningVacationGroup = GetVacationGroupFromCache(employeeId, currentVacationGroup.GetPrevDay());

            //Check whole earning VacationGroup -14 days (into previous VacationGroup) to earning VacationGroup +14 days (into current VacationGroup)
            DateTime vacationYearDateFromQualifying = earningVacationGroup != null ? earningVacationGroup.RealDateFrom.AddDays(-Constants.VACATION_QUALIFYINGDAYS) : currentVacationGroup.RealDateFrom.AddYears(-1).AddDays(-Constants.VACATION_QUALIFYINGDAYS);
            DateTime vacationYearDateToQualifying = earningVacationGroup != null ? earningVacationGroup.RealDateTo.AddDays(Constants.VACATION_QUALIFYINGDAYS) : currentVacationGroup.RealDateTo.AddYears(-1).AddDays(Constants.VACATION_QUALIFYINGDAYS);

            int healthyDay = 0;
            DateTime currentStopDate = vacationYearDateToQualifying;
            while (currentStopDate >= vacationYearDateFromQualifying)
            {
                if (IsHealthyInQualifyingPeriod(employeeId, sysPayrollTypeLevel3, vacationYearDateFromQualifying, vacationYearDateToQualifying, ref currentStopDate, ref healthyDay))
                    return true;
            }

            return false;
        }

        private bool IsHealthyInQualifyingPeriod(int employeeId, int sysPayrollTypeLevel3, DateTime qualifyingStartdate, DateTime qualifyingStopDate, ref DateTime currentStopDate, ref int healthyDay)
        {
            //Check out or range
            if (currentStopDate < qualifyingStartdate)
                return false;

            //DateTo
            if (currentStopDate > qualifyingStopDate)
                currentStopDate = qualifyingStopDate;

            //DateFrom
            DateTime currentStartDate = currentStopDate.AddDays(-Constants.VACATION_QUALIFYINGDAYS);
            if (currentStartDate < qualifyingStartdate)
                currentStartDate = qualifyingStartdate;

            List<DateTime> sickDays = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, currentStartDate, currentStopDate, sysPayrollTypeLevel3)
                .Select(i => i.TimeBlockDate.Date.Date)
                .Distinct()
                .ToList();

            //Look for sequence without sickness
            DateTime currentDate = currentStopDate;
            while (currentDate >= currentStartDate)
            {
                if (sickDays.Contains(currentDate))
                {
                    healthyDay = 0;
                }
                else
                {
                    healthyDay++;
                    if (healthyDay > Constants.VACATION_QUALIFYINGDAYS)
                        return true;
                }

                currentDate = currentDate.AddDays(-1);
            }

            //Update ref parameter if not healthy, to be able to continue seek
            currentStopDate = currentDate;

            return false;
        }

        private int GetDayOfAbsenceNumber(TimeEngineTemplate template, PayrollProduct payrollProduct, List<TimeAbsenceRuleRow> timeAbsenceRuleRows, TermGroup_TimeAbsenceRuleRowScope scope, out DateTime qualifyingDate, out int checkDaysBack, out int checkDaysForward, bool isAbsenceTotal = false, int nrOfAbsenceDays = 0, EmployeeChild employeeChild = null)
        {
            int dayOfAbsenceNumber = 0;
            qualifyingDate = CalendarUtility.DATETIME_DEFAULT;
            checkDaysBack = 0;
            checkDaysForward = 0;

            if (isAbsenceTotal)
                return nrOfAbsenceDays + 1;

            if (template.HasValidIdentity() && payrollProduct?.SysPayrollTypeLevel3 != null)
            {
                if (scope == TermGroup_TimeAbsenceRuleRowScope.Coherent)
                    dayOfAbsenceNumber = GetDayOfAbsenceNumberCoherent(template.EmployeeId, template.Date, payrollProduct.SysPayrollTypeLevel3.Value, timeAbsenceRuleRows.GetMaxDays(doApplyInfinityAssumption: true), payrollProduct.GetCoherentIntervalDays(), out qualifyingDate, out checkDaysBack, out checkDaysForward, employeeChild);
                else if (scope == TermGroup_TimeAbsenceRuleRowScope.Calendaryear)
                    dayOfAbsenceNumber = GetDayOfAbsenceNumberCalendarYear(template.EmployeeId, template.Date.AddDays(-1), payrollProduct.SysPayrollTypeLevel3.Value, out qualifyingDate);
            }
            return dayOfAbsenceNumber;
        }

        private int GetDayOfAbsenceNumberCoherent(int employeeId, DateTime date, int sysPayrollTypeLevel3, int maxDays, int interval, out DateTime qualifyingDate, out int checkDaysBack, out int checkDaysForward, EmployeeChild employeeChild = null, int? noneScheduleDaysUntilQualifyingDay = null)
        {
            int dayOfAbsenceNumber = 0;
            qualifyingDate = date;
            checkDaysBack = 0;
            checkDaysForward = 0;

            if (maxDays <= 0 || interval <= 0 || sysPayrollTypeLevel3 <= 0)
                return dayOfAbsenceNumber;

            #region Calculate day of Absence

            //When the number of qualifying days of illness is known and passed as parameter, add these days to maxDays (subtracted from the result)
            if (noneScheduleDaysUntilQualifyingDay.HasValue && PayrollRulesUtil.IsAbsenceSickOrWorkInjury(sysPayrollTypeLevel3))
                maxDays += noneScheduleDaysUntilQualifyingDay.Value;

            #region Check back

            DateTime currentDate = date;
            DateTime? foundDate = date;

            while (foundDate.HasValue)
            {
                //Check for TimePayrollTransactions with specified SysPayrollTypeLevel3 for specified Employee
                List<TimePayrollTransaction> timePayrollTransactions = GetTransactions(currentDate.AddDays(-interval), currentDate.AddDays(-1));
                TimePayrollTransaction timePayrollTransaction = timePayrollTransactions.OrderBy(i => i.TimeBlockDate.Date).FirstOrDefault();

                //If a date was found, check again from that date and backwards (interval number of days). Otherwise exit and use previously found date
                foundDate = timePayrollTransaction?.TimeBlockDate?.Date;
                if (foundDate.HasValue)
                    currentDate = foundDate.Value;
                else
                    break;

                //If excess pass maxDays, exit
                if (GetTransactions(currentDate, date.AddDays(-1)).GetNrOfDays() > maxDays && HasDaySchedule(employeeId, currentDate))
                    break;
            }

            qualifyingDate = currentDate;
            dayOfAbsenceNumber = currentDate == date ? 1 : GetTransactions(currentDate, date.AddDays(-1)).GetNrOfDays() + 1;
            checkDaysBack = dayOfAbsenceNumber - 1;

            #endregion

            #region Check forward

            currentDate = date;
            foundDate = date;
            while (foundDate.HasValue)
            {
                //Check for TimePayrollTransactions with specified SysPayrollTypeLevel3 for specified Employee
                var timePayrollTransactions = GetTransactions(currentDate.AddDays(1), currentDate.AddDays(interval));
                var timePayrollTransaction = timePayrollTransactions.OrderByDescending(i => i.TimeBlockDate.Date).FirstOrDefault();

                //If a date was found, check again from that date and forward (interval number of days). Otherwise exit and use previously found date
                foundDate = timePayrollTransaction?.TimeBlockDate?.Date;
                if (foundDate.HasValue && foundDate.Value > currentDate)
                    currentDate = foundDate.Value;
                else
                    break;

                //If excess pass maxDays, exit
                if (GetTransactions(date.AddDays(1), currentDate).GetNrOfDays() + dayOfAbsenceNumber > maxDays)
                    break;
            }

            if (currentDate > date)
                checkDaysForward = (int)currentDate.Subtract(date).TotalDays;

            #endregion

            #endregion

            #region Calculate qualifying day

            currentDate = qualifyingDate;

            if (PayrollRulesUtil.IsAbsenceSickOrWorkInjury(sysPayrollTypeLevel3))
            {
                if (noneScheduleDaysUntilQualifyingDay.HasValue)
                {
                    //When the number of qualifying days of illness is known and passed as parameter, subtract these days from dayOfAbsenceNumber (added to maxDays)
                    dayOfAbsenceNumber -= noneScheduleDaysUntilQualifyingDay.Value;
                    if (dayOfAbsenceNumber <= 0)
                        dayOfAbsenceNumber = 1;
                }
                else
                {
                    //Check if first day(s) is qualifying day
                    noneScheduleDaysUntilQualifyingDay = 0;
                    while (currentDate <= date)
                    {
                        if (noneScheduleDaysUntilQualifyingDay == Constants.SICKNESS_RELAPSEDAYS || HasDaySchedule(employeeId, currentDate))
                            break;

                        noneScheduleDaysUntilQualifyingDay++;
                        currentDate = currentDate.AddDays(1);
                    }

                    if (noneScheduleDaysUntilQualifyingDay.Value > 0 && noneScheduleDaysUntilQualifyingDay.Value < Constants.SICKNESS_RELAPSEDAYS)
                        dayOfAbsenceNumber = GetDayOfAbsenceNumberCoherent(employeeId, date, sysPayrollTypeLevel3, maxDays, interval, out qualifyingDate, out checkDaysForward, out checkDaysForward, noneScheduleDaysUntilQualifyingDay: noneScheduleDaysUntilQualifyingDay);
                }
            }

            #endregion

            List<TimePayrollTransaction> GetTransactions(DateTime dateFrom, DateTime dateTo) =>
                GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, dateFrom, dateTo, sysPayrollTypeLevel3, employeeChild: employeeChild)
                    .Where(t => !t.IsReversed)
                    .ToList();

            return dayOfAbsenceNumber;
        }

        private int GetDayOfAbsenceNumberCalendarYear(int employeeId, DateTime date, int sysPayrollTypeLevel3, out DateTime qualifyingDate)
        {
            List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, CalendarUtility.GetFirstDateOfYear(date), date, sysPayrollTypeLevel3);
            qualifyingDate = timePayrollTransactions.GetFirstDay() ?? CalendarUtility.DATETIME_DEFAULT;
            return timePayrollTransactions.GetNrOfDays() + 1;
        }

        private int? GetDayOfAbsenceNumberInSequence(TimeEngineTemplate template, TimeAbsenceRuleHead absenceRule, int sysPayrollTypeLevel3, TermGroup_TimeAbsenceRuleRowType absenceRuleRowType, bool forceWholeday, EmployeeChild employeeChild = null)
        {
            if (absenceRuleRowType != TermGroup_TimeAbsenceRuleRowType.Unknown && absenceRuleRowType != GetAbsenceRuleRowType(template, absenceRule, sysPayrollTypeLevel3, forceWholeday: forceWholeday))
                return 1;

            var (absenceDayNumberInSequence, _, _) = GetDayOfAbsenceNumberInSequence(template.EmployeeId, template.Date, sysPayrollTypeLevel3, absenceRule.GetMaxDays(absenceRuleRowType, doApplyInfinityAssumption: true), employeeChild: employeeChild);
            return absenceDayNumberInSequence;
        }

        private (DateTime?, DateTime?) GetAbsenceDatesFromSequence(int employeeId, List<RestoreAbsenceDay> absenceDays, int sysPayrollTypeLevel3, int? maxDays = null, DateTime? limitStartDate = null)
        {
            DateTime? startDate = null;
            DateTime? stopDate = null;
            List<DateTime> dates = absenceDays.GetDates(sysPayrollTypeLevel3);
            for (int date = 0; date < dates.Count; date++)
            {
                var (_, absenceStartDate, absenceStopDate) = GetDayOfAbsenceNumberInSequence(employeeId, dates[date], sysPayrollTypeLevel3, maxDays, maxDaysPerDirection: maxDays.HasValue, limitStartDate: limitStartDate);
                if (!startDate.HasValue || startDate > absenceStartDate)
                    startDate = absenceStartDate;
                if (!stopDate.HasValue || stopDate < absenceStopDate)
                    stopDate = absenceStopDate;
            }
            return (startDate, stopDate);
        }

        private (int?, DateTime, DateTime) GetDayOfAbsenceNumberInSequence(int employeeId, DateTime date, int sysPayrollTypeLevel3, int? maxDays = null, bool maxDaysPerDirection = false, EmployeeChild employeeChild = null, DateTime? limitStartDate = null)
        {
            int absenceDayNumberInSequence = 1;
            DateTime absenceStartDate = date;
            DateTime absenceStopDate = date;
            List<DateTime> dates = new List<DateTime>();

            //Loop 2 times. First backwards, then forwards
            for (int i = 1; i <= 2; i++)
            {
                if (maxDaysPerDirection && dates.Any())
                    dates.Clear();

                bool backwards = i == 1;
                DateTime currentDate = date.AddDays(backwards ? -1 : 1);

                bool isCoherentInterval = true;
                while (isCoherentInterval && dates.Count <= maxDays)
                {
                    if (limitStartDate.HasValue && limitStartDate.Value >= currentDate)
                        break;

                    if (HasInitiatedAbsenceDays(employeeId, currentDate, sysPayrollTypeLevel3) || GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, currentDate, currentDate, sysPayrollTypeLevel3, employeeChild: employeeChild).Any())
                    {
                        absenceDayNumberInSequence++;
                        if (backwards)
                            absenceStartDate = currentDate;
                        else
                            absenceStopDate = currentDate;
                        dates.Add(currentDate);
                        currentDate = currentDate.AddDays(backwards ? -1 : 1);
                    }
                    else
                        isCoherentInterval = false;
                }
            }

            return (absenceDayNumberInSequence, absenceStartDate, absenceStopDate);
        }

        private int GetAbsenceElapsedDays(DateTime vacationGroupStart, DateTime currentDate)
        {
            return CalendarUtility.GetBeginningOfDay(currentDate).Subtract(CalendarUtility.GetBeginningOfDay(vacationGroupStart)).Days + 1;
        }

        private int? GetAbsencePayedDays(int employeeId, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null)
        {
            int? payedDays = null;

            switch (sysPayrollTypeLevel3)
            {
                //Arbetsskada (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury:
                    payedDays = Int32.MaxValue;
                    break;
                //Sjuk (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick:
                    payedDays = 180;
                    break;
                //Tillfällig föräldrapenning (120/180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave:
                    payedDays = HasEmployeeAnyChildWithSingelCustody(employeeId) ? 180 : 120;
                    break;
                //Graviditetspenning (Alltid betald)
                case (int)TermGroup_TimeAbsenceRuleType.PregnancyMoney_PAID:
                    payedDays = Int32.MaxValue;
                    break;
                //Föräldraledig (120/180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave:
                    if (employeeChild != null)
                        payedDays = employeeChild.SingelCustody ? 180 : 120;
                    else
                        payedDays = 0;
                    break;
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PregnancyCompensation:
                    payedDays = 270;
                    break;
                //Överföring av smitta (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TransmissionOfInfection:
                    payedDays = 180;
                    break;
                //Facklig utbildning (180)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_UnionEduction:
                    payedDays = 180;
                    break;
                //Totalförsvarsplikt (60)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService:
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService_Total:
                    payedDays = 60;
                    break;
                //Svenska för invandrare (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_SwedishForImmigrants:
                    payedDays = Int32.MaxValue;
                    break;
                //Betald frånvaro (Permittering/Permission) (Alltid betald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PayedAbsence:
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Permission:
                    payedDays = Int32.MaxValue;
                    break;
                //Ledighet utan lön (Tjänstledighet) (Alltid obetald)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence:
                    payedDays = 0;
                    break;
                //Närståendevård (45)
                case (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_RelativeCare:
                    payedDays = 45;
                    break;
            }

            return payedDays;
        }

        #endregion

        #region Break evaluation

        private List<TimeBlock> EvaluateBreaksRules(TimeEngineTemplate template, bool? discardBreakEvaluation = null)
        {
            return EvaluateBreaksRules(template?.EmployeeGroup, template?.TimeBlockDate, template?.Identity?.ScheduleBlocks, template?.Identity?.TimeBlocks, discardBreakEvaluation);
        }

        private List<TimeBlock> EvaluateBreaksRules(EmployeeGroup employeeGroup, TimeBlockDate timeBlockDate, List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks, bool? discardBreakEvaluation = null)
        {
            if (timeBlockDate == null || employeeGroup == null)
                return new List<TimeBlock>();

            #region Split break periods

            DecideTimeBlockTypes(timeBlocks);
            List<EvaluteBreakPeriodDTO> breakPeriods = GetBreakPeriodsToEvaluate(scheduleBlocks, timeBlocks);
            int? currentBreakPeriodKey = GetCurrentBreakPeriodKey(breakPeriods);

            #endregion

            #region Handle DiscardBreakEvaluation

            if (employeeGroup.AlwaysDiscardBreakEvaluation)
            {
                //Setting on EmployeeGroup overrides passed value
                if (timeBlocks.Any(i => i.CalculatedAsAbsence == true) && !timeBlocks.Any(i => i.CalculatedAsPresence == true))
                    discardBreakEvaluation = false;
                else
                    discardBreakEvaluation = true;
            }
            if (!discardBreakEvaluation.HasValue)
            {
                //Get setting from database if not passed
                discardBreakEvaluation = timeBlockDate.DoDiscardedBreakEvaluation(key: currentBreakPeriodKey);
            }

            //Save latest setting on TimeBlockDate
            timeBlockDate.SetDiscardedBreakEvaluation(discardBreakEvaluation.Value, key: currentBreakPeriodKey, forceDetails: breakPeriods.Count > 1);

            #endregion

            #region Evaluate break periods

            List<TimeBlock> evaluatedTimeBlocks = new List<TimeBlock>();
            foreach (EvaluteBreakPeriodDTO breakPeriod in breakPeriods)
            {
                if (timeBlockDate.DoDiscardedBreakEvaluation(breakPeriod.Key))
                    evaluatedTimeBlocks.AddRange(EvaluateDiscardBreakEvaluation(breakPeriod, employeeGroup));
                else
                    evaluatedTimeBlocks.AddRange(EvaluateBreaksRulesForPeriod(breakPeriod, employeeGroup));
            }

            return evaluatedTimeBlocks;

            #endregion
        }

        private List<TimeBlock> EvaluateDiscardBreakEvaluation(EvaluteBreakPeriodDTO breakPeriod, EmployeeGroup employeeGroup)
        {
            if (breakPeriod == null || employeeGroup == null)
                return new List<TimeBlock>();

            List<TimeBlock> evaluatedTimeBlocks = breakPeriod.TimeBlocks;
            if (!evaluatedTimeBlocks.Any(i => i.IsAbsence()))
                return evaluatedTimeBlocks;

            List<TimeScheduleTemplateBlock> scheduleBlocks = breakPeriod.GetSchedule();
            List<TimeScheduleTemplateBlockDTO> scheduleBlockBreaks = GetAllScheduleBreaksForEvaluation(scheduleBlocks).ToDTOs();
            if (scheduleBlockBreaks.IsNullOrEmpty())
                return evaluatedTimeBlocks;

            List<TimeBlock> timeBlockBreaks = evaluatedTimeBlocks.Where(tb => tb.IsBreakOrGeneratedFromBreak).ToList();

            foreach (TimeScheduleTemplateBlockDTO scheduleBlockBreak in scheduleBlockBreaks.OrderBy(i => i.StartTime))
            {
                List<TimeBlock> timeBlockBreaksForScheduleBreak = GetTimeBlockBreakForScheduleBreak(timeBlockBreaks, scheduleBlockBreak.TimeScheduleTemplateBlockId);
                ApplyBreakTimeBlocksDuringAbsence(ref timeBlockBreaksForScheduleBreak, ref evaluatedTimeBlocks, scheduleBlockBreak, scheduleBlockBreaks.Count, out _, addNewTimeblock: true);
            }

            return evaluatedTimeBlocks;
        }

        private List<TimeBlock> EvaluateBreaksRulesForPeriod(EvaluteBreakPeriodDTO breakPeriod, EmployeeGroup employeeGroup)
        {
            if (breakPeriod == null || employeeGroup == null)
                return new List<TimeBlock>();

            List<TimeBlock> timeBlocks = breakPeriod.GetTimeBlocks();
            if (timeBlocks.IsNullOrEmpty())
                return new List<TimeBlock>();

            List<TimeBlock> validTimeBlocks = GetRecreatedTimeBlocks(timeBlocks, employeeGroup);
            List<TimeScheduleTemplateBlock> scheduleBlocks = breakPeriod.GetSchedule();
            List<TimeScheduleTemplateBlock> scheduleBlockBreaks = GetAllScheduleBreaksForEvaluation(scheduleBlocks);
            if (scheduleBlockBreaks.IsNullOrEmpty())
                return validTimeBlocks;

            var (evaluatedTimeBlocks, timeBlockBreaks) = DivideTimeBlocksForBreakEvaluation(validTimeBlocks);
            List<TimeScheduleTemplateBlockDTO> validScheduleBreaks = GetValidScheduleBreaksForEvaluation(breakPeriod.StopTime, scheduleBlocks, scheduleBlockBreaks, validTimeBlocks, timeBlockBreaks, employeeGroup);
            foreach (TimeScheduleTemplateBlockDTO validScheduleBreak in validScheduleBreaks.RemoveDuplicates().OrderBy(i => i.StartTime))
            {
                List<TimeBlock> timeBlockBreaksForScheduleBreak = GetTimeBlockBreakForScheduleBreak(timeBlockBreaks, validScheduleBreak.TimeScheduleTemplateBlockId);
                ApplyBreakTimeBlocksDuringAbsence(ref timeBlockBreaksForScheduleBreak, ref evaluatedTimeBlocks, validScheduleBreak, validScheduleBreaks.Count, out ApplyBreakPaddingSetting paddingRule);
                ApplyBreakRounding(timeBlockBreaksForScheduleBreak, evaluatedTimeBlocks, validScheduleBreak, employeeGroup);

                ApplyBreakDTO dto = new ApplyBreakDTO(validScheduleBreak, scheduleBlocks, timeBlockBreaksForScheduleBreak, employeeGroup, paddingRule);
                ApplyBreakBlockZero(dto, ref timeBlockBreaksForScheduleBreak, ref evaluatedTimeBlocks);
                ApplyBreakRules(dto, ref timeBlockBreaksForScheduleBreak, ref evaluatedTimeBlocks);
            }

            return evaluatedTimeBlocks.OrderBy(i => i.StartTime).ToList();
        }

        private (List<TimeBlock> nonTimeBlockBreaks, List<TimeBlock> breakTimeBlocks) DivideTimeBlocksForBreakEvaluation(List<TimeBlock> timeBlocks)
        {
            List<TimeBlock> nonTimeBlockBreaks = new List<TimeBlock>();
            List<TimeBlock> timeBlockBreaks = new List<TimeBlock>();

            if (!timeBlocks.IsNullOrEmpty())
            {
                foreach (TimeBlock validTimeblock in timeBlocks.Where(timeBlock => timeBlock != null))
                {
                    if (validTimeblock.IsBreakOrGeneratedFromBreak)
                        timeBlockBreaks.Add(validTimeblock);
                    else
                        nonTimeBlockBreaks.Add(validTimeblock);
                }
            }

            return (nonTimeBlockBreaks, timeBlockBreaks);
        }

        #region EvaluteBreakPeriod

        private List<EvaluteBreakPeriodDTO> GetBreakPeriodsToEvaluate(List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks)
        {
            if (!TryGetEvaluateBreakPeriods(out List<EvaluteBreakPeriodDTO> evaluateBreakPeriods, scheduleBlocks, timeBlocks))
                evaluateBreakPeriods = new List<EvaluteBreakPeriodDTO> { new EvaluteBreakPeriodDTO(scheduleBlocks, timeBlocks) };
            return evaluateBreakPeriods;
        }

        private List<EvaluteBreakPeriodDTO> CreateEvaluateBreakPeriods(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (!scheduleBlocks.TrySplitByAccount(true, out Dictionary<int, List<TimeScheduleTemplateBlock>> dict))
                return new List<EvaluteBreakPeriodDTO>();

            List<EvaluteBreakPeriodDTO> evaluateBreakPeriods = new List<EvaluteBreakPeriodDTO>();
            foreach (var pair in dict)
            {
                evaluateBreakPeriods.Add(new EvaluteBreakPeriodDTO(pair.Value, key: pair.Key));
            }
            return evaluateBreakPeriods;
        }

        private EvaluteBreakPeriodDTO DecideEvaluateBreakPeriodForTimeBlock(List<EvaluteBreakPeriodDTO> breakPeriods, TimeBlock timeBlock)
        {
            if (breakPeriods.IsNullOrEmpty() || timeBlock == null)
                return null;

            //First, take the period the timeblock starts in
            EvaluteBreakPeriodDTO breakPeriod = breakPeriods.FirstOrDefault(i => i.StartTime <= timeBlock.StartTime && i.StopTime > timeBlock.StartTime);

            //Second, take the first period if timeblock is before first period
            if (breakPeriod == null && breakPeriods.FirstOrDefault()?.StartTime > timeBlock.StartTime)
                breakPeriod = breakPeriods.FirstOrDefault();

            //Third, take the last period if timeblock is after the last period
            if (breakPeriod == null && breakPeriods.LastOrDefault()?.StopTime < timeBlock.StartTime)
                breakPeriod = breakPeriods.FirstOrDefault();

            //Fourth, take the next period if starts in hole
            if (breakPeriod == null && breakPeriods.Any(i => i.StartTime > timeBlock.StartTime))
                breakPeriod = breakPeriods.FirstOrDefault(i => i.StartTime > timeBlock.StartTime);

            return breakPeriod;
        }

        private bool TryGetEvaluateBreakPeriods(out List<EvaluteBreakPeriodDTO> evaluateBreakPeriods, List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks)
        {
            evaluateBreakPeriods = null;

            if (!UseAccountHierarchy())
                return false;
            if (!GetCompanyBoolSettingFromCache(CompanySettingType.TimeSplitBreakOnAccount))
                return false;

            evaluateBreakPeriods = CreateEvaluateBreakPeriods(scheduleBlocks);
            if (evaluateBreakPeriods.IsNullOrEmpty())
                return false;

            if (!TryDecideEvaluateBreakPeriodForTimeBlocks(evaluateBreakPeriods, timeBlocks))
                return false;

            return true;
        }

        private bool TryDecideEvaluateBreakPeriodForTimeBlocks(List<EvaluteBreakPeriodDTO> evaluateBreakPeriods, List<TimeBlock> timeBlocks)
        {
            if (timeBlocks != null)
            {
                foreach (TimeBlock timeBlock in timeBlocks.OrderBy(i => i.StartTime))
                {
                    EvaluteBreakPeriodDTO breakPeriod = DecideEvaluateBreakPeriodForTimeBlock(evaluateBreakPeriods, timeBlock);
                    if (breakPeriod == null)
                        return false;

                    breakPeriod.AddTimeBlock(timeBlock);
                }
            }

            return true;
        }

        private int? GetCurrentBreakPeriodKey(List<EvaluteBreakPeriodDTO> breakPeriods)
        {
            if (breakPeriods.IsNullOrEmpty())
                return null;

            List<int> keys = breakPeriods.Where(i => i.Key.HasValue).Select(i => i.Key.Value).ToList();
            if (keys.Count <= 1)
                return null;

            Account account = AccountManager.GetAccountHierarchySettingAccount(entities, actorCompanyId: actorCompanyId, userId: userId);
            if (account == null)
                return null;

            return keys.FirstOrDefault(id => id == account.AccountId);
        }

        #endregion

        #region Apply rules

        private void ApplyBreakRounding(List<TimeBlock> timeBlockBreaksForScheduleBreak, List<TimeBlock> evaluatedTimeBlocks, TimeScheduleTemplateBlockDTO validScheduleBreak, EmployeeGroup employeeGroup)
        {
            if (timeBlockBreaksForScheduleBreak.Any() && !employeeGroup.AutogenTimeblocks && (employeeGroup.BreakRoundingUp != 0 || employeeGroup.BreakRoundingDown != 0))
            {
                TimeBlock presenceBreakToAdjust = timeBlockBreaksForScheduleBreak.Last();
                int presenceBreakMinutes = (int)timeBlockBreaksForScheduleBreak.Sum(i => i.StopTime.Subtract(i.StartTime).TotalMinutes);
                if (DoExtendBreak(employeeGroup.BreakRoundingUp, validScheduleBreak.TimeCode.DefaultMinutes, presenceBreakMinutes, out int diffMinutes))
                    ExtendBreak(presenceBreakToAdjust, evaluatedTimeBlocks.Concat(timeBlockBreaksForScheduleBreak).ToList(), diffMinutes);
                else if (DoDecreaseBreak(employeeGroup.BreakRoundingDown, validScheduleBreak.TimeCode.DefaultMinutes, presenceBreakMinutes, out diffMinutes))
                    DecreaseBreak(presenceBreakToAdjust, evaluatedTimeBlocks.Concat(timeBlockBreaksForScheduleBreak).ToList(), diffMinutes);
            }
        }

        private void ApplyBreakBlockZero(ApplyBreakDTO dto, ref List<TimeBlock> timeBlockBreaksForScheduleBreak, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (dto == null || !dto.DoCreateZeroBreak || timeBlockBreaksForScheduleBreak == null)
                return;

            TimeBlock zeroBreak = timeBlockBreaksForScheduleBreak.FirstOrDefault(b => b.StartTime == b.StopTime && b.StartTime == dto.ScheduleBreakDTO.StartTime);
            if (zeroBreak == null)
            {
                zeroBreak = CreateBreakTimeBlock(dto.ScheduleBreakDTO, forceZero: true);
                if (zeroBreak != null)
                    timeBlockBreaksForScheduleBreak.Add(zeroBreak);
            }
            evaluatedTimeBlocks = SplitTimeBlocks(zeroBreak, evaluatedTimeBlocks);
        }

        /// <summary>
        /// Apply break rules for given break TimeBlocks. Also adjust presence TimeBlocks that are affected by the break rules.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="dto">Input DTO</param>
        /// <param name="inputBreakTimeBlocks">The break TimeBlock's to evaluate</param>
        /// <param name="evaluatedTimeBlocks">The non-break TimeBlock's that may be affected by break rules</param>
        /// <returns>Returns the evaluated breaks</returns>
        private void ApplyBreakRules(ApplyBreakDTO dto, ref List<TimeBlock> inputBreakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            ResetTimeCodeForTimeBlocks(inputBreakTimeBlocks, dto.TimeCodeDTO, dto.AutogenTimeblocks);
            ApplyBreakWindowRules(dto, ref inputBreakTimeBlocks);

            List<TimeBlock> breakTimeBlocksInsideWindow = inputBreakTimeBlocks.GetInsideBreakWindow(dto.ScheduleBreakWindowStartTime, dto.ScheduleBreakWindowStopTime);
            List<TimeBlock> breakTimeBlocksOutsideWindow = inputBreakTimeBlocks.GetOutsideBreakWindow(dto.ScheduleBreakWindowStartTime, dto.ScheduleBreakWindowStopTime);
            ApplyBreaksOutsideWindow(dto, ref evaluatedTimeBlocks, breakTimeBlocksOutsideWindow, breakTimeBlocksInsideWindow);

            List<TimeBlock> breakTimeBlocks = MergeConnectedBreakTimeBlocks(dto, breakTimeBlocksInsideWindow.Concat(breakTimeBlocksOutsideWindow).ToList());
            ApplyBreakIntervalRules(dto, ref breakTimeBlocks, ref evaluatedTimeBlocks);
            ApplyBreakPaddingSettings(dto, breakTimeBlocks, ref evaluatedTimeBlocks);

            foreach (TimeBlock breakTimeBlock in breakTimeBlocks)
            {
                if (!evaluatedTimeBlocks.Any(i => i.StartTime == breakTimeBlock.StartTime && i.StopTime == breakTimeBlock.StopTime && i.State == breakTimeBlock.State))
                    evaluatedTimeBlocks.Add(breakTimeBlock);
            }
        }

        /// <summary>
        /// Add presence breaks outisde schedule and recalculates sums
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="resultBreakTimeBlocks"></param>
        /// <param name="breakTimeBlocksOutsideWindow"></param>
        /// <param name="breakTimeBlocksInsideWindow"></param>
        private void ApplyBreaksOutsideWindow(ApplyBreakDTO dto, ref List<TimeBlock> resultBreakTimeBlocks, List<TimeBlock> breakTimeBlocksOutsideWindow, List<TimeBlock> breakTimeBlocksInsideWindow)
        {
            if (breakTimeBlocksOutsideWindow.IsNullOrEmpty() || resultBreakTimeBlocks == null)
                return;

            resultBreakTimeBlocks.AddRange(breakTimeBlocksOutsideWindow);

            dto.PresenceBreakMinutesOutsideWindow = breakTimeBlocksOutsideWindow.Sum(tb => tb.TotalMinutes);
            dto.PresenceBreakMinutes = dto.PresenceBreakMinutesOutsideWindow;
            if (!breakTimeBlocksInsideWindow.IsNullOrEmpty())
                dto.PresenceBreakMinutes += breakTimeBlocksInsideWindow.Sum(tb => tb.TotalMinutes);
        }

        /// <summary>
        /// Apply break rules for TimeBlock's outside window. Splits up TimeBlocks's that are fully or partly outside the break window
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="dto">Input DTO</param>
        /// <param name="breakTimeBlocks">The break TimeBlock's to evaluate</param>
        private void ApplyBreakWindowRules(ApplyBreakDTO dto, ref List<TimeBlock> breakTimeBlocks)
        {
            if (dto == null || breakTimeBlocks == null)
                return;

            if (dto.PresenceBreakStartMinutes < dto.ScheduleBreakWindowStartMinutes && dto.PresenceBreakStartMinutes > 0)
                ApplyBreakWindowRuleEarlierThanStart(dto, ref breakTimeBlocks);
            if (dto.PresenceBreakStopMinutes > dto.ScheduleBreakWindowStopMinutes)
                ApplyBreakWindowRuleLaterThanStop(dto, ref breakTimeBlocks);
        }

        /// <summary>
        /// Apply break rule for TimeBlock's inside window.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="breakTimeBlocks">The break TimeBlock's to evaluate</param>
        /// <param name="limitTime">The limit for the breakwindow. If its start or stop time is determined by isStopBreakPoint</param>
        /// <param name="isBeforeWindow">Determines if the limitTime corresponds to the start or the stop of the break window</param>
        /// <param name="timeCodeRule">The rule to apply</param>
        private void ApplyBreakWindowRuleEarlierThanStart(ApplyBreakDTO dto, ref List<TimeBlock> breakTimeBlocks)
        {
            if (dto == null || breakTimeBlocks.IsNullOrEmpty())
                return;

            TimeCodeRuleDTO timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart);
            if (timeCodeRule == null)
                return;

            List<TimeBlock> splittedTimeBlocks = new List<TimeBlock>();

            breakTimeBlocks = breakTimeBlocks.OrderBy(i => i.StartTime).ToList();
            for (int i = 0; i < breakTimeBlocks.Count; i++)
            {
                TimeBlock breakTimeBlock = breakTimeBlocks[i];

                //Within window
                if (breakTimeBlock.StartTime >= dto.ScheduleBreakWindowStartTime)
                    break;

                int length = breakTimeBlock.GetMinutes();
                if (length > 0)
                {
                    //Update original TimeBlock
                    breakTimeBlock.StopTime = dto.ScheduleBreakWindowStartTime;
                    breakTimeBlock.CalculatedOutsideBreakWindow = true;

                    //Split excess TimeBlock
                    DateTime startTime = breakTimeBlock.StopTime;
                    DateTime stopTime = breakTimeBlock.StopTime.AddMinutes(length);
                    TimeBlock splittedTimeBlock = CreateBreakTimeBlock(breakTimeBlock, startTime, stopTime, timeCodeId: 0, manuallyAdjusted: true);

                    //Add to list so it will be handled by other rule evaluation
                    splittedTimeBlocks.Add(splittedTimeBlock);
                }

                //ReCreate TimeBlock and apply rule
                ReCreateBreakTimeBlock(breakTimeBlocks, i, timeCodeRule, false);
            }

            breakTimeBlocks = AddWindowBreaks(breakTimeBlocks, splittedTimeBlocks);
        }

        private void ApplyBreakWindowRuleLaterThanStop(ApplyBreakDTO dto, ref List<TimeBlock> breakTimeBlocks)
        {
            if (dto == null || breakTimeBlocks.IsNullOrEmpty())
                return;

            TimeCodeRuleDTO timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop);
            if (timeCodeRule == null)
                return;

            List<TimeBlock> splittedTimeBlocks = new List<TimeBlock>();

            breakTimeBlocks = breakTimeBlocks.OrderByDescending(i => i.StopTime).ToList();
            for (int i = 0; i < breakTimeBlocks.Count; i++)
            {
                TimeBlock breakTimeBlock = breakTimeBlocks[i];

                //Within window
                if (breakTimeBlock.StopTime <= dto.ScheduleBreakWindowStopTime)
                    break;

                int length = (int)dto.ScheduleBreakWindowStopTime.Subtract(breakTimeBlock.StartTime).TotalMinutes;
                if (length > 0)
                {
                    //Update original TimeBlock
                    breakTimeBlock.StartTime = dto.ScheduleBreakWindowStopTime;
                    breakTimeBlock.TimeCode.RemoveRange(breakTimeBlock.TimeCode.Where(tc => tc.Type != (int)SoeTimeCodeType.Break));
                    breakTimeBlock.CalculatedOutsideBreakWindow = true;

                    //Split excess TimeBlock
                    DateTime stopTime = breakTimeBlock.StartTime;
                    DateTime startTime = breakTimeBlock.StartTime.AddMinutes(-length);
                    TimeBlock splittedTimeBlock = CreateBreakTimeBlock(breakTimeBlock, startTime, stopTime, timeCodeId: 0, manuallyAdjusted: true);

                    //Add to list so it will be handled by other rule evaluation
                    splittedTimeBlocks.Add(splittedTimeBlock);
                }

                //ReCreate TimeBlock and apply rule
                ReCreateBreakTimeBlock(breakTimeBlocks, i, timeCodeRule, false);
            }

            breakTimeBlocks = AddWindowBreaks(breakTimeBlocks, splittedTimeBlocks);
        }

        private static List<TimeBlock> AddWindowBreaks(List<TimeBlock> breakTimeBlocks, List<TimeBlock> splittedTimeBlocks)
        {
            if (breakTimeBlocks.IsNullOrEmpty())
                return new List<TimeBlock>();
            if (!splittedTimeBlocks.IsNullOrEmpty())
                breakTimeBlocks.AddRange(splittedTimeBlocks);
            return breakTimeBlocks.OrderBy(i => i.StartTime).ToList();
        }

        /// <summary>
        /// Apply break rules for TimeBlock's inside window. Splits up TimeBlock's according to the min, default and max break time values.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="dto">Input DTO</param>
        /// <param name="breakTimeBlocks">The break TimeBlock's to evaluate</param>
        /// <param name="evaluatedTimeBlocks">The non-break TimeBlock's that may be affected by break rules</param>
        private void ApplyBreakIntervalRules(ApplyBreakDTO dto, ref List<TimeBlock> breakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (dto == null || breakTimeBlocks == null || evaluatedTimeBlocks == null)
                return;

            TimeCodeRuleDTO timeCodeRule;
            int timeBlockCounter = 0;
            int handledMinutes = 0;

            dto.CalculatedRuleType = GetTimeCodeRuleType(dto);
            switch (dto.CalculatedRuleType)
            {
                case TermGroup_TimeCodeRuleType.TimeCodeLessThanMin:
                case TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd:
                    #region TimeCodeLessThanMin / TimeCodeBetweenMinAndStd

                    if (dto.PresenceBreakMinutes > 0)
                    {
                        timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd);
                        ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.PresenceBreakMinutes, timeCodeRule);
                    }
                    ApplyBreakPaddingIntervalRules(dto, ref breakTimeBlocks, ref evaluatedTimeBlocks);

                    #endregion
                    break;
                case TermGroup_TimeCodeRuleType.TimeCodeStd:
                    #region TimeCodeStd

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd);
                    AddRuleDepictedTimeCodeToTimeBlocks(breakTimeBlocks, timeCodeRule);

                    #endregion
                    break;
                case TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax:
                    #region TimeCodeBetweenStdAndMax

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd);
                    ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.TimeCodeDTO.DefaultMinutes, timeCodeRule);

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax);
                    ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.TimeCodeDTO.MaxMinutes, timeCodeRule);

                    #endregion
                    break;
                case TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax:
                    #region TimeCodeMoreThanMax

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd);
                    ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.TimeCodeDTO.DefaultMinutes, timeCodeRule);

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax);
                    ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.TimeCodeDTO.MaxMinutes, timeCodeRule);

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax);
                    ApplyBreakIntervalRule(ref breakTimeBlocks, ref timeBlockCounter, ref handledMinutes, dto.PresenceBreakMinutes, timeCodeRule);

                    #endregion
                    break;
                case TermGroup_TimeCodeRuleType.AutogenBreakOnStamping:
                    #region AutogenBreakOnStamping

                    timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd);
                    CreateBreakTimeBlock(dto, dto.TimeCodeDTO.DefaultMinutes, timeCodeRule, ref breakTimeBlocks, ref evaluatedTimeBlocks, false);

                    #endregion
                    break;
            }
        }

        private void ApplyBreakPaddingSettings(ApplyBreakDTO dto, List<TimeBlock> breakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (dto?.PaddingSetting == null)
                return;
            if (dto.CalculatedRuleType != TermGroup_TimeCodeRuleType.TimeCodeLessThanMin && dto.CalculatedRuleType != TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd)
                return;
            if (breakTimeBlocks.IsNullOrEmpty() || evaluatedTimeBlocks.IsNullOrEmpty())
                return;

            TimeBlock originalBreakTimeBlock = breakTimeBlocks.FirstOrDefault(b => b.CalculatedAsExcludeFromPaddingRules == true);
            if (originalBreakTimeBlock == null)
                return;

            List<TimeBlock> paddedBreakTimeBlocks = breakTimeBlocks.Where(b => b.CalculatedAsExcludeFromPaddingRules != true).OrderBy(b => b.StartTime).ToList();
            if (paddedBreakTimeBlocks.IsNullOrEmpty())
                return;

            DateTime paddedBreakStartTime = paddedBreakTimeBlocks.First().StartTime;
            DateTime paddedBreakStopTime = paddedBreakTimeBlocks.Last().StopTime;
            bool hasPaddedForward = originalBreakTimeBlock.StartTime < paddedBreakStartTime;
            bool hasPaddedBackward = !hasPaddedForward;

            if (dto.PaddingSetting.DoMoveForward && hasPaddedForward)
            {
                TimeBlock nonBreakTimeBlock = evaluatedTimeBlocks.GetBasedOnStartTime(paddedBreakStopTime);
                if (nonBreakTimeBlock == null)
                    return;

                nonBreakTimeBlock.UpdateStartTime(originalBreakTimeBlock.StopTime);
                MovePaddingBreaksForward(dto.PaddingSetting, paddedBreakTimeBlocks, ref evaluatedTimeBlocks);
            }
            else if (dto.PaddingSetting.DoMoveForward && hasPaddedBackward)
            {
                TimeBlock nonBreakTimeBlock = evaluatedTimeBlocks.GetBasedOnStopTime(paddedBreakStartTime);
                if (nonBreakTimeBlock == null)
                    return;

                nonBreakTimeBlock.UpdateStopTime(originalBreakTimeBlock.StartTime);
                MovePaddingBreaksForward(dto.PaddingSetting, paddedBreakTimeBlocks, ref evaluatedTimeBlocks);
            }
            else if (dto.PaddingSetting.DoMoveBackward && hasPaddedForward)
            {
                TimeBlock nonBreakTimeBlock = evaluatedTimeBlocks.GetBasedOnStartTime(paddedBreakStopTime);
                if (nonBreakTimeBlock == null)
                    return;

                nonBreakTimeBlock.UpdateStartTime(originalBreakTimeBlock.StopTime);
                MovePaddingBreaksBackward(dto.PaddingSetting, paddedBreakTimeBlocks, ref evaluatedTimeBlocks);
            }
            else if (dto.PaddingSetting.DoMoveBackward && hasPaddedBackward)
            {
                TimeBlock nonBreakTimeBlock = evaluatedTimeBlocks.GetBasedOnStopTime(paddedBreakStartTime);
                if (nonBreakTimeBlock == null)
                    return;

                nonBreakTimeBlock.UpdateStopTime(originalBreakTimeBlock.StartTime);
                MovePaddingBreaksBackward(dto.PaddingSetting, paddedBreakTimeBlocks, ref evaluatedTimeBlocks);
            }
        }

        private void MovePaddingBreaksForward(ApplyBreakPaddingSetting paddingSetting, List<TimeBlock> paddedBreakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (paddingSetting == null || paddedBreakTimeBlocks.IsNullOrEmpty() || evaluatedTimeBlocks.IsNullOrEmpty())
                return;

            DateTime currentStartTime = paddingSetting.SlotStartTime;
            foreach (TimeBlock paddedBreakTimeBlock in paddedBreakTimeBlocks)
            {
                int length = paddedBreakTimeBlock.GetMinutes();
                paddedBreakTimeBlock.StartTime = currentStartTime;
                paddedBreakTimeBlock.StopTime = paddedBreakTimeBlock.StartTime.AddMinutes(length);
                SetModifiedProperties(paddedBreakTimeBlock);

                evaluatedTimeBlocks = RearrangeNewTimeBlockAgainstExisting(paddedBreakTimeBlock, evaluatedTimeBlocks, null, addNewTimeBlock: false, addExistingTimeBlocks: true);
                currentStartTime = paddedBreakTimeBlock.StopTime;
            }
        }

        private void MovePaddingBreaksBackward(ApplyBreakPaddingSetting paddingSetting, List<TimeBlock> paddedBreakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (paddingSetting == null || paddedBreakTimeBlocks.IsNullOrEmpty() || evaluatedTimeBlocks.IsNullOrEmpty())
                return;

            DateTime currentStopTime = paddingSetting.SlotStopTime;
            foreach (TimeBlock paddedBreakTimeBlock in paddedBreakTimeBlocks)
            {
                int length = paddedBreakTimeBlock.GetMinutes();
                paddedBreakTimeBlock.StopTime = currentStopTime;
                paddedBreakTimeBlock.StartTime = paddedBreakTimeBlock.StopTime.AddMinutes(-length);
                SetModifiedProperties(paddedBreakTimeBlock);

                evaluatedTimeBlocks = RearrangeNewTimeBlockAgainstExisting(paddedBreakTimeBlock, evaluatedTimeBlocks, null, addNewTimeBlock: false, addExistingTimeBlocks: true);
                currentStopTime = paddedBreakTimeBlock.StartTime;
            }
        }

        /// <summary>
        /// Apply break rule for TimeBlock's inside window
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="breakTimeBlocks">The break TimeBlock's to evaluate</param>
        /// <param name="timeBlockCounter">Counter that shows which TimeBlock in the breakTimeBlocks collection that are currently being evaluated</param>
        /// <param name="handledMinutes">Counter that shows how many minutes that has been evaluated</param>
        /// <param name="maxMinutes">Counter that determines the max minutes that should be evaluated</param>
        /// <param name="timeCodeRule">The rule to apply</param>
        private void ApplyBreakIntervalRule(ref List<TimeBlock> breakTimeBlocks, ref int timeBlockCounter, ref int handledMinutes, int maxMinutes, TimeCodeRuleDTO timeCodeRule)
        {
            if (timeCodeRule == null)
                return;

            while (handledMinutes < maxMinutes && timeBlockCounter < breakTimeBlocks.Count)
            {
                try
                {
                    TimeBlock breakTimeBlock = breakTimeBlocks[timeBlockCounter];
                    if (breakTimeBlock.State != (int)SoeEntityState.Active)
                        continue;

                    if (breakTimeBlock.CalculatedOutsideBreakWindow == true)
                        continue;

                    AddRuleDepictedTimeCodeToTimeBlock(breakTimeBlock, timeCodeRule);

                    int lengthMinutes = breakTimeBlock.GetMinutes();
                    int remainingMinutes = maxMinutes - handledMinutes;
                    int overflowMinutes = lengthMinutes - remainingMinutes;

                    if (overflowMinutes > 0)
                    {
                        //Update original TimeBlock
                        breakTimeBlock.StopTime = breakTimeBlock.StartTime.AddMinutes(remainingMinutes);

                        //Create excess TimeBlock
                        DateTime startTime = breakTimeBlock.StopTime;
                        DateTime stopTime = breakTimeBlock.StopTime.AddMinutes(overflowMinutes);
                        TimeBlock splittedTimeBlock = CreateBreakTimeBlock(breakTimeBlock, startTime, stopTime, timeCodeId: 0, manuallyAdjusted: true);

                        //Add TimeCodes
                        List<TimeCode> timeCodes = breakTimeBlock.TimeCode.Where(i => i.Type == (int)SoeTimeCodeType.Break).ToList();
                        AddBreakCommonToTimeBlock(splittedTimeBlock, breakTimeBlock, timeCodes, null);

                        //Add to list so it will be handled by other rule evaluation
                        breakTimeBlocks.Insert(timeBlockCounter + 1, splittedTimeBlock);

                        handledMinutes = maxMinutes;
                    }
                    else
                    {
                        handledMinutes += lengthMinutes;
                    }
                }
                finally
                {
                    timeBlockCounter++;
                }
            }
        }

        /// <summary>
        /// Apply break rule and creates padding TimeBlocks to fill out to certain rule (Min and Std)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="dto">Input DTO</param>
        /// <param name="breakTimeBlocks">The break TimeBlock's to evaluate</param>
        /// <param name="evaluatedTimeBlocks">The non-break TimeBlock's that may be affected by break rules</param>
        private void ApplyBreakPaddingIntervalRules(ApplyBreakDTO dto, ref List<TimeBlock> breakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            if (dto == null || breakTimeBlocks == null || evaluatedTimeBlocks.IsNullOrEmpty())
                return;

            TermGroup_TimeCodeRuleType ruleType = GetTimeCodeRuleType(dto);
            if (ruleType != TermGroup_TimeCodeRuleType.TimeCodeLessThanMin && ruleType != TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd)
                return;

            int paddingMinutes = dto.TimeCodeDTO.MinMinutes - dto.PresenceBreakMinutes;
            var (breakTimeBlock, nonBreakTimeBlock) = FindRelatedBreakTimeBlocks(dto, breakTimeBlocks, evaluatedTimeBlocks, dto.ScheduleOut, paddingMinutes);

            if (ruleType == TermGroup_TimeCodeRuleType.TimeCodeLessThanMin)
            {
                #region LessThanMin

                TimeCodeRuleDTO timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin);

                if (!breakTimeBlocks.Any())
                {
                    breakTimeBlock = CreateBreakTimeBlock(dto, dto.TimeCodeDTO.MinMinutes, timeCodeRule, ref breakTimeBlocks, ref evaluatedTimeBlocks, true);
                    if (breakTimeBlock != null)
                        nonBreakTimeBlock = evaluatedTimeBlocks.FirstOrDefault(i => i.StartTime == breakTimeBlock.StopTime);
                }
                else if (paddingMinutes > 0)
                {
                    TimeBlock breakPaddingTimeBlock = ApplyBreakPaddingIntervalRule(nonBreakTimeBlock, breakTimeBlock, paddingMinutes, timeCodeRule);
                    if (breakPaddingTimeBlock != null)
                    {
                        breakTimeBlocks.Add(breakPaddingTimeBlock);
                        breakTimeBlock = breakPaddingTimeBlock;
                    }
                }

                #endregion
            }

            #region BetweenMinAndStd

            paddingMinutes = ruleType == TermGroup_TimeCodeRuleType.TimeCodeLessThanMin ? (dto.TimeCodeDTO.DefaultMinutes - dto.TimeCodeDTO.MinMinutes) : (dto.TimeCodeDTO.DefaultMinutes - dto.PresenceBreakMinutes);
            if (paddingMinutes > 0)
            {
                TimeCodeRuleDTO timeCodeRule = dto.TimeCodeDTO.GetTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd);
                TimeBlock breakPaddingTimeBlock = ApplyBreakPaddingIntervalRule(nonBreakTimeBlock, breakTimeBlock, paddingMinutes, timeCodeRule);
                if (breakPaddingTimeBlock != null)
                    breakTimeBlocks.Add(breakPaddingTimeBlock);
            }

            #endregion

            FixOverlappingTimes(ref breakTimeBlocks, ref evaluatedTimeBlocks);
        }

        /// <summary>
        /// Apply break rule and creates padding TimeBlock to fill out to certain rule (Min and Std)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="nonBreakTimeBlock">The nearest non-break TimeBlock</param>
        /// <param name="breakTimeBlock">The nearest break Timeblock</param>
        /// <param name="paddingMinutes">Minutes to create padding TimeBlock for</param>
        /// <param name="timeCodeRule">The rule to apply</param>
        /// <returns>The created padding TimeBlock</returns>
        private TimeBlock ApplyBreakPaddingIntervalRule(TimeBlock nonBreakTimeBlock, TimeBlock breakTimeBlock, int paddingMinutes, TimeCodeRuleDTO timeCodeRule)
        {
            if (nonBreakTimeBlock == null || breakTimeBlock == null)
                return null;

            TimeBlock breakPaddingTimeBlock = null;

            bool paddingBefore = breakTimeBlock.StartTime > nonBreakTimeBlock.StartTime;
            if (paddingBefore)
            {
                #region Padding before

                //Create padding TimeBlock
                DateTime stopTime = breakTimeBlock.StartTime;
                DateTime startTime = breakTimeBlock.StartTime.AddMinutes(-paddingMinutes);
                breakPaddingTimeBlock = CreateBreakTimeBlock(breakTimeBlock, startTime, stopTime, timeCodeId: 0, manuallyAdjusted: true);

                //Update non-break TimeBlock
                if (nonBreakTimeBlock.StopTime > breakPaddingTimeBlock.StartTime)
                    nonBreakTimeBlock.StopTime = breakPaddingTimeBlock.StartTime;

                #endregion
            }
            else
            {
                #region Padding after

                //Create padding TimeBlock
                DateTime startTime = breakTimeBlock.StopTime;
                DateTime stopTime = breakTimeBlock.StopTime.AddMinutes(paddingMinutes);
                breakPaddingTimeBlock = CreateBreakTimeBlock(breakTimeBlock, startTime, stopTime, timeCodeId: 0, manuallyAdjusted: true);

                //Update non-break TimeBlock
                if (nonBreakTimeBlock.StartTime < breakPaddingTimeBlock.StopTime)
                {
                    if (breakPaddingTimeBlock.StopTime > nonBreakTimeBlock.StopTime)
                        nonBreakTimeBlock.StartTime = nonBreakTimeBlock.StopTime;
                    else
                        nonBreakTimeBlock.StartTime = breakPaddingTimeBlock.StopTime;
                    if (nonBreakTimeBlock.StartTime == nonBreakTimeBlock.StopTime)
                        ChangeEntityState(nonBreakTimeBlock, SoeEntityState.Deleted);
                }

                #endregion
            }

            #region Common

            FixTimeBlockTimes(nonBreakTimeBlock);

            //Add TimeCodes
            List<TimeCode> timeCodes = breakTimeBlock.TimeCode.Where(i => i.Type == (int)SoeTimeCodeType.Break).ToList();
            AddBreakCommonToTimeBlock(breakPaddingTimeBlock, breakTimeBlock, timeCodes, null);

            //Apply rule
            AddRuleDepictedTimeCodeToTimeBlock(breakPaddingTimeBlock, timeCodeRule);

            #endregion

            return breakPaddingTimeBlock;
        }

        #endregion

        #region Help-methods

        private List<TimeScheduleTemplateBlock> GetAllScheduleBreaksForEvaluation(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            List<TimeScheduleTemplateBlock> scheduleBlockBreaks = new List<TimeScheduleTemplateBlock>();

            foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
            {
                if (!scheduleBlock.TimeCodeReference.IsLoaded)
                    scheduleBlock.TimeCodeReference.Load();
                if (scheduleBlock.TimeCode == null || !scheduleBlock.TimeCode.IsBreak())
                    continue;

                if (scheduleBlock.TimeCode is TimeCodeBreak timeCodeBreak)
                {
                    if (!timeCodeBreak.TimeCodeRule.IsLoaded)
                        timeCodeBreak.TimeCodeRule.Load();

                    scheduleBlockBreaks.Add(scheduleBlock);
                }
            }

            return scheduleBlockBreaks.OrderBy(i => i.StartTime).ToList();
        }

        private List<TimeScheduleTemplateBlockDTO> GetValidScheduleBreaksForEvaluation(DateTime scheduleOut, List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeScheduleTemplateBlock> scheduleBlockBreaks, List<TimeBlock> timeBlocks, List<TimeBlock> timeBlockBreaks, EmployeeGroup employeeGroup)
        {
            List<TimeScheduleTemplateBlockDTO> validScheduleBreaks = new List<TimeScheduleTemplateBlockDTO>();
            if (employeeGroup != null && employeeGroup.MergeScheduleBreaksOnDay && !employeeGroup.AutogenTimeblocks && !timeBlockBreaks.IsNullOrEmpty())
                validScheduleBreaks.AddRange(MergeTemplateBlockBreaks(scheduleOut, scheduleBlocks, scheduleBlockBreaks, timeBlocks, timeBlockBreaks)); //Use last schedule break (only stamping)
            else
                validScheduleBreaks.AddRange(scheduleBlockBreaks.ToDTOs()); //Use all schedule breaks
            return validScheduleBreaks;
        }

        private List<TimeScheduleTemplateBlockDTO> MergeTemplateBlockBreaks(DateTime scheduleOut, List<TimeScheduleTemplateBlock> templateBlocks, List<TimeScheduleTemplateBlock> templateBlockBreaks, List<TimeBlock> timeBlocks, List<TimeBlock> timeBlockBreaks)
        {
            if (templateBlocks.IsNullOrEmpty() || templateBlockBreaks.IsNullOrEmpty())
                return null;

            List<TimeScheduleTemplateBlockDTO> validTemplateBlockBreaks = new List<TimeScheduleTemplateBlockDTO>();
            TimeScheduleTemplateBlockDTO mergedTemplateBlockBreak = null;
            List<TimeBlock> absenceTimeBlocks = timeBlocks.Where(i => i.IsAbsence() && !i.IsBreak()).ToList();

            for (int i = templateBlockBreaks.Count - 1; i >= 0; i--)
            {
                TimeScheduleTemplateBlock templateBlock = templateBlockBreaks[i];

                if (templateBlock.IsBreakSurroundedByAbsence(templateBlocks) || templateBlock.IsScheduleBlockSurroundedByAbsenceTimeBlock(absenceTimeBlocks))
                {
                    //Add breaks that are surrounded by absence as they are
                    validTemplateBlockBreaks.Add(templateBlock.ToDTO());
                }
                else
                {
                    if (mergedTemplateBlockBreak == null)
                    {
                        //Use last break as template as base
                        mergedTemplateBlockBreak = templateBlock.ToDTO();
                        validTemplateBlockBreaks.Add(mergedTemplateBlockBreak);
                    }
                    else
                    {
                        if (templateBlock.TimeCode is TimeCodeBreak timeCodeBreak)
                        {
                            mergedTemplateBlockBreak.TimeCode.MinMinutes += timeCodeBreak.MinMinutes;
                            mergedTemplateBlockBreak.TimeCode.DefaultMinutes += timeCodeBreak.DefaultMinutes;
                            mergedTemplateBlockBreak.TimeCode.MaxMinutes += timeCodeBreak.MaxMinutes;
                            mergedTemplateBlockBreak.StopTime = mergedTemplateBlockBreak.StopTime.AddMinutes(timeCodeBreak.DefaultMinutes);
                            if (mergedTemplateBlockBreak.StopTime > scheduleOut)
                            {
                                double overflowMinutes = mergedTemplateBlockBreak.StopTime.Subtract(scheduleOut).TotalMinutes;
                                mergedTemplateBlockBreak.StartTime = mergedTemplateBlockBreak.StartTime.AddMinutes(-overflowMinutes);
                                mergedTemplateBlockBreak.StopTime = mergedTemplateBlockBreak.StopTime.AddMinutes(-overflowMinutes);
                            }

                            List<TimeBlock> timeBlockBreaksForTemplateBlockBreak = GetTimeBlockBreakForScheduleBreak(timeBlockBreaks, templateBlock.TimeScheduleTemplateBlockId);
                            foreach (TimeBlock timeBlock in timeBlockBreaksForTemplateBlockBreak)
                            {
                                //Redirect to point to merged break
                                timeBlock.TimeScheduleTemplateBlockBreakId = mergedTemplateBlockBreak.TimeScheduleTemplateBlockId;
                            }
                        }
                    }
                }
            }

            return validTemplateBlockBreaks;
        }

        private List<TimeBlock> GetRecreatedTimeBlocks(List<TimeBlock> timeBlocks, EmployeeGroup employeeGroup)
        {
            if (timeBlocks.IsNullOrEmpty())
                return new List<TimeBlock>();

            List<TimeBlock> recreatedTimeBlocks = new List<TimeBlock>();

            timeBlocks = timeBlocks.OrderBy(i => i.StartTime).ToList();
            for (int i = 0; i < timeBlocks.Count; i++)
            {
                TimeBlock originalTimeBlock = timeBlocks[i];

                //Preserve for example + Flex generated earlier for deviation
                bool doSkipIsBreak = employeeGroup.AutogenTimeblocks && originalTimeBlock.IsGeneratedFromBreakButNotBreak;
                if (!doSkipIsBreak)
                    originalTimeBlock.IsBreak = originalTimeBlock.IsBreak();

                //Must be done after checked if break above, because TimeCode that are used to determine breaks are cleaned here
                if (DoTimeBlockNeedsToBeRecreated(originalTimeBlock))
                    originalTimeBlock = ReCreateGeneratedTimeBlock(originalTimeBlock);

                recreatedTimeBlocks.Add(originalTimeBlock);
            }

            return recreatedTimeBlocks;
        }

        private List<TimeBlock> MergeConnectedBreakTimeBlocks(ApplyBreakDTO dto, List<TimeBlock> breakTimeBlocks)
        {
            if (breakTimeBlocks.IsNullOrEmpty())
                return new List<TimeBlock>();
            if (breakTimeBlocks.Count == 1 && breakTimeBlocks[0].StartTime == breakTimeBlocks[0].StopTime)
                return breakTimeBlocks;

            List<TimeBlock> mergedTimeBlocks = new List<TimeBlock>();
            bool hasMorePresenceBreakThanSchedule = dto.PresenceBreakMinutes > dto.ScheduleBreakDTO.TimeCode.DefaultMinutes && !breakTimeBlocks.Any(tb => tb.CalculatedOutsideBreakWindow == true);

            foreach (TimeBlock breakTimeBlock in breakTimeBlocks.OrderBy(i => i.StartTime).ToList())
            {
                if (mergedTimeBlocks.Any())
                {
                    //Connected breaks - merge
                    TimeBlock lastTimeBlock = mergedTimeBlocks.LastOrDefault(i => i.State == (int)SoeEntityState.Active);
                    if (lastTimeBlock != null && lastTimeBlock.StopTime == breakTimeBlock.StartTime && (hasMorePresenceBreakThanSchedule || IsTimeCodesEqual(lastTimeBlock.TimeCode, breakTimeBlock.TimeCode)))
                    {
                        //Update merged TimeBlock
                        lastTimeBlock.StopTime = breakTimeBlock.StopTime;

                        //Delete original TimeBlock
                        SetTimeBlockAndTransactionsToDeleted(breakTimeBlock, saveChanges: false);
                        mergedTimeBlocks.Add(breakTimeBlock);
                        continue;
                    }
                }

                TimeBlock createdBreakTimeBlock = ReCreateBreakTimeBlockIfNeeded(breakTimeBlock, dto.TimeCodeDTO.TimeCodeId, false, out bool created);
                if (createdBreakTimeBlock != null)
                {
                    mergedTimeBlocks.Add(createdBreakTimeBlock);
                    if (created)
                    {
                        createdBreakTimeBlock.CalculatedAsExcludeFromPaddingRules = breakTimeBlock.CalculatedAsExcludeFromPaddingRules;
                        SetTimeBlockAndTransactionsToDeleted(breakTimeBlock, saveChanges: false);
                    }
                }
            }

            //Return sorted list
            return mergedTimeBlocks.OrderBy(i => i.StartTime).ToList();
        }

        private (DateTime, DateTime) GetTimeCodeBreakWindow(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut)
        {
            int scheduleInMinutes = CalendarUtility.TimeToMinutes(scheduleIn);
            int scheduleOutMinutes = CalendarUtility.TimeToMinutes(scheduleOut);
            int scheduleBreakWindowStartMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StartType, timeCodeBreak.StartTimeMinutes, scheduleInMinutes, scheduleOutMinutes);
            int scheduleBreakWindowStopMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StopType, timeCodeBreak.StopTimeMinutes, scheduleInMinutes, scheduleOutMinutes);
            DateTime scheduleBreakWindowStartTime = CalendarUtility.GetDateFromMinutes(CalendarUtility.DATETIME_DEFAULT, scheduleBreakWindowStartMinutes);
            DateTime scheduleBreakWindowStopTime = CalendarUtility.GetDateFromMinutes(CalendarUtility.DATETIME_DEFAULT, scheduleBreakWindowStopMinutes);
            return (scheduleBreakWindowStartTime, scheduleBreakWindowStopTime);
        }

        private TimeCode GetRuleDepictedTimeCode(TimeCodeRuleDTO timeCodeRule)
        {
            return timeCodeRule != null ? GetTimeCodeFromCache(timeCodeRule.Value) : null;
        }

        private TermGroup_TimeCodeRuleType GetTimeCodeRuleType(ApplyBreakDTO dto)
        {
            TermGroup_TimeCodeRuleType type = TermGroup_TimeCodeRuleType.Unknown;
            if (dto == null)
                return type;

            if (dto.AutogenBreakOnStamping && dto.PresenceBreakMinutes == 0)
            {
                type = TermGroup_TimeCodeRuleType.AutogenBreakOnStamping;
            }
            else
            {
                if (dto.PresenceBreakMinutes < dto.TimeCodeDTO.MinMinutes)
                    type = TermGroup_TimeCodeRuleType.TimeCodeLessThanMin;
                else if (dto.PresenceBreakMinutes < dto.TimeCodeDTO.DefaultMinutes && dto.PresenceBreakMinutes >= dto.TimeCodeDTO.MinMinutes)
                    type = TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd;
                else if (dto.PresenceBreakMinutes == dto.TimeCodeDTO.DefaultMinutes)
                    type = TermGroup_TimeCodeRuleType.TimeCodeStd;
                else if (dto.PresenceBreakMinutes > dto.TimeCodeDTO.DefaultMinutes && dto.PresenceBreakMinutes <= dto.TimeCodeDTO.MaxMinutes)
                    type = TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax;
                else if (dto.PresenceBreakMinutes > dto.TimeCodeDTO.MaxMinutes)
                    type = TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax;
            }
            return type;
        }

        private void ApplyBreakTimeBlocksDuringAbsence(ref List<TimeBlock> presenceBreaks, ref List<TimeBlock> evaluatedTimeBlocks, TimeScheduleTemplateBlockDTO scheduleBreak, int nrOfScheduleBreaks, out ApplyBreakPaddingSetting paddingSetting, bool addNewTimeblock = false)
        {
            paddingSetting = null;

            TimeBlock overlappingAbsenceTimeBlock = scheduleBreak.GetAbsenceTimeBlockOverlappingScheduleBlock(evaluatedTimeBlocks.Where(i => i.IsAbsence()));
            if (overlappingAbsenceTimeBlock != null)
            {
                //Add break during schedule if no breaks at all, or one scheduled break and it is during absence and the break taken is before or after absence.
                bool addBreakDuringAbsence = presenceBreaks.IsNullOrEmpty() || (nrOfScheduleBreaks == 1 && presenceBreaks.All(pb => 0 == CalendarUtility.GetOverlappingMinutes(scheduleBreak.StartTime, scheduleBreak.StopTime, pb.StartTime, pb.StopTime)));
                if (addBreakDuringAbsence)
                {
                    TimeBlock presenceBreak = CreateBreakTimeBlock(scheduleBreak, length: scheduleBreak.GetMinutes() - presenceBreaks.Sum(pb => pb.GetMinutes()));
                    if (presenceBreak != null)
                    {
                        presenceBreak.PayrollImportEmployeeTransactionId = overlappingAbsenceTimeBlock.PayrollImportEmployeeTransactionId;
                        presenceBreaks.Add(presenceBreak);
                        evaluatedTimeBlocks = RearrangeNewTimeBlockAgainstExisting(presenceBreak, evaluatedTimeBlocks, null, addNewTimeBlock: addNewTimeblock, addExistingTimeBlocks: true);
                    }
                }
                else if (presenceBreaks.IsNoneOverlapping(scheduleBreak.StartTime, scheduleBreak.StopTime))
                {
                    presenceBreaks.ForEach(b => b.CalculatedAsExcludeFromPaddingRules = true);
                    paddingSetting = new ApplyBreakPaddingSetting(scheduleBreak.StartTime, scheduleBreak.StopTime, presenceBreaks.Min(b => b.StopTime) <= scheduleBreak.StartTime);
                }
            }
        }

        private void AddRuleDepictedTimeCodeToTimeBlocks(List<TimeBlock> timeBlocks, TimeCodeRuleDTO timeCodeRule)
        {
            if (timeBlocks.IsNullOrEmpty() || timeCodeRule == null)
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                AddRuleDepictedTimeCodeToTimeBlock(timeBlock, timeCodeRule);
            }
        }

        private void AddRuleDepictedTimeCodeToTimeBlock(TimeBlock timeBlock, TimeCodeRuleDTO timeCodeRule)
        {
            if (timeBlock == null || timeBlock.TimeCode == null || timeCodeRule == null)
                return;

            AddTimeCodeToTimeBlock(timeBlock, GetRuleDepictedTimeCode(timeCodeRule));
        }

        private void ResetTimeCodeForTimeBlocks(List<TimeBlock> breakTimeBlocks, TimeCodeDTO timeCode, bool autogenTimeblocks)
        {
            if (breakTimeBlocks.IsNullOrEmpty())
                return;

            foreach (TimeBlock breakTimeBlock in breakTimeBlocks.Where(breakTimeBlock => breakTimeBlock != null))
            {
                bool doSkipClear = autogenTimeblocks && breakTimeBlock.IsGeneratedFromBreakButNotBreak;
                if (!doSkipClear)
                {
                    if (breakTimeBlock.TimeCode == null)
                        breakTimeBlock.TimeCode = new EntityCollection<TimeCode>();
                    else
                        breakTimeBlock.TimeCode.Clear();
                }

                AddTimeCodeToTimeBlock(breakTimeBlock, timeCode);
            }
        }

        private void AddTimeCodeToTimeBlock(TimeBlock timeBlock, TimeCodeDTO timeCode)
        {
            if (timeBlock == null || timeBlock.TimeCode == null || timeCode == null)
                return;

            AddTimeCodeToTimeBlock(timeBlock, GetTimeCodeFromCache(timeCode.TimeCodeId));
        }

        private (TimeBlock breakTimeBlock, TimeBlock nonBreakTimeBlock) FindRelatedBreakTimeBlocks(ApplyBreakDTO dto, List<TimeBlock> breakTimeBlocks, List<TimeBlock> timeBlocks, DateTime scheduleOut, int paddingMinutes)
        {
            TimeBlock breakTimeBlock = null;
            TimeBlock nonBreakTimeBlock = null;

            if (breakTimeBlocks.Any())
            {
                if (breakTimeBlocks.Count == 1)
                {
                    breakTimeBlock = breakTimeBlocks.First();

                    //Check if break is connected to start or stop of schedule
                    if (breakTimeBlock.StartTime == dto.ScheduleBreakDTO.StartTime)
                        nonBreakTimeBlock = timeBlocks.FirstOrDefault(i => i.StartTime == breakTimeBlock.StopTime);
                    else if (breakTimeBlock.StopTime == dto.ScheduleBreakDTO.StopTime)
                        nonBreakTimeBlock = timeBlocks.FirstOrDefault(i => i.StopTime == breakTimeBlock.StartTime);
                }
                else
                {
                    breakTimeBlock = breakTimeBlocks.Last();
                }

                //Non-break - NOT connected to start or stop of schedule
                if (nonBreakTimeBlock == null)
                {
                    //Multiple breaks or break dont start or stops at schedule. Take first non-break TimeBlock after break
                    if (paddingMinutes < 0)
                        paddingMinutes = (int)NumberUtility.TurnAmount(paddingMinutes);

                    nonBreakTimeBlock = FindTimeBlockAfterBreak(timeBlocks, breakTimeBlock);
                    if (nonBreakTimeBlock != null && nonBreakTimeBlock.StartTime.AddMinutes(paddingMinutes) > scheduleOut)
                    {
                        nonBreakTimeBlock = FindTimeBlockBeforeBreak(timeBlocks, paddingMinutes, breakTimeBlock) ?? nonBreakTimeBlock;
                    }
                    else if (nonBreakTimeBlock == null)
                    {
                        nonBreakTimeBlock = timeBlocks.LastOrDefault();
                        if (nonBreakTimeBlock != null && nonBreakTimeBlock.StopTime.AddMinutes(paddingMinutes) > scheduleOut)
                            nonBreakTimeBlock = FindTimeBlockBeforeBreak(timeBlocks, paddingMinutes, breakTimeBlock) ?? nonBreakTimeBlock;
                    }
                }
            }

            return (breakTimeBlock, nonBreakTimeBlock);
        }

        private static TimeBlock FindTimeBlockAfterBreak(List<TimeBlock> timeBlocks, TimeBlock breakTimeBlock)
        {
            return timeBlocks?.FirstOrDefault(i => i.StartTime >= breakTimeBlock.StopTime);
        }

        private static TimeBlock FindTimeBlockBeforeBreak(List<TimeBlock> timeBlocks, int paddingMinutes, TimeBlock breakTimeBlock)
        {
            return timeBlocks?.FirstOrDefault(i => i.StopTime == breakTimeBlock.StartTime && i.TotalMinutes > paddingMinutes);
        }

        private void FixOverlappingTimes(ref List<TimeBlock> breakTimeBlocks, ref List<TimeBlock> evaluatedTimeBlocks)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();
            timeBlocks.AddRange(evaluatedTimeBlocks);
            timeBlocks.AddRange(breakTimeBlocks);

            DateTime? lastStopTime = null;
            foreach (TimeBlock timeBlock in timeBlocks.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime))
            {
                if (lastStopTime.HasValue && timeBlock.StartTime < lastStopTime.Value)
                {
                    timeBlock.StartTime = lastStopTime.Value;
                    FixTimeBlockTimes(timeBlock);
                }

                lastStopTime = timeBlock.StopTime;
            }
        }

        private void SetBreakTime(TimeScheduleTemplateBlock templateBlockBreak, DateTime breakStartTime, DateTime actualDate, int breakMinutes)
        {
            if (templateBlockBreak == null)
                return;

            templateBlockBreak.StartTime = CalendarUtility.GetDateTime(breakStartTime);
            templateBlockBreak.StopTime = templateBlockBreak.StartTime.AddMinutes(breakMinutes);

            // Handle midnight
            int diffDays = 0;
            if (breakStartTime.Date < actualDate)
                diffDays = -1;
            else if (breakStartTime.Date > actualDate)
                diffDays = 1;

            templateBlockBreak.StartTime = templateBlockBreak.StartTime.AddDays(diffDays);
            templateBlockBreak.StopTime = templateBlockBreak.StopTime.AddDays(diffDays);
        }

        private void ParseBreak(TimeScheduleTemplateBlock templateBlockBreak, List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (templateBlockBreak == null)
                return;

            ParseBreak(templateBlockBreak.TimeCodeId, templateBlocks.GetScheduleIn(), templateBlocks.GetScheduleOut(), out DateTime breakstartTime, out DateTime breakStopTime);
            templateBlockBreak.StartTime = breakstartTime;
            templateBlockBreak.StopTime = breakStopTime;
        }

        private void ParseBreak(TimeScheduleTemplateBlockDTO templateBlockBreak, List<TimeScheduleTemplateBlockDTO> templateBlocks)
        {
            if (templateBlockBreak == null)
                return;

            ParseBreak(templateBlockBreak.TimeCodeId, templateBlocks.GetScheduleIn(), templateBlocks.GetScheduleOut(), out DateTime breakstartTime, out DateTime breakStopTime);
            templateBlockBreak.StartTime = breakstartTime;
            templateBlockBreak.StopTime = breakStopTime;
        }

        private void ParseBreak(int timeCodeId, DateTime scheduleIn, DateTime scheduleOut, out DateTime breakStartTime, out DateTime breakStopTime)
        {
            TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(timeCodeId);
            ParseBreak(timeCodeBreak, scheduleIn, scheduleOut, out breakStartTime, out breakStopTime);
        }

        private void ParseBreak(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut, out DateTime breakStartTime, out DateTime breakStopTime)
        {
            //Get break time
            ParseBreakTime(timeCodeBreak, scheduleIn, scheduleOut, out breakStartTime, out breakStopTime);

            bool isBreakTimeDuringSchedule = IsBreakTimeDuringSchedule(scheduleIn, scheduleOut, breakStartTime, breakStopTime);
            if (!isBreakTimeDuringSchedule)
            {
                //Check midnight. Try one more time but this time add 24h to handle midnight
                ParseBreakTime(timeCodeBreak, scheduleIn, scheduleOut, out breakStartTime, out breakStopTime, true);
            }

            isBreakTimeDuringSchedule = IsBreakTimeDuringSchedule(scheduleIn, scheduleOut, breakStartTime, breakStopTime);
            if (!isBreakTimeDuringSchedule)
            {
                TimeSpan length = breakStopTime.Subtract(breakStartTime);
                if (breakStartTime < scheduleIn)
                {
                    //Break before schedule in
                    breakStartTime = scheduleIn;
                    breakStopTime = scheduleIn.Add(length);
                }
                else if (breakStopTime > scheduleOut)
                {
                    //Break after schedule out
                    breakStopTime = scheduleOut;
                    breakStartTime = breakStopTime.Subtract(length);
                }
            }
        }

        private void ParseBreakTime(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut, out DateTime startTime, out DateTime stopTime, bool breakWindowStartAfterMidnight = false)
        {
            startTime = CalendarUtility.DATETIME_DEFAULT;
            stopTime = CalendarUtility.DATETIME_DEFAULT;

            if (timeCodeBreak == null || scheduleIn == scheduleOut)
                return;

            //Schedule, minutes from midnight
            int scheduleInMinutes = CalendarUtility.TimeToMinutes(scheduleIn);
            int scheduleOutMinutes = CalendarUtility.TimeToMinutes(scheduleOut);

            //Break window, minutes from midnight
            int breakWindowStartTimeMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StartType, timeCodeBreak.StartTimeMinutes, scheduleInMinutes, scheduleOutMinutes);
            int breakWindowStopTimeMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StopType, timeCodeBreak.StopTimeMinutes, scheduleInMinutes, scheduleOutMinutes);

            //If stop is less than start we assume that the breakwindow spans over midnight
            bool spansOverMidnight = breakWindowStopTimeMinutes < breakWindowStartTimeMinutes;
            if (spansOverMidnight)
                breakWindowStopTimeMinutes += CalendarUtility.GetOneDayInMinutes();

            //Check midnight
            if (breakWindowStartAfterMidnight)
            {
                //Add 24hours
                breakWindowStartTimeMinutes += CalendarUtility.GetOneDayInMinutes();
                breakWindowStopTimeMinutes += CalendarUtility.GetOneDayInMinutes();
            }

            bool placedAfterStartTime = TryPlaceBreakAfterStartTime(timeCodeBreak, breakWindowStartTimeMinutes, breakWindowStopTimeMinutes, out startTime, out stopTime, breakWindowStartAfterMidnight);
            if (!placedAfterStartTime)
                CalendarUtility.GetTimeInMiddle(timeCodeBreak.DefaultMinutes, breakWindowStartTimeMinutes, breakWindowStopTimeMinutes, out startTime, out stopTime);
        }

        private bool DoExtendBreak(int roundingUpMinutes, int scheduleBreakMinutes, int presenceBreakMinutes, out int diffMinutes)
        {
            diffMinutes = roundingUpMinutes != 0 ? scheduleBreakMinutes - presenceBreakMinutes : 0;
            return diffMinutes > 0 && diffMinutes <= roundingUpMinutes;
        }

        private bool DoDecreaseBreak(int roundingDownMinutes, int scheduleBreakMinutes, int presenceBreakMinutes, out int diffMinutes)
        {
            diffMinutes = roundingDownMinutes != 0 ? presenceBreakMinutes - scheduleBreakMinutes : 0;
            return diffMinutes > 0 && diffMinutes <= roundingDownMinutes;
        }

        private void ExtendBreak(TimeBlock breakTimeBlock, List<TimeBlock> timeBlocks, int minutes)
        {
            ModifyBreak(breakTimeBlock, timeBlocks, minutes, decrease: false);
        }

        private void DecreaseBreak(TimeBlock breakTimeBlock, List<TimeBlock> timeBlocks, int minutes)
        {
            ModifyBreak(breakTimeBlock, timeBlocks, minutes, decrease: true);
        }

        private void ModifyBreak(TimeBlock breakTimeBlock, List<TimeBlock> timeBlocks, int minutes, bool decrease = false)
        {
            if (breakTimeBlock == null)
                return;

            bool extend = !decrease;
            DateTime oldStopTime = breakTimeBlock.StopTime;
            breakTimeBlock.StopTime = breakTimeBlock.StopTime.AddMinutes(extend ? minutes : -minutes);

            if (timeBlocks != null)
            {
                foreach (TimeBlock timeBlock in timeBlocks.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime))
                {
                    if (timeBlock.TimeBlockId == breakTimeBlock.TimeBlockId || (timeBlock.GuidId.HasValue && timeBlock.GuidId == breakTimeBlock.GuidId))
                        continue; //Skip self
                    if (timeBlock.StopTime <= breakTimeBlock.StartTime)
                        continue; //TimeBlock is before change, continue to next
                    if (extend && timeBlock.StartTime >= breakTimeBlock.StopTime)
                        break; //TimeBlock is after change and isnt affected, break
                    if (decrease && timeBlock.StartTime > oldStopTime)
                        break; //TimeBlock is after change and isnt affected, break

                    if (extend)
                    {
                        if (CalendarUtility.IsCurrentOverlappedByNew(oldStopTime, breakTimeBlock.StopTime, timeBlock.StartTime, timeBlock.StopTime))
                        {
                            //TimeBlock is overlapped by change, delete it
                            base.ChangeEntityState(timeBlock, SoeEntityState.Deleted);
                            if (timeBlock.TimeBlockId == 0)
                                entities.DeleteObject(timeBlock);
                        }
                        else if (CalendarUtility.IsNewStopInCurrent(oldStopTime, breakTimeBlock.StopTime, timeBlock.StartTime, timeBlock.StopTime))
                        {
                            //TimeBlock is affected by change, move start forward
                            timeBlock.StartTime = breakTimeBlock.StopTime;
                        }
                    }
                    else if (timeBlock.StartTime == oldStopTime)
                    {
                        //TimeBlock is affected by change, move start backward
                        timeBlock.StartTime = breakTimeBlock.StopTime;
                    }
                }
            }
        }

        private bool TryPlaceBreakAfterStartTime(TimeCodeBreak timeCodeBreak, int breakWindowStartTimeMinutes, int breakWindowStopTimeMinutes, out DateTime startTime, out DateTime stopTime, bool assumeBreakWindowStartAfterMidnight)
        {
            bool result = false;

            startTime = CalendarUtility.GetDateFromMinutes(breakWindowStartTimeMinutes);
            stopTime = CalendarUtility.GetDateFromMinutes(breakWindowStopTimeMinutes);

            if (timeCodeBreak == null || !timeCodeBreak.StartTime.HasValue)
                return result;

            DateTime startTimeBreak = timeCodeBreak.StartTime.Value;

            //If stop is less than start we assume that the Starttime start past midnight or if we are assuming break Window starts after midnight
            if ((startTimeBreak < startTime) || assumeBreakWindowStartAfterMidnight)
                startTimeBreak = startTimeBreak.AddDays(1);

            DateTime stopTimeBreak = startTimeBreak.AddMinutes(timeCodeBreak.DefaultMinutes);

            //If startTimeBreak is inside breakWindow
            if (startTimeBreak >= startTime && stopTimeBreak <= stopTime)
            {
                startTime = startTimeBreak;
                stopTime = stopTimeBreak;
                result = true;
            }

            return result;
        }

        private bool DoTimeBlockNeedsToBeRecreated(TimeBlock timeBlock)
        {
            return timeBlock != null && timeBlock.TimeBlockId == 0 && !base.IsEntityAvailableInContext(entities, timeBlock);
        }

        private bool IsBreakWindowDuringSchedule(TimeCode timeCode, List<TimeScheduleTemplateBlockDTO> scheduleBlockItems)
        {
            TimeCodeBreak timeCodeBreak = timeCode is TimeCodeBreak ? timeCode as TimeCodeBreak : null;
            if (timeCodeBreak == null)
                return false;

            //Schedule, minutes from midnight
            int scheduleInMinutes = scheduleBlockItems.GetScheduleInMinutes();
            int scheduleOutMinutes = scheduleBlockItems.GetScheduleOutMinutes();

            //Break window, minutes from midnight
            int breakWindowStartTimeMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StartType, timeCodeBreak.StartTimeMinutes, scheduleInMinutes, scheduleOutMinutes);
            int breakWindowStopTimeMinutes = CalendarUtility.GetTimeInMinutes(timeCodeBreak.StopType, timeCodeBreak.StopTimeMinutes, scheduleInMinutes, scheduleOutMinutes);

            bool isBreakWindowDuringSchedule = IsBreakWindowTimeDuringSchedule(scheduleInMinutes, scheduleOutMinutes, breakWindowStartTimeMinutes, breakWindowStopTimeMinutes);
            if (!isBreakWindowDuringSchedule)
            {
                //Add 24hours
                breakWindowStartTimeMinutes += CalendarUtility.GetOneDayInMinutes();
                breakWindowStopTimeMinutes += CalendarUtility.GetOneDayInMinutes();

                isBreakWindowDuringSchedule = IsBreakWindowTimeDuringSchedule(scheduleInMinutes, scheduleOutMinutes, breakWindowStartTimeMinutes, breakWindowStopTimeMinutes);
            }

            return isBreakWindowDuringSchedule;
        }

        private bool IsBreakWindowTimeDuringSchedule(int scheduleInMinutes, int scheduleOutMinutes, int breakWindowStartTimeMinutes, int breakWindowStopTimeMinutes)
        {
            return breakWindowStartTimeMinutes >= scheduleInMinutes && breakWindowStopTimeMinutes <= scheduleOutMinutes;
        }

        private bool IsBreakTimeDuringSchedule(DateTime scheduleIn, DateTime scheduleOut, DateTime breakStartTime, DateTime breakStopTime)
        {
            return breakStartTime >= scheduleIn && breakStopTime <= scheduleOut;
        }

        private ValidateBreakChangeResult ValidateBreakChange(int employeeId, int timeScheduleTemplateBlockId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, DateTime startTime, int breakLength, bool isTemplate, int? timeScheduleScenarioHeadId)
        {
            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.EmployeeNotFound, GetText(10083, "Anställd hittades inte"));

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(startTime.Date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.EmployeeGroupNotFound, GetText(8539, "Tidavtal hittades inte"));

            if (isTemplate)
                startTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, startTime);
            DateTime stopTime = startTime.AddMinutes(breakLength);

            List<TimeScheduleTemplateBlock> templateBlocks = null;
            if (isTemplate)
                templateBlocks = GetTemplateScheduleBlocksForPeriod(null, timeScheduleTemplatePeriodId, true, false);
            else
                templateBlocks = GetScheduleBlocksForEmployee(timeScheduleScenarioHeadId, employeeId, startTime.Date, stopTime.Date);

            TimeScheduleTemplateBlock templateBlockBreak = templateBlocks.FirstOrDefault(i => i.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId);
            if (templateBlockBreak == null)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.BreakNotFound, GetText(11544, "Rast hittades inte"));

            DateTime scheduleIn = templateBlocks.GetScheduleIn();
            DateTime scheduleOut = templateBlocks.GetScheduleOut();

            #endregion

            #region Rule: Break must be inside schedule and cannot start/stop day with break

            if (templateBlocks.IsNullOrEmpty())
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.NoSchedule, GetText(11536, "Inget aktivt schema för dagen hittades"));
            if (templateBlocks.GetScheduleIn(actualDate: !isTemplate) > startTime)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.BeforeScheduleIn, GetText(11537, "Rast får inte starta före schemat startar"));
            if (templateBlocks.GetScheduleOut(actualDate: !isTemplate) < stopTime)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.AfterScheduleOut, GetText(11538, "Rast får inte sluta efter schemat slutar"));
            if (templateBlocks.GetScheduleIn(actualDate: !isTemplate) == startTime)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.StartsWithBreak, GetText(11539, "Får inte börja dagen med rast"));
            if (templateBlocks.GetScheduleOut(actualDate: !isTemplate) == stopTime)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.EndsWithBreak, GetText(11540, "Får inte avsluta dagen med rast"));

            #endregion

            #region Rule: Only TimeCodeBreaks connected to the EmployeeGroup can be used

            List<TimeCodeBreakGroup> timeCodeBreakGroups = GetTimeCodeBreakGroups();
            if (timeCodeBreakGroups.IsNullOrEmpty())
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.TimeCodeBreakGroupsNotFound, GetText(11543, "Rastgrupper hittades inte"));

            List<TimeCodeBreak> timeCodeBreaks = new List<TimeCodeBreak>();
            foreach (TimeCodeBreakGroup timeCodeBreakGroup in timeCodeBreakGroups)
            {
                TimeCodeBreak timeCodeBreakForGroup = GetTimeCodeBreakForEmployeeGroupFromCache(timeCodeBreakGroup.TimeCodeBreakGroupId, employeeGroup.EmployeeGroupId);
                if (timeCodeBreakForGroup != null)
                    timeCodeBreaks.Add(timeCodeBreakForGroup);
            }
            if (timeCodeBreaks.IsNullOrEmpty())
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.TimeCodeBreaksNotFound, GetText(11541, "Rasttyper hittades inte"));

            #endregion

            #region Rule: Length must corresponds to a existing TimeCodeBreak that are connected to the EmployeeGroup

            TimeCodeBreak timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == breakLength);
            if (timeCodeBreak == null)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.TimeCodeBreakForLengthNotFound, String.Format(GetText(11542, "Rasttyp för {0} minuter hittades inte"), breakLength), timeCodeBreaks.Select(i => i.TimeCodeId).ToList());

            #endregion

            #region Rule: Break cannot overlap another break

            templateBlockBreak.StartTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, startTime);
            templateBlockBreak.StopTime = templateBlockBreak.StartTime.AddMinutes(breakLength);

            List<TimeScheduleTemplateBlock> otherTemplateBlockBreaks = templateBlocks.GetBreaks();
            foreach (TimeScheduleTemplateBlock otherTemplateBlockBreak in otherTemplateBlockBreaks)
            {
                if (otherTemplateBlockBreak.TimeScheduleTemplateBlockId == templateBlockBreak.TimeScheduleTemplateBlockId)
                    continue;

                if (CalendarUtility.IsDatesOverlapping(templateBlockBreak.StartTime, templateBlockBreak.StopTime, otherTemplateBlockBreak.StartTime, otherTemplateBlockBreak.StopTime))
                    return new ValidateBreakChangeResult(SoeValidateBreakChangeError.BreakOverlapsAnotherBreak, GetText(11545, "Rasten får inte överlappa annan rast"));
            }

            #endregion

            #region Rule: Break cannot be outside break windows

            if (!ValidateBreakWindow(timeCodeBreakId, templateBlockBreak.StartTime, scheduleIn, scheduleOut, employee, startTime.Date).Success)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.TimeCodeBreakWindowInvalid, GetText(11792, "Rasten måste ligga inom rastfönstret"));

            #endregion

            #region Rule: Break can only overlap more than one shift if they are linked

            List<TimeScheduleTemplateBlock> templateBlocksOverlappingBreak = new List<TimeScheduleTemplateBlock>();
            foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks.Where(i => !i.IsBreak))
            {
                if (CalendarUtility.IsDatesOverlapping(templateBlockBreak.StartTime, templateBlockBreak.StopTime, templateBlock.StartTime, templateBlock.StopTime))
                    templateBlocksOverlappingBreak.Add(templateBlock);
            }
            if (templateBlocksOverlappingBreak.Count > 1 && templateBlocksOverlappingBreak.Select(i => i.Link).Distinct().Count() > 1)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.BreakOverlapsUnlinkedShifts, GetText(91931, "Rasten får inte gå in i flera pass om dom inte är länkade"));

            #endregion

            if (timeCodeBreak.TimeCodeId != timeCodeBreakId)
                return new ValidateBreakChangeResult(SoeValidateBreakChangeError.TimeCodeBreakChanged, "", new List<int>() { timeCodeBreak.TimeCodeId });
            return new ValidateBreakChangeResult();
        }

        private ActionResult ValidateBreakWindow(int timeCodeId, DateTime breakStartTime, DateTime scheduleIn, DateTime scheduleOut, Employee employee, DateTime date)
        {
            TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(timeCodeId);
            return ValidateBreakWindow(timeCodeBreak, breakStartTime, scheduleIn, scheduleOut, employee, date);
        }

        private ActionResult ValidateBreakWindow(TimeCodeBreak timeCodeBreak, DateTime breakStartTime, DateTime scheduleIn, DateTime scheduleOut, Employee employee, DateTime date)
        {
            if (scheduleIn > scheduleOut || timeCodeBreak == null)
                return new ActionResult(false);
            if (scheduleIn == scheduleOut)
                return new ActionResult(true);

            var (breakWindowStart, breakWindowStop) = GetTimeCodeBreakWindowFromCache(timeCodeBreak, scheduleIn, scheduleOut);
            if (breakStartTime < breakWindowStart || breakStartTime.AddMinutes(timeCodeBreak.DefaultMinutes) > breakWindowStop)
                return new ActionResult(false, (int)ActionResultSave.IncorrectInput, String.Format(GetText(9315, "Rasten {0} kl {1} ligger utanför sitt rastfönster, {2} - {3}"), timeCodeBreak.Name, breakStartTime.ToShortTimeString(), breakWindowStart.ToShortTimeString(), breakWindowStop.ToShortTimeString()) + ". " + string.Format(GetText(8840, "Anställd {0}, {1}."), employee.EmployeeNr, date.ToShortDateString()));

            return new ActionResult(true);
        }

        private ActionResult ValidateBreakWindow(List<TimeSchedulePlanningDayDTO> shifts)
        {
            foreach (var shiftsByEmployee in shifts.GroupBy(x => x.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(shiftsByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                foreach (var shiftsByEmployeeAndDate in shiftsByEmployee.GroupBy(x => x.ActualDate))
                {
                    DateTime scheduleIn = shiftsByEmployeeAndDate.ToList().GetScheduleIn();
                    DateTime scheduleOut = shiftsByEmployeeAndDate.ToList().GetScheduleOut();
                    List<BreakDTO> breaks = shiftsByEmployeeAndDate.ToList().GetBreaks(false);
                    foreach (var breakDTO in breaks)
                    {
                        DateTime breakStartTime = CalendarUtility.GetScheduleTime(breakDTO.StartTime, shiftsByEmployeeAndDate.Key.Date, breakDTO.StartTime.Date);
                        DateTime scheduleStartTime = CalendarUtility.GetScheduleTime(scheduleIn, shiftsByEmployeeAndDate.Key.Date, scheduleIn.Date);
                        DateTime scheduleStopTime = CalendarUtility.GetScheduleTime(scheduleOut, scheduleIn.Date, scheduleOut.Date);

                        ActionResult result = ValidateBreakWindow(breakDTO.TimeCodeId, breakStartTime, scheduleStartTime, scheduleStopTime, employee, shiftsByEmployeeAndDate.Key);
                        if (!result.Success)
                            return result;
                    }
                }
            }
            return new ActionResult(true);
        }

        #endregion

        #endregion

        #region TimeCodeRule evaluation

        private TimeCodeRule GetTimeCodeRule(TimeCode timeCode)
        {
            if (timeCode == null)
                return null;

            return timeCode.GetTimeCodeRule(TermGroup_TimeCodeRuleType.AdjustQuantityOnTime) ?? timeCode.GetTimeCodeRule(TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay);
        }

        private (TimeCode newTimeCodeId, DateTime newStartTime, DateTime newStopTime, decimal newQuantity) ApplyTimeCodeRules(TimeCodeTransaction timeCodeTransaction, TimeCode timeCode, TimeCodeRule timeCodeRule, Employee employee, DateTime date)
        {
            TimeCode newTimeCode = null;
            DateTime newStartTime = CalendarUtility.DATETIME_DEFAULT;
            DateTime newStopTime = CalendarUtility.DATETIME_DEFAULT;
            decimal quantity = 0;

            if (employee != null && timeCodeTransaction != null && timeCode != null && timeCodeRule != null)
            {
                DateTime? boundary = GetTimeCodeRuleBoundaryTime();
                if (IsBoundaryValid(boundary, timeCodeTransaction.Start, timeCodeTransaction.Stop))
                {
                    newTimeCode = timeCodeRule.Value > 0 ? GetTimeCodeFromCache(timeCodeRule.Value, loadRules: true) : null;
                    if (newTimeCode != null && newTimeCode.TimeCodeId != timeCode.TimeCodeId)
                    {
                        newStartTime = boundary.Value;
                        newStopTime = timeCodeTransaction.Stop;
                        quantity = GetQuantity(newStartTime, newStopTime);
                    }

                    timeCodeTransaction.Stop = boundary.Value;
                    timeCodeTransaction.Quantity = GetQuantity(timeCodeTransaction.Start, timeCodeTransaction.Stop);
                }
            }

            return (newTimeCode, newStartTime, newStopTime, quantity);

            DateTime? GetTimeCodeRuleBoundaryTime()
            {
                DateTime? boundary = null;
                if (timeCodeRule.Type == (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnTime)
                    return timeCodeRule.Time;
                else if (timeCodeRule.Type == (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay)
                    return GetScheduleInFromCache(employee.EmployeeId, date.AddDays(1), nextDay: true);
                return boundary;
            }
            bool IsBoundaryValid(DateTime? boundary, DateTime start, DateTime stop)
            {
                return boundary.HasValue && boundary.Value >= start && boundary.Value <= stop;
            }
            decimal GetQuantity(DateTime start, DateTime stop)
            {
                return Convert.ToDecimal(stop.Subtract(start).TotalMinutes);
            }
        }

        private decimal GetQuantityByEmploymentPercentage(Employee employee, TimeBlock timeBlock, decimal quantity)
        {
            if (employee == null || timeBlock == null)
                return quantity;

            TimeBlockDate timeBlockDate = timeBlock.TimeBlockDate ?? GetTimeBlockDateFromCache(employee.EmployeeId, timeBlock.TimeBlockDateId);
            Employment employment = timeBlockDate != null ? employee.GetEmployment(timeBlockDate.Date) : null;
            decimal? workPercentage = employment?.GetPercent(timeBlockDate.Date).ToNullable();
            quantity = workPercentage > 0 ? (quantity * (workPercentage.Value / 100)) : 0;

            return quantity;
        }

        #endregion

        #region Overtime evaluation

        private (DateTime Start, DateTime Stop) GetDatesForOvertimePeriod(DateTime currentDate, bool isSchedule = true)
        {
            //For now, always 1 week
            DateTime periodStart = CalendarUtility.GetBeginningOfWeek(currentDate);
            DateTime periodStop = isSchedule ? CalendarUtility.GetEndOfWeek(currentDate) : CalendarUtility.GetEndOfDay(currentDate);
            return (periodStart, periodStop);
        }

        private PayrollProductDistributionRuleHead GetPayrollProductDistributionRuleHeadWithRules(int payrollProductDistributionRuleHeadId)
        {
            return entities.PayrollProductDistributionRuleHead
                .Include("PayrollProductDistributionRule")
                .FirstOrDefault(h => h.PayrollProductDistributionRuleHeadId == payrollProductDistributionRuleHeadId && h.State == (int)SoeEntityState.Active);
        }

        private ActionResult GetPlanningPeriodsForPeriodCalculation(int timePeriodId, out TimePeriod childPeriod, out TimePeriod parentPeriod)
        {
            parentPeriod = null;
            childPeriod = GetTimePeriodWithHead(timePeriodId);
            if (childPeriod?.TimePeriodHead == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));

            parentPeriod = GetTimePeriodParent(childPeriod.TimePeriodHead.TimePeriodHeadId, childPeriod.StopDate);
            return new ActionResult(true);
        }

        private ActionResult DeleteTransactionsForPlanningPeriodCalculation(Employee employee, TimePeriod childPeriod, TimePeriod parentPeriod)
        {
            List<int> timePeriodIds = childPeriod.TimePeriodId.ObjToList();
            if (parentPeriod != null)
                timePeriodIds.Add(parentPeriod.TimePeriodId);

            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactions(employee.EmployeeId, timePeriodIds);
            return SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges: true);
        }

        private ActionResult ApplyPlanningPeriodCalculation(Employee employee, TimePeriod childPeriod, TimePeriod parentPeriod, List<SysExtraField> sysExtraFields, out EmployeePeriodTimeSummary childSummary, out EmployeePeriodTimeSummary parentSummary)
        {
            parentSummary = null;
            childSummary = GetEmployeePeriodTimeSummaryForEmployee(employee.EmployeeId, childPeriod, sysExtraFields);
            if (childSummary == null)
                return new ActionResult((int)ActionResultSave.PlannedPeriodCalculationCalculationSummaryFailed, GetText(8819, "Beräkning misslyckades"));

            if (parentPeriod != null)
            {
                parentSummary = GetEmployeePeriodTimeSummaryForEmployee(employee.EmployeeId, parentPeriod, sysExtraFields);
                if (parentSummary == null)
                    return new ActionResult((int)ActionResultSave.PlannedPeriodCalculationCalculationSummaryFailed, GetText(8819, "Beräkning misslyckades"));
            }
            return new ActionResult(true);
        }

        private List<TimeEnginePayrollProduct> ApplyPlannedPeriodCalculationRules(Employee employee, EmployeePeriodTimeSummary childSummary, EmployeePeriodTimeSummary parentSummary, TimePeriod childPeriod, TimePeriod parentPeriod)
        {
            if (employee == null)
                return new List<TimeEnginePayrollProduct>();

            var products = new List<TimeEnginePayrollProduct>();
            TryGenerateProductsForPlannedPeriodCalculation(childSummary, childPeriod?.TimePeriodHead?.PayrollProductDistributionRuleHeadId, ref products);
            TryGenerateProductsForPlannedPeriodCalculation(parentSummary, parentPeriod?.TimePeriodHead?.PayrollProductDistributionRuleHeadId, ref products);
            return products;
        }

        private void TryGenerateProductsForPlannedPeriodCalculation(EmployeePeriodTimeSummary summary, int? payrollProductDistributionRuleHeadId, ref List<TimeEnginePayrollProduct> products)
        {
            if (summary == null || !summary.ParentTimePeriodId.HasValue || summary.ParentPayrollPeriodBalanceTimeMinutes == 0 || !payrollProductDistributionRuleHeadId.HasValue)
                return;

            PayrollProductDistributionRuleHead head = GetPayrollProductDistributionRuleHeadWithRules(payrollProductDistributionRuleHeadId.Value);
            TryGenerateProductsForPlannedPeriodCalculation(head, summary.ParentTimePeriodId.Value, summary.ParentPayrollPeriodBalanceTimeMinutes, ref products);
        }

        private void TryGenerateProductsForPlannedPeriodCalculation(PayrollProductDistributionRuleHead head, int periodId, decimal quantity, ref List<TimeEnginePayrollProduct> products)
        {
            if (head?.PayrollProductDistributionRule == null || !head.PayrollProductDistributionRule.Any(r => r.Start <= quantity))
                return;

            decimal quantityUsed = 0;
            foreach (var row in head.PayrollProductDistributionRule.OrderBy(r => r.Start).ThenBy(r => r.Stop))
            {
                decimal rowQuantity = Math.Min(quantity, row.Stop) - quantityUsed;
                if (rowQuantity <= 0)
                    continue;

                var product = new TimeEnginePayrollProduct
                {
                    Quantity = rowQuantity,
                    PayrollProductId = row.PayrollProductId,
                    PlanningPeriodCalculationId = periodId,
                };
                products.Add(product);

                quantityUsed += product.Quantity;
                if (quantityUsed <= 0)
                    break;
            }
        }

        #endregion

        #region Five days vacation per week evaluation

        private bool UseVacationFiveDaysPerWeek(VacationGroupDTO vacationGroup)
        {
            return
                vacationGroup?.VacationGroupSE?.VacationDaysGrossUseFiveDaysPerWeek == true &&
                GetPayrollProductFromCache(sysPayrollTypeLevel4: (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_NoVacationDaysDeducted) != null;
        }

        private bool HasVacationWholeWeek(int employeeId, DateTime date)
        {
            DateTime weekStart = CalendarUtility.GetFirstDateOfWeek(date);
            DateTime weekStop = weekStart.AddDays(4);
            return HasCoherentVacation(employeeId, weekStart, weekStop, true, out _);
        }

        private bool HasCoherentVacation5Days(int employeeId, DateTime date)
        {
            if (HasCoherentVacation(employeeId, date, date.AddDays(4), true, out int nrOfDays))
                return true;
            if (HasCoherentVacation(employeeId, date.AddDays(-(5 - nrOfDays)), date, false, out _))
                return true;
            return false;
        }

        private bool HasCoherentVacation(int employeeId, DateTime startDate, DateTime stopDate, bool forward, out int nrOfDays)
        {
            nrOfDays = 0;
            int level3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;
            List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, startDate, stopDate, level3);
            List<DateTime> currentVacationDates = GetCurrentGeneratingAbsenceDates(employeeId, level3);

            DateTime currentDate = GetStartDate();
            while (DoContinue())
            {
                if (!HasVacation())
                    return false;
                Next();
                nrOfDays++;
            }
            return true;

            DateTime GetStartDate()
            {
                if (forward)
                    return startDate;
                else
                    return stopDate;
            }
            bool HasVacation()
            {
                return currentVacationDates.Contains(currentDate) || timePayrollTransactions.HasTransaction(currentDate);
            }
            bool DoContinue()
            {
                if (forward)
                    return currentDate <= stopDate;
                else
                    return currentDate >= startDate;
            }
            void Next()
            {
                if (forward)
                    currentDate = currentDate.AddDays(1);
                else
                    currentDate = currentDate.AddDays(-1);
            }
        }

        private bool IsVacationFiveDaysPerWeekZeroWeekDay(PayrollProduct payrollProduct, ApplyAbsenceDay applyAbsenceOutput)
        {
            return
                payrollProduct != null &&
                payrollProduct.IsAbsenceVacation() &&
                applyAbsenceOutput != null &&
                applyAbsenceOutput.VacationFiveDaysPerWeekHasSchedule == false &&
                applyAbsenceOutput.VacationFiveDaysPerWeekIsWeekend == false;
        }

        #endregion

        #region Sickness evaluation

        private bool HasEmployeeHighRiskProtection(Employee employee, DateTime date)
        {
            if (employee == null)
                return false;

            if (employee.HasHighRiskProtection(date))
                return true;

            List<TimePayrollTransaction> timePayrollTransactions = GetSicknessQualifyingDayTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, date.AddMonths(-12), date.AddDays(-Constants.SICKNESS_RELAPSEDAYS));
            return timePayrollTransactions.GetNrOfDays() >= Constants.SICKNESS_QUALIFYINGDAYS_TO_REACH_HIGHRISCPROTECTION;

        }

        private bool DoEvaluateQualifyingDeduction(TimeEngineTemplate template, ApplyAbsenceDay absenceDay, PayrollProduct qualifyingDeductionProduct, List<TimeTransactionItem> timeTransactionsItemsForDay = null)
        {
            if (UsePayroll())
            {
                if (qualifyingDeductionProduct == null || absenceDay == null)
                    return false;
                return absenceDay.AbsenceDayNumber > 0 && absenceDay.AbsenceDayNumber <= 14;
            }
            else
            {
                bool hasSickTransactions =
                    (template?.Outcome?.TimePayrollTransactions?.Any(i => i.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick) ?? false)
                    ||
                    (timeTransactionsItemsForDay?.Any(i => i.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick) ?? false);

                if (!hasSickTransactions)
                    return false;
                return qualifyingDeductionProduct != null || (template?.Outcome?.TimeCodeTransactions?.Any(i => i.IsSickDuringIwhOrStandbyTransaction) ?? false);
            }
        }

        private SicknessPeriod GetSicknessPeriod(TimeEngineTemplate template, Employee employee, ApplyAbsenceDay absenceDay, List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays, List<TimeTransactionItem> timeTransactionItemsForDate = null)
        {
            #region Prereq

            if (template == null || employee == null)
                return null;

            DateTime date = template.Date;
            if (date < Constants.SICKNESS_QUALIFYINGDAY_NEWRULESTART)
                return null;

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return null;
            if (!HasEmployeeRightToSicknessSalaryFromCache(employee, date))
                return null;
            if (HasEmployeeHighRiskProtectionFromCache(employee, date))
                return null;

            List<TimeTransactionItem> absenceSickTimeTransactionItems = null;
            bool hasTransactionsForDate = timeTransactionItemsForDate != null;
            if (hasTransactionsForDate)
            {
                timeTransactionItemsForDate = timeTransactionItemsForDate.Where(i => i.Date == date && i.TimeCodeStart.HasValue && i.TimeCodeStop.HasValue && i.TransactionType == SoeTimeTransactionType.TimePayroll).ToList();
                absenceSickTimeTransactionItems = timeTransactionItemsForDate.Where(i => i.IsAbsenceSickOrWorkInjury()).ToList();
            }

            List<StandbyInterval> standbyIntervals = GetStandbyIntervals(employee.EmployeeId, template.Outcome.UseStandby, absenceSickIwhStandbyDays);

            #endregion

            #region Cached SicknessPeriod

            SicknessPeriod sicknessPeriod = GetEmployeeSicknessPeriodFromCache(employee.EmployeeId, date);
            if (sicknessPeriod != null)
            {
                //Use cached SicknessPeriod
                if (sicknessPeriod.Dates.Contains(date))
                    return sicknessPeriod;

                //Extend SicknessPeriod
                List<TimePayrollTransaction> absenceSickTimePayrollTransactions = GetSicknessTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, date, date);
                if (sicknessPeriod.TryExtend(date, employeeGroup, absenceSickTimePayrollTransactions, standbyIntervals))
                    CreateQualifyingDeductionPeriods(sicknessPeriod, date);
                else
                    sicknessPeriod = null; //force create new SicknessPeriod
            }

            #endregion

            #region Create SicknessPeriod

            if (sicknessPeriod == null)
            {
                #region Get WorkTimeWeek

                DateTime? qualifyingWeekEnd = GetQualifyingWeekEnd(absenceDay, absenceSickIwhStandbyDays);
                int? employeeWeekMinutes = CalculateEmployeeWeekMinutes(employee, employeeGroup, date, qualifyingWeekEnd);
                if (!employeeWeekMinutes.HasValue || (employeeWeekMinutes.Value <= 0 && standbyIntervals.IsNullOrEmpty()))
                    return null;

                #endregion

                #region Get transactions and calculate qualifying day

                List<TimePayrollTransaction> absenceSickTimePayrollTransactions = new List<TimePayrollTransaction>();

                DateTime? possibleQualifyingDay = date;
                while (possibleQualifyingDay.HasValue)
                {
                    DateTime startDate = possibleQualifyingDay.Value.AddDays(-Constants.SICKNESS_RELAPSEDAYS);
                    DateTime stopDate = possibleQualifyingDay.Value;
                    if (stopDate == date && hasTransactionsForDate)
                        stopDate = stopDate.AddDays(-1);

                    List<TimePayrollTransaction> timePayrollTransactions = GetSicknessTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, startDate, stopDate);
                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                    {
                        if (absenceSickTimePayrollTransactions.Any(tpt => tpt.TimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId))
                            continue;
                        if (timePayrollTransaction.TimeBlockDate.Date < Constants.SICKNESS_QUALIFYINGDAY_NEWRULESTART)
                            return null;
                        absenceSickTimePayrollTransactions.Add(timePayrollTransaction);
                    }

                    possibleQualifyingDay = GetPossibleQualificationDay(timePayrollTransactions, possibleQualifyingDay.Value);
                }

                if (!absenceSickTimePayrollTransactions.Any() && absenceSickTimeTransactionItems != null && !absenceSickTimeTransactionItems.Any())
                    return null;

                #endregion

                sicknessPeriod = CreateSicknessPeriods(employee, employeeGroup, employeeWeekMinutes.Value, template.Outcome.UseStandby, standbyIntervals, absenceSickTimePayrollTransactions, absenceSickTimeTransactionItems).FirstOrDefault();
                if (sicknessPeriod != null)
                    CreateQualifyingDeductionPeriods(sicknessPeriod, timeTransactionItemsForDate);

                AddEmployeeSicknessPeriodToCache(employee.EmployeeId, sicknessPeriod);
            }

            #endregion

            return sicknessPeriod;
        }

        private DateTime? GetQualifyingWeekEnd(ApplyAbsenceDay absenceDay, List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays)
        {
            if (absenceDay != null)
                return absenceDay.QualifyingWeekEnd;
            else if (!absenceSickIwhStandbyDays.IsNullOrEmpty())
                return absenceSickIwhStandbyDays.First().QualifyingWeekEnd;
            return null;
        }

        private int? CalculateEmployeeWeekMinutes(Employee employee, EmployeeGroup employeeGroup, DateTime date, DateTime? qualifyingWeekEnd)
        {
            if (employee != null && employeeGroup != null)
            {
                switch (employeeGroup.QualifyingDayCalculationRule)
                {
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeek:
                        return GetEmployeeWorkTimeWeekForQualifyingSickness(employee, date);
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusExtraShifts:
                        return GetEmployeeWorkTimeWeekPlusExtraShiftsMinutesForQualifyingSickness(employee, date, qualifyingWeekEnd);
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusAdditionalContract:
                        return GetEmployeeWorkTimeWeekPlusAdditionalContractMinutesForQualifyingSickness(employee, employeeGroup, date);
                    case (int)TermGroup_QualifyingDayCalculationRule.UseAverageCalculationInTimePeriod:
                        return GetEmployeeAverageCalculateMinutesForQualifyingSickness(employee, employeeGroup, date);
                }
            }
            return null;
        }

        /// <summary>
        /// Anställdes genomsmittliga veckoarbetstid enligt anställningskontraktet
        /// </summary>
        private static int GetEmployeeWorkTimeWeekForQualifyingSickness(Employee employee, DateTime date)
        {
            return employee.GetEmployeeWorkTimeWeekMinutes(date);
        }

        /// <summary>
        /// X = (A + B)
        /// 
        /// A - Anställdes genomsmittliga veckoarbetstid enligt anställningskontraktet
        /// B - Schemapass markerat som extrapass
        /// X - Beräknad genomsnittlig veckoarbetstid
        /// 
        /// </summary>
        private int GetEmployeeWorkTimeWeekPlusExtraShiftsMinutesForQualifyingSickness(Employee employee, DateTime date, DateTime? qualifyingWeekEnd)
        {
            //Formula
            int employeeWorkTimeWeekMinutes = employee.GetEmployeeWorkTimeWeekMinutes(date);
            int employeeExtraShiftMinutes = GetEmployeeExtraShiftMinutesForQualifyingSickness(employee, date, qualifyingWeekEnd);
            int resultMinutes = employeeWorkTimeWeekMinutes + employeeExtraShiftMinutes;

            return resultMinutes;
        }

        /// <summary>
        /// X = A + (B - E) - C
        /// 
        /// A - Anställdes genomsmittliga veckoarbetstid enligt anställningskontraktet
        /// B - Aktuell veckas schematid enligt aktivt schema
        /// C - Aktuell veckas schematid enligt grundschema
        /// E - Tjänstledighet
        /// X - Beräknad genomsnittlig veckoarbetstid
        /// 
        /// </summary>
        private int? GetEmployeeWorkTimeWeekPlusAdditionalContractMinutesForQualifyingSickness(Employee employee, EmployeeGroup employeeGroup, DateTime date)
        {
            if (employee == null || employeeGroup == null)
                return null;

            Employment employment = employee.GetEmployment(date);
            if (employment == null)
                return null;

            //Formula
            var week = CalendarUtility.GetWeek(date);
            int employeeWorkTimeWeek = employment.GetWorkTimeWeek(date);
            int activeScheduleMinutes = GetScheduleMinutesForEmployee(employee.EmployeeId, week.DateFrom, week.DateTo);
            int templateScheduleMinutes = GetTemplateScheduleMinutes(employee.EmployeeId, week.DateFrom, week.DateTo);
            int leaveOfAbsenceMinutes = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, week.DateFrom, week.DateTo, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence).GetMinutes();
            int resultMinutes = employeeWorkTimeWeek + (activeScheduleMinutes - leaveOfAbsenceMinutes) - templateScheduleMinutes;

            return resultMinutes;
        }

        /// <summary>
        /// X = (D-E)/(F/7)
        /// Max: Heltidsmått enl Tidavtal
        /// 
        ///D - Aktuell schematid enligt Aktivt schema i avvikelseperioden
        ///E - Tjänstledighet
        ///F - Anställningsdagar i avvikelseperioden
        ///G - Tjänstledighet(hel dag - antal)
        ///H - Arbetstidsmått per vecka(Heltid)
        ///X - Beräknad genomsnittlig veckoarbetstid
        ///
        /// </summary>
        private int? GetEmployeeAverageCalculateMinutesForQualifyingSickness(Employee employee, EmployeeGroup employeeGroup, DateTime date)
        {
            if (employee == null || employeeGroup == null)
                return null;

            Employment employment = employee.GetEmployment(date);
            if (employment == null)
                return null;

            TimePeriod timePeriod = GetTimePeriodForEmployee(employee, date);
            if (timePeriod == null)
                return null;

            DateTime dateFrom = timePeriod.StartDate;
            DateTime dateTo = timePeriod.StopDate;

            //Formula
            int activeScheduleMinutes = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, dateFrom, dateTo).GetWorkMinutes();
            int leaveOfAbsenceMinutes = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, dateFrom, dateTo, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence).GetMinutes();
            int employmentDaysInPeriod = employee.GetEmploymentDays(dateFrom, dateTo);
            int validScheduleMinutes = activeScheduleMinutes - leaveOfAbsenceMinutes;
            int employmentWeeksInPeriod = employmentDaysInPeriod > 0 ? (employmentDaysInPeriod / 7) : 0;

            if (employmentWeeksInPeriod == 0)
            {
                if (employmentDaysInPeriod > 0)
                    employmentWeeksInPeriod = 1;
                else
                    return 0;
            }

            int resultMinutes = validScheduleMinutes / employmentWeeksInPeriod;
            AdjustEmployeeWeekMinutesByEmployeeGroupRule(ref resultMinutes, employment, employeeGroup, date);

            return resultMinutes;
        }

        private int GetEmployeeExtraShiftMinutesForQualifyingSickness(Employee employee, DateTime date, DateTime? qualifyingWeekEnd)
        {
            if (qualifyingWeekEnd.HasValue && qualifyingWeekEnd.Value > date)
            {
                DateTime periodDateFrom = CalendarUtility.GetFirstDateOfWeek(date).Date;
                DateTime periodDateTo = CalendarUtility.GetLastDateOfWeek(date).Date;
                return GetExtraShiftCalculationFromCache(employee, periodDateFrom, periodDateTo, useIgnoreIfExtraShifts: false)?.WorkMinutes ?? 0;
            }
            return 0;
        }

        private void AdjustEmployeeWeekMinutesByEmployeeGroupRule(ref int minutes, Employment employment, EmployeeGroup employeeGroup, DateTime date)
        {
            if (employment == null || employeeGroup == null)
                return;

            PayrollManager.AdjustEmployeeWeekMinutesByEmployeeGroupRule(ref minutes, employment.GetPercent(date), employeeGroup.RuleWorkTimeWeek);
        }

        private List<StandbyInterval> GetStandbyIntervals(int employeeId, bool useStandby, List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays)
        {
            if (!useStandby || absenceSickIwhStandbyDays.IsNullOrEmpty())
                return new List<StandbyInterval>();

            List<StandbyInterval> standbyIntervals = new List<StandbyInterval>();
            foreach (ApplyAbsenceSickIwhStandbyDay absenceDay in absenceSickIwhStandbyDays.Where(i => i.IsQualifyingStandby).OrderBy(i => i.ReplaceStartTime))
            {
                standbyIntervals.Add(new StandbyInterval(absenceDay));
            }

            if (standbyIntervals.Any(i => i.AbsenceDayNumber == 2))
            {
                ApplyAbsenceSickIwhStandbyDay absenceDay2 = absenceSickIwhStandbyDays.FirstOrDefault(i => i.AbsenceDayNumber == 2);
                if (absenceDay2 != null)
                {
                    List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, absenceDay2.Date.AddDays(-Constants.SICKNESS_RELAPSEDAYS), absenceDay2.Date.AddDays(-1), (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick);
                    TimePayrollTransaction timePayrollTransactionDay1 = timePayrollTransactions.OrderBy(i => i.TimeBlockDate.Date).LastOrDefault();
                    if (timePayrollTransactionDay1 != null)
                    {
                        //Create qualifying standby intervals from schedule
                        List<TimeScheduleTemplateBlock> scheduleBlocksDay1 = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(timePayrollTransactionDay1.EmployeeId, timePayrollTransactionDay1.TimeBlockDate.Date, null, includeStandBy: true);
                        foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocksDay1.GetWork().GetStandby())
                        {
                            standbyIntervals.Add(new StandbyInterval(scheduleBlock));
                        }

                        //Create qualifying standby intervals from schedule breaks
                        List<TimePayrollTransaction> timePayrollTransactionsDay1 = timePayrollTransactions.Where(i => i.TimeBlockDateId == timePayrollTransactionDay1.TimeBlockDateId).ToList();
                        timePayrollTransactionsDay1.Where(tpt => !tpt.TimeCodeTransactionReference.IsLoaded).ToList().ForEach(tpt => tpt.TimeCodeTransactionReference.Load());
                        foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBlocksDay1.GetBreaks())
                        {
                            if (timePayrollTransactionsDay1.Any(tpt => tpt.ProductId == absenceDay2.ReplaceRelatedProductId && tpt.TimeCodeTransaction.Start == scheduleBreak.StartTime && tpt.TimeCodeTransaction.Stop == scheduleBreak.StopTime))
                                standbyIntervals.Add(new StandbyInterval(scheduleBreak));
                        }

                        bool doAnyTransactonStartBeforeMidnight = false;
                        foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions.Where(i => i.TimeBlockDateId == timePayrollTransactionDay1.TimeBlockDateId))
                        {
                            if (!timePayrollTransaction.TimeCodeTransactionReference.IsLoaded)
                                timePayrollTransaction.TimeCodeTransactionReference.Load();

                            if (timePayrollTransaction.TimeCodeTransaction.Start.Date == CalendarUtility.DATETIME_DEFAULT)
                            {
                                doAnyTransactonStartBeforeMidnight = true;
                                break;
                            }
                        }

                        //Sickness starts after midnight day 1, and therefore any sickness on day 2 should be regarded as qualifying
                        if (!doAnyTransactonStartBeforeMidnight)
                        {
                            foreach (StandbyInterval standbyInterval in standbyIntervals.Where(i => i.AbsenceDayNumber == 2))
                            {
                                standbyInterval.SetAbsenceDayNumber1();
                            }
                        }

                    }
                }
            }

            return standbyIntervals.OrderBy(i => i.AbsenceDayNumber).ThenBy(i => i.StartTime).ToList();
        }

        private List<SicknessPeriod> CreateSicknessPeriods(
            Employee employee,
            EmployeeGroup employeeGroup,
            int employeeWeekMinutes,
            bool useStandby,
            List<StandbyInterval> standbyIntervals,
            List<TimePayrollTransaction> absenceSickTimePayrollTransactions,
            List<TimeTransactionItem> absenceSickTimeTransactionItems)
        {
            List<SicknessPeriod> sicknessPeriods = new List<SicknessPeriod>();

            if (employee == null || employeeGroup == null)
                return sicknessPeriods;

            absenceSickTimePayrollTransactions = absenceSickTimePayrollTransactions?.Where(i => i.TimeBlockDate != null).ToList() ?? new List<TimePayrollTransaction>();
            absenceSickTimeTransactionItems = absenceSickTimeTransactionItems?.Where(i => i.Date.HasValue).ToList() ?? new List<TimeTransactionItem>();
            if (!absenceSickTimePayrollTransactions.Any() && !absenceSickTimeTransactionItems.Any())
                return sicknessPeriods;
            if (absenceSickTimePayrollTransactions.Any(i => i.TimeBlockDate.Date < Constants.SICKNESS_QUALIFYINGDAY_NEWRULESTART))
                return sicknessPeriods;
            if (absenceSickTimeTransactionItems.Any(i => i.Date.Value < Constants.SICKNESS_QUALIFYINGDAY_NEWRULESTART))
                return sicknessPeriods;

            DateTime startDate = CalendarUtility.GetEarliestDate(absenceSickTimePayrollTransactions.GetFirstDate(), absenceSickTimeTransactionItems.GetFirstDate()) ?? CalendarUtility.DATETIME_DEFAULT;
            DateTime stopDate = CalendarUtility.GetLatestDate(absenceSickTimePayrollTransactions.GetLastDate(), absenceSickTimeTransactionItems.GetLastDate()) ?? CalendarUtility.DATETIME_DEFAULT;
            if (startDate == CalendarUtility.DATETIME_DEFAULT || stopDate == CalendarUtility.DATETIME_DEFAULT)
                return sicknessPeriods;

            SicknessPeriod sicknessPeriod = null;
            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                try
                {
                    if (absenceSickTimePayrollTransactions.Any(i => i.TimeBlockDate.Date == currentDate))
                        SetSicknessPeriod(ref sicknessPeriod, employee, employeeGroup, employeeWeekMinutes, currentDate, useStandby, standbyIntervals, absenceSickTimePayrollTransactions.Where(i => i.TimeBlockDate.Date == currentDate).ToList());
                    else if (absenceSickTimeTransactionItems.Any(i => i.Date == currentDate))
                        SetSicknessPeriod(ref sicknessPeriod, employee, employeeGroup, employeeWeekMinutes, currentDate, useStandby, standbyIntervals, absenceSickTimeTransactionItems.Where(i => i.Date == currentDate).ToList());

                    if (!sicknessPeriods.Any(i => i.Guid == sicknessPeriod.Guid))
                        sicknessPeriods.Add(sicknessPeriod);
                }
                finally
                {
                    currentDate = currentDate.AddDays(1);
                }
            }

            return sicknessPeriods;
        }

        private void SetSicknessPeriod(
            ref SicknessPeriod sicknessPeriod,
            Employee employee,
            EmployeeGroup employeeGroup,
            int employeeWeekMinutes,
            DateTime date,
            bool useStandby,
            List<StandbyInterval> standbyIntervals,
            object transactions
            )
        {
            if (sicknessPeriod == null || !sicknessPeriod.TryExtend(date, employeeGroup, transactions, standbyIntervals))
                sicknessPeriod = SicknessPeriod.Start(date, employee.EmployeeId, employeeWeekMinutes, useStandby, standbyIntervals, transactions);
        }

        private void CreateQualifyingDeductionPeriods(SicknessPeriod sicknessPeriod, List<TimeTransactionItem> transactionItems = null)
        {
            if (sicknessPeriod == null || !sicknessPeriod.HasAbsenceSickTransactions)
                return;

            if (sicknessPeriod.HasAbsenceSickTimePayrollTransactions)
            {
                foreach (DateTime date in sicknessPeriod.GetAbsenceSickTimePayrollTransactionsDates())
                    CreateQualifyingDeductionPeriods(sicknessPeriod, date);
            }

            if (sicknessPeriod.HasAbsenceSickTimeTransactionItems && !transactionItems.IsNullOrEmpty())
            {
                foreach (DateTime date in sicknessPeriod.GetAbsenceSickTimePayrollTransactionItemsDates())
                    CreateQualifyingDeductionPeriods(sicknessPeriod, date, transactionItems);
            }
        }

        private void CreateQualifyingDeductionPeriods(SicknessPeriod sicknessPeriod, DateTime date)
        {
            //Try break early
            if (sicknessPeriod == null || sicknessPeriod.DoBreakPeriodEvaluation(null))
                return;

            List<TimePayrollTransaction> timePayrollTransactionsForDay = sicknessPeriod.GetTimePayrollTransactions(date);
            if (timePayrollTransactionsForDay.IsNullOrEmpty())
                return;

            List<TimeRule> timeRules = TimeRuleManager.GetTimeRulesFromCache(entities, actorCompanyId);
            int timeBlockDateId = timePayrollTransactionsForDay.First().TimeBlockDateId;

            List<TimePayrollTransaction> qualifyingDeductionBasisTransactions = GetQualifyingDeductionBasisTransactions(sicknessPeriod.EmployeeId, timeBlockDateId);
            foreach (TimePayrollTransaction qualifyingDeductionBasisTransaction in qualifyingDeductionBasisTransactions.Where(i => i.PayrollStartValueRowId.HasValue))
            {
                QualifyingDeductionPeriod qualifyingDeductionPeriod = sicknessPeriod.CreateQualifyingDeductionPeriod(qualifyingDeductionBasisTransaction, date);
                if (sicknessPeriod.DoBreakPeriodEvaluation(qualifyingDeductionPeriod))
                    return;
            }
            foreach (TimePayrollTransaction qualifyingDeductionBasisTransaction in qualifyingDeductionBasisTransactions.Where(i => !i.PayrollStartValueRowId.HasValue && i.TimeCodeTransaction != null).OrderBy(i => i.TimeCodeTransaction.Start))
            {
                if (qualifyingDeductionBasisTransaction.TimeCodeTransaction.IsSickDuringIwhOrStandbyTransaction)
                    continue;
                if (timeRules.IsInconvenientWorkHours(qualifyingDeductionBasisTransaction.TimeCodeTransaction.TimeRuleId ?? 0))
                    continue;

                QualifyingDeductionPeriod qualifyingDeductionPeriod = sicknessPeriod.CreateQualifyingDeductionPeriod(qualifyingDeductionBasisTransaction, date);
                if (sicknessPeriod.DoBreakPeriodEvaluation(qualifyingDeductionPeriod))
                    return;
            }
        }

        private void CreateQualifyingDeductionPeriods(SicknessPeriod sicknessPeriod, DateTime date, List<TimeTransactionItem> transactionItems)
        {
            //Try break early
            if (sicknessPeriod == null || sicknessPeriod.DoBreakPeriodEvaluation(null))
                return;

            List<TimeTransactionItem> timeTransactionItemsForDay = sicknessPeriod.GetTimePayrollTransactionItems(date);
            if (timeTransactionItemsForDay.IsNullOrEmpty())
                return;

            List<TimeTransactionItem> qualifyingDeductionBasisTransactions = GetQualifyingDeductionBasisTransactions(transactionItems);
            foreach (TimeTransactionItem qualifyingDeductionBasisTransactionItem in qualifyingDeductionBasisTransactions.OrderBy(i => i.TimeCodeStart))
            {
                QualifyingDeductionPeriod qualifyingDeductionPeriod = sicknessPeriod.CreateQualifyingDeductionPeriod(qualifyingDeductionBasisTransactionItem, date);
                if (sicknessPeriod.DoBreakPeriodEvaluation(qualifyingDeductionPeriod))
                    return;
            }
        }

        private DateTime? GetPossibleQualificationDay(List<TimePayrollTransaction> timePayrollTransactions, DateTime? curentDate = null)
        {
            TimePayrollTransaction firstTransaction = timePayrollTransactions.GetFirst();
            if (firstTransaction == null)
                return null;
            if (curentDate.HasValue && curentDate.Value <= firstTransaction.TimeBlockDate.Date)
                return null;
            return firstTransaction.TimeBlockDate.Date;
        }

        private bool HasEmployeeRightToSicknessSalary(Employee employee, DateTime date)
        {
            int? payrollGroupId = employee.GetPayrollGroupId(date);
            if (payrollGroupId.HasValue)
            {
                bool useSicknessSalaryRegulation = GetPayrollGroupWithSettingsFromCache(payrollGroupId.Value)?.PayrollGroupSetting?.FirstOrDefault(i => i.Type == (int)PayrollGroupSettingType.SicknessSalaryRegulation)?.BoolData ?? false;
                if (useSicknessSalaryRegulation)
                {
                    Employment employment = employee.GetEmployment(date);
                    if (employment != null && employment.DateFrom.HasValue && employment.GetEmploymentDays() <= 31)
                    {
                        int employmentDays = employment.GetEmploymentDaysUntilDate(date);
                        if (employmentDays < 14)
                        {
                            // Walk back through previous employments, counting consecutive calendar days as long as the gap between employments does not exceed 14 days.
                            // Stop traversing if accumulated consecutive days exceed 31 days.
                            Employment currentEmployment = employment;
                            while (currentEmployment != null && employmentDays < 14 && employment.GetEmploymentDays() <= 31)
                            {
                                Employment prevEmployment = employee.GetPrevEmployment(currentEmployment.GetDateFromOrMin());
                                if (prevEmployment == null || prevEmployment.EmploymentId == currentEmployment.EmploymentId)
                                    break;

                                // Ensure the gap between prevEmployment and current is <= 14 days
                                if (prevEmployment.DateTo.HasValue &&
                                    prevEmployment.DateTo.Value >= currentEmployment.DateFrom.Value.AddDays(-14))
                                {
                                    // Only count consecutive days up to the day before the current employment start to preserve the "in a row" requirement.
                                    DateTime until = currentEmployment.DateFrom.Value.AddDays(-1);
                                    int consecutivePrevDays = prevEmployment.GetEmploymentDaysUntilDate(until);
                                    if (consecutivePrevDays <= 0)
                                        break;

                                    employmentDays += consecutivePrevDays;

                                    // Stop traversing further if the total chain exceeds 31 days (no need to continue)
                                    if (employmentDays > 31)
                                        break;

                                    // Continue walking backwards
                                    currentEmployment = prevEmployment;
                                }
                                else
                                {
                                    // Gap exceeds 14 days; break the chain
                                    break;
                                }
                            }

                            if (employmentDays < 14)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private List<TimePayrollTransaction> GetSicknessQualifyingDayTransactionsWithTimeBlockDateFromCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeId, dateFrom, dateTo);

            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();
            timePayrollTransactions.AddRange(GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick, sysPayrollTypeLevel4: (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_QualifyingDay));
            timePayrollTransactions.AddRange(GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury, sysPayrollTypeLevel4: (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury_QualifyingDay));
            return timePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetSicknessTransactionsWithTimeBlockDateFromCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeId, dateFrom, dateTo);

            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();
            timePayrollTransactions.AddRange(GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick));
            timePayrollTransactions.AddRange(GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury));
            return timePayrollTransactions;
        }

        private ExtraShiftCalculationPeriod GetExtraShiftCalculation(Employee employee, DateTime dateFrom, DateTime dateTo, bool useIgnoreIfExtraShifts)
        {
            ExtraShiftCalculationPeriod extraShiftCalculation = new ExtraShiftCalculationPeriod(employee.EmployeeId, dateFrom, dateTo, useIgnoreIfExtraShifts);
            int totalWorkAndBreakMinutes = 0;
            int totalBreakMinutes = 0;

            List<TimeScheduleType> scheduleTypesWithIgnoreExtraShifts = useIgnoreIfExtraShifts ? GetTimeScheduleTypesWithIgnoreIfExtraShiftFromCache(): null;
            List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksForEmployee(null, employee.EmployeeId, dateFrom, dateTo, loadStaffingIfUsed: false);
            foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks.Where(tb => tb.ExtraShift && !tb.IsBreak))
            {
                if (useIgnoreIfExtraShifts && scheduleBlock.TimeScheduleTypeId.HasValue && scheduleTypesWithIgnoreExtraShifts.Any(st => st.TimeScheduleTypeId == scheduleBlock.TimeScheduleTypeId.Value))
                    continue;

                int workMinutes = (int)(scheduleBlock.StopTime - scheduleBlock.StartTime).TotalMinutes;
                totalWorkAndBreakMinutes += workMinutes;

                List<TimeScheduleTemplateBlock> breaksWithinExtraShift = new List<TimeScheduleTemplateBlock>();
                if (scheduleBlock.TimeScheduleEmployeePeriodId.HasValue)
                {
                    List<TimeScheduleTemplateBlock> scheduleBreaks = scheduleBlocks
                        .Where(tb => 
                            tb.IsBreak && 
                            tb.TimeScheduleEmployeePeriodId.HasValue && 
                            tb.TimeScheduleEmployeePeriodId.Value == scheduleBlock.TimeScheduleEmployeePeriodId.Value)
                        .ToList();

                    foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBreaks)
                    {
                        int breakMinutes = (int)CalendarUtility.GetNewTimeInCurrent(scheduleBlock.StartTime, scheduleBlock.StopTime, scheduleBreak.StartTime, scheduleBreak.StopTime).TotalMinutes;
                        totalBreakMinutes += breakMinutes;
                        if (breakMinutes > 0)
                            breaksWithinExtraShift.Add(scheduleBreak);
                    }
                }
                extraShiftCalculation.AddBasis(scheduleBlock, breaksWithinExtraShift);
            }
            extraShiftCalculation.WorkMinutes = totalWorkAndBreakMinutes - totalBreakMinutes;
            return extraShiftCalculation;
        }

        #endregion

        private sealed class TimeEngineRuleEvaluatorParam
        {
            #region Variables

            public DateTime Date { get; set; }
            public TimeChunk InputTimeChunk { get; set; }
            public Employee Employee { get; set; }
            public Employment Employment
            {
                get
                {
                    return this.Employee.GetEmployment(this.Date);
                }
            }
            public EmployeeGroup EmployeeGroup { get; set; }
            public TimeRule TimeRule { get; set; }
            public TimeCode TimeCode { get; set; }
            public List<TimeScheduleTemplateBlock> ScheduleBlocks { get; set; }
            public List<TimeScheduleTemplateBlock> ScheduleBlocksInOvertimePeriod { get; set; }
            public List<TimeBlock> PresenceTimeBlocks { get; set; }
            public List<TimeBlock> PresenceTimeBlocksInOvertimePeriod { get; set; }
            public List<TimeCodeTransaction> PreviousTimeCodeTransactions { get; set; }
            public DateTime ScheduleIn { get; set; }
            public DateTime ScheduleOut { get; set; }
            public TimeSpan PresenceStart { get; set; }
            public TimeSpan PresenceStop { get; set; }
            public TimeSpan PresenceStartCurrent { get; set; }
            public TimeSpan PresenceStopCurrent { get; set; }
            public List<int> TimeDeviationCauseIdsOvertime { get; set; }
            public List<int> TimeScheduleTypeIdsIsNotScheduleTime { get; set; }

            #endregion

            #region Ctor

            public TimeEngineRuleEvaluatorParam(DateTime date, TimeChunk inputTimeChunk, Employee employee, EmployeeGroup employeeGroup, TimeRule timeRule, TimeCode timeCode, List<int> timeDeviationCauseIdsOvertime, List<int> timeScheduleTypeIdsIsNotScheduleTime, List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeScheduleTemplateBlock> scheduleBlocksForOvertimePeriod, List<TimeBlock> presenceTimeBlocks, List<TimeBlock> presenceTimeBlocksForOvertimePeriod, List<TimeCodeTransaction> previousTimeCodeTransactions)
            {
                this.Date = date;
                this.InputTimeChunk = inputTimeChunk;
                this.Employee = employee;
                this.EmployeeGroup = employeeGroup;
                this.TimeRule = timeRule;
                this.TimeRule.HasFailed = false;
                this.TimeCode = timeCode;
                this.TimeDeviationCauseIdsOvertime = timeDeviationCauseIdsOvertime ?? new List<int>();
                this.TimeScheduleTypeIdsIsNotScheduleTime = timeScheduleTypeIdsIsNotScheduleTime ?? new List<int>();
                this.ScheduleBlocks = scheduleBlocks.FilterScheduleOrStandby().SortByStartTime();
                this.ScheduleBlocksInOvertimePeriod = scheduleBlocksForOvertimePeriod.FilterScheduleOrStandby().SortByStartTime();
                this.ScheduleIn = this.ScheduleBlocks.GetScheduleIn(this.TimeScheduleTypeIdsIsNotScheduleTime);
                this.ScheduleOut = this.ScheduleBlocks.GetScheduleOut(this.TimeScheduleTypeIdsIsNotScheduleTime);
                this.PresenceTimeBlocks = presenceTimeBlocks.SortByStart();
                this.PresenceTimeBlocksInOvertimePeriod = presenceTimeBlocksForOvertimePeriod.SortByStart();
                this.PresenceStart = CalendarUtility.GetTimeSpanFromDateTime(this.PresenceTimeBlocks.GetStartTime());
                this.PresenceStop = CalendarUtility.GetTimeSpanFromDateTime(this.PresenceTimeBlocks.GetStopTime());
                this.PresenceStartCurrent = this.PresenceStart;
                this.PresenceStopCurrent = this.PresenceStop;
                this.PreviousTimeCodeTransactions = previousTimeCodeTransactions ?? new List<TimeCodeTransaction>();
            }

            #endregion

            #region Properties

            public bool IsValid
            {
                get
                {
                    if (TimeRule == null || TimeCode == null)
                        return false;

                    //RuleEvaluation fail if TimeRule contains no TimeRuleExpression
                    if (TimeRule.TimeRuleExpression.IsNullOrEmpty())
                        return false;

                    //RuleEvaluation fail if start or stop TimeRuleExpression of TimeRule where not found
                    if (StartExpression == null || StopExpression == null)
                        return false;

                    return true;
                }
            }

            private TimeRuleExpression startExpression = null;
            public TimeRuleExpression StartExpression
            {
                get
                {
                    if (startExpression == null)
                        startExpression = TimeRule.GetStartExpression();
                    return startExpression;
                }
            }

            private TimeRuleExpression stopExpression = null;
            public TimeRuleExpression StopExpression
            {
                get
                {
                    if (stopExpression == null)
                        stopExpression = TimeRule.GetStopExpression();
                    return stopExpression;
                }
            }

            private bool? timeRuleContainsNotOperand = null;
            public bool TimeRuleContainsNotOperand
            {
                get
                {
                    if (!timeRuleContainsNotOperand.HasValue)
                        timeRuleContainsNotOperand = TimeRule.ContainsOperand(SoeTimeRuleOperatorType.TimeRuleOperatorNot);
                    return timeRuleContainsNotOperand.Value;
                }
            }

            public bool IsForward
            {
                get
                {
                    return this.TimeRule?.RuleStartDirection == (int)SoeTimeRuleDirection.Forward;
                }
            }
            public bool IsBackward
            {
                get
                {
                    return this.TimeRule?.RuleStartDirection == (int)SoeTimeRuleDirection.Backward;
                }
            }
            public bool HasTimeBlocks
            {
                get
                {
                    return !PresenceTimeBlocks.IsNullOrEmpty();
                }
            }

            #endregion

            #region Methods

            public List<TimeBlock> FilterPresenceTimeBlocks(bool onlyPaid = false, bool onlyOvertime = false)
            {
                List<TimeBlock> valid = onlyOvertime ? this.PresenceTimeBlocksInOvertimePeriod : this.PresenceTimeBlocks;
                List<TimeBlock> validPresenceTimeBlocks = valid?.Where(tb => tb.StartTime <= tb.StopTime && tb.State != (int)SoeEntityState.Temporary).ToList() ?? new List<TimeBlock>();
                if (onlyPaid)
                    validPresenceTimeBlocks = validPresenceTimeBlocks.Where(tb => tb.IsPayed).ToList();
                return validPresenceTimeBlocks.OrderBy(tb => tb.TimeBlockDate.Date).ThenBy(tb => tb.StartTime).ThenBy(tb => tb.StopTime).ToList();
            }

            public List<TimeRange> GetScheduleHoles()
            {
                List<TimeRange> scheduleHoles = new List<TimeRange>();

                TimeScheduleTemplateBlock previousTemplateBlock = null;
                foreach (TimeScheduleTemplateBlock templateBlock in ScheduleBlocks)
                {
                    if (previousTemplateBlock != null)
                    {
                        if (templateBlock.IsBreak && templateBlock.StopTime < previousTemplateBlock.StopTime)
                            continue;

                        int holeMinutes = Convert.ToInt32(templateBlock.StartTime.Subtract(previousTemplateBlock.StopTime).TotalMinutes);
                        if (holeMinutes > 0)
                            scheduleHoles.Add(new TimeRange(previousTemplateBlock.StopTime, templateBlock.StartTime));
                    }

                    previousTemplateBlock = templateBlock;
                }

                return scheduleHoles;
            }

            public bool IsEmploymentPercentFulltime()
            {
                return this.Employment?.GetPercent(this.Date) == 100;
            }

            public int GetFullTimeWorkWeekTime()
            {
                return this.Employment?.GetFullTimeWorkTimeWeek(this.EmployeeGroup, this.Date) ?? 0;
            }

            public DateTime? GetScheduleInForDate(DateTime date, bool onlyOvertime, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
            {
                return onlyOvertime ? this.ScheduleBlocksInOvertimePeriod.Filter(date).GetScheduleInNullable(timeScheduleTypeIdsIsNotScheduleTime) : this.ScheduleIn;
            }

            public DateTime? GetScheduleOutForDate(DateTime date, bool onlyOvertime, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
            {
                return onlyOvertime ? this.ScheduleBlocksInOvertimePeriod.Filter(date).GetScheduleOutNullable(timeScheduleTypeIdsIsNotScheduleTime) : this.ScheduleOut;
            }

            #endregion
        }

        private sealed class TimeEngineRuleEvaluatorProgress
        {
            #region Constants

            /// <summary>
            /// Balance rules uses previously generated transactions
            /// Constant rules should only be evaluated once
            /// Standby rules uses previously generated transactions
            /// </summary>
            public enum TimeRuleStage
            {
                Regular = 1,
                BalanceAndConstant = 2,
                RegularStandby = 3,
                BalanceAndConstantStandBy = 4,
            }

            #endregion

            #region Variables

            public int NrOfStages { get; private set; }
            public int FirstStage
            {
                get
                {
                    return (int)TimeRuleStage.Regular;
                }
            }
            public bool UseStandbyStages
            {
                get
                {
                    return this.NrOfStages > (int)TimeRuleStage.BalanceAndConstant;
                }
            }
            private TimeRuleStage currentStage;
            private readonly List<TimeRule> allTimeRules;
            private readonly List<int> evaluatedRuleIds;
            private readonly List<int> resultedRuleIds;
            public bool DoScheduleContainsStandby { get; }
            public bool HasOvertimePresence { get; }
            public bool AllowEmptyTransactions { get; set; }

            #endregion

            #region Ctor

            public TimeEngineRuleEvaluatorProgress(List<TimeRule> timeRules, bool doScheduleContansStandby, bool hasOvertimePresence = false)
            {
                this.DoScheduleContainsStandby = doScheduleContansStandby;
                this.HasOvertimePresence = hasOvertimePresence;
                this.allTimeRules = timeRules?.Where(i => doScheduleContansStandby || !i.IsStandby).ToList() ?? new List<TimeRule>();
                this.NrOfStages = this.allTimeRules.HasStandby() ? (int)TimeRuleStage.BalanceAndConstantStandBy : (int)TimeRuleStage.BalanceAndConstant;
                this.currentStage = TimeRuleStage.Regular;
                this.evaluatedRuleIds = new List<int>();
                this.resultedRuleIds = new List<int>();
            }

            #endregion

            #region Public methods

            public List<TimeRule> GetTimeRules()
            {
                return this.allTimeRules;
            }

            public List<TimeRule> GetTimeRules(SoeTimeRuleType type, bool standBy)
            {
                List<TimeRule> timeRules = null;

                switch (type)
                {
                    case SoeTimeRuleType.Presence:
                        timeRules = this.GetPresenceRules().Where(i => i.IsStandby == standBy).ToList();
                        break;
                    case SoeTimeRuleType.Absence:
                        timeRules = this.GetAbsenceRules();
                        break;
                    case SoeTimeRuleType.Constant:
                        timeRules = this.GetConstantRules().Where(i => i.IsStandby == standBy).ToList();
                        break;
                    default:
                        timeRules = new List<TimeRule>();
                        break;
                }

                return timeRules;
            }

            public bool ContainsStandbyRules()
            {
                return this.allTimeRules.Any(i => i.IsStandby);
            }

            public bool IsFirstStage()
            {
                return this.currentStage == TimeRuleStage.Regular;
            }

            public bool IsStandbyStage()
            {
                return this.currentStage == TimeRuleStage.RegularStandby || this.currentStage == TimeRuleStage.BalanceAndConstantStandBy;
            }

            public bool DoEvaluateBalanceRules()
            {
                return this.currentStage == TimeRuleStage.BalanceAndConstant || this.currentStage == TimeRuleStage.BalanceAndConstantStandBy;
            }

            public bool DoEvaluateConstantRules()
            {
                return DoEvaluateBalanceRules() && this.GetConstantRules().Any();
            }

            public bool DoEvaluateStandby()
            {
                return this.currentStage == TimeRuleStage.RegularStandby || this.currentStage == TimeRuleStage.BalanceAndConstantStandBy;
            }

            public void SetRuleStage(int ruleStage)
            {
                if (Enum.IsDefined(typeof(TimeRuleStage), ruleStage))
                    this.currentStage = (TimeRuleStage)ruleStage;
            }

            public void SetRuleEvaluated(TimeRule timeRule, List<TimeChunk> timeChunks)
            {
                if (timeRule == null)
                    return;

                if (!this.evaluatedRuleIds.Contains(timeRule.TimeRuleId))
                    this.evaluatedRuleIds.Add(timeRule.TimeRuleId);
                if (!timeChunks.IsNullOrEmpty() && !this.resultedRuleIds.Contains(timeRule.TimeRuleId))
                    this.resultedRuleIds.Add(timeRule.TimeRuleId);
            }

            public List<TimeScheduleTemplateBlock> FilterScheduleBlocks(List<TimeScheduleTemplateBlock> scheduleBlocks, bool? forceStandbyValue = null)
            {
                if (!this.UseStandbyStages)
                    return scheduleBlocks;

                bool doEvaluateStandby = forceStandbyValue ?? DoEvaluateStandby();
                return scheduleBlocks?.Where(i => i.IsStandby() == doEvaluateStandby).ToList() ?? new List<TimeScheduleTemplateBlock>();
            }

            public List<TimeBlock> FilterTimeBlocks(List<TimeBlock> timeBlocks)
            {
                if (!this.UseStandbyStages)
                    return timeBlocks;

                bool doEvaluateStandby = DoEvaluateStandby();
                return timeBlocks?.Where(i => i.CalculatedAsStandby == doEvaluateStandby || i.IsSickDuringStandbyTimeBlock).ToList() ?? new List<TimeBlock>();
            }

            #region Presence

            public List<TimeRule> GetPresenceRules()
            {
                return this.allTimeRules
                    .SeparateRulesByType(SoeTimeRuleType.Presence)
                    .SortRules();
            }

            public List<TimeRule> GetPresenceRules(int timeDeviationCauseId, TimeBlock timeBlock, bool useMultipleScheduleTypes, List<int> timeScheduleTypeIdsIsNotScheduleTime)
            {
                return this.GetPresenceRules()
                    .SeparateRulesByTimeDeviationCause(timeDeviationCauseId)
                    .SeparateRulesByTimeScheduleType(timeBlock, useMultipleScheduleTypes, timeScheduleTypeIdsIsNotScheduleTime)
                    .SeparateRulesByStage(DoEvaluateBalanceRules(), DoEvaluateStandby())
                    .SortRules();
            }

            public List<TimeRule> GetPresenceRulesResulted()
            {
                return this.GetPresenceRules()
                    .Where(tr => resultedRuleIds.Contains(tr.TimeRuleId))
                    .SortRules();
            }

            public bool HasAnyPresenceRuleResulted(int timeCodeId, bool excludeInconvinientWorkingHoursAndStandbyRules)
            {
                return this.GetPresenceRulesResulted().Any(tr => tr.TimeCodeId == timeCodeId && (!excludeInconvinientWorkingHoursAndStandbyRules || (!tr.IsInconvenientWorkHours && !tr.IsStandby)));
            }

            #endregion

            #region Absence

            public List<TimeRule> GetAbsenceRules()
            {
                return this.allTimeRules
                    .SeparateRulesByType(SoeTimeRuleType.Absence)
                    .SortRules();
            }

            public List<TimeRule> GetAbsenceRules(int timeDeviationCauseId, TimeBlock timeBlock, bool useMultipleScheduleTypes, bool doSeparateRulesByStage = true)
            {
                return this.GetAbsenceRules()
                    .Where(tr => !tr.Internal)
                    .SeparateRulesByTimeDeviationCause(timeDeviationCauseId)
                    .SeparateRulesByTimeScheduleType(timeBlock, useMultipleScheduleTypes)
                    .SeparateRulesByStage(DoEvaluateBalanceRules(), false, doSeparateRulesByStage)
                    .SortRules();
            }

            public bool ContainsAbsenceRule(int timeDeviationCauseId, TimeBlock timeBlock, bool useMultipleScheduleTypes, bool doSeparateRulesByStage = true)
            {
                return !GetAbsenceRules(timeDeviationCauseId, timeBlock, useMultipleScheduleTypes, doSeparateRulesByStage: doSeparateRulesByStage).IsNullOrEmpty();
            }

            #endregion

            #region Constant

            public List<TimeRule> GetConstantRules()
            {
                return this.allTimeRules.SeparateRulesByType(SoeTimeRuleType.Constant)
                    .SortRules();
            }

            public List<TimeRule> GetConstantRules(int timeDeviationCauseId, TimeBlock timeBlock, bool useMultipleScheduleTypes, List<int> timeScheduleTypeIdsIsNotScheduleTime)
            {
                return this.GetConstantRules()
                    .SeparateRulesByTimeDeviationCause(timeDeviationCauseId)
                    .SeparateRulesByTimeScheduleType(timeBlock, useMultipleScheduleTypes, timeScheduleTypeIdsIsNotScheduleTime)
                    .SeparateRulesByStage(null, DoEvaluateStandby())
                    .SortRules();
            }

            public List<TimeRule> GetConstantRulesResulted()
            {
                return this.GetConstantRules()
                    .Where(tr => resultedRuleIds.Contains(tr.TimeRuleId))
                    .SortRules();
            }

            public bool HasConstantRuleResulted(int timeRuleId)
            {
                return this.GetConstantRulesResulted().Any(tr => tr.TimeRuleId == timeRuleId);
            }

            #endregion

            #region Internal

            public TimeRule GetInternalRule(int timeCodeId, int timeDeviationCauseId, int dayTypeId)
            {
                return this.allTimeRules.Where(tr => tr.Internal && tr.TimeCodeId == timeCodeId && tr.ContainsRow(timeDeviationCauseId, dayTypeId, null, null)).SortRules().FirstOrDefault();
            }

            public void AddInternalRule(TimeRule rule)
            {
                if (rule != null && rule.Internal)
                    this.allTimeRules.Add(rule);
            }

            #endregion

            #endregion
        }

        private static class TimeEngineRuleEvaluator
        {
            #region Public methods

            /// <summary>
            /// Find valid time intervals for a given rule at a given time interval
            /// </summary>
            ///<param name="param">Parameter object</param>
            /// <returns>An array of TimeChunks with all time intervals where the rule can be applied</returns>
            public static List<TimeChunk> EvaluateRule(TimeEngineRuleEvaluatorParam param)
            {
                List<TimeChunk> timeChunks = new List<TimeChunk>();

                if (param == null || !param.IsValid)
                    return timeChunks;

                //Algorithm to find valid TimeChunks, nested binary search over minutes
                timeChunks = EvaluateRuleByBinarySearch(param);

                //Round start and stop time
                timeChunks = RoundTimeChunks(param, timeChunks);

                //Convert to Constant rule TimeChunks
                timeChunks = ConvertToConstantRuleTimeChunks(param, timeChunks);

                //Check TimeCodeMaxLength
                timeChunks = AdjustTimeChunkLength(param, timeChunks);

                return timeChunks.OrderBy(i => i.Start).ToList();
            }

            #endregion

            #region Private methods

            #region Binary search

            private static List<TimeChunk> EvaluateRuleByBinarySearch(TimeEngineRuleEvaluatorParam param)
            {
                #region Init

                List<TimeChunk> matchingTimeChunks = new List<TimeChunk>();
                List<TimeChunk> startSearchMatchingTimeChunks = new List<TimeChunk>();
                List<TimeChunk> stopSearchMatchingTimeChunks = new List<TimeChunk>();
                TimeChunk originalTimeChunk = null;

                int minStart = param.InputTimeChunk.TotalStartMinutes;
                int maxStop = param.InputTimeChunk.TotalStopMinutes;
                int lastFailedStop = 0;

                #endregion

                //Find time starting (Would be faster with larger increment, problem is that we would jump over valid spans)
                for (int currentStart = minStart; currentStart < maxStop; currentStart += 1)
                {
                    #region Start values

                    int currentMinStop = currentStart + 1; //Min. 1 minut
                    int currentStop = maxStop;

                    #endregion

                    #region Validations

                    //Start exceeds stop (should never happen)
                    if (currentStart >= currentStop)
                        break;

                    #endregion

                    //Find time ending 
                    while (currentStop >= currentMinStop)
                    {
                        #region Validations

                        //Check: Start not exceeds stop (should never happen)
                        if (currentStart >= currentStop)
                            break;

                        #endregion

                        #region Init TimeChunk

                        TimeSpan start = new TimeSpan(0, currentStart, 0);
                        TimeSpan stop = new TimeSpan(0, currentStop, 0);

                        //Init new TimeChunk with base values, incremented with current iteration values
                        TimeChunk timeChunk = new TimeChunk(start, stop, originalTimeChunk);

                        //Keep current 
                        if (originalTimeChunk == null)
                            originalTimeChunk = timeChunk;

                        #endregion

                        #region Evaluate TimeChunk

                        //Evaluate time intersection
                        bool result = EvaluateTimeChunk(param, timeChunk);
                        if (result)
                        {
                            #region Succeeded

                            //Add found range
                            stopSearchMatchingTimeChunks.Add(timeChunk);

                            //No point in decreasing further
                            if (currentStop == maxStop)
                                break;

                            #endregion
                        }
                        else
                        {
                            #region Failed

                            lastFailedStop = timeChunk.TotalStopMinutes;

                            #endregion
                        }

                        #endregion

                        #region Refine search

                        //Refine if previous stop match was found 
                        bool refineStopSearch = stopSearchMatchingTimeChunks.Any();
                        if (refineStopSearch)
                        {
                            if (lastFailedStop == 0)
                                break;

                            int prevStop = currentStop;
                            int lastSucceededStop = stopSearchMatchingTimeChunks.LastOrDefault()?.TotalStopMinutes ?? 0;

                            //Split stop
                            currentStop = BinarySearchSplit(lastSucceededStop, lastFailedStop);

                            //Break if no more ranges are available
                            if (prevStop == currentStop || currentMinStop >= currentStop)
                                break;
                        }
                        else
                        {
                            int prevStopMinutes = currentStop;
                            bool stopSearchDecrease = !result; //Determine scaling direction

                            //Split stop
                            currentStop = BinarySearchSplit(currentStart, currentStop, currentMinStop, maxStop, stopSearchDecrease);

                            //Break if no more ranges are available
                            if (prevStopMinutes == currentStop || maxStop <= currentStop)
                                break;
                        }

                        #endregion
                    }

                    #region Evaluate stop matches

                    //Check if temporary matches exists
                    if (stopSearchMatchingTimeChunks.Any())
                    {
                        //Add highest endtime to temporary start matches
                        startSearchMatchingTimeChunks.Add(stopSearchMatchingTimeChunks.OrderByDescending(tc => tc.Stop).FirstOrDefault(tc => tc.Start < tc.Stop));

                        //Only valid if no not rules are present
                        if (!param.TimeRuleContainsNotOperand)
                            return startSearchMatchingTimeChunks;

                        //Clear temporary matches
                        stopSearchMatchingTimeChunks.Clear();
                    }

                    #endregion
                }

                #region Evaluate start matches

                //Check if temporary matches exists
                if (startSearchMatchingTimeChunks.Any())
                    matchingTimeChunks.AddRange(startSearchMatchingTimeChunks.FindIntersectingTimeChunks());

                #endregion

                return matchingTimeChunks;
            }

            private static int BinarySearchSplit(int lastSucceededStopSearch, int lastFailedStopSearch)
            {
                return ((lastSucceededStopSearch + lastFailedStopSearch) / 2);
            }

            private static int BinarySearchSplit(int currentStart, int currentStop, int min, int max, bool decrease)
            {
                if (decrease)
                    currentStop = ((min + currentStop) / 2);
                else
                    currentStop = currentStop + ((max - currentStart) / 2);

                //Added to optimize low ranges which causes execution minute by minute
                if (currentStop < min)// + 14)
                    currentStop = min;

                return currentStop;
            }

            #endregion

            #region TimeChunk

            private static List<TimeChunk> ConvertToConstantRuleTimeChunks(TimeEngineRuleEvaluatorParam param, List<TimeChunk> timeChunks)
            {
                if (param.TimeRule.Type != (int)SoeTimeRuleType.Constant)
                    return timeChunks;

                List<TimeChunk> constantRuleTimeChunks = new List<TimeChunk>();

                int totalMinutes = timeChunks.GetMinutesFromTimeChunks();
                if (totalMinutes > 0)
                {
                    TimeSpan start = timeChunks.OrderBy(i => i.Start).Select(i => i.Start).FirstOrDefault();
                    TimeSpan stop = start.Add(new TimeSpan(0, param.TimeCode.MinutesByConstantRules, 0));
                    constantRuleTimeChunks.Add(new TimeChunk(start, stop));
                }

                return constantRuleTimeChunks;
            }

            private static List<TimeChunk> AdjustTimeChunkLength(TimeEngineRuleEvaluatorParam param, List<TimeChunk> timeChunks)
            {
                if (timeChunks.IsNullOrEmpty())
                    return new List<TimeChunk>();

                if (param.TimeRule.StandardMinutes.HasValue)
                    return AdjustTimeChunksAfterStandardMinutes(timeChunks, param.TimeRule.StandardMinutes.Value);
                else if (param.TimeRule.UseMaxAsStandard) //Temp solution for backward-compability with ugly solution
                    return AdjustTimeChunksAfterStandardMinutes(timeChunks, param.TimeRule.TimeCodeMaxLength ?? 0);
                else if (param.TimeRule.TimeCodeMaxLength.HasValue)
                    return AdjustTImeChunksAfterMaxMinutes(timeChunks, param.TimeRule.TimeCodeMaxLength.Value);
                else
                    return timeChunks;
            }

            private static List<TimeChunk> AdjustTimeChunksAfterStandardMinutes(List<TimeChunk> timeChunks, int standardMinutes)
            {
                if (timeChunks.IsNullOrEmpty() || standardMinutes == 0)
                    return new List<TimeChunk>();

                TimeChunk timeChunk = timeChunks[0];
                if (timeChunk.IntervallMinutes != standardMinutes)
                {
                    int currentMinutes = timeChunk.IntervallMinutes;
                    if (currentMinutes < standardMinutes)
                        timeChunk.IncreaseStopTime(standardMinutes - currentMinutes);
                    else
                        timeChunk.DecreaseStopTime(currentMinutes - standardMinutes);
                }

                return timeChunks.Where(i => i.IntervallMinutes > 0).ToList();
            }

            private static List<TimeChunk> AdjustTImeChunksAfterMaxMinutes(List<TimeChunk> timeChunks, int maxMinutes)
            {
                if (timeChunks.IsNullOrEmpty() || maxMinutes == 0)
                    return new List<TimeChunk>();

                int totalMinutes = timeChunks.Sum(i => i.IntervallMinutes);
                if (totalMinutes > maxMinutes)
                {
                    int handledMinutes = 0;

                    foreach (var timeChunk in timeChunks.OrderBy(i => i.Start))
                    {
                        int currentMinutes = timeChunk.IntervallMinutes;

                        int totalRestMinutes = (currentMinutes + handledMinutes) - maxMinutes;
                        if (totalRestMinutes > 0)
                            currentMinutes -= totalRestMinutes;

                        int currentRestMinutes = timeChunk.IntervallMinutes - currentMinutes;
                        if (currentRestMinutes > 0)
                            timeChunk.DecreaseStopTime(currentRestMinutes);

                        handledMinutes += timeChunk.IntervallMinutes;
                    }
                }

                return timeChunks.Where(i => i.IntervallMinutes > 0).ToList();
            }

            private static List<TimeChunk> RoundTimeChunks(TimeEngineRuleEvaluatorParam param, List<TimeChunk> timeChunks)
            {
                if (timeChunks.IsNullOrEmpty())
                    return new List<TimeChunk>();

                foreach (TimeChunk timeChunk in timeChunks)
                {
                    int minutes = param.TimeCode.RoundTimeCode(timeChunk.IntervallMinutes);
                    if (param.TimeCode.RoundStartTime)
                        timeChunk.Start = timeChunk.Stop.Add(new TimeSpan(0, -minutes, 0));
                    else
                        timeChunk.Stop = timeChunk.Start.Add(new TimeSpan(0, minutes, 0));

                    if (param.TimeRule.UseAdjustStartToTimeBlockStart)
                    {
                        TimeBlock timeBlock = param.PresenceTimeBlocks.Get(CalendarUtility.GetDateTime(timeChunk.Start));
                        if (timeBlock != null)
                        {
                            int timeChunkMinutes = timeChunk.IntervallMinutes;
                            timeChunk.Start = CalendarUtility.GetTimeSpanFromDateTime(timeBlock.StartTime);
                            timeChunk.Stop = CalendarUtility.GetTimeSpanFromDateTime(timeBlock.StartTime.AddMinutes(timeChunkMinutes));
                        }
                    }
                }

                return timeChunks;
            }

            private static bool EvaluateTimeChunk(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                try
                {
                    bool result = false;

                    //Evaluate start and stop parts of rules
                    result = EvaluateRuleExpression(param, timeChunk, param.StartExpression, true);
                    if (!result)
                        return false;

                    result = EvaluateRuleExpression(param, timeChunk, param.StopExpression, false);
                    if (!result)
                        return false;

                    //if both are true then the chunk combination is valid for the rule
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }

                return false;
            }

            private static bool EvaluateMinutes(TimeRuleOperand operand, int leftMinutes, int rightMinutes, bool disapprove = false)
            {
                var comparisor = operand.ComparisonOperator;

                if (disapprove && operand.ComparisonOperator == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThanOrEqualsTo)
                    comparisor = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThan;

                if (comparisor == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThan)
                    return leftMinutes < rightMinutes;
                else if (comparisor == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThanOrEqualsTo)
                    return leftMinutes <= rightMinutes;
                else if (comparisor == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorEqualsTo)
                    return leftMinutes == rightMinutes;
                else if (comparisor == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThanOrEqualsTo)
                    return leftMinutes >= rightMinutes;
                else if (comparisor == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThan)
                    return leftMinutes > rightMinutes;
                return false;
            }

            private static int SummarizeScheduleTime(List<TimeScheduleTemplateBlock> scheduleBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime, TimeChunk timeChunk = null, int? timeCodeId = null, bool doHandleBreakAsSchedule = false)
            {
                List<TimeChunk> scheduleTimeChunks = new List<TimeChunk>();
                List<TimeChunk> breakTimeChunks = new List<TimeChunk>();

                bool restrictOnTimeCodeId = timeCodeId.HasValue;
                bool subtractBreaks = !doHandleBreakAsSchedule && !restrictOnTimeCodeId;
                var schedulByDateGrouping = scheduleBlocks.GroupBy(b => b.Date);
                bool isPeriod = schedulByDateGrouping.Count() > 1;

                foreach (var scheduleBlocksByDate in schedulByDateGrouping)
                {
                    List<TimeScheduleTemplateBlock> scheduleBlocksForDate = scheduleBlocksByDate.ToList();
                    foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocksForDate.GetWork())
                    {
                        //Disapprove "not schedule time"
                        if (!scheduleBlock.IsScheduleTime(timeScheduleTypeIdsIsNotScheduleTime))
                            continue;
                        //Disapprove when restricted by TimeCode
                        if (restrictOnTimeCodeId && timeCodeId.Value != scheduleBlock.TimeCodeId)
                            continue;

                        if (timeChunk != null)
                        {
                            DateTime startTime = timeChunk.GetStartTime();
                            DateTime stopTime = timeChunk.GetStopTime();
                            if (!IsPresenceWithinSchedule(startTime, stopTime, scheduleBlock.StartTime, scheduleBlock.StopTime, ref startTime, ref stopTime))
                                continue;
                        }

                        //Schedule
                        scheduleTimeChunks.Add(new TimeChunk(scheduleBlock.StartTime, scheduleBlock.StopTime));

                        //Breaks
                        if (subtractBreaks)
                        {
                            foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBlocksForDate.GetBreaks())
                            {
                                //Breaks can be partly in block if they are linked
                                int overlappingMinutes = CalendarUtility.GetOverlappingMinutes(scheduleBreak.StartTime, scheduleBreak.StopTime, scheduleBlock.StartTime, scheduleBlock.StopTime);
                                if (overlappingMinutes > 0)
                                {
                                    int breakMinutes = (int)scheduleBreak.StopTime.Subtract(scheduleBreak.StartTime).TotalMinutes;
                                    if (overlappingMinutes == breakMinutes) //Break is completely in block
                                        breakTimeChunks.Add(new TimeChunk(scheduleBreak.StartTime, scheduleBreak.StopTime));
                                    else if (CalendarUtility.IsNewOverlappingCurrentStart(scheduleBreak.StartTime, scheduleBreak.StopTime, scheduleBlock.StartTime, scheduleBlock.StopTime)) //Break is partly in start of block
                                        breakTimeChunks.Add(new TimeChunk(scheduleBlock.StartTime, scheduleBlock.StartTime.AddMinutes(overlappingMinutes)));
                                    else //Break is partly in stop of block
                                        breakTimeChunks.Add(new TimeChunk(scheduleBlock.StopTime.AddMinutes(-overlappingMinutes), scheduleBlock.StopTime));
                                }
                            }
                        }
                    }
                }

                int totalScheduleMinutes = scheduleTimeChunks.GetMinutesFromTimeChunks(!isPeriod);
                int totalBreakMinutes = breakTimeChunks.GetMinutesFromTimeChunks(!isPeriod);
                return totalScheduleMinutes - totalBreakMinutes;
            }

            private static int SummarizeScheduleTime(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk = null, int? timeCodeId = null, bool doHandleBreakAsSchedule = false)
            {
                return SummarizeScheduleTime(param.ScheduleBlocks, param.TimeScheduleTypeIdsIsNotScheduleTime, timeChunk, timeCodeId, doHandleBreakAsSchedule);
            }

            private static int SummarizeScheduleTimeInOvertimePeriod(TimeEngineRuleEvaluatorParam param)
            {
                return SummarizeScheduleTime(param.ScheduleBlocksInOvertimePeriod, param.TimeScheduleTypeIdsIsNotScheduleTime);
            }

            private static int SummarizeOvertimeInOvertimePeriod(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                if (param.ScheduleIn == param.ScheduleOut && param.EmployeeGroup?.CandidateForOvertimeOnZeroDayExcluded == true)
                    return 0;
                return SummarizePresenceBeforeSchedule(param, timeChunk, onlyOvertime: true) + SummarizePresenceAfterSchedule(param, timeChunk, onlyOvertime: true, skipZeroDayForOvertime: true);
            }

            private static int SummarizeSchedulePlusOvertimeInOvertimePeriod(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizeScheduleTimeInOvertimePeriod(param) + SummarizeOvertimeInOvertimePeriod(param, timeChunk);
            }

            private static int SummarizeTransactionTimeForTimeCode(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, int timeCodeId)
            {
                List<TimeChunk> timeChunks = new List<TimeChunk>();

                if (param.TimeRule.TimeCodeId == timeCodeId)
                    timeChunks.Add(timeChunk);

                TimeSpan presenceStart = param.PresenceStartCurrent;
                TimeSpan presenceStop = param.PresenceStopCurrent;
                bool hasSetPrecenseStart = false;

                foreach (TimeCodeTransaction previousTimeCodeTransaction in param.PreviousTimeCodeTransactions)
                {
                    if (previousTimeCodeTransaction.TimeCodeId != timeCodeId)
                        continue;

                    DateTime startTime = previousTimeCodeTransaction.Start;
                    DateTime stopTime = previousTimeCodeTransaction.Stop;
                    if (!FilterTimesOnTimeChunk(param, timeChunk, ref startTime, ref stopTime))
                        continue;

                    timeChunks.Add(new TimeChunk(startTime, stopTime));

                    EvaluateCurrentPresenceStartStopTime(param, previousTimeCodeTransaction.Start, previousTimeCodeTransaction.Stop, ref presenceStart, ref presenceStop, ref hasSetPrecenseStart);
                }

                UpdateCurrentPrecenseStartStopTime(param, presenceStart, presenceStop);

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizePresence(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, bool onlyPayed = false)
            {
                List<TimeChunk> timeChunks = new List<TimeChunk>();

                TimeSpan presenceStart = param.PresenceStartCurrent;
                TimeSpan presenceStop = param.PresenceStopCurrent;
                bool hasSetPrecenseStart = false;

                foreach (TimeBlock presenceTimeBlock in param.FilterPresenceTimeBlocks(onlyPayed))
                {
                    DateTime startTime = presenceTimeBlock.StartTime;
                    DateTime stopTime = presenceTimeBlock.StopTime;
                    if (!FilterTimesOnTimeChunk(param, timeChunk, ref startTime, ref stopTime))
                        continue;

                    timeChunks.Add(new TimeChunk(startTime, stopTime));

                    EvaluateCurrentPresenceStartStopTime(param, presenceTimeBlock.StartTime, presenceTimeBlock.StopTime, ref presenceStart, ref presenceStop, ref hasSetPrecenseStart);
                }

                UpdateCurrentPrecenseStartStopTime(param, presenceStart, presenceStop);

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizePresenceWithinSchedule(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                //Disapprove TimeChunk not within schedule
                if (!IsPresenceWithinSchedule(timeChunk.Start, timeChunk.Stop, param.ScheduleIn, param.ScheduleOut))
                    return 0;

                List<TimeChunk> timeChunks = new List<TimeChunk>();

                if (SummarizePresenceInScheduleHole(param, timeChunk) == timeChunk.IntervallMinutes)
                    return 0;

                foreach (TimeBlock presenceTimeBlock in param.FilterPresenceTimeBlocks())
                {
                    //Disapprove "not schedule time"
                    if (!presenceTimeBlock.IsScheduleTime(param.TimeScheduleTypeIdsIsNotScheduleTime))
                        continue;
                    //Disapprove presence before schedule
                    if (presenceTimeBlock.StopTime < param.ScheduleIn)
                        continue;
                    //Disapprove presence after schedule
                    if (presenceTimeBlock.StartTime > param.ScheduleOut)
                        continue;

                    DateTime startTime = presenceTimeBlock.StartTime;
                    DateTime stopTime = presenceTimeBlock.StopTime;
                    if (!IsPresenceWithinSchedule(presenceTimeBlock.StartTime, presenceTimeBlock.StopTime, param.ScheduleIn, param.ScheduleOut, ref startTime, ref stopTime))
                        continue;

                    timeChunks.Add(new TimeChunk(startTime, stopTime));
                }

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizePresenceBeforeSchedule(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, bool onlyPayed = false, bool onlyOvertime = false)
            {
                //Disapprove TimeChunk not before schedule
                if (!onlyOvertime && !IsPresenceBeforeSchedule(timeChunk.Start, timeChunk.Stop, param.ScheduleIn, param.ScheduleOut))
                    return 0;

                List<TimeChunk> timeChunks = new List<TimeChunk>();

                foreach (var presenceTimeBlocksByDate in param.FilterPresenceTimeBlocks(onlyPayed, onlyOvertime).GroupBy(tb => tb.TimeBlockDate.Date))
                {
                    DateTime? scheduleIn = param.GetScheduleInForDate(presenceTimeBlocksByDate.Key, onlyOvertime, param.TimeScheduleTypeIdsIsNotScheduleTime);
                    DateTime? scheduleOut = param.GetScheduleOutForDate(presenceTimeBlocksByDate.Key, onlyOvertime, param.TimeScheduleTypeIdsIsNotScheduleTime);

                    foreach (TimeBlock presenceTimeBlock in presenceTimeBlocksByDate)
                    {
                        //Disapprove "not schedule time" (Do not disapprove for overtime, because overtime summarizes all time outside schedule)
                        if (!onlyOvertime && !presenceTimeBlock.IsScheduleTime(param.TimeScheduleTypeIdsIsNotScheduleTime))
                            continue;

                        DateTime startTime = presenceTimeBlock.StartTime;
                        DateTime stopTime = presenceTimeBlock.StopTime;
                        if (scheduleIn.HasValue && scheduleOut.HasValue)
                        {
                            if (presenceTimeBlock.StartTime > scheduleIn.Value)
                                continue;
                            if (!IsPresenceBeforeSchedule(presenceTimeBlock.StartTime, presenceTimeBlock.StopTime, scheduleIn.Value, scheduleOut.Value, ref startTime, ref stopTime))
                                continue;
                        }
                        if (presenceTimeBlock.IsOnDate(param.Date) && !FilterTimesOnTimeChunk(param, timeChunk, ref startTime, ref stopTime))
                            continue;
                        timeChunks.Add(new TimeChunk(startTime, stopTime));
                    }
                }

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizePresenceAfterSchedule(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, bool onlyPayed = false, bool onlyOvertime = false, bool skipZeroDayForOvertime = false)
            {
                //Disapprove TimeChunk not after schedule
                if (!onlyOvertime && !IsPresenceAfterSchedule(timeChunk.Start, timeChunk.Stop, param.ScheduleIn, param.ScheduleOut))
                    return 0;

                List<TimeChunk> timeChunks = new List<TimeChunk>();

                foreach (var presenceTimeBlocksByDate in param.FilterPresenceTimeBlocks(onlyPayed, onlyOvertime).GroupBy(tb => tb.TimeBlockDate.Date))
                {
                    DateTime? scheduleIn = param.GetScheduleInForDate(presenceTimeBlocksByDate.Key, onlyOvertime, param.TimeScheduleTypeIdsIsNotScheduleTime);
                    DateTime? scheduleOut = param.GetScheduleOutForDate(presenceTimeBlocksByDate.Key, onlyOvertime, param.TimeScheduleTypeIdsIsNotScheduleTime);

                    foreach (TimeBlock presenceTimeBlock in presenceTimeBlocksByDate)
                    {
                        //Disapprove "not schedule time" (Do not disapprove for overtime, because overtime summarizes all time outside schedule)
                        if (!onlyOvertime && !presenceTimeBlock.IsScheduleTime(param.TimeScheduleTypeIdsIsNotScheduleTime))
                            continue;
                        if (onlyOvertime && skipZeroDayForOvertime && !scheduleIn.HasValue && !scheduleOut.HasValue)
                            continue;

                        DateTime startTime = presenceTimeBlock.StartTime;
                        DateTime stopTime = presenceTimeBlock.StopTime;
                        if (scheduleIn.HasValue && scheduleOut.HasValue)
                        {
                            if (presenceTimeBlock.StopTime < scheduleOut.Value)
                                continue;
                            if (!IsPresenceAfterSchedule(presenceTimeBlock.StartTime, presenceTimeBlock.StopTime, scheduleIn.Value, scheduleOut.Value, ref startTime, ref stopTime))
                                continue;
                        }
                        if (presenceTimeBlock.IsOnDate(param.Date) && !FilterTimesOnTimeChunk(param, timeChunk, ref startTime, ref stopTime))
                            continue;
                        timeChunks.Add(new TimeChunk(startTime, stopTime));
                    }
                }

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizePayed(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizePresence(param, timeChunk, onlyPayed: true);
            }

            private static int SummarizePayedBeforeSchedule(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizePresenceBeforeSchedule(param, timeChunk, true);
            }

            private static int SummarizePayedBeforeSchedulePlusScheduleTime(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizePayedBeforeSchedule(param, timeChunk) + SummarizeScheduleTime(param);
            }

            private static int SummarizePayedAfterSchedule(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizePresenceAfterSchedule(param, timeChunk, true);
            }

            private static int SummarizePayedAfterSchedulePlusScheduleTime(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                return SummarizePayedAfterSchedule(param, timeChunk) + SummarizeScheduleTime(param);
            }

            private static int SummarizePresenceInScheduleHole(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk)
            {
                //Disapprove TimeChunk before schedule
                if (CalendarUtility.GetDateTime(timeChunk.Stop) <= param.ScheduleIn)
                    return 0;
                //Disapprove TimeChunk after schedule
                if (CalendarUtility.GetDateTime(timeChunk.Start) >= param.ScheduleOut)
                    return 0;

                List<TimeChunk> timeChunks = new List<TimeChunk>();

                foreach (TimeRange scheduleHoles in param.GetScheduleHoles())
                {
                    DateTime startTime = CalendarUtility.GetDateTime(timeChunk.Start);
                    DateTime stopTime = CalendarUtility.GetDateTime(timeChunk.Stop);
                    if (!IsPresenceWithinSchedule(startTime, stopTime, scheduleHoles.StartTime, scheduleHoles.StopTime, ref startTime, ref stopTime))
                        continue;

                    timeChunks.Add(new TimeChunk(startTime, stopTime));
                }

                return timeChunks.GetMinutesFromTimeChunks();
            }

            private static int SummarizeFulltimeWeek(TimeEngineRuleEvaluatorParam param)
            {
                return param.IsEmploymentPercentFulltime() ? SummarizeScheduleTimeInOvertimePeriod(param) : param.GetFullTimeWorkWeekTime();
            }

            private static void EvaluateCurrentPresenceStartStopTime(TimeEngineRuleEvaluatorParam param, DateTime newStartTime, DateTime newStopTime, ref TimeSpan presenceStart, ref TimeSpan presenceStop, ref bool hasSetPrecenseStart)
            {
                //Take last approved precense stop
                if (param.IsForward)
                {
                    presenceStop = CalendarUtility.GetTimeSpanFromDateTime(newStopTime);
                }

                //Take first approved precense start
                if (param.IsBackward && !hasSetPrecenseStart)
                {
                    presenceStart = CalendarUtility.GetTimeSpanFromDateTime(newStartTime);

                    //Handle that we are looping forward, but rule is backward
                    hasSetPrecenseStart = true;
                }
            }

            private static void UpdateCurrentPrecenseStartStopTime(TimeEngineRuleEvaluatorParam param, TimeSpan presenceStart, TimeSpan presenceStop)
            {
                if (param.IsForward && presenceStop != param.PresenceStopCurrent)
                    param.PresenceStopCurrent = presenceStop;
                if (param.IsBackward && presenceStart != param.PresenceStartCurrent)
                    param.PresenceStartCurrent = presenceStart;
            }

            private static bool FilterTimesOnTimeChunk(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, ref DateTime startTime, ref DateTime stopTime)
            {
                if (param == null)
                    return false;

                if (timeChunk != null)
                {
                    int startTimeMinutes = (startTime.Hour * 60 + startTime.Minute);
                    int stopTimeMinutes = (stopTime.Hour * 60 + stopTime.Minute);

                    //Handle midnight
                    bool isOverMidnight = stopTime.Date == CalendarUtility.DATETIME_DEFAULT.AddDays(1);
                    if (isOverMidnight)
                        stopTimeMinutes += 24 * 60;

                    if (param.IsForward)
                    {
                        if (startTimeMinutes > (int)timeChunk.Stop.TotalMinutes)
                            return false;
                        if (stopTimeMinutes > (int)timeChunk.Stop.TotalMinutes)
                        {
                            stopTime = new DateTime(stopTime.Year, stopTime.Month, stopTime.Day).Add(timeChunk.Stop);
                            if (isOverMidnight)
                                stopTime = stopTime.AddDays(-1);
                        }
                    }
                    else if (param.IsBackward)
                    {
                        if (stopTimeMinutes < (int)timeChunk.Start.TotalMinutes)
                            return false;
                        if (startTimeMinutes < (int)timeChunk.Start.TotalMinutes)
                            startTime = new DateTime(stopTime.Year, stopTime.Month, stopTime.Day).Add(timeChunk.Start);
                    }
                }

                return stopTime > startTime;
            }

            public static bool IsPresenceWithinSchedule(TimeSpan presenceStart, TimeSpan precenseStop, DateTime scheduleIn, DateTime scheduleOut)
            {
                DateTime startTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
                return IsPresenceWithinSchedule(CalendarUtility.GetDateTime(presenceStart), CalendarUtility.GetDateTime(precenseStop), scheduleIn, scheduleOut, ref startTime, ref stopTime);
            }

            public static bool IsPresenceWithinSchedule(DateTime presenceStart, DateTime precenseStop, DateTime scheduleIn, DateTime scheduleOut, ref DateTime startTime, ref DateTime stopTime)
            {
                if (CalendarUtility.IsNewOverlappingCurrentStart(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Overlaps to the left
                    startTime = scheduleIn;
                    stopTime = precenseStop;
                }
                else if (CalendarUtility.IsCurrentOverlappedByNew(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Contains schedule
                    startTime = scheduleIn;
                    stopTime = scheduleOut;
                }
                else if (CalendarUtility.IsNewOverlappingCurrentStop(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Overlaps to the right
                    startTime = presenceStart;
                    stopTime = scheduleOut;
                }
                else if (CalendarUtility.IsNewOverlappedByCurrent(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Within schedule
                    startTime = presenceStart;
                    stopTime = precenseStop;
                }
                else
                {
                    //Invalid
                    startTime = stopTime;
                }

                return stopTime > startTime;
            }

            public static bool IsPresenceBeforeSchedule(TimeSpan presenceStart, TimeSpan precenseStop, DateTime scheduleIn, DateTime scheduleOut)
            {
                DateTime startTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
                return IsPresenceBeforeSchedule(CalendarUtility.GetDateTime(presenceStart), CalendarUtility.GetDateTime(precenseStop), scheduleIn, scheduleOut, ref startTime, ref stopTime);
            }

            public static bool IsPresenceBeforeSchedule(DateTime presenceStart, DateTime precenseStop, DateTime scheduleIn, DateTime scheduleOut, ref DateTime startTime, ref DateTime stopTime)
            {
                if (CalendarUtility.IsNewBeforeCurrentStart(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Before left
                    startTime = presenceStart;
                    stopTime = precenseStop;
                }
                else if (CalendarUtility.IsNewOverlappingCurrentStart(presenceStart, precenseStop, scheduleIn, scheduleOut))
                {
                    //Overlaps to the left
                    startTime = presenceStart;
                    stopTime = scheduleIn;
                }
                else
                {
                    //Invalid
                    startTime = stopTime;
                }

                return stopTime > startTime;
            }

            public static bool IsPresenceAfterSchedule(TimeSpan presenceStart, TimeSpan precenseStop, DateTime scheduleIn, DateTime scheduleOut)
            {
                DateTime startTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
                return IsPresenceAfterSchedule(CalendarUtility.GetDateTime(presenceStart), CalendarUtility.GetDateTime(precenseStop), scheduleIn, scheduleOut, ref startTime, ref stopTime);
            }

            public static bool IsPresenceAfterSchedule(DateTime presenceStart, DateTime presenseStop, DateTime scheduleIn, DateTime scheduleOut, ref DateTime startTime, ref DateTime stopTime)
            {
                if (CalendarUtility.IsNewAfterCurrentStop(presenceStart, presenseStop, scheduleIn, scheduleOut))
                {
                    //After left
                    startTime = presenceStart;
                    stopTime = presenseStop;
                }
                else if (CalendarUtility.IsNewOverlappingCurrentStop(presenceStart, presenseStop, scheduleIn, scheduleOut))
                {
                    //Overlaps to the right
                    startTime = scheduleOut;
                    stopTime = presenseStop;
                }
                else
                {
                    //Invalid
                    startTime = stopTime;
                }

                return stopTime > startTime;
            }

            #endregion

            #region TimeRuleExpression

            private static bool EvaluateRuleExpression(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleExpression expression, bool isStartExpression)
            {
                if (expression?.TimeRuleOperand == null)
                    return false;

                bool result;
                List<RuleEvaluationResult> evaluationResults = new List<RuleEvaluationResult>();

                foreach (TimeRuleOperand operand in expression.TimeRuleOperand.OrderBy(o => o.OrderNbr))
                {
                    if (operand.TimeRuleExpressionRecursive != null)
                    {
                        #region Nested

                        result = EvaluateRuleExpression(param, timeChunk, operand.TimeRuleExpressionRecursive, isStartExpression);

                        #endregion
                    }
                    else
                    {
                        switch (operand.OperatorType)
                        {
                            case (int)SoeTimeRuleOperatorType.TimeRuleOperatorAnd:
                                #region And

                                evaluationResults.Add(RuleEvaluationResult.and);
                                if (evaluationResults.Contains(RuleEvaluationResult.falsified))
                                    return false;

                                continue;
                            #endregion
                            case (int)SoeTimeRuleOperatorType.TimeRuleOperatorOr:
                                #region Or

                                evaluationResults.Add(RuleEvaluationResult.or);
                                if (evaluationResults.Contains(RuleEvaluationResult.succeeded))
                                    return true;

                                continue;
                            #endregion
                            default:
                                #region Evaluate

                                result = EvaluateRuleOperand(param, timeChunk, operand, isStartExpression);
                                if (!result && evaluationResults.Contains(RuleEvaluationResult.and))
                                    return false;
                                else if (result && evaluationResults.Contains(RuleEvaluationResult.or))
                                    return true;

                                break;
                                #endregion
                        }
                    }

                    evaluationResults.Add(GetRuleEvaluationResult(result));
                }

                return ParseRuleEvaluationResult(evaluationResults);
            }

            #endregion

            #region TimeRuleOperand

            private static bool EvaluateRuleOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, bool isStartExpression)
            {
                if (operand == null || param.TimeRule == null)
                    return false;
                if (param.TimeRule.UseBreakIfAnyFailed && param.TimeRule.HasFailed)
                    return false;

                bool result = false;

                switch (operand.OperatorType)
                {
                    case (int)SoeTimeRuleOperatorType.TimeRuleOperatorBalance:
                        result = EvaluateBalanceOperand(param, timeChunk, operand, isStartExpression);
                        break;
                    case (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn:
                        result = EvaluateScheduleInOperand(param, timeChunk, operand, isStartExpression);
                        break;
                    case (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut:
                        result = EvaluateScheduleOutOperand(param, timeChunk, operand, isStartExpression);
                        break;
                    case (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock:
                        result = EvaluateClockOperand(timeChunk, operand, isStartExpression);
                        break;
                    case (int)SoeTimeRuleOperatorType.TimeRuleOperatorNot:
                        result = EvaluateNotOperand(param, timeChunk, operand);
                        break;
                }

                if (!result)
                    param.TimeRule.HasFailed = true;

                return result;
            }

            /// <summary>
            /// Returns true if time of summarized transactions of operand determined type exceeds schedule time of operand determined type
            /// </summary>
            /// <param name="param"></param>
            /// <param name="timeChunk"></param>
            /// <param name="operand"></param>
            /// <param name="isStartExpression"></param>
            /// <returns>True if time of summarized transactions of operand determined type exceeds schedule time of operand determined type</returns>
            private static bool EvaluateBalanceOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, bool isStartExpression)
            {
                if (!param.HasTimeBlocks)
                    return false;

                //LeftValueType
                if (!TrySummarizeOperandLeftValue(param, timeChunk, operand, out int leftMinutes))
                    return false;

                //RightValueType
                int rightMinutes = 0;
                if (operand.RightValueType.HasValue && !TrySummarizeOperandRightValue(param, operand, out rightMinutes))
                    return false;

                //Add time offset to schedule
                rightMinutes += operand.Minutes;

                //Evaluate
                if (isStartExpression)
                    return EvaluateStartExpressionOperand(param, timeChunk, operand, leftMinutes, rightMinutes);
                else
                    return EvaluateStopExpressionOperand(param, timeChunk, operand, leftMinutes, rightMinutes);
            }

            private static bool TrySummarizeOperandLeftValue(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, out int minutes)
            {
                bool result = true;
                minutes = 0;

                switch (operand.LeftValueType)
                {
                    case (int)SoeTimeRuleValueType.TimeCodeLeft:
                        if (operand.LeftValueId.HasValue)
                            minutes = SummarizeTransactionTimeForTimeCode(param, timeChunk, operand.LeftValueId.Value);
                        break;
                    case (int)SoeTimeRuleValueType.ScheduleLeft:
                        minutes = SummarizeScheduleTime(param);
                        break;
                    case (int)SoeTimeRuleValueType.ScheduleAndBreakLeft:
                        minutes = SummarizeScheduleTime(param, doHandleBreakAsSchedule: true);
                        break;
                    case (int)SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod:
                        minutes = SummarizeSchedulePlusOvertimeInOvertimePeriod(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.Presence:
                        minutes = SummarizePresence(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PresenceWithinSchedule:
                        minutes = SummarizePresenceWithinSchedule(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PresenceBeforeSchedule:
                        minutes = SummarizePresenceBeforeSchedule(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PresenceAfterSchedule:
                        minutes = SummarizePresenceAfterSchedule(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PresenceInScheduleHole:
                        minutes = SummarizePresenceInScheduleHole(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.Payed:
                        minutes = SummarizePayed(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PayedBeforeSchedule:
                        minutes = SummarizePayedBeforeSchedule(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PayedBeforeSchedulePlusSchedule:
                        minutes = SummarizePayedBeforeSchedulePlusScheduleTime(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PayedAfterSchedule:
                        minutes = SummarizePayedAfterSchedule(param, timeChunk);
                        break;
                    case (int)SoeTimeRuleValueType.PayedAfterSchedulePlusSchedule:
                        minutes = SummarizePayedAfterSchedulePlusScheduleTime(param, timeChunk);
                        break;
                    default:
                        //Rule is incorrect
                        result = false;
                        break;
                }

                return result;
            }

            private static bool TrySummarizeOperandRightValue(TimeEngineRuleEvaluatorParam param, TimeRuleOperand operand, out int minutes)
            {
                bool result = true;
                minutes = 0;

                switch (operand.RightValueType)
                {
                    case (int)SoeTimeRuleValueType.TimeCodeRight:
                        if (operand.RightValueId.HasValue && operand.RightValueId.Value > 0)
                            minutes = SummarizeScheduleTime(param, timeCodeId: operand.RightValueId.Value);
                        break;
                    case (int)SoeTimeRuleValueType.ScheduleRight:
                        minutes = SummarizeScheduleTime(param);
                        break;
                    case (int)SoeTimeRuleValueType.FulltimeWeek:
                        minutes = SummarizeFulltimeWeek(param);
                        break;
                    default:
                        result = false; //Rule is incorrect
                        break;
                }

                return result;
            }

            private static bool EvaluateStartExpressionOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, int leftMinutes, int rightMinutes)
            {
                if (OperandHasLeftValueType(operand, SoeTimeRuleValueType.ScheduleLeft, SoeTimeRuleValueType.ScheduleAndBreakLeft))
                {
                    //For ScheduleTime only Yes or No is relevant
                    return EvaluateMinutes(operand, leftMinutes, rightMinutes);
                }
                else if (!EvaluateMinutes(operand, leftMinutes, rightMinutes))
                {
                    //Disapprove
                    return false;
                }

                return ValidateExpressionValue(param, timeChunk, operand, leftMinutes, rightMinutes, true);
            }

            private static bool EvaluateStopExpressionOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, int leftMinutes, int rightMinutes)
            {
                if (OperandHasLeftValueType(operand, SoeTimeRuleValueType.ScheduleLeft, SoeTimeRuleValueType.ScheduleAndBreakLeft))
                {
                    //For ScheduleTime only Yes or No is relevant
                    return EvaluateMinutes(operand, leftMinutes, rightMinutes);
                }
                else if (OperandHasLeftValueType(operand, SoeTimeRuleValueType.TimeCodeLeft))
                {
                    //Disapprove all left > right (Ex: Tidkod > x)
                    if (EvaluateMinutes(operand, leftMinutes, rightMinutes, true))
                        return false;

                }
                else if (OperandHasLeftValueType(operand, SoeTimeRuleValueType.Presence, SoeTimeRuleValueType.PresenceBeforeSchedule, SoeTimeRuleValueType.PresenceAfterSchedule, SoeTimeRuleValueType.Payed, SoeTimeRuleValueType.PayedBeforeSchedule, SoeTimeRuleValueType.PayedBeforeSchedulePlusSchedule, SoeTimeRuleValueType.PayedAfterSchedule, SoeTimeRuleValueType.PayedAfterSchedulePlusSchedule, SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod))
                {
                    if (leftMinutes == 0)
                    {
                        //Dispprove all if no left (Ex: PresenceBeforeSchedule but left is 0)
                        if (!EvaluateMinutes(operand, leftMinutes, rightMinutes, true))
                            return false;
                    }
                    else
                    {
                        //Disapprove all left > right
                        if (EvaluateMinutes(operand, leftMinutes, rightMinutes, true))
                            return false;
                    }
                }
                else
                {
                    if (leftMinutes == 0)
                    {
                        //Dispprove all if no left (Ex: PresenceWithinSchedule but left is 0)
                        if (!EvaluateMinutes(operand, leftMinutes, rightMinutes))
                            return false;
                    }
                    else
                    {
                        //Approve all left > right (Ex: left > (right + x)
                        if (!EvaluateMinutes(operand, leftMinutes, rightMinutes))
                            return true;
                    }
                }

                return ValidateExpressionValue(param, timeChunk, operand, leftMinutes, rightMinutes, false);
            }

            private static bool ValidateExpressionValue(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, int leftMinutes, int rightMinutes, bool isStartExpression)
            {
                int diffMinutes = leftMinutes - rightMinutes;

                if (diffMinutes > 0 && rightMinutes == 0)
                    return true;
                if (param.TimeRule.Type == (int)SoeTimeRuleType.Constant && isStartExpression)
                    return diffMinutes > 0;

                TimeSpan compareValue;
                TimeSpan diff = new TimeSpan(0, diffMinutes, 0);

                if (isStartExpression)
                {
                    if (param.IsForward)
                    {
                        #region Start-Forward

                        switch (operand.LeftValueType)
                        {
                            case (int)SoeTimeRuleValueType.ScheduleLeft:
                            case (int)SoeTimeRuleValueType.ScheduleAndBreakLeft:
                            case (int)SoeTimeRuleValueType.PresenceWithinSchedule:
                                compareValue = timeChunk.StopOriginal;
                                break;
                            case (int)SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod:
                            case (int)SoeTimeRuleValueType.TimeCodeLeft:
                            case (int)SoeTimeRuleValueType.Presence:
                            case (int)SoeTimeRuleValueType.Payed:
                                compareValue = timeChunk.Stop;
                                break;
                            default:
                                compareValue = param.PresenceStop;
                                break;
                        }

                        compareValue = compareValue.Subtract(diff);

                        //Dont allow chunk starts before block stoptime (subtracted diff)
                        if (timeChunk.Start < compareValue)
                            return false;

                        #endregion
                    }
                    else if (param.IsBackward)
                    {
                        #region Start-Backward

                        switch (operand.LeftValueType)
                        {
                            case (int)SoeTimeRuleValueType.ScheduleLeft:
                            case (int)SoeTimeRuleValueType.ScheduleAndBreakLeft:
                            case (int)SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod:
                            case (int)SoeTimeRuleValueType.PresenceWithinSchedule:
                                compareValue = timeChunk.StartOriginal;
                                break;
                            case (int)SoeTimeRuleValueType.TimeCodeLeft:
                            case (int)SoeTimeRuleValueType.Presence:
                            case (int)SoeTimeRuleValueType.Payed:
                                compareValue = timeChunk.Start;
                                break;
                            default:
                                compareValue = param.PresenceStart;
                                break;
                        }

                        compareValue = compareValue.Add(diff);

                        //Dont allow chunk stops after block starttime (added diff)
                        if (timeChunk.Stop > compareValue)
                            return false;

                        #endregion
                    }
                }
                else
                {
                    if (param.IsForward)
                    {
                        #region Stop-Forward

                        switch (operand.LeftValueType)
                        {
                            case (int)SoeTimeRuleValueType.ScheduleLeft:
                            case (int)SoeTimeRuleValueType.PresenceWithinSchedule:
                                compareValue = timeChunk.StopOriginal;
                                break;
                            case (int)SoeTimeRuleValueType.TimeCodeLeft:
                            case (int)SoeTimeRuleValueType.Presence:
                            case (int)SoeTimeRuleValueType.Payed:
                                compareValue = timeChunk.Stop;
                                break;
                            default:
                                compareValue = param.PresenceStop;
                                break;
                        }

                        compareValue = compareValue.Subtract(diff);

                        //Dont allow chunk stops after block stoptime (subtracted diff)
                        if (timeChunk.Stop > compareValue)
                            return false;

                        #endregion
                    }
                    else if (param.IsBackward)
                    {
                        #region Stop-Backward

                        compareValue = timeChunk.StopOriginal;

                        //Dont allow chunk that dont stop at original stop
                        if (timeChunk.Stop != compareValue)
                            return false;

                        #endregion
                    }
                }

                return true;
            }

            /// <summary>
            /// Returns true if chunk starts before schema out +/- minutes
            /// </summary>
            /// <param name="param"></param>
            /// <param name="timeChunk"></param>
            /// <param name="operand"></param>
            /// <param name="isStartExpression"></param>
            /// <returns>True if chunk starts before schema out +/- minutes</returns>
            private static bool EvaluateScheduleInOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, bool isStartExpression)
            {
                int operandMinutes = operand.ComparisonOperator == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonClockNegative ? operand.Minutes * -1 : operand.Minutes;
                int scheduleInMinutes = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleIn) + operandMinutes;

                //Verify that schedule in relative is not higher than schedule out
                if (operandMinutes > 0)
                {
                    int scheduleOutMinutes = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleOut);
                    if (scheduleOutMinutes < scheduleInMinutes)
                        scheduleInMinutes = scheduleOutMinutes;
                }

                return timeChunk.EvaluateRelativeTime(isStartExpression, scheduleInMinutes);
            }

            /// <summary>
            /// Returns true if chunk starts before schema out +/- minutes
            /// </summary>
            /// <param name="param"></param>
            /// <param name="timeChunk"></param>
            /// <param name="operand"></param>
            /// <param name="isStartExpression"></param>
            /// <returns>True if chunk starts before schema out +/- minutes</returns>
            private static bool EvaluateScheduleOutOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, bool isStartExpression)
            {
                int operandMinutes = operand.ComparisonOperator == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonClockNegative ? operand.Minutes * -1 : operand.Minutes;
                int scheduleOutMinutes = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleOut) + operandMinutes;

                //Verify that schedule out relative is not less than schedule in
                if (operandMinutes < 0)
                {
                    int scheduleInMinutes = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleIn);
                    if (scheduleInMinutes > scheduleOutMinutes)
                        scheduleOutMinutes = scheduleInMinutes;
                }

                return timeChunk.EvaluateRelativeTime(isStartExpression, scheduleOutMinutes);
            }

            /// <summary>
            /// Returns true if chunk starts on or after operand minutes
            /// </summary>
            /// <param name="timeChunk"></param>
            /// <param name="operand"></param>
            /// <param name="isStartExpression"></param>
            /// <returns>True if chunk starts on or after operand minutes</returns>
            private static bool EvaluateClockOperand(TimeChunk timeChunk, TimeRuleOperand operand, bool isStartExpression)
            {
                return timeChunk.EvaluateRelativeTime(isStartExpression, operand.Minutes);
            }

            /// <summary>
            /// Returns true if no transactions exist with the same type as the operand value
            /// </summary>
            /// <param name="param"></param>
            /// <param name="timeChunk"></param>
            /// <param name="operand"></param>
            /// <returns>True if no transactions exist with the same type as the operand value</returns>
            private static bool EvaluateNotOperand(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand)
            {
                SoeTimeRuleValueType leftValueType = (SoeTimeRuleValueType)(operand.LeftValueType ?? 0);
                switch (leftValueType)
                {
                    case SoeTimeRuleValueType.ScheduleLeft:
                        if (SummarizeScheduleTime(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.ScheduleAndBreakLeft:
                        if (SummarizeScheduleTime(param, timeChunk, doHandleBreakAsSchedule: true) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod:
                        if (SummarizeSchedulePlusOvertimeInOvertimePeriod(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PresenceWithinSchedule:
                        if (SummarizePresenceWithinSchedule(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.Presence:
                        if (SummarizePresence(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PresenceBeforeSchedule:
                        if (SummarizePresenceBeforeSchedule(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PresenceAfterSchedule:
                        if (SummarizePresenceAfterSchedule(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PresenceInScheduleHole:
                        if (SummarizePresenceInScheduleHole(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.Payed:
                        if (SummarizePayed(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PayedBeforeSchedule:
                        if (SummarizePayedBeforeSchedule(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PayedBeforeSchedulePlusSchedule:
                        if (SummarizePayedBeforeSchedulePlusScheduleTime(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PayedAfterSchedule:
                        if (SummarizePayedAfterSchedule(param, timeChunk) > 0)
                            return false;
                        break;
                    case SoeTimeRuleValueType.PayedAfterSchedulePlusSchedule:
                        if (SummarizePayedAfterSchedulePlusScheduleTime(param, timeChunk) > 0)
                            return false;
                        break;
                    default:
                        return HasMatchingPreviousTransactions(param, timeChunk, operand, leftValueType);
                }

                return true;
            }

            private static bool HasMatchingPreviousTransactions(TimeEngineRuleEvaluatorParam param, TimeChunk timeChunk, TimeRuleOperand operand, SoeTimeRuleValueType leftValueType)
            {
                foreach (TimeCodeTransaction previousTransaction in param.PreviousTimeCodeTransactions)
                {
                    if (leftValueType == SoeTimeRuleValueType.PresenceWithinSchedule)
                    {
                        //Check if not rule uses special flags - Presence within schedule
                        if (timeChunk.TotalStopMinutes <= CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleIn) || timeChunk.TotalStartMinutes >= CalendarUtility.GetTotalMinutesFromDateTimeAsTime(param.ScheduleOut))
                            return true;
                    }
                    else if (previousTransaction.TimeCodeId != operand.LeftValueId)
                    {
                        //Check if transaction is not the same as the one not allowed
                        continue;
                    }

                    if (param.TimeRule.UseStandardMinutes && previousTransaction.IsGeneratedFromUseTimeCodeMaxAsStandardMinutes && CalendarUtility.IsDatesOverlapping(timeChunk.StartOriginal, timeChunk.StopOriginal, CalendarUtility.GetTimeSpanFromDateTime(previousTransaction.Start), CalendarUtility.GetTimeSpanFromDateTime(previousTransaction.Stop)))
                        return false;

                    //Skip if the transaction time doesnt overlap with the timechunks
                    int transactionStartMinute = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(previousTransaction.Start);
                    int transactionStopMinute = CalendarUtility.GetTotalMinutesFromDateTimeAsTime(previousTransaction.Stop);
                    if (transactionStopMinute <= timeChunk.TotalStartMinutes || transactionStartMinute >= timeChunk.TotalStopMinutes)
                        continue;

                    //Transaction exist that fulfills operands condition
                    return false;
                }
                return true;
            }

            private static bool OperandHasLeftValueType(TimeRuleOperand operand, params SoeTimeRuleValueType[] types)
            {
                return operand.LeftValueType.HasValue && types.Contains((SoeTimeRuleValueType)operand.LeftValueType.Value);
            }

            #endregion

            #region RuleEvaluationResult

            /// <summary>
            /// Evaluates rule parsing result, to abstract this logic from parsing method
            /// An expression can only consist of the pattern {value} or {leftvalue comparison rightvalue}
            /// When more operands are used then these are nested into operands following mentioned pattern forming a chain
            /// </summary>
            /// <param name="evaluationResultSymbols"></param>
            /// <returns></returns>
            private static bool ParseRuleEvaluationResult(List<RuleEvaluationResult> evaluationResultSymbols)
            {
                return evaluationResultSymbols.Contains(RuleEvaluationResult.and)
                    ? !evaluationResultSymbols.Contains(RuleEvaluationResult.falsified) //all parts succeeded
                    : evaluationResultSymbols.Contains(RuleEvaluationResult.succeeded); //contains succedeed part, valid for or comparison and single value operands
            }

            private static RuleEvaluationResult GetRuleEvaluationResult(bool result)
            {
                return result ? RuleEvaluationResult.succeeded : RuleEvaluationResult.falsified;
            }

            #endregion

            #endregion
        }
    }
}
