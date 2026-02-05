using AngleSharp.Dom;
using log4net.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected int actorCompanyId;
        protected int userId;
        private TimeEngineTemplateRepository templateRepository;
        private TimeEngineInputDTO _iDTO;
        private CompEntities entities;
        private readonly bool forcedExternalEntities;
        private TransactionScope currentTransaction;

        #endregion

        protected TimeEngineOutputDTO PerformTask(SoeTimeEngineTask task)
        {
            InitBeforeRunningTask(task);

            try
            {
                switch (task)
                {
                    #region Accounting

                    case SoeTimeEngineTask.RecalculateAccounting:
                        return this.TaskRecalculateAccounting();
                    case SoeTimeEngineTask.RecalculateAccountingFromPayroll:
                        return this.TaskRecalculateAccountingFromPayroll();
                    case SoeTimeEngineTask.SaveAccountProvisionBase:
                        return this.TaskSaveAccountProvisionBase();
                    case SoeTimeEngineTask.LockAccountProvisionBase:
                        return this.TaskLockAccountProvisionBase();
                    case SoeTimeEngineTask.UnLockAccountProvisionBase:
                        return this.TaskUnLockAccountProvisionBase();
                    case SoeTimeEngineTask.UpdateAccountProvisionTransactions:
                        return this.TaskUpdateAccountProvisionTransactions();

                    #endregion

                    #region Attest

                    case SoeTimeEngineTask.SaveAttestForEmployee:
                        return this.TaskSaveAttestForEmployee();
                    case SoeTimeEngineTask.SaveAttestForEmployees:
                        return this.TaskSaveAttestForEmployees();
                    case SoeTimeEngineTask.SaveAttestForTransactions:
                        return this.TaskSaveAttestForTransactions();
                    case SoeTimeEngineTask.SaveAttestForAccountProvision:
                        return this.TaskSaveAttestForAccountProvision();
                    case SoeTimeEngineTask.RunAutoAttest:
                        return this.TaskRunAutoAttest();
                    case SoeTimeEngineTask.SendAttestReminder:
                        return this.TaskSendAttestReminder();

                    #endregion

                    #region Calendar

                    case SoeTimeEngineTask.ImportHolidays:
                        return this.TaskImportHolidays();
                    case SoeTimeEngineTask.CalculateDayTypeForEmployee:
                        return this.TaskCalculateDayTypeForEmployee();
                    case SoeTimeEngineTask.CalculateDayTypesForEmployee:
                        return this.TaskCalculateDayTypesForEmployee();
                    case SoeTimeEngineTask.CalculateDayTypeForEmployees:
                        return this.TaskCalculateDayTypeForEmployees();
                    case SoeTimeEngineTask.SaveUniqueday:
                        return this.TaskSaveUniqueDay();
                    case SoeTimeEngineTask.SaveUniqueDayFromHalfDay:
                        return this.TaskSaveUniqueDayFromHalfDay();
                    case SoeTimeEngineTask.UpdateUniqueDayFromHalfDay:
                        return this.TaskUpdateUniqueDayFromHalfDay();
                    case SoeTimeEngineTask.AddUniqueDayFromHoliday:
                        return this.TaskAddUniqueDayFromHoliday();
                    case SoeTimeEngineTask.UpdateUniqueDayFromHoliday:
                        return this.TaskUpdateUniqueDayFromHoliday();
                    case SoeTimeEngineTask.DeleteUniqueDayFromHoliday:
                        return this.TaskDeleteUniqueDayFromHoliday();
                    case SoeTimeEngineTask.CreateTransactionsForEarnedHoliday:
                        return this.TaskCreateTransactionsForEarnedHoliday();
                    case SoeTimeEngineTask.DeleteTransactionsForEarnedHoliday:
                        return this.TaskDeleteTransactionsForEarnedHoliday();

                    #endregion

                    #region Deviations

                    case SoeTimeEngineTask.GenerateDeviationsFromTimeInterval:
                        return this.TaskGenerateDeviationsFromTimeInterval();
                    case SoeTimeEngineTask.SaveGeneratedDeviations:
                        return this.TaskSaveGeneratedDeviations();
                    case SoeTimeEngineTask.SaveWholedayDeviations:
                        return this.TaskSaveWholedayDeviations();
                    case SoeTimeEngineTask.ValidateDeviationChange:
                        return this.TaskValidateDeviationChange();                    
                    case SoeTimeEngineTask.RecalculateUnhandledShiftChanges:
                        return this.TaskRecalculateUnhandledShiftChanges();
                    case SoeTimeEngineTask.GetDayOfAbsenceNumber:
                        return this.TaskGetDayOfAbsenceNumber();
                    case SoeTimeEngineTask.CreateAbsenceDetails:
                        return this.TaskCreateAbsenceDetails();
                    case SoeTimeEngineTask.SaveAbsenceDetailsRatio:
                        return this.TaskSaveAbsenceDetailsRatio();
                    case SoeTimeEngineTask.GetDeviationsAfterEmployment:
                        return this.TaskGetDeviationsAfterEmployment();
                    case SoeTimeEngineTask.DeleteDeviationsDaysAfterEmployment:
                        return this.TaskDeleteDeviationsDaysAfterEmployment();

                    #endregion

                    #region Expense

                    case SoeTimeEngineTask.SaveExpenseValidation:
                        return this.TaskSaveExpenseValidation();
                    case SoeTimeEngineTask.SaveExpense:
                        return this.TaskSaveExpense();
                    case SoeTimeEngineTask.DeleteExpense:
                        return this.TaskDeleteExpense();

                    #endregion

                    #region Mobile

                    case SoeTimeEngineTask.MobileModifyBreak:
                        return this.TaskMobileModifyBreak();
                    case SoeTimeEngineTask.AddModifyTimeBlocks:
                        return this.TaskAddModifyTimeBlocks();

                    #endregion

                    #region PayrollEnd

                    case SoeTimeEngineTask.SaveVacationYearEnd:
                        return this.TaskSaveVacationYearEnd();
                    case SoeTimeEngineTask.DeleteVacationYearEnd:
                        return this.TaskDeleteVacationYearEnd();
                    case SoeTimeEngineTask.CreateFinalSalary:
                        return this.TaskCreateFinalSalary();
                    case SoeTimeEngineTask.DeleteFinalSalary:
                        return this.TaskDeleteFinalSalary();
                    case SoeTimeEngineTask.ValidateVacationYearEnd:
                        return this.TaskValidateVacationYearEnd();

                    #endregion

                    #region PayrollImport

                    case SoeTimeEngineTask.PayrollImport:
                        return this.TaskPayrollImport();
                    case SoeTimeEngineTask.RollbackPayrollImport:
                        return this.TaskRollbackPayrollImport();
                    case SoeTimeEngineTask.ValidatePayrollImport:
                        return this.TaskValidatePayrollImport();

                    #endregion

                    #region Payroll

                    case SoeTimeEngineTask.LockPayrollPeriod:
                        return this.TaskLockPayrollPeriod();
                    case SoeTimeEngineTask.UnLockPayrollPeriod:
                        return this.TaskUnLockPayrollPeriod();
                    case SoeTimeEngineTask.RecalculatePayrollPeriod:
                        return this.TaskRecalculatePayrollPeriod();
                    case SoeTimeEngineTask.RecalculateExportedEmploymentTaxJOB:
                        return this.TaskRecalculateExportedEmploymentTaxJOB();
                    case SoeTimeEngineTask.SavePayrollTransactionAmounts:
                        return this.TaskSavePayrollTransactionAmounts();
                    case SoeTimeEngineTask.GetUnhandledPayrollTransactions:
                        return this.TaskGetUnhandledPayrollTransactions();
                    case SoeTimeEngineTask.AssignPayrollTransactionsToTimePeriod:
                        return this.TaskAssignPayrollTransactionsToTimePeriod();
                    case SoeTimeEngineTask.ReverseTransactionsValidation:
                        return this.TaskReverseTransactionsValidation();
                    case SoeTimeEngineTask.ReverseTransactions:
                        return this.TaskReverseTransactions();
                    case SoeTimeEngineTask.SaveFixedPayrollRows:
                        return this.TaskSaveFixedPayrollRows();
                    case SoeTimeEngineTask.SaveAddedTransaction:
                        return this.TaskSaveAddedTransaction();
                    case SoeTimeEngineTask.CreateAddedTransactionsFromTemplate:
                        return this.TaskCreateAddedTransactionsFromTemplate();
                    case SoeTimeEngineTask.SavePayrollScheduleTransactions:
                        return this.TaskSavePayrollScheduleTransactions();
                    case SoeTimeEngineTask.ClearPayrollCalculation:
                        return this.TaskClearPayrollCalculation();
                    case SoeTimeEngineTask.RecalculatePayrollControllResult:
                        return this.RecalculatePayrollControll();

                    #endregion

                    #region PayrollRetro

                    case SoeTimeEngineTask.SaveRetroactivePayroll:
                        return this.TaskSaveRetroactivePayroll();
                    case SoeTimeEngineTask.SaveRetroactivePayrollOutcome:
                        return this.TaskSaveRetroactivePayrollOutcome();
                    case SoeTimeEngineTask.DeleteRetroactivePayroll:
                        return this.TaskDeleteRetroactivePayroll();
                    case SoeTimeEngineTask.CalculateRetroactivePayroll:
                        return this.TaskCalculateRetroactivePayroll();
                    case SoeTimeEngineTask.DeleteRetroactivePayrollOutcomes:
                        return this.TaskDeleteRetroactivePayrollOutcomes();
                    case SoeTimeEngineTask.CreateRetroactivePayrollTransactions:
                        return this.TaskCreateRetroactivePayrollTransactions();
                    case SoeTimeEngineTask.DeleteRetroactivePayrollTransactions:
                        return this.TaskDeleteRetroactivePayrollTransactions();

                    #endregion

                    #region PayrollStartValue

                    case SoeTimeEngineTask.SavePayrollStartValues:
                        return this.TaskSavePayrollStartValues();
                    case SoeTimeEngineTask.SaveTransactionsForPayrollStartValues:
                        return this.TaskSaveTransactionsForPayrollStartValues();
                    case SoeTimeEngineTask.DeleteTransactionsForPayrollStartValues:
                        return this.TaskDeleteTransactionsForPayrollStartValues();
                    case SoeTimeEngineTask.DeletePayrollStartValueHead:
                        return this.TaskDeletePayrollStartValueHead();

                    #endregion

                    #region Stamping

                    case SoeTimeEngineTask.SynchTimeStamps:
                        return this.TaskSynchTimeStamps();
                    case SoeTimeEngineTask.SynchGTSTimeStamps:
                        return this.TaskSynchGTSTimeStamps();
                    case SoeTimeEngineTask.ReGenerateDayBasedOnTimeStamps:
                        return this.TaskReGenerateDayBasedOnTimeStamps();
                    case SoeTimeEngineTask.SaveTimeStampsFromJob:
                        return this.TaskSaveDeviationsFromStampingJob();

                    #endregion

                    #region Schedule

                    //Template
                    case SoeTimeEngineTask.GetTimeScheduleTemplate:
                        return this.TaskGetTimeScheduleTemplate();
                    case SoeTimeEngineTask.SaveTimeScheduleTemplate:
                        return this.TaskSaveTimeScheduleTemplate();
                    case SoeTimeEngineTask.SaveTimeScheduleTemplateStaffing:
                        return this.TaskSaveTimeScheduleTemplateStaffing();
                    case SoeTimeEngineTask.UpdateTimeScheduleTemplateStaffing:
                        return this.TaskUpdateTimeScheduleTemplateStaffing();
                    case SoeTimeEngineTask.DeleteTimeScheduleTemplate:
                        return this.TaskDeleteTimeScheduleTemplate();
                    case SoeTimeEngineTask.RemoveEmployeeFromTimeScheduleTemplate:
                        return this.TaskRemoveEmployeeFromTimeScheduleTemplate();
                    case SoeTimeEngineTask.AssignTimeScheduleTemplateToEmployee:
                        return this.TaskAssignTimeScheduleTemplateToEmployee();

                    //Schedule
                    case SoeTimeEngineTask.GetSequentialSchedule:
                        return this.TaskGetSequentialSchedule();
                    case SoeTimeEngineTask.SaveShiftPrelToDef:
                        return this.TaskSaveShiftPrelToDef();
                    case SoeTimeEngineTask.SaveShiftDefToPrel:
                        return this.TaskSaveShiftDefToPrel();
                    case SoeTimeEngineTask.CopySchedule:
                        return this.TaskCopySchedule();
                    case SoeTimeEngineTask.GenerateAndSaveAbsenceFromStaffing:
                        return this.TaskGenerateAndSaveAbsenceFromStaffing();

                    //Breaks
                    case SoeTimeEngineTask.GetBreaksForScheduleBlock:
                        return this.TaskGetBreaksForScheduleBlock();
                    case SoeTimeEngineTask.HasEmployeeValidTimeCodeBreak:
                        return this.TaskHasEmployeeValidTimeCodeBreak();
                    case SoeTimeEngineTask.ValidateBreakChange:
                        return this.TaskValidateBreakChange();

                    //Scenario
                    case SoeTimeEngineTask.SaveTimeScheduleScenarioHead:
                        return this.TaskSaveTimeScheduleScenarioHead();
                    case SoeTimeEngineTask.RemoveAbsenceInScenario:
                        return this.TaskRemoveAbsenceInScenario();
                    case SoeTimeEngineTask.ActivateScenario:
                        return this.TaskActivateScenario();
                    case SoeTimeEngineTask.CreateTemplateFromScenario:
                        return this.TaskCreateTemplateFromScenario();

                    //Placement
                    case SoeTimeEngineTask.SaveEmployeeSchedulePlacement:
                        return this.TaskSaveEmployeeSchedulePlacement();
                    case SoeTimeEngineTask.SaveEmployeeSchedulePlacementStaffing:
                        return this.TaskSaveEmployeeSchedulePlacementStaffing();
                    case SoeTimeEngineTask.SaveEmployeeSchedulePlacementFromJob:
                        return this.TaskSaveEmployeeSchedulePlacementFromJob();
                    case SoeTimeEngineTask.DeleteEmployeeSchedulePlacement:
                        return this.TaskDeleteEmployeeSchedulePlacement();
                    case SoeTimeEngineTask.ControlEmployeeSchedulePlacement:
                        return this.TaskControlEmployeeSchedulePlacement();

                    //EmployeeRequest
                    case SoeTimeEngineTask.GetEmployeeRequests:
                        return this.TaskGetEmployeeRequests();
                    case SoeTimeEngineTask.LoadEmployeeRequest:
                        return this.TaskLoadEmployeeRequest();
                    case SoeTimeEngineTask.SaveEmployeeRequest:
                        return this.TaskSaveEmployeeRequest();
                    case SoeTimeEngineTask.DeleteEmployeeRequest:
                        return this.TaskDeleteEmployeeRequest();
                    case SoeTimeEngineTask.SaveOrDeleteEmployeeRequest:
                        return this.TaskSaveOrDeleteEmployeeRequest();
                    case SoeTimeEngineTask.PerformAbsenceRequestPlanningAction:
                        return this.TaskPerformAbsenceRequestPlanningAction();

                    //Shift
                    case SoeTimeEngineTask.GetAvailableTime:
                        return this.TaskGetAvailableTime();
                    case SoeTimeEngineTask.GetAvailableEmployees:
                        return this.TaskGetAvailableEmployees();
                    case SoeTimeEngineTask.InitiateScheduleSwap:
                        return this.TaskInitiateScheduleSwap();
                    case SoeTimeEngineTask.ApproveScheduleSwap:
                        return this.TaskApproveScheduleSwap();
                    case SoeTimeEngineTask.SaveTimeScheduleShift:
                        return this.TaskSaveTimeScheduleShift();
                    case SoeTimeEngineTask.DeleteTimeScheduleShift:
                        return this.TaskDeleteTimeScheduleShift();
                    case SoeTimeEngineTask.HandleTimeScheduleShift:
                        return this.TaskHandleTimeScheduleShift();
                    case SoeTimeEngineTask.SplitTimeScheduleShift:
                        return this.TaskSplitTimeScheduleShift();
                    case SoeTimeEngineTask.DragTimeScheduleShift:
                        return this.TaskDragTimeScheduleShift();
                    case SoeTimeEngineTask.DragTimeScheduleShiftMultipel:
                        return this.TaskDragTimeScheduleShiftMultipel();
                    case SoeTimeEngineTask.SplitTemplateTimeScheduleShift:
                        return this.TaskSplitTemplateShift();
                    case SoeTimeEngineTask.DragTemplateTimeScheduleShift:
                        return this.TaskDragTemplateTimeScheduleShift();
                    case SoeTimeEngineTask.DragTemplateTimeScheduleShiftMultipel:
                        return this.TaskDragTemplateTimeScheduleShiftMultipel();
                    case SoeTimeEngineTask.RemoveEmployeeFromShiftQueue:
                        return this.TaskRemoveEmployeeFromShiftQueue();
                    case SoeTimeEngineTask.AssignTaskToEmployee:
                        return this.TaskAssignTaskToEmployee();
                    case SoeTimeEngineTask.AssignTemplateShiftTask:
                        return this.TaskAssignTemplateShiftTask();
                    case SoeTimeEngineTask.PerformRestoreAbsenceRequestedShifts:
                        return this.TaskPerformRestoreAbsenceRequestedShifts();
                    case SoeTimeEngineTask.EmployeeActiveScheduleImport:
                        return this.TaskEmployeeActiveScheduleImport();

                    #endregion

                    #region Time

                    case SoeTimeEngineTask.RestoreDaysToSchedule:
                        return this.TaskRestoreDaysToSchedule();
                    case SoeTimeEngineTask.RestoreToScheduleDiscardDeviations:
                        return this.TaskRestoreDaysToScheduleDiscardDeviations();
                    case SoeTimeEngineTask.RestoreDaysToTemplateSchedule:
                        return this.TaskRestoreDaysToTemplateSchedule();
                    case SoeTimeEngineTask.ReGenerateTransactionsDiscardAttest:
                        return this.TaskReGenerateTransactionsDiscardAttest();
                    case SoeTimeEngineTask.CleanDays:
                        return this.TaskCleanDays();
                    case SoeTimeEngineTask.SaveTimeCodeTransactions:
                        return this.TaskSaveTimeCodeTransactions();
                    case SoeTimeEngineTask.CreateTransactionsForPlannedPeriodCalculation:
                        return this.TaskCreateTransactionsForPlannedPeriodCalculation();

                    #endregion

                    #region Project

                    case SoeTimeEngineTask.SaveOrderShift:
                        return this.TaskSaveOrderShift();
                    case SoeTimeEngineTask.SaveOrderAssignments:
                        return this.TaskSaveOrderAssignments();
                    case SoeTimeEngineTask.SaveTimeBlocksFromProjectTimeBlock:
                        return this.TaskSaveTimeBlocksFromProjectTimeBlocks();

                    #endregion

                    #region TimeWorkAccount

                    case SoeTimeEngineTask.CalculateTimeWorkAccountYearEmployee:
                        return this.TaskCalculateTimeWorkAccountYearEmployee();
                    case SoeTimeEngineTask.CalculateTimeWorkAccountYearEmployeeBasis:
                        return this.TaskCalculateTimeWorkAccountYearEmployeeBasis();
                    case SoeTimeEngineTask.TimeWorkAccountChoiceSendXEMail:
                        return this.TaskTimeWorkAccountChoiceSendXEMail();
                    case SoeTimeEngineTask.TimeWorkAccountGenerateOutcome:
                        return this.TaskTimeWorkAccountGenerateOutcome();
                    case SoeTimeEngineTask.TimeWorkAccountReverseTransaction:
                        return this.TaskTimeWorkAccountReverseTransaction();
                    case SoeTimeEngineTask.TimeWorkAccountGenerateUnusedPaidBalance:
                        return this.TaskTimeWorkAccountGenerateUnusedPaidBalance();
                    case SoeTimeEngineTask.TimeWorkAccountYearReversePaidBalance:
                        return this.TaskTimeWorkAccountYearReversePaidBalance();

                    #endregion

                    #region TimeWorkReduction

                    case SoeTimeEngineTask.CalculateTimeWorkReductionReconciliationYearEmployee:
                        return this.TaskCalculateTimeWorkReductionReconciliationEmployee();

                    case SoeTimeEngineTask.TimeWorkReductionReconciliationYearEmployeeGenerateOutcome:
                        return this.TaskTimeWorkReductionReconciliationYearEmployeeGenerateOutcome();

                    case SoeTimeEngineTask.TimeWorkReductionReconciliationYearEmployeeReverseTransactions:
                        return this.TaskTimeWorkReductionReconciliationYearEmployeeReverseTransactions();
                    #endregion

                    #region WorkRules

                    case SoeTimeEngineTask.SaveEvaluateAllWorkRulesByPass:
                        return this.TaskSaveEvaluateAllWorkRulesByPass();
                    case SoeTimeEngineTask.EvaluateAllWorkRules:
                        return this.TaskEvaluateAllWorkRules();
                    case SoeTimeEngineTask.EvaluatePlannedShiftsAgainstWorkRules:
                        return this.TaskEvaluatePlannedShiftsAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluatePlannedShiftsAgainstWorkRulesEmployeePost:
                        return this.TaskEvaluatePlannedShiftsAgainstWorkRulesEmployeePost();
                    case SoeTimeEngineTask.EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules:
                        return this.TaskEvaluateAbsenceRequestPlannedShiftsAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateDragShiftAgainstWorkRules:
                        return this.TaskEvaluateDragShiftAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateDragShiftAgainstWorkRulesMultipel:
                        return this.TaskEvaluateDragShiftAgainstWorkRulesMultipel();
                    case SoeTimeEngineTask.EvaluateDragTemplateShiftAgainstWorkRules:
                        return this.TaskEvaluateDragTemplateShiftAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateDragTemplateShiftAgainstWorkRulesMultipel:
                        return this.TaskEvaluateDragTemplateShiftAgainstWorkRulesMultipel();
                    case SoeTimeEngineTask.EvaluateSplitShiftAgainstWorkRules:
                        return this.TaskEvaluateSplitShiftAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateSplitTemplateShiftAgainstWorkRules:
                        return this.TaskEvaluateSplitTemplateShiftAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateAssignTaskToEmployeeAgainstWorkRules:
                        return this.TaskEvaluateAssignTaskToEmployeeAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateActivateScenarioAgainstWorkRules:
                        return this.TaskEvaluateActivateScenarioAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateDeviationsAgainstWorkRulesAndSendXEMail:
                        return this.TaskEvaluateDeviationsAgainstWorkRulesAndSendXEMail();
                    case SoeTimeEngineTask.EvaluateScenarioToTemplateAgainstWorkRules:
                        return this.TaskEvaluateScenarioToTemplateAgainstWorkRules();
                    case SoeTimeEngineTask.EvaluateScheduleSwapAgainstWorkRules:
                        return this.TaskEvaluateScheduleSwapAgainstWorkRules();
                    case SoeTimeEngineTask.IsDayAttested:
                        return this.TaskIsDayAttested();

                    #endregion

                    default:
                        return new TimeEngineOutputDTO();
                }
            }
            finally
            {
                CompleteTask(base.currentTaskWatchLogId);
            }
        }

        private void InitBeforeRunningTask(SoeTimeEngineTask task)
        {
            base.currentTaskWatchLogId = StartTask(task);

            if (templateRepository == null)
                templateRepository = new TimeEngineTemplateRepository();

        }

        #region Core functions

        public bool IsValid(int actorCompanyId, int userId)
        {
            return this.actorCompanyId == actorCompanyId && this.userId == userId;
        }

        protected void CreateTaskInput(TimeEngineInputDTO iDTO)
        {
            this._iDTO = iDTO;
        }

        protected T GetInputDTO<T>() where T : TimeEngineInputDTO
        {
            return this._iDTO is T ? this._iDTO as T : null;
        }

        protected (T_in, T_out) InitTask<T_in, T_out>()
            where T_in : TimeEngineInputDTO
            where T_out : TimeEngineOutputDTO, new()
        {
            var iDTO = GetInputDTO<T_in>();
            var oDTO = (T_out)Activator.CreateInstance(typeof(T_out), iDTO != null);
            return (iDTO, oDTO);
        }

        private void InitContext(CompEntities entities)
        {
            if (entities == null)
                return;

            if (forcedExternalEntities && this.entities != null)
                return;

            ClearCachedContent();
            this.entities = entities;
        }

        private void InitTransaction(TransactionScope transaction)
        {
            if (transaction == null)
                return;

            this.currentTransaction = transaction;

            if (this.entities.Connection.State != ConnectionState.Open)
                this.entities.Connection.Open();
        }

        private ActionResult BulkSave()
        {
            return SaveChanges(this.entities, this.currentTransaction, useBulkSaveChanges: true);
        }

        private ActionResult Save()
        {
            return SaveChanges(this.entities, this.currentTransaction);
        }

        private bool TrySave(TimeEngineOutputDTO oDTO)
        {
            if (oDTO?.Result != null && oDTO.Result.Success)
            {
                oDTO.Result = Save();
                return true;
            }
            return false;
        }

        private void TrySaveAndCommit(TimeEngineOutputDTO oDTO)
        {
            if (TrySave(oDTO))
                TryCommit(oDTO);
        }

        private bool TryCommit(TimeEngineOutputDTO oDTO)
        {
            try
            {
                if (oDTO?.Result != null && oDTO.Result.Success && this.currentTransaction != null)
                {
                    this.currentTransaction.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                oDTO.Result = new ActionResult(ex);
                return false;
            }

            return false;
        }

        private void Rollback()
        {
            try
            {
                // Delete added objects that did not get saved
                foreach (var objectStateEntry in this.entities.ObjectStateManager.GetObjectStateEntries(System.Data.Entity.EntityState.Added))
                {
                    try
                    {
                        if (objectStateEntry.State == System.Data.Entity.EntityState.Added && objectStateEntry.Entity != null)
                            this.entities.DeleteObject(objectStateEntry.Entity);
                    }
                    catch (Exception ex)
                    {
                        LogError(GetRollbackErrorMessage(objectStateEntry, "Added"));
                        LogError(ex);
                    }
                }
                // Refetch modified objects from database
                foreach (var objectStateEntry in this.entities.ObjectStateManager.GetObjectStateEntries(System.Data.Entity.EntityState.Modified))
                {
                    try
                    {
                        if (objectStateEntry.Entity != null)
                            this.entities.Refresh(RefreshMode.StoreWins, objectStateEntry.Entity);
                    }
                    catch (Exception ex)
                    {
                        LogError(GetRollbackErrorMessage(objectStateEntry, "Modified"));
                        LogError(ex);
                    }
                }
                // Recover modified objects that got deleted
                foreach (var objectStateEntry in this.entities.ObjectStateManager.GetObjectStateEntries(System.Data.Entity.EntityState.Deleted))
                {
                    try
                    {
                        if (objectStateEntry.Entity != null)
                            this.entities.Refresh(RefreshMode.StoreWins, objectStateEntry.Entity);
                    }
                    catch (Exception ex)
                    {
                        LogError(GetRollbackErrorMessage(objectStateEntry, "Deleted"));
                        LogError(ex);
                    }
                }
                this.entities.AcceptAllChanges();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            string GetRollbackErrorMessage(ObjectStateEntry objectStateEntry, string action)
            {
                return $"Failed to rollback. Action:{action}. State:{objectStateEntry?.State}";
            }
        }

        private void Log(Level level, Exception ex)
        {
            if (level == Level.Info)
                LogInfo(ex);
            else if (level == Level.Warn)
                LogWarning(ex);
            else if (level == Level.Error)
                LogError(ex);
        }

        private void LogInfo(Exception ex)
        {
            base.LogInfo(ex, this.log);
        }

        private void LogWarning(Exception ex)
        {
            base.LogWarning(ex, this.log);
        }

        private void LogError(Exception ex)
        {
            base.LogError(ex, this.log, base.currentTaskWatchLogId);
        }

        private void LogTransactionFailed(string source)
        {
            base.LogTransactionFailed(source, this.log);
        }

        private void ApplyPlausibilityCheck(TimeEngineTemplate template)
        {
            if (template?.Outcome == null)
                return;

            ApplyPlausibilityCheck(template.Employee, template.Date, template.Outcome.TimeCodeTransactions, template.Outcome.TimePayrollTransactions, template.Identity.TimeBlocks);
        }
        private void ApplyPlausibilityCheck(Employee employee, DateTime date, List<TimeCodeTransaction> timeCodeTransactions, List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlock> timeBlocks, string callerPrefix = "")
        {
            try
            {

                if (employee == null)
                    return;

                CheckUnpayedTransactions();
                CheckInternalAccountIsMissing();

                int? scopeLicenseId = null;
                int? scopeActorCompanyId = null;
                bool EvaluateScope(int licenseId, int actorCompanyId)
                {
                    if (base.LicenseId != licenseId || base.ActorCompanyId != actorCompanyId)
                        return false;
                    scopeLicenseId = licenseId;
                    scopeActorCompanyId = actorCompanyId;
                    return true;
                }

                /* Från 91565 COOP ObetaLd tid efter schema ut
                   Kontrollera om det endast fallit ut en lönetrans och flera tidkodstransar.
                   Tidkodstransen har skapats för tidkod Obetald tid efter schema ut (78063)
                   Lönetransen är löneart Arbetada dagar (4206336)
                   Läs om scheman ocachat och kontrollera om transaktionerna verkligen ligger utanför schemat.
                   Om inte, logga ett tydligt fel
                */
                void CheckUnpayedTransactions()
                {
                    //Only Coop CBS
                    if (!EvaluateScope(1521, 2149518))
                        return;

                    int timeCodeIdUnpayedTime = 78063; //Obetald tid efter schema ut
                    int timeCodeIdWorkedDays = 78066; //Arbetade dagar
                    int payrollProductIdWorkedDays = 4206336; //Arbetade dgr

                    //Evaluate TimeCodeTransactions
                    if (timeCodeTransactions.IsNullOrEmpty())
                        return;
                    List<int> outcomeTimeCodeIds = timeCodeTransactions.Select(t => t.TimeCodeId).Distinct().ToList();
                    if (outcomeTimeCodeIds.Count != 2)
                        return;
                    if (!outcomeTimeCodeIds.Contains(timeCodeIdUnpayedTime))
                        return;
                    if (!outcomeTimeCodeIds.Contains(timeCodeIdWorkedDays))
                        return;

                    //Evaluate TimePayrollTransactions
                    if (timePayrollTransactions?.Count != 1)
                        return;
                    if (timePayrollTransactions[0].ProductId != payrollProductIdWorkedDays)
                        return;

                    //Re-fetch data
                    List<TimeScheduleTemplateBlock> schedule = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employee.EmployeeId, date, includeStandBy: true);
                    if (schedule.IsNullOrEmpty())
                        return;
                    DateTime scheduleOut = schedule.GetScheduleOut();
                    List<TimeCodeTransaction> unpayedTimeCodeTransactions = timeCodeTransactions.Where(t => t.TimeCodeId == timeCodeIdUnpayedTime).ToList();
                    if (unpayedTimeCodeTransactions.All(t => t.Stop > scheduleOut))
                        return;

                    //Log
                    Log(actorCompanyId, "CheckUnpayedTransactions", schedule.ToJson());
                }

                void CheckInternalAccountIsMissing()
                {
                    //Only ICA Maxi Special
                    if (!EvaluateScope(281, 326666))
                        return;

                    List<int> productIds = new List<int>();
                    productIds.Add(73536); //030 Timlön               
                    productIds.Add(73791); //Arbetad tid månadslön    
                    int accountDimId = 2390;//Kostnadsställe

                    var invalidTransactions = timePayrollTransactions.Where(x => productIds.Contains(x.ProductId)  && x.AccountInternal.Count < 3).ToList();
                    if (invalidTransactions.IsNullOrEmpty())
                        return;

                    List<string> details = new List<string>();
                    var timeBlockIds = invalidTransactions.Select(x => x.TimeBlockId).ToList();
                    var invalidTimeBlocks = timeBlocks.Where(x => timeBlockIds.Contains(x.TimeBlockId) && x.AccountInternal.Count < 3).ToList();
                    bool invalidTimeBlocksExists = invalidTimeBlocks.Any();
                    if (invalidTimeBlocksExists)
                        details.Add("invalidTimeBlocksExists: true");

                    //Re-fetch data
                    AccountingPrioDTO accountingPrio = GetAccountingPrioByPayrollProductFromCache(GetPayrollProductFromCache(73536), employee, date);
                    int? accountId = accountingPrio.GetAccountInternalId(accountDimId);
                    bool accountMissingInPrio = accountId == null || accountId.Value == 0;
                    if (accountMissingInPrio)
                        details.Add("accountMissingInPrio: true");

                    accountingPrio = GetAccountingPrioByPayrollProductFromCache(GetPayrollProductFromCache(73791), employee, date);
                    accountId = accountingPrio.GetAccountInternalId(accountDimId);
                    accountMissingInPrio = accountId == null || accountId.Value == 0;
                    if (accountMissingInPrio)
                        details.Add("accountMissingInPrio: true");

                    //Log
                    Log(actorCompanyId, "CheckInternalAccountIsMissing", invalidTransactions.ToJson(), details.ToJson());
                }

                void Log(int actorCompanyId, string methodName, string data, string details = "")
                {
                    LogError($"Plausibility check {methodName} failed for company {actorCompanyId} and employee {employee.NumberAndName} on {date.ToShortDateString()}. Task:{(int)this.currentTask} Data: ({data}) Details: ({details}) Prefix: ({callerPrefix})");
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void CheckDuplicateTimeBlocks(TimeEngineTemplate template, params string[] comments)
        {
            if (!template.HasValidIdentity())
                return;

            CheckDuplicateTimeBlocks(template.Identity.TimeBlocks, template.Employee, template.TimeBlockDate, comments);
        }

        private void CheckDuplicateTimeBlocks(List<TimeBlock> timeBlocks, Employee employee, TimeBlockDate timeBlockDate, params string[] comments)
        {
            if (timeBlocks.TryGetDuplicateJson(out string json, employee, timeBlockDate, (int)base.currentTask, comments))
                LogError($"DuplicateTimeBlock. JSON: {json}");
        }

        private void CheckAttemptToAddDuplicateTimeBlock(List<TimeBlock> timeBlocks, TimeBlock timeBlock, params string[] comments)
        {
            if (timeBlocks.TryGetDuplicateJson(out string json, timeBlock, base.ActorCompanyId, (int)base.currentTask, comments))
                LogWarning($"AttemptToAddDuplicateTimeBlock. JSON: {json}");
        }

        private void LogEmployeeIdOnTransactionAndTimeBlockDateMissMatch(List<TimePayrollTransaction> timePayrollTransactions)
        {
            foreach (var timePayrollTransaction in timePayrollTransactions)
                LogEmployeeIdOnTransactionAndTimeBlockDateMissMatch(timePayrollTransaction);
        }

        private void LogEmployeeIdOnTransactionAndTimeBlockDateMissMatch(TimePayrollTransaction timePayrollTransaction)
        {
            if (timePayrollTransaction.TimeBlockDate != null && timePayrollTransaction.EmployeeId != timePayrollTransaction.TimeBlockDate.EmployeeId)
                LogError($"EmployeeId missmatch.Task:{(int)base.currentTask}, ActorCompanyId:{actorCompanyId}," + $"TransactionId:{timePayrollTransaction.TimePayrollTransactionId}");
        }

        private long? StartTask(SoeTimeEngineTask task)
        {
            this.currentTask = task;

            if (GetTasksToExcludeFromTaskWatchLog().Contains(task))
                return 0;

            string name = $"PerformTask_{(int)base.currentTask}_{Enum.GetName(typeof(SoeTimeEngineTask), base.currentTask)}";
            string className = MethodBase.GetCurrentMethod().DeclaringType.ToString();
            string batch = Guid.NewGuid().ToString();
            string parameters = this._iDTO?.ToString();
            int? idCount = this._iDTO?.GetIdCount();
            int? intervalCount = this._iDTO?.GetIntervalCount();

            return base.StartTask(name, className, batch, parameters, idCount, intervalCount);
        }

        private IEnumerable<SoeTimeEngineTask> GetTasksToExcludeFromTaskWatchLog()
        {
            yield return SoeTimeEngineTask.CalculateDayTypeForEmployee;
            yield return SoeTimeEngineTask.CalculateDayTypeForEmployees;
            yield return SoeTimeEngineTask.CalculateDayTypesForEmployee;
            yield return SoeTimeEngineTask.SynchTimeStamps;
            yield return SoeTimeEngineTask.GetDayOfAbsenceNumber;
        }

        #endregion

        #region State Deleted

        private ActionResult SetScheduleToDeleted(EmployeeSchedule employeeSchedule, bool saveChanges = true, bool discardCheckes = false)
        {
            if (employeeSchedule == null)
                return new ActionResult(true);

            var result = ChangeEntityState(entities, employeeSchedule, SoeEntityState.Deleted, saveChanges, GetUserFromCache(), discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetScheduleToDeleted(int employeeScheduleId, int employeeId, DateTime dateFrom, DateTime dateTo, bool saveChanges = true)
        {
            var scheduleBlocks = GetScheduleBlocksForEmployee(null, employeeId, dateFrom, dateTo, employeeScheduleId, loadStaffingIfUsed: true, includeOnDuty: true);

            var result = SetScheduleToDeleted(scheduleBlocks, saveChanges: false);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetScheduleToDeleted(List<TimeScheduleTemplateBlock> templateBlocks, bool saveChanges = true)
        {
            if (templateBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = SetTimeScheduleBlocksToDeleted(templateBlocks, saveChanges: false);
            if (result.Success && UseStaffing())
                result = SetTimeScheduleTemplateBlockTasksToDeleted(templateBlocks, saveChanges: false);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetScheduleAndDeviationsToDeleted(int employeeId, DateTime dateFrom, DateTime dateTo, bool saveChanges = true, bool discardCheckes = false)
        {
            var scheduleBlocks = GetScheduleBlocksForEmployee(null, employeeId, dateFrom, dateTo, loadStaffingIfUsed: true, includeOnDuty: true);
            var timeBlocks = GetTimeBlocksWithTransactions(employeeId, dateFrom, dateTo);

            var result = SetScheduleAndDeviationsToDeleted(scheduleBlocks, timeBlocks, saveChanges: false, discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetScheduleAndDeviationsToDeleted(int employeeId, DateTime date, int timeBlockDateId, bool saveChanges = true, bool discardCheckes = false)
        {
            var scheduleBlocks = GetScheduleBlocksForEmployee(null, employeeId, date, date, loadStaffingIfUsed: true, includeOnDuty: true);
            var timeBlocks = GetTimeBlocksWithTransactions(employeeId, timeBlockDateId);

            var result = SetScheduleAndDeviationsToDeleted(scheduleBlocks, timeBlocks, saveChanges: false, discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetScheduleAndDeviationsToDeleted(List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks, bool saveChanges = true, bool discardCheckes = false)
        {
            if (scheduleBlocks == null || timeBlocks == null)
                return new ActionResult(true);

            var result = SetTimeScheduleBlocksToDeleted(scheduleBlocks, saveChanges: false);
            if (result.Success && UseStaffing())
                result = SetTimeScheduleTemplateBlockTasksToDeleted(scheduleBlocks, saveChanges: false);
            if (result.Success)
                result = SetTimeBlocksAndTransactionsToDeleted(timeBlocks, saveChanges: false, discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimeScheduleBlocksToDeleted(List<TimeScheduleTemplateBlock> scheduleBlocks, bool saveChanges = true)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var scheduleBlock in scheduleBlocks)
            {
                result = ChangeEntityState(scheduleBlock, SoeEntityState.Deleted, GetUserFromCache());
                if (result.Success && scheduleBlock.TimeScheduleTemplateBlockTask != null)
                    result = SetTimeScheduleTemplateBlockTasksToDeleted(scheduleBlock.TimeScheduleTemplateBlockTask.ToList(), saveChanges: false);
            }

            ClearScheduleFromCache(scheduleBlocks);

            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeScheduleBlockZeroToDeleted(int? timeScheduleScenarioHeadId, DateTime date, int? templatePeriodId, int employeeId, bool saveChanges = true)
        {
            var scheduleBlocks = GetScheduleBlockZero(timeScheduleScenarioHeadId, employeeId, date, templatePeriodId);
            if (scheduleBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
            {
                result = ChangeEntityState(scheduleBlock, SoeEntityState.Deleted, GetUserFromCache());
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimeScheduleTemplateBlockTasksToDeleted(List<TimeScheduleTemplateBlock> templateBlocks, bool saveChanges = true)
        {
            if (templateBlocks == null)
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var templateBlockBlockTasks in templateBlocks.Select(t => t.TimeScheduleTemplateBlockTask))
            {
                if (!templateBlockBlockTasks.IsLoaded)
                    templateBlockBlockTasks.Load();

                result = SetTimeScheduleTemplateBlockTasksToDeleted(templateBlockBlockTasks.ToList(), saveChanges: false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeScheduleTemplateBlockTasksToDeleted(List<TimeScheduleTemplateBlockTask> timeScheduleTemplateBlockTasks, bool saveChanges = true)
        {
            if (timeScheduleTemplateBlockTasks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeScheduleTemplateBlockTask in timeScheduleTemplateBlockTasks)
            {
                result = SetTimeScheduleTemplateBlockTaskToDeleted(timeScheduleTemplateBlockTask, saveChanges: false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeScheduleTemplateBlockTaskToDeleted(TimeScheduleTemplateBlockTask timeScheduleTemplateBlockTask, bool saveChanges = true)
        {
            if (timeScheduleTemplateBlockTask == null)
                return new ActionResult(true);

            var result = ChangeEntityState(timeScheduleTemplateBlockTask, SoeEntityState.Deleted, GetUserFromCache());
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimeBlockDateDetailsToDeleted(List<TimeBlockDate> timeBlockDates, SoeTimeBlockDateDetailType type, int? outcomeId, bool saveChanges = false, bool onlyWithoutRatio = false)
        {
            if (timeBlockDates.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeBlockDate in timeBlockDates)
            {
                result = SetTimeBlockDateDetailsToDeleted(timeBlockDate, type, outcomeId, saveChanges: saveChanges, onlyWithoutRatio: onlyWithoutRatio);
                if (!result.Success)
                    return result;
            }

            return result;
        }
        private ActionResult SetTimeBlockDateDetailsToDeleted(TimeBlockDate timeBlockDate, SoeTimeBlockDateDetailType type, int? outcomeId, DateTime? modified = null, string modifiedBy = null, bool onlyWithoutRatio = false, bool saveChanges = false)
        {
            if (timeBlockDate != null && !timeBlockDate.TimeBlockDateDetail.IsLoaded)
                timeBlockDate.TimeBlockDateDetail.Load();

            var timeBlockDateDetails = timeBlockDate.GetTimeBlockDateDetailsToDelete(type, outcomeId);
            if (timeBlockDateDetails.IsNullOrEmpty())
                return new ActionResult(true);
            
            if (!modified.HasValue)
                modified = DateTime.Now;

            var result = new ActionResult(true);

            foreach (var timeBlockDateDetail in timeBlockDateDetails)
            {
                if (onlyWithoutRatio && timeBlockDateDetail.Ratio.HasValue)
                    continue;

                result = ChangeEntityState(timeBlockDateDetail, SoeEntityState.Deleted);
                if (!result.Success)
                    return result;

                timeBlockDateDetail.Modified = modified;
                if (!modifiedBy.IsNullOrEmpty())
                    timeBlockDateDetail.ModifiedBy = modifiedBy;
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(TimeBlockDate timeBlockDate, int employeeId, bool doDeleteTimeBlockDateDetails = false, bool doDeleteTimeBlockDateDetailsWithoutRatio = false, bool clearScheduledAbsence = false, bool saveChanges = true, bool discardCheckes = false, List<ActionResultSave> acceptErrors = null)
        {
            var timeBlocks = GetTimeBlocksWithTransactions(employeeId, timeBlockDate.TimeBlockDateId, onlyActive: false);

            return SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(timeBlocks, timeBlockDate, employeeId, doDeleteTimeBlockDateDetails, doDeleteTimeBlockDateDetailsWithoutRatio, clearScheduledAbsence, saveChanges, discardCheckes, acceptErrors: acceptErrors);
        }
        private ActionResult SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(List<TimeBlock> timeBlocks, TimeBlockDate timeBlockDate, int employeeId, bool doDeleteTimeBlockDateDetails = false, bool doDeleteTimeBlockDateDetailsWithoutRatio = false, bool clearScheduledAbsence = false, bool saveChanges = true, bool discardCheckes = false, List<ActionResultSave> acceptErrors = null)
        {
            if (timeBlockDate == null)
                return new ActionResult(true);

            var result = SetTimeBlocksAndTransactionsToDeleted(timeBlocks, saveChanges: false, discardCheckes: discardCheckes, acceptErrors: acceptErrors);
            if (result.Success)
                SetTimePayrollScheduleTransactionsToDeleted(timeBlockDate.TimeBlockDateId, employeeId, saveChanges: false);
            if (result.Success && (doDeleteTimeBlockDateDetails || doDeleteTimeBlockDateDetailsWithoutRatio))
                result = SetTimeBlockDateDetailsToDeleted(timeBlockDate, SoeTimeBlockDateDetailType.Absence, null, onlyWithoutRatio: doDeleteTimeBlockDateDetailsWithoutRatio, saveChanges: true);
            if (result.Success && clearScheduledAbsence && UseStaffing())
                ClearTimeScheduleTemplateBlocks(employeeId, timeBlockDate.Date, clearScheduledAbsence: true, clearScheduledPlacement: false, updateOrderRemainingTime: false);
            if (result.Success && saveChanges)
                result = Save();
            if (result.Success)
                AddCurrentDayPayrollWarning(timeBlockDate);

            return result;
        }

        private ActionResult SetTimeBlocksAndTransactionsToDeleted(List<TimeBlock> timeBlocks, bool saveChanges = true, bool discardCheckes = false, List<ActionResultSave> acceptErrors = null)
        {
            if (timeBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeBlock in timeBlocks)
            {
                result = SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false, discardCheckes: discardCheckes);
                if (!result.Success && !result.DoAcceptError(acceptErrors))
                    return result;
            }

            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeBlockAndTransactionsToDeleted(TimeBlock timeBlock, bool saveChanges = true, bool discardCheckes = false)
        {
            if (timeBlock == null)
                return new ActionResult(true);

            var result = SetTransactionsToDeleted(timeBlock, saveChanges: saveChanges, discardCheckes: discardCheckes);
            if (result.Success)
                result = ChangeEntityState(timeBlock, SoeEntityState.Deleted, GetUserFromCache(), discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTransactionsToDeleted(List<TimeBlock> timeBlocks, bool saveChanges = true, bool discardCheckes = false, bool isAbsenceRecalculation = false)
        {
            if (timeBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeBlock in timeBlocks)
            {
                result = SetTransactionsToDeleted(timeBlock, saveChanges: false, discardCheckes: discardCheckes, isAbsenceRecalculation: isAbsenceRecalculation);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTransactionsToDeleted(TimeBlock timeBlock, bool saveChanges = true, bool discardCheckes = false, bool isAbsenceRecalculation = false)
        {
            if (timeBlock == null)
                return new ActionResult(true);

            var result = SetExternalTransactionsToDeleted(timeBlock, saveChanges: false, discardCheckes: discardCheckes, isAbsenceRecalculation: isAbsenceRecalculation);
            if (result.Success)
                result = SetTimeCodeTransactionsToDeleted(timeBlock, saveChanges: false);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetExternalTransactionsToDeleted(TimeBlock timeBlock, bool saveChanges = true, bool discardCheckes = false, bool isAbsenceRecalculation = false)
        {
            if (timeBlock == null)
                return new ActionResult(true);

            var result = new ActionResult(true);

            if (CanEntityLoadReferences(entities, timeBlock) && !timeBlock.TimeInvoiceTransaction.IsLoaded)
                timeBlock.TimeInvoiceTransaction.Load();
            if (CanEntityLoadReferences(entities, timeBlock) && !timeBlock.TimePayrollTransaction.IsLoaded)
                timeBlock.TimePayrollTransaction.Load();

            if (result.Success && !timeBlock.TimeInvoiceTransaction.IsNullOrEmpty())
                result = SetTimeInvoiceTransactionsToDeleted(timeBlock.TimeInvoiceTransaction.ToList(), saveChanges: false, discardCheckes: discardCheckes);
            if (result.Success && !timeBlock.TimePayrollTransaction.IsNullOrEmpty())
                result = SetTimePayrollTransactionsToDeleted(timeBlock.TimePayrollTransaction.ToList(), saveChanges: false, discardCheckes: discardCheckes, isAbsenceRecalculation: isAbsenceRecalculation);

            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetExternalTransactionsToDeleted(TimeCodeTransaction timeCodeTransaction, bool saveChanges = true, bool discardCheckes = false)
        {
            if (timeCodeTransaction == null)
                return new ActionResult(true);

            var result = new ActionResult(true);

            if (CanEntityLoadReferences(entities, timeCodeTransaction) && !timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                timeCodeTransaction.TimeInvoiceTransaction.Load();
            if (CanEntityLoadReferences(entities, timeCodeTransaction) && !timeCodeTransaction.TimePayrollTransaction.IsLoaded)
                timeCodeTransaction.TimePayrollTransaction.Load();

            if (result.Success && !timeCodeTransaction.TimeInvoiceTransaction.IsNullOrEmpty())
            {
                result = SetTimeInvoiceTransactionsToDeleted(timeCodeTransaction.TimeInvoiceTransaction.ToList(), saveChanges: false, discardCheckes: discardCheckes);
                if (result.Success && saveChanges)
                    result = Save();
            }

            if (result.Success && !timeCodeTransaction.TimePayrollTransaction.IsNullOrEmpty())
            {
                result = SetTimePayrollTransactionsToDeleted(timeCodeTransaction.TimePayrollTransaction.ToList(), saveChanges: false, discardCheckes: discardCheckes);
                if (result.Success && saveChanges)
                    result = Save();
            }

            return result;
        }

        private ActionResult SetTimeCodeTransactionsToDeleted(TimeBlock timeBlock, bool saveChanges = true)
        {
            if (timeBlock == null)
                return new ActionResult(true);

            var result = new ActionResult(true);

            if (CanEntityLoadReferences(entities, timeBlock) && !timeBlock.TimeCodeTransaction.IsLoaded)
                timeBlock.TimeCodeTransaction.Load();

            if (!timeBlock.TimeCodeTransaction.IsNullOrEmpty())
            {
                result = SetTimeCodeTransactionsToDeleted(timeBlock.TimeCodeTransaction.ToList(), saveChanges: false);
                if (result.Success && saveChanges)
                    result = Save();
            }

            return result;
        }
        private ActionResult SetTimeCodeTransactionsToDeleted(List<TimeCodeTransaction> timeCodeTransactions, bool saveChanges = true)
        {
            if (timeCodeTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeCodeTransaction in timeCodeTransactions)
            {
                result = SetTimeCodeTransactionToDeleted(timeCodeTransaction, saveChanges: false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeCodeTransactionToDeleted(TimeCodeTransaction timeCodeTransaction, bool saveChanges = true)
        {
            if (timeCodeTransaction == null || timeCodeTransaction.State == (int)SoeEntityState.Deleted || timeCodeTransaction.ReversedDate.HasValue)
                return new ActionResult(true);

            var result = ChangeEntityState(timeCodeTransaction, SoeEntityState.Deleted, GetUserFromCache());
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimeInvoiceTransactionsToDeleted(List<TimeInvoiceTransaction> timeInvoiceTransctions, bool saveChanges = true, bool discardCheckes = false)
        {
            if (timeInvoiceTransctions.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timeInvoiceTransction in timeInvoiceTransctions)
            {
                result = SetTimeInvoiceTransactionToDeleted(timeInvoiceTransction, saveChanges: false, discardCheckes: discardCheckes);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimeInvoiceTransactionToDeleted(TimeInvoiceTransaction timeInvoiceTransaction, bool saveChanges = true, bool discardCheckes = false)
        {
            if (timeInvoiceTransaction == null || timeInvoiceTransaction.State == (int)SoeEntityState.Deleted)
                return new ActionResult(true);

            if (!discardCheckes)
            {
                var attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
                if (attestStateInitialInvoice != null && timeInvoiceTransaction.AttestStateId != attestStateInitialInvoice.AttestStateId)
                    return new ActionResult((int)ActionResultSave.TimeInvoiceTransactionCannotDeleteNotInitialAttestState, GetText(5759, "Det finns attesterade transaktioner som inte kan tas bort"));
            }

            var result = ChangeEntityState(timeInvoiceTransaction, SoeEntityState.Deleted, GetUserFromCache(), discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetTimePayrollTransactionsToDeleted(List<TimePayrollTransaction> timePayrollTransactions, bool saveChanges = true, bool discardCheckes = false, bool discardPayrollStartValues = false, bool isAbsenceRecalculation = false)
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timePayrollTransaction in timePayrollTransactions)
            {
                result = SetTimePayrollTransactionToDeleted(timePayrollTransaction, false, discardCheckes, discardPayrollStartValues, isAbsenceRecalculation);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollTransactionToDeleted(TimePayrollTransaction timePayrollTransaction, bool saveChanges = true, bool discardCheckes = false, bool discardPayrollStartValues = false, bool isAbsenceRecalculation = false)
        {
            if (timePayrollTransaction == null || timePayrollTransaction.State == (int)SoeEntityState.Deleted || timePayrollTransaction.ReversedDate.HasValue)
                return new ActionResult(true);

            // Check AttestState payroll (cannot override check)
            int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
            if (timePayrollTransaction.AttestStateId.IsEqualToAny(payrollLockedAttestStateIds))
            {
                if (isAbsenceRecalculation)
                    return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteIsPayroll, GetText(10099, "Det finns transaktioner inom frånvarointervallet som är överförda till lön som inte kan tas bort") + ". " + GetText(11023, "De måste hanteras manuellt") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
                else
                    return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteIsPayroll, GetText(10005, "Det finns transaktioner som är överförda till lön som inte kan tas bort") + ". " + GetText(11023, "De måste hanteras manuellt") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
            }

            if (!discardCheckes)
            {
                var attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                if (attestStateInitialPayroll != null && timePayrollTransaction.AttestStateId != attestStateInitialPayroll.AttestStateId)
                {
                    if (isAbsenceRecalculation)
                        return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState, GetText(10100, "Det finns attesterade dagar inom frånvarointervallet. Ändra status till Registrerad och spara frånvaron på nytt") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
                    else
                        return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState, GetText(5759, "Det finns attesterade transaktioner som inte kan tas bort") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
                }
            }

            if (!discardPayrollStartValues && timePayrollTransaction.PayrollStartValueRowId.HasValue)
            {
                if (isAbsenceRecalculation)
                    return new ActionResult((int)ActionResultSave.EntityNotUpdated, GetText(10101, "Det finns transaktioner inom frånvarointervallet som innehåller startvärden för lön som inte kan tas bort") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
                else
                    return new ActionResult((int)ActionResultSave.EntityNotUpdated, GetText(8656, "Det finns transaktioner som innehåller startvärden för lön som inte kan tas bort") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
            }

            //Clear comment, because transaction can be reused
            timePayrollTransaction.Comment = String.Empty;

            ActionResult result = ChangeEntityState(timePayrollTransaction, SoeEntityState.Deleted, GetUserFromCache(), discardCheckes: discardCheckes);
            if (result.Success && saveChanges)
                result = Save();
            return result;
        }
        
        private ActionResult SetTimePayrollTransactionsForPayrollStartValuesToDeleted(List<int> payrollStartValueRowIds, bool saveChanges = true)
        {
            var attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            var timePayrollTransactions = GetTimePayrollTransactionsForPayrollStartValues(payrollStartValueRowIds);
            if (timePayrollTransactions.Any(i => i.AttestStateId != attestStateInitial.AttestStateId))
                return new ActionResult((int)ActionResultSave.Unknown, GetText(8631, "Kan inte ta bort transaktionerna. Det finns transaktioner som inte har lägsta attestnivå"));

            return SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges, true, true, false);
        }
        private ActionResult SetTimePayrollTransactionsForEmploymentTaxCreditToDeleted(List<int> timeBlockDateIds, int employeeId, bool saveChanges = true)
        {
            if (timeBlockDateIds.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (int timeBlockDateId in timeBlockDateIds)
            {
                result = SetTimePayrollTransactionsForEmploymentTaxCreditToDeleted(timeBlockDateId, employeeId, saveChanges: false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollTransactionsForEmploymentTaxCreditToDeleted(int timeBlockDateId, int employeeId, bool saveChanges = true)
        {
            var timePayrollTranscations = GetTimePayrollTransactionsForDay(employeeId, timeBlockDateId, TermGroup_SysPayrollType.SE_EmploymentTaxCredit);
            if (timePayrollTranscations.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var timePayrollTranscation in timePayrollTranscations)
            {
                result = ChangeEntityState(entities, timePayrollTranscation, SoeEntityState.Deleted, false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollTransactionsForRecalculateToDeleted(List<TimePayrollTransaction> timePayrollTransactions, bool saveChanges = true)
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            //Fixed, tax, netsalary
            ActionResult result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsFixed || i.IsTaxAndNotOptional() || i.IsNetSalaryPaid()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //SupplementCharge and Employmenttax
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsSupplementCharge() || i.IsEmploymentTax()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //Rounding
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsCentRounding || i.IsQuantityRounding).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //SalaryDistress
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistressAmount).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //VacationCompensation
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsVacationCompensationDirectPaid()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //UnionFee
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.UnionFeeId.HasValue).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //EmployeeVehicle 
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.EmployeeVehicleId.HasValue).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //Prepayment - invert
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsVacationAdditionOrSalaryPrepaymentInvert()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //Prepayment Variable- invert
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsVacationAdditionOrSalaryVariablePrepaymentInvert()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //Benefit - invert - level 3 not null
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsBenefitInvertWithLevel3NotNull()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            //WeekendSalary
            result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(i => i.IsWeekendSalary()).ToList(), saveChanges: false, discardCheckes: true);
            if (!result.Success)
                return result;

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollTransactionsForMassRegistrationToDeleted(int employeeId, int timePeriodId, int massRegistrationTemplateRowId, bool saveChanges = true)
        {
            AttestStateDTO initialAttestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (initialAttestState == null)
                return new ActionResult(true);

            var timePayrollTransactions = (from t in entities.TimePayrollTransaction
                                           where t.EmployeeId == employeeId &&
                                           t.TimePeriodId == timePeriodId &&
                                           t.MassRegistrationTemplateRowId == massRegistrationTemplateRowId &&
                                           t.State == (int)SoeEntityState.Active
                                           select t).ToList();

            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            return SetTimePayrollTransactionsToDeleted(timePayrollTransactions.Where(x => x.AttestStateId == initialAttestState.AttestStateId).ToList(), saveChanges: saveChanges);
        }

        private ActionResult SetTimePayrollScheduleTransactionsToDeleted(List<int> timeBlockDateIds, int employeeId, bool saveChanges = true, bool excludeEmploymentTaxAndSupplementCharge = true)
        {
            var result = new ActionResult(true);

            if (timeBlockDateIds.IsNullOrEmpty())
                return result;

            var scheduleTransactions = GetTimePayrollScheduleTransactions(employeeId, timeBlockDateIds, SoeTimePayrollScheduleTransactionType.Absence);
            if (scheduleTransactions.IsNullOrEmpty())
                return result;

            result = SetTimePayrollScheduleTransactionsToDeleted(scheduleTransactions, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: excludeEmploymentTaxAndSupplementCharge);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollScheduleTransactionsToDeleted(int timeBlockDateId, int employeeId, bool saveChanges = true, bool excludeEmploymentTaxAndSupplementCharge = true)
        {
            if (timeBlockDateId == 0)
                return new ActionResult(true);

            var result = new ActionResult(true);

            var scheduleTransactions = GetTimePayrollScheduleTransactions(employeeId, timeBlockDateId, SoeTimePayrollScheduleTransactionType.Absence);
            if (scheduleTransactions.IsNullOrEmpty())
                return result;

            result = SetTimePayrollScheduleTransactionsToDeleted(scheduleTransactions, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: excludeEmploymentTaxAndSupplementCharge);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollScheduleTransactionsToDeleted(List<TimePayrollScheduleTransaction> scheduleTransactions, bool saveChanges = true, bool excludeEmploymentTaxAndSupplementCharge = true)
        {
            if (scheduleTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            var result = new ActionResult(true);

            foreach (var scheduleTransaction in scheduleTransactions)
            {
                result = SetTimePayrollScheduleTransactionToDeleted(scheduleTransaction, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: excludeEmploymentTaxAndSupplementCharge);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }
        private ActionResult SetTimePayrollScheduleTransactionToDeleted(TimePayrollScheduleTransaction timePayrollScheduleTransaction, bool saveChanges = true, bool excludeEmploymentTaxAndSupplementCharge = true)
        {
            if (timePayrollScheduleTransaction == null || timePayrollScheduleTransaction.State == (int)SoeEntityState.Deleted)
                return new ActionResult(true);
            if (timePayrollScheduleTransaction.ReversedDate.HasValue)
                return new ActionResult(true);
            if (excludeEmploymentTaxAndSupplementCharge && (timePayrollScheduleTransaction.IsEmploymentTax() || timePayrollScheduleTransaction.IsSupplementCharge()))
                return new ActionResult(true);

            var result = ChangeEntityState(entities, timePayrollScheduleTransaction, SoeEntityState.Deleted, false, user: GetUserFromCache());
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetRetroPayrollEmployeeToDeleted(
            RetroactivePayrollEmployee retroEmployee,
            IEnumerable<RetroactivePayrollOutcome> retroOutcomes = null,
            bool doDeleteEmployee = false,
            bool doDeleteOutcome = false,
            bool doDeleteBasis = false, bool
            doDeleteTransactions = false, bool
            saveChanges = false,
            TermGroup_SoeRetroactivePayrollEmployeeStatus retroEmployeeStatusAfter = TermGroup_SoeRetroactivePayrollEmployeeStatus.Saved)
        {
            if (retroEmployee == null)
                return new ActionResult(true);

            var result = SetRetroPayrollOutcomesToDeleted(retroOutcomes ?? retroEmployee.RetroactivePayrollOutcome, doDeleteOutcome, doDeleteBasis, doDeleteTransactions, saveChanges: false);
            if (!result.Success)
                return result;

            result = SetRetroEmployeeToDeleted(retroEmployee, doDeleteEmployee, retroEmployeeStatusAfter);
            if (!result.Success)
                return result;

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetRetroEmployeeToDeleted(RetroactivePayrollEmployee retroEmployee, bool doDeleteEmployee, TermGroup_SoeRetroactivePayrollEmployeeStatus retroEmployeeStatusAfter)
        {
            if (doDeleteEmployee)
            {
                return ChangeEntityState(entities, retroEmployee, SoeEntityState.Deleted, false, GetUserFromCache());
            }
            else
            {
                retroEmployee.Status = (int)retroEmployeeStatusAfter;
                SetModifiedProperties(retroEmployee);
                return new ActionResult(true);
            }
        }

        private ActionResult SetRetroPayrollOutcomesToDeleted(IEnumerable<RetroactivePayrollOutcome> retroOutcomes, bool doDeleteOutcome = true, bool doDeleteBasis = false, bool doDeleteTransactions = false, bool saveChanges = false)
        {
            var result = new ActionResult(true);

            if (!doDeleteOutcome && !doDeleteBasis && !doDeleteTransactions)
                return result;

            foreach (RetroactivePayrollOutcome retroOutcome in retroOutcomes.Where(i => i.State == (int)SoeEntityState.Active))
            {
                #region RetroactivePayrollOutcome

                if (doDeleteTransactions || doDeleteBasis)
                {
                    // Check AttestState payroll (cannot override check)
                    int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
                    foreach (var timePayrollTransaction in retroOutcome.TimePayrollTransaction)
                    {
                        if (timePayrollTransaction.AttestStateId.IsEqualToAny(payrollLockedAttestStateIds))
                            return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteIsPayroll, GetText(10005, "Det finns transaktioner som är överförda till lön som inte kan tas bort") + ". " + GetText(11023, "De måste hanteras manuellt") + (timePayrollTransaction.TimeBlockDate != null ? String.Format(" ({0})", timePayrollTransaction.TimeBlockDate.Date.ToShortDateString()) : ""));
                    }

                    #region RetroactivePayrollBasis

                    if (doDeleteTransactions || doDeleteBasis)
                    {
                        if (!retroOutcome.RetroactivePayrollBasis.IsLoaded)
                            retroOutcome.RetroactivePayrollBasis.Load();

                        foreach (var retroBasis in retroOutcome.RetroactivePayrollBasis.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            if (doDeleteTransactions && retroBasis.RetroTimePayrollTransaction != null)
                            {
                                result = ChangeEntityState(entities, retroBasis.RetroTimePayrollTransaction, SoeEntityState.Deleted, false, GetUserFromCache());
                                if (!result.Success)
                                    return result;

                                retroBasis.RetroTimePayrollTransaction = null;
                            }

                            if (doDeleteBasis)
                            {
                                result = ChangeEntityState(entities, retroBasis, SoeEntityState.Deleted, false, GetUserFromCache());
                                if (!result.Success)
                                    return result;
                            }
                        }
                    }

                    if (doDeleteTransactions && retroOutcome.TimePayrollTransaction != null)
                        retroOutcome.TimePayrollTransaction.Clear();

                    #endregion

                    #region RetroactivePayrollScheduleBasis

                    if (doDeleteTransactions || doDeleteBasis)
                    {
                        if (!retroOutcome.RetroactivePayrollScheduleBasis.IsLoaded)
                            retroOutcome.RetroactivePayrollScheduleBasis.Load();

                        foreach (RetroactivePayrollScheduleBasis retroBasis in retroOutcome.RetroactivePayrollScheduleBasis.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            #region RetroactivePayrollBasis

                            if (doDeleteTransactions && retroBasis.RetroTimePayrollScheduleTransaction != null)
                            {
                                result = ChangeEntityState(entities, retroBasis.RetroTimePayrollScheduleTransaction, SoeEntityState.Deleted, false);
                                if (!result.Success)
                                    return result;

                                retroBasis.RetroTimePayrollScheduleTransaction = null;
                            }

                            if (doDeleteBasis)
                            {
                                result = ChangeEntityState(entities, retroBasis, SoeEntityState.Deleted, false);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion
                        }
                    }

                    if (doDeleteTransactions && retroOutcome.TimePayrollScheduleTransaction != null)
                        retroOutcome.TimePayrollScheduleTransaction.Clear();

                    #endregion
                }
                if (doDeleteOutcome)
                {
                    result = ChangeEntityState(entities, retroOutcome, SoeEntityState.Deleted, false);
                    if (!result.Success)
                        return result;
                }

                #endregion
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetEmployeeTimePeriodToDeleted(EmployeeTimePeriod employeeTimePeriod, bool saveChanges = true)
        {
            if (employeeTimePeriod == null)
                return new ActionResult(true);

            if (employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                return new ActionResult((int)ActionResultSave.Locked, GetText(110672, "Löneperioden är låst och kan inte tas bort"));

            ActionResult result;

            if (!employeeTimePeriod.EmployeeTimePeriodValue.IsNullOrEmpty())
            {
                foreach (var value in employeeTimePeriod.EmployeeTimePeriodValue.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    result = ChangeEntityState(value, SoeEntityState.Deleted, GetUserFromCache(), discardCheckes: true);
                    if (!result.Success)
                        return result;
                }
            }

            result = ChangeEntityState(employeeTimePeriod, SoeEntityState.Deleted, GetUserFromCache(), discardCheckes: true);
            if (result.Success && saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetPayrollSlipsToDeleted(CompEntities entities, TimePeriod timePeriod, int employeeId, bool saveChanges = true)
        {
            var result = new ActionResult(true);

            var user = GetUserFromCache();

            var dataStorages = GeneralManager.GetDataStorages(entities, SoeDataStorageRecordType.PayrollSlipXML, timePeriod.TimePeriodId, employeeId, this.actorCompanyId);
            foreach (var dataStorage in dataStorages)
            {
                result = ChangeEntityState(entities, dataStorage, SoeEntityState.Deleted, false, user: user);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        #endregion
    }
}
