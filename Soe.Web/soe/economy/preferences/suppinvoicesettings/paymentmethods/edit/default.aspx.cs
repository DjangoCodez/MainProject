using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings.paymentmethods.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected PaymentManager pm;
        protected AccountManager am;

        private PaymentMethod paymentMethod;
        private int paymentMethodId;
        private SoeOriginType paymentType = SoeOriginType.SupplierPayment;
        private int countryId;
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit;
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
                        Form1.MessageWarning = GetText(1778, "Betalningsmetod hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1789, "Redigera betalningsmetod");
            string registerModeTabHeaderText = GetText(1791, "Registrera betalningsmetod");
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

            SysPaymentMethod.ConnectDataSource(pm.GetSysPaymentMethodsDict(paymentType, (TermGroup_Country)countryId, true));
            PaymentInformation.ConnectDataSource(pm.GetPaymentInformationViewsDict(SoeCompany.ActorCompanyId, (TermGroup_Country)countryId, true));
            

            #endregion

            #region Set data

            Account.OnChange = "getAccountName('Account', '" + accountDimStd.AccountDimId + "')";

            if (paymentMethod != null)
            {
                Name.Value = paymentMethod.Name;
                SysPaymentMethod.Value = paymentMethod.SysPaymentMethodId.ToString();
                if (pm.IsExportPaymentType(paymentMethod.SysPaymentMethodId))
                {
                    PaymentInformation.Value = paymentMethod.PaymentInformationRow.PaymentInformationRowId.ToString();
                    CustomerNr.Value = paymentMethod.CustomerNr.HasValue() ? paymentMethod.CustomerNr.ToString(): string.Empty;
                }

                //Account
                if (paymentMethod.AccountStd != null && paymentMethod.AccountStd.Account != null)
                {
                    Account.Value = paymentMethod.AccountStd.Account.AccountNr;
                    Account.InfoText = paymentMethod.AccountStd.Account.Name;
                }
                
                //BankId
                if (paymentMethod.PaymentInformationRow.PayerBankId != null && countryId == 3)
                {
                    PayerBankId.Value = paymentMethod.PaymentInformationRow.PayerBankId;
                }
                else
                {
                    PayerBankId.Visible = false;
                }

            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(1779, "Betalningsmetod sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(1780, "Betalningsmetod kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(1781, "Betalningsmetod uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(1782, "Betalningsmetod kunde inte uppdateras");
                else if (MessageFromSelf == "SYSPAYMENTMETHOD_MANDATORY")
                    Form1.MessageWarning = GetText(1783, "Exporttyp måste anges");
                else if (MessageFromSelf == "PAYMENTNR_MANDATORY")
                    Form1.MessageWarning = GetText(1775, "Betalningsuppgifter måste anges");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1983, "Betalningsmetod borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1792, "Betalningsmetod kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (paymentMethod != null)
            {
                Form1.SetRegLink(GetText(1788, "Registrera betalningsmetod"), "",
                    Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string name = F["Name"];
            int sysPaymentMethodId;
            Int32.TryParse(F["SysPaymentMethod"], out sysPaymentMethodId);
            int paymentInformationRowId;
            Int32.TryParse(F["PaymentInformation"], out paymentInformationRowId);
            string customerNr = F["CustomerNr"];
            string accountNr = F["Account"];
            string payerBankId = F["PayerBankId"];
            if (payerBankId == string.Empty && countryId == 3)
                payerBankId = null;

            if (paymentInformationRowId == 0 && pm.IsExportPaymentType(sysPaymentMethodId))
                RedirectToSelf("PAYMENTNR_MANDATORY", true);

            if (sysPaymentMethodId <= 0)
                RedirectToSelf("SYSPAYMENTMETHOD_MANDATORY", true);

            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod()
                {
                    Name = name,
                    SysPaymentMethodId = sysPaymentMethodId,
                    CustomerNr = customerNr,
                };

                if (pm.AddPaymentMethodNoTrans(paymentMethod, paymentInformationRowId, accountNr, SoeCompany.ActorCompanyId, paymentType, payerBankId).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                paymentMethod.Name = name;
                paymentMethod.SysPaymentMethodId = sysPaymentMethodId;
                paymentMethod.CustomerNr = customerNr;

                if (pm.UpdatePaymentMethodNoTrans(paymentMethod, paymentInformationRowId, accountNr, SoeCompany.ActorCompanyId, payerBankId).Success)
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
