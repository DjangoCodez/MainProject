using Common.Util;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Mozilla;
using RestSharp;
using Soe.Sys.Common.DTO;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Core.Template;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Security;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util;
using SoftOne.Status.Shared.DTO;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using static Soe.Sys.Common.Enumerations;
using Version = SoftOne.Soe.Data.Version;

namespace SoftOne.Soe.Business.Util
{
    public static class ConfigurationSetupUtil
    {
        private static SysServiceManager _ssm { get; set; }
        private static SysServiceManager ssm
        {
            get
            {
                if (_ssm == null)
                {
                    _ssm = new SysServiceManager(null);
                }
                return _ssm;
            }
        }
        private static CompanyTemplateManager _companyTemplateManager { get; set; }
        private static CompanyTemplateManager companyTemplateManager
        {
            get
            {
                if (_companyTemplateManager == null)
                {
                    _companyTemplateManager = new CompanyTemplateManager(null);
                }
                return _companyTemplateManager;
            }
        }
        private static List<SysCompDBDTO> sysCompDBDTOs;
        private static List<StatusServiceDTO> statusServiceDTOs;
        private static List<StatusServerDTO> statusServerDTOs;
        private static string sysServiceUrl;
        private static DateTime sysServiceUrlValidUntil;
        private static KeyVaultSettings keyVaultSettings;
        private static RunLocally runLocally;

        private static string localConnectionStringOverrideFilePath
        {
            get
            {
                return @"C:\temp\LocalConnectionStringOverride.json";
            }
        }

        private static string sysCompDBsFilePath
        {
            get
            {
                return @"C:\temp\" + CreateFileName("SysCompDBs", "txt");
            }
        }

        private static string statusServicesFilePath
        {
            get
            {
                return @"C:\temp\" + CreateFileName("StatusServices", "txt");
            }
        }

        private static string statusServersFilePath
        {
            get
            {
                return @"C:\temp\" + CreateFileName("StatusServers", "txt");
            }
        }

        private static object logFileLock = new object();
        private static object initLock = new object();
        private static bool InitDone { get; set; }

        public static void Init(bool isSOEWeb = false)
        {
            if (InitDone)
                return;

            var sw = Stopwatch.StartNew();
            try
            {
                lock (initLock)
                {
                    if (InitDone)
                        return;

                    InitRunLocally();
                    LogMessage("Init started");
                    string secret = string.Empty;

                    try
                    {
                        var currentProtocol = ServicePointManager.SecurityProtocol;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        bool isTest = CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test;
                        LogMessage($"Init isTest {isTest}");

                        Parallel.Invoke(
                            () => LogMessageWithTimer("SetupSSM", () => _ssm = new SysServiceManager(null)),
                            () =>
                            {
                                sysCompDBDTOs = GetSysCompDBDTOs();
                                LogMessage($"Init sysCompDBDTOs count {sysCompDBDTOs?.Count ?? -1}");
                            },
                            () =>
                             {
                                 statusServiceDTOs = GetStatusServiceDTOs();
                                 LogMessage($"Init statusServiceDTOs count {statusServiceDTOs?.Count ?? -1}");
                             },
                            () =>
                            {
                                statusServerDTOs = GetStatusServerDTOs();
                                LogMessage($"Init statusServerDTOs count {statusServerDTOs?.Count ?? -1}");
                            },
                            () =>
                            {
                                Z.EntityFramework.Extensions.LicenseManager.AddLicense("836;101-SoftOne", "808e16a2-f300-1dd0-5be5-bc37afb71327");
                                if (!Z.EntityFramework.Extensions.LicenseManager.ValidateLicense(out string licenseErrorMessage))
                                {
                                    throw new LicenseException(typeof(Z.EntityFramework.Extensions.LicenseManager), null, licenseErrorMessage);
                                }
                                LogMessage("Init Z.EntityFramework.Extensions.LicenseManager done");
                            },
                            () =>
                            {
                                keyVaultSettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
                                secret = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, isTest ? "TerminalTestPubSub-Key" : "TerminalPubSub-Key", keyVaultSettings.StoreLocation);

                                if (string.IsNullOrEmpty(secret))
                                    LogMessage("TerminalPubSub-Key is null");

                                ApplicationInsightSetup();
                                if (string.IsNullOrEmpty(secret))
                                {
                                    var message = $"Failed to fetch secret. isTest:{isTest} Store:{keyVaultSettings.StoreLocation} Url:{keyVaultSettings.KeyVaultUrl}";
                                    LogMessage(message);
                                    LogCollector.LogCollector.LogError($"Failed to fetch secret. isTest:{isTest} Store:{keyVaultSettings.StoreLocation} Url:{keyVaultSettings.KeyVaultUrl}");
                                }
                            }
                        );

                        LogMessage("Init sys fetching Done");

                        try
                        {
                            CompDbCache.Instance.SysCompDbId = GetCurrentSysCompDbId();
                            LogMessage($"Init CompDbCache Done SysCompDbId {CompDbCache.Instance.SysCompDbId}");

                            if (IsTestBasedOnMachine() && runLocally != null && runLocally.RunLocalDatabase && runLocally.RunLocalDatabase && runLocally.NextRestore < DateTime.UtcNow)
                            {
                                LogMessage($"Restore started");
                                var restorer = new DatabaseRestorer();
                                restorer.RestoreDatabases();
                                runLocally.LastRestore = DateTime.UtcNow;
                                runLocally.NextRestore = DateTime.UtcNow.AddHours(240);
                                SaveRunLocally(runLocally);
                                LogMessage($"Restore completed");
                            }
                            LogMessage($"BaseDirectory {AppDomain.CurrentDomain.BaseDirectory}");
                            LogMessage($"GetCurrentFolderName {GetCurrentFolderName()}");
                            LogMessage($"GetCurrentAppName {GetCurrentAppName()}");
                            LogMessage($"Init keyvaultsettings {JsonConvert.SerializeObject(keyVaultSettings)}");

                            Parallel.Invoke(
                                () => LogMessageWithTimer("GetAccessToken", () => ConnectorBase.GetAccessToken()),
                                () => LogMessageWithTimer("GetSysServiceUrl", () => GetSysServiceUrl()),
                                () => LogMessageWithTimer("GetCookieSecret", () => TimeOutOwinHelper.CookieSecret = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, "GoCookieKey", keyVaultSettings.StoreLocation)),
                                () => LogMessageWithTimer("SetCompSqlConnectionStringBuilder", () => SetSqlConnectionStringBuilder(keyVaultSettings)),
                                () => LogMessageWithTimer("SetSysSqlConnectionStringBuilder", () => SetSysSqlConnectionStringBuilder(keyVaultSettings)),
                                () => GetEvoUrl()
                            );

                            WebPubSubUtil.Init(secret);
                            LogMessage($"Init WebPubSubUtil.Init done");

                            LogMessage($"Init IsTestBasedOnMachine {IsTestBasedOnMachine()}");
                            LogMessage($"Init GetCurrentSysCompDbDTO() {JsonConvert.SerializeObject(GetCurrentSysCompDbDTO())}");
                            LogMessage($"Init GetCurrentUrl()");
                            LogMessage($"Init GetCurrentFolderName() {GetCurrentFolderName()}");
                            Task.Run(() => StartWebApi());
                            LogMessage($"UseL1AndL2Cache {UseL1AndL2Cache()}");

                            Parallel.Invoke(
                                () => LogMessage($"Init SysLanguages - count {SysDbCache.Instance.SysLanguages?.Count() ?? -1}"),
                                () => LogMessage($"Init SysFeatureCache - count {SysDbCache.Instance.SysFeatures?.Count() ?? -1}"),
                                () => LogMessageWithTimer("InitAgreement100", () => companyTemplateManager.InitAgreement100()),
                                () => LogMessageWithTimer("SetupSysTermCache", () => TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true))
                            );

                            if (!IsDebug())
                                SetVersionToDatabase();

                            LogCollector.LogCollector.LogInfo($"Pubsub setup done url: {secret}");
                            LoggerConnector.Init();
                            LogMessage("LoggerConnector Init done");
                            LogMessage($"UseL1AndL2Cache {UseL1AndL2Cache()}");
                            LogMessage("Init completed successfully");
                            ServicePointManager.SecurityProtocol = currentProtocol;
                            reachedEnd = true;
                            CleanOldLogPosts();
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Setup failed: {ex}");
                            LogCollector.LogCollector.LogError($"Setup failed: {ex}");
                            reachedEnd = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = $"Init failed {ex}";
                        LogMessage(message);
                        LogCollector.LogCollector.LogError(message);
                        reachedEnd = false;
                    }
                }

            }
            finally
            {
                // Only mark initialization as done if validation passes
                if (ValidateInit())
                    InitDone = true;
            }
            LogMessage($"Init took {sw.Elapsed.TotalSeconds} seconds");
        }

        private static bool reachedEnd = false;

        private static bool ValidateInit()
        {
            try
            {
                if (!CompEntities.HasValidConnectionString())
                    throw new DataException("DB error: csc");

                if (!SOESysEntities.HasValidConnectionString())
                    throw new DataException("DB error: css");

                // Access token must be available; throw if missing
                if (string.IsNullOrEmpty(ConnectorBase.GetAccessToken()))
                    throw new DataException("setup error: atn");

                if (string.IsNullOrEmpty(GetSysServiceUrl()))
                    throw new DataException("setup error: ssu");

                if (string.IsNullOrEmpty(TimeOutOwinHelper.CookieSecret))
                    throw new DataException("setup error: tco");

                if (!reachedEnd)
                    throw new DataException("setup error: NotComplete");

                if (GetCurrentSysCompDbId() == 0)
                    throw new DataException("setup error: Scpdb0");

                if (GetCurrentUrl() == null)
                    throw new DataException("setup error: curun");
            }
            catch (Exception ex)
            {
                LogMessage("ValidateInit exception: " + ex.ToString());
                LogCollector.LogCollector.LogError("ValidateInit failed: " + ex.ToString());
                throw new DataException("Config failed: " + ex.ToString());
            }

            return true;
        }

        public static SqlConnectionStringBuilder GetSqlConnectionStringBuilder()
        {
            if (_sqlConnectionStringBuilder == null)
            {
                SetSqlConnectionStringBuilder(keyVaultSettings);
                CompEntities.SetSqlConnectionStringBuilder(_sqlConnectionStringBuilder);
            }

            return _sqlConnectionStringBuilder;
        }

        private static SqlConnectionStringBuilder _sqlConnectionStringBuilder { get; set; }

        private static void SetSqlConnectionStringBuilder(KeyVaultSettings keyVaultSettings)
        {
#if DEBUG
            if (TryGetConnectionStringBuilderFromLocalOverride("SOECompEntities", out SqlConnectionStringBuilder overrideBuilder))
            {
                _sqlConnectionStringBuilder = overrideBuilder;
                FrownedUponSQLClient.Password = overrideBuilder.Password;
                CompEntities.SetSqlConnectionStringBuilder(_sqlConnectionStringBuilder);
                return;
            }
#endif

            var userAndPassword = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, $"Database-UserAndPassword-SysCompServerId{GetCurrentSysCompDbDTO().SysCompServerDTO.SysCompServerId}", keyVaultSettings.StoreLocation);
            if (userAndPassword.IsNullOrEmpty())
                return;

            var arr = userAndPassword.Split(new string[] { "##" }, StringSplitOptions.None);
            var user = arr[0];
            var password = arr[1];
            var database = GetDatabaseNameFromSysCompDbId();
            var instance = GetInstanceNameFromSysCompDbId(false);

            if (IsTestBasedOnMachine() && runLocally.RunLocalDatabase)
                instance = ".\\dev";

            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = instance,
                InitialCatalog = database,
                UserID = user,
                Password = password,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework",
                ConnectTimeout = 30,
                Encrypt = true,
                TrustServerCertificate = true,
                IntegratedSecurity = false,
            };
            FrownedUponSQLClient.Password = password;
            CompEntities.SetSqlConnectionStringBuilder(_sqlConnectionStringBuilder);
        }

        private static SqlConnectionStringBuilder GetSqlConnectionStringBuilder(int sysCompDbId)
        {
            var sysCompDb = GetSysCompDbsOfSameType().FirstOrDefault(w => w.SysCompDbId == sysCompDbId);

            if (sysCompDb != null)
            {
                var source = sysCompDb.SysCompServerDTO.Name;

                if (IsTestBasedOnMachine() && runLocally.RunLocalDatabase)
                    source = ".\\dev";

                return new SqlConnectionStringBuilder()
                {
                    DataSource = source,
                    InitialCatalog = sysCompDb.Name,
                    UserID = _sqlConnectionStringBuilder.UserID,
                    Password = _sqlConnectionStringBuilder.Password,
                    MultipleActiveResultSets = true,
                    ApplicationName = "EntityFramework",
                    ConnectTimeout = 30,
                    Encrypt = true,
                    TrustServerCertificate = true,
                    IntegratedSecurity = false,
                };
            }

            return null;
        }
