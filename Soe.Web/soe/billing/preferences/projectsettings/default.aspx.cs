using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;
using System.Linq;

namespace SoftOne.Soe.Web.soe.billing.preferences.projectsettings
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;
        private TimeCodeManager tcm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProjectSettings;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);
            tcm = new TimeCodeManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true, "", "");

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate
            //Dictionary<int, string> timeCodesDict = tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false, (int)SoeTimeCodeType.Work, (int)SoeTimeCodeType.Material);
            TimeCodeSelectEntry.ConnectDataSource(tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false, (int)SoeTimeCodeType.Work, (int)SoeTimeCodeType.Material));

            //TimeCodeSelectEntry.ConnectDataSource(timeCodesDict, "Value", "Key");

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> projectSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Project, SoeCompany.ActorCompanyId);

            //General
            AutoCreateProjectOnNewInvoice.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, (int)SettingDataType.Boolean);
            IncludeTimeProjectReport.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectIncludeTimeProjectReport, (int)SettingDataType.Boolean);
            MoveTransactionToInvoiceRow.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectCreateInvoiceRowFromTransaction, (int)SettingDataType.Boolean);
            // Default is true
            UseOrderNumberAsProjectNumber.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber, (int)SettingDataType.Boolean);
            LimitOrderToProjectUsers.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, (int)SettingDataType.Boolean, Boolean.TrueString);
           //KeepEmployeesOnWeekChange.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectKeepEmployeesOnWeekChange, (int)SettingDataType.Boolean, Boolean.TrueString);
            ChargeCostsToProject.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectChargeCostsToProject, (int)SettingDataType.Boolean, Boolean.TrueString);
            IncludeOnlyInvoicedTimeInTimeProjectReport.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, (int)SettingDataType.Boolean, Boolean.TrueString);
            UseCustomerNameAsProjectName.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectUseCustomerNameAsProjectName, (int)SettingDataType.Boolean, Boolean.TrueString);

            OverheadCostAsFixedAmount.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, (int)SettingDataType.Boolean);
            OverheadCostAsAmountPerHour.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, (int)SettingDataType.Boolean);

            AutosaveOnWeekChangeInOrder.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectAutosaveOnWeekChangeInOrder, (int)SettingDataType.Boolean);

            InvoiceTimeAsWorkTime.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectInvoiceTimeAsWorkTime, (int)SettingDataType.Boolean);

            ExtendedTimeRegistration.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, (int)SettingDataType.Boolean);
            CreateTransactionsBaseOnTimeRules.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules, (int)SettingDataType.Boolean);

            TimeCodeSelectEntry.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectDefaultTimeCodeId, (int)SettingDataType.Integer);

            ProjectAutoUpdateAccountSettings.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectAutoUpdateAccountSettings, (int)SettingDataType.Boolean);
            ProjectAutoUpdateInternalAccounts.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectAutoUpdateInternalAccounts, (int)SettingDataType.Boolean);

            //Lock certain settings

            if (StringUtility.GetBool(ExtendedTimeRegistration.Value, false) && IsSupportLoggedIn)
            {
                UseProjectTimeBlocks.Visible = true;
                UseProjectTimeBlocks.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, 0, SoeCompany.ActorCompanyId, 0, false) ? "True" : "False";
                UseProjectTimeBlocks.ReadOnly = StringUtility.GetBool(UseProjectTimeBlocks.Value, true);
            }
            else
            {
                UseProjectTimeBlocks.Visible = false;
            }

            var timeSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Payroll, SoeCompany.ActorCompanyId);
            UsingPayroll.Value = sm.GetBoolSettingFromDict(timeSettingsDict, (int)CompanySettingType.UsePayroll) ? "1" : "0";
            if (UsingPayroll.Value == "1")
            {
                ExtendedTimeRegistration.ReadOnly = StringUtility.GetBool(ExtendedTimeRegistration.Value, false);
                CreateTransactionsBaseOnTimeRules.ReadOnly = StringUtility.GetBool(CreateTransactionsBaseOnTimeRules.Value, false);
            }

            GetPurchasePriceFromInvoiceProduct.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.GetPurchasePriceFromInvoiceProduct, (int)SettingDataType.Boolean, Boolean.TrueString);
            UseDateIntervalInIncomeNotInvoiced.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.UseDateIntervalInIncomeNotInvoiced, (int)SettingDataType.Boolean);

            BlockTimeBlockWithZeroStartTime.Value = sm.GetSettingFromDict(projectSettingsDict, (int)CompanySettingType.ProjectBlockTimeBlockWithZeroStartTime, (int)SettingDataType.Boolean);

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

            boolValues.Add((int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, StringUtility.GetBool(F["AutoCreateProjectOnNewInvoice"]));
            boolValues.Add((int)CompanySettingType.ProjectIncludeTimeProjectReport, StringUtility.GetBool(F["IncludeTimeProjectReport"]));
            boolValues.Add((int)CompanySettingType.ProjectCreateInvoiceRowFromTransaction, StringUtility.GetBool(F["MoveTransactionToInvoiceRow"]));
            boolValues.Add((int)CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber, StringUtility.GetBool(F["UseOrderNumberAsProjectNumber"]));
            boolValues.Add((int)CompanySettingType.ProjectLimitOrderToProjectUsers, StringUtility.GetBool(F["LimitOrderToProjectUsers"]));
            //boolValues.Add((int)CompanySettingType.ProjectKeepEmployeesOnWeekChange, StringUtility.GetBool(F["KeepEmployeesOnWeekChange"]));
            boolValues.Add((int)CompanySettingType.ProjectChargeCostsToProject, StringUtility.GetBool(F["ChargeCostsToProject"]));
            boolValues.Add((int)CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, StringUtility.GetBool(F["IncludeOnlyInvoicedTimeInTimeProjectReport"]));
            boolValues.Add((int)CompanySettingType.ProjectUseCustomerNameAsProjectName, StringUtility.GetBool(F["UseCustomerNameAsProjectName"]));
            boolValues.Add((int)CompanySettingType.ProjectOverheadCostAsFixedAmount, StringUtility.GetBool(F["OverheadCostAsFixedAmount"]));
            boolValues.Add((int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, StringUtility.GetBool(F["OverheadCostAsAmountPerHour"]));
            boolValues.Add((int)CompanySettingType.ProjectAutosaveOnWeekChangeInOrder, StringUtility.GetBool(F["AutosaveOnWeekChangeInOrder"]));
            boolValues.Add((int)CompanySettingType.ProjectInvoiceTimeAsWorkTime, StringUtility.GetBool(F["InvoiceTimeAsWorkTime"]));
            boolValues.Add((int)CompanySettingType.ProjectAutoUpdateAccountSettings, StringUtility.GetBool(F["ProjectAutoUpdateAccountSettings"]));
            boolValues.Add((int)CompanySettingType.GetPurchasePriceFromInvoiceProduct, StringUtility.GetBool(F["GetPurchasePriceFromInvoiceProduct"]));
            boolValues.Add((int)CompanySettingType.UseDateIntervalInIncomeNotInvoiced, StringUtility.GetBool(F["UseDateIntervalInIncomeNotInvoiced"]));
            boolValues.Add((int)CompanySettingType.ProjectBlockTimeBlockWithZeroStartTime, StringUtility.GetBool(F["BlockTimeBlockWithZeroStartTime"])); 
            boolValues.Add((int)CompanySettingType.ProjectAutoUpdateInternalAccounts, StringUtility.GetBool(F["ProjectAutoUpdateInternalAccounts"]));



            if (F["UsingPayroll"] == "1")
            {
                if ( F.AllKeys.Contains("ExtendedTimeRegistration") )
                    boolValues.Add((int)CompanySettingType.ProjectUseExtendedTimeRegistration, StringUtility.GetBool(F["ExtendedTimeRegistration"]));
                if (F.AllKeys.Contains("CreateTransactionsBaseOnTimeRules"))
                    boolValues.Add((int)CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules, StringUtility.GetBool(F["CreateTransactionsBaseOnTimeRules"]));
            }
            else
            {
                boolValues.Add((int)CompanySettingType.ProjectUseExtendedTimeRegistration, StringUtility.GetBool(F["ExtendedTimeRegistration"]));
                boolValues.Add((int)CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules, StringUtility.GetBool(F["CreateTransactionsBaseOnTimeRules"]));
            }
            if (F.AllKeys.Contains("UseProjectTimeBlocks") && StringUtility.GetBool(F["UseProjectTimeBlocks"]))
            {
                boolValues.Add((int)CompanySettingType.UseProjectTimeBlocks, true);
            }
              

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int
            var intValues = new Dictionary<int, int>();

            int defaultTimeCodeId = 0;
            Int32.TryParse(F["TimeCodeSelectEntry"], out defaultTimeCodeId);
            intValues.Add((int)CompanySettingType.ProjectDefaultTimeCodeId, defaultTimeCodeId);

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }
    }
}
