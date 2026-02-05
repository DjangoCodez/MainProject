import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IDayTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IDayTypesForm {
  validationHandler: ValidationHandler;
  element: IDayTypeDTO | undefined;
}
export class DayTypesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDayTypesForm) {
    super(validationHandler, {
      dayTypeId: new SoeTextFormControl(element?.dayTypeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 80, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 50 },
        'common.description'
      ),
      standardWeekdayFrom: new SoeSelectFormControl(
        element?.standardWeekdayFrom || undefined
      ),
      standardWeekdayTo: new SoeSelectFormControl(
        element?.standardWeekdayTo || undefined
      ),
      weekendSalary: new SoeCheckboxFormControl(
        element?.weekendSalary || false
      ),
    });
  }

  get dayTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dayTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
}
