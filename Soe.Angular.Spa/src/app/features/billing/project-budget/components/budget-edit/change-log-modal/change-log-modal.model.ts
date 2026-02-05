import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BudgetRowProjectChangeLogDTO } from '@features/billing/project-budget/models/project-budget.model';
import { FormArray } from '@angular/forms';

export class ChangeLogDialogData implements DialogData {
  title: string;
  size?: DialogSize;
  items: BudgetRowProjectChangeLogDTO[];
  isReadOnly: boolean;

  constructor() {
    this.title = 'billing.projects.budget.handlerowhistory';
    this.size = 'sm';
    this.items = [];
    this.isReadOnly = false;
  }
}

interface IChangePeriodForm {
  validationHandler: ValidationHandler;
  generalComment: string;
}

export class ChangeLogForm extends SoeFormGroup {
  constructor({ validationHandler, generalComment }: IChangePeriodForm) {
    super(validationHandler, {
      generalComment: new SoeTextFormControl(generalComment || ''),
    });
  }

  get generalComment(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.generalComment;
  }
}
