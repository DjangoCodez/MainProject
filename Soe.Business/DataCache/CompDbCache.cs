using SoftOne.Common.KeyVault;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Business.DataCache
{
    public sealed class CompDbCache
    {
        #region Variables

        static readonly object timeRuleCacheLock = new object();
        static readonly object formsLock = new object();
        static readonly object fieldsLock = new object();

        //public 
        private TermGroup_SysPageStatusSiteType _siteType;
        public TermGroup_SysPageStatusSiteType SiteType
        {
            get
            {
                if (_siteType == TermGroup_SysPageStatusSiteType.Test)
                {
                    _siteType = ConfigurationSetupUtil.GetSiteType();
                }
                return _siteType;
            }
            set
            {
                _siteType = value;
            }
        }
        private int _sysCompDbId { get; set; }
        public int SysCompDbId
        {
            get
            {
                if (_sysCompDbId == 0)
                    _sysCompDbId = ConfigurationSetupUtil.GetCurrentSysCompDbId();

                return _sysCompDbId;
            }
            set
            {
                _sysCompDbId = value;
            }
        }
        private string _redisCacheConnectionString;
        public string RedisCacheConnectionString
        {
            get
            {

                if (string.IsNullOrEmpty(_redisCacheConnectionString))
                    _redisCacheConnectionString = KeyVaultSecretsFetcher.GetSecret("RedisCacheConnection");

                return _redisCacheConnectionString;
            }
            set
            {
                _redisCacheConnectionString = value;
            }
        }

        #endregion

        #region Singleton

        private CompDbCache() { }
        private static readonly Lazy<CompDbCache> instance = new Lazy<CompDbCache>(() => new CompDbCache());
        public static CompDbCache Instance
        {
            get => instance.Value;
        }

        #endregion

        #region Public methods

        public void FlushAll(int actorCompanyId)
        {
            lock (timeRuleCacheLock)
            {
                TimeRuleManager trm = new TimeRuleManager(null);
                trm.FlushTimeRulesFromCache(actorCompanyId);

                MethodInfo[] methodInfos = typeof(CompDbCache).GetMethods();
                foreach (MethodInfo methodInfo in methodInfos)
                {
                    if (methodInfo.Name != "FlushAll" && methodInfo.Name.StartsWith("Flush") && !methodInfo.GetParameters().Any())
                        methodInfo.Invoke(this, null);
                }
            }
        }

        #endregion

        #region Form

        private List<Form> forms;
        public List<Form> Forms
        {
            get
            {
                if (this.forms == null)
                {
                    lock (formsLock)
                    {
                        var m = new FieldSettingManager(null);
                        this.forms = m.GetAllFormsFromDB();
                    }
                }
                return this.forms;
            }
        }
        public void FlushForms()
        {
            this.forms = null;
        }

        #endregion

        #region Field

        private List<Field> fields;
        public List<Field> Fields
        {
            get
            {
                if (this.fields == null)
                {
                    lock (fieldsLock)
                    {
                        var m = new FieldSettingManager(null);
                        this.fields = m.GetFieldsFromDb();
                    }
                }
                return this.fields;
            }
        }
        public void FlushFields()
        {
            this.forms = null;
        }

        #endregion

        #region UserCompanyRole

        public List<UserCompanyRole> GetUserCompanyRoles(int userId)
        {
            string key = "GetUserCompanyRoles#" + userId.ToString();
            List<UserCompanyRole> userCompanyRoles = BusinessMemoryCache<List<UserCompanyRole>>.Get(key);
            if (userCompanyRoles == null)
            {
                UserManager m = new UserManager(null);
                userCompanyRoles = m.GetUserCompanyRolesByUser(userId, loadUser: true, loadCompany: true, loadRole: true);
                if (userCompanyRoles != null)
                    BusinessMemoryCache<List<UserCompanyRole>>.Set(key, userCompanyRoles, 60 * 10);
            }

            return userCompanyRoles;
        }
        public void FlushUserCompanyRoles(int userId)
        {
            BusinessMemoryCache<List<UserCompanyRole>>.Delete("GetUserCompanyRoles#" + userId.ToString());
        }

        #endregion

    }
}
