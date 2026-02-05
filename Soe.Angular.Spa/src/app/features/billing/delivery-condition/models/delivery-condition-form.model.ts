import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DeliveryConditionDTO } from './delivery-condition.model';

interface IDeliveryConditionForm {
  validationHandler: ValidationHandler;
  element: DeliveryConditionDTO | undefined;
}
export class DeliveryConditionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDeliveryConditionForm) {
    super(validationHandler, {
      deliveryConditionId: new SoeTextFormControl(
        element?.deliveryConditionId || 0,
        {
          isIdField: true,
        }
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { isNameField: true, required: true, maxLength: 20, minLength: 1 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { maxLength: 100, required: true },
        'common.name'
      ),
    });
  }

  get deliveryConditionId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.deliveryConditionId;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
}
