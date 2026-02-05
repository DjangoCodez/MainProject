using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.time.preferences.timesettings.accounts
{
    public partial class _default : PageBase
    {
        #region Variables

        public int AccountDimId { get; set; }
        public string stdDimID; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_TimeSettings_Accounts;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            //Scripts.Add("texts1.js.aspx");
            //Scripts.Add("texts2.js.aspx");
            Scripts.Add("/cssjs/account.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

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

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;
            stdDimID = AccountDimId.ToString();

            // Common
            SetAccountField(CompanySettingType.AccountEmployeeGroupCost, EmployeeGroupCost, "EmployeeGroupCost");
            SetAccountField(CompanySettingType.AccountEmployeeGroupIncome, EmployeeGroupIncome, "EmployeeGroupIncome");

            // Payroll
            SetAccountField(CompanySettingType.AccountPayrollEmploymentTax, EmploymentTax, "EmploymentTax");
            SetAccountField(CompanySettingType.AccountPayrollPayrollTax, PayrollTax, "PayrollTax");
            SetAccountField(CompanySettingType.AccountPayrollOwnSupplementCharge, OwnSupplementCharge, "OwnSupplementCharge");
            DivPayroll.Visible = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UsePayroll, UserId, SoeCompany.ActorCompanyId, 0);

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
            bool success = true;

            #region Int

            var intValues = new Dictionary<int, int>();

            // Common
            intValues.Add((int)CompanySettingType.AccountEmployeeGroupCost, GetAccountId(F["EmployeeGroupCost"]));
            intValues.Add((int)CompanySettingType.AccountEmployeeGroupIncome, GetAccountId(F["EmployeeGroupIncome"]));

            // Payroll
            intValues.Add((int)CompanySettingType.AccountPayrollEmploymentTax, GetAccountId(F["EmploymentTax"]));
            intValues.Add((int)CompanySettingType.AccountPayrollPayrollTax, GetAccountId(F["PayrollTax"]));
            intValues.Add((int)CompanySettingType.AccountPayrollOwnSupplementCharge, GetAccountId(F["OwnSupplementCharge"]));

            if (!SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
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
            int accountId = SettingManager.GetIntSetting(SettingMainType.Company, (int)type, UserId, SoeCompany.ActorCompanyId, 0);
            Account account = AccountManager.GetAccount(SoeCompany.ActorCompanyId, accountId);
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
            AccountStd acc = AccountManager.GetAccountStdByNr(accountNr, SoeCompany.ActorCompanyId);

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
            Account account = AccountManager.GetAccount(SoeCompany.ActorCompanyId, accountId);

            // Invalid account id
            if (account == null)
                return String.Empty;

            return account.AccountNr;
        }

        #endregion
    }
}
