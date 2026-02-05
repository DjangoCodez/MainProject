import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeSelectFormControl,
  SoeFormGroup,
  SoeSwitchFormControl,
  SoeFormControl,
  SoeDateFormControl,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SieImportVoucheriesMappingForm } from './sie-series-selection-form.model';
import {
  ISieAccountDimMappingDTO,
  ISieAccountMappingDTO,
  ISieImportPreviewDTO,
  ISieVoucherSeriesMappingDTO,
} from '@shared/models/generated-interfaces/SieImportDTO';
import { IFileDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IVoucherSeriesDTO } from '@shared/models/generated-interfaces/VoucherSeriesDTOs';
import { SieFormValidators } from './sie-form-validators.model';

interface ISieImportForm {
  validationHandler: ValidationHandler;
}
interface ISieImportPreviewForm {
  validationHandler: ValidationHandler;
  element: ISieImportPreviewDTO | undefined;
}

interface ISieAccountDimMappingForm {
  validationHandler: ValidationHandler;
  element: ISieAccountDimMappingDTO | undefined;
}

interface ISieAccountMappingForm {
  validationHandler: ValidationHandler;
  element: ISieAccountMappingDTO | undefined;
}

interface ISieVoucherSeriesMappingForm {
  validationHandler: ValidationHandler;
  element: ISieVoucherSeriesMappingDTO | undefined;
}

export class SieVoucherSeriesMappingForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISieVoucherSeriesMappingForm) {
    super(validationHandler, {
      number: new SoeTextFormControl(element?.number || '', {}),
      voucherNrFrom: new SoeNumberFormControl(
        element?.voucherNrFrom || undefined
      ),
      voucherNrTo: new SoeNumberFormControl(element?.voucherNrTo || undefined),
      voucherSeriesTypeId: new SoeNumberFormControl(
        element?.voucherSeriesTypeId || undefined
      ),
    });
  }
}

export class SieAccountMappingForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISieAccountMappingForm) {
    super(validationHandler, {
      name: new SoeTextFormControl(element?.name || '', {}),
      number: new SoeTextFormControl(element?.number || '', {}),
      accountId: new SoeNumberFormControl(element?.accountId || undefined),
      action: new SoeNumberFormControl(element?.action || undefined),
    });
  }
}

export class SieAccountDimMappingForm extends SoeFormGroup {
  accountDimMappingValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISieAccountDimMappingForm) {
    super(validationHandler, {
      dimNr: new SoeNumberFormControl(element?.dimNr || undefined),

      name: new SoeTextFormControl(element?.name || '', {}),

      accountDimId: new SoeNumberFormControl(
        element?.accountDimId || undefined
      ),

      isAccountStd: new SoeCheckboxFormControl(element?.isAccountStd || false),
      isImport: new SoeCheckboxFormControl(element?.isImport || false),

      accountMappings: new FormArray<SieAccountMappingForm>([]), //TODO: patch account mappings -done in the parent form
    });
    this.accountDimMappingValidationHandler = validationHandler;
    this.customPatchValues(element?.accountMappings);
  }
  get accountDimId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDimId;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get isAccountStd(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isAccountStd;
  }
  get isImport(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isImport;
  }
  get accountMappings(): FormArray<SieAccountMappingForm> {
    return <FormArray<SieAccountMappingForm>>this.controls.accountMappings;
  }

  public customPatchValues(elements: ISieAccountMappingDTO[] | undefined) {
    if (elements) {
      this.accountMappings?.clear({ emitEvent: false });
      if (elements && elements.length > 0) {
        elements.forEach(e => {
          this.accountMappings.push(
            new SieAccountMappingForm({
              validationHandler: this.accountDimMappingValidationHandler,
              element: e,
            }),
            { emitEvent: false }
          );
        });
        this.accountMappings.updateValueAndValidity();
      }
    }
  }
}

