using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.Controls
{
    public class Grid : SoeGrid, IFormControl
    {
        #region Variables

        private PageBase pageBase;

        #endregion

        #region Terms

        public int TermID { get; set; }
        public string DefaultTerm { get; set; }

        public override string Title { get; set; }

        public override string ExportText
        {
            get
            {
                return this.GetText(5472, "Exportera till Excel");
            }
        }

        public override string EmptyDataText
        {
            get
            {
                string a = base.EmptyDataText;
                return this.GetText(TermID, DefaultTerm);
            }
        }

        #endregion

        #region Ctor

        public Grid()
        {
            TermID = 36;
            DefaultTerm = "Ingen information";
        }

        #endregion

        #region Events

        protected override void RenderRegTab(HtmlTextWriter writer, bool secondTab = false)
        {
            base.RenderRegTab(writer);

            if (pageBase == null)
                pageBase = ((PageBase)Page);

            #region Prefix

            writer.Write("<li");
            writer.WriteAttribute("class", "reg");
            writer.Write(">");

            #endregion

            #region Reg link

            if ((!String.IsNullOrEmpty(RegUrl) && !secondTab)
                ||
                (!String.IsNullOrEmpty(RegUrl2) && secondTab))
            {
                Link link = new Link()
                {
                    Href = secondTab ? RegUrl2 : RegUrl,
                    Alt = secondTab ? RegLabel2 : RegLabel,
                    //FontAwesomeIcon = secondTab ? RegImage2Url : "fal fa-plus",
                    ImageSrc = secondTab ? RegImage2Url : "/cssjs/merge/SoeTabView/plus-light.png",
                    Value = "+",
                    Invisible = false,
                };

                link.RenderControl(writer);
            }

            #endregion

            #region Postfix

            writer.Write("</li>");

            #endregion
        }

        #endregion

        #region Public methods

        public void AddRegLink(string label, string href, Feature feature, Permission permission, bool secondLink = false, string fontAwesomeIcon = null, bool useImageLink=false)
        {
            if (pageBase == null)
                pageBase = ((PageBase)Page);

            if (pageBase.HasRolePermission(feature, permission))
            {
                if (secondLink)
                {
                    base.RegUrl2 = href;
                    base.RegLabel2 = label;
                    if (useImageLink)
                    {
                        if (fontAwesomeIcon != null)
                            base.RegImage2Url = fontAwesomeIcon;
                        else
                            base.RegImage2Url = "/cssjs/merge/SoeTabView/plus-light.png";
                    }
                    else
                    {
                        if (fontAwesomeIcon != null)
                            base.RegImage2Url = fontAwesomeIcon;
                        else
                            base.RegImage2Url = "fal fa-plus";
                    }

                }
                else
                {   
                    base.RegUrl = href;
                    base.RegLabel = label;
                }
            }
        }

        #endregion
    }

    public class TemplateField : SoeTemplateField, IFormControl
    {
        public int TermID { get; set; }
        public string DefaultTerm { get; set; }

        public override string HeaderText
        {
            get
            {
                return this.GetText(TermID, DefaultTerm);
            }
        }
    }

    public class BoundField : SoeBoundField, IFormControl
    {
        public int TermID { get; set; }
        public string DefaultTerm { get; set; }

        public override string HeaderText
        {
            get
            {
                if (DefaultTerm == null)
                    return String.Empty;
                return this.GetText(TermID, DefaultTerm);
            }
        }
    }
}
