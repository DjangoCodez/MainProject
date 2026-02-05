using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.economy.preferences.custinvoicesettings.paymentmethods.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected PaymentManager pm;
        protected AccountManager am;

        private PaymentMethod paymentMethod;
        private int paymentMethodId;
        private SoeOriginType paymentType = SoeOriginType.CustomerPayment;

        private int countryId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new PaymentManager(ParameterObject);
            am = new AccountManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            if (Int32.TryParse(QS["method"], out paymentMethodId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    paymentMethod = pm.GetPrevNextPaymentMethod(paymentMethodId, SoeCompany.ActorCompanyId, paymentType, Mode);
                    ClearSoeFormObject();
                    if (paymentMethod != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?method=" + paymentMethod.PaymentMethodId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?method=" + paymentMethodId);
                }
                else
                {
                    paymentMethod = pm.GetPaymentMethod(paymentMethodId, SoeCompany.ActorCompanyId, true);
                    if (paymentMethod == null)
                    {
                        Form1.MessageWarning = GetText(1848, "Inbetalningsmetod hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1849, "Redigera inbetalningsmetod");
            string registerModeTabHeaderText = GetText(1850, "Registrera inbetalningsmetod");
            PostOptionalParameterCheck(Form1, paymentMethod, true, editModeTabHeaderText, registerModeTabHeaderText);

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

            if (SoeCompany.SysCountryId == null)
            {
                // default to Swedish
                countryId = 1;
            }
            else
            {
                countryId = (int)SoeCompany.SysCountryId;
            }

            //PaymentInformation.ConnectDataSource(pm.GetPaymentInformationViewsDict(SoeCompany.ActorCompanyId, true));
            PaymentInformation.ConnectDataSource(pm.GetPaymentInformationViewsDict(SoeCompany.ActorCompanyId,  (TermGroup_Country)countryId, true));
            

            #endregion

            #region Set data

            Account.OnChange = "getAccountName('Account', '" + accountDimStd.AccountDimId + "')";

            if (paymentMethod != null)
            {
                Name.Value = paymentMethod.Name;
                PaymentInformation.Value = paymentMethod.PaymentInformationRow.PaymentInformationRowId.ToString();

                //Account
                if (paymentMethod.AccountStd != null && paymentMethod.AccountStd.Account != null && am.AccountExist(SoeCompany.ActorCompanyId, paymentMethod.AccountStd.AccountId, true))
                {
                    Account.Value = paymentMethod.AccountStd.Account.AccountNr;
                    Account.InfoText = paymentMethod.AccountStd.Account.Name;
                }
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(1852, "Inbetalningsmetod sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(1853, "Inbetalningsmetod kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(1854, "Inbetalningsmetod uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(1855, "Inbetalningsmetod kunde inte uppdateras");
                else if (MessageFromSelf == "PAYMENTNR_MANDATORY")
                    Form1.MessageWarning = GetText(1775, "Betalningsuppgifter måste anges");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1981, "Inbetalningsmetod borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1856, "Inbetalningsmetod kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (paymentMethod != null)
            {
                Form1.SetRegLink(GetText(1847, "Registrera inbetalningsmetod"), "",
                    Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string name = F["Name"];
            int paymentInformationRowId;
            Int32.TryParse(F["PaymentInformation"], out paymentInformationRowId);
            string accountNr = F["Account"];

            if (paymentInformationRowId == 0)
                RedirectToSelf("PAYMENTNR_MANDATORY", true);

            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod()
                {
                    Name = name,
                    SysPaymentMethodId = 0,
                };

                if (pm.AddPaymentMethodNoTrans(paymentMethod, paymentInformationRowId, accountNr, SoeCompany.ActorCompanyId, paymentType).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                paymentMethod.Name = name;

                if (pm.UpdatePaymentMethodNoTrans(paymentMethod, paymentInformationRowId, accountNr, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }
        }

        protected override void Delete()
        {
            if (pm.DeletePaymentMethod(paymentMethod, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
