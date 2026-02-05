import {
  Component,
  Input,
  OnDestroy,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeTimeCodeType,
  TermGroup_ExpenseType,
  TermGroup_InvoiceProductVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ITimeCodeInvoiceProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellKeyDownEvent,
  CellValueChangedEvent,
  RowDataUpdatedEvent,
} from 'ag-grid-community';
import {
  BehaviorSubject,
  Observable,
  Subject,
  take,
  takeUntil,
  tap,
} from 'rxjs';
import { TimeCodeMaterialsForm } from '../../models/material-codes-form.model';
import { TimeCodeInvoiceProductDTO } from '../../models/material-codes.model';
import { MaterialCodesService } from '../../services/material-codes.service';

type TimeCodeInvoiceProductGridDTO = AG_NODE<ITimeCodeInvoiceProductDTO>;

@Component({
  selector: 'soe-material-codes-edit-item-grid',
  templateUrl: './material-codes-edit-item-grid.component.html',
  styleUrls: ['./material-codes-edit-item-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MaterialCodesEditItemGridComponent
  extends GridBaseDirective<ITimeCodeInvoiceProductDTO>
  implements OnInit, OnDestroy
{
  @Input({ required: true }) form: TimeCodeMaterialsForm | undefined;
  @Input() expenseType: TermGroup_ExpenseType = TermGroup_ExpenseType.Expense;

  private _destroy$ = new Subject<void>();
  materialCodesService = inject(MaterialCodesService);
  progressService = inject(ProgressService);
  flowHandler = inject(FlowHandlerService);
  products: ISmallGenericType[] = [];
  rowData = new BehaviorSubject<ITimeCodeInvoiceProductDTO[]>([]);

  performLoadInvoiceProducts = new Perform<ISmallGenericType[]>(
    this.progressService
  );

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Stock, 'Time.Time.Timecode.Invoiceproduct', {
      skipInitialLoad: true,
      useLegacyToolbar: true,
      lookups: [this.loadProducts()],
    });

    this.form?.invoiceProducts.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(values => {
        if (!this.form?.invoiceProducts.dirty) {
          this.rowData.next(values);
        }
      });

    if (this.form?.invoiceProducts) {
      this.rowData.next(this.form?.invoiceProducts.value);
    }
  }

  override createLegacyGridToolbar(): void {
    console.log('what are u doink');
    super.createLegacyGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });

    this.toolbarUtils.toolbarGroups[0].buttons.push(
      this.toolbarUtils.createLegacyButton({
        icon: 'plus',
        title: 'core.newrow',
        label: 'core.newrow',
        onClick: () => this.addItems(),
        disabled: signal(false),
        hidden: signal(false),
      })
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeCodeInvoiceProductDTO>
  ) {
    grid.setNbrOfRowsToShow(4, 10);
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onRowDataUpdated: this.onRowDataUpdated.bind(this),
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'time.time.timecode.invoiceproduct',
        'time.time.timecode.factor',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'invoiceProductId',
          terms['time.time.timecode.invoiceproduct'],
          this.products || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 2,
            editable: true,
          }
        );
        this.grid.addColumnNumber(
          'factor',
          terms['time.time.timecode.factor'],
          {
            flex: 1,
            editable: true,
            alignLeft: true,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.delete(row);
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  loadProducts(): Observable<ISmallGenericType[]> {
    return this.performLoadInvoiceProducts
      .load$(
        this.materialCodesService.getInvoiceProducts(
          +this.form?.type.value === +SoeTimeCodeType.Material ||
            this.expenseType === TermGroup_ExpenseType.Expense
            ? TermGroup_InvoiceProductVatType.Service
            : TermGroup_InvoiceProductVatType.None,
          true
        )
      )
      .pipe(
        tap(p => {
          this.products = p;
        })
      );
  }

  onRowDataUpdated(event: RowDataUpdatedEvent): void {
    if (event.context.newRow && !event.api.isAnyFilterPresent()) {
      const index = event.api.getLastDisplayedRowIndex();
      event.api.setFocusedCell(index, 'invoiceProductId');
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'invoiceProductId',
      });
      event.context.newRow = false;
    }
  }

  onCellValueChanged(event: CellValueChangedEvent): void {
    if (
      (event.rowIndex || event.rowIndex === 0) &&
      event.newValue !== event.oldValue
    ) {
      this.form?.updateInvoiceProduct(event.rowIndex, event.data);
    }
  }

  onCellKeyDown(event: CellKeyDownEvent): void {
    if (event.rowIndex || event.rowIndex === 0) {
      this.form?.invoiceProducts.markAsDirty();
    }
  }

  addItems() {
    const row = new TimeCodeInvoiceProductDTO();
    row.timeCodeId = this.form?.value.timeCodeId;
    row.timeCodeInvoiceProductId = 0;
    row.invoiceProductId = 0;
    row.factor = 1.0;
    this.grid?.addRow(row);
    this.form?.addInvoiceProductRow(row);
    this.grid.options.context.newRow = true;
  }

  delete(row: TimeCodeInvoiceProductGridDTO): void {
    if (row.AG_NODE_ID) {
      this.form?.deleteInvoiceProduct(+row.AG_NODE_ID);
      this.grid.deleteRow(row);
    }
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
