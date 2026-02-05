import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { FormArray } from '@angular/forms';
import { EditableGridTestDataDTO } from '../components/grid-test-components/editable-grid.component';

interface IEditableGridTestDataForm {
  validationHandler: ValidationHandler;
  element: EditableGridTestDataDTO | undefined;
}
export class EditableGridTestDataForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IEditableGridTestDataForm) {
    super(validationHandler, {
      id: new SoeTextFormControl(element?.id || 0, {
        isIdField: true,
      }),
      city: new SoeTextFormControl(element?.city || ''),
      name2: new SoeTextFormControl(element?.name2 || ''),
      isDefault: new SoeCheckboxFormControl(element?.isDefault || false),
      timeFrom: new SoeDateFormControl(element?.timeFrom || new Date()),
      timeTo: new SoeDateFormControl(element?.timeTo || new Date()),
      length: new SoeNumberFormControl(element?.length || 0),
      itemId: new SoeSelectFormControl(element?.itemId || undefined),
      typeId: new SoeSelectFormControl(element?.typeId || undefined),
      date: new SoeDateFormControl(element?.date || new Date()),
      number: new SoeNumberFormControl(element?.number || 0),
    });
    this.thisValidationHandler = validationHandler;
  }

  get editableGridRows(): FormArray {
    return <FormArray>this.controls.editableGridRows;
  }

  updateStopTime(): void {
    this.controls.timeTo.patchValue(
      (this.controls.timeFrom.value as Date).addMinutes(
        this.controls.length.value
      )
    );
  }
}
