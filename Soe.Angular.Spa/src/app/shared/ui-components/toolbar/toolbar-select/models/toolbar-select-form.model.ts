import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ToolbarSelectModel } from './toolbar-select.model';

interface IToolbarSelectForm {
  validationHandler: ValidationHandler;
  element: ToolbarSelectModel;
}

export class ToolbarSelectForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IToolbarSelectForm) {
    super(validationHandler, {
      selectedId: new SoeTextFormControl(element?.selectedId),
    });
    this.thisValidationHandler = validationHandler;
  }

  get selectedId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.selectedId;
  }
}
