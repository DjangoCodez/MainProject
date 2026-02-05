using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.Sales
{
    [RoutePrefix("Internal/Sales/Contract")]
    public class ContractController : ApiBase
    {
        #region Constructor

        public ContractController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="actorCompanyId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Categories")]
        [ResponseType(typeof(List<CategoryDTO>))]
        public IHttpActionResult GetCategories(int actorCompanyId)
        {
            #region Validation
            #endregion

            var categoryManager = new CategoryManager(null);
            var result = categoryManager.GetCategories(SoeCategoryType.Contract, actorCompanyId).ToDTOs(false).ToList();
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

    }
}