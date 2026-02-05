import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ProductUnitService } from '@features/billing/product-units/services/product-unit.service';
import { BatchUpdateComponent } from '@shared/components/batch-update/components/batch-update/batch-update.component';
import { BatchUpdateDialogData } from '@shared/components/batch-update/models/batch-update-dialog-data.model';
import {
  ProductSearchResult,
  SearchInvoiceProductDialogData,
} from '@shared/components/search-invoice-product-dialog/models/search-invoice-product-dialog.models';
import { SearchInvoiceProductDialogComponent } from '@shared/components/search-invoice-product-dialog/search-invoice-product-dialog.component';
import { SelectReportDialogComponent } from '@shared/components/select-report-dialog/components/select-report-dialog/select-report-dialog.component';
import {
  SelectReportDialogCloseData,
  SelectReportDialogData,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  CompanySettingType,
  Feature,
  SoeEntityType,
  SoeOriginType,
  SoeReportTemplateType,
  SoeTimeCodeType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { Dict } from '@ui/grid/services/selected-item.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import {
  InvoiceProductExtendedGridDTO,
  InvoiceProductGridDTO,
} from '../../models/invoice-product.model';
import { CopyInvoiceProductModel } from '../../models/product.model';
import { ProductService } from '../../services/product.service';
import { ProductsUnitConversionComponent } from './products-unit-conversion-dialog/products-unit-conversion-dialog.component';
import { ProductUnitConversionDialogData } from './products-unit-conversion-dialog/models/products-unit-conversion-dialog.models';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MultiValueCellRenderer } from '@ui/grid/cell-renderers/multi-value-cell-renderer/multi-value-cell-renderer.component';
import { IDefaultFilterSettings } from '@ui/grid/interfaces';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ProductsCleanupDialogData } from '../../models/products-cleanup-dialog.model';
import { ProductsCleanupDialogComponent } from './products-cleanup-dialog/products-cleanup-dialog-component/products-cleanup-dialog-component';

