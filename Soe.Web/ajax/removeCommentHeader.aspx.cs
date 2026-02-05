using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class removeCommentHeader : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["id"], out int commentHeaderId))
            {
                ActionResult result = CommentManager.DeleteCommentHeader(commentHeaderId);
                ResponseObject = new { Success = result.Success };
            }
            else
                ResponseObject = new { Success = false };
        }
    }
}
