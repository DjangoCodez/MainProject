import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  AccountInternalDTO,
  VoucherHeadDTO,
  VoucherRowDTO,
} from './voucher.model';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';
import { AccountingRowsForm } from '@shared/components/accounting-rows/accounting-rows/accounting-rows-form.model';

interface IVoucherForm {
  validationHandler: ValidationHandler;
  element: VoucherHeadDTO | undefined;
}

interface IVoucherRowsForm {
  validationHandler: ValidationHandler;
  element: VoucherRowDTO | undefined;
}

interface IAccountInternalDTOForReportsForm {
  validationHandler: ValidationHandler;
  element: AccountInternalDTO | undefined;
}

interface IAccountIdsForm {
  validationHandler: ValidationHandler;
  element: number | undefined;
}

interface IKeepVoucherOpenAfterSaveForm {
  validationHandler: ValidationHandler;
  element: number | false;
}

export class VoucherForm extends SoeFormGroup {
  voucherValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IVoucherForm) {
    super(validationHandler, {
      voucherHeadId: new SoeTextFormControl(element?.voucherHeadId || 0, {
        isIdField: true,
      }),
      voucherSeriesId: new SoeNumberFormControl(element?.voucherSeriesId, {
        required: true,
      }),
      accountPeriodId: new SoeNumberFormControl(element?.accountPeriodId, {}),

      voucherNr: new SoeNumberFormControl(
        element?.voucherNr || '',
        { isNameField: true },
        'common.number'
      ),
      date: new SoeDateFormControl(element?.date || undefined, {}),
      text: new SoeTextFormControl(element?.text || '', {}),
      template: new SoeCheckboxFormControl(element?.template || false, {}),
      typeBalance: new SoeCheckboxFormControl(
        element?.typeBalance || false,
        {}
      ),
      vatVoucher: new SoeCheckboxFormControl(element?.vatVoucher || false, {}),
      companyGroupVoucher: new SoeCheckboxFormControl(
        element?.companyGroupVoucher || false,
        {}
      ),
      voucherStatus: new SoeNumberFormControl(element?.status, {}),
      note: new SoeTextFormControl(element?.note || '', {}),
      sourceType: new SoeTextFormControl(element?.sourceType || undefined, {}),
      voucherSeriesTypeId: new SoeTextFormControl(
        element?.voucherSeriesTypeId,
        {}
      ),
      voucherSeriesTypeName: new SoeTextFormControl(
        element?.voucherSeriesTypeName || undefined,
        {}
      ),
      voucherSeriesTypeNr: new SoeTextFormControl(
        element?.voucherSeriesTypeNr || undefined,
        {}
      ),
      sourceTypeName: new SoeTextFormControl(
        element?.sourceTypeName || false,
        {}
      ),
      isSelected: new SoeCheckboxFormControl(element?.isSelected || false, {}),
      accountIdsHandled: new SoeCheckboxFormControl(
        element?.accountIdsHandled || false,
        {}
      ),
      accountYearId: new SoeNumberFormControl(element?.accountYearId, {}),
      budgetAccountId: new SoeNumberFormControl(element?.budgetAccountId, {}),
      templateId: new SoeNumberFormControl(element?.templateId, {}),
      accountingRows: new FormArray<AccountingRowsForm>([]),

      rows: new FormArray<VoucherRowsForm>([]),
      accountIds: new FormArray<AccountIdsForm>([]),
    });
    this.voucherValidationHandler = validationHandler;
  }

  //#region Custom patch values

  customPatchValue(element: VoucherHeadDTO) {
    if (element) {
      this.patchValue({ voucherStatus: element.status }, { emitEvent: false });
    }
  }

  customAccountingRowsPatchValue(accountingRows: AccountingRowDTO[]) {
    if (accountingRows) {
      (this.controls.accountingRows as FormArray).clear();

      if (accountingRows) {
        for (const accountingRow of accountingRows) {
          const row = new AccountingRowsForm({
            validationHandler: this.voucherValidationHandler,
            element: accountingRow,
          });
          (this.controls.accountingRows as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  customVoucherRowsPatchValue(voucherRows: VoucherRowDTO[]) {
    if (voucherRows && this.controls.voucherRows) {
      (this.controls.voucherRows as FormArray).clear();

      if (voucherRows) {
        for (const voucherRow of voucherRows) {
          const row = new VoucherRowsForm({
            validationHandler: this.voucherValidationHandler,
            element: voucherRow,
          });
          (this.controls.voucherRows as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  customAccountIdsPatchValue(accountIds: number[]) {
    if (accountIds) {
      (this.controls.accountIds as FormArray).clear();

      if (accountIds) {
        for (const accountId of accountIds) {
          const row = new AccountIdsForm({
            validationHandler: this.voucherValidationHandler,
            element: accountId,
          });
          (this.controls.accountIds as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  //#endregion

  //#region Getters of controls

  get accountingRows(): FormArray<AccountingRowsForm> {
    return <FormArray>this.controls.accountingRows;
  }

  get rows(): FormArray<VoucherRowsForm> {
    return <FormArray>this.controls.rows;
  }

  get accountIds(): FormArray<AccountIdsForm> {
    return <FormArray>this.controls.accountIds;
  }

  get voucherHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherHeadId;
  }
  get voucherSeriesId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherSeriesId;
  }
  get accountPeriodId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountPeriodId;
  }
  get voucherNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherNr;
  }
  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }
  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
  get template(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.template;
  }
  get typeBalance(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.typeBalance;
  }
  get vatVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.vatVoucher;
  }
  get companyGroupVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.companyGroupVoucher;
  }
  get voucherStatus(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherStatus;
  }
  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }
  get sourceType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sourceType;
  }
  get voucherSeriesTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeId;
  }
  get voucherSeriesTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeName;
  }
  get voucherSeriesTypeNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeNr;
  }
  get sourceTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sourceTypeName;
  }
  get isSelected(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelected;
  }
  get accountIdsHandled(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.accountIdsHandled;
  }
  get accountYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountYearId;
  }
  get budgetAccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.budgetAccountId;
  }
  get templateId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.templateId;
  }

  //#endregion
}

