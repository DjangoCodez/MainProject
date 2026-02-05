using System.Web.UI.Adapters;
using System.Web.UI;
using System;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.HtmlControlAdapters
{
	public class HtmlControlAdaperBase : ControlAdapter
	{
		protected void WriteAttribute(HtmlTextWriter writer, HtmlControl control, string attributeName)
		{
			string val = control.Attributes[attributeName];
			if (val != null)
				writer.WriteAttribute(attributeName, val);
		}

		protected void WriteStandardAttributes(HtmlTextWriter writer, HtmlControl control)
		{
			if (!String.IsNullOrEmpty(control.ID))
				writer.WriteAttribute("id", control.ID);			
			WriteAttribute(writer, control, "title");
			WriteAttribute(writer, control, "class");
		}
	}
}
