using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Azure;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO.SoftOneId;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Login")]
    public class LoginController : WebApiExternalBase
    {
        #region Variables

        LoginManager lm;
        private ConnectUtil connectUtil;

        public object Usermanager { get; private set; }


        #endregion

        #region Constructor

        public LoginController(LoginManager lm, WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.lm = lm;
            this.connectUtil = new ConnectUtil(null);
        }

        #endregion

        #region Validate

        /// <summary>
        /// Get a token to be able to use the api. The token needs to used within 60 seconds. The LoginDTO.Token contains the Token.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Token/")]
        [ResponseType(typeof(SoftOne.Soe.Common.DTO.LoginDTO))]
        public IHttpActionResult GetToken(Guid companyApiKey, Guid connectApiKey, SoftOne.Soe.Common.DTO.LoginDTO loginDTO)
        {
            loginDTO = connectUtil.GetToken(companyApiKey, connectApiKey, loginDTO);
            if (loginDTO != null)
                loginDTO.Password = "";
            return Content(HttpStatusCode.OK, loginDTO);

            //#region Validation

            //string token = string.Empty;
            //ParameterObject parameterObject = ParameterObject.Empty();
            //string validatationResult = string.Empty;
            //UserManager um = new UserManager(null);

            //int userId = 0;
            //int actorCompanyId = 0;

            //if (!string.IsNullOrEmpty(loginDTO.Token))
            //{
            //    string checkTokenResult = connectUtil.ValidateToken(companyApiKey, connectApiKey, loginDTO.Token, out parameterObject, out actorCompanyId, false);

            //    if (!string.IsNullOrEmpty(checkTokenResult))
            //        validatationResult = connectUtil.ValidateUserAndKeys(companyApiKey, connectApiKey, loginDTO, out actorCompanyId, out parameterObject, out userId);
            //    else
            //    {
            //        loginDTO.Message = "Token is valid, session extended";
            //        return Content(HttpStatusCode.OK, loginDTO);
            //    }
            //}
            //else if (!connectApiKey.ToString().Equals(Constants.SoftOneStage))
            //{
            //    validatationResult = connectUtil.ValidateUserAndKeys(companyApiKey, connectApiKey, loginDTO, out actorCompanyId, out parameterObject, out userId);
            //}
            //else
            //{
            //    if (!connectUtil.ValidateCompany(companyApiKey.ToString(), out actorCompanyId))
            //    {
            //        loginDTO.Message = "Invalid companyApiKey";
            //        Thread.Sleep(2000);
            //        return Content(HttpStatusCode.OK, loginDTO);
            //    }
            //    else
            //    {
            //        userId = um.GetAdminUser(actorCompanyId).UserId;
            //        loginDTO.Token = connectUtil.CreateToken(companyApiKey.ToString(), connectApiKey.ToString(), ref userId);
            //        return Content(HttpStatusCode.OK, loginDTO);
            //    }
            //}

            //if (!string.IsNullOrEmpty(validatationResult))
            //{
            //    Thread.Sleep(2000);
            //    loginDTO.Message = "Login failed";
            //    return Content(HttpStatusCode.OK, loginDTO);
            //}

            //#endregion

            //var session = new UserSession();

            //using (CompEntities entities = new CompEntities())
            //{
            //    if (string.IsNullOrEmpty(loginDTO.Token))
            //        session = um.GetLastUserSession(entities, userId, onlyLast24Hours: true);
            //    else
            //        session = um.GetUserSession(entities, loginDTO.Token);

            //    if (session != null)
            //    {
            //        int id = userId;
            //        loginDTO.Token = connectUtil.CreateToken(companyApiKey.ToString(), connectApiKey.ToString(), ref id);
            //        loginDTO.Message = "Login successful, use token within 60 seconds";

            //        session.Token = loginDTO.Token;
            //        session.Logout = DateTime.Now.AddMinutes(1);
            //        int res = entities.SaveChanges();

            //        return Content(HttpStatusCode.OK, loginDTO);
            //    }
            //    else
            //    {
            //        loginDTO.Message = "Login failed";
            //        return Content(HttpStatusCode.OK, loginDTO);
            //    }
            //}
        }

        [HttpPost]
        [Route("Validate/Session")]
        [ResponseType(typeof(ValidateUserResultDTO))]
        public IHttpActionResult ValidateUser(Guid companyApiKey, Guid connectApiKey, ValidateUserRequestDTO validateUserRequestDTO)
        {
            #region Validation

            string token = string.Empty;
            ParameterObject parameterObject = ParameterObject.Empty();
            string validatationResult = string.Empty;
            UserManager um = new UserManager(null);
            ValidateUserResultDTO validateUserResultDTO = new ValidateUserResultDTO();
            validateUserResultDTO.IsValid = false;

            int actorCompanyId = 0;

            if (!string.IsNullOrEmpty(validateUserRequestDTO.SessionToken))
                validatationResult = connectUtil.ValidateToken(companyApiKey, connectApiKey, validateUserRequestDTO.SessionToken, out parameterObject, out actorCompanyId, false);

            if (!string.IsNullOrEmpty(validatationResult))
            {
                Thread.Sleep(2000);
                validateUserResultDTO.IsValid = false;
                validateUserResultDTO.ErrorMessage = validatationResult;
                return Content(HttpStatusCode.OK, validateUserResultDTO);
            }
            else
            {
                validateUserResultDTO.IsValid = true;

                var user = um.GetUser(validateUserRequestDTO.UserId);

                List<UserCompanyRole> userCompanyRoles = um.GetUserCompanyRolesByUser(user.UserId);

                if (userCompanyRoles.Select(i => i.ActorCompanyId).Contains(actorCompanyId))
                {
                    if (validateUserRequestDTO.GetExtendedInformation && validateUserRequestDTO.UserId != 0)
                    {
                        if (user != null)
                        {
                            validateUserResultDTO.Name = user.Name;
                            validateUserResultDTO.Email = user.Email;

                        }
                    }

                    LanguageManager lm = new LanguageManager(parameterObject);
                    validateUserResultDTO.Language = lm.GetSysLanguageCode(parameterObject != null && parameterObject.SoeUser != null && parameterObject.SoeUser.LangId.HasValue ? parameterObject.SoeUser.LangId.Value : (int)TermGroup_Languages.Swedish);

                    return Content(HttpStatusCode.OK, validateUserResultDTO);
                }
                else
                {
                    Thread.Sleep(2000);
                    validateUserResultDTO.IsValid = false;
                    validateUserResultDTO.ErrorMessage = "Invalid UserId";
                    return Content(HttpStatusCode.OK, validateUserResultDTO);
                }
            }

            #endregion
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/LicenseAndUser/{connectApiKey}/{userName}/{licenseNr}")]
        [ResponseType(typeof(ValidateUserResultDTO))]
        public IHttpActionResult ValidateUserAndLicense(string connectApiKey, string userName, string licenseNr)
        {
            #region Validation

            ValidateUserResultDTO validateUserResultDTO = new ValidateUserResultDTO();
            validateUserResultDTO.IsValid = false;
            UserManager userManager = new UserManager(null);
            var user = userManager.GetUserOnLicense(licenseNr, userName);

            if (user == null)
            {
                Thread.Sleep(3000);
                return Content(HttpStatusCode.OK, validateUserResultDTO);
            }
            else
            {
                validateUserResultDTO.IsValid = true;
            }

            LogoManager logoManager = new LogoManager(null);
            var logo = logoManager.GetDefaultCompanyLogo(user.DefaultActorCompanyId.Value);
            if (logo != null)
            {
                BlobUtil blobUtil = new BlobUtil();
                blobUtil.Init("templogo");
                Guid guid = Guid.NewGuid();
                blobUtil.UploadData(guid, logo.Logo, licenseNr, "jpg");
                validateUserResultDTO.LogotypeUrl = blobUtil.GetDownloadLink(guid.ToString());
            }

            LanguageManager lm = new LanguageManager(null);
            validateUserResultDTO.Language = lm.GetSysLanguageCode(user.LangId.HasValue ? user.LangId.Value : (int)TermGroup_Languages.Swedish);

            return Content(HttpStatusCode.OK, validateUserResultDTO);

            #endregion
        }


        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/UserInfo/{connectApiKey}/{SoftOneIdSuperKey}/{idUserGuid}")]
        [ResponseType(typeof(UserInfoDTO))]
        public IHttpActionResult GetUserInfo(string connectApiKey, string SoftOneIdSuperKey, string idUserGuid, bool checkMissingMandatoryInformation = false)
        {
            #region ValidateSoftOneIdSuperKey

            //TODO

            #endregion

            #region UserInfo

            UserManager userManager = new UserManager(null);
            UserInfoDTO userInfoDTO = userManager.GetUserInfoDTO(Guid.Parse(idUserGuid), checkMissingMandatoryInformation: checkMissingMandatoryInformation);

            return Content(HttpStatusCode.OK, userInfoDTO);

            #endregion
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/UserInfos/{connectApiKey}/{SoftOneIdSuperKey}/")]
        [ResponseType(typeof(UserInfoDTO))]
        public IHttpActionResult GetUserInfos(string connectApiKey, string SoftOneIdSuperKey, bool checkMissingMandatoryInformation = false)
        {
            #region UserInfo

            try
            {
                UserManager userManager = new UserManager(null);

                using (CompEntities entities = new CompEntities())
                {
                    int? sysCompDbId = CompDbCache.Instance.SysCompDbId;
                    string url = sysCompDbId.HasValue && sysCompDbId != 0 ? SysCompanyConnector.GetWebUri(sysCompDbId.Value) : string.Empty;

                    List<UserInfoDTO> users = (from u in entities.User
                                                    .Include("license")
                                               where u.State == (int)SoeEntityState.Active &&
                                               u.License.State == (int)SoeEntityState.Active &&
                                               u.idLoginGuid.HasValue
                                               select new UserInfoDTO
                                               {
                                                   LicenseGuid = u.License.LicenseGuid,
                                                   UserId = u.UserId,
                                                   LicenseNr = u.License.LicenseNr,
                                                   LicenseId = u.License.LicenseId,
                                                   LicenseName = u.License.Name,
                                                   SysCompDbId = sysCompDbId.HasValue ? sysCompDbId.Value : 0,
                                                   SysServerId = u.License.SysServerId.HasValue ? u.License.SysServerId.Value : 0,
                                                   IdLoginGuid = u.idLoginGuid.Value,
                                                   Email = u.Email,
                                                   Url = url,
                                                   MobilePhone = u.EstatusLoginId,
                                                   SysLanguageId = u.LangId.HasValue ? u.LangId.Value : Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT,
                                                   UsernameInGo = u.LoginName
                                               }).ToList();

                    if (sysCompDbId.HasValue && sysCompDbId.Value == 4)
                        users = users.Where(w => w.LicenseNr == "100").ToList();

                    if (checkMissingMandatoryInformation)
                    {
                        foreach (var userInfoDTO in users)
                            userManager.AddMandatoryInformationToUserInfoDTO(userInfoDTO, userInfoDTO.UserId);
                    }

                    return Content(HttpStatusCode.OK, users);
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "GetUserInfos failes");
                return Content(HttpStatusCode.InternalServerError, "Failed");
            }

            #endregion
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/UserInfo/{connectApiKey}/{SoftOneIdSuperKey}/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveMandatoryInformationFromUserInfoDTO(string connectApiKey, string SoftOneIdSuperKey, UserInfoDTO userInfo)
        {
            #region ValidateSoftOneIdSuperKey

            //TODO

            #endregion

            #region UserInfo

            UserManager userManager = new UserManager(null);

            return Content(HttpStatusCode.OK, userManager.SaveMandatoryInformationFromUserInfoDTO(userInfo));

            #endregion
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/UserInfoStart/{connectApiKey}/{IdSuperKey}")]
        [ResponseType(typeof(List<UserInfoStartDTO>))]
        public IHttpActionResult GeUserInfoStartDTOs(string connectApiKey, string IdSuperKey)
        {
            #region ValidateSoftOneIdSuperKey

            //TODO

            #endregion

            #region UserInfo

            UserManager userManager = new UserManager(null);

            using (CompEntities entities = new CompEntities())
            {
                int? sysCompDbId = CompDbCache.Instance.SysCompDbId;
                string url = sysCompDbId.HasValue && sysCompDbId != 0 ? SysCompanyConnector.GetWebUri(sysCompDbId.Value) : string.Empty;

                List<UserInfoStartDTO> users = (from u in entities.User
                                                .Include("license")
                                                where u.State == (int)SoeEntityState.Active &&
                                                u.License.State == (int)SoeEntityState.Active &&
                                                u.idLoginGuid.HasValue
                                                select new UserInfoStartDTO
                                                {
                                                    UserName = u.LoginName,
                                                    Password = null,
                                                    LicenseNr = u.License.LicenseNr,
                                                    LicenseId = u.License.LicenseId,
                                                    SysCompDbId = sysCompDbId.HasValue ? sysCompDbId.Value : 0,
                                                    SysServerId = u.License.SysServerId.HasValue ? u.License.SysServerId.Value : 0,
                                                    IdLoginGuid = u.idLoginGuid.Value,
                                                    Email = u.Email,
                                                    Url = url,
                                                    PhoneNumber = u.EstatusLoginId,
                                                    sysServerId = u.License.SysServerId.HasValue ? u.License.SysServerId.Value : 0
                                                }).ToList();

                return Content(HttpStatusCode.OK, users);
            }

            #endregion
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("IdLoginGuid/UserLinkConnectionKey")]
        public IHttpActionResult ChangeIdLoginGuid([FromBody] IdLoginConfidential idLoginConfidential)
        {
            ActionResult result = new ActionResult(false);
            result.ErrorMessage = $"UserLinkConnectionKey ChangeIdLoginGuid() second validation idLoginConfidential == null {idLoginConfidential == null} ";

            if (idLoginConfidential != null)
            {

                if (idLoginConfidential.IdLoginGuid != Guid.Empty && !string.IsNullOrEmpty(idLoginConfidential.Confidential) && !string.IsNullOrEmpty(idLoginConfidential.ConfidentialSecond))
                {
                    ActorManager ac = new ActorManager(null);
                    result = ac.TryChangingGuid(idLoginConfidential.Confidential, idLoginConfidential.LicenseId.ToString(), idLoginConfidential.Email, idLoginConfidential.IdLoginGuid);
                    return Content(HttpStatusCode.OK, result);
                }
                else
                {
                    result.ErrorMessage += $"idLoginConfidential IdLoginGuid {idLoginConfidential.IdLoginGuid} Confidential {idLoginConfidential.Confidential} ConfidentialSecond {idLoginConfidential.ConfidentialSecond} ";
                }
            }
            return Content(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Validate/ParameterClaimsObject/{idSuperKey}/{IdLoginGuid}")]
        [ResponseType(typeof(ParameterClaimsObjectDTO))]
        public IHttpActionResult GetParameterClaimsObjectDTO(string idSuperKey, Guid idLoginGuid)
        {
            #region ValidateSoftOneIdSuperKey

            //TODO

            #endregion

            #region ParameterClaimsObjectDTO

            UserManager userManager = new UserManager(null);
            ParameterClaimsObjectDTO parameterClaimsObjectDTO = userManager.GetParameterClaimsObjectDTO(idLoginGuid, out _);

            return Content(HttpStatusCode.OK, parameterClaimsObjectDTO);


            #endregion
        }

        #endregion

        #region LicenseLoginInfo

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("LicenseLoginInfos/All")]
        [ResponseType(typeof(List<LicenseLoginInfo>))]
        public IHttpActionResult GetLicenseLoginInfos()
        {
            UserManager userManager = new UserManager(null);
            return Content(HttpStatusCode.OK, userManager.GetAllLicenseLogins());
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("LicenseLoginInfos/One")]
        [ResponseType(typeof(List<LicenseLoginInfo>))]
        public IHttpActionResult GetLicenseLoginInfos(Guid idLoginGuid)
        {
            UserManager userManager = new UserManager(null);
            return Content(HttpStatusCode.OK, userManager.GetLicenseLogins(idLoginGuid));
        }

        #endregion

        #region ManadatoryInformation

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("ManadatoryInformation/Guids/")]
        [ResponseType(typeof(List<Guid>))]
        public IHttpActionResult GetIdLoginGuidsWhereMandatoryInformationSetting(string connectApiKey, string SoftOneIdSuperKey)
        {
            UserManager userManager = new UserManager(null);
            return Content(HttpStatusCode.OK, userManager.GetIdLoginGuidsWhereMandatoryInformationSetting());
        }

        #endregion

        #region Health

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Health")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetHealth(string connectApiKey, Guid companyApiKey)
        {
            try
            {
                if (string.IsNullOrEmpty(SoftOneStatusConnector.GetApiInternal(ConfigurationSetupUtil.GetCurrentSysCompDbId())))
                {
                    return Content(HttpStatusCode.PreconditionFailed, "Status unavailable");
                }

                if (!SoftOneIdConnector.CheckHealth())
                {
                    return Content(HttpStatusCode.PreconditionFailed, "Login unavailable");
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "GetHealth failed");
                return Content(HttpStatusCode.InternalServerError, "Failed with exception");
            }

            return Content(HttpStatusCode.OK, "OK");
        }


        #endregion
    }
}
