import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ICustomerStatisticsDTO } from '@shared/models/generated-interfaces/CustomerStatisticsDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TranslateService } from '@ngx-translate/core';
import {
  Feature,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, take, tap } from 'rxjs';
import { SalesStatisticsService } from '../../services/sales-statistics.service';
import { Perform } from '@shared/util/perform.class';
import { GeneralProductStatisticsDTO } from '../../models/sales-statistics.model';
import { EconomyService } from '@src/app/features/economy/services/economy.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-sales-statistics-grid',
  templateUrl: './sales-statistics-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SalesStatisticsGridComponent
  extends GridBaseDirective<ICustomerStatisticsDTO, SalesStatisticsService>
  implements OnInit
{
  service = inject(SalesStatisticsService);
  private translationService = inject(TranslateService);
  public flowHandler = inject(FlowHandlerService);
  public progressService = inject(ProgressService);
  public coreService = inject(EconomyService);

  performAction = new Perform<ICustomerStatisticsDTO[]>(this.progressService);
  model = new GeneralProductStatisticsDTO();

  //Footer summeries
  selectedQuantity = 0;
  selectedPrice = 0;
  selectedAmount = 0;
  selectedPurchasePrice = 0;
  selectedMarginalIncome = 0;
  filteredQuantity = 0;
  filteredPrice = 0;
  filteredAmount = 0;
  filteredPurchasePrice = 0;
  filteredMarginalIncome = 0;

  //columns
  private accountDim1ColumnName = '';

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Statistics, 'statisticsGrid', {
      lookups: [this.loadAccountDims()],
      skipInitialLoad: true,
    });
  }

  private loadAccountDims() {
    return this.coreService
      .getAccountDimsSmall(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap(accountDims => {
          if (accountDims.length > 1)
            this.accountDim1ColumnName = accountDims[1].name;
        })
      );
  }

  onGridReadyToDefine(grid: GridComponent<ICustomerStatisticsDTO>) {
    super.onGridReadyToDefine(grid);

    this.translationService
      .get([
        'common.number',
        'common.type',
        'common.invoicenr',
        'common.productnr',
        'common.name',
        'common.quantity',
        'common.customer.invoices.amountexvat',
        'common.purchaseprice',
        'common.price',
        'common.date',
        'common.customer.customer.marginalincome',
        'common.customer.customer.marginalincomeratio',
        'common.customer',
        'common.customer.customer.customercategory',
        'common.customer.customer.productcategory',
        'common.customer.customer.ordercategory',
        'common.customer.customer.contractcategory',
        'common.customer.customer.customernr',
        'common.contactaddresses.addressrow.postaladdress',
        'common.contactaddresses.addressrow.postalcode',
        'common.customer.customer.filtered',
        'common.customer.customer.selected',
        'common.customer.customer.ordercostcentre',
        'common.customer.customer.orderproject',
        'common.customer.customer.wholesellername',
        'common.customer.customer.marginalincomeratioprocent',
        'common.customerinvoice',
        'common.startdate',
        'common.customer.invoices.invoicedate',
        'billing.offer.offerdate',
        'billing.order.invoicedate',
        'common.order',
        'common.offer',
        'common.contract',
        'billing.order.ordertype',
        'billing.order.owners',
        'billing.order.selectusers.responsible',
        'billing.order.ourreference',
        'common.customer.invoices.articlename',
        'common.customer.invoices.currencycode',
        'common.customer.invoices.foreignamount',
        'common.country',
        'common.customer.customer.payingcustomer',
        'common.customer.invoices.ordernr',
        'common.customer.invoices.rowstatus',
        'billing.product.materialcode',
        'billing.product.productgroup',
        'billing.product.productcategories',
        'billing.product.headproductcategories',
        'common.purchasepricecurrency',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText('customerName', terms['common.customer'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'customerPostalAddress',
          terms['common.contactaddresses.addressrow.postaladdress'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'customerPostalCode',
          terms['common.contactaddresses.addressrow.postalcode'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('customerCountry', terms['common.country'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'orderNr',
          terms['common.customer.invoices.ordernr'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'payingCustomerName',
          terms['common.customer.customer.payingcustomer'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText('invoiceNr', terms['common.number'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'productName',
          terms['common.customer.invoices.articlename'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'attestStateName',
          terms['common.customer.invoices.rowstatus'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            tooltipField: 'attestStateName',
            shapeConfiguration: {
              shape: 'circle',
              colorField: 'attestStateColor',
              showShapeField: 'attestStateName',
            },
          }
        );
        this.grid.addColumnNumber('productQuantity', terms['common.quantity'], {
          flex: 1,
          enableGrouping: true,
          aggFuncOnGrouping: 'sum',
          enableHiding: true,
        });
        this.grid.addColumnText(
          'orderTypeName',
          terms['billing.order.ordertype'],
          {
            flex: 1,
            hide: true,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'customerCategory',
          terms['common.customer.customer.customercategory'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'productCategory',
          terms['billing.product.productcategories'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'contractCategory',
          terms['common.customer.customer.contractcategory'],
          {
            enableHiding: true,
            hide: true,
            enableGrouping: true,
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'orderCategory',
          terms['common.customer.customer.ordercategory'],
          {
            enableHiding: true,
            flex: 1,
            hide: true,
            enableGrouping: true,
          }
        );
        if (this.accountDim1ColumnName) {
          this.grid.addColumnText('costCentre', this.accountDim1ColumnName, {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          });
        }
        this.grid.addColumnText(
          'projectNr',
          terms['common.customer.customer.orderproject'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'wholeSellerName',
          terms['common.customer.customer.wholesellername'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'referenceOur',
          terms['billing.order.ourreference'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('originUsers', terms['billing.order.owners'], {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'mainUserName',
          terms['billing.order.selectusers.responsible'],
          {
            flex: 1,
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber('productPrice', terms['common.price'], {
          showSetFilter: true,
          aggFuncOnGrouping: 'sum',
          flex: 1,
          decimals: 2,
          enableGrouping: true,
          enableHiding: true,
        });
        this.grid.addColumnNumber(
          'productSumAmount',
          terms['common.customer.invoices.amountexvat'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            showSetFilter: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'productSumAmountCurrency',
          terms['common.customer.invoices.foreignamount'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            showSetFilter: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'currencyCode',
          terms['common.customer.invoices.currencycode'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'productPurchasePrice',
          terms['common.purchaseprice'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            showSetFilter: true,
            enableHiding: true,
          }
        );

        this.grid.addColumnNumber(
          'productPurchasePriceCurrency',
          terms['common.purchasepricecurrency'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            showSetFilter: true,
            enableHiding: true,
            hide: true,
          }
        );
        this.grid.addColumnNumber(
          'productMarginalIncome',
          terms['common.customer.customer.marginalincome'],
          {
            flex: 1,
            decimals: 2,
            enableGrouping: true,
            enableHiding: true,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber(
          'productMarginalRatio',
          terms['common.customer.customer.marginalincomeratioprocent'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'timeCodeName',
          terms['billing.product.materialcode'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'productGroupName',
          terms['billing.product.productgroup'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'parentProductCategories',
          terms['billing.product.headproductcategories'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            enableGrouping: true,
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
          selectChildren: true,
          groupSelectsFiltered: true,
        });

        this.grid.addGroupTimeSpanSumAggFunction(true);
        this.grid.suppressSizeToFitForAllColumns();

        super.finalizeInitGrid();
      });
  }

  filterChange(originType: SoeOriginType) {
    if (originType == SoeOriginType.Order) {
      this.grid.showColumns([
        'attestStateName',
        'orderTypeName',
        'orderCategory',
      ]);
      this.grid.hideColumns(['contractCategory']);
    } else if (originType == SoeOriginType.Contract) {
      this.grid.showColumns(['contractCategory']);
      this.grid.hideColumns([
        'attestStateName',
        'orderTypeName',
        'orderCategory',
      ]);
    } else if (originType == SoeOriginType.Offer) {
      this.grid.showColumns(['attestStateName']);
      this.grid.hideColumns([
        'orderTypeName',
        'orderCategory',
        'contractCategory',
      ]);
    } else if (originType == SoeOriginType.CustomerInvoice) {
      this.grid.hideColumns([
        'attestStateName',
        'contractCategory',
        'orderCategory',
        'orderTypeName',
      ]);
    }
  }

  summarizeFiltered() {
    const rows = this.grid.getFilteredRows();
    const { quantity, price, amount, purchasePrice, income } =
      this.calculateSummery(rows);
    this.filteredQuantity = quantity;
    this.filteredPrice = price;
    this.filteredAmount = amount;
    this.filteredPurchasePrice = purchasePrice;
    this.filteredMarginalIncome = income;
  }

  triggerSelectedItemCalc() {
    const { quantity, price, amount, purchasePrice, income } =
      this.calculateSummery(this.grid.getSelectedRows());
    this.selectedQuantity = quantity;
    this.selectedPrice = price;
    this.selectedAmount = amount;
    this.selectedPurchasePrice = purchasePrice;
    this.selectedMarginalIncome = income;
  }

  calculateSummery(obj: ICustomerStatisticsDTO[]) {
    let quantity = 0,
      price = 0,
      amount = 0,
      purchasePrice = 0,
      income = 0;
    obj.forEach(element => {
      if (element) {
        quantity += element.productQuantity;
        price += element.productPrice;
        amount += element.productSumAmount;
        purchasePrice += element.productPurchasePrice;
        income += element.productMarginalIncome;
      }
    });
    return { quantity, price, amount, purchasePrice, income };
  }

  doSearch(event: GeneralProductStatisticsDTO) {
    this.model = event;
    this.filterChange(event.originType);
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<ICustomerStatisticsDTO[]> {
    return this.performAction.load$(
      this.service.getGrid(undefined, { model: this.model })
    );
  }

  override onAfterLoadData() {
    this.summarizeFiltered();
  }
}
