using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Save PayrollStartValues
        /// </summary>
        /// <returns></returns>
        private SavePayrollStartValuesOutputDTO TaskSavePayrollStartValues()
        {
            var (iDTO, oDTO) = InitTask<SavePayrollStartValuesInputDTO, SavePayrollStartValuesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.StartValueRows == null)
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

                        #region Perform

                        PayrollStartValueHead head = GetPayrollStartValueHead(iDTO.PayrollStartValueHeadId);
                        if (head == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "PayrollStartValueHead");
                            return oDTO;
                        }

                        List<PayrollStartValueRow> rows = GetPayrollStartValueRowsWithTransaction(iDTO.PayrollStartValueHeadId);

                        foreach (var inputRowsByEmployee in iDTO.StartValueRows.GroupBy(p => p.EmployeeId))
                        {
                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(inputRowsByEmployee.Key, onlyActive: false);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format("Employee {0}", inputRowsByEmployee.Key));
                                return oDTO;
                            }

                            foreach (PayrollStartValueRowDTO inputRow in inputRowsByEmployee)
                            {
                                if (inputRow.Date < head.DateFrom || inputRow.Date > head.DateTo)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollStartValueRowDateIsOutsideHeadDateRange, GetText(91942, "Datumet {0} ligger utanför importens datumintervall"));
                                    return oDTO;
                                }

                                PayrollStartValueRow row = null;
                                if (inputRow.PayrollStartValueRowId == 0)
                                    oDTO.Result = CreatePayrollStartValueRow(employee, inputRow, out row);
                                else
                                    oDTO.Result = UpdatePayrollStartValueRow(employee, inputRow, rows.FirstOrDefault(x => x.PayrollStartValueRowId == inputRow.PayrollStartValueRowId), head);
                                
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        TryCommit(oDTO);

                        #endregion
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
        /// Save transactions for PayrollStartValues
        /// </summary>
        /// <returns></returns>
        private SaveTransactionsForPayrollStartValuesOutputDTO TaskSaveTransactionsForPayrollStartValues()
        {
            var (iDTO, oDTO) = InitTask<SaveTransactionsForPayrollStartValuesInputDTO, SaveTransactionsForPayrollStartValuesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.PayrollStartValueHeadId == 0)
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

                        Employee employee = null;
                        if (iDTO.EmployeeId.HasValue)
                        {
                            employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId.Value);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                                return oDTO;
                            }
                        }

                        List<PayrollStartValueRow> rows = GetPayrollStartValueRowsWithTransaction(iDTO.PayrollStartValueHeadId, employee?.EmployeeId);
                        if (!rows.IsNullOrEmpty())
                        {
                            if (rows.HasTransactions())
                            {
                                if (employee == null)
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollStartValueTransactionsAlreadyCreated, GetText(10068, "Det finns redan transaktioner skapade för importen"));
                                else
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollStartValueTransactionsAlreadyCreated, String.Format(GetText(10069, "Det finns redan transaktioner skapade för anställd {0}"), employee.EmployeeNrAndName));

                                return oDTO;
                            }

                            foreach (var rowsByEmployee in rows.GroupBy(x => x.EmployeeId))
                            {
                                if (!iDTO.EmployeeId.HasValue)
                                    employee = GetEmployeeWithContactPersonAndEmploymentFromCache(rowsByEmployee.Key, onlyActive: false);
                                if (employee == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format("Employee {0}", rowsByEmployee.Key));
                                    return oDTO;
                                }

                                foreach (var payrollStartValueRow in rowsByEmployee)
                                {
                                    oDTO.Result = CreateTimePayrollTransactionsForPayrollStartValueRow(payrollStartValueRow, employee);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        /// <summary>
        /// Delete transactions for PayrollStartValues
        /// </summary>
        /// <returns></returns>
        private DeleteTransactionsForPayrollStartValuesOutputDTO TaskDeleteTransactionsForPayrollStartValues()
        {
            var (iDTO, oDTO) = InitTask<DeleteTransactionsForPayrollStartValuesInputDTO, DeleteTransactionsForPayrollStartValuesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.PayrollStartValueHeadId == 0)
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

                        Employee employee = null;
                        if (iDTO.EmployeeId.HasValue)
                        {
                            employee = GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId.Value);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                                return oDTO;
                            }
                        }

                        List<PayrollStartValueRow> rows = GetPayrollStartValueRowsWithTransaction(iDTO.PayrollStartValueHeadId, employee?.EmployeeId);
                        if (!rows.IsNullOrEmpty())
                        {
                            if (!rows.HasTransactions())
                            {
                                if (employee == null)
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollStartValueTransactionsAlreadyCreated, GetText(10070, "Det finns inga transaktioner skapade för importen"));
                                else
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollStartValueTransactionsAlreadyCreated, String.Format(GetText(10071, "Det finns inga transaktioner skapade för anställd {0}"), employee.EmployeeNrAndName));

                                return oDTO;
                            }

                            foreach (var rowsByEmployee in rows.GroupBy(t => t.EmployeeId))
                            {
                                oDTO.Result = SetTimePayrollTransactionsForPayrollStartValuesToDeleted(rowsByEmployee.Select(x => x.PayrollStartValueRowId).ToList(), saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        /// <summary>
        /// Delete PayrollStartValues
        /// </summary>
        /// <returns></returns>
        private DeletePayrollStartValueHeadOutputDTO TaskDeletePayrollStartValueHead()
        {
            var (iDTO, oDTO) = InitTask<DeletePayrollStartValueHeadInputDTO, DeletePayrollStartValueHeadOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.PayrollStartValueHeadId == 0)
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

                        PayrollStartValueHead head = GetPayrollStartValueHeadWithRows(iDTO.PayrollStartValueHeadId);
                        if (head == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "PayrollStartValueHead");
                            return oDTO;
                        }

                        oDTO.Result = ChangeEntityState(head, SoeEntityState.Deleted);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        List<PayrollStartValueRow> rows = head.PayrollStartValueRow.Where(r => r.State == (int)SoeEntityState.Active).ToList();
                        if (!rows.IsNullOrEmpty())
                        {
                            foreach (var rowsByEmployee in rows.GroupBy(x => x.EmployeeId))
                            {
                                foreach (PayrollStartValueRow row in rows)
                                {
                                    oDTO.Result = ChangeEntityState(row, SoeEntityState.Deleted);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }

                                oDTO.Result = SetTimePayrollTransactionsForPayrollStartValuesToDeleted(rows.Select(x => x.PayrollStartValueRowId).ToList(), saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        #endregion

        #region PayrollStartValueHead

        private List<PayrollStartValueHead> GetPayrollStartValueHeads()
        {
            return (from e in entities.PayrollStartValueHead
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        private PayrollStartValueHead GetPayrollStartValueHead(int payrollStartValueHeadId)
        {
            return (from e in entities.PayrollStartValueHead
                    where e.ActorCompanyId == actorCompanyId &&
                    e.PayrollStartValueHeadId == payrollStartValueHeadId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        private PayrollStartValueHead GetPayrollStartValueHeadWithRows(int payrollStartValueHeadId)
        {
            return (from e in entities.PayrollStartValueHead
                    .Include("PayrollStartValueRow")
                    where e.ActorCompanyId == actorCompanyId &&
                    e.PayrollStartValueHeadId == payrollStartValueHeadId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        #endregion

        #region PayrollStartValueRow

        private List<PayrollStartValueRow> GetPayrollStartValueRowsWithTransaction(int payrollStartValueHeadId, int? employeeId = null)
        {
            return (from e in entities.PayrollStartValueRow
                        .Include("TimePayrollTransaction")
                    where e.ActorCompanyId == actorCompanyId &&
                    (!employeeId.HasValue || e.EmployeeId == employeeId.Value) &&
                    e.PayrollStartValueHeadId == payrollStartValueHeadId &&
                    e.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        private List<PayrollStartValueRow> GetPayrollStartValueRowsForEmployee(int employeeId, DateTime date)
        {
            return (from p in entities.PayrollStartValueRow
                    where p.ActorCompanyId == actorCompanyId &&
                    p.EmployeeId == employeeId &&
                    p.Date == date.Date &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        private List<PayrollStartValueRow> GetPayrollStartValueRowsForEmployee(int employeeId, DateTime startDate, DateTime stopDate)
        {
            return (from p in entities.PayrollStartValueRow
                    where p.ActorCompanyId == actorCompanyId &&
                    p.EmployeeId == employeeId &&
                    p.Date >= startDate.Date &&
                    p.Date <= stopDate.Date &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        private ActionResult CreatePayrollStartValueRow(Employee employee, PayrollStartValueRowDTO inputRow, out PayrollStartValueRow row)
        {
            row = null;
            if (employee == null || inputRow == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            row = new PayrollStartValueRow()
            {
                PayrollStartValueHeadId = inputRow.PayrollStartValueHeadId,
                ActorCompanyId = inputRow.ActorCompanyId,
                EmployeeId = inputRow.EmployeeId,
            };
            SetCreatedProperties(row);
            entities.PayrollStartValueRow.AddObject(row);
            SetPayrollStartValueRowValues(inputRow, row);

            return inputRow.DoCreateTransaction ? CreateTimePayrollTransactionsForPayrollStartValueRow(row, employee) : new ActionResult(true);
        }

        private ActionResult UpdatePayrollStartValueRow(Employee employee, PayrollStartValueRowDTO inputRow, PayrollStartValueRow row, PayrollStartValueHead head)
        {
            if (employee == null || inputRow == null || row == null || head == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            TimePayrollTransaction timePayrollTransaction = row.TimePayrollTransaction?.FirstOrDefault(t => t.State == (int)SoeEntityState.Active);
            if (timePayrollTransaction != null)
            {
                if (!timePayrollTransaction.TimeBlockDateReference.IsLoaded)
                    timePayrollTransaction.TimeBlockDateReference.Load();

                bool isModified = false;
                if (timePayrollTransaction.Quantity != inputRow.Quantity)
                {
                    timePayrollTransaction.Quantity = inputRow.Quantity;
                    isModified = true;
                }
                if (timePayrollTransaction.Amount != inputRow.Amount)
                {
                    timePayrollTransaction.Amount = inputRow.Amount;
                    isModified = true;
                }
                if (timePayrollTransaction.Date.Date != inputRow.Date.Date)
                {
                    timePayrollTransaction.TimeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, inputRow.Date, createIfNotExists: true);
                    isModified = true;
                }
                if (isModified)
                    SetModifiedProperties(timePayrollTransaction);
            }

            SetPayrollStartValueRowValues(inputRow, row);

            return new ActionResult(true);
        }

        private ActionResult CreateTimePayrollTransactionsForPayrollStartValueRow(PayrollStartValueRow row, Employee employee)
        {
            if (row == null || employee == null)
                return new ActionResult((int)ActionResultSave.InsufficientInput);
            if (row.TimePayrollTransaction.Any(x => x.State == (int)SoeEntityState.Active))
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8628, "Det finns redan transaktioner kopplade till aktuella startvärden, transaktionerna måste tas bort innan de kan skapas om."));

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10046, "Attestnivå med startnivå hittades inte"));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(row.EmployeeId, row.Date, true);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(row.ProductId);
            if (payrollProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format("PayrollProduct {0}", row.ProductId));

            TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, timeBlockDate, row.Quantity, row.Amount, 0, row.Amount, "", attestStateInitialPayroll.AttestStateId, null, employee.EmployeeId);
            if (timePayrollTransaction == null)
                return new ActionResult((int)ActionResultSave.PayrollStartValueCreateTransactionsFailed, string.Format(GetText(8629, "Kunde inte skapa transaktion för anställd med anställningsnummer {0}. Löneart: {1}, datum: {2} "), employee.EmployeeNr, payrollProduct.Number, timeBlockDate.Date.ToShortDateString()));

            if (row.PayrollStartValueRowId > 0)
                timePayrollTransaction.PayrollStartValueRowId = row.PayrollStartValueRowId;
            else
                timePayrollTransaction.PayrollStartValueRow = row;

            ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, timeBlockDate.Date, payrollProduct);

            return new ActionResult(true);
        }

        private void SetPayrollStartValueRowValues(PayrollStartValueRowDTO inputRow, PayrollStartValueRow row)
        {
            if (inputRow == null || row == null)
                return;

            row.ProductId = inputRow.ProductId;
            row.SysPayrollStartValueId = inputRow.SysPayrollStartValueId;
            row.Quantity = inputRow.Quantity;
            row.Amount = inputRow.Amount;
            row.Date = inputRow.Date;
            row.SysPayrollTypeLevel1 = (int)inputRow.SysPayrollTypeLevel1;
            row.SysPayrollTypeLevel2 = (int)inputRow.SysPayrollTypeLevel2;
            row.SysPayrollTypeLevel3 = (int)inputRow.SysPayrollTypeLevel3;
            row.SysPayrollTypeLevel4 = (int)inputRow.SysPayrollTypeLevel4;
            row.ScheduleTimeMinutes = inputRow.ScheduleTimeMinutes;
            row.AbsenceTimeMinutes = inputRow.AbsenceTimeMinutes;
            row.State = (int)inputRow.State;
            if (row.PayrollStartValueRowId > 0)
                SetModifiedProperties(row);
        }

        #endregion
    }
}
