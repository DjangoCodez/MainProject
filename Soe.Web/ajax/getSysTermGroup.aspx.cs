using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSysTermGroup : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["sysTermGroupId"], out int sysTermGroupId))
			{
                var termDict = TermCacheManager.Instance.GetTermGroupContent((TermGroup)sysTermGroupId, skipUnknown: true)?.ToDictionary(k => k.Id, v => v.Name);               
                if (termDict != null)
					ResponseObject = termDict;
			}

			if (ResponseObject == null)
			{
				ResponseObject = new Dictionary<int, string>(0);
			}
		}
	}
}