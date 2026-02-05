import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getStockInventories,
  getStockInventory,
  saveStockInventoryRows,
  deleteStockInventory,
  generateStockInventoryRows,
  closeStockInventory,
} from '@shared/services/generated-service-endpoints/billing/StockInventory.endpoints';
import {
  IStockInventoryGridDTO,
  IStockInventoryRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  StockInventoryFilterDTO,
  StockInventoryHeadDTO,
} from '../models/stock-inventory.model';
import { DateUtil } from '@shared/util/date-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class StockInventoryService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    includeCompleted: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      includeCompleted: boolean;
    }
  ): Observable<IStockInventoryGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IStockInventoryGridDTO[]>(
      getStockInventories(this.getGridAdditionalProps.includeCompleted, id)
    );
  }

  get(id: number): Observable<StockInventoryHeadDTO> {
    return this.http.get<StockInventoryHeadDTO>(getStockInventory(id)).pipe(
      tap(value => {
        if (value.inventoryStart) {
          value.inventoryStartStr = DateUtil.format(
            new Date(value.inventoryStart),
            'yyyy-MM-dd HH:mm'
          );
        }

        if (value.inventoryStop) {
          value.inventoryStopStr = DateUtil.format(
            new Date(value.inventoryStop),
            'yyyy-MM-dd HH:mm'
          );
        }
      })
    );
  }

  save(model: StockInventoryHeadDTO): Observable<any> {
    return this.http.post<StockInventoryHeadDTO>(
      saveStockInventoryRows(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteStockInventory(id));
  }

  generateStockInventoryRows(
    filter: StockInventoryFilterDTO
  ): Observable<IStockInventoryRowDTO[]> {
    return this.http.post<IStockInventoryRowDTO[]>(
      generateStockInventoryRows(),
      filter
    );
  }

  closeInventory(stockHeadId: number): Observable<BackendResponse> {
    return this.http.get(closeStockInventory(stockHeadId));
  }
}
