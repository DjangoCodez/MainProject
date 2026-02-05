using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Data.Edm.Library.Expressions;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Util;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace SoftOne.Soe.Business.Util
{
    public static class CryptographyUtility
    {
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                return GetMd5HashAsString(md5Hash, input);
            }
        }

        public static byte[] GetMd5HashAsBytes(MD5 md5Hash, string input)
        {
            return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string GetMd5HashAsString(MD5 md5Hash, string input)
        {
            byte[] data = GetMd5HashAsBytes(md5Hash, input);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            string hashOfInput = GetMd5HashAsString(md5Hash, input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return (0 == comparer.Compare(hashOfInput, hash));
        }

        // Threshold for logging slow hash operations
        private const int SlowHashThresholdMs = 100;

        // The following Prime constants are used in ComputeXXH32
        private const uint Prime1 = 2654435761U; // 0x9E3779B1
        private const uint Prime2 = 2246822519U; // 0x85EBCA77
        private const uint Prime3 = 3266489917U; // 0xC2B2AE3D
        private const uint Prime4 = 668265263U;  // 0x27D4EB2F
        private const uint Prime5 = 374761393U;  // 0x165667B1

        // MessagePack Options (Assumed to be defined here for compilation)
        private static readonly MessagePackSerializerOptions ContractlessOptions =
            MessagePack.MessagePackSerializerOptions.Standard
                .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        // Delimiters for custom parsing (must match EfRecursiveSerializer helpers)
        private const char RecordSeparator = '\n';
        private const char FieldSeparator = '|';
        private const char EntityStartMarker = '{';
        private const char EntityEndMarker = '}';

        // --- Main Entry Point Method ---

        /// <summary>
        /// A simple fast hash computation for cache value change detection.
        /// Uses MessagePack/Custom Serializer and XXHash32.
        /// </summary>
        public static string ComputeHashHex<T>(T value, string key)
        {
            // Start measuring the total execution time
            var stopwatch = Stopwatch.StartNew();

            // Add StringBuilder to log information if needed
            StringBuilder logInfo = new StringBuilder();

            // 1. Handle Null/Default Case
            if (value == null)
            {
                stopwatch.Stop();
                return "00000000"; // Consistent hash for null
            }

            byte[] payload = null;

            // 2. Trivial Types (Fast Path)
            if (value is byte[] bytes)
            {
                payload = bytes;
            }
            else if (value is string str)
            {
                payload = Encoding.UTF8.GetBytes(str);
            }
            else if (value is IFormattable formattable)
            {
                // Use IFormattable for fast conversion of primitives (DateTime, numbers)
                // CultureInfo.InvariantCulture must be static for hash consistency!
                payload = Encoding.UTF8.GetBytes(formattable.ToString(null, CultureInfo.InvariantCulture));
            }
            else if (value.GetType().IsClass)
            {
                // 3. Complex Types (EF vs. DTO)
                try
                {
                    logInfo.AppendLine($"Elapsed before serialization: {stopwatch.ElapsedMilliseconds} ms");
                    if (EfEntityHasher.TryGetEntityRepresentation((dynamic)value, out string hashFromEF) && !string.IsNullOrEmpty(hashFromEF))
                    {
                        payload = Encoding.UTF8.GetBytes(hashFromEF);
                        logInfo.AppendLine($"Elapsed after EF serialization: {stopwatch.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        try
                        {
                            logInfo.AppendLine($"Elapsed before JsonConvert serialization: {stopwatch.ElapsedMilliseconds} ms");
                            // Non-EF entity - use MessagePack for speed
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                            payload = Encoding.UTF8.GetBytes(json);
                            logInfo.AppendLine($"Elapsed after JsonConvert serialization: {stopwatch.ElapsedMilliseconds} ms");
                        }
                        catch (Exception msgPackEx)
                        {
                            // Log MessagePack failure and fallback to ToString
                            LogCollector.LogCollector.LogError($"{Environment.MachineName} [CacheHash] Key:{key} MessagePack failed for type {typeof(T).FullName}: {msgPackEx.ToString()}");
                            payload = Encoding.UTF8.GetBytes(value.ToString());
                            logInfo.AppendLine($"Elapsed after ToString fallback: {stopwatch.ElapsedMilliseconds} ms");
                            var json = System.Text.Json.JsonSerializer.Serialize(value);
                            logInfo.AppendLine($"Elapsed after System.Text.Json serialization: {stopwatch.ElapsedMilliseconds} ms");
                            payload = Encoding.UTF8.GetBytes(json);
                        }
                    }
                }

                catch (Exception ex)
                {
                    // Log the serialization failure
                    LogCollector.LogCollector.LogError($"{Environment.MachineName} [CacheHash] Key:{key} Serialize fallback failed for type {typeof(T).FullName}: {ex}");

                    if (payload == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Fallback to type name and size only
                        long count = (dynamic)value is ICollection collection ? collection.Count : 1;
                        sb.AppendLine($"{DateTime.Today.Day}${DateTime.Now.Hour}");
                        sb.AppendLine($" | Count: {count}");
                        sb.AppendLine($"  | Type: {value.GetType().FullName}");
                        var payloadInformation = sb.ToString();
                        payload = Encoding.UTF8.GetBytes(payloadInformation);
                    }
                }
            }
            else
            {
                payload = Encoding.UTF8.GetBytes(value.ToString());
            }

            // 4. Compute XXHash32
            logInfo.AppendLine($"Elapsed before XXH32: {stopwatch.ElapsedMilliseconds} ms");
            uint hash = ComputeXXH32(payload);
            logInfo.AppendLine($"Elapsed after XXH32: {stopwatch.ElapsedMilliseconds} ms");
            string hashHex = hash.ToString("x8");

            // 5. Check Performance and Log if Slow
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowHashThresholdMs)
            {
                LogCollector.LogCollector.LogWithTrace(
                    $"[CacheHash SLOW] Hashing type {typeof(T).FullName} took {elapsedMs} ms. " +
                    $"Payload size: {payload.Length} bytes. Ensure data model excludes volatile fields ([IgnoreMember])." +
                    $" Key: {key}" +
                    $" Timelog: {Environment.NewLine}{logInfo}", logLevel: LogLevel.Warning
                );
            }

            return hashHex;
        }

        private static byte[] MessagePackSerializerSerialize<T>(T value)
        {
            // Define the timeout duration (4 seconds)
            const int TimeoutMs = 4000;

            // Use a CancellationTokenSource to manage the timeout
            using (var cts = new CancellationTokenSource())
            {
                // 1. Create a Task to run the synchronous serialization operation
                Task<byte[]> serializeTask = Task.Run(() =>
                {
                    // If the task is cancelled before starting (unlikely here), throw.
                    cts.Token.ThrowIfCancellationRequested();

                    // Execute the actual synchronous MessagePack call
                    return MessagePackSerializer.Serialize(value, ContractlessOptions);

                }, cts.Token); // Pass the token to link the task lifecycle

                try
                {
                    // 2. Wait for the task to complete, enforcing the timeout
                    if (serializeTask.Wait(TimeoutMs, cts.Token))
                    {
                        // Success: Serialization completed within 10 seconds
                        return serializeTask.Result;
                    }
                    else
                    {
                        // Failure: Timeout occurred
                        cts.Cancel(); // Attempt to cancel the task

                        // Throw the custom exception
                        throw new TimeoutException($"MessagePack serialization timed out after {TimeoutMs / 1000} seconds for type {typeof(T).FullName}.");
                    }
                }
                catch (AggregateException aex) when (aex.InnerExceptions.Count == 1 && aex.InnerException is OperationCanceledException)
                {
                    // This catches the cancellation exception if the task times out or is explicitly cancelled
                    throw new TimeoutException($"MessagePack serialization timed out after {TimeoutMs / 1000} seconds for type {typeof(T).FullName}.", aex.InnerException);
                }
                catch (AggregateException aex)
                {
                    // Unwrap and rethrow the actual serialization exception (e.g., MsgPackFormatterException)
                    throw aex.InnerException ?? aex;
                }
            }
        }



        /// <summary>
        /// Handles custom formatting for primitive types (e.g., DateTime).
        /// </summary>
        private static string HandlePrimitiveProperty(string propName, object propValue)
        {
            if (propValue == null) return "null";

            if (propValue is DateTime dt)
            {
                return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                return propValue.ToString();
            }
        }

        // --- XXHash Implementation Helper Method ---

        private static uint ComputeXXH32(byte[] data, uint seed = 0)
        {
            // ... (XXHash32 implementation remains as provided) ...
            unchecked
            {
                uint h32 = seed;
                int index = 0;
                int length = data.Length;

                if (length >= 16)
                {
                    uint v1 = seed + Prime1 + Prime2;
                    uint v2 = seed + Prime2;
                    uint v3 = seed;
                    uint v4 = seed - Prime1;

                    do
                    {
                        v1 += BitConverter.ToUInt32(data, index) * Prime2;
                        v1 = (v1 << 13) | (v1 >> 19);
                        v1 *= Prime1;
                        index += 4;

                        v2 += BitConverter.ToUInt32(data, index) * Prime2;
                        v2 = (v2 << 13) | (v2 >> 19);
                        v2 *= Prime1;
                        index += 4;

                        v3 += BitConverter.ToUInt32(data, index) * Prime2;
                        v3 = (v3 << 13) | (v3 >> 19);
                        v3 *= Prime1;
                        index += 4;

                        v4 += BitConverter.ToUInt32(data, index) * Prime2;
                        v4 = (v4 << 13) | (v4 >> 19);
                        v4 *= Prime1;
                        index += 4;
                    } while (index <= length - 16);

                    h32 = ((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) +
                          ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14));
                }
                else
                {
                    h32 = seed + Prime5;
                }

                h32 += (uint)length;

                while (index + 4 <= length)
                {
                    h32 += BitConverter.ToUInt32(data, index) * Prime3;
                    h32 = ((h32 << 17) | (h32 >> 15)) * Prime4;
                    index += 4;
                }

                while (index < length)
                {
                    h32 += data[index] * Prime5;
                    h32 = ((h32 << 11) | (h32 >> 21)) * Prime1;
                    index++;
                }

                h32 ^= h32 >> 15;
                h32 *= Prime2;
                h32 ^= h32 >> 13;
                h32 *= Prime3;
                h32 ^= h32 >> 16;

                return h32;
            }
        }
    }

    public static class EfEntityHasher
    {
        static Random random = new Random();
        private static ThreadLocal<Dictionary<string, string>> threadLocalCache =
        new ThreadLocal<Dictionary<string, string>>(() => new Dictionary<string, string>());

        public static bool TryGetEntityRepresentation<TEntity>(TEntity entity, out string hash) where TEntity : class
        {
            hash = null;
            if (entity == null)
            {
                return false;
            }

            if (!EntityUtil.IsEntityFrameworkClass(entity))
                return false;

            var localCache = threadLocalCache.Value;
            var internalHashCode = entity.GetHashCode().ToString();

            if (localCache.TryGetValue(internalHashCode, out hash))
            {
                return true;
            }

            if (entity is ICollection)
                return TryGetEntityRepresentationFromCollection((IEnumerable)entity, out hash);

            hash = GetInternalRepresentation(entity, 0);
            if (threadLocalCache.Value.ContainsKey(internalHashCode))
                threadLocalCache.Value[internalHashCode] = hash;

            return true;
        }

        public static bool TryGetEntityRepresentationFromCollection(IEnumerable entities, out string hash)
        {
            hash = null;
            if (entities == null)
            {
                return false;
            }

            hash = GetInternalRepresentation(entities, 0);
            return true;
        }

        private static string GetInternalRepresentation(object item, int depth)
        {
            if (depth > 2)
            {
                return string.Empty; // Stop recursion beyond max depth
            }

            var sb = new StringBuilder();
            if (item is IEnumerable enumerable && !(item is string)) // Treat as collection, but exclude strings
            {
                BuildCollectionRepresentation(sb, enumerable, depth);
            }
            else
            {
                BuildEntityRepresentation(sb, item, depth);
            }

            return sb.ToString();
        }

        private static void BuildEntityRepresentation(StringBuilder sb, object entity, int depth)
        {
            var type = entity.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .OrderBy(p => p.Name) // Sort properties by name for consistency
                                 .ToList();

            sb.Append("{");
            for (int i = 0; i < properties.Count; i++)
            {
                var prop = properties[i];
                var value = prop.GetValue(entity, null);
                sb.Append(prop.Name).Append(":");

                if (value == null)
                {
                    sb.Append("null");
                }
                else
                {
                    var propType = prop.PropertyType;
                    if (propType.IsValueType || propType == typeof(string))
                    {
                        sb.Append(value.ToString());
                    }
                    else if (depth < 2)
                    {
                        sb.Append(GetInternalRepresentation(value, depth + 1)); // Recurse for nested entities
                    }
                    else
                    {
                        sb.Append("[max_depth]");
                    }
                }

                if (i < properties.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");
        }

        private static void BuildCollectionRepresentation(StringBuilder sb, IEnumerable collection, int depth)
        {
            var itemRepresentations = new List<string>();
            foreach (var item in collection)
            {
                if (item != null)
                {
                    itemRepresentations.Add(GetInternalRepresentation(item, depth + 1));
                }
                else
                {
                    itemRepresentations.Add("null");
                }
            }

            // Sort the representations for order-independent hashing
            itemRepresentations.Sort();

            sb.Append("[");
            for (int i = 0; i < itemRepresentations.Count; i++)
            {
                sb.Append(itemRepresentations[i]);
                if (i < itemRepresentations.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("]");
        }

        private static string ComputeMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
