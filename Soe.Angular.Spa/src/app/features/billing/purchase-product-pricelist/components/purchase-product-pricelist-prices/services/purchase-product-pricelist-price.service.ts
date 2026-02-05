import { Injectable } from '@angular/core';
import { SupplierProductPriceListForm } from '../../../models/purchase-product-pricelist-form.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { SupplierProductPriceComparisonDTO } from '../../../models/purchase-product-pricelist.model';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

@Injectable()
export class PurchaseProductPriceListPriceService {
  private _form?: SupplierProductPriceListForm;
  private rowsSource = new BehaviorSubject<SupplierProductPriceComparisonDTO[]>(
    []
  );
  private _rows$?: Observable<SupplierProductPriceComparisonDTO[]>;

  get rows$(): Observable<SupplierProductPriceComparisonDTO[]> {
    if (!this._rows$) {
      this._rows$ = this.rowsSource.asObservable();
    }
    return this._rows$;
  }

  private get rows(): SupplierProductPriceComparisonDTO[] {
    return this.rowsSource.value;
  }

  private set rows(rows: SupplierProductPriceComparisonDTO[]) {
    rows ||= [];
    this.rowsSource.next(rows);
  }

  private setData(priceRows: SupplierProductPriceComparisonDTO[]): void {
    this.rows = priceRows.filter(x => x.entityState !== SoeEntityState.Deleted);
  }

  public init(
    form: SupplierProductPriceListForm,
    priceRows: SupplierProductPriceComparisonDTO[]
  ): void {
    this._form = form;
    form.resetPriceRows(priceRows);
    this.setData(priceRows);
  }

  public rowIsModified(row: SupplierProductPriceComparisonDTO) {
    row.isModified = true;
  }

  addRow(): void {
    if (this._form) {
      const row = this._form.addPriceRow(
        new SupplierProductPriceComparisonDTO()
      );
      this.rowIsModified(row);
      this.setData([...this.rows, row]);
    }
  }

  addRows(priceRows: SupplierProductPriceComparisonDTO[]): void {
    if (this._form) {
      for (let r = 0; r < priceRows.length; r++) {
        priceRows[r] = this._form.addPriceRow(priceRows[r]);
        this.rowIsModified(priceRows[r]);
      }
      this.setData([...this.rows, ...priceRows]);
    }
  }

  updateRow(row: SupplierProductPriceComparisonDTO, setDirty: boolean): void {
    if (this._form) {
      this._form.updatePriceRow(row, setDirty);
      this.setData(
        this._form.priceRows.value as SupplierProductPriceComparisonDTO[]
      );
    }
  }

  deleteRows(productPriceIds: Array<number>): void {
    if (this._form) {
      this._form?.deletePriceRow(productPriceIds);

      setTimeout(() => {
        this.setData(
          this._form!.priceRows.value as SupplierProductPriceComparisonDTO[]
        );
      });
    }
  }
}
