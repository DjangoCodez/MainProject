using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getComment : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? companyId = string.IsNullOrEmpty(QS["c"]) ? null : (int?)Convert.ToInt32(QS["c"]);
            int? userId = string.IsNullOrEmpty(QS["u"]) ? null : (int?)Convert.ToInt32(QS["u"]);

            if (Int32.TryParse(QS["id"], out int commentId))
            {
                Comment comment = CommentManager.GetComment(userId, companyId, commentId);
                ResponseObject = new Result()
                {
                    Comment = comment.Comment1,
                    CommentId = comment.CommentId
                };
            }
            else
                ResponseObject = null;
        }

        private class Result
        {
            public string Comment { get; set; }
            public int CommentId { get; set; }
        }
    }
}
