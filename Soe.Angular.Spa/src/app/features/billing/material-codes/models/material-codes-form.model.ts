import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  TimeCodeInvoiceProductDTO,
  TimeCodeMaterialDTO,
} from './material-codes.model';
import { FormArray } from '@angular/forms';
import { TimeCodeMaterialInvoiceProductRowsForm } from './material-codes-invoice-product-row-form.model';
import { SoeTimeCodeType } from '@shared/models/generated-interfaces/Enumerations';

interface ITimeCodeMaterialsForm {
  validationHandler: ValidationHandler;
  element: TimeCodeMaterialDTO | undefined;
}
export class TimeCodeMaterialsForm extends SoeFormGroup {
  invoiceProductValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ITimeCodeMaterialsForm) {
    super(validationHandler, {
      timeCodeId: new SoeTextFormControl(element?.timeCodeId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 20, minLength: 1 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      note: new SoeTextFormControl(
        element?.note || '',
        { maxLength: 50 },
        'common.note'
      ),
      state: new SoeTextFormControl(element?.state || ''),
      type: new SoeTextFormControl(element?.type || +SoeTimeCodeType.Material),
      invoiceProducts: new FormArray<TimeCodeMaterialInvoiceProductRowsForm>(
        []
      ),
    });
    this.invoiceProductValidationHandler = validationHandler;
    this.patchInvoiceProducts(element?.invoiceProducts ?? []);
  }

  get timeCodeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeId;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }
  get state(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.state;
  }
  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }
  get invoiceProducts(): FormArray<TimeCodeMaterialInvoiceProductRowsForm> {
    return <FormArray>this.controls.invoiceProducts;
  }

  onDoCopy() {
    this.invoiceProducts.controls.forEach(invProduct => {
      invProduct.patchValue({
        timeCodeId: 0,
        timeCodeInvoiceProductId: 0,
      });
    });
  }

  customPatchValue(element: TimeCodeMaterialDTO) {
    this.patchValue(element);

    this.invoiceProducts.clear();
    this.patchInvoiceProducts(element.invoiceProducts);
  }

  patchInvoiceProducts(products: TimeCodeInvoiceProductDTO[]) {
    for (const invoiceProductRow of products) {
      const inventoryRow = new TimeCodeMaterialInvoiceProductRowsForm({
        validationHandler: this.invoiceProductValidationHandler,
        element: invoiceProductRow,
      });
      this.invoiceProducts.push(inventoryRow, { emitEvent: false });
    }
    this.invoiceProducts.updateValueAndValidity();
  }

  addInvoiceProductRow(row: TimeCodeInvoiceProductDTO) {
    const inventoryRow = new TimeCodeMaterialInvoiceProductRowsForm({
      validationHandler: this.invoiceProductValidationHandler,
      element: row,
    });

    this.invoiceProducts.push(inventoryRow, { emitEvent: false });
    this.invoiceProducts.markAsDirty();
    this.markAsDirty();
  }

  updateInvoiceProduct(
    index: number,
    invoiceProduct: TimeCodeInvoiceProductDTO
  ): void {
    this.invoiceProducts.at(index).patchValue({
      invoiceProductId: invoiceProduct.invoiceProductId,
      factor: invoiceProduct.factor,
    });
    this.invoiceProducts.markAsDirty();
    this.invoiceProducts.updateValueAndValidity();
    this.markAsDirty();
  }

  deleteInvoiceProduct(index: number) {
    this.invoiceProducts.removeAt(index);
    this.invoiceProducts.markAsDirty();
    this.markAsDirty();
  }
}
