using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SO.Internal.Shared.Api.Cache.DistrubutedCache;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Business.Util.Svefaktura.Schema.Envelope;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Evo.Connectors.Cache
{
    public class EvoDistributionCacheConnector : EvoConnectorBase
    {
        private static string UpdateKey(string key)
        {
            return $"db{ConfigurationSetupUtil.GetCurrentSysCompDbId()}k#{key}";
        }

        // Track consecutive empty/null responses and last throttle time per key
        private class EmptyResponseInfo
        {
            public int Count;
            public DateTime LastThrottle;
        }

        private static readonly ConcurrentDictionary<string, EmptyResponseInfo> _emptyResponseInfo = new ConcurrentDictionary<string, EmptyResponseInfo>();
        private static readonly TimeSpan _emptyThrottleInterval = TimeSpan.FromSeconds(10);
        public static DateTime lastThrottleClean = DateTime.UtcNow;
        private const int _emptyResponseThreshold = 3;
        private static int numberOfThrottledRequests = 0;
        private static int numberOfSucessfulRequests = 0;

        public static T GetCachedValue<T>(string key)
        {
            if (IsInvalidKey(key, out ActionResult actionResult))
                throw new InvalidOperationException("GetCachedValue is invalid");

            var updatedKey = UpdateKey(key);
            var now = DateTime.UtcNow;

            // Get or create info for this key
            var info = _emptyResponseInfo.GetOrAdd(updatedKey, _ => new EmptyResponseInfo());

            // Throttle only if threshold reached and interval not elapsed
            if (info.Count >= _emptyResponseThreshold && now - info.LastThrottle < _emptyThrottleInterval)
            {
                var throttled = Interlocked.Increment(ref numberOfThrottledRequests);
                if (throttled % 1000 == 0) // Log every 1000th throttled request
                    LogCollector.LogWithTrace($"Throttled repeated cache access for key: {updatedKey} (last {info.Count} responses were empty)", LogLevel.Warning);
                if (info.Count >= 100 && info.Count % 100 == 0) // Log every 100th throttled request
                    LogCollector.LogWithTrace($"Throttled cache accesses for same key {updatedKey} count: {info.Count} @ {DateTime.Now}", LogLevel.Warning);
                Interlocked.Increment(ref info.Count);
                return default;
            }

            try
            {
                var request = new DistributedCacheGetRequest()
                {
                    Key = updatedKey
                };

                var response = Task.Run(() => DistributedCacheClient.GetCachedValueAsync(Url, Token, request)).GetAwaiter().GetResult();

                var respVal = response?.Value;
                var isEmpty = response == null || string.IsNullOrEmpty(respVal);

                if (isEmpty)
                {
                    var newCount = Interlocked.Increment(ref info.Count);
                    if (newCount >= _emptyResponseThreshold)
                        info.LastThrottle = now;
                    return default;
                }
                else
                {
                    var successful = Interlocked.Increment(ref numberOfSucessfulRequests);
                    if (successful <= 5000)
                    {
                        if (successful % 1000 == 0) // Log every 1000th successful request for the first 500
                            LogCollector.LogInfo($"Successful cache accesses: {successful}");
                    }

                    if (successful % 50000 == 0) // Log every 50000th successful request
                        LogCollector.LogInfo($"Successful cache accesses: {successful}");
                }

                // Successful non-empty response: reset counter and throttle time
                Interlocked.Exchange(ref info.Count, 0);
                info.LastThrottle = DateTime.MinValue;

                if (!response.Success)
                    return default;

                return JsonConvert.DeserializeObject<T>(respVal);
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return default;
            }
            finally
            {
                try
                {
                    // Clean up dictionary to prevent unbounded growth
                    if (info.Count == 0)
                        _emptyResponseInfo.TryRemove(updatedKey, out _);

                    if (lastThrottleClean.AddMinutes(5) < now)
                    {
                        var oldestAllowed = now.AddMinutes(-1);
                        var keysToRemove = _emptyResponseInfo.Where(kvp => kvp.Value.LastThrottle < oldestAllowed).Select(kvp => kvp.Key).ToList();
                        foreach (var keyToRemove in keysToRemove)
                            _emptyResponseInfo.TryRemove(keyToRemove, out _);

                        lastThrottleClean = now;
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogError(ex);
                }
            }
        }

        public static ActionResult UpsertCachedValue<T>(string key, T value, TimeSpan? expirationTime = null, bool useSlidingExpiration = false)
        {
            var container = new DistributedCacheUpsertRequest()
            {
                Key = key,
                Value = JsonConvert.SerializeObject(value),
                Expiration = expirationTime ?? TimeSpan.FromMinutes(5),
                UseSlidingExpiration = useSlidingExpiration
            };

            return UpsertCachedValue<T>(container);
        }

        public static ActionResult UpsertCachedValue<T>(DistributedCacheUpsertRequest request)
        {
            if (IsInvalidKey(request.Key, out ActionResult actionResult))
                return actionResult;

            request.Key = UpdateKey(request.Key);
            try
            {
                var response = Task.Run(() => DistributedCacheClient.UpsertCachedValueAsync(Url, Token, request)).GetAwaiter().GetResult();
                if (response == null)
                    return new ActionResult(new Exception("Failed to upsert value in cache of type " + typeof(T).Name));
                if (!response.Success)
                    return new ActionResult(new Exception(response.Message));

                return new ActionResult();
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return default;
            }
        }

        public static ActionResult UpsertCacheValuesBatch(DistributedCacheUpsertBatchRequest batchRequest)
        {
            foreach (var request in batchRequest.Requests)
            {
                if (IsInvalidKey(request.Key, out ActionResult actionResult))
                    return actionResult;
                request.Key = UpdateKey(request.Key);
            }
            try
            {
                var response = Task.Run(() => DistributedCacheClient.UpsertCachedBatchValuesAsync(Url, Token, batchRequest)).GetAwaiter().GetResult();
                if (response == null)
                    return new ActionResult(new Exception("Failed to upsert batch values in cache"));
                if (!response.Success)
                    return new ActionResult(new Exception(response.ErrorMessage));
                return new ActionResult();
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return default;
            }
        }

        public static ActionResult DeleteValue(string key)
        {
            if (IsInvalidKey(key, out ActionResult actionResult))
                return actionResult;
            try
            {
                var request = new DistributedCacheUpsertRequest()
                {
                    Key = UpdateKey(key),
                    Expiration = TimeSpan.Zero
                };

                var response = Task.Run(() => DistributedCacheClient.UpsertCachedValueAsync(Url, Token, request)).GetAwaiter().GetResult();
                if (response == null)
                    return new ActionResult(new Exception("Failed to delete value from cache"));
                if (!response.Success)
                    return new ActionResult(new Exception(response.Message));
                return new ActionResult();
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return default;
            }
        }


        public static bool IsInvalidKey(string key, out ActionResult actionResult)
        {
            actionResult = null;
            if (string.IsNullOrEmpty(key))
                actionResult = new ActionResult(new Exception("Key cannot be null or empty"));

            if (values.Any() && values.Any(k => key.Contains(k)))
                actionResult = new ActionResult(new Exception("Key is set to crash"));

            if (actionResult != null)
                return true;

            return false;
        }

        private static ConcurrentBag<string> values = new ConcurrentBag<string>();

        public static void SetCrashOnKey(string key)
        {
            values.Add(key);
        }
    }

}

