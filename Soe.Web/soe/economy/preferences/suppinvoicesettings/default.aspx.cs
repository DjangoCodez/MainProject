using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am;
        private AttestManager atm;
        private CountryCurrencyManager ccm;
        private InventoryManager im;
        private SettingManager sm;
        private SequenceNumberManager snm;
        private ReportManager rm;
        private PaymentManager pm;
        private UserManager um;
        private FeatureManager fm;
        private InvoiceManager invm;
        private CommunicationManager com;
        bool IsTransferToVoucher = false;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_SuppInvoiceSettings;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            atm = new AttestManager(ParameterObject);
            ccm = new CountryCurrencyManager(ParameterObject);
            im = new InventoryManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            snm = new SequenceNumberManager(ParameterObject);
            rm = new ReportManager(ParameterObject);
            pm = new PaymentManager(ParameterObject);
            um = new UserManager(ParameterObject);
            fm = new FeatureManager(ParameterObject);
            invm = new InvoiceManager(ParameterObject);
            com = new CommunicationManager(ParameterObject);

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

            #region Registration

            var anyRoleHasTransferToPaymentPermission = fm.HasAnyRolePermission(Feature.Economy_Supplier_Invoice_Status_PayedToVoucher, Permission.Readonly, SoeCompany.ActorCompanyId, SoeCompany.LicenseId);

            DefaultVatType.ConnectDataSource(GetGrpText(TermGroup.InvoiceVatType, addEmptyRow: true));
            DefaultPaymentCondition.ConnectDataSource(pm.GetPaymentConditionsDict(SoeCompany.ActorCompanyId, true));
            DefaultPaymentMethod.ConnectDataSource(pm.GetPaymentMethodsDict(SoeOriginType.SupplierPayment, SoeCompany.ActorCompanyId, false));
            SettlePaymentMethod.ConnectDataSource(pm.GetPaymentMethodsDict(SoeOriginType.SupplierPayment, SoeCompany.ActorCompanyId, false));
            ObservationMethod.ConnectDataSource(GetGrpText(TermGroup.SupplierPaymentObservationMethod, addEmptyRow: true));
            BankCode.ConnectDataSource(GetGrpText(TermGroup.ForeignPaymentBankCode, addEmptyRow: true));

            #endregion

            #region AttestFlow

            Dictionary<int, string> attestOrigin = GetGrpText(TermGroup.OriginStatus, addEmptyRow: true);
            Dictionary<int, string> filterAttestOrigin = new Dictionary<int, string>();

            foreach (var attest in attestOrigin)
            {
                switch (attest.Key)
                {
                    case (int)SoeOriginStatus.Draft:
                        filterAttestOrigin.Add(attest.Key, attest.Value);
                        break;
                    case (int)SoeOriginStatus.Origin:
                        filterAttestOrigin.Add(attest.Key, attest.Value);
                        break;
                    case (int)SoeOriginStatus.Voucher:
                        filterAttestOrigin.Add(attest.Key, attest.Value);
                        break;
                    default:
                        break;
                }

            }

            SelectEntryAttest.ConnectDataSource(filterAttestOrigin);

            SelectAttestGroupSuggestionPrio1.ConnectDataSource(GetGrpText(TermGroup.AttestGroupSuggestionPrio, addEmptyRow: true));
            SelectAttestGroupSuggestionPrio2.ConnectDataSource(GetGrpText(TermGroup.AttestGroupSuggestionPrio, addEmptyRow: true));
            SelectAttestGroupSuggestionPrio3.ConnectDataSource(GetGrpText(TermGroup.AttestGroupSuggestionPrio, addEmptyRow: true));
            SelectAttestGroupSuggestionPrio4.ConnectDataSource(GetGrpText(TermGroup.AttestGroupSuggestionPrio, addEmptyRow: true));

            //hide dropdowns until this feature is ready to be released
            //SelectAttestGroupSuggestionPrio1.Visible = false;
            //SelectAttestGroupSuggestionPrio2.Visible = false;
            //SelectAttestGroupSuggestionPrio3.Visible = false;
            //SelectAttestGroupSuggestionPrio4.Visible = false;

            #endregion

            #region Currency

            //CurrencySource
            Dictionary<int, string> grpUpdateIntervalTypes = new Dictionary<int, string>();
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(SoeCompany.ActorCompanyId);
            if (baseSysCurrencyId == (int)TermGroup_Currency.SEK)
            {
                grpUpdateIntervalTypes.Add(0, " ");
                grpUpdateIntervalTypes.Add((int)TermGroup_CurrencySource.Daily, TextService.GetText((int)TermGroup_CurrencySource.Daily, (int)TermGroup.CurrencySource));
                grpUpdateIntervalTypes.Add((int)TermGroup_CurrencySource.Tullverket, TextService.GetText((int)TermGroup_CurrencySource.Tullverket, (int)TermGroup.CurrencySource));
            }
            else if (baseSysCurrencyId == (int)TermGroup_Currency.EUR)
            {
                grpUpdateIntervalTypes.Add(0, " ");
                grpUpdateIntervalTypes.Add((int)TermGroup_CurrencySource.Daily, TextService.GetText((int)TermGroup_CurrencySource.Daily, (int)TermGroup.CurrencySource));
                grpUpdateIntervalTypes.Add((int)TermGroup_CurrencySource.Tullverket, TextService.GetText((int)TermGroup_CurrencySource.ECB, (int)TermGroup.CurrencySource));
            }
            CurrencySource.ConnectDataSource(grpUpdateIntervalTypes);

            //TODO: Remove when CurrencySource is needed
            CurrencySource.Visible = false;

            //CurrencyIntervalType
            Dictionary<int, string> grpCurrencyIntervalType = GetGrpText(TermGroup.CurrencyIntervalType, addEmptyRow: true);
            CurrencyIntervalType.ConnectDataSource(grpCurrencyIntervalType);

            #endregion

            #region Age distribution

            Dictionary<int, int> ageDistInterval = new Dictionary<int, int>();
            ageDistInterval.Add(3, 3);
            ageDistInterval.Add(4, 4);
            ageDistInterval.Add(5, 5);
            ageDistInterval.Add(6, 6);
            AgeDistNbrOfIntervals.ConnectDataSource(ageDistInterval);

            #endregion

            #region Liquidity planning

            /*Dictionary<int, int> liqPlanInterval = new Dictionary<int, int>();
            liqPlanInterval.Add(3, 3);
            liqPlanInterval.Add(4, 4);
            liqPlanInterval.Add(5, 5);
            liqPlanInterval.Add(6, 6);
            LiqPlanNbrOfIntervals.ConnectDataSource(liqPlanInterval);*/

            #endregion

            #region Export

            //Default balance report template
            DefaultSupplierBalanceList.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.SupplierBalanceList, addEmptyRow: true));
            DefaultPaymentSuggestionList.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.SupplierBalanceList, addEmptyRow: true));
            DefaultChecklistPayments.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.SupplierBalanceList, addEmptyRow: true));

            #endregion

            #region Inventory

            InventoryEditTriggerAccounts.LabelHeader = GetText(3503, "Avskrivningsmall");
            InventoryEditTriggerAccounts.Labels = im.GetInventoryWriteOffTemplatesDict(SoeCompany.ActorCompanyId);
            InventoryEditTriggerAccounts.LabelFrom = GetText(3504, "Konto");
            InventoryEditTriggerAccounts.DataSourceFrom = am.GetAccountStdsDict(SoeCompany.ActorCompanyId, true);

            #endregion

            #region Scanning

            ScanningReferenceTargetField.ConnectDataSource(GetGrpText(TermGroup.ScanningReferenceTargetField, addEmptyRow: true));
            ScanningCodeTargetField.ConnectDataSource(GetGrpText(TermGroup.ScanningCodeTargetField, addEmptyRow: true));

            #endregion

            #region Onward invoicing

            BatchOnwardInvoiceingOrderTemplate.ConnectDataSource(invm.GetInvoiceTemplatesDict(SoeCompany.ActorCompanyId, SoeOriginType.Order, SoeInvoiceType.CustomerInvoice, true));

            #endregion

            #region Message recipient Groups

            SupplierPaymentNotificationRecipientGroup.ConnectDataSource(com.GetMessageGroupsDict(SoeCompany.ActorCompanyId, SoeUser.UserId, addEmptyRow: true).ToDictionary(x => x.Id, x => x.Name));

            #endregion

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> supplierSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Supplier, SoeCompany.ActorCompanyId);

            #region Registration

            // Default draft (Draft default checked)
            DefaultDraft.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceDefaultDraft, (int)SettingDataType.Boolean);
            // Keep supplier when new invoice is created
            KeepSupplier.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceKeepSupplier, (int)SettingDataType.Boolean);
            // Allow edit origin (Invoice with status Origin can be edited)
            AllowEditOrigin.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAllowEditOrigin, (int)SettingDataType.Boolean);
            // Generate account distribution without asking
            AutomaticAccountDistribution.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAutomaticAccountDistribution, (int)SettingDataType.Boolean);
            // Transfer SupplierInvoice to voucher (Automatically transfer invoice to voucher when saved)
            InvoiceTransferToVoucher.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceTransferToVoucher, (int)SettingDataType.Boolean);

            Boolean.TryParse(InvoiceTransferToVoucher.Value, out IsTransferToVoucher);

            // Ask if voucher should be printed when transferring to voucer
            SupplierInvoiceAskPrintVoucherOnTransfer.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer, (int)SettingDataType.Boolean);
            // Transfer manual SupplierPayment to voucher (Automatically transfer payments when checked in grid)
            PaymentManualTransferToVoucher.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentManualTransferToVoucher, (int)SettingDataType.Boolean);
            // Ask if voucher should be printed when transferring payment to voucer
            SupplierPaymentAskPrintVoucherOnTransfer.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer, (int)SettingDataType.Boolean);
            // Close invoice when transferred to voucher
            CloseInvoicesWhenTransferredToVoucher.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher, (int)SettingDataType.Boolean);
            // Set default payment date as due date
            SetPaymentDefaultPayDateAsDueDate.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierSetPaymentDefaultPayDateAsDueDate, (int)SettingDataType.Boolean);
            // Use payment suggestions grid
            UsePayementSuggestions.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierUsePaymentSuggestions, (int)SettingDataType.Boolean);

            // FI OCR Check
            FICheckOCRValidity.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.FISupplierInvoiceOCRCheckReference, (int)SettingDataType.Boolean);

            // Default VAT type
            DefaultVatType.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceDefaultVatType, (int)SettingDataType.Integer);
            // Default payment condition
            DefaultPaymentCondition.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, (int)SettingDataType.Integer);
            // Default payment method
            DefaultPaymentMethod.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentDefaultPaymentMethod, (int)SettingDataType.Integer);
            // Default payment method
            SettlePaymentMethod.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentSettlePaymentMethod, (int)SettingDataType.Integer);
            // ServeillanceType and days
            ObservationMethod.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentObservationMethod, (int)SettingDataType.Integer);
            ObservationDays.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentObservationDays, (int)SettingDataType.Integer);
            BankCode.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentForeignBankCode, (int)SettingDataType.Integer);
            //Time discount
            UseTimeDiscount.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierUseTimeDiscount, (int)SettingDataType.Boolean);
            // Hide autogiro
            HideAutogiroInvoicesFromUnpaid.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierHideAutogiroInvoicesFromUnpaid, (int)SettingDataType.Boolean);
            // Aggregate payments in SEPA export file
            AggregatePaymentsInSEPAExportFile.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierAggregatePaymentsInSEPAExportFile, (int)SettingDataType.Boolean);
            //Allow changing of invoice's accountingrows even if voucher is made
            AllowEditAccountingRows.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAllowEditAccountingRows, (int)SettingDataType.Boolean);
            //Use internal accounts with balance sheet accounts
            UseInternalAccountsWithBalanceSheetAccounts.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts, (int)SettingDataType.Boolean);
            //Use quantity in supplier invoice accounting rows
            UseQuantityInSupplierInvoiceAccountingRows.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows, (int)SettingDataType.Boolean);

            RoundVatOnSupplerInvoice.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceRoundVAT, (int)SettingDataType.Boolean);
            GetInternalAccountsFromOrder.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder, (int)SettingDataType.Boolean);


            AutoTransferAutogiroInvoices.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroInvoicesToPayment, (int)SettingDataType.Boolean);
            AutoTransferAutogiroInvoices.Visible = anyRoleHasTransferToPaymentPermission;
            AutoTransferAutogiroPaymentsToVoucher.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher, (int)SettingDataType.Boolean);
            AutoTransferAutogiroPaymentsToVoucher.Visible = anyRoleHasTransferToPaymentPermission;

            UseAutoAccountDistributionOnVoucher.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceUseAutoAccountDistributionOnVoucher, (int)SettingDataType.Boolean);

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
            SeqNbrPerType.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceSeqNbrPerType, (int)SettingDataType.Boolean);
            SeqNbrPerTypeInstruction.DefaultIdentifier = " ";
            SeqNbrPerTypeInstruction.DisableFieldset = true;
            SeqNbrPerTypeInstruction.Instructions = new List<string>()
            {
                GetText(3138, "Ja = En serie för debetfakturor, en för kreditfakturor etc."),
                GetText(5208, "Nej = En gemensam serie för alla leverantörsfakturor."),
            };

            // Startnumber invoice
            SeqNbrStart.Value = sm.GetIntSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceSeqNbrStart, 1).ToString();
            SeqNbrStart.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoice"));
            // Startnumber invoice debit
            SeqNbrStartDebit.Value = sm.GetIntSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceSeqNbrStartDebit, 1).ToString();
            SeqNbrStartDebit.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceDebit"));
            // Startnumber invoice credit
            SeqNbrStartCredit.Value = sm.GetIntSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceSeqNbrStartCredit, 1).ToString();
            SeqNbrStartCredit.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceCredit"));
            // Startnumber invoice interest
            SeqNbrStartInterest.Value = sm.GetIntSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceSeqNbrStartInterest, 1).ToString();
            SeqNbrStartInterest.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceInterest"));

            #endregion

            #region Currency

            CurrencySource.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingCurrencySource, 0, SoeCompany.ActorCompanyId, 0).ToString();
            CurrencyIntervalType.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingCurrencyIntervalType, 0, SoeCompany.ActorCompanyId, 0).ToString();

            ShowTransactionCurrency.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierShowTransactionCurrency, (int)SettingDataType.Boolean);
            ShowEnterpriseCurrency.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierShowEnterpriseCurrency, (int)SettingDataType.Boolean);
            ShowLedgerCurrency.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierShowLedgerCurrency, (int)SettingDataType.Boolean);

            #endregion

            #region Scanning

            // Transfer scanning to invoice
            ScanningTransferToInvoice.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.ScanningTransferToSupplierInvoice, (int)SettingDataType.Boolean);

            // Close scanning when transfered to invoice
            ScanningCloseWhenTransferedToInvoice.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.ScanningCloseWhenTransferedToSupplierInvoice, (int)SettingDataType.Boolean);

            ScanningCalcDueDateFromSupplier.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.ScanningCalcDueDateFromSupplier, (int)SettingDataType.Boolean);
            // Target field for referencenumber
            ScanningReferenceTargetField.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.ScanningReferenceTargetField, (int)SettingDataType.Integer);

            // Target field for code separated with '#' in buyercontactpersonname / ErReference
            ScanningCodeTargetField.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.ScanningCodeTargetField, (int)SettingDataType.Integer);

            #endregion

            #region Merge ledger

            //Merge Invoice to VoucherHead
            InvoiceMergeVoucherOnVoucherDate.Value = Boolean.FalseString;
            int invoiceToVoucherHeadType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherHeadType, UserId, SoeCompany.ActorCompanyId, 0);
            if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate)
                InvoiceMergeVoucherOnVoucherDate.Value = Boolean.TrueString;

            //Merge Invoice to VoucherRow
            InvoiceMergeVoucherRowsOnAccount.Value = Boolean.FalseString;
            int invoiceToVoucherRowType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherRowType, UserId, SoeCompany.ActorCompanyId, 0);
            if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount)
                InvoiceMergeVoucherRowsOnAccount.Value = Boolean.TrueString;

            //Merge Payment to VoucherHead
            PaymentMergeVoucherOnVoucherDate.Value = Boolean.FalseString;
            int paymentToVoucherHeadType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentToVoucherHeadType, UserId, SoeCompany.ActorCompanyId, 0);
            if (paymentToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate)
                PaymentMergeVoucherOnVoucherDate.Value = Boolean.TrueString;

            //Merge Payment to VoucherRow
            PaymentMergeVoucherRowsOnAccount.Value = Boolean.FalseString;
            int paymentToVoucherRowType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentToVoucherRowType, UserId, SoeCompany.ActorCompanyId, 0);
            if (paymentToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount)
                PaymentMergeVoucherRowsOnAccount.Value = Boolean.TrueString;

            #endregion

            #region Age distribution

            AgeDistNbrOfIntervals.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionNbrOfIntervals, (int)SettingDataType.Integer);
            AgeDistInterval1.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionInterval1, (int)SettingDataType.Integer);
            AgeDistInterval2.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionInterval2, (int)SettingDataType.Integer);
            AgeDistInterval3.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionInterval3, (int)SettingDataType.Integer);
            AgeDistInterval4.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionInterval4, (int)SettingDataType.Integer);
            AgeDistInterval5.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAgeDistributionInterval5, (int)SettingDataType.Integer);

            #endregion

            #region Liquidity planning

            /*LiqPlanNbrOfIntervals.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningNbrOfIntervals, (int)SettingDataType.Integer);
            LiqPlanInterval1.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval1, (int)SettingDataType.Integer);
            LiqPlanInterval2.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval2, (int)SettingDataType.Integer);
            LiqPlanInterval3.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval3, (int)SettingDataType.Integer);
            LiqPlanInterval4.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval4, (int)SettingDataType.Integer);
            LiqPlanInterval5.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval5, (int)SettingDataType.Integer);*/

            #endregion

            #region Finvoice

            // Finvoice - Only import for current company
            FinvoiceImportOnlyForCompany.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.FinvoiceImportOnlyForCompany, (int)SettingDataType.Boolean);

            // Finvoice - Use transfer to order
            FinvoiceUseTransferToOrder.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.FinvoiceUseTransferToOrder, (int)SettingDataType.Boolean);

            // Transfer finvoice to invoice (Automatically transfer when read imported Finvoice XML)
            FinvoiceTransferToInvoice.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.FinvoiceTransferToSupplierInvoice, (int)SettingDataType.Boolean);

            #endregion

            #region Product rows

            ProductRowsImport.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceProductRowsImport, (int)SettingDataType.Boolean);
            DetailedCodingRows.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceDetailedCodingRowsBasedOnProductRows, (int)SettingDataType.Boolean);

            #endregion

            #region Export

            // Interim
            /*AllowInterim.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAllowInterim, (int)SettingDataType.Boolean);
            int countryId;
            //SyscountryID 0,1 = Sweden , 3=Finland (FI) , 4 = Norge (NO) ... hide from Finnish users 
            int.TryParse(SoeCompany.SysCountryId.ToString(), out countryId);
            if (countryId == 3)
                InterimExpander.Attributes.Add("style", "visibility:hidden");*/

            //Default Supplier balance report
            DefaultSupplierBalanceList.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierDefaultBalanceList, (int)SettingDataType.Integer);

            //Default Payment Suggestion report
            DefaultPaymentSuggestionList.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierDefaultPaymentSuggestionList, (int)SettingDataType.Integer);

            //Default Checklist Payments report
            DefaultChecklistPayments.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierDefaultChecklistPayments, (int)SettingDataType.Integer);

            #endregion

            #region Inventory

            // Accounts for Inventory
            SetInventoryEditTriggerAccounts(sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, UserId, SoeCompany.ActorCompanyId, 0));

            #endregion

            #region Attestflow

            //Min invoice amount to require specified User
            AttestFlowMinAmount.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired, (int)SettingDataType.Integer);

            //UserId
            Dictionary<int, string> UsersDict = um.GetUsersByCompanyDict(SoeCompany.ActorCompanyId, this.RoleId, UserId, true, true, false, false);
            AttestFlowAmountUserId.ConnectDataSource(um.GetUsersByCompanyDict(SoeCompany.ActorCompanyId, this.RoleId, UserId, true, true, false, false));
            AttestFlowAmountUserId.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired, (int)SettingDataType.Integer);

            //Attest state to start accounts payable
            Dictionary<int, string> AttestStatesDict = atm.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, true, false);
            AttestFlowState.ConnectDataSource(atm.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, true, false));
            AttestFlowState.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow, (int)SettingDataType.Integer);

            var attestTransitionsDict = atm.GetAttestTransitionsDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, Common.Util.SoeModule.Economy, false);
            AttestFlowProjectLeader.ConnectDataSource(attestTransitionsDict);
            AttestFlowProjectLeader.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowProjectLeaderLevel, (int)SettingDataType.Integer);

            Dictionary<int, string> AttestWorkFlowTemplateDict = atm.GetAttestWorkFlowTemplateHeadDict(SoeCompany.ActorCompanyId, true, TermGroup_AttestEntity.SupplierInvoice);
            AttestFlowSelect.ConnectDataSource(atm.GetAttestWorkFlowTemplateHeadDict(SoeCompany.ActorCompanyId, true, TermGroup_AttestEntity.SupplierInvoice));
            AttestFlowSelect.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestTemplate, (int)SettingDataType.Integer);
            //AttestGroup
            Dictionary<int, string> AttestWorkFlowGroupDict = atm.GetAttestWorkFlowGroups(SoeCompany.ActorCompanyId, true);
            AttestGroupSelect.ConnectDataSource(atm.GetAttestWorkFlowGroups(SoeCompany.ActorCompanyId, true));
            AttestGroupSelect.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup, (int)SettingDataType.Integer);

            //Days before duedays, when grid row is set red
            DaysToWarnBeforeInvoiceIsDue.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestFlowDueDays, (int)SettingDataType.Integer);

            //Hide attested invoices
            ShowNonAttestedInvoices.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoicesShowNonAttestedInvoices, (int)SettingDataType.Boolean);

            // Show only attested invoices
            ShowOnlyAttestedInvoicesAtUnPayed.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoicesShowOnlyAttestedAtUnpayed, (int)SettingDataType.Boolean);

            // Create attest automatically from attest template registered on the supplier
            CreateAutoAttestFromSupplierOnEDI.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.CreateAutoAttestFromSupplierOnEDI, (int)SettingDataType.Boolean);

            //SaveSupplierInvoiceAsOrigin.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SaveSupplierInvoiceAsOrigin, (int)SettingDataType.Boolean);

            if (IsTransferToVoucher)
            {
                SelectEntryAttest.ReadOnly = true;
                SelectEntryAttest.Value = Convert.ToString((int)SoeOriginStatus.Voucher);
            }
            else
            {
                SelectEntryAttest.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SaveSupplierInvoiceAttestType, (int)SettingDataType.Integer);
            }

            // Transfer to voucher automatically on accepted attest 
            SupplierInvoiceTransferToVoucherOnAcceptedAttest.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceTransferToVoucherOnAcceptedAttest, (int)SettingDataType.Boolean);

            //Prios for automatic attestgroup suggestion
            SelectAttestGroupSuggestionPrio1.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio1, (int)SettingDataType.Integer);
            SelectAttestGroupSuggestionPrio2.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio2, (int)SettingDataType.Integer);
            SelectAttestGroupSuggestionPrio3.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio3, (int)SettingDataType.Integer);
            SelectAttestGroupSuggestionPrio4.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio4, (int)SettingDataType.Integer);

            #endregion

            #region Reports

            // Show pending payments in saldolistan(?)
            ShowPendingPaymentsInReport.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceReportShowPendingPayments, (int)SettingDataType.Boolean);

            #endregion

            #region Onward invoicing

            BatchOnwardInvoiceingOrderTemplate.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingOrderTemplate, (int)SettingDataType.Integer);
            BatchOnwardInvoiceingAttachImage.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingAttachImage, (int)SettingDataType.Boolean);

            #endregion

            #region Payment Notificaiton

            SupplierPaymentNotificationRecipientGroup.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierPaymentNotificationRecipientGroup, (int)SettingDataType.Integer, defaultValue: "0");

            #endregion

            #endregion

            #region MessageFromSelf

            if (!string.IsNullOrEmpty(MessageFromSelf))
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

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)CompanySettingType.SupplierInvoiceDefaultDraft, StringUtility.GetBool(F["DefaultDraft"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceKeepSupplier, StringUtility.GetBool(F["KeepSupplier"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAllowEditOrigin, StringUtility.GetBool(F["AllowEditOrigin"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceTransferToVoucher, StringUtility.GetBool(F["InvoiceTransferToVoucher"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer, StringUtility.GetBool(F["SupplierInvoiceAskPrintVoucherOnTransfer"]));
            boolValues.Add((int)CompanySettingType.SupplierPaymentManualTransferToVoucher, StringUtility.GetBool(F["PaymentManualTransferToVoucher"]));
            boolValues.Add((int)CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer, StringUtility.GetBool(F["SupplierPaymentAskPrintVoucherOnTransfer"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceSeqNbrPerType, StringUtility.GetBool(F["SeqNbrPerType"]));
            //boolValues.Add((int)CompanySettingType.SupplierInvoiceAllowInterim, StringUtility.GetBool(F["AllowInterim"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAutomaticAccountDistribution, StringUtility.GetBool(F["AutomaticAccountDistribution"]));
            boolValues.Add((int)CompanySettingType.ScanningTransferToSupplierInvoice, StringUtility.GetBool(F["ScanningTransferToInvoice"]));
            boolValues.Add((int)CompanySettingType.ScanningCloseWhenTransferedToSupplierInvoice, StringUtility.GetBool(F["ScanningCloseWhenTransferedToInvoice"]));
            boolValues.Add((int)CompanySettingType.ScanningCalcDueDateFromSupplier, StringUtility.GetBool(F["ScanningCalcDueDateFromSupplier"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceRoundVAT, StringUtility.GetBool(F["RoundVatOnSupplerInvoice"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder, StringUtility.GetBool(F["GetInternalAccountsFromOrder"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroInvoicesToPayment, StringUtility.GetBool(F["AutoTransferAutogiroInvoices"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher, StringUtility.GetBool(F["AutoTransferAutogiroPaymentsToVoucher"]));

            boolValues.Add((int)CompanySettingType.FinvoiceImportOnlyForCompany, StringUtility.GetBool(F["FinvoiceImportOnlyForCompany"]));
            boolValues.Add((int)CompanySettingType.FinvoiceUseTransferToOrder, StringUtility.GetBool(F["FinvoiceUseTransferToOrder"]));
            boolValues.Add((int)CompanySettingType.FinvoiceTransferToSupplierInvoice, StringUtility.GetBool(F["FinvoiceTransferToInvoice"]));
            boolValues.Add((int)CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher, StringUtility.GetBool(F["CloseInvoicesWhenTransferredToVoucher"]));
            boolValues.Add((int)CompanySettingType.SupplierSetPaymentDefaultPayDateAsDueDate, StringUtility.GetBool(F["SetPaymentDefaultPayDateAsDueDate"]));
            boolValues.Add((int)CompanySettingType.SupplierUsePaymentSuggestions, StringUtility.GetBool(F["UsePayementSuggestions"]));
            boolValues.Add((int)CompanySettingType.FISupplierInvoiceOCRCheckReference, StringUtility.GetBool(F["FICheckOCRValidity"]));
            boolValues.Add((int)CompanySettingType.SupplierUseTimeDiscount, StringUtility.GetBool(F["UseTimeDiscount"]));
            boolValues.Add((int)CompanySettingType.SupplierHideAutogiroInvoicesFromUnpaid, StringUtility.GetBool(F["HideAutogiroInvoicesFromUnpaid"]));

            boolValues.Add((int)CompanySettingType.SupplierInvoiceProductRowsImport, StringUtility.GetBool(F["ProductRowsImport"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceDetailedCodingRowsBasedOnProductRows, StringUtility.GetBool(F["DetailedCodingRows"]));

            boolValues.Add((int)CompanySettingType.SupplierShowTransactionCurrency, StringUtility.GetBool(F["ShowTransactionCurrency"]));
            boolValues.Add((int)CompanySettingType.SupplierShowEnterpriseCurrency, StringUtility.GetBool(F["ShowEnterpriseCurrency"]));
            boolValues.Add((int)CompanySettingType.SupplierShowLedgerCurrency, StringUtility.GetBool(F["ShowLedgerCurrency"]));

            boolValues.Add((int)CompanySettingType.SupplierInvoicesShowNonAttestedInvoices, StringUtility.GetBool(F["ShowNonAttestedInvoices"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoicesShowOnlyAttestedAtUnpayed, StringUtility.GetBool(F["ShowOnlyAttestedInvoicesAtUnPayed"]));
            boolValues.Add((int)CompanySettingType.CreateAutoAttestFromSupplierOnEDI, StringUtility.GetBool(F["CreateAutoAttestFromSupplierOnEDI"]));
            //boolValues.Add((int)CompanySettingType.SaveSupplierInvoiceAsOrigin, StringUtility.GetBool(F["SaveSupplierInvoiceAsOrigin"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceTransferToVoucherOnAcceptedAttest, StringUtility.GetBool(F["SupplierInvoiceTransferToVoucherOnAcceptedAttest"]));

            boolValues.Add((int)CompanySettingType.SupplierInvoiceReportShowPendingPayments, StringUtility.GetBool(F["ShowPendingPaymentsInReport"]));
            boolValues.Add((int)CompanySettingType.SupplierAggregatePaymentsInSEPAExportFile, StringUtility.GetBool(F["AggregatePaymentsInSEPAExportFile"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceAllowEditAccountingRows, StringUtility.GetBool(F["AllowEditAccountingRows"]));
            boolValues.Add((int)CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts, StringUtility.GetBool(F["UseInternalAccountsWithBalanceSheetAccounts"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows, StringUtility.GetBool(F["UseQuantityInSupplierInvoiceAccountingRows"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingAttachImage, StringUtility.GetBool(F["BatchOnwardInvoiceingAttachImage"]));
            boolValues.Add((int)CompanySettingType.SupplierInvoiceUseAutoAccountDistributionOnVoucher, StringUtility.GetBool(F["UseAutoAccountDistributionOnVoucher"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.SupplierInvoiceDefaultVatType, StringUtility.GetInt(F["DefaultVatType"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, StringUtility.GetInt(F["DefaultPaymentCondition"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentDefaultPaymentMethod, StringUtility.GetInt(F["DefaultPaymentMethod"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentSettlePaymentMethod, StringUtility.GetInt(F["SettlePaymentMethod"], 0));
            intValues.Add((int)CompanySettingType.SupplierDefaultBalanceList, StringUtility.GetInt(F["DefaultSupplierBalanceList"], 0));
            intValues.Add((int)CompanySettingType.SupplierDefaultPaymentSuggestionList, StringUtility.GetInt(F["DefaultPaymentSuggestionList"], 0));
            intValues.Add((int)CompanySettingType.SupplierDefaultChecklistPayments, StringUtility.GetInt(F["DefaultChecklistPayments"], 0));
            intValues.Add((int)CompanySettingType.AccountingCurrencySource, StringUtility.GetInt(F["CurrencySource"], 0));
            intValues.Add((int)CompanySettingType.AccountingCurrencyIntervalType, StringUtility.GetInt(F["CurrencyIntervalType"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentObservationMethod, StringUtility.GetInt(F["ObservationMethod"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentObservationDays, StringUtility.GetInt(F["ObservationDays"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentForeignBankCode, StringUtility.GetInt(F["BankCode"], 0));
            IsTransferToVoucher = StringUtility.GetBool(F["InvoiceTransferToVoucher"]);
            if (!IsTransferToVoucher)
            {
                if (StringUtility.GetInt(F["SelectEntryAttest"], 0) == 0)
                    intValues.Add((int)CompanySettingType.SaveSupplierInvoiceAttestType, StringUtility.GetInt(string.Format("{0}", (int)SoeOriginStatus.Voucher), 0));
                else
                    intValues.Add((int)CompanySettingType.SaveSupplierInvoiceAttestType, StringUtility.GetInt(F["SelectEntryAttest"], 0));
            }
            else
            {
                intValues.Add((int)CompanySettingType.SaveSupplierInvoiceAttestType, StringUtility.GetInt(string.Format("{0}", (int)SoeOriginStatus.Voucher), 0));
            }

            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio1, StringUtility.GetInt(F["SelectAttestGroupSuggestionPrio1"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio2, StringUtility.GetInt(F["SelectAttestGroupSuggestionPrio2"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio3, StringUtility.GetInt(F["SelectAttestGroupSuggestionPrio3"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestGroupSuggestionPrio4, StringUtility.GetInt(F["SelectAttestGroupSuggestionPrio4"], 0));

            // Sequence numbers. Check if start numbers has changed
            int initSeqNbrStart = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceSeqNbrStart, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartDebit = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceSeqNbrStartDebit, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartCredit = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceSeqNbrStartCredit, UserId, SoeCompany.ActorCompanyId, 0);
            int initSeqNbrStartInterest = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceSeqNbrStartInterest, UserId, SoeCompany.ActorCompanyId, 0);
            int newSeqNbrStart = StringUtility.GetInt(F["SeqNbrStart"], 1);
            int newSeqNbrStartDebit = StringUtility.GetInt(F["SeqNbrStartDebit"], 1);
            int newSeqNbrStartCredit = StringUtility.GetInt(F["SeqNbrStartCredit"], 1);
            int newSeqNbrStartInterest = StringUtility.GetInt(F["SeqNbrStartInterest"], 1);

            if (newSeqNbrStart != initSeqNbrStart)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoice");
            if (newSeqNbrStartDebit != initSeqNbrStartDebit)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceDebit");
            if (newSeqNbrStartCredit != initSeqNbrStartCredit)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceCredit");
            if (newSeqNbrStartInterest != initSeqNbrStartInterest)
                snm.DeleteSequenceNumber(SoeCompany.ActorCompanyId, "SupplierInvoiceInterest");

            intValues.Add((int)CompanySettingType.SupplierInvoiceSeqNbrStart, newSeqNbrStart);
            intValues.Add((int)CompanySettingType.SupplierInvoiceSeqNbrStartDebit, newSeqNbrStartDebit);
            intValues.Add((int)CompanySettingType.SupplierInvoiceSeqNbrStartCredit, newSeqNbrStartCredit);
            intValues.Add((int)CompanySettingType.SupplierInvoiceSeqNbrStartInterest, newSeqNbrStartInterest);

            //Merge Invoice to VoucherHead
            int invoiceToVoucherHeadType = StringUtility.GetBool(F["InvoiceMergeVoucherOnVoucherDate"]) ? (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate : (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;
            intValues.Add((int)CompanySettingType.SupplierInvoiceToVoucherHeadType, invoiceToVoucherHeadType);

            //Merge Invoice to VoucherRow
            int invoiceToVoucherRowType = StringUtility.GetBool(F["InvoiceMergeVoucherRowsOnAccount"]) ? (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount : (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
            intValues.Add((int)CompanySettingType.SupplierInvoiceToVoucherRowType, invoiceToVoucherRowType);

            //Merge Payment to VoucherHead
            int paymentToVoucherHeadType = StringUtility.GetBool(F["PaymentMergeVoucherOnVoucherDate"]) ? (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate : (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;
            intValues.Add((int)CompanySettingType.SupplierPaymentToVoucherHeadType, paymentToVoucherHeadType);

            //Merge Payment to VoucherRow
            int paymentToVoucherRowType = StringUtility.GetBool(F["PaymentMergeVoucherRowsOnAccount"]) ? (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount : (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
            intValues.Add((int)CompanySettingType.SupplierPaymentToVoucherRowType, paymentToVoucherRowType);

            //AttestFlow min amount to require userid
            int attestFlowInvoiceMinAmount = StringUtility.GetInt(F["AttestFlowMinAmount"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired, attestFlowInvoiceMinAmount);

            int AttestFlowUserId = StringUtility.GetInt(F["AttestFlowAmountUserId"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired, AttestFlowUserId);

            int AttestStateId = StringUtility.GetInt(F["AttestFlowState"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow, AttestStateId);

            int AttestFlowProjectLeaderId = StringUtility.GetInt(F["AttestFlowProjectLeader"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowProjectLeaderLevel, AttestFlowProjectLeaderId);

            int AttestWorkFlowTemplateHeadId = StringUtility.GetInt(F["AttestFlowSelect"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestTemplate, AttestWorkFlowTemplateHeadId);
            //AttestGroup
            int AttestWorkFlowGroupId = StringUtility.GetInt(F["AttestGroupSelect"], 0);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup, AttestWorkFlowGroupId);

            int AttestWorkFlowDueDays = StringUtility.GetInt(F["DaysToWarnBeforeInvoiceIsDue"], 1);
            intValues.Add((int)CompanySettingType.SupplierInvoiceAttestFlowDueDays, AttestWorkFlowDueDays);

            // Age distribution
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionNbrOfIntervals, StringUtility.GetInt(F["AgeDistNbrOfIntervals"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionInterval1, StringUtility.GetInt(F["AgeDistInterval1"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionInterval2, StringUtility.GetInt(F["AgeDistInterval2"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionInterval3, StringUtility.GetInt(F["AgeDistInterval3"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionInterval4, StringUtility.GetInt(F["AgeDistInterval4"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceAgeDistributionInterval5, StringUtility.GetInt(F["AgeDistInterval5"], 0));

            // Liquidity planning
            /*intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningNbrOfIntervals, StringUtility.GetInt(F["LiqPlanNbrOfIntervals"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval1, StringUtility.GetInt(F["LiqPlanInterval1"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval2, StringUtility.GetInt(F["LiqPlanInterval2"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval3, StringUtility.GetInt(F["LiqPlanInterval3"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval4, StringUtility.GetInt(F["LiqPlanInterval4"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceLiquidityPlanningInterval5, StringUtility.GetInt(F["LiqPlanInterval5"], 0));*/

            //Scanning reference target field
            int ScanningReferenceTarget = StringUtility.GetInt(F["ScanningReferenceTargetField"], 0);
            intValues.Add((int)CompanySettingType.ScanningReferenceTargetField, ScanningReferenceTarget);

            //Scanning code target field
            int ScanningCodeTarget = StringUtility.GetInt(F["ScanningCodeTargetField"], 0);
            intValues.Add((int)CompanySettingType.ScanningCodeTargetField, ScanningCodeTarget);

            // Batch onward invoicing
            int orderTemplate = StringUtility.GetInt(F["BatchOnwardInvoiceingOrderTemplate"], 0);
            intValues.Add((int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingOrderTemplate, orderTemplate);

            // Payment Notification reciepient group
            int recipientGroup = StringUtility.GetInt(F["SupplierPaymentNotificationRecipientGroup"], 0);
            intValues.Add((int)CompanySettingType.SupplierPaymentNotificationRecipientGroup, recipientGroup);

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region String

            var stringValues = new Dictionary<int, string>();

            stringValues.Add((int)CompanySettingType.InventoryEditTriggerAccounts, GetInventoryEditTriggerAccounts());

            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        private string GetInventoryEditTriggerAccounts()
        {
            Collection<FormIntervalEntryItem> formIntervalEntryItems = InventoryEditTriggerAccounts.GetData(F);
            if (formIntervalEntryItems == null || formIntervalEntryItems.Count == 0)
                return String.Empty;

            // Each account/method pair is comma separated
            // AccountId and MethodId is colon separated
            string accounts = String.Empty;
            int accountId = 0;
            int writeOffTemplateId = 0;
            foreach (FormIntervalEntryItem item in formIntervalEntryItems)
            {
                accountId = am.GetAccountStdIdFromAccountNr(item.From, SoeCompany.ActorCompanyId);
                writeOffTemplateId = item.LabelType;
                if (accountId != 0 && writeOffTemplateId != 0)
                {
                    if (accounts.Length > 0)
                        accounts += ",";

                    accounts += String.Format("{0}:{1}", accountId, writeOffTemplateId);
                }
            }

            return accounts;
        }

        private void SetInventoryEditTriggerAccounts(string accounts)
        {
            if (String.IsNullOrEmpty(accounts))
                return;

            string[] records = accounts.Split(',');
            if (records.Length > 0)
            {
                int pos = 0;
                foreach (var record in records)
                {
                    string[] valuePair = record.Split(':');

                    int accountId = 0;
                    Int32.TryParse(valuePair[0], out accountId);
                    if (accountId == 0)
                        continue;

                    AccountStd accountStd = am.GetAccountStd(SoeCompany.ActorCompanyId, accountId, true, false);
                    if (accountStd != null)
                    {
                        InventoryEditTriggerAccounts.AddLabelValue(pos, valuePair[1]);
                        InventoryEditTriggerAccounts.AddValueFrom(pos, accountStd.Account.AccountNr);

                        pos++;
                        if (pos == InventoryEditTriggerAccounts.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
