import { ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions/soe-formgroup.extension';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { IIncomingDeliveryTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IIncomingDeliveryTypeForm {
  validationHandler: ValidationHandler;
  element: IIncomingDeliveryTypeDTO | undefined;
}
export class IncomingDeliveryTypeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IIncomingDeliveryTypeForm) {
    super(validationHandler, {
      incomingDeliveryTypeId: new SoeTextFormControl(
        element?.incomingDeliveryTypeId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      length: new SoeNumberFormControl(
        element?.length || 0,
        {},
        'time.schedule.incomingdeliverytype.lengthminutes'
      ),
      nbrOfPersons: new SoeNumberFormControl(
        element?.nbrOfPersons || 0,
        {},
        'common.nbrOfPersons'
      ),
      accountId: new SoeSelectFormControl(element?.accountId || undefined),
    });
  }

  get incomingDeliveryTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.incomingDeliveryTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get length(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.length;
  }
  get nbrOfPersons(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.nbrOfPersons;
  }
}

export function createLengthValidator(
  errorMessage: string,
  minLength: number
): ValidatorFn {
  return (form): ValidationErrors | null => {
    let invalidLength = false;
    const length = form.get('length')?.value || 0;

    if (length < minLength) invalidLength = true;

    console.log('createLengthValidator', invalidLength, length, minLength);
    return invalidLength ? { [errorMessage]: true } : null;
  };
}
