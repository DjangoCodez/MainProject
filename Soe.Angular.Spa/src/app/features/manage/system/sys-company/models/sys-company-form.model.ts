import { ValidationHandler } from '@shared/handlers';
import {
  SysCompanyBankAccountDTO,
  SysCompanyDTO,
  SysCompanySettingDTO,
  SysCompanyUniqueValueDTO,
} from '../../../models/sysCompany.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { FormArray } from '@angular/forms';
import { SysCompanySettingForm } from './sys-company-setting-form.model';
import { SysCompanyBankAccountForm } from './sys-company-bank-account-form.model';
import { SoeSysEntityState } from '@shared/util/Enumerations';
import { SysCompanyUniqueValueForm } from './sys-company-unique-value-form.model';

interface ISysCompanyForm {
  validationHandler: ValidationHandler;
  element: SysCompanyDTO | undefined;
}

export class SysCompanyForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISysCompanyForm) {
    super(validationHandler, {
      sysCompanyId: new SoeTextFormControl(element?.sysCompanyId || 0, {
        isIdField: true,
      }),
      companyApiKey: new SoeTextFormControl(element?.companyApiKey),
      actorCompanyId: new SoeTextFormControl(element?.actorCompanyId || 0),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      number: new SoeTextFormControl(element?.number || ''),
      licenseId: new SoeTextFormControl(element?.licenseId || 0),
      licenseNumber: new SoeTextFormControl(
        element?.licenseNumber || '',
        {
          required: true,
        },
        'common.licensenr'
      ),
      licenseName: new SoeTextFormControl(
        element?.licenseName || '',
        {
          required: true,
        },
        'common.licensename'
      ),
      serverName: new SoeTextFormControl(
        element?.serverName || '',
        {
          required: true,
        },
        'common.server'
      ),
      dbName: new SoeTextFormControl(
        element?.dbName || '',
        {
          required: true,
        },
        'manage.system.syscompany.syscompdb'
      ),
      verifiedOrgNr: new SoeTextFormControl(element?.verifiedOrgNr),
      isSOP: new SoeCheckboxFormControl(element?.isSOP || false),
      sysCompanySettingDTOs: new FormArray<SysCompanySettingForm>([]),
      sysCompanyBankAccountDTOs: new FormArray<SysCompanyBankAccountForm>([]),
      sysCompanyUniqueValueDTOs: new FormArray<SysCompanyUniqueValueForm>([]),
      usesBankIntegration: new SoeCheckboxFormControl(
        element?.usesBankIntegration
      ),
    });
    this.thisValidationHandler = validationHandler;
  }

  get sysCompanyId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysCompanyId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get licenseNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.licenseNumber;
  }

  get licenseName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.licenseName;
  }

  get serverName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.serverName;
  }

  get dbName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dbName;
  }

  get isSOP(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSOP;
  }

  get sysCompanySettingDTOs(): FormArray<SysCompanySettingForm> {
    return <FormArray<SysCompanySettingForm>>(
      this.controls.sysCompanySettingDTOs
    );
  }

  get sysCompanyBankAccountDTOs(): FormArray<SysCompanyBankAccountForm> {
    return <FormArray<SysCompanyBankAccountForm>>(
      this.controls.sysCompanyBankAccountDTOs
    );
  }

  get sysCompanyUniqueValueDTOs(): FormArray<SysCompanyUniqueValueForm> {
    return <FormArray<SysCompanyUniqueValueForm>>(
      this.controls.sysCompanyUniqueValueDTOs
    );
  }

  customPatch(sysCompany: SysCompanyDTO): void {
    this.reset(sysCompany);
    this.patchSettings(sysCompany.sysCompanySettingDTOs);
    this.patchBankAccounts(sysCompany.sysCompanyBankAccountDTOs);
    this.patchUniqueValues(sysCompany.sysCompanyUniqueValueDTOs || []);
    this.markAsUntouched();
    this.markAsPristine();
  }

  patchSettings(settings?: SysCompanySettingDTO[]): void {
    this.sysCompanySettingDTOs.clear({ emitEvent: false });
    if (settings && settings.length > 0) {
      for (const setting of settings) {
        const settingRow = new SysCompanySettingForm({
          validationHandler: this.thisValidationHandler,
          element: setting,
        });
        this.sysCompanySettingDTOs.push(settingRow, { emitEvent: false });
      }
    }
    this.sysCompanySettingDTOs.updateValueAndValidity();
  }

  addCompanySetting(setting: SysCompanySettingDTO): void {
    this.sysCompanySettingDTOs.push(
      new SysCompanySettingForm({
        validationHandler: this.thisValidationHandler,
        element: setting,
      })
    );
    this.markAsDirty();
  }

  updateCompanySetting(index: number, setting: SysCompanySettingDTO): void {
    this.sysCompanySettingDTOs.at(index).patchValue({
      settingType: setting.settingType,
      stringValue: setting.stringValue,
      intValue: setting.intValue,
      boolValue: setting.boolValue,
      decimalValue: setting.decimalValue,
    });
    this.markAsDirty();
  }

  deleteCompanySetting(index: number): void {
    this.sysCompanySettingDTOs.removeAt(index);
    this.markAsDirty();
  }

  patchBankAccounts(bankAccounts?: SysCompanyBankAccountDTO[]): void {
    this.sysCompanyBankAccountDTOs.clear({ emitEvent: false });
    if (bankAccounts && bankAccounts.length > 0) {
      for (const bankAccount of bankAccounts) {
        const bankAccountForm = new SysCompanyBankAccountForm({
          validationHandler: this.thisValidationHandler,
          element: bankAccount,
        });
        this.sysCompanyBankAccountDTOs.push(bankAccountForm, {
          emitEvent: false,
        });
      }
    }
    this.sysCompanyBankAccountDTOs.updateValueAndValidity();
  }

  addBankAccount(bankAccount: SysCompanyBankAccountDTO): void {
    this.sysCompanyBankAccountDTOs.push(
      new SysCompanyBankAccountForm({
        validationHandler: this.thisValidationHandler,
        element: bankAccount,
      })
    );
    this.markAsDirty();
  }

  updateBankAccount(
    index: number,
    bankAccount: SysCompanyBankAccountDTO
  ): void {
    this.sysCompanyBankAccountDTOs.at(index).patchValue({
      sysBankId: bankAccount.sysBankId,
      sysCompanyId: bankAccount.sysCompanyId,
      accountType: bankAccount.accountType,
      paymentNr: bankAccount.paymentNr,
      state: bankAccount.state,
    });
    this.markAsDirty();
  }

  deleteBankAccount(index: number): void {
    this.sysCompanyBankAccountDTOs.at(index).patchValue({
      state: SoeSysEntityState.Deleted,
    });
    this.markAsDirty();
  }

  patchUniqueValues(uniqueValues?: SysCompanyUniqueValueDTO[]): void {
    this.sysCompanyUniqueValueDTOs.clear({ emitEvent: false });
    if (uniqueValues && uniqueValues.length > 0) {
      for (const uniqueValue of uniqueValues) {
        const uniqueValueForm = new SysCompanyUniqueValueForm({
          validationHandler: this.thisValidationHandler,
          element: uniqueValue,
        });
        this.sysCompanyUniqueValueDTOs.push(uniqueValueForm, {
          emitEvent: false,
        });
      }
    }
    this.sysCompanyUniqueValueDTOs.updateValueAndValidity();
  }

  addUniqueValue(uniqueValue: SysCompanyUniqueValueDTO): void {
    this.sysCompanyUniqueValueDTOs.push(
      new SysCompanyUniqueValueForm({
        validationHandler: this.thisValidationHandler,
        element: uniqueValue,
      })
    );
    this.markAsDirty();
  }

  updateUniqueValue(
    index: number,
    uniqueValue: SysCompanyUniqueValueDTO
  ): void {
    this.sysCompanyUniqueValueDTOs.at(index).patchValue({
      sysCompanyId: uniqueValue.sysCompanyId,
      uniqueValueType: uniqueValue.uniqueValueType,
      value: uniqueValue.value,
    });
    this.markAsDirty();
  }

  deleteUniqueValue(index: number): void {
    this.sysCompanyUniqueValueDTOs.at(index).patchValue({
      state: SoeSysEntityState.Deleted,
    });
    this.markAsDirty();
  }
}
