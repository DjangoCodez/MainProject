using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.BatchHelper;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.ExportFiles;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;
using EnumerationsSID = SoftOneId.Common.Enumerations;

namespace SoftOne.Soe.Business.Core
{
    public class AttestManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public AttestManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Attest common

        /// <summary>
        /// Get attest state entities (TermGroup)
        /// </summary>
        /// <param name="addEmptyRow">If true, an empty entity will be added as first item in list, with Id = 0</param>
        /// <param name="skipUnknown">If true, the entity with Id = 0 (Unknown) will not be returned</param>
        /// <param name="module">The module to get attest entities for</param>
        /// <returns>State entities</returns>
        public List<GenericType> GetAttestEntities(bool addEmptyRow, bool skipUnknown, SoeModule module)
        {
            List<GenericType> terms = base.GetTermGroupContent(TermGroup.AttestEntity, addEmptyRow: addEmptyRow, skipUnknown: skipUnknown);
            switch (module)
            {
                case SoeModule.Billing:
                    #region Billing

                    if (addEmptyRow || !skipUnknown)
                        terms = terms.Where(i => i.Id == 0 || (i.Id >= (int)TermGroup_AttestEntityGroup.BillingStart && i.Id <= (int)TermGroup_AttestEntityGroup.BillingStop)).ToList();
                    else
                        terms = terms.Where(i => i.Id >= (int)TermGroup_AttestEntityGroup.BillingStart && i.Id <= (int)TermGroup_AttestEntityGroup.BillingStop).ToList();

                    #endregion
                    break;
                case SoeModule.Economy:
                    #region Economy

                    if (addEmptyRow || !skipUnknown)
                        terms = terms.Where(i => i.Id == 0 || (i.Id >= (int)TermGroup_AttestEntityGroup.EconomyStart && i.Id <= (int)TermGroup_AttestEntityGroup.EconomyStop)).ToList();
                    else
                        terms = terms.Where(i => i.Id >= (int)TermGroup_AttestEntityGroup.EconomyStart && i.Id <= (int)TermGroup_AttestEntityGroup.EconomyStop).ToList();
                    break;

                #endregion
                case SoeModule.Time:
                    #region Time

                    if (addEmptyRow || !skipUnknown)
                        terms = terms.Where(i => i.Id == 0 || (i.Id >= (int)TermGroup_AttestEntityGroup.TimeStart && i.Id <= (int)TermGroup_AttestEntityGroup.TimeStop)).ToList();
                    else
                        terms = terms.Where(i => i.Id >= (int)TermGroup_AttestEntityGroup.TimeStart && i.Id <= (int)TermGroup_AttestEntityGroup.TimeStop).ToList();

                    #endregion
                    break;
            }
            return terms;
        }

        #endregion

        #region AttestReminder

        public List<GenericType> GetAttestReminderTerms(Employee employee)
        {
            if (employee != null && !employee.UserReference.IsLoaded)
                employee.UserReference.Load();

            return GetAttestReminderTerms(employee?.User);
        }

        public List<GenericType> GetAttestReminderTerms(User user)
        {
            return GetAttestReminderTerms(user?.LangId);
        }

        public List<GenericType> GetAttestReminderTerms(int? langId)
        {
            return GetTermGroupContent(TermGroup.AttestReminder, langId: langId ?? 0);
        }

        public (string subject, string body) GetAttestReminderMailToEmployee(Employee employee, int reminderPeriodType, AttestState reminderAttestState, DateTime dateTo, TimePeriod timePeriod)
        {
            var textsResult = GetAttestReminderTextsEmployee(employee, reminderPeriodType, dateTo, timePeriod);

            string reminderAttestStateName = reminderAttestState?.Name ?? "[]";

            //Påminnelse: Ändra [period] till [status]
            string subject = $"{textsResult.Reminder} {textsResult.Change} {textsResult.PeriodName} {textsResult.To.ToLower()} {reminderAttestStateName}";

            //Påminnelse: Detta är ett automatiskt meddelande.\nSoftone påminner dig om att [period] ännu inte har status [status]
            string body = $"{textsResult.Text} {textsResult.PeriodName} {textsResult.StateNotReached.ToLower()} {reminderAttestStateName}";

            return (subject, body);
        }

        public (string subject, string body) GetAttestReminderMailToExecutive(User user, List<string> employeeNames)
        {
            List<GenericType> terms = GetAttestReminderTerms(user);

            string subject = GetAttestReminderText(terms, 23);
            string body = GetAttestReminderText(terms, 24);
            string employeeInfo = employeeNames.Any() ? Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, employeeNames) : "";

