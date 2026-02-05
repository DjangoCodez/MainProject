using System;
using System.Threading.Tasks;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.import.xeconnect
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_XEConnect;
            try
            {
                Task.Run(() => FileUtil.DeleteOldFiles(Constants.SOE_CRGEN_PATH, DateTime.Now.AddHours(-12)));
                Task.Run(() => FileUtil.DeleteOldFiles(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL, DateTime.Now.AddHours(-12)));
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }            
            
            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/xeconnect/default.aspx");
        }
    }
}