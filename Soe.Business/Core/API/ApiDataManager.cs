using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ApiDataManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ApiDataManager(ParameterObject parameterObject) : base(parameterObject)
        {

        }

        #endregion

        #region Core

        private readonly Dictionary<TermGroup, Dictionary<int, string>> apiTerms = new Dictionary<TermGroup, Dictionary<int, string>>();
        public string GetApiTerm(TermGroup termGroup, int id)
        {
            if (!apiTerms.ContainsKey(termGroup))
                apiTerms.Add(termGroup, GetTermGroupContent(termGroup).ToDictionary(k => k.Id, v => v.Name));
            return apiTerms[termGroup].ContainsKey(id) ? apiTerms[termGroup][id] : string.Empty;
        }

        #endregion

        #region ApiMessage

        public List<ApiMessageGridDTO> GetApiMessagesForGrid(TermGroup_ApiMessageType type, TermGroup_ApiMessageSourceType source, DateTime? fromDate, DateTime? toDate, bool showVerified, bool showOnlyErrors)
        {
            List<ApiMessageGridDTO> dtos = new List<ApiMessageGridDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ApiMessage.NoTracking();
            var query = (from a in entities.ApiMessage
                         where a.ActorCompanyId == ActorCompanyId &&
                         a.Type == (int)type
                         select a);

            if (!showVerified)
                query = query.Where(a => a.Status != (int)TermGroup_ApiMessageStatus.Verified);
            if (fromDate.HasValue)
            {
                fromDate = CalendarUtility.GetBeginningOfDay(fromDate.Value);
                query = query.Where(a => a.Created >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                toDate = CalendarUtility.GetEndOfDay(toDate.Value);
                query = query.Where(a => a.Created <= toDate.Value);
            }

            var apiMessages = (from t in query
                               select new
                               {
                                   t.ApiMessageId,
                                   t.Type,
                                   t.EntityType,
                                   t.SourceType,
                                   t.Status,
                                   t.Comment,
                                   t.ValidationMessage,
                                   t.RecordCount,
                                   t.Created,
                                   t.Modified,
                                   Changes = t.ApiMessageChange.ToList(),
                               }).ToList();

            if (apiMessages.IsNullOrEmpty())
                return dtos;

            if (apiMessages.Any())
            {
                FilterSource();
                Dictionary<int, string> records = LoadRecords();
                Dictionary<Feature, bool> readPermissions = new Dictionary<Feature, bool>();

                foreach (var apiMessage in apiMessages.OrderByDescending(c => c.Created))
                {
                    var message = new ApiMessageGridDTO()
                    {
                        ApiMessageId = apiMessage.ApiMessageId,
                        Status = (TermGroup_ApiMessageStatus)apiMessage.Status,
                        StatusName = GetApiTerm(TermGroup.ApiMessageStatus, apiMessage.Status),
                        TypeName = GetApiTerm(TermGroup.ApiMessageType, apiMessage.Type),
                        SourceTypeName = GetApiTerm(TermGroup.ApiMessageSourceType, apiMessage.SourceType),
                        RecordCount = apiMessage.RecordCount,
                        Comment = apiMessage.Comment,
                        ValidationMessage = apiMessage.ValidationMessage,
                        HasFile = true,
                        Created = apiMessage.Created,
                        Modified = apiMessage.Modified,
                        Changes = new List<ApiMessageChangeGridDTO>()
                    };

                    if (!apiMessage.Changes.IsNullOrEmpty())
                    {
                        foreach (var changesByRecord in apiMessage.Changes.Where(i => i.RecordId.HasValue).GroupBy(i => i.RecordId.Value))
                        {
                            var (recordName, hasPermissionToRecord) = GetRecord(apiMessage.EntityType, changesByRecord.Key);

                            foreach (var apiMessageChange in changesByRecord.OrderBy(i => i.ApiMessageChangeId))
                            {
                                bool hasPermissionToChange = hasPermissionToRecord && HasPermissionToChange(apiMessageChange.Type, apiMessageChange.FieldType);

                                ApiMessageChangeGridDTO change = new ApiMessageChangeGridDTO
                                {
                                    TypeName = GetApiTerm(TermGroup.ApiMessageChangeType, apiMessageChange.Type),
                                    FieldTypeName = GetFieldTypeName(apiMessage.EntityType, apiMessageChange.Type, apiMessageChange.FieldType),
                                    RecordName = hasPermissionToRecord ? recordName : ApiMessageChangeGridDTO.NOPERMISSIONINDICATOR,
                                    Identifier = hasPermissionToRecord ? apiMessageChange.Identifier : ApiMessageChangeGridDTO.NOPERMISSIONINDICATOR,
                                    FromValue = hasPermissionToChange ? GetFromToValueText(apiMessageChange.FromValue, apiMessageChange.FromValueName) : "*",
                                    ToValue = hasPermissionToChange ? GetFromToValueText(apiMessageChange.ToValue, apiMessageChange.ToValueName) : "*",
                                    FromDate = apiMessageChange.FromDate,
                                    ToDate = apiMessageChange.ToDate,
                                    Error = apiMessageChange.Error,
                                    HasError = !string.IsNullOrEmpty(apiMessageChange.Error) && apiMessageChange.FieldType != EmployeeUserImport.NOTHING_UPDATED,
                                };

                                if (!apiMessageChange.FromValue.IsNullOrEmpty() && apiMessageChange.FromValue == apiMessageChange.ToValue)
                                    change.ToValue = $"{change.ToValue} ({GetText(92022, "Datumförändring")})";

                                if (DoShowApiMessageChange(change, showOnlyErrors))
                                    message.Changes.Add(change);
                            }
                        }
                    }

                    List<string> identifiers = message.Changes.Select(i => i.Identifier).Distinct().ToList();
                    message.Identifiers = identifiers.ToCommaSeparated();
                    if (identifiers.Count == 1 && message.Changes.First().RecordName != ApiMessageChangeGridDTO.NOPERMISSIONINDICATOR)
                        message.Identifiers = $"{identifiers.ToCommaSeparated()} {message.Changes.First().RecordName}";
                    if (!message.Changes.IsNullOrEmpty() && message.Changes.Any(i => i.HasError))
                        message.HasError = true;

                    if (DoShowApiMessage(message, showOnlyErrors))
                        dtos.Add(message);
                }

                void FilterSource()
                {
                    if (apiMessages.IsNullOrEmpty())
                        return;

                    switch (source)
                    {
                        case TermGroup_ApiMessageSourceType.API:
                        case TermGroup_ApiMessageSourceType.APIManual:
                        case TermGroup_ApiMessageSourceType.APIManualOnlyLogging:
                        case TermGroup_ApiMessageSourceType.AllAPI:
                            apiMessages = apiMessages.Where(a => a.SourceType == (int)TermGroup_ApiMessageSourceType.API ||
                                                                 a.SourceType == (int)TermGroup_ApiMessageSourceType.APIManual ||
                                                                 a.SourceType == (int)TermGroup_ApiMessageSourceType.APIManualOnlyLogging).ToList();
                            break;
                        case TermGroup_ApiMessageSourceType.UnitTest:
                            apiMessages = apiMessages.Where(a => a.SourceType == (int)TermGroup_ApiMessageSourceType.UnitTest).ToList();
                            break;
                        case TermGroup_ApiMessageSourceType.MassUpdateEmployeeFields:
                            apiMessages = apiMessages.Where(a => a.SourceType == (int)TermGroup_ApiMessageSourceType.MassUpdateEmployeeFields).ToList();
                            break;
                    }
                }
                Dictionary<int, string> LoadRecords()
                {
                    List<int> recordIds = GetRecordIds();
                    if (apiMessages.Any(e => e.EntityType == (int)SoeEntityType.Employee))
                    {
                        List<int> filterIds = EmployeeManager.GetEmployeeIdsForQuery(recordIds);
                        return EmployeeManager.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, dateTo: DateTime.Today.AddYears(1), employeeFilter: filterIds, active: null, loadEmployment: false, includeEnded: true).ToDictionary(k => k.EmployeeId, v => v.Name);

                    }
                    return null;
                }
                List<int> GetRecordIds()
                {
                    return apiMessages.SelectMany(a => a.Changes.Where(c => c.RecordId.HasValue).Select(c => c.RecordId.Value)).Distinct().ToList();
                }
                (string recordName, bool hasPermissionToRecord) GetRecord(int entityType, int recordId)
                {
                    string recordName = string.Empty;
                    bool hasPermissionToRecord = false;
                    switch (entityType)
                    {
                        case (int)SoeEntityType.Employee:
                            records.TryGetValue(recordId, out recordName);
                            hasPermissionToRecord = recordName != null || recordId == 0;
                            break;
                    }
                    return (recordName, hasPermissionToRecord);
                }
                bool HasPermissionToChange(int changeType, int fieldType)
                {
                    switch (changeType)
                    {
                        case (int)TermGroup_ApiMessageChangeType.Employee:
                            switch (fieldType)
                            {
                                case (int)EmployeeChangeType.EmploymentPriceType:
                                    return HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
                            }
                            break;

                    }
                    return true;
                }
                bool HasReadPermission(Feature feature)
                {
                    if (!readPermissions.ContainsKey(feature))
                        readPermissions.Add(feature, FeatureManager.HasRolePermission(feature, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId));
                    return readPermissions[feature];
                }
            }

            return dtos;
        }

        public List<ApiMessage> GetApiMessages(int actorCompanyId, TermGroup_ApiMessageType type, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ApiMessage.NoTracking();
            List<ApiMessage> apiMessages = (from t in entities.ApiMessage
                                                .Include("ApiMessageChange")
                                            where t.ActorCompanyId == actorCompanyId &&
                                            t.Type == (int)type &&
                                            (!dateFrom.HasValue || t.Created >= dateFrom.Value) &&
                                            (!dateTo.HasValue || t.Created <= dateTo.Value)
                                            select t).ToList();

            return apiMessages.OrderByDescending(c => c.ApiMessageId).ToList();
        }

        public ApiMessage GetApiMessage(int apiMessageId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ApiMessage.NoTracking();
            return GetApiMessage(entities, apiMessageId);
        }

        public ApiMessage GetApiMessage(CompEntities entities, int apiMessageId)
        {
            return entities.ApiMessage.FirstOrDefault(api => api.ApiMessageId == apiMessageId && api.ActorCompanyId == ActorCompanyId);
        }

        private string GetFromToValueText(string value, string valueName)
        {
            // As default use value directly. Special treatment below.
            string text;
            if (valueName.IsNullOrEmpty() || valueName.Equals(value))
                text = value;
            else if (!string.IsNullOrEmpty(value))
                text = $"{valueName} ({value})";
            else
                text = valueName;

            return text;
        }

        private string GetFieldTypeName(int entityType, int changeType, int changeFieldType)
        {
            string name = "";

            if (entityType == (int)SoeEntityType.Employee)
            {
                if (changeType == (int)TermGroup_ApiMessageChangeType.Employee)
                    name = GetApiTerm(TermGroup.EmployeeChangeFieldType, changeFieldType);
                else if (changeType == (int)TermGroup_ApiMessageChangeType.Employment)
                    name = GetApiTerm(TermGroup.EmploymentChangeFieldType, changeFieldType);
            }

            return name;
        }

        private bool DoShowApiMessageChange(ApiMessageChangeGridDTO dto, bool showOnlyErrors)
        {
            if (dto == null)
                return false;
            if (showOnlyErrors && !dto.HasError)
                return false;
            return true;
        }

        private bool DoShowApiMessage(ApiMessageGridDTO dto, bool showOnlyErrors)
        {
            if (dto == null)
                return false;
            if (showOnlyErrors && !dto.HasError)
                return false;
            return true;
        }

        public ActionResult SetApiMessageAsVerified(List<int> apiMessageIds)
        {
            ActionResult result = new ActionResult(true);

            if (!apiMessageIds.IsNullOrEmpty())
            {
                using (CompEntities entities = new CompEntities())
                {
                    List<int> modifiedApiMessageIds = new List<int>();
                    foreach (int apiMessageId in apiMessageIds)
                    {
                        ApiMessage apiMessage = ApiDataManager.GetApiMessage(entities, apiMessageId);
                        if (apiMessage == null)
                            continue;
                        if (apiMessage.Status != (int)TermGroup_ApiMessageStatus.Processed && apiMessage.Status != (int)TermGroup_ApiMessageStatus.Error)
                            continue;

                        //Dont set modified, that is used as received date
                        apiMessage.Status = (int)TermGroup_ApiMessageStatus.Verified;
                        modifiedApiMessageIds.Add(apiMessage.ApiMessageId);
                    }

                    if (modifiedApiMessageIds.Any())
                        result = SaveChanges(entities);
                }
            }

            return result;
        }

        #endregion

        #region ApiSetting

        public List<ApiSettingDTO> GetApiSettingsForGrid()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysTermDTO> terms = base.GetSystermsWithDescriptionFromCache(sysEntitiesReadOnly, CacheConfig.System(), (int)TermGroup.ApiSettingType, GetLangId());
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<ApiSettingDTO> settings = GetApiSettings(entitiesReadOnly).ToDTOs().ToList();

            foreach (ApiSettingDTO setting in settings)
            {
                setting.SetNameAndDescription(terms.FirstOrDefault(t => t.SysTermId == (int)setting.Type));
                setting.SetDataType();
            }

            settings.CreateMissing(terms);

            return settings.OrderBy(s => s.Type).ToList();
        }

        public List<ApiSetting> GetApiSettings(CompEntities entities)
        {
            int actorCompanyId = base.ActorCompanyId;

            List<ApiSetting> settings = (from s in entities.ApiSetting
                                         where s.ActorCompanyId == actorCompanyId &&
                                         s.State == (int)SoeEntityState.Active
                                         select s).ToList();

            return settings.OrderBy(s => s.Type).ToList();
        }

        public ActionResult SaveApiSettings(List<ApiSettingDTO> settingsInput)
        {
            ActionResult result = new ActionResult();

            if (settingsInput.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        List<ApiSetting> settings = GetApiSettings(entities);
                        foreach (var settingInputByType in settingsInput.GroupBy(s => s.Type))
                        {
                            List<ApiSetting> settingsByType = settings.Where(s => s.Type == (int)settingInputByType.Key).ToList();

                            foreach (ApiSettingDTO settingInput in settingInputByType)
                            {
                                settingInput.SetValue();

                                ApiSetting setting = settingsByType.FirstOrDefault(i => i.ApiSettingId == settingInput.ApiSettingId);
                                if (setting == null && settingInput.Value.IsNullOrEmpty())
                                    continue;

                                if (setting == null)
                                {
                                    setting = new ApiSetting()
                                    {
                                        ActorCompanyId = base.ActorCompanyId,
                                        Type = (int)settingInput.Type,
                                    };
                                    entities.ApiSetting.AddObject(setting);

                                    SetValues();
                                    SetCreatedProperties(setting);
                                }
                                else if (setting.IsModified(settingInput))
                                {
                                    SetValues();
                                    SetModifiedProperties(setting);
                                }

                                if (setting.ApiSettingId > 0)
                                    settingsByType.Remove(setting);

                                void SetValues()
                                {
                                    setting.Value = settingInput.Value.NullToEmpty();
                                    setting.StartDate = settingInput.StartDate;
                                    setting.StopDate = settingInput.StopDate;
                                }
                            }
                        }

                        result = SaveChanges(entities);
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
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion
    }
}
