import { FormArray, FormControl, Validators } from '@angular/forms';
import {
  SoeFormGroup,
  SoeDateFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationFieldTerms, ValidationHandler } from '@shared/handlers';
import { ITimeScheduleEventDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';

interface ITimeScheduleEventForm {
  validationHandler: ValidationHandler;
  element: ITimeScheduleEventDTO | undefined;
}

export class TimeScheduleEventForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeScheduleEventForm) {
    super(validationHandler, {
      timeScheduleEventId: new SoeTextFormControl(
        element?.timeScheduleEventId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { required: true, maxLength: 512 },
        'common.description'
      ),
      date: new SoeDateFormControl(
        element?.date || new Date(),
        { required: false },
        'common.date'
      ),
      messageGroupIds: arrayToFormArray(
        element?.timeScheduleEventMessageGroups?.map(x => x.messageGroupId) ||
          (element as any)?.messageGroupIds ||
          []
      ),
    });

    this.messageGroupIds.addValidators(Validators.required);
  }

  override getValidationFieldTerms(): ValidationFieldTerms {
    const terms = super.getValidationFieldTerms();
    terms['messageGroupIds'] = 'core.xemail.selectedreceivers';
    return terms;
  }

  get messageGroupIds(): FormArray<FormControl<number>> {
    return <FormArray<FormControl<number>>>this.controls.messageGroupIds;
  }

  customPatchValue(value: ITimeScheduleEventDTO) {
    this.patchValue(value);
    clearAndSetFormArray(
      value.timeScheduleEventMessageGroups?.map(x => x.messageGroupId) || [],
      this.messageGroupIds
    );
  }
}
