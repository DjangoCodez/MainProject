using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SO.Internal.Shared.Api.Cache.DistrubutedCache;
using System.Linq;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Util.LogCollector;
using MessagePack.Resolvers;

namespace SoftOne.Soe.Business.DataCache
{
    public enum CacheType
    {
        Unknown = 0,
        System = 1,

        License = 101,

        Company = 201,
        CompanyAndKey = 202,
        CompanyAndId = 203,

        User = 301,
    }

    public enum CacheTTL
    {
        Unknown = 0,
        Default = 300, // 5 * 60
        ThirtyMinutes = 1800,   //30 * 60     
    }

    public class CacheConfig
    {
        public CacheType Type { get; private set; }
        public Guid? Key { get; private set; }
        public int Id1 { get; private set; }
        public int Id2 { get; private set; }
        public int Seconds { get; private set; }
        public int LicenseId { get; private set; }
        public int ActorCompanyId { get; private set; }
        public int RoleId { get; private set; }
        public int? UserId { get; private set; }
        public bool DiscardCache { get; private set; }
        public bool KeepAlive { get; private set; }
        public BusinessMemoryDistributionSetting BusinessMemoryDistributionSetting { get; private set; } = BusinessMemoryDistributionSetting.Disabled;

        private const int SECONDS = 5 * 60;

        private CacheConfig() { }

        public static CacheConfig System(int? seconds = null, bool discardCache = false)
        {
            return new CacheConfig()
            {
                Type = CacheType.System,
                Seconds = seconds ?? SECONDS,
                DiscardCache = discardCache,
            };
        }

        public static CacheConfig Company(int actorCompanyId, int? seconds = null, bool discardCache = false)
        {
            return new CacheConfig()
            {
                Type = CacheType.Company,
                ActorCompanyId = actorCompanyId,
                Seconds = seconds ?? SECONDS,
                DiscardCache = discardCache,
            };
        }

        public static CacheConfig Company(int actorCompanyId, Guid? key, int? seconds = null, bool discardCache = false, bool keepAlive = false)
        {
            return new CacheConfig()
            {
                Type = key != null && key != Guid.Empty ? CacheType.CompanyAndKey : CacheType.Company,
                ActorCompanyId = actorCompanyId,
                Key = key,
                Seconds = seconds ?? SECONDS,
                DiscardCache = discardCache,
                KeepAlive = keepAlive,
            };
        }

        public static CacheConfig Company(int actorCompanyId, int id1, int id2, int? seconds = null, bool discardCache = false)
        {
            return new CacheConfig()
            {
                Type = CacheType.CompanyAndId,
                ActorCompanyId = actorCompanyId,
                Id1 = id1,
                Id2 = id2,
                Seconds = seconds ?? SECONDS,
                DiscardCache = discardCache,
            };
        }

        public static CacheConfig User(int actorCompanyId, int userId, int? seconds = null, bool discardCache = false, int? roleId = null)
        {
            return new CacheConfig()
            {
                Type = CacheType.User,
                ActorCompanyId = actorCompanyId,
                UserId = userId,
                Key = null,
                Seconds = seconds ?? SECONDS,
                DiscardCache = discardCache,
                RoleId = roleId ?? 0,
            };
        }

        public static CacheConfig License(int licenseId, int? seconds = null)
        {
            return new CacheConfig()
            {
                Type = CacheType.License,
                LicenseId = licenseId,
                ActorCompanyId = 0,
                RoleId = 0,
                Key = null,
                Seconds = seconds ?? SECONDS,
            };
        }

        public string GetCacheKey(int type, string optionalKeyPart = null)
        {
            string key = null;

            if (this.DiscardCache)
                return key;

            if (this.Type == CacheType.System)
                key += $"System_{type}";
            if (this.Type == CacheType.License)
                key = $"License_{this.LicenseId}_{type}_{DateTime.Today.ToString("yyyyMMdd")}";
            else if (this.Type == CacheType.Company)
                key = $"Company_{this.ActorCompanyId}_{type}";
            else if (this.Type == CacheType.CompanyAndKey && (this.Key != null && this.Key != Guid.Empty))
                key = $"Company_{this.ActorCompanyId}_{type}_{this.Key}";
            else if (this.Type == CacheType.CompanyAndId && this.Id1 != 0)
                key = $"Company_{this.ActorCompanyId}_{type}_{this.Id1}_{this.Id2}";
            else if (this.Type == CacheType.User)
                key = $"User_{this.ActorCompanyId}_{UserId}_{RoleId}_{type}";

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(optionalKeyPart))
                key += $"_{optionalKeyPart}";

            if (string.IsNullOrEmpty(key))
            {
                key = $"Unknown_{type}" + Guid.NewGuid().ToString(); //Makes the data is not accesable to other companies/requests
                var json = JsonConvert.SerializeObject(this);
                LogCollector.LogError("Invalid use of cache, key is: " + json);
            }

