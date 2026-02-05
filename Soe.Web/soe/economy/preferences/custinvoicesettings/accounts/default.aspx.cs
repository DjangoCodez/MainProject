using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.preferences.custinvoicesettings.accounts
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        protected SettingManager sm;

        public int AccountDimId { get; set; }
        public string stdDimID; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_CustInvoiceSettings_Accounts;
            base.Page_Init(sender, e);
            Scripts.Add("/cssjs/account.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            sm = new SettingManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;

            stdDimID = accountDimStd.AccountDimId.ToString();

            SetAccountField(CompanySettingType.AccountCustomerSalesVat, AccountCustomerSalesVat, "AccountCustomerSalesVat");
            SetAccountField(CompanySettingType.AccountCustomerSalesNoVat, AccountCustomerSalesNoVat, "AccountCustomerSalesNoVat");
            SetAccountField(CompanySettingType.AccountCustomerSalesWithinEU, AccountCustomerSalesWithinEU, "AccountCustomerSalesWithinEU");
            SetAccountField(CompanySettingType.AccountCustomerSalesOutsideEU, AccountCustomerSalesOutsideEU, "AccountCustomerSalesOutsideEU");
            SetAccountField(CompanySettingType.AccountCustomerSalesWithinEUService, AccountCustomerSalesWithinEUService, "AccountCustomerSalesWithinEUService");
            SetAccountField(CompanySettingType.AccountCustomerSalesOutsideEUService, AccountCustomerSalesOutsideEUService, "AccountCustomerSalesOutsideEUService");
            SetAccountField(CompanySettingType.AccountCustomerSalesTripartiteTrade, AccountCustomerSalesTripartiteTrade, "AccountCustomerSalesTripartiteTrade");
            SetAccountField(CompanySettingType.AccountCustomerFreight, AccountCustomerFreight, "AccountCustomerFreight");
            SetAccountField(CompanySettingType.AccountCustomerOrderFee, AccountCustomerOrderFee, "AccountCustomerOrderFee");
            SetAccountField(CompanySettingType.AccountCustomerInsurance, AccountCustomerInsurance, "AccountCustomerInsurance");
            SetAccountField(CompanySettingType.AccountCustomerClaim, AccountCustomerClaim, "AccountCustomerClaim");
            SetAccountField(CompanySettingType.AccountCustomerUnderpay, AccountCustomerUnderpay, "AccountCustomerUnderpay");
            SetAccountField(CompanySettingType.AccountCustomerOverpay, AccountCustomerOverpay, "AccountCustomerOverpay");
            SetAccountField(CompanySettingType.AccountCustomerPenaltyInterest, AccountCustomerPenaltyInterest, "AccountCustomerPenaltyInterest");
            SetAccountField(CompanySettingType.AccountCustomerClaimCharge, AccountCustomerClaimCharge, "AccountCustomerClaimCharge");
            SetAccountField(CompanySettingType.AccountCustomerPaymentFromTaxAgency, AccountCustomerPaymentFromTaxAgency, "AccountCustomerPaymentFromTaxAgency");
            SetAccountField(CompanySettingType.AccountUncertainCustomerClaim, AccountUncertainCustomerClaim, "AccountUncertainCustomerClaim");
            SetAccountField(CompanySettingType.AccountCustomerDiscount, AccountCustomerDiscount, "AccountCustomerDiscount");
            SetAccountField(CompanySettingType.AccountCustomerDiscountOffset, AccountCustomerDiscountOffset, "AccountCustomerDiscountOffset");
            int countryId;
            //SyscountryID 0,1 = Sweden , 3=Finland (FI) , 4 = Norge (NO) ... hide from Finnish and Norwegian users 
            int.TryParse(SoeCompany.SysCountryId.ToString(), out countryId);
            if (countryId == 3 || countryId == 4)
                AccountCustomerPaymentFromTaxAgency.Visible = false;

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.AccountCustomerSalesVat, GetAccountId(F["AccountCustomerSalesVat"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesNoVat, GetAccountId(F["AccountCustomerSalesNoVat"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesWithinEU, GetAccountId(F["AccountCustomerSalesWithinEU"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesOutsideEU, GetAccountId(F["AccountCustomerSalesOutsideEU"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesWithinEUService, GetAccountId(F["AccountCustomerSalesWithinEUService"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesOutsideEUService, GetAccountId(F["AccountCustomerSalesOutsideEUService"]));
            intValues.Add((int)CompanySettingType.AccountCustomerSalesTripartiteTrade, GetAccountId(F["AccountCustomerSalesTripartiteTrade"]));
            intValues.Add((int)CompanySettingType.AccountCustomerFreight, GetAccountId(F["AccountCustomerFreight"]));
            intValues.Add((int)CompanySettingType.AccountCustomerOrderFee, GetAccountId(F["AccountCustomerOrderFee"]));
            intValues.Add((int)CompanySettingType.AccountCustomerInsurance, GetAccountId(F["AccountCustomerInsurance"]));
            intValues.Add((int)CompanySettingType.AccountCustomerClaim, GetAccountId(F["AccountCustomerClaim"]));
            intValues.Add((int)CompanySettingType.AccountCustomerUnderpay, GetAccountId(F["AccountCustomerUnderpay"]));
            intValues.Add((int)CompanySettingType.AccountCustomerOverpay, GetAccountId(F["AccountCustomerOverpay"]));
            intValues.Add((int)CompanySettingType.AccountCustomerPenaltyInterest, GetAccountId(F["AccountCustomerPenaltyInterest"]));
            intValues.Add((int)CompanySettingType.AccountCustomerClaimCharge, GetAccountId(F["AccountCustomerClaimCharge"]));
            intValues.Add((int)CompanySettingType.AccountCustomerPaymentFromTaxAgency, GetAccountId(F["AccountCustomerPaymentFromTaxAgency"]));
            intValues.Add((int)CompanySettingType.AccountUncertainCustomerClaim, GetAccountId(F["AccountUncertainCustomerClaim"]));
            intValues.Add((int)CompanySettingType.AccountCustomerDiscount, GetAccountId(F["AccountCustomerDiscount"]));
            intValues.Add((int)CompanySettingType.AccountCustomerDiscountOffset, GetAccountId(F["AccountCustomerDiscountOffset"]));

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        private void SetAccountField(CompanySettingType type, TextEntry control, string id)
        {
            int accountId = sm.GetIntSetting(SettingMainType.Company, (int)type, UserId, SoeCompany.ActorCompanyId, 0);
            Account account = am.GetAccount(SoeCompany.ActorCompanyId, accountId);
            if (account != null)
            {
                control.Value = GetAccountNr(accountId);
                control.InfoText = account.Name;
            }
            control.OnChange = "getAccountName('" + id + "', '" + AccountDimId + "')";
        }

        private int GetAccountId(string accountNr)
        {
            // No account entered
            if (String.IsNullOrEmpty(accountNr))
                return 0;

            // Get account by specified number
            AccountStd acc = am.GetAccountStdByNr(accountNr, SoeCompany.ActorCompanyId);

            // Invalid account number
            if (acc == null)
                return 0;

            return acc.AccountId;
        }

        private string GetAccountNr(int accountId)
        {
            // No account specified
            if (accountId == 0)
                return String.Empty;

            // Get account by specified id
            Account account = am.GetAccount(SoeCompany.ActorCompanyId, accountId);

            // Invalid account id
            if (account == null)
                return String.Empty;

            return account.AccountNr;
        }

        #endregion
    }
}

