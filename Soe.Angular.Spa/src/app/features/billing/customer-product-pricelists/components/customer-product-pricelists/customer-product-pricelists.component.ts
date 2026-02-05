import {
  Component,
  Input,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { CustomerProductPricelistsTypeService } from '../../services/customer-product-priceliststype.service';
import { InvoiceProductSmallDTO } from '../../models/customer-product-pricelist.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CustomerProductPriceListsService,
  PriceListsForm,
} from './services/customer-product-pricelists.service';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { SettingsUtil } from '@shared/util/settings-util';
import { addEmptyOption } from '@shared/util/array-util';
import {
  CellClassParams,
  CellValueChangedEvent,
  RowDataUpdatedEvent,
  TabToNextCellParams,
} from 'ag-grid-community';
import { PriceListsValidatorService } from './services/pricelists-validator.service';
import { Subscription, of } from 'rxjs';
import { SoeFormGroup } from '@shared/extensions';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IPriceListDTO } from '@shared/models/generated-interfaces/PriceListDTOs';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

@Component({
  selector: 'soe-customer-product-pricelists',
  templateUrl: './customer-product-pricelists.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerProductPriceListsComponent
  extends GridBaseDirective<IPriceListDTO>
  implements OnInit, OnDestroy
{
  @Input() parentForm!: SoeFormGroup;
  @Input() showPriceListTypeSelection = false;
  @Input() showProductSelection = false;

  @ViewChild(GridComponent)
  rowSubscription?: Subscription;

  products: InvoiceProductSmallDTO[] = [];

  private priceListsTypeService = inject(CustomerProductPricelistsTypeService);
  private priceListsService = inject(CustomerProductPriceListsService);
  private priceListsValidator = inject(PriceListsValidatorService);
  private coreService = inject(CoreService);

  private useQuantityPrices = false;

  get showHistoricPrices() {
    return this.priceListsService.showHistoricPrices;
  }

  set showHistoricPrices(value: boolean) {
    this.priceListsService.showHistoricPrices = value;
    this.priceListsService.updateRows();
  }

  ngOnInit(): void {
    this.priceListsService.init(
      this.parentForm as PriceListsForm,
      this.showProductSelection,
      this.showPriceListTypeSelection
    );
    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit,
      'Billing.Products.Pricelists',
      {
        skipInitialLoad: true,
        lookups: [this.loadProducts()],
        useLegacyToolbar: true,
      }
    );
  }

  ngOnDestroy() {
    this.rowSubscription?.unsubscribe();
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.BillingUseQuantityPrices])
      .pipe(
        tap(setting => {
          this.useQuantityPrices = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.BillingUseQuantityPrices
          );
        })
      );
  }

  //Setup
  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => {
          this.priceListsService.clearEmptyRows();
          this.refreshGrid();
        },
      },
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: 'plus',
          title: 'core.add',
          onClick: () => this.addRow(),
          disabled: signal(false), //!this.handler.modifyPermission,
          hidden: signal(false),
        }),
      ],
    });
  }

  onGridReadyToDefine(grid: GridComponent<PriceListDTO>) {
    super.onGridReadyToDefine(grid);
    this.rowSubscription = this.priceListsService.rows$.subscribe(rows => {
      this.rowData.next(this.priceListsService.filterRows(rows));
    });

    this.grid.api.updateGridOptions({
      onRowDataUpdated: this.onDataUpdated.bind(this),
      tabToNextCell: this.onTabToNextCell.bind(this),
      onCellValueChanged: this.onCellChanged.bind(this),
    });
    this.grid.agGrid.api.sizeColumnsToFit(); //Waybe should an attribute directive linked to the grid component?

    this.translate
      .get([
        'common.name',
        'billing.product.number',
        'billing.products.pricelists.purchaseprice',
        'billing.products.pricelists.price',
        'billing.products.pricelists.startdate',
        'billing.products.pricelists.stopdate',
        'billing.products.pricelists.pricelist',
        'common.quantity',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        if (this.showProductSelection) {
          this.addProductSelectionColumns(grid, terms);
        }

        this.grid.addColumnNumber(
          'price',
          terms['billing.products.pricelists.price'],
          { decimals: 2, maxDecimals: 4, editable: true, flex: 10 }
        );

        if (this.useQuantityPrices) {
          this.grid.addColumnNumber('quantity', terms['common.quantity'], {
            decimals: 2,
            maxDecimals: 2,
            editable: true,
            flex: 10,
          });
        }

        this.grid.addColumnDate(
          'startDateDisplay',
          terms['billing.products.pricelists.startdate'],
          {
            editable: true,
            flex: 10,
            cellClassRules: {
              'alert-danger': (params: CellClassParams) =>
                !this.priceListsValidator.validDates(params.data),
            },
          }
        );
        this.grid.addColumnDate(
          'stopDateDisplay',
          terms['billing.products.pricelists.stopdate'],
          {
            editable: true,
            flex: 10,
            cellClassRules: {
              'alert-danger': (params: CellClassParams) =>
                !this.priceListsValidator.validDates(params.data),
            },
          }
        );
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });
        super.finalizeInitGrid();
      });
  }

  addProductSelectionColumns(grid: GridComponent<PriceListDTO>, terms: any) {
    grid.addColumnAutocomplete<InvoiceProductSmallDTO>(
      'productId',
      terms['billing.product.number'],
      {
        editable: true,
        flex: 20,
        source: _ => this.products,
        updater: (row, product) => {
          this.priceListsService.setProductValue(row, product);
          if (!row.productId) {
            this.rowChanged(row);
          }
        },
        optionIdField: 'productId',
        optionNameField: 'numberName',
        optionDisplayNameField: 'number',
        cellClassRules: {
          'alert-danger': (params: CellClassParams) =>
            !this.priceListsValidator.validProduct(
              this.showProductSelection,
              params.data
            ),
        },
      }
    );
    grid.addColumnText('name', terms['common.name'], { flex: 30 });
    grid.addColumnNumber(
      'purchasePrice',
      terms['billing.products.pricelists.purchaseprice'],
      { decimals: 2, maxDecimals: 4, flex: 10 }
    );
  }

  loadProducts() {
    return this.priceListsTypeService.getProducts(true).pipe(
      tap(data => {
        addEmptyOption(data);
        this.products = data;
      })
    );
  }

  //Actions
  deleteRow(row: PriceListDTO) {
    this.priceListsService.deleteRow(row);
  }

  addRow() {
    this.priceListsService.addRow();
    this.grid.options.context.newRow = true;
  }

  rowChanged(row: any) {
    this.priceListsService.rowIsModified(row);
    this.grid.refreshCells();
  }

  //Grid events
  onDataUpdated(event: RowDataUpdatedEvent) {
    if (event.context.newRow && !event.api.isAnyFilterPresent()) {
      const index = event.api.getLastDisplayedRowIndex();
      event.api.setFocusedCell(index, 'productId');
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'productId',
      });
      event.context.newRow = false;
    }
  }

  onCellChanged(event: CellValueChangedEvent) {
    const { colDef, data, newValue, oldValue } = event;
    switch (colDef.field) {
      case 'productId': {
        if (newValue !== oldValue && newValue) {
          data.purchasePrice =
            this.products.find(x => x.productId === newValue)?.purchasePrice ??
            0;
        }
        break;
      }
    }

    this.rowChanged(data);
  }

  onTabToNextCell(event: TabToNextCellParams) {
    if (
      !event.backwards &&
      event.nextCellPosition?.column.isPinned() &&
      event.previousCellPosition.rowIndex ===
        event.api.getLastDisplayedRowIndex()
    ) {
      this.addRow();
      return false; //Don't perform tab
    }
    return event.nextCellPosition || false;
  }
}
