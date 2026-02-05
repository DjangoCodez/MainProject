using System;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;

namespace SoftOne.Soe.Web.soe.manage.system.admin.xearticles
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            int sysPermissionId;
            if (!Int32.TryParse(QS["permission"], out sysPermissionId))
                throw new SoeQuerystringException("permission", this.ToString());

            #endregion

            #region Populate

            //Set UserControl parameters
            FeaturePermissionTree.CurrentFeature = this.Feature;
            FeaturePermissionTree.FeatureType = SoeFeatureType.SysXEArticle;
            FeaturePermissionTree.Permission = (Permission)sysPermissionId;
            FeaturePermissionTree.SubTitle = GetText(1604, "för") + " " + GetText(1996, "SoftOne Artiklar");

            // Clear permission cache if permission tree is saved
            if (IsPostBack)
            {
                RemoveAllOutputCacheItems(Request.Url.AbsolutePath);
            }

            #endregion

            #region Navigation

            if (sysPermissionId == (int)Permission.Readonly)
            {
                FeaturePermissionTree.AddLink(GetText(1080, "Skrivbehörighet"), "?&permission=" + (int)Permission.Modify,
                    Feature.Manage_System, Permission.Readonly);
            }
            else if (sysPermissionId == (int)Permission.Modify)
            {
                FeaturePermissionTree.AddLink(GetText(1077, "Läsbehörighet"), "?&permission=" + (int)Permission.Readonly,
                    Feature.Manage_System, Permission.Readonly);
            }

            #endregion
        }
    }
}
