using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;

namespace SoftOne.Soe.Business.Util
{
    public class LicenseObject
    {
        #region Variables

        private LicenseManager lm;
        private UserManager um;

        public License License;
        public int CurrentConcurrentUsers;

        #endregion

        #region Ctor

        public LicenseObject(License license)
        {
            this.License = license;
        }

        #endregion

        #region Properties

        private Cache Cache
        {
            get
            {
                return ((Page)HttpContext.Current.Handler).Cache;
            }
        }

        #endregion

        #region Public methods

        public List<User> GetLoggedInUsers()
        {
            #region Init

            if (um == null)
                um = new UserManager(null);

            #endregion

            List<User> userLoggedIn = new List<User>();
            List<User> usersInLicense = um.GetUsersByLicense(License.LicenseId, 0, 0, 0);

            var cacheEnum = Cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                string cacheItem = cacheEnum.Key.ToString();
                int userId = um.GetUserIdFromCache(cacheItem);
                if (userId > 0)
                {
                    User user = usersInLicense.FirstOrDefault(i => i.UserId == userId);
                    if (user != null)
                    {
                        user.LastUserSession = um.GetLastUserSession(user.UserId);
                        if (user.LastUserSession != null)
                            user.LoggedIn = user.LastUserSession.Login.ToString();

                        userLoggedIn.Add(user);
                    }
                }
            }

            return userLoggedIn;
        }

        public bool IsUserValid(User user, int actorCompanyId)
        {
            string cachedUserInfo = GetUserFromCache(user, actorCompanyId);
            if (String.IsNullOrEmpty(cachedUserInfo))
                return false;
            else
                return true;
        }

        public bool IsUserValid(UserDTO user, int actorCompanyId)
        {
            string cachedUserInfo = GetUserFromCache(user, actorCompanyId);
            if (String.IsNullOrEmpty(cachedUserInfo))
                return false;

            else
                return true;
        }

        public SoeLoginState LoginUser(UserDTO user, bool interruptDuplicate, bool mobileLogin, bool fakeLogin, string userEnvironmentInfo, out string detailedMessage)
        {
            detailedMessage = "";

            //Check if License has been terminated
            if (IsLicenseTerminated())
                return SoeLoginState.LicenseTerminated;

            //// Check if User is mobile user
            //if (mobileLogin && !user.IsMobileUser)
            //    return SoeLoginState.IsNotMobileUser;

            //Check if User already is logged in

            if (!this.License.AllowDuplicateUserLogin)
            {
                string cachedUserInfo = GetUserFromCache(user, user.DefaultActorCompanyId ?? 0);
                if (!string.IsNullOrEmpty(cachedUserInfo))
                {

                    detailedMessage = cachedUserInfo;

                    #region Deprecated (Prevents "Take over session" to work when multiple users have same ip-nr or behind proxy)
                    /*
                    //Can login if the User is on the same physical machine (useful after a browser crash)
                    if (cachedUserInfo == SysLogManager.GetUserEnvironmentInfo())
                        return SoeLoginState.OK;
                    */
                    #endregion

                    //Can logout other session
                    if (!interruptDuplicate)
                    {
                        LogCollector.LogCollector.LogInfo(String.Format("LicenseObject.LoginUser: Duplicate login detected for UserId:{0}, LoginName:{1}, LicenseId:{2}, LicenseName:{3}",
                            user.UserId,
                            user.LoginName,
                            (this.License != null ? this.License.LicenseId.ToString() : "NULL"),
                            (this.License != null ? this.License.Name : "NULL")));
                        return SoeLoginState.DuplicateUserLogin;
                    }
                }
            }

            //Check License concurrent users
            if (CurrentConcurrentUsers >= License.ConcurrentUsers)
                return SoeLoginState.ConcurrentUserViolation;

            //No user in Cache, so session is either expired or user is new sign-on
            if (!mobileLogin && !fakeLogin)
                InsertUserToCache(user, userEnvironmentInfo);

            return SoeLoginState.OK;
        }

        public void LogoutUser(UserDTO user, int actorCompanyId)
        {
            RemoveUserFromCache(user, actorCompanyId);
        }

        #endregion

        #region Help-methods

        private string GetUserFromCache(User user, int actorCompanyId)
        {
            if (user == null || !IsInWebContext())
                return String.Empty;

            return SessionCache.GetUserFromCache(user.UserId, actorCompanyId)?.LoginName;
        }

        private string GetUserFromCache(UserDTO user, int actorCompanyId)
        {
            if (user == null || !IsInWebContext())
                return String.Empty;

            return SessionCache.GetUserFromCache(user.UserId, actorCompanyId)?.LoginName;
        }

        private void InsertUserToCache(UserDTO user, string userEnvironmentInfo)
        {
            if (user == null || !IsInWebContext())
                return;

            //Set sliding expiration to the Timeout defined in Web.config. Will be renewed on each page request in PageBase.Init()
            TimeSpan slidingExpiration = new TimeSpan(0, 0, HttpContext.Current.Session.Timeout, 0, 0);

            //Callback raised when a user is removed from Cache
            CacheItemRemovedCallback onLogoutUser = new CacheItemRemovedCallback(this.UserLoggedOut);

            //Add user to Cache
            HttpContext.Current.Cache.Insert(UserManager.GetUserCacheCredentials(user), userEnvironmentInfo, null, Cache.NoAbsoluteExpiration, slidingExpiration, CacheItemPriority.NotRemovable, onLogoutUser);
            CurrentConcurrentUsers++;
        }

        private void RemoveUserFromCache(UserDTO user, int actorCompanyId)
        {
            if (user == null || !IsInWebContext() || this.License.AllowDuplicateUserLogin)
                return;

            //Remove User from cache. Callback function UserLoggedOut() will count down currentConcurrentUsers
            SessionCache.RemoveUserFromCache(user.UserId, actorCompanyId);
        }

        private bool IsInWebContext()
        {
            return (HttpContext.Current != null && HttpContext.Current.Handler != null && (HttpContext.Current.Handler is Page));
        }

        private bool IsLicenseTerminated()
        {
            if (lm == null)
                lm = new LicenseManager(null);

            return lm.IsLicenseTerminated(License.LicenseNr);
        }

        #endregion

        #region Events

        private void UserLoggedOut(string key, object value, CacheItemRemovedReason reason)
        {
            //string message = String.Format("UserLoggedOut#LicenseId:{0}#LicenseName:{1}#key:{2}#value:{3}#reason{4}#CurrentConcurrentUsers{5}",
            //    (this.License != null ? this.License.LicenseId.ToString() : "NULL"),
            //    (this.License != null ? this.License.Name : "NULL"),
            //    (key != null ? key : "NULL"),
            //    (value != null ? value : "NULL"),
            //    ((int)reason),
            //    CurrentConcurrentUsers);
            //var slm = new SysLogManager(null);
            //slm.AddSysLog(new Exception(message), log4net.Core.Level.Error, HttpContext.Current);

            if (CurrentConcurrentUsers > 0)
                CurrentConcurrentUsers--;
        }

        #endregion
    }
}
