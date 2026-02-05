import { computed, inject, Injectable, Signal, signal } from '@angular/core';
import {
  CompanySettingType,
  TermGroup_InvoiceVatType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { forkJoin, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SupplierInvoiceSettingsService {
  private readonly coreService = inject(CoreService);

  private static readonly ALL_COMPANY_SETTING_TYPES: CompanySettingType[] = [
    // Invoice Processing Settings
    CompanySettingType.BillingCopyInvoiceNrToOcr,
    CompanySettingType.SupplierInvoiceTransferToVoucher,
    CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer,
    CompanySettingType.SupplierInvoiceDefaultDraft,
    CompanySettingType.SupplierInvoiceAllowEditOrigin,
    CompanySettingType.SupplierInvoiceAllowInterim,
    CompanySettingType.SupplierInvoiceKeepSupplier,
    CompanySettingType.SupplierInvoiceRoundVAT,
    CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder,
    CompanySettingType.SupplierInvoiceProductRowsImport,
    CompanySettingType.SupplierInvoiceAllowEditAccountingRows,

    // Currency Display Settings
    CompanySettingType.SupplierShowTransactionCurrency,
    CompanySettingType.SupplierShowEnterpriseCurrency,
    CompanySettingType.SupplierShowLedgerCurrency,

    // Default Values
    CompanySettingType.SupplierInvoiceDefaultVatType,
    CompanySettingType.SupplierPaymentDefaultPaymentCondition,
    CompanySettingType.SupplierInvoiceVoucherSeriesType,
    CompanySettingType.AccountingDefaultVoucherList,
    CompanySettingType.AccountingDefaultVatCode,
    CompanySettingType.ProjectDefaultTimeCodeId,

    // Feature Toggles
    CompanySettingType.ProjectChargeCostsToProject,
    CompanySettingType.ProductMisc,
    CompanySettingType.FISupplierInvoiceOCRCheckReference,
    CompanySettingType.SupplierUseTimeDiscount,
    CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts,

    // Attestation Settings
    CompanySettingType.SaveSupplierInvoiceAttestType,
    CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup,

    // Scanning/Import Settings
    CompanySettingType.ScanningReferenceTargetField,
    CompanySettingType.ScanningCodeTargetField,
    CompanySettingType.IntrastatImportOriginType,

    // Account Settings - Basic
    CompanySettingType.AccountSupplierDebt,
    CompanySettingType.AccountSupplierPurchase,
    CompanySettingType.AccountCommonVatReceivable,
    CompanySettingType.AccountSupplierInterim,
    CompanySettingType.AccountCommonReverseVatPurchase,
    CompanySettingType.AccountCommonVatPayable1Reversed,
    CompanySettingType.AccountCommonVatReceivableReversed,

    // Account Settings - EU Import
    CompanySettingType.AccountCommonVatPayable1EUImport,
    CompanySettingType.AccountCommonVatPayable2EUImport,
    CompanySettingType.AccountCommonVatPayable3EUImport,
    CompanySettingType.AccountCommonVatReceivableEUImport,
    CompanySettingType.AccountCommonVatPurchaseEUImport,

    // Account Settings - Non-EU Import
    CompanySettingType.AccountCommonVatPayable1NonEUImport,
    CompanySettingType.AccountCommonVatPayable2NonEUImport,
    CompanySettingType.AccountCommonVatPayable3NonEUImport,
    CompanySettingType.AccountCommonVatReceivableNonEUImport,
    CompanySettingType.AccountCommonVatPurchaseNonEUImport,
  ];

  private static readonly ALL_USER_SETTING_TYPES: UserSettingType[] = [
    // UserSettingType.SupplierInvoiceSimplifiedRegistration,
    UserSettingType.BillingSupplierInvoiceDefaultExpanders,
    // UserSettingType.BillingSupplierInvoiceSlider,
    // UserSettingType.BillingSupplierInvoiceScale,
  ];

  private settings: UserCompanySettingCollection = {};

  private readonly expanders = signal<Set<string>>(new Set<string>());

  private readonly expanderOpen = (name: string): Signal<boolean> =>
    computed(() => this.expanders().has(name));

  // Expander getters - using a more compact approach
  readonly attestUserExpanderOpen = this.expanderOpen('AttestUserExpander');
  readonly projectRowsExpanderOpen = this.expanderOpen('ProjectRowsExpander');
  readonly projectOrderExpanderOpen = this.expanderOpen('ProjectOrderExpander');
  readonly accountingRowsExpanderOpen = this.expanderOpen(
    'AccountingRowsExpander'
  );
  readonly tracingExpanderOpen = this.expanderOpen('TracingExpander');
  readonly imageGalleryExpanderOpen = this.expanderOpen('ImageGalleryExpander');
  readonly filesExpanderOpen = this.expanderOpen('FilesExpander');
  readonly purchaseExpanderOpen = this.expanderOpen('PurchaseExpander');
  readonly productRowsExpanderOpen = this.expanderOpen('ProductRowsExpander');
  readonly costAllocationExpanderOpen = this.expanderOpen(
    'CostAllocationExpander'
  );

  /** Load everything in one go and cache locally */
  public loadSettings() {
    return forkJoin([this.loadCompanySettings(), this.loadUserSettings()]);
  }

  private loadCompanySettings() {
    return this.coreService
      .getCompanySettings(
        SupplierInvoiceSettingsService.ALL_COMPANY_SETTING_TYPES
      )
      .pipe(tap((x: UserCompanySettingCollection) => (this.settings = x)));
  }

  private loadUserSettings() {
    return this.coreService
      .getUserSettings(SupplierInvoiceSettingsService.ALL_USER_SETTING_TYPES)
      .pipe(
        tap((x: UserCompanySettingCollection) => {
          const expanderSetting = x[
            UserSettingType.BillingSupplierInvoiceDefaultExpanders
          ] as string;

          const expanderSet = new Set<string>();

          expanderSetting
            .split(';')
            .map(s => s.trim())
            .forEach(p => expanderSet.add(p));

          this.expanders.set(expanderSet);
        })
      );
  }

  // Generic helper methods to reduce repetition
  private getBoolSetting(settingType: CompanySettingType): boolean {
    return SettingsUtil.getBoolCompanySetting(this.settings, settingType);
  }

  public getIntSetting(settingType: CompanySettingType): number | undefined {
    return SettingsUtil.getIntCompanySetting(this.settings, settingType);
  }

  // ===== Bools =====
  public get usesAccountHierarchy(): boolean {
    return this.getBoolSetting(CompanySettingType.UseAccountHierarchy);
  }

  public get billingCopyInvoiceNrToOcr(): boolean {
    return this.getBoolSetting(CompanySettingType.BillingCopyInvoiceNrToOcr);
  }

  public get supplierInvoiceTransferToVoucher(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceTransferToVoucher
    );
  }

  public get supplierInvoiceAskPrintVoucherOnTransfer(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer
    );
  }

  public get supplierInvoiceDefaultDraft(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierInvoiceDefaultDraft);
  }

  public get supplierInvoiceAllowEditOrigin(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceAllowEditOrigin
    );
  }

  public get supplierInvoiceAllowInterim(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierInvoiceAllowInterim);
  }

  public get showTransactionCurrency(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierShowTransactionCurrency
    );
  }

  public get showEnterpriseCurrency(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierShowEnterpriseCurrency
    );
  }

  public get showLedgerCurrency(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierShowLedgerCurrency);
  }

  public get projectChargeCostsToProject(): boolean {
    return this.getBoolSetting(CompanySettingType.ProjectChargeCostsToProject);
  }

  public get fiSupplierInvoiceOCRCheckReference(): boolean {
    return this.getBoolSetting(
      CompanySettingType.FISupplierInvoiceOCRCheckReference
    );
  }

  public get usesTimeDiscount(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierUseTimeDiscount);
  }

  public get supplierInvoiceAllowEditAccountingRows(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceAllowEditAccountingRows
    );
  }

  public get supplierInvoiceKeepSupplier(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierInvoiceKeepSupplier);
  }

  public get useInternalAccountsWithBalanceSheetAccounts(): boolean {
    return this.getBoolSetting(
      CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts
    );
  }

  public get supplierInvoiceRoundVAT(): boolean {
    return this.getBoolSetting(CompanySettingType.SupplierInvoiceRoundVAT);
  }

  public get supplierInvoiceGetInternalAccountsFromOrder(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder
    );
  }

  public get supplierInvoiceProductRowsImport(): boolean {
    return this.getBoolSetting(
      CompanySettingType.SupplierInvoiceProductRowsImport
    );
  }

  // ===== Ints =====
  public get defaultVatType(): TermGroup_InvoiceVatType {
    return (
      this.getIntSetting(CompanySettingType.SupplierInvoiceDefaultVatType) ??
      TermGroup_InvoiceVatType.Merchandise
    );
  }

  public get defaultPaymentConditionId(): number | undefined {
    return this.getIntSetting(
      CompanySettingType.SupplierPaymentDefaultPaymentCondition
    );
  }

  public get supplierInvoiceVoucherSeriesType(): number | undefined {
    return this.getIntSetting(
      CompanySettingType.SupplierInvoiceVoucherSeriesType
    );
  }

  public get accountingDefaultVoucherList(): number | undefined {
    return this.getIntSetting(CompanySettingType.AccountingDefaultVoucherList);
  }

  public get productMisc(): number | undefined {
    return this.getIntSetting(CompanySettingType.ProductMisc);
  }

  public get saveSupplierInvoiceAttestType(): number | undefined {
    return this.getIntSetting(CompanySettingType.SaveSupplierInvoiceAttestType);
  }

  public get supplierInvoiceAttestFlowDefaultAttestGroup(): number | undefined {
    return this.getIntSetting(
      CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup
    );
  }

  public get projectDefaultTimeCodeId(): number | undefined {
    return this.getIntSetting(CompanySettingType.ProjectDefaultTimeCodeId);
  }

  public get scanningReferenceTargetField(): number | undefined {
    return this.getIntSetting(CompanySettingType.ScanningReferenceTargetField);
  }

  public get scanningCodeTargetField(): number | undefined {
    return this.getIntSetting(CompanySettingType.ScanningCodeTargetField);
  }

  public get intrastatImportOriginType(): number | undefined {
    return this.getIntSetting(CompanySettingType.IntrastatImportOriginType);
  }

  public get accountingDefaultVatCodeId(): number | undefined {
    return this.getIntSetting(CompanySettingType.AccountingDefaultVatCode);
  }
}
