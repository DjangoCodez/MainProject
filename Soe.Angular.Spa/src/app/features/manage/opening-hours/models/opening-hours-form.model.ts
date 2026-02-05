import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IOpeningHoursDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IOpeningHoursForm {
  validationHandler: ValidationHandler;
  element: IOpeningHoursDTO | undefined;
}
export class OpeningHoursForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IOpeningHoursForm) {
    super(validationHandler, {
      openingHoursId: new SoeTextFormControl(element?.openingHoursId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 50, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 100 },
        'manage.registry.openinghours.description'
      ),
      standardWeekDay: new SoeSelectFormControl(
        element?.standardWeekDay || 0,
        {},
        'manage.registry.openinghours.standardweekday'
      ),
      specificDate: new SoeDateFormControl(
        element?.specificDate || null,
        {},
        'manage.registry.openinghours.specificdate'
      ),
      openingTime: new SoeTextFormControl(
        element?.openingTime || '',
        {},
        'manage.registry.openinghours.openingtime'
      ),
      closingTime: new SoeTextFormControl(
        element?.closingTime || '',
        {},
        'manage.registry.openinghours.closingtime'
      ),
      fromDate: new SoeDateFormControl(
        element?.fromDate || null,
        {},
        'common.validfrom'
      ),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  disableSpecificDate(value: number, modifyPermission: boolean) {
    if (value == 0) {
      if (modifyPermission) this.controls.specificDate.enable();
    } else {
      this.controls.specificDate.disable();
      this.controls.specificDate.setValue(null);
    }
  }

  setupDisabledStates() {
    if (this.controls.standardWeekDay.value != 0) {
      this.controls.specificDate.disable();
    }
  }
}
