import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class BudgetGridHeaderForm extends SoeFormGroup {
  constructor(validationHandler: ValidationHandler) {
    super(validationHandler, {
      budgetTypeId: new SoeSelectFormControl(
        0,
        {},
        'billing.order.pricelisttype'
      ),
    });
  }
}
