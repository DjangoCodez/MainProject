import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class ImportAgreementForm extends SoeFormGroup {
  get fileName() {
    return <SoeTextFormControl>this.controls.fileName;
  }

  get isSolar() {
    return this.controls.wholesellerId.value == 5;
  }

  constructor(validationHandler: ValidationHandler) {
    super(validationHandler, {
      file: new SoeTextFormControl(''),
      wholesellerId: new SoeSelectFormControl(
        null,
        {
          required: true,
        },
        'common.customer.customer.wholesellername'
      ),
      priceListTypeId: new SoeSelectFormControl(
        0,
        {},
        'billing.order.pricelisttype'
      ),
      generalDiscount: new SoeNumberFormControl(
        0,
        {
          decimals: 2,
        },
        'billing.invoices.supplieragreement.generaldiscount'
      ),
    });
  }
}
