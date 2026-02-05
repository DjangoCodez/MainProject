import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
interface IImportPaymentsFilterForm {
  validationHandler: ValidationHandler;
  element: number | undefined;
}
export class ImportPaymentsFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportPaymentsFilterForm) {
    super(validationHandler, {
      durationSelection: new SoeSelectFormControl(element || 3),
    });
  }
  get durationSelection(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.durationSelection;
  }
}
