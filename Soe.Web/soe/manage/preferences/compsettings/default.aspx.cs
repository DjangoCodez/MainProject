using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System.Linq;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Business.Util.API.Fortnox;
using SoftOne.Soe.Business.Util.API.VismaEAccounting;

namespace SoftOne.Soe.Web.soe.manage.preferences.compsettings
{
    public partial class Default : PageBase
    {
        #region Variables

        private const string GeneralEmailAddress = "noreply@softone.se";

        private AccountManager acm;
        private AttestManager am;
        private CompanyManager cm;
        private RoleManager rm;
        private SettingManager sm;
        private ContactManager ccm;
        private Company company;

        private bool releaseModeApi = false;
        private bool isSupportAdmin = false;
        private Dictionary<int, object> manageSettingsDict;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences_CompSettings;
            base.Page_Init(sender, e);

            //Add scripts for hiding supportlogin date & time 
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            acm = new AccountManager(ParameterObject);
            am = new AttestManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);
            rm = new RoleManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            ccm = new ContactManager(ParameterObject);

            //Load before Save so Save can check saved values
            this.company = cm.GetCompany(SoeCompany.ActorCompanyId);
            this.manageSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Manage, SoeCompany.ActorCompanyId);
            this.isSupportAdmin = base.IsSupportAdmin;

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            DefaultRole.ConnectDataSource(rm.GetRolesByCompanyDict(SoeCompany.ActorCompanyId, true, false));

            Dictionary<int, string> accountDims = acm.GetAccountDimsByCompanyDict(SoeCompany.ActorCompanyId, true, onlyInternal: true);
            DefaultEmployeeAccountDimEmployee.ConnectDataSource(accountDims);
            DefaultEmployeeAccountDimSelector.ConnectDataSource(accountDims);

            PasswordLengthInstruction.LabelSetting = String.Format(GetText(3789, "Intervallet måste vara mellan {0} och {1} tecken"), Constants.PASSWORD_DEFAULT_MIN_LENGTH, Constants.PASSWORD_DEFAULT_MAX_LENGTH);

            AddModuleIconItems();

            Dictionary<int, string> attestStates = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.CaseProject, SoeModule.Manage, true, false);
            CaseProjectAttestStateReceived.ConnectDataSource(attestStates);

            releaseModeApi = true;
            bool settingAPITestMode = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, UserId, SoeCompany.ActorCompanyId, 0);
            if (settingAPITestMode)
                releaseModeApi = false;

            #endregion

            #region Set data

            //General
            DefaultRole.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.DefaultRole, (int)SettingDataType.Integer);            
            CleanReportPrintoutAfterNrOfDays.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CleanReportPrintoutAfterNrOfDays, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            //Login
            DoNotAddToSoftOneIdDirectlyOnSave.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.DoNotAddToSoftOneIdDirectlyOnSave, (int)SettingDataType.Boolean);
            BlockFromDateOnUserAfterNrOfDays.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.BlockFromDateOnUserAfterNrOfDays, (int)SettingDataType.Integer);
            MandatoryContactInformation.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.UseMissingMandatoryInformation, (int)SettingDataType.Boolean);

            //Only customer can change, not support
            AllowSupportLogin.ReadOnly = this.isSupportAdmin;
            SupportLoginTo.ReadOnly = this.isSupportAdmin;
            SupportLoginTimeTo.ReadOnly = this.isSupportAdmin;

            if (!this.isSupportAdmin)
                AllowSupportLogin.OnClick = "syncAllowSupportLoginChanged()";

            if (company != null)
            {
                if (company.AllowSupportLogin.HasValue)
                    AllowSupportLogin.Value = company.AllowSupportLogin.Value.ToString();

                // DateTime field won't show hours without crashing. Placed into separate field.
                if (company.AllowSupportLoginTo.HasValue)
                {
                    if (base.IsLanguageSwedish())
                    {
                        SupportLoginTo.Value = String.Format("{0:yyyy-MM-dd}", company.AllowSupportLoginTo.Value);
                        SupportLoginTimeTo.Value = String.Format("{0:HH:mm}", company.AllowSupportLoginTo.Value);
                    }
                    else if (base.IsLanguageEnglish())
                    {
                        SupportLoginTo.Value = String.Format("{0:MM/dd/yyyy}", company.AllowSupportLoginTo.Value);
                        SupportLoginTimeTo.Value = String.Format("{0:HH:mm}", company.AllowSupportLoginTo.Value);
                    }
                    else if (base.IsLanguageFinnish() || base.IsLangugeNorwegian())
                    {
                        SupportLoginTo.Value = String.Format("{0:dd.MM.yyyy}", company.AllowSupportLoginTo.Value);
                        //For some reason Finnish time is represented with "." as time delimiter. Change it to ":" to avoid error when saving.
                        SupportLoginTimeTo.Value = String.Format("{0:HH:mm}", company.AllowSupportLoginTo.Value).Replace(".", ":");
                    }
                }
            }

            // Password
            int minLength = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMinLength, UserId, SoeCompany.ActorCompanyId, 0);
            int maxLength = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMaxLength, UserId, SoeCompany.ActorCompanyId, 0);
            PasswordMinLength.Value = minLength == 0 ? Constants.PASSWORD_DEFAULT_MIN_LENGTH.ToString() : minLength.ToString();
            PasswordMaxLength.Value = maxLength == 0 ? Constants.PASSWORD_DEFAULT_MAX_LENGTH.ToString() : maxLength.ToString();

            //Accounts
            UseAccountsHierarchy.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.UseAccountHierarchy, (int)SettingDataType.Boolean);
            DefaultEmployeeAccountDimEmployee.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, (int)SettingDataType.Integer);
            UseLimitedEmployeeAccountDimLevels.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.UseLimitedEmployeeAccountDimLevels, (int)SettingDataType.Boolean);
            UseExtendedEmployeeAccountDimLevels.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.UseExtendedEmployeeAccountDimLevels, (int)SettingDataType.Boolean);
            DefaultEmployeeAccountDimSelector.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.DefaultEmployeeAccountDimSelector, (int)SettingDataType.Integer);
            UseAccountHierarchyInstruction.DefaultIdentifier = " ";
            UseAccountHierarchyInstruction.DisableFieldset = true;
            FallbackOnEmployeeAccountInPrio.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.FallbackOnEmployeeAccountInPrio, (int)SettingDataType.Boolean);
            BaseSelectableAccountsOnEmployeeInsteadOfAttestRole.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.BaseSelectableAccountsOnEmployeeInsteadOfAttestRole, (int)SettingDataType.Boolean);
            SendReminderToExecutivesBasedOnEmployeeAccountOnly.Value = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.SendReminderToExecutivesBasedOnEmployeeAccountOnly, (int)SettingDataType.Boolean);

            DefaultEmployeeAccountDimSelector.ReadOnly = !this.isSupportAdmin;
            UseAccountsHierarchy.ReadOnly = !this.isSupportAdmin;
            DefaultEmployeeAccountDimEmployee.ReadOnly = !this.isSupportAdmin;
            UseLimitedEmployeeAccountDimLevels.ReadOnly = !this.isSupportAdmin;
            UseExtendedEmployeeAccountDimLevels.ReadOnly = !this.isSupportAdmin;

            if (UseAccountsHierarchy.Value == Boolean.TrueString)
            {
                UseAccountsHierarchy.ReadOnly = true;
            }                
            else
            {
                DefaultEmployeeAccountDimEmployee.ReadOnly = true;
                UseLimitedEmployeeAccountDimLevels.ReadOnly = true;
                UseExtendedEmployeeAccountDimLevels.ReadOnly = true;
                UseAccountHierarchyInstruction.Instructions = new List<string>() 
                {
                    GetText(5627,"Om du väljer att använda ekonomisk tillhörighet går det inte att ångra."),
                    GetText(5628,"Aktivera enbart i samråd med konsult på SoftOne."),
                };
            }

            //EntityHistory
            EntityHistoryInstruction.DefaultIdentifier = " ";
            EntityHistoryInstruction.DisableFieldset = true;
            EntityHistoryInstruction.Instructions = new List<string>()
            {
                GetText(5645, "För vissa poster, t.ex offert, order och faktura, så visas information om historiska händelser om posten när den öppnas."),
                GetText(5646, "Systemet kollar då ett antal minuter bakåt i tiden om posten har öppnats eller sparats av någon användare."),
            };
            int? entityHistoryInterval = sm.GetNullableIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreEntityHistoryInterval, UserId, SoeCompany.ActorCompanyId, 0);
            if (entityHistoryInterval.HasValue)
                EntityHistoryInterval.Value = entityHistoryInterval.Value.ToString();
            else
                EntityHistoryInterval.InfoText = string.Format(GetText(5655, "Default är {0} min"), Constants.ENTITY_HISTORY_INTERVAL_DEFAULT_MIN.ToString());

            // Default email
            EmailInstructions.DefaultIdentifier = " ";
            EmailInstructions.DisableFieldset = true;
            EmailInstructions.Instructions = new List<string>()
            {
                GetText(7557, "Här fyller du i avsändande epostadress för utgående epost från systemet. T.ex kundfakturor."),
                string.Format(GetText(7558, "Som standard används {0}."), GeneralEmailAddress),
                string.Format(GetText(7559, "Tänk på att adresser från domäner som @telia.com, @hotmail.com, @live.com ofta stoppas i säkerhetsfilter vilket inte gör dessa till bra avsändare av kundfakturor.")),
                string.Format(GetText(7726, "Ett antal domäner som @gmail.com och @yahooo.com får ej användas på grund av att dessa förbjuder utskick från tredjepartstjänster.")),
            };

            string defEmail = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, UserId, SoeCompany.ActorCompanyId, 0);
            DefaultEmailAddress.Value = !string.IsNullOrEmpty(defEmail) ? defEmail: GeneralEmailAddress;

            bool useDefaultEmail = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseDefaultEmailAddress, UserId, SoeCompany.ActorCompanyId, 0);
            UseDefaultEmailAddress.Value = useDefaultEmail.ToString();

            DisableMessageOnInboundEmailError.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.DisableMessageOnInboundEmailError, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            // External links
            ActivateExternalLinks.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseExternalLinks, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            ExternalLink1.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ExternalLink1, UserId, SoeCompany.ActorCompanyId, 0);
            ExternalLink2.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ExternalLink2, UserId, SoeCompany.ActorCompanyId, 0);
            ExternalLink3.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ExternalLink3, UserId, SoeCompany.ActorCompanyId, 0);

            // Dashboard
            int interval = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DashboardRefreshInterval, UserId, SoeCompany.ActorCompanyId, 0);
            if (interval == 0)
                interval = 5;
            DashboardRefreshInterval.Value = interval.ToString();

            // Module icon and header
            ModuleIconImage.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeModuleIcon, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            PersonellModuleHeader.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.TimeModuleHeader, UserId, SoeCompany.ActorCompanyId, 0);

            // Case project
            CaseProjectAttestStateReceived.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CaseProjectAttestStateReceived, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            //Inexchange ftp-settings
            InExchangeAddress.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpAddress, UserId, SoeCompany.ActorCompanyId, 0);
            InExchangeUser.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpUsername, UserId, SoeCompany.ActorCompanyId, 0);
            InExchangePasswd.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpPassword, UserId, SoeCompany.ActorCompanyId, 0);
            InExchangeAddressTest.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpAddressTest, UserId, SoeCompany.ActorCompanyId, 0);
            InExchangeUserTest.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpUsernameTest, UserId, SoeCompany.ActorCompanyId, 0);
            InExchangePasswdTest.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeFtpPasswordTest, UserId, SoeCompany.ActorCompanyId, 0);

            //InExchange API-settings
            var API_SendRegistered = GetInexchangeAPISendRegisteredSetting();
            var API_ReciveRegistered = GetInexchangeAPIRecivedRegisteredSetting();
            var API_Activated = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeAPIActivated, UserId, SoeCompany.ActorCompanyId, 0);

            RegisterAPI.Enabled = (!API_SendRegistered) && (!API_ReciveRegistered); // (!API_SendRegistered) || (!API_ReciveRegistered);
            ActivateAPI.Enabled = !API_Activated && (API_SendRegistered || API_ReciveRegistered);
            InExchangeAPISendRegistered.Value = API_SendRegistered.ToString();
            InExchangeAPISendRegistered.ReadOnly = false; //API_SendRegistered;
            InExchangeAPIReciveRegistered.Value = API_ReciveRegistered.ToString();
            InExchangeAPIReciveRegistered.ReadOnly = false; //API_ReciveRegistered;
            InexchangedRegisterDate.Value = GetInexchangeAPIRegisteredDateSetting();

            // Attest Autoreminder
            bool supplierInvoiceAutoReminder = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAutoReminder, UserId, SoeCompany.ActorCompanyId, 0);
            SupplierInvoiceAutoReminder.Value = supplierInvoiceAutoReminder.ToString();

            IntrumSettings.Visible = HasRolePermission(Feature.Manage_Preferences_CompSettings_Intrum, Permission.Modify);
            IntrumClientNo.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.IntrumClientNo, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            IntrumHubNo.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumHubNo, UserId, SoeCompany.ActorCompanyId, 0);
            IntrumLedgerNo.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.IntrumLedgerNo, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            IntrumTestMode.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.IntrumTestMode, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            IntrumUser.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumUser, UserId, SoeCompany.ActorCompanyId, 0);
            IntrumPwd.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumPwd, UserId, SoeCompany.ActorCompanyId, 0);

            ZetesSettings.Visible = HasRolePermission(Feature.Manage_Preferences_CompSettings_Zetes, Permission.Modify);
            ZetesClientCode.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ZetesClientCode, UserId, SoeCompany.ActorCompanyId, 0);
            ZetesStakeholderCode.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ZetesStakeholderCode, UserId, SoeCompany.ActorCompanyId, 0);
            ZetesUser.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ZetesUser, UserId, SoeCompany.ActorCompanyId, 0);
            ZetesPwd.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.ZetesPwd, UserId, SoeCompany.ActorCompanyId, 0);
            ZetesTestMode.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ZetesTestMode, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            // Kivra
            KivraTenentKey.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.KivraTenentKey, UserId, SoeCompany.ActorCompanyId, 0);

            //Finvoice
            Finvoice.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);
            FinvoiceAddress.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);
            FinvoiceOperator.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);
            FinvoiceUseBankIntegration.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);
            FinvoiceAddress.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceAddress, UserId, SoeCompany.ActorCompanyId, 0);
            FinvoiceOperator.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceOperator, UserId, SoeCompany.ActorCompanyId, 0);
            FinvoiceUseBankIntegration.Value = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.FinvoiceUseBankIntegration, UserId, SoeCompany.ActorCompanyId,0).ToString();

            ChainAffiliation.ConnectDataSource(GetGrpText(TermGroup.ChainAffiliation, addEmptyRow: true));
            ChainAffiliation.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ChainAffiliation, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            ChainAffiliation.Visible = isSupportAdmin;
            ChainSettings.Visible = isSupportAdmin;

            //Fortnox
            CheckIfUpdatingFortnoxToken();
            CheckIfHasFortnoxToken();

            //Visma E-accounting
            CheckIfUpdatingEAccountingToken();
            CheckIfHasEAccountingToken();

            //AzoraOne
            CheckIfHasAzoraOneActivated();

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "UPDATED")
                Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            else if (MessageFromSelf == "INVALID_PASSWORD_SETTINGS")
                Form1.MessageError = String.Format(GetText(3790, "Lösenordsintervallet måste vara mellan {0} och {1} tecken"), Constants.PASSWORD_DEFAULT_MIN_LENGTH, Constants.PASSWORD_DEFAULT_MAX_LENGTH);
            else if (MessageFromSelf == "NO_EMAILADDRESS_DEFINED")
                Form1.MessageError = GetText(7069, "Fältet för emailadress får inte vara tomt");
            else if (MessageFromSelf == "API_REGISTERED")
                Form1.MessageSuccess = GetText(4673, "API registration klar");
            else if (MessageFromSelf == "API_FAIL_REGISTER")
                Form1.MessageError = GetText(4674, "API registration misslyckades");
            else if (MessageFromSelf == "API_FAIL_REGISTER_NO_SELECTION")
                Form1.MessageError = GetText(4674, "API registration misslyckades") + "\n" + GetText(7434, "Ingen InExchange-tjänst har valts");
            else if (MessageFromSelf == "API_FAIL_REGISTER_NO_ADDRESS")
                Form1.MessageError = GetText(4674, "API registration misslyckades") + "\n" + GetText(7435, "Fullständig postadress saknas för företaget");
            else if (MessageFromSelf == "API_FAIL_REGISTER_SETTING")
                Form1.MessageError = GetText(4675, "API registration klar men inställning kunde inte uppdateras");
            else if (MessageFromSelf == "API_ACTIVATED")
                Form1.MessageSuccess = GetText(4676, "API aktivering klar");
            else if (MessageFromSelf == "API_FAIL_ACTIVATE")
                Form1.MessageError = GetText(4677, "API aktivering misslyckades");
            else if (MessageFromSelf == "API_FAIL_ACTIVATE_SETTING")
                Form1.MessageError = GetText(4678, "API aktivering klar men inställning kunde inte uppdateras");
            else if (MessageFromSelf == "FINVOICE_ADDRESS_ALREADY_IN_USE")
                Form1.MessageError = GetText(7671, "Finvoice addressen är redan använda i ett annat företag tillsammans med bankintegration") + "\n" + GetText(7672, "En specifik Finvoice address can endast användas för bankintegration av ett företag i systemet");
            else if (!string.IsNullOrEmpty(MessageFromSelf))
                Form1.MessageError = MessageFromSelf;

            #endregion
        }

        private bool GetInexchangeAPISendRegisteredSetting()
        {
            return sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeAPISendRegistered, UserId, SoeCompany.ActorCompanyId, 0);
        }
        private bool GetInexchangeAPIRecivedRegisteredSetting()
        {
            return sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeAPIReciveRegistered, UserId, SoeCompany.ActorCompanyId, 0);
        }

        private string GetInexchangeAPIRegisteredDateSetting()
        {
            var settingDate = sm.GetDateSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeAPIRegisteredDate, UserId, SoeCompany.ActorCompanyId, 0);
            return settingDate.Year > 1900 ? settingDate.ToString("yyyy-MM-dd HH:mm") : "";
        }

        private void UpdateInexchangeAPIRegisteredDate()
        {
            sm.UpdateInsertDateSetting(SettingMainType.Company, (int)CompanySettingType.InExchangeAPIRegisteredDate, DateTime.Now, UserId, SoeCompany.ActorCompanyId, 0);
        }

        #region Action-methods

        protected override void Save()
        {
            ValidateForm();

            bool success = true;


            #region Validate email
            string email = F["DefaultEmailAddress"];
            if (!string.IsNullOrEmpty(email))
            {
                var emailValidation = EmailManager.IsValidFromAddress(email);
                if (!emailValidation.Success)
                {
                    RedirectToSelf(emailValidation.ErrorMessage);
                }
            }
            #endregion
            
            #region API

            bool settingAPITestMode = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, UserId, SoeCompany.ActorCompanyId, 0);

            releaseModeApi = true;
            if (settingAPITestMode)
                releaseModeApi = false;

            foreach (string curr in F.AllKeys)
            {
                if (curr.StartsWith("RegisterAPI"))
                {
                    List<ContactAddressRow> companyAddress = new List<ContactAddressRow>();

                    Contact companyContactPreferences = ccm.GetContactFromActor(SoeCompany.ActorCompanyId);
                    if (companyContactPreferences != null)
                    {
                        companyAddress = ccm.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Distribution);
                    }
                    ContactAddressRow companyPostalCode = companyAddress.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                    ContactAddressRow companyPostalAddress = companyAddress.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                    ContactAddressRow companyAddressStreetName = companyAddress.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);

                    var inExchangeAPISendRegistered = StringUtility.GetBool(F["InExchangeAPISendRegistered"]);  
                    var inExchangeAPIReciveRegistered = StringUtility.GetBool(F["InExchangeAPIReciveRegistered"]); 

                    if (!inExchangeAPISendRegistered && !inExchangeAPIReciveRegistered)
                    {
                        this.RedirectToSelf("API_FAIL_REGISTER_NO_SELECTION", true);
                    }

                    if (companyAddressStreetName == null || companyPostalCode == null || companyPostalAddress == null)
                    {
                        this.RedirectToSelf("API_FAIL_REGISTER_NO_ADDRESS", true);
                    }

                    bool successAPI = InExchangeConnector.RegisterCompanyToInExchangeAPI(SoeCompany.ActorCompanyId, SoeCompany, companyAddressStreetName.Text, companyPostalCode.Text, companyPostalAddress.Text, email, inExchangeAPISendRegistered, inExchangeAPIReciveRegistered, releaseModeApi);

                    if (successAPI)
                    {
                        UpdateInexchangeAPIRegisteredDate();
                        var boolValuesAPI = new Dictionary<int, bool>();
                        boolValuesAPI.Add((int)CompanySettingType.InExchangeAPISendRegistered, inExchangeAPISendRegistered);
                        boolValuesAPI.Add((int)CompanySettingType.InExchangeAPIReciveRegistered, inExchangeAPIReciveRegistered);

                        if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValuesAPI, UserId, SoeCompany.ActorCompanyId, 0).Success)
                        {
                            success = false;
                            this.RedirectToSelf("API_FAIL_REGISTER_SETTING", true);
                        }
                        else
                        {
                            RegisterAPI.Enabled = false;
                            ActivateAPI.Enabled = true;
                            this.RedirectToSelf("API_REGISTERED", true);
                        }
                    }
                    else
                    {
                        this.RedirectToSelf("API_FAIL_REGISTER", true);
                    }

                }

                if (curr.StartsWith("ActivateAPI"))
                {
                    string country = "SE";
                    if (SoeCompany.SysCountryId == 3)
                        country = "FI";

                    bool successAPI = InExchangeConnector.ActivateCompany_In_InExchangeAPI(SoeCompany.ActorCompanyId, SoeCompany.OrgNr, country, releaseModeApi);

                    if (success)
                    {
                        var boolValuesAPI = new Dictionary<int, bool>();
                        boolValuesAPI.Add((int)CompanySettingType.InExchangeAPIActivated, true);
                        if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValuesAPI, UserId, SoeCompany.ActorCompanyId, 0).Success)
                        {
                            success = false;
                            this.RedirectToSelf("API_FAIL_ACTIVATE_SETTING", true);
                        }
                        else
                        {
                            ActivateAPI.Enabled = false;
                            this.RedirectToSelf("API_ACTIVATED", true);
                        }
                    }
                    else
                        this.RedirectToSelf("API_FAIL_ACTIVATE", true);
                }

                if (curr.StartsWith("FortnoxDeactivate"))
                {
                    sm.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFortnoxRefreshToken, "", 0, SoeCompany.ActorCompanyId, 0);
                    this.RedirectToSelf("UPDATED", true);
                }

                if (curr.StartsWith("VismaEAccountingDeactivate"))
                {
                    sm.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingVismaEAccountingRefreshToken, "", 0, SoeCompany.ActorCompanyId, 0);
                    this.RedirectToSelf("UPDATED", true);
                }

                if (curr.StartsWith("AzoraOneActivate"))
                {
                    //Upload company to AzoraOne and set setting
                    var ediManager = new EdiManager(ParameterObject);
                    var result = ediManager.ActivateAzoraOneWithAlternative(SoeCompany.ActorCompanyId, 
                        doSyncSuppliers: true);

                    if (!result.Success)
                        this.RedirectToSelf(result.ErrorMessage, false);
                    else
                        this.RedirectToSelf("API_ACTIVATED", false);
                }

                if (curr.StartsWith("AzoraOneDeactivate"))
                {
                    var ediManager = new EdiManager(ParameterObject);
                    var result = ediManager.DeactivateAzoraOneIntegration(SoeCompany.ActorCompanyId);
                    if (!result.Success)
                        this.RedirectToSelf(result.ErrorMessage, false);
                    else
                        this.RedirectToSelf("API_ACTIVATED", false);
                }

                if (curr.StartsWith("AzoraOneSendTrainingData"))
                {
                    var ediManager = new EdiManager(ParameterObject);
                    ediManager.StartTrainAzoraInterpretor(SoeCompany.ActorCompanyId);
                    this.RedirectToSelf("API_ACTIVATED", false);
                }
            }

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.BlockFromDateOnUserAfterNrOfDays, StringUtility.GetInt(F["BlockFromDateOnUserAfterNrOfDays"]));
            intValues.Add((int)CompanySettingType.DefaultRole, F["DefaultRole"] != null ? Int32.Parse(F["DefaultRole"]) : 0);
            intValues.Add((int)CompanySettingType.CleanReportPrintoutAfterNrOfDays, F["CleanReportPrintoutAfterNrOfDays"] != null ? Int32.Parse(F["CleanReportPrintoutAfterNrOfDays"]) : 0);

            // Password policy settings
            intValues.Add((int)CompanySettingType.CorePasswordMinLength, StringUtility.GetInt(F["PasswordMinLength"], Constants.PASSWORD_DEFAULT_MIN_LENGTH));
            intValues.Add((int)CompanySettingType.CorePasswordMaxLength, StringUtility.GetInt(F["PasswordMaxLength"], Constants.PASSWORD_DEFAULT_MAX_LENGTH));

            // Accounts
            if (this.isSupportAdmin)
            {
                intValues.Add((int)CompanySettingType.DefaultEmployeeAccountDimEmployee, F["DefaultEmployeeAccountDimEmployee"] != null ? Int32.Parse(F["DefaultEmployeeAccountDimEmployee"]) : 0);
                intValues.Add((int)CompanySettingType.DefaultEmployeeAccountDimSelector, F["DefaultEmployeeAccountDimSelector"] != null ? Int32.Parse(F["DefaultEmployeeAccountDimSelector"]) : 0);
            }

            // Dashboard
            int interval = StringUtility.GetInt(F["DashboardRefreshInterval"], 5);
            if (interval == 0)
                interval = 5;
            intValues.Add((int)CompanySettingType.DashboardRefreshInterval, interval);

            //Module icon
            intValues.Add((int)CompanySettingType.TimeModuleIcon, F["ModuleIconImage"] != null ? Int32.Parse(F["ModuleIconImage"]) : 7099);

            // CaseProject
            intValues.Add((int)CompanySettingType.CaseProjectAttestStateReceived, F["CaseProjectAttestStateReceived"] != null ? Int32.Parse(F["CaseProjectAttestStateReceived"]) : 0);

            //Intrium
            intValues.Add((int)CompanySettingType.IntrumClientNo, F["IntrumClientNo"] != null ? Int32.Parse(F["IntrumClientNo"]) : 0);
            intValues.Add((int)CompanySettingType.IntrumLedgerNo, F["IntrumLedgerNo"] != null ? Int32.Parse(F["IntrumLedgerNo"]) : 0);

            if (this.isSupportAdmin)
            {
                intValues.Add((int)CompanySettingType.ChainAffiliation, StringUtility.GetInt(F["ChainAffiliation"], 0));
            }

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region bool

            var invoiceUseBankIntegration = StringUtility.GetBool(F["FinvoiceUseBankIntegration"]);
            var finvoiceAddress = F["FinvoiceAddress"];

            //check that is not used by other companies
            if (invoiceUseBankIntegration)
            {
                var settings = sm.GetCompanySettingsWithUniqueStringValue((int)CompanySettingType.BillingFinvoiceAddress, finvoiceAddress);
                if (settings.Any(x=> x.ActorCompanyId != SoeCompany.ActorCompanyId))
                {
                    this.RedirectToSelf("FINVOICE_ADDRESS_ALREADY_IN_USE", true);
                }
            }

            var boolValues = new Dictionary<int, bool>
            {
                { (int)CompanySettingType.DoNotAddToSoftOneIdDirectlyOnSave, StringUtility.GetBool(F["DoNotAddToSoftOneIdDirectlyOnSave"]) },
                { (int)CompanySettingType.UseMissingMandatoryInformation, StringUtility.GetBool(F["MandatoryContactInformation"]) },
                { (int)CompanySettingType.UseDefaultEmailAddress, StringUtility.GetBool(F["UseDefaultEmailAddress"]) },
                { (int)CompanySettingType.UseExternalLinks, StringUtility.GetBool(F["ActivateExternalLinks"]) },
                { (int)CompanySettingType.SupplierInvoiceAutoReminder, StringUtility.GetBool(F["SupplierInvoiceAutoReminder"]) },
                { (int)CompanySettingType.ZetesTestMode, StringUtility.GetBool(F["ZetesTestMode"]) },
                { (int)CompanySettingType.IntrumTestMode, StringUtility.GetBool(F["IntrumTestMode"]) },
                { (int)CompanySettingType.DisableMessageOnInboundEmailError, StringUtility.GetBool(F["DisableMessageOnInboundEmailError"]) }
            };

            if (this.isSupportAdmin)
            {
                string useAccountHierarchySettingString = sm.GetSettingFromDict(manageSettingsDict, (int)CompanySettingType.UseAccountHierarchy, (int)SettingDataType.Boolean);
                if (String.IsNullOrEmpty(useAccountHierarchySettingString) || useAccountHierarchySettingString == Boolean.FalseString)
                    boolValues.Add((int)CompanySettingType.UseAccountHierarchy, StringUtility.GetBool(F["UseAccountsHierarchy"]));
                boolValues.Add((int)CompanySettingType.UseLimitedEmployeeAccountDimLevels, StringUtility.GetBool(F["UseLimitedEmployeeAccountDimLevels"]));
                boolValues.Add((int)CompanySettingType.UseExtendedEmployeeAccountDimLevels, StringUtility.GetBool(F["UseExtendedEmployeeAccountDimLevels"]));
                boolValues.Add((int)CompanySettingType.FallbackOnEmployeeAccountInPrio, StringUtility.GetBool(F["FallbackOnEmployeeAccountInPrio"]));
                boolValues.Add((int)CompanySettingType.BaseSelectableAccountsOnEmployeeInsteadOfAttestRole, StringUtility.GetBool(F["BaseSelectableAccountsOnEmployeeInsteadOfAttestRole"]));
                boolValues.Add((int)CompanySettingType.SendReminderToExecutivesBasedOnEmployeeAccountOnly, StringUtility.GetBool(F["SendReminderToExecutivesBasedOnEmployeeAccountOnly"]));
            }

            boolValues.Add((int)CompanySettingType.FinvoiceUseBankIntegration, StringUtility.GetBool(F["FinvoiceUseBankIntegration"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region string

            var stringValues = new Dictionary<int, string>
            {
                { (int)CompanySettingType.DefaultEmailAddress, F["DefaultEmailAddress"] },
                { (int)CompanySettingType.ExternalLink1, F["ExternalLink1"] },
                { (int)CompanySettingType.ExternalLink2, F["ExternalLink2"] },
                { (int)CompanySettingType.ExternalLink3, F["ExternalLink3"] },
                { (int)CompanySettingType.TimeModuleHeader, F["PersonellModuleHeader"] },
                { (int)CompanySettingType.InExchangeFtpAddress, F["InExchangeAddress"] },
                { (int)CompanySettingType.InExchangeFtpUsername, F["InExchangeUser"] },
                { (int)CompanySettingType.InExchangeFtpPassword, F["InExchangePasswd"] },
                { (int)CompanySettingType.InExchangeFtpAddressTest, F["InExchangeAddressTest"] },
                { (int)CompanySettingType.InExchangeFtpUsernameTest, F["InExchangeUserTest"] },
                { (int)CompanySettingType.InExchangeFtpPasswordTest, F["InExchangePasswdTest"] },
                { (int)CompanySettingType.IntrumHubNo, F["IntrumHubNo"] },
                { (int)CompanySettingType.KivraTenentKey, F["KivraTenentKey"] },
                { (int)CompanySettingType.IntrumUser, F["IntrumUser"] },
                { (int)CompanySettingType.IntrumPwd, F["IntrumPwd"] },
                { (int)CompanySettingType.ZetesUser, F["ZetesUser"] },
                { (int)CompanySettingType.ZetesPwd, F["ZetesPwd"] },
                { (int)CompanySettingType.ZetesClientCode, F["ZetesClientCode"] },
                { (int)CompanySettingType.ZetesStakeholderCode, F["ZetesStakeholderCode"] },
                { (int)CompanySettingType.BillingFinvoiceAddress, F["FinvoiceAddress"] },
                { (int)CompanySettingType.BillingFinvoiceOperator, F["FinvoiceOperator"] }
            };

            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Support login

            if (company != null && !this.isSupportAdmin)
            {
                bool allowSupportLogin = StringUtility.GetBool(F["AllowSupportLogin"]);
                string loginTo = F["SupportLoginTo"];
                string loginToTime = F["SupportLoginTimeTo"];
                DateTime? allowSupportLoginTo = null;

                if (!String.IsNullOrEmpty(loginTo))
                {
                    //If date is set but not time, set 23:59:59
                    if (String.IsNullOrEmpty(loginToTime))
                        loginToTime = "23:59:00";

                    allowSupportLoginTo = CalendarUtility.ApplyTimeOnDateTime(Convert.ToDateTime(loginTo), loginToTime);
                }

                company.AllowSupportLogin = allowSupportLogin;
                company.AllowSupportLoginTo = allowSupportLoginTo;

                success = cm.UpdateCompany(company).Success;
            }

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
            int passwordMinLength = StringUtility.GetInt(F["PasswordMinLength"], Constants.PASSWORD_DEFAULT_MIN_LENGTH);
            if (passwordMinLength < Constants.PASSWORD_DEFAULT_MIN_LENGTH || passwordMinLength > Constants.PASSWORD_DEFAULT_MAX_LENGTH)
                RedirectToSelf("INVALID_PASSWORD_SETTINGS", true);

            int passwordMaxLength = StringUtility.GetInt(F["PasswordMaxLength"], Constants.PASSWORD_DEFAULT_MAX_LENGTH);
            if (passwordMaxLength < Constants.PASSWORD_DEFAULT_MIN_LENGTH || passwordMaxLength > Constants.PASSWORD_DEFAULT_MAX_LENGTH)
                RedirectToSelf("INVALID_PASSWORD_SETTINGS", true);

            if (StringUtility.GetBool(F["UseDefaultEmailAddress"]) && F["DefaultEmailAddress"] == String.Empty)
                RedirectToSelf("NO_EMAILADDRESS_DEFINED", true);
        }

        protected void AddModuleIconItems()
        {
            Dictionary<int, string> iconsDict = new Dictionary<int, string>();

            foreach (SoeModuleIconType icon in Enum.GetValues(typeof(SoeModuleIconType)))
            {
                //exclude empty for now
                if ((int)icon > 0)
                {
                    iconsDict.Add((int)icon, GetText((int)icon, ""));
                }
            }

            ModuleIconImage.ConnectDataSource(iconsDict);
        }

        protected void ButtonRegister_Click(object sender, EventArgs e)
        {

        }

        protected void ButtonActivate_Click(object sender, EventArgs e)
        {
            bool success = InExchangeConnector.ActivateCompany_In_InExchangeAPI(SoeCompany.ActorCompanyId, SoeCompany.OrgNr, "", releaseModeApi);

            if (success)
            {
                var boolValues = new Dictionary<int, bool>();
                boolValues.Add((int)CompanySettingType.InExchangeAPIActivated, true);
                if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                {
                    success = false;
                }
                else
                {
                    ActivateAPI.Enabled = false;
                    this.RedirectToSelf("API_ACTIVATED", true);
                }
            }
            else
                this.RedirectToSelf("API_FAIL_ACTIVATE", true);

        }
        #endregion

        #region VismaEAccounting
        protected bool CheckIfHasEAccountingToken()
        {
            var integrator = new VismaEAccountingIntegrationManager();
            var setting = sm.GetUserCompanySetting(SettingMainType.Company, (int)integrator.Params.RefreshTokenStoragePoint, 0, SoeCompany.ActorCompanyId, 0);
            var integrationIsActive = setting != null && setting.StrData.HasValue();

            if (integrationIsActive) VismaEAccountingInstruction.LabelSetting = GetText(9375, "Integrationen är aktiverad.");
            else VismaEAccountingInstruction.LabelSetting = GetText(9376, "Integrationen är inte aktiverad.");

            VismaEAccountingDeactivate.Visible = integrationIsActive;
            VismaEAccountingActivationUrl.Visible = !integrationIsActive;

            var uri = this.Request.Url.ToString();
            VismaEAccountingActivationUrl.HRef = integrator.GetActivationUrl(uri);

            return integrationIsActive;
        }

        protected bool CheckIfUpdatingEAccountingToken()
        {
            var connector = new VismaEAccountingIntegrationManager();
            var key = GetUrlParameter(this.Url, "vismaeaccountingcode");
            if (key.HasValue())
            {
                connector.SetAuthFromCode(key);
                var refreshToken = connector.GetRefreshToken();

                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                sm.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingVismaEAccountingRefreshToken, refreshToken, 0, SoeCompany.ActorCompanyId, 0);
                RedirectToSelf("UPDATED", absolutePath: true);
                return true;
            }
            return false;
        }

        #endregion

        #region Fortnox
        protected bool CheckIfHasFortnoxToken() 
        {
            var setting = sm.GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.BillingFortnoxRefreshToken, 0, SoeCompany.ActorCompanyId, 0);
            var integrationIsActive = setting != null && setting.StrData.HasValue();
            
            if (integrationIsActive) FortnoxInstruction.LabelSetting = GetText(9375, "Integrationen är aktiverad.");
            else FortnoxInstruction.LabelSetting = GetText(9376, "Integrationen är inte aktiverad.");
            
            FortnoxDeactivate.Visible = integrationIsActive;
            FortnoxActivationUrl.Visible = !integrationIsActive;

            var uri = this.Request.Url.ToString();
            FortnoxActivationUrl.HRef = FortnoxConnector.GetActivationUrl(uri);
            
            return integrationIsActive;
        }

        protected bool CheckIfUpdatingFortnoxToken()
        {
            var connector = new FortnoxConnector();
            var key = GetUrlParameter(this.Url, "fortnoxkey");
            if (key.HasValue())
            {
                connector.SetAuthFromCode(key);
                var refreshToken = connector.GetRefreshToken();
                sm.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFortnoxRefreshToken, refreshToken, 0, SoeCompany.ActorCompanyId, 0);
                RedirectToSelf("UPDATED", absolutePath: true);
                return true;
            }
            return false;
        }
        #endregion

        #region AzoraOne

        public void CheckIfHasAzoraOneActivated()
        {
            var azoraOneStatus = sm.GetCompanyIntSetting(CompanySettingType.ScanningUsesAzoraOne);

            if (azoraOneStatus > (int)AzoraOneStatus.ActivatedInBackground)
            {
                AzoraOneInstruction.LabelSetting = GetText(9375, "Integrationen är aktiverad.");
                AzoraOneActivate.Visible = false;
            }
            else
            {
                AzoraOneInstruction.LabelSetting = GetText(9376, "Integrationen är inte aktiverad.");
                AzoraOneSendTrainingData.Visible = false;
                AzoraOneDeactivate.Visible = false;
            }
        }

        #endregion
    }
}
