using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class PermissionParameterObject
    {
        public CompEntities Entities { get; set; }
        public int LicenseId { get; private set; }
        public int ActorCompanyId { get; private set; }
        public int RoleId { get; private set; }
        public string Thread { get; set; }
        public bool Valid { get; private set; }
        public PermissionParameterObject(CompEntities entities, int licenseId, int actorCompanyId, int roleId, string thread)
        {
            this.Entities = entities;
            this.LicenseId = licenseId;
            this.ActorCompanyId = actorCompanyId;
            this.RoleId = roleId;
            this.Thread = thread;
        }
        public void SetValid()
        {
            this.Valid = true;
        }
        public override string ToString()
        {
            return $"{this.Thread} : {this.LicenseId}_{this.ActorCompanyId}_{this.RoleId}";
        }
    }
    public static class PermissionParameterObjectExtensions
    {
        public static bool IsValid(this PermissionParameterObject e)
        {
            return e?.Valid ?? false;
        }
    }

    public class PermissionCacheRepository
    {
        #region Variables

        private LicensePermissionCache licenseCache = null;
        public int LicenseId { get; private set; }

        #endregion

        #region Ctor

        public PermissionCacheRepository(int licenseId)
        {
            this.LicenseId = licenseId;
        }

        #endregion

        #region Public methods

        public Permission GetPermission(PermissionParameterObject param, Feature feature)
        {
            var layers = GetCacheLayers(param);
            return layers.IsLoaded ? CalculatePermission(layers.Company, layers.Role, feature) : Permission.None;
        }
        public bool HasReadPermission(PermissionParameterObject param, Feature feature)
        {
            return HasPermission(param, feature, Permission.Readonly);
        }
        public bool HasModifyPermission(PermissionParameterObject param, Feature feature)
        {
            return HasPermission(param, feature, Permission.Modify);
        }
        public bool HasPermission(PermissionParameterObject param, Feature feature, Permission permission)
        {
            return GetPermission(param, feature) >= permission;
        }
        public void AddPermissions(PermissionParameterObject param, List<FeaturePermissionView> permissionsItems)
        {
            if (permissionsItems == null)
                return;

            var layers = GetCacheLayers(param);

            if (this.licenseCache == null)
                this.licenseCache = new LicensePermissionCache(param.LicenseId);
            else
                this.licenseCache.ClearFeaturePermissions();
            this.licenseCache.AddPermissions(permissionsItems, FeaturePermissionType.License);

            CompanyPermissionCache companyCache = layers.Company;
            if (companyCache == null)
                companyCache = new CompanyPermissionCache(param.LicenseId, param.ActorCompanyId);
            else
                companyCache.ClearFeaturePermissions();
            companyCache.AddPermissions(permissionsItems, FeaturePermissionType.Company);

            RolePermissionCache roleCache = layers.Role;
            if (roleCache == null)
                roleCache = new RolePermissionCache(param.LicenseId, param.ActorCompanyId, param.RoleId);
            else
                roleCache.ClearFeaturePermissions();
            roleCache.AddPermissions(permissionsItems, FeaturePermissionType.Role);

            companyCache.AddRoleCache(roleCache);
            licenseCache.AddCompanyCache(companyCache);
        }
        public void ClearPermissions(PermissionParameterObject param, FeaturePermissionType type)
        {
            var layers = GetCacheLayers(param);
            if (type == FeaturePermissionType.License && this.licenseCache != null)
            {
                this.licenseCache.Flush();
                this.licenseCache = null;
            }
            else if (type == FeaturePermissionType.Company && layers.Company != null)
            {
                layers.Company.Flush();
                if (this.licenseCache != null)
                    this.licenseCache.RemoveCompanyCache(layers.Company);
            }
            else if (type == FeaturePermissionType.Role && layers.Role != null)
            {
                layers.Role.Flush();
                if (layers.Company != null)
                    layers.Company.RemoveRoleCache(layers.Role);
            }                
        }
        public bool IsLoaded(PermissionParameterObject param)
        {
            return GetCacheLayers(param).IsLoaded;
        }

        #endregion

        #region Privat methods

        private (bool IsLoaded, CompanyPermissionCache Company, RolePermissionCache Role) GetCacheLayers(PermissionParameterObject param)
        {
            if (this.licenseCache == null || param == null)
                return (false, null, null);

            CompanyPermissionCache companyCache = param.ActorCompanyId > 0 ? this.licenseCache.GetCompanyCache(param.ActorCompanyId) : null;
            RolePermissionCache roleCache = param.RoleId > 0 ? companyCache?.GetRoleCache(param.RoleId) : null;
            return (companyCache != null && roleCache != null, companyCache, roleCache);
        }
        private Permission CalculatePermission(CompanyPermissionCache companyCache, RolePermissionCache roleCache, Feature feature)
        {
            if (feature == Feature.None || this.licenseCache == null || companyCache == null || roleCache == null)
                return Permission.None;

            Permission licensePermission = this.licenseCache.GetPermission(feature);
            if (licensePermission == Permission.None)
                return licensePermission;
            Permission companyPermission = companyCache.GetPermission(feature).EvaluatePermission(licensePermission);
            if (companyPermission == Permission.None)
                return companyPermission;
            Permission rolePermission = roleCache.GetPermission(feature).EvaluatePermission(companyPermission);
            return rolePermission;
        }

        #endregion
    }

    public abstract class PermissionCacheBase
    {
        public string Key { get; }
        private readonly Dictionary<Feature, Permission> FeaturePermissions = new Dictionary<Feature, Permission>();
        protected PermissionCacheBase(params int[] keyParts)
        {
            this.Key = GetKey(keyParts);
        }
        public Permission GetPermission(Feature feature)
        {
            if (this.FeaturePermissions.ContainsKey(feature))
                return this.FeaturePermissions[feature];
            else
                return Permission.None;
        }
        public void AddPermission(Feature feature, int? permission)
        {
            if (!permission.HasValue)
                return;
            AddPermission(feature, (Permission)permission.Value);
        }
        public void AddPermission(Feature feature, Permission permission)
        {
            if (feature == Feature.None)
                return;

            if (this.FeaturePermissions.ContainsKey(feature))
                this.FeaturePermissions[feature] = permission;
            else
                this.FeaturePermissions.Add(feature, permission);
        }
        public void AddPermissions(List<FeaturePermissionView> permissionsItems, FeaturePermissionType type)
        {
            if (permissionsItems.IsNullOrEmpty())
                return;

            foreach (FeaturePermissionView permissionsItem in permissionsItems.Where(item => item.Type == (int)type && Enum.IsDefined(typeof(Feature), item.SysFeatureId)))
            {
                this.AddPermission((Feature)permissionsItem.SysFeatureId, (Permission)permissionsItem.SysPermissionId);
            }
        }
        public void ClearFeaturePermissions()
        {
            this.FeaturePermissions.Clear();
        }
        protected string GetKey(params int[] keyParts)
        {
            return keyParts.JoinToString("_");
        }
    }

    public class LicensePermissionCache : PermissionCacheBase
    {
        public int LicenseId { get; private set; }
        private readonly Dictionary<string, CompanyPermissionCache> CompanyCache = new Dictionary<string, CompanyPermissionCache>();
        public LicensePermissionCache(int licenseId) : base(licenseId)
        {
            this.LicenseId = licenseId;
        }
        public CompanyPermissionCache GetCompanyCache(int actorCompanyId)
        {
            return this.CompanyCache.GetValue(base.GetKey(this.LicenseId, actorCompanyId));
        }
        public void AddCompanyCache(CompanyPermissionCache companyCache)
        {
            this.CompanyCache.SetValue(companyCache.Key, companyCache);
        }
        public void RemoveCompanyCache(CompanyPermissionCache companyCache)
        {
            this.CompanyCache.Remove(companyCache.Key);
        }
        public List<CompanyPermissionCache> GetCompanyCaches()
        {
            return CompanyCache.Select(p => p.Value).ToList();
        }
        public void Flush()
        {
            base.ClearFeaturePermissions();
            foreach (var companyCache in this.CompanyCache.Values)
            {
                companyCache.ClearFeaturePermissions();
            }
            this.CompanyCache.Clear();
        }
    }

    public class CompanyPermissionCache : PermissionCacheBase
    {
        public int LicenseId { get; private set; }
        public int ActorCompanyId { get; private set; }
        private readonly Dictionary<string, RolePermissionCache> RoleCache = new Dictionary<string, RolePermissionCache>();
        public CompanyPermissionCache(int licenseId, int actorCompanyId) : base(licenseId, actorCompanyId)
        {
            this.LicenseId = licenseId;
            this.ActorCompanyId = actorCompanyId;
        }
        public RolePermissionCache GetRoleCache(int roleId)
        {
            return this.RoleCache.GetValue(base.GetKey(this.LicenseId, this.ActorCompanyId, roleId));
        }
        public void AddRoleCache(RolePermissionCache roleCache)
        {
            this.RoleCache.SetValue(roleCache.Key, roleCache);
        }
        public void RemoveRoleCache(RolePermissionCache roleCache)
        {
            this.RoleCache.Remove(roleCache.Key);
        }
        public List<RolePermissionCache> GetRoleCaches()
        {
            return RoleCache.Select(p => p.Value).ToList();
        }
        public void Flush()
        {
            base.ClearFeaturePermissions();
            foreach (var roleCache in this.RoleCache.Values)
            {
                roleCache.ClearFeaturePermissions();
            }
            this.RoleCache.Clear();
        }
    }

    public class RolePermissionCache : PermissionCacheBase
    {
        public int LicenseId { get; private set; }
        public int ActorCompanyId { get; private set; }
        public int RoleId { get; private set; }
        public RolePermissionCache(int licenseId, int actorCompanyId, int roleId) : base(licenseId, actorCompanyId, roleId)
        {
            this.LicenseId = licenseId;
            this.ActorCompanyId = actorCompanyId;
            this.RoleId = roleId;
        }

        public void Flush()
        {
            base.ClearFeaturePermissions();
        }
    }
}
