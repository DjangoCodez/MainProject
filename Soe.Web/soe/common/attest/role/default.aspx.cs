using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoftOne.Soe.Web.soe.common.attest.role
{
    public partial class _default : PageBase
    {
        #region Variables

        private AttestManager am;
        protected int actorCompanyId;
        protected AttestRole attestRole;

        //Module specifics
        protected bool EnableEconomy { get; set; }
        protected bool EnableBilling { get; set; }
        protected bool EnableTime { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/soe/common/attest/transition/texts.js.aspx");
            Scripts.Add("/UserControls/AttestTransition.js");
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Manage_Attest_Customer_AttestRoles_Edit:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Manage_Attest_Customer_AttestRoles_Edit;
                        break;
                    case Feature.Manage_Attest_Supplier_AttestRoles_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Manage_Attest_Supplier_AttestRoles_Edit;
                        break;
                    case Feature.Manage_Attest_Time_AttestRoles_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Manage_Attest_Time_AttestRoles_Edit;
                        break;
                    case Feature.Manage_Attest_CaseProject_AttestRoles_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Manage;
                        FeatureEdit = Feature.Manage_Attest_CaseProject_AttestRoles_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AttestManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery, true);

            bool copy = false;

            //Optional parameters
            int attestRoleId;

            if (Mode == SoeFormMode.RegisterFromCopy)
            {
                copy = true;
                Int32.TryParse(QS["copyId"], out attestRoleId);
            }
            else
            {
                if (Int32.TryParse(QS["role"], out attestRoleId))
                {
                    if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                    {
                        attestRole = am.GetPrevNextAttestRole(attestRoleId, SoeCompany.ActorCompanyId, Mode);
                        ClearSoeFormObject();
                        if (attestRole != null)
                            Response.Redirect(Request.Url.AbsolutePath + "?role=" + attestRole.AttestRoleId);
                        else
                            Response.Redirect(Request.Url.AbsolutePath + "?role=" + attestRoleId);
                    }
                    else
                    {
                        attestRole = am.GetAttestRole(attestRoleId, SoeCompany.ActorCompanyId, true, true, true);
                        if (attestRole == null)
                        {
                            Form1.MessageWarning = GetText(5227, "Attestroll hittades inte");
                            Mode = SoeFormMode.Register;
                        }
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(5218, "Redigera attestroll");
            string registerModeTabHeaderText = GetText(5222, "Registrera attestroll");
            PostOptionalParameterCheck(Form1, attestRole, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = attestRole != null ? attestRole.Name : "";

            #endregion

            #region UserControls

            CompanyCategoriesPrimary.InitControl(Form1, "CatPrimary");
            CompanyCategoriesSecondary.InitControl(Form1, "CatSecondary");
            AttestTransitions.InitControl(Form1);
            if (UseAccountHierarchy())
            {
                ShowUncategorized.TermID = 9313;
                ShowUncategorized.DefaultTerm = "Visa anställda utan tillhörighet";

                ShowAllCategories.TermID = 9312;
                ShowAllCategories.DefaultTerm = "Visa alla konton";
            }
            else
            {
                ShowUncategorized.TermID = 5303;
                ShowUncategorized.DefaultTerm = "Visa anställda utan kategori";

                ShowAllCategories.TermID = 5609;
                ShowAllCategories.DefaultTerm = "Visa alla kategorier";
            }

            #endregion

            #region Actions

            if (Form1.IsPosted && !copy)
            {
                Save();
            }

            #endregion

            #region Populate

            int recordId = attestRole?.AttestRoleId ?? (copy ? attestRoleId : 0);

            CompanyCategoriesPrimary.Populate(!copy && Repopulate, SoeCompany.ActorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, recordId, true, GetText(7071, "Primära kategorier"), "CatPrimary");
            CompanyCategoriesSecondary.Populate(!copy && Repopulate, SoeCompany.ActorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, recordId, true, GetText(7072, "Sekundära kategorier"), "CatSecondary");
            AttestTransitions.PopulateAttestRoleTransitions(!copy && Repopulate, SoeCompany.ActorCompanyId, recordId, TargetSoeModule);

            #region Reminder settings

            bool addPeriods = true;

            if (TargetSoeModule == SoeModule.Economy)
            {
                Dictionary<int, string> attestStates = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, TargetSoeModule, true, false);
                AttestStateSelectEntry.ConnectDataSource(attestStates, "Value", "Key");
            }
            else if (TargetSoeModule == SoeModule.Time)
            {
                Dictionary<int, string> attestStates = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, TargetSoeModule, true, false);
                attestStates.AddRange(am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.InvoiceTime, TargetSoeModule, false, false));
                AttestStateSelectEntry.ConnectDataSource(attestStates, "Value", "Key");
            }
            else
            {
                DivVisibleAttestStates.Visible = true;
                if (TargetSoeModule == SoeModule.Billing)
                {
                    var visibleAttestStates = am.GetVisibleAttestStates(recordId);
                    Dictionary<int, string> attestStates = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.Order, TargetSoeModule, true, false);
                    VisibleAttestStates.DataSourceFrom = attestStates;
                    VisibleAttestStates.PreviousForm = PreviousForm;
                    int pos = 0;
                    foreach (var item in visibleAttestStates)
                    {
                        VisibleAttestStates.AddValueFrom(pos, item.AttestStateId.ToString());

                        pos++;
                        if (pos == VisibleAttestStates.NoOfIntervals)
                            break;
                    }
                }

                addPeriods = false;
                DivAttestReminders.Visible = false;
            }

            if (addPeriods)
            {
                Dictionary<int, string> periods = new Dictionary<int, string>();
                periods.Add((int)AttestPeriodType.Unknown, " ");
                periods.Add((int)AttestPeriodType.Day, GetText(7162, "dagen"));
                periods.Add((int)AttestPeriodType.Week, GetText(7163, "veckan"));
                periods.Add((int)AttestPeriodType.Month, GetText(7164, "månaden"));
                periods.Add((int)AttestPeriodType.Period, GetText(7165, "perioden"));
                PeriodSelectEntry.ConnectDataSource(periods, "Value", "Key");
            }

            #endregion

            #endregion

            #region Set data

            if (attestRole != null)
            {
                Name.Value = attestRole.Name;
                Description.Value = attestRole.Description;
                DefaultMaxAmount.Value = Decimal.Round(attestRole.DefaultMaxAmount, 0).ToString();
                ShowUncategorized.Value = attestRole.ShowUncategorized ? Boolean.TrueString : Boolean.FalseString;
                ShowAllCategories.Value = attestRole.ShowAllCategories ? Boolean.TrueString : Boolean.FalseString;
                ShowAllSecondaryCategories.Value = attestRole.ShowAllSecondaryCategories ? Boolean.TrueString : Boolean.FalseString;
                ShowTemplateSchedule.Value = attestRole.ShowTemplateSchedule ? Boolean.TrueString : Boolean.FalseString;
                AlsoAttestAdditionsFromTime.Value = attestRole.AlsoAttestAdditionsFromTime ? Boolean.TrueString : Boolean.FalseString;
                AttestByEmployeeAccount.Value = attestRole.AttestByEmployeeAccount ? Boolean.TrueString : Boolean.FalseString;
                StaffingByEmployeeAccount.Value = attestRole.StaffingByEmployeeAccount ? Boolean.TrueString : Boolean.FalseString;
                IsExecutive.Value = attestRole.IsExecutive.ToString();
                HumanResourcesPrivacy.Value = attestRole.HumanResourcesPrivacy ? Boolean.TrueString : Boolean.FalseString;
                AttestStateSelectEntry.Value = attestRole.ReminderAttestStateId?.ToString() ?? "0";
                NoOfDaysTextEntry.Value = attestRole.ReminderNoOfDays?.ToString() ?? "";
                PeriodSelectEntry.Value = attestRole.ReminderPeriodType?.ToString() ?? "0";
                ExternalCodes.Value = attestRole.ExternalCodesString;
            }

            if (EnableBilling || EnableEconomy)
            {
                TabSettings.Visible = false;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(5230, "Attestroll sparad");
                else if (MessageFromSelf == "SAVED_WITH_CATEGORYERRORS")
                    Form1.MessageWarning = GetText(5230, "Attestroll sparad") + ". " + GetText(5228, "Alla kategorier kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_TRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5230, "Attestroll sparad") + ". " + GetText(3347, "Alla övergångar kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_CATEGORYANDTRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5230, "Attestroll sparad") + ". " + GetText(3348, "Alla kategorier och övergångar kunde inte sparas");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(5231, "Attestroll kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(5232, "Attestroll uppdaterad");
                else if (MessageFromSelf == "UPDATED_WITH_CATEGORYERRORS")
                    Form1.MessageWarning = GetText(5232, "Attestroll uppdaterad") + ". " + GetText(5229, "Alla kategorier kunde inte uppdateras");
                else if (MessageFromSelf == "UPDATED_WITH_TRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5232, "Attestroll uppdaterad") + ". " + GetText(3345, "Alla övergångar kunde inte uppdateras");
                else if (MessageFromSelf == "UPDATED_WITH_CATEGORYANDTRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5232, "Attestroll uppdaterad") + ". " + GetText(3346, "Alla kategorier och övergångar kunde inte uppdateras");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(5233, "Attestroll kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(5234, "Attestroll finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(5235, "Attestroll kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(5236, "Attestroll borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(5237, "Attestroll kunde inte tas bort, kontrollera att det inte används");
            }

            #endregion

            #region Navigation

            if (attestRole != null)
            {
                Form1.SetRegLink(GetText(5222, "Registrera attestroll"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string name = F["Name"];
            string externalCodes = F["ExternalCodes"];
            string description = F["Description"];
            decimal.TryParse(F["DefaultMaxAmount"], out decimal defaultMaxAmount);
            bool showUncategorized = StringUtility.GetBool(F["ShowUncategorized"]);
            bool showAllCategories = StringUtility.GetBool(F["ShowAllCategories"]);
            bool showAllSecondaryCategories = StringUtility.GetBool(F["ShowAllSecondaryCategories"]);
            bool showTemplateSchedule = StringUtility.GetBool(F["ShowTemplateSchedule"]);
            bool alsoAttestAdditionsFromTime = StringUtility.GetBool(F["AlsoAttestAdditionsFromTime"]);
            bool attestByEmployeeAccount = StringUtility.GetBool(F["AttestByEmployeeAccount"]);
            bool staffingByEmployeeAccount = StringUtility.GetBool(F["StaffingByEmployeeAccount"]);
            bool isExecutive = StringUtility.GetBool(F["IsExecutive"]);
            bool humanResourcesPrivacy = StringUtility.GetBool(F["HumanResourcesPrivacy"]);
            Int32.TryParse(F["AttestStateSelectEntry"], out int reminderAttestStateId);
            Int32.TryParse(F["NoOfDaysTextEntry"], out int reminderNoOfDays);
            Int32.TryParse(F["PeriodSelectEntry"], out int reminderPeriodType);

            if (attestRole == null)
            {
                // Validation: AttestRole not already exist
                if (am.ExistsAttestRole(name, SoeCompany.ActorCompanyId, TargetSoeModule))
                    RedirectToSelf("EXIST", true);

                // Create AttestRole
                attestRole = new AttestRole()
                {
                    ActorCompanyId = SoeCompany.ActorCompanyId,
                    Module = (int)TargetSoeModule,
                    Name = name,
                    Description = description,
                    DefaultMaxAmount = defaultMaxAmount,
                    ShowUncategorized = showUncategorized,
                    ShowAllCategories = showAllCategories,
                    ShowAllSecondaryCategories = showAllSecondaryCategories,
                    ShowTemplateSchedule = showTemplateSchedule,
                    AlsoAttestAdditionsFromTime = alsoAttestAdditionsFromTime,
                    AttestByEmployeeAccount = attestByEmployeeAccount,
                    StaffingByEmployeeAccount = staffingByEmployeeAccount,
                    IsExecutive = isExecutive,
                    HumanResourcesPrivacy = humanResourcesPrivacy,
                    ExternalCodesString = externalCodes
                };

                if (reminderAttestStateId != 0 && reminderPeriodType != 0)
                {
                    attestRole.ReminderAttestStateId = reminderAttestStateId;
                    attestRole.ReminderNoOfDays = reminderNoOfDays;
                    attestRole.ReminderPeriodType = reminderPeriodType;
                }

                if (am.AddAttestRole(attestRole).Success)
                {
                    bool primaryCategorySuccess = SavePrimaryCategories();
                    bool secondaryCategorySuccess = SaveSecondaryCategories();
                    bool transitionSuccess = SaveTransitions();

                    if ((!primaryCategorySuccess || !secondaryCategorySuccess) && !transitionSuccess)
                        RedirectToSelf("SAVED_WITH_CATEGORYANDTRANSITIONERRORS");
                    if (!primaryCategorySuccess || !secondaryCategorySuccess)
                        RedirectToSelf("SAVED_WITH_CATEGORYERRORS");
                    if (!transitionSuccess)
                        RedirectToSelf("SAVED_WITH_TRANSITIONERRORS");
                    RedirectToSelf("SAVED");
                }
                else
                {
                    RedirectToSelf("NOTSAVED", true);
                }
            }
            else
            {
                if (attestRole.Name != name && am.ExistsAttestRole(name, SoeCompany.ActorCompanyId, TargetSoeModule))
                    RedirectToSelf("EXIST", true);

                // Update Condition
                attestRole.Name = name;
                attestRole.Description = description;
                attestRole.DefaultMaxAmount = defaultMaxAmount;
                attestRole.ShowUncategorized = showUncategorized;
                attestRole.ShowAllCategories = showAllCategories;
                attestRole.ShowAllSecondaryCategories = showAllSecondaryCategories;
                attestRole.ShowTemplateSchedule = showTemplateSchedule;
                attestRole.AlsoAttestAdditionsFromTime = alsoAttestAdditionsFromTime;
                attestRole.AttestByEmployeeAccount = attestByEmployeeAccount;
                attestRole.StaffingByEmployeeAccount = staffingByEmployeeAccount;
                attestRole.IsExecutive = isExecutive;
                attestRole.HumanResourcesPrivacy = humanResourcesPrivacy;
                attestRole.ExternalCodesString = externalCodes;

                if (reminderAttestStateId != 0 && reminderPeriodType != 0)
                {
                    attestRole.ReminderAttestStateId = reminderAttestStateId;
                    attestRole.ReminderNoOfDays = reminderNoOfDays;
                    attestRole.ReminderPeriodType = reminderPeriodType;
                }

                if (TargetSoeModule == Common.Util.SoeModule.Billing)
                {
                    Collection<FormIntervalEntryItem> visibleAttestStatesItems = VisibleAttestStates.GetData(F);
                    am.SaveVisibleAttestStatesForOrder(SoeCompany.ActorCompanyId, attestRole.AttestRoleId, visibleAttestStatesItems);
                }

                if (am.UpdateAttestRole(attestRole, SoeCompany.ActorCompanyId).Success)
                {
                    bool categorySuccess = SavePrimaryCategories();
                    bool secondaryCategorySuccess = SaveSecondaryCategories();
                    bool transitionSuccess = SaveTransitions();

                    if ((!categorySuccess || !secondaryCategorySuccess) && !transitionSuccess)
                        RedirectToSelf("UPDATED_WITH_CATEGORYANDTRANSITIONERRORS");
                    if (!categorySuccess || !secondaryCategorySuccess)
                        RedirectToSelf("UPDATED_WITH_CATEGORYERRORS");
                    if (!transitionSuccess)
                        RedirectToSelf("UPDATED_WITH_TRANSITIONERRORS");
                    RedirectToSelf("UPDATED");
                }
                else
                {
                    RedirectToSelf("NOTUPDATED", true);
                }
            }
        }

        private bool SavePrimaryCategories()
        {
            return CompanyCategoriesPrimary.Save(F, SoeCompany.ActorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, attestRole.AttestRoleId, "CatPrimary");
        }

        private bool SaveSecondaryCategories()
        {
            return CompanyCategoriesSecondary.Save(F, SoeCompany.ActorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, attestRole.AttestRoleId, "CatSecondary");
        }

        private bool SaveTransitions()
        {
            return AttestTransitions.SaveAttestRoleTransitions(F, SoeCompany.ActorCompanyId, attestRole.AttestRoleId);
        }

        protected override void Delete()
        {
            if (am.DeleteAttestRole(attestRole, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