#if DEBUG
        private static bool TryGetConnectionStringBuilderFromLocalOverride(string name, out SqlConnectionStringBuilder sqlConnectionStringBuilder)
        {
            sqlConnectionStringBuilder = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                LogMessage("TryGetConnectionStringBuilderFromLocalOverride called with empty name.");
                return false;
            }

            try
            {
                var overrideObj = GetLocalConnectionStringOverride();
                if (overrideObj == null)
                    return false;

                var connectionStringOverride = overrideObj.ConnectionStrings
                    ?.FirstOrDefault(f => f?.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                if (connectionStringOverride == null)
                {
                    LogMessage($"TryGetConnectionStringBuilderFromLocalOverride: entry '{name}' not found in override file.");
                    return false;
                }

                var builder = new SqlConnectionStringBuilder(connectionStringOverride.ConnectionString);

                // Minimal validation/warning to help diagnose local issues
                if (string.IsNullOrWhiteSpace(builder.DataSource) || string.IsNullOrWhiteSpace(builder.InitialCatalog))
                {
                    LogMessage($"TryGetConnectionStringBuilderFromLocalOverride: entry '{name}' has missing DataSource or InitialCatalog.");
                }

                sqlConnectionStringBuilder = builder;
                LogMessage($"TryGetConnectionStringBuilderFromLocalOverride: using local override for '{name}'.");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("TryGetConnectionStringBuilderFromLocalOverride " + ex.ToString());
                return false;
            }
        }

        private class LocalConnectionStringOverride
        {
            public List<ConnectionStringOverride> ConnectionStrings { get; set; } = new List<ConnectionStringOverride>();
        }

        private class ConnectionStringOverride
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }
        }

        private static LocalConnectionStringOverride GetLocalConnectionStringOverride()
        {
            try
            {
                // Allow environment override; fallback to default path
                var filePath = Environment.GetEnvironmentVariable("SOE_LOCAL_CONNSTR_PATH");
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = localConnectionStringOverrideFilePath;
                }

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = File.ReadAllText(filePath);
                var overrideObj = JsonConvert.DeserializeObject<LocalConnectionStringOverride>(json);
                return overrideObj;
            }
            catch (Exception ex)
            {
                LogMessage("GetLocalConnectionStringOverride " + ex.ToString());
                return null;
            }
        }
