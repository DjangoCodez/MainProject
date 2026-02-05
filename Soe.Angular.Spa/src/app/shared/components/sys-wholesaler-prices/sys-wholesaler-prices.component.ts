import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  input,
  model,
  output,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  PriceListOrigin,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { GridSizeChangedEvent, RowDoubleClickedEvent } from 'ag-grid-community';
import { debounce } from 'lodash';
import { Observable, take, tap } from 'rxjs';
import {
  InvoiceProductPriceSearchViewDTO,
  SearchProductPricesModel,
} from './models/sys-wholesaler-prices.models';
import { SysWholesalerPricesService } from './services/sys-wholesaler-prices.service';

@Component({
  selector: 'soe-sys-wholesaler-prices',
  templateUrl: './sys-wholesaler-prices.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysWholesalerPricesComponent
  extends GridBaseDirective<
    InvoiceProductPriceSearchViewDTO,
    SysWholesalerPricesService
  >
  implements OnInit, OnDestroy
{
  priceListTypeId = input<number>(0);
  customerId = input<number>(0);
  currencyId = input<number>(0);
  sysWholesellerId = input<number>(0);
  number = input<string>('');
  providerType = input<number>(0);

  rowSelected = output<InvoiceProductPriceSearchViewDTO>();
  selectedPrice = model<InvoiceProductPriceSearchViewDTO>();

  service = inject(SysWholesalerPricesService);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<any>(this.progress);
  private readonly coreService = inject(CoreService);

  private sysWholesalerTypes: SmallGenericType[] = [];
  protected isSearching: boolean = false;
  protected compPriceExist: boolean = false;

  numberEff = effect(() => {
    if (!this.isSearching) {
      const _number = this.number();
      this.loadProducts();
    }
  });

  providerTypeEff = effect(() => {
    if (!this.isSearching) {
      const _providerType = this.providerType();
      this.loadProducts();
    }
  });

  priceListTypeIdEff = effect(() => {
    if (!this.isSearching) {
      const _priceListTypeId = this.priceListTypeId();
      this.loadProducts();
    }
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Supplier_Invoice_Invoices_Edit,
      'Billing.Dialogs.SearchInvoiceProduct.Prices',
      {
        additionalReadPermissions: [
          Feature.Billing_Product_Products_ShowPurchasePrice,
          Feature.Billing_Product_Products_ShowSalesPrice,
        ],
        skipInitialLoad: true,
        lookups: [this.loadSysWholesalerTypes()],
      }
    );
  }

  private loadSysWholesalerTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.SysWholesellerType, false, false)
      .pipe(
        tap(types => {
          this.sysWholesalerTypes = types;
        })
      );
  }

  override onGridReadyToDefine(
    grid: GridComponent<InvoiceProductPriceSearchViewDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.setNbrOfRowsToShow(8, 8);
    this.grid.context.suppressGridMenu = true;

    this.grid.api.updateGridOptions({
      onFilterModified: this.filterModified.bind(this),
      onGridSizeChanged: (event: GridSizeChangedEvent): void => {
        if (event.clientHeight > 0 && event.clientWidth > 0)
          this.grid.api.sizeColumnsToFit();
      },
      onRowDoubleClicked: this.rowDoubleClicked.bind(this),
    });

    this.translate
      .get([
        'common.syswholesellerprices.wholeseller',
        'common.syswholesellerprices.gnp',
        'common.syswholesellerprices.nettonetto',
        'common.syswholesellerprices.customerprice',
        'common.syswholesellerprices.marginalincome',
        'common.syswholesellerprices.marginalincomeratio',
        'common.syswholesellerprices.providertype',
        'common.syswholesellerprices.purchaseunit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(() => true, true);
        this.grid.addColumnText(
          'wholeseller',
          terms['common.syswholesellerprices.wholeseller'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'gnp',
          terms['common.syswholesellerprices.gnp'],
          { decimals: 2, flex: 1 }
        );
        this.grid.addColumnNumber(
          'nettoNettoPrice',
          terms['common.syswholesellerprices.nettonetto'],
          { decimals: 2, tooltipField: 'code', flex: 1 }
        );
        this.grid.addColumnNumber(
          'customerPrice',
          terms['common.syswholesellerprices.customerprice'],
          { decimals: 2, tooltipField: 'priceFormula', flex: 1 }
        );
        this.grid.addColumnNumber(
          'marginalIncome',
          terms['common.syswholesellerprices.marginalincome'],
          { decimals: 2, flex: 1 }
        );
        this.grid.addColumnNumber(
          'marginalIncomeRatio',
          terms['common.syswholesellerprices.marginalincomeratio'],
          { decimals: 2, flex: 1 }
        );
        this.grid.addColumnText(
          'productProviderTypeText',
          terms['common.syswholesellerprices.providertype'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'purchaseUnit',
          terms['common.syswholesellerprices.purchaseunit'],
          { flex: 1 }
        );

        this.grid.setNbrOfRowsToShow(8, 8);
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();

        this.grid.setData([]);
      });
  }

  private filterModified(): void {
    debounce(() => {
      if (!this.isSearching) {
        this.loadProducts();
      }
    }, 500)();
  }

  private loadProducts(): void {
    if (!this.number() || this.number().length === 0) return;

    this.isSearching = true;
    this.perform
      .load$(
        this.service
          .searchInvoiceProductPrices(
            new SearchProductPricesModel(
              this.priceListTypeId(),
              this.customerId(),
              this.currencyId(),
              this.number(),
              this.providerType()
            )
          )
          .pipe(
            tap((prices: InvoiceProductPriceSearchViewDTO[]) => {
              const indexToFirst = prices.findIndex(
                r => r.sysWholesellerId === this.sysWholesellerId()
              );
              if (indexToFirst >= 0) {
                const objectToFirst = prices[indexToFirst];
                prices.splice(indexToFirst, 1);
                prices.unshift(objectToFirst);
              }

              prices.forEach(p => {
                if (p.priceListOrigin == PriceListOrigin.CompDbPriceList) {
                  p.wholeseller = p.wholeseller + ' (N*)';
                  this.compPriceExist = true;
                }

                if (p.productProviderType) {
                  const type = this.sysWholesalerTypes.find(
                    x => p.productProviderType === x.id
                  );
                  if (type) {
                    p.productProviderTypeText = type.name;
                  }
                }
              });

              this.rowData.next(prices);

              if (prices.length > 0) {
                this.grid?.api.getRenderedNodes()[0].setSelected(true);
              }
            })
          )
      )
      .subscribe(() => {
        this.isSearching = false;
      });
  }

  protected priceSelectionChanged(
    rows: InvoiceProductPriceSearchViewDTO[]
  ): void {
    if (rows.length > 0) this.selectedPrice.set(rows[0]);
  }

  private rowDoubleClicked(event: RowDoubleClickedEvent): void {
    this.rowSelected.emit(event.data);
  }

  ngOnDestroy(): void {
    this.numberEff?.destroy();
    this.providerTypeEff?.destroy();
    this.priceListTypeIdEff?.destroy();
  }
}
