import { Injectable } from '@angular/core';
import { IStockInventoryRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BehaviorSubject, Observable } from 'rxjs';
import { StockInventoryHeadForm } from '../../../../models/stock-inventory-head-form.model';

@Injectable()
export class StockInventoryEditItemGridService {
  private rowsSource = new BehaviorSubject<IStockInventoryRowDTO[]>([]);
  private _rows$?: Observable<IStockInventoryRowDTO[]>;
  private touchForm: (
    values: IStockInventoryRowDTO[],
    setDirty: boolean
  ) => void = () => {};
  private _isInIt = false;

  get rows$() {
    if (!this._rows$) {
      this._rows$ = this.rowsSource.asObservable();
    }
    return this._rows$;
  }

  get isInit() {
    return this._isInIt;
  }

  private get rows() {
    return this.rowsSource.value;
  }

  private set rows(rows: IStockInventoryRowDTO[]) {
    rows ||= [];
    this.rowsSource.next(rows);
  }

  public init(form: StockInventoryHeadForm) {
    if (!this._isInIt) {
      const value = form?.stockInventoryRows?.value as IStockInventoryRowDTO[];
      this.setData(value || []);
      this.setFormReference(form);
      this._isInIt = true;
    }
  }

  private setFormReference(form?: StockInventoryHeadForm) {
    if (!form) return;

    this.touchForm = (values: IStockInventoryRowDTO[], setDirty = false) => {
      if (!form.dirty && setDirty) form.stockInventoryRows?.markAsDirty();
      form.addStockInventoryRows(values);
    };
  }

  public getRows = () => this.rows;

  public setData(rows: IStockInventoryRowDTO[]) {
    this.rows = rows;
  }

  public getDataForSave(): IStockInventoryRowDTO[] {
    return this.rows;
  }

  public updateRows() {
    this.rows = [...this.rows];
  }

  public rowModified() {
    this.touchForm(this.rows, true);
  }

  public updateInventoryRowDiff(
    row: IStockInventoryRowDTO,
    inventoryCount: number
  ): IStockInventoryRowDTO | undefined {
    const stockInventoryRow = this.rows.find(
      x => x.stockInventoryRowId === row.stockInventoryRowId
    );
    if (stockInventoryRow) {
      const diff =
        -1 * this.getDifference(Number(row.startingSaldo), inventoryCount);
      stockInventoryRow.inventorySaldo = inventoryCount;
      stockInventoryRow.difference = diff;
      this.updateRows();
      this.touchForm(this.rows, true);
    }

    return stockInventoryRow;
  }

  private getDifference(wareHouseQty: number, inventoryQty = 0): number {
    return wareHouseQty - inventoryQty;
  }

  public updateInventoryRowsTransactionDate(
    selectedIds: number[],
    selectedDate: Date
  ) {
    this.rows
      .filter(x => selectedIds.includes(x.stockInventoryRowId))
      .forEach(row => {
        row.transactionDate = selectedDate;
      });
    this.updateRows();
    this.touchForm(this.rows, true);
  }

  public updateInventoryRowsQuantity(selectedIds: number[]) {
    this.rows
      .filter(
        x =>
          selectedIds.includes(x.stockInventoryRowId) &&
          x.inventorySaldo === 0 &&
          !x.transactionDate
      )
      .forEach(row => {
        row.inventorySaldo = row.startingSaldo;
        row.difference = this.getDifference(
          Number(row.startingSaldo),
          Number(row.startingSaldo)
        );
      });
    this.updateRows();
    this.touchForm(this.rows, true);
  }

  public updateTransactionDate(
    inventoryRowId: number,
    previnventoryRowId?: number
  ): IStockInventoryRowDTO | undefined {
    const row = this.rows.find(x => x.stockInventoryRowId === inventoryRowId);
    const preRow = this.rows.find(
      x => x.stockInventoryRowId === previnventoryRowId
    );

    if (row && preRow && preRow.transactionDate && !row.transactionDate) {
      row.transactionDate = preRow.transactionDate;
      this.updateRows();
      this.touchForm(this.rows, true);
    } else if (row && !row.transactionDate) {
      row.transactionDate = new Date();
      this.updateRows();
      this.touchForm(this.rows, true);
    }

    return row;
  }

  public updateTransactionDatebyValue(
    inventoryRowId: number,
    trDate?: Date
  ): void {
    const row = this.rows.find(x => x.stockInventoryRowId === inventoryRowId);
    if (row) {
      row.transactionDate = trDate;
      this.updateRows();
      this.touchForm(this.rows, true);
    }
  }
}
