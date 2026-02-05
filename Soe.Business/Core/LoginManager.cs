using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class LoginManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public LoginManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Login

        public SoeSupportLoginState LoginSupportByCompany(int actorCompanyId, UserDTO supportUser, out Company company, out User user, out Role role)
        {
            user = null;
            role = null;

            #region User, Company and role

            company = CompanyManager.GetCompany(actorCompanyId, true);
            if (company == null || company.License == null)
                return SoeSupportLoginState.NotLoggedIn;
            if (!company.IsSupportLoginAllowed())
                return SoeSupportLoginState.NotAllowed;

            role = RoleManager.GetRoleAdmin(company.ActorCompanyId);
            if (role == null)
                return SoeSupportLoginState.NotLoggedIn;

            user = UserManager.GetUserInCompanyRole(role.RoleId, company.ActorCompanyId);
            if (user == null)
                return SoeSupportLoginState.NotLoggedIn;

            user.ActiveRoleId = role.RoleId;

            #endregion

            #region Clear cache

            //Remove users favorites from favorites cache for current user
            SettingCacheManager.Instance.RemoveFavoritesFromCacheTS(supportUser.UserId);

            #endregion

            return SoeSupportLoginState.OK;
        }

        public SoeSupportLoginState LoginSupportByUser(int userId, UserDTO supportUser, out Company company, out User user, out Role role)
        {
            company = null;
            role = null;

            #region User, Company and role

            user = UserManager.GetUser(userId, onlyActive: true, loadLicense: true);
            if (user == null || !user.DefaultActorCompanyId.HasValue)
                return SoeSupportLoginState.NotLoggedIn;

            int defaultRoleId = UserManager.GetDefaultRoleId(user.DefaultActorCompanyId.Value, user);
            role = defaultRoleId > 0 ? RoleManager.GetRole(defaultRoleId) : null;
            if (role == null)
                return SoeSupportLoginState.NotLoggedIn;

            company = CompanyManager.GetCompany(user.DefaultActorCompanyId.Value, loadLicense: true);
            if (company == null)
                return SoeSupportLoginState.NotLoggedIn;
            if (!company.IsSupportLoginAllowed())
                return SoeSupportLoginState.NotAllowed;

            user.ActiveRoleId = role.RoleId;

            #endregion

            #region Clear cache

            //Remove users favorites from favorites cache for current user
            SettingCacheManager.Instance.RemoveFavoritesFromCacheTS(supportUser.UserId);

            #endregion

            return SoeSupportLoginState.OK;
        }

        public SoeLoginState LoginUser(string licenseNr, string loginName, string password, out string detailedMessage, out Company company, out User user, out Role role, bool interruptDuplicate = false, bool mobileLogin = false, bool fakeLogin = false)
        {
            if (string.IsNullOrEmpty(password))
            {
                user = null;
                role = null;
                company = null;
                detailedMessage = "Password is empty";
                return SoeLoginState.BadLogin;
            }

            byte[] passwordhash = GetPasswordHash(loginName, password);
            return LoginUser(licenseNr, loginName, passwordhash, out detailedMessage, out company, out user, out role, interruptDuplicate, mobileLogin, fakeLogin, password);
        }

        public SoeLoginState LoginUser(string licenseNr, string loginName, byte[] passwordhash, out string detailedMessage, out Company company, out User user, out Role role, bool interruptDuplicate = false, bool mobileLogin = false, bool fakeLogin = false, string password = "")
        {
            #region Init

            company = null;
            user = null;
            role = null;
            detailedMessage = "";

            if (String.IsNullOrEmpty(licenseNr) || String.IsNullOrEmpty(loginName))
                return SoeLoginState.BadLogin;

            // Store loginname in lower case to make it case-insensitive
            loginName = loginName.ToLowerInvariant();

            #endregion

            #region SoftOneId

            if (!string.IsNullOrEmpty(password))
                user = ValidateLoginSoftOneId(licenseNr, loginName, password);

            #endregion

            #region Password

            if (user == null)
            {
                user = UserManager.GetUser(licenseNr, loginName, passwordhash, true);
                if (user == null)
                    return SoeLoginState.BadLogin;
            }

            #endregion

            #region Default Company and Role

            //Set current role
            if (UserManager.TryGetUserDefaultRoleAndCompany(user, mobileLogin, out company, out role))
                user.ActiveRoleId = role.RoleId;

            if (company == null)
                return SoeLoginState.BadDefaultCompany;
            if (role == null)
                return SoeLoginState.RoleNotConnectedToCompany;

            user.DefaultRoleName = RoleManager.GetRoleNameText(role);

            #endregion

            #region License restrictions

            LoginManager.BlockedFromDateValidation(user);
            return LicenseCacheManager.Instance.LoginUser(user, licenseNr, interruptDuplicate, mobileLogin, fakeLogin, parameterObject?.ExtendedUserParams?.UserEnvironmentInfo, out detailedMessage);

            #endregion
        }

        public string GetLoginErrorMessage(SoeLoginState state)
        {
            string message = "";

            if (state != SoeLoginState.OK)
            {
                message = GetText(1456, "Inloggningen misslyckades") + ", ";
                if (state == SoeLoginState.BadLogin)
                    message += GetText(1453, "felaktigt användarnamn eller lösenord");
                else if (state == SoeLoginState.ConcurrentUserViolation)
                    message += GetText(1534, "licensen tillåter inte fler samtidiga användare");
                else if (state == SoeLoginState.DuplicateUserLogin)
                    message += GetText(1535, "användaren är redan inloggad");
                else if (state == SoeLoginState.RoleNotConnectedToCompany)
                    message += GetText(1955, "användarens default roll inte kopplat till företaget");
                else if (state == SoeLoginState.LicenseTerminated)
                    message += GetText(5519, "licensen är avslutad");
                else if (state == SoeLoginState.IsNotMobileUser)
                    message += GetText(5676, "ej registrerad mobilanvändare");
                else if (state == SoeLoginState.LoginInvalidOrExceededTimeout)
                    message += GetText(5998, "inlogget är ogiltigt eller tog för lång tid");
                else if (state == SoeLoginState.LoginServerNotFound)
                    message += GetText(5180, "ingen kontakt med servern");
                else if (state == SoeLoginState.SoftOneOnlineError)
                    message += GetText(11557, "användaren hittades inte i SoftOne online");
                else if (state == SoeLoginState.BlockedFromDatePassed)
                    message += GetText(11831, "användaren är blockerad");
                else
                    message += GetText(1458, "okänt fel uppstod");
            }

            return message;
        }

        #endregion

        #region Logout

        public bool Logout(UserDTO user, bool supportLogout = false, bool mobileLogout = false, bool forcedLogout = false)
        {
            if (user == null)
                return false;

            //Do not logoff User if is support. Support login is never registred as a login.
            if (!supportLogout && !mobileLogout)
                LicenseCacheManager.Instance.LogoutUser(user, ActorCompanyId);

            if (!forcedLogout)
                ClearCache(user);

            return true;
        }

        private void ClearCache(UserDTO user)
        {
            if (user == null)
                return;

            //Clear favorites cache
            SettingCacheManager.Instance.RemoveFavoritesFromCacheTS(user.UserId);

            //Clear UserCompanyRoles cache
            CompDbCache.Instance.FlushUserCompanyRoles(user.UserId);

            //Clear page cache
            //string absolutPath = "";
            //if (Request.UrlReferrer != null)
            //    absolutPath = Request.UrlReferrer.AbsolutePath;
            //RemoveAllOutputCacheItems(absolutPath);

            //Clear log properties
            //SysLogManager.ClearLog4netProperties();
        }

        public string GetLogoutErrorMessage()
        {
            return GetText(5605, "Utloggning misslyckades");
        }

        #endregion

        #region SysServer

        public SysServer GetSysServer(int sysServerId)
        {
using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
return GetSysServer(sysEntitiesReadOnly, sysServerId);
        }

        public SysServer GetSysServer(SOESysEntities sysEntities, int sysServerId)
        {
            return sysEntities.SysServer.FirstOrDefault(i => i.SysServerId == sysServerId);
        }

        public SysServer GetSysServer(string licenseNr)
        {
            SysServer sysServer = null;

            License license = LicenseManager.GetLicenseByNrFromCache(licenseNr);
            if (license != null && license.SysServerId.HasValue)
                sysServer = GetSysServer(license.SysServerId.Value);

            return sysServer;
        }

        public bool IsOnValidSysServer(int sysServerId, string currentUrl, out string validSysServerUrl)
        {
            validSysServerUrl = String.Empty;

            SysServer sysServer = LoginManager.GetSysServer(sysServerId);
            if (sysServer != null && sysServer.UseLoadBalancer && !currentUrl.StartsWith(sysServer.Url))
                validSysServerUrl = sysServer.Url;

            return string.IsNullOrEmpty(validSysServerUrl);
        }

        public List<SysServer> GetSysServers()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysServer.ToList();
        }

        #endregion

        #region SysServerLogin

        public SysServerLogin GetSysServerLogin(Guid guid)
        {
using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
return GetSysServerLogin(sysEntitiesReadOnly, guid);
        }

        public SysServerLogin GetSysServerLogin(SOESysEntities entities, Guid guid)
        {
            return entities.SysServerLogin.FirstOrDefault(i => i.Guid == guid);
        }

        public byte[] GetPasswordFromSysServerLogin(Guid guid)
        {
            byte[] passwordhash = null;

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysServerLogin sysServerLogin = GetSysServerLogin(entities, guid);
                if (sysServerLogin != null)
                {
                    //Verify that SysServerLogin is not to old
                    DateTime boundaryDate = DateTime.Now.AddSeconds(-30);
                    if (sysServerLogin.Created > boundaryDate)
                    {
                        //Get password
                        passwordhash = sysServerLogin.passwordhash;

                        //Delete login
                        entities.SysServerLogin.Remove(sysServerLogin);
                        if (!SaveChanges(entities).Success)
                            passwordhash = null;
                    }
                }
            }

            return passwordhash;
        }

        public SysServerLogin AddSysServerLogin(string licenseNr, string loginName, byte[] passwordhash, SysServer sysServer)
        {
            SysServerLogin sysServerLogin = null;

            if (sysServer != null)
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    sysServerLogin = new SysServerLogin()
                    {
                        Guid = Guid.NewGuid(),
                        passwordhash = passwordhash,
                        Created = DateTime.Now,
                        CreatedBy = String.Format("{0} [{1}]", loginName, licenseNr),

                        //Set FK
                        SysServerId = sysServer.SysServerId,
                    };
                    entities.SysServerLogin.Add(sysServerLogin);

                    var result = SaveChanges(entities);
                    if (!result.Success)
                        sysServerLogin = null;
                }
            }

            return sysServerLogin;
        }

        #endregion

        #region Password

        public byte[] GetPasswordHash(string loginName, string password)
        {
            // We store loginname in lower case to make it case-insensitive
            loginName = loginName.ToLowerInvariant();
            byte[] data = Constants.ENCODING_IBM437.GetBytes(loginName + password);
            SHA256 shaM = new SHA256Managed();
            return shaM.ComputeHash(data);
        }

        public string GetRandomPassword()
        {
            string password = "";

            //Get numeric 4 digits randomnumber
            Random random = new Random();
            int num = random.Next(1000, 10000);

            //Get 6 char randomized
            for (int i = 0; i < 6; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                password += ch.ToString();
            }
            password = password.ToLower();
            password += num.ToString();

            return password;
        }

        #endregion

        #region Validate

        public string BlockedFromDateValidation(List<User> users, int actorCompanyId, int numberOfDays, List<Employee> employees = null)
        {
            StringBuilder sb = new StringBuilder();
            if (numberOfDays >= 0)
            {
                foreach (var user in users.Where(w => !w.BlockedFromDate.HasValue))
                {
                    Employee employee = null;
                    if (employees == null)
                        employee = EmployeeManager.GetEmployeeForUser(user.UserId, actorCompanyId, false, true);
                    else
                        employee = employees.FirstOrDefault(f => f.UserId == user.UserId && f.ActorCompanyId == actorCompanyId);

                    if (employee != null)
                    {
                        var info = SetBlockedFromDate(user, numberOfDays, employee);
                        if (!string.IsNullOrEmpty(info))
                            sb.AppendLine(info);
                    }
                }
            }
            return sb.ToString();
        }

        public void BlockedFromDateValidation(User user)
        {
            if (user != null && user.DefaultActorCompanyId.HasValue && !user.BlockedFromDate.HasValue)
            {
                int? numberOfDays = SettingManager.GetNullableIntSetting(SettingMainType.Company, (int)CompanySettingType.BlockFromDateOnUserAfterNrOfDays, 0, user.DefaultActorCompanyId.Value, 0).ToNullable();
                if (numberOfDays.HasValue)
                {
                    var employee = EmployeeManager.GetEmployeeForUser(user.UserId, user.DefaultActorCompanyId.Value, false, true);
                    if (employee != null)
                        SetBlockedFromDate(user, numberOfDays.Value, employee);
                }
            }
        }

        private string SetBlockedFromDate(User user, int numberOfDays, Employee employee)
        {
            if (employee != null && !employee.HasEmployment(DateTime.Today) && numberOfDays >= 0)
            {
                var lastEmploymentEndDate = employee.GetLastEmployment().GetEndDate();
                if (lastEmploymentEndDate.HasValue && lastEmploymentEndDate.Value < DateTime.Today)
                {
                    var blockedFromDate = lastEmploymentEndDate.Value.AddDays(numberOfDays + 1);

                    if (blockedFromDate <= DateTime.Today.AddDays(2) && blockedFromDate > DateTime.Today.AddYears(-1))
                    {
                        //Double check if user is on other company
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        if (entitiesReadOnly.Employee.Any(e => e.UserId == user.UserId && e.ActorCompanyId != employee.ActorCompanyId))
                            return string.Format("UserId {0} EmployeeNr {1} is connected to Employee on other Company, automatic blocking not possible on date ({2})", employee.EmployeeNr, user.UserId, blockedFromDate.ToShortDateString());

                        using (CompEntities entities = new CompEntities())
                        {
                            var updateUser = entities.User.First(f => f.UserId == user.UserId);
                            updateUser.BlockedFromDate = blockedFromDate;
                            SetModifiedProperties(updateUser);
                            SaveChanges(entities);
                            return string.Format("UserId {0} EmployeeNr {1} is blocked from {2}", user.UserId, employee.EmployeeNr, blockedFromDate.ToShortDateString());
                        }
                    }
                }
            }

            return string.Empty;
        }

        public User ValidateLoginSoftOneId(string licenseNr, string loginName, string password)
        {
            try
            {
                Guid guid = new Guid();
                User user = null;
                if (parameterObject != null && parameterObject.UserId != 0)
                    user = UserManager.GetUser(parameterObject.UserId, loadLicense: true);

                if (user == null)
                    user = UserManager.GetUser(licenseNr, loginName, true);

                if (user == null || loginName.Contains("@")) //Ugly temporary fix since we do not have any parameterObject from ApiExternal Logins
                {
                    var parsed = loginName.Split('@')[0];
                    user = UserManager.GetUser(licenseNr, parsed, true);
                }

                if (user == null)
                {
                    var userGuid = SoftOneIdConnector.GetIdLoginGuidUsingUsernameAndPassword(Guid.Parse(Constants.SoftOneStage), loginName, password);
                    if (userGuid.HasValue && userGuid.Value != new Guid())
                        user = UserManager.GetUser(userGuid.Value, includeLicense: true);

                    if (user != null) // SoftOneId only returns the guid if the credentials are correct
                        return user;
                }

                if (user == null || user.License == null)
                    return null;

                if (user.License.LicenseNr != licenseNr)
                    return null;

                if (user != null && user.idLoginGuid.HasValue)
                    guid = user.idLoginGuid.Value;

                if (string.IsNullOrEmpty(password))
                    return null;

                if (SoftOneIdConnector.ValidateLogin(user.idLoginGuid.Value, loginName, password))
                    return user;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
            return null;
        }

        #endregion
    }
}