export function addDebitCreditBalanceValidator(errorTerm: string): ValidatorFn {
  return (_form): ValidationErrors | null => {
    _form.clearValidators();
    const error: ValidationErrors = {};
    error[errorTerm] = true;
    return error;
  };
}

export class VoucherRowsForm extends SoeFormGroup {
  voucherValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IVoucherRowsForm) {
    super(validationHandler, {
      voucherRowId: new SoeTextFormControl(element?.voucherRowId || 0, {
        isIdField: true,
      }),

      voucherHeadId: new SoeNumberFormControl(element?.voucherHeadId, {}),
      parentRowId: new SoeNumberFormControl(element?.parentRowId, {}),
      accountDistributionHeadId: new SoeNumberFormControl(
        element?.accountDistributionHeadId,
        {}
      ),
      date: new SoeDateFormControl(element?.date || undefined, {}),
      text: new SoeTextFormControl(element?.text || '', {}),
      quantity: new SoeNumberFormControl(element?.quantity, {}),
      amount: new SoeNumberFormControl(element?.amount, {}),
      amountEntCurrency: new SoeNumberFormControl(
        element?.amountEntCurrency,
        {}
      ),
      merged: new SoeCheckboxFormControl(element?.merged || false, {}),
      state: new SoeNumberFormControl(element?.state, {}),
      voucherNr: new SoeNumberFormControl(element?.voucherNr, {}),
      voucherSeriesTypeNr: new SoeTextFormControl(
        element?.voucherSeriesTypeNr || undefined,
        {}
      ),
      voucherSeriesTypeName: new SoeTextFormControl(
        element?.voucherSeriesTypeName || undefined,
        {}
      ),
      tempRowId: new SoeNumberFormControl(element?.tempRowId, {}),
      startDate: new SoeDateFormControl(element?.startDate || undefined, {}),
      numberOfPeriods: new SoeNumberFormControl(
        element?.numberOfPeriods || undefined,
        {}
      ),

      dim1Id: new SoeNumberFormControl(element?.dim1Id, {}),
      dim1Nr: new SoeTextFormControl(element?.dim1Nr || '', {}),
      dim1Name: new SoeTextFormControl(element?.dim1Name || '', {}),
      dim1UnitStop: new SoeCheckboxFormControl(
        element?.dim1UnitStop || false,
        {}
      ),
      dim1AmountStop: new SoeNumberFormControl(element?.dim1AmountStop, {}),
      dim2Id: new SoeNumberFormControl(element?.dim2Id, {}),
      dim2Nr: new SoeTextFormControl(element?.dim2Nr || '', {}),
      dim2Name: new SoeTextFormControl(element?.dim2Name || '', {}),
      dim3Id: new SoeNumberFormControl(element?.dim3Id, {}),
      dim3Nr: new SoeTextFormControl(element?.dim3Nr || '', {}),
      dim3Name: new SoeTextFormControl(element?.dim3Name || '', {}),
      dim4Id: new SoeNumberFormControl(element?.dim4Id, {}),
      dim4Nr: new SoeTextFormControl(element?.dim4Nr || '', {}),
      dim4Name: new SoeTextFormControl(element?.dim4Name || '', {}),
      dim5Id: new SoeNumberFormControl(element?.dim5Id, {}),
      dim5Nr: new SoeTextFormControl(element?.dim5Nr || '', {}),
      dim5Name: new SoeTextFormControl(element?.dim5Name || '', {}),
      dim6Id: new SoeNumberFormControl(element?.dim6Id, {}),
      dim6Nr: new SoeTextFormControl(element?.dim6Nr || '', {}),
      dim6Name: new SoeTextFormControl(element?.dim6Name || '', {}),
      rowNr: new SoeNumberFormControl(element?.rowNr, {}),
      sysVatAccountId: new SoeNumberFormControl(element?.sysVatAccountId, {}),
      dim1AccountType: new SoeNumberFormControl(element?.dim1AccountType, {}),
      amountCredit: new SoeNumberFormControl(element?.amountCredit, {}),
      amountDebet: new SoeNumberFormControl(element?.amountDebet, {}),
      accountInternalDTO_forReports:
        new FormArray<AccountInternalDTOForReportsForm>([]),
    });
    this.voucherValidationHandler = validationHandler;
    if (element?.accountInternalDTO_forReports) {
      this.customAccountInternalDTOForReportsPatchValue(
        element?.accountInternalDTO_forReports
      );
    }
  }

  customAccountInternalDTOForReportsPatchValue(
    accountingInternalRows: AccountInternalDTO[]
  ) {
    if (accountingInternalRows) {
      (this.controls.accountInternalDTO_forReports as FormArray).clear();

      if (accountingInternalRows) {
        for (const accountingInternalRow of accountingInternalRows) {
          const row = new AccountInternalDTOForReportsForm({
            validationHandler: this.voucherValidationHandler,
            element: accountingInternalRow,
          });
          (this.controls.accountInternalDTO_forReports as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  get voucherRowId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherRowId;
  }
  get voucherHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherHeadId;
  }
  get parentRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.parentRowId;
  }
  get accountDistributionHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDistributionHeadId;
  }
  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }
  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
  get quantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.quantity;
  }
  get amount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amount;
  }
  get amountEntCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountEntCurrency;
  }
  get merged(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.merged;
  }
  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }
  get voucherNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherNr;
  }
  get voucherSeriesTypeNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeNr;
  }
  get voucherSeriesTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeName;
  }
  get tempRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.tempRowId;
  }

  get dim1Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1Id;
  }
  get dim1Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim1Nr;
  }
  get dim1Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim1Name;
  }
  get dim1UnitStop(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.dim1UnitStop;
  }
  get dim1AmountStop(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1AmountStop;
  }
  get dim2Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim2Id;
  }
  get dim2Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim2Nr;
  }
  get dim2Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim2Name;
  }
  get dim3Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim3Id;
  }
  get dim3Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim3Nr;
  }
  get dim3Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim3Name;
  }
  get dim4Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim4Id;
  }
  get dim4Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim4Nr;
  }
  get dim4Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim4Name;
  }
  get dim5Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim5Id;
  }
  get dim5Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim5Nr;
  }
  get dim5Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim5Name;
  }
  get dim6Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim6Id;
  }
  get dim6Nr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim6Nr;
  }
  get dim6Name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim6Name;
  }
  get rowNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.rowNr;
  }
  get sysVatAccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysVatAccountId;
  }
  get dim1AccountType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1AccountType;
  }
  get amountCredit(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountCredit;
  }
  get amountDebet(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountDebet;
  }
  get accountInternalDTO_forReports(): FormArray<AccountInternalDTOForReportsForm> {
    return <FormArray>this.controls.accountInternalDTO_forReports;
  }
}

