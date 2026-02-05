import {
  Component,
  Input,
  OnInit,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { PurchaseProductsEditComponent } from '@features/billing/purchase-products/components/purchase-products-edit/purchase-products-edit.component';
import { PurchaseProductForm } from '@features/billing/purchase-products/models/purchase-product-form.model';
import { PurchaseProductsService } from '@features/billing/purchase-products/services/purchase-products.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  ISupplierProductGridDTO,
  ISupplierProductSearchDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { TermCollection } from '../../../../../../shared/localization/term-types';
import { BillingService } from '@features/billing/services/services/billing.service';
import { SupplierProductPriceDTO } from '@features/billing/purchase-products/models/purchase-product.model';
import { orderBy } from 'lodash';
import { IProductBasicInfo } from '@features/billing/products/models/product.model';
import { ProductService } from '@features/billing/products/services/product.service';
import { ActionTaken } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';

@Component({
  selector: 'soe-supplier-products',
  templateUrl: './supplier-products.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierProductsComponent
  extends GridBaseDirective<ISupplierProductGridDTO>
  implements OnInit
{
  @Input() actionTakenSignal = signal<ActionTaken | undefined>(undefined);
  productId = input.required<number>();
  parentProductInfo = input<IProductBasicInfo>();

  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<ISupplierProductGridDTO[]>(
    this.progress
  );
  purchaseProductService = inject(PurchaseProductsService);
  productService = inject(ProductService);
  private readonly billingService = inject(BillingService);

  constructor() {
    super();
    effect(() => {
      const prodId = this.productId();
      this.loadData(undefined, { invoiceProductId: prodId }).subscribe();
    });

    effect(() => {
      const action = this.actionTakenSignal();
      if (action?.type === CrudActionTypeEnum.Save) {
        const savedProductId = action.additionalProps?.productId;

        if (savedProductId && savedProductId === this.productId()) {
          this.loadData(undefined, {
            invoiceProductId: this.productId(),
          }).subscribe();
        }
      }
    });
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Purchase,
      'Billing.Products.Products.Views.SupplierProducts',
      { skipInitialLoad: true }
    );
    this.exportFilenameKey.set('billing.product.stocks.stock');
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideClearFilters: true,
      hideReload: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('createNew', {
          iconName: signal('plus'),
          caption: signal('common.createnew'),
          onAction: this.triggerNewPurchaseProduct.bind(this),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISupplierProductGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.product.supplieritemno',
        'billing.purchase.product.supplieritemname',
        'billing.product.purchaseprice',
        'billing.purchase.product.pricestartdate',
        'billing.purchase.product.priceqty',
        'billing.purchase.product.priceenddate',
        'common.currency',
        'billing.purchase.product.linkedtopricelist',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection) => {
        // Configure master-detail grid
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 200,
            columnDefs: [
              ColumnUtil.createColumnModified('isModified'),
              ColumnUtil.createColumnNumber(
                'quantity',
                terms['billing.purchase.product.priceqty'],
                {
                  flex: 1,
                  decimals: 2,
                }
              ),
              ColumnUtil.createColumnNumber(
                'price',
                terms['billing.product.purchaseprice'],
                {
                  flex: 1,
                  decimals: 2,
                }
              ),
              ColumnUtil.createColumnText(
                'currencyCode',
                terms['common.currency'],
                {
                  flex: 1,
                }
              ),

              ColumnUtil.createColumnDate(
                'startDate',
                terms['billing.purchase.product.pricestartdate'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnDate(
                'endDate',
                terms['billing.purchase.product.priceenddate'],
                {
                  flex: 1,
                }
              ),
            ],
          },
          {
            autoHeight: false,
            getDetailRowData: (params: any) => {
              this.loadDetailRows(params);
            },
          }
        );

        // Master grid columns
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'supplierProductNr',
          terms['billing.purchase.product.supplieritemno'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'supplierProductName',
          terms['billing.purchase.product.supplieritemname'],
          { flex: 1 }
        );

        if (this.flowHandler.modifyPermission()) {
          this.grid.addColumnIconEdit({
            onClick: this.triggerEditPurchaseProduct.bind(this),
          });
        }

        this.grid.setNbrOfRowsToShow(8, 8);
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
        this.grid.updateGridHeightBasedOnNbrOfRows();
      });
  }

  override loadData(
    id?: number,
    additionalProps?: {
      invoiceProductId: number;
    }
  ): Observable<ISupplierProductGridDTO[]> {
    if (!additionalProps?.invoiceProductId) return of([]);

    return this.perform.load$(
      this.purchaseProductService
        .getGrid(undefined, {
          searchDto: {
            invoiceProductId: additionalProps?.invoiceProductId,
          } as ISupplierProductSearchDTO,
        })
        .pipe(
          tap(data => {
            this.rowData.next(data);
          })
        )
    );
  }

  loadDetailRows(params: any) {
    this.performLoadData
      .load$(
        this.billingService.getSupplierProductPricesGrid(
          params.data.supplierProductId
        )
      )
      .subscribe((priceRows: SupplierProductPriceDTO[]) => {
        const orderedPriceRows = orderBy(priceRows, [
          'currencyCode',
          'quantity',
          'startDate',
        ]);

        params.successCallback(orderedPriceRows);
      });
  }

  private openPurchaseProduct(row?: ISupplierProductGridDTO): void {
    this.openEditInNewTab.emit({
      id: row?.supplierProductId ?? 0,
      additionalProps: {
        editComponent: PurchaseProductsEditComponent,
        FormClass: PurchaseProductForm,
        editTabLabel: 'billing.purchase.product.product',
      },
    });
  }

  private triggerNewPurchaseProduct(): void {
    this.productService.parentProductContext = this.parentProductInfo();
    this.openPurchaseProduct(undefined);
  }

  private triggerEditPurchaseProduct(row: ISupplierProductGridDTO): void {
    this.openPurchaseProduct(row);
  }
}
