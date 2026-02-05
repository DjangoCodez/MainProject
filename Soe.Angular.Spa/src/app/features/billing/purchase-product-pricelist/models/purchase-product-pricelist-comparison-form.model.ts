import { ValidationHandler } from '@shared/handlers';
import { SupplierProductPriceComparisonDTO } from './purchase-product-pricelist.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

interface ISupplierProductPriceComparisonForm {
  validationHandler: ValidationHandler;
  element?: SupplierProductPriceComparisonDTO;
}

export class SupplierProductPriceComparisonForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ISupplierProductPriceComparisonForm) {
    super(validationHandler, {
      supplierProductPriceId: new SoeTextFormControl(
        element?.supplierProductPriceId || 0,
        { isIdField: true }
      ),
      supplierProductId: new SoeTextFormControl(
        element?.supplierProductId || 0
      ),
      productName: new SoeTextFormControl(element?.productName || ''),
      ourProductName: new SoeTextFormControl(element?.ourProductName || ''),
      compareQuantity: new SoeNumberFormControl(element?.compareQuantity || 0),
      comparePrice: new SoeNumberFormControl(element?.comparePrice || 0),
      compareStartDate: new SoeTextFormControl(element?.compareStartDate || ''),
      compareEndDate: new SoeTextFormControl(element?.compareEndDate || ''),
      quantity: new SoeNumberFormControl(element?.quantity || undefined),
      price: new SoeNumberFormControl(element?.price || undefined),
      isModified: new SoeCheckboxFormControl(element?.isModified || undefined),
      entityState: new SoeTextFormControl(
        element?.entityState || SoeEntityState.Active
      ),
      state: new SoeTextFormControl(element?.state || SoeEntityState.Active),
    });
  }

  get supplierProductPriceId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductPriceId;
  }

  get supplierProductId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductId;
  }

  get productName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productName;
  }

  get ourProductName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.ourProductName;
  }

  get compareQuantity(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.compareQuantity;
  }

  get comparePrice(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.comparePrice;
  }

  get compareStartDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.compareStartDate;
  }

  get compareEndDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.compareEndDate;
  }

  get quantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.quantity;
  }

  get price(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.price;
  }

  get isModified(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isModified;
  }

  get entityState(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.entityState;
  }

  get state(): SoeTextFormControl {
    return <SoeCheckboxFormControl>this.controls.state;
  }
}
