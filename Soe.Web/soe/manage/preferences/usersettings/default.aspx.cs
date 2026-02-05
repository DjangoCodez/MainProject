using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Web.Util;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.manage.preferences.usersettings
{
    public partial class Default : PageBase
    {
        #region Variables

        protected SettingManager sm;
        protected CompanyManager cm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            // Language
            UserLangId.ConnectDataSource(GetGrpText(TermGroup.Language));
            UserCompanyId.ConnectDataSource(cm.GetCompaniesByLicense(SoeLicense.LicenseId), "Name", "ActorCompanyId");

            #endregion

            #region Set data

            // Get user settings
			int langId = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.CoreLangId, UserId, SoeCompany.ActorCompanyId, 0);
            UserLangId.Value = langId.ToString();

			int companyId = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.CoreCompanyId, UserId, SoeCompany.ActorCompanyId, 0);
            UserCompanyId.Value = companyId.ToString();

            bool showAnimations = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.CoreShowAnimations, UserId, SoeCompany.ActorCompanyId, 0);
            UserShowAnimations.Value = showAnimations.ToString();

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "UPDATED")
                Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");

            #endregion
        }

        protected override void Save()
        {            
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)UserSettingType.CoreShowAnimations, StringUtility.GetBool(F["UserShowAnimations"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.User, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)UserSettingType.CoreLangId, StringUtility.GetInt(F["UserLangId"], 0));
            intValues.Add((int)UserSettingType.CoreCompanyId, StringUtility.GetInt(F["UserCompanyId"], 0));

            if (!sm.UpdateInsertIntSettings(SettingMainType.User, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }
    }
}
