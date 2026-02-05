using Newtonsoft.Json;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeWorkAccountManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeWorkAccountManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeWorkAccount

        public List<TimeWorkAccount> GetTimeWorkAccounts(int? id = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkAccount.NoTracking();
            return GetTimeWorkAccounts(entities, id);
        }

        public List<TimeWorkAccount> GetTimeWorkAccounts(CompEntities entities, int? id = null)
        {
            int actorCompanyId = base.ActorCompanyId;
            return entities.TimeWorkAccount.Where(a => a.ActorCompanyId == actorCompanyId && a.State == (int)SoeEntityState.Active && (!id.HasValue || (id.HasValue && id.Value == a.TimeWorkAccountId))).OrderBy(a => a.Name).ToList();
        }

        public TimeWorkAccount GetTimeWorkAccount(int timeWorkAccountId, bool loadYears = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkAccount.NoTracking();
            return GetTimeWorkAccount(entities, timeWorkAccountId, loadYears);
        }

        public TimeWorkAccount GetTimeWorkAccount(CompEntities entities, int timeWorkAccountId, bool loadYears = false)
        {
            int actorCompanyId = base.ActorCompanyId;

            IQueryable<TimeWorkAccount> query = entities.TimeWorkAccount;
            if (loadYears)
                query = query.Include("TimeWorkAccountYear");

            return query.FirstOrDefault(a => a.ActorCompanyId == actorCompanyId && a.TimeWorkAccountId == timeWorkAccountId && a.State == (int)SoeEntityState.Active);
        }

        public bool DoTimeWorkAccountExists(CompEntities entities, string name, int? excludeTimeWorkTimeAccountId = 0)
        {
            int actorCompanyId = base.ActorCompanyId;
            name = name?.ToLower();

            return !string.IsNullOrEmpty(name) && entities.TimeWorkAccount.Any(w =>
                w.ActorCompanyId == actorCompanyId &&
                w.Name == name &&
                w.State == (int)SoeEntityState.Active &&
                (!excludeTimeWorkTimeAccountId.HasValue || excludeTimeWorkTimeAccountId.Value != w.TimeWorkAccountId));
        }

        private ActionResult IsOkToDeleteTimeWorkAccount(CompEntities entities, int timeWorkAccountId)
        {
            if (entities.TimeWorkAccountYear.Any(i => i.TimeWorkAccountId == timeWorkAccountId && i.State == (int)SoeEntityState.Active))
                return new ActionResult((int)ActionResultDelete.TimeWorkAccountHasYears, GetText(91958, "Arbetstidskonto har år"));
            if (entities.EmployeeTimeWorkAccount.Any(i => i.TimeWorkAccountId == timeWorkAccountId && i.State == (int)SoeEntityState.Active))
                return new ActionResult((int)ActionResultDelete.TimeWorkAccountHasEmployees, GetText(91959, "Arbetstidskonto har anställda"));
            return new ActionResult(true);
        }

        public ActionResult SaveTimeWorkAccount(TimeWorkAccountDTO timeWorkAccountInput)
        {
            if (timeWorkAccountInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91956, "Arbetstidskonto hittades inte"));

            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                if (DoTimeWorkAccountExists(entities, timeWorkAccountInput.Name, excludeTimeWorkTimeAccountId: timeWorkAccountInput.TimeWorkAccountId.ToNullable()))
                    return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91957, "Arbetstidskonto med samma namn finns redan"));

                TimeWorkAccount timeWorkAccount = null;

                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (timeWorkAccountInput.IsNew)
                        {
                            timeWorkAccount = new TimeWorkAccount()
                            {
                                ActorCompanyId = base.ActorCompanyId,
                            };
                            SetCreatedProperties(timeWorkAccount);
                            entities.TimeWorkAccount.AddObject(timeWorkAccount);
                        }
                        else
                        {
                            timeWorkAccount = GetTimeWorkAccount(entities, timeWorkAccountInput.TimeWorkAccountId);
                            if (timeWorkAccount == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91956, "Arbetstidskonto hittades inte"));

                            SetModifiedProperties(timeWorkAccount);
                        }

                        timeWorkAccount.SetProperties(timeWorkAccountInput);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                        result.IntegerValue = timeWorkAccount?.TimeWorkAccountId ?? 0;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveTimeWorkAccountYearEmployeeChoice(int timeWorkAccountYearEmployeeId, int employeeId, int selectedWithdrawalMethod)
        {
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = GetTimeWorkAccountYearEmployee(entities, timeWorkAccountYearEmployeeId, employeeId);
                    if (timeWorkAccountYearEmployee == null)
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91956, "Arbetstidskonto hittades inte"));
                    if (timeWorkAccountYearEmployee.Status != (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated || timeWorkAccountYearEmployee.SentDate == null)
                        return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91999, "Arbetstidskontot har felaktig status och valet kan inte göras"));

                    timeWorkAccountYearEmployee.UserSelectedWithdrawalMethod((TermGroup_TimeWorkAccountWithdrawalMethod)selectedWithdrawalMethod);
                    SetModifiedProperties(timeWorkAccountYearEmployee);

                    return SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    return new ActionResult(ex);
                }
            }
        }

        public ActionResult DeleteTimeWorkAccount(int timeWorkAccountId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(entities, timeWorkAccountId);
                if (timeWorkAccount == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91956, "Arbetstidskonto hittades inte"));

                ActionResult result = IsOkToDeleteTimeWorkAccount(entities, timeWorkAccountId);
                if (!result.Success)
                    return result;

                result = ChangeEntityState(entities, timeWorkAccount, SoeEntityState.Deleted, true);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;

                return result;
            }
        }

        #endregion

        #region TimeWorkAccountYear

        public TimeWorkAccountYear GetTimeWorkAccountYear(int workTimeAccountYearId, int timeWorkAccountId, bool loadEmployees = false, bool loadWorkTimeWeek = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkAccountYear.NoTracking();
            return GetTimeWorkAccountYear(entities, workTimeAccountYearId, timeWorkAccountId, loadEmployees, loadWorkTimeWeek);
        }

        public TimeWorkAccountYear GetTimeWorkAccountYear(CompEntities entities, int workTimeAccountYearId, int timeWorkAccountId, bool loadEmployees = false, bool loadWorkTimeWeek = false)
        {
            IQueryable<TimeWorkAccountYear> query = entities.TimeWorkAccountYear;
            if (loadEmployees)
                query = query.Include("TimeWorkAccountYearEmployee").Include("TimeWorkAccountYearEmployee.Employee.ContactPerson");
            if (loadWorkTimeWeek)
                query = query.Include("TimeWorkAccountWorkTimeWeek");

            return (from w in query
                    where w.TimeWorkAccountId == timeWorkAccountId &&
                    w.TimeWorkAccountYearId == workTimeAccountYearId &&
                    w.State == (int)SoeEntityState.Active
                    select w).FirstOrDefault();
        }

        public TimeWorkAccountYear GetTimeWorkAccountLastYear(int timeWorkAccountId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeWorkAccountLastYear(entities, timeWorkAccountId);
        }

        public TimeWorkAccountYear GetTimeWorkAccountLastYear(CompEntities entities, int timeWorkAccountId)
        {
            return (from w in entities.TimeWorkAccountYear.Include("TimeWorkAccountWorkTimeWeek")
                    where w.TimeWorkAccountId == timeWorkAccountId &&
                    w.State == (int)SoeEntityState.Active
                    orderby w.EarningStart descending
                    select w).FirstOrDefault();
        }

        private ActionResult IsOkToDeleteTimeWorkAccountYear(CompEntities entities, int timeWorkAccountYearId)
        {
            if (entities.TimeWorkAccountYearEmployee.Any(i => i.TimeWorkAccountYearId == timeWorkAccountYearId && i.Status > (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated && i.State == (int)SoeEntityState.Active))
                return new ActionResult((int)ActionResultDelete.TimeWorkAccountYearHasEmployees, GetText(91960, "Året för arbettidskonto har anställda"));
            return new ActionResult(true);
        }

        private ActionResult ValidateTimeWorkAccountOverlappingYears(TimeWorkAccountYearDTO timeWorkAccountYearInput)
        {
            if (timeWorkAccountYearInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91956, "Arbetstidskonto hittades inte"));

            TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(timeWorkAccountYearInput.TimeWorkAccountId, loadYears: true);
            foreach (TimeWorkAccountYear year in timeWorkAccount.TimeWorkAccountYear.Where(w => w.TimeWorkAccountYearId != timeWorkAccountYearInput.TimeWorkAccountYearId && w.State == (int)SoeEntityState.Active))
            {
                if (CalendarUtility.IsDatesOverlapping(timeWorkAccountYearInput.EarningStart, timeWorkAccountYearInput.EarningStop, year.EarningStart, year.EarningStop))
                    return new ActionResult((int)ActionResultSave.TimeWorkAccountOverlapping, GetText(92015, "Årskörningar kan inte överlappa varandra"));
            }

            return new ActionResult(true);
        }

        public ActionResult SaveTimeWorkAccountYear(TimeWorkAccountYearDTO timeWorkAccountYearInput)
        {
            if (timeWorkAccountYearInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));

            bool loadEmployees = !timeWorkAccountYearInput.TimeWorkAccountYearEmployees.IsNullOrEmpty();
            bool isPercentChanged = false;

            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                TimeWorkAccountYear year = null;

                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    result = ValidateTimeWorkAccountOverlappingYears(timeWorkAccountYearInput);
                    if (!result.Success)
                        return result;

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region TimeWorkAccountYear

                        if (timeWorkAccountYearInput.IsNew)
                        {
                            year = new TimeWorkAccountYear()
                            {
                                TimeWorkAccountId = timeWorkAccountYearInput.TimeWorkAccountId,
                            };
                            SetCreatedProperties(year);
                            entities.TimeWorkAccountYear.AddObject(year);
                        }
                        else
                        {
                            year = GetTimeWorkAccountYear(entities, timeWorkAccountYearInput.TimeWorkAccountYearId, timeWorkAccountYearInput.TimeWorkAccountId, loadEmployees: loadEmployees, loadWorkTimeWeek: true);
                            if (year == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91961, "År för arbetstidskonto hittades inte"));

                            if (year.DirectPaymentPercent != timeWorkAccountYearInput.DirectPaymentPercent ||
                                year.PaidLeavePercent != timeWorkAccountYearInput.PaidLeavePercent ||
                                year.PensionDepositPercent != timeWorkAccountYearInput.PensionDepositPercent)
                                isPercentChanged = true;

                            SetModifiedProperties(year);
                        }

                        year.SetProperties(timeWorkAccountYearInput);

                        #endregion

                        #region TimeWorkAccountYearEmployee

                        if (year.TimeWorkAccountYearEmployee != null)
                        {
                            foreach (TimeWorkAccountYearEmployee yearEmployee in year.TimeWorkAccountYearEmployee.Where(w => w.State == (int)SoeEntityState.Active))
                            {
                                TimeWorkAccountYearEmployeeDTO yearEmployeeInput = timeWorkAccountYearInput.TimeWorkAccountYearEmployees.FirstOrDefault(y => y.TimeWorkAccountYearEmployeeId == yearEmployee.TimeWorkAccountYearEmployeeId);
                                if (yearEmployeeInput != null && yearEmployee.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed)
                                {
                                    decimal specifiedWorkingTimePromoted = yearEmployeeInput.SpecifiedWorkingTimePromoted ?? yearEmployee.CalculatedWorkingTimePromoted;

                                    if (yearEmployee.SelectedWithdrawalMethod != (int)yearEmployeeInput.SelectedWithdrawalMethod ||
                                        yearEmployee.SpecifiedWorkingTimePromoted != yearEmployeeInput.SpecifiedWorkingTimePromoted)
                                    {
                                        if (yearEmployeeInput.SelectedWithdrawalMethod > TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed && yearEmployee.SelectedWithdrawalMethod != (int)yearEmployeeInput.SelectedWithdrawalMethod && yearEmployee.Status < (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed)
                                            yearEmployee.Status = (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed;
                                        else if (yearEmployeeInput.SelectedWithdrawalMethod == TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed && yearEmployee.SelectedWithdrawalMethod != (int)yearEmployeeInput.SelectedWithdrawalMethod && yearEmployee.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed)
                                            yearEmployee.SetNotChoosed();

                                        yearEmployee.SetSelectedWithdrawalMethod(yearEmployeeInput.SelectedWithdrawalMethod, null);

                                        if (yearEmployee.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                                        {
                                            if (yearEmployee.SpecifiedWorkingTimePromoted != yearEmployeeInput.SpecifiedWorkingTimePromoted || isPercentChanged)
                                                yearEmployee.CalculateOptionAmounts(specifiedWorkingTimePromoted, year);

                                            yearEmployee.SpecifiedWorkingTimePromoted = yearEmployeeInput.SpecifiedWorkingTimePromoted;
                                        }

                                        SetModifiedProperties(yearEmployee);
                                    }
                                    else if (isPercentChanged && yearEmployee.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                                    {
                                        yearEmployee.CalculateOptionAmounts(specifiedWorkingTimePromoted, year);
                                        SetModifiedProperties(yearEmployee);
                                    }
                                }
                                else if (isPercentChanged && yearEmployee.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                                {
                                    yearEmployee.CalculateOptionAmounts(yearEmployee.CalculatedWorkingTimePromoted, year);
                                }
                            }
                        }

                        #endregion

                        #region WorkTimeWeek

                        if (year.TimeWorkAccountWorkTimeWeek != null)
                        {
                            // Check overlapping
                            bool overlapping = false;
                            for (int i = 0; i < timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks.Count && !overlapping; i++)
                            {
                                TimeWorkAccountWorkTimeWeekDTO source = timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks[i];
                                for (int y = 0; y < timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks.Count && !overlapping; y++)
                                {
                                    if (i == y)
                                        continue;

                                    TimeWorkAccountWorkTimeWeekDTO target = timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks[y];
                                    if ((source.WorkTimeWeekFrom > target.WorkTimeWeekFrom && source.WorkTimeWeekFrom < target.WorkTimeWeekTo) || (source.WorkTimeWeekTo > target.WorkTimeWeekFrom && source.WorkTimeWeekTo < target.WorkTimeWeekTo))
                                        overlapping = true;

                                }
                            }

                            if (overlapping)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91997, "Arbetsmått kan inte överlappa varandra"));

                            foreach (TimeWorkAccountWorkTimeWeek workTimeWeek in year.TimeWorkAccountWorkTimeWeek.Where(p => p.State == (int)SoeEntityState.Active).ToList())
                            {
                                TimeWorkAccountWorkTimeWeekDTO workTimeWeekInput = timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks.FirstOrDefault(w => w.TimeWorkAccountWorkTimeWeekId == workTimeWeek.TimeWorkAccountWorkTimeWeekId);
                                if (workTimeWeekInput != null)
                                {
                                    #region Update

                                    if (workTimeWeek.WorkTimeWeekFrom != workTimeWeekInput.WorkTimeWeekFrom || workTimeWeek.WorkTimeWeekTo != workTimeWeekInput.WorkTimeWeekTo || workTimeWeek.PaidLeaveTime != workTimeWeekInput.PaidLeaveTime)
                                    {
                                        workTimeWeek.WorkTimeWeekFrom = workTimeWeekInput.WorkTimeWeekFrom;
                                        workTimeWeek.WorkTimeWeekTo = workTimeWeekInput.WorkTimeWeekTo;
                                        workTimeWeek.PaidLeaveTime = workTimeWeekInput.PaidLeaveTime;
                                        SetModifiedProperties(workTimeWeek);
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Delete

                                    ChangeEntityState(workTimeWeek, SoeEntityState.Deleted);

                                    #endregion
                                }

                                // Remove from input to prevent adding it again below
                                timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks.Remove(workTimeWeekInput);
                            }

                            #region Add

                            // Add all worktimeweeks that is left in the input
                            if (timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks != null)
                            {
                                foreach (TimeWorkAccountWorkTimeWeekDTO workTimeWeekInput in timeWorkAccountYearInput.TimeWorkAccountWorkTimeWeeks)
                                {
                                    TimeWorkAccountWorkTimeWeek workTimeWeek = new TimeWorkAccountWorkTimeWeek()
                                    {
                                        WorkTimeWeekFrom = workTimeWeekInput.WorkTimeWeekFrom,
                                        WorkTimeWeekTo = workTimeWeekInput.WorkTimeWeekTo,
                                        PaidLeaveTime = workTimeWeekInput.PaidLeaveTime
                                    };
                                    SetCreatedProperties(workTimeWeek);
                                    year.TimeWorkAccountWorkTimeWeek.Add(workTimeWeek);
                                }
                            }

                            #endregion
                        }


                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                        result.IntegerValue = year?.TimeWorkAccountYearId ?? 0;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteTimeWorkAccountYear(int timeWorkAccountYearId, int timeWorkAccountId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(entities, timeWorkAccountYearId, timeWorkAccountId);
                if (timeWorkAccountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91995, "Årskörning hittades inte"));

                ActionResult result = IsOkToDeleteTimeWorkAccountYear(entities, timeWorkAccountYearId);
                if (!result.Success)
                    return result;

                result = ChangeEntityState(entities, timeWorkAccountYear, SoeEntityState.Deleted, true);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;

                return result;
            }
        }

        #endregion

        #region TimeWorkAccountYearEmployee

        public List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkAccountYearEmployee.NoTracking();
            return GetTimeWorkAccountYearEmployees(entities, employeeId, actorCompanyId);
        }

        public List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(CompEntities entities, int employeeId, int actorCompanyId)
        {
            return (from ye in entities.TimeWorkAccountYearEmployee
                        .Include("TimeWorkAccount")
                    where ye.EmployeeId == employeeId &&
                    ye.TimeWorkAccount.ActorCompanyId == actorCompanyId &&
                    ye.State == (int)SoeEntityState.Active
                    select ye).OrderBy(ye => ye.EarningStart).ThenBy(ye => ye.EarningStop).ToList();
        }

        private List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(TimeWorkAccount timeWorkAccount, int timeWorkAccountYearId, List<int> timeWorkAccountEmployeeIds)
        {
            if (timeWorkAccount == null)
                return new List<TimeWorkAccountYearEmployee>();

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeWorkAccountYearEmployee.NoTracking();
            var query = from y in entitiesReadOnly.TimeWorkAccountYearEmployee
                        where y.TimeWorkAccountYearId == timeWorkAccountYearId &&
                        y.TimeWorkAccountId == timeWorkAccount.TimeWorkAccountId &&
                        y.State == (int)SoeEntityState.Active &&
                        (y.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome ||
                        y.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance)
                        select y;

            if (!timeWorkAccountEmployeeIds.IsNullOrEmpty())
            {
                query = query.Where(w => timeWorkAccountEmployeeIds.Contains(w.TimeWorkAccountYearEmployeeId) &&
                        (w.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit ||
                            ((w.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave || w.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed)
                             && timeWorkAccount.DefaultPaidLeaveNotUsed == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit)
                            )
                        );
            }

            return query.ToList();
        }

        public TimeWorkAccountYearEmployee GetTimeWorkAccountYearEmployee(int timeWorkAccountYearEmployeeId, int employeeId, bool includeTimeWorkAccount = false, bool includeTimeWorkAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeWorkAccountYearEmployee.NoTracking();
            return GetTimeWorkAccountYearEmployee(entities, timeWorkAccountYearEmployeeId, employeeId, includeTimeWorkAccount, includeTimeWorkAccountYear);
        }

        public TimeWorkAccountYearEmployee GetTimeWorkAccountYearEmployee(CompEntities entities, int timeWorkAccountYearEmployeeId, int employeeId, bool includeTimeWorkAccount = false, bool includeTimeWorkAccountYear = false)
        {
            var query = (from y in entities.TimeWorkAccountYearEmployee
                         where y.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId &&
                         y.EmployeeId == employeeId &&
                         y.State == (int)SoeEntityState.Active
                         select y);

            if (includeTimeWorkAccount)
                query = query.Include("TimeWorkAccount");
            if (includeTimeWorkAccountYear)
                query = query.Include("TimeWorkAccountYear");

            return query.FirstOrDefault();
        }

        public List<TimeWorkAccountYearEmployeeBasisDTO> GetTimeWorkAccountYearEmployeeCalculationBasis(int timeWorkAccountYearEmployeeId, int employeeId)
        {
            TimeWorkAccountYearEmployee yearEmployee = GetTimeWorkAccountYearEmployee(timeWorkAccountYearEmployeeId, employeeId);
            if (yearEmployee == null || string.IsNullOrEmpty(yearEmployee.CalculationBasis))
                return null;

            return JsonConvert.DeserializeObject<List<TimeWorkAccountYearEmployeeBasisDTO>>(yearEmployee.CalculationBasis);
        }

        public List<TimePayrollTransaction> GetTimeWorkAccountYearEmployeeTransactions(List<int> timeWorkAccountYearOutcomeIds, out List<TimePeriod> timePeriods)
        {
            List<TimePayrollTransaction> transactions = new List<TimePayrollTransaction>();
            timePeriods = new List<TimePeriod>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (!timeWorkAccountYearOutcomeIds.IsNullOrEmpty())
            {
                entitiesReadOnly.TimePayrollTransaction.NoTracking();
                transactions = (from y in entitiesReadOnly.TimePayrollTransaction
                                where y.TimeWorkAccountYearOutcomeId.HasValue &&
                                timeWorkAccountYearOutcomeIds.Contains(y.TimeWorkAccountYearOutcomeId.Value) &&
                                y.State == (int)SoeEntityState.Active
                                select y).ToList();

                if (transactions.Any())
                {
                    List<int> timePeriodsId = transactions.Select(s => s.TimePeriodId.Value).Distinct().ToList();

                    entitiesReadOnly.TimePeriod.NoTracking();
                    timePeriods = (from y in entitiesReadOnly.TimePeriod
                                   where timePeriodsId.Contains(y.TimePeriodId)
                                   select y).ToList();
                }
            }
            return transactions;
        }

        public DateTime? GetTimeWorkAccountYearEmployeePaymentDate(int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId)
        {
            int? timeWorkAccountYearOutcomeId = GetTimeWorkAccountYearOutcome(timeWorkAccountYearId, timeWorkAccountYearEmployeeId, SoeTimeWorkAccountYearOutcomeType.Selection);
            if (!timeWorkAccountYearOutcomeId.HasValue)
                return null;

            DateTime? paymentDate = null;
            List<TimePayrollTransaction> payrollTransactions = GetTimeWorkAccountYearEmployeeTransactions(timeWorkAccountYearOutcomeId.Value.ObjToList(), out List<TimePeriod> timePeriods);
            if (payrollTransactions.Any() && timePeriods.Any())
            {
                TimePayrollTransaction payrollTransaction = payrollTransactions.FirstOrDefault(f => f.TimeWorkAccountYearOutcomeId == timeWorkAccountYearOutcomeId);
                paymentDate = timePeriods?.FirstOrDefault(f => payrollTransaction.TimePeriodId.HasValue && f.TimePeriodId == payrollTransaction.TimePeriodId.Value)?.PaymentDate;
            }
            return paymentDate;
        }

        public ActionResult DeleteTimeWorkAccountYearEmployeeRow(int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId, int employeeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = GetTimeWorkAccountYearEmployee(entities, timeWorkAccountYearEmployeeId, employeeId);
                if (timeWorkAccountYearEmployee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91956, "Arbetstidskonto hittades inte"));

                if (timeWorkAccountYearEmployee.Status != (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated || timeWorkAccountYearEmployee.TimeWorkAccountYearId != timeWorkAccountYearId)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92013, "Arbetstidskontot har felaktig status och kan inte tas bort"));

                ActionResult result = ChangeEntityState(entities, timeWorkAccountYearEmployee, SoeEntityState.Deleted, true);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;

                return result;
            }
        }

        #endregion

        #region TimeWorkAccountYearOutcome

        private int? GetTimeWorkAccountYearOutcome(int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId, SoeTimeWorkAccountYearOutcomeType? type = null)
        {
            return GetTimeWorkAccountYearOutcomes(timeWorkAccountYearId, timeWorkAccountYearEmployeeId.ObjToList(), type)?.FirstOrDefault(w => w.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId)?.TimeWorkAccountYearOutcomeId;
        }

        private List<TimeWorkAccountYearOutcome> GetTimeWorkAccountYearOutcomes(int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds, SoeTimeWorkAccountYearOutcomeType? type = null)
        {
            if (timeWorkAccountYearEmployeeIds.IsNullOrEmpty())
                return new List<TimeWorkAccountYearOutcome>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var query = (from t in entitiesReadOnly.TimeWorkAccountYearOutcome
                         where t.TimeWorkAccountYearId == timeWorkAccountYearId &&
                         timeWorkAccountYearEmployeeIds.Contains(t.TimeWorkAccountYearEmployeeId) &&
                         t.State == (int)SoeEntityState.Active
                         select t);

            if (type != null)
                query = query.Where(w => w.Type == (int)type.Value);

            return query.ToList();
        }

        #endregion

        #region TimeWorkAccountExportPension

        public List<TimeWorkAccountExportPensionDTO> GetTimeWorkAccountYearPensionExport(int timeWorkAccountId, int timeWorkAccountYearId, List<int> timeWorkAccountEmployeeIds, int roleId)
        {
            var result = new List<TimeWorkAccountExportPensionDTO>();

            var timeWorkAccount = GetTimeWorkAccount(timeWorkAccountId, loadYears: true);
            var timeWorkAccountYear = timeWorkAccount?.TimeWorkAccountYear?.FirstOrDefault(w => w.TimeWorkAccountYearId == timeWorkAccountYearId);
            if (timeWorkAccount == null || timeWorkAccountYear == null)
                return result;

            var timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccount, timeWorkAccountYearId, timeWorkAccountEmployeeIds);
            if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                return result;

            var employees = EmployeeManager.GetAllEmployeesByIds(base.ActorCompanyId, timeWorkAccountYearEmployees.Select(s => s.EmployeeId).ToList());
            if (employees.IsNullOrEmpty())
                return result;

            var timeWorkAccountYearOutcomes = GetTimeWorkAccountYearOutcomes(timeWorkAccountYearId, timeWorkAccountEmployeeIds);
            if (timeWorkAccountYearOutcomes.IsNullOrEmpty())
                return result;

            var timeWorkAccountYearOutcomeIds = timeWorkAccountYearOutcomes.Select(w => w.TimeWorkAccountYearOutcomeId).ToList();
            var timePayrollTransactions = GetTimeWorkAccountYearEmployeeTransactions(timeWorkAccountYearOutcomeIds, out List<TimePeriod> timePeriods)
                .Where(w => timeWorkAccountYear.PensionDepositPayrollProductId.HasValue && w.ProductId == timeWorkAccountYear.PensionDepositPayrollProductId.Value)
                .ToList();
            if (timePayrollTransactions.IsNullOrEmpty())
                return result;

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, base.ActorCompanyId);

            foreach (var payrollTransaction in timePayrollTransactions)
            {
                var employee = employees.FirstOrDefault(f => f.EmployeeId == payrollTransaction.EmployeeId);
                if (employee == null)
                    continue;

                var timeWorkAccountYearOutcome = timeWorkAccountYearOutcomes.FirstOrDefault(w => payrollTransaction.TimeWorkAccountYearOutcomeId.HasValue && w.TimeWorkAccountYearOutcomeId == payrollTransaction.TimeWorkAccountYearOutcomeId.Value);

                var dto = new TimeWorkAccountExportPensionDTO()
                {
                    EmployeeId = employee.EmployeeId,
                    TimeWorkAccountId = timeWorkAccountId,
                    TimeWorkAccountYearId = timeWorkAccountYearId,
                    EmployeeNrAndName = employee.EmployeeNrAndName,
                    EmployeeSocialSec = showSocialSec ? StringUtility.SocialSecYYYYMMDD_Dash_XXXX(employee.SocialSec) : string.Empty,
                    Amount = payrollTransaction.Amount ?? decimal.Zero,
                    PaymentDate = timePeriods?.FirstOrDefault(f => payrollTransaction.TimePeriodId.HasValue && f.TimePeriodId == payrollTransaction.TimePeriodId.Value)?.PaymentDate,
                    Ended = timeWorkAccountYearOutcome != null && timeWorkAccountYearOutcome.Type == (int)SoeTimeWorkAccountYearOutcomeType.AccAdjustment,
                };

                result.Add(dto);
            }

            return result;
        }

        #endregion
    }
}
