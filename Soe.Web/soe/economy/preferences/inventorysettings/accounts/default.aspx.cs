using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.preferences.inventorysettings.accounts
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        protected SettingManager sm;

        public int AccountDimId { get; set; }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_InventorySettings_Accounts;
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

            SetAccountField(CompanySettingType.AccountInventoryInventories, AccountInventoryInventories, "AccountInventoryInventories");
            SetAccountField(CompanySettingType.AccountInventoryAccWriteOff, AccountInventoryAccWriteOff, "AccountInventoryAccWriteOff");
            SetAccountField(CompanySettingType.AccountInventoryWriteOff, AccountInventoryWriteOff, "AccountInventoryWriteOff");
            SetAccountField(CompanySettingType.AccountInventoryAccOverWriteOff, AccountInventoryAccOverWriteOff, "AccountInventoryAccOverWriteOff");
            SetAccountField(CompanySettingType.AccountInventoryOverWriteOff, AccountInventoryOverWriteOff, "AccountInventoryOverWriteOff");
            SetAccountField(CompanySettingType.AccountInventoryAccWriteDown, AccountInventoryAccWriteDown, "AccountInventoryAccWriteDown");
            SetAccountField(CompanySettingType.AccountInventoryWriteDown, AccountInventoryWriteDown, "AccountInventoryWriteDown");
            SetAccountField(CompanySettingType.AccountInventoryAccWriteUp, AccountInventoryAccWriteUp, "AccountInventoryAccWriteUp");
            SetAccountField(CompanySettingType.AccountInventoryWriteUp, AccountInventoryWriteUp, "AccountInventoryWriteUp");
            SetAccountField(CompanySettingType.AccountInventorySalesProfit, AccountInventorySalesProfit, "AccountInventorySalesProfit");
            SetAccountField(CompanySettingType.AccountInventorySalesLoss, AccountInventorySalesLoss, "AccountInventorySalesLoss");
            SetAccountField(CompanySettingType.AccountInventorySales, AccountInventorySales, "AccountInventorySales");

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

            intValues.Add((int)CompanySettingType.AccountInventoryInventories, GetAccountId(F["AccountInventoryInventories"]));
            intValues.Add((int)CompanySettingType.AccountInventoryAccWriteOff, GetAccountId(F["AccountInventoryAccWriteOff"]));
            intValues.Add((int)CompanySettingType.AccountInventoryWriteOff, GetAccountId(F["AccountInventoryWriteOff"]));
            intValues.Add((int)CompanySettingType.AccountInventoryAccOverWriteOff, GetAccountId(F["AccountInventoryAccOverWriteOff"]));
            intValues.Add((int)CompanySettingType.AccountInventoryOverWriteOff, GetAccountId(F["AccountInventoryOverWriteOff"]));
            intValues.Add((int)CompanySettingType.AccountInventoryAccWriteDown, GetAccountId(F["AccountInventoryAccWriteDown"]));
            intValues.Add((int)CompanySettingType.AccountInventoryWriteDown, GetAccountId(F["AccountInventoryWriteDown"]));
            intValues.Add((int)CompanySettingType.AccountInventoryAccWriteUp, GetAccountId(F["AccountInventoryAccWriteUp"]));
            intValues.Add((int)CompanySettingType.AccountInventoryWriteUp, GetAccountId(F["AccountInventoryWriteUp"]));
            intValues.Add((int)CompanySettingType.AccountInventorySalesProfit, GetAccountId(F["AccountInventorySalesProfit"]));
            intValues.Add((int)CompanySettingType.AccountInventorySalesLoss, GetAccountId(F["AccountInventorySalesLoss"]));
            intValues.Add((int)CompanySettingType.AccountInventorySales, GetAccountId(F["AccountInventorySales"]));

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
