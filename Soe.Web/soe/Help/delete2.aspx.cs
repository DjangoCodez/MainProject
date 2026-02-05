using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.soe.Help
{
    public partial class _default : SoePageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            //this.Feature  TODO
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Add Silverlight control
            Dictionary<string, string> initParams = new Dictionary<string, string>();
            initParams.Add("actorCompanyId", SoeCompany.ActorCompanyId.ToString());
            initParams.Add("userId", SoeUser.UserId.ToString());
                                                                        //Why doesn't new Unit("100%") work for Height??
            SLHost.Initialize("HelpTextEdit", initParams, new Unit("100%"), new Unit(800));
        }
    }
}