import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IStockDTO,
  IStockGridDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteStock,
  getGridStocks,
  getStock,
  getStockPlaces,
  getStocks,
  getStocksDict,
  getStocksForInvoiceProduct,
  saveStock,
  validateStockPlace,
} from '@shared/services/generated-service-endpoints/billing/StockV2.endpoints';
import { Observable } from 'rxjs';
import { StockDTO } from '../models/stock-warehouse.model';
import { getStockProductsByStockId } from '@shared/services/generated-service-endpoints/billing/StockProduct.endpoints';
import { StockProductDTO } from '@features/billing/stock-balance/models/stock-balance.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class StockWarehouseService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IStockGridDTO[]> {
    return this.http.get<IStockGridDTO[]>(getGridStocks(id));
  }

  get(id: number): Observable<StockDTO> {
    return this.http.get<StockDTO>(getStock(id, true));
  }

  getStocksByProduct(id: number): Observable<StockDTO[]> {
    return this.http.get<StockDTO[]>(getStocksForInvoiceProduct(id));
  }

  save(model: StockDTO): Observable<any> {
    return this.http.post<StockDTO>(saveStock(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteStock(id));
  }

  getStocks(addEmptyRow: boolean): Observable<IStockDTO[]> {
    return this.http.get<IStockDTO[]>(getStocks(addEmptyRow));
  }

  getStockWarehousesDict(
    addEmptyRow: boolean,
    sort?: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getStocksDict(addEmptyRow, sort));
  }

  getStockShelves(
    addEmptyRow: boolean,
    stockWarehouseId: number
  ): Observable<IStockShelfDTO[]> {
    return this.http.get(getStockPlaces(addEmptyRow, stockWarehouseId));
  }

  validateShelfBeforeDelete(stockShelfId: number): Observable<BackendResponse> {
    return this.http.get(validateStockPlace(stockShelfId));
  }

  getStockProducts(stockId: number): Observable<StockProductDTO[]> {
    return this.http.get<StockProductDTO[]>(getStockProductsByStockId(stockId));
  }
}
