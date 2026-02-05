using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.preferences.systeminfosettings
{
    public partial class Default : PageBase
    {
        #region Variables

        private PayrollManager pm;
        private SettingManager sm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences_SystemInfoSettings;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new PayrollManager(ParameterObject);
            sm = new SettingManager(ParameterObject);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            /*DefaultRole.ConnectDataSource(roleDict);

            PasswordLengthInstruction.LabelSetting = String.Format(GetText(3789, "Intervallet måste vara mellan {0} och {1} tecken"), Constants.PASSWORD_DEFAULT_MIN_LENGTH, Constants.PASSWORD_DEFAULT_MAX_LENGTH);*/

            #endregion

            #region Set data

            SystemInfoSetting setting;
            
            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSkill_Use, SoeCompany.ActorCompanyId);
            UseSkill.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSkill_Ends, SoeCompany.ActorCompanyId);
            SkillDaysInAdvance.Value = setting != null && setting.IntData != null ? setting.IntData.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSchedule_Use, SoeCompany.ActorCompanyId);
            UsePlacement.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSchedule_Ends, SoeCompany.ActorCompanyId);
            PlacementDaysInAdvanced.Value = setting != null && setting.IntData != null ? setting.IntData.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks_Use, SoeCompany.ActorCompanyId);
            UseClosePreliminaryTimeScheduleTemplateBlocks.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks, SoeCompany.ActorCompanyId);
            PreliminaryTimeScheduleTemplateBlocksDaysInAdvanced.Value = setting != null && setting.IntData != null ? setting.IntData.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.AttestReminder_Use, SoeCompany.ActorCompanyId);
            UseAttestReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllness, SoeCompany.ActorCompanyId);
            UseIllnessReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessDays, SoeCompany.ActorCompanyId);
            if (setting != null && setting.IntData != null && setting.IntData != 0)
            {
                ReminderDaysAfterIlnessStarted.Value = setting.IntData.Value.ToString();
            }
            else
            {
                //Default from SysPayrollPrice
                var sysPayrollPrice = pm.GetSysPayrollPrice(SoeCompany.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_Absence_MedicalCertificateDays, null);
                if (sysPayrollPrice != null)
                    ReminderDaysAfterIlnessStarted.Value = ((int)sysPayrollPrice.Amount).ToString();
            }

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessSocialInsuranceAgency, SoeCompany.ActorCompanyId);
            UseIllnessReminderSIA.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessDaysSocialInsuranceAgency, SoeCompany.ActorCompanyId);
            ReminderDaysAfterIlnessStartedSIA.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessEmailSocialInsuranceAgency, SoeCompany.ActorCompanyId);
            ReminderIllnessEmailSocialInsuranceAgency.Value = setting != null && setting.StrData != null ? setting.StrData : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderEmployment, SoeCompany.ActorCompanyId);
            UseEmploymentReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderEmploymentDays, SoeCompany.ActorCompanyId);
            ReminderDaysBeforeEmploymentEnds.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.PublishScheduleAutomaticly, SoeCompany.ActorCompanyId);
            PublishScheduleAutomaticlyDaysInAdvanced.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : ""; 

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.PublishScheduleAutomaticly_Use, SoeCompany.ActorCompanyId);
            UsePublishScheduleAutomaticly.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderOrderSchedule, SoeCompany.ActorCompanyId);
            ReminderOrderScheduleDaysInAdvance.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderOrderSchedule_Use, SoeCompany.ActorCompanyId);
            UseReminderOrderSchedule.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            //EmployeeExperience (reached)
            setting = sm.GetSystemInfoSetting((int)SystemInfoType.UseEmployeeExperienceReminder, SoeCompany.ActorCompanyId);
            UseEmployeeExperienceReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeExperienceReminderMonths, SoeCompany.ActorCompanyId);
            EmployeeExperienceReminderMonths.Value = setting != null && setting.StrData != null ? setting.StrData : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderDaysBeforeEmployeeExperienceReached, SoeCompany.ActorCompanyId);
            ReminderDaysBeforeEmployeeExperienceReached.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            //Update EmployeeExperience
            setting = sm.GetSystemInfoSetting((int)SystemInfoType.UseUpdateEmployeeExperienceReminder, SoeCompany.ActorCompanyId);
            UseUpdateExperienceReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            //EmployeeAge
            setting = sm.GetSystemInfoSetting((int)SystemInfoType.UseEmployeeAgeReminder, SoeCompany.ActorCompanyId);
            UseEmployeeAgeReminder.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeAgeReminderAges, SoeCompany.ActorCompanyId);
            EmployeeAgeReminderAges.Value = setting != null && setting.StrData != null ? setting.StrData : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderDaysBeforeEmployeeAgeReached, SoeCompany.ActorCompanyId);
            ReminderDaysBeforeEmployeeAgeReached.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            //ReminderAfterLongAbsence
            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderAfterLongAbsence, SoeCompany.ActorCompanyId);
            ReminderAfterLongAbsence.Value = setting != null && setting.BoolData != null ? setting.BoolData.ToString() : "false";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderAfterLongAbsenceDaysInAdvance, SoeCompany.ActorCompanyId);
            ReminderAfterLongAbsenceDaysInAdvance.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            setting = sm.GetSystemInfoSetting((int)SystemInfoType.IsReminderAfterLongAbsenceAfterDays, SoeCompany.ActorCompanyId);
            IsReminderAfterLongAbsenceAfterDays.Value = setting != null && setting.IntData != null ? setting.IntData.Value.ToString() : "";

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "UPDATED")
                Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            ValidateForm();

            bool success = true;

            #region int

            var values = new Dictionary<int, object>();

            values.Add((int)SystemInfoType.EmployeeSkill_Ends, StringUtility.GetInt(F["SkillDaysInAdvance"],0));
            values.Add((int)SystemInfoType.EmployeeSchedule_Ends, StringUtility.GetInt(F["PlacementDaysInAdvanced"], 0));
            values.Add((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks, StringUtility.GetInt(F["PreliminaryTimeScheduleTemplateBlocksDaysInAdvanced"], 0));
            values.Add((int)SystemInfoType.PublishScheduleAutomaticly, StringUtility.GetInt(F["PublishScheduleAutomaticlyDaysInAdvanced"], 0));
            values.Add((int)SystemInfoType.ReminderIllnessDays, StringUtility.GetInt(F["ReminderDaysAfterIlnessStarted"], 0));
            values.Add((int)SystemInfoType.ReminderIllnessDaysSocialInsuranceAgency, StringUtility.GetInt(F["ReminderDaysAfterIlnessStartedSIA"], 0));
            values.Add((int)SystemInfoType.ReminderEmploymentDays, StringUtility.GetInt(F["ReminderDaysBeforeEmploymentEnds"], 0));
            values.Add((int)SystemInfoType.ReminderOrderSchedule, StringUtility.GetInt(F["ReminderOrderScheduleDaysInAdvance"], 0));
            values.Add((int)SystemInfoType.ReminderDaysBeforeEmployeeAgeReached, StringUtility.GetInt(F["ReminderDaysBeforeEmployeeAgeReached"], 0));
            values.Add((int)SystemInfoType.ReminderDaysBeforeEmployeeExperienceReached, StringUtility.GetInt(F["ReminderDaysBeforeEmployeeExperienceReached"], 0));
            values.Add((int)SystemInfoType.ReminderAfterLongAbsenceDaysInAdvance, StringUtility.GetInt(F["ReminderAfterLongAbsenceDaysInAdvance"], 0));
            values.Add((int)SystemInfoType.IsReminderAfterLongAbsenceAfterDays, StringUtility.GetInt(F["IsReminderAfterLongAbsenceAfterDays"], 0));

            if (!sm.AddUpdateSystemInfoSettings(values, (int)SettingDataType.Integer, SoeCompany.ActorCompanyId).Success)
                success = false;

            #endregion

            #region bool

            values = new Dictionary<int, object>();

            values.Add((int)SystemInfoType.EmployeeSkill_Use, StringUtility.GetBool(F["UseSkill"]));
            values.Add((int)SystemInfoType.EmployeeSchedule_Use, StringUtility.GetBool(F["UsePlacement"]));
            values.Add((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks_Use, StringUtility.GetBool(F["UseClosePreliminaryTimeScheduleTemplateBlocks"]));
            values.Add((int)SystemInfoType.AttestReminder_Use, StringUtility.GetBool(F["UseAttestReminder"]));
            values.Add((int)SystemInfoType.ReminderIllness, StringUtility.GetBool(F["UseIllnessReminder"]));
            values.Add((int)SystemInfoType.ReminderIllnessSocialInsuranceAgency, StringUtility.GetBool(F["UseIllnessReminderSIA"]));
            values.Add((int)SystemInfoType.ReminderEmployment, StringUtility.GetBool(F["UseEmploymentReminder"]));
            values.Add((int)SystemInfoType.PublishScheduleAutomaticly_Use, StringUtility.GetBool(F["UsePublishScheduleAutomaticly"]));
            values.Add((int)SystemInfoType.ReminderOrderSchedule_Use, StringUtility.GetBool(F["UseReminderOrderSchedule"]));
            values.Add((int)SystemInfoType.UseEmployeeAgeReminder, StringUtility.GetBool(F["UseEmployeeAgeReminder"]));
            values.Add((int)SystemInfoType.UseEmployeeExperienceReminder, StringUtility.GetBool(F["UseEmployeeExperienceReminder"]));
            values.Add((int)SystemInfoType.UseUpdateEmployeeExperienceReminder, StringUtility.GetBool(F["UseUpdateExperienceReminder"]));
            values.Add((int)SystemInfoType.ReminderAfterLongAbsence, StringUtility.GetBool(F["ReminderAfterLongAbsence"]));

            if (!sm.AddUpdateSystemInfoSettings(values, (int)SettingDataType.Boolean, SoeCompany.ActorCompanyId).Success)
                success = false;

            #endregion

            #region string

            values = new Dictionary<int, object>();

            values.Add((int)SystemInfoType.EmployeeAgeReminderAges, F["EmployeeAgeReminderAges"]);
            values.Add((int)SystemInfoType.EmployeeExperienceReminderMonths, F["EmployeeExperienceReminderMonths"]);
            values.Add((int)SystemInfoType.ReminderIllnessEmailSocialInsuranceAgency, F["ReminderIllnessEmailSocialInsuranceAgency"]);

            if (!sm.AddUpdateSystemInfoSettings(values, (int)SettingDataType.String, SoeCompany.ActorCompanyId).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        protected override void ValidateForm()
        {
            // Validate password policy settings
            /*int passwordMinLength = StringUtility.GetNumeric(F["PasswordMinLength"], Constants.PASSWORD_DEFAULT_MIN_LENGTH);
            if (passwordMinLength < Constants.PASSWORD_DEFAULT_MIN_LENGTH || passwordMinLength > Constants.PASSWORD_DEFAULT_MAX_LENGTH)
                RedirectToSelf("INVALID_PASSWORD_SETTINGS", true);

            int passwordMaxLength = StringUtility.GetNumeric(F["PasswordMaxLength"], Constants.PASSWORD_DEFAULT_MAX_LENGTH);
            if (passwordMaxLength < Constants.PASSWORD_DEFAULT_MIN_LENGTH || passwordMaxLength > Constants.PASSWORD_DEFAULT_MAX_LENGTH)
                RedirectToSelf("INVALID_PASSWORD_SETTINGS", true);

            if (StringUtility.GetBool(F["UseDefaultEmailAddress"]) == true && F["DefaultEmailAddress"] == String.Empty)
                RedirectToSelf("NO_EMAILADDRESS_DEFINED", true);*/
        }

        #endregion
    }
}
