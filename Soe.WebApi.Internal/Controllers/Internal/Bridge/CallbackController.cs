using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using Bridge.Common.Models;
using Newtonsoft.Json;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Bridge/Callback")]
    public class InternalCallbackController : ApiBase
    {
        #region Constructor

        public InternalCallbackController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("BridgeCallback")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult BridgeCallback(Guid companyApiKey, BridgeCallback bridgeCallback)
        {
            LogCollector.LogInfo($"BridgeCallback Test {companyApiKey} {JsonConvert.SerializeObject(bridgeCallback)} ");

            // Parse model from json prop
            // Call whatever manager you need to call
            // Return result

            return Content(HttpStatusCode.OK, new ActionResult());
        }

        #endregion
    }
}