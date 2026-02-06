using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core
{
    public sealed class LicenseCacheManager
    {
        #region Variables

        // Singleton (thread safe)
        static readonly object getLicenseObjectLock = new object();

        private readonly LicenseManager lm;
        private readonly Hashtable licenseCache;

        #endregion

        #region Singleton

        private LicenseCacheManager()
        {
            lm = new LicenseManager(null);
            licenseCache = new Hashtable();
        }
        private static readonly Lazy<LicenseCacheManager> instance = new Lazy<LicenseCacheManager>(() => new LicenseCacheManager());
        public static LicenseCacheManager Instance
        {
            get => instance.Value;
        }

        #endregion

        #region Login / Logout

        /// <summary>
        /// Method to be called when a User is loggin in.
        /// </summary>
        /// <param name="user">The User to login</param>
        /// <param name="role">The User's default Role</param>
        /// <param name="licenseNr">The LicenseNr to login with</param>
        /// <param name="interruptDuplicate">True if any active Session for the current Users should be interrupted and force a login</param>
        /// <param name="mobileLogin">True if the login is attempted from a mobile unit</param>
        /// <param name="fakeLogin">True if is a load balancing login that validate login on first machine before redirecting to second machine</param>
        /// <param name="detailedMessage">Detailed message concerning any possible login failure</param>
        /// <returns>A SoeLoginState indicating the result of the login</returns>
        public SoeLoginState LoginUser(User user, string licenseNr, bool interruptDuplicate, bool mobileLogin, bool fakeLogin, string userEnvironmentInfo, out string detailedMessage)
        {
            detailedMessage = "";

            if (user == null)
                return SoeLoginState.BadLogin;

            if (user.BlockedFromDate.HasValue && user.BlockedFromDate.Value < DateTime.Now)
                return SoeLoginState.BlockedFromDatePassed;

            if (IsUserLoggedIn(user, licenseNr, user.DefaultActorCompanyId ?? 0))
                return SoeLoginState.OK;

            LicenseObject licenseObject = GetLicenseObject(licenseNr);
            if (licenseObject == null)
                return SoeLoginState.Unknown;

            return licenseObject.LoginUser(user.ToDTO(user.ActiveRoleId), interruptDuplicate, mobileLogin, fakeLogin, userEnvironmentInfo, out detailedMessage);
        }

        /// <summary>
        /// Method to be called when user is logging out or session has timed out
        /// </summary>
        /// <param name="user">The User to logout</param>
        public void LogoutUser(UserDTO user, int actorCompanyId)
        {
            if (user == null)
                return;

            LicenseObject licenseObject = GetLicenseObject(user.LicenseNr);
            if (licenseObject == null)
                return;

            licenseObject.LogoutUser(user, actorCompanyId);
        }

        #endregion

        #region LicenseObject

        public IEnumerable<LicenseObject> GetLicenseObjects()
        {
            List<LicenseObject> licenseObjects = new List<LicenseObject>();

            IDictionaryEnumerator cacheEnum = licenseCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                LicenseObject licenseObject = (LicenseObject)licenseCache[cacheEnum.Key];
                if (licenseObject != null)
                    licenseObjects.Add(licenseObject);
            }

            return licenseObjects;
        }

        /// <summary>
        /// First try to get License from Cache.
        /// Second it calls the thread-safe method to get License from database.
        /// </summary>
        public LicenseObject GetLicenseObject(string licenseNr)
        {
            //Get License from Cache
            LicenseObject licenseObject = GetLicenseObjectFromCache(licenseNr);
            if (licenseObject != null)
                return licenseObject;

            //Get License from database and add to Cache
            return GetLicenseObjectTS(licenseNr);
        }

        /// <summary>
        /// Synchronizes all access to database and Cache.
        /// Adds License to Cache.
        /// </summary>
        private LicenseObject GetLicenseObjectTS(string licenseNr)
        {
            lock (getLicenseObjectLock)
            {
                //Try get term from Cache. Could be added by other user when holding lock
                LicenseObject licenseObject = GetLicenseObjectFromCache(licenseNr);
                if (licenseObject != null)
                    return licenseObject;

                //Try get License from database
                licenseObject = GetLicenseObjectFromDatabase(licenseNr);
                if (licenseObject != null)
                    licenseCache.Add(licenseNr, licenseObject);

                return licenseObject;
            }
        }

        /// <summary>
        /// Get License from Cache.
        /// </summary>
        private LicenseObject GetLicenseObjectFromCache(string licenseNr)
        {
            return (LicenseObject)licenseCache[licenseNr];
        }

        /// <summary>
        /// Get License from database.
        /// </summary>
        private LicenseObject GetLicenseObjectFromDatabase(string licenseNr)
        {
            License license = lm.GetLicenseByNr(licenseNr);
            if (license == null)
                return null;

            return new LicenseObject(license);
        }

        public License GetLicense(string licenseNr)
        {
            LicenseObject licenseObject = GetLicenseObject(licenseNr);
            if (licenseObject == null)
                return null;

            return licenseObject.License;
        }

        public bool IsUserLoggedIn(User user, string licenseNr, int actorCompanyId)
        {
            LicenseObject licenseObject = GetLicenseObject(licenseNr);
            if (licenseObject == null)
                return false;

            return licenseObject.IsUserValid(user, actorCompanyId);
        }

        public bool IsUserLoggedIn(UserDTO user, string licenseNr, int actorCompanyId)
        {
            LicenseObject licenseObject = GetLicenseObject(licenseNr);
            if (licenseObject == null)
                return false;

            return licenseObject.IsUserValid(user, actorCompanyId);
        }

        #endregion
    }
}
