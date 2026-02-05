import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { BudgetHeadFlattenedDTO, BudgetRowFlattenedDTO } from './budget.model';
import { FormArray } from '@angular/forms';
import { BudgetRowForm } from './budget-row-form.model';
import {
  DistributionCodeBudgetType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';

interface IBudgetForm {
  validationHandler: ValidationHandler;
  element: BudgetHeadFlattenedDTO | undefined;
}

export class BudgetForm extends SoeFormGroup {
  budgetValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IBudgetForm) {
    super(validationHandler, {
      budgetHeadId: new SoeNumberFormControl(element?.budgetHeadId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, isNameField: true },
        'common.name'
      ),
      noOfPeriods: new SoeNumberFormControl(
        element?.noOfPeriods || 12,
        { required: true, zeroNotAllowed: true },
        'economy.accounting.budget.noofperiods'
      ),
      accountYearId: new SoeSelectFormControl(
        element?.accountYearId || 0,
        { disabled: element?.status == 2, zeroNotAllowed: true },
        'economy.accounting.accountyear.accountyear'
      ),
      distributionCodeHeadId: new SoeSelectFormControl(
        element?.distributionCodeHeadId || 0,
        { disabled: element?.status == 2 }
      ),
      useDim2: new SoeCheckboxFormControl(element?.useDim2 || false, {
        disabled: element?.status == 2,
      }),
      dim2Id: new SoeSelectFormControl(element?.dim2Id || 0, {
        disabled: element?.status == 2,
      }),
      useDim3: new SoeCheckboxFormControl(element?.useDim3 || false, {
        disabled: element?.status == 2,
      }),
      dim3Id: new SoeSelectFormControl(element?.dim3Id || 0, {
        disabled: element?.status == 2,
      }),
      status: new SoeNumberFormControl(
        element?.status || +SoeEntityState.Active
      ),
      type: new SoeNumberFormControl(
        element?.type || +DistributionCodeBudgetType.AccountingBudget
      ),

      Created: new SoeDateFormControl(element?.created),
      createdBy: new SoeTextFormControl(element?.createdBy || ''),
      modified: new SoeDateFormControl(element?.modified),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy || ''),

      rows: new FormArray<BudgetRowForm>([]),
    });

    this.budgetValidationHandler = validationHandler;
    this.customBudgetRowsPatchValues(
      <BudgetRowFlattenedDTO[]>element?.rows ?? []
    );
  }

  get budgetHeadId(): SoeNumberFormControl {
    return <SoeTextFormControl>this.controls.budgetHeadId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get noOfPeriods(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.noOfPeriods;
  }

  get accountYearId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearId;
  }

  get distributionCodeHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.distributionCodeHeadId;
  }

  get useDim2(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useDim2;
  }

  get dim2Id(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim2Id;
  }

  get useDim3(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useDim3;
  }

  get dim3Id(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim3Id;
  }

  get rows(): FormArray<BudgetRowForm> {
    return <FormArray>this.controls.rows;
  }

  get type(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.type;
  }

  get lockStatus(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.status;
  }
  get Created(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.Created;
  }
  get createdBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.createdBy;
  }
  get modified(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.modified;
  }
  get modifiedBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.modifiedBy;
  }

  customBudgetRowsPatchValues(bRows: BudgetRowFlattenedDTO[]) {
    this.rows.clear({ emitEvent: false });
    if (bRows) {
      for (const brw of bRows) {
        if (
          brw.budgetRowId !== null &&
          brw.budgetRowId !== undefined &&
          typeof brw.budgetRowId === 'number'
        ) {
          const row = new BudgetRowForm({
            validationHandler: this.budgetValidationHandler,
            element: brw,
          });
          if (brw.isDeleted) row.disable();
          this.rows.push(row, { emitEvent: false });
        }
      }
    }
    this.rows.updateValueAndValidity();
  }

  deleteBudgetRow(row: BudgetRowFlattenedDTO) {
    const budgetRows = <BudgetRowFlattenedDTO[]>this.rows.getRawValue();
    budgetRows.forEach((rw, i) => {
      if (rw.budgetRowId == row.budgetRowId) {
        rw.isDeleted = true;
        this.markAsDirty();
        this.rows.markAsDirty();
        this.rows.updateValueAndValidity();
      }
    });
    this.customBudgetRowsPatchValues(
      budgetRows.filter(x => !(x.isDeleted && x.budgetRowId <= 0))
    );
  }

  removeEmptyBudgetRows() {
    const rows: BudgetRowFlattenedDTO[] = [];
    const formRows = <BudgetRowFlattenedDTO[]>this.rows.getRawValue();
    formRows.forEach(b => {
      if (
        (
          (
            b.budgetRowId > 0 || !b.isDeleted
          ) 
          && 
          (b.totalAmount || b.dim1Id)
        )) {
        rows.push(b);
      }
    });
    this.customBudgetRowsPatchValues(rows);
  }

  addBudgetRow(): BudgetRowFlattenedDTO {
    const newRowDto = new BudgetRowFlattenedDTO(this.getNewRowId());
    newRowDto.budgetHeadId = this.budgetHeadId.value;
    newRowDto.budgetRowId = this.getNewRowId();
    newRowDto.dim2Id = this.dim2Id.value ?? 0;
    newRowDto.dim3Id = this.dim3Id.value ?? 0;
    newRowDto.distributionCodeHeadId = this.distributionCodeHeadId.value ?? 0;
    newRowDto.totalAmount = 0;
    newRowDto.isDeleted = false;
    newRowDto.budgetRowNr = 0;
    newRowDto.amount1 = 0;
    newRowDto.amount2 = 0;
    newRowDto.amount3 = 0;
    newRowDto.amount4 = 0;
    newRowDto.amount5 = 0;
    newRowDto.amount6 = 0;
    newRowDto.amount7 = 0;
    newRowDto.amount8 = 0;
    newRowDto.amount9 = 0;
    newRowDto.amount10 = 0;
    newRowDto.amount11 = 0;
    newRowDto.amount12 = 0;
    newRowDto.amount13 = 0;
    newRowDto.amount14 = 0;
    newRowDto.amount15 = 0;
    newRowDto.amount16 = 0;
    newRowDto.amount17 = 0;
    newRowDto.amount18 = 0;

    const newRow = new BudgetRowForm({
      validationHandler: this.budgetValidationHandler,
      element: newRowDto,
    });

    this.rows.push(newRow, { emitEvent: false });

    this.markAsDirty();
    this.markAsTouched();
    this.rows.markAsDirty();
    // this.rows.updateValueAndValidity();

    return newRowDto;
  }

  private getNewRowId(): number {
    let minId = 0;

    if (this.rows.value.length > 0) {
      minId = (<BudgetRowFlattenedDTO[]>this.rows.value)
        .map(x => x.budgetRowId)
        .reduce((a, b) => Math.min(a, b));
    }

    return minId > 0 ? 0 : --minId;
  }

  setDirtyOnbudgetRowChange(budgetRow: BudgetRowFlattenedDTO) {
    budgetRow.isModified = true;
    const totalAmount = this.getTotalAmount(budgetRow);
    // const totalAmount = budgetRow.totalAmount;
    this.rows.controls.forEach(x => {
      if (x.budgetRowId.value === budgetRow.budgetRowId) {
        x.patchValue({
          accountId: budgetRow.accountId,
          dim1Id: budgetRow.dim1Id,
          dim2Id: budgetRow.dim2Id,
          dim3Id: budgetRow.dim3Id,
          distributionCodeHeadId: budgetRow.distributionCodeHeadId,
          amount1: budgetRow.amount1,
          amount2: budgetRow.amount2,
          amount3: budgetRow.amount3,
          amount4: budgetRow.amount4,
          amount5: budgetRow.amount5,
          amount6: budgetRow.amount6,
          amount7: budgetRow.amount7,
          amount8: budgetRow.amount8,
          amount9: budgetRow.amount9,
          amount10: budgetRow.amount10,
          amount11: budgetRow.amount11,
          amount12: budgetRow.amount12,
          amount13: budgetRow.amount13,
          amount14: budgetRow.amount14,
          amount15: budgetRow.amount15,
          amount16: budgetRow.amount16,
          amount17: budgetRow.amount17,
          amount18: budgetRow.amount18,
          totalAmount,
          isModified: budgetRow.isModified,
        });
      }
    });
    this.markAsDirty();
    this.rows.markAsDirty();
    this.rows.markAsTouched();
    return totalAmount;
  }

  private getTotalAmount(budgetRow: BudgetRowFlattenedDTO) {
    const total =
      (budgetRow.amount1 || 0) +
      (budgetRow.amount2 || 0) +
      (budgetRow.amount3 || 0) +
      (budgetRow.amount4 || 0) +
      (budgetRow.amount5 || 0) +
      (budgetRow.amount6 || 0) +
      (budgetRow.amount7 || 0) +
      (budgetRow.amount8 || 0) +
      (budgetRow.amount9 || 0) +
      (budgetRow.amount10 || 0) +
      (budgetRow.amount11 || 0) +
      (budgetRow.amount12 || 0) +
      (budgetRow.amount13 || 0) +
      (budgetRow.amount14 || 0) +
      (budgetRow.amount15 || 0) +
      (budgetRow.amount16 || 0) +
      (budgetRow.amount17 || 0) +
      (budgetRow.amount18 || 0);
    return total;
  }

  resetAmounts(budgetRow: BudgetRowFlattenedDTO) {
    budgetRow.amount1 = 0;
    budgetRow.amount2 = 0;
    budgetRow.amount3 = 0;
    budgetRow.amount4 = 0;
    budgetRow.amount5 = 0;
    budgetRow.amount6 = 0;
    budgetRow.amount7 = 0;
    budgetRow.amount8 = 0;
    budgetRow.amount9 = 0;
    budgetRow.amount10 = 0;
    budgetRow.amount11 = 0;
    budgetRow.amount12 = 0;
    budgetRow.amount13 = 0;
    budgetRow.amount14 = 0;
    budgetRow.amount15 = 0;
    budgetRow.amount16 = 0;
    budgetRow.amount17 = 0;
    budgetRow.amount18 = 0;
  }

  lockUnlockFormControls(value: boolean) {
    if (value) {
      this.name.disable();
      this.noOfPeriods.disable();
      this.accountYearId.disable();
      this.useDim2.disable();
      this.useDim3.disable();
      this.dim2Id.disable();
      this.dim3Id.disable();
      this.distributionCodeHeadId.disable();
    } else {
      this.name.enable();
      this.noOfPeriods.enable();
      this.accountYearId.enable();
      this.useDim2.enable();
      this.useDim3.enable();
      if (this.useDim2.value) this.dim2Id.enable();
      if (this.useDim3.value) this.dim3Id.enable();
      this.distributionCodeHeadId.enable();
    }
  }
}
