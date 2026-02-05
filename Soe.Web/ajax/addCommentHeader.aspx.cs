using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class addCommentHeader : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? companyId = string.IsNullOrEmpty(QS["c"]) ? null : (int?)Convert.ToInt32(QS["c"]);
            int? userId = string.IsNullOrEmpty(QS["u"]) ? null : (int?)Convert.ToInt32(QS["u"]);
            int? roleId = string.IsNullOrEmpty(QS["r"]) ? null : (int?)Convert.ToInt32(QS["r"]);
            string title = QS["t"];
            string comment = QS["q"];
            Int32.TryParse(QS["f"], out int featureId);

            if (featureId == 0)
            {
                ResponseObject = null;
            }                
            else
            {
                var result = CommentManager.AddComment(title, comment, userId, companyId,roleId, featureId);
                ResponseObject = result.Success ? Boolean.TrueString : Boolean.FalseString;
            }
        }
    }
}