@Component({
  selector: 'soe-products-grid',
  templateUrl: './products-grid.component.html',
  styleUrls: ['./products-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductsGridComponent
  extends GridBaseDirective<InvoiceProductExtendedGridDTO, ProductService>
  implements OnInit
{
  service = inject(ProductService);
  private readonly progress = inject(ProgressService);
  private readonly coreService = inject(CoreService);
  private readonly dialogService = inject(DialogService);
  private readonly productUnitService = inject(ProductUnitService);
  private readonly perform = new Perform<any>(this.progress);
  private readonly reportService = inject(ReportService);
  private readonly messageBoxService = inject(MessageboxService);

  private hideUnitConversion = signal<boolean>(true);
  private hasBatchUpdatePermission = signal<boolean>(false);
  private rowsNotSelected = signal<boolean>(true);
  private selectedItems = signal<Dict>(<Dict>{});
  private disableSave = computed(() => {
    return Object.keys(this.selectedItems().dict).length <= 0;
  });

  protected defaultPriceListTypeId?: number;
  protected currencyId?: number;
  protected isDisabled = signal<boolean>(true);

  productGroupFilterOptions: SmallGenericType[] = [];
  materialCodeFilterOptions: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Product_Products,
      'Billing.Products.Product',
      {
        additionalModifyPermissions: [
          Feature.Billing_Stock,
          Feature.Billing_Product_Products_BatchUpdate,
        ],
        additionalReadPermissions: [
          Feature.Billing_Stock,
          Feature.Billing_Product_Products_BatchUpdate,
        ],
        lookups: [this.loadProductGroups(), this.loadMaterialCodes()],
      }
    );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.hideUnitConversion.set(
      !this.flowHandler.hasReadAccess(Feature.Billing_Stock)
    );
    this.hasBatchUpdatePermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Product_Products_BatchUpdate
      ) ?? false
    );
  }

  override loadCompanySettings(): Observable<void> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.BillingDefaultPriceListType,
        CompanySettingType.CoreBaseCurrency,
      ])
      .pipe(
        tap((settings: any) => {
          this.defaultPriceListTypeId = SettingsUtil.getIntCompanySetting(
            settings as any[],
            CompanySettingType.BillingDefaultPriceListType
          );
          this.currencyId = SettingsUtil.getIntCompanySetting(
            settings as any[],
            CompanySettingType.CoreBaseCurrency
          );
        })
      );
  }

  protected deleteRows(): void {
    this.messageBoxService
      .question(
        '',
        String(this.terms['billing.products.deleteselectedquestion']).replace(
          '{0}',
          String(this.grid.getSelectedCount())
        )
      )
      .afterClosed()
      .subscribe((res: IMessageboxComponentResponse): void => {
        if (res.result === true) {
          const productIds = this.grid.getSelectedIds('productId');
          this.perform.crud(
            CrudActionTypeEnum.Save,
            this.service.deleteProducts(productIds).pipe(
              tap((res: BackendResponse) => {
                if (res.success) {
                  this.refreshGrid();
                }
              })
            ),
            undefined,
            undefined,
            {
              showToastOnComplete: false,
            }
          );
        }
      });
  }

  override onGridReadyToDefine(
    grid: GridComponent<InvoiceProductExtendedGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.number',
        'common.name',
        'billing.products.productgroupcode',
        'billing.products.productgroupname',
        'billing.products.productcategories',
        'billing.products.eancode',
        'billing.products.external',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.info',
        'core.yes',
        'core.no',
        'billing.products.print.productlist.noselectedproducts',
        'common.batchupdate',
        'billing.product.materialcode',
        'billing.products.deleteselectedquestion',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.enableRowSelection();
        this.grid.addColumnActive('isActive', terms['common.active'], {
          width: 60,
          enableHiding: false,
          editable: true,
          idField: 'productId',
        });
        this.grid.addColumnText('number', terms['common.number'], {
          flex: 1,
          enableHiding: true,
          filterOptions: ['startsWith', 'contains', 'endsWith'],
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnText(
          'productGroupCode',
          terms['billing.products.productgroupcode'],
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnSelect(
          'productGroupId',
          terms['billing.products.productgroupname'],
          this.productGroupFilterOptions,
          undefined,
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnText(
          'productCategoriesArray',
          terms['billing.products.productcategories'],
          {
            flex: 1,
            enableHiding: true,
            cellRenderer: MultiValueCellRenderer,
            filter: 'agSetColumnFilter',
          }
        );

        this.grid.addColumnText('eanCode', terms['billing.products.eancode'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnSelect(
          'isExternalId',
          terms['billing.products.external'],
          [
            { id: 0, name: terms['core.no'] },
            { id: 1, name: terms['core.yes'] },
          ],
          undefined,
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnSelect(
          'timeCodeId',
          terms['billing.product.materialcode'],
          this.materialCodeFilterOptions,
          undefined,
          {
            flex: 1,
            enableHiding: true,
            hide: true,
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        const defaultFilter: IDefaultFilterSettings = {
          field: 'isActive',
          filterModel: {
            values: ['true'],
          },
        };

        super.finalizeInitGrid(undefined, defaultFilter);
      });
  }

  protected gridRowSelectedChanged(rows: InvoiceProductGridDTO[]): void {
    this.isDisabled.set(rows.length === 0);
    this.rowsNotSelected.set(rows.length === 0);
  }

  override selectedItemsChanged(items: Dict): void {
    super.selectedItemsChanged(items);
    this.selectedItems.set(items);
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      saveOption: {
        onAction: this.saveState.bind(this),
        disabled: this.disableSave,
      },
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('print', {
          iconName: signal('print'),
          tooltip: signal('core.print'),
          caption: signal('core.print'),
          disabled: this.rowsNotSelected,
          onAction: this.openReportDialog.bind(this),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('search', {
          iconName: signal('search'),
          tooltip: signal('billing.products.searchfromexternalproducts'),
          caption: signal('billing.products.searchfromexternalproducts'),
          onAction: this.searchProducts.bind(this),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('units', {
          iconName: signal('divide'),
          tooltip: signal('billing.products.product.unitconversion'),
          caption: signal('billing.products.product.unitconversion'),
          disabled: this.rowsNotSelected,
          hidden: this.hideUnitConversion,
          onAction: this.importProductUnitConversions.bind(this),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('batchUpdate', {
          iconName: signal('pencil'),
          tooltip: signal('common.batchupdate.title'),
          caption: signal('common.batchupdate.title'),
          disabled: this.rowsNotSelected,
          onAction: this.openBatchUpdate.bind(this),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('cleanup', {
          iconName: signal('broom'),
          tooltip: signal('billing.products.cleanup.title'),
          caption: signal('billing.products.cleanup.title'),
          onAction: this.openCleanupProducts.bind(this),
        }),
      ],
    });
  }

  private saveState(): void {
    if (Object.keys(this.selectedItems().dict ?? {}).length <= 0) return;

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.updateProductState(this.selectedItems()),
      (response: BackendResponse) => {
        if (response.success) {
          this.refreshGrid();
        }
      },
      undefined
    );
  }

  private searchProducts(): void {
    const dialogOpts = <Partial<SearchInvoiceProductDialogData>>{
      size: 'xl',
      disableClose: true,
      hideProducts: false,
      priceListTypeId: 0,
      customerId: 0,
      currencyId: this.currencyId,
      sysWholesellerId: undefined,
      number: '',
      name: '',
      quantity: 1,
    };
    this.dialogService
      .open(SearchInvoiceProductDialogComponent, dialogOpts)
      .afterClosed()
      .subscribe(result => {
        if (result && result instanceof ProductSearchResult) {
          this.copyInvoiceProduct(result);
        }
      });
  }

  private copyInvoiceProduct(searchResult: ProductSearchResult): void {
    const copyModel = new CopyInvoiceProductModel(
      searchResult.productId,
      searchResult.purchasePrice,
      searchResult.salesPrice,
      searchResult.productUnit,
      searchResult.priceListTypeId,
      searchResult.sysPriceListHeadId,
      searchResult.sysWholesalerName,
      0,
      searchResult.priceListOrigin
    );

    this.service.copyInvoiceProduct(copyModel).subscribe(result => {
      if (result && result.product) {
        this.refreshGrid();
      }
    });
  }

  private importProductUnitConversions(): void {
    const dialogOpts = <Partial<ProductUnitConversionDialogData>>{
      size: 'lg',
      title: 'billing.products.product.unitconversion',
      disableClose: true,
      productIds: this.grid.getSelectedIds('productId'),
    };
    this.dialogService
      .open(ProductsUnitConversionComponent, dialogOpts)
      .afterClosed()
      .subscribe(res => {
        if (res === true) {
          this.perform.load(
            this.productUnitService.getGrid(undefined, { useCache: false })
          );
        }
      });
  }

  private openBatchUpdate(): void {
    const dialogOpts = <Partial<BatchUpdateDialogData>>{
      title: 'common.batchupdate.title',
      size: 'lg',
      disableClose: true,
      entityType: SoeEntityType.InvoiceProduct,
      selectedIds: this.grid?.getSelectedIds('productId'),
    };
    this.dialogService
      .open(BatchUpdateComponent, dialogOpts)
      .afterClosed()
      .subscribe(res => {
        if (res) {
          this.refreshGrid();
        }
      });
  }

  private openReportDialog(): void {
    const dialogData = new SelectReportDialogData();
    dialogData.title = 'common.selectreport';
    dialogData.size = 'lg';
    dialogData.reportTypes = [SoeReportTemplateType.ProductListReport];
    dialogData.showCopy = false;
    dialogData.showEmail = false;
    dialogData.copyValue = false;
    dialogData.reports = [];
    dialogData.defaultReportId = 0;
    dialogData.langId = SoeConfigUtil.languageId;
    dialogData.showReminder = false;
    dialogData.showLangSelection = false;
    dialogData.showSavePrintout = false;
    dialogData.savePrintout = false;
    const selectReportDialog = this.dialogService.open(
      SelectReportDialogComponent,
      dialogData
    );

    selectReportDialog
      .afterClosed()
      .subscribe((result: SelectReportDialogCloseData) => {
        if (result && result.reportId) {
          const productIds: number[] = this.grid
            .getSelectedRows()
            .map(p => p.productId);

          this.perform.load(
            this.reportService
              .getProductListReportUrl(
                productIds,
                result.reportId,
                SoeReportTemplateType.ProductListReport
              )
              .pipe(
                tap(url => {
                  BrowserUtil.openInSameTab(window, url);
                })
              )
          );
        }
      });
  }

  private loadProductGroups(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getProductGroups()
        .pipe(
          tap(
            pGroups =>
              (this.productGroupFilterOptions = pGroups.map(
                p => new SmallGenericType(p.productGroupId, p.name)
              ))
          )
        )
    );
  }

  private loadMaterialCodes(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getMaterialCodes(SoeTimeCodeType.Material, true, false).pipe(
        tap(mCodes => {
          this.materialCodeFilterOptions = mCodes.map(
            x => new SmallGenericType(x.timeCodeId, x.name)
          );
          this.materialCodeFilterOptions.splice(
            0,
            0,
            new SmallGenericType(0, '')
          );
        })
      )
    );
  }

  private openCleanupProducts(): void {
    const dialogData = new ProductsCleanupDialogData();
    dialogData.title = this.translate.instant('billing.products.cleanup.title');
    dialogData.size = 'lg';
    dialogData.originType = SoeOriginType.None;

    this.dialogService
      .open(ProductsCleanupDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result) {
          console.log('Cleanup confirmed');
        }
      });
  }
}
