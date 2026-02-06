using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TrackChangesManager : ManagerBase
    {
        #region Constructor

        public TrackChangesManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Track changes

        #region Cache

        private readonly Dictionary<int, string> attestRoleCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> companyCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> employeeCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> roleCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> supplierCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> userCache = new Dictionary<int, string>();

        #endregion

        #region Repository

        public EmployeeUserChangesRepositoryDTO CreateEmployeeUserChangesRepository(int actorComapnyId, Guid batch, TermGroup_TrackChangesActionMethod actionMethod, SoeEntityType topEntity, EmployeeUserApplyFeaturesResult applyFeaturesResult = null)
        {
            return new EmployeeUserChangesRepositoryDTO(actorComapnyId, batch, actionMethod, topEntity, applyFeaturesResult);
        }

        public ActionResult SaveEmployeeUserChanges(CompEntities entities, TransactionScope transaction, EmployeeUserChangesRepositoryDTO changeRepository)
        {
            ActionResult result = new ActionResult(true);

            List<TrackChangesDTO> trackChangesItems = changeRepository?.GetChanges();
            if (!trackChangesItems.IsNullOrEmpty())
            {
                result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);
                if (result.Success)
                {
                    Employee employee = changeRepository?.employeeBefore;
                    if (employee != null && employee.UserId.HasValue && employee.User != null && employee.User.State == (int)SoeEntityState.Active)
                    {
                        string subject = GetText(11748, "Ändringar har skett på dina uppgifter");
                        string text = GetText(11749, "Tillägg eller justering har skett på dina personliga uppgifter. För att kontrollera ändringarna gå in i och kontrollera dina uppgifter under Anställd. Detta är ett automatiskt meddelande.");

                        #region Create message

                        List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>() { new MessageRecipientDTO()
                        {
                            UserId = employee.User.UserId,
                            SendCopyAsEmail = true,
                            UserName = employee.User.LoginName,
                            Name = employee.User.Name,
                            EmailAddress = employee.User.Email,
                            Type = XEMailRecipientType.User,
                        }};

                        string senderName = "";
                        if (parameterObject.SoeUser != null)
                            senderName = !string.IsNullOrEmpty(parameterObject.SoeUser.Name) ? parameterObject.SoeUser.Name : parameterObject.SoeUser.LoginName;
                        else
                            senderName = UserManager.GetUser(entities, parameterObject.UserId)?.Name ?? string.Empty;

                        MessageEditDTO message = new MessageEditDTO()
                        {
                            LicenseId = employee.User.LicenseId,
                            MessagePriority = TermGroup_MessagePriority.Normal,
                            MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                            MessageTextType = TermGroup_MessageTextType.Text,
                            MessageType = TermGroup_MessageType.UserInitiated,
                            Recievers = receivers,
                            ForceSendToReceiver = true,
                            RoleId = parameterObject.RoleId.ToNullable(),
                            MarkAsOutgoing = false,
                            SenderName = senderName,
                            SenderEmail = string.Empty,
                            Subject = subject,
                            Text = text,
                            ShortText = text,
                            ParentId = null,
                            AnswerType = XEMailAnswerType.None,
                            Entity = 0,
                            RecordId = 0,
                            ActorCompanyId = parameterObject.ActorCompanyId,
                            SenderUserId = parameterObject.UserId,
                            ForceSendToEmailReceiver = true,
                        };

                        #endregion

                        #region Send message

                        result = CommunicationManager.SendXEMail(transaction, entities, message, parameterObject.ActorCompanyId, parameterObject.RoleId, parameterObject.UserId);

                        #endregion
                    }
                }
            }

            return result;
        }

        #endregion

        #region Init

        public TrackChangesDTO InitTrackChanges(CompEntities entities, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod, TermGroup_TrackChangesAction action, SoeEntityType topEntity, int topRecordId, SoeEntityType entity, int recordId, SettingDataType dataType, string columnName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, SoeEntityType parentEntity = SoeEntityType.None, int? parentRecordId = null, string fromValue = null, string toValue = null, string fromValueName = null, string toValueName = null)
        {
            return new TrackChangesDTO()
            {
                ActorCompanyId = actorCompanyId,
                Batch = Guid.NewGuid(),
                ActionMethod = actionMethod,
                TopEntity = topEntity,
                TopRecordId = topRecordId,
                Entity = entity,
                RecordId = recordId,
                ColumnName = columnName,
                ColumnType = columnType,
                ParentEntity = parentEntity,
                ParentRecordId = parentRecordId,
                Action = action,
                DataType = dataType,
                FromValue = fromValue,
                FromValueName = fromValueName,
                ToValue = toValue,
                ToValueName = toValueName,
                Role = parameterObject != null ? RoleManager.GetRoleName(entities, parameterObject.RoleId) : "System"
            };
        }

        public TrackChangesDTO InitTrackChanges(CompEntities entities, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod, TermGroup_TrackChangesAction action, SoeEntityType topEntity, int topRecordId, SoeEntityType entity, int recordId, SettingDataType dataType, string columnName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, string fromValue = null, string toValue = null, string fromValueName = null, string toValueName = null)
        {
            return InitTrackChanges(entities, actorCompanyId, actionMethod, action, topEntity, topRecordId, entity, recordId, dataType, columnName, columnType, SoeEntityType.None, null, fromValue, toValue, fromValueName, toValueName);
        }

        public TrackChangesDTO InitTrackChanges(CompEntities entities, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod, TermGroup_TrackChangesAction action, SoeEntityType topEntity, int topRecordId, SoeEntityType entity, int recordId = 0, SoeEntityType parentEntity = SoeEntityType.None, int? parentRecordId = null)
        {
            TrackChangesDTO dto = InitTrackChanges(entities, actorCompanyId, actionMethod, action, topEntity, topRecordId, entity, recordId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Unspecified, parentEntity, parentRecordId, null, null);
            dto.RecordId = recordId;
            return dto;
        }

        #endregion

        #region Add

        public ActionResult AddTrackChanges(CompEntities entities, TransactionScope transaction, TrackChangesDTO trackChangesItem)
        {
            if (trackChangesItem == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TrackChanges");

            string roleName = parameterObject != null ? RoleManager.GetRoleName(entities, parameterObject.RoleId) : "System";
            string userDetails = GetUserDetails() ?? "System";

            TrackChanges trackChanges = CreateTrackChanges(trackChangesItem, roleName, userDetails);
            return AddEntityItem(entities, trackChanges, "TrackChanges", transaction);
        }

        public ActionResult AddTrackChanges(CompEntities entities, TransactionScope transaction, List<TrackChangesDTO> trackChangesItems)
        {
            if (trackChangesItems.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TrackChanges");

            string roleName = RoleManager.GetRoleName(entities, parameterObject.RoleId);
            string userDetails = GetUserDetails();
            DateTime created = DateTime.Now;
            Guid batch = Guid.NewGuid();

            foreach (TrackChangesDTO trackChangesItem in trackChangesItems)
            {
                TrackChanges trackChanges = CreateTrackChanges(trackChangesItem, roleName, userDetails, created, batch);
                entities.TrackChanges.AddObject(trackChanges);
            }

            return SaveChanges(entities, transaction);
        }

        public List<TrackChangesDTO> CreateTrackStringChanges(List<SmallGenericType> stringFields, int actorCompanyId, SoeEntityType entity, int? recordId, object masterObject, object inputObject, TermGroup_TrackChangesAction actionType, SoeEntityType parentEntity = SoeEntityType.None, int parentRecordId = 0)
        {
            var changes = new List<TrackChangesDTO>();
            var masterProps = masterObject?.GetType().GetProperties();
            var inputProps = inputObject?.GetType().GetProperties();
            foreach (var field in stringFields)
            {
                var currentValue = masterProps?.FirstOrDefault(x => x.Name == field.Name)?.GetValue(masterObject)?.ToString();
                var inputValue = inputProps?.FirstOrDefault(x => x.Name == field.Name)?.GetValue(inputObject)?.ToString();

                if (
                    (actionType == TermGroup_TrackChangesAction.Insert && !string.IsNullOrEmpty(inputValue)) ||
                    (actionType == TermGroup_TrackChangesAction.Delete && !string.IsNullOrEmpty(currentValue)) ||
                    (actionType == TermGroup_TrackChangesAction.Update && inputValue != currentValue)
                   )
                {
                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, entity, (TermGroup_TrackChangesColumnType)field.Id, recordId.GetValueOrDefault(), currentValue, inputValue, actionType, SettingDataType.String, parentEntity, parentRecordId));
                }
            }

            return changes;
        }

        public TrackChangesDTO CreateTrackChangesDTO(int actorCompanyId, SoeEntityType entity, TermGroup_TrackChangesColumnType column, int recordId, string fromValue, string toValue, TermGroup_TrackChangesAction action, SettingDataType dataType, SoeEntityType parentEntity = SoeEntityType.None, int parentRecordId = 0, string fromValueName = null, string toValueName = null)
        {

            return new TrackChangesDTO
            {
                ActorCompanyId = actorCompanyId,
                ColumnType = column,
                Entity = entity,
                RecordId = recordId,
                FromValue = action == TermGroup_TrackChangesAction.Insert ? "" : fromValue,
                ToValue = action == TermGroup_TrackChangesAction.Delete ? "" : toValue,
                FromValueName = action == TermGroup_TrackChangesAction.Insert ? "" : fromValueName,
                ToValueName = action == TermGroup_TrackChangesAction.Delete ? "" : toValueName,
                Action = action,
                DataType = dataType,
                ParentEntity = parentEntity,
                ParentRecordId = parentRecordId > 0 ? parentRecordId : (int?)null
            };
        }
        #endregion

        #endregion

        #region LogEntities

        public List<SmallGenericType> GetTrackChangesLogEntities()
        {
            List<GenericType> terms = GetTermGroupContent(TermGroup.SoeEntityType);

            List<SmallGenericType> ents = new List<SmallGenericType>();

            AddEntityToTrackChangesLogEntities(SoeEntityType.AttestRole, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.AttestRole_AttestTransition, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.AttestRole_PrimaryCategory, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.AttestRole_SecondaryCategory, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.AttestRoleUser, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.ContactPerson, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmployeeAccount, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmployeeRequest_Availability, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmployeeTaxSE, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmployeeSetting, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmploymentPriceType, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EmploymentPriceTypePeriod, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.EventHistory, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.FixedPayrollRow, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.PaymentInformationRow, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.Role, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.RoleFeature, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.Supplier, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.TimeStampEntry, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.UserCompanyRole, ents, terms);
            AddEntityToTrackChangesLogEntities(SoeEntityType.UserCompanySetting_License, ents, terms);

            return ents.OrderBy(e => e.Name).ToList();
        }

        private void AddEntityToTrackChangesLogEntities(SoeEntityType type, List<SmallGenericType> ents, List<GenericType> terms)
        {
            GenericType term = terms.FirstOrDefault(t => t.Id == (int)type);
            if (term != null)
                ents.Add(new SmallGenericType((int)type, term.Name));
        }

        #endregion

        #region Get changes

        public List<TrackChanges> GetTrackChanges(int actorCompanyId, SoeEntityType entity, int recordId, bool includeChildren = false, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TrackChanges.NoTracking();
            List<TrackChanges> changes = (from t in entitiesReadOnly.TrackChanges
                                          where t.ActorCompanyId == actorCompanyId &&
                                          t.Entity == (int)entity &&
                                          t.RecordId == recordId &&
                                          (!dateFrom.HasValue || t.Created >= dateFrom.Value) &&
                                          (!dateTo.HasValue || t.Created <= dateTo.Value)
                                          select t).ToList();

            if (includeChildren)
            {
                changes.AddRange(from t in entitiesReadOnly.TrackChanges
                                 where t.ActorCompanyId == actorCompanyId &&
                                 t.ParentEntity == (int)entity &&
                                 t.ParentRecordId == recordId &&
                                 (!dateFrom.HasValue || t.Created >= dateFrom.Value) &&
                                 (!dateTo.HasValue || t.Created <= dateTo.Value)
                                 select t);
            }

            return changes.OrderByDescending(c => c.TrackChangesId).ToList();
        }

        public List<TrackChanges> GetTrackChangesForLog(int actorCompanyId, SoeEntityType entity, int recordId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TrackChanges.NoTracking();
            List<TrackChanges> changes = (from t in entities.TrackChanges
                                          where t.ActorCompanyId == actorCompanyId &&
                                          ((t.TopEntity == (int)entity && t.TopRecordId == recordId) || (t.ParentEntity == (int)entity && t.ParentRecordId == recordId) || (t.Entity == (int)entity && t.RecordId == recordId)) &&
                                          (!dateFrom.HasValue || t.Created >= dateFrom.Value) &&
                                          (!dateTo.HasValue || t.Created <= dateTo.Value)
                                          select t).ToList();

            if (entity == SoeEntityType.Employee)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var employee = entitiesReadOnly.Employee.FirstOrDefault(f => f.EmployeeId == recordId && f.State == (int)SoeEntityState.Active);

                if (employee?.UserId != null && (employee.UserId == base.UserId || FeatureManager.HasRolePermission(Feature.Manage_Users_Edit, Permission.Modify, base.RoleId, actorCompanyId)))
                {
                    changes.AddRange(GetTrackChangesForLog(actorCompanyId, SoeEntityType.User, employee.UserId.Value, dateFrom, dateTo));
                }
            }

            return changes.OrderByDescending(c => c.TrackChangesId).ToList();
        }

        public List<TrackChangesLogDTO> GetTrackChangesLog(int actorCompanyId, SoeEntityType entity, int recordId, DateTime dateFrom, DateTime dateTo)
        {
            List<TrackChangesLogDTO> logs = new List<TrackChangesLogDTO>();

            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            List<TrackChanges> changes = GetTrackChangesForLog(actorCompanyId, entity, recordId, dateFrom, dateTo).ToList();
            if (changes.Any())
            {
                int batchNbr = 0;
                string prevBatch = String.Empty;

                foreach (TrackChanges change in changes)
                {
                    if (string.IsNullOrEmpty(prevBatch) || change.Batch != prevBatch)
                        batchNbr++;

                    prevBatch = change.Batch;

                    string columnText = "";
                    if (change.ColumnType != (int)TermGroup_TrackChangesColumnType.Unspecified)
                    {
                        columnText = GetText(change.ColumnType, (int)TermGroup.TrackChangesColumnType);
                        if (!string.IsNullOrEmpty(change.ColumnName))
                            columnText += $" ({change.ColumnName})";
                    }
                    else
                        columnText = change.ColumnName;

                    logs.Add(new TrackChangesLogDTO()
                    {
                        BatchNbr = batchNbr,
                        TopRecordName = GetTopRecordName(change),
                        ActionMethodText = GetText(change.ActionMethod, (int)TermGroup.TrackChangesActionMethod),
                        EntityText = GetText(change.Entity, (int)TermGroup.SoeEntityType),
                        ColumnText = columnText,
                        ActionText = GetText(change.Action, (int)TermGroup.TrackChangesAction),
                        FromValueText = GetFromToValueText(change, true),
                        ToValueText = GetFromToValueText(change, false),
                        Created = change.Created,
                        CreatedBy = change.CreatedBy,
                        Role = change.Role,
                        RecordId = change.RecordId,
                        RecordName = GetRecordName(change)
                    });
                }
            }

            return logs;
        }

        public List<TrackChangesLogDTO> GetTrackChangesLogForEntity(int actorCompanyId, SoeEntityType entity, DateTime dateFrom, DateTime dateTo, List<string> users)
        {
            #region Query

            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TrackChanges.NoTracking();
            IQueryable<TrackChanges> query = (from t in entities.TrackChanges
                                              where t.ActorCompanyId == actorCompanyId &&
                                              (t.Created >= dateFrom) &&
                                              (t.Created <= dateTo)
                                              select t);

            if (entity != SoeEntityType.None)
                query = query.Where(t => (t.TopEntity == (int)entity || t.ParentEntity == (int)entity || t.Entity == (int)entity));

            if (users.Count > 0)
                query = query.Where(t => users.Contains(t.CreatedBy));

            List<TrackChanges> changes = query.ToList();

            #endregion

            #region Create log records

            int batchNbr = 0;
            string prevBatch = String.Empty;
            List<TrackChangesLogDTO> logs = new List<TrackChangesLogDTO>();

            foreach (TrackChanges change in changes)
            {
                if (string.IsNullOrEmpty(prevBatch) || change.Batch != prevBatch)
                    batchNbr++;

                prevBatch = change.Batch;

                string columnText = "";
                if (change.ColumnType != (int)TermGroup_TrackChangesColumnType.Unspecified)
                {
                    columnText = GetText(change.ColumnType, (int)TermGroup.TrackChangesColumnType);
                    if (!string.IsNullOrEmpty(change.ColumnName))
                        columnText += $" ({change.ColumnName})";
                }
                else
                    columnText = change.ColumnName;

                logs.Add(new TrackChangesLogDTO()
                {
                    TrackChangesId = change.TrackChangesId,
                    TopRecordName = GetTopRecordName(change),
                    RecordId = change.RecordId,
                    RecordName = GetRecordName(change),
                    BatchNbr = batchNbr,
                    ActionMethodText = GetText(change.ActionMethod, (int)TermGroup.TrackChangesActionMethod),
                    EntityText = GetText(change.Entity, (int)TermGroup.SoeEntityType),
                    ColumnText = columnText,
                    ActionText = GetText(change.Action, (int)TermGroup.TrackChangesAction),
                    FromValueText = GetFromToValueText(change, true),
                    ToValueText = GetFromToValueText(change, false),
                    Created = change.Created,
                    CreatedBy = change.CreatedBy,
                    Role = change.Role,
                    Batch = Guid.Parse(change.Batch),
                });
            }

            #endregion

            return logs;
        }

        private string GetTopRecordName(TrackChanges change)
        {
            string topRecordName = string.Empty;
            SoeEntityType entity;
            int id = 0;
            if (change.TopEntity.HasValidValue() && change.TopRecordId.HasValidValue())
            {
                entity = (SoeEntityType)change.TopEntity;
                id = (int)change.TopRecordId;
            }
            else if (change.ParentEntity.HasValidValue() && change.ParentRecordId.HasValidValue())
            {
                entity = (SoeEntityType)change.ParentEntity;
                id = (int)change.ParentRecordId;

            }
            else
            {
                entity = (SoeEntityType)change.Entity;
                id = change.RecordId;
            }

            if (entity != SoeEntityType.None && id != 0)
            {
                switch (entity)
                {
                    case SoeEntityType.AttestRole:
                        topRecordName = GetTopRecordNameForAttestRole(id);
                        break;
                    case SoeEntityType.Company:
                        topRecordName = GetTopRecordNameForCompany(id);
                        break;
                    case SoeEntityType.Employee:
                        topRecordName = GetTopRecordNameForEmployee(id);
                        if (topRecordName.IsNullOrEmpty())
                            topRecordName = GetTopRecordNameForUser(id);
                        break;
                    case SoeEntityType.Role:
                        topRecordName = GetTopRecordNameForRole(id);
                        break;
                    case SoeEntityType.Supplier:
                        topRecordName = GetTopRecordNameForSupplier(id);
                        break;
                    case SoeEntityType.User:
                        topRecordName = GetTopRecordNameForUser(id);
                        break;
                    case SoeEntityType.UserCompanySetting_License:
                        topRecordName = GetTopRecordNameForUserCompanySettingLicense(id);
                        break;
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForAttestRole(int id)
        {
            string topRecordName = string.Empty;

            if (attestRoleCache.ContainsKey(id))
            {
                topRecordName = attestRoleCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                AttestRole attestRole = entitiesReadOnly.AttestRole.FirstOrDefault(a => a.AttestRoleId == id);
                if (attestRole != null)
                {
                    topRecordName = attestRole.Name;
                    attestRoleCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForCompany(int id)
        {
            string topRecordName = string.Empty;

            if (companyCache.ContainsKey(id))
            {
                topRecordName = companyCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                Company company = entitiesReadOnly.Company.FirstOrDefault(c => c.ActorCompanyId == id);
                if (company != null)
                {
                    topRecordName = company.Name;
                    companyCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForEmployee(int id)
        {
            string topRecordName = string.Empty;

            if (employeeCache.ContainsKey(id))
            {
                topRecordName = employeeCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                Employee employee = entitiesReadOnly.Employee.Include("ContactPerson").FirstOrDefault(e => e.EmployeeId == id);
                if (employee != null)
                {
                    topRecordName = employee.NumberAndName;
                    employeeCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForRole(int id)
        {
            string topRecordName = string.Empty;

            if (roleCache.ContainsKey(id))
            {
                topRecordName = roleCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                Role role = entitiesReadOnly.Role.FirstOrDefault(r => r.RoleId == id);
                if (role != null)
                {
                    topRecordName = role.Name;
                    roleCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForSupplier(int id)
        {
            string topRecordName = string.Empty;

            if (supplierCache.ContainsKey(id))
            {
                topRecordName = supplierCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                Supplier supplier = entitiesReadOnly.Supplier.FirstOrDefault(s => s.ActorSupplierId == id);
                if (supplier != null)
                {
                    topRecordName = $"{supplier.SupplierNr} {supplier.Name}";
                    supplierCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForUser(int id)
        {
            string topRecordName = string.Empty;

            if (userCache.ContainsKey(id))
            {
                topRecordName = userCache[id];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                User user = entitiesReadOnly.User.FirstOrDefault(u => u.UserId == id);
                if (user != null)
                {
                    topRecordName = $"({user.LoginName}) {user.Name}";
                    userCache.Add(id, topRecordName);
                }
            }

            return topRecordName;
        }

        private string GetTopRecordNameForUserCompanySettingLicense(int id)
        {
            string topRecordName = string.Empty;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            License license = entitiesReadOnly.License.FirstOrDefault(l => l.LicenseId == id);
            if (license != null)
            {
                topRecordName = license.Name;
            }

            return topRecordName;
        }

        private string GetRecordName(TrackChanges change)
        {
            string recordName = string.Empty;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            switch ((SoeEntityType)change.Entity)
            {
                case SoeEntityType.AttestRole:
                    recordName = entitiesReadOnly.AttestRole.FirstOrDefault(a => a.AttestRoleId == change.RecordId)?.Name;
                    break;
                case SoeEntityType.AttestRole_AttestTransition:
                case SoeEntityType.AttestRole_PrimaryCategory:
                case SoeEntityType.AttestRole_SecondaryCategory:
                    break;
                case SoeEntityType.AttestRoleUser:
                    recordName = entitiesReadOnly.AttestRoleUser.Where(r => r.AttestRoleUserId == change.RecordId && r.UserId == change.TopRecordId).Select(r => r.AttestRole.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.ContactAddress:
                    recordName = entitiesReadOnly.ContactAddress.Where(c => c.ContactAddressId == change.RecordId).FirstOrDefault()?.Name ?? string.Empty;
                    break;
                case SoeEntityType.ContactECom:
                    recordName = entitiesReadOnly.ContactECom.Where(c => c.ContactEComId == change.RecordId).FirstOrDefault()?.Name ?? string.Empty;
                    break;
                case SoeEntityType.ContactPerson:
                    recordName = entitiesReadOnly.ContactPerson.Where(c => c.ActorContactPersonId == change.RecordId).FirstOrDefault()?.Name ?? string.Empty;
                    break;
                case SoeEntityType.EmployeeAccount:
                    recordName = entitiesReadOnly.EmployeeAccount.Where(a => a.EmployeeAccountId == change.RecordId && a.EmployeeId == change.TopRecordId).Select(a => a.Account.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.EmployeeSetting:
                    EmployeeSetting setting = entitiesReadOnly.EmployeeSetting.FirstOrDefault(s => s.EmployeeSettingId == change.RecordId);
                    if (setting != null)
                        recordName = $"{GetText(setting.EmployeeSettingAreaType, TermGroup.EmployeeSettingType)} - {GetText(setting.EmployeeSettingGroupType, TermGroup.EmployeeSettingType)} - {GetText(setting.EmployeeSettingType, TermGroup.EmployeeSettingType)}";
                    break;
                case SoeEntityType.EmployeeTaxSE:
                    recordName = entitiesReadOnly.EmployeeTaxSE.FirstOrDefault(t => t.EmployeeTaxId == change.RecordId && t.EmployeeId == change.TopRecordId)?.Year.ToString();
                    break;
                case SoeEntityType.EmploymentPriceType:
                    recordName = entitiesReadOnly.EmploymentPriceType.Where(p => p.EmploymentPriceTypeId == change.RecordId && p.Employment.EmployeeId == change.TopRecordId).Select(p => p.PayrollPriceType.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.EmploymentPriceTypePeriod:
                    recordName = entitiesReadOnly.EmploymentPriceTypePeriod.Where(p => p.EmploymentPriceTypePeriodId == change.RecordId && p.EmploymentPriceType.Employment.EmployeeId == change.TopRecordId).Select(p => p.EmploymentPriceType.PayrollPriceType.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.FixedPayrollRow:
                    recordName = entitiesReadOnly.FixedPayrollRow.Where(p => p.FixedPayrollRowId == change.RecordId && p.EmployeeId == change.TopRecordId).Select(p => p.PayrollProduct.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.Role:
                    recordName = entitiesReadOnly.Role.FirstOrDefault(r => r.RoleId == change.RecordId)?.Name;
                    break;
                case SoeEntityType.RoleFeature:
                    {
                        using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                        SysFeature feature = sysEntitiesReadOnly.SysFeature.FirstOrDefault(f => f.SysFeatureId == change.RecordId);
                        if (feature != null)
                        {
                            if (!string.IsNullOrEmpty(recordName))
                                recordName += " - ";
                            recordName += GetText(feature.SysTermId, feature.SysTermGroupId);
                        }
                        recordName += $" [{change.RecordId}]";
                    }
                    break;
                case SoeEntityType.Supplier:
                    recordName = entitiesReadOnly.Supplier.FirstOrDefault(s => s.ActorSupplierId == change.RecordId)?.Name;
                    break;
                case SoeEntityType.TimeStampEntry:
                    recordName = GetRecordNameForTimeStampEntry(change.RecordId);
                    break;
                case SoeEntityType.TimeStampEntryExtended:
                    if (change.ParentRecordId.HasValue)
                        recordName = GetRecordNameForTimeStampEntry(change.ParentRecordId.Value);
                    TimeStampEntryExtended extended = entitiesReadOnly.TimeStampEntryExtended.FirstOrDefault(e => e.TimeStampEntryExtendedId == change.RecordId);
                    if (extended != null)
                    {
                        if (!string.IsNullOrEmpty(recordName))
                            recordName += " - ";

                        if (extended.TimeScheduleTypeId.HasValue)
                            recordName += entitiesReadOnly.TimeScheduleType.Where(t => t.TimeScheduleTypeId == extended.TimeScheduleTypeId.Value).Select(t => t.Name).FirstOrDefault() ?? string.Empty;
                        else if (extended.TimeCodeId.HasValue)
                            recordName += entitiesReadOnly.TimeCode.Where(t => t.TimeCodeId == extended.TimeCodeId.Value).Select(t => t.Name).FirstOrDefault() ?? string.Empty;
                        else if (extended.AccountId.HasValue)
                            recordName += entitiesReadOnly.Account.Where(a => a.AccountId == extended.AccountId.Value).Select(a => a.Name).FirstOrDefault() ?? string.Empty;
                    }
                    break;
                case SoeEntityType.UserCompanyRole:
                    recordName = entitiesReadOnly.UserCompanyRole.Where(r => r.UserCompanyRoleId == change.RecordId && r.UserId == change.TopRecordId).Select(r => r.Role.Name).FirstOrDefault() ?? string.Empty;
                    break;
                case SoeEntityType.UserCompanySetting_License:
                    recordName = SettingManager.GetLicenseSettingName(change.RecordId);
                    break;
            }

            return recordName;
        }

        private string GetRecordNameForTimeStampEntry(int timeStampEntryId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TimeStampEntry entry = entitiesReadOnly.TimeStampEntry.FirstOrDefault(t => t.TimeStampEntryId == timeStampEntryId);
            if (entry != null)
            {
                string typeName = entry.IsBreak ? GetText(5990, 1, "Rast") : GetText(entry.Type, (int)TermGroup.TimeStampEntryType);
                return $"{typeName}: {CalendarUtility.ToShortDateTimeString(entry.Time)} (ID: {entry.TimeStampEntryId})";
            }

            return string.Empty;
        }

        #endregion

        #region Help-methods

        private TrackChanges CreateTrackChanges(TrackChangesDTO trackChangesItem, string roleName, string userDetails, DateTime? created = null, Guid? batch = null)
        {
            return new TrackChanges()
            {
                ActionMethod = (int)trackChangesItem.ActionMethod,
                TopEntity = (int)trackChangesItem.TopEntity,
                TopRecordId = trackChangesItem.TopRecordId,
                Entity = (int)trackChangesItem.Entity,
                RecordId = trackChangesItem.RecordId,
                ColumnName = trackChangesItem.ColumnName,
                ColumnType = (int)trackChangesItem.ColumnType,
                ParentEntity = trackChangesItem.ParentEntity == SoeEntityType.None ? (int?)null : (int?)trackChangesItem.ParentEntity,
                ParentRecordId = trackChangesItem.ParentRecordId,
                Action = (int)trackChangesItem.Action,
                DataType = (int)trackChangesItem.DataType,
                FromValue = trackChangesItem.FromValue,
                ToValue = trackChangesItem.ToValue,
                FromValueName = trackChangesItem.FromValueName,
                ToValueName = trackChangesItem.ToValueName,
                Role = roleName,
                Created = created ?? trackChangesItem.Created,
                CreatedBy = userDetails,
                Batch = batch?.ToString() ?? trackChangesItem.Batch.ToString(),

                //Set FK
                ActorCompanyId = trackChangesItem.ActorCompanyId,
            };
        }

        private string GetFromToValueText(TrackChanges change, bool isFrom)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string value = isFrom ? change.FromValue : change.ToValue;
            string valueName = isFrom ? change.FromValueName : change.ToValueName;

            // As default use value directly. Special treatment below.
            string text = "";
            if (valueName.IsNullOrEmpty())
                text = value;
            else
                text = $"{valueName} ({value})";

            if (change.DataType == (int)SettingDataType.Boolean)
                text = GetBoolText(value);

            switch ((SoeEntityType)change.Entity)
            {
                case SoeEntityType.EmployeeTaxSE:
                    #region EmployeeTaxSE

                    switch ((TermGroup_TrackChangesColumnType)change.ColumnType)
                    {
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_Type:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxType);
                            break;
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentType:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxAdjustmentType);
                            break;
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_SinkType:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxSinkType);
                            break;
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentTaxType:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxEmploymentTaxType);
                            break;
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentAbroadCode:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxEmploymentAbroadCode);
                            break;
                        case TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressAmountType:
                            text = GetTermGroupText(value, TermGroup.EmployeeTaxSalaryDistressAmountType);
                            break;
                    }

                    #endregion
                    break;
                case SoeEntityType.ContactPerson:
                    #region ContactPerson

                    switch ((TermGroup_TrackChangesColumnType)change.ColumnType)
                    {
                        case TermGroup_TrackChangesColumnType.ContactPerson_Sex:
                            text = GetTermGroupText(value, TermGroup.Sex);
                            break;
                    }

                    #endregion
                    break;
                case SoeEntityType.TimeStampEntry:
                    #region TimeStampEntry

                    switch ((TermGroup_TrackChangesColumnType)change.ColumnType)
                    {
                        case TermGroup_TrackChangesColumnType.TimeStampEntry_Type:
                            text = GetTermGroupText(value, TermGroup.TimeStampEntryType);
                            break;
                    }

                    #endregion
                    break;
                case SoeEntityType.TimeStampEntryExtended:
                    #region TimeStampEntryExtended

                    switch ((TermGroup_TrackChangesColumnType)change.ColumnType)
                    {
                        case TermGroup_TrackChangesColumnType.TimeStampEntryExtended_TimeScheduleTypeId:
                            if (int.TryParse(value, out int timeScheduleTypeId))
                                text = entitiesReadOnly.TimeScheduleType.Where(t => t.TimeScheduleTypeId == timeScheduleTypeId).Select(t => t.Name).FirstOrDefault() ?? string.Empty;
                            break;
                        case TermGroup_TrackChangesColumnType.TimeStampEntryExtended_TimeCodeId:
                            if (int.TryParse(value, out int timeCodeId))
                                text = entitiesReadOnly.TimeCode.Where(t => t.TimeCodeId == timeCodeId).Select(t => t.Name).FirstOrDefault() ?? string.Empty;
                            break;
                        case TermGroup_TrackChangesColumnType.TimeStampEntryExtended_AccountId:
                            if (int.TryParse(value, out int accountId))
                                text = entitiesReadOnly.Account.Where(a => a.AccountId == accountId).Select(a => a.Name).FirstOrDefault() ?? string.Empty;
                            break;
                    }

                    #endregion
                    break;
            }

            return text;
        }

        private string GetBoolText(string boolTextValue)
        {
            GenericType term = null;

            bool value;
            if (Boolean.TryParse(boolTextValue, out value))
            {
                List<GenericType> terms = GetTermGroupContent(TermGroup.YesNo, skipUnknown: true);
                term = terms.FirstOrDefault(x => x.Id == (int)(value ? TermGroup_YesNo.Yes : TermGroup_YesNo.No));
            }

            return term?.Name ?? string.Empty;
        }

        private string GetTermGroupText(string valueText, TermGroup termGroup)
        {
            string text = "";

            int value;
            if (Int32.TryParse(valueText, out value))
                return GetText(value, (int)termGroup);

            return text;
        }

        #endregion
    }
}
