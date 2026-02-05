using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormCheckBoxEntry : SoeFormEntryBase
    {
        protected override void Render(HtmlTextWriter writer)
        {
            RenderPrefix(writer);

            writer.Write("<input");
            writer.WriteAttribute("id", ID);
            writer.WriteAttribute("type", "checkbox");
            writer.WriteAttribute("name", Name);
            writer.WriteAttribute("title", Label);

            RenderEntrySettings(writer);
            RenderEntryActions(writer);

            string css = "checkbox";
            if (!String.IsNullOrEmpty(CssClass))
                css += " " + CssClass;
            writer.WriteAttribute("class", css);
            writer.WriteAttribute("value", Boolean.TrueString);

            if (Value == Boolean.TrueString)
                writer.WriteAttribute("checked", null);

            writer.Write(">");

            RenderPostfix(writer);
        }
    }
}