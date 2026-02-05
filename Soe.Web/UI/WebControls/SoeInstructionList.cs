using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeInstructionList : Control
    {
        public string HeaderText { get; set; }
        public List<string> Instructions { get; set; }
        public bool Numeric { get; set; }
        public bool DisableFieldset  { get; set; }

        private string defaultIdentifier;
        public string DefaultIdentifier
        {
            get
            {
                if (!string.IsNullOrEmpty((defaultIdentifier)))
                    return defaultIdentifier;
                return "-"; //default
            }
            set
            {
                defaultIdentifier = value;
            }
        }

        private HtmlGenericControl fieldset = null;
        private HtmlGenericControl legend = null;
        private HtmlTable table = null;

        protected override void Render(HtmlTextWriter writer)
        {
            if (Instructions == null || Instructions.Count == 0)
                return;

            RenderPrefix(writer);

            int counter = 1;
            foreach (string instruction in Instructions)
            {
                var row = new HtmlTableRow();
                var cell = new HtmlTableCell();
                cell.Controls.Add(new LiteralControl((Numeric ? counter.ToString() + "." : DefaultIdentifier) + " " + instruction));
                row.Controls.Add(cell);
                table.Controls.Add(row);

                counter++;
            }

            RenderPostfix(writer);
        }

        protected void RenderPrefix(HtmlTextWriter writer)
        {
            if (!DisableFieldset)
            {
                fieldset = new HtmlGenericControl("fieldset");
                legend = new HtmlGenericControl("legend");
                legend.InnerText = HeaderText;
                fieldset.Controls.Add(legend);
            }

            table = new HtmlTable();
            table.Attributes.Add("class", "instruction");
        }

        protected void RenderPostfix(HtmlTextWriter writer)
        {
            if (!DisableFieldset)
            {
                fieldset.Controls.Add(table);
                fieldset.RenderControl(writer);
            }
            else
            {
                table.RenderControl(writer);
            }
        }
    }
}