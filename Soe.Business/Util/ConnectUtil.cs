using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SoftOne.Soe.Business.Util
{
    public class ConnectUtil : ManagerBase
    {
        public ConnectUtil(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        #region Constants

        new protected const string THREAD = "WebService";
        public const string VALIDATION_CREDENTIAL_ERRORMESSAGE = "Security credentials not valid";
        public const string VALIDATION_APIKEY_ERRORMESSAGE = "Invalid API key";
        public const string VALIDATION_LASTSYNCHDATE_ERRORMESSAGE = "Invalid date format";
        public const string VALIDATION_SOURCE_ERRORMESSAGE = "Invalid source";
        public const string VALIDATION_DEFINITION_ERRORMESSAGE = "Invalid definition";
        public const string AXFOODLICENSEAPIKEY = "ee9c2f7c-fc1e-4ec7-bb91-e066215452d5";

        #endregion

        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TokenMemoryCacher tokenCache = new TokenMemoryCacher();
        private static readonly ConnectApiKeyMemoryCacher connectApiCache = new ConnectApiKeyMemoryCacher();
        private static readonly CompanyApiKeyMemoryCacher companyApiKeyCache = new CompanyApiKeyMemoryCacher();

        #endregion

        public ParameterObject GetParameterObject(int actorCompanyId, int userId)
        {
            CompanyManager cm = new CompanyManager(null);
            UserManager um = new UserManager(null);

            Company company = actorCompanyId > 0 ? cm.GetCompany(actorCompanyId, true) : null;
            User user = userId > 0 ? um.GetUser(userId, loadUserCompanyRole: true) : new User() { LoginName = this.ToString() };

            return ParameterObject.Create(user: um.GetSoeUser(actorCompanyId, user),
                                          company: cm.GetSoeCompany(company),
                                          thread: THREAD,
                                          roleId: um.GetDefaultRoleId(actorCompanyId, userId));
        }

        public bool ValidateLogin(Login login, out string detailedMessage, out int userId, out User user, out int licenseId, out int roleId, out ParameterObject paramenterObject, bool ignoreImportPermission = false)
        {
            LoginDTO loginDTO = new LoginDTO();
            loginDTO.License = login.license;
            loginDTO.UserName = login.userName;
            loginDTO.Password = login.password;

            return ValidateLogin(loginDTO, out detailedMessage, out userId, out user, out licenseId, out roleId, out paramenterObject, ignoreImportPermission);
        }

        private bool HasConnectPermission(int roleId, int actorCompanyId, int licensenId)
        {
            var fm = new FeatureManager(null);
            return (fm.HasRolePermission(Feature.Billing_Import_XEConnect, Permission.Modify, roleId, actorCompanyId, licensenId) ||
                    fm.HasRolePermission(Feature.Time_Import_XEConnect, Permission.Modify, roleId, actorCompanyId, licensenId) ||
                    fm.HasRolePermission(Feature.Economy_Import_XEConnect, Permission.Modify, roleId, actorCompanyId, licensenId) ||
                    fm.HasRolePermission(Feature.Time_Export_XEConnect, Permission.Modify, roleId, actorCompanyId, licensenId));
        }

        private bool HasCompanyPermission(int userId, int actorCompanyId, int licenseId)
        {
            var connectedCompanies = CompDbCache.Instance.GetUserCompanyRoles(userId).Where(x => x.ActorCompanyId == actorCompanyId);

            foreach (var companyRole in connectedCompanies)
            {
                if (HasConnectPermission(companyRole.RoleId, companyRole.ActorCompanyId, licenseId))
                { return true; }
            }
            return false;
        }

        private bool ValidateLogin(LoginDTO loginDTO, out string detailedMessage, out int userId, out User user, out int licenseId, out int roleId, out ParameterObject paramenterObject, bool ignoreImportPermission = false)
        {
            LoginManager lm = new LoginManager(null);
            paramenterObject = ParameterObject.Create(extendedUserParams: new ExtendedUserParams());

            string license = loginDTO.License;
            string loginName = loginDTO.UserName;
            string password = loginDTO.Password;
            Company company;
            user = new User();
            Role role;
            userId = 0;
            licenseId = 0;
            roleId = 0;

            SoeLoginState state = lm.LoginUser(license, loginName, password, out detailedMessage, out company, out user, out role, true, false);

            if (state != SoeLoginState.OK)
            {
                detailedMessage = $"State={state.ToString()}";
                return false;
            }
            else
            {

                userId = user.UserId;
                licenseId = user.LicenseId;
                roleId = role != null ? role.RoleId : 0;

                if (company != null)
                    paramenterObject = GetParameterObject(company.ActorCompanyId, userId);

                if (ignoreImportPermission)
                {
                    return true;
                }
                else if (role != null && company != null && HasConnectPermission(role.RoleId, company.ActorCompanyId, company.LicenseId))
                {
                    var result = UserManager.LoginUserSession(user.UserId, user.LoginName, company.ActorCompanyId, company.Name);

                    if (!result.StringValue.IsNullOrEmpty() && int.TryParse(result.StringValue, out int userSessionId))
                    {
                        using (CompEntities entities = new CompEntities())
                        {
                            var userSession = entities.UserSession.FirstOrDefault(f => f.UserSessonId == userSessionId);

                            if (userSession != null && System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                            {
                                var bc = System.Web.HttpContext.Current.Request.Browser;
                                if (bc != null)
                                {
                                    //Browser
                                    userSession.Browser += bc.Browser + " ";
                                    userSession.Browser += bc.Version + " ";
                                    if (!bc.Cookies)
                                        userSession.Browser += "Cookies:0" + " ";
                                    if (!bc.SupportsCss)
                                        userSession.Browser += "CSS:0" + " ";
                                    if (bc.Beta)
                                        userSession.Screen += "Beta:1" + " ";

                                    //Platform
                                    userSession.Platform += bc.Platform + " ";
                                    if (bc.Win16)
                                        userSession.Platform += "Win16:1" + " ";
                                    else if (bc.Win32)
                                        userSession.Platform += "Win32:1" + " ";
                                }
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    detailedMessage = "No Import Permission";
                    return false;
                }
            }
        }

        public string Validate(int source, string apiKey, string importApiKey, int sysImportHeadId, out int companyId, out TermGroup_IOSource ioSource, out TermGroup_IOType ioType, out int? sysDefinitionId, out int? compImportId)
        {
            ioType = TermGroup_IOType.WebService;

            string validationMessage = "";
            if (!ValidateCompany(apiKey, out companyId))
                validationMessage = VALIDATION_APIKEY_ERRORMESSAGE;
            if (!ValidateSource(source, out ioSource))
                validationMessage = VALIDATION_SOURCE_ERRORMESSAGE;
            if (!ValidateDefinition(importApiKey, sysImportHeadId, out sysDefinitionId, out compImportId))
                validationMessage = VALIDATION_DEFINITION_ERRORMESSAGE;

            return validationMessage;
        }

        public string Validate(int source, string companyApiKey, out int companyId, out TermGroup_IOSource ioSource, out TermGroup_IOType ioType)
        {
            ioSource = TermGroup_IOSource.Unknown;
            ioType = TermGroup_IOType.WebService;

            string validationMessage = "";
            companyId = 0;

            if (!ValidateCompany(companyApiKey, out companyId))
                validationMessage = VALIDATION_APIKEY_ERRORMESSAGE;
            if (!ValidateSource(source, out ioSource))
                validationMessage = VALIDATION_SOURCE_ERRORMESSAGE;

            return validationMessage;
        }

        public LoginDTO GetToken(Guid companyApiKey, Guid connectApiKey, SoftOne.Soe.Common.DTO.LoginDTO loginDTO)
        {
            #region Validation

            if (loginDTO == null)
                return new LoginDTO() { Message = "Login is null" };
            if (companyApiKey == Guid.Empty)
                return new LoginDTO() { Message = "Invalid companyApiKey" };
            if (connectApiKey == Guid.Empty)
                return new LoginDTO() { Message = "Invalid connectApiKey" };

            UserManager um = new UserManager(null);

            string validatationResult = string.Empty;
            int userId, actorCompanyId;
            ParameterObject parameterObject;
            bool isLicenseApiKey = IsLicenseApiKey(companyApiKey.ToString());
            if (isLicenseApiKey)
            {
                validatationResult = ValidateUserAndKeys(companyApiKey, connectApiKey, loginDTO, out actorCompanyId, out parameterObject, out userId);
            }
            else if (!string.IsNullOrEmpty(loginDTO.Token))
            {
                string checkTokenResult = ValidateToken(companyApiKey, connectApiKey, loginDTO.Token, out parameterObject, out actorCompanyId, false);
                if (!string.IsNullOrEmpty(checkTokenResult))
                {
                    validatationResult = ValidateUserAndKeys(companyApiKey, connectApiKey, loginDTO, out actorCompanyId, out parameterObject, out userId);
                }
                else
                {
                    loginDTO.Message = "Token is valid, session extended";
                    return loginDTO;
                }
            }
            else if (!connectApiKey.ToString().Equals(Constants.SoftOneStage))
            {
                validatationResult = ValidateUserAndKeys(companyApiKey, connectApiKey, loginDTO, out actorCompanyId, out parameterObject, out userId);
            }
            else
            {
                if (!ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
                {
                    loginDTO.Message = "Invalid companyApiKey";
                    Thread.Sleep(2000);
                    return loginDTO;
                }
                else
                {
                    userId = um.GetAdminUser(actorCompanyId).UserId;
                    loginDTO.Token = CreateToken(companyApiKey.ToString(), connectApiKey.ToString(), isLicenseApiKey, ref userId);
                    return loginDTO;
                }
            }

            if (!string.IsNullOrEmpty(validatationResult))
            {
                Thread.Sleep(2000);
                loginDTO.Message = "Login failed " + validatationResult;
                return loginDTO;
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                UserSession session;

                if (string.IsNullOrEmpty(loginDTO.Token))
                {
                    session = um.GetLastUserSession(entities, userId, onlyLast24Hours: true);
                    if (session != null && session.Login < DateTime.Now.AddSeconds(10))
                    {
                        session = null;
                        User user = UserManager.GetUser(entities, userId);

                        if (user != null)
                        {
                            session = new UserSession()
                            {
                                Login = DateTime.Now,
                                User = user,
                                Description = $"connectApiKey {connectApiKey} companyApiKey {companyApiKey} isLicenseApiKey {isLicenseApiKey}"
                            };
                            entities.UserSession.AddObject(session);
                            SaveChanges(entities);
                        }
                    }
                }
                else
                    session = um.GetUserSession(entities, loginDTO.Token);

                companyApiKeyCache.Add(companyApiKey.ToString(), actorCompanyId);

                if (session != null)
                {
                    int id = userId;
                    if (string.IsNullOrEmpty(loginDTO.Token))
                        loginDTO.Token = CreateToken(companyApiKey.ToString(), connectApiKey.ToString(), isLicenseApiKey, ref id);
                    loginDTO.Message = "Login successful, use token within 120 seconds";

                    session.Token = loginDTO.Token;
                    session.Logout = DateTime.Now.AddMinutes(2);
                    int res = entities.SaveChanges();

                    return loginDTO;
                }
                else
                {
                    loginDTO.Message = "Login failed";
                    return loginDTO;
                }
            }
        }

        public string ValidateCompanyApiKeyAndActorCompanyId(Guid companyApiKey, int actorCompanyId)
        {
            var actualActorCompanyId = companyApiKeyCache.GetValue(companyApiKey.ToString());
            if (actualActorCompanyId == 0)
            {
                var fromdb = CompanyManager.GetActorCompanyIdFromApiKey(companyApiKey.ToString());

                if (fromdb.HasValue)
                    actualActorCompanyId = fromdb.Value;
            }

            if (actualActorCompanyId != actorCompanyId || actualActorCompanyId == 0)
            {
                return $"CompanyApiKey is not matching {companyApiKey.ToString()} : {actualActorCompanyId} <> {actorCompanyId} ";
            }
            return string.Empty;
        }

        public string ValidateToken(string token, Guid companyApiKey, out ParameterObject parameterObject, out int actorCompanyId)
        {
            parameterObject = tokenCache.GetValue(token + companyApiKey.ToString());

            if (!ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
                return " CompanyApiKey Invalid";

            if (parameterObject != null && actorCompanyId == parameterObject.ActorCompanyId)
                return string.Empty;

            using (CompEntities entities = new CompEntities())
            {
                var userSession = UserManager.GetUserSession(entities, token);
                if (userSession == null)
                {
                    Thread.Sleep(2000);
                    actorCompanyId = 0;
                    return "Invalid Token";
                }

                if (userSession.Logout.HasValue && userSession.Logout.Value < DateTime.Now)
                    return "Session Expired";
                if (!userSession.Logout.HasValue && userSession.Login < DateTime.Now.AddMinutes(-1))
                    return "Session not activated, Token needs to be used within 120 seconds";

                userSession.Logout = DateTime.Now.AddMinutes(60);
                entities.SaveChanges();

                parameterObject = GetParameterObject(actorCompanyId, userSession.User.UserId);
                tokenCache.Add(token + companyApiKey.ToString(), parameterObject);
                return string.Empty;
            }
        }

        public string ValidateToken(Guid companyApiKey, Guid connectApiKey, string token, out ParameterObject parameterObject, out int actorCompanyId, bool sleep = true)
        {
            string detailedMessage = string.Empty;
            actorCompanyId = 0;
            parameterObject = ParameterObject.Empty();
            ConnectUtil connectUtil = new ConnectUtil(null);

            using (CompEntities entities = new CompEntities())
            {
                int id = 0;

                var validConnectApikey = ValidateConnectApiKey(connectApiKey);

                if (!validConnectApikey.Success)
                    return validConnectApikey.ErrorMessage;

                var PreCheckToken = connectUtil.ValidateToken(companyApiKey.ToString(), connectApiKey.ToString(), token, ref id);

                if (PreCheckToken.Success && !PreCheckToken.BooleanValue)
                {
                    if (!ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
                        detailedMessage = detailedMessage + " CompanyApiKey Invalid";
                    else
                    {
                        parameterObject = GetParameterObject(actorCompanyId, id);
                        return detailedMessage;
                    }
                }
                else if (!PreCheckToken.Success)
                    return "Invalid Token string";

                UserSession userSession = UserManager.GetUserSession(entities, token);

                if (userSession == null)
                {
                    detailedMessage = "Invalid Token";
                    if (sleep)
                        Thread.Sleep(2000);
                    return detailedMessage;
                }

                if (!ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
                    detailedMessage = detailedMessage + " CompanyApiKey Invalid";

                parameterObject = ParameterObject.Empty();

                if (string.IsNullOrEmpty(detailedMessage))
                {
                    parameterObject = GetParameterObject(actorCompanyId, userSession.User.UserId);

                    if (userSession.Logout.HasValue && userSession.Logout.Value < DateTime.Now)
                        detailedMessage = "Session Expired";

                    if (!userSession.Logout.HasValue && userSession.Login < DateTime.Now.AddMinutes(-2))
                        detailedMessage = detailedMessage + " Session not activated, Token needs to be used within 120 seconds";

                    if (string.IsNullOrEmpty(detailedMessage))
                    {
                        userSession.Logout = DateTime.Now.AddMinutes(10);
                        entities.SaveChanges();
                    }
                }
            }

            return detailedMessage;
        }

        public string ValidateUserAndKeys(Guid companyApiKey, Guid connectApiKey, LoginDTO loginDTO, out int actorCompanyId, out ParameterObject parameterObject, out int userId)
        {
            actorCompanyId = 0;
            string detailedMessage;
            User user;
            int licenseId, roleId;
            if (!ValidateLogin(loginDTO, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                LogWarning($"ValidateLogin failed with message: {detailedMessage} license:{loginDTO.License} username:{loginDTO.UserName}");
                return string.IsNullOrEmpty(detailedMessage) ? "Login failed (User)" : detailedMessage;
            }

            bool isLicenseApiKey = companyApiKey.ToString().ToLower().Equals(AXFOODLICENSEAPIKEY, StringComparison.OrdinalIgnoreCase);
            if (isLicenseApiKey && !ValidateLicense(companyApiKey.ToString(), out actorCompanyId))
            {
                LogWarning($"ValidateLicense failed with message: {detailedMessage} license:{loginDTO.License} username:{loginDTO.UserName}");
                return "Login failed (License)";
            }
            if (!isLicenseApiKey && !ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
            {
                LogWarning($"ValidateCompany failed with message: {detailedMessage} license:{loginDTO.License} username:{loginDTO.UserName}");
                return "Login failed (Company)";
            }

            if (parameterObject.ActorCompanyId != actorCompanyId && !HasCompanyPermission(parameterObject.UserId, actorCompanyId, parameterObject.LicenseId))
            {
                LogWarning($"User ( name:{loginDTO.UserName} userActorCompanyId:{parameterObject.ActorCompanyId} ) has not access to CompanyApiKey company (actorCompanyId:{actorCompanyId}): license:{loginDTO.License}");
                return "User has not permission to CompanyApiKey company";
            }

            var validConnectApikey = ValidateConnectApiKey(connectApiKey);
            if (!validConnectApikey.Success)
            {
                LogWarning($"ValidateConnectApiKey failed with message: {validConnectApikey.ErrorMessage} license:{loginDTO.License} username:{loginDTO.UserName} ");
                return validConnectApikey.ErrorMessage;
            }

            if (user == null)
            {
                LogWarning($"Invalid user license:{loginDTO.License} username:{loginDTO.UserName}");
                return "Invalid User";
            }

            userId = user.UserId;

            return detailedMessage;
        }

        public ActionResult ValidateConnectApiKey(Guid connectApiKey)
        {
            var result = new ActionResult();

            try
            {
                var key = connectApiKey.ToString();
                if (connectApiCache.HasKey(key))
                {
                    return result;
                }

                var apiKeys = SysServiceManager.GetConnectApiKeys();

                if (apiKeys != null && apiKeys.Values.Contains(connectApiKey.ToString()))
                {
                    connectApiCache.Add(key);
                    if (connectApiKey.ToString().Equals(Constants.SoftOneUnknown))
                        Thread.Sleep(10000);
                }
                else
                {
                    LogError($"ConnectUtil.ValidateConnectApiKey fails for key {connectApiKey}");
                    result.Success = false;
                    result.ErrorMessage = "connectApiKey does not exist";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public ConnectHistoryDTO SetupConnectHistoryDTO(TermGroup_IOImportHeadType type, int numberOfItemInImport)
        {
            ConnectHistoryDTO connectHistoryDTO = new ConnectHistoryDTO();

            connectHistoryDTO.ImportedBy = parameterObject.SoeUser.Name;
            connectHistoryDTO.Type = type;
            connectHistoryDTO.NumberOfItemsInImport = numberOfItemInImport;
            connectHistoryDTO.Started = DateTime.Now;
            connectHistoryDTO.Duration = 0;
            connectHistoryDTO.State = SoeEntityState.Active;
            connectHistoryDTO.NumberOfItemsImported = 0;
            connectHistoryDTO.ConnectHistoryRowDTOs = new List<ConnectHistoryRowDTO>();

            return connectHistoryDTO;
        }

        public ConnectHistoryRowDTO SetupConnectHistoryRowDTO(DateTime? created, DateTime? modified, int xeId, string externalKey, string externalValue)
        {
            ConnectHistoryRowDTO connectHistoryRow = new ConnectHistoryRowDTO();

            connectHistoryRow.Created = !modified.HasValue ? created : null;
            connectHistoryRow.Modified = modified;
            connectHistoryRow.XeId = xeId;
            connectHistoryRow.ExternalKey = externalKey;
            connectHistoryRow.ExternalValue = externalValue;

            return connectHistoryRow;
        }

        public ActionResult SaveConnectHistory(ConnectHistoryDTO connectHistoryDTO, int numberOfItemsImported = 0)
        {
            ActionResult result = new ActionResult();

            connectHistoryDTO.Finished = DateTime.Now;
            connectHistoryDTO.NumberOfItemsImported = numberOfItemsImported;

            return result;
        }

        public string CreateToken(string companyApiKey, string connectApiKey, bool isLicenseApiKey, ref int userId)
        {
            if (string.IsNullOrEmpty(companyApiKey))
                return "";
            string token = GenerateToken(companyApiKey.ToLower(), connectApiKey.ToLower(), isLicenseApiKey, ref userId);
            return token;
        }

        public string GenerateToken(string companyApiKey, string connectApiKey, bool isLicenseApiKey, ref int userId)
        {
            string clearToken;
            if (!isLicenseApiKey)
                clearToken = Guid.NewGuid().ToString().Replace("-", "") + "_" + companyApiKey + "_" + DateTime.Now.ToString() + "_" + userId.ToString() + "_" + connectApiKey;
            else
                clearToken = Guid.NewGuid().ToString().Replace("-", "") + "_" + companyApiKey + "_" + DateTime.Now.ToString() + "_" + userId.ToString() + "_" + connectApiKey + "_true";

            string encrypted = Encrypt(Convert.ToBase64String(Encoding.UTF8.GetBytes(clearToken)), companyApiKey);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
        }

        public ActionResult ValidateToken(string companyApiKey, string connectApiKey, string token, ref int userId)
        {
            if (string.IsNullOrEmpty(token))
            {
                LogWarning($"ConnectUtil.ValidateToken failed with empty token for companyApiKey:{companyApiKey}");
                return new ActionResult(false);
            }

            var result = new ActionResult();

            Byte[] bytes = null;
            try
            {
                bytes = Convert.FromBase64String(token);
            }
            catch
            {
                return new ActionResult(false);
            }

            token = Encoding.UTF8.GetString(bytes);
            var decryptedToken = Decrypt(token, companyApiKey);
            if (string.IsNullOrEmpty(decryptedToken))
                decryptedToken = Decrypt(token, AXFOODLICENSEAPIKEY);

            if (string.IsNullOrEmpty(decryptedToken))
            {
                LogWarning($"Decrypt Token failed companyApiKey {companyApiKey} connectApiKey {connectApiKey} token {token}");
                return new ActionResult(false);
            }

            List<string> strings = new List<string>();

            try
            {
                strings = Encoding.UTF8.GetString(Convert.FromBase64String(decryptedToken)).Split('_').ToList();
            }
            catch (Exception ex)
            {
                try
                {
                    decryptedToken = Decrypt(token, AXFOODLICENSEAPIKEY);
                    token = decryptedToken;
                    strings = Encoding.UTF8.GetString(Convert.FromBase64String(decryptedToken)).Split('_').ToList();
                }
                catch (Exception ex2)
                {
                    LogError(ex2, log);
                }
                LogError(ex, log);
            }

            token = decryptedToken;

            string tokenguid = string.Empty;
            string tokenCompanyApiKey = string.Empty;
            string tokenUserId = string.Empty;
            DateTime time = new DateTime();
            string tokenConnectApiKey = string.Empty;
            string tokenIsLicenseApiKey = "false";

            int seq = 1;

            foreach (var st in strings)
            {
                if (seq == 1)
                    tokenguid = st;
                else if (seq == 2)
                    tokenCompanyApiKey = st;
                else if (seq == 3)
                    time = DateTime.Parse(st);
                else if (seq == 4)
                    tokenUserId = st;
                else if (seq == 5)
                    tokenConnectApiKey = st;
                else if (seq == 6)
                    tokenIsLicenseApiKey = st;

                seq++;
            }

            if (!string.IsNullOrEmpty(tokenUserId))
                userId = Convert.ToInt32(tokenUserId);

            if (tokenConnectApiKey.Equals(connectApiKey) && tokenConnectApiKey.Equals(Constants.SoftOneStage))
            {
                result.Success = true;
                result.BooleanValue = false;
                return result;
            }

            bool isLicenseApiKey = false;
            bool.TryParse(tokenIsLicenseApiKey, out isLicenseApiKey);
            if (!isLicenseApiKey && companyApiKey.Equals(tokenCompanyApiKey))
            {
                if (time < DateTime.Now.AddMinutes(-60))
                {
                    result.Success = false;
                    return result;
                }
                else
                {
                    result.Success = true;
                    result.BooleanValue = true;
                    return result;
                }
            }
            else if (isLicenseApiKey && tokenCompanyApiKey.ToLower() == AXFOODLICENSEAPIKEY) //Hardcoded for Axfood. Maybe set this on License.
            {
                if (time < DateTime.Now.AddMinutes(-120))
                {
                    result.ErrorMessage = $"Token has expired. Expired: {DateTime.Now.AddMinutes(-120).ToString()} ";
                    result.Success = false;
                    return result;
                }
                else
                {
                    result.Success = true;
                    result.BooleanValue = true;
                    return result;
                }
            }


            LogWarning($"ConnectUtil.ValidateToken failed for companyApiKey:{companyApiKey}");
            result.Success = false;

            return result;
        }

        public string Encrypt(string token, string key)
        {
            string value = new string(token.Reverse().ToArray());
            string EncryptionKey = "!3nl1t3n3xtr4n4ck3lförd3t4rn0gbÄ2t!" + key.ToLower();
            byte[] clearBytes = Encoding.Unicode.GetBytes(value);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 253, 9, 67, 34, 237, 1, 138, 150, 82, 97, 103, 128, 23, 65, 76, 67, 7, 51, 200, 225, 146, 180, 51, 123, 118, 167, 45, 10, 184, 181, 202, 190 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    value = Convert.ToBase64String(ms.ToArray());
                }
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        CopyTo(msi, gs);
                    }
                    value = Convert.ToBase64String(mso.ToArray());
                }
            }

            //TryDecryt
            if (Decrypt(value, key.ToLower()).Equals(token))
                return value;
            else
                return string.Empty;
        }

        public string Decrypt(string token, string key)
        {
            string value = string.Empty;
            try
            {
                string str = token;

                byte[] bytes = Convert.FromBase64String(str);
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }
                    str = Encoding.UTF8.GetString(mso.ToArray());
                }

                string EncryptionKey = "!3nl1t3n3xtr4n4ck3lförd3t4rn0gbÄ2t!" + key.ToLower();
                str = str.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(str);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 253, 9, 67, 34, 237, 1, 138, 150, 82, 97, 103, 128, 23, 65, 76, 67, 7, 51, 200, 225, 146, 180, 51, 123, 118, 167, 45, 10, 184, 181, 202, 190 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        str = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return new string(str.Reverse().ToArray());
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return value;

        }




        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];
            int cnt;
            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public string ValidateLastSynchDate(string lastSynchDate, out DateTime dateTime)
        {
            string validationMessage = "";

            if (!DateTime.TryParse(lastSynchDate, out dateTime))
                validationMessage = VALIDATION_LASTSYNCHDATE_ERRORMESSAGE;

            return validationMessage;
        }

        public bool ValidateCompany(string companyApiKey, out int companyId)
        {
            companyId = 0;

            int? actorCompanyId = new CompanyManager(null).GetActorCompanyIdFromApiKey(companyApiKey);
            if (actorCompanyId.HasValue && actorCompanyId.Value > 0)
            {
                companyId = actorCompanyId.Value;
                return true;
            }

            return false;
        }

        public bool ValidateLicense(string apiKey, out int companyId)
        {
            companyId = 0;

            if (!apiKey.ToLower().Equals(AXFOODLICENSEAPIKEY, StringComparison.OrdinalIgnoreCase))
                return false;

            companyId = 1292;

            return true;
        }

        public bool IsLicenseApiKey(string apiKey)
        {
            int companyId = 0;
            return ValidateLicense(apiKey, out companyId);
        }

        public bool ValidateDefinition(string importApiKey, int sysImportHeadId, out int? sysDefinitionId, out int? compImportId)
        {
            ImportExportManager iem = new ImportExportManager(null);
            sysDefinitionId = null;
            compImportId = null;
            Guid guid;

            if (!Guid.TryParse(importApiKey, out guid))
                return false;

            var sysDefinition = iem.GetSysImportDefinition(guid);
            var compImport = iem.GetImport(guid);

            if (sysDefinition == null && compImport == null)
                return false;

            if (compImport != null)
            {
                var definition = iem.GetSysImportDefinition(compImport.ImportDefinitionId);

                if (definition.SysImportHeadId.HasValue && definition.SysImportHeadId == sysImportHeadId)
                {
                    compImportId = compImport.ImportId;
                    sysDefinitionId = definition.SysImportDefinitionId;
                    return true;
                }
                else
                {
                    compImportId = null;
                    return false;
                }
            }
            else if (sysDefinition.SysImportHeadId == sysImportHeadId)
            {
                sysDefinitionId = (int?)sysDefinition.SysImportDefinitionId;
                return true;
            }

            return false;
        }

        public bool ValidateSource(int source, out TermGroup_IOSource ioSource)
        {
            ioSource = TermGroup_IOSource.Unknown;

            bool valid = false;
            switch (source)
            {
                case (int)TermGroup_IOSource.TilTid:
                    ioSource = TermGroup_IOSource.TilTid;
                    valid = true;
                    break;
                case (int)TermGroup_IOSource.FlexForce:
                    ioSource = TermGroup_IOSource.FlexForce;
                    valid = false; //Not supported
                    break;
                case (int)TermGroup_IOSource.Connect:
                    ioSource = TermGroup_IOSource.Connect;
                    valid = true;
                    break;
            }
            return valid;
        }

        public List<byte[]> ConvertToList(List<FileInfo> files)
        {
            List<byte[]> contents = new List<byte[]>();
            try
            {
                foreach (var file in files)
                {
                    FileStream fileStream = file.OpenRead();
                    Byte[] array = null;

                    try
                    {
                        //Perferred way to create array
                        BinaryReader br = new BinaryReader(fileStream);
                        long numBytes = fileStream.Length;
                        array = br.ReadBytes((int)numBytes);
                    }
                    catch
                    {
                        //Try this i other fail
                        MemoryStream stream = new MemoryStream();
                        stream.Position = 0;
                        fileStream.CopyTo(stream, 1024);
                        array = stream.ToArray();
                    }
                    contents.Add(array);
                }
            }
            catch (System.IO.IOException)
            {

            }

            return contents;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public ConnectResult ActionResult2ConnectResult(ActionResult actionResult)
        {
            ConnectResult connectResult = new ConnectResult();

            connectResult.Success = actionResult.Success;
            connectResult.SuccessNumber = actionResult.SuccessNumber;
            connectResult.ErrorMessage = actionResult.ErrorMessage;
            connectResult.ErrorNumber = actionResult.ErrorNumber;
            connectResult.IntegerValue = actionResult.IntegerValue;
            connectResult.IntegerValue2 = actionResult.IntegerValue2;

            return connectResult;
        }

        public int CreateCompany(int licenseId, int userId, int templateCompanyId, int companyNr, string name, int? baseSysCurrencyId, int? baseSysEntCurrencyId)
        {
            CompanyManager cm = new CompanyManager(null);
            LicenseManager lm = new LicenseManager(null);
            RoleManager rm = new RoleManager(null);
            UserManager um = new UserManager(null);
            FeatureManager fm = new FeatureManager(null);
            SettingManager sm = new SettingManager(null);
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            AccountManager am = new AccountManager(null);

            var tempCompany = cm.GetCompany(templateCompanyId);
            var license = lm.GetLicense(licenseId);
            int actorCompanyId = 0;

            using (CompEntities entities = new CompEntities())
            {

                Company company = null;
                if (lm.ValidateCompany(license, company).Success)
                {
                    #region Company

                    company = new Company()
                    {
                        CompanyNr = companyNr,
                        Name = name,
                        ShortName = string.Empty,
                        OrgNr = string.Empty,
                        VatNr = string.Empty,
                        Template = false,
                        Global = false,
                        Demo = false,

                        SysCountryId = tempCompany.SysCountryId.HasValue && tempCompany.SysCountryId != (int)TermGroup_Languages.Unknown ? tempCompany.SysCountryId : (int)TermGroup_Languages.Swedish,
                    };

                    #endregion

                    if (cm.AddCompany(company, licenseId).Success)
                    {
                        #region Relations and settings

                        #region Role

                        // Add default role, otherwise users can't be added to company
                        Role role = new Role()
                        {
                            TermId = (int)TermGroup_Roles.Systemadmin,
                        };
                        rm.AddRole(role, company.ActorCompanyId);

                        var user = um.GetUser(userId);
                        // Just connect to default role if company created in samt license                        
                        if (licenseId == user.LicenseId)
                        {
                            // Connect current user to role and new company
                            um.AddUserCompanyRoleMapping(userId, company.ActorCompanyId, role.RoleId, true);
                        }

                        #endregion Role

                        #region Currency

                        // Try to fetch base currency from template company
                        int sysCurrencyId = 0;
                        int sysEntCurrencyId = 0;

                        if (!baseSysCurrencyId.HasValue)
                        {
                            sysCurrencyId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, 0, tempCompany.ActorCompanyId, 0);
                            if (sysCurrencyId <= 0)
                                sysCurrencyId = (int)TermGroup_Currency.SEK;
                        }
                        else
                        {
                            sysCurrencyId = (int)baseSysCurrencyId;
                        }

                        Currency baseCurrency = new Currency()
                        {
                            SysCurrencyId = sysCurrencyId,
                            IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                            UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                        };
                        ccm.AddCurrency(baseCurrency, DateTime.Today, company.ActorCompanyId);

                        if (!baseSysEntCurrencyId.HasValue)
                        {
                            sysEntCurrencyId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, 0, tempCompany.ActorCompanyId, 0);
                            if (sysEntCurrencyId <= 0)
                                sysEntCurrencyId = (int)TermGroup_Currency.SEK;
                        }
                        else
                        {
                            sysEntCurrencyId = (int)baseSysEntCurrencyId;
                        }

                        Currency enterpriseCurrency = new Currency()
                        {
                            SysCurrencyId = sysEntCurrencyId,
                            IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                            UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                        };
                        ccm.AddCurrency(enterpriseCurrency, DateTime.Today, company.ActorCompanyId);

                        //Settings
                        sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, sysCurrencyId, user.UserId, company.ActorCompanyId, 0);
                        sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseEntCurrency, sysEntCurrencyId, user.UserId, company.ActorCompanyId, 0);

                        #endregion Currency

                        #endregion relations and settings

                        #region AccountDim

                        // Add AccountDim std
                        AccountDim accountDim = new AccountDim()
                        {
                            AccountDimNr = Constants.ACCOUNTDIM_STANDARD,
                            Name = GetText(1258, "Konto"),
                            ShortName = "Std",
                            SysSieDimNr = null,
                            MinChar = null,
                            MaxChar = null,
                        };
                        am.AddAccountDim(accountDim, company.ActorCompanyId);

                        #endregion

                        actorCompanyId = company.ActorCompanyId;
                    }
                }
            }

            Guid companyApiKey = Guid.NewGuid();

            sm.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, companyApiKey.ToString(), userId, actorCompanyId, 0);


            return actorCompanyId;
        }

        public AzureStorageDTO GetAzureStorageDTOFromString(string content)
        {
            AzureStorageDTO dto = new AzureStorageDTO();

            try
            {
                dto = (AzureStorageDTO)XmlUtil.XMLToObject(content, dto);
            }
            catch
            {
                // Ignore errors
                // NOSONAR
            }

            return dto;

        }

    }

    public class AzureStorageDTO
    {
        public string ContainerName { get; set; }
        public string FileName { get; set; }
        public Guid guid { get; set; }
        public string CompanyApiKey { get; set; }
    }



    public class Login
    {
        public string license { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class ConnectObject
    {
        public LoginDTO Login { get; set; }
        public EvaluatedSelection EvaluatedSelection { get; set; }
    }

    public class ConnectResult
    {
        public bool Success { get; set; }
        public int SuccessNumber { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
        public int IntegerValue { get; set; }
        public int IntegerValue2 { get; set; }
        public decimal DecimalValue { get; set; }
        public string StringValue { get; set; }
        public bool BooleanValue { get; set; }
        public bool BooleanValue2 { get; set; }
        public DateTime DateTimeValue { get; set; }
        public object Value { get; set; }
        public object Value2 { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<int> Keys { get; set; }
        public List<TimeStampAttendanceGaugeDTO> timeStampAttendanceGaugeDTOs { get; set; }
        public List<IODictionaryTypeValidation> IODictionaryTypeValidations { get; set; }
    }

    public class CompanyWS
    {
        public int CompanyNr { get; set; }
        public string Name { get; set; }
        public string CompanyAPIKey { get; set; }
        public bool? isTemplate { get; set; }
    }

    public class ReportSelectionWS
    {
        public int ActorCompanyId { get; set; }
    }

    public class ReportResult
    {
        public Guid guid { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; }

        public byte[] ReportFile { get; set; }

        public string URL { get; set; }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class IODictionaryTypeValidation
    {
        public IODictionaryType IODictionaryType { get; set; }
        public string NoOrCode { get; set; }
        public string Name { get; set; }

        public bool Validated { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class TokenMemoryCacher
    {
        public ParameterObject GetValue(string key)
        {
            try
            {
                MemoryCache memoryCache = MemoryCache.Default;
                return (ParameterObject)memoryCache.Get(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Add(string key, ParameterObject value)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, value, DateTimeOffset.UtcNow.AddSeconds(60));
        }
    }

    public class ConnectApiKeyMemoryCacher
    {
        public bool HasKey(string key)
        {
            try
            {
                MemoryCache memoryCache = MemoryCache.Default;
                var value = memoryCache.Get(key);
                return value == null ? false : (bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Add(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, true, DateTimeOffset.UtcNow.AddMinutes(60));
        }
    }

    public class CompanyApiKeyMemoryCacher
    {
        /*
        CacheItemPolicy policy = new CacheItemPolicy
        {
            SlidingExpiration = TimeSpan.FromMinutes(60)
        };
        */

        public int GetValue(string key)
        {
            try
            {
                MemoryCache memoryCache = MemoryCache.Default;
                var value = memoryCache.Get(key);
                if (value == null)
                    return 0;
                return (int)value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool Add(string key, int actorCompanyId)
        {
            MemoryCache memoryCache = MemoryCache.Default;

            var value = memoryCache.Get(key);
            if (value == null)
            {
                return memoryCache.Add(key, actorCompanyId, DateTimeOffset.UtcNow.AddMinutes(480));
                //return memoryCache.Add(key, actorCompanyId, policy);
            }

            return false;
        }

    }
}
