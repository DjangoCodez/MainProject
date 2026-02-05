using System;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Util;

namespace SoftOne.Soe.Web.soe.communication.xemail
{
    public partial class _default : PageBase
    {
        protected SoeModule TargetSoeModule = SoeModule.None;
        protected Feature FeatureEdit = Feature.None;
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Communication_XEmail;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Optional paramters
            string guid = QS["downloadfile"];
            if (!String.IsNullOrEmpty(guid))
            {
                ExportUtil.DownloadAttachment(Convert.ToInt32(guid));
            }          
        }
    }
}