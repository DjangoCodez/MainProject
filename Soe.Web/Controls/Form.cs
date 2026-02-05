using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Web.UI;

namespace SoftOne.Soe.Web.Controls
{
    [ParseChildren(true)]
    public class Form : SoeForm, IFormControl
    {
        #region Variabels

        public string Name { get; set; }
        public FieldSettingManager fsm { get; set; }

        private PageBase pageBase;
        private readonly SoeFieldSettingType type = SoeFieldSettingType.Web;

        #endregion

        #region Terms

        public int TermID { get; set; }
        public string DefaultTerm { get; set; }

        #endregion

        #region Settings

        public List<SettingObject> settings;

        /// <summary>
        /// Disable settings. No settings are applied, and no setting icons are visible.
        /// </summary>
        private bool disableSettings;
        public bool DisableSettings
        {
            get
            {
                return disableSettings;
            }
            set
            {
                disableSettings = value;
                DisableSwitchLabelText = disableSettings;
            }
        }
        /// <summary>
        /// Skip to apply settings. But icons are still enable. If DisableSettings are true, this property are discouraged.
        /// Default: Apply settings
        /// </summary>
        public bool StopSettings { get; set; }

        #endregion

        #region Form / Mode

        public SoftOne.Soe.Data.Form form;
        public NameValueCollection PreviousForm { get; set; }
        public SoeFormMode Mode { get; set; }

        #endregion

        #region Entity status

        private string entityCreated;
        private string entityCreatedBy;
        private string entityModified;
        private string entityModifiedBy;
        private EntityObject entity;
        public EntityObject Entity
        {
            get
            {
                return entity;
            }
            set
            {
                entity = value;
                SetEntityStatus();
            }
        }

        private void SetEntityStatus()
        {
            if (Entity != null)
            {
                //Created
                object created = Entity.GetEntityProperty("Created");
                if (created != null)
                    entityCreated = (created is DateTime ? Convert.ToDateTime(created).ToShortDateLongTimeString() : String.Empty);

                //CreatedBy
                object createdBy = Entity.GetEntityProperty("CreatedBy");
                if (createdBy != null)
                    entityCreatedBy = createdBy.ToString();

                //Modified
                object modified = Entity.GetEntityProperty("Modified");
                if (modified != null)
                    entityModified = (modified is DateTime ? Convert.ToDateTime(modified).ToShortDateLongTimeString() : String.Empty);

                //ModifiedBy
                object modifiedBy = Entity.GetEntityProperty("ModifiedBy");
                if (modifiedBy != null)
                    entityModifiedBy = modifiedBy.ToString();
            }
        }

        #endregion

        #region Buttons / Links

        /// <summary>Cache the previous ButtonText. Used to repopulate</summary>
        private string previousButtonText;
        protected string PreviousButtonText
        {
            get
            {
                try
                {
                    if (String.IsNullOrEmpty(previousButtonText))
                        previousButtonText = ((Page)System.Web.HttpContext.Current.Handler).Session[Constants.SESSION_SOEFORM_PREVIOUS_BUTTONTEXT] as string;
                    if (!String.IsNullOrEmpty(previousButtonText))
                        return previousButtonText;
                    return this.GetText(30, "Spara");
                }
                catch (Exception ex)
                {
                    ex.ToString(); // Prevent compiler warning
                    return this.GetText(30, "Spara");
                }
            }
            set
            {
                try
                {
                    ((Page)System.Web.HttpContext.Current.Handler).Session[Constants.SESSION_SOEFORM_PREVIOUS_BUTTONTEXT] = value;
                }
                catch (Exception ex)
                {
                    ex.ToString(); //Prevent compiler warning
                    ((Page)System.Web.HttpContext.Current.Handler).Session[Constants.SESSION_SOEFORM_PREVIOUS_BUTTONTEXT] = this.GetText(30, "Spara");
                }
            }
        }

        public override string ButtonText
        {
            get
            {
                string buttonText = "";

                if (TermID > 0 && !String.IsNullOrEmpty(DefaultTerm))
                {
                    buttonText = this.GetText(TermID, DefaultTerm);
                }
                else
                {
                    switch (Mode)
                    {
                        case SoeFormMode.Update:
                            buttonText = this.GetText(1231, "Uppdatera");
                            break;
                        case SoeFormMode.Register:
                            buttonText = this.GetText(30, "Spara");
                            break;
                        case SoeFormMode.RegisterFromCopy:
                            buttonText = this.GetText(1233, "Spara från kopia");
                            break;
                        case SoeFormMode.Save:
                            buttonText = this.GetText(30, "Spara");
                            break;
                        case SoeFormMode.Back:
                            buttonText = this.GetText(84, "Tillbaka"); 
                            break;
                        case SoeFormMode.ExecuteAdminTask:
                            buttonText = this.GetText(5126, "Utför");
                            break;
                        case SoeFormMode.Repopulate:
                            buttonText = PreviousButtonText;
                            break;
                        default:
                            buttonText = this.GetText(TermID, DefaultTerm);
                            break;
                    }
                }

                PreviousButtonText = buttonText;
                return buttonText;
            }
        }

        private List<Link> links;
        public List<Link> Links
        {
            get
            {
                return links;
            }
        }

        #endregion

        #region Ctor

        public Form()
        {
            Mode = SoeFormMode.Save;
        }

        #endregion

        #region Events

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (pageBase == null)
                pageBase = ((PageBase)Page);

            fsm = new FieldSettingManager(pageBase.ParameterObject);

            #region Mode

            //Prevent Copy, Delete and PrevNext in Registration mode, and recursive copying
            if ((Mode == SoeFormMode.Register) || (Mode == SoeFormMode.RegisterFromCopy))
            {
                EnableCopy = false;
                EnableDelete = false;
                EnablePrevNext = false;
            }

            //Prevent delete when repopulated
            if (Mode == SoeFormMode.Repopulate)
            {
                EnableDelete = false;
            }

            #endregion

            #region Permissions

            //Check permissions
            if (!pageBase.HasRolePermission(pageBase.Feature, Permission.Modify))
            {
                EnableDelete = false;
                EnableCopy = false;
            }

            #endregion

            #region Settings

            //Settings for Form
            if (!DisableSettings)
            {
                //Only check modify permission if DisableSettings not been manually set to true in SOE:Form
                if (!pageBase.HasRolePermission(pageBase.Feature, Permission.Modify))
                    DisableSave = true;

                //Get current url without querystrings and FormId as unique Form name
                Name = Page.Request.Url.AbsolutePath + "#" + this.ID;

                form = fsm.GetForm(this.Name, this.type);
                if (form != null)
                    settings = fsm.GetSettingsForForm(form.FormId, pageBase.RoleId, pageBase.SoeCompany.ActorCompanyId);
                else
                    form = fsm.AddForm(this.Name, this.type);
            }

            #endregion

            foreach (Control control in Tabs.Controls)
            {
                FindControls(control);
            }
        }

        protected override void RenderRegTab(HtmlTextWriter writer)
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

            if (!String.IsNullOrEmpty(RegLabel))
            {
                Link link = new Link()
                {
                    Href = RegUrl,
                    Alt = RegLabel,
                    ImageSrc = "/cssjs/merge/SoeTabView/plus-light.png",
//                    FontAwesomeIcon = "fal fa-plus",
                    Invisible = false,
                };
                link.RenderControl(writer);
            }

            #endregion

            #region Postfix

            writer.Write("</li>");

            #endregion
        }

        protected override void RenderFormToolBar(HtmlTextWriter writer)
        {
            base.RenderFormToolBar(writer);

            if (pageBase == null)
                pageBase = ((PageBase)Page);

            #region Prefix

            writer.Write("<span");
            writer.WriteAttribute("class", "toolBar");
            writer.Write(">");

            #endregion

            #region Title

            if (!String.IsNullOrEmpty(Title))
            {
                writer.Write("<span");
                writer.WriteAttribute("class", "title");
                writer.Write(">");
                writer.Write(StringUtility.XmlEncode(Title));
                writer.Write("</span>");
            }

            #endregion

            #region Icons

            writer.Write("<div");
            writer.WriteAttribute("class", "actionBar");
            writer.Write(">");

            //DummyButton för Modifyrights
            if (pageBase.HasRolePermission(pageBase.Feature, Permission.Modify))
            {
                this.RenderDummyButton(writer);
            }

            //Setting icons
            if (pageBase.HasRolePermission(Feature.Common, Permission.Readonly))
            {
                if ((!DisableSettings) && (form == null))
                    form = fsm.GetForm(this.Name, this.type);

                this.RenderSettingsIcons(writer);
            }

            //Modify icons
            if (pageBase.HasRolePermission(pageBase.Feature, Permission.Modify))
            {
                this.RenderModifyIcons(writer);
            }

            //LabelText icons
            this.RenderSwitchLabelTextIcon(writer);

            //Navigation icons
            this.RenderNavigationIcons(writer);

            writer.Write("</div>");

            #endregion

            #region Postfix

            writer.Write("</span>");

            #endregion
        }

        protected override void RenderFormStatus(HtmlTextWriter writer)
        {
            if (pageBase == null)
                pageBase = ((PageBase)Page);

            bool hasStatusInfo = false;

            #region Created, CreatedBy

            if (!String.IsNullOrEmpty(entityCreated) && !String.IsNullOrEmpty(entityCreatedBy))
            {
                writer.Write("<span>");
                writer.WriteEncodedText(pageBase.GetText(1922, "Skapad") + " ");
                writer.Write("<b>");
                writer.WriteEncodedText(entityCreated);
                writer.Write("</b>");
                writer.WriteEncodedText(" " + pageBase.GetText(1923, "av") + " ");
                writer.Write("<b>");
                writer.WriteEncodedText(entityCreatedBy);
                writer.Write("</b>");
                writer.Write("</span>");

                hasStatusInfo = true;
            }

            #endregion

            #region Modified, ModifiedBy

            writer.Write("<br />");
            if (!String.IsNullOrEmpty(entityModified) && !String.IsNullOrEmpty(entityModifiedBy))
            {
                writer.Write("<span>");
                writer.WriteEncodedText(pageBase.GetText(1924, "Ändrad") + " ");
                writer.Write("<b>");
                writer.WriteEncodedText(entityModified);
                writer.Write("</b>");
                writer.WriteEncodedText(" " + pageBase.GetText(1923, "av") + " ");
                writer.Write("<b>");
                writer.WriteEncodedText(entityModifiedBy);
                writer.Write("</b>");
                writer.Write("</span>");

                hasStatusInfo = true;
            }

            #endregion

            #region No status info

            if (!hasStatusInfo)
            {
                writer.Write("<span");
                writer.WriteAttribute("class", "noinfo");
                writer.Write(">");
                writer.WriteEncodedText(pageBase.GetText(1946, "Ingen historik"));
                writer.Write("</span>");
            }

            #endregion
        }

        protected override void RenderFormLinks(HtmlTextWriter writer)
        {
            base.RenderFormLinks(writer);

            if (pageBase == null)
                pageBase = ((PageBase)Page);

            this.RenderLinks(writer);
        }

        protected override void RenderFormButtons(HtmlTextWriter writer)
        {
            base.RenderFormButtons(writer);

            if (pageBase == null)
                pageBase = ((PageBase)Page);

            this.RenderButtons(writer);
        }

        #endregion

        #region Public methods

        public void SetRegLink(string label, string href, Feature feature, Permission permission)
        {
            if (pageBase == null)
                pageBase = ((PageBase)Page);

            if (pageBase.HasRolePermission(feature, permission))
            {
                base.RegUrl = href;
                base.RegLabel = label;
            }
        }

        /// <summary>
        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        public void AddLink(string label, string href, Feature feature, Permission permission)
        {
            AddLink(label, href, feature, permission, false);
        }

        /// <summary>
        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        /// <param name="modal">True if the link should open as a modal window/param>
        public void AddLink(string label, string href, Feature feature, Permission permission, bool modal)
        {
            if (pageBase == null)
                pageBase = ((PageBase)Page);

            if (pageBase.HasRolePermission(feature, permission))
            {
                Link link = new Link()
                {
                    Href = href,
                    Value = label,
                    Alt = label,
                };

                if (modal)
                    link.CssClass = "PopLink";

                if (links == null)
                    links = new List<Link>();
                links.Add(link);
            }
        }

        #endregion

        #region Help-methods

        private void FindControls(Control obj)
        {
            //FieldSettings
            if (obj is IEntryControl)
            {
                IEntryControl control = obj as IEntryControl;
                if (control != null && !DisableSettings)
                    control.CheckFieldProperties(fsm, form, this.Name, this.StopSettings, this.settings);
            }

            //Mode
            if ((Mode == SoeFormMode.RegisterFromCopy || Mode == SoeFormMode.Repopulate) && PreviousForm != null && obj is SoeFormEntryBase)
            {
                //Use SoeFormEntryBase to get to Name and Value properties
                    bool repopulate = true;
                    if (obj is SoeFormPasswordEntry)
                    {
                        repopulate = false;
                    }

                    if (repopulate)
                    {
                        SoeFormEntryBase control = obj as SoeFormEntryBase;
                        string value = PreviousForm[control.Name];
                        if (!String.IsNullOrEmpty(value))
                            control.Value = value;
                        else
                            control.Value = null;
                    }
            }

            foreach (Control ctrl in obj.Controls)
            {
                FindControls(ctrl);
            }
        }

        public void SetTabHeaderText(int tabNr, string headerText)
        {
            int i = 1;
            foreach (Control control in Tabs.Controls)
            {
                Tab tab = control as Tab;
                if (tab != null)
                {
                    if (i == tabNr)
                    {
                        tab.HeaderTextSetting = headerText;
                        return;
                    }

                    i++;
                }
            }
        }

        #endregion
    }
}
