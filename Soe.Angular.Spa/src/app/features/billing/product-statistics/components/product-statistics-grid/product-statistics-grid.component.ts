import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { ProductService } from '../../../products/services/product.service';
import {
  ProductStatisticsDTO,
  ProductStatisticsRequest,
} from '../../models/product-statistics.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { InitialGroupOrderComparatorParams } from 'ag-grid-community';

@Component({
  selector: 'soe-product-statistics-grid',
  templateUrl: './product-statistics-grid.component.html',
  styleUrls: ['./product-statistics-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductStatisticsGridComponent
  extends GridBaseDirective<ProductStatisticsDTO>
  implements OnInit
{
  coreService = inject(CoreService);
  productService = inject(ProductService);
  performType = new Perform<SmallGenericType[]>(this.progressService);
  performGridLoad = new Perform<ProductStatisticsDTO[]>(this.progressService);
  model?: ProductStatisticsRequest;
  vatTypes?: SmallGenericType[];

  constructor(public flowHandler: FlowHandlerService) {
    super();
  }

  ngOnInit(): void {
    this.startFlow(
      Feature.Billing_Statistics_Product,
      'Billing.Product.Statistics',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [this.loadVatTypes()],
      }
    );
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.loadSearch(),
      },
    });
  }

  loadVatTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceProductVatType, false, false)
      .pipe(
        tap(res => {
          this.vatTypes = res;
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ProductStatisticsDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'billing.product.number',
        'billing.product.name',
        'common.year',
        'common.month',
        'common.customer.invoices.invoicedate',
        'billing.purchaserows.deliverydate',
        'common.customer.invoices.invoicenr',
        'common.customer.invoices.ordernr',
        'billing.purchase.purchasenr',
        'common.customer.customer.customernr',
        'common.customer.customer.customername',
        'economy.supplier.supplier.suppliernr.grid',
        'economy.supplier.supplier.suppliername.grid',
        'billing.purchaserows.deliveredquantity',
        'common.customer.invoices.invoiceqty',
        'common.customer.invoices.invoiceamount',
        'billing.productrows.marginalincome.short',
        'billing.productrows.marginalincomeratio.short',
        'billing.product.calculationtype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('originTypeName', terms['common.type'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnText('productNr', terms['billing.product.number'], {
          flex: 1,
          enableGrouping: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'pencil',
            onClick: row => this.editProduct(row),
          },
        });
        this.grid.addColumnText('productName', terms['billing.product.name'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnText('year', terms['common.year'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText('month', terms['common.month'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.invoicedate'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnDate(
          'purchaseDeliveryDate',
          terms['billing.purchaserows.deliverydate'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'invoiceNr',
          terms['common.customer.invoices.invoicenr'],
          {
            flex: 1,
            enableGrouping: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.editInvoice(row),
              show: row => row && row.invoiceId > 0,
            },
          }
        );
        this.grid.addColumnText(
          'orderNr',
          terms['common.customer.invoices.ordernr'],
          {
            flex: 1,
            enableGrouping: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.editOrder(row),
              show: row => row && row.orderId > 0,
            },
          }
        );
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
            enableGrouping: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.editPurchase(row),
              show: row => row && row.purchaseId > 0,
            },
          }
        );
        this.grid.addColumnText(
          'customerNr',
          terms['common.customer.customer.customernr'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'customerName',
          terms['common.customer.customer.customername'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'supplierNr',
          terms['economy.supplier.supplier.suppliernr.grid'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['economy.supplier.supplier.suppliername.grid'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'purchaseQty',
          terms['billing.purchaserows.deliveredquantity'],
          {
            showSetFilter: true,
            aggFuncOnGrouping: 'sum',
            flex: 1,
            enableGrouping: true,
            allowEmpty: true,
          }
        );
        this.grid.addColumnNumber(
          'customerInvoiceQty',
          terms['common.customer.invoices.invoiceqty'],
          {
            showSetFilter: true,
            aggFuncOnGrouping: 'sum',
            flex: 1,
            enableGrouping: true,
            allowEmpty: true,
          }
        );
        this.grid.addColumnNumber(
          'customerInvoiceAmount',
          terms['common.customer.invoices.invoiceamount'],
          {
            showSetFilter: true,
            aggFuncOnGrouping: 'sum',
            flex: 1,
            enableGrouping: true,
            allowEmpty: true,
          }
        );
        this.grid.addColumnNumber(
          'marginalIncome',
          terms['billing.productrows.marginalincome.short'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            enableHiding: true,
            allowEmpty: true,
          }
        );
        this.grid.addColumnNumber(
          'marginalRatio',
          terms['billing.productrows.marginalincomeratio.short'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
            allowEmpty: true,
          }
        );
        this.grid.addColumnText(
          'vatTypeName',
          terms['billing.product.calculationtype'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
            hide: true,
          }
        );

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

        this.grid.groupRowsByColumn('productNr', 'productNr');

        this.grid.agGrid.initialGroupOrderComparator = (
          params: InitialGroupOrderComparatorParams
        ) => {
          const a = params.nodeA.key || '';
          const b = params.nodeB.key || '';
          return a < b ? -1 : a > b ? 1 : 0;
        };

        super.finalizeInitGrid();
        this.grid.setData([]);
      });
  }

  editProduct(row: ProductStatisticsDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/product/products/default.aspx?&productId=${row.productId}&productNr=${row.productNr}`
    );
  }

  editInvoice(row: ProductStatisticsDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  editOrder(row: ProductStatisticsDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&orderId=${row.invoiceId}&orderNr=${row.invoiceNr}`
    );
  }

  editPurchase(row: ProductStatisticsDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/purchase/list/default.aspx?&purchaseId=${row.purchaseId}&purchaseNr=${row.purchaseNr}`
    );
  }

  doSearch(event: ProductStatisticsRequest) {
    this.model = event;
    this.loadSearch();
  }

  loadSearch() {
    if (!this.model) {
      return;
    }
    this.performGridLoad.load(
      this.productService
        .getProductStatistics(this.model, this.vatTypes!)
        .pipe(tap(value => this.grid.setData(value)))
    );
  }
}
