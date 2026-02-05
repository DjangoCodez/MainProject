import { Injectable } from '@angular/core';
import { IProductsInStockModel } from '@shared/models/generated-interfaces/BillingModels';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { IUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { validateProductsInStock } from '@shared/services/generated-service-endpoints/billing/StockProduct.endpoints';
import { getSmallDTOUsers } from '@shared/services/generated-service-endpoints/manage/UserV2.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PurchaseRowsService {
  constructor(private http: SoeHttpClient) {}

  getUsers(
    setDefaultRoleName: boolean,
    active?: boolean,
    skipNonEmployeeUsers?: boolean,
    includeEmployeesWithSameAccountOnAttestRole?: boolean,
    includeEmployeeCategories?: boolean,
    showEnded?: boolean
  ): Observable<IUserSmallDTO[]> {
    return this.http.get<IUserSmallDTO[]>(
      getSmallDTOUsers(
        setDefaultRoleName,
        active ?? false,
        skipNonEmployeeUsers ?? false,
        includeEmployeesWithSameAccountOnAttestRole ?? false,
        includeEmployeeCategories ?? false,
        showEnded ?? false
      )
    );
  }

  validateProductsInStock(
    model: IProductsInStockModel
  ): Observable<IProductSmallDTO> {
    return this.http.post<IProductSmallDTO>(validateProductsInStock(), model);
  }
}
