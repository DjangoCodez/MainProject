using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
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
    public class TimeAccumulatorManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<TimeCode> cachedTimeCodesWithProducts = new List<TimeCode>();
        private readonly List<TimePeriodHeadDTO> cachedTimePeriodHeads = new List<TimePeriodHeadDTO>();

        #endregion

        #region Ctor

        public TimeAccumulatorManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region EmployeeAccumulator

        public List<EmployeeAccumulatorDTO> GetEmployeeAccumulators(List<int> employeeIds, List<int> accumulatorIds, DateTime dateFrom, DateTime dateTo, TermGroup_TimeAccumulatorCompareModel compareModel = TermGroup_TimeAccumulatorCompareModel.Unknown, int? ownLimitMin = null, int? ownLimitMax = null)
        {
            List<EmployeeAccumulatorDTO> employeeAccumulators = new List<EmployeeAccumulatorDTO>();

            List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, dateFrom: dateFrom, dateTo: dateTo, employeeFilter: employeeIds);
            if (employees.IsNullOrEmpty())
                return employeeAccumulators;

            TermGroup_AccumulatorTimePeriodType comparePeriodType = GetAccumulatorPeriodType(compareModel);
            List<TermGroup_VacationBalance> vacationBalances = GetAccumulatorVacationBalances(accumulatorIds).ToList();
            bool calculateAccTodayValue = comparePeriodType == TermGroup_AccumulatorTimePeriodType.AccToday;
            bool calculateRangeValue = comparePeriodType == TermGroup_AccumulatorTimePeriodType.Range;
            bool calculateTimeWorkAccount = accumulatorIds.IsNullOrEmpty() || accumulatorIds.Any(id => id == Constants.TIMEACCUMULATOR_TIMEWORKACCOUNT);

            GetTimeAccumulatorItemsInput input = GetTimeAccumulatorItemsInput.CreateInput(base.ActorCompanyId, base.UserId, 0, dateFrom, dateTo, calculateAccToday: true, calculateAccTodayValue: calculateAccTodayValue, calculateRange: true, calculateRangeValue: calculateRangeValue, employees: employees, timeAccumulatorIds: accumulatorIds.ToNullable());
            Dictionary<int, List<TimeAccumulatorItem>> timeAccumulatorItemsByEmployee = TimeAccumulatorManager.GetTimeAccumulatorItemsByEmployee(input);
            
            Dictionary<int, List<TimeWorkAccountYearEmployeeCalculation>> timeWorkAccountBasisByEmployee = new Dictionary<int, List<TimeWorkAccountYearEmployeeCalculation>>();
            List<TimeAccumulator> timeWorkAccountAccumulators = new List<TimeAccumulator>();
            if (calculateTimeWorkAccount)
            {
                var basis = TimeEngineManager(base.ActorCompanyId, base.UserId).CalculateTimeWorkAccountYearEmployeeBasis(employeeIds, dateFrom, dateTo);
                timeWorkAccountBasisByEmployee.AddRange(basis.GroupBy(b => b.EmployeeId).ToDictionary(k => k.Key, v => v.ToList()));
                timeWorkAccountAccumulators.AddRange(GetTimeAccumulators(base.ActorCompanyId, basis.GetTimeAccumulatorIds(), onlyActive: false));
            }

            foreach (Employee employee in employees)
            {
                employeeAccumulators.AddRange(CreateEmployeeAccumulatorsByAcc(employee, timeAccumulatorItemsByEmployee.GetList(employee.EmployeeId), timeWorkAccountBasisByEmployee.GetList(employee.EmployeeId)));
                employeeAccumulators.AddRange(CreateEmployeeAccumulatorsByVacation(employee, GetEmployeeVacation(employee.EmployeeId, compareModel, dateFrom, dateTo)));
            }

            IEnumerable<EmployeeAccumulatorDTO> CreateEmployeeAccumulatorsByAcc(Employee employee, List<TimeAccumulatorItem> accForEmployee, List<TimeWorkAccountYearEmployeeCalculation> basisForEmployee)
            {
                foreach (var acc in accForEmployee.OrderBy(i => i.Name))
                {
                    yield return CreateEmployeeAccumulatorByAcc(employee, acc, comparePeriodType, ownLimitMin, ownLimitMax);
                }
                foreach (var basis in basisForEmployee.OrderBy(i => i.TimeWorkAccount.Name))
                {
                    var acc = basis.HasTimeAccumulatorId ? accForEmployee.FirstOrDefault(a => a.TimeAccumulatorId == basis.TimeAccumulatorId) : null;
                    var timeAccumulator = basis.HasTimeAccumulatorId ? timeWorkAccountAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == basis.TimeAccumulatorId) : null;
                    yield return CreateEmployeeAccumulatorsByTimeWorkAccount(employee, basis, acc, timeAccumulator, TermGroup_AccumulatorTimePeriodType.Period, ownLimitMin, ownLimitMax);
                }
            }
            IEnumerable<EmployeeAccumulatorDTO> CreateEmployeeAccumulatorsByVacation(Employee employee, EmployeeVacationSE employeeVacation)
            {
                foreach (TermGroup_VacationBalance vacationBalance in vacationBalances)
                {
                    yield return CreateEmployeeAccumulatorByVacation(employee, employeeVacation, vacationBalance, comparePeriodType, ownLimitMin, ownLimitMax);
                }
            }

            return employeeAccumulators;
        }

        private EmployeeAccumulatorDTO CreateEmployeeAccumulatorByAcc(Employee employee, TimeAccumulatorItem acc, TermGroup_AccumulatorTimePeriodType comparePeriodType, int? ownLimitMinMinutes, int? ownLimitMaxMinutes)
        {
            decimal? amount = comparePeriodType == TermGroup_AccumulatorTimePeriodType.AccToday ? acc.SumAccTodayValue : acc.SumRangeValue;
            decimal compareValue = comparePeriodType == TermGroup_AccumulatorTimePeriodType.AccToday ? acc.SumAccToday : acc.SumRange;
            TimeAccumulatorRuleItem employeeGroupRule = acc.EmployeeGroupRules.FirstOrDefault(i => i.PeriodType == comparePeriodType) ?? new TimeAccumulatorRuleItem();
            TimeAccumulatorRuleItem ownRule = GetTimeAccumulatorPeriodRule(ownLimitMinMinutes, null, ownLimitMaxMinutes, null, comparePeriodType, compareValue);
            bool hasValidEmployeeGroupRule = employeeGroupRule.PeriodType == comparePeriodType;

            return new EmployeeAccumulatorDTO()
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.Name,
                EmployeeNr = employee.EmployeeNr,

                AccumulatorId = acc.TimeAccumulatorId,
                AccumulatorName = acc.Name,
                AccumulatorAccTodayValue = acc.SumAccToday,
                AccumulatorAccTodayDates = $"{acc.AccTodayStartDate.ToShortDateString()} - {acc.AccTodayStopDate.ToShortDateString()}",
                AccumulatorPeriodValue = acc.SumRange,
                AccumulatorPeriodDates = acc.TimePeriodDatesText,
                AccumulatorAmount = amount,
                AccumulatorRuleMinMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMinMinutes : null,
                AccumulatorRuleMinWarningMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMinMinutesWarning : null,
                AccumulatorRuleMaxMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMaxMinutes : null,
                AccumulatorRuleMaxWarningMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMaxMinutesWarning : null,
                AccumulatorDiff = (int)employeeGroupRule.DiffValue.TotalMinutes,
                AccumulatorStatus = employeeGroupRule.Comparison,
                AccumulatorStatusName = GetStatusName(employeeGroupRule, comparePeriodType),
                AccumulatorShowError = employeeGroupRule.ShowError,
                AccumulatorShowWarning = employeeGroupRule.ShowWarning,

                OwnLimitMin = ownLimitMinMinutes,
                OwnLimitMax = ownLimitMaxMinutes,
                OwnLimitDiff = (int)ownRule.DiffValue.TotalMinutes,
                OwnLimitStatus = ownRule.Comparison,
                OwnLimitStatusName = GetStatusName(ownRule, comparePeriodType, ownRule: true),
                OwnLimitShowError = ownRule.ShowError,
            };
        }

        private EmployeeAccumulatorDTO CreateEmployeeAccumulatorsByTimeWorkAccount(Employee employee, TimeWorkAccountYearEmployeeCalculation basis, TimeAccumulatorItem acc, TimeAccumulator timeAccumulator, TermGroup_AccumulatorTimePeriodType comparePeriodType, int? ownLimitMinMinutes, int? ownLimitMaxMinutes)
        {
            decimal? compareValue = acc?.SumRange;
            TimeAccumulatorRuleItem employeeGroupRule = acc?.EmployeeGroupRules.FirstOrDefault(i => i.PeriodType == comparePeriodType) ?? new TimeAccumulatorRuleItem();
            TimeAccumulatorRuleItem ownRule = GetTimeAccumulatorPeriodRule(ownLimitMinMinutes, null, ownLimitMaxMinutes, null, comparePeriodType, compareValue ?? 0);
            bool hasValidEmployeeGroupRule = acc != null && employeeGroupRule.PeriodType == comparePeriodType;
            
            StringBuilder name = new StringBuilder(GetText(92020, "ATK intjänat"));
            if (timeAccumulator != null)
                name.Append($" ({timeAccumulator.Name})");

            return new EmployeeAccumulatorDTO()
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.Name,
                EmployeeNr = employee.EmployeeNr,

                AccumulatorId = acc?.TimeAccumulatorId ?? 0,
                AccumulatorName = name.ToString(),
                AccumulatorAccTodayValue = 0,
                AccumulatorAccTodayDates = null,
                AccumulatorPeriodValue = basis.GetPaidLeaveMinutes(),
                AccumulatorPeriodDates = acc?.TimePeriodDatesText,
                AccumulatorAmount = basis.GetAmount(),
                AccumulatorRuleMinMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMinMinutes : null,
                AccumulatorRuleMinWarningMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMinMinutesWarning : null,
                AccumulatorRuleMaxMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMaxMinutes : null,
                AccumulatorRuleMaxWarningMinutes = hasValidEmployeeGroupRule ? acc.EmployeeGroupRuleMaxMinutesWarning : null,
                AccumulatorDiff = (int)employeeGroupRule.DiffValue.TotalMinutes,
                AccumulatorStatus = employeeGroupRule.Comparison,
                AccumulatorStatusName = GetStatusName(employeeGroupRule, comparePeriodType),
                AccumulatorShowError = employeeGroupRule.ShowError,
                AccumulatorShowWarning = employeeGroupRule.ShowWarning,

                OwnLimitMin = ownLimitMinMinutes,
                OwnLimitMax = ownLimitMaxMinutes,
                OwnLimitDiff = (int)ownRule.DiffValue.TotalMinutes,
                OwnLimitStatus = ownRule.Comparison,
                OwnLimitStatusName = GetStatusName(ownRule, comparePeriodType, ownRule: true),
                OwnLimitShowError = ownRule.ShowError,
            };
        }

        private EmployeeAccumulatorDTO CreateEmployeeAccumulatorByVacation(Employee employee, EmployeeVacationSE employeeVacation, TermGroup_VacationBalance vacationBalance, TermGroup_AccumulatorTimePeriodType comparePeriodType, int? ownLimitMin, int? ownLimitMax)
        {
            decimal? days = GetAccumulatorVacationBalanceValue(employeeVacation, vacationBalance);
            TimeAccumulatorRuleItem ownRule = GetTimeAccumulatorPeriodRule(ownLimitMin, null, ownLimitMax, null, comparePeriodType, days ?? 0);

            return new EmployeeAccumulatorDTO()
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.Name,
                EmployeeNr = employee.EmployeeNr,

                AccumulatorId = (int)vacationBalance,
                AccumulatorName = GetText((int)vacationBalance, TermGroup.VacationBalance),
                AccumulatorAccTodayValue = days ?? 0,
                AccumulatorAmount = null,
                AccumulatorRuleMinMinutes = null,
                AccumulatorRuleMinWarningMinutes = null,
                AccumulatorRuleMaxMinutes = null,
                AccumulatorRuleMaxWarningMinutes = null,
                AccumulatorDiff = null,
                AccumulatorStatus = SoeTimeAccumulatorComparison.OK,
                AccumulatorStatusName = string.Empty,
                AccumulatorShowError = false,
                AccumulatorShowWarning = false,

                OwnLimitMin = ownLimitMin,
                OwnLimitMax = ownLimitMax,
                OwnLimitDiff = (int)ownRule.DiffValue.TotalMinutes,
                OwnLimitStatus = ownRule.Comparison,
                OwnLimitStatusName = GetStatusName(ownRule, employeeVacation, comparePeriodType, ownRule: true),
                OwnLimitShowError = ownRule.ShowError,
            };
        }

        private TermGroup_AccumulatorTimePeriodType GetAccumulatorPeriodType(TermGroup_TimeAccumulatorCompareModel compareModel)
        {
            switch (compareModel)
            {
                case TermGroup_TimeAccumulatorCompareModel.AccToday:
                    return TermGroup_AccumulatorTimePeriodType.AccToday;
                case TermGroup_TimeAccumulatorCompareModel.SelectedRange:
                default:
                    return TermGroup_AccumulatorTimePeriodType.Range;
            }
        }

        private IEnumerable<TermGroup_VacationBalance> GetAccumulatorVacationBalances(List<int> accumulatorIds)
        {
            bool hasAccumulatorsIds = !accumulatorIds.IsNullOrEmpty();
            foreach (TermGroup_VacationBalance vacationBalance in GetAllAccumulatorVacationBalances())
            {
                if (!hasAccumulatorsIds || accumulatorIds.Contains((int)vacationBalance))
                    yield return vacationBalance;
            }
        }

        private EmployeeVacationSE GetEmployeeVacation(int employeeId, TermGroup_TimeAccumulatorCompareModel compareModel, DateTime dateFrom, DateTime dateTo)
        {
            if (compareModel == TermGroup_TimeAccumulatorCompareModel.SelectedRange)
                return null;

            List<EmployeeVacationSE> employeeVacationSEs = EmployeeManager.GetEmployeeVacationSEs(employeeId, onlyActive: false);
            return employeeVacationSEs.GetByAdjustmentDateOrCreated(dateFrom, dateTo);
        }

        private decimal? GetAccumulatorVacationBalanceValue(EmployeeVacationSE employeeVacation, TermGroup_VacationBalance vacationBalance)
        {
            if (employeeVacation == null)
                return null;

            decimal? value = null;
            switch (vacationBalance)
            {
                case TermGroup_VacationBalance.RemainingDays:
                    value = employeeVacation.SumRemainingDays();
                    break;
                case TermGroup_VacationBalance.RemainingDaysPaid:
                    value = employeeVacation.RemainingDaysPaid;
                    break;
                case TermGroup_VacationBalance.RemainingDaysUnpaid:
                    value = employeeVacation.RemainingDaysUnpaid;
                    break;
                case TermGroup_VacationBalance.RemainingDaysAdvance:
                    value = employeeVacation.RemainingDaysAdvance;
                    break;
                case TermGroup_VacationBalance.RemainingDaysYear1:
                    value = employeeVacation.RemainingDaysYear1;
                    break;
                case TermGroup_VacationBalance.RemainingDaysYear2:
                    value = employeeVacation.RemainingDaysYear2;
                    break;
                case TermGroup_VacationBalance.RemainingDaysYear3:
                    value = employeeVacation.RemainingDaysYear3;
                    break;
                case TermGroup_VacationBalance.RemainingDaysYear4:
                    value = employeeVacation.RemainingDaysYear4;
                    break;
                case TermGroup_VacationBalance.RemainingDaysYear5:
                    value = employeeVacation.RemainingDaysYear5;
                    break;
                case TermGroup_VacationBalance.RemainingDaysOverdue:
                    value = employeeVacation.RemainingDaysOverdue;
                    break;
            }
            return value;
        }

        private IEnumerable<TermGroup_VacationBalance> GetAllAccumulatorVacationBalances()
        {
            yield return TermGroup_VacationBalance.RemainingDays;
            yield return TermGroup_VacationBalance.RemainingDaysPaid;
            yield return TermGroup_VacationBalance.RemainingDaysUnpaid;
            yield return TermGroup_VacationBalance.RemainingDaysAdvance;
            yield return TermGroup_VacationBalance.RemainingDaysYear1;
            yield return TermGroup_VacationBalance.RemainingDaysYear2;
            yield return TermGroup_VacationBalance.RemainingDaysYear3;
            yield return TermGroup_VacationBalance.RemainingDaysYear4;
            yield return TermGroup_VacationBalance.RemainingDaysYear5;
            yield return TermGroup_VacationBalance.RemainingDaysOverdue;
        }

        private string GetStatusName(TimeAccumulatorRuleItem rule, EmployeeVacationSE employeeVacation, TermGroup_AccumulatorTimePeriodType comparePeriodType, bool ownRule = false)
        {
            if (employeeVacation == null)
                return GetText(91902, "Semester kan ej visas för vald period");

            return GetStatusName(rule, comparePeriodType, ownRule);
        }

        private string GetStatusName(TimeAccumulatorRuleItem rule, TermGroup_AccumulatorTimePeriodType comparePeriodType, bool ownRule = false)
        {
            switch (rule.Comparison)
            {
                case SoeTimeAccumulatorComparison.LessThanMin:
                    return ownRule ? GetText(12059, "Understiger gränsvärde: min saldo") : GetText(12060, "Understiger saldoregel: min saldo");
                case SoeTimeAccumulatorComparison.LessThanMinWarning:
                    return GetText(12061, "Understiger saldoregel: min saldo varning");
                case SoeTimeAccumulatorComparison.MoreThanMax:
                    return ownRule ? GetText(12062, "Överstiger gränsvärde: max saldo") : GetText(12063, "Överstiger saldoregel: max saldo");
                case SoeTimeAccumulatorComparison.MoreThanMaxWarning:
                    return GetText(12064, "Överstiger saldoregel: max saldo varning");
                default:
                    if (rule.NoRulesDefined && ownRule)
                        return GetText(12065, "Egna gränsvärden saknas");
                    else if (rule.NoRulesDefined)
                        return comparePeriodType == TermGroup_AccumulatorTimePeriodType.AccToday ? GetText(12066, "Saldoregel saknas för period löpande") : GetText(91901, "Saldoregel kan ej användas vid eget datumintervall");
                    else
                        return "OK";
            }
        }

        #endregion

        #region TimeAccumulator

        public List<TimeAccumulator> GetTimeAccumulators(int actorCompanyId, List<int> timeAccumulatorIds = null, bool onlyActive = true, bool loadEmployeeGroupRule = false, bool loadTimeCode = false, bool loadPayrollProduct = false, bool loadInvoiceProduct = false, bool onlyFinalSalary = false, bool showInTimeReports = false, bool timeWorkAccount = false, bool loadTimePeriodHead = false, bool loadTimeWorkReductionEarning = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAccumulator.NoTracking();
            return GetTimeAccumulators(entities, actorCompanyId, timeAccumulatorIds, onlyActive, loadEmployeeGroupRule, loadTimeCode, loadPayrollProduct, loadInvoiceProduct, onlyFinalSalary, showInTimeReports, timeWorkAccount, loadTimePeriodHead, loadTimeWorkReductionEarning);
        }

        public List<TimeAccumulator> GetTimeAccumulators(CompEntities entities, int actorCompanyId, List<int> timeAccumulatorIds = null, bool onlyActive = true, bool loadEmployeeGroupRule = false, bool loadTimeCode = false, bool loadPayrollProduct = false, bool loadInvoiceProduct = false, bool onlyFinalSalary = false, bool showInTimeReports = false, bool timeWorkAccount = false, bool loadTimePeriodHead = false, bool loadTimeWorkReductionEarning = false)
        {
            IQueryable<TimeAccumulator> query = (from ta in entities.TimeAccumulator
                                                 where ta.ActorCompanyId == actorCompanyId &&
                                                 (!showInTimeReports || ta.ShowInTimeReports)
                                                 orderby ta.Name
                                                 select ta);

            if (onlyActive)
                query = query.Where(a => a.State == (int)SoeEntityState.Active);
            else
                query = query.Where(a => a.State != (int)SoeEntityState.Deleted);
            
            if (timeWorkAccount)
                query = query.Where(a => a.UseTimeWorkAccount);
            if (timeAccumulatorIds != null)
                query = query.Where(i => timeAccumulatorIds.Contains(i.TimeAccumulatorId));
            if (loadEmployeeGroupRule)
                query = query.Include("TimeAccumulatorEmployeeGroupRule");
            if (loadPayrollProduct)
                query = query.Include("TimeAccumulatorPayrollProduct");
            if (loadInvoiceProduct)
                query = query.Include("TimeAccumulatorInvoiceProduct");
            if (loadTimeCode)
                query = query.Include("TimeAccumulatorTimeCode");
            if (loadTimePeriodHead)
                query = query.Include("TimePeriodHead");
            if (loadTimeWorkReductionEarning) { 
                query = query.Include("TimeWorkReductionEarning");
                query = query.Include("TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup");
            }

            List<TimeAccumulator> timeAccumulators = query.ToList();
            foreach (TimeAccumulator accumulator in timeAccumulators)
            {
                accumulator.TypeName = GetText(accumulator.Type, (int)TermGroup.TimeAccumulatorType);
            }

            //Post query filtering
            if (onlyFinalSalary)
                timeAccumulators = timeAccumulators.Where(i => i.FinalSalary && i.TimeCodeId.HasValue).ToList();

            return timeAccumulators;
        }

        public Dictionary<int, string> GetTimeAccumulatorsDict(int actorCompanyId, bool addEmptyRow, bool includeVacationBalance = false, bool includeWorkTimeAccountBalance = false, bool onlyTimeWorkAccountReduction = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<TimeAccumulator> timeAccumulators = GetTimeAccumulators(actorCompanyId);

            if(onlyTimeWorkAccountReduction)
                timeAccumulators = timeAccumulators.Where(ta => ta.UseTimeWorkReductionWithdrawal).ToList();

            foreach (TimeAccumulator timeAccumulator in timeAccumulators)
            {
                dict.Add(timeAccumulator.TimeAccumulatorId, timeAccumulator.Name);
            }
            if (includeWorkTimeAccountBalance && timeAccumulators.Any(ta => ta.UseTimeWorkAccount))
                dict.Add(Constants.TIMEACCUMULATOR_TIMEWORKACCOUNT, GetText(92020, "ATK intjänat"));
            dict = dict.Sort();

            if (includeVacationBalance)
                dict.AddRange(GetTermGroupContent(TermGroup.VacationBalance).ToDictionary().OrderByDescending(x => x.Key));

            return dict;
        }

        public TimeAccumulator GetTimeAccumulator(int actorCompanyId, int timeAccumulatorId, bool onlyActive = true, bool loadEmployeeGroups = false, bool loadTimeWorkReductionEarning = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAccumulator.NoTracking();
            return GetTimeAccumulator(entities, actorCompanyId, timeAccumulatorId, onlyActive, loadEmployeeGroups, loadTimeWorkReductionEarning);
        }

        public TimeAccumulator GetTimeAccumulator(CompEntities entities, int actorCompanyId, int timeAccumulatorId, bool onlyActive = true, bool loadEmployeeGroups = false, bool loadTimeWorkReductionEarning = false)
        {
            IQueryable<TimeAccumulator> query = (from ta in entities.TimeAccumulator
                                                 where ta.ActorCompanyId == actorCompanyId &&
                                                 ta.TimeAccumulatorId == timeAccumulatorId
                                                 select ta);

            if (onlyActive)
                query = query.Where(a => a.State == (int)SoeEntityState.Active);
            else
                query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

            query = query.Include("TimePeriodHead");
            query = query.Include("TimeAccumulatorInvoiceProduct");
            query = query.Include("TimeAccumulatorPayrollProduct");
            query = query.Include("TimeAccumulatorTimeCode");
            if (loadEmployeeGroups)
                query = query.Include("TimeAccumulatorEmployeeGroupRule.EmployeeGroup");

            TimeAccumulator timeAccumulator = query.FirstOrDefault();

            if (loadTimeWorkReductionEarning && timeAccumulator?.TimeWorkReductionEarningId != null)
            {
                var timeWorkReductionEarning = TimeWorkReductionManager.GetTimeWorkReductionEarning(entities, timeAccumulator.TimeWorkReductionEarningId.Value);
                if (timeWorkReductionEarning != null)
                    timeAccumulator.TimeWorkReductionEarning = timeWorkReductionEarning;
            } 

            return timeAccumulator;
        }
               
        public bool HasMaxTimeAccumulatorsInTimeReport(int actorCompanyId)
        {
            return GetTimeAccumulators(actorCompanyId, showInTimeReports: true).Count >= Constants.NOOFTIMEACCUMULATORS;
        }

        public ActionResult SaveTimeAccumulator(TimeAccumulatorDTO timeAccumulatorInput)
        {
            if (timeAccumulatorInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeAccumulator");

            ActionResult result = null;

            int timeAccumulatorId = timeAccumulatorInput.TimeAccumulatorId;
            List<Employee> employeesWithTimeWorkReductionTransactionsToRecalculate = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region TimeAccumulator

                        // Get existing
                        TimeAccumulator timeAccumulator = GetTimeAccumulator(entities, base.ActorCompanyId, timeAccumulatorId, onlyActive: false, loadEmployeeGroups: true, loadTimeWorkReductionEarning: true);
                        if (timeAccumulator == null)
                        {
                            #region Add

                            timeAccumulator = new TimeAccumulator()
                            {
                                ActorCompanyId = base.ActorCompanyId,
                            };
                            SetCreatedProperties(timeAccumulator);
                            entities.AddObject("TimeAccumulator", timeAccumulator);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(timeAccumulator);

                            #endregion
                        }

                        timeAccumulator.Name = timeAccumulatorInput.Name;
                        timeAccumulator.Description = timeAccumulatorInput.Description;
                        timeAccumulator.Type = (int)timeAccumulatorInput.Type;
                        timeAccumulator.ShowInTimeReports = timeAccumulatorInput.ShowInTimeReports;
                        timeAccumulator.FinalSalary = timeAccumulatorInput.FinalSalary;
                        timeAccumulator.TimePeriodHeadId = timeAccumulatorInput.TimePeriodHeadId.HasValue && timeAccumulatorInput.TimePeriodHeadId.Value > 0 ? timeAccumulatorInput.TimePeriodHeadId.Value : (int?)null;
                        timeAccumulator.TimeCodeId = timeAccumulatorInput.TimeCodeId.HasValue && timeAccumulatorInput.TimeCodeId.Value > 0 ? timeAccumulatorInput.TimeCodeId.Value : (int?)null;
                        timeAccumulator.State = (int)timeAccumulatorInput.State;
                        timeAccumulator.UseTimeWorkAccount = timeAccumulatorInput.UseTimeWorkAccount;
                        timeAccumulator.UseTimeWorkReductionWithdrawal = timeAccumulatorInput.UseTimeWorkReductionWithdrawal;
                        
                        result = SaveTimeAccumulatorTimeCodes(entities, timeAccumulator, timeAccumulatorInput);
                        if (!result.Success)
                            return result;

                        result = SaveTimeAccumulatorPayrollProducts(entities, timeAccumulator, timeAccumulatorInput);
                        if (!result.Success)
                            return result;

                        result = SaveTimeAccumulatorInvoiceProducts(entities, timeAccumulator, timeAccumulatorInput);
                        if (!result.Success)
                            return result;
                        
                        result = SaveTimeAccumulatorEmployeeGroupRules(entities, timeAccumulator, timeAccumulatorInput);
                        if (!result.Success)
                            return result;

                        result = TimeWorkReductionManager.SaveTimeAccumulatorTimeWorkReductionEarnings(entities, timeAccumulator, timeAccumulatorInput, out employeesWithTimeWorkReductionTransactionsToRecalculate);
                        if (!result.Success)
                            return result;

                        if (timeAccumulatorInput.TimeWorkReductionEarning != null && TimeWorkReductionManager.HasOverlappingTimeWorkReductionEarningEmployeeGroups(entities, timeAccumulatorInput, out string message))
                            return new ActionResult((int)ActionResultSave.NothingSaved, message);

                        if (timeAccumulatorInput.TimeWorkReductionEarning == null)
                            timeAccumulator.TimeWorkReductionEarningId = null;
                        
                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            timeAccumulatorId = timeAccumulator.TimeAccumulatorId;
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
                        result.IntegerValue = timeAccumulatorId;
                        
                        if (!employeesWithTimeWorkReductionTransactionsToRecalculate.IsNullOrEmpty())
                        {
                            result.InfoMessage = string.Format(GetText(110688, "Det finns {0} anställda som har ATF-transaktioner under den förkortade perioden. Dessa behöver räknas om i attestera tid"), employeesWithTimeWorkReductionTransactionsToRecalculate.Count);
                            result.Strings = employeesWithTimeWorkReductionTransactionsToRecalculate.Select(e => e.EmployeeNrAndName).ToList();
                        }
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private ActionResult SaveTimeAccumulatorInvoiceProducts(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput)
        {
            if (timeAccumulatorInput.InvoiceProducts != null && timeAccumulatorInput.InvoiceProducts.Any(p => p.InvoiceProductId <= 0))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(110691, "Artikel måste anges"));

            var existingInvoiceProducts = timeAccumulator.TimeAccumulatorInvoiceProduct?.ToList() ?? new List<TimeAccumulatorInvoiceProduct>();

            #region Delete

            foreach (var existingInvoiceProduct in existingInvoiceProducts)
            {
                if (!timeAccumulatorInput.InvoiceProducts.Any(p => p.InvoiceProductId == existingInvoiceProduct.InvoiceProductId))
                    DeleteEntityItem(entities, existingInvoiceProduct);
            }

            #endregion

            #region Add/update

            if (timeAccumulatorInput.InvoiceProducts != null)
            {
                foreach (var invoiceProductInput in timeAccumulatorInput.InvoiceProducts)
                {
                    var invoiceProduct = ProductManager.GetInvoiceProduct(entities, invoiceProductInput.InvoiceProductId);

                    var timeAccumulatorInvoiceProduct = timeAccumulator.TimeAccumulatorInvoiceProduct.FirstOrDefault(p => p.InvoiceProductId == invoiceProductInput.InvoiceProductId);
                    if (timeAccumulatorInvoiceProduct == null)
                    {
                        timeAccumulatorInvoiceProduct = TimeAccumulatorInvoiceProduct.Create(timeAccumulator, invoiceProduct, invoiceProductInput.Factor);
                        if (timeAccumulatorInvoiceProduct == null)
                            continue;

                        entities.TimeAccumulatorInvoiceProduct.AddObject(timeAccumulatorInvoiceProduct);
                    }
                    else if (
                        invoiceProduct.ProductId != timeAccumulatorInvoiceProduct.InvoiceProductId ||
                        timeAccumulatorInvoiceProduct.Factor != invoiceProductInput.Factor)
                    {
                        timeAccumulatorInvoiceProduct.Update(invoiceProduct, invoiceProductInput.Factor);
                    }
                }
            }

            #endregion

            return new ActionResult(true);
        }

        private ActionResult SaveTimeAccumulatorPayrollProducts(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput)
        {
            if (timeAccumulatorInput.PayrollProducts != null && timeAccumulatorInput.PayrollProducts.Any(p => p.PayrollProductId <= 0))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(110690, "Löneart måste anges"));

            var existingPayrollProducts = timeAccumulator.TimeAccumulatorPayrollProduct?.ToList() ?? new List<TimeAccumulatorPayrollProduct>();

            #region Delete

            foreach (var existingPayrollProduct in existingPayrollProducts)
            {
                if (!timeAccumulatorInput.PayrollProducts.Any(p => p.PayrollProductId == existingPayrollProduct.PayrollProductId))
                    DeleteEntityItem(entities, existingPayrollProduct);
            }

            #endregion

            #region Add/update

            if (timeAccumulatorInput.PayrollProducts != null)
            {
                foreach (var payrollProductInput in timeAccumulatorInput.PayrollProducts)
                {
                    var payrollProduct = ProductManager.GetPayrollProduct(entities, payrollProductInput.PayrollProductId);

                    var timeAccumulatorPayrollProduct = timeAccumulator.TimeAccumulatorPayrollProduct.FirstOrDefault(p => p.PayrollProductId == payrollProductInput.PayrollProductId);
                    if (timeAccumulatorPayrollProduct == null)
                    {
                        timeAccumulatorPayrollProduct = TimeAccumulatorPayrollProduct.Create(timeAccumulator, payrollProduct, payrollProductInput.Factor);
                        if (timeAccumulatorPayrollProduct == null)
                            continue;

                        entities.TimeAccumulatorPayrollProduct.AddObject(timeAccumulatorPayrollProduct);
                    }
                    else if (
                        payrollProduct.ProductId != timeAccumulatorPayrollProduct.PayrollProductId ||
                        timeAccumulatorPayrollProduct.Factor != payrollProductInput.Factor)
                    {
                        timeAccumulatorPayrollProduct.Update(payrollProduct, payrollProductInput.Factor);
                    }
                }
            }

            #endregion

            return new ActionResult(true);
        }

        private ActionResult SaveTimeAccumulatorTimeCodes(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput)
        {
            if (timeAccumulatorInput.TimeCodes != null && timeAccumulatorInput.TimeCodes.Any(p => p.TimeCodeId <= 0))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(110690, "Tidkod måste anges"));

            var existingTimeCodes = timeAccumulator.TimeAccumulatorTimeCode?.ToList() ?? new List<TimeAccumulatorTimeCode>();

            #region Delete

            foreach (var existingTimeCode in existingTimeCodes)
            {
                if (!timeAccumulatorInput.TimeCodes.Any(t => t.TimeCodeId == existingTimeCode.TimeCodeId))
                    DeleteEntityItem(entities, existingTimeCode);
            }

            #endregion

            #region Add/update

            if (timeAccumulatorInput.TimeCodes != null)
            {
                foreach (var timeCodeInput in timeAccumulatorInput.TimeCodes)
                {
                    var timeCode = TimeCodeManager.GetTimeCode(entities, timeCodeInput.TimeCodeId, base.ActorCompanyId, onlyActive: false);

                    var timeAccumulatorTimeCode = timeAccumulator.TimeAccumulatorTimeCode.FirstOrDefault(t => t.TimeCodeId == timeCodeInput.TimeCodeId);
                    if (timeAccumulatorTimeCode == null)
                    {
                        timeAccumulatorTimeCode = TimeAccumulatorTimeCode.Create(timeAccumulator, timeCode, timeCodeInput.Factor, timeCodeInput.ImportDefault, timeCodeInput.IsHeadTimeCode);
                        if (timeAccumulatorTimeCode == null)
                            continue;

                        entities.TimeAccumulatorTimeCode.AddObject(timeAccumulatorTimeCode);
                    }
                    else if (
                        timeCode.TimeCodeId != timeAccumulatorTimeCode.TimeCodeId ||
                        timeAccumulatorTimeCode.Factor != timeCodeInput.Factor || 
                        timeAccumulatorTimeCode.ImportDefault != timeCodeInput.ImportDefault ||
                        timeAccumulatorTimeCode.IsHeadTimeCode != timeCodeInput.IsHeadTimeCode
                        )
                    {
                        timeAccumulatorTimeCode.Update(timeCode, timeCodeInput.Factor, timeCodeInput.ImportDefault, timeCodeInput.IsHeadTimeCode);
                    }
                }
            }

            #endregion

            return new ActionResult(true);
        }

        private ActionResult SaveTimeAccumulatorEmployeeGroupRules(CompEntities entities, TimeAccumulator timeAccumulator, TimeAccumulatorDTO timeAccumulatorInput)
        {
            ActionResult result = new ActionResult(true);

            List<TimeAccumulatorEmployeeGroupRule> existingRules = (timeAccumulator.TimeAccumulatorEmployeeGroupRule != null ? timeAccumulator.TimeAccumulatorEmployeeGroupRule.Where(r => r.State == (int)SoeEntityState.Active).ToList() : new List<TimeAccumulatorEmployeeGroupRule>());

            #region Delete

            foreach (TimeAccumulatorEmployeeGroupRule existingRule in existingRules)
            {
                if (!timeAccumulatorInput.EmployeeGroupRules.Any(r => r.EmployeeGroupId == existingRule.EmployeeGroupId))
                {
                    result = ChangeEntityState(entities, existingRule, SoeEntityState.Deleted, false);
                    if (!result.Success)
                        return result;
                }
                    
            }

            #endregion

            #region Add/update

            if (timeAccumulatorInput.EmployeeGroupRules != null)
            {
                List<int> updatedIds = new List<int>();
                foreach (TimeAccumulatorEmployeeGroupRuleDTO ruleInput in timeAccumulatorInput.EmployeeGroupRules)
                {
                    TimeAccumulatorEmployeeGroupRule existingRule = timeAccumulator.TimeAccumulatorEmployeeGroupRule.FirstOrDefault(r => r.State == (int)SoeEntityState.Active && r.EmployeeGroupId == ruleInput.EmployeeGroupId && !updatedIds.Contains(r.TimeAccumulatorEmployeeGroupRuleId));
                    if (existingRule == null)
                    {
                        existingRule = new TimeAccumulatorEmployeeGroupRule()
                        {
                            EmployeeGroupId = ruleInput.EmployeeGroupId,
                        };
                        SetCreatedProperties(existingRule);
                        if (timeAccumulator.TimeAccumulatorEmployeeGroupRule == null)
                            timeAccumulator.TimeAccumulatorEmployeeGroupRule = new EntityCollection<TimeAccumulatorEmployeeGroupRule>();
                        timeAccumulator.TimeAccumulatorEmployeeGroupRule.Add(existingRule);
                    }
                    else
                    {
                        SetModifiedProperties(existingRule);
                        updatedIds.Add(existingRule.TimeAccumulatorEmployeeGroupRuleId);
                    }

                    existingRule.Type = (int)ruleInput.Type;
                    existingRule.MinMinutes = ruleInput.MinMinutes;
                    existingRule.MinTimeCodeId = ruleInput.MinTimeCodeId;
                    existingRule.MaxMinutes = ruleInput.MaxMinutes;
                    existingRule.MaxTimeCodeId = ruleInput.MaxTimeCodeId;
                    existingRule.ShowOnPayrollSlip = ruleInput.ShowOnPayrollSlip;
                    existingRule.MinMinutesWarning = ruleInput.MinMinutesWarning;
                    existingRule.MaxMinutesWarning = ruleInput.MaxMinutesWarning;
                    existingRule.ScheduledJobHeadId = ruleInput.ScheduledJobHeadId.HasValue && ruleInput.ScheduledJobHeadId.Value != 0 ? ruleInput.ScheduledJobHeadId.Value : (int?)null;
                    existingRule.ThresholdMinutes = ruleInput.ThresholdMinutes;
                }
            }

            #endregion

            return result;
        }

        public ActionResult DeleteTimeAccumulator(int actorCompanyId, int timeAccumulatorId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeAccumulator timeAccumulator = GetTimeAccumulator(entities, actorCompanyId, timeAccumulatorId, loadTimeWorkReductionEarning: true);
                if (timeAccumulator == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeAccumulator");

                if(timeAccumulator.TimeWorkReductionEarningReference != null)
                {
                    List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup> existingEarningEmployeeGroups = (timeAccumulator.TimeWorkReductionEarning?.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup != null ? timeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Where(r => r.State == (int)SoeEntityState.Active).ToList() : new List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup>());
                    foreach (TimeAccumulatorTimeWorkReductionEarningEmployeeGroup existingEarningEmployeeGroup in existingEarningEmployeeGroups)
                         ChangeEntityState(entities, existingEarningEmployeeGroup, SoeEntityState.Deleted, false);
    
                    TimeWorkReductionEarning existingTimeWorkReductionEarning = timeAccumulator.TimeWorkReductionEarning != null && timeAccumulator.TimeWorkReductionEarning.State == (int)SoeEntityState.Active ? timeAccumulator.TimeWorkReductionEarning : null;
                    if(existingTimeWorkReductionEarning != null)
                        ChangeEntityState(entities, existingTimeWorkReductionEarning, SoeEntityState.Deleted, true);
                }

                return ChangeEntityState(entities, timeAccumulator, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region TimeAccumulatorEmployeeGroupRule

        public List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRulesForCompany(int actorCompanyId, int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeAccumulatorEmployeeGroupRulesForCompany(entities, actorCompanyId, scheduledJobHeadId);
        }

        public List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRulesForCompany(CompEntities entities, int actorCompanyId, int scheduledJobHeadId)
        {
            return (from r in entities.TimeAccumulatorEmployeeGroupRule
                    where r.TimeAccumulator.ActorCompanyId == actorCompanyId &&
                    r.ScheduledJobHeadId == scheduledJobHeadId &&
                    r.State == (int)SoeEntityState.Active
                    select r).ToList();
        }

        public List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRulesForCompany(CompEntities entities, int actorCompanyId, List<int> accumulatorIds = null, bool loadTimeAccumulator = false, bool loadTimeAccumulatorRelations = false)
        {
            var query = (from r in entities.TimeAccumulatorEmployeeGroupRule
                         where r.State == (int)SoeEntityState.Active &&
                         r.TimeAccumulator.State == (int)SoeEntityState.Active &&
                         r.TimeAccumulator.ActorCompanyId == actorCompanyId
                         select r);

            if (loadTimeAccumulatorRelations)
            {
                query = query.Include("EmployeeGroup");
                query = query.Include("TimeAccumulator.TimeAccumulatorPayrollProduct.PayrollProduct");
                query = query.Include("TimeAccumulator.TimeAccumulatorInvoiceProduct.InvoiceProduct");
                query = query.Include("TimeAccumulator.TimeAccumulatorTimeCode.TimeCode");
            }
            else if (loadTimeAccumulator)
            {
                query = query.Include("TimeAccumulator");
            }

            if (!accumulatorIds.IsNullOrEmpty())
                query = query.Where(r => accumulatorIds.Contains(r.TimeAccumulator.TimeAccumulatorId));

            return query.ToList();
        }

        public List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRules(int employeeGroupId, bool loadOnlyActive = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAccumulatorEmployeeGroupRule.NoTracking();
            return GetTimeAccumulatorEmployeeGroupRules(entities, employeeGroupId, loadOnlyActive);
        }

        public List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRules(CompEntities entities, int employeeGroupId, bool loadOnlyActive = true)
        {
            return (from r in entities.TimeAccumulatorEmployeeGroupRule
                    .Include("TimeAccumulator")
                    where r.EmployeeGroupId == employeeGroupId &&
                    (!loadOnlyActive || r.State == (int)SoeEntityState.Active)
                    select r).ToList();
        }

        public TimeAccumulatorEmployeeGroupRule GetEmployeeGroupAccumulatorSetting(CompEntities entities, int timeAccumulatorId, int employeeGroupId, bool loadOnlyActive = true)
        {
            return (from r in entities.TimeAccumulatorEmployeeGroupRule
                    where r.TimeAccumulatorId == timeAccumulatorId &&
                    r.EmployeeGroupId == employeeGroupId &&
                    (!loadOnlyActive || r.State == (int)SoeEntityState.Active)
                    select r).FirstOrDefault();
        }

        public ActionResult SaveEmployeeGroupAccumulatorSettings(List<AccumulatorSaveItem> dtos, int employeeGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (var item in dtos)
                {
                    TimeAccumulatorEmployeeGroupRule rule = GetEmployeeGroupAccumulatorSetting(entities, item.TimeAccumulatorId, employeeGroupId);
                    if (rule == null)
                        continue;

                    bool hasChanged = item.Type != rule.Type ||
                                      item.MinMinutes != rule.MinMinutes ||
                                      item.MaxMinutes != rule.MaxMinutes ||
                                      item.MinTimeCodeId != rule.MinTimeCodeId ||
                                      item.MaxTimeCodeId != rule.MaxTimeCodeId ||
                                      item.ShowOnPayrollSlip != rule.ShowOnPayrollSlip;

                    if (!hasChanged)
                        continue;

                    //Update
                    rule.Type = item.Type;
                    rule.MinMinutes = item.MinMinutes;
                    rule.MinTimeCodeId = item.MinTimeCodeId.HasValue && item.MinTimeCodeId.Value > 0 ? item.MinTimeCodeId.Value : (int?)null;
                    rule.MaxMinutes = item.MaxMinutes;
                    rule.MaxTimeCodeId = item.MaxTimeCodeId.HasValue && item.MaxTimeCodeId.Value > 0 ? item.MaxTimeCodeId.Value : (int?)null;
                    rule.ShowOnPayrollSlip = item.ShowOnPayrollSlip;

                    SetModifiedProperties(rule);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region TimeAccumulators for reports

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForTimeAccumulatorReport(CompEntities entities, DateTime dateTo, int employeeId, int timeCodeId)
        {
            return (from tct in entities.TimeCodeTransaction
                         .Include("TimeBlock")
                         .Include("TimeBlock.TimeBlockDate")
                         .Include("TimeCode")
                    where tct.TimeBlock.State == (int)SoeEntityState.Active &&
                    tct.TimeCode.TimeCodeId == timeCodeId &&
                    tct.TimeBlock.EmployeeId == employeeId &&
                    tct.TimeBlock.TimeBlockDate.Date <= dateTo &&
                    tct.Type == (int)TimeCodeTransactionType.Time
                    select tct).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForTimeAccumulatorReport(CompEntities entities, DateTime dateTo, int employeeId, int payrollProductId)
        {
            return (from tpt in entities.TimePayrollTransaction
                         .Include("TimeBlockDate")
                         .Include("PayrollProduct")
                    where tpt.State == (int)SoeEntityState.Active &&
                    tpt.PayrollProduct.ProductId == payrollProductId &&
                    tpt.EmployeeId == employeeId &&
                    tpt.TimeBlockDate.Date <= dateTo
                    select tpt).ToList();
        }

        public List<TimeInvoiceTransaction> GetTimeInvoiceTransactionsForTimeAccumulatorReport(CompEntities entities, DateTime dateTo, int employeeId, int invoiceProductId)
        {
            return (from tit in entities.TimeInvoiceTransaction
                        .Include("TimeBlockDate")
                        .Include("InvoiceProduct")
                    where tit.State == (int)SoeEntityState.Active &&
                    tit.InvoiceProduct.ProductId == invoiceProductId &&
                    tit.Employee.EmployeeId == employeeId &&
                    tit.TimeBlockDate.Date <= dateTo
                    select tit).ToList();
        }

        #endregion

        #region TimeAccumulatorItem

        public Dictionary<int, List<TimeAccumulatorItem>> GetTimeAccumulatorItemsByEmployee(GetTimeAccumulatorItemsInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeAccumulatorItemsByEmployee(entities, input);
        }

        public Dictionary<int, List<TimeAccumulatorItem>> GetTimeAccumulatorItemsByEmployee(CompEntities entities, GetTimeAccumulatorItemsInput input)
        {
            Dictionary<int, List<TimeAccumulatorItem>> itemsByEmployee = new Dictionary<int, List<TimeAccumulatorItem>>();

            if (input.Employees == null)
                return itemsByEmployee;

            List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, input.ActorCompanyId);
            List<TimeAccumulator> timeAccumulators = GetTimeAccumulators(entities, input.ActorCompanyId, timeAccumulatorIds: input.TimeAccumulatorIds, loadEmployeeGroupRule: true);
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(input.StartDate, TermGroup_TimePeriodType.Payroll, input.ActorCompanyId);

            foreach (Employee employee in input.Employees)
            {
                var items = GetTimeAccumulatorItems(entities, GetTimeAccumulatorItemsInput.CreateInput(input, employee.EmployeeId), timeAccumulators, employee, employeeGroups, timePeriod);
                if (!itemsByEmployee.ContainsKey(employee.EmployeeId))
                    itemsByEmployee.Add(employee.EmployeeId, items);
            }

            return itemsByEmployee;
        }

        public List<TimeAccumulatorItem> GetTimeAccumulatorItems(GetTimeAccumulatorItemsInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeAccumulatorItems(entities, input);
        }

        public List<TimeAccumulatorItem> GetTimeAccumulatorItems(CompEntities entities, GetTimeAccumulatorItemsInput input)
        {
            List<TimeAccumulator> timeAccumulators = GetTimeAccumulators(input.ActorCompanyId, loadEmployeeGroupRule: true);
            Employee employee = EmployeeManager.GetEmployeeWithEmploymentAndEmploymentChangeBatch(input.EmployeeId, input.ActorCompanyId);
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(input.StartDate, TermGroup_TimePeriodType.Payroll, input.ActorCompanyId);

            return GetTimeAccumulatorItems(entities, input, timeAccumulators, employee, employeeGroups, timePeriod);
        }

        public List<TimeAccumulatorItem> GetTimeAccumulatorItems(
            CompEntities entities, 
            GetTimeAccumulatorItemsInput input, 
            List<TimeAccumulator> timeAccumulators, 
            Employee employee, 
            List<EmployeeGroup> employeeGroups,
            TimePeriod timePeriod = null, 
            bool useLastEmploymentIfNotExists = false
            )
        {
            List<TimeAccumulatorItem> items = new List<TimeAccumulatorItem>();

            if (input == null || employee == null || timeAccumulators.IsNullOrEmpty())
                return items;

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(input.StartDate, input.StopDate, employeeGroups);
            if (employeeGroup == null && useLastEmploymentIfNotExists)
                employeeGroup = employee.GetLastEmployment().GetEmployeeGroup();
            if (employeeGroup == null)
                return items;

            List<TimeAccumulatorBalance> timeAccumulatorBalancesForEmployee = GetTimeAccumulatorBalancesForEmployee(entities, employee.ActorCompanyId, employee.EmployeeId);
            int? clockRounding = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningClockRounding, 0, employee.ActorCompanyId, 0);

            foreach (TimeAccumulator timeAccumulator in timeAccumulators)
            {
                if (timeAccumulator.TimeWorkReductionEarningId.HasValue)
                    continue; //Never show in GUI

                TimeAccumulatorEmployeeGroupRule employeeGroupRule = timeAccumulator.TimeAccumulatorEmployeeGroupRule?.FirstOrDefault(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId && i.State == (int)SoeEntityState.Active);
                if (employeeGroupRule == null)
                    continue;

                TimeAccumulatorItem item = GetTimeAccumulatorItem(entities, input, timeAccumulator, employee, employeeGroupRule, timePeriod, timeAccumulatorBalancesForEmployee, clockRounding);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        public TimeAccumulatorItem GetTimeAccumulatorItem(
            CompEntities entities, 
            GetTimeAccumulatorItemsInput input, 
            TimeAccumulator timeAccumulator, 
            Employee employee, 
            TimeAccumulatorEmployeeGroupRule rule = null, 
            TimePeriod timePeriod = null, 
            List<TimeAccumulatorBalance> timeAccumulatorBalances = null, 
            int? clockRounding = null
            )
        {
            if (input == null || !input.CalculateAny() || timeAccumulator == null || employee == null)
                return null;

            TimePeriodHeadDTO timeAccumulatorTimePeriodHead = null;
            if (timeAccumulator.TimePeriodHeadId.HasValue)
            {
                timeAccumulatorTimePeriodHead = cachedTimePeriodHeads.FirstOrDefault(i => i.TimePeriodHeadId == timeAccumulator.TimePeriodHeadId.Value && i.ActorCompanyId == employee.ActorCompanyId);
                if (timeAccumulatorTimePeriodHead == null)
                {
                    timeAccumulatorTimePeriodHead = TimePeriodManager.GetTimePeriodHead(entities, timeAccumulator.TimePeriodHeadId.Value, employee.ActorCompanyId, loadPeriods: true).ToDTO(true);
                    if (timeAccumulatorTimePeriodHead != null)
                        cachedTimePeriodHeads.Add(timeAccumulatorTimePeriodHead);
                }
            }

            TimeAccumulatorItem item = new TimeAccumulatorItem(
                timeAccumulator.ToDTO(),
                timePeriod?.ToDTO(),
                timeAccumulatorTimePeriodHead,
                employee.EmployeeId,
                input.StartDate,
                input.StopDate,
                input.CalculateDay,
                input.CalculatePeriod,
                input.CalculatePlanningPeriod,
                input.CalculateYear,
                input.CalculateAccToday,
                input.CalculateRange,
                input.IncludeBalanceYear);

            CalculateTimeAccumulatorBalance(entities, item, employee.EmployeeId, timeAccumulatorBalances);
            CalculateTimeAccumulatorYear(entities, item, (input.OverrideDateOnYear ? input.StopDate : (DateTime?)null));
            CalculateTimeAccumulatorAccumulated(entities, item, employee, input.CalculateAccumulatedValue);
            if (!CalculateTimeAccumulatorPlanningPeriod(entities, item, out TimePeriodDTO currentPlanningTimePeriod) && input.CalculateOnlyPlanningPeriod)
                return null;// If only calculating planning period and this accumulator should not be calculated, return null to prevent it from being added to the collection.
            CalculateTimeAccumulatorPeriod(entities, item);
            CalculateTimeAccumulatorRange(entities, item, employee, input.CalculateRangeValue);
            CalculateTimeAccumulatorDay(entities, item);

            if (input.AddSourceIds)
                AddTimeAccumultorSourceIds(entities, item, timeAccumulator.TimeAccumulatorId);
            AddTimeAccumulatorEmployeeGroupRules(entities, item, rule, currentPlanningTimePeriod, employee, clockRounding);

            return item;
        }

        public TimeAccumulatorItem GetBreakTimeAccumulatorItem(int actorCompanyId, DateTime time, int employeeId, int employeeGroupId)
        {
            int scheduledBreakMinutes = 0;
            int actualBreakMinutes = 0;

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check employee group setting when to break the day
                int minutesAfterMidnight = 0;
                List<EmployeeGroup> employeeGroups = base.GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));
                EmployeeGroup employeeGroup = employeeGroups.FirstOrDefault(f => f.EmployeeGroupId == employeeGroupId);
                if (employeeGroup != null)
                    minutesAfterMidnight = employeeGroup.BreakDayMinutesAfterMidnight;

                DateTime date = time.AddMinutes(-minutesAfterMidnight).Date;
                DateTime? lastIn = null;

                #endregion

                #region Scheduled breaks

                List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                  where tb.EmployeeId == employeeId &&
                                                                  tb.Date.HasValue && tb.Date.Value == date.Date &&
                                                                  tb.State == (int)SoeEntityState.Active
                                                                  select tb).ToList();

                templateBlocks = templateBlocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();
                DateTime scheduleOut = templateBlocks.GetScheduleOut(actualDate: true);
                if (scheduleOut < time)
                    time = scheduleOut;

                // Calculate total breaks in minutes
                foreach (TimeScheduleTemplateBlock templateBreakBlock in templateBlocks.Where(b => b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak).ToList())
                {
                    scheduledBreakMinutes += (int)(templateBreakBlock.StopTime - templateBreakBlock.StartTime).TotalMinutes;
                }

                #endregion

                #region Actual breaks

                TimeBlockDateDTO timeBlockDate = TimeBlockManager.GetTimeBlockDateFromCache(entities, actorCompanyId, employeeId, date);
                if (timeBlockDate != null)
                {
                    // Get all entries for specified employee and specified date
                    List<TimeStampEntry> entries = (from e in entities.TimeStampEntry.Include("TimeDeviationCause")
                                                    where e.EmployeeId == employeeId &&
                                                    e.TimeBlockDateId == timeBlockDate.TimeBlockDateId &&
                                                    e.State == (int)SoeEntityState.Active
                                                    orderby e.Time
                                                    select e).ToList();

                    // Loop through entries and calculate length of holes between out and in stamps
                    TimeStampEntry prevEntry = null;
                    foreach (TimeStampEntry entry in entries)
                    {
                        if (entry.Type == (int)TermGroup_TimeStampEntryType.Out)
                        {
                            prevEntry = entry;
                            lastIn = null;
                            continue;
                        }

                        if (prevEntry != null)
                        {
                            // If last stamp was in, store time to be used when calculating gaps below
                            lastIn = entry.Time;
                            actualBreakMinutes += (int)(entry.Time - prevEntry.Time).TotalMinutes;
                            prevEntry = null;
                        }
                    }

                    if (prevEntry != null && prevEntry.Type == (int)TimeStampEntryType.Out)
                    {
                        // If last stamp was out, sum time from out to now
                        TimeDeviationCause timeDeviationCause = null;
                        if (prevEntry.TimeDeviationCauseId.HasValue && prevEntry.TimeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                        {
                            List<TimeDeviationCause> timeDeviationCauses = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(actorCompanyId));
                            timeDeviationCause = timeDeviationCauses.FirstOrDefault(f => f.TimeDeviationCauseId == prevEntry.TimeDeviationCauseId.Value);
                        }

                        // If last stamp is absence do not calculate the this part as break
                        if (timeDeviationCause == null || prevEntry.IsBreak)
                        {
                            int diff = (int)(time - prevEntry.Time).TotalMinutes;
                            if (diff > 0)
                                actualBreakMinutes += diff;
                        }
                    }

                    #region Gaps in schedule

                    if (actualBreakMinutes > 0)
                    {
                        int gapMinutes = 0;
                        List<BreakDTO> gaps = templateBlocks.GetGaps(true);
                        if (gaps.Any())
                        {
                            // If last entry was in, compare to that time instead of now
                            if (lastIn.HasValue)
                                time = lastIn.Value;

                            foreach (BreakDTO gap in gaps)
                            {
                                if (gap.StopTime <= time)
                                {
                                    // The whole gap has passed
                                    gapMinutes += gap.BreakMinutes;
                                }
                                else if (gap.StartTime < time)
                                {
                                    // Within a gap
                                    gapMinutes += (int)(time - gap.StartTime).TotalMinutes;
                                }
                            }

                            actualBreakMinutes -= gapMinutes;
                        }
                    }

                    #endregion
                }

                #endregion      
            }

            if (actualBreakMinutes < 0)
                actualBreakMinutes = 0;

            TimeAccumulatorItem item = new TimeAccumulatorItem()
            {
                TimeAccumulatorId = 0,
                Name = string.Format("{0}: ", (actualBreakMinutes > scheduledBreakMinutes ? GetText(3573, "För mycket rast med") : GetText(3774, "Rast kvar idag"))),
                SumPeriod = Math.Abs(scheduledBreakMinutes - actualBreakMinutes),
                SumToday = scheduledBreakMinutes,
                SumAccToday = actualBreakMinutes
            };

            return item;
        }

        public decimal GetTimeAccumulatorValue(CompEntities entities, int actorCompanyId, Employee employee, DateTime startDate, DateTime stopDate, decimal sum, TimeAccumulator timeAccumulator)
        {
            return GetTimeAccumulatorValue(entities, actorCompanyId, employee, startDate, stopDate, sum, timeAccumulator?.TimeCodeId);
        }

        public decimal GetTimeAccumulatorValue(CompEntities entities, int actorCompanyId, Employee employee, DateTime startDate, DateTime stopDate, decimal sum, int? timeCodeId)
        {
            decimal value = 0;

            if (!timeCodeId.HasValue)
                return value;

            Employment employment = employee?.GetEmployment(startDate, stopDate, forward: false);
            if (employment == null)
                return value;

            TimeCode timeCode = cachedTimeCodesWithProducts.FirstOrDefault(tc => tc.TimeCodeId == timeCodeId.Value);
            if (timeCode == null)
            {
                timeCode = TimeCodeManager.GetTimeCodeWithPayrollProducts(entities, timeCodeId.Value, actorCompanyId);
                if (timeCode != null)
                    cachedTimeCodesWithProducts.Add(timeCode);
            }

            if (timeCode != null && !timeCode.TimeCodePayrollProduct.IsNullOrEmpty())
            {
                foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCode.TimeCodePayrollProduct)
                {
                    var formulaResult = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, timeCodePayrollProduct.PayrollProduct, stopDate);
                    if (formulaResult == null)
                        continue;

                    decimal formulaAmount = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                    decimal quantity = sum * timeCodePayrollProduct.Factor;
                    decimal quantityHours = Math.Abs(quantity) / 60;
                    decimal amount = Decimal.Round(formulaAmount * quantityHours, 2, MidpointRounding.AwayFromZero);
                    if (sum < 0)
                        amount = Decimal.Negate(amount);

                    value += amount;
                }

                if (value != 0)
                    value = Decimal.Round(value, 2);
            }

            return value;
        }

        #region Help-methods

        private void CalculateTimeAccumulatorBalance(CompEntities entities, TimeAccumulatorItem item, int employeeId, List<TimeAccumulatorBalance> timeAccumulatorBalances = null)
        {
            if (!item.DoCalculuateBalanceYear(out DateTime calculationStartDate))
                return;

            TimeAccumulatorBalance timeAccumulatorBalance = GetTimeAccumulatorBalance(entities, item.TimeAccumulatorId, employeeId, SoeTimeAccumulatorBalanceType.Year, calculationStartDate, timeAccumulatorBalances);
            item.TimeAccumulatorBalanceYear = timeAccumulatorBalance?.Quantity ?? 0;
        }

        private void CalculateTimeAccumulatorYear(CompEntities entities, TimeAccumulatorItem item, DateTime? overrideStopDate = null)
        {
            if (!item.DoCalculateYear(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, overrideStopDate))
                return;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumYear = item.SumTimeCodeYear + item.SumInvoiceYear + item.SumPayrollYear;
                item.SumYearWithIB = item.SumYear + item.TimeAccumulatorBalanceYear;
            }
        }

        private void CalculateTimeAccumulatorAccumulated(CompEntities entities, TimeAccumulatorItem item, Employee employee, bool calculateValue)
        {
            if (!item.DoCalculateAccToday(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType))
                return;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumAccToday = item.SumTimeCodeAccToday + item.SumInvoiceAccToday + item.SumPayrollAccToday + item.TimeAccumulatorBalanceYear;
                item.AccTodayStartDate = calculationStartDate;
                item.AccTodayStopDate = calculationStopDate;
            }

            if (calculateValue)
                item.SumAccTodayValue = CalculateTimeAccumulatorAccumulatedValue(entities, item, calculationStartDate, calculationStopDate, employee);
        }

        private decimal? CalculateTimeAccumulatorAccumulatedValue(CompEntities entities, TimeAccumulatorItem item, DateTime calculationStartDate, DateTime calculationStopDate, Employee employee)
        {
            if (!item.FinalSalary || !item.TimeCodeId.HasValue || item.SumAccToday == 0 || employee == null)
                return null;

            return GetTimeAccumulatorValue(entities, employee.ActorCompanyId, employee, calculationStartDate, calculationStopDate, item.SumAccToday, item.TimeCodeId);
        }

        private bool CalculateTimeAccumulatorPlanningPeriod(CompEntities entities, TimeAccumulatorItem item, out TimePeriodDTO currentPlanningTimePeriod)
        {
            if (!item.DoCalculatePlanningPeriod(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, out currentPlanningTimePeriod))
                return false;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumPlanningPeriod = item.SumTimeCodePlanningPeriod + item.SumInvoicePlanningPeriod + item.SumPayrollPlanningPeriod;
                item.PlanningPeriodName = item.TimeAccumulatorTimePeriodHead?.Name;
                item.PlanningPeriodStartDate = calculationStartDate;
                item.PlanningPeriodStopDate = calculationStopDate;
                item.PlanningPeriodDatesText = $"{calculationStartDate.ToShortDateString()} - {calculationStopDate.ToShortDateString()}";
                item.HasPlanningPeriod = true;
            }
            return true;
        }

        private void CalculateTimeAccumulatorPeriod(CompEntities entities, TimeAccumulatorItem item)
        {
            if (!item.DoCalculatePeriod(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, out string timePeriodName, out string timePeriodDatesText))
                return;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumPeriod = item.SumTimeCodePeriod + item.SumInvoicePeriod + item.SumPayrollPeriod;
                item.TimePeriodName = timePeriodName;
                item.TimePeriodDatesText = timePeriodDatesText;
                item.HasTimePeriod = true;
            }
        }

        private void CalculateTimeAccumulatorRange(CompEntities entities, TimeAccumulatorItem item, Employee employee, bool calculateValue)
        {
            if (!item.DoCalculateRange(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType))
                return;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumRange = item.SumTimeCodeRange + item.SumInvoiceRange + item.SumPayrollRange;
                if (calculateValue)
                    item.SumRangeValue = CalculateTimeAccumulatorAccumulatedValue(entities, item, calculationStartDate, calculationStopDate, employee);
            }
        }

        private void CalculateTimeAccumulatorDay(CompEntities entities, TimeAccumulatorItem item)
        {
            if (!item.DoCalculateDay(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType))
                return;

            if (TryCalculateAndUpdateTimeAccumulator(entities, item, calculationStartDate, calculationStopDate, periodType))
            {
                item.SumToday = item.SumTimeCodeToday + item.SumInvoiceToday + item.SumPayrollToday;
            }
        }

        private bool TryCalculateAndUpdateTimeAccumulator(CompEntities entities, TimeAccumulatorItem item, DateTime calculationStartDate, DateTime calculationStopDate, TermGroup_AccumulatorTimePeriodType periodType)
        {
            item.UpdateTimeCode(periodType, CalculateTimeCodeTransactions(entities, item, calculationStartDate, calculationStopDate));
            item.UpdatePayrollProduct(periodType, CalculateTimePayrollTransactions(entities, item, calculationStartDate, calculationStopDate));
            item.UpdateInvoiceProduct(periodType, CalculateTimeInvoiceTransactions(entities, item, calculationStartDate, calculationStopDate));
            return true;
        }

        private void AddTimeAccumultorSourceIds(CompEntities entities, TimeAccumulatorItem item, int timeAccumulatorId)
        {
            var sources = entities.GetTimeAccumulatorFactors(timeAccumulatorId);
            foreach (var source in sources)
            {
                switch (source.EntityType)
                {
                    case (int)SoeTimeTransactionType.TimeCode:
                        item.TimeCodeIds.Add(source.EntityId, source.Factor);
                        break;
                    case (int)SoeTimeTransactionType.TimePayroll:
                        item.PayrollProductIds.Add(source.EntityId, source.Factor);
                        break;
                    case (int)SoeTimeTransactionType.TimeInvoice:
                        item.InvoiceProductIds.Add(source.EntityId, source.Factor);
                        break;
                }
            }
        }

        private void AddTimeAccumulatorEmployeeGroupRules(CompEntities entities, TimeAccumulatorItem item, TimeAccumulatorEmployeeGroupRule employeeGroupRule, TimePeriodDTO currentPlanningTimePeriod, Employee employee, int? clockRounding = null)
        {
            if (employeeGroupRule == null || employee == null)
                return;

            item.EmployeeGroupRules = new List<TimeAccumulatorRuleItem>();

            int? defaultMinutes = null;

            switch ((TermGroup_AccumulatorTimePeriodType)employeeGroupRule.Type)
            {
                case TermGroup_AccumulatorTimePeriodType.Day:
                    #region Day

                    item.EmployeeGroupRules.Add(GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.Day, item.SumToday));

                    #endregion
                    break;
                case TermGroup_AccumulatorTimePeriodType.Week:
                case TermGroup_AccumulatorTimePeriodType.Month:
                    // NA
                    break;
                case TermGroup_AccumulatorTimePeriodType.Period:
                    #region Period

                    item.EmployeeGroupRules.Add(GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.Period, item.SumPeriod));

                    #endregion
                    break;
                case TermGroup_AccumulatorTimePeriodType.PlanningPeriod:
                    #region PlanningPeriod

                    if (currentPlanningTimePeriod != null)
                    {
                        if (!clockRounding.HasValue)
                            clockRounding = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningClockRounding, 0, employee.ActorCompanyId, 0);

                        int? currentPlanningPeriodMinutes = null;
                        if (item.HasPlanningPeriod && (!employeeGroupRule.MinMinutes.HasValue || !employeeGroupRule.MaxMinutes.HasValue))
                            defaultMinutes = currentPlanningPeriodMinutes = EmployeeManager.GetAnnualWorkTimeMinutes(entities, currentPlanningTimePeriod.StartDate, currentPlanningTimePeriod.StopDate, employee, clockRounding: clockRounding);
                        item.EmployeeGroupRules.Add(GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.PlanningPeriod, item.SumPlanningPeriod, defaultRuleValue: currentPlanningPeriodMinutes));

                        if (item.AccTodayStartDate.HasValue && item.AccTodayStopDate.HasValue && (item.TimeAccumulatorTimePeriodHead?.TimePeriods?.Any(i => i.StartDate == item.AccTodayStartDate.Value) ?? false))
                        {
                            int planningPeriodAccTodayMinutes = 0;
                            List<TimePeriodDTO> planningTimePeriods = item.TimeAccumulatorTimePeriodHead.TimePeriods.Where(i => i.StartDate >= item.AccTodayStartDate.Value && i.StartDate <= item.AccTodayStopDate.Value).OrderBy(i => i.StartDate).ToList();
                            foreach (TimePeriodDTO planningTimePeriod in planningTimePeriods)
                            {
                                if (planningTimePeriod.TimePeriodId == currentPlanningTimePeriod.TimePeriodId && currentPlanningPeriodMinutes.HasValue && currentPlanningTimePeriod.StopDate <= item.AccTodayStopDate.Value)
                                {
                                    //Re-use calculation above
                                    planningPeriodAccTodayMinutes += currentPlanningPeriodMinutes.Value;
                                }
                                else
                                {
                                    DateTime planningPeriodStopDate = planningTimePeriod.StopDate <= item.AccTodayStopDate.Value ? planningTimePeriod.StopDate : item.AccTodayStopDate.Value;
                                    planningPeriodAccTodayMinutes += EmployeeManager.GetAnnualWorkTimeMinutes(entities, planningTimePeriod.StartDate, planningPeriodStopDate, employee, clockRounding: clockRounding);
                                }
                            }

                            item.EmployeeGroupRules.Add((GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.PlanningPeriodRunning, item.SumAccToday, planningPeriodAccTodayMinutes, forceDefault: true)));
                        }
                    }

                    #endregion
                    break;
                case TermGroup_AccumulatorTimePeriodType.AccToday:
                    #region Running

                    item.EmployeeGroupRules.Add(GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.AccToday, item.SumAccToday));

                    #endregion
                    break;
                case TermGroup_AccumulatorTimePeriodType.Year:
                    #region Year

                    item.EmployeeGroupRules.Add(GetTimeAccumulatorPeriodRule(employeeGroupRule, TermGroup_AccumulatorTimePeriodType.Year, item.SumYear));

                    #endregion
                    break;
            }

            item.EmployeeGroupRuleType = (TermGroup_AccumulatorTimePeriodType)employeeGroupRule.Type;
            item.EmployeeGroupRuleMinMinutes = employeeGroupRule.MinMinutes;
            item.EmployeeGroupRuleMinMinutesWarning = employeeGroupRule.MinMinutesWarning;
            item.EmployeeGroupRuleMaxMinutes = employeeGroupRule.MaxMinutes;
            item.EmployeeGroupRuleMaxMinutesWarning = employeeGroupRule.MaxMinutesWarning;
            item.EmployeeGroupRuleBoundaries = employeeGroupRule.GetRuleBoundaries(GetText(employeeGroupRule.Type, (int)TermGroup.AccumulatorTimePeriodType), defaultMinutes);
        }

        private TimeAccumulatorRuleItem GetTimeAccumulatorPeriodRule(TimeAccumulatorEmployeeGroupRule employeeGroupRule, TermGroup_AccumulatorTimePeriodType periodType, decimal value, int? defaultRuleValue = null, bool forceDefault = false)
        {
            if (employeeGroupRule == null)
                return new TimeAccumulatorRuleItem(periodType);

            return GetTimeAccumulatorPeriodRule(employeeGroupRule.MinMinutes, employeeGroupRule.MinMinutesWarning, employeeGroupRule.MaxMinutes, employeeGroupRule.MaxMinutesWarning, periodType, value, defaultRuleValue, forceDefault);
        }

        private TimeAccumulatorRuleItem GetTimeAccumulatorPeriodRule(int? ruleMin, int? ruleMinWarning, int? ruleMax, int? ruleMaxWarning, TermGroup_AccumulatorTimePeriodType periodType, decimal value, int? defaultRuleValue = null, bool forceDefault = false)
        {
            int? minMinutes = GetTimeAccumulatorPeriodRule(ruleMin, defaultRuleValue, forceDefault);
            if (minMinutes.HasValue && value < minMinutes.Value)
                return new TimeAccumulatorRuleItem(periodType, SoeTimeAccumulatorComparison.LessThanMin, value, minMinutes.Value);

            int? minMinutesWarning = GetTimeAccumulatorPeriodRule(ruleMinWarning, defaultRuleValue, forceDefault);
            if (minMinutesWarning.HasValue && value < minMinutesWarning.Value)
                return new TimeAccumulatorRuleItem(periodType, SoeTimeAccumulatorComparison.LessThanMinWarning, value, minMinutesWarning.Value);

            int? maxMinutes = GetTimeAccumulatorPeriodRule(ruleMax, defaultRuleValue, forceDefault);
            if (maxMinutes.HasValue && value > maxMinutes.Value)
                return new TimeAccumulatorRuleItem(periodType, SoeTimeAccumulatorComparison.MoreThanMax, value, maxMinutes.Value);

            int? maxMinutesWarning = GetTimeAccumulatorPeriodRule(ruleMaxWarning, defaultRuleValue, forceDefault);
            if (maxMinutesWarning.HasValue && value > maxMinutesWarning.Value)
                return new TimeAccumulatorRuleItem(periodType, SoeTimeAccumulatorComparison.MoreThanMaxWarning, value, maxMinutesWarning.Value);

            return new TimeAccumulatorRuleItem(periodType, noRulesDefined: !minMinutes.HasValue && !minMinutesWarning.HasValue && !maxMinutes.HasValue && !maxMinutesWarning.HasValue);
        }

        private int? GetTimeAccumulatorPeriodRule(int? ruleValue, int? defaultValue, bool forceDefault)
        {
            return ruleValue.HasValue && !forceDefault ? ruleValue.Value : defaultValue;
        }

        #endregion

        #endregion

        #region TimeAccumulatorTimeCode

        public Dictionary<int, List<TimeCode>> GetTimeAccumulatorTimeCodes(CompEntities entities, List<int> timeAccumulatorIds)
        {
            Dictionary<int, List<TimeCode>> dict = new Dictionary<int, List<TimeCode>>();
            if (timeAccumulatorIds.IsNullOrEmpty())
                return dict;

            foreach (int timeAccumulatorId in timeAccumulatorIds)
            {
                if (!dict.ContainsKey(timeAccumulatorId))
                    dict.Add(timeAccumulatorId, GetTimeAccumulatorTimeCodes(entities, timeAccumulatorId));
            }

            return dict;
        }

        public List<TimeCode> GetTimeAccumulatorTimeCodes(CompEntities entities, int timeAccumulatorId)
        {
            return (from t in entities.TimeAccumulatorTimeCode
                    where t.TimeAccumulatorID == timeAccumulatorId
                    select t.TimeCode).ToList();
        }

        public decimal CalculateTimeCodeTransactions(CompEntities entities, int employeeId, int timeAccumulatorId, DateTime startDate, DateTime stopDate)
        {
            decimal sum = 0;

            var transactions = TimeTransactionManager.GetTimeCodeTransactionsForAcc(entities, timeAccumulatorId, employeeId, startDate, stopDate, loadTimeCode: false).ToList();
            foreach (var transaction in transactions)
            {
                sum += transaction.Quantity * transaction.Factor;
            }

            return sum;
        }

        public TimeAccumulatorCalculationTimeCodeResult CalculateTimeCodeTransactions(CompEntities entities, TimeAccumulatorItem item, DateTime startDate, DateTime stopDate)
        {
            if (item.TimeCodeCalculation == null)
                item.TimeCodeCalculation = new TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationTimeCodeRow>(item.EmployeeId);

            if (!item.TimeCodeCalculation.ContainsDateRange(startDate, stopDate, out List<DateRangeDTO> missingDateRanges))
            {
                foreach (DateRangeDTO dateRange in missingDateRanges)
                {
                    var timeCodeTransactionsForAcc = TimeTransactionManager.GetTimeCodeTransactionsForAcc(entities, item.TimeAccumulatorId, item.EmployeeId, dateRange.Start, dateRange.Stop);
                    foreach (var timeCodeTransaction in timeCodeTransactionsForAcc)
                    {
                        item.TimeCodeCalculation.TryAddRow(new TimeAccumulatorCalculationTimeCodeRow(
                            timeCodeTransaction.TimeAccumulatorId,
                            timeCodeTransaction.Date,
                            timeCodeTransaction.Quantity,
                            timeCodeTransaction.Factor,
                            timeCodeTransaction.TimeCodeId,
                            timeCodeTransaction.TimeCodeTransactionId,
                            timeCodeTransaction.TimeCode?.RegistrationType ?? 0));
                    }
                }

                item.TimeCodeCalculation.AddDateIntervals(missingDateRanges);
            }

            return item.TimeCodeCalculation.GetResult<TimeAccumulatorCalculationTimeCodeResult>(startDate, stopDate);
        }

        #endregion

        #region TimeAccumulatorPayrollProduct

        public Dictionary<int, List<PayrollProduct>> GetTimeAccumulatorPayrollProducts(CompEntities entities, List<int> timeAccumulatorIds)
        {
            Dictionary<int, List<PayrollProduct>> dict = new Dictionary<int, List<PayrollProduct>>();
            if (timeAccumulatorIds.IsNullOrEmpty())
                return dict;

            foreach (int timeAccumulatorId in timeAccumulatorIds)
            {
                if (!dict.ContainsKey(timeAccumulatorId))
                    dict.Add(timeAccumulatorId, GetTimeAccumulatorPayrollProducts(entities, timeAccumulatorId));
            }

            return dict;
        }

        public List<PayrollProduct> GetTimeAccumulatorPayrollProducts(CompEntities entities, int timeAccumulatorId)
        {
            return (from t in entities.TimeAccumulatorPayrollProduct
                    where t.TimeAccumulatorId == timeAccumulatorId
                    select t.PayrollProduct).ToList();
        }

        public decimal CalculateTimePayrollTransactions(CompEntities entities, int employeeId, int timeAccumulatorId, DateTime startDate, DateTime stopDate)
        {
            decimal sum = 0;

            var transactions = TimeTransactionManager.GetTimePayrollTransactionsForAcc(entities, timeAccumulatorId, employeeId, startDate, stopDate, loadProduct: false);
            foreach (var transaction in transactions)
            {
                sum += transaction.Quantity * transaction.Factor;
            }

            return sum;
        }

        public TimeAccumulatorCalculationPayrollProductResult CalculateTimePayrollTransactions(CompEntities entities, TimeAccumulatorItem item, DateTime startDate, DateTime stopDate)
        {
            if (item.TimePayrollCalculation == null)
                item.TimePayrollCalculation = new TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationPayrollProductRow>(item.EmployeeId);

            if (!item.TimePayrollCalculation.ContainsDateRange(startDate, stopDate, out List<DateRangeDTO> missingDateRanges))
            {
                foreach (DateRangeDTO dateRange in missingDateRanges)
                {
                    var transactions = TimeTransactionManager.GetTimePayrollTransactionsForAcc(entities, item.TimeAccumulatorId, item.EmployeeId, dateRange.Start, dateRange.Stop).ToList();
                    foreach (var transaction in transactions)
                    {
                        item.TimePayrollCalculation.TryAddRow(
                            new TimeAccumulatorCalculationPayrollProductRow(transaction.TimeAccumulatorId, transaction.Date, transaction.Quantity, transaction.Factor, transaction.ProductId, transaction.TimePayrollTransactionId));
                    }
                }

                item.TimePayrollCalculation.AddDateIntervals(missingDateRanges);
            }

            return item.TimePayrollCalculation.GetResult<TimeAccumulatorCalculationPayrollProductResult>(startDate, stopDate);
        }

        #endregion

        #region TimeAccumulatorInvoiceProduct

        public TimeAccumulatorCalculationInvoiceProductResult CalculateTimeInvoiceTransactions(CompEntities entities, TimeAccumulatorItem item, DateTime startDate, DateTime stopDate)
        {
            if (item.TimeInvoiceCalculation == null)
                item.TimeInvoiceCalculation = new TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationInvoiceProductRow>(item.EmployeeId);

            if (!item.TimeInvoiceCalculation.ContainsDateRange(startDate, stopDate, out List<DateRangeDTO> missingDateRanges))
            {
                foreach (DateRangeDTO dateRange in missingDateRanges)
                {
                    var transactions = entities.GetTimeInvoiceTransactionsForAcc(item.TimeAccumulatorId, item.EmployeeId, startDate, stopDate).ToList();
                    foreach (var transaction in transactions)
                    {
                        item.TimeInvoiceCalculation.TryAddRow(
                            new TimeAccumulatorCalculationInvoiceProductRow(transaction.TimeAccumulatorId, transaction.Date, transaction.Quantity, transaction.Factor, transaction.ProductId, transaction.TimeInvoiceTransactionId));
                    }
                }

                item.TimeInvoiceCalculation.AddDateIntervals(missingDateRanges);
            }

            return item.TimeInvoiceCalculation.GetResult<TimeAccumulatorCalculationInvoiceProductResult>(startDate, stopDate);
        }

        #endregion

        #region TimeAccumulatorBalance

        public List<TimeAccumulatorBalance> GetTimeAccumulatorBalancesForCompany(CompEntities entities, int actorCompanyId, DateTime calculationDate, DateTime balanceDate)
        {
            return (from tab in entities.TimeAccumulatorBalance
                    where tab.ActorCompanyId == actorCompanyId &&
                    (tab.Date == calculationDate || tab.Date == balanceDate)
                    select tab).ToList();
        }

        public List<TimeAccumulatorBalance> GetTimeAccumulatorBalancesForEmployee(CompEntities entities, int actorCompanyId, int employeeId)
        {
            return (from tab in entities.TimeAccumulatorBalance
                    where tab.ActorCompanyId == actorCompanyId &&
                    tab.EmployeeId == employeeId
                    select tab).ToList();
        }

        public TimeAccumulatorBalance GetTimeAccumulatorBalance(CompEntities entities, int timeAccumulatorId, int employeeId, SoeTimeAccumulatorBalanceType type, DateTime date, List<TimeAccumulatorBalance> timeAccumulatorBalancesForCompany = null)
        {
            TimeAccumulatorBalance timeAccumulatorBalance = null;

            switch (type)
            {
                case SoeTimeAccumulatorBalanceType.Year:
                    #region Year

                    if (timeAccumulatorBalancesForCompany != null)
                        timeAccumulatorBalance = timeAccumulatorBalancesForCompany.FirstOrDefault(i => i.TimeAccumulatorId == timeAccumulatorId && i.EmployeeId == employeeId && i.Date == date);
                    else
                        timeAccumulatorBalance = entities.TimeAccumulatorBalance.FirstOrDefault(i => i.TimeAccumulatorId == timeAccumulatorId && i.EmployeeId == employeeId && i.Date == date && i.State == (int)SoeEntityState.Active);

                    #endregion 
                    break;
            }

            return timeAccumulatorBalance;
        }

        /// <summary>
        /// Calculate and creates/updates TimeAccumulatorBalance for all Employees and TimeAccumulators in Company
        /// </summary>
        /// <param name="actorCompanyId">The ActorCmpanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateTimeAccumulatorYearBalance(int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<TimeAccumulator> timeAccumulators = GetTimeAccumulators(entities, actorCompanyId);
                List<Employee> employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, active: true, loadEmployment: true);

                return CalculateTimeAccumulatorYearBalance(entities, timeAccumulators, employees, actorCompanyId, checkEmployeeGroup: true);
            }
        }

        /// <summary>
        /// Calculate and creates/updates TimeAccumulatorBalance for given Employee and TimeAccumulator
        /// </summary>
        /// <param name="timeAccumulatorId">The TimeAccumulatorId</param>
        /// <param name="employeeId">The EmployeeId</param>
        /// <param name="actorCompanyId">The ActorCmpanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateTimeAccumulatorYearBalance(int actorCompanyId, List<int> timeAccumulatorIds)
        {
            List<TimeAccumulator> timeAccumulators = new List<TimeAccumulator>();

            using (CompEntities entities = new CompEntities())
            {
                foreach (int timeAccumulatorId in timeAccumulatorIds)
                {
                    timeAccumulators.Add(GetTimeAccumulator(entities, actorCompanyId, timeAccumulatorId));
                }

                List<Employee> employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, loadEmployment: true);

                return CalculateTimeAccumulatorYearBalance(entities, timeAccumulators, employees, actorCompanyId);
            }
        }

        #region Help-methods

        /// <summary>
        /// Calculate and creates/updates TimeAccumulatorBalance for given Employees and TimeAccumulators.
        /// Saves once per Employee
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="timeAccumulators">The TimeAccumulators</param>
        /// <param name="employees">The Employees</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        private ActionResult CalculateTimeAccumulatorYearBalance(CompEntities entities, List<TimeAccumulator> timeAccumulators, List<Employee> employees, int actorCompanyId, bool checkEmployeeGroup = false)
        {
            ActionResult result = new ActionResult(true);

            timeAccumulators = timeAccumulators?.Where(i => i.Type == (int)TermGroup_TimeAccumulatorType.Rolling).ToList();
            if (timeAccumulators.IsNullOrEmpty())
                return result;

            List<TimeAccumulatorEmployeeGroupRule> timeAccumulatorEmployeeGroupRules = null;
            if (checkEmployeeGroup)
            {
                timeAccumulatorEmployeeGroupRules = GetTimeAccumulatorEmployeeGroupRulesForCompany(entities, actorCompanyId);
                if (timeAccumulatorEmployeeGroupRules.IsNullOrEmpty())
                    return result;
            }
            else
                timeAccumulatorEmployeeGroupRules = new List<TimeAccumulatorEmployeeGroupRule>();

            DateTime today = DateTime.Today;
            DateTime balanceDate = new DateTime(today.Year, 1, 1); //First day of current year
            DateTime calculationDate = CalendarUtility.GetBeginningOfDay(balanceDate.AddYears(-1)); //Previous year (Calculation function will get previous year's start and stop date)

            List<TimeAccumulatorBalance> timeAccumulatorBalancesForCompany = GetTimeAccumulatorBalancesForCompany(entities, actorCompanyId, calculationDate, balanceDate);
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));

            foreach (Employee employee in employees)
            {
                List<TimeAccumulator> validTimeAccumulators = new List<TimeAccumulator>();

                if (checkEmployeeGroup)
                {
                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(null, employeeGroups);
                    List<int> timeAccumulatorIds = timeAccumulatorEmployeeGroupRules.Where(a => a.EmployeeGroupId == employeeGroup?.EmployeeGroupId).Select(s => s.TimeAccumulatorId).ToList();
                    if (!timeAccumulatorIds.IsNullOrEmpty())
                        validTimeAccumulators.AddRange(timeAccumulators.Where(w => timeAccumulatorIds.Contains(w.TimeAccumulatorId)));
                }
                else
                {
                    validTimeAccumulators.AddRange(timeAccumulators);
                }

                if (!validTimeAccumulators.Any())
                    continue;

                foreach (TimeAccumulator timeAccumulator in validTimeAccumulators)
                {
                    #region TimeAccumulatorBalance

                    TimeAccumulatorBalance timeAccumulatorBalance = GetTimeAccumulatorBalance(entities, timeAccumulator.TimeAccumulatorId, employee.EmployeeId, SoeTimeAccumulatorBalanceType.Year, balanceDate, timeAccumulatorBalancesForCompany);
                    if (timeAccumulatorBalance == null)
                    {
                        timeAccumulatorBalance = new TimeAccumulatorBalance()
                        {
                            Date = balanceDate,
                            Type = (int)SoeTimeAccumulatorBalanceType.Year,

                            //Set FK
                            EmployeeId = employee.EmployeeId,
                            TimeAccumulatorId = timeAccumulator.TimeAccumulatorId,
                            ActorCompanyId = actorCompanyId,
                        };
                        SetCreatedProperties(timeAccumulatorBalance);
                        entities.TimeAccumulatorBalance.AddObject(timeAccumulatorBalance);
                    }
                    else
                    {
                        SetModifiedProperties(timeAccumulatorBalance);
                    }

                    //Set Quantity
                    GetTimeAccumulatorItemsInput timeAccInput = GetTimeAccumulatorItemsInput.CreateInput(actorCompanyId, base.UserId, employee.EmployeeId, calculationDate, calculationDate, calculateYear: true, includeBalanceYear: true);
                    TimeAccumulatorItem item = GetTimeAccumulatorItem(entities, timeAccInput, timeAccumulator, employee, timeAccumulatorBalances: timeAccumulatorBalancesForCompany);
                    if (item != null)
                        timeAccumulatorBalance.Quantity = item.SumYearWithIB;

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;
            }

            return result;
        }

        #endregion

        #endregion

        #region AccumulatorBalanceRuleView

        public List<AccumulatorBalanceRuleView> GetAccumulatorBalanceRulesForEmployee(int actorCompanyId, int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccumulatorBalanceRuleView.NoTracking();
            return (from abrv in entities.AccumulatorBalanceRuleView
                    where abrv.ActorCompanyId == actorCompanyId &&
                    abrv.IsEmployee == 1 &&
                    (!abrv.EmployeeId.HasValue || abrv.EmployeeId.Value == employeeId)
                    select abrv).ToList();
        }

        public List<AccumulatorBalanceRuleView> GetAccumulatorBalanceRulesForEmployeeGroup(int actorCompanyId, int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccumulatorBalanceRuleView.NoTracking();
            return (from abrv in entities.AccumulatorBalanceRuleView
                    where abrv.ActorCompanyId == actorCompanyId &&
                    abrv.IsEmployeeGroup == 1 &&
                    (!abrv.EmployeeGroupId.HasValue || abrv.EmployeeGroupId.Value == employeeGroupId)
                    select abrv).ToList();
        }

        #endregion
    }
}
