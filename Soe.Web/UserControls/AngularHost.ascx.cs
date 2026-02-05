using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class AngularHost : ControlBase
    {
        public string ModuleName { get; set; }
        public string AppName { get; set; }

        public bool ShowMessages { get; set; }

        protected void Page_Init(object sender, EventArgs e)
        {
            PageBase.HasAngularHost = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ShowMessages = PageBase.HasRolePermission(Feature.Communication_XEmail, Permission.Readonly);
        }

        public bool IsLegacy()
        {
            return Request.QueryString["legacy"] != null;
        }
    }
}