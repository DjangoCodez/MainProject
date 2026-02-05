using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.AnnualLeave.PreFlight;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class AnnualLeaveManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public AnnualLeaveManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region AnnualLeave

        public (DateTime, DateTime, int)? GetAnnualLeaveShiftTimes(DateTime date, int employeeId, int actorCompanyId)
        {
            // Set default start time and length based on agreement type
            DateTime startTime = CalendarUtility.MergeDateAndTime(date, GetAnnualLeaveShiftDefaultStartTime());
            int length = GetAnnualLeaveShiftLengthForEmployee(date, employeeId, actorCompanyId);
            DateTime stopTime = startTime.AddMinutes(length);

            // Check if there are existing shifts that overlaps with the new shift
            // In that case move the new shift to the next available time slot
            List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeScheduleShifts(actorCompanyId, parameterObject.UserId, parameterObject.RoleId, employeeId, startTime.AddDays(-1), stopTime.AddDays(1), null, false, false);
            if (shifts.Count > 0)
            {

                DateTime lastPrevStop = DateTime.MinValue;
                TimeSchedulePlanningDayDTO prevShift = shifts.Where(s => s.StartTime < startTime).OrderByDescending(s => s.StopTime).FirstOrDefault();
                if (prevShift != null)
                {
                    lastPrevStop = prevShift.StopTime;
                }
                DateTime firstNextStart = DateTime.MaxValue;
                TimeSchedulePlanningDayDTO nextShift = shifts.Where(s => s.StartTime >= startTime).OrderBy(s => s.StartTime).FirstOrDefault();
                if (nextShift != null)
                {
                    firstNextStart = nextShift.StartTime;
                }

                TimeSpan gapLength = firstNextStart.Subtract(lastPrevStop);
                if (gapLength.TotalMinutes < length)
                {
                    return null;
                }

                if (lastPrevStop > startTime)
                {
                    // Move shift to begin right after the end of the previous shift
                    startTime = lastPrevStop;
                    stopTime = startTime.AddMinutes(length);
                }
                if (firstNextStart < stopTime)
                {
                    // Move shift to end right before the start of the next shift
                    stopTime = firstNextStart;
                    startTime = stopTime.AddMinutes(-length);
                }
            }

            return (startTime, stopTime, length);
        }

        public ActionResult CreateAnnualLeaveShift(DateTime date, int employeeId, int actorCompanyId)
        {
            // Get employee
            Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, date, loadEmployment: true);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            // Get employment for specified date
            Employment employment = employee.GetEmployment(date);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            // Get annual leave group specified on employment
            int annualLeaveGroupId = employment.GetAnnualLeaveGroupId(date) ?? 0;
            if (annualLeaveGroupId == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(94101, "Årsledighetsavtal ej angivet på aktuell anställning"));

            AnnualLeaveGroup annualLeaveGroup = GetAnnualLeaveGroup(annualLeaveGroupId);
            if (annualLeaveGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(94102, "Årsledighetsavtal hittades inte"));

            // Set default start time, stop time and length based on agreement type and check if shift fits between existing shifts
            (DateTime, DateTime, int)? shiftTimes = GetAnnualLeaveShiftTimes(date, employeeId, actorCompanyId);

            if (!shiftTimes.HasValue)
                return new ActionResult((int)ActionResultSave.DatesInvalid, "Frånvaron får inte plats mellan existerande pass");

            int length = shiftTimes.Value.Item3;

            // Create shift DTO to send to the save method
            TimeSchedulePlanningDayDTO dto = new TimeSchedulePlanningDayDTO();
            dto.Type = TermGroup_TimeScheduleTemplateBlockType.Schedule;
            dto.EmployeeId = employeeId;
            dto.StartTime = shiftTimes.Value.Item1;
            dto.StopTime = shiftTimes.Value.Item2;
            dto.TimeDeviationCauseId = annualLeaveGroup.TimeDeviationCauseId;
            dto.AbsenceType = TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave;
            
            // Save shift
            TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, parameterObject.UserId);
            ActionResult result = tem.SaveTimeScheduleShift("createannualleaveshift", new List<TimeSchedulePlanningDayDTO>() { dto }, false, true, false, 0, null);
            if (!result.Success)
                return result;

            // Extend values and save absence
            dto.AbsenceStartTime = dto.StartTime;
            dto.AbsenceStopTime = dto.StopTime;
            dto.EmployeeId = -1; // to avoid validation error
            dto.ApprovalTypeId = (int)TermGroup_YesNo.Yes;

            EmployeeRequestDTO request = new EmployeeRequestDTO()
            {
                EmployeeId = employeeId,
                ActorCompanyId = actorCompanyId,
                TimeDeviationCauseId = dto.TimeDeviationCauseId,
                Type = TermGroup_EmployeeRequestType.AbsenceRequest,
                Start = dto.StartTime,
                Stop = dto.StartTime,
                Status = TermGroup_EmployeeRequestStatus.RequestPending,
            };

            result = tem.GenerateAndSaveAbsenceFromStaffing(request, new List<TimeSchedulePlanningDayDTO>() { dto }, true, true, null);
            if (!result.Success)
                return result;

            // Save transaction
            result = SaveTransactionSpent(date, employeeId, length, actorCompanyId);

            return result;
        }

        public ActionResult DeleteAnnualLeaveShift(int timeScheduleTemplateBlockId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            // Delete shift
            TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, parameterObject.UserId);
            ActionResult result = tem.DeleteTimeScheduleShifts(new List<int>() { timeScheduleTemplateBlockId }, true, (int?)null, new List<int>());
            if (!result.Success)
                return result;

            // Get shift
            TimeScheduleTemplateBlock block = (from t in entitiesReadOnly.TimeScheduleTemplateBlock where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId select t).FirstOrDefault();

            // Delete transaction
            if (block != null && block.Date.HasValue && block.EmployeeId.HasValue)
                result = DeleteTransactionSpent(block.Date.Value, block.EmployeeId.Value, actorCompanyId);

            return result;
        }

        #endregion

        #region AnnualLeave Balance

        public List<AnnualLeaveBalance> GetAnnualLeaveBalance(DateTime date, List<int> employeeIds, int actorCompanyId)
        {
            List<AnnualLeaveBalance> balances = new List<AnnualLeaveBalance>();
            DateTime startDate = new DateTime(date.Year, 1, 1);

            foreach (int employeeId in employeeIds)
            {
                // Get annual leave group type for current employee
                TermGroup_AnnualLeaveGroupType annualLeaveGroupType = TermGroup_AnnualLeaveGroupType.Unknown;
                AnnualLeaveGroup annualLeaveGroup = GetAnnualLeaveGroupForEmployee(date, employeeId, actorCompanyId);
                if (annualLeaveGroup != null)
                    annualLeaveGroupType = (TermGroup_AnnualLeaveGroupType)annualLeaveGroup.Type;

                // Get existing transactions
                List<AnnualLeaveTransaction> existingTransactions = GetAnnualLeaveTransactions(actorCompanyId, new List<int> { employeeId }, startDate, date, excludeManually: true);
                //List<AnnualLeaveTransaction> existingTransactionsCurrentYear = existingTransactions.Where(t => t.DateEarned >= startDate).ToList();
                decimal days = 0;
                int minutes = 0;
                foreach (AnnualLeaveTransaction trans in existingTransactions)
                {
                    if (trans.Type == (int)TermGroup_AnnualLeaveTransactionType.YearlyBalance)
                    {
                        days += trans.DayBalance;
                        minutes += trans.MinuteBalance;
                    }
                    else
                    {
                        if (trans.DateEarned <= date)
                        {
                            // Transaction was earned before end of current period, increase balance.
                            days++;
                            minutes += trans.MinutesEarned;
                        }

                        if (trans.DateSpent.HasValue)
                        {
                            // Transaction has already been spent, reduce balance
                            days--;
                            minutes -= trans.MinutesSpent;
                        }
                    }
                }

                int additionalBalanceDays = 0;
                int additionalBalanceMinutes = 0;
                if (date > DateTime.Today.AddDays(-1))
                {
                    // If calculating balance for future days, we need to calculate earned days/hours by looking at activated schedule.
                    // The job only stores accumulated minutes every time a day is earned.
                    // Therefore we need to calculate earned minutes from last earned transaction up to end of current period (date parameter passed into this method).

                    // Get last transaction earned for current year
                    DateTime lastDate = startDate;
                    int lastLevel = 0;
                    int lastAccMinutes = 0;
                    AnnualLeaveTransaction lastTrans = existingTransactions.Where(t => t.DateEarned >= startDate).OrderByDescending(t => t.LevelEarned).FirstOrDefault();
                    if (lastTrans != null)
                    {
                        lastDate = lastTrans.DateEarned.Value.AddDays(1);
                        lastLevel = lastTrans.LevelEarned;
                        lastAccMinutes = lastTrans.AccumulatedMinutes;
                    }

                    // Calculate minutes from transactions and/or active schedule between lastDate and date. Send in the lastLevel to the calculation to get the correct next level(s).
                    TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, parameterObject.UserId);
                    List<AnnualLeaveTransactionEarned> transactions = tem.GetAnnualLeaveTransactionsEarned(new List<int> { employeeId }, lastDate, date, lastLevel, lastAccMinutes);

                    foreach (var transaction in transactions.Where(t => t.Level > lastLevel))
                    {
                        additionalBalanceDays += transaction.Days;
                        additionalBalanceMinutes += transaction.Minutes;
                    }

                }

                balances.Add(new AnnualLeaveBalance() { EmployeeId = employeeId, AnnualLeaveBalanceDays = days + additionalBalanceDays, AnnualLeaveBalanceMinutes = minutes + additionalBalanceMinutes });
            }

            return balances;
        }

        public List<AnnualLeaveBalance> RecalculateAnnualLeaveBalance(DateTime date, List<int> employeeIds, int actorCompanyId, bool previousYear = false)
        {
            // Call method that calculates annual leave balance.
            // Date for the calculation is specified in that method (normally yesterday), not this date that is passed in. It's only used for calling GetAnnualLeaveBalance() after recalculated.
            ActionResult calculateResult = CalculateAnnualLeaveTransactions(actorCompanyId, employeeIds, previousYear: previousYear);
            if (!calculateResult.Success)
            {
                // Log error and return empty balance list
                base.LogError(new Exception(calculateResult.ErrorMessage), this.log);
                return new List<AnnualLeaveBalance>();
            }

            return GetAnnualLeaveBalance(date, employeeIds, actorCompanyId);
        }

        #endregion

        #region AnnualLeave Calculation

        public ActionResult CalculateAnnualLeaveTransactions(int actorCompanyId, List<int> employeeIds, DateTime? startDate = null, DateTime? stopDate = null, bool previousYear = false)
        {
            ActionResult result = new ActionResult();
            result.Keys = new List<int>();

            if (employeeIds.IsNullOrEmpty())
            {
                result.Success = false;
                //result.ErrorMessage = GetText(94103, "Inga anställda angivna för beräkning av årsledighetstransaktioner");
                return result;
            }

            // If no dates are specified used current year from 1:st of januari until yesterday.
            if (startDate == null)
                startDate = new DateTime(DateTime.Today.AddDays(-1).Year, 1, 1);
            if (stopDate == null)
                stopDate = DateTime.Today.AddDays(-1);

            if (startDate > stopDate)
            {
                result.Success = false;
                //result.ErrorMessage = GetText(94104, "Period felaktig");
                return result;
            }

            #region Prereq Yearly balance

            DateTime? yearlyBalanceDate = null;

            // check previous year flag and adjust startDate and stopDate
            if (previousYear && stopDate.Value.Year == DateTime.Today.Year)
            {
                startDate = new DateTime(DateTime.Today.AddYears(-1).Year, 1, 1);
                stopDate = new DateTime(DateTime.Today.AddYears(-1).Year, 12, 31);
            }
            // ensure to set previousYear flag if datespan is a valid full year 
            else if (StartAndEndDateIsWholeYear(startDate.Value, stopDate.Value) && stopDate.Value.Year < DateTime.Today.Year)
                previousYear = true;

            if (previousYear)
                yearlyBalanceDate = stopDate.Value.AddDays(1); // should now be 1:st of January

            #endregion

            #region Load data

            TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, parameterObject.UserId);
            List<AnnualLeaveTransactionEarned> transactions = tem.GetAnnualLeaveTransactionsEarned(employeeIds, startDate.Value, stopDate.Value);

            #endregion

            #region Save data

            // Save earned transactions

            // Group on employee
            IEnumerable<IGrouping<int, AnnualLeaveTransactionEarned>> groupedTransactions = transactions.GroupBy(t => t.EmployeeId);
            foreach (var empTransactionGroup in groupedTransactions)
            {
                ActionResult empResult = new ActionResult();
                int employeeId = empTransactionGroup.Key;
                List<AnnualLeaveTransactionEarned> earnedTransactions = empTransactionGroup.Select(g => g).OrderBy(t => t.Level).ToList();
                int maxLevelEarned = earnedTransactions.Max(t => t.Level);

                // Create a new database transaction for each employee
                using (CompEntities entities = new CompEntities())
                {
                    try
                    {
                        entities.Connection.Open();

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            // Get existing transactions
                            List<AnnualLeaveTransaction> existingTransactions = GetAnnualLeaveTransactions(entities, employeeId, actorCompanyId, excludeManually: true, excludeYearly: true);
                            List<AnnualLeaveTransaction> existingTransactionsCurrentYear = existingTransactions.Where(t => t.DateEarned >= startDate).ToList();

                            bool employeeModified = false;
                            foreach (AnnualLeaveTransactionEarned earnedTransaction in earnedTransactions)
                            {
                                // Check if a transaction already exists on this level.
                                // If it does, update earned values if they have changed.
                                AnnualLeaveTransaction trans = existingTransactionsCurrentYear.FirstOrDefault(t => t.LevelEarned == earnedTransaction.Level);
                                if (trans != null)
                                {
                                    if (trans.DateEarned != earnedTransaction.Date ||
                                        trans.MinutesEarned != earnedTransaction.Minutes ||
                                        trans.AccumulatedMinutes != earnedTransaction.AccumulatedMinutes)
                                    {
                                        trans.DateEarned = earnedTransaction.Date;
                                        trans.MinutesEarned = earnedTransaction.Minutes;
                                        trans.AccumulatedMinutes = earnedTransaction.AccumulatedMinutes;
                                        SetModifiedProperties(trans);
                                        SetTransactionBalanceFields(trans);
                                        employeeModified = true;
                                    }
                                }
                                else
                                {
                                    // No transaction exists, check if any spent (in advance) transaction exists.
                                    // If it does, update it with earned values.
                                    trans = existingTransactions.Where(t => t.DateSpent.HasValue && !t.DateEarned.HasValue).OrderBy(t => t.DateSpent).FirstOrDefault();
                                    if (trans != null)
                                    {
                                        SetModifiedProperties(trans);
                                    }
                                    else
                                    {
                                        // No transaction exists, create a new one and set earned values.
                                        trans = new AnnualLeaveTransaction()
                                        {
                                            ActorCompanyId = actorCompanyId,
                                            EmployeeId = employeeId,
                                        };
                                        SetCreatedProperties(trans);
                                        AddEntityItem(entities, trans, "AnnualLeaveTransaction", transaction);
                                    }

                                    trans.LevelEarned = earnedTransaction.Level;
                                    trans.DateEarned = earnedTransaction.Date;
                                    trans.MinutesEarned = earnedTransaction.Minutes;
                                    trans.AccumulatedMinutes = earnedTransaction.AccumulatedMinutes;
                                    employeeModified = true;

                                    SetTransactionBalanceFields(trans);
                                }
                            }

                            // Final check, transaction exists on a level that is no longer reached.
                            // If it has spent values set, remove the earned values, otherwise remove the whole transaction.
                            if (existingTransactionsCurrentYear.Any() && existingTransactionsCurrentYear.Max(t => t.LevelEarned) > maxLevelEarned)
                            {
                                foreach (AnnualLeaveTransaction trans in existingTransactionsCurrentYear.Where(t => t.LevelEarned > maxLevelEarned).ToList())
                                {
                                    if (trans.DateSpent.HasValue)
                                    {
                                        trans.LevelEarned = 0;
                                        trans.DateEarned = null;
                                        trans.MinutesEarned = 0;
                                        trans.AccumulatedMinutes = 0;
                                        SetModifiedProperties(trans);
                                        SetTransactionBalanceFields(trans);
                                    }
                                    else
                                    {
                                        ChangeEntityState(entities, trans, SoeEntityState.Deleted, false);
                                    }
                                }
                                employeeModified = true;
                            }

                            empResult = SaveChanges(entities, transaction);
                            if (empResult.Success)
                            {
                                // ActionResult.Keys will contain employeeIds that has been modified.
                                if (employeeModified)
                                    result.Keys.Add(employeeId);
                                transaction.Complete();
                            }
                            else
                            {
                                // ActionResult.StrDict will contain employeeIds and error messages of failed employees.
                                result.StrDict.Add(employeeId, empResult.ErrorMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);
                        empResult = new ActionResult(ex);
                    }
                    finally
                    {
                        if (empResult == null || !empResult.Success)
                            base.LogTransactionFailed(this.ToString(), this.log);

                        entities.Connection.Close();
                    }
                }
            }

            #endregion

            #region Yearly balance

            if (previousYear)
            {
                List<int> employeesWithoutTransactions = new List<int>();
                var transactionsPreviousYearGrouped = GetAnnualLeaveTransactions(actorCompanyId, employeeIds, startDate.Value, stopDate.Value).GroupBy(t => t.EmployeeId);

                // Find employees without any transactions in the period. Used for fallback case, if need to remove an existing yearly balance transaction.
                foreach (int employeeId in employeeIds)
                {
                    if (!transactionsPreviousYearGrouped.Any(g => g.Key == employeeId))
                    {
                        employeesWithoutTransactions.Add(employeeId);
                    }
                }

                foreach (var transactionsPreviousYearGroup in transactionsPreviousYearGrouped)
                {
                    ActionResult empResult = null;

                    int employeeId = transactionsPreviousYearGroup.Key;
                    int balanceDays = 0;
                    int balanceMinutes = 0;

                    // Create a new database transaction for each employee
                    using (CompEntities entities = new CompEntities())
                    {
                        try
                        {
                            entities.Connection.Open();

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                #region Calculate

                                foreach (AnnualLeaveTransaction transactionPreviousYear in transactionsPreviousYearGroup)
                                {
                                    if (transactionPreviousYear.Type == (int)TermGroup_AnnualLeaveTransactionType.YearlyBalance)
                                    {
                                        balanceDays += transactionPreviousYear.DayBalance;
                                        balanceMinutes += transactionPreviousYear.MinuteBalance;
                                    }
                                    else
                                    {
                                        if (TransactionEarnedBetweenDates(transactionPreviousYear, startDate.Value, stopDate.Value))
                                        {
                                            balanceDays++; // Add DayBalance value instead?
                                            balanceMinutes += transactionPreviousYear.MinutesEarned;
                                        }
                                        if (TransactionSpentBetweenDates(transactionPreviousYear, startDate.Value, stopDate.Value))
                                        {
                                            balanceDays--; // Reduce DayBalance value instead?
                                            balanceMinutes -= transactionPreviousYear.MinutesSpent;
                                        }
                                    }
                                }

                                #endregion

                                #region Save

                                // check if existing yearly balance transaction exists. If it does, update it. Else create new.
                                List<AnnualLeaveTransaction> yearlyTransactions = GetAnnualLeaveTransactions(entities, actorCompanyId, employeeIds, yearlyBalanceDate.Value, yearlyBalanceDate.Value, excludeManually: true);
                                yearlyTransactions = yearlyTransactions.Where(t => t.Type == (int)TermGroup_AnnualLeaveTransactionType.YearlyBalance).ToList();
                                AnnualLeaveTransaction yearlyTransaction = yearlyTransactions.FirstOrDefault();

                                // if having any duplicate yearly balance transactions, remove the extras
                                if (yearlyTransactions.Count > 1)
                                {
                                    foreach (AnnualLeaveTransaction duplicate in yearlyTransactions.Where(t => t.AnnualLeaveTransactionId != yearlyTransaction.AnnualLeaveTransactionId))
                                    {
                                        ChangeEntityState(entities, duplicate, SoeEntityState.Deleted, true);
                                    }
                                }

                                if (yearlyTransaction != null)
                                {
                                    SetModifiedProperties(yearlyTransaction);
                                }
                                else
                                {
                                    yearlyTransaction = new AnnualLeaveTransaction()
                                    {
                                        ActorCompanyId = actorCompanyId,
                                        EmployeeId = employeeId,
                                    };
                                    SetCreatedProperties(yearlyTransaction);
                                    AddEntityItem(entities, yearlyTransaction, "AnnualLeaveTransaction", transaction);
                                }

                                yearlyTransaction.DateEarned = yearlyBalanceDate;
                                yearlyTransaction.DayBalance = balanceDays;
                                yearlyTransaction.MinuteBalance = balanceMinutes;
                                yearlyTransaction.Type = (int)TermGroup_AnnualLeaveTransactionType.YearlyBalance;

                                empResult = SaveChanges(entities, transaction);
                                if (empResult.Success)
                                {
                                    // ActionResult.Keys will contain employeeIds that has been modified.
                                    result.Keys.Add(employeeId);
                                    transaction.Complete();
                                }
                                else
                                {
                                    // ActionResult.StrDict will contain employeeIds and error messages of failed employees.
                                    result.StrDict.Add(employeeId, empResult.ErrorMessage);
                                }

                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            base.LogError(ex, this.log);
                            empResult = new ActionResult(ex);
                        }
                        finally
                        {
                            if (empResult == null || !empResult.Success)
                                base.LogTransactionFailed(this.ToString(), this.log);

                            entities.Connection.Close();
                        }
                    }
                }

                // Fallback case, remove any existing yearly balance transaction for employees without any transactions in the period.
                if (employeesWithoutTransactions.Any())
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        // get only yearly balance transactions for these employees
                        var yearlyBalanceTransactionsGrouped = GetAnnualLeaveTransactions(entities, actorCompanyId, employeesWithoutTransactions, yearlyBalanceDate.Value, yearlyBalanceDate.Value).Where(t => t.IsYearlyBalance).GroupBy(t => t.EmployeeId);

                        foreach (var yearlyBalanceTransactionGroup in yearlyBalanceTransactionsGrouped)
                        {
                            ActionResult transResult = new ActionResult();

                            try
                            {
                                // Create a new database transaction for each employee
                                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                                {
                                    entities.Connection.Open();

                                    #region Delete

                                    foreach (AnnualLeaveTransaction yearlyBalanceTransaction in yearlyBalanceTransactionGroup)
                                    {
                                        ChangeEntityState(entities, yearlyBalanceTransaction, SoeEntityState.Deleted, false);

                                        transResult = SaveChanges(entities, transaction);
                                        if (transResult.Success)
                                        {
                                            transaction.Complete();
                                            transResult.IntegerValue = yearlyBalanceTransaction != null ? yearlyBalanceTransaction.AnnualLeaveTransactionId : 0;
                                        }
                                    }

                                    #endregion
                                }
                            }
                            catch (Exception ex)
                            {
                                base.LogError(ex, this.log);
                                transResult = new ActionResult(ex);
                            }
                            finally
                            {
                                if (transResult == null || !transResult.Success)
                                    base.LogTransactionFailed(this.ToString(), this.log);

                                entities.Connection.Close();
                            }
                        }
                    }
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region AnnualLeaveGroup

        public List<AnnualLeaveGroup> GetAnnualLeaveGroups(int actorCompanyId, int? annualLeaveGroupId = null, bool setTypeNames = false, bool includeTimeDeviationCause = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveGroup.NoTracking();
            return GetAnnualLeaveGroups(entities, actorCompanyId, annualLeaveGroupId, setTypeNames, includeTimeDeviationCause);
        }

        public List<AnnualLeaveGroup> GetAnnualLeaveGroups(CompEntities entities, int actorCompanyId, int? annualLeaveGroupId = null, bool setTypeNames = false, bool includeTimeDeviationCause = false)
        {
            IQueryable<AnnualLeaveGroup> query = (from alg in entities.AnnualLeaveGroup
                                                  where alg.ActorCompanyId == actorCompanyId &&
                                                  alg.State == (int)SoeEntityState.Active
                                                  select alg);

            if (includeTimeDeviationCause)
                query = query.Include("TimeDeviationCause");

            if (annualLeaveGroupId.HasValue)
                query = query.Where(t => t.AnnualLeaveGroupId == annualLeaveGroupId.Value);

            List<AnnualLeaveGroup> groups = query.ToList();

            if (setTypeNames)
            {
                List<GenericType> types = base.GetTermGroupContent(TermGroup.AnnualLeaveGroupType);
                foreach (AnnualLeaveGroup group in groups)
                {
                    group.TypeName = types.Where(t => t.Id == group.Type).Select(t => t.Name).FirstOrDefault();
                }
            }

            return groups;
        }

        public Dictionary<int, string> GetAnnualLeaveGroupsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            IEnumerable<AnnualLeaveGroup> annualLeaveGroups = GetAnnualLeaveGroups(actorCompanyId);
            foreach (AnnualLeaveGroup group in annualLeaveGroups)
            {
                dict.Add(group.AnnualLeaveGroupId, group.Name);
            }

            return dict;
        }

        public AnnualLeaveGroup GetAnnualLeaveGroup(int annualLeaveGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveGroup.NoTracking();
            return GetAnnualLeaveGroup(entities, annualLeaveGroupId);
        }

        public AnnualLeaveGroup GetAnnualLeaveGroup(CompEntities entities, int annualLeaveGroupId)
        {
            return entities.AnnualLeaveGroup
                .Where(t => t.AnnualLeaveGroupId == annualLeaveGroupId && t.State == (int)SoeEntityState.Active)
                .FirstOrDefault();
        }

        public AnnualLeaveGroup GetAnnualLeaveGroupForEmployee(DateTime date, int employeeId, int actorCompanyId)
        {
            AnnualLeaveGroup annualLeaveGroup = null;

            // Get employee
            Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, date, loadEmployment: true);
            if (employee != null)
            {
                // Get employment for specified date
                Employment employment = employee.GetEmployment(date);
                if (employment != null)
                {
                    // Get annual leave group specified on employment
                    employment.ApplyEmploymentChanges(date);
                    annualLeaveGroup = employment.GetAnnualLeaveGroup(date);
                }
            }

            return annualLeaveGroup;
        }

        public List<AnnualLeaveGroupLimitDTO> GetAnnualLeaveGroupLimits(TermGroup_AnnualLeaveGroupType type)
        {
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            return annualLeaveCalculation.GetAnnualLeaveGroupLimits(type);
        }

        private void AddLimitToList(List<AnnualLeaveGroupLimitDTO> limits, int workedHours, int nbrOfDaysAnnualLeave, double nbrOfHoursAnnualLeave)
        {
            var annualLeaveCalculation = new AnnualLeaveCalculation();
            annualLeaveCalculation.AddLimitToList(limits, workedHours, nbrOfDaysAnnualLeave, nbrOfHoursAnnualLeave);
        }

        public ActionResult SaveAnnualLeaveGroup(AnnualLeaveGroupDTO annualLeaveGroupInput, int actorCompanyId)
        {
            if (annualLeaveGroupInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(94102, "Årsledighetsavtal hittades inte"));

            // Default result is successful
            ActionResult result = null;

            int annualLeaveGroupId = annualLeaveGroupInput.AnnualLeaveGroupId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region AnnualLeaveGroup

                        // Get existing annual leave group
                        AnnualLeaveGroup annualLeaveGroup = GetAnnualLeaveGroup(entities, annualLeaveGroupId);
                        if (annualLeaveGroup == null)
                        {
                            #region AnnualLeaveGroup Add

                            annualLeaveGroup = new AnnualLeaveGroup()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(annualLeaveGroup);
                            entities.AnnualLeaveGroup.AddObject(annualLeaveGroup);

                            #endregion
                        }
                        else
                        {
                            #region AnnualLeaveGroup Update

                            SetModifiedProperties(annualLeaveGroup);

                            #endregion
                        }

                        annualLeaveGroup.Name = annualLeaveGroupInput.Name;
                        annualLeaveGroup.Type = (int)annualLeaveGroupInput.Type;
                        annualLeaveGroup.Description = annualLeaveGroupInput.Description;
                        annualLeaveGroup.QualifyingDays = annualLeaveGroupInput.QualifyingDays;
                        annualLeaveGroup.QualifyingMonths = annualLeaveGroupInput.QualifyingMonths;
                        annualLeaveGroup.GapDays = annualLeaveGroupInput.GapDays;
                        annualLeaveGroup.State = (int)annualLeaveGroupInput.State;
                        annualLeaveGroup.TimeDeviationCauseId = annualLeaveGroupInput.TimeDeviationCauseId;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            annualLeaveGroupId = annualLeaveGroup.AnnualLeaveGroupId;
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
                    {
                        //Set success properties
                        result.IntegerValue = annualLeaveGroupId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteAnnualLeaveGroup(int annualLeaveGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check relations
                if (AnnualLeaveGroupHasEmployments(annualLeaveGroupId))
                    return new ActionResult((int)ActionResultDelete.AnnualLeaveGroupHasEmployments, GetText(11910, "Anställning"));

                #endregion

                AnnualLeaveGroup annualLeaveGroup = GetAnnualLeaveGroup(entities, annualLeaveGroupId);
                if (annualLeaveGroup == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(94102, "Årsledighetsavtal hittades inte"));

                return ChangeEntityState(entities, annualLeaveGroup, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region AnnualLeaveTransaction

        public ActionResult SaveTransactionEarned(int actorCompanyId, AnnualLeaveTransactionEarned transaction)
        {
            return SaveTransactionEarned(transaction.Date, transaction.EmployeeId, transaction.Minutes, transaction.AccumulatedMinutes, actorCompanyId);
        }

        public ActionResult SaveTransactionEarned(DateTime date, int employeeId, int minutes, int ackumulatedMinutes, int actorCompanyId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Check if there are any spent transactions not earned yet (spent in advance)
                        // Get the earliest transaction (if any)
                        AnnualLeaveTransaction trans = GetAnnualLeaveTransactions(entities, employeeId, actorCompanyId, excludeManually: true, excludeYearly: true).Where(t => !t.DateEarned.HasValue && t.DateSpent.HasValue).OrderBy(t => t.DateSpent).FirstOrDefault();
                        
                        if (trans == null)
                        {
                            #region Add

                            trans = new AnnualLeaveTransaction()
                            {
                                ActorCompanyId = actorCompanyId,
                                EmployeeId = employeeId,
                            };
                            SetCreatedProperties(trans);
                            entities.AnnualLeaveTransaction.AddObject(trans);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(trans);

                            #endregion
                        }

                        trans.DateEarned = date;
                        trans.MinutesEarned = minutes;

                        SetTransactionBalanceFields(trans);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            result.IntegerValue = trans.AnnualLeaveTransactionId;
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
                    if (result == null || !result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }


                return result;
            }
        }

        public ActionResult SaveTransactionSpent(DateTime date, int employeeId, int minutes, int actorCompanyId, bool manuallySpent = false)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Fetch transactions
                        List<AnnualLeaveTransaction> transactions = GetAnnualLeaveTransactions(entities, employeeId, actorCompanyId, excludeManually: true, excludeYearly: true).ToList();

                        // Check if transactions need to be swapped to get in correct DateSpent order
                        checkSwapTransaction(date, minutes);

                        // Check if there are any earned transactions not spent yet
                        // Get the earliest transaction (if any)
                        AnnualLeaveTransaction trans = transactions.Where(t => t.DateEarned.HasValue && !t.DateSpent.HasValue).OrderBy(t => t.DateEarned).FirstOrDefault();

                        if (trans == null)
                        {
                            #region Add

                            trans = new AnnualLeaveTransaction()
                            {
                                ActorCompanyId = actorCompanyId,
                                EmployeeId = employeeId,
                            };
                            SetCreatedProperties(trans);
                            entities.AnnualLeaveTransaction.AddObject(trans);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(trans);

                            #endregion
                        }

                        trans.DateSpent = date;
                        trans.MinutesSpent = minutes;
                        trans.ManuallySpent = manuallySpent;

                        SetTransactionBalanceFields(trans);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            result.IntegerValue = trans.AnnualLeaveTransactionId;
                        }

                        void checkSwapTransaction(DateTime transSwapDate, int spentSwapMinutes)
                        {
                            //Check for any spent later than this date, if any found then do a switch
                            AnnualLeaveTransaction firstSpentAfter = transactions.OrderBy(t => t.DateSpent).FirstOrDefault(t => t.DateSpent.HasValue && t.DateSpent.Value > transSwapDate);
                            if (firstSpentAfter != null)
                            {
                                // get current values on that transaction
                                DateTime tempDate = firstSpentAfter.DateSpent.Value;
                                int tempMinutes = firstSpentAfter.MinutesSpent;

                                // set values on the found transaction
                                firstSpentAfter.DateSpent = transSwapDate;
                                firstSpentAfter.MinutesSpent = spentSwapMinutes;

                                SetModifiedProperties(firstSpentAfter);
                                SetTransactionBalanceFields(firstSpentAfter);

                                // set back to use on the new transaction
                                date = tempDate;
                                minutes = tempMinutes;

                                checkSwapTransaction(date, minutes);
                            }
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
                    if (result == null || !result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }


                return result;
            }
        }

        public ActionResult DeleteTransactionSpent(DateTime date, int employeeId, int actorCompanyId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Check if there are any earned transactions not spent yet
                        AnnualLeaveTransaction spent = GetAnnualLeaveTransactions(entities, employeeId, actorCompanyId, date, excludeManually: true, excludeYearly: true).FirstOrDefault();
                        if (spent != null)
                        {
                            if (spent.DateEarned.HasValue)
                            {
                                #region Update

                                // Transaction has earned values, just clear the spent values
                                spent.DateSpent = null;
                                spent.MinutesSpent = 0;

                                SetModifiedProperties(spent);
                                SetTransactionBalanceFields(spent);

                                #endregion
                            }
                            else
                            {
                                #region Delete

                                // Transaction only has spent values, delete it
                                result = ChangeEntityState(entities, spent, SoeEntityState.Deleted, false);

                                #endregion
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            result.IntegerValue = spent != null ? spent.AnnualLeaveTransactionId : 0;
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
                    if (result == null || !result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }


                return result;
            }
        }

        public AnnualLeaveTransaction GetAnnualLeaveTransaction(int annualLeaveTransactionId, bool includeEmployee = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveTransaction.NoTracking();
            return GetAnnualLeaveTransaction(entities, annualLeaveTransactionId, includeEmployee);
        }

        public AnnualLeaveTransaction GetAnnualLeaveTransaction(CompEntities entities, int annualLeaveTransactionId, bool includeEmployee = false)
        {
            IQueryable<AnnualLeaveTransaction> query = entities.AnnualLeaveTransaction;

            if (includeEmployee)
                query = query.Include("Employee.ContactPerson");

            return query
                .Where(t => t.AnnualLeaveTransactionId == annualLeaveTransactionId && t.State == (int)SoeEntityState.Active)
                .FirstOrDefault();
        }

        private List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(int employeeId, int actorCompanyId, DateTime? dateSpent = null, bool excludeManually = false, bool excludeYearly = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveTransaction.NoTracking();
            return GetAnnualLeaveTransactions(entities, employeeId, actorCompanyId, dateSpent, excludeManually, excludeYearly);
        }

        private List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(CompEntities entities, int employeeId, int actorCompanyId, DateTime? dateSpent = null, bool excludeManually = false, bool excludeYearly = false)
        {
            IQueryable<AnnualLeaveTransaction> query = (from t in entities.AnnualLeaveTransaction
                                                        where t.ActorCompanyId == actorCompanyId &&
                                                        t.EmployeeId == employeeId &&
                                                        t.State == (int)SoeEntityState.Active
                                                        select t);

            if (dateSpent.HasValue)
                query = query.Where(t => t.DateSpent == dateSpent.Value);

            List<AnnualLeaveTransaction> transactions = query.ToList();

            if (excludeManually)
                transactions = transactions.Where(t => !t.IsManually).ToList();

            if (excludeYearly)
                transactions = transactions.Where(t => !t.IsYearlyBalance).ToList();

            return transactions;
        }

        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(int actorCompanyId, DateTime? dateSpent = null, bool excludeManually = false, bool excludeYearly = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveTransaction.NoTracking();
            return GetAnnualLeaveTransactions(entities, actorCompanyId, dateSpent, excludeManually, excludeYearly);
        }

        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(CompEntities entities, int actorCompanyId, DateTime? dateSpent = null, bool excludeManually = false, bool excludeYearly = false)
        {
            IQueryable<AnnualLeaveTransaction> query = (from t in entities.AnnualLeaveTransaction
                                                        where t.ActorCompanyId == actorCompanyId &&
                                                        t.State == (int)SoeEntityState.Active
                                                        select t);

            if (dateSpent.HasValue)
                query = query.Where(t => t.DateSpent == dateSpent.Value);

            List<AnnualLeaveTransaction> transactions = query.ToList();

            if (excludeManually)
                transactions = transactions.Where(t => !t.IsManually).ToList();

            if (excludeYearly)
                transactions = transactions.Where(t => !t.IsYearlyBalance).ToList();

            return transactions;
        }

        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool includeEmployee = false, bool excludeManually = false, bool excludeYearly = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveTransaction.NoTracking();
            return GetAnnualLeaveTransactions(entities, actorCompanyId, employeeIds, dateFrom, dateTo, includeEmployee, excludeManually, excludeYearly);
        }
        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactions(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool includeEmployee = false, bool excludeManually = false, bool excludeYearly = false)
        {
            IQueryable<AnnualLeaveTransaction> query = entities.AnnualLeaveTransaction;
            if (includeEmployee)
                query = query.Include("Employee.ContactPerson");

            query = (from t in query
                     where t.ActorCompanyId == actorCompanyId &&
                     t.State == (int)SoeEntityState.Active &&
                     employeeIds.Contains(t.EmployeeId) &&
                     ((t.DateEarned.HasValue && t.DateEarned.Value >= dateFrom && t.DateEarned.Value <= dateTo) ||
                         (t.DateSpent.HasValue && t.DateSpent.Value >= dateFrom && t.DateSpent.Value <= dateTo))
                     select t);

            List<AnnualLeaveTransaction> transactions = query.ToList();

            if (excludeManually)
                transactions = transactions.Where(t => !t.IsManually).ToList();

            if (excludeYearly)
                transactions = transactions.Where(t => !t.IsYearlyBalance).ToList();

            // set type names
            int langId = GetLangId();
            Dictionary<int, string> typeDict = base.GetTermGroupDict(TermGroup.AnnualLeaveTransactionType, langId);

            foreach (AnnualLeaveTransaction transaction in transactions)
            {
                transaction.TypeName = GetValueFromDict(transaction.Type, typeDict);
            }

            return transactions;
        }

        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactionsManuallyEarned(int actorCompanyId, DateTime dateStart, DateTime dateStop, List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AnnualLeaveTransaction.NoTracking();
            return GetAnnualLeaveTransactionsManuallyEarned(entities, actorCompanyId, dateStart, dateStop, employeeIds);
        }

        public List<AnnualLeaveTransaction> GetAnnualLeaveTransactionsManuallyEarned(CompEntities entities, int actorCompanyId, DateTime dateStart, DateTime dateStop, List<int> employeeIds)
        {
            IQueryable<AnnualLeaveTransaction> query = (from t in entities.AnnualLeaveTransaction
                                                        where t.ActorCompanyId == actorCompanyId &&
                                                        t.State == (int)SoeEntityState.Active &&
                                                        employeeIds.Contains(t.EmployeeId) &&
                                                        t.DateEarned >= dateStart &&
                                                        t.DateEarned <= dateStop &&
                                                        t.Type == (int)TermGroup_AnnualLeaveTransactionType.ManuallyEarned
                                                        select t);
            return query.ToList();
        }

        public ActionResult SaveManuallySpentOnCalculatedTransaction(int actorCompanyId, AnnualLeaveTransactionEditDTO annualLeaveTransactionInput)
        {
            return SaveTransactionSpent(annualLeaveTransactionInput.DateSpent.Value, annualLeaveTransactionInput.EmployeeId, annualLeaveTransactionInput.MinutesSpent, actorCompanyId, true);
        }

        public ActionResult SaveAnnualLeaveTransaction(AnnualLeaveTransactionEditDTO annualLeaveTransactionInput, int actorCompanyId)
        {
            if (annualLeaveTransactionInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(94104, "Årsledighetstransaktion kunde inte hittas"));

            if (annualLeaveTransactionInput.Type == TermGroup_AnnualLeaveTransactionType.ManuallySpent)
                return SaveManuallySpentOnCalculatedTransaction(actorCompanyId, annualLeaveTransactionInput);

            // Default result is successful
            ActionResult result = null;

            int annualLeaveTransactionId = annualLeaveTransactionInput.AnnualLeaveTransactionId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region AnnualLeaveTransaction

                        // Get existing annual leave group
                        AnnualLeaveTransaction annualLeaveTransaction = GetAnnualLeaveTransaction(entities, annualLeaveTransactionId);
                        if (annualLeaveTransaction == null)
                        {
                            #region AnnualLeaveTransaction Add

                            annualLeaveTransaction = new AnnualLeaveTransaction()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(annualLeaveTransaction);
                            entities.AnnualLeaveTransaction.AddObject(annualLeaveTransaction);

                            #endregion
                        }
                        else
                        {
                            #region AnnualLeaveTransaction Update

                            SetModifiedProperties(annualLeaveTransaction);

                            #endregion
                        }

                        annualLeaveTransaction.EmployeeId = annualLeaveTransactionInput.EmployeeId;
                        annualLeaveTransaction.Type = (int)annualLeaveTransactionInput.Type;
                        annualLeaveTransaction.LevelEarned = annualLeaveTransactionInput.LevelEarned;
                        annualLeaveTransaction.DateEarned = annualLeaveTransactionInput.DateEarned;
                        annualLeaveTransaction.MinutesEarned = annualLeaveTransactionInput.MinutesEarned;
                        annualLeaveTransaction.AccumulatedMinutes = annualLeaveTransactionInput.AccumulatedMinutes;
                        annualLeaveTransaction.DateSpent = annualLeaveTransactionInput.DateSpent;
                        annualLeaveTransaction.MinutesSpent = annualLeaveTransactionInput.MinutesSpent;
                        annualLeaveTransaction.ManuallyAdded = true; // always true when added/edited manually
                        annualLeaveTransaction.DayBalance = 0;
                        annualLeaveTransaction.MinuteBalance = annualLeaveTransactionInput.Type == TermGroup_AnnualLeaveTransactionType.ManuallyEarned ? annualLeaveTransactionInput.MinutesEarned : -annualLeaveTransactionInput.MinutesSpent;
                        annualLeaveTransaction.ManuallyEarned = annualLeaveTransactionInput.Type == TermGroup_AnnualLeaveTransactionType.ManuallyEarned;

                        // TODO: If previous year, recalculate the yearly balance transaction row if any.


                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            annualLeaveTransactionId = annualLeaveTransaction.AnnualLeaveTransactionId;
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
                    {
                        //Set success properties
                        result.IntegerValue = annualLeaveTransactionId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                if (result.Success)
                {
                    // recalculate transactions
                    result = CalculateAnnualLeaveTransactions(actorCompanyId, new List<int>() { annualLeaveTransactionInput.EmployeeId });
                }

                return result;
            }
        }

        public ActionResult DeleteAnnualLeaveTransaction(int annualLeaveTransactionId, bool onlyManually = true)
        {
            using (CompEntities entities = new CompEntities())
            {
                AnnualLeaveTransaction annualLeaveTransaction = GetAnnualLeaveTransaction(entities, annualLeaveTransactionId);
                if (annualLeaveTransaction == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(94104, "Årsledighetstransaktion kunde inte hittas"));
                else if (!annualLeaveTransaction.IsManually && onlyManually)
                    return new ActionResult((int)ActionResultDelete.NothingDeleted, GetText(94105, "Årsledighetstransaktion av denna typ kan ej tas bort"));

                // don't really delete calculated transactions that are manually spent and having earned-data, just blank the spent-values
                if (annualLeaveTransaction.Type == (int)TermGroup_AnnualLeaveTransactionType.Calculated && annualLeaveTransaction.ManuallySpent && annualLeaveTransaction.DateEarned != null && annualLeaveTransaction.MinutesEarned > 0)
                    return ClearManuallySpentAnnualLeaveTransaction(annualLeaveTransactionId);

                ActionResult result = ChangeEntityState(entities, annualLeaveTransaction, SoeEntityState.Deleted, true);

                if (result.Success)
                {
                    // TODO: Calculate AccumulatedMinutes? LevelEarned? DayBalance? MinuteBalance?
                    // recalculate transactions
                    result = CalculateAnnualLeaveTransactions(annualLeaveTransaction.ActorCompanyId, new List<int>() { annualLeaveTransaction.EmployeeId });
                }

                return result;
            }
        }

        public ActionResult ClearManuallySpentAnnualLeaveTransaction(int annualLeaveTransactionId)
        {
            ActionResult result = null;
            int recalculateForEmployeeId = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        AnnualLeaveTransaction annualLeaveTransaction = GetAnnualLeaveTransaction(entities, annualLeaveTransactionId);

                        if (annualLeaveTransaction == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(94104, "Årsledighetstransaktion kunde inte hittas"));

                        // Clear manually spent values
                        annualLeaveTransaction.DateSpent = null;
                        annualLeaveTransaction.MinutesSpent = 0;
                        annualLeaveTransaction.ManuallySpent = false;
                        SetModifiedProperties(annualLeaveTransaction);
                        SetTransactionBalanceFields(annualLeaveTransaction);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            recalculateForEmployeeId = annualLeaveTransaction.EmployeeId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    return new ActionResult(ex);
                }
                finally
                {
                    if (result == null || !result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                if (recalculateForEmployeeId != 0)
                {
                    // recalculate transactions
                    result = CalculateAnnualLeaveTransactions(ActorCompanyId, new List<int>() { recalculateForEmployeeId });
                }

                return result;
            }
        }

        #endregion

        #region AnnualLeave Settings

        public List<int> GetCompanyIdsWithAnnualLeaveSetting()
        {
            return SettingManager.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.UseAnnualLeave, true);
        }

        #endregion

        #region Helpers

        private bool AnnualLeaveGroupHasEmployments(int annualLeaveGroupId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.Employment.Any(s => s.OriginalAnnualLeaveGroupId == annualLeaveGroupId && s.State == (int)SoeEntityState.Active);
        }

        private TimeSpan GetAnnualLeaveShiftDefaultStartTime()
        {
            return TimeSpan.FromHours(8);
        }

        private int GetAnnualLeaveShiftLength(TermGroup_AnnualLeaveGroupType type)
        {
            switch (type)
            {
                case TermGroup_AnnualLeaveGroupType.Commercial:
                case TermGroup_AnnualLeaveGroupType.HotelRestaurant:
                    return (int)(7.5 * 60); // 7.5 hours in minutes
                case TermGroup_AnnualLeaveGroupType.HotelRestaurantNight:
                    return 6 * 60; // 6 hours in minutes
                default:
                    return 0;
            }
        }

        public int GetAnnualLeaveShiftLengthForEmployee(DateTime date, int employeeId, int actorCompanyId)
        {
            // First check if there is any existing earned transaction that has not been spent yet.
            // If it does, take the earned length from that transaction.
            int length = GetAnnualLeaveTransactions(employeeId, actorCompanyId, excludeManually: true, excludeYearly: true)
                .Where(t => t.DateEarned.HasValue && !t.DateSpent.HasValue)
                .OrderBy(t => t.DateEarned)
                .Select(t => t.MinutesEarned)
                .FirstOrDefault();

            if (length == 0)
            {
                // No existing earned transaction, so get the length from the annual leave group for the employee.
                AnnualLeaveGroup annualLeaveGroup = GetAnnualLeaveGroupForEmployee(date, employeeId, actorCompanyId);
                if (annualLeaveGroup != null)
                    length = GetAnnualLeaveShiftLength((TermGroup_AnnualLeaveGroupType)annualLeaveGroup.Type);
            }

            return length;
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }

        public void SetTransactionBalanceFields(AnnualLeaveTransaction transaction)
        {
            int minutesDiff = transaction.MinutesEarned - transaction.MinutesSpent;
            transaction.MinuteBalance = minutesDiff;

            int daysDiff = (transaction.MinutesEarned > 0 ? 1 : 0) - (transaction.MinutesSpent > 0 ? 1 : 0);
            transaction.DayBalance = transaction.Type == (int)TermGroup_AnnualLeaveTransactionType.ManuallyEarned ? 0 : daysDiff;
        }

        private bool StartAndEndDateIsWholeYear(DateTime startDate, DateTime stopDate)
        {
            return (startDate.Year == stopDate.Year && startDate.Month == 1 && startDate.Day == 1 && stopDate.Month == 12 && stopDate.Day == 31);
        }

        private bool TransactionEarnedBetweenDates(AnnualLeaveTransaction trans, DateTime startDate, DateTime stopDate)
        {
            return (trans.DateEarned != null && trans.DateEarned.Value >= startDate && trans.DateEarned.Value <= stopDate);
        }

        private bool TransactionSpentBetweenDates(AnnualLeaveTransaction trans, DateTime startDate, DateTime stopDate)
        {
            return (trans.DateSpent != null && trans.DateSpent.Value >= startDate && trans.DateSpent.Value <= stopDate);
        }

        #endregion
    }
}
