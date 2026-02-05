import { SoeCheckboxFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SelectProjectDialogFormDTO } from './select-project-dialog.model';
interface ISelectProjectDialogForm {
  validationHandler: ValidationHandler;
  element: SelectProjectDialogFormDTO;
}
export class SelectProjectDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISelectProjectDialogForm) {
    super(validationHandler, {
      showWithoutCustomer: new SoeCheckboxFormControl(
        element.showWithoutCustomer || false
      ),
      showFindHidden: new SoeCheckboxFormControl(
        element.showFindHidden || false
      ),
      projectsWithoutCustomer: new SoeCheckboxFormControl(
        element.projectsWithoutCustomer || false
      ),
      showMine: new SoeCheckboxFormControl(element.showMine || false),
    });
  }
  get showWithoutCustomer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showWithoutCustomer;
  }
  get showFindHidden(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showFindHidden;
  }
  get showMine(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showMine;
  }
}
