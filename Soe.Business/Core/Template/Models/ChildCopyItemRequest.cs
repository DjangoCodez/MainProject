using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models
{
    public class ChildCopyItemRequest
    {
        public ChildCopyItemRequestType ChildCopyItemRequestType { get; set; }
        public List<int> Ids { get; set; } = new List<int>();
    }
}
