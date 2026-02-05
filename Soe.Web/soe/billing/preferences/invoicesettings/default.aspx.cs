using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe.billing.preferences.invoicesettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private AttestManager am;
        private AccountManager acm;
        private CustomerManager cm;
        private EmailManager em;
        private InvoiceManager im;
        private PaymentManager pm;
        private ProductManager prm;
        private ReportManager rptm;
        private SettingManager sm;
        private SequenceNumberManager snm;
        private WholeSellerManager wm;
        private ProductPricelistManager pplm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_InvoiceSettings;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AttestManager(ParameterObject);
            acm = new AccountManager(ParameterObject);
            cm = new CustomerManager(ParameterObject);
            em = new EmailManager(ParameterObject);
            im = new InvoiceManager(ParameterObject);
            pm = new PaymentManager(ParameterObject);
            prm = new ProductManager(ParameterObject);
            rptm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            snm = new SequenceNumberManager(ParameterObject);
            wm = new WholeSellerManager(ParameterObject);
            pplm = new ProductPricelistManager(ParameterObject);

        //Mandatory parameters

        //Mode
        PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            DivOfferSeqNbr.Visible = HasRolePermission(Feature.Billing_Offer, Permission.Modify);
            DivOrderSeqNbr.Visible = HasRolePermission(Feature.Billing_Order, Permission.Modify);
            DivEInvoice.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice, Permission.Modify) ||
                                  HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify) ||
                                  HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura, Permission.Modify) ||
                                  HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateIntruminvoice, Permission.Modify);

            FinvoiceSingleInvoicePerFile.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);
            FinvoiceInvoiceLabelToOrderIdentifier.Visible = HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, Permission.Modify);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            #region Registration

            DefaultVatType.ConnectDataSource(GetGrpText(TermGroup.InvoiceVatType, addEmptyRow: true));
            DefaultVatCode.ConnectDataSource(acm.GetVatCodesDict(SoeCompany.ActorCompanyId, true));
            DefaultPriceListType.ConnectDataSource(pplm.GetPriceListTypesDict(SoeCompany.ActorCompanyId, true));
            DefaultDeliveryType.ConnectDataSource(im.GetDeliveryTypesDict(SoeCompany.ActorCompanyId, true));
            DefaultDeliveryCondition.ConnectDataSource(im.GetDeliveryConditionsDict(SoeCompany.ActorCompanyId, true));
            DefaultPaymentCondition.ConnectDataSource(pm.GetPaymentConditionsDict(SoeCompany.ActorCompanyId, true));
            DefaultWholeSeller.ConnectDataSource(wm.GetWholesellersDictSorted(SoeCompany.ActorCompanyId, true));
            DefaultOneTimeCustomer.ConnectDataSource(cm.GetCustomersByCompanyDict(SoeCompany.ActorCompanyId, true, true, onlyOneTime: true));
            DefaultPaymentConditionHouseholdDeduction.ConnectDataSource(pm.GetPaymentConditionsDict(SoeCompany.ActorCompanyId, true));            

            #endregion

            #region Autosave

            var autoSaveDict = GetGrpText(TermGroup.AutoSaveInterval);
            AutoSaveOfferInterval.ConnectDataSource(autoSaveDict);
            AutoSaveOrderInterval.ConnectDataSource(autoSaveDict);
            AutoSaveContractInterval.ConnectDataSource(autoSaveDict);

            //Permissions
            AutoSaveOfferInterval.Visible = HasRolePermission(Feature.Billing_Offer_Offers_Edit, Permission.Modify);
            AutoSaveOrderInterval.Visible = HasRolePermission(Feature.Billing_Order_Orders_Edit, Permission.Modify);
            AutoSaveContractInterval.Visible = HasRolePermission(Feature.Billing_Contract_Contracts_Edit, Permission.Modify);

            #endregion

            #region Status

            //Offer
            var offerStatus = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.Offer, SoeModule.Billing, true, false);
            StatusForTransferOfferToOrder.ConnectDataSource(offerStatus);
            StatusForTransferOfferToInvoice.ConnectDataSource(offerStatus);

            //Order
            var orderStatus = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing, true, false);
            StatusForTransferOrderToInvoice.ConnectDataSource(orderStatus);
            StatusForTransferOrderToContract.ConnectDataSource(orderStatus);
            StatusOrderReadyMobile.ConnectDataSource(orderStatus);
            StatusOrderDeliverFromStock.ConnectDataSource(orderStatus);

            #endregion

            #region Export

            var emailTemplatesDict = em.GetEmailTemplatesDic(SoeCompany.ActorCompanyId, true);
            var emailTemplatesInvoicesDict = em.GetEmailTemplatesByTypeDict(SoeCompany.ActorCompanyId, (int)EmailTemplateType.Invoice, true);
            DefaultOfferTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingOffer, onlyOriginal: true, addEmptyRow: true));
            DefaultContractTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingContract, onlyOriginal: true, addEmptyRow: true));
            DefaultOrderTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingOrder, onlyOriginal: true, addEmptyRow: true));
            DefaultWorkingOrderTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingOrder, onlyOriginal: true, addEmptyRow: true));
            DefaultInvoiceTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingInvoice, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeProjectReportTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeProjectReport, onlyOriginal: true, addEmptyRow: true));
            DefaultEmailTemplate.ConnectDataSource(emailTemplatesDict);
            DefaultEmailTemplateOffer.ConnectDataSource(emailTemplatesInvoicesDict);
            DefaultEmailTemplateOrder.ConnectDataSource(emailTemplatesInvoicesDict);
            DefaultEmailTemplateContract.ConnectDataSource(emailTemplatesInvoicesDict);
            DefaultHouseholdDeductionTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.HousholdTaxDeduction, onlyOriginal: true, addEmptyRow: true));
            DefaultExpenseReportTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.ExpenseReport, onlyOriginal: true, addEmptyRow: true));
            DefaultPrintTemplateCashSales.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingInvoice, onlyOriginal: true, addEmptyRow: true));
            DefaultTemplatePurchaseOrder.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.PurchaseOrder, onlyOriginal: true, addEmptyRow: true));
            DefaultEmailTemplateCashSales.ConnectDataSource(emailTemplatesDict);
            DefaultEmailTemplatePurchase.ConnectDataSource(em.GetEmailTemplatesByTypeDict(SoeCompany.ActorCompanyId,(int) EmailTemplateType.PurchaseOrder, true));

            #endregion

            #region ProductRows

            MergeInvoiceRowsMerchandise.ConnectDataSource(GetGrpText(TermGroup.MergeInvoiceProductRows));
            MergeInvoiceRowsService.ConnectDataSource(GetGrpText(TermGroup.MergeInvoiceProductRows));
            DefaultHouseholdDeductionType.ConnectDataSource(prm.GetSysHouseholdTypeDict(true));

            #endregion

            #region EInvoice

            //useless until gets more distributors than InExchange
            //EInvoiceDistributor.ConnectDataSource(GetGrpText((int)TermGroup.EInvoiceDistributor, true, false));
            EInvoiceFormat.ConnectDataSource(GetGrpText(TermGroup.EInvoiceFormat, addEmptyRow: true));

            #endregion

            #region AccountingPrio

            Dictionary<int, string> invoiceAccountingPrioDict = GetGrpText(TermGroup.CompanyInvoiceProductAccountingPrio);
            InvoiceProductAccountingPrio1.ConnectDataSource(invoiceAccountingPrioDict);
            InvoiceProductAccountingPrio2.ConnectDataSource(invoiceAccountingPrioDict);
            InvoiceProductAccountingPrio3.ConnectDataSource(invoiceAccountingPrioDict);
            InvoiceProductAccountingPrio4.ConnectDataSource(invoiceAccountingPrioDict);
            InvoiceProductAccountingPrio5.ConnectDataSource(invoiceAccountingPrioDict);

            #endregion

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> billingSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Billing, SoeCompany.ActorCompanyId);
            Dictionary<int, object> customerSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Customer, SoeCompany.ActorCompanyId);

            #region Registration

            // Default VAT type
            DefaultVatType.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceDefaultVatType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            // Default VAT code
            DefaultVatCode.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            // Default pricelist type
            DefaultPriceListType.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultPriceListType, (int)SettingDataType.Integer);
            // Default delivery type
            DefaultDeliveryType.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultDeliveryType, (int)SettingDataType.Integer);
            // Default delivery condition
            DefaultDeliveryCondition.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultDeliveryCondition, (int)SettingDataType.Integer);
            // Default payment condition
            DefaultPaymentCondition.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            // Default Wholeseller
            DefaultWholeSeller.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultWholeseller, (int)SettingDataType.Integer);
            // Default One Time Customer
            DefaultOneTimeCustomer.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultOneTimeCustomer, (int)SettingDataType.Integer);
            // Default payment condition
            DefaultPaymentConditionHouseholdDeduction.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            // Our reference on invoice
            OurReference.Value = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceOurReference, UserId, SoeCompany.ActorCompanyId, 0);
            // Number of digits in invoice number - used in pgimport to find invoice
            InvoiceNumberLength.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingInvoiceNumberLength, (int)SettingDataType.Integer);
            // Copy invoice nr to OCR
            CopyInvoiceNrToOcr.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCopyInvoiceNrToOcr, (int)SettingDataType.Boolean);
            // Number of days an offer is valid - used to calculate end date of an offer
            OfferValidNoOfDays.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOfferValidNoOfDays, (int)SettingDataType.Integer);

            // Copy FI - Bank referens to OCR : Modification 12.3.2014 to use standard finnish bank reference
            FormFIReferenceToOcr.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingFormFIReferenceNumberToOCR, (int)SettingDataType.Boolean);
            // Copy RF -referens to OCR
            FormReferenceToOcr.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingFormReferenceNumberToOCR, (int)SettingDataType.Boolean);
            // Copy FI - Bank referens to OCR : Modification 12.3.2014 to use standard finnish bank reference
            FormFIReferenceToOcr.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingFormFIReferenceNumberToOCR, (int)SettingDataType.Boolean);
            // Use freight
            UseFreightAmount.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseFreightAmount, (int)SettingDataType.Boolean);
            // Use invoice fee
            UseInvoiceFee.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInvoiceFee, (int)SettingDataType.Boolean);
            // Use cent rounding
            UseCentRounding.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseCentRounding, (int)SettingDataType.Boolean);
            // Close invoice when transferred to voucher
            CloseInvoicesWhenTransferredToVoucher.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerCloseInvoicesWhenTransferredToVoucher, (int)SettingDataType.Boolean);
            // Hide wholeseller settings - hidden item 51290
            //HideWholesaleSettings.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideWholesaleSettings, (int)SettingDataType.Boolean);
            // Hide VAT warnings
            HideVatWarnings.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideVatWarnings, (int)SettingDataType.Boolean);
            // Use invoice fee limit
            UseInvoiceFeeLimit.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInvoiceFeeLimit, (int)SettingDataType.Boolean);
            // Invoice fee limit amount
            InvoiceFeeLimitAmount.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInvoiceFeeLimitAmount, (int)SettingDataType.Integer);
            // Cash sales in use (euro rounding when customer is cashcustomer)
            UseCashSales.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseCashSales, (int)SettingDataType.Boolean);
            //Show warning when rows have zero sum or negative margin
            ShowWarningOnZeroRows.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowZeroRowWarning, (int)SettingDataType.Boolean);
            // Mandatory checklist
            MandatoryChecklist.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingMandatoryChecklist, (int)SettingDataType.Boolean);
            // Print checklist with order
            PrintCheckListWithOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingPrintChecklistWithOrder, (int)SettingDataType.Boolean);
            //Automatically set owner in customer registration
            AutomaticCustomerOwner.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingAutomaticCustomerOwner, (int)SettingDataType.Boolean);
            //Hide the tax deduction in customer edit
            HideTaxDeductionContacts.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCustomerHideTaxDeductionContacts, (int)SettingDataType.Boolean);
            //Use the possibility to invoice a partial amount of a customer invoice row
            UsePartialInvoicingOnOrderRow.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUsePartialInvoicingOnOrderRow, (int)SettingDataType.Boolean);
            // Hide VAT rate
            HideVatRate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideVatRate, (int)SettingDataType.Boolean);
            //Use deliveryAddress as invoiceaddress
            bool UseDeliveryAdress = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseDeliveryAddressAsInvoiceAddress, UserId, SoeCompany.ActorCompanyId, 0);
            UseDeliveryAdressAsInvoiceAddress.Value = UseDeliveryAdress.ToString();

            AllowInvoiceOfCreditOrders.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.AllowInvoiceOfCreditOrders, (int)SettingDataType.Boolean);

            // Ask to print invoice when creating it from order
            AskOpenInvoiceWhenCreateInvoiceFromOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingAskOpenInvoiceWhenCreateInvoiceFromOrder, (int)SettingDataType.Boolean);

            // Set our reference on order identification row when co-invoiceing
            SetOurReferenceOnMergedInvoices.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingSetOurReferenceOnMergedInvoices, (int)SettingDataType.Boolean, "True");

            UseQuantityPrices.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseQuantityPrices, (int)SettingDataType.Boolean);

            UseExternalInvoiceNr.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseExternalInvoiceNr, (int)SettingDataType.Boolean);

            #endregion

            #region Sequence numbers

            // Warning
            SeqNbrStartWarning.DefaultIdentifier = " ";
            SeqNbrStartWarning.DisableFieldset = true;
            SeqNbrStartWarning.Instructions = new List<string>()
			{
				GetText(3505, "Varning! Ändring av startnummer kan innebära hål eller krockar i löpnummerserien."),
				GetText(3506, "Om ändring ska ske bör därför numret endast höjas för att undvika krockar."),
			};

            // Seqnr per invoice type
            SeqNbrPerType.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrPerType, (int)SettingDataType.Boolean);
            SeqNbrPerTypeInstruction.DefaultIdentifier = " ";
            SeqNbrPerTypeInstruction.DisableFieldset = true;
            SeqNbrPerTypeInstruction.Instructions = new List<string>()
			{
				GetText(3138, "Ja = En serie för debetfakturor, en för kreditfakturor etc."),
				GetText(3265, "Nej = En gemensam serie för alla kundfakturor."),
			};

            // Startnumber offer
            OfferSeqNbrStart.Value = sm.GetIntSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOfferSeqNbrStart, 1).ToString();
            OfferSeqNbrStart.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "Offer"));

            // Startnumber order
            UseOrderSeqNbrInternal.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderUseSeqNbrForInternal, (int)SettingDataType.Boolean);
            OrderSeqNbrStart.Value = sm.GetIntSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderSeqNbrStart, 1).ToString();
            OrderSeqNbrStart.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "Order"));

            OrderSeqNbrStartInternal.Value = sm.GetIntSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderSeqNbrStartInternal, 1).ToString();
            OrderSeqNbrStartInternal.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "OrderInternal"));
            

            // Startnumber invoice
            SeqNbrStart.Value = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrStart, 1).ToString();
            SeqNbrStart.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoice"));
            // Startnumber invoice debit
            SeqNbrStartDebit.Value = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrStartDebit, 1).ToString();
            SeqNbrStartDebit.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceDebit"));
            // Startnumber invoice credit
            SeqNbrStartCredit.Value = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrStartCredit, 1).ToString();
            SeqNbrStartCredit.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceCredit"));
            // Startnumber invoice interest
            SeqNbrStartInterest.Value = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrStartInterest, 1).ToString();
            SeqNbrStartInterest.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceInterest"));
            // Startnumber invoice Cash
            SegNbrStartCash.Value = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceSeqNbrStartCash, 1).ToString();
            SegNbrStartCash.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceCash"));
            #endregion

            #region Autosave

            // Autosave offer
            AutoSaveOfferInterval.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOfferAutoSaveInterval, (int)SettingDataType.Integer);
            // Autosave order
            AutoSaveOrderInterval.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderAutoSaveInterval, (int)SettingDataType.Integer);
            // Autosave contract
            AutoSaveContractInterval.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingContractAutoSaveInterval, (int)SettingDataType.Integer);

            #endregion

            #region Invoice

            // Invoice text
            InvoiceText.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingInvoiceText, (int)SettingDataType.String);

            #endregion

            #region ProductRows

            // Merge invoice product rows (merchandise)
            MergeInvoiceRowsMerchandise.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingMergeInvoiceProductRowsMerchandise, (int)SettingDataType.Integer).ToString();
            // Merge invoice product rows (service)
            MergeInvoiceRowsService.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingMergeInvoiceProductRowsService, (int)SettingDataType.Integer).ToString();

            DefaultHouseholdDeductionType.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, (int)SettingDataType.Integer).ToString();
            // Marginal income limit
            ProductRowMarginalLimit.Value = sm.GetDecimalSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingProductRowMarginalLimit, 0).ToString();
            // Ask for wholeseller
            OrderAskForWholeseller.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderAskForWholeseller, (int)SettingDataType.Boolean);
            // Ask for wholeseller
            InvoiceAskForWholeseller.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingInvoiceAskForWholeseller, (int)SettingDataType.Boolean);
            // Show only product number in productnumber field (by default number and nmae is shown)
            ProductShowOnlyProductNumber.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingInvoiceShowOnlyProductNumber, (int)SettingDataType.Boolean);
            // Use extended info when merging orders to invoice
            ExtendedInfoOnMergeOrders.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderMergeOrderWithExtendedInfo, (int)SettingDataType.Boolean);
            // Use additional discount
            UseAdditionalDiscount.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseAdditionalDiscount, (int)SettingDataType.Boolean);
            // Use customer categorie and productgroup discount
            UseCustomerCategorieAndArticleGroupsDiscount.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseProductGroupCustomerCategoryDiscount, (int)SettingDataType.Boolean);
            // Show picture link in productrows
            ProductShowProductPictureLink.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseExternalProductInfoLink, (int)SettingDataType.Boolean);
            // Auto set date on product rows on order
            AutoCreateDateOnProductRows.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingAutoCreateDateOnProductRows, (int)SettingDataType.Boolean);
            // Auto set date on product rows on order
            CalculateMarginalIncomeForRowsWithZeroPurchasePrice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCalculateMarginalIncomeForRowsWithZeroPurchasePrice, (int)SettingDataType.Boolean, "True");
            // Show product group and extended info in external search grid
            UseExtendedInfoInExternalSearch.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowExtendedInfoInExternalSearch, (int)SettingDataType.Boolean);
            // Show product group and extended info in external search grid
            CreateSubtotalRowInConsolidatedInvoices.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCreateSubtotalRowInConsolidatedInvoices, (int)SettingDataType.Boolean);
            // Show import product rows 
            ShowImportProductRows.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowImportProductRows, (int)SettingDataType.Boolean);

            #endregion

            #region Status

            // Status transfer offer-order
            StatusForTransferOfferToOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, (int)SettingDataType.Integer);
            // Status transfer offer-invoice
            StatusForTransferOfferToInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, (int)SettingDataType.Integer);
            // Status transfer order-invoice
            StatusForTransferOrderToInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, (int)SettingDataType.Integer);
            // Status transfer by mobile
            StatusOrderReadyMobile.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusOrderReadyMobile, (int)SettingDataType.Integer);
            // Status transfer order-contract
            StatusForTransferOrderToContract.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusTransferredOrderToContract, (int)SettingDataType.Integer);
            //
            StatusOrderDeliverFromStock.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingStatusOrderDeliverFromStock, (int)SettingDataType.Integer);
            // Hide Rows Transferred To Order Or Invoice From Offer
            HideRowsTransferredToOrderOrInvoiceFromOffer.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideRowsTransferredToOrderInvoiceFromOffer, (int)SettingDataType.Boolean);
            // Hide Rows Transferred To Invoice From Order
            HideRowsTransferredToInvoiceFromOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideRowsTransferredToInvoiceFromOrder, (int)SettingDataType.Boolean);
            // Hide orders ready for invoice in mobile
            HideStatusOrderReadyForMobile.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideStatusOrderReadyForMobile, (int)SettingDataType.Boolean);
            // Ask to create invoice when changing the order to ready
            AskCreateInvoiceWhenOrderReady.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingAskCreateInvoiceWhenOrderReady, (int)SettingDataType.Boolean);

            #endregion

            #region Export

            // Default offer report template
            DefaultOfferTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultOfferTemplate, (int)SettingDataType.Integer);
            // Default contract report template
            DefaultContractTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultContractTemplate, (int)SettingDataType.Integer);
            // Default order report template
            DefaultOrderTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultOrderTemplate, (int)SettingDataType.Integer);
            // Default order (whithout prices) report template
            DefaultWorkingOrderTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultWorkingOrderTemplate, (int)SettingDataType.Integer);
            // Default invoice report template
            DefaultInvoiceTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultInvoiceTemplate, (int)SettingDataType.Integer);
            // Default timeproject report template
            DefaultTimeProjectReportTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultTimeProjectReportTemplate, (int)SettingDataType.Integer);
            // Default E-mail template
            DefaultEmailTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultEmailTemplate, (int)SettingDataType.Integer);
            // Default E-mail template
            DefaultEmailTemplateOffer.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOfferDefaultEmailTemplate, (int)SettingDataType.Integer);
            // Default E-mail template
            DefaultEmailTemplateOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderDefaultEmailTemplate, (int)SettingDataType.Integer);
            // Default E-mail template
            DefaultEmailTemplateContract.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingContractDefaultEmailTemplate, (int)SettingDataType.Integer);
            // Default Household template
            DefaultHouseholdDeductionTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultHouseholdDeductionTemplate, (int)SettingDataType.Integer);
            // Default Rut template
            //DefaultRUTDeductionTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultRUTDeductionTemplate, (int)SettingDataType.Integer);
            // Default Expense template
            DefaultExpenseReportTemplate.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultExpenseReportTemplate, (int)SettingDataType.Integer);
            // DefaultPrintTemplateCashSales
            DefaultPrintTemplateCashSales.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultInvoiceTemplateCashSales, (int)SettingDataType.Integer);
            // DefaultEmailTemplateCashSales
            DefaultEmailTemplateCashSales.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultEmailTemplateCashSales, (int)SettingDataType.Integer);
            // Purchase
            DefaultTemplatePurchaseOrder.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultPurchaseOrderReportTemplate, (int)SettingDataType.Integer);
            // Use ReportDataHistory for BillingInvoice
            UseInvoiceReportDataHistory.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInvoiceReportDataHistory, (int)SettingDataType.Boolean);
            // DefaultEmailTemplatePurchase
            DefaultEmailTemplatePurchase.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingDefaultEmailTemplatePurchase, (int)SettingDataType.Integer);

            // Number of offercopies
            int nbrOfOfferCopies = 0;
            Int32.TryParse(sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingNbrOfOfferCopies, (int)SettingDataType.Integer), out nbrOfOfferCopies);
            NbrOfOfferCopies.Value = nbrOfOfferCopies.ToString();
            // Number of contractcopies
            int nbrOfContractCopies = 0;
            Int32.TryParse(sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingNbrOfContractCopies, (int)SettingDataType.Integer), out nbrOfContractCopies);
            NbrOfContractCopies.Value = nbrOfContractCopies.ToString();
            // Number of ordercopies
            int nbrOfOrderCopies = 0;
            Int32.TryParse(sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingNbrOfOrderCopies, (int)SettingDataType.Integer), out nbrOfOrderCopies);
            NbrOfOrderCopies.Value = nbrOfOrderCopies.ToString();
            // Number of invoicecopies
            int nbrOfInvoiceCopies = 0;
            Int32.TryParse(sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingNbrOfCopies, (int)SettingDataType.Integer), out nbrOfInvoiceCopies);
            NbrOfInvoiceCopies.Value = nbrOfInvoiceCopies.ToString();
            // Tax
            PrintTaxBill.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingPrintTaxBillText, (int)SettingDataType.Boolean);
            // OrderNr on Invoice
            ShowOrdernrOnInvoiceReport.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowOrdernrOnInvoiceReport, (int)SettingDataType.Boolean);
            // Use standardwholeseller pricelist
            //CCInvoiceMailToSelf.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCCInvoiceMailToSelf, (int)SettingDataType.Boolean);
            // Show C/O label
            ShowCOLabelOnReport.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowCOLabelOnReport, (int)SettingDataType.Boolean);
           
            // Finvoice single invoice per file
            FinvoiceSingleInvoicePerFile.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingFinvoiceSingleInvoicePerFile, (int)SettingDataType.Boolean);
            // Label in OrderReference
            FinvoiceInvoiceLabelToOrderIdentifier.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingFinvoiceInvoiceLabelToOrderIdentifier, (int)SettingDataType.Boolean);
            //Inclide working description as textrow on invoice
            IncludeWorkingDescriptionOnInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingIncludeWorkDescriptionOnInvoice, (int)SettingDataType.Boolean);
            // Include remaing amount in printed invoice
            IncludeRemainingAmountOnInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingIncludeRemainingAmountOnInvoice, (int)SettingDataType.Boolean);
            BillingShowStartStopInTimeReport.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowStartStopInTimeReport, (int)SettingDataType.Boolean);
            //IncludeTimeProject in XML for Order - REMOVED ITEM 49453
            //BillingOrderIncludeTimeProjectinReport.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingOrderIncludeTimeProjectinReport, (int)SettingDataType.Boolean);
            //IncludeTimeProject in XML for Invoice - REMOVED ITEM 49453
            //BillingIncludeTimeProjectinReport.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingIncludeTimeProjectinReport, (int)SettingDataType.Boolean);
            BillingShowPurchaseDateOnInvoice.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingShowPurchaseDateOnInvoice, (int)SettingDataType.Boolean);

            CCInvoiceMailAddress.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingCCInvoiceMailAddress, (int)SettingDataType.String);
            BCCInvoiceMailAddress.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingBCCInvoiceMailAddress, (int)SettingDataType.String);


            #endregion

            #region EInvoice
            //E-invoice distributor (useless until gets more distributors than InExchange)
            //EInvoiceDistributor.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceDistributor, UserId, SoeCompany.ActorCompanyId).ToString();

            //E-invoice format
            EInvoiceFormat.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            bool sveFakturaToFile = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToFile, UserId, SoeCompany.ActorCompanyId, 0);
            SveFakturaToFile.Value = sveFakturaToFile.ToString();

            bool sveFakturaToAPI = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, UserId, SoeCompany.ActorCompanyId, 0);
            SveFakturaToAPITestMode.Value = sveFakturaToAPI.ToString();

            bool sveFakturaSingleInvoicePerFie = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, UserId, SoeCompany.ActorCompanyId, 0);
            SveFakturaToAPITestMode.Value = sveFakturaToAPI.ToString();

            //Hide article number on Svefaktura
            BillingHideArticleNrOnSvefaktura.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingHideArticleNrOnSvefaktura, (int)SettingDataType.Boolean);
            BillingUseInvoiceDeliveryProvider.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInvoiceDeliveryProvider, (int)SettingDataType.Boolean);

            // Use InExchange as delivery provider for all invoices
            BillingUseInExchangeDeliveryProvider.Value = sm.GetSettingFromDict(billingSettingsDict, (int)CompanySettingType.BillingUseInExchangeDeliveryProvider, (int)SettingDataType.Boolean);

            // Information
            BillingUseInExchangeDeliveryProviderInstruction.DefaultIdentifier = " ";
            BillingUseInExchangeDeliveryProviderInstruction.DisableFieldset = true;
            BillingUseInExchangeDeliveryProviderInstruction.Instructions =
            [
                GetText(7776, "Klicka i checkbox om du vill att samtliga kundfakturor ska hanteras av InExchange. För att detta flöde ska fungera ska E-fakturaformat vara satt till API - Svefaktura."),
                GetText(7789, "Om checkbox är tom och e-fakturaformat API - Svefaktura är vald innebär det att endast fakturor med kunder som kan ta emot e-fakturor skickas till InExchange.")
            ];

            #endregion

            #region Accounting Prio

            string invoiceAccountingPrio = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.TimeCompanyInvoiceProductAccountingPrio, 0, SoeCompany.ActorCompanyId, 0);
            if (invoiceAccountingPrio != null)
            {
                string[] invoiceAccountingPrios = invoiceAccountingPrio.Split(',');
                if (invoiceAccountingPrios.Length > 0)
                    InvoiceProductAccountingPrio1.Value = invoiceAccountingPrios[0];
                if (invoiceAccountingPrios.Length > 1)
                    InvoiceProductAccountingPrio2.Value = invoiceAccountingPrios[1];
                if (invoiceAccountingPrios.Length > 2)
                    InvoiceProductAccountingPrio3.Value = invoiceAccountingPrios[2];
                if (invoiceAccountingPrios.Length > 3)
                    InvoiceProductAccountingPrio4.Value = invoiceAccountingPrios[3];
                if (invoiceAccountingPrios.Length > 4)
                    InvoiceProductAccountingPrio5.Value = invoiceAccountingPrios[4];
            }

            #endregion

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
                else if (MessageFromSelf == "INTEREST_ERROR")
                    Form1.MessageError = GetText(4091, "Ränta måste faktureras på ett sätt");
                else if (MessageFromSelf == "REMINDER_ERROR")
                    Form1.MessageError = GetText(1932, "Kravavgift måste faktureras på ett sätt");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)CompanySettingType.BillingCopyInvoiceNrToOcr, StringUtility.GetBool(F["CopyInvoiceNrToOcr"]));
            boolValues.Add((int)CompanySettingType.BillingFormReferenceNumberToOCR, StringUtility.GetBool(F["FormReferenceToOcr"]));
            boolValues.Add((int)CompanySettingType.BillingFormFIReferenceNumberToOCR, StringUtility.GetBool(F["FormFIReferenceToOcr"]));
            boolValues.Add((int)CompanySettingType.BillingUseFreightAmount, StringUtility.GetBool(F["UseFreightAmount"]));
            boolValues.Add((int)CompanySettingType.BillingUseInvoiceFee, StringUtility.GetBool(F["UseInvoiceFee"]));
            boolValues.Add((int)CompanySettingType.BillingUseCentRounding, StringUtility.GetBool(F["UseCentRounding"]));
            boolValues.Add((int)CompanySettingType.BillingUseInvoiceReportDataHistory, StringUtility.GetBool(F["UseInvoiceReportDataHistory"]));
            //boolValues.Add((int)CompanySettingType.BillingCCInvoiceMailToSelf, StringUtility.GetBool(F["CCInvoiceMailToSelf"]));
            boolValues.Add((int)CompanySettingType.BillingShowCOLabelOnReport, StringUtility.GetBool(F["ShowCOLabelOnReport"]));
            boolValues.Add((int)CompanySettingType.BillingPrintTaxBillText, StringUtility.GetBool(F["PrintTaxBill"]));
            boolValues.Add((int)CompanySettingType.BillingShowOrdernrOnInvoiceReport, StringUtility.GetBool(F["ShowOrdernrOnInvoiceReport"]));
            boolValues.Add((int)CompanySettingType.BillingInvoiceAskForWholeseller, StringUtility.GetBool(F["InvoiceAskForWholeseller"]));
            boolValues.Add((int)CompanySettingType.BillingOrderAskForWholeseller, StringUtility.GetBool(F["OrderAskForWholeseller"]));
            boolValues.Add((int)CompanySettingType.BillingHideRowsTransferredToOrderInvoiceFromOffer, StringUtility.GetBool(F["HideRowsTransferredToOrderOrInvoiceFromOffer"]));
            boolValues.Add((int)CompanySettingType.BillingHideRowsTransferredToInvoiceFromOrder, StringUtility.GetBool(F["HideRowsTransferredToInvoiceFromOrder"]));
            boolValues.Add((int)CompanySettingType.CustomerCloseInvoicesWhenTransferredToVoucher, StringUtility.GetBool(F["CloseInvoicesWhenTransferredToVoucher"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrPerType, StringUtility.GetBool(F["SeqNbrPerType"]));
            boolValues.Add((int)CompanySettingType.BillingOrderUseSeqNbrForInternal, StringUtility.GetBool(F["UseOrderSeqNbrInternal"]));
            //boolValues.Add((int)CompanySettingType.BillingHideWholesaleSettings, StringUtility.GetBool(F["HideWholesaleSettings"]));  - Hidden item 51290
            boolValues.Add((int)CompanySettingType.BillingHideVatWarnings, StringUtility.GetBool(F["HideVatWarnings"]));
            boolValues.Add((int)CompanySettingType.BillingUseInvoiceFeeLimit, StringUtility.GetBool(F["UseInvoiceFeeLimit"]));
            boolValues.Add((int)CompanySettingType.BillingUseCashSales, StringUtility.GetBool(F["UseCashSales"]));
            boolValues.Add((int)CompanySettingType.BillingShowZeroRowWarning, StringUtility.GetBool(F["ShowWarningOnZeroRows"]));
            boolValues.Add((int)CompanySettingType.BillingInvoiceShowOnlyProductNumber, StringUtility.GetBool(F["ProductShowOnlyProductNumber"]));
            boolValues.Add((int)CompanySettingType.BillingIncludeWorkDescriptionOnInvoice, StringUtility.GetBool(F["IncludeWorkingDescriptionOnInvoice"]));
            boolValues.Add((int)CompanySettingType.BillingOrderMergeOrderWithExtendedInfo, StringUtility.GetBool(F["ExtendedInfoOnMergeOrders"]));
            boolValues.Add((int)CompanySettingType.BillingUseAdditionalDiscount, StringUtility.GetBool(F["UseAdditionalDiscount"]));
            boolValues.Add((int)CompanySettingType.BillingUseProductGroupCustomerCategoryDiscount, StringUtility.GetBool(F["UseCustomerCategorieAndArticleGroupsDiscount"]));
            boolValues.Add((int)CompanySettingType.BillingUseExternalProductInfoLink, StringUtility.GetBool(F["ProductShowProductPictureLink"]));
            boolValues.Add((int)CompanySettingType.BillingAutoCreateDateOnProductRows, StringUtility.GetBool(F["AutoCreateDateOnProductRows"]));
            boolValues.Add((int)CompanySettingType.BillingShowExtendedInfoInExternalSearch, StringUtility.GetBool(F["UseExtendedInfoInExternalSearch"]));
            boolValues.Add((int)CompanySettingType.BillingCalculateMarginalIncomeForRowsWithZeroPurchasePrice, StringUtility.GetBool(F["CalculateMarginalIncomeForRowsWithZeroPurchasePrice"]));
            boolValues.Add((int)CompanySettingType.BillingMandatoryChecklist, StringUtility.GetBool(F["MandatoryChecklist"]));
            boolValues.Add((int)CompanySettingType.BillingPrintChecklistWithOrder, StringUtility.GetBool(F["PrintCheckListWithOrder"]));
            boolValues.Add((int)CompanySettingType.BillingAutomaticCustomerOwner, StringUtility.GetBool(F["AutomaticCustomerOwner"]));
            boolValues.Add((int)CompanySettingType.BillingCustomerHideTaxDeductionContacts, StringUtility.GetBool(F["HideTaxDeductionContacts"]));
            boolValues.Add((int)CompanySettingType.BillingUsePartialInvoicingOnOrderRow, StringUtility.GetBool(F["UsePartialInvoicingOnOrderRow"]));
            boolValues.Add((int)CompanySettingType.BillingIncludeRemainingAmountOnInvoice, StringUtility.GetBool(F["IncludeRemainingAmountOnInvoice"]));
            //boolValues.Add((int)CompanySettingType.BillingIncludeTimeProjectinReport, StringUtility.GetBool(F["BillingIncludeTimeProjectinReport"]));
            //boolValues.Add((int)CompanySettingType.BillingOrderIncludeTimeProjectinReport, StringUtility.GetBool(F["BillingOrderIncludeTimeProjectinReport"]));
            boolValues.Add((int)CompanySettingType.BillingShowStartStopInTimeReport, StringUtility.GetBool(F["BillingShowStartStopInTimeReport"]));
            boolValues.Add((int)CompanySettingType.BillingHideVatRate, StringUtility.GetBool(F["HideVatRate"]));
            boolValues.Add((int)CompanySettingType.BillingHideArticleNrOnSvefaktura, StringUtility.GetBool(F["BillingHideArticleNrOnSvefaktura"]));
            boolValues.Add((int)CompanySettingType.BillingUseInvoiceDeliveryProvider, StringUtility.GetBool(F["BillingUseInvoiceDeliveryProvider"]));
            boolValues.Add((int)CompanySettingType.BillingHideStatusOrderReadyForMobile, StringUtility.GetBool(F["HideStatusOrderReadyForMobile"]));
            boolValues.Add((int)CompanySettingType.BillingAskOpenInvoiceWhenCreateInvoiceFromOrder, StringUtility.GetBool(F["AskOpenInvoiceWhenCreateInvoiceFromOrder"]));
            boolValues.Add((int)CompanySettingType.BillingAskCreateInvoiceWhenOrderReady, StringUtility.GetBool(F["AskCreateInvoiceWhenOrderReady"]));
            boolValues.Add((int)CompanySettingType.UseDeliveryAddressAsInvoiceAddress, StringUtility.GetBool(F["UseDeliveryAdressAsInvoiceAddress"]));
            boolValues.Add((int)CompanySettingType.AllowInvoiceOfCreditOrders, StringUtility.GetBool(F["AllowInvoiceOfCreditOrders"]));
            boolValues.Add((int)CompanySettingType.BillingSveFakturaToFile, StringUtility.GetBool(F["SveFakturaToFile"]));
            boolValues.Add((int)CompanySettingType.BillingSveFakturaToAPITestMode, StringUtility.GetBool(F["SveFakturaToAPITestMode"]));
            boolValues.Add((int)CompanySettingType.BillingSetOurReferenceOnMergedInvoices, StringUtility.GetBool(F["SetOurReferenceOnMergedInvoices"]));
            boolValues.Add((int)CompanySettingType.BillingFinvoiceSingleInvoicePerFile, StringUtility.GetBool(F["FinvoiceSingleInvoicePerFile"]));
            boolValues.Add((int)CompanySettingType.BillingUseQuantityPrices, StringUtility.GetBool(F["UseQuantityPrices"]));
            boolValues.Add((int)CompanySettingType.BillingUseExternalInvoiceNr, StringUtility.GetBool(F["UseExternalInvoiceNr"]));
            boolValues.Add((int)CompanySettingType.BillingFinvoiceInvoiceLabelToOrderIdentifier, StringUtility.GetBool(F["FinvoiceInvoiceLabelToOrderIdentifier"]));
            boolValues.Add((int)CompanySettingType.BillingCreateSubtotalRowInConsolidatedInvoices, StringUtility.GetBool(F["CreateSubtotalRowInConsolidatedInvoices"]));
            boolValues.Add((int)CompanySettingType.BillingShowImportProductRows, StringUtility.GetBool(F["ShowImportProductRows"]));
            boolValues.Add((int)CompanySettingType.BillingUseInExchangeDeliveryProvider, StringUtility.GetBool(F["BillingUseInExchangeDeliveryProvider"]));
            boolValues.Add((int)CompanySettingType.BillingShowPurchaseDateOnInvoice, StringUtility.GetBool(F["BillingShowPurchaseDateOnInvoice"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Decimal

            var decimalValues = new Dictionary<int, decimal>();

            decimalValues.Add((int)CompanySettingType.BillingProductRowMarginalLimit, NumberUtility.ToDecimal(F["ProductRowMarginalLimit"], 0));

            if (!sm.UpdateInsertDecimalSettings(SettingMainType.Company, decimalValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.BillingInvoiceNumberLength, StringUtility.GetInt(F["InvoiceNumberLength"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultPriceListType, StringUtility.GetInt(F["DefaultPriceListType"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultDeliveryType, StringUtility.GetInt(F["DefaultDeliveryType"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultWholeseller, StringUtility.GetInt(F["DefaultWholeSeller"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultOneTimeCustomer, StringUtility.GetInt(F["DefaultOneTimeCustomer"], 0));
            intValues.Add((int)CompanySettingType.BillingOfferAutoSaveInterval, StringUtility.GetInt(F["AutoSaveOfferInterval"], 0));
            intValues.Add((int)CompanySettingType.BillingOrderAutoSaveInterval, StringUtility.GetInt(F["AutoSaveOrderInterval"], 0));
            intValues.Add((int)CompanySettingType.BillingContractAutoSaveInterval, StringUtility.GetInt(F["AutoSaveContractInterval"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultDeliveryCondition, StringUtility.GetInt(F["DefaultDeliveryCondition"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceDefaultVatType, StringUtility.GetInt(F["DefaultVatType"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultVatCode, StringUtility.GetInt(F["DefaultVatCode"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, StringUtility.GetInt(F["DefaultPaymentCondition"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, StringUtility.GetInt(F["DefaultPaymentConditionHouseholdDeduction"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultEmailTemplate, StringUtility.GetInt(F["DefaultEmailTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingOfferDefaultEmailTemplate, StringUtility.GetInt(F["DefaultEmailTemplateOffer"], 0));
            intValues.Add((int)CompanySettingType.BillingOrderDefaultEmailTemplate, StringUtility.GetInt(F["DefaultEmailTemplateOrder"], 0));
            intValues.Add((int)CompanySettingType.BillingContractDefaultEmailTemplate, StringUtility.GetInt(F["DefaultEmailTemplateContract"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultHouseholdDeductionTemplate, StringUtility.GetInt(F["DefaultHouseholdDeductionTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingMergeInvoiceProductRowsMerchandise, StringUtility.GetInt(F["MergeInvoiceRowsMerchandise"], 1));
            intValues.Add((int)CompanySettingType.BillingMergeInvoiceProductRowsService, StringUtility.GetInt(F["MergeInvoiceRowsService"], 1));
            intValues.Add((int)CompanySettingType.BillingDefaultOfferTemplate, StringUtility.GetInt(F["DefaultOfferTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultContractTemplate, StringUtility.GetInt(F["DefaultContractTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultOrderTemplate, StringUtility.GetInt(F["DefaultOrderTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultWorkingOrderTemplate, StringUtility.GetInt(F["DefaultWorkingOrderTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultInvoiceTemplate, StringUtility.GetInt(F["DefaultInvoiceTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultTimeProjectReportTemplate, StringUtility.GetInt(F["DefaultTimeProjectReportTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingNbrOfCopies, StringUtility.GetInt(F["nbrOfInvoiceCopies"], 0));
            intValues.Add((int)CompanySettingType.BillingNbrOfOrderCopies, StringUtility.GetInt(F["nbrOfOrderCopies"], 0));
            intValues.Add((int)CompanySettingType.BillingNbrOfOfferCopies, StringUtility.GetInt(F["nbrOfOfferCopies"], 0));
            intValues.Add((int)CompanySettingType.BillingNbrOfContractCopies, StringUtility.GetInt(F["nbrOfContractCopies"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusTransferredOfferToOrder, StringUtility.GetInt(F["StatusForTransferOfferToOrder"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusTransferredOfferToInvoice, StringUtility.GetInt(F["StatusForTransferOfferToInvoice"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusTransferredOrderToInvoice, StringUtility.GetInt(F["StatusForTransferOrderToInvoice"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusOrderReadyMobile, StringUtility.GetInt(F["StatusOrderReadyMobile"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusOrderDeliverFromStock, StringUtility.GetInt(F["StatusOrderDeliverFromStock"], 0));
            intValues.Add((int)CompanySettingType.BillingStatusTransferredOrderToContract, StringUtility.GetInt(F["StatusForTransferOrderToContract"], 0));
            intValues.Add((int)CompanySettingType.BillingUseInvoiceFeeLimitAmount, StringUtility.GetInt(F["InvoiceFeeLimitAmount"], 0));
            intValues.Add((int)CompanySettingType.BillingEInvoiceDistributor, StringUtility.GetInt(F["EInvoiceDistributor"], 0));
            intValues.Add((int)CompanySettingType.BillingEInvoiceFormat, StringUtility.GetInt(F["EInvoiceFormat"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultHouseholdDeductionType, StringUtility.GetInt(F["DefaultHouseholdDeductionType"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultExpenseReportTemplate, StringUtility.GetInt(F["DefaultExpenseReportTemplate"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultInvoiceTemplateCashSales, StringUtility.GetInt(F["DefaultPrintTemplateCashSales"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultEmailTemplateCashSales, StringUtility.GetInt(F["DefaultEmailTemplateCashSales"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultPurchaseOrderReportTemplate, StringUtility.GetInt(F["DefaultTemplatePurchaseOrder"], 0));
            intValues.Add((int)CompanySettingType.BillingDefaultEmailTemplatePurchase, StringUtility.GetInt(F["DefaultEmailTemplatePurchase"], 0));
            intValues.Add((int)CompanySettingType.BillingOfferValidNoOfDays, StringUtility.GetInt(F["OfferValidNoOfDays"], 0));

            // Offer sequence numbers. Check if start numbers has changed
            UpdateStartNumberChange(intValues, CompanySettingType.BillingOfferSeqNbrStart, "OfferSeqNbrStart", "Offer");

            // Order sequence numbers. Check if start numbers has changed
            UpdateStartNumberChange(intValues, CompanySettingType.BillingOrderSeqNbrStart, "OrderSeqNbrStart", "Order");
            UpdateStartNumberChange(intValues, CompanySettingType.BillingOrderSeqNbrStartInternal, "OrderSeqNbrStartInternal", "OrderInternal");

            // Invoice sequence numbers. Check if start numbers has changed
            int initSeqNbrStart = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceSeqNbrStart, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartDebit = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceSeqNbrStartDebit, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartCredit = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceSeqNbrStartCredit, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartInterest = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceSeqNbrStartInterest, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartCash = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceSeqNbrStartCash, UserId, SoeCompany.ActorCompanyId, 0);
            int newSeqNbrStart = StringUtility.GetInt(F["SeqNbrStart"], 1);
            int newSeqNbrStartDebit = StringUtility.GetInt(F["SeqNbrStartDebit"], 1);
            int newSeqNbrStartCredit = StringUtility.GetInt(F["SeqNbrStartCredit"], 1);
            int newSeqNbrStartInterest = StringUtility.GetInt(F["SeqNbrStartInterest"], 1);
            int newSeqNbrStartCash = StringUtility.GetInt(F["SegNbrStartCash"], 1);

            if (initSeqNbrStart != 0 && newSeqNbrStart != initSeqNbrStart)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoice");
            if (initSeqNbrStartDebit != 0 && newSeqNbrStartDebit != initSeqNbrStartDebit)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceDebit");
            if (initSeqNbrStartCredit != 0 && newSeqNbrStartCredit != initSeqNbrStartCredit)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceCredit");
            if (initSeqNbrStartInterest != 0 && newSeqNbrStartInterest != initSeqNbrStartInterest)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceInterest");
            if (initSeqNbrStartCash != 0 && newSeqNbrStartCash != initSeqNbrStartCash)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "CustomerInvoiceCash");

            intValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrStart, newSeqNbrStart);
            intValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrStartDebit, newSeqNbrStartDebit);
            intValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrStartCredit, newSeqNbrStartCredit);
            intValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrStartInterest, newSeqNbrStartInterest);
            intValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrStartCash, newSeqNbrStartCash);

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region String

            var stringValues = new Dictionary<int, string>();

            stringValues.Add((int)CompanySettingType.BillingCCInvoiceMailAddress, F["CCInvoiceMailAddress"]);
            stringValues.Add((int)CompanySettingType.BillingBCCInvoiceMailAddress, F["BCCInvoiceMailAddress"]);
            stringValues.Add((int)CompanySettingType.BillingInvoiceText, F["InvoiceText"]);
            stringValues.Add((int)CompanySettingType.CustomerInvoiceOurReference, F["OurReference"]);
            stringValues.Add((int)CompanySettingType.TimeCompanyInvoiceProductAccountingPrio, String.Format("{0},{1},{2},{3},{4}", F["InvoiceProductAccountingPrio1"], F["InvoiceProductAccountingPrio2"], F["InvoiceProductAccountingPrio3"], F["InvoiceProductAccountingPrio4"], F["InvoiceProductAccountingPrio5"]));

            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        private void UpdateStartNumberChange(Dictionary<int, int> intValues, CompanySettingType setting,string formFieldName, string sequenceName)
        {
            int existingNumber = sm.GetIntSetting(SettingMainType.Company, (int)setting, UserId, SoeCompany.ActorCompanyId, 0);
            int newNumber = StringUtility.GetInt(F[formFieldName], 1);
            if (existingNumber != 0 && newNumber != existingNumber)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, sequenceName);
            intValues.Add((int)setting, newNumber);
        }

        #endregion
    }
}
