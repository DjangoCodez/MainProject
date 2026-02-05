using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private ImportHolidaysOutputDTO TaskImportHolidays()
        {
            var (iDTO, oDTO) = InitTask<ImportHolidaysInputDTO, ImportHolidaysOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO.Holidays == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull);
                return oDTO;
            }

            List<Tuple<int, int, bool>> tuples = new List<Tuple<int, int, bool>>();

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        foreach (HolidayDTO holidayInput in iDTO.Holidays.Where(h => h.DayType != null))
                        {
                            if (DayTypeExists(holidayInput.DayType.Name))
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.DayTypeExists);
                                return oDTO;
                            }                                
                            if (HolidayExists(holidayInput.Date, holidayInput.DayTypeId))
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.HolidayExists);
                                return oDTO;
                            }                                

                            DayType dayType = CreateDayType(holidayInput.DayType);
                            Holiday holiday = CreateHoliday(holidayInput, dayType);

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            if (iDTO.UpdateSchedules)
                                tuples.Add(Tuple.Create(holiday.HolidayId, holidayInput.DayTypeId, false));
                        }

                        #endregion

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

            #region Save UniqueDays

            CreateTaskInput(new SaveUniqueDayInputDTO(tuples));
            oDTO.Result = PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;

            #endregion

            return oDTO;
        }

        /// <summary>
        /// Calculates DayType for a Employee and date
        /// </summary>
        /// <returns>Output DTO</returns>
        private CalculateDayTypeForEmployeeOutputDTO TaskCalculateDayTypeForEmployee()
        {
            var (iDTO, oDTO) = InitTask<CalculateDayTypeForEmployeeInputDTO, CalculateDayTypeForEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.DayType = GetDayTypeForEmployeeFromCache(iDTO.EmployeeId, iDTO.Date, iDTO.DoNotCheckHoliday);
            }

            return oDTO;
        }

        /// <summary>
        /// Calculates DayType for a Employee and dates
        /// </summary>
        /// <returns>Output DTO</returns>
        private CalculateDayTypesForEmployeeOutputDTO TaskCalculateDayTypesForEmployee()
        {
            var (iDTO, oDTO) = InitTask<CalculateDayTypesForEmployeeInputDTO, CalculateDayTypesForEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                Dictionary<DateTime, DayType> dayTypesDict = new Dictionary<DateTime, DayType>();

                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId);
                if (employee != null)
                {
                    foreach (DateTime date in iDTO.Dates)
                    {
                        if (dayTypesDict.ContainsKey(date))
                            continue;

                        DayType dayType = GetDayTypeForEmployeeFromCache(employee, date, iDTO.DoNotCheckHoliday);
                        if (dayType != null)
                            dayTypesDict.Add(date, dayType);
                    }
                }

                oDTO.DayTypes = dayTypesDict;
            }

            return oDTO;
        }

        /// <summary>
        /// Calculates DayTypes for a Employees and dates
        /// </summary>
        /// <returns>Output DTO</returns>
        private CalculateDayTypeForEmployeeOutputDTO TaskCalculateDayTypeForEmployees()
        {
            try
            {
                var (iDTO, oDTO) = InitTask<CalculateDayTypeForEmployeesInputDTO, CalculateDayTypeForEmployeeOutputDTO>();
                if (!oDTO.Result.Success)
                    return oDTO;

                ClearCachedContent();

                using (CompEntities taskEntities = new CompEntities())
                {
                    InitContext(taskEntities);

                    if (iDTO.Employee != null)
                        CalculateDayTypeForEmployee(iDTO.Dtos, iDTO.DoNotCheckHoliday, iDTO.CompanyHolidays, iDTO.CompanyDayTypes, iDTO.Employee);
                    else if (iDTO.Dtos.Select(s => s.EmployeeId).Distinct().Count() == 1)
                        CalculateDayTypeForEmployee(iDTO.Dtos, iDTO.DoNotCheckHoliday, iDTO.CompanyHolidays, iDTO.CompanyDayTypes, GetEmployeeFromCache(iDTO.Dtos.FirstOrDefault()?.EmployeeId ?? 0));
                    else
                        CalculateDayTypeForEmployees(iDTO.Dtos, iDTO.DoNotCheckHoliday, iDTO.CompanyHolidays, iDTO.CompanyDayTypes);
                }

                return oDTO;
            }
            catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Saves unique days
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveUniqueDayOutputDTO TaskSaveUniqueDay()
        {
            var (iDTO, oDTO) = InitTask<SaveUniqueDayInputDTO, SaveUniqueDayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

                        //Order so the "removeOnly" gets processed first (false is sorted first in ascending)
                        foreach (Tuple<int, int, bool> tuple in iDTO.Tuples.OrderByDescending(i => i.Item3).ToList())
                        {
                            int holidayId = tuple.Item1;
                            int dayTypeId = tuple.Item2;
                            bool removeOnly = tuple.Item3;

                            HolidayDTO holiday = GetHolidayWithDayTypeDiscardedStateFromCache(holidayId);
                            if (holiday == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");
                                return oDTO;
                            }

                            List<Employee> employees = GetEmployeesForCompanyWithEmployment();
                            foreach (Employee employee in employees.Where(e => e.State == (int)SoeEntityState.Active))
                            {
                                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(holiday.Date, employeeGroups: employeeGroups);
                                if (employeeGroup == null)
                                    continue;

                                if (!employeeGroup.DayType.IsLoaded)
                                    employeeGroup.DayType.Load();
                                DayType dayType = employeeGroup.DayType.FirstOrDefault(i => i.DayTypeId == dayTypeId);
                                if (dayType == null)
                                    continue;

                                if (!employee.EmployeeSchedule.IsLoaded)
                                    employee.EmployeeSchedule.Load();
                                EmployeeSchedule employeeSchedule = employee.EmployeeSchedule.Get(holiday.Date);
                                if (employeeSchedule == null)
                                    continue;

                                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, holiday.Date);
                                if (timeBlockDate != null && HasAttestedDeviations(timeBlockDate.TimeBlockDateId, employee.EmployeeId))
                                    continue;

                                List<HolidayDTO> holidaysToAdd = new List<HolidayDTO>();
                                if (!removeOnly)
                                    holidaysToAdd.Add(holiday);

                                if (timeBlockDate != null)
                                    oDTO.Result = SetScheduleAndDeviationsToDeleted(employee.EmployeeId, holiday.Date, timeBlockDate.TimeBlockDateId, saveChanges: false);
                                else
                                    oDTO.Result = SetScheduleAndDeviationsToDeleted(employee.EmployeeId, holiday.Date, holiday.Date, saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                List<TimeScheduleTemplateBlockReference> references = CreateTimeScheduleTemplateBlocks(employee, employeeSchedule, holiday.Date, holiday.Date, holidays: holidaysToAdd);
                                iDTO.AsyncTemplateBlocks.AddRange(references.GetTemplateBlocks(true));
                                
                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                        }

                        if (!iDTO.CreateTimeBlocksAndTransactionsAsync)
                            SaveTimeBlocksAndTransactionsFromTemplate(iDTO.AsyncTemplateBlocks);

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

            #region Create TimeBlocks and transactions (async)

            if (oDTO.Result.Success && iDTO.CreateTimeBlocksAndTransactionsAsync && iDTO.AsyncTemplateBlocks.Any())
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SaveTimeBlocksAndTransactionsFromTemplateAsync(iDTO)));

            #endregion

            return oDTO;
        }

        /// <summary>
        /// Updates unique days from halfday
        /// </summary>
        /// <returns>Output DTO</returns>
        private UpdateUniqueDayFromHalfDayOutputDTO TaskUpdateUniqueDayFromHalfDay()
        {
            var (iDTO, oDTO) = InitTask<UpdateUniqueDayFromHalfDayInputDTO, UpdateUniqueDayFromHalfDayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Prereq

            List<Tuple<int, int, bool>> tuples = new List<Tuple<int, int, bool>>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                TimeHalfday halfday = GetTimeHalfdayWithDayTypeAndHoliday(iDTO.TimeHalfDayId);
                if (halfday?.DayType == null || halfday.DayType.Holiday.IsNullOrEmpty())
                {
                    oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeHalfday");
                    return oDTO;
                }

                bool changedDayType = halfday.DayTypeId != iDTO.DayTypeId;

                //Remove/Update unique days
                foreach (Holiday holiday in halfday.DayType.Holiday.Where(h => h.State == (int)SoeEntityState.Active))
                {
                    //Remove only if changed DayType, will be re-added below
                    tuples.Add(Tuple.Create<int, int, bool>(holiday.HolidayId, halfday.DayType.DayTypeId, changedDayType));
                }

                //Add unique days
                if (changedDayType)
                {
                    //Holidays to add unique days for
                    List<HolidayDTO> holidaysForDayType = GetHolidaysFromDayType(iDTO.DayTypeId);
                    foreach (HolidayDTO holiday in holidaysForDayType)
                    {
                        tuples.Add(Tuple.Create<int, int, bool>(holiday.HolidayId, iDTO.DayTypeId, false));
                    }
                }
            }

            #endregion

            #region Perform

            CreateTaskInput(new SaveUniqueDayInputDTO(tuples));
            oDTO.Result = PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;

            #endregion

            return oDTO;
        }

        /// <summary>
        /// Save unique days from halfday
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveUniqueDayFromHalfDayOutputDTO TaskSaveUniqueDayFromHalfDay()
        {
            var (iDTO, oDTO) = InitTask<SaveUniqueDayFromHalfDayInputDTO, SaveUniqueDayFromHalfDayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Prereq

            List<Tuple<int, int, bool>> tuples = new List<Tuple<int, int, bool>>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                TimeHalfday halfday = GetTimeHalfdayWithDayTypeAndHoliday(iDTO.TimeHalfdayId);
                if (halfday?.DayType == null || halfday.DayType.Holiday.IsNullOrEmpty())
                {
                    oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeHalfday");
                    return oDTO;
                }

                //Save unique days
                foreach (Holiday holiday in halfday.DayType.Holiday)
                {
                    tuples.Add(Tuple.Create<int, int, bool>(holiday.HolidayId, halfday.DayTypeId, iDTO.RemoveOnly));
                }
            }

            #endregion

            #region Perform

            CreateTaskInput(new SaveUniqueDayInputDTO(tuples, false));
            oDTO.Result = PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;

            #endregion

            return oDTO;
        }

        /// <summary>
        /// Add unique day from holiday
        /// </summary>
        /// <returns>Output DTO</returns>
        private AddUniqueDayFromHolidayOutputDTO TaskAddUniqueDayFromHoliday()
        {
            var (iDTO, oDTO) = InitTask<AddUniqueDayFromHolidayInputDTO, AddUniqueDayFromHolidayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            CreateTaskInput(new SaveUniqueDayInputDTO(iDTO.HolidayId, iDTO.DayTypeId, false));
            oDTO.Result = PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;

            return oDTO;
        }

        /// <summary>
        /// Update unique day from holiday
        /// </summary>
        /// <returns>Output DTO</returns>
        private UpdateUniqueDayFromHolidayOutputDTO TaskUpdateUniqueDayFromHoliday()
        {
            var (iDTO, oDTO) = InitTask<UpdateUniqueDayFromHolidayInputDTO, UpdateUniqueDayFromHolidayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            //If old date is null, the date for the holiday is already handled above
            if (iDTO.OldDateToDelete.HasValue)
            {
                //RemoveUniqueDayFromHoliday (remove old)
                CreateTaskInput(new DeleteUniqueDayFromHolidayInputDTO(iDTO.HolidayId, iDTO.DayTypeId, iDTO.OldDateToDelete, false));
                oDTO.Result = PerformTask(SoeTimeEngineTask.DeleteUniqueDayFromHoliday).Result;
            }

            CreateTaskInput(new SaveUniqueDayInputDTO(iDTO.HolidayId, iDTO.DayTypeId, false));
            oDTO.Result = PerformTask(SoeTimeEngineTask.SaveUniqueday).Result;

            return oDTO;
        }

        /// <summary>
        /// Delete unique day from holiday
        /// </summary>
        /// <returns>Output DTO</returns>
        private DeleteUniqueDayFromHolidayOutputDTO TaskDeleteUniqueDayFromHoliday()
        {
            var (iDTO, oDTO) = InitTask<DeleteUniqueDayFromHolidayInputDTO, DeleteUniqueDayFromHolidayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        //If we have an oldDateToDelete we will not use holidayId. The only time have a oldDateToDelete is when we have changed the date for a holiday. Then we have to restore the employees schedules on that old date.
                        DateTime date;
                        DateTime? oldDateToDelete = iDTO.OldDateToDelete;
                        if (oldDateToDelete.HasValue)
                        {
                            date = oldDateToDelete.Value;
                        }
                        else
                        {
                            //Get unique day
                            HolidayDTO holiday = GetHolidayWithDayTypeDiscardedStateFromCache(iDTO.HolidayId);
                            if (holiday == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");
                                return oDTO;
                            }

                            date = holiday.Date;
                        }

                        #endregion

                        #region Perform

                        List<Employee> employees = GetEmployeesForCompanyWithEmployment();
                        foreach (Employee employee in employees.Where(e => e.State == (int)SoeEntityState.Active))
                        {
                            if (!employee.EmployeeSchedule.IsLoaded)
                                employee.EmployeeSchedule.Load();
                            EmployeeSchedule employeeSchedule = employee.EmployeeSchedule.Get(date);
                            if (employeeSchedule == null)
                                continue;

                            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date);
                            if (timeBlockDate != null && HasAttestedDeviations(timeBlockDate.TimeBlockDateId, employee.EmployeeId))
                                continue;

                            if (timeBlockDate != null)
                                oDTO.Result = SetScheduleAndDeviationsToDeleted(employee.EmployeeId, date, timeBlockDate.TimeBlockDateId, saveChanges: false);
                            else
                                oDTO.Result = SetScheduleAndDeviationsToDeleted(employee.EmployeeId, date, date, saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            List<TimeScheduleTemplateBlockReference> references = CreateTimeScheduleTemplateBlocks(employee, employeeSchedule, date, date);
                            iDTO.AsyncTemplateBlocks.AddRange(references.GetTemplateBlocks(true));

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        if (!iDTO.CreateTimeBlocksAndTransactionsAsync)
                            SaveTimeBlocksAndTransactionsFromTemplate(iDTO.AsyncTemplateBlocks);

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

            //Create TimeBlocks and transactions (async)
            if (oDTO.Result.Success && iDTO.CreateTimeBlocksAndTransactionsAsync && iDTO.AsyncTemplateBlocks.Any())
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SaveTimeBlocksAndTransactionsFromTemplateAsync(iDTO)));

            return oDTO;
        }

        /// <summary>
        /// Creates transactions for earned holiday
        /// </summary>
        /// <returns></returns>
        private CreateTransactionsForEarnedHolidayOutputDTO TaskCreateTransactionsForEarnedHoliday()
        {
            var (iDTO, oDTO) = InitTask<CreateTransactionsForEarnedHolidayInputDTO, CreateTransactionsForEarnedHolidayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);
                entities.CommandTimeout = 500;

                try
                {
                    #region Prereq

                    HolidayDTO holiday = GetHoliday(iDTO.HolidayId, iDTO.Year);
                    if (holiday == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8740, "Helgdag kunde inte hittas"));
                        return oDTO;
                    }

                    List<Employee> employees = GetEmployeesWithEmployment(iDTO.EmployeeIds);
                    if (employees.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8743, "Inga anställda hittades"));
                        return oDTO;
                    }

                    List<PayrollGroup> payrollGroups = GetPayrollGroupsWithSettingsFromCache();

                    foreach (Employee employee in employees)
                    {
                        Employment employment = employee.GetEmployment(holiday.Date);
                        if (employment == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8692, "Anställning på datum {0} saknas."), holiday.Date.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                            return oDTO;
                        }

                        PayrollGroup payrollGroup = employment.GetPayrollGroup(holiday.Date, payrollGroups);
                        if (payrollGroup == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(11035, "Löneavtal på datum {0} saknas."), holiday.Date.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                            return oDTO;
                        }

                        PayrollGroupSetting setting = PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.EarnedHoliday, payrollGroups: payrollGroups);
                        if (setting == null || !setting.BoolData.HasValue || !setting.BoolData.Value)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, "Avtalet omfattas av intjänade röda dagar(HRF) ej aktiverat. " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                            return oDTO;
                        }
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION(new TimeSpan(1, 0, 0))))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = CreateTransactionsForEarnedHoliday(employees, iDTO.HolidayId, iDTO.Year);
                        if (!oDTO.Result.Success)
                            return oDTO;

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
        /// Deletes transactions for earned holiday
        /// </summary>
        /// <returns></returns>
        private DeleteTransactionsForEarnedHolidayOutputDTO TaskDeleteTransactionsForEarnedHoliday()
        {
            var (iDTO, oDTO) = InitTask<DeleteTransactionsForEarnedHolidayInputDTO, DeleteTransactionsForEarnedHolidayOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    HolidayDTO holiday = GetHoliday(iDTO.HolidayId, iDTO.Year);
                    if (holiday == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8740, "Helgdag kunde inte hittas"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForDayAndEarnedHoliday(iDTO.EmployeeIds, holiday.Date);
                        List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();
                        timePayrollTransactions.ForEach(x => timeCodeTransactions.Add(x.TimeCodeTransaction));

                        oDTO.Result = SetTimeCodeTransactionsToDeleted(timeCodeTransactions, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges: false, discardCheckes: true);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

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
    }
}
