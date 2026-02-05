import { SoeFormGroup, SoeSelectFormControl } from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";

export class DeleteAgreementForm extends SoeFormGroup {
  constructor(validationHandler: ValidationHandler) {
    super(validationHandler, {
      wholesellerId: new SoeSelectFormControl(
        null,
        {
          required: true
        },
        'common.customer.customer.wholesellername'
      ),
      priceListTypeId: new SoeSelectFormControl(
        0,
        {},
        'billing.order.pricelisttype'
      )
    });
  }
}