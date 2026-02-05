using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;

namespace SoftOne.Soe.Web.soe.economy.preferences.vouchersettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportManager rm;
        private SettingManager sm;
        private VoucherManager vm;
        private AccountManager am;

        public int AccountDimId { get; set; }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_VoucherSettings;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            vm = new VoucherManager(ParameterObject); 
            am = new AccountManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            DivConsolidatedAccounting.Visible = HasRolePermission(Feature.Economy_Accounting_CompanyGroup, Permission.Modify);
            DivIntrastat.Visible = HasRolePermission(Feature.Economy_Intrastat, Permission.Modify);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            #region Accounting

            MaxYearOpen.DataSource = SettingManager.MaxYearsOpen;
            MaxYearOpen.DataBind();

            MaxPeriodOpen.DataSource = SettingManager.MaxPeriodsOpen;
            MaxPeriodOpen.DataBind();

            DefaultVatCodeAccounting.ConnectDataSource(am.GetVatCodesDict(SoeCompany.ActorCompanyId, true));

            #endregion

            #region VoucherSeries

            Dictionary<int, string> voucherSeriesTypes = vm.GetVoucherSeriesTypesDict(SoeCompany.ActorCompanyId, false, true);
            VoucherSeriesTypeManual.ConnectDataSource(voucherSeriesTypes);
            VoucherSeriesTypeVat.ConnectDataSource(voucherSeriesTypes);
            SupplierInvoiceVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            SupplierPaymentVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            CustomerInvoiceVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            CustomerPaymentVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            StockVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            PayrollAccountExportVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            AccountdistributionVoucherSeries.ConnectDataSource(voucherSeriesTypes);
            #endregion

            #region Voucher registration

            DisableInlineValidationExplanation.DefaultIdentifier = " ";
            DisableInlineValidationExplanation.DisableFieldset = true;
            DisableInlineValidationExplanation.Instructions = new List<string>()
			{
				GetText(3351, "Om direkt validering är inaktiv valideras konton, belopp etc.") + " " + GetText(3352, "först när hela verifikatraden har registrerats och lämnats."),
			};

            #endregion

            #region Export

            //Default accounting order (voucherlist)
            DefaultAccountingOrder.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.VoucherList, onlyOriginal: true, addEmptyRow: true));
            //Default voucherlist
            DefaultVoucherList.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.VoucherList, onlyOriginal: true, addEmptyRow: true));
            //Default accountanalysis report
            DefaultAccountAnalysis.ConnectDataSource(rm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.GeneralLedger, onlyOriginal: true, addEmptyRow: true));

            #endregion

            #region Import

            AccountYear currentYear = am.GetCurrentAccountYear(SoeCompany.ActorCompanyId);
            if (currentYear != null)
            {
                Dictionary<int, string> voucherSeries = vm.GetVoucherSeriesByYearDict(currentYear.AccountYearId, SoeCompany.ActorCompanyId, true, false);
                VoucherImportVoucherSerie.ConnectDataSource(voucherSeries);
            }

            #endregion

            #region Consolidation

            Dictionary<int, string> dims = am.GetAccountDimsByCompanyDict(SoeCompany.ActorCompanyId, true, false, true);
            MapCompanyToAccount.ConnectDataSource(dims);
            //Mapc.ConnectDataSource(dims);

            #endregion

            #region Intrastat

            Dictionary<int, string> intrastatOrigins = new Dictionary<int, string>()
            {
                { 0, "" },
                { (int)SoeOriginType.SupplierInvoice, GetText(31, "Leverantörsfaktura") },
                { (int)SoeOriginType.Purchase, GetText(7538, "Beställning") },
            };
            IntrastatImportOriginType.ConnectDataSource(intrastatOrigins);

            #endregion

            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> accountingSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Accounting, SoeCompany.ActorCompanyId);

            #region Accounting

            //Max years
            MaxYearOpen.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingMaxYearOpen, (int)SettingDataType.Integer);
            //Max periods
            MaxPeriodOpen.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingMaxPeriodOpen, (int)SettingDataType.Integer);
            //Multi periods
            AllowMultiPeriodChange.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAllowMultiplePeriodChange, (int)SettingDataType.Boolean);
            //Use dims
            UseDimsInRegistration.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingUseDimsInRegistration, (int)SettingDataType.Boolean);

            CreateDiffRowOnBalanceTransfer.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingCreateDiffRowOnBalanceTransfer, (int)SettingDataType.Boolean, "True");

            // Default VAT code
            DefaultVatCodeAccounting.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingDefaultVatCode, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            #endregion

            #region VoucherSeries

            //Manual
            VoucherSeriesTypeManual.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingVoucherSeriesTypeManual, (int)SettingDataType.Integer);
            //Vat
            VoucherSeriesTypeVat.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingVoucherSeriesTypeVat, (int)SettingDataType.Integer);
            //SupplierInvoice
            SupplierInvoiceVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //CustomerInvoice
            CustomerInvoiceVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //SupplierPayment
            SupplierPaymentVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //CustomerPayment
            CustomerPaymentVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //CustomerPayment
            StockVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.StockDefaultVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //PayrollAccountVouchers
            PayrollAccountExportVoucherSeries.Value = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.PayrollAccountExportVoucherSeriesType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            //AccountdistributionVoucher
            AccountdistributionVoucherSeries.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountdistributionVoucherSeriesType, (int)SettingDataType.Integer);

            #endregion

            #region Voucher registration

            //Quantity
            UseQuantityInVoucher.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingUseQuantityInVoucher, (int)SettingDataType.Boolean);
            //Unbalanced
            AllowUnbalancedVoucher.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAllowUnbalancedVoucher, (int)SettingDataType.Boolean);
            //Edit voucher
            AllowEditVoucher.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAllowEditVoucher, (int)SettingDataType.Boolean);
            //Edit voucherdate
            AllowEditVoucherDate.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAllowEditVoucherDate, (int)SettingDataType.Boolean);
            //Validation
            DisableInlineValidation.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingDisableInlineValidation, (int)SettingDataType.Boolean);
            //Automatic accountdistribution
            AutomaticAccountDistribution.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAutomaticAccountDistribution, (int)SettingDataType.Boolean);
            //Unbalanced accountdistribution
            AllowUnbalancedAccountDistribution.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingAllowUnbalancedAccountDistribution, (int)SettingDataType.Boolean);
            //Combine periodaccounting rows to same voucher
            SeparateVouchersInPeriodAccounting.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingSeparateVouchersInPeriodAccounting, (int)SettingDataType.Boolean);
            //Combine periodaccounting rows to same voucher
            CreateVouchersForStockTransactions.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingCreateVouchersForStockTransactions, (int)SettingDataType.Boolean, "True");

            #endregion

            #region Currency

            ShowEnterpriseCurrency.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingShowEnterpriseCurrency, (int)SettingDataType.Boolean);

            #endregion

            #region Export

            //Default accounting order (voucherlist)
            DefaultAccountingOrder.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingDefaultAccountingOrder, (int)SettingDataType.Integer);
            //Default voucherlist
            DefaultVoucherList.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingDefaultVoucherList, (int)SettingDataType.Integer);
            //Default account analysis report
            DefaultAccountAnalysis.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingDefaultAnalysisReport, (int)SettingDataType.Integer);

            
            #endregion

            #region Import

            VoucherImportVoucherSerie.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.AccountingVoucherImportVoucherSerie, (int)SettingDataType.Integer);

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;

            SetAccountField(CompanySettingType.AccountingVoucherImportStandardAccount, VoucherImportDefaultAccount, "VoucherImportDefaultAccount");

            #endregion

            #region Consolidation

            MapCompanyToAccount.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.MapCompanyToAccountDimInConsolidation, (int)SettingDataType.Integer);

            #endregion

            #region Intrastat

            IntrastatImportOriginType.Value = sm.GetSettingFromDict(accountingSettingsDict, (int)CompanySettingType.IntrastatImportOriginType, (int)SettingDataType.Integer);

            #endregion

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

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)CompanySettingType.AccountingAllowUnbalancedVoucher, StringUtility.GetBool(F["AllowUnbalancedVoucher"]));
            boolValues.Add((int)CompanySettingType.AccountingUseQuantityInVoucher, StringUtility.GetBool(F["UseQuantityInVoucher"]));
            boolValues.Add((int)CompanySettingType.AccountingAllowMultiplePeriodChange, StringUtility.GetBool(F["AllowMultiPeriodChange"]));
            boolValues.Add((int)CompanySettingType.AccountingAllowEditVoucher, StringUtility.GetBool(F["AllowEditVoucher"]));
            boolValues.Add((int)CompanySettingType.AccountingAllowEditVoucherDate, StringUtility.GetBool(F["AllowEditVoucherDate"]));
            boolValues.Add((int)CompanySettingType.AccountingDisableInlineValidation, StringUtility.GetBool(F["DisableInlineValidation"]));
            boolValues.Add((int)CompanySettingType.AccountingAutomaticAccountDistribution, StringUtility.GetBool(F["AutomaticAccountDistribution"]));
            boolValues.Add((int)CompanySettingType.AccountingAllowUnbalancedAccountDistribution, StringUtility.GetBool(F["AllowUnbalancedAccountDistribution"]));
            boolValues.Add((int)CompanySettingType.AccountingSeparateVouchersInPeriodAccounting, StringUtility.GetBool(F["SeparateVouchersInPeriodAccounting"]));
            boolValues.Add((int)CompanySettingType.AccountingCreateVouchersForStockTransactions, StringUtility.GetBool(F["CreateVouchersForStockTransactions"]));

            boolValues.Add((int)CompanySettingType.AccountingShowEnterpriseCurrency, StringUtility.GetBool(F["ShowEnterpriseCurrency"]));

            boolValues.Add((int)CompanySettingType.AccountingUseDimsInRegistration, StringUtility.GetBool(F["UseDimsInRegistration"]));
            boolValues.Add((int)CompanySettingType.AccountingCreateDiffRowOnBalanceTransfer, StringUtility.GetBool(F["CreateDiffRowOnBalanceTransfer"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Int

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)CompanySettingType.AccountingDefaultVatCode, StringUtility.GetInt(F["DefaultVatCodeAccounting"], 0));
            intValues.Add((int)CompanySettingType.AccountingVoucherSeriesTypeManual, StringUtility.GetInt(F["VoucherSeriesTypeManual"], 0));
            intValues.Add((int)CompanySettingType.AccountingVoucherSeriesTypeVat, StringUtility.GetInt(F["VoucherSeriesTypeVat"], 0));
            intValues.Add((int)CompanySettingType.SupplierInvoiceVoucherSeriesType, StringUtility.GetInt(F["SupplierInvoiceVoucherSeries"], 0));
            intValues.Add((int)CompanySettingType.CustomerInvoiceVoucherSeriesType, StringUtility.GetInt(F["CustomerInvoiceVoucherSeries"], 0));
            intValues.Add((int)CompanySettingType.SupplierPaymentVoucherSeriesType, StringUtility.GetInt(F["SupplierPaymentVoucherSeries"], 0));
            intValues.Add((int)CompanySettingType.CustomerPaymentVoucherSeriesType, StringUtility.GetInt(F["CustomerPaymentVoucherSeries"], 0));
            intValues.Add((int)CompanySettingType.StockDefaultVoucherSeriesType, StringUtility.GetInt(F["StockVoucherSeries"], 0));
            intValues.Add((int)CompanySettingType.AccountingMaxYearOpen, StringUtility.GetInt(F["MaxYearOpen"], 0));
            intValues.Add((int)CompanySettingType.AccountingMaxPeriodOpen, StringUtility.GetInt(F["MaxPeriodOpen"], 0));
            intValues.Add((int)CompanySettingType.AccountingDefaultAccountingOrder, StringUtility.GetInt(F["DefaultAccountingOrder"], 0));
            intValues.Add((int)CompanySettingType.AccountingDefaultVoucherList, StringUtility.GetInt(F["DefaultVoucherList"], 0));
            intValues.Add((int)CompanySettingType.AccountingDefaultAnalysisReport, StringUtility.GetInt(F["DefaultAccountAnalysis"], 0));
            
            intValues.Add((int)CompanySettingType.AccountingVoucherImportVoucherSerie, StringUtility.GetInt(F["VoucherImportVoucherSerie"], 0));
            intValues.Add((int)CompanySettingType.AccountingVoucherImportStandardAccount, GetAccountId(F["VoucherImportDefaultAccount"]));
            intValues.Add((int)CompanySettingType.PayrollAccountExportVoucherSeriesType, StringUtility.GetInt(F["PayrollAccountExportVoucherSeries"], 0));

            intValues.Add((int)CompanySettingType.MapCompanyToAccountDimInConsolidation, StringUtility.GetInt(F["MapCompanyToAccount"], 0));

            intValues.Add((int)CompanySettingType.IntrastatImportOriginType, StringUtility.GetInt(F["IntrastatImportOriginType"], 0));

            intValues.Add((int)CompanySettingType.AccountdistributionVoucherSeriesType, StringUtility.GetInt(F["AccountdistributionVoucherSeries"], 0));


            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help methods

        private void SetAccountField(CompanySettingType type, TextEntry control, string id)
        {
            int accountId = sm.GetIntSetting(SettingMainType.Company, (int)type, UserId, SoeCompany.ActorCompanyId, 0);
            Account account = am.GetAccount(SoeCompany.ActorCompanyId, accountId);
            if (account != null)
            {
                control.Value = GetAccountNr(accountId);
                control.InfoText = account.Name;
            }
            control.OnChange = "getAccountName('" + id + "', '" + AccountDimId + "')";
        }

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

        private string GetAccountNr(int accountId)
        {
            // No account specified
            if (accountId == 0)
                return String.Empty;

            // Get account by specified id
            Account account = am.GetAccount(SoeCompany.ActorCompanyId, accountId);

            // Invalid account id
            if (account == null)
                return String.Empty;

            return account.AccountNr;
        }

        #endregion
    }
}
