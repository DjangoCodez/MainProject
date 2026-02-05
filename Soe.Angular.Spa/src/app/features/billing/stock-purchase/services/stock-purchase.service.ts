import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { StockPurchaseFilterDTO } from '../models/stock-purchase.model';
import { generatePurchaseSuggestion } from '@shared/services/generated-service-endpoints/billing/StockPurchase .endpoints';
import { IPurchaseRowFromStockDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';

@Injectable({
  providedIn: 'root',
})
export class StockPurchaseService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    searchDto: new StockPurchaseFilterDTO(),
  };
  getGrid(
    id?: number,
    additionalProps?: {
      searchDto: StockPurchaseFilterDTO;
    }
  ): Observable<IPurchaseRowFromStockDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.post<IPurchaseRowFromStockDTO[]>(
      generatePurchaseSuggestion(),
      this.getGridAdditionalProps.searchDto
    );
  }
}
