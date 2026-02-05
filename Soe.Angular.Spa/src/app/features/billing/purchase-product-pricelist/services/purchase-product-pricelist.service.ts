import { Injectable } from '@angular/core';
import { Observable, map, throwError } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSupplierPricelistsBySupplier,
  getSupplierPricelistById,
  saveSupplierPricelist,
  deleteSupplierPricelist,
  getSupplierPricelist,
  getSupplierProductPriceCompare,
  performPricelistImport,
} from '@shared/services/generated-service-endpoints/billing/SupplierProductPriceList.endpoints';
import {
  ISupplierProductImportDTO,
  ISupplierProductPriceListGridDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';
import {
  SupplierProductPriceComparisonDTO,
  SupplierProductPriceListGridHeaderDTO,
  SupplierProductPriceListSaveDTO,
  SupplierProductPricelistDTO,
} from '../models/purchase-product-pricelist.model';
import { DateUtil } from '@shared/util/date-util';
import { IImportDynamicResultDTO } from '@shared/models/generated-interfaces/ImportDynamicDTO';

@Injectable({
  providedIn: 'root',
})
export class PurchaseProductPricelistService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISupplierProductPriceListGridDTO[]> {
    return this.http.get<ISupplierProductPriceListGridDTO[]>(
      getSupplierPricelistsBySupplier(id)
    );
  }

  get(pricelistId: number): Observable<SupplierProductPricelistDTO> {
    return this.http.get<SupplierProductPricelistDTO>(
      getSupplierPricelistById(pricelistId)
    );
  }

  getSupplierPriceGrid(
    supplierId: number,
    includeComparison: boolean
  ): Observable<SupplierProductPriceComparisonDTO[]> {
    return this.http
      .get<
        SupplierProductPriceComparisonDTO[]
      >(getSupplierPricelist(supplierId, includeComparison))
      .pipe(
        map(value => {
          return this.fixDates(value);
        })
      );
  }

  getSupplierPriceCompare(
    model: SupplierProductPriceListGridHeaderDTO
  ): Observable<SupplierProductPriceComparisonDTO[]> {
    return this.http
      .post<
        SupplierProductPriceComparisonDTO[]
      >(getSupplierProductPriceCompare(), model)
      .pipe(
        map(
          (
            products: SupplierProductPriceComparisonDTO[]
          ): SupplierProductPriceComparisonDTO[] => {
            const emptyDate = new Date('0001-01-01');
            const defualtMinDate1 = new Date('1900-01-01'),
              defualtMinDate2 = new Date('1901-01-01');
            const defaultMaxDate = new Date('9999-01-01');

            const formatMinDate = (date: any): Date | undefined => {
              const d = new Date(date);
              return DateUtil.isValidDate(d) &&
                !(
                  d.getTime() === emptyDate.getTime() ||
                  d.getTime() === defualtMinDate1.getTime() ||
                  d.getTime() === defualtMinDate2.getTime()
                )
                ? d
                : undefined;
            };
            const formatMaxDate = (date: any): Date | undefined => {
              const d = new Date(date);
              return DateUtil.isValidDate(d) &&
                !(d.getTime() === defaultMaxDate.getTime())
                ? d
                : undefined;
            };

            products.forEach(product => {
              product.compareStartDate = formatMinDate(
                product.compareStartDate
              );
              product.compareEndDate = formatMaxDate(product.compareEndDate);
            });
            return products;
          }
        )
      );
  }

  save(mode: SupplierProductPricelistDTO): Observable<any> {
    return throwError(
      () =>
        new Error('Do not use this method.Created to comply with IApiService')
    );
  }

  saveData(model: SupplierProductPriceListSaveDTO): Observable<any> {
    return this.http.post<SupplierProductPriceListSaveDTO>(
      saveSupplierPricelist(),
      model
    );
  }

  delete(pricelistId: number): Observable<any> {
    return this.http.delete(deleteSupplierPricelist(pricelistId));
  }

  performPricelistImport(
    model: ISupplierProductImportDTO
  ): Observable<IImportDynamicResultDTO> {
    return this.http.post(performPricelistImport(), model);
  }

  fixDates(
    data: SupplierProductPriceComparisonDTO[]
  ): SupplierProductPriceComparisonDTO[] {
    const startDate = new Date('1901-01-02');
    const stopDate = new Date('9998-12-31');

    return data.map(r => {
      const rowStart = DateUtil.parseDateOrJson(r.startDate);
      const rowEnd = DateUtil.parseDateOrJson(r.endDate);
      const rowStartCompare = DateUtil.parseDateOrJson(r.compareStartDate);
      const rowEndCompare = DateUtil.parseDateOrJson(r.compareEndDate);

      if (rowStart && rowStart < startDate) r.startDate = undefined;
      if ((rowEnd && rowEnd > stopDate) || (rowEnd && rowEnd < startDate))
        r.endDate = undefined;
      if (rowStartCompare && rowStartCompare <= startDate)
        r.compareStartDate = undefined;
      if (rowEndCompare && rowEndCompare >= stopDate)
        r.compareEndDate = undefined;
      return r;
    });
  }
}
