import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { BudgetRowProjectDTO } from './project-budget.model';

interface IBudgetRowForm {
  validationHandler: ValidationHandler;
  element: BudgetRowProjectDTO | undefined;
}

export class ProjectBudgetRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IBudgetRowForm) {
    super(validationHandler, {
      budgetRowId: new SoeTextFormControl(element?.budgetRowId || 0, {
        isIdField: true,
      }),
      totalAmount: new SoeNumberFormControl(element?.totalAmount || 0),
    });
  }

  get budgetRowId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.budgetRowId;
  }
  get totalAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.totalAmount;
  }

  get isDeleted(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isDeleted;
  }
}
