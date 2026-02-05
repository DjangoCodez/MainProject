using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.PaymentIO.Lb;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.DataCache
{
    public sealed class SysDbCache
    {
        #region Variables
        #endregion

        #region Singleton

        private SysDbCache() { }
        private static readonly Lazy<SysDbCache> instance = new Lazy<SysDbCache>(() => new SysDbCache());
        public static SysDbCache Instance
        {
            get => instance.Value;
        }

        #endregion

        #region Public methods

        public void FlushAll()
        {
            MethodInfo[] methodInfos = typeof(SysDbCache).GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo.Name != "FlushAll" && methodInfo.Name.StartsWith("Flush"))
                    methodInfo.Invoke(this, null);
            }
        }



        #endregion

        #region SysServiceConnector

        static readonly object sysServiceUrlsLock = new object();
        private List<string> sysServiceUrls;
        public List<string> SysServiceUrls
        {
            get
            {
                if (this.sysServiceUrls == null)
                {
                    lock (sysServiceUrlsLock)
                    {
                        var m = new SettingManager(null);
                        this.sysServiceUrls = m.GetSysServiceUrisFromSettings();
                    }
                }
                return this.sysServiceUrls;
            }
        }
        public void FlushSysServiceUrls()
        {
            this.sysServiceUrls = null;
        }

        public void SetSysServiceUrls(string url)
        {
            if (!string.IsNullOrEmpty(url) && url.Length > 8 && url.ToLower().Contains("https"))
            {
                this.sysServiceUrls = new List<string>() { url };
            }
        }

        #endregion

        #region SysAccountStd

        static readonly object sysAccountStdsLock = new object();
        private List<SysAccountStd> sysAccountStds;
        public List<SysAccountStd> SysAccountStds
        {
            get
            {
                if (this.sysAccountStds == null)
                {
                    lock (sysAccountStdsLock)
                    {
                        var m = new AccountManager(null);
                        this.sysAccountStds = m.GetSysAccountStds();
                    }
                }
                return this.sysAccountStds;
            }
        }
        public void FlushSysAccountStds()
        {
            this.sysAccountStds = null;
        }

        #endregion

        #region SysAccountStdType

        static readonly object sysAccountStdTypesLock = new object();
        private List<SysAccountStdType> sysAccountStdTypes;
        public List<SysAccountStdType> SysAccountStdTypes
        {
            get
            {
                if (this.sysAccountStdTypes == null)
                {
                    lock (sysAccountStdTypesLock)
                    {
                        var m = new AccountManager(null);
                        this.sysAccountStdTypes = m.GetSysAccountStdTypes();
                    }
                }
                return this.sysAccountStdTypes;
            }
        }
        public void FlushSysAccountStdTypes()
        {
            this.sysAccountStdTypes = null;
        }

        #endregion

        #region SysAccountSruCode

        static readonly object sysAccountSruCodesLock = new object();
        private List<SysAccountSruCode> sysAccountSruCodes;
        public List<SysAccountSruCode> SysAccountSruCodes
        {
            get
            {
                if (this.sysAccountSruCodes == null)
                {
                    lock (sysAccountSruCodesLock)
                    {
                        var m = new AccountManager(null);
                        this.sysAccountSruCodes = m.GetSysAccountSruCodes();
                    }
                }
                return this.sysAccountSruCodes;
            }
        }
        public void FlushSysAccountSruCodes()
        {
            this.sysAccountSruCodes = null;
        }

        #endregion

        #region SysVatAccount

        static readonly object sysVatAccountsLock = new object();
        private List<SysVatAccount> sysVatAccounts;
        public List<SysVatAccount> SysVatAccounts
        {
            get
            {
                if (this.sysVatAccounts == null)
                {
                    lock (sysVatAccountsLock)
                    {
                        var m = new AccountManager(null);
                        this.sysVatAccounts = m.GetSysVatAccounts();
                    }
                }
                return this.sysVatAccounts;
            }
        }
        public void FlushSysVatAccounts()
        {
            this.sysVatAccounts = null;
        }

        #endregion

        #region SysContactType

        static readonly object sysContactTypesLock = new object();
        private List<SysContactType> sysContactTypes;
        public List<SysContactType> SysContactTypes
        {
            get
            {
                if (this.sysContactTypes == null)
                {
                    lock (sysContactTypesLock)
                    {
                        var m = new ContactManager(null);
                        this.sysContactTypes = m.GetSysContactTypes();
                    }
                }
                return this.sysContactTypes;
            }
        }
        public void FlushSysContactTypes()
        {
            this.sysContactTypes = null;
        }

        #endregion

        #region SysContactAddressType

        static readonly object sysContactAddressTypesLock = new object();
        private List<SysContactAddressType> sysContactAddressTypes;
        public List<SysContactAddressType> SysContactAddressTypes
        {
            get
            {
                if (this.sysContactAddressTypes == null)
                {
                    lock (sysContactAddressTypesLock)
                    {
                        var m = new ContactManager(null);
                        this.sysContactAddressTypes = m.GetSysContactAddressTypes();
                    }
                }
                return this.sysContactAddressTypes;
            }
        }
        public void FlushSysContactAddressTypes()
        {
            this.sysContactAddressTypes = null;
        }

        #endregion

        #region SysContactAddressRowType

        static readonly object sysContactAddressRowTypesLock = new object();
        private List<SysContactAddressRowType> sysContactAddressRowTypes;
        public List<SysContactAddressRowType> SysContactAddressRowTypes
        {
            get
            {
                if (this.sysContactAddressRowTypes == null)
                {
                    lock (sysContactAddressRowTypesLock)
                    {
                        var m = new ContactManager(null);
                        this.sysContactAddressRowTypes = m.GetSysContactAddressRowTypes();
                    }
                }
                return this.sysContactAddressRowTypes;
            }
        }
        public void FlushSysContactAddressRowTypes()
        {
            this.sysContactAddressRowTypes = null;
        }

        #endregion

        #region SysContactEComsTypes

        static readonly object sysContactEComTypesLock = new object();
        private List<SysContactEComType> sysContactEComTypes;
        public List<SysContactEComType> SysContactEComTypes
        {
            get
            {
                if (this.sysContactEComTypes == null)
                {
                    lock (sysContactEComTypesLock)
                    {
                        var m = new ContactManager(null);
                        this.sysContactEComTypes = m.GetSysContactEComTypes();
                    }
                }
                return this.sysContactEComTypes;
            }
        }
        public void FlushSysContactEComTypes()
        {
            this.sysContactEComTypes = null;
        }

        #endregion

        #region SysCountry

        static readonly object sysCountryLock = new object();
        private List<SysCountry> sysCountrys;
        public List<SysCountry> SysCountrys
        {
            get
            {
                if (this.sysCountrys == null)
                {
                    lock (sysCountryLock)
                    {
                        var m = new CountryCurrencyManager(null);
                        this.sysCountrys = m.GetSysCountries();
                    }
                }
                return this.sysCountrys;
            }
        }
        public void FlushSysCountrys()
        {
            this.sysCountrys = null;
        }

        #endregion

        #region SysCurrency

        static readonly object sysCurrenciesLock = new object();
        private List<SysCurrency> sysCurrencies;
        public List<SysCurrency> SysCurrencies
        {
            get
            {
                if (this.sysCurrencies == null)
                {
                    lock (sysCurrenciesLock)
                    {
                        var m = new CountryCurrencyManager(null);
                        this.sysCurrencies = m.GetSysCurrencies();
                    }
                }
                return this.sysCurrencies;
            }
        }
        public void FlushSysCurrencies()
        {
            this.sysCurrencies = null;
        }

        #endregion

        #region SysDayType

        static readonly object sysDayTypesLock = new object();
        private List<SysDayType> sysDayTypes;
        public List<SysDayType> SysDayTypes
        {
            get
            {
                if (this.sysDayTypes == null)
                {
                    lock (sysDayTypesLock)
                    {
                        var m = new CalendarManager(null);
                        this.sysDayTypes = m.GetSysDayTypes();
                    }
                }
                return this.sysDayTypes;
            }
        }
        public void FlushSysDayTypes()
        {
            this.sysDayTypes = null;
        }

        #endregion

        #region SysFeature

        static readonly object sysFeaturesLock = new object();
        private List<SysFeatureDTO> sysFeatures;
        public List<SysFeatureDTO> SysFeatures
        {
            get
            {
                if (this.sysFeatures == null)
                {
                    var cached = EvoDistributionCacheConnector.GetCachedValue<List<SysFeatureDTO>>("SOESysFeature");

                    if (cached != null)
                    {
                        this.sysFeatures = cached;
                        Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SetSysFeatureFromDatabase()));
                    }
                    else
                        SetSysFeatureFromDatabase();
                }
                return this.sysFeatures;
            }
        }
        public void FlushSysFeatures()
        {
            this.sysFeatures = null;
        }

        public void SetSysFeatureFromDatabase()
        {
            lock (sysFeaturesLock)
            {
                var m = new FeatureManager(null);
                this.sysFeatures = m.GetSysFeatures().ToDTOs();
                EvoDistributionCacheConnector.UpsertCachedValue("SOESysFeature", this.sysFeatures, TimeSpan.FromHours(1));
            }
        }

        #endregion

        #region SysHoliday

        static readonly object sysHolidaysLock = new object();
        private List<SysHoliday> sysHolidays;
        public List<SysHoliday> SysHolidays
        {
            get
            {
                if (this.sysHolidays == null)
                {
                    lock (sysHolidaysLock)
                    {
                        var m = new CalendarManager(null);
                        this.sysHolidays = m.GetSysHolidays().ToList();
                    }
                }
                return this.sysHolidays;
            }
        }
        public void FlushSysHolidays()
        {
            this.sysHolidays = null;
        }

        #endregion

        #region SysHolidayDTO

        static readonly object sysHolidayDTOsLock = new object();
        private List<SysHolidayDTO> sysHolidayDTOs;
        public List<SysHolidayDTO> SysHolidayDTOs
        {
            get
            {
                if (this.sysHolidayDTOs.IsNullOrEmpty())
                {
                    lock (sysHolidayDTOsLock)
                    {
                        var m = new CalendarManager(null);
                        this.sysHolidayDTOs = m.GetSysHolidayDTOs().ToList();
                    }
                }
                return this.sysHolidayDTOs;
            }
        }
        public void FlushSysHolidayDTOs()
        {
            this.sysHolidayDTOs = null;
        }

        #endregion

        #region SysHolidayTypeDTO

        static readonly object sysHolidayTypeDTOsLock = new object();
        private List<SysHolidayTypeDTO> sysHolidayTypeDTOs;
        public List<SysHolidayTypeDTO> SysHolidayTypeDTOs
        {
            get
            {
                if (this.sysHolidayTypeDTOs.IsNullOrEmpty())
                {
                    lock (sysHolidayTypeDTOsLock)
                    {
                        var m = new CalendarManager(null);
                        this.sysHolidayTypeDTOs = m.GetSysHolidayTypeDTOs();
                    }
                }
                return this.sysHolidayTypeDTOs;
            }
        }
        public void FlushSysHolidayTypeDTOs()
        {
            this.sysHolidayTypeDTOs = null;
        }

        #endregion

        #region SysHouseholdType

        static readonly object sysHouseholdTypesLock = new object();
        private List<SysHouseholdType> sysHouseholdTypes;
        public List<SysHouseholdType> SysHouseholdTypes
        {
            get
            {
                if (this.sysHouseholdTypes == null)
                {
                    lock (sysHouseholdTypesLock)
                    {
                        var m = new ProductManager(null);
                        this.sysHouseholdTypes = m.GetAllSysHouseholdTypes();
                    }
                }
                return this.sysHouseholdTypes;
            }
        }
        public void FlushSysHouseholdTypes()
        {
            this.sysHouseholdTypes = null;
        }

        #endregion

        #region SysIntrastatCode

        static readonly object sysIntrastatCodeLock = new object();
        private List<SysIntrastatCode> sysIntrastatCodes;
        public List<SysIntrastatCode> SysIntrastatCodes
        {
            get
            {
                if (this.sysIntrastatCodes == null)
                {
                    lock (sysIntrastatCodeLock)
                    {
                        var m = new CommodityCodeManager(null);
                        this.sysIntrastatCodes = m.GetSysIntrastatCodes();
                    }
                }
                return this.sysIntrastatCodes;
            }
        }
        public void FlushSysIntrastatCodes()
        {
            this.sysIntrastatCodes = null;
        }

        #endregion

        #region SysLanguage

        static readonly object sysLanguagesLock = new object();
        private List<SysLanguageDTO> sysLanguages;
        public List<SysLanguageDTO> SysLanguages
        {
            get
            {
                if (this.sysLanguages == null)
                {
          
                        var cached = EvoDistributionCacheConnector.GetCachedValue<List<SysLanguageDTO>>("SOESysLanguage");

                        if (cached != null)
                        {
                            this.sysLanguages = cached;
                            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SetSysLanguageFromDatabase()));
                        }
                        else
                            SetSysLanguageFromDatabase();
                    
                }
                return this.sysLanguages;
            }
        }
        public void FlushSysLanguages()
        {
            this.sysLanguages = null;
        }

        public void SetSysLanguageFromDatabase()
        {
            lock (sysLanguagesLock)
            {
                var m = new LanguageManager(null);
                this.sysLanguages = m.GetSysLanguages();
                EvoDistributionCacheConnector.UpsertCachedValue("SOESysLanguage", this.sysLanguages, TimeSpan.FromHours(1));
            }
        }

        #endregion

        #region SysLbError

        static readonly object sysLbErrorsLock = new object();
        private List<SysLbError> sysLbErrors;
        public List<SysLbError> SysLbErrors
        {
            get
            {
                if (this.sysLbErrors == null)
                {
                    lock (sysLbErrorsLock)
                    {
                        var m = new LbManager(null);
                        this.sysLbErrors = m.GetSysLbErrors();
                    }
                }
                return this.sysLbErrors;
            }
        }
        public void FlushSysLbErrors()
        {
            this.sysLbErrors = null;
        }

        #endregion

        #region SysVatRate

        static readonly object sysVatRatesLock = new object();
        private List<SysVatRate> sysVatRates;
        public List<SysVatRate> SysVatRates
        {
            get
            {
                if (this.sysVatRates == null)
                {
                    lock (sysVatRatesLock)
                    {
                        var m = new AccountManager(null);
                        this.sysVatRates = m.GetSysVatRates();
                    }
                }
                return this.sysVatRates;
            }
        }
        public void FlushSysVatRates()
        {
            this.sysVatRates = null;
        }

        #endregion

        #region SysNews

        static readonly object sysNewsLock = new object();
        private List<SysNews> sysNews;
        public List<SysNews> SysNews
        {
            get
            {
                if (this.sysNews == null)
                {
                    lock (sysNewsLock)
                    {
                        var m = new SysNewsManager(null);
                        this.sysNews = m.GetSysNewsAll();
                    }
                }
                return this.sysNews;
            }
        }
        public void FlushSysNews()
        {
            this.sysNews = null;
        }

        #endregion

        #region SysPaymentMethod

        static readonly object sysPaymentMethodsLock = new object();
        private List<SysPaymentMethod> sysPaymentMethods;
        public List<SysPaymentMethod> SysPaymentMethods
        {
            get
            {
                if (this.sysPaymentMethods == null)
                {
                    lock (sysPaymentMethodsLock)
                    {
                        var m = new PaymentManager(null);
                        this.sysPaymentMethods = m.GetAllSysPaymentMethods();
                    }
                }
                return this.sysPaymentMethods;
            }
        }
        public void FlushSysPaymentMethods()
        {
            this.sysPaymentMethods = null;
        }

        #endregion

        #region SysPaymentType

        static readonly object sysPaymentTypesLock = new object();
        private List<SysPaymentType> sysPaymentTypes;
        public List<SysPaymentType> SysPaymentTypes
        {
            get
            {
                if (this.sysPaymentTypes == null)
                {
                    lock (sysPaymentTypesLock)
                    {
                        var m = new PaymentManager(null);
                        this.sysPaymentTypes = m.GetSysPaymentTypes();
                    }
                }
                return this.sysPaymentTypes;
            }
        }
        public void FlushSysPaymentTypes()
        {
            this.sysPaymentTypes = null;
        }

        #endregion

        #region SysPayrollPrice

        static readonly object sysPayrollPriceViewDTOsLock = new object();
        static readonly string sysPayrollPriceViewDTOsKey = "sysPayrollPriceViewDTOsKey";
        static readonly string sysPayrollPriceViewDTOsKeyBackup = sysPayrollPriceViewDTOsKey + "Backup";
        private List<SysPayrollPriceViewDTO> sysPayrollPriceViewDTOs
        {
            get
            {
                return BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Get(sysPayrollPriceViewDTOsKey);
            }
        }
        public List<SysPayrollPriceViewDTO> SysPayrollPriceViewDTOs
        {
            get
            {
                if (this.sysPayrollPriceViewDTOs.IsNullOrEmpty())
                {
                    var fromBackup = BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Get(sysPayrollPriceViewDTOsKeyBackup);

                    if (!fromBackup.IsNullOrEmpty())
                    {
                        BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Set(sysPayrollPriceViewDTOsKey, fromBackup, 60);
                    }

                    lock (sysPayrollPriceViewDTOsLock)
                    {
                        var value = SysPayrollConnector.GetSysPayrollPriceViews();

                        if (!value.IsNullOrEmpty())
                        {
                            BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Set(sysPayrollPriceViewDTOsKey, value, 60 * 10);
                            BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Set(sysPayrollPriceViewDTOsKeyBackup, value, 60 * 60 * 8);
                        }
                    }
                }
                return this.sysPayrollPriceViewDTOs;
            }
        }
        public void FlushSysPayrollPriceViewDTOs()
        {
            BusinessMemoryCache<List<SysPayrollPriceViewDTO>>.Delete(sysPayrollPriceViewDTOsKey);
        }

        #endregion

        #region SysPermission

        static readonly object sysPermissionsLock = new object();
        private List<SysPermission> sysPermissions;
        public List<SysPermission> SysPermissions
        {
            get
            {
                if (this.sysPermissions == null)
                {
                    lock (sysPermissionsLock)
                    {
                        var m = new FeatureManager(null);
                        this.sysPermissions = m.GetSysPermissions();
                    }
                }
                return this.sysPermissions;
            }
        }
        public void FlushSysPermissions()
        {
            this.sysPermissions = null;
        }

        #endregion

        #region SysProduct

        static readonly object sysProductsLock = new object();
        private List<SysProduct> sysProducts;
        public List<SysProduct> SysProducts
        {
            get
            {
                if (this.sysProducts == null)
                {
                    lock (sysProductsLock)
                    {
                        var m = new SysPriceListManager(null);
                        this.sysProducts = m.GetSysProducts();
                    }
                }
                return this.sysProducts;
            }
        }
        public void FlushSysProducts()
        {
            this.sysProducts = null;
        }

        #endregion

        #region SysReportTemplate

        static readonly object sysReportTemplateTypesLock = new object();
        private List<SysReportTemplateType> sysReportTemplateTypes;
        public List<SysReportTemplateType> SysReportTemplateTypes
        {
            get
            {
                if (this.sysReportTemplateTypes == null)
                {
                    lock (sysReportTemplateTypesLock)
                    {
                        var m = new ReportManager(null);
                        this.sysReportTemplateTypes = m.GetSysReportTemplateTypes();
                    }
                }
                return this.sysReportTemplateTypes;
            }
        }
        public void FlushSysReportTemplateTypes()
        {
            this.sysReportTemplateTypes = null;
        }

        #endregion

        #region SysReportType

        static readonly object sysReportTypesLock = new object();
        private List<SysReportType> sysReportTypes;
        public List<SysReportType> SysReportTypes
        {
            get
            {
                if (this.sysReportTypes == null)
                {
                    lock (sysReportTypesLock)
                    {
                        var m = new ReportManager(null);
                        this.sysReportTypes = m.GetSysReportTypes();
                    }
                }
                return this.sysReportTypes;
            }
        }
        public void FlushSysReportTypes()
        {
            this.sysReportTypes = null;
        }

        #endregion

        #region SysSetting

        static readonly object sysSettingsLock = new object();
        private List<SysSetting> sysSettings;
        public List<SysSetting> SysSettings
        {
            get
            {
                if (this.sysSettings == null)
                {
                    lock (sysSettingsLock)
                    {
                        var m = new FieldSettingManager(null);
                        this.sysSettings = m.GetSysSettings();
                    }
                }
                return this.sysSettings;
            }
        }
        public void FlushSysSettings()
        {
            this.sysSettings = null;
        }

        #endregion

        #region SysSettingType

        static readonly object sysSettingTypesLock = new object();
        private List<SysSettingType> sysSettingTypes;
        public List<SysSettingType> SysSettingTypes
        {
            get
            {
                if (this.sysSettingTypes == null)
                {
                    lock (sysSettingTypesLock)
                    {
                        var m = new FieldSettingManager(null);
                        this.sysSettingTypes = m.GetSysSettingTypes();
                    }
                }
                return this.sysSettingTypes;
            }
        }
        public void FlushSysSettingTypes()
        {
            this.sysSettingTypes = null;
        }

        #endregion

        #region SysWholeseller

        static readonly object sysWholesellersLock = new object();
        private Dictionary<int, SysWholeseller> sysWholesellers;
        public Dictionary<int, SysWholeseller> SysWholesellers
        {
            get
            {
                if (this.sysWholesellers == null)
                {
                    lock (sysWholesellersLock)
                    {
                        var m = new SysPriceListManager(null);
                        var list = m.GetSysWholesellers();
                        sysWholesellers = new Dictionary<int, SysWholeseller>(list.Count);
                        list.ForEach(x => sysWholesellers.Add(x.SysWholesellerId, x));
                    }
                }
                return this.sysWholesellers;
            }
        }
        public void FlushSysWholesellers()
        {
            this.sysWholesellers = null;
        }

        #endregion

        #region SysXEArticle

        static readonly object sysXEArticlesLock = new object();
        private List<SysXEArticle> sysXEArticles;
        public List<SysXEArticle> SysXEArticles
        {
            get
            {
                if (this.sysXEArticles == null)
                {
                    lock (sysXEArticlesLock)
                    {
                        var m = new FeatureManager(null);
                        this.sysXEArticles = m.GetSysXEArticles();
                    }
                }
                return this.sysXEArticles;
            }
        }
        public void FlushSysXEArticles()
        {
            this.sysXEArticles = null;
        }

        #endregion

        #region SysXEArticleFeature

        static readonly object sysXEArticleFeaturesLock = new object();
        private List<SysXEArticleFeature> sysXEArticleFeatures;
        public List<SysXEArticleFeature> SysXEArticleFeatures
        {
            get
            {
                if (this.sysXEArticleFeatures == null)
                {
                    lock (sysXEArticleFeaturesLock)
                    {
                        var m = new FeatureManager(null);
                        this.sysXEArticleFeatures = m.GetSysXEArticleFeatures();
                    }
                }
                return this.sysXEArticleFeatures;
            }
        }
        public void FlushSysXEArticleFeatures()
        {
            this.sysXEArticleFeatures = null;
        }

        #endregion
    }
}
