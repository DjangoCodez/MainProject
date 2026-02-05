using Soe.CacheR;
using System;
using System.Runtime.Caching;

namespace SoftOne.Soe.Shared.Cache
{
    public sealed class SoeCache
    {
        #region CacheR

        public static string RedisConnectionString { get; set; }
        static readonly object connectorLock = new object();
        private Connector connector { get; set; }
        public Connector Connector
        {
            get
            {
                if (connector == null)
                {
                    lock (connectorLock)
                    {
                        connector = new Connector(new CacheROptions()
                        {
                            CacheRType = Enumerations.CacheRType.CsRedis,
                            ConnectionString = RedisConnectionString,
                            IsTest = true,
                            Project = "SoeCache",
                            CsRedisOptions = new CsRedisOptions()
                            {
                                Instances = 2,
                            }
                        });
                    }
                }

                return connector;
            }
        }

        #endregion

        #region Variables

        #endregion

        #region Ctor

        public SoeCache()
        {

        }

        #endregion

        #region Fully lazy instantiation

        public static SoeCache Instance
        {
            get
            {
                return NestedSingleton.instance;
            }
        }

        private class NestedSingleton
        {
            internal static readonly SoeCache instance = new SoeCache();

            static NestedSingleton()
            {
            }
        }

        #endregion

        #region Object Implementation

        public void RemoveFromCache(string key)
        {
            try
            {
                MemoryCache.Default.Remove(key);
                connector.RemoveCache(key);
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public T GetValueFromCache<T>(string key, bool checkRedis = true)
        {
            try
            {
                var value = (T)MemoryCache.Default.Get(key);

                if (value == null && checkRedis)
                {
                    value = Connector.GetObject<T>(key);

                    if (value != null)
                    {
                        int ttl = connector.GetTimeToLive(key);

                        if (ttl > 2)
                        {
                            AddValueToCache(value, key, Convert.ToInt32(decimal.Round(ttl, 2)), true, false);
                        }
                    }
                }
                return value;
            }
            catch
            {
                return (T)MemoryCache.Default.Get(key);
            }
        }

        public void AddValueToCache<T>(T value, string key, int minutesToLive = 5, bool AddToRunTimeCache = true, bool AddToRedis = true, int secondsToLive = 0)
        {
            try
            {
                if (secondsToLive == 0)
                {
                    if (AddToRunTimeCache && minutesToLive > 0)
                    {
                        CacheItem cacheItem = new CacheItem(key);
                        cacheItem.Value = value;
                        MemoryCache.Default.Set(cacheItem, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(minutesToLive) });
                    }


                    if (AddToRedis && minutesToLive > 0)
                        Connector.AddObject<T>(value, key, minutesToLive);
                }
                else
                {
                    if (AddToRunTimeCache && secondsToLive > 0)
                    {
                        CacheItem cacheItem = new CacheItem(key);
                        cacheItem.Value = value;
                        MemoryCache.Default.Set(cacheItem, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(secondsToLive) });
                    }


                    if (AddToRedis && secondsToLive > 0)
                        Connector.AddObject<T>(value, key, decimal.Round(decimal.Divide(secondsToLive, 60), 2));
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        #endregion
    }
}

