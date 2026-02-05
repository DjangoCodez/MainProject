using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.HtmlControlAdapters
{
	public class HtmlAnchorAdapter : HtmlControlAdaperBase
	{
		public virtual string QueryStringAppendix { get; set ; }
		
		protected override void Render(HtmlTextWriter writer)
		{
			HtmlAnchor a = (HtmlAnchor)Control;

			string href;
			if (String.IsNullOrEmpty(QueryStringAppendix) || a.HRef == "#")
			{
				href = a.HRef;
			}
			else
			{
				string qsapp = "?" + QueryStringAppendix;
				
				string[] p = a.HRef.Split('#');
				string[] pp = p[0].Split('?');

				href = pp[0] + qsapp;
				
				if (pp.Length > 1)
					href += "&amp;" + String.Join(String.Empty, pp, 1, pp.Length - 1);
				if (p.Length > 1)
					href += "#" + String.Join(String.Empty, p, 1, p.Length - 1);	
			}		
			
			writer.Write("<a");
			writer.WriteAttribute("href", href);
			WriteStandardAttributes(writer, a);			
			writer.Write(">");
			RenderChildren(writer);
			writer.Write("</a>");
		}
	}
}
