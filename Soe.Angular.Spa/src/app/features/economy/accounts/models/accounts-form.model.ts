import {
  SoeCheckboxFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountEditDTO } from './accounts.model';
import { LanguageTranslationForm } from '@shared/features/language-translations/models/language-translations-form.model';
import { FormArray, Validators } from '@angular/forms';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import {
  IAccountMappingDTO,
  ICompTermDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AccountMappingForm } from './account-mapping-form.model';
import { addEmptyOption } from '@shared/util/array-util';

interface IAccountForm {
  validationHandler: ValidationHandler;
  element: AccountEditDTO | undefined;
}

export class AccountForm extends SoeFormGroup {
  translateValidationHandler: ValidationHandler;
  private _isStdAccount: boolean = false;

  get isStdAccount() {
    return this._isStdAccount;
  }

  set isStdAccount(value: boolean) {
    this._isStdAccount = value;
    this.resetAccountTypeSysTermIdValidators();
  }

  constructor({ validationHandler, element }: IAccountForm) {
    super(validationHandler, {
      accountId: new SoeTextFormControl(element?.accountId || 0, {
        isIdField: true,
      }),
      accountDimId: new SoeTextFormControl(element?.accountDimId || 0),
      accountNr: new SoeTextFormControl(
        element?.accountNr || '',
        { required: true, isNameField: true },
        'economy.accounting.accountnr'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || ''),
      externalCode: new SoeTextFormControl(element?.externalCode || ''),
      active: new SoeCheckboxFormControl(element?.active || true),
      translations: new FormArray<LanguageTranslationForm>([]),

      categoryIds: new SoeSelectFormControl(element?.categoryIds || []),

      attestWorkFlowHeadId: new SoeSelectFormControl(
        element?.attestWorkFlowHeadId || 0
      ),
      parentAccountId: new SoeTextFormControl(element?.parentAccountId || 0),
      useVatDeductionDim: new SoeCheckboxFormControl(
        element?.useVatDeductionDim || false
      ),
      useVatDeduction: new SoeCheckboxFormControl(
        element?.useVatDeduction || false
      ),
      vatDeduction: new SoeNumberFormControl(element?.vatDeduction || 0, {
        decimals: 2,
      }),
      accountMappings: new FormArray<AccountMappingForm>([]),
      sysAccountSruCode1Id: new SoeNumberFormControl(
        element?.sysAccountSruCode1Id || 0
      ),
      sysAccountSruCode2Id: new SoeNumberFormControl(
        element?.sysAccountSruCode2Id || 0
      ),
      sysVatAccountId: new SoeNumberFormControl(element?.sysVatAccountId || 0),
      accountTypeSysTermId: new SoeNumberFormControl(
        element?.accountTypeSysTermId || 0,
        {},
        'economy.accounting.accounttype'
      ),
      isAccrualAccount: new SoeCheckboxFormControl(
        element?.isAccrualAccount || false
      ),
      sysVatRate: new SoeTextFormControl(element?.sysVatRate || '', {
        disabled: true,
      }),
      amountStop: new SoeSelectFormControl(element?.amountStop || 0),
      unit: new SoeTextFormControl(element?.unit || ''),
      unitStop: new SoeCheckboxFormControl(element?.unitStop || false),
      rowTextStop: new SoeCheckboxFormControl(element?.rowTextStop || false),
      excludeVatVerification: new SoeCheckboxFormControl(
        element?.excludeVatVerification || false
      ),
      hierarchyOnly: new SoeCheckboxFormControl(
        element?.hierarchyOnly || false
      ),
      hierarchyNotOnSchedule: new SoeCheckboxFormControl(
        element?.hierarchyNotOnSchedule || false
      ),
    });
    this.translateValidationHandler = validationHandler;
    this.customPatchTranslations(element?.translations ?? []);
  }

  get name(): string {
    return this.controls.name.value;
  }

  get accountDimId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountDimId;
  }

  get accountId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountId;
  }

  get accountNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountNr;
  }

  get categoryIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.categoryIds;
  }

  get attestWorkFlowHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.attestWorkFlowHeadId;
  }

  get translations(): FormArray<LanguageTranslationForm> {
    return <FormArray>this.controls.translations;
  }

  get parentAccountId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.parentAccountId;
  }

  get useVatDeductionDim(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useVatDeductionDim;
  }

  get useVatDeduction(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useVatDeduction;
  }

  get vatDeduction(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.vatDeduction;
  }

  get accountMappings(): FormArray<AccountMappingForm> {
    return <FormArray<AccountMappingForm>>this.controls.accountMappings;
  }

  get sysAccountSruCode1Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysAccountSruCode1Id;
  }

  get sysAccountSruCode2Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysAccountSruCode2Id;
  }

  get sysVatAccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysVatAccountId;
  }

  get accountTypeSysTermId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountTypeSysTermId;
  }

  get isAccrualAccount(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isAccrualAccount;
  }

  get hierarchyOnly(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hierarchyOnly;
  }

  get hierarchyNotOnSchedule(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hierarchyNotOnSchedule;
  }

  get sysVatRate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysVatRate;
  }

  get accountMappingsValue(): IAccountMappingDTO[] {
    const accountMappings: IAccountMappingDTO[] = [];
    for (const accountMappingForm of this.accountMappings.controls) {
      const accountMapping = (
        accountMappingForm as AccountMappingForm
      ).getRawValue() as IAccountMappingDTO;
      accountMapping.accounts = accountMappingForm.accounts;
      accountMapping.mandatoryLevels = accountMappingForm.mandatoryLevels;
      accountMappings.push(accountMapping);
    }

    return accountMappings;
  }

  customPatchTranslations(compTermRows: ICompTermDTO[]) {
    this.translations?.clear();

    for (const compTerm of compTermRows) {
      const langageRow = new LanguageTranslationForm({
        validationHandler: this.translateValidationHandler,
        element: compTerm as CompTermDTO,
      });

      this.translations.push(langageRow, { emitEvent: false });
    }
    this.translations.updateValueAndValidity();
  }

  customPatchAccountMappings(accountMappings: IAccountMappingDTO[]) {
    this.accountMappings?.clear();

    for (const accountMapping of accountMappings) {
      addEmptyOption(accountMapping.accounts);
      const accountMappingRow = new AccountMappingForm({
        validationHandler: this.translateValidationHandler,
        element: accountMapping,
      });

      this.accountMappings.push(accountMappingRow);
    }
    this.accountMappings.updateValueAndValidity();
  }

  private resetAccountTypeSysTermIdValidators(): void {
    this.accountTypeSysTermId.clearValidators();
    this.accountTypeSysTermId.clearAsyncValidators();
    if (this.isStdAccount) {
      this.accountTypeSysTermId.addValidators([Validators.required]);
      this.accountTypeSysTermId.addAsyncValidators(
        SoeFormControl.validateZeroNotAllowed()
      );
    }
  }

  public resetFormForCopy(): void {
    this.resetTranslationsRelationIds();
    this.resetAccountMappingsRelationIds();
  }

  private resetTranslationsRelationIds(): void {
    this.translations.controls.forEach(translation => {
      const translationForm = translation as LanguageTranslationForm;
      translationForm.recordId.setValue(0);
    });
  }

  private resetAccountMappingsRelationIds(): void {
    this.accountMappings.controls.forEach(accountMapping => {
      const accountMappingForm = accountMapping as AccountMappingForm;
      accountMappingForm.patchValue({
        accountId: 0,
      });
    });
  }
}
