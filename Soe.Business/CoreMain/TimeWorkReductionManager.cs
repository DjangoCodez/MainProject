using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeWorkReductionManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeWorkReductionManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeWorkReduction

        public bool UseTimeWorkReduction(CompEntities entities, int actorCompanyId)
        {
            return entities.TimeAccumulator.Any(e =>
                e.ActorCompanyId == actorCompanyId &&
                e.TimeWorkReductionEarningId.HasValue &&
                e.Type == (int)TermGroup_TimeWorkReductionPeriodType.Week &&
                e.State == (int)SoeEntityState.Active
                );
        }

        #endregion

        #region TimeWorkReductionEarning

        public TimeWorkReductionEarning GetTimeWorkReductionEarning(CompEntities entities, int timeWorkReductionEarningId)
        {
            return (from tw in entities.TimeWorkReductionEarning
                    .Include("TimeAccumulatorTimeWorkReductionEarningEmployeeGroup")
                    where tw.TimeWorkReductionEarningId == timeWorkReductionEarningId &&
                    tw.State == (int)SoeEntityState.Active
                    select tw).FirstOrDefault();
        }

        public bool HasOverlappingTimeWorkReductionEarningEmployeeGroups(CompEntities entities, TimeAccumulatorDTO timeAccumulatorInput, out string message)
        {
            message = string.Empty;

            var inputEarningEmployeeGroups = timeAccumulatorInput?.TimeWorkReductionEarning?.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup?.Where(t => t.State == SoeEntityState.Active);
            if (inputEarningEmployeeGroups.IsNullOrEmpty())
                return false;

            int actorCompanyId = base.ActorCompanyId;

            var otherEarningEmployeeGroups = entities.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup
                .Include(eeg => eeg.EmployeeGroup)
                .Include(eeg => eeg.TimeWorkReductionEarning.TimeAccumulator)
                .Where(eeg =>
                    eeg.TimeWorkReductionEarning.TimeAccumulator.Any(ta => ta.ActorCompanyId == actorCompanyId && ta.State == (int)SoeEntityState.Active) &&
                    eeg.State == (int)SoeEntityState.Active
                )
                .ToList();

            if (!otherEarningEmployeeGroups.Any())
                return false;

            var sb = new StringBuilder();
            bool hasOverlapping = false;

            foreach (var inputEarningEmployeeGroup in inputEarningEmployeeGroups)
            {
                foreach (var otherEarningEmployeeGroup in otherEarningEmployeeGroups)
                {
                    if (inputEarningEmployeeGroup.EmployeeGroupId != otherEarningEmployeeGroup.EmployeeGroupId)
                        continue;
                    if (inputEarningEmployeeGroup.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId == otherEarningEmployeeGroup.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId)
                        continue;

                    if (CalendarUtility.IsDatesOverlappingNullable(
                        inputEarningEmployeeGroup.DateFrom, inputEarningEmployeeGroup.DateTo,
                        otherEarningEmployeeGroup.DateFrom, otherEarningEmployeeGroup.DateTo,
                        validateDatesAreTouching: true))
                    {
                        sb.AppendFormat(
                            GetText(8880, "På Saldoregel '{0}' överlappar tidavtalet '{1} {2} - {3}'"),
                            otherEarningEmployeeGroup.TimeWorkReductionEarning.TimeAccumulator.First().Name,
                            otherEarningEmployeeGroup.EmployeeGroup.Name,
                            otherEarningEmployeeGroup.DateFrom?.ToShortDateString() ?? " -- ",
                            otherEarningEmployeeGroup.DateTo?.ToShortDateString() ?? " -- "
                        );
                        sb.Append("<br/>");
                        hasOverlapping = true;
                    }
                }
            }

            if (hasOverlapping)
                message = sb.ToString();

            return hasOverlapping;
        }

        public ActionResult SaveTimeAccumulatorTimeWorkReductionEarnings(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput, out List<Employee> employeesWithTransactionsToRecalculate)
        {
            ActionResult result = new ActionResult();

            employeesWithTransactionsToRecalculate = new List<Employee>();
            TimeWorkReductionEarning existingTimeWorkReductionEarning = timeAccumulator.TimeWorkReductionEarning != null && timeAccumulator.TimeWorkReductionEarning.State == (int)SoeEntityState.Active ? timeAccumulator.TimeWorkReductionEarning : null;

            #region Delete

            if (existingTimeWorkReductionEarning != null && existingTimeWorkReductionEarning.TimeWorkReductionEarningId != timeAccumulatorInput.TimeWorkReductionEarning?.TimeWorkReductionEarningId)
            {
                result = ChangeEntityState(entities, existingTimeWorkReductionEarning, SoeEntityState.Deleted, false);
                if (!result.Success)
                    return result;
            }
                

            #endregion

            #region Add/update

            if (timeAccumulatorInput.TimeWorkReductionEarning != null)
            {
                if (existingTimeWorkReductionEarning == null)
                {
                    existingTimeWorkReductionEarning = TimeWorkReductionEarning.Create(
                        timeAccumulatorInput.TimeWorkReductionEarning.MinutesWeight,
                        timeAccumulatorInput.TimeWorkReductionEarning.PeriodType,
                        timeAccumulator
                        );
                    SetCreatedProperties(existingTimeWorkReductionEarning);
                }
                else
                {
                    existingTimeWorkReductionEarning.Update(timeAccumulatorInput.TimeWorkReductionEarning.MinutesWeight, timeAccumulatorInput.TimeWorkReductionEarning.PeriodType);
                    SetModifiedProperties(existingTimeWorkReductionEarning);
                }
            }

            SaveTimeAccumulatorTimeWorkReductionEarningEmployeeGroup(entities, timeAccumulator, timeAccumulatorInput, out var removedOrShortenedTimeWorkReductionEarningEmployeeGroupRanges);

            #endregion

            if (removedOrShortenedTimeWorkReductionEarningEmployeeGroupRanges.Any())
            {
                foreach (var pair in removedOrShortenedTimeWorkReductionEarningEmployeeGroupRanges)
                {
                    var timePayrollTransactionsForAccumulator = TimeTransactionManager.GetTimePayrollTransactionsTimeAccumulator(entities, timeAccumulator.TimeAccumulatorId, pair.Value);
                    var employeeIds = timePayrollTransactionsForAccumulator
                        .Select(tpt => tpt.EmployeeId)
                        .Distinct()
                        .ToList();
                    var employees = EmployeeManager
                        .GetEmployees(entities, base.ActorCompanyId, employeeIds, onlyActive: true, loadContactPerson: true, loadEmployment: true)
                        .Where(e => e.GetEmployeeGroupId(pair.Value.First().Start, pair.Value.First().Stop) == pair.Key)
                        .ToList();

                    employeesWithTransactionsToRecalculate.AddRange(employees);
                }
            }

            return result;
        }

        private void AddRemovedTimeWorkReductionEarningEmployeeGroup(Dictionary<int, List<DateRangeDTO>> removedDateRangesByEmployeeGroupId, int employeeGroupId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (removedDateRangesByEmployeeGroupId == null)
                removedDateRangesByEmployeeGroupId = new Dictionary<int, List<DateRangeDTO>>();
            if (!removedDateRangesByEmployeeGroupId.ContainsKey(employeeGroupId))
                removedDateRangesByEmployeeGroupId[employeeGroupId] = new List<DateRangeDTO>();
            removedDateRangesByEmployeeGroupId[employeeGroupId].Add(new DateRangeDTO(dateFrom ?? DateTime.MinValue, dateTo ?? DateTime.MaxValue));
        }

        private void SaveTimeAccumulatorTimeWorkReductionEarningEmployeeGroup(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput, out Dictionary<int, List<DateRangeDTO>> removedDateRangesByEmployeeGroupId)
        {
            removedDateRangesByEmployeeGroupId = new Dictionary<int, List<DateRangeDTO>>();

            if (timeAccumulator == null || timeAccumulatorInput == null)
                return;

            var existingEarningEmployeeGroups = timeAccumulator.TimeWorkReductionEarning?.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup?
                .Where(r => r.State == (int)SoeEntityState.Active)
                .ToList()
                ?? new List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup>();

            var inputEarningEmployeeGroups = timeAccumulatorInput.TimeWorkReductionEarning?.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup?
                .Where(r => r.State == (int)SoeEntityState.Active)
                .ToList()
                ?? new List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO>();

            #region Delete
            foreach (var existing in existingEarningEmployeeGroups)
            {
                if (!inputEarningEmployeeGroups.Any(input => input.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId == existing.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId))
                {
                    AddRemovedTimeWorkReductionEarningEmployeeGroup(removedDateRangesByEmployeeGroupId, existing.EmployeeGroupId, existing.DateFrom, existing.DateTo);
                    ChangeEntityState(entities, existing, SoeEntityState.Deleted, false);
                }
            }
            #endregion

            #region Add/update
            List<int> updatedIds = new List<int>();
            foreach (var input in inputEarningEmployeeGroups)
            {
                var existing = existingEarningEmployeeGroups
                    .FirstOrDefault(r =>
                        r.State == (int)SoeEntityState.Active &&
                        r.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId == input.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId &&
                        !updatedIds.Contains(r.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId)
                    );

                if (existing == null)
                {
                    existing = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroup()
                    {
                        EmployeeGroupId = input.EmployeeGroupId,
                        DateFrom = input.DateFrom,
                        DateTo = input.DateTo,
                    };
                    SetCreatedProperties(existing);

                    if (timeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup == null)
                        timeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup = new EntityCollection<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup>();

                    timeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Add(existing);
                    updatedIds.Add(existing.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId);
                }
                else
                {
                    DateTime currentStart = existing.DateFrom ?? DateTime.MinValue;
                    DateTime currentStop = existing.DateTo ?? DateTime.MaxValue;
                    DateTime newStart = input.DateFrom ?? DateTime.MinValue;
                    DateTime newStop = input.DateTo ?? DateTime.MaxValue;

                    if (!CalendarUtility.IsDatesOverlapping(newStart, newStop, currentStart, currentStop))
                    {
                        AddRemovedTimeWorkReductionEarningEmployeeGroup(removedDateRangesByEmployeeGroupId, existing.EmployeeGroupId, currentStart, currentStop);
                    }
                    else
                    {
                        if (currentStart < newStart)
                            AddRemovedTimeWorkReductionEarningEmployeeGroup(removedDateRangesByEmployeeGroupId, existing.EmployeeGroupId, currentStart, newStart);
                        if (currentStop > newStop)
                            AddRemovedTimeWorkReductionEarningEmployeeGroup(removedDateRangesByEmployeeGroupId, existing.EmployeeGroupId, newStop, currentStop);
                    }

                    existing.EmployeeGroupId = input.EmployeeGroupId;
                    existing.DateFrom = input.DateFrom;
                    existing.DateTo = input.DateTo;
                    SetModifiedProperties(existing);
                    updatedIds.Add(existing.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId);
                }
            }
            #endregion
        }

        #endregion

        #region TimeWorkReductionReconciliation

        public List<TimeWorkReductionReconciliation> GetTimeWorkReductionReconciliations(int actorCompanyId, int? timeWorkReductionReconciliationId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkReductionReconciliation.NoTracking();
            return (from twrr in entities.TimeWorkReductionReconciliation
                    where twrr.ActorCompanyId == actorCompanyId &&
                    twrr.State == (int)SoeEntityState.Active &&
                    (!timeWorkReductionReconciliationId.HasValue || twrr.TimeWorkReductionReconciliationId == timeWorkReductionReconciliationId.Value)
                    select twrr).ToList();
        }

        public TimeWorkReductionReconciliation GetTimeWorkReductionReconciliation(int actorCompanyId, int timeWorkReductionReconciliationId, bool includeYears = false, bool includeEmployees = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeWorkReductionReconciliation(entities, actorCompanyId, timeWorkReductionReconciliationId, includeYears, includeEmployees);
        }

        public TimeWorkReductionReconciliation GetTimeWorkReductionReconciliation(CompEntities entities, int actorCompanyId, int timeWorkReductionReconciliationId, bool includeYears = false, bool includeEmployees = false)
        {
            IQueryable<TimeWorkReductionReconciliation> query = (from twrr in entities.TimeWorkReductionReconciliation
                                                                 where twrr.ActorCompanyId == actorCompanyId &&
                                                                 twrr.State == (int)SoeEntityState.Active &&
                                                                 twrr.TimeWorkReductionReconciliationId == timeWorkReductionReconciliationId
                                                                 select twrr);

            if (includeYears)
                query = query.Include("TimeWorkReductionReconciliationYear");

            if (includeEmployees)
                query = query.Include("TimeWorkReductionReconciliationYear.TimeWorkReductionReconciliationEmployee");

            return query.FirstOrDefault();
        }

        public ActionResult SaveTimeWorkReductionReconciliation(TimeWorkReductionReconciliationDTO input, int actorCompanyId)
        {
            if (input == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeWorkReductionReconciliation");

            ActionResult result = null;
            int timeWorkReductionReconciliationId = input.TimeWorkReductionReconciliationId;

            using (CompEntities entities = new CompEntities())
            {

                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        TimeWorkReductionReconciliation timeWorkReductionReconciliation = GetTimeWorkReductionReconciliation(entities, actorCompanyId, input.TimeWorkReductionReconciliationId);
                        if (timeWorkReductionReconciliation == null)
                        {
                            #region Add

                            timeWorkReductionReconciliation = new TimeWorkReductionReconciliation()
                            {
                                State = (int)SoeEntityState.Active,
                            };
                            SetCreatedProperties(timeWorkReductionReconciliation);
                            entities.AddObject("TimeWorkReductionReconciliation", timeWorkReductionReconciliation);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(timeWorkReductionReconciliation);

                            #endregion
                        }
                        timeWorkReductionReconciliation.Description = input.Description;
                        timeWorkReductionReconciliation.ActorCompanyId = actorCompanyId;
                        timeWorkReductionReconciliation.TimeAccumulatorId = input.TimeAccumulatorId;
                        timeWorkReductionReconciliation.UsePensionDeposit = input.UsePensionDeposit;
                        timeWorkReductionReconciliation.UseDirectPayment = input.UseDirectPayment;
                        timeWorkReductionReconciliation.DefaultWithdrawalMethod = (int)input.DefaultWithdrawalMethod;

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            timeWorkReductionReconciliationId = timeWorkReductionReconciliation.TimeWorkReductionReconciliationId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result != null && result.Success)
                        result.IntegerValue = timeWorkReductionReconciliationId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;

            }
        }

        public ActionResult DeleteTimeWorkReductionReconciliation(int timeWorkReductionReconciliationId, int actorCompanyId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                TimeWorkReductionReconciliation timeWorkReductionReconciliation = GetTimeWorkReductionReconciliation(entities, actorCompanyId, timeWorkReductionReconciliationId, includeYears: true, includeEmployees: true);
                if (timeWorkReductionReconciliation == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "TimeWorkReductionReconciliation");

                if (timeWorkReductionReconciliation.TimeWorkReductionReconciliationYear.Any(w => w.State == (int)SoeEntityState.Active))
                {
                    foreach (var year in timeWorkReductionReconciliation.TimeWorkReductionReconciliationYear.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        if (HasActiveEmployeesWithOutcomeOrHigher(year))
                            return new ActionResult((int)ActionResultDelete.NothingDeleted, GetText(7381, "Arbetstidsförkortningen innehåller aktiva avstämningar och kan inte tas bort"));

                        if (year.TimeWorkReductionReconciliationEmployee.Any(w => w.State == (int)SoeEntityState.Active))
                        {
                            foreach (var employee in year.TimeWorkReductionReconciliationEmployee.Where(w => w.State == (int)SoeEntityState.Active))
                            {
                                result = ChangeEntityState(entities, employee, SoeEntityState.Deleted, saveChanges: false);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        result = ChangeEntityState(entities, year, SoeEntityState.Deleted, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                }

                result = ChangeEntityState(entities, timeWorkReductionReconciliation, SoeEntityState.Deleted, saveChanges: false);
                if (!result.Success)
                    return result;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region TimeWorkReductionReconciliationYear

        public TimeWorkReductionReconciliationYear GetTimeWorkReductionReconciliationYear(CompEntities entities, int timeWorkReductionReconciliationYearId, bool includeEmployees = false)
        {
            var query = from y in entities.TimeWorkReductionReconciliationYear
                        where y.State == (int)SoeEntityState.Active &&
                        y.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId
                        select y;

            if (includeEmployees)
                query = query.Include("TimeWorkReductionReconciliationEmployee");

            return query.FirstOrDefault();
        }

        public ActionResult SaveTimeWorkReductionReconciliationYear(TimeWorkReductionReconciliationYearDTO input)
        {
            if (input == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeWorkReductionReconciliationYear");

            ActionResult result = null;
            int timeWorkReductionReconciliationYearId = input.TimeWorkReductionReconciliationYearId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear = GetTimeWorkReductionReconciliationYear(entities, input.TimeWorkReductionReconciliationYearId, includeEmployees: true);
                        if (timeWorkReductionReconciliationYear == null)
                        {
                            #region Add

                            timeWorkReductionReconciliationYear = new TimeWorkReductionReconciliationYear()
                            {
                                State = (int)SoeEntityState.Active,
                                TimeWorkReductionReconciliationId = input.TimeWorkReductionReconciliationId,
                            };
                            SetCreatedProperties(timeWorkReductionReconciliationYear);
                            entities.AddObject("TimeWorkReductionReconciliationYear", timeWorkReductionReconciliationYear);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(timeWorkReductionReconciliationYear);

                            #endregion
                        }
                        timeWorkReductionReconciliationYear.Stop = input.Stop;
                        timeWorkReductionReconciliationYear.EmployeeLastDecidedDate = input.EmployeeLastDecidedDate;
                        timeWorkReductionReconciliationYear.PensionDepositPayrollProductId = input.PensionDepositPayrollProductId.ToNullable();
                        timeWorkReductionReconciliationYear.DirectPaymentPayrollProductId = input.DirectPaymentPayrollProductId.ToNullable();

                        if (input.TimeWorkReductionReconciliationEmployeeDTO != null)
                        {
                            foreach (var employee in input.TimeWorkReductionReconciliationEmployeeDTO)
                            {
                                var currentEmployee = timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationEmployee?.FirstOrDefault(w => w.State == (int)SoeEntityState.Active && w.EmployeeId == employee.EmployeeId);
                                if (currentEmployee != null && (currentEmployee.SelectedWithdrawalMethod != (int)employee.SelectedWithdrawalMethod) && currentEmployee.Status <= (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed)
                                {
                                    if (currentEmployee.SelectedWithdrawalMethod != (int)employee.SelectedWithdrawalMethod && (int)employee.SelectedWithdrawalMethod > 0)
                                    {
                                        if (currentEmployee.Status == (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated)
                                            currentEmployee.Status = (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed;

                                        currentEmployee.SelectedDate = DateTime.Now;
                                    }
                                    else
                                    {
                                        currentEmployee.SelectedDate = null;
                                        if (currentEmployee.Status == (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed)
                                            currentEmployee.Status = (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated;
                                    }

                                    currentEmployee.SelectedWithdrawalMethod = (int)employee.SelectedWithdrawalMethod;

                                    SetModifiedProperties(currentEmployee);

                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            timeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationYearId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex, this.log);
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = timeWorkReductionReconciliationYearId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;

            }
        }

        public ActionResult DeleteTimeWorkReductionReconciliationYear(int timeWorkReductionReconciliationYearId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear = GetTimeWorkReductionReconciliationYear(entities, timeWorkReductionReconciliationYearId, includeEmployees: true);
                if (timeWorkReductionReconciliationYear == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "TimeWorkReductionReconciliationYear");

                if (HasActiveEmployeesWithOutcomeOrHigher(timeWorkReductionReconciliationYear))
                    return new ActionResult((int)ActionResultDelete.NothingDeleted, GetText(7380, "Avstämningen är aktiv och kan inte tas bort"));

                if (timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationEmployee.Any(w => w.State == (int)SoeEntityState.Active))
                {
                    foreach (var employee in timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationEmployee.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        ChangeEntityState(entities, employee, SoeEntityState.Deleted, true);
                    }
                }
                return ChangeEntityState(entities, timeWorkReductionReconciliationYear, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region TimeWorkReductionReconciliationEmployee

        public List<TimeWorkReductionReconciliationEmployee> GetTimeWorkReductionReconciliationEmployee(int timeWorkReductionReconciliationYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkReductionReconciliationEmployee.NoTracking();
            return (from twre in entities.TimeWorkReductionReconciliationEmployee
                        .Include("Employee.ContactPerson")
                    where twre.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId &&
                    twre.State == (int)SoeEntityState.Active
                    select twre).ToList();
        }

        private List<TimeWorkReductionReconciliationEmployee> GetTimeWorkReductionReconciliationEmployee(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationEmployeeIds)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeWorkAccountYearEmployee.NoTracking();
            var query = from y in entitiesReadOnly.TimeWorkReductionReconciliationEmployee
                        where y.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId &&
                        y.TimeWorkReductionReconciliationId == timeWorkReductionReconciliationId &&
                        y.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit &&
                        y.State == (int)SoeEntityState.Active &&
                        y.Status == (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome
                        select y;

            if (!timeWorkReductionReconciliationEmployeeIds.IsNullOrEmpty())
                query = query.Where(w => timeWorkReductionReconciliationEmployeeIds.Contains(w.TimeWorkReductionReconciliationEmployeeId));

            return query.ToList();
        }

        private bool HasActiveEmployeesWithOutcomeOrHigher(TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear)
        {
            return timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationEmployee.Any(e => e.State == (int)SoeEntityState.Active && e.Status >= (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome);
        }

        #endregion

        #region TimeWorkReductionReconciliationOutcome

        private List<TimeWorkReductionReconciliationOutcome> GetTimeWorkReductionReconciliationOutcomes(int timeWorkReductionReconciliationYearId, List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkReductionReconciliationOutcome.NoTracking();
            return (from t in entities.TimeWorkReductionReconciliationOutcome
                    where t.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId &&
                    employeeIds.Contains(t.EmployeeId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimeWorkReductionYearOutcomeTransactions(List<int> timeWorkReductionYearOutcomeIds)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimePayrollTransaction.NoTracking();
            return (from y in entitiesReadOnly.TimePayrollTransaction
                    where y.TimeWorkReductionReconciliationOutcomeId.HasValue &&
                    timeWorkReductionYearOutcomeIds.Contains(y.TimeWorkReductionReconciliationOutcomeId.Value) &&
                    y.State == (int)SoeEntityState.Active
                    select y).ToList();
        }

        #endregion

        #region TimeWorkReductionExportPension

        public List<TimeWorkReductionExportPensionDTO> GetTimeWorkReductionYearPensionExport(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationEmployeeIds)
        {
            var result = new List<TimeWorkReductionExportPensionDTO>();

            var timeWorkReduction = TimeWorkReductionManager.GetTimeWorkReductionReconciliation(base.ActorCompanyId, timeWorkReductionReconciliationId, includeYears: true);
            var timeWorkReductionYear = timeWorkReduction?.TimeWorkReductionReconciliationYear?.FirstOrDefault(w => w.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId);
            if (timeWorkReduction == null || timeWorkReductionYear == null)
                return result;

            var timeWorkReductionEmployees = GetTimeWorkReductionReconciliationEmployee(timeWorkReductionReconciliationId, timeWorkReductionReconciliationYearId, timeWorkReductionReconciliationEmployeeIds);
            if (timeWorkReductionEmployees.IsNullOrEmpty())
                return result;

            var employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, employeeFilter: timeWorkReductionEmployees.Select(s => s.EmployeeId).ToList(), dateFrom: timeWorkReductionYear.Stop, dateTo: timeWorkReductionYear.Stop);
            if (employees.IsNullOrEmpty())
                return result;

            var employeeIds = employees.Select(s => s.EmployeeId).ToList();
            var timeWorkReductionYearOutcomes = GetTimeWorkReductionReconciliationOutcomes(timeWorkReductionReconciliationYearId, employeeIds);
            if (!timeWorkReductionYearOutcomes.Any())
                return result;

            var timeWorkReductionYearOutcomeIds = timeWorkReductionYearOutcomes.Select(s => s.TimeWorkReductionReconciliationOutcomeId).ToList();
            var timePayrollTransactions = GetTimeWorkReductionYearOutcomeTransactions(timeWorkReductionYearOutcomeIds);
            if (!timePayrollTransactions.Any())
                return result;

            var timePeriodIds = timePayrollTransactions.Select(s => s.TimePeriodId.Value).Distinct().ToList();
            var timePeriods = TimePeriodManager.GetTimePeriods(timePeriodIds, base.ActorCompanyId);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, base.RoleId, base.ActorCompanyId);

            foreach (var timePayrollTransaction in timePayrollTransactions)
            {
                var employee = employees.FirstOrDefault(f => f.EmployeeId == timePayrollTransaction.EmployeeId);
                if (employee == null)
                    continue;

                var dto = new TimeWorkReductionExportPensionDTO()
                {
                    EmployeeId = employee.EmployeeId,
                    TimeWorkReductionReconciliationId = timeWorkReductionReconciliationId,
                    TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYearId,
                    EmployeeNr = employee?.EmployeeNr ?? string.Empty,
                    EmployeeName = employee?.Name ?? string.Empty,
                    EmployeeSocialSec = showSocialSec ? StringUtility.SocialSecYYYYMMDD_Dash_XXXX(employee.SocialSec) : string.Empty,
                    Amount = timePayrollTransaction.Amount ?? decimal.Zero,
                    PaymentDate = timePeriods?.FirstOrDefault(f => timePayrollTransaction.TimePeriodId.HasValue && f.TimePeriodId == timePayrollTransaction.TimePeriodId.Value)?.PaymentDate,
                };

                result.Add(dto);
            }

            return result;
        }

        #endregion
    }
}
