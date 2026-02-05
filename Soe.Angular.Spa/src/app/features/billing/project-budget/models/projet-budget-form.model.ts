import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { FormArray } from '@angular/forms';
import { ProjectBudgetRowForm } from './project-budget-row-form.model';
import {
  DistributionCodeBudgetType,
  SoeEntityState,
  TermGroup_ProjectBudgetPeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  BudgetHeadProjectDTO,
  BudgetRowProjectDTO,
} from './project-budget.model';

interface IBudgetForm {
  validationHandler: ValidationHandler;
  element: BudgetHeadProjectDTO | undefined;
}

export class ProjectBudgetForm extends SoeFormGroup {
  budgetValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IBudgetForm) {
    super(validationHandler, {
      budgetHeadId: new SoeNumberFormControl(element?.budgetHeadId || 0, {
        isIdField: true,
      }),
      projectId: new SoeNumberFormControl(element?.projectId || 0),
      projectName: new SoeTextFormControl(element?.projectName || ''),
      projectNr: new SoeTextFormControl(element?.projectNr || ''),
      projectFromDate: new SoeSelectFormControl(
        element?.projectFromDate || new Date()
      ),
      projectToDate: new SoeSelectFormControl(
        element?.projectToDate || new Date()
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, isNameField: true },
        'common.name'
      ),
      fromDate: new SoeSelectFormControl(element?.fromDate || undefined),
      toDate: new SoeSelectFormControl(element?.toDate || undefined),
      status: new SoeNumberFormControl(
        element?.status || +SoeEntityState.Active
      ),
      type: new SoeSelectFormControl(
        element?.type || +DistributionCodeBudgetType.ProjectBudgetExtended,
        {
          disabled: true,
        }
      ),
      periodType: new SoeSelectFormControl(
        element?.periodType || +TermGroup_ProjectBudgetPeriodType.SinglePeriod
      ),
      rows: new FormArray<ProjectBudgetRowForm>([]),
      noOfPeriods: new SoeNumberFormControl(element?.noOfPeriods ?? 0),
      parentBudgetHeadId: new SoeNumberFormControl(
        element?.parentBudgetHeadId || 0
      ),
    });

    this.budgetValidationHandler = validationHandler;
    this.customBudgetRowsPatchValues(
      <BudgetRowProjectDTO[]>element?.rows ?? []
    );
  }

  get budgetHeadId(): SoeNumberFormControl {
    return <SoeTextFormControl>this.controls.budgetHeadId;
  }

  get projectId(): SoeNumberFormControl {
    return <SoeTextFormControl>this.controls.projectId;
  }

  get parentBudgetHeadId(): SoeNumberFormControl {
    return <SoeTextFormControl>this.controls.parentBudgetHeadId;
  }

  get projectNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectNr;
  }

  get projectName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectName;
  }

  get projectFromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.projectFromDate;
  }

  get projectToDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.projectToDate;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }
  get fromDateValue(): Date {
    return <Date>this.fromDate.value;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }
  get toDateValue(): Date {
    return <Date>this.toDate.value;
  }

  get rows(): FormArray<ProjectBudgetRowForm> {
    return <FormArray>this.controls.rows;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get periodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.periodType;
  }
  get periodTypeValue(): TermGroup_ProjectBudgetPeriodType {
    return <TermGroup_ProjectBudgetPeriodType>this.periodType.value;
  }

  get lockStatus(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.status;
  }

  customBudgetRowsPatchValues(bRows: BudgetRowProjectDTO[]) {
    this.rows.clear({ emitEvent: false });
    if (bRows) {
      for (const brw of bRows) {
        if (
          brw.budgetRowId !== null &&
          brw.budgetRowId !== undefined &&
          typeof brw.budgetRowId === 'number'
        ) {
          const row = new ProjectBudgetRowForm({
            validationHandler: this.budgetValidationHandler,
            element: brw,
          });
          if (brw.isDeleted) row.disable();
          this.rows.push(row, { emitEvent: false });
        }
      }
      console.log('customBudgetRowsPatchValues', this.rows);
    }
    this.rows.updateValueAndValidity();
  }

  deleteBudgetRow(row: BudgetRowProjectDTO) {
    const budgetRows = <BudgetRowProjectDTO[]>this.rows.getRawValue();
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
    const rows: BudgetRowProjectDTO[] = [];
    const formRows = <BudgetRowProjectDTO[]>this.rows.getRawValue();
    formRows.forEach(b => {
      if (
        !b.isDefault &&
        !((b.budgetRowId <= 0 && b.isDeleted) || !b.totalAmount)
      ) {
        rows.push(b);
      }
    });
    this.customBudgetRowsPatchValues(rows);
  }

  addBudgetRow(): BudgetRowProjectDTO {
    const newRowDto = new BudgetRowProjectDTO(this.getNewRowId());
    newRowDto.budgetHeadId = this.budgetHeadId.value;
    newRowDto.budgetRowId = this.getNewRowId();

    const newRow = new ProjectBudgetRowForm({
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
      minId = (<BudgetRowProjectDTO[]>this.rows.value)
        .map(x => x.budgetRowId)
        .reduce((a, b) => Math.min(a, b));
    }

    return minId > 0 ? 0 : --minId;
  }

  setDirtyOnbudgetRowChange(budgetRow: BudgetRowProjectDTO) {
    const totalAmount = this.getTotalAmount(budgetRow);
    // const totalAmount = budgetRow.totalAmount;
    this.rows.controls.forEach(x => {
      if (x.budgetRowId.value === budgetRow.budgetRowId) {
        x.patchValue({});
      }
    });
    this.markAsDirty();
    this.rows.markAsDirty();
    this.rows.markAsTouched();
    return totalAmount;
  }

  private getTotalAmount(budgetRow: BudgetRowProjectDTO) {
    const total = 0;
    return total;
  }

  resetAmounts(budgetRow: BudgetRowProjectDTO) {}

  lockUnlockFormControls(value: boolean) {
    if (value) {
      this.name.disable();
    } else {
      this.name.enable();
    }
  }
}
