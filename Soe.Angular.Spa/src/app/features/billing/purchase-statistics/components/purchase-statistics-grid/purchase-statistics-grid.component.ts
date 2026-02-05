import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { PurchaseStatisticsService } from '../../services/purchase-statistics.service';
import { Perform } from '@shared/util/perform.class';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Observable, take } from 'rxjs';
import {
  PurchaseStatisticsFilterDTO,
  PurchaseStatisticsDTO,
} from '../../models/purchase-statistics.model';
import { IPurchaseStatisticsDTO } from '@shared/models/generated-interfaces/PurchaseStatisticsDTO';

@Component({
  selector: 'soe-purchase-statistics-grid',
  templateUrl: './purchase-statistics-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseStatisticsGridComponent
  extends GridBaseDirective<IPurchaseStatisticsDTO, PurchaseStatisticsService>
  implements OnInit
{
  progressService = inject(ProgressService);
  service = inject(PurchaseStatisticsService);
  performAction = new Perform<PurchaseStatisticsDTO[]>(this.progressService);
  model = new PurchaseStatisticsFilterDTO();

  //Footer summery
  selectedQuantity = 0;
  selectedCurrencyAmount = 0;
  selectedAmount = 0;
  filteredQuantity = 0;
  filteredCurrencyAmount = 0;
  filteredAmount = 0;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Statistics_Purchase,
      'Billing_Purchase_Statistics',
      { skipInitialLoad: true }
    );
  }

  onGridReadyToDefine(grid: GridComponent<IPurchaseStatisticsDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.suppliername',
        'billing.purchase.supplierno',
        'billing.purchase.supplier',
        'common.customer.invoices.articlename',
        'billing.purchaserows.productnr',
        'billing.purchaserows.quantity',
        'billing.purchaserows.purchaseunit',
        'billing.purchase.purchasenr',
        'common.purchaseprice',
        'billing.purchase.purchasedate',
        'billing.purchaserows.wanteddeliverydate',
        'billing.purchaserows.accdeliverydate',
        'billing.purchaserows.deliveredquantity',
        'billing.purchaserows.deliverydate',
        'billing.purchaserows.sumamount',
        'billing.purchaserows.discount',
        'billing.purchaserow.totalexvat',
        'billing.purchase.foreignamount',
        'common.order',
        'common.status',
        'common.customer.invoices.currencycode',
        'common.report.selection.projectnr',
        'common.report.selection.stockplace',
        'common.customer.invoices.rowstatus',
        'common.code',
        'common.customer.invoices.foreignamount',
        'billing.purchase.product.supplieritemno',
        'billing.purchase.product.supplieritemname',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'supplierNumberName',
          terms['billing.purchase.supplier'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'supplierItemNumber',
          terms['billing.purchase.product.supplieritemno'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'supplierItemName',
          terms['billing.purchase.product.supplieritemname'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'productNumber',
          terms['billing.purchaserows.productnr'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'productName',
          terms['common.customer.invoices.articlename'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.purchaserows.quantity'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'unit',
          terms['billing.purchaserows.purchaseunit'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'purchasePrice',
          terms['common.purchaseprice'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'purchaseDate',
          terms['billing.purchase.purchasedate'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'wantedDeliveryDate',
          terms['billing.purchaserows.wanteddeliverydate'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'acknowledgeDeliveryDate',
          terms['billing.purchaserows.accdeliverydate'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('customerOrderNumber', terms['common.order'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnNumber(
          'deliveredQuantity',
          terms['billing.purchaserows.deliveredquantity'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'deliveryDate',
          terms['billing.purchaserows.deliverydate'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'sumAmount',
          terms['billing.purchaserow.totalexvat'],
          {
            flex: 1,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'sumAmountCurrency',
          terms['billing.purchase.foreignamount'],
          {
            flex: 1,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'currencyCode',
          terms['common.customer.invoices.currencycode'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'projectNumber',
          terms['common.report.selection.projectnr'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('statusName', terms['common.status'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'rowStatusName',
          terms['common.customer.invoices.rowstatus'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'stockPlace',
          terms['common.report.selection.stockplace'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });

        this.grid.setExportExcelOptions({
          groupedTotals: true,
          rowGroupExpandState: 'expanded',
          termGroupedSubTotal: 'Delsumma',
          termGroupedGrandTotal: 'Total',
        });

        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          stickyGrandTotalRow: 'bottom',
        });

        this.grid.addGroupTimeSpanSumAggFunction(true);
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined
  ): Observable<IPurchaseStatisticsDTO[]> {
    return this.performAction.load$(
      this.service.getGrid(undefined, { model: this.model })
    );
  }

  override onAfterLoadData(): void {
    this.summarizeFiltered();
  }

  doSearch(event: PurchaseStatisticsFilterDTO) {
    this.model = event;
    this.refreshGrid();
  }

  summarizeFiltered() {
    const rows = this.grid.getFilteredRows();
    const { quantity, currencyAmount, amount } = this.calculateSummery(rows);
    this.filteredQuantity = quantity;
    this.filteredCurrencyAmount = currencyAmount;
    this.filteredAmount = amount;
  }

  triggerSelectedItemCalc() {
    const { quantity, currencyAmount, amount } = this.calculateSummery(
      this.grid.getSelectedRows()
    );
    this.selectedQuantity = quantity;
    this.selectedCurrencyAmount = currencyAmount;
    this.selectedAmount = amount;
  }

  calculateSummery(obj: IPurchaseStatisticsDTO[]) {
    let quantity = 0,
      currencyAmount = 0,
      amount = 0;
    obj.forEach(element => {
      if (element) {
        quantity += element.quantity;
        currencyAmount += element.sumAmountCurrency;
        amount += element.sumAmount;
      }
    });
    return { quantity, currencyAmount, amount };
  }
}
