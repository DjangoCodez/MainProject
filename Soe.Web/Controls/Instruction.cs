using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Web;

namespace SoftOne.Soe.Web.Controls
{
    public class Instruction : SoeFormText
    {
        public int TermID { get; set; }
        public string DefaultTerm { get; set; }
        public string LabelSetting { get; set; }
        public override bool FitInTable { get; set; }
        public override string CssClass
        {
            get
            {
                return "instruction";
            }
        }

        public override string Label
        {
            get
            {
                string label = "";
                if (!String.IsNullOrEmpty(LabelSetting))
                {
                    label = LabelSetting;
                }
                else
                {
                    if (TermID > 0)
                        label = ((PageBase)HttpContext.Current.Handler).GetText(TermID, DefaultTerm);
                }
                return label;
            }
        }
    }
}
