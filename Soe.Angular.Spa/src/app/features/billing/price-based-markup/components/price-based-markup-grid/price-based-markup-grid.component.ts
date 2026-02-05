import { Component, OnInit, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { PriceBasedMarkupService } from '../../services/price-based-markup.service';
import { Perform } from '@shared/util/perform.class';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take, tap } from 'rxjs';
import { PriceBasedMarkupDTO } from '../../models/price-based-markup.model';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-price-based-markup-grid',
  templateUrl: './price-based-markup-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PriceBasedMarkupGridComponent
  extends GridBaseDirective<PriceBasedMarkupDTO, PriceBasedMarkupService>
  implements OnInit
{
  service = inject(PriceBasedMarkupService);
  progressService = inject(ProgressService);
  idFieldName = 'priceBasedMarkupId';

  performAction = new Perform<BackendResponse>(this.progressService);
  performLoad = new Perform<any[]>(this.progressService);

  pricelistDict: SmallGenericType[] = [];
  markupRows: PriceBasedMarkupDTO[] = [];

  constructor(public flowHandler: FlowHandlerService) {
    super();

    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_PriceBasedMarkup,
      'Billing.Preferences.PriceBasedMarkup',
      {
        lookups: this.loadPriceList(),
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      saveOption: {
        onAction: () => this.save(),
      },
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('plus', {
          iconName: signal('plus'),
          caption: signal('billing.invoices.pricebasedmarkup.newmarkup'),
          tooltip: signal('billing.invoices.pricebasedmarkup.newmarkup'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  private loadPriceList(): Observable<SmallGenericType[]> {
    return this.performLoad.load$(
      this.service.getPriceList().pipe(
        tap(x => {
          this.pricelistDict = x;
        })
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<PriceBasedMarkupDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.invoices.markup.markup',
        'economy.supplier.invoice.matches.amountfrom',
        'economy.customer.invoice.matches.amountto',
        'billing.projects.list.pricelist',
        'billing.invoices.markup.markuppercent',
        'billing.invoices.pricebasedmarkup.newmarkup',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');

        this.grid.addColumnNumber(
          'minPrice',
          terms['economy.supplier.invoice.matches.amountfrom'],
          {
            flex: 1,
            enableHiding: false,
            editable: true,
          }
        );
        this.grid.addColumnNumber(
          'maxPrice',
          terms['economy.customer.invoice.matches.amountto'],
          {
            flex: 1,
            enableHiding: false,
            editable: true,
          }
        );

        this.grid.addColumnAutocomplete(
          'priceListTypeId',
          terms['billing.projects.list.pricelist'],
          {
            editable: true,
            limit: 7,
            flex: 1,
            source: () => this.pricelistDict,
            optionIdField: 'id',
            optionNameField: 'name',
            optionDisplayNameField: 'priceListName',
          }
        );

        this.grid.addColumnNumber(
          'markupPercent',
          terms['billing.invoices.markup.markuppercent'],
          {
            flex: 1,
            enableHiding: false,
            editable: true,
          }
        );
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });
        super.finalizeInitGrid();
      });
  }

  onCellValueChanged(row: CellValueChangedEvent) {
    this.rowIsModified(row.data);
  }

  addRow() {
    const row = new PriceBasedMarkupDTO();
    this.rowData.value.push(row);
    this.grid.setData(this.rowData.value);
    this.focusFirstCell();
    this.rowIsModified(row);
  }

  deleteRow(row: PriceBasedMarkupDTO) {
    if (this.markupRows) this.markupRows.push(row);

    const rows = this.rowData.value;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.grid.resetRows();
    }
    row.state = SoeEntityState.Deleted;
  }

  save(options?: ProgressOptions) {
    const markupRows = this.rowData.value;
    const rowsToSave: PriceBasedMarkupDTO[] = [];

    for (const row of markupRows) {
      if (row.isModified) {
        rowsToSave.push(row);
      }
    }

    //Delete list
    if (this.markupRows) {
      for (const row of this.markupRows) rowsToSave.push(row);
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(rowsToSave).pipe(
        tap(result => {
          if (result.success) this.loadData();
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  override loadData(
    id?: number | undefined
  ): Observable<PriceBasedMarkupDTO[]> {
    return this.performLoad.load$(
      this.service.getGrid().pipe(
        map(data => {
          this.markupRows = [];
          return data;
        })
      )
    );
  }

  private focusFirstCell(): void {
    const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
    this.grid?.api.setFocusedCell(lastRowIdx, 'minPrice');
    this.grid?.api.startEditingCell({
      rowIndex: lastRowIdx,
      colKey: 'minPrice',
    });
  }

  public rowIsModified(row: PriceBasedMarkupDTO, modified = true) {
    row.isModified = modified;
    this.grid.agGrid.api.refreshCells();
  }
}
