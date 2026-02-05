import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { ImportStockBalancesDTO } from '../models/stock-balance-file-import.model';
import { Observable } from 'rxjs';
import { importStockBalances } from '@shared/services/generated-service-endpoints/billing/StockV2.endpoints';
import { importStockInventory } from '@shared/services/generated-service-endpoints/billing/StockInventory.endpoints';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';

@Injectable({
  providedIn: 'root',
})
export class StockBalanceFileImportService {
  constructor(private http: SoeHttpClient) {}

  importStockBalances(
    model: ImportStockBalancesDTO
  ): Observable<IActionResult> {
    return this.http.post<any>(importStockBalances(), model);
  }

  importStockInventory(
    model: ImportStockBalancesDTO
  ): Observable<IActionResult> {
    return this.http.post<any>(importStockInventory(), model);
  }
}
