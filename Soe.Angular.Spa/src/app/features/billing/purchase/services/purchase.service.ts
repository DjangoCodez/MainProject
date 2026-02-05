import { Injectable } from '@angular/core';
import {
  ICustomerInvoiceRowPurchaseDTO,
  IPurchaseGridDTO,
  IPurchaseRowFromStockDTO,
  IPurchaseSmallDTO,
} from '@shared/models/generated-interfaces/PurchaseDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  createPurchaseFromStockSuggestion,
  deletePurchase,
  getDeliveryAddresses,
  getOpenPurchasesForSelect,
  getPurchase,
  getPurchaseList,
  getPurchaseRows,
  getPurchaseStatus,
  savePurchase,
  savePurchaseStatus,
  sendPurchaseAsEmail,
  sendPurchasesAsEmail,
} from '@shared/services/generated-service-endpoints/billing/PurchaseOrders.endpoints';
import { BehaviorSubject, map, Observable } from 'rxjs';
import {
  PurchaseDTO,
  PurchaseFilterDTO,
  PurchaseStatusTextDTO,
  SavePurchaseModel,
  SavePurchaseStatus,
  SendPurchaseEmail,
} from '../models/purchase.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { PurchaseDeliveryStatus } from '@shared/models/generated-interfaces/Enumerations';
import { getCustomerInvoiceRows } from '@shared/services/generated-service-endpoints/billing/PurchaseCustomerInvoiceRows.endpoints';
import {
  getCustomerEmailAddresses,
  getCustomerReferences,
} from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { getStocks } from '@shared/services/generated-service-endpoints/billing/StockV2.endpoints';
import { StockDTO } from '@features/billing/stock-warehouse/models/stock-warehouse.model';
import { PurchaseRowDTO } from '../models/purchase-rows.model';

@Injectable({
  providedIn: 'root',
})
export class PurchaseService {
  filter = new PurchaseFilterDTO();
  purchaseStatusText = new PurchaseStatusTextDTO();
  constructor(private http: SoeHttpClient) {}

  private gridFilterSubject = new BehaviorSubject<PurchaseFilterDTO>(
    this.filter
  );

  private purchaseStatusTextSubject =
    new BehaviorSubject<PurchaseStatusTextDTO>(this.purchaseStatusText);

  readonly gridFilter$ = this.gridFilterSubject.asObservable();
  readonly purchaseStatusText$ = this.purchaseStatusTextSubject.asObservable();

  setFilterSubject(filter: PurchaseFilterDTO) {
    this.filter = filter;
    this.gridFilterSubject.next(filter);
  }

  setPurchaseStatusTextSubject(purchaseStatusText: PurchaseStatusTextDTO) {
    this.purchaseStatusText = purchaseStatusText;
    this.purchaseStatusTextSubject.next(purchaseStatusText);
  }

  getGridAdditionalProps: {
    allItemsSelection?: number;
    selectedPurchaseStatusIds?: number[];
  } = {
    allItemsSelection: 0,
    selectedPurchaseStatusIds: [],
  };
  getGrid(
    id?: number,
    additionalProps?: {
      allItemsSelection?: number;
      selectedPurchaseStatusIds?: number[];
    }
  ): Observable<IPurchaseGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http
      .get<
        IPurchaseGridDTO[]
      >(getPurchaseList(this.getGridAdditionalProps.allItemsSelection ?? 0, this.getGridAdditionalProps.selectedPurchaseStatusIds ?? [], id))
      .pipe(
        map(records => {
          records.forEach(e => {
            switch (e.deliveryStatus as PurchaseDeliveryStatus) {
              case PurchaseDeliveryStatus.Unknown:
                (<any>e).deliveryStatusColor = '#CCCCCC';
                break;
              case PurchaseDeliveryStatus.Late:
                (<any>e).deliveryStatusColor = '#FF0000';
                break;
              case PurchaseDeliveryStatus.PartlyDelivered:
                (<any>e).deliveryStatusColor = '#FFFF00';
                break;
              case PurchaseDeliveryStatus.Delivered:
                (<any>e).deliveryStatusColor = '#00FF00';
                break;
              case PurchaseDeliveryStatus.Accepted:
                (<any>e).deliveryStatusColor = '#0000FF';
                break;
            }
          });
          return records;
        })
      );
  }

  getPurchaseStatus(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getPurchaseStatus());
  }

  get(id: number): Observable<PurchaseDTO> {
    return this.http.get<PurchaseDTO>(getPurchase(id));
  }

  getOpenPurchasesForSelect(
    forDelivery: boolean
  ): Observable<IPurchaseSmallDTO[]> {
    return this.http.get<IPurchaseSmallDTO[]>(
      getOpenPurchasesForSelect(forDelivery)
    );
  }

  createPurchaseProposal(model: IPurchaseRowFromStockDTO[]): Observable<any> {
    return this.http.post<IPurchaseRowFromStockDTO[]>(
      createPurchaseFromStockSuggestion(),
      model
    );
  }

  getPurchaseOrderRows(id: number): Observable<PurchaseRowDTO[]> {
    return this.http.get<PurchaseRowDTO[]>(getPurchaseRows(id));
  }

  // getProductRows(
  //   productIds: ProductsSimpleModel
  // ): Observable<IProductRowsProductDTO[]> {
  //   return this.http.post<IProductRowsProductDTO[]>(
  //     getProductRowsProducts(),
  //     productIds
  //   );
  // }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePurchase(id));
  }
  save(model: PurchaseDTO): Observable<any> {
    return this.http.post<PurchaseDTO>(savePurchase(), model);
  }

  savePurchase(model: SavePurchaseModel): Observable<any> {
    return this.http.post<SavePurchaseModel>(savePurchase(), model);
  }

  getSupplierReferences(
    customerId: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getCustomerReferences(customerId, addEmptyRow)
    );
  }

  getCustomerEmailAddresses(
    customerId: number,
    loadContactPersonsEmails: boolean,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getCustomerEmailAddresses(
        customerId,
        loadContactPersonsEmails,
        addEmptyRow
      )
    );
  }

  getStocks(addEmptyRow: boolean): Observable<StockDTO[]> {
    return this.http.get<StockDTO[]>(getStocks(addEmptyRow));
  }

  getDeliveryAddresses(customerId: number): Observable<string[]> {
    return this.http.get<string[]>(getDeliveryAddresses(customerId));
  }

  sendPurchasesAsEmail(model: SendPurchaseEmail): Observable<any> {
    return this.http.post<any>(sendPurchasesAsEmail(), model);
  }

  savePurchaseStatus(model: SavePurchaseStatus): Observable<any> {
    return this.http.post<any>(savePurchaseStatus(), model);
  }

  sendPurchaseAsEmail(model: SendPurchaseEmail): Observable<any> {
    return this.http.post<any>(sendPurchaseAsEmail(), model);
  }

  //#region CustomerInvoiceRows
  getCustomerInvoiceRows(
    viewType: number,
    id: number
  ): Observable<ICustomerInvoiceRowPurchaseDTO[]> {
    return this.http.get<ICustomerInvoiceRowPurchaseDTO[]>(
      getCustomerInvoiceRows(viewType, id)
    );
  }
  //#endregion
}
