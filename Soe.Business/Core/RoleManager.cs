using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class RoleManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public RoleManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Role

        public List<Role> GetAllRoles(bool loadCompanyAndLicense = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetAllRoles(entities, loadCompanyAndLicense);
        }

        public List<Role> GetAllRoles(CompEntities entities, bool loadCompanyAndLicense = false)
        {
            if (loadCompanyAndLicense)
                return entities.Role.Include("Company.License").Where(r => r.State == (int)SoeEntityState.Active).ToList();
            else
                return entities.Role.Where(r => r.State == (int)SoeEntityState.Active).ToList();
        }

        public List<Role> GetRolesByUser(int userId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanyRole.NoTracking();
            return GetRolesByUser(entities, userId, actorCompanyId, dateFrom, dateTo);
        }

        public List<Role> GetRolesByUser(CompEntities entities, int userId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (dateFrom == null)
                dateFrom = DateTime.Today;
            if (dateTo == null)
                dateTo = dateFrom;

            var roles = (from ucr in entities.UserCompanyRole.Include("Role")
                         where ucr.ActorCompanyId == actorCompanyId &&
                         ucr.State == (int)SoeEntityState.Active &&
                         (!ucr.DateFrom.HasValue || ucr.DateFrom <= dateTo) &&
                         (!ucr.DateTo.HasValue || ucr.DateTo >= dateFrom) &&
                         ucr.UserId == userId
                         orderby ucr.Role.Name
                         select ucr.Role).ToList();

            SetRoleNameTexts(roles);
            return roles.OrderBy(r => r.Name).ToList();
        }

        public List<UserCompanyRole> GetAllUserCompanyRolesByCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanyRole.NoTracking();
            return GetAllUserCompanyRolesByCompany(entities, actorCompanyId);
        }

        public List<UserCompanyRole> GetAllUserCompanyRolesByCompany(CompEntities entities, int actorCompanyId)
        {
            var userCompanyRoles = (from r in entities.UserCompanyRole
                                    .Include("Role")
                                    where r.ActorCompanyId == actorCompanyId && 
                                    (r.State == (int)SoeEntityState.Active || r.State == (int)SoeEntityState.Inactive)
                                    select r).ToList();

            SetRoleNameTexts(userCompanyRoles);

            return userCompanyRoles.OrderBy(r => r.Role.Sort).ToList();
        }

        public List<Role> GetAllRolesByCompany(int actorCompanyId, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetAllRolesByCompany(entities, actorCompanyId, loadExternalCode);
        }

        public List<Role> GetAllRolesByCompany(CompEntities entities, int actorCompanyId, bool loadExternalCode = false)
        {
            var roles = (from r in entities.Role
                         where r.ActorCompanyId == actorCompanyId
                         && (r.State.Equals(0) || r.State.Equals(1))
                         select r).ToList();

            if (loadExternalCode)
                LoadRoleExternalCodes(entities, roles, actorCompanyId);

            SetRoleNameTexts(roles);
            return roles.OrderBy(r => r.Name).ToList();
        }


        public List<Role> GetRolesByCompany(int actorCompanyId, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetRolesByCompany(entities, actorCompanyId, loadExternalCode);
        }

        public List<Role> GetRolesByCompany(CompEntities entities, int actorCompanyId, bool loadExternalCode = false)
        {
            var roles = (from r in entities.Role
                         where r.ActorCompanyId == actorCompanyId &&
                         r.State == (int)SoeEntityState.Active
                         select r).ToList();

            if (loadExternalCode)
                LoadRoleExternalCodes(entities, roles, actorCompanyId);

            SetRoleNameTexts(roles);
            return roles.OrderBy(r => r.Name).ToList();
        }

        public List<Role> GetRolesByName(string name, int? actorCompanyId)
        {
            List<Role> validRoles = new List<Role>();

            name = name.ToLower();

            List<Role> roles = actorCompanyId.HasValue ? GetRolesByCompany(actorCompanyId.Value) : GetAllRoles();
            foreach (Role role in roles)
            {
                role.Name = GetRoleNameText(role);
                if (role.Name.ToLower() == name)
                    validRoles.Add(role);
            }

            return validRoles.OrderBy(r => r.Name).ToList();
        }

        public Dictionary<int, string> GetRolesByCompanyDict(int actorCompanyId, bool addEmptyRow, bool addEmptyRowAsAll)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");
            else if (addEmptyRowAsAll)
                dict.Add(0, GetText(4366, "Alla"));

            List<Role> roles = GetRolesByCompany(actorCompanyId);
            foreach (Role role in roles)
            {
                if (!dict.ContainsKey(role.RoleId))
                    dict.Add(role.RoleId, GetRoleNameText(role));
            }

            return dict;
        }

        public Role GetRole(int roleId, int actorCompanyId, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetRole(entities, roleId, actorCompanyId, loadExternalCode);
        }

        public Role GetRole(CompEntities entities, int roleId, int actorCompanyId, bool loadExternalCode = false)
        {
            var role = entities.Role.FirstOrDefault(r => r.RoleId == roleId && r.ActorCompanyId == actorCompanyId);
            if (role != null && loadExternalCode)
                LoadRoleExternalCodes(entities, role);
            return role;
        }

        public Role GetRole(int roleId, bool loadCompany = false, bool loadLicense = false, bool loadExternalCode = false, bool loadWithDeactivatedRoles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetRole(entities, roleId, loadCompany, loadLicense, loadExternalCode, loadWithDeactivatedRoles);
        }

        public Role GetRole(CompEntities entities, int roleId, bool loadCompany = false, bool loadLicense = false, bool loadExternalCode = false, bool discardState = false)
        {
            IQueryable<Role> query = entities.Role.Where(r => r.RoleId == roleId);
            if (!discardState)
                query = query.Where(r => r.State == (int)SoeEntityState.Active);
            if (loadCompany)
                query = query.Include("Company");
            if (loadLicense)
                query = query.Include("Company.License");

            Role role = query.FirstOrDefault();
            if (role != null)
            {
                SetSystemRoleName(role);
                if (loadExternalCode)
                    LoadRoleExternalCodes(entities, role);
            }
            return role;
        }

        public Role GetRoleDiscardState(int roleId, bool loadCompany = false, bool loadLicense = false, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetRoleDiscardState(entities, roleId, loadCompany, loadLicense, loadExternalCode);
        }

        public Role GetRoleDiscardState(CompEntities entities, int roleId, bool loadCompany = false, bool loadLicense = false, bool loadExternalCode = false)
        {
            IQueryable<Role> query = entities.Role.Where(r => r.RoleId == roleId);
            if (loadCompany)
                query = query.Include("Company");
            if (loadLicense)
                query = query.Include("Company.License");

            Role role = query.FirstOrDefault();
            if (role != null)
            {
                SetSystemRoleName(role);
                if (loadExternalCode)
                    LoadRoleExternalCodes(entities, role);
            }

            return role;
        }

        public Role GetRoleAdmin(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Role.NoTracking();
            return GetRoleAdmin(entities, actorCompanyId);
        }

        public Role GetRoleAdmin(CompEntities entities, int actorCompanyId)
        {
            //Take first created
            return (from r in entities.Role
                    where r.ActorCompanyId == actorCompanyId &&
                    r.TermId == (int)TermGroup_Roles.Systemadmin &&
                    r.State == (int)SoeEntityState.Active
                    orderby r.RoleId
                    select r).FirstOrDefault();
        }

        public Role GetRoleByName(string name, int actorCompanyId)
        {
            name = name.ToLower();

            List<Role> rolesByCompany = GetRolesByCompany(actorCompanyId);
            foreach (Role role in rolesByCompany)
            {
                role.Name = GetRoleNameText(role);
                if (role.Name.ToLower() == name)
                    return role;
            }
            return null;
        }

        public string GetRoleName(int roleId, bool includeCompanyName = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetRoleName(entities, roleId, includeCompanyName);
        }

        public string GetRoleName(CompEntities entities, int roleId, bool includeCompanyName = false)
        {
            string key = $"GetRoleName#{roleId}#{includeCompanyName}";

            var roleName = BusinessMemoryCache<string>.Get(key);
            if (!roleName.IsNullOrEmpty())
                return roleName;

            Role role = GetRole(entities, roleId, includeCompanyName, false);
            roleName = GetRoleNameText(role, includeCompanyName);
            if (!roleName.IsNullOrEmpty())
                BusinessMemoryCache<string>.Set(key, roleName, 60);

            return roleName;
        }

        public string GetRoleNameText(Role role, bool includeCompanyName = false)
        {
            if (role == null)
                return string.Empty;

            int termId = role.TermId ?? 0;
            string roleName = role.Name;

            if (string.IsNullOrEmpty(roleName) && termId > 0)
                roleName = GetText(termId, (int)TermGroup.Role);

            if (includeCompanyName && role.Company != null)
                roleName = $"{roleName} ({role.Company.Name})";

            return roleName;
        }

        public void SetRoleNameTexts(List<UserCompanyRole> userCompanyRoles)
        {
            foreach (UserCompanyRole userCompanyRole in userCompanyRoles.Where(r => r.Role != null))
            {
                userCompanyRole.Role.Name = GetRoleNameText(userCompanyRole.Role);
            }
        }

        public void SetRoleNameTexts(List<Role> roles)
        {
            foreach (Role role in roles)
            {
                role.Name = GetRoleNameText(role);
            }
        }

        public void SetSystemRoleName(Role role)
        {
            if (role != null && role.TermId.HasValue && role.TermId.Value > 0)
                role.SystemRoleName = GetText(role.TermId.Value, (int)TermGroup.Role);
        }

        public bool HasUserGivenRole(int userId, int roleId, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanyRole.NoTracking();
            return (from ucr in entities.UserCompanyRole
                    where ucr.User.UserId == userId &&
                    ucr.Role.RoleId == roleId &&
                    ucr.State == (int)SoeEntityState.Active &&
                    ucr.User.State == (int)SoeEntityState.Active &&
                    (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                    (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                    select ucr).Any();
        }

        public bool HasUserAnyRoles(int roleId, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanyRole.NoTracking();
            return (from ucr in entities.UserCompanyRole
                    where ucr.Role.RoleId == roleId &&
                    ucr.State == (int)SoeEntityState.Active &&
                    ucr.User.State == (int)SoeEntityState.Active &&
                    (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                    (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                    select ucr).Any();
        }

        public bool ValidateRoleInfo(int roleId, ref int actorCompanyId, ref int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ValidateLicenseCompanyRoleIds(entities, roleId, ref actorCompanyId, ref licenseId);
        }

        public bool ValidateLicenseCompanyRoleIds(CompEntities entities, int roleId, ref int actorCompanyId, ref int licenseId)
        {
            if (actorCompanyId <= 0 || licenseId <= 0)
            {
                if (actorCompanyId > 0 && actorCompanyId == base.parameterObject?.ActorCompanyId)
                {
                    licenseId = base.parameterObject.LicenseId;
                }
                else
                {
                    Company company = CompanyManager.GetCompanyByRoleId(entities, roleId);
                    if (company != null)
                    {
                        actorCompanyId = company.ActorCompanyId;
                        licenseId = company.LicenseId;
                    }
                }
            }
            return actorCompanyId > 0 && licenseId > 0;
        }

        public void LoadRoleExternalCodes(CompEntities entities, List<Role> roles, int actorCompanyId)
        {
            if (roles.IsNullOrEmpty())
                return;

            List<CompanyExternalCode> externalCodes = ActorManager.TryPreloadCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.Role, roles.Select(i => i.RoleId).ToList(), actorCompanyId);
            if (externalCodes.IsNullOrEmpty())
                return;

            foreach (Role role in roles)
            {
                LoadRoleExternalCodes(entities, role, externalCodes);
            }
        }

        public void LoadRoleExternalCodes(CompEntities entities, Role role, List<CompanyExternalCode> externalCodes = null)
        {
            if (role == null || !role.ActorCompanyId.HasValue)
                return;

            //Only load if not already loaded
            if (!role.ExternalCodes.IsNullOrEmpty())
                return;

            if (externalCodes != null)
                externalCodes = externalCodes.Where(i => i.RecordId == role.RoleId).ToList();
            else
                externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.Role, role.RoleId, role.ActorCompanyId.Value);

            role.ExternalCodes = new List<string>();
            if (!externalCodes.IsNullOrEmpty())
            {
                role.ExternalCodes.AddRange(externalCodes.Select(s => s.ExternalCode));
                role.ExternalCodesString = StringUtility.GetSeparatedString(externalCodes.Select(s => s.ExternalCode), Constants.Delimiter, true, false);
            }
        }

        public ActionResult AddRole(Role role, int actorCompanyId)
        {
            if (role == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Role");

            using (CompEntities entities = new CompEntities())
            {
                role.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (role.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (string.IsNullOrEmpty(role.ExternalCodesString))
                {
                    return AddEntityItem(entities, role, "Role");
                }
                else
                {
                    AddEntityItem(entities, role, "Role");
                    return ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.Role, role.RoleId, role.ExternalCodesString, actorCompanyId);
                }
            }
        }

        public ActionResult SaveRole(RoleEditDTO roleInput, int actorCompanyId, int userId)
        {
            if (roleInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Role");

            ActionResult result = null;

            int roleId = roleInput.RoleId;

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
            Dictionary<int, EntityObject> tcDict = new Dictionary<int, EntityObject>();
            int tempIdCounter = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool isNew = false;

                        #region Role

                        Role role = GetRole(entities, roleId, loadExternalCode: true, discardState: true);
                        if (role == null)
                        {
                            #region Add

                            role = new Role()
                            {
                                ActorCompanyId = actorCompanyId,
                                TermId = 0,
                                FavoriteOption = roleInput.FavoriteOption,
                                State = (int)SoeEntityState.Active
                            };
                            SetCreatedProperties(role);
                            entities.Role.AddObject(role);
                            isNew = true;

                            tempIdCounter++;
                            trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonInsert, TermGroup_TrackChangesAction.Insert, SoeEntityType.Role, 0, SoeEntityType.Role, tempIdCounter));
                            tcDict.Add(tempIdCounter, role);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (role.FavoriteOption != roleInput.FavoriteOption)
                            {
                                Dictionary<int, string> favorites = SettingManager.GetFavoriteItemOptionsDict(false);
                                string fromValueName = favorites.ContainsKey(role.FavoriteOption) ? favorites[role.FavoriteOption] : string.Empty;
                                string toValueName = favorites.ContainsKey(roleInput.FavoriteOption) ? favorites[roleInput.FavoriteOption] : string.Empty;
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, role.RoleId, SoeEntityType.Role, role.RoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_FavoriteOption, role.FavoriteOption.ToString(), roleInput.FavoriteOption.ToString(), fromValueName, toValueName));

                                role.FavoriteOption = roleInput.FavoriteOption;
                            }

                            SetModifiedProperties(role);

                            #endregion
                        }

                        #region Common

                        if (!role.IsAdmin && role.Name != roleInput.Name)
                        {
                            if (!isNew)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, role.RoleId, SoeEntityType.Role, role.RoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.Role_Name, role.Name, roleInput.Name));
                            role.Name = roleInput.Name;
                        }

                        if (!role.IsAdmin && role.State != (int)roleInput.State)
                        {
                            if (!isNew)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, role.RoleId, SoeEntityType.Role, role.RoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_State, role.State.ToString(), ((int)roleInput.State).ToString()));
                            role.State = (int)roleInput.State;
                        }

                        if (role.Sort != roleInput.Sort)
                        {
                            if (!isNew)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, role.RoleId, SoeEntityType.Role, role.RoleId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_Sort, role.Sort.ToString(), roleInput.Sort.ToString()));
                            role.Sort = roleInput.Sort;
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region External codes

                        if (role.ExternalCodesString != roleInput.ExternalCodesString)
                        {
                            if (!isNew)
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, role.RoleId, SoeEntityType.Role, role.RoleId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.Role_ExternalCodes, role.ExternalCodesString, roleInput.ExternalCodesString));

                            if (!isNew && string.IsNullOrEmpty(roleInput.ExternalCodesString))
                                ActorManager.DeleteExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.Role, roleInput.RoleId, actorCompanyId, false);
                            else if (!string.IsNullOrEmpty(roleInput.ExternalCodesString))
                                ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.Role, role.RoleId, roleInput.ExternalCodesString, actorCompanyId, false);
                        }

                        #endregion

                        #region Copy role permissions

                        if (isNew && roleInput.TemplateRoleId != 0)
                        {
                            List<RoleFeature> roleFeatures = FeatureManager.GetRoleFeatures(entities, roleInput.TemplateRoleId);
                            if (roleFeatures.Any())
                            {
                                foreach (RoleFeature roleFeature in roleFeatures)
                                {
                                    RoleFeature newRoleFeature = new RoleFeature()
                                    {
                                        SysFeatureId = roleFeature.SysFeatureId,
                                        SysPermissionId = roleFeature.SysPermissionId
                                    };
                                    SetCreatedProperties(newRoleFeature);
                                    role.RoleFeature.Add(newRoleFeature);
                                }
                            }
                        }

                        #endregion

                        #region Update start page on users

                        if (!isNew && roleInput.UpdateStartPage)
                        {
                            FavoriteItem favoriteItem = SettingManager.GetFavoriteItemOptionFromRole(role);
                            if (favoriteItem != null)
                            {
                                List<User> users = UserManager.GetUsersByRole(entities, actorCompanyId, roleInput.RoleId, userId, active: null);
                                if (users.Any())
                                {
                                    Company company = CompanyManager.GetCompany(entities, actorCompanyId);

                                    foreach (User user in users)
                                    {
                                        UserFavorite userFavorite = SettingManager.GetUserFavoriteDefault(entities, user.UserId);
                                        if (userFavorite == null)
                                        {
                                            userFavorite = new UserFavorite()
                                            {
                                                IsDefault = true,
                                                Company = company,
                                                User = user,
                                            };
                                        }
                                        userFavorite.Name = favoriteItem.FavoriteName;
                                        userFavorite.Url = favoriteItem.FavoriteUrl;
                                    }
                                }
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            #region TrackChanges

                            // Add track changes
                            foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
                            {
                                // Replace temp ids with actual ids created on save
                                if (dto.Entity == SoeEntityType.Role && tcDict[dto.RecordId] is Role)
                                    dto.RecordId = (tcDict[dto.RecordId] as Role).RoleId;
                            }
                            if (trackChangesItems.Any())
                                result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

                            #endregion

                            transaction.Complete();
                            roleId = role.RoleId;
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
                        result.IntegerValue = roleId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteRole(int roleId, int actorCompanyId)
        {
            if (HasUserAnyRoles(roleId))
                return new ActionResult((int)ActionResultDelete.RoleHasUsers, GetText(1286, "Roll kunde inte tas bort, kontrollera att den inte används"));

            using (CompEntities entities = new CompEntities())
            {
                Role originalRole = GetRole(entities, roleId, loadCompany: true, discardState: true);
                if (originalRole == null || !originalRole.ActorCompanyId.HasValue || originalRole.ActorCompanyId.Value != actorCompanyId)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Role");

                ActionResult result = ChangeEntityState(entities, originalRole, SoeEntityState.Deleted, true);
                if (result.Success)
                    TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonDelete, TermGroup_TrackChangesAction.Delete, SoeEntityType.Role, originalRole.RoleId, SoeEntityType.Role, originalRole.RoleId));

                return result;
            }
        }

        public ActionResult UpdateRolesState(Dictionary<int, bool> roleStates)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> roleState in roleStates)
                {
                    Role role = GetRole(entities, roleState.Key);
                    if (role == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

                    ChangeEntityState(role, roleState.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult VerifyRoleHasUsers(int roleId)
        {
            var result = new ActionResult();
            if (HasUserAnyRoles(roleId))
                result = new ActionResult((int)ActionResultDelete.RoleHasUsers, GetText(13030003));
            return result;
        }

        #endregion
    }
}
