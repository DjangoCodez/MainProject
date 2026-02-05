using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/Billing/Stock")]
    public class StockController : WebApiExternalBase
    {
        #region Constructor

        public StockController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
         
        }

        #endregion

        #region Methods

        #region Stocks

        /// <summary>
        /// Get products/Articles
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Stocks")]
        [ResponseType(typeof(List<StockIODTO>))]
        public IHttpActionResult Stocks(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Stock, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var stocks = importExportManager.GetStocks(apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, stocks);
        }
        #endregion

        #endregion

    }
}