            key = $"db{ConfigurationSetupUtil.GetCurrentSysCompDbId()}k#{key}";

            return key;
        }

        public void SetBusinessMemoryDistributionSetting(BusinessMemoryDistributionSetting setting)
        {
            BusinessMemoryDistributionSetting = setting;
        }
    }

    public enum BusinessMemoryDistributionSetting
    {
        ConservativeHybridCache = 0,
        FullyHybridCache = 1,
        Disabled = 2
    }

    public interface IBusinessMemoryCache
    {
        T Get<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled);
        bool TryGetValue<T>(string key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled);
        void Set<T>(string key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled);
        bool Delete<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled);
        bool Contains<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled);
    }

    internal sealed class OldCacheAdapter : IBusinessMemoryCache
    {
        public T Get<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheOld<T>.Get(key);
        public bool TryGetValue<T>(string key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheOld<T>.TryGetValue(key, out value);
        public void Set<T>(string key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheOld<T>.Set(key, value, seconds);
        public bool Delete<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheOld<T>.Delete(key);
        public bool Contains<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheOld<T>.Contains(key);
    }

    internal sealed class NewCacheAdapter : IBusinessMemoryCache
    {
        public T Get<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheNew<T>.Get(key, distributionSetting);
        public bool TryGetValue<T>(string key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheNew<T>.TryGetValue(key, out value, distributionSetting);
        public void Set<T>(string key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheNew<T>.Set(key, value, seconds, distributionSetting);
        public bool Delete<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheNew<T>.Delete(key, distributionSetting);
        public bool Contains<T>(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => BusinessMemoryCacheNew<T>.Contains(key, distributionSetting);
    }

    public static class BusinessMemoryCacheRouting
    {
        private static readonly IBusinessMemoryCache _old = new OldCacheAdapter();
        private static readonly IBusinessMemoryCache _new = new NewCacheAdapter();

        public enum Mode { OldOnly, NewOnly }

        private static int _mode =
        ConfigurationSetupUtil.UseL1AndL2Cache() ? (int)Mode.NewOnly : (int)Mode.OldOnly;

        /// <summary>
        /// Thread-safe read of the current cache mode
        /// </summary>        
        public static Mode CurrentMode => (Mode)Interlocked.CompareExchange(ref _mode, 0, 0);

        /// <summary>
        /// Set the current cache mode
        /// </summary>
        public static void SetMode(Mode mode) =>
            Interlocked.Exchange(ref _mode, (int)mode);

        /// <summary>
        /// The Current Active Cache Implementation
        /// </summary>
        public static IBusinessMemoryCache Active
        {
            get
            {
                // If tripped, force OldOnly mode
                if (FailBackToOldIsTripped)
                    return _old;
                return CurrentMode == Mode.NewOnly ? _new : _old;
            }
        }

        /// <summary>
        /// Track the number of consecutive failures in the new cache implementation.
        /// </summary>
        private static int _fails;
        /// <summary>
        /// This field is used to track when the breaker was tripped until.
        /// </summary>
        private static long _tripUntilTicks;
        /// <summary>
        /// Represents the threshold value used to determine when a trip condition is met.
        /// </summary>
        private const int TripThreshold = 15;
        /// <summary>
        /// How long to stay in OldOnly mode when tripped
        /// </summary>
        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);
        /// <summary>
        /// true if we are currently in the cooldown period after a trip
        /// </summary>
        public static bool FailBackToOldIsTripped => DateTime.UtcNow.Ticks < Interlocked.Read(ref _tripUntilTicks);
        /// <summary>
        /// If the new cache implementation operation succeeded, call this to reset failure count.
        /// </summary>
        public static void RecordSuccess() => Interlocked.Exchange(ref _fails, 0);
        /// <summary>
        /// If the new cache implementation operation failed, call this to increment failure count
        /// </summary>
        public static void RecordFailure()
        {
            if (Interlocked.Increment(ref _fails) >= TripThreshold)
            {
                Interlocked.Exchange(ref _fails, 0);
                Interlocked.Exchange(ref _tripUntilTicks, DateTime.UtcNow.Add(Cooldown).Ticks);
                SetMode(Mode.OldOnly);
                SysLogConnector.LogErrorString("[Cache] Breaker tripped → OldOnly for 1 min.");
            }
        }
    }

    // Shared, non-generic settings moved out to avoid static per-generic-type behavior
    internal static class BusinessMemoryCacheSettings
    {
        // How often we re-read ConfigurationSetupUtil (tune as needed)
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

        // Backing fields (mutable, updated atomically)
        private static int _acceptSeconds = ConfigurationSetupUtil.CacheIntAcceptSeconds();
        private static int _checkIntervalSeconds = ConfigurationSetupUtil.CacheCheckIntervalSeconds();
        private static int _leaseSeconds = ConfigurationSetupUtil.CacheLeaseSeconds();

        // Last refresh timestamp (ticks) - used with Interlocked.Read/Exchange
        private static long _lastRefreshTicks = DateTime.UtcNow.Ticks;

        private static readonly object _sync = new object();

        private static void EnsureFresh()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks - Interlocked.Read(ref _lastRefreshTicks) < RefreshInterval.Ticks)
                return;

            lock (_sync)
            {
                nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks - Interlocked.Read(ref _lastRefreshTicks) < RefreshInterval.Ticks)
                    return;

                try
                {
                    var accept = ConfigurationSetupUtil.CacheIntAcceptSeconds();
                    var check = ConfigurationSetupUtil.CacheCheckIntervalSeconds();
                    var lease = ConfigurationSetupUtil.CacheLeaseSeconds();

                    Interlocked.Exchange(ref _acceptSeconds, accept);
                    Interlocked.Exchange(ref _checkIntervalSeconds, check);
                    Interlocked.Exchange(ref _leaseSeconds, lease);

                    Interlocked.Exchange(ref _lastRefreshTicks, DateTime.UtcNow.Ticks);
                }
                catch (Exception ex)
                {
                    SysLogConnector.LogErrorString($"[CacheSettings] Refresh failed: {ex}");
                    // keep last known values on error
                }
            }
        }

        internal static int AcceptSeconds { get { EnsureFresh(); return Interlocked.CompareExchange(ref _acceptSeconds, 0, 0); } }
        internal static int CheckIntervalSeconds { get { EnsureFresh(); return Interlocked.CompareExchange(ref _checkIntervalSeconds, 0, 0); } }
        internal static int LeaseSeconds { get { EnsureFresh(); return Interlocked.CompareExchange(ref _leaseSeconds, 0, 0); } }

        internal static TimeSpan DistTtlForStatus(int seconds) => TimeSpan.FromSeconds(Math.Max(seconds * 2, 60));
        internal static TimeSpan DistTtlForValue(int seconds) => TimeSpan.FromSeconds(Math.Max(seconds, 10));
    }

    // Shared global items used by BusinessMemoryCacheNew<T> (ensures single instance across all T)
    internal static class BusinessMemoryCacheShared
    {
        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max);

        internal static readonly string OwnerId = Truncate(
            $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}:{Guid.NewGuid():N}",
            48);

        internal static readonly ConcurrentDictionary<string, Lazy<SemaphoreSlim>> KeyLocks =
            new ConcurrentDictionary<string, Lazy<SemaphoreSlim>>();

        internal static SemaphoreSlim KeyLock(string nsKey) =>
            KeyLocks.GetOrAdd(nsKey, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1))).Value;

        internal static void CleanupKeyLock(string nsKey)
        {
            // Remove reference so the semaphore can be GC’d when no one holds it.
            // Do NOT Dispose here – another thread may still be using it.
            Lazy<SemaphoreSlim> _;
            KeyLocks.TryRemove(nsKey, out _);
        }

        internal static readonly ConcurrentDictionary<string, bool> ValidForDistributedCache = new ConcurrentDictionary<string, bool>();

        internal static readonly ConcurrentDictionary<string, EvoDistributionCacheStatus> CacheStatusLocal =
            new ConcurrentDictionary<string, EvoDistributionCacheStatus>();

        internal static readonly ConcurrentDictionary<string, EvoDistributionCacheStatus> CacheStatusFetched =
            new ConcurrentDictionary<string, EvoDistributionCacheStatus>();

    }

    public static class BusinessMemoryCache<T>
    {
        private static IBusinessMemoryCache Active => BusinessMemoryCacheRouting.Active;

        public static T Get(Guid? key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Get(key?.ToString(), distributionSetting);

        public static T Get(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Active.Get<T>(key, distributionSetting);

        public static bool TryGetValue(Guid? key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) =>
            Active.TryGetValue<T>(key?.ToString(), out value, distributionSetting);

        public static bool TryGetValue(string key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) =>
            Active.TryGetValue<T>(key, out value, distributionSetting);

        public static void Set(Guid? key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) =>
            Set(key?.ToString(), value, seconds, distributionSetting);

        public static void Set(string key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) =>
            Active.Set<T>(key, value, seconds, distributionSetting);

        public static bool Delete(Guid key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Delete(key.ToString(), distributionSetting);

        public static bool Delete(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Active.Delete<T>(key, distributionSetting);

        public static bool Contains(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Active.Contains<T>(key, distributionSetting);
    }

    /// <summary>
    /// New Business Memory Cache implementation with distributed cache backing and coordination.
    /// Do not use this class directly; use BusinessMemoryCache<T> instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class BusinessMemoryCacheNew<T>
    {

        #region Configuration & Constants

        private static class Prefix
        {
            public const string Status = "sts:";
            public const string Value = "val:";
            public const string Lease = "lease:";
            public const string Hash = "hash:";
        }

        private sealed class Lease
        {
            public string Owner { get; set; }
            public DateTimeOffset UntilUtc { get; set; }
            public DateTimeOffset CreatedUtc { get; set; }
        }

        private static SemaphoreSlim KeyLock(string nsKey) =>
            BusinessMemoryCacheShared.KeyLock(nsKey);

        private static MemoryCache MemoryCache => MemoryCache.Default;
        private static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        private static bool IsValidKey(string key) => !string.IsNullOrEmpty(key) && key != Guid.Empty.ToString();
        private static string NamespaceKey(string key)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) != null)
                return $"{Nullable.GetUnderlyingType(typeof(T)).FullName}:{key}";

            else
                return $"{typeof(T).FullName}:{key}";
        }
        private static string StatusKey(string nsKey) => $"{Prefix.Status}{nsKey}";
        private static string ValueKey(string nsKey) => $"{Prefix.Value}{nsKey}";
        private static string LeaseKey(string nsKey) => $"{Prefix.Lease}{nsKey}";
        private static string HashKey(string nsKey) => $"{Prefix.Hash}{nsKey}";

        private static void CleanupKeyLock(string nsKey)
        {
            BusinessMemoryCacheShared.CleanupKeyLock(nsKey);
        }

        private static CacheItemPolicy NewPolicy(string nsKey, int ttlSeconds)
        {
            return new CacheItemPolicy
            {
                AbsoluteExpiration = UtcNow.AddSeconds(Math.Max(ttlSeconds, 1)),
                RemovedCallback = _ => CleanupKeyLock(nsKey)
            };
        }
        #endregion

        #region Get
        public static T Get(Guid? key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Get(key?.ToString(), distributionSetting);
        public static T Get(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => TryGetValue(key, out var v, distributionSetting) ? v : default(T);

        public static bool TryGetValue(string key, out T value, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled)
        {
            value = default(T);
            try
            {
                if (!IsValidKey(key)) return false;

                if (distributionSetting == BusinessMemoryDistributionSetting.Disabled)
                {
                    return BusinessMemoryCacheOld<T>.TryGetValue(key, out value);
                }

                var ns = NamespaceKey(key);
                var now = UtcNow;

                var local = GetLocalEntryFromCache(ns);
                if (CheckLocalAcceptWindow(local, now, out value))
                    return true;

                var gate = KeyLock(ns);
                gate.Wait();
                try
                {
                    local = GetLocalEntryFromCache(ns);
                    if (ShouldThrottle(local, now, out value))
                        return true;

                    // If we don't have a local entry, attempt to load from distributed (   when enabled)
                    if (local == null)
                    {
                        if (distributionSetting != BusinessMemoryDistributionSetting.Disabled && IsValidForDistributedCache(default(T), 0, distributionSetting, key))
                            return TryLoadFromDistributed(ns, now, out value);

                        value = default(T);
                        return false;
                    }

                    var expired = IsLocalExpired(local, now);
                    if (!expired)
                    {
                        if (NeedsStatusCheck(local, now) && RefreshFromDistributedIfNewer(ns, local, now, out value))
                            return true;

                        ExtendAccept(local, now);
                        value = (T)local.Value;
                        return true;
                    }

                    // Local is expired. Try to acquire lease to become the refresher.
                    string winner;
                    bool acquired = TryAcquireLease(LeaseKey(ns), out winner);

                    if (!acquired)
                    {
                        // Another process owns the lease; continue serving last known value.
                        ExtendAccept(local, now);
                        value = (T)local.Value;
                        return true;
                    }

                    // We acquired the lease. Attempt to refresh from distributed cache immediately.
                    try
                    {
                        if (TryLoadFromDistributed(ns, now, out var fetched))
                        {
                            value = fetched;
                            return true;
                        }

                        // If distributed load failed, re-insert the previous local entry and serve it to avoid a miss.
                        MemoryCache.Set(ns, local, NewPolicy(ns, local.TtlSeconds));
                        value = (T)local.Value;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        SysLogConnector.LogErrorString(ex.ToString());
                        // Best-effort: restore local and serve it
                        try { MemoryCache.Set(ns, local, NewPolicy(ns, local.TtlSeconds)); } catch { /* ignore and move along */ }
                        value = (T)local.Value;
                        return true;
                    }
                }
                finally
                {
                    try { gate.Release(); }
                    catch (ObjectDisposedException) { /* already disposed elsewhere, safe to ignore */ }
                    catch (SemaphoreFullException) { /* defensive: mismatched release, ignore */ }
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.LogErrorString(ex.ToString());
                value = default(T);
                return false;
            }
        }
        #endregion

        #region Set
        public static void Set(Guid? key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => Set(key?.ToString(), value, seconds, distributionSetting);

        public static void Set(string key, T value, int seconds = 60, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled)
        {
            try
            {
                if (!IsValidKey(key)) return;

                if (distributionSetting == BusinessMemoryDistributionSetting.Disabled)
                {
                    BusinessMemoryCacheOld<T>.Set(key, value, seconds);
                    return;
                }
                var ns = NamespaceKey(key);
                var now = UtcNow;

                var newHash = ComputeHashHex(value, key);
                var existingHash = EvoDistributionCacheConnector.GetCachedValue<string>(HashKey(ns));
                var changed = !StringEqualsSafe(existingHash, newHash);

                var statusKey = StatusKey(ns);
                var status = GetStatus(statusKey) ?? new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = now };

                var batchRequests = new List<DistributedCacheUpsertRequest>();

                if (value != null && IsValidForDistributedCache(value, seconds, distributionSetting, key))
                {
                    batchRequests.Add(new DistributedCacheUpsertRequest
                    {
                        Key = ValueKey(ns),
                        Value = Newtonsoft.Json.JsonConvert.SerializeObject(value),
                        Expiration = BusinessMemoryCacheSettings.DistTtlForValue(seconds),
                        UseSlidingExpiration = false
                    });

                    batchRequests.Add(new DistributedCacheUpsertRequest
                    {
                        Key = HashKey(ns),
                        Value = Newtonsoft.Json.JsonConvert.SerializeObject(newHash),
                        Expiration = BusinessMemoryCacheSettings.DistTtlForValue(seconds),
                        UseSlidingExpiration = false
                    });

                    if (changed || status.OriginalTtlSeconds <= 0)
                    {
                        status.LastChangeUtc = now;
                        status.OriginalTtlSeconds = Math.Max(seconds, 1); // store original TTL (in seconds)
                    }

                    batchRequests.Add(new DistributedCacheUpsertRequest
                    {
                        Key = statusKey,
                        Value = Newtonsoft.Json.JsonConvert.SerializeObject(status),
                        Expiration = BusinessMemoryCacheSettings.DistTtlForStatus(seconds),
                        UseSlidingExpiration = false
                    });

                    UpsertBatchSafe(new DistributedCacheUpsertBatchRequest { Requests = batchRequests.ToArray() });
                }

                var entry = LocalEntry.Create(
                    value,
                    newHash,
                    status.LastChangeUtc,
                    now.AddSeconds(BusinessMemoryCacheSettings.AcceptSeconds),
                    Math.Max(seconds, 1));

                MemoryCache.Set(ns, entry, NewPolicy(ns, entry.TtlSeconds));
            }
            catch (Exception ex)
            {
                BusinessMemoryCacheRouting.RecordFailure();
                SysLogConnector.LogErrorString(ex.ToString());
            }
        }

        private static bool IsValidForDistributedCache(T value, int? seconds, BusinessMemoryDistributionSetting distributionSetting, string cacheKey)
        {
            if (value == null)
                return true;

            if (seconds.HasValue && seconds > 60)
                BusinessMemoryCacheShared.ValidForDistributedCache.TryRemove(cacheKey, out _);

            if (BusinessMemoryCacheShared.ValidForDistributedCache.TryGetValue(cacheKey, out var cached) && !cached)
                return false;

            if (distributionSetting == BusinessMemoryDistributionSetting.ConservativeHybridCache && (!seconds.HasValue || seconds <= 60))
            {
                BusinessMemoryCacheShared.ValidForDistributedCache.TryAdd(cacheKey, false);
                return false;
            }

            // Use the runtime type name as the cache key (fall back if FullName is null)
            Type type = value.GetType();
            string key = type.FullName ?? type.ToString();

            // Fast-path cached result
            if (BusinessMemoryCacheShared.ValidForDistributedCache.TryGetValue(key, out var cachedBool))
                return cachedBool;

            try
            {
                bool isEf = false;
                try
                {
                    // Defensive: Entity detection can throw for odd runtime types
                    isEf = EntityUtil.IsEntityFrameworkClass(value);
                }
                catch (Exception ex)
                {
                    // Log and conservatively treat as EF (i.e. NOT valid for distributed cache)
                    SysLogConnector.LogErrorString($"[Cache] Entity detection failed for type '{key}': {ex}");
                    BusinessMemoryCacheShared.ValidForDistributedCache.TryAdd(key, false);
                    return false;
                }

                bool validForDist = !isEf;
                BusinessMemoryCacheShared.ValidForDistributedCache.TryAdd(key, validForDist);
                return validForDist;
            }
            catch (Exception ex)
            {
                // Last-resort: log and avoid distributed cache
                SysLogConnector.LogErrorString($"[Cache] IsValidForDistributedCache unexpected error for type '{key}': {ex}");
                return false;
            }
        }

        private static bool UpsertBatchSafe(DistributedCacheUpsertBatchRequest batchRequest)
        {
            try
            {
                var res = EvoDistributionCacheConnector.UpsertCacheValuesBatch(batchRequest);

                if (res == null)
                {
                    var keys = string.Join(", ", batchRequest.Requests.Select(r => r.Key));
                    SysLogConnector.LogErrorString($"[CacheUpsertBatch] NULL ActionResult for keys='{keys}'.");
                    BusinessMemoryCacheRouting.RecordFailure();
                    return false;
                }

                if (!res.Success)
                {
                    var keys = string.Join(", ", batchRequest.Requests.Select(r => r.Key));
                    var message = res.ErrorMessage ?? res.Exception?.ToString() ?? res.InfoMessage ?? "No error message.";
                    SysLogConnector.LogErrorString($"[CacheUpsertBatch] Failed for keys='{keys}'. Message='{message}'.");
                    BusinessMemoryCacheRouting.RecordFailure();
                    return false;
                }

                BusinessMemoryCacheRouting.RecordSuccess();
                return true;
            }
            catch (Exception ex)
            {
                var keys = string.Join(", ", batchRequest.Requests.Select(r => r.Key));
                SysLogConnector.LogErrorString($"[CacheUpsertBatch] Exception for keys='{keys}': {ex}");
                BusinessMemoryCacheRouting.RecordFailure();
                return false;
            }
        }

        #endregion

        #region Delete

        public static bool Delete(Guid key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled) => key != Guid.Empty && Delete(key.ToString(), distributionSetting);

        public static bool Delete(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled)
        {
            try
            {
                if (!IsValidKey(key)) return false;

                if (distributionSetting == BusinessMemoryDistributionSetting.Disabled)
                {
                    return BusinessMemoryCacheOld<T>.Delete(key);
                }

                var ns = NamespaceKey(key);
                var now = UtcNow;

                MemoryCache.Remove(ns);
                CleanupKeyLock(ns);

                var status = new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = now };

                var batchRequest = new DistributedCacheUpsertBatchRequest
                {
                    Requests = new[]
                    {
                        new DistributedCacheUpsertRequest
                        {
                            Key = HashKey(ns),
                            Value = Newtonsoft.Json.JsonConvert.SerializeObject(string.Empty),
                            Expiration = TimeSpan.FromSeconds(1),
                            UseSlidingExpiration = false
                        },
                        new DistributedCacheUpsertRequest
                        {
                            Key = ValueKey(ns),
                            Value = Newtonsoft.Json.JsonConvert.SerializeObject(default(T)),
                            Expiration = TimeSpan.FromSeconds(1),
                            UseSlidingExpiration = false
                        },
                        new DistributedCacheUpsertRequest
                        {
                            Key = StatusKey(ns),
                            Value = Newtonsoft.Json.JsonConvert.SerializeObject(status),
                            Expiration = BusinessMemoryCacheSettings.DistTtlForStatus(60),
                            UseSlidingExpiration = false
                        }
                    }
                };

                UpsertBatchSafe(batchRequest);

                return true;
            }
            catch (Exception ex)
            {
                SysLogConnector.LogErrorString(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Contains
        public static bool Contains(string key, BusinessMemoryDistributionSetting distributionSetting = BusinessMemoryDistributionSetting.Disabled)
        {
            try
            {
                if (!IsValidKey(key)) return false;

                if (distributionSetting == BusinessMemoryDistributionSetting.Disabled)
                {
                    return BusinessMemoryCacheOld<T>.Contains(key);
                }

                return MemoryCache.Contains(NamespaceKey(key));
            }
            catch { return false; }
        }
        #endregion

        #region Helpers

        private static LocalEntry GetLocalEntryFromCache(string ns)
        {
            return MemoryCache.Get(ns) as LocalEntry;
        }

        /// <summary>
        /// See if we are within the local accept window.The Local accpt window is used to avoid frequent distributed checks.
        /// </summary>
        private static bool CheckLocalAcceptWindow(LocalEntry local, DateTimeOffset now, out T value)
        {
            if (local != null && now < local.AcceptUntilUtc)
            {
                value = (T)local.Value;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// See if we should throttle distributed cache checks based on last checked time. Throttling is used to avoid frequent distributed cache checks.
        /// </summary>
        private static bool ShouldThrottle(LocalEntry local, DateTimeOffset now, out T value)
        {
            int checkIntervalSeconds = BusinessMemoryCacheSettings.CheckIntervalSeconds;
            if (local != null && local.TtlSeconds > 0 && checkIntervalSeconds > local.TtlSeconds)
                checkIntervalSeconds = local.TtlSeconds;

            if (local != null && (now - local.LastCheckedUtc) < TimeSpan.FromSeconds(checkIntervalSeconds))
            {
                value = (T)local.Value;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Check distributed cache for value and status, and load into local cache if found.
        /// </summary>
        private static bool TryLoadFromDistributed(string ns, DateTimeOffset now, out T value)
        {
            value = default(T);
            var status = GetStatus(StatusKey(ns));
            if (status == null) return false;
            if (status.OriginalTtlSeconds <= 0) return false;

            // compute remaining local TTL based on distributed status age
            var ageSeconds = (int)Math.Max((now - status.LastChangeUtc).TotalSeconds, 0);
            var remaining = status.OriginalTtlSeconds - ageSeconds;

            if (remaining <= 0)
            {
                Delete(ns, BusinessMemoryDistributionSetting.FullyHybridCache);
                return false;
            }

            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashKey(ns));
            if (string.IsNullOrEmpty(hash))
                return false;

            var distVal = EvoDistributionCacheConnector.GetCachedValue<T>(ValueKey(ns));
            if (IsMissing(distVal))
                return false; 

            var entry = LocalEntry.Create(
                distVal,
                hash,
                status.LastChangeUtc,
                now.AddSeconds(BusinessMemoryCacheSettings.AcceptSeconds),
                remaining);

            MemoryCache.Set(ns, entry, NewPolicy(ns, entry.TtlSeconds));
            value = distVal;
            return true;
        }

        /// <summary>
        /// Indicates whether the specified value is considered "missing" (null for reference types and nullable value types).
        /// </summary>
        private static bool IsMissing<TValue>(TValue value)
        {
            if (!typeof(TValue).IsValueType)
                return value == null;

            if (Nullable.GetUnderlyingType(typeof(TValue)) != null)
                return value == null;

            return false;
        }

        /// <summary>
        /// Check if we need to perform a status check based on last checked time.
        /// </summary>
        private static bool NeedsStatusCheck(LocalEntry local, DateTimeOffset now)
            => (now - local.LastCheckedUtc) >= TimeSpan.FromSeconds(BusinessMemoryCacheSettings.CheckIntervalSeconds);

        /// <summary>
        /// Extend the local accept window to avoid frequent distributed cache checks.
        /// </summary>
        private static void ExtendAccept(LocalEntry local, DateTimeOffset now)
        {
            local.LastCheckedUtc = now;
            local.AcceptUntilUtc = now.AddSeconds(BusinessMemoryCacheSettings.AcceptSeconds);
        }

        /// <summary>
        /// Check if the local entry has expired based on its TTL.
        /// </summary>
        private static bool IsLocalExpired(LocalEntry local, DateTimeOffset now)
            => (now - local.StatusStampUtc) >= TimeSpan.FromSeconds(Math.Max(local.TtlSeconds, 1));

        /// <summary>
        /// If the distributed cache has a newer version, refresh the local cache from distributed.
        /// </summary>
        private static bool RefreshFromDistributedIfNewer(string ns, LocalEntry local, DateTimeOffset now, out T value)
        {
            value = default(T);
            var status = GetStatus(StatusKey(ns));
            local.LastCheckedUtc = now;

            if (status != null && status.LastChangeUtc > local.StatusStampUtc)
            {
                if (status.OriginalTtlSeconds <= 0)
                {
                    MemoryCache.Remove(ns);
                    return false;
                }

                var distHash = EvoDistributionCacheConnector.GetCachedValue<string>(HashKey(ns));
                if (string.IsNullOrEmpty(distHash))
                {
                    MemoryCache.Remove(ns);
                    return false;
                }

                if (StringEqualsSafe(distHash, local.HashHex))
                {
                    local.StatusStampUtc = status.LastChangeUtc;
                    local.AcceptUntilUtc = now.AddSeconds(BusinessMemoryCacheSettings.AcceptSeconds);

                    // keep remaining TTL in sync even when value unchanged
                    var ageSecondsSame = (int)Math.Max((now - status.LastChangeUtc).TotalSeconds, 0);
                    local.UpdateTtl(Math.Max(status.OriginalTtlSeconds - ageSecondsSame, 1));

                    value = (T)local.Value;
                    MemoryCache.Set(ns, local, NewPolicy(ns, local.TtlSeconds));
                    return true;
                }

                var distVal = EvoDistributionCacheConnector.GetCachedValue<T>(ValueKey(ns));
                if (IsMissing(distVal))
                {
                    MemoryCache.Remove(ns);
                    return false;
                }

                // compute remaining TTL
                var ageSeconds = (int)Math.Max((now - status.LastChangeUtc).TotalSeconds, 0);
                var remaining = Math.Max(status.OriginalTtlSeconds - ageSeconds, 1);

                var entry = LocalEntry.Create(
                    distVal,
                    distHash,
                    status.LastChangeUtc,
                    now.AddSeconds(BusinessMemoryCacheSettings.AcceptSeconds),
                    remaining);

                MemoryCache.Set(ns, entry, NewPolicy(ns, entry.TtlSeconds));
                value = distVal;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fetch the status object from distributed cache.
        /// </summary>
        private static EvoDistributionCacheStatus GetStatus(string statusKey)
        {
            try
            {
                return EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(statusKey);
            }
            catch (Exception ex)
            {
                SysLogConnector.LogErrorString(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// The lease is used to coordinate which process is responsible for refreshing a given cache entry.
        /// If a lease is held by another process, we avoid refreshing the entry to reduce thundering herd.
        /// Since the distributed cache does not support atomic operations, we use a get-then-upsert approach with a short TTL. (setting.LeaseSeconds)
        /// </summary>
        private static bool TryAcquireLease(string leaseKey, out string winner)
        {
            winner = null;
            try
            {
                var now = UtcNow;
                var lease = EvoDistributionCacheConnector.GetCachedValue<Lease>(leaseKey);

                if (lease != null && lease.UntilUtc > now)
                {
                    winner = lease.Owner;
                    return false;
                }

                var myLease = new Lease
                {
                    Owner = BusinessMemoryCacheShared.OwnerId,
                    CreatedUtc = now,
                    UntilUtc = now.AddSeconds(BusinessMemoryCacheSettings.LeaseSeconds)
                };

                EvoDistributionCacheConnector.UpsertCachedValue(leaseKey, myLease, TimeSpan.FromSeconds(BusinessMemoryCacheSettings.LeaseSeconds), false);

                lease = EvoDistributionCacheConnector.GetCachedValue<Lease>(leaseKey);
                winner = lease?.Owner;
                return lease != null && lease.Owner == BusinessMemoryCacheShared.OwnerId && lease.UntilUtc > now;
            }
            catch (Exception ex)
            {
                SysLogConnector.LogErrorString(ex.ToString());
                return false;
            }
        }

        private static string ComputeHashHex(T value, string key)
        {
            return CryptographyUtility.ComputeHashHex(value, key);
        }

        private static bool StringEqualsSafe(string a, string b)
            => string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);

        #endregion
    }

    internal sealed class LocalEntry
    {
        public object Value { get; private set; }
        public string HashHex { get; private set; }
        public DateTimeOffset StatusStampUtc { get; set; }
        public DateTimeOffset AcceptUntilUtc { get; set; }
        public DateTimeOffset LastCheckedUtc { get; set; }
        public int TtlSeconds { get; private set; }

        public void UpdateTtl(int ttlSeconds)
        {
            if (ttlSeconds <= 0)
                ttlSeconds = 0;

            TtlSeconds = ttlSeconds;
        }

        private LocalEntry() { }

        public static LocalEntry Create(object value, string hashHex, DateTimeOffset status, DateTimeOffset accept, int ttlSeconds)
            => new LocalEntry
            {
                Value = value,
                HashHex = hashHex,
                StatusStampUtc = status,
                AcceptUntilUtc = accept,
                TtlSeconds = ttlSeconds
            };
    }
    public static class BusinessMemoryCacheOld<T>
    {
        private static bool IsValidKey(string key)
        {
            return !string.IsNullOrEmpty(key) && key != Guid.Empty.ToString();
        }

        public static T Get(Guid? key)
        {
            return Get(key?.ToString());
        }

        public static T Get(string key)
        {
            try
            {
                if (!IsValidKey(key))
                    return default(T);

                MemoryCache memoryCache = MemoryCache.Default;
                object obj = memoryCache.Get(key);
                return obj != null ? (T)obj : default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        public static bool TryGetValue(string key, out T value)
        {
            try
            {
                if (!IsValidKey(key))
                {
                    value = default(T);
                    return false;
                }

                MemoryCache memoryCache = MemoryCache.Default;
                object obj = memoryCache.Get(key);

                if (obj is T)
                {
                    value = (T)obj;
                    return true;
                }
            }
            catch
            {
                value = default(T);
                return false;
            }

            value = default(T);
            return false;
        }

        public static void Set(Guid? key, T value, int seconds = 60)
        {
            Set(key?.ToString(), value, seconds);
        }

        public static void Set(string key, T value, int seconds = 60)
        {
            try
            {
                if (value == null)
                    return;

                if (!IsValidKey(key))
                    return;

                MemoryCache memoryCache = MemoryCache.Default;
                memoryCache.Set(key, value, DateTimeOffset.UtcNow.AddSeconds(seconds));
            }
            catch (Exception ex)
            {
                SysLogConnector.LogErrorString(ex.ToString());
            }
        }



        public static bool Delete(Guid key)
        {
            if (key == Guid.Empty)
                return false;
            return Delete(key.ToString());
        }

        public static bool Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            try
            {
                MemoryCache memoryCache = MemoryCache.Default;
                memoryCache.Remove(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Contains(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            try
            {
                MemoryCache memoryCache = MemoryCache.Default;
                return memoryCache.Contains(key);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}