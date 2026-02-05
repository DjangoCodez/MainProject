using RestSharp;
using SoftOne.Soe.Common.Util;
using SoftOne.Status.Shared.DTO.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Data.Util
{
    public static class DataProjectLogCollector
    {
        public static Uri GetURI()
        {
            var url = IsTest() ? "https://logcollectors1Test.azurewebsites.net/" : "https://logcollectors1.azurewebsites.net/";
            Uri uri = new Uri(url);
            return uri;
        }

        public static bool IsTest()
        {
            return Environment.MachineName.Contains("33") || !Environment.MachineName.ToLower().Contains("softone");
        }

        public static Random random;

        public static bool RunInRelease()
        {
            if (random == null) { random = new Random(); }
            if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 18)
                return random.Next(0, 80) == 10;
            else
                return random.Next(0, 20) == 1;
        }

        public static void LogLoadedEntityInExtension(string info = "", [CallerMemberName] string callerName = "")
        {
            var model = new LogModel()
            {
                Source = "SOE.DATA",
                Message = callerName + " " + info,
                Stack = Environment.StackTrace,
                LogLevel = LogLevel.Information
            };

            if (IsTest() || RunInRelease())
                LogModel(model);
        }

        public static RestRequest CreateRequest(string resource, Method method, object obj = null, Dictionary<string, object> parameterDict = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            request.Timeout = TimeSpan.FromMilliseconds(200);

            if (obj != null)
                request.AddJsonBody(obj);

            if (parameterDict != null)
            {
                foreach (var parameter in parameterDict)
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
            }

            request.AddHeader("Token", "e8d7bf57fd1b44a684689cfce813f783");
            return request;
        }

        public static void LogModel(LogModel logModel)
        {
            try
            {
                new RestClient(GetURI(), useClientFactory:true).Execute(CreateRequest("Log/LogModel", Method.Post, logModel, null));
            }
            catch
            {
            }
        }
    }
}
