using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        #endregion

        #region Generate

        private ActionResult GenerateInternalTransactions(ref TimeEngineTemplate template, bool? discardBreakEvaluation = null)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            if (!template.HasValidIdentity(requireEmployeeGroup: true))
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeEngineTemplateIdentity");
            if (template.Identity.TimeBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlocks");

            template.Outcome = new TimeEngineTemplateOutcome();

            CheckDuplicateTimeBlocks(template, "GenerateInternalTransactions");

            #endregion

            #region Prereq

            EnsureScheduleForTimeEngineIdentity(template.Identity);

            List<TimeAbsenceRuleHead> absenceRules = GetTimeAbsenceRuleHeadsWithRowsFromCache(template.EmployeeGroupId);
            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache();
            List<int> timeDeviationCauseIdsOvertime = timeDeviationCauses.GetOvertimeDeviationCauseIds();
            List<int> timeScheduleTypeIdsIsNotScheduleTime = GetTimeScheduleTypeIdsIsNotScheduleTimeFromCache();

            bool useMultipleScheduleTypes = GetCompanyBoolSettingFromCache(CompanySettingType.UseMultipleScheduleTypes);
            bool containsNotOvertime = timeDeviationCauseIdsOvertime.IsNullOrEmpty() || template.Identity.TimeBlocks.Select(t => t.TimeDeviationCauseStartId).Any(tid => !tid.HasValue || !timeDeviationCauseIdsOvertime.Contains(tid.Value));
            List<TimeScheduleTemplateBlock> scheduleBlocksForOvertimePeriod = null;
            List<TimeBlock> presenceTimeBlocksForOvertimePeriod = null;

            #endregion

            #region Analyze schedule and deviations

            template.Identity.TimeBlocks = EvaluateBreaksRules(template, discardBreakEvaluation);
            template.Identity.TimeBlocks = SplitTimeBlocksAfterTimeScheduleType(template.Identity.TimeBlocks, template.Identity.ScheduleBlocks);

            SetTimeBlockTypes(template.EmployeeId, template.Date, template.Identity.TimeBlocks, template.Identity.ScheduleBlocks, setScheduleTypeOnConnectedTimeBlocksOutsideSchedule: containsNotOvertime);
            SetTimeBlockRelations(template.EmployeeId, template.Identity.TimeBlocks, template.TimeBlockDateId, template.Identity.TimeScheduleTemplatePeriodId);

            var (breakTimeBlocks, presenceTimeBlocks, absenceTimeBlocks, absenceAndPresenceTimeBlocks) = SplitAfterTimeBlockTypes(template, absenceRules);

            TimeEngineRuleEvaluatorProgress progress = new TimeEngineRuleEvaluatorProgress(
                TimeRuleManager.GetTimeRulesFromCache(entities, actorCompanyId).Filter(date: template.Date, dayTypeId: template.DayTypeId, employeeGroupId: template.EmployeeGroupId.ToNullable(), doFilterTimeScheduleTypeId: false),
                doScheduleContansStandby: template.Identity.ScheduleBlocks.ContainsStandby(),
                hasOvertimePresence: presenceTimeBlocks.ContainsTimeDeviationCause(timeDeviationCauseIdsOvertime)
            );

            if (progress.ContainsStandbyRules())
            {
                DecideTimeBlockStandby(presenceTimeBlocks, template.Identity.ScheduleBlocks, template.EmployeeGroup, timeDeviationCauses);
                absenceTimeBlocks.ForEach(i => i.CalculatedAsStandby = false);
            }

            #endregion

            #region Generate TimeCodeTransactions

            #region Breaks

            foreach (TimeBlock timeBlock in breakTimeBlocks)
            {
                foreach (TimeCode timeCode in timeBlock.TimeCode)
                {
                    //Breaks should not generated transactions
                    if (timeCode.IsBreak())
                        continue;

                    //Presence/Absence generated from break
                    TimeCodeTransaction timeCodeTransaction = ConvertToTimeCodeTransaction(template.Identity.Employee, timeBlock, timeCode);
                    if (timeCodeTransaction != null)
                        template.Outcome.TimeCodeTransactions.Add(timeCodeTransaction);
                }
            }

            #endregion

            #region Presence/Absence

            for (int ruleStage = progress.FirstStage; ruleStage <= progress.NrOfStages; ruleStage++)
            {
                progress.SetRuleStage(ruleStage);
                if (progress.IsStandbyStage())
                    template.Outcome.UseStandby = true;

                List<TimeBlock> validPresenceTimeBlocks = progress.FilterTimeBlocks(presenceTimeBlocks);
                List<TimeBlock> validAbsenceTimeBlocks = progress.FilterTimeBlocks(absenceTimeBlocks);

                #region Presence TimeRules

                if (validPresenceTimeBlocks.Any())
                {
                    List<TimeScheduleTemplateBlock> validScheduleBlocks = progress.FilterScheduleBlocks(template.Identity.ScheduleBlocks);

                    foreach (TimeBlock presenceTimeBlock in validPresenceTimeBlocks)
                    {
                        #region TimeBlock

                        //Only evaluate absenceAndPresence TimeBlocks for balance rules
                        if (!progress.DoEvaluateBalanceRules() && absenceAndPresenceTimeBlocks.Any() && absenceAndPresenceTimeBlocks.Any(i => i.GuidId == presenceTimeBlock.GuidId))
                            continue;

                        TimeChunk inputTimeChunk = new TimeChunk(presenceTimeBlock);

                        bool isOverlappedByAbsenceTimeBlock = IsAnyAbsenceTimeBlockOverlappingPresenceTimeBlock(validAbsenceTimeBlocks, presenceTimeBlock);
                        int timeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(template.Identity.Employee, template.EmployeeGroup, presenceTimeBlock, inputTimeChunk.UseStartCause);
                        List<TimeRule> presenceTimeRules = progress.GetPresenceRules(timeDeviationCauseId, presenceTimeBlock, useMultipleScheduleTypes, isOverlappedByAbsenceTimeBlock ? timeScheduleTypeIdsIsNotScheduleTime : null);

                        foreach (TimeRule timeRule in presenceTimeRules)
                        {
                            #region TimeRule

                            TimeCode timeCode = GetTimeCodeFromCache(timeRule.TimeCodeId, loadRules: true);
                            if (timeCode == null || timeCode.State != (int)SoeEntityState.Active)
                                continue;

                            //Eval
                            List<TimeChunk> timeChunks = EvaluateRule(template, inputTimeChunk, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, validScheduleBlocks, ref scheduleBlocksForOvertimePeriod, validPresenceTimeBlocks, ref presenceTimeBlocksForOvertimePeriod, template.Outcome.TimeCodeTransactions);

                            //Set resulted
                            progress.SetRuleEvaluated(timeRule, timeChunks);

                            //Convert to TimeCodeTransaction
                            template.Outcome.TimeCodeTransactions.AddRange(ConvertToTimeCodeTransactions(template.Identity.Employee, presenceTimeBlock, timeCode, timeRule, timeChunks, template.Date));

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion

                #region Absence TimeRules

                if (!progress.IsStandbyStage() && validAbsenceTimeBlocks.Any())
                {
                    foreach (TimeBlock absenceTimeBlock in validAbsenceTimeBlocks)
                    {
                        #region TimeBlock

                        if (absenceTimeBlock.IsBreak || !template.DayTypeId.HasValue)
                            continue;

                        List<TimeScheduleTemplateBlock> validScheduleBlocks = template.Identity.ScheduleBlocks;
                        if (progress.DoScheduleContainsStandby)
                        {
                            TimeScheduleTemplateBlock matchingScheduleBlock = template.Identity.ScheduleBlocks.GetMatchingScheduleBlock(absenceTimeBlock, false);
                            if (matchingScheduleBlock != null)
                                validScheduleBlocks = progress.FilterScheduleBlocks(template.Identity.ScheduleBlocks, forceStandbyValue: matchingScheduleBlock.IsStandby());
                        }

                        TimeChunk inputTimeChunk = new TimeChunk(absenceTimeBlock);

                        int timeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(template.Identity.Employee, template.EmployeeGroup, absenceTimeBlock, inputTimeChunk.UseStartCause);
                        List<TimeRule> absenceTimeRules = progress.GetAbsenceRules(timeDeviationCauseId, absenceTimeBlock, useMultipleScheduleTypes);

                        #region Internal absence TimeRule

                        //Only run on first iteration. A default absence rule is applied if no matching exists for the TimeDeviationCause (doSeparateRulesByStage -> false to also include balance rules)
                        if (progress.IsFirstStage() && !absenceTimeRules.Any() && !progress.ContainsAbsenceRule(timeDeviationCauseId, absenceTimeBlock, useMultipleScheduleTypes, doSeparateRulesByStage: false))
                        {
                            TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(timeDeviationCauseId);
                            if (timeDeviationCause != null && timeDeviationCause.TimeCodeId.HasValue)
                            {
                                TimeRule internalAbsenceTimeRule = progress.GetInternalRule(timeDeviationCause.TimeCodeId.Value, timeDeviationCause.TimeDeviationCauseId, template.DayTypeId.Value);
                                if (internalAbsenceTimeRule == null)
                                {
                                    internalAbsenceTimeRule = CreateInternalAbsenceTimeRule(timeDeviationCause, template.DayTypeId.Value);
                                    if (internalAbsenceTimeRule != null)
                                        progress.AddInternalRule(internalAbsenceTimeRule);
                                }
                                if (internalAbsenceTimeRule != null)
                                    absenceTimeRules.Add(internalAbsenceTimeRule);
                            }
                        }

                        #endregion

                        foreach (TimeRule timeRule in absenceTimeRules)
                        {
                            #region TimeRule

                            TimeCode timeCode = GetTimeCodeFromCache(timeRule.TimeCodeId, loadRules: true);
                            if (timeCode == null || timeCode.State != (int)SoeEntityState.Active)
                                continue;

                            //Eval
                            List<TimeChunk> timeChunks = EvaluateRule(template, inputTimeChunk, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, validScheduleBlocks, ref scheduleBlocksForOvertimePeriod, validPresenceTimeBlocks, ref presenceTimeBlocksForOvertimePeriod, template.Outcome.TimeCodeTransactions);
                            if (timeChunks.Any())
                                absenceTimeBlock.TransactionsResulted = true;

                            //Set resulted
                            progress.SetRuleEvaluated(timeRule, timeChunks);

                            //Convert to TimeCodeTransaction
                            template.Outcome.TimeCodeTransactions.AddRange(ConvertToTimeCodeTransactions(template.Identity.Employee, absenceTimeBlock, timeCode, timeRule, timeChunks, template.Date));

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion

                #region Constant TimeRules

                //Presence rules only allowed to be applied once per day
                if (progress.DoEvaluateConstantRules())
                {
                    List<TimeScheduleTemplateBlock> validScheduleBlocks = progress.FilterScheduleBlocks(template.Identity.ScheduleBlocks);

                    List<TimeBlock> validPresenceAndAbsenceTimeBlocks = validPresenceTimeBlocks.Concat(validAbsenceTimeBlocks).Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
                    foreach (TimeBlock timeBlock in validPresenceAndAbsenceTimeBlocks)
                    {
                        #region TimeBlock

                        TimeChunk inputTimeChunk = new TimeChunk(timeBlock);

                        int timeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(template.Identity.Employee, template.EmployeeGroup, timeBlock, inputTimeChunk.UseStartCause);
                        List<TimeRule> constantTimeRules = progress.GetConstantRules(timeDeviationCauseId, timeBlock, useMultipleScheduleTypes, null);

                        foreach (TimeRule timeRule in constantTimeRules)
                        {
                            #region TimeRule

                            TimeCode timeCode = GetTimeCodeFromCache(timeRule.TimeCodeId, loadRules: true);
                            if (timeCode == null || timeCode.State != (int)SoeEntityState.Active)
                                continue;

                            //Remove multiple validations of same TimeRule
                            if (progress.HasConstantRuleResulted(timeRule.TimeRuleId))
                                continue;

                            //Eval
                            List<TimeChunk> timeChunks = EvaluateRule(template, inputTimeChunk, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, validScheduleBlocks, ref scheduleBlocksForOvertimePeriod, validPresenceTimeBlocks, ref presenceTimeBlocksForOvertimePeriod, template.Outcome.TimeCodeTransactions);

                            //Set resulted
                            progress.SetRuleEvaluated(timeRule, timeChunks);

                            //Convert to TimeCodeTransaction
                            template.Outcome.TimeCodeTransactions.AddRange(ConvertToTimeCodeTransactions(template.Identity.Employee, timeBlock, timeCode, timeRule, timeChunks, template.Date));

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion
            }

            #endregion

            #endregion

            #region Adjust TimeCodeTransactions

            template.Outcome.TimeCodeTransactions = GenerateTransactionForZeroDays(template, progress, absenceTimeBlocks);
            template.Outcome.TimeCodeTransactions = SetTimeCodeTransactionRelations(template);
            if (absenceRules.Any())
                template.Outcome.TimeCodeTransactions = HandleSickDuringIwhOrStandbyTransactions(template, progress, ref presenceTimeBlocks, ref absenceTimeBlocks);
            template.Outcome.TimeCodeTransactions = HandleScheduleTransactions(template, ref presenceTimeBlocks, ref absenceTimeBlocks);
            template.Outcome.TimeCodeTransactions = AdjustTimeCodeTransactionsByBreakTime(template, progress);
            template.Outcome.TimeCodeTransactions = AdjustTimeCodeTransactionsForRoundingWholeDay(template);
            template.Outcome.TimeCodeTransactions = AdjustTimeCodeTransactionsForAbsenceQuantity(template);
            template.Outcome.TimeCodeTransactions = AdjustTimeCodeTransactionsForTimeCodeRanking(template);
            template.Outcome.TimeCodeTransactions = RemoveEmptyTimeCodeTransactions(template, progress);

            #endregion

            #region Adjust TimeBlocks

            SetTimeCodeOnTimeBlocksFromTransactions(presenceTimeBlocks, template.Outcome.TimeCodeTransactions);
            SetTimeCodeOnTimeBlocksFromTransactions(absenceTimeBlocks, template.Outcome.TimeCodeTransactions);
            SetTimeCodeIdsOnTimeBlocksFromTransactions(template.Identity.TimeBlocks, template.Outcome.TimeCodeTransactions);
            ApplyAccountingOnNewTimeBlockFromNearestTimeBlockIfMissing(template.Identity.TimeBlocks, template.Identity.Employee);

            if (progress.HasOvertimePresence)
                AddDayToOvertimeTracker(template.Date);

            #endregion

            return result;
        }

        private List<TimeTransactionItem> GenerateExternalTransactions(TimeEngineTemplate template, bool isAdditionOrDeduction = false, int projectId = 0)
        {
            List<TimeTransactionItem> timeTransactionItems = new List<TimeTransactionItem>();

            #region Init

            if (!template.HasValidIdentity() || template.Outcome?.TimeCodeTransactions == null)
                return timeTransactionItems;

            CheckDuplicateTimeBlocks(template, "GenerateExternalTransactions");

            #endregion

            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId);
            if (employee == null)
                return timeTransactionItems;

            #endregion

            #region Perform

            bool? hasCheckAbsenceOnReversedDay = null;
            var absenceDays = new List<ApplyAbsenceDayBase>();
            var applyAbsenceResults = new List<ApplyAbsenceResult>();
            var inputTimeCodeTransactions = template.Outcome.GetTimeCodeTransactionsForExternalTransactions();

            for (int tct = inputTimeCodeTransactions.Count - 1; tct >= 0; tct--) //Count down to be able to remove items
            {
                TimeCodeTransaction timeCodeTransaction = inputTimeCodeTransactions[tct];
                if (timeCodeTransaction == null)
                    continue;

                TimeCode timeCode = GetTimeCodeWithProductsFromCache(timeCodeTransaction.TimeCodeId);
                if (timeCode == null)
                    continue;

                #region TimeInvoiceTransactions

                if (!timeCode.TimeCodeInvoiceProduct.IsNullOrEmpty())
                {
                    foreach (TimeCodeInvoiceProduct timeCodeInvoiceProduct in timeCode.TimeCodeInvoiceProduct)
                    {
                        InvoiceProduct invoiceProduct = GetInvoiceProductFromCache(timeCodeInvoiceProduct.ProductId);
                        if (invoiceProduct == null)
                            continue;

                        AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
                        if (attestStateInitial == null)
                            continue;

                        TimeBlock timeBlock = template.Identity.TimeBlocks?.FirstOrDefault(i => i.GuidId == timeCodeTransaction.GuidTimeBlock);
                        int transactionId = timeCodeTransaction.TimeInvoiceTransaction?.FirstOrDefault(i => i.ProductId == timeCodeInvoiceProduct.ProductId)?.TimeInvoiceTransactionId ?? 0;

                        TimeTransactionItem item = timeCodeTransaction.CreateTransactionItem(SoeTimeTransactionType.TimeInvoice, transactionId, timeCodeInvoiceProduct.ProductId, template.TimeBlockDateId, template.Date, timeCodeInvoiceProduct.Factor, employee, timeBlock, attestStateInitial);
                        if (item != null)
                        {
                            ApplyAccountingOnTimeTransactionItem(item, timeBlock: timeBlock);
                            timeTransactionItems.Add(item);
                        }
                    }
                }

                #endregion

                #region TimePayrollTransactions

                if (!timeCode.TimeCodePayrollProduct.IsNullOrEmpty())
                {
                    foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCode.TimeCodePayrollProduct)
                    {
                        PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timeCodePayrollProduct.ProductId);
                        if (payrollProduct == null)
                            continue;

                        AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                        if (attestStateInitial == null)
                            continue;

                        TimeBlock connectedTimeBlock = template.Identity.TimeBlocks?.FirstOrDefault(i => i.GuidId == timeCodeTransaction.GuidTimeBlock);
                        EmployeeChild employeeChild = connectedTimeBlock?.EmployeeChildId != null ? GetEmployeeChild(connectedTimeBlock.EmployeeChildId.Value) : null;
                        int transactionId = timeCodeTransaction.TimePayrollTransaction?.FirstOrDefault(i => i.ProductId == timeCodePayrollProduct.ProductId)?.TimePayrollTransactionId ?? 0;

                        (ApplyAbsenceDay absenceDay, bool valid) = ApplyAbsence(template, ref payrollProduct, ref timeCode, timeCodeTransaction, connectedTimeBlock, employee, employeeChild, absenceDays, applyAbsenceResults, false, ref hasCheckAbsenceOnReversedDay);
                        if (!valid)
                        {
                            inputTimeCodeTransactions.RemoveAt(tct);
                            break;
                        }

                        TimeTransactionItem item = timeCodeTransaction.CreateTransactionItem(SoeTimeTransactionType.TimePayroll, transactionId, payrollProduct.ProductId, template.TimeBlockDateId, template.Date, timeCodePayrollProduct.Factor, employee, connectedTimeBlock, attestStateInitial, employeeChild, payrollProduct);
                        if (item != null)
                        {
                            if (absenceDay != null)
                            {
                                if (absenceDay.VacationFiveDaysPerWeekHasSchedule == false)
                                    item.Quantity = 1;
                                if (absenceDay.IsVacationReplacementAndResulted)
                                    item.IsVacationReplacement = true;
                            }

                            ApplyAccountingOnTimeTransactionItem(item, employee, payrollProduct, template.Date, connectedTimeBlock, projectId: projectId);
                            timeTransactionItems.Add(item);
                        }
                    }
                }

                #endregion
            }

            foreach (var itemsByProduct in timeTransactionItems.Where(i => i.ProductName.IsNullOrEmpty()).GroupBy(i => i.ProductId))
            {
                Product product = GetProductFromCache(itemsByProduct.Key);
                if (product == null)
                    return null;

                foreach (var item in itemsByProduct)
                {
                    item.ProductName = product.Name;
                    item.ProductNr = product.Number;
                }
            }

            if (!isAdditionOrDeduction)
            {
                ActionResult result = CreateTransactionsFromPayrollProductChain(timeTransactionItems, employee.EmployeeId, template.Identity.TimeBlockDate);
                if (result.Success)
                    result = CreateQualifyingDeductionTransactions(template, absenceDays, timeTransactionItems);
                if (result.Success)
                    CreateFixedAccountingTransactions(timeTransactionItems, employee.EmployeeId, template.Identity.TimeBlockDate);
            }

            #endregion

            return timeTransactionItems;
        }

        private ActionResult GenerateDeviationsForPeriod(DateTime date, int timeScheduleTemplatePeriodId, int employeeId, List<TimeBlock> inputTimeBlocks, out List<TimeBlock> outputTimeBlocks, out List<TimeTransactionItem> outputTimeTransactionItems, List<TimeScheduleTemplateBlock> scheduleBlocks = null, TimeIntervalAccountingDTO timeIntervalAccounting = null)
        {
            outputTimeBlocks = new List<TimeBlock>();
            outputTimeTransactionItems = new List<TimeTransactionItem>();

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, date.ToShortDateString()));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date, false);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");
            if (timeBlockDate.IsLocked)
                return new ActionResult((int)ActionResultSave.Locked, GetText(91937, "Dagen är låst och kan ej behandlas"));

            DayType dayType = GetDayTypeForEmployeeFromCache(employee.EmployeeId, date);
            if (dayType == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

            if (scheduleBlocks == null)
                scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employeeId, timeBlockDate.Date, null, includeStandBy: true);

            SetTimeBlockTimeDevationCauses(inputTimeBlocks);
            ApplyAccountingOnTimeBlockFromTemplateBlocks(employee, scheduleBlocks, inputTimeBlocks, setAccountingOnExcessToDockedTimeBlockOrNearestSchedule: true, timeIntervalAccounting: timeIntervalAccounting);
            CreateAbsenceTimeBlocksDuringScheduleTime(ref inputTimeBlocks, timeBlockDate, timeScheduleTemplatePeriodId, scheduleBlocks, employee);

            TimeEngineTemplate template = new TimeEngineTemplate(TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate, timeScheduleTemplatePeriodId, scheduleBlocks: scheduleBlocks, timeBlocks: inputTimeBlocks, dayType: dayType, isAnyTimeBlockAbsence: IsAnyTimeBlockAbsence(inputTimeBlocks))); //Do not look for existing template
            ActionResult result = GenerateInternalTransactions(ref template);
            if (!result.Success)
                return result;

            outputTimeBlocks.AddRange(template.Identity.TimeBlocks);
            outputTimeTransactionItems.AddRange(GenerateExternalTransactions(template));
            outputTimeTransactionItems.AddRange(GetTimeTransactionItemsWithoutTimeBlocks(template, timeScheduleTemplatePeriodId));
            outputTimeTransactionItems.AddRange(ConvertToTimeTransactionItems(template));

            return result;
        }

        #endregion

        #region Save transactions

        private ActionResult SaveTransactionsForPeriods(List<TimeEngineDay> days, bool? discardBreakEvaluation = null)
        {
            if (days.IsNullOrEmpty())
                return new ActionResult(true);

            ActionResult result = new ActionResult(true);

            foreach (TimeEngineDay day in days.OrderBy(i => i.Date))
            {
                #region TimeEngineDay

                #region Prereq

                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(day.EmployeeId);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

                EmployeeGroup employeeGroup = day.EmployeeGroup ?? employee.GetEmployeeGroup(day.Date, GetEmployeeGroupsFromCache());
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, day.Date.ToShortDateString()));

                TimeBlockDate timeBlockDate = day.TimeBlockDate ?? GetTimeBlockDateFromCache(day.EmployeeId, day.Date);
                if (timeBlockDate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                DayType dayType = GetDayTypeForEmployeeFromCache(day.EmployeeId, day.Date);
                if (dayType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                int standardTimeDeviationCauseId = day.StandardTimeDeviationCauseId ?? GetTimeDeviationCauseIdFromPrio(employee, employeeGroup, null, true);

                #endregion

                #region Repository

                TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate, day.TemplatePeriodId, standardTimeDeviationCauseId, timeBlocks: day.TimeBlocks, dayType: dayType, isAnyTimeBlockAbsence: IsAnyTimeBlockAbsence(day.TimeBlocks), dependencyGuid: day.Key);
                result = SaveTransactionsUsingTemplateRepository(identity, discardBreakEvaluation, day.AdditionalTimeCodeTransactions);
                if (!result.Success)
                    return result;

                #endregion

                #region Update status

                SetTimeBlockDateStatus(day, SoeTimeBlockDateStatus.None);
                if (!employeeGroup.AutogenTimeblocks && this.initiatedAbsenceDays.IsNullOrEmpty())
                    SetTimeStampEntryStatus(day, TermGroup_TimeStampEntryStatus.Processing, TermGroup_TimeStampEntryStatus.Processed);

                result = Save();
                if (!result.Success)
                    return result;

                #endregion

                #endregion
            }

            return result;
        }

        private ActionResult SaveTransactionsForPeriod(TimeBlockDate timeBlockDate, Employee employee, int? templatePeriodId, List<TimeScheduleTemplateBlock> scheduleBlocksForDay = null, List<TimeBlock> timeBlocksForDay = null, bool? discardBreakEvaluation = null)
        {
            #region Prereq

            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, timeBlockDate.Date.ToShortDateString()));

            DayType dayType = GetDayTypeForEmployeeFromCache(employee.EmployeeId, timeBlockDate.Date);
            if (dayType == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

            if (scheduleBlocksForDay == null)
                scheduleBlocksForDay = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employee.EmployeeId, timeBlockDate.Date, includeStandBy: true);
            if (timeBlocksForDay == null)
                timeBlocksForDay = GetTimeBlocksWithTimeCodeAndAccountInternal(employee.EmployeeId, timeBlockDate.TimeBlockDateId, templatePeriodId);

            int standardTimeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(employee, employeeGroup, null, true);

            #endregion

            #region Repository

            TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate, templatePeriodId, standardTimeDeviationCauseId, scheduleBlocks: scheduleBlocksForDay, timeBlocks: timeBlocksForDay, dayType: dayType, isAnyTimeBlockAbsence: IsAnyTimeBlockAbsence(timeBlocksForDay));
            return SaveTransactionsUsingTemplateRepository(identity, discardBreakEvaluation);

            #endregion
        }

        private ActionResult SaveExternalTransactions(ref TimeEngineTemplate template, bool isAdditionOrDeduction = false, int projectId = 0)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            if (template?.Identity?.Employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (template?.Identity?.TimeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");
            if (template?.Outcome?.TimeCodeTransactions == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCodeTransactions");
            if (template.Identity.TimeBlockDate.IsLocked)
                return new ActionResult((int)ActionResultSave.Locked, GetText(91937, "Dagen är låst och kan ej behandlas"));

            CheckDuplicateTimeBlocks(template, "SaveExternalTransactions");

            #endregion

            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            Employment employment = employee.GetEmployment(template.Date);
            if (employment == null && !isAdditionOrDeduction)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));
                                   
            if (!isAdditionOrDeduction)
            {
                List<TimePayrollScheduleTransaction> existingScheduleTransactions = GetTimePayrollScheduleTransactions(template.EmployeeId, template.TimeBlockDateId, SoeTimePayrollScheduleTransactionType.Absence)
                    .Where(i => !i.IsEmploymentTax() && !i.IsSupplementCharge())
                    .ToList();

                SetTimePayrollScheduleTransactionsToDeleted(existingScheduleTransactions, saveChanges: false);
            }

            #endregion

            #region Perform

            bool? hasCheckAbsenceOnReversedDay = null;
            var applyAbsenceDays = new List<ApplyAbsenceDayBase>();
            var applyAbsenceResults = new List<ApplyAbsenceResult>();
            var inputTimeCodeTransactions = template.Outcome.GetTimeCodeTransactionsForExternalTransactions();

            for (int tct = inputTimeCodeTransactions.Count - 1; tct >= 0; tct--) //Count down to be able to remove items
            {
                TimeCodeTransaction inputTimeCodeTransaction = inputTimeCodeTransactions[tct];
                if (inputTimeCodeTransaction == null)
                    continue;
                
                TimeCodeTransaction timeCodeTransaction = inputTimeCodeTransaction.TimeCodeTransactionId == 0 ? inputTimeCodeTransaction : GetTimeCodeTransaction(inputTimeCodeTransaction.TimeCodeTransactionId, true);
                if (timeCodeTransaction == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeCodeTransaction");

                TimeCode timeCode = GetTimeCodeWithProductsFromCache(timeCodeTransaction.TimeCodeId);
                if (timeCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                if (!timeCodeTransaction.TimeBlockDateId.HasValue && timeCodeTransaction.TimeBlockDate == null)
                    timeCodeTransaction.TimeBlockDate = template.Identity.TimeBlockDate;

                bool isScheduleTransaction = UsePayroll() && timeCodeTransaction.IsScheduleTransaction;

                #region TimeInvoiceTransactions

                if (!timeCode.TimeCodeInvoiceProduct.IsNullOrEmpty() && !template.Identity.TimeBlocks.IsFromProjectTimeBlock())
                {
                    foreach (TimeCodeInvoiceProduct timeCodeInvoiceProduct in timeCode.TimeCodeInvoiceProduct)
                    {
                        int originalProductId = timeCodeInvoiceProduct.ProductId;

                        TimeBlockDate timeBlockDate = null;
                        if (timeCodeTransaction.TimeBlock != null && base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock.TimeBlockDate))
                            timeBlockDate = timeCodeTransaction.TimeBlock.TimeBlockDate;
                        else if (timeCodeTransaction.TimeBlock != null && timeCodeTransaction.TimeBlock.TimeBlockId > 0)
                            timeBlockDate = GetTimeBlockDateFromTimeBlock(timeCodeTransaction.TimeBlock.TimeBlockId);
                        else
                            timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, template.TimeBlockDateId);

                        TimeInvoiceTransaction timeInvoiceTransaction = timeCodeTransaction.TimeInvoiceTransaction.FirstOrDefault(i => i.ProductId == originalProductId);
                        if (timeInvoiceTransaction == null)
                        {
                            #region Add

                            AttestStateDTO attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
                            if (attestStateInitialInvoice == null)
                                return new ActionResult(8517, GetText(8517, "Atteststatus - lägsta nivå saknas"));

                            timeInvoiceTransaction = new TimeInvoiceTransaction
                            {
                                Invoice = true,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                                EmployeeId = employee.EmployeeId,
                                ProductId = originalProductId,
                                AttestStateId = attestStateInitialInvoice.AttestStateId,
                            };
                            SetCreatedProperties(timeInvoiceTransaction);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            timeInvoiceTransaction.State = (int)SoeEntityState.Active; //State can be removed and go back to active on update
                            SetModifiedProperties(timeInvoiceTransaction);

                            #endregion
                        }

                        timeInvoiceTransaction.Quantity = timeCodeTransaction.Quantity * timeCodeInvoiceProduct.Factor;
                        timeInvoiceTransaction.Amount = timeCodeTransaction.Amount;
                        timeInvoiceTransaction.VatAmount = timeCodeTransaction.Vat;
                        timeInvoiceTransaction.TimeBlockDate = timeBlockDate;

                        SetTimeInvoiceTransactionTimeBlock(timeInvoiceTransaction, timeCodeTransaction);
                        SetTimeInvoiceTransactionCurrencyAmounts(timeInvoiceTransaction);
                        ApplyAccountingOnTimeInvoiceTransaction(timeInvoiceTransaction, timeCodeTransaction.TimeBlock);

                        timeCodeTransaction.TimeInvoiceTransaction.Add(timeInvoiceTransaction);
                        template.Outcome.TimeInvoiceTransactions.Add(timeInvoiceTransaction);
                        result.IntegerValue = timeInvoiceTransaction.TimeInvoiceTransactionId;
                    }
                }

                #endregion

                #region TimePayrollTransactions

                if (!timeCode.TimeCodePayrollProduct.IsNullOrEmpty())
                {
                    foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCode.TimeCodePayrollProduct)
                    {
                        PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timeCodePayrollProduct.ProductId);
                        if (payrollProduct == null)
                            continue;

                        TimeBlock connectedTimeBlock = template.Identity.TimeBlocks?.FirstOrDefault(i => i.GuidId == timeCodeTransaction.GuidTimeBlock);

                        TimeBlockDate timeBlockDate = null;
                        if (timeCodeTransaction.TimeBlock != null && base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock.TimeBlockDate))
                            timeBlockDate = timeCodeTransaction.TimeBlock.TimeBlockDate;
                        else if (timeCodeTransaction.TimeBlock != null && timeCodeTransaction.TimeBlock.TimeBlockId > 0)
                            timeBlockDate = GetTimeBlockDateFromTimeBlock(timeCodeTransaction.TimeBlock.TimeBlockId);
                        else
                            timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, template.TimeBlockDateId);

                        EmployeeChild employeeChild = null;
                        if (timeCodeTransaction.TimeBlock != null && base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock.EmployeeChild))
                            employeeChild = timeCodeTransaction.TimeBlock.EmployeeChild;
                        else if (timeCodeTransaction.TimeBlock != null && timeCodeTransaction.TimeBlock.EmployeeChildId.HasValue)
                            employeeChild = GetEmployeeChildFromCache(employee.EmployeeId, timeCodeTransaction.TimeBlock.EmployeeChildId.Value);

                        (ApplyAbsenceDay absenceDay, bool valid) = ApplyAbsence(template, ref payrollProduct, ref timeCode, timeCodeTransaction, connectedTimeBlock, employee, employeeChild, applyAbsenceDays, applyAbsenceResults, isScheduleTransaction, ref hasCheckAbsenceOnReversedDay);
                        if (!valid)
                        {
                            inputTimeCodeTransactions.RemoveAt(tct);
                            break;
                        }

                        decimal transactionQuantity = timeCodeTransaction.Quantity * timeCodePayrollProduct.Factor;
                        decimal? transactionAmount = timeCodeTransaction.Amount;
                        decimal? transactionVatAmount = timeCodeTransaction.Vat;

                        if (isScheduleTransaction)
                        {
                            #region TimePayrollScheduleTransaction

                            TimePayrollScheduleTransaction timePayrollScheduleTransaction = CreateTimePayrollScheduleTransaction(payrollProduct, timeBlockDate, transactionQuantity, transactionAmount, transactionVatAmount, 0, (int)SoeTimePayrollScheduleTransactionType.Absence, employee.EmployeeId, timeblockStartTime: timeCodeTransaction.Start, timeBlockStopTime: timeCodeTransaction.Stop);
                            if (timePayrollScheduleTransaction != null)
                            {
                                ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, employee, timeBlockDate.Date, payrollProduct, timeCodeTransaction.TimeBlock);
                                template.Outcome.TimePayrollScheduleTransactions.Add(timePayrollScheduleTransaction);

                                PayrollProduct payrollProductTurned = GetTurnedPayrollProductForAbsenceScheduleTransaction(template, applyAbsenceResults, timePayrollScheduleTransaction, timeCodeTransaction, payrollProduct);
                                if (payrollProductTurned != null)
                                {
                                    TimePayrollScheduleTransaction timePayrollScheduleTransactionTurned = CreateTimePayrollScheduleTransaction(payrollProductTurned, timeBlockDate, -transactionQuantity, transactionAmount, transactionVatAmount, 0, (int)SoeTimePayrollScheduleTransactionType.Absence, employee.EmployeeId, timeblockStartTime: timeCodeTransaction.Start, timeBlockStopTime: timeCodeTransaction.Stop);
                                    if (timePayrollScheduleTransactionTurned != null)
                                    {
                                        ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransactionTurned, employee, timeBlockDate.Date, payrollProductTurned, timeCodeTransaction.TimeBlock);
                                        template.Outcome.TimePayrollScheduleTransactions.Add(timePayrollScheduleTransactionTurned);
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region TimePayrollTransaction

                            TimePayrollTransaction timePayrollTransaction = timeCodeTransaction.TimePayrollTransaction.FirstOrDefault(i => i.ProductId == payrollProduct.ProductId);
                            if (timePayrollTransaction == null)
                            {
                                #region Add

                                AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                                if (attestStateInitialPayroll == null)
                                    return new ActionResult(8517, GetText(8517, "Atteststatus - lägsta nivå saknas"));

                                // Add new transactions from the items that are added in the collection
                                timePayrollTransaction = new TimePayrollTransaction
                                {
                                    //Set FK
                                    ActorCompanyId = actorCompanyId,
                                    EmployeeId = employee.EmployeeId,
                                    AttestStateId = attestStateInitialPayroll.AttestStateId,
                                };
                                SetCreatedProperties(timePayrollTransaction);
                                entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

                                #endregion
                            }
                            else
                            {
                                #region Update

                                timePayrollTransaction.Comment = String.Empty; //Clear value since transaction can be reused
                                timePayrollTransaction.State = (int)SoeEntityState.Active; //State can be removed and go back to active on update
                                SetModifiedProperties(timePayrollTransaction);

                                #endregion
                            }

                            timePayrollTransaction.ProductId = payrollProduct.ProductId;
                            timePayrollTransaction.Amount = transactionAmount;
                            timePayrollTransaction.VatAmount = transactionVatAmount;
                            timePayrollTransaction.IsAdditionOrDeduction = timeCodeTransaction.IsAdditionOrDeduction;
                            timePayrollTransaction.IsVacationReplacement = absenceDay?.IsVacationReplacementAndResulted ?? false;
                            timePayrollTransaction.TimeBlockDate = timeBlockDate;
                            timePayrollTransaction.EmployeeChild = employeeChild;

                            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                            SetTimePayrollTransactionTimeBlock(timePayrollTransaction, timeCodeTransaction);
                            SetTimePayrollTransactionToVacationFiveDaysPerWeek(timePayrollTransaction, payrollProduct, absenceDay);
                            SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                            SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employment, timeBlockDate, transactionQuantity, template.Identity.ScheduleBlocks);
                            SetTimePayrollTransactionStaffingFromTimeBlock(timePayrollTransaction, timeCodeTransaction.TimeBlock);
                            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
                            ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, timeBlockDate.Date, payrollProduct, timeCodeTransaction.TimeBlock, projectId: projectId);

                            timeCodeTransaction.TimePayrollTransaction.Add(timePayrollTransaction);
                            template.Outcome.TimePayrollTransactions.Add(timePayrollTransaction);
                            result.IntegerValue = timePayrollTransaction.TimePayrollTransactionId;

                            #endregion
                        }
                    }
                }

                #endregion
            }

            template.Identity.TimeBlockDate.Status = (int)SoeTimeBlockDateStatus.None;
            template.Outcome.TimeCodeTransactions = template.Outcome.TimeCodeTransactions.Where(i => i.EntityState != EntityState.Detached).ToList();

            if (this.useInitiatedAbsenceDays)
            {
                List<TimeEngineAbsenceDay> currentInitiatedAbsenceDays = GetInitiatedAbsenceDays(template.EmployeeId, template.Date, SoeTimeBlockDateDetailType.Absence);
                if (!currentInitiatedAbsenceDays.IsNullOrEmpty())
                    CreateTimeBlockDetailOutcome(template.TimeBlockDate, currentInitiatedAbsenceDays);
            }

            result = Save();
            if (result.Success)
            {
                LogEmployeeIdOnTransactionAndTimeBlockDateMissMatch(template.Outcome.TimePayrollTransactions);

                //Clear after to refresh cache with transactions added after day was (possible) added to cache
                ClearTimePayrollTransactionsWithTimeBlockDateFromCache(template.Identity.TimeBlockDate);

                if (!isAdditionOrDeduction)
                {
                    if (result.Success)
                        result = CreateTransactionsFromPayrollProductChain(template);
                    if (result.Success)
                        result = CreateQualifyingDeductionTransactions(template, applyAbsenceDays);
                    if (result.Success)
                        result = CreateTimeWorkReductionTransactions(template);
                    if (result.Success)
                        result = CreateFixedAccountingTransactions(template);
                    if (result.Success)
                        result = SaveTimePayrollTransactionAmounts(template.Identity);
                    if (result.Success)
                        result = SetTimePayrollScheduleTransactionAmounts(template.Identity.TimeBlockDate, SoeTimePayrollScheduleTransactionType.Absence);
                }

                AddCurrentDayNotifyChangeOfDeviations(template.Identity.TimeBlockDate, employment?.GetEmployeeGroup(template.Date, GetEmployeeGroupsFromCache()));
                AddCurrentDayPayrollWarning(template.Identity.TimeBlockDate);
            }

            ClearTimePayrollTransactionsWithTimeBlockDateFromCache(template.Identity.TimeBlockDate);
            this.SetDoNotCollectDaysForRecalculationLocally(null);

            #endregion
            
            return result;
        }

        private ActionResult SaveTimeCodeTransactions(List<TimeCodeTransactionDTO> timeCodeTransactions)
        {
            ActionResult result = new ActionResult(true);

            foreach (var timeCodeTransactionsByEmployee in timeCodeTransactions.GroupBy(g => g.EmployeeId))
            {
                #region Prereq

                int? employeeId = timeCodeTransactionsByEmployee.Key;
                Employee employee = employeeId.HasValue ? GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId.Value) : null;
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

                #endregion

                #region Perform

                foreach (TimeCodeTransactionDTO dto in timeCodeTransactionsByEmployee)
                {
                    #region Prereq

                    TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId.Value, dto.Start.Date, true);
                    if (timeBlockDate == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups: employeeGroups);
                    if (employeeGroup == null)
                        employeeGroup = employee.GetLastEmployeeGroup(GetEmployeeGroupsFromCache()); //Should be possible to report Addition/Deduction after last Employment
                    if (employeeGroup == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, timeBlockDate.Date.ToShortDateString()));

                    TimeCode timeCode = GetTimeCodeFromCache(dto.TimeCodeId);
                    if (timeCode == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                    #endregion

                    #region Repository

                    TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate);
                    TimeEngineTemplate template = CreateTemplate(identity); //Do not look for existing template for now

                    #endregion

                    #region TimeCodeTransaction

                    TimeCodeTransaction timeCodeTransaction;
                    if (dto.TimeCodeTransactionId != 0)
                    {
                        timeCodeTransaction = GetTimeCodeTransactionWithExternalTransactions(dto.TimeCodeTransactionId, false);
                        if (timeCodeTransaction == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(91904, "Tidkodtransaktion hittades inte ({0})"), dto.TimeCodeTransactionId));

                        SetModifiedProperties(timeCodeTransaction);
                    }
                    else
                    {
                        timeCodeTransaction = new TimeCodeTransaction();
                        entities.TimeCodeTransaction.AddObject(timeCodeTransaction);
                        SetCreatedProperties(timeCodeTransaction);
                    }

                    if (timeCodeTransaction.IsEarnedHoliday)
                    {
                        //Earned holiday transactions are only alloweed to be deleted from addition/deduction view
                        timeCodeTransaction.State = (int)dto.State;
                        if (timeCodeTransaction.State == (int)SoeEntityState.Deleted)
                        {
                            //Delete external transactions
                            result = SetExternalTransactionsToDeleted(timeCodeTransaction, saveChanges: false);
                            if (!result.Success)
                                return result;
                        }
                    }
                    else
                    {
                        if (timeCodeTransaction.TimeCodeTransactionId != 0)
                        {
                            //Delete external transactions
                            result = SetExternalTransactionsToDeleted(timeCodeTransaction, saveChanges: false);
                            if (!result.Success)
                                return result;
                        }

                        timeCodeTransaction.Start = dto.Start;
                        timeCodeTransaction.Stop = dto.Stop;
                        timeCodeTransaction.Amount = dto.Amount;
                        timeCodeTransaction.Vat = dto.Vat;
                        timeCodeTransaction.Quantity = dto.Quantity;
                        timeCodeTransaction.Comment = dto.Comment;
                        timeCodeTransaction.State = (int)dto.State;
                        timeCodeTransaction.Type = (int)dto.Type;
                        timeCodeTransaction.IsAdditionOrDeduction = timeCode.IsAdditionAndDeduction();
                        timeCodeTransaction.TimeCodeId = dto.TimeCodeId;
                        timeCodeTransaction.TimeBlockDate = timeBlockDate;

                        // Currency
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

                        if (timeCodeTransaction.State != (int)SoeEntityState.Deleted)
                            template.Outcome.TimeCodeTransactions.Add(timeCodeTransaction);

                        result = Save();
                        if (!result.Success)
                            return result;

                        if (timeCodeTransaction.State != (int)SoeEntityState.Deleted)
                        {
                            result = SaveExternalTransactions(ref template, isAdditionOrDeduction: true);
                            if (!result.Success)
                                return result;
                        }

                        var timePayrollTransactionItem = dto.TimePayrollTransactionItems.FirstOrDefault();//This is ugly, but it works?. Seems like it is only used to set accounting on the transactions, therefore FirstOrDefault. The accounting settings should be placed on the parent dto, because it is the same for all transactions, see how it is used in the GUI - ButtonSelectAccounts_Click
                        if (timePayrollTransactionItem != null && template.Outcome.TimePayrollTransactions.Count > 0)
                        {
                            var timePayrollTransaction = template.Outcome.TimePayrollTransactions.FirstOrDefault();
                            if (timePayrollTransaction == null)
                            {
                                timePayrollTransaction = new TimePayrollTransaction()
                                {
                                    ActorCompanyId = actorCompanyId,
                                };
                                SetCreatedProperties(timePayrollTransaction);
                            }

                            //AccountStd
                            timePayrollTransaction.AccountStdId = timePayrollTransactionItem.AccountStd.AccountId;

                            //AccountInternals
                            if (timePayrollTransactionItem.AccountInternals != null && timePayrollTransactionItem.AccountInternals.Count > 0)
                            {
                                if (!timePayrollTransaction.AccountInternal.IsLoaded)
                                    timePayrollTransaction.AccountInternal.Load();

                                timePayrollTransaction.AccountInternal.Clear();

                                foreach (var accountId in timePayrollTransactionItem.AccountInternals
                                    .Where(a => a.AccountId > 0)
                                    .Select(a => a.AccountId))
                                {
                                    if (!timePayrollTransaction.AccountInternal.Any(ai => ai.AccountId == accountId))
                                        TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, GetAccountInternalWithAccountFromCache(accountId));
                                }
                            }
                        }

                        //Can not be done in SaveExternalTransactions(), it has to be done after accountinternal has been set in this method, bacause they will be passed on to the child transactions
                        result = CreateTransactionsFromPayrollProductChain(template);
                        if (!result.Success)
                            return result;

                        result = CreateFixedAccountingTransactions(template);
                        if (!result.Success)
                            return result;

                    }
                    result = Save();
                    if (!result.Success)
                        return result;

                    #endregion
                }

                #endregion


            }

            return result;
        }

        private ActionResult SaveTimeCodeTransactions(List<AttestEmployeeDayTimeCodeTransactionDTO> inputTimeCodeTransactions, List<TimeBlock> timeBlocks, TimeBlockDate timeBlockDate)
        {
            if (inputTimeCodeTransactions == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeCodeTransactions");
            if (timeBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlocks");

            List<TimeCodeTransaction> savedTimeCodeTransactions = new List<TimeCodeTransaction>();

            foreach (AttestEmployeeDayTimeCodeTransactionDTO inputTimeCodeTransaction in inputTimeCodeTransactions)
            {
                #region TimeBlock

                TimeBlock timeBlock = null;
                if (inputTimeCodeTransaction.TimeBlockId > 0)
                {
                    //Already saved TimeCodeTransaction's have TimeBlockId relation
                    timeBlock = timeBlocks.FirstOrDefault(i => i.TimeBlockId == inputTimeCodeTransaction.TimeBlockId);
                    if (timeBlock == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlock");
                }
                else if (!String.IsNullOrEmpty(inputTimeCodeTransaction.GuidIdTimeBlock))
                {
                    //TimeCodeTransaction's for new TimeBlock's only have Guid relation
                    timeBlock = timeBlocks.FirstOrDefault(i => i.GuidId.HasValue && i.GuidId.Value.ToString() == inputTimeCodeTransaction.GuidIdTimeBlock);
                    if (timeBlock == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlock");
                }

                #endregion

                #region Add/Update

                TimeCodeTransaction existingTimeCodeTransaction = inputTimeCodeTransaction.TimeCodeTransactionId > 0 ? GetTimeCodeTransaction(inputTimeCodeTransaction.TimeCodeTransactionId, false) : null;
                if (existingTimeCodeTransaction != null && existingTimeCodeTransaction.Type != (int)TimeCodeTransactionType.Time)
                    continue;
                    
                if (existingTimeCodeTransaction == null)
                {
                    //Add
                    existingTimeCodeTransaction = CreateTimeCodeTransaction(inputTimeCodeTransaction.TimeCodeId, TimeCodeTransactionType.Time, inputTimeCodeTransaction.Quantity, inputTimeCodeTransaction.StartTime, inputTimeCodeTransaction.StopTime, timeBlockId: timeBlock?.TimeBlockId, timeBlockDate: timeBlockDate);
                }
                else
                {
                    //Update
                    existingTimeCodeTransaction.TimeCodeId = inputTimeCodeTransaction.TimeCodeId;
                    existingTimeCodeTransaction.TimeBlockId = timeBlock?.TimeBlockId;
                    existingTimeCodeTransaction.Type = (int)TimeCodeTransactionType.Time;
                    existingTimeCodeTransaction.Quantity = inputTimeCodeTransaction.Quantity;
                    existingTimeCodeTransaction.Start = inputTimeCodeTransaction.StartTime;
                    existingTimeCodeTransaction.Stop = inputTimeCodeTransaction.StopTime;
                    SetModifiedProperties(existingTimeCodeTransaction);
                }

                if (inputTimeCodeTransaction.TimeBlockId == 0)
                    inputTimeCodeTransaction.TimeBlockId = timeBlock?.TimeBlockId;
                if (inputTimeCodeTransaction.TimeRuleId > 0)
                    existingTimeCodeTransaction.TimeRuleId = inputTimeCodeTransaction.TimeRuleId;
                if (!String.IsNullOrEmpty(inputTimeCodeTransaction.GuidId))
                    existingTimeCodeTransaction.SetIdentifier(Guid.Parse(inputTimeCodeTransaction.GuidId));

                savedTimeCodeTransactions.Add(existingTimeCodeTransaction);

                #endregion
            }

            ActionResult result = Save();
            if (result.Success)
                result.Value = savedTimeCodeTransactions;

            return result;
        }

        private ActionResult SaveTimeInvoiceTransactions(List<TimeTransactionItem> timeTransactionItems, List<TimeCodeTransaction> timeCodeTransactions, DateTime dateFrom, DateTime dateTo, int employeeId)
        {
            ActionResult result = null;

            #region Prereq

            // Get AccountInternal's (Dim 2-6)
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            DateTime logDate = DateTime.Now;

            #endregion

            #region Perform

            #region Update/Delete TimeInvoiceTransaction

            // Get all TimeInvoiceTransaction for current Employee and date interval. Update if exists otherwise delete
            List<TimeInvoiceTransaction> existingTimeInvoiceTransactions = GetTimeInvoiceTransactions(dateFrom, dateTo, employeeId);
            foreach (TimeInvoiceTransaction timeInvoiceTransaction in existingTimeInvoiceTransactions)
            {
                TimeTransactionItem item = timeTransactionItems.FirstOrDefault(r => r.TimeTransactionId == timeInvoiceTransaction.TimeInvoiceTransactionId);
                if (item != null)
                {
                    #region Update TimeInvoiceTransaction

                    // Update existing transaction with data from item collection
                    timeInvoiceTransaction.Invoice = item.TransactionType == SoeTimeTransactionType.TimeInvoice;
                    timeInvoiceTransaction.InvoiceQuantity = item.InvoiceQuantity;
                    timeInvoiceTransaction.ManuallyAdded = item.ManuallyAdded;

                    //InvoiceProduct
                    if (timeInvoiceTransaction.ProductId != item.ProductId)
                        timeInvoiceTransaction.ProductId = item.ProductId;

                    //AccountStd
                    if (timeInvoiceTransaction.AccountStdId != item.Dim1Id)
                        timeInvoiceTransaction.AccountStdId = item.Dim1Id;

                    // TimeBlock
                    if (item.TimeBlockId > 0)
                        timeInvoiceTransaction.TimeBlockId = item.TimeBlockId;

                    // TimeCodeTransaction
                    if (timeCodeTransactions != null && item.GuidInternalFK.HasValue)
                        timeInvoiceTransaction.TimeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.Guid.HasValue && i.Guid.Value == item.GuidInternalFK.Value);

                    // Accounting
                    ApplyAccountingOnTimeInvoiceTransaction(timeInvoiceTransaction, item, accountInternals);

                    // Attest
                    if (!timeInvoiceTransaction.AttestStateId.HasValue || timeInvoiceTransaction.AttestStateId != item.AttestStateId)
                    {
                        int? previousAttestStateId = timeInvoiceTransaction.AttestStateId;
                        int newAttestStateId = item.AttestStateId;
                        if (previousAttestStateId.HasValue && newAttestStateId > 0)
                        {
                            AttestTransitionDTO attestTransition = GetUserAttestTransitionForState(TermGroup_AttestEntity.InvoiceTime, previousAttestStateId.Value, newAttestStateId);
                            if (attestTransition == null)
                                timeInvoiceTransaction.AttestStateId = previousAttestStateId;
                            else
                                UpdateTimeInvoiceTransactionAttestState(entities, actorCompanyId, timeInvoiceTransaction, attestTransition, logDate);
                        }
                    }

                    #endregion
                }
                else
                {
                    #region Delete TimeInvoiceTransaction

                    // Transaction does not exist in item collection, delete it
                    result = SetTimeInvoiceTransactionToDeleted(timeInvoiceTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;

                    #endregion
                }

                SetModifiedProperties(timeInvoiceTransaction);
            }

            #endregion

            #region Add TimeInvoiceTransaction

            List<TimeTransactionItem> newTransactionItems = timeTransactionItems.Where(i => i.TimeTransactionId == 0).ToList();
            if (newTransactionItems.Count > 0)
            {
                // Get/Create TimeBlockDate
                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, dateFrom, true);
                if (timeBlockDate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                // Add new transactions from the items that are added in the collection
                foreach (TimeTransactionItem item in newTransactionItems)
                {
                    #region TimeInvoiceTransactionItem

                    TimeInvoiceTransaction timeInvoiceTransaction = new TimeInvoiceTransaction()
                    {
                        Amount = 0,
                        VatAmount = 0,
                        Quantity = 0,
                        InvoiceQuantity = item.InvoiceQuantity,
                        Invoice = item.TransactionType == SoeTimeTransactionType.TimeInvoice,
                        ManuallyAdded = item.ManuallyAdded,
                        Exported = false,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employeeId,
                        ProductId = item.ProductId,
                        AccountStdId = item.Dim1Id,
                        AttestStateId = item.AttestStateId,
                        TimeBlockId = (item.TimeBlockId > 0 ? item.TimeBlockId : (int?)null),

                        //Set references (may be created)
                        TimeBlockDate = timeBlockDate,
                    };
                    SetModifiedProperties(timeInvoiceTransaction);
                    entities.TimeInvoiceTransaction.AddObject(timeInvoiceTransaction);

                    SetTimeInvoiceTransactionCurrencyAmounts(timeInvoiceTransaction);
                    ApplyAccountingOnTimeInvoiceTransaction(timeInvoiceTransaction, item, accountInternals);

                    #region TimeCodeTransaction

                    if (timeCodeTransactions != null && item.GuidInternalFK.HasValue)
                        timeInvoiceTransaction.TimeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.Guid.HasValue && i.Guid.Value == item.GuidInternalFK.Value);

                    #endregion

                    #region Attest

                    // Get AttestState initial
                    AttestStateDTO attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
                    if (attestStateInitialInvoice != null)
                    {
                        int initialAttestStateId = attestStateInitialInvoice.AttestStateId;
                        int? newAttestStateId = timeInvoiceTransaction.AttestStateId;
                        if (newAttestStateId.HasValue && newAttestStateId.Value > 0 && newAttestStateId.Value != attestStateInitialInvoice.AttestStateId)
                        {
                            //Get AttestTransition
                            AttestTransitionDTO attestTransition = GetUserAttestTransitionForState(TermGroup_AttestEntity.InvoiceTime, initialAttestStateId, newAttestStateId.Value);
                            if (attestTransition == null)
                                timeInvoiceTransaction.AttestStateId = initialAttestStateId;
                            else
                                UpdateTimeInvoiceTransactionAttestState(entities, actorCompanyId, timeInvoiceTransaction, attestTransition, logDate);
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            #endregion

            result = Save();

            #endregion

            return result;
        }

        private ActionResult SaveTimePayrollTransactions(List<TimeTransactionItem> timeTransactionItems, List<TimeCodeTransaction> timeCodeTransactions, DateTime dateFrom, DateTime dateTo, int employeeId)
        {
            ActionResult result = null;

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            var payrollTransactionMapping = new List<Tuple<Guid, TimePayrollTransaction>>();

            #region Prereq

            // Get/Create TimeBlockDate
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, dateFrom, true);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            // Get Employment
            Employment employment = employee.GetEmployment(timeBlockDate.Date, timeBlockDate.Date);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            // Get AccountInternal's (Dim 2-6)
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            //Filter away IsScheduleTransaction (precaution)
            timeTransactionItems = timeTransactionItems.Where(i => !i.IsScheduleTransaction).ToList();

            //VacationGroups
            List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();

            #endregion

            #region Perform

            #region Update/Delete TimePayrollTransaction

            List<TimePayrollTransaction> existingTimePayrollTransactions = GetTimePayrollTransactions(employeeId, dateFrom, dateTo);
            foreach (TimePayrollTransaction timePayrollTransaction in existingTimePayrollTransactions)
            {
                TimeTransactionItem item = timeTransactionItems.FirstOrDefault(r => r.TimeTransactionId == timePayrollTransaction.TimePayrollTransactionId);
                if (item != null)
                {
                    #region Update TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransaction.ProductId);
                    if (payrollProduct == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                    timePayrollTransaction.EmployeeChildId = (item.EmployeeChildId > 0) ? item.EmployeeChildId : (int?)null;
                    timePayrollTransaction.IncludedInPayrollProductChain = item.IncludedInPayrollProductChain;
                    timePayrollTransaction.ManuallyAdded = item.ManuallyAdded;
                    if (timePayrollTransaction.ManuallyAdded)
                        timePayrollTransaction.Comment = item.Comment; //We are only interested in saving comments for manually added transactions on the TimePayrollTransaction entity autogenerated transactions gets their comment from its corresponding timeblock
                    if (timePayrollTransaction.ProductId != item.ProductId)
                        timePayrollTransaction.ProductId = item.ProductId;
                    if (item.TimeBlockId > 0)
                        timePayrollTransaction.TimeBlockId = item.TimeBlockId;

                    CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                    SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                    SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employment, timeBlockDate, item.Quantity, vacationGroups: vacationGroups);
                    SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);

                    // Accounting
                    if (timePayrollTransaction.AccountStdId != item.Dim1Id)
                        timePayrollTransaction.AccountStdId = item.Dim1Id;
                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, item, accountInternals);

                    //TimeCodeTransactions
                    if (timeCodeTransactions != null && item.GuidInternalFK.HasValue)
                        timePayrollTransaction.TimeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.Guid.HasValue && i.Guid.Value == item.GuidInternalFK.Value);

                    //Attest
                    CreateTimePayrollTransactionAttestTransitionLog(timePayrollTransaction, item.AttestStateId);

                    //PayrollProductChain
                    if (item.IncludedInPayrollProductChain && !item.ManuallyAdded && item.GuidId.HasValue)
                        payrollTransactionMapping.Add(Tuple.Create(item.GuidId.Value, timePayrollTransaction));

                    #endregion
                }
                else
                {
                    #region Delete TimePayrollTransaction

                    //Dont delete transactions created by payroll (ex arbetsgivaravgift) (occurs when applying absence on a reversed day in locked period)
                    if (!timePayrollTransaction.IsExcludedInTime())
                    {
                        // Transaction does not exist in item collection, delete it
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransaction, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }

                    #endregion
                }

                SetModifiedProperties(timePayrollTransaction);
            }

            #endregion

            #region Add TimePayrollTransaction

            List<TimeTransactionItem> newTransactionItems = existingTimePayrollTransactions.Any() ? timeTransactionItems.Where(i => i.TimeTransactionId == 0).ToList() : timeTransactionItems;
            if (newTransactionItems.Any())
            {
                foreach (TimeTransactionItem item in newTransactionItems)
                {
                    #region TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(item.ProductId);
                    if (payrollProduct == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                    TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction()
                    {
                        Amount = 0,
                        VatAmount = 0,
                        Quantity = item.Quantity,
                        IsPreliminary = false,
                        ManuallyAdded = item.ManuallyAdded,
                        IncludedInPayrollProductChain = item.IncludedInPayrollProductChain,
                        Exported = false,
                        AutoAttestFailed = false,
                        Comment = item.ManuallyAdded ? item.Comment : String.Empty, //Autogenerated transaction gets its comment from its corresponding timeblock

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employeeId,
                        ProductId = item.ProductId,
                        AttestStateId = item.AttestStateId,
                        TimeBlockId = (item.TimeBlockId > 0 ? item.TimeBlockId : (int?)null),
                        EmployeeChildId = (item.EmployeeChildId > 0 ? item.EmployeeChildId : (int?)null),

                        //Set references (may be created)
                        TimeBlockDate = timeBlockDate,
                    };
                    SetCreatedProperties(timePayrollTransaction);
                    entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

                    CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                    SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                    SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employment, timeBlockDate, item.Quantity, vacationGroups: vacationGroups);
                    SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);

                    // Accounting
                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, item, accountInternals);

                    // TimeCodeTransactions
                    if (timeCodeTransactions != null && item.GuidInternalFK.HasValue)
                        timePayrollTransaction.TimeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.Guid.HasValue && i.Guid.Value == item.GuidInternalFK.Value);

                    // Attest
                    AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitialPayroll != null)
                        timePayrollTransaction.AttestStateId = attestStateInitialPayroll.AttestStateId;

                    //PayrollProductChain
                    if (item.IncludedInPayrollProductChain && !item.ManuallyAdded && item.GuidId.HasValue)
                        payrollTransactionMapping.Add(Tuple.Create(item.GuidId.Value, timePayrollTransaction));

                    //FixedAccounting
                    if (item.ManuallyAdded)
                    {
                        #region PayrollProductChain/Fixed Accounting

                        List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction> { timePayrollTransaction };

                        result = CreateTransactionsFromPayrollProductChain(timePayrollTransaction, employee, timeBlockDate, out List<TimePayrollTransaction> childTransactions);
                        if (!result.Success)
                            return result;

                        timePayrollTransactions.AddRange(childTransactions);

                        result = CreateFixedAccountingTransactions(timePayrollTransactions, employee, timeBlockDate, out List<TimePayrollTransaction> fixedAccountingTransactions);
                        if (!result.Success)
                            return result;

                        timePayrollTransactions.AddRange(fixedAccountingTransactions);

                        #endregion
                    }

                    #endregion
                }
            }

            #endregion

            #region PayrollProductChain - set parent

            SetPayrollProductChainParent(timeTransactionItems, payrollTransactionMapping);

            #endregion

            result = Save();
            if (result.Success)
                result = SaveTimePayrollTransactionAmounts(timeBlockDate);

            #endregion

            return result;
        }

        private ActionResult SaveTimePayrollTransactions(Employee employee, List<AttestPayrollTransactionDTO> timePayrollTransactions, List<TimeCodeTransaction> timeCodeTransactions = null, bool keepExistingTransactions = false)
        {
            ActionResult result = new ActionResult(true);

            if (!timePayrollTransactions.IsNullOrEmpty())
            {
                foreach (var timePayrollTransactionsByDate in timePayrollTransactions.GroupBy(g => g.Date))
                {
                    DateTime date = timePayrollTransactionsByDate.Key;
                    result = SaveTimePayrollTransactions(employee, timePayrollTransactionsByDate.ToList(), timeCodeTransactions ?? new List<TimeCodeTransaction>(), date, date, keepExistingTransactions);
                    if (!result.Success)
                        return result;
                }
            }

            return result;
        }

        private ActionResult SaveTimePayrollTransactions(Employee employee, List<AttestPayrollTransactionDTO> inputTimePayrollTransactions, List<TimeCodeTransaction> timeCodeTransactions, DateTime dateFrom, DateTime dateTo, bool keepExistingTransactions = false)
        {
            ActionResult result;

            #region Init

            if (inputTimePayrollTransactions == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePayrollTransactions");
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            #endregion

            #region Prereq

            Employment employment = employee.GetEmployment(dateFrom);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, dateFrom, true);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();
            List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();

            //Filter away IsScheduleTransaction (precaution)
            inputTimePayrollTransactions = inputTimePayrollTransactions.Where(i => !i.IsScheduleTransaction).ToList();

            var payrollTransactionMapping = new List<Tuple<string, TimePayrollTransaction>>();
            List<TimePayrollTransaction> importedTimePayrollTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollTransaction> savedTimePayrollTransactions = new List<TimePayrollTransaction>();

            #endregion

            #region Perform

            #region Update/Delete TimePayrollTransaction

            List<TimePayrollTransaction> existingTimePayrollTransactions = GetTimePayrollTransactions(employee.EmployeeId, dateFrom, dateTo);
            foreach (TimePayrollTransaction timePayrollTransaction in existingTimePayrollTransactions)
            {
                AttestPayrollTransactionDTO inputTimePayrollTransaction = inputTimePayrollTransactions.FirstOrDefault(r => r.TimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId);
                if (inputTimePayrollTransaction != null)
                {
                    #region Update TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransaction.ProductId);
                    if (payrollProduct == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                    timePayrollTransaction.EmployeeChildId = (inputTimePayrollTransaction.EmployeeChildId > 0) ? inputTimePayrollTransaction.EmployeeChildId : (int?)null;
                    timePayrollTransaction.IncludedInPayrollProductChain = inputTimePayrollTransaction.IncludedInPayrollProductChain;
                    timePayrollTransaction.ManuallyAdded = inputTimePayrollTransaction.ManuallyAdded;
                    if (timePayrollTransaction.ManuallyAdded)
                        timePayrollTransaction.Comment = inputTimePayrollTransaction.Comment; //We are only interested in saving comments for manually added transactions on the TimePayrollTransaction entity autogenerated transactions gets their comment from its corresponding timeblock
                    if (timePayrollTransaction.ProductId != inputTimePayrollTransaction.PayrollProductId)
                        timePayrollTransaction.ProductId = inputTimePayrollTransaction.PayrollProductId;
                    if (inputTimePayrollTransaction.TimeBlockId > 0)
                        timePayrollTransaction.TimeBlockId = inputTimePayrollTransaction.TimeBlockId;
                    if (inputTimePayrollTransaction.PayrollImportEmployeeTransactionId.HasValue)
                        timePayrollTransaction.PayrollImportEmployeeTransactionId = inputTimePayrollTransaction.PayrollImportEmployeeTransactionId;
                    timePayrollTransaction.TimeCodeTransaction = timeCodeTransactions.Get(inputTimePayrollTransaction.GuidIdTimeCodeTransaction);

                    CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                    SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                    SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employment, timeBlockDate, inputTimePayrollTransaction.Quantity, vacationGroups: vacationGroups);
                    SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
                    if (timePayrollTransaction.AccountStdId != inputTimePayrollTransaction.AccountStdId)
                        timePayrollTransaction.AccountStdId = inputTimePayrollTransaction.AccountStdId;
                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, inputTimePayrollTransaction, accountInternals);
                    CreateTimePayrollTransactionAttestTransitionLog(timePayrollTransaction, inputTimePayrollTransaction.AttestStateId);

                    if (inputTimePayrollTransaction.IncludedInPayrollProductChain && !String.IsNullOrEmpty(inputTimePayrollTransaction.GuidId))
                        payrollTransactionMapping.Add(Tuple.Create(inputTimePayrollTransaction.GuidId, timePayrollTransaction));

                    if (timePayrollTransaction.PayrollImportEmployeeTransactionId.HasValue)
                        importedTimePayrollTransactions.Add(timePayrollTransaction);

                    savedTimePayrollTransactions.Add(timePayrollTransaction);

                    #endregion
                }
                else
                {
                    #region Delete TimePayrollTransaction

                    //Dont delete transactions created by payroll (ex arbetsgivaravgift) (occurs when applying absence on a reversed day in locked period)
                    if (!keepExistingTransactions && !timePayrollTransaction.IsExcludedInTime() && !timePayrollTransaction.IsAdditionOrDeduction)
                    {
                        // Transaction does not exist in item collection, delete it
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransaction, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }

                    #endregion
                }

                SetModifiedProperties(timePayrollTransaction);
            }

            #endregion

            #region Add TimePayrollTransaction

            List<AttestPayrollTransactionDTO> newTimePayrollTransactions = existingTimePayrollTransactions.Any() ? inputTimePayrollTransactions.Where(i => i.TimePayrollTransactionId == 0).ToList() : inputTimePayrollTransactions;
            foreach (AttestPayrollTransactionDTO newTimePayrollTransaction in newTimePayrollTransactions)
            {
                #region TimePayrollTransaction

                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(newTimePayrollTransaction.PayrollProductId);
                if (payrollProduct == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction()
                {
                    Amount = 0,
                    VatAmount = 0,
                    Quantity = newTimePayrollTransaction.Quantity,
                    IsPreliminary = false,
                    ManuallyAdded = newTimePayrollTransaction.ManuallyAdded,
                    IncludedInPayrollProductChain = newTimePayrollTransaction.IncludedInPayrollProductChain,
                    Exported = false,
                    AutoAttestFailed = false,
                    Comment = newTimePayrollTransaction.ManuallyAdded ? newTimePayrollTransaction.Comment : String.Empty, //Autogenerated transaction gets its comment from its corresponding timeblock

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employee.EmployeeId,
                    ProductId = newTimePayrollTransaction.PayrollProductId,
                    AttestStateId = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime)?.AttestStateId ?? newTimePayrollTransaction.AttestStateId,
                    TimeBlockId = newTimePayrollTransaction.TimeBlockId.ToNullable(),
                    EmployeeChildId = newTimePayrollTransaction.EmployeeChildId.ToNullable(),
                    TimePeriodId = newTimePayrollTransaction.TimePeriodId,
                    PayrollImportEmployeeTransactionId = newTimePayrollTransaction.PayrollImportEmployeeTransactionId,

                    //Set references
                    TimeBlockDate = timeBlockDate,
                    TimeCodeTransaction = timeCodeTransactions.Get(newTimePayrollTransaction.GuidIdTimeCodeTransaction)
                };
                SetCreatedProperties(timePayrollTransaction);
                entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

                CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employment, timeBlockDate, newTimePayrollTransaction.Quantity, vacationGroups: vacationGroups);
                SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
                ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, newTimePayrollTransaction, accountInternals);

                if (newTimePayrollTransaction.IncludedInPayrollProductChain && !String.IsNullOrEmpty(newTimePayrollTransaction.GuidId))
                    payrollTransactionMapping.Add(Tuple.Create(newTimePayrollTransaction.GuidId, timePayrollTransaction));

                if (timePayrollTransaction.PayrollImportEmployeeTransactionId.HasValue)
                    importedTimePayrollTransactions.Add(timePayrollTransaction);

                if (newTimePayrollTransaction.ManuallyAdded)
                {
                    List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction> { timePayrollTransaction };

                    result = CreateTransactionsFromPayrollProductChain(timePayrollTransaction, employee, timeBlockDate, out List<TimePayrollTransaction> childTransactions);
                    if (!result.Success)
                        return result;

                    timePayrollTransactions.AddRange(childTransactions);
                    if (timePayrollTransaction.PayrollImportEmployeeTransactionId.HasValue)
                        importedTimePayrollTransactions.AddRange(childTransactions);

                    result = CreateFixedAccountingTransactions(timePayrollTransactions, employee, timeBlockDate, out List<TimePayrollTransaction> fixedAccountingTransactions);
                    if (!result.Success)
                        return result;

                    timePayrollTransactions.AddRange(fixedAccountingTransactions);
                    if (timePayrollTransaction.PayrollImportEmployeeTransactionId.HasValue)
                        importedTimePayrollTransactions.AddRange(fixedAccountingTransactions);
                }

                savedTimePayrollTransactions.Add(timePayrollTransaction);

                #endregion
            }

            #endregion

            SetPayrollProductChainParent(inputTimePayrollTransactions, payrollTransactionMapping);

            result = Save();
            if (!result.Success)
                return result;

            result = SaveTimePayrollTransactionAmounts(timeBlockDate);
            if (!result.Success)
                return result;

            result = SavePayrollImportEmployeeTransactionLinks(timeBlockDate.Date, employee.EmployeeId, importedTimePayrollTransactions);
            if (!result.Success)
                return result;

            #endregion

            if (result.Success)
                result.Value = savedTimePayrollTransactions;

            return result;
        }

        private ActionResult SaveTimePayrollScheduleTransactions(Dictionary<int, List<DateTime>> employeeDates)
        {
            ActionResult result = new ActionResult(true);

            if (!employeeDates.IsNullOrEmpty())
            {
                foreach (var employeeDate in employeeDates)
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeDate.Key);
                    if (employee == null)
                        continue;

                    List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employee.EmployeeId, employeeDate.Value);
                    if (timeBlockDates.IsNullOrEmpty())
                        continue;

                    result = SaveTimePayrollScheduleTransactions(employee, timeBlockDates);
                    if (!result.Success)
                        return result;
                }
            }

            return result;
        }

        private ActionResult SaveTimePayrollScheduleTransactions(Employee employee, List<TimeBlockDate> timeBlockDates, TimePeriod timePeriod = null, bool createTaxDebet = false, bool createSupplementCharge = false)
        {
            ActionResult result = new ActionResult(true);

            #region Inits

            bool doRequireTimePeriod = createTaxDebet || createSupplementCharge;
            bool doCalculateAmount = doRequireTimePeriod;

            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (timePeriod == null && doRequireTimePeriod)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8693, "Period hittades inte"));
            if (timeBlockDates == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");

            if (timeBlockDates.Count == 0)
                return result;

            #endregion

            #region Prereq

            PayrollProduct payrollProductEmploymentTaxDebet = null;
            if (createTaxDebet)
            {
                payrollProductEmploymentTaxDebet = GetPayrollProductEmploymentTaxDebet();
                if (payrollProductEmploymentTaxDebet == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PayrollProductEmploymentTaxDebet");
            }
            PayrollProduct payrollProductSupplementChargeDebet = null;
            if (createSupplementCharge)
            {
                payrollProductSupplementChargeDebet = GetPayrollProductSupplementChargeDebet();
                if (payrollProductSupplementChargeDebet == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "payrollProductSupplementChargeDebet");
            }

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            List<TimePayrollScheduleTransaction> existingScheduleTransactionsForPeriod = GetTimePayrollScheduleTransactions(employee.EmployeeId, timeBlockDates.GetFirstDate(), timeBlockDates.GetLastDate(), timePeriod?.TimePeriodId);
            existingScheduleTransactionsForPeriod = existingScheduleTransactionsForPeriod.Where(x => !x.IsRetroTransaction()).ToList();

            bool currentDoNotCalculateAmounts = DoNotCalculateAmounts();
            if (!doCalculateAmount)
                SetDoNotCalculateAmounts(true);

            #endregion

            try
            {
                #region Perform

                foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(i => i.Date))
                {
                    #region Handle Existing schedule transactions (this needs to be done first since scheduleblocks may have been deleted since last time)

                    //Delete existing schedule transactions of type Schedule
                    List<TimePayrollScheduleTransaction> existingScheduleTransactionsOfTypeSchedule = existingScheduleTransactionsForPeriod.Get(SoeTimePayrollScheduleTransactionType.Schedule, timeBlockDate.TimeBlockDateId);
                    SetTimePayrollScheduleTransactionsToDeleted(existingScheduleTransactionsOfTypeSchedule, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);

                    //Delete existing schedule transactions of type absence and payrollproduct employment tax and supplementcharge- they will be recalculated
                    if (createTaxDebet || createSupplementCharge)
                    {
                        List<TimePayrollScheduleTransaction> existingScheduleTransactionsOfTypeAbsence = existingScheduleTransactionsForPeriod.Get(SoeTimePayrollScheduleTransactionType.Absence, timeBlockDate.TimeBlockDateId);
                        if (createTaxDebet)
                            SetTimePayrollScheduleTransactionsToDeleted(existingScheduleTransactionsOfTypeAbsence.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit).ToList(), saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);
                        if (createSupplementCharge)
                            SetTimePayrollScheduleTransactionsToDeleted(existingScheduleTransactionsOfTypeAbsence.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit).ToList(), saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);
                        SetTimePayrollScheduleTransactionAmounts(existingScheduleTransactionsOfTypeAbsence, employee, timeBlockDate);
                    }

                    result = Save();
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Prereq

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());
                    if (employeeGroup == null)
                        continue;

                    DayType dayType = GetDayTypeForEmployeeFromCache(employee.EmployeeId, timeBlockDate.Date);
                    if (dayType == null)
                        continue;

                    List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employee.EmployeeId, timeBlockDate.Date);
                    if (scheduleBlocks.IsNullOrEmpty())
                        continue;

                    int standardTimeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(employee, employeeGroup, null, true);

                    #endregion

                    #region Create temporary TimeBlocks from TimeScheduleTemplateBlocks

                    List<TimeBlock> timeBlocks = CreateTimeBlocksFromTemplate(employee, scheduleBlocks, timeBlockDate, standardTimeDeviationCauseId, temporary: true);
                    timeBlocks.ForEach(timeBlock => timeBlock.IsSchedulePreliminaryTimeBlock = true);

                    #endregion

                    #region Generate transactions

                    TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate, scheduleBlocks.First().TimeScheduleTemplatePeriodId, standardTimeDeviationCauseId, null, timeBlocks, dayType, IsAnyTimeBlockAbsence(timeBlocks));
                    TimeEngineTemplate template = CreateTemplate(identity); //Do not look for existing template for now

                    result = GenerateInternalTransactions(ref template);
                    if (!result.Success)
                        return result;

                    List<TimeTransactionItem> externalTimeTransactionItems = GenerateExternalTransactions(template);
                    CreateTimePayrollScheduleTransactions(externalTimeTransactionItems, timeBlocks, scheduleBlocks, timeBlockDate, employee, doCalculateAmount);

                    #endregion

                    #region Detach

                    base.TryDetachEntitys(entities, template.Identity.TimeBlocks);
                    base.TryDetachEntitys(entities, template.Outcome.TimeCodeTransactions);

                    #endregion

                    #region Save

                    result = Save();
                    if (!result.Success)
                        return result;

                    #endregion

                    #region EmploymentTax

                    if (createTaxDebet)
                    {
                        result = CreateEmploymentTaxDebetScheduleTransactions(employee, timeBlockDate, timePeriod, null, payrollProductEmploymentTaxDebet, 0);
                        if (!result.Success)
                            return result;
                    }

                    #endregion

                    #region SupplementCharge

                    if (createSupplementCharge)
                    {
                        result = CreateSupplementChargeDebetScheduleTransactions(employee, timeBlockDate, timePeriod, null, payrollProductSupplementChargeDebet, 0);
                        if (!result.Success)
                            return result;
                    }

                    #endregion
                }

                return result;

                #endregion
            }
            finally
            {
                if (!doCalculateAmount)
                    SetDoNotCalculateAmounts(currentDoNotCalculateAmounts);
            }
        }

        private ActionResult SaveTimePayrollScheduleTransactions(List<TimeTransactionItem> timeTransactionItems, DateTime dateFrom, DateTime dateTo, int employeeId)
        {
            ActionResult result = null;

            #region Prereq

            // Get/Create TimeBlockDate
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, dateFrom, true);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            // Get AccountInternal's (Dim 2-6)
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            //Filter away non IsScheduleTransaction (precaution)
            timeTransactionItems = timeTransactionItems.Where(i => i.IsScheduleTransaction).ToList();

            #endregion

            #region Perform

            #region Update/Delete TimePayrollScheduleTransaction

            // Get all TimePayrollScheduleTransaction's for current Employee and date interval. Update if exists otherwise delete
            List<TimePayrollScheduleTransaction> existingTimePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(employeeId, dateFrom, dateTo, null, SoeTimePayrollScheduleTransactionType.Absence);
            foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in existingTimePayrollScheduleTransactions)
            {
                TimeTransactionItem item = timeTransactionItems.FirstOrDefault(r => r.TimeTransactionId == timePayrollScheduleTransaction.TimePayrollScheduleTransactionId);
                if (item != null)
                {
                    #region Update TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransaction.ProductId);

                    timePayrollScheduleTransaction.SetTimePayrollTransactionType(payrollProduct);
                    timePayrollScheduleTransaction.Quantity = item.Quantity;
                    if (timePayrollScheduleTransaction.ProductId != item.ProductId)
                        timePayrollScheduleTransaction.ProductId = item.ProductId;
                    if (timePayrollScheduleTransaction.AccountStdId != item.Dim1Id)
                        timePayrollScheduleTransaction.AccountStdId = item.Dim1Id;

                    // Accounting
                    ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, item, accountInternals);

                    #endregion
                }
                else
                {
                    #region Delete TimePayrollTransaction

                    // Transaction does not exist in item collection, delete it
                    result = SetTimePayrollScheduleTransactionToDeleted(timePayrollScheduleTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;

                    #endregion
                }

                SetModifiedProperties(timePayrollScheduleTransaction);
            }

            #endregion

            #region Add TimePayrollScheduleTransaction

            // Add new transactions from the items that are added in the collection
            List<TimeTransactionItem> newTransactionItems = timeTransactionItems.Where(i => i.TimeTransactionId == 0).ToList();
            if (newTransactionItems.Count > 0)
            {
                foreach (TimeTransactionItem item in newTransactionItems)
                {
                    #region TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(item.ProductId);
                    if (payrollProduct == null)
                        continue;

                    TimePayrollScheduleTransaction timePayrollScheduleTransaction = new TimePayrollScheduleTransaction
                    {
                        Type = (int)SoeTimePayrollScheduleTransactionType.Absence,
                        Quantity = item.Quantity,
                        Amount = 0,
                        AmountCurrency = 0,
                        AmountLedgerCurrency = 0,
                        AmountEntCurrency = 0,
                        VatAmount = 0,
                        VatAmountCurrency = 0,
                        VatAmountLedgerCurrency = 0,
                        VatAmountEntCurrency = 0,
                        UnitPrice = 0,
                        UnitPriceCurrency = 0,
                        UnitPriceLedgerCurrency = 0,
                        UnitPriceEntCurrency = 0,
                        SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                        TimeBlockStartTime = null,
                        TimeBlockStopTime = null,
                        Formula = null,
                        FormulaPlain = null,
                        FormulaExtracted = null,
                        FormulaNames = null,
                        FormulaOrigin = null,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employeeId,
                        ProductId = payrollProduct.ProductId,
                        TimeScheduleTemplatePeriodId = null,
                        TimeScheduleTemplateBlockId = null,
                        PayrollPriceFormulaId = null,
                        PayrollPriceTypeId = null,

                        //Set references
                        TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                    };
                    SetCreatedProperties(timePayrollScheduleTransaction);
                    entities.TimePayrollScheduleTransaction.AddObject(timePayrollScheduleTransaction);

                    // Accounting
                    ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, item, accountInternals);

                    #endregion
                }
            }

            #endregion

            result = Save();
            if (result.Success)
                result = SetTimePayrollScheduleTransactionAmounts(timeBlockDate, SoeTimePayrollScheduleTransactionType.Absence);

            #endregion

            return result;
        }

        private ActionResult SaveTimePayrollScheduleTransactions(List<AttestPayrollTransactionDTO> timePayrollScheduleTransactionDTOs, DateTime dateFrom, DateTime dateTo, Employee employee)
        {
            ActionResult result = null;

            #region Prereq

            // Get/Create TimeBlockDate
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, dateFrom, true);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            // Get AccountInternal's (Dim 2-6)
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            //Filter away non IsScheduleTransaction (precaution)
            timePayrollScheduleTransactionDTOs = timePayrollScheduleTransactionDTOs.Where(i => i.IsScheduleTransaction).ToList();

            #endregion

            #region Perform

            #region Update/Delete TimePayrollScheduleTransaction

            // Get all TimePayrollScheduleTransaction's for current Employee and date interval. Update if exists otherwise delete
            List<TimePayrollScheduleTransaction> existingTimePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(employee.EmployeeId, dateFrom, dateTo, null, SoeTimePayrollScheduleTransactionType.Absence);
            foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in existingTimePayrollScheduleTransactions)
            {
                AttestPayrollTransactionDTO timePayrollScheduleTransactionDTO = timePayrollScheduleTransactionDTOs.FirstOrDefault(r => r.TimePayrollTransactionId == timePayrollScheduleTransaction.TimePayrollScheduleTransactionId);
                if (timePayrollScheduleTransactionDTO != null)
                {
                    #region Update TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransaction.ProductId);

                    timePayrollScheduleTransaction.SetTimePayrollTransactionType(payrollProduct);
                    timePayrollScheduleTransaction.Quantity = timePayrollScheduleTransactionDTO.Quantity;
                    if (timePayrollScheduleTransaction.ProductId != timePayrollScheduleTransactionDTO.PayrollProductId)
                        timePayrollScheduleTransaction.ProductId = timePayrollScheduleTransactionDTO.PayrollProductId;

                    // Accounting
                    ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, timePayrollScheduleTransactionDTO, accountInternals);

                    #endregion
                }
                else
                {
                    #region Delete TimePayrollTransaction

                    // Transaction does not exist in item collection, delete it
                    result = SetTimePayrollScheduleTransactionToDeleted(timePayrollScheduleTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;

                    #endregion
                }

                SetModifiedProperties(timePayrollScheduleTransaction);
            }

            #endregion

            #region Add TimePayrollScheduleTransaction

            // Add new transactions from the items that are added in the collection
            List<AttestPayrollTransactionDTO> newTimePayrollTransactionDTOs = timePayrollScheduleTransactionDTOs.Where(i => i.TimePayrollTransactionId == 0).ToList();
            if (newTimePayrollTransactionDTOs.Count > 0)
            {
                foreach (AttestPayrollTransactionDTO newTimePayrollTransactionDTO in newTimePayrollTransactionDTOs)
                {
                    #region TimePayrollTransaction

                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(newTimePayrollTransactionDTO.PayrollProductId);
                    if (payrollProduct == null)
                        continue;

                    TimePayrollScheduleTransaction timePayrollScheduleTransaction = new TimePayrollScheduleTransaction
                    {
                        Type = (int)SoeTimePayrollScheduleTransactionType.Absence,
                        Quantity = newTimePayrollTransactionDTO.Quantity,
                        Amount = 0,
                        AmountCurrency = 0,
                        AmountLedgerCurrency = 0,
                        AmountEntCurrency = 0,
                        VatAmount = 0,
                        VatAmountCurrency = 0,
                        VatAmountLedgerCurrency = 0,
                        VatAmountEntCurrency = 0,
                        UnitPrice = 0,
                        UnitPriceCurrency = 0,
                        UnitPriceLedgerCurrency = 0,
                        UnitPriceEntCurrency = 0,
                        SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                        TimeBlockStartTime = null,
                        TimeBlockStopTime = null,
                        Formula = null,
                        FormulaPlain = null,
                        FormulaExtracted = null,
                        FormulaNames = null,
                        FormulaOrigin = null,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employee.EmployeeId,
                        ProductId = payrollProduct.ProductId,
                        TimeScheduleTemplatePeriodId = null,
                        TimeScheduleTemplateBlockId = null,
                        PayrollPriceFormulaId = null,
                        PayrollPriceTypeId = null,

                        //Set references
                        TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                    };
                    SetCreatedProperties(timePayrollScheduleTransaction);
                    entities.TimePayrollScheduleTransaction.AddObject(timePayrollScheduleTransaction);

                    // Accounting
                    ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, newTimePayrollTransactionDTO, accountInternals);

                    #endregion
                }
            }

            #endregion

            result = Save();
            if (result.Success)
                result = SetTimePayrollScheduleTransactionAmounts(timeBlockDate, SoeTimePayrollScheduleTransactionType.Absence);

            #endregion

            return result;
        }

        private ActionResult SaveTimePayrollTransactionsForAccountProvision(TimePeriod timePeriod)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            if (timePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));

            DateTime startDate = CalendarUtility.GetBeginningOfDay(timePeriod.StartDate);
            DateTime stopDate = CalendarUtility.GetBeginningOfDay(timePeriod.StopDate);

            int accountProvisionAccountDimId = GetCompanyIntSettingFromCache(CompanySettingType.PayrollAccountProvisionAccountDim);
            if (accountProvisionAccountDimId == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10042, "Företagsinställning saknas, kontodimension för provisionsunderlag"));

            int accountProvisionTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.PayrollAccountProvisionTimeCode);
            if (accountProvisionTimeCodeId == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10043, "Företagsinställning saknas, tidkod för service provision"));
            TimeCode timeCode = GetTimeCodeWithProductsFromCache(accountProvisionTimeCodeId);
            if (timeCode == null || timeCode.TimeCodePayrollProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10044, "Tidkod för service provision hittades inte"));

            List<TimePeriodAccountValue> timePeriodAccountValues = GetTimePeriodAccountValues(timePeriod.TimePeriodId);
            if (timePeriodAccountValues.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10045, "Underlag för provision saknas"));

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10046, "Attestnivå med startnivå hittades inte"));

            List<Employee> employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, active: true, loadEmployment: true);
            if (employees.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10047, "Inga anställda hittades, transkationer för provision kan ej skapas"));

            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForCompanyWithTimeCodeAndAccountInternals(startDate, stopDate);
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10048, "Ingen närvaro hittades i vald period, transaktioner för provision kan ej skapas"));

            timePayrollTransactions = timePayrollTransactions.Where(i => i.TimeCodeTransaction == null || !i.TimeCodeTransaction.IsProvision).ToList();
            timePayrollTransactions = timePayrollTransactions.Where(i => i.AccountInternal != null && i.AccountInternal.Any(ai => ai.Account != null && ai.Account.AccountDimId == accountProvisionAccountDimId)).ToList();
            if (timePayrollTransactions.Count == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10048, "Ingen närvaro hittades i vald period, transaktioner för provision kan ej skapas"));

            #endregion

            #region Perform

            int nrOfEmployeesWithTransactionsCreated = 0;
            int nrOfTransactionsCreated = 0;
            foreach (Employee employee in employees)
            {
                foreach (Employment employment in employee.GetEmployments(startDate, stopDate))
                {
                    #region Prereq

                    startDate = CalendarUtility.GetLatestDate(timePeriod.StartDate, employment.DateFrom);
                    stopDate = CalendarUtility.GetEarliestDate(timePeriod.StopDate, employment.DateTo);

                    PayrollGroup payrollGroup = employee.GetPayrollGroup(startDate, stopDate, GetPayrollGroupsFromCache());

                    List<TimePayrollTransaction> timePayrollTransactionsForEmployee = (from tpt in timePayrollTransactions
                                                                                       where tpt.EmployeeId == employee.EmployeeId &&
                                                                                       tpt.TimeBlockDate.Date >= startDate &&
                                                                                       tpt.TimeBlockDate.Date <= stopDate &&
                                                                                       tpt.Quantity > 0 &&
                                                                                       (tpt.IsWork() || tpt.IsAbsencePermission())
                                                                                       select tpt).ToList();
                    if (!timePayrollTransactionsForEmployee.Any())
                        continue; //No transactions for period, continue to next..

                    //Check if leader
                    if (payrollGroup != null)
                    {
                        if (!payrollGroup.PayrollProductSetting.IsLoaded)
                            payrollGroup.PayrollProductSetting.Load();

                        var setting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsPersonalCategory);
                        if (setting != null && setting.IntData.HasValue && setting.IntData == 9)
                            continue;
                    }

                    Dictionary<int, decimal> accountInternalsPaidQuantity = GetTimePayrollTransactionQuantityPaidByAccountInternal(timePayrollTransactionsForEmployee, accountProvisionAccountDimId, out List<AccountInternal> accountInternals);
                    if (accountInternals.IsNullOrEmpty())
                        continue; //No accounts for period, continue to next..

                    TermGroup_PayrollExportSalaryType salaryType = GetEmployeeSalaryType(employee, startDate, stopDate);
                    if (salaryType == TermGroup_PayrollExportSalaryType.Unknown)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(10049, "Löneform för anställd {0} kunde inte fastställas"), employee.EmployeeNrAndName));

                    Dictionary<DateTime, decimal> employmentRates = GetEmployeeEmploymentRates(employee, startDate, stopDate);
                    if (employmentRates == null || employmentRates.Count == 0)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(10050, "Sysselsättningsgrad för anställd {0} kunde inte fastställas"), employee.EmployeeNrAndName));

                    List<TimeScheduleTemplateBlock> scheduleBlocksForEmployee = new List<TimeScheduleTemplateBlock>();
                    List<TimeScheduleTemplateBlock> scheduleBlockBreaksForEmployee = new List<TimeScheduleTemplateBlock>();
                    if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                    {
                        scheduleBlocksForEmployee = GetScheduleBlocksForEmployeeWithTimeCodeAndAccounting(null, employee.EmployeeId, startDate, stopDate);
                        scheduleBlockBreaksForEmployee.AddRange(scheduleBlocksForEmployee.Where(b => b.IsBreak()));
                    }

                    decimal totalScheduledTime = scheduleBlocksForEmployee.GetWorkMinutes(new List<TimeScheduleTypeDTO>());
                    decimal totalPaidTime = accountInternalsPaidQuantity.Sum(i => i.Value);

                    #endregion

                    int nrOfTransactionsCreatedForEmployee = 0;
                    foreach (AccountInternal accountInternal in accountInternals)
                    {
                        #region Calculate

                        if (!accountInternalsPaidQuantity.ContainsKey(accountInternal.AccountId))
                            continue; //No paid time for account, continue to next...

                        TimePeriodAccountValue timePeriodAccountValue = timePeriodAccountValues.FirstOrDefault(i => i.AccountId == accountInternal.AccountId);
                        if (timePeriodAccountValue == null)
                            continue; //No provision for account, continue to next...

                        List<TimePayrollTransaction> timePayrollTransactionsForEmployeeAndAccount = timePayrollTransactionsForEmployee.Where(i => i.AccountInternal.Any(ai => ai.AccountId == accountInternal.AccountId)).ToList();
                        if (!timePayrollTransactionsForEmployeeAndAccount.Any())
                            continue; //No transactions for period with correct account, continue to next..

                        TimeCodePayrollProduct timeCodePayrollProduct = timeCode.TimeCodePayrollProduct?.FirstOrDefault();
                        PayrollProduct payrollProduct = timeCodePayrollProduct != null ? GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timeCodePayrollProduct.ProductId) : null;
                        if (payrollProduct == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(10051, "Löneart mappad mot tidkod för service provision hittades inte"), employee.EmployeeNrAndName));

                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, stopDate, createIfNotExists: true);
                        decimal paidTime = accountInternalsPaidQuantity[accountInternal.AccountId];

                        decimal provisionQuantity = PayrollManager.CalculateAccountProvisionQuantity(startDate, stopDate, salaryType, paidTime, totalPaidTime, totalScheduledTime, employmentRates);
                        PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(timeBlockDate.Date, employee, employee.GetEmployment(timeBlockDate.Date), payrollProduct, timePeriodAccountValue.Value);
                        decimal amount = formulaResult.Amount * provisionQuantity;
                        if (amount <= 0)
                            continue;

                        #endregion

                        #region TimeCodeTransaction

                        TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransaction(timeCode.TimeCodeId, TimeCodeTransactionType.Time, 1, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_DEFAULT, amount, timeBlockDate: timeBlockDate);
                        if (timeCodeTransaction == null)
                            continue;

                        timeCodeTransaction.IsProvision = true;

                        #endregion

                        #region TimePayrollTransaction

                        TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, timeBlockDate, 1, amount, 0, amount, "", attestStateInitialPayroll.AttestStateId, null, employee.EmployeeId);
                        if (timePayrollTransaction == null)
                            continue;

                        timePayrollTransaction.TimeCodeTransaction = timeCodeTransaction;

                        if (UsePayroll())
                        {
                            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                            timePayrollTransaction.TimePayrollTransactionExtended.FormulaPlain = $"{timePayrollTransaction.TimePayrollTransactionExtended.FormulaPlain} {GetText(10053, "Orginalbelopp")}: {amount}";
                        }

                        //Accounting
                        ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, stopDate, payrollProduct, accountInternals: new List<AccountInternal>() { accountInternal });

                        #endregion

                        nrOfTransactionsCreatedForEmployee++;
                    }

                    if (nrOfTransactionsCreatedForEmployee > 0)
                    {
                        nrOfEmployeesWithTransactionsCreated++;
                        nrOfTransactionsCreated += nrOfTransactionsCreatedForEmployee;
                    }
                }
            }

            if (nrOfTransactionsCreated > 0)
            {
                result = Save();
                if (!result.Success)
                    return result;
            }

            #endregion

            if (result.Success)
            {
                result.IntegerValue = nrOfEmployeesWithTransactionsCreated;
                result.IntegerValue2 = nrOfTransactionsCreated;
            }

            return result;
        }

        #endregion

        #region Save amounts

        private ActionResult SaveTimePayrollTransactionAmounts(TimeEngineTemplateIdentity identity, bool collectTransactions = false)
        {
            if (identity == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeEngineTemplateIdentity");

            return SaveTimePayrollTransactionAmounts(identity.TimeBlockDate, collectTransactions: collectTransactions);
        }

        private ActionResult SaveTimePayrollTransactionAmounts(int employeeId, DateTime date, bool collectTransactions = false)
        {
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, date);
            if (timeBlockDate == null)
                return new ActionResult(true);

            return SaveTimePayrollTransactionAmounts(timeBlockDate, collectTransactions: collectTransactions);
        }

        private ActionResult SaveTimePayrollTransactionAmounts(int employeeId, int timeBlockDateId, bool collectTransactions = false)
        {
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, timeBlockDateId);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            return SaveTimePayrollTransactionAmounts(timeBlockDate, collectTransactions: collectTransactions);
        }

        private ActionResult SaveTimePayrollTransactionAmounts(TimeBlockDate timeBlockDate, TimePayrollTransaction timePayrollTransaction = null, bool collectTransactions = false, TimePeriod timePeriod = null)
        {
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");

            return SaveTimePayrollTransactionAmounts(timeBlockDate.ObjToList(), timePayrollTransaction?.ObjToList(), collectTransactions, timePeriod);
        }

        private ActionResult SaveTimePayrollTransactionAmounts(List<TimeBlockDate> timeBlockDates, List<TimePayrollTransaction> timePayrollTransactions = null, bool collectTransactions = false, TimePeriod timePeriod = null)
        {
            if (DoNotCalculateAmounts())
                return new ActionResult(true);
            if (!UsePayroll())
                return new ActionResult(true);
            if (timeBlockDates.IsNullOrEmpty())
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(timeBlockDates.First().EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            if (timePayrollTransactions == null)
                timePayrollTransactions = GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(employee.EmployeeId, timeBlockDates.OrderBy(i => i.Date).First().Date, timeBlockDates.OrderBy(i => i.Date).Last().Date, timePeriod);

            if (timePeriod != null)
            {
                List<int> timeBlockDateIdsOutsideInterval = timePayrollTransactions.Where(x => x.TimePeriodId.HasValue && x.TimePeriodId.Value == timePeriod.TimePeriodId).Select(x => x.TimeBlockDateId).ToList();
                foreach (var timeBlockDateId in timeBlockDateIdsOutsideInterval)
                {
                    if (timeBlockDates.Any(x => x.TimeBlockDateId == timeBlockDateId))
                        continue;

                    TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timeBlockDateId);
                    if (timeBlockDate != null)
                        timeBlockDates.Add(timeBlockDate);
                }

                timeBlockDates = timeBlockDates.OrderBy(x => x.Date).ToList();
            }

            StartWatch("EvaluatePayrollPriceFormulaCalc", append: true);

            int evaluatePayrollPriceFormulaCounter = 1;
            foreach (TimeBlockDate timeBlockDate in timeBlockDates)
            {
                List<TimePayrollTransaction> timePayrollTransactionsForDate = timePayrollTransactions.Where(i => !i.IsRetroTransaction() && i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                Employment employment = employee.GetEmployment(timeBlockDate.Date);
                if (employment == null && timePayrollTransactionsForDate.Any(x => x.VacationYearEndRowId.HasValue))
                {
                    employment = employee.GetEmployment(timeBlockDates.OrderBy(x => x.Date).First().Date, timeBlockDates.OrderBy(x => x.Date).Last().Date, forward: false);
                    timePayrollTransactionsForDate = timePayrollTransactionsForDate.Where(x => x.VacationYearEndRowId.HasValue).ToList();
                }
                if (employment == null)
                    continue;

                timePayrollTransactionsForDate = timePayrollTransactionsForDate
                                                    .Where(x => x.State == (int)SoeEntityState.Active &&
                                                            !(x.IsAdded && x.IsSpecifiedUnitPrice) &&
                                                            !x.IsFixed &&
                                                            !x.UnionFeeId.HasValue &&
                                                            !x.EmployeeVehicleId.HasValue &&
                                                            !x.IsVacationAdditionOrSalaryPrepaymentInvert() &&
                                                            !x.IsVacationAdditionOrSalaryVariablePrepaymentInvert() &&
                                                            !x.IsSpecifiedUnitPrice &&
                                                            !x.IsVacationCompensationAdvance() &&
                                                            !x.IsRetroTransaction() &&
                                                            !x.IsCompensation_Vat() &&
                                                            !x.PayrollStartValueRowId.HasValue &&
                                                            !x.TimeWorkAccountYearOutcomeId.HasValue)
                                                    .ToList();


                foreach (var productsGrouping in timePayrollTransactionsForDate.GroupBy(i => i.ProductId))
                {
                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(productsGrouping.Key);
                    if (payrollProduct == null)
                        continue;

                    PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(timeBlockDate.Date, employee, employment, payrollProduct);

                    evaluatePayrollPriceFormulaCounter++;

                    foreach (TimePayrollTransaction timePayrollTransaction in productsGrouping)
                    {
                        if (IsEmployeeTimePeriodLockedForChanges(timePayrollTransaction.EmployeeId, timePeriodId: timePayrollTransaction.TimePeriodId, date: timeBlockDate.Date))
                            continue;
                        if (timePayrollTransaction.IsAdded && !timePayrollTransaction.IsSpecifiedUnitPrice)
                        {
                            if (timePayrollTransaction.AddedDateTo.HasValue)
                                SetAddedTimeTransactionAmounts(employee, employment, timePayrollTransaction, timePayrollTransaction.AddedDateTo.Value);
                        }
                        else if (timePayrollTransaction.IsAdditionOrDeduction)
                        {
                            SetUnitPriceAndAmountOnAddictionDeductionTransactions(employee, timePayrollTransaction, formulaResult, null);
                        }
                        else
                        {
                            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                            SetTimePayrollTransactionFormulas(timePayrollTransaction, formulaResult);
                            SetTimePayrollTransactionUnitPriceAndAmounts(timePayrollTransaction, productsGrouping.ToList(), payrollProduct, timeBlockDate, employment, formulaResult);
                            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
                        }
                        timePayrollTransaction.State = (int)SoeEntityState.Active;
                        SetModifiedProperties(timePayrollTransaction);
                    }
                }
            }

            StopWatch("EvaluatePayrollPriceFormulaCalc");

            StartWatch("EvaluatePayrollPriceFormulaSave", append: true);

            ActionResult result = Save();
            if (result.Success && collectTransactions)
                result.Value = timePayrollTransactions;

            StopWatch("EvaluatePayrollPriceFormulaSave");

            return result;
        }

        private ActionResult SetTimePayrollScheduleTransactionAmounts(TimeBlockDate timeBlockDate, SoeTimePayrollScheduleTransactionType type)
        {
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(timeBlockDate.EmployeeId, getHidden: false);
            List<TimePayrollScheduleTransaction> scheduleTransactions = GetTimePayrollScheduleTransactions(timeBlockDate.EmployeeId, timeBlockDate.TimeBlockDateId, type);
            return SetTimePayrollScheduleTransactionAmounts(scheduleTransactions, employee, timeBlockDate);
        }

        private ActionResult SetTimePayrollScheduleTransactionAmounts(List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions, Employee employee, TimeBlockDate timeBlockDate)
        {
            ActionResult result = new ActionResult(true);

            if (DoNotCalculateAmounts())
                return new ActionResult(true);
            if (timePayrollScheduleTransactions.IsNullOrEmpty() || !UsePayroll())
                return result;

            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");

            Employment employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            foreach (var productsGrouping in timePayrollScheduleTransactions.Where(x => !x.IsRetroTransaction()).GroupBy(i => i.ProductId))
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(productsGrouping.Key);
                if (payrollProduct == null)
                    continue;

                if (payrollProduct.IsSupplementCharge() || payrollProduct.IsEmploymentTax())
                    continue;

                PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(timeBlockDate.Date, employee, employment, payrollProduct);
                foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in productsGrouping)
                {
                    #region TimePayrollScheduleTransaction

                    //Formula
                    timePayrollScheduleTransaction.Formula = formulaResult.Formula;
                    timePayrollScheduleTransaction.FormulaPlain = formulaResult.FormulaPlain;
                    timePayrollScheduleTransaction.FormulaExtracted = formulaResult.FormulaExtracted;
                    timePayrollScheduleTransaction.FormulaNames = formulaResult.FormulaNames;
                    timePayrollScheduleTransaction.FormulaOrigin = formulaResult.FormulaOrigin;

                    //Set FK
                    timePayrollScheduleTransaction.PayrollPriceFormulaId = formulaResult.PayrollPriceFormulaId;
                    timePayrollScheduleTransaction.PayrollPriceTypeId = formulaResult.PayrollPriceTypeId;

                    SetTimePayrollScheduleTransactionUnitAndAmounts(timePayrollScheduleTransaction, payrollProduct, timeBlockDate, employment, formulaResult);

                    // Currency
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollScheduleTransaction);

                    #endregion
                }
            }

            return result;
        }

        #endregion

        #region TimeCodeTransaction

        private List<TimeCodeTransaction> AdjustTimeCodeTransactionsByBreakTime(TimeEngineTemplate template, TimeEngineRuleEvaluatorProgress progress)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();
            Employee employee = template?.Employee;
            if (employee == null || timeCodeTransactions.IsNullOrEmpty() || progress == null)
                return timeCodeTransactions;

            List<TimeScheduleTemplateBlock> schedueBreaks = template.Identity.ScheduleBlocks.GetBreaks();
            int totalBreakMinutes = schedueBreaks?.GetBreakMinutes() ?? 0;
            if (totalBreakMinutes <= 0)
                return timeCodeTransactions;

            var (toIncrease, toDecrease) = GetTimeCodesValidForAdjustment(template.Outcome.TimeCodeTransactions);
            if (toIncrease.Any())
            {
                #region Increase

                List<TimeRule> timeRules = progress.GetTimeRules();
                List<TimeBlock> timeBlocks = template.Identity.TimeBlocks.OrderBy(i => i.StartTime).ToList();
                List<TimeBlock> absenceTimeBlocks = timeBlocks.Where(i => i.CalculatedAsAbsence == true).ToList();
                List<TimeScheduleTemplateBlock> scheduledBreaks = template.Identity.ScheduleBlocks.GetBreaks();
                List<TimeScheduleTemplateBlock> scheduledStandby = template.Identity.ScheduleBlocks.GetWork(onlyStandby: true);
                bool isAllStandbyAbsence = !absenceTimeBlocks.IsNullOrEmpty() && scheduledStandby.All(scheduleBlock => absenceTimeBlocks.Any(timeBlock => scheduleBlock.StartTime == timeBlock.StartTime && scheduleBlock.StopTime == timeBlock.StopTime));

                foreach (var timeCodeTransactionsByTimeCode in timeCodeTransactions.GroupBy(i => i.TimeCodeId))
                {
                    TimeCode timeCodeIncrease = toIncrease.FirstOrDefault(i => i.TimeCodeId == timeCodeTransactionsByTimeCode.Key);
                    if (timeCodeIncrease == null)
                        continue;

                    List<TimeCodeTransaction> timeCodeTransactionsForTimeCode = timeCodeTransactionsByTimeCode.ToList();
                    TimeCode timeCodeToAdjust = timeCodeIncrease.AdjustQuantityTimeCodeId.HasValue ? GetTimeCodeFromCache(timeCodeIncrease.AdjustQuantityTimeCodeId.Value) : timeCodeIncrease;
                    if (timeCodeToAdjust == null || !timeCodeTransactionsForTimeCode.DoIncreaseTimeCodeTransactionsAfterTimeCode(timeCodeIncrease))
                        continue;

                    TimeCodeTransaction prototypeTransaction = timeCodeTransactionsForTimeCode.OrderByDescending(i => i.Stop).FirstOrDefault();
                    TimeRule timeRule = timeRules?.FirstOrDefault(i => i.TimeRuleId == prototypeTransaction.TimeRuleId);

                    foreach (TimeScheduleTemplateBlock scheduledBreak in scheduledBreaks)
                    {
                        TimeBlock timeBlockBefore = timeBlocks.LastOrDefault(i => i.StopTime <= scheduledBreak.StartTime);
                        TimeBlock timeBlockAfter = timeBlocks.FirstOrDefault(i => i.StartTime >= scheduledBreak.StopTime);
                        int breakMinutes = (int)scheduledBreak.StopTime.Subtract(scheduledBreak.StartTime).TotalMinutes;
                        bool isBreakSurroundedByAbsence = timeBlockBefore?.CalculatedAsAbsence == true && timeBlockAfter?.CalculatedAsAbsence == true;

                        TimeCodeTransaction timeCodeTransactionDuringBreak = null;
                        if (isAllStandbyAbsence && isBreakSurroundedByAbsence)
                        {
                            var (firstTimeCodeTranscation, firstTimeBlock) = absenceTimeBlocks.GetFirstTimeBlock(timeCodeTransactionsForTimeCode);
                            TimeCode timeCode = firstTimeCodeTranscation != null ? GetTimeCodeFromCache(firstTimeCodeTranscation.TimeCodeId) : null;
                            timeCodeTransactionDuringBreak = ConvertToTimeCodeTransaction(employee, firstTimeBlock, timeCode, timeRule, breakMinutes, scheduledBreak.StartTime, scheduledBreak.StartTime.AddMinutes(breakMinutes), timeBlockDateId: prototypeTransaction.TimeBlockDateId, useTimeBlockTimes: true, isAbsenceDuringStandbyTransaction: true);
                        }
                        else if (!isBreakSurroundedByAbsence)
                        {
                            TimeBlock timeBlock = timeBlocks?.FirstOrDefault(i => i.TimeScheduleTemplateBlockBreakId == scheduledBreak.TimeScheduleTemplateBlockId) ?? prototypeTransaction.TimeBlock;
                            TimeCode timeCode = timeCodeToAdjust;
                            timeCodeTransactionDuringBreak = ConvertToTimeCodeTransaction(employee, timeBlock, timeCode, timeRule, breakMinutes, scheduledBreak.StartTime, scheduledBreak.StartTime.AddMinutes(breakMinutes), timeBlockDateId: prototypeTransaction.TimeBlockDateId, useTimeBlockTimes: true);
                        }
                        if (timeCodeTransactionDuringBreak != null)
                            timeCodeTransactions.Add(timeCodeTransactionDuringBreak);
                    }
                }

                #endregion
            }
            if (toDecrease.Any())
            {
                #region Decrease

                int breakMinutesToRemove = totalBreakMinutes;
                foreach (var timeCodeTransactionsByTimeCode in timeCodeTransactions.GroupBy(i => i.TimeCodeId))
                {
                    TimeCode timeCode = toDecrease.FirstOrDefault(i => i.TimeCodeId == timeCodeTransactionsByTimeCode.Key);
                    if (timeCode == null)
                        continue;

                    foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsByTimeCode.OrderBy(i => i.Start).ThenBy(i => i.Stop).ToList())
                    {
                        if (breakMinutesToRemove <= 0)
                            break;

                        int quantity = Convert.ToInt32(timeCodeTransaction.Quantity);
                        if (quantity < breakMinutesToRemove)
                        {
                            timeCodeTransaction.Quantity = 0;
                            breakMinutesToRemove -= quantity;
                        }
                        else
                        {
                            timeCodeTransaction.Quantity -= breakMinutesToRemove;
                            breakMinutesToRemove = 0;
                        }
                    }
                }

                #endregion
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> AdjustTimeCodeTransactionsForRoundingWholeDay(TimeEngineTemplate template)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();
            Employee employee = template?.Employee;
            if (timeCodeTransactions.IsNullOrEmpty() || employee == null)
                return timeCodeTransactions;

            var timeCodeTransactionsThatCanBeInterrupted = timeCodeTransactions.Where(t => t.TimeCode.RoundingInterruptionTimeCodeId.HasValue).ToList();
            if (timeCodeTransactionsThatCanBeInterrupted.Any())
            {
                foreach (var timeCodeTransactionToInterrupt in timeCodeTransactionsThatCanBeInterrupted.OrderBy(t => t.Start).ThenBy(t => t.Stop))
                {
                    var interruptingTransactions = timeCodeTransactions
                        .Where(t =>
                            t.TimeCodeId == timeCodeTransactionToInterrupt.TimeCode.RoundingInterruptionTimeCodeId.Value &&
                            t.Start > timeCodeTransactionToInterrupt.Start &&
                            t.Start < timeCodeTransactionToInterrupt.Stop)
                        .OrderBy(t => t.Start)
                        .ToList();

                    // If any such transaction exists, set Stop to the earliest Start
                    if (interruptingTransactions.Any())
                    {
                        var firstInterrupt = interruptingTransactions.First();
                        if (firstInterrupt.Start < timeCodeTransactionToInterrupt.Stop)
                        {
                            timeCodeTransactionToInterrupt.Stop = firstInterrupt.Start;
                            timeCodeTransactionToInterrupt.SetQuantity();
                        }
                    }
                }
            }

            var timeCodeTransactionsToRound = timeCodeTransactions.FilterTransactionsToRoundWholeDay();
            if (timeCodeTransactionsToRound.Any())
            {
                foreach (var timeCodeTransactionsToRoundByKey in timeCodeTransactionsToRound.GroupBy(t => t.TimeCode.RoundingGroupKey.NullToEmpty().ToLower()))
                {
                    List<TimeCodeTransaction> timeCodeTransactionsInRoundingGroup = timeCodeTransactionsToRoundByKey.ToList();
                    TimeCodeTransaction timeCodeTransactionToRound = timeCodeTransactionsInRoundingGroup.Last();

                    decimal quantity = timeCodeTransactionToRound.TimeCode.RoundTimeCodeWholeDay((int)timeCodeTransactionsInRoundingGroup.Sum(i => i.Quantity), (int)timeCodeTransactionToRound.Quantity, out TermGroup_TimeCodeRoundingType roundingType);
                    decimal rounding = quantity - timeCodeTransactionToRound.Quantity;
                    if (rounding == 0)
                        continue;

                    if (roundingType == TermGroup_TimeCodeRoundingType.RoundDownWholeDay)
                        timeCodeTransactions.AddRange(DoRoundTimeCodeTransactionWholeDayDown(rounding, timeCodeTransactionToRound, timeCodeTransactionsInRoundingGroup, employee));
                    else if (roundingType == TermGroup_TimeCodeRoundingType.RoundUpWholeDay)
                        DoRoundTimeCodeTransactionWholeDayUp(rounding, timeCodeTransactionToRound);
                }
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> DoRoundTimeCodeTransactionWholeDayDown(decimal rounding, TimeCodeTransaction timeCodeTransactionToRound, List<TimeCodeTransaction> timeCodeTransactionsInRoundingGroup, Employee employee)
        {
            List<TimeCodeTransaction> roundingTimeCodeTransactions = new List<TimeCodeTransaction>();
            if (rounding >= 0 || timeCodeTransactionToRound == null || timeCodeTransactionsInRoundingGroup.IsNullOrEmpty() || employee == null)
                return roundingTimeCodeTransactions;

            RoundLastTransaction();
            while (rounding < 0 && timeCodeTransactionToRound != null)
            {
                timeCodeTransactionToRound = timeCodeTransactionsInRoundingGroup.GetPrevious(timeCodeTransactionToRound);
                if (timeCodeTransactionToRound != null)
                    RoundLastTransaction();
            }

            return roundingTimeCodeTransactions;

            void RoundLastTransaction()
            {
                decimal rest = timeCodeTransactionToRound.Quantity - (-rounding);
                decimal currentRounding = rest >= 0 ? rounding : -timeCodeTransactionToRound.Quantity;
                rounding -= currentRounding;

                DateTime roundingFrom = timeCodeTransactionToRound.Stop.AddMinutes((int)currentRounding);
                DateTime roundingTo = timeCodeTransactionToRound.Stop;
                decimal roundingQuantity = (int)roundingTo.Subtract(roundingFrom).TotalMinutes;

                timeCodeTransactionToRound.Stop = roundingFrom;
                timeCodeTransactionToRound.SetQuantity();

                if (timeCodeTransactionToRound.TimeCode.RoundingTimeCodeId.HasValue)
                {
                    TimeCode roundingTimeCode = GetTimeCodeFromCache(timeCodeTransactionToRound.TimeCode.RoundingTimeCodeId.Value);
                    TimeCodeTransaction roundingTimeCodeTransaction = ConvertToTimeCodeTransaction(employee, timeCodeTransactionToRound.TimeBlock, roundingTimeCode, null, roundingQuantity, roundingFrom, roundingTo, timeBlockDateId: timeCodeTransactionToRound.TimeBlockDateId);
                    if (roundingTimeCodeTransaction != null)
                        roundingTimeCodeTransactions.Add(roundingTimeCodeTransaction);
                }
            }
        }

        private void DoRoundTimeCodeTransactionWholeDayUp(decimal rounding, TimeCodeTransaction timeCodeTransactionToRound)
        {
            if (rounding <= 0 || timeCodeTransactionToRound == null)
                return;

            timeCodeTransactionToRound.Stop = timeCodeTransactionToRound.Stop.AddMinutes((int)rounding);
            timeCodeTransactionToRound.SetQuantity();
        }

        private List<TimeCodeTransaction> AdjustTimeCodeTransactionsForAbsenceQuantity(TimeEngineTemplate template)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();
            if (timeCodeTransactions.IsNullOrEmpty())
                return timeCodeTransactions;

            bool hasAppliedQuantity = false;
            foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions.Where(i => i.Quantity > 0))
            {
                TimeCode timeCode = timeCodeTransaction.TimeCode ?? GetTimeCodeFromCache(timeCodeTransaction.TimeCodeId);
                if (timeCode != null && timeCode.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Quantity && timeCode.IsAbsence())
                {
                    timeCodeTransaction.Quantity = hasAppliedQuantity ? 0 : 1;
                    hasAppliedQuantity = true;
                }
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> AdjustTimeCodeTransactionsForTimeCodeRanking(TimeEngineTemplate template)
        {
            var timeCodeTransactions = template?.Outcome?.TimeCodeTransactions
                .Where(tct => tct.State == (int)SoeEntityState.Active)
                .OrderBy(tct => tct.Start)
                .ThenBy(tct => tct.Stop)
                .ToList() ?? new List<TimeCodeTransaction>();

            if (timeCodeTransactions.IsNullOrEmpty() || !UseTimeCodeRankingFromCache())
                return timeCodeTransactions;

            var timeCodeRankingGroup = GetTimeCodeRankingGroupWithRankingsFromCache(template.Date);
            var timeCodeRankings = timeCodeRankingGroup?.TimeCodeRanking.Where(r => r.State == (int)SoeEntityState.Active).ToList();
            if (timeCodeRankings.IsNullOrEmpty())
                return timeCodeTransactions;

            var leftTimeCodeIds = timeCodeRankings.Select(r => r.LeftTimeCodeId).Distinct();
            if (!timeCodeTransactions.Any(tct => leftTimeCodeIds.Contains(tct.TimeCodeId)))
                return timeCodeTransactions;

            var newTimeCodeTransactions = new List<TimeCodeTransaction>();
            var leftTimeCodes = GetTimeCodesFromCache(leftTimeCodeIds);

            foreach (var leftTimeCode in leftTimeCodes)
            {
                var leftTimeCodeRankings = timeCodeRankings.Where(r => r.LeftTimeCodeId == leftTimeCode.TimeCodeId);
                var leftTimeCodeTransactions = timeCodeTransactions.Where(tct => tct.TimeCodeId == leftTimeCode.TimeCodeId);

                foreach (var leftTimeCodeTransaction in leftTimeCodeTransactions)
                {
                    var rightTimeCodeTransactions = timeCodeTransactions
                        .Where(rightTimeCodeTransaction =>
                            leftTimeCodeTransaction.Guid != rightTimeCodeTransaction.Guid &&
                            leftTimeCodeRankings.Any(r => r.RightTimeCodeId == rightTimeCodeTransaction.TimeCodeId) &&
                            CalendarUtility.IsDatesOverlapping(
                                leftTimeCodeTransaction.Start, 
                                leftTimeCodeTransaction.Stop,
                                rightTimeCodeTransaction.Start,
                                rightTimeCodeTransaction.Stop
                                )                            
                            )
                        .ToList();

                    foreach (var rightTimeCodeTransaction in rightTimeCodeTransactions)
                    {
                        if (!CalendarUtility.GetOverlappingDates(leftTimeCodeTransaction.Start, leftTimeCodeTransaction.Stop, rightTimeCodeTransaction.Start, rightTimeCodeTransaction.Stop, out DateTime newStart, out DateTime newStop))
                            continue;
                                                
                        var timeCodeRanking = leftTimeCodeRankings.FirstOrDefault(r => r.RightTimeCodeId == rightTimeCodeTransaction.TimeCodeId);
                        if (timeCodeRanking == null)
                            continue;

                        var looserTimeCodeTransaction = timeCodeRanking.OperatorType == (int)TermGroup_TimeCodeRankingOperatorType.GreaterThan
                            ? rightTimeCodeTransaction
                            : leftTimeCodeTransaction;

                        //Create winner parts of the looser
                        if (newStart > looserTimeCodeTransaction.Start)
                            newTimeCodeTransactions.Add(CopyTimeCodeTransaction(looserTimeCodeTransaction, looserTimeCodeTransaction.Start, newStart, timeCodeRankingId: timeCodeRanking.TimeCodeRankingId));
                        if (newStop < looserTimeCodeTransaction.Stop)
                            newTimeCodeTransactions.Add(CopyTimeCodeTransaction(looserTimeCodeTransaction, looserTimeCodeTransaction.Stop, looserTimeCodeTransaction.Stop, timeCodeRankingId: timeCodeRanking.TimeCodeRankingId));

                        //Update looser to looser times
                        looserTimeCodeTransaction.SetTimeAndQuantity(newStart, newStop);
                        looserTimeCodeTransaction.SetTurnedByTimeCodeRanking();
                        looserTimeCodeTransaction.SetTimeCodeRankingId(timeCodeRanking.TimeCodeRankingId);

                        //Turn looser
                        TimeCodeTransaction turnedLooserTimeCodeTransaction = CopyTimeCodeTransaction(looserTimeCodeTransaction, newStart, newStop, turn: true, timeCodeRankingId: timeCodeRanking.TimeCodeRankingId);
                        turnedLooserTimeCodeTransaction.SetTurnedByTimeCodeRanking(); 
                        newTimeCodeTransactions.Add(turnedLooserTimeCodeTransaction);
                    }
                }
            }

            timeCodeTransactions.AddRange(newTimeCodeTransactions);

            return timeCodeTransactions.OrderBy(tct => tct.Start).ThenBy(tct => tct.Stop).ToList();
        }

        private List<TimeCodeTransaction> GenerateTransactionForZeroDays(TimeEngineTemplate template, TimeEngineRuleEvaluatorProgress progress, List<TimeBlock> absenceTimeBlocks)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();
            Employee employee = template?.Identity?.Employee;
            if (employee == null || progress == null || absenceTimeBlocks?.Count != 1)
                return timeCodeTransactions;

            TimeBlock absenceTimeBlock = absenceTimeBlocks.First();
            if (absenceTimeBlock.IsZeroBlock() && !absenceTimeBlock.TransactionsResulted && absenceTimeBlock.TimeCode?.Count == 1)
            {
                timeCodeTransactions.Add(ConvertToTimeCodeTransaction(employee, absenceTimeBlock, absenceTimeBlock.TimeCode.First()));
                progress.AllowEmptyTransactions = true;
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> HandleSickDuringIwhOrStandbyTransactions(TimeEngineTemplate template, TimeEngineRuleEvaluatorProgress progress, ref List<TimeBlock> presenceTimeBlocks, ref List<TimeBlock> absenceTimeBlocks)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();

            //Count down to be able to remove items
            for (int ptb = presenceTimeBlocks.Count - 1; ptb >= 0; ptb--)
            {
                TimeBlock tempPresenceTimeBlock = presenceTimeBlocks[ptb];
                if (tempPresenceTimeBlock == null || tempPresenceTimeBlock.State != (int)SoeEntityState.Temporary)
                    continue;
                if (!tempPresenceTimeBlock.IsSickDuringIwhOrStandbyTimeBlock || tempPresenceTimeBlock.IsScheduleAbsenceTimeBlock || tempPresenceTimeBlock.IsSchedulePreliminaryTimeBlock)
                    continue;

                //Find origin absence TimeBlock that temp presence TimeBlock was generated from
                TimeBlock originalAbsenceTimeBlock = FindTempPresenceTimeBlockOriginalAbenceTimeBlock(tempPresenceTimeBlock, absenceTimeBlocks);
                if (originalAbsenceTimeBlock != null)
                {
                    //Count down to be able to remove items
                    for (int tct = timeCodeTransactions.Count - 1; tct >= 0; tct--)
                    {
                        TimeCodeTransaction timeCodeTransaction = timeCodeTransactions[tct];
                        if (timeCodeTransaction == null || timeCodeTransaction.GuidTimeBlock != tempPresenceTimeBlock.GuidId)
                            continue;

                        if (progress.HasAnyPresenceRuleResulted(timeCodeTransaction.TimeCodeId, true))
                        {
                            //Detach and remove none-iwh-transactions generated for temp presence TimeBlocks
                            base.TryDetachEntity(entities, timeCodeTransaction);
                            timeCodeTransactions.RemoveAt(tct);
                        }
                        else
                        {
                            //Redirect iwh-transactions to the original absence TimeBlock
                            timeCodeTransaction.SetTimeBlock(originalAbsenceTimeBlock);                           
                            timeCodeTransaction.SetIsSickDuringIwhTransaction(tempPresenceTimeBlock.IsSickDuringIwhTimeBlock);
                            timeCodeTransaction.SetIsAbsenceDuringStandbyTransaction(tempPresenceTimeBlock.IsSickDuringStandbyTimeBlock);
                        }
                    }
                }

                //Detach and remove temp presence TimeBlock
                base.TryDetachEntity(entities, tempPresenceTimeBlock);
                presenceTimeBlocks.RemoveAt(ptb);
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> HandleScheduleTransactions(TimeEngineTemplate template, ref List<TimeBlock> presenceTimeBlocks, ref List<TimeBlock> absenceTimeBlocks)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();

            if (presenceTimeBlocks.Any(i => i.IsScheduleAbsenceTimeBlock))
            {
                //Count down to be able to remove items
                for (int ptb = presenceTimeBlocks.Count - 1; ptb >= 0; ptb--)
                {
                    TimeBlock tempPresenceTimeBlock = presenceTimeBlocks[ptb];
                    if (tempPresenceTimeBlock == null || tempPresenceTimeBlock.State != (int)SoeEntityState.Temporary)
                        continue;
                    if (!tempPresenceTimeBlock.IsScheduleAbsenceTimeBlock || tempPresenceTimeBlock.IsSchedulePreliminaryTimeBlock || tempPresenceTimeBlock.IsSickDuringIwhOrStandbyTimeBlock)
                        continue;

                    //Count down to be able to remove items
                    for (int tct = timeCodeTransactions.Count - 1; tct >= 0; tct--)
                    {
                        TimeCodeTransaction timeCodeTransaction = timeCodeTransactions[tct];
                        if (timeCodeTransaction == null || timeCodeTransaction.GuidTimeBlock != tempPresenceTimeBlock.GuidId)
                            continue;

                        //Redirect schedule transaction to the original absence TimeBlock
                        TimeBlock originalAbsenceTimeBlock = FindTempPresenceTimeBlockOriginalAbenceTimeBlock(tempPresenceTimeBlock, absenceTimeBlocks);
                        if (originalAbsenceTimeBlock != null)
                        {
                            timeCodeTransaction.SetTimeBlock(originalAbsenceTimeBlock);
                            timeCodeTransaction.SetIsScheduleTransaction(true);
                        }
                    }

                    //Detach and remove temp presence TimeBlock
                    base.TryDetachEntity(entities, tempPresenceTimeBlock);
                    presenceTimeBlocks.RemoveAt(ptb);
                }
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> RemoveEmptyTimeCodeTransactions(TimeEngineTemplate template, TimeEngineRuleEvaluatorProgress progress)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();

            if (progress == null || progress.AllowEmptyTransactions)
                return timeCodeTransactions;

            //Count down to be able to remove items
            for (int tct = timeCodeTransactions.Count - 1; tct >= 0; tct--)
            {
                TimeCodeTransaction timeCodeTransaction = timeCodeTransactions[tct];
                if ((int)(timeCodeTransaction.Stop - timeCodeTransaction.Start).TotalMinutes == 0)
                    timeCodeTransactions.Remove(timeCodeTransaction);
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> SetTimeCodeTransactionRelations(TimeEngineTemplate template)
        {
            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();

            if (template?.TimeBlockDateId > 0)
            {
                foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions)
                {
                    if (!timeCodeTransaction.TimeBlockDateId.HasValue || timeCodeTransaction.TimeBlockDateId.Value == 0)
                        timeCodeTransaction.TimeBlockDateId = template.TimeBlockDateId;
                }
            }

            return timeCodeTransactions;
        }

        private (TimeCodeTransaction timeCodeTransactionForRule, TimeCode timeCodeForRule) CreateTimeCodeRuleTransaction(Employee employee, TimeBlock timeBlock, TimeCode timeCode, TimeRule timeRule, DateTime date, TimeCodeTransaction timeCodeTransaction)
        {
            if (employee == null || timeBlock == null || timeCode == null)
                return (null, null);

            TimeCodeRule timeCodeRule = GetTimeCodeRule(timeCode);
            if (timeCodeRule == null)
                return (null, null);

            var (newTimeCode, newStartTime, newStopTime, newQuantity) = ApplyTimeCodeRules(timeCodeTransaction, timeCode, timeCodeRule, employee, date);
            if (newTimeCode == null)
                return (null, null);

            TimeCodeTransaction timeCodeTransactionForRule = ConvertToTimeCodeTransaction(employee, timeBlock, newTimeCode, timeRule, newQuantity, newStartTime, newStopTime, forceNew: true);
            return (timeCodeTransactionForRule, newTimeCode);
        }

        private List<TimeCodeTransaction> ConvertToTimeCodeTransactions(Employee employee, TimeBlock timeBlock, TimeCode timeCode, TimeRule timeRule, List<TimeChunk> timeChunks, DateTime date)
        {
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            foreach (TimeChunk timeChunk in timeChunks)
            {
                decimal quantity = timeChunk.IntervallMinutes * timeRule.Factor;
                DateTime start = CalendarUtility.DATETIME_DEFAULT.Add(timeChunk.Start);
                DateTime stop = CalendarUtility.DATETIME_DEFAULT.Add(timeChunk.Stop);

                TimeCodeTransaction timeCodeTransaction = ConvertToTimeCodeTransaction(employee, timeBlock, timeCode, timeRule, quantity, start, stop);
                if (timeCodeTransaction == null)
                    continue;

                timeCodeTransactions.Add(timeCodeTransaction);
                timeCodeTransactions.AddRange(CreateTimeCodeRuleTransactions(employee, timeBlock, timeCode, timeRule, date, timeCodeTransaction));
            }

            return timeCodeTransactions;
        }

        private List<TimeCodeTransaction> CreateTimeCodeRuleTransactions(Employee employee, TimeBlock timeBlock, TimeCode timeCode, TimeRule timeRule, DateTime date, TimeCodeTransaction timeCodeTransaction)
        {
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            TimeCodeTransaction currentTimeCodeTransaction = timeCodeTransaction;
            TimeCode currentTimeCode = timeCode;
            while (currentTimeCode != null)
            {
                var (nextTimeCodeTransaction, nextTimeCode) = CreateTimeCodeRuleTransaction(employee, timeBlock, currentTimeCode, timeRule, date, currentTimeCodeTransaction);

                //Prevent cirkular referencing
                if (nextTimeCodeTransaction != null && nextTimeCode?.TimeCodeId != currentTimeCode.TimeCodeId && !timeCodeTransactions.Any(tct => tct.TimeCodeId == nextTimeCodeTransaction.TimeCodeId))
                {
                    timeCodeTransactions.Add(nextTimeCodeTransaction);
                    currentTimeCodeTransaction = nextTimeCodeTransaction;
                    currentTimeCode = nextTimeCode;
                }
                else
                    currentTimeCode = null;
            }

            return timeCodeTransactions;
        }

        private TimeCodeTransaction ConvertToTimeCodeTransaction(Employee employee, TimeBlock timeBlock, TimeCode timeCode)
        {
            return ConvertToTimeCodeTransaction(employee, timeBlock, timeCode, null, Convert.ToDecimal(timeBlock.TotalMinutes), timeBlock.StartTime, timeBlock.StopTime);
        }

        private TimeCodeTransaction ConvertToTimeCodeTransaction(Employee employee, TimeBlock timeBlock, TimeCode timeCode, TimeRule timeRule, decimal quantity, DateTime start, DateTime stop, int? timeBlockDateId = null, bool useTimeBlockTimes = false, bool isAbsenceDuringStandbyTransaction = false, bool forceNew = false)
        {
            if (employee == null || timeBlock == null || timeCode == null)
                return null;

            if (timeCode.FactorBasedOnWorkPercentage)
                quantity = GetQuantityByEmploymentPercentage(employee, timeBlock, quantity);

            TimeCodeTransaction timeCodeTransaction = forceNew ? null : GetTimeCodeTransactionWithExternalTransactionDiscardedState(timeBlock, TimeCodeTransactionType.Time, timeCode.TimeCodeId, timeRule?.TimeRuleId ?? 0, useTimeBlockTimes);
            if (timeCodeTransaction == null)
            {
                timeCodeTransaction = new TimeCodeTransaction
                {
                    Type = (int)TimeCodeTransactionType.Time,

                    //Set FK
                    TimeCodeId = timeCode.TimeCodeId,
                    ProjectTimeBlockId = timeBlock.ProjectTimeBlockId
                };
                entities.TimeCodeTransaction.AddObject(timeCodeTransaction);
                SetCreatedProperties(timeCodeTransaction);
            }
            else
            {
                SetModifiedProperties(timeCodeTransaction);
            }

            timeCodeTransaction.Activate(Guid.NewGuid());
            timeCodeTransaction.SetTimeAndQuantity(start, stop, quantity);
            timeCodeTransaction.SetIsAbsenceDuringStandbyTransaction(isAbsenceDuringStandbyTransaction);
            timeCodeTransaction.SetTimeBlock(timeBlock, timeBlockDateId);
            timeCodeTransaction.SetTimeRule(timeRule);
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        private TimeCodeTransaction CreateTimeCodeTransaction(int timeCodeId, TimeCodeTransactionType transactionType, decimal quantity, DateTime start, DateTime stop, decimal? amount = null, bool isProvision = false, int? timeBlockId = null, TimeBlockDate timeBlockDate = null, int? timeBlockDateId = null, int? timeRuleId = null, int? projectTimeBlockId = null)
        {
            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
            {
                Type = (int)transactionType,
                Quantity = quantity,
                InvoiceQuantity = null,
                Start = start,
                Stop = stop,
                Vat = null,
                Amount = amount,
                ExternalComment = null,
                DoNotChargeProject = null,
                ReversedDate = null,
                IsReversed = false,
                IsAdditionOrDeduction = false,
                IsProvision = isProvision,

                //Set FK
                TimeBlockId = timeBlockId,
                TimeCodeId = timeCodeId,
                TimeRuleId = timeRuleId,
                CustomerInvoiceRowId = null,
                ProjectId = null,
                ProjectInvoiceDayId = null,
                SupplierInvoiceId = null,
                TimeSheetWeekId = null,
                ProjectTimeBlockId = projectTimeBlockId
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            if (timeBlockDate != null)
                timeCodeTransaction.TimeBlockDate = timeBlockDate;
            else if (timeBlockDateId.HasValue)
                timeCodeTransaction.TimeBlockDateId = timeBlockDateId.Value;

            // Currency
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        private TimeCodeTransaction CopyTimeCodeTransaction(TimeCodeTransaction prototype, DateTime start, DateTime stop, decimal? quantity = null, bool turn = false, int? timeCodeRankingId = null)
        {
            if (!quantity.HasValue)
                quantity = (int)stop.Subtract(start).TotalMinutes;
            if (turn)
                quantity = decimal.Negate(quantity.Value);

            var timeCodeTransaction = new TimeCodeTransaction()
            {
                Type = prototype.Type,
                Quantity = quantity.Value,
                InvoiceQuantity = prototype.InvoiceQuantity,
                Start = start,
                Stop = stop,
                ExternalComment = null,
                Comment = null,
                Vat = prototype.UnitPrice,
                UnitPrice = prototype.UnitPrice,
                Amount = prototype.Amount,
                DoNotChargeProject = prototype.DoNotChargeProject,
                ReversedDate = prototype.ReversedDate,
                IsReversed = prototype.IsReversed,
                IsAdditionOrDeduction = prototype.IsAdditionOrDeduction,
                IsProvision = prototype.IsProvision,
                IsEarnedHoliday = prototype.IsEarnedHoliday,
                Accounting = prototype.Accounting,

                //Set FK
                TimeBlockId = prototype.TimeBlockId,
                TimeCodeId = prototype.TimeCodeId,
                TimeRuleId = prototype.TimeRuleId,
                ProjectTimeBlockId = prototype.ProjectTimeBlockId,
                TimeBlockDateId = prototype.TimeBlockDateId,
                CustomerInvoiceRowId = prototype.CustomerInvoiceId,
                ProjectId = prototype.ProjectId,
                ProjectInvoiceDayId = prototype.ProjectInvoiceDayId,
                SupplierInvoiceId = prototype.SupplierInvoiceId,
                TimeSheetWeekId = prototype.TimeSheetWeekId,
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            if (prototype.TimeBlockDate != null)
                timeCodeTransaction.TimeBlockDate = prototype.TimeBlockDate;
            if (prototype.TimeBlock != null)
                timeCodeTransaction.TimeBlock = prototype.TimeBlock;
            if (timeCodeRankingId.HasValue)
                timeCodeTransaction.SetTimeCodeRankingId(timeCodeRankingId.Value);
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            //Not persistent values
            timeCodeTransaction.SetIdentifier(Guid.NewGuid());
            timeCodeTransaction.SetTimeBlockIdentifier(prototype.GuidTimeBlock);
            timeCodeTransaction.SetIsScheduleTransaction(prototype.IsScheduleTransaction);
            timeCodeTransaction.SetIsSickDuringIwhTransaction(prototype.IsSickDuringIwhTransaction);
            timeCodeTransaction.SetIsAbsenceDuringStandbyTransaction(prototype.IsAbsenceDuringStandbyTransaction);

            return timeCodeTransaction;
        }

        #endregion

        #region TimePayrollTransaction

        private ActionResult CreateTimePayrollTransactions(Employee employee, List<TimeEnginePayrollProduct> products)
        {
            if (products.IsNullOrEmpty())
                return new ActionResult(true);

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult(8517, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            foreach (TimeEnginePayrollProduct product in products)
            {
                PayrollProduct payrollProduct = GetPayrollProductFromCache(product.PayrollProductId);
                if (payrollProduct == null)
                    return new ActionResult(8517, GetText(91923, "Löneart hittades inte"));

                if (!product.TimePeriodId.HasValue && !product.PlanningPeriodCalculationId.HasValue)
                    return new ActionResult(8517, GetText(8693, "Period hittades inte"));

                TimePeriod timePeriod = null;
                if (product.TimePeriodId.HasValue)
                {
                    timePeriod = GetTimePeriodFromCache(product.TimePeriodId.Value);
                    if (timePeriod == null)
                        return new ActionResult(8517, GetText(8693, "Period hittades inte"));
                }
                TimePeriod planningTimePeriod = null;
                if (product.PlanningPeriodCalculationId.HasValue)
                {
                    planningTimePeriod = GetTimePeriodFromCache(product.PlanningPeriodCalculationId.Value);
                    if (planningTimePeriod == null)
                        return new ActionResult(8517, GetText(8693, "Period hittades inte"));
                }

                DateTime? startDate = timePeriod?.StartDate ?? planningTimePeriod?.StartDate;
                DateTime? stopDate =  timePeriod?.StopDate ?? planningTimePeriod?.StopDate;
                if (!startDate.HasValue)
                    return new ActionResult(8517, GetText(8693, "Period hittades inte"));

                if (!employee.HasEmployment(stopDate.Value))
                {
                    stopDate = employee.GetLatestEmploymentDate(startDate.Value, stopDate.Value);
                    if (!stopDate.HasValue)
                        return new ActionResult(8517, GetText(10084, "Anställning hittades inte"));
                }                    

                TimeBlockDate timeBlockDate = GetTimeBlockDate(employee.EmployeeId, stopDate.Value, createIfNotExists: true);

                CreateTimePayrollTransaction(employee, timeBlockDate, payrollProduct, attestStateInitialPayroll.AttestStateId, product.Quantity, timePeriodId: product.TimePeriodId, planningPeriodCalculationId: product.PlanningPeriodCalculationId);
            }

            return Save();
        }

        private TimePayrollTransaction CreateTimePayrollTransaction(
            Employee employee, 
            TimeBlockDate timeBlockDate,
            PayrollProduct payrollProduct, 
            int attestStateId,
            decimal quantity, 
            decimal amount = 0,
            decimal? unitPrice = null,
            int? timePeriodId = null,
            int? planningPeriodCalculationId = null,
            int? employeeChildId = null,
            bool doCreateExtended = false,
            bool doForceExtended = false,
            TimeCodeTransaction timeCodeTransaction = null
            )
        {
            if (employee == null || timeBlockDate == null || payrollProduct == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = quantity,
                UnitPrice = unitPrice,
                Amount = amount,

                //Set FK
                ActorCompanyId = actorCompanyId,
                AttestStateId = attestStateId,
                TimePeriodId = timePeriodId,
                PlanningPeriodCalculationId = planningPeriodCalculationId,
                EmployeeChildId = employeeChildId,

                //Set references
                Employee = employee,
                TimeBlockDate = timeBlockDate,
                PayrollProduct = payrollProduct,
                TimeCodeTransaction = timeCodeTransaction,
            };
            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            if (doCreateExtended || doForceExtended)
                CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId, force: doForceExtended);
            SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
            if (amount > 0)
                SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
            ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, timeBlockDate.Date, payrollProduct);

            return timePayrollTransaction;
        }

        private void CreateTimePayrollTransactionExtended(TimePayrollTransaction timePayrollTransaction, int employeeId, int actorCompanyId, bool force = false)
        {
            if (!force && !UsePayroll())
                return;

            if (timePayrollTransaction.IsExtended)
            {
                if (!timePayrollTransaction.IsAdded() && !timePayrollTransaction.TimePayrollTransactionExtendedReference.IsLoaded)
                    timePayrollTransaction.TimePayrollTransactionExtendedReference.Load();
            }
            else
            {
                timePayrollTransaction.TimePayrollTransactionExtended = new TimePayrollTransactionExtended()
                {
                    EmployeeId = employeeId,
                    ActorCompanyId = actorCompanyId,
                };
                timePayrollTransaction.IsExtended = true;
            }
        }

        private void RevertTimePayrollTransactionExtended(TimePayrollTransaction timePayrollTransaction)
        {
            if (timePayrollTransaction.IsExtended)
            {
                timePayrollTransaction.TimePayrollTransactionExtended = null;
                timePayrollTransaction.IsExtended = false;
            }
        }

        private void SetTimePayrollTransactionsIsSpecifiedUnitPrice(List<TimePayrollTransaction> timePayrollTransactions, bool isSpecifiedUnitPrice)
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return;

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {
                timePayrollTransaction.IsSpecifiedUnitPrice = isSpecifiedUnitPrice;
            }
        }

        private void SetTimePayrollTransactionsTimePeriodId(List<TimePayrollTransaction> timePayrollTransactions, int? timePeriodId)
        {
            if (timePayrollTransactions.IsNullOrEmpty() || !timePeriodId.HasValue || timePeriodId.Value <= 0)
                return;

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {
                timePayrollTransaction.TimePeriodId = timePeriodId;
            }
        }

        #endregion

        #region ITransactionProc

        private List<GetTimePayrollTransactionsForEmployee_Result> LoadPayrollTransactions(int employeeId, DateTime startDate, DateTime stopDate, List<GetTimePayrollTransactionsForEmployee_Result> existing)
        {
            List<GetTimePayrollTransactionsForEmployee_Result> transactions = null;
            if (existing.Any())
            {
                transactions = LoadPayrollTransactions(employeeId, startDate, existing.Min(t => t.Date).Date.AddDays(-1)).Concat(
                               LoadPayrollTransactions(employeeId, existing.Max(t => t.Date).Date.AddDays(1), stopDate)).ToList();
            }
            else
            {
                transactions = LoadPayrollTransactions(employeeId, startDate, stopDate);
            }
            return existing.Concat(transactions).OrderBy(t => t.Date).ToList();
        }

        private List<GetTimePayrollTransactionsForEmployee_Result> LoadPayrollTransactions(int employeeId, DateTime startDate, DateTime stopDate)
        {
            if (startDate > stopDate)
                return new List<GetTimePayrollTransactionsForEmployee_Result>();

            return TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, employeeId, startDate, stopDate);
        }

        private (List<GetTimePayrollTransactionsForEmployee_Result> Payroll, List<GetTimePayrollScheduleTransactionsForEmployee_Result> Schedule) LoadTransactions(int employeeId, List<int> timePeriodIds)
        {
            var payrollTransactions = new List<GetTimePayrollTransactionsForEmployee_Result>();
            var scheduleTransactions = new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();
            if (!timePeriodIds.IsNullOrEmpty())
            {
                foreach (int timePeriodId in timePeriodIds)
                {
                    TimePeriod timePeriod = GetTimePeriodFromCache(timePeriodId);
                    if (timePeriod == null)
                        continue;

                    var (startDate, stopDate) = timePeriod.GetDates();
                    payrollTransactions.AddRange(TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, employeeId, startDate, stopDate, timePeriodId));
                    if (!timePeriod.ExtraPeriod)
                        scheduleTransactions.AddRange(TimeTransactionManager.GetTimePayrollScheduleTransactionsForEmployee(entities, (int)SoeTimePayrollScheduleTransactionType.Absence, timePeriod.StartDate, timePeriod.StopDate, timePeriodId, employeeId));
                }
            }
            return (payrollTransactions, scheduleTransactions);
        }

        #endregion
    }
}
