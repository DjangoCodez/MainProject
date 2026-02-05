using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormComboEntry : SoeFormEntryBase
	{
		public string DataTextField { get; set; }
		public string DataValueField { get; set; }
		
		private object dataSource;
		private string selectedValue;
		private string selectedName;

		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				// make sure we're working with a string, XmlReader, or TextReader
				if (value == null || value is IEnumerable)
					dataSource = value;
				else
					throw new ArgumentException("DataSource must be assigned an IEnumerable");
			}
		}

		protected virtual IEnumerable GetDataSource()
		{
			if (dataSource == null)
				return null;

			IEnumerable resolvedDataSource;
			resolvedDataSource = dataSource as IEnumerable;

			return resolvedDataSource;
		}

		protected override string DefaultCssClass
		{
			get
			{
				string css = base.DefaultCssClass;
				if (css != null && css.Length > 0)
					css += " ";
				css += "combo";
				return css;
			}
		}

		protected virtual void CreateMyControlHeirarchy()
		{
			IEnumerable resolvedDataSource = GetDataSource();

			if (resolvedDataSource != null)
			{
				foreach (object dataItem in resolvedDataSource)
				{
					string text = null;
					if (!String.IsNullOrEmpty(DataTextField))
						text = DataBinder.Eval(dataItem, DataTextField).ToString();
					else
						text = dataItem.ToString();

					string value = null;
					if (!String.IsNullOrEmpty(DataValueField))
						value = DataBinder.Eval(dataItem, DataValueField).ToString();
					else
						value = dataItem.ToString();

					var li = new HtmlGenericControl("li");
					var a = new HtmlAnchor();
					if (value != null)
					{
						if (value.Equals(Value)) 
						{
							selectedValue = value;
							selectedName = text;
						}
					}
					a.HRef = "#";
					a.ID = ID + "-" + value;
					a.Controls.Add(new LiteralControl(text));
					li.Controls.Add(a);
					
					Controls.Add(li);
				}				
			}
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();
			CreateMyControlHeirarchy();
			ChildControlsCreated = true;
		}

		protected override void Render(HtmlTextWriter writer)
		{
			CreateChildControls();				
			
			RenderPrefix(writer);

			writer.Write("<input");
			writer.WriteAttribute("id", ID + "-value");
			writer.WriteAttribute("name", Name + "-value");
			writer.WriteAttribute("type", "hidden");
			writer.WriteAttribute("value", selectedValue);
            writer.WriteAttribute("title", Label);
			writer.Write(">");

			writer.Write("<input");
			writer.WriteAttribute("id", ID);
			writer.WriteAttribute("name", Name);
			writer.WriteAttribute("value", selectedName);

			if (ReadOnly)
				writer.WriteAttribute("disabled", "disabled");
			if (SkipTabStop)
				writer.WriteAttribute("tabindex", "-1");
			
			RenderCssClassAttribute(writer);
			
			writer.Write("><ol");
			writer.WriteAttribute("class", "combo-options");
			writer.WriteAttribute("id", ID + "-options");
			writer.Write(">");

			RenderChildren(writer);

			writer.Write("</ol>");

			RenderPostfix(writer);
		}
	}
}
