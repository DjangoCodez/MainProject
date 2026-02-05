using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.Controls
{
    public class Link : ControlBase, IFormControl
    {
        public string Href { get; set; }
        public string Value { get; set; }
        public string Alt { get; set; }
        public string CssClass { get; set; }
        public string ImageSrc { get; set; }
        public string ImageText { get; set; }
        public string FontAwesomeIcon { get; set; }
        public string OnClick { get; set; }
        public bool Invisible { get; set; }
        public bool SkipTabstop { get; set; }
        public bool NewWindow { get; set; }
        public Feature Feature { get; set; }
        public Permission Permission { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Feature != Feature.None && !PageBase.HasRolePermission(Feature, Permission))
                return;

            #region Anchor

            writer.Write("<a");

            //ID
            if (!String.IsNullOrEmpty(ID))
                writer.WriteAttribute("id", ID);

            //Alt
            writer.WriteAttribute("alt", Alt);

            //Href
            string href = "";
            if (String.IsNullOrEmpty(OnClick) && Href != null)
            {
                if (PageBase != null)
                {
                    href = UrlUtil.AddQueryStringParameter(Href, "r", PageBase.RoleId.ToString());
                    href = UrlUtil.AddQueryStringParameter(href, "c", PageBase.SoeCompany.ActorCompanyId.ToString());
                }
                else
                {
                    href = UrlUtil.AddQueryStringParameter(Href, "r", ((PageBase)System.Web.HttpContext.Current.Handler).RoleId.ToString());
                    href = UrlUtil.AddQueryStringParameter(href, "c", ((PageBase)System.Web.HttpContext.Current.Handler).SoeCompany.ActorCompanyId.ToString());
                }
            }

            //OnClick
            if (!String.IsNullOrEmpty(OnClick))
            {
                writer.WriteAttribute("OnClick", OnClick);
                writer.WriteAttribute("href", "javascript:;");
            }
            else
            {
                if (!String.IsNullOrEmpty(href))
                    writer.WriteAttribute("href", href);
            }

            //Css
            if (!String.IsNullOrEmpty(CssClass))
                writer.WriteAttribute("class", CssClass);

            //Attributes
            if (Invisible)
                writer.WriteAttribute("style", "display:none");
            if (SkipTabstop)
                writer.WriteAttribute("tabindex", "-1");
            if (NewWindow)
                writer.WriteAttribute("target", "_blank");

            writer.Write(">");

            #endregion

            #region Image

            // FontAwesome icon
            if (!String.IsNullOrEmpty(FontAwesomeIcon))
            {
                writer.Write("<i");
                writer.WriteAttribute("class", FontAwesomeIcon);
                writer.WriteAttribute("title", Alt);
                writer.Write("></i>");
            }

            //ImageSrc
            if (!String.IsNullOrEmpty(ImageSrc))
            {
                writer.Write("<img");
                writer.WriteAttribute("src", ImageSrc);
                writer.WriteAttribute("width", "16");
                writer.WriteAttribute("height", "16");
                writer.WriteAttribute("align", "middle");
                writer.WriteAttribute("style", "margin-bottom:2px");
                writer.WriteAttribute("alt", Alt);
                writer.WriteAttribute("title", Alt);
                writer.Write(">");

                //ImageText
                if (!String.IsNullOrEmpty(ImageText))
                {
                    writer.Write("<span>");
                    writer.Write(StringUtility.XmlEncode(ImageText.Trim()));
                    writer.Write("</span>");
                }
            }
            else
            {
                writer.Write(StringUtility.XmlEncode(Value));
            }

            writer.Write("</a>");

            #endregion
        }
    }
}