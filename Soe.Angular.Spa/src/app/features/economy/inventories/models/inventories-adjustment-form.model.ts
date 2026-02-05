import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryAdjustmentDTO } from './inventories.model';
import {
  FormArray,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { AccountingRowsForm } from '@shared/components/accounting-rows/accounting-rows/accounting-rows-form.model';

interface IInventoriesAdjustmentForm {
  validationHandler: ValidationHandler;
  element: InventoryAdjustmentDTO | undefined;
}
export class InventoriesAdjustmentForm extends SoeFormGroup {
  InventoryAdjustmentValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IInventoriesAdjustmentForm) {
    super(validationHandler, {
      inventoryId: new SoeTextFormControl(element?.inventoryId || 0, {
        isIdField: true,
      }),
      amount: new SoeNumberFormControl(
        element?.amount || 0,
        {
          required: true,
          decimals: 2,
          zeroNotAllowed: true,
        },
        'common.amount'
      ),
      adjustmentDate: new SoeDateFormControl(
        element?.adjustmentDate || new Date(),
        { required: true },
        'economy.inventory.inventories.adjustmentdate'
      ),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId || undefined,
        {},
        'economy.inventory.inventorywriteofftemplate.voucherserie'
      ),
      adjustmentType: new SoeTextFormControl(element?.adjustmentType || ''),
      noteText: new SoeTextFormControl(
        element?.noteText || '',
        {},
        'common.note'
      ),
      accountingRows: new FormArray([]),
      isDisposed: new SoeCheckboxFormControl(false),
    });
    this.InventoryAdjustmentValidationHandler = validationHandler;
    this.isDisposed.valueChanges.subscribe(value => {
      this.voucherSeriesTypeId.clearValidators();
      this.voucherSeriesTypeId.clearAsyncValidators();
      if (value === true) {
        this.voucherSeriesTypeId.addValidators(Validators.required);
        this.voucherSeriesTypeId.addAsyncValidators(
          SoeFormControl.validateZeroNotAllowed()
        );
      }
      this.voucherSeriesTypeId.updateValueAndValidity();
    });
  }

  get inventoryId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryId;
  }
  get purchaseDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.purchaseDate;
  }
  get adjustmentDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.adjustmentDate;
  }
  get noteText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.noteText;
  }
  get accountingRows(): FormArray {
    return <FormArray>this.controls.accountingRows;
  }
  get voucherSeriesTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }
  get isDisposed(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isDisposed;
  }

  customPatchValue(element: InventoryAdjustmentDTO) {
    this.patchAccountingRows(element.accountingRows);
  }

  patchAccountingRow(
    r: AccountingRowDTO | undefined,
    updateValueAndValidity = true
  ) {
    if (r) {
      this.accountingRows.push(
        new AccountingRowsForm({
          validationHandler: this.InventoryAdjustmentValidationHandler,
          element: r,
        }),
        { emitEvent: false }
      );
      if (updateValueAndValidity) this.accountingRows.updateValueAndValidity();
    }
  }

  patchAccountingRows(rows: AccountingRowDTO[] | undefined) {
    this.accountingRows?.clear();
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.patchAccountingRow(r, false);
      });
      this.accountingRows.updateValueAndValidity();
    }
  }
}

export function addAdjustmentDateValidator(errorTerm: string): ValidatorFn {
  return (_form): ValidationErrors | null => {
    const adjustmentDate = _form.get('adjustmentDate')?.value;
    const purchaseDate = _form.get('purchaseDate')?.value;
    if (adjustmentDate < purchaseDate) {
      const error: ValidationErrors = {};
      error[errorTerm] = true;
      return error;
    }

    return null;
  };
}

export function debitCreditBalanceValidationError(): ValidatorFn {
  return (): ValidationErrors | null => {
    return {
      custom: {
        translationKey: 'economy.accounting.voucher.unbalancedrows',
      },
    };
  };
}
