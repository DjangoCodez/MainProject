using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SaveRetroactivePayrollOutputDTO TaskSaveRetroactivePayroll()
        {
            var (iDTO, oDTO) = InitTask<SaveRetroactivePayrollInputDTO, SaveRetroactivePayrollOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollInput == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            int retroactivePayrollId = 0;
            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Validation

                        if (String.IsNullOrEmpty(iDTO.RetroactivePayrollInput.Name))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8780, "Benämning har inte angetts"));
                            return oDTO;
                        }

                        if (iDTO.RetroactivePayrollInput.TimePeriodId <= 0)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8781, "Period har inte angetts"));
                            return oDTO;
                        }

                        TimePeriod timePeriod = GetTimePeriodFromCache(iDTO.RetroactivePayrollInput.TimePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8781, "Period har inte angetts"));
                            return oDTO;
                        }

                        if (timePeriod.PaymentDate < iDTO.RetroactivePayrollInput.DateTo)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8786, "Intervallets slutdatum kan inte vara senare än utbetalningsdatum"));
                            return oDTO;
                        }

                        if (iDTO.RetroactivePayrollInput.DateTo.HasValue && iDTO.RetroactivePayrollInput.DateFrom > iDTO.RetroactivePayrollInput.DateTo.Value)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8779, "Intervallets startdatum kan inte vara senare än slutdatumet"));
                            return oDTO;
                        }

                        #endregion

                        #region RetroactivePayroll

                        RetroactivePayroll retroactivePayroll = GetRetroactivePayroll(iDTO.RetroactivePayrollInput.RetroactivePayrollId, loadRetroAccounting: true, loadRetroEmployee: true);
                        if (retroactivePayroll == null)
                        {
                            retroactivePayroll = new RetroactivePayroll()
                            {
                                ActorCompanyId = this.actorCompanyId,
                                Status = (int)TermGroup_SoeRetroactivePayrollStatus.Saved
                            };

                            entities.RetroactivePayroll.AddObject(retroactivePayroll);
                            SetCreatedProperties(retroactivePayroll);
                        }
                        else
                        {
                            SetModifiedProperties(retroactivePayroll);
                        }

                        #region Common

                        retroactivePayroll.TimePeriodId = iDTO.RetroactivePayrollInput.TimePeriodId;
                        retroactivePayroll.Name = iDTO.RetroactivePayrollInput.Name;
                        retroactivePayroll.Note = iDTO.RetroactivePayrollInput.Note != null ? iDTO.RetroactivePayrollInput.Note.ToString() : String.Empty;
                        retroactivePayroll.DateFrom = iDTO.RetroactivePayrollInput.DateFrom;
                        retroactivePayroll.DateTo = iDTO.RetroactivePayrollInput.DateTo;

                        #endregion

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region RetroactivePayrollAccount

                        if (iDTO.RetroactivePayrollInput.RetroactivePayrollAccounts != null)
                        {
                            foreach (var retroactivePayrollAccountInput in iDTO.RetroactivePayrollInput.RetroactivePayrollAccounts)
                            {
                                AccountDim accountDim = GetAccountDim(retroactivePayrollAccountInput.AccountDimId);
                                if (accountDim == null)
                                    continue;

                                RetroactivePayrollAccount retroAccount = null;
                                if (retroactivePayrollAccountInput.RetroactivePayrollAccountId == 0)
                                {
                                    #region Add

                                    retroAccount = new RetroactivePayrollAccount()
                                    {
                                        RetroactivePayrollId = retroactivePayroll.RetroactivePayrollId,
                                    };

                                    SetCreatedProperties(retroAccount);
                                    entities.RetroactivePayrollAccount.AddObject(retroAccount);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    retroAccount = retroactivePayroll.RetroactivePayrollAccount.FirstOrDefault(x => x.RetroactivePayrollAccountId == retroactivePayrollAccountInput.RetroactivePayrollAccountId);
                                    if (retroAccount != null)
                                        SetModifiedProperties(retroAccount);

                                    #endregion
                                }

                                #region Common

                                if (retroAccount != null)
                                {
                                    retroAccount.AccountDimId = accountDim.AccountDimId;
                                    retroAccount.AccountStdId = accountDim.IsStandard ? retroactivePayrollAccountInput.AccountId : null;
                                    retroAccount.AccountInternalId = accountDim.IsInternal ? retroactivePayrollAccountInput.AccountId : null;
                                    retroAccount.Type = (int)retroactivePayrollAccountInput.Type;
                                    retroAccount.State = (int)retroactivePayrollAccountInput.State;
                                }

                                #endregion

                            }
                        }

                        #endregion

                        #region RetroactivePayrollEmployee

                        if (iDTO.RetroactivePayrollInput.RetroactivePayrollEmployees != null)
                        {
                            #region Delete rows that exists in db but not in input.

                            List<RetroactivePayrollEmployee> existingRetroEmployees = retroactivePayroll.RetroactivePayrollEmployee.Where(x => x.State == (int)SoeEntityState.Active).ToList();
                            foreach (RetroactivePayrollEmployee existingRetroEmployee in existingRetroEmployees)
                            {
                                if (!iDTO.RetroactivePayrollInput.RetroactivePayrollEmployees.Any(x => x.RetroactivePayrollEmployeeId == existingRetroEmployee.RetroactivePayrollEmployeeId))
                                    ChangeEntityState(existingRetroEmployee, SoeEntityState.Deleted);
                            }

                            #endregion

                            #region Add/Update

                            foreach (var retroactivePayrollEmployeeInput in iDTO.RetroactivePayrollInput.RetroactivePayrollEmployees)
                            {
                                RetroactivePayrollEmployee retroEmployee = null;
                                if (retroactivePayrollEmployeeInput.RetroactivePayrollEmployeeId == 0)
                                {
                                    #region Add

                                    retroEmployee = new RetroactivePayrollEmployee()
                                    {
                                        RetroactivePayrollId = retroactivePayroll.RetroactivePayrollId,
                                        ActorCompanyId = this.actorCompanyId,
                                        EmployeeId = retroactivePayrollEmployeeInput.EmployeeId,
                                        Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Saved,
                                    };
                                    SetCreatedProperties(retroEmployee);
                                    entities.RetroactivePayrollEmployee.AddObject(retroEmployee);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    retroEmployee = retroactivePayroll.RetroactivePayrollEmployee.FirstOrDefault(x => x.RetroactivePayrollEmployeeId == retroactivePayrollEmployeeInput.RetroactivePayrollEmployeeId);
                                    if (retroEmployee != null)
                                        SetModifiedProperties(retroEmployee);

                                    #endregion
                                }

                                #region Common

                                if (retroEmployee != null)
                                {
                                    retroEmployee.Note = retroactivePayrollEmployeeInput.Note;
                                    retroEmployee.State = (int)retroactivePayrollEmployeeInput.State;
                                }

                                #endregion
                            }

                            #endregion
                        }

                        #endregion

                        oDTO.Result = Save();

                        TryCommit(oDTO);

                        retroactivePayrollId = retroactivePayroll.RetroactivePayrollId;
                    }

                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = retroactivePayrollId;
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SaveRetroactivePayrollOutcomeOutputDTO TaskSaveRetroactivePayrollOutcome()
        {
            var (iDTO, oDTO) = InitTask<SaveRetroactivePayrollOutcomeInputDTO, SaveRetroactivePayrollOutcomeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroOutcomesInput == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        List<RetroactivePayrollOutcome> retroOutcomes = GetRetroactivePayrollOutcomesWithProduct(iDTO.RetroactivePayrollId, iDTO.EmployeeId);

                        #endregion

                        #region Perform

                        foreach (RetroactivePayrollOutcomeDTO retroOutcomeInput in iDTO.RetroOutcomesInput)
                        {
                            RetroactivePayrollOutcome retroactivePayrollOutcome = retroOutcomes.FirstOrDefault(x => x.RetroactivePayrollOutcomeId == retroOutcomeInput.RetroactivePayrollOutcomeId);
                            if (retroactivePayrollOutcome != null)
                            {
                                retroactivePayrollOutcome.IsSpecifiedUnitPrice = retroOutcomeInput.IsSpecifiedUnitPrice;
                                retroactivePayrollOutcome.SpecifiedUnitPrice = retroOutcomeInput.SpecifiedUnitPrice;
                                retroactivePayrollOutcome.Amount = retroOutcomeInput.Amount;
                                SetModifiedProperties(retroactivePayrollOutcome);
                            }
                        }

                        #endregion

                        oDTO.Result = Save();

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DeleteRetroactivePayrollOutputDTO TaskDeleteRetroactivePayroll()
        {
            var (iDTO, oDTO) = InitTask<DeleteRetroactivePayrollInputDTO, DeleteRetroactivePayrollOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            bool updateStatus = false;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    var retro = GetRetro(iDTO.RetroactivePayrollId);
                    if (!retro.Result.Success)
                    {
                        oDTO.Result = retro.Result;
                        return oDTO;
                    }

                    LoadRetroactivePayrollTransactions(retro.Payroll, out bool hasTransactions, doAbortIfHasTransactions: true);
                    if (hasTransactions)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11039, "Det finns transaktioner som måste backas innan retroaktiv lön kan tas bort"));
                        return oDTO;
                    }

                    foreach (RetroactivePayrollEmployee retroEmployee in retro.Employees.Where(rpe => rpe.IsValidToDeleteOutcomes()))
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);
                            
                            oDTO.Result = SetRetroPayrollEmployeeToDeleted(retroEmployee, doDeleteEmployee: true, doDeleteOutcome: true, doDeleteBasis: true, doDeleteTransactions: true, retroEmployeeStatusAfter: TermGroup_SoeRetroactivePayrollEmployeeStatus.Saved);
                            if (!oDTO.Result.Success)
                                break;

                            oDTO.Result = Save();
                            if (!TryCommit(oDTO))
                                break;

                            updateStatus = true;
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
                    
                    entities.Connection.Close();
                }
            }

            if (updateStatus)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    try
                    {
                        InitContext(taskEntities);

                        RetroactivePayroll retroPayroll = GetRetroactivePayroll(iDTO.RetroactivePayrollId, loadRetroEmployee: true);
                        if (retroPayroll?.RetroactivePayrollEmployee?.All(i => i.State == (int)SoeEntityState.Deleted) == true)
                        {
                            ActionResult result = ChangeEntityState(entities, retroPayroll, SoeEntityState.Deleted, saveChanges: true, user: GetUserFromCache());                            
                            if (!result.Success)
                                return oDTO;
                        }
                    }
                    catch (Exception ex)
                    {
                        oDTO.Result.Exception = ex;
                        LogError(ex);
                    }
                    finally
                    {

                        entities.Connection.Close();
                    }
                }
            }
            
            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CalculateRetroactivePayrollOutputDTO TaskCalculateRetroactivePayroll()
        {
            var (iDTO, oDTO) = InitTask<CalculateRetroactivePayrollInputDTO, CalculateRetroactivePayrollOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollInput == null || iDTO.RetroactivePayrollInput.RetroactivePayrollId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }
            bool updateStatus = false;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    var retro = GetRetro(iDTO.RetroactivePayrollInput.RetroactivePayrollId, iDTO.FilterEmployeeIds);
                    if (!retro.Result.Success)
                    {
                        oDTO.Result = retro.Result;
                        return oDTO;
                    }

                    TimePeriod timePeriod = GetTimePeriodWithHead(retro.Payroll.TimePeriodId);
                    if (timePeriod == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.RetroactivePayrollNotFound, GetText(8250, "Löneperiod hittades inte"));
                        return oDTO;
                    }

                    List<RetroactivePayroll> existingRetroPayrolls = GetOverlappingRetroactivePayrollsWithEmployee(retro.Payroll);
                    List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache();

                    //Internal cache
                    List<RetroactivePayrollBasis> retroactivePayrollBasesAlReadyRetro = new List<RetroactivePayrollBasis>();

                    #endregion

                    #region Perform

                    InitEvaluatePriceFormulaInputDTO(employeeIds: retro.Employees.Select(e => e.EmployeeId).ToList());

                    foreach (RetroactivePayrollEmployee retroEmployee in retro.Employees)
                    {
                        #region RetroEmployee

                        if (!iDTO.IncludeAlreadyCalculated && retroEmployee.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated)
                            continue;

                        List<RetroactivePayrollOutcome> retroOutcomes = GetRetroactivePayrollOutcomes(retroEmployee, out bool hasAnyOutcomeTransactions);
                        if (hasAnyOutcomeTransactions)
                            continue;

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            try
                            {
                                #region Prereq

                                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(retroEmployee.EmployeeId);
                                if (employee == null)
                                {
                                    retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Error;                       
                                    continue;
                                }                                

                                List<EmployeeTimePeriod> lockedOrPaidEmployeePeriods = GetEmployeeTimePeriodsWithPeriodsUsePayrollDates(retro.DateFrom, retro.DateTo, retroEmployee.EmployeeId);
                                if (IsEmployeeInRetroAndOpenedPeriod(existingRetroPayrolls, lockedOrPaidEmployeePeriods, employee.EmployeeId))
                                {
                                    retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.EmployeeExistsInOtherActiveRetro;
                                    continue;
                                }

                                Employment employmentForPeriod = employee.GetEmployment(timePeriod.StartDate, timePeriod.StopDate, false) ?? employee.GetLastEmployment();
                                if (employmentForPeriod == null)
                                    continue;

                                PayrollGroup payrollGroup = employmentForPeriod.GetPayrollGroup(timePeriod.StartDate, timePeriod.StopDate, payrollGroups, forward: false);
                                if (payrollGroup == null || payrollGroup.TimePeriodHeadId != timePeriod.TimePeriodHead.TimePeriodHeadId)
                                {
                                    retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.EmployeeHasChangedTimePeriodHead;
                                    continue;
                                }

                                #endregion

                                #region Delete old outcomes

                                oDTO.Result = SetRetroPayrollOutcomesToDeleted(retroOutcomes, saveChanges: false);
                                if (!oDTO.Result.Success)
                                    break;

                                #endregion

                                #region Fetch and validate transactions

                                #region Payroll

                                List<TimePayrollTransaction> timePayrollTransactionsValid = new List<TimePayrollTransaction>();
                                List<TimePayrollTransaction> timePayrollTransactionsAlreadyRetro = new List<TimePayrollTransaction>();

                                List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForRetroCalculation(retroEmployee.EmployeeId, retro.DateFrom, retro.DateTo, timePeriod, lockedOrPaidEmployeePeriods.Select(x => x.TimePeriodId).ToList());
                                if (timePayrollTransactions.Count > 0)
                                {
                                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                                    {
                                        if (timePayrollTransaction.IsRetroactive)
                                        {
                                            EmployeeTimePeriod employeeTimePeriod = timePayrollTransaction != null ? lockedOrPaidEmployeePeriods.FirstOrDefault(i => i.TimePeriodId == timePayrollTransaction.TimePeriodId.Value) : null;
                                            if (employeeTimePeriod != null && (employeeTimePeriod.Status == (int)SoeEmployeeTimePeriodStatus.Locked || employeeTimePeriod.Status == (int)SoeEmployeeTimePeriodStatus.Paid))
                                            {
                                                #region Validate Monthlysalary before considering it to be retro on retro

                                                #region Bugg 117300 Explanation:

                                                //You first do a retro for period X that ends up in period Y. Then you do a retro for period Y.
                                                //We discover the retrotrans that is in period Y and we assume that it will then be retro on retro,
                                                //i.e.that we first negate it and then calculate a new salary.That is, we assume that the retrotrans is made for period Y but it is actually made for period X.
                                                //Solution: Only add the retrotrans if the retrotrans period (payrolldates, see how existingRetroPayrolls is fetched) is overlapped by retro y's period (dateFrom, dateTo).

                                                #endregion

                                                if (timePayrollTransaction.IsMonthlySalaryAndFixed())
                                                {
                                                    if (!retroactivePayrollBasesAlReadyRetro.Any(x => x.RetroactivePayrollOutcomeId == timePayrollTransaction.RetroactivePayrollOutcomeId))
                                                        retroactivePayrollBasesAlReadyRetro.AddRange(GetRetroactivePayrollBasisWithTransactionBasis(timePayrollTransaction.RetroactivePayrollOutcomeId.Value));
                                                    
                                                    RetroactivePayrollBasis retroactivePayrollBasis = retroactivePayrollBasesAlReadyRetro.FirstOrDefault(x => x.RetroactivePayrollOutcomeId == timePayrollTransaction.RetroactivePayrollOutcomeId.Value &&  x.RetroTimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId);
                                                    if (retroactivePayrollBasis == null)
                                                        continue;

                                                    //Monthlysalaryandfixed must have timeperiodid
                                                    if (!retroactivePayrollBasis.BasisTimePayrollTransaction.TimePeriodId.HasValue)
                                                        continue;

                                                    TimePeriod retroTransTimePeriod = GetTimePeriodFromCache(retroactivePayrollBasis.BasisTimePayrollTransaction.TimePeriodId.Value);
                                                    if (retroTransTimePeriod == null)
                                                        continue;

                                                    //Check if retrotrans period is overlapped by the current retro period
                                                    if (!CalendarUtility.IsDatesOverlapping(retroTransTimePeriod.PayrollStartDate.Value, retroTransTimePeriod.PayrollStopDate.Value, retro.DateFrom, retro.DateTo ?? DateTime.MaxValue))
                                                        continue;

                                                }

                                                #endregion


                                                timePayrollTransactionsAlreadyRetro.Add(timePayrollTransaction);
                                            }
                                        }
                                        else
                                        {
                                            if (timePayrollTransaction.IsValidForRetro())
                                                timePayrollTransactionsValid.Add(timePayrollTransaction);
                                        }
                                    }
                                }

                                #endregion

                                #region Schedule

                                List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsValid = new List<TimePayrollScheduleTransaction>();
                                List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsAlreadyRetro = new List<TimePayrollScheduleTransaction>();

                                List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(retroEmployee.EmployeeId, retro.DateFrom, retro.DateTo, null, SoeTimePayrollScheduleTransactionType.Absence);
                                if (timePayrollScheduleTransactions.Count > 0)
                                {
                                    foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in timePayrollScheduleTransactions)
                                    {
                                        if (timePayrollScheduleTransaction.IsRetroactive)
                                        {
                                            EmployeeTimePeriod employeeTimePeriod = timePayrollScheduleTransaction != null ? lockedOrPaidEmployeePeriods.FirstOrDefault(i => i.TimePeriodId == timePayrollScheduleTransaction.TimePeriodId.Value) : null;
                                            if (employeeTimePeriod != null && employeeTimePeriod.Status == (int)SoeEmployeeTimePeriodStatus.Paid)
                                                timePayrollScheduleTransactionsAlreadyRetro.Add(timePayrollScheduleTransaction);
                                        }
                                        else
                                        {
                                            if (timePayrollScheduleTransaction.IsValidForRetro())
                                                timePayrollScheduleTransactionsValid.Add(timePayrollScheduleTransaction);
                                        }
                                    }
                                }

                                #endregion

                                if (timePayrollTransactionsValid.Count == 0 && timePayrollScheduleTransactionsValid.Count == 0)
                                {
                                    retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.EmployeeHasNoBasis;
                                    continue;
                                }

                                #endregion

                                #region Calculate retro amount

                                List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

                                List<int> timeBlockDateIdsPayroll = timePayrollTransactionsValid.Select(i => i.TimeBlockDateId).ToList();
                                List<int> timeBlockDateIdsPayrollAlreadyRetro = timePayrollTransactionsAlreadyRetro.Select(i => i.TimeBlockDateId).ToList();
                                List<int> timeBlockDateIdsSchedule = timePayrollScheduleTransactionsValid.Select(i => i.TimeBlockDateId).ToList();
                                List<int> timeBlockDateIdsSchedulAlreadyRetro = timePayrollScheduleTransactionsAlreadyRetro.Select(i => i.TimeBlockDateId).ToList();
                                List<int> timeBlockDateIds = timeBlockDateIdsPayroll.Union(timeBlockDateIdsPayrollAlreadyRetro).Union(timeBlockDateIdsSchedule).Union(timeBlockDateIdsSchedulAlreadyRetro).ToList();
                                List<TimeBlockDate> timeBlockDates = GetTimeBlockDates(retroEmployee.EmployeeId, timeBlockDateIds);

                                foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(i => i.Date))
                                {
                                    Employment employmentForDate = employee.GetEmployment(timeBlockDate.Date);

                                    #region Payroll - valid for retro

                                    if (timeBlockDateIdsPayroll.Contains(timeBlockDate.TimeBlockDateId))
                                    {
                                        List<TimePayrollTransaction> timePayrollTransactionsForDate = timePayrollTransactionsValid.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                                        foreach (var transactionsByProduct in timePayrollTransactionsForDate.GroupBy(i => i.ProductId))
                                        {
                                            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(transactionsByProduct.Key);
                                            if (payrollProduct == null)
                                                continue;

                                            PayrollProductSetting productSetting = payrollProduct?.GetSetting(payrollGroup?.PayrollGroupId);
                                            if (productSetting?.DontIncludeInRetroactivePayroll ?? false)
                                                continue;

                                            List<TimePayrollTransaction> addedTimePayrollTransactions = transactionsByProduct.Where(i => i.IsAdded).ToList();
                                            if (addedTimePayrollTransactions.Count > 0)
                                                retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollTransactionsAdded(employee, retro.DateFrom, retro.DateTo, timeBlockDate, payrollProduct, addedTimePayrollTransactions));

                                            List<TimePayrollTransaction> fixedTimePayrollTransactions = transactionsByProduct.Where(i => i.IsFixed).ToList();
                                            if (fixedTimePayrollTransactions.Count > 0)
                                                retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollTransactionsFixed(employee, employmentForDate, retro.DateFrom, retro.DateTo, timeBlockDate, payrollProduct, fixedTimePayrollTransactions));

                                            List<TimePayrollTransaction> otherTimePayrollTransactions = transactionsByProduct.Where(i => !i.IsAdded && !i.IsFixed).ToList();
                                            if (otherTimePayrollTransactions.Count > 0)
                                                retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollTransactions(employee, employmentForDate, timeBlockDate, payrollProduct, otherTimePayrollTransactions));
                                        }
                                    }

                                    #endregion

                                    #region Payroll - already retro

                                    if (timeBlockDateIdsPayrollAlreadyRetro.Contains(timeBlockDate.TimeBlockDateId))
                                    {
                                        List<TimePayrollTransaction> timePayrollTransactionsForDate = timePayrollTransactionsAlreadyRetro.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                                        foreach (var transactionsByProduct in timePayrollTransactionsForDate.GroupBy(i => i.ProductId))
                                        {
                                            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(transactionsByProduct.Key);
                                            if (payrollProduct == null)
                                                continue;

                                            retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollTransactionsAlreadyRetro(employee, employmentForDate, timeBlockDate, payrollProduct, transactionsByProduct.ToList()));
                                        }
                                    }

                                    #endregion

                                    #region Schedule - valid for retro calculation

                                    if (timeBlockDateIdsSchedule.Contains(timeBlockDate.TimeBlockDateId))
                                    {
                                        List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsForDate = timePayrollScheduleTransactionsValid.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                                        foreach (var transactionsByProduct in timePayrollScheduleTransactionsForDate.GroupBy(i => i.ProductId))
                                        {
                                            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(transactionsByProduct.Key);
                                            if (payrollProduct == null)
                                                continue;

                                            PayrollProductSetting productSetting = payrollProduct?.GetSetting(payrollGroup?.PayrollGroupId);
                                            if (productSetting?.DontIncludeInRetroactivePayroll ?? false)
                                                continue;

                                            retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollScheduleTransactions(employee, employmentForDate, timeBlockDate, payrollProduct, transactionsByProduct.ToList()));
                                        }
                                    }

                                    #endregion

                                    #region Schedule - Already retro

                                    if (timeBlockDateIdsSchedulAlreadyRetro.Contains(timeBlockDate.TimeBlockDateId))
                                    {
                                        List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsForDate = timePayrollScheduleTransactionsAlreadyRetro.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                                        foreach (var transactionsByProduct in timePayrollScheduleTransactionsForDate.GroupBy(i => i.ProductId))
                                        {
                                            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(transactionsByProduct.Key);
                                            if (payrollProduct == null)
                                                continue;

                                            retroCalculations.AddRange(CalculateRetroactivePayrollFromTimePayrollScheduleTransactionsAlreadyRetro(employee, employmentForDate, timeBlockDate, payrollProduct, transactionsByProduct.ToList()));
                                        }
                                    }

                                    #endregion
                                }

                                if (retroCalculations.Count == 0)
                                {
                                    retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated;
                                    continue;
                                }

                                #endregion

                                #region Create outcome

                                foreach (var retroCalculationsByType in retroCalculations.GroupBy(i => i.IsReversed))
                                {
                                    bool isReversed = retroCalculationsByType.Key;

                                    foreach (var retroCalculationsByProduct in retroCalculationsByType.GroupBy(i => i.PayrollProductId))
                                    {
                                        int productId = retroCalculationsByProduct.Key;

                                        foreach (var retroCalculationsByTransactionUnitPrice in retroCalculationsByProduct.GroupBy(i => i.TransactionUnitPrice))
                                        {
                                            decimal transactionUnitPrice = retroCalculationsByTransactionUnitPrice.Key;

                                            foreach (var retroCalculationsByRetroUnitPrice in retroCalculationsByTransactionUnitPrice.GroupBy(i => i.RetroUnitPrice))
                                            {
                                                decimal retroCalculationUnitPrice = retroCalculationsByRetroUnitPrice.Key;

                                                foreach (var retroCalculationsByResultType in retroCalculationsByRetroUnitPrice.GroupBy(i => i.ResultType))
                                                {
                                                    TermGroup_PayrollResultType resultType = retroCalculationsByResultType.Key;

                                                    foreach (var retroCalculationsByErrorCode in retroCalculationsByResultType.GroupBy(i => i.ErrorCode))
                                                    {
                                                        int errorCode = retroCalculationsByErrorCode.Key;
                                                        decimal quantity = RetroactivePayrollCalculationDTO.GetTotalQuantity(retroCalculationsByErrorCode.ToList());
                                                        var (_, retroAmount) = CalculateRetroAmount(transactionUnitPrice, retroCalculationUnitPrice, quantity, resultType);

                                                        RetroactivePayrollOutcome retroOutcome = new RetroactivePayrollOutcome()
                                                        {
                                                            Quantity = Decimal.Round(quantity, 4, MidpointRounding.AwayFromZero),
                                                            Amount = Decimal.Round(retroAmount, 4, MidpointRounding.AwayFromZero),
                                                            TransactionUnitPrice = Decimal.Round(transactionUnitPrice, 4, MidpointRounding.AwayFromZero),
                                                            RetroUnitPrice = Decimal.Round(retroCalculationUnitPrice, 4, MidpointRounding.AwayFromZero),
                                                            IsSpecifiedUnitPrice = false,
                                                            SpecifiedUnitPrice = null,
                                                            ErrorCode = errorCode,
                                                            IsRetroCalculated = errorCode == 0,
                                                            ResultType = (int)resultType,
                                                            IsReversed = isReversed,

                                                            //Set FK
                                                            ActorCompanyId = actorCompanyId,
                                                            EmployeeId = retroEmployee.EmployeeId,
                                                            PayrollProductId = productId,

                                                            //Set references
                                                            RetroactivePayrollEmployee = retroEmployee,
                                                        };
                                                        SetCreatedProperties(retroOutcome);
                                                        entities.RetroactivePayrollOutcome.AddObject(retroOutcome);

                                                        foreach (RetroactivePayrollCalculationDTO retroCalculation in retroCalculationsByErrorCode.OrderBy(i => i.Date))
                                                        {
                                                            #region RetroactivePayrollBasis

                                                            foreach (TimePayrollTransaction timePayrollTransaction in retroCalculation.TimePayrollTransactionBasis)
                                                            {
                                                                RetroactivePayrollBasis retroBasis = new RetroactivePayrollBasis()
                                                                {
                                                                    State = (int)SoeEntityState.Active,

                                                                    //Set references
                                                                    BasisTimePayrollTransaction = timePayrollTransaction,
                                                                    RetroTimePayrollTransaction = null,
                                                                    RetroactivePayrollOutcome = retroOutcome,
                                                                };
                                                                entities.RetroactivePayrollBasis.AddObject(retroBasis);
                                                                retroOutcome.RetroactivePayrollBasis.Add(retroBasis);
                                                            }

                                                            #endregion

                                                            #region RetroactivePayrollScheduleBasis

                                                            foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in retroCalculation.TimePayrollScheduleTransactionBasis)
                                                            {
                                                                RetroactivePayrollScheduleBasis retroBasis = new RetroactivePayrollScheduleBasis()
                                                                {
                                                                    State = (int)SoeEntityState.Active,

                                                                    //Set references
                                                                    BasisTimePayrollScheduleTransaction = timePayrollScheduleTransaction,
                                                                    RetroTimePayrollScheduleTransaction = null,
                                                                    RetroactivePayrollOutcome = retroOutcome,
                                                                };
                                                                entities.RetroactivePayrollScheduleBasis.AddObject(retroBasis);
                                                                retroOutcome.RetroactivePayrollScheduleBasis.Add(retroBasis);
                                                            }

                                                            #endregion
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion

                                //Set status
                                retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated;
                            }
                            catch
                            {
                                //Set status
                                retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Error;
                            }
                            finally
                            {
                                if (oDTO.Result.Success)
                                {
                                    oDTO.Result = Save();
                                    TryCommit(oDTO);
                                }                                    
                            }

                            if (!oDTO.Result.Success)
                                break;

                            updateStatus = true;
                        }

                        #endregion
                    }

                    #endregion
                       
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            #region Retro status

            if (updateStatus)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    try
                    {
                        InitContext(taskEntities);

                        ActionResult result = UpdateRetroactivePayrollStatus(iDTO.RetroactivePayrollInput.RetroactivePayrollId);
                        if (!result.Success)
                            return oDTO;
                    }
                    catch (Exception ex)
                    {
                        oDTO.Result.Exception = ex;
                        LogError(ex);
                    }
                    finally
                    {

                        entities.Connection.Close();
                    }
                }
            }

            #endregion

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DeleteRetroactivePayrollOutcomesOutputDTO TaskDeleteRetroactivePayrollOutcomes()
        {
            var (iDTO, oDTO) = InitTask<DeleteRetroactivePayrollOutcomesInputDTO, DeleteRetroactivePayrollOutcomesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollInput == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            bool updateStatus = false;

            using (CompEntities taskEntities = new CompEntities())
            {
                try
                {                
                    InitContext(taskEntities);

                    #region Prereq

                    var retro = GetRetro(iDTO.RetroactivePayrollInput.RetroactivePayrollId);
                    if (!retro.Result.Success)
                    {
                        oDTO.Result = retro.Result;
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    foreach (RetroactivePayrollEmployee retroEmployee in retro.Employees.Where(rpe => rpe.IsValidToDeleteOutcomes()))
                    {
                        List<RetroactivePayrollOutcome> retroOutcomes = GetRetroactivePayrollOutcomes(retroEmployee);
                        if (retroOutcomes.IsNullOrEmpty())
                            continue;

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            oDTO.Result = SetRetroPayrollEmployeeToDeleted(retroEmployee, retroOutcomes: retroOutcomes, doDeleteOutcome: true, retroEmployeeStatusAfter: TermGroup_SoeRetroactivePayrollEmployeeStatus.Saved);
                            if (!oDTO.Result.Success)
                                break;

                            oDTO.Result = Save();
                            if (!TryCommit(oDTO))
                                break;

                            updateStatus = true;
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {

                    entities.Connection.Close();
                }
            }

            #region Retro status

            if (updateStatus)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    try
                    {
                        InitContext(taskEntities);

                        ActionResult result = UpdateRetroactivePayrollStatus(iDTO.RetroactivePayrollInput.RetroactivePayrollId);
                        if (!result.Success)
                            return oDTO;
                    }
                    catch (Exception ex)
                    {
                        oDTO.Result.Exception = ex;
                        LogError(ex);
                    }
                    finally
                    {

                        entities.Connection.Close();
                    }
                }                
            }

            #endregion

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CreateRetroactivePayrollTransactionsOutputDTO TaskCreateRetroactivePayrollTransactions()
        {
            var (iDTO, oDTO) = InitTask<CreateRetroactivePayrollTransactionsInputDTO, CreateRetroactivePayrollTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollInput == null || iDTO.RetroactivePayrollInput.RetroactivePayrollId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            bool updateStatus = false;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {

                    #region Prereq

                    var retro = GetRetro(iDTO.RetroactivePayrollInput.RetroactivePayrollId, iDTO.FilterEmployeeIds, loadRetroAccounting: true);
                    if (!retro.Result.Success)
                    {
                        oDTO.Result = retro.Result;
                        return oDTO;
                    }

                    TimePeriod timePeriod = GetTimePeriodWithHead(retro.Payroll.TimePeriodId);
                    if (timePeriod == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.RetroactivePayrollNotFound, GetText(8250, "Löneperiod hittades inte"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    List<AccountDim> accountDims = GetAccountDimsFromCache();

                    #endregion

                    #region Perform

                    foreach (RetroactivePayrollEmployee retroEmployee in retro.Employees.Where(i => i.IsValidToCreateTransactions()))
                    {
                        #region RetroactivePayrollEmployee

                        Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(retroEmployee.EmployeeId);
                        if (employee == null)
                            continue;

                        Employment employment = employee.GetEmployment(timePeriod.StartDate, timePeriod.StopDate, forward: false) ?? employee.GetLastEmployment();
                        if (employment == null)
                            continue;

                        PayrollGroup payrollGroup = employment.GetPayrollGroup(timePeriod.StartDate, timePeriod.StopDate, GetPayrollGroupsFromCache(), forward: false);
                        if (payrollGroup == null || payrollGroup.TimePeriodHeadId != timePeriod.TimePeriodHead.TimePeriodHeadId)
                            continue;

                        #endregion

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            #region Perform

                            #region Check that Employee hasnt transactions already

                            List<RetroactivePayrollOutcome> retroOutcomes = GetRetroactivePayrollOutcomes(retroEmployee).Where(i => i.IsValidForTransaction()).ToList();
                            if (retroOutcomes.IsNullOrEmpty())
                                continue;

                            bool hasEmployeeTransactions = false;
                            var retroOutcomeBasis = new List<Tuple<int, List<RetroactivePayrollBasis>, List<RetroactivePayrollScheduleBasis>>>();

                            foreach (RetroactivePayrollOutcome retroOutcome in retroOutcomes)
                            {
                                if (hasEmployeeTransactions || retroOutcomeBasis.Any(i => i.Item1 == retroOutcome.RetroactivePayrollOutcomeId))
                                    break;

                                List<RetroactivePayrollBasis> retroPayrollBasis = GetRetroactivePayrollBasisWithTransactionBasisAndAccounting(retroOutcome.RetroactivePayrollOutcomeId);
                                List<RetroactivePayrollScheduleBasis> retroPayrollScheduleBasis = GetRetroactivePayrollScheduleBasisWithTransactionBasisAndAccounting(retroOutcome.RetroactivePayrollOutcomeId);

                                if (retroPayrollBasis.Any(i => i.RetroTimePayrollTransactionId.HasValue) || retroPayrollScheduleBasis.Any(i => i.RetroTimePayrollScheduleTransactionId.HasValue))
                                    hasEmployeeTransactions = true;
                                else
                                    retroOutcomeBasis.Add(Tuple.Create(retroOutcome.RetroactivePayrollOutcomeId, retroPayrollBasis, retroPayrollScheduleBasis));
                            }

                            if (hasEmployeeTransactions)
                                continue;

                            #endregion

                            #region Create retro transactions

                            foreach (RetroactivePayrollOutcome retroOutcome in retroOutcomes.OrderByDescending(i => i.IsReversed))
                            {
                                #region Prereq

                                var outcomeBasis = retroOutcomeBasis.FirstOrDefault(i => i.Item1 == retroOutcome.RetroactivePayrollOutcomeId);
                                if (outcomeBasis == null)
                                    continue;

                                List<RetroactivePayrollBasis> retroPayrollBasis = outcomeBasis.Item2;
                                List<RetroactivePayrollScheduleBasis> retroPayrollScheduleBasis = outcomeBasis.Item3;
                                if (retroPayrollBasis.Count == 0 && retroPayrollScheduleBasis.Count == 0)
                                    continue;

                                List<TimePayrollTransaction> timePayrollTransactionsBasis = new List<TimePayrollTransaction>();
                                retroPayrollBasis.ForEach(x => timePayrollTransactionsBasis.Add(x.BasisTimePayrollTransaction));
                                List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsBasis = new List<TimePayrollScheduleTransaction>();
                                retroPayrollScheduleBasis.ForEach(x => timePayrollScheduleTransactionsBasis.Add(x.BasisTimePayrollScheduleTransaction));

                                #endregion

                                if (retroOutcome.IsReversed)
                                {
                                    #region Already retro outcome

                                    #region Payroll

                                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsBasis)
                                    {
                                        TimePayrollTransaction timePayrollTransactionReversed = CreateTimePayrollTransactionReversed(timePayrollTransaction, timePeriod.TimePeriodId, attestStateInitial.AttestStateId, retroOutcome);
                                        if (timePayrollTransactionReversed != null)
                                        {
                                            #region RetroactivePayrollBasis

                                            RetroactivePayrollBasis retroBasis = retroPayrollBasis.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.BasisTimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId);
                                            if (retroBasis != null)
                                                retroBasis.RetroTimePayrollTransaction = timePayrollTransactionReversed;

                                            #endregion
                                        }
                                    }

                                    #endregion

                                    #region Schedule

                                    foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in timePayrollScheduleTransactionsBasis)
                                    {
                                        TimePayrollScheduleTransaction timePayrollScheduleTransactionReversed = CreateTimePayrollScheduleTransactionReversed(timePayrollScheduleTransaction, timePeriod.TimePeriodId, retroOutcome);
                                        if (timePayrollScheduleTransactionReversed != null)
                                        {
                                            #region RetroactivePayrollScheduleBasis

                                            RetroactivePayrollScheduleBasis retroBasis = retroOutcome.RetroactivePayrollScheduleBasis.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.BasisTimePayrollScheduleTransactionId == timePayrollScheduleTransactionReversed.TimePayrollScheduleTransactionId);
                                            if (retroBasis != null)
                                                retroBasis.RetroTimePayrollScheduleTransaction = timePayrollScheduleTransactionReversed;

                                            #endregion
                                        }
                                    }

                                    #endregion

                                    #endregion
                                }
                                else
                                {
                                    #region Regular outcome

                                    if (!retroOutcome.PayrollProductReference.IsLoaded)
                                        retroOutcome.PayrollProductReference.Load();

                                    #region Payroll

                                    foreach (var timePayrollTransactionsBasisByDate in timePayrollTransactionsBasis.GroupBy(i => i.TimeBlockDateId))
                                    {
                                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(retroOutcome.EmployeeId, timePayrollTransactionsBasisByDate.Key);
                                        if (timeBlockDate == null)
                                            continue;

                                        foreach (var timePayrollTransactionsBasisByAccounting in timePayrollTransactionsBasisByDate.GroupBy(i => i.GetAccountingString(accountDims)))
                                        {
                                            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsBasisByAccounting)
                                            {
                                                #region TimePayrollTransaction

                                                var (unitPriceDiff, retroAmount) = CalculateRetroAmount(retroOutcome.TransactionUnitPrice, retroOutcome.IsSpecifiedUnitPrice ? retroOutcome.SpecifiedUnitPrice.Value : retroOutcome.RetroUnitPrice.Value, timePayrollTransaction.Quantity, (TermGroup_PayrollResultType)retroOutcome.ResultType);
                                                if (retroAmount == 0)
                                                    continue;

                                                TimePayrollTransaction retroTimePayrollTransaction = CreateTimePayrollTransaction(retroOutcome.PayrollProduct, timeBlockDate, timePayrollTransaction.Quantity, retroAmount, 0, unitPriceDiff, "", attestStateInitial.AttestStateId, timePeriod.TimePeriodId, retroOutcome.EmployeeId);
                                                if (retroTimePayrollTransaction == null)
                                                    continue;

                                                retroTimePayrollTransaction.IsFixed = timePayrollTransaction.IsFixed;
                                                retroTimePayrollTransaction.IsAdded = timePayrollTransaction.IsAdded;
                                                retroTimePayrollTransaction.EmployeeChildId = timePayrollTransaction.EmployeeChildId;
                                                retroTimePayrollTransaction.AddedDateFrom = timePayrollTransaction.AddedDateFrom;
                                                retroTimePayrollTransaction.AddedDateTo = timePayrollTransaction.AddedDateTo;
                                                retroTimePayrollTransaction.IsAdditionOrDeduction = timePayrollTransaction.IsAdditionOrDeduction;
                                                retroTimePayrollTransaction.IsSpecifiedUnitPrice = timePayrollTransaction.IsSpecifiedUnitPrice;
                                                retroTimePayrollTransaction.IsCentRounding = timePayrollTransaction.IsCentRounding;
                                                retroTimePayrollTransaction.IsQuantityRounding = timePayrollTransaction.IsQuantityRounding;
                                                retroTimePayrollTransaction.EmployeeVehicleId = timePayrollTransaction.EmployeeVehicleId;
                                                retroTimePayrollTransaction.UnionFeeId = timePayrollTransaction.UnionFeeId;
                                                retroTimePayrollTransaction.IsVacationReplacement = timePayrollTransaction.IsVacationReplacement;
                                                retroTimePayrollTransaction.RetroactivePayrollOutcomeId = retroOutcome.RetroactivePayrollOutcomeId;

                                                CreateTimePayrollTransactionExtended(retroTimePayrollTransaction, employee.EmployeeId, base.ActorCompanyId);
                                                if (retroTimePayrollTransaction.TimePayrollTransactionExtended != null && timePayrollTransaction.TimePayrollTransactionExtended != null)
                                                {
                                                    retroTimePayrollTransaction.TimePayrollTransactionExtended.IsDistributed = timePayrollTransaction.TimePayrollTransactionExtended.IsDistributed;
                                                    retroTimePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit;
                                                    retroTimePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays;
                                                    retroTimePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays;
                                                    retroTimePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor;
                                                }

                                                #endregion

                                                #region RetroactivePayrollBasis

                                                if (!timePayrollTransaction.BasisRetroactivePayrollBasis.IsLoaded)
                                                    timePayrollTransaction.BasisRetroactivePayrollBasis.Load();

                                                List<RetroactivePayrollBasis> transactionRetroBasisis = (from b in timePayrollTransaction.BasisRetroactivePayrollBasis
                                                                                                         where b.RetroactivePayrollOutcomeId == retroOutcome.RetroactivePayrollOutcomeId &&
                                                                                                         b.State == (int)SoeEntityState.Active
                                                                                                         select b).ToList();

                                                foreach (RetroactivePayrollBasis transactionRetroBasis in transactionRetroBasisis)
                                                {
                                                    transactionRetroBasis.RetroTimePayrollTransaction = retroTimePayrollTransaction;
                                                }

                                                #endregion

                                                #region Accounting

                                                if (!timePayrollTransaction.AccountInternal.IsLoaded)
                                                    timePayrollTransaction.AccountInternal.Load();

                                                foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
                                                {
                                                    RetroactivePayrollAccount retroAccount = retro.Payroll.RetroactivePayrollAccount?.FirstOrDefault(i => i.AccountDimId == accountDim.AccountDimId);

                                                    if (accountDim.IsStandard)
                                                    {
                                                        if (retroAccount != null && retroAccount.Type == (int)TermGroup_RetroactivePayrollAccountType.OtherAccount && retroAccount.AccountStdId.HasValue)
                                                        {
                                                            //AccountStd other
                                                            AccountStd accountStd = GetAccountStd(retroAccount.AccountStdId.Value);
                                                            if (accountStd != null)
                                                                retroTimePayrollTransaction.AccountStdId = accountStd.AccountId;
                                                        }
                                                        else
                                                        {
                                                            //AccountStd according to transaction
                                                            retroTimePayrollTransaction.AccountStdId = timePayrollTransaction.AccountStdId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (retroAccount != null && retroAccount.Type == (int)TermGroup_RetroactivePayrollAccountType.OtherAccount && retroAccount.AccountInternalId.HasValue)
                                                        {
                                                            //AccountInternal other
                                                            AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(retroAccount.AccountInternalId.Value);
                                                            if (accountInternal != null)
                                                                retroTimePayrollTransaction.AccountInternal.Add(accountInternal);
                                                        }
                                                        else
                                                        {
                                                            //AccountInternal according to transaction
                                                            AccountInternal accountInternal = timePayrollTransaction.AccountInternal.FirstOrDefault(i => i.Account != null && i.Account.AccountDimId == accountDim.AccountDimId);
                                                            if (accountInternal != null)
                                                                retroTimePayrollTransaction.AccountInternal.Add(accountInternal);
                                                        }
                                                    }
                                                }

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Schedule

                                    foreach (var timePayrollScheduleTransactionsBasisByDate in timePayrollScheduleTransactionsBasis.GroupBy(i => i.TimeBlockDateId))
                                    {
                                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(retroOutcome.EmployeeId, timePayrollScheduleTransactionsBasisByDate.Key);
                                        if (timeBlockDate == null)
                                            continue;

                                        foreach (var timePayrollTransactionsBasisByAccounting in timePayrollScheduleTransactionsBasisByDate.GroupBy(i => i.GetAccountingString(accountDims)))
                                        {
                                            foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in timePayrollTransactionsBasisByAccounting)
                                            {
                                                #region TimePayrollScheduleTransaction

                                                var (unitPriceDiff, retroAmount) = CalculateRetroAmount(retroOutcome.TransactionUnitPrice, retroOutcome.IsSpecifiedUnitPrice ? retroOutcome.SpecifiedUnitPrice.Value : retroOutcome.RetroUnitPrice.Value, timePayrollScheduleTransaction.Quantity, (TermGroup_PayrollResultType)retroOutcome.ResultType);

                                                TimePayrollScheduleTransaction retroTimePayrollScheduleTransaction = CreateTimePayrollScheduleTransaction(retroOutcome.PayrollProduct, timeBlockDate, timePayrollScheduleTransaction.Quantity, retroAmount, 0, unitPriceDiff, (int)SoeTimePayrollScheduleTransactionType.Absence, retroOutcome.EmployeeId, timePeriodId: timePeriod.TimePeriodId);
                                                if (retroTimePayrollScheduleTransaction == null)
                                                    continue;

                                                retroTimePayrollScheduleTransaction.RetroactivePayrollOutcomeId = retroOutcome.RetroactivePayrollOutcomeId;

                                                #endregion

                                                #region RetroactivePayrollScheduleBasis

                                                if (!timePayrollScheduleTransaction.BasisRetroactivePayrollScheduleBasis.IsLoaded)
                                                    timePayrollScheduleTransaction.BasisRetroactivePayrollScheduleBasis.Load();

                                                List<RetroactivePayrollScheduleBasis> transactionRetroBasisis = (from b in timePayrollScheduleTransaction.BasisRetroactivePayrollScheduleBasis
                                                                                                                 where b.RetroactivePayrollOutcomeId == retroOutcome.RetroactivePayrollOutcomeId &&
                                                                                                                 b.State == (int)SoeEntityState.Active
                                                                                                                 select b).ToList();

                                                foreach (RetroactivePayrollScheduleBasis transactionRetroBasis in transactionRetroBasisis)
                                                {
                                                    transactionRetroBasis.RetroTimePayrollScheduleTransaction = retroTimePayrollScheduleTransaction;
                                                }

                                                #endregion

                                                #region Accounting

                                                if (!timePayrollScheduleTransaction.AccountInternal.IsLoaded)
                                                    timePayrollScheduleTransaction.AccountInternal.Load();

                                                foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
                                                {
                                                    RetroactivePayrollAccount retroAccount = retro.Payroll.RetroactivePayrollAccount?.FirstOrDefault(i => i.AccountDimId == accountDim.AccountDimId);

                                                    if (accountDim.IsStandard)
                                                    {
                                                        if (retroAccount != null && retroAccount.Type == (int)TermGroup_RetroactivePayrollAccountType.OtherAccount && retroAccount.AccountStdId.HasValue)
                                                        {
                                                            //AccountStd other
                                                            AccountStd accountStd = GetAccountStd(retroAccount.AccountStdId.Value);
                                                            if (accountStd != null)
                                                                retroTimePayrollScheduleTransaction.AccountStdId = accountStd.AccountId;
                                                        }
                                                        else
                                                        {
                                                            //AccountStd according to transaction
                                                            retroTimePayrollScheduleTransaction.AccountStdId = timePayrollScheduleTransaction.AccountStdId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (retroAccount != null && retroAccount.Type == (int)TermGroup_RetroactivePayrollAccountType.OtherAccount && retroAccount.AccountInternalId.HasValue)
                                                        {
                                                            //AccountInternal other
                                                            AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(retroAccount.AccountInternalId.Value);
                                                            if (accountInternal != null)
                                                                retroTimePayrollScheduleTransaction.AccountInternal.Add(accountInternal);
                                                        }
                                                        else
                                                        {
                                                            //AccountInternal according to transaction
                                                            AccountInternal accountInternal = timePayrollScheduleTransaction.AccountInternal.FirstOrDefault(i => i.Account != null && i.Account.AccountDimId == accountDim.AccountDimId);
                                                            if (accountInternal != null)
                                                                retroTimePayrollScheduleTransaction.AccountInternal.Add(accountInternal);
                                                        }
                                                    }
                                                }

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion

                                    #endregion
                                }

                                //Save once per outcome becuase of performance (save per employee seems to take too long time)
                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    break;
                            }

                            #endregion
                            
                            ActivateWarningPayrollPeriodHasChanged(retroEmployee.EmployeeId, timePeriod.TimePeriodId);

                            #region Employee status

                            if (oDTO.Result.Success)
                            {
                                retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Payroll;

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    break;

                                if (!TryCommit(oDTO))
                                    break;

                                updateStatus = true;
                            }
                            else
                            {
                                break;
                            }

                            #endregion

                            #endregion
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {

                    entities.Connection.Close();
                }
            }

            #region Retro status

            if (updateStatus)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    try
                    {
                        InitContext(taskEntities);

                        ActionResult result = UpdateRetroactivePayrollStatus(iDTO.RetroactivePayrollInput.RetroactivePayrollId);
                        if (!result.Success)
                            return oDTO;
                    }
                    catch (Exception ex)
                    {
                        oDTO.Result.Exception = ex;
                        LogError(ex);
                    }
                    finally
                    {

                        entities.Connection.Close();
                    }
                }
            }

            #endregion

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DeleteRetroactivePayrollTransactionsOutputDTO TaskDeleteRetroactivePayrollTransactions()
        {
            var (iDTO, oDTO) = InitTask<DeleteRetroactivePayrollTransactionsInputDTO, DeleteRetroactivePayrollTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.RetroactivePayrollInput == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            bool updateStatus = false;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    var retro = GetRetro(iDTO.RetroactivePayrollInput.RetroactivePayrollId, iDTO.FilterEmployeeIds);
                    if (!retro.Result.Success)
                    {
                        oDTO.Result = retro.Result;
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    foreach (RetroactivePayrollEmployee retroEmployee in retro.Employees.Where(rpe => rpe.IsValidToDeleteTransactions()))
                    {
                        List<RetroactivePayrollOutcome> retroOutcomes = GetRetroactivePayrollOutcomes(retroEmployee);
                        if (retroOutcomes.IsNullOrEmpty())
                            continue;

                        LoadRetroactivePayrollTransactions(retroOutcomes, out _);

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            oDTO.Result = SetRetroPayrollEmployeeToDeleted(retroEmployee, retroOutcomes: retroOutcomes, doDeleteTransactions: true, retroEmployeeStatusAfter: TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated);
                            if (!oDTO.Result.Success)
                                break;

                            ActivateWarningPayrollPeriodHasChanged(retroEmployee.EmployeeId, retro.Payroll.TimePeriodId);

                            oDTO.Result = Save();
                            if (!TryCommit(oDTO))
                                break;

                            updateStatus = true;
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {                    
                    entities.Connection.Close();
                }
            }

            #region Retro status

            if (updateStatus)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    try
                    {
                        InitContext(taskEntities);

                        ActionResult result = UpdateRetroactivePayrollStatus(iDTO.RetroactivePayrollInput.RetroactivePayrollId);
                        if (!result.Success)
                            return oDTO;
                    }
                    catch (Exception ex)
                    {
                        oDTO.Result.Exception = ex;
                        LogError(ex);
                    }
                    finally
                    {

                        entities.Connection.Close();
                    }
                }
            }

            #endregion

            return oDTO;
        }

        #endregion

        #region Retro

        private RetroContainer GetRetro(int retroactivePayrollId, List<int> filterEmployeeIds = null, bool loadRetroAccounting = false)
        {
            RetroactivePayroll retroPayroll = GetRetroactivePayroll(retroactivePayrollId, loadRetroAccounting: loadRetroAccounting);
            List<RetroactivePayrollEmployee> retroEmployees = retroPayroll != null ? GetRetroactivePayrollEmployees(retroactivePayrollId, filterEmployeeIds) : null;
            
            ActionResult result;
            if (retroPayroll == null)
                result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(110445, "Retrokörning hittades inte"));
            else if (retroPayroll.RetroactivePayrollEmployee.IsNullOrEmpty())
                result = new ActionResult((int)ActionResultSave.RetroactivePayrollNotFound, GetText(11038, "Retroaktiv lön innehåller inga anställda"));
            else
                result = new ActionResult(true);

            return RetroContainer.Create(result, retroPayroll, retroEmployees);
        }

        #endregion

        #region RetroactivePayroll

        private List<RetroactivePayroll> GetOverlappingRetroactivePayrollsWithEmployee(RetroactivePayroll retroPayroll)
        {
            if (retroPayroll == null)
                return new List<RetroactivePayroll>();

            List<RetroactivePayroll> existingRetroPayrolls = (from rp in entities.RetroactivePayroll
                                                                .Include("RetroactivePayrollEmployee")
                                                              where rp.ActorCompanyId == actorCompanyId &&
                                                              rp.RetroactivePayrollId != retroPayroll.RetroactivePayrollId &&
                                                              rp.State == (int)SoeEntityState.Active
                                                              select rp).ToList();

            List<RetroactivePayroll> retroPayrollsInInterval = new List<RetroactivePayroll>();
            foreach (RetroactivePayroll existingRetroPayroll in existingRetroPayrolls)
            {
                if (CalendarUtility.IsDatesOverlappingNullable(existingRetroPayroll.DateFrom, existingRetroPayroll.DateTo, retroPayroll.DateFrom, retroPayroll.DateTo))
                    retroPayrollsInInterval.Add(existingRetroPayroll);
            }
            return retroPayrollsInInterval;
        }

        private RetroactivePayroll GetRetroactivePayroll(int retroactivePayrollId, bool loadRetroAccounting = false, bool loadRetroEmployee = false, bool loadRetroOutcomes = false)
        {
            IQueryable<RetroactivePayroll> query = entities.RetroactivePayroll;

            if (loadRetroAccounting)
                query = query.Include("RetroactivePayrollAccount");

            if (loadRetroOutcomes)
                query = query.Include("RetroactivePayrollEmployee.RetroactivePayrollOutcome");
            else if (loadRetroEmployee)
                query = query.Include("RetroactivePayrollEmployee");

            return (from rp in query
                    where rp.RetroactivePayrollId == retroactivePayrollId &&
                    rp.ActorCompanyId == actorCompanyId &&
                    rp.State == (int)SoeEntityState.Active
                    select rp).FirstOrDefault();
        }

        private void LoadRetroactivePayrollTransactions(RetroactivePayroll retroPayroll, out bool hasTransactions, bool doAbortIfHasTransactions = false)
        {
            if (!retroPayroll.RetroactivePayrollEmployee.IsLoaded)
                retroPayroll.RetroactivePayrollEmployee.Load();

            LoadRetroactivePayrollTransactions(retroPayroll.RetroactivePayrollEmployee, out hasTransactions, doAbortIfHasTransactions);
        }

        private void LoadRetroactivePayrollTransactions(IEnumerable<RetroactivePayrollEmployee> retroEmployees, out bool hasTransactions, bool doAbortIfHasTransactions = false)
        {
            hasTransactions = false;

            foreach (RetroactivePayrollEmployee retroEmployee in retroEmployees.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (!retroEmployee.RetroactivePayrollOutcome.IsLoaded)
                    retroEmployee.RetroactivePayrollOutcome.Load();

                LoadRetroactivePayrollTransactions(retroEmployee.RetroactivePayrollOutcome, out bool hasEmployeeTransactions, doAbortIfHasTransactions);
                if (!hasTransactions && hasEmployeeTransactions)
                    hasTransactions = true;
                if (hasTransactions && doAbortIfHasTransactions)
                    return;                    
            }
        }

        private void LoadRetroactivePayrollTransactions(IEnumerable<RetroactivePayrollOutcome> retroOutcomes, out bool hasTransactions, bool doAbortIfHasTransactions = false)
        {
            hasTransactions = false;

            foreach (RetroactivePayrollOutcome retroOutcome in retroOutcomes.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (!retroOutcome.TimePayrollTransaction.IsLoaded)
                    retroOutcome.TimePayrollTransaction.Load();
                if (!retroOutcome.TimePayrollScheduleTransaction.IsLoaded)
                    retroOutcome.TimePayrollScheduleTransaction.Load();

                if (!retroOutcome.RetroactivePayrollBasis.IsLoaded)
                    retroOutcome.RetroactivePayrollBasis.Load();
                foreach (RetroactivePayrollBasis retroBasis in retroOutcome.RetroactivePayrollBasis.Where(i => i.State == (int)SoeEntityState.Active))
                {
                    LoadRetroactivePayrollTransactions(retroBasis);
                    if (!hasTransactions && retroBasis.RetroTimePayrollTransaction?.State == (int)SoeEntityState.Active)
                        hasTransactions = true;
                    if (hasTransactions && doAbortIfHasTransactions)
                        return;
                }

                if (!retroOutcome.RetroactivePayrollScheduleBasis.IsLoaded)
                    retroOutcome.RetroactivePayrollScheduleBasis.Load();
                foreach (RetroactivePayrollScheduleBasis retroBasis in retroOutcome.RetroactivePayrollScheduleBasis.Where(i => i.State == (int)SoeEntityState.Active))
                {
                    LoadRetroactivePayrollTransactions(retroBasis);
                }
            }
        }

        private void LoadRetroactivePayrollTransactions(RetroactivePayrollBasis retroBasis)
        {
            if (retroBasis == null)
                return;
            if (!retroBasis.RetroTimePayrollTransactionReference.IsLoaded)
                retroBasis.RetroTimePayrollTransactionReference.Load();
            if (!retroBasis.BasisTimePayrollTransactionReference.IsLoaded)
                retroBasis.BasisTimePayrollTransactionReference.Load();
        }

        private void LoadRetroactivePayrollTransactions(RetroactivePayrollScheduleBasis retroBasis)
        {
            if (retroBasis == null)
                return;
            if (!retroBasis.RetroTimePayrollScheduleTransactionReference.IsLoaded)
                retroBasis.RetroTimePayrollScheduleTransactionReference.Load();
            if (!retroBasis.BasisTimePayrollScheduleTransactionReference.IsLoaded)
                retroBasis.BasisTimePayrollScheduleTransactionReference.Load();
        }

        private bool IsEmployeeInRetroAndOpenedPeriod(List<RetroactivePayroll> retroPayrolls, List<EmployeeTimePeriod> employeePeriods, int employeeId)
        {
            List<EmployeeTimePeriod> lockedOrPaidEmployeePeriods = employeePeriods.Where(etp => etp.Status == (int)SoeEmployeeTimePeriodStatus.Locked || etp.Status == (int)SoeEmployeeTimePeriodStatus.Paid).ToList();

            foreach (RetroactivePayroll retroPayroll in retroPayrolls)
            {
                if (!retroPayroll.RetroactivePayrollEmployee.Where(x => x.State == (int)SoeEntityState.Active).Any(i => i.EmployeeId == employeeId))
                    continue;

                EmployeeTimePeriod employeeTimePeriod = lockedOrPaidEmployeePeriods.FirstOrDefault(i => i.TimePeriodId == retroPayroll.TimePeriodId);
                if (employeeTimePeriod != null)
                    continue;

                return true;
            }

            return false;
        }

        private ActionResult UpdateRetroactivePayrollStatus(int retroActivePayrollId)
        {
            RetroactivePayroll retroPayroll = GetRetroactivePayroll(retroActivePayrollId, loadRetroEmployee: true);
            if (retroPayroll == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(110445, "Retrokörning hittades inte"));

            List<RetroactivePayrollEmployee> updatedEmployees = retroPayroll.RetroactivePayrollEmployee.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            retroPayroll.Status = (int)updatedEmployees.GetRetroactivePayrollStatus();
            return Save();

        }

        #endregion

        #region RetroactivePayrollEmployee

        private List<RetroactivePayrollEmployee> GetRetroactivePayrollEmployees(int retroactivePayrollId, List<int> employeeIds = null)
        {
            var query = from rpe in entities.RetroactivePayrollEmployee
                        where rpe.RetroactivePayrollId == retroactivePayrollId &&                      
                        rpe.ActorCompanyId == actorCompanyId &&
                        rpe.State == (int)SoeEntityState.Active
                        select rpe;

            if (!employeeIds.IsNullOrEmpty())
                query = query.Where(rpe => employeeIds.Contains(rpe.EmployeeId));

            return query.ToList();
        }

        private RetroactivePayrollEmployee GetRetroactivePayrollEmployee(int employeeId, int timePeriodId)
        {
            return (from rpe in entities.RetroactivePayrollEmployee
                    where rpe.ActorCompanyId == actorCompanyId &&
                    rpe.EmployeeId == employeeId &&
                    rpe.State == (int)SoeEntityState.Active &&
                    rpe.RetroactivePayroll.TimePeriodId == timePeriodId &&
                    rpe.RetroactivePayroll.State == (int)SoeEntityState.Active
                    select rpe).FirstOrDefault();
        }

        private ActionResult SetRetroactivePayrollEmployeeLocked(int employeeId, int timePeriodId)
        {
            ActionResult result = new ActionResult(true);

            RetroactivePayrollEmployee retroEmployee = GetRetroactivePayrollEmployee(employeeId, timePeriodId);
            if (retroEmployee != null && retroEmployee.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Payroll)
            {
                RetroactivePayroll retroPayroll = GetRetroactivePayroll(retroEmployee.RetroactivePayrollId, loadRetroEmployee: true);
                if (retroPayroll == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(110445, "Retrokörning hittades inte"));

                retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked;
                retroPayroll.Status = (int)retroPayroll.GetRetroactivePayrollStatus();
            }

            return result;
        }

        private ActionResult SetRetroactivePayrollEmployeeUnLocked(int employeeId, int timePeriodId)
        {
            ActionResult result = new ActionResult(true);

            RetroactivePayrollEmployee retroEmployee = GetRetroactivePayrollEmployee(employeeId, timePeriodId);
            if (retroEmployee != null && retroEmployee.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked)
            {
                RetroactivePayroll retroPayroll = GetRetroactivePayroll(retroEmployee.RetroactivePayrollId, loadRetroEmployee: true);
                if (retroPayroll == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(110445, "Retrokörning hittades inte"));

                retroEmployee.Status = (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Payroll;
                retroPayroll.Status = (int)retroPayroll.GetRetroactivePayrollStatus();
            }

            return result;
        }

        #endregion

        #region RetroactivePayrollOutcome

        private List<RetroactivePayrollOutcome> GetRetroactivePayrollOutcomesWithProduct(int retroactivePayrollId, int employeeId)
        {
            return (from rpo in entities.RetroactivePayrollOutcome
                        .Include("PayrollProduct")
                    where rpo.ActorCompanyId == actorCompanyId &&
                    rpo.RetroactivePayrollEmployee.RetroactivePayrollId == retroactivePayrollId &&
                    rpo.EmployeeId == employeeId &&
                    rpo.State == (int)SoeEntityState.Active
                    select rpo).ToList();
        }

        private List<RetroactivePayrollOutcome> GetRetroactivePayrollOutcomes(RetroactivePayrollEmployee retroEmployee)
        {
            return GetRetroactivePayrollOutcomesQuery(retroEmployee.RetroactivePayrollEmployeeId).ToList();
        }

        private List<RetroactivePayrollOutcome> GetRetroactivePayrollOutcomes(RetroactivePayrollEmployee retroEmployee, out bool hasAnyOutcomeTransactions)
        {
            var retroactivePayrollOutcomes = (from rpo in GetRetroactivePayrollOutcomesQuery(retroEmployee.RetroactivePayrollEmployeeId)
                                              select new
                                              {
                                                  RetroactivePayrollOutcome = rpo,
                                                  HasTransactions = rpo.TimePayrollTransaction.Any(t => t.State == (int)SoeEntityState.Active),
                                              }).ToList();

            hasAnyOutcomeTransactions = retroactivePayrollOutcomes.Any(r => r.HasTransactions);
            return retroactivePayrollOutcomes.Select(rpo => rpo.RetroactivePayrollOutcome).ToList();
        }

        private IQueryable<RetroactivePayrollOutcome> GetRetroactivePayrollOutcomesQuery(int retroactiveEmployeeId) => entities.RetroactivePayrollOutcome.Where(rpo => rpo.RetroactivePayrolIEmployeeId == retroactiveEmployeeId && rpo.State == (int)SoeEntityState.Active);

        #endregion

        #region RetroactivePayrollBasis

        private List<RetroactivePayrollBasis> GetRetroactivePayrollBasisWithTransactionBasis(int retroactivePayrollOutcomeId)
        {
            return (from t in entities.RetroactivePayrollBasis
                        .Include("BasisTimePayrollTransaction")                        
                    where t.RetroactivePayrollOutcomeId == retroactivePayrollOutcomeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<RetroactivePayrollBasis> GetRetroactivePayrollBasisWithTransactionBasisAndAccounting(int retroactivePayrollOutcomeId)
        {
            return (from t in entities.RetroactivePayrollBasis
                        .Include("BasisTimePayrollTransaction.TimePayrollTransactionExtended")
                        .Include("BasisTimePayrollTransaction.AccountInternal.Account")
                    where t.RetroactivePayrollOutcomeId == retroactivePayrollOutcomeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        #endregion

        #region RetroactivePayrollBasis

        private List<RetroactivePayrollScheduleBasis> GetRetroactivePayrollScheduleBasisWithTransactionBasisAndAccounting(int retroactivePayrollOutcomeId)
        {
            return (from t in entities.RetroactivePayrollScheduleBasis
                        .Include("BasisTimePayrollScheduleTransaction")
                        .Include("BasisTimePayrollScheduleTransaction.AccountInternal.Account")
                    where t.RetroactivePayrollOutcomeId == retroactivePayrollOutcomeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        #endregion
    }

    internal class RetroContainer
    {
        public ActionResult Result { get; private set; }
        public RetroactivePayroll Payroll { get; private set; }
        public List<RetroactivePayrollEmployee> Employees { get; private set; }

        public DateTime DateFrom => this.Payroll?.DateFrom ?? CalendarUtility.DATETIME_DEFAULT;
        public DateTime? DateTo => this.Payroll?.DateTo;

        private RetroContainer() { }
        public static RetroContainer Create(ActionResult result, RetroactivePayroll retroPayroll, List<RetroactivePayrollEmployee> retroEmployee)
        {
            return new RetroContainer
            {
                Result = result,
                Payroll = retroPayroll,
                Employees = retroEmployee
            };
        }
    }
}
