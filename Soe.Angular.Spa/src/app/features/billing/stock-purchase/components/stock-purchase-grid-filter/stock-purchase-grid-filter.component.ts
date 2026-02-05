import { Component, EventEmitter, Output } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { StockPurchaseFilterForm } from '../../models/stock-purchase-filter-form.model';
import { ValidationHandler } from '@shared/handlers';
import { StockPurchaseFilterDTO } from '../../models/stock-purchase.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { TranslateService } from '@ngx-translate/core';
import {
  EditDeliveryAddressComponent,
  IEditDeliveryAddressDialogData,
} from '@shared/components/billing/edit-delivery-address/edit-delivery-address.component';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { StockWarehouseService } from '../../../stock-warehouse/services/stock-warehouse.service';

@Component({
  selector: 'soe-stock-purchase-grid-filter',
  templateUrl: './stock-purchase-grid-filter.component.html',
  standalone: false,
})
export class StockPurchaseGridFilterComponent {
  @Output() onSearchClick = new EventEmitter<StockPurchaseFilterDTO>();

  performLoadWarehouse = new Perform<SmallGenericType[]>(this.progressService);
  performLoadSuggestionBase = new Perform<SmallGenericType[]>(
    this.progressService
  );
  formSearch: StockPurchaseFilterForm = new StockPurchaseFilterForm({
    validationHandler: this.validationHandler,
    element: new StockPurchaseFilterDTO(),
  });

  constructor(
    private translate: TranslateService,
    private progressService: ProgressService,
    private stockWarehouseService: StockWarehouseService,
    public handler: FlowHandlerService,
    public validationHandler: ValidationHandler,
    public dialogServiceV2: DialogService,
    public coreService: CoreService
  ) {
    this.handler.execute({
      lookups: [this.loadWareHouses(), this.loadBaseSuggestion()],
      onFinished: this.finished.bind(this),
    });
  }

  finished() {
    this.formSearch?.patchValue({ triggerQuantityPercent: 0 });
  }

  loadBaseSuggestion() {
    return this.performLoadSuggestionBase.load$(
      this.coreService.getTermGroupContent(
        TermGroup.StockPurchaseGenerationOptions,
        false,
        true,
        true
      )
    );
  }

  loadWareHouses() {
    return this.performLoadWarehouse.load$(
      this.stockWarehouseService.getStockWarehousesDict(false, false)
    );
  }

  generateSuggestion() {
    const searchDto = this.formSearch.value as StockPurchaseFilterDTO;
    this.onSearchClick.emit({
      defaultDeliveryAddress: this.formSearch.value.defaultDeliveryAddress,
      excludeMissingPurchaseQuantity: searchDto.excludeMissingPurchaseQuantity,
      excludeMissingTriggerQuantity: searchDto.excludeMissingTriggerQuantity,
      excludePurchaseQuantityZero: searchDto.excludePurchaseQuantityZero,
      productNrFrom: searchDto.productNrFrom,
      productNrTo: searchDto.productNrTo,
      purchaseGenerationType: searchDto.purchaseGenerationType,
      purchaser: searchDto.purchaser,
      stockPlaceIds: searchDto.stockPlaceIds,
      triggerQuantityPercent: searchDto.triggerQuantityPercent,
    });
  }

  openEditAddress() {
    this.dialogServiceV2
      .open(EditDeliveryAddressComponent, {
        title: 'billing.order.deliveryaddress',
        addressString: this.formSearch.value.defaultDeliveryAddress,
        size: 'sm',
      } as IEditDeliveryAddressDialogData)
      .afterClosed()
      .subscribe(value => {
        this.formSearch.patchValue({ defaultDeliveryAddress: value });
      });
  }
}
