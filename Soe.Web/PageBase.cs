using Microsoft.Owin.Security;
using Ninject;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Security;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.Security;
using SoftOne.Soe.Web.Services;
using SoftOne.Soe.Web.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI;

namespace SoftOne.Soe.Web
{
    /// <summary>
    /// Base class that provides base functionality for SOE ASP.NET pages.
    /// </summary>
    public abstract class PageBase : Page
    {
        #region Lazy loaders

        private MasterPageBase masterPageBase = null;
        protected MasterPageBase MasterPageBase
        {
            get
            {
                if (masterPageBase == null)
                    masterPageBase = Master as MasterPageBase;
                return masterPageBase;
            }
        }

        private AccountManager accountManager;
        protected AccountManager AccountManager
        {
            get
            {
                return accountManager ?? (accountManager = new AccountManager(ParameterObject));
            }
        }
        private CompanyManager companyManager;
        protected CompanyManager CompanyManager
        {
            get
            {
                return companyManager ?? (companyManager = new CompanyManager(ParameterObject));
            }
        }
        private CommentManager commentManager;
        protected CommentManager CommentManager
        {
            get
            {
                return commentManager ?? (commentManager = new CommentManager(ParameterObject));
            }
        }
        private EmployeeManager employeeManager;
        protected EmployeeManager EmployeeManager
        {
            get
            {
                return employeeManager ?? (employeeManager = new EmployeeManager(ParameterObject));
            }
        }
        private FeatureManager featureManager;
        protected FeatureManager FeatureManager
        {
            get
            {
                return featureManager ?? (featureManager = new FeatureManager(ParameterObject));
            }
        }
        private GeneralManager generalManager;
        protected GeneralManager GeneralManager
        {
            get
            {
                return generalManager ?? (generalManager = new GeneralManager(ParameterObject));
            }
        }
        private LanguageManager languageManager;
        protected LanguageManager LanguageManager
        {
            get
            {
                return languageManager ?? (languageManager = new LanguageManager(ParameterObject));
            }
        }
        private LoginManager loginManager;
        protected LoginManager LoginManager
        {
            get
            {
                return loginManager ?? (loginManager = new LoginManager(ParameterObject));
            }
        }

        private LicenseManager licenseManager;
        protected LicenseManager LicenseManager
        {
            get
            {
                return licenseManager ?? (licenseManager = new LicenseManager(ParameterObject));
            }
        }

        private SettingManager settingManager;
        protected SettingManager SettingManager
        {
            get
            {
                return settingManager ?? (settingManager = new SettingManager(ParameterObject));
            }
        }
        private SysLogManager sysLogManager;
        protected SysLogManager SysLogManager
        {
            get
            {
                return sysLogManager ?? (sysLogManager = new SysLogManager(ParameterObject));
            }
        }
        private UserManager userManager;
        protected UserManager UserManager
        {
            get
            {
                return userManager ?? (userManager = new UserManager(ParameterObject));
            }
        }

        #endregion

        #region Core members

        private ParameterObject _parameterObject;
        public ParameterObject ParameterObject
        {
            get
            {
                try
                {
                    EnsureParameterObject();
                    return this._parameterObject;
                }
                catch (Exception ex)
                {
                    LogCollector.LogError(ex, "_parameterObject(set) failed");
                    return null;
                }
            }
        }
        public License SoeLicense
        {
            get
            {
                return SoeUser != null ? LicenseCacheManager.Instance.GetLicense(SoeUser.LicenseNr) : null;
            }
        }
        public CompanyDTO SoeCompany
        {
            get
            {
                return ParameterObject?.SoeCompany;
            }
        }
        public UserDTO SoeUser
        {
            get
            {
                return ParameterObject?.SoeUser;
            }
        }

        public int UserId
        {
            get
            {
                var userId = SoeUser?.UserId;
                if (userId.HasValue)
                    return userId.Value;

                userId = ClaimsHelper.GetIntClaim(Context, SoeClaimType.UserId);
                if (!userId.HasValue)
                {
                    var userGuid = ClaimsHelper.GetGuidClaim(Context, SoeClaimType.UserGuid);
                    if (userGuid.HasValue)
                    {
                        var user = UserManager.GetUser(userGuid.Value, includeLicense: true);
                        if (user != null)
                        {
                            ParameterObject?.SetSoeUser(user.ToDTO());
                            return user.UserId;
                        }
                    }
                }

                var companyId = ClaimsHelper.GetIntClaim(Context, SoeClaimType.ActorCompanyId);

                if (userId.HasValue && userId.Value > 0)
                    ParameterObject?.SetSoeUser(SessionCache.GetUserFromCache(userId.Value, companyId ?? 0));

                return userId ?? 0;
            }
        }
        public int RoleId
        {
            get
            {
                var roleid = ParameterObject?.RoleId;
                if (roleid.HasValue)
                    return roleid.Value;

                roleid = ClaimsHelper.GetIntClaim(Context, SoeClaimType.RoleId);
                if (roleid.HasValue)
                    ParameterObject.SetActiveRoleId(roleid.Value);

                return roleid ?? 0;
            }
        }

