import { Injectable } from '@angular/core';
import { TermGroup_PurchaseCartStatus } from '@shared/models/generated-interfaces/Enumerations';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  changePriceOptimizationStatus,
  deletePriceOptimization,
  deletePriceOptimizations,
  getPriceOptimization,
  getPriceOptimizationRow,
  getPriceOptimizationRowPrices,
  getPriceOptimizationsForGrid,
  savePriceOptimization,
  transferPriceOptimizationRowsToOrder,
} from '@shared/services/generated-service-endpoints/billing/PriceOptimization.endpoints';
import { Observable } from 'rxjs';
import {
  ChangeCartStateModel,
  PurchaseCartDTO,
  PurchaseCartRowDTO,
} from '../models/price-optimization.model';
import { ITransferInvoiceDTO } from '@shared/models/generated-interfaces/PurchaseCartDTOs';
import { getSmallGenericSysWholesellers } from '@shared/services/generated-service-endpoints/billing/SysWholeseller.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ISysWholsesellerPriceSearchDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PriceOptimizationService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps: {
    allItemsSelectionId: number;
    selectedCartStatusIds: number[];
  } = {
    allItemsSelectionId: 0,
    selectedCartStatusIds: [TermGroup_PurchaseCartStatus.Open],
  };

  getGrid(
    id?: number,
    additionalProps?: {
      allItemsSelectionId: number;
      selectedCartStatusIds: number[];
    }
  ): Observable<PurchaseCartDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;

    return this.http.get<PurchaseCartDTO[]>(
      getPriceOptimizationsForGrid(
        this.getGridAdditionalProps.allItemsSelectionId,
        this.getGridAdditionalProps.selectedCartStatusIds,
        id
      )
    );
  }

  get(id: number): Observable<PurchaseCartDTO> {
    return this.http.get<PurchaseCartDTO>(getPriceOptimization(id));
  }

  deletePurchaseCarts(model: PurchaseCartDTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(deletePriceOptimizations(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(deletePriceOptimization(id));
  }

  changeStatus(model: ChangeCartStateModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      changePriceOptimizationStatus(),
      model
    );
  }

  save(model: PurchaseCartDTO): Observable<any> {
    return this.http.post<PurchaseCartDTO>(savePriceOptimization(), model);
  }

  // #region ShoppingCartRow

  getPurchaseCartRow(shoppingCartId: number): Observable<PurchaseCartRowDTO[]> {
    return this.http.get<PurchaseCartRowDTO[]>(
      getPriceOptimizationRow(shoppingCartId)
    );
  }

  getPurchaseCartRowPrices(
    sysProductIds: number[]
  ): Observable<ISysWholsesellerPriceSearchDTO[]> {
    return this.http.post<ISysWholsesellerPriceSearchDTO[]>(
      getPriceOptimizationRowPrices(),
      sysProductIds
    );
  }

  transferPurchaseCartRowsToOrder(
    model: ITransferInvoiceDTO
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      transferPriceOptimizationRowsToOrder(),
      model
    );
  }

  //endregion

  //region SysWholesaler
  //PS: Move to SysWholesaler service later?

  getWholesalers(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSmallGenericSysWholesellers(addEmptyRow)
    );
  }

  //endregion
}
