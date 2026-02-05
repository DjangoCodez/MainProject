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
import { AccountDistributionHeadDTO } from './account-distribution.model';
import {
  SoeAccountDistributionType,
  TermGroup_AccountDistributionCalculationType,
  TermGroup_AccountDistributionPeriodType,
  TermGroup_AccountDistributionTriggerType,
  WildCard,
} from '@shared/models/generated-interfaces/Enumerations';
import { FormArray, Validators } from '@angular/forms';
import { AccountDistributionRowForm } from './account-distribution-row-form.model';
import { IAccountDistributionRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';
import { AccountDistributionValidators } from './account-distribution-validators';

interface IAccountDistributionForm {
  validationHandler: ValidationHandler;
  element: AccountDistributionHeadDTO | undefined;
  accountDistributionType?: SoeAccountDistributionType;
}

export class AccountDistributionForm extends SoeFormGroup {
  accountDimValidationHandler: ValidationHandler;
  constructor({
    validationHandler,
    element,
    accountDistributionType,
  }: IAccountDistributionForm) {
    super(validationHandler, {
      accountDistributionHeadId: new SoeTextFormControl(
        element?.accountDistributionHeadId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      startDate: new SoeDateFormControl(
        element?.startDate ||
          (accountDistributionType == SoeAccountDistributionType.Period
            ? DateUtil.getDateFirstInMonth(new Date())
            : undefined),
        {
          required: true,
        },
        'economy.accounting.accountdistribution.startdate'
      ),
      type: new SoeTextFormControl(
        element?.type || SoeAccountDistributionType.Period
      ),
      triggerType: new SoeSelectFormControl(
        element?.triggerType ||
          TermGroup_AccountDistributionTriggerType.Registration
      ),
      periodType: new SoeSelectFormControl(
        element?.periodType || TermGroup_AccountDistributionPeriodType.Period,
        {},
        'economy.accounting.accountdistribution.periodtype'
      ),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId,
        {
          required:
            accountDistributionType == SoeAccountDistributionType.Period,
        },
        'economy.accounting.accountdistribution.voucherserie'
      ),
      calculationTypeId: new SoeSelectFormControl(
        element?.calculationType ||
          TermGroup_AccountDistributionCalculationType.Percent,
        {
          required: true,
        },
        'economy.accounting.accountdistribution.calculationtype'
      ),
      endDate: new SoeDateFormControl(element?.endDate || null),
      dayNumber: new SoeTextFormControl(
        element?.dayNumber || 31,
        {},
        'economy.accounting.accountdistribution.dayinperiod'
      ),
      numberOfTimes: new SoeTextFormControl(
        element?.periodValue || 1,
        {},
        'economy.accounting.accountdistribution.periodvalue'
      ),
      description: new SoeTextFormControl(element?.description || ''),

      //account dims
      dim1Expression: new SoeNumberFormControl(element?.dim1Expression || ''),
      dim2Expression: new SoeNumberFormControl(element?.dim2Expression || ''),
      dim3Expression: new SoeNumberFormControl(element?.dim3Expression || ''),
      dim4Expression: new SoeNumberFormControl(element?.dim4Expression || ''),
      dim5Expression: new SoeNumberFormControl(element?.dim5Expression || ''),
      dim6Expression: new SoeNumberFormControl(element?.dim6Expression || ''),

      amountOperator: new SoeNumberFormControl(
        element?.amountOperator || WildCard.Equals
      ),
      amount: new SoeNumberFormControl(element?.amount || 0),
      keepRow: new SoeCheckboxFormControl(element?.keepRow || false),

      //Use in
      useInVoucher: new SoeCheckboxFormControl(element?.useInVoucher),
      useInCustomerInvoice: new SoeCheckboxFormControl(
        element?.useInCustomerInvoice
      ),
      useInPayrollVoucher: new SoeCheckboxFormControl(
        element?.useInPayrollVoucher
      ),
      useInSupplierInvoice: new SoeCheckboxFormControl(
        element?.useInSupplierInvoice
      ),
      useInImport: new SoeCheckboxFormControl(element?.useInImport),
      useInPayrollVacationVoucher: new SoeCheckboxFormControl(
        element?.useInPayrollVacationVoucher
      ),
      rows: new FormArray<AccountDistributionRowForm>([]),
      sort: new SoeNumberFormControl(element?.sort || 0),
      calculationType: new SoeNumberFormControl(
        element?.calculationType ||
          TermGroup_AccountDistributionCalculationType.Percent,
        { required: true, zeroNotAllowed: true },
        'economy.accounting.accountdistribution.calculationtype'
      ),
      dim1Id: new SoeNumberFormControl(element?.dim1Id || undefined),
      dim2Id: new SoeNumberFormControl(element?.dim2Id || undefined),
      dim3Id: new SoeNumberFormControl(element?.dim3Id || undefined),
      dim4Id: new SoeNumberFormControl(element?.dim4Id || undefined),
      dim5Id: new SoeNumberFormControl(element?.dim5Id || undefined),
      dim6Id: new SoeNumberFormControl(element?.dim6Id || undefined),

      //GridFilter
      showOpen: new SoeCheckboxFormControl(element?.showOpen || true),
      showClosed: new SoeCheckboxFormControl(element?.showClosed || false),
    });
    this.accountDimValidationHandler = validationHandler;

    this.type.valueChanges.subscribe(value => {
      this.togglePeriodAccountingRequiredValidators(value);
    });
  }

  //#region Getters
  get accountDistributionHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountDistributionHeadId;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }
  get startDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.startDate;
  }
  get endDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.endDate;
  }
  get triggerType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.triggerType;
  }
  get periodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.periodType;
  }
  get voucherSeriesTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }
  get dayNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dayNumber;
  }
  get numberOfTimes(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.numberOfTimes;
  }
  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get amount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amount;
  }
  get dim1Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1Expression;
  }
  get dim2Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim2Expression;
  }
  get dim3Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim3Expression;
  }
  get dim4Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim4Expression;
  }
  get dim5Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim5Expression;
  }
  get dim6Expression(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim6Expression;
  }

  get sort(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sort;
  }
  get amountOperator(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountOperator;
  }
  get calculationType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.calculationType;
  }
  get dim1Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1Id;
  }
  get dim2Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim2Id;
  }
  get dim3Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim3Id;
  }
  get dim4Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim4Id;
  }
  get dim5Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim5Id;
  }
  get dim6Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim6Id;
  }
  get keepRow(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.keepRow;
  }
  get rows(): FormArray<AccountDistributionRowForm> {
    return <FormArray>this.controls.rows;
  }

  get useInVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInVoucher;
  }
  get useInSupplierInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInSupplierInvoice;
  }
  get useInCustomerInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInCustomerInvoice;
  }
  get useInImport(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInImport;
  }
  get useInPayrollVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInPayrollVoucher;
  }
  get useInPayrollVacationVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInPayrollVacationVoucher;
  }

  get showOpen(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showOpen;
  }
  get showClosed(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showClosed;
  }
  //#endregion

  private togglePeriodAccountingRequiredValidators(
    value: SoeAccountDistributionType
  ): void {
    const controls = [
      this.voucherSeriesTypeId,
      this.periodType,
      this.dayNumber,
      this.startDate,
    ];

    if (value === SoeAccountDistributionType.Period) {
      this.addPeriodAccountingRequiredValidators(controls);
      this.addValidators([
        AccountDistributionValidators.periodAccountingPeriod(),
        AccountDistributionValidators.periodAccountingRowsDiff(),
      ]);
    } else {
      this.removePeriodAccountingRequiredValidators(controls);
      this.removeValidators([
        AccountDistributionValidators.periodAccountingPeriod(),
        AccountDistributionValidators.periodAccountingRowsDiff(),
      ]);
    }
    this.updateValueAndValidity();
  }

  private addPeriodAccountingRequiredValidators(
    controls: Array<SoeFormControl>
  ): void {
    controls.forEach(control => {
      control.setValidators([Validators.required]);
      control.updateValueAndValidity();
    });
  }

  private removePeriodAccountingRequiredValidators(
    controls: Array<SoeFormControl>
  ): void {
    controls.forEach(control => {
      control.clearValidators();
      control.updateValueAndValidity();
    });
  }

  customPatchValue(element: any) {
    this.patchValue(element);
    this.numberOfTimes.patchValue(element.periodValue || 1);
    this.rows.clear();
    this.patchAccountingRows(element.rows);
  }

  patchAccountingRows(rows: IAccountDistributionRowDTO[]) {
    for (const row of rows) {
      const accountDimRow = new AccountDistributionRowForm({
        validationHandler: this.accountDimValidationHandler,
        element: row,
      });
      this.rows.push(accountDimRow, { emitEvent: false });
    }
    this.rows.updateValueAndValidity();
  }

  addDistributionAccountingRow(
    row: IAccountDistributionRowDTO
  ): IAccountDistributionRowDTO {
    //get maxrow id from the rows
    const maxRowId =
      this.rows.length > 0
        ? (
            this.rows.at(this.rows.length - 1)
              .value as IAccountDistributionRowDTO
          ).accountDistributionRowId
        : 0;
    row.accountDistributionRowId = maxRowId + 1;

    const accountDimRow = new AccountDistributionRowForm({
      validationHandler: this.accountDimValidationHandler,
      element: row,
    });

    this.rows.push(accountDimRow, { emitEvent: false });
    this.rows.updateValueAndValidity();

    return row;
  }

  updateDistributionAccountingRow(row: IAccountDistributionRowDTO): void {
    const index = this.rows.controls.findIndex(
      r => r.value.accountDistributionRowId === row.accountDistributionRowId
    );
    if (index !== -1) {
      const accountDimRow = new AccountDistributionRowForm({
        validationHandler: this.accountDimValidationHandler,
        element: row,
      });
      this.rows.setControl(index, accountDimRow, { emitEvent: false });
      this.rows.updateValueAndValidity();
    }
  }

  resetDistributionAccountingRow(rows: IAccountDistributionRowDTO[]): void {
    this.rows.clear();
    this.patchAccountingRows(rows);
  }

  getDistributionRowsForCopy(): IAccountDistributionRowDTO[] {
    if (!this.rows || this.rows.length === 0) {
      return [];
    }
    let rowId = 0;
    return this.rows.value.map(
      (row: IAccountDistributionRowDTO): IAccountDistributionRowDTO => {
        row.accountDistributionHeadId = 0;
        row.accountDistributionRowId = ++rowId;
        return row;
      }
    );
  }
}

export class PeriodAccountDistributionForm extends AccountDistributionForm {
  constructor({ validationHandler, element }: IAccountDistributionForm) {
    super({
      validationHandler,
      element,
      accountDistributionType: SoeAccountDistributionType.Period,
    });
  }
}

export class AutoAccountDistributionForm extends AccountDistributionForm {
  constructor({ validationHandler, element }: IAccountDistributionForm) {
    super({
      validationHandler,
      element,
      accountDistributionType: SoeAccountDistributionType.Auto,
    });
  }
}
