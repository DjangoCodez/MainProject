import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  effect,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { PriceListDTO } from '@features/billing/models/pricelist.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPriceListTypeGridDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { orderBy } from 'lodash';
import { Observable, take, tap } from 'rxjs';
import { TermCollection } from '../../../../../../shared/localization/term-types';
import { InvoiceProductForm } from '../../../models/invoice-product-form.model';
import { ProductService } from '../../../services/product.service';

@Component({
  selector: 'soe-products-price-lists',
  templateUrl: './products-price-lists.component.html',
  styleUrls: ['./products-price-lists.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductsPriceListsComponent
  extends GridBaseDirective<PriceListDTO>
  implements OnInit, OnDestroy
{
  priceRows = model.required<PriceListDTO[]>();
  form = input.required<InvoiceProductForm>();
  readOnly = input<boolean>();

  private readonly coreService = inject(CoreService);
  private readonly productService = inject(ProductService);

  private useQuantityPrices: boolean = false;
  private allPriceLists = signal<IPriceListTypeGridDTO[]>([]);
  private hideAddRow = computed((): boolean => {
    return this.readOnly() ?? false;
  });

  private priceRowsEffRef = effect(() => {
    const pRows = this.priceRows();
    const priceLists = this.allPriceLists();
    pRows.forEach(pRow => {
      const priceList = priceLists.find(
        p => p.priceListTypeId === pRow.priceListTypeId
      );
      pRow.name = priceList ? priceList.name : '';
    });
    setTimeout(() => {
      let orderedRows = orderBy(
        pRows.filter(
          p => p.state === SoeEntityState.Active && p.name && p.name.length > 0
        ),
        ['name', 'quantity', 'startDate']
      );
      orderedRows = [
        ...orderedRows,
        ...pRows.filter(
          p =>
            p.state === SoeEntityState.Active && !(p.name && p.name.length > 0)
        ),
      ];
      this.rowData.next(orderedRows);
    });
  });

  ngOnInit(): void {
    this.startFlow(
      Feature.Billing_Product_Products_Edit,
      'Billing.Products.Products.Views.PriceLists',
      {
        skipInitialLoad: true,
        lookups: [this.loadPriceLists()],
      }
    );
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: CompanySettingType[] = [
      CompanySettingType.BillingUseQuantityPrices,
    ];
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap((settings: any) => {
        this.useQuantityPrices = SettingsUtil.getBoolCompanySetting(
          settings,
          CompanySettingType.BillingUseQuantityPrices,
          false
        );
      })
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideClearFilters: true,
      hideReload: true,
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newRow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          hidden: this.hideAddRow,
          onAction: this.addRow.bind(this),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<PriceListDTO>): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.cellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.product.pricelist.name',
        'billing.product.pricelist.price',
        'billing.products.pricelists.startdate',
        'billing.products.pricelists.stopdate',
        'core.delete',
        'common.quantity',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection): void => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnSelect<IPriceListTypeGridDTO>(
          'priceListTypeId',
          terms['billing.product.pricelist.name'],
          this.allPriceLists(),
          undefined,
          {
            dropDownIdLabel: 'priceListTypeId',
            dropDownValueLabel: 'name',
            editable: true,
            flex: 1,
          }
        );
        if (this.useQuantityPrices) {
          this.grid.addColumnNumber('quantity', terms['common.quantity'], {
            editable: true,
            decimals: 2,
            maxDecimals: 2,
            flex: 1,
          });
        }
        this.grid.addColumnNumber(
          'price',
          terms['billing.product.pricelist.price'],
          {
            editable: true,
            decimals: 2,
            maxDecimals: 4,
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'startDate',
          terms['billing.products.pricelists.startdate'],
          {
            enableHiding: true,
            editable: true,
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'stopDate',
          terms['billing.products.pricelists.stopdate'],
          {
            enableHiding: true,
            editable: true,
            flex: 1,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deleteRow(row),
        });

        this.exportFilenameKey.set('billing.invoices.pricelists.pricelist');
        this.grid.setNbrOfRowsToShow(10, 10);
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
        this.grid.updateGridHeightBasedOnNbrOfRows();
      });
  }

  private loadPriceLists(): Observable<IPriceListTypeGridDTO[]> {
    return this.productService.getPriceListTypesGrid().pipe(
      tap(priceLists => {
        priceLists.forEach(p => {
          p.name = `${p.name} (${p.currency})`;
        });
        this.allPriceLists.set(priceLists);
      })
    );
  }

  private addRow(): void {
    const row = new PriceListDTO();
    row.isModified = true;

    this.priceRows.update((rows: PriceListDTO[]): PriceListDTO[] => {
      const _rows = [...rows, row];
      return _rows;
    });
    this.form().markAsDirty();
    setTimeout((): void => {
      this.focusFirstCell();
    }, 100);
  }

  private deleteRow(row: PriceListDTO): void {
    row.state = SoeEntityState.Deleted;
    row.isModified = true;
    this.priceRows.update((rows: PriceListDTO[]): PriceListDTO[] => {
      const _rows = [
        ...rows.filter(r => r.priceListTypeId !== row.priceListTypeId),
        row,
      ];
      return _rows;
    });
  }

  private focusFirstCell(): void {
    const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
    this.grid?.api.setFocusedCell(lastRowIdx, 'priceListTypeId');
    this.grid?.api.startEditingCell({
      rowIndex: lastRowIdx,
      colKey: 'priceListTypeId',
    });
  }

  private cellValueChanged(event: CellValueChangedEvent): void {
    if (event.newValue !== event.oldValue) {
      const rowData = event.data as PriceListDTO;
      this.form().markAsDirty();
      rowData.isModified = true;

      if (event.colDef.colId === 'priceListTypeId') {
        rowData.name = this.getPriceListName(rowData.priceListTypeId);
        rowData.isModified = true;
        this.priceRows.update(rows => {
          const row = rows.find(
            r => r.priceListTypeId === rowData.priceListTypeId
          );
          if (row) row.name = rowData.name;
          return rows;
        });
      }
    }
  }

  private getPriceListName(priceListTypeId: number): string {
    const priceList = this.allPriceLists().find(
      p => p.priceListTypeId === priceListTypeId
    );
    return priceList ? priceList.name : '';
  }

  ngOnDestroy(): void {
    this.priceRowsEffRef?.destroy();
  }
}