        public CompanyDTO SoeSupportCompany
        {
            get
            {
                return ParameterObject?.SoeSupportCompany;
            }
        }
        public UserDTO SoeSupportUser
        {
            get
            {
                return ParameterObject?.SoeSupportUser;
            }
        }
        public int? SoeActorCompanyId
        {
            get
            {
                return ParameterObject?.ActorCompanyId;
            }
        }
        public int? SoeUserId
        {
            get
            {
                return ParameterObject?.UserId;
            }
        }
        public int? SoeSupportActorCompanyId
        {
            get
            {
                return ParameterObject?.SupportActorCompanyId;
            }
        }
        public int? SoeSupportUserId
        {
            get
            {
                return ParameterObject?.SupportUserId;
            }
        }
        protected bool IsSupportLoggedInByClaims()
        {
            int? supportActorCompanyId = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.SupportActorCompanyId);
            int? supportUserId = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.SupportUserId);
            bool? isSupportLoggedInByCompany = ClaimsHelper.GetBoolClaim(this.Context, SoeClaimType.IsSupportLoggedInByCompany);
            bool? isSupportLoggedInByUser = ClaimsHelper.GetBoolClaim(this.Context, SoeClaimType.IsSupportLoggedInByUser);
            return supportActorCompanyId.HasValue && supportUserId.HasValue && (isSupportLoggedInByCompany == true || isSupportLoggedInByUser == true);
        }

        //Page settings
        public string PageTitle { get; set; }
        public string Language { get; private set; }
        public List<string> Icons { get; set; } = new List<string>();
        public List<string> StyleSheets { get; set; } = new List<string>();
        public List<string> Scripts { get; set; } = new List<string>();
        private bool? relaseMode;
        public bool ReleaseMode
        {
            get
            {
                if (!relaseMode.HasValue)
                    relaseMode = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
                return relaseMode.Value;
            }
        }
        private bool? enableUserSession;
        public bool EnableUserSession
        {
            get
            {
                if (!enableUserSession.HasValue)
                    enableUserSession = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_ENABLEUSERSESSION));
                return enableUserSession.Value;
            }
        }

        // Angular host
        public bool HasAngularHost { get; set; }

        // Angular SPA host
        public bool HasAngularSpaHost { get; set; }

        public bool UseAngularSpa
        {
            get
            {
                if (!HasAngularSpaHost)
                    return false;

                if (HasAngularSpaHost && !HasAngularHost)
                    return true;

                // Angular SPA
                bool? useAngularSpa = null;
                if (!string.IsNullOrEmpty(QS["spa"]))
                {
                    bool.TryParse(QS["spa"], out bool forceAngularSpa);
                    if (forceAngularSpa)
                        useAngularSpa = true;
                }

                // AngularJS
                if (!string.IsNullOrEmpty(QS["ng"]))
                {
                    bool.TryParse(QS["ng"], out bool forceAngularJs);
                    if (forceAngularJs)
                        useAngularSpa = false;
                }

                if (!useAngularSpa.HasValue)
                    useAngularSpa = this.CanUseAngularSpa;
                return useAngularSpa.Value;
            }
        }
        private readonly Dictionary<Feature, bool> featureShowAngularSpaIconDict = new Dictionary<Feature, bool>();
        public bool ShowAngularSpaIcon
        {
            get
            {
                if (!this.HasAngularSpaHost)
                    return false;

                if (!featureShowAngularSpaIconDict.ContainsKey(this.Feature))
                    this.featureShowAngularSpaIconDict.Add(this.Feature, ShowAngularSpa(false));
                return featureShowAngularSpaIconDict[this.Feature];
            }
        }
        private readonly Dictionary<Feature, bool> featureCanUseAngularSpaDict = new Dictionary<Feature, bool>();
        public bool CanUseAngularSpa
        {
            get
            {
                if (!featureCanUseAngularSpaDict.ContainsKey(this.Feature))
                    this.featureCanUseAngularSpaDict.Add(this.Feature, ShowAngularSpa(true));
                return featureCanUseAngularSpaDict[this.Feature];
            }
        }
        private bool ShowAngularSpa(bool currentPage)
        {
            return this.Feature == Feature.None ? false : GeneralManager.IsAngularSpaValid(this.Feature, this.SiteType, currentPage);
        }

        public bool ShowLogoInTopMenu
        {
            get
            {
                if (Request.Url.AbsolutePath == "/soe/default.aspx")
                    return true;

                return false;
            }
        }

        // Branding
        private string _brandingCompanyClass = string.Empty;
        public string BrandingCompanyClass
        {
            get
            {
                if (_brandingCompanyClass.IsNullOrEmpty())
                {
                    TermGroup_BrandingCompanies brandingCompany = (TermGroup_BrandingCompanies)SettingManager.GetIntSetting(SettingMainType.License, (int)LicenseSettingType.BrandingCompany, 0, 0, this.SoeLicense.LicenseId);
                    if (brandingCompany == TermGroup_BrandingCompanies.FlexiblaKontoret)
                        _brandingCompanyClass = "brand-flexibla-kontoret";
                    else
                        _brandingCompanyClass = "brand-softone";
                }

                return _brandingCompanyClass;
            }
        }
        private string _brandingCompanyLogo = string.Empty;
        public string BrandingCompanyLogo
        {
            get
            {
                if (_brandingCompanyLogo.IsNullOrEmpty())
                {
                    TermGroup_BrandingCompanies brandingCompany = (TermGroup_BrandingCompanies)SettingManager.GetIntSetting(SettingMainType.License, (int)LicenseSettingType.BrandingCompany, 0, 0, this.SoeLicense.LicenseId);
                    if (brandingCompany == TermGroup_BrandingCompanies.FlexiblaKontoret)
                        _brandingCompanyLogo = "/img/logo_fk.svg";
                    else
                        _brandingCompanyLogo = "/img/logo_go_white.svg";
                }

                return _brandingCompanyLogo;
            }
        }

        //Sitetype
        private TermGroup_SysPageStatusSiteType SiteType
        {
            get
            {
                return CompDbCache.Instance.SiteType;
            }
        }
        public bool CanUseAngularJs
        {
            get
            {
                return GeneralManager.CanShowAngularJsPage(this.Feature, this.SiteType);
            }
        }
        public bool AngularJsFirst
        {
            get
            {
                return GeneralManager.ShowAngularJsFirst(this.Feature, this.SiteType);
            }
        }
        public bool ShowPageStatusBetaSelector
        {
            get
            {
#if DEBUG
                return HasAngularSpaHost && HasAngularHost;
#else
                return HasAngularSpaHost && HasAngularHost && IsSupportAdmin;
#endif
            }
        }
        public bool ShowPageStatusLiveSelector
        {
            get
            {
                return HasAngularSpaHost && HasAngularHost && IsSupportAdmin;
            }
        }

        // Font Awesome
        private FontAwesomeIconSource? fontAwesomeSource;
        public FontAwesomeIconSource FontAwesomeSource
        {
            get
            {
                fontAwesomeSource = FontAwesomeIconSource.SoftOne_Embedded;
                //if (!fontAwesomeSource.HasValue)
                //    fontAwesomeSource = (FontAwesomeIconSource)SettingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.FontAwesomeSource, 0, 0, 0);
                return fontAwesomeSource.Value;
            }
        }

        //Bootstrap
        public bool HasBootstrapMenu { get; set; }
        public bool UseBootstrapMenu
        {
            // TODO: Remove setting UseBootstrapMenu on all levels (App, Company, User)
            get { return true; }
        }
        private bool shouldLoadOidcClientScripts = true;
        public bool ShouldLoadOidcClientScripts
        {
            get { return shouldLoadOidcClientScripts; }
            set { shouldLoadOidcClientScripts = value; }
        }

        //AccountHierarchy
        private bool? useAccountHierarchy = null;
        public bool UseAccountHierarchy()
        {
            if (useAccountHierarchy.HasValue)
                return useAccountHierarchy.Value;
            if (SoeCompany != null)
                useAccountHierarchy = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, SoeCompany.ActorCompanyId, 0);

            return useAccountHierarchy == true;
        }

        #endregion

        #region Events

        protected override void OnPreInit(EventArgs e)
        {
            if (string.IsNullOrEmpty(this.MasterPageFile) && !this.IsAddOnUrl)
            {
                this.HasBootstrapMenu = true;
                this.MasterPageFile = "~/soe/bootstrap.master";
            }

            var owinContext = HttpContext.Current?.GetOwinContext();
            if (owinContext != null)
            {
                if (!TimeOutOwinHelper.TryToSlideTimeoutForward(owinContext))
                {
                    if (owinContext.Authentication.User.Identity.IsAuthenticated)
                    {
                        RedirectToLogin();
                    }
                    else
                    {
                        ClearDataFromSession();
                        RedirectToLogout();
                    }
                }
                else if (
                    !Request.Url.ToString().Contains("/setcompany") &&
                    !Request.Url.ToString().Contains("/setrole") &&
                    owinContext.Authentication.User.Identity.IsAuthenticated &&
                    TryGetActorCompanyIdFromQS(out int actorCompanyIdFromQS) && actorCompanyIdFromQS != ClaimsHelper.GetIntClaim(Context, SoeClaimType.ActorCompanyId))
                {
                    Redirect(string.Format("/setcompany.aspx/?c={0}&prev={1}", actorCompanyIdFromQS, Request.Url.AbsolutePath));
                }
            }
            base.OnPreInit(e);
        }

        protected virtual void Page_Init(object sender, EventArgs e)
        {
            this.PageTitle = Constants.APPLICATION_NAME;
            this.Scripts = new List<string>();
            this.StyleSheets = new List<string>();
            this.Language = GetLanguage();

            SetMessageFromSelf();
            SetSysTermVersion();
            SetScriptTimeout();
            SetSoeFormObject();

            bool isUserValid = false;
            if (IsSupportLoggedIn || IsUserLoggedIn)
            {
                if (!IsSupportLoggedIn)
                    AddCacheDependency();

                if (!TryGetActorCompanyIdFromQS(out string _))
                {
                    RedirectWithValidQS();
                    return;
                }

                if (SoeCompany.LicenseId == SoeLicense.LicenseId || IsSupportLoggedIn)
                    isUserValid = true;
            }

            if (isUserValid)
            {
                SetLogProperties();
                SetPageTitle();
            }
            else if (Request.Url.AbsolutePath.StartsWith("/soe"))
            {
                if (SoeCompany != null && SoeUser != null)
                {
                    RedirectToLogin();
                }
                else
                {
                    ClearDataFromSession();
                    RedirectToLogout();
                }
            }
        }

        protected virtual void Page_Unload(object sender, EventArgs e)
        {
            Server.ScriptTimeout = Constants.SOE_EXECUTION_TIMEOUT_SECONDS;
        }

        protected override void InitializeCulture()
        {
            SetLanguageOnThread();
        }

        #endregion

        #region Event helpers

        protected bool TryGetActorCompanyIdFromQS(out string c)
        {
            c = QS != null ? QS["c"].NullToEmpty() : string.Empty;
            return !string.IsNullOrEmpty(c);
        }
        protected bool TryGetActorCompanyIdFromQS(out int actorCompanyId)
        {
            if (!TryGetActorCompanyIdFromQS(out string c))
            {
                actorCompanyId = 0;
                return false;
            }
            actorCompanyId = GetValidActorCompanyIdFromQS(c);
            return actorCompanyId > 0;
        }
        protected int GetValidActorCompanyIdFromQS(string c)
        {
            int comaIndex = c.LastIndexOf(',');
            if (comaIndex > 0)
                c = c.Substring(comaIndex + 1, c.Length - comaIndex - 1);

            return Int32.Parse(c);
        }

        protected bool TryGetRoleIdFromQS(out string r)
        {
            r = QS != null ? QS["r"].NullToEmpty() : string.Empty;
            return !string.IsNullOrEmpty(r);
        }
        protected int GetValidRoleIdFromQS(string r)
        {
            return Int32.Parse(r);
        }
        protected bool TryGetRoleIdFromQS(out int roleId)
        {
            if (!TryGetRoleIdFromQS(out string r))
            {
                roleId = 0;
                return false;
            }
            roleId = GetValidRoleIdFromQS(r);
            return roleId > 0;
        }

        private void SetMessageFromSelf()
        {
            MessageFromSelf = Session[Constants.SESSION_MESSAGE_FROM_SELF] as string;
            if (MessageFromSelf != null)
            {
                Session[Constants.SESSION_MESSAGE_FROM_SELF] = null;
                Session.Remove(Constants.SESSION_MESSAGE_FROM_SELF);
            }

        }

        private void SetSoeFormObject()
        {
            soeFormObject = Session[Constants.SESSION_SOEFORM_OBJECT] as SoeFormObject;
            if (soeFormObject != null)
            {
                if (soeFormObject.AbsolutePath != Request.Url.AbsolutePath)
                    ClearSoeFormObject();
            }
            else
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    if (key == null)
                        continue;

                    SoeFormMode? mode = GetSoeFormMode(key);
                    if (mode != null)
                    {
                        soeFormObject = new SoeFormObject()
                        {
                            Mode = (SoeFormMode)mode,
                            F = Request.Form,
                            AbsolutePath = Request.Url.AbsolutePath,
                        };
                        Session[Constants.SESSION_SOEFORM_OBJECT] = soeFormObject;
                        break;
                    }
                }
            }
        }

        private void SetScriptTimeout()
        {
            Server.ScriptTimeout = Constants.SOE_EXECUTION_TIMEOUT_SECONDS;
        }

        private void SetLogProperties()
        {
            SysLogManager.SetLog4NetUserProperties();
        }

        private void SetPageTitle()
        {
            if (SoeCompany == null)
                return;

            var suffix = Feature == Feature.None ? null : GetText(FeatureManager.GetSysFeatureTermId(Feature), 1);
            var displayName = string.IsNullOrEmpty(SoeCompany.ShortName) ? SoeCompany.Name : SoeCompany.ShortName;
            if (!string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(displayName))
                PageTitle = displayName.Left(15) + ": " + suffix;
            else
                PageTitle += " : " + displayName;
        }

        protected void ChangeCurrentCompany(Company company, bool updateSettings)
        {
            if (company == null)
                return;

            UserCompanyRole userCompanyRole = UserManager.GetUserCompanyRoleMapping(UserId, company.ActorCompanyId);
            if (userCompanyRole == null)
            {
                RedirectToSelfWithQS();
                return;
            }

            ParameterObject.SetSoeCompany(company.ToCompanyDTO());

            int roleId = SessionCache.GetDefaultRoleId(UserId, company.ActorCompanyId) ?? userCompanyRole.RoleId;
            ChangeCurrentRole(roleId, company.ActorCompanyId, updateSettings);

            ChangeAccountYear();
        }


        protected void ChangeAccountYear()
        {
            int accountYearId = SettingManager.GetIntSetting(
                SettingMainType.UserAndCompany, 
                (int)UserSettingType.AccountingAccountYear, 
                UserId, 
                ParameterObject.ActorCompanyId, 
                0);
            if (accountYearId > 0)
            {
                CurrentAccountYear = AccountManager.GetAccountYear(accountYearId);
            }
            else
            {
                CurrentAccountYear = null;
            }
        }

        protected void ChangeCurrentRole(int roleId, int actorCompanyId, bool updateSettings)
        {
            if (!SessionCache.ExistUserCompanyRoleMapping(UserId, actorCompanyId, roleId))
            {
                RedirectToSelfWithQS();
                return;
            }

            ParameterObject.SetActiveRoleId(roleId);

            if (updateSettings)
            {
                SettingManager.UpdateInsertIntSetting(SettingMainType.User, (int)UserSettingType.CoreRoleId, roleId, UserId, 0, 0);
                SettingManager.UpdateInsertIntSetting(SettingMainType.User, (int)UserSettingType.CoreCompanyId, actorCompanyId, UserId, 0, 0);
            }

            var claimsDict = new Dictionary<string, string>
            {
                { SoeClaimType.ActorCompanyId, actorCompanyId.ToString() },
                { SoeClaimType.RoleId, roleId.ToString() }
            };
            ClaimsHelper.UpdateClaims(Context, claimsDict);
        }

        #endregion

        #region SoftOneId

        private IClaimsHelper _claimsHelper;
        public IClaimsHelper ClaimsHelper
        {
            get
            {
                return _claimsHelper ?? (_claimsHelper = new DefaultClaimsHelper("Cookies"));
            }
        }

        private TokenHelper _tokenHelper;
        private TokenHelper TokenHelper => _tokenHelper ?? (_tokenHelper = new TokenHelper());

        public string UserToken
        {
            get
            {
                //Ask Chris!
                if (!((ClaimsPrincipal)User).HasClaim(SoeClaimType.LegacyLogIn, "true"))
                {
                    // Only legacy logins require a manually added token
                    // Non-legacy will use oidc-client to get token
                    // Not issuing a token here, will also make the client use a different 
                    // implementation of IHttpService.ts
                    return "";
                }

                return "-";
            }
        }

        protected bool HasUserVerifiedEmail()
        {
            if (ParameterObject != null && ParameterObject.SoeUser != null && ParameterObject.SoeUser.HasUserVerifiedEmail)
                return true;

            UserCompanySetting userCompanySetting = SettingManager.GetUserCompanySetting(SettingMainType.User, (int)UserSettingType.CoreHasUserVerifiedEmail, UserId, 0, 0);
            if (userCompanySetting != null && userCompanySetting.BoolData == true)
            {
                if (ParameterObject?.SoeUser != null)
                    ParameterObject.SoeUser.HasUserVerifiedEmail = true;
                return true;
            }
            return false;
        }

        public bool IsSoftoneLicense(int licenseId, string licenseNr)
        {
            return (licenseId == 20 && licenseNr == "1000");
        }

        protected void CreateLegacyLogin(ClaimsIdentity identity, UserDTO user = null, CompanyDTO company = null, CompanyDTO supportCompany = null, UserDTO supportUser = null, string redirect = "/")
        {
            var authenticationManager = Context.GetOwinContext().Authentication;

            // Retrieve the current claims identity
            var currentIdentity = authenticationManager.User.Identity as ClaimsIdentity;

            AuthenticationTicket newAuthTicket;
            if (currentIdentity != null && currentIdentity.IsAuthenticated)
            {
                ClaimsIdentity newIdentity = new ClaimsIdentity(currentIdentity.Claims, "Cookies");

                // Add new claims or update existing ones
                foreach (var newClaim in identity.Claims)
                {
                    // Remove any existing claim with the same type
                    var existingClaim = newIdentity.FindFirst(newClaim.Type);
                    if (existingClaim != null)
                    {
                        newIdentity.RemoveClaim(existingClaim);
                    }
                    newIdentity.AddClaim(newClaim);
                }

                // Sign out the current identity
                //if (supportCompanyDTO != null)
                //    authenticationManager.SignOut("Cookies");

                // Create a new authentication ticket with the updated claims
                newAuthTicket = new AuthenticationTicket(newIdentity, new AuthenticationProperties { RedirectUri = redirect });
            }
            else
            {
                // Create a new authentication ticket for the new identity
                newAuthTicket = new AuthenticationTicket(identity, new AuthenticationProperties { RedirectUri = redirect });

            }
            // Sign in with
            authenticationManager.SignIn(newAuthTicket.Properties, newAuthTicket.Identity);

            SetParameterObject(LazyParameterObject.Create(identity, company, user, supportCompany, supportUser));
        }

        #endregion

        #region Navigation

        public NameValueCollection QS
        {
            get
            {
                return Request.QueryString;
            }
        }
        public IDictionary CTX
        {
            get
            {
                return Context.Items;
            }
        }
        private string url = null;
        public string Url
        {
            get
            {
                if (url == null)
                    url = UrlUtil.TrimUrl(Request.Url.PathAndQuery);
                return url;
            }
        }
        private string[] pathParts = null;
        private string[] PathParts
        {
            get
            {
                if (pathParts == null)
                {
                    if (Request.Url.AbsolutePath == "/ajax/qSearch.aspx")
                        pathParts = UrlUtil.GetPathParts(Page.Request.UrlReferrer.AbsolutePath);
                    else
                        pathParts = UrlUtil.GetPathParts(Page.Request.Path);
                }
                return pathParts;
            }
        }
        public SoeModule SoeModule
        {
            get
            {
                SoeModule module = SoeModule.None;
                switch (Module)
                {
                    case Constants.SOE_MODULE_ECONOMY:
                        module = SoeModule.Economy;
                        break;
                    case Constants.SOE_MODULE_BILLING:
                        module = SoeModule.Billing;
                        break;
                    case Constants.SOE_MODULE_MANAGE:
                        module = SoeModule.Manage;
                        break;
                    case Constants.SOE_MODULE_ESTATUS:
                        module = SoeModule.Estatus;
                        break;
                    case Constants.SOE_MODULE_TIME:
                        module = SoeModule.Time;
                        break;
                    case Constants.SOE_MODULE_CLIENTMANAGEMENT:
                        module = SoeModule.ClientManagement;
                        break;
                }
                return module;
            }
        }
        public string Module
        {
            get
            {
                if (PathParts != null && PathParts.Length > 2)
                    return PathParts[2].ToLower();
                return null;
            }
        }
        public string Section
        {
            get
            {
                if (PathParts != null && PathParts.Length > 3)
                    return PathParts[3];
                return null;
            }
        }
        public string SelectionUrl
        {
            get
            {
                if (PathParts != null && PathParts.Length > 3)
                {
                    var selectionUrl = "";
                    for (int i = 3; i < PathParts.Length; i++)
                    {
                        selectionUrl += PathParts[i] + "/";
                    }

                    return selectionUrl;
                }

                return null;
            }
        }
        private bool IsAddOnUrl
        {
            get
            {
                string urlFormatted = !String.IsNullOrEmpty(this.Url) ? this.Url.ToLower() : String.Empty;

                return
                    urlFormatted.Contains("/ajax/") ||
                    urlFormatted.Contains(".js.aspx") ||
                    urlFormatted.Contains("/errors/CookiesDisabled");
            }
        }
        public bool IsFieldOrFormSettingPage
        {
            get
            {
                return PathParts != null && PathParts.Length >= 2 && PathParts[1] != null && PathParts[1].ToLower() == "settings";
            }
        }
        protected bool IsLegacyLogin
        {
            get
            {
                return ((ClaimsPrincipal)Context.User).HasClaim(SoeClaimType.LegacyLogIn, "true");
            }
        }

        public string AddUrlParameter(string url, string parameter, string value, bool addFirst = false, string[] parametersTobeRemoved = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                url = HttpContext.Current.Request.Url.PathAndQuery;

            List<KeyValuePair<string, string>> queryParams = this.GetUrlParameters(url).ToList();

            if (!queryParams.Exists(x => x.Key == parameter))
            {
                if (addFirst)
                    queryParams.Insert(0, new KeyValuePair<string, string>(parameter, value));
                else
                    queryParams.Add(new KeyValuePair<string, string>(parameter, value));
            }

            if (!(parametersTobeRemoved is null) && parametersTobeRemoved.Length > 0)
                queryParams = queryParams.Where(x => !parametersTobeRemoved.Contains(x.Key)).ToList();

            url = this.GetURLPathWithoutQueryString(url) + "?" + string.Join("&", queryParams.Select(x => string.Format("{0}={1}", x.Key, x.Value)));

            return url;
        }

        public NameValueCollection GetUrlParameters(string url)
        {
            int index = url.IndexOf("?");
            string query = index >= 0 ? url.Substring(index) : "";

            return HttpUtility.ParseQueryString(query);
        }

        private string GetURLPathWithoutQueryString(string url)
        {
            return url.Split('?')[0];
        }

        public string GetUrlParameter(string url, string parameter)
        {
            var query = GetUrlParameters(url);
            return query.Get(parameter);
        }

        public string UrlLogin
        {
            get
            {
                return "/Login.aspx";
            }

        }
        protected void RedirectToLogin()
        {
            Response.Redirect(UrlLogin, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        public string UrlLogout
        {
            get
            {
                return "/logout.aspx";
            }
        }
        protected void RedirectToLogout()
        {
            Response.Redirect(UrlLogout);
        }

        public string UrlSoftOneIdUnauthorized
        {
            get
            {
                return "/Unauthorized.aspx";
            }
        }
        protected void RedirectToSoftOneIdUnauthorized(SoeLoginState? loginState = null, bool endResponse = true)
        {
            string urlSoftOneId = UrlSoftOneIdUnauthorized;
            if (loginState.HasValue)
                urlSoftOneId += "/?loginState=" + ((int)loginState.Value).ToString();
            Response.Redirect(urlSoftOneId);
        }

        public string UrlLoginByTimeout
        {
            get
            {
                return "/?timeout=1";
            }
        }
        protected void RedirectToLoginByTimeout()
        {
            Response.Redirect(UrlLoginByTimeout);
        }

        public string UrlHome
        {
            get
            {
                return "/soe/";
            }
        }
        protected void RedirectToHome()
        {
            Response.Redirect(UrlHome);
        }

        public string UrlModuleRoot
        {
            get
            {
                return "/soe/" + Module;
            }
        }
        protected void RedirectToModuleRoot()
        {
            Response.Redirect(UrlModuleRoot);
        }
        protected void RedirectToModule(string module)
        {
            Response.Redirect("/soe/" + module);
        }

        public string UrlChangePassword
        {
            get
            {
                return "/soe/manage/users/edit/password/?user=" + UserId;
            }
        }
        protected void RedirectToChangePassword()
        {
            Response.Redirect(UrlChangePassword);
        }

        public string UrlVerifyEmail
        {
            get
            {
                return "/soe/manage/users/edit/email/?user=" + UserId;
            }
        }
        protected void RedirectToVerifyEmail()
        {
            Response.Redirect(UrlVerifyEmail);
        }

        public string UrlMultipleLicenses
        {
            get
            {
                return "~/errors/MultipleLicenses.aspx";
            }
        }
        protected bool TryRedirectToMultipleLicenses(Company prevCompany, License prevLicense, CompanyDTO currentCompany, License currentLicense)
        {
            if (prevCompany == null || prevLicense == null || currentCompany == null || currentLicense == null)
                return false;
            if (Context.Items["PrevCompanyName"] != null && Context.Items["PrevLicenseName"] != null && Context.Items["CurrentCompanyName"] != null && Context.Items["CurrentLicenseName"] != null)
                return false;
            Context.Items["PrevCompanyName"] = prevCompany.Name;
            Context.Items["PrevLicenseName"] = prevLicense.Name;
            Context.Items["CurrentCompanyId"] = currentCompany.ActorCompanyId;
            Context.Items["CurrentCompanyName"] = currentCompany.Name;
            Context.Items["CurrentLicenseName"] = currentLicense.Name;
            Server.Transfer(UrlMultipleLicenses);
            return true;
        }

        public string UrlError
        {
            get
            {
                return "~/errors/Error.aspx";
            }
        }
        protected void RedirectToError(string message = "")
        {
            Context.Items["Message"] = message;
            Server.Transfer(UrlError);
        }

        public string UrlReportError
        {
            get
            {
                return "~/errors/ReportError.aspx";
            }
        }
        protected void RedirectToReportError(string headerMessage, string detailMessage)
        {
            Context.Items["HeaderMessage"] = headerMessage;
            Context.Items["DetailMessage"] = detailMessage;
            Server.Transfer(UrlReportError);
        }

        public string UrlUnauthorized
        {
            get
            {
                return "~/errors/Unauthorized.aspx";
            }
        }
        protected void RedirectToUnauthorized(UnauthorizationType type)
        {
            Context.Items["UnauthorizationType"] = type;
            Server.Transfer(UrlUnauthorized);
        }

        public string UrlRemoteLoginFailed
        {
            get
            {
                return "~/errors/RemoteLoginFailed.aspx";
            }
        }
        protected void RedirectToRemoteLoginFailed(RemoteLoginFailedType type, string details = null)
        {
            Context.Items["RemoteLoginFailedType"] = type;
            if (!String.IsNullOrEmpty(details))
                Context.Items["RemoteLoginFailedDetails"] = details;
            Server.Transfer(UrlRemoteLoginFailed);
        }

        private void RedirectWithValidQS()
        {
            var redirectUrl = UrlUtil.GetModifiedUrl(Request);

            var companyId = ParameterObject?.ActorCompanyId.ToNullable()?.ToString() ?? SoeUser?.DefaultActorCompanyId?.ToString() ?? "0";
            if (companyId == "0")
            {
                var userid = SoeUser?.UserId ?? ParameterObject?.UserId ?? -1;
                LogCollector.LogError($"CompanyId == 0 in RedirectWithValidQS. userid {userid}, parameterObjectIsnull: {ParameterObject == null}, soeUserIsnull: {SoeUser == null}, licenseId: {ParameterObject?.LicenseId}");
                RedirectToLogout();
            }

            redirectUrl = UrlUtil.AddQueryStringParameter(new Uri(redirectUrl), "c", companyId);

            if (Request.Url.ToString().Contains(UrlLogout))
                Response.Redirect(redirectUrl); //Logout doesnt seem to work if redirect in other way. Stays logged in
            else
                Redirect(redirectUrl);
        }

        protected void Redirect(string url = null)
        {
            url = UrlUtil.HandlePotentialXForwarders(url, HttpContext.Current.Request);
            if (string.IsNullOrEmpty(url))
            {
                string lastModule = this.LastModule;
                if (!string.IsNullOrEmpty(lastModule))
                    RedirectToModule(lastModule);
                else
                    RedirectToHome();
            }
            else
            {
                Response.Redirect(url);
                //Context.ApplicationInstance.CompleteRequest();
                //revert to old since  RedirectToSelf("UPDATED") in old code will not stop execution if Response.Redirect endResponse = false
            }
        }

        /// <summary>
        /// Redirect to self with message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="repopulate">True to repopulate all IEntryControls in SoeForm with posted values</param>
        /// <param name="absolutePath">True to redirect to AbsolutePath, otherwise with PathAndQuery</param>
        public void RedirectToSelf(string message, bool repopulate = false, bool absolutePath = false)
        {
            //Repolulate SoeForm after redirect
            if (repopulate)
            {
                soeFormObject = new SoeFormObject()
                {
                    Mode = SoeFormMode.Repopulate,
                    F = Request.Form,
                };
                Session[Constants.SESSION_SOEFORM_OBJECT] = soeFormObject;
            }

            if (!String.IsNullOrEmpty(message))
                Session[Constants.SESSION_MESSAGE_FROM_SELF] = message;

            if (absolutePath)
                Redirect(Request.Url.AbsolutePath + GetCompanyAndRoleQS());
            else
                Redirect(Request.Url.PathAndQuery);
        }

        /// <summary>
        /// Redirect to self with message and defined querystring
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="qs">The querystring to append to absolute path</param>
        /// <param name="repopulate">True to repopulate all IEntryControls in SoeForm with posted values</param>
        public void RedirectToSelf(string message, string qs, bool repopulate = false)
        {
            //Repolulate SoeForm after redirect
            if (repopulate)
            {
                soeFormObject = new SoeFormObject()
                {
                    Mode = SoeFormMode.Repopulate,
                    F = Request.Form,
                };
                Session[Constants.SESSION_SOEFORM_OBJECT] = soeFormObject;
            }

            if (!String.IsNullOrEmpty(message))
                Session[Constants.SESSION_MESSAGE_FROM_SELF] = message;

            Redirect(Request.Url.AbsolutePath + GetCompanyAndRoleQS() + qs);
        }

        public void RedirectToSelfWithQS()
        {
            Redirect(Request.Url.AbsolutePath + GetCompanyAndRoleQS());
        }

        private string GetCompanyAndRoleQS()
        {
            if (SoeCompany != null && SoeUser != null)
            {
                var query = $"?c={SoeCompany.ActorCompanyId}&r={RoleId}";

                return AddClassficationToQueryString(query);
            }

            return string.Empty;
        }

        public string AddClassficationToQueryString(string query)
        {
            var allQs = QS.AllKeys.SelectMany(QS.GetValues, (k, v) => new { key = k, value = v });

            foreach (var qs in allQs)
            {
                if (qs.key == "classificationgroup")
                {
                    return query + $"&{qs.key}={qs.value}";
                }
            }

            return query;
        }

        #endregion

        #region WebForms

        private SoeFormObject soeFormObject = null;
        public NameValueCollection F
        {
            get
            {
                return Request.Form;
            }
        }
        protected NameValueCollection PreviousForm
        {
            get
            {
                if (soeFormObject != null)
                    return soeFormObject.F;
                return null;
            }
        }
        public string MessageFromSelf { get; private set; }
        protected bool Repopulate
        {
            get
            {
                return (Mode == SoeFormMode.Repopulate || Mode == SoeFormMode.RegisterFromCopy);
            }
        }
        public SoeFormMode Mode
        {
            get
            {
                if (soeFormObject != null)
                    return soeFormObject.Mode;
                return SoeFormMode.Save;
            }
            set
            {
                if (soeFormObject == null)
                {
                    soeFormObject = Session[Constants.SESSION_SOEFORM_OBJECT] as SoeFormObject;
                    if (soeFormObject == null)
                    {
                        soeFormObject = new SoeFormObject();
                    }
                }
                soeFormObject.Mode = value;
                Session[Constants.SESSION_SOEFORM_OBJECT] = soeFormObject;
            }
        }

        public SoeFormMode? GetSoeFormMode(string key)
        {
            SoeFormMode? mode = null;

            //Delete
            if ((key == "deletepost"))
                mode = SoeFormMode.Delete;
            //Copy
            else if ((key == "CopyPost") || (key == "CopyPost.x") || (key == "CopyPost.y"))
                mode = SoeFormMode.Copy;
            //StopSettings
            else if ((key == "StopSettings") || (key == "StopSettings.x") || (key == "StopSettings.y"))
                mode = SoeFormMode.StopSettings;
            //RunSettings
            else if ((key == "RunSettings") || (key == "RunSettings.x") || (key == "RunSettings.y"))
                mode = SoeFormMode.RunSettings;
            //Prev
            else if ((key == "Prev") || (key == "Prev.x") || (key == "Prev.y"))
                mode = SoeFormMode.Prev;
            //Next
            else if ((key == "Next") || (key == "Next.x") || (key == "Next.y"))
                mode = SoeFormMode.Next;
            //Back                    
            else if (key.ToLower(CultureInfo.InvariantCulture) == "back")
                mode = SoeFormMode.Back;
            //PrintReport
            else if ((key == "RunReport") || (key == "RunReport.x") || (key == "RunReport.y") || key.ToLower(CultureInfo.InvariantCulture) == "runreport")
                mode = SoeFormMode.RunReport;

            return mode;
        }

        public void ClearSoeFormObject()
        {
            Session[Constants.SESSION_SOEFORM_OBJECT] = null;
        }

        /// <summary>
        /// Generic method for all derived pages that check the current Mode for the page.
        /// 
        /// Its purpose is to set redirect to self on certain Modes, with a modified Mode.
        /// If the Mode matches certain Modes, the page is redirected to self with QS passed as parameters.
        /// 
        /// Should be called BEFORE optional parameters are checked on the derived Page.
        /// </summary>
        /// <param name="absolutePathAndMandatoryQS">Path that that include mandatory QS for the derived page</param>
        /// <param name="pathAndQuery">Path with the all current QS</param>
        public void PreOptionalParameterCheck(string absolutePathAndMandatoryQS, string pathAndQuery, bool keepRecordIdOnCopy = false)
        {
            //Step 1: Redirect to self with new Mode (with correct query string and Form not posted)
            if (Mode == SoeFormMode.Copy)
            {
                Mode = SoeFormMode.RegisterFromCopy;

                if (!keepRecordIdOnCopy)
                    Response.Redirect(absolutePathAndMandatoryQS);
                else
                    Response.Redirect(absolutePathAndMandatoryQS + "?copyId=" + QS["role"]);
            }
            else if (Mode == SoeFormMode.StopSettings)
            {
                Mode = SoeFormMode.NoSettingsApplied;
                Response.Redirect(pathAndQuery);
            }
            else if (Mode == SoeFormMode.RunSettings)
            {
                Mode = SoeFormMode.WithSettingsApplied;
                Response.Redirect(pathAndQuery);
            }
        }

        /// <summary>
        /// Generic method for all derived pages that check the current Mode for the page.
        /// 
        /// Its purpose is to set properties for the page's Form.
        /// If the Mode matches certain Modes, the page sets properties on the passed Form object.
        /// 
        /// Should be called AFTER optional parameters are checked on the derived Page.
        /// </summary>
        /// <param name="soeForm">The page's Form object</param>
        /// <param name="entity">The page's Entity (for example: Account, Customer etc). Null if the page is in Registration Mode</param>
        /// <param name="additionalUpdateModeCondition">Additional conditions that should be met to set the page to Update mode (for example: A Report cannot be in Update Mode if the Report is a original</param>
        /// <param name="editModeTabHeaderText">The text that the first Tab in the SoeForm should have in Edit mode</param>
        /// <param name="registerModeTabHeaderText">The text that the first Tab in the SoeForm should have in Register mode</param>
        /// <param name="title">The Title on the SoeForm</param>
        public void PostOptionalParameterCheck(Controls.Form soeForm, EntityObject entity, bool additionalUpdateModeCondition, string editModeTabHeaderText = "", string registerModeTabHeaderText = "", string soeFormTitle = "")
        {
            if (soeForm == null)
                return;

            //Step 2: Pass information to the SoeForm and clear Session
            if ((Mode == SoeFormMode.RegisterFromCopy) || (Mode == SoeFormMode.Repopulate))
            {
                soeForm.Mode = Mode;
                soeForm.PreviousForm = PreviousForm;
                ClearSoeFormObject();
            }
            else if (Mode == SoeFormMode.NoSettingsApplied)
            {
                soeForm.StopSettings = true;
                ClearSoeFormObject();
            }
            else if (Mode == SoeFormMode.WithSettingsApplied)
            {
                soeForm.StopSettings = false;
                ClearSoeFormObject();
            }
            else if (Mode == SoeFormMode.Delete)
            {
                ClearSoeFormObject();
                Delete();
            }

            if (entity != null)
                soeForm.Entity = entity;

            //Step 3: Check Register or Update to change Button text
            if ((soeForm.Mode != SoeFormMode.RegisterFromCopy) && (soeForm.Mode != SoeFormMode.Repopulate))
            {
                if (entity != null && additionalUpdateModeCondition)
                {
                    soeForm.Mode = SoeFormMode.Update;
                }
                else
                {
                    soeForm.Mode = SoeFormMode.Register;
                }
            }

            //Step 4: Set TabHeaderText on first Tab
            if (soeForm.Mode == SoeFormMode.Update)
            {
                if (!String.IsNullOrEmpty(editModeTabHeaderText))
                    soeForm.SetTabHeaderText(1, editModeTabHeaderText);
            }
            else if ((soeForm.Mode == SoeFormMode.Register || soeForm.Mode == SoeFormMode.RegisterFromCopy) && !String.IsNullOrEmpty(registerModeTabHeaderText))
            {
                soeForm.SetTabHeaderText(1, registerModeTabHeaderText);
            }

            //Step 5: Set Title on SoeForm
            if (!String.IsNullOrEmpty(soeFormTitle))
            {
                soeForm.Title = soeFormTitle;
            }
        }

        protected virtual void ValidateForm()
        {
            //Overrided by derived Pages
        }

        protected virtual void Save()
        {
            //Overrided by derived Pages
        }

        protected virtual void Delete()
        {
            //Overrided by derived Pages
        }

        protected virtual void Print()
        {
            //Overrided by derived Pages
        }

        #endregion

        #region Permissions

        private PermissionParameterObject permissionParam = null;
        private PermissionCacheRepository permissionRepository = null;

        private Feature feature = Feature.None;
        public Feature Feature
        {
            get
            {
                return feature;
            }
            set
            {
                feature = value;
                EnsureParameterObject();
                if (!HasRolePermission(feature, Permission.Readonly))
                    RedirectToUnauthorized(UnauthorizationType.FeaturePermissionMissing);
            }
        }

        public bool IsAdmin
        {
            get
            {
                return IsLicenseAdmin || IsSupportAdmin;
            }
        }
        public bool IsLicenseAdmin
        {
            get
            {
                return SoeUser.IsAdmin;
            }
        }
        public bool IsLicenseSuperAdmin
        {
            get
            {
                return SoeUser.IsSuperAdmin && SoeUser.IsAdmin;
            }
        }
        public bool IsSupportAdmin
        {
            get
            {
                return IsSupportLicense || IsSupportCompanyLoggedIn || IsSupportUserLoggedIn;
            }
        }
        public bool IsSupportSuperAdmin
        {
            get
            {
                var isSuperAdmin = ParameterObject?.IsSuperAdminMode;
                return IsSupportAdmin && isSuperAdmin.HasValue && isSuperAdmin.Value && (SoeSupportUser != null && SoeSupportUser.IsSuperAdmin);
            }
        }
        public bool IsSupportLicense
        {
            get
            {
                return SoeLicense != null && SoeLicense.Support;
            }
        }
        public bool IsSupportLoggedIn
        {
            get
            {
                return IsSupportCompanyLoggedIn || IsSupportUserLoggedIn;
            }
        }
        public bool IsUserLoggedIn
        {
            get
            {
                if (SoeUser == null || SoeLicense == null)
                    return false;
                return LicenseCacheManager.Instance.IsUserLoggedIn(SoeUser, SoeLicense.LicenseNr, SoeCompany?.ActorCompanyId ?? 0);
            }
        }
        public bool IsSupportCompanyLoggedIn
        {
            get
            {
                return SoeSupportActorCompanyId.HasValue && SoeSupportUserId.HasValue && (ParameterObject?.IsSupportLoggedInByCompany ?? false);
            }
        }
        public bool IsSupportUserLoggedIn
        {
            get
            {
                return SoeSupportActorCompanyId.HasValue && SoeSupportUserId.HasValue && (ParameterObject?.IsSupportLoggedInByUser ?? false);
            }
        }
        public bool HasValidLicenseToSuperSupportLogin
        {
            get
            {
                return IsSupportLicense && IsLicenseSuperAdmin && !IsSupportLoggedIn;
            }
        }

        public bool IsAuthorizedInCompany(int actorCompanyId)
        {
            //Rule 1: Same Company
            if (SoeCompany.ActorCompanyId == actorCompanyId)
                return true;

            //Rule 2: Administrators on SupportLicense
            if (SoeLicense.Support && SoeUser.IsAdmin)
                return true;

            //Rule 3: Administrators on Company
            if (UserManager.IsUserAdminInCompany(SoeUser, actorCompanyId))
                return true;

            return false;
        }

        public bool HasValidLicenseToSupportLogin(int licenseId, string licenseNr)
        {
            if (!IsSupportLicense)
                return false;
            if (IsSupportLoggedIn)
                return false;
            if (IsSoftoneLicense(licenseId, licenseNr) && !IsLicenseSuperAdmin)
                return false;
            return true;
        }

        public bool CanSeeUsersForLicense(License license)
        {
            if (license == null)
                return false;
            return IsSupportLicense || license.LicenseId == SoeLicense.LicenseId;
        }

        public bool CanSeeUsersForCompany(Company company)
        {
            if (company == null)
                return false;
            return IsSupportLicense || company.LicenseId == SoeLicense.LicenseId;
        }

        public bool CanSeeUsersForRole(Role role)
        {
            if (role == null || role.Company == null)
                return false;
            return IsSupportLicense || role.Company.LicenseId == SoeLicense.LicenseId;
        }

        public bool CanSeeUser(User user)
        {
            if (user == null)
                return false;
            return IsSupportLicense || user.LicenseId == SoeLicense.LicenseId;
        }

        public bool CanLoginAsSupportAdmin(Company company, int licenseId, string licenseNr)
        {
            if (company == null)
                return false;
            if (!HasValidLicenseToSupportLogin(licenseId, licenseNr))
                return false;
            if (!company.IsSupportLoginAllowed())
                return false;
            return true;
        }

        public bool HasRolePermission(Feature feature, Permission permission)
        {
            if (feature == Feature.None || permission == Permission.None)
                return true;

            Permission currentPermission = GetRolePermission(feature, permission == Permission.Modify);
            return currentPermission.IsValid(permission);
        }

        public Permission GetRolePermission(Feature feature, bool checkAccountYear = false)
        {
            if (SoeCompany == null)
                return Permission.None;
            if (feature == Feature.None)
                return Permission.Modify;

            bool isTemplateCompany = SoeCompany.Template || SoeCompany.Global;
            bool isIinvalidAccountYear = checkAccountYear && (CurrentAccountYear == null || CurrentAccountYear.Status != (int)TermGroup_AccountStatus.Open);
            bool isSupportCompany = SoeLicense.Support;
            bool isSupportCompanyLoggedIn = IsSupportCompanyLoggedIn;

            Permission? permission = FeatureManager.CheckFeatureValidity(feature, isTemplateCompany, isIinvalidAccountYear, isSupportCompany, isSupportCompanyLoggedIn, SoeUser);
            if (permission.HasValue)
                return permission.Value;

            if (this.permissionParam == null || this.permissionRepository == null)
            {
                this.ParameterObject?.SetThread("Web");
                var permissions = new FeatureManager(this.ParameterObject).GetPermissionRepository(this.ParameterObject?.LicenseId ?? this.SoeCompany.LicenseId, this.ParameterObject?.ActorCompanyId ?? this.SoeCompany.ActorCompanyId, this.ParameterObject?.RoleId ?? this.SoeUser.DefaultRoleId);
                this.permissionParam = permissions.Param;
                this.permissionRepository = permissions.Repository;
            }
            return this.permissionRepository.GetPermission(this.permissionParam, feature);
        }

        public void ClearLicensePermissionsFromCache(params int[] licenseIds)
        {
            foreach (int licenseId in licenseIds)
            {
                FeatureManager.ClearLicensePermissionsFromCache(licenseId);
            }
        }

        public void ClearCompanyPermissionsFromCache(int licenseId, int actorCompanyId)
        {
            FeatureManager.ClearCompanyPermissionsFromCache(licenseId, actorCompanyId);
        }

        public void ClearRolePermissionsFromCache(int licenseId, int actorCompanyId, int roleId)
        {
            FeatureManager.ClearRolePermissionsFromCache(licenseId, actorCompanyId, roleId);
        }

        #endregion

        #region Page cache

        private void AddCacheDependency()
        {
            Response.AddCacheItemDependency(Constants.CACHE_OUTPUT_CACHE_DEPENDENCY);
        }

        public void RemoveOutputCacheItem(string path)
        {
            HttpResponse.RemoveOutputCacheItem(path);
        }

        public void RemoveAllOutputCacheItems(string currentPath)
        {
            HttpContext.Current.Cache.Insert(Constants.CACHE_OUTPUT_CACHE_DEPENDENCY, DateTime.Now, null, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.NotRemovable, null);

            if (!String.IsNullOrEmpty(currentPath))
            {
                //Sometimes the current page is not removed otherwise
                if (currentPath.EndsWith("/"))
                    currentPath += "default.aspx";
                RemoveOutputCacheItem(currentPath);
            }
        }

        #endregion

        #region Language/Terms

        public ITextService TextService
        {
            get
            {
                return App_Start.DependencyInjectionSetUp.Kernel.Get<ITextService>();
            }
        }

        protected void CheckLanguage()
        {
            if (Request.Form.Count > 0)
                return;

            //Validate lang parameter
            string cultureCode = QS["lang"];
            if (!String.IsNullOrEmpty(cultureCode) && !LanguageManager.IsValidSysLanguage(cultureCode))
                cultureCode = "";

            if (String.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetLanguage();
                SetLanguage(cultureCode);
            }
            else
            {
                SetLanguage(cultureCode);
                RedirectToLogin();
            }
        }

        protected void SetLanguage(string cultureCode)
        {
            if (Request == null || string.IsNullOrEmpty(cultureCode))
                return;

            CultureInfo culture = CalendarUtility.GetValidCultureInfo(cultureCode);
            if (culture == null)
                return;

            SaveCookie(Constants.COOKIE_LANG, cultureCode);
        }
        private void SetLanguageOnThread()
        {
            if (!String.IsNullOrEmpty(this.Language))
                return;

            string lang = GetLanguage();

            CultureInfo cultureInfo = CalendarUtility.GetValidCultureInfo(lang);
            if (cultureInfo == null)
                return;

            //Culture
            this.Culture = lang;
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            //UICulture
            this.UICulture = lang;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public string GetLanguage(bool skipCookie = false)
        {
            string cultureCode = "";

            //Prio 1: Session
            //Deprecated

            //Prio 2: Cookie
            if (!skipCookie)
                cultureCode = GetLanguageFromCookie();

            //Prio 3: User
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetLanguageFromFromUser();
                if (!string.IsNullOrEmpty(cultureCode))
                    SetLanguage(cultureCode);
            }

            //Prio 4: Default
            if (string.IsNullOrEmpty(cultureCode))
                cultureCode = GetLanguageDefault();

            return cultureCode;
        }

        public int GetLanguageId()
        {
            if (string.IsNullOrEmpty(this.Language))
                this.Language = GetLanguage();
            return TermCacheManager.Instance.GetLangId(this.Language);
        }

        private string GetLanguageFromCookie()
        {
            return GetCookie(Constants.COOKIE_LANG);
        }

        private string GetLanguageFromFromUser()
        {
            return SoeUser != null && SoeUser.LangId.HasValue ? LanguageManager.GetSysLanguageCode(SoeUser.LangId.Value) : String.Empty;
        }

        private string GetLanguageDefault()
        {
            return Constants.SYSLANGUAGE_LANGCODE_DEFAULT;
        }

        public bool IsLanguageSwedish() => GetLanguageId() == (int)TermGroup_Languages.Swedish;
        public bool IsLanguageEnglish() => GetLanguageId() == (int)TermGroup_Languages.English;
        public bool IsLanguageFinnish() => GetLanguageId() == (int)TermGroup_Languages.Finnish;
        public bool IsLangugeNorwegian() => GetLanguageId() == (int)TermGroup_Languages.Norwegian;

        public string SysTermVersion;
        private string GetSysTermVersion() => TermCacheManager.SysTermVersion;
        public void SetSysTermVersion()
        {
            var cachedVersion = GetSysTermVersion();
            if (!string.IsNullOrEmpty(cachedVersion))
            {
                if (string.IsNullOrEmpty(SysTermVersion))
                    SysTermVersion = cachedVersion;
                return;
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fileVersionInfo.ProductVersion;

            if (version == null || version.Equals("1.0.0.0"))
                version = DateTime.Now.ToString("yyyy.MM.dd.HHmm");

            TermCacheManager.SysTermVersion = version;
            SysTermVersion = version;
        }
               
        public string GetText(int sysTermId, string defaultTerm) => TextService?.GetText(sysTermId, defaultTerm: defaultTerm) ?? string.Empty;
        public string GetText(int sysTermId, int sysTermgroupId) => TextService?.GetText(sysTermId, sysTermgroupId) ?? string.Empty;

        public Dictionary<int, string> GetGrpText(TermGroup termGroup, bool addEmptyRow = false, bool includeKey = false, bool? sortByValue = null)
        {
            return TermCacheManager.Instance.GetTermGroupDictFromWeb(termGroup, Thread.CurrentThread.CurrentCulture.Name, addEmptyRow, includeKey, sortByValue);
        }
        public SortedDictionary<int, string> GetGrpTextSorted(TermGroup termGroup, bool addEmptyRow = false, bool includeKey = false, int? minKey = null, int? maxKey = null)
        {
            return TermCacheManager.Instance.GetTermGroupDictSorted(termGroup, GetLanguageId(), addEmptyRow, includeKey, minKey, maxKey);
        }

        #endregion

        #region Session/Cookies

        private AccountYear _currentAccountYear = null;
        public AccountYear CurrentAccountYear
        {
            get
            {
                if (this._currentAccountYear == null)
                    this._currentAccountYear = Session[Constants.SESSION_ACCOUNTYEAR] as AccountYear;
                return this._currentAccountYear;
            }
            set
            {
                this._currentAccountYear = value;
                Session[Constants.SESSION_ACCOUNTYEAR] = this._currentAccountYear;
            }
        }

        private string _currentAccountHierarchy = null;
        public string CurrentAccountHierarchy
        {
            get
            {
                if (this._currentAccountHierarchy == null)
                    this._currentAccountHierarchy = GetSessionAndCookie(Constants.COOKIE_ACCOUNTHIERARCHY);
                return this._currentAccountHierarchy;
            }
            set
            {
                this._currentAccountHierarchy = value;
                AddToSessionAndCookie(Constants.COOKIE_ACCOUNTHIERARCHY, this._currentAccountHierarchy);
            }
        }

        private string _lastModule = null;
        public string LastModule
        {
            get
            {
                if (string.IsNullOrEmpty(this._lastModule))
                    this._lastModule = GetSessionAndCookie(Constants.COOKIE_LASTMODULE);
                return this._lastModule;
            }
            set
            {
                if (value != null && value != this._lastModule)
                {
                    this._lastModule = value;
                    AddToSessionAndCookie(Constants.COOKIE_LASTMODULE, this._lastModule);
                }
            }
        }

        public DateTime GetTimeoutWarningTime()
        {
            DateTime sessionModifed = Convert.ToDateTime(GetSessionAndCookie(Constants.COOKIE_KEEPSESSIONALIVE));
            DateTime warningTime = sessionModifed.AddMinutes(GetLifeTimeMinutes()).AddMinutes(-Constants.SOE_SESSION_TIMEOUT_WARN_MINUTES);
            return warningTime;
        }
        public DateTime GetTimeoutLogoutTime()
        {
            DateTime sessionModifed = Convert.ToDateTime(GetSessionAndCookie(Constants.COOKIE_KEEPSESSIONALIVE));
            DateTime logoutTime = sessionModifed.AddMinutes(GetLifeTimeMinutes());
            return logoutTime;
        }
        private int GetLifeTimeMinutes()
        {
            int userId = SoeUser?.UserId ?? 0;
            if (userId == 0)
                return 120; //Dont use Session.Timeout beacuse it may not have been loaded when coming from login.aspx and not yet is logged in

            string key = "GetLifeTimeMinutes#" + userId;
            var lifetimeMinutes = BusinessMemoryCache<int>.Get(key);
            if (lifetimeMinutes > 0)
                return lifetimeMinutes;

            lifetimeMinutes = this.Session.Timeout;
            var lifetimeSecondsOnUser = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.LifetimeSeconds, userId, 0, 0);
            if (lifetimeSecondsOnUser > 60 * 10)
                lifetimeMinutes = lifetimeSecondsOnUser / 60;

            BusinessMemoryCache<int>.Set(key, lifetimeMinutes, 60 * 60);
            return lifetimeMinutes;
        }

        protected void SetParameterObject(ParameterObject parameterObject)
        {
            if (parameterObject != null)
                this._parameterObject = parameterObject;
        }
        protected void EnsureParameterObject()
        {
            if (this._parameterObject == null && HttpContext.Current?.User?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                int? userIdFromClaim = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.UserId);
                int? actorCompanyIdFromClaim = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.ActorCompanyId);

                var user = GetUser(userIdFromClaim, actorCompanyIdFromClaim);
                var company = GetCompany(actorCompanyIdFromClaim, user);
                var supportCompany = GetSupportCompany();
                var supportUser = supportCompany != null ? GetSupportUser() : null;

                SetParameterObject(LazyParameterObject.Create(identity, company, user, supportCompany, supportUser));
            }
        }

        private UserDTO GetUser(int? userIdFromClaim, int? actorCompanyId)
        {
            var company = actorCompanyId.HasValue ? SessionCache.GetCompanyFromCache(actorCompanyId.Value) : null;
            var user = userIdFromClaim.HasValue ? SessionCache.GetUserFromCache(userIdFromClaim.Value, company?.ActorCompanyId ?? 0) : null;

            if (user == null)
            {
                var userGuid = ClaimsHelper.GetGuidClaim(this.Context, SoeClaimType.UserGuid);
                if (userGuid.HasValue)
                {
                    var um = new UserManager(null);
                    var users = um.GetUsersWithGuid(userGuid.Value);
                    if (users.Count == 1)
                        user = SessionCache.GetUserFromCache(users[0].UserId, company?.ActorCompanyId ?? 0);
                }
            }
            return user;
        }
        private CompanyDTO GetCompany(int? actorCompanyIdFromQS, UserDTO user)
        {
            var company = actorCompanyIdFromQS.HasValue ? SessionCache.GetCompanyFromCache(actorCompanyIdFromQS.Value) : null;
            if (company == null)
            {
                var actorCompanyIdFromClaims = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.ActorCompanyId);
                if (actorCompanyIdFromClaims.HasValue)
                    company = SessionCache.GetCompanyFromCache(actorCompanyIdFromClaims.Value);
                if (company == null && user?.DefaultActorCompanyId != null)
                    company = SessionCache.GetCompanyFromCache(user.DefaultActorCompanyId.Value);
            }
            return company;
        }

        private CompanyDTO GetSupportCompany()
        {
            var supportActorCompanyId = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.SupportActorCompanyId);
            return supportActorCompanyId.ToNullable().HasValue ? SessionCache.GetCompanyFromCache(supportActorCompanyId.Value) : null;
        }
        private UserDTO GetSupportUser()
        {
            var supportUserId = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.SupportUserId);
            return supportUserId.ToNullable().HasValue ? SessionCache.GetUserFromCache(supportUserId.Value, GetSupportCompany()?.ActorCompanyId ?? 0) : null;
        }

        protected object GetDataFromSession(string type)
        {
            return Session?[type];
        }
        protected void AddDataToSession(string type, object value)
        {
            Session?.Add(type, value);
        }
        protected void ClearDataFromSession()
        {
            /// Q:  Why not just do Session.Clear()?
            /// A:  Because there might be Session data that we want to keep after logout.
            /// A2: For now, we want to delete ALL Session data
            /// A3: And now, we actually want to destroy the session (after pen-test)

            Context?.GetOwinContext().Authentication.SignOut("Cookies");
            Session.Abandon();
            SetParameterObject(null);
            SysLogManager.ClearLog4netUserProperties();
            Response.Cookies.Clear();
            Request.Cookies.Clear();
        }

        protected string GetCookie(string type)
        {
            if (Request?.Cookies == null)
                return null;

            HttpCookie cookie = Request.Cookies[type];
            if (cookie != null && type == Constants.COOKIE_LANG)
            {
                string cultureCode = cookie.Value;
                if (!string.IsNullOrEmpty(cultureCode) && !SysDbCache.Instance.SysLanguages.Any(l => l.LangCode == cultureCode))
                    cultureCode = "";

                if (string.IsNullOrEmpty(cultureCode))
                {
                    cultureCode = GetLanguage(skipCookie: true);
                    if (string.IsNullOrEmpty(cultureCode))
                        cultureCode = "sv-SE";
                }

                SetLanguage(cultureCode);
            }

            return cookie?.Value;
        }
        protected void SaveCookie(string name, string value, int? expirationDays = null, bool httpOnly = false)
        {
            if (Request == null || value == null)
                return;

            if (!expirationDays.HasValue)
                expirationDays = Constants.COOKIE_EXPIRATIONDAYS;

            HttpCookie cookie = Request.Cookies[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name)
                {
                    Value = value,
                    Expires = DateTime.Now.AddDays(expirationDays.Value),
                    HttpOnly = httpOnly, //NOSONAR
                    SameSite = SameSiteMode.None,
                    Secure = true,
                };
                Response.Cookies.Add(cookie);
            }
            else
            {
                DateTime expiration = DateTime.Now.AddDays(expirationDays.Value);
                if (cookie.Value != value || expiration.Subtract(cookie.Expires).TotalMinutes > 1)
                {
                    cookie.Value = value;
                    cookie.Expires = expiration;
                    cookie.HttpOnly = httpOnly; //NOSONAR
                    cookie.SameSite = SameSiteMode.None;
                    cookie.Secure = true;
                    Response.Cookies.Set(cookie);
                }
            }
        }

        public string GetSessionAndCookie(string type, bool useSession = true)
        {
            if (useSession)
            {
                var session = GetDataFromSession(type);
                if (session != null)
                    return session.ToString();
            }

            if (Request != null && Request.Cookies != null && Request.Cookies[type] != null)
            {
                var cookie = Request.Cookies[type];
                if (cookie != null)
                    return cookie.Value;
            }

            return null;
        }
        public bool GetSessionAndCookieBool(string type, bool useSession = true, bool useCookie = true)
        {
            bool value = false;

            if (useSession)
            {
                var session = GetDataFromSession(type);
                if (session != null)
                {
                    bool.TryParse(session.ToString(), out value);
                    return value;
                }
            }

            if (useCookie)
            {
                var cookie = GetCookie(type);
                if (cookie != null)
                {
                    bool.TryParse(cookie, out value);
                    return value;
                }
            }

            return value;
        }

        public void AddToSessionAndCookie<T>(string type, T value, bool useSession = true)
        {
            if (useSession)
                AddDataToSession(type, value);
            SaveCookie(type, value?.ToString());
        }
        public void RemoveFromSessionAndCookie(string type)
        {
            Session?.Remove(type);
            Request?.Cookies.Remove(type);
        }

        #endregion

        #region Services

        public bool UseCrystalService()
        {
            bool isLocalHost = Page.Request != null && UrlUtil.UrlContainsSectionUrl(Page.Request.Url.AbsoluteUri, "localhost");
            bool useWebService = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseWebService, 0, 0, 0);
            return !isLocalHost && useWebService;
        }
        public bool UseWebApiInternal()
        {
#if DEBUG
            return false;
#else
            bool isLocalHost = Page.Request != null && UrlUtil.UrlContainsSectionUrl(Page.Request.Url.AbsoluteUri, "localhost");
            bool useWebApiInternal = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseWebApiInternal, 0, 0, 0);
            return !isLocalHost && useWebApiInternal;
#endif
        }
        public ICrystalChannel GetCrystalServiceChannel()
        {
            var binding = new System.ServiceModel.BasicHttpBinding()
            {
                Name = "crystalChannel_basic",
                SendTimeout = new TimeSpan(2, 0, 0),
                ReceiveTimeout = new TimeSpan(2, 0, 0),
                OpenTimeout = new TimeSpan(0, 5, 0),
                CloseTimeout = new TimeSpan(0, 5, 0),
                MaxBufferSize = Int32.MaxValue - 1,
                MaxReceivedMessageSize = Int32.MaxValue - 1,
            };
            string address = SettingManager.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.CrystalServiceUrl, 0, 0, 0);
            var endpoint = new System.ServiceModel.EndpointAddress(address);
            var channelFactory = new System.ServiceModel.ChannelFactory<ICrystalChannel>(binding, endpoint);
            var channel = channelFactory.CreateChannel();

            return channel;
        }

        #endregion

        #region Encoding

        public string Enc(object source)
        {
            if (source == null)
                return string.Empty;
            return StringUtility.XmlEncode(source.ToString());
        }

        #endregion
    }
}
