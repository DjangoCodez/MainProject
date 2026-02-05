using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeButton : Control
    {
        public string Text { get; set; } = "OK";
        public SoeButtonType Type { get; set; } = SoeButtonType.Submit;
        public bool SecondarySubmit { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<button");
            writer.WriteAttribute("type", "submit"); //NI 20190704: Set all buttons as submit as Edge otherwise post buttons each postback causing unexpected behavior
            writer.WriteAttribute("class", "btn btn-default" + (this.SecondarySubmit ? " secondary" : "") + (this.Type == SoeButtonType.Submit ? " submit" : ""));
            writer.WriteAttribute("title", Text);
            if (!String.IsNullOrEmpty(ID))
            {
                writer.WriteAttribute("id", ID.ToLower());
                writer.WriteAttribute("name", ID.ToLower());
            }
            writer.Write(">");
            writer.WriteEncodedText(Text);
            writer.Write("</button>");
        }
    }

    public enum SoeButtonType
    {
        Submit = 1,
        Delete = 2,
    }
}
