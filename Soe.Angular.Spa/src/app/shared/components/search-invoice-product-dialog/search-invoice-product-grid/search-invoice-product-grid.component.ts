import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  model,
  output,
  signal,
} from '@angular/core';
import { InvoiceProductPriceSearchViewDTO } from '@shared/components/sys-wholesaler-prices/models/sys-wholesaler-prices.models';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  TermGroup_InitProductSearch,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IInvoiceProductPriceSearchViewDTO,
  IInvoiceProductSearchViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISysProductGroupSmallDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClickedEvent, GridSizeChangedEvent } from 'ag-grid-community';
import { Observable, catchError, take, tap } from 'rxjs';
import { ValidationHandler } from '../../../handlers/validation.handler';
import { CoreService } from '../../../services/core.service';
import { SearchInvoiceProductForm } from '../models/search-invoice-product-dialog-form.model';
import {
  ProductSearchResult,
  SearchInvoiceProductDialogData,
} from '../models/search-invoice-product-dialog.models';
import { SearchInvoiceProductDialogServiceService } from '../services/search-invoice-product-dialog.service';
import { ProductImageCellRendererComponent } from './product-image/product-image-cell-renderer';
import { SysWholesalerPricesService } from '@shared/components/sys-wholesaler-prices/services/sys-wholesaler-prices.service';

