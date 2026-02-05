using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings.accounts
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
            this.Feature = Feature.Economy_Preferences_SuppInvoiceSettings_Accounts;
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

            SetAccountField(CompanySettingType.AccountSupplierDebt, AccountSupplierDebt, "AccountSupplierDebt");
            SetAccountField(CompanySettingType.AccountSupplierPurchase, AccountSupplierPurchase, "AccountSupplierPurchase");
            SetAccountField(CompanySettingType.AccountSupplierInterim, AccountSupplierInterim, "AccountSupplierInterim");
            SetAccountField(CompanySettingType.AccountSupplierUnderpay, AccountSupplierUnderpay, "AccountSupplierUnderpay");
            SetAccountField(CompanySettingType.AccountSupplierOverpay, AccountSupplierOverpay, "AccountSupplierOverpay");

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

            intValues.Add((int)CompanySettingType.AccountSupplierDebt, GetAccountId(F["AccountSupplierDebt"]));
            intValues.Add((int)CompanySettingType.AccountSupplierPurchase, GetAccountId(F["AccountSupplierPurchase"]));
            intValues.Add((int)CompanySettingType.AccountSupplierInterim, GetAccountId(F["AccountSupplierInterim"]));
            intValues.Add((int)CompanySettingType.AccountSupplierUnderpay, GetAccountId(F["AccountSupplierUnderpay"]));
            intValues.Add((int)CompanySettingType.AccountSupplierOverpay, GetAccountId(F["AccountSupplierOverpay"]));

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