export class SieImportPreviewForm extends SoeFormGroup {
  sieValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISieImportPreviewForm) {
    super(validationHandler, {
      fileContainsAccountStd: new SoeCheckboxFormControl(
        element?.fileContainsAccountStd || false,
        { disabled: true, defaultValue: false },
        ''
      ),
      fileContainsVouchers: new SoeCheckboxFormControl(
        element?.fileContainsVouchers || false,
        { disabled: true, defaultValue: false },
        ''
      ),
      fileContainsAccountBalances: new SoeCheckboxFormControl(
        element?.fileContainsAccountBalances || false,
        { disabled: true, defaultValue: false },
        ''
      ),

      accountingYearFrom: new SoeDateFormControl(
        element?.accountingYearFrom || undefined,
        {}
      ),

      accountingYearTo: new SoeDateFormControl(
        element?.accountingYearTo || undefined,
        {}
      ),

      accountingYearId: new SoeNumberFormControl(
        element?.accountingYearId || undefined
      ),

      accountingYearIsClosed: new SoeCheckboxFormControl(
        element?.accountingYearIsClosed || false,
        { disabled: true, defaultValue: false },
        ''
      ),

      accountStd: new SieAccountDimMappingForm({
        validationHandler,
        element: undefined,
      }),
      accountDims: new FormArray<SieAccountDimMappingForm>([]),
      voucherSeriesMappings: new FormArray<SieVoucherSeriesMappingForm>([]),
    });
    this.sieValidationHandler = validationHandler;
    this.customPatchValues(element);
  }
  customPatchValues(element: ISieImportPreviewDTO | undefined) {
    if (element) {
      this.patchValue(element);
      this.customAccountStdPatchValue(element.accountStd);
      this.customAccountDimsPatchValue(element.accountDims);
      this.customeVoucherSeriesMappingsPatchValue(
        element.voucherSeriesMappings
      );
    }
  }

  customAccountStdPatchValue(element: ISieAccountDimMappingDTO | undefined) {
    if (element) {
      this.accountStd.patchValue(element);
      this.accountStd.customPatchValues(element.accountMappings);
    }
  }

  customAccountDimsPatchValue(
    elements: ISieAccountDimMappingDTO[] | undefined
  ) {
    if (elements) {
      this.accountDims?.clear({ emitEvent: false });
      if (elements && elements.length > 0) {
        elements.forEach(e => {
          this.accountDims.push(
            new SieAccountDimMappingForm({
              validationHandler: this.sieValidationHandler,
              element: e,
            }),
            { emitEvent: false }
          );
        });
        this.accountDims.updateValueAndValidity();
      }
    }
  }

  customeVoucherSeriesMappingsPatchValue(
    elements: ISieVoucherSeriesMappingDTO[] | undefined
  ) {
    if (elements) {
      this.voucherSeriesMappings?.clear({ emitEvent: false });
      if (elements && elements.length > 0) {
        elements.forEach(e => {
          this.voucherSeriesMappings.push(
            new SieVoucherSeriesMappingForm({
              validationHandler: this.sieValidationHandler,
              element: e,
            }),
            { emitEvent: false }
          );
        });
        this.voucherSeriesMappings.updateValueAndValidity();
      }
    }
  }

  get fileContainsAccountStd(): SoeCheckboxFormControl {
    return this.controls.fileContainsAccountStd as SoeCheckboxFormControl;
  }

  get fileContainsVouchers(): SoeCheckboxFormControl {
    return this.controls.fileContainsVouchers as SoeCheckboxFormControl;
  }

  get fileContainsAccountBalances(): SoeCheckboxFormControl {
    return this.controls.fileContainsAccountBalances as SoeCheckboxFormControl;
  }

  get accountingYearFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.accountingYearFrom;
  }

  get accountingYearTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.accountingYearTo;
  }
  get accountingYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountingYearId;
  }

  get accountingYearIsClosed(): SoeCheckboxFormControl {
    return this.controls.accountingYearIsClosed as SoeCheckboxFormControl;
  }

  get file() {
    return this.controls.file as SoeFormControl<IFileDTO>;
  }

  get accountStd(): SieAccountDimMappingForm {
    return this.controls.accountStd as SieAccountDimMappingForm;
  }
  get accountDims(): FormArray<SieAccountDimMappingForm> {
    return <FormArray<SieAccountDimMappingForm>>this.controls.accountDims;
  }
  get voucherSeriesMappings(): FormArray<SieVoucherSeriesMappingForm> {
    return <FormArray<SieVoucherSeriesMappingForm>>(
      this.controls.voucherSeriesMappings
    );
  }
}

