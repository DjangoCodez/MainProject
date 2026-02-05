using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class saveNote : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? companyId;
            int? userId;
            int? roleId;
            int featureId;
            string title;
            string comment;
            bool personal;
            bool rolebased;

            companyId = string.IsNullOrEmpty(QS["c"]) ? null : (int?)Convert.ToInt32(QS["c"]);
            userId = string.IsNullOrEmpty(QS["u"]) ? null : (int?)Convert.ToInt32(QS["u"]);
            roleId = string.IsNullOrEmpty(QS["r"]) ? null : (int?)Convert.ToInt32(QS["r"]);

            personal = Convert.ToBoolean(QS["p"]);
            rolebased = Convert.ToBoolean(QS["rb"]);

            Int32.TryParse(QS["f"], out featureId);
            comment = QS["com"];
            title = QS["t"];

            if (rolebased)
                companyId = null;

            if (personal)
            {
                companyId = null;
                roleId = null;
            }

            ActionResult  result = CommentManager.AddComment(title, comment, userId, companyId,roleId, featureId);
            ResponseObject = new { Success = result.Success};
        }
    }
}
