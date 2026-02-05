import { Injectable } from '@angular/core';
import { AsyncValidatorFn } from '@angular/forms';
import { of, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

@Injectable({ providedIn: 'root' })
export class PriceListsValidatorService {
  errorTerm = '';

  constructor(translationService: TranslateService) {
    translationService
      .get(['billing.product.pricelist.invalidrows'])
      .subscribe(terms => {
        this.errorTerm = terms['billing.product.pricelist.invalidrows'];
      });
  }

  public isEmptyProduct(row?: PriceListDTO): boolean {
    //Checking empty rows
    const isInstance = !!row;
    if (isInstance) {
      const isProductEmpty =
        row.productId === undefined ||
        row.productId === null ||
        row.productId === 0;
      const isPriceEmpty =
        row.price === undefined || row.price === null || row.price === 0;
      const isQuantityEmpty =
        row.quantity === undefined ||
        row.quantity === null ||
        row.quantity === 0;

      return isProductEmpty && isPriceEmpty && isQuantityEmpty;
    }

    return true;
  }

  public validDates(row?: PriceListDTO): boolean {
    if (row && row.startDate && row.stopDate) {
      return row.startDate <= row.stopDate;
    }
    return true;
  }

  public validProduct(editableProduct: boolean, row?: PriceListDTO): boolean {
    if (!row || !editableProduct) {
      return true;
    }

    return editableProduct && (this.isEmptyProduct(row) || row.productId > 0);
  }

  public validPriceListType(
    editablePriceListType: boolean,
    row?: PriceListDTO
  ): boolean {
    if (!row || !editablePriceListType) {
      return true;
    }
    return row.priceListTypeId > 0;
  }

  public uniqueCondition(
    priceLists: PriceListDTO[],
    editablePriceListType: boolean,
    editableProduct: boolean
  ) {
    if (!priceLists) return true;

    const unique = new Set<string>();

    const keyBuilder = (r: PriceListDTO) => {
      const priceListTypeId = editablePriceListType ? r.priceListTypeId : '0';
      const productId = editableProduct ? r.productId : '0';
      return (
        priceListTypeId +
          '_' +
          productId +
          '_' +
          r.quantity +
          '_' +
          r.startDate?.getTime() || '0'
      );
    };

    return !priceLists.some(r => {
      if (r.state !== SoeEntityState.Active) {
        return false;
      }

      const key = keyBuilder(r);
      if (unique.has(key)) {
        return true;
      } else {
        unique.add(key);
      }
      return !(
        this.validPriceListType(editablePriceListType, r) &&
        this.validProduct(editableProduct, r) &&
        this.validDates(r)
      );
    });
  }

  validatePriceLists(
    editablePriceListType: boolean,
    editableProduct: boolean
  ): AsyncValidatorFn {
    return control =>
      timer(200).pipe(
        switchMap(() =>
          of(
            this.uniqueCondition(
              control.value,
              editablePriceListType,
              editableProduct
            )
              ? null
              : {
                  custom: {
                    value: this.errorTerm,
                  },
                }
          )
        )
      );
  }
}
