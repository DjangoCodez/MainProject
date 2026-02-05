using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getReports : JsonBase
	{
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["company"], out int actorCompanyId) && Int32.TryParse(QS["templatetype"], out int sysReportTemplateId))
            {
                ReportManager rm = new ReportManager(ParameterObject);
                Dictionary<int, string> dict = rm.GetReportsByTemplateTypeDict(actorCompanyId, (SoeReportTemplateType)sysReportTemplateId, onlyOriginal: true, addEmptyRow: true);
                Queue q = new Queue();
                int i = 0;
                foreach (KeyValuePair<int, string> kvp in dict)
                {
                    q.Enqueue(new
                    {
                        Found = true,
                        Position = i,
                        ReportId = kvp.Key,
                        Name = kvp.Value,
                    });

                    i++;
                }
                ResponseObject = q;
            }
            if (ResponseObject == null)
			{
				ResponseObject = new
				{
					Found = false
				};
			}
		}
	}
}
