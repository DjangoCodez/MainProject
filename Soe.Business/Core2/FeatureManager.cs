using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public sealed class FeaturePermissionService
    {
        #region Variables

        private static readonly ConcurrentDictionary<int, object> licensePermissionLocks = new ConcurrentDictionary<int, object>();

        #endregion

        #region Singleton

        private FeaturePermissionService() { }
        private static readonly Lazy<FeaturePermissionService> instance = new Lazy<FeaturePermissionService>(() => new FeaturePermissionService());
        public static FeaturePermissionService Instance
        {
            get => instance.Value;
        }

        #endregion

        #region Public methods

        public Dictionary<int, bool> HasPermissions(PermissionParameterObject param, List<Feature> features, Permission permission)
        {
            Dictionary<Feature, Permission> featurePermission = GetPermissions(param, features);
            return featurePermission.ToDictionary(p => (int)p.Key, p => featurePermission[p.Key].IsValid(permission));
        }
        public Dictionary<Feature, Permission> GetPermissions(PermissionParameterObject param, List<Feature> features)
        {
            if (features.IsNullOrEmpty())
                return new Dictionary<Feature, Permission>();

            PermissionCacheRepository cacheRepository = GetPermissionCacheRepository(param);
            return features.Distinct().ToDictionary(feature => feature, feature => IsValidFeature(feature) ? cacheRepository.GetPermission(param, feature) : Permission.None);
        }
        public Permission GetPermission(PermissionParameterObject param, Feature feature)
        {
            if (!IsValidFeature(feature))
                return Permission.None;

            PermissionCacheRepository cacheRepository = GetPermissionCacheRepository(param);
            return cacheRepository.GetPermission(param, feature);
        }
        public PermissionCacheRepository GetRepository(PermissionParameterObject param)
        {
            return GetPermissionCacheRepository(param);
        }
        public void ClearPermissions(PermissionParameterObject param, FeaturePermissionType type)
        {
            this.ClearPermissionsFromRepository(param, type);
        }

        #endregion

        #region Private methods

        private object GetLicenseLock(int licenseId)
        {
            lock (licensePermissionLocks)
            {
                if (!licensePermissionLocks.TryGetValue(licenseId, out var licenseLock))
                {
                    licenseLock = new object();
                    licensePermissionLocks.TryAdd(licenseId, licenseLock);
                }
                return licenseLock;
            }
        }
        private PermissionCacheRepository GetPermissionCacheRepository(PermissionParameterObject param)
        {
            PermissionCacheRepository cacheRepository;
            CacheConfig config;

            if (!TryLoad())
            {
                object licenseLock = GetLicenseLock(param.LicenseId);
                if (licenseLock != null)
                {
                    lock (licenseLock)
                    {
                        if (!TryLoad())
                        {
                            if (cacheRepository == null)
                                cacheRepository = new PermissionCacheRepository(param.LicenseId);
                            cacheRepository.AddPermissions(param, GetPermissionsFromDb(param.Entities, param));
                            AddPermissionCacheRepositoryToCache(cacheRepository, config);
                        }
                    }
                }
            }
            bool TryLoad()
            {
                cacheRepository = GetPermissionCacheRepositoryFromCache(param.LicenseId, out config);
                return cacheRepository?.IsLoaded(param) ?? false;
            }
            return cacheRepository;
        }
        private void ClearPermissionsFromRepository(PermissionParameterObject param, FeaturePermissionType type)
        {
            PermissionCacheRepository cacheRepository = GetPermissionCacheRepositoryFromCache(param.LicenseId, out CacheConfig config);
            if (cacheRepository != null)
            {
                object licenseLock = GetLicenseLock(param.LicenseId);
                lock (licenseLock)
                {
                    cacheRepository = GetPermissionCacheRepositoryFromCache(param.LicenseId, out config);
                    if (cacheRepository != null)
                    {
                        cacheRepository.ClearPermissions(param, type);
                        AddPermissionCacheRepositoryToCache(cacheRepository, config);
                    }
                }
            }

        }
        private PermissionCacheRepository GetPermissionCacheRepositoryFromCache(int licenseId, out CacheConfig config)
        {
            config = CacheConfig.License(licenseId, 5 * 60);
            string key = config.GetCacheKey((int)BusinessCacheType.FeaturePermissions);
            return BusinessMemoryCache<PermissionCacheRepository>.Get(key, BusinessMemoryDistributionSetting.FullyHybridCache);
        }
        private void AddPermissionCacheRepositoryToCache(PermissionCacheRepository cacheRepository, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.FeaturePermissions);
            BusinessMemoryCache<PermissionCacheRepository>.Set(key, cacheRepository, config.Seconds, BusinessMemoryDistributionSetting.FullyHybridCache);
        }
        private List<FeaturePermissionView> GetPermissionsFromDb(CompEntities entities, PermissionParameterObject param)
        {
            return entities.FeaturePermissionView
                .Where(p =>
                    (p.Id == param.LicenseId && p.Type == (int)FeaturePermissionType.License) ||
                    (p.Id == param.ActorCompanyId && p.Type == (int)FeaturePermissionType.Company) ||
                    (p.Id == param.RoleId && p.Type == (int)FeaturePermissionType.Role)
                    )
                .ToList();
        }
        private bool IsValidFeature(Feature feature)
        {
            return SysDbCache.Instance.SysFeatures.SingleOrDefault(f => f.SysFeatureId == (int)feature && !f.Inactive) != null;
        }

        #endregion
    }

    public class FeatureManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Collection<SoeSiteMapNode> SiteMap;
        private bool siteMapFileChanged;

        #endregion

        #region Ctor

        public FeatureManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SiteMap

        /// <summary>
        /// Loads the Web.sitemap file into a List of SiteMapNodes
        /// </summary>
        public void ImportFeaturesFromSiteMap()
        {
            SiteMap = new Collection<SoeSiteMapNode>();

            //Load Web.sitemap
            XDocument xdoc = XDocument.Load(ConfigSettings.SOE_SERVER_SITEMAP);
            siteMapFileChanged = false;

            FindSiteMapNodes(xdoc.Root, null, 1);

            if (siteMapFileChanged)
                xdoc.Save(ConfigSettings.SOE_SERVER_SITEMAP);
        }

        public int GetFeatureIdFromPath(string path)
        {
            string fileName = System.Web.HttpContext.Current.Server.MapPath("~/Web.sitemap");
            var parent = XDocument.Load(fileName).Root;
            var feature = FindFeatureIdFromPath(parent, path);
            return String.IsNullOrEmpty(feature) ? 0 : Convert.ToInt32(feature);
        }

        public string FindFeatureIdFromPath(XElement parent, string path)
        {
            var result = "0";
            IEnumerable<XElement> elements = from e in parent.Elements()
                                             where e.Name.LocalName == "siteMapNode"
                                             select e;

            foreach (XElement element in elements)
            {
                if (element.Attribute("url").Value == path)
                {
                    result = element.Attribute("featureId").Value;
                    break;
                }
                if (element.HasElements)
                {
                    result = FindFeatureIdFromPath(element, path);
                    if (result != "0")
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Recursive method that traverse a XElement hierarchy and adds it to the siteMapNodes List
        /// </summary>
        /// <param name="parent">The parent XElement</param>
        /// <param name="parentFeatureId">The parent SysFeatureId</param>
        /// <param name="level">The calculated level for the current XElement</param>
        private void FindSiteMapNodes(XElement parent, int? parentFeatureId, int level)
        {
            IEnumerable<XElement> elements = from e in parent.Elements()
                                             where e.Name.LocalName == "siteMapNode"
                                             select e;

            foreach (XElement element in elements)
            {
                //Synch the level in Web.sitemap with the calculated level
                if (Convert.ToInt32(element.Attribute("level").Value, CultureInfo.InvariantCulture) != level)
                {
                    element.SetAttributeValue("level", level);
                    siteMapFileChanged = true;
                }

                SoeSiteMapNode siteMapNode = new SoeSiteMapNode()
                {
                    Title = element.Attribute("title").Value,
                    Url = element.Attribute("url").Value,
                    Level = element.Attribute("level").Value,
                    ReliantOn = element.Attribute("reliantOn").Value,
                    FeatureId = element.Attribute("featureId").Value,
                    Order = element.Attribute("order") != null ? element.Attribute("order").Value : "0",
                };

                int? featureId = null;
                bool isFeature = StringUtility.GetBool(element.Attribute("isFeature").Value);
                if (isFeature)
                {
                    featureId = SynchSiteMapFeature(siteMapNode, element, parentFeatureId);
                    SiteMap.Add(siteMapNode);
                }

                if (element.HasElements)
                    FindSiteMapNodes(element, featureId, level + 1);
            }
        }

        /// <summary>
        /// Synch the SiteMapNode in the SiteMap with the Feature in the SysFeatures table
        /// 
        /// The Attribute Feature in the Web.Sitemap file is as Feature name.
        /// If it is empty, the Attribute Url is used as Feature name.
        /// 
        /// If the Attribute Feature in the Web.Sitemap not is empty, but the Url is used as Feature name
        /// in the SysFeatures table (i.e. added in Web.sitemap after SysFeatures), it is changed to the Attribute Feature name.
        /// 
        /// </summary>
        /// <param name="siteMapNode">The current SiteMapNode</param>
        /// <param name="element">The current XElement</param>
        /// <param name="parentFeatureId">The parent SysFeatureId</param>
        /// <returns>The SysFeatureId</returns>
        private int SynchSiteMapFeature(SoeSiteMapNode siteMapNode, XElement element, int? parentSysFeatureId)
        {
            SysDbCache.Instance.FlushSysFeatures();

            int sysFeatureId;
            if (!Int32.TryParse(siteMapNode.FeatureId, out sysFeatureId))
            {
                //Only synch nodes with empty SysFeatureId
                SysFeature sysFeature = new SysFeature()
                {
                    SysTermId = Convert.ToInt32(siteMapNode.Title, CultureInfo.InvariantCulture),
                    SysTermGroupId = (int)TermGroup.General,
                    Order = Convert.ToInt32(siteMapNode.Order, CultureInfo.InvariantCulture),
                };

                //Add
                if (AddSysFeature(sysFeature, Convert.ToInt32(parentSysFeatureId, CultureInfo.InvariantCulture)).Success)
                {
                    //Save featureId in SiteMap
                    element.SetAttributeValue("featureId", sysFeature.SysFeatureId);
                    siteMapFileChanged = true;
                }

                sysFeatureId = sysFeature.SysFeatureId;
            }

            return sysFeatureId;
        }

        #endregion

        #region Copy

        public ActionResult CopyFeatures(SoeFeatureType featureType, List<int> sysFeatureFilter, int sourceLicenseId, int sourceArticleId, List<int> destinationLicenses, List<int> destinationCompanies, List<int> destinationRoles, bool addNew, bool promoteExisting, bool degradeExisting, bool deleteLeftOvers)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            var tracks = new List<FeatureTrackItem>();

            #region Prereq

            var sourceFeatures = new List<FeatureTemplate>();

            //Convert LicenseFeature/SysXEArticleFeature (depending on SoeFeatureType) to FeatureTemplate objects
            if (featureType == SoeFeatureType.License)
            {
                var sourceLicenseFeatures = GetLicenseFeatures(sourceLicenseId, sysFeatureFilter);
                foreach (var licenseFeature in sourceLicenseFeatures)
                {
                    sourceFeatures.Add(new FeatureTemplate()
                    {
                        FeatureId = licenseFeature.LicenseId,
                        SysFeatureId = licenseFeature.SysFeatureId,
                        SysPermissionId = licenseFeature.SysPermissionId,
                    });
                }
            }
            else if (featureType == SoeFeatureType.SysXEArticle)
            {
                var sourceArticlesFeatures = GetSysXEArticleFeatures(sourceArticleId, sysFeatureFilter);
                foreach (var articleFeature in sourceArticlesFeatures)
                {
                    sourceFeatures.Add(new FeatureTemplate()
                    {
                        FeatureId = articleFeature.SysXEArticleId,
                        SysFeatureId = articleFeature.SysFeatureId,
                        SysPermissionId = articleFeature.SysPermissionId,
                    });
                }
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region LicenseFeature

                        if (!destinationLicenses.IsNullOrEmpty())
                        {
                            foreach (int destinationLicenseId in destinationLicenses)
                            {
                                #region Prereq

                                //Destination License
                                License destinationLicense = LicenseManager.GetLicense(entities, destinationLicenseId);
                                if (destinationLicense == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

                                //Destination LicenseFeatures
                                List<LicenseFeature> destinationLicenseFeatures = GetLicenseFeatures(entities, destinationLicenseId, sysFeatureFilter);

                                #endregion

                                var licenseTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.License,
                                    LicenseId = destinationLicense.LicenseId,
                                    LicenseNr = destinationLicense.LicenseNr,
                                    Name = destinationLicense.Name,
                                };

                                foreach (FeatureTemplate sourceFeature in sourceFeatures)
                                {
                                    LicenseFeature destinationLicenseFeature = destinationLicenseFeatures.FirstOrDefault(i => i.SysFeatureId == sourceFeature.SysFeatureId);
                                    if (destinationLicenseFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            //Add LicenseFeature
                                            destinationLicenseFeature = new LicenseFeature()
                                            {
                                                License = destinationLicense,
                                                SysFeatureId = sourceFeature.SysFeatureId,
                                                SysPermissionId = sourceFeature.SysPermissionId,
                                            };
                                            SetCreatedProperties(destinationLicenseFeature);
                                            entities.LicenseFeature.AddObject(destinationLicenseFeature);
                                            licenseTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sourceFeature.SysPermissionId == destinationLicenseFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            licenseTrack.FeaturesIgnored++;
                                        }
                                        else if (sourceFeature.SysPermissionId > destinationLicenseFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote LicenseFeature permission
                                                destinationLicenseFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationLicenseFeature);
                                                licenseTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sourceFeature.SysPermissionId < destinationLicenseFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sourceFeature.SysPermissionId == (int)Permission.None)
                                            {
                                                //Delete LicenseFeature
                                                entities.DeleteObject(destinationLicenseFeature);
                                                licenseTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade LicenseFeature permission
                                                destinationLicenseFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationLicenseFeature);
                                                licenseTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination LicenseFeatures from collection (to be able to remove leftovers)
                                        destinationLicenseFeatures.Remove(destinationLicenseFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination LicenseFeatures that not existed in source
                                    foreach (LicenseFeature destinationLicenseFeature in destinationLicenseFeatures)
                                    {
                                        entities.DeleteObject(destinationLicenseFeature);
                                        licenseTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(licenseTrack);
                            }
                        }

                        #endregion

                        #region CompanyFeature

                        if (!destinationCompanies.IsNullOrEmpty())
                        {
                            foreach (int destinationCompanyId in destinationCompanies)
                            {
                                #region Prereq

                                //Destination Company
                                Company destinationCompany = CompanyManager.GetCompany(entities, destinationCompanyId, true);
                                if (destinationCompany == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                                //Get all CompanyFeatures once
                                List<CompanyFeature> destinationCompanyFeatures = GetCompanyFeatures(entities, destinationCompanyId, sysFeatureFilter);

                                #endregion

                                var companyTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.Company,
                                    LicenseId = destinationCompany.LicenseId,
                                    ActorCompanyId = destinationCompany.ActorCompanyId,
                                    CompanyNr = destinationCompany.CompanyNr,
                                    Name = destinationCompany.Name,
                                };

                                foreach (FeatureTemplate sourceFeature in sourceFeatures)
                                {
                                    CompanyFeature destinationCompanyFeature = destinationCompanyFeatures.FirstOrDefault(i => i.SysFeatureId == sourceFeature.SysFeatureId);
                                    if (destinationCompanyFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            destinationCompanyFeature = new CompanyFeature()
                                            {
                                                Company = destinationCompany,
                                                SysFeatureId = sourceFeature.SysFeatureId,
                                                SysPermissionId = sourceFeature.SysPermissionId,
                                            };
                                            SetCreatedProperties(destinationCompanyFeature);
                                            entities.CompanyFeature.AddObject(destinationCompanyFeature);
                                            companyTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sourceFeature.SysPermissionId == destinationCompanyFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            companyTrack.FeaturesIgnored++;
                                        }
                                        else if (sourceFeature.SysPermissionId > destinationCompanyFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote CompanyFeature permission
                                                destinationCompanyFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationCompanyFeature);
                                                companyTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sourceFeature.SysPermissionId < destinationCompanyFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sourceFeature.SysPermissionId == (int)Permission.None)
                                            {
                                                //Delete CompanyFeature
                                                entities.DeleteObject(destinationCompanyFeature);
                                                companyTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade CompanyFeature permission
                                                destinationCompanyFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationCompanyFeature);
                                                companyTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination CompanyFeatures from collection (to be able to remove leftovers)
                                        destinationCompanyFeatures.Remove(destinationCompanyFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination LicenseFeatures that not existed in source
                                    foreach (CompanyFeature destinationCompanyFeature in destinationCompanyFeatures)
                                    {
                                        entities.DeleteObject(destinationCompanyFeature);
                                        companyTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(companyTrack);
                            }
                        }

                        #endregion

                        #region RoleFeature

                        if (!destinationRoles.IsNullOrEmpty())
                        {
                            foreach (int destinationRoleId in destinationRoles)
                            {
                                #region Prereq

                                //Destination Role
                                Role destinationRole = RoleManager.GetRole(entities, destinationRoleId);
                                if (destinationRole == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

                                //Get all RoleFeatures once
                                List<RoleFeature> destinationRoleFeatures = GetRoleFeatures(entities, destinationRoleId, sysFeatureFilter);

                                #endregion

                                var roleTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.Role,
                                    LicenseId = destinationRole.Company.LicenseId,
                                    ActorCompanyId = destinationRole.Company.ActorCompanyId,
                                    RoleId = destinationRole.RoleId,
                                    Name = RoleManager.GetRoleNameText(destinationRole),
                                };

                                foreach (FeatureTemplate sourceFeature in sourceFeatures)
                                {
                                    RoleFeature destinationRoleFeature = destinationRoleFeatures.FirstOrDefault(i => i.SysFeatureId == sourceFeature.SysFeatureId);
                                    if (destinationRoleFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            destinationRoleFeature = new RoleFeature()
                                            {
                                                Role = destinationRole,
                                                SysFeatureId = sourceFeature.SysFeatureId,
                                                SysPermissionId = sourceFeature.SysPermissionId,
                                            };
                                            SetCreatedProperties(destinationRoleFeature);
                                            entities.RoleFeature.AddObject(destinationRoleFeature);
                                            roleTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sourceFeature.SysPermissionId == destinationRoleFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            roleTrack.FeaturesIgnored++;
                                        }
                                        else if (sourceFeature.SysPermissionId > destinationRoleFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote CompanyFeature permission
                                                destinationRoleFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationRoleFeature);
                                                roleTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sourceFeature.SysPermissionId < destinationRoleFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sourceFeature.SysPermissionId == (int)Permission.None)
                                            {
                                                //Delete CompanyFeature
                                                entities.DeleteObject(destinationRoleFeature);
                                                roleTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade CompanyFeature permission
                                                destinationRoleFeature.SysPermissionId = sourceFeature.SysPermissionId;
                                                SetModifiedProperties(destinationRoleFeature);
                                                roleTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination RoleFeatures from collection (to be able to remove leftovers)
                                        destinationRoleFeatures.Remove(destinationRoleFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination RoleFeatures that not existed in source
                                    foreach (RoleFeature destinationRoleFeature in destinationRoleFeatures)
                                    {
                                        entities.DeleteObject(destinationRoleFeature);
                                        roleTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(roleTrack);
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
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
                        result.Value = tracks;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult CopyFeatures(Permission permission, List<int> sysFeatureFilter, List<int> destinationLicenses, List<int> destinationCompanies, List<int> destinationRoles, bool addNew, bool promoteExisting, bool degradeExisting, bool deleteLeftOvers)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            var tracks = new List<FeatureTrackItem>();

            #region Prereq

            int sysPermissionId = (int)permission;
            var sourceSysFeatures = GetSysFeatures(sysFeatureFilter);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region LicenseFeature

                        if (!destinationLicenses.IsNullOrEmpty())
                        {
                            //Get all Licenses once
                            List<License> licences = LicenseManager.GetLicenses(entities);

                            foreach (int destinationLicenseId in destinationLicenses)
                            {
                                #region Prereq

                                //Destination License
                                License destinationLicense = licences.FirstOrDefault(i => i.LicenseId == destinationLicenseId);
                                if (destinationLicense == null)
                                    continue;

                                //Destination LicenseFeatures
                                List<LicenseFeature> destinationLicenseFeatures = GetLicenseFeatures(entities, destinationLicenseId, sysFeatureFilter);

                                #endregion

                                var licenseTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.License,
                                    LicenseId = destinationLicense.LicenseId,
                                    LicenseNr = destinationLicense.LicenseNr,
                                    Name = destinationLicense.Name,
                                };

                                foreach (var sysFeature in sourceSysFeatures)
                                {
                                    LicenseFeature destinationLicenseFeature = destinationLicenseFeatures.FirstOrDefault(i => i.SysFeatureId == sysFeature.SysFeatureId);
                                    if (destinationLicenseFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            //Add LicenseFeature
                                            destinationLicenseFeature = new LicenseFeature()
                                            {
                                                License = destinationLicense,
                                                SysFeatureId = sysFeature.SysFeatureId,
                                                SysPermissionId = sysPermissionId,
                                            };
                                            SetCreatedProperties(destinationLicenseFeature);
                                            entities.LicenseFeature.AddObject(destinationLicenseFeature);
                                            licenseTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sysPermissionId == destinationLicenseFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            licenseTrack.FeaturesIgnored++;
                                        }
                                        else if (sysPermissionId > destinationLicenseFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote LicenseFeature permission
                                                destinationLicenseFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationLicenseFeature);
                                                licenseTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sysPermissionId < destinationLicenseFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sysPermissionId == (int)Permission.None)
                                            {
                                                //Delete LicenseFeature
                                                entities.DeleteObject(destinationLicenseFeature);
                                                licenseTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade LicenseFeature permission
                                                destinationLicenseFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationLicenseFeature);
                                                licenseTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination LicenseFeatures from collection (to be able to remove leftovers)
                                        destinationLicenseFeatures.Remove(destinationLicenseFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination LicenseFeatures that not existed in source
                                    foreach (LicenseFeature destinationLicenseFeature in destinationLicenseFeatures)
                                    {
                                        entities.DeleteObject(destinationLicenseFeature);
                                        licenseTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(licenseTrack);
                            }
                        }

                        #endregion

                        #region CompanyFeature

                        if (!destinationCompanies.IsNullOrEmpty())
                        {
                            //Get all Companies once
                            List<Company> companies = CompanyManager.GetCompanies(entities, true);

                            foreach (int destinationCompanyId in destinationCompanies)
                            {
                                #region Prereq

                                //Destination Company
                                Company destinationCompany = companies.FirstOrDefault(i => i.ActorCompanyId == destinationCompanyId);
                                if (destinationCompany == null)
                                    continue;

                                //Get all CompanyFeatures once
                                List<CompanyFeature> destinationCompanyFeatures = GetCompanyFeatures(entities, destinationCompanyId, sysFeatureFilter);

                                #endregion

                                var companyTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.Company,
                                    LicenseId = destinationCompany.LicenseId,
                                    ActorCompanyId = destinationCompany.ActorCompanyId,
                                    CompanyNr = destinationCompany.CompanyNr,
                                    Name = destinationCompany.Name,
                                };

                                foreach (var sysFeature in sourceSysFeatures)
                                {
                                    CompanyFeature destinationCompanyFeature = destinationCompanyFeatures.FirstOrDefault(i => i.SysFeatureId == sysFeature.SysFeatureId);
                                    if (destinationCompanyFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            destinationCompanyFeature = new CompanyFeature()
                                            {
                                                Company = destinationCompany,
                                                SysFeatureId = sysFeature.SysFeatureId,
                                                SysPermissionId = sysPermissionId,
                                            };
                                            SetCreatedProperties(destinationCompanyFeature);
                                            entities.CompanyFeature.AddObject(destinationCompanyFeature);
                                            companyTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sysPermissionId == destinationCompanyFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            companyTrack.FeaturesIgnored++;
                                        }
                                        else if (sysPermissionId > destinationCompanyFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote CompanyFeature permission
                                                destinationCompanyFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationCompanyFeature);
                                                companyTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sysPermissionId < destinationCompanyFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sysPermissionId == (int)Permission.None)
                                            {
                                                //Delete CompanyFeature
                                                entities.DeleteObject(destinationCompanyFeature);
                                                companyTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade CompanyFeature permission
                                                destinationCompanyFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationCompanyFeature);
                                                companyTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination CompanyFeatures from collection (to be able to remove leftovers)
                                        destinationCompanyFeatures.Remove(destinationCompanyFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination LicenseFeatures that not existed in source
                                    foreach (CompanyFeature destinationCompanyFeature in destinationCompanyFeatures)
                                    {
                                        entities.DeleteObject(destinationCompanyFeature);
                                        companyTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(companyTrack);
                            }
                        }

                        #endregion

                        #region RoleFeature

                        if (!destinationRoles.IsNullOrEmpty())
                        {
                            List<Role> roles = RoleManager.GetAllRoles(entities, true);

                            foreach (int destinationRoleId in destinationRoles)
                            {
                                #region Prereq

                                //Destination Role
                                Role destinationRole = roles.FirstOrDefault(i => i.RoleId == destinationRoleId);
                                if (destinationRole == null)
                                    continue;

                                //Get all RoleFeatures once
                                List<RoleFeature> destinationRoleFeatures = GetRoleFeatures(entities, destinationRoleId, sysFeatureFilter);

                                #endregion

                                var roleTrack = new FeatureTrackItem()
                                {
                                    Type = SoeFeatureType.Role,
                                    LicenseId = destinationRole.Company.LicenseId,
                                    ActorCompanyId = destinationRole.Company.ActorCompanyId,
                                    RoleId = destinationRole.RoleId,
                                    Name = RoleManager.GetRoleNameText(destinationRole),
                                };

                                foreach (var sysFeature in sourceSysFeatures)
                                {
                                    RoleFeature destinationRoleFeature = destinationRoleFeatures.FirstOrDefault(i => i.SysFeatureId == sysFeature.SysFeatureId);
                                    if (destinationRoleFeature == null)
                                    {
                                        #region Add

                                        if (addNew)
                                        {
                                            destinationRoleFeature = new RoleFeature()
                                            {
                                                Role = destinationRole,
                                                SysFeatureId = sysFeature.SysFeatureId,
                                                SysPermissionId = sysPermissionId,
                                            };
                                            SetCreatedProperties(destinationRoleFeature);
                                            entities.RoleFeature.AddObject(destinationRoleFeature);
                                            roleTrack.FeaturesAdded++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update / Delete

                                        if (sysPermissionId == destinationRoleFeature.SysPermissionId)
                                        {
                                            //Same permission, do nothing
                                            roleTrack.FeaturesIgnored++;
                                        }
                                        else if (sysPermissionId > destinationRoleFeature.SysPermissionId)
                                        {
                                            if (promoteExisting)
                                            {
                                                //Promote CompanyFeature permission
                                                destinationRoleFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationRoleFeature);
                                                roleTrack.FeaturesPromoted++;
                                            }
                                        }
                                        else if (sysPermissionId < destinationRoleFeature.SysPermissionId && degradeExisting)
                                        {
                                            if (sysPermissionId == (int)Permission.None)
                                            {
                                                //Delete CompanyFeature
                                                entities.DeleteObject(destinationRoleFeature);
                                                roleTrack.FeaturesDeleted++;
                                            }
                                            else
                                            {
                                                //Degrade CompanyFeature permission
                                                destinationRoleFeature.SysPermissionId = sysPermissionId;
                                                SetModifiedProperties(destinationRoleFeature);
                                                roleTrack.FeaturesDegraded++;
                                            }
                                        }

                                        #endregion

                                        //Delete handled destination RoleFeatures from collection (to be able to remove leftovers)
                                        destinationRoleFeatures.Remove(destinationRoleFeature);
                                    }
                                }

                                #region LeftOvers

                                if (deleteLeftOvers)
                                {
                                    //Delete destination RoleFeatures that not existed in source
                                    foreach (RoleFeature destinationRoleFeature in destinationRoleFeatures)
                                    {
                                        entities.DeleteObject(destinationRoleFeature);
                                        roleTrack.FeaturesDeleted++;
                                    }
                                }

                                #endregion

                                tracks.Add(roleTrack);
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
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
                        result.Value = tracks.OrderBy(i => i.LicenseId).ThenBy(i => i.ActorCompanyId).ThenBy(i => i.RoleId).ToList();
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region FeaturePermissionItem

        public Dictionary<int, FeaturePermissionItem> LoadFeaturePermissions(SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {
            var featurePermissionDict = new Dictionary<int, FeaturePermissionItem>();

            //LicenseFeatures
            var licensePermissionDict = GetLicenseFeatures(licenseId).ToDictionary(k => k.SysFeatureId, v => v.SysPermissionId.ToPermission());

            //CompanyFeatures
            var companyPermissionDict = new Dictionary<int, Permission>();
            if (featureType == SoeFeatureType.Company || featureType == SoeFeatureType.Role)
                companyPermissionDict.AddRange(GetCompanyFeatures(actorCompanyId).ToDictionary(k => k.SysFeatureId, v => v.SysPermissionId.ToPermission()));

            //RoleFeatures
            var rolePermissionDict = new Dictionary<int, Permission>();
            if (featureType == SoeFeatureType.Role)
                rolePermissionDict.AddRange(GetRoleFeatures(roleId).ToDictionary(k => k.SysFeatureId, v => v.SysPermissionId.ToPermission()));

            //SysXEArticleFeatures
            var sysXEArticlePermissionDict = new Dictionary<int, Permission>();
            if (featureType == SoeFeatureType.SysXEArticle)
                sysXEArticlePermissionDict.AddRange(GetSysXEArticleFeatures(sysXEArticleId).ToDictionary(k => k.SysFeatureId, v => v.SysPermissionId.ToPermission()));

            //Uses SysDbCache
            foreach (var sysFeature in SysDbCache.Instance.SysFeatures)
            {
                var featurePermissionItem = new FeaturePermissionItem();
                if (licensePermissionDict.ContainsKey(sysFeature.SysFeatureId))
                {
                    featurePermissionItem.LicensePermission = licensePermissionDict[sysFeature.SysFeatureId];
                    if (companyPermissionDict.ContainsKey(sysFeature.SysFeatureId))
                    {
                        featurePermissionItem.CompanyPermission = companyPermissionDict[sysFeature.SysFeatureId];
                        if (rolePermissionDict.ContainsKey(sysFeature.SysFeatureId))
                            featurePermissionItem.RolePermission = rolePermissionDict[sysFeature.SysFeatureId];
                    }
                }
                if (sysXEArticlePermissionDict.ContainsKey(sysFeature.SysFeatureId))
                    featurePermissionItem.SysXEArticlePermission = sysXEArticlePermissionDict[sysFeature.SysFeatureId];

                featurePermissionDict.Add(sysFeature.SysFeatureId, featurePermissionItem);
            }

            return featurePermissionDict;
        }

        #endregion

        #region LicenseFeature

        public List<LicenseFeature> GetLicenseFeatures()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.LicenseFeature.NoTracking();
            return GetLicenseFeatures(entitiesReadOnly);
        }

        public List<LicenseFeature> GetLicenseFeatures(CompEntities entities)
        {
            return entities.LicenseFeature.ToList();
        }

        public List<LicenseFeature> GetLicenseFeatures(int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LicenseFeature.NoTracking();
            return GetLicenseFeatures(entities, licenseId);
        }

        public List<LicenseFeature> GetLicenseFeatures(CompEntities entities, int licenseId)
        {
            return entities.LicenseFeature.Where(lf => lf.LicenseId == licenseId).ToList();
        }

        public List<LicenseFeature> GetLicenseFeatures(int licenseId, List<int> sysFeatureFilter)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LicenseFeature.NoTracking();
            return GetLicenseFeatures(entities, licenseId, sysFeatureFilter);
        }

        public List<LicenseFeature> GetLicenseFeatures(CompEntities entities, int licenseId, List<int> sysFeatureFilter)
        {
            return GetLicenseFeatures(entities, licenseId).Where(lf => sysFeatureFilter.Contains(lf.SysFeatureId)).ToList();
        }

        public LicenseFeature GetLicenseFeature(int licenseId, int featureId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LicenseFeature.NoTracking();
            return GetLicenseFeature(entities, licenseId, featureId);
        }

        private LicenseFeature GetLicenseFeature(CompEntities entities, int licenseId, int featureId)
        {
            return entities.LicenseFeature.FirstOrDefault(lf => lf.LicenseId == licenseId && lf.SysFeatureId == featureId);
        }

        public List<int> GetLicensesWithPermission(int sysFeatureId, int sysPermissionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LicenseFeature.NoTracking();
            return GetLicensesWithPermission(entities, sysFeatureId, sysPermissionId);
        }

        public List<int> GetLicensesWithPermission(CompEntities entities, int sysFeatureId, int sysPermissionId)
        {
            return (from lf in entities.LicenseFeature
                    where lf.SysFeatureId == sysFeatureId &&
                    lf.SysPermissionId == sysPermissionId &&
                    lf.License.State == (int)SoeEntityState.Active
                    select lf.LicenseId).Distinct().ToList();
        }

        public ActionResult CopyLicenseFeatures(int newLicenseId, int templateLicenseId)
        {
            List<LicenseFeature> licenseFeatures = GetLicenseFeatures(templateLicenseId);

            using (CompEntities entities = new CompEntities())
            {
                License license = LicenseManager.GetLicense(entities, newLicenseId);
                if (license == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

                foreach (LicenseFeature licenseFeature in licenseFeatures)
                {
                    LicenseFeature newLicenseFeature = new LicenseFeature()
                    {
                        License = license,
                        SysFeatureId = licenseFeature.SysFeatureId,
                        SysPermissionId = licenseFeature.SysPermissionId,
                    };
                    SetCreatedProperties(newLicenseFeature);
                    entities.LicenseFeature.AddObject(newLicenseFeature);
                }

                var result = SaveChanges(entities);
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(newLicenseId);

                return result;
            }
        }

        public ActionResult AddLicensePermission(LicenseFeature licenseFeature, int licenseId)
        {
            if (licenseFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "LicenseFeature");

            using (CompEntities entities = new CompEntities())
            {
                licenseFeature.License = LicenseManager.GetLicense(entities, licenseId);
                if (licenseFeature.License == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

                var result = AddEntityItem(entities, licenseFeature, "LicenseFeature");
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(licenseId);

                return result;
            }
        }

        public ActionResult SaveLicensePermissions(int licenseId, Dictionary<int, int> featuresDict, Dictionary<int, int> deletedFeaturesDict)
        {
            if (featuresDict == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (CompEntities entities = new CompEntities())
            {
                License license = LicenseManager.GetLicenseAndFeatures(entities, licenseId);
                if (license == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

                List<LicenseFeature> licenseFeatures = GetLicenseFeatures(entities, licenseId);

                #region Delete (must be done first)

                foreach (var pair in deletedFeaturesDict)
                {
                    int sysFeatureId = pair.Key;

                    //Same feature can exists in many articles
                    if (featuresDict.ContainsKey(sysFeatureId))
                        continue;

                    LicenseFeature licenseFeature = licenseFeatures.FirstOrDefault(i => i.SysFeatureId == sysFeatureId);
                    if (licenseFeature != null)
                        entities.DeleteObject(licenseFeature);
                }

                #endregion

                #region Add/Update

                foreach (var pair in featuresDict)
                {
                    int sysFeatureId = pair.Key;
                    int sysPermissionId = pair.Value;

                    LicenseFeature licenseFeature = licenseFeatures.FirstOrDefault(i => i.SysFeatureId == sysFeatureId);
                    if (licenseFeature == null)
                    {
                        #region Add

                        licenseFeature = new LicenseFeature()
                        {
                            SysFeatureId = sysFeatureId,
                            SysPermissionId = sysPermissionId,

                            //References
                            License = license,
                        };
                        SetCreatedProperties(licenseFeature);
                        license.LicenseFeature.Add(licenseFeature);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        if (licenseFeature.SysPermissionId < sysPermissionId)
                            licenseFeature.SysPermissionId = sysPermissionId;

                        #endregion
                    }
                }

                #endregion

                var result = SaveChanges(entities);
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(licenseId);

                return result;
            }
        }

        public ActionResult UpdateLicensePermission(LicenseFeature licenseFeature)
        {
            if (licenseFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "LicenseFeature");

            using (CompEntities entities = new CompEntities())
            {
                LicenseFeature originalLicenseFeature = GetLicenseFeature(entities, licenseFeature.LicenseId, licenseFeature.SysFeatureId);
                if (originalLicenseFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "LicenseFeature");

                //Do not add read permission if License already has modify permission
                if ((licenseFeature.SysPermissionId == (int)Permission.Readonly) &&
                    (originalLicenseFeature.SysPermissionId == (int)Permission.Modify))
                    return new ActionResult((int)ActionResultSave.PermissionCantAddReadIfModifyExist);

                var result = UpdateEntityItem(entities, originalLicenseFeature, licenseFeature, "LicenseFeature");
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(licenseFeature.LicenseId);

                return result;
            }
        }

        public ActionResult DeleteLicensePermission(LicenseFeature licenseFeature)
        {
            if (licenseFeature == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull);

            using (CompEntities entities = new CompEntities())
            {
                LicenseFeature originalLicenseFeature = GetLicenseFeature(entities, licenseFeature.LicenseId, licenseFeature.SysFeatureId);
                if (originalLicenseFeature == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "LicenseFeature");

                var result = DeleteEntityItem(entities, originalLicenseFeature);
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(licenseFeature.LicenseId);

                return result;
            }
        }

        #endregion

        #region CompanyFeature

        public List<CompanyFeature> GetCompanyFeatures(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFeature.NoTracking();
            return GetCompanyFeatures(entities, actorCompanyId);
        }

        public List<CompanyFeature> GetCompanyFeatures(CompEntities entities, int actorCompanyId)
        {
            return entities.CompanyFeature.Where(cf => cf.ActorCompanyId == actorCompanyId).ToList();
        }

        public List<CompanyFeature> GetCompanyFeatures(int actorCompanyId, List<int> sysFeatureFilter)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFeature.NoTracking();
            return GetCompanyFeatures(entities, actorCompanyId, sysFeatureFilter);
        }

        public List<CompanyFeature> GetCompanyFeatures(CompEntities entities, int actorCompanyId, List<int> sysFeatureFilter)
        {
            return GetCompanyFeatures(entities, actorCompanyId).Where(cf => sysFeatureFilter.Contains(cf.SysFeatureId)).ToList();
        }

        public List<int> GetCompaniesByFeature(Feature feature)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFeature.NoTracking();
            return GetCompaniesByFeature(entities, feature);
        }

        public List<int> GetCompaniesByFeature(CompEntities entities, Feature feature)
        {
            return entities.CompanyFeature
                .Where(cf => cf.SysFeatureId == (int)feature)
                .Select(cf => cf.ActorCompanyId).ToList();
        }

        public CompanyFeature GetCompanyFeature(int actorCompanyId, int featureId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFeature.NoTracking();
            return GetCompanyFeature(entities, actorCompanyId, featureId);
        }

        public CompanyFeature GetCompanyFeature(CompEntities entities, int actorCompanyId, int featureId)
        {
            return entities.CompanyFeature.FirstOrDefault(cf => cf.ActorCompanyId == actorCompanyId && cf.SysFeatureId == featureId);
        }

        public List<int> GetCompaniesWithPermission(int sysFeatureId, int sysPermissionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFeature.NoTracking();
            return GetCompaniesWithPermission(entities, sysFeatureId, sysPermissionId);
        }

        public List<int> GetCompaniesWithPermission(CompEntities entities, int sysFeatureId, int sysPermissionId)
        {
            return (from cf in entities.CompanyFeature
                    where cf.SysFeatureId == sysFeatureId &&
                    cf.SysPermissionId == sysPermissionId &&
                    cf.Company.State == (int)SoeEntityState.Active
                    select cf.ActorCompanyId).Distinct().ToList();
        }

        public ActionResult CopyCompanyPermissions(int newCompanyId, int templateCompanyId)
        {
            ActionResult result = new ActionResult();

            List<CompanyFeature> companyFeatures = GetCompanyFeatures(templateCompanyId);
            foreach (CompanyFeature companyFeature in companyFeatures)
            {
                CompanyFeature newCompanyFeature = new CompanyFeature()
                {
                    SysFeatureId = companyFeature.SysFeatureId,
                    SysPermissionId = companyFeature.SysPermissionId
                };

                ActionResult innerResult = AddCompanyPermission(newCompanyFeature, newCompanyId);
                if (!innerResult.Success)
                    result = innerResult;
            }

            if (result.Success)
                EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(newCompanyId);

            return result;
        }

        public ActionResult AddCompanyPermissions(Dictionary<Feature, Permission> featurePermissionsDict, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddCompanyPermissions(entities, featurePermissionsDict, actorCompanyId);
            }
        }

        public ActionResult AddCompanyPermissions(CompEntities entities, Dictionary<Feature, Permission> featurePermissionsDict, int actorCompanyId)
        {
            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            foreach (var pair in featurePermissionsDict)
            {
                CompanyFeature companyFeature = new CompanyFeature()
                {
                    SysFeatureId = (int)pair.Key,
                    SysPermissionId = (int)pair.Value,

                    //Set references
                    Company = company,
                };
                SetCreatedProperties(companyFeature);
                entities.CompanyFeature.AddObject(companyFeature);
            }

            var result = SaveChanges(entities);
            if (result.Success)
                EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(actorCompanyId);

            return result;
        }

        public ActionResult AddCompanyPermission(CompanyFeature companyFeature, int actorCompanyId)
        {
            if (companyFeature == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyFeature");

            using (CompEntities entities = new CompEntities())
            {
                companyFeature.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (companyFeature.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(actorCompanyId);
                return AddEntityItem(entities, companyFeature, "CompanyFeature");
            }
        }

        public ActionResult UpdateCompanyPermission(CompanyFeature companyFeature)
        {
            if (companyFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyFeature");

            using (CompEntities entities = new CompEntities())
            {
                CompanyFeature originalCompanyFeature = GetCompanyFeature(entities, companyFeature.ActorCompanyId, companyFeature.SysFeatureId);
                if (originalCompanyFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CompanyFeature");

                //Do not add read permission if Company already has modify permission
                if ((companyFeature.SysPermissionId == (int)Permission.Readonly) &&
                    (originalCompanyFeature.SysPermissionId == (int)Permission.Modify))
                    return new ActionResult((int)ActionResultSave.PermissionCantAddReadIfModifyExist);

                var result = UpdateEntityItem(entities, originalCompanyFeature, companyFeature, "CompanyFeature");
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(companyFeature.ActorCompanyId);

                return result;
            }
        }

        public ActionResult DeleteCompanyPermission(CompanyFeature companyFeature)
        {
            if (companyFeature == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyFeature");

            using (CompEntities entities = new CompEntities())
            {
                CompanyFeature originalCompanyFeature = GetCompanyFeature(entities, companyFeature.ActorCompanyId, companyFeature.SysFeatureId);
                if (originalCompanyFeature == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyFeature");

                var result = DeleteEntityItem(entities, originalCompanyFeature);
                if (result.Success)
                    EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(companyFeature.ActorCompanyId);

                return result;
            }
        }

        #endregion

        #region RoleFeature

        public List<RoleFeature> GetRoleFeatures(int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFeature.NoTracking();
            return GetRoleFeatures(entities, roleId);
        }

        public List<RoleFeature> GetRoleFeatures(CompEntities entities, int roleId)
        {
            return entities.RoleFeature.Where(rf => rf.RoleId == roleId).ToList();
        }

        public List<RoleFeature> GetRoleFeatures(int roleId, List<int> sysFeatureFilter)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFeature.NoTracking();
            return GetRoleFeatures(entities, roleId, sysFeatureFilter);
        }

        public List<RoleFeature> GetRoleFeatures(CompEntities entities, int roleId, List<int> sysFeatureFilter)
        {
            return GetRoleFeatures(entities, roleId).Where(rf => sysFeatureFilter.Contains(rf.SysFeatureId)).ToList();
        }

        public List<RoleFeature> GetRoleFeaturesForCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFeature.NoTracking();
            return GetRoleFeaturesForCompany(entities, actorCompanyId);
        }

        public List<RoleFeature> GetRoleFeaturesForCompany(CompEntities entities, int actorCompanyId)
        {
            return entities.RoleFeature.Where(rf => rf.Role.ActorCompanyId == actorCompanyId).ToList();
        }

        public RoleFeature GetRoleFeature(int roleId, int featureId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFeature.NoTracking();
            return GetRoleFeature(entities, roleId, featureId);
        }

        public RoleFeature GetRoleFeature(CompEntities entities, int roleId, int featureId)
        {
            return entities.RoleFeature.FirstOrDefault(rf => rf.RoleId == roleId && rf.SysFeatureId == featureId);
        }

        public List<int> GetRolesWithPermission(int sysFeatureId, int sysPermissionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFeature.NoTracking();
            return GetRolesWithPermission(entities, sysFeatureId, sysPermissionId);
        }

        public List<int> GetRolesWithPermission(CompEntities entities, int sysFeatureId, int sysPermissionId)
        {
            return (from rf in entities.RoleFeature
                    where rf.SysFeatureId == sysFeatureId &&
                    rf.SysPermissionId == sysPermissionId &&
                    rf.Role.State == (int)SoeEntityState.Active
                    select rf.RoleId).Distinct().ToList();
        }

        public ActionResult AddRolePermission(RoleFeature roleFeature, int roleId)
        {
            if (roleFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RoleFeature");

            using (CompEntities entities = new CompEntities())
            {
                roleFeature.Role = RoleManager.GetRole(entities, roleId);
                if (roleFeature.Role == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

                int toValue = roleFeature.SysPermissionId;

                ActionResult result = AddEntityItem(entities, roleFeature, "RoleFeature");
                if (result.Success)
                {
                    EvoFeatureCacheInvalidationConnector.InvalidateRoleCache(roleId);
                    TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonInsert, TermGroup_TrackChangesAction.Insert, SoeEntityType.Role, roleId, SoeEntityType.RoleFeature, roleFeature.SysFeatureId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_Permission, 0, toValue, string.Empty, GetText(toValue == (int)Permission.Modify ? 1080 : 1077, 1)));
                }

                return result;
            }
        }

        public ActionResult AddRolePermissions(CompEntities entities, List<RoleFeature> roleFeatures)
        {
            ActionResult result = new ActionResult();

            if (roleFeatures == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RoleFeature");

            foreach (var roleFeature in roleFeatures)
            {
                if (roleFeature.Role == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

                int toValue = roleFeature.SysPermissionId;

                SetCreatedProperties(roleFeature);
                entities.RoleFeature.AddObject(roleFeature);

                if (result.Success)
                    TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonInsert, TermGroup_TrackChangesAction.Insert, SoeEntityType.Role, roleFeature.Role.RoleId, SoeEntityType.RoleFeature, roleFeature.SysFeatureId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_Permission, 0, toValue, string.Empty, GetText(toValue == (int)Permission.Modify ? 1080 : 1077, 1)));
                else
                    return result;
            }

            if (result.Success && roleFeatures.Count > 0)
                EvoFeatureCacheInvalidationConnector.InvalidateRoleCache(roleFeatures.First().RoleId);

            return result;
        }

        public ActionResult UpdateRolePermission(RoleFeature roleFeature)
        {
            if (roleFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RoleFeature");

            using (CompEntities entities = new CompEntities())
            {
                RoleFeature originalRoleFeature = GetRoleFeature(entities, roleFeature.RoleId, roleFeature.SysFeatureId);
                if (originalRoleFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "RoleFeature");

                //Do not add read permission if Role already has modify permission
                if ((roleFeature.SysPermissionId == (int)Permission.Readonly) &&
                    (originalRoleFeature.SysPermissionId == (int)Permission.Modify))
                    return new ActionResult((int)ActionResultSave.PermissionCantAddReadIfModifyExist);

                int fromValue = originalRoleFeature.SysPermissionId;
                int toValue = roleFeature.SysPermissionId;

                ActionResult result = UpdateEntityItem(entities, originalRoleFeature, roleFeature, "RoleFeature");
                if (result.Success)
                {
                    EvoFeatureCacheInvalidationConnector.InvalidateRoleCache(roleFeature.RoleId);
                    TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Role, roleFeature.RoleId, SoeEntityType.RoleFeature, roleFeature.SysFeatureId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_Permission, fromValue.ToString(), toValue.ToString(), GetText(fromValue == (int)Permission.Modify ? 1080 : 1077, 1), GetText(toValue == (int)Permission.Modify ? 1080 : 1077, 1)));
                }

                return result;
            }
        }

        public ActionResult DeleteRolePermission(RoleFeature roleFeature)
        {
            if (roleFeature == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "RoleFeature");

            using (CompEntities entities = new CompEntities())
            {
                RoleFeature originalRoleFeature = GetRoleFeature(entities, roleFeature.RoleId, roleFeature.SysFeatureId);
                if (originalRoleFeature == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "RoleFeature");

                int fromValue = originalRoleFeature.SysPermissionId;

                ActionResult result = DeleteEntityItem(entities, originalRoleFeature);
                if (result.Success)
                {
                    EvoFeatureCacheInvalidationConnector.InvalidateRoleCache(roleFeature.RoleId);
                    TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, TermGroup_TrackChangesActionMethod.CommonDelete, TermGroup_TrackChangesAction.Delete, SoeEntityType.Role, roleFeature.RoleId, SoeEntityType.RoleFeature, roleFeature.SysFeatureId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Role_Permission, fromValue.ToString(), string.Empty, GetText(fromValue == (int)Permission.Modify ? 1080 : 1077, 1), string.Empty));
                }

                return result;
            }
        }

        #endregion

        #region PermissionCacheRepository

        public (PermissionParameterObject Param, PermissionCacheRepository Repository) GetPermissionRepository(int licenseId, int actorCompanyId, int roleId, CompEntities entities = null)
        {
            var param = CreateAndValidatePermissionParameterObject(licenseId, actorCompanyId, roleId, entities);
            return param.IsValid() ? (param, FeaturePermissionService.Instance.GetRepository(param)) : (param, null);
        }
        public Permission GetRolePermission(Feature feature, int licenseId, int actorCompanyId, int roleId, CompEntities entities = null)
        {
            var param = CreateAndValidatePermissionParameterObject(licenseId, actorCompanyId, roleId, entities);
            return param.IsValid() ? FeaturePermissionService.Instance.GetPermission(param, feature) : Permission.None;
        }
        public Dictionary<int, bool> HasRolePermissions(List<Feature> features, Permission permission, int licenseId, int actorCompanyId, int roleId, CompEntities entities = null)
        {
            var param = CreateAndValidatePermissionParameterObject(licenseId, actorCompanyId, roleId, entities);
            return param.IsValid() ? FeaturePermissionService.Instance.HasPermissions(param, features, permission) : new Dictionary<int, bool>();
        }
        public bool HasRolePermission(Feature feature, Permission permission, int roleId, int actorCompanyId, int licenseId = 0, CompEntities entities = null)
        {
            if (feature == Feature.None || permission == Permission.None)
                return true;
            Permission rolePermission = GetRolePermission(feature, licenseId, actorCompanyId, roleId, entities);
            return rolePermission.IsValid(permission);
        }
        public bool HasAnyRolePermission(Feature feature, Permission permission, int actorCompanyId, int licenseId, List<Role> roles = null, CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (entities == null)
                entities = entitiesReadOnly;
            if (roles == null)
                roles = RoleManager.GetRolesByCompany(entities, actorCompanyId);
            return roles?.Exists(role => HasRolePermission(feature, permission, role.RoleId, actorCompanyId, licenseId, entities)) ?? false;
        }

        public void ClearLicensePermissionsFromCache(int licenseId)
        {
            var param = CreatePermissionParameterObject(licenseId, 0, 0);
            FeaturePermissionService.Instance.ClearPermissions(param, FeaturePermissionType.License);
        }
        public void ClearCompanyPermissionsFromCache(int licenseId, int actorCompanyId)
        {
            var param = CreatePermissionParameterObject(licenseId, actorCompanyId, 0);
            FeaturePermissionService.Instance.ClearPermissions(param, FeaturePermissionType.Company);
        }
        public void ClearRolePermissionsFromCache(int licenseId, int actorCompanyId, int roleId)
        {
            var param = CreatePermissionParameterObject(licenseId, actorCompanyId, roleId);
            FeaturePermissionService.Instance.ClearPermissions(param, FeaturePermissionType.Role);
        }
        private PermissionParameterObject CreatePermissionParameterObject(int licenseId, int actorCompanyId, int roleId, CompEntities entities = null)
        {
            return new PermissionParameterObject(entities ?? CompEntitiesProvider.LeaseReadOnlyContext(), licenseId, actorCompanyId, roleId, base.parameterObject?.Thread);
        }
        private PermissionParameterObject CreateAndValidatePermissionParameterObject(int licenseId, int actorCompanyId, int roleId, CompEntities entities = null)
        {
            var param = CreatePermissionParameterObject(licenseId, actorCompanyId, roleId, entities);
            if (!ValidateParam(ref param))
                return null;
            return param;
        }
        private bool ValidateParam(ref PermissionParameterObject param)
        {
            if (param == null)
                return false;
            if (param.IsValid())
                return true;

            if (param.ActorCompanyId == 0 || param.LicenseId == 0)
            {
                if (param.ActorCompanyId > 0 && param.ActorCompanyId == parameterObject?.ActorCompanyId)
                {
                    param = new PermissionParameterObject(param.Entities, parameterObject.LicenseId, param.ActorCompanyId, param.RoleId, param.Thread);
                }
                else
                {
                    Company company = CompanyManager.GetCompanyByRoleId(param.Entities, param.RoleId);
                    if (company != null)
                        param = new PermissionParameterObject(param.Entities, company.LicenseId, company.ActorCompanyId, param.RoleId, param.Thread);
                }
            }

            if (param.ActorCompanyId > 0 && param.LicenseId > 0)
            {
                param.SetValid();
                return true;
            }
            else
                return false;
        }

        #endregion

        #region SysFeature

        public List<SysFeature> GetSysFeatures()
        {
            using (var sysEntities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return sysEntities.SysFeature.AsNoTracking().Include("SysFeature2").OrderBy(i => i.Order).ToList();
                }
            }
        }

        public List<SysFeatureDTO> GetSysFeatures(List<SoeXeArticle> xeArticles)
        {
            List<SysXEArticleFeature> sysXEArticleFeatures = GetSysXEArticleFeatures(xeArticles);
            return GetSysFeatures(sysXEArticleFeatures);
        }

        public List<SysFeatureDTO> GetSysFeatures(List<SysXEArticleFeature> sysXEArticleFeatures)
        {
            List<int> sysFeatureIdFilter = sysXEArticleFeatures.Select(i => i.SysFeatureId).ToList();
            return GetSysFeatures(sysFeatureIdFilter);
        }

        public List<SysFeatureDTO> GetSysFeatures(List<int> sysFeatureIdFilter)
        {
            List<SysFeatureDTO> sysFeatures = new List<SysFeatureDTO>();

            //Uses SysDbCache
            foreach (var sysFeature in SysDbCache.Instance.SysFeatures)
            {
                if (sysFeatureIdFilter.Contains(sysFeature.SysFeatureId))
                    sysFeatures.Add(sysFeature);
            }

            return sysFeatures;
        }

        public int GetSysFeatureTermId(Feature feature)
        {
            //Uses SysDbCache
            return SysDbCache.Instance.SysFeatures.FirstOrDefault(s => s.SysFeatureId == (int)feature)?.SysTermId ?? 0;
        }

        public List<SysFeatureDTO> GetSysFeatureRoots()
        {
            //Uses SysDbCache
            return (from sf in SysDbCache.Instance.SysFeatures
                    where sf.ParentFeatureId == null
                    orderby sf.Order ascending
                    select sf).ToList();
        }

        public SysFeature GetSysFeature(int sysFeatureId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysFeature(sysEntitiesReadOnly, sysFeatureId);
        }

        private SysFeature GetSysFeature(SOESysEntities entities, int sysFeatureId)
        {
            return entities.SysFeature.FirstOrDefault(sf => sf.SysFeatureId == sysFeatureId);
        }

        public ActionResult AddSysFeature(SysFeature sysFeature, int parentSysFeatureId)
        {
            if (sysFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysFeature");

            using (SOESysEntities entities = new SOESysEntities())
            {
                sysFeature.SysFeature2 = GetSysFeature(entities, parentSysFeatureId);
                return SaveChanges(entities);
            }
        }

        public List<Feature> GetFeaturesThatRequireValidAccountYear()
        {
            return new List<Feature>()
            { 
                #region System

                #endregion

                #region Manage

                #endregion

                #region Economy

                Feature.Economy_Import_Sie,
                Feature.Economy_Import_Sie_Account,
                Feature.Economy_Import_Sie_Voucher,
                Feature.Economy_Import_Sie_AccountBalance,
                Feature.Economy_Accounting_Vouchers_Edit,
                Feature.Economy_Customer_Invoice,
                Feature.Economy_Customer_Invoice_Invoices_Edit,
                Feature.Economy_Customer_Invoice_Status,
                Feature.Economy_Customer_Payment,
                Feature.Economy_Customer_Payment_Payments_Edit,
                Feature.Economy_Supplier_Invoice,
                Feature.Economy_Supplier_Invoice_Invoices_Edit,
                Feature.Economy_Supplier_Invoice_Status,
                Feature.Economy_Supplier_Payment,
                Feature.Economy_Supplier_Payment_Payments_Edit,

                #endregion

                #region Billing

                Feature.Billing_Contract_Contracts_Edit,
                Feature.Billing_Contract_Status,
                Feature.Billing_Contract_Groups_Edit,
                Feature.Billing_Invoice_Invoices_Edit,
                Feature.Billing_Invoice_Status,
                Feature.Billing_Offer_Offers_Edit,
                Feature.Billing_Offer_Status,
                Feature.Billing_Order_Orders_Edit,
                Feature.Billing_Order_Status,

                #endregion

                #region Time

                #endregion

                #region Communication

                #endregion
            };
        }

        public List<Feature> GetFeaturesInvalidForTemplateCompany()
        {
            return new List<Feature>()
            { 
                #region System

                #endregion

                #region Manage

                #endregion

                #region Economy

                Feature.Economy_Distribution_Reports_Selection_Download,
                Feature.Economy_Distribution_Reports_Selection_Preview,
                Feature.Economy_Export_Sie,
                Feature.Economy_Export_Sie_Type1,
                Feature.Economy_Export_Sie_Type2,
                Feature.Economy_Export_Sie_Type3,
                Feature.Economy_Export_Sie_Type4,
                Feature.Economy_Import_Sie,
                Feature.Economy_Import_Sie_Account,
                Feature.Economy_Import_Sie_Voucher,
                Feature.Economy_Import_Sie_AccountBalance,
                Feature.Economy_Accounting_Vouchers_Edit,
                Feature.Economy_Customer_Invoice,
                Feature.Economy_Customer_Invoice_Invoices_Edit,
                Feature.Economy_Customer_Invoice_Status,
                Feature.Economy_Customer_Payment,
                Feature.Economy_Customer_Payment_Payments_Edit,
                Feature.Economy_Supplier_Invoice,
                Feature.Economy_Supplier_Invoice_Invoices_Edit,
                Feature.Economy_Supplier_Invoice_Status,
                Feature.Economy_Supplier_Payment,
                Feature.Economy_Supplier_Payment_Payments_Edit,

                #endregion

                #region Billing

                Feature.Billing_Distribution_Reports_Selection_Download,
                Feature.Billing_Distribution_Reports_Selection_Preview,
                Feature.Billing_Contract_Contracts_Edit,
                Feature.Billing_Contract_Status,
                Feature.Billing_Contract_Groups_Edit,
                Feature.Billing_Offer_Offers_Edit,
                Feature.Billing_Offer_Status,
                Feature.Billing_Order_Orders_Edit,
                Feature.Billing_Order_Status,
                Feature.Billing_Invoice_Invoices_Edit,
                Feature.Billing_Invoice_Status,

                #endregion

                #region Time

                Feature.Time_Distribution_Reports_Selection_Download,
                Feature.Time_Distribution_Reports_Selection_Preview,
                Feature.Time_Export_Salary,
                Feature.Time_Schedule_Templates_Edit,
                Feature.Time_Schedule_Placement,
                Feature.Time_Time_Attest,
                Feature.Time_Time_Attest_Edit,
                Feature.Time_Time_Attest_RestoreToSchedule,
                Feature.Time_Time_Attest_EditSchedule,
                Feature.Time_Time_AttestUser,
                Feature.Time_Time_AttestUser_Edit,
                Feature.Time_Time_TimeSalarySpecification,
                Feature.Time_Project_Edit,
                Feature.Time_Project_Invoice_Edit,

                #endregion

                #region Communication

                #endregion
            };
        }

        public List<Feature> GetFeaturesThatRequireSupportCompany()
        {
            return new List<Feature>()
            { 
                #region System

                Feature.Manage_System,

                #endregion

                #region Manage

                Feature.Manage_Contracts,
                Feature.Manage_Contracts_Edit,
                Feature.Manage_Contracts_Edit_Permission,

                #endregion

                #region Economy

                Feature.Economy_Distribution_SysTemplates,
                Feature.Economy_Distribution_SysTemplates_Edit,

                #endregion

                #region Billing

                Feature.Billing_Distribution_SysTemplates,
                Feature.Billing_Distribution_SysTemplates_Edit,

                #endregion

                #region Time

                Feature.Time_Distribution_SysTemplates,
                Feature.Time_Distribution_SysTemplates_Edit,

                #endregion

                #region Communication

                #endregion
            };
        }

        public List<Feature> GetFeaturesThatIsValidForSupportLogin()
        {
            return new List<Feature>()
            { 
                #region System

                #endregion

                #region Manage

                Feature.Manage_Support,
                Feature.Manage_Support_Logs,
                Feature.Manage_Support_Logs_Edit,
                Feature.Manage_Support_Logs_System,
                Feature.Manage_Support_Logs_License,
                Feature.Manage_Support_Logs_Company,
                Feature.Manage_Support_Logs_Role,
                Feature.Manage_Support_Logs_User,
                Feature.Manage_Support_Logs_Machine,

                #endregion

                #region Economy

                #endregion

                #region Billing
                #endregion

                #region Time

                #endregion

                #region Communication

                #endregion
            };
        }

        public Permission? CheckFeatureValidity(Feature feature, bool isTemplateCompany, bool isInvalidAccountYear, bool isSupportCompany, bool isSupportCompanyLoggedIn, int userId)
        {
            User user = UserManager.GetUser(userId);
            UserDTO userDTO = user?.ToDTO();
            return CheckFeatureValidity(feature, isTemplateCompany, isInvalidAccountYear, isSupportCompany, isSupportCompanyLoggedIn, userDTO);
        }

        public Permission? CheckFeatureValidity(Feature feature, bool isTemplateCompany, bool isInvalidAccountYear, bool isSupportCompany, bool isSupportCompanyLoggedIn, UserDTO user)
        {
            //Check change password
            if ((feature == Feature.Manage_Users_Edit || feature == Feature.Manage_Users_Edit_Password) && user != null && user.ChangePassword)
                return Permission.Modify;

            //Check template Company
            if (isTemplateCompany && FeatureManager.GetFeaturesInvalidForTemplateCompany().Contains(feature))
                return Permission.None;

            //Check AccountYear
            if (isInvalidAccountYear && FeatureManager.GetFeaturesThatRequireValidAccountYear().Contains(feature))
                return Permission.None;

            //Support License check
            if (!isSupportCompany && FeatureManager.GetFeaturesThatRequireSupportCompany().Contains(feature))
                return Permission.None;

            //Extend permission for support login
            if (isSupportCompanyLoggedIn && FeatureManager.GetFeaturesThatIsValidForSupportLogin().Contains(feature))
                return Permission.Modify;

            return null;
        }

        #endregion

        #region SysPermission

        public List<SysPermission> GetSysPermissions()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysPermission.ToList();
        }

        #endregion

        #region SysXEArticleFeature

        public List<SysXEArticleFeature> GetSysXEArticleFeatures()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from xe in sysEntitiesReadOnly.SysXEArticleFeature
                    orderby xe.SysXEArticleId
                    select xe).ToList();
        }

        public List<SysXEArticleFeature> GetSysXEArticleFeatures(List<SoeXeArticle> xeArticles)
        {
            List<SysXEArticleFeature> features = new List<SysXEArticleFeature>();

            foreach (var xeArticle in xeArticles)
            {
                features.AddRange(GetSysXEArticleFeatures(xeArticle));
            }

            return features;
        }

        public List<SysXEArticleFeature> GetSysXEArticleFeatures(SoeXeArticle xeArticle)
        {
            return GetSysXEArticleFeatures((int)xeArticle);
        }

        public List<SysXEArticleFeature> GetSysXEArticleFeatures(int sysXEArticleId)
        {
            //Uses SysDbCache
            return (from xe in SysDbCache.Instance.SysXEArticleFeatures
                    where xe.SysXEArticleId == sysXEArticleId
                    select xe).ToList();
        }

        public List<SysXEArticleFeature> GetSysXEArticleFeatures(int sysXEArticleId, List<int> sysFeatureFilter)
        {
            var sysXEArticleFeatures = new List<SysXEArticleFeature>();

            var sysXEArticleFeaturesAll = GetSysXEArticleFeatures(sysXEArticleId);
            foreach (var sysXEArticleFeature in sysXEArticleFeaturesAll)
            {
                if (sysFeatureFilter.Contains(sysXEArticleFeature.SysFeatureId))
                    sysXEArticleFeatures.Add(sysXEArticleFeature);
            }

            return sysXEArticleFeatures;
        }

        public Dictionary<int, int> GetSysXEArticleFeaturesDict(List<int> sysXEArticleIds)
        {
            var dict = new Dictionary<int, int>();

            //All SysXEArticlesFeatures
            List<SysXEArticleFeature> allSysXEArticleFeatures = GetSysXEArticleFeatures();

            //All SysXEArticles to merge features for
            foreach (int sysXEArticleId in sysXEArticleIds)
            {
                //All SysXEArticlesFeatures for currrent SysXEArticle
                List<SysXEArticleFeature> sysXEArticleFeatures = allSysXEArticleFeatures.Where(i => i.SysXEArticleId == sysXEArticleId).OrderBy(i => i.SysFeatureId).ToList();
                foreach (SysXEArticleFeature sysXEArticleFeature in sysXEArticleFeatures)
                {
                    if (dict.ContainsKey(sysXEArticleFeature.SysFeatureId))
                    {
                        //Update if current feature has higher permission than existing
                        int sysPermissionId = dict[sysXEArticleFeature.SysFeatureId];
                        if (sysPermissionId < sysXEArticleFeature.SysPermissionId)
                            dict[sysXEArticleFeature.SysFeatureId] = sysXEArticleFeature.SysPermissionId;
                    }
                    else
                    {
                        //Add new feature
                        dict.Add(sysXEArticleFeature.SysFeatureId, sysXEArticleFeature.SysPermissionId);
                    }
                }
            }

            //Sort dictionary
            var sortedDict = new Dictionary<int, int>();
            foreach (var pair in dict.OrderBy(i => i.Key))
            {
                if (!sortedDict.ContainsKey(pair.Key))
                    sortedDict.Add(pair.Key, pair.Value);
            }
            return sortedDict;
        }

        public SysXEArticleFeature GetSysXEArticleFeature(int sysXEArticleId, Feature feature)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysXEArticleFeature(sysEntitiesReadOnly, sysXEArticleId, (int)feature);
        }

        public SysXEArticleFeature GetSysXEArticleFeature(SOESysEntities entities, int sysXEArticleId, int featureId)
        {
            return (from xe in entities.SysXEArticleFeature
                    where ((xe.SysXEArticleId == sysXEArticleId) &&
                    (xe.SysFeatureId == featureId))
                    select xe).FirstOrDefault<SysXEArticleFeature>();
        }

        public ActionResult AddSysXEArticlePermission(SysXEArticleFeature sysXEArticleFeatureFromGUI, int sysXEArticleId, int sysFeatureId)
        {
            if (sysXEArticleFeatureFromGUI == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysXEArticleFeature");

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysXEArticleFeature sysXEArticleFeature = entities.SysXEArticleFeature.FirstOrDefault(f => f.SysXEArticleId == sysXEArticleFeatureFromGUI.SysXEArticleId && f.SysFeatureId == sysXEArticleFeatureFromGUI.SysFeatureId);
                sysXEArticleFeature.SysXEArticle = GetSysXEArticle(entities, sysXEArticleId);
                if (sysXEArticleFeature.SysXEArticle == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysXEArticle");

                sysXEArticleFeature.SysFeature = GetSysFeature(entities, sysFeatureId);
                if (sysXEArticleFeature.SysFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysFeature");

                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateSysXEArticlePermission(SysXEArticleFeature sysXEArticleFeature)
        {
            if (sysXEArticleFeature == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysXEArticleFeature");

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysXEArticleFeature originalSysXEArticleFeature = GetSysXEArticleFeature(entities, sysXEArticleFeature.SysXEArticleId, sysXEArticleFeature.SysFeatureId);
                if (originalSysXEArticleFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysXEArticleFeature");

                //Do not add read permission if Role already has modify permission
                if ((sysXEArticleFeature.SysPermissionId == (int)Permission.Readonly) &&
                    (originalSysXEArticleFeature.SysPermissionId == (int)Permission.Modify))
                    return new ActionResult((int)ActionResultSave.PermissionCantAddReadIfModifyExist);

                originalSysXEArticleFeature.SysFeatureId = sysXEArticleFeature.SysFeatureId;
                originalSysXEArticleFeature.SysPermissionId = sysXEArticleFeature.SysPermissionId;
                originalSysXEArticleFeature.SysXEArticleId = sysXEArticleFeature.SysXEArticleId;
                SetModifiedPropertiesOnEntity(originalSysXEArticleFeature);
                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteSysXEArticlePermission(SysXEArticleFeature sysXEArticleFeature)
        {
            if (sysXEArticleFeature == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "SysXEArticleFeature");

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysXEArticleFeature originalSysXEArticleFeature = GetSysXEArticleFeature(entities, sysXEArticleFeature.SysXEArticleId, sysXEArticleFeature.SysFeatureId);
                if (originalSysXEArticleFeature == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysXEArticleFeature");

                entities.SysXEArticleFeature.Remove(originalSysXEArticleFeature);
                return SaveChanges(entities);
            }
        }

        #endregion

        #region SysXEArticle

        public List<SysXEArticle> GetSysXEArticles()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from xe in sysEntitiesReadOnly.SysXEArticle
                           .Include("SysXEArticleFeature")
                    where !xe.Inactive
                    orderby xe.SortOrder, xe.ModuleGroup, xe.Name ascending
                    select xe).ToList<SysXEArticle>();
        }

        public SysXEArticle GetSysXEArticle(int sysXEArticleId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysXEArticle(sysEntitiesReadOnly, sysXEArticleId);
        }

        public SysXEArticle GetSysXEArticle(SOESysEntities entities, int sysXEArticleId)
        {
            return (from xe in entities.SysXEArticle
                    where xe.SysXEArticleId == sysXEArticleId &&
                    !xe.Inactive
                    orderby xe.ArticleNr ascending
                    select xe).FirstOrDefault();
        }

        #endregion
    }
}
