import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { getInvoiceProductsSmall } from '@shared/services/generated-service-endpoints/billing/InvoiceProduct.endpoints';
import { Observable } from 'rxjs';
import { IPriceListDTO } from '../../../../shared/models/generated-interfaces/PriceListDTOs';
import { IPriceListTypeGridDTO } from '../../../../shared/models/generated-interfaces/PriceListTypeDTOs';

import {
  deletePriceListType,
  getPriceLists,
  getPriceListType,
  getPriceListTypesGrid,
  savePriceListType,
  getPriceListsDict,
} from '../../../../shared/services/generated-service-endpoints/billing/PriceList.endpoints';
import {
  InvoiceProductSmallDTO,
  PriceListTypeDTO,
} from '../models/customer-product-pricelist.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomerProductPricelistsTypeService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IPriceListTypeGridDTO[]> {
    return this.http.get<IPriceListTypeGridDTO[]>(getPriceListTypesGrid(id));
  }

  get(priceListTypeId: number) {
    return this.http.get<PriceListTypeDTO>(getPriceListType(priceListTypeId));
  }

  save(head: PriceListTypeDTO, prices: IPriceListDTO[]) {
    const dto = {
      priceListType: head,
      priceLists: prices,
    };
    return this.http.post<BackendResponse>(savePriceListType(), dto);
  }

  delete(priceListTypeId: number) {
    return this.http.delete<BackendResponse>(
      deletePriceListType(priceListTypeId)
    );
  }

  getPriceLists(priceListTypeId: number) {
    return this.http.get<IPriceListDTO[]>(getPriceLists(priceListTypeId));
  }

  getProducts(excludeExternal: boolean) {
    return this.http.get<InvoiceProductSmallDTO[]>(
      getInvoiceProductsSmall(excludeExternal)
    );
  }

  getPriceListsDict(
    addEmptyRow: boolean = true
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getPriceListsDict(addEmptyRow));
  }
}
