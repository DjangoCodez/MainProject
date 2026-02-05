using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO.SoftOneId;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace SoftOne.Soe.Web.Security
{
    public class LoginHelper
    {
        private readonly bool _loadBalancingEnabled;
        private readonly UserManager _userManager;
        private readonly CompanyManager _companyManager;
        private readonly LoginManager _loginManager;
        private readonly SysLogManager _log;
        private readonly IClaimsHelper _claimsHelper;

        public ParameterClaimsObjectDTO ParameterClaims { get; private set; }
        public User User { get; private set; }
        public Company Company { get; private set; }
        public bool IsAuthorizedLogin { get; private set; }

        public LoginHelper(bool loadBalancingEnabled, UserManager userManager, CompanyManager companyManager, LoginManager loginManager, SysLogManager log, IClaimsHelper claimsHelper)
        {
            this._loadBalancingEnabled = loadBalancingEnabled;
            this._userManager = userManager;
            this._companyManager = companyManager;
            this._loginManager = loginManager;
            this._log = log;
            this._claimsHelper = claimsHelper;
        }

        public ClaimsIdentity GetIdentity()
        {
            return _claimsHelper.GetIdentity(true,
                User.LicenseId,
                User.License.LicenseGuid,
                ParameterClaims.ActorCompanyId,
                ParameterClaims.RoleId,
                ParameterClaims.UserId,
                User.idLoginGuid,
                ParameterClaims.UserName,
                null,
                ParameterClaims.SoeSupportUserId,
                ParameterClaims.IsSuperAdminMode,
                ParameterClaims.IsSupportLoggedInByCompany,
                ParameterClaims.IsSupportLoggedInByUser);
        }

        public static string GetLicenseIdCookieKey(int licenseId)
        {
            return DateTime.Now.ToString() + "#" + licenseId.ToString();
        }

        public static Guid? GetIdLoginGuid()
        {
            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return null;

                if (identity.HasClaim(c => c.Type == SoeClaimType.UserGuid))
                {
                    var idLoginGuid = identity.FindFirst(SoeClaimType.UserGuid);
                    if (idLoginGuid?.Value != null)
                        return Guid.Parse(idLoginGuid.Value);
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return null;
        }

        public static Guid? GetLicenseGuidFromClaims()
        {
            var choosenLicenseGuid = GetChoosenLicenseGuidFromClaims();
            var claimLicenseGuid = (Guid?)null;
            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return null;
                if (identity.HasClaim(c => c.Type == SoeClaimType.LicenseGuid))
                {
                    var licenseGuid = identity.FindFirst(SoeClaimType.LicenseGuid);
                    if (licenseGuid?.Value != null)
                        claimLicenseGuid = Guid.Parse(licenseGuid.Value);
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogInfo("Error getting licenseguid from claims " + ex.ToString());
                return Guid.Empty;
            }

            if (choosenLicenseGuid != null && claimLicenseGuid != null && choosenLicenseGuid != claimLicenseGuid)
            {
                LogCollector.LogInfo($"ChoosenLicenseGuid is different from claimLicenseGuid. ChoosenLicenseGuid:{choosenLicenseGuid} ClaimLicenseGuid:{claimLicenseGuid}");
                return choosenLicenseGuid;
            }

            return claimLicenseGuid;
        }

        private static Guid? GetChoosenLicenseGuidFromClaims()
        {
            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return Guid.Empty;

                var choosenLicenseId = identity.FindFirst(SoeClaimType.ChoosenLicenseId)?.Value;
                if (choosenLicenseId == null)
                    return null;

                var parts = choosenLicenseId.Split('#');
                var createdPart = parts.Length >= 2 ? parts[1] : null;

                if (createdPart != null && DateTime.TryParse(createdPart, out DateTime created) && created.AddMinutes(1) < DateTime.UtcNow)
                {
                    LogCollector.LogInfo($"ChoosenLicenseId is expired now:{DateTime.UtcNow} time:{createdPart} claim:{choosenLicenseId}");
                    return null;
                }

                if (parts.Length > 2 && Guid.TryParse(parts[2], out Guid licGuid) && licGuid != Guid.Empty)
                    return licGuid;

                return null;
            }
            catch (Exception ex)
            {
                LogCollector.LogInfo("Error getting ChoosenLicenseId from claims " + ex.ToString());
                return null;
            }
        }

        public static int? GetLicenseIdFromClaims()
        {
            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return null;

                if (identity.HasClaim(c => c.Type == SoeClaimType.ChoosenLicenseId))
                {
                    var licenseIdString = identity.FindFirst(SoeClaimType.ChoosenLicenseId);
                    if (licenseIdString != null)
                    {
                        var arr = licenseIdString.ToString().Replace(SoeClaimType.ChoosenLicenseId, "").Replace(": ", "").Trim().Split('#');
                        if (int.TryParse(arr[0], out int choosenLicenseId))
                        {
                            if (arr.Count() == 1)
                                return choosenLicenseId;

                            if (arr.Count() > 1 && DateTime.TryParse(arr[1], out DateTime created) && created > DateTime.UtcNow.AddSeconds(-90))
                                return choosenLicenseId;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogCollector.LogInfo("Error getting license id from claims " + ex.ToString());
                return null;
            }
        }

        public static int GetLicenseIdFromCookie(string fromCookie)
        {
            if (string.IsNullOrEmpty(fromCookie) || !fromCookie.Contains("#"))
                return 0;

            var arr = fromCookie.Split('#');
            if (arr.Count() == 2)
            {
                DateTime.TryParse(arr[0], out DateTime date);
                if (date.AddMinutes(2) > DateTime.Now)
                {
                    int.TryParse(arr[1], out int licenseId);
                    return licenseId;
                }
            }
            return 0;
        }

        public static int? GetUserId()
        {
            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return null;

                if (identity.HasClaim(c => c.Type == SoeClaimType.UserId))
                {
                    var userIdString = identity.FindFirst(SoeClaimType.UserId);
                    if (userIdString != null && int.TryParse(userIdString.Value, out int userid))
                        return userid;
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return null;
        }

        public string GetReturnUrl(string requestedReturnUrl)
        {
            var returnUrl = "/";
            if (!string.IsNullOrEmpty(requestedReturnUrl) && !requestedReturnUrl.Contains("logout.aspx"))
            {
                returnUrl = "soe/?c=" + ParameterClaims.ActorCompanyId + "&cd=1";
                var defaultUrl = SettingCacheManager.Instance.GetFavoriteUrl(ParameterClaims.UserId);
                if (!string.IsNullOrEmpty(defaultUrl))
                    returnUrl = defaultUrl;
            }
            return returnUrl;
        }

        public bool TryAuthenticateUser(ClaimsIdentity identity, int licenseId, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (identity == null)
                return false;

            Guid.TryParse(identity.FindFirst(SoeClaimType.UserGuid)?.Value, out Guid userIdGuid);
            if (userIdGuid == Guid.Empty)
            {
                errorMessage = ($"TryAuthenticateUser  userIdGuid {userIdGuid} return false");
                return false;
            }
            ParameterClaims = _userManager.GetParameterClaimsObjectDTO(userIdGuid, out User user, licenseId);
            if (ParameterClaims == null)
            {
                var paraminfo = ParameterClaims != null ? JsonConvert.SerializeObject(ParameterClaims) : "null";
                errorMessage = ($"TryAuthenticateUser  ParameterClaims {paraminfo} return false");
                return false;
            }

            if (user == null)
            {
                errorMessage = ($"TryAuthenticateUser  user is null return false userIdGuid:{userIdGuid}");
                return false;
            }

            User = user;
            Company = _companyManager.GetCompany(ParameterClaims.ActorCompanyId, loadLicense: true);
            IsAuthorizedLogin = EnsureIsAuthorizedLogin();

            return true;
        }

        public bool ShouldRedirectForLoadBalancing(out string redirectUrl)
        {
            if (UrlUtil.HasXForwardedHeaders(HttpContext.Current.Request))
            {
                redirectUrl = null;
                return false;
            }

            redirectUrl = null;

            if (HttpContext.Current == null || !_loadBalancingEnabled || Company.License.SysServerId == null || Company.License.SysServerId == 0)
                return false;

            string currentUrl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
            if (currentUrl.Contains("localhost"))
                return false;

            //Do not use load balancer if SysServer not found or load balancing turned off on SysServer
            var sysServer = _loginManager.GetSysServer(User.License.LicenseNr);
            if (sysServer == null || !sysServer.UseLoadBalancer || currentUrl.Contains(sysServer.Url.ToLower()))
                return false;

            redirectUrl = sysServer.Url;

            return SoftOneStatusConnector.IsServerLive(StringUtility.GetSubDomainFromUrl(redirectUrl));
        }

        private bool EnsureIsAuthorizedLogin()
        {
            if (User == null || Company == null || User.State != (int)SoeEntityState.Active || Company.State != (int)SoeEntityState.Active)
            {
                var errorLogRecord = "User is unauthorized - ";
                errorLogRecord += User == null ? " user is null " : "";
                errorLogRecord += Company == null ? " company is null " : "";
                if (User != null)
                    errorLogRecord += " - user.State is " + User.State.ToString();
                if (Company != null)
                    errorLogRecord += " - company.State is " + Company.State.ToString();
                _log.LogInfo(errorLogRecord);
                return false;
            }
            return true;
        }
    }
}