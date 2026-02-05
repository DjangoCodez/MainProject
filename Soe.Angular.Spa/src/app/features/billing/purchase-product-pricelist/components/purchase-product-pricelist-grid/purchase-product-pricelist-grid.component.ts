import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISupplierProductPriceListGridDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { PurchaseProductPricelistService } from '../../services/purchase-product-pricelist.service';

@Component({
  selector: 'soe-purchase-product-pricelist-grid',
  templateUrl: './purchase-product-pricelist-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseProductPricelistGridComponent
  extends GridBaseDirective<
    ISupplierProductPriceListGridDTO,
    PurchaseProductPricelistService
  >
  implements OnInit
{
  service = inject(PurchaseProductPricelistService);
  flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  performGridLoad = new Perform<ISupplierProductPriceListGridDTO[]>(
    this.progressService
  );
  searchSupplierId = 0;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Pricelists,
      'Billing.Purchase.Pricelists',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISupplierProductPriceListGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.product.wholesellertype',
        'billing.purchase.product.wholeseller',
        'billing.purchase.product.pricestartdate',
        'billing.purchase.product.priceenddate',
        'common.currency',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          {
            flex: 2,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          {
            flex: 2,
            enableHiding: false,
          }
        );
        this.grid.addColumnText('currencyCode', terms['common.currency'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnDate(
          'startDate',
          terms['billing.purchase.product.pricestartdate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'endDate',
          terms['billing.purchase.product.priceenddate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.doSearch(),
      },
    });
  }

  doSearch(supplierId?: number) {
    this.searchSupplierId = supplierId ?? this.searchSupplierId;
    this.loadData(this.searchSupplierId).subscribe(x => {
      this.grid.setData(x);
    });
  }
}
