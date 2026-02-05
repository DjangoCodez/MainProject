import {
  SoeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CompanyGroupTransferHeadDTO } from './company-group-transfer.model';
import { arrayToFormArray } from '@shared/util/form-util';
import { FormArray, FormControl, Validators } from '@angular/forms';
import { ICompanyGroupTransferRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CompanyGroupTransferType } from '@shared/models/generated-interfaces/Enumerations';

interface ICompanyGroupTransferForm {
  validationHandler: ValidationHandler;
  element: CompanyGroupTransferHeadDTO | undefined;
}
export class CompanyGroupTransferForm extends SoeFormGroup {
  CompanyGroupTransferValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ICompanyGroupTransferForm) {
    super(validationHandler, {
      companyGroupTransferHeadId: new SoeTextFormControl(
        element?.companyGroupTransferHeadId || 0,
        {
          isIdField: true,
        }
      ),
      transferType: new SoeSelectFormControl(element?.transferType || 1),
      accountYearId: new SoeSelectFormControl(
        element?.accountYearId || undefined,
        { required: true, zeroNotAllowed: true },
        'economy.accounting.accountyear'
      ),
      fromAccountPeriodId: new SoeSelectFormControl(
        element?.fromAccountPeriodId || undefined
      ),
      toAccountPeriodId: new SoeSelectFormControl(
        element?.toAccountPeriodId || undefined
      ),
      voucherSeriesId: new SoeSelectFormControl(
        element?.voucherSeriesId || undefined
      ),
      masterBudgetId: new SoeSelectFormControl(
        element?.childBudgetId || undefined
      ),
      childCompanyId: new SoeSelectFormControl(
        element?.childCompanyId || undefined
      ),
      childBudgetId: new SoeSelectFormControl(
        element?.childBudgetId || undefined
      ),
      companyGroupTransferRows: arrayToFormArray(
        element?.companyGroupTransferRows || []
      ),
    });

    this.CompanyGroupTransferValidationHandler = validationHandler;
    this.transferType.valueChanges.subscribe(() => this.updateValidators());
    this.updateValidators();
  }

  get companyGroupTransferHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.companyGroupTransferHeadId;
  }

  get transferType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.transferType;
  }

  get accountYearId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearId;
  }

  get fromAccountPeriodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.fromAccountPeriodId;
  }

  get toAccountPeriodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.toAccountPeriodId;
  }

  get voucherSeriesId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesId;
  }

  get masterBudgetId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.masterBudgetId;
  }

  get childCompanyId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.childCompanyId;
  }

  get childBudgetId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.childBudgetId;
  }

  get companyGroupTransferRows(): FormArray<
    FormControl<ICompanyGroupTransferRowDTO>
  > {
    return <FormArray>this.controls.companyGroupTransferRows;
  }

  updateValidators(): void {
    this.clearDynamicValidators([
      this.voucherSeriesId,
      this.fromAccountPeriodId,
      this.toAccountPeriodId,
      this.masterBudgetId,
      this.childCompanyId,
    ]);

    if (this.transferType.value === CompanyGroupTransferType.Consolidation) {
      this.addDynamicValidators([
        this.voucherSeriesId,
        this.fromAccountPeriodId,
        this.toAccountPeriodId,
      ]);
    } else if (this.transferType.value === CompanyGroupTransferType.Budget) {
      this.addDynamicValidators([
        this.masterBudgetId,
        this.fromAccountPeriodId,
        this.childCompanyId,
      ]);
    }

    this.updateControlsValidity([
      this.voucherSeriesId,
      this.fromAccountPeriodId,
      this.toAccountPeriodId,
      this.masterBudgetId,
      this.childCompanyId,
    ]);
  }

  private addDynamicValidators(controls: Array<SoeSelectFormControl>): void {
    controls.forEach(control => {
      control.addValidators(Validators.required);
      control.addAsyncValidators(SoeFormControl.validateZeroNotAllowed());
    });
  }

  private clearDynamicValidators(controls: Array<SoeSelectFormControl>): void {
    controls.forEach(control => {
      control.clearAsyncValidators();
      control.clearValidators();
    });
  }

  private updateControlsValidity(controls: Array<SoeSelectFormControl>): void {
    controls.forEach(control => {
      control.updateValueAndValidity();
    });
  }
}
