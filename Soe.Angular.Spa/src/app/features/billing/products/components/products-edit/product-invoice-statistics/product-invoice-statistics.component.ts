import {
  Component,
  OnInit,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { ProductService } from '@features/billing/products/services/product.service';
import { CustomerStatisticsDTO } from '@features/billing/sales-statistics/models/sales-statistics.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeOriginType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import {
  ProductStatisticsForm,
  ProductStatisticsModel,
} from './models/product-invoice-statistics.models';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-product-invoice-statistics',
  templateUrl: './product-invoice-statistics.component.html',
  styleUrls: ['./product-invoice-statistics.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductInvoiceStatisticsComponent
  extends GridBaseDirective<CustomerStatisticsDTO>
  implements OnInit
{
  productId = input.required<number>();

  private readonly validationHandler = inject(ValidationHandler);
  private readonly coreService = inject(CoreService);
  private readonly productService = inject(ProductService);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<CustomerStatisticsDTO[]>(
    this.progress
  );

  protected allItemsSelectionDict: SmallGenericType[] = [];
  protected originTypes: SmallGenericType[] = [];
  form = new ProductStatisticsForm({
    validationHandler: this.validationHandler,
    element: new ProductStatisticsModel(),
  });

  constructor() {
    super();
    effect(() => {
      const pId = this.productId();
      this.form?.productId.setValue(pId);
    });
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'common.statistics', {
      skipInitialLoad: true,
      lookups: [this.loadSelectionTypes(), this.loadOriginTypes()],
    });
  }

  private loadSelectionTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ChangeStatusGridAllItemsSelection,
        false,
        true,
        true
      )
      .pipe(
        tap(x => {
          this.allItemsSelectionDict = x;
          this.form?.allItemSelection.setValue(1);
        })
      );
  }

  private loadOriginTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.OriginType, true, false)
      .pipe(
        tap(origins => {
          const includes = [
            SoeOriginType.CustomerInvoice,
            SoeOriginType.Order,
            SoeOriginType.Offer,
            SoeOriginType.Contract,
          ];

          this.originTypes = origins.filter(o => includes.includes(o.id));
          this.form?.originType.setValue(SoeOriginType.CustomerInvoice);
        })
      );
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('search', {
          iconName: signal('search'),
          caption: signal('core.search'),
          tooltip: signal('core.search'),
          onAction: this.searchStatistics.bind(this),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<CustomerStatisticsDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'common.invoicenr',
        'common.quantity',
        'common.price',
        'common.amount',
        'common.purchaseprice',
        'billing.productrows.purchasepricesum',
        'common.customerinvoice',
        'common.order',
        'common.offer',
        'common.contract',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.addColumnDate('date', this.terms['common.date'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnText('invoiceNr', this.terms['common.invoicenr'], {
          flex: 1,
          enableGrouping: true,
        });
        this.grid.addColumnNumber(
          'productQuantity',
          this.terms['common.quantity'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber('productPrice', this.terms['common.price'], {
          flex: 1,
          enableGrouping: true,
          aggFuncOnGrouping: 'sum',
          decimals: 2,
        });
        this.grid.addColumnNumber(
          'productSumAmount',
          this.terms['common.amount'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            decimals: 2,
          }
        );
        this.grid.addColumnNumber(
          'productPurchasePrice',
          this.terms['common.purchaseprice'],
          {
            flex: 1,
            enableGrouping: true,
            decimals: 2,
          }
        );
        this.grid.addColumnNumber(
          'productPurchaseAmount',
          this.terms['billing.productrows.purchasepricesum'],
          {
            flex: 1,
            enableGrouping: true,
            aggFuncOnGrouping: 'sum',
            decimals: 2,
          }
        );

        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          stickyGrandTotalRow: 'bottom',
          hideGroupPanel: false,
        });

        this.exportFilenameKey.set('common.statistics');
        super.finalizeInitGrid();
      });
  }

  private searchStatistics(): void {
    const { productId, originType, allItemSelection } = this.form.getRawValue();
    this.perform.load(
      this.productService
        .getProductInvoiceStatistics(productId, originType, allItemSelection)
        .pipe(
          tap(rows => {
            this.rowData.next(rows);
          })
        )
    );
  }
}
