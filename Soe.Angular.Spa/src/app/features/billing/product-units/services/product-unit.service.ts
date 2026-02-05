import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteProductUnit,
  getProductUnit,
  getProductUnits,
  getProductUnitsDict,
  saveProductUnit,
} from '@shared/services/generated-service-endpoints/billing/ProductUnit.endpoints';
import { Observable, map } from 'rxjs';
import { ProductUnitSmallDTO } from '../models/product-units.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductUnitService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps: { useCache: boolean; cacheExpireTime?: number } = {
    useCache: false,
    cacheExpireTime: undefined,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      useCache: boolean;
      cacheExpireTime?: number;
    }
  ): Observable<ProductUnitSmallDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<ProductUnitSmallDTO[]>(getProductUnits(id), {
      useCache: this.getGridAdditionalProps.useCache,
      cacheOptions: { expires: this.getGridAdditionalProps.cacheExpireTime },
    });
  }

  get(id: number): Observable<ProductUnitSmallDTO> {
    return this.http.get<ProductUnitSmallDTO>(getProductUnit(id)).pipe(
      map(data => {
        const obj = new ProductUnitSmallDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  getProductUnitsDict(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getProductUnitsDict());
  }

  save(model: any): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProductUnit(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteProductUnit(id));
  }
}