export class AccountInternalDTOForReportsForm extends SoeFormGroup {
  voucherValidationHandler: ValidationHandler;
  constructor({
    validationHandler,
    element,
  }: IAccountInternalDTOForReportsForm) {
    super(validationHandler, {
      accountId: new SoeTextFormControl(element?.accountId || 0, {
        isIdField: true,
      }),
      accountNr: new SoeTextFormControl(element?.accountNr || '', {}),
      name: new SoeNumberFormControl(element?.name || '', {
        isNameField: true,
      }),

      accountDimId: new SoeNumberFormControl(element?.accountDimId, {}),
      accountDimNr: new SoeNumberFormControl(element?.accountDimNr, {}),
      sysSieDimNr: new SoeNumberFormControl(element?.sysSieDimNr, {}),
      sysSieDimNrOrAccountDimNr: new SoeNumberFormControl(
        element?.sysSieDimNrOrAccountDimNr,
        {}
      ),
      mandatoryLevel: new SoeNumberFormControl(element?.mandatoryLevel, {}),
      useVatDeduction: new SoeCheckboxFormControl(
        element?.useVatDeduction || false,
        {}
      ),
      vatDeduction: new SoeNumberFormControl(element?.vatDeduction, {}),
    });
    this.voucherValidationHandler = validationHandler;
  }

