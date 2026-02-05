using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Text;
using System.Web;

namespace SoftOne.Soe.Web.soe.time.import.api.download
{
    public partial class _default : PageBase
    {
        #region Variables

        private ApiDataManager apdm;
        protected ApiMessage apiMessage;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (apdm == null)
                apdm = new ApiDataManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["apiMessageId"], out int apiMessageId))
                apiMessage = apdm.GetApiMessage(apiMessageId);
            if (apiMessage == null)
                throw new SoeEntityNotFoundException("ApiMessage", this.ToString());

            try
            {
                JToken parsedJson = JToken.Parse(apiMessage.Message.NullToEmpty());
                string beautified = parsedJson.ToString(Formatting.Indented);

                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ContentType = "application/json";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + $"ApiMessage_{apiMessageId}_{apiMessage.Created}.json");
                HttpContext.Current.Response.BinaryWrite(Encoding.UTF8.GetBytes(beautified));

                try
                {
                    HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }
    }
}