using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysLogConnector : SysConnectorBase
    {
        #region Ctor
        public SysLogConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public static ActionResult SaveErrorMessage(string errorMessage)
        {
            LogModel logModel = new LogModel()
            {
                LogLevel = LogLevel.Error,
                Message = errorMessage,
                Source = "SOE",
                Stack = ""
            };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 2000);
                RestResponse response = client.Execute(CreateRequest("System/Log/ErrorMessage/LogModel", Method.Post, logModel, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                return new ActionResult(ex, "SaveErrorMessage");
            }
        }

        public static ActionResult SaveWarningMessage(string message)
        {
            LogModel logModel = new LogModel()
            {
                LogLevel = LogLevel.Warning,
                Message = message,
                Source = "SOE",
                Stack = ""
            };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 2000);
                RestResponse response = client.Execute(CreateRequest("System/Log/infoMessage/LogModel", Method.Post, logModel, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                return new ActionResult(ex, "SaveErrorMessage");
            }
        }

        public static ActionResult SaveInfoMessage(string message)
        {
            LogModel logModel = new LogModel()
            {
                LogLevel = LogLevel.Information,
                Message = message,
                Source = "SOE",
                Stack = ""
            };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 2000);
                RestResponse response = client.Execute(CreateRequest("System/Log/infoMessage/LogModel", Method.Post, logModel, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                return new ActionResult(ex, "SaveErrorMessage");
            }
        }
    }
}
