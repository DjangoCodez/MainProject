using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormText : Control 
    {
        public virtual string Label { get; set; }
        public virtual bool FitInTable { get; set; }
        public virtual bool BoldLabel { get; set; }
        public virtual string CssClass { get; set; }
        public virtual bool Highlight { get; set; }        

        protected override void Render(HtmlTextWriter writer)
        {
            if (!FitInTable)
            {
                writer.Write("<tr");
                writer.WriteAttribute("valign", "middle");
                writer.Write("><th>");
            }

            writer.Write("<label");

            string cssClass = CssClass;
            if (Highlight)
            {
                if (!String.IsNullOrEmpty(cssClass))
                    cssClass += " ";
                cssClass += "highlight";
            }
            if (!String.IsNullOrEmpty(cssClass))
                writer.WriteAttribute("class", cssClass);
            writer.Write(">");
            writer.WriteEncodedText(Label);
            writer.Write("</label>");

            if (!FitInTable)
                writer.Write("</th><td>");
        }
    }
}
