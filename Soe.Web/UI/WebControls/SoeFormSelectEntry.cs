using System;
using System.Collections;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormSelectEntry : SoeFormEntryBase
	{
		public string DataTextField { get; set; }
		public string DataValueField { get; set; }
        public SelectEntryValidation Validation { get; set; }

		private object dataSource;

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

		protected virtual void CreateMyControlHeirarchy()
		{
			IEnumerable resolvedDataSource = GetDataSource();

			if (resolvedDataSource != null)
			{
				foreach (object dataItem in resolvedDataSource)
				{
                    //Commented because it caused values not showing up in the settings page
                    //if (DataBinder.GetPropertyValue(dataItem, DataTextField) == null)
                    //    continue;

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

					var option = new HtmlGenericControl("option");
					if (value != null)
					{
						option.Attributes["value"] = value;
						if (value.Equals(Value))
							option.Attributes["selected"] = "selected";
					}
					option.Controls.Add(new LiteralControl(text));
					
					Controls.Add(option);
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

			writer.Write("<select");
			writer.WriteAttribute("id", ID);
			writer.WriteAttribute("name", Name);
            writer.WriteAttribute("title", Label);

			RenderEntrySettings(writer);
			RenderEntryActions(writer);
			RenderCssClassAttribute(writer);
			
			writer.Write(">");

			RenderChildren(writer);

			writer.Write("</select>");

			RenderPostfix(writer);
		}
        protected override string DefaultCssClass
        {
            get
            {
                StringBuilder css = new StringBuilder("validate");
                if ((Validation & SelectEntryValidation.NotEmpty) != 0)
                    css.Append("-notempty");
                return css.ToString();
            }
        }
        public override bool Validate()
        {
            if (!base.Validate())
                return false;
            if ((Validation & SelectEntryValidation.NotEmpty) != 0 && String.IsNullOrEmpty(Value))
                return false;
            return true;
        }
	}
    [Flags]
    public enum SelectEntryValidation : byte
    {
        //Enumeration also in SoftOne.Soe.Business.Util)
        NotEmpty = 1,
    }	
}
