import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IPriceListDTO } from '../../../../../../shared/models/generated-interfaces/PriceListDTOs';
import { InvoiceProductSmallDTO } from '../../../models/customer-product-pricelist.model';
import { PriceListsValidatorService } from './pricelists-validator.service';
import { CustomerProductPriceListTypeForm } from '../../../models/customer-product-pricelisttype-form.model';
import { SoeEntityState } from '../../../../../../shared/models/generated-interfaces/Enumerations';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

export type PriceListsForm = CustomerProductPriceListTypeForm; //| IProductForm...

@Injectable()
export class CustomerProductPriceListsService {
  private rowsSource = new BehaviorSubject<PriceListDTO[]>([]);
  private _rows$?: Observable<PriceListDTO[]>;
  private touchForm: (values: PriceListDTO[], setDirty: boolean) => void =
    () => {};
  private productsEditable = false;
  private priceListTypeEditable = false;
  public showHistoricPrices = false;

  constructor(private validator: PriceListsValidatorService) {}

  get rows$() {
    if (!this._rows$) {
      this._rows$ = this.rowsSource.asObservable();
    }
    return this._rows$;
  }

  private get rows() {
    return this.rowsSource.value;
  }

  private set rows(rows: PriceListDTO[]) {
    rows ||= [];
    this.touchForm(rows, false);
    this.rowsSource.next(rows);
  }

  public setData(rows: PriceListDTO[]) {
    this.rows = rows;
  }

  public getDataForSave(): IPriceListDTO[] {
    const rowsForSave = this.rows.filter(
      r => r.isModified && !this.validator.isEmptyProduct(r)
    );

    return rowsForSave.map(r => PriceListDTO.fromClient(r));
  }

  public updateRows() {
    this.rows = [...this.rows];
  }

  public init(
    form: PriceListsForm | undefined,
    productsEditable: boolean,
    priceListTypeEditable: boolean
  ) {
    this.productsEditable = productsEditable;
    this.priceListTypeEditable = priceListTypeEditable;
    const value = form?.getPriceLists()?.value as PriceListDTO[];
    this.setData(value || []);
    this.setFormReference(form);

    if (form?.isNew) {
      this.clearKeys();
    }
  }

  private setFormReference(form?: PriceListsForm) {
    if (!form) return;

    const control = form.getPriceLists();
    control?.addAsyncValidators(
      this.validator.validatePriceLists(
        this.priceListTypeEditable,
        this.productsEditable
      )
    );
    control?.setValue(this.rows);

    this.touchForm = (values: PriceListDTO[], setDirty = false) => {
      if (!form.dirty && setDirty) form.getPriceLists()?.markAsDirty();
      form.setPriceLists(values);
    };
  }

  setProductValue(row: PriceListDTO, product?: InvoiceProductSmallDTO) {
    if (row) {
      row.productId = product?.productId ?? 0;
      row.name = product?.name ?? '';
      row.number = product?.number ?? '';
      row.purchasePrice = product?.purchasePrice;
      if (!product) {
        row.quantity = 0;
        row.price = 0;
      }
    }
  }

  public deleteRow(row: PriceListDTO) {
    if (row.priceListId) {
      row.state = SoeEntityState.Deleted;
      this.rowIsModified(row);
      this.updateRows();
    } else {
      this.rows = this.rows.filter(r => r !== row);
    }
  }

  public addRow() {
    const row = new PriceListDTO();
    this.rowIsModified(row);
    this.rows = [...this.rows, row];
  }

  public rowIsModified(row: PriceListDTO) {
    row.isModified = true;
    this.touchForm(this.rows, true);
  }

  public clearKeys() {
    this.rows.forEach(r => {
      r.isModified = true;
      r.priceListId = 0;
      r.priceListTypeId = 0;
    });
  }

  public filterRows(rows: PriceListDTO[]): PriceListDTO[] {
    const today = new Date();
    return rows.filter(r => {
      if (
        r.state === SoeEntityState.Deleted ||
        (!r.isModified &&
          !this.showHistoricPrices &&
          r.stopDate &&
          r.stopDate < today)
      ) {
        return false;
      }
      return true;
    });
  }

  public clearEmptyRows(): void {
    this.rows = this.rows.filter(r => !this.validator.isEmptyProduct(r));
  }
}