            return (subject, body + employeeInfo);
        }

        public (string Reminder, string Change, string Text, string To, string StateNotReached, string PeriodName) GetAttestReminderTextsEmployee(Employee employee, int reminderPeriodType, DateTime dateTo, TimePeriod timePeriod)
        {
            List<GenericType> terms = GetAttestReminderTerms(employee);

            string reminder = GetAttestReminderText(terms, 18);
            string change = GetAttestReminderText(terms, 25);
            string text = reminderPeriodType == (int)AttestPeriodType.Day ? GetAttestReminderText(terms, 22) : GetAttestReminderText(terms, 20);
            string to = GetAttestReminderText(terms, 19);
            string stateNotReached = GetAttestReminderText(terms, 21);
            string periodName = GetReminderPeriodName(terms, reminderPeriodType, dateTo, timePeriod);

            return (reminder, change, text, to, stateNotReached, periodName);
        }

        private string GetAttestReminderText(List<GenericType> terms, int id)
        {
            return terms?.FirstOrDefault(p => p.Id == id)?.Name ?? string.Empty;
        }

        private string GetReminderPeriodName(List<GenericType> terms, int reminderPeriodType, DateTime date, TimePeriod timePeriod)
        {
            string name = "";

            if (reminderPeriodType == (int)AttestPeriodType.Period && timePeriod == null)
                reminderPeriodType = (int)AttestPeriodType.Month;

            switch (reminderPeriodType)
            {
                case (int)AttestPeriodType.Day:
                    #region Day

                    name = date.ToShortDateString();

                    #endregion
                    break;
                case (int)AttestPeriodType.Week:
                    #region Week

                    name = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CultureInfo.InvariantCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString();

                    #endregion
                    break;
                case (int)AttestPeriodType.Month:
                    #region Month

                    switch (date.Month)
                    {
                        case 1:
                            name = terms?.FirstOrDefault(p => p.Id == 5)?.Name.ToLower() ?? "januari";
                            break;
                        case 2:
                            name = terms?.FirstOrDefault(p => p.Id == 6)?.Name.ToLower() ?? "februari";
                            break;
                        case 3:
                            name = terms?.FirstOrDefault(p => p.Id == 7)?.Name.ToLower() ?? "mars";
                            break;
                        case 4:
                            name = terms?.FirstOrDefault(p => p.Id == 8)?.Name.ToLower() ?? "april";
                            break;
                        case 5:
                            name = terms?.FirstOrDefault(p => p.Id == 9)?.Name.ToLower() ?? "maj";
                            break;
                        case 6:
                            name = terms?.FirstOrDefault(p => p.Id == 10)?.Name.ToLower() ?? "juni";
                            break;
                        case 7:
                            name = terms?.FirstOrDefault(p => p.Id == 11)?.Name.ToLower() ?? "juli";
                            break;
                        case 8:
                            name = terms?.FirstOrDefault(p => p.Id == 12)?.Name.ToLower() ?? "augusti";
                            break;
                        case 9:
                            name = terms?.FirstOrDefault(p => p.Id == 13)?.Name.ToLower() ?? "september";
                            break;
                        case 10:
                            name = terms?.FirstOrDefault(p => p.Id == 14)?.Name.ToLower() ?? "oktober";
                            break;
                        case 11:
                            name = terms?.FirstOrDefault(p => p.Id == 15)?.Name.ToLower() ?? "november";
                            break;
                        case 12:
                            name = terms?.FirstOrDefault(p => p.Id == 16)?.Name.ToLower() ?? "december";
                            break;
                    }

                    #endregion
                    break;
                case (int)AttestPeriodType.Period:
                    #region Period

                    name = timePeriod?.Name ?? string.Empty;

                    #endregion
                    break;
            }

            return name;
        }

        #endregion

        #region AttestRole

        public List<AttestRole> GetAttestRoles(int actorCompanyId, SoeModule module = SoeModule.None, bool includeInactive = false, bool loadAttestRoleUser = false, bool loadExternalCode = false, bool onlyHumanResourcesPrivacy = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRole.NoTracking();
            return GetAttestRoles(entities, actorCompanyId, module, includeInactive, loadAttestRoleUser, loadExternalCode, onlyHumanResourcesPrivacy);
        }

        public List<AttestRole> GetAttestRoles(CompEntities entities, int actorCompanyId, SoeModule module = SoeModule.None, bool includeInactive = false, bool loadAttestRoleUser = false, bool loadExternalCode = false, bool onlyHumanResourcesPrivacy = false)
        {
            var query = entities.AttestRole.Where(ar => ar.ActorCompanyId == actorCompanyId);
            if (loadAttestRoleUser)
                query = query.Include("AttestRoleUser.Account");

            if (module != SoeModule.None)
                query = query.Where(ar => ar.Module == (int)module);

            if (includeInactive)
                query = query.Where(ar => ar.State < (int)SoeEntityState.Deleted);
            else
                query = query.Where(ar => ar.State == (int)SoeEntityState.Active);

            if (onlyHumanResourcesPrivacy)
                query = query.Where(ar => ar.HumanResourcesPrivacy);

            List<AttestRole> attestRoles = query.ToList();
            if (loadExternalCode)
                LoadAttestRoleExternalCodes(entities, attestRoles, actorCompanyId);

            return attestRoles.OrderBy(ar => ar.Name).ToList();
        }

        public List<AttestRole> GetAttestRolesForUser(int actorCompanyId, int userId, DateTime? date = null, SoeModule module = SoeModule.None, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestUserRoleView.NoTracking();
            entities.AttestRole.NoTracking();
            return GetAttestRolesForUser(entities, userId, actorCompanyId, date, module, loadExternalCode);
        }

        public List<AttestRole> GetAttestRolesForUser(CompEntities entities, int userId, int actorCompanyId, DateTime? date = null, SoeModule module = SoeModule.None, bool loadExternalCode = false)
        {
            IQueryable<AttestUserRoleNoTransitionsView> query = GetAttestUserRoleNoTransitionsViewsQuery(entities, userId, actorCompanyId, date, module);
            if (query == null)
                return new List<AttestRole>();

            var attestRolesQuery = (from ar in entities.AttestRole
                                    join q in query on ar.AttestRoleId equals q.AttestRoleId
                                    where ar.ActorCompanyId == actorCompanyId &&
                                    ar.State < (int)SoeEntityState.Deleted
                                    orderby ar.Name
                                    select ar);

            List<AttestRole> attestRoles = attestRolesQuery.Distinct().ToList();
            if (loadExternalCode)
                LoadAttestRoleExternalCodes(entities, attestRoles, actorCompanyId);

            return attestRoles;
        }

        public List<AttestRole> GetAttestRolesAndRoleUser(int actorCompanyId, SoeModule module = SoeModule.None, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRole.NoTracking();
            return GetAttestRolesAndRoleUser(entities, actorCompanyId, module, loadExternalCode);
        }

        public List<AttestRole> GetAttestRolesAndRoleUser(CompEntities entities, int actorCompanyId, SoeModule module = SoeModule.None, bool loadExternalCode = false)
        {
            int moduleId = (int)module;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var attestRoles = (from ar in entitiesReadOnly.AttestRole
                                .Include("AttestRoleUser.User")
                               where ar.ActorCompanyId == actorCompanyId &&
                               (ar.Module == moduleId || module == SoeModule.None) &&
                               ar.State < (int)SoeEntityState.Deleted
                               orderby ar.Name
                               select ar).ToList();

            if (loadExternalCode)
                LoadAttestRoleExternalCodes(entities, attestRoles, actorCompanyId);

            return attestRoles;
        }

        public List<AttestRole> GetAttestRolesForAttestTransition(int attestTransitionId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            AttestTransition attestTransition = entitiesReadOnly.AttestTransition.Include("AttestRole").FirstOrDefault(ar => ar.AttestTransitionId == attestTransitionId);
            return attestTransition?.AttestRole?.ToList() ?? new List<AttestRole>();
        }

        public Dictionary<int, string> GetAttestRolesDict(int actorCompanyId, SoeModule module, bool addEmptyRow, bool onlyHumanResourcesPrivacy = false)
        {
            Dictionary<int, string> dict = GetAttestRoles(actorCompanyId, module, onlyHumanResourcesPrivacy: onlyHumanResourcesPrivacy).ToDictionary(k => k.AttestRoleId, v => v.Name);
            if (addEmptyRow)
                dict.Add(0, " ");
            return dict;
        }

        public AttestRole GetAttestRole(int attestRoleId, int actorCompanyId, bool loadAttestRoleUser = false, bool loadTransitions = false, bool loadExternalCode = false, bool loadCategories = false, bool loadAttestRoleMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRole.NoTracking();
            return GetAttestRole(entities, attestRoleId, actorCompanyId, loadAttestRoleUser, loadTransitions, loadExternalCode, loadCategories, loadAttestRoleMapping);
        }

        public AttestRole GetAttestRole(CompEntities entities, int attestRoleId, int actorCompanyId, bool loadAttestRoleUser = false, bool loadTransitions = false, bool loadExternalCode = false, bool loadCategories = false, bool loadAttestRoleMapping = false)
        {
            AttestRole attestRole = entities.AttestRole.FirstOrDefault(ar => ar.AttestRoleId == attestRoleId && ar.State < (int)SoeEntityState.Deleted);
            if (attestRole == null)
                return null;

            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            if (loadAttestRoleUser && !attestRole.AttestRoleUser.IsLoaded)
                attestRole.AttestRoleUser.Load();
            if (loadAttestRoleMapping)
                attestRole.ParentAttestRoleMapping.Load();
            if (loadTransitions)
                LoadAttestRoleTransitions(attestRole);
            if (loadExternalCode)
                LoadAttestRoleExternalCodes(entities, attestRole);
            if (loadCategories && !useAccountHierarchy)
            {
                attestRole.PrimaryCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, attestRoleId, actorCompanyId);
                attestRole.SecondaryCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, attestRoleId, actorCompanyId);
            }

            return attestRole;
        }

        public AttestRole GetPrevNextAttestRole(int attestRoleId, int actorCompanyId, SoeFormMode mode)
        {
            AttestRole attestRole = null;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestRole.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                attestRole = (from ar in entitiesReadOnly.AttestRole
                              where (ar.AttestRoleId > attestRoleId) &&
                              ar.ActorCompanyId == actorCompanyId &&
                              ar.State == (int)SoeEntityState.Active
                              orderby ar.AttestRoleId
                              select ar).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                attestRole = (from ar in entitiesReadOnly.AttestRole
                              where (ar.AttestRoleId < attestRoleId) &&
                              ar.ActorCompanyId == actorCompanyId &&
                              ar.State == (int)SoeEntityState.Active
                              orderby ar.AttestRoleId descending
                              select ar).FirstOrDefault();
            }

            if (attestRole != null && !attestRole.AttestTransition.IsLoaded)
                attestRole.AttestTransition.Load();

            return attestRole;
        }

        public bool ExistsAttestRole(string name, int actorCompanyId, SoeModule module)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRole.NoTracking();
            return ExistsAttestRole(entities, name, actorCompanyId, module);
        }

        public bool ExistsAttestRole(CompEntities entities, string name, int actorCompanyId, SoeModule module)
        {
            return (from ar in entities.AttestRole
                    where ar.ActorCompanyId == actorCompanyId &&
                    ar.Name == name &&
                    ar.Module == (int)module &&
                    ar.State == (int)SoeEntityState.Active
                    select ar).Any();
        }

        public bool AlsoAttestAdditionsFromTime(CompEntities entities, int userId, int actorCompanyId, DateTime? date, EmployeeAuthModelRepository repository = null)
        {
            if (repository?.AttestRoleUsers.All(aru => aru.AttestRole != null) ?? false)
                return repository.AttestRoleUsers.AlsoAttestAdditionsFromTime(date);
            else
                return AttestManager.GetAttestRolesForUser(entities, userId, actorCompanyId, date: date).Any(ar => ar.AlsoAttestAdditionsFromTime);
        }

        private void LoadAttestRoleTransitions(AttestRole attestRole)
        {
            if (attestRole == null)
                return;

            if (!attestRole.AttestTransition.IsLoaded)
                attestRole.AttestTransition.Load();

            foreach (AttestTransition transition in attestRole.AttestTransition)
            {
                if (!transition.AttestStateFromReference.IsLoaded)
                    transition.AttestStateFromReference.Load();
            }
        }

        public void LoadAttestRoleExternalCodes(CompEntities entities, List<AttestRole> attestRoles, int actorCompanyId)
        {
            if (attestRoles.IsNullOrEmpty())
                return;

            List<CompanyExternalCode> externalCodes = ActorManager.TryPreloadCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRoles.Select(i => i.AttestRoleId).ToList(), actorCompanyId);
            foreach (AttestRole attestRole in attestRoles)
            {
                LoadAttestRoleExternalCodes(entities, attestRole, externalCodes);
            }
        }

        private void LoadAttestRoleExternalCodes(CompEntities entities, AttestRole attestRole, List<CompanyExternalCode> externalCodes = null)
        {
            if (attestRole == null)
                return;

            //Only load if not already loaded
            if (!attestRole.ExternalCodes.IsNullOrEmpty())
                return;

            if (externalCodes != null)
                externalCodes = externalCodes.Where(i => i.RecordId == attestRole.AttestRoleId).ToList();
            else
                externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRole.AttestRoleId, attestRole.ActorCompanyId);

            attestRole.ExternalCodes = new List<string>();

            if (!externalCodes.IsNullOrEmpty())
            {
                attestRole.ExternalCodes.AddRange(externalCodes.Select(s => s.ExternalCode));
                attestRole.ExternalCodesString = StringUtility.GetSeparatedString(externalCodes.Select(s => s.ExternalCode), Constants.Delimiter, true, false);
            }
        }

        public ActionResult AddAttestRole(AttestRole attestRole)
        {
            if (attestRole == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRole");

            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = AddEntityItem(entities, attestRole, "AttestRole");
                if (!result.Success)
                    return result;

                TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, attestRole.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonInsert, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole, attestRole.AttestRoleId));

                if (!string.IsNullOrEmpty(attestRole.ExternalCodesString))
                    result = ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRole.AttestRoleId, attestRole.ExternalCodesString, attestRole.ActorCompanyId);

                return result;
            }
        }

        public ActionResult UpdateAttestRole(AttestRole attestRole, int actorCompanyId)
        {
            if (attestRole == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRole");

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();

            using (CompEntities entities = new CompEntities())
            {
                AttestRole originalAttestRole = GetAttestRole(entities, attestRole.AttestRoleId, actorCompanyId, loadExternalCode: true);
                if (originalAttestRole == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                ActionResult result;

                #region Track changes

                #region AttestRole

                if (attestRole.Name != originalAttestRole.Name)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_Name, originalAttestRole.Name, attestRole.Name));

                if (attestRole.Description != originalAttestRole.Description)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_Description, originalAttestRole.Description, attestRole.Description));

                if (attestRole.ExternalCodesString != originalAttestRole.ExternalCodesString)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_ExternalCodes, originalAttestRole.ExternalCodesString, attestRole.ExternalCodesString));

                if (attestRole.Sort != originalAttestRole.Sort)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_Sort, originalAttestRole.Sort.ToString(), attestRole.Sort.ToString()));

                if (attestRole.DefaultMaxAmount.ToString("N2") != originalAttestRole.DefaultMaxAmount.ToString("N2"))
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Decimal, null, TermGroup_TrackChangesColumnType.AttestRole_DefaultMaxAmount, originalAttestRole.DefaultMaxAmount.ToString("N2"), attestRole.DefaultMaxAmount.ToString("N2")));

                #endregion

                #region Transitions

                // Transitions are currently tracked in SaveAttestRoleTransitions

                #endregion

                #region Reminder

                if (attestRole.ReminderAttestStateId != originalAttestRole.ReminderAttestStateId)
                {
                    string fromValueName = originalAttestRole.ReminderAttestStateId.HasValue ? GetAttestStateName(entities, originalAttestRole.ReminderAttestStateId.Value) : null;
                    string toValueName = attestRole.ReminderAttestStateId.HasValue ? GetAttestStateName(entities, attestRole.ReminderAttestStateId.Value) : null;
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderAttestStateId, originalAttestRole.ReminderAttestStateId.ToString(), attestRole.ReminderAttestStateId.ToString(), fromValueName, toValueName));
                }

                if (attestRole.ReminderNoOfDays != originalAttestRole.ReminderNoOfDays)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderNoOfDays, originalAttestRole.ReminderNoOfDays.ToString(), attestRole.ReminderNoOfDays.ToString()));

                if (attestRole.ReminderPeriodType != originalAttestRole.ReminderPeriodType)
                {
                    Dictionary<int, string> periods = new Dictionary<int, string>
                    {
                        { (int)AttestPeriodType.Day, GetText(7162, "dagen") },
                        { (int)AttestPeriodType.Week, GetText(7163, "veckan") },
                        { (int)AttestPeriodType.Month, GetText(7164, "månaden") },
                        { (int)AttestPeriodType.Period, GetText(7165, "perioden") }
                    };
                    string fromValueName = originalAttestRole.ReminderPeriodType.HasValue && periods.ContainsKey(originalAttestRole.ReminderPeriodType.Value) ? periods[originalAttestRole.ReminderPeriodType.Value] : null;
                    string toValueName = attestRole.ReminderPeriodType.HasValue && periods.ContainsKey(attestRole.ReminderPeriodType.Value) ? periods[attestRole.ReminderPeriodType.Value] : null;
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderPeriodType, originalAttestRole.ReminderPeriodType.ToString(), attestRole.ReminderPeriodType.ToString(), fromValueName, toValueName));
                }

                #endregion

                #region Settings

                if (attestRole.ShowUncategorized != originalAttestRole.ShowUncategorized)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowUncategorized, originalAttestRole.ShowUncategorized.ToString(), attestRole.ShowUncategorized.ToString()));

                if (attestRole.ShowAllCategories != originalAttestRole.ShowAllCategories)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowAllCategories, originalAttestRole.ShowAllCategories.ToString(), attestRole.ShowAllCategories.ToString()));

                if (attestRole.ShowAllSecondaryCategories != originalAttestRole.ShowAllSecondaryCategories)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowAllSecondaryCategories, originalAttestRole.ShowAllSecondaryCategories.ToString(), attestRole.ShowAllSecondaryCategories.ToString()));

                if (attestRole.ShowTemplateSchedule != originalAttestRole.ShowTemplateSchedule)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowTemplateSchedule, originalAttestRole.ShowTemplateSchedule.ToString(), attestRole.ShowTemplateSchedule.ToString()));

                if (attestRole.AlsoAttestAdditionsFromTime != originalAttestRole.AlsoAttestAdditionsFromTime)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_AlsoAttestAdditionsFromTime, originalAttestRole.AlsoAttestAdditionsFromTime.ToString(), attestRole.AlsoAttestAdditionsFromTime.ToString()));

                if (attestRole.HumanResourcesPrivacy != originalAttestRole.HumanResourcesPrivacy)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_HumanResourcesPrivacy, originalAttestRole.HumanResourcesPrivacy.ToString(), attestRole.HumanResourcesPrivacy.ToString()));

                if (attestRole.AttestByEmployeeAccount != originalAttestRole.AttestByEmployeeAccount)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_AttestByEmployeeAccount, originalAttestRole.AttestByEmployeeAccount.ToString(), attestRole.AttestByEmployeeAccount.ToString()));

                if (attestRole.StaffingByEmployeeAccount != originalAttestRole.StaffingByEmployeeAccount)
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SoeEntityType.AttestRole, originalAttestRole.AttestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_StaffingByEmployeeAccount, originalAttestRole.StaffingByEmployeeAccount.ToString(), attestRole.StaffingByEmployeeAccount.ToString()));

                #endregion

                #region Categories


                #endregion

                #endregion

                if (string.IsNullOrEmpty(attestRole.ExternalCodesString))
                {
                    ActorManager.DeleteExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRole.AttestRoleId, attestRole.ActorCompanyId);
                    result = UpdateEntityItem(entities, originalAttestRole, attestRole, "AttestRole");
                }
                else
                {
                    result = UpdateEntityItem(entities, originalAttestRole, attestRole, "AttestRole");
                    ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRole.AttestRoleId, attestRole.ExternalCodesString, attestRole.ActorCompanyId);
                }

                if (result.Success && trackChangesItems.Any())
                    result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);

                return result;
            }
        }

        public ActionResult SaveAttestRole(AttestRoleDTO attestRoleInput, int actorCompanyId, List<int> selectedTransitionIds)
        {
            if (attestRoleInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRole");

            // Default result is successful
            ActionResult result = new ActionResult();

            int attestRoleId = attestRoleInput.AttestRoleId;
            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool isNew = false;

                        #region AttestRole

                        AttestRole attestRole = GetAttestRole(entities, attestRoleId, actorCompanyId, loadTransitions: true, loadExternalCode: true);
                        if (attestRole == null)
                        {
                            isNew = true;

                            if (ExistsAttestRole(entities, attestRoleInput.Name, actorCompanyId, attestRoleInput.Module))
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(5234, "Attestroll finns redan"));

                            #region Add

                            attestRole = new AttestRole()
                            {
                                ActorCompanyId = actorCompanyId,
                                Module = (int)attestRoleInput.Module,
                            };
                            SetCreatedProperties(attestRole);
                            entities.AttestRole.AddObject(attestRole);

                            #endregion
                        }
                        else
                        {
                            if (attestRole.Name != attestRoleInput.Name && ExistsAttestRole(entities, attestRoleInput.Name, actorCompanyId, attestRoleInput.Module))
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(5234, "Attestroll finns redan"));

                            #region Update

                            #region TrackChanges

                            #region AttestRole
                            if (attestRole.Name != attestRoleInput.Name)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_Name, attestRole.Name, attestRoleInput.Name));

                            if (attestRole.Description != attestRoleInput.Description)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_Description, attestRole.Description, attestRoleInput.Description));

                            if (attestRole.ExternalCodesString != attestRoleInput.ExternalCodesString)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.AttestRole_ExternalCodes, attestRole.ExternalCodesString, attestRoleInput.ExternalCodesString));

                            if (attestRole.DefaultMaxAmount.ToString("N2") != attestRoleInput.DefaultMaxAmount.ToString("N2"))
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Decimal, null, TermGroup_TrackChangesColumnType.AttestRole_DefaultMaxAmount, attestRole.DefaultMaxAmount.ToString("N2"), attestRoleInput.DefaultMaxAmount.ToString("N2")));

                            #endregion

                            #region Reminder

                            if (attestRole.ReminderAttestStateId != attestRoleInput.ReminderAttestStateId)
                            {
                                string fromValueName = attestRole.ReminderAttestStateId.HasValue ? GetAttestStateName(entities, attestRole.ReminderAttestStateId.Value) : null;
                                string toValueName = attestRoleInput.ReminderAttestStateId.HasValue ? GetAttestStateName(entities, attestRoleInput.ReminderAttestStateId.Value) : null;
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderAttestStateId, attestRole.ReminderAttestStateId.ToString(), attestRoleInput.ReminderAttestStateId.ToString(), fromValueName, toValueName));
                            }

                            if (attestRole.ReminderNoOfDays != attestRoleInput.ReminderNoOfDays)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderNoOfDays, attestRole.ReminderNoOfDays.ToString(), attestRoleInput.ReminderNoOfDays.ToString()));

                            if (attestRole.ReminderPeriodType != attestRoleInput.ReminderPeriodType)
                            {
                                Dictionary<int, string> periods = new Dictionary<int, string>
                                {
                                    { (int)AttestPeriodType.Day, GetText(7162, "dagen") },
                                    { (int)AttestPeriodType.Week, GetText(7163, "veckan") },
                                    { (int)AttestPeriodType.Month, GetText(7164, "månaden") },
                                    { (int)AttestPeriodType.Period, GetText(7165, "perioden") }
                                };
                                string fromValueName = attestRole.ReminderPeriodType.HasValue && periods.ContainsKey(attestRole.ReminderPeriodType.Value) ? periods[attestRole.ReminderPeriodType.Value] : null;
                                string toValueName = attestRoleInput.ReminderPeriodType.HasValue && periods.ContainsKey(attestRoleInput.ReminderPeriodType.Value) ? periods[attestRoleInput.ReminderPeriodType.Value] : null;
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_ReminderPeriodType, attestRole.ReminderPeriodType.ToString(), attestRoleInput.ReminderPeriodType.ToString(), fromValueName, toValueName));
                            }

                            #endregion

                            #region Transitions

                            // Transitions are currently tracked in SaveAttestRoleTransitions

                            #endregion

                            #region Settings

                            if (attestRole.ShowUncategorized != attestRoleInput.ShowUncategorized)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowUncategorized, attestRole.ShowUncategorized.ToString(), attestRoleInput.ShowUncategorized.ToString()));

                            if (attestRole.ShowAllCategories != attestRoleInput.ShowAllCategories)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowAllCategories, attestRole.ShowAllCategories.ToString(), attestRoleInput.ShowAllCategories.ToString()));

                            if (attestRole.ShowAllSecondaryCategories != attestRoleInput.ShowAllSecondaryCategories)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowAllSecondaryCategories, attestRole.ShowAllSecondaryCategories.ToString(), attestRoleInput.ShowAllSecondaryCategories.ToString()));

                            if (attestRole.ShowTemplateSchedule != attestRoleInput.ShowTemplateSchedule)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_ShowTemplateSchedule, attestRole.ShowTemplateSchedule.ToString(), attestRoleInput.ShowTemplateSchedule.ToString()));

                            if (attestRole.AlsoAttestAdditionsFromTime != attestRoleInput.AlsoAttestAdditionsFromTime)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_AlsoAttestAdditionsFromTime, attestRole.AlsoAttestAdditionsFromTime.ToString(), attestRoleInput.AlsoAttestAdditionsFromTime.ToString()));

                            if (attestRole.HumanResourcesPrivacy != attestRoleInput.HumanResourcesPrivacy)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_HumanResourcesPrivacy, attestRole.HumanResourcesPrivacy.ToString(), attestRoleInput.HumanResourcesPrivacy.ToString()));

                            if (attestRole.AttestByEmployeeAccount != attestRoleInput.AttestByEmployeeAccount)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_AttestByEmployeeAccount, attestRole.AttestByEmployeeAccount.ToString(), attestRoleInput.AttestByEmployeeAccount.ToString()));

                            if (attestRole.StaffingByEmployeeAccount != attestRoleInput.StaffingByEmployeeAccount)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_StaffingByEmployeeAccount, attestRole.StaffingByEmployeeAccount.ToString(), attestRoleInput.StaffingByEmployeeAccount.ToString()));

                            if (attestRole.AllowToAddOtherEmployeeAccounts != attestRoleInput.AllowToAddOtherEmployeeAccounts)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.AttestRole, attestRoleId, SoeEntityType.AttestRole, attestRoleId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.AttestRole_AllowToAddOtherEmployeeAccounts, attestRole.AllowToAddOtherEmployeeAccounts.ToString(), attestRoleInput.AllowToAddOtherEmployeeAccounts.ToString()));

                            #endregion

                            #region Categories


                            #endregion

                            #endregion

                            #endregion

                            SetModifiedProperties(attestRole);
                        }

                        #endregion

                        attestRole.Name = attestRoleInput.Name;
                        attestRole.Description = attestRoleInput.Description;
                        attestRole.DefaultMaxAmount = attestRoleInput.DefaultMaxAmount;
                        attestRole.ShowUncategorized = attestRoleInput.ShowUncategorized;
                        attestRole.ShowAllCategories = attestRoleInput.ShowAllCategories;
                        attestRole.ShowAllSecondaryCategories = attestRoleInput.ShowAllSecondaryCategories;
                        attestRole.ShowTemplateSchedule = attestRoleInput.ShowTemplateSchedule;
                        attestRole.AlsoAttestAdditionsFromTime = attestRoleInput.AlsoAttestAdditionsFromTime;
                        attestRole.AttestByEmployeeAccount = attestRoleInput.AttestByEmployeeAccount;
                        attestRole.StaffingByEmployeeAccount = attestRoleInput.StaffingByEmployeeAccount;
                        attestRole.IsExecutive = attestRoleInput.IsExecutive;
                        attestRole.HumanResourcesPrivacy = attestRoleInput.HumanResourcesPrivacy;
                        attestRole.AllowToAddOtherEmployeeAccounts = attestRoleInput.AllowToAddOtherEmployeeAccounts;
                        attestRole.ExternalCodesString = attestRoleInput.ExternalCodesString;
                        attestRole.Sort = attestRoleInput.Sort;

                        if (attestRoleInput.State < SoeEntityState.Deleted && (int)attestRoleInput.State != attestRole.State)
                            attestRole.State = (int)attestRoleInput.State;

                        if (attestRoleInput.ReminderAttestStateId != 0 && attestRoleInput.ReminderPeriodType != 0)
                        {
                            attestRole.ReminderAttestStateId = attestRoleInput.ReminderAttestStateId;
                            attestRole.ReminderNoOfDays = attestRoleInput.ReminderNoOfDays;
                            attestRole.ReminderPeriodType = attestRoleInput.ReminderPeriodType;
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        attestRoleId = attestRole.AttestRoleId;

                        result = SaveAttestRoleTransitions(entities, attestRole, selectedTransitionIds, actorCompanyId);
                        if (!result.Success)
                            return result;

                        if (attestRoleInput.PrimaryCategoryRecords != null)
                        {
                            result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, attestRoleInput.PrimaryCategoryRecords, actorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, attestRoleId);
                            if (!result.Success)
                                return result;
                        }
                        if (attestRoleInput.SecondaryCategoryRecords != null)
                        {
                            result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, attestRoleInput.SecondaryCategoryRecords, actorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, attestRoleId);
                            if (!result.Success)
                                return result;
                        }

                        if (attestRoleInput.AttestRoleMapping != null)
                        {
                            result = SaveAttestRoleMapping(entities, transaction, attestRoleId, attestRoleInput.AttestRoleMapping);
                            if (!result.Success)
                                return result;
                        }

                        if (isNew)
                        {
                            result = TrackChangesManager.AddTrackChanges(entities, transaction, TrackChangesManager.InitTrackChanges(entities, attestRole.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonInsert, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole, attestRole.AttestRoleId));
                            if (!result.Success)
                                return result;

                            if (!string.IsNullOrEmpty(attestRole.ExternalCodesString))
                                result = ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRole.AttestRoleId, attestRole.ExternalCodesString, attestRole.ActorCompanyId);

                            if (!result.Success)
                                return result;
                        }
                        else
                        {
                            if (trackChangesItems.Any())
                                result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

                            if (!result.Success)
                                return result;

                            if (string.IsNullOrEmpty(attestRoleInput.ExternalCodesString))
                                result = ActorManager.DeleteExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRoleId, attestRole.ActorCompanyId);
                            else
                                result = ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRoleId, attestRoleInput.ExternalCodesString, attestRole.ActorCompanyId);

                            if (!result.Success)
                                return result;
                        }

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestRoleId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }
        public ActionResult UpdateAttestRoleState(Dictionary<int, bool> roles, int actorCompanyId, SoeModule module)
        {
            using (var entities = new CompEntities())
            {
                var orginalAttestRoles = GetAttestRoles(entities, actorCompanyId, module, includeInactive: true);

                foreach (KeyValuePair<int, bool> role in roles)
                {
                    var orginalAttestRole = orginalAttestRoles.FirstOrDefault(w => w.AttestRoleId == role.Key);
                    if (orginalAttestRole == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                    ChangeEntityState(orginalAttestRole, role.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveAttestRoleTransitions(CompEntities entities, AttestRole attestRole, List<int> selectedTransitionIds, int actorCompanyId)
        {
            if (selectedTransitionIds == null)
                return new ActionResult();

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();


            #region Track changes

            List<int> originalTransitions = attestRole.AttestTransition.Select(t => t.AttestTransitionId).ToList();
            List<int> currentTransitions = new List<int>();
            currentTransitions.AddRange(selectedTransitionIds);

            List<int> unchangedTransitions = originalTransitions.Intersect(currentTransitions).ToList();
            // Remove unchanged (intersecting) transitions, leaving only new and deleted
            foreach (int id in unchangedTransitions)
            {
                originalTransitions.Remove(id); // Anyone left here will be deleted (does not exist in current)
                currentTransitions.Remove(id);  // Anyone left here will be added   (does not exist in original)
            }

            // Deleted
            foreach (int id in originalTransitions)
            {
                AttestTransition trans = entities.AttestTransition.FirstOrDefault(t => t.AttestTransitionId == id);
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole_AttestTransition, id, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_AttestTransition, id.ToString(), null, trans?.Name));
            }

            // Added
            foreach (int id in currentTransitions)
            {
                AttestTransition trans = entities.AttestTransition.FirstOrDefault(t => t.AttestTransitionId == id);
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole_AttestTransition, id, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_AttestTransition, null, id.ToString(), null, trans?.Name));
            }

            #endregion

            // Delete all current AttestRoleTransitions
            attestRole.AttestTransition.Clear();

            // Add new transitions, from the input collection
            foreach (int transitionId in selectedTransitionIds)
            {

                // Prevent duplicates
                var record = attestRole.AttestTransition.FirstOrDefault(a => a.AttestTransitionId == transitionId);
                if (record != null)
                    continue;

                // Get attest transition
                AttestTransition transition = GetAttestTransition(entities, transitionId);
                if (transition != null)
                    attestRole.AttestTransition.Add(transition);
            }

            ActionResult result = SaveChanges(entities);
            if (result.Success && trackChangesItems.Any())
                result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);

            return result;

        }

        public ActionResult SaveAttestRoleMapping(CompEntities entities, TransactionScope transaction, int attestRoleId, List<AttestRoleMappingDTO> attestRoleMappingsInput)
        {
            List<AttestRoleMapping> attestRoleMapping = (from a in entities.AttestRoleMapping
                                                         where a.ParentAttestRoleId == attestRoleId &&
                                                         a.State == (int)SoeEntityState.Active
                                                         select a).ToList();


            #region AttestRuleRow Update/Delete

            // Update or Delete existing AttestRuleRows
            foreach (AttestRoleMapping existingAttestRoleMapping in attestRoleMapping.Where(r => r.State == (int)SoeEntityState.Active))
            {
                // Try get AttestRuleRow from input
                AttestRoleMappingDTO attestRoleMappingInput = (from r in attestRoleMappingsInput
                                                               where r.AttestRoleMappingId == existingAttestRoleMapping.AttestRoleMappingId
                                                               select r).FirstOrDefault();

                if (attestRoleMappingInput != null)
                {
                    #region Update

                    SetEntityProperties(existingAttestRoleMapping, attestRoleMappingInput);

                    #endregion
                }
                else
                {
                    #region Delete

                    // Delete existing row
                    ChangeEntityState(existingAttestRoleMapping, SoeEntityState.Deleted);

                    #endregion
                }

            }

            #endregion

            #region Add

            foreach (AttestRoleMappingDTO attestRoleMappingToAdd in attestRoleMappingsInput.Where(x => x.AttestRoleMappingId == 0).ToList())
                SetEntityProperties(new AttestRoleMapping(), attestRoleMappingToAdd);

            #endregion


            #region Local functions

            void SetEntityProperties(AttestRoleMapping mapping, AttestRoleMappingDTO attestRoleMappingInput)
            {
                mapping.ParentAttestRoleId = attestRoleId;
                mapping.ChildAttestRoleId = attestRoleMappingInput.ChildtAttestRoleId;
                mapping.DateFrom = attestRoleMappingInput.DateFrom;
                mapping.DateTo = attestRoleMappingInput.DateTo;
                mapping.Entity = (int)attestRoleMappingInput.Entity;
                mapping.State = (int)attestRoleMappingInput.State;

                if (mapping.AttestRoleMappingId == 0)
                {
                    SetCreatedProperties(mapping);
                    entities.AttestRoleMapping.AddObject(mapping);
                }
                else
                    SetModifiedProperties(mapping);
            }

            #endregion

            return SaveChanges(entities, transaction);
        }

        public ActionResult DeleteAttestRole(int attestRoleId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                AttestRole attestRole = GetAttestRole(entities, attestRoleId, actorCompanyId, loadAttestRoleUser: true);
                if (attestRole == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                // Check relation dependencies
                if (attestRole.AttestRoleUser != null && attestRole.AttestRoleUser.Any())
                    return new ActionResult((int)ActionResultDelete.AttestRoleHasUsers, GetText(5237, "Attestroll kunde inte tas bort, kontrollera att det inte används"));

                if (attestRole.IsSigningRole)
                {
                    attestRole.ChildAttestRoleMapping.Load();
                    if (attestRole.ChildAttestRoleMapping != null && attestRole.ChildAttestRoleMapping.Any(x => x.State == (int)SoeEntityState.Active))
                        return new ActionResult((int)ActionResultDelete.AttestRoleIsInUse, GetText(8927, "Signeringsrollen kunde inte tas bort, rollen är kopplad till en attestroll."));
                }
                ActionResult result = ChangeEntityState(entities, attestRole, SoeEntityState.Deleted, true);
                if (!result.Success)
                    return result;

                TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, attestRole.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonDelete, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole, attestRole.AttestRoleId));

                return result;
            }
        }

        public ActionResult DeleteAttestRole(AttestRole attestRole, int actorCompanyId)
        {
            if (attestRole == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AttestRole");

            using (CompEntities entities = new CompEntities())
            {
                AttestRole orginalAttestRole = GetAttestRole(entities, attestRole.AttestRoleId, actorCompanyId, loadAttestRoleUser: true);
                if (orginalAttestRole == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                // Check relation dependencies
                if (orginalAttestRole.AttestRoleUser != null && orginalAttestRole.AttestRoleUser.Any(x => x.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.AttestRoleHasUsers);

                ActionResult result = ChangeEntityState(entities, orginalAttestRole, SoeEntityState.Deleted, true);
                if (!result.Success)
                    return result;

                TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, attestRole.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonDelete, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, orginalAttestRole.AttestRoleId, SoeEntityType.AttestRole, orginalAttestRole.AttestRoleId));

                return result;
            }
        }

        #endregion

        #region AttestState

        public List<AttestState> GetAttestStates(int actorCompanyId, List<int> attestStateIds = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetAttestStates(entities, actorCompanyId, attestStateIds);
        }

        public List<AttestState> GetAttestStates(CompEntities entities, int actorCompanyId, List<int> attestStateIds = null)
        {
            IQueryable<AttestState> query = (from a in entities.AttestState
                                             where a.ActorCompanyId == actorCompanyId &&
                                             a.State == (int)SoeEntityState.Active
                                             select a);

            if (attestStateIds != null)
                query = query.Where(a => attestStateIds.Contains(a.AttestStateId));

            return query.ToList();
        }

        public List<AttestState> GetAttestStates(int actorCompanyId, List<TermGroup_AttestEntity> entitys, SoeModule module, bool addEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return this.GetAttestStates(entitiesReadOnly, actorCompanyId, entitys, module, addEmptyRow);
        }

        public List<AttestState> GetAttestStates(CompEntities entities, int actorCompanyId, List<TermGroup_AttestEntity> entitys, SoeModule module, bool addEmptyRow = false)
        {
            List<AttestState> attestStates = new List<AttestState>();
            foreach (TermGroup_AttestEntity entity in entitys)
            {
                attestStates.AddRange(GetAttestStates(entities, actorCompanyId, entity, module, addEmptyRow));
            }
            return attestStates;
        }

        public List<AttestState> GetAttestStates(int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, bool addEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetAttestStates(entities, actorCompanyId, entity, module, addEmptyRow);
        }

        public List<AttestState> GetAttestStates(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, bool addEmptyRow = false)
        {
            int entityId = (int)entity;
            int moduleId = (int)module;

            List<AttestState> attestStates = (from a in entities.AttestState
                                              where a.ActorCompanyId == actorCompanyId &&
                                              (entityId == (int)TermGroup_AttestEntity.Unknown || a.Entity == entityId) &&
                                              (moduleId == (int)SoeModule.None || a.Module == moduleId) &&
                                              a.State == (int)SoeEntityState.Active
                                              select a).ToList();

            attestStates = attestStates.OrderBy(a => a.Entity).ThenByDescending(a => a.Initial).ThenBy(a => a.Sort).ToList();

            if (addEmptyRow)
            {
                attestStates.Insert(0, new AttestState()
                {
                    AttestStateId = 0,
                    Entity = (int)entity,
                    Name = " "
                });
            }

            return attestStates;
        }

        public List<AttestState> GetHiddenAttestStates(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity)
        {
            int entityId = (int)entity;

            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Hidden
                    select a).ToList();
        }

        public List<AttestStateDTO> GetUserValidAttestStates(TermGroup_AttestEntity entity, DateTime? dateFrom, DateTime? dateTo, bool excludePayrollStates, int? employeeGroupId = null)
        {
            var validAttestStates = new List<AttestStateDTO>();
            var allAttestStatesTo = new List<AttestStateDTO>();

            //May exclude
            int companyPayrollResultingAttestStateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, base.UserId, base.ActorCompanyId, 0);
            int companyPayrollLockedAttestStateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentLockedAttestStateId, base.UserId, base.ActorCompanyId, 0);
            int companyPayrollApproved1AttestStateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved1AttestStateId, base.UserId, base.ActorCompanyId, 0);
            int companyPayrollApproved2AttestStateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, base.UserId, base.ActorCompanyId, 0);

            //Always exclude
            int companyPayrollExportFileCreatedAttestStateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, base.UserId, base.ActorCompanyId, 0);

            if (employeeGroupId.HasValue)
            {
                var attestTransitions = GetAttestTransitionsForEmployeeGroup(entity, employeeGroupId.Value);
                allAttestStatesTo.AddRange(attestTransitions.GetAttestStatesTo(excludePayrollStates ? companyPayrollResultingAttestStateId : 0));
            }
            else
            {
                var attestTransitions = GetAttestTransitionsForAttestRoleUser(entity, base.ActorCompanyId, base.UserId, dateFrom, dateTo).ToDTOs(true).ToList();
                allAttestStatesTo.AddRange(attestTransitions.GetAttestStatesTo(excludePayrollStates ? companyPayrollResultingAttestStateId : 0));
            }

            foreach (var attestState in allAttestStatesTo)
            {
                if (attestState.HasId(companyPayrollExportFileCreatedAttestStateId))
                    continue;
                if (excludePayrollStates && attestState.HasId(companyPayrollLockedAttestStateId, companyPayrollApproved1AttestStateId, companyPayrollApproved2AttestStateId))
                    continue;
                if (attestState.AttestStateId == companyPayrollLockedAttestStateId && companyPayrollLockedAttestStateId == companyPayrollApproved1AttestStateId && companyPayrollApproved1AttestStateId == companyPayrollApproved2AttestStateId)
                    continue;

                validAttestStates.Add(attestState);
            }

            return validAttestStates;
        }

        public List<AttestStateDTO> GetValidAttestStatesForEmployee(int employeeId, int userId, int actorCompanyId, int timePeriodId, bool isMySelf)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetValidAttestStatesForEmployee(entities, employeeId, userId, actorCompanyId, timePeriodId, isMySelf);
        }

        public List<AttestStateDTO> GetValidAttestStatesForEmployee(CompEntities entities, int employeeId, int userId, int actorCompanyId, int timePeriodId, bool isMySelf)
        {
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId);
            if (timePeriod == null)
                return new List<AttestStateDTO>();

            return GetValidAttestStatesForEmployee(entities, employeeId, userId, actorCompanyId, timePeriod.StartDate, timePeriod.StopDate, isMySelf);
        }

        public List<AttestStateDTO> GetValidAttestStatesForEmployee(CompEntities entities, int employeeId, int userId, int actorCompanyId, DateTime startDate, DateTime stopDate, bool isMySelf)
        {
            Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true);
            Employment employment = employee?.GetEmployment(startDate, stopDate);
            if (employment == null)
                return new List<AttestStateDTO>();

            List<AttestTransitionDTO> attestTransitions;
            if (isMySelf)
                attestTransitions = AttestManager.GetAttestTransitionsForEmployeeGroup(entities, TermGroup_AttestEntity.PayrollTime, employment.GetEmployeeGroupId()).ToDTOs(true).ToList();
            else
                attestTransitions = AttestManager.GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompanyId, entity: TermGroup_AttestEntity.PayrollTime, dateFrom: startDate, dateTo: stopDate).ToDTOs(true).ToList();
            if (attestTransitions.IsNullOrEmpty())
                return new List<AttestStateDTO>();

            int companyPayrollResultingAttestStateId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, userId, actorCompanyId, 0);
            return attestTransitions.GetAttestStatesTo(companyPayrollResultingAttestStateId);
        }

        public List<int> GetHiddenAttestStateIds(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module)
        {
            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == (int)entity &&
                    a.Module == (int)module &&
                    a.State == (int)SoeEntityState.Active &&
                    a.Hidden
                    select a.AttestStateId).ToList();
        }

        public List<int> GetClosedAttestStatesIds(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity)
        {
            int entityId = (int)entity;
            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Closed
                    select a.AttestStateId).ToList();
        }

        public List<int> GetLockedAttestStatesIds(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity)
        {
            int entityId = (int)entity;
            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Locked
                    select a.AttestStateId).ToList();
        }

        public List<int> GetPayrollLockedAttestStateIds(CompEntities entities, int actorCompanyId, bool excludeExportPayrollResulting = false, bool includeExportPayrollMinimum = false)
        {
            return GetPayrollLockedAttestStateSettings(entities, actorCompanyId, excludeExportPayrollResulting, includeExportPayrollMinimum).Select(i => i.Value).ToList();
        }

        public Dictionary<int, string> GetAttestStatesDict(int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, bool addEmptyRow, bool addMultipleRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");
            if (addMultipleRow)
                dict.Add(-2, GetText(3468, "Flera status"));

            var attestStates = GetAttestStates(actorCompanyId, entity, module);
            foreach (var attestState in attestStates)
            {
                dict.Add(attestState.AttestStateId, attestState.Name);
            }

            return dict;
        }

        public Dictionary<int, List<AttestState>> GetTransactionAttestStatesByEmployee(int actorCompanyId, List<int> employeeIds, DateTime startDate, DateTime stopDate, List<AttestState> attestStates = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            entities.TimePayrollTransaction.NoTracking();
            return GetTransactionAttestStatesByEmployee(entities, actorCompanyId, employeeIds, startDate, stopDate, attestStates);
        }

        public Dictionary<int, List<AttestState>> GetTransactionAttestStatesByEmployee(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime startDate, DateTime stopDate, List<AttestState> attestStates = null)
        {
            Dictionary<int, List<AttestState>> transactionAttestStatesByEmployee = new Dictionary<int, List<AttestState>>();

            List<TimePayrollTransactionTreeDTO> transactions = TimeTransactionManager.GetTimePayrollTransactionsForTree(entities, actorCompanyId, startDate, stopDate, null, employeeIds);
            if (!transactions.IsNullOrEmpty())
            {
                if (attestStates == null)
                    attestStates = AttestManager.GetAttestStates(entities, actorCompanyId);

                if (!attestStates.IsNullOrEmpty())
                {
                    foreach (var transactionsGroupedByEmployee in transactions.GroupBy(i => i.EmployeeId))
                    {
                        List<int> transactionAttestStateIds = transactionsGroupedByEmployee.Select(i => i.AttestStateId).Distinct().ToList();
                        List<AttestState> transactionAttestStates = attestStates.Where(a => transactionAttestStateIds.Contains(a.AttestStateId)).ToList();
                        if (!transactionAttestStates.IsNullOrEmpty())
                            transactionAttestStatesByEmployee.Add(transactionsGroupedByEmployee.Key, transactionAttestStates);
                    }
                }
            }

            return transactionAttestStatesByEmployee;
        }

        public Dictionary<CompanySettingType, int> GetPayrollLockedAttestStateSettings(int actorCompanyId, bool excludeExportPayrollResulting = false, bool includeExportPayrollMinimum = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollLockedAttestStateSettings(entities, actorCompanyId, excludeExportPayrollResulting, includeExportPayrollMinimum);
        }

        public Dictionary<CompanySettingType, int> GetPayrollLockedAttestStateSettings(CompEntities entities, int actorCompanyId, bool excludeExportPayrollResulting = false, bool includeExportPayrollMinimum = false)
        {
            List<CompanySettingType> settingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.SalaryPaymentLockedAttestStateId,
                CompanySettingType.SalaryPaymentApproved1AttestStateId,
                CompanySettingType.SalaryPaymentApproved2AttestStateId,
                CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId,
            };
            if (!excludeExportPayrollResulting)
                settingTypes.Add(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
            if (includeExportPayrollMinimum)
                settingTypes.Add(CompanySettingType.SalaryExportPayrollMinimumAttestStatus);

            Dictionary<CompanySettingType, int> dict = new Dictionary<CompanySettingType, int>();
            dict.AddRange(GetCompanySettingAttestStates(entities, actorCompanyId, settingTypes.ToArray()));
            return dict;
        }

        public IEnumerable<KeyValuePair<CompanySettingType, int>> GetCompanySettingAttestStates(CompEntities entities, int actorCompanyId, params CompanySettingType[] settingTypes)
        {
            foreach (CompanySettingType settingType in settingTypes)
            {
                yield return GetCompanySettingAttestState(entities, actorCompanyId, settingType);
            }
        }

        public KeyValuePair<CompanySettingType, int> GetCompanySettingAttestState(CompEntities entities, int actorCompanyId, CompanySettingType settingType)
        {
            return new KeyValuePair<CompanySettingType, int>(settingType, GetCompanySettingAttestStateId(entities, actorCompanyId, settingType));
        }

        public AttestState GetAttestState(int attestStateId, bool loadTransitions = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetAttestState(entities, attestStateId, loadTransitions);
        }

        public AttestState GetAttestState(CompEntities entities, int attestStateId, bool loadTransitions = false)
        {
            if (attestStateId <= 0)
                return null;

            AttestState attestState = entities.AttestState.FirstOrDefault(a => a.AttestStateId == attestStateId);
            if (attestState != null && loadTransitions)
            {
                if (!attestState.AttestTransitionFrom.IsLoaded)
                    attestState.AttestTransitionFrom.Load();
                if (!attestState.AttestTransitionTo.IsLoaded)
                    attestState.AttestTransitionTo.Load();
            }

            return attestState;
        }

        public AttestState GetAttestState(TermGroup_AttestEntity entity, int actorCompanyId, SoeModule module, string name)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetAttestState(entities, actorCompanyId, entity, module, name);
        }

        public AttestState GetAttestState(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, string name)
        {
            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Name == name &&
                    a.Entity == (int)entity &&
                    a.Module == (int)module
                    select a).FirstOrDefault();
        }

        public AttestState GetAttestState(CompEntities entities, List<AttestState> attestStates, CompanySettingType settingType, bool preCondition = true)
        {
            if (attestStates.IsNullOrEmpty())
                return null;
            if (!preCondition)
                return null;

            int attestStateId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)settingType, 0, base.ActorCompanyId, 0);
            return attestStates.FirstOrDefault(i => i.AttestStateId == attestStateId);
        }

        public AttestState GetInitialAttestState(int actorCompanyId, TermGroup_AttestEntity entity)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetInitialAttestState(entities, actorCompanyId, entity);
        }

        public AttestState GetInitialAttestState(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity)
        {
            int entityId = (int)entity;

            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Initial &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault();
        }

        public int GetCompanySettingAttestStateId(CompEntities entities, int actorCompanyId, CompanySettingType settingType)
        {
            return SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)settingType, 0, actorCompanyId, 0);
        }

        public int GetInitialAttestStateId(int actorCompanyId, TermGroup_AttestEntity entity)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetInitialAttestStateId(entities, actorCompanyId, entity);
        }

        public int GetInitialAttestStateId(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity)
        {
            string key = $"GetInitialAttestStateId#{actorCompanyId}#{entity}";
            var fromCache = BusinessMemoryCache<int?>.Get(key);
            if (fromCache.HasValidValue())
                return fromCache.Value;

            int entityId = (int)entity;

            var attestStateId = (from a in entities.AttestState
                                 where a.ActorCompanyId == actorCompanyId &&
                                 a.Entity == entityId &&
                                 a.Initial &&
                                 a.State == (int)SoeEntityState.Active
                                 select a.AttestStateId).FirstOrDefault();

            if (attestStateId != 0)
                BusinessMemoryCache<int>.Set(key, attestStateId);

            return attestStateId;
        }

        public bool HasHiddenAttestState(TermGroup_AttestEntity entity, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == (int)entity &&
                    a.Hidden
                    select a).Any();
        }

        public bool HasInitialAttestState(TermGroup_AttestEntity entity, SoeModule module, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            IQueryable<AttestState> query = (from a in entities.AttestState
                                             where a.ActorCompanyId == actorCompanyId &&
                                             a.Initial
                                             select a);

            if (entity != TermGroup_AttestEntity.Unknown)
                query = query.Where(a => a.Entity == (int)entity);

            if (module != SoeModule.None)
                query = query.Where(a => a.Module == (int)module);

            return query.Any();
        }

        public bool ExistsAttestState(TermGroup_AttestEntity entity, string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return ExistsAttestState(entities, entity, name, actorCompanyId);
        }

        public bool ExistsAttestState(CompEntities entities, TermGroup_AttestEntity entity, string name, int actorCompanyId)
        {
            int entityId = (int)entity;

            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Name == name &&
                    a.State == (int)SoeEntityState.Active
                    select a).Any();
        }

        public string GetAttestStateName(CompEntities entities, int attestStateId)
        {
            return GetAttestState(entities, attestStateId)?.Name ?? string.Empty;
        }

        public AttestState GetPrevNextAttestState(int attestStateId, SoeFormMode mode)
        {
            AttestState currentState = GetAttestState(attestStateId);
            if (currentState == null)
                return null;

            AttestState attestState = null;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestState.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                attestState = (from a in entitiesReadOnly.AttestState
                               where (a.ActorCompanyId == currentState.ActorCompanyId &&
                               a.Entity == currentState.Entity &&
                               a.Sort > currentState.Sort &&
                               (a.State == (int)SoeEntityState.Active || a.State == (int)SoeEntityState.Inactive))
                               orderby a.Sort ascending
                               select a).FirstOrDefault<AttestState>();
            }
            else if (mode == SoeFormMode.Prev)
            {
                attestState = (from a in entitiesReadOnly.AttestState
                               where (a.ActorCompanyId == currentState.ActorCompanyId &&
                               a.Entity == currentState.Entity &&
                               a.Sort < currentState.Sort &&
                               (a.State == (int)SoeEntityState.Active || a.State == (int)SoeEntityState.Inactive))
                               orderby a.Sort ascending
                               select a).FirstOrDefault<AttestState>();
            }

            return attestState;
        }

        public ActionResult SaveAttestState(AttestStateDTO attestStateInput, int actorCompanyId)
        {
            if (attestStateInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestState");

            // Check initial state
            if (attestStateInput.Initial)
            {
                AttestState initial = GetInitialAttestState(actorCompanyId, attestStateInput.Entity);
                if (initial != null && initial.AttestStateId != attestStateInput.AttestStateId)
                    return new ActionResult((int)ActionResultSave.DuplicateInitialState, GetText(3329, "Endast en nivå per typ kan vara markerad som startnivå"));
            }

            // Default result is successful
            ActionResult result = new ActionResult();

            int attestStateId = attestStateInput.AttestStateId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        #region AttestState

                        AttestState attestState = GetAttestState(entities, attestStateId);
                        if (attestState == null)
                        {
                            if (ExistsAttestState(entities, attestStateInput.Entity, attestStateInput.Name, actorCompanyId))
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(3320, "Nivå finns redan"));

                            #region Add

                            attestState = new AttestState()
                            {
                                ActorCompanyId = actorCompanyId,
                                Module = (int)attestStateInput.Module,
                                Entity = (int)attestStateInput.Entity,
                                Name = attestStateInput.Name,
                                Description = attestStateInput.Description,
                                Sort = attestStateInput.Sort,
                                Initial = attestStateInput.Initial,
                                Closed = attestStateInput.Closed,
                                Hidden = attestStateInput.Hidden,
                                Locked = attestStateInput.Locked,
                                Color = StringUtility.NullToEmpty(attestStateInput.Color),
                                ImageSource = attestStateInput.ImageSource
                            };
                            SetCreatedProperties(attestState);
                            entities.AttestState.AddObject(attestState);

                            #endregion
                        }
                        else
                        {
                            if (attestState.Name != attestStateInput.Name && ExistsAttestState(entities, attestStateInput.Entity, attestStateInput.Name, actorCompanyId))
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(3320, "Nivå finns redan"));

                            #region Update

                            attestState.Entity = (int)attestStateInput.Entity;
                            attestState.Name = attestStateInput.Name;
                            attestState.Description = attestStateInput.Description;
                            attestState.Sort = attestStateInput.Sort;
                            attestState.Initial = attestStateInput.Initial;
                            attestState.Closed = attestStateInput.Closed;
                            attestState.Hidden = attestStateInput.Hidden;
                            attestState.Locked = attestStateInput.Locked;
                            attestState.Color = StringUtility.NullToEmpty(attestStateInput.Color);
                            attestState.ImageSource = attestStateInput.ImageSource;

                            SetModifiedProperties(attestState);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        attestStateId = attestState.AttestStateId;

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestStateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult AddAttestState(AttestState attestState, int actorCompanyId)
        {
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));

            // Check initial state
            if (attestState.Initial)
            {
                AttestState initial = GetInitialAttestState(actorCompanyId, (TermGroup_AttestEntity)attestState.Entity);
                if (initial != null)
                    return new ActionResult((int)ActionResultSave.DuplicateInitialState);
            }

            using (CompEntities entities = new CompEntities())
            {
                attestState.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (attestState.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, attestState, "AttestState");
            }
        }

        public ActionResult UpdateAttestState(AttestState attestState, int actorCompanyId)
        {
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));

            // Check initial state
            if (attestState.Initial)
            {
                AttestState initial = GetInitialAttestState(actorCompanyId, (TermGroup_AttestEntity)attestState.Entity);
                if (initial != null && initial.AttestStateId != attestState.AttestStateId)
                    return new ActionResult((int)ActionResultSave.DuplicateInitialState);
            }

            using (CompEntities entities = new CompEntities())
            {
                AttestState orginalAttestState = GetAttestState(entities, attestState.AttestStateId);
                if (orginalAttestState == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestState");

                return UpdateEntityItem(entities, orginalAttestState, attestState, "AttestState");
            }
        }

        public ActionResult DeleteAttestState(int attestStateId)
        {
            using (CompEntities entities = new CompEntities())
            {
                AttestState orginalAttestState = GetAttestState(entities, attestStateId, true);
                if (orginalAttestState == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestState");

                // Check relation dependencies
                if ((orginalAttestState.AttestTransitionFrom != null && orginalAttestState.AttestTransitionFrom.Any(a => a.State == (int)SoeEntityState.Active)) ||
                    (orginalAttestState.AttestTransitionTo != null && orginalAttestState.AttestTransitionTo.Any(a => a.State == (int)SoeEntityState.Active)))
                    return new ActionResult((int)ActionResultDelete.AttestStateHasTransitions, GetText(3323, "Nivå kunde inte tas bort, kontrollera att den inte används"));

                //Check for transactions
                if (TimeTransactionManager.CheckIfTransactionsExistForAttestState(attestStateId))
                    return new ActionResult((int)ActionResultDelete.AttestStateHasTransactions, GetText(3323, "Nivå kunde inte tas bort, kontrollera att den inte används"));

                return ChangeEntityState(entities, orginalAttestState, SoeEntityState.Deleted, true);
            }

        }

        public ActionResult DeleteAttestState(AttestState attestState)
        {
            if (attestState == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AttestState");

            using (CompEntities entities = new CompEntities())
            {
                AttestState orginalAttestState = GetAttestState(entities, attestState.AttestStateId, true);
                if (orginalAttestState == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestState");

                // Check relation dependencies
                if ((orginalAttestState.AttestTransitionFrom != null && orginalAttestState.AttestTransitionFrom.Any(a => a.State == (int)SoeEntityState.Active)) ||
                    (orginalAttestState.AttestTransitionTo != null && orginalAttestState.AttestTransitionTo.Any(a => a.State == (int)SoeEntityState.Active)))
                    return new ActionResult((int)ActionResultDelete.AttestStateHasTransitions);

                //Check for transactions
                if (TimeTransactionManager.CheckIfTransactionsExistForAttestState(attestState.AttestStateId))
                    return new ActionResult((int)ActionResultDelete.AttestStateHasTransactions);

                return ChangeEntityState(entities, orginalAttestState, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region AttestTransition

        public List<AttestTransition> GetAllAttestTransitions(int actorCompanyId)
        {
            CompEntities entities = new CompEntities();
            return (from a in entities.AttestTransition
                                        .Include("AttestStateFrom")
                                        .Include("AttestStateTo")
                                        .Include("AttestRole")
                    where a.ActorCompanyId == actorCompanyId &&
                    a.State == (int)SoeEntityState.Active
                    select a).ToList();
        }

        public Dictionary<int, string> GetAttestTransitionsDict(int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, bool loadAttestRole, bool setEntityName = false)
        {
            var dict = GetAttestTransitions(entity, module, loadAttestRole, actorCompanyId, setEntityName).ToDictionary(k => k.AttestTransitionId, v => v.Name);
            return dict;
        }

        public List<AttestTransition> GetAttestTransitions(CompEntities entities, List<TermGroup_AttestEntity> entitys, SoeModule module, bool loadAttestRole, int actorCompanyId, bool setEntityName = false)
        {
            List<AttestTransition> attestTransitions = new List<AttestTransition>();

            foreach (TermGroup_AttestEntity entity in entitys)
            {
                attestTransitions.AddRange(GetAttestTransitions(entities, entity, module, loadAttestRole, actorCompanyId, setEntityName));
            }

            return attestTransitions;
        }

        public List<AttestTransition> GetAttestTransitions(TermGroup_AttestEntity entity, SoeModule module, bool loadAttestRole, int actorCompanyId, bool setEntityName = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return GetAttestTransitions(entities, entity, module, loadAttestRole, actorCompanyId, setEntityName);
        }

        public List<AttestTransition> GetAttestTransitions(CompEntities entities, TermGroup_AttestEntity entity, SoeModule module, bool loadAttestRole, int actorCompanyId, bool setEntityName = false)
        {
            IQueryable<AttestTransition> query = (from a in entities.AttestTransition
                                                    .Include("AttestStateFrom")
                                                    .Include("AttestStateTo")
                                                  where a.ActorCompanyId == actorCompanyId &&
                                                  a.State == (int)SoeEntityState.Active
                                                  select a);

            if (module != SoeModule.None)
                query = query.Where(a => a.Module == (int)module);
            if (entity != TermGroup_AttestEntity.Unknown)
                query = query.Where(a => a.AttestStateFrom.Entity == (int)entity || a.AttestStateTo.Entity == (int)entity);

            // EntityName terms
            List<GenericType> terms = setEntityName ? base.GetTermGroupContent(TermGroup.AttestEntity) : null;

            List<AttestTransition> attestTransitions = query.ToList();
            foreach (AttestTransition attestTransition in attestTransitions)
            {
                if (loadAttestRole && !attestTransition.AttestRole.IsLoaded)
                    attestTransition.AttestRole.Load();

                if (setEntityName)
                {
                    // Set EntityName
                    var attestStateFrom = terms?.FirstOrDefault(t => t.Id == attestTransition.AttestStateFrom.Entity);
                    if (attestStateFrom != null)
                        attestTransition.AttestStateFrom.EntityName = attestStateFrom.Name;
                    var attestStateTo = terms?.FirstOrDefault(t => t.Id == attestTransition.AttestStateTo.Entity);
                    if (attestStateTo != null)
                        attestTransition.AttestStateTo.EntityName = attestStateTo.Name;
                }
            }

            return attestTransitions.OrderBy(a => a.AttestStateFrom.Entity).ThenByDescending(a => a.AttestStateFrom.Initial).ThenBy(a => a.AttestStateFrom.Sort).ThenBy(a => a.AttestStateTo.Sort).ThenBy(a => a.Name).ToList();
        }

        public List<AttestTransition> GetAttestTransitions(CompEntities entities, List<int> attestTransitionIds)
        {
            return (from at in entities.AttestTransition
                        .Include("AttestStateFrom")
                        .Include("AttestStateTo")
                    where attestTransitionIds.Contains(at.AttestTransitionId)
                    select at).ToList();
        }

        public List<AttestTransition> GetAttestTransitionsFromState(int attestStateId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return GetAttestTransitionsFromState(entities, attestStateId);
        }

        public List<AttestTransition> GetAttestTransitionsFromState(CompEntities entities, int attestStateId)
        {
            return (from a in entities.AttestTransition
                        .Include("AttestStateFrom")
                        .Include("AttestStateTo")
                    where a.AttestStateFromId == attestStateId &&
                    a.State == (int)SoeEntityState.Active
                    select a).OrderByDescending(a => a.AttestStateFrom.Initial).ThenBy(a => a.AttestStateFrom.Sort).ToList();
        }

        public List<AttestTransition> GetAttestTransitionsToState(CompEntities entities, int attestStateId)
        {
            return (from a in entities.AttestTransition
                        .Include("AttestStateFrom")
                        .Include("AttestStateTo")
                    where a.AttestStateToId == attestStateId &&
                    a.State == (int)SoeEntityState.Active
                    select a).OrderByDescending(a => a.AttestStateTo.Initial).ThenBy(a => a.AttestStateTo.Sort).ToList();
        }

        public List<AttestTransition> GetAttestTransitionsForAttestRoleUser(TermGroup_AttestEntity entity, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, SoeModule module = SoeModule.None)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestUserRoleView.NoTracking();
            entities.AttestTransition.NoTracking();
            return GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompanyId, module, entity, dateFrom, dateTo);
        }

        public List<AttestTransition> GetAttestTransitionsForAttestRoleUser(CompEntities entities, int userId, int actorCompanyId, SoeModule module = SoeModule.None, TermGroup_AttestEntity entity = TermGroup_AttestEntity.Unknown, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            List<AttestTransition> validAttestTransitions = new List<AttestTransition>();
            List<AttestUserRoleView> attestUserRoleViews = GetAttestUserRoleViews(entities, userId, actorCompanyId, dateFrom, dateTo, module);

            if (base.RoleId != 0)
                attestUserRoleViews = attestUserRoleViews.Where(i => !i.RoleId.HasValue || i.RoleId == base.RoleId).ToList();

            List<int> attestTransitionIds = attestUserRoleViews.Select(i => i.AttestTransitionId).ToList();
            List<AttestTransition> attestTransitions = GetAttestTransitions(entities, attestTransitionIds);

            foreach (AttestTransition attestTransition in attestTransitions)
            {
                if (entity != TermGroup_AttestEntity.Unknown && (int)entity != attestTransition.AttestStateFrom.Entity)
                    continue;

                if (!validAttestTransitions.Any(i => i.AttestTransitionId == attestTransition.AttestTransitionId))
                    validAttestTransitions.Add(attestTransition);
            }

            return validAttestTransitions;
        }

        public List<AttestTransition> GetAttestTransitionsForEmployeeGroup(TermGroup_AttestEntity entity, int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return GetAttestTransitionsForEmployeeGroup(entities, entity, employeeGroupId);
        }

        public List<AttestTransition> GetAttestTransitionsForEmployeeGroup(CompEntities entities, TermGroup_AttestEntity entity, int employeeGroupId)
        {
            var attestTranstitions = new List<AttestTransition>();

            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(entities, employeeGroupId, true, false, false, false);
            if (employeeGroup == null)
                return attestTranstitions;

            int entityId = (int)entity;

            foreach (AttestTransition attestTranstition in employeeGroup.AttestTransition.Where(t => t.State == (int)SoeEntityState.Active))
            {
                if (attestTranstitions.Any(i => i.AttestTransitionId == attestTranstition.AttestTransitionId))
                    continue;

                if (!attestTranstition.AttestStateFromReference.IsLoaded)
                    attestTranstition.AttestStateFromReference.Load();

                if (entityId != (int)TermGroup_AttestEntity.Unknown && entityId != attestTranstition.AttestStateFrom.Entity)
                    continue;

                if (!attestTranstition.AttestStateToReference.IsLoaded)
                    attestTranstition.AttestStateToReference.Load();

                attestTranstitions.Add(attestTranstition);
            }

            return attestTranstitions;
        }

        public AttestTransition GetAttestTransition(int attestTransitionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return GetAttestTransition(entities, attestTransitionId);
        }

        public AttestTransition GetAttestTransition(CompEntities entities, int attestTransitionId)
        {
            return (from a in entities.AttestTransition
                        .Include("AttestStateFrom")
                        .Include("AttestStateTo")
                    where a.AttestTransitionId == attestTransitionId
                    select a).FirstOrDefault<AttestTransition>();
        }

        public AttestTransition GetAttestTransitionWithAttestRoles(int attestTransitionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return GetAttestTransitionWithAttestRoles(entities, attestTransitionId);
        }

        public AttestTransition GetAttestTransitionWithAttestRoles(CompEntities entities, int attestTransitionId)
        {
            return (from a in entities.AttestTransition.Include("AttestRole.AttestRoleUser.User")
                    where a.AttestTransitionId == attestTransitionId
                    select a).FirstOrDefault<AttestTransition>();
        }

        public AttestTransition GetPrevNextAttestTransition(int attestTransitionId, SoeFormMode mode)
        {
            AttestTransition currentTransition = GetAttestTransition(attestTransitionId);
            if (currentTransition == null)
                return null;

            AttestTransition attestTransition = null;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestTransition.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                attestTransition = (from a in entitiesReadOnly.AttestTransition
                                        .Include("AttestStateFrom")
                                        .Include("AttestStateTo")
                                    where (a.ActorCompanyId == currentTransition.ActorCompanyId &&
                                    a.AttestStateFrom.Entity == currentTransition.AttestStateFrom.Entity &&
                                    a.Name.CompareTo(currentTransition.Name) > 0 &&
                                    (a.State == (int)SoeEntityState.Active || a.State == (int)SoeEntityState.Inactive))
                                    orderby a.Name ascending
                                    select a).FirstOrDefault<AttestTransition>();
            }
            else if (mode == SoeFormMode.Prev)
            {
                attestTransition = (from a in entitiesReadOnly.AttestTransition
                                        .Include("AttestStateFrom")
                                        .Include("AttestStateTo")
                                    where (a.ActorCompanyId == currentTransition.ActorCompanyId &&
                                    a.AttestStateFrom.Entity == currentTransition.AttestStateFrom.Entity &&
                                    a.Name.CompareTo(currentTransition.Name) < 0 &&
                                    (a.State == (int)SoeEntityState.Active || a.State == (int)SoeEntityState.Inactive))
                                    orderby a.Name ascending
                                    select a).FirstOrDefault<AttestTransition>();
            }

            return attestTransition;
        }

        public AttestTransition GetUserAttestTransitionForState(CompEntities entities, TermGroup_AttestEntity entity, int attestStateFromId, int attestStateToId, int actorCompany, int userId)
        {
            var transitions = GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompany, entity: entity);

            return (from t in transitions
                    where t.AttestStateFromId == attestStateFromId &&
                    t.AttestStateToId == attestStateToId
                    select t).FirstOrDefault();
        }

        public List<User> GetUsersByAttestTransition(int attestTransitionId)
        {
            List<User> users = new List<User>();

            AttestTransition attestTransition = GetAttestTransitionWithAttestRoles(attestTransitionId);
            foreach (AttestRole attestRole in attestTransition.AttestRole.Where(r => r.State == (int)SoeEntityState.Active))
            {
                foreach (AttestRoleUser attestRoleUser in attestRole.AttestRoleUser.Where(x => x.State == (int)SoeEntityState.Active && x.User.State == (int)SoeEntityState.Active))
                {
                    if (!users.Any(u => u.UserId == attestRoleUser.User.UserId))
                        users.Add(attestRoleUser.User);
                }
            }

            return users;
        }

        public List<User> GetUsersByAttestRoleMapping(int attestTransitionId)
        {
            List<User> users = new List<User>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            AttestTransition attestTransition = (from a in entitiesReadOnly.AttestTransition.Include("AttestRole.ChildAttestRoleMapping.ParentAttestRole.AttestRoleUser.User")
                                                 where a.AttestTransitionId == attestTransitionId
                                                 select a).FirstOrDefault();
            if (attestTransition != null)
            {
                foreach (AttestRole attestRole in attestTransition.AttestRole.Where(ar => ar.State == (int)SoeEntityState.Active))
                {
                    foreach (AttestRoleMapping mapping in attestRole.ChildAttestRoleMapping.Where(car => car.State == (int)SoeEntityState.Active))
                    {
                        foreach (AttestRoleUser attestRoleUser in mapping.ParentAttestRole.AttestRoleUser.Where(aru => aru.State == (int)SoeEntityState.Active && aru.User.State == (int)SoeEntityState.Active))
                        {
                            if (!users.Any(u => u.UserId == attestRoleUser.User.UserId))
                                users.Add(attestRoleUser.User);
                        }
                    }
                }
            }

            return users;
        }

        public int GetAttestRoleIdByAttestTransition(CompEntities entities, int attestTransitionId, int userId)
        {
            int attestRoleId = 0;

            AttestTransition attestTransition = GetAttestTransitionWithAttestRoles(entities, attestTransitionId);
            foreach (AttestRole attestRole in attestTransition.AttestRole.Where(r => r.State == (int)SoeEntityState.Active))
            {
                foreach (AttestRoleUser attestRoleUser in attestRole.AttestRoleUser.Where(u => u.State == (int)SoeEntityState.Active))
                {
                    attestRoleId = attestRole.AttestRoleId;
                    if (attestRoleUser.User.UserId == userId)
                    {
                        attestRoleId = attestRole.AttestRoleId;
                        break;
                    }
                }
            }

            return attestRoleId;
        }

        public int? GetSigningAttestRoleIdByAttestTransition(CompEntities entities, int attestTransitionId, int userId)
        {
            int? attestRoleId = null;

            AttestTransition attestTransition = (from a in entities.AttestTransition.Include("AttestRole.ChildAttestRoleMapping.ParentAttestRole.AttestRoleUser.User")
                                                 where a.AttestTransitionId == attestTransitionId
                                                 select a).FirstOrDefault();
            if (attestTransition != null)
            {
                foreach (AttestRole attestRole in attestTransition.AttestRole.Where(ar => ar.State == (int)SoeEntityState.Active))
                {
                    foreach (AttestRoleMapping mapping in attestRole.ChildAttestRoleMapping.Where(car => car.State == (int)SoeEntityState.Active))
                    {
                        foreach (AttestRoleUser attestRoleUser in mapping.ParentAttestRole.AttestRoleUser.Where(aru => aru.State == (int)SoeEntityState.Active && aru.User.State == (int)SoeEntityState.Active))
                        {
                            if (userId == attestRoleUser.User.UserId)
                            {
                                if (!attestRoleId.HasValue)
                                    attestRoleId = attestRole.AttestRoleId;
                                break;
                            }
                        }
                    }
                }
            }

            return attestRoleId;
        }

        public bool ExistsAttestTransition(TermGroup_AttestEntity entity, string name, int actorCompanyId)
        {
            int entityId = (int)entity;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestTransition.NoTracking();
            return (from a in entities.AttestTransition
                    where a.AttestStateFrom.Entity == entityId &&
                    a.Name == name &&
                    a.ActorCompanyId == actorCompanyId &&
                    a.State == (int)SoeEntityState.Active
                    select a).Any();
        }


        public ActionResult SaveAttestTransition(AttestTransitionDTO attestTransitionInput, int actorCompanyId)
        {
            if (attestTransitionInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestTransition");

            // Default result is successful
            ActionResult result = new ActionResult();

            int attestTransitionId = attestTransitionInput.AttestTransitionId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        #region AttestState

                        AttestTransition attestTransition = GetAttestTransition(entities, attestTransitionId);
                        if (attestTransition == null)
                        {
                            #region Add

                            attestTransition = new AttestTransition()
                            {
                                ActorCompanyId = actorCompanyId,
                                Module = (int)attestTransitionInput.Module,

                            };
                            SetCreatedProperties(attestTransition);
                            entities.AttestTransition.AddObject(attestTransition);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(attestTransition);

                            #endregion
                        }

                        attestTransition.Name = attestTransitionInput.Name;
                        attestTransition.AttestStateFromId = attestTransitionInput.AttestStateFromId;
                        attestTransition.AttestStateToId = attestTransitionInput.AttestStateToId;
                        attestTransition.NotifyChangeOfAttestState = attestTransitionInput.NotifyChangeOfAttestState;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        attestTransitionId = attestTransition.AttestTransitionId;

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestTransitionId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult AddAttestTransition(AttestTransition attestTransition)
        {
            if (attestTransition == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestTransition");

            using (CompEntities entities = new CompEntities())
            {
                return AddAttestTransition(entities, attestTransition);
            }
        }

        public ActionResult AddAttestTransition(CompEntities entities, AttestTransition attestTransition)
        {
            return AddEntityItem(entities, attestTransition, "AttestTransition");
        }

        public ActionResult UpdateAttestTransition(AttestTransition attestTransition)
        {
            if (attestTransition == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestTransition");

            using (CompEntities entities = new CompEntities())
            {
                AttestTransition orginalAttestTransition = GetAttestTransition(entities, attestTransition.AttestTransitionId);
                if (orginalAttestTransition == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestTransition");

                return UpdateEntityItem(entities, orginalAttestTransition, attestTransition, "AttestTransition");
            }
        }

        public ActionResult DeleteAttestTransition(int attestTransitionId)
        {
            using (CompEntities entities = new CompEntities())
            {
                AttestTransition attestTransition = GetAttestTransition(entities, attestTransitionId);
                if (attestTransition == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestTransition");

                // Check relation dependencies
                ActionResult result = IsAttestTransitionUsed(entities, attestTransition);
                if (!result.Success)
                    return result;

                return ChangeEntityState(entities, attestTransition, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteAttestTransition(AttestTransition attestTransition)
        {
            if (attestTransition == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AttestTransition");

            using (CompEntities entities = new CompEntities())
            {
                AttestTransition orginalAttestTransition = GetAttestTransition(entities, attestTransition.AttestTransitionId);
                if (orginalAttestTransition == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestTransition");

                // Check relation dependencies
                ActionResult result = IsAttestTransitionUsed(entities, attestTransition);
                if (!result.Success)
                    return result;

                return ChangeEntityState(entities, orginalAttestTransition, SoeEntityState.Deleted, true);
            }
        }

        private ActionResult IsAttestTransitionUsed(CompEntities entities, AttestTransition attestTransition)
        {
            ActionResult result = new ActionResult();

            if (entities.AttestRole.Any(r => r.AttestTransition.Any(a => a.AttestTransitionId == attestTransition.AttestTransitionId) && r.State == (int)SoeEntityState.Active))
            {
                if (attestTransition.IsSigningTransition)
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_Role, GetText(8928, "Övergång kunde inte tas bort, den används på en signeringsroll"));
                else
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_Role, GetText(3196, "Övergång kunde inte tas bort, den används på en attestroll"));
            }

            if (entities.EmployeeGroup.Any(g => g.AttestTransition.Any(a => a.AttestTransitionId == attestTransition.AttestTransitionId) && g.State == (int)SoeEntityState.Active))
                return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_EmployeeGroup, GetText(3197, "Övergång kunde inte tas bort, den används på ett tidavtal"));

            if (entities.AttestWorkFlowRow.Any(r => r.AttestTransitionId == attestTransition.AttestTransitionId && r.State == (int)SoeEntityState.Active))
            {
                if (attestTransition.IsSigningTransition)
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_Workflow, GetText(8929, "Övergång kunde inte tas bort, den används i ett signeringsflöde"));
                else
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_Workflow, GetText(3198, "Övergång kunde inte tas bort, den används i ett attestflöde"));
            }

            if (entities.AttestWorkFlowTemplateRow.Any(r => r.AttestTransitionId == attestTransition.AttestTransitionId && r.AttestWorkFlowTemplateHead.State == (int)SoeEntityState.Active))
            {
                if (attestTransition.IsSigningTransition)
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_WorkflowTemplate, GetText(8930, "Övergång kunde inte tas bort, den används på en mall"));
                else
                    return new ActionResult((int)ActionResultDelete.AttestTransitionInUse_WorkflowTemplate, GetText(3199, "Övergång kunde inte tas bort, den används på en attestflödesmall"));
            }

            return result;
        }

        public ActionResult SaveAttestRoleTransitions(Collection<FormIntervalEntryItem> formIntervalEntryItems, int actorCompanyId, int attestRoleId)
        {
            if (formIntervalEntryItems == null)
                return new ActionResult();

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();

            using (CompEntities entities = new CompEntities())
            {
                // Get attest role
                AttestRole attestRole = GetAttestRole(entities, attestRoleId, actorCompanyId, loadTransitions: true);
                if (attestRole == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                #region Track changes

                List<int> originalTransitions = attestRole.AttestTransition.Select(t => t.AttestTransitionId).ToList();
                List<int> currentTransitions = new List<int>();
                foreach (FormIntervalEntryItem item in formIntervalEntryItems)
                {
                    int id = Convert.ToInt32(item.From);
                    if (id != 0)
                        currentTransitions.Add(id);
                }

                List<int> unchangedTransitions = originalTransitions.Intersect(currentTransitions).ToList();
                // Remove unchanged (intersecting) transitions, leaving only new and deleted
                foreach (int id in unchangedTransitions)
                {
                    originalTransitions.Remove(id); // Anyone left here will be deleted (does not exist in current)
                    currentTransitions.Remove(id);  // Anyone left here will be added   (does not exist in original)
                }

                // Deleted
                foreach (int id in originalTransitions)
                {
                    AttestTransition trans = entities.AttestTransition.FirstOrDefault(t => t.AttestTransitionId == id);
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole_AttestTransition, id, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_AttestTransition, id.ToString(), null, trans?.Name));
                }

                // Added
                foreach (int id in currentTransitions)
                {
                    AttestTransition trans = entities.AttestTransition.FirstOrDefault(t => t.AttestTransitionId == id);
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, attestRole.AttestRoleId, SoeEntityType.AttestRole_AttestTransition, id, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.AttestRole_AttestTransition, null, id.ToString(), null, trans?.Name));
                }

                #endregion

                // Delete all current AttestRoleTransitions
                attestRole.AttestTransition.Clear();

                // Add new transitions, from the input collection
                foreach (FormIntervalEntryItem item in formIntervalEntryItems)
                {
                    // Empty
                    int transitionId = Convert.ToInt32(item.From);
                    if (transitionId == 0)
                        continue;

                    // Prevent duplicates
                    var record = attestRole.AttestTransition.FirstOrDefault(a => a.AttestTransitionId == transitionId);
                    if (record != null)
                        continue;

                    // Get attest transition
                    AttestTransition transition = GetAttestTransition(entities, transitionId);
                    if (transition != null && transition.AttestStateFrom.Entity == item.LabelType && transition.AttestStateTo.Entity == item.LabelType)
                        attestRole.AttestTransition.Add(transition);
                }

                ActionResult result = SaveChanges(entities);
                if (result.Success && trackChangesItems.Any())
                    result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);

                return result;
            }
        }

        public ActionResult SaveEmployeeGroupAttestTransitions(Collection<FormIntervalEntryItem> formIntervalEntryItems, int employeeGroupId)
        {
            if (formIntervalEntryItems == null)
                return new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                // Get employee group
                EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(entities, employeeGroupId, true, true, true, false);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                // Delete all current EmployeeGroupAttestTransitions
                employeeGroup.AttestTransition.Clear();

                // Add new transitions, from the input collection
                foreach (FormIntervalEntryItem item in formIntervalEntryItems)
                {
                    // Empty
                    int transitionId = Convert.ToInt32(item.From);
                    if (transitionId == 0)
                        continue;

                    // Prevent duplicates
                    if (employeeGroup.AttestTransition.Any(a => a.AttestTransitionId == transitionId))
                        continue;

                    // Get attest transition
                    AttestTransition transition = GetAttestTransition(entities, transitionId);
                    if (transition != null)
                        employeeGroup.AttestTransition.Add(transition);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region AttestTransitionLog

        public List<AttestTransitionLogDTO> GetAttestTransitionLogsForEmployee(int employeeId, DateTime date, int? timePayrollTransactionId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var attestTransitionLogItems = entitiesReadOnly.GetAttestTransitionLogsForEmployee(employeeId, CalendarUtility.GetBeginningOfDay(date), CalendarUtility.GetEndOfDay(date)).ToList();
            if (timePayrollTransactionId.HasValue && timePayrollTransactionId.Value > 0)
                attestTransitionLogItems = attestTransitionLogItems.Where(i => i.TimePayrollTransactionId == timePayrollTransactionId.Value).ToList();

            return attestTransitionLogItems.OrderBy(i => i.AttestTransitionLogDate).ToList().ToDTOs();
        }

        public List<AttestTransitionLogDTO> GetAttestTransitionLogs(int employeeId, int timeBlockDateId, int? timePayrollTransactionId = null)
        {
            TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(timeBlockDateId, employeeId);
            if (timeBlockDate == null)
                return new List<AttestTransitionLogDTO>();

            return GetAttestTransitionLogsForEmployee(employeeId, timeBlockDate.Date, timePayrollTransactionId);
        }

        public List<GetAttestTransitionLogsForEmployeeResult> GetLimitedAttestTransitionLogsForEmployee(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return GetLimitedAttestTransitionLogsForEmployees(entities, employeeId.ObjToList(), dateFrom, dateTo).FirstOrDefault().Value ?? new List<GetAttestTransitionLogsForEmployeeResult>();
        }

        public Dictionary<int, List<GetAttestTransitionLogsForEmployeeResult>> GetLimitedAttestTransitionLogsForEmployees(CompEntities entities, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            var attestTransitionLogsDict = new Dictionary<int, List<GetAttestTransitionLogsForEmployeeResult>>();
            employeeIds = employeeIds.Distinct().ToList();
            var batchHelper = BatchHelper.Create(employeeIds, 1000);

            while (batchHelper.HasMoreBatches())
            {
                var batchEmployeeIds = batchHelper.GetCurrentBatchIds();
                int actorCompanyId = base.ActorCompanyId;

                var query = (from tpt in entities.TimePayrollTransaction.AsNoTracking()
                             join atl in entities.AttestTransitionLog.AsNoTracking() on tpt.TimePayrollTransactionId equals atl.RecordId
                             join at in entities.AttestTransition.AsNoTracking() on atl.AttestTransitionId equals at.AttestTransitionId
                             where batchEmployeeIds.Contains(tpt.EmployeeId) && tpt.ActorCompanyId == actorCompanyId && atl.ActorCompanyId == actorCompanyId && atl.Date >= dateFrom && atl.Date <= dateTo && atl.Entity == (int)TermGroup_AttestEntity.PayrollTime
                             select new
                             {
                                 TimePayrollTransactionId = tpt.TimePayrollTransactionId,
                                 AttestTransitionLogId = atl.AttestTransitionLogId,
                                 AttestTransitionLogDate = atl.Date,
                                 AttestStateFromId = at.AttestStateFromId,
                                 AttestStateToId = at.AttestStateToId,
                                 AttestTransitionUserId = atl.UserId,
                                 EmployeeId = tpt.EmployeeId
                             }).ToList();

                foreach (var attestLogsPerEmployee in query.GroupBy(g => g.EmployeeId))
                {
                    var attestLogs = new List<GetAttestTransitionLogsForEmployeeResult>();
                    foreach (var attestLog in attestLogsPerEmployee)
                    {
                        var attestTransitionLog = new GetAttestTransitionLogsForEmployeeResult
                        {
                            AttestTransitionLogId = attestLog.AttestTransitionLogId,
                            AttestTransitionLogDate = attestLog.AttestTransitionLogDate,
                            AttestStateFromId = attestLog.AttestStateFromId,
                            AttestStateToId = attestLog.AttestStateToId,
                            AttestTransitionUserId = attestLog.AttestTransitionUserId
                        };
                        attestLogs.Add(attestTransitionLog);
                    }
                    attestTransitionLogsDict.Add(attestLogsPerEmployee.Key, attestLogs);
                }

                batchHelper.MoveToNextBatch();
            }

            return attestTransitionLogsDict;
        }

        #endregion

        #region AttestRule

        public List<AttestRuleHead> GetAttestRuleHeads(SoeModule module, int actorCompanyId, bool onlyActive, bool loadEmployeeGroups = false, bool loadRows = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRuleHead.NoTracking();
            return GetAttestRuleHeads(entities, module, actorCompanyId, onlyActive, loadEmployeeGroups, loadRows);
        }

        public List<AttestRuleHead> GetAttestRuleHeads(CompEntities entities, SoeModule module, int actorCompanyId, bool onlyActive, bool loadEmployeeGroups = false, bool loadRows = false)
        {
            int moduleId = (int)module;
            IQueryable<AttestRuleHead> query = (from arh in entities.AttestRuleHead
                                                .Include("DayType")
                                                where arh.ActorCompanyId == actorCompanyId &&
                                                arh.Module == moduleId
                                                select arh);

            if (loadEmployeeGroups)
                query = query.Include("EmployeeGroup");
            if (loadRows)
                query = query.Include("AttestRuleRow");

            if (onlyActive)
                query = query.Where(a => a.State == (int)SoeEntityState.Active);
            else
                query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

            return query.OrderBy(a => a.Name).ToList();
        }

        public AttestRuleHead GetAttestRuleHead(int attestRuleHeadId, bool loadEmployeeGroups = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRuleHead.NoTracking();
            return GetAttestRuleHead(entities, attestRuleHeadId, loadEmployeeGroups);
        }

        public AttestRuleHead GetAttestRuleHead(CompEntities entities, int attestRuleHeadId, bool loadEmployeeGroups = false)
        {
            AttestRuleHead rule = (from a in entities.AttestRuleHead
                                    .Include("AttestRuleRow")
                                   where a.AttestRuleHeadId == attestRuleHeadId &&
                                   a.State != (int)SoeEntityState.Deleted
                                   select a).FirstOrDefault();

            if (loadEmployeeGroups && rule != null && !rule.EmployeeGroup.IsLoaded)
                rule.EmployeeGroup.Load();

            return rule;
        }

        /// <summary>
        /// Insert or update an AttestRuleHead and its rows
        /// </summary>
        /// <param name="ruleInput">AttestRuleHead</param>
        /// <param name="rows">Collection of AttestRuleRowDTOs</param>
        /// <param name="employeeGroups">Collection of EmployeeGroups</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveAttestRuleHead(AttestRuleHeadDTO ruleInput, List<AttestRuleRowDTO> rows, int actorCompanyId)
        {
            if (ruleInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRuleHead");

            ActionResult result = null;

            int attestRuleHeadId = ruleInput.AttestRuleHeadId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Convert

                        List<AttestRuleRow> attestRuleRowsInput = rows.FromDTOs();

                        #endregion

                        #region AttestRuleHead

                        // Get existing rule
                        AttestRuleHead rule = GetAttestRuleHead(entities, ruleInput.AttestRuleHeadId, true);
                        if (rule == null)
                        {
                            #region AttestRuleHead Add

                            rule = new AttestRuleHead();
                            entities.AttestRuleHead.AddObject(rule);
                            SetCreatedProperties(rule);

                            #endregion
                        }
                        else
                        {
                            #region AttestRuleHead Update

                            SetModifiedProperties(rule);

                            #endregion
                        }

                        rule.ActorCompanyId = actorCompanyId;
                        rule.DayTypeId = ruleInput.DayTypeId;
                        rule.ScheduledJobHeadId = ruleInput.ScheduledJobHeadId.HasValue && ruleInput.ScheduledJobHeadId.Value != 0 ? ruleInput.ScheduledJobHeadId.Value : (int?)null;
                        rule.Module = (int)ruleInput.Module;
                        rule.Name = ruleInput.Name;
                        rule.Description = ruleInput.Description;
                        rule.State = (int)ruleInput.State;

                        #endregion

                        #region EmployeeGroups

                        rule.EmployeeGroup.Clear();
                        EmployeeGroup group;
                        foreach (int employeeGroupId in ruleInput.EmployeeGroupIds)
                        {
                            group = EmployeeManager.GetEmployeeGroup(entities, employeeGroupId);
                            if (group != null)
                                rule.EmployeeGroup.Add(group);
                        }

                        #endregion

                        #region AttestRuleRows

                        #region AttestRuleRow Update/Delete

                        // Update or Delete existing AttestRuleRows
                        foreach (AttestRuleRow attestRuleRow in rule.AttestRuleRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            // Try get AttestRuleRow from input
                            AttestRuleRow attestRuleRowInput = (from r in attestRuleRowsInput
                                                                where r.AttestRuleRowId == attestRuleRow.AttestRuleRowId
                                                                select r).FirstOrDefault();

                            if (attestRuleRowInput != null)
                            {
                                #region AttestRuleRow Update

                                // Update existing row
                                attestRuleRow.LeftValueType = attestRuleRowInput.LeftValueType;
                                attestRuleRow.LeftValueId = attestRuleRowInput.LeftValueId;
                                attestRuleRow.ComparisonOperator = attestRuleRowInput.ComparisonOperator;
                                attestRuleRow.RightValueType = attestRuleRowInput.RightValueType;
                                attestRuleRow.RightValueId = attestRuleRowInput.RightValueId;
                                attestRuleRow.Minutes = attestRuleRowInput.Minutes;

                                SetModifiedProperties(attestRuleRow);

                                // Detach the input row to prevent adding a new
                                base.TryDetachEntity(entities, attestRuleRowInput);

                                #endregion
                            }
                            else
                            {
                                #region AttestRuleRow Delete

                                // Delete existing row
                                ChangeEntityState(attestRuleRow, SoeEntityState.Deleted);

                                #endregion
                            }
                        }

                        #endregion

                        #region AttestRuleRow Add

                        // Get new AttestRuleRows
                        IEnumerable<AttestRuleRow> attestRuleRowsToAdd = (from r in attestRuleRowsInput
                                                                          where r.AttestRuleRowId == 0
                                                                          select r).ToList();

                        foreach (AttestRuleRow attestRuleRowToAdd in attestRuleRowsToAdd)
                        {
                            // Add AttestRuleRow to AttestRuleHead
                            rule.AttestRuleRow.Add(attestRuleRowToAdd);

                            SetCreatedProperties(attestRuleRowToAdd);
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            attestRuleHeadId = rule.AttestRuleHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result != null && result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestRuleHeadId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Delete AttestRuleHead and all its rows
        /// </summary>
        /// <param name="attestRuleHeadId">AttestRuleHead ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteAttestRuleHead(int attestRuleHeadId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get rule
                        AttestRuleHead originalRule = GetAttestRuleHead(entities, attestRuleHeadId);
                        if (originalRule == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestRuleHead");

                        #endregion

                        #region AttestRuleRow

                        // Set all rows as deleted
                        foreach (AttestRuleRow row in originalRule.AttestRuleRow)
                        {
                            result = ChangeEntityState(row, SoeEntityState.Deleted);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        #region AttestRuleHead

                        // Set rule as deleted
                        result = ChangeEntityState(originalRule, SoeEntityState.Deleted);
                        if (!result.Success)
                            return result;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult ChangeAttestRuleStates(List<AttestRuleHeadDTO> attestRuleHeads)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (AttestRuleHeadDTO attestRuleHead in attestRuleHeads)
                {
                    AttestRuleHead originalAttestRuleHead = GetAttestRuleHead(entities, attestRuleHead.AttestRuleHeadId);
                    if (originalAttestRuleHead == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestRuleHead");

                    ChangeEntityState(originalAttestRuleHead, attestRuleHead.State);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region AttestRoleUser

        public List<AttestRoleUser> GetAttestRoleUsers(CompEntities entities, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, SoeModule? module = SoeModule.Time, bool onlyWithAccountId = false, bool includeAttestRole = false, bool ignoreDates = false, bool onlyDefaultAccounts = true)
        {
            return GetAttestRoleUsers(entities, actorCompanyId, userId, includeAttestRole, module)
                .Filter(dateFrom, dateTo, onlyWithAccountId, ignoreDates, onlyDefaultAccounts);
        }

        public List<AttestRoleUser> GetAttestRoleUsers(CompEntities entities, int actorCompanyId, int userId, bool includeAttestRole, SoeModule? module = SoeModule.Time)
        {
            var query = from aru in entities.AttestRoleUser
                        where aru.UserId == userId &&
                        aru.AttestRole.ActorCompanyId == actorCompanyId &&
                        aru.AttestRole.State < (int)SoeEntityState.Deleted &&
                        aru.State == (int)SoeEntityState.Active
                        select aru;

            if (includeAttestRole)
                query = query.Include("AttestRole");
            if (module.HasValue)
                query = query.Where(t => t.AttestRole.Module == (int)module.Value);

            return query.ToList();
        }

        public List<AttestRoleUser> GetAttestRoleUsersByLicense(int userId, int licenseId, bool includeAttestRole = false, bool includeAccountAndChildren = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRoleUser.NoTracking();
            return GetAttestRoleUsersByLicense(entities, userId, licenseId, includeAttestRole, includeAccountAndChildren);
        }

        public List<AttestRoleUser> GetAttestRoleUsersByLicense(CompEntities entities, int userId, int licenseId, bool includeAttestRole = false, bool includeAccountAndChildren = false)
        {
            IQueryable<AttestRoleUser> query = (from aru in entities.AttestRoleUser
                                                where aru.UserId == userId &&
                                                aru.AttestRole.Company.LicenseId == licenseId &&
                                                aru.State < (int)SoeEntityState.Deleted
                                                select aru);

            if (includeAttestRole)
                query = query.Include("AttestRole");
            if (includeAccountAndChildren)
            {
                query = query.Include("Account");
                query = query.Include("Children.Account");
                query = query.Include("Children.Children.Account");
            }

            List<AttestRoleUser> attestRoleUsers = query.ToList();

            if (includeAttestRole)
                SetAttestRoleModuleName(attestRoleUsers);

            return attestRoleUsers;
        }

        public List<AttestRoleUser> GetAttestRoleUsersByCompany(int actorCompanyId, DateTime date, bool includeAttestRole = false, bool includeAccount = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRoleUser.NoTracking();
            return GetAttestRoleUsersByCompany(entities, actorCompanyId, date, includeAttestRole, includeAccount);
        }

        public List<AttestRoleUser> GetAttestRoleUsersByCompany(CompEntities entities, int actorCompanyId, DateTime date, bool includeAttestRole = false, bool includeAccount = false)
        {
            var query = (from aru in entities.AttestRoleUser
                         where
                         aru.AttestRole.ActorCompanyId == actorCompanyId &&
                         (!aru.DateFrom.HasValue || aru.DateFrom <= date) &&
                         (!aru.DateTo.HasValue || aru.DateTo >= date) &&
                         aru.State < (int)SoeEntityState.Deleted
                         select aru);

            if (includeAttestRole)
                query = query.Include("AttestRole");

            if (includeAccount)
            {
                query = query.Include("Account");
                query = query.Include("Children.Account");
            }

            return query.ToList();
        }

        public List<AttestRoleUser> GetAttestRoleUsersByCompany(CompEntities entities, int actorCompanyId, SoeModule? module = null)
        {
            var query = (from aru in entities.AttestRoleUser
                         where
                         aru.AttestRole.ActorCompanyId == actorCompanyId &&
                         aru.State < (int)SoeEntityState.Deleted
                         select aru);

            query = query.Include("AttestRole");
            query = query.Include("User");

            if (module.HasValue)
                query = query.Where(t => t.AttestRole.Module == (int)module.Value);

            return query.ToList();
        }

        public List<Category> GetCategoriesForAttestRoleUser(CompEntities entities, SoeCategoryType categoryType, SoeCategoryRecordEntity recordEntity, int actorCompanyId, int userId, List<AttestRoleUser> attestRoleUsers, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (attestRoleUsers.IsNullOrEmpty())
                return new List<Category>();

            if (attestRoleUsers.ShowAll())
            {
                return CategoryManager.GetCategories(entities, categoryType, actorCompanyId, loadChildren: true);
            }
            else
            {
                List<int> attestRoleIds = attestRoleUsers.Select(i => i.AttestRoleId).Distinct().ToList();
                List<CompanyCategoryRecord> attestRoleCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, categoryType, recordEntity, attestRoleIds, actorCompanyId, false, dateFrom, dateTo);
                List<Category> categories = attestRoleCategoryRecords.Where(i => i.Category != null).Select(i => i.Category).Distinct().ToList();
                return categories;
            }
        }

        public List<User> GetAttestRoleUsersForCategory(CompEntities entities, List<CompanyCategoryRecord> categoryRecordsAttestRole, List<CompanyCategoryRecord> categoryRecordsAttestRoleSecondary, int categoryId, int actorCompanyId, DateTime? startDate = null, DateTime? stopDate = null, SoeModule module = SoeModule.None, bool checkAttestRoleSecondary = false, bool onlyExecutive = false)
        {
            var attestUserRoleViewsForCompany = new List<AttestUserRoleNoTransitionsView>();
            var attestUserRoleViews = GetAttestUserRoleNoTransitionsViewsQuery(entities, actorCompanyId, startDate, stopDate, module).ToList();
            foreach (var attestUserRoleView in attestUserRoleViews)
            {
                if (attestUserRoleView.ShowAllCategories && !onlyExecutive)
                {
                    attestUserRoleViewsForCompany.Add(attestUserRoleView);
                    continue;
                }
                if (CategoryManager.HasCategoryCompanyCategoryRecords(categoryRecordsAttestRole, SoeCategoryRecordEntity.AttestRole, attestUserRoleView.AttestRoleId, categoryId, actorCompanyId, startDate, stopDate, onlyExecutive: onlyExecutive))
                {
                    attestUserRoleViewsForCompany.Add(attestUserRoleView);
                    continue;
                }
                if (checkAttestRoleSecondary && categoryRecordsAttestRoleSecondary != null && CategoryManager.HasCategoryCompanyCategoryRecords(categoryRecordsAttestRoleSecondary, SoeCategoryRecordEntity.AttestRoleSecondary, attestUserRoleView.AttestRoleId, categoryId, actorCompanyId, startDate, stopDate) && !onlyExecutive)
                {
                    attestUserRoleViewsForCompany.Add(attestUserRoleView);
                }
            }

            List<int> userIds = attestUserRoleViewsForCompany.Select(i => i.UserId).Distinct().ToList();
            List<User> users = UserManager.GetUsers(entities, userIds);

            return users;
        }

        public List<AttestRoleExtendedUserDTO> GetAttestRoleUsersForEmployeeAccounts(CompEntities entities, int actorCompanyId, List<int> accountIds, DateTime? startDate = null, DateTime? stopDate = null, bool onlyExecutive = false)
        {
            var accountString = string.Join("#", accountIds);
            string key = $"GetAttestRoleUsersForEmployeeAccounts#{accountString}#{startDate}#{stopDate}#{onlyExecutive}";
            var fromCache = BusinessMemoryCache<List<AttestRoleExtendedUserDTO>>.Get(key);
            if (fromCache != null)
                return fromCache;
            var allAccounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));
            var validAccounts = allAccounts.Where(w => accountIds.Contains(w.AccountId)).ToList();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId));
            accountDims.CalculateLevels();
            var setting = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);
            var defaultLevel = accountDims.FirstOrDefault(f => f.AccountDimId == setting)?.Level ?? int.MaxValue;
            var employeeHasChildrenAccounts = validAccounts.Any(w => accountDims.FirstOrDefault(f => f.AccountDimId == w.AccountDimId)?.Level > defaultLevel);

            List<AttestRoleExtendedUserDTO> users = new List<AttestRoleExtendedUserDTO>();
            List<AttestRoleUser> validAttestRoleUsersForAccounts = new List<AttestRoleUser>();
            List<AttestRoleUser> attestRoleUsersForAccounts = base.GetAttestRoleUsersFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(w => (!onlyExecutive || w.IsExecutive) && w.State == (int)SoeEntityState.Active && w.AccountId.HasValue && validAccounts.Select(s => s.AccountId).Contains(w.AccountId.Value)).ToList();
            RemoveInactiveEmployeesFromAttestRoleUsers(entities, actorCompanyId, attestRoleUsersForAccounts);

            if (!attestRoleUsersForAccounts.Any())
            {
                attestRoleUsersForAccounts = base.GetAttestRoleUsersFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(w => (!onlyExecutive || w.IsExecutive) && w.State == (int)SoeEntityState.Active && w.AccountId.HasValue && accountIds.Contains(w.AccountId.Value)).ToList();
                RemoveInactiveEmployeesFromAttestRoleUsers(entities, actorCompanyId, attestRoleUsersForAccounts);
            }

            if (!employeeHasChildrenAccounts && attestRoleUsersForAccounts.GroupBy(g => g.UserId).Count() != 1)
            {
                foreach (var groupByUser in attestRoleUsersForAccounts.GroupBy(g => g.UserId))
                {
                    var childrenAccounts = groupByUser.Where(w => !w.Children.IsNullOrEmpty()).SelectMany(m => m.Children).ToList();
                    //if (childrenAccounts.Any())
                    //{
                    //    var childrensChildrenAccounts = childrenAccounts.Where(w => !w.Children.IsNullOrEmpty()).SelectMany(m => m.Children).ToList();
                    //    childrenAccounts.AddRange(childrensChildrenAccounts);
                    //    if (childrensChildrenAccounts.Any())
                    //    {
                    //        var childrensChildrensChildrenAccounts = childrensChildrenAccounts.Where(w => !w.Children.IsNullOrEmpty()).SelectMany(m => m.Children);

                    //        if (childrensChildrensChildrenAccounts.Any())
                    //        {
                    //            childrenAccounts.AddRange(childrensChildrensChildrenAccounts);
                    //        }
                    //    }
                    //}

                    if (!childrenAccounts.Any())
                    {
                        validAttestRoleUsersForAccounts.AddRange(groupByUser);
                    }
                }
            }

            if (!validAttestRoleUsersForAccounts.Any())
                validAttestRoleUsersForAccounts = attestRoleUsersForAccounts;


            List<UserCompanyRole> userCompanyRoles = base.GetUserCompanyRolesForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId));
            attestRoleUsersForAccounts = validAttestRoleUsersForAccounts.Filter(startDate, stopDate).OrderBy(o => o.UserId).ToList();
            if (!attestRoleUsersForAccounts.IsNullOrEmpty())
            {
                foreach (var attestRoleUsersForAccountsByUser in attestRoleUsersForAccounts.Filter(startDate, stopDate).GroupBy(u => u.UserId))
                {
                    var userCompanyRolesOnUser = userCompanyRoles.GetByUser(attestRoleUsersForAccountsByUser.Key, startDate, stopDate);
                    users.Add(new AttestRoleExtendedUserDTO()
                    {
                        User = attestRoleUsersForAccountsByUser.First().User.ToDTO(),
                        UserAttestRoles = attestRoleUsersForAccountsByUser.ToDTOs(),
                        UserCompanyRoles = userCompanyRolesOnUser.ToDTOs()
                    });
                }
            }

            BusinessMemoryCache<List<AttestRoleExtendedUserDTO>>.Set(key, users);
            return users;
        }

        private void RemoveInactiveEmployeesFromAttestRoleUsers(CompEntities entities, int actorCompanyId, List<AttestRoleUser> attestRoleUsers)
        {
            foreach (AttestRoleUser user in attestRoleUsers.ToList())
            {
                Employee employee = EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, user.UserId, ignoreState: true);
                if (employee != null && employee.State != (int)SoeEntityState.Active)
                    attestRoleUsers.Remove(user);
            }
        }

        public List<int> GetAttestUserIds(CompEntities entities, int attestRoleId, SoeModule module)
        {
            return (from entry in entities.AttestRoleUser
                    where entry.AttestRoleId == attestRoleId &&
                    entry.AttestRole.Module == (int)module &&
                    entry.AttestRole.State < (int)SoeEntityState.Deleted &&
                    entry.State == (int)SoeEntityState.Active
                    select entry.UserId).ToList();
        }

        public List<int> GetAttestRoleUserIds(CompEntities entities, int actorCompanyId, int attestRoleId, SoeModule module = SoeModule.None)
        {
            return (from aru in entities.AttestRoleUser
                    where aru.AttestRoleId == attestRoleId &&
                    aru.AttestRole.ActorCompanyId == actorCompanyId &&
                    aru.AttestRole.State < (int)SoeEntityState.Active &&
                    (aru.AttestRole.Module == (int)module || module == SoeModule.None) &&
                    aru.State == (int)SoeEntityState.Active
                    orderby aru.AttestRole.Name
                    select aru.UserId).ToList();
        }

        public AttestRoleUser GetAttestRoleUser(int attestRoleUserId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRoleUser.NoTracking();
            return GetAttestRoleUser(entities, attestRoleUserId);
        }

        public AttestRoleUser GetAttestRoleUser(CompEntities entities, int attestRoleUserId)
        {
            return entities.AttestRoleUser.FirstOrDefault(aru => aru.AttestRoleUserId == attestRoleUserId);
        }

        public bool HasAttestRoleUsersForUser(int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAttestRoleUsers(entities, actorCompanyId, userId, dateFrom, dateTo).Any();
        }

        public bool HasAttestRoleUsersForCompany(int actorcompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestRoleUser.NoTracking();
            return (from aru in entities.AttestRoleUser
                    where aru.AttestRole.ActorCompanyId == actorcompanyId &&
                    aru.AttestRole.State < (int)SoeEntityState.Deleted &&
                    aru.State == (int)SoeEntityState.Active
                    select aru).Any();
        }

        public bool HasAttestByEmployeeAccount(int actorCompanyId, int userId, DateTime date)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestRoleUser.NoTracking();
            List<AttestRoleUser> attestRoleUsers = (from a in entitiesReadOnly.AttestRoleUser
                                                    where a.UserId == userId &&
                                                    a.AttestRole.ActorCompanyId == actorCompanyId &&
                                                    a.AttestRole.AttestByEmployeeAccount &&
                                                    a.AttestRole.State < (int)SoeEntityState.Deleted &&
                                                    a.State == (int)SoeEntityState.Active
                                                    select a).ToList();

            attestRoleUsers = attestRoleUsers.Filter(date, date);

            return attestRoleUsers.Any();
        }

        public bool HasStaffingByEmployeeAccount(int actorCompanyId, int userId, DateTime date)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestRoleUser.NoTracking();
            List<AttestRoleUser> attestRoleUsers = (from a in entitiesReadOnly.AttestRoleUser
                                                    where a.UserId == userId &&
                                                    a.AttestRole.ActorCompanyId == actorCompanyId &&
                                                    a.AttestRole.StaffingByEmployeeAccount &&
                                                    a.AttestRole.State < (int)SoeEntityState.Deleted &&
                                                    a.State == (int)SoeEntityState.Active
                                                    select a).ToList();

            attestRoleUsers = attestRoleUsers.Filter(date, date);

            return attestRoleUsers.Any();
        }

        public bool HasAllowToAddOtherEmployeeAccounts(int actorCompanyId, int userId, DateTime date)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AttestRoleUser.NoTracking();
            List<AttestRoleUser> attestRoleUsers = (from a in entitiesReadOnly.AttestRoleUser
                                                    where a.UserId == userId &&
                                                    a.AttestRole.ActorCompanyId == actorCompanyId &&
                                                    a.AttestRole.State < (int)SoeEntityState.Deleted &&
                                                    a.AttestRole.AllowToAddOtherEmployeeAccounts &&
                                                    a.State == (int)SoeEntityState.Active
                                                    select a).ToList();

            attestRoleUsers = attestRoleUsers.Filter(date, date);

            return attestRoleUsers.Any();
        }

        public string GetAttestRoleModuleName(int module)
        {
            switch ((SoeModule)module)
            {
                case SoeModule.Manage:
                    return GetText(7, "Administrera");
                case SoeModule.Economy:
                    return GetText(6, "Ekonomi");
                case SoeModule.Billing:
                    return GetText(1829, "Försäljning");
                case SoeModule.Estatus:
                    return GetText(2245, "Fastighet");
                case SoeModule.Time:
                    return GetText(5002, "Personal");
                case SoeModule.TimeSchedulePlanning:
                    return GetText(5690, "Planering");
                default:
                    return string.Empty;
            }
        }

        private void SetAttestRoleModuleName(List<AttestRoleUser> attestRoleUsers)
        {
            if (attestRoleUsers.IsNullOrEmpty())
                return;

            foreach (AttestRoleUser attestRoleUser in attestRoleUsers.Where(aru => aru.AttestRole != null))
            {
                attestRoleUser.AttestRole.ModuleName = GetAttestRoleModuleName(attestRoleUser.AttestRole.Module);
            }
        }

        public ActionResult SaveAttestRoleUsers(CompEntities entities, int actorCompanyId, int userId, params int[] attestRoleId)
        {
            var existingAttestRoleUser = GetAttestRoleUsers(entities, actorCompanyId, userId, onlyDefaultAccounts: false);
            var existingAttestRoleIds = GetAttestRoles(entities, actorCompanyId, includeInactive: true, loadAttestRoleUser: true).Select(ar => ar.AttestRoleId);

            Array.ForEach(existingAttestRoleUser.ToArray(), (aru) => entities.DeleteObject(aru));

            foreach (var item in attestRoleId)
            {
                if (!existingAttestRoleIds.Contains(item))
                    continue;

                AttestRoleUser attestRoleUser = new AttestRoleUser()
                {
                    AttestRoleId = item,
                    UserId = userId,
                };
                SetCreatedProperties(attestRoleUser);
                entities.AttestRoleUser.AddObject(attestRoleUser);
            }

            return SaveChanges(entities);
        }

        public ActionResult SaveAttestRoleUsers(CompEntities entities, List<UserRolesDTO> userRoles, User user, User currentUser, int actorCompanyId, bool onlyValidateAttestRolesInCompany = false)
        {
            List<AttestRoleUser> attestRoleUsers = new List<AttestRoleUser>();
            userRoles.ForEach(ur => attestRoleUsers.AddRange(ur.AttestRoles.Where(a => a.State == (int)SoeEntityState.Active).FromDTOs(user)));
            bool saveDelta = userRoles.Any(i => i.IsDeltaChange);
            var attestRoles = GetAttestRoles(entities, actorCompanyId, includeInactive: true, loadAttestRoleUser: true);

            if (saveDelta)
            {
                #region Delta save (API)

                DateTime dateMin = DateTime.Today.AddYears(-100);
                DateTime dateMax = DateTime.Today.AddYears(100);
                List<int> newAttestRoleIds = attestRoleUsers.Select(a => a.AttestRoleId).ToList();

                SaveChanges(entities);

                if (!user.AttestRoleUser.IsLoaded)
                    user.AttestRoleUser.Load();

                #region Delete

                foreach (AttestRoleUser attestRoleUser in user.AttestRoleUser.Where(aru => aru.State == (int)SoeEntityState.Active))
                {
                    if (onlyValidateAttestRolesInCompany && !attestRoleUser.AttestRoleReference.IsLoaded)
                        attestRoleUser.AttestRoleReference.Load();

                    if ((!onlyValidateAttestRolesInCompany || attestRoleUser.AttestRole.ActorCompanyId == actorCompanyId) && !newAttestRoleIds.Contains(attestRoleUser.AttestRoleId))
                    {
                        attestRoleUser.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(attestRoleUser);
                    }
                }

                #endregion

                #region Update or add

                foreach (UserRolesDTO userRole in userRoles)
                {
                    foreach (UserAttestRoleDTO userAttestRolesInput in userRole.AttestRoles.Where(e => e.IsModified && e.State == SoeEntityState.Active))
                    {
                        AttestRole attestRole = AttestManager.GetAttestRole(entities, userAttestRolesInput.AttestRoleId, actorCompanyId, false, false);
                        if (attestRole != null)
                        {
                            AttestRoleUser attestRoleUser = user.AttestRoleUser.GetClosestAttestRole(userAttestRolesInput, dateMin, dateMax);
                            if (attestRoleUser != null)
                            {
                                #region Update

                                bool changes = false;
                                if (userAttestRolesInput.DateFrom.HasValue && userAttestRolesInput.DateFrom.Value != attestRoleUser.DateFrom)
                                {
                                    attestRoleUser.DateFrom = userAttestRolesInput.DateFrom;
                                    changes = true;
                                }
                                if (userAttestRolesInput.DateTo != attestRoleUser.DateTo)
                                {
                                    attestRoleUser.DateTo = userAttestRolesInput.DateTo;
                                    changes = true;
                                }
                                if (!attestRole.ShowAllCategories && userAttestRolesInput.AccountId.HasValue && userAttestRolesInput.AccountId.Value != 0 && userAttestRolesInput.AccountId.Value != attestRoleUser.AccountId)
                                {
                                    attestRoleUser.AccountId = userAttestRolesInput.AccountId;
                                    changes = true;
                                }
                                else if (attestRole.ShowAllCategories && attestRoleUser.AccountId.HasValue)
                                {
                                    attestRoleUser.AccountId = null;
                                    changes = true;
                                }

                                if (userAttestRolesInput.IsExecutive != attestRoleUser.IsExecutive)
                                {
                                    attestRoleUser.IsExecutive = userAttestRolesInput.IsExecutive;
                                    changes = true;
                                }
                                if (userAttestRolesInput.IsNearestManager != attestRoleUser.IsNearestManager)
                                {
                                    attestRoleUser.IsNearestManager = userAttestRolesInput.IsNearestManager;
                                    changes = true;
                                }
                                if (userAttestRolesInput.RoleId != attestRoleUser.RoleId)
                                {
                                    attestRoleUser.RoleId = userAttestRolesInput.RoleId;
                                    changes = true;
                                }
                                if (changes)
                                    SetModifiedProperties(attestRoleUser);

                                #endregion
                            }
                            else
                            {
                                #region Add

                                attestRoleUser = new AttestRoleUser()
                                {
                                    DateFrom = userAttestRolesInput.DateFrom,
                                    DateTo = userAttestRolesInput.DateTo,
                                    IsExecutive = userAttestRolesInput.IsExecutive,
                                    IsNearestManager = userAttestRolesInput.IsNearestManager,

                                    //Set FK
                                    UserId = user.UserId,
                                    AttestRoleId = userAttestRolesInput.AttestRoleId,
                                    AccountId = !attestRole.ShowAllCategories && userAttestRolesInput.AccountId.HasValue && userAttestRolesInput.AccountId != 0 ? userAttestRolesInput.AccountId : null
                                };
                                SetCreatedProperties(attestRoleUser);
                                user.AttestRoleUser.Add(attestRoleUser);

                                #endregion
                            }
                        }
                    }
                }

                #endregion

                return SaveChanges(entities);

                #endregion
            }
            else
            {
                #region Complete save (GUI)

                return SaveAttestRoleUsers(entities, attestRoleUsers, user, currentUser);

                #endregion
            }
        }

        public ActionResult SaveAttestRoleUsers(CompEntities entities, List<AttestRoleUser> attestRoleUsersInput, User user, User currentUser)
        {
            if (attestRoleUsersInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRoleUser");

            if (!currentUser.AttestRoleUser.IsLoaded)
                currentUser.AttestRoleUser.Load();

            List<UserCompanyRole> userCompanyRoles = null;
            List<UserCompanyRole> GetUserCompanyRoles() => userCompanyRoles ?? (userCompanyRoles = UserManager.GetUserCompanyRolesByUser(entities, user.UserId));

            List<AttestRoleUser> existingAttestRoleUsers = GetAttestRoleUsersByLicense(entities, user.UserId, user.LicenseId, includeAttestRole: true, includeAccountAndChildren: true);
            foreach (AttestRoleUser parent in existingAttestRoleUsers.Where(a => !a.ParentAttestRoleUserId.HasValue).ToList())
            {
                AttestRoleUser parentInput = attestRoleUsersInput.FirstOrDefault(a => a.AttestRoleUserId == parent.AttestRoleUserId);
                if (parentInput == null && !GetUserCompanyRoles().Any(a => a.ActorCompanyId == parent.AttestRole.ActorCompanyId))
                    continue; //User has only AttesRoles on that company, but not any Role. Skip..

                bool doDeleteOrUpdate = parentInput == null || parent.IsModified(parentInput);
                if (doDeleteOrUpdate && !currentUser.HasAttestRole(this.parameterObject.RoleId, parent.AttestRoleId, parentInput?.AttestRoleId))
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91963, "Bara administratörer kan ge, uppdatera och ta bort en roll man inte själv har"));

                if (parentInput != null)
                {
                    #region Update parent

                    UpdateAttestRoleUser(parent, parentInput);
                    attestRoleUsersInput.Remove(parentInput);

                    #endregion

                    if (parent.Children != null)
                    {
                        foreach (AttestRoleUser child in parent.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
                        {
                            AttestRoleUser childInput = parentInput.Children.FirstOrDefault(a => a.AttestRoleUserId == child.AttestRoleUserId);
                            if (childInput != null)
                            {
                                #region Update child

                                UpdateAttestRoleUserChild(child, childInput);
                                parentInput.Children.Remove(childInput);

                                #endregion

                                foreach (AttestRoleUser grandChild in child.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
                                {
                                    AttestRoleUser grandChildInput = childInput.Children.FirstOrDefault(a => a.AttestRoleUserId == grandChild.AttestRoleUserId);
                                    if (grandChildInput != null)
                                    {
                                        #region Update grandchild

                                        UpdateAttestRoleUserChild(grandChild, grandChildInput);
                                        childInput.Children.Remove(grandChildInput);

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Delete grandchild

                                        DeleteAttestRoleUserGrandChild(grandChild);

                                        #endregion
                                    }
                                }

                                #region Add grandchilds

                                AddAttestRoleUsersGrandChilds(child, childInput);

                                #endregion
                            }
                            else
                            {
                                #region Delete child

                                DeleteAttestRoleUserChild(child);

                                #endregion
                            }
                        }
                    }

                    #region Add childrens

                    AddAttestRoleUserChildrens(parent, parentInput);

                    #endregion
                }
                else
                {
                    #region Delete parent and childrens

                    DeleteAttestRoleUserAndChildren(parent);

                    #endregion
                }
            }

            #region Add parents

            foreach (AttestRoleUser parentInput in attestRoleUsersInput)
            {
                if (!currentUser.HasAttestRole(this.parameterObject.RoleId, parentInput.AttestRoleId))
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91963, "Bara administratörer kan ge, uppdatera och ta bort en roll man inte själv har"));

                AttestRoleUser parent = CrateAttestRoleUserParentAndChildrens(user, parentInput);
                entities.AttestRoleUser.AddObject(parent);
            }

            #endregion

            return SaveChanges(entities);
        }

        private AttestRoleUser CrateAttestRoleUserParentAndChildrens(User user, AttestRoleUser parentInput)
        {
            if (user == null || parentInput == null)
                return null;

            AttestRoleUser parent = CreateAttestRoleUserParent(user, parentInput);
            if (parent != null && parentInput.Children != null)
            {
                foreach (AttestRoleUser childInput in parentInput.Children)
                {
                    AttestRoleUser child = CreateAttestRoleUserChild(parent, childInput);
                    if (childInput.Children != null)
                    {
                        foreach (AttestRoleUser grandChildInput in childInput.Children)
                        {
                            CreateAttestRoleUserChild(child, grandChildInput);
                        }
                    }
                }
            }
            return parent;
        }

        private AttestRoleUser CreateAttestRoleUserParent(User user, AttestRoleUser parentInput)
        {
            if (user == null || parentInput == null)
                return null;

            AttestRoleUser parent = new AttestRoleUser()
            {
                UserId = user.UserId,
                AttestRoleId = parentInput.AttestRoleId,
            };
            parent.Update(parentInput);
            SetCreatedProperties(parent);

            return parent;
        }

        private AttestRoleUser CreateAttestRoleUserChild(AttestRoleUser parent, AttestRoleUser childInput)
        {
            if (parent == null || childInput == null)
                return null;

            AttestRoleUser child = new AttestRoleUser()
            {
                UserId = parent.UserId,
                AttestRoleId = parent.AttestRoleId,
            };
            child.UpdateChild(childInput);
            SetCreatedProperties(child);

            if (parent.Children == null)
                parent.Children = new EntityCollection<AttestRoleUser>();
            parent.Children.Add(child);

            return child;
        }

        private void AddAttestRoleUserChildrens(AttestRoleUser parent, AttestRoleUser parentInput)
        {
            if (parent == null || parentInput == null)
                return;

            foreach (AttestRoleUser childInput in parentInput.Children.Where(a => !a.ParentAttestRoleUserId.HasValue || a.ParentAttestRoleUserId.Value == 0).ToList())
            {
                AddAttestRoleUserChildren(parent, childInput);
            }
        }

        private void AddAttestRoleUserChildren(AttestRoleUser parent, AttestRoleUser childInput)
        {
            if (parent == null || childInput == null)
                return;

            AttestRoleUser child = CreateAttestRoleUserChild(parent, childInput);
            if (child != null && childInput.Children != null)
            {
                foreach (AttestRoleUser grandChildInput in childInput.Children)
                {
                    CreateAttestRoleUserChild(child, grandChildInput);
                }
            }
        }

        private void AddAttestRoleUsersGrandChilds(AttestRoleUser child, AttestRoleUser childInput)
        {
            if (child == null || childInput == null)
                return;

            foreach (AttestRoleUser grandChildInput in childInput.Children)
            {
                CreateAttestRoleUserChild(child, grandChildInput);
            }
        }

        private void UpdateAttestRoleUser(AttestRoleUser parent, AttestRoleUser parentInput)
        {
            if (parent != null && parentInput != null && parent.IsModified(parentInput))
            {
                parent.Update(parentInput);
                SetModifiedProperties(parent);
            }
        }

        private void UpdateAttestRoleUserChild(AttestRoleUser child, AttestRoleUser childInput)
        {
            if (child != null && childInput != null && child.IsChildModified(childInput))
            {
                child.UpdateChild(childInput);
                SetModifiedProperties(child);
            }
        }

        private void DeleteAttestRoleUserAndChildren(AttestRoleUser parent)
        {
            if (parent == null)
                return;

            ChangeEntityState(parent, SoeEntityState.Deleted);
            if (parent.Children != null)
            {
                foreach (AttestRoleUser child in parent.Children.Where(a => a.State == (int)SoeEntityState.Active))
                {
                    DeleteAttestRoleUserChild(child);
                }
            }
        }

        private void DeleteAttestRoleUserChild(AttestRoleUser child)
        {
            if (child == null)
                return;

            ChangeEntityState(child, SoeEntityState.Deleted);
            if (child.Children != null)
            {
                foreach (AttestRoleUser grandChild in child.Children.Where(a => a.State == (int)SoeEntityState.Active))
                {
                    DeleteAttestRoleUserGrandChild(grandChild);
                }
            }
        }

        private void DeleteAttestRoleUserGrandChild(AttestRoleUser grandChild)
        {
            if (grandChild == null)
                return;

            ChangeEntityState(grandChild, SoeEntityState.Deleted);
        }

        #endregion

        #region AttestRoleUserView

        public List<AttestUserRoleView> GetAttestUserRoleViewsForDate(int userId, int? actorCompanyId, DateTime date, SoeModule module = SoeModule.None)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestUserRoleView.NoTracking();
            return GetAttestUserRoleViewsForDate(entities, userId, actorCompanyId, date, module);
        }

        public List<AttestUserRoleView> GetAttestUserRoleViewsForDate(CompEntities entities, int userId, int? actorCompanyId, DateTime date, SoeModule module = SoeModule.None)
        {
            return (from v in entities.AttestUserRoleView
                    where v.UserId == userId &&
                    (!actorCompanyId.HasValue || v.ActorCompanyId == actorCompanyId.Value) &&
                    (v.Module == (int)module || module == SoeModule.None) &&
                    v.DateFrom <= date &&
                    v.DateTo >= date
                    select v).ToList();
        }

        public List<AttestUserRoleView> GetAttestUserRoleViews(CompEntities entities, int userId, int? actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, SoeModule module = SoeModule.None)
        {
            List<AttestUserRoleView> attestUserRoleViews = (from v in entities.AttestUserRoleView
                                                            where v.UserId == userId &&
                                                            (!actorCompanyId.HasValue || v.ActorCompanyId == actorCompanyId.Value) &&
                                                            (v.Module == (int)module || module == SoeModule.None)
                                                            select v).ToList();

            if (dateFrom.HasValue || dateTo.HasValue)
                attestUserRoleViews = attestUserRoleViews.Where(i => i.IsDateValid(dateFrom, dateTo)).ToList();

            return attestUserRoleViews;
        }

        public List<AttestUserRoleView> GetAttestUserRoleViews(CompEntities entities, int userId, int actorCompanyId, int attestStateToId, DateTime? dateFrom = null, DateTime? dateTo = null, SoeModule module = SoeModule.None)
        {
            List<AttestUserRoleView> attestUserRoleViews = GetAttestUserRoleViews(entities, userId, actorCompanyId, dateFrom, dateTo, module);
            return attestUserRoleViews.Where(i => i.AttestStateToId == attestStateToId).ToList();
        }

        public bool HasValidTransition(List<AttestUserRoleView> userValidTransitions, int attestStateFromId, DateTime dateFrom, DateTime dateTo)
        {
            return (from v in userValidTransitions
                    where v.AttestStateFromId == attestStateFromId &&
                    v.DateFrom <= dateFrom &&
                    v.DateTo >= dateTo
                    select v).Any();
        }

        #region Help-methods

        private IQueryable<AttestUserRoleNoTransitionsView> GetAttestUserRoleNoTransitionsViewsQuery(CompEntities entities, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, SoeModule module = SoeModule.None)
        {
            var query = from v in entities.AttestUserRoleNoTransitionsView
                        where (v.ActorCompanyId == actorCompanyId &&
                        (v.Module == (int)module || module == SoeModule.None) &&
                        (!dateFrom.HasValue || !v.DateFrom.HasValue || v.DateFrom.Value <= dateFrom.Value) &&
                        (!dateTo.HasValue || !v.DateTo.HasValue || v.DateTo.Value >= dateTo.Value))
                        select v;

            return query;
        }

        private IQueryable<AttestUserRoleNoTransitionsView> GetAttestUserRoleNoTransitionsViewsQuery(CompEntities entities, int userId, int actorCompanyId, DateTime? date = null, SoeModule module = SoeModule.None)
        {
            var query = from v in entities.AttestUserRoleNoTransitionsView
                        where (v.ActorCompanyId == actorCompanyId &&
                        v.UserId == userId &&
                        (v.Module == (int)module || module == SoeModule.None) &&
                        (!date.HasValue || !v.DateFrom.HasValue || v.DateFrom.Value <= date.Value) &&
                        (!date.HasValue || !v.DateTo.HasValue || v.DateTo.Value >= date.Value))
                        select v;

            return query;
        }

        #endregion

        #endregion

        #region AttestStateDTO

        public List<AttestStateDTO> GetAttestStateDTOs(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module, bool addEmptyRow)
        {
            int langId = GetLangId();
            int entityId = (int)entity;
            int moduleId = (int)module;

            IQueryable<AttestState> query = (from a in entities.AttestState
                                             where a.ActorCompanyId == actorCompanyId
                                             select a
                                             );

            if (entity == TermGroup_AttestEntity.Unknown)
            {
                query = query.Where(a => a.Module == moduleId);
            }
            else
            {
                query = query.Where(a => a.Module == moduleId && a.Entity == entityId);
            }

            var attestStates = query.OrderBy(a => a.Entity).ThenByDescending(a => a.Initial).ThenBy(a => a.Sort).Select(EntityExtensions.AttestStateDTO).ToList();

            if (!attestStates.IsNullOrEmpty())
            {
                foreach (var attestState in attestStates)
                {
                    attestState.EntityName = GetText((int)attestState.Entity, TermGroup.AttestEntity, langId);
                    attestState.LangId = langId;
                }
            }

            if (addEmptyRow)
            {
                attestStates.Insert(0, new AttestStateDTO()
                {
                    AttestStateId = 0,
                    Entity = entity,
                    Name = " "
                });
            }

            return attestStates;
        }

        public AttestStateDTO GetAttestStateClosed(int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestState.NoTracking();
            return GetAttestStateClosed(entities, actorCompanyId, entity, module);
        }

        public AttestStateDTO GetAttestStateClosed(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity entity, SoeModule module)
        {
            int langId = GetLangId();
            int entityId = (int)entity;
            int moduleId = (int)module;

            var attestState = (from a in entities.AttestState
                               where a.ActorCompanyId == actorCompanyId &&
                               a.Module == moduleId &&
                               a.Entity == entityId &&
                               a.Closed
                               select a).FirstOrDefault();

            if (attestState != null)
            {
                var dto = attestState.ToDTO();
                dto.EntityName = GetText(attestState.Entity, TermGroup.AttestEntity, langId);
                dto.LangId = langId;
            }
            return attestState?.ToDTO();
        }

        #endregion

        #region Attest (Billing)

        public bool CanUserCreateInvoice(int actorCompanyId, int userId, int currentAttestStateId)
        {
            // Get state for created invoice (company setting)
            int attestStateToId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, userId, actorCompanyId, 0);

            // Get all order transitions for current user
            List<AttestTransition> transitions = GetAttestTransitionsForAttestRoleUser(TermGroup_AttestEntity.Order, actorCompanyId, userId, module: SoeModule.Billing);

            // Check if user has a valid transition from current (specified) state to the invoice state
            bool transitionExists = transitions.Any(t => t.AttestStateFromId == currentAttestStateId && t.AttestStateToId == attestStateToId);

            return transitionExists;
        }

        public List<VisibleAttestState> GetVisibleAttestStates(int attestRoleId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return GetVisibleAttestStates(entities, attestRoleId);
            }
        }

        public List<VisibleAttestState> GetVisibleAttestStates(CompEntities entities, int attestRoleId)
        {
            return (from a in entities.VisibleAttestState
                    where a.AttestRoleId == attestRoleId &&
                    a.State == (int)SoeEntityState.Active
                    select a).ToList();

        }

        public List<int> GetVisibleAttestStatesForUser(int userId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return GetVisibleAttestStatesForUser(entities, userId, actorCompanyId);
            }
        }

        public List<int> GetVisibleAttestStatesForUser(CompEntities entities, int userId, int actorCompanyId)
        {
            return (from aru in entities.AttestRoleUser
                    join va in entities.VisibleAttestState
                    on aru.AttestRoleId equals va.AttestRoleId
                    join ar in entities.AttestRole
                    on aru.AttestRoleId equals ar.AttestRoleId
                    where aru.UserId == userId && ar.ActorCompanyId == actorCompanyId
                    select va.AttestStateId).Distinct().ToList();
        }

        public bool SaveVisibleAttestStatesForOrder(int actorCompanyId, int attestRoleId, Collection<FormIntervalEntryItem> visibleAttestStates)
        {

            using (CompEntities entities = new CompEntities())
            {
                var oldVisibleAttestStates = GetVisibleAttestStates(entities, attestRoleId);
                foreach (var oldItem in oldVisibleAttestStates)
                {
                    if (visibleAttestStates.FirstOrDefault(x => int.Parse(x.From) == oldItem.AttestStateId) == null)
                    {
                        entities.DeleteObject(oldItem);
                    }
                }

                foreach (var item in visibleAttestStates)
                {

                    if ((!string.IsNullOrEmpty(item.From)) && (item.From != "0") && (oldVisibleAttestStates.FirstOrDefault(x => x.AttestStateId == int.Parse(item.From)) == null))
                    {
                        var visibleAttestState = new VisibleAttestState
                        {
                            AttestRoleId = attestRoleId,
                            AttestStateId = int.Parse(item.From)
                        };
                        this.SetCreatedProperties(visibleAttestState);
                        entities.VisibleAttestState.AddObject(visibleAttestState);
                    }

                }
                entities.SaveChanges();
            }
            return true;
        }

        #endregion

        #region AttestPayrollTransaction

        public List<AttestPayrollTransactionDTO> GetAttestPayrollTransactionDTOs(CompEntities entities, int actorCompanyId, List<TimePeriod> timePeriods, List<Employee> employees, bool applyEmploymentTaxMinimumRule = false, bool getEmployeeTimePeriodSettings = true, bool isPayrollSlip = false, bool ignoreAccounting = false, List<AccountDTO> allAccountDTOs = null, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null)
        {
            List<AttestPayrollTransactionDTO> transactions = new List<AttestPayrollTransactionDTO>();
            List<int> timePeriodIds = timePeriods.Select(t => t.TimePeriodId).ToList();
            List<int> employeeIds = employees.Select(t => t.EmployeeId).ToList();
            List<EmployeeTimePeriod> allemployeeTimePeriods = entities.EmployeeTimePeriod.Include("EmployeeTimePeriodValue").Include("EmployeeTimePeriodProductSetting").Where(w => timePeriodIds.Contains(w.TimePeriodId) && employeeIds.Contains(w.EmployeeId)).ToList();
            Dictionary<int, List<int>> validEmployeesForTimePeriods = PayrollManager.GetValidEmployeesForTimePeriod(actorCompanyId, timePeriodIds, employees, payrollGroups, true, allemployeeTimePeriods);
            List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetEmployeesTimePeriodsWithValues(entities, timePeriods.Select(t => t.TimePeriodId).ToList(), employees.Select(e => e.EmployeeId).ToList(), actorCompanyId);

            List<PayrollCalculationProductDTO> payrollCalculationProducts = new List<PayrollCalculationProductDTO>();
            foreach (TimePeriod timePeriod in timePeriods)
            {
                List<int> validEmployeeIds = validEmployeesForTimePeriods.ContainsKey(timePeriod.TimePeriodId) ? validEmployeesForTimePeriods[timePeriod.TimePeriodId] : new List<int>();
                List<Employee> validEmployees = employees.Where(e => validEmployeeIds.Contains(e.EmployeeId)).ToList();

                payrollCalculationProducts.AddRange(TimeTreePayrollManager.GetPayrollCalculationProducts(
                    entities,
                    actorCompanyId,
                    timePeriod,
                    validEmployees,
                    showAllTransactions: true,
                    applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                    ignoreAccounting: ignoreAccounting,
                    isPayrollSlip: isPayrollSlip,
                    getEmployeeTimePeriodSettings: getEmployeeTimePeriodSettings,
                    employeeTimePeriods: employeeTimePeriods,
                    allAccountDTOs: allAccountDTOs,
                    employeeGroups: employeeGroups
                ));
            }

            foreach (PayrollCalculationProductDTO payrollCalculationProduct in payrollCalculationProducts)
            {
                transactions.AddRange(payrollCalculationProduct.AttestPayrollTransactions);
            }

            return transactions;
        }

        public List<AttestPayrollTransactionDTO> GetAttestPayrollTransactionDTOs(CompEntities entities, int actorCompanyId, List<TimePeriod> timePeriods, List<Employee> employees, DateTime transactionsParamStartDate, DateTime transactionsParamStopDate, bool applyEmploymentTaxMinimumRule = false, bool getEmployeeTimePeriodSettings = true, bool isPayrollSlip = false, List<AccountDTO> allAccountDTOs = null, List<EmployeeGroup> employeeGroups = null)
        {
            List<AttestPayrollTransactionDTO> transactions = new List<AttestPayrollTransactionDTO>();
            List<PayrollCalculationProductDTO> payrollCalculationProductDTOs = new List<PayrollCalculationProductDTO>();
            List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
            List<int> timePeriodIds = timePeriods.Select(e => e.TimePeriodId).ToList();
            List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetEmployeesTimePeriodsWithValues(entities, timePeriods.Select(t => t.TimePeriodId).ToList(), employees.Select(e => e.EmployeeId).ToList(), actorCompanyId);

            //TimePayrollTransactions
            var timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployees(entities, actorCompanyId, employeeIds, timePeriodIds, transactionsParamStartDate, transactionsParamStopDate).ToList();

            //TimePayrollTransactions AccountStds
            var timePayrollTransactionAccountStds = AccountManager.GetAccountStds(entities, base.ActorCompanyId, timePayrollTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false, allAccountDTOs);

            //TimePayrollTransactions AccountInternals
            var timePayrollTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollTransactionAccountsForTimePeriodsCompany(entities, transactionsParamStartDate, transactionsParamStopDate, timePeriodIds, employeeIds, actorCompanyId);

            //TimePayrollScheduleTransactions
            var timePayrollScheduleTransactionItems = TimeTransactionManager.GetTimePayrollScheduleTransactionsForTimePeriodsCompany(entities, null, transactionsParamStartDate, transactionsParamStopDate, timePeriodIds, employeeIds, actorCompanyId).ToList();

            //TimePayrollScheduleTransactions AccountStds
            var timePayrollScheduleTransactionAccountStds = AccountManager.GetAccountStds(entities, base.ActorCompanyId, timePayrollScheduleTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false, allAccountDTOs);

            //TimePayrollScheduleTransactions AccountInternals
            var timePayrollScheduleTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollScheduleTransactionAccountsForTimePeriodsCompany(entities, null, transactionsParamStartDate, transactionsParamStopDate, actorCompanyId, timePeriodIds, employeeIds);

            foreach (var timePeriod in timePeriods)
            {
                DateTime periodTransactionsParamStartDate = !timePeriod.ExtraPeriod ? timePeriod.StartDate : CalendarUtility.DATETIME_DEFAULT;
                DateTime periodTransactionsParamStopDate = !timePeriod.ExtraPeriod ? timePeriod.StopDate : CalendarUtility.DATETIME_DEFAULT;

                var filteredTimePayrollTransactionItems = timePayrollTransactionItems.Filter(timePeriod.TimePeriodId, periodTransactionsParamStartDate, periodTransactionsParamStopDate);
                var filteredTimePayrollTransactionAccountInternalItems = timePayrollTransactionAccountInternalItems.Filter(filteredTimePayrollTransactionItems.Select(t => t.TimePayrollTransactionId).ToList(), timePeriod.TimePeriodId);
                var filteredTimePayrollScheduleTransactionItems = timePayrollScheduleTransactionItems.Filter(periodTransactionsParamStartDate, periodTransactionsParamStopDate).ToList();
                var filteredTimePayrollScheduleTransactionAccountInternalItems = timePayrollScheduleTransactionAccountInternalItems.Filter(filteredTimePayrollScheduleTransactionItems.Select(t => t.TimePayrollScheduleTransactionId).ToList(), timePeriod.TimePeriodId);

                payrollCalculationProductDTOs.AddRange(TimeTreePayrollManager.GetPayrollCalculationProducts(
                    entities,
                    actorCompanyId,
                    timePeriod,
                    employees,
                    showAllTransactions: true,
                    applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                    isPayrollSlip: isPayrollSlip,
                    getEmployeeTimePeriodSettings: getEmployeeTimePeriodSettings,
                    timePayrollTransactionItems: filteredTimePayrollTransactionItems,
                    timePayrollTransactionAccountStds: timePayrollTransactionAccountStds,
                    timePayrollTransactionAccountInternalItems: filteredTimePayrollTransactionAccountInternalItems,
                    timePayrollScheduleTransactionItems: filteredTimePayrollScheduleTransactionItems,
                    timePayrollScheduleTransactionAccountStds: timePayrollScheduleTransactionAccountStds,
                    timePayrollScheduleTransactionAccountInternalItems: filteredTimePayrollScheduleTransactionAccountInternalItems,
                    employeeTimePeriods: employeeTimePeriods,
                    allAccountDTOs: allAccountDTOs,
                    employeeGroups: employeeGroups));
            }

            foreach (var item in payrollCalculationProductDTOs)
                transactions.AddRange(item.AttestPayrollTransactions);

            return transactions;
        }

        #endregion

        #region AttestWorkFlowTemplateHead

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actorCompanyId">Company Id</param>
        /// <param name="addEmptyRow">If true, an empty row is added</param>
        /// <returns>Active AttestWorkFlowHeads for specified company </returns>
        public Dictionary<int, string> GetAttestWorkFlowTemplateHeadDict(int actorCompanyId, bool addEmptyRow, TermGroup_AttestEntity attestEntity)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<AttestWorkFlowTemplateHead> attestWorkFlowTemplateHeads = GetAttestWorkFlowTemplateHeads(actorCompanyId, attestEntity);
            foreach (AttestWorkFlowTemplateHead attestWorkFlowTemplateHead in attestWorkFlowTemplateHeads)
            {
                dict.Add(attestWorkFlowTemplateHead.AttestWorkFlowTemplateHeadId, attestWorkFlowTemplateHead.Name);
            }

            return dict;
        }

        public bool HasAttestWorkFlowTemplateHeads(int actorCompanyId, TermGroup_AttestEntity attestEntity)
        {
            bool hasTemplates;

            string key = $"hasAttestWorkFlowTemplateHeads#{actorCompanyId}#{(int)attestEntity}";
            bool? fromCache = BusinessMemoryCache<bool?>.Get(key);
            if (fromCache.HasValue)
            {
                hasTemplates = fromCache.Value;
            }
            else
            {
                hasTemplates = GetAttestWorkFlowTemplateHeads(base.ActorCompanyId, attestEntity).Any();
                BusinessMemoryCache<bool?>.Set(key, hasTemplates, 60 * 30, BusinessMemoryDistributionSetting.Disabled); // 30 min
            }

            return hasTemplates;
        }

        public List<AttestWorkFlowTemplateHead> GetAttestWorkFlowTemplateHeads(int actorCompanyId, TermGroup_AttestEntity attestEntity)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowTemplateHead.NoTracking();
            return GetAttestWorkFlowTemplateHeads(entities, actorCompanyId, attestEntity);
        }

        public List<AttestWorkFlowTemplateHead> GetAttestWorkFlowTemplateHeads(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity attestEntity)
        {
            if (attestEntity == TermGroup_AttestEntity.Unknown)
            {
                return (from a in entities.AttestWorkFlowTemplateHead
                        where a.ActorCompanyId == actorCompanyId &&
                        a.State == (int)SoeEntityState.Active
                        select a).ToList<AttestWorkFlowTemplateHead>();
            }
            else
            {
                return (from a in entities.AttestWorkFlowTemplateHead
                        where a.ActorCompanyId == actorCompanyId &&
                        a.AttestEntity == (int)attestEntity &&
                        a.State == (int)SoeEntityState.Active
                        select a).ToList<AttestWorkFlowTemplateHead>();
            }
        }

        public List<AttestWorkFlowTemplateHead> GetAttestWorkFlowTemplateHeadsIncludingRows(CompEntities entities, int actorCompanyId, TermGroup_AttestEntity attestEntity)
        {
            if (attestEntity == TermGroup_AttestEntity.Unknown)
            {
                return (from a in entities.AttestWorkFlowTemplateHead.Include("AttestWorkFlowTemplateRow")
                        where a.ActorCompanyId == actorCompanyId &&
                        a.State == (int)SoeEntityState.Active
                        select a).ToList<AttestWorkFlowTemplateHead>();
            }
            else
            {
                return (from a in entities.AttestWorkFlowTemplateHead.Include("AttestWorkFlowTemplateRow")
                        where a.ActorCompanyId == actorCompanyId &&
                        a.AttestEntity == (int)attestEntity &&
                        a.State == (int)SoeEntityState.Active
                        select a).ToList<AttestWorkFlowTemplateHead>();
            }

        }

        public AttestWorkFlowTemplateHead GetAttestWorkFlowTemplateHead(int attestWorkFlowTemplateHeadId, bool includeRows = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowTemplateHead.NoTracking();
            return GetAttestWorkFlowTemplateHead(entities, attestWorkFlowTemplateHeadId, includeRows);
        }

        public AttestWorkFlowTemplateHead GetAttestWorkFlowTemplateHead(CompEntities entities, int attestWorkFlowTemplateHeadId, bool includeRows = false)
        {
            if (includeRows)
            {
                return (from s in entities.AttestWorkFlowTemplateHead.Include("AttestWorkFlowTemplateRow")
                        where s.AttestWorkFlowTemplateHeadId == attestWorkFlowTemplateHeadId
                        select s).FirstOrDefault();
            }
            else
            {
                return (from s in entities.AttestWorkFlowTemplateHead
                        where s.AttestWorkFlowTemplateHeadId == attestWorkFlowTemplateHeadId
                        select s).FirstOrDefault();
            }
        }

        /// <summary>
        /// Insert or update an AttestWorkFlowTemplateHead
        /// </summary>
        /// <param name="attestWorkFlowTemplateHeadInput">AttestworkflowTemplateHead DTO</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveAttestWorkFlowTemplateHead(AttestWorkFlowTemplateHeadDTO attestWorkFlowTemplateHeadInput, int actorCompanyId)
        {
            if (attestWorkFlowTemplateHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestWorkFlowTemplateHead");

            ActionResult result = null;

            int attestWorkFlowTemplateId = attestWorkFlowTemplateHeadInput.AttestWorkFlowTemplateHeadId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region AttestWorkFlowTemplateHead

                        // Get existing 
                        AttestWorkFlowTemplateHead attestWorkFlowTemplateHead = GetAttestWorkFlowTemplateHead(entities, attestWorkFlowTemplateId);

                        if (attestWorkFlowTemplateHead == null)
                        {
                            #region Add

                            attestWorkFlowTemplateHead = new AttestWorkFlowTemplateHead()
                            {
                                ActorCompanyId = actorCompanyId,
                                Name = attestWorkFlowTemplateHeadInput.Name,
                                Description = attestWorkFlowTemplateHeadInput.Description,
                                State = (int)attestWorkFlowTemplateHeadInput.State,
                                Type = (int)attestWorkFlowTemplateHeadInput.Type,
                                AttestEntity = (int)attestWorkFlowTemplateHeadInput.AttestEntity,
                            };
                            SetCreatedProperties(attestWorkFlowTemplateHead);
                            entities.AttestWorkFlowTemplateHead.AddObject(attestWorkFlowTemplateHead);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            attestWorkFlowTemplateHead.Type = (int)attestWorkFlowTemplateHeadInput.Type;
                            attestWorkFlowTemplateHead.Name = attestWorkFlowTemplateHeadInput.Name;
                            attestWorkFlowTemplateHead.Description = attestWorkFlowTemplateHeadInput.Description;

                            SetModifiedProperties(attestWorkFlowTemplateHead);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;
                        else
                        {
                            //Commit transaction
                            transaction.Complete();
                            attestWorkFlowTemplateId = attestWorkFlowTemplateHead.AttestWorkFlowTemplateHeadId;

                            // Clear memory cache
                            string key = $"hasAttestWorkFlowTemplateHeads#{actorCompanyId}#{attestWorkFlowTemplateHead.AttestEntity}";
                            BusinessMemoryCache<bool?>.Delete(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result != null && result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestWorkFlowTemplateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets a attestworkflowTemplatesHead state to Deleted
        /// </summary>
        /// <param name="attestworkflowTemplatesHeadId">attestworkflowTemplatesHead to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteAttestWorkFlowTemplateHead(int attestworkflowTemplatesHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                AttestWorkFlowTemplateHead attestWorkFlowTemplateHead = GetAttestWorkFlowTemplateHead(entities, attestworkflowTemplatesHeadId);
                if (attestWorkFlowTemplateHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestWorkFlowTemplateHead");

                ActionResult result = ChangeEntityState(entities, attestWorkFlowTemplateHead, SoeEntityState.Deleted, true);
                if (result.Success)
                {
                    // Clear memory cache
                    string key = $"hasAttestWorkFlowTemplateHeads#{attestWorkFlowTemplateHead.ActorCompanyId}#{attestWorkFlowTemplateHead.AttestEntity}";
                    BusinessMemoryCache<bool?>.Delete(key);
                }

                return result;
            }
        }

        #endregion

        #region AttestWorkFlowTemplateRow

        public List<AttestWorkFlowTemplateRow> GetAttestWorkFlowTemplateRows(int attestWorkFlowTemplateHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                AttestWorkFlowTemplateHead attestWorkFlowTemplateHead = GetAttestWorkFlowTemplateHead(entities, attestWorkFlowTemplateHeadId);

                List<AttestWorkFlowTemplateRow> rows = (from a in entities.AttestWorkFlowTemplateRow
                                                            .Include("AttestTransition.AttestRole")
                                                            .Include("AttestTransition.AttestStateFrom")
                                                            .Include("AttestTransition.AttestStateTo")
                                                        where a.AttestWorkFlowTemplateHeadId == attestWorkFlowTemplateHeadId
                                                        orderby a.Sort
                                                        select a).ToList();

                foreach (AttestWorkFlowTemplateRow row in rows)
                {
                    if (row.Type == null)
                        row.Type = attestWorkFlowTemplateHead.Type;
                }

                return rows;
            }
        }

        public ActionResult SaveAttestWorkFlowTemplateRows(List<AttestWorkFlowTemplateRowDTO> attestWorkFlowRowsInput, int attestWorkFlowHeadId)
        {
            if (attestWorkFlowRowsInput.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestWorkFlowTemplateRow");

            // Default result is successful
            ActionResult result = null;

            #region Validate

            var lastRow = attestWorkFlowRowsInput.OrderBy(a => a.Sort).Last();
            var transition = GetAttestTransition(lastRow.AttestTransitionId);
            if (transition != null && !transition.AttestStateTo.Closed)
                return new ActionResult((int)ActionResultSave.AttestLastEntryMustHavePropertyClosed, GetText(4886, "Den sista attestövergången måste ske till en nivå som har statusen stängd"));

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get existing 
                        AttestWorkFlowTemplateHead attestWorkFlowTemplateHead = GetAttestWorkFlowTemplateHead(entities, attestWorkFlowHeadId);

                        #endregion

                        #region AttestWorkFlowTemplateRow

                        #region Update/Delete

                        //Get existing rows
                        List<AttestWorkFlowTemplateRow> attestWorkFlowTemplateCurrentRows = (from a in entities.AttestWorkFlowTemplateRow
                                                                                             where a.AttestWorkFlowTemplateHeadId == attestWorkFlowHeadId
                                                                                             select a).OrderBy(a => a.Sort).ToList();

                        // Update or Delete existing AttestRuleRows
                        foreach (AttestWorkFlowTemplateRow attestWorkFlowTemplateRowCurrent in attestWorkFlowTemplateCurrentRows)
                        {
                            // Try get AttestRuleRow from input
                            AttestWorkFlowTemplateRowDTO attestWorkFlowTemplateRowInput = (from r in attestWorkFlowRowsInput
                                                                                           where r.AttestWorkFlowTemplateRowId == attestWorkFlowTemplateRowCurrent.AttestWorkFlowTemplateRowId
                                                                                           select r).FirstOrDefault();

                            if (attestWorkFlowTemplateRowInput != null)
                            {
                                #region Update

                                // Update existing row
                                attestWorkFlowTemplateRowCurrent.AttestTransitionId = attestWorkFlowTemplateRowInput.AttestTransitionId;
                                attestWorkFlowTemplateRowCurrent.Sort = attestWorkFlowTemplateRowInput.Sort;
                                attestWorkFlowTemplateRowCurrent.Type = attestWorkFlowTemplateRowInput.Type == null ? attestWorkFlowTemplateHead.Type : attestWorkFlowTemplateRowInput.Type;
                                SetModifiedProperties(attestWorkFlowTemplateRowCurrent);

                                #endregion
                            }
                            else
                            {
                                #region Delete

                                // Delete existing row
                                DeleteEntityItem(entities, attestWorkFlowTemplateRowCurrent);

                                #endregion
                            }
                        }

                        #endregion

                        #region Add

                        // Get new AttestRuleRows
                        IEnumerable<AttestWorkFlowTemplateRowDTO> attestWorkFlowTemplateNewRows = (from r in attestWorkFlowRowsInput
                                                                                                   where r.AttestWorkFlowTemplateRowId == 0
                                                                                                   select r).ToList();

                        foreach (AttestWorkFlowTemplateRowDTO attestWorkFlowTemplateNewRow in attestWorkFlowTemplateNewRows)
                        {
                            attestWorkFlowTemplateHead.AttestWorkFlowTemplateRow.Add(new AttestWorkFlowTemplateRow()
                            {
                                AttestTransitionId = attestWorkFlowTemplateNewRow.AttestTransitionId,
                                AttestWorkFlowTemplateHeadId = attestWorkFlowTemplateNewRow.AttestWorkFlowTemplateHeadId,
                                Sort = attestWorkFlowTemplateNewRow.Sort,
                                Type = attestWorkFlowTemplateNewRow.Type == null ? attestWorkFlowTemplateHead.Type : attestWorkFlowTemplateNewRow.Type
                            });
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result != null && !result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region AttestWorkFlowHead

        public AttestWorkFlowHead GetAttestWorkFlowHead(int attestWorkFlowHeadId, bool loadRows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestWorkFlowHead(entities, attestWorkFlowHeadId, loadRows);
        }

        public AttestWorkFlowHead GetAttestWorkFlowHead(CompEntities entities, int attestWorkFlowHeadId, bool loadRows)
        {
            IQueryable<AttestWorkFlowHead> query = entities.AttestWorkFlowHead.Include("AttestWorkFlowTemplateHead");
            if (loadRows)
                query = query.Include("AttestWorkFlowRow");

            return (from a in query
                    where a.AttestWorkFlowHeadId == attestWorkFlowHeadId &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault();
        }

        public AttestWorkFlowHead GetAttestWorkFlowHead(SoeEntityType entity, int recordId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from a in entitiesReadOnly.AttestWorkFlowHead
                    where a.Entity == (int)entity &&
                    a.RecordId == recordId &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault();
        }

        public AttestWorkFlowHead GetAttestWorkFlowHeadFromType(CompEntities entities, int recordId, SoeEntityType type)
        {
            return (from s in entities.AttestWorkFlowHead
                    where s.Entity == (int)type &&
                    s.RecordId == recordId &&
                    s.State == (int)SoeEntityState.Active
                    select s).FirstOrDefault();
        }

        public AttestWorkFlowHead GetAttestWorkFlowHeadFromInvoiceId(int invoiceId, bool setTypeName, bool loadTemplate, bool loadRows, bool loadRemoved = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestWorkFlowHeadFromInvoiceId(entities, invoiceId, setTypeName, loadTemplate, loadRows, loadRemoved);
        }

        public List<AttestWorkFlowHead> GetAttestWorkFlowHeadFromInvoiceIds(
            List<int> invoiceIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAttestWorkFlowHeadFromInvoiceIds(entitiesReadOnly, invoiceIds);
        }

        public AttestWorkFlowHead GetAttestWorkFlowHeadFromInvoiceId(CompEntities entities, int invoiceId, bool setTypeName, bool loadTemplate, bool loadRows, bool loadRemoved = false)
        {
            IQueryable<AttestWorkFlowHead> query = entities.AttestWorkFlowHead;
            if (loadTemplate)
                query = query.Include("AttestWorkFlowTemplateHead");
            if (loadRows)
                query = query.Include("AttestWorkFlowRow");

            AttestWorkFlowHead head = null;
            if (loadRemoved)
            {
                head = (from s in query
                        where s.Entity == (int)SoeEntityType.SupplierInvoice &&
                        s.RecordId == invoiceId
                        orderby s.State, s.Modified descending
                        select s).FirstOrDefault();
            }
            else
            {
                head = (from s in query
                        where s.Entity == (int)SoeEntityType.SupplierInvoice &&
                        s.RecordId == invoiceId &&
                        s.State == (int)SoeEntityState.Active
                        select s).FirstOrDefault();
            }

            if (head != null && setTypeName)
                head.TypeName = GetText(head.Type, (int)TermGroup.AttestWorkFlowType);

            if (head != null && loadRows)
            {
                foreach (var row in head.AttestWorkFlowRow)
                {
                    if (row.Type == null)
                        row.Type = head.Type;
                }
            }

            return head;
        }


        public List<AttestWorkFlowHead> GetAttestWorkFlowHeadFromInvoiceIds(
            CompEntities entities, List<int> invoiceIds)
        {
            IQueryable<AttestWorkFlowHead> query = entities.AttestWorkFlowHead;

            List<AttestWorkFlowHead> heads = (from s in query
                                              where s.Entity == (int)SoeEntityType.SupplierInvoice &&
                                              invoiceIds.Contains(s.RecordId) &&
                                              s.State == (int)SoeEntityState.Active
                                              select s).ToList();

            return heads;
        }

        public List<AttestWorkFlowRowDTO> GetAttestWorkFlowRowsFromRecordId(SoeEntityType entity, int recordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestWorkFlowRowsFromRecordId(entities, entity, recordId);
        }

        public List<AttestWorkFlowRowDTO> GetAttestWorkFlowRowsFromRecordId(CompEntities entities, SoeEntityType entity, int recordId)
        {
            List<AttestWorkFlowRowDTO> rows = new List<AttestWorkFlowRowDTO>();
            var head = (from s in entities.AttestWorkFlowHead.Include("AttestWorkFlowRow.User")
                        where s.Entity == (int)entity &&
                        s.RecordId == recordId &&
                        s.State == (int)SoeEntityState.Active
                        select s).FirstOrDefault();

            if (head != null)
            {
                foreach (var row in head.AttestWorkFlowRow)
                {
                    if (row.Type == null)
                        row.Type = head.Type;

                    rows.Add(row.ToDTO(true));
                }
            }

            return rows;
        }

        public ActionResult SaveAttestWorkFlowForInvoices(List<int> invoiceIds, int actorCompanyId, bool sendMessage)
        {
            var result = new ActionResult(true);
            int success = 0;
            int fail = 0;
            var receivers = new List<Tuple<int, List<int>>>();

            foreach (var id in invoiceIds)
            {
                if (id == 0)
                {
                    return new ActionResult(GetText(9344));
                }
            }

            foreach (int invoiceId in invoiceIds)
            {
                SupplierInvoice invoice = SupplierInvoiceManager.GetSupplierInvoice(invoiceId, false, false, false, false, false, false, false, false);
                AttestWorkFlowHeadDTO attestWorkFlowHeadInput = GetAttestWorkFlowHead(invoice.AttestGroupId != null ? (int)invoice.AttestGroupId : 0, true).ToDTO(false, false);
                if (attestWorkFlowHeadInput != null)
                {
                    List<AttestWorkFlowRowDTO> attestWorkFlowRowsInput = GetAttestWorkFlowRows(attestWorkFlowHeadInput.AttestWorkFlowHeadId, true).ToDTOs(false).ToList();
                    if (attestWorkFlowRowsInput.Count > 0)
                    {
                        attestWorkFlowHeadInput.RecordId = invoice.InvoiceId;
                        attestWorkFlowHeadInput.Entity = SoeEntityType.SupplierInvoice;
                        result = SaveAttestWorkFlow(attestWorkFlowHeadInput, attestWorkFlowRowsInput, sendMessage, actorCompanyId, null, invoiceIds.Any());
                        if (result.Success)
                        {
                            //Update list of receivers
                            if (result.Value != null)
                            {
                                foreach (var rec in result.Value as List<MessageRecipientDTO>)
                                {
                                    var receiver = receivers.FirstOrDefault(r => r.Item1 == rec.UserId);
                                    if (receiver != null && !receiver.Item2.Contains(invoice.InvoiceId))
                                        receiver.Item2.Add(invoice.InvoiceId);
                                    else
                                        receivers.Add(new Tuple<int, List<int>>(rec.UserId, new List<int>() { invoice.InvoiceId }));
                                }
                            }
                            success++;
                        }
                        else
                            fail++;
                    }
                    else
                        fail++;
                }
                else
                    fail++;
            }

            if (sendMessage && receivers.Any())
            {
                foreach (var receiver in receivers)
                {
                    // Send message
                    var mailResult = SendAttestWorkFlowMessageForSupplierInvoice(receiver, base.ActorCompanyId, false);
                    if (!mailResult.Success)
                    {
                        base.LogError(mailResult.Exception, this.log);
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }
                }
            }

            result.IntegerValue = success;
            result.IntegerValue2 = fail;

            return result;
        }

        public ActionResult SaveAttestWorkFlowForMultipleInvoices(AttestWorkFlowHeadDTO head, List<int> invoiceIds, int actorCompanyId)
        {
            var result = new ActionResult(true);
            var success = 0;
            var fail = 0;
            var receivers = new List<Tuple<int, List<int>>>();

            var sendMessage = head.SendMessage;

            foreach (int invoiceId in invoiceIds)
            {
                SupplierInvoice invoice = SupplierInvoiceManager.GetSupplierInvoice(invoiceId, false, false, false, false, false, false, false, false);

                var rowsToSave = new List<AttestWorkFlowRowDTO>();
                foreach (var row in head.Rows)
                {
                    var clonedRow = row.CloneDTO();
                    clonedRow.AttestWorkFlowHeadId = 0;
                    clonedRow.AttestWorkFlowRowId = 0;
                    rowsToSave.Add(clonedRow);
                }
                var headToSave = head.CloneDTO();
                headToSave.RecordId = invoice.InvoiceId;
                headToSave.Entity = SoeEntityType.SupplierInvoice;
                result = SaveAttestWorkFlow(headToSave, rowsToSave, sendMessage, actorCompanyId, null, invoiceIds.Any(), true);
                if (result.Success)
                {
                    //Update list of receivers
                    if (result.Value != null)
                    {
                        foreach (var rec in result.Value as List<MessageRecipientDTO>)
                        {
                            var receiver = receivers.FirstOrDefault(r => r.Item1 == rec.UserId);
                            if (receiver != null && !receiver.Item2.Contains(invoice.InvoiceId))
                                receiver.Item2.Add(invoice.InvoiceId);
                            else
                                receivers.Add(new Tuple<int, List<int>>(rec.UserId, new List<int>() { invoice.InvoiceId }));
                        }
                    }
                    success++;
                }
                else
                    fail++;
            }

            if (sendMessage && receivers.Any())
            {
                foreach (var receiver in receivers)
                {
                    // Send message
                    var mailResult = SendAttestWorkFlowMessageForSupplierInvoice(receiver, base.ActorCompanyId, false);
                    if (!mailResult.Success)
                    {
                        base.LogError(mailResult.Exception, this.log);
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }
                }
            }

            result.IntegerValue = success;
            result.IntegerValue2 = fail;

            return result;
        }

        public ActionResult SaveAttestWorkFlow(AttestWorkFlowHeadDTO attestWorkFlowHeadInput, List<AttestWorkFlowRowDTO> attestWorkFlowRowsInput, bool sendMessage, int actorCompanyId, int? userId = null, bool returnReceivers = false, bool deleteExisting = false)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
            int attestWorkFlowHeadId = 0;

            if (attestWorkFlowHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestWorkFlowHead");
            if (attestWorkFlowRowsInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestWorkFlowRow");

            User user = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool newAttestFlow = false;
                        AttestTransition rowTransition = null;
                        List<AttestTransition> nextLevelTransitions = null;

                        #region Prereq

                        // Get User
                        if (userId.HasValue)
                            user = UserManager.GetUser(entities, userId.Value);
                        else if (parameterObject != null && parameterObject.UserId != 0)
                            user = UserManager.GetUser(entities, parameterObject.UserId);

                        #endregion

                        #region Invoice

                        if (deleteExisting)
                        {
                            var existingHead = GetAttestWorkFlowHeadFromInvoiceId(entities, attestWorkFlowHeadInput.RecordId, false, false, false);

                            if (existingHead != null)
                            {
                                // Delete existing head
                                if (existingHead is AttestWorkFlowGroup)
                                {
                                    ChangeEntityState((AttestWorkFlowGroup)existingHead, SoeEntityState.Deleted);
                                    SetModifiedProperties(existingHead);
                                }
                                else
                                {
                                    ChangeEntityState(existingHead, SoeEntityState.Deleted);
                                    SetModifiedProperties(existingHead);
                                }

                                // Save state change
                                result = SaveChanges(entities);

                                if (!result.Success)
                                    return result;
                            }
                        }

                        #endregion

                        #region AttestWorkFlowHead

                        // Get existing 
                        AttestWorkFlowHead attestWorkFlowHead = GetAttestWorkFlowHead(entities, attestWorkFlowHeadInput.AttestWorkFlowHeadId, true);

                        // If not found or if the found is a attestgroup wich has a recordid (needs to be copied)
                        if (attestWorkFlowHead == null || (attestWorkFlowHead is AttestWorkFlowGroup && attestWorkFlowHeadInput.RecordId > 0))
                        {
                            #region Add

                            if (attestWorkFlowHeadInput.IsAttestGroup && attestWorkFlowHeadInput.RecordId <= 0)
                            {
                                var agInput = (AttestGroupDTO)attestWorkFlowHeadInput;
                                if (this.ValidateAttestWorkFlowGroup(actorCompanyId, agInput.AttestGroupName, agInput.AttestGroupCode, entities: entities))
                                    return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetText(9249, "Namnet eller koden finns redan"));

                                attestWorkFlowHead = new AttestWorkFlowGroup()
                                {
                                    AttestGroupCode = agInput.AttestGroupCode,
                                    AttestGroupName = agInput.AttestGroupName,
                                };
                            }
                            else
                            {
                                attestWorkFlowHead = new AttestWorkFlowHead();
                            }

                            attestWorkFlowHead.AttestWorkFlowTemplateHeadId = attestWorkFlowHeadInput.AttestWorkFlowTemplateHeadId;
                            attestWorkFlowHead.ActorCompanyId = actorCompanyId;
                            attestWorkFlowHead.Entity = (int)attestWorkFlowHeadInput.Entity;
                            attestWorkFlowHead.Type = (int)attestWorkFlowHeadInput.Type;
                            attestWorkFlowHead.RecordId = attestWorkFlowHeadInput.RecordId;
                            attestWorkFlowHead.State = (int)attestWorkFlowHeadInput.State;
                            attestWorkFlowHead.Name = attestWorkFlowHeadInput.Name;
                            attestWorkFlowHead.SendMessage = attestWorkFlowHeadInput.SendMessage;
                            attestWorkFlowHead.AdminInformation = attestWorkFlowHeadInput.AdminInformation;

                            SetCreatedProperties(attestWorkFlowHead, user);
                            entities.AttestWorkFlowHead.AddObject(attestWorkFlowHead);

                            newAttestFlow = true;

                            #endregion
                        }
                        else
                        {
                            #region Update

                            attestWorkFlowHead.AttestWorkFlowTemplateHeadId = attestWorkFlowHeadInput.AttestWorkFlowTemplateHeadId;
                            attestWorkFlowHead.Name = attestWorkFlowHeadInput.Name;
                            attestWorkFlowHead.Type = (int)attestWorkFlowHeadInput.Type;
                            attestWorkFlowHead.SendMessage = attestWorkFlowHeadInput.SendMessage;
                            attestWorkFlowHead.AdminInformation = attestWorkFlowHeadInput.AdminInformation;

                            if (attestWorkFlowHead is AttestWorkFlowGroup && attestWorkFlowHeadInput is AttestGroupDTO)
                            {
                                AttestGroupDTO attestGroupInput = attestWorkFlowHeadInput as AttestGroupDTO;
                                if (this.ValidateAttestWorkFlowGroup(actorCompanyId, attestGroupInput.AttestGroupName, attestGroupInput.AttestGroupCode, attestWorkFlowHead.AttestWorkFlowHeadId, entities))
                                    return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetText(9249, "Namnet eller koden finns redan"));

                                AttestWorkFlowGroup attestWorkFlowGroup = attestWorkFlowHead as AttestWorkFlowGroup;
                                attestWorkFlowGroup.AttestGroupCode = attestGroupInput.AttestGroupCode;
                                attestWorkFlowGroup.AttestGroupName = attestGroupInput.AttestGroupName;
                            }

                            SetModifiedProperties(attestWorkFlowHead, user);

                            #endregion
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        attestWorkFlowHeadId = attestWorkFlowHead.AttestWorkFlowHeadId;

                        #endregion

                        #region AttestWorkFlowRow

                        //Get existing AttestFlowRows for Head
                        List<AttestWorkFlowRow> existingAttestWorkFlowRows = GetAttestWorkFlowRows(entities, attestWorkFlowHeadId, false);
                        foreach (AttestWorkFlowRow existingAttestWorkFlowRow in existingAttestWorkFlowRows)
                        {
                            //Get input AttestWorkFlowRow
                            AttestWorkFlowRowDTO attestWorkFlowRow = attestWorkFlowRowsInput.FirstOrDefault(i => i.AttestWorkFlowRowId == existingAttestWorkFlowRow.AttestWorkFlowRowId);
                            if (attestWorkFlowRow != null)
                            {
                                #region Update

                                if ((existingAttestWorkFlowRow.UserId != attestWorkFlowRow.UserId) || (existingAttestWorkFlowRow.AttestRoleId != attestWorkFlowRow.AttestRoleId))
                                {
                                    existingAttestWorkFlowRow.AttestRoleId = attestWorkFlowRow.AttestRoleId;
                                    existingAttestWorkFlowRow.UserId = attestWorkFlowRow.UserId;
                                    existingAttestWorkFlowRow.ProcessType = (int)attestWorkFlowRow.ProcessType;
                                    existingAttestWorkFlowRow.Type = attestWorkFlowRow.Type == null ? attestWorkFlowHead.Type : (int?)attestWorkFlowRow.Type;

                                    //Only set as updated if any value was changed
                                    SetModifiedProperties(existingAttestWorkFlowRow, user);
                                }

                                //Remove from input list
                                attestWorkFlowRowsInput.Remove(attestWorkFlowRow);

                                #endregion
                            }
                            else
                            {
                                #region Delete

                                entities.DeleteObject(existingAttestWorkFlowRow);

                                #endregion
                            }
                        }

                        #region Add

                        int firstTransitionId = 0;

                        //Add remaining input items
                        foreach (AttestWorkFlowRowDTO inputAttestWorkFlowRow in attestWorkFlowRowsInput)
                        {
                            // Remember first AttestTransitionId (used to send XEMail to users of first level)
                            if (firstTransitionId == 0)
                                firstTransitionId = inputAttestWorkFlowRow.AttestTransitionId;

                            #region AttestWorkFlowRow

                            int? attestRoleId = inputAttestWorkFlowRow.AttestRoleId;
                            if ((!attestRoleId.HasValue || attestRoleId == 0) && inputAttestWorkFlowRow.UserId.HasValue)
                                attestRoleId = GetAttestRoleIdByAttestTransition(entities, inputAttestWorkFlowRow.AttestTransitionId, inputAttestWorkFlowRow.UserId.Value);

                            AttestWorkFlowRow attestWorkFlowRow = new AttestWorkFlowRow()
                            {
                                AttestWorkFlowHeadId = attestWorkFlowHeadId,
                                AttestTransitionId = inputAttestWorkFlowRow.AttestTransitionId,
                                AttestRoleId = attestRoleId,
                                UserId = inputAttestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered ? user.UserId : inputAttestWorkFlowRow.UserId,
                                ProcessType = (int)inputAttestWorkFlowRow.ProcessType,
                                State = (int)TermGroup_AttestFlowRowState.Unhandled,
                                Type = inputAttestWorkFlowRow.Type == null ? attestWorkFlowHead.Type : (int?)inputAttestWorkFlowRow.Type,
                                Comment = inputAttestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && attestWorkFlowHeadInput.AdminInformation.HasValue() ? attestWorkFlowHeadInput.AdminInformation : null,
                                CommentDate = inputAttestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && attestWorkFlowHeadInput.AdminInformation.HasValue() ? attestWorkFlowHead.Created : null,
                                CommentUser = inputAttestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && attestWorkFlowHeadInput.AdminInformation.HasValue() ? user.Name : null,
                            };
                            SetCreatedProperties(attestWorkFlowRow, user);

                            // Set modified for certain process types to get the date to show up in GUI
                            if (attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Registered ||
                                attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess ||
                                attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Returned)
                                SetModifiedProperties(attestWorkFlowRow, user);
                            entities.AttestWorkFlowRow.AddObject(attestWorkFlowRow);

                            #endregion

                            #region XEMail to users in attest flow

                            //sendMessage && 
                            if (inputAttestWorkFlowRow.AttestTransitionId == firstTransitionId)
                            {
                                // Get current transition on row
                                rowTransition = GetAttestTransition(entities, inputAttestWorkFlowRow.AttestTransitionId);
                                // Get transitions from current state
                                nextLevelTransitions = GetAttestTransitionsFromState(entities, rowTransition.AttestStateToId);

                                if (inputAttestWorkFlowRow.ProcessType != TermGroup_AttestWorkFlowRowProcessType.Registered)
                                {
                                    if (inputAttestWorkFlowRow.UserId.HasValue && !inputAttestWorkFlowRow.Answer.HasValue)
                                    {
                                        // User specified
                                        User attestWorkFlowUser = UserManager.GetUser(entities, inputAttestWorkFlowRow.UserId.Value);
                                        if (attestWorkFlowUser != null)
                                            receivers.Add(new MessageRecipientDTO() { UserId = inputAttestWorkFlowRow.UserId.Value, Name = attestWorkFlowUser.Name });
                                    }
                                    else if (inputAttestWorkFlowRow.AttestRoleId.HasValue)
                                    {
                                        // Role specified, add all users from role
                                        List<int> usrIds = GetAttestRoleUserIds(entities, actorCompanyId, inputAttestWorkFlowRow.AttestRoleId.Value);
                                        foreach (int usrId in usrIds)
                                        {
                                            User attestWorkFlowUser = UserManager.GetUser(entities, usrId);
                                            if (attestWorkFlowUser != null)
                                                receivers.Add(new MessageRecipientDTO() { UserId = usrId, Name = attestWorkFlowUser.Name });
                                        }
                                    }
                                }
                            }

                            #endregion
                        }

                        if (receivers.Any())
                        {
                            var replacers = GetReplacers(entities, receivers.Select(r => r.UserId).ToList());
                            receivers.ForEach(r =>
                            {
                                if (replacers.ContainsKey(r.UserId))
                                {
                                    var replacer = replacers[r.UserId];
                                    r.UserId = replacer.UserId;
                                    r.Name = replacer.Name;
                                }
                            });
                        }
                        #endregion

                        #endregion

                        #region Supplier invoice

                        if (newAttestFlow && attestWorkFlowHeadInput.Entity != SoeEntityType.Supplier && attestWorkFlowHeadInput.RecordId > 0)
                        {
                            string currentUsers = String.Empty;

                            if (receivers.Count > 0)
                                currentUsers = String.Join(", ", receivers.Select(r => r.Name));

                            // Update attest state on invoice
                            SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, attestWorkFlowHeadInput.RecordId);
                            AttestTransition att = GetAttestTransition(entities, firstTransitionId);
                            supplierInvoice.CurrentAttestUsers = currentUsers;
                            supplierInvoice.AttestStateId = att.AttestStateFromId;
                            if (attestWorkFlowHeadInput.AttestWorkFlowGroupId.HasValue)
                                supplierInvoice.AttestGroupId = attestWorkFlowHeadInput.AttestWorkFlowGroupId.Value;
                            SetModifiedProperties(supplierInvoice, user);
                            newAttestFlow = false;
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            result.IntegerValue = attestWorkFlowHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();

                    // Send XEMail if successful
                    if (sendMessage && receivers.Any() && attestWorkFlowHeadId != 0)
                    {
                        if (returnReceivers)
                            result.Value = receivers;
                        else
                        {
                            ActionResult mailResult = SendAttestWorkFlowMessageForSupplierInvoice(receivers, actorCompanyId, attestWorkFlowHeadInput.RecordId, user.ToDTO());
                            if (!mailResult.Success)
                            {
                                base.LogError(mailResult.Exception, this.log);
                                base.LogTransactionFailed(this.ToString(), this.log);
                            }
                        }
                    }
                }

                return result;
            }
        }

        public ActionResult DeleteAttestWorkFlowHead(int attestworkflowHeadId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                #region AttestWorkFlowHead

                AttestWorkFlowHead attestWorkFlowHead = GetAttestWorkFlowHead(entities, attestworkflowHeadId, true);
                if (attestWorkFlowHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AttestWorkFlowHead");

                if (attestWorkFlowHead is AttestWorkFlowGroup attestWorkFlowGroup)
                    ChangeEntityState(attestWorkFlowGroup, SoeEntityState.Deleted);
                else
                    ChangeEntityState(attestWorkFlowHead, SoeEntityState.Deleted);

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                AttestWorkFlowHead activeAttestWorkFlowHead = GetAttestWorkFlowHeadFromInvoiceId(entities, attestWorkFlowHead.RecordId, false, false, false);

                #endregion

                if (attestWorkFlowHead.Entity != (int)SoeEntityType.Supplier && attestWorkFlowHead.RecordId > 0)
                {
                    #region SupplierInvoice

                    // Get supplier invoice with rows
                    SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, attestWorkFlowHead.RecordId, loadInvoiceRow: true, loadInvoiceAccountRow: true);
                    if (supplierInvoice == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "SupplierInvoice");

                    if (activeAttestWorkFlowHead == null)
                    {
                        // Reset attest state
                        supplierInvoice.AttestStateId = null;
                        supplierInvoice.CurrentAttestUsers = null;
                    }
                    // Delete all attest rows
                    foreach (var row in supplierInvoice.ActiveSupplierInvoiceRows.ToList())
                    {
                        if (row.SupplierInvoiceAccountRow.Any(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow))
                        {
                            foreach (var accRow in row.SupplierInvoiceAccountRow.Where(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow).ToList())
                            {
                                entities.DeleteObject(accRow);
                            }
                            entities.DeleteObject(row);
                        }
                    }



                    #endregion
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult DeleteAttestWorkFlowHead(CompEntities entities, TransactionScope transaction, SupplierInvoice supplierInvoice, AttestWorkFlowHead attestWorkFlowHead)
        {
            // Change state on head
            ChangeEntityState(attestWorkFlowHead, SoeEntityState.Deleted);

            AttestWorkFlowHead activeAttestWorkFlowHead = GetAttestWorkFlowHeadFromInvoiceId(entities, attestWorkFlowHead.RecordId, false, false, false);

            if (attestWorkFlowHead.Entity != (int)SoeEntityType.Supplier && attestWorkFlowHead.RecordId > 0)
            {
                if (activeAttestWorkFlowHead == null)
                {
                    // Reset attest state
                    supplierInvoice.AttestStateId = null;
                    supplierInvoice.CurrentAttestUsers = null;
                }

                // Delete all attest rows
                foreach (var row in supplierInvoice.ActiveSupplierInvoiceRows.ToList())
                {
                    if (row.SupplierInvoiceAccountRow.Any(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow))
                    {
                        foreach (var accRow in row.SupplierInvoiceAccountRow.Where(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow).ToList())
                        {
                            entities.DeleteObject(accRow);
                        }
                        entities.DeleteObject(row);
                    }
                }
            }

            return SaveChanges(entities, transaction);
        }

        public ActionResult DeleteAttestWorkFlowHeads(List<int> attestWorkFlowHeadIds)
        {
            ActionResult result = new ActionResult();

            foreach (int attestWorkFlowHeadId in attestWorkFlowHeadIds)
            {
                result = DeleteAttestWorkFlowHead(attestWorkFlowHeadId);
            }

            return result;
        }

        public ActionResult HideUnattestedInvoices(int actorcompanyId, List<int> invoices)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                AttestState hiddenAttestState = GetHiddenAttestStates(entities, actorcompanyId, TermGroup_AttestEntity.SupplierInvoice).FirstOrDefault();
                if (hiddenAttestState == null)
                    return new ActionResult((int)ActionResultSave.HiddenAttestStateMissing, "AttestState");

                #endregion

                #region SupplierInvoice

                foreach (int invoiceId in invoices)
                {
                    // Get supplier invoice with rows
                    SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoiceId);
                    if (supplierInvoice == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "SupplierInvoice");

                    supplierInvoice.AttestState = hiddenAttestState;
                    SetModifiedProperties(supplierInvoice);
                }

                result = SaveChanges(entities);

                #endregion
            }

            return result;
        }

        #endregion

        #region AttestWorkFlowRow

        public List<AttestWorkFlowRow> GetAttestWorkFlowRows(int attestWorkFlowHeadId, bool loadAttestRoleAndUser)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowRow.NoTracking();
            return GetAttestWorkFlowRows(entities, attestWorkFlowHeadId, loadAttestRoleAndUser);
        }

        public List<AttestWorkFlowRow> GetAttestWorkFlowRows(CompEntities entities, int attestWorkFlowHeadId, bool loadAttestRoleAndUser, bool loadUser = false)
        {
            IQueryable<AttestWorkFlowRow> query = (from a in entities.AttestWorkFlowRow.Include("AttestTransition.AttestStateFrom").Include("AttestTransition.AttestStateTo")
                                                   where a.AttestWorkFlowHeadId == attestWorkFlowHeadId
                                                   orderby a.AttestTransition.AttestStateFrom.Sort, a.AttestWorkFlowRowId
                                                   select a);

            if (loadAttestRoleAndUser)
                query = query.Include("AttestRole").Include("User");
            else if (loadUser)
                query = query.Include("User");

            return query.ToList();
        }

        public List<AttestWorkFlowRowDTO> GetAttestWorkFlowRowDTOs(int attestWorkFlowHeadId, int roleId, int userId, bool loadAttestRoleAndUser)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowRow.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AttestWorkFlowRow> rows = GetAttestWorkFlowRows(entitiesReadOnly, attestWorkFlowHeadId, loadAttestRoleAndUser);

            // Get process type terms once
            List<GenericType> processTypes = base.GetTermGroupContent(TermGroup.AttestWorkFlowRowProcessType);

            //get type terms
            List<GenericType> types = base.GetTermGroupContent(TermGroup.AttestWorkFlowType);

            // Sort rows
            var sortedRows = new List<AttestWorkFlowRow>();
            foreach (var row in rows)
            {
                if (row.OriginateFromRowId.HasValue)
                {
                    var index = sortedRows.FindIndex(r => r.AttestWorkFlowRowId == row.OriginateFromRowId);
                    sortedRows.Insert(index + 1, row);
                }
                else
                {
                    sortedRows.Add(row);
                }
            }

            // Convert to DTO
            List<AttestWorkFlowRowDTO> dtos = new List<AttestWorkFlowRowDTO>();
            foreach (AttestWorkFlowRow row in sortedRows)
            {
                DateTime now = DateTime.Now.Date;

                // Check if any replacement user exists for this period
                if (row.State == (int)TermGroup_AttestFlowRowState.Unhandled && row.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess && row.UserId.HasValue)
                {
                    entities.UserReplacement.NoTracking();
                    UserReplacement repUser = (from ur in entities.UserReplacement
                                               where ur.OriginUserId == row.UserId &&
                                               ur.StartDate <= now &&
                                               ur.StopDate >= now &&
                                               ur.Type == (int)UserReplacementType.AttestFlow &&
                                               ur.State == (int)SoeEntityState.Active
                                               select ur).FirstOrDefault();

                    if (repUser != null)
                    {
                        // Replacement found, check if it has the required role
                        //same attest transition can be added to several roles, check if roles connected to replacement user are including attest transition instead
                        List<AttestUserRoleView> attestUserRoles = GetAttestUserRoleViewsForDate(repUser.ReplacementUserId, null, DateTime.Today);
                        if (attestUserRoles.Any(a => a.AttestTransitionId == row.AttestTransitionId))
                        {
                            User repUserEnt = UserManager.GetUser(repUser.ReplacementUserId);

                            row.State = (int)TermGroup_AttestFlowRowState.Deleted;
                            row.Comment = String.Format("{0} {1}", GetText(3884, "Användaren är tillfälligt ersatt av"), repUserEnt.LoginName);
                            row.Modified = now;
                            dtos.Add(row.ToDTO(true));

                            var repDto = row.ToDTO(true);
                            repDto.UserId = repUser.ReplacementUserId;
                            repDto.LoginName = repUserEnt.LoginName;
                            repDto.Name = repUserEnt.Name;
                            repDto.IsDeleted = false;
                            repDto.WorkFlowRowIdToReplace = row.AttestWorkFlowRowId;
                            repDto.Comment = String.Format("{0} {1}", GetText(3885, "Användaren attesterar tillfälligt för"), row.User.LoginName);
                            repDto.Modified = now;
                            dtos.Add(repDto);

                            continue;
                        }
                    }
                }

                dtos.Add(row.ToDTO(true));
            }

            // Set some extensions
            foreach (var dto in dtos)
            {
                // Current user
                dto.IsCurrentUser = ((dto.UserId.HasValue && dto.UserId.Value == userId) ||
                                     (!dto.UserId.HasValue && dto.AttestRoleId == roleId));

                // No attest state name on first (Registered) row
                if (dto.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered)
                {
                    dto.AttestStateToName = String.Empty;
                    dto.IsCurrentUser = false; // Will prevent row to be bold
                }

                // ProcessTypeName
                dto.ProcessTypeName = processTypes?.FirstOrDefault(t => t.Id == (int)dto.ProcessType)?.Name ?? string.Empty;

                // TypeName
                if (dto.Type != null)
                    dto.TypeName = types.FirstOrDefault(t => t.Id == (int)dto.Type)?.Name ?? string.Empty;
                else
                    dto.TypeName = string.Empty;

                if (dto.OriginateFromRowId != 0)
                {
                    // Get origin row
                    var originRow = dtos.FirstOrDefault(r => r.AttestWorkFlowRowId == dto.OriginateFromRowId);
                    if (originRow != null)
                    {
                        dto.ProcessTypeSort = originRow.ProcessTypeSort;
                        continue;
                    }
                }

                // Sort on process type
                switch (dto.ProcessType)
                {
                    case TermGroup_AttestWorkFlowRowProcessType.Registered:
                        dto.ProcessTypeSort = 1;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess:
                        if (dto.State == TermGroup_AttestFlowRowState.Deleted)
                            dto.ProcessTypeSort = 2;
                        else
                            dto.ProcessTypeSort = dto.OriginateFromRowId.HasValue ? 7 : 8;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Processed:
                        dto.ProcessTypeSort = 6;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.LevelNotReached:
                        dto.ProcessTypeSort = 9;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredToOtherUser:
                        dto.ProcessTypeSort = 3;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn:
                        dto.ProcessTypeSort = 4;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Returned:
                        dto.ProcessTypeSort = 5;
                        break;
                }
            }

            return dtos;
        }

        public AttestWorkFlowRowDTO GetAttestWorkFlowRowToAttest(int attestWorkFlowHeadId, int roleId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowRow.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AttestWorkFlowRow> rows = GetAttestWorkFlowRows(entitiesReadOnly, attestWorkFlowHeadId, false, true);

            // Convert to DTOs
            List<AttestWorkFlowRowDTO> dtos = new List<AttestWorkFlowRowDTO>();
            foreach (AttestWorkFlowRow row in rows)
            {
                DateTime now = DateTime.Now.Date;

                // Check if any replacement user exists for this period
                if (row.State == (int)TermGroup_AttestFlowRowState.Unhandled && row.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess && row.UserId.HasValue)
                {
                    entities.UserReplacement.NoTracking();
                    UserReplacement repUser = (from ur in entities.UserReplacement
                                               where ur.OriginUserId == row.UserId &&
                                               ur.StartDate <= now &&
                                               ur.StopDate >= now &&
                                               ur.Type == (int)UserReplacementType.AttestFlow &&
                                               ur.State == (int)SoeEntityState.Active
                                               select ur).FirstOrDefault();

                    if (repUser != null)
                    {
                        // Replacement found, check if it has the required role
                        entities.AttestUserRoleView.NoTracking();
                        //same attest transition can be added to several roles, check if roles connected to replacement user are including attest transition instead
                        //if (GetAttestUserRoleViews(CompEntities, repUser.ReplacementUserId).Any(a => a.AttestRoleId == row.AttestRoleId))
                        if (GetAttestUserRoleViewsForDate(repUser.ReplacementUserId, null, DateTime.Today).Any(a => a.AttestTransitionId == row.AttestTransitionId))
                        {
                            User repUserEnt = UserManager.GetUser(repUser.ReplacementUserId);

                            row.State = (int)TermGroup_AttestFlowRowState.Deleted;
                            row.Comment = String.Format("{0} {1}", GetText(3884, "Användaren är tillfälligt ersatt av"), repUserEnt.LoginName);
                            dtos.Add(row.ToDTO(true));

                            var repDto = row.ToDTO(true);
                            repDto.UserId = repUser.ReplacementUserId;
                            repDto.IsDeleted = false;
                            repDto.WorkFlowRowIdToReplace = row.AttestWorkFlowRowId;
                            repDto.Comment = String.Format("{0} {1}", GetText(3885, "Användaren attesterar tillfälligt för"), row.User.LoginName);
                            dtos.Add(repDto);

                            continue;
                        }
                    }
                }

                dtos.Add(row.ToDTO(true));
            }

            // Set some extensions
            foreach (var dto in dtos.Where(r => r.ProcessType != TermGroup_AttestWorkFlowRowProcessType.Registered))
            {
                // Current user
                dto.IsCurrentUser = ((dto.UserId.HasValue && dto.UserId.Value == userId) || (!dto.UserId.HasValue && dto.AttestRoleId == roleId));

                if (dto.OriginateFromRowId != 0)
                {
                    // Get origin row
                    var originRow = dtos.FirstOrDefault(r => r.AttestWorkFlowRowId == dto.OriginateFromRowId);
                    if (originRow != null)
                    {
                        dto.ProcessTypeSort = originRow.ProcessTypeSort;
                        continue;
                    }
                }

                // Sort on process type
                switch (dto.ProcessType)
                {
                    case TermGroup_AttestWorkFlowRowProcessType.Registered:
                        dto.ProcessTypeSort = 1;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess:
                        if (dto.State == TermGroup_AttestFlowRowState.Deleted)
                            dto.ProcessTypeSort = 2;
                        else
                            dto.ProcessTypeSort = dto.OriginateFromRowId.HasValue ? 7 : 8;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Processed:
                        dto.ProcessTypeSort = 6;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.LevelNotReached:
                        dto.ProcessTypeSort = 9;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredToOtherUser:
                        dto.ProcessTypeSort = 3;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn:
                        dto.ProcessTypeSort = 4;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Returned:
                        dto.ProcessTypeSort = 5;
                        break;
                }
            }

            return dtos.Where(r => r.IsCurrentUser && r.ProcessType == TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess).OrderBy(r => r.AttestStateSort).ThenBy(r => r.ProcessTypeSort).ThenBy(r => r.Modified).ThenBy(r => r.Created).FirstOrDefault();
        }

        public AttestWorkFlowRow GetAttestWorkFlowRow(CompEntities entities, int attestWorkFlowRowId, bool loadUser = false, bool includeHead = false)
        {
            var workFlowRow = (from s in entities.AttestWorkFlowRow
                               where s.AttestWorkFlowRowId == attestWorkFlowRowId
                               select s).FirstOrDefault();

            if (workFlowRow != null && loadUser && !workFlowRow.UserReference.IsLoaded)
                workFlowRow.UserReference.Load();
            if (workFlowRow != null && includeHead && !workFlowRow.AttestWorkFlowHeadReference.IsLoaded)
                workFlowRow.AttestWorkFlowHeadReference.Load();

            return workFlowRow;
        }

        public ActionResult ReplaceAttestWorkFlowUser(AttestFlow_ReplaceUserReason reason, int attestWorkFlowRowId, string comment, int replacementUserId, int actorCompanyId, int invoiceId, bool sendMailToUsers, bool updateInvoiceAttesStateId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
            int attestWorkFlowHeadId = 0, attestTransitionId = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get current user
                        User currentUser = UserManager.GetUser(entities, base.UserId);

                        // Get Replacement user
                        User replacementUser = null;
                        if (replacementUserId != 0)
                            replacementUser = UserManager.GetUser(entities, replacementUserId);

                        #endregion

                        #region AttestWorkFlowRow

                        #region Existing row

                        // Get existing row
                        AttestWorkFlowRow existingRow = GetAttestWorkFlowRow(entities, attestWorkFlowRowId, true);
                        if (existingRow == null)
                            return new ActionResult((int)ActionResultSelect.EntityNotFound, "AttestWorkFlowRow");

                        // Keep any existing answer
                        if (!String.IsNullOrEmpty(existingRow.AnswerText))
                            existingRow.AnswerText += "\n";

                        // Keep any existing comment
                        if (!String.IsNullOrEmpty(existingRow.Comment))
                            existingRow.Comment += "\n";

                        if (!String.IsNullOrEmpty(comment))
                        {
                            existingRow.Comment += comment;
                            existingRow.CommentDate = DateTime.Now;
                            existingRow.CommentUser = currentUser.Name;
                        }

                        // Add reason to comment
                        switch (reason)
                        {
                            case AttestFlow_ReplaceUserReason.Remove:
                                existingRow.AnswerText += String.Format(GetText(11804, "Togs bort av {0}"), currentUser.Name) + (currentUser.Name != existingRow.User.Name ? " " + String.Format(GetText(11814, "för {0}"), existingRow.User.Name) : "");
                                if (replacementUser != null)
                                    existingRow.AnswerText += " " + String.Format(GetText(11805, "och ersattes av {0}"), replacementUser.Name);
                                existingRow.AnswerText += ".";
                                existingRow.AnswerDate = DateTime.Now;
                                break;
                            case AttestFlow_ReplaceUserReason.Transfer:
                                if (replacementUser != null)
                                    existingRow.AnswerText += String.Format(GetText(11806, "Överförd till {0}."), replacementUser.Name);
                                existingRow.AnswerDate = DateTime.Now;
                                existingRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.TransferredToOtherUser;
                                break;
                            case AttestFlow_ReplaceUserReason.TransferWithReturn:
                                if (replacementUser != null)
                                    existingRow.AnswerText += String.Format(GetText(11807, "Överförd med retur till {0}."), replacementUser.Name);
                                existingRow.AnswerDate = DateTime.Now;
                                existingRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn;
                                break;
                        }

                        #region OLD COMMENT AND ANSWERS
                        // Add user comment to comment
                        /*if (!String.IsNullOrEmpty(comment))
                        {
                            string prefix = existingRow.Comment != null && existingRow.Comment.StartsWith("*") ? string.Empty : "*";
                            existingRow.Comment = string.Concat(prefix, existingRow.Comment ?? string.Empty, String.Format(GetText(9055, "{0}: Kommentar från {1}: {2}"), timeStamp, base.LoginName, comment), "\n");
                        }

                        // Add reason to comment
                        switch (reason)
                        {
                            case AttestFlow_ReplaceUserReason.Remove:
                                existingRow.Comment += String.Format(GetText(9021, "{0}: Togs bort av {1}"), timeStamp, base.LoginName);
                                if (replacementUser != null)
                                    existingRow.Comment += " " + String.Format(GetText(9022, "och ersattes av {0}"), replacementUser.LoginName);
                                existingRow.Comment += ".";
                                break;
                            case AttestFlow_ReplaceUserReason.Transfer:
                                if (replacementUser != null)
                                    existingRow.Comment += String.Format(GetText(9047, "{0}: Överförd till {1}."), timeStamp, replacementUser.LoginName);
                                existingRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.TransferredToOtherUser;
                                break;
                            case AttestFlow_ReplaceUserReason.TransferWithReturn:
                                if (replacementUser != null)
                                    existingRow.Comment += String.Format(GetText(9048, "{0}: Överförd med retur till {1}."), timeStamp, replacementUser.LoginName);
                                existingRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn;
                                break;
                        }*/
                        #endregion

                        existingRow.State = (int)TermGroup_AttestFlowRowState.Deleted;
                        SetModifiedProperties(existingRow);

                        #endregion

                        #region Add row for replacement user

                        if (replacementUser != null)
                        {
                            AttestWorkFlowRow newRow = new AttestWorkFlowRow()
                            {
                                AttestWorkFlowHeadId = existingRow.AttestWorkFlowHeadId,
                                AttestTransitionId = existingRow.AttestTransitionId,
                                AttestRoleId = existingRow.AttestRoleId,
                                UserId = replacementUserId,
                                OriginateFromRowId = existingRow.AttestWorkFlowRowId,
                                ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess,
                                State = (int)TermGroup_AttestFlowRowState.Unhandled,
                                Type = existingRow.Type,
                            };

                            // Comments only added to the row transferring from
                            /*if (!String.IsNullOrEmpty(comment))
                            {
                                newRow.Comment = comment;
                                newRow.CommentDate = DateTime.Now;
                                newRow.CommentUser = currentUser.Name;
                            }*/

                            switch (reason)
                            {
                                case AttestFlow_ReplaceUserReason.Remove:
                                    newRow.AnswerText += String.Format(GetText(11808, "Ersätter {0}."), existingRow.User.Name);
                                    newRow.AnswerDate = DateTime.Now;
                                    break;
                                case AttestFlow_ReplaceUserReason.Transfer:
                                    newRow.AnswerText += String.Format(GetText(11809, "Överförd från {0}."), existingRow.User.Name);
                                    newRow.AnswerDate = DateTime.Now;
                                    break;
                                case AttestFlow_ReplaceUserReason.TransferWithReturn:
                                    newRow.AnswerText += String.Format(GetText(11810, "Överförd med retur från {0}."), existingRow.User.Name);
                                    newRow.AnswerDate = DateTime.Now;
                                    break;
                            }

                            SetCreatedProperties(newRow);
                            // Also set modified for the date to show up in GUI
                            SetModifiedProperties(newRow);
                            entities.AttestWorkFlowRow.AddObject(newRow);
                            receivers.Add(new MessageRecipientDTO() { UserId = replacementUser.UserId });

                        }

                        #endregion

                        #endregion

                        #region SupplierInvoice

                        if (replacementUserId == 0 && updateInvoiceAttesStateId)
                        {
                            SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoiceId);
                            List<AttestWorkFlowRow> allRows = GetAttestWorkFlowRows(entities, existingRow.AttestWorkFlowHeadId, false);

                            // If no replacement user, we might want to move to next level
                            if (allRows.OkToMoveAttestFlowToNextLevel(existingRow.AttestTransitionId))
                            {
                                if (!existingRow.AttestTransitionReference.IsLoaded)
                                    existingRow.AttestTransitionReference.Load();

                                supplierInvoice.AttestStateId = existingRow.AttestTransition.AttestStateToId;
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            #region Add row for user to return                            

                            if (reason == AttestFlow_ReplaceUserReason.TransferWithReturn)
                            {
                                AttestWorkFlowRow replacementRow = GetAttestWorkFlowRows(entities, existingRow.AttestWorkFlowHeadId, false)?.FirstOrDefault(i => i.OriginateFromRowId == existingRow.AttestWorkFlowRowId);

                                AttestWorkFlowRow newRowForReturn = new AttestWorkFlowRow()
                                {
                                    AttestWorkFlowHeadId = existingRow.AttestWorkFlowHeadId,
                                    AttestTransitionId = existingRow.AttestTransitionId,
                                    AttestRoleId = existingRow.AttestRoleId,
                                    UserId = existingRow.UserId,
                                    OriginateFromRowId = replacementRow?.AttestWorkFlowRowId,
                                    Comment = String.Empty,
                                    ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.LevelNotReached,
                                    State = (int)TermGroup_AttestFlowRowState.Unhandled,
                                    Type = existingRow.Type,
                                };
                                SetCreatedProperties(newRowForReturn);
                                // Also set modified for the date to show up in GUI
                                SetModifiedProperties(newRowForReturn);
                                entities.AttestWorkFlowRow.AddObject(newRowForReturn);
                                SaveChanges(entities, transaction);
                            }

                            #endregion

                            transaction.Complete();
                            attestTransitionId = existingRow.AttestTransitionId;
                            attestWorkFlowHeadId = existingRow.AttestWorkFlowHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        if (attestWorkFlowHeadId > 0)
                        {
                            this.UpdateCurrentAttestUsers(actorCompanyId, invoiceId, attestWorkFlowHeadId, attestTransitionId);
                        }

                        //Set success properties
                        result.IntegerValue = attestWorkFlowRowId;

                        #region Send XEMail if successful

                        if (result.IntegerValue != 0 && sendMailToUsers && receivers.Any())
                        {
                            ActionResult mailResult = SendAttestWorkFlowMessageForSupplierInvoice(receivers, actorCompanyId, invoiceId, parameterObject.SoeUser);
                            if (!mailResult.Success)
                            {
                                base.LogError(mailResult.Exception, this.log);
                                base.LogTransactionFailed(this.ToString(), this.log);
                            }
                        }

                        #endregion
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SaveAttestWorkFlowRowAnswer(int attestWorkFlowRowId, string comment, bool answer, int actorCompanyId, bool sendEmail = true, List<FileUploadDTO> attachments = null)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            ActionResult mailResult = new ActionResult(true);

            var receivers = new List<MessageRecipientDTO>();
            int invoiceId = 0;
            bool sendMessage = true;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get current user
                        User currentUser = UserManager.GetUser(entities, base.UserId);

                        // Get AttestStateId's for closed states
                        List<int> closedAttestStateIds = GetClosedAttestStatesIds(entities, actorCompanyId, TermGroup_AttestEntity.SupplierInvoice);
                        bool closedAttestState = false;

                        #endregion

                        string timeStamp = DateTime.Now.ToShortDateShortTimeString();

                        #region AttestWorkFlowRow

                        // Update row with answer
                        AttestWorkFlowRow attestWorkFlowRow = GetAttestWorkFlowRow(entities, attestWorkFlowRowId, true, true);
                        if (attestWorkFlowRow == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestWorkFlowRow");

                        sendMessage = attestWorkFlowRow?.AttestWorkFlowHead.SendMessage.GetValueOrDefault(true) ?? true;

                        //Keep any existing comment
                        if (!string.IsNullOrEmpty(attestWorkFlowRow.Comment))
                            attestWorkFlowRow.Comment += "\n";

                        // Keep any existing comment
                        if (!string.IsNullOrEmpty(attestWorkFlowRow.AnswerText))
                            attestWorkFlowRow.AnswerText += "\n";

                        // Add user comment to comment
                        if (!string.IsNullOrEmpty(comment))
                        {
                            attestWorkFlowRow.Comment = comment;
                            attestWorkFlowRow.CommentDate = DateTime.Now;
                            attestWorkFlowRow.CommentUser = currentUser.Name;
                        }

                        // Add answer type to comment
                        if (answer)
                            attestWorkFlowRow.AnswerText += string.Format(GetText(11811, "Godkändes av {0}"), currentUser.Name) + (currentUser.Name != attestWorkFlowRow.User.Name ? " " + String.Format(GetText(11814, "för {0}"), attestWorkFlowRow.User.Name) : "");
                        else
                            attestWorkFlowRow.AnswerText += string.Format(GetText(11812, "Avslogs av {0}"), currentUser.Name) + (currentUser.Name != attestWorkFlowRow.User.Name ? " " + String.Format(GetText(11814, "för {0}"), attestWorkFlowRow.User.Name) : "");
                        attestWorkFlowRow.AnswerDate = DateTime.Now;

                        attestWorkFlowRow.Answer = answer;
                        attestWorkFlowRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.Processed;
                        attestWorkFlowRow.State = (int)TermGroup_AttestFlowRowState.Handled;
                        SetModifiedProperties(attestWorkFlowRow);

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Add row for return

                        bool withReturn = false;
                        if (attestWorkFlowRow.OriginateFromRowId.HasValue)
                        {
                            // Check if originate row has process type 'with return'
                            AttestWorkFlowRow originateFromRow = GetAttestWorkFlowRow(entities, attestWorkFlowRow.OriginateFromRowId.Value);
                            if (originateFromRow != null && originateFromRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn)
                            {
                                withReturn = true;
                                AttestWorkFlowRow returnRow = GetAttestWorkFlowRows(entities, attestWorkFlowRow.AttestWorkFlowHeadId, false)?.FirstOrDefault(i => i.OriginateFromRowId == attestWorkFlowRow.AttestWorkFlowRowId && i.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.LevelNotReached);
                                if (returnRow != null)
                                {
                                    returnRow.AttestTransitionId = originateFromRow.AttestTransitionId;
                                    returnRow.OriginateFromRow = attestWorkFlowRow;
                                    returnRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess;
                                    returnRow.AnswerText = String.Format(GetText(9052, "{0}: Returnerad från {1}."), timeStamp, attestWorkFlowRow.User.Name);
                                    returnRow.AnswerDate = DateTime.Now;
                                    returnRow.State = (int)TermGroup_AttestFlowRowState.Unhandled;
                                    returnRow.Type = originateFromRow.Type;
                                    SetModifiedProperties(returnRow);
                                }
                                else
                                {
                                    // Add new row for return
                                    returnRow = new AttestWorkFlowRow()
                                    {
                                        AttestWorkFlowHeadId = originateFromRow.AttestWorkFlowHeadId,
                                        AttestTransitionId = originateFromRow.AttestTransitionId,
                                        AttestRoleId = originateFromRow.AttestRoleId,
                                        UserId = originateFromRow.UserId,
                                        OriginateFromRow = attestWorkFlowRow,
                                        ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess,
                                        AnswerText = String.Format(GetText(9052, "{0}: Returnerad från {1}."), timeStamp, attestWorkFlowRow.User.Name),
                                        AnswerDate = DateTime.Now,
                                        State = (int)TermGroup_AttestFlowRowState.Unhandled,
                                        Type = originateFromRow.Type,
                                    };
                                    SetCreatedProperties(returnRow);
                                    // Also set modified for the date to show up in GUI
                                    SetModifiedProperties(returnRow);
                                    entities.AttestWorkFlowRow.AddObject(returnRow);
                                }

                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        #endregion

                        if (!withReturn)
                        {
                            if (answer)
                            {
                                #region Positive answer

                                // Get head
                                AttestWorkFlowHead attestWorkFlowHead = attestWorkFlowRow.AttestWorkFlowHead ?? GetAttestWorkFlowHead(entities, attestWorkFlowRow.AttestWorkFlowHeadId, true);
                                // Get invoice
                                invoiceId = attestWorkFlowHead.RecordId;
                                SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoiceId);
                                // Get current transition on row
                                AttestTransition rowTransition = GetAttestTransition(entities, attestWorkFlowRow.AttestTransitionId);
                                // Is attest state closed?
                                closedAttestState = closedAttestStateIds.Contains(rowTransition.AttestStateToId);

                                // Get transitions from current state
                                List<AttestTransition> nextLevelTransitions = GetAttestTransitionsFromState(entities, rowTransition.AttestStateToId);
                                // Get all rows
                                List<AttestWorkFlowRow> allRows = GetAttestWorkFlowRows(entities, attestWorkFlowHead.AttestWorkFlowHeadId, false);

                                int attestWorkFlowRowType = attestWorkFlowRow.Type == null ? attestWorkFlowHead.Type : (int)attestWorkFlowRow.Type;

                                if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.Any)
                                {
                                    #region WorkFlowType Any

                                    #region XEMail to users in next level

                                    // Send XEMail if invoice hasn't reached closed state yet
                                    if (!closedAttestStateIds.Contains(rowTransition.AttestStateToId))
                                    {
                                        foreach (var at in nextLevelTransitions)
                                        {
                                            receivers.AddRange(CreateRecipientListForNextAttestLevel(entities, allRows, at.AttestTransitionId, actorCompanyId, applyReplacers: true));
                                        }
                                    }

                                    #endregion

                                    #region AttestWorkFlowRows (the rest)

                                    // Update rest of the (unhandled) rows at the same level to handled because type is 'Any'
                                    foreach (var row in allRows.Where(r => r.AttestTransitionId == attestWorkFlowRow.AttestTransitionId && r.State == (int)TermGroup_AttestFlowRowState.Unhandled && r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered))
                                    {
                                        row.State = (int)TermGroup_AttestFlowRowState.Handled;
                                        row.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.Processed;
                                    }

                                    // Update process type on rows in new level
                                    foreach (var row in allRows.Where(r => r.AttestTransition.AttestStateFromId == rowTransition.AttestStateToId))
                                    {
                                        row.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess;
                                        SetModifiedProperties(row);
                                    }

                                    #endregion

                                    #region Supplier invoice

                                    string currentUsers = string.Empty;

                                    if (receivers.Count > 0)
                                        currentUsers = string.Join(", ", receivers.Select(r => r.Name));

                                    // Update attest state on invoice
                                    if (!closedAttestState)
                                        supplierInvoice.CurrentAttestUsers = currentUsers;
                                    supplierInvoice.AttestStateId = rowTransition.AttestStateToId;
                                    SetModifiedProperties(supplierInvoice);
                                    result = SaveChanges(entities, transaction);
                                    if (!result.Success)
                                        return result;

                                    #endregion

                                    #endregion
                                }
                                else if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.All)
                                {
                                    #region WorkFlowType All

                                    // Check if all the rows at current level are answered
                                    if (allRows.OkToMoveAttestFlowToNextLevel(attestWorkFlowRow.AttestTransitionId))
                                    {
                                        #region XEMail to users in next level

                                        // Send XEMail if invoice hasn't reached closed state yet
                                        if (!closedAttestStateIds.Contains(rowTransition.AttestStateToId))
                                        {
                                            foreach (var at in nextLevelTransitions)
                                            {
                                                receivers.AddRange(CreateRecipientListForNextAttestLevel(entities, allRows, at.AttestTransitionId, actorCompanyId, applyReplacers: true));
                                            }
                                        }

                                        #endregion

                                        #region Supplier invoice

                                        string currentUsers = String.Empty;

                                        if (receivers.Count > 0)
                                            currentUsers = String.Join(", ", receivers.Select(r => r.Name));

                                        // Update attest state on invoice
                                        if (!closedAttestState)
                                            supplierInvoice.CurrentAttestUsers = currentUsers;
                                        supplierInvoice.AttestStateId = rowTransition.AttestStateToId;
                                        SetModifiedProperties(supplierInvoice);

                                        #endregion

                                        #region AttestWorkFlowRows

                                        // Update process type on rows in new level
                                        foreach (var row in allRows.Where(r => r.AttestTransition.AttestStateFromId == rowTransition.AttestStateToId))
                                        {
                                            row.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess;
                                            SetModifiedProperties(row);
                                        }

                                        #endregion

                                        result = SaveChanges(entities, transaction);
                                        if (!result.Success)
                                            return result;
                                    }
                                    else
                                    {
                                        closedAttestState = false;

                                        // Update invoice on who's next to do attest
                                        supplierInvoice.CurrentAttestUsers = this.GetNextUsersToAttestString(entities, allRows, rowTransition.AttestTransitionId, actorCompanyId, applyReplacers: true);
                                        SetModifiedProperties(supplierInvoice);
                                        result = SaveChanges(entities, transaction);
                                        if (!result.Success)
                                            return result;
                                    }

                                    #endregion
                                }

                                #endregion
                            }
                            else
                            {
                                #region Negative answer

                                // TODO: Showstopper someone rejected invoice (who do we send mail to?)

                                #endregion
                            }
                        }
                        else
                        {
                            // Get head
                            AttestWorkFlowHead attestWorkFlowHead = attestWorkFlowRow.AttestWorkFlowHead ?? GetAttestWorkFlowHead(entities, attestWorkFlowRow.AttestWorkFlowHeadId, true);
                            // Get invoice
                            SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, attestWorkFlowHead.RecordId);
                            // Get current transition on row
                            AttestTransition rowTransition = GetAttestTransition(entities, attestWorkFlowRow.AttestTransitionId);
                            // Get transitions from current state
                            var currentLevelTransitions = GetAttestTransitionsFromState(entities, rowTransition.AttestStateFromId);
                            // Get all rows
                            var allRows = GetAttestWorkFlowRows(entities, attestWorkFlowHead.AttestWorkFlowHeadId, false);

                            foreach (var ct in currentLevelTransitions)
                            {
                                receivers.AddRange(CreateRecipientListForNextAttestLevel(entities, allRows, ct.AttestTransitionId, actorCompanyId, applyReplacers: true));
                            }

                            if (receivers.Count > 0)
                                supplierInvoice.CurrentAttestUsers = string.Join(", ", receivers.Select(r => r.Name));

                            SetModifiedProperties(supplierInvoice);
                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                                return result;
                        }

                        //Save attachments
                        if (attachments != null && attachments.Count > 0)
                        {
                            var images = attachments.Where(f => f.ImageId.HasValue);
                            GraphicsManager.UpdateImages(entities, images, invoiceId);

                            var files = attachments.Where(f => f.Id.HasValue).Except(images);
                            GeneralManager.UpdateFiles(entities, files, invoiceId);

                            entities.SaveChanges();
                        }

                        if (result.Success)
                        {
                            if (answer && closedAttestState && !withReturn)
                            {
                                // Transfer to voucher (maybe)
                                result = SupplierInvoiceManager.TryTransferSupplierInvoiceToVoucherAcceptedAttest(entities, new List<int>() { invoiceId }, actorCompanyId);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }

                            //Commit transaction
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = attestWorkFlowRowId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();

                    // Send XEMail if successful
                    if (sendMessage && receivers.Any() && invoiceId != 0 && sendEmail)
                    {
                        mailResult = SendAttestWorkFlowMessageForSupplierInvoice(receivers, actorCompanyId, invoiceId, parameterObject.SoeUser);
                        if (!mailResult.Success)
                        {
                            base.LogError(mailResult.Exception, this.log);
                            base.LogTransactionFailed(this.ToString(), this.log);
                        }
                    }
                    else if (sendMessage && receivers.Any() && invoiceId != 0 && !sendEmail)
                    {
                        result.Value = new Tuple<int, List<MessageRecipientDTO>>(invoiceId, receivers);
                    }
                }

                return result;
            }
        }

        public ActionResult SaveAttestWorkFlowRowAnswers(List<int> invoiceIds, string comment, bool answer, int actorCompanyId, int roleId, List<FileUploadDTO> attachments = null)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            List<Tuple<int, List<MessageRecipientDTO>>> attestedInvoices = new List<Tuple<int, List<MessageRecipientDTO>>>();

            foreach (int invoiceId in invoiceIds)
            {
                AttestWorkFlowHead head = GetAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, true);
                if (head == null)
                    continue;

                AttestWorkFlowRowDTO row = GetAttestWorkFlowRowToAttest(head.AttestWorkFlowHeadId, roleId, base.UserId);
                if (row == null || row.AttestWorkFlowRowId == 0)
                    continue;

                if (row.WorkFlowRowIdToReplace > 0)
                {
                    row.Comment = comment;
                    ActionResult res = ReplaceAttestWorkFlowUser(AttestFlow_ReplaceUserReason.Remove, row.WorkFlowRowIdToReplace, string.Empty, row.UserId.Value, actorCompanyId, invoiceId, false, false);
                    if (res.Success)
                    {
                        int newWorkFlowRowId = res.IntegerValue;
                        var invoiceAttachments = attachments?.Where(x => x.RecordId == invoiceId).ToList() ?? new List<FileUploadDTO>();
                        result = SaveAttestWorkFlowRowAnswer(newWorkFlowRowId, comment, answer, actorCompanyId, attachments: invoiceAttachments);
                    }
                }
                else
                {
                    var invoiceAttachments = attachments?.Where(x => x.RecordId == invoiceId).ToList() ?? new List<FileUploadDTO>();
                    result = SaveAttestWorkFlowRowAnswer(row.AttestWorkFlowRowId, comment, answer, actorCompanyId, false, attachments: invoiceAttachments);
                    if (result.Success && result.Value != null)
                        attestedInvoices.Add((Tuple<int, List<MessageRecipientDTO>>)result.Value);
                }

                if (!result.Success)
                    return result;
            }

            if (attestedInvoices.Count > 0)
            {
                result = SendAttestWorkFlowMessageForMultipleSupplierInvoices(attestedInvoices, actorCompanyId);
            }

            return result;
        }

        public ActionResult SendAttestWorkFlowMessageForMultipleSupplierInvoices(List<Tuple<int, List<MessageRecipientDTO>>> attestedInvoices, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            List<Tuple<int, List<int>>> receivers = new List<Tuple<int, List<int>>>();//Tuple(recepient, invoiceId list)

            try
            {
                //Mapping recipients with invoices
                foreach (var recepient in attestedInvoices.SelectMany(x => x.Item2).DistinctBy(x => x.UserId))
                {
                    var invoiceIds = attestedInvoices.Where(x => x.Item2.Any(y => y.UserId == recepient.UserId)).Select(x => x.Item1).ToList();
                    receivers.Add(Tuple.Create(recepient.UserId, invoiceIds));
                }

                if (receivers.Count > 0)
                {
                    //Send Emails
                    foreach (var receiver in receivers)
                    {
                        result = SendAttestWorkFlowMessageForSupplierInvoice(receiver, actorCompanyId);
                        if (!result.Success)
                        {
                            base.LogError(result.Exception, this.log);
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Success = false;
                result.Exception = ex;
            }

            return result;
        }

        #endregion

        #region AttestFlow XEMail

        private void UpdateCurrentAttestUsers(int actorCompanyId, int invoiceId, int attestWorkFlowHeadId, int currentAttestTransitionId)
        {
            using (var entities = new CompEntities())
            {
                SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoiceId);
                List<AttestWorkFlowRow> allRows = GetAttestWorkFlowRows(entities, attestWorkFlowHeadId, false);
                supplierInvoice.CurrentAttestUsers = this.GetNextUsersToAttestString(entities, allRows, currentAttestTransitionId, actorCompanyId, applyReplacers: true);

                SaveChanges(entities);
            }
        }

        private string GetNextUsersToAttestString(CompEntities entities, List<AttestWorkFlowRow> allRows, int transitionId, int actorCompanyId, bool applyReplacers = false)
        {
            // Update invoice on who's next to do attest
            List<MessageRecipientDTO> currentAttestUsers = CreateRecipientListForNextAttestLevel(entities, allRows, transitionId, actorCompanyId, TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess, applyReplacers);
            return string.Join(", ", currentAttestUsers.Select(u => u.Name));
        }

        private List<MessageRecipientDTO> CreateRecipientListForNextAttestLevel(CompEntities entities, List<AttestWorkFlowRow> allRows, int transitionId, int actorCompanyId, TermGroup_AttestWorkFlowRowProcessType filterOnProcessType = TermGroup_AttestWorkFlowRowProcessType.Unknown, bool applyReplacers = false)
        {
            List<int> userIds = new List<int>();

            // Get users for next transition
            foreach (AttestWorkFlowRow row in allRows.Where(r => r.AttestTransitionId == transitionId && r.State == (int)TermGroup_AttestFlowRowState.Unhandled))
            {
                if (row.UserId.HasValue)
                {
                    if (filterOnProcessType != TermGroup_AttestWorkFlowRowProcessType.Unknown && row.ProcessType != (int)filterOnProcessType)
                        continue;

                    // User specified, check that row is not answered
                    if (!row.Answer.HasValue)
                        userIds.Add(row.UserId.Value);
                }
                else if (row.AttestRoleId.HasValue)
                {
                    // Role specified, add all users from role
                    List<int> usrIds = GetAttestRoleUserIds(entities, actorCompanyId, row.AttestRoleId.Value);
                    foreach (int usrId in usrIds)
                    {
                        userIds.Add(usrId);
                    }
                }
            }

            // Add users to recipient list
            return AddUsersToRecipientList(entities, userIds);
        }

        public ActionResult SendSingleAttestReminders(int actorCompanyId, int roleId, int userId, List<int> invoiceIds)  // Jukka, one email per user including all items to attest
        {
            List<MessageRecipientDTO> usersAndInvoices = new List<MessageRecipientDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            // Gather users from selected invoices. 
            foreach (var invoiceId in invoiceIds)
            {
                //Select invoice
                SupplierInvoice invoice = SupplierInvoiceManager.GetSupplierInvoice(invoiceId, false, false, false, false, false, false, false, false, false);
                if (invoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                // Linkkaa AttestWorkFlow laskuun
                AttestWorkFlowHead attWorkFlowHead = GetAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, false);
                if (attWorkFlowHead == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestWorkFlowHead");

                var baseQuery = (from workFlowRow in entitiesReadOnly.AttestWorkFlowRow
                                 where
                                  workFlowRow.AttestTransition.AttestStateFromId == invoice.AttestStateId &&
                                  workFlowRow.AttestWorkFlowHeadId == attWorkFlowHead.AttestWorkFlowHeadId &&
                                  workFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess &&
                                  workFlowRow.AttestTransition.ActorCompanyId == actorCompanyId &&
                                  workFlowRow.State == (int)TermGroup_AttestFlowRowState.Unhandled
                                 select workFlowRow);

                List<MessageRecipientDTO> workFlowRowUsers = (from workFlowRow in baseQuery
                                                              where workFlowRow.UserId.HasValue && workFlowRow.State == (int)TermGroup_AttestFlowRowState.Unhandled
                                                              select new MessageRecipientDTO()
                                                              {
                                                                  UserId = workFlowRow.UserId.Value,
                                                                  ExternalId = invoiceId
                                                              }).ToList();

                foreach (MessageRecipientDTO workFlowRowUser in workFlowRowUsers)
                {
                    if (!usersAndInvoices.Any(u => u.UserId == workFlowRowUser.UserId && u.ExternalId == workFlowRowUser.ExternalId))
                        usersAndInvoices.Add(workFlowRowUser);
                }
            }

            // users now have all users and Invoice numbers to receive reminders inside
            return SendAttestWorkFlowMessagesForUser(usersAndInvoices, actorCompanyId, roleId, UserManager.GetUser(userId), true);
        }

        public ActionResult SendAttestWorkFlowMessagesForUser(List<MessageRecipientDTO> receiversListAndInvoices, int actorCompanyId, int roleId, User user, bool isReminder = false)
        {
            ActionResult result = new ActionResult(true);
            ActionResult defresult = new ActionResult(true);

            List<MessageRecipientDTO> invoicesOrderedByUsers = new List<MessageRecipientDTO>();
            List<MessageRecipientDTO> onlyreceiversOrderedByUsers = new List<MessageRecipientDTO>();

            string defaultEmail = this.SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, actorCompanyId, 0);
            int nrSuccess = 0;
            int nrFailures = 0;

            foreach (MessageRecipientDTO item in receiversListAndInvoices.GroupBy(i => i.UserId).Select(group => group.First()))
            {
                onlyreceiversOrderedByUsers.Add(item);
            }

            foreach (MessageRecipientDTO item2 in receiversListAndInvoices.OrderBy(i => i.UserId).ThenBy(i => i.ExternalId))
            {
                invoicesOrderedByUsers.Add(item2);
            }

            foreach (MessageRecipientDTO singleUser in onlyreceiversOrderedByUsers)
            {
                StringBuilder messageText = new StringBuilder();

                foreach (var invoice in invoicesOrderedByUsers)
                {
                    if (invoice.UserId == singleUser.UserId)
                    {
                        using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                        entities.Invoice.NoTracking();
                        var sInvoice = (from i in entities.InvoicePaymentUnionView
                                        where i.ActorCompanyId == actorCompanyId &&
                                        i.OriginType == (int)SoeOriginType.SupplierInvoice &&
                                        i.InvoiceId == invoice.ExternalId
                                        select i).FirstOrDefault();
                        if (sInvoice == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "Invoice");

                        #region Prefix

                        string prefix = string.Empty;
                        switch (Environment.MachineName.ToLower())
                        {
                            case "xetester":
                                prefix = "xetest.softone.se";
                                break;
                            case "softones09":
                                prefix = "xe.softone.se";
                                break;
                            case "softones11":
                                prefix = "xe.softone.se";
                                break;
                            case "softones20":
                                prefix = "xe.softone.se";
                                break;
                            case "softones19":
                                prefix = "xe2.softone.se";
                                break;
                            case "softones23":
                                prefix = "try.softone.se";
                                break;
                            case "softones22":
                                prefix = "demo.softone.se";
                                break;
                            case "softones17":
                                prefix = "skola.softone.se";
                                break;
                            case "kaide-hp":
                                prefix = "release.softone.se";//testing through development
                                break;
                        }

                        //If none of the above
                        if (prefix == String.Empty && System.ServiceModel.OperationContext.Current != null)
                            prefix = System.ServiceModel.OperationContext.Current.RequestContext.RequestMessage.Headers.To.Host;
                        if (prefix.Contains("localhost"))
                            prefix = "xetest.softone.se";

                        if (prefix == string.Empty) //testing through development
                            prefix = "release.softone.se";

                        #endregion

                        if (isReminder)
                            messageText.Append(GetText(9042, "Påminnelse").ToUpper() + "<br/><br/>");
                        messageText.Append(GetText(7132, "Faktura redo för attestering") + "<br/><br/>" + GetText(7133, "Fakturanr") + ": ");
                        messageText.Append(sInvoice.InvoiceNr);
                        messageText.Append("<br/>" + GetText(7134, "Leverantör") + ": " + sInvoice.ActorName + "<br/>" + GetText(7135, "Belopp") + ": " + sInvoice.InvoiceTotalAmountCurrency + " " + CountryCurrencyManager.GetCurrencyCode(sInvoice.SysCurrencyId));
                        messageText.Append("<br/>");
                    }
                }

                var dto = new MessageEditDTO
                {
                    ActorCompanyId = actorCompanyId,
                    LicenseId = user.LicenseId,
                    Entity = SoeEntityType.SupplierInvoice,
                    //RecordId = invoiceId,
                    Created = DateTime.Now,
                    SentDate = DateTime.Now,
                    SenderUserId = user.UserId,
                    SenderName = GetText(4573, "Faktura för attest"),
                    Subject = (isReminder ?
                        GetText(9042, "Påminnelse") + " " + GetText(9261, "Attestera fakturor").ToLower() :
                        GetText(9261, "Attestera fakturor")),
                    Text = messageText.ToString(),
                    ShortText = messageText.ToString().Replace("<br/>", "\r\n"),
                    AnswerType = XEMailAnswerType.None,
                    MessagePriority = TermGroup_MessagePriority.Normal,
                    MessageType = TermGroup_MessageType.AttestInvoice,
                    MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                    MessageTextType = TermGroup_MessageTextType.HTML,
                };

                // Set senders email. 
                if (defaultEmail != null && defaultEmail.Trim() != String.Empty)
                    dto.SenderEmail = defaultEmail;
                else if (user.Email != null)
                    dto.SenderEmail = user.Email;

                dto.Recievers.Add(singleUser);

                var subresult = CommunicationManager.SendXEMail(dto, actorCompanyId, roleId, user.UserId, false);
                if (subresult.Success)
                    nrSuccess++;
                else
                    nrFailures++;

                if (subresult != defresult)
                    result = subresult;
                result.IntegerValue = nrSuccess;
                result.IntegerValue2 = nrFailures;
            }

            return result;
        }

        public ActionResult SendAttestWorkFlowMessageForSupplierInvoice(List<MessageRecipientDTO> receiversList, int actorCompanyId, int invoiceId, UserDTO user, bool isReminder = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Invoice.NoTracking();
            var sInvoice = (from i in entities.InvoicePaymentUnionView
                            where i.ActorCompanyId == actorCompanyId &&
                            i.OriginType == (int)SoeOriginType.SupplierInvoice &&
                            i.InvoiceId == invoiceId
                            select i).FirstOrDefault();

            if (sInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Invoice");

            Company company = CompanyManager.GetCompany(actorCompanyId);

            string defaultEmail = this.SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, actorCompanyId, 0);

            // Link to invoice
            StringBuilder messageText = new StringBuilder();
            if (isReminder)
                messageText.Append(GetText(9042, "Påminnelse").ToUpper() + "<br/><br/>");

            messageText.Append(GetText(7132, "Faktura redo för attestering") + "<br/><br/>");
            messageText.Append(GetText(1011, "Företag") + ": " + company.Name + "<br/>");
            messageText.Append(GetText(7133, "Fakturanr") + ": ");
            messageText.Append(sInvoice.InvoiceNr);
            messageText.Append("<br/>" + GetText(7134, "Leverantör") + ": " + sInvoice.ActorName + "<br/>" + GetText(7135, "Belopp") + ": " + sInvoice.InvoiceTotalAmountCurrency + " " + CountryCurrencyManager.GetCurrencyCode(sInvoice.SysCurrencyId));

            var dto = new MessageEditDTO
            {
                ActorCompanyId = actorCompanyId,
                LicenseId = user.LicenseId,
                Entity = SoeEntityType.SupplierInvoice,
                RecordId = invoiceId,
                Created = DateTime.Now,
                SentDate = DateTime.Now,
                SenderUserId = user.UserId,
                SenderName = GetText(4573, "Faktura för attest"),
                Subject = (isReminder ?
                    GetText(9042, "Påminnelse") + " " + GetText(7139, "Attestera").ToLower() + ": " :
                    GetText(7139, "Attestera"))
                    + ": " + sInvoice.ActorName + " " + CountryCurrencyManager.GetCurrencyCode(sInvoice.SysCurrencyId) + " " + sInvoice.InvoiceTotalAmountCurrency,
                Text = messageText.ToString(),
                ShortText = messageText.ToString().Replace("<br/>", "\r\n"),
                AnswerType = XEMailAnswerType.None,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageType = TermGroup_MessageType.AttestInvoice,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.HTML,
            };

            if (defaultEmail != null && defaultEmail.Trim() != string.Empty)
            {
                dto.SenderEmail = defaultEmail;
            }
            else
            {
                if (user.Email != null)
                    dto.SenderEmail = user.Email;
            }

            dto.Recievers = receiversList;

            return CommunicationManager.SendXEMail(dto, actorCompanyId, 0, user.UserId, false);
        }

        public ActionResult SendAttestWorkFlowMessageForSupplierInvoice(Tuple<int, List<int>> receiver, int actorCompanyId, bool isReminder = false)
        {
            var senderUser = UserManager.GetUser(base.UserId);
            var recieverUser = UserManager.GetUser(receiver.Item1);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Invoice.NoTracking();
            var invoices = (from i in entities.InvoicePaymentUnionView
                            where i.ActorCompanyId == actorCompanyId &&
                            i.OriginType == (int)SoeOriginType.SupplierInvoice &&
                            receiver.Item2.Contains(i.InvoiceId)
                            select i);

            Company company = CompanyManager.GetCompany(actorCompanyId);

            string defaultEmail = this.SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, actorCompanyId, 0);

            // Link to invoice
            StringBuilder messageText = new StringBuilder();
            if (isReminder)
                messageText.Append(GetText(9042, "Påminnelse").ToUpper() + "<br/><br/>");

            messageText.Append(invoices.Count() > 1 ? GetText(11815, "Fakturor redo för attestering") : GetText(7132, "Faktura redo för attestering"));
            messageText.Append("<br/><br/>");

            foreach (var sInvoice in invoices)
            {
                messageText.Append(GetText(1011, "Företag") + ": " + company.Name + "<br/>");
                messageText.Append(GetText(7133, "Fakturanr") + ": ");
                messageText.Append(sInvoice.InvoiceNr);
                messageText.Append("<br/>" + GetText(7134, "Leverantör") + ": " + sInvoice.ActorName + "<br/>" + GetText(7135, "Belopp") + ": " + sInvoice.InvoiceTotalAmountCurrency + " " + CountryCurrencyManager.GetCurrencyCode(sInvoice.SysCurrencyId));
                messageText.Append("<br/><br/>");
            }

            var dto = new MessageEditDTO
            {
                ActorCompanyId = actorCompanyId,
                LicenseId = senderUser.LicenseId,
                Entity = SoeEntityType.SupplierInvoice,
                Created = DateTime.Now,
                SentDate = DateTime.Now,
                SenderUserId = senderUser.UserId,
                SenderName = invoices.Count() > 1 ? GetText(11816, "Fakturor för attest") : GetText(4573, "Faktura för attest"),
                Subject = (isReminder ?
                    GetText(9042, "Påminnelse") + " " + GetText(7139, "Attestera").ToLower() + ": " :
                    GetText(7139, "Attestera")),
                Text = messageText.ToString(),
                ShortText = messageText.ToString().Replace("<br/>", "\r\n"),
                AnswerType = XEMailAnswerType.None,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageType = TermGroup_MessageType.AttestInvoice,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.HTML,
            };

            if (defaultEmail != null && defaultEmail.Trim() != string.Empty)
            {
                dto.SenderEmail = defaultEmail;
            }
            else
            {
                if (senderUser.Email != null)
                    dto.SenderEmail = senderUser.Email;
            }

            dto.Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { UserId = recieverUser.UserId, Name = recieverUser.Name, Type = XEMailRecipientType.User } };

            return CommunicationManager.SendXEMail(dto, actorCompanyId, 0, senderUser.UserId, false);
        }

        #endregion

        #region AttestWorkFlowGroup

        private bool ValidateAttestWorkFlowGroup(int actorCompanyId, string name, string code, int attestWorkFlowHeadId = 0, CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (entities == null)
                entities = entitiesReadOnly;

            if (attestWorkFlowHeadId != 0)
            {
                var entries = (from entry in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                               where
                               entry.ActorCompanyId == actorCompanyId &&
                               entry.AttestWorkFlowHeadId != attestWorkFlowHeadId &&
                               entry.State == (int)SoeEntityState.Active &&
                               (entry.AttestGroupCode.ToLower() == code.ToLower() || entry.AttestGroupName.ToLower() == name.ToLower())
                               select entry);

                return entries.Any();

            }
            else
            {
                var entries = (from entry in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                               where
                               entry.ActorCompanyId == actorCompanyId &&
                               entry.State == (int)SoeEntityState.Active &&
                               (entry.AttestGroupCode.ToLower() == code.ToLower() || entry.AttestGroupName.ToLower() == name.ToLower())
                               select entry);

                return entries.Any();
            }
        }

        public AttestWorkFlowGroup GetAttestWorkFlowGroup(int attestWorkFlowHeadId, int actorCompanyId, bool loadRows)
        {
            return this.GetAttestWorkFlowGroupQuery(attestWorkFlowHeadId, actorCompanyId, loadRows).FirstOrDefault();
        }

        public IQueryable<AttestWorkFlowGroup> GetAttestWorkFlowGroupQuery(int attestWorkFlowHeadId, int actorCompanyId, bool loadRows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var oQuery = entitiesReadOnly.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>();
            if (loadRows)
                oQuery = oQuery.Include("AttestWorkFlowRow");

            var query = (from entry in oQuery
                         where entry.ActorCompanyId == actorCompanyId &&
                         entry.AttestWorkFlowHeadId == attestWorkFlowHeadId &&
                         entry.State == (int)SoeEntityState.Active
                         select entry);

            return query;
        }

        public AttestWorkFlowGroup GetAttestGroupSuggestion(int actorCompanyId, int supplierId, int projectId, int costplaceAccountId, string referenceOur)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestGroupSuggestion(entities, actorCompanyId, supplierId, projectId, costplaceAccountId, referenceOur);
        }

        public AttestWorkFlowGroup GetAttestGroupSuggestion(CompEntities entities, int actorCompanyId, int supplierId, int projectId, int costplaceAccountId, string referenceOur)
        {
            #region init

            int attestWorkFlowHeadId = 0;

            //get attestworkflowheadid from supplier
            Supplier supplier = SupplierManager.GetSupplier(entities, supplierId);
            if (supplier == null)
            {
                throw new Exception(GetText(5897) + ": " + supplierId);
            }

            int attestWorkFlowHeadId_Supplier = supplier.AttestWorkFlowGroupId != null ? (int)supplier.AttestWorkFlowGroupId : 0;

            //get attestworkflowheadid from costplace
            Account costplace = AccountManager.GetAccount(entities, actorCompanyId, costplaceAccountId, false);
            int attestWorkFlowHeadId_Costplace = costplace != null && costplace.AttestWorkFlowHeadId != null ? (int)costplace.AttestWorkFlowHeadId : 0;

            //get attestworkflowheadid from project
            int attestWorkFlowHeadId_Project = 0;
            if (projectId != 0)
            {
                var attWorkFlowHeadId = (from i in entities.Project
                                         where i.ActorCompanyId == actorCompanyId &&
                                         i.ProjectId == projectId
                                         select i.AttestWorkFlowHeadId).FirstOrDefault();

                attestWorkFlowHeadId_Project = attWorkFlowHeadId != null ? (int)attWorkFlowHeadId : 0;
            }

            //get attestworkflowheadid from attestgroup
            int attestWorkFlowHeadId_ReferenceOur = 0;
            if (referenceOur != null && referenceOur != string.Empty)
            {
                var attestGroup = (from i in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                                   where i.ActorCompanyId == actorCompanyId &&
                                   i.AttestGroupCode == referenceOur &&
                                   i.State == (int)SoeEntityState.Active
                                   select i).FirstOrDefault();

                attestWorkFlowHeadId_ReferenceOur = attestGroup != null ? attestGroup.AttestWorkFlowHeadId : 0;
            }

            #endregion

            #region Prio
            //Prio1:
            int prio1 = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio1, 0, ActorCompanyId, 0);
            switch (prio1)
            {
                case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.None:
                    attestWorkFlowHeadId = attestWorkFlowHeadId_Supplier != 0 ? attestWorkFlowHeadId_Supplier : attestWorkFlowHeadId;
                    break;

                case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Supplier:
                    attestWorkFlowHeadId = attestWorkFlowHeadId_Supplier;
                    break;

                case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Costplace:
                    attestWorkFlowHeadId = attestWorkFlowHeadId_Costplace;
                    break;

                case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Project:
                    attestWorkFlowHeadId = attestWorkFlowHeadId_Project;
                    break;

                case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.OurReference:
                    attestWorkFlowHeadId = attestWorkFlowHeadId_ReferenceOur;
                    break;
            }

            //Prio2:
            if (attestWorkFlowHeadId == 0)
            {
                int prio2 = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio2, 0, ActorCompanyId, 0);
                switch (prio2)
                {
                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.None:

                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Supplier:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Supplier;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Costplace:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Costplace;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Project:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Project;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.OurReference:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_ReferenceOur;
                        break;
                }
            }

            //Prio3:
            if (attestWorkFlowHeadId == 0)
            {
                int prio3 = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio3, 0, ActorCompanyId, 0);
                switch (prio3)
                {
                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.None:

                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Supplier:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Supplier;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Costplace:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Costplace;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Project:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Project;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.OurReference:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_ReferenceOur;
                        break;
                }
            }

            //Prio4:
            if (attestWorkFlowHeadId == 0)
            {
                int prio4 = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio4, 0, ActorCompanyId, 0);
                switch (prio4)
                {
                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.None:

                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Supplier:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Supplier;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Costplace:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Costplace;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.Project:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_Project;
                        break;

                    case (int)TermGroup_SupplierInvoiceAttestGroupSuggestionPrio.OurReference:
                        attestWorkFlowHeadId = attestWorkFlowHeadId_ReferenceOur;
                        break;
                }
            }

            if (attestWorkFlowHeadId == 0)
                attestWorkFlowHeadId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup, 0, ActorCompanyId, 0);

            var attestGroupSuggestion = (from entry in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                                         where entry.ActorCompanyId == actorCompanyId &&
                                         entry.AttestWorkFlowHeadId == attestWorkFlowHeadId
                                         select entry).FirstOrDefault();

            return attestGroupSuggestion;

            #endregion
        }

        public IEnumerable<AttestGroupGridDTO> GetAttestWorkFlowGroups(int actorCompanyId, int? attestWorkFlowHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestWorkFlowGroups(entities, actorCompanyId, attestWorkFlowHeadId);
        }

        public IEnumerable<AttestGroupGridDTO> GetAttestWorkFlowGroups(CompEntities entities, int actorCompanyId, int? attestWorkFlowHeadId = null)
        {
            IQueryable<AttestGroupGridDTO> query = (from entry in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                                                    where entry.ActorCompanyId == actorCompanyId && entry.State == (int)SoeEntityState.Active
                                                    select new AttestGroupGridDTO()
                                                    {
                                                        Code = entry.AttestGroupCode,
                                                        Name = entry.AttestGroupName,
                                                        AttestWorkFlowHeadId = entry.AttestWorkFlowHeadId,
                                                        AttestWorkFlowTemplateHeadId = entry.AttestWorkFlowTemplateHeadId,
                                                    });

            if (attestWorkFlowHeadId.HasValue)
            {
                query = query.Where(ag => ag.AttestWorkFlowHeadId.Equals(attestWorkFlowHeadId.Value));
            }

            return query.ToList();
        }

        public List<AttestWorkFlowGroup> GetAttestWorkFlowGroupsSimple(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowHead.NoTracking();
            return GetAttestWorkFlowGroupsSimple(entities, actorCompanyId);
        }

        public List<AttestWorkFlowGroup> GetAttestWorkFlowGroupsSimple(CompEntities entities, int actorCompanyId)
        {
            return (from entry in entities.AttestWorkFlowHead.OfType<AttestWorkFlowGroup>()
                    where entry.ActorCompanyId == actorCompanyId && entry.State == (int)SoeEntityState.Active
                    select entry).ToList();
        }

        public Dictionary<int, string> GetAttestWorkFlowGroups(int actorCompanyId, bool addEmptyRow, int? attestWorkFlowHeadId = null)
        {

            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var attestGroups = GetAttestWorkFlowGroups(actorCompanyId, attestWorkFlowHeadId);
            foreach (var attestGroup in attestGroups)
            {
                dict.Add(attestGroup.AttestWorkFlowHeadId, attestGroup.Name ?? " ");
            }

            return dict;
        }

        #endregion

        #region Document signing

        public ActionResult InitiateDocumentSigning(AttestWorkFlowHeadDTO headInput, int actorCompanyId, int userId, int licenseId)
        {
            if (headInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestWorkFlowHead");

            ActionResult result = new ActionResult(true);

            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
            int attestWorkFlowHeadId = 0;

            User user = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        user = UserManager.GetUser(entities, userId);
                        if (user == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

                        DataStorage dataStorage = GeneralManager.GetDataStorageByDataStorageRecordId(entities, headInput.RecordId, actorCompanyId, true);
                        if (dataStorage == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorage");

                        string fileKey = Guid.NewGuid().ToString();

                        #endregion

                        #region AttestWorkFlowHead

                        AttestWorkFlowHead attestWorkFlowHead = new AttestWorkFlowHead
                        {
                            AttestWorkFlowTemplateHeadId = headInput.AttestWorkFlowTemplateHeadId,
                            ActorCompanyId = actorCompanyId,
                            Entity = (int)headInput.Entity,
                            Type = (int)headInput.Type,
                            RecordId = headInput.RecordId,
                            State = (int)headInput.State,
                            Name = headInput.Name,
                            SendMessage = headInput.SendMessage,
                            AdminInformation = headInput.AdminInformation
                        };
                        SetCreatedProperties(attestWorkFlowHead, user);
                        entities.AttestWorkFlowHead.AddObject(attestWorkFlowHead);

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        attestWorkFlowHeadId = attestWorkFlowHead.AttestWorkFlowHeadId;

                        #endregion

                        #region AttestWorkFlowRows

                        int firstTransitionId = 0;

                        // Add rows for all transitions
                        foreach (AttestWorkFlowRowDTO rowInput in headInput.Rows)
                        {
                            // Remember first AttestTransitionId (used to send XEMail and connect to SoftOneId for users of first transition)
                            if (firstTransitionId == 0)
                                firstTransitionId = rowInput.AttestTransitionId;

                            // User must be set
                            if (!rowInput.UserId.HasValue)
                                continue;

                            #region AttestWorkFlowRow

                            // Get signature attest role by user and transition
                            int? attestRoleId = rowInput.AttestRoleId;
                            if ((!attestRoleId.HasValue || attestRoleId == 0) && rowInput.UserId.HasValue)
                                attestRoleId = GetSigningAttestRoleIdByAttestTransition(entities, rowInput.AttestTransitionId, rowInput.UserId.Value);

                            AttestWorkFlowRow attestWorkFlowRow = new AttestWorkFlowRow()
                            {
                                AttestWorkFlowHeadId = attestWorkFlowHeadId,
                                AttestTransitionId = rowInput.AttestTransitionId,
                                AttestRoleId = attestRoleId,
                                UserId = rowInput.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered ? user.UserId : rowInput.UserId,
                                ProcessType = (int)rowInput.ProcessType,
                                State = (int)TermGroup_AttestFlowRowState.Unhandled,
                                Type = rowInput.Type == null ? attestWorkFlowHead.Type : (int?)rowInput.Type,
                                Comment = rowInput.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && headInput.AdminInformation.HasValue() ? headInput.AdminInformation : null,
                                CommentDate = rowInput.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && headInput.AdminInformation.HasValue() ? attestWorkFlowHead.Created : null,
                                CommentUser = rowInput.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered && headInput.AdminInformation.HasValue() ? user.Name : null,
                            };
                            SetCreatedProperties(attestWorkFlowRow, user);

                            // Set modified for certain process types to get the date to show up in GUI
                            if (attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Registered ||
                                attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess ||
                                attestWorkFlowRow.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Returned)
                                SetModifiedProperties(attestWorkFlowRow, user);
                            entities.AttestWorkFlowRow.AddObject(attestWorkFlowRow);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #endregion

                            #region Create receivers for first transition

                            if (rowInput.AttestTransitionId == firstTransitionId && rowInput.ProcessType != TermGroup_AttestWorkFlowRowProcessType.Registered)
                            {
                                User attestWorkFlowUser = UserManager.GetUser(entities, rowInput.UserId.Value);
                                if (attestWorkFlowUser != null)
                                    receivers.Add(new MessageRecipientDTO() { UserId = rowInput.UserId.Value, Name = attestWorkFlowUser.Name, ExternalId = attestWorkFlowRow.AttestWorkFlowRowId });
                            }

                            #endregion
                        }

                        #endregion

                        #region DataStorage

                        // Create a copy of the DataStorage, keeping the existing one as an original

                        if (headInput.Entity == SoeEntityType.DataStorageRecord)
                        {
                            DataStorage newDataStorage = new DataStorage
                            {
                                ActorCompanyId = dataStorage.ActorCompanyId,
                                ParentDataStorageId = dataStorage.DataStorageId,    // Relation to original
                                EmployeeId = dataStorage.EmployeeId,
                                TimePeriodId = dataStorage.TimePeriodId,
                                Type = dataStorage.Type,
                                Description = dataStorage.Description,
                                Data = dataStorage.Data,
                                DataCompressed = dataStorage.DataCompressed,
                                XML = dataStorage.XML,
                                XMLCompressed = dataStorage.XMLCompressed,
                                FileSize = dataStorage.FileSize,
                                FileName = dataStorage.FileName,
                                Name = dataStorage.Name,
                                Hash = dataStorage.Hash,
                                OriginType = dataStorage.OriginType,
                                Extension = dataStorage.Extension,
                                ExternalLink = fileKey,         // Key to send to SoftOneId
                                Folder = dataStorage.Folder,
                                ValidFrom = dataStorage.ValidFrom,
                                ValidTo = dataStorage.ValidTo,
                                UserId = dataStorage.UserId,
                                NeedsConfirmation = dataStorage.NeedsConfirmation
                            };

                            SetCreatedProperties(newDataStorage, user);
                            entities.AddObject("DataStorage", newDataStorage);
                            dataStorage.State = (int)SoeEntityState.Inactive;
                            SetModifiedProperties(dataStorage);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            string currentUsers = String.Empty;
                            if (receivers.Count > 0)
                                currentUsers = string.Join(", ", receivers.Select(r => r.Name));

                            AttestTransition att = GetAttestTransition(entities, firstTransitionId);

                            // Link records to new storage and update attest information
                            // Should be only one, but in database it's a collection
                            foreach (DataStorageRecord rec in dataStorage.DataStorageRecord.ToList())
                            {
                                rec.DataStorageId = newDataStorage.DataStorageId;
                                rec.AttestStateId = att.AttestStateFromId;
                                rec.CurrentAttestUsers = currentUsers;
                                rec.AttestStatus = (int)TermGroup_DataStorageRecordAttestStatus.Initialized;
                                SetModifiedProperties(rec, user);
                            }

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        #region Send to SoftOneId

                        result = SendDocumentSigningFlowToSoftOneId(transaction, entities, receivers, headInput.RecordId, fileKey, headInput.AdminInformation, userId, actorCompanyId, licenseId, true, true);
                        string signPageLink = result.StringValue;

                        #endregion

                        if (result.Success)
                        {
                            transaction.Complete();
                            result.IntegerValue = attestWorkFlowHeadId;

                            // If initiator has checked "Sign initial", return link to sign page
                            result.StringValue = headInput.SignInitial ? signPageLink : string.Empty;
                        }
                        else
                        {
                            LogError(result.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SendDocumentSigningFlowToSoftOneId(TransactionScope transaction, CompEntities entities, List<MessageRecipientDTO> receivers, int recordId, string fileKey, string adminInformation, int userId, int actorCompanyId, int licenseId, bool forceSendToReceiver = false, bool forceSendToEmailReceiver = false)
        {
            User currentUser = UserManager.GetUser(entities, userId);
            if (currentUser == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

            ActionResult result = new ActionResult();

            int? externalIdOnSelf = null;

            List<CommunicatorPerson> commPersons = new List<CommunicatorPerson>();
            foreach (MessageRecipientDTO receiver in receivers)
            {
                User commUser = UserManager.GetUser(entities, receiver.UserId);
                if (commUser != null)
                {
                    if (commUser.UserId == currentUser.UserId)
                        externalIdOnSelf = receiver.ExternalId;

                    string signeeKey = Guid.NewGuid().ToString();
                    receiver.SigneeKey = signeeKey;

                    CommunicatorPerson commPerson = new CommunicatorPerson();
                    commPerson.SetSigneeKey(signeeKey);
                    commPerson.SetIdLoginGuid(commUser.idLoginGuid.Value.ToString());
                    commPerson.ExternalId = receiver.ExternalId;
                    commPerson.CompanyId = actorCompanyId;
                    commPerson.Email = commUser.Email;
                    commPersons.Add(commPerson);
                }
            }

            CommunicatorMessage commMessage = new CommunicatorMessage();
            commMessage.SetLicenseIdToMetaData(licenseId);
            commMessage.SetSysCompDbIdToMetaData(SysServiceManager.GetSysCompDBId().Value);
            commMessage.SetSignatureFileKey(fileKey);
            commMessage.SetSignatureKey(fileKey);
            commMessage.CommunicationType = CommunicationType.IdSignature;
            commMessage.Body = adminInformation;
            commMessage.Recievers = commPersons;

            // Send signature flow to SoftOneId
            // TODO: Better error handling
            CommunicatorMessage commResult = SoftOneIdConnector.SaveSignature(commMessage);
            if (commResult == null || commResult.Recievers == null || !commResult.Recievers.Any())
            {
                base.LogError("SendDocumentSigningFlowToSoftOneId failed message: " + JsonConvert.SerializeObject(commResult) + " Resonse " + JsonConvert.SerializeObject(commResult));
                return new ActionResult((int)ActionResultSave.NoItemsToProcess, "SendDocumentSigningFlowToSoftOneId: " + (commResult != null ? StringUtility.NullToEmpty(commResult.Subject) : string.Empty));
            }

            // Send XEMail to receivers
            string signPageLink = null;
            foreach (CommunicatorPerson commPerson in commResult.Recievers)
            {
                MessageRecipientDTO receiver = receivers.FirstOrDefault(r => r.SigneeKey == commPerson.GetSigneeKey());
                if (receiver != null)
                {
                    var sendResult = SendDocumentSigningRequestMessage(transaction, entities, receiver.ObjToList(), adminInformation, commPerson.KeySecondary, actorCompanyId, recordId, currentUser.ToDTO(), false, forceSendToReceiver, forceSendToEmailReceiver);
                    if (!sendResult.Success)
                    {
                        if (sendResult.Exception != null)
                            base.LogError(result.Exception, this.log);
                        else
                            base.LogError("SendDocumentSigningRequestMessage failed "
                                + (!string.IsNullOrEmpty(sendResult.ErrorMessage) ? sendResult.ErrorMessage : "")
                                + (!string.IsNullOrEmpty(sendResult.StringValue) ? sendResult.StringValue : ""));
                    }
                    if (!externalIdOnSelf.IsNullOrEmpty() && externalIdOnSelf == commPerson.ExternalId)
                        signPageLink = commPerson.KeySecondary;
                }
            }

            result.StringValue = signPageLink;

            return result;
        }

        public ActionResult ResendAndExtendIfError(CompEntities entities, int sysCompDbId, int actorCompanyId, int licenseId, DateTime from, DateTime to, bool send)
        {
            var statuses = SoftOneIdConnector.GetSignatureStatuses(sysCompDbId, licenseId, from, to);
            var sb = new StringBuilder();
            sb.AppendLine($"Statuses count {statuses.Count}");

            foreach (var status in statuses)
            {
                EnumerationsSID.SignatureStatus st = status.Status;
                var signeeStatuses = status.SigneeStatuses;

                foreach (var signeeStatus in signeeStatuses)
                {
                    var matching = entities.AttestWorkFlowRow.Include(i => i.AttestWorkFlowHead).FirstOrDefault(r => r.AttestWorkFlowRowId == signeeStatus.ExternalId);
                    var resend = false;

                    if (matching?.AttestWorkFlowHead?.RecordId == null)
                        continue;

                    DataStorageRecord dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, matching.AttestWorkFlowHead.RecordId);

                    if (dataStorageRecord == null)
                        continue;

                    if (signeeStatus.Status == EnumerationsSID.SigneeStatus.Signed && dataStorageRecord.AttestStatus == (int)TermGroup_DataStorageRecordAttestStatus.Signed)
                        continue;

                    switch (st)
                    {
                        case EnumerationsSID.SignatureStatus.Initialized:
                            if (dataStorageRecord.AttestStatus != (int)TermGroup_DataStorageRecordAttestStatus.Initialized)
                                resend = true;
                            break;
                        case EnumerationsSID.SignatureStatus.PartlySigned:
                            if (dataStorageRecord.AttestStatus != (int)TermGroup_DataStorageRecordAttestStatus.PartlySigned)
                                resend = true;
                            break;
                        case EnumerationsSID.SignatureStatus.Signed:
                            if (dataStorageRecord.AttestStatus != (int)TermGroup_DataStorageRecordAttestStatus.Signed)
                                resend = true;
                            break;
                        case EnumerationsSID.SignatureStatus.Rejected:
                            if (dataStorageRecord.AttestStatus != (int)TermGroup_DataStorageRecordAttestStatus.Rejected)
                                resend = true;
                            break;
                        case EnumerationsSID.SignatureStatus.Cancelled:
                            if (dataStorageRecord.AttestStatus != (int)TermGroup_DataStorageRecordAttestStatus.Cancelled)
                                resend = true;
                            break;
                        default:
                            break;
                    }

                    if (resend)
                    {
                        var employee = EmployeeManager.GetEmployee(entities, dataStorageRecord.DataStorage.EmployeeId ?? 0, actorCompanyId, DateTime.Today, DateTime.Today.AddYears(1), loadEmployment: true, loadContactPerson: true);

                        if (employee == null)
                            continue;

                        var employment = employee.GetEmployment(DateTime.Today, DateTime.Today.AddYears(1));

                        if (employment == null)
                            continue;

                        var mess = entities.Message.FirstOrDefault(f => f.RecordId == dataStorageRecord.DataStorageRecordId && f.State == 0);

                        if (mess != null)
                        {
                            var message = CommunicationManager.GetMessage(entities, mess.MessageId, true, true);

                            if (message != null)
                            {
                                var user = UserManager.GetUser(entities, message.UserId);
                                var receivers = new List<MessageRecipientDTO>()
                                {
                                    new MessageRecipientDTO()
                                    {
                                        UserId = employee.UserId ?? 0,
                                        Name = employee.Name,
                                        Type = XEMailRecipientType.User
                                    }
                                };

                                if (send)
                                {
                                    var result = SendDocumentSigningRequestMessage(null, entities, receivers, message.MessageText.Text, signeeStatus.Code, actorCompanyId, dataStorageRecord.DataStorageRecordId, UserManager.GetUser(entities, message.UserId).ToDTO(), true, true, true);

                                    var updateResult = SoftOneIdConnector.UpdateSignatureCode(signeeStatus.Code, signeeStatus.Code, signeeStatus.CodeValidTo, DateTime.Today.AddDays(30));
                                }
                                else
                                    sb.AppendLine($"Resend {signeeStatus.Code} to {employee.NumberAndName} id {employee.EmployeeId} dsrid {dataStorageRecord.DataStorageRecordId} workFlowRowId {matching.AttestWorkFlowRowId} code {signeeStatus.Code} ");
                            }
                        }

                    }
                }
            }

            return new ActionResult(true) { InfoMessage = sb.ToString() };
        }

        public ActionResult SaveDocumentSigningAnswer(int attestWorkFlowRowId, Common.Util.SigneeStatus signeeStatus, string comment, int userId, int actorCompanyId, int licenseId, string base64Data = null)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveDocumentSigningAnswer(transaction, entities, attestWorkFlowRowId, signeeStatus, comment, userId, actorCompanyId, licenseId, base64Data);
                        if (result.Success)
                            transaction.Complete();
                        else
                        {
                            LogError(result.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);
                }

                return result;
            }
        }

        public ActionResult SaveDocumentSigningAnswer(TransactionScope transaction, CompEntities entities, int attestWorkFlowRowId, SigneeStatus signeeStatus, string comment, int userId, int actorCompanyId, int licenseId, string base64Data = null)
        {
            ActionResult result = new ActionResult(true);

            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
            //bool receiverIsFinal = false;
            int dataStorageRecordId = 0;
            bool answer = (signeeStatus == SigneeStatus.Signed);

            // Get current user
            User currentUser = UserManager.GetUser(entities, userId);

            string adminInformation = null;

            #region AttestWorkFlowRow

            AttestWorkFlowRow attestWorkFlowRow = GetAttestWorkFlowRow(entities, attestWorkFlowRowId, true, true);
            if (attestWorkFlowRow == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestWorkFlowRow");

            if (actorCompanyId == 0)
                actorCompanyId = attestWorkFlowRow.AttestWorkFlowHead.ActorCompanyId;

            DateTime answerTimeStamp = DateTime.Now;

            attestWorkFlowRow.AnswerDate = answerTimeStamp;
            SetModifiedProperties(attestWorkFlowRow);

            if (signeeStatus == SigneeStatus.Opened)
            {
                // Document only opened, no answer yet
                // Just update AnswerDate on the row until an answer returns
            }
            else
            {
                // Update row with answer
                attestWorkFlowRow.Answer = answer;
                attestWorkFlowRow.AnswerDate = answerTimeStamp;
                if (!string.IsNullOrEmpty(attestWorkFlowRow.AnswerText))
                    attestWorkFlowRow.AnswerText += "\n";
                attestWorkFlowRow.AnswerText += string.Format(answer ? GetText(12134, "Signerades av {0}") : GetText(12135, "Avslogs av {0}"), currentUser.Name);

                // Add user comment
                if (!string.IsNullOrEmpty(comment))
                {
                    // Keep any existing comments
                    if (!string.IsNullOrEmpty(attestWorkFlowRow.Comment))
                        attestWorkFlowRow.Comment += "\n";

                    attestWorkFlowRow.Comment = comment;
                    attestWorkFlowRow.CommentDate = answerTimeStamp;
                    attestWorkFlowRow.CommentUser = currentUser.Name;
                }

                attestWorkFlowRow.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.Processed;
                attestWorkFlowRow.State = (int)TermGroup_AttestFlowRowState.Handled;
            }

            result = SaveChanges(entities, transaction);
            if (!result.Success)
                return result;

            #endregion

            if (signeeStatus != SigneeStatus.Opened)
            {
                // Get head
                AttestWorkFlowHead attestWorkFlowHead = attestWorkFlowRow.AttestWorkFlowHead ?? GetAttestWorkFlowHead(entities, attestWorkFlowRow.AttestWorkFlowHeadId, false);
                adminInformation = attestWorkFlowHead.AdminInformation;

                // Get document
                dataStorageRecordId = attestWorkFlowHead.RecordId;
                DataStorageRecord dataStorageRecord = GeneralManager.GetDataStorageRecordOnly(entities, actorCompanyId, dataStorageRecordId);

                // Get current transition on row
                AttestTransition rowTransition = GetAttestTransition(entities, attestWorkFlowRow.AttestTransitionId);

                // Is attest state closed?
                List<int> closedAttestStateIds = GetClosedAttestStatesIds(entities, actorCompanyId, TermGroup_AttestEntity.SigningDocument);
                bool closedAttestState = closedAttestStateIds.Contains(rowTransition.AttestStateToId);

                // Get transitions from current state
                List<AttestTransition> nextLevelTransitions = GetAttestTransitionsFromState(entities, rowTransition.AttestStateToId);

                // Get all rows
                List<AttestWorkFlowRow> allRows = GetAttestWorkFlowRows(entities, attestWorkFlowHead.AttestWorkFlowHeadId, false);

                int attestWorkFlowRowType = attestWorkFlowRow.Type == null ? attestWorkFlowHead.Type : (int)attestWorkFlowRow.Type;

                string currentUsers = null;

                if (answer)
                {
                    #region Positive answer

                    bool okToMove = allRows.OkToMoveAttestFlowToNextLevel(attestWorkFlowRow.AttestTransitionId);

                    if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.Any || okToMove)
                    {
                        #region Move to next level

                        #region AttestWorkFlowRows on same level

                        if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.Any)
                        {
                            // Update rest of the (unhandled) rows at the same level to handled because type is 'Any'
                            foreach (AttestWorkFlowRow row in allRows.Where(r => r.AttestTransitionId == attestWorkFlowRow.AttestTransitionId && r.State == (int)TermGroup_AttestFlowRowState.Unhandled && r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered))
                            {
                                row.State = (int)TermGroup_AttestFlowRowState.Handled;
                                row.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.Processed;
                                SetModifiedProperties(row);
                            }
                        }

                        #endregion

                        if (!closedAttestStateIds.Contains(rowTransition.AttestStateToId))
                        {
                            #region Partly signed

                            #region XEMail to users in next level

                            foreach (AttestTransition at in nextLevelTransitions)
                            {
                                List<MessageRecipientDTO> nextRecipients = CreateRecipientListForNextAttestLevel(entities, allRows, at.AttestTransitionId, actorCompanyId);
                                if (nextRecipients.Any())
                                {
                                    AttestWorkFlowRow nextRow = allRows.FirstOrDefault(r => r.AttestTransitionId == at.AttestTransitionId && r.State == (int)TermGroup_AttestFlowRowState.Unhandled);
                                    if (nextRow != null)
                                    {
                                        foreach (MessageRecipientDTO r in nextRecipients)
                                        {
                                            r.ExternalId = nextRow.AttestWorkFlowRowId;
                                        }
                                    }

                                    //if (dataStorageRecord.Entity == (int)SoeEntityType.Employee &&
                                    //    at.AttestStateTo.Closed &&
                                    //    nextRecipients.Count == 1 &&
                                    //    EmployeeManager.GetEmployeeIdForUser(entities, nextRecipients.First().UserId, actorCompanyId) == dataStorageRecord.RecordId)
                                    //{
                                    //    receiverIsFinal = true;
                                    //}

                                    receivers.AddRange(nextRecipients);
                                }
                            }

                            if (receivers.Any())
                                currentUsers = string.Join(", ", receivers.Select(r => r.Name));

                            #endregion

                            #region AttestWorkFlowRows on next level

                            // Update process type on rows in next level
                            foreach (AttestWorkFlowRow row in allRows.Where(r => r.AttestTransition.AttestStateFromId == rowTransition.AttestStateToId))
                            {
                                row.ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess;
                                SetModifiedProperties(row);
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Fully signed

                            // Send XEMail to all users that has signed the document
                            List<int> userIds = GetUsersThatSignedDocument(allRows, currentUser.UserId);
                            // Include final user
                            userIds.Add(currentUser.UserId);

                            // Add users to recipient list
                            List<MessageRecipientDTO> answeredReceivers = AddUsersToRecipientList(entities, userIds);
                            if (answeredReceivers.Any())
                            {
                                result = SendDocumentSigningFullySignedMessage(transaction, entities, answeredReceivers, actorCompanyId, dataStorageRecordId, currentUser.ToDTO());
                                if (!result.Success)
                                    return result;
                            }

                            #endregion
                        }

                        #endregion
                    }
                    else if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.All && !okToMove)
                    {
                        #region Stay on same level

                        closedAttestState = false;

                        // Update document on who is left to sign
                        currentUsers = GetNextUsersToAttestString(entities, allRows, rowTransition.AttestTransitionId, actorCompanyId);

                        #endregion
                    }

                    #region DataStorage

                    if (!string.IsNullOrEmpty(base64Data))
                    {
                        // Update document
                        DataStorage dataStorage = GeneralManager.GetDataStorageByDataStorageRecordId(entities, dataStorageRecordId, actorCompanyId, false);
                        if (dataStorage != null)
                        {
                            GeneralManager.SetDataStorageData(dataStorage, base64Data);
                            SetModifiedProperties(dataStorage);
                        }
                    }

                    #endregion

                    #region DataStorageRecord

                    // Update attest state on document
                    dataStorageRecord.CurrentAttestUsers = currentUsers;
                    dataStorageRecord.AttestStateId = rowTransition.AttestStateToId;
                    dataStorageRecord.AttestStatus = (int)(closedAttestState ? TermGroup_DataStorageRecordAttestStatus.Signed : TermGroup_DataStorageRecordAttestStatus.PartlySigned);
                    SetModifiedProperties(dataStorageRecord);

                    #endregion

                    #endregion
                }
                else
                {
                    #region Negative answer

                    // Someone rejected, if process type is any and other users exists on same level, keep flow since someone else can sign
                    bool rejectAll = false;

                    #region AttestWorkFlowRows on same level

                    if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.All)
                    {
                        rejectAll = true;
                    }
                    else if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.Any)
                    {
                        if (!allRows.Any(r => r.AttestTransitionId == attestWorkFlowRow.AttestTransitionId && r.State == (int)TermGroup_AttestFlowRowState.Unhandled && r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered && r.AttestWorkFlowRowId != attestWorkFlowRowId))
                            rejectAll = true;
                    }

                    #endregion

                    #region DataStorageRecord

                    currentUsers = rejectAll ? string.Empty : GetNextUsersToAttestString(entities, allRows, rowTransition.AttestTransitionId, actorCompanyId);
                    dataStorageRecord.CurrentAttestUsers = currentUsers;
                    if (rejectAll)
                        dataStorageRecord.AttestStatus = (int)TermGroup_DataStorageRecordAttestStatus.Rejected;
                    SetModifiedProperties(dataStorageRecord);

                    #endregion

                    #region XEMail to concerned users

                    // Send XEMail to users already answered
                    List<int> userIds = GetUsersThatSignedDocument(allRows, currentUser.UserId);

                    if (attestWorkFlowRowType == (int)TermGroup_AttestWorkFlowType.All || string.IsNullOrEmpty(currentUsers))
                    {
                        // Flow rejected by only user on level and therefore stopped
                    }
                    else
                    {
                        // Also send XEMail to users on current transition (next to sign)
                        userIds.AddRange(GetUsersNextToSignDocument(allRows, currentUser.UserId));
                    }

                    // Add users to recipient list
                    List<MessageRecipientDTO> rejectReceivers = AddUsersToRecipientList(entities, userIds);
                    if (rejectReceivers.Any())
                    {
                        result = SendDocumentSigningRejectionMessage(transaction, entities, rejectReceivers, comment, actorCompanyId, dataStorageRecordId, currentUser.ToDTO());
                        if (!result.Success)
                            return result;
                    }

                    #endregion

                    #endregion
                }

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                #region Send to SoftOneId

                if (receivers.Any() && dataStorageRecordId != 0)
                {
                    string fileKey = GeneralManager.GetDataStorageExternalLink(entities, actorCompanyId, dataStorageRecordId);
                    result = SendDocumentSigningFlowToSoftOneId(transaction, entities, receivers, dataStorageRecordId, fileKey, adminInformation, userId, actorCompanyId, licenseId, true, true);
                }

                #endregion

                #region Send XEMail to the signee, just to confirm the answer

                MessageRecipientDTO receiver = new MessageRecipientDTO() { UserId = currentUser.UserId, Name = currentUser.Name };

                result = SendDocumentSigningConfirmationMessage(transaction, entities, receiver, answer, actorCompanyId, dataStorageRecordId, currentUser.ToDTO());
                if (!result.Success)
                    base.LogError(result.Exception, this.log);

                #endregion
            }

            return result;
        }

        public ActionResult CancelDocumentSigning(int attestWorkFlowHeadId, string comment, int userId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            // Get current user
            User currentUser = UserManager.GetUser(userId);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        List<AttestWorkFlowRow> allRows = GetAttestWorkFlowRows(entities, attestWorkFlowHeadId, false);
                        DateTime answerTimeStamp = DateTime.Now;

                        #endregion

                        #region AttestWorkFlowHead

                        AttestWorkFlowHead attestWorkFlowHead = GetAttestWorkFlowHead(entities, attestWorkFlowHeadId, true);

                        int currentTransitionId = attestWorkFlowHead.AttestWorkFlowRow.Where(r => r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess).Select(r => r.AttestTransitionId).FirstOrDefault();
                        if (currentTransitionId == 0)
                            currentTransitionId = attestWorkFlowHead.AttestWorkFlowRow.Where(r => r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Registered).Select(r => r.AttestTransitionId).FirstOrDefault();

                        // Add comment to initial comment
                        if (!string.IsNullOrEmpty(attestWorkFlowHead.AdminInformation))
                            attestWorkFlowHead.AdminInformation += "\n\n";
                        attestWorkFlowHead.AdminInformation += string.Format(GetText(12137, "Avbröts av {0}"), currentUser.Name) + "\n\n" + comment;
                        SetModifiedProperties(attestWorkFlowHead, currentUser);

                        #endregion

                        #region Document

                        DataStorageRecord dataStorageRecord = GeneralManager.GetDataStorageRecordOnly(entities, actorCompanyId, attestWorkFlowHead.RecordId);
                        dataStorageRecord.AttestStatus = (int)TermGroup_DataStorageRecordAttestStatus.Cancelled;
                        dataStorageRecord.CurrentAttestUsers = null;
                        SetModifiedProperties(dataStorageRecord, currentUser);

                        #endregion

                        #region AttestWorkFlowRow

                        // Create a new row
                        AttestWorkFlowRow attestWorkFlowRow = new AttestWorkFlowRow
                        {
                            AttestWorkFlowHeadId = attestWorkFlowHeadId,
                            AttestTransitionId = currentTransitionId,
                            UserId = currentUser.UserId,
                            Comment = comment,
                            CommentDate = answerTimeStamp,
                            CommentUser = currentUser.Name,
                            AnswerDate = answerTimeStamp,
                            AnswerText = string.Format(GetText(12137, "Avbröts av {0}"), currentUser.Name),
                            ProcessType = (int)TermGroup_AttestWorkFlowRowProcessType.Processed,
                            Type = (int)TermGroup_AttestWorkFlowType.Any,
                            State = (int)TermGroup_AttestFlowRowState.Handled
                        };
                        SetCreatedProperties(attestWorkFlowRow, currentUser);
                        attestWorkFlowHead.AttestWorkFlowRow.Add(attestWorkFlowRow);

                        #endregion

                        #region XEMail to concerned users

                        // Send XEMail to users already answered
                        List<int> userIds = GetUsersThatSignedDocument(allRows, currentUser.UserId);

                        // Also send XEMail to users on current transition (next to sign)
                        userIds.AddRange(GetUsersNextToSignDocument(allRows, currentUser.UserId));

                        // Add users to recipient list
                        List<MessageRecipientDTO> cancelReceivers = AddUsersToRecipientList(entities, userIds);
                        if (cancelReceivers.Any())
                        {
                            result = SendDocumentSigningCancellationMessage(transaction, entities, cancelReceivers, comment, actorCompanyId, attestWorkFlowHead.RecordId, currentUser.ToDTO());
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;
                        else
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SendDocumentSigningRequestMessage(TransactionScope transaction, CompEntities entities, List<MessageRecipientDTO> receivers, string adminInformation, string signPageLink, int actorCompanyId, int dataStorageRecordId, UserDTO user, bool isReminder = false, bool forceSendToReceiver = false, bool forceSendToEmailReceiver = false)
        {
            DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
            if (record == null || record.DataStorage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorageRecord");

            string docName = record.DataStorage.Description;
            string subject = (isReminder ? GetText(9042, "Påminnelse") + " " + GetText(12136, "Signera").ToLower() : GetText(12136, "Signera")) + ": " + docName;
            string employeeName = GetDocumentEmployeeName(entities, actorCompanyId, record);

            StringBuilder body = new StringBuilder();
            if (isReminder)
                body.Append(GetText(9042, "Påminnelse").ToUpper() + "<br/><br/>");

            body.Append(GetText(12133, "Dokument att signera") + "<br/><br/>");
            if (!string.IsNullOrEmpty(signPageLink))
                body.Append($"<a href=\"{signPageLink}\">{docName}</a>");
            else
                body.Append(docName);

            if (!string.IsNullOrEmpty(employeeName))
                body.Append(" " + string.Format(GetText(12140, "tillhörande {0}"), employeeName));
            body.Append(".");

            if (!string.IsNullOrEmpty(adminInformation))
                body.Append("<br/><br/>" + adminInformation);

            return SendDocumentSigningMessage(transaction, entities, actorCompanyId, dataStorageRecordId, user, TermGroup_MessageType.DocumentSigningRequest, subject, body.ToString(), GetText(12133, "Dokument att signera"), receivers, forceSendToReceiver, forceSendToEmailReceiver);
        }

        public ActionResult SendDocumentSigningConfirmationMessage(TransactionScope transaction, CompEntities entities, MessageRecipientDTO receiver, bool answer, int actorCompanyId, int dataStorageRecordId, UserDTO user)
        {
            DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
            if (record == null || record.DataStorage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorageRecord");

            string docName = record.DataStorage.Description;
            string subject = (answer ? GetText(12141, "Dokument signerat") : GetText(12142, "Dokument avslagit"));
            string employeeName = GetDocumentEmployeeName(entities, actorCompanyId, record);

            StringBuilder body = new StringBuilder();
            body.Append(string.Format((answer ? GetText(12138, "Du har nu signerat dokument {0}") : GetText(12139, "Du har nu avslagit dokument {0}")), docName));
            if (!string.IsNullOrEmpty(employeeName))
                body.Append(" " + string.Format(GetText(12140, "tillhörande {0}"), employeeName));
            body.Append(".");

            return SendDocumentSigningMessage(transaction, entities, actorCompanyId, dataStorageRecordId, user, TermGroup_MessageType.DocumentSigningConfirmation, subject, body.ToString(), subject, new List<MessageRecipientDTO>() { receiver }, true, true);
        }

        public ActionResult SendDocumentSigningRejectionMessage(TransactionScope transaction, CompEntities entities, List<MessageRecipientDTO> receivers, string comment, int actorCompanyId, int dataStorageRecordId, UserDTO user)
        {
            DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
            if (record == null || record.DataStorage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorageRecord");

            string docName = record.DataStorage.Description;
            string subject = GetText(12142, "Dokument avslagit");
            string employeeName = GetDocumentEmployeeName(entities, actorCompanyId, record);

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(12143, "{0} har avslagit dokument {1}"), user.Name, docName));
            if (!string.IsNullOrEmpty(employeeName))
                body.Append(" " + string.Format(GetText(12140, "tillhörande {0}"), employeeName));
            body.Append(".<br/><br/>");
            body.Append(GetText(1436, "Kommentar") + ": " + comment);

            return SendDocumentSigningMessage(transaction, entities, actorCompanyId, dataStorageRecordId, user, TermGroup_MessageType.DocumentSigningRejection, subject, body.ToString(), subject, receivers, true, true);
        }

        public ActionResult SendDocumentSigningCancellationMessage(TransactionScope transaction, CompEntities entities, List<MessageRecipientDTO> receivers, string comment, int actorCompanyId, int dataStorageRecordId, UserDTO user)
        {
            DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
            if (record == null || record.DataStorage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorageRecord");

            string docName = record.DataStorage.Description;
            string subject = GetText(12145, "Dokumentsignering avbruten");
            string employeeName = GetDocumentEmployeeName(entities, actorCompanyId, record);

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(12144, "{0} har avbrutit signeringsflödet för dokument {1}"), user.Name, docName));
            if (!string.IsNullOrEmpty(employeeName))
                body.Append(" " + string.Format(GetText(12140, "tillhörande {0}"), employeeName));
            body.Append(".<br/><br/>");
            body.Append(GetText(1436, "Kommentar") + ": " + comment);

            return SendDocumentSigningMessage(transaction, entities, actorCompanyId, dataStorageRecordId, user, TermGroup_MessageType.DocumentSigningCancellation, subject, body.ToString(), subject, receivers, true, true);
        }

        public ActionResult SendDocumentSigningFullySignedMessage(TransactionScope transaction, CompEntities entities, List<MessageRecipientDTO> receivers, int actorCompanyId, int dataStorageRecordId, UserDTO user)
        {
            DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
            if (record == null || record.DataStorage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataStorageRecord");

            string docName = record.DataStorage.Description;
            string subject = GetText(12146, "Dokument fullständigt signerat");
            string employeeName = GetDocumentEmployeeName(entities, actorCompanyId, record);

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(12147, "Dokument {0} fullständigt signerat"), docName));
            if (!string.IsNullOrEmpty(employeeName))
                body.Append(" " + string.Format(GetText(12140, "tillhörande {0}"), employeeName));
            body.Append(".");

            return SendDocumentSigningMessage(transaction, entities, actorCompanyId, dataStorageRecordId, user, TermGroup_MessageType.DocumentSigningFullySigned, subject, body.ToString(), subject, receivers, true, true);
        }

        public ActionResult SendDocumentSigningMessage(TransactionScope transaction, CompEntities entities, int actorCompanyId, int dataStorageRecordId, UserDTO user, TermGroup_MessageType messageType, string subject, string body, string senderName, List<MessageRecipientDTO> receiversList, bool forceSendToReceiver = false, bool forceSendToEmailReciever = false)
        {
            MessageEditDTO dto = new MessageEditDTO
            {
                ActorCompanyId = actorCompanyId,
                LicenseId = user.LicenseId,
                Entity = SoeEntityType.Employee,
                RecordId = dataStorageRecordId,
                Created = DateTime.Now,
                SentDate = DateTime.Now,
                SenderUserId = user.UserId,
                SenderName = senderName,
                Subject = subject,
                Text = body,
                ShortText = body.Replace("<br/>", "\r\n"),
                AnswerType = XEMailAnswerType.None,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageType = messageType,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.HTML,
                ForceSendToReceiver = forceSendToReceiver,
                ForceSendToEmailReceiver = forceSendToEmailReciever,
            };

            string defaultEmail = this.SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, actorCompanyId, 0);
            if (defaultEmail != null && defaultEmail.Trim() != string.Empty)
                dto.SenderEmail = defaultEmail;
            else if (user.Email != null)
                dto.SenderEmail = user.Email;

            dto.Recievers = receiversList;

            return CommunicationManager.SendXEMail(transaction, entities, dto, actorCompanyId, 0, user.UserId, false);
        }

        private List<int> GetUsersThatSignedDocument(List<AttestWorkFlowRow> allRows, int currentUserId)
        {
            return (from AttestWorkFlowRow row in allRows.Where(r => r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Processed &&
                    r.State == (int)TermGroup_AttestFlowRowState.Handled &&
                    r.UserId.HasValue && r.UserId.Value != currentUserId &&
                    r.Answer.HasValue)
                    select row.UserId.Value).ToList();
        }

        private List<int> GetUsersNextToSignDocument(List<AttestWorkFlowRow> allRows, int currentUserId)
        {
            return (from AttestWorkFlowRow row in allRows.Where(r => r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess &&
                    r.State == (int)TermGroup_AttestFlowRowState.Unhandled &&
                    r.UserId.HasValue && r.UserId.Value != currentUserId &&
                    !r.Answer.HasValue)
                    select row.UserId.Value).ToList();
        }

        private List<MessageRecipientDTO> AddUsersToRecipientList(CompEntities entities, List<int> userIds, bool applyReplacers = false)
        {
            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
            var replacements = GetReplacers(entities, userIds);
            foreach (int userId in userIds.Distinct())
            {
                User user = UserManager.GetUser(entities, userId);

                if (user == null)
                    continue;

                if (replacements.ContainsKey(userId) && applyReplacers)
                    receivers.Add(replacements[userId]);
                else
                    receivers.Add(new MessageRecipientDTO() { UserId = userId, Name = user.Name });
            }

            return receivers;
        }

        private Dictionary<int, MessageRecipientDTO> GetReplacers(CompEntities entities, List<int> userIds)
        {
            return entities.UserReplacement.Where(r => userIds.Contains(r.OriginUserId) &&
                    r.State == (int)SoeEntityState.Active && r.Type == (int)UserReplacementType.AttestFlow &&
                    (r.StartDate == null || r.StartDate <= DateTime.Today) && (r.StopDate == null || r.StopDate >= DateTime.Today))
                .Select(r => new
                {
                    r.OriginUserId,
                    r.ReplacementUserId,
                    r.ReplacementUser.Name,
                })
                .ToDictionary(
                    r => r.OriginUserId,
                    r => new MessageRecipientDTO() { UserId = r.ReplacementUserId, Name = r.Name });
        }

        private string GetDocumentEmployeeName(CompEntities entities, int actorCompanyId, DataStorageRecord record)
        {
            string employeeName = null;
            if (record.Entity == (int)SoeEntityType.Employee)
            {
                Employee employee = EmployeeManager.GetEmployee(entities, record.RecordId, actorCompanyId, loadContactPerson: true);
                if (employee != null)
                    employeeName = employee.Name;
            }

            return employeeName;
        }

        public List<AttestWorkFlowRowDTO> GetDocumentSigningStatus(SoeEntityType entity, int recordId, int userId, int actorCompanyId)
        {
            // Get head
            AttestWorkFlowHead attestWorkFlowHead = GetAttestWorkFlowHead(entity, recordId);
            if (attestWorkFlowHead == null)
                return new List<AttestWorkFlowRowDTO>();

            // Get rows
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AttestWorkFlowRow.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AttestWorkFlowRow> rows = GetAttestWorkFlowRows(entitiesReadOnly, attestWorkFlowHead.AttestWorkFlowHeadId, true);

            // Get document
            entitiesReadOnly.DataStorageRecord.NoTracking();
            DataStorageRecord dataStorageRecord = GeneralManager.GetDataStorageRecordOnly(entitiesReadOnly, actorCompanyId, attestWorkFlowHead.RecordId);

            List<GenericType> processTypes = base.GetTermGroupContent(TermGroup.AttestWorkFlowRowProcessType);
            List<GenericType> types = base.GetTermGroupContent(TermGroup.AttestWorkFlowType);

            // Sort rows
            List<AttestWorkFlowRow> sortedRows = new List<AttestWorkFlowRow>();
            foreach (AttestWorkFlowRow row in rows)
            {
                if (row.OriginateFromRowId.HasValue)
                {
                    int index = sortedRows.FindIndex(r => r.AttestWorkFlowRowId == row.OriginateFromRowId);
                    sortedRows.Insert(index + 1, row);
                }
                else
                {
                    sortedRows.Add(row);
                }
            }

            // Convert to DTO
            List<AttestWorkFlowRowDTO> dtos = sortedRows.ToDTOs(true).ToList();

            // Set some extensions
            foreach (AttestWorkFlowRowDTO dto in dtos)
            {
                // Current user
                dto.IsCurrentUser = (dto.UserId.HasValue && dto.UserId.Value == userId);

                // No attest state name on first (Registered) row
                if (dto.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered)
                {
                    dto.AttestStateToName = String.Empty;
                    dto.IsCurrentUser = false; // Will prevent row to be bold
                }

                // ProcessTypeName
                dto.ProcessTypeName = processTypes?.FirstOrDefault(t => t.Id == (int)dto.ProcessType)?.Name ?? string.Empty;

                // TypeName
                dto.TypeName = dto.Type != null ? types.FirstOrDefault(t => t.Id == (int)dto.Type)?.Name ?? string.Empty : string.Empty;

                // Sort on process type
                switch (dto.ProcessType)
                {
                    case TermGroup_AttestWorkFlowRowProcessType.Registered:
                        dto.ProcessTypeSort = 1;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess:
                        if (dto.State == TermGroup_AttestFlowRowState.Deleted)
                            dto.ProcessTypeSort = 2;
                        else
                            dto.ProcessTypeSort = dto.OriginateFromRowId.HasValue ? 7 : 8;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Processed:
                        dto.ProcessTypeSort = 6;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.LevelNotReached:
                        dto.ProcessTypeSort = 9;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredToOtherUser:
                        dto.ProcessTypeSort = 3;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.TransferredWithReturn:
                        dto.ProcessTypeSort = 4;
                        break;
                    case TermGroup_AttestWorkFlowRowProcessType.Returned:
                        dto.ProcessTypeSort = 5;
                        break;
                }
            }

            return dtos;
        }

        #endregion
    }

    public class GenerateTimeEmployeeTreeInput
    {
        public DateTime StartDate { get; private set; }
        public DateTime StopDate { get; private set; }
        public TimePeriod TimePeriod { get; private set; }
        public List<Employee> Employees { get; private set; }
        public List<Employee> EndedEmployees { get; private set; }
        public EmployeeAuthModel EmployeeAuthModel { get; private set; }
        public List<EmployeeGroup> EmployeeGroups { get; private set; }
        public List<PayrollGroup> PayrollGroups { get; private set; }
        public List<AttestState> AttestStates { get; private set; }
        public AttestState HighestAttestState { get; private set; }
        public List<TimePayrollTransactionTreeDTO> TransactionsItems { get; private set; }

        public GenerateTimeEmployeeTreeInput(DateTime startDate, DateTime stopDate, TimePeriod timePeriod, List<Employee> employees, List<Employee> endedEmployees, EmployeeAuthModel employeeAuthModel, List<EmployeeGroup> employeeGroups, List<PayrollGroup> payrollGroups, List<AttestState> attestStates, AttestState highestAttestState, List<TimePayrollTransactionTreeDTO> transactionsItems)
        {
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.TimePeriod = timePeriod;
            this.Employees = employees;
            this.EndedEmployees = endedEmployees;
            this.EmployeeAuthModel = employeeAuthModel;
            this.EmployeeGroups = employeeGroups;
            this.PayrollGroups = payrollGroups;
            this.AttestStates = attestStates;
            this.HighestAttestState = highestAttestState;
            this.TransactionsItems = transactionsItems;
        }
    }
}
