using System;
using System.Linq;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.edisettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private EdiManager em;
        private SettingManager sm;
        private WholeSellerManager wsm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_EDISettings;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EdiManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            wsm = new WholeSellerManager(ParameterObject);

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

            CloseEdiEntryCondition.ConnectDataSource(GetGrpText(TermGroup.CloseEdiEntryCondition, addEmptyRow: true));

            EDIToOrderTransferRules.Labels = GetWholesellerDict();
            EDIToOrderTransferRules.DataSourceFrom = GetEdiMessageTypeDict();

            PriceSetting.ConnectDataSource(GetGrpText(TermGroup.EDIPriceSettingRule));

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> billingSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Billing, SoeCompany.ActorCompanyId);

            //Simple EDI to Order setting
            //EdiTransferToOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingEdiTransferToOrder, (int)SettingDataType.Boolean);                        

            //Advanced EDI to Order setting
            int pos = 0;
            var ediToOrderAdvancedSettings = em.GetEdiToOrderAdvancedSetting(SoeCompany.ActorCompanyId);
            foreach (var setting in ediToOrderAdvancedSettings.Take(this.EDIToOrderTransferRules.NoOfIntervals))
            {
                int ruleSysWholeSellerId = setting[0];
                int ruleMessageType = setting[1];

                this.EDIToOrderTransferRules.AddLabelValue(pos, ruleSysWholeSellerId.ToString());
                this.EDIToOrderTransferRules.AddValueFrom(pos, ruleMessageType.ToString());
                pos++;
            }

            //Transfer EDI to SupplierInvoice (Automatically transfer when read XML from FTP)
            EdiTransferToInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingEdiTransferToSupplierInvoice, (int)SettingDataType.Boolean);
            EdiTransferCreditInvToOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingEdiTransferCreditInvoiceToOrder, (int)SettingDataType.Boolean);
            BillingUseEDIPriceForSalesPriceRecalculation.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseEDIPriceForSalesPriceRecalculation, (int)SettingDataType.Boolean);

            //Condition for Close EdiEntry
            CloseEdiEntryCondition.Value = Convert.ToString(sm.GetIntSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCloseEdiEntryCondition, 0));

            //EDI PriceSetting rule
            PriceSetting.Value = Convert.ToString(sm.GetIntSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingEDIPriceSettingRule, 0));
            //SupplementCharge.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingEDISupplementCharge, UserId, SoeCompany.ActorCompanyId, 0);

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

        #region Action methods

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)CompanySettingType.BillingEdiTransferToSupplierInvoice, StringUtility.GetBool(F["EdiTransferToInvoice"]));
            boolValues.Add((int)CompanySettingType.BillingEdiTransferCreditInvoiceToOrder, StringUtility.GetBool(F["EdiTransferCreditInvToOrder"]));
            boolValues.Add((int)CompanySettingType.BillingUseEDIPriceForSalesPriceRecalculation, StringUtility.GetBool(F["BillingUseEDIPriceForSalesPriceRecalculation"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            //CloseEdiEntryCondition
            int closeCondition = 0;
            if (!String.IsNullOrEmpty(F["CloseEdiEntryCondition"]))
                closeCondition = Convert.ToInt32(F["CloseEdiEntryCondition"]);
            intValues.Add((int)CompanySettingType.BillingCloseEdiEntryCondition, closeCondition);

            //Edi pricesetting rule
            int ediPriceSettingRule = 0;
            if (!String.IsNullOrEmpty(F["PriceSetting"]))
                ediPriceSettingRule = Convert.ToInt32(F["PriceSetting"]);
            intValues.Add((int)CompanySettingType.BillingEDIPriceSettingRule, ediPriceSettingRule);

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region String

            var stringValues = new Dictionary<int, string>();

            //stringValues.Add((int)CompanySettingType.BillingEDISupplementCharge, F["SupplementCharge"]);
            stringValues.Add((int)CompanySettingType.BillingEdiTransferToOrderAdvanced, EdiManager.FormatEdiToOrderAdvancedSetting(this.EDIToOrderTransferRules.GetData(F).Where(i => i.LabelType != 0).Select(i => i.LabelType + ":" + i.From).JoinToString(",")));

            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        private SortedDictionary<int, string> GetWholesellerDict()
        {
            if (wsm == null)
                wsm = new WholeSellerManager(ParameterObject);

            var dict = wsm.GetWholesellersDictSorted(SoeCompany.ActorCompanyId, false);
            dict[Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_NONE] = " "; //None
            dict[Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_ALL] = GetText(9007, " Alla grossister");

            return dict;
        }

        private SortedDictionary<int, string> GetEdiMessageTypeDict()
        {
            var dict = GetGrpTextSorted(TermGroup.EdiMessageType, false, false);
            dict[Constants.EDI_TRANSFERTOORDER_TYPES_NONE] = " "; //None
            dict[Constants.EDI_TRANSFERTOORDER_TYPES_ALL] = GetText(9008, " Alla typer");

            return dict;
        }

        #endregion
    }
}