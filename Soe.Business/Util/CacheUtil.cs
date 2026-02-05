using CachingFramework.Redis;
using CachingFramework.Redis.Serializers;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace SoftOne.Soe.Business.Util
{
    public class CacheUtil
    {
        private readonly string connection;
        private readonly TimeSpan timeToLive;
        private readonly JsonSerializer jsonSerializer;

        public CacheUtil()
        {
            this.jsonSerializer = new JsonSerializer();
            this.connection = CompDbCache.Instance.RedisCacheConnectionString;
            this.timeToLive = TimeSpan.FromMinutes(30);
            var info = new StackExchange.Redis.ClientInfo(); // Force loading of StackExchange
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(CompDbCache.Instance.RedisCacheConnectionString);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public ActionResult AddRangeToList<T>(Dictionary<string, T> keyObjectDict, string listKey, int? minutesToLive = null)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.SetHashed<T>(listKey, keyObjectDict);
                    context.Cache.KeyTimeToLive(listKey, minutesToLive.HasValue ? TimeSpan.FromMinutes(minutesToLive.Value) : timeToLive);
                    AddCacheKeyTime(listKey, minutesToLive);
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "AddRangeToList", listKey);
            }

            return result;
        }

        public ActionResult AddObjectToList<T>(T obj, string listKey, string propertyId, int? minutesToLive = null, bool addToCacheKeyTime = true)
        {
            Dictionary<string, T> keyObjectDict = new Dictionary<string, T>();
            keyObjectDict.Add(propertyId, obj);

            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.SetHashed<T>(listKey, keyObjectDict);
                    context.Cache.KeyTimeToLive(listKey, minutesToLive.HasValue ? TimeSpan.FromMinutes(minutesToLive.Value) : timeToLive);

                    if (addToCacheKeyTime)
                        AddCacheKeyTime(listKey, minutesToLive);

                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "AddRangeToList", listKey);
            }

            return result;
        }


        public ActionResult FlushAll()
        {
            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.FlushAll();
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "FlushAll", "");
            }

            return result;
        }


        public CacheKeyTime GetCacheKeyTime(string cacheKey)
        {
            string key = "CacheKeyTime";
            return GetObjectFromList<CacheKeyTime>(key, cacheKey);
        }

        public ActionResult RemoveCacheKeyTime(string cacheKey)
        {
            string key = "CacheKeyTime";
            return RemoveObjectFromList(key, cacheKey);
        }

        public ActionResult AddCacheKeyTime(string cacheKey, int? minutesToLive = null)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                CacheKeyTime cacheKeyTime = new CacheKeyTime()
                {
                    CacheKey = cacheKey,
                    Expires = DateTime.UtcNow.Add(minutesToLive.HasValue ? TimeSpan.FromMinutes(minutesToLive.Value) : timeToLive)
                };

                AddObjectToList(cacheKeyTime, "CacheKeyTime", cacheKey, addToCacheKeyTime: false);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "AddCacheKeyTime", cacheKey);
            }

            return result;
        }

        public bool ClearOldCache(string cacheKey)
        {
            string key = "CacheKeyTime";
            var allObjects = GetAllObjectsFromList<CacheKeyTime>(key);

            foreach (var item in allObjects)
            {
                if (item.Expires > DateTime.UtcNow)
                    RemoveCache(item.CacheKey);
            }

            return false;
        }

        public CacheMisses GetCacheMisses(string cacheKey)
        {

            string key = "CacheMisses";
            return GetObjectFromList<CacheMisses>(key, cacheKey);
        }

        public List<CacheMisses> GetCacheMissesList()
        {
            string key = "CacheMisses";
            return GetAllObjectsFromList<CacheMisses>(key);
        }

        public ActionResult RemoveCacheMisses(string cacheKey)
        {
            string key = "CacheMisses";
            return RemoveObjectFromList(key, cacheKey);
        }

        public ActionResult AddCacheMisses(string cacheKey)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                CacheMisses cacheMisses = new CacheMisses()
                {
                    CacheKey = cacheKey,
                    Time = DateTime.UtcNow
                };

                Dictionary<string, CacheMisses> keyObjectDict = new Dictionary<string, CacheMisses>();
                keyObjectDict.Add(cacheKey, cacheMisses);

                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.SetHashed<CacheMisses>("CacheMisses", keyObjectDict);
                    context.Cache.KeyTimeToLive("CacheMisses", TimeSpan.FromMinutes(3600));

                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "AddCacheMisses", cacheKey);
            }

            return result;
        }


        public ActionResult RemoveObjectFromList(string listKey, string propertyId)
        {

            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.RemoveHashed(listKey, propertyId);
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "RemoveObjectFromList", listKey);
            }

            return result;
        }

        public T GetObjectFromList<T>(string listKey, int idKey)
        {
            return GetObjectFromList<T>(listKey, idKey.ToString());
        }

        public T GetObjectFromList<T>(string listKey, string idKey)
        {
            object obj = null;

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    obj = context.Cache.GetHashed<T>(listKey, idKey);
                    if (obj == null)
                        AddCacheMisses(listKey + idKey);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetObjectFromList", listKey);
            }

            return (T)obj;
        }

        public List<T> GetAllObjectsFromList<T>(string listKey)
        {
            List<T> list = new List<T>();

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    IDictionary<string, T> dict = context.Cache.GetHashedAll<T>(listKey);

                    if (dict != null)
                    {
                        foreach (var item in dict)
                        {
                            list.Add(item.Value);
                        }
                    }

                    if (list.IsNullOrEmpty())
                        AddCacheMisses(listKey);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetAllObjectsFromList", listKey);
            }

            return list;
        }

        public ActionResult RemoveCache(string key)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.Remove(key);
                    RemoveCacheKeyTime(key);
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
                LogError(ex, "RemoveCache", key);
            }

            return result;
        }


        public ActionResult AddObject<t>(object obj, string key, int? minutesToLive = null)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                using (RedisContext context = new RedisContext(this.connection, this.jsonSerializer))
                {
                    context.Cache.SetObject(key, obj, minutesToLive.HasValue ? TimeSpan.FromMinutes(minutesToLive.Value) : timeToLive);
                    AddCacheKeyTime(key, minutesToLive);
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();
            }

            return result;
        }

        public void LogError(Exception ex, string message, string key)
        {

        }
    }

    public class CacheKeyTime
    {
        public string CacheKey { get; set; }
        public DateTime Expires { get; set; }
    }

    public class CacheMisses
    {
        public string CacheKey { get; set; }
        public DateTime Time { get; set; }
    }

}


