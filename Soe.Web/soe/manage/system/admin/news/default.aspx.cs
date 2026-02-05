using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.system.admin.news
{
    public partial class _default : PageBase
    {
        protected int sysNewsId;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            SysNewsManager snm = new SysNewsManager(ParameterObject);

            SoeGrid1.Title = GetText(5427, "Nyheter");

            #endregion

            #region Actions

            int delete;
            if (Int32.TryParse(QS["delete"], out delete) && delete == 1)
            {
                if (Int32.TryParse(QS["newsId"], out sysNewsId) && sysNewsId > 0)
                {
                    if (snm.DeleteSysNews(sysNewsId, SoeUser).Success)
                        RedirectToSelf("", false, true);
                }
            }

            #endregion

            #region Populate

            SoeGrid1.DataSource = snm.GetSysNewsAll();
            SoeGrid1.DataBind();

            #endregion

            #region Navigation

            SoeGrid1.AddRegLink(GetText(5187, "Registrera nyhet"), "edit/",
                Feature.Manage_System, Permission.Modify);

            #endregion
        }
    }
}
