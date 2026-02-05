using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls.Layout
{
    public class TopMenuSelectorItem
    {
        #region Enums

        #endregion

        #region Variables

        public string ID { get; set; }
        public string Href { get; set; }
        public string Label { get; set; }
        public string ToolTip { get; set; }
        public bool ShowDelete { get; set; }
        public string DeleteToolTip { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsHeader { get; set; }
        public bool IsModal { get; set; }
        public bool NewWindow { get; set; }
        public List<TopMenuSelectorItem> SubItems { get; set; }

        #endregion

        #region Ctor

        public TopMenuSelectorItem()
        {

        }

        #endregion

        #region Public methods

        public HtmlGenericControl GetTopMenuSelectorItemControl(bool selected, bool showCaret)
        {
            HtmlGenericControl ctrl;

            if (selected)
            {
                ctrl = GetDropdownControl(true);

                if (showCaret)
                {
                    var span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "caret");
                    ctrl.Controls.Add(span);
                }
            }
            else
            {
                ctrl = GetListItemControl();

                if (this.SubItems != null && this.SubItems.Count > 0)
                {
                    ctrl.Attributes.Add("class", "dropdown-submenu");

                    HtmlGenericControl link = GetDropdownControl(false);
                    link.Attributes.Add("onclick", "event.stopPropagation();");
                    ctrl.Controls.Add(link);

                    HtmlGenericControl ul = GetListControl();
                    foreach (TopMenuSelectorItem item in this.SubItems)
                    {
                        ul.Controls.Add(item.GetTopMenuSelectorItemControl(false, false));
                    }
                    ctrl.Controls.Add(ul);
                }
                else
                {
                    HtmlGenericControl link = GetLinkControl();

                    if (this.ShowDelete)
                    {
                        HtmlGenericControl span = new HtmlGenericControl("span");
                        span.Attributes.Add("class", "fa fa-times pull-right remove-favorite-item");
                        if (!String.IsNullOrEmpty(this.DeleteToolTip))
                            span.Attributes.Add("title", this.DeleteToolTip);
                        span.Attributes.Add("onclick", "RemoveFavorite(" + this.ID + "); return false;");

                        link.Controls.Add(span);
                    }

                    ctrl.Controls.Add(link);
                }
            }

            return ctrl;
        }

        #endregion

        #region Help-methods

        private HtmlGenericControl GetDropdownControl(bool isToggle)
        {
            HtmlGenericControl anchor = new HtmlGenericControl("a");
            if (isToggle)
                anchor.Attributes.Add("data-toggle", "dropdown");
            anchor.InnerText = this.Label;

            return anchor;
        }

        private HtmlGenericControl GetListControl()
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "dropdown-menu");

            return ul;
        }

        private HtmlGenericControl GetListItemControl()
        {
            HtmlGenericControl li = new HtmlGenericControl("li");

            return li;
        }

        public HtmlGenericControl GetLinkControl()
        {
            HtmlGenericControl anchor = new HtmlGenericControl("a");

            if (!String.IsNullOrEmpty(this.Href))
                anchor.Attributes.Add("href", this.Href);

            if (!String.IsNullOrEmpty(this.ToolTip))
                anchor.Attributes.Add("Alt", this.ToolTip);
            else if (!String.IsNullOrEmpty(this.Label))
                anchor.Attributes.Add("Alt", this.Label);

            if (this.NewWindow)
                anchor.Attributes.Add("target", "_blank");

            if (!String.IsNullOrEmpty(this.Label))
                anchor.InnerHtml = StringUtility.XmlEncode(this.Label);

            if (this.IsModal)
                anchor.Attributes.Add("class", "PopLink");

            return anchor;
        }

        #endregion
    }
}