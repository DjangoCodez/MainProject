using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class hasComments : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? companyId = string.IsNullOrEmpty(QS["c"]) ? null : (int?)Convert.ToInt32(QS["c"]);
            int? roleId = string.IsNullOrEmpty(QS["r"]) ? null : (int?)Convert.ToInt32(QS["r"]);
            int? userId = string.IsNullOrEmpty(QS["u"]) ? null : (int?)Convert.ToInt32(QS["u"]);
            int featureId = Convert.ToInt32(QS["f"]);

            bool result = CommentManager.HasComments(userId, companyId, roleId, featureId);
            ResponseObject = new { Success = result};
        }
    }
}
