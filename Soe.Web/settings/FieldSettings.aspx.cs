using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.Settings
{
    public partial class FieldSettings : PageBase
    {
        #region Variables

        private FieldSettingManager fsm;

		protected int formId;
		protected int fieldId;
		protected string url;
        protected bool required;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Common_Field;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            fsm = new FieldSettingManager(ParameterObject);

            if (!int.TryParse(QS["form"], out formId))
                throw new SoeQuerystringException("form", this.ToString());

            if (!int.TryParse(QS["field"], out fieldId))
                throw new SoeQuerystringException("field", this.ToString());

            if (Int32.TryParse(QS["tab"], out int activeTab))
                Form1.ActiveTab = activeTab;

            required = StringUtility.GetBool(QS["req"]);

            //Check RoleSettings permission
            if (!HasRolePermission(Feature.Common_Field_Role, Permission.Readonly))
			{
				RoleSettings.Visible = false;
				Form1.ActiveTab = 2;
			}

			//Check CompanySettings permission
			if (!HasRolePermission(Feature.Common_Field_Company, Permission.Readonly))
			{
				CompanySettings.Visible = false;
				Form1.ActiveTab = 1;
			}

			if (!RoleSettings.Visible && !CompanySettings.Visible)
				Response.Redirect(Request.UrlReferrer.ToString());

            if (Request.UrlReferrer?.LocalPath?.ToString() != "/Settings/FormSettings.aspx" && Session[Constants.SESSION_REFERRER_URL] == null)
			    Session[Constants.SESSION_REFERRER_URL] = Request.UrlReferrer.ToString();

            #endregion

            #region Actions

            if (Form1.IsPosted)
			{
				SaveFieldSettings();
				RedirectToSelf("SAVED");
            }

            #endregion

            #region Populate

            Dictionary<int, string> grpYesNoDefault = GetGrpText(TermGroup.YesNoDefault);

            RoleFieldVisible.ConnectDataSource(grpYesNoDefault);
            RoleFieldTabStop.ConnectDataSource(grpYesNoDefault);
            RoleFieldReadOnly.ConnectDataSource(grpYesNoDefault);
            RoleFieldBoldLabel.ConnectDataSource(grpYesNoDefault);
            if (required)
                RoleFieldVisible.ReadOnly = true;

            CompanyFieldVisible.ConnectDataSource(grpYesNoDefault);
            CompanyFieldTabStop.ConnectDataSource(grpYesNoDefault);
            CompanyFieldReadOnly.ConnectDataSource(grpYesNoDefault);
            CompanyFieldBoldLabel.ConnectDataSource(grpYesNoDefault);
            if (required)
                RoleFieldVisible.ReadOnly = true;

            #endregion

            #region Set data

            #region Default values

            Field field = fsm.GetField(fieldId, formId);
            if (field?.ReadOnly != null)
            {
                RoleFieldReadOnly.ReadOnly = Convert.ToBoolean(field.ReadOnly);
                if (RoleFieldReadOnly.ReadOnly)
                    RoleFieldReadOnly.Value = ((int)TermGroup_YesNoDefault.Yes).ToString();

                CompanyFieldReadOnly.ReadOnly = Convert.ToBoolean(field.ReadOnly);
                if (CompanyFieldReadOnly.ReadOnly)
                    CompanyFieldReadOnly.Value = ((int)TermGroup_YesNoDefault.Yes).ToString();
            }

            #endregion

            #region RoleFieldSettings

            RoleFieldVisible.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            RoleFieldTabStop.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            RoleFieldReadOnly.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            RoleFieldBoldLabel.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);

            FieldSetting roleFieldSetting = fsm.GetFieldSettingForRole(formId, fieldId, RoleId);
            if (roleFieldSetting != null)
            {
                foreach (FieldSettingDetail fieldSettingDetail in roleFieldSetting.GetFieldSettingDetails())
                {
                    switch (fieldSettingDetail.SysSettingId)
                    {
                        case (int)SoeSetting.Label:
                            RoleFieldLabel.Value = fieldSettingDetail.Value;
                            break;
                        case (int)SoeSetting.Visible:
                            RoleFieldVisible.Value = fieldSettingDetail.Value;
                            break;
                        case (int)SoeSetting.SkipTabStop:
                            RoleFieldTabStop.Value = fieldSettingDetail.Value;
                            break;
                        case (int)SoeSetting.ReadOnly:
                            //Don't overwrite default value
                            if (!RoleFieldReadOnly.ReadOnly)
                                RoleFieldReadOnly.Value = fieldSettingDetail.Value;
                            break;
                        case (int)SoeSetting.BoldLabel:
                            RoleFieldBoldLabel.Value = fieldSettingDetail.Value;
                            break;
                    }
                }
            }

            #endregion

            #region CompanyFieldSettings
            
            CompanyFieldVisible.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            CompanyFieldTabStop.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            CompanyFieldReadOnly.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);
            CompanyFieldBoldLabel.Value = Convert.ToString((int)TermGroup_YesNoDefault.Default);

            FieldSetting companyFieldSetting = fsm.GetFieldSettingForCompany(formId, fieldId, SoeCompany.ActorCompanyId);
			if (companyFieldSetting != null)
			{
				foreach (FieldSettingDetail fieldSettingDetail in companyFieldSetting.GetFieldSettingDetails())
				{
					switch (fieldSettingDetail.SysSettingId)
					{
						case (int)SoeSetting.Label:
							CompanyFieldLabel.Value = fieldSettingDetail.Value;
							break;
						case (int)SoeSetting.Visible:
							CompanyFieldVisible.Value = fieldSettingDetail.Value;
							break;
						case (int)SoeSetting.SkipTabStop:
							CompanyFieldTabStop.Value = fieldSettingDetail.Value;
							break;
						case (int)SoeSetting.ReadOnly:
							//Don't overwrite default value
							if(!CompanyFieldReadOnly.ReadOnly)
								CompanyFieldReadOnly.Value = fieldSettingDetail.Value;
							break;
						case (int)SoeSetting.BoldLabel:
							CompanyFieldBoldLabel.Value = fieldSettingDetail.Value;
							break;
					}
				}
            }

            #endregion

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf) && MessageFromSelf == "SAVED")
            {
                string referrerUrl = Session[Constants.SESSION_REFERRER_URL].ToString();
                Session.Remove(Constants.SESSION_REFERRER_URL);
                Response.Redirect(referrerUrl);
            }

            #endregion
        }

        #region Action-methods

        private void SaveFieldSettings()
		{
            int roleId = this.RoleId;
            int actorCompanyId = SoeCompany.ActorCompanyId;

            fsm.SaveRoleFieldSetting(formId, fieldId, (int)SoeSetting.Label, F["RoleFieldLabel"], roleId);
            if(!required)
                fsm.SaveRoleFieldSetting(formId, fieldId, (int)SoeSetting.Visible, F["RoleFieldVisible"], roleId);
            fsm.SaveRoleFieldSetting(formId, fieldId, (int)SoeSetting.SkipTabStop, F["RoleFieldTabStop"], roleId);
            fsm.SaveRoleFieldSetting(formId, fieldId, (int)SoeSetting.ReadOnly, F["RoleFieldReadOnly"], roleId);
            fsm.SaveRoleFieldSetting(formId, fieldId, (int)SoeSetting.BoldLabel, F["RoleFieldBoldLabel"], roleId);

            fsm.SaveCompanyFieldSetting(formId, fieldId, (int)SoeSetting.Label, F["CompanyFieldLabel"], actorCompanyId);
            if(!required)
                fsm.SaveCompanyFieldSetting(formId, fieldId, (int)SoeSetting.Visible, F["CompanyFieldVisible"], actorCompanyId);
            fsm.SaveCompanyFieldSetting(formId, fieldId, (int)SoeSetting.SkipTabStop, F["CompanyFieldTabStop"], actorCompanyId);
            fsm.SaveCompanyFieldSetting(formId, fieldId, (int)SoeSetting.BoldLabel, F["CompanyFieldBoldLabel"], actorCompanyId);
        }

        #endregion
    }
}
