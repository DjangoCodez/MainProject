using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.UI;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Restores TimeBlock's and transactions according to schedule
        /// </summary>
        /// <returns>Output DTO</returns>
        private RestoreDaysToScheduleOutputDTO TaskRestoreDaysToSchedule()
        {
            var (iDTO, oDTO) = InitTask<RestoreDaysToScheduleInputDTO, RestoreDaysToScheduleOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultDelete.InsufficientInput);
                return oDTO;
            }
            if (iDTO.Items.IsNullOrEmpty())
                return oDTO;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = RestoreDaysToSchedule(iDTO.Items);

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
        private RestoreDaysToScheduleDiscardDeviationsOutputDTO TaskRestoreDaysToScheduleDiscardDeviations()
        {
            var (iDTO, oDTO) = InitTask<RestoreDaysToScheduleInputDTO, RestoreDaysToScheduleDiscardDeviationsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultDelete.InsufficientInput);
                return oDTO;
            }
            if (iDTO.Items.IsNullOrEmpty())
                return oDTO;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = RestoreDaysToScheduleDiscardDeviations(iDTO.Items);

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
        /// Restore TimeScheduleTemplateBlock's to template schedule
        /// </summary>
        /// <returns>Output DTO</returns>
        private RestoreDaysToTemplateScheduleOutputDTO TaskRestoreDaysToTemplateSchedule()
        {
            var (iDTO, oDTO) = InitTask<RestoreDaysToTemplateScheduleInputDTO, RestoreDaysToTemplateScheduleOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull);
                return oDTO;
            }
            if (iDTO.Items.IsNullOrEmpty())
                return oDTO;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = RestoreDaysToScheduleTemplate(iDTO.Items, out List<TimeBlockDate> autogenTimeBlockDates, out List<TimeBlockDate> stampingTimeBlockDates);
                        if (oDTO.Result.Success)
                        {
                            oDTO.AutogenTimeBlockDateIds.AddRange(autogenTimeBlockDates.GetTimeBlockDateIds());
                            oDTO.StampingTimeBlockDateIds.AddRange(stampingTimeBlockDates.GetTimeBlockDateIds());

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
        /// Regenerates day for Employees. Validates for each day and employee if it should regenerate transactions (EmployeeGroup.Autogen = 1) or regenerate from timestamps (EmployeeGroup.Autogen = 0)
        /// </summary>
        /// <returns></returns>
        private ReGenerateTransactionsDiscardAttestOutputDTO TaskReGenerateTransactionsDiscardAttest()
        {
            var (iDTO, oDTO) = InitTask<ReGenerateTransactionsDiscardAttestInputDTO, ReGenerateTransactionsDiscardAttestOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null)
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

                        Company company = GetCompanyFromCache();
                        if (company == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "Company");
                            return oDTO;
                        }

                        List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
                        int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();

                        #endregion

                        #region Check Vacation --> LeaveOfAbsence

                        if (iDTO.VacationResetLeaveOfAbsence && iDTO.LimitStartDate.HasValue && iDTO.LimitStopDate.HasValue)
                        {
                            List<TimePayrollTransaction> timePayrolTransctions = GetTimePayrollTransactionsForCompanyWithVacationThatResultedInLeaveOfAbsence(iDTO.LimitStartDate.Value, iDTO.LimitStopDate.Value);
                            timePayrolTransctions = timePayrolTransctions.Where(tpt => !payrollLockedAttestStateIds.Contains(tpt.AttestStateId)).ToList();
                            if (!timePayrolTransctions.Any())
                            {
                                oDTO.Logs.Add(String.Format("Inga transaktioner att hantera för företag {0}.{1}", company.ActorCompanyId, company.Name));
                                oDTO.Result = new ActionResult(true);
                                return oDTO;
                            }

                            var timePayrollTransactionsGroupedByEmployee = timePayrolTransctions.GroupBy(i => i.EmployeeId).ToList();
                            foreach (var transactionsGroupedByEmployee in timePayrollTransactionsGroupedByEmployee)
                            {
                                Employee employee = GetEmployeeFromCache(transactionsGroupedByEmployee.Key);
                                if (employee == null)
                                {
                                    oDTO.Logs.Add(String.Format("Kunde ej hitta anställd med EmployeeId={0} på företag {1}.{2}", transactionsGroupedByEmployee.Key, company.ActorCompanyId, company.Name));
                                    continue;
                                }

                                foreach (TimePayrollTransaction timePayrollTransaction in transactionsGroupedByEmployee)
                                {
                                    if (timePayrollTransaction.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence)
                                    {
                                        timePayrollTransaction.SysPayrollTypeLevel3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;
                                        oDTO.Logs.Add(String.Format("Ändrar transaktion {0} för anställd {1} på företag {2}.{3}", timePayrollTransaction.TimePayrollTransactionId, employee.EmployeeNr, company.ActorCompanyId, company.Name));
                                    }
                                }

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                {
                                    oDTO.Logs.Add(String.Format("Ändring av tjänstledig-transaktioner misslyckades {0}", oDTO.Result.ErrorMessage));
                                    return oDTO;
                                }

                                var input = GetAttestEmployeeInput.CreateAttestInputForWeb(actorCompanyId, userId, 0, employee.EmployeeId, iDTO.LimitStartDate.Value, iDTO.LimitStopDate.Value);
                                List<AttestEmployeeDayDTO> dayItems = TimeTreeAttestManager.GetAttestEmployeeDays(entities, input);
                                foreach (AttestEmployeeDayDTO item in dayItems)
                                {
                                    iDTO.Items.Add(new AttestEmployeeDaySmallDTO(item.EmployeeId, item.Date, item.TimeBlockDateId, item.TimeScheduleTemplatePeriodId));
                                }
                            }
                        }

                        #endregion

                        #region Check Vacation --> 30000

                        if (iDTO.VacationReset30000 && iDTO.LimitStartDate.HasValue && iDTO.LimitStopDate.HasValue)
                        {
                            List<TimePayrollTransaction> timePayrolTransctions = GetTimePayrollTransactionsForCompanyWithVacation3000(iDTO.LimitStartDate.Value, iDTO.LimitStopDate.Value);
                            if (timePayrolTransctions.Count > 0)
                                timePayrolTransctions = timePayrolTransctions.Where(tpt => !payrollLockedAttestStateIds.Contains(tpt.AttestStateId)).ToList();
                            if (timePayrolTransctions.Count == 0)
                            {
                                oDTO.Logs.Add(String.Format("Inga transaktioner att hantera för företag {0}.{1}", company.ActorCompanyId, company.Name));
                                oDTO.Result = new ActionResult(true);
                                return oDTO;
                            }

                            foreach (var transactionsGroupedByEmployee in timePayrolTransctions.GroupBy(i => i.EmployeeId).ToList())
                            {
                                Employee employee = GetEmployeeFromCache(transactionsGroupedByEmployee.Key);
                                if (employee == null)
                                {
                                    oDTO.Logs.Add(String.Format("Kunde ej hitta anställd med EmployeeId={0} på företag {1}.{2}", transactionsGroupedByEmployee.Key, company.ActorCompanyId, company.Name));
                                    continue;
                                }

                                var input = GetAttestEmployeeInput.CreateAttestInputForWeb(actorCompanyId, userId, 0, employee.EmployeeId, iDTO.LimitStartDate.Value, iDTO.LimitStopDate.Value);

                                List<AttestEmployeeDayDTO> dayItems = TimeTreeAttestManager.GetAttestEmployeeDays(entities, input);
                                foreach (AttestEmployeeDayDTO item in dayItems)
                                {
                                    iDTO.Items.Add(new AttestEmployeeDaySmallDTO(item.EmployeeId, item.Date, item.TimeBlockDateId, item.TimeScheduleTemplatePeriodId));
                                }
                            }
                        }

                        #endregion

                        foreach (var daysByEmployee in iDTO.Items.GroupBy(i => i.EmployeeId))
                        {
                            #region Employee

                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(daysByEmployee.Key);
                            if (employee == null)
                                continue;

                            List<AttestEmployeeDaySmallDTO> daysForEmployee = new List<AttestEmployeeDaySmallDTO>();
                            DateTime dateFrom = daysByEmployee.OrderBy(i => i.Date).Select(i => i.Date).FirstOrDefault();
                            DateTime dateTo = daysByEmployee.OrderByDescending(i => i.Date).Select(i => i.Date).FirstOrDefault();

                            if (iDTO.VacationOnly)
                            {
                                List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, dateFrom, dateTo, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation);
                                foreach (var timePayrollTransactionsGroupedByDay in timePayrollTransactions.GroupBy(i => i.TimeBlockDate.Date))
                                {
                                    AttestEmployeeDaySmallDTO day = daysByEmployee.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId && i.Date == timePayrollTransactionsGroupedByDay.Key);
                                    if (day != null)
                                        daysForEmployee.Add(day);
                                }
                            }
                            else
                            {
                                daysForEmployee.AddRange(daysByEmployee.ToList());
                            }

                            oDTO.StampingDates = GetEmployeeStampingDates(employee, dateFrom, dateTo, employeeGroups);

                            List<AttestEmployeeDaySmallDTO> autogenDaysForEmployee = daysForEmployee.Where(d => !oDTO.StampingDates.Contains(d.Date)).ToList();
                            if (autogenDaysForEmployee.Any())
                            {
                                oDTO.Logs.Add(String.Format("Räknar om anställd {0} för företag {1}.{2}", employee.EmployeeNr, company.ActorCompanyId, company.Name));
                                oDTO.Result = ReCalculateTransactionsDiscardAttest(autogenDaysForEmployee.OrderBy(i => i.Date).ToList(), doNotRecalculateAmounts: iDTO.DoNotRecalculateAmounts);
                            }

                            #endregion
                        }

                        DoInitiatePayrollWarnings();
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
                    oDTO.Result.StrDict = new Dictionary<int, string>();
                    int counter = 1;
                    foreach (string oLog in oDTO.Logs)
                    {
                        oDTO.Result.StrDict.Add(counter, oLog);
                        counter++;
                    }

                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        /// <summary>
        /// Deletes TimeBlocks and transaction
        /// </summary>
        /// <returns>Output DTO</returns>
        private CleanDaysOutputDTO TaskCleanDays()
        {
            var (iDTO, oDTO) = InitTask<CleanDaysInputDTO, CleanDaysOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultDelete.InsufficientInput);
                return oDTO;
            }
            if (iDTO.Items.IsNullOrEmpty())
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = CleanDays(iDTO.Items);
                        if (oDTO.Result.Success)
                            DoInitiatePayrollWarnings();

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
        /// Saves TimeCodeTransactions from addition and deduction
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveTimeCodeTransactionsOutputDTO TaskSaveTimeCodeTransactions()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeCodeTransactionsInputDTO, SaveTimeCodeTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if ((iDTO == null) || iDTO.TimeCodeTransactionsInput.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultDelete.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveTimeCodeTransactions(iDTO.TimeCodeTransactionsInput);

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

        #region Recalculation Trackers

        private enum ReCalculateRelatedDaysOption
        {
            Apply,
            Restore,
            ApplyAndRestore
        }

        private OvertimeTracker<OvertimeDay> overtimeTracker = null;
        private ApplyAbsenceTracker<ApplyAbsenceDay> applyAbsenceTracker = null;
        private RestoreAbsenceTracker<RestoreAbsenceDay> restoreAbsenceTracker = null;

        #region Overtime tracker

        private void InitOvertimeTracker()
        {
            if (this.overtimeTracker == null)
                this.overtimeTracker = new OvertimeTracker<OvertimeDay>();
        }
        private void CloseOvertimeTracker()
        {
            this.overtimeTracker = null;
        }

        private OvertimeTracker<OvertimeDay> GetOvertimeTracker() => this.overtimeTracker;
        private ReCalculationTrackerDetails<OvertimeTracker<OvertimeDay>, OvertimeDay> GetOvertimeTrackerDetails()
        {
            var tracker = GetOvertimeTracker();
            var days = tracker?.Days ?? new List<OvertimeDay>();
            var hasOvertimeDaysCalculated = HasOvertimeDaysCalculated(days);

            return new ReCalculationTrackerDetails<OvertimeTracker<OvertimeDay>, OvertimeDay>(tracker, days, hasOvertimeDaysCalculated);
        }

        private void TryAddDayToOvertimeTracker(DateTime date, List<TimeBlock> timeBlocksForDay)
        {
            if (timeBlocksForDay.ContainsTimeDeviationCause(GetTimeDeviationCausesFromCache().GetOvertimeDeviationCauseIds()))
                AddDayToOvertimeTracker(date);
        }
        private void AddDayToOvertimeTracker(DateTime date)
        {
            if (DoNotCollectDaysForRecalculation())
                return;

            InitOvertimeTracker();

            if (!this.overtimeTracker.HasDay(date))
                this.overtimeTracker.AddDay(new OvertimeDay(date));
        }

        #endregion

        #region Apply absence tracker

        private ApplyAbsenceTracker<ApplyAbsenceDay> GetApplyAbsenceTracker() => this.applyAbsenceTracker;
        private ReCalculationTrackerDetails<ApplyAbsenceTracker<ApplyAbsenceDay>, ApplyAbsenceDay> GetApplyAbsenceTrackerDetails(int employeeId)
        {
            var tracker = GetApplyAbsenceTracker();
            var days = tracker?.Days ?? new List<ApplyAbsenceDay>();
            var hasAbsenceDaysCalculated = HasAbsenceDaysCalculated(days, employeeId);

            return new ReCalculationTrackerDetails<ApplyAbsenceTracker<ApplyAbsenceDay>, ApplyAbsenceDay>(tracker, days, hasAbsenceDaysCalculated);
        }
        private List<ApplyAbsenceDay> GetDaysFromApplyAbsenceTracker()
        {
            return applyAbsenceTracker?.Days ?? new List<ApplyAbsenceDay>();
        }

        private void InitApplyAbsenceTracker()
        {
            if (this.applyAbsenceTracker == null)
                this.applyAbsenceTracker = new ApplyAbsenceTracker<ApplyAbsenceDay>();
        }
        private void CloseApplyAbsenceTracker()
        {
            this.applyAbsenceTracker = null;
        }
        private void AddOrUpdateDaysToApplyAbsenceTracker(List<ApplyAbsenceDTO> applyAbsenceItems, TimeBlockDate timeBlockDate)
        {
            if (applyAbsenceItems.IsNullOrEmpty() || timeBlockDate == null)
                return;

            foreach (var applyAbsenceItem in applyAbsenceItems)
            {
                ApplyAbsenceDay absenceDay = new ApplyAbsenceDay(timeBlockDate.Date, applyAbsenceItem.NewProductId, applyAbsenceItem.SysPayrollTypeLevel3, applyAbsenceItem.IsVacation);

                foreach (int timePayrollTransactionId in applyAbsenceItem.TimePayrollTransactionIdsToRecalculate)
                {
                    TimePayrollTransaction timePayrollTransaction = GetTimePayrollTransactionWithTimeBlockDateAndExtended(timePayrollTransactionId);
                    if (timePayrollTransaction != null)
                    {
                        if (applyAbsenceItem.NewProductId.HasValue)
                            timePayrollTransaction.ProductId = applyAbsenceItem.NewProductId.Value;
                        absenceDay.TimePayrollTransactionsToRecalculate.Add(timePayrollTransaction);
                    }
                }

                AddOrUpdateDaysToApplyAbsenceTracker(absenceDay);
            }
        }
        private void AddOrUpdateDaysToApplyAbsenceTracker(ApplyAbsenceDay day)
        {
            InitApplyAbsenceTracker();
            applyAbsenceTracker.AddOrUpdate(day);
        }
        private void TryAddAbsenceDayIfNotExistsToApplyAbsenceTracker(ApplyAbsenceDay day)
        {
            InitApplyAbsenceTracker();
            applyAbsenceTracker.AddDay(day);
        }

        #endregion

        #region Restore days tracker

        private void InitRestoreAbsenceTracker()
        {
            if (this.restoreAbsenceTracker == null)
                this.restoreAbsenceTracker = new RestoreAbsenceTracker<RestoreAbsenceDay>();
        }
        private void CloseRestoreAbsenceTracker()
        {
            this.restoreAbsenceTracker = null;
        }

        private RestoreAbsenceTracker<RestoreAbsenceDay> GetRestoreAbsenceTracker() => this.restoreAbsenceTracker;
        private ReCalculationTrackerDetails<RestoreAbsenceTracker<RestoreAbsenceDay>, RestoreAbsenceDay> GetRestoreAbsenceTrackerDetails(int employeeId)
        {
            var tracker = GetRestoreAbsenceTracker();
            var days = tracker?.Days ?? new List<RestoreAbsenceDay>();
            var hasAbsenceDaysCalculated = HasAbsenceDaysCalculated(days, employeeId);

            return new ReCalculationTrackerDetails<RestoreAbsenceTracker<RestoreAbsenceDay>, RestoreAbsenceDay>(tracker, days, hasAbsenceDaysCalculated);
        }

        private void AddDaysToRestoreAbsenceTracker(List<TimeEngineDay> days, List<TimeBlock> timeBlocks, int employeeId)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            foreach (var timeBlocksByTimeBlockDateId in timeBlocks.GroupBy(i => i.TimeBlockDateId))
            {
                TimeEngineDay day = days.FirstOrDefault(i => i.TimeBlockDateId == timeBlocksByTimeBlockDateId.Key);
                if (day != null)
                    AddDayToRestoreAbsenceTracker(day.Date, timeBlocksByTimeBlockDateId.ToList(), employeeId);
            }
        }
        private void AddDayToRestoreAbsenceTracker(DateTime date, List<TimeBlock> timeBlocks, int employeeId)
        {
            timeBlocks = timeBlocks?.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            if (timeBlocks.IsNullOrEmpty())
                return;

            TryAddDayToAbsenceRestoreTracker(date, employeeId, timeBlocks);
            TryAddDayToOvertimeTracker(date, timeBlocks);
        }
        private void AddDaysToRestoreDayTrackerBasedOnDeviationCauseChanged(DateTime date, int employeeId, List<TimeBlock> timeBlocksForDayPrev, List<TimeBlock> timeBlocksForDayCurrent, List<TimeDeviationCause> timeDeviationCauses)
        {
            if (DoNotCollectDaysForRecalculation())
                return;

            List<int> timeDeviationCauseIdsAbsence = timeDeviationCauses.GetAbsenceDeviationCauseIds();
            bool isAbsenceDeviationCauseRestored = timeBlocksForDayPrev.ContainsTimeDeviationCause(timeDeviationCauseIdsAbsence) && !timeBlocksForDayCurrent.ContainsTimeDeviationCause(timeDeviationCauseIdsAbsence);
            if (isAbsenceDeviationCauseRestored)
                TryAddDayToAbsenceRestoreTracker(date, employeeId, timeBlocksForDayPrev);

            List<int> timeDeviationCauseIdsOvertime = timeDeviationCauses.GetOvertimeDeviationCauseIds();
            bool isOvertimeDeviationCauseRestored = timeBlocksForDayPrev.ContainsTimeDeviationCause(timeDeviationCauseIdsOvertime) && !timeBlocksForDayCurrent.ContainsTimeDeviationCause(timeDeviationCauseIdsOvertime);
            if (isOvertimeDeviationCauseRestored)
                AddDayToOvertimeTracker(date);
        }

        private void TryAddDayToAbsenceRestoreTracker(DateTime date, int employeeId, List<TimeBlock> timeBlocksForDay)
        {
            if (!timeBlocksForDay.TryGetAbsenceSysPayrollTypeLevel3s(out List<int> sysPayrollTypeLevel3s))
                return;

            bool isVacationFiveDaysPerWeek = timeBlocksForDay.ContainsVacationFiveDaysPerWeek();
            AddDayToAbsenceRestoreTracker(date, sysPayrollTypeLevel3s, isVacationFiveDaysPerWeek);

            if (isVacationFiveDaysPerWeek)
            {
                List<TimeBlock> timeBlockForWeek = GetTimeBlocksWithDateAndTimePayrollTransaction(employeeId, CalendarUtility.GetFirstDateOfWeek(date), CalendarUtility.GetLastDateOfWeek(date));
                foreach (var timeBlockForWeekBydate in timeBlockForWeek.GroupBy(i => i.TimeBlockDate.Date))
                {
                    AddDayToAbsenceRestoreTracker(timeBlockForWeekBydate.Key, sysPayrollTypeLevel3s, true);
                }
            }
        }
        private void AddDayToAbsenceRestoreTracker(DateTime date, List<int> sysPayrollTypeLevel3s, bool isVacationFiveDaysPerWeek)
        {
            if (DoNotCollectDaysForRecalculation())
                return;

            InitRestoreAbsenceTracker();

            if (!this.restoreAbsenceTracker.HasDay(date))
                this.restoreAbsenceTracker.AddDay(new RestoreAbsenceDay(date, sysPayrollTypeLevel3s, isVacationFiveDaysPerWeek));
        }

        #endregion

        #endregion

        #region Recalculate

        private bool HasOvertimeDaysCalculated<T>(List<T> overtimeDays) where T : OvertimeDay
        {
            if (overtimeDays.IsNullOrEmpty())
                return false;

            return true;
        }

        private bool HasAbsenceDaysCalculated<T>(List<T> absenceDays, int employeeId) where T : AbsenceDayBase
        {
            if (absenceDays.IsNullOrEmpty())
                return false;

            if (!UsePayroll())
            {
                //Only recalculate sick if not use payroll
                if (!absenceDays.Any(d => d.ContainsLeve3(TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick)))
                    return false;

                //Must have sick iwh rules, otherwise recaculate not necessary
                if (!HasEmployeeSickDuringIwhOrStandyRules(absenceDays.Select(i => i.Date).Distinct().ToList(), employeeId))
                    return false;
            }

            return true;
        }

        private bool TrySkipRecalculateDay(AttestEmployeeDaySmallDTO day, List<TimePayrollTransaction> timePayrollTransactionsForDay, bool skipDaysInSameAbsenceRuleRow)
        {
            if (skipDaysInSameAbsenceRuleRow && !day.IsDayForward)
            {
                var absenceRepository = GetApplyAbsenceTracker();
                if (absenceRepository != null)
                {
                    //Check if previous day was absence (exclude vacation)
                    ApplyAbsenceDay prevAbsenceDay = absenceRepository.GetDay(day.Date.AddDays(-1));
                    if (prevAbsenceDay != null && prevAbsenceDay.SysPayrollTypeLevel3 != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation)
                    {
                        //Check if day is still in same AbsenceRuleRow interval
                        bool isValidTimeAbsenceRuleRow = prevAbsenceDay.TimeAbsenceRuleRow?.PayrollProductId != null && prevAbsenceDay.AbsenceDayNumber > 0 && prevAbsenceDay.AbsenceDayNumber + 1 <= prevAbsenceDay.TimeAbsenceRuleRow.Stop;
                        if (isValidTimeAbsenceRuleRow)
                        {
                            //Check that current day has transactions according to AbsenceRuleRow
                            List<TimePayrollTransaction> timePayrollTransactionsDayAndForLevel3 = timePayrollTransactionsForDay.Where(i => i.SysPayrollTypeLevel3 == prevAbsenceDay.SysPayrollTypeLevel3 && i.ProductId == prevAbsenceDay.TimeAbsenceRuleRow.PayrollProductId.Value).ToList();
                            if (timePayrollTransactionsDayAndForLevel3.Any())
                            {
                                PayrollProduct payrollProduct = GetPayrollProductFromCache(prevAbsenceDay.TimeAbsenceRuleRow.PayrollProductId.Value);
                                if (payrollProduct != null)
                                {
                                    ApplyAbsenceDay absenceDay = new ApplyAbsenceDay(day.Date, payrollProduct);
                                    absenceDay.Update(
                                        prevAbsenceDay.TimeAbsenceRule,
                                        prevAbsenceDay.TimeAbsenceRuleRow,
                                        absenceDayNumber: (payrollProduct.IsLeaveOfAbsence() || payrollProduct.IsAbsenceParentalLeaveOrTemporaryParentalLeave()) ? prevAbsenceDay.AbsenceDayNumber : prevAbsenceDay.AbsenceDayNumber + 1);

                                    absenceRepository.AddDay(absenceDay);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private int? PreserveTransactionsAttestStateId(List<TimePayrollTransaction> timePayrollTransactionsForDay, int attestStateInitialId)
        {
            if (timePayrollTransactionsForDay.IsNullOrEmpty())
                return null;

            int? attestStateId = null;

            List<int> transactionAttestStateIds = timePayrollTransactionsForDay.GetAttestStateIds();
            if (transactionAttestStateIds.Count == 1)
            {
                attestStateId = transactionAttestStateIds.First();
            }
            else if (transactionAttestStateIds.Count > 1)
            {
                if (transactionAttestStateIds.Any(id => id == attestStateInitialId))
                    attestStateId = attestStateInitialId;
                else
                    attestStateId = GetAttestStatesFromCache(transactionAttestStateIds.ToArray()).OrderBy(i => i.Sort).ThenBy(i => i.AttestStateId).FirstOrDefault()?.AttestStateId;
            }

            if (attestStateId != attestStateInitialId)
            {
                timePayrollTransactionsForDay.ForEach(tpt => tpt.AttestStateId = attestStateInitialId);
                if (!Save().Success)
                    attestStateId = null;
            }
            else
                attestStateId = null;

            return attestStateId;
        }
        
        private ActionResult ReCalculateDayFromSchedule(int? templatePeriodId, List<TimeScheduleTemplateBlock> templateBlocks, TimeBlockDate timeBlockDate, Employee employee, bool discardCheckes = false)
        {
            ActionResult result;

            #region Prereq

            if (templateBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, timeBlockDate.Date.ToShortDateString()));

            if (templatePeriodId <= 0)
                templatePeriodId = GetTimeScheduleTemplatePeriodIdFromCache(employee.EmployeeId, timeBlockDate.Date) ?? 0;

            #endregion

            #region Perform

            if (employeeGroup.AutogenTimeblocks)
            {
                if (IsManuallyAdjusted(employee.EmployeeId, timeBlockDate.TimeBlockDateId, templatePeriodId))
                {
                    //Only re-calculate day
                    result = SaveTransactionsForPeriod(timeBlockDate, employee, templatePeriodId);
                    if (!result.Success)
                        return result;
                }
                else
                {
                    //Delete TimeBlock's and transactions, and re-create day from schedule (like a placement)
                    result = SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(timeBlockDate, employee.EmployeeId, saveChanges: true, discardCheckes: discardCheckes);
                    if (result.Success)
                    {
                        result = SaveTimeBlocksAndTransactionsFromTemplate(templateBlocks);
                        if (!result.Success)
                            return result;
                    }
                }
            }
            else
            {
                //Handled later by another call to ReGenerateDaysForEmployeeBasedOnTimeStamps
                result = Save();
            }

            #endregion

            return result;
        }

        private ActionResult ReCalculateTransactions(List<AttestEmployeeDaySmallDTO> days, bool discardAttesteState = false, bool skipDaysInSameAbsenceRuleRow = false)
        {
            #region Prereq

            if (days.IsNullOrEmpty())
                return new ActionResult(true);

            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
            List<AttestStateDTO> payrollLockedAttestStates = GetAttestStatesFromCache(payrollLockedAttestStateIds);

            #endregion

            #region ReCalculate

            ActionResult result = new ActionResult(true);

            foreach (var daysByEmployee in days.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(daysByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

                DateTime dateFrom = days.Min(i => i.Date);
                DateTime dateTo = days.Max(i => i.Date);

                List<TimeBlockDate> timeBlockDates = null;
                if (daysByEmployee.All(i => i.TimeBlockDateId > 0))
                    timeBlockDates = GetTimeBlockDatesFromCache(employee.EmployeeId, daysByEmployee.Select(i => i.TimeBlockDateId));
                else
                    timeBlockDates = GetTimeBlockDatesFromCache(employee.EmployeeId, dateFrom, dateTo);

                List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, dateFrom, dateTo, includeStandBy: true);
                Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksByDay = scheduleBlocks.GroupBy(i => i.Date).ToDictionary(k => k.Key.Value, v => v.ToList());
                List<TimeBlock> timeBlocks = GetTimeBlocksWithTimeCodeAndTransactions(employee.EmployeeId, timeBlockDates.Select(i => i.TimeBlockDateId).ToList());
                Dictionary<int, List<TimeBlock>> timeBlocksByDay = timeBlocks.GroupBy(i => i.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                List<TimePayrollTransaction> timePayrollTransactionsForEmployee = timeBlocks.GetTimePayrollTransactions();
                Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsByDay = timePayrollTransactionsForEmployee.GroupBy(i => i.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                this.InitAbsenceDays(employee.EmployeeId, timePayrollTransactionsForEmployee.GetAbsenceDates(timeBlockDates), SoeTimeBlockDateDetailType.Read);

                List<TimeBlockDate> regeneratedTimeBlockDates = new List<TimeBlockDate>();
                foreach (AttestEmployeeDaySmallDTO day in daysByEmployee)
                {
                    TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(i => i.Date == day.Date);
                    if (timeBlockDate == null)
                        continue;

                    if (day.TimeBlockDateId <= 0)
                        day.TimeBlockDateId = timeBlockDate.TimeBlockDateId;

                    List<TimeBlock> timeBlocksForDay = timeBlocksByDay.GetList(timeBlockDate.TimeBlockDateId);
                    if (timeBlocksForDay.IsNullOrEmpty())
                        continue;

                    List<TimeScheduleTemplateBlock> scheduleBlocksForDay = scheduleBlocksByDay.GetList(timeBlockDate.Date);
                    List<TimePayrollTransaction> timePayrollTransactionsForDay = timePayrollTransactionsByDay.GetList(day.TimeBlockDateId);
                    if (timePayrollTransactionsForDay.GetAttestStateIds().IsEqualToAny(payrollLockedAttestStates.Select(i => i.AttestStateId).ToArray()))
                        continue;

                    if (TrySkipRecalculateDay(day, timePayrollTransactionsForDay, skipDaysInSameAbsenceRuleRow))
                        continue;

                    if (!day.TimeScheduleTemplatePeriodId.HasValue)
                        day.TimeScheduleTemplatePeriodId = GetTimeScheduleTemplatePeriodIdFromCache(day.EmployeeId, timeBlockDate.Date) ?? 0;

                    int? preservedAttestStateId = discardAttesteState ? PreserveTransactionsAttestStateId(timePayrollTransactionsForDay, attestStateInitial.AttestStateId) : null;

                    result = SetTransactionsToDeleted(timeBlocksForDay, saveChanges: false);
                    if (!result.Success)
                        return result;

                    result = Save();
                    if (!result.Success)
                        return result;

                    result = SaveTransactionsForPeriod(timeBlockDate, employee, day.TimeScheduleTemplatePeriodId, scheduleBlocksForDay: scheduleBlocksForDay, timeBlocksForDay: timeBlocksForDay);
                    if (!result.Success)
                        return result;

                    if (preservedAttestStateId.HasValue)
                    {
                        result = RestorePreservedAttestState(day.EmployeeId, day.Date, preservedAttestStateId.Value);
                        if (!result.Success)
                            return result;
                    }

                    regeneratedTimeBlockDates.Add(timeBlockDate);
                }

                result = TryRestoreUnhandledShiftsChanges(employee, regeneratedTimeBlockDates);
            }

            #endregion

            return result;
        }

        private ActionResult ReCalculateTransactionsDiscardAttest(List<TimeBlockDate> timeBlockDates, List<int> errorNumbers, DateTime? currentDate = null)
        {
            if (timeBlockDates.IsNullOrEmpty())
                return new ActionResult(true);

            List<AttestEmployeeDaySmallDTO> days = new List<AttestEmployeeDaySmallDTO>();
            foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(tbd => tbd.Date))
            {
                if (!days.Any(i => i.Date == timeBlockDate.Date))
                    days.Add(new AttestEmployeeDaySmallDTO(timeBlockDate.EmployeeId, timeBlockDate.Date, timeBlockDate.TimeBlockDateId, isDayForward: currentDate.HasValue && currentDate.Value < timeBlockDate.Date));
            }

            ActionResult result = ReCalculateTransactionsDiscardAttest(days, doNotRecalculateAmounts: true, skipDaysInSameAbsenceRuleRow: true);
            if (!result.Success)
            {
                if (!errorNumbers.Contains(result.ErrorNumber))
                    errorNumbers.Add(result.ErrorNumber);
                return result;
            }

            return result;
        }

        private ActionResult ReCalculateTransactionsDiscardAttest(List<AttestEmployeeDaySmallDTO> days, bool doNotRecalculateAmounts = false, bool skipDaysInSameAbsenceRuleRow = false)
        {
            //Preserve previous settings
            bool doNotCollectDaysForRecalculation = DoNotCollectDaysForRecalculation(onlyGlobal: true);
            bool doNotCalculateAmounts = DoNotCalculateAmounts();

            try
            {
                SetDoNotCollectDaysForRecalculation(true);
                SetDoNotCalculateAmounts(doNotRecalculateAmounts);

                return ReCalculateTransactions(days, discardAttesteState: true, skipDaysInSameAbsenceRuleRow: skipDaysInSameAbsenceRuleRow);
            }
            finally
            {
                //Reset previous settings
                SetDoNotCollectDaysForRecalculation(doNotCollectDaysForRecalculation);
                SetDoNotCalculateAmounts(doNotCalculateAmounts);
            }
        }

        private ActionResult RestorePreservedAttestState(int employeeId, DateTime date, int presevervedAttestStateId)
        {
            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsConnectedToTimeBlock(employeeId, date).Where(t => t.AttestStateId != presevervedAttestStateId).ToList();
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            timePayrollTransactions.ForEach(t => t.AttestStateId = presevervedAttestStateId);
            return Save();
        }

        private ActionResult ReCalculateRelatedDays(ReCalculateRelatedDaysOption option, int employeeId)
        {
            var context = new ReCalculationContext(
                overtimeTrackerDetails: GetOvertimeTrackerDetails(),
                applyAbsenceTrackerDetails: GetApplyAbsenceTrackerDetails(employeeId),
                restoreAbsenceTrackerDetails: GetRestoreAbsenceTrackerDetails(employeeId)
            );

            if (!context.HasDaysCalculated())
                return new ActionResult(true);

            ActionResult result = new ActionResult(true);

            try
            {
                SetDoNotCollectDaysForRecalculation(true);

                DateTime? lastLockedDate = GetLastPaidTimePayrollTransactionDate(employeeId);

                result = ReCalculateOvertimeDays(context, employeeId, lastLockedDate);
                if (!result.Success)
                    return result;

                if (option == ReCalculateRelatedDaysOption.Apply || option == ReCalculateRelatedDaysOption.ApplyAndRestore)
                {
                    result = ReCalculateApplyAbsenceDays(context, employeeId, lastLockedDate);
                    if (!result.Success)
                        return result;
                }
                if (option == ReCalculateRelatedDaysOption.Restore || option == ReCalculateRelatedDaysOption.ApplyAndRestore)
                {
                    result = ReCalculateRestoreAbsenceDays(context, employeeId, lastLockedDate);
                    if (!result.Success)
                        return result;
                }

                if (context.HasErrorNumbers)
                {
                    var messages = new HashSet<string>();
                    if (context.ContainsErrorNumber((int)ActionResultSave.TimePayrollTransactionCannotDeleteIsPayroll))
                    {
                        messages.Add(!string.IsNullOrEmpty(result.ErrorMessage)
                            ? result.ErrorMessage
                            : GetText(10006, "Vissa sjukdagar framåt har transaktioner som är överförda till lön och behöver hanteras manuellt."));
                    }
                    if (context.ContainsErrorNumber((int)ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState))
                    {
                        messages.Add(GetText(10007, "Vissa sjukdagar framåt räknades om och backades till startnivå och måste attesteras om."));
                    }
                    result.InfoMessage = string.Join(" ", messages);
                }

                return result;
            }
            finally
            {
                SetDoNotCollectDaysForRecalculation(false);
                CloseOvertimeTracker();
                CloseRestoreAbsenceTracker();
                CloseApplyAbsenceTracker();

                result.Success = true; //Never return error, only InfoMessage
            }
        }
        
        private ActionResult ReCalculateOvertimeDays(ReCalculationContext context, int employeeId, DateTime? lastLockedDate)
        {
            if (!context.HasOvertimeDaysCalculated())
                return new ActionResult();

            ActionResult result = new ActionResult(true);

            context.AddDatesAsCalculated(context.OvertimeTrackerDetails.Dates);
            context.AddDatesAsCalculated(GetInitiatedCalculationDates());

            foreach (OvertimeDay overtimeDay in context.OvertimeTrackerDetails.Days.ToList()) //Absence days may be added during execution
            {
                var overtimePeriod = GetDatesForOvertimePeriod(overtimeDay.Date);

                var overtimeDates = CalendarUtility
                  .GetDates(overtimePeriod.Start, overtimePeriod.Stop)
                  .Exclude(context.DatesCalculated)
                  .Where(i => !lastLockedDate.HasValue || i.Date > lastLockedDate.Value)
                  .ToList();

                var timeBlockDates = GetTimeBlockDatesFromCache(employeeId, overtimeDates);
                if (timeBlockDates.IsNullOrEmpty())
                    continue;

                result = ReCalculateTransactionsDiscardAttest(timeBlockDates, context.ErrorNumbers, currentDate: overtimeDay.Date);
                if (!result.Success)
                    return result;

                context.AddDatesAsCalculated(timeBlockDates.GetDates());
            }

            return result;
        }
        
        private ActionResult ReCalculateApplyAbsenceDays(ReCalculationContext context, int employeeId, DateTime? lastLockedDate)
        {
            if (!context.HasApplyAbsenceDaysCalculated())
                return new ActionResult();

            ActionResult result = new ActionResult(true);

            context.AddDatesAsCalculated(context.ApplyAbsenceTrackerDetails.Dates);

            if (lastLockedDate.HasValue && lastLockedDate.Value > context.ApplyAbsenceTrackerDetails.Dates.Max())
                lastLockedDate = null;

            foreach (ApplyAbsenceDay absenceDay in context.ApplyAbsenceTrackerDetails.Days.ToList()) //Absence days may be added during execution
            {
                var timeBlockDates = absenceDay.TimePayrollTransactionsToRecalculate.GetTimeBlockDates();

                if (context.ApplyAbsenceTrackerDetails.Tracker.HasVacationFiveDaysPerWeek())
                {
                    var weekDates = GetTimeBlockDatesFromCache(
                        employeeId,
                        CalendarUtility.GetFirstDateOfWeek(absenceDay.Date),
                        CalendarUtility.GetLastDateOfWeek(absenceDay.Date)
                    );
                    timeBlockDates.AddRange(weekDates);
                }

                timeBlockDates = timeBlockDates
                    .Where(i => !lastLockedDate.HasValue || i.Date > lastLockedDate.Value)
                    .ToList()
                    .Exclude(context.DatesCalculated);

                if (timeBlockDates.IsNullOrEmpty())
                    continue;

                result = ReCalculateTransactionsDiscardAttest(timeBlockDates, context.ErrorNumbers, currentDate: absenceDay.Date);
                if (!result.Success)
                    return result;

                context.AddDatesAsCalculated(timeBlockDates.GetDates());
            }

            return result;
        }
        
        private ActionResult ReCalculateRestoreAbsenceDays(ReCalculationContext context, int employeeId, DateTime? lastLockedDate)
        {
            ActionResult result = new ActionResult(true);

            if (!context.HasRestoreAbsenceDaysCalculated())
                return result;

            #region Recalculate vacation

            List<DateTime> absenceVacationDates = context.RestoreAbsenceTrackerDetails.Tracker.GetAbsenceDates(isVacation: true);
            if (!absenceVacationDates.IsNullOrEmpty())
            {
                DateTime absenceVacationStartDate = absenceVacationDates.First();
                DateTime absenceVacationStopDate = absenceVacationDates.Last();

                VacationGroupDTO startVacationGroup = GetVacationGroupFromCache(employeeId, absenceVacationStartDate);
                VacationGroupDTO stopVacationGroup = GetVacationGroupFromCache(employeeId, absenceVacationStopDate);
                if (startVacationGroup != null && stopVacationGroup != null)
                {
                    int level3Vacation = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;

                    //Only relevant to recalculate back if the absence rule has more than one row
                    TimeAbsenceRuleHead absenceRule = GetTimeAbsenceRuleHeadWithRowsFromCache(TermGroup_TimeAbsenceRuleType.Vacation, employeeId, absenceVacationStartDate);
                    if (absenceRule != null && absenceRule.GetRows().Count > 1)
                    {
                        //Find overtimeDates before start the restored period
                        var (absenceSequenceStartDate, _) = GetAbsenceDatesFromSequence(employeeId, context.RestoreAbsenceTrackerDetails.Days, level3Vacation, limitStartDate: lastLockedDate);
                        if (absenceSequenceStartDate.HasValue && absenceSequenceStartDate.Value < absenceVacationStartDate)
                        {
                            result = ReCalculateAbsence(absenceSequenceStartDate.Value, absenceVacationStartDate.AddDays(-1), level3Vacation);
                            if (!result.Success)
                                return result;
                        }
                    }

                    if (startVacationGroup.VacationGroupId == stopVacationGroup.VacationGroupId)
                    {
                        //If restored a coherent period, recalculate from first date after range, otherwise recalculate from first gap. To end of vacation year.
                        DateTime startDate = context.RestoreAbsenceTrackerDetails.Tracker.HasVacationFiveDaysPerWeek() ? absenceVacationStartDate : CalendarUtility.GetDateAfterCoherentRangeOrFirstGap(absenceVacationDates);
                        startVacationGroup.RealDateFrom = startVacationGroup.CalculateFromDate(startDate);
                        result = ReCalculateAbsence(startDate, startVacationGroup.RealDateTo, level3Vacation);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        //Recalculate from start date to end of first VacationGroup
                        DateTime startDate = context.RestoreAbsenceTrackerDetails.Tracker.HasVacationFiveDaysPerWeek() ? absenceVacationStartDate : absenceVacationStartDate.AddDays(1);
                        startVacationGroup.RealDateFrom = startVacationGroup.CalculateFromDate(startDate);
                        result = ReCalculateAbsence(startDate, startVacationGroup.RealDateTo, level3Vacation);
                        if (!result.Success)
                            return result;

                        //Recalculate whole second VacationGroup
                        startDate = startVacationGroup.RealDateTo.AddDays(1);
                        stopVacationGroup.RealDateFrom = stopVacationGroup.CalculateFromDate(startDate);
                        result = ReCalculateAbsence(startDate, stopVacationGroup.RealDateTo, level3Vacation);
                        if (!result.Success)
                            return result;
                    }
                }
            }

            #endregion

            #region Recalculate other absence

            List<DateTime> absenceOtherDates = context.RestoreAbsenceTrackerDetails.Tracker.GetAbsenceDates(isVacation: false);
            if (absenceOtherDates.Any())
            {
                foreach (int sysPayrollTypeLevel3 in context.RestoreAbsenceTrackerDetails.Days.GetSysPayrollTypeLevel3s())
                {
                    if (PayrollRulesUtil.IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave(sysPayrollTypeLevel3))
                    {
                        //Find overtimeDates start and stop of absence period for sysPayrollTypeLevel3
                        var (startDate, stopDate) = GetAbsenceDatesFromSequence(
                            employeeId,
                            context.RestoreAbsenceTrackerDetails.Days,
                            sysPayrollTypeLevel3,
                            maxDays: 6,
                            limitStartDate: lastLockedDate
                            );

                        if (startDate.HasValue && stopDate.HasValue)
                            result = ReCalculateAbsence(startDate.Value, stopDate.Value, sysPayrollTypeLevel3);

                    }
                    else
                    {
                        //If restored a coherent period, recalculate from first date after range, otherwise recalculate from first gap. To 1 year forward.
                        DateTime startDate = CalendarUtility.GetDateAfterCoherentRangeOrFirstGap(absenceOtherDates);
                        DateTime stopDate = startDate.AddYears(1);
                        result = ReCalculateAbsence(startDate, stopDate, sysPayrollTypeLevel3);
                    }

                    if (!result.Success)
                        return result;
                }
            }

            return result;

            ActionResult ReCalculateAbsence(DateTime startDate, DateTime stopDate, int sysPayrollTypeLevel3)
            {
                if (lastLockedDate.HasValue && lastLockedDate.Value > startDate)
                    startDate = lastLockedDate.Value;

                List<DateTime> dates = CalendarUtility.GetDatesInInterval(startDate, stopDate).Exclude(context.DatesCalculated);

                List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeId, dates);
                if (timeBlockDates.IsNullOrEmpty())
                    return new ActionResult(true);

                List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, sysPayrollTypeLevel3);
                if (sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation)
                    timePayrollTransactions.AddRange(GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(employeeId, timeBlockDates));

                timeBlockDates = timePayrollTransactions.GetTimeBlockDates();
                if (timeBlockDates.IsNullOrEmpty())
                    return new ActionResult(true);

                return ReCalculateTransactionsDiscardAttest(timeBlockDates, context.ErrorNumbers);
            }

            #endregion
        }

        #endregion

        #region Restore

        private ActionResult RestoreDaysToSchedule(List<AttestEmployeeDaySmallDTO> items)
        {
            ActionResult result = new ActionResult(true);

            if (items.IsNullOrEmpty())
                return result;

            foreach (var itemsByEmployee in items.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(itemsByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                List<TimeEngineDay> days = ConvertToTimeEngineDays(employee, itemsByEmployee.ToList());

                result = RestoreDaysToSchedule(employee.EmployeeId, days, clearScheduledAbsenceOnDates: itemsByEmployee.Select(i => i.Date).ToList(), acceptDaysWithAttestedTransactions: true);
                if (result.Success)
                {
                    var recalculateResult = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Restore, employee.EmployeeId);
                    if (!recalculateResult.Success)
                        return recalculateResult;

                    DoNotifyChangeOfDeviations();
                    DoInitiatePayrollWarnings();
                }
            }

            return result;
        }

        private ActionResult RestoreDaysToSchedule(int employeeId, List<TimeEngineDay> days, List<DateTime> clearScheduledAbsenceOnDates = null, bool doFilterValidDays = true, bool skipZeroSchedule = false, bool acceptDaysWithAttestedTransactions = false, bool deleteOnlyTimeBlockDateDetailsWithoutRatio = false)
        {
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: true);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            ActionResult result = new ActionResult(true);

            days = days.GetValidDays(employee, GetEmployeeGroupsFromCache(), out Dictionary<DateTime, bool> dateValidDict);
            if (!days.Any())
                return result;

            if (employee.Hidden)
                doFilterValidDays = false;

            var (scheduleBlocks, timeBlocks, acceptErrors) = DoLoadData();
            if (DoFilterData())
            {
                DoInitAbsence();
                DoClearSchedule();
                if (!employee.Hidden)
                {
                    DoDelete();
                    DoCreate();
                    DoTryRestoreUnhandledShiftChanges();
                    DoAddCurrentDaysPayrollWarning();
                }
            }
            DoFormatResult();

            return result;

            #region Functions

            (List<TimeScheduleTemplateBlock>, List<TimeBlock>, List<ActionResultSave>) DoLoadData()
            {
                return
                    (
                        GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, days.GetValidDates(dateValidDict), includeStandBy: true, includeOnDuty: true),
                        GetTimeBlocksWithTransactionsAndPayrollTransactionAccount(employee.EmployeeId, days.GetValidTimeBlockDateIds(dateValidDict)),
                        acceptDaysWithAttestedTransactions ? new List<ActionResultSave> { ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState } : null
                    );
            }
            bool DoFilterData()
            {
                if (doFilterValidDays)
                    FilterValidDays(employee.EmployeeId, days, dateValidDict, ref scheduleBlocks, ref timeBlocks, skipZeroSchedule);

                return scheduleBlocks.Any() || timeBlocks.Any();
            }
            void DoInitAbsence()
            {
                AddDaysToRestoreAbsenceTracker(days.GetValidDays(dateValidDict), timeBlocks, employee.EmployeeId);
            }
            void DoClearSchedule()
            {
                if (result.Success)
                    result = ClearTimeScheduleTemplateBlocksAndApplyAccounting(scheduleBlocks, employee, clearScheduledAbsenceOnDates, clearScheduledPlacement: true, updateOrderRemainingTime: true);
            }
            void DoDelete()
            {
                if (result.Success)
                    result = SetTimeBlockDateDetailsToDeleted(days.GetTimeBlockDates(), SoeTimeBlockDateDetailType.Absence, null, saveChanges: false, onlyWithoutRatio: deleteOnlyTimeBlockDateDetailsWithoutRatio);
                if (result.Success)
                    result = SetTimeBlocksAndTransactionsToDeleted(timeBlocks, saveChanges: false, acceptErrors: acceptErrors);
                if (result.Success)
                    result = SetTimePayrollScheduleTransactionsToDeleted(days.GetValidTimeBlockDateIds(dateValidDict), employee.EmployeeId, saveChanges: false);
                if (result.Success)
                    result = Save();
            }
            void DoCreate()
            {
                if (result.Success && !scheduleBlocks.IsNullOrEmpty())
                    result = SaveTimeBlocksAndTransactionsFromTemplate(scheduleBlocks, inputDays: days, acceptErrors: acceptErrors, onlyAutogenTimeBlocks: true, skipHidden: true);
            }
            void DoTryRestoreUnhandledShiftChanges()
            {
                if (result.Success)
                    result = TryRestoreUnhandledShiftsChanges(employee, days.GetValidTimeBlockDateIds(dateValidDict), force: true);
            }
            void DoAddCurrentDaysPayrollWarning()
            {
                AddCurrentDaysPayrollWarning(GetTimeBlockDatesFromCache(employeeId, days.GetValidDates(dateValidDict)));
            }
            void DoFormatResult()
            {
                SetResultValidDays(ref result, days, dateValidDict);
            }

            #endregion
        }

        private ActionResult RestoreDayToSchedule(TimeBlockDate timeBlockDate, bool clearScheduledAbsence = false, ShiftHistoryLogCallStackProperties logProperties = null, List<TimeScheduleTemplateBlock> scheduleBlocks = null)
        {
            #region Prereq

            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            Employee employee = GetEmployeeFromCache(timeBlockDate.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
            if (employee.Hidden)
                return new ActionResult(true);

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, timeBlockDate.Date.ToShortDateString()));

            TimeScheduleTemplateBlock originalShift = logProperties != null ? GetScheduleBlock(logProperties.OriginalShiftId) : null;
            TimeSchedulePlanningDayDTO originalShiftDto = originalShift?.ToTimeSchedulePlanningDayDTO();
            List<TimeBlock> existingTimeBlocks = GetTimeBlocksWithTransactions(employee.EmployeeId, timeBlockDate.TimeBlockDateId);

            AddDayToRestoreAbsenceTracker(timeBlockDate.Date, existingTimeBlocks, employee.EmployeeId);

            var result = SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(existingTimeBlocks, timeBlockDate, employee.EmployeeId, doDeleteTimeBlockDateDetails: true, saveChanges: true);
            if (!result.Success)
                return result;

            #endregion

            #region Clear schedule

            if (scheduleBlocks == null)
                scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, timeBlockDate.Date, null, includeStandBy: true);

            result = ClearTimeScheduleTemplateBlocksAndApplyAccounting(scheduleBlocks, employee, clearScheduledAbsence, clearScheduledPlacement: true, updateOrderRemainingTime: true);
            if (!result.Success)
                return result;

            #endregion

            #region Create TimeBlocks and transactions

            if (employeeGroup.AutogenTimeblocks && !employee.Hidden && !scheduleBlocks.IsNullOrEmpty())
                result = SaveTimeBlocksAndTransactionsFromTemplate(scheduleBlocks);

            #endregion

            #region Log

            if (logProperties != null && originalShiftDto != null)
            {
                logProperties.NewShiftId = originalShiftDto.TimeScheduleTemplateBlockId;

                TimeScheduleTemplateBlock shiftAfterChange = GetScheduleBlock(logProperties.NewShiftId);
                if (shiftAfterChange != null)
                {
                    ActionResult logResult = CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftDto, shiftAfterChange.ToTimeSchedulePlanningDayDTO(), logProperties);
                    if (!logResult.Success)
                        return logResult;
                }
            }

            #endregion

            return result;
        }

        private ActionResult RestoreDaysToScheduleDiscardDeviations(List<AttestEmployeeDaySmallDTO> items)
        {
            ActionResult result = new ActionResult(true);

            if (!items.IsNullOrEmpty())
            {
                foreach (var itemsByEmployee in items.GroupBy(i => i.EmployeeId))
                {
                    result = RestoreDayToScheduleDiscardDeviations(itemsByEmployee.Key, itemsByEmployee.ToList());
                }
            }

            return result;
        }

        private ActionResult RestoreDayToScheduleDiscardDeviations(int employeeId, List<AttestEmployeeDaySmallDTO> items)
        {
            DateTime? date = items?.FirstOrDefault()?.Date;
            if (!date.HasValue)
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date.Value, employeeGroups);
            if (employeeGroup == null || !employeeGroup.AutogenTimeblocks)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10114, "Tidavtal använder stämpling, inte avvikelserapportering"));

            List<TimeEngineDay> days = ConvertToTimeEngineDays(employee, items, employeeGroups);
            if (days.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimeEngineDay> validDays = ConvertToTimeEngineDaysWithoutDeviations(employeeId, days);
            if (validDays.IsNullOrEmpty())
                return new ActionResult(true);

            ActionResult result = RestoreDaysToSchedule(employeeId, validDays, skipZeroSchedule: true, acceptDaysWithAttestedTransactions: true);

            List<DateTime> invalidDates = days.Select(d => d.Date).Except(validDays.Select(d => d.Date)).ToList();
            if (invalidDates.Any())
            {
                if (result.Dates == null)
                    result.Dates = new List<DateTime>();
                result.Dates.AddRange(invalidDates);
            }

            return result;
        }

        private ActionResult RestoreCurrentDaysToSchedule(bool deleteOnlyTimeBlockDateDetailsWithoutRatio = false)
        {
            ActionResult result = new ActionResult(true);

            if (this.currentDaysToRestoreToSchedule.IsNullOrEmpty())
                return result;

            foreach (var daysByEmployee in this.currentDaysToRestoreToSchedule.GroupBy(x => x.EmployeeId).ToList())
            {
                int employeeId = daysByEmployee.Key;
                List<TimeEngineDay> days = ConvertToTimeEngineDays(employeeId, daysByEmployee.ToList());
                if (days.IsNullOrEmpty())
                    continue;

                List<DateTime> clearScheduledAbsenceOnDates = daysByEmployee.Where(i => i.ClearScheduledAbsence).Select(i => i.Date).ToList();
                result = RestoreDaysToSchedule(employeeId, days, clearScheduledAbsenceOnDates, doFilterValidDays: false, deleteOnlyTimeBlockDateDetailsWithoutRatio: deleteOnlyTimeBlockDateDetailsWithoutRatio);
                if (!result.Success)
                    return result;
            }
            this.currentDaysToRestoreToSchedule.Clear();

            return result;
        }

        private ActionResult RestoreDaysToScheduleTemplate(List<AttestEmployeeDaySmallDTO> items, out List<TimeBlockDate> autogenTimeBlockDates, out List<TimeBlockDate> stampingTimeBlockDates)
        {
            ActionResult result = new ActionResult(true);
            autogenTimeBlockDates = new List<TimeBlockDate>();
            stampingTimeBlockDates = new List<TimeBlockDate>();

            if (items.IsNullOrEmpty())
                return result;

            foreach (var itemsByEmployee in items.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(itemsByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                List<TimeEngineDay> days = ConvertToTimeEngineDays(employee, itemsByEmployee.ToList());

                result = RestoreDaysToScheduleTemplate(employee.EmployeeId, days, ref autogenTimeBlockDates, ref stampingTimeBlockDates);
                if (result.Success)
                {
                    DoNotifyChangeOfDeviations();
                    DoInitiatePayrollWarnings();
                }
            }

            return result;
        }

        private ActionResult RestoreDaysToScheduleTemplate(int employeeId, List<TimeEngineDay> days, ref List<TimeBlockDate> autogenTimeBlockDates, ref List<TimeBlockDate> stampingTimeBlockDates)
        {
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            ActionResult result = new ActionResult(true);

            days = days.GetValidDays(employee, GetEmployeeGroupsFromCache(), out Dictionary<DateTime, bool> dateValidDict);
            if (!days.Any())
                return result;

            var (scheduleBlocks, timeBlocks) = DoLoadData();
            if (DoFilterData())
            {
                DoInitAbsence();
                DoDeleteSchedule();
                DoCreateSchedule();
            }
            DoFormatResult();

            foreach (TimeEngineDay day in days.GetValidDays(dateValidDict))
            {
                if (day.EmployeeGroup.AutogenTimeblocks)
                    autogenTimeBlockDates.Add(day.CalculatedTimeBlockDate);
                else
                    stampingTimeBlockDates.Add(day.CalculatedTimeBlockDate);
            }

            return result;

            #region Functions

            (List<TimeScheduleTemplateBlock>, List<TimeBlock>) DoLoadData()
            {
                LoadTimeBlockDateAndEmployeeGroupAsMandatory(days, employee.EmployeeId, dateValidDict);
                return
                    (
                        GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employeeId, days.GetValidDates(dateValidDict)),
                        GetTimeBlocksWithTransactionsAndPayrollTransactionAccount(employee.EmployeeId, days.GetValidTimeBlockDateIds(dateValidDict))
                    );
            }
            bool DoFilterData()
            {
                FilterValidDays(employee.EmployeeId, days, dateValidDict, ref scheduleBlocks, ref timeBlocks);
                return scheduleBlocks.Any() || timeBlocks.Any();
            }
            void DoInitAbsence()
            {
                AddDaysToRestoreAbsenceTracker(days.GetValidDays(dateValidDict), timeBlocks, employee.EmployeeId);
            }
            void DoDeleteSchedule()
            {
                if (result.Success)
                    result = SetScheduleToDeleted(scheduleBlocks, saveChanges: true);
            }
            void DoCreateSchedule()
            {
                if (result.Success)
                {
                    foreach (TimeEngineDay day in days.GetValidDays(dateValidDict).ToList())
                    {
                        List<TimeScheduleTemplateBlockReference> references = CreateTimeScheduleTemplateBlocks(employee, day.CalculatedEmployeeSchedule, day.Date, day.Date, deleteExisting: false);
                        result = ReCalculateDayFromSchedule(day.TemplatePeriodId, references.GetTemplateBlocks(), day.CalculatedTimeBlockDate, employee);
                        if (!result.Success)
                            dateValidDict[day.Date] = false;
                    }
                }
            }
            void DoFormatResult()
            {
                SetResultValidDays(ref result, days, dateValidDict);
            }

            #endregion
        }

        #endregion

        #region Status changes

        private void SetTimeBlockDateStatus(TimeEngineDay day, SoeTimeBlockDateStatus status)
        {
            if (day == null)
                return;

            SetTimeBlockDateStatus(new List<TimeEngineDay> { day }, status);
        }

        private void SetTimeBlockDateStatus(List<TimeEngineDay> days, SoeTimeBlockDateStatus status)
        {
            if (days.IsNullOrEmpty())
                return;

            foreach (TimeEngineDay day in days)
            {
                day.TimeBlockDate.Status = (int)status;
            }
        }

        private void SetTimeStampEntryStatus(TimeEngineDay day, TermGroup_TimeStampEntryStatus oldStatus, TermGroup_TimeStampEntryStatus newStatus)
        {
            if (day == null)
                return;

            SetTimeStampEntryStatus(new List<TimeEngineDay> { day }, oldStatus, newStatus);
        }

        private void SetTimeStampEntryStatus(List<TimeEngineDay> days, TermGroup_TimeStampEntryStatus oldStatus, TermGroup_TimeStampEntryStatus newStatus)
        {
            if (days.IsNullOrEmpty())
                return;

            List<int> timeStampEntryIds = new List<int>();
            foreach (TimeEngineDay day in days.Where(d => !d.TimeStampEntryIds.IsNullOrEmpty()))
            {
                foreach (int timeStampEntryId in day.TimeStampEntryIds)
                {
                    if (!timeStampEntryIds.Contains(timeStampEntryId))
                        timeStampEntryIds.Add(timeStampEntryId);
                }

                timeStampEntryIds.AddRange(day.TimeStampEntryIds);
            }

            List<TimeStampEntry> timeStampEntries = (from tse in entities.TimeStampEntry
                                                     where tse.Status == (int)oldStatus &&
                                                     tse.State == (int)SoeEntityState.Active &&
                                                     timeStampEntryIds.Contains(tse.TimeStampEntryId)
                                                     select tse).ToList();

            foreach (TimeStampEntry timeStampEntry in timeStampEntries)
            {
                timeStampEntry.Status = (int)newStatus;
            }
        }

        #endregion

        #region TimeBlock generation

        private List<TimeBlock> CreateTimeBlocksFromTemplate(Employee employee, List<TimeScheduleTemplateBlock> scheduleBlocks, TimeBlockDate timeBlockDate, int? standardTimeDeviationCauseId, bool temporary = false)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();

            if (scheduleBlocks.IsNullOrEmpty())
                return timeBlocks;

            foreach (TimeEngineBlock block in TimeEngineBlock.Create(scheduleBlocks))
            {
                TimeScheduleTemplateBlock originalScheduleBlock = block.FindOriginal(scheduleBlocks);
                if (originalScheduleBlock == null)
                    continue;

                TimeCode timeCode = originalScheduleBlock.TimeCode != null && base.IsEntityAvailableInContext(entities, originalScheduleBlock) ? originalScheduleBlock.TimeCode : GetTimeCodeFromCache(originalScheduleBlock.TimeCodeId);
                if (timeCode == null)
                    continue;

                TimeBlock timeBlock = CreateTimeBlock(block.StartTime, block.StopTime, employee, originalScheduleBlock, timeCode, timeBlockDate, standardTimeDeviationCauseId, temporary: temporary);
                if (timeBlock == null)
                    continue;

                timeBlocks.Add(timeBlock);
            }

            return timeBlocks;
        }

        private ActionResult SaveTimeBlocks(out List<TimeEngineDay> days, List<TimeBlock> inputTimeBlocks, List<TimeCodeTransaction> additionalTimeCodeTransactions = null)
        {
            days = new List<TimeEngineDay>();

            additionalTimeCodeTransactions = additionalTimeCodeTransactions?.Where(t => t.TimeBlockDate != null).ToList();

            if (inputTimeBlocks == null && additionalTimeCodeTransactions.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlock");

            foreach (var timeBlockInputsByDate in inputTimeBlocks.Where(i => i.TimeBlockDate != null && i.TimeScheduleTemplatePeriodId.HasValue).GroupBy(i => i.TimeBlockDate.Date))
            {
                TimeBlockDate timeBlockDate = timeBlockInputsByDate.First().TimeBlockDate;
                int templatePeriodId = timeBlockInputsByDate.First().TimeScheduleTemplatePeriodId.Value;

                if (timeBlockInputsByDate.Any(tb => tb.CalculatedTimeRuleType == SoeTimeRuleType.Absence))
                    InitAbsenceDay(timeBlockDate.EmployeeId, timeBlockDate.Date);

                foreach (TimeBlock timeBlockInput in timeBlockInputsByDate)
                {
                    timeBlockInput.IsBreak = timeBlockInput.IsBreak();
                    entities.TimeBlock.AddObject(timeBlockInput);
                }

                days.AddDay(
                    templatePeriodId: templatePeriodId, 
                    timeBlockDate: timeBlockDate, 
                    timeBlocks: timeBlockInputsByDate.ToList(),
                    additionalTimeCodeTransactions: additionalTimeCodeTransactions
                    );
            }

            if (!days.Any() && !additionalTimeCodeTransactions.IsNullOrEmpty())
            {
                foreach(var additionalTimeCodeTransactionsByDay in additionalTimeCodeTransactions.GroupBy(t => t.TimeBlockDate))
                {
                    var timeBlockDate = additionalTimeCodeTransactionsByDay.Key;
                    var templatePeriod = GetTimeScheduleTemplatePeriod(timeBlockDate.EmployeeId, timeBlockDate.Date);

                    days.AddDay(
                        templatePeriodId: templatePeriod?.TimeScheduleTemplatePeriodId,
                        timeBlockDate: timeBlockDate,
                        additionalTimeCodeTransactions: additionalTimeCodeTransactionsByDay.ToList()
                        );
                }
            }

            return Save();
        }

        private ActionResult SaveTimeBlocks(List<AttestEmployeeDayTimeBlockDTO> inputTimeBlocks, Employee employee, TimeBlockDate timeBlockDate, int timeScheduleTemplatePeriodId)
        {
            if (inputTimeBlocks == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlocks");

            ActionResult result = null;
            List<TimeBlock> savedTimeBlocks = new List<TimeBlock>();

            List<TimeScheduleTemplateBlock> existingScheduleBlocks = GetScheduleBlocksFromCache(employee.EmployeeId, timeBlockDate.Date);
            List<TimeBlock> existingTimeBlocks = GetTimeBlocksWithTimeCodeAndAccountInternal(employee.EmployeeId, timeBlockDate.TimeBlockDateId, timeScheduleTemplatePeriodId);

            foreach (TimeBlock timeBlock in existingTimeBlocks)
            {
                if (inputTimeBlocks.IsNullOrEmpty() && existingTimeBlocks.Count == 1 && existingTimeBlocks.First()?.StartTime == existingTimeBlocks.First()?.StopTime)
                {
                    savedTimeBlocks.Add(timeBlock);
                    continue;
                }

                AttestEmployeeDayTimeBlockDTO inputTimeBlock = inputTimeBlocks.FirstOrDefault(i => i.TimeBlockId == timeBlock.TimeBlockId);
                if (inputTimeBlock != null)
                {
                    #region Update

                    bool manuallyAdjusted = timeBlock.StartTime != inputTimeBlock.StartTime || timeBlock.StopTime != inputTimeBlock.StopTime;

                    SetTimeBlockValues(timeBlock, inputTimeBlock, existingScheduleBlocks, employee, timeBlockDate, timeScheduleTemplatePeriodId, manuallyAdjusted);
                    SetModifiedProperties(timeBlock);
                    savedTimeBlocks.Add(timeBlock);

                    #endregion
                }
                else
                {
                    #region Delete

                    result = SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
                    if (!result.Success)
                        return result;

                    #endregion
                }
            }

            #region Create

            foreach (AttestEmployeeDayTimeBlockDTO inputTimeBlock in inputTimeBlocks.Where(i => i.TimeBlockId == 0))
            {
                bool manuallyAdjusted = !inputTimeBlock.IsBreak || inputTimeBlock.ManuallyAdjusted;

                TimeBlock timeBlock = new TimeBlock();
                SetTimeBlockValues(timeBlock, inputTimeBlock, existingScheduleBlocks, employee, timeBlockDate, timeScheduleTemplatePeriodId, manuallyAdjusted);
                SetCreatedProperties(timeBlock);
                entities.TimeBlock.AddObject(timeBlock);

                savedTimeBlocks.Add(timeBlock);
            }

            #endregion

            result = Save();
            if (result.Success)
                result.Value = savedTimeBlocks;

            return result;
        }

        private void SetTimeBlockValues(TimeBlock timeBlock, AttestEmployeeDayTimeBlockDTO prototype, List<TimeScheduleTemplateBlock> scheduleBlocks, Employee employee, TimeBlockDate timeBlockDate, int templatePeriodId, bool manuallyAdjusted)
        {
            if (timeBlock == null || prototype == null || employee == null || timeBlockDate == null)
                return;

            timeBlock.StartTime = prototype.StartTime;
            timeBlock.StopTime = prototype.StopTime;
            timeBlock.Comment = prototype.Comment;
            timeBlock.ManuallyAdjusted = timeBlock.ManuallyAdjusted || manuallyAdjusted;
            timeBlock.IsBreak = prototype.IsBreak;
            timeBlock.State = (int)SoeEntityState.Active;

            //Set FK
            timeBlock.EmployeeId = employee.EmployeeId;
            timeBlock.TimeBlockDateId = timeBlockDate.TimeBlockDateId;
            timeBlock.TimeScheduleTemplatePeriodId = templatePeriodId;
            timeBlock.TimeDeviationCauseStartId = prototype.TimeDeviationCauseStartId;
            timeBlock.TimeDeviationCauseStopId = prototype.TimeDeviationCauseStopId;
            timeBlock.TimeScheduleTemplateBlockBreakId = prototype.TimeScheduleTemplateBlockBreakId;
            timeBlock.EmployeeChildId = prototype.EmployeeChildId;
            timeBlock.ShiftTypeId = prototype.ShiftTypeId;
            timeBlock.TimeScheduleTypeId = prototype.TimeScheduleTypeId;
            
            if (!prototype.DeviationAccounts.IsNullOrEmpty())
            {
                timeBlock.DeviationAccountIds = prototype.DeviationAccounts.Select(i => i.AccountId).ToCommaSeparated();
                timeBlock.SetDeviationAccounts(GetAccountInternalsWithAccountFromCache(prototype.DeviationAccounts.Select(a => a.AccountId)));
            }
            if (!prototype.TimeCodes.IsNullOrEmpty())
            {
                foreach (var prototypeTimeCode in prototype.TimeCodes)
                {
                    TimeCode timeCode = GetTimeCodeFromCache(prototypeTimeCode.TimeCodeId);
                    if (timeCode != null)
                        timeBlock.TimeCode.Add(timeCode);
                }
            }

            if (!String.IsNullOrEmpty(prototype.GuidId))
                timeBlock.GuidId = Guid.Parse(prototype.GuidId);

            ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, scheduleBlocks.GetTemplateBlockFromTimeBlock(timeBlock), employee, accountInternals: timeBlock.DeviationAccounts);
        }

        private ActionResult SaveTimeBlocksFromTemplate(List<TimeScheduleTemplateBlock> templateBlocks, List<TimeEngineDay> inputDays, out List<TimeEngineDay> processedDays, List<ActionResultSave> acceptErrors = null, bool onlyAutogenTimeBlocks = false, bool skipHidden = false)
        {
            processedDays = new List<TimeEngineDay>();

            if (templateBlocks.IsNullOrEmpty())
                return new ActionResult(true);

            ActionResult result = new ActionResult(true);

            #region Perform

            try
            {
                templateBlocks = templateBlocks.Where(b => b.EmployeeId.HasValue && b.Date.HasValue && !b.IsOnDuty()).ToList();
                foreach (var templateBlocksByEmployee in templateBlocks.GroupBy(b => b.EmployeeId.Value))
                {
                    try
                    {
                        Employee employee = GetEmployeeFromCache(templateBlocksByEmployee.Key);
                        if (employee == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));

                        if (skipHidden && employee.Hidden)
                            continue;

                        List<TimeEngineDay> inputDaysForEmployee = inputDays?.Where(i => i.EmployeeId == employee.EmployeeId).ToList();
                        if (onlyAutogenTimeBlocks && inputDaysForEmployee != null && inputDaysForEmployee.All(i => i.EmployeeGroup?.AutogenTimeblocks == false))
                            continue;

                        List<TimeEngineDay> processedDaysForEmployee = new List<TimeEngineDay>();
                        Dictionary<DateTime, List<TimeScheduleTemplateBlock>> templateBlocksForEmployeeByDate = templateBlocksByEmployee.GroupBy(i => i.Date.Value).ToDictionary(k => k.Key, v => v.ToList());
                        Dictionary<DateTime, List<TimeBlock>> timeBlocksForEmployeeByDate = GetTimeBlocksWithTimeBlockDate(employee.EmployeeId, templateBlocksForEmployeeByDate.Keys.Min(), templateBlocksForEmployeeByDate.Keys.Max()).GroupBy(i => i.TimeBlockDate.Date).ToDictionary(k => k.Key, v => v.ToList());

                        foreach (var pair in templateBlocksForEmployeeByDate.OrderBy(i => i.Key))
                        {
                            #region Prereq

                            DateTime date = pair.Key;
                            List<TimeScheduleTemplateBlock> scheduleBlocksForDate = pair.Value;
                            TimeEngineDay inputDay = inputDays?.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId && i.Date == date);

                            EmployeeGroup employeeGroup = inputDay?.EmployeeGroup ?? employee.GetEmployeeGroup(date, employeeGroups: GetEmployeeGroupsFromCache());
                            if (employeeGroup == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, date.ToShortDateString()));
                            if (onlyAutogenTimeBlocks && !employeeGroup.AutogenTimeblocks)
                                continue;

                            TimeBlockDate timeBlockDate = inputDay?.TimeBlockDate ?? GetTimeBlockDateFromCache(employee.EmployeeId, date, true);
                            if (timeBlockDate == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                            int standardTimeDeviationCauseId = inputDay?.StandardTimeDeviationCauseId ?? GetTimeDeviationCauseIdFromPrio(employee, employeeGroup, null, true);
                            int? templatePeriodId = inputDay?.TemplatePeriodId?.ToNullable() ?? scheduleBlocksForDate.First().TimeScheduleTemplatePeriodId;

                            #endregion

                            #region Delete existing TimeBlock's

                            if (timeBlocksForEmployeeByDate.ContainsKey(date))
                            {
                                List<TimeBlock> timeBlocksForEmployeeAndDate = timeBlocksForEmployeeByDate[date];
                                if (timeBlocksForEmployeeAndDate.Any() && timeBlocksForEmployeeAndDate.All(b => b.IsZeroBlock() && b.TimeDeviationCauseStartId.HasValue))
                                    continue; //Skip zero days with absence

                                result = SetTimeBlocksAndTransactionsToDeleted(timeBlocksForEmployeeAndDate, saveChanges: false);
                                if (!result.Success)
                                {
                                    bool acceptError = acceptErrors?.Contains((ActionResultSave)result.ErrorNumber) ?? false;
                                    if (acceptError)
                                        continue;
                                    else
                                        return result;
                                }
                            }

                            #endregion

                            #region Create new TimeBlock's

                            TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate, templatePeriodId, standardTimeDeviationCauseId: standardTimeDeviationCauseId, scheduleBlocks: scheduleBlocksForDate);
                            List<TimeBlock> timeBlocksForDate = CreateTimeBlocksUsingTemplateRepository(identity);
                            
                            processedDaysForEmployee.AddDay(
                                templatePeriodId: templatePeriodId, 
                                timeBlockDate: timeBlockDate, 
                                timeBlocks: timeBlocksForDate, 
                                employeeGroup: employeeGroup, 
                                standardTimeDeviationCauseId: standardTimeDeviationCauseId
                                );

                            #endregion
                        }

                        #region Update status for TimeBlockDate's

                        SetTimeBlockDateStatus(processedDaysForEmployee, SoeTimeBlockDateStatus.Regenerating);

                        #endregion


                        #region Postfix

                        //Detach so changes to the template not is saved (i.e. break is parsed to its actual value instead of break window)
                        DetachSchedule(templateBlocks);

                        #endregion

                        result = Save();
                        if (result.Success)
                            processedDays.AddRange(processedDaysForEmployee);
                    }
                    catch (Exception ex)
                    {
                        //Continue to next Employee if placement failes
                        LogError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.Success = false;
                result.Exception = ex;
            }

            #endregion

            return result;
        }

        private ActionResult SaveTimeBlocksAndTransactionsFromTemplate(List<TimeScheduleTemplateBlock> templateBlocks, List<TimeEngineDay> inputDays = null, List<ActionResultSave> acceptErrors = null, bool onlyAutogenTimeBlocks = false, bool skipHidden = false)
        {
            ActionResult result = SaveTimeBlocksFromTemplate(templateBlocks, inputDays, out List<TimeEngineDay> processedDays, acceptErrors, onlyAutogenTimeBlocks, skipHidden);
            if (result.Success)
                result = SaveTransactionsForPeriods(processedDays);
            return result;
        }

        /// <summary>
        /// Rearranges existing TimeBlocks to make room for the newTimeBlock. Will possibly split existingTimeBlocks into new TimeBlock's.
        /// </summary>
        /// <param name="newTimeBlock">The new TimeBlock to make room for</param>
        /// <param name="existingTimeBlocks">The existing TimeBlock's to rearrange</param>
        /// <param name="timeDeviationCause">The TimeDeviationCause</param>
        /// <param name="addNewTimeBlock">If the newTimeBlock should be included in the returned result</param>
        /// <param name="addExistingTimeBlocks">If the existingTimeBlocks should be included in the returned result</param>
        /// <returns>List of TimeBlocks. Content will be affected by the passed parameters addNewTimeBlock and addExistingTimeBlocks</returns>
        private List<TimeBlock> RearrangeNewTimeBlockAgainstExisting(TimeBlock newTimeBlock, List<TimeBlock> existingTimeBlocks, TimeDeviationCause timeDeviationCause, bool addNewTimeBlock = true, bool addExistingTimeBlocks = true, bool deleteBreaksOverlappedByAbsence = false, bool orderToSalary = false, bool copyAllValues = false)
        {
            List<TimeBlock> outputTimeBlocks = new List<TimeBlock>();
            List<TimeBlock> timeBlocksToDelete = new List<TimeBlock>();

            existingTimeBlocks = existingTimeBlocks.OrderBy(i => i.StartTime).ToList();

            for (int i = 0; i < existingTimeBlocks.Count; i++)
            {
                TimeBlock currentTimeBlock = existingTimeBlocks[i];
                if (!currentTimeBlock.GuidId.HasValue || currentTimeBlock.GuidId.Value == Guid.Empty)
                    currentTimeBlock.GuidId = Guid.NewGuid();

                if (IsNewTimeBlockOverlappedByCurrentTimeBlock(newTimeBlock, currentTimeBlock))
                {
                    #region Overlapped by current

                    if (currentTimeBlock.IsAttested)
                    {
                        //The newTimeBlock will not be created
                        addNewTimeBlock = false;
                        break;
                    }
                    else
                    {
                        //Split control with TimeBlock between them

                        //Create TimeBlock corresponding to second part
                        if ((newTimeBlock.StopTime != currentTimeBlock.StopTime) || orderToSalary) //dont create zero timeblocks
                        {
                            var splittedTimeBlock = new TimeBlock
                            {
                                StartTime = newTimeBlock.StopTime,
                                StopTime = currentTimeBlock.StopTime,
                                ManuallyAdjusted = true,
                                GuidId = Guid.NewGuid()
                            };
                            SetCreatedProperties(splittedTimeBlock);

                            if (orderToSalary)
                            {
                                splittedTimeBlock.StopTime = splittedTimeBlock.StopTime.AddMinutes(newTimeBlock.TotalMinutes);
                            }

                            splittedTimeBlock.CopyFrom(currentTimeBlock, copyAllValues);
                            SetCreatedProperties(splittedTimeBlock);

                            // Accounting
                            splittedTimeBlock.AccountStdId = currentTimeBlock.AccountStdId ?? GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);

                            AddAccountInternalsToTimeBlock(splittedTimeBlock, currentTimeBlock);

                            //Add TimeStampEntry's
                            AddTimeStampEntrysToTimeBlock(splittedTimeBlock, currentTimeBlock);

                            //Add TimeBlock
                            outputTimeBlocks.Add(splittedTimeBlock);

                            if (orderToSalary)
                            {
                                //move the coming blocks forward if they now overlapp....
                                var minutesToAdd = CalendarUtility.TimeSpanToMinutes(newTimeBlock.StopTime, newTimeBlock.StartTime);
                                var compareBlock = splittedTimeBlock;
                                for (int y = i + 1; y < existingTimeBlocks.Count; y++)
                                {
                                    var blockToMove = existingTimeBlocks[y];
                                    if (blockToMove.StartTime < compareBlock.StopTime)
                                    {
                                        blockToMove.StartTime = blockToMove.StartTime.AddMinutes(minutesToAdd);
                                        blockToMove.StopTime = blockToMove.StopTime.AddMinutes(minutesToAdd);
                                    }
                                    compareBlock = blockToMove;
                                }
                            }
                        }

                        //Reduce first parts stop time
                        currentTimeBlock.StopTime = newTimeBlock.StartTime;

                        if (currentTimeBlock.StartTime == currentTimeBlock.StopTime)
                        {
                            SetTimeBlockAndTransactionsToDeleted(currentTimeBlock, saveChanges: false);
                            timeBlocksToDelete.Add(currentTimeBlock);
                            if (currentTimeBlock.TimeBlockId == 0 && IsEntityAvailableInContext(entities, currentTimeBlock))
                                entities.DeleteObject(currentTimeBlock);
                        }
                    }

                    #endregion
                }
                else if (IsCurrentTimeBlockOverlappedByNewTimeBlock(newTimeBlock, currentTimeBlock))
                {
                    #region Overlapped by new TimeBlock

                    if (orderToSalary)
                    {
                        //move the current timeblock after the new....
                        var minutesToAdd = CalendarUtility.TimeSpanToMinutes(newTimeBlock.StopTime, currentTimeBlock.StartTime);
                        currentTimeBlock.StartTime = currentTimeBlock.StartTime.AddMinutes(minutesToAdd);
                        currentTimeBlock.StopTime = currentTimeBlock.StopTime.AddMinutes(minutesToAdd);

                        //move the coming blocks forward...
                        for (int y = i + 1; y < existingTimeBlocks.Count; y++)
                        {
                            var blockToMove = existingTimeBlocks[y];
                            blockToMove.StartTime = blockToMove.StartTime.AddMinutes(minutesToAdd);
                            blockToMove.StopTime = blockToMove.StopTime.AddMinutes(minutesToAdd);
                        }
                    }
                    else if (currentTimeBlock.IsAttested)
                    {
                        //Update new TimeBlock
                        newTimeBlock.StartTime = currentTimeBlock.StopTime;
                    }
                    else if (currentTimeBlock.IsBreak && !deleteBreaksOverlappedByAbsence && timeDeviationCause != null && (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence || timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence))
                    {
                        if (newTimeBlock.CreateAsBlank)
                            continue;

                        if (newTimeBlock.StartTime != currentTimeBlock.StartTime)//dont create zero timeblocks
                        {
                            TimeBlock timeBlockBeforeBreak = new TimeBlock()
                            {
                                StartTime = newTimeBlock.StartTime,
                                StopTime = currentTimeBlock.StartTime,
                                EmployeeId = currentTimeBlock.EmployeeId,
                                IsBreak = newTimeBlock.IsBreak(),
                                Comment = newTimeBlock.Comment,
                                ManuallyAdjusted = true,
                                IsPreliminary = newTimeBlock.IsPreliminary,
                                GuidId = Guid.NewGuid(),

                                //Set FK
                                TimeDeviationCauseStopId = timeDeviationCause.TimeDeviationCauseId,
                                TimeDeviationCauseStartId = timeDeviationCause.TimeDeviationCauseId,
                                TimeScheduleTemplateBlockBreakId = null,
                                EmployeeChildId = newTimeBlock.EmployeeChildId,
                                ShiftTypeId = newTimeBlock.ShiftTypeId,
                                TimeScheduleTypeId = newTimeBlock.TimeScheduleTypeId,
                                ProjectTimeBlockId = newTimeBlock.ProjectTimeBlockId,
                                PayrollImportEmployeeTransactionId = newTimeBlock.PayrollImportEmployeeTransactionId,
                            };
                            SetCreatedProperties(timeBlockBeforeBreak);

                            //Add TimeCode                 
                            if (timeDeviationCause.TimeCode != null)
                                timeBlockBeforeBreak.TimeCode.Add(timeDeviationCause.TimeCode);

                            // Accounting
                            timeBlockBeforeBreak.AccountStdId = GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);
                            AddAccountInternalsToTimeBlock(timeBlockBeforeBreak, currentTimeBlock);

                            //Add TimeStampEntry's
                            AddTimeStampEntrysToTimeBlock(timeBlockBeforeBreak, currentTimeBlock);

                            //Add TimeBlock
                            outputTimeBlocks.Add(timeBlockBeforeBreak);
                        }

                        //Update new TimeBlock
                        newTimeBlock.StartTime = currentTimeBlock.StopTime;
                        if (addNewTimeBlock && newTimeBlock.StartTime != newTimeBlock.StopTime)
                            AddAccountInternalsToTimeBlock(newTimeBlock, currentTimeBlock);
                    }
                    else
                    {
                        SetTimeBlockAndTransactionsToDeleted(currentTimeBlock, saveChanges: false);
                        timeBlocksToDelete.Add(currentTimeBlock);
                        if (currentTimeBlock.TimeBlockId == 0 && IsEntityAvailableInContext(entities, currentTimeBlock))
                            entities.DeleteObject(currentTimeBlock);
                    }

                    #endregion
                }
                else if (IsNewTimeBlockStopInCurrentTimeBlock(newTimeBlock, currentTimeBlock))
                {
                    #region Place new TimeBlock before current

                    //The newTimeBlock will be added in front of control
                    if (currentTimeBlock.IsAttested)
                        newTimeBlock.StopTime = currentTimeBlock.StartTime;
                    else
                        currentTimeBlock.StartTime = newTimeBlock.StopTime;

                    #endregion
                }
                else if (IsNewTimeBlockStartInCurrentTimeBlock(newTimeBlock, currentTimeBlock))
                {

                    #region Splitt current TimeBlock

                    if (orderToSalary)
                    {
                        var splittedTimeBlock = new TimeBlock
                        {
                            StartTime = newTimeBlock.StopTime,
                            StopTime = currentTimeBlock.StopTime.AddMinutes(newTimeBlock.TotalMinutes),
                            ManuallyAdjusted = true,
                            GuidId = Guid.NewGuid()
                        };
                        splittedTimeBlock.CopyFrom(currentTimeBlock, copyAllValues);
                        SetCreatedProperties(splittedTimeBlock);

                        // Accounting
                        splittedTimeBlock.AccountStdId = currentTimeBlock.AccountStdId ?? GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);

                        AddAccountInternalsToTimeBlock(splittedTimeBlock, currentTimeBlock);

                        //Add TimeStampEntry's
                        AddTimeStampEntrysToTimeBlock(splittedTimeBlock, currentTimeBlock);

                        //Add TimeBlock
                        outputTimeBlocks.Add(splittedTimeBlock);

                        //Reduce first parts stop time
                        currentTimeBlock.StopTime = newTimeBlock.StartTime;

                        //move the coming blocks forward...
                        var minutesToAdd = CalendarUtility.TimeSpanToMinutes(newTimeBlock.StopTime, newTimeBlock.StartTime);
                        for (int y = i + 1; y < existingTimeBlocks.Count; y++)
                        {
                            var blockToMove = existingTimeBlocks[y];
                            blockToMove.StartTime = blockToMove.StartTime.AddMinutes(minutesToAdd);
                            blockToMove.StopTime = blockToMove.StopTime.AddMinutes(minutesToAdd);
                        }
                    }

                    #endregion

                    #region Place new TimeBlock after current
                    else
                    {
                        //The newTimeBlock will be added after control
                        if (currentTimeBlock.IsAttested)
                            newTimeBlock.StartTime = currentTimeBlock.StopTime;
                        else
                            currentTimeBlock.StopTime = newTimeBlock.StartTime;
                    }
                    #endregion
                }
            }

            #region Leftovers existing

            if (addExistingTimeBlocks)
            {
                //Copy what is left of existingTimeBlocks
                foreach (TimeBlock existingTimeBlock in existingTimeBlocks)
                {
                    if (!timeBlocksToDelete.Any(i => i.GuidId == existingTimeBlock.GuidId))
                        outputTimeBlocks.Add(existingTimeBlock);
                }
            }

            #endregion

            #region New TimeBlock

            if (addNewTimeBlock)
            {
                outputTimeBlocks.Add(newTimeBlock);
            }

            #endregion

            //Order
            outputTimeBlocks = outputTimeBlocks.OrderBy(tb => tb.StartTime).ToList();

            return outputTimeBlocks;
        }

        private void SaveTimeBlocksAndTransactionsFromTemplateAsync(TimeEngineInputDTO iDTO)
        {
            if (iDTO == null || iDTO.AsyncTemplateBlocks.IsNullOrEmpty())
                return;

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                foreach (var asyncTemplateBlocksByEmployee in iDTO.AsyncTemplateBlocks.Where(t => t.EmployeeId.HasValue && t.Date.HasValue).GroupBy(t => t.EmployeeId.Value))
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        List<TimeScheduleTemplateBlock> asyncTemplateBlocks = asyncTemplateBlocksByEmployee.OrderBy(i => i.Date.Value).ThenBy(i => i.StartTime).ToList();

                        ActionResult result = SaveTimeBlocksFromTemplate(asyncTemplateBlocks, null, out List<TimeEngineDay> days);
                        if (!result.Success)
                        {
                            LogEmployeeError("SaveTimeBlocksFromTemplates");
                            continue;
                        }


                        result = SaveTransactionsForPeriods(days);
                        if (!result.Success)
                        {
                            LogEmployeeError("SaveTransactionsForPeriods");
                            continue;
                        }

                        if (result.Success)
                            this.currentTransaction.Complete();

                        void LogEmployeeError(string methodName)
                        {
                            LogError(new Exception($"SaveTimeBlocksAndTransactionsFromTemplateAsync, Failed {methodName}, Error: {result.ErrorNumber} ({result.ErrorMessage}) EmployeeNr:{GetEmployeeFromCache(asyncTemplateBlocksByEmployee.Key).EmployeeNr}"));
                        }
                    }
                }
            }
        }

        #endregion

        #region TimeEngineDay operations

        private ActionResult CleanDays(List<AttestEmployeeDaySmallDTO> items)
        {
            ActionResult result = new ActionResult(true);

            if (items.IsNullOrEmpty())
                return result;

            foreach (var itemsByEmployee in items.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(itemsByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                List<TimeEngineDay> days = ConvertToTimeEngineDays(employee, itemsByEmployee.ToList());

                result = CleanDays(employee.EmployeeId, days);
                if (result.Success)
                {
                    var recalculateResult = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Restore, employee.EmployeeId);
                    if (!recalculateResult.Success)
                        return recalculateResult;

                    DoNotifyChangeOfDeviations();
                }
            }

            return result;
        }

        private ActionResult CleanDays(int employeeId, List<TimeEngineDay> days)
        {
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            ActionResult result = new ActionResult(true);

            days = days.GetValidDays(employee, GetEmployeeGroupsFromCache(), out Dictionary<DateTime, bool> dateValidDict);
            if (!days.Any())
                return result;

            var (scheduleBlocks, timeBlocks) = DoLoadData();
            if (DoFilterData())
            {
                DoInitAbsence();
                DoDelete();
                DoTryRestoreUnhandledShiftChanges();
                DoAddCurrentDaysPayrollWarning();
            }
            DoFormatResult();

            return result;

            #region Functions

            (List<TimeScheduleTemplateBlock>, List<TimeBlock>) DoLoadData()
            {
                return
                    (
                        GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, days.GetValidDates(dateValidDict), includeStandBy: true),
                        GetTimeBlocksWithTransactionsAndPayrollTransactionAccount(employee.EmployeeId, days.GetValidTimeBlockDateIds(dateValidDict), onlyActive: false)
                    );
            }
            bool DoFilterData()
            {
                FilterValidDays(employee.EmployeeId, days, dateValidDict, ref scheduleBlocks, ref timeBlocks);
                return days.GetValidTimeBlockDateIds(dateValidDict).Any();
            }
            void DoInitAbsence()
            {
                AddDaysToRestoreAbsenceTracker(days.GetValidDays(dateValidDict), timeBlocks, employee.EmployeeId);
            }
            void DoDelete()
            {
                if (result.Success)
                    result = SetTimeBlockDateDetailsToDeleted(days.GetTimeBlockDates(), SoeTimeBlockDateDetailType.Absence, null, saveChanges: false);
                if (result.Success)
                    result = SetTimeBlocksAndTransactionsToDeleted(timeBlocks, saveChanges: false);
                if (result.Success)
                    result = SetTimePayrollScheduleTransactionsToDeleted(days.GetValidTimeBlockDateIds(dateValidDict), employee.EmployeeId, saveChanges: false);
                if (result.Success)
                    result = SetTimePayrollTransactionsForEmploymentTaxCreditToDeleted(days.GetValidTimeBlockDateIds(dateValidDict), employee.EmployeeId, saveChanges: false);
                if (result.Success)
                    result = Save();
            }
            void DoTryRestoreUnhandledShiftChanges()
            {
                if (result.Success)
                    result = TryRestoreUnhandledShiftsChanges(employee, days.GetValidTimeBlockDateIds(dateValidDict), force: true);
            }
            void DoAddCurrentDaysPayrollWarning()
            {
                AddCurrentDaysPayrollWarning(GetTimeBlockDatesFromCache(employeeId, days.GetValidDates(dateValidDict)));
            }
            void DoFormatResult()
            {
                SetResultValidDays(ref result, days, dateValidDict);
            }

            #endregion
        }

        private void LoadTimeBlockDateAndEmployeeGroupAsMandatory(List<TimeEngineDay> days, int employeeId, Dictionary<DateTime, bool> dateValidDict)
        {
            foreach (TimeEngineDay day in days.GetValidDays(dateValidDict))
            {
                day.CalculatedTimeBlockDate = GetTimeBlockDateFromCache(employeeId, day.Date, true);
                day.CalculatedEmployeeSchedule = GetEmployeeScheduleFromCache(employeeId, day.Date);
                if (day.CalculatedTimeBlockDate == null || day.CalculatedEmployeeSchedule == null)
                    dateValidDict[day.Date] = false;
            }
        }

        private void FilterValidDays(int employeeId, List<TimeEngineDay> days, Dictionary<DateTime, bool> dateValidDict, ref List<TimeScheduleTemplateBlock> scheduleBlocks, ref List<TimeBlock> timeBlocks, bool skipZeroSchedule = false)
        {
            FilterValidDaysByTimeBlockDateStatus(days, dateValidDict);
            if (employeeId != GetCurrentEmployee()?.EmployeeId)
                FilterValidDaysByLended(employeeId, days, dateValidDict, ref scheduleBlocks, ref timeBlocks);
            FilterValidDaysByAttested(days, timeBlocks, dateValidDict);
            if (skipZeroSchedule)
                FilterValidDaysByZeroSchedule(dateValidDict, scheduleBlocks);
            scheduleBlocks = scheduleBlocks.Filter(days.GetValidDates(dateValidDict));
            timeBlocks = timeBlocks.Filter(days.GetValidTimeBlockDateIds(dateValidDict));
        }

        private void FilterValidDaysByTimeBlockDateStatus(List<TimeEngineDay> days, Dictionary<DateTime, bool> dateValidDict)
        {
            foreach (TimeEngineDay day in days.Where(day => day.TimeBlockDate?.Status == (int)SoeTimeBlockDateStatus.Locked))
            {
                if (dateValidDict.ContainsKey(day.Date))
                    dateValidDict[day.Date] = false;
            }
        }

        private void FilterValidDaysByAttested(List<TimeEngineDay> days, List<TimeBlock> timeBlocks, Dictionary<DateTime, bool> dateValidDict)
        {
            int initialAttestStateId = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime)?.AttestStateId ?? 0;

            foreach (TimeEngineDay day in days)
            {
                if (timeBlocks.GetTimePayrollTransactions(day.Date).HasAttestStateNoneInitial(initialAttestStateId))
                {
                    day.HasAttestStateNoneInitial = true;
                    dateValidDict[day.Date] = false;
                }
            }
        }

        private void FilterValidDaysByLended(int employeeId, List<TimeEngineDay> days, Dictionary<DateTime, bool> dateValidDict, ref List<TimeScheduleTemplateBlock> scheduleBlocks, ref List<TimeBlock> timeBlocks)
        {
            if (dateValidDict.IsNullOrEmpty())
                return;

            int accountId = AccountManager.GetAccountHierarchySettingAccountId(entities, UseAccountHierarchy());
            if (accountId == 0)
                return;

            List<DateTime> validDates = dateValidDict.Select(k => k.Key).ToList();
            if (validDates.IsNullOrEmpty())
                return;

            List<AccountDTO> accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            List<DateTime> validDatesByAccounts = AccountManager.GetValidDatesOnGivenAccounts(entities, employeeId, days.GetValidDates(dateValidDict), accountId, accountInternals, scheduleBlocks, timeBlocks.GetTimePayrollTransactions()).Select(k => k.Key).ToList();

            foreach (DateTime invalidDate in validDates.Except(validDatesByAccounts))
            {
                if (dateValidDict.ContainsKey(invalidDate))
                    dateValidDict[invalidDate] = false;
            }
        }

        private void FilterValidDaysByZeroSchedule(Dictionary<DateTime, bool> dateValidDict, List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            List<DateTime> validDates = dateValidDict.Select(k => k.Key).ToList();
            if (validDates.IsNullOrEmpty())
                return;

            foreach (DateTime date in validDates)
            {
                if (scheduleBlocks.Where(b => b.Date == date).All(b => b.StartTime == b.StopTime))
                    dateValidDict[date] = false;
            }
        }

        private void SetResultValidDays(ref ActionResult result, List<TimeEngineDay> days, Dictionary<DateTime, bool> dateValidDict)
        {
            if (days.All(d => d.HasAttestStateNoneInitial))
            {
                result = new ActionResult((int)ActionResultSave.NothingSaved, $"{TermManager.GetDaysTerm(days)} {GetText(9303, "har attesterade transaktioner och kan ej behandlas")}");
            }
            else if (result != null && result.Success && !dateValidDict.IsNullOrEmpty())
            {
                result.Dates = dateValidDict.Where(p => !p.Value).Select(p => p.Key).ToList();
                if (dateValidDict.Values.All(val => !val))
                    result.Success = false;
            }
        }

        #endregion
    }
}
