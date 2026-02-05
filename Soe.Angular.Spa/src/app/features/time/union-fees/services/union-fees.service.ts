import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import {
  IUnionFeeDTO,
  IUnionFeeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteUnionFee,
  getUnionFee,
  getUnionFeesGrid,
  saveUnionFee,
  getPayrollPriceTypesDict,
  getUnionFeePayrollProducts,
} from '@shared/services/generated-service-endpoints/time/UnionFee.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UnionFeesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IUnionFeeGridDTO[]> {
    return this.http.get<IUnionFeeGridDTO[]>(getUnionFeesGrid(id));
  }

  get(unionFeeId: number): Observable<IUnionFeeDTO> {
    return this.http.get<IUnionFeeDTO>(getUnionFee(unionFeeId));
  }

  save(model: IUnionFeeDTO): Observable<any> {
    return this.http.post<IUnionFeeDTO>(saveUnionFee(), model);
  }

  delete(unionFeeId: number): Observable<any> {
    return this.http.delete(deleteUnionFee(unionFeeId));
  }

  getPayrollPriceTypesDict(): Observable<ISmallGenericType[]> {
    return this.http.get(getPayrollPriceTypesDict());
  }

  getUnionFeePayrollProducts(): Observable<IProductSmallDTO[]> {
    return this.http.get<IProductSmallDTO[]>(getUnionFeePayrollProducts());
  }
}
