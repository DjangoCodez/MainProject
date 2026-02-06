using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class TimeBlockManager : ManagerBase
    {
        #region Ctor

        public TimeBlockManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeBlock

        public List<TimeBlock> GetTimeBlocks(CompEntities entities, int employeeId, List<int> timeBlockDateIds, bool loadDeviationAccounts = false, bool loadTimeBlockDate = false, bool loadTimeCode = false, bool loadTimeDeviationCauses = false)
        {
            IQueryable<TimeBlock> query = entities.TimeBlock;
            if (loadTimeBlockDate)
                query = query.Include("TimeBlockDate");
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadTimeDeviationCauses)
            {
                query = query.Include("TimeDeviationCauseStart");
                query = query.Include("TimeDeviationCauseStop");
            }

            var timeBlocks = (from t in query
                              where t.EmployeeId == employeeId &&
                              timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                              t.State == (int)SoeEntityState.Active
                              select t).ToList();

            if (loadDeviationAccounts)
                LoadDeviationAccounts(entities, timeBlocks);

            return timeBlocks;
        }

        public List<TimeBlock> GetTimeBlocksWithNoProjectTimeBlock(List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool loadTimeBlockDate, bool loadTimeCodes, bool loadTimeDeviationCauses, bool includeEmployee, bool includeEmployeeChild)
        {
            var timeBlockDateIds = new List<int>();
            foreach (var employeeId in employeeIds)
            {
                timeBlockDateIds.AddRange(GetTimeBlockDates(employeeId, dateFrom, dateTo).Select(s => s.TimeBlockDateId).ToList());
            }

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlock.NoTracking();
            return GetTimeBlocksWithNoProjectTimeBlock(entities, employeeIds, timeBlockDateIds, loadTimeBlockDate, loadTimeCodes, loadTimeDeviationCauses, includeEmployee, includeEmployeeChild);
        }

        public List<TimeBlock> GetTimeBlocksWithNoProjectTimeBlock(CompEntities entities, List<int> employeeIds, List<int> timeBlockDateIds, bool loadTimeBlockDate, bool loadTimeCodes, bool loadTimeDeviationCauses, bool includeEmployee, bool includeEmployeeChild)
        {
            IQueryable<TimeBlock> query = entities.TimeBlock;
            if (loadTimeBlockDate)
                query = query.Include("TimeBlockDate");
            if (loadTimeCodes)
                query = query.Include("TimeCode");
            if (includeEmployee)
                query = query.Include("Employee.ContactPerson");

            if (loadTimeDeviationCauses)
            {
                query = query.Include("TimeDeviationCauseStart");
                query = query.Include("TimeDeviationCauseStop");
            }

            if (includeEmployeeChild)
            {
                query = query.Include("EmployeeChild");
            }

            var timeBlocks = (from t in query
                              where
                              employeeIds.Contains(t.EmployeeId) &&
                              timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                              !t.ProjectTimeBlockId.HasValue &&
                              t.State == (int)SoeEntityState.Active
                              select t);

            timeBlocks = timeBlocks.Where(t => !t.IsBreak);

            return timeBlocks.ToList();
        }

        public List<TimeBlock> GetTimesWithBlockDatesAndTransactions(CompEntities entities, int employeeId, DateTime dateFrom, DateTime? dateTo = null)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            if (dateTo.HasValue)
                dateTo = CalendarUtility.GetEndOfDay(dateTo.Value);

            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                        .Include("TimeInvoiceTransaction")
                        .Include("TimePayrollTransaction")
                        .Include("TimeCodeTransaction")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    tb.TimeBlockDate.Date >= dateFrom &&
                    (!dateTo.HasValue || tb.TimeBlockDate.Date <= dateTo.Value) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        public List<TimeBlock> GetBreakBlocksForGivenScheduleBreakBlock(TimeScheduleTemplateBlock scheduleBreakBlock, TimeBlockDate timeBlockDate)
        {
            if (scheduleBreakBlock == null || timeBlockDate == null || timeBlockDate.TimeBlockDateId == 0)
                return new List<TimeBlock>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeBlock.NoTracking();

            return (from t in entitiesReadOnly.TimeBlock
                        .Include("TimeScheduleTemplateBlockBreak")
                    where t.TimeScheduleTemplateBlockBreakId == scheduleBreakBlock.TimeScheduleTemplateBlockId &&
                    t.TimeScheduleTemplatePeriodId == scheduleBreakBlock.TimeScheduleTemplatePeriodId &&
                    t.TimeBlockDateId == timeBlockDate.TimeBlockDateId &&
                    t.EmployeeId == timeBlockDate.EmployeeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeBlock> GetTimeBlocks(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from tb in entities.TimeBlock
                    where tb.EmployeeId == employeeId &&
                    tb.TimeBlockDate.EmployeeId == employeeId &&
                    tb.TimeBlockDate.Date >= dateFrom &&
                    tb.TimeBlockDate.Date <= dateTo &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        public List<TimeBlock> GetTimeBlocks(CompEntities entities, int employeeId, List<DateTime> dates)
        {
            return (from tb in entities.TimeBlock
                    where tb.EmployeeId == employeeId &&
                    tb.TimeBlockDate.EmployeeId == employeeId &&
                    dates.Contains(tb.TimeBlockDate.Date) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        public TimeBlock GetTimeBlockDiscardState(int timeBlockId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlock.NoTracking();
            return GetTimeBlockDiscardState(entities, timeBlockId);
        }

        public TimeBlock GetTimeBlockDiscardState(CompEntities entities, int timeBlockId)
        {
            return entities.TimeBlock.Include("TimeCode").FirstOrDefault(t => t.TimeBlockId == timeBlockId);
        }

        public bool HasCompanyActiveTimeBlocks(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlock.NoTracking();
            return (from tb in entities.TimeBlock
                    where tb.Employee.ActorCompanyId == actorCompanyId &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).Any();
        }

        public void LoadDeviationAccounts(CompEntities entities, TimeBlock timeBlock, List<AccountInternal> allAccountInternals = null)
        {
            if (timeBlock == null)
                return;

            LoadDeviationAccounts(entities, timeBlock.ObjToList(), allAccountInternals);
        }

        public void LoadDeviationAccounts(CompEntities entities, List<TimeBlock> timeBlocks, List<AccountInternal> allAccountInternals = null)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            var timeBockDeviationAccountIds = timeBlocks.GetWork(excludeGeneratedFromBreak: true).ToDictionary(tb => tb.TimeBlockId, tb => tb.GetDeviationAccountIds());
            if (!timeBockDeviationAccountIds.Any(kvp => kvp.Value.Any()))
                return;

            var accountInternalIds = timeBockDeviationAccountIds.SelectMany(kvp => kvp.Value).Distinct().ToList();
            var accountInternals = 
                allAccountInternals?.Where(a => accountInternalIds.Contains(a.AccountId)).ToList() ?? 
                AccountManager.GetAccountInternals(entities, accountInternalIds, base.ActorCompanyId, loadAccount: true);

            if (accountInternals.IsNullOrEmpty())
                return;

            foreach (var kvp in timeBockDeviationAccountIds.Where(kvp => !kvp.Value.IsNullOrEmpty()))
            {
                TimeBlock timeBlock = timeBlocks.FirstOrDefault(t => t.TimeBlockId == kvp.Key);
                if (timeBlock != null)
                    timeBlock.DeviationAccounts = accountInternals.Where(a => kvp.Value.Contains(a.AccountId)).ToList();
            }
        }

        #endregion

        #region TimeBlockDate

        public List<TimeBlockDate> GetTimeBlockDates(int employeeId, List<DateTime> dates)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlockDate.NoTracking();
            return GetTimeBlockDates(entities, employeeId, dates);
        }

        public List<TimeBlockDate> GetTimeBlockDates(CompEntities entities, int employeeId, IEnumerable<DateTime> dates, bool loadTimeBlockDateDetails = false)
        {
            var query = from tbd in entities.TimeBlockDate
                        where tbd.EmployeeId == employeeId &&
                        dates.Contains(tbd.Date)
                        select tbd;

            if (loadTimeBlockDateDetails)
                query = query.Include(t => t.TimeBlockDateDetail);

            return query.ToList();
        }

        public List<TimeBlockDate> GetTimeBlockDates(int employeeId, List<int> timeBlockDateIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlockDate.NoTracking();
            return GetTimeBlockDates(entities, employeeId, timeBlockDateIds);
        }

        public List<TimeBlockDate> GetTimeBlockDates(CompEntities entities, int employeeId, List<int> timeBlockDateIds)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(tbd.TimeBlockDateId)
                    select tbd).ToList();
        }

        public List<TimeBlockDate> GetTimeBlockDates(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlockDate.NoTracking();
            return GetTimeBlockDates(entities, employeeId, dateFrom, dateTo);
        }

        public List<TimeBlockDate> GetTimeBlockDates(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    tbd.Date >= dateFrom &&
                    tbd.Date <= dateTo
                    select tbd).ToList();
        }

        public List<TimeBlockDate> GetTimeBlockDates(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime? dateFrom, DateTime? dateTo, List<int> stampingStatusIds = null)
        {
            if (employeeIds.IsNullOrEmpty())
                return new List<TimeBlockDate>();
            return GetTimeBlockDatesQuery(entities, actorCompanyId, employeeIds, dateFrom, dateTo, stampingStatusIds).ToList();
        }

        public IQueryable<TimeBlockDate> GetTimeBlockDatesQuery(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime? dateFrom, DateTime? dateTo, List<int> stampingStatusIds)
        {
            IQueryable<TimeBlockDate> query = (from tbd in entities.TimeBlockDate
                                               where tbd.ActorCompanyId == actorCompanyId
                                               select tbd);

            if (dateFrom.HasValue)
                query = query.Where(tbd => tbd.Date >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(tbd => tbd.Date <= dateTo.Value);
            if (employeeIds != null && (!dateFrom.HasValue && !dateTo.HasValue) || EmployeeManager.UseEmployeeIdsInQuery(entities, employeeIds))
                query = query.Where(tbd => employeeIds.Contains(tbd.EmployeeId));
            if (!stampingStatusIds.IsNullOrEmpty())
                query = query.Where(tbd => stampingStatusIds.Contains(tbd.StampingStatus));

            return query;
        }

        public Dictionary<int, List<DateTime>> GetTimeBlockDatesWithStampingErrors(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            var validEmployeeIds = EmployeeManager.GetEmployeeIdsForQuery(entities, employeeIds);

            var result = GetTimeBlockDatesWithStampingStatus(entities, actorCompanyId, validEmployeeIds, dateFrom, dateTo,
                TermGroup_TimeBlockDateStampingStatus.FirstStampIsNotIn,
                TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps,
                TermGroup_TimeBlockDateStampingStatus.InvalidSequenceOfStamps,
                TermGroup_TimeBlockDateStampingStatus.StampsWithInvalidType,
                TermGroup_TimeBlockDateStampingStatus.AttestedDay,
                TermGroup_TimeBlockDateStampingStatus.InvalidDoubleStamp);

            if (validEmployeeIds == null)
                result = result.Where(r => employeeIds.Contains(r.Key)).ToDictionary(k => k.Key, v => v.Value);

            return result;
        }

        public Dictionary<int, List<DateTime>> GetTimeBlockDatesWithStampingStatus(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, params TermGroup_TimeBlockDateStampingStatus[] stampingStatuses)
        {
            if (stampingStatuses.IsNullOrEmpty())
                return new Dictionary<int, List<DateTime>>();

            List<int> stampingStatusIds = stampingStatuses.Select(status => (int)status).ToList();
            var query = GetTimeBlockDatesQuery(entities, actorCompanyId, employeeIds, dateFrom, dateTo, stampingStatusIds);
            var timeBlockDates = (from tbd in query
                                  select new
                                  {
                                      tbd.EmployeeId,
                                      tbd.Date,
                                  }).ToList();
            var dict = timeBlockDates
                .GroupBy(tbd => tbd.EmployeeId)
                .ToDictionary(k => k.Key, v => v.Select(tbd => tbd.Date).ToList());

            return dict;
        }

        public TimeBlockDate GetTimeBlockDate(int timeBlockDateId, int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlockDate.NoTracking();
            return GetTimeBlockDate(entities, timeBlockDateId, employeeId);
        }

        public TimeBlockDate GetTimeBlockDate(CompEntities entities, int timeBlockDateId, int employeeId)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    tbd.TimeBlockDateId == timeBlockDateId
                    select tbd).FirstOrDefault();
        }

        public TimeBlockDate GetTimeBlockDate(int actorCompanyId, int employeeId, DateTime date, bool createfNotExist = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlockDate.NoTracking();
            return GetTimeBlockDate(entities, actorCompanyId, employeeId, date, createfNotExist);
        }

        public TimeBlockDate GetTimeBlockDate(CompEntities entities, int actorCompanyId, int employeeId, DateTime date, bool createfNotExist = false, List<TimeBlockDate> timeBlockDates = null)
        {
            date = date.Date;
            TimeBlockDate timeBlockDate = timeBlockDates?.FirstOrDefault(tbd => tbd.EmployeeId == employeeId && tbd.Date == date);
            if (timeBlockDate == null)
                timeBlockDate = entities.TimeBlockDate.FirstOrDefault(tbd => tbd.ActorCompanyId == actorCompanyId && tbd.EmployeeId == employeeId && tbd.Date == date);
            if (timeBlockDate == null && createfNotExist)
            {
                timeBlockDate = CreateTimeBlockDate(entities, date, employeeId, actorCompanyId);
                if (timeBlockDate != null && timeBlockDates != null)
                    timeBlockDates.Add(timeBlockDate);
            }
            return timeBlockDate;
        }

        public TimeBlockDateDTO GetTimeBlockDateFromCache(CompEntities entities, int actorCompanyId, int employeeId, DateTime date, bool createfNotExist = false, List<TimeBlockDate> timeBlockDates = null)
        {
            string key = $"GetTimeBlockDateFromCache{actorCompanyId}_{employeeId}_{date.ToShortDateString()}";

            TimeBlockDateDTO timeBlockDateDTO = BusinessMemoryCache<TimeBlockDateDTO>.Get(key);

            if (timeBlockDateDTO != null)
                return timeBlockDateDTO;

            var timeBlockDate = GetTimeBlockDate(entities, actorCompanyId, employeeId, date, createfNotExist, timeBlockDates);

            if (timeBlockDate == null)
                return null;

            BusinessMemoryCache<TimeBlockDateDTO>.Set(key, timeBlockDate.ToDTO(), 60 * 60);

            return timeBlockDate.ToDTO();
        }

        public TimeBlockDate GetTimeBlockDateWithCache(int? timeBlockDateId, int employeeId, ref List<TimeBlockDate> timeBlockDates)
        {
            if (!timeBlockDateId.HasValue)
                return null;

            TimeBlockDate timeBlockDate = timeBlockDates?.FirstOrDefault(i => i.TimeBlockDateId == timeBlockDateId.Value);
            if (timeBlockDate == null)
            {
                timeBlockDate = GetTimeBlockDate(timeBlockDateId.Value, employeeId);
                if (timeBlockDate != null && timeBlockDates != null)
                    timeBlockDates.Add(timeBlockDate);
            }

            return timeBlockDate;
        }

        public TimeBlockDate GetTimeBlockDateFromTimeBlock(CompEntities entities, int timeBlockId)
        {
            return (from tb in entities.TimeBlock
                    where tb.TimeBlockId == timeBlockId
                    select tb.TimeBlockDate).FirstOrDefault();
        }

        public List<TimeBlockDate> CreateAndGetNewTimeBlockDates(CompEntities entities, DateTime fromDate, DateTime toDate, int actorCompanyId, int employeeId)
        {
            List<TimeBlockDate> timeBlockDates = new List<TimeBlockDate>();
            var dates = CalendarUtility.GetDates(fromDate, toDate);
            foreach (var item in dates)
            {
               var tbd= GetTimeBlockDate(entities, actorCompanyId, employeeId, item, true);

                if (tbd != null)
                    timeBlockDates.Add(tbd);
            }
            return timeBlockDates;
        }

        public TimeBlockDate CreateTimeBlockDate(CompEntities entities, DateTime date, EmployeeDTO employee, SoeTimeEngineTask task = 0)
        {
            if (employee == null)
                return null;
            return CreateTimeBlockDate(entities, date, employee.EmployeeId, employee.ActorCompanyId, task: task);
        }

        public TimeBlockDate CreateTimeBlockDate(CompEntities entities, DateTime date, int employeeId, int actorCompanyId, bool addtoEntitiesAndSaveChanges = true, SoeTimeEngineTask task = 0)
        {
            if (actorCompanyId == 0)
                LogError($"CreateTimeBlockDate - actorCompanyId=0,employeeId={employeeId},date={date.ToShortDateString()},taskWatchLogId={(int)task}");

            TimeBlockDate timeBlockDate = new TimeBlockDate()
            {
                Date = date.Date,
                Status = (int)SoeTimeBlockDateStatus.None,
                StampingStatus = (int)TermGroup_TimeBlockDateStampingStatus.NoStamps,
                IsNew = true,

                //Set FK
                EmployeeId = employeeId,
                ActorCompanyId = actorCompanyId,
            };
            timeBlockDate.SetDiscardedBreakEvaluation(false);

            if (addtoEntitiesAndSaveChanges)
            {
                entities.TimeBlockDate.AddObject(timeBlockDate);
                if (!SaveChanges(entities).Success)
                    return null;
            }
            return timeBlockDate;
        }

        public ActionResult CleanDuplicateTimeBlockDates(Company company, string employeeNr, DateTime dateFrom, DateTime dateTo)
        {
            if (company == null)
                return new ActionResult("Company");
            if (dateFrom > dateTo)
                return new ActionResult("Invalid dates");

            using (CompEntities entities = new CompEntities())
            {
                Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, company.ActorCompanyId);
                if (employee == null)
                    return new ActionResult("Employee");

                List<int> timeBlockDateIdsDeleted = new List<int>();
                List<TimeBlockDate> timeBlockDates = entities.TimeBlockDate.Include("TimeBlockDateDetail").Where(e => e.EmployeeId == employee.EmployeeId && e.Date >= dateFrom && e.Date <= dateTo).ToList();
                foreach (var timeBlockDatesByDate in timeBlockDates.OrderBy(tbd => tbd.Date).GroupBy(tbd => tbd.Date))
                {
                    if (timeBlockDatesByDate.Count() <= 1)
                        continue;

                    int timeBlockDateIdToKeep = timeBlockDatesByDate.OrderBy(e => e.TimeBlockDateId).First().TimeBlockDateId;
                    foreach (TimeBlockDate timeBlockDateToDelete in timeBlockDatesByDate.Where(tbd => tbd.TimeBlockDateId != timeBlockDateIdToKeep))
                    {
                        #region Input tables (move and keep)

                        foreach (var e in entities.ProjectTimeBlock.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                        }
                        foreach (var e in entities.TimeStampEntry.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                        }
                        foreach (var e in entities.ExpenseHead.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                        }

                        #endregion

                        #region Outcome tables (move and delete)

                        foreach (var e in entities.TimeBlock.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                            e.State = (int)SoeEntityState.Deleted;
                        }
                        foreach (var e in entities.TimeCodeTransaction.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                            e.State = (int)SoeEntityState.Deleted;
                        }
                        foreach (var e in entities.TimePayrollTransaction.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                            e.State = (int)SoeEntityState.Deleted;
                        }
                        foreach (var e in entities.TimePayrollScheduleTransaction.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                            e.State = (int)SoeEntityState.Deleted;
                        }
                        foreach (var e in entities.TimeInvoiceTransaction.Where(e => e.TimeBlockDateId == timeBlockDateToDelete.TimeBlockDateId))
                        {
                            e.TimeBlockDateId = timeBlockDateIdToKeep;
                            e.State = (int)SoeEntityState.Deleted;
                        }
                        foreach (TimeBlockDateDetail timeBlockDateDetailToDelete in timeBlockDateToDelete.TimeBlockDateDetail)
                        {
                            entities.DeleteObject(timeBlockDateDetailToDelete);
                        }

                        #endregion

                        entities.DeleteObject(timeBlockDateToDelete);
                        timeBlockDateIdsDeleted.Add(timeBlockDateToDelete.TimeBlockDateId);
                    }
                }

                if (!timeBlockDateIdsDeleted.Any())
                    return new ActionResult(true);

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    result.Keys = timeBlockDateIdsDeleted;

                return result;
            }
        }


        #endregion

        #region TimeBlockDateDetail

        public List<TimeAbsenceDetailDTO> GetTimeAbsenceDetails(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeAbsenceDetails(entities, employeeId, dateFrom, dateTo);
        }

        public List<TimeAbsenceDetailDTO> GetTimeAbsenceDetails(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeAbsenceDetailDTO> absenceDetails = new List<TimeAbsenceDetailDTO>();

            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, base.ActorCompanyId, loadContactPerson: true);
            if (employee == null)
                return absenceDetails;

            List<TimeBlockDate> timeBlockDates = entities.TimeBlockDate
                .Include("TimeBlockDateDetail")
                .Where(tbd => tbd.EmployeeId == employeeId && tbd.Date >= dateFrom && tbd.Date <= dateTo)
                .ToList();

            if (timeBlockDates.IsNullOrEmpty())
                return absenceDetails;

            List<HolidayDTO> holidays = base.GetHolidaysFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            Dictionary<DateTime, DayType> dayTypesDict = TimeEngineManager(base.ActorCompanyId, base.UserId).CalculateDayTypesForEmployee(timeBlockDates.Select(tbd => tbd.Date).ToList(), employeeId, false, holidays);
            string ratioNotCalculatedText = GetText(91945, "Omfattning ej beräknad");

            foreach (TimeBlockDate timeBlockDate in timeBlockDates)
            {
                List<TimeBlockDateDetail> timeBlockDateDetails = timeBlockDate.TimeBlockDateDetail.Where(d => d.State == (int)SoeEntityState.Active).ToList();
                if (timeBlockDateDetails.IsNullOrEmpty())
                    continue;

                string dayName = CalendarUtility.GetDayNameFromCulture(timeBlockDate.Date);
                int dayOfWeekNr = CalendarUtility.GetDayNrFromCulture(timeBlockDate.Date);
                int weekNr = CalendarUtility.GetWeekNr(timeBlockDate.Date);
                string weekInfo = GetText(5393, "Vecka:") + " " + weekNr;
                HolidayDTO holiday = holidays?.FirstOrDefault(h => h.Date == timeBlockDate.Date);
                DayTypeDTO dayType = dayTypesDict.GetValue(timeBlockDate.Date)?.ToDTO();

                foreach (var timeBlockDateDetailByRecord in timeBlockDateDetails.GroupBy(d => d.RecordId))
                {
                    TimeDeviationCause timeDeviationCause = timeBlockDateDetailByRecord.Key > 0 ? TimeDeviationCauseManager.GetTimeDeviationCause(entities, timeBlockDateDetailByRecord.Key, base.ActorCompanyId) : null;

                    foreach (var timeBlockDateDetailByOutcome in timeBlockDateDetailByRecord.GroupBy(d => d.OutcomeId))
                    {
                        string sysPayrollTypeLevel3Name = GetText(timeBlockDateDetailByOutcome.Key, (int)TermGroup.SysPayrollType);

                        foreach (TimeBlockDateDetail timeBlockDateDetail in timeBlockDateDetailByOutcome)
                        {
                            absenceDetails.Add(new TimeAbsenceDetailDTO
                            {
                                TimeBlockDateDetailId = timeBlockDateDetail.TimeBlockDateDetailId,
                                TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                EmployeeId = employee.EmployeeId,
                                EmployeeNrAndName = employee.NumberAndName,
                                Date = timeBlockDate.Date,
                                DayName = dayName,
                                DayOfWeekNr = dayOfWeekNr,
                                WeekInfo = weekInfo,
                                WeekNr = weekNr,
                                HolidayId = holiday?.HolidayId,
                                HolidayName = holiday?.Name,
                                IsHoliday = holiday != null,
                                DayTypeId = dayType?.DayTypeId,
                                DayTypeName = dayType?.Name,
                                TimeDeviationCauseId = timeBlockDateDetail.RecordId,
                                TimeDeviationCauseName = timeDeviationCause?.Name,
                                SysPayrollTypeLevel3 = timeBlockDateDetail.OutcomeId,
                                SysPayrollTypeLevel3Name = sysPayrollTypeLevel3Name,
                                Ratio = timeBlockDateDetail.Ratio,
                                RatioText = timeBlockDateDetail.Ratio.HasValue ? timeBlockDateDetail.Ratio.Value.ToString() : ratioNotCalculatedText,
                                ManuallyAdjusted = timeBlockDateDetail.ManuallyAdjusted,
                                Created = timeBlockDateDetail.Created,
                                CreatedBy = timeBlockDateDetail.CreatedBy,
                                Modified = timeBlockDateDetail.Modified,
                                ModifiedBy = timeBlockDateDetail.ModifiedBy,
                            });
                        }
                    }
                }
            }

            return absenceDetails;
        }

        public List<TimeAbsenceDetailDTO> GetTimeAbsenceDetails(CompEntities entities, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeAbsenceDetailDTO> absenceDetails = new List<TimeAbsenceDetailDTO>();

            if (employeeIds == null)
                return absenceDetails;

            List<TimeBlockDate> timeBlockDates = entities.TimeBlockDate
                .Include("TimeBlockDateDetail")
                .Where(tbd => employeeIds.Contains(tbd.EmployeeId) && tbd.Date >= dateFrom && tbd.Date <= dateTo)
                .ToList();

            var employees = entities.Employee.Include("ContactPerson").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

            if (timeBlockDates.IsNullOrEmpty())
                return absenceDetails;

            List<HolidayDTO> holidays = base.GetHolidaysFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            string ratioNotCalculatedText = GetText(91945, "Omfattning ej beräknad");
            List<TimeDeviationCause> timeDeviationCauses = timeBlockDates.Any() ? base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(base.ActorCompanyId)) : null;

            foreach (var groupedOnEmployee in timeBlockDates.GroupBy(g => g.EmployeeId))
            {
                foreach (TimeBlockDate timeBlockDate in groupedOnEmployee)
                {
                    List<TimeBlockDateDetail> timeBlockDateDetails = timeBlockDate.TimeBlockDateDetail.Where(d => d.State == (int)SoeEntityState.Active).ToList();
                    if (timeBlockDateDetails.IsNullOrEmpty())
                        continue;

                    string dayName = CalendarUtility.GetDayNameFromCulture(timeBlockDate.Date);
                    int dayOfWeekNr = CalendarUtility.GetDayNrFromCulture(timeBlockDate.Date);
                    int weekNr = CalendarUtility.GetWeekNr(timeBlockDate.Date);
                    string weekInfo = GetText(5393, "Vecka:") + " " + weekNr;
                    HolidayDTO holiday = holidays?.FirstOrDefault(h => h.Date == timeBlockDate.Date);

                    foreach (var timeBlockDateDetailByRecord in timeBlockDateDetails.GroupBy(d => d.RecordId))
                    {
                        TimeDeviationCause timeDeviationCause = timeBlockDateDetailByRecord.Key > 0 ? timeDeviationCauses.FirstOrDefault(f => f.TimeDeviationCauseId == timeBlockDateDetailByRecord.Key) ?? TimeDeviationCauseManager.GetTimeDeviationCause(entities, timeBlockDateDetailByRecord.Key, base.ActorCompanyId) : null;

                        foreach (var timeBlockDateDetailByOutcome in timeBlockDateDetailByRecord.GroupBy(d => d.OutcomeId))
                        {
                            string sysPayrollTypeLevel3Name = GetText(timeBlockDateDetailByOutcome.Key, (int)TermGroup.SysPayrollType);

                            foreach (TimeBlockDateDetail timeBlockDateDetail in timeBlockDateDetailByOutcome)
                            {

                                absenceDetails.Add(new TimeAbsenceDetailDTO
                                {
                                    TimeBlockDateDetailId = timeBlockDateDetail.TimeBlockDateDetailId,
                                    TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                    EmployeeId = groupedOnEmployee.Key,
                                    EmployeeNrAndName = employees.FirstOrDefault(f => groupedOnEmployee.Key == f.EmployeeId)?.NumberAndName ?? "",
                                    Date = timeBlockDate.Date,
                                    DayName = dayName,
                                    DayOfWeekNr = dayOfWeekNr,
                                    WeekInfo = weekInfo,
                                    WeekNr = weekNr,
                                    //HolidayId = holiday?.HolidayId,
                                    //HolidayName = holiday?.Name,
                                    //IsHoliday = holiday != null,
                                    //DayTypeId = dayType?.DayTypeId,
                                    //DayTypeName = dayType?.Name,
                                    TimeDeviationCauseId = timeBlockDateDetail.RecordId,
                                    TimeDeviationCauseName = timeDeviationCause?.Name,
                                    SysPayrollTypeLevel3 = timeBlockDateDetail.OutcomeId,
                                    SysPayrollTypeLevel3Name = sysPayrollTypeLevel3Name,
                                    Ratio = timeBlockDateDetail.Ratio,
                                    RatioText = timeBlockDateDetail.Ratio.HasValue ? timeBlockDateDetail.Ratio.Value.ToString() : ratioNotCalculatedText,
                                    ManuallyAdjusted = timeBlockDateDetail.ManuallyAdjusted,
                                    Created = timeBlockDateDetail.Created,
                                    CreatedBy = timeBlockDateDetail.CreatedBy,
                                    Modified = timeBlockDateDetail.Modified,
                                    ModifiedBy = timeBlockDateDetail.ModifiedBy,
                                });
                            }
                        }
                    }
                }
            }

            return absenceDetails;
        }


        #endregion
    }
}
