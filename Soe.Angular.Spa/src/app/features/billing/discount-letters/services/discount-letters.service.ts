import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ISupplierAgreementModel,
  ISupplierNetPricesDeleteModel,
} from '@shared/models/generated-interfaces/BillingModels';

import { ISupplierAgreementDTO } from '@shared/models/generated-interfaces/SupplierAgreementDTOs';
import { IWholsellerNetPriceRowDTO } from '@shared/models/generated-interfaces/WholsellerNetPriceDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { getPriceLists } from '@shared/services/generated-service-endpoints/billing/InvoicePriceLists.endpoints';
import {
  deleteSupplierAgreements,
  getSupplierAgreementProviders,
  getSupplierAgreements,
  saveSupplierAgreementDiscount,
  saveSupplierAgreements,
} from '@shared/services/generated-service-endpoints/billing/SupplierAgreements.endpoints';
import {
  getNetPrices,
  deleteNetPriceRows,
  saveNetPrices,
  getWholeSellers,
} from '@shared/services/generated-service-endpoints/billing/WholeSellerNetPrices.endpoints';
import { Observable, of } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class DiscountLettersService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISupplierAgreementDTO[]> {
    return of([]);
  }

  getSupplierAgreements(
    providerType: number
  ): Observable<ISupplierAgreementDTO[]> {
    return this.http.get<ISupplierAgreementDTO[]>(
      getSupplierAgreements(providerType)
    );
  }

  getNetPrices(providerType: number): Observable<IWholsellerNetPriceRowDTO[]> {
    return this.http.get<IWholsellerNetPriceRowDTO[]>(
      getNetPrices(providerType)
    );
  }

  deleteNetPriceRows(
    deleteModel: ISupplierNetPricesDeleteModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(deleteNetPriceRows(), deleteModel);
  }

  save(model: ISupplierAgreementDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveSupplierAgreementDiscount(),
      model
    );
  }

  getNetWholeSellers(
    onlyCurrentCountry: boolean,
    onlySeparateFile: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getWholeSellers(onlyCurrentCountry, onlySeparateFile)
    );
  }

  importSupplierAgreement(
    model: ISupplierAgreementModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveSupplierAgreements(), model);
  }

  importNetPrices(model: ISupplierAgreementModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveNetPrices(), model);
  }

  delete(
    wholesellerId: number,
    priceListTypeId: number
  ): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteSupplierAgreements(wholesellerId, priceListTypeId)
    );
  }

  getProvidersDict(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getSupplierAgreementProviders());
  }

  getPriceLists(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getPriceLists(addEmptyRow));
  }
}
