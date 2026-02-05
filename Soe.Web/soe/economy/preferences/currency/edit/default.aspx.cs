using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.soe.economy.preferences.currency.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private CountryCurrencyManager ccm;

        protected Currency currency;
        private int currencyId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_Currency_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("texts.js.aspx");
            Scripts.Add("currencyEdit.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            ccm = new CountryCurrencyManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            Int32.TryParse(QS["currencyId"], out int updateCurrencyId);
            Int32.TryParse(F["Code"], out int addSysCurrenctyId);

            bool isUpdateMode = updateCurrencyId > 0 && addSysCurrenctyId <= 0;

            if (updateCurrencyId > 0 || addSysCurrenctyId > 0)
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    currency = ccm.GetPrevNextCurrency(updateCurrencyId, SoeCompany.ActorCompanyId, Mode); //TODO: DON*T KNOW WHAT TO DO
                    ClearSoeFormObject();
                    Response.Redirect(Request.Url.AbsolutePath + "?currencyId=" + (currency != null ? currency.CurrencyId : updateCurrencyId));
                }
                else
                {
                    currency = isUpdateMode ? ccm.GetCurrencyAndRateById(updateCurrencyId, SoeCompany.ActorCompanyId)
                    : ccm.GetCurrencyAndRateBySysId(addSysCurrenctyId, SoeCompany.ActorCompanyId);

                    if (currency == null)
                    {
                        Form1.MessageWarning = GetText(3222, "Valuta hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }

                this.currencyId = isUpdateMode ? updateCurrencyId : addSysCurrenctyId;
            }

            //Mode
            string editModeTabHeaderText = GetText(3210, "Redigera valuta");
            string registerModeTabHeaderText = GetText(3209, "Registrera valuta");
            PostOptionalParameterCheck(Form1, currency, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = currency != null ? currency.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            Code.ConnectDataSource(GetCurrencies());
            IntervalType.ConnectDataSource(GetGrpText(TermGroup.CurrencyIntervalType, addEmptyRow: true));

            #endregion

            #region Set data

            if (currency != null)
            {
                Code.Value = currency.SysCurrencyId.ToString();
                Code.ReadOnly = true;
                IntervalType.Value = currency.IntervalType.ToString();

                if (currency.CurrencyRate != null)
                {
                    CurrencyRate currencyRate = currency.CurrencyRate.OrderByDescending(i => i.Date).FirstOrDefault();
                    if (currencyRate != null)
                    {
                        if (currencyRate.Date.HasValue)
                            RateDate.Value = currencyRate.Date.Value.ToShortDateString();

                        RateToBase.Value = currencyRate.RateToBase == null ? String.Empty : currencyRate.RateToBase.ToString();
                        RateFromBase.Value = currencyRate.RateFromBase == null ? String.Empty : currencyRate.RateFromBase.ToString();
                    }
                }

                //Accesible fields
                RateToBase.ReadOnly = currency.IntervalType != (int)TermGroup_CurrencyIntervalType.Manually;
                RateDate.ReadOnly = currency.IntervalType != (int)TermGroup_CurrencyIntervalType.Manually;
            }
            else
            {
                //Default value
                IntervalType.Value = Constants.CURRENCY_INTERVALTYPE_DEFAULT.ToString();

                //Accesible fields
                RateToBase.ReadOnly = Constants.CURRENCY_INTERVALTYPE_DEFAULT != (int)TermGroup_CurrencyIntervalType.Manually;
                RateDate.ReadOnly = Constants.CURRENCY_INTERVALTYPE_DEFAULT != (int)TermGroup_CurrencyIntervalType.Manually;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3211, "Valuta sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3212, "Valuta kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3213, "Valuta uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3214, "Valuta kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3215, "Valuta finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3216, "Valuta kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1982, "Valuta borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3217, "Valuta kunde inte tas bort");
                else if (MessageFromSelf == "MANDATORY_RATE")
                    Form1.MessageError = GetText(5722, "Valutakurser måste anges");
                else
                    Form1.MessageError = MessageFromSelf;
            }

            #endregion

            #region Navigation

            if (currency != null)
            {
                Form1.SetRegLink(GetText(3209, "Registrera valuta"), "",
                    Feature.Economy_Preferences_Currency_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            int intervalType = StringUtility.GetInt(F["IntervalType"], Constants.CURRENCY_INTERVALTYPE_DEFAULT);

            int useSysRate = 1;
            DateTime rateDate = DateTime.Today;
            decimal? rateToBase = null;
            decimal? rateFromBase = null;
            if (intervalType == (int)TermGroup_CurrencyIntervalType.Manually)
            {
                useSysRate = 0;
                if (!String.IsNullOrEmpty(F["RateDate"]))
                    rateDate = Convert.ToDateTime(F["RateDate"]);
                if (!String.IsNullOrEmpty(F["RateToBase"]))
                    rateToBase = NumberUtility.ToDecimal(F["RateToBase"],4);
                if (!String.IsNullOrEmpty(F["RateFromBase"]))
                    rateFromBase = NumberUtility.ToDecimal(F["RateFromBase"],4);

                if (!rateToBase.HasValue && !rateFromBase.HasValue)
                    RedirectToSelf("MANDATORY_RATE");
            }

            if (rateFromBase == null && rateToBase != null)
                rateFromBase = 1 / rateToBase;

            if (currency == null || currency.EntityKey == null)
            {
                #region Add

                currency = new Currency()
                {
                    SysCurrencyId = currencyId,
                    IntervalType = intervalType,
                    UseSysRate = useSysRate,
                };

                if (ccm.AddCurrency(currency, rateDate, SoeCompany.ActorCompanyId, rateToBase, rateFromBase).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);

                #endregion
            }
            else
            {
                #region Update

                currency.IntervalType = intervalType;
                currency.UseSysRate = useSysRate;

                if (ccm.UpdateCurrency(currency, rateDate, SoeCompany.ActorCompanyId, rateToBase, rateFromBase).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);

                #endregion
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (ccm.DeleteCurrency(currency).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion

        #region Help-methods

        private Dictionary<int, string> GetCurrencies()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            string selectedCode = "";

            if (currency != null)
            {
                //Add to dict
                dict.Add(currency.SysCurrencyId, currency.Description);

                //Keep selected
                selectedCode = currency.Code;
            }
            else
            {
                var compCurrencies = ccm.GetCompCurrencies(SoeCompany.ActorCompanyId, false);
                var sysCurrencies = ccm.GetSysCurrencies(true);
                foreach (var sysCurrency in sysCurrencies.OrderBy(i => i.SysCurrencyId))
                {
                    var compCurrency = compCurrencies.FirstOrDefault(i => i.SysCurrencyId == sysCurrency.SysCurrencyId);
                    if (compCurrency == null)
                    {
                        dict.Add(sysCurrency.SysCurrencyId, sysCurrency.Description);
                        if (String.IsNullOrEmpty(selectedCode))
                            selectedCode = sysCurrency.Code;
                    }
                }
            }

            var baseCurrency = ccm.GetCompanyBaseCurrency(SoeCompany.ActorCompanyId);
            if (baseCurrency != null)
            {
                RateToBase.InfoText = baseCurrency.Code + "/" + selectedCode;
                RateFromBase.InfoText = selectedCode + "/" + baseCurrency.Code;
            }

            return dict;
        }

        #endregion
    }
}
