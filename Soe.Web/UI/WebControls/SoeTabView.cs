using SoftOne.Soe.Util.Exceptions;
using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class SoeTabView : Control, INamingContainer
    {
        #region Enums

        public enum SoeMessageType
        {
            Unknown = 0,
            Information = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
        }

        #endregion

        #region Properties

        private TabsContainer tabs;

        [TemplateContainer(typeof(TabsContainer))]
        public TabsContainer Tabs
        {
            get
            {
                return tabs;
            }
            set
            {
                tabs = value;
            }
        }

        private int activeTab = 1;
        public int ActiveTab
        {
            get { return activeTab; }
            set { activeTab = value; }
        }

        /// <summary>
        /// The content of the title element that is rendered and associated with the form element.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// A MessageType set
        /// </summary>
        protected SoeMessageType MessageType { get; set; }

        /// <summary>
        /// Url for register new post
        /// </summary>
        protected string RegUrl { get; set; }

        /// <summary>
        /// Text for register new post
        /// </summary>
        protected string RegLabel { get; set; }

        #endregion

        #region Events

        protected override void Render(HtmlTextWriter writer)
        {
            #region Init

            if (string.IsNullOrEmpty(ID))
                throw new SoeGeneralException("ID of SoeTabView not defined", this.ToString());

            #endregion

            #region Prefix

            writer.Write("<div");
            writer.WriteAttribute("id", ID);
            writer.WriteAttribute("class", "row SoeTabView");
            writer.Write(">");

            #endregion

            #region TabList

            writer.Write("<div");
            writer.WriteAttribute("class", "tabList");
            writer.Write(">");
            writer.Write("<ul>");

            int i = 0;
            foreach (Control control in Tabs.Controls)
            {
                #region SoeTab

                SoeTab tab = control as SoeTab;
                if (tab != null && tab.Visible)
                {
                    writer.Write("<li>");
                    writer.Write("<a");
                    writer.WriteAttribute("href", "#" + ID + "_" + (++i).ToString());
                    if (i == ActiveTab)
                        writer.WriteAttribute("class", "active");
                    writer.Write(">");

                    tab.TabNo = i;
                    if (!String.IsNullOrEmpty(tab.HeaderText))
                        writer.WriteEncodedText(tab.HeaderText);

                    writer.Write("</a>");
                    writer.Write("</li>");
                }

                #endregion
            }

            RenderRegTab(writer);

            writer.Write("</ul>");
            writer.Write("</div>");

            #endregion

            #region TabContent

            i = 0;
            foreach (Control control in Tabs.Controls)
            {
                #region SoeTab

                SoeTab tab = control as SoeTab;
                if (tab != null && tab.Visible)
                {
                    string className = "tabContent";
                    if (++i == ActiveTab)
                        className += " active";

                    writer.Write("<div");
                    writer.WriteAttribute("id", ID + "_" + i.ToString());
                    writer.WriteAttribute("class", className);
                    writer.Write(">");

                    #region FormHeader

                    #region Prefix

                    writer.Write("<div");
                    writer.WriteAttribute("class", "formHeader");
                    writer.Write(">");

                    #endregion

                    RenderFormHeader(writer);

                    #region Postfix

                    writer.Write("\n</div>");

                    #endregion

                    #endregion

                    tab.RenderControl(writer);

                    writer.Write("</div>");
                }

                #endregion
            }

            #endregion

            #region Postfix

            writer.Write("</div>");

            #endregion
        }

        #region Virtual (overrided by descendants)

        protected virtual void RenderRegTab(HtmlTextWriter writer)
        {
            //Overrided by SoeForm
        }

        protected virtual void RenderFormHeader(HtmlTextWriter writer)
        {
            //Overrided by SoeForm
        }

        #endregion

        #endregion
    }

    public class TabsContainer : Control, INamingContainer
    {
    }

    public class SoeTab : Control
    {
        public virtual string HeaderText { get; set; }
        public virtual string ImgSrc { get; set; }
        public virtual string ImgAlt { get; set; }
        public int TabNo { get; set; }
    }
}
