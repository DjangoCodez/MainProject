using Soe.WebApi.Filters;
using Soe.WebApi.Middlewares;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.Controllers
{
    [LanguageHeaderFilter]
    [HostAuthentication("Bearer")]
    [SOEAuthorize]
    public class SoeApiController : ApiController
    {
        private static Random _random;

        private Random random
        {
            get
            {
                if (_random == null)
                    _random = new Random();

                return _random;
            }
        }

        protected IHttpActionResult Error(HttpStatusCode code, ModelStateDictionary modelState, string translationKey, string errorMessage)
        {
            if (String.IsNullOrEmpty(errorMessage))
                errorMessage = GetModelStateErrors(modelState);

            // Log to syslog
            SysLogManager slm = new SysLogManager(ParameterObject);
            slm.AddSysLogErrorMessage("", "WebApi", errorMessage, Request.RequestUri.ToString());

#if DEBUG
            return ResponseMessage(Request.CreateResponse(code, new WebApiError(translationKey, errorMessage)));
#else
            return ResponseMessage(Request.CreateResponse(code, new WebApiError(translationKey, null)));
#endif
        }

        protected static readonly SoeMonitor monitor = new SoeMonitor();

        public ParameterObject ParameterObject
        {
            get
            {
                return (ParameterObject)HttpContext.Current.GetOwinContext().Environment["Soe.ParameterObject"];
            }
        }
        public int LicenseId
        {
            get
            {
                return ParameterObject.LicenseId;
            }
        }
        public int ActorCompanyId
        {
            get
            {
                return ParameterObject.ActorCompanyId;
            }
        }
        public Guid? CompanyGuid
        {
            get
            {
                return ParameterObject.CompanyGuid;
            }
        }
        public int RoleId
        {
            get
            {
                return ParameterObject.RoleId;
            }
        }
        public int UserId
        {
            get
            {
                return ParameterObject.UserId;
            }
        }

        protected string GetModelStateErrors(ModelStateDictionary modelState)
        {
            string messages = String.Empty;
            foreach (ModelState value in modelState.Values)
            {
                foreach (ModelError err in value.Errors)
                {
                    string message = String.Empty;
                    if (!String.IsNullOrEmpty(err.ErrorMessage))
                        message = err.ErrorMessage;
                    else if (err.Exception != null && !String.IsNullOrEmpty(err.Exception.Message))
                        message = err.Exception.Message;

                    if (!String.IsNullOrEmpty(message))
                    {
                        if (!String.IsNullOrEmpty(messages))
                            messages += ", ";
                        messages += message;
                    }
                }
            }

            return messages;
        }

        protected DateTime? BuildDateTimeFromString(string dateString, bool clearTime, DateTime? defaultDateTime = null)
        {
            return Util.CalendarUtility.BuildDateTimeFromString(dateString, clearTime, defaultDateTime);
        }

        protected List<GenericType> GetTermGroupContent(TermGroup termGroup, bool addEmptyRow = false, bool skipUnknown = false, bool sortById = false)
        {
            return TermCacheManager.Instance.GetTermGroupContent(termGroup, addEmptyRow: addEmptyRow, skipUnknown: skipUnknown, sortById: sortById);
        }

        protected bool IsDuplicateRequest(string key, int seconds, int sleep = 0)
        {
            Thread.Sleep(random.Next(1, 15));

            bool? value = BusinessMemoryCache<bool?>.Get(key);

            if (value.HasValue && value.Value)
            {
                if (sleep > 0)
                    Thread.Sleep(sleep);
                return value.Value;
            }

            BusinessMemoryCache<bool?>.Set(key, true, seconds);

            return false;
        }

        #region Language

        protected string GetText(int sysTermId, string text)
        {
            return TermCacheManager.Instance.GetText(sysTermId, (int)TermGroup.General, text, Thread.CurrentThread.CurrentCulture.Name);
        }

        protected void SetLanguage(string cultureCode)
        {
            SetLanguage(new CultureInfo(cultureCode));
        }
        protected void SetLanguage(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        #endregion
    }
}