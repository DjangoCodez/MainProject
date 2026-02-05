using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.BatchHelper;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private CalculateTimeWorkAccountYearEmployeeOutputDTO TaskCalculateTimeWorkAccountYearEmployee()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountYearEmployeeInputDTO, CalculateTimeWorkAccountYearEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    var timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    var timeWorkAccountYear = GetTimeWorkAccountYearWithWorkTimeWeek(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }
                    if (!timeWorkAccountYear.HasWorkTimeWeeks() && timeWorkAccount.UsePaidLeave)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92011, "Årskörning saknar arbetsmått intervall"));
                        return oDTO;
                    }

                    var allEmployeeTimeWorkAccounts = GetTimeWorkAccountsForEmployees(iDTO.TimeWorkAccountId, iDTO.EmployeeIds);
                    var allTimeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds, iDTO.EmployeeIds);
                    var employeeIds = allEmployeeTimeWorkAccounts
                        .Select(e => e.EmployeeId)
                        .Concat(allTimeWorkAccountYearEmployees.Select(e => e.EmployeeId))
                        .Distinct()
                        .ToList();

                    if (employeeIds.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    foreach (int employeeId in employeeIds)
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            oDTO.FunctionResult.Rows.AddRange(CalculateTimeWorkAccountYearEmployee(
                                employeeId, 
                                timeWorkAccount,
                                timeWorkAccountYear,
                                allEmployeeTimeWorkAccounts,
                                allTimeWorkAccountYearEmployees, 
                                timeWorkAccountYearEmployeeIds: iDTO.TimeWorkAccountYearEmployeeIds
                                )
                            );

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

        private CalculateTimeWorkAccountYearEmployeeBasisOutputDTO TaskCalculateTimeWorkAccountYearEmployeeBasis()
        {
            var (iDTO, oDTO) = InitTask<TaskCalculateTimeWorkAccountYearEmployeeBasisInputDTO, CalculateTimeWorkAccountYearEmployeeBasisOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    oDTO.Basis = CalculateTimeWorkAccountYearEmployeesBasis(iDTO.EmployeeIds, iDTO.DateFrom, iDTO.DateTo);
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
            }

            return oDTO;
        }

        private TimeWorkAccountChoiceSendXEMailOutputDTO TaskTimeWorkAccountChoiceSendXEMail()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountYearEmployeeInputDTO, TimeWorkAccountChoiceSendXEMailOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }

                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds);
                    if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    List<int> timeWorkAccountYearEmployeeIds = (timeWorkAccountYearEmployees.Select(e => e.TimeWorkAccountYearEmployeeId)).Distinct().ToList();

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.FunctionResult.Rows = TimeWorkAccountChoiceSendXEMail(timeWorkAccountYearEmployees, timeWorkAccountYear.EmployeeLastDecidedDate, iDTO.TimeWorkAccountId, iDTO.TimeWorkAccountYearId, timeWorkAccountYearEmployeeIds);
                        if (oDTO.FunctionResult.Rows.Any(w => w.Code == TermGroup_TimeWorkAccountYearSendMailCode.Succeeded))
                        {
                            oDTO.Result = SetSentTimeWorkAccountYearEmployee(timeWorkAccountYearEmployees, oDTO.FunctionResult.Rows.Where(w => w.Code == TermGroup_TimeWorkAccountYearSendMailCode.Succeeded).Select(s => s.TimeWorkAccountYearEmployeeId).ToList());
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

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

        private TimeWorkAccountTransactionOutputDTO TaskTimeWorkAccountGenerateOutcome()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountGenerateOutcomeInputDTO, TimeWorkAccountTransactionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }
                    if (timeWorkAccount.UseDirectPayment && !HasPayrollProductFromCache(timeWorkAccountYear.DirectPaymentPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92012, "Löneart för kontant ersättning ej angivet"));
                        return oDTO;
                    }
                    if (timeWorkAccount.UsePensionDeposit && !HasPayrollProductFromCache(timeWorkAccountYear.PensionDepositPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92013, "Löneart för pensionspremie ej angivet"));
                        return oDTO;
                    }

                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds);
                    if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkAccountYearEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(TimeWorkAccountGenerateOutcome(employees.Where(x => batchHelper.GetCurrentBatchIds().Contains(x.EmployeeId)).ToList(), timeWorkAccount, timeWorkAccountYear, timeWorkAccountYearEmployees, attestStateInitial, iDTO.PaymentDate, iDTO.OverrideChoosen));
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

        private TimeWorkAccountTransactionOutputDTO TaskTimeWorkAccountReverseTransaction()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountYearEmployeeInputDTO, TimeWorkAccountTransactionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }

                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds);
                    if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkAccountYearEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            List<int> currentEmployeeIds = batchHelper.GetCurrentBatchIds();
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(SaveTimeWorkAccountReverseTransaction(employees.Where(x => currentEmployeeIds.Contains(x.EmployeeId)).ToList(), timeWorkAccount, timeWorkAccountYear, timeWorkAccountYearEmployees.Where(x => currentEmployeeIds.Contains(x.EmployeeId)).ToList(), attestStateInitial, TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome));
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

        private TimeWorkAccountTransactionOutputDTO TaskTimeWorkAccountGenerateUnusedPaidBalance()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountGenerateOutcomeInputDTO, TimeWorkAccountTransactionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }
                    if (timeWorkAccount.UseDirectPayment && !HasPayrollProductFromCache(timeWorkAccountYear.DirectPaymentPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92012, "Löneart för kontant ersättning ej angivet"));
                        return oDTO;
                    }
                    if (timeWorkAccount.UsePensionDeposit && !HasPayrollProductFromCache(timeWorkAccountYear.PensionDepositPayrollProductId))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92013, "Löneart för pensionspremie ej angivet"));
                        return oDTO;
                    }

                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds);
                    if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkAccountYearEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    List<TimeAccumulator> timeAccumulators = GetTimeAccumulatorsForTimeWorkAccount();
                    if (timeAccumulators.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92021, "Inget saldo hittades"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(SaveTimeWorkAccountGenerateUnusedPaidBalance(employees.Where(x => batchHelper.GetCurrentBatchIds().Contains(x.EmployeeId)).ToList(), timeWorkAccount, timeWorkAccountYear, timeWorkAccountYearEmployees, attestStateInitial, iDTO.PaymentDate, timeAccumulators));
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

        private TimeWorkAccountTransactionOutputDTO TaskTimeWorkAccountYearReversePaidBalance()
        {
            var (iDTO, oDTO) = InitTask<TimeWorkAccountYearEmployeeInputDTO, TimeWorkAccountTransactionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(iDTO.TimeWorkAccountId);
                    if (timeWorkAccount == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91994, "Arbettidskonto hittades inte"));
                        return oDTO;
                    }

                    TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYear(iDTO.TimeWorkAccountYearId);
                    if (timeWorkAccountYear == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91995, "Årskörning hittades inte"));
                        return oDTO;
                    }

                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(timeWorkAccountYear.TimeWorkAccountYearId, iDTO.TimeWorkAccountYearEmployeeIds);
                    if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment((timeWorkAccountYearEmployees.Select(e => e.EmployeeId)).Distinct().ToList());
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91996, "Inga anställda kopplade till arbetstidskontot"));
                        return oDTO;
                    }

                    #endregion

                    BatchHelper batchHelper = BatchHelper.Create(employees.Select(s => s.EmployeeId).ToList(), 100);
                    while (batchHelper.HasMoreBatches())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            List<int> currentEmployeeIds = batchHelper.GetCurrentBatchIds();
                            InitTransaction(transaction);
                            oDTO.FunctionResult.Rows.AddRange(SaveTimeWorkAccountReverseTransaction(employees.Where(x => currentEmployeeIds.Contains(x.EmployeeId)).ToList(), timeWorkAccount, timeWorkAccountYear, timeWorkAccountYearEmployees.Where(x => currentEmployeeIds.Contains(x.EmployeeId)).ToList(), attestStateInitial, TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance));
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

        #region TimeWorkAccount

        private TimeWorkAccount GetTimeWorkAccount(int timeWorkAccountId)
        {
            return entities.TimeWorkAccount.FirstOrDefault(e => e.TimeWorkAccountId == timeWorkAccountId && e.State == (int)SoeEntityState.Active);
        }

        #endregion

        #region TimeWorkAccountYear

        private List<TimeWorkAccountYear> GetTimeWorkAccountYearsWithWorkTimeWeek()
        {
            return entities.TimeWorkAccountYear.Include("TimeWorkAccountWorkTimeWeek").Where(e => e.TimeWorkAccount.ActorCompanyId == actorCompanyId && e.State == (int)SoeEntityState.Active).ToList();
        }

        private List<TimeWorkAccountYear> GetTimeWorkAccountYearsWithWorkTimeWeek(DateTime dateFrom, DateTime dateTo)
        {
            return GetTimeWorkAccountYearsWithWorkTimeWeek().Where(y => CalendarUtility.IsDatesOverlapping(y.EarningStart, y.EarningStop, dateFrom, dateTo, validateDatesAreTouching: true)).ToList();
        }

        private TimeWorkAccountYear GetTimeWorkAccountYear(int timeWorkAccountYearId)
        {
            return entities.TimeWorkAccountYear
                .FirstOrDefault(e => e.TimeWorkAccountYearId == timeWorkAccountYearId && e.State == (int)SoeEntityState.Active);
        }
        
        private TimeWorkAccountYear GetTimeWorkAccountYearWithEmployees(int timeWorkAccountYearId)
        {
            return entities.TimeWorkAccountYear.Include("TimeWorkAccountYearEmployee")
                .FirstOrDefault(e => e.TimeWorkAccountYearId == timeWorkAccountYearId && e.State == (int)SoeEntityState.Active);
        }
        
        private TimeWorkAccountYear GetLatestTimeWorkAccountYearWithAccount(int timeWorkAccountId, DateTime date)
        {
            return entities.TimeWorkAccountYear
                .Include(e => e.TimeWorkAccount)
                .Where(e => e.TimeWorkAccountId == timeWorkAccountId)
                .OrderByDescending(e => e.EarningStart)
                .FirstOrDefault(e => e.EarningStart < date && e.State == (int)SoeEntityState.Active);
        }

        private TimeWorkAccountYear GetTimeWorkAccountYearWithWorkTimeWeek(int timeWorkAccountYearId)
        {
            return entities.TimeWorkAccountYear.Include("TimeWorkAccountWorkTimeWeek")
                .FirstOrDefault(e => e.TimeWorkAccountYearId == timeWorkAccountYearId && e.State == (int)SoeEntityState.Active);
        }
        
        #endregion

        #region TimeWorkAccountYearOutcome

        private TimeWorkAccountYearOutcome GetTimeWorkAccountYearOutcome(int timeWorkAccountYearOutcomeId)
        {
            return (from t in entities.TimeWorkAccountYearOutcome
                    where t.TimeWorkAccountYearOutcomeId == timeWorkAccountYearOutcomeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private List<TimeWorkAccountYearOutcome> GetTimeWorkAccountYearOutcomeIds(int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds, SoeTimeWorkAccountYearOutcomeType type)
        {
            if (timeWorkAccountYearEmployeeIds.IsNullOrEmpty())
                return new List<TimeWorkAccountYearOutcome>();

            return (from t in entities.TimeWorkAccountYearOutcome
                    where t.TimeWorkAccountYearId == timeWorkAccountYearId &&
                    timeWorkAccountYearEmployeeIds.Contains(t.TimeWorkAccountYearEmployeeId) &&
                    t.Type == (int)type &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private TimeWorkAccountYearOutcome CreateTimeWorkAccountYearOutcome(TimeWorkAccountYearEmployee timeWorkAccountYearEmployee, SoeTimeWorkAccountYearOutcomeType type)
        {
            if (timeWorkAccountYearEmployee == null)
                return null;

            TimeWorkAccountYearOutcome timeWorkAccountYearOutcome = new TimeWorkAccountYearOutcome()
            {
                TimeWorkAccountYearId = timeWorkAccountYearEmployee.TimeWorkAccountYearId,
                TimeWorkAccountYearEmployeeId = timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId,
                EmployeeId = timeWorkAccountYearEmployee.EmployeeId,
                Type = (int)type,
                State = (int)SoeEntityState.Active
            };

            SetCreatedProperties(timeWorkAccountYearOutcome);

            if (!Save().Success)
                return null;

            return timeWorkAccountYearOutcome;
        }

        #endregion

        #region TimeWorkAccountYearEmployee

        private List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds, List<int> employeeIds = null)
        {
            var query = entities.TimeWorkAccountYearEmployee.Where(e => e.TimeWorkAccountYearId == timeWorkAccountYearId && e.State == (int)SoeEntityState.Active);
            if (!employeeIds.IsNullOrEmpty())
                query = query.Where(e => employeeIds.Contains(e.EmployeeId));
            else if (!timeWorkAccountYearEmployeeIds.IsNullOrEmpty())
                query = query.Where(e => timeWorkAccountYearEmployeeIds.Contains(e.TimeWorkAccountYearEmployeeId));
            return query.ToList();
        }

        public List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(int employeeId)
        {
            return entities.TimeWorkAccountYearEmployee.Where(e => e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active).ToList();
        }

        public List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployeesWithYear(int employeeId)
        {
            return entities.TimeWorkAccountYearEmployee.Include(e => e.TimeWorkAccountYear).Where(e => e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active).ToList();
        }

        private List<TimeWorkAccountYearEmployee> GetTimeWorkAccountYearEmployees(int employeeId, int timeWorkAccountYearId)
        {
            return entities.TimeWorkAccountYearEmployee.Where(e => e.EmployeeId == employeeId && e.TimeWorkAccountYearId == timeWorkAccountYearId && e.State == (int)SoeEntityState.Active).ToList();
        }

        private ActionResult SetSentTimeWorkAccountYearEmployee(List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees, List<int> timeWorkAccountYearEmployeeIds)
        {
            DateTime sentDate = DateTime.Now;

            if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                return new ActionResult(true);

            foreach (int timeWorkAccountYearEmployeeId in timeWorkAccountYearEmployeeIds)
            {
                TimeWorkAccountYearEmployee employeeRow = timeWorkAccountYearEmployees.FirstOrDefault(w => w.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId);
                if (employeeRow != null)
                {
                    employeeRow.SentDate = sentDate;
                    SetModifiedProperties(employeeRow);
                }

            }

            return Save();

        }

        private List<TimePayrollTransaction> GetTransactionsForTimeWorkAccountWithTimeCodeTransactionAndOutcome(int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds, SoeTimeWorkAccountYearOutcomeType type)
        {
            if (timeWorkAccountYearEmployeeIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            List<int> timeWorkAccountYearOutcomeIds = GetTimeWorkAccountYearOutcomeIds(timeWorkAccountYearId, timeWorkAccountYearEmployeeIds, type).Select(w => w.TimeWorkAccountYearOutcomeId).ToList();

            if (timeWorkAccountYearOutcomeIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeCodeTransaction")
                        .Include("TimeWorkAccountYearOutcome")
                    where t.TimeWorkAccountYearOutcomeId.HasValue &&
                    timeWorkAccountYearOutcomeIds.Contains(t.TimeWorkAccountYearOutcomeId.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeWorkAccountYearEmployeeResultRowDTO> CalculateTimeWorkAccountYearEmployee(
            int employeeId, 
            TimeWorkAccount timeWorkAccount,
            TimeWorkAccountYear timeWorkAccountYear, 
            List<EmployeeTimeWorkAccount> allEmployeeTimeWorkAccounts, 
            List<TimeWorkAccountYearEmployee> allTimeWorkAccountYearEmployees, 
            TimePeriod finalSalaryTimePeriod = null,
            List<int> timeWorkAccountYearEmployeeIds = null
            )
        {
            var employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return Empty();

            var results = new List<TimeWorkAccountYearEmployeeResultRowDTO>();

            var employeeTimeWorkAccounts = allEmployeeTimeWorkAccounts?.Where(e => e.EmployeeId == employeeId).ToList() ?? new List<EmployeeTimeWorkAccount>();
            var yearEmployees = (allTimeWorkAccountYearEmployees ?? Enumerable.Empty<TimeWorkAccountYearEmployee>())
                .Where(e =>
                    e.EmployeeId == employeeId &&
                    (timeWorkAccountYearEmployeeIds == null || timeWorkAccountYearEmployeeIds.Contains(e.TimeWorkAccountYearEmployeeId)))
                .ToList();

            if (!employeeTimeWorkAccounts.IsNullOrEmpty())
            {
                var allDeletedEmployeeYearIds = new List<int>();
                foreach (var employeeTimeWorkAccount in employeeTimeWorkAccounts)
                {
                    if (!employeeTimeWorkAccount.IsValid(out DateTime earningStart, out DateTime earningStop, employee, timeWorkAccountYear))
                        continue;
                    if (timeWorkAccountYearEmployeeIds != null && !yearEmployees.Exists(w => CalendarUtility.IsDatesOverlapping(earningStart, earningStop, w.EarningStart, w.EarningStop)))
                        continue;

                    var result = CreateResult(employeeTimeWorkAccount);
                    if (TryDeleteExistingEmployeeYears(yearEmployees, earningStart, earningStop, timeWorkAccountYearEmployeeIds, out TermGroup_TimeWorkAccountYearEmployeeStatus status, out List<int> deletedEmployeeYearIds))
                    {
                        if (!deletedEmployeeYearIds.IsNullOrEmpty())
                            allDeletedEmployeeYearIds.AddRange(deletedEmployeeYearIds);

                        var yearEmployee = CalculateTimeWorkAccountYearEmployee(employee, timeWorkAccount, timeWorkAccountYear, employeeTimeWorkAccount, earningStart, earningStop, finalSalaryTimePeriod);
                        if (yearEmployee != null)
                            result.Succeeded();
                        else
                            result.Failed(TermGroup_TimeWorkAccountYearResultCode.CalculationFailed);
                    }
                    else
                    {
                        result.Failed(TermGroup_TimeWorkAccountYearResultCode.InvalidStatus, status);
                    }
                }
                if (timeWorkAccountYearEmployeeIds == null || timeWorkAccountYearEmployeeIds.Any())
                {
                    foreach (var existingYearEmployee in yearEmployees.Where(e => e.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated))
                    {
                        if (!allDeletedEmployeeYearIds.Contains(existingYearEmployee.TimeWorkAccountYearEmployeeId))
                        {
                            DeleteExistingEmployeeYear(existingYearEmployee);
                            Deleted();
                        }
                    }
                }
            }
            else if (!yearEmployees.IsNullOrEmpty())
            {
                if (TryDeleteExistingEmployeeYears(yearEmployees, timeWorkAccountYear.EarningStart, timeWorkAccountYear.EarningStop, timeWorkAccountYearEmployeeIds, out TermGroup_TimeWorkAccountYearEmployeeStatus status, out List<int> deletedEmployeeYearIds))
                {
                    if (!deletedEmployeeYearIds.IsNullOrEmpty())
                        return Save().Success ? Deleted() : Failed(TermGroup_TimeWorkAccountYearResultCode.SaveFailed);
                }
                else
                    return Failed(TermGroup_TimeWorkAccountYearResultCode.InvalidStatus, status);
            }

            if (results.IsNullOrEmpty())
                return Failed(TermGroup_TimeWorkAccountYearResultCode.NoValidAccounts);
            if (!Save().Success)
                return Failed(TermGroup_TimeWorkAccountYearResultCode.SaveFailed);

            return results;

            TimeWorkAccountYearEmployeeResultRowDTO CreateResult(EmployeeTimeWorkAccount employeeTimeWorkAccount = null)
            {
                var result = TimeWorkAccountYearEmployeeResultRowDTO.Create(
                    timeWorkAccountYear.TimeWorkAccountId,
                    timeWorkAccountYear.TimeWorkAccountYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    employeeTimeWorkAccount?.DateFrom,
                    employeeTimeWorkAccount?.DateTo
                    );
                results.Add(result);
                return result;
            }
            List<TimeWorkAccountYearEmployeeResultRowDTO> Failed(TermGroup_TimeWorkAccountYearResultCode resultCode, TermGroup_TimeWorkAccountYearEmployeeStatus employeeStatus = TermGroup_TimeWorkAccountYearEmployeeStatus.NotCalculated)
            {
                return CreateResult().Failed(resultCode, employeeStatus).ObjToList();
            }
            List<TimeWorkAccountYearEmployeeResultRowDTO> Deleted()
            {
                return CreateResult().Deleted().ObjToList();
            }
            List<TimeWorkAccountYearEmployeeResultRowDTO> Empty()
            {
                return new List<TimeWorkAccountYearEmployeeResultRowDTO>();
            }
        }

        private TimeWorkAccountYearEmployee CalculateTimeWorkAccountYearEmployee(
            Employee employee, 
            TimeWorkAccount timeWorkAccount, 
            TimeWorkAccountYear timeWorkAccountYear, 
            EmployeeTimeWorkAccount employeeTimeWorkAccount, 
            DateTime earningStart, 
            DateTime earningStop, 
            TimePeriod finalSalaryTimePeriod = null
            )
        {
            try
            {
                TimeWorkAccountYearEmployeeCalculation calculationBasis = CalculateTimeWorkAccountYearEmployeeBasis(employee, timeWorkAccount, timeWorkAccountYear, earningStart, earningStop, finalSalaryTimePeriod);
                if (calculationBasis != null)
                {
                    TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = TimeWorkAccountYearEmployee.Create(employeeTimeWorkAccount, timeWorkAccountYear, earningStart, earningStop);
                    if (timeWorkAccountYearEmployee != null)
                    {
                        timeWorkAccountYearEmployee.SetCalculated(calculationBasis);
                        entities.AddToTimeWorkAccountYearEmployee(timeWorkAccountYearEmployee);
                        SetCreatedProperties(timeWorkAccountYearEmployee);
                        return timeWorkAccountYearEmployee;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return null;
        }

        private TimeWorkAccountYearEmployeeCalculation CalculateTimeWorkAccountYearEmployeeBasis(Employee employee, TimeWorkAccount timeWorkAccount, TimeWorkAccountYear timeWorkAccountYear, DateTime earningStart, DateTime earningStop, TimePeriod finalSalaryTimePeriod = null)
        {
            if (employee == null || timeWorkAccount == null || timeWorkAccountYear == null || earningStart > earningStop)
                return TimeWorkAccountYearEmployeeCalculation.Empty;

            List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriodsBasedOnPayrollStopDateWithTimePeriod(employee.EmployeeId, earningStart, earningStop);
            List<int> timePeriodIds = employeeTimePeriods.Where(p => p.Status == (int)SoeEmployeeTimePeriodStatus.Locked || p.Status == (int)SoeEmployeeTimePeriodStatus.Paid).Select(p => p.TimePeriodId).Distinct().ToList();
            if (finalSalaryTimePeriod != null && !timePeriodIds.Contains(finalSalaryTimePeriod.TimePeriodId))
                timePeriodIds.AddIfNotNull(GetEmployeeTimePeriod(finalSalaryTimePeriod.TimePeriodId, employee.EmployeeId)?.TimePeriodId);

            var transactionsForTimePeriods = LoadTransactions(employee.EmployeeId, timePeriodIds);
            var transactionsForEarningPeriod = timeWorkAccount.UsePaidLeave ? LoadPayrollTransactions(employee.EmployeeId, earningStart, earningStop, transactionsForTimePeriods.Payroll) : new List<GetTimePayrollTransactionsForEmployee_Result>();
            var transactionDates = GetTimeWorkAccountTransactionDates(transactionsForTimePeriods, earningStart, earningStop);

            List<int> productIds = transactionsForTimePeriods.Payroll.GetProductIds().Concat(transactionsForTimePeriods.Schedule.GetProductIds()).Distinct().ToList();
            List<PayrollProductSetting> workTimePromotedSettings = GetPayrollProductWithSettingsFromCache(productIds).GetSettings().Where(s => s.WorkingTimePromoted).ToList();
            List<DateRangeDTO> payrollGroupRanges = GetEmploymentChangeRanges(TermGroup_EmploymentChangeFieldType.PayrollGroupId, employee.EmployeeId, transactionDates.Start, transactionDates.Stop);
            List<DateRangeDTO> workTimeWeekRanges = GetEmploymentChangeRanges(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, employee.EmployeeId, transactionDates.Start, transactionDates.Stop);

            return TimeWorkAccountYearEmployeeCalculation.Create(
                employee.EmployeeId,
                timeWorkAccount,
                timeWorkAccountYear,
                CreateTimeWorkAccountYearEmployeeTransactions(transactionsForTimePeriods.Payroll, payrollGroupRanges, workTimePromotedSettings),
                CreateTimeWorkAccountYearEmployeeTransactions(transactionsForTimePeriods.Schedule, payrollGroupRanges, workTimePromotedSettings),
                CreateTimeWorkAccountYearEmployeeDays(transactionsForEarningPeriod, workTimeWeekRanges, employee, timeWorkAccount, timeWorkAccountYear)
                );
        }

        private List<TimeWorkAccountYearEmployeeCalculation> CalculateTimeWorkAccountYearEmployeesBasis(List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            if (employeeIds.IsNullOrEmpty() || dateFrom > dateTo)
                return new List<TimeWorkAccountYearEmployeeCalculation>();

            List<TimeWorkAccountYear> timeWorkAccountYears = GetTimeWorkAccountYearsWithWorkTimeWeek(dateFrom, dateTo);
            if (timeWorkAccountYears.IsNullOrEmpty())
                return new List<TimeWorkAccountYearEmployeeCalculation>();

            List<int> timeWorkAccountIds = timeWorkAccountYears.Select(y => y.TimeWorkAccountId).Distinct().ToList();
            Dictionary<int, TimeWorkAccount> timeWorkAccountsById = GetTimeWorkAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId))
                .Where(t => timeWorkAccountIds.Contains(t.TimeWorkAccountId))
                .ToDictionary(k => k.TimeWorkAccountId, v => v);
            if (timeWorkAccountsById.IsNullOrEmpty())
                return new List<TimeWorkAccountYearEmployeeCalculation>();

            Dictionary<int, List<EmployeeTimeWorkAccount>> employeeTimeWorkAccountsByEmployee = GetTimeWorkAccountsForEmployees(timeWorkAccountIds, employeeIds).GroupBy(t => t.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            if (timeWorkAccountYears.IsNullOrEmpty())
                return new List<TimeWorkAccountYearEmployeeCalculation>();

            List<TimeWorkAccountYearEmployeeCalculation> basis = new List<TimeWorkAccountYearEmployeeCalculation>();

            foreach (int employeeId in employeeIds)
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                if (employee == null)
                    continue;

                List<EmployeeTimeWorkAccount> employeeTimeWorkAccounts = employeeTimeWorkAccountsByEmployee.GetList(employee.EmployeeId);
                if (employeeTimeWorkAccounts.IsNullOrEmpty())
                    continue;

                foreach (EmployeeTimeWorkAccount employeeTimeWorkAccount in employeeTimeWorkAccounts)
                {
                    foreach (TimeWorkAccountYear timeWorkAccountYear in timeWorkAccountYears.Where(y => y.TimeWorkAccountId == employeeTimeWorkAccount.TimeWorkAccountId))
                    {
                        TimeWorkAccount timeWorkAccount = timeWorkAccountsById.GetValue(employeeTimeWorkAccount.TimeWorkAccountId);
                        if (timeWorkAccount == null)
                            continue;
                        if (!employeeTimeWorkAccount.IsValid(out DateTime earningStart, out DateTime earningStop, employee, timeWorkAccountYear, dateFrom, dateTo))
                            continue;

                        basis.Add(CalculateTimeWorkAccountYearEmployeeBasis(employee, timeWorkAccount, timeWorkAccountYear, earningStart, earningStop));
                    }
                }
            }

            return basis;
        }

        public List<TimeWorkAccountYearEmployeeTransaction> CreateTimeWorkAccountYearEmployeeTransactions(IEnumerable<ITransactionProc> transactions, List<DateRangeDTO> dateRanges, List<PayrollProductSetting> settings)
        {
            if (dateRanges.IsNullOrEmpty() || settings.IsNullOrEmpty())
                return new List<TimeWorkAccountYearEmployeeTransaction>();

            var result = new List<TimeWorkAccountYearEmployeeTransaction>();
            foreach (DateRangeDTO dateRange in dateRanges)
            {
                foreach (var timePayrollTransactionsInRangeByProduct in transactions.Filter(dateRange.Start, dateRange.Stop).GroupBy(t => t.ProductId))
                {
                    PayrollProductSetting setting = settings.GetSetting(StringUtility.GetNullableInt(dateRange.Value), timePayrollTransactionsInRangeByProduct.Key, getDefaultIfNotFound: true);
                    if (setting != null)
                        result.AddRange(timePayrollTransactionsInRangeByProduct.ToTimeWorkAccountEmployeeTransaction());
                }
            }
            return result;
        }

        public List<TimeWorkAccountYearEmployeeDay> CreateTimeWorkAccountYearEmployeeDays(IEnumerable<IPayrollTransactionProc> transactions, List<DateRangeDTO> dateRanges, Employee employee, TimeWorkAccount timeWorkAccount, TimeWorkAccountYear timeWorkAccountYear)
        {
            if (timeWorkAccount == null || !timeWorkAccount.UsePaidLeave || timeWorkAccountYear == null || transactions.IsNullOrEmpty() || dateRanges.IsNullOrEmpty())
                return new List<TimeWorkAccountYearEmployeeDay>();

            var result = new List<TimeWorkAccountYearEmployeeDay>();
            var yearEarningDays = timeWorkAccountYear.GetEarningDays();
            var earningYearTransactionsByDate = transactions.GroupBy(t => t.Date).ToDictionary(k => k.Key, v => v.ToList());

            var employeeTimePeriods = GetEmployeeTimePeriodsFromCache(employee.EmployeeId);
            var timePeriods = new List<TimePeriodDTO>();
            TimePeriodDTO GetTimePeriodDTO(TimePeriod timePeriod)
            {
                if (timePeriod == null)
                    return null;

                var dto = timePeriods.FirstOrDefault(t => t.TimePeriodId == timePeriod.TimePeriodId);
                if (dto == null)
                {
                    dto = timePeriod.ToDTO();
                    timePeriods.Add(dto);
                }
                return dto;
            }

            (DateRangeDTO Range, TimePeriod TimePeriod, int RuleWorkTimeWeek, int WorkTimeWeek, decimal EmploymentPercent, int PaidLeaveTimeMinutes) current = (null, null, 0, 0, 0, 0);
            (DateRangeDTO, TimePeriod, int, int, decimal, int) GetCurrentRange(DateTime date)
            {
                var range = dateRanges.Find(r => r.IsValid(date));
                var employment = employee.GetEmployment(date);
                var timePeriod = employeeTimePeriods.GetTimePeriod(date);
                int ruleWorkTimeWeek = employment?.GetFullTimeWorkTimeWeek(employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache()), date) ?? 0;
                int workTimeWeekMinutes = StringUtility.GetInt(range?.Value);
                decimal employmentPercent = employee.GetEmployment(date)?.GetPercent(date) ?? Decimal.Zero;
                int paidLeaveTimeMinutes = timeWorkAccountYear.GetPaidLeaveMinutes(ruleWorkTimeWeek);
                return (range, timePeriod, ruleWorkTimeWeek, workTimeWeekMinutes, employmentPercent, paidLeaveTimeMinutes);
            }

            List<PayrollStartValueRow> startValueRows = GetPayrollStartValueRowsForEmployee(employee.EmployeeId, dateRanges.Min(r => r.Start), dateRanges.Max(r => r.Stop));

            DateTime currentDate = CalendarUtility.GetEarliestDate(timeWorkAccountYear.EarningStart.Date, transactions.Where(t => t.TimePeriodId.HasValue).ToNullIfEmpty()?.Min(d => d.Date));
            while (currentDate <= timeWorkAccountYear.EarningStop)
            {
                if (current.Range == null || !current.Range.IsValid(currentDate))
                    current = GetCurrentRange(currentDate);

                result.Add(TimeWorkAccountYearEmployeeDay.Create(
                    currentDate,
                    current.EmploymentPercent,
                    current.RuleWorkTimeWeek,
                    current.WorkTimeWeek,
                    current.PaidLeaveTimeMinutes,
                    yearEarningDays,
                    employee.HasEmployment(currentDate),
                    GetTimePeriodDTO(current.TimePeriod),
                    CalculateUnpaidAbsenceRatio()));
                currentDate = currentDate.Date.AddDays(1);
            }

            return result;

            decimal CalculateUnpaidAbsenceRatio()
            {
                decimal unpaidAbsenceQuantity = earningYearTransactionsByDate.GetList(currentDate).Where(t => t.IsAbsenceNoVacation()).Sum(t => t.Quantity);
                if (unpaidAbsenceQuantity <= 0)
                    return 0;

                decimal scheduleMinutes = startValueRows.FirstOrDefault(r => r.Date == currentDate)?.ScheduleTimeMinutes ?? GetScheduleBlocksForEmployee(null, employee.EmployeeId, currentDate).GetWorkMinutes();
                if (scheduleMinutes <= 0)
                    return 0;

                if (unpaidAbsenceQuantity >= scheduleMinutes)
                    return 1;

                return Decimal.Round(Decimal.Divide(unpaidAbsenceQuantity, scheduleMinutes), 2);
            }
        }

        private bool TryDeleteExistingEmployeeYears(List<TimeWorkAccountYearEmployee> employeeYears, DateTime earningStart, DateTime earningStop, List<int> timeWorkAccountYearEmployeeIds, out TermGroup_TimeWorkAccountYearEmployeeStatus status, out List<int> deletedEmployeeYearIds)
        {
            deletedEmployeeYearIds = new List<int>();
            if (!employeeYears.IsNullOrEmpty())
            {
                List<TimeWorkAccountYearEmployee> employeeYearsOverlapping = employeeYears.GetOverlapping(earningStart, earningStop);
                foreach (TimeWorkAccountYearEmployee employeeYear in employeeYearsOverlapping)
                {
                    if (!timeWorkAccountYearEmployeeIds.IsNullOrEmpty() && !timeWorkAccountYearEmployeeIds.Contains(employeeYear.TimeWorkAccountYearEmployeeId))
                        continue;

                    if (!employeeYear.IsValidToRecalculate(earningStart, earningStop))
                    {
                        status = (TermGroup_TimeWorkAccountYearEmployeeStatus)employeeYear.Status;
                        return false;
                    }

                    DeleteExistingEmployeeYear(employeeYear);
                    deletedEmployeeYearIds.Add(employeeYear.TimeWorkAccountYearEmployeeId);
                }
            }
            status = TermGroup_TimeWorkAccountYearEmployeeStatus.NotCalculated;
            return true;
        }

        private void DeleteExistingEmployeeYear(TimeWorkAccountYearEmployee employeeYear)
        {
            if (employeeYear != null)
                ChangeEntityState(employeeYear, SoeEntityState.Deleted);
        }

        private List<TimeWorkAccountChoiceResultRowDTO> TimeWorkAccountChoiceSendXEMail(List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees, DateTime lastDate, int timeWorkAccountId, int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds)
        {
            if (timeWorkAccountYearEmployees == null)
                return new List<TimeWorkAccountChoiceResultRowDTO>();

            List<TimeWorkAccountChoiceResultRowDTO> results = new List<TimeWorkAccountChoiceResultRowDTO>();
            User sender = GetUserFromCache();

            foreach (int timeWorkAccountYearEmployeeId in timeWorkAccountYearEmployeeIds)
            {
                int employeeId = timeWorkAccountYearEmployees.FirstOrDefault(w => w.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId)?.EmployeeId ?? 0;
                Employee employee = GetEmployeeWithContactPersonFromCache(employeeId, getHidden: false);
                TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = timeWorkAccountYearEmployees.FirstOrDefault(w => w.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId);

                TermGroup_TimeWorkAccountYearSendMailCode code;
                if (employee == null || timeWorkAccountYearEmployee == null)
                    code = TermGroup_TimeWorkAccountYearSendMailCode.EmployeeNotFound;
                else if (timeWorkAccountYearEmployee.Status < (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                    code = TermGroup_TimeWorkAccountYearSendMailCode.EmployeeNotCalculated;
                else if (timeWorkAccountYearEmployee.Status > (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                    code = TermGroup_TimeWorkAccountYearSendMailCode.EmployeeHasChoosen;
                else if (!timeWorkAccountYearEmployee.SpecifiedWorkingTimePromoted.HasValue && timeWorkAccountYearEmployee.CalculatedWorkingTimePromoted == 0 || (timeWorkAccountYearEmployee.SpecifiedWorkingTimePromoted.HasValue && timeWorkAccountYearEmployee.SpecifiedWorkingTimePromoted.Value == 0))
                    code = TermGroup_TimeWorkAccountYearSendMailCode.EmployeeNoAmount;
                else
                {
                    if (SendXEMailTimeWorkAccountChoice(employee, lastDate, timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId, sender))
                        code = TermGroup_TimeWorkAccountYearSendMailCode.Succeeded;
                    else
                        code = TermGroup_TimeWorkAccountYearSendMailCode.SendFailed;
                }

                results.Add(TimeWorkAccountChoiceResultRowDTO.Create(timeWorkAccountId, timeWorkAccountYearId, timeWorkAccountYearEmployeeId, employeeId, employee?.EmployeeNrAndName, code));
            }

            return results;
        }

        private List<TimeWorkAccountTransactionResultRowDTO> TimeWorkAccountGenerateOutcome(List<Employee> employees, TimeWorkAccount timeWorkAccount, TimeWorkAccountYear timeWorkAccountYear, List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees, AttestStateDTO attestStateInitial, DateTime paymentDate, bool overrideChoosen, TimePeriod overrideTimePeriod = null, VacationYearEndRow vacationYearEndRow = null)
        {
            if (employees.IsNullOrEmpty() || timeWorkAccount == null || timeWorkAccountYear == null || timeWorkAccountYearEmployees.IsNullOrEmpty() || attestStateInitial == null)
                return Empty();

            List<TimeWorkAccountTransactionResultRowDTO> results = new List<TimeWorkAccountTransactionResultRowDTO>();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

            #region Lazy loaders

            PayrollProduct LoadProduct(int? productId)
            {
                return productId.HasValue ? GetPayrollProductFromCache(productId.Value) : null;
            }
            (bool IsLoaded, PayrollProduct Product) pensionDeposit = (false, null);
            PayrollProduct GetPayrollProductForPensionDeposit()
            {
                if (!pensionDeposit.IsLoaded)
                    pensionDeposit = (true, LoadProduct(timeWorkAccountYear.PensionDepositPayrollProductId));
                return pensionDeposit.Product;
            }
            (bool IsLoaded, PayrollProduct Product) directPayment = (false, null);
            PayrollProduct GetPayrollProductForDirectPayment()
            {
                if (!directPayment.IsLoaded)
                    directPayment = (true, LoadProduct(timeWorkAccountYear.DirectPaymentPayrollProductId));
                return directPayment.Product;
            }
            Dictionary<int, bool> employeeGroupHasTimeAccumulator = new Dictionary<int, bool>();
            (bool IsLoaded, TimeCode TimeCode, List<PayrollProduct> PayrollProducts) paidLeave = (false, null, null);
            (TimeCode, List<PayrollProduct>) GetTimeCodeAndPayrollProductsForPaidLeave()
            {
                if (!paidLeave.IsLoaded)
                {
                    TimeAccumulator acc = GetTimeAccumulator();
                    TimeCode timeCode = acc?.TimeCodeId != null ? GetTimeCodeWithProductsFromCache(acc.TimeCodeId.Value) : null;
                    List<PayrollProduct> payrollProducts = timeCode?.TimeCodePayrollProduct.Select(m => GetPayrollProductFromCache(m.ProductId)).ToList();
                    paidLeave = (true, timeCode, payrollProducts);
                }
                return (paidLeave.TimeCode, paidLeave.PayrollProducts);
            }
            (bool IsLoaded, TimeAccumulator Acc) timeAccumulator = (false, null);
            TimeAccumulator GetTimeAccumulator()
            {
                if (!timeAccumulator.IsLoaded)
                    timeAccumulator = (true, timeWorkAccountYear.TimeAccumulatorId.HasValue ? GetTimeAccumulatorWithTimeCode(timeWorkAccountYear.TimeAccumulatorId.Value) : null);
                return timeAccumulator.Acc;
            }
            bool IsEmployeeGroupConnectedToTimeAccumulator(EmployeeGroup employeeGroup)
            {
                if (employeeGroup == null)
                    return false;

                if (!employeeGroupHasTimeAccumulator.ContainsKey(employeeGroup.EmployeeGroupId))
                {
                    TimeAccumulator acc = GetTimeAccumulator();
                    List<TimeAccumulatorEmployeeGroupRule> rules = GetTimeAccumulatorEmployeeGroupRule(acc.TimeAccumulatorId);
                    employeeGroupHasTimeAccumulator.Add(employeeGroup.EmployeeGroupId, rules.Any(r => r.EmployeeGroupId == employeeGroup.EmployeeGroupId));
                }
                return employeeGroupHasTimeAccumulator[employeeGroup.EmployeeGroupId];
            }

            #endregion

            List<TimePeriodHead> timePeriodHeadCache = new List<TimePeriodHead>();
            foreach (var timeWorkAccountYearEmployeesForEmployee in timeWorkAccountYearEmployees.GroupBy(r => r.EmployeeId))
            {
                Employee employee = employees.FirstOrDefault(f => f.EmployeeId == timeWorkAccountYearEmployeesForEmployee.Key);
                if (employee == null)
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotFound, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }

                TimePeriod timePeriod = overrideTimePeriod ?? GetTimePeriod(employee, paymentDate, timePeriodHeadCache);
                if (timePeriod == null)
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodNotFound, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }
                if (IsEmployeeTimePeriodLockedForChanges(employee.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodLocked, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }

                foreach (TimeWorkAccountYearEmployee timeWorkAccountYearEmployee in timeWorkAccountYearEmployeesForEmployee)
                {
                    TimeWorkAccountTransactionResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkAccountWithdrawalMethod)timeWorkAccountYearEmployee.SelectedWithdrawalMethod);

                    if (timeWorkAccountYearEmployee.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome)
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.EmployeeAlreadyGenerated);
                    else if (timeWorkAccountYearEmployee.Status < (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotCalculated);
                    else if (timeWorkAccountYearEmployee.Status < (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed && !overrideChoosen)
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.EmployeeHasntChoosen);
                    else
                    {
                        if (timeWorkAccountYearEmployee.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed && overrideChoosen)
                            timeWorkAccountYearEmployee.SelectedWithdrawalMethod = timeWorkAccount.DefaultWithdrawalMethod;

                        if (timeWorkAccountYearEmployee.IsWithdrawalMethodDirectPayment() || (overrideChoosen && timeWorkAccountYearEmployee.IsWithdrawalMethodNotChoosed() && timeWorkAccount.DefaulWithdrawalMethodIsDirectPayment()))
                        {
                            result.UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment);
                            var payrollProduct = GetPayrollProductForDirectPayment();
                            if (payrollProduct == null)
                            {
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollPeriodDirectPaymentNotFound);
                                continue;
                            }
                            CreatePayrollTransaction(payrollProduct, GetLatestEmploymentDateInPeriod(), 1, timeWorkAccountYearEmployee.CalculatedDirectPaymentAmount, timeWorkAccountYearEmployee.CalculatedDirectPaymentAmount);
                        }
                        else if (timeWorkAccountYearEmployee.IsWithdrawalMethodPensionDeposit() || (overrideChoosen && timeWorkAccountYearEmployee.IsWithdrawalMethodNotChoosed() && timeWorkAccount.DefaultWithdrawalMethodIsPensionDeposit()))
                        {
                            result.UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit);
                            var payrollProduct = GetPayrollProductForPensionDeposit();
                            if (payrollProduct == null)
                            {
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollPeriodPensionDepositNotFound);
                                continue;
                            }
                            CreatePayrollTransaction(payrollProduct, GetLatestEmploymentDateInPeriod(), 1, timeWorkAccountYearEmployee.CalculatedPensionDepositAmount, timeWorkAccountYearEmployee.CalculatedPensionDepositAmount);
                        }
                        else if (timeWorkAccountYearEmployee.IsWithdrawalMethodPaidLeave() || (overrideChoosen && timeWorkAccountYearEmployee.IsWithdrawalMethodNotChoosed() && timeWorkAccount.DefaultWithdrawalMethodIsPaidLeave()))
                        {
                            result.UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave);
                            var (timeCode, payrollProducts) = GetTimeCodeAndPayrollProductsForPaidLeave();
                            if (timeCode == null)
                            {
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.TimeCodePaidLeaveNotFound);
                                continue;
                            }
                            if (payrollProducts.IsNullOrEmpty())
                            {
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollProductPaidLeaveNotFound);
                                continue;
                            }

                            DateTime date = timeWorkAccountYear.WithdrawalStart;
                            if (IsEmployeeGroupConnectedToTimeAccumulator(employee.GetEmployeeGroup(date, employeeGroups)))
                            {
                                decimal quantity = timeWorkAccountYearEmployee.CalculatedPaidLeaveMinutes;
                                decimal unitPrice = timeWorkAccountYearEmployee.GetUnitPrice();
                                CreateFactor(date, unitPrice);
                                TimeCodeTransaction timeCodeTransaction = CreateCodeTransaction(timeCode, date, quantity);
                                CreatePayrollTransactions(payrollProducts, date, quantity, timeCodeTransaction: timeCodeTransaction);
                            }
                            else
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotConnectedToTimeAccumulator);
                        }

                        DateTime GetLatestEmploymentDateInPeriod()
                        {
                            DateTime? latestEmploymentDateInPeriod = employee.GetLatestEmploymentDate(timePeriod.StartDate, timePeriod.StopDate);
                            if (!latestEmploymentDateInPeriod.HasValue)
                                latestEmploymentDateInPeriod = timePeriod.StopDate;
                            return latestEmploymentDateInPeriod.Value;
                        }
                        void CreateFactor(DateTime date, decimal unitPrice)
                        {
                            EmployeeFactor.Create(timeWorkAccountYearEmployee, TermGroup_EmployeeFactorType.TimeWorkAccountPaidLeave, date, unitPrice, GetUserDetails());
                        }
                        TimeCodeTransaction CreateCodeTransaction(TimeCode timeCode, DateTime date, decimal quantity)
                        {
                            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date, true);
                            TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransaction(timeCode.TimeCodeId, TimeCodeTransactionType.Time, quantity, timeBlockDate.Date, timeBlockDate.Date, timeBlockDate: timeBlockDate);
                            if (timeCodeTransaction != null)
                            {
                                timeWorkAccountYearEmployee.UpdateStatus(TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome, GetUserDetails());
                                result.CreateSucess();
                            }
                            else
                            {
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed);
                            }
                            return timeCodeTransaction;
                        }
                        void CreatePayrollTransactions(List<PayrollProduct> payrollProducts, DateTime date, decimal quantity, decimal unitPrice = 0, decimal amount = 0, TimeCodeTransaction timeCodeTransaction = null)
                        {
                            payrollProducts.ForEach(payrollProduct => CreatePayrollTransaction(payrollProduct, date, quantity, unitPrice, amount, timeCodeTransaction));
                        }
                        void CreatePayrollTransaction(PayrollProduct payrollProduct, DateTime date, decimal quantity, decimal unitPrice = 0, decimal amount = 0, TimeCodeTransaction timeCodeTransaction = null)
                        {
                            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date, true);
                            TimePayrollTransaction timePayrollTransaction = CreateOrUpdateTimePayrollTransaction(payrollProduct, timeBlockDate, employee, timePeriod.TimePeriodId, attestStateInitial.AttestStateId, quantity, unitPrice, amount);
                            if (timePayrollTransaction != null)
                            {
                                TimeWorkAccountYearOutcome timeWorkAccountYearOutcome = CreateTimeWorkAccountYearOutcome(timeWorkAccountYearEmployee, SoeTimeWorkAccountYearOutcomeType.Selection);
                                if (timeWorkAccountYearOutcome == null)
                                    result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed);

                                string userDetails = GetUserDetails();
                                timePayrollTransaction.SetTimeCodeTransaction(timeCodeTransaction, userDetails);
                                timePayrollTransaction.SetTimeWorkAccountYearOutcome(timeWorkAccountYearOutcome, userDetails);
                                if (vacationYearEndRow != null)
                                    timePayrollTransaction.SetVacationYearEndRow(vacationYearEndRow, userDetails);
                                timeWorkAccountYearEmployee.UpdateStatus(TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome, userDetails);
                                result.CreateSucess();
                            }
                            else
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed);
                        }
                    }
                }
            }

            return Save().Success ? results : ResultsAsSaveFailed();

            TimeWorkAccountTransactionResultRowDTO CreateResult(Employee employee, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                TimeWorkAccountTransactionResultRowDTO result = TimeWorkAccountTransactionResultRowDTO.Create(
                    timeWorkAccount.TimeWorkAccountId,
                    timeWorkAccountYear.TimeWorkAccountYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    method);
                results.Add(result);
                return result;
            }
            void Failed(Employee employee, TermGroup_TimeWorkAccountTransactionActionCode code, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                CreateResult(employee, method).Failed(code);
            }
            List<TimeWorkAccountTransactionResultRowDTO> Empty()
            {
                return new List<TimeWorkAccountTransactionResultRowDTO>();
            }
            List<TimeWorkAccountTransactionResultRowDTO> ResultsAsSaveFailed()
            {
                results.ForEach(r => r.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed));
                return results;
            }
        }

        private List<TimeWorkAccountTransactionResultRowDTO> SaveTimeWorkAccountReverseTransaction(List<Employee> employees, TimeWorkAccount timeWorkAccount, TimeWorkAccountYear timeWorkAccountYear, List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees, AttestStateDTO attestStateInitial, TermGroup_TimeWorkAccountYearEmployeeStatus status)
        {
            if (employees.IsNullOrEmpty() || timeWorkAccount == null || timeWorkAccountYear == null || timeWorkAccountYearEmployees.IsNullOrEmpty() || attestStateInitial == null)
                return Empty();
            if (status != TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome && status != TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance)
                return Empty();

            SoeTimeWorkAccountYearOutcomeType timeWorkSelectionType = status == TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome ? SoeTimeWorkAccountYearOutcomeType.Selection : SoeTimeWorkAccountYearOutcomeType.AccAdjustment;

            List<TimeWorkAccountTransactionResultRowDTO> results = new List<TimeWorkAccountTransactionResultRowDTO>();
            List<TimePayrollTransaction> transactions = GetTransactionsForTimeWorkAccountWithTimeCodeTransactionAndOutcome(timeWorkAccountYear.TimeWorkAccountYearId, timeWorkAccountYearEmployees.Select(s => s.TimeWorkAccountYearEmployeeId).ToList(), timeWorkSelectionType);

            foreach (Employee employee in employees)
            {
                List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployeesForEmployee = timeWorkAccountYearEmployees.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                foreach (var timeWorkAccountYearEmployee in timeWorkAccountYearEmployeesForEmployee)
                {
                    if (timeWorkAccountYearEmployee == null)
                    {
                        Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotFound, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                        continue;
                    }

                    if (timeWorkAccountYearEmployee.Status != (int)status)
                    {
                        TimeWorkAccountTransactionResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkAccountWithdrawalMethod)timeWorkAccountYearEmployee.SelectedWithdrawalMethod);
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotGenerated);
                    }
                    else
                    {
                        TimeWorkAccountTransactionResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkAccountWithdrawalMethod)timeWorkAccountYearEmployee.SelectedWithdrawalMethod);
                        List<TimePayrollTransaction> employeeTransactions = transactions.Where(w => w.TimeWorkAccountYearOutcome != null && w.TimeWorkAccountYearOutcome.Type == (int)timeWorkSelectionType && w.TimeWorkAccountYearOutcome.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId).ToList();

                        if (employeeTransactions.Any(t => t.AttestStateId != attestStateInitial.AttestStateId))
                        {
                            result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.PayrollTransactionWrongState);
                        }
                        else
                        {
                            foreach (var timePayrollTransaction in employeeTransactions)
                            {
                                ChangeEntityState(timePayrollTransaction, SoeEntityState.Deleted);
                                if (timePayrollTransaction.TimeWorkAccountYearOutcome != null)
                                    ChangeEntityState(timePayrollTransaction.TimeWorkAccountYearOutcome, SoeEntityState.Deleted);
                                if (timePayrollTransaction.TimeCodeTransaction != null)
                                    ChangeEntityState(timePayrollTransaction.TimeCodeTransaction, SoeEntityState.Deleted);
                            }
                            timeWorkAccountYearEmployee.Status = status == TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome
                                ? (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed
                                : (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome;

                            SetModifiedProperties(timeWorkAccountYearEmployee);

                            if (status == TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome && timeWorkAccountYearEmployee.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave)
                            {
                                List<EmployeeFactor> factors = GetEmployeeFactorsForTimeWorkAccount(timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId);

                                if (factors.Any())
                                {
                                    foreach (EmployeeFactor factor in factors)
                                    {
                                        ChangeEntityState(factor, SoeEntityState.Deleted);
                                        SetModifiedProperties(factor);
                                    }
                                }
                            }

                            result.DeleteSuccess();
                        }
                    }
                }
            }

            return Save().Success ? results : ResultsAsSaveFailed();

            TimeWorkAccountTransactionResultRowDTO CreateResult(Employee employee, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                TimeWorkAccountTransactionResultRowDTO result = TimeWorkAccountTransactionResultRowDTO.Create(
                    timeWorkAccount.TimeWorkAccountId,
                    timeWorkAccountYear.TimeWorkAccountYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    method);
                results.Add(result);
                return result;
            }
            void Failed(Employee employee, TermGroup_TimeWorkAccountTransactionActionCode code, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                CreateResult(employee, method).Failed(code);
            }
            List<TimeWorkAccountTransactionResultRowDTO> Empty()
            {
                return new List<TimeWorkAccountTransactionResultRowDTO>();
            }
            List<TimeWorkAccountTransactionResultRowDTO> ResultsAsSaveFailed()
            {
                results.ForEach(r => r.Failed(TermGroup_TimeWorkAccountTransactionActionCode.SaveFailed));
                return results;
            }
        }

        private List<TimeWorkAccountTransactionResultRowDTO> SaveTimeWorkAccountGenerateUnusedPaidBalance(List<Employee> employees, TimeWorkAccount timeWorkAccount, TimeWorkAccountYear timeWorkAccountYear, List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees, AttestStateDTO attestStateInitial, DateTime paymentDate, List<TimeAccumulator> timeAccumulators, TimePeriod overrideTimePeriod = null, VacationYearEndRow vacationYearEndRow = null, bool isFinalSalary = false)
        {
            if (employees.IsNullOrEmpty() || timeWorkAccount == null || timeWorkAccountYear == null || timeWorkAccountYearEmployees.IsNullOrEmpty() || attestStateInitial == null)
                return Empty();

            timeWorkAccountYearEmployees = timeWorkAccountYearEmployees.Where(y => y.TimeWorkAccountYearId == timeWorkAccountYear.TimeWorkAccountYearId).ToList();
            if (timeWorkAccountYearEmployees.IsNullOrEmpty())
                return Empty();

            List<TimeWorkAccountTransactionResultRowDTO> externalResults = new List<TimeWorkAccountTransactionResultRowDTO>();

            #region Lazy loaders
            PayrollProduct LoadProduct(int? productId)
            {
                return productId.HasValue ? GetPayrollProductFromCache(productId.Value) : null;
            }
            (bool IsLoaded, PayrollProduct Product) directPayment = (false, null);
            PayrollProduct GetPayrollProductForDirectPayment()
            {
                if (!directPayment.IsLoaded)
                    directPayment = (true, LoadProduct(timeWorkAccountYear.DirectPaymentPayrollProductId));
                return directPayment.Product;
            }
            (bool IsLoaded, PayrollProduct Product) pensionDeposit = (false, null);
            PayrollProduct GetPayrollProductForPensionDeposit()
            {
                if (!pensionDeposit.IsLoaded)
                    pensionDeposit = (true, LoadProduct(timeWorkAccountYear.PensionDepositPayrollProductId));
                return pensionDeposit.Product;
            }
            #endregion

            List<TimePeriodHead> timePeriodHeadCache = new List<TimePeriodHead>();
            List<TimeWorkAccountTransactionResultRowDTO> internalResults = new List<TimeWorkAccountTransactionResultRowDTO>();

            foreach (var timeWorkAccountYearEmployeesForEmployee in timeWorkAccountYearEmployees.GroupBy(r => r.EmployeeId))
            {
                internalResults = new List<TimeWorkAccountTransactionResultRowDTO>();
                Employee employee = employees.FirstOrDefault(f => f.EmployeeId == timeWorkAccountYearEmployeesForEmployee.Key);
                if (employee == null)
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotFound, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }

                TimePeriod timePeriod = overrideTimePeriod ?? GetTimePeriod(employee, paymentDate, timePeriodHeadCache);
                if (timePeriod == null)
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodNotFound, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }
                if (IsEmployeeTimePeriodLockedForChanges(employee.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                {
                    Failed(employee, TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodLocked, TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed);
                    continue;
                }

                foreach (TimeWorkAccountYearEmployee timeWorkAccountYearEmployee in timeWorkAccountYearEmployeesForEmployee.OrderByDescending(x => x.EarningStop))
                {
                    TimeWorkAccountTransactionResultRowDTO result = CreateResult(employee, (TermGroup_TimeWorkAccountWithdrawalMethod)timeWorkAccountYearEmployee.SelectedWithdrawalMethod);

                    if (timeWorkAccountYearEmployee.Status == (int)TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance)
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.BalanceAlreadyPaid);
                    else if (timeWorkAccountYearEmployee.Status < (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome)
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.BalanceNotGenerated);
                    else if (timeWorkAccountYearEmployee.SelectedWithdrawalMethod != (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave &&
                        (timeWorkAccountYearEmployee.SelectedWithdrawalMethod != (int)TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed && timeWorkAccount.DefaultWithdrawalMethod != (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave)
                        )
                        result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.BalanceNotChoosen);
                    else
                    {
                        if (internalResults.Any(x => x.EmployeeId == timeWorkAccountYearEmployee.EmployeeId && x.Code == TermGroup_TimeWorkAccountTransactionActionCode.BalanceSuccess))
                            continue;

                        TimeBlockDate timeBlockDate = isFinalSalary && employee.GetLastEmployment().DateTo.HasValue ? GetTimeBlockDateFromCache(employee.EmployeeId, employee.GetLastEmployment().DateTo.Value, true) : GetTimeBlockDateFromCache(employee.EmployeeId, timeWorkAccountYear.PaidAbsenceStopDate, true);
                        TimeAccumulatorItem item = GetTimeAccumulatorItemForTimeWorkAccountUnusedPaidBalance(timeAccumulators, employee, timePeriod, timeWorkAccountYear);
                        if (item == null || item.SumAccToday <= 0)
                        {
                            result.UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode.BalanceNotFound);
                            continue;
                        }

                        List<EmployeeFactor> factors = GetEmployeeFactorsForTimeWorkAccount(timeWorkAccountYearEmployee.TimeWorkAccountYearEmployeeId);
                        if (!factors.Any())
                            continue;

                        TimeCode timeCode = item.TimeCodeId.HasValue ? GetTimeCodeWithProductsFromCache(item.TimeCodeId.Value) : null;
                        if (timeCode == null)
                            continue;

                        TimeWorkAccountYearOutcome timeWorkAccountYearOutcome = CreateTimeWorkAccountYearOutcome(timeWorkAccountYearEmployee, SoeTimeWorkAccountYearOutcomeType.AccAdjustment);
                        if (timeWorkAccountYearOutcome == null)
                        {
                            result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed);
                            continue;
                        }

                        List<PayrollProduct> payrollProducts = timeCode.TimeCodePayrollProduct.Select(m => GetPayrollProductFromCache(m.ProductId)).ToList();
                        if (payrollProducts == null)
                        {
                            result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollPeriodDirectPaymentNotFound);
                            continue;
                        }

                        TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransaction(item.TimeCodeId.Value, TimeCodeTransactionType.Time, -item.SumAccToday, timeBlockDate.Date, timeBlockDate.Date, 0, timeBlockDate: timeBlockDate);
                        if (timeCodeTransaction == null)
                            continue;

                        CreatePayrollTransactions(payrollProducts, timeBlockDate, timeWorkAccountYearOutcome, 1, timeCodeTransaction.UnitPrice ?? 0, timeCodeTransaction.Amount ?? 0, timeCodeTransaction: timeCodeTransaction);
                        if (!result.IsBalanceSuccess())
                            continue;

                        EmployeeFactor factor = factors.FirstOrDefault();
                        decimal amount = (item.SumAccToday / 60) * factor.Factor;

                        if (isFinalSalary || timeWorkAccount.DefaultPaidLeaveNotUsed == (int)TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment)
                        {
                            result.UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment);
                            PayrollProduct payrollProduct = GetPayrollProductForDirectPayment();
                            if (payrollProduct == null)
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollPeriodDirectPaymentNotFound);
                            else
                                CreatePayrollTransaction(payrollProduct, timeBlockDate, timeWorkAccountYearOutcome, 1, amount, amount);
                        }
                        else if (timeWorkAccount.DefaultPaidLeaveNotUsed == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit)
                        {
                            result.UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit);
                            PayrollProduct payrollProduct = GetPayrollProductForPensionDeposit();
                            if (payrollProduct == null)
                                result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.PayrollPeriodPensionDepositNotFound);
                            else
                                CreatePayrollTransaction(payrollProduct, timeBlockDate, timeWorkAccountYearOutcome, 1, amount, amount);
                        }
                    }

                    void CreatePayrollTransactions(List<PayrollProduct> payrollProducts, TimeBlockDate timeBlockDate, TimeWorkAccountYearOutcome timeWorkAccountYearOutcome, decimal quantity, decimal unitPrice = 0, decimal amount = 0, TimeCodeTransaction timeCodeTransaction = null)
                    {
                        payrollProducts.ForEach(payrollProduct => CreatePayrollTransaction(payrollProduct, timeBlockDate, timeWorkAccountYearOutcome, quantity, unitPrice, amount, timeCodeTransaction));
                    }
                    void CreatePayrollTransaction(PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, TimeWorkAccountYearOutcome timeWorkAccountYearOutcome, decimal quantity, decimal unitPrice = 0, decimal amount = 0, TimeCodeTransaction timeCodeTransaction = null)
                    {
                        TimePayrollTransaction timePayrollTransaction = CreateOrUpdateTimePayrollTransaction(payrollProduct, timeBlockDate, employee, timePeriod.TimePeriodId, attestStateInitial.AttestStateId, quantity, unitPrice, amount);
                        if (timePayrollTransaction != null)
                        {
                            string userDetails = GetUserDetails();
                            timePayrollTransaction.SetTimeCodeTransaction(timeCodeTransaction, userDetails);
                            timePayrollTransaction.SetTimeWorkAccountYearOutcome(timeWorkAccountYearOutcome, userDetails);
                            if (vacationYearEndRow != null)
                                timePayrollTransaction.SetVacationYearEndRow(vacationYearEndRow, userDetails);
                            timeWorkAccountYearEmployee.UpdateStatus(TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance, userDetails);
                            result.BalanceSuccess();
                        }
                        else
                            result.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed);
                    }
                }

                if (!Save().Success)
                    ResultsAsSaveFailed();

                externalResults.AddRange(internalResults);
            }

            return externalResults;

            TimeWorkAccountTransactionResultRowDTO CreateResult(Employee employee, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                TimeWorkAccountTransactionResultRowDTO result = TimeWorkAccountTransactionResultRowDTO.Create(
                    timeWorkAccount.TimeWorkAccountId,
                    timeWorkAccountYear.TimeWorkAccountYearId,
                    employee.EmployeeId,
                    employee.EmployeeNrAndName,
                    method);
                internalResults.Add(result);
                return result;
            }
            void Failed(Employee employee, TermGroup_TimeWorkAccountTransactionActionCode code, TermGroup_TimeWorkAccountWithdrawalMethod method)
            {
                CreateResult(employee, method).Failed(code);
            }
            List<TimeWorkAccountTransactionResultRowDTO> Empty()
            {
                return new List<TimeWorkAccountTransactionResultRowDTO>();
            }
            void ResultsAsSaveFailed()
            {
                internalResults.ForEach(r => r.Failed(TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed));
            }

        }

        private ActionResult ApplyTimeWorkAccountFinalSalary(TimeEngineVacationYearEndEmployee finalSalaryEmployee)
        {
            if (finalSalaryEmployee?.Employee == null)
                return new ActionResult(true);

            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10085, "Attestnivå hittades inte"));

            if (GetTimeWorkAccountsFromCache(entities, CacheConfig.Company(actorCompanyId)).IsNullOrEmpty())
                return new ActionResult(true);

            List<EmployeeTimeWorkAccount> employeeTimeWorkAccounts = GetTimeWorkAccountsForEmployee(finalSalaryEmployee.EmployeeId);
            if (employeeTimeWorkAccounts.IsNullOrEmpty())
                return new ActionResult(true);

            #region Lazy loaders

            List<TimeAccumulator> timeAccumulators = null;
            List<TimeAccumulator> GetTimeAccumulators()
            {
                if (timeAccumulators == null)
                    timeAccumulators = GetTimeAccumulatorsForTimeWorkAccountFromCache();
                return timeAccumulators;
            }

            #endregion

            ActionResult result = TryPayCurrentYear();
            if (!result.Success)
                return result;
            result = TryPayPreviousYear();
            if (!result.Success)
                return result;

            //Assumes that TimeWorkAccountYear is calculated before next TimeWorkAccountYear is created
            //Maybe need a warning that calculation must be handled manually if status > Calculated
            ActionResult TryPayCurrentYear()
            {
                EmployeeTimeWorkAccount employeeTimeWorkAccount = employeeTimeWorkAccounts.GetLatest(finalSalaryEmployee.Date);
                TimeWorkAccountYear timeWorkAccountYear = employeeTimeWorkAccount != null ? GetLatestTimeWorkAccountYearWithAccount(employeeTimeWorkAccount.TimeWorkAccountId, finalSalaryEmployee.Date) : null;
                if (timeWorkAccountYear != null)
                {
                    List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(finalSalaryEmployee.EmployeeId);
                    TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = timeWorkAccountYearEmployees?.OrderByDescending(e => e.EarningStop).FirstOrDefault(e => e.TimeWorkAccountYearId == timeWorkAccountYear.TimeWorkAccountYearId);
                    if (timeWorkAccountYearEmployee == null || timeWorkAccountYearEmployee.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated)
                    {
                        List<TimeWorkAccountYearEmployeeResultRowDTO> calculateResultRows = CalculateTimeWorkAccountYearEmployee(finalSalaryEmployee.EmployeeId, timeWorkAccountYear.TimeWorkAccount, timeWorkAccountYear, employeeTimeWorkAccounts, timeWorkAccountYearEmployees, finalSalaryTimePeriod: finalSalaryEmployee.TimePeriod);
                        if (!calculateResultRows.IsNullOrEmpty())
                        {
                            if (calculateResultRows.Any(r => r.HasFailed))
                                return new ActionResult((int)ActionResultSave.EmploymentFinalSalaryATKFailed, GetText((int)calculateResultRows.First(r => r.HasFailed).Code, (int)TermGroup.TimeWorkAccountYearResultCode));

                            foreach (TimeWorkAccountYearEmployeeResultRowDTO resultRow in calculateResultRows)
                            {
                                List<TimeWorkAccountYearEmployee> selectedTimeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployees(resultRow.EmployeeId, resultRow.TimeWorkAccountYearId);
                                selectedTimeWorkAccountYearEmployees.SetSelectedWithdrawalMethod(TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment);
                                List<TimeWorkAccountTransactionResultRowDTO> outcomeResultRows = TimeWorkAccountGenerateOutcome(finalSalaryEmployee.Employee.ObjToList(), timeWorkAccountYear.TimeWorkAccount, timeWorkAccountYear, selectedTimeWorkAccountYearEmployees, attestStateInitial, finalSalaryEmployee.Date, false, finalSalaryEmployee.TimePeriod, finalSalaryEmployee.VacationYearEndRow);
                                if (outcomeResultRows.Any(r => r.HasFailed))
                                    return new ActionResult((int)ActionResultSave.EmploymentFinalSalaryATKFailed, GetText((int)outcomeResultRows.First(r => r.HasFailed).Code, (int)TermGroup.TimeWorkAccountGenerateOutcomeCode));
                            }
                        }
                    }
                }

                return new ActionResult(true);
            }
            ActionResult TryPayPreviousYear()
            {
                List<TimeWorkAccountYearEmployee> timeWorkAccountYearEmployees = GetTimeWorkAccountYearEmployeesWithYear(finalSalaryEmployee.EmployeeId);
                TimeWorkAccountYearEmployee timeWorkAccountYearEmployeePaidLeave = timeWorkAccountYearEmployees?.OrderByDescending(e => e.EarningStop).FirstOrDefault(e => e.Status > (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated && e.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave);
                TimeWorkAccount timeWorkAccount = timeWorkAccountYearEmployeePaidLeave?.TimeWorkAccountYear != null ? GetTimeWorkAccount(timeWorkAccountYearEmployeePaidLeave.TimeWorkAccountYear.TimeWorkAccountId) : null;
                if (timeWorkAccount != null)
                {
                    List<TimeWorkAccountTransactionResultRowDTO> balanceResultRows = SaveTimeWorkAccountGenerateUnusedPaidBalance(finalSalaryEmployee.Employee.ObjToList(), timeWorkAccount, timeWorkAccountYearEmployeePaidLeave.TimeWorkAccountYear, timeWorkAccountYearEmployees, attestStateInitial, finalSalaryEmployee.Date, GetTimeAccumulators(), finalSalaryEmployee.TimePeriod, finalSalaryEmployee.VacationYearEndRow, true);
                    if (balanceResultRows.Any(r => r.HasFailed))
                        return new ActionResult((int)ActionResultSave.EmploymentFinalSalaryATKFailed, GetText((int)balanceResultRows.First(r => r.HasFailed).Code, (int)TermGroup.TimeWorkAccountYearResultCode));
                }
                return new ActionResult(true);
            }

            return result;
        }

        private ActionResult ReverseTimeWorkAccountTransactionFinalSalary(List<TimePayrollTransaction> timePayrollTransactions)
        {
            ActionResult result = new ActionResult();
            List<Employee> employees = new List<Employee>();
            List<TimeWorkAccountTransactionResultRowDTO> transactionResult = new List<TimeWorkAccountTransactionResultRowDTO>();
            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {

                TimeWorkAccountYearOutcome timeWorkAccountYearOutcome = GetTimeWorkAccountYearOutcome(timePayrollTransaction.TimeWorkAccountYearOutcomeId.Value);
                if (timeWorkAccountYearOutcome == null)
                    continue;

                TimeWorkAccountYear timeWorkAccountYear = GetTimeWorkAccountYearWithEmployees(timeWorkAccountYearOutcome.TimeWorkAccountYearId);
                if (timeWorkAccountYear == null)
                    continue;

                TimeWorkAccount timeWorkAccount = GetTimeWorkAccount(timeWorkAccountYear.TimeWorkAccountId);
                TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = timeWorkAccountYear.TimeWorkAccountYearEmployee.FirstOrDefault(w => w.TimeWorkAccountYearEmployeeId == timeWorkAccountYearOutcome.TimeWorkAccountYearEmployeeId);

                if (timeWorkAccountYearEmployee == null || timeWorkAccount == null)
                    continue;

                var employee = GetEmployee(timeWorkAccountYearEmployee.EmployeeId);
                if (employee != null)
                    employees.Add(employee);

                if (!employees.Any())
                    continue;

                transactionResult.AddRange(SaveTimeWorkAccountReverseTransaction(employees, timeWorkAccount, timeWorkAccountYear, timeWorkAccountYearEmployee.ObjToList(), attestStateInitial, (TermGroup_TimeWorkAccountYearEmployeeStatus)timeWorkAccountYearEmployee.Status));
            }

            if (transactionResult.FirstOrDefault()?.Code == TermGroup_TimeWorkAccountTransactionActionCode.DeleteSuccess)
                result.Success = true;
            else
                result.Success = false;

            return result;

        }

        #endregion

        #region Helpers

        private (DateTime Start, DateTime Stop) GetTimeWorkAccountTransactionDates((List<GetTimePayrollTransactionsForEmployee_Result> Payroll, List<GetTimePayrollScheduleTransactionsForEmployee_Result> Schedule) transactions, DateTime earningStart, DateTime earningStop)
        {
            if (transactions.Payroll.IsNullOrEmpty() && transactions.Schedule.IsNullOrEmpty())
                return (earningStart, earningStop);

            DateTime? transactionStartDate = CalendarUtility.GetEarliestDate(transactions.Payroll.GetStartDate(), transactions.Schedule.GetStartDate());
            DateTime? transactionStopDate = CalendarUtility.GetLatestDate(transactions.Payroll.GetStopDate(), transactions.Schedule.GetStopDate());
            return (
                CalendarUtility.GetBeginningOfDay(CalendarUtility.GetEarliestDate(earningStart, transactionStartDate)),
                CalendarUtility.GetEndOfDay(CalendarUtility.GetLatestDate(earningStop, transactionStopDate))
                );
        }

        private void Beautify(TimeWorkAccountYearEmployeeResultDTO result)
        {
            if (result == null || result.Rows.IsNullOrEmpty())
                return;

            foreach (var resultsByCode in result.Rows.GroupBy(r => r.Code))
            {
                string name = GetText((int)resultsByCode.Key, TermGroup.TimeWorkAccountYearResultCode);
                resultsByCode.ToList().ForEach(r => r.UpdateCode(r.Code, name));
            }
            foreach (var resultsByEmployeeStatus in result.Rows.GroupBy(r => r.EmployeeStatus))
            {
                string name = GetText((int)resultsByEmployeeStatus.Key, TermGroup.TimeWorkAccountYearEmployeeStatus);
                resultsByEmployeeStatus.ToList().ForEach(r => r.UpdateEmployeeStatusName(r.EmployeeStatus, name));
            }
        }

        private void Beautify(TimeWorkAccountGenerateOutcomeResultDTO result)
        {
            if (result == null || result.Rows.IsNullOrEmpty())
                return;

            foreach (var resultsByCode in result.Rows.GroupBy(r => r.Code))
            {
                string name = GetText((int)resultsByCode.Key, TermGroup.TimeWorkAccountGenerateOutcomeCode);
                resultsByCode.ToList().ForEach(r => r.UpdateCode(r.Code, name));
            }
            foreach (var resultsByMethod in result.Rows.GroupBy(r => r.Method))
            {
                string name = GetText((int)resultsByMethod.Key, TermGroup.TimeWorkAccountWithdrawalMethod);
                resultsByMethod.ToList().ForEach(r => r.UpdateMethod(r.Method, name));
            }
        }

        private void Beautify(TimeWorkAccountChoiceResultDTO result)
        {
            if (result == null || result.Rows.IsNullOrEmpty())
                return;

            foreach (var resultsByCode in result.Rows.GroupBy(r => r.Code))
            {
                string name = GetText((int)resultsByCode.Key, TermGroup.TimeWorkAccountYearSendMailCode);
                resultsByCode.ToList().ForEach(r => r.UpdateCode(r.Code, name));
            }
        }

        #endregion
    }
}
