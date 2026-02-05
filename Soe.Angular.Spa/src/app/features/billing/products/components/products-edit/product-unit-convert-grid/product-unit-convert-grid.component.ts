import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { InvoiceProductForm } from '@features/billing/products/models/invoice-product-form.model';
import { ProductUnitConvertDTO } from '@features/billing/products/models/product.model';
import { ProductService } from '@features/billing/products/services/product.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { Observable, take, tap } from 'rxjs';

type ProductUnitConvertGridDTO = AG_NODE<ProductUnitConvertDTO>;

@Component({
  selector: 'soe-product-unit-convert-grid',
  templateUrl: './product-unit-convert-grid.component.html',
  styleUrls: ['./product-unit-convert-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductUnitConvertGridComponent
  extends GridBaseDirective<ProductUnitConvertDTO>
  implements OnInit, OnDestroy
{
  productId = input.required<number>();
  productUnitConverts = model.required<ProductUnitConvertDTO[]>();
  productUnitId = input.required<number>();
  form = model.required<InvoiceProductForm>();

  productService = inject(ProductService);
  private readonly messageBox = inject(MessageboxService);
  private units = signal<SmallGenericType[]>([]);
  private productUnits: SmallGenericType[] = [];

  private setRowDataEff = effect((): void => {
    const rows = this.productUnitConverts();
    setTimeout(() => {
      this.rowData.next(rows.filter(x => !x.isDeleted));
    });
  });

  private productUnitsEff = effect((): void => {
    const pUId = this.productUnitId();
    const pUnits = this.units();
    if ((pUId || pUId === 0) && pUnits.length > 0) {
      this.productUnits = pUnits.filter(u => u.id !== pUId);
    }
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Product_Products_Edit,
      'Billing.Products.Products.Views.Stocks',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [this.loadProductUnits()],
      }
    );
  }

  private loadProductUnits(): Observable<SmallGenericType[]> {
    return this.productService
      .getProductUnitsDict()
      .pipe(tap(u => this.units.set(u)));
  }

  override createLegacyGridToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: ['fal', 'plus'],
          label: 'common.newrow',
          onClick: this.addRow.bind(this),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ProductUnitConvertDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.cellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.product.productunit',
        'billing.product.productunit.convertfactor',
        'core.delete',
        'billing.product.productunit.convertfactornotzero',
        'core.error',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection): void => {
        this.terms = terms;
        this.grid.addColumnSelect(
          'productUnitId',
          terms['billing.product.productunit'],
          this.productUnits,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: true,
          }
        );
        this.grid.addColumnNumber(
          'convertFactor',
          terms['billing.product.productunit.convertfactor'],
          {
            editable: true,
            flex: 1,
            clearZero: true,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deleteRow(row),
        });

        this.grid.setNbrOfRowsToShow(5, 5);
        this.grid.context.suppressGridMenu = true;
        this.exportFilenameKey.set('billing.product.productunit');
        super.finalizeInitGrid({ hidden: true });
        this.grid.updateGridHeightBasedOnNbrOfRows();
      });
  }

  private cellValueChanged({
    newValue,
    oldValue,
    api,
    colDef,
    data,
  }: CellValueChangedEvent): void {
    if (newValue === oldValue) return;

    const rowData = data as ProductUnitConvertGridDTO;
    if (colDef.field === 'convertFactor' && newValue <= 0) {
      this.messageBox
        .error(
          this.terms['core.error'],
          this.terms['billing.product.productunit.convertfactornotzero']
        )
        .afterClosed()
        .subscribe(() => {
          rowData.convertFactor = oldValue;
          api.refreshCells();
        });

      return;
    }

    if (rowData.convertFactor) {
      rowData.isModified = true;
      this.form().markAsDirty();
    }
  }

  private addRow(): void {
    const row = new ProductUnitConvertDTO();
    row.productId = this.productId();
    this.productUnitConverts.update(
      (rows: ProductUnitConvertDTO[]): ProductUnitConvertDTO[] => {
        return [...rows, row];
      }
    );
    this.focusFirstCell();
  }

  private deleteRow(row: ProductUnitConvertGridDTO): void {
    if (row) {
      row.isDeleted = true;
      row.isModified = true;
      this.productUnitConverts.update((rows: ProductUnitConvertDTO[]) => {
        rows = [
          ...rows.filter(
            r => (r as ProductUnitConvertGridDTO).AG_NODE_ID !== row.AG_NODE_ID
          ),
          row,
        ];
        return rows;
      });
      this.form().markAsDirty();
    }
  }

  private focusFirstCell(): void {
    setTimeout((): void => {
      const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
      this.grid?.api.setFocusedCell(lastRowIdx, 'productUnitId');
      this.grid?.api.startEditingCell({
        rowIndex: lastRowIdx,
        colKey: 'productUnitId',
      });
    }, 100);
  }

  ngOnDestroy(): void {
    this.setRowDataEff?.destroy();
    this.productUnitsEff?.destroy();
  }
}
