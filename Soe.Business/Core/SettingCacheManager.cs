using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public sealed class SettingCacheManager
    {
        #region Variables

        static readonly List<string> allowedUrlParametersInFavorties = new List<string>(){ "classificationgroup" };

        // Singleton (thread safe)
        static readonly object getUserFavoritesLock = new object();
        static readonly object addUserFavoriteLock = new object();
        static readonly object removeUserFavoriteLock = new object();
        static readonly object removeUserFavoritesFromCacheLock = new object();
        
        //Cache
        private readonly Hashtable userFavoritesCache = new Hashtable();

        private readonly SettingManager sm;

        #endregion

        #region Singleton

        private SettingCacheManager()
        {
            sm = new SettingManager(null);
        }
        private static readonly Lazy<SettingCacheManager> instance = new Lazy<SettingCacheManager>(() => new SettingCacheManager());
        public static SettingCacheManager Instance 
        { 
            get => instance.Value; 
        }

        #endregion

        #region Favorites

        public List<FavoriteItem> GetUserFavorites(int userId)
        {
            //Get UserFavorites from Cache
            List<FavoriteItem> userFavorites = GetUserFavoritesFromCache(userId);
            if (userFavorites != null)
                return userFavorites;

            //Get UserFavorites from database and add to Cache
            return GetUserFavoritesTS(userId);
        }

        /// <summary>
        /// Synchronizes all access to database and Cache.
        /// Adds UserFavorites to Cache.
        /// </summary>
        public List<FavoriteItem> GetUserFavoritesTS(int userId)
        {
            lock (getUserFavoritesLock)
            {
                //Try UserFavorites from Cache. Could be added by other user when holding lock
                List<FavoriteItem> userFavorites = GetUserFavoritesFromCache(userId);
                if (userFavorites != null)
                    return userFavorites;

                //Try get UserFavorites from database
                userFavorites = GetUserFavoritesFromDatabase(userId);
                if (userFavorites != null)
                {
                    //Add UserFavorites to Cache
                    userFavoritesCache.Add(userId.ToString(), userFavorites);
                }

                return userFavorites;
            }
        }

        /// <summary>
        /// Get UserFavorites from Cache.
        /// </summary>
        private List<FavoriteItem> GetUserFavoritesFromCache(int userId)
        {
            return (List<FavoriteItem>)userFavoritesCache[userId.ToString()];
        }

        /// <summary>
        /// Get UserFavorites from database.
        /// </summary>
        private List<FavoriteItem> GetUserFavoritesFromDatabase(int userId)
        {
            return sm.GetUserFavoriteItems(userId);
        }

        public ActionResult AddUserFavoriteTS(int userId, int currentActorCompanyId, int? actorCompanyId, string name, string url, bool isDefaultPage)
        {
            lock (addUserFavoriteLock)
            {
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                {
                    // If global favorite, remove company parameter from URL
                    if (!actorCompanyId.HasValue)
                    {
                        string param = "?c=" + currentActorCompanyId;
                        int pos = url.IndexOf(param);
                        if (pos > 0)
                        {
                            NameValueCollection qscoll = WebUtil.ParseQueryString(url);

                            url = url.Substring(0, pos);
                            bool first = true;
                            //Add back allowed url params
                            foreach (var q in qscoll.AllKeys)
                            {
                                if (allowedUrlParametersInFavorties.Contains(q))
                                {
                                    url += first ? "?" : "&";
                                    url += $"{q}={qscoll[q]}";

                                    first = false;
                                }
                            }
                        }
                    }

                    var userFavorite = new UserFavorite
                    {
                        Name = name,
                        Url = url,
                        IsDefault = isDefaultPage,
                    };

                    //Add UserFavorite to database
                    ActionResult result = sm.AddUserFavorite(userFavorite, userId, actorCompanyId);
                    if (!result.Success)
                        return result;

                    //Add UserFavorite to Cache
                    List<FavoriteItem> userFavorites = GetUserFavoritesFromCache(userId);
                    userFavorites.Add(new FavoriteItem
                    {
                        FavoriteId = userFavorite.UserFavoriteId,
                        FavoriteName = name,
                        FavoriteUrl = url,
                        FavoriteCompany = actorCompanyId,
                        IsDefault = isDefaultPage,
                    });
                }

                return new ActionResult(true);
            }
        }

        public ActionResult DeleteUserFavoriteTS(int userId, int userFavoriteId)
        {
            lock (removeUserFavoriteLock)
            {
                //Remove UserFavorite from database
                ActionResult result = sm.DeleteUserFavorite(userId, userFavoriteId);
                if (!result.Success)
                    return result;

                List<FavoriteItem> userFavorites = GetUserFavoritesFromCache(userId);

                FavoriteItem favoriteItem = userFavorites.FirstOrDefault(fi => fi.FavoriteId == userFavoriteId);
                if (favoriteItem == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "FavoriteItem");

                //Remove UserFavorite from Cache
                userFavorites.Remove(favoriteItem);
                //userFavoritesCache.Remove(userId);

                return new ActionResult();
            }
        }

        public void RemoveFavoritesFromCacheTS(int userId)
        {
            lock (removeUserFavoritesFromCacheLock)
            {
                string userIdStr = userId.ToString();

                if (userFavoritesCache.ContainsKey(userIdStr))
                    userFavoritesCache.Remove(userIdStr);
            }
        }

        #endregion

        #region Default page

        /// <summary>
        /// Returnes favorite that is default or an item that is null if no default has been stored
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetFavoriteUrl(int userId)
        {
            string url = string.Empty;
            var favorites = GetUserFavorites(userId);
            foreach (var favorite in favorites)
            {
                if (favorite.IsDefault)
                {
                    url = favorite.FavoriteUrl;
                    break;
                }
            }
            return url;
        }

        public ActionResult RemoveFavoriteDefaultPage(int userId)
        {
            ActionResult result = new ActionResult(true);

            //Get default FavoriteItem
            List<FavoriteItem> items = GetUserFavorites(userId);
            FavoriteItem defaultItem = items.FirstOrDefault(i => i.IsDefault);
            if (defaultItem == null)
                return new ActionResult(true);

            int defaultUserFavoriteId = defaultItem.FavoriteId;
            lock (removeUserFavoriteLock)
            {
                //Get UserFavorite
                UserFavorite userFavorite = sm.GetUserFavorite(defaultUserFavoriteId, userId);
                if (userFavorite == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "UserFavorite");

                //Update UserFavorite
                userFavorite.IsDefault = false;
                result = sm.UpdateUserFavorite(userId, userFavorite);
                if (!result.Success)
                    return result;

                //Reset cached default FavoriteItem
                items = GetUserFavoritesFromCache(userId);
                defaultItem = items.FirstOrDefault(fi => fi.FavoriteId == defaultUserFavoriteId);
                if (defaultItem != null)
                    defaultItem.IsDefault = false;
            }

            return result;
        }

        #endregion
    }
}
