using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.matchsettings.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        protected TermManager tm;
        protected InvoiceManager im;

        protected int matchCodeId;
        protected MatchCode matchCode;

        public int AccountDimId { get; set; }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_VoucherSettings_MatchCodes;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
            Scripts.Add("texts.js.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            tm = new TermManager(ParameterObject);
            im = new InvoiceManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);
            
            //Optional parameters
            if (Int32.TryParse(QS["matchcode"], out matchCodeId))
            {
                matchCode = im.GetMatchCode(matchCodeId);
                if (matchCode == null)
                {
                    Form1.MessageWarning = GetText(7128, "Restkod hittades inte");
                    Mode = SoeFormMode.Register;
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(7119, "Redigera restkod");
            string registerModeTabHeaderText = GetText(7118, "Registrera restkod");
            PostOptionalParameterCheck(Form1, matchCode, true, editModeTabHeaderText, registerModeTabHeaderText);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            MatchCodeType.ConnectDataSource(base.GetGrpText(TermGroup.MatchCodeType), "Name", "SysTermId");

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;


            #endregion

            #region Set data

            MatchCodeAccount.OnChange = "getAccountName('MatchCodeAccount', '" + accountDimStd.AccountDimId + "')";

            if (matchCode != null)
            {
                Name.Value = matchCode.Name;
                Description.Value = matchCode.Description;
                MatchCodeType.Value = matchCode.Type.ToString();
                
                //Account
                if (matchCode.AccountId != 0)
                {
                    Account account = am.GetAccount(SoeCompany.ActorCompanyId, matchCode.AccountId);
                    if (account != null)
                    {
                        MatchCodeAccount.Value = account.AccountNr;
                        MatchCodeAccount.InfoText = account.Name;
                    }
                }

                //Account
                if (matchCode.VatAccountId != null && matchCode.VatAccountId != 0)
                {
                    Account account = am.GetAccount(SoeCompany.ActorCompanyId, (int)matchCode.VatAccountId);
                    if (account != null)
                    {
                        MatchCodeVatAccount.Value = account.AccountNr;
                        MatchCodeVatAccount.InfoText = account.Name;
                    }
                }
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(7120, "Restkod sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(7121, "Restkod kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(7122, "Restkod uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(7123, "Restkod kunde inte uppdateras");
                else if (MessageFromSelf == "TYPE_MANDATORY")
                    Form1.MessageWarning = GetText(7124, "Typ måste anges");
                else if (MessageFromSelf == "ACCOUNT_MANDATORY")
                    Form1.MessageWarning = GetText(7125, "Konto måste anges");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(7126, "Restkod borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(7127, "Restkod kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (matchCode != null)
            {
                Form1.SetRegLink(GetText(7118, "Registrera restkod"), "",
                    Feature.Economy_Preferences_VoucherSettings_MatchCodes, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string name = F["Name"];
            string description = F["Description"];
            int matchCodeType;
            Int32.TryParse(F["MatchCodeType"], out matchCodeType);
            string customerNr = F["CustomerNr"];
            string accountNr = F["MatchCodeAccount"];
            int accountId = GetAccountId(F["MatchCodeAccount"]);
            string vatAccountNr = F["MatchCodeVatAccount"];
            int vatAccountId = GetAccountId(F["MatchCodeVatAccount"]);

            if (matchCodeType <= 0)
                RedirectToSelf("TYPE_MANDATORY", true);

            if (accountId <= 0)
                RedirectToSelf("ACCOUNT_MANDATORY", true);

            if (matchCode == null)
            {
                matchCode = new MatchCode()
                {
                    ActorCompanyId = SoeCompany.ActorCompanyId,
                    Name = name,
                    Description = description,
                    Type = matchCodeType,
                    AccountId = accountId,
                    AccountNr = accountNr,
                };

                if (vatAccountId != 0)
                {
                    matchCode.VatAccountId = vatAccountId;
                    matchCode.VatAccountNr = vatAccountNr;
                }

                if (im.AddMatchCode(matchCode).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                matchCode.Name = name;
                matchCode.Description = description;
                matchCode.Type = matchCodeType;
                matchCode.AccountId = accountId;
                matchCode.AccountNr = accountNr;
                matchCode.VatAccountId = vatAccountId;
                matchCode.VatAccountNr = vatAccountNr;

                if (im.UpdateMatchCode(matchCode).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }
        }

        protected override void Delete()
        {
            if (im.DeleteMatchCode(matchCode).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion

        #region HelpMethods

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

        #endregion
    }
}
