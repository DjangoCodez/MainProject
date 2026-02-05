import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSupplierProductList,
  getSupplierProduct,
  saveProduct,
  deleteProduct,
  getSupplierByInvoiceProduct,
  getSupplierProductsDict,
  getSupplierProductsSmall,
  getSupplierProductListDict,
  getSupplierProductByInvoiceProduct,
} from '@shared/services/generated-service-endpoints/billing/SupplierPurchaseProduct.endpoints';
import { ISupplierProductGridDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import {
  SupplierProductDTO,
  SupplierProductGridHeaderDTO,
  SupplierProductSmallDTO,
} from '../models/purchase-product.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PurchaseProductsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    searchDto: new SupplierProductGridHeaderDTO(),
  };
  getGrid(
    id?: number,
    additionalProps?: {
      searchDto: SupplierProductGridHeaderDTO;
    }
  ): Observable<ISupplierProductGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.post<ISupplierProductGridDTO[]>(
      getSupplierProductList(),
      this.getGridAdditionalProps.searchDto
    );
  }

  getAllDict(
    searchDto: SupplierProductGridHeaderDTO
  ): Observable<SmallGenericType[]> {
    return this.http.post<SmallGenericType[]>(
      getSupplierProductListDict(),
      searchDto
    );
  }

  get(supplierProductId: number): Observable<SupplierProductDTO> {
    return this.http.get<SupplierProductDTO>(
      getSupplierProduct(supplierProductId)
    );
  }

  getSupplierProductsDict(
    supplierProductId: number
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSupplierProductsDict(supplierProductId)
    );
  }

  getSupplierProductsSmall(
    supplierProductId: number
  ): Observable<SupplierProductSmallDTO[]> {
    return this.http.get<SupplierProductSmallDTO[]>(
      getSupplierProductsSmall(supplierProductId)
    );
  }

  getSupplierProductByInvoiceProduct(
    invoiceProductId: number,
    supplierId: number
  ): Observable<SupplierProductDTO> {
    return this.http.get<SupplierProductDTO>(
      getSupplierProductByInvoiceProduct(invoiceProductId, supplierId)
    );
  }

  getSupplierByProductId(
    supplierProductId: number
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSupplierByInvoiceProduct(supplierProductId)
    );
  }

  save(model: SupplierProductDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProduct(), model);
  }

  delete(supplierProductId: number): Observable<BackendResponse> {
    return this.http.delete(deleteProduct(supplierProductId));
  }
}