  get accountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountId;
  }
  get accountNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountNr;
  }
  get name(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.name;
  }
  get accountDimId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDimId;
  }
  get accountDimNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDimNr;
  }
  get sysSieDimNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysSieDimNr;
  }
  get sysSieDimNrOrAccountDimNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysSieDimNrOrAccountDimNr;
  }
  get mandatoryLevel(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.mandatoryLevel;
  }
  get useVatDeduction(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useVatDeduction;
  }
  get vatDeduction(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.vatDeduction;
  }
}

export class AccountIdsForm extends SoeFormGroup {
  accountIdsValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IAccountIdsForm) {
    super(validationHandler, {
      id: new SoeTextFormControl(element || 0, {
        isIdField: true,
      }),
    });
    this.accountIdsValidationHandler = validationHandler;
  }

  get id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.id;
  }
}

export class KeepVoucherOpenAfterSaveForm extends SoeFormGroup {
  keepVoucherOpenAfterSaveValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IKeepVoucherOpenAfterSaveForm) {
    super(validationHandler, {
      keepVoucherOpenAfterSave: new SoeCheckboxFormControl(
        element || false,
        {}
      ),
    });
    this.keepVoucherOpenAfterSaveValidationHandler = validationHandler;
  }

  get keepVoucherOpenAfterSave(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.keepVoucherOpenAfterSave;
  }
}
