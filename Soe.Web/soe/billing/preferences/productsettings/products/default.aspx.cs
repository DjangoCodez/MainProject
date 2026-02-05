using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings.products
{
    public partial class _default : PageBase
    {
        #region Variables

        protected ProductManager pm;
        protected SettingManager sm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings_Products;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
            //Scripts.Add("texts.js.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new ProductManager(ParameterObject);
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

            #region Set data

            // Products
            SetProduct(CompanySettingType.ProductFreight, Freight);
            SetProduct(CompanySettingType.ProductInvoiceFee, InvoiceFee);
            SetProduct(CompanySettingType.ProductCentRounding, CentRounding);
            SetProduct(CompanySettingType.ProductHouseholdTaxDeduction, HouseholdTaxDeduction);
            SetProduct(CompanySettingType.ProductHouseholdTaxDeductionDenied, HouseholdTaxDeductionDenied);
            SetProduct(CompanySettingType.ProductHousehold50TaxDeduction, Household50TaxDeduction);
            SetProduct(CompanySettingType.ProductHousehold50TaxDeductionDenied, Household50TaxDeductionDenied);
            SetProduct(CompanySettingType.ProductRUTTaxDeduction, RUTTaxDeduction);
            SetProduct(CompanySettingType.ProductRUTTaxDeductionDenied, RUTTaxDeductionDenied);
            SetProduct(CompanySettingType.ProductGreen15TaxDeduction, Green15TaxDeduction);
            SetProduct(CompanySettingType.ProductGreen15TaxDeductionDenied, Green15TaxDeductionDenied);
            SetProduct(CompanySettingType.ProductGreen20TaxDeduction, Green20TaxDeduction);
            SetProduct(CompanySettingType.ProductGreen20TaxDeductionDenied, Green20TaxDeductionDenied);
            SetProduct(CompanySettingType.ProductGreen50TaxDeduction, Green50TaxDeduction);
            SetProduct(CompanySettingType.ProductGreen50TaxDeductionDenied, Green50TaxDeductionDenied);
            SetProduct(CompanySettingType.ProductFlatPrice, FlatPrice);
            SetProduct(CompanySettingType.ProductMisc, MiscProduct);
            SetProduct(CompanySettingType.ProductGuarantee, GuaranteeProduct);
            SetProduct(CompanySettingType.ProductReminderFee, ReminderFee);
            SetProduct(CompanySettingType.ProductInterestInvoicing, InterestInvoicing);
            SetProduct(CompanySettingType.ProductFlatPriceKeepPrices, FlatPriceKeepPrices);

            InvoiceExplanation.DefaultIdentifier = " ";
            InvoiceExplanation.DisableFieldset = true;

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

        private void SetProduct(CompanySettingType settingType, TextEntry text)
        {
            InvoiceProduct product = pm.GetInvoiceProductFromSetting(settingType, SoeCompany.ActorCompanyId);
            if (product != null)
            {
                text.Value = product.Number;
                text.InfoText = product.Name;
            }
        }

        protected override void Save()
        {
            bool success = true;

            #region Bool

            Dictionary<int, bool> boolValues = new Dictionary<int, bool>();

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            Dictionary<int, int> intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.ProductFreight, !String.IsNullOrEmpty(F["Freight"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Freight"]) : 0);
            intValues.Add((int)CompanySettingType.ProductInvoiceFee, !String.IsNullOrEmpty(F["InvoiceFee"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["InvoiceFee"]) : 0);
            intValues.Add((int)CompanySettingType.ProductCentRounding, !String.IsNullOrEmpty(F["CentRounding"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["CentRounding"]) : 0);
            intValues.Add((int)CompanySettingType.ProductHouseholdTaxDeduction, !String.IsNullOrEmpty(F["HouseholdTaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["HouseholdTaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductHouseholdTaxDeductionDenied, !String.IsNullOrEmpty(F["HouseholdTaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["HouseholdTaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductHousehold50TaxDeduction, !String.IsNullOrEmpty(F["Household50TaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Household50TaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductHousehold50TaxDeductionDenied, !String.IsNullOrEmpty(F["Household50TaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Household50TaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductRUTTaxDeduction, !String.IsNullOrEmpty(F["RUTTaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["RUTTaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductRUTTaxDeductionDenied, !String.IsNullOrEmpty(F["RUTTaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["RUTTaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen15TaxDeduction, !String.IsNullOrEmpty(F["Green15TaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green15TaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen15TaxDeductionDenied, !String.IsNullOrEmpty(F["Green15TaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green15TaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen20TaxDeduction, !String.IsNullOrEmpty(F["Green20TaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green20TaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen20TaxDeductionDenied, !String.IsNullOrEmpty(F["Green20TaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green20TaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen50TaxDeduction, !String.IsNullOrEmpty(F["Green50TaxDeduction"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green50TaxDeduction"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGreen50TaxDeductionDenied, !String.IsNullOrEmpty(F["Green50TaxDeductionDenied"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["Green50TaxDeductionDenied"]) : 0);
            intValues.Add((int)CompanySettingType.ProductFlatPrice, !String.IsNullOrEmpty(F["FlatPrice"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["FlatPrice"]) : 0);
            intValues.Add((int)CompanySettingType.ProductMisc, !String.IsNullOrEmpty(F["MiscProduct"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["MiscProduct"]) : 0);
            intValues.Add((int)CompanySettingType.ProductGuarantee, !String.IsNullOrEmpty(F["GuaranteeProduct"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["GuaranteeProduct"]) : 0);
            intValues.Add((int)CompanySettingType.ProductReminderFee, !String.IsNullOrEmpty(F["ReminderFee"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["ReminderFee"]) : 0);
            intValues.Add((int)CompanySettingType.ProductInterestInvoicing, !String.IsNullOrEmpty(F["InterestInvoicing"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["InterestInvoicing"]) : 0);
            intValues.Add((int)CompanySettingType.ProductFlatPriceKeepPrices, !String.IsNullOrEmpty(F["FlatPriceKeepPrices"]) ? pm.GetProductId(SoeCompany.ActorCompanyId, F["FlatPriceKeepPrices"]) : 0);

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion
    }
}
