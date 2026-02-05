import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PriceBasedMarkupDTO } from './price-based-markup.model';

interface IPriceBasedMarkupForm {
  validationHandler: ValidationHandler;
  element: PriceBasedMarkupDTO | undefined;
}

export class PriceBasedMarkupForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPriceBasedMarkupForm) {
    super(validationHandler, {
      priceBasedMarkupId: new SoeTextFormControl(
        element?.priceBasedMarkupId || 0,
        {
          isIdField: true,
        }
      ),
      minPrice: new SoeNumberFormControl(
        element?.minPrice || 0,
        {
          minValue: 0,
        },
        'economy.supplier.invoice.matches.amountfrom'
      ),
      maxPrice: new SoeNumberFormControl(
        element?.maxPrice || 0,
        {
          required: true,
          minValue: 0,
          zeroNotAllowed: true,
        },
        'economy.customer.invoice.matches.amountto'
      ),
      markupPercent: new SoeNumberFormControl(element?.markupPercent || 0, {
        required: true,
        minValue: 0,
        zeroNotAllowed: true,
      }, 'billing.invoices.markup.markuppercent'
    ),
      priceListTypeId: new SoeSelectFormControl(
        element?.priceListTypeId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'billing.projects.list.pricelist'
      ),
      state: new SoeTextFormControl(element?.state || ''),
    });
  }

  get priceBasedMarkupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.priceBasedMarkupId;
  }

  get minPrice(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.minPrice;
  }

  get maxPrice(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.maxPrice;
  }

  get markupPercent(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.markupPercent;
  }

  get priceListTypeId():  SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.priceListTypeId;
  }

  get state(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.state;
  }

 }
