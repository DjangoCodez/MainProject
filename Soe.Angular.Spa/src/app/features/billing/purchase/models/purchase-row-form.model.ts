import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseDTO } from './purchase.model';
import { FormArray } from '@angular/forms';
import { OriginUserForm } from './purchase-origin-user-form.model';
import { IOriginUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  PurchaseRowType,
  SoeEntityState,
  SoeOriginStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { PurchaseRowDTO } from './purchase-rows.model';

interface IStocksForProductForm {
  validationHandler: ValidationHandler;
  element: SmallGenericType | undefined;
}

export class StocksForProductForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStocksForProductForm) {
    super(validationHandler, {
      id: new SoeNumberFormControl(element?.id || 0),
      name: new SoeTextFormControl(element?.name || ''),
    });
  }

  get id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.id;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
}

interface ICustomerInvoiceRowIdForm {
  validationHandler: ValidationHandler;
  element: number | undefined;
}

export class CustomerInvoiceRowIdForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerInvoiceRowIdForm) {
    super(validationHandler, {
      element: new SoeNumberFormControl(element || undefined),
    });
  }

  get element(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.element;
  }
}

interface IPurchaseRowForm {
  validationHandler: ValidationHandler;
  element: PurchaseRowDTO | undefined;
}

export class PurchaseRowForm extends SoeFormGroup {
  purchaseRowValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IPurchaseRowForm) {
    super(validationHandler, {
      purchaseRowId: new SoeNumberFormControl(element?.purchaseRowId || 0, {
        isIdField: true,
      }),
      tempRowId: new SoeNumberFormControl(element?.tempRowId || 0),
      purchaseId: new SoeNumberFormControl(element?.purchaseId || 0),
      purchaseNr: new SoeTextFormControl(element?.purchaseNr || '', {
        isNameField: true,
      }),
      productId: new SoeNumberFormControl(element?.productId || undefined),
      productName: new SoeTextFormControl(element?.productName || ''),
      productNr: new SoeTextFormControl(element?.productNr || ''),

      stockId: new SoeNumberFormControl(element?.stockId || undefined),
      stockCode: new SoeTextFormControl(element?.stockCode || ''),
      rowNr: new SoeNumberFormControl(element?.rowNr || 0),

      purchaseUnitId: new SoeNumberFormControl(
        element?.purchaseUnitId || undefined
      ),
      parentRowId: new SoeNumberFormControl(element?.parentRowId || undefined),

      quantity: new SoeNumberFormControl(element?.quantity || 0),

      deliveredQuantity: new SoeNumberFormControl(
        element?.deliveredQuantity || undefined
      ),
      wantedDeliveryDate: new SoeDateFormControl(
        element?.wantedDeliveryDate || undefined
      ),
      accDeliveryDate: new SoeDateFormControl(
        element?.accDeliveryDate || undefined
      ),
      deliveryDate: new SoeDateFormControl(element?.deliveryDate || undefined),
      text: new SoeTextFormControl(element?.text || ''),

      purchasePrice: new SoeNumberFormControl(element?.purchasePrice || 0),
      purchasePriceCurrency: new SoeNumberFormControl(
        element?.purchasePriceCurrency || 0
      ),
      discountType: new SoeNumberFormControl(element?.discountType || 0),
      discountAmount: new SoeNumberFormControl(element?.discountAmount || 0),
      discountAmountCurrency: new SoeNumberFormControl(
        element?.discountAmountCurrency || 0
      ),
      discountPercent: new SoeNumberFormControl(element?.discountPercent || 0),
      vatAmount: new SoeNumberFormControl(element?.vatAmount || 0),
      vatAmountCurrency: new SoeNumberFormControl(
        element?.vatAmountCurrency || 0
      ),
      vatRate: new SoeNumberFormControl(element?.vatRate || 0),
      vatCodeId: new SoeNumberFormControl(element?.vatCodeId || undefined),

      vatCodeName: new SoeTextFormControl(element?.vatCodeName || ''),
      vatCodeCode: new SoeTextFormControl(element?.vatCodeCode || ''),

      sumAmount: new SoeNumberFormControl(element?.sumAmount || 0),
      sumAmountCurrency: new SoeNumberFormControl(
        element?.sumAmountCurrency || 0
      ),
      orderId: new SoeNumberFormControl(element?.orderId || undefined),
      orderNr: new SoeTextFormControl(element?.orderNr || ''),
      purchaseRowStatus: new SoeNumberFormControl(element?.status || 0),
      statusName: new SoeTextFormControl(element?.statusName || ''),
      isLocked: new SoeCheckboxFormControl(element?.isLocked || false),

      modified: new SoeDateFormControl(element?.modified || undefined),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy || ''),

      state: new SoeTextFormControl(element?.state || SoeEntityState.Active),
      type: new SoeTextFormControl(element?.type || PurchaseRowType.Unknown),

      supplierProductId: new SoeNumberFormControl(
        element?.supplierProductId || undefined
      ),
      supplierProductNr: new SoeTextFormControl(
        element?.supplierProductNr || ''
      ),
      intrastatCodeId: new SoeNumberFormControl(
        element?.intrastatCodeId || undefined
      ),
      sysCountryId: new SoeNumberFormControl(
        element?.sysCountryId || undefined
      ),
      intrastatTransactionId: new SoeNumberFormControl(
        element?.intrastatTransactionId || undefined
      ),

      isModified: new SoeCheckboxFormControl(element?.isModified || false),
      statusIcon: new SoeTextFormControl(element?.statusIcon || ''),
      purchaseProductUnitCode: new SoeTextFormControl(
        element?.purchaseProductUnitCode || ''
      ),
      discountTypeText: new SoeTextFormControl(element?.discountTypeText || ''),
      discountValue: new SoeNumberFormControl(element?.discountValue || 0),
      customerInvoiceRowIds: new FormArray<CustomerInvoiceRowIdForm>([]),
      stocksForProduct: new FormArray<StocksForProductForm>([]),
    });
    this.purchaseRowValidationHandler = validationHandler;
    if (element?.stocksForProduct)
      this.customStocksForProductPatchValue(element?.stocksForProduct);
    if (element?.customerInvoiceRowIds)
      this.customCustomerInvoiceRowIdsPatchValue(element.customerInvoiceRowIds);
  }

  get purchaseRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseRowId;
  }
  get tempRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.tempRowId;
  }
  get purchaseId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseId;
  }
  get purchaseNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseNr;
  }
  get productId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.productId;
  }
  get productName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productName;
  }
  get productNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNr;
  }
  get stockId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.stockId;
  }
  get stockCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockCode;
  }
  get rowNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.rowNr;
  }
  get purchaseUnitId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseUnitId;
  }
  get parentRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.parentRowId;
  }
  get quantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.quantity;
  }
  get deliveredQuantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.deliveredQuantity;
  }
  get wantedDeliveryDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.wantedDeliveryDate;
  }
  get accDeliveryDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.accDeliveryDate;
  }
  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
  get purchasePrice(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchasePrice;
  }
  get purchasePriceCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchasePriceCurrency;
  }
  get discountType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountType;
  }

  get discountAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountAmount;
  }
  get discountAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountAmountCurrency;
  }
  get discountPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountPercent;
  }
  get vatAmount(): SoeNumberFormControl {
    return <SoeSelectFormControl>this.controls.vatAmount;
  }
  get vatAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.currenvatAmountCurrencycyId;
  }
  get vatRate(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.vatRate;
  }
  get vatCodeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.vatCodeId;
  }

  get vatCodeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.vatCodeName;
  }

  get vatCodeCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.vatCodeCode;
  }
  get sumAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sumAmount;
  }

  get sumAmountCurrency(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sumAmountCurrency;
  }

  get orderId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderId;
  }
  get orderNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.orderNr;
  }
  get purchaseRowStatus(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseRowStatus;
  }
  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }

  get isLocked(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.isLocked;
  }
  get modified(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.modified;
  }
  get modifiedBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.modifiedBy;
  }
  get state(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.state;
  }
  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }
  get supplierProductId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.supplierProductId;
  }
  get supplierProductNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductNr;
  }

  get intrastatCodeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.intrastatCodeId;
  }

  get sysCountryId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysCountryId;
  }
  get intrastatTransactionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.intrastatTransactionId;
  }

  get isModified(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isModified;
  }
  get statusIcon(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusIcon;
  }
  get purchaseProductUnitCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseProductUnitCode;
  }
  get discountTypeText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.discountTypeText;
  }
  get discountValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.discountValue;
  }

  get customerInvoiceRowIds(): FormArray<CustomerInvoiceRowIdForm> {
    return <FormArray>this.controls.customerInvoiceRowIds;
  }
  get stocksForProduct(): FormArray<StocksForProductForm> {
    return <FormArray>this.controls.stocksForProduct;
  }
  customStocksForProductPatchValue(stocksForProducts: SmallGenericType[]) {
    (this.controls.stocksForProduct as FormArray).clear();

    if (stocksForProducts) {
      for (const stocksForProduct of stocksForProducts) {
        const row = new StocksForProductForm({
          validationHandler: this.purchaseRowValidationHandler,
          element: stocksForProduct,
        });
        (this.controls.stocksForProduct as FormArray).push(row, {
          emitEvent: false,
        });
      }
    }
  }

  customCustomerInvoiceRowIdsPatchValue(customerInvoiceRowIds: number[]) {
    (this.controls.customerInvoiceRowIds as FormArray).clear();

    if (customerInvoiceRowIds) {
      for (const customerInvoiceRowId of customerInvoiceRowIds) {
        const row = new CustomerInvoiceRowIdForm({
          validationHandler: this.purchaseRowValidationHandler,
          element: customerInvoiceRowId,
        });
        (this.controls.customerInvoiceRowIds as FormArray).push(row, {
          emitEvent: false,
        });
      }
    }
  }
}
