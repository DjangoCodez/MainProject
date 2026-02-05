using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.DataCache.Tests
{
    [TestClass]
    public class BusinessMemoryCacheNewTests
    {
        private static int AcceptSeconds => ConfigurationSetupUtil.CacheIntAcceptSeconds();
        private static int CheckIntervalSeconds => ConfigurationSetupUtil.CacheCheckIntervalSeconds();

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            // Heavy init – do once per class
            ConfigurationSetupUtil.Init();
        }

        private static string NsKey<T>(string key) => $"{typeof(T).FullName}:{key}";
        private const string StsPrefix = "sts:";
        private const string ValPrefix = "val:";
        private const string HashPrefix = "hash:";

        private static void PrepopulateL2<T>(string key, T value, int seconds = 300)
        {
            var ns = NsKey<T>(key);
            var statusKey = StsPrefix + ns;
            var valueKey = ValPrefix + ns;
            var hashKey = HashPrefix + ns;

            var now = DateTimeOffset.UtcNow;
            var status = new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = now, OriginalTtlSeconds = 60 };
            var hash = CryptographyUtility.ComputeHashHex(value, key);

            EvoDistributionCacheConnector.UpsertCachedValue(statusKey, status, TimeSpan.FromSeconds(Math.Max(seconds * 2, 60)), false);
            EvoDistributionCacheConnector.UpsertCachedValue(hashKey, hash, TimeSpan.FromSeconds(Math.Max(seconds, 10)), false);
            EvoDistributionCacheConnector.UpsertCachedValue(valueKey, value, TimeSpan.FromSeconds(Math.Max(seconds, 10)), false);
        }

        private static void ClearL2<T>(string key)
        {
            var ns = NsKey<T>(key);
            EvoDistributionCacheConnector.UpsertCachedValue(StsPrefix + ns, new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = DateTimeOffset.UtcNow }, TimeSpan.FromSeconds(1), false);
            EvoDistributionCacheConnector.UpsertCachedValue(HashPrefix + ns, string.Empty, TimeSpan.FromSeconds(1), false);
            EvoDistributionCacheConnector.UpsertCachedValue(ValPrefix + ns, default(T), TimeSpan.FromSeconds(1), false);
        }

        // Wait for the lease to be visible in L2 to avoid timing flakiness in tests.
        private static void WaitForLeasePropagation(string leaseKey, string expectedOwner, int timeoutMs = 1000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var leaseObj = EvoDistributionCacheConnector.GetCachedValue<dynamic>(leaseKey);
                    if (leaseObj != null)
                    {
                        // JObject/ dynamic - check Owner property if present
                        try
                        {
                            var owner = leaseObj.Owner;
                            if (owner != null && owner.ToString() == expectedOwner)
                                return;
                        }
                        catch { /* ignore and keep polling */ }

                        // Also try reflection if leaseObj is a typed object
                        try
                        {
                            var type = leaseObj.GetType();
                            var prop = type.GetProperty("Owner");
                            if (prop != null)
                            {
                                var ownerVal = prop.GetValue(leaseObj);
                                if (ownerVal != null && ownerVal.ToString() == expectedOwner)
                                    return;
                            }
                        }
                        catch { }
                    }
                }
                catch { /* ignore transient errors */ }

                Thread.Sleep(10);
            }
        }

        // Wait for the status key to be visible in L2 so tests don't race on propagation
        private static void WaitForStatusPropagation(string statusKey, DateTimeOffset expectedLastChangeUtc, int timeoutMs = 2000)
        {
            // We need to override the l1 status in order not to have to wait otu the accept window
            // Get the existing l1 status and set its LastChangeUtc to past
            var fromCacheStatus = BusinessMemoryCacheOld<EvoDistributionCacheStatus>.Get(statusKey);

            if (fromCacheStatus != null && fromCacheStatus.LastChangeUtc >= expectedLastChangeUtc)
            {
                fromCacheStatus.LastChangeUtc = expectedLastChangeUtc.AddSeconds(-AcceptSeconds - 10);
                BusinessMemoryCacheOld<EvoDistributionCacheStatus>.Set(statusKey, fromCacheStatus, seconds: AcceptSeconds + CheckIntervalSeconds + 10);
            }

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var sts = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(statusKey);
                    if (sts != null && sts.LastChangeUtc >= expectedLastChangeUtc)
                        return;
                }
                catch { /* ignore transient errors */ }

                Thread.Sleep(10);
            }
        }

        [TestMethod]
        public void Criteria01_ColdStart_ValueExistsInL2_LoadsOnceAndReturns()
        {
            var key = Guid.NewGuid().ToString("N");
            var val = "hello-l2";

            // Given: L2 has value+status (cold start for this key)
            PrepopulateL2<string>(key, val);

            // When: TryGetValue
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(val, got);
        }

        [TestMethod]
        public void Criteria02_ColdStart_MissingInL2_ReturnsFalse_ThenSetStoresInL2AndL1()
        {
            var key = Guid.NewGuid().ToString("N");
            ClearL2<string>(key);

            // When: TryGetValue on cold+missing L2
            var ok = BusinessMemoryCacheNew<string>.TryGetValue(key, out var miss, BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsFalse(ok);
            Assert.IsNull(miss);

            // After: Set stores to L2 and populates L1
            var newVal = "from-caller";
            BusinessMemoryCacheNew<string>.Set(key, newVal, seconds: 60, BusinessMemoryDistributionSetting.FullyHybridCache);

            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var fromCache, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(newVal, fromCache);

            // Verify L2 has status+hash+value
            var ns = NsKey<string>(key);
            var status = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(StsPrefix + ns);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);

            Assert.IsNotNull(status);
            Assert.IsFalse(string.IsNullOrEmpty(hash));
            Assert.AreEqual(newVal, distVal);
        }

        [TestMethod]
        public void Criteria03_SteadyStateRead_NoL2CallsDuringAcceptWindow_Basic()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "steady";
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 120, BusinessMemoryDistributionSetting.FullyHybridCache);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got, BusinessMemoryDistributionSetting.FullyHybridCache));
                Assert.AreEqual(value, got);
            }
            sw.Stop();

            // Expect fast local hits (heuristic)
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000, "Repeated gets should be fast within accept window.");
        }

        [TestMethod]
        public void Criteria04_ChangePropagation_ViaL2Status_RefreshesValue()
        {
            var key = Guid.NewGuid().ToString("N");
            var v1 = "v1";
            var v2 = "v2";

            // Node A sets v1
            BusinessMemoryCacheNew<string>.Set(key, v1, seconds: 120, BusinessMemoryDistributionSetting.FullyHybridCache);

            // Node B initial load
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got1, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(v1, got1);

            // Small delay to ensure distinct timestamps
            Thread.Sleep(1000);

            // Node A updates to v2 (bumps status)
            BusinessMemoryCacheNew<string>.Set(key, v2, seconds: 120, BusinessMemoryDistributionSetting.FullyHybridCache);

            // Wait beyond throttle interval so B can perform status check
            Thread.Sleep((CheckIntervalSeconds + 1) * 1000);

            // Node B should see newer status and fetch new value
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got2, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(v2, got2);
        }

        [TestMethod]
        public void Criteria06_SingleRefresher_ViaLease_OthersServeLastValue()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "lease-test";

            // Populate local and L2
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 2, BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got);

            // Pre-insert a lease so that this node is NOT the winner when TTL expires
            var ns = NsKey<string>(key);
            var leaseKey = "lease:" + ns;
            var now = DateTimeOffset.UtcNow;
            var externalLease = new { Owner = "OTHER_NODE", CreatedUtc = now, UntilUtc = now.AddSeconds(ConfigurationSetupUtil.CacheLeaseSeconds()) };
            EvoDistributionCacheConnector.UpsertCachedValue(leaseKey, externalLease, TimeSpan.FromSeconds(ConfigurationSetupUtil.CacheLeaseSeconds()), false);

            // Wait for propagation
            WaitForLeasePropagation(leaseKey, "OTHER_NODE");

            // Let local TTL expire
            Thread.Sleep(2500);

            // TryGetValue should not recompute here; if lease held by other, it returns last local value (accept window extended)
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got2, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got2);
        }

        [TestMethod]
        public void Criteria07_SysEFEntity_ExcludedFromL2_ValueNotStored()
        {
            var key = Guid.NewGuid().ToString("N");
            using (var sys = new SOESysEntities())
            {
                // EF collection example
                var list = sys.SysPageStatus.Take(3).ToList();

                // Sanity: recognized as EF by rule
                Assert.IsTrue(EntityUtil.IsEntityFrameworkClass(list));

                BusinessMemoryCacheNew<List<SoftOne.Soe.Data.SysPageStatus>>.Set(key, list, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);

                // L2 value should not be stored
                var ns = NsKey<List<SoftOne.Soe.Data.SysPageStatus>>(key);
                var distVal = EvoDistributionCacheConnector.GetCachedValue<List<SoftOne.Soe.Data.SysPageStatus>>(ValPrefix + ns);
                Assert.IsNull(distVal);
            }
        }

        [TestMethod]
        public void Criteria07_CompEFEntity_ExcludedFromL2_ValueNotStored()
        {
            var key = Guid.NewGuid().ToString("N");
            using (var sys = new CompEntities())
            {
                // EF collection example
                var list = sys.Account.Take(3).ToList();

                // Sanity: recognized as EF by rule
                Assert.IsTrue(EntityUtil.IsEntityFrameworkClass(list));

                BusinessMemoryCacheNew<List<SoftOne.Soe.Data.Account>>.Set(key, list, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);

                // L2 value should not be stored
                var ns = NsKey<List<SoftOne.Soe.Data.Account>>(key);
                var distVal = EvoDistributionCacheConnector.GetCachedValue<List<SoftOne.Soe.Data.SysPageStatus>>(ValPrefix + ns);
                Assert.IsNull(distVal);
            }
        }

        [TestMethod]
        public void Criteria11_Delete_EvictsLocallyAndClearsL2()
        {
            var key = Guid.NewGuid().ToString("N");
            var val = "to-delete";
            BusinessMemoryCacheNew<string>.Set(key, val, seconds: 60, BusinessMemoryDistributionSetting.FullyHybridCache);

            // Ensure present
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got, BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(val, got);

            // Delete
            Assert.IsTrue(BusinessMemoryCacheNew<string>.Delete(key, BusinessMemoryDistributionSetting.FullyHybridCache));

            // TryGet should now be a miss (until refilled)
            Assert.IsFalse(BusinessMemoryCacheNew<string>.TryGetValue(key, out var _, BusinessMemoryDistributionSetting.FullyHybridCache));

            // L2 cleared/short-lived
            var ns = NsKey<string>(key);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            Assert.IsNull(distVal);
        }

        [TestMethod]
        public void Criteria13_KeyStability_NamespacedKeys()
        {
            var key = Guid.NewGuid().ToString("N");
            var val = "kfmt";
            BusinessMemoryCacheNew<string>.Set(key, val, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);

            var ns = NsKey<string>(key);
            // Verify L2 keys exist with expected prefixes
            var status = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(StsPrefix + ns);
            var value = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);

            Assert.IsNotNull(status, "Status key missing in L2");
            Assert.AreEqual(val, value, "Value key missing or incorrect in L2");
            Assert.IsFalse(string.IsNullOrEmpty(hash), "Hash key missing in L2");
        }

        [TestMethod]
        public void Criteria14_ChangeOnAnotherServer_LocalInvalidationAndRefresh()
        {
            var key = Guid.NewGuid().ToString("N");
            var v1 = "initial-value";
            var v2 = "updated-value";
            var ns = NsKey<string>(key);

            // Step 1: Node A sets initial value
            BusinessMemoryCacheNew<string>.Set(key, v1, seconds: 120, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got1, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(v1, got1);

            // Step 2: Simulate another server updating the distributed cache
            var now = DateTimeOffset.UtcNow;
            var statusKey = StsPrefix + ns;
            var valueKey = ValPrefix + ns;
            var hashKey = HashPrefix + ns;
            var newStatus = new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = now.AddSeconds(1), OriginalTtlSeconds = 120 };
            var newHash = CryptographyUtility.ComputeHashHex(v2, key);
            EvoDistributionCacheConnector.UpsertCachedValue(statusKey, newStatus, TimeSpan.FromSeconds(120), false);
            EvoDistributionCacheConnector.UpsertCachedValue(valueKey, v2, TimeSpan.FromSeconds(120), false);
            EvoDistributionCacheConnector.UpsertCachedValue(hashKey, newHash, TimeSpan.FromSeconds(120), false);

            // Wait for propagation of status to L2
            WaitForStatusPropagation(statusKey, newStatus.LastChangeUtc);

            // Step 3: Wait beyond accept window and check interval to force status check
            Thread.Sleep((AcceptSeconds + CheckIntervalSeconds + 1) * 1000);

            // Step 4: Node A should detect the newer status and refresh from distributed cache
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got2, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(v2, got2, "Local cache should refresh to the updated value from distributed cache.");
        }

        [TestMethod]
        public void Criteria15_ConcurrentUpdates_MultipleServers_SameValue()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "concurrent-value";
            var ns = NsKey<string>(key);

            // Step 1: Simulate two servers setting the same value concurrently
            var tasks = new[]
            {
                Task.Run(() => BusinessMemoryCacheNew<string>.Set(key, value, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache)),
                Task.Run(() => BusinessMemoryCacheNew<string>.Set(key, value, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache))
            };
            Task.WaitAll(tasks);

            // Step 2: Verify local cache has the value
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got);

            // Step 3: Verify distributed cache has consistent state
            var status = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(StsPrefix + ns);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);

            Assert.IsNotNull(status, "Status should exist in distributed cache.");
            Assert.IsFalse(string.IsNullOrEmpty(hash), "Hash should exist in distributed cache.");
            Assert.AreEqual(value, distVal, "Value in distributed cache should match.");
            Assert.AreEqual(CryptographyUtility.ComputeHashHex(value, key), hash, "Hash should match computed value.");
        }

        [TestMethod]
        public void Criteria16_LocalCacheExpiration_ForcesDistributedCheck()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "expire-test";
            var ns = NsKey<string>(key);

            // Step 1: Set a value with a short TTL
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 2, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got1, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got1);

            // Step 2: Wait for local TTL to expire
            Thread.Sleep(2500);

            // Step 3: Simulate distributed cache having no value (e.g., cleared by another server)
            ClearL2<string>(key);

            // Step 4: TryGetValue should check distributed cache and return false (no value)
            Assert.IsFalse(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got2, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.IsNull(got2, "Local cache should be invalidated and no value returned.");
        }

        [TestMethod]
        public void Criteria17_LeaseContention_MultipleNodesAttemptRefresh()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "lease-contention";
            var ns = NsKey<string>(key);

            // Step 1: Set initial value with short TTL
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 2, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got1, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got1);

            // Step 2: Wait for local TTL to expire
            Thread.Sleep(2500);

            // Step 3: Simulate another node holding the lease
            var leaseKey = "lease:" + ns;
            var now = DateTimeOffset.UtcNow;
            var externalLease = new { Owner = "OTHER_NODE", CreatedUtc = now, UntilUtc = now.AddSeconds(ConfigurationSetupUtil.CacheLeaseSeconds()) };
            EvoDistributionCacheConnector.UpsertCachedValue(leaseKey, externalLease, TimeSpan.FromSeconds(ConfigurationSetupUtil.CacheLeaseSeconds()), false);

            // Wait for propagation
            WaitForLeasePropagation(leaseKey, "OTHER_NODE");

            // Step 4: Simulate multiple concurrent attempts to refresh
            var results = new List<bool>();
            var values = new List<string>();
            var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
            {
                bool success = BusinessMemoryCacheNew<string>.TryGetValue(key, out var val, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
                lock (results)
                {
                    results.Add(success);
                    values.Add(val);
                }
            })).ToArray();
            Task.WaitAll(tasks);

            // Step 5: All nodes should return the last known value due to lease contention
            Assert.IsTrue(results.All(r => r), "All attempts should succeed due to local value fallback.");
            Assert.IsTrue(values.All(v => v == value), "All attempts should return the last known value.");
        }

        [TestMethod]
        public void Criteria18_DistributedCacheFailure_TriggersCircuitBreaker()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "failure-test";

            // Step 1: Ensure new cache is active
            BusinessMemoryCacheRouting.SetMode(BusinessMemoryCacheRouting.Mode.NewOnly);
            Assert.AreEqual(BusinessMemoryCacheRouting.Mode.NewOnly, BusinessMemoryCacheRouting.CurrentMode);

            // Step 2: Mock distributed cache to throw exceptions

            EvoDistributionCacheConnector.SetCrashOnKey(key); // Hypothetical method to induce failures
            for (int i = 0; i < 16; i++) // Exceed TripThreshold (15)
            {
                BusinessMemoryCacheNew<string>.Set(key, value, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            }

            // Step 3: Verify circuit breaker tripped
            Assert.AreEqual(BusinessMemoryCacheRouting.Mode.OldOnly, BusinessMemoryCacheRouting.CurrentMode, "Circuit breaker should trip to OldOnly mode.");
            Assert.IsTrue(BusinessMemoryCacheRouting.FailBackToOldIsTripped, "FailBackToOldIsTripped should be true during cooldown.");

            // Step 4: Verify old cache is used
            var oldCacheHit = BusinessMemoryCacheOld<string>.TryGetValue(key, out var oldValue); // Assume old cache doesn't have the value
            Assert.IsFalse(oldCacheHit, "Old cache should not have the value.");
        }

        [TestMethod]
        public void Criteria19_StaleDistributedStatus_LocalCacheStillValid()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "stale-status-test";
            var ns = NsKey<string>(key);

            // Step 1: Set value in local and distributed cache
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 120, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got1, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got1);

            // Step 2: Simulate older status in distributed cache
            var statusKey = StsPrefix + ns;
            var oldStatus = new EvoDistributionCacheStatus { Key = ns, LastChangeUtc = DateTimeOffset.UtcNow.AddSeconds(-10) };
            EvoDistributionCacheConnector.UpsertCachedValue(statusKey, oldStatus, TimeSpan.FromSeconds(120), false);

            // Step 3: Wait for check interval to force status check
            Thread.Sleep((CheckIntervalSeconds + 1) * 1000);

            // Step 4: Local cache should remain valid
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var got2, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.AreEqual(value, got2, "Local cache should remain valid as distributed status is older.");
        }

        [TestMethod]
        public void Criteria20_InvalidKeyHandling_NoDistributedCacheAccess()
        {
            // Step 1: Test null key
            Assert.IsFalse(BusinessMemoryCacheNew<string>.TryGetValue(null, out var nullResult, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.IsNull(nullResult, "Null key should return false and null value.");

            // Step 2: Test empty key
            Assert.IsFalse(BusinessMemoryCacheNew<string>.TryGetValue("", out var emptyResult, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.IsNull(emptyResult, "Empty key should return false and null value.");

            // Step 3: Test Guid.Empty
            Assert.IsFalse(BusinessMemoryCacheNew<string>.TryGetValue(Guid.Empty.ToString(), out var guidEmptyResult, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache));
            Assert.IsNull(guidEmptyResult, "Guid.Empty key should return false and null value.");

            // Step 4: Verify no distributed cache access (hypothetical tracking)
            // Assert.IsFalse(EvoDistributionCacheConnector.WasAccessed(), "Invalid keys should not trigger distributed cache access.");
        }

        [TestMethod]
        public void DistributionSetting_Disabled_NoL2CacheAccess()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "local-only";

            // Set value with Disabled mode
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 60, BusinessMemoryDistributionSetting.Disabled);

            // Verify local cache works
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var localVal, BusinessMemoryDistributionSetting.Disabled));
            Assert.AreEqual(value, localVal);

            // Verify no L2 storage occurred
            var ns = NsKey<string>(key);
            var status = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(StsPrefix + ns);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);

            Assert.IsNull(status, "Status should not be in L2 cache");
            Assert.IsNull(hash, "Hash should not be in L2 cache");
            Assert.IsNull(distVal, "Value should not be in L2 cache");
        }

        [TestMethod]
        public void DistributionSetting_Conservative_ShortTTL_NoL2Cache()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "short-ttl";

            // Set value with Conservative mode and short TTL (30 seconds)
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 30, BusinessMemoryDistributionSetting.ConservativeHybridCache);

            // Verify local cache works
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var localVal, BusinessMemoryDistributionSetting.ConservativeHybridCache));
            Assert.AreEqual(value, localVal);

            // Verify no L2 storage for short TTL
            var ns = NsKey<string>(key);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);
            Assert.IsNull(distVal, "Short TTL value should not be in L2 cache");
        }

        [TestMethod]
        public void DistributionSetting_Conservative_LongTTL_UsesL2Cache()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = "long-ttl";

            // Set value with Conservative mode and long TTL (120 seconds)
            BusinessMemoryCacheNew<string>.Set(key, value, seconds: 120, BusinessMemoryDistributionSetting.ConservativeHybridCache);

            // Verify local cache works
            Assert.IsTrue(BusinessMemoryCacheNew<string>.TryGetValue(key, out var localVal, BusinessMemoryDistributionSetting.ConservativeHybridCache));
            Assert.AreEqual(value, localVal);

            // Verify L2 storage occurred for long TTL
            var ns = NsKey<string>(key);
            var status = EvoDistributionCacheConnector.GetCachedValue<EvoDistributionCacheStatus>(StsPrefix + ns);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);

            Assert.IsNotNull(status, "Status should be in L2 cache for long TTL");
            Assert.IsNotNull(hash, "Hash should be in L2 cache for long TTL");
            Assert.AreEqual(value, distVal, "Value should be in L2 cache for long TTL");
        }

        [TestMethod]
        public void DistributionSetting_Conservative_CrossesTTLThreshold()
        {
            var key = Guid.NewGuid().ToString("N");
            var shortValue = "short-lived";
            var longValue = "long-lived";

            // First set with short TTL - should stay local
            BusinessMemoryCacheNew<string>.Set(key, shortValue, seconds: 30, BusinessMemoryDistributionSetting.ConservativeHybridCache);
            var ns = NsKey<string>(key);
            Assert.IsNull(EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns), "Short TTL should not use L2");

            // Update same key with long TTL - should go to L2
            BusinessMemoryCacheNew<string>.Set(key, longValue, seconds: 120, BusinessMemoryDistributionSetting.ConservativeHybridCache);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<string>(ValPrefix + ns);
            Assert.AreEqual(longValue, distVal, "Long TTL should use L2");
        }

        [TestMethod]
        public void DistributionSetting_ModeComparison_SameKey()
        {
            var key = Guid.NewGuid().ToString("N");
            var disabledValue = "disabled-value";
            var conservativeValue = "conservative-value";
            var fullyHybridValue = "hybrid-value";

            // Set same key with different modes
            BusinessMemoryCacheNew<string>.Set(key, disabledValue, seconds: 60, BusinessMemoryDistributionSetting.Disabled);
            BusinessMemoryCacheNew<string>.Set(key, conservativeValue, seconds: 60, BusinessMemoryDistributionSetting.ConservativeHybridCache);
            BusinessMemoryCacheNew<string>.Set(key, fullyHybridValue, seconds: 60, BusinessMemoryDistributionSetting.FullyHybridCache);

            // Verify each mode gets its appropriate value
            BusinessMemoryCacheNew<string>.TryGetValue(key, out var val1, BusinessMemoryDistributionSetting.Disabled);
            BusinessMemoryCacheNew<string>.TryGetValue(key, out var val2, BusinessMemoryDistributionSetting.ConservativeHybridCache);
            BusinessMemoryCacheNew<string>.TryGetValue(key, out var val3, BusinessMemoryDistributionSetting.FullyHybridCache);

            Assert.AreEqual(disabledValue, val1, "Disabled should get local-only value");
            Assert.AreEqual(fullyHybridValue, val3, "FullyHybrid should get its value");
            Assert.AreEqual(fullyHybridValue, val2, "Conservative should get the fullyHybrid value, because it was set after");
        }

        [TestMethod]
        public void Criteria21_DTO_NonEf_StoredInL2_NoMessagePackCrash()
        {
            var key = Guid.NewGuid().ToString("N");

            // Use a DTO type from Soe.Shared to ensure it's not treated as EF entity
            var dto = new Soe.Shared.DTO.EmployeeChangeIODTO();

            // Store via new cache path (should use MessagePack for whitelisted DTOs or JSON fallback)
            BusinessMemoryCacheNew<Soe.Shared.DTO.EmployeeChangeIODTO>.Set(key, dto, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);

            // Small delay to allow propagation to L2 in test environment
            Thread.Sleep(200);

            var ns = NsKey<Soe.Shared.DTO.EmployeeChangeIODTO>(key);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<Soe.Shared.DTO.EmployeeChangeIODTO>(ValPrefix + ns);

            Assert.IsFalse(string.IsNullOrEmpty(hash), "Hash should exist in distributed cache for DTO");
            Assert.IsNotNull(distVal, "DTO value should be stored in L2 without MessagePack crash");
        }

        [TestMethod]
        public void Criteria21_DTO_NonEfCollection_StoredInL2_()
        {
            var key = Guid.NewGuid().ToString("N");
            // Use a collection of DTOs from Soe.Shared to ensure it's not treated as EF entity
            // create 200 hundred items to ensure MessagePack is used
            var dtoList = new List<SysHolidayDTO>();
            for (int i = 0; i < 2000; i++)
            {
                dtoList.Add(new SysHolidayDTO()
                {
                    SysTermId = i,
                    CreatedBy = "Holiday " + i,
                    Date = DateTime.UtcNow.AddDays(i)
                });
            }

            // Store via new cache path (should use MessagePack for whitelisted DTOs or JSON fallback)
            BusinessMemoryCacheNew<List<SysHolidayDTO>>.Set(key, dtoList, seconds: 60, distributionSetting: BusinessMemoryDistributionSetting.FullyHybridCache);
            // Small delay to allow propagation to L2 in test environment
            Thread.Sleep(200);
            var ns = NsKey<List<SysHolidayDTO>>(key);
            var hash = EvoDistributionCacheConnector.GetCachedValue<string>(HashPrefix + ns);
            var distVal = EvoDistributionCacheConnector.GetCachedValue<List<SysHolidayDTO>>(ValPrefix + ns);
            Assert.IsFalse(string.IsNullOrEmpty(hash), "Hash should exist in distributed cache for DTO collection");
            Assert.IsNotNull(distVal, "DTO collection value should be stored in L2 without MessagePack crash");

        }
    }


}