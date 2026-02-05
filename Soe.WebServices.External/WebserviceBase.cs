using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Net;
using System.Web;
using System.Web.Services;

namespace Soe.WebServices.External
{
    public class WebserviceBase : WebService
    {
        #region Constants

        protected const string THREAD = "WebService";
        protected const string VALIDATION_CREDENTIAL_ERRORMESSAGE = "Security credentials not valid";
        protected const string VALIDATION_APIKEY_ERRORMESSAGE = "Invalid API key";
        protected const string VALIDATION_LASTSYNCHDATE_ERRORMESSAGE = "Invalid date format";

        #endregion

        #region Ctor

        public WebserviceBase()
        {
            SysServiceManager sysServiceManager = new SysServiceManager(null);
            SetupTermCache();
        }

        #endregion

        #region Public methods

        public void SetupTermCache()
        {
            //Will only load the cache if it doesnt exist
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, THREAD);
        }

        #endregion

        #region Protected methods


        protected UserParameters GetUserParametersObject()
        {
            if (HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.UserParameters") && HttpContext.Current.GetOwinContext().Environment["Soe.UserParameters"] != null)
                return (UserParameters)(HttpContext.Current.GetOwinContext().Environment["Soe.UserParameters"]);
            else
                return null;
        }

        protected ParameterObject GetParameterObject(int actorCompanyId = 0, int userId = 0, int roleId = 0)
        {
            if (HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.ParameterObject") && HttpContext.Current.GetOwinContext().Environment["Soe.ParameterObject"] != null)
            {
                var parameterObject = (ParameterObject)(HttpContext.Current.GetOwinContext().Environment["Soe.ParameterObject"]);
                if (parameterObject != null)
                {
                    parameterObject.SetExtendedUserParams(ExtendedUserParams.Create(GetCurrentDirectory(), GetClientIP(), GetRequestUrl(), GetUserEnvironmentInfo()));
                    return parameterObject;
                }
            }

            Company company = actorCompanyId > 0 ?  new CompanyManager(null).GetCompany(actorCompanyId, true) : null;
            User user = userId > 0 ? new UserManager(null).GetUser(userId) : new User() { LoginName = this.ToString() };

            return ParameterObject.Create(user: user.ToDTO(),
                                          company: company.ToCompanyDTO(),
                                          roleId: roleId,
                                          thread: THREAD,
                                          extendedUserParams: ExtendedUserParams.Create(GetCurrentDirectory(), GetClientIP(), GetRequestUrl(), GetUserEnvironmentInfo()));
        }

        protected string GetCurrentDirectory()
        {
            return HttpContext.Current.Server.MapPath("~");
        }
        protected string GetUserEnvironmentInfo()
        {
            return GetHostInfo() + Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR + GetClientIP();
        }
        protected string GetHostIP()
        {
            string ipNr = "";

            IPHostEntry hostEntry = Dns.GetHostEntry(GetHostName());
            if (hostEntry != null)
            {
                IPAddress[] ipHostEntry = hostEntry.AddressList;
                ipNr = ipHostEntry[ipHostEntry.Length - 1].ToString();
            }
            return ipNr;
        }
        protected string GetHostName()
        {
            return Dns.GetHostName();
        }
        protected string GetHostInfo()
        {
            return $"{GetHostIP()}_{GetHostName()}";
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
        private String GetRequestUrl()
        {
            string url = String.Empty;
            if (HttpContext.Current?.Request?.Url != null)
                HttpContext.Current.Request.Url.ToString();
            return url;
        }

        #endregion

        #region Logging

        protected void LogError(Exception ex)
        {
            SysLogManager slm = new SysLogManager(GetParameterObject());
            slm.AddSysLogErrorMessage(Environment.MachineName, THREAD, ex);
        }

        protected void LogError(string message)
        {
            SysLogManager slm = new SysLogManager(GetParameterObject());
            slm.AddSysLogErrorMessage(Environment.MachineName, THREAD, message);
        }

        protected void LogWarning(string message)
        {
            SysLogManager slm = new SysLogManager(GetParameterObject());
            slm.AddSysLogWarningMessage(Environment.MachineName, THREAD, message);
        }

        protected void LogInfo(string message)
        {
            SysLogManager slm = new SysLogManager(GetParameterObject());
            slm.AddSysLogInfoMessage(Environment.MachineName, THREAD, message);
        }

        #endregion
    }
}