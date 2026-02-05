import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SupplierProductDTO } from './purchase-product.model';

interface IPurchaseProductForm {
  validationHandler: ValidationHandler;
  element: SupplierProductDTO | undefined;
}

export class PurchaseProductForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseProductForm) {
    super(validationHandler, {
      supplierProductId: new SoeTextFormControl(
        element?.supplierProductId || 0,
        { isIdField: true }
      ),
      supplierId: new SoeSelectFormControl(
        element?.supplierId || undefined,
        { required: true },
        'billing.purchase.supplier'
      ),
      supplierProductNr: new SoeTextFormControl(
        element?.supplierProductNr || '',
        { required: true, isNameField: true },
        'billing.purchase.product.supplieritemno'
      ),
      supplierProductName: new SoeTextFormControl(
        element?.supplierProductName || '',
        { required: true },
        'billing.purchase.product.supplieritemname'
      ),
      supplierProductUnitId: new SoeSelectFormControl(
        element?.supplierProductUnitId || undefined,
        { required: true },
        'billing.purchase.product.supplierunit'
      ),
      supplierProductCode: new SoeTextFormControl(
        element?.supplierProductCode || ''
      ),
      packSize: new SoeNumberFormControl(element?.packSize || 0),
      deliveryLeadTimeDays: new SoeNumberFormControl(
        element?.deliveryLeadTimeDays || 0
      ),
      productId: new SoeSelectFormControl(element?.productId || null, {}, ''),

      itemName: new SoeTextFormControl(element?.itemName || ''),
      itemUnit: new SoeTextFormControl(element?.itemUnit || ''),
    });
    this.itemName.disable();
    this.itemUnit.disable();
  }

  get supplierProductId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductId;
  }

  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }

  get supplierProductNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductNr;
  }

  get supplierProductName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductName;
  }

  get supplierProductUnitId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierProductUnitId;
  }

  get supplierProductCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductCode;
  }

  get packSize(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.packSize;
  }

  get deliveryLeadTimeDays(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.deliveryLeadTimeDays;
  }

  get productId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productId;
  }

  get itemUnit(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.itemUnit;
  }

  get itemName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.itemName;
  }

  patchSelectedItemValues(name: string, productUnit: string): void {
    this.patchValue({
      itemName: '',
      itemUnit: '',
    });

    this.patchValue({
      itemName: name,
      itemUnit: productUnit,
    });
  }
}
