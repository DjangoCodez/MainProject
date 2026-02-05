using SoftOne.Soe.Business.DataCache;
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
    public class TimeDeviationCauseManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeDeviationCauseManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeDeviationCause

        public List<TimeDeviationCause> GetTimeDeviationCauses(int actorCompanyId, bool sortByName = false, bool loadTimeCode = false, bool loadEmployeeGroups = false, bool loadPayrollProduct = false, bool setTimeDeviationTypeName = true, int? timeDeviationCauseId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCauses(entities, actorCompanyId, sortByName, loadTimeCode, loadEmployeeGroups, loadPayrollProduct, setTimeDeviationTypeName, timeDeviationCauseId);
        }

        public List<TimeDeviationCause> GetTimeDeviationCauses(CompEntities entities, int actorCompanyId, bool sortByName = false, bool loadTimeCode = false, bool loadEmployeeGroups = false, bool loadPayrollProduct = false, bool setTimeDeviationTypeName = true, int? timeDeviationCauseId = null)
        {
            IQueryable<TimeDeviationCause> query = entities.TimeDeviationCause;
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadEmployeeGroups)
                query = query.Include("EmployeeGroupTimeDeviationCause");

            if (loadPayrollProduct)
                query = query.Include("TimeCode.TimeCodePayrollProduct.PayrollProduct");

            if (timeDeviationCauseId.HasValue)
                query = query.Where(w => w.TimeDeviationCauseId == timeDeviationCauseId.Value);

            var timeDeviationCauses = (from t in query
                                       where t.ActorCompanyId == actorCompanyId &&
                                       t.State == (int)SoeEntityState.Active
                                       select t).ToList();

            if (setTimeDeviationTypeName)
                SetTimeDeviationCausesTypeName(timeDeviationCauses);

            if (sortByName)
                return timeDeviationCauses.OrderBy(t => t.Name).ToList();
            else
                return timeDeviationCauses.OrderByDescending(i => i.Name.ToLower() == "standard").ThenBy(i => i.Type).ThenBy(i => i.Name).ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesAbsence(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesAbsence(entities, actorCompanyId);
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesAbsence(CompEntities entities, int actorCompanyId)
        {
            return GetTimeDeviationCauses(entities, actorCompanyId)
                .Where(x => x.IsAbsence)
                .ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesHibernating(int actorCompanyId, bool loadTimeCode = false, bool loadEmployeeGroups = false)
        {
            return GetTimeDeviationCauses(actorCompanyId, loadTimeCode: loadTimeCode, loadEmployeeGroups: loadEmployeeGroups)
                .Where(t => t.ValidForHibernating && t.Type != (int)TermGroup_TimeDeviationCauseType.Presence)
                .ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesEmployeeRequests(int actorCompanyId, int employeeGroupId, int employeeId)
        {
            if (employeeGroupId == 0)
                employeeGroupId = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true)?.GetEmployeeGroupId(DateTime.Today) ?? 0;

            return GetTimeDeviationCausesEmployeeRequests(actorCompanyId, employeeGroupId)
                .Where(t => t.IsAbsence)
                .ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesEmployeeRequests(int actorCompanyId, int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausedEmployeeRequest(entities, actorCompanyId, employeeGroupId);
        }

        public List<TimeDeviationCause> GetTimeDeviationCausedEmployeeRequest(CompEntities entities, int actorCompanyId, int employeeGroupId)
        {
            var timeDeviationCauses = (from t in entities.TimeDeviationCause
                                        .Include("EmployeeGroupTimeDeviationCauseRequest")
                                       where t.ActorCompanyId == actorCompanyId &&
                                       t.State == (int)SoeEntityState.Active &&
                                       t.EmployeeGroupTimeDeviationCauseRequest.Any(e => e.EmployeeGroupId == employeeGroupId)
                                       select t).ToList();

            return timeDeviationCauses.OrderBy(t => t.Name).ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesAbsenceAnnouncements(int actorCompanyId, int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesAbsenceAnnouncements(entities, actorCompanyId, employeeGroupId);
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesAbsenceAnnouncements(CompEntities entities, int actorCompanyId, int employeeGroupId)
        {
            var timeDeviationCauses = (from t in entities.TimeDeviationCause
                                        .Include("EmployeeGroupTimeDeviationCauseAbsenceAnnouncement")
                                       where t.ActorCompanyId == actorCompanyId &&
                                       t.State == (int)SoeEntityState.Active &&
                                       t.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Any(e => e.EmployeeGroupId == employeeGroupId)
                                       select t).ToList();

            return timeDeviationCauses.OrderBy(t => t.Name).ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesCurrent(int actorCompanyId, int employeeId, DateTime localTime)
        {
            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromEmployeeId(actorCompanyId, employeeId, onlyUseInTimeTerminal: true);
            if (timeDeviationCauses.IsNullOrEmpty())
                return new List<TimeDeviationCause>();

            TermGroup_TimeDeviationCauseType causeType = TermGroup_TimeDeviationCauseType.Presence;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TimeScheduleTemplateBlock closestSchedule = TimeScheduleManager.GetClosestSchedule(entitiesReadOnly, employeeId, localTime);
            if (closestSchedule != null)
            {
                bool insideSchedule = localTime >= closestSchedule.ActualStartTime && localTime <= closestSchedule.ActualStopTime;
                if (insideSchedule)
                {
                    TimeStampEntry lastEntry = TimeStampManager.GetLastTimeStampEntryForEmployee(employeeId, true);
                    TimeStampEntryType lastEntryType = lastEntry != null ? (TimeStampEntryType)lastEntry.Type : TimeStampEntryType.Unknown;
                    bool lastEntryInsideSchedule = lastEntry != null && (lastEntry.Time >= closestSchedule.ActualStartTime && lastEntry.Time <= closestSchedule.ActualStopTime);

                    if (lastEntryType == TimeStampEntryType.Out && lastEntryInsideSchedule)
                        causeType = TermGroup_TimeDeviationCauseType.Presence;
                    else
                        causeType = TermGroup_TimeDeviationCauseType.Absence;
                }
            }

            return timeDeviationCauses
                .Where(t => t.Type == (int)causeType || t.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence)
                .OrderByDescending(i => i.Name.ToLower() == "standard")
                .ThenBy(i => i.Type)
                .ThenBy(i => i.Name)
                .ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesAbsenceFromEmployeeId(int actorCompanyId, int employeeId, DateTime? date = null, bool onlyUseInTimeTerminal = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesByEmployeeId(entitiesReadOnly, actorCompanyId, employeeId, date, onlyUseInTimeTerminal)
                .Where(x => x.IsAbsence)
                .ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesFromEmployeeId(int actorCompanyId, int employeeId, DateTime? date = null, bool onlyUseInTimeTerminal = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesByEmployeeId(entities, actorCompanyId, employeeId, date, onlyUseInTimeTerminal);
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesByEmployeeId(CompEntities entities, int actorCompanyId, int employeeId, DateTime? date = null, bool onlyUseInTimeTerminal = false)
        {
            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
            if (employee == null)
                return new List<TimeDeviationCause>();

            int employeeGroupId = employee.GetEmployeeGroupId(date);
            if (employeeGroupId == 0)
                return GetTimeDeviationCauses(entities, actorCompanyId, loadEmployeeGroups: true);

            var timeDeviationCauses = (from t in entities.TimeDeviationCause
                                        .Include("EmployeeGroupTimeDeviationCause")
                                       where t.ActorCompanyId == actorCompanyId &&
                                       t.State == (int)SoeEntityState.Active &&
                                       t.EmployeeGroupTimeDeviationCause.Any(e => e.EmployeeGroupId == employeeGroupId && e.State == (int)SoeEntityState.Active && (!onlyUseInTimeTerminal || e.UseInTimeTerminal))
                                       select t).ToList();

            return timeDeviationCauses.OrderBy(t => t.Name).ToList();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesByEmployeeGroup(int actorCompanyId, int? employeeGroupId = null, bool sort = false, bool loadTimeCode = false, bool setTimeDeviationTypeName = false, bool onlyUseInTimeTerminal = false, bool removeAbsence = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesByEmployeeGroup(entities, actorCompanyId, employeeGroupId, sort, loadTimeCode, setTimeDeviationTypeName, onlyUseInTimeTerminal, removeAbsence);
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesByEmployeeGroup(CompEntities entities, int actorCompanyId, int? employeeGroupId = null, bool sortByTypeAndStandard = false, bool loadTimeCode = false, bool setTimeDeviationTypeName = true, bool onlyUseInTimeTerminal = false, bool removeAbsence = false)
        {
            IQueryable<TimeDeviationCause> oQuery = entities.TimeDeviationCause.Include("Company").Include("EmployeeGroupTimeDeviationCause");

            if (loadTimeCode)
                oQuery = oQuery.Include("TimeCode");

            var query = from t in oQuery
                        where t.ActorCompanyId == actorCompanyId &&
                        t.State == (int)SoeEntityState.Active
                        select t;

            if (employeeGroupId.HasValue)
                query = query.Where(t => t.EmployeeGroupTimeDeviationCause.Any(e => e.EmployeeGroupId == employeeGroupId && e.State == (int)SoeEntityState.Active && (!onlyUseInTimeTerminal || e.UseInTimeTerminal)));

            List<TimeDeviationCause> timeDeviationCauses = query.ToList();

            if (sortByTypeAndStandard)
                timeDeviationCauses = timeDeviationCauses.SortTimeDeviationCausesByType().SortTimeDeviationCausesByStandard();
            else
                timeDeviationCauses = timeDeviationCauses.SortTimeDeviationCausesByName();

            if (setTimeDeviationTypeName)
                SetTimeDeviationCausesTypeName(timeDeviationCauses);

            return removeAbsence ? timeDeviationCauses.Where(x => x.IsPresence).ToList() : timeDeviationCauses;
        }

        public List<TimeDeviationsCauseForWTSDTO> GetTimeDeviationCauseForEmployeeNow(int actorCompanyId, int employeeId, CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (entities == null)
                entities = entitiesReadOnly;

            List<TimeDeviationsCauseForWTSDTO> allTimeDeviationCauses = new List<TimeDeviationsCauseForWTSDTO>();
            List<TimeDeviationsCauseForWTSDTO> filteredTimeDeviationCauses = new List<TimeDeviationsCauseForWTSDTO>();
            Dictionary<int, string> children = new Dictionary<int, string>();
            DateTime now = DateTime.Now;

            Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId);
            if (employee == null)
                return new List<TimeDeviationsCauseForWTSDTO>();

            //Get all deviationcauses for the employee
            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesByEmployeeId(entities, actorCompanyId, employeeId, onlyUseInTimeTerminal: true);

            //Check to see if there SpecifyChild on Cause
            if (timeDeviationCauses.Any(c => c.SpecifyChild))
            {
                List<EmployeeChild> employeeChilds = EmployeeManager.GetEmployeeChilds(employeeId, actorCompanyId);
                if (!employeeChilds.IsNullOrEmpty())
                {
                    foreach (EmployeeChild employeeChild in employeeChilds)
                    {
                        children.Add(employeeChild.EmployeeChildId, employeeChild.FirstName);
                    }
                }
            }

            if (!timeDeviationCauses.Any())
                return new List<TimeDeviationsCauseForWTSDTO>();

            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                var dto = new TimeDeviationsCauseForWTSDTO()
                {
                    TimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId,
                    Name = timeDeviationCause.Name,
                };
                if (!timeDeviationCause.SpecifyChild)
                {
                    dto.Children = new Dictionary<int, string>();
                    dto.Children.AddRange(children);
                }
                allTimeDeviationCauses.Add(dto);
            }

            //Check if time is with schedule
            bool withinSchedule = TimeScheduleManager.IsEmployeeWithinSchedule(entities, employee, now);

            //Check if last Timestamp
            TimeStampEntry lastEntry = TimeStampManager.GetLastTimeStampEntryForEmployee(employee.EmployeeId);
            bool lastEntrywithinSchedule = false;
            if (lastEntry != null && lastEntry.Time < now && lastEntry.Time > now.AddDays(-1) && lastEntry.Type == (int)TimeStampEntryType.Out && withinSchedule)
                lastEntrywithinSchedule = TimeScheduleManager.IsEmployeeWithinSchedule(entities, employee, lastEntry.Time);

            if (withinSchedule && lastEntrywithinSchedule)
                withinSchedule = false;

            //Add different type of causes to dict
            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                if (withinSchedule && (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence || timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence))
                    filteredTimeDeviationCauses.AddRange(allTimeDeviationCauses.Where(d => d.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId));
                if (!withinSchedule && (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Presence || timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence))
                    filteredTimeDeviationCauses.AddRange(allTimeDeviationCauses.Where(d => d.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId));
            }

            return filteredTimeDeviationCauses;
        }

        public Dictionary<int, string> GetTimeDeviationCausesDict(int actorCompanyId, bool addEmptyRow, bool removeAbsence = false, bool removePresence = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesDict(entities, actorCompanyId, addEmptyRow, removeAbsence, removePresence);
        }

        public Dictionary<int, string> GetTimeDeviationCausesDict(CompEntities entities, int actorCompanyId, bool addEmptyRow, bool removeAbsence = false, bool removePresence = false)
        {
            return GetTimeDeviationCauses(entities, actorCompanyId)
                .ToDict(addEmptyRow, removeAbsence, removePresence);
        }

        public Dictionary<int, string> GetTimeDeviationCausesDictDiscardState(int actorCompanyId, IEnumerable<int> timeDeviationCauseIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesDictDiscardState(entities, actorCompanyId, timeDeviationCauseIds);
        }

        public Dictionary<int, string> GetTimeDeviationCausesDictDiscardState(CompEntities entities, int actorCompanyId, IEnumerable<int> timeDeviationCauseIds)
        {
            if (timeDeviationCauseIds.IsNullOrEmpty())
                return new Dictionary<int, string>();

            timeDeviationCauseIds = timeDeviationCauseIds.Distinct();

            var timeDeviationCauses = (from t in entities.TimeDeviationCause
                                       where t.ActorCompanyId == actorCompanyId &&
                                       timeDeviationCauseIds.Contains(t.TimeDeviationCauseId)
                                       select t).ToList();

            return timeDeviationCauses.ToDict();
        }

        public Dictionary<int, string> GetTimeDeviationCausesDictByEmployeeGroup(int actorCompanyId, int? employeeGroupId, bool addEmptyRow, bool removeAbsence = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCausesDictByEmployeeGroup(entities, actorCompanyId, addEmptyRow, employeeGroupId, removeAbsence);
        }

        public Dictionary<int, string> GetTimeDeviationCausesDictByEmployeeGroup(CompEntities entities, int actorCompanyId, bool addEmptyRow, int? employeeGroupId, bool removeAbsence = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesByEmployeeGroup(entities, actorCompanyId, employeeGroupId);
            foreach (var timeDeviationCause in timeDeviationCauses)
            {
                if (!removeAbsence || timeDeviationCause.IsPresence)
                    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetTimeDeviationCausesAbsenceDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCauses(actorCompanyId);
            foreach (var timeDeviationCause in timeDeviationCauses)
            {
                if (timeDeviationCause.IsAbsence)
                    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);
            }

            return dict.Sort();
        }

        public Dictionary<int, string> GetTimeDeviationCausesRequestsDict(int actorCompanyId, int employeeGroupId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timeDeviationCauses = GetTimeDeviationCausesEmployeeRequests(actorCompanyId, employeeGroupId);
            foreach (var timeDeviationCause in timeDeviationCauses)
            {
                if (timeDeviationCause.IsAbsence)
                    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetTimeDeviationCausesAbsenceAnnouncementDict(int actorCompanyId, int employeeGroupId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timeDeviationCauses = GetTimeDeviationCausesAbsenceAnnouncements(actorCompanyId, employeeGroupId);
            foreach (var timeDeviationCause in timeDeviationCauses)
            {
                if (timeDeviationCause.IsAbsence)
                    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetTimeDeviationCauseTypesDict(bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            List<GenericType> terms = base.GetTermGroupContent(TermGroup.TimeDeviationCauseType, addEmptyRow: addEmptyRow, skipUnknown: true);
            terms.ForEach(i => dict.Add(i.Id, i.Name));

            return dict;
        }

        public TimeDeviationCause GetTimeDeviationCause(int timeDeviationCauseId, int actorCompanyId, bool loadEmployeeGroup = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCause(entities, timeDeviationCauseId, actorCompanyId, loadEmployeeGroup);
        }

        public TimeDeviationCause GetTimeDeviationCause(CompEntities entities, int timeDeviationCauseId, int actorCompanyId, bool loadEmployeeGroup = false)
        {
            if (timeDeviationCauseId == 0)
                return null;

            IQueryable<TimeDeviationCause> query = entities.TimeDeviationCause.Include("TimeCode");

            if (loadEmployeeGroup)
                query = query.Include("EmployeeGroupTimeDeviationCause");

            var timeDeviationCause = (from t in query
                                      where t.TimeDeviationCauseId == timeDeviationCauseId &&
                                      t.State == (int)SoeEntityState.Active &&
                                      t.ActorCompanyId == actorCompanyId
                                      select t).FirstOrDefault();

            return timeDeviationCause;
        }

        public TimeDeviationCause GetTimeDeviationCause(int? timeDeviationCauseId, int actorCompanyId, ref List<TimeDeviationCause> timeDeviationCauses)
        {
            TimeDeviationCause timeDeviationCause = null;

            if (timeDeviationCauseId.HasValue)
            {
                timeDeviationCause =
                    timeDeviationCauses?.FirstOrDefault(i => i.TimeDeviationCauseId == timeDeviationCauseId.Value) ??
                    GetTimeDeviationCause(timeDeviationCauseId.Value, actorCompanyId);

                if (timeDeviationCause != null && timeDeviationCauses != null)
                    timeDeviationCauses.Add(timeDeviationCause);

            }

            return timeDeviationCause;
        }

        public TimeDeviationCause GetTimeDeviationCauseFromPrio(int employeeId, int employeeGroupId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeDeviationCauseFromPrio(entities, employeeId, employeeGroupId, actorCompanyId);
        }

        public TimeDeviationCause GetTimeDeviationCauseFromPrio(CompEntities entities, int employeeId, int employeeGroupId, int actorCompanyId)
        {
            int timeDeviationCauseId = GetTimeDeviationCauseIdFromPrio(employeeId, employeeGroupId, actorCompanyId);
            return GetTimeDeviationCause(entities, timeDeviationCauseId, actorCompanyId, false);
        }

        public int GetTimeDeviationCauseIdFromPrio(int employeeId, int employeeGroupId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeDeviationCauseIdFromPrio(entities, employeeId, employeeGroupId, actorCompanyId);
        }

        public int GetTimeDeviationCauseIdFromPrio(CompEntities entities, int employeeId, int employeeGroupId, int actorCompanyId)
        {
            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId);
            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(employeeGroupId);
            return GetTimeDeviationCauseIdFromPrio(employee, employeeGroup);
        }

        public int GetTimeDeviationCauseIdFromPrio(int employeeId, int actorCompanyId, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeDeviationCauseIdFromPrio(entities, employeeId, actorCompanyId, date);
        }

        public int GetTimeDeviationCauseIdFromPrio(CompEntities entities, int employeeId, int actorCompanyId, DateTime? date = null)
        {
            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
            return GetTimeDeviationCauseIdFromPrio(employee, date);
        }

        public int GetTimeDeviationCauseIdFromPrio(Employee employee, DateTime? date = null, List<EmployeeGroup> employeeGroups = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            EmployeeGroup employeeGroup = null;
            if (employee != null)
            {
                if (employeeGroups == null)
                    employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));
                employeeGroup = employee.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup == null)
                    employeeGroup = employee.CurrentEmployeeGroup;
            }

            return GetTimeDeviationCauseIdFromPrio(employee, employeeGroup);
        }

        public int GetTimeDeviationCauseIdFromPrio(Employee employee, EmployeeGroup employeeGroup)
        {
            return GetTimeDeviationCauseIdFromPrio(employee?.TimeDeviationCauseId, employeeGroup?.TimeDeviationCauseId);
        }

        public int GetTimeDeviationCauseIdFromPrio(int? employeeTimeDeviationCauseId, int? employeeGroupTimeDeviationCauseId)
        {
            int deviationCauseId = 0;

            // Try get from Employee
            if (employeeTimeDeviationCauseId.HasValue)
                deviationCauseId = employeeTimeDeviationCauseId.Value;

            // Try get from EmployeeGroup
            if (deviationCauseId == 0 && employeeGroupTimeDeviationCauseId.HasValue)
                deviationCauseId = employeeGroupTimeDeviationCauseId.Value;

            return deviationCauseId;
        }

        public bool ExistsTimeDeviationCause(string timeDeviationCauseName, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return ExistsTimeDeviationCause(entities, timeDeviationCauseName, actorCompanyId);
        }

        public bool ExistsTimeDeviationCause(CompEntities entities, string timeDeviationCauseName, int actorCompanyId)
        {
            return (from t in entities.TimeDeviationCause
                    where t.Name == timeDeviationCauseName &&
                    t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).Any();
        }

        public bool ExistsTimeDeviationCause(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return ExistsTimeDeviationCause(entities, actorCompanyId);
        }

        public bool ExistsTimeDeviationCause(CompEntities entities, int actorCompanyId)
        {
            return (from t in entities.TimeDeviationCause
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).Any();
        }

        private void SetTimeDeviationCausesTypeName(List<TimeDeviationCause> timeDeviationCauses)
        {
            List<GenericType> terms = base.GetTermGroupContent(TermGroup.TimeDeviationCauseType, skipUnknown: true);
            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                GenericType term = terms.FirstOrDefault(i => i.Id == timeDeviationCause.Type);
                if (term != null)
                    timeDeviationCause.TypeName = term.Name;
            }
        }

        public ActionResult SaveTimeDeviationCauses(int actorCompanyId, TimeDeviationCauseDTO inputTimeDeviationCause)
        {
            ActionResult result = null;
            int timeDeviationCauseId = inputTimeDeviationCause.TimeDeviationCauseId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region TimeDeviationCause

                        TimeDeviationCause timeDeviationCause = GetTimeDeviationCause(entities, timeDeviationCauseId, actorCompanyId);
                        if (timeDeviationCause == null)
                        {
                            #region TimeDeviationCause Add

                            timeDeviationCause = new TimeDeviationCause()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(timeDeviationCause);
                            entities.AddToTimeDeviationCause(timeDeviationCause);

                            #endregion
                        }
                        else
                        {
                            #region TimeDeviationCause Update

                            SetModifiedProperties(timeDeviationCause);

                            #endregion
                        }

                        timeDeviationCause.Name = inputTimeDeviationCause.Name;
                        timeDeviationCause.Description = inputTimeDeviationCause.Description;
                        timeDeviationCause.ExtCode = inputTimeDeviationCause.ExtCode;
                        timeDeviationCause.Type = (int)inputTimeDeviationCause.Type;
                        timeDeviationCause.TimeCodeId = inputTimeDeviationCause.TimeCodeId.ToNullable();
                        timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore = inputTimeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore;
                        timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride = inputTimeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride;
                        timeDeviationCause.AttachZeroDaysNbrOfDaysAfter = inputTimeDeviationCause.AttachZeroDaysNbrOfDaysAfter;
                        timeDeviationCause.AttachZeroDaysNbrOfDaysBefore = inputTimeDeviationCause.AttachZeroDaysNbrOfDaysBefore;
                        timeDeviationCause.ChangeCauseOutsideOfPlannedAbsence = inputTimeDeviationCause.ChangeCauseOutsideOfPlannedAbsence;
                        timeDeviationCause.ChangeCauseInsideOfPlannedAbsence = inputTimeDeviationCause.ChangeCauseInsideOfPlannedAbsence;
                        timeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence = inputTimeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence;
                        timeDeviationCause.AdjustTimeOutsideOfPlannedAbsence = inputTimeDeviationCause.AdjustTimeOutsideOfPlannedAbsence;
                        timeDeviationCause.AdjustTimeInsideOfPlannedAbsence = inputTimeDeviationCause.AdjustTimeInsideOfPlannedAbsence;
                        timeDeviationCause.AllowGapToPlannedAbsence = inputTimeDeviationCause.AllowGapToPlannedAbsence;
                        timeDeviationCause.ShowZeroDaysInAbsencePlanning = inputTimeDeviationCause.ShowZeroDaysInAbsencePlanning;
                        timeDeviationCause.IsVacation = inputTimeDeviationCause.IsVacation;
                        timeDeviationCause.Payed = inputTimeDeviationCause.Payed;
                        timeDeviationCause.NotChargeable = inputTimeDeviationCause.NotChargeable;
                        timeDeviationCause.OnlyWholeDay = inputTimeDeviationCause.OnlyWholeDay;
                        timeDeviationCause.SpecifyChild = inputTimeDeviationCause.SpecifyChild;
                        timeDeviationCause.ExcludeFromPresenceWorkRules = inputTimeDeviationCause.ExcludeFromPresenceWorkRules;
                        timeDeviationCause.ExcludeFromScheduleWorkRules = inputTimeDeviationCause.ExcludeFromScheduleWorkRules;
                        timeDeviationCause.ValidForStandby = inputTimeDeviationCause.ValidForStandby;
                        timeDeviationCause.ValidForHibernating = inputTimeDeviationCause.ValidForHibernating;
                        timeDeviationCause.CandidateForOvertime = inputTimeDeviationCause.CandidateForOvertime;
                        timeDeviationCause.CalculateAsOtherTimeInSales = inputTimeDeviationCause.CalculateAsOtherTimeInSales;
                        timeDeviationCause.MandatoryNote = inputTimeDeviationCause.MandatoryNote;
                        timeDeviationCause.MandatoryTime = inputTimeDeviationCause.MandatoryTime;

                        #endregion

                        if (timeDeviationCauseId == 0)
                        {
                            SetCreatedProperties(timeDeviationCause);
                            entities.AddToTimeDeviationCause(timeDeviationCause);
                        }
                        else
                            SetModifiedProperties(timeDeviationCause);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            timeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId;
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
                        result.IntegerValue = timeDeviationCauseId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult IsOkToDeleteTimeDeviationCause(CompEntities entities, int timeDeviationCauseId)
        {
            ActionResult result = new ActionResult(true);

            //Employee
            if (entities.Employee.Any(i => i.TimeDeviationCauseId.HasValue && i.TimeDeviationCauseId.Value == timeDeviationCauseId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployee, GetText(5018, "Anställda"));
            //EmployeeGroup
            else if (entities.EmployeeGroup.Any(i => i.TimeDeviationCauseId.HasValue && i.TimeDeviationCauseId.Value == timeDeviationCauseId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeGroup, GetText(4408, "Tidavtal"));
            else if (entities.EmployeeGroup.Any(i => i.EmployeeGroupTimeDeviationCause.Any(tdc => tdc.TimeDeviationCauseId == timeDeviationCauseId) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeGroup, GetText(4408, "Tidavtal"));
            else if (entities.EmployeeGroup.Any(i => i.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Any(tdc => tdc.TimeDeviationCauseId == timeDeviationCauseId) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeGroup, GetText(4408, "Tidavtal"));
            else if (entities.EmployeeGroup.Any(i => i.EmployeeGroupTimeDeviationCauseRequest.Any(tdc => tdc.TimeDeviationCauseId == timeDeviationCauseId) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeGroup, GetText(4408, "Tidavtal"));
            else if (entities.EmployeeGroup.Any(i => i.EmployeeGroupTimeDeviationCauseTimeCode.Any(tdc => tdc.TimeDeviationCauseId == timeDeviationCauseId) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeGroup, GetText(4408, "Tidavtal"));
            //EmployeeRequest
            else if (entities.EmployeeRequest.Any(i => i.TimeDeviationCauseId == timeDeviationCauseId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeRequest, GetText(12124, "Frånvaroansökan"));
            //TimeStampEntry
            else if (entities.TimeStampEntry.Any(i => i.TimeDeviationCauseId.HasValue && i.TimeDeviationCauseId.Value == timeDeviationCauseId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeRequest, GetText(8139, "Stämplingar"));
            //VacationGroupSE
            else if (entities.VacationGroupSE.Any(i => i.TimeDeviationCause != null && i.TimeDeviationCause.TimeDeviationCauseId == timeDeviationCauseId))
                result = new ActionResult((int)ActionResultDelete.TimeDeviationCauseHasEmployeeRequest, GetText(12123, "Semesteravtal"));
            //TimeRule
            else if (entities.TimeRuleRow.Any(i => i.TimeDeviationCauseId == timeDeviationCauseId && i.TimeRule.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasTimeRules, GetText(12119, "Tidsregler"));

            if (!result.Success)
                result.ErrorMessage = $"{GetText(5664, "Kontrollera att den inte är kopplad till")} {result.ErrorMessage.ToLower()}. ({GetText(5654, "Felkod:")}{result.ErrorNumber})";

            return result;
        }

        public ActionResult ValidateTimeDeviationCausePolicy(int actorCompanyId, EmployeeRequestDTO request)
        {
            try
            {
                if (!request.TimeDeviationCauseId.HasValue)
                    return new ActionResult(true);

                TimeDeviationCause deviationCause = GetTimeDeviationCause(request.TimeDeviationCauseId.Value, actorCompanyId, false);
                if (deviationCause == null)
                    return new ActionResult(true);

                if (deviationCause.EmployeeRequestPolicyNbrOfDaysBefore == 0)
                    return new ActionResult(true);

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                bool validateEmployeeRequestPolicyNbrOfDaysBefore = false;
                bool employeeRequestPolicyNbrOfDaysBeforeViolated = false;
                if (request.EmployeeRequestId == 0)
                {
                    validateEmployeeRequestPolicyNbrOfDaysBefore = true;
                }
                else
                {
                    entitiesReadOnly.EmployeeRequest.NoTracking();
                    EmployeeRequest originalRequest = (from req in entitiesReadOnly.EmployeeRequest
                                                       where req.ActorCompanyId == actorCompanyId &&
                                                       req.EmployeeRequestId == request.EmployeeRequestId
                                                       select req).FirstOrDefault();

                    if (originalRequest != null && (originalRequest.Start.Date != request.Start.Date || originalRequest.TimeDeviationCauseId != request.TimeDeviationCauseId))
                        validateEmployeeRequestPolicyNbrOfDaysBefore = true;
                }

                if (validateEmployeeRequestPolicyNbrOfDaysBefore && Math.Abs((request.Start - DateTime.Today.Date).Days) < deviationCause.EmployeeRequestPolicyNbrOfDaysBefore)
                    employeeRequestPolicyNbrOfDaysBeforeViolated = true;

                if (employeeRequestPolicyNbrOfDaysBeforeViolated)
                {
                    ActionResult result = new ActionResult(true);
                    result.InfoMessage = string.Format(GetText(8789, "Policy för ansökan om {0}"), deviationCause.Name);
                    result.ErrorMessage = string.Format(GetText(8790, "{0} ska sökas senast {1} dagar före ledighet"), deviationCause.Name, deviationCause.EmployeeRequestPolicyNbrOfDaysBefore) + "\n";

                    if (deviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride)
                    {
                        result.CanUserOverride = true;
                        result.ErrorMessage += GetText(8791, "Prata med din chef om denna ansökan") + "\n";
                        result.ErrorMessage += GetText(8493, "Vill du spara ändå?");
                    }
                    else
                    {
                        result.CanUserOverride = false;
                        result.ErrorMessage += GetText(8792, "Ändra datum om du lagt in fel datum");
                    }

                    return result;
                }
                else
                {
                    return new ActionResult(true);
                }
            }
            catch (Exception e)
            {
                return new ActionResult(e);
            }
        }

        public ActionResult DeleteTimeDeviationCause(int timeDeviationCauseId, int actorCompanyId)
        {

            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = IsOkToDeleteTimeDeviationCause(entities, timeDeviationCauseId);
                if (!result.Success)
                    return result;

                TimeDeviationCause timeDeviationCause = GetTimeDeviationCause(entities, timeDeviationCauseId, actorCompanyId, false);
                if (timeDeviationCause == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeDeviationCause");

                result = ChangeEntityState(entities, timeDeviationCause, SoeEntityState.Deleted, true);
                if (result.Success)
                    SendWebPubSubMessage(entities, actorCompanyId, timeDeviationCause, WebPubSubMessageAction.Delete);

                return result;
            }
        }

        private void SendWebPubSubMessage(CompEntities entities, int actorCompanyId, TimeDeviationCause timeDeviationCause, WebPubSubMessageAction action)
        {
            List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, actorCompanyId);
            foreach (int terminalId in terminalIds)
            {
                base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(actorCompanyId, terminalId), timeDeviationCause.GetUpdateMessage(action));
            }
        }

        #endregion
    }
}
