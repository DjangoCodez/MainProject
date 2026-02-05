import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISupplierProductGridDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { SupplierProductGridHeaderDTO } from '../../models/purchase-product.model';
import { PurchaseProductsService } from '../../services/purchase-products.service';
import { ImportProductsDialogComponent } from '../import-products-dialog/import-products-dialog.component';
import {
  PriceUpdateComponent,
  PriceUpdateDialogData,
} from './price-update-modal/price-update-modal.component';

@Component({
  selector: 'soe-purchase-products-grid',
  templateUrl: './purchase-products-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseProductsGridComponent
  extends GridBaseDirective<ISupplierProductGridDTO, PurchaseProductsService>
  implements OnInit
{
  hasPriceUpdatePermission = signal(false);
  disablePriceUpdate = signal(true);
  hidePriceUpdate = computed(() => !this.hasPriceUpdatePermission());

  flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  service = inject(PurchaseProductsService);
  coreService = inject(CoreService);
  dialogService = inject(DialogService);
  performGridLoad = new Perform<ISupplierProductGridDTO[]>(
    this.progressService
  );
  _searchDto = new SupplierProductGridHeaderDTO();

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Products,
      'Billing.Purchase.Products',
      {
        additionalModifyPermissions: [
          Feature.Billing_Purchase_Products_PriceUpdate,
        ],
        skipInitialLoad: true,
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({});

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('import', {
          iconName: signal('file-import'),
          caption: signal('billing.purchase.product.importpricelist'),
          tooltip: signal('billing.purchase.product.importpricelist'),
          onAction: () => this.importSupplierProducts(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('priceadjustment2', {
          iconName: signal('chart-mixed-up-circle-dollar'),
          tooltip: signal('billing.purchase.product.priceadjustment'),
          disabled: this.disablePriceUpdate,
          hidden: this.hidePriceUpdate,
          onAction: () => this.openPriceEditModal(),
        }),
      ],
    });
  }

  override onFinished(): void {
    this.setPriceUpdatePermission();
  }

  setPriceUpdatePermission() {
    this.hasPriceUpdatePermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Purchase_Products_PriceUpdate
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ISupplierProductGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.product.supplieritemno',
        'billing.purchase.product.supplieritemname',
        'billing.product.number',
        'billing.product.name',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'supplierProductNr',
          terms['billing.purchase.product.supplieritemno'],
          {
            flex: 1,
            enableHiding: false,
            filterOptions: ['startsWith', 'contains', 'endsWith'],
          }
        );
        this.grid.addColumnText(
          'supplierProductName',
          terms['billing.purchase.product.supplieritemname'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText('productNr', terms['billing.product.number'], {
          flex: 1,
          enableHiding: false,
          filterOptions: ['startsWith', 'contains', 'endsWith'],
        });
        this.grid.addColumnText('productName', terms['billing.product.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.exportFilenameKey.set('billing.purchase.products.list');
        super.finalizeInitGrid();
      });
  }

  doSearch(searchDto: SupplierProductGridHeaderDTO) {
    this._searchDto = searchDto;
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<ISupplierProductGridDTO[]> {
    return this.performGridLoad.load$(
      this.service.getGrid(undefined, { searchDto: this._searchDto })
    );
  }

  openPriceEditModal() {
    this.dialogService.open(PriceUpdateComponent, {
      title: 'billing.purchase.product.priceadjustment',
      size: 'lg',
      selectedRows: this.grid.getSelectedRows().map(x => x.supplierProductId),
    } as PriceUpdateDialogData);
  }

  selectionChanged(data: ISupplierProductGridDTO[]) {
    this.disablePriceUpdate.set(data.length === 0);
  }

  private importSupplierProducts(): void {
    this.dialogService.open(ImportProductsDialogComponent, {
      size: 'lg',
      disableClose: true,
      title: 'billing.stock.stocks.importfromfile',
    });
  }
}
