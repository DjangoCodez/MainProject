import { Injectable } from '@angular/core';
import {
  IStockProductDTO,
  IStockTransactionDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getStockProduct,
  getStockProductTransactions,
  getStockProducts,
  saveStockTransaction,
  getStockProductProducts,
  getStockProductsByProductId,
  getStockProductsByStockId,
} from '@shared/services/generated-service-endpoints/billing/StockProduct.endpoints';
import { Observable, of } from 'rxjs';
import {
  StockProductDTO,
  StockTransactionDTO,
} from '../models/stock-balance.model';
import {
  recalculateStockBalance,
  getStocksDict,
} from '@shared/services/generated-service-endpoints/billing/StockV2.endpoints';
import { getSmallGenericSysWholesellers } from '@shared/services/generated-service-endpoints/billing/SysWholeseller.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class StockBalanceService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    includeInactive: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      includeInactive: boolean;
    }
  ): Observable<IStockProductDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IStockProductDTO[]>(
      getStockProducts(this.getGridAdditionalProps.includeInactive, id)
    );
  }

  getStockProductsByStockId(stockId: number): Observable<IStockProductDTO[]> {
    return this.http.get<IStockProductDTO[]>(
      getStockProductsByStockId(stockId)
    );
  }

  get(stockProductId: number): Observable<IStockProductDTO> {
    return this.http.get<IStockProductDTO>(getStockProduct(stockProductId));
  }

  getStockProductsByProductId(
    productId: number
  ): Observable<IStockProductDTO[]> {
    return this.http.get<IStockProductDTO[]>(
      getStockProductsByProductId(productId)
    );
  }

  getStocksDict(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getStocksDict(addEmptyRow));
  }

  getSmallGenericSysWholesellers(
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getSmallGenericSysWholesellers(addEmptyRow)
    );
  }

  getStockProductTransactions(
    stockProductId: number
  ): Observable<StockTransactionDTO[]> {
    return this.http.get<StockTransactionDTO[]>(
      getStockProductTransactions(stockProductId)
    );
  }

  save(model: StockProductDTO): Observable<any> {
    const StockTransactionDTOs = [];
    StockTransactionDTOs.push(model.transaction);
    return this.http.post<StockTransactionDTO[]>(
      saveStockTransaction(),
      StockTransactionDTOs
    );
  }

  saveTransactions(stockTransactions: IStockTransactionDTO[]): Observable<any> {
    return this.http.post<IStockTransactionDTO[]>(
      saveStockTransaction(),
      stockTransactions
    );
  }

  delete(stockProductId: number): Observable<any> {
    return of();
  }

  recalCulateStockBalance(stockId: number): Observable<any> {
    return this.http.post<any>(recalculateStockBalance(stockId), stockId);
  }

  getStockProductProducts(
    stockId?: number,
    onlyActive?: boolean
  ): Observable<IProductSmallDTO[]> {
    return this.http.get<IProductSmallDTO[]>(
      getStockProductProducts(stockId ?? 0, onlyActive ?? false)
    );
  }
}