export class SieImportForm extends SoeFormGroup {
  sieValidationHandler: ValidationHandler;
  voucherSeries: IVoucherSeriesDTO[] = [];

  constructor({ validationHandler }: ISieImportForm) {
    super(
      validationHandler,
      {
        // File upload
        file: new SoeFormControl<IFileDTO>(
          null,
          { required: true },
          null,
          null,
          'economy.import.sie.file'
        ),
        accountYearId: new SoeSelectFormControl(
          undefined,
          { disabled: true, defaultValue: null },
          'sie.import.accountingYear'
        ),
        allowNotOpenAccountYear: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'economy.import.sie.allowonlyopenyear'
        ),

        fileHasAccounts: new SoeCheckboxFormControl(false),
        importAccounts: new SoeSwitchFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.selectAccount'
        ),

        // Account settings
        importAccountStd: new SoeCheckboxFormControl(
          true,
          { disabled: true, defaultValue: true },
          'sie.import.importAccountStd'
        ),
        importAccountInternal: new SoeCheckboxFormControl(
          true,
          { disabled: true, defaultValue: true },
          'sie.import.importAccountInternal'
        ),
        overrideNameConflicts: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.overrideNameConflicts'
        ),
        approveEmptyAccountNames: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.approveEmptyAccountNames'
        ),

        // Voucher Settings

        fileHasVouchers: new SoeCheckboxFormControl(false),
        importVouchers: new SoeSwitchFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.selectVoucher'
        ),

        defaultVoucherSeriesId: new SoeSelectFormControl(
          undefined,
          { disabled: true, defaultValue: undefined },
          'sie.import.defaultVoucherSeriesId'
        ),
        overrideVoucherSeries: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.overrideVoucherSeries'
        ),
        useAccountDistribution: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.useAccountDistribution'
        ),

        voucherSeriesMapping: new FormArray<SieImportVoucheriesMappingForm>([]),
        skipAlreadyExistingVouchers: new SoeCheckboxFormControl(
          true,
          { disabled: true, defaultValue: true },
          'sie.import.skipAlreadyExistingVouchers'
        ),
        takeVoucherNrFromSeries: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.takeVoucherNrFromSeries'
        ),

        overrideVoucherSeriesDelete: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.overrideVoucherSeriesDelete'
        ),
        voucherSeriesDelete: new SoeSelectFormControl([], {
          disabled: true,
          defaultValue: [],
        }),

        // Account balance settings
        fileHasAccountBalances: new SoeSwitchFormControl(false, {
          defaultValue: false,
        }),
        importAccountBalances: new SoeCheckboxFormControl(
          false,
          {
            defaultValue: false,
            disabled: true,
          },
          'sie.import.importAccountBalances'
        ),
        overrideAccountBalance: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.overrideAccountBalance'
        ),
        useUBInsteadOfIB: new SoeCheckboxFormControl(
          false,
          { disabled: true, defaultValue: false },
          'sie.import.useUBInsteadOfIB'
        ),
        preview: new SieImportPreviewForm({
          validationHandler,
          element: undefined,
        }),
      },
      {
        validators: [
          SieFormValidators.selectAtLeastOneImport,
          SieFormValidators.unmappedVoucherSeries,
          SieFormValidators.missingAccountName,
        ],
      }
    );
    this.sieValidationHandler = validationHandler;
    this.setupSubscribers();
  }

  public customPatchValue(element: ISieImportPreviewDTO) {
    this.preview.customPatchValues(element);
  }

  setupSubscribers(): void {
    this.file.valueChanges.subscribe(value => {
      if (value) {
        this.accountYearId.enable();
        this.allowNotOpenAccountYear.enable();
        this.importAccounts.enable();
      } else {
        this.accountYearId.disable();
        this.allowNotOpenAccountYear.disable();
        this.importAccounts.disable();
      }
    });

    this.fileHasAccountBalances.valueChanges.subscribe(value => {
      if (value) {
        this.importAccountBalances.enable();
        this.accountYearId.enable();
        this.allowNotOpenAccountYear.enable();
      } else {
        this.importAccountBalances.disable();

        if (!this.fileHasVouchers.value) {
          this.accountYearId.disable();
          this.allowNotOpenAccountYear.disable();
        }
      }
    });

    this.fileHasAccounts.valueChanges.subscribe(value => {
      if (value) {
        this.importAccounts.enable();
      } else {
        this.importAccounts.disable();
      }
    });

    this.fileHasVouchers.valueChanges.subscribe(value => {
      if (value) {
        this.importVouchers.enable();
        this.accountYearId.enable();
        this.allowNotOpenAccountYear.enable();
      } else {
        this.importVouchers.disable();

        if (!this.fileHasAccountBalances.value) {
          this.accountYearId.disable();
          this.allowNotOpenAccountYear.disable();
        }
      }
    });

    this.accountYearId.valueChanges.subscribe(value => {
      if (value) {
        if (this.fileHasVouchers.value) {
          this.importVouchers.enable();
        }
        if (this.fileHasAccounts.value) {
          this.importAccountBalances.enable();
        }
      } else {
        this.importVouchers.disable();
        this.importAccountBalances.disable();
      }
    });

    this.importAccounts.valueChanges.subscribe(value => {
      if (value) {
        this.importAccountStd.enable();
        this.importAccountInternal.enable();
        this.overrideNameConflicts.enable();
        this.approveEmptyAccountNames.enable();
      } else {
        this.importAccountStd.disable();
        this.importAccountInternal.disable();
        this.overrideNameConflicts.disable();
        this.approveEmptyAccountNames.disable();
      }
    });

    this.importAccountInternal.valueChanges.subscribe(value => {
      if (this.preview && this.preview.accountDims) {
        this.preview.accountDims.controls.forEach(control => {
          control.isImport.patchValue(value, { emitEvent: false });
          if (value && this.importAccounts.value) {
            control.isImport.enable();
          } else {
            control.isImport.disable();
          }
        });
      }
    });

    this.importVouchers.valueChanges.subscribe(value => {
      if (value) {
        this.defaultVoucherSeriesId.enable();
        this.overrideVoucherSeries.enable();
        this.useAccountDistribution.enable();
        this.skipAlreadyExistingVouchers.enable();
        this.takeVoucherNrFromSeries.enable();
        this.voucherSeriesDelete.enable({ emitEvent: false });
        this.overrideVoucherSeriesDelete.enable({ emitEvent: false });
        this.enableVoucherSeriesMappings();
      } else {
        this.defaultVoucherSeriesId.disable();
        this.overrideVoucherSeries.disable();
        this.useAccountDistribution.disable();
        this.skipAlreadyExistingVouchers.disable();
        this.takeVoucherNrFromSeries.disable();
        this.voucherSeriesDelete.disable({ emitEvent: false });
        this.overrideVoucherSeriesDelete.disable({ emitEvent: false });
        this.disableVoucherSeriesMappings();
      }
    });

    this.importAccountBalances.valueChanges.subscribe(value => {
      if (value) {
        this.overrideAccountBalance.enable();
        this.useUBInsteadOfIB.enable();
      } else {
        this.overrideAccountBalance.disable();
        this.useUBInsteadOfIB.disable();
      }
    });

    this.overrideVoucherSeriesDelete.valueChanges.subscribe(value => {
      if (value) {
        this.voucherSeriesDelete.patchValue([], { emitEvent: false });
        this.voucherSeriesDelete.disable({ emitEvent: false });
      } else {
        this.voucherSeriesDelete.enable({ emitEvent: false });
      }
    });

    this.voucherSeriesDelete.valueChanges.subscribe(value => {
      if (value && value.length > 0) {
        this.overrideVoucherSeriesDelete.patchValue(false, {
          emitEvent: false,
        });
        this.overrideVoucherSeriesDelete.disable({ emitEvent: false });
      } else {
        this.overrideVoucherSeriesDelete.enable({ emitEvent: false });
      }
    });

    this.defaultVoucherSeriesId.valueChanges.subscribe(value => {
      const voucherSeries = this.voucherSeries.find(
        v => v.voucherSeriesId === value
      );
      if (value && voucherSeries) {
        this.voucherSeriesMappings.controls.forEach(control => {
          control.defaultVoucherSeriesChanged(
            voucherSeries.voucherSeriesTypeId
          );
        });
      }
    });
  }
  get file() {
    return this.controls.file as SoeFormControl<IFileDTO>;
  }

  get accountYearId(): SoeSelectFormControl {
    return this.controls.accountYearId as SoeSelectFormControl;
  }

  get allowNotOpenAccountYear() {
    return this.controls.allowNotOpenAccountYear as SoeCheckboxFormControl;
  }

  get fileHasAccounts() {
    return this.controls.fileHasAccounts as SoeCheckboxFormControl;
  }
  get importAccounts(): SoeSwitchFormControl {
    return this.controls.importAccounts as SoeCheckboxFormControl;
  }

  get importAccountStd(): SoeCheckboxFormControl {
    return this.controls.importAccountStd as SoeCheckboxFormControl;
  }

  get importAccountInternal(): SoeCheckboxFormControl {
    return this.controls.importAccountInternal as SoeCheckboxFormControl;
  }

  get overrideNameConflicts(): SoeCheckboxFormControl {
    return this.controls.overrideNameConflicts as SoeCheckboxFormControl;
  }

  get approveEmptyAccountNames(): SoeCheckboxFormControl {
    return this.controls.approveEmptyAccountNames as SoeCheckboxFormControl;
  }

  get fileHasVouchers() {
    return this.controls.fileHasVouchers as SoeCheckboxFormControl;
  }
  get importVouchers(): SoeSwitchFormControl {
    return this.controls.importVouchers as SoeCheckboxFormControl;
  }

  get defaultVoucherSeriesId(): SoeSelectFormControl {
    return this.controls.defaultVoucherSeriesId as SoeSelectFormControl;
  }

  get overrideVoucherSeries(): SoeCheckboxFormControl {
    return this.controls.overrideVoucherSeries as SoeCheckboxFormControl;
  }

  get useAccountDistribution(): SoeCheckboxFormControl {
    return this.controls.useAccountDistribution as SoeCheckboxFormControl;
  }

  get skipAlreadyExistingVouchers(): SoeCheckboxFormControl {
    return this.controls.skipAlreadyExistingVouchers as SoeCheckboxFormControl;
  }

  get takeVoucherNrFromSeries() {
    return this.controls.takeVoucherNrFromSeries as SoeCheckboxFormControl;
  }

  get overrideVoucherSeriesDelete(): SoeCheckboxFormControl {
    return this.controls.overrideVoucherSeriesDelete as SoeCheckboxFormControl;
  }

  get voucherSeriesDelete(): SoeSelectFormControl {
    return this.controls.voucherSeriesDelete as SoeSelectFormControl;
  }

  get voucherSeriesMappings() {
    return this.controls
      .voucherSeriesMapping as FormArray<SieImportVoucheriesMappingForm>;
  }

  get fileHasAccountBalances() {
    return this.controls.fileHasAccountBalances as SoeCheckboxFormControl;
  }

  get importAccountBalances(): SoeSwitchFormControl {
    return this.controls.importAccountBalances as SoeCheckboxFormControl;
  }

  get overrideAccountBalance(): SoeCheckboxFormControl {
    return this.controls.overrideAccountBalance as SoeCheckboxFormControl;
  }

  get useUBInsteadOfIB(): SoeCheckboxFormControl {
    return this.controls.useUBInsteadOfIB as SoeCheckboxFormControl;
  }

  get preview(): SieImportPreviewForm {
    return this.controls.preview as SieImportPreviewForm;
  }

  setVoucherSeries(voucherSeries: IVoucherSeriesDTO[]) {
    this.voucherSeries = voucherSeries;
    this.voucherSeriesMappings.controls.forEach(control => {
      control.voucherSeriesChanged(voucherSeries);
    });
  }

  addVoucherSeriesMappings(items: ISieVoucherSeriesMappingDTO[]) {
    items.forEach(item => {
      this.voucherSeriesMappings.push(
        new SieImportVoucheriesMappingForm({
          validationHandler: this.sieValidationHandler,
          element: item,
        })
      );
    });
  }

  enableVoucherSeriesMappings() {
    this.voucherSeriesMappings.controls.forEach(control => {
      control.enable();
      control.setRequired();
    });
  }

  disableVoucherSeriesMappings() {
    this.voucherSeriesMappings.controls.forEach(control => {
      control.disable();
      control.setNotRequired();
    });
  }

  doReset() {
    this.reset();
    this.voucherSeriesMappings.clear();
    this.voucherSeriesDelete.patchValue([], { emitEvent: false });
  }
}
