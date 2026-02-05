using Soe.Api.Internal.Middlewares;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Soe.Api.Internal
{
    [HostAuthentication("Bearer")]
    [SOEAuthorize]
    public class ApiBase : ApiController
    {
        protected const string THREAD = "WebApi.Internal";
        protected WebApiInternalParamObject webApiInternalParamObject;

        public ApiBase(WebApiInternalParamObject webApiInternalParamObject)
        {
            try
            {
                this.webApiInternalParamObject = webApiInternalParamObject;

                //If there is any HttpContext, then we can get the WebApiInternalParamObject from the OwinContext
                if (HttpContext.Current != null)
                    this.webApiInternalParamObject = (WebApiInternalParamObject)HttpContext.Current.GetOwinContext().Environment["Soe.WebApiInternalParamObject"];
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ApiBase", ex);
            }
        }

        public ParameterObject GetParameterObject(int actorCompanyId, int userId, int? roleId = null, string loginName = null)
        {
            CompanyManager cm = new CompanyManager(null);
            UserManager um = new UserManager(null);

            Company company = actorCompanyId > 0 ? cm.GetCompany(actorCompanyId, true) : null;
            User user = userId > 0 ? um.GetUser(userId, loadUserCompanyRole: true) : new User() { LoginName = string.IsNullOrEmpty(loginName) ? this.ToString() : loginName };

            return ParameterObject.Create(user: um.GetSoeUser(actorCompanyId, user, roleId),
                                            company: cm.GetSoeCompany(company),
                                            thread: THREAD,
                                            roleId: roleId,
                                            extendedUserParams: ExtendedUserParams.Create(GetCurrentDirectory(), GetClientIP(), GetRequestUrl()));
        }

        protected string GetCurrentDirectory()
        {
            return HttpContext.Current.Server.MapPath("~");
        }
        
        protected string GetClientIP()
        {
            string ipNr = "";

            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    ipNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipNr))
                        ipNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch
            {
                //Continue on error on IP
            }

            return ipNr;
        }

        private string GetRequestUrl()
        {
            string url = String.Empty;
            if (HttpContext.Current?.Request?.Url != null)
                HttpContext.Current.Request.Url.ToString();
            return url;
        }

        protected void SetLanguage(string cultureCode)
        {
            SetLanguage(new CultureInfo(cultureCode));
        }
        protected void SetLanguage(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }
    }

    public class WebApiInternalParamObject
    {
        public string Token { get; set; }
    }
}