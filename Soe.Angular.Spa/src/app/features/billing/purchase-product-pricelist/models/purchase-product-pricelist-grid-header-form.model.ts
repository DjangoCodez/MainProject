import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SupplierProductPriceListGridHeaderDTO } from './purchase-product-pricelist.model';

interface ISupplierProductPriceListGridHeaderForm {
  validationHandler: ValidationHandler;
  element: SupplierProductPriceListGridHeaderDTO | undefined;
}

export class SupplierProductPriceListGridHeaderForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ISupplierProductPriceListGridHeaderForm) {
    super(validationHandler, {
      supplierId: new SoeSelectFormControl(element?.supplierId || 0),
      currencyId: new SoeTextFormControl(element?.currencyId || 0),
      compareDate: new SoeTextFormControl(element?.compareDate || ''),
      includePricelessProducts: new SoeTextFormControl(
        element?.includePricelessProducts || false
      ),
    });
  }

  get supplierId() {
    return <SoeSelectFormControl>this.controls.supplierId;
  }

  get currencyId() {
    return <SoeTextFormControl>this.controls.currencyId;
  }

  get compareDate() {
    return <SoeTextFormControl>this.controls.compareDate;
  }

  get includePricelessProducts() {
    return <SoeTextFormControl>this.controls.includePricelessProducts;
  }
}
