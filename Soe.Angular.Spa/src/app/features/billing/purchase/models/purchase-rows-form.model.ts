import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseRowSummeryFormDTO } from './purchase-rows.model';

interface IPurchaseRowsForm {
  validationHandler: ValidationHandler;
  element: PurchaseRowSummeryFormDTO | undefined;
}

export class PurchaseRowsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseRowsForm) {
    super(validationHandler, {
      centRounding: new SoeNumberFormControl(element?.centRounding || 0.0),
      totalAmountExVatCurrency: new SoeNumberFormControl(
        element?.totalAmountExVatCurrency || 0.0
      ),
      baseCurrencyCode: new SoeNumberFormControl(
        element?.baseCurrencyCode || 0.0
      ),
    });
  }

  get centRounding(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.centRounding;
  }
  get totalAmountExVatCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.totalAmountExVatCurrency;
  }
  get baseCurrencyCode(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.baseCurrencyCode;
  }
}
