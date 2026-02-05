using SoftOne.Soe.Business.Util.BatchHelper;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Employee = SoftOne.Soe.Data.Employee;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private CalculateTimeWorkReductionReconciliationYearEmployeeOutputDTO TaskCalculateTimeWorkReductionReconciliationEmployee()
        {
            var (iDTO, oDTO) = InitTask<CalculateTimeWorkReductionReconciliationYearEmployeeInputDTO, CalculateTimeWorkReductionReconciliationYearEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    var reconciliation = GetTimeWorkReductionReconciliation(iDTO.TimeWorkReductionReconciliationId);
                    if (reconciliation == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(110686, "ATF avstämning hittades inte"));
                        return oDTO;
                    }

                    var reconciliationYear = GetTimeWorkReductionReconciliationYear(iDTO.TimeWorkReductionReconciliationId, iDTO.TimeWorkReductionReconciliationYearId);
                    if (reconciliationYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }

                    var reconciliationEmployeesForYear = GetTimeWorkReductionReconciliationEmployees(reconciliationYear.TimeWorkReductionReconciliationYearId, iDTO.TimeWorkReductionReconciliationEmployeeIds, iDTO.EmployeeIds);

                    var allEmployeeGroups = GetEmployeeGroupsFromCache();
                    var timeAccumulator = GetTimeAccumulatorWithEmployeeGroupRules(reconciliation.TimeAccumulatorId);

                    List<Employee> validEmployees;
                    if (!iDTO.EmployeeIds.IsNullOrEmpty())
                    {
                        validEmployees = GetEmployeesWithEmployment(iDTO.EmployeeIds);
                    }
                    else
                    {
                        validEmployees = GetEmployeesForCompanyWithEmployment()
                            .Where(e =>
                            {
                                var employeeGroup = e.GetEmployeeGroup(reconciliationYear.Stop, allEmployeeGroups);
                                bool hasTimeAccumulator = employeeGroup != null && timeAccumulator.TimeAccumulatorEmployeeGroupRule.Any(eg => eg.EmployeeGroupId == employeeGroup.EmployeeGroupId && eg.ThresholdMinutes.HasValue && eg.State == (int)SoeEntityState.Active);
                                bool hasExistingCalculationForYear = reconciliationEmployeesForYear.Any(re => re.EmployeeId == e.EmployeeId);
                                return hasTimeAccumulator || hasExistingCalculationForYear;
                            })
                            .ToList();
                    }
                    if (validEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(110687, "Inga anställda kopplade till ATF-saldot"));
                        return oDTO;
                    }
                    
                    foreach (var employee in validEmployees)
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            var result = CalculateTimeWorkReductionReconciliationEmployee(
                                employee,
                                reconciliation,
                                reconciliationYear,
                                timeAccumulator,
                                reconciliationEmployeesForYear,
                                reconciliationEmployeeIds: iDTO.TimeWorkReductionReconciliationEmployeeIds
                                );
                            if (result != null)
                                oDTO.FunctionResult.Rows.Add(result);

                            TryCommit(oDTO);
                        }
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    Beautify(oDTO.FunctionResult);
                    if (!oDTO.Result.Success)
                    {
                        oDTO.FunctionResult.Result = oDTO.Result;
                        LogTransactionFailed(this.ToString());
                    }

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }
        
        private TimeWorkReductionReconciliationTransactionResultRowDTO TaskTimeWorkReductionReconciliationYearEmployeeGenerateOutcome()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkReductionReconciliationYearEmployeeGenerateOutcomeInputDTO, TimeWorkReductionReconciliationTransactionResultRowDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkReductionReconciliation timeWorkReductionReconciliation = GetTimeWorkReductionReconciliation(iDTO.TimeWorkReductionReconciliationId);
                    if (timeWorkReductionReconciliation == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10200, "Arbetstidsförkortning hittades inte"));
                        return oDTO;
                    }

                    TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear = GetTimeWorkReductionReconciliationYear(iDTO.TimeWorkReductionReconciliationId, iDTO.TimeWorkReductionReconciliationYearId);
                    if (timeWorkReductionReconciliationYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10201, "Avstämning hittades inte"));
                        return oDTO;
                    }

                    if (timeWorkReductionReconciliation.UseDirectPayment && !HasPayrollProductFromCache(timeWorkReductionReconciliationYear.DirectPaymentPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92012, "Löneart för kontant ersättning ej angivet"));
                        return oDTO;
                    }
                    if (timeWorkReductionReconciliation.UsePensionDeposit && !HasPayrollProductFromCache(timeWorkReductionReconciliationYear.PensionDepositPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92013, "Löneart för pensionspremie ej angivet"));
                        return oDTO;
                    }

                    List<TimeWorkReductionReconciliationEmployee> timeWorkReductionReconciliationEmployees = GetTimeWorkReductionReconciliationEmployees(iDTO.TimeWorkReductionReconciliationYearId, iDTO.TimeWorkReductionReconciliationEmployeeIds, iDTO.EmployeeIds);
                    if (timeWorkReductionReconciliationEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10202, "Inga anställda kopplade till arbetstidsförkortning"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkReductionReconciliationEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10202, "Inga anställda kopplade till arbetstidsförkortning"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(TimeWorkReductionReconciliationGenerateOutcome(employees.Where(x => batchHelper.GetCurrentBatchIds().Contains(x.EmployeeId)).ToList(), timeWorkReductionReconciliation, timeWorkReductionReconciliationYear, timeWorkReductionReconciliationEmployees, attestStateInitial, iDTO.PaymentDate, iDTO.OverrideChoosen));
                            TryCommit(oDTO);
                        }
                        batchHelper.MoveToNextBatch();
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    Beautify(oDTO.FunctionResult);
                    if (!oDTO.Result.Success)
                    {
                        oDTO.FunctionResult.Result = oDTO.Result;
                        LogTransactionFailed(this.ToString());
                    }

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private TimeWorkReductionReconciliationTransactionResultRowDTO TaskTimeWorkReductionReconciliationYearEmployeeReverseTransactions()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkReductionReconciliationYearEmployeeReverseTransactionsInputDTO, TimeWorkReductionReconciliationTransactionResultRowDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkReductionReconciliation timeWorkReductionReconciliation = GetTimeWorkReductionReconciliation(iDTO.TimeWorkReductionReconciliationId);
                    if (timeWorkReductionReconciliation == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10200, "Arbetstidsförkortning hittades inte"));
                        return oDTO;
                    }

                    TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear = GetTimeWorkReductionReconciliationYear(iDTO.TimeWorkReductionReconciliationId, iDTO.TimeWorkReductionReconciliationYearId);
                    if (timeWorkReductionReconciliationYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10201, "Avstämning hittades inte"));
                        return oDTO;
                    }

                    List<TimeWorkReductionReconciliationEmployee> timeWorkReductionReconciliationEmployees = GetTimeWorkReductionReconciliationEmployees(iDTO.TimeWorkReductionReconciliationYearId, iDTO.TimeWorkReductionReconciliationEmployeeIds, iDTO.EmployeeIds);
                    if (timeWorkReductionReconciliationEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10202, "Inga anställda kopplade till arbetstidsförkortning"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkReductionReconciliationEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10202, "Inga anställda kopplade till arbetstidsförkortning"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(TimeWorkReductionReconciliationReverseTransaction(employees.Where(x => batchHelper.GetCurrentBatchIds().Contains(x.EmployeeId)).ToList(), timeWorkReductionReconciliation, timeWorkReductionReconciliationYear, timeWorkReductionReconciliationEmployees, attestStateInitial, TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome));
                            TryCommit(oDTO);
                        }
                        batchHelper.MoveToNextBatch();
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    Beautify(oDTO.FunctionResult);
                    if (!oDTO.Result.Success)
                    {
                        oDTO.FunctionResult.Result = oDTO.Result;
                        LogTransactionFailed(this.ToString());
                    }

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }
        #endregion

        #region TimeWorkReduction

        private ActionResult CreateTimeWorkReductionTransactions(TimeEngineTemplate template)
        {
            return CreateTimeWorkReductionTransactions(template.Employee, template.Date);
        }

        private ActionResult CreateTimeWorkReductionTransactions(Employee employee, DateTime date)
        {
            ActionResult result = new ActionResult(true);

            if (!UseTimeWorkReductionFromCache())
                return result;

            var earningPeriod = CalculateTimeWorkReductionEarningPeriod(employee, date);
            if (earningPeriod == null)
                return result;

            var earningPeriodSchedule = GetScheduleBlocksFromCache(employee.EmployeeId, earningPeriod.Start, earningPeriod.Stop);

            if (!DoCalculateTimeWorkReductionForEmployee(employee, date, earningPeriod, earningPeriodSchedule))
                return result;

            var existingEarningTimeCodeTransactions = GetTimeCodeTransactionsWithEarningAccumulator(employee.EmployeeId, earningPeriod.Start, earningPeriod.Stop);
            result = SetTimeCodeTransactionsToDeleted(existingEarningTimeCodeTransactions, saveChanges: false);
            if (!result.Success)
                return result;

            var existingEarningTimePayrollTransction = GetTimePayrollTransactionsWithEarningAccumulator(employee.EmployeeId, earningPeriod.Start, earningPeriod.Stop);
            result = SetTimePayrollTransactionsToDeleted(existingEarningTimePayrollTransction, saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            result = Save();
            if (!result.Success)
                return result;

            var employmentsByDate = employee.GetEmploymentsByDate(earningPeriod.Start, earningPeriod.Stop);

            var earningIntervals = CalculateTimeWorkReductionEarningIntervals(employee, date, earningPeriod, employmentsByDate, earningPeriodSchedule);
            if (earningIntervals.IsNullOrEmpty())
                return result;

            var earningResults = earningIntervals.Select(CalculateTimeWorkReductionEarningResult).ToList();
            if (earningResults.IsNullOrEmpty())
                return result;

            var attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            foreach (var earningResult in earningResults.OrderBy(er => er.EarningInterval.DateFrom))
            {
                if (!earningResult.EarningInterval.TimeAccumulator.TimeCodeId.HasValue)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Tidkod för intjänande saldo hittades inte");

                var earningTimeCode = GetTimeCodeWithProductsFromCache(earningResult.EarningInterval.TimeAccumulator.TimeCodeId.Value);
                if (earningTimeCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Tidkod för intjänande saldo hittades inte");

                var transactionDate = earningResult.EarningInterval.DateTo;
                var timeBlockDate = GetTimeBlockDateFromCache(earningResult.EarningInterval.Employee.EmployeeId, transactionDate, true);

                var timeCodeTransaction = CreateOrUpdateTimeWorkReductionTimeCodeTransaction(earningResult, earningTimeCode, timeBlockDate, existingEarningTimeCodeTransactions);
                if (earningTimeCode.TimeCodePayrollProduct != null)
                {
                    foreach (var timeCodePayrollProduct in earningTimeCode.TimeCodePayrollProduct)
                    {
                        PayrollProduct earningPayrollProduct = GetPayrollProductFromCache(timeCodePayrollProduct.ProductId);
                        if (earningPayrollProduct == null)
                            return new ActionResult(8517, GetText(91923, "Löneart hittades inte"));

                        CreateOrUpdateTimeWorkReductionTimePayrollTransaction(earningResult, earningPayrollProduct, timeBlockDate, attestStateInitial, timeCodeTransaction, existingEarningTimePayrollTransction);
                    }
                }
            }

            result = Save();
            return result;
        }

        private DateRangeDTO CalculateTimeWorkReductionEarningPeriod(Employee employee, DateTime date)
        {
            var earningPeriod = TimeWorkReductionEarningInterval.GetEarningPeriod(date);

            var lastLockedTimePayrollTransaction = GetTimePayrollTransactionLastLockedWithTimeBlockDate(employee.EmployeeId, earningPeriod.Start);
            if (lastLockedTimePayrollTransaction != null)
            {
                if (lastLockedTimePayrollTransaction.TimeBlockDate.Date >= earningPeriod.Stop)
                    return null;
                if (lastLockedTimePayrollTransaction.TimeBlockDate.Date >= earningPeriod.Start)
                    earningPeriod.Start = lastLockedTimePayrollTransaction.TimeBlockDate.Date.AddDays(1);
            }

            return earningPeriod;
        }

        public List<TimeWorkReductionEarningInterval> CalculateTimeWorkReductionEarningIntervals(
            Employee employee, 
            DateTime date,
            DateRangeDTO earningPeriod,
            Dictionary<DateTime, Employment> employmentsByDate,
            List<TimeScheduleTemplateBlock> earningPeriodSchedule
            )   
        {
            var earningIntervals = new List<TimeWorkReductionEarningInterval>();

            var employeeEndDate = employee.GetLastEmployment().DateTo;
            if (employeeEndDate.HasValue && employeeEndDate.Value < earningPeriod.Stop)
                earningPeriod.Stop = employeeEndDate.Value;

            var employeeGroupIntervals = employee.GetEmployeeGroupIntervals(
                dateFrom: earningPeriod.Start,
                dateTo: earningPeriod.Stop,
                allEmployeeGroups: GetEmployeeGroupsFromCache(),
                employmentsByDate: employmentsByDate
                );

            foreach (var employeeGroupInterval in employeeGroupIntervals.OrderBy(i => i.DateFrom))
            {
                var earningAccumulatorEmployeeGroupIntervals = GetEarningTimeAccumulatorEmployeeGroups(
                    employeeGroupInterval.EmployeeGroup.EmployeeGroupId,
                    employeeGroupInterval.DateFrom,
                    employeeGroupInterval.DateTo
                );

                foreach (var earningEmployeeGroupInterval in earningAccumulatorEmployeeGroupIntervals)
                {
                    if (earningEmployeeGroupInterval.TimeWorkReductionEarning.PeriodType != (int)TermGroup_TimeWorkReductionPeriodType.Week)
                        continue;

                    var intervalDateFrom = earningEmployeeGroupInterval.DateFrom ?? employeeGroupInterval.DateFrom;
                    if (intervalDateFrom < employeeGroupInterval.DateFrom)
                        intervalDateFrom = employeeGroupInterval.DateFrom;

                    var intervalDateTo = earningEmployeeGroupInterval.DateTo ?? employeeGroupInterval.DateTo;
                    if (intervalDateTo > employeeGroupInterval.DateTo)
                        intervalDateTo = employeeGroupInterval.DateTo;

                    if (intervalDateTo < employeeGroupInterval.DateFrom || intervalDateFrom > employeeGroupInterval.DateTo)
                        continue;

                    earningIntervals.Add(new TimeWorkReductionEarningInterval(
                        employee,
                        employmentsByDate: employmentsByDate
                            .Where(kv => kv.Key >= intervalDateFrom && kv.Key <= intervalDateTo)
                            .ToDictionary(kv => kv.Key, kv => kv.Value),
                        employeeGroup: employeeGroupInterval.EmployeeGroup,
                        timeAccumulator: earningEmployeeGroupInterval.TimeWorkReductionEarning.TimeAccumulator.FirstOrDefault(),
                        scheduleBlocks: earningPeriodSchedule
                            .Where(sb => sb.Date >= intervalDateFrom && sb.Date <= intervalDateTo)
                            .ToList(),
                        weightMinutes: earningEmployeeGroupInterval.TimeWorkReductionEarning.MinutesWeight,
                        currentDate: date,
                        dateFrom: intervalDateFrom,
                        dateTo: intervalDateTo
                    ));
                }
            }


            return earningIntervals;
        }

        private TimeCodeTransaction CreateOrUpdateTimeWorkReductionTimeCodeTransaction(
            TimeWorkReductionEarningResult earningResult, 
            TimeCode earningTimeCode,
            TimeBlockDate timeBlockDate,
            List<TimeCodeTransaction> reusableTimeCodeTransactions
            )
        {
            var timeCodeTransaction = reusableTimeCodeTransactions.FirstOrDefault(t =>
                t.State == (int)SoeEntityState.Deleted &&
                t.TimeCodeId == earningTimeCode.TimeCodeId &&
                t.TimeBlockDate.Date == timeBlockDate.Date
            );

            if (timeCodeTransaction != null)
            {
                timeCodeTransaction.State = (int)SoeEntityState.Active;
                timeCodeTransaction.Quantity = earningResult.Quantity;
                SetModifiedProperties(timeCodeTransaction);
            }
            else
            {
                timeCodeTransaction = CreateTimeCodeTransaction(
                    timeCodeId: earningTimeCode.TimeCodeId,
                    transactionType: TimeCodeTransactionType.Time,
                    quantity: earningResult.Quantity,
                    start: timeBlockDate.Date,
                    stop: timeBlockDate.Date,
                    timeBlockDate: timeBlockDate
                );
            }
            timeCodeTransaction.EarningTimeAccumulatorId = earningResult.EarningInterval.TimeAccumulator.TimeAccumulatorId;
            earningResult.AddTimeCodeTransaction(timeCodeTransaction);

            return timeCodeTransaction;
        }

        private void CreateOrUpdateTimeWorkReductionTimePayrollTransaction(
            TimeWorkReductionEarningResult earningResult,
            PayrollProduct earningPayrollProduct,
            TimeBlockDate timeBlockDate,
            AttestStateDTO attestState,
            TimeCodeTransaction timeCodeTransaction,
            List<TimePayrollTransaction> reusableTimePayrollTransactions
        )
        {
            var timePayrollTransaction = reusableTimePayrollTransactions.FirstOrDefault(t =>
                t.State == (int)SoeEntityState.Deleted &&
                t.ProductId == earningPayrollProduct.ProductId &&
                t.TimeBlockDate.Date == timeBlockDate.Date
            );

            if (timePayrollTransaction != null)
            {
                timePayrollTransaction.State = (int)SoeEntityState.Active;
                timePayrollTransaction.Quantity = earningResult.Quantity;
                SetModifiedProperties(timePayrollTransaction);
            }
            else
            {
                timePayrollTransaction = CreateTimePayrollTransaction(
                    employee: earningResult.EarningInterval.Employee,
                    payrollProduct: earningPayrollProduct,
                    timeBlockDate: timeBlockDate,
                    quantity: earningResult.Quantity,
                    timePeriodId: null,
                    attestStateId: attestState.AttestStateId,
                    timeCodeTransaction: timeCodeTransaction,
                    doForceExtended: true
                );
            }
            timePayrollTransaction.TimePayrollTransactionExtended.FormulaPlain = earningResult.GetFormulaPlain();
            timePayrollTransaction.TimePayrollTransactionExtended.FormulaExtracted = earningResult.GetFormulaExtracted();
            timePayrollTransaction.EarningTimeAccumulatorId = earningResult.EarningInterval.TimeAccumulator.TimeAccumulatorId;
            earningResult.AddTimePayrollTransaction(timePayrollTransaction);
        }

        /// <summary>
        /// Calculates the quantity for TimeWorkReduction transactions based on the formula:
        /// A * B * C * (D/E)
        /// A = Number of days in period (One week unless Employee is ending or changing TimeWorkReduction during week)
        /// B = Minutes on ATF entity
        /// C = Employee percentage (Different types of calculations possible here)
        /// D = Accumulated minutes in TimeWorkReduction accumulator
        /// E = Total schedule minutes for period
        /// </summary>
        /// <param name="employee">The current Employee</param>
        /// <param name="earningInterval">The earning interval for date and EmployeeGroup</param>
        /// <returns>TimeWorkReductionEarningResult containing result and calculation</returns>
        private TimeWorkReductionEarningResult CalculateTimeWorkReductionEarningResult(TimeWorkReductionEarningInterval earningInterval)
        {
            var fullTimeWorkTimeWeek = earningInterval.Employee.CalculateFullTimeWorkTimeWeekAverage(earningInterval.EmployeeGroup, earningInterval.EmploymentsByDate);
            int workTimeWeek = CalculateTimeWorkReductionEmployeeWeekMinutes(earningInterval, earningInterval.EmploymentsByDate) ?? 0;
            decimal employeePercentage = Employment.FormatEmploymentPercent(fullTimeWorkTimeWeek, workTimeWeek);
            int accEarningMinutes = GetAccumulatedTimeWorkReductionBasisMinutes(earningInterval.TimeAccumulator, earningInterval.Employee, earningInterval.DateFrom, earningInterval.DateTo);
            int scheduleMinutes = CalculateScheduleMinutesForTimeWorkReduction(earningInterval);

            var earningResult = new TimeWorkReductionEarningResult(
                earningInterval,
                employeePercentage,
                accEarningMinutes,
                scheduleMinutes,
                (id, defTerm) => GetText(id, defTerm)
            );
            return earningResult;
        }

        /// <summary>
        /// Calculates the employment week minutes according to the selected rule in EmployeeGroup
        /// </summary>
        /// <param name="earningInterval">The earning interval for date and EmployeeGroup</param>
        /// <returns>The employment week minutes</returns>
        private int? CalculateTimeWorkReductionEmployeeWeekMinutes(TimeWorkReductionEarningInterval earningInterval, Dictionary<DateTime, Employment> employmentsByDate)
        {
            if (employmentsByDate.IsNullOrEmpty())
                return null;

            var employee = earningInterval.Employee;
            var employeeGroup = earningInterval.EmployeeGroup;
            var dateFrom = earningInterval.DateFrom;
            var dateTo = earningInterval.DateTo;

            if (employee != null && employeeGroup != null)
            {
                switch (employeeGroup.TimeWorkReductionCalculationRule)
                {
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeek:
                        return GetEmployeeWorkTimeWeekForTimeWorkReduction(employmentsByDate, employee);
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeekPlusExtraShifts:
                        return GetEmployeeWorkTimeWeekPlusExtraShiftsMinutesForTimeWorkReduction(employee, dateFrom, dateTo, employmentsByDate);
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeekPlusAdditionalContract:
                        return GetEmployeeWorkTimeWeekPlusAdditionalContractMinutesTimeWorkReduction(employee, employeeGroup, dateFrom);
                }
            }
            return null;
        }

        /// <summary>
        /// Anställdes genomsmittliga veckoarbetstid enligt anställningskontraktet
        /// </summary>
        private static int GetEmployeeWorkTimeWeekForTimeWorkReduction(Dictionary<DateTime, Employment> employmentsByDate, Employee employee)
        {
            return employee.CalculateWorkTimeWeekAverage(employmentsByDate);
        }

        /// <summary>
        /// X = (A + B)
        /// 
        /// A - Anställdes genomsmittliga veckoarbetstid enligt anställningskontraktet
        /// B - Schemapass markerat som extrapass
        /// X - Beräknad genomsnittlig veckoarbetstid
        /// 
        /// </summary>
        private int GetEmployeeWorkTimeWeekPlusExtraShiftsMinutesForTimeWorkReduction(Employee employee, DateTime periodDateFrom, DateTime periodDateTo, Dictionary<DateTime, Employment> employmentsByDate)
        {
            //Formula
            int employeeWorkTimeWeekMinutes = employee.CalculateWorkTimeWeekAverage(employmentsByDate);
            int employeeExtraShiftMinutes = GetEmployeeExtraShiftMinutesForTimeWorkReduction(employee, periodDateFrom, periodDateTo);
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
        private int? GetEmployeeWorkTimeWeekPlusAdditionalContractMinutesTimeWorkReduction(Employee employee, EmployeeGroup employeeGroup, DateTime date)
        {
            if (employee == null || employeeGroup == null)
                return null;

            Employment employment = employee.GetEmployment(date);
            if (employment == null)
                return null;

            List<TimeScheduleType> scheduleTypesWithIgnoreExtraShifts = GetTimeScheduleTypesWithIgnoreIfExtraShiftFromCache();

            //Formula
            var week = CalendarUtility.GetWeek(date);
            int employeeWorkTimeWeek = employment.GetWorkTimeWeek(date);
            int activeScheduleMinutes = GetScheduleMinutesForEmployee(employee.EmployeeId, week.DateFrom, week.DateTo, excludeTimeScheduleTypes: scheduleTypesWithIgnoreExtraShifts);
            int templateScheduleMinutes = GetTemplateScheduleMinutes(employee.EmployeeId, week.DateFrom, week.DateTo);
            int leaveOfAbsenceMinutes = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, week.DateFrom, week.DateTo, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence).GetMinutes();
            int resultMinutes = employeeWorkTimeWeek + (activeScheduleMinutes - leaveOfAbsenceMinutes) - templateScheduleMinutes;

            return resultMinutes;
        }

        /// <summary>
        /// Calculates extra shifts during the period, subtracted any schedule with flag ignoreIfExtraShifts
        /// </summary>
        /// <param name="employee">The employee</param>
        /// <param name="periodDateFrom">The period start date</param>
        /// <param name="periodDateTo">The period stop date</param>
        /// <returns>Extra shift minutes to add to Employment week minutes</returns>
        private int GetEmployeeExtraShiftMinutesForTimeWorkReduction(Employee employee, DateTime periodDateFrom, DateTime periodDateTo)
        {
            return GetExtraShiftCalculationFromCache(employee, periodDateFrom, periodDateTo, useIgnoreIfExtraShifts: true)?.WorkMinutes ?? 0;
        }

        /// <summary>
        /// Calculates the employment week minutes according to the selected rule in EmployeeGroup
        /// </summary>
        /// <param name="earningInterval">The earning interval for date and EmployeeGroup</param>
        /// <returns>The employment week minutes</returns>
        private int CalculateScheduleMinutesForTimeWorkReduction(TimeWorkReductionEarningInterval earningInterval)
        {
            var employee = earningInterval.Employee;
            var employeeGroup = earningInterval.EmployeeGroup;

            if (employee != null && employeeGroup != null)
            {
                switch (employeeGroup.TimeWorkReductionCalculationRule)
                {
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeek:
                        return earningInterval.ScheduleBlocks.GetWorkMinutes();
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeekPlusExtraShifts:
                    case (int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeekPlusAdditionalContract:
                        List<TimeScheduleType> scheduleTypesWithIgnoreExtraShifts = GetTimeScheduleTypesWithIgnoreIfExtraShiftFromCache();
                        return earningInterval.ScheduleBlocks.Filter(scheduleTypesWithIgnoreExtraShifts).GetWorkMinutes();
                }
            }
            return 0;
        }

        /// <summary>
        /// Determines if TimeWorkReduction calculation should be done for employee on given date
        /// </summary>
        /// <param name="employee">The Employee</param>
        /// <param name="date">The date being calculated</param>
        /// <param name="earningPeriodSchedule">The schedule for the reduction period</param>
        /// <returns></returns>
        private bool DoCalculateTimeWorkReductionForEmployee(Employee employee, DateTime date, DateRangeDTO earningPeriod, List<TimeScheduleTemplateBlock> earningPeriodSchedule)
        {
            if (employee == null || earningPeriod == null)
                return false;

            // No schedule in period means no calculation
            earningPeriodSchedule = earningPeriodSchedule.Where(b => b.Date.HasValue && b.StartTime < b.StopTime).ToList();
            if (earningPeriodSchedule.IsNullOrEmpty())
                return false;

            // The last schedule day in period triggers the calculation
            var lastScheduleDay = earningPeriodSchedule.Where(b => b.StartTime < b.StopTime).GetLast();

            // Check if recalculating last schedule day in period
            if (lastScheduleDay.Date == date)
                return true;

            // Check if last schedule day will be recalculated later
            if (date != lastScheduleDay.Date && HasDayInitiatatedCalculation(lastScheduleDay.Date.Value))
                return false;

            // Check if later day in period has initiated calculation
            if (HasInitiatedDaysAfter(date, CalendarUtility.GetEarliestDate(earningPeriod.Stop, lastScheduleDay.Date)))
                return false;

            // Check if last schedule day in period has outcome 
            var timeBlocksLastDay = GetTimePayrollTransactionsConnectedToTimeBlock(employee.EmployeeId, lastScheduleDay.Date.Value);
            if (timeBlocksLastDay.Any())
                return true;

            return false;
        }

        #endregion

        #region TimeWorkReductionReconciliation

        private TimeWorkReductionReconciliation GetTimeWorkReductionReconciliation(int timeWorkReductionReconciliationId)
        {
            return entities.TimeWorkReductionReconciliation.FirstOrDefault(e => 
                e.TimeWorkReductionReconciliationId == timeWorkReductionReconciliationId && 
                e.State == (int)SoeEntityState.Active
                );
        }

        #endregion

        #region TimeWorkReductionReconciliationYear

        private TimeWorkReductionReconciliationYear GetTimeWorkReductionReconciliationYear(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId)
        {
            return entities.TimeWorkReductionReconciliationYear.FirstOrDefault(e =>
                e.TimeWorkReductionReconciliationId == timeWorkReductionReconciliationId &&
                e.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId &&
                e.State == (int)SoeEntityState.Active
                );
        }

        #endregion

        #region TimeWorkReductionReconciliationEmployee

        private List<TimeWorkReductionReconciliationEmployee> GetTimeWorkReductionReconciliationEmployees(int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationEmployeeIds, List<int> employeeIds = null)
        {
            var query = entities.TimeWorkReductionReconciliationEmployee.Where(e => e.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId && e.State == (int)SoeEntityState.Active);
            if (!employeeIds.IsNullOrEmpty())
                query = query.Where(e => employeeIds.Contains(e.EmployeeId));
            else if (!timeWorkReductionReconciliationEmployeeIds.IsNullOrEmpty())
                query = query.Where(e => timeWorkReductionReconciliationEmployeeIds.Contains(e.TimeWorkReductionReconciliationEmployeeId));
            return query.ToList();
        }

        private List<TimeWorkReductionReconciliationEmployee> GetTimeWorkReductionReconciliationEmployees(int employeeId, int? excludeTimeWorkReductionReconciliationEmployeeId, params TermGroup_TimeWorkReductionReconciliationEmployeeStatus[] statuses)
        {
            var query = entities.TimeWorkReductionReconciliationEmployee.Where(e => e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active);
            if (!statuses.IsNullOrEmpty())
                query = query.Where(e => statuses.Select(s => (int)s).Contains(e.Status));
            if (excludeTimeWorkReductionReconciliationEmployeeId != null)
                query = query.Where(e => e.TimeWorkReductionReconciliationEmployeeId != excludeTimeWorkReductionReconciliationEmployeeId.Value);
            return query.ToList();
        }

        private TimeWorkReductionReconciliationYearEmployeeResultRowDTO CalculateTimeWorkReductionReconciliationEmployee(
            Employee employee,
            TimeWorkReductionReconciliation reconciliation,
            TimeWorkReductionReconciliationYear reconciliationYear,
            TimeAccumulator timeAccumulator,
            List<TimeWorkReductionReconciliationEmployee> reconciliationEmployeesForYear,
            List<int> reconciliationEmployeeIds = null
            )
        {
            if (employee == null)
                return null;

            var result = TimeWorkReductionReconciliationYearEmployeeResultRowDTO.Create(
                reconciliation.TimeWorkReductionReconciliationId,
                reconciliationYear.TimeWorkReductionReconciliationYearId,
                employee.EmployeeId,
                employee.EmployeeNrAndName
            );

            if (!reconciliationEmployeesForYear.Any(w => w.EmployeeId == employee.EmployeeId && w.Status >= (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome))
            {
                var existingReconciliationEmployee = (reconciliationEmployeesForYear ?? Enumerable.Empty<TimeWorkReductionReconciliationEmployee>())
                .FirstOrDefault(e =>
                    e.EmployeeId == employee.EmployeeId &&
                    (reconciliationEmployeeIds.IsNullOrEmpty() || reconciliationEmployeeIds.Contains(e.TimeWorkReductionReconciliationEmployeeId))
                );
                if (existingReconciliationEmployee != null)
                    ChangeEntityState(existingReconciliationEmployee, SoeEntityState.Deleted);

                var employeeGroup = employee.GetEmployeeGroup(reconciliationYear.Stop, GetEmployeeGroupsFromCache());
                var employeeGroupRule = employeeGroup != null && timeAccumulator?.TimeAccumulatorEmployeeGroupRule != null ? timeAccumulator.TimeAccumulatorEmployeeGroupRule
                    .FirstOrDefault(eg =>
                        eg.EmployeeGroupId == employeeGroup.EmployeeGroupId &&
                        eg.ThresholdMinutes.HasValue &&
                        eg.State == (int)SoeEntityState.Active
                        ) : null;

                if (employeeGroupRule != null)
                {
                    var ongoingReconciliationEmployees = GetTimeWorkReductionReconciliationEmployees(
                        employee.EmployeeId,
                        existingReconciliationEmployee?.TimeWorkReductionReconciliationEmployeeId,
                        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Created,
                        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated
                        );

                    if (ongoingReconciliationEmployees.IsNullOrEmpty())
                    {
                        int earningMinutes = GetAccumulatedTimeWorkReductionEarningMinutes(reconciliation.TimeAccumulator, employee, reconciliationYear.Stop);

                        var newReconciliationEmployee = TimeWorkReductionReconciliationEmployee.Create(
                            employee,
                            reconciliation,
                            reconciliationYear,
                            earningMinutes,
                            employeeGroupRule.ThresholdMinutes.Value,
                            reconciliation.GetDefaultWithdrawalMethod()
                        );

                        if (newReconciliationEmployee != null)
                        {
                            SetCreatedProperties(existingReconciliationEmployee);
                            entities.AddToTimeWorkReductionReconciliationEmployee(newReconciliationEmployee);
                            newReconciliationEmployee.SetCalculated();
                            result.Succeeded();
                        }
                        else
                        {
                            result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.CalculationFailed);
                        }
                    }
                    else
                        result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.CalculatedInOtherReconsilation, TermGroup_TimeWorkReductionReconciliationEmployeeStatus.NotCalculated);
                }
                else if (existingReconciliationEmployee != null)
                    result.Deleted();
                else
                    result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.NoValidAccumulators);

                if (!Save().Success)
                    result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.SaveFailed);
            }
            else
                result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.InvalidStatus);


            return result;
        }

        #endregion

        #region TimeWorkReductionReconciliationOutcome
        private TimeWorkReductionReconciliationOutcome CreateTimeWorkReductionReconciliationOutcomee(TimeWorkReductionReconciliationEmployee timeWorkReductionReconciliationEmployee)
        {
            if (timeWorkReductionReconciliationEmployee == null)
                return null;

            TimeWorkReductionReconciliationOutcome timeWorkReductionReconciliationOutcome = new TimeWorkReductionReconciliationOutcome()
            {
                TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationEmployee.TimeWorkReductionReconciliationYearId,
                TimeWorkReductionReconciliationEmployeeId = timeWorkReductionReconciliationEmployee.TimeWorkReductionReconciliationEmployeeId,
                EmployeeId = timeWorkReductionReconciliationEmployee.EmployeeId,
                State = (int)SoeEntityState.Active
            };

            SetCreatedProperties(timeWorkReductionReconciliationOutcome);

            if (!Save().Success)
                return null;

            return timeWorkReductionReconciliationOutcome;
        }

        private List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> TimeWorkReductionReconciliationGenerateOutcome(List<Employee> employees, TimeWorkReductionReconciliation timeWorkReductionReconciliation, TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear, List<TimeWorkReductionReconciliationEmployee> timeWorkReductionReconciliationYearEmployees, AttestStateDTO attestStateInitial, DateTime? paymentDate, bool overrideChoosen)
        {
            if (employees.IsNullOrEmpty() || timeWorkReductionReconciliation == null || timeWorkReductionReconciliationYear == null || timeWorkReductionReconciliationYearEmployees.IsNullOrEmpty() || attestStateInitial == null)
                return Empty();

            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> results = new List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO>();

            #region Lazy loaders

            PayrollProduct LoadProduct(int? productId)
            {
                return productId.HasValue ? GetPayrollProductFromCache(productId.Value) : null;
            }
            (bool IsLoaded, PayrollProduct Product) pensionDeposit = (false, null);
            PayrollProduct GetPayrollProductForPensionDeposit()
            {
                if (!pensionDeposit.IsLoaded)
                    pensionDeposit = (true, LoadProduct(timeWorkReductionReconciliationYear.PensionDepositPayrollProductId));
                return pensionDeposit.Product;
            }
            (bool IsLoaded, PayrollProduct Product) directPayment = (false, null);
            PayrollProduct GetPayrollProductForDirectPayment()
            {
                if (!directPayment.IsLoaded)
                    directPayment = (true, LoadProduct(timeWorkReductionReconciliationYear.DirectPaymentPayrollProductId));
                return directPayment.Product;
            }

            #endregion

            List<TimePeriodHead> timePeriodHeadCache = new List<TimePeriodHead>();
            foreach (var timeWorkReductionReconciliationYearEmployeesForEmployee in timeWorkReductionReconciliationYearEmployees.GroupBy(r => r.EmployeeId))
            {
                Employee employee = employees.FirstOrDefault(f => f.EmployeeId == timeWorkReductionReconciliationYearEmployeesForEmployee.Key);
                if (employee == null)
                {
                    Failed(employee, TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeNotFound, TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed);
                    continue;
                }
                TimePeriod timePeriod = null;
                if (paymentDate.HasValue)
                {
                    timePeriod = GetTimePeriod(employee, paymentDate.Value, timePeriodHeadCache);
                    if (timePeriod == null)
                    {
                        Failed(employee, TermGroup_TimeWorkReductionReconciliationResultCode.TimePeriodNotFound, TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed);
                        continue;
                    }
                    if (IsEmployeeTimePeriodLockedForChanges(employee.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                    {
                        Failed(employee, TermGroup_TimeWorkReductionReconciliationResultCode.TimePeriodLocked, TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed);
                        continue;
                    }
                }

                foreach (TimeWorkReductionReconciliationEmployee timeWorkReductionReconciliationYearEmployee in timeWorkReductionReconciliationYearEmployeesForEmployee)
                {
                    TimeWorkReductionReconciliationYearEmployeeResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkReductionWithdrawalMethod)timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod);
                    int selectedWithdrawalMethod = timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod;

                    if (timeWorkReductionReconciliationYearEmployee.Status == (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome)
                        result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeAlreadyGenerated);
                    else if (timeWorkReductionReconciliationYearEmployee.Status < (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated)
                        result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeNotCalculated);
                    else if (timeWorkReductionReconciliationYearEmployee.Status < (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed && !overrideChoosen)
                        result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeHasntChoosen);
                    else
                    {
                        if (timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed && overrideChoosen)
                            timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod = timeWorkReductionReconciliation.DefaultWithdrawalMethod;

                        decimal quantityIn = timeWorkReductionReconciliationYearEmployee.MinutesOverThreshold;

                        if (quantityIn == 0)
                        {
                            result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.NoAmountToWithdraw);
                            timeWorkReductionReconciliationYearEmployee.UpdateStatus(TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome, GetUserDetails());
                            continue;
                        }
                       
                        if (IsWithdrawalMethodDirectPayment(selectedWithdrawalMethod) || (overrideChoosen && IsWithdrawalMethodNotChoosed(selectedWithdrawalMethod) && timeWorkReductionReconciliation.DefaultWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.DirectPayment))
                        {
                            result.UpdateMethod(TermGroup_TimeWorkReductionWithdrawalMethod.DirectPayment);
                            var payrollProduct = GetPayrollProductForDirectPayment();
                            if (payrollProduct == null)
                            {
                                result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.PayrollPeriodDirectPaymentNotFound);
                                continue;
                            }

                            CreatePayrollTransaction(payrollProduct,quantityIn);
                        }

                        else if (IsWithdrawalMethodPensionDeposit(selectedWithdrawalMethod) || (overrideChoosen && IsWithdrawalMethodNotChoosed(selectedWithdrawalMethod) && timeWorkReductionReconciliation.DefaultWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit))
                        {
                            result.UpdateMethod(TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit);
                            var payrollProduct = GetPayrollProductForPensionDeposit();
                            if (payrollProduct == null)
                            {
                                result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.PayrollPeriodPensionDepositNotFound);
                                continue;
                            }
                           
                            CreatePayrollTransaction(payrollProduct, quantityIn);
                        }

                        void CreatePayrollTransaction(PayrollProduct payrollProduct, decimal quantity)
                        {
                            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timeWorkReductionReconciliationYear.Stop, true);
                            TimePayrollTransaction timePayrollTransaction = CreateOrUpdateTimePayrollTransaction(payrollProduct, timeBlockDate, employee, timePeriod?.TimePeriodId ?? null, attestStateInitial.AttestStateId, quantity, 0, 0);
                            if (timePayrollTransaction != null)
                            {
                                SaveTimePayrollTransactionAmounts(timeBlockDate, timePayrollTransaction);
                                TimeWorkReductionReconciliationOutcome timeWorkReductionReconciliationOutcome = CreateTimeWorkReductionReconciliationOutcomee(timeWorkReductionReconciliationYearEmployee);
                                if (timeWorkReductionReconciliationOutcome == null)
                                    result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.GenerationFailed);

                                string userDetails = GetUserDetails();
                                timePayrollTransaction.SetTimeWorkReductionReconciliationOutcome(timeWorkReductionReconciliationOutcome, userDetails);
                                timeWorkReductionReconciliationYearEmployee.UpdateStatus(TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome, userDetails);

                                result.CreateSucess();
                            }
                            else
                                result.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.GenerationFailed);
                        }
                    }
                }
            }

            return Save().Success ? results : ResultsAsSaveFailed();

            TimeWorkReductionReconciliationYearEmployeeResultRowDTO CreateResult(Employee employee, TermGroup_TimeWorkReductionWithdrawalMethod method)
            {
                TimeWorkReductionReconciliationYearEmployeeResultRowDTO result = TimeWorkReductionReconciliationYearEmployeeResultRowDTO.Create(
                    timeWorkReductionReconciliation.TimeWorkReductionReconciliationId,
                    timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    method);
                results.Add(result);
                return result;
            }
            void Failed(Employee employee, TermGroup_TimeWorkReductionReconciliationResultCode code, TermGroup_TimeWorkReductionWithdrawalMethod method)
            {
                CreateResult(employee, method).Failed(code);
            }
            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> Empty()
            {
                return new List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO>();
            }
            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> ResultsAsSaveFailed()
            {
                results.ForEach(r => r.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.GenerationFailed));
                return results;
            }
        }

        private List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> TimeWorkReductionReconciliationReverseTransaction(List<Employee> employees, TimeWorkReductionReconciliation timeWorkReductionReconciliation, TimeWorkReductionReconciliationYear timeWorkReductionReconciliationYear, List<TimeWorkReductionReconciliationEmployee> timeWorkReductionReconciliationYearEmployees, AttestStateDTO attestStateInitial, TermGroup_TimeWorkReductionReconciliationEmployeeStatus status)
        {
            if (employees.IsNullOrEmpty() || timeWorkReductionReconciliation == null || timeWorkReductionReconciliationYear == null || timeWorkReductionReconciliationYearEmployees.IsNullOrEmpty() || attestStateInitial == null)
                return Empty();
            if (status != TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome)
                return Empty();

            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> results = new List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO>();
            List<TimePayrollTransaction> transactions = GetTransactionsForTimeWorkReductionReconciliationWithTimeCodeTransactionAndOutcome(timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationYearId, timeWorkReductionReconciliationYearEmployees.Select(s => s.TimeWorkReductionReconciliationEmployeeId).ToList());

            foreach (Employee employee in employees)
            {
                List<TimeWorkReductionReconciliationEmployee> timeWorkReductionReconciliationYearEmployeesForEmployee = timeWorkReductionReconciliationYearEmployees.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                foreach (var timeWorkReductionReconciliationYearEmployee in timeWorkReductionReconciliationYearEmployeesForEmployee)
                {
                    if (timeWorkReductionReconciliationYearEmployee == null)
                    {
                        Failed(employee, TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeNotFound, TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed);
                        continue;
                    }

                    if (timeWorkReductionReconciliationYearEmployee.Status != (int)status)
                    {
                        TimeWorkReductionReconciliationYearEmployeeResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkReductionWithdrawalMethod)timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod);
                        result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.EmployeeNotGenerated);
                    }
                    else
                    {
                        TimeWorkReductionReconciliationYearEmployeeResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkReductionWithdrawalMethod)timeWorkReductionReconciliationYearEmployee.SelectedWithdrawalMethod);
                        List<TimePayrollTransaction> employeeTransactions = transactions.Where(w => w.TimeWorkReductionReconciliationOutcome != null && w.TimeWorkReductionReconciliationOutcome.TimeWorkReductionReconciliationEmployeeId == timeWorkReductionReconciliationYearEmployee.TimeWorkReductionReconciliationEmployeeId).ToList();

                        if (employeeTransactions.Any(t => t.AttestStateId != attestStateInitial.AttestStateId))
                        {
                            result.UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode.PayrollTransactionWrongState);
                        }
                        else
                        {
                            foreach (var timePayrollTransaction in employeeTransactions)
                            {
                                ChangeEntityState(timePayrollTransaction, SoeEntityState.Deleted);
                                if (timePayrollTransaction.TimeWorkReductionReconciliationOutcome != null)
                                    ChangeEntityState(timePayrollTransaction.TimeWorkReductionReconciliationOutcome, SoeEntityState.Deleted);
                                if (timePayrollTransaction.TimeCodeTransaction != null)
                                    ChangeEntityState(timePayrollTransaction.TimeCodeTransaction, SoeEntityState.Deleted);
                            }
                            timeWorkReductionReconciliationYearEmployee.Status = status == TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome
                                ? (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed
                                : (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome;

                            SetModifiedProperties(timeWorkReductionReconciliationYearEmployee);

                            result.DeleteSuccess();
                        }
                    }
                }
            }

            return Save().Success ? results : ResultsAsSaveFailed();

            TimeWorkReductionReconciliationYearEmployeeResultRowDTO CreateResult(Employee employee, TermGroup_TimeWorkReductionWithdrawalMethod method)
            {
                TimeWorkReductionReconciliationYearEmployeeResultRowDTO result = TimeWorkReductionReconciliationYearEmployeeResultRowDTO.Create(
                    timeWorkReductionReconciliation.TimeWorkReductionReconciliationId,
                    timeWorkReductionReconciliationYear.TimeWorkReductionReconciliationYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    method);
                results.Add(result);
                return result;
            }
            void Failed(Employee employee, TermGroup_TimeWorkReductionReconciliationResultCode code, TermGroup_TimeWorkReductionWithdrawalMethod method)
            {
                CreateResult(employee, method).Failed(code);
            }
            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> Empty()
            {
                return new List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO>();
            }
            List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> ResultsAsSaveFailed()
            {
                results.ForEach(r => r.Failed(TermGroup_TimeWorkReductionReconciliationResultCode.SaveFailed));
                return results;
            }
        }

        private List<TimePayrollTransaction> GetTransactionsForTimeWorkReductionReconciliationWithTimeCodeTransactionAndOutcome(int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationYearEmployeeIds)
        {
            if (timeWorkReductionReconciliationYearEmployeeIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            List<int> timeWorkReductionReconciliationOutcomeIds = GetTimeWorkReductionReconciliationOutcomeIds(timeWorkReductionReconciliationYearId, timeWorkReductionReconciliationYearEmployeeIds).Select(w => w.TimeWorkReductionReconciliationOutcomeId).ToList();

            if (timeWorkReductionReconciliationOutcomeIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeCodeTransaction")
                        .Include("TimeWorkReductionReconciliationOutcome")
                    where t.TimeWorkReductionReconciliationOutcomeId.HasValue &&
                    timeWorkReductionReconciliationOutcomeIds.Contains(t.TimeWorkReductionReconciliationOutcomeId.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }
        private List<TimeWorkReductionReconciliationOutcome> GetTimeWorkReductionReconciliationOutcomeIds(int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationYearEmployeeIds)
        {
            if (timeWorkReductionReconciliationYearEmployeeIds.IsNullOrEmpty())
                return new List<TimeWorkReductionReconciliationOutcome>();

            return (from t in entities.TimeWorkReductionReconciliationOutcome
                    where t.TimeWorkReductionReconciliationYearId == timeWorkReductionReconciliationYearId &&
                    timeWorkReductionReconciliationYearEmployeeIds.Contains(t.TimeWorkReductionReconciliationEmployeeId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }   
        #endregion

        #region Helpers

        private void Beautify(TimeWorkReductionReconciliationYearEmployeeResultDTO result)
        {
            if (result == null || result.Rows.IsNullOrEmpty())
                return;

            foreach (var resultsByCode in result.Rows.GroupBy(r => r.Code))
            {
                string name = GetText((int)resultsByCode.Key, TermGroup.TimeWorkReductionReconciliationResultCode);
                resultsByCode.ToList().ForEach(r => r.UpdateCode(r.Code, name));
            }
            foreach (var resultsByEmployeeStatus in result.Rows.GroupBy(r => r.EmployeeStatus))
            {
                string name = GetText((int)resultsByEmployeeStatus.Key, TermGroup.TimeWorkReductionReconciliationEmployeeStatus);
                resultsByEmployeeStatus.ToList().ForEach(r => r.UpdateEmployeeStatusName(r.EmployeeStatus, name));
            }
            foreach (var resultsByMethod in result.Rows.GroupBy(r => r.Method))
            {
                string name = GetText((int)resultsByMethod.Key, TermGroup.TimeWorkReductionWithdrawalMethod);
                resultsByMethod.ToList().ForEach(r => r.UpdateMethod(r.Method, name));
            }
        }
        private bool IsWithdrawalMethodDirectPayment(int selectedWithdrawalMethod)
        {
            return selectedWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.DirectPayment;
        }
        private bool IsWithdrawalMethodPensionDeposit(int selectedWithdrawalMethod)
        {
            return selectedWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit;
        }
        private bool IsWithdrawalMethodNotChoosed(int selectedWithdrawalMethod)
        {
            return selectedWithdrawalMethod == (int)TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed;
        }
        #endregion

    }
}
