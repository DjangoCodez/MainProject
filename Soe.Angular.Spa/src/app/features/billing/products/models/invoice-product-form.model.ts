import { ValidationHandler } from '@shared/handlers';
import { InvoiceProductDTO } from './invoice-product.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { FormArray, FormControl } from '@angular/forms';
import { AccountingSettingsForm } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import {
  IAccountingSettingsRowDTO,
  ICompTermDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  ExternalProductType,
  ProductAccountType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { StockDTO } from './stock.model';
import { LanguageTranslationForm } from '@shared/features/language-translations/models/language-translations-form.model';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

interface IInvoiceProductForm {
  validationHandler: ValidationHandler;
  element?: InvoiceProductDTO;
}

export class InvoiceProductForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IInvoiceProductForm) {
    super(validationHandler, {
      productId: new SoeTextFormControl(element?.productId || 0, {
        isIdField: true,
      }),
      number: new SoeTextFormControl(
        element?.number || '',
        {
          required: true,
        },
        'billing.product.number'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          required: true,
          isNameField: true,
        },
        'billing.product.name'
      ),
      description: new SoeTextFormControl(element?.description || ''),
      vatType: new SoeSelectFormControl(element?.vatType || undefined),
      productUnitId: new SoeSelectFormControl(
        element?.productUnitId || undefined
      ),
      vatCodeId: new SoeSelectFormControl(element?.vatCodeId || undefined),
      timeCodeId: new SoeSelectFormControl(element?.timeCodeId || undefined),
      productGroupId: new SoeSelectFormControl(
        element?.productGroupId || undefined
      ),
      ean: new SoeTextFormControl(element?.ean || ''),
      showDescriptionAsTextRow: new SoeCheckboxFormControl(
        element?.showDescriptionAsTextRow || false
      ),
      showDescrAsTextRowOnPurchase: new SoeCheckboxFormControl(
        element?.showDescrAsTextRowOnPurchase || false
      ),
      householdDeductionType: new SoeSelectFormControl(
        element?.householdDeductionType || undefined
      ),
      householdDeductionPercentage: new SoeNumberFormControl(
        element?.householdDeductionPercentage || undefined
      ),
      useCalculatedCost: new SoeCheckboxFormControl(
        element?.useCalculatedCost || undefined
      ),
      calculationType: new SoeSelectFormControl(
        element?.calculationType || undefined
      ),
      weight: new SoeNumberFormControl(element?.weight || undefined),
      guaranteePercentage: new SoeNumberFormControl(
        element?.guaranteePercentage || undefined
      ),
      intrastatCodeId: new SoeSelectFormControl(
        element?.intrastatCodeId || undefined
      ),
      sysCountryId: new SoeSelectFormControl(
        element?.sysCountryId || undefined
      ),
      categoryIds: arrayToFormArray(element?.categoryIds || []),
      dontUseDiscountPercent: new SoeCheckboxFormControl(
        element?.dontUseDiscountPercent || false
      ),
      purchasePrice: new SoeNumberFormControl(
        element?.purchasePrice || 0.0,
        {
          decimals: 4,
        },
        'billing.product.purchaseprice'
      ),
      accountingSettings: new FormArray<AccountingSettingsForm>([]),
      accountingPrio: new SoeTextFormControl(
        element?.accountingPrio || undefined
      ),
      isStockProduct: new SoeCheckboxFormControl(
        element?.isStockProduct || false
      ),
      stockAccountingSettings: new FormArray<AccountingSettingsForm>([]),
      isExternal: new SoeCheckboxFormControl(element?.isExternal || false),
      sysWholesellerName: new SoeTextFormControl(
        element?.sysWholesellerName || ''
      ),
      externalPrice: new SoeNumberFormControl(undefined),
      priceListName: new SoeTextFormControl(undefined),
      sysProductType: new SoeNumberFormControl(
        element?.sysProductType || ExternalProductType.Unknown
      ),
      state: new SoeNumberFormControl(
        element?.sysProductType || SoeEntityState.Active
      ),
      defaultGrossMarginCalculationType: new SoeSelectFormControl(
        element?.defaultGrossMarginCalculationType || undefined
      ),
      active: new SoeCheckboxFormControl(element?.active || true),
      translations: new FormArray<LanguageTranslationForm>([]),

      priceLists: arrayToFormArray(element?.priceLists || []),
      stocks: arrayToFormArray(element?.stocks || []),
      extraFields: arrayToFormArray(element?.extraFields || []),
      copyPrice: new SoeCheckboxFormControl(true),
      copyAccounts: new SoeCheckboxFormControl(true),
      copyStock: new SoeCheckboxFormControl(true),
    });

    this.thisValidationHandler = validationHandler;
    this.patchAccountingSettings(element?.accountingSettings ?? []);
    this.patchCompTerms(element?.translations ?? []);

    this.active.valueChanges.subscribe(a => {
      this.state.setValue(a ? SoeEntityState.Active : SoeEntityState.Inactive);
    });
  }

  get productId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productId;
  }
  get number(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.number;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
  get vatType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.vatType;
  }
  get productUnitId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productUnitId;
  }
  get vatCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.vatCodeId;
  }
  get timeCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeCodeId;
  }
  get productGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productGroupId;
  }
  get ean(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.ean;
  }
  get showDescriptionAsTextRow(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showDescriptionAsTextRow;
  }
  get showDescrAsTextRowOnPurchase(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showDescrAsTextRowOnPurchase;
  }
  get householdDeductionType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.householdDeductionType;
  }
  get householdDeductionPercentage(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.householdDeductionPercentage;
  }
  get useCalculatedCost(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useCalculatedCost;
  }
  get calculationType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.calculationType;
  }
  get weight(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.weight;
  }
  get guaranteePercentage(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.guaranteePercentage;
  }
  get intrastatCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.intrastatCodeId;
  }
  get sysCountryId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysCountryId;
  }
  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }
  get dontUseDiscountPercent(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.dontUseDiscountPercent;
  }
  get purchasePrice(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchasePrice;
  }
  get accountingSettings(): FormArray<AccountingSettingsForm> {
    return <FormArray<AccountingSettingsForm>>this.controls.accountingSettings;
  }
  get accountingPrio(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountingPrio;
  }
  get isStockProduct(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isStockProduct;
  }
  get stockAccountingSettings(): FormArray<AccountingSettingsForm> {
    return <FormArray<AccountingSettingsForm>>(
      this.controls.stockAccountingSettings
    );
  }
  get isExternal(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isExternal;
  }
  get sysWholesellerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysWholesellerName;
  }
  get externalPrice(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.externalPrice;
  }
  get priceListName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.priceListName;
  }
  get sysProductType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysProductType;
  }
  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }
  get active(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.active;
  }

  get priceLists(): FormArray<FormControl<PriceListDTO>> {
    return <FormArray<FormControl<PriceListDTO>>>this.controls.priceLists;
  }

  get stocks(): FormArray<FormControl<StockDTO>> {
    return <FormArray<FormControl<StockDTO>>>this.controls.stocks;
  }

  get translations(): FormArray<LanguageTranslationForm> {
    return <FormArray<LanguageTranslationForm>>this.controls.translations;
  }

  get extraFields(): FormArray<FormControl<IExtraFieldRecordDTO>> {
    return <FormArray<FormControl<IExtraFieldRecordDTO>>>(
      this.controls.extraFields
    );
  }
  get copyPrice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.copyPrice;
  }
  get copyAccounts(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.copyAccounts;
  }
  get copyStock(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.copyStock;
  }

  customPatchValue(product: InvoiceProductDTO): void {
    this.patchValue(product);
    this.customCategoryIdsPatchValue(product.categoryIds);

    this.patchAccountingSettings(product.accountingSettings);
  }

  private patchAccountingSettings(rows: IAccountingSettingsRowDTO[]): void {
    const productAccSettings = this.getAccountSettings(rows, true);
    const stockAccSettings = this.getAccountSettings(rows, false);
    this.patchAccountingSettingsRows(productAccSettings);
    this.patchStockAccountingSettingsRows(stockAccSettings);
  }

  customCategoryIdsPatchValue(categoryIds: number[]): void {
    clearAndSetFormArray(categoryIds, this.categoryIds);
  }

  mergeAccountingSettings(): void {
    const productAccSettings = this.accountingSettings.value;
    const stockAccSettings = this.stockAccountingSettings.value;

    this.customPatchAccountSettings(
      productAccSettings.concat(stockAccSettings)
    );
  }

  customPatchAccountSettings(
    rows: IAccountingSettingsRowDTO[],
    isProductAccount: boolean = true
  ): void {
    if (isProductAccount) this.patchAccountingSettingsRows(rows);
    else this.patchStockAccountingSettingsRows(rows);
  }

  private patchAccountingSettingsRows(
    rows: IAccountingSettingsRowDTO[] | undefined
  ): void {
    this.accountingSettings?.clear();
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.accountingSettings.push(
          new AccountingSettingsForm({
            validationHandler: this.thisValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.accountingSettings.updateValueAndValidity();
    }
  }

  private patchStockAccountingSettingsRows(
    rows: IAccountingSettingsRowDTO[] | undefined
  ): void {
    this.stockAccountingSettings?.clear();
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.stockAccountingSettings.push(
          new AccountingSettingsForm({
            validationHandler: this.thisValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.stockAccountingSettings.updateValueAndValidity();
    }
  }

  private getAccountSettings(
    accountingSettings: IAccountingSettingsRowDTO[],
    isProduct: boolean = true
  ): IAccountingSettingsRowDTO[] {
    return accountingSettings.filter(a => {
      const res = [
        ProductAccountType.Purchase,
        ProductAccountType.Sales,
        ProductAccountType.VAT,
        ProductAccountType.SalesNoVat,
        ProductAccountType.SalesContractor,
      ].includes(a.type);

      return isProduct ? res : !res;
    });
  }

  patchCompTerms(compTermRows: ICompTermDTO[]) {
    this.translations?.clear();

    for (const compTerm of compTermRows) {
      const languageRow = new LanguageTranslationForm({
        validationHandler: this.thisValidationHandler,
        element: compTerm as CompTermDTO,
      });
      this.translations.push(languageRow, { emitEvent: false });
    }
    this.translations.updateValueAndValidity();
  }

  patchCopyItems(
    priceLists: PriceListDTO[],
    stocks: StockDTO[],
    extraFields: IExtraFieldRecordDTO[],
    copyPrice: boolean,
    copyAccounts: boolean,
    copyStock: boolean
  ): void {
    this.patchValue({
      copyPrice,
      copyAccounts,
      copyStock,
    });

    this.patchPriceList(priceLists);
    this.patchStocks(stocks);
    this.patchExtraFields(extraFields);
  }

  patchPriceList(priceLists: PriceListDTO[]): void {
    this.priceLists.clear({ emitEvent: false });

    if (!this.copyPrice.value) return;

    const pList = arrayToFormArray(priceLists);
    for (let i = 0; i < pList.length; i++) {
      this.priceLists.push(pList.at(i) as FormControl<PriceListDTO>, {
        emitEvent: false,
      });
    }
  }

  patchStocks(stocks: StockDTO[]): void {
    this.stocks.clear({ emitEvent: false });

    if (!this.copyStock.value) return;

    const pStocks = arrayToFormArray(stocks);
    for (let i = 0; i < pStocks.length; i++) {
      this.stocks.push(pStocks.at(i) as FormControl<StockDTO>, {
        emitEvent: false,
      });
    }
  }

  patchExtraFields(extraFields: IExtraFieldRecordDTO[]): void {
    this.extraFields.clear({ emitEvent: false });

    const pExtraFields = arrayToFormArray(extraFields);
    for (let i = 0; i < pExtraFields.length; i++) {
      this.extraFields.push(
        pExtraFields.at(i) as FormControl<IExtraFieldRecordDTO>,
        {
          emitEvent: false,
        }
      );
    }
  }
}
