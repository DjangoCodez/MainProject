using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace SoftOne.Soe.Data
{
    public class ParameterObject
    {
        public int LicenseId => SoeCompany?.LicenseId ?? 0;

        private int? _actorCompanyId { get; set; }
        public int ActorCompanyId
        {
            get
            {
                if (!_actorCompanyId.ToNullable().HasValue && SoeCompany != null)
                {
                    _actorCompanyId = SoeCompany?.ActorCompanyId ?? 0;
                    if (_actorCompanyId == 0)
                        throw new Exception("ActorCompanyId is 0");
                }
                return _actorCompanyId ?? 0;
            }
            private set
            {
                _actorCompanyId = value;
            }
        }
        private CompanyDTO _soeCompany = null;
        public CompanyDTO SoeCompany
        {
            get
            {
                return _soeCompany;
            }
        }
        internal void SetActorCompanyId(int actorCompanyId)
        {
            ActorCompanyId = actorCompanyId;
            _soeCompany = null;
        }
        public void SetSoeCompany(CompanyDTO company)
        {
            _soeCompany = company;
            ActorCompanyId = company?.ActorCompanyId ?? 0;
        }

        private int? _userId { get; set; }
        public int UserId
        {
            get
            {
                return _userId ?? SoeUser?.UserId ?? 0;
            }
            private set
            {
                _userId = value;
            }
        }
        private UserDTO _soeUser = null;
        public UserDTO SoeUser
        {
            get
            {
                return _soeUser;
            }
        }
        internal void SetUserId(int userId)
        {
            UserId = userId;
            _soeUser = null;
        }
        public void SetSoeUser(UserDTO user)
        {
            _soeUser = user;
            UserId = user?.UserId ?? 0;
            _loginName = user?.LoginName ?? string.Empty;
            _email = user?.Email ?? string.Empty;
            _fullName = user?.Name ?? string.Empty;
        }

        public Guid? IdLoginGuid => SoeUser?.idLoginGuid;

        private string _email { get; set; }
        public string Email => _email ?? SoeUser?.Email ?? string.Empty;
        public void SetEmail(string email) => this._email = email;

        private string _loginName { get; set; }
        public string LoginName => _loginName ?? SoeUser?.LoginName ?? string.Empty;
        public void SetUserName(string loginName) => this._loginName = loginName;

        private string _fullName { get; set; }
        public string FullName => _fullName ?? SoeUser?.Name ?? string.Empty;
        public void SetFullName(string fullName) => this._fullName = fullName;

        public int? SupportActorCompanyId { get; private set; }
        private CompanyDTO _soeSupportCompany = null;
        public CompanyDTO SoeSupportCompany
        {
            get
            {
                return _soeSupportCompany;
            }
        }
        internal void SetSoeSupportCompany(CompanyDTO company)
        {
            _soeSupportCompany = company;
            SupportActorCompanyId = company?.ActorCompanyId;
        }

        public Guid? CompanyGuid => SoeCompany?.GetCompanyGuid();
        public Guid? LicenseGuid => SoeCompany?.GetLicenseGuid();

        public bool IsSupportLoggedInByCompany { get; private set; }
        internal void SetIsSupportLoggedInByCompany(bool value) => IsSupportLoggedInByCompany = value;

        public int? SupportUserId { get; private set; }
        private UserDTO _soeSupportUser = null;
        public UserDTO SoeSupportUser
        {
            get
            {
                return _soeSupportUser;
            }
        }
        internal void SetSoeSupportUser(UserDTO user)
        {
            _soeSupportUser = user;
            SupportUserId = user?.UserId;
            SupportActiveRoleId = user?.DefaultRoleId;
        }

        private int? activeRoleId;
        public void SetActiveRoleId(int? roleId) => activeRoleId = roleId;
        public int RoleId => activeRoleId ?? SoeUser?.DefaultRoleId ?? 0;

        public bool IsSupportLoggedInByUser { get; private set; }
        internal void SetIsSupportLoggedInByUser(bool value) => IsSupportLoggedInByUser = value;

        public bool IsSupportLoggedIn => IsSupportLoggedInByCompany || IsSupportLoggedInByUser;
        public bool HasSupportUserAndCompany => SupportUserId > 0 && SupportActorCompanyId > 0;

        public int? SupportActiveRoleId { get; private set; }

        public bool IsSuperAdminMode { get; private set; }
        internal void SetIsSuperAdminMode(bool value) => IsSuperAdminMode = value;

        public string Thread { get; private set; }
        public void SetThread(string thread) => Thread = thread;

        public bool IncludeInactiveAccounts { get; private set; }
        public void SetIncludeInactiveAccounts(bool value)
        {
            IncludeInactiveAccounts = value;
        }

        public ExtendedUserParams ExtendedUserParams { get; private set; } = new ExtendedUserParams();
        public void SetExtendedUserParams(ExtendedUserParams parameters) => ExtendedUserParams = parameters;

        protected ParameterObject() { }

        public static ParameterObject Empty() => new ParameterObject();
        public ParameterObject Clone(
            int? actorCompanyId = null,
            int? activeRoleId = null,
            string thread = null)
        {
            var param = new ParameterObject
            {
                ActorCompanyId = actorCompanyId ?? ActorCompanyId,
                UserId = UserId,
                activeRoleId = activeRoleId ?? this.activeRoleId,

                SupportActorCompanyId = SupportActorCompanyId,
                _soeSupportCompany = _soeSupportCompany,
                IsSupportLoggedInByCompany = IsSupportLoggedInByCompany,

                SupportUserId = SupportUserId,
                _soeSupportUser = _soeSupportUser,
                IsSupportLoggedInByUser = IsSupportLoggedInByUser,

                IsSuperAdminMode = IsSuperAdminMode,
                IncludeInactiveAccounts = IncludeInactiveAccounts,
                Thread = thread,
            };

            param.SetSoeUser(_soeUser);
            param.SetSoeCompany(_soeCompany);

            return param;
        }

        public static ParameterObject Create(
            CompanyDTO company,
            UserDTO user,
            CompanyDTO supportCompany,
            UserDTO supportUser,
            int activeRoleId,
            bool isSupportLoggedInByCompany,
            bool isSupportLoggedInByUser,
            bool isSuperAdminMode,
            bool includeInactiveAccounts)
        {
            var param = new ParameterObject();
            param.SetSoeCompany(company);
            param.SetSoeUser(user);
            param.SetActiveRoleId(activeRoleId.ToNullable());
            param.SetSoeSupportCompany(supportCompany);
            param.SetSoeSupportUser(supportUser);
            param.SetIsSupportLoggedInByCompany(isSupportLoggedInByCompany);
            param.SetIsSupportLoggedInByUser(isSupportLoggedInByUser);
            param.SetIsSuperAdminMode(isSuperAdminMode);
            param.SetIncludeInactiveAccounts(includeInactiveAccounts);

            return param;
        }

        public static ParameterObject Create(
            UserDTO user = null,
            CompanyDTO company = null,
            string thread = null,
            int? roleId = null,
            ExtendedUserParams extendedUserParams = null)
        {
            var param = new ParameterObject
            {
                activeRoleId = roleId,
                Thread = thread,
                ExtendedUserParams = extendedUserParams
            };

            param.SetSoeCompany(company);
            param.SetSoeUser(user);
            return param;
        }

        public void SignInSupport(
            UserDTO remoteUser,
            CompanyDTO remoteCompany,
            bool isSuperAdminMode,
            bool isSupportLoggedInByCompany,
            bool isSupportLoggedInByUser,
            int activeRoleId,
            bool persist = true)
        {
            //Move current user/company to SoeSupportUser/SoeSupportCompany
            SetSoeSupportCompany(SoeCompany);
            SetSoeSupportUser(SoeUser);
            SetIsSuperAdminMode(isSuperAdminMode);

            //Set new user/company as SoeUser/SoeCompany
            SetSoeUser(remoteUser);
            SetSoeCompany(remoteCompany);
            SetActiveRoleId(activeRoleId);
            SetIsSupportLoggedInByCompany(isSupportLoggedInByCompany);
            SetIsSupportLoggedInByUser(isSupportLoggedInByUser);

            if (persist)
            {
                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                if (authenticationManager.User.Identity is ClaimsIdentity currentIdentity && currentIdentity.IsAuthenticated)
                {
                    var newIdentity = new ClaimsIdentity(currentIdentity);
                    newIdentity.UpsertClaim(SoeClaimType.LicenseId, remoteCompany.LicenseId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.ActorCompanyId, remoteCompany.ActorCompanyId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.UserId, remoteUser.UserId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.UserName, remoteUser.LoginName.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.RoleId, RoleId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.SupportActorCompanyId, SupportActorCompanyId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.SupportUserId, SupportUserId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.SupportActiveRoleId, SupportActiveRoleId.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.IsSupportLoggedInByCompany, IsSupportLoggedInByCompany.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.IsSupportLoggedInByUser, IsSupportLoggedInByUser.ToString());
                    newIdentity.UpsertClaim(SoeClaimType.IsSuperAdminMode, IsSuperAdminMode.ToString());

                    authenticationManager.SignIn(newIdentity);
                }
            }
        }

        public void SignOutSupport()
        {
            //Set varibbles from supportUser/SupportCompany
            int licenseId = SoeSupportCompany.LicenseId;
            int actorCompanyId = SoeSupportCompany.ActorCompanyId;
            int userId = SoeSupportUser.UserId;
            string userName = SoeSupportUser.LoginName;
            int roleId = SupportActiveRoleId ?? 0;
            if (roleId == 0)
                using (CompEntities entities = new CompEntities())
                    roleId = entities.User.FirstOrDefault(x => x.UserId == userId)?.DefaultRoleId ?? 0;

            //Move previous user/company to SoeUser/SoeCompany
            SetSoeUser(SoeSupportUser);
            SetUserId(SupportUserId.Value);
            SetSoeCompany(SoeSupportCompany);
            SetActorCompanyId(SupportActorCompanyId.Value);
            SetActiveRoleId(roleId);

            //Clear SoeSupportUser/SoeSupportCompany
            SetSoeSupportUser(null);
            SetSoeSupportCompany(null);
            SetIsSupportLoggedInByCompany(false);
            SetIsSupportLoggedInByUser(false);

            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            if (authenticationManager.User.Identity is ClaimsIdentity currentIdentity && currentIdentity.IsAuthenticated)
            {
                var newIdentity = new ClaimsIdentity(currentIdentity);
                newIdentity.UpsertClaim(SoeClaimType.LicenseId, licenseId.ToString());
                newIdentity.UpsertClaim(SoeClaimType.ActorCompanyId, actorCompanyId.ToString());
                newIdentity.UpsertClaim(SoeClaimType.UserId, userId.ToString());
                newIdentity.UpsertClaim(SoeClaimType.UserName, userName);
                newIdentity.UpsertClaim(SoeClaimType.RoleId, roleId.ToString());
                newIdentity.SafeRemoveClaim(SoeClaimType.SupportActorCompanyId);
                newIdentity.SafeRemoveClaim(SoeClaimType.SupportUserId);
                newIdentity.SafeRemoveClaim(SoeClaimType.IsSupportLoggedInByCompany);
                newIdentity.SafeRemoveClaim(SoeClaimType.IsSupportLoggedInByUser);
                newIdentity.SafeRemoveClaim(SoeClaimType.IsSuperAdminMode);
                newIdentity.SafeRemoveClaim(SoeClaimType.IncludeInactiveAccounts);

                authenticationManager.SignIn(newIdentity);
            }
        }
    }

    public class LazyParameterObject : ParameterObject
    {
        protected LazyParameterObject()
        {

        }

        public static ParameterObject Create(
            ClaimsIdentity identity,
            CompanyDTO company,
            UserDTO user,
            CompanyDTO supportCompany,
            UserDTO supportUser)
        {
            if (identity == null || !identity.IsAuthenticated)
                return null;

            var ret = new LazyParameterObject();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    #region Read required 

                    Claim claimUserId = identity.FindFirst(SoeClaimType.UserId);
                    if (!claimUserId.TryGetInt(out _))
                        return null;

                    Claim claimRoleId = identity.FindFirst(SoeClaimType.RoleId) ?? new Claim(SoeClaimType.RoleId, (ret.RoleId.ToNullable()?.ToString() ?? user?.DefaultRoleId.ToNullable()?.ToString() ?? ""));
                    if (!claimRoleId.TryGetInt(out int roleId))
                        return null;

                    #endregion

                    #region Synch to ParmeterObject

                    ret.SetSoeCompany(company);
                    ret.SetSoeUser(user);
                    ret.SetActiveRoleId(roleId.ToNullable());

                    //SupportActorCompanyId
                    if (supportCompany != null)
                        ret.SetSoeSupportCompany(supportCompany);
                    else
                        ret.SetSoeSupportCompany(null);

                    // IsSupportLoggedInByCompany
                    if (identity.TryGetBool(SoeClaimType.IsSupportLoggedInByCompany, out bool isSupportLoggedInByCompany))
                        ret.SetIsSupportLoggedInByCompany(isSupportLoggedInByCompany);
                    else
                        ret.SetIsSupportLoggedInByCompany(false);

                    //SupportUserId
                    if (supportUser != null)
                        ret.SetSoeSupportUser(supportUser);
                    else
                        ret.SetSoeSupportUser(null);

                    // IsSupportLoggedInByUser
                    if (identity.TryGetBool(SoeClaimType.IsSupportLoggedInByUser, out bool isSupportLoggedInByUser))
                        ret.SetIsSupportLoggedInByUser(isSupportLoggedInByUser);
                    else
                        ret.SetIsSupportLoggedInByUser(false);

                    //SuperAdmin
                    if (identity.TryGetBool(SoeClaimType.IsSuperAdminMode, out bool isSuperAdminMode))
                        ret.SetIsSuperAdminMode(isSuperAdminMode);
                    else
                        ret.SetIsSuperAdminMode(false);

                    //SuperAdmin
                    if (identity.TryGetBool(SoeClaimType.IncludeInactiveAccounts, out bool includeInactiveAccounts))
                        ret.SetIncludeInactiveAccounts(includeInactiveAccounts);
                    else
                        ret.SetIncludeInactiveAccounts(false);

                    ret.SetExtendedUserParams(ExtendedUserParams.Create(GetCurrentDirectory(), GetClientIP(), GetRequestUrl(), GetUserEnvironmentInfo()));

                    #endregion
                }
            }
            catch 
            {
                return ret;
            }

            return ret;
        }

        private static string GetCurrentDirectory()
        {
            return HttpContext.Current.Server.MapPath("~");
        }
        private static string GetUserEnvironmentInfo()
        {
            return GetHostInfo() + Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR + GetClientIP();
        }
        private static string GetHostIP()
        {
            string ipNr = "";

            IPHostEntry hostEntry = Dns.GetHostEntry(GetHostName());
            if (hostEntry != null)
            {
                IPAddress[] ipHostEntry = hostEntry.AddressList;
                ipNr = ipHostEntry[ipHostEntry.Length - 1].ToString();
            }
            return ipNr;
        }
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }
        public static string GetHostInfo()
        {
            return $"{GetHostIP()}_{GetHostName()}";
        }
        protected static string GetClientIP()
        {
            string ipNr = "";

            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    ipNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipNr))
                        ipNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch
            {
                //Continue on error on IP
            }

            return ipNr;
        }
        private static string GetRequestUrl()
        {
            string url = String.Empty;
            if (HttpContext.Current?.Request?.Url != null)
                HttpContext.Current.Request.Url.ToString();
            return url;
        }
    }

    public class ExtendedUserParams
    {
        public string CurrentDirectory { get; set; }
        public string IpAddress { get; set; }
        public string Request { get; set; }
        public bool FromCore { get; set; }
        public string UserEnvironmentInfo { get; set; }

        public static ExtendedUserParams Create(string currentDirectory, string ipAdress, string request, string userEnvironmentInfo = "", bool fromCore = false)
        {
            return new ExtendedUserParams
            {
                CurrentDirectory = currentDirectory,
                IpAddress = ipAdress,
                Request = request,
                UserEnvironmentInfo = userEnvironmentInfo,
                FromCore = fromCore,
            };
        }
    }

    public static class ParameterObjectExtensions
    {
        public static void UpsertClaim(this ClaimsIdentity identity, string claimType, string value)
        {
            var existingClaim = identity.FindFirst(claimType);
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }
            identity.AddClaim(new Claim(claimType, value));
        }
        public static void SafeRemoveClaim(this ClaimsIdentity identity, string claimType)
        {
            var existingClaim = identity.FindFirst(claimType);
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }
        }
    }
}
