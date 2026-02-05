import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeNumberFormControl,
  SoeCheckboxFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PaymentConditionDTO } from './payment-condition.model';

interface IPyamentConditionForm {
  validationHandler: ValidationHandler;
  element: PaymentConditionDTO | undefined;
}

export class PaymentConditionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPyamentConditionForm) {
    super(validationHandler, {
      paymentConditionId: new SoeTextFormControl(
        element?.paymentConditionId || 0,
        {
          isIdField: true,
        }
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 20, minLength: 1 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      days: new SoeNumberFormControl(
        element?.days || undefined,
        { required: true, minValue: 0 },
        'economy.accounting.paymentcondition.days'
      ),
      startOfNextMonth: new SoeCheckboxFormControl(
        element?.startOfNextMonth || false,
        {},
        'common.paymentcondition.calculatefromnextmonth'
      ),
      discountDays: new SoeNumberFormControl(
        element?.discountDays || 0,
        { minValue: 0 },
        'economy.accounting.paymentcondition.discountdays'
      ),
      discountPercent: new SoeNumberFormControl(
        element?.discountPercent || 0,
        { maxValue: 100 },
        'economy.accounting.paymentcondition.discountpercent'
      ),
    });
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }
}
