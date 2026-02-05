using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Text;
using System.Web;

namespace SoftOne.Soe.Web.soe.manage.support.logs.edit.download
{
    public partial class _default : PageBase
    {
        #region Variables

        private SysLogManager slm;
        protected SysLogDTO sysLog;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (slm == null)
                slm = new SysLogManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["sysLogId"], out int sysLogId))
                sysLog = slm.GetSysLog(sysLogId).ToDTO();
            if (sysLog == null)
                throw new SoeEntityNotFoundException("SysLog", this.ToString());

            try
            {
                string json = JsonConvert.SerializeObject(sysLog, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
                JToken parsedJson = JToken.Parse(json.NullToEmpty());
                string beautified = parsedJson.ToString(Formatting.Indented);

                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ContentType = "application/json";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + $"SysLog_{sysLogId}_{sysLog.Date}.json");
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