using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Api.Internal.Controllers.WebApiExternal
{
    public class WebApiExternalBase : ApiBase
    {
        public WebApiExternalBase(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }
        protected bool ValidateToken(Guid companyApiKey, Guid connectApiKey, string token, out string validatationResult, out ParameterObject parameterObject)
        {
            var apiManager = new ApiManager(null);
            validatationResult = string.Empty;
            Dictionary<string, IEnumerable<string>> ss = Request.Headers.ToDictionary(a => a.Key, a => a.Value);
            if (apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult, Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value))))
            {
                parameterObject = apiManager.GetParameterObject();
                return true;
            }
            else
            {
                parameterObject = null;
                return false;
            }
        }
    }
}