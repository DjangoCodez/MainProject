import { Component, Input, OnInit } from '@angular/core';
import { PurchaseEditComponent } from '@features/billing/purchase/components/purchase-edit/purchase-edit.component';
import { PurchaseForm } from '@features/billing/purchase/models/purchase-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseDeliveryRowDTO } from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellValueChangedEvent,
  EditableCallbackParams,
} from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { PurchaseDeliveryRowDTO } from 'src/app/features/billing/purchase-delivery/models/purchase-delivery.model';

@Component({
  selector: 'soe-delivery-rows',
  templateUrl: './delivery-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DeliveryRowsComponent
  extends GridBaseDirective<IPurchaseDeliveryRowDTO>
  implements OnInit
{
  @Input() rows!: BehaviorSubject<PurchaseDeliveryRowDTO[]>;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Purchase_Edit,
      'billing.purchase.rows',
      {
        additionalModifyPermissions: [
          Feature.Billing_Stock,
          Feature.Billing_Product_Products_ShowSalesPrice,
        ],
        skipInitialLoad: true,
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IPurchaseDeliveryRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    // this.grid.options.onFirstDataRendered = this.onLoad.bind(this); //TODO: initial load set isModified = true

    this.translate
      .get([
        'common.rownr',
        'billing.order.ordernr',
        'billing.purchase.purchasenr',
        'billing.purchase.delivery.finaldelivery',
        'billing.purchase.delivery.remainingqty',
        'billing.purchase.delivery.purchaseqty',
        'billing.productrows.stockcode',
        'billing.productrows.addtextrow',
        'billing.productrows.stockcode',
        'billing.purchaserows.productnr',
        'billing.purchaserows.purchaseprice',
        'billing.purchaserows.purchasepricecurrency',
        'billing.purchaserows.quantity',
        'billing.purchaserows.deliverydate',
        'billing.purchaserows.productnr',
        'billing.purchaserows.text',
        'billing.purchaserows.deliveredquantity',
        'billing.purchaserows.purchaseprice',
        'billing.purchaserows.purchaseunit',
        'billing.purchaserows.text',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');

        this.grid.addColumnText(
          'productNr',
          terms['billing.purchaserows.productnr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'productName',
          terms['billing.purchaserows.text'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'deliveredQuantity',
          terms['billing.purchaserows.deliveredquantity'],
          {
            flex: 1,
            editable: row => this.isEditable(row),
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'purchaseQuantity',
          terms['billing.purchase.delivery.purchaseqty'],
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'remainingQuantity',
          terms['billing.purchase.delivery.remainingqty'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnBool(
          'isLocked',
          terms['billing.purchase.delivery.finaldelivery'],
          {
            flex: 1,
            alignCenter: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'stockCode',
          terms['billing.productrows.stockcode'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'purchasePriceCurrency',
          terms['billing.purchaserows.purchasepricecurrency'],
          {
            flex: 1,
            decimals: 2,
            editable: row => this.isEditable(row),
            enableHiding: false,
          }
        );
        this.grid.addColumnDate(
          'deliveryDate',
          terms['billing.purchaserows.deliverydate'],
          {
            flex: 1,
            editable: row => this.isEditable(row),
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
            enableHiding: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pen',
              show: () => true,
              tooltip: this.terms['billing.productrows.edit'],
              onClick: (row: IPurchaseDeliveryRowDTO) =>
                this.openPurchaseInNewEdit(row),
            },
          }
        );

        this.exportFilenameKey.set('billing.purchase.rows');
        super.finalizeInitGrid();
      });
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    switch (event.colDef.field) {
      case 'deliveredQuantity':
        this.changeDeliveredQuantity(event);
        break;
      case 'purchasePriceCurrency':
        this.setRowAsModified(event);
        break;
    }
  }

  private changeDeliveredQuantity(row: CellValueChangedEvent) {
    const diff = row.oldValue - row.newValue;
    row.data.remainingQuantity += diff;
    this.setRowAsModified(row);
  }

  isEditable(row: EditableCallbackParams): boolean {
    return row.data.isLocked != undefined ? !row.data.isLocked : false;
  }

  setRowAsModified(row: CellValueChangedEvent) {
    if (row.data) {
      row.data.isModified = true;
      this.grid.refreshCells();
    }
  }

  openPurchaseInNewEdit(row: IPurchaseDeliveryRowDTO) {
    this.openEditInNewTab.emit({
      id: row.purchaseId || 0,
      additionalProps: {
        editComponent: PurchaseEditComponent,
        FormClass: PurchaseForm,
        editTabLabel: 'billing.purchase.list.purchase',
      },
    });
  }
}
