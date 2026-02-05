using System;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Web.HtmlControlAdapters
{
	public class HtmlAnchorAdapter : SoftOne.Soe.Web.UI.HtmlControlAdapters.HtmlAnchorAdapter
	{
		public override string QueryStringAppendix
		{
			get
			{
				PageBase pb = Page as PageBase;
				if (pb != null)
				{
					CompanyDTO c = pb.SoeCompany;
					if (c != null)
						return "c=" + c.ActorCompanyId;
				}

				return String.Empty;
			}
		}
	}
}