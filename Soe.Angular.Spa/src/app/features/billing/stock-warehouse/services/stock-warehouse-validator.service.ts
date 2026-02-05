import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StockShelfDTO } from '../models/stock-warehouse.model';
import { ValidatorFn } from '@angular/forms';
import { of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StockWarehouseValidatorService {
  errorTerm = '';
  terms: any = [];
  constructor(translationService: TranslateService) {
    translationService
      .get(['billing.stock.stockplaces.invalidrows'])
      .subscribe(terms => {
        this.terms = terms;
        this.errorTerm = this.terms['billing.stock.stockplaces.invalidrows'];
      });
  }
  public isEmptyStockShelf(row?: StockShelfDTO): boolean {
    //Checking empty rows
    if (row) {
      const isCodeEmpty =
        row.code === undefined || row.code === null || row.code === '';
      const isNameEmpty =
        row.name === undefined || row.name === null || row.name === '';

      return isCodeEmpty && isNameEmpty;
    }

    return true;
  }

  public validateStockShelfEntry(rows?: StockShelfDTO[]): boolean {
    let validRows = true;
    if (rows) {
      rows?.forEach((row?: StockShelfDTO) => {
        if (row) {
          if (!row.isDelete && !this.isEmptyStockShelf(row)) {
            if (
              row.code === null ||
              row.code === '' ||
              row.name === null ||
              row.name === ''
            ) {
              validRows = false;
            }
          }
        }
      });
    }
    return validRows;
  }

  public validateStockShelfs(): ValidatorFn {
    return control =>
      of(
        this.validateStockShelfEntry(control.value)
          ? null
          : {
              custom: {
                value: this.errorTerm,
              },
            }
      );
  }
}
