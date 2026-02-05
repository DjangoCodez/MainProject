using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe
{
    public partial class bootstrap : MasterPageBase
    {
        private bool useAngularSpaClass { get; set; }
        private bool useCollapsedMenu { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            useAngularSpaClass = PageBase.UseAngularSpa;

            SettingManager sm = new SettingManager(PageBase.ParameterObject);
            useCollapsedMenu = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.UseCollapsedMenu, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
        }

        public string MainViewClass()
        {
            // In Angular SPA we use a newer Bootstrap version and need to have some different styling
            var classes = useAngularSpaClass ? "container-fluid bootstrap-master-nx" : "container-fluid bootstrap-master";
            return IsLegacy() ? $"{classes} legacy" : classes;
        }

        public string ContainerClass()
        {
            // In AngularSpa we use a newer Bootstrap version and need to have some different styling
            return "container-fluid main-container"; // Soe.Angular.Spa
        }

        public string CollapsedMenu()
        {
            return useCollapsedMenu ? "collapsed-menu" : "";
        }

        public bool IsLegacy()
        {
            return Request.QueryString["legacy"] != null;
        }
    }
}
