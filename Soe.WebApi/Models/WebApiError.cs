using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Soe.WebApi.Models
{
    public class WebApiError
    {
        public WebApiError(string translationKey, string message)
        {
            TranslationKey = translationKey;
            Message = message;
        }

        public string TranslationKey { get; private set; }
        public string Message { get; private set; }
    }
}