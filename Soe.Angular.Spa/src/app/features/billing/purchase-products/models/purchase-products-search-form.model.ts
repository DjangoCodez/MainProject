import { ValidationHandler } from '@shared/handlers';
import { SupplierProductGridHeaderDTO } from './purchase-product.model';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISupplierProductGridHeaderForm {
  validationHandler: ValidationHandler;
  element: SupplierProductGridHeaderDTO | undefined;
}

export class SupplierProductGridHeaderForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISupplierProductGridHeaderForm) {
    super(validationHandler, {
      supplierIds: new SoeSelectFormControl(element?.supplierIds || []),
      supplierProduct: new SoeTextFormControl(element?.supplierProduct || ''),
      supplierProductName: new SoeTextFormControl(
        element?.supplierProductName || ''
      ),
      product: new SoeTextFormControl(element?.product || ''),
      productName: new SoeTextFormControl(element?.productName || ''),
      invoiceProductId: new SoeTextFormControl(
        element?.invoiceProductId || undefined
      ),
    });
  }

  get supplierIds() {
    return <SoeSelectFormControl>this.controls.supplierIds;
  }

  get supplierProduct() {
    return <SoeTextFormControl>this.controls.supplierProduct;
  }

  get supplierProductName() {
    return <SoeTextFormControl>this.controls.supplierProductName;
  }

  get product() {
    return <SoeTextFormControl>this.controls.product;
  }

  get productName() {
    return <SoeTextFormControl>this.controls.productName;
  }
}
