import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DeliveryTypeDTO } from './delivery-types.model';

interface IDeliveryTypesForm {
  validationHandler: ValidationHandler;
  element: DeliveryTypeDTO | undefined;
}
export class DeliveryTypesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDeliveryTypesForm) {
    super(validationHandler, {
      deliveryTypeId: new SoeTextFormControl(element?.deliveryTypeId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        { isNameField: true, required: true, maxLength: 20 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
    });
  }

  get deliveryTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.deliveryTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }
}
