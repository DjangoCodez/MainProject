using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.preferences.custinvoicesettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private CountryCurrencyManager ccm;
        private PaymentManager pm;
        private ReportManager rptm;
        private SettingManager sm;
        private SequenceNumberManager snm;

        #endregion

        #region Constants

        private const int DefaultNrOfClaimLevels = 4;

        #endregion

        #region Properties

        private bool showAutomasterPreferences
        {
            get
            {
                bool showAutomasterPreferences = HasRolePermission(Feature.Economy_Import_Invoices_Automaster, Permission.Modify);               

                return showAutomasterPreferences;
            }
        }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_CustInvoiceSettings;
            base.Page_Init(sender, e);

            // Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            ccm = new CountryCurrencyManager(ParameterObject);
            pm = new PaymentManager(ParameterObject);
            rptm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            snm = new SequenceNumberManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            DivAutomaster.Visible = showAutomasterPreferences;

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            #region Registration
            var defaultPaymentConditions = pm.GetPaymentConditionsDict(SoeCompany.ActorCompanyId, true);
            // Default VAT type
            DefaultVatType.ConnectDataSource(GetGrpText(TermGroup.InvoiceVatType, addEmptyRow: true));
            // Default payment condition
            DefaultPaymentCondition.ConnectDataSource(defaultPaymentConditions);
            DefaultPaymentConditionClaimsAndInterest.ConnectDataSource(defaultPaymentConditions);
            DefaultPaymentMethod.ConnectDataSource(pm.GetPaymentMethodsDict(SoeOriginType.CustomerPayment, SoeCompany.ActorCompanyId));
            SettlePaymentMethod.ConnectDataSource(pm.GetPaymentMethodsDict(SoeOriginType.CustomerPayment, SoeCompany.ActorCompanyId, true));


            //NrOfClaims
            var nrOfClaims = new GenericType[] 
            { 
                new GenericType() { Id = 1, Name = "1" } ,
                new GenericType() { Id = 2, Name = "2" } ,
                new GenericType() { Id = 3, Name = "3" }, 
                new GenericType() { Id = 4, Name = "4" }, 
            };

            NrOfClaimLevels.ConnectDataSource(nrOfClaims, "Name", "Id");
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

            DefaultCustomerBalanceList.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.CustomerBalanceList, addEmptyRow: true));
            DefaultReminderTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingInvoiceReminder, addEmptyRow: true));
            DefaultInterestTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.BillingInvoiceInterest, addEmptyRow: true));
            DefaultInterestRateCalculationTemplate.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.InterestRateCalculation, addEmptyRow: true));

            #endregion

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> customerSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Customer, SoeCompany.ActorCompanyId);

            #region Registration

            // Apply quantities during invoice entry (Apply quantities when entering invoice)
            ApplyQuantitiesDuringInvoiceEntry.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceApplyQuantitiesDuringInvoiceEntry, (int)SettingDataType.Boolean);

            // Default draft (Draft default checked)
            DefaultDraft.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceDefaultDraft, (int)SettingDataType.Boolean);
            // Allow edit origin (Invoice with status Origin can be edited)
            AllowEditOrigin.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAllowEditOrigin, (int)SettingDataType.Boolean);
            // Generate account distribution without asking
            AutomaticAccountDistribution.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAutomaticAccountDistribution, (int)SettingDataType.Boolean);
            // Transfer CustmerInvoice to voucher (Automatically transfer invoice to voucher when saved)
            InvoiceTransferToVoucher.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceTransferToVoucher, (int)SettingDataType.Boolean);
            // Ask if voucher should be printed when transferring to voucer
            CustomerInvoiceAskPrintVoucherOnTransfer.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer, (int)SettingDataType.Boolean);
            // Transfer manual CustomerPayment to voucher (Automatically transfer payments when checked in grid)
            PaymentManualTransferToVoucher.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentManualTransferToVoucher, (int)SettingDataType.Boolean);
            // Ask if voucher should be printed when transferring payment to voucer
            CustomerPaymentAskPrintVoucherOnTransfer.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer, (int)SettingDataType.Boolean);
            // Close invoice when transferred to voucher
            CloseInvoicesWhenTransferredToVoucher.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerCloseInvoicesWhenTransferredToVoucher, (int)SettingDataType.Boolean);
            // Close invoice when transferred to voucher
            CloseInvoicesWhenExported.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerCloseInvoicesWhenExported, (int)SettingDataType.Boolean);
            // Default VAT type
            DefaultVatType.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceDefaultVatType, (int)SettingDataType.Integer);
            // Default payment condition
            DefaultPaymentCondition.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, (int)SettingDataType.Integer);
            // Default payment condition reminder/interest
            DefaultPaymentConditionClaimsAndInterest.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, (int)SettingDataType.Integer);
            // Default payment method
            DefaultPaymentMethod.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentDefaultPaymentMethod, (int)SettingDataType.Integer);
            SettlePaymentMethod.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentSettlePaymentMethod, (int)SettingDataType.Integer);
            //Default Credit limit
            int creditLimit = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultCreditLimit, 0);
            DefaultCreditLimit.Value = creditLimit == 0 ? string.Empty : creditLimit.ToString();
            // Our reference on invoice
            OurReference.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceOurReference, (int)SettingDataType.String);
            // ClientId at DnBNor Finans
            int dnBNorClientId = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceDnBNorClientId, 0);
            DnBNorClientId.Value = dnBNorClientId == 0 ? string.Empty : dnBNorClientId.ToString();
            // ClientId at DnBNor Finans
            UseDeliveryCustomerInvoicing.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceUseDeliveryCustomer, (int)SettingDataType.Boolean);
            // ClientId at BGC (Autogiro)
            int autogiroClientId = sm.GetIntSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAutogiroClientId, 0);
            AutogiroClientId.Value = autogiroClientId == 0 ? string.Empty : autogiroClientId.ToString();
            // Transfer payment service from customer only to contract
            TransferPaymentServiceOnlyToContract.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentServiceOnlyToContract, (int)SettingDataType.Boolean);
            //Allow changing of invoice's accountingrows even if voucher is made
            AllowEditAccountingRows.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAllowEditAccountingRows, (int)SettingDataType.Boolean);
            //Company has triangulation sales 
            TriangulationSales.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceTriangulationSales, (int)SettingDataType.Boolean);

            AddCustomerNameToPaymentInternaDescr.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerPaymentAddCustomerNameToInternaDescr, (int)SettingDataType.Boolean);
            
            UseAutoAccountDistributionOnVoucher.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceUseAutoAccountDistributionOnVoucher, (int)SettingDataType.Boolean);
            AllowChangesToInternalAccountsOnPaidCustomerInvoice.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.AllowChangesToInternalAccountsOnPaidCustomerInvoice, (int)SettingDataType.Boolean);


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

            AutomaticLedgerInvoiceNrWhenImport.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.AutomaticLedgerInvoiceNrWhenImport, (int)SettingDataType.Boolean);
            #endregion

            #region Currency

            CurrencySource.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingCurrencySource, 0, SoeCompany.ActorCompanyId, 0).ToString();
            CurrencyIntervalType.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingCurrencyIntervalType, 0, SoeCompany.ActorCompanyId, 0).ToString();

            ShowTransactionCurrency.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerShowTransactionCurrency, (int)SettingDataType.Boolean);
            ShowEnterpriseCurrency.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerShowEnterpriseCurrency, (int)SettingDataType.Boolean);
            ShowLedgerCurrency.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerShowLedgerCurrency, (int)SettingDataType.Boolean);

            #endregion

            #region Merge ledger

            //Merge Invoice to VoucherHead
            InvoiceMergeVoucherOnVoucherDate.Value = Boolean.FalseString;
            int invoiceToVoucherHeadType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceToVoucherHeadType, UserId, SoeCompany.ActorCompanyId, 0);
            if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate)
                InvoiceMergeVoucherOnVoucherDate.Value = Boolean.TrueString;

            //Merge Invoice to VoucherRow
            InvoiceMergeVoucherRowsOnAccount.Value = Boolean.FalseString;
            int invoiceToVoucherRowType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceToVoucherRowType, UserId, SoeCompany.ActorCompanyId, 0);
            if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount)
                InvoiceMergeVoucherRowsOnAccount.Value = Boolean.TrueString;

            //Merge Payment to VoucherHead
            PaymentMergeVoucherOnVoucherDate.Value = Boolean.FalseString;
            int PaymentToVoucherHeadType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentToVoucherHeadType, UserId, SoeCompany.ActorCompanyId, 0);
            if (PaymentToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate)
                PaymentMergeVoucherOnVoucherDate.Value = Boolean.TrueString;

            //Merge Payment to VoucherRow
            PaymentMergeVoucherRowsOnAccount.Value = Boolean.FalseString;
            int paymentToVoucherRowType = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentToVoucherRowType, UserId, SoeCompany.ActorCompanyId, 0);
            if (paymentToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount)
                PaymentMergeVoucherRowsOnAccount.Value = Boolean.TrueString;

            #endregion

            #region Automaster

            //Combine accounting rows when importing invoice accounting rows
            CombineAccountingRows.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceCombineImportedAccountingRows, (int)SettingDataType.Boolean);
            //Use automatic distribution when importing invoice accounting rows
            UseAutomaticDistribution.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceUseAutomaticDistributionInImport, (int)SettingDataType.Boolean);
            //Customer number which is used with cash invoices
            CashCustomerNumber.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceCashCustomerNumber, (int)SettingDataType.String);
            //List of transfer types which will close the invoice after creating voucher
            TransferTypesToBeClosed.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceTransferTypesToBeClosed, (int)SettingDataType.String);

            TransferTypeInstruction.DefaultIdentifier = " ";
            TransferTypeInstruction.DisableFieldset = true;
            TransferTypeInstruction.Instructions = new List<string>()
            {
                GetText(4871, "Fakturor med överföringstyp som definieras här, kommer att vara stängd efter överföring till verifikat"),
                GetText(4872, "Överföringstyper ges som en kommaseparerad lista, t.ex: AK,KK,VK,AS,KS,VS"),
            };

            #endregion

            #region Age distribution

            AgeDistNbrOfIntervals.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionNbrOfIntervals, (int)SettingDataType.Integer);
            AgeDistInterval1.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionInterval1, (int)SettingDataType.Integer);
            AgeDistInterval2.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionInterval2, (int)SettingDataType.Integer);
            AgeDistInterval3.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionInterval3, (int)SettingDataType.Integer);
            AgeDistInterval4.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionInterval4, (int)SettingDataType.Integer);
            AgeDistInterval5.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceAgeDistributionInterval5, (int)SettingDataType.Integer);

            #endregion

            #region Liquidity planning

            /*LiqPlanNbrOfIntervals.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningNbrOfIntervals, (int)SettingDataType.Integer);
            LiqPlanInterval1.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval1, (int)SettingDataType.Integer);
            LiqPlanInterval2.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval2, (int)SettingDataType.Integer);
            LiqPlanInterval3.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval3, (int)SettingDataType.Integer);
            LiqPlanInterval4.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval4, (int)SettingDataType.Integer);
            LiqPlanInterval5.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval5, (int)SettingDataType.Integer);*/

            #endregion

            #region Export

            DefaultCustomerBalanceList.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultBalanceList, (int)SettingDataType.Integer);
            DefaultReminderTemplate.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultReminderTemplate, (int)SettingDataType.Integer);
            DefaultInterestTemplate.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultInterestTemplate, (int)SettingDataType.Integer);
            DefaultInterestRateCalculationTemplate.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerDefaultInterestRateCalculationTemplate, (int)SettingDataType.Integer);

            #endregion

            #region Reminder/Interest

            // How to handle interest
            int interestHandlingType = 0;
            Int32.TryParse(sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInterestHandlingType, (int)SettingDataType.Integer), out interestHandlingType);
            InterestHandlingTypeNew.Value = interestHandlingType == (int)SoeInvoiceInterestHandlingType.CreateNewInvoice ? Boolean.TrueString : Boolean.FalseString;
            InterestHandlingTypeNext.Value = interestHandlingType == (int)SoeInvoiceInterestHandlingType.AddToNextInvoice ? Boolean.TrueString : Boolean.FalseString;

            // Interest percent
            InterestPercent.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInterestPercent, (int)SettingDataType.Decimal);
            // Accumulated interest before invoicing
            InterestAccumulatedBeforeInvoice.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerInterestAccumulatedBeforeInvoice, (int)SettingDataType.Integer);
            // Grace period in days
            GracePeriodDays.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerGracePeriodDays, (int)SettingDataType.Integer);

            bool reminderGenerateProductRow = false;
            Boolean.TryParse(sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerReminderGenerateProductRow, (int)SettingDataType.Boolean), out reminderGenerateProductRow);
            if (reminderGenerateProductRow)
            {
                ReminderGenerateProductRow.Value = Boolean.TrueString;

                // How to handle reminder
                int reminderHandlingType = 0;
                Int32.TryParse(sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerReminderHandlingType, (int)SettingDataType.Integer), out reminderHandlingType);
                ReminderHandlingTypeNew.Value = reminderHandlingType == (int)SoeInvoiceReminderHandlingType.CreateNewInvoice ? Boolean.TrueString : Boolean.FalseString;
                ReminderHandlingTypeNext.Value = reminderHandlingType == (int)SoeInvoiceReminderHandlingType.AddToNextInvoice ? Boolean.TrueString : Boolean.FalseString;
            }

            // Days until next claim letter
            MinNrOfDaysForNewClaim.Value = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerReminderMinNrOfDaysForNewClaim, (int)SettingDataType.Integer);

            //NrOfClaimLevels
            var nrOfClaimLevels = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerReminderNrOfClaimLevels, (int)SettingDataType.Integer);
            NrOfClaimLevels.Value = string.IsNullOrEmpty(nrOfClaimLevels) ? DefaultNrOfClaimLevels.ToString() : nrOfClaimLevels;

            //ClaimLevelText
            string claimText = "";
            claimText = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerClaimLevel1Text, (int)SettingDataType.String);
            ClaimLevelText1.Text = string.IsNullOrEmpty(claimText) ? GetText(4704, "Kära kund,\nVi har observerat att ni inte betalt nedanstående faktura/-or som är förfallna. Skulle det föreligga några oklarheter så vänligen kontakta oss. Vi emotser er betalning snarast.") : claimText;
            claimText = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerClaimLevel2Text, (int)SettingDataType.String);
            ClaimLevelText2.Text = string.IsNullOrEmpty(claimText) ? GetText(4705, "Hej,\nTrots tidigare påminnelser har vi ännu inte mottagit betalning för nedanstående fakturor. Om ni inte betalar inom 10 dagar kommer ärendet gå vidare till inkasso.") : claimText;
            ClaimLevelText3.Text = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerClaimLevel3Text, (int)SettingDataType.String);
            ClaimLevelText4.Text = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerClaimLevel4Text, (int)SettingDataType.String);
            ClaimLevelText5.Text = sm.GetSettingFromDict(customerSettingsDict, (int)CompanySettingType.CustomerClaimLevelDebtCollectionText, (int)SettingDataType.String);

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
                    Form1.MessageError = string.Format(GetText(9155, "Fel, du måste välja {0} eller {1}"), GetText(1931, "Faktureras separat som ny faktura"), GetText(1927, "Adderas till nästkommande faktura"));
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            //Reminder
            bool reminderGenerateProductRow = StringUtility.GetBool(F["ReminderGenerateProductRow"]);
            int reminderHandlingType = 0;
            if (reminderGenerateProductRow)
            {
                if (StringUtility.GetBool(F["ReminderHandlingTypeNew"]))
                    reminderHandlingType = (int)SoeInvoiceReminderHandlingType.CreateNewInvoice;
                else if (StringUtility.GetBool(F["ReminderHandlingTypeNext"]))
                    reminderHandlingType = (int)SoeInvoiceReminderHandlingType.AddToNextInvoice;

                if (reminderHandlingType == 0)
                    RedirectToSelf("REMINDER_ERROR", true);
            }
            boolValues.Add((int)CompanySettingType.CustomerInvoiceApplyQuantitiesDuringInvoiceEntry, StringUtility.GetBool(F["ApplyQuantitiesDuringInvoiceEntry"]));
            boolValues.Add((int)CompanySettingType.CustomerReminderGenerateProductRow, reminderGenerateProductRow);

            boolValues.Add((int)CompanySettingType.CustomerInvoiceDefaultDraft, StringUtility.GetBool(F["DefaultDraft"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceAllowEditOrigin, StringUtility.GetBool(F["AllowEditOrigin"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceTransferToVoucher, StringUtility.GetBool(F["InvoiceTransferToVoucher"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer, StringUtility.GetBool(F["CustomerInvoiceAskPrintVoucherOnTransfer"]));
            boolValues.Add((int)CompanySettingType.CustomerPaymentManualTransferToVoucher, StringUtility.GetBool(F["PaymentManualTransferToVoucher"]));
            boolValues.Add((int)CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer, StringUtility.GetBool(F["CustomerPaymentAskPrintVoucherOnTransfer"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceSeqNbrPerType, StringUtility.GetBool(F["SeqNbrPerType"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceAutomaticAccountDistribution, StringUtility.GetBool(F["AutomaticAccountDistribution"]));
            boolValues.Add((int)CompanySettingType.CustomerCloseInvoicesWhenTransferredToVoucher, StringUtility.GetBool(F["CloseInvoicesWhenTransferredToVoucher"]));
            boolValues.Add((int)CompanySettingType.CustomerCloseInvoicesWhenExported, StringUtility.GetBool(F["CloseInvoicesWhenExported"]));

            boolValues.Add((int)CompanySettingType.CustomerShowTransactionCurrency, StringUtility.GetBool(F["ShowTransactionCurrency"]));
            boolValues.Add((int)CompanySettingType.CustomerShowEnterpriseCurrency, StringUtility.GetBool(F["ShowEnterpriseCurrency"]));
            boolValues.Add((int)CompanySettingType.CustomerShowLedgerCurrency, StringUtility.GetBool(F["ShowLedgerCurrency"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceUseDeliveryCustomer, StringUtility.GetBool(F["UseDeliveryCustomerInvoicing"]));
            boolValues.Add((int)CompanySettingType.CustomerPaymentServiceOnlyToContract, StringUtility.GetBool(F["TransferPaymentServiceOnlyToContract"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceAllowEditAccountingRows, StringUtility.GetBool(F["AllowEditAccountingRows"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceCombineImportedAccountingRows, StringUtility.GetBool(F["CombineAccountingRows"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceUseAutomaticDistributionInImport, StringUtility.GetBool(F["UseAutomaticDistribution"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceTriangulationSales, StringUtility.GetBool(F["TriangulationSales"]));
            boolValues.Add((int)CompanySettingType.CustomerPaymentAddCustomerNameToInternaDescr, StringUtility.GetBool(F["AddCustomerNameToPaymentInternaDescr"]));
            boolValues.Add((int)CompanySettingType.CustomerInvoiceUseAutoAccountDistributionOnVoucher, StringUtility.GetBool(F["UseAutoAccountDistributionOnVoucher"])); 
            boolValues.Add((int)CompanySettingType.AllowChangesToInternalAccountsOnPaidCustomerInvoice, StringUtility.GetBool(F["AllowChangesToInternalAccountsOnPaidCustomerInvoice"]));
            boolValues.Add((int)CompanySettingType.AutomaticLedgerInvoiceNrWhenImport, StringUtility.GetBool(F["AutomaticLedgerInvoiceNrWhenImport"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            int interestHandlingType = 0;
            if (StringUtility.GetBool(F["InterestHandlingTypeNew"]))
                interestHandlingType = (int)SoeInvoiceInterestHandlingType.CreateNewInvoice;
            else if (StringUtility.GetBool(F["InterestHandlingTypeNext"]))
                interestHandlingType = (int)SoeInvoiceInterestHandlingType.AddToNextInvoice;

            if (interestHandlingType == 0)
                RedirectToSelf("INTEREST_ERROR", true);

            intValues.Add((int)CompanySettingType.CustomerReminderNrOfClaimLevels, StringUtility.GetInt(F["NrOfClaimLevels"]));
            intValues.Add((int)CompanySettingType.CustomerInterestHandlingType, interestHandlingType);
            intValues.Add((int)CompanySettingType.CustomerInterestAccumulatedBeforeInvoice, StringUtility.GetInt(F["InterestAccumulatedBeforeInvoice"], 0));
            intValues.Add((int)CompanySettingType.CustomerGracePeriodDays, StringUtility.GetInt(F["GracePeriodDays"], 0));
            intValues.Add((int)CompanySettingType.CustomerReminderHandlingType, reminderHandlingType);
            intValues.Add((int)CompanySettingType.CustomerReminderMinNrOfDaysForNewClaim, StringUtility.GetInt(F["MinNrOfDaysForNewClaim"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceDefaultVatType, StringUtility.GetInt(F["DefaultVatType"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, StringUtility.GetInt(F["DefaultPaymentCondition"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentSettlePaymentMethod, StringUtility.GetInt(F["SettlePaymentMethod"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, StringUtility.GetInt(F["DefaultPaymentConditionClaimsAndInterest"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentMethod, StringUtility.GetInt(F["DefaultPaymentMethod"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultBalanceList, StringUtility.GetInt(F["DefaultCustomerBalanceList"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultReminderTemplate, StringUtility.GetInt(F["DefaultReminderTemplate"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultInterestTemplate, StringUtility.GetInt(F["DefaultInterestTemplate"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultInterestRateCalculationTemplate, StringUtility.GetInt(F["DefaultInterestRateCalculationTemplate"], 0));
            intValues.Add((int)CompanySettingType.CustomerDefaultCreditLimit, StringUtility.GetInt(F["DefaultCreditLimit"], 0));
            intValues.Add((int)CompanySettingType.AccountingCurrencySource, StringUtility.GetInt(F["CurrencySource"], 0));
            intValues.Add((int)CompanySettingType.AccountingCurrencyIntervalType, StringUtility.GetInt(F["CurrencyIntervalType"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceDnBNorClientId, StringUtility.GetInt(F["DnBNorClientId"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAutogiroClientId, StringUtility.GetInt(F["AutogiroClientId"], 0));

            // Sequence numbers. Check if start numbers has changed
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

            //Merge Invoice to VoucherHead
            int invoiceToVoucherHeadType = StringUtility.GetBool(F["InvoiceMergeVoucherOnVoucherDate"]) ? (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate : (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;
            intValues.Add((int)CompanySettingType.CustomerInvoiceToVoucherHeadType, invoiceToVoucherHeadType);

            //Merge Invoice to VoucherRow
            int invoiceToVoucherRowType = StringUtility.GetBool(F["InvoiceMergeVoucherRowsOnAccount"]) ? (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount : (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
            intValues.Add((int)CompanySettingType.CustomerInvoiceToVoucherRowType, invoiceToVoucherRowType);

            //Merge Payment to VoucherHead
            int paymentToVoucherHeadType = StringUtility.GetBool(F["PaymentMergeVoucherOnVoucherDate"]) ? (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate : (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;
            intValues.Add((int)CompanySettingType.CustomerPaymentToVoucherHeadType, paymentToVoucherHeadType);

            //Merge Payment to VoucherRow
            int paymentToVoucherRowType = StringUtility.GetBool(F["PaymentMergeVoucherRowsOnAccount"]) ? (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount : (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
            intValues.Add((int)CompanySettingType.CustomerPaymentToVoucherRowType, paymentToVoucherRowType);

            // Age distribution
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionNbrOfIntervals, StringUtility.GetInt(F["AgeDistNbrOfIntervals"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionInterval1, StringUtility.GetInt(F["AgeDistInterval1"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionInterval2, StringUtility.GetInt(F["AgeDistInterval2"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionInterval3, StringUtility.GetInt(F["AgeDistInterval3"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionInterval4, StringUtility.GetInt(F["AgeDistInterval4"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceAgeDistributionInterval5, StringUtility.GetInt(F["AgeDistInterval5"], 0));

            // Liquidity planning
            /*intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningNbrOfIntervals, StringUtility.GetInt(F["LiqPlanNbrOfIntervals"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval1, StringUtility.GetInt(F["LiqPlanInterval1"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval2, StringUtility.GetInt(F["LiqPlanInterval2"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval3, StringUtility.GetInt(F["LiqPlanInterval3"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval4, StringUtility.GetInt(F["LiqPlanInterval4"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceLiquidityPlanningInterval5, StringUtility.GetInt(F["LiqPlanInterval5"], 0));*/

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Decimal

            var decimalValues = new Dictionary<int, decimal>();
            decimalValues.Add((int)CompanySettingType.CustomerInterestPercent, NumberUtility.ToDecimal(F["InterestPercent"], 2));

            if (!sm.UpdateInsertDecimalSettings(SettingMainType.Company, decimalValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region String

            var stringValues = new Dictionary<int, string>();

            stringValues.Add((int)CompanySettingType.CustomerInvoiceOurReference, F["OurReference"]);
            stringValues.Add((int)CompanySettingType.CustomerInvoiceTransferTypesToBeClosed, F["TransferTypesToBeClosed"]);
            stringValues.Add((int)CompanySettingType.CustomerInvoiceCashCustomerNumber, F["CashCustomerNumber"]);

            //Claim levels
            stringValues.Add((int)CompanySettingType.CustomerClaimLevel1Text, F["ClaimLevelText1"]);
            stringValues.Add((int)CompanySettingType.CustomerClaimLevel2Text, F["ClaimLevelText2"]);
            stringValues.Add((int)CompanySettingType.CustomerClaimLevel3Text, F["ClaimLevelText3"]);
            stringValues.Add((int)CompanySettingType.CustomerClaimLevel4Text, F["ClaimLevelText4"]);
            stringValues.Add((int)CompanySettingType.CustomerClaimLevelDebtCollectionText, F["ClaimLevelText5"]);

            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion
    }
}
