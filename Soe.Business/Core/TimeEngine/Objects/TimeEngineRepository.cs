using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Collect flags

        private bool? localDoNotCollectDaysForRecalculation = null;
        private int? localDoNotCollectDaysForRecalculationLevel2 = null;
        private int? localDoNotCollectDaysForRecalculationLevel3 = null;
        private List<int> localDoNotCollectDaysForRecalculationLevel3Reversed = null;
        private bool globalDoNotCollectDaysForRecalculation = false;
        private bool DoCollectDaysForRecalculation(bool onlyGlobal = false)
        {
            return !this.DoNotCollectDaysForRecalculation(onlyGlobal);
        }
        private bool DoNotCollectDaysForRecalculation(bool onlyGlobal = false)
        {
            if (!onlyGlobal && this.localDoNotCollectDaysForRecalculation.HasValue)
                return this.localDoNotCollectDaysForRecalculation.Value;
            else
                return this.globalDoNotCollectDaysForRecalculation;
        }
        private void SetDoNotCollectDaysForRecalculation(bool value)
        {
            this.globalDoNotCollectDaysForRecalculation = value;
        }
        private void SetDoNotCollectDaysForRecalculationLocally(bool? value, PayrollProduct payrollProduct = null, List<int> reversedSysPayrollTypeLevel3 = null)
        {
            this.localDoNotCollectDaysForRecalculation = value;
            if (value.HasValue)
            {
                if (payrollProduct != null)
                {
                    this.localDoNotCollectDaysForRecalculationLevel2 = payrollProduct.SysPayrollTypeLevel2;
                    this.localDoNotCollectDaysForRecalculationLevel3 = payrollProduct.SysPayrollTypeLevel3;
                }
                if (reversedSysPayrollTypeLevel3 != null)
                {
                    if (this.localDoNotCollectDaysForRecalculationLevel3Reversed == null)
                        this.localDoNotCollectDaysForRecalculationLevel3Reversed = new List<int>();
                    this.localDoNotCollectDaysForRecalculationLevel3Reversed.AddRange(reversedSysPayrollTypeLevel3);
                }
            }
        }

        private bool DoCollectDayAsAttested(TimeBlockDate timeBlockDate, AttestTransitionDTO attestTransition)
        {
            if (timeBlockDate == null || attestTransition == null || !attestTransition.NotifyChangeOfAttestState)
                return false;
            //Support logged in
            if (parameterObject != null && parameterObject.SupportUserId.HasValue)
                return false;
            return true;
        }
        private bool DoCollectDayAsNotifyChangeOfDeviations(TimeBlockDate timeBlockDate, EmployeeGroup employeeGroup)
        {
            if (timeBlockDate == null || employeeGroup == null || !employeeGroup.NotifyChangeOfDeviations)
                return false;
            //Support logged in
            if (parameterObject != null && parameterObject.SupportUserId.HasValue)
                return false;
            //Stamping not supported
            if (!employeeGroup.AutogenTimeblocks)
                return false;
            //Day is recalculated after apply/restore
            if (DoNotCollectDaysForRecalculation())
                return false;

            return true;
        }

        private bool DoCollectDaysPayrollWarning(TimeBlockDate timeBlockDate)
        {
            if (timeBlockDate == null)
                return false;
            
            //Day is recalculated after apply/restore - not completly sure if this is correct
            if (DoNotCollectDaysForRecalculation())
                return false;

            return true;
        }

        #endregion

        #region Days generating

        private List<DateTime> initiatedCalculationDays = null;
        public void InitiateCalculationDates(List<DateTime> dates)
        {
            if (!dates.IsNullOrEmpty())
                dates.ForEach(date => InitiateCalculationDate(date));
        }
        public void InitiateCalculationDate(DateTime date)
        {
            if (this.initiatedCalculationDays == null)
                this.initiatedCalculationDays = new List<DateTime> { date };
            else
                this.initiatedCalculationDays.Add(date);
        }
        public List<DateTime> GetInitiatedCalculationDates()
        {
            return this.initiatedCalculationDays ?? new List<DateTime>();
        }
        public bool HasDayInitiatatedCalculation(DateTime date)
        {
            return this.initiatedCalculationDays != null && this.initiatedCalculationDays.Contains(date);
        }
        public bool HasInitiatedDaysAfter(DateTime date, DateTime stopDate)
        {
            return this.initiatedCalculationDays != null && this.initiatedCalculationDays.Any(d => d > date && d <= stopDate);
        }

        private readonly bool useInitiatedAbsenceDays = true;
        private List<TimeEngineAbsenceDay> initiatedAbsenceDays = null;
        private void InitAbsenceDay(int employeeId, DateTime date, decimal? ratio = null, int? timeDeviationCauseId = null)
        {
            InitAbsenceDays(employeeId, date.ObjToList(), ratio, timeDeviationCauseId);
        }
        private void InitAbsenceDays(int employeeId, List<DateTime> dates, decimal? ratio = null, int? timeDeviationCauseId = null)
        {
            InitAbsenceDays(employeeId, dates, SoeTimeBlockDateDetailType.Absence, ratio, timeDeviationCauseId);
        }
        private void InitAbsenceDays(int employeeId, List<DateTime> dates, SoeTimeBlockDateDetailType type, decimal? ratio = null, int? timeDeviationCauseId = null)
        {
            foreach (DateTime date in dates.Distinct().OrderBy(date => date))
            {
                AddAbsenceDay(employeeId, date, type, ratio, timeDeviationCauseId);
            }
        }
        private void AddAbsenceDay(int employeeId, DateTime date, SoeTimeBlockDateDetailType type, decimal? ratio = null, int? timeDeviationCauseId = null)
        {
            TimeEngineAbsenceDay absenceDay = new TimeEngineAbsenceDay(employeeId, date, type, ratio, timeDeviationCauseId);
            if (!this.initiatedAbsenceDays.Exists(absenceDay))
            {
                if (this.initiatedAbsenceDays == null)
                    this.initiatedAbsenceDays = absenceDay.ObjToList();
                else
                    this.initiatedAbsenceDays.Add(absenceDay);
            }                
        }
        private void UpdateAbsenceDay(int employeeId, DateTime date, int sysPayrollTypeLevel3)
        {
            if (!this.useInitiatedAbsenceDays)
                return;

            TimeEngineAbsenceDay timeAbsenceDayForDate = GetInitiatedAbsenceDays(employeeId, date)?.FirstOrDefault(d => d.Date == date && !d.SysPayrollTypeLevel3.HasValue);
            if (timeAbsenceDayForDate != null)
            {
                foreach (TimeEngineAbsenceDay timeAbsenceDay in GetAllInitiatedAbsenceDays(employeeId).Where(d => !d.SysPayrollTypeLevel3.HasValue && d.TimeDeviationCauseId == timeAbsenceDayForDate.TimeDeviationCauseId))
                {
                    if (timeAbsenceDay.Date != date && timeAbsenceDay.Type == SoeTimeBlockDateDetailType.Read && !timeAbsenceDay.TimeDeviationCauseId.HasValue)
                        continue; //Dont assume that sysPayrollTypeLevel3 should be same on other days when recalculating (Type: Read)
                    timeAbsenceDay.Update(sysPayrollTypeLevel3, timeAbsenceDay.TimeDeviationCauseId);
                }
            }
        }
        private void UpdateAbsenceDay(int employeeId, DateTime date, TimeBlock timeBlock, PayrollProduct payrollProduct)
        {
            if (!this.useInitiatedAbsenceDays || timeBlock == null || payrollProduct?.SysPayrollTypeLevel3 == null || !payrollProduct.IsAbsence())
                return;

            List<TimeEngineAbsenceDay> timeAbsenceDays = GetInitiatedAbsenceDays(employeeId, date);
            if (timeAbsenceDays.IsNullOrEmpty())
                return;

            TimeEngineAbsenceDay timeAbsenceDay = timeAbsenceDays.FirstOrDefault(d => d.Date == date && (!d.SysPayrollTypeLevel3.HasValue || d.SysPayrollTypeLevel3.Value != payrollProduct.SysPayrollTypeLevel3.Value));
            if (timeAbsenceDay != null)
            {
                timeAbsenceDay.Update(payrollProduct.SysPayrollTypeLevel3.Value, timeBlock.TimeDeviationCauseStartId);
            }
            else
            {
                TimeEngineAbsenceDay prototype = GetInitiatedAbsenceDay(employeeId, date, timeBlock);
                AddAbsenceDay(employeeId, date, SoeTimeBlockDateDetailType.Absence, prototype?.Ratio, prototype?.TimeDeviationCauseId);
            }
        }
        private void SetInitiatedAbsenceExisting(int employeeId, DateTime date, List<int> sysPayrollTypeLevel3sExisting)
        {
            if (!this.useInitiatedAbsenceDays)
                return;

            List<TimeEngineAbsenceDay> timeAbsenceDays = GetInitiatedAbsenceDays(employeeId, date, SoeTimeBlockDateDetailType.Absence);
            if (timeAbsenceDays.IsNullOrEmpty())
                return;

            foreach (TimeEngineAbsenceDay timeAbsenceDay in timeAbsenceDays)
            {
                timeAbsenceDay.SetExisting(sysPayrollTypeLevel3sExisting);
            }
        }
        private bool HadSameAbsenceBefore(int employeeId, DateTime date, int? sysPayrollTypeLevel3)
        {
            List<int> sysPayrollTypeLevel3sExisting = initiatedAbsenceDays?.FirstOrDefault(a => a.EmployeeId == employeeId && a.Date == date)?.SysPayrollTypeLevel3sExisting;
            return sysPayrollTypeLevel3sExisting?.Any(level3 => level3 == sysPayrollTypeLevel3) ?? false;
        }

        private List<DateTime> GetCurrentGeneratingAbsenceDates(int employeeId, int level3)
        {
            return this.initiatedAbsenceDays?
                .Where(a => a.EmployeeId == employeeId && a.SysPayrollTypeLevel3 == level3)
                .Select(a => a.Date)
                .Distinct()
                .ToList() ?? new List<DateTime>();
        }
        private List<TimeEngineAbsenceDay> GetAllInitiatedAbsenceDays(int employeeId)
        {
            return this.initiatedAbsenceDays?
                .Where(a => a.EmployeeId == employeeId)
                .ToList();
        }
        private List<TimeEngineAbsenceDay> GetInitiatedAbsenceDays(int employeeId, DateTime date, SoeTimeBlockDateDetailType? type = null, int? sysPayrollTypeLevel3 = null)
        {
            return this.initiatedAbsenceDays?
                .Where(a =>
                    a.EmployeeId == employeeId &&
                    a.Date == date &&
                    (!sysPayrollTypeLevel3.HasValue || !a.SysPayrollTypeLevel3.HasValue || a.SysPayrollTypeLevel3.Value == sysPayrollTypeLevel3.Value) &&
                    (!type.HasValue || a.Type == type.Value))
                .ToList();
        }
        private TimeEngineAbsenceDay GetInitiatedAbsenceDay(int employeeId, DateTime date, int timeDeviationCauseId)
        {
            return this.initiatedAbsenceDays?
                .FirstOrDefault(a =>
                    a.EmployeeId == employeeId &&
                    a.Date == date &&
                    a.TimeDeviationCauseId == timeDeviationCauseId);
        }
        private TimeEngineAbsenceDay GetInitiatedAbsenceDay(int employeeId, DateTime date, TimeBlock timeBlock)
        {
            TimeEngineAbsenceDay absenceDay = null;
            if (timeBlock?.TimeDeviationCauseStartId != null)
                absenceDay = GetInitiatedAbsenceDay(employeeId, date, timeBlock.TimeDeviationCauseStartId.Value);
            if (absenceDay == null && timeBlock?.TimeDeviationCauseStopId != null)
                absenceDay = GetInitiatedAbsenceDay(employeeId, date, timeBlock.TimeDeviationCauseStopId.Value);
            return absenceDay;
        }
        private decimal? GetInitiatedAbsenceDaysRatio(int employeeId, DateTime date, TimeBlock timeBlock)
        {
            decimal? ratio = null;
            if (timeBlock?.TimeDeviationCauseStartId != null)
                ratio = GetInitiatedAbsenceDay(employeeId, date, timeBlock.TimeDeviationCauseStartId.Value)?.Ratio;
            if (!ratio.HasValue && timeBlock?.TimeDeviationCauseStopId != null)
                ratio = GetInitiatedAbsenceDay(employeeId, date, timeBlock.TimeDeviationCauseStopId.Value)?.Ratio;
            return ratio;
        }
        private bool HasInitiatedAbsenceDays(int employeeId, DateTime date, int? sysPayrollTypeLevel3 = null)
        {
            return !GetInitiatedAbsenceDays(employeeId, date, SoeTimeBlockDateDetailType.Absence, sysPayrollTypeLevel3).IsNullOrEmpty();
        }
        private bool TryAdjustDaysBackAfterInitiatedAbsence(int employeeId, DateTime date, ref int checkDaysBack)
        {
            if (checkDaysBack > 0)
            {
                DateTime currentDate = date.AddDays(-checkDaysBack);
                while (currentDate < date && checkDaysBack > 0)
                {
                    if (HasInitiatedAbsenceDays(employeeId, currentDate))
                        checkDaysBack--;
                    currentDate = currentDate.AddDays(1);
                }
            }
            return checkDaysBack > 0;
        }

        #endregion

        #region TimeEngineTemplate

        private Func<EntityObject, bool> _canLoadReferencesDelegate;
        private Func<EntityObject, bool> CanLoadReferencesDelegate
        {
            get
            {
                if (_canLoadReferencesDelegate == null)
                    _canLoadReferencesDelegate = entity => base.CanEntityLoadReferences(entities, entity);
                return _canLoadReferencesDelegate;
            }
        }

        private TimeEngineTemplate CreateTemplate(TimeEngineTemplateIdentity identity)
        {
            return new TimeEngineTemplate(identity);
        }

        private List<TimeBlock> CreateTimeBlocksUsingTemplateRepository(TimeEngineTemplateIdentity identity)
        {
            if (identity == null)
                return null;

            TimeEngineTemplate template = templateRepository.GetTemplate(identity, SoeTimeEngineTemplateType.TimeBlocksFromTemplate, UsePayrollOrAbsenceRules(), CanLoadReferencesDelegate);
            if (template != null)
                return CopyTimeBlocks(template.Outcome.TimeBlocks, identity.ScheduleBlocks, identity.TimeBlockDate);

            template = CreateTemplate(identity);
            template.Outcome.TimeBlocks = CreateTimeBlocksFromTemplate(identity.Employee, identity.ScheduleBlocks, identity.TimeBlockDate, identity.StandardTimeDeviationCauseId); //Create TimeBlocks and make sure that created TimeBlock's that are breaks have BreakNumber set, otherwise it's invalid as a template
            templateRepository.AddTemplate(template, CanLoadReferencesDelegate);
            return template.Outcome.TimeBlocks;
        }

        private ActionResult SaveTransactionsUsingTemplateRepository(TimeEngineTemplateIdentity identity, bool? discardBreakEvaluation = null, List<TimeCodeTransaction> additionalTimeCodeTransactions = null)
        {
            if (identity == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeEngineTemplateIdentity");

            if (identity.TimeBlockDate != null)
            {
                EnsureScheduleForTimeEngineIdentity(identity);
                SetTimeBlockTypes(identity.TimeBlockDate.EmployeeId, identity.TimeBlockDate.Date, identity.TimeBlocks, identity.ScheduleBlocks, setScheduleTypeOnConnectedTimeBlocksOutsideSchedule: true);
            }

            TimeEngineTemplate template = additionalTimeCodeTransactions != null ? templateRepository.GetTemplate(identity, SoeTimeEngineTemplateType.TransactionsFromTimeBlocks, UsePayrollOrAbsenceRules(), CanLoadReferencesDelegate) : null;
            if (template != null)
                return CopyTransactionsUsingTemplateRepository(identity, template, save: true);

            template = CreateTemplate(identity);

            ActionResult result = SaveTransactionsUsingTemplateRepository(template, discardBreakEvaluation, additionalTimeCodeTransactions);
            if (result.Success)
                templateRepository.AddTemplate(template, CanLoadReferencesDelegate);

            return result;
        }

        private ActionResult SaveTransactionsUsingTemplateRepository(TimeEngineTemplate template, bool? discardBreakEvaluation = null, List<TimeCodeTransaction> additionalTimeCodeTransactions = null)
        {
            if (!template.HasValidIdentity(requireEmployeeGroup: true))
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeEngineTemplateIdentity");
            if (template.Identity.TimeBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlocks");

            SetTimeBlockTimeDevationCauses(template.Identity.TimeBlocks, template.Identity.StandardTimeDeviationCauseId);

            ActionResult result = GenerateInternalTransactions(ref template, discardBreakEvaluation);
            if (!result.Success)
                return result;

            if (!additionalTimeCodeTransactions.IsNullOrEmpty())
                template.Outcome.TimeCodeTransactions.AddRange(additionalTimeCodeTransactions);

            List<TimeBlock> timeBlocks = FilterTimeBlocksWithUnsavedLink(template);
            templateRepository.UpdateOutcome(template);

            result = SaveExternalTransactions(ref template);
            if (!result.Success)
                return result;

            result = SavePayrollImportEmployeeTransactionLinks(template.Date, template.EmployeeId, timeBlocks);
            if (!result.Success)
                return result;

            ApplyPlausibilityCheck(template);

            return result;
        }

        private ActionResult CopyTransactionsUsingTemplateRepository(TimeEngineTemplateIdentity identity, TimeEngineTemplate template, bool save)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            if (template == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeEngineTemplate");
            if (identity?.Employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (identity.TimeBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlocks");

            #endregion

            #region Prereq

            Employee employee = GetEmployeeFromCache(identity.Employee.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            Employment employment = employee.GetEmployment(identity.TimeBlockDate.Date);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            #endregion

            #region Copy internal transactions and related external transactions

            foreach (TimeBlock prototypeTimeBlock in template.Identity.TimeBlocks.Where(t => t.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime))
            {
                TimeBlock cloneTimeBlock = identity.TimeBlocks.FirstOrDefault(tb => tb.StartTime == prototypeTimeBlock.StartTime && tb.StopTime == prototypeTimeBlock.StopTime && tb.IsBreak == prototypeTimeBlock.IsBreak);
                if (cloneTimeBlock == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlock");

                List<TimeCodeTransaction> prototypeTimeCodeTransactions = template.Outcome.TimeCodeTransactions.Where(tct => tct.TimeBlockId == prototypeTimeBlock.TimeBlockId && tct.State == (int)SoeEntityState.Active).ToList();
                foreach (TimeCodeTransaction prototypeTimeCodeTransaction in prototypeTimeCodeTransactions)
                {
                    //int = prototype timePayrollTransactionId, TimePayrollTransaction = cloned transaction
                    List<Tuple<int, TimePayrollTransaction>> payrollTransactionMapping = new List<Tuple<int, TimePayrollTransaction>>();

                    #region TimeCodeTransaction

                    TimeCodeTransaction cloneTimeCodeTransaction = new TimeCodeTransaction()
                    {
                        //Set FK (from template)
                        TimeRuleId = prototypeTimeCodeTransaction.TimeRuleId,
                        TimeCodeId = prototypeTimeCodeTransaction.TimeCodeId,

                        //Set FK (from identity)
                        TimeBlockId = cloneTimeBlock.TimeBlockId,
                        TimeBlockDateId = cloneTimeBlock.TimeBlockDateId,
                        ProjectTimeBlockId = cloneTimeBlock.ProjectTimeBlockId,

                        Start = prototypeTimeCodeTransaction.Start,
                        Stop = prototypeTimeCodeTransaction.Stop,
                        Quantity = prototypeTimeCodeTransaction.Quantity,
                        Type = prototypeTimeCodeTransaction.Type,
                    };
                    SetCreatedProperties(cloneTimeCodeTransaction);
                    entities.TimeCodeTransaction.AddObject(cloneTimeCodeTransaction);

                    // Currency
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, cloneTimeCodeTransaction);

                    #endregion

                    #region TimeInvoiceTransaction

                    foreach (TimeInvoiceTransaction prototypeTimeInvoiceTransaction in prototypeTimeCodeTransaction.TimeInvoiceTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                    {
                        TimeInvoiceTransaction cloneTimeInvoiceTransaction = CopyTimeInvoiceTransaction(prototypeTimeInvoiceTransaction, identity.TimeBlockDate, cloneTimeBlock.TimeBlockId, employee.EmployeeId);
                        if (cloneTimeInvoiceTransaction != null)
                            cloneTimeCodeTransaction.TimeInvoiceTransaction.Add(cloneTimeInvoiceTransaction);
                    }

                    #endregion

                    #region TimePayrollTransaction

                    foreach (TimePayrollTransaction prototypeTimePayrollTransaction in prototypeTimeCodeTransaction.TimePayrollTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                    {
                        TimePayrollTransaction cloneTimePayrollTransaction = CopyTimePayrollTransaction(prototypeTimePayrollTransaction, identity.TimeBlockDate, cloneTimeBlock.TimeBlockId, employment);
                        if (cloneTimePayrollTransaction != null)
                        {
                            cloneTimeCodeTransaction.TimePayrollTransaction.Add(cloneTimePayrollTransaction);
                            payrollTransactionMapping.Add(Tuple.Create(prototypeTimePayrollTransaction.TimePayrollTransactionId, cloneTimePayrollTransaction));
                        }
                    }

                    #region PayrollProductChain - set parent

                    if (prototypeTimeCodeTransaction.TimePayrollTransaction.Any(i => i.IncludedInPayrollProductChain && i.ParentId.HasValue))
                    {
                        foreach (TimePayrollTransaction prototypeTimePayrollTransaction in prototypeTimeCodeTransaction.TimePayrollTransaction)
                        {
                            if (prototypeTimePayrollTransaction.IncludedInPayrollProductChain && prototypeTimePayrollTransaction.ParentId.HasValue)
                            {
                                var prototypeParentTransaction = prototypeTimeCodeTransaction.TimePayrollTransaction.FirstOrDefault(x => x.TimePayrollTransactionId == prototypeTimePayrollTransaction.ParentId.Value);
                                if (prototypeParentTransaction != null)
                                {
                                    var prototypeTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == prototypeTimePayrollTransaction.TimePayrollTransactionId);
                                    if (prototypeTuple != null)
                                    {
                                        var prototypeParentTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == prototypeParentTransaction.TimePayrollTransactionId);
                                        if (prototypeParentTuple != null)
                                            prototypeTuple.Item2.Parent = prototypeParentTuple.Item2;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            #endregion

            #region Copy external transactions without internal transaction

            foreach (TimeInvoiceTransaction prototypeTimeInvoiceTransaction in template.Outcome.TimeInvoiceTransactions.Where(i => i.TimeCodeTransaction == null))
            {
                CopyTimeInvoiceTransaction(prototypeTimeInvoiceTransaction, identity.TimeBlockDate, null, employee.EmployeeId);
            }

            foreach (TimePayrollTransaction prototypeTimePayrollTransaction in template.Outcome.TimePayrollTransactions.Where(i => i.TimeCodeTransaction == null))
            {
                CopyTimePayrollTransaction(prototypeTimePayrollTransaction, identity.TimeBlockDate, null, employment);
            }

            #endregion

            if (save)
                result = Save();

            if (result.Success)
                result = CreateTimeWorkReductionTransactions(identity.Employee, identity.TimeBlockDate.Date);

            ClearTimePayrollTransactionsWithTimeBlockDateFromCache(identity.TimeBlockDate);
            ApplyPlausibilityCheck(template);

            return result;
        }

        public bool UsePayrollOrAbsenceRules()
        {
            return UsePayroll() || !GetTimeAbsenceRuleHeadsWithRowsFromCache().IsNullOrEmpty();
        }

        private void EnsureScheduleForTimeEngineIdentity(TimeEngineTemplateIdentity identity)
        {
            if (identity?.Employee != null && identity.TimeBlockDate != null && identity.ScheduleBlocks.IsNullOrEmpty())
                identity.ScheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, identity.Employee.EmployeeId, identity.TimeBlockDate.Date, includeStandBy: true);
        }

        #endregion
    }

}