@Component({
  selector: 'soe-search-invoice-product-grid',
  templateUrl: './search-invoice-product-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SearchInvoiceProductGridComponent
  extends GridBaseDirective<
    IInvoiceProductSearchViewDTO,
    SearchInvoiceProductDialogServiceService
  >
  implements OnInit, OnDestroy
{
  dialogData = model<SearchInvoiceProductDialogData>(
    new SearchInvoiceProductDialogData()
  );
  productPriceResult = model<ProductSearchResult>();
  productResult = model<IInvoiceProductSearchViewDTO>();
  triggerClose = output();

  service = inject(SearchInvoiceProductDialogServiceService);
  sysWholesalerPricesService = inject(SysWholesalerPricesService);

  private readonly coreService = inject(CoreService);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<any>(this.progress);
  private readonly validationHandler = inject(ValidationHandler);

  private isSearching: boolean = false;
  private useAutoSearch: boolean = false;
  protected useExtendSearchInfo = false;
  private productSearchMinPrefixLength: number = 2;
  private productSearchMinPopulateDelay: number = 100;
  protected selectedProductRow = signal<
    IInvoiceProductSearchViewDTO | undefined
  >(undefined);

  protected hasSearchedProducts = signal<boolean>(false);
  protected showPriceListSelect = signal<boolean>(false);
  protected priceLists = signal<SmallGenericType[]>([]);

  firstTierCategories: ISysProductGroupSmallDTO[] = [];
  secondTierCategories = signal<ISysProductGroupSmallDTO[]>([]);
  secondTierCategoriesAll: ISysProductGroupSmallDTO[] = [];

  private selectedFirstTierCategoryId: number = 0;
  private selectedSecondTierCategoryId: number = 0;

  protected selectedProductPrice = signal<
    InvoiceProductPriceSearchViewDTO | undefined
  >(undefined);
  protected form = new SearchInvoiceProductForm({
    validationHandler: this.validationHandler,
    element: new SearchInvoiceProductDialogData(),
  });

  selectedProductPriceEff = effect(() => {
    const p = this.selectedProductPrice();
    if (p) this.productPriceResult.set(this.getProductPriceResult(p));
    else this.productPriceResult.set(undefined);
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.form.reset(this.dialogData);
    this.startFlow(
      Feature.None,
      'Billing.Dialogs.SearchInvoiceProduct.Product',
      {
        skipInitialLoad: true,
        lookups: [this.loadPriceLists(), this.loadProductGroups()],
      }
    );
  }

  override loadCompanySettings(): Observable<void> {
    return this.perform.load$(
      this.coreService
        .getCompanySettings([
          CompanySettingType.BillingInitProductSearch,
          CompanySettingType.BillingShowExtendedInfoInExternalSearch,
        ])
        .pipe(
          tap((settings: any) => {
            const searchType = SettingsUtil.getIntCompanySetting(
              settings as any[],
              CompanySettingType.BillingInitProductSearch,
              TermGroup_InitProductSearch.WithEnter
            );
            this.useExtendSearchInfo = SettingsUtil.getBoolCompanySetting(
              settings,
              CompanySettingType.BillingShowExtendedInfoInExternalSearch
            );
            this.useAutoSearch =
              searchType === TermGroup_InitProductSearch.Automatic;
          })
        )
    );
  }

  override loadUserSettings(): Observable<void> {
    return this.perform.load$(
      this.coreService
        .getUserSettings([
          UserSettingType.BillingProductSearchMinPrefixLength,
          UserSettingType.BillingProductSearchMinPopulateDelay,
        ])
        .pipe(
          tap(settings => {
            this.productSearchMinPrefixLength = SettingsUtil.getIntUserSetting(
              settings,
              UserSettingType.BillingProductSearchMinPrefixLength,
              this.productSearchMinPrefixLength
            );
            this.productSearchMinPopulateDelay = SettingsUtil.getIntUserSetting(
              settings,
              UserSettingType.BillingProductSearchMinPopulateDelay,
              this.productSearchMinPopulateDelay
            );
          })
        )
    );
  }

  private loadPriceLists(): Observable<SmallGenericType[]> {
    return this.service.getPriceLists(true).pipe(
      tap(p => {
        this.priceLists.set(p);
        if (this.dialogData().priceListTypeId === 0)
          this.showPriceListSelect.set(true);
      })
    );
  }

  private loadProductGroups(): Observable<ISysProductGroupSmallDTO[]> {
    return this.service.VVSGroupsForSearch().pipe(
      tap(p => {
        this.firstTierCategories = p.filter(x => !x.parentSysProductGroupId);
        this.firstTierCategories.splice(0, 0, {
          sysProductGroupId: 0,
          parentSysProductGroupId: 0,
          identifier: '',
          name: this.terms['common.all'],
        });

        this.secondTierCategoriesAll = p.filter(x => x.parentSysProductGroupId);
        this.secondTierCategories.set([]);
      })
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['common.all']);
  }

  override onGridReadyToDefine(
    grid: GridComponent<IInvoiceProductSearchViewDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    if (this.useExtendSearchInfo) this.grid.setNbrOfRowsToShow(10, 10);
    else this.grid.setNbrOfRowsToShow(8, 8);

    this.grid.context.suppressGridMenu = true;

    this.grid.api.updateGridOptions({
      onGridSizeChanged: (event: GridSizeChangedEvent): void => {
        if (event.clientHeight > 0 && event.clientWidth > 0)
          this.grid.api.sizeColumnsToFit();
      },
      onCellClicked: (event: CellClickedEvent) => {
        if (event && event.rowIndex !== null) this.selectRow(event.rowIndex);
      },
      rowHeight: this.useExtendSearchInfo ? 40 : 25,
    });

    this.translate
      .get([
        'common.number',
        'common.name',
        'common.searchinvoiceproduct.showexternalproductinfo',
        'common.manufacturer',
        'core.info',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(() => true, true);

        this.grid.addColumnIcon('imageUrl', '', {
          suppressFilter: true,
          width: 60,
          minWidth: 60,
          maxWidth: 60,
          cellRenderer: ProductImageCellRendererComponent,
          hide: !this.useExtendSearchInfo,
        });

        this.grid.addColumnText('number', terms['common.number'], {
          flex: 1,
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });

        this.grid.addColumnText('extendedInfo', terms['core.info'], {
          flex: 2,
          hide: !this.useExtendSearchInfo,
        });

        this.grid.addColumnText('manufacturer', terms['common.manufacturer'], {
          flex: 3,
          hide: !this.useExtendSearchInfo,
        });

        this.grid.addColumnIcon(null, '', {
          showIcon: (row): boolean => {
            return row.endAt !== null && row.endAt !== undefined;
          },
          iconName: 'ban',
          iconClass: 'errorColor',
          tooltipField: 'endAtTooltip',
        });
        this.grid.addColumnIcon(null, '', {
          showIcon: (row): boolean => {
            return row.type === 2;
          },
          iconName: 'arrow-up-right-from-square',
          tooltip: terms['common.searchinvoiceproduct.showexternalproductinfo'],
          onClick: this.openExternalUrl.bind(this),
        });
        super.finalizeInitGrid();
        this.grid?.setData([]);
      });
  }

  private openExternalUrl(row: IInvoiceProductSearchViewDTO): void {
    BrowserUtil.openInNewTab(window, row.externalUrl);
  }

  doSearch(searchText: string) {
    let group: string | undefined = undefined;

    if (this.selectedSecondTierCategoryId > 0)
      group =
        this.secondTierCategories().find(
          x => x.sysProductGroupId === this.selectedSecondTierCategoryId
        )?.identifier ?? undefined;
    else if (this.selectedFirstTierCategoryId)
      group =
        this.firstTierCategories.find(
          x => x.sysProductGroupId === this.selectedFirstTierCategoryId
        )?.identifier ?? undefined;

    this.isSearching = true;
    this.perform
      .load$(
        this.service
          .searchInvoiceProductsExtended(
            searchText === '' ? undefined : searchText,
            group
          )
          .pipe(
            catchError(err => {
              this.isSearching = false;
              throw new Error(err);
            }),
            tap((products: IInvoiceProductSearchViewDTO[]) => {
              this.rowData.next(products);
              if (products.length > 0)
                setTimeout(() => {
                  this.selectRow(0);
                });
              this.hasSearchedProducts.set(products.length > 0);
            })
          )
      )
      .subscribe(() => {
        this.isSearching = false;
      });
  }

  firstTierCategoryChanged(category: number) {
    this.selectedFirstTierCategoryId = category;

    if (category) {
      const filteredCategories = this.secondTierCategoriesAll.filter(
        x => x.parentSysProductGroupId === category
      );

      filteredCategories.splice(0, 0, {
        sysProductGroupId: 0,
        parentSysProductGroupId: 0,
        identifier: '',
        name: this.terms['common.all'],
      });

      this.secondTierCategories.set(filteredCategories);
    } else this.secondTierCategories.set([]);
  }

  secondTierCategoryChanged(category: number) {
    this.selectedSecondTierCategoryId = category;
  }

  productSelectionChanged(rows: IInvoiceProductSearchViewDTO[]): void {
    if (rows && rows.length > 0) {
      this.productResult.set(rows[0]);
      this.selectedProductRow.set(rows[0]);
    } else {
      this.productResult.set(undefined);
      this.selectedProductRow.set(undefined);
    }
  }

  protected wholesalerPriceSelected(
    price: IInvoiceProductPriceSearchViewDTO
  ): void {
    if (price) {
      this.productPriceResult.set(this.getProductPriceResult(price));
    } else {
      this.productPriceResult.set(undefined);
    }
    this.triggerClose.emit();
  }

  private getProductPriceResult(
    price: IInvoiceProductPriceSearchViewDTO
  ): ProductSearchResult {
    const product = new ProductSearchResult();
    product.productId = price.productId;
    product.priceListTypeId = this.form.priceListTypeId.value;
    product.purchasePrice = price.nettoNettoPrice ? price.nettoNettoPrice : 0;
    product.salesPrice = price.customerPrice ? price.customerPrice : 0;
    product.productName = price.name || '';
    product.productUnit = price.salesUnit
      ? price.salesUnit
      : price.purchaseUnit;
    product.sysPriceListHeadId = price.sysPriceListHeadId;
    product.sysWholesalerName = price.wholeseller;
    product.priceListOrigin = price.priceListOrigin;
    product.quantity = this.form.getAllValues({
      includeDisabled: true,
    }).quantity;

    return product;
  }

  private selectRow(index: number): void {
    const rows = this.grid?.api.getRowNode(String(index));
    if (rows) {
      rows.setSelected(true);
    }
  }

  ngOnDestroy(): void {
    this.selectedProductPriceEff?.destroy();
  }
}