#endif
        private static void SetSysSqlConnectionStringBuilder(KeyVaultSettings keyVaultSettings)
        {
#if DEBUG
            if (TryGetConnectionStringBuilderFromLocalOverride("SOESysEntities", out SqlConnectionStringBuilder overrideBuilder))
            {
                _sqlConnectionStringBuilder = overrideBuilder;
                SOESysEntities.SetSqlConnectionStringBuilder(_sqlConnectionStringBuilder);
                return;
            }
#endif

            var userAndPassword = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, $"Database-UserAndPassword-SysCompServerId{GetCurrentSysCompDbDTO().SysCompServerDTO.SysCompServerId}", keyVaultSettings.StoreLocation);
            var arr = userAndPassword.Split(new string[] { "##" }, StringSplitOptions.None);
            var user = arr[0];
            var password = arr[1];
            var database = "SoeSysv2";
            var instance = GetInstanceNameFromSysCompDbId(true);


            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = instance,
                InitialCatalog = database,
                UserID = user,
                Password = password,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework",
                ConnectTimeout = 30,
                Encrypt = true,
                TrustServerCertificate = true,
                IntegratedSecurity = false,
            };

            SOESysEntities.SetSqlConnectionStringBuilder(_sqlConnectionStringBuilder);
        }

        private static readonly string useL1AndL2CacheKey = "UseL1AndL2Cache";
        private readonly static bool defaultUseL1AndL2Cache = true;
        private static bool? useL1AndL2Cache { get; set; }

        public static bool UseL1AndL2Cache()
        {
            try
            {
                var value = BusinessMemoryCacheOld<bool?>.Get(useL1AndL2CacheKey);

                if (value.HasValue)
                    return value.Value;

                Task.Run(() => SetUseL1AndL2Cache());

                if (useL1AndL2Cache.HasValue)
                    return useL1AndL2Cache.Value;

                return defaultUseL1AndL2Cache;
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError("UseL1AndL2Cache failed to get setting from cache or database, returning default value. " + ex.ToString());
                return defaultUseL1AndL2Cache;
            }
        }

        private static void SetUseL1AndL2Cache()
        {
            var settingManager = new SettingManager(null);
            var setting = settingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseL1AndL2Cache, 0, 0, 0, defaultValue: true);
            BusinessMemoryCacheOld<bool?>.Set(useL1AndL2CacheKey, setting, 60 * 10);
            useL1AndL2Cache = setting;
            LogCollector.LogCollector.LogInfo($"UseL1AndL2Cache set to {setting}");
        }

        private static int cacheIntAcceptSeconds { get; set; }
        private readonly static int defaultCacheIntAcceptSeconds = 60;
        private readonly static string cacheIntAcceptSecondsKey = "Cache_AcceptSeconds";
        public static int CacheIntAcceptSeconds()
        {
            try
            {
                var value = BusinessMemoryCacheOld<int?>.Get(cacheIntAcceptSecondsKey);
                if (value.HasValue)
                    return value.Value;

                Task.Run(() => SetCacheIntAcceptSecondsFromDatabase());

                if (cacheIntAcceptSeconds != 0)
                    return cacheIntAcceptSeconds;

                return defaultCacheIntAcceptSeconds;
            }
            catch
            {
                return defaultCacheIntAcceptSeconds;
            }
        }

        private static void SetCacheIntAcceptSecondsFromDatabase()
        {
            var settingManager = new SettingManager(null);
            var setting = settingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.CacheAcceptSeconds, 0, 0, 0);
            if (setting != 0)
            {
                if (setting != cacheIntAcceptSeconds)
                    LogCollector.LogCollector.LogInfo($"CacheIntAcceptSeconds changed from {cacheIntAcceptSeconds} to {setting}");

                cacheIntAcceptSeconds = setting;
            }
            else
                cacheIntAcceptSeconds = defaultCacheIntAcceptSeconds;
            BusinessMemoryCacheOld<int?>.Set(cacheIntAcceptSecondsKey, cacheIntAcceptSeconds, 60 * 5);
        }

        private static int cacheCheckIntervalSeconds { get; set; }
        private readonly static int defaultCacheCheckIntervalSeconds = 30;
        private readonly static string cacheCheckIntervalSecondsKey = "Cache_CheckIntervalSeconds";

        public static int CacheCheckIntervalSeconds()
        {
            try
            {
                var value = BusinessMemoryCacheOld<int?>.Get(cacheCheckIntervalSecondsKey);
                if (value.HasValue)
                    return value.Value;
                Task.Run(() => SetCacheCheckIntervalSecondsFromDatabase());
                if (cacheCheckIntervalSeconds != 0)
                    return cacheCheckIntervalSeconds;
                return defaultCacheCheckIntervalSeconds;
            }
            catch
            {
                return defaultCacheCheckIntervalSeconds;
            }
        }

        private static void SetCacheCheckIntervalSecondsFromDatabase()
        {
            var settingManager = new SettingManager(null);
            var setting = settingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.CacheCheckIntervalSeconds, 0, 0, 0);
            if (setting != 0)
            {
                if (setting != cacheCheckIntervalSeconds)
                    LogCollector.LogCollector.LogInfo($"CacheCheckIntervalSeconds changed from {cacheCheckIntervalSeconds} to {setting}");

                cacheCheckIntervalSeconds = setting;
            }
            else
                cacheCheckIntervalSeconds = defaultCacheCheckIntervalSeconds;
            BusinessMemoryCacheOld<int?>.Set(cacheCheckIntervalSecondsKey, cacheCheckIntervalSeconds, 60 * 5);
        }

        private static int cacheDefaultLocalTtlSeconds { get; set; }
        private readonly static int defaultCacheDefaultLocalTtlSeconds = 60;
        private readonly static string cacheDefaultLocalTtlSecondsKey = "Cache_DefaultLocalTtlSeconds";

        public static int CacheDefaultLocalTtlSeconds()
        {
            try
            {
                var value = BusinessMemoryCacheOld<int?>.Get(cacheDefaultLocalTtlSecondsKey);
                if (value.HasValue)
                    return value.Value;
                Task.Run(() => SetCacheDefaultLocalTtlSecondsFromDatabase());
                if (cacheDefaultLocalTtlSeconds != 0)
                    return cacheDefaultLocalTtlSeconds;
                return defaultCacheDefaultLocalTtlSeconds;
            }
            catch
            {
                return defaultCacheDefaultLocalTtlSeconds;
            }
        }

        private static void SetCacheDefaultLocalTtlSecondsFromDatabase()
        {
            var settingManager = new SettingManager(null);
            var setting = settingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.CacheDefaultLocalTtlSeconds, 0, 0, 0);
            if (setting != 0)
            {
                if (setting != cacheDefaultLocalTtlSeconds)
                    LogCollector.LogCollector.LogInfo($"CacheDefaultLocalTtlSeconds changed from {cacheDefaultLocalTtlSeconds} to {setting}");

                cacheDefaultLocalTtlSeconds = setting;
            }
            else
                cacheDefaultLocalTtlSeconds = defaultCacheDefaultLocalTtlSeconds;
            BusinessMemoryCacheOld<int?>.Set(cacheDefaultLocalTtlSecondsKey, cacheDefaultLocalTtlSeconds, 60 * 5);
        }

        private static int cacheLeaseSeconds { get; set; }
        private readonly static int defaultCacheLeaseSeconds = 60;
        private readonly static string cacheLeaseSecondsKey = "Cache_LeaseSeconds";

        public static int CacheLeaseSeconds()
        {
            try
            {
                var value = BusinessMemoryCacheOld<int?>.Get(cacheLeaseSecondsKey);
                if (value.HasValue)
                    return value.Value;
                Task.Run(() => SetCacheLeaseSecondsFromDatabase());
                if (cacheLeaseSeconds != 0)
                    return cacheLeaseSeconds;
                return defaultCacheLeaseSeconds;
            }
            catch
            {
                return defaultCacheLeaseSeconds;
            }
        }

        private static void SetCacheLeaseSecondsFromDatabase()
        {
            var settingManager = new SettingManager(null);
            var setting = settingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.CacheLeaseSeconds, 0, 0, 0);
            if (setting != 0)
                cacheLeaseSeconds = setting;
            else
                cacheLeaseSeconds = defaultCacheLeaseSeconds;
            BusinessMemoryCacheOld<int?>.Set(cacheLeaseSecondsKey, cacheLeaseSeconds, 60 * 5);
        }


        public static void SetVersionToDatabase()
        {
            using (CompEntities entities = new CompEntities())
            {
                var type = "Code";
                var app = GetCurrentAppName();
                var assemblyVersion = GeneralManager.GetAssemblyVersion();
                var machineName = Environment.MachineName;
                Version version = entities.Version.Where(w => w.Type == type && w.App == app && w.Machine == machineName && w.Version1 == assemblyVersion).FirstOrDefault();
                if (version == null)
                {
                    entities.Version.AddObject(new Version()
                    {
                        App = app,
                        Machine = machineName,
                        Version1 = assemblyVersion,
                        Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Type = type,
                    });
                }
                else
                {
                    var created = version.Created;
                    if (created.Contains("|"))
                    {
                        created = created.Split('|')[0];
                    }
                    version.Created = created + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (IsTestBasedOnMachine())
                {
                    var licenses100 = entities.License.FirstOrDefault(w => w.LicenseNr == "100");
                    if (licenses100 != null)
                    {
                        var parentId = GetParentSysCompDbId();

                        if (parentId == 1)
                            licenses100.Name = "Major";
                        else if (parentId == 2)
                            licenses100.Name = "Ref";
                        else if (parentId == 4)
                            licenses100.Name = "Demo template";
                        else if (parentId == 5)
                            licenses100.Name = "Demo";
                        else if (parentId == 7)
                            licenses100.Name = "ICA";
                        else if (parentId == 8)
                            licenses100.Name = "Axfood";
                        else if (parentId == 18)
                            licenses100.Name = "Major 2";
                        else if (parentId == 19)
                            licenses100.Name = "Finland";
                        else if (parentId == 39)
                            licenses100.Name = "Template";
                        else if (parentId == 40)
                            licenses100.Name = "Coop";
                        else if (parentId == 50)
                            licenses100.Name = "M&S";
                        else if (parentId == 51)
                            licenses100.Name = "Flexibla kontoret";
                        else if (parentId != 0)
                        {
                            var parent = GetSysCompDBDTOs().FirstOrDefault(f => f.SysCompDbId == parentId);
                            if (parent != null)
                            {
                                licenses100.Name = parent.Name;
                            }
                        }
                    }
                }

                entities.SaveChanges();
            }
        }

        private static readonly string AG1_IP = "192.168.50.100";
        private static readonly string AG2_IP = "192.168.50.101";
        private static readonly List<int> SysCompDbIdsOnAg2 = new List<int> { 2, 7, 39, 40 };

        private static string _instanceName;
        private static string _sysInstanceName;

        private static string GetInstanceNameFromSysCompDbId(bool isSys)
        {
            // Choose the appropriate instance name to use
            string instanceToUse = isSys ? _sysInstanceName : _instanceName;

            if (!string.IsNullOrEmpty(instanceToUse))
                return instanceToUse;

            if (IsTestBasedOnMachine())
                instanceToUse = GetCurrentSysCompDbDTO().SysCompServerDTO.Name;
            else
                instanceToUse = isSys ? AG1_IP : GetInstanceNameBasedOnAg2Cluster();

            if (IsTestBasedOnMachine() && runLocally.RunLocalDatabase)
                instanceToUse = ".\\dev";

            // Assign the computed value back to the appropriate instance name
            if (isSys)
                _sysInstanceName = instanceToUse;
            else
                _instanceName = instanceToUse;

            return instanceToUse;
        }

        private static string GetInstanceNameBasedOnAg2Cluster()
        {
            return SysCompDbIdsOnAg2.Contains(GetCurrentSysCompDbId()) ? AG2_IP : GetCurrentSysCompDbDTO().SysCompServerDTO.Name;
        }

        private static string _databaseName;
        private static string GetDatabaseNameFromSysCompDbId()
        {
            if (_databaseName != null)
                return _databaseName;

            _databaseName = GetCurrentSysCompDbDTO().Name;

            return _databaseName;
        }

        private static TermGroup_SysPageStatusSiteType _siteType { get; set; } = TermGroup_SysPageStatusSiteType.Test;

        public static TermGroup_SysPageStatusSiteType GetSiteType()
        {
            if (_siteType == TermGroup_SysPageStatusSiteType.Live || _siteType == TermGroup_SysPageStatusSiteType.Beta)
                return _siteType;

            var sysCompDBDTO = GetSysCompDBDTOs().FirstOrDefault(f => f.SysCompDbId == GetCurrentSysCompDbId());

            if (sysCompDBDTO != null)
            {
                switch (sysCompDBDTO.Type)
                {
                    case SysCompDBType.Test:
                        _siteType = TermGroup_SysPageStatusSiteType.Test;
                        break;
                    case SysCompDBType.Production:
                        _siteType = TermGroup_SysPageStatusSiteType.Live;
                        break;
                    default:
                        _siteType = TermGroup_SysPageStatusSiteType.Beta;
                        break;
                }
            }
            else if (!IsTestBasedOnMachine())
                _siteType = TermGroup_SysPageStatusSiteType.Live;
            else
                _siteType = TermGroup_SysPageStatusSiteType.Test;

            return _siteType;
        }

        private static bool? _isTestBasedOnMachine = null;

        public static bool IsTestBasedOnMachine()
        {
            if (_isTestBasedOnMachine.HasValue)
                return _isTestBasedOnMachine.Value;

            var machineName = Environment.MachineName.ToLower();
            bool isTest;

            // Rule A: Any machine NOT containing "softones" is always considered test (dev machines etc.)
            if (!machineName.Contains("softones"))
            {
                LogMessage("IsTestBasedOnMachine identified test environment based on machine name: " + machineName);
                isTest = true;
            }
            else
            {
                // Machines containing "softones" can be either production or test.

                // Rule B: The legacy test machine "softones33"
                if (machineName.Contains("s33"))
                {
                    LogMessage("IsTestBasedOnMachine identified test environment based on machine name: " + machineName);
                    isTest = true;
                }
                else
                {
                    // Default assumption for "softones" machines is production unless proven otherwise.
                    isTest = false;

                    // Rule C: Test environment identified by IP range 192.168.53.*
                    try
                    {
                        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork &&
                                ip.ToString().StartsWith("192.168.53."))
                            {
                                LogMessage("IsTestBasedOnMachine identified test environment based on IP: " + ip.ToString());
                                isTest = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage("IsTestBasedOnMachine DNS lookup failed: " + ex);
                    }
                }
            }

            LogMessage("IsTestBasedOnMachine result for machine " + machineName + ": " + isTest);
            _isTestBasedOnMachine = isTest;
            return isTest;
        }



        public static string GetSysServiceUrl(bool tryToRenew = false)
        {
            if (tryToRenew || string.IsNullOrEmpty(sysServiceUrl) || DateTime.Now >= sysServiceUrlValidUntil)
            {
                if (GetSiteType() == TermGroup_SysPageStatusSiteType.Live)
                {
                    var fetchedSysServiceUrl = SoftOneStatusConnector.GetSysServiceUrl(false, false);

                    if (!string.IsNullOrEmpty(fetchedSysServiceUrl))
                        sysServiceUrl = fetchedSysServiceUrl;
                }
                else
                {
                    var fetchedSysServiceUrl = SoftOneStatusConnector.GetDefaultSysServiceUrl(GetCurrentSysCompDbId(), false, false);

                    if (!string.IsNullOrEmpty(fetchedSysServiceUrl))
                        sysServiceUrl = fetchedSysServiceUrl;
                }

                sysServiceUrlValidUntil = DateTime.Now.AddMinutes(30);
            }

            return sysServiceUrl;
        }

        public static IEnumerable<string> GetPossibleFolderNames(this SysCompDBDTO sysCompDBDTO)
        {
            List<string> subdomains = new List<string>();

            if (sysCompDBDTO.SysCompDbId == 5)
                subdomains.AddRange(new List<string>() { "demo" });

            if (!string.IsNullOrEmpty(sysCompDBDTO.ApiUrl))
            {
                Uri apiUrl = new Uri(sysCompDBDTO.ApiUrl);
                string subdomain = apiUrl.Authority.Split('.')[0];
                subdomains.Add(subdomain);
            }

            foreach (var serverId in HostServerIds)
            {
                subdomains.Add($"s{serverId}s1d{sysCompDBDTO.SysCompDbId}");
                subdomains.Add($"s1d{sysCompDBDTO.SysCompDbId}");
            }
            return subdomains.Distinct().ToList();
        }

        public static int[] HostServerIds => new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 101, 201 };

        private static SysCompDBDTO currentSysCompDbDTO { get; set; } = null;
        public static SysCompDBDTO GetCurrentSysCompDbDTO()
        {
            if (currentSysCompDbDTO != null)
                return currentSysCompDbDTO;
            currentSysCompDbDTO = GetSysCompDBDTOs().FirstOrDefault(f => f.SysCompDbId == GetCurrentSysCompDbId());
            return currentSysCompDbDTO;
        }
        private static int currentSysCompDbId { get; set; } = 0;
        public static int GetCurrentSysCompDbId()
        {
            if (currentSysCompDbId != 0)
                return currentSysCompDbId;
            currentSysCompDbId = GetCurrentSysCompDbId(sysCompDBDTOs, AppDomain.CurrentDomain.BaseDirectory, out _);
            return currentSysCompDbId;
        }

        public static int GetParentSysCompDbId()
        {
            return GetCurrentSysCompDbDTO()?.ParentSysCompDbId ?? 0;
        }

        public static List<SysCompDBDTO> GetSysCompDbsOfSameType(bool includeProductionOnDemo = false)
        {
            var db = GetCurrentSysCompDbDTO();
            var dbs = GetSysCompDBDTOs();
            dbs = dbs.Where(w => w.Type == db.Type).ToList();

            if (includeProductionOnDemo && db.Type == SysCompDBType.Demo)
                dbs = GetSysCompDBDTOs().Where(w => w.Type == SysCompDBType.Production || w.Type == SysCompDBType.Demo).ToList();
            else if (db.Type == SysCompDBType.Test)
            {
                if (db.ApiUrl.ToLower().Contains("dev"))
                    dbs = dbs.Where(w => w.ApiUrl.ToLower().Contains("dev")).ToList();
                else if (db.ApiUrl.ToLower().Contains("stage"))
                    dbs = dbs.Where(w => w.ApiUrl.ToLower().Contains("stage")).ToList();
                else if (db.ApiUrl.ToLower().Contains("mirror"))
                    dbs = dbs.Where(w => w.ApiUrl.ToLower().Contains("mirror")).ToList();
            }

            return dbs.ToList();
        }

        public static List<EntityConnection> GetEntityConnectionsForDbsOfTheSameType(bool includeProductionOnDemo = false)
        {
            var dbs = GetSysCompDbsOfSameType(includeProductionOnDemo);
            List<EntityConnection> entityConnections = new List<EntityConnection>();

            foreach (var db in dbs)
            {
                var builder = GetSqlConnectionStringBuilder(db.SysCompDbId);

                if (builder != null)
                {
                    var entityConnection = new EntityConnection(builder.ToString());
                    entityConnections.Add(entityConnection);
                }
            }

            return entityConnections;
        }
        public static string GetCurrentUrl()
        {
            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    var url = UrlUtil.GetCurrentAuthorityUrl(HttpContext.Current.Request);
                    if (url != null && !url.Contains("127.") && !url.ToLower().Contains("localhost"))
                    {
                        return url;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("GetCurrentUrl " + ex.ToString());
            }

            var currentFolderName = GetCurrentFolderName();
            if (currentFolderName == "NewSource")
                return "https://main.softone.se/";

            var machineName = Environment.MachineName.ToLower();
            var machineServer = GetStatusServerDTOs().FirstOrDefault(w => w.Name.ToLower() == machineName);
            var sysCompDBs = GetSysCompDBDTOs();

            foreach (var sysCompDb in sysCompDBs.Where(w => w.Type != SysCompDBType.Test))
            {
                var possibleFolderNames = sysCompDb.GetPossibleFolderNames();

                if (sysCompDb.Type == SysCompDBType.Demo && currentFolderName.ToLower().Contains("demo"))
                {
                    var service = GetStatusServiceDTOs().FirstOrDefault(f => f.SysCompDbId == sysCompDb.SysCompDbId);
                    if (service != null)
                        return $"https://{currentFolderName}.softone.se/";
                }

                if (possibleFolderNames.Contains(currentFolderName.ToLower()))
                {
                    var service = GetStatusServiceDTOs().FirstOrDefault(f => f.SysCompDbId == sysCompDb.SysCompDbId);
                    if (service != null)
                        return machineServer != null ? $"https://s{machineServer.StatusServerId}s1d{service.SysCompDbId}.softone.se/" : service.Url;
                }
            }

            if (machineServer != null)
            {
                foreach (var sysCompDb in sysCompDBs.Where(w => w.Type != SysCompDBType.Production))
                {
                    var possibleFolderNames = sysCompDb.GetPossibleFolderNames();
                    if (possibleFolderNames.Contains(currentFolderName.ToLower()))
                    {
                        var service = GetStatusServiceDTOs().FirstOrDefault(f => f.SysCompDbId == sysCompDb.SysCompDbId);
                        if (service != null)
                            return service.Url;
                    }
                }
            }

            return $"https://{currentFolderName}.softone.se/";
        }

        public static string GetCurrentFolderName()
        {
            var currentFilePathDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var sysCompDbId = GetCurrentSysCompDbId(GetSysCompDBDTOs(), currentFilePathDirectory, out string foundFolderName);
            return foundFolderName;
        }

        public static string GetCurrentAppName()
        {
            var currentFolder = GetCurrentFolderName();

            if (string.IsNullOrEmpty(currentFolder))
                return "UnDefined";

            if (currentFolder.ToLower().Contains("wsx"))
                return "Wsx";

            if (currentFolder.ToLower().Contains("internal"))
                return "WebApiInternal";

            if (currentFolder.ToLower().Contains("api"))
                return "Webapi";

            if (currentFolder.ToLower().Contains("web"))
                return "Webforms";

            return "Unknown";
        }

        public static void StartWebApi()
        {
            if (GetCurrentFolderName() == "NewSource" && AppDomain.CurrentDomain.BaseDirectory.Contains(@"Soe.Web\"))
            {
                var currentUrl = GetCurrentUrl();
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    var url = new Uri(currentUrl).EnsureTrailingSlash().ToString() + "api";

                    RestClientOptions options = new RestClientOptions(url);
#if DEBUG
                     options = new RestClientOptions(url)
                    {
                        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                    };
#endif

                    using var client = new RestClient(options);
                    var result = client.Get(new RestRequest());
                }
            }
        }

        public static int GetCurrentSysCompDbId(IEnumerable<SysCompDBDTO> sysCompDBDTOs, string currentFilePathDirectory, out string foundFolderName)
        {
            foundFolderName = null;
            if (currentFilePathDirectory.Contains("NewSource"))
            {
                foundFolderName = "NewSource";
                if (currentFilePathDirectory.Contains("NewSource"))
                {
                    foundFolderName = "NewSource";
                    var connectionString = ConfigurationManager.ConnectionStrings["SoeCompEntities"]?.ConnectionString;
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        connectionString = connectionString.ToLower();

                        //                        SysCompDbId SysCompServerId Name Description ApiUrl
                        //                        70  6   SoeCompv_65 Mirror Major 3  https://mirrors1d65.softone.se/apiinternal
                        //                        71  7   SoeCompv_65 Stage Major 3   https://stages1d65.softone.se/apiinternal
                        //                        72  8   SoeCompv_65 Dev Major 3 https://devs1d65.softone.se/apiinternal
                        //                        73  6   SoeCompv_66 Mirror Major 4  https://mirrors1d66.softone.se/apiinternal
                        //                        74  7   SoeCompv_66 Stage Major 4   https://stages1d66.softone.se/apiinternal
                        //                        75  8   SoeCompv_66 Dev Major 4 https://devs1d66.softone.se/apiinternal
                        //                        76  6   SoeCompv_67 Mirror Major 5  https://mirrors1d67.softone.se/apiinternal
                        //                        77  7   SoeCompv_67 Stage Major 5   https://stages1d67.softone.se/apiinternal
                        //                        78  8   SoeCompv_67 Dev Major 5 https://devs1d67.softone.se/apiinternal
                        //                        79  6   SoeCompv_68 Mirror Major 6  https://mirrors1d68.softone.se/apiinternal
                        //                        80  7   SoeCompv_68 Stage Major 6   https://stages1d68.softone.se/apiinternal
                        //                        81  8   SoeCompv_68 Dev Major 6 https://devs1d68.softone.se/apiinternal
                        //                        82  6   soeintegrationtests Mirror integration tests    https://mirrorintegrationtests.softone.se
                        //                        83  6   MIrrore2e MirrorE2E   https://MirroE2E.softone.se/apiinternal

                        // Mirror related conditions
                        if (connectionString.Contains("mirror"))
                        {
                            if (connectionString.Contains("soecompv2"))
                                return 21;
                            if (connectionString.Contains("soecompv_ref"))
                                return 22;
                            if (connectionString.Contains("soecompv_ica"))
                                return 23;
                            if (connectionString.Contains("soecompv_8"))
                                return 24;
                            if (connectionString.Contains("soecompv_18"))
                                return 25;
                            if (connectionString.Contains("soecompv_19"))
                                return 26;
                            if (connectionString.Contains("soecompv_39"))
                                return 41;
                            if (connectionString.Contains("soecompv_40"))
                                return 42;
                            if (connectionString.Contains("soecompv_50"))
                                return 52;
                            if (connectionString.Contains("soecompv_65"))
                                return 70;
                            if (connectionString.Contains("soecompv_66"))
                                return 73;
                            if (connectionString.Contains("soecompv_67"))
                                return 76;
                            if (connectionString.Contains("soecompv_68"))
                                return 79;
                        }
                        // Stage related conditions
                        else if (connectionString.Contains("stage"))
                        {
                            if (connectionString.Contains("soecompv2"))
                                return 27;
                            if (connectionString.Contains("soecompv_ref"))
                                return 28;
                            if (connectionString.Contains("soecompv_ica"))
                                return 29;
                            if (connectionString.Contains("soecompv_8"))
                                return 30;
                            if (connectionString.Contains("soecompv_18"))
                                return 31;
                            if (connectionString.Contains("soecompv_19"))
                                return 32;
                            if (connectionString.Contains("soecompv_39"))
                                return 43;
                            if (connectionString.Contains("soecompv_40"))
                                return 44;
                            if (connectionString.Contains("soecompv_65"))
                                return 71;
                            if (connectionString.Contains("soecompv_66"))
                                return 74;
                            if (connectionString.Contains("soecompv_67"))
                                return 77;
                            if (connectionString.Contains("soecompv_68"))
                                return 80;
                            if (connectionString.Contains("soecompv_testtemplate"))
                                return 47;
                            if (connectionString.Contains("soecompv_test"))
                                return 48;
                            if (connectionString.Contains("soecompv_50"))
                                return 54;
                            if (connectionString.Contains("stagee2e"))
                                return 64;
                        }
                        // Development related conditions
                        else if (connectionString.Contains("dev"))
                        {
                            if (connectionString.Contains("soedemo"))
                                return 9;
                            if (connectionString.Contains("soecompv_8paxf"))
                                return 16;
                            if (connectionString.Contains("devdemo"))
                                return 20;
                            if (connectionString.Contains("soecompv2"))
                                return 33;
                            if (connectionString.Contains("soecompv_ref"))
                                return 34;
                            if (connectionString.Contains("soecompv_ica"))
                                return 35;
                            if (connectionString.Contains("soecompv_8"))
                                return 36;
                            if (connectionString.Contains("soecompv_18"))
                                return 37;
                            if (connectionString.Contains("soecompv_19"))
                                return 38;
                            if (connectionString.Contains("soecompv_39"))
                                return 45;
                            if (connectionString.Contains("soecompv_40"))
                                return 46;
                            if (connectionString.Contains("soecompv_65"))
                                return 72;
                            if (connectionString.Contains("soecompv_66"))
                                return 75;
                            if (connectionString.Contains("soecompv_67"))
                                return 78;
                            if (connectionString.Contains("soecompv_68"))
                                return 81;
                            if (connectionString.Contains("soecompv_test"))
                                return 49;
                            if (connectionString.Contains("soecompv_50"))
                                return 56;
                            if (connectionString.Contains("integrationtests"))
                                return 62;
                            if (connectionString.Contains("deve2e"))
                                return 63;
                        }
                    }
                }


                return 9;
            }

            foreach (var item in GetSysCompDBDTOs())
            {
                var possibleDomainNames = item.GetPossibleFolderNames();
                var currentFilePath = currentFilePathDirectory;

                while (!string.IsNullOrEmpty(currentFilePath))
                {
                    string currentFolderName = Path.GetFileName(currentFilePath);
                    currentFolderName = currentFolderName.TrimEnd(Path.DirectorySeparatorChar);
                    if (string.IsNullOrEmpty(currentFolderName))
                    {
                        currentFolderName = Path.GetFileName(Path.GetDirectoryName(currentFilePath));
                    }

                    if (!string.IsNullOrEmpty(currentFolderName) && possibleDomainNames.Contains(currentFolderName.ToLower()))
                    {
                        foundFolderName = currentFolderName;
                        return item.SysCompDbId;
                    }

                    currentFilePath = Path.GetDirectoryName(currentFilePath);
                }
            }

            return -1; // SysCompDbId not found
        }

        public static List<SysCompDBDTO> GetSysCompDBDTOs()
        {
            if (sysCompDBDTOs.IsNullOrEmpty())
            {
                sysCompDBDTOs = FetchSysCompDBDTOsFromDisk();
                if (!sysCompDBDTOs.IsNullOrEmpty())
                    Task.Run(() => SetSysCompDBDTOs());
                else
                    sysCompDBDTOs = FetchSysCompDBDTOs();
            }
            return sysCompDBDTOs;
        }

        private static void SetSysCompDBDTOs()
        {
            sysCompDBDTOs = FetchSysCompDBDTOs();
        }

        public static string GetUrlFromSysCompDbId(int sysCompDbId)
        {
            var sysCompDBDTO = GetSysCompDBDTOs().FirstOrDefault(f => f.SysCompDbId == sysCompDbId);
            return sysCompDBDTO?.ApiUrl.ToLower().Replace("apix", "apiinternal");
        }

        private static List<StatusServiceDTO> GetStatusServiceDTOs()
        {
            if (statusServiceDTOs.IsNullOrEmpty())
            {
                statusServiceDTOs = FetchStatusServiceDTOsFromDisk();

                if (!statusServiceDTOs.IsNullOrEmpty())
                    Task.Run(() => SetStatusServiceDTOs());
                else
                    statusServiceDTOs = FetchStatusServiceDTOs();
            }
            return statusServiceDTOs;
        }

        private static void SetStatusServiceDTOs()
        {
            statusServiceDTOs = FetchStatusServiceDTOs();
        }

        private static List<StatusServerDTO> GetStatusServerDTOs()
        {
            if (statusServerDTOs.IsNullOrEmpty())
            {
                statusServerDTOs = FetchStatusServerDTOsFromDisk();

                if (!statusServerDTOs.IsNullOrEmpty())
                    Task.Run(() => SetStatusServerDTOs());
                else
                    statusServerDTOs = FetchStatusServerDTOs();
            }
            return statusServerDTOs;
        }

        private static void SetStatusServerDTOs()
        {
            statusServerDTOs = FetchStatusServerDTOs();
        }

        private static List<StatusServiceDTO> FetchStatusServiceDTOs()
        {
            List<StatusServiceDTO> statusServices = new List<StatusServiceDTO>();

            try
            {
                statusServices = SoftOneStatusConnector.GetStatusServices();
                if (!statusServices.IsNullOrEmpty())
                {
                    SaveStatusServiceDTOsToDisk(statusServices);
                }
                else
                {
                    statusServices = FetchStatusServiceDTOsFromDisk();
                }
            }
            catch (Exception ex)
            {
                var message = "FetchStatusServiceDTOs" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
                statusServices = FetchStatusServiceDTOsFromDisk();
            }

            return statusServices;
        }

        private static List<StatusServiceDTO> FetchStatusServiceDTOsFromDisk()
        {
            try
            {
                if (File.Exists(statusServicesFilePath))
                {
                    string json = File.ReadAllText(statusServicesFilePath);
                    return JsonConvert.DeserializeObject<List<StatusServiceDTO>>(json);
                }
            }
            catch (Exception ex)
            {
                var message = "FetchStatusServiceDTOsFromDisk" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
            }

            return new List<StatusServiceDTO>();
        }

        private static void SaveStatusServiceDTOsToDisk(List<StatusServiceDTO> StatusServiceDTOs)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(statusServicesFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(statusServicesFilePath));

                string json = JsonConvert.SerializeObject(StatusServiceDTOs);
                File.WriteAllText(statusServicesFilePath, json);
            }
            catch (Exception ex)
            {
                var message = "SaveStatusServiceDTOsToDisk" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
            }
        }

        private static List<StatusServerDTO> FetchStatusServerDTOs()
        {
            List<StatusServerDTO> statusServices;

            try
            {
                statusServices = SoftOneStatusConnector.GetStatusServers();
                if (!statusServices.IsNullOrEmpty())
                {
                    SaveStatusServerDTOsToDisk(statusServices);
                }
                else
                {
                    statusServices = FetchStatusServerDTOsFromDisk();
                }
            }
            catch (Exception ex)
            {
                var message = "FetchStatusServerDTOs" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogInfo(message);
                statusServices = FetchStatusServerDTOsFromDisk();
            }

            return statusServices;
        }

        private static List<StatusServerDTO> FetchStatusServerDTOsFromDisk()
        {
            try
            {
                if (File.Exists(statusServersFilePath))
                {
                    string json = File.ReadAllText(statusServersFilePath);
                    return JsonConvert.DeserializeObject<List<StatusServerDTO>>(json);
                }
            }
            catch (Exception ex)
            {
                var message = "FetchStatusServerDTOsFromDisk" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
            }

            return new List<StatusServerDTO>();
        }

        private static void SaveStatusServerDTOsToDisk(List<StatusServerDTO> StatusServerDTOs)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(statusServersFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(statusServersFilePath));

                string json = JsonConvert.SerializeObject(StatusServerDTOs);
                File.WriteAllText(statusServersFilePath, json);
            }
            catch (Exception ex)
            {
                var message = "SaveStatusServerDTOsToDisk" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogInfo(message);
            }
        }

        public static List<SysCompDBDTO> FetchSysCompDBDTOs()
        {
            List<SysCompDBDTO> sysCompDBs = new List<SysCompDBDTO>();

            try
            {
                sysCompDBs = SoftOneStatusConnector.GetAllSysCompDBTOs();

                if (sysCompDBs.IsNullOrEmpty())
                    sysCompDBs = ssm.GetSysCompDBs();

                if (!sysCompDBs.IsNullOrEmpty())
                {
                    SaveSysCompDBDTOsToDisk(sysCompDBs);
                }
                else
                {
                    sysCompDBs = FetchSysCompDBDTOsFromDisk();
                }
            }
            catch (Exception ex)
            {
                var message = "FetchSysCompDBDTOs" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
                sysCompDBs = FetchSysCompDBDTOsFromDisk();
            }

            return sysCompDBs;
        }

        private static List<SysCompDBDTO> FetchSysCompDBDTOsFromDisk()
        {
            try
            {
                string jsonString = File.ReadAllText(sysCompDBsFilePath);
                List<SysCompDBDTO> sysCompDBDTOFromDisk = JsonConvert.DeserializeObject<List<SysCompDBDTO>>(jsonString);
                return sysCompDBDTOFromDisk;
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex.Message);

                    var firstPartOfFileName = sysCompDBsFilePath.Split('_')[0];

                    string[] alternativeFileEntries = Directory.GetFiles(Directory.GetDirectoryRoot(sysCompDBsFilePath));

                    foreach (string fileName in alternativeFileEntries.Where(w => w.StartsWith(firstPartOfFileName)))
                    {
                        try
                        {
                            string jsonString = File.ReadAllText(fileName);
                            List<SysCompDBDTO> sysCompDBDTOsFromDisk = JsonConvert.DeserializeObject<List<SysCompDBDTO>>(jsonString);
                            return sysCompDBDTOsFromDisk;
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2.Message);
                        }
                    }
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }
            return new List<SysCompDBDTO>();
        }

        private static void SaveSysCompDBDTOsToDisk(List<SysCompDBDTO> sysCompDBDTOs)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(sysCompDBsFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(sysCompDBsFilePath));

                string json = JsonConvert.SerializeObject(sysCompDBDTOs);
                File.WriteAllText(sysCompDBsFilePath, json);
            }
            catch (Exception ex)
            {
                var message = "SaveSysCompDBDTOsToDisk" + (ex.ToString());
                LogMessage(message);
                LogCollector.LogCollector.LogInfo(message);
            }
        }

        public static string CreateFileName(string fileName, string extension)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string sanitizedBaseDirectory = baseDirectory.Replace('\\', '_').Replace(':', '_').Replace(".", "");
            return fileName + "_" + sanitizedBaseDirectory + "." + extension;
        }

        public static void LogMessage(string message)
        {
            string logEntry = $"{DateTime.Now} - {AppDomain.CurrentDomain.BaseDirectory} - {message}{Environment.NewLine}";

            try
            {
                lock (logFileLock)
                {
                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }
        public static void LogMessageWithTimer(string actionName, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogMessage($"'{actionName}' encountered an error: {ex}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                LogMessage($"'{actionName}' done, completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        public static string LogFilePath
        {
            get
            {
                var fullPath = Path.Combine(@"C:\temp\", CreateFileName("ConfigurationSetup", "log"));
                if (!Directory.Exists(@"C:\temp\"))
                    Directory.CreateDirectory(@"C:\temp\");
                return fullPath;
            }
        }

        public static void CleanOldLogPosts()
        {
            const long maxFileSizeBytes = 2L * 1024 * 1024; // 2 MB
            const double keepRatio = 0.5;                   // keep newest ~50%
            long targetBytes = (long)(maxFileSizeBytes * keepRatio);

            try
            {
                if (!File.Exists(LogFilePath))
                    return;

                var fileInfo = new FileInfo(LogFilePath);
                if (fileInfo.Length <= maxFileSizeBytes)
                    return;

                var allLines = File.ReadAllLines(LogFilePath);

                var linesToKeep = new List<string>(capacity: Math.Min(allLines.Length, 4096));
                long accumulatedBytes = 0;

                for (int i = allLines.Length - 1; i >= 0; i--)
                {
                    string line = allLines[i];
                    int lineBytes = System.Text.Encoding.UTF8.GetByteCount(line) + 2; // 2 bytes for newline

                    if (accumulatedBytes + lineBytes > targetBytes)
                        break;

                    accumulatedBytes += lineBytes;
                    linesToKeep.Add(line);
                }

                linesToKeep.Reverse();

                // safer write: temp + replace
                string tempPath = LogFilePath + ".tmp";
                File.WriteAllLines(tempPath, linesToKeep);
                File.Copy(tempPath, LogFilePath, overwrite: true);
                File.Delete(tempPath);
                LogMessage("CleanOldLogPosts: Log file trimmed successfully.");
            }
            catch (Exception ex)
            {
                var message = "CleanOldLogPosts error: " + ex;
                LogMessage(message);
                LogCollector.LogCollector.LogError(message);
            }
        }

        public static Uri GetEvoUrl()
        {
            var sysCompDbDTO = GetCurrentSysCompDbDTO();
            if (sysCompDbDTO != null)
            {
                var apiUrl = sysCompDbDTO.ApiUrl;
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    if (IsTestBasedOnMachine())
                    {
                        if (apiUrl.ToLower().Contains("dev"))
                        {
#if DEBUG

                            return new Uri("https://evo.softone.se:7061");
#else
                            return new Uri("https://dev.softone.se");
#endif
                        }
                        if (apiUrl.ToLower().Contains("stage"))
                            return new Uri("https://stage.softone.se");
                        if (apiUrl.ToLower().Contains("mirror"))
                            return new Uri("https://mirror.softone.se");
                    }
                    else
                        return GetValidProductionEvoUrl();
                }
            }

            return new Uri("https://evo.softone.se:6071");
        }

        public static Uri GetValidProductionEvoUrl()
        {
            var evoUrl = new Uri("https://app.softone.se");

            if (GetSiteType() == TermGroup_SysPageStatusSiteType.Beta)
                return new Uri("https://appb.softone.se");

            try
            {
                string key = "EvoInternalApiUrl";
                var fromCache = BusinessMemoryCache<Uri>.Get(key);

                if (fromCache != null)
                    return fromCache;

                var url = new Uri(SoftOneStatusConnector.GetEvoUrl());

                BusinessMemoryCache<Uri>.Set(key, url, 240);

                return url;
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError("GetEvoInternalApiString" + (ex.ToString()));
                return evoUrl;
            }
        }

        public static List<Uri> GetBackupEvoInternalApi()
        {
            if (IsTestBasedOnMachine())
            {
                return new List<Uri>();
            }

            List<Uri> backupUris = new List<Uri>();
            var currentDefault = GetEvoUrl();

            foreach (var serverId in HostServerIds)
                backupUris.Add(new Uri(currentDefault.ToString().ToLower().Replace(".softone.se", $"s{serverId}.softone.se")));

            return backupUris;
        }
        public static void InitRunLocally()
        {
            if (IsTestBasedOnMachine())
            {
                if (File.Exists(runLocallyPath))
                {
                    string json = File.ReadAllText(runLocallyPath);
                    runLocally = JsonConvert.DeserializeObject<RunLocally>(json);
                }
                else
                {
                    var rl = new RunLocally { RunLocalDatabase = false, LastRestore = DateTime.UtcNow };
                    SaveRunLocally(rl);
                    runLocally = rl;
                }
            }
            else
            {
                runLocally = new RunLocally { RunLocalDatabase = false, LastRestore = DateTime.UtcNow };
            }
        }

        private static string runLocallyPath = @"C:\temp\RunLocally.json";

        public static void SaveRunLocally(RunLocally rl)
        {
            if (IsTestBasedOnMachine())
            {
                //Save to disk
                if (File.Exists(runLocallyPath))
                    File.Delete(runLocallyPath);

                if (!File.Exists(runLocallyPath))
                    File.WriteAllText(runLocallyPath, JsonConvert.SerializeObject(rl));
            }
        }

        public static string GetTestPrefix()
        {
            if (IsTestBasedOnMachine())
            {
                var sysCompDbDTO = GetCurrentSysCompDbDTO();
                if (sysCompDbDTO != null)
                {
                    if (sysCompDbDTO.SysCompServerDTO.Name.ToLower().Contains("dev"))
                        return "dev";

                    if (sysCompDbDTO.SysCompServerDTO.Name.ToLower().Contains("stage"))
                        return "stage";

                    if (sysCompDbDTO.SysCompServerDTO.Name.ToLower().Contains("mirror"))
                        return "mirror";
                }
            }
            return string.Empty;
        }
        private static string blobStorageConnectionString { get; set; }
        public static string GetBlobStorageConnectionString()
        {
            if (!string.IsNullOrEmpty(blobStorageConnectionString))
                return blobStorageConnectionString;
            var keyvaultSettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            var connString = KeyVaultSecretsFetcher.GetSecret(keyvaultSettings, "AzureBlobStorageConnectionString", keyvaultSettings.StoreLocation);

            if (!string.IsNullOrEmpty(connString))
                blobStorageConnectionString = connString;
            return blobStorageConnectionString;
        }

        public static Uri GetEvoUrlByTestPrefix()
        {
            var prefix = GetTestPrefix();
            if (!string.IsNullOrEmpty(prefix))
                return new Uri($"https://{prefix}.softone.se");
            return new Uri("https://evo:softone.se:7061");
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static void ApplicationInsightSetup()
        {
            try
            {
                var vaultSettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
                if (vaultSettings == null)
                {
                    LogMessage("ApplicationInsightSetup: KeyVaultSettings not available.");
                    return;
                }

                // Try multiple secret names for compatibility across environments
                var kvKey = "SOE-AppInsights-InstrumentationKey";
                string secretValue = null;

                try
                {
                    secretValue = KeyVaultSecretsFetcher.GetSecret(vaultSettings, kvKey, vaultSettings.StoreLocation);
                    if (string.IsNullOrWhiteSpace(secretValue))
                    {
                        LogMessage($"ApplicationInsightSetup: no Application Insights secret found in Key Vault.");
                        return;
                    }

                    LogMessage($"ApplicationInsightSetup: fetched secret '{kvKey}' from Key Vault (length={secretValue.Length}).");
                }
                catch (Exception ex)
                {
                    // Non-fatal; continue trying other names
                    LogMessage($"ApplicationInsightSetup: failed to fetch secret '{kvKey}': {ex.Message}");
                }

                string connectionString = secretValue.Trim();

                // Apply the connection string to the active TelemetryConfiguration if available
                if (TelemetryConfiguration.Active != null)
                {
                    TelemetryConfiguration.Active.ConnectionString = connectionString;
                    LogMessage("ApplicationInsightSetup: TelemetryConfiguration.Active.ConnectionString set.");
                }
                else
                {
                    // Active may be read-only or null; create a temporary configuration and TelemetryClient so telemetry works locally
                    var cfg = TelemetryConfiguration.CreateDefault();
                    cfg.ConnectionString = connectionString;
                    var tmpClient = new Microsoft.ApplicationInsights.TelemetryClient(cfg);
                    _ = tmpClient;
                    LogMessage("ApplicationInsightSetup: Temporary TelemetryClient created with connection string.");
                }
            }
            catch (Exception ex)
            {
                LogMessage("ApplicationInsightSetup failed: " + ex.ToString());
            }
        }
    }

    public class RunLocally
    {
        public bool RunLocalDatabase { get; set; } = false;
        public DateTime LastRestore { get; set; } = DateTime.MinValue;
        public DateTime NextRestore { get; set; } = DateTime.MaxValue;
    }
}

