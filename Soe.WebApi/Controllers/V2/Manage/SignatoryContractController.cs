namespace Soe.WebApi.V2.Manage
{

    using Soe.WebApi.Controllers;
    using SoftOne.Soe.Business.Core;
    using SoftOne.Soe.Common.DTO.SignatoryContract;
    using SoftOne.Soe.Common.Util;
    using System.Collections.Generic;
    using System.Net;
    using System.Web.Http;

    [RoutePrefix("V2/Manage/Preferences/Registry/SignatoryContract")]
    public class SignatoryContractController : SoeApiController
    {
        #region Variables

        private readonly SignatoryContractManager scm;
        private readonly FeatureManager fm;

        #endregion

        #region Constructor

        public SignatoryContractController(
            SignatoryContractManager signatoryContractManager,
            FeatureManager featureManager)
        {
            this.scm = signatoryContractManager;
            this.fm = featureManager;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("Grid/{signatoryContractId:int?}")]
        public IHttpActionResult GetSignatoryContractsGrid(int? signatoryContractId = null)
        {
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract, 
                Permission.Readonly,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (isAuthorized)
            {
                List<SignatoryContractGridDTO> signatoryContracts
                    = scm.GetSignatoryContractsGrid(signatoryContractId);

                return Content(HttpStatusCode.OK, signatoryContracts);
            }
            else
            {
                return Content(
                    HttpStatusCode.Forbidden, 
                    new List<SignatoryContractGridDTO>());
            }

        }


        [HttpGet]
        [Route("{signatoryContractId}")]
        public IHttpActionResult GetSignatoryContract(int signatoryContractId)
        {
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract,
                Permission.Readonly,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (isAuthorized)
            {
                SignatoryContractDTO signatoryContract
                    = scm.GetSignatoryContract(signatoryContractId);

                return Content(HttpStatusCode.OK, signatoryContract);
            }
            else
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    new SignatoryContractDTO());
            }

        }

        [HttpGet]
        [Route("{signatoryContractId}/SubContract")]
        public IHttpActionResult GetSignatoryContractSubContract(int signatoryContractId)
        {
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract,
                Permission.Readonly,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (isAuthorized)
            {
                List<SignatoryContractDTO> signatoryContracts
                    = scm.GetSignatoryContractSubContract(signatoryContractId);

                return Content(HttpStatusCode.OK, signatoryContracts);
            }
            else
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    new SignatoryContractDTO());
            }

        }

        [HttpGet]
        [Route("PermissionTerms/{signatoryContractId}")]
        public IHttpActionResult GetPermissionTerms(int signatoryContractId)
        {
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract,
                Permission.Readonly,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (isAuthorized)
            {
                List<SignatoryContractPermissionEditItem> permissionEditItems
                    = scm.GetPermissionTerms(signatoryContractId);
                return Content(HttpStatusCode.OK, permissionEditItems);
            }
            else
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    new SignatoryContractDTO());
            }

        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSignatoryContract(
            SignatoryContractDTO signatoryContract)
        {
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract,
                Permission.Modify,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (!isAuthorized)
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    new SignatoryContractDTO());
            }
            else if (!ModelState.IsValid)
            {
                return Error(
                    HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                ActionResult result = scm.SaveSignatoryContract(
                    signatoryContract);
                return Content(HttpStatusCode.OK, result);

            }
        }

        [HttpPost]
        [Route("{signatoryContractId}/Revoke")]
        public IHttpActionResult RevokeSignatoryContract(
            int signatoryContractId,
            SignatoryContractRevokeDTO item)
        {
            item.SignatoryContractId = signatoryContractId;
            bool isAuthorized = fm.HasRolePermission(
                Feature.Manage_Preferences_Registry_SignatoryContract,
                Permission.Modify,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (!isAuthorized)
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    new SignatoryContractDTO());
            }
            else if (!ModelState.IsValid)
            {
                return Error(
                    HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                ActionResult result = scm.RevokeSignatoryContract(
                    signatoryContractId,
                    item);
                return Content(HttpStatusCode.OK, result);

            }
        }


        [HttpPost]
        [Route("Authorize")]
        public IHttpActionResult SignatoryContractAuthorize(
            AuthorizeRequestDTO authorizeRequest)
        {
            if (ModelState.IsValid 
                && authorizeRequest != null)
            {
                GetPermissionResultDTO permissionResult
                    = scm.Authorize(authorizeRequest);
                return Content(HttpStatusCode.OK, permissionResult);
            }
            else 
            {
                return Error(
                    HttpStatusCode.BadRequest, ModelState, null, null);

            }

        }

        [HttpPost]
        [Route("Authenticate")]
        public IHttpActionResult SignatoryContractAuthenticate(
            AuthenticationResponseDTO authenticationResponse)
        {
            AuthenticationResultDTO authenticationResult = 
                scm.ValidateAuthenticationResponse(authenticationResponse);

            ActionResult result = new ActionResult
            {
                Success = authenticationResult.Success,
                StringValue = authenticationResult.Message
            };

            return Content(HttpStatusCode.OK, result);
        }

        #endregion

    }
}
