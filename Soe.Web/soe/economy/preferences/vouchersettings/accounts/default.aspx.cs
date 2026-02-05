using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.accounts
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
            this.Feature = Feature.Economy_Preferences_VoucherSettings_Accounts;
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

            #region Populate

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;
            stdDimID = AccountDimId.ToString();
                       

            SetAccountField(CompanySettingType.AccountCommonVatPayable1, AccountCommonVatPayable1, "AccountCommonVatPayable1");
            SetAccountField(CompanySettingType.AccountCommonVatPayable2, AccountCommonVatPayable2, "AccountCommonVatPayable2");
            SetAccountField(CompanySettingType.AccountCommonVatPayable3, AccountCommonVatPayable3, "AccountCommonVatPayable3");
            SetAccountField(CompanySettingType.AccountCommonMixedVat, AccountCommonMixedVat, "AccountCommonMixedVat");
            SetAccountField(CompanySettingType.AccountCommonVatReceivable, AccountCommonVatReceivable, "AccountCommonVatReceivable");
            SetAccountField(CompanySettingType.AccountCommonVatAccountingKredit, AccountCommonVatAccountingKredit, "AccountCommonVatAccountingKredit");
            SetAccountField(CompanySettingType.AccountCommonVatAccountingDebet, AccountCommonVatAccountingDebet, "AccountCommonVatAccountingDebet");
            SetAccountField(CompanySettingType.AccountCommonVatPayable1Reversed, AccountCommonVatPayable1Reversed, "AccountCommonVatPayable1Reversed");
            SetAccountField(CompanySettingType.AccountCommonVatPayable2Reversed, AccountCommonVatPayable2Reversed, "AccountCommonVatPayable2Reversed");
            SetAccountField(CompanySettingType.AccountCommonVatPayable3Reversed, AccountCommonVatPayable3Reversed, "AccountCommonVatPayable3Reversed");
            SetAccountField(CompanySettingType.AccountCommonVatReceivableReversed, AccountCommonVatReceivableReversed, "AccountCommonVatReceivableReversed");
            SetAccountField(CompanySettingType.AccountCommonCheck, AccountCommonCheck, "AccountCommonCheck");
            SetAccountField(CompanySettingType.AccountCommonPG, AccountCommonPG, "AccountCommonPG");
            SetAccountField(CompanySettingType.AccountCommonBG, AccountCommonBG, "AccountCommonBG");
            SetAccountField(CompanySettingType.AccountCommonAG, AccountCommonAG, "AccountCommonAG");
            SetAccountField(CompanySettingType.AccountCommonCentRounding, AccountCommonCentRounding, "AccountCommonCentRounding");
            SetAccountField(CompanySettingType.AccountCommonCurrencyProfit, AccountCommonCurrencyProfit, "AccountCommonCurrencyProfit");
            SetAccountField(CompanySettingType.AccountCommonCurrencyLoss, AccountCommonCurrencyLoss, "AccountCommonCurrencyLoss");
            SetAccountField(CompanySettingType.AccountCommonDiff, AccountCommonDiff, "AccountCommonDiff");
            SetAccountField(CompanySettingType.AccountCommonBankFee, AccountCommonBankFee, "AccountCommonBankFee");
            SetAccountField(CompanySettingType.AccountCommonReverseVatSales, AccountCommonReverseVatSales, "AccountCommonReverseVatSales");
            SetAccountField(CompanySettingType.AccountCommonReverseVatPurchase, AccountCommonReverseVatPurchase, "AccountCommonReverseVatPurchase");
            //EU Moms
            SetAccountField(CompanySettingType.AccountCommonVatPayable1EUImport, AccountCommonVatPayable1EUImport, "AccountCommonVatPayable1EUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPayable2EUImport, AccountCommonVatPayable2EUImport, "AccountCommonVatPayable2EUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPayable3EUImport, AccountCommonVatPayable3EUImport, "AccountCommonVatPayable3EUImport");
            SetAccountField(CompanySettingType.AccountCommonVatReceivableEUImport, AccountCommonVatReceivableEUImport, "AccountCommonVatReceivableEUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPurchaseEUImport, AccountCommonVatPurchaseEUImport, "AccountCommonVatPurchaseEUImport");
            //ej EU MOMS
            SetAccountField(CompanySettingType.AccountCommonVatPayable1NonEUImport, AccountCommonVatPayable1NonEUImport, "AccountCommonVatPayable1NonEUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPayable2NonEUImport, AccountCommonVatPayable2NonEUImport, "AccountCommonVatPayable2NonEUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPayable3NonEUImport, AccountCommonVatPayable3NonEUImport, "AccountCommonVatPayable3NonEUImport");
            SetAccountField(CompanySettingType.AccountCommonVatReceivableNonEUImport, AccountCommonVatReceivableNonEUImport, "AccountCommonVatReceivableNonEUImport");
            SetAccountField(CompanySettingType.AccountCommonVatPurchaseNonEUImport, AccountCommonVatPurchaseNonEUImport, "AccountCommonVatPurchaseNonEUImport");
            //Accral Settings
            SetAccountField(CompanySettingType.AccountCommonAccrualCostAccount, AccountCommonAccrualCostAccount, "AccountCommonAccrualCostAccount");
            SetAccountField(CompanySettingType.AccountCommonAccrualRevenueAccount, AccountCommonAccrualRevenueAccount, "AccountCommonAccrualRevenueAccount");

            int pos = 0;
            var data = am.GetAccrualAccountMappings(SoeCompany.ActorCompanyId);
            foreach (var item in data)
            {
                this.AccrualAccountMapping.AddValueFrom(pos, item.SourceAccountNr);
                this.AccrualAccountMapping.AddValueTo(pos, item.TargetAccrualAccountNr);
                pos++;
            }

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

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

            intValues.Add((int)CompanySettingType.AccountCommonVatPayable1, GetAccountId(F["AccountCommonVatPayable1"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable2, GetAccountId(F["AccountCommonVatPayable2"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable3, GetAccountId(F["AccountCommonVatPayable3"]));
            intValues.Add((int)CompanySettingType.AccountCommonMixedVat, GetAccountId(F["AccountCommonMixedVat"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatReceivable, GetAccountId(F["AccountCommonVatReceivable"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatAccountingKredit, GetAccountId(F["AccountCommonVatAccountingKredit"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatAccountingDebet, GetAccountId(F["AccountCommonVatAccountingDebet"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable1Reversed, GetAccountId(F["AccountCommonVatPayable1Reversed"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable2Reversed, GetAccountId(F["AccountCommonVatPayable2Reversed"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable3Reversed, GetAccountId(F["AccountCommonVatPayable3Reversed"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatReceivableReversed, GetAccountId(F["AccountCommonVatReceivableReversed"]));
            intValues.Add((int)CompanySettingType.AccountCommonCheck, GetAccountId(F["AccountCommonCheck"]));
            intValues.Add((int)CompanySettingType.AccountCommonPG, GetAccountId(F["AccountCommonPG"]));
            intValues.Add((int)CompanySettingType.AccountCommonBG, GetAccountId(F["AccountCommonBG"]));
            intValues.Add((int)CompanySettingType.AccountCommonAG, GetAccountId(F["AccountCommonAG"]));
            intValues.Add((int)CompanySettingType.AccountCommonCentRounding, GetAccountId(F["AccountCommonCentRounding"]));
            intValues.Add((int)CompanySettingType.AccountCommonCurrencyProfit, GetAccountId(F["AccountCommonCurrencyProfit"]));
            intValues.Add((int)CompanySettingType.AccountCommonCurrencyLoss, GetAccountId(F["AccountCommonCurrencyLoss"]));
            intValues.Add((int)CompanySettingType.AccountCommonBankFee, GetAccountId(F["AccountCommonBankFee"]));
            intValues.Add((int)CompanySettingType.AccountCommonDiff, GetAccountId(F["AccountCommonDiff"]));
            intValues.Add((int)CompanySettingType.AccountCommonReverseVatSales, GetAccountId(F["AccountCommonReverseVatSales"]));
            intValues.Add((int)CompanySettingType.AccountCommonReverseVatPurchase, GetAccountId(F["AccountCommonReverseVatPurchase"]));
            //EU MOMS
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable1EUImport, GetAccountId(F["AccountCommonVatPayable1EUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable2EUImport, GetAccountId(F["AccountCommonVatPayable2EUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable3EUImport, GetAccountId(F["AccountCommonVatPayable3EUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatReceivableEUImport, GetAccountId(F["AccountCommonVatReceivableEUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPurchaseEUImport, GetAccountId(F["AccountCommonVatPurchaseEUImport"]));
            //ej EU MOMS
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable1NonEUImport, GetAccountId(F["AccountCommonVatPayable1NonEUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable2NonEUImport, GetAccountId(F["AccountCommonVatPayable2NonEUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPayable3NonEUImport, GetAccountId(F["AccountCommonVatPayable3NonEUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatReceivableNonEUImport, GetAccountId(F["AccountCommonVatReceivableNonEUImport"]));
            intValues.Add((int)CompanySettingType.AccountCommonVatPurchaseNonEUImport, GetAccountId(F["AccountCommonVatPurchaseNonEUImport"]));
            //Accrual Settings
            intValues.Add((int)CompanySettingType.AccountCommonAccrualCostAccount, GetAccountId(F["AccountCommonAccrualCostAccount"]));
            intValues.Add((int)CompanySettingType.AccountCommonAccrualRevenueAccount, GetAccountId(F["AccountCommonAccrualRevenueAccount"]));

            SaveAccrualAccountMappings();

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

        private void SaveAccrualAccountMappings()
        {
            Collection<FormIntervalEntryItem>  intervals = this.AccrualAccountMapping.GetData(F);
            List<AccrualAccountMappingDTO> accrualAccountMappings = new List<AccrualAccountMappingDTO>();

            foreach (FormIntervalEntryItem interval in intervals)
            {
                var fromccountId = am.GetAccountStdIdFromAccountNr(interval.From, SoeCompany.ActorCompanyId);
                var toAccountId = am.GetAccountStdIdFromAccountNr(interval.To, SoeCompany.ActorCompanyId);

                accrualAccountMappings.Add(new AccrualAccountMappingDTO
                {
                    SourceAccountId = fromccountId,
                    TargetAccrualAccountId = toAccountId
                });
            }

            am.SaveAccrualAccountMappings(accrualAccountMappings, SoeCompany.ActorCompanyId);
        }

        #endregion
    }
}
