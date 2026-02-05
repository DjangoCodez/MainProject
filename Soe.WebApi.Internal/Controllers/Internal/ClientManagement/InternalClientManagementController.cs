using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Business.Core.ClientManagement;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.ClientManagement
{
    [RoutePrefix("Internal/Client/Management")]
    public class InternalClientManagementController : ApiBase
    {
        public InternalClientManagementController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #region Methods

        [HttpPost]
        [Route("Process")]
        [ResponseType(typeof(MultiCompanyApiResponseDTO))]
        public IHttpActionResult ProcessRequest(MultiCompanyApiRequestDTO request)
        {
            var clientmanagementApiManager = new ClientManagementApiManager(null);
            return Content(HttpStatusCode.OK, clientmanagementApiManager.ProcessMultiCompanyRequest(request));
        }

        #endregion
    }
}