using SoftOne.Soe.Common.Util;
using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    #region Enums

    public enum ColumnFilterModes
    {
        None,
        Contains,
        Match,
        Numeric,
        StartsWith
    }

    public enum ColumnSortModes
    {
        None,
        Date,
        Numeric,
        Text
    }

    #endregion

    public class SoeGrid : GridView
    {
        #region Grid properties

        private int[] pageLengths = new int[] { 20, 50, 100 };
        private bool isEmpty = false;

        public override string EmptyDataText
        {
            get
            {
                isEmpty = true;
                return base.EmptyDataText;
            }
        }

        /// <summary>
        /// The content of the title element that is rendered and associated with the grid element.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The content of the export element that is rendered and associated with the grid element.
        /// </summary>
        public virtual string ExportText { get; set; }

        /// <summary>
        /// Url for register new post
        /// </summary>
        protected string RegUrl { get; set; }

        /// <summary>
        /// Text for register new post
        /// </summary>
        protected string RegLabel { get; set; }

        public string RegUrl2 { get; set; }

        public string RegLabel2 { get; set; }

        public string RegImage2Url { get; set; }

        public Table DataTable
        {
            get
            {
                return this.Controls.Count > 0 ? (Table)this.Controls[0] : null;
            }
        }

        public int NoOfColumns
        {
            get
            {
                return this.Columns.Count;
            }
        }

        public int NoOfRows
        {
            get
            {
                Table table = DataTable;
                return table != null ? table.Rows.Count : 0;
            }
        }

        #endregion

        #region Events

        protected override void Render(HtmlTextWriter writer)
        {
            #region Init

            this.RowStyle.CssClass = "odd";
            this.AlternatingRowStyle.CssClass = "even";

            PrepareControlHierarchy();

            if (!String.IsNullOrEmpty(CssClass))
                CssClass += " ";
            CssClass += "pageable no-arrow normal-text";

            #endregion

            //SoeGrid
            writer.Write("<div");
            writer.WriteAttribute("class", "SoeGrid");
            writer.Write(">");

            #region TabList

            //SoeTabView
            writer.Write("<div");
            writer.WriteAttribute("class", "SoeTabView");
            writer.Write(">");

            //TabList
            writer.Write("<div");
            writer.WriteAttribute("class", "tabList");
            writer.Write(">");

            #region Tabs

            writer.Write("<ul>");

            //Title
            writer.Write("<li>");
            writer.Write("<a");
            writer.WriteAttribute("href", "#" + ID + "_" + "1");
            writer.WriteAttribute("class", "active");
            writer.Write(">");
            if (!String.IsNullOrEmpty(Title))
                writer.WriteEncodedText(Title);
            writer.Write("</a>");
            writer.Write("</li>");

            //Reg link
            RenderRegTab(writer);

            //Reg link2
            if (RegUrl2 != null && RegLabel2 != null)
                RenderRegTab(writer, true);

            writer.Write("</ul>");

            #endregion

            writer.Write("</div>");
            writer.Write("</div>");

            #endregion

            //GridContent
            writer.Write("<div");
            writer.WriteAttribute("class", "gridContent");
            writer.Write(">");

            //GridHeader
            writer.Write("<div");
            writer.WriteAttribute("id", "GridHeader");
            writer.Write(">");
            writer.Write("<form");
            writer.WriteAttribute("action", "#");
            writer.Write(">");
            writer.Write("<a");
            writer.WriteAttribute("class", "exportLink");
            writer.WriteAttribute("href", "#");
            writer.WriteAttribute("style", "display:none");
            writer.Write(">");
            writer.Write("<img");
            writer.WriteAttribute("src", "/img/export.png");
            writer.Write(">");
            writer.Write(ExportText);
            writer.Write("</a>");
            writer.Write("</form>");
            writer.Write("</div>");

            #region Grid table

            writer.Write("<form");
            writer.WriteAttribute("action", "#");
            writer.Write(">");

            writer.Write("<table");
            writer.WriteAttribute("class", CssClass);
            writer.WriteAttribute("id", ID);
            writer.Write(">");

            Table table = DataTable;
            if (table != null)
            {
                if (isEmpty)
                {
                    #region Empty

                    writer.Write("<tbody>");
                    table.Controls[0].RenderControl(writer);
                    writer.Write("</tbody>");

                    #endregion
                }
                else
                {
                    #region Header

                    byte columnCounter = 0;
                    bool focusFirst = true;

                    #region Thead

                    writer.Write("<thead>");

                    #region Filter

                    //SoeGridFilter
                    writer.Write("<tr");
                    writer.WriteAttribute("class", "SoeGridFilter");
                    writer.Write(">");

                    for (int i = 0; i < table.Controls[0].Controls.Count; i++)
                    {
                        #region Filter control

                        if (!this.Columns[i].Visible)
                            continue;

                        ColumnFilterModes filterable = ColumnFilterModes.None;
                        string text = String.Empty;

                        if (this.Columns[i] is SoeTemplateField)
                        {
                            var field = (SoeTemplateField)this.Columns[i];
                            filterable = field.Filterable;

                            // Väldigt ful lösning
                            StringBuilder sb = new StringBuilder();
                            HtmlTextWriter htw = new HtmlTextWriter(new StringWriter(sb));
                            table.Controls[0].Controls[i].RenderControl(htw);
                            text = sb.ToString().RemoveTags();
                        }
                        else if (this.Columns[i] is SoeBoundField)
                        {
                            var field = (SoeBoundField)this.Columns[i];
                            text = field.HeaderText;
                            filterable = field.Filterable;
                        }

                        #region Filter input

                        writer.Write("<th>");

                        if (filterable != ColumnFilterModes.None && !isEmpty)
                        {
                            string columnId = ID + "-" + (columnCounter++).ToString();

                            writer.Write("<input");
                            writer.WriteAttribute("id", columnId);
                            writer.WriteAttribute("class", filterable.ToString().ToLower());
                            writer.Write(">");
                            if (focusFirst)
                            {
                                writer.Write("<script type='text/javascript'>" + "document.getElementById('" + columnId + "').focus();" + "</script>");
                                focusFirst = false;
                            }
                        }

                        writer.Write("</th>");

                        #endregion

                        #endregion
                    }

                    writer.Write("</tr>");

                    #endregion

                    table.Controls[0].RenderControl(writer);

                    writer.Write("</thead>");

                    #endregion

                    #endregion

                    #region Body

                    writer.Write("<tbody");
                    writer.WriteAttribute("class", "content");
                    writer.Write(">");

                    for (int i = 1; i < table.Controls.Count; i++)
                    {
                        table.Controls[i].RenderControl(writer);
                    }

                    writer.Write("</tbody>");

                    #endregion

                    #region Footer

                    writer.Write("<tfoot>");

                    writer.Write("<tr>");
                    writer.Write("<td");
                    writer.WriteAttribute("colspan", this.NoOfColumns.ToString());
                    writer.Write(">");
                    writer.Write("</td>");
                    writer.Write("</tr>");

                    writer.Write("</tfoot>");

                    #endregion
                }
            }
            else
            {
                writer.Write("<tbody />");
            }

            writer.Write("</table>");

            writer.Write("</form>");

            #endregion

            writer.Write("</div>");
            writer.Write("</div>");
        }

        #endregion

        #region Virtual (overrided by descendants)

        protected virtual void RenderRegTab(HtmlTextWriter writer, bool secondTab = false)
        {
            //Overrided by SoeForm
        }

        #endregion
    }

    public class SoeTemplateField : TemplateField
    {
        private ColumnFilterModes filterable = ColumnFilterModes.None;
        public ColumnFilterModes Filterable
        {
            get { return filterable; }
            set
            {
                filterable = value;
                if (!String.IsNullOrEmpty(this.HeaderStyle.CssClass))
                {
                    this.HeaderStyle.CssClass += " ";
                }
                this.HeaderStyle.CssClass += "filterable-";
                this.HeaderStyle.CssClass += filterable.ToString().ToLower();
            }
        }

        private ColumnSortModes sortable = ColumnSortModes.None;
        public ColumnSortModes Sortable
        {
            get { return sortable; }
            set
            {
                sortable = value;
                if (!String.IsNullOrEmpty(this.HeaderStyle.CssClass))
                {
                    this.HeaderStyle.CssClass += " ";
                }
                this.HeaderStyle.CssClass += "sortable-";
                this.HeaderStyle.CssClass += sortable.ToString().ToLower();
            }
        }
    }

    public class SoeBoundField : BoundField
    {
        private ColumnFilterModes filterable = ColumnFilterModes.None;
        public ColumnFilterModes Filterable
        {
            get { return filterable; }
            set
            {
                filterable = value;
                if (!String.IsNullOrEmpty(this.HeaderStyle.CssClass))
                {
                    this.HeaderStyle.CssClass += " ";
                }
                this.HeaderStyle.CssClass += "filterable-";
                this.HeaderStyle.CssClass += filterable.ToString().ToLower();
            }
        }

        private ColumnSortModes sortable = ColumnSortModes.None;
        public ColumnSortModes Sortable
        {
            get { return sortable; }
            set
            {
                sortable = value;
                if (!String.IsNullOrEmpty(this.HeaderStyle.CssClass))
                {
                    this.HeaderStyle.CssClass += " ";
                }
                this.HeaderStyle.CssClass += "sortable-";
                this.HeaderStyle.CssClass += sortable.ToString().ToLower();
            }
        }
    }
}
