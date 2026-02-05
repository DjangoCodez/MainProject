using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getCommentHeaders : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? companyId = string.IsNullOrEmpty(QS["c"]) ? null : (int?)Convert.ToInt32(QS["c"]);
            int? userId = string.IsNullOrEmpty(QS["u"]) ? null : (int?)Convert.ToInt32(QS["u"]);
            int? roleId = string.IsNullOrEmpty(QS["r"]) ? null : (int?)Convert.ToInt32(QS["r"]);
            Int32.TryParse(QS["f"], out int featureId);

            if (featureId == 0)
            {
                ResponseObject = null;
            }                
            else
            {
                var headers = CommentManager.GetCommentHeaders(userId, companyId,roleId, featureId);
                var result = new Result(); 
                foreach (var header in headers)
                {
                    var commentId = header.Comment.First<Comment>().CommentId;
                    result.Add(header.CommentHeaderId, header.Title, header.Date.ToShortDateString(),commentId);              
                }
                
                ResponseObject = result;
            }
        }

        private class Result
        { 
            private readonly Queue<CommentTitle> commentHeaders;

            public Result()
            {
                commentHeaders = new Queue<CommentTitle>();
            }

            public void Add(int id, string title, string regDate,int commentId)
            {
                CommentTitle commentHeader = new CommentTitle
                {
                    HeaderId = id,
                    Title = title,
                    RegDate = regDate,
                    CommentId = commentId
                };
                commentHeaders.Enqueue(commentHeader);
            }
        }
        private struct CommentTitle
        {
            public int HeaderId{get;set;}
            public string Title { get; set; }
            public string RegDate{get;set;}
            public int CommentId { get; set; }
        }
    }
}
