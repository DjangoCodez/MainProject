using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.Settings
{
    public partial class FormSettings : PageBase
    {
        #region Variables

        private FieldSettingManager fsm = null;

        protected string RedirectUrl;
		protected string title;
		protected string url;
		protected int formId;
		protected int fieldId;
		protected int? actorCompanyId;
		protected int? roleId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Common_Form;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            fsm = new FieldSettingManager(ParameterObject);

			//Mandatory parameters
			if (!Int32.TryParse(QS["form"], out formId))
                throw new SoeQuerystringException("form", this.ToString());

			if (Session[Constants.SESSION_REFERRER_URL] == null)
				Session[Constants.SESSION_REFERRER_URL] = Request.UrlReferrer.ToString();
            RedirectUrl = Session[Constants.SESSION_REFERRER_URL].ToString();

            SoeGrid1.Title = GetText(5424, "Inställningar");

            #endregion

            #region Actions

            int delete;
			if (Int32.TryParse(QS["delete"], out delete) && delete == 1)
			{
				if(Int32.TryParse(QS["field"], out fieldId) && fieldId > 0)
				{
					string company = QS["company"];
					if(!String.IsNullOrEmpty(company))
						actorCompanyId = Convert.ToInt32(company);

					string role = QS["role"];
					if(!String.IsNullOrEmpty(role))
						roleId = Convert.ToInt32(role);

					bool deleted = DeleteSetting();
                    if (deleted)
                        Response.Redirect(Request.Url.AbsolutePath + "?form=" + formId);
				}
            }

            #endregion

            #region Populate

            SoeGrid1.DataSource = fsm.GetSettingsForForm(formId, RoleId, SoeCompany.ActorCompanyId);
			SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
			SoeGrid1.DataBind();

            #endregion
        }

        #region Action-methods

        private bool DeleteSetting()
		{
			if (actorCompanyId != null)
			{
                CompanyFieldSetting companyFieldSetting = fsm.GetCompanyFieldSetting(fieldId, formId, Convert.ToInt32(actorCompanyId));
                if (companyFieldSetting != null)
                    return fsm.DeleteCompanyFieldSetting(companyFieldSetting).Success;

			}
			else if (roleId != null)
			{
                RoleFieldSetting roleFieldSettings = fsm.GetRoleFieldSetting(fieldId, formId, Convert.ToInt32(roleId));
				if (roleFieldSettings != null)
                    return fsm.DeleteRoleFieldSetting(roleFieldSettings).Success;
			}

			return false;
        }

        #endregion

        #region Help-methods

        protected void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			SettingObject settingObject = ((e.Row.DataItem) as SettingObject);
			if (settingObject != null)
			{
				//Setting name
				SysSetting sysSetting = fsm.GetSysSetting(settingObject.SysSettingId);
				if (sysSetting != null)
				{
					PlaceHolder setting = (PlaceHolder)e.Row.FindControl("phSetting");
					if (setting != null)
					{
						Label lbl = new Label();
						lbl.Text = TextService.GetText(sysSetting.SysTermId);
						setting.Controls.Add(lbl);
					}
				}

				//Dimension (Company or Role)
				PlaceHolder dimension = (PlaceHolder)e.Row.FindControl("phDimension");
				if (dimension != null)
				{
					Label lbl = new Label();
					if (settingObject.ActorCompanyId != null)
						lbl.Text = GetText(1115, "Företag");
					else
						lbl.Text = GetText(1116, "Roll");
					dimension.Controls.Add(lbl);
				}

				//Value
				PlaceHolder value = (PlaceHolder)e.Row.FindControl("phValue");
				if (value != null)
				{
					Label lbl = new Label();
					switch (settingObject.SysSettingId)
					{
						case (int) SoeSetting.Label:
							lbl.Text = settingObject.Value;
							break;
						case (int)SoeSetting.Visible:
							lbl.Text = TextService.GetText(Convert.ToInt32(settingObject.Value), (int)TermGroup.YesNoDefault);
							break;
						case (int)SoeSetting.SkipTabStop:
							lbl.Text = TextService.GetText(Convert.ToInt32(settingObject.Value), (int)TermGroup.YesNoDefault);
							break;
						case (int)SoeSetting.ReadOnly:
							lbl.Text = TextService.GetText(Convert.ToInt32(settingObject.Value), (int)TermGroup.YesNoDefault);
							break;
						case (int)SoeSetting.BoldLabel:
							lbl.Text = TextService.GetText(Convert.ToInt32(settingObject.Value), (int)TermGroup.YesNoDefault);
							break;
					}
					value.Controls.Add(lbl);
				}
			}
        }

        #endregion
    }
}
