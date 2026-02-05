using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormBooleanEntry : SoeFormEntryBase
    {
        public virtual string TrueLabel { get; set; }
        public virtual string FalseLabel { get; set; }
        public virtual bool SeparateRows { get; set; }

        public override string Value
        {
            set
            {
                if (value == Boolean.TrueString || value == Boolean.FalseString)
                    base.Value = value;
                else
                    base.Value = Boolean.FalseString;//throw new SoeGeneralException("Illegal value. Only True and False allowed.", this.ToString());
            }
        }

        protected override string DefaultCssClass
        {
            get
            {
                string css = base.DefaultCssClass;
                return css;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            RenderPrefix(writer);

            RenderOne(writer, Boolean.TrueString, TrueLabel);
            if (SeparateRows)
                writer.Write("<br/>");
            RenderOne(writer, Boolean.FalseString, FalseLabel);

            RenderPostfix(writer);
        }

        private void RenderOne(HtmlTextWriter writer, string value, string label)
        {
            writer.Write("<label");
            writer.WriteAttribute("class", "discreet");
            writer.Write(">");
            writer.Write("<input");
            writer.WriteAttribute("id", ID);
            writer.WriteAttribute("type", "radio");
            writer.WriteAttribute("name", Name);
            writer.WriteAttribute("title", Label);

            RenderEntrySettings(writer);
            RenderEntryActions(writer);
            RenderCssClassAttribute(writer);

            writer.WriteAttribute("value", value);
            if (base.Value == value)
                writer.WriteAttribute("checked", "checked");

            writer.Write(">");
            writer.WriteEncodedText(label);
            writer.Write("</label>");
        }
    }
}
