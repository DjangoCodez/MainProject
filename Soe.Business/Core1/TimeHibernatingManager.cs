using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class TimeHibernatingManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeHibernatingManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeHibernatingAbsence

        public ActionResult CreateTimeHibernatingAbsence(CompEntities entities, Employee employee, Employment temporaryPrimaryEmployment, int? timeDeviationCauseId, DateTime batchCreated)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
            if (temporaryPrimaryEmployment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            TimeHibernatingAbsenceHead hibernatingHead = CreateTimeHibernatingAbsenceHead(employee, temporaryPrimaryEmployment, timeDeviationCauseId, batchCreated);
            temporaryPrimaryEmployment.TimeHibernatingAbsenceHead.Add(hibernatingHead);

            return CreateHibernatingAbsenceRowsAndClearData(entities, hibernatingHead, batchCreated);
        }

        public ActionResult ShortenHibernatingAbsence(out TimeHibernatingAbsenceHead hibernatingHead, Employee employee, Employment temporaryPrimaryEmployment, DateTime batchCreated, bool deleteHead)
        {
            hibernatingHead = null;

            List<Employment> hibernatingEmployments = employee.GetHibernatingEmployments(temporaryPrimaryEmployment);
            if (hibernatingEmployments.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval);

            hibernatingHead = temporaryPrimaryEmployment.GetHibernatingHead();
            if (hibernatingHead == null)
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryHibernatingHeadNotFound);

            var result = SetTimeHibernatingAbsenceRowsToDeleted(hibernatingHead, batchCreated);
            if (result.Success && deleteHead)
                result = ChangeEntityState(hibernatingHead, SoeEntityState.Deleted, modified: batchCreated);
            return result;
        }

        public ActionResult ExtendHibernatingAbsence(CompEntities entities, out TimeHibernatingAbsenceHead hibernatingHead, EmploymentDateChange dateChangeInterval, Employee employee, Employment temporaryPrimaryEmployment, DateTime batchCreated)
        {
            hibernatingHead = null;

            List<Employment> hibernatingEmployments = employee.GetHibernatingEmployments(temporaryPrimaryEmployment);
            if (hibernatingEmployments.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval);

            hibernatingHead = temporaryPrimaryEmployment.GetHibernatingHead();
            if (hibernatingHead == null)
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryHibernatingHeadNotFound);

            return CreateHibernatingAbsenceRowsAndClearData(entities, hibernatingHead, batchCreated, dateChangeInterval.IntervalDateFrom, dateChangeInterval.IntervalDateTo);
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return GetScheduleBlocksForHibernation(entities, hibernatingHead.EmployeeId, dateFrom, dateTo);
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForHibernation(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return TimeScheduleManager.GetTimeScheduleTemplateBlocks(entities, employeeId, dateFrom, dateTo, includeBreaks: true, includePrel: true);
        }

        private List<TimeSchedulePlanningDayDTO> GetTemplateScheduleBlocksForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            if (dateFrom > dateTo)
                return new List<TimeSchedulePlanningDayDTO>();

            return TimeScheduleManager.GetTimeSchedulePlanningDaysFromTemplate(entities, hibernatingHead.ActorCompanyId, base.RoleId, base.UserId, dateFrom, dateTo, null, employeeIds: hibernatingHead.EmployeeId.ObjToList());

        }

        private List<TimeBlock> GetTimeBlocksForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return TimeBlockManager.GetTimeBlocks(entities, hibernatingHead.EmployeeId, dateFrom, dateTo);
        }

        private List<TimeCodeTransaction> GetTimeCodeTransactionsForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return TimeTransactionManager.GetTimeCodeTransactionsForEmployee(entities, hibernatingHead.EmployeeId, dateFrom, dateTo);
        }

        private List<TimePayrollTransaction> GetPayrollTransactionsForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return TimeTransactionManager.GetTimePayrollTransactionsForEmployee(entities, hibernatingHead.EmployeeId, dateFrom, dateTo, loadTimeBlockDate: true);
        }

        private List<TimePayrollScheduleTransaction> GetScheduleTransactionsForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return TimeTransactionManager.GetTimePayrollScheduleTransactionsForEmployee(entities, hibernatingHead.EmployeeId, dateFrom, dateTo);
        }

        private List<EmployeeSchedule> GetEmployeeSchedulesForHiberation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo)
        {
            return TimeScheduleManager.GetEmployeeSchedulesForEmployee(entities, hibernatingHead.EmployeeId, dateFrom, dateTo, hibernatingHead.ActorCompanyId);
        }

        private EmployeeSchedule CreateNewEmployeeScheduleForHibernating(CompEntities entities, EmployeeSchedule prototype, DateTime startDate, DateTime stopDate)
        {
            if (prototype == null || startDate > stopDate)
                return null;

            EmployeeSchedule employeeSchedule = new EmployeeSchedule
            {
                StartDate = startDate,
                StopDate = stopDate,
                EmployeeId = prototype.EmployeeId,
                TimeScheduleTemplateHeadId = prototype.TimeScheduleTemplateHeadId,
                TimeScheduleTemplateGroupId = prototype.TimeScheduleTemplateGroupId,
                IsPreliminary = prototype.IsPreliminary,
                ModifiedWithNoCheckes = prototype.ModifiedWithNoCheckes
            };
            SetCreatedProperties(employeeSchedule);
            entities.AddToEmployeeSchedule(employeeSchedule);
            return employeeSchedule;
        }

        private void ClearDataForHibernation(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime dateFrom, DateTime dateTo, DateTime batchCreated, List<TimeScheduleTemplateBlock> scheduleBlocks = null, List<TimePayrollTransaction> transactions = null)
        {
            if (hibernatingHead == null)
                return;

            if (scheduleBlocks == null)
                scheduleBlocks = GetScheduleBlocksForHibernation(entities, hibernatingHead, dateFrom, dateTo);
            if (transactions == null)
                transactions = GetPayrollTransactionsForHibernation(entities, hibernatingHead, dateFrom, dateTo);

            List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedulesForHiberation(entities, hibernatingHead, dateFrom, dateTo);
            List<TimeBlock> timeBlocks = GetTimeBlocksForHibernation(entities, hibernatingHead, dateFrom, dateTo);
            List<TimeCodeTransaction> timeCodeTransactions = GetTimeCodeTransactionsForHibernation(entities, hibernatingHead, dateFrom, dateTo);
            List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = GetScheduleTransactionsForHibernation(entities, hibernatingHead, dateFrom, dateTo);

            foreach (var e in employeeSchedules)
            {
                if (e.StartDate >= dateFrom && e.StopDate <= dateTo)
                {
                    //Completely overlapped by temporary Employment - delete
                    ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
                }
                if (e.StartDate < dateFrom && e.StopDate > dateTo)
                {
                    //Completely overlapping temporary Employment - end and create new
                    EmployeeSchedule created = CreateNewEmployeeScheduleForHibernating(entities, e, dateTo.AddDays(1), e.StopDate);
                    if (created != null)
                    {
                        List<TimeScheduleTemplateBlock> scheduleBlockForCreated = GetScheduleBlocksForHibernation(entities, created.EmployeeId, created.StartDate, created.StopDate);
                        TimeScheduleManager.MoveScheduleBlocksToEmployeeSchedule(entities, created, scheduleBlockForCreated, modified: batchCreated);
                        e.UpdateStop(dateFrom.AddDays(-1));
                        SetModifiedProperties(e, modified: batchCreated);
                    }
                }
                if (e.StartDate < dateFrom && e.StopDate <= dateTo)
                {
                    //Is partly overlapped by temporary Employment at stop - move stop backward
                    e.UpdateStop(dateFrom.AddDays(-1));
                    SetModifiedProperties(e, modified: batchCreated);
                }
                if (e.StopDate > dateTo && e.StartDate >= dateFrom)
                {
                    //Is partly overlapped by temporary Employment at start - move start forward
                    e.UpdateStart(dateTo.AddDays(+1));
                    SetModifiedProperties(e, modified: batchCreated);
                }
            }
            foreach (var e in scheduleBlocks)
            {
                ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
            }
            foreach (var e in transactions)
            {
                ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
            }
            foreach (var e in timeBlocks)
            {
                ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
            }
            foreach (var e in timeCodeTransactions)
            {
                ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
            }
            foreach (var e in timePayrollScheduleTransactions)
            {
                ChangeEntityState(e, SoeEntityState.Deleted, modified: batchCreated);
            }
        }

        public void TrySetHibernatingErrorMessage(ref ActionResult result)
        {
            if (result == null || result.Success)
                return;

            switch (result.ErrorNumber)
            {
                case (int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo:
                    result.ErrorMessage = GetText(91966, "Tillfälligt primär anställning måste ha start- och slutdatum");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval:
                    result.ErrorMessage = GetText(91967, "Tillfälligt primär anställning måste ha en ordinarie vilande anställning under hela den tillfälliga perioden");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryAlreadyExistsInInterval:
                    result.ErrorMessage = GetText(91968, "Det finns redan en tillfälligt primär anställning i perioden  ");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryHibernatingHeadNotFound:
                    result.ErrorMessage = GetText(91969, "Kunde inte uppdatera vilande anställnings frånvaro");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryCannotBeSecondary:
                    result.ErrorMessage = GetText(91972, "Anställningen får inte vara både tillfälligt primär och sekundär");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryExistsAttestedTransactions:
                    result.ErrorMessage = GetText(91974, "Det finns attesterade transaktioner under perioden som inte kan tas bort");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryExistsLockedTransactions:
                    result.ErrorMessage = GetText(91975, "Det finns låsta transaktioner under perioden som inte kan tas bort");
                    break;
                case (int)ActionResultSave.TemporaryPrimaryHasNoPerissionToCreateDeleteShortenExtend:
                    result.ErrorMessage = GetText(91977, "Behörighet saknas för att ta bort eller förändra längden på den tillfälligt aktiva anställningen");
                    break;
            }
        }

        #endregion

        #region TimeHibernateAbsenceHead

        public TimeHibernatingAbsenceHead GetHibernatingAbsenceHead(int employeeId, int employmentId, bool includeRows = false, bool includeEmployee = false, bool includeEmployment = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHibernatingAbsenceHead.NoTracking();
            return GetHibernatingAbsenceHead(entities, employeeId, employmentId, includeRows, includeEmployee, includeEmployment);
        }

        private TimeHibernatingAbsenceHead GetHibernatingAbsenceHead(CompEntities entities, int employeeId, int employmentId, bool includeRows = false, bool includeEmployee = false, bool includeEmployment = false)
        {
            IQueryable<TimeHibernatingAbsenceHead> query = entities.TimeHibernatingAbsenceHead.Where(w => w.ActorCompanyId == ActorCompanyId && w.EmployeeId == employeeId && w.EmploymentId == employmentId && w.State == (int)SoeEntityState.Active);
            if (includeEmployee)
                query = query.Include("Employee.ContactPerson");
            if (includeEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange");
            if (includeRows)
                query = query.Include("TimeHibernatingAbsenceRow");

            return query.FirstOrDefault();
        }

        private TimeHibernatingAbsenceHead CreateTimeHibernatingAbsenceHead(Employee employee, Employment temporaryPrimaryEmployment, int? timeDeviationCauseId, DateTime batchCreated)
        {
            TimeHibernatingAbsenceHead hibernatingHead = new TimeHibernatingAbsenceHead()
            {
                //Set FK
                ActorCompanyId = base.ActorCompanyId,
                TimeDeviationCauseId = timeDeviationCauseId,

                //Set references
                Employee = employee,
                Employment = temporaryPrimaryEmployment,
            };
            SetCreatedProperties(hibernatingHead, created: batchCreated);
            return hibernatingHead;
        }

        private void UpdateTimeHibernatingAbsenceHead(TimeHibernatingAbsenceHead hibernatingHead, int? timeDeviationCauseId, DateTime batchCreated)
        {
            if (hibernatingHead == null || hibernatingHead.TimeDeviationCauseId == timeDeviationCauseId)
                return;

            hibernatingHead.TimeDeviationCauseId = timeDeviationCauseId;
            SetModifiedProperties(hibernatingHead, modified: batchCreated);
        }

        public ActionResult SaveHibernatingAbsenceHead(TimeHibernatingAbsenceHeadDTO hibernatingHeadInput)
        {
            if (hibernatingHeadInput?.Rows == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeHibernatingAbsence");
            if (hibernatingHeadInput.Employment == null || hibernatingHeadInput.Employment.EmploymentId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10084, "Anställning hittades inte"));

            hibernatingHeadInput.TimeDeviationCauseId = hibernatingHeadInput.TimeDeviationCauseId.ToNullable();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    Employee employee = EmployeeManager.GetEmployee(entities, hibernatingHeadInput.EmployeeId, base.ActorCompanyId, loadEmployment: true);
                    if (employee == null)
                        return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10083, "Anställd hittades inte"));

                    Employment temporaryPrimaryEmployment = employee.GetEmployment(hibernatingHeadInput.Employment.EmploymentId);
                    if (temporaryPrimaryEmployment == null)
                        return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10084, "Anställning hittades inte"));

                    DateTime batchCreated = DateTime.Now;

                    TimeHibernatingAbsenceHead hibernatingHead = hibernatingHeadInput.TimeHibernatingAbsenceHeadId > 0 ? GetHibernatingAbsenceHead(entities, hibernatingHeadInput.EmployeeId, hibernatingHeadInput.Employment.EmploymentId) : null;
                    if (hibernatingHead == null)
                        hibernatingHead = CreateTimeHibernatingAbsenceHead(employee, temporaryPrimaryEmployment, hibernatingHeadInput.TimeDeviationCauseId, batchCreated);
                    else
                        UpdateTimeHibernatingAbsenceHead(hibernatingHead, hibernatingHeadInput.TimeDeviationCauseId, batchCreated);

                    ActionResult result = UpdateHibernatingAbsenceRows(entities, hibernatingHead, hibernatingHeadInput, batchCreated);
                    if (result.Success)
                        result = SaveChanges(entities);

                    return result;
                }
                catch(Exception ex)
                {
                    LogError(ex, log);
                    return new ActionResult(ex);
                }
            }           
        }

        #endregion

        #region TimeHibernatingAbsenceRow

        public List<TimeHibernatingAbsenceRow> GetHibernatingAbsenceRows(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo, List<int> employeeIds)
        {
            var rows = entities.TimeHibernatingAbsenceRow.Include("TimeHibernatingAbsenceHead").Where(w => w.TimeHibernatingAbsenceHead.ActorCompanyId == actorCompanyId && w.TimeHibernatingAbsenceHead.State == (int)SoeEntityState.Active && w.Date >= dateFrom && w.Date <= dateTo && w.State == (int)SoeEntityState.Active).ToList();

            rows = rows.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

            return rows;
        }

        public List<TimeHibernatingAbsenceRow> GetHibernatingAbsenceRows(TimeHibernatingAbsenceHead hibernatingHead)
        {
            if (hibernatingHead == null || hibernatingHead.TimeHibernatingAbsenceHeadId == 0)
                return new List<TimeHibernatingAbsenceRow>();

            if (!hibernatingHead.TimeHibernatingAbsenceRow.IsLoaded)
                hibernatingHead.TimeHibernatingAbsenceRow.Load();

            return hibernatingHead.TimeHibernatingAbsenceRow.Where(r => r.State == (int)SoeEntityState.Active).ToList();
        }

        public ActionResult CreateHibernatingAbsenceRowsAndClearData(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, DateTime? batchCreated = null, DateTime? onlyDateFrom = null, DateTime? onlyDateTo = null)
        {
            if (hibernatingHead?.Employment == null)
                return new ActionResult(false, (int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            List<TimeHibernatingAbsenceRow> hibernatingRows = GetHibernatingAbsenceRows(hibernatingHead);
            if (!hibernatingRows.IsNullOrEmpty() && hibernatingRows.Count == hibernatingHead.Employment.GetEmploymentDays())
                return new ActionResult(true);

            DateTime dateFrom = onlyDateFrom ?? hibernatingHead.DateFrom;
            DateTime dateTo = onlyDateTo ?? hibernatingHead.DateTo;
            if (dateTo == DateTime.MaxValue)
                return new ActionResult(false);

            Dictionary<DateTime, List<TimePayrollTransaction>> transactionsByDate = GetPayrollTransactionsForHibernation(entities, hibernatingHead, dateFrom, dateTo).ByDate();
            Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksByDate = GetScheduleBlocksForHibernation(entities, hibernatingHead, dateFrom, dateTo).ByDate();

            DateTime templateStartDate = scheduleBlocksByDate.Keys.Any()
                ? scheduleBlocksByDate.Keys.Max().AddDays(1)
                : dateFrom;

            Dictionary<DateTime, List<TimeSchedulePlanningDayDTO>> templateScheduleBlocksByDate =
                scheduleBlocksByDate.Keys.LastOrDefault() < dateTo
                    ? GetTemplateScheduleBlocksForHibernation(entities, hibernatingHead, templateStartDate, dateTo).ByDate()
                    : new Dictionary<DateTime, List<TimeSchedulePlanningDayDTO>>();

            List<DateTime> datesAffected = new List<DateTime>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                TimeHibernatingAbsenceRow hibernatingRow = hibernatingHead.TimeHibernatingAbsenceRow.FirstOrDefault(r => r.Date == date);
                if (hibernatingRow == null)
                {
                    int scheduleMinutes = 0;
                    int absenceMinutes = 0;

                    var scheduleBlocksForDate = scheduleBlocksByDate.GetList(date, nullIfNotFound: true);
                    if (scheduleBlocksForDate != null)
                    {
                        //Get schedule and absence from schedule blocks and transactions
                        scheduleMinutes = scheduleBlocksForDate.GetWorkMinutes();
                        absenceMinutes = transactionsByDate.GetList(date).GetAbsenceMinutes();
                    }
                    else
                    {
                        //Get schedule and absence from template schedule blocks
                        scheduleMinutes = templateScheduleBlocksByDate.GetList(date).GetWorkMinutes(null);
                        absenceMinutes = scheduleMinutes;
                    }

                    hibernatingRow = CreateTimeHibernatingAbsenceRow(hibernatingHead, date, scheduleMinutes, absenceMinutes, batchCreated.Value);
                    datesAffected.Add(hibernatingRow.Date);
                }
                date = date.AddDays(1);
            }

            if (datesAffected.Any())
                ClearDataForHibernation(entities, hibernatingHead, datesAffected.Min(), datesAffected.Max(), batchCreated.Value, scheduleBlocksByDate.SelectMany(g => g.Value).ToList(), transactionsByDate.SelectMany(g => g.Value).ToList());

            return new ActionResult(true);
        }

        public ActionResult UpdateHibernatingAbsenceRows(CompEntities entities, TimeHibernatingAbsenceHead hibernatingHead, TimeHibernatingAbsenceHeadDTO hibernatingHeadInput, DateTime batchCreated)
        {
            if (hibernatingHead?.Employment == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10084, "Anställning hittades inte"));

            List<TimeHibernatingAbsenceRowDTO> hibernatingRowsInput = hibernatingHeadInput?.Rows?.Where(r => r.State == (int)SoeEntityState.Active).ToList();
            if (hibernatingRowsInput.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(5608, "Inga rader hittades"));

            List<TimeHibernatingAbsenceRow> hibernatingRows = GetHibernatingAbsenceRows(hibernatingHead);

            DateTime date = hibernatingHead.DateFrom;
            while (date <= hibernatingHead.DateTo)
            {
                TimeHibernatingAbsenceRowDTO inputRow = hibernatingRowsInput.FirstOrDefault(r => r.Date == date);
                int scheduleTimeMinutesInput = inputRow?.ScheduleTimeMinutes ?? 0;
                int absenceTimeMinutesInput = inputRow?.AbsenceTimeMinutes ?? 0;

                TimeHibernatingAbsenceRow hibernatingRow = hibernatingRows.FirstOrDefault(r => r.Date == date);
                if (hibernatingRow == null)
                {
                    CreateTimeHibernatingAbsenceRow(hibernatingHead, date, scheduleTimeMinutesInput, absenceTimeMinutesInput, batchCreated);
                }
                else if (hibernatingRow.ScheduleTimeMinutes != scheduleTimeMinutesInput || hibernatingRow.AbsenceTimeMinutes != absenceTimeMinutesInput)
                {
                    hibernatingRow.ScheduleTimeMinutes = inputRow?.ScheduleTimeMinutes ?? 0;
                    hibernatingRow.AbsenceTimeMinutes = inputRow?.AbsenceTimeMinutes ?? 0;
                    SetModifiedProperties(hibernatingRow, modified: batchCreated);
                }
                date = date.AddDays(1);
            }
            return new ActionResult(true);
        }

        private TimeHibernatingAbsenceRow CreateTimeHibernatingAbsenceRow(TimeHibernatingAbsenceHead hibernatingHead, DateTime date, int scheduleMinutes, int absenceMinutes, DateTime batchCreated)
        {
            TimeHibernatingAbsenceRow hibernatingRow = new TimeHibernatingAbsenceRow()
            {
                Date = date,
                ScheduleTimeMinutes = scheduleMinutes,
                AbsenceTimeMinutes = absenceMinutes,

                //Set FK
                ActorCompanyId = hibernatingHead.ActorCompanyId,
                EmployeeId = hibernatingHead.EmployeeId,
                EmployeeChildId = null, //not supported yet

                //Set references
                TimeHibernatingAbsenceHead = hibernatingHead,
            };
            SetCreatedProperties(hibernatingRow, created: batchCreated);
            return hibernatingRow;
        }

        private ActionResult SetTimeHibernatingAbsenceRowsToDeleted(TimeHibernatingAbsenceHead hibernatingHead, DateTime? batchCreated = null)
        {
            if (hibernatingHead?.Employment == null)
                return new ActionResult(true);

            List<TimeHibernatingAbsenceRow> hibernatingRows = GetHibernatingAbsenceRows(hibernatingHead);
            if (hibernatingRows.IsNullOrEmpty())
                return new ActionResult(true);

            if (hibernatingHead.Employment.State == (int)SoeEntityState.Active)
            {
                foreach (TimeHibernatingAbsenceRow hibernatingRow in hibernatingRows.Where(r => r.Date < hibernatingHead.DateFrom))
                    ChangeEntityState(hibernatingRow, SoeEntityState.Deleted, modified: batchCreated);
                foreach (TimeHibernatingAbsenceRow hibernatingRow in hibernatingRows.Where(r => r.Date > hibernatingHead.DateTo))
                    ChangeEntityState(hibernatingRow, SoeEntityState.Deleted, modified: batchCreated);
            }
            else if (hibernatingHead.Employment.State == (int)SoeEntityState.Deleted)
            {
                foreach (TimeHibernatingAbsenceRow hibernatingRow in hibernatingRows)
                    ChangeEntityState(hibernatingRow, SoeEntityState.Deleted, modified: batchCreated);
            }

            return new ActionResult(true);
        }

        #endregion
    }
}
