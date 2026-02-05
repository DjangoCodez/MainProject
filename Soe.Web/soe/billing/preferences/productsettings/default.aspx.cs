using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;
        protected ProductManager prm;
        protected TimeCodeManager tcm;
        protected StockManager stm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);
            prm = new ProductManager(ParameterObject);
            tcm = new TimeCodeManager(ParameterObject);
            stm = new StockManager(ParameterObject);

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

            // Default product type
            DefaultInvoiceProductVatType.ConnectDataSource(GetGrpText(TermGroup.InvoiceProductVatType, addEmptyRow: true));

            // Default product unit
            DefaultInvoiceProductUnit.ConnectDataSource(prm.GetProductUnitsDict(SoeCompany.ActorCompanyId, true));

            DefaultMaterialCode.ConnectDataSource(tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false, (int)SoeTimeCodeType.Material));

            DefaultStock.ConnectDataSource(stm.GetStocksDict(SoeCompany.ActorCompanyId, true));

            // Init gross margin calculation method/type
            DefaultGrossMarginCalculationType.ConnectDataSource(GetGrpText(TermGroup.GrossMarginCalculationType));

            // Init external product search
            InitProductSearch.ConnectDataSource(GetGrpText(TermGroup.InitProductSearch));

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> billingSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Billing, SoeCompany.ActorCompanyId);

            DefaultInvoiceProductVatType.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultInvoiceProductVatType, (int)SettingDataType.Integer);
            DefaultInvoiceProductUnit.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, (int)SettingDataType.Integer);
            DefaultMaterialCode.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStandardMaterialCode, (int)SettingDataType.Integer);
            DefaultStock.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultStock, (int)SettingDataType.Integer);
            InitProductSearch.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingInitProductSearch, (int)SettingDataType.Integer);
            DefaultGrossMarginCalculationType.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultGrossMarginCalculationType, (int)SettingDataType.Integer, TermGroup_GrossMarginCalculationType.PurchasePrice.ToString());

            UseProductUnitConvert.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseProductUnitConvert, (int)SettingDataType.Boolean);
            ManuallyUpdatedAvgPrices.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingManuallyUpdatedAvgPrices, (int)SettingDataType.Boolean);
            ProductRowDescriptionsUppercase.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingProductRowTextUppercase, (int)SettingDataType.Boolean);
            
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

        protected override void Save()
        {
            bool success = true;

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.BillingDefaultInvoiceProductVatType, StringUtility.GetInt(F["DefaultInvoiceProductVatType"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultInvoiceProductUnit, StringUtility.GetInt(F["DefaultInvoiceProductUnit"], 0));
            intValues.Add((int)CompanySettingType.BillingStandardMaterialCode, StringUtility.GetInt(F["DefaultMaterialCode"], 0));
            intValues.Add((int)CompanySettingType.BillingInitProductSearch, StringUtility.GetInt(F["InitProductSearch"], 1));
            intValues.Add((int)CompanySettingType.BillingDefaultStock, StringUtility.GetInt(F["DefaultStock"], 1));
            intValues.Add((int)CompanySettingType.BillingDefaultGrossMarginCalculationType, StringUtility.GetInt(F["DefaultGrossMarginCalculationType"], (int)TermGroup_GrossMarginCalculationType.PurchasePrice));

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)CompanySettingType.BillingUseProductUnitConvert, StringUtility.GetBool(F["UseProductUnitConvert"])); 
            boolValues.Add((int)CompanySettingType.BillingManuallyUpdatedAvgPrices, StringUtility.GetBool(F["ManuallyUpdatedAvgPrices"]));
            boolValues.Add((int)CompanySettingType.BillingProductRowTextUppercase, StringUtility.GetBool(F["ProductRowDescriptionsUppercase"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }
    }
}
