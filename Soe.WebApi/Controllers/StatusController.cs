using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.DTO;
using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.WebApi.Controllers
{
    public class StatusController : ApiController
    {
        #region SoftOne Status

        [HttpGet]
        [Route("SoftOneStatus/GetSoftOneStatus")]
        [ResponseType(typeof(SoftOneStatusDTO))]
        public SoftOneStatusDTO Status(Guid guid, string key)
        {
            StatusManager statusManager = new StatusManager();

            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                return statusManager.GetSoftOneStatusDTO(ServiceType.WebApi);
            }

            return null;
        }

        #endregion
    } 
}