using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.Controls
{
    /// <summary>
    /// Extension Methods for Controls
    /// </summary>
    public static class ControlExtensions
    {
        #region Variables

        private static SoeFieldSettingType type = SoeFieldSettingType.Web;

        #endregion

        #region Form

        /// <summary>
        /// Extension Method for Form to implement Settings icons on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderSettingsIcons(this Form form, HtmlTextWriter writer)
        {
            //Deprecated
        }

        /// <summary>
        /// Extension Method for Form to implement Modify icons on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderModifyIcons(this Form form, HtmlTextWriter writer)
        {
            if (form.EnableCopy)
            {
                writer.Write("<button");
                writer.WriteAttribute("id", "CopyPost");
                writer.WriteAttribute("name", "CopyPost");
                writer.WriteAttribute("title", ((PageBase)HttpContext.Current.Handler).GetText(1227, "Kopiera post"));
                writer.WriteAttribute("tabindex", "-1");
                writer.WriteAttribute("runat", "server");
                writer.Write(">");
                writer.Write("<span");
                writer.WriteAttribute("class", "fal fa-clone");
                writer.Write(">");
                writer.Write("</span>");
                writer.Write("</button>");
            }
        }

        /// <summary>
        /// Extension Method for Form to implement switch labeltext icon on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderSwitchLabelTextIcon(this Form form, HtmlTextWriter writer)
        {
            #region Deprecated
            /*
            if (!form.DisableSwitchLabelText)
            {
                writer.Write("<a");
                writer.WriteAttribute("title", ((PageBase)HttpContext.Current.Handler).GetText(1766, "Skifta ledtext och datanamn"));
                writer.WriteAttribute("tabindex", "-1");
                writer.WriteAttribute("href", "javascript:;");
                writer.WriteAttribute("onclick", "showHideClasses('LabelText','LabelDataText');");
                writer.Write(">");
                writer.Write("<span");
                writer.WriteAttribute("class", "fal fa-info");
                writer.Write(">");
                writer.Write("</span>");
                writer.Write("</a>");
            }
            */
            #endregion
        }

        /// <summary>
        /// Extension Method for Form to implement Settings icons on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderNavigationIcons(this Form form, HtmlTextWriter writer)
        {
            // PrevNext
            if (form.EnablePrevNext)
            {
                #region Prev button

                writer.Write("<button");
                writer.WriteAttribute("id", "Prev");
                writer.WriteAttribute("name", "Prev");
                writer.WriteAttribute("title", ((PageBase)HttpContext.Current.Handler).GetText(2139, "Föregående post"));
                writer.WriteAttribute("tabindex", "-1");
                writer.WriteAttribute("runat", "server");
                writer.Write(">");
                writer.Write("<span");
                writer.WriteAttribute("class", "fal fa-long-arrow-left");
                writer.Write(">");
                writer.Write("</span>");
                writer.Write("</button>");

                #endregion

                #region Next button

                writer.Write("<button");
                writer.WriteAttribute("id", "Next");
                writer.WriteAttribute("name", "Next");
                writer.WriteAttribute("title", ((PageBase)HttpContext.Current.Handler).GetText(2140, "Nästa post"));
                writer.WriteAttribute("tabindex", "-1");
                writer.WriteAttribute("runat", "server");
                writer.Write(">");
                writer.Write("<span");
                writer.WriteAttribute("class", "fal fa-long-arrow-right");
                writer.Write(">");
                writer.Write("</span>");
                writer.Write("</button>");

                #endregion
            }
        }

        /// <summary>
        /// Extension Method for Form to implement Links on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderLinks(this Form form, HtmlTextWriter writer)
        {
            if (form.Links != null && form.Links.Count > 0)
            {
                var ul = new HtmlGenericControl("ul");
                ul.ID = "actionLinks";
                ul.Visible = false;

                foreach (Link link in form.Links)
                {
                    ul.Visible = true;

                    var li = new HtmlGenericControl("li");
                    li.Controls.Add(link);

                    ul.Controls.Add(li);
                }
                ul.RenderControl(writer);
            }
        }

        /// <summary>
        /// Renders a "dummy button"
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writer"></param>
        public static void RenderDummyButton(this Form form, HtmlTextWriter writer)
        {
            Button suppressLink = new Button();
            suppressLink.ID = "SuppressEnter";
            suppressLink.Width = 1;
            suppressLink.Height = 1;
            suppressLink.BackColor = System.Drawing.Color.White;
            suppressLink.BorderColor = System.Drawing.Color.White;
            suppressLink.Style["cursor"] = "default";
            suppressLink.TabIndex = -1;
            suppressLink.RenderControl(writer);
        }

        /// <summary>
        /// Extension Method for Form to implement Sumbit icons on the Form
        /// </summary>
        /// <param name="form">Form control</param>
        /// <param name="writer">The HtmlTextWriter</param>
        public static void RenderButtons(this Form form, HtmlTextWriter writer)
        {
            if (form.EnableBack)
            {
                SoeButton btn = new SoeButton()
                {
                    ID = SoeForm.SOEFORM_BUTTON_BACK,
                    Type = SoftOne.Soe.Web.UI.WebControls.SoeButtonType.Submit,
                    Text = ((PageBase)HttpContext.Current.Handler).GetText(84, "Tillbaka"),
                    SecondarySubmit = true
                };
                btn.RenderControl(writer);
            }

            if (form.EnableDelete)
            {
                SoeButton btn = new SoeButton()
                {
                    ID = SoeForm.SOEFORM_BUTTON_DELETEPOST,
                    Type = SoftOne.Soe.Web.UI.WebControls.SoeButtonType.Delete,
                    Text = form.GetText(2185, "Ta bort"),
                };
                btn.RenderControl(writer);
            }

            if (form.EnableRunReport)
            {
                SoeButton btn = new SoeButton()
                {
                    ID = SoeForm.SOEFORM_BUTTON_RUNREPORT,
                    Type = SoftOne.Soe.Web.UI.WebControls.SoeButtonType.Submit,
                    Text = form.GetText(1452, "Skriv ut rapport"),
                };
                btn.RenderControl(writer);
            }

            if (!form.DisableSave)
            {
                SoeButton button = new SoeButton()
                {
                    ID = SoeForm.SOEFORM_BUTTON_SUBMIT,
                    Type = SoftOne.Soe.Web.UI.WebControls.SoeButtonType.Submit,
                    Text = form.ButtonText,
                };
                button.RenderControl(writer);
            }
        }

        #endregion

        #region IEntryControl

        /// <summary>
        /// Extension Method for IEntryControl to implement field setting icon
        /// </summary>
        /// <param name="control">IEntryControl interface</param>
        /// <param name="writer">The HtmlTextWriter</param>
        /// <param name="formId">The controls FormId</param>
        /// <param name="fieldId">The controls FieldId</param>
        /// <param name="required">True if the control is required in GUI</param>
        public static void RenderFieldSettingIcon(this IEntryControl control, HtmlTextWriter writer, int formId, int fieldId, bool required)
        {
            if (!control.DisableSettings)
            {
                var pageBase = (PageBase)HttpContext.Current.Handler;
                if (pageBase != null && pageBase.SoeUser != null)
                {
                    int roleId = pageBase.RoleId;
                    FeatureManager fm = new FeatureManager(pageBase.ParameterObject);

                    if (pageBase.HasRolePermission(Feature.Common_Field, Permission.Readonly))
                    {
                        string url = "/Settings/FieldSettings.aspx?form=" + formId + "&field=" + fieldId;
                        if (required)
                            url += "&req=1";

                        Link link = new Link()
                        {
                            Href = url,
                            ImageSrc = "/img/gear_view.png",
                            Alt = pageBase.GetText(1025, "Fältinställningar"),
                            CssClass = "SettingsNav",
                            SkipTabstop = true,
                            Invisible = true
                        };
                        link.RenderControl(writer);
                    }
                }
            }
        }

        private static string currenFormName;
        private static IEnumerable<Field> currentFields;

        /// <summary>
        /// Extension Method for IEntryControl to enable Field Settings for dynamically added controls
        /// Check and apply FieldSettings for given IBaseEntry
        /// </summary>
        /// <param name="control">The IBaseEntry Control to find FieldProperties for</param>
        /// <param name="fsm">Instance of FieldSettingManager</param>
        /// <param name="form">The SoftOne.Soe.Data.Form the field exist in</param>
        /// <param name="formName">The SoeForm the field exist in</param>
        /// <param name="settings">The settings for the SoeForm</param>
        /// <param name="stopSettings">True if Settings are stopped, otherwise false</param>
        /// <param name="relaseMode">True if is in RelaseMode</param>
        public static void CheckFieldProperties(this IEntryControl control, FieldSettingManager fsm, SoftOne.Soe.Data.Form form, string formName, bool stopSettings, List<SettingObject> settings)
        {
            if (!control.DisableSettings)
            {
                Field field = null;

                if (!stopSettings)
                {
                    //Cache Fields on the current Form
                    if (currenFormName != formName || currentFields == null)
                    {
                        currentFields = fsm.GetFields(formName, type);
                        currenFormName = formName;
                    }

                    field = currentFields.FirstOrDefault(f => f.Name == control.ID);
                    if (field != null)
                    {
                        //Get all FieldSettings for the Field
                        FieldSetting fieldSetting = fsm.FilterFieldSetting(field, settings);
                        if (fieldSetting != null)
                        {
                            #region FieldSetting

                            foreach (FieldSettingDetail fieldSettingDetail in fieldSetting.GetFieldSettingDetails())
                            {
                                switch (fieldSettingDetail.SysSettingId)
                                {
                                    case (int)SoeSetting.Label:
                                        if (fieldSettingDetail.HasValue)
                                            control.LabelSetting = fieldSettingDetail.Value;
                                        break;
                                    case (int)SoeSetting.Visible:
                                        if (fieldSettingDetail.HasValue)
                                            control.Visible = StringUtility.GetBool(fieldSettingDetail.Value);
                                        break;
                                    case (int)SoeSetting.SkipTabStop:
                                        if (fieldSettingDetail.HasValue)
                                            control.SkipTabStop = StringUtility.GetBool(fieldSettingDetail.Value);
                                        break;
                                    case (int)SoeSetting.ReadOnly:
                                        if (fieldSettingDetail.HasValue)
                                            control.ReadOnly = StringUtility.GetBool(fieldSettingDetail.Value);
                                        break;
                                    case (int)SoeSetting.BoldLabel:
                                        if (fieldSettingDetail.HasValue)
                                            control.BoldLabel = StringUtility.GetBool(fieldSettingDetail.Value);
                                        break;
                                }
                            }

                            #endregion
                        }
                    }
                    else
                    {
                        field = fsm.AddField(control.ID, formName, control.ReadOnly, type);

                        //Reset cache
                        currentFields = null;
                        currenFormName = "";
                    }
                }

                if (form != null)
                    control.FormId = form.FormId;
                if (field != null)
                    control.FieldId = field.FieldId;
            }
        }

        #endregion

        #region IFormControl

        public static string GetText(this IFormControl control, int termId, string defaultTerm)
        {
            return ((PageBase)HttpContext.Current.Handler).GetText(termId, defaultTerm);
        }

        #endregion
    }
}
