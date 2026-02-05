import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  SupplierProductPriceComparisonDTO,
  SupplierProductPricelistDTO,
} from './purchase-product-pricelist.model';
import { FormArray } from '@angular/forms';
import { SupplierProductPriceComparisonForm } from './purchase-product-pricelist-comparison-form.model';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

interface ISupplierProductPriceListForm {
  validationHandler: ValidationHandler;
  element: SupplierProductPricelistDTO | undefined;
}

export class SupplierProductPriceListForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISupplierProductPriceListForm) {
    super(validationHandler, {
      supplierProductPriceListId: new SoeTextFormControl(
        element?.supplierProductPriceListId || 0,
        { isIdField: true }
      ),
      supplierId: new SoeSelectFormControl(
        element?.supplierId || null,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'billing.purchase.supplier'
      ),
      startDate: new SoeDateFormControl(
        element?.startDate || null,
        {
          required: true,
        },
        'billing.purchase.pricelists.activefrom'
      ),
      endDate: new SoeDateFormControl(
        element?.endDate || null,
        {
          required: true,
        },
        'billing.purchase.pricelists.activeto'
      ),
      currencyId: new SoeSelectFormControl(
        element?.currencyId || 0,
        {
          required: true,
        },
        'common.currency'
      ),
      supplierName: new SoeTextFormControl(element?.supplierNr || undefined, {
        isNameField: true,
      }),
      priceRows: new FormArray<SupplierProductPriceComparisonForm>([]),
    });
    this.thisValidationHandler = validationHandler;
    this.resetPriceRows(element?.priceRows ?? []);
  }

  get supplierProductPriceListId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierProductPriceListId;
  }

  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }

  get startDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.startDate;
  }

  get endDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.endDate;
  }

  get currencyId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.currencyId;
  }

  get supplierName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierName;
  }

  get priceRows(): FormArray<SupplierProductPriceComparisonForm> {
    return <FormArray<SupplierProductPriceComparisonForm>>(
      this.controls.priceRows
    );
  }

  resetPriceRows(rows: SupplierProductPriceComparisonDTO[]): void {
    this.updatePriceRows(rows);
    this.priceRows.markAsUntouched();
    this.priceRows.markAsPristine();
    this.priceRows.updateValueAndValidity();
  }

  updatePriceRows(rows: SupplierProductPriceComparisonDTO[]): void {
    this.priceRows.clear({ emitEvent: false });
    rows.forEach(row => {
      this.priceRows.push(
        new SupplierProductPriceComparisonForm({
          validationHandler: this.thisValidationHandler,
          element: row,
        }),
        { emitEvent: false }
      );
    });
  }

  addPriceRow(
    row: SupplierProductPriceComparisonDTO
  ): SupplierProductPriceComparisonDTO {
    if (row.supplierProductPriceId === 0)
      row.supplierProductPriceId = this.getTempPriceListProductId();

    this.priceRows.push(
      new SupplierProductPriceComparisonForm({
        validationHandler: this.thisValidationHandler,
        element: row,
      }),
      { emitEvent: false }
    );

    return row;
  }

  updatePriceRow(
    row: SupplierProductPriceComparisonDTO,
    setDirty: boolean
  ): void {
    this.priceRows.controls.forEach(r => {
      if (r.supplierProductPriceId.value === row.supplierProductPriceId) {
        r.patchValue({
          supplierProductId: row.supplierProductId,
          productName: row.productName,
          ourProductName: row.ourProductName,
          compareQuantity: row.compareQuantity,
          comparePrice: row.comparePrice,
          compareStartDate: row.compareStartDate,
          compareEndDate: row.compareEndDate,
          quantity: row.quantity,
          price: row.price,
          isModified: setDirty,
        });
        if (setDirty) {
          this.priceRows.markAsDirty();
          this.markAsDirty();
        } else {
          this.priceRows.markAsUntouched();
          this.markAsUntouched();
        }
        return;
      }
    });
  }

  deletePriceRow(productPriceIds: Array<number>): void {
    if (productPriceIds.length > 0) {
      this.priceRows.controls.forEach(r => {
        if (productPriceIds.includes(r.supplierProductPriceId.value)) {
          this.priceRows.markAsDirty();
          this.markAsDirty();
          const dd = +SoeEntityState.Deleted;
          r.patchValue({
            isModified: true,
            entityState: dd,
            state: dd,
          });
        }
      });
    }
  }

  private getTempPriceListProductId(): number {
    let minId = 0;

    if (this.priceRows.value.length > 0) {
      minId = (<SupplierProductPriceComparisonDTO[]>this.priceRows.value)
        .map(x => x.supplierProductPriceId)
        .reduce((a, b) => Math.min(a, b));
    }

    return minId > 0 ? 0 : --minId;
  }
}
