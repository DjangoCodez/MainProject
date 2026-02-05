using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web
{
    public partial class setpagestatus : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int feat;
            if (int.TryParse(QS["sysfeature"], out feat))
            {
                int type;
                if (int.TryParse(QS["sitetype"], out type))
                {
                    int status;
                    if (int.TryParse(QS["status"], out status))
                    {
                        Feature sysFeature = (Feature)feat;
                        TermGroup_SysPageStatusSiteType siteType = (TermGroup_SysPageStatusSiteType)type;
                        TermGroup_SysPageStatusStatusType statusType = (TermGroup_SysPageStatusStatusType)status;

                        GeneralManager gm = new GeneralManager(ParameterObject);
                        gm.SetSysPageStatus(sysFeature, siteType, statusType);
                    }
                }
            }

            Response.Redirect(Request.UrlReferrer.ToString());
        }
    }
}
