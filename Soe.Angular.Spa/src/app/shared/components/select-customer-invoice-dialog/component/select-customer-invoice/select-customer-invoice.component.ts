import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, debounceTime, Subject, take, tap } from 'rxjs';
import { ICustomerInvoiceSearchResultDTO } from '../../model/customer-invoice-search.model';

@Component({
  selector: 'soe-select-customer-invoice',
  templateUrl: './select-customer-invoice.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SelectCustomerInvoiceComponent
  extends GridBaseDirective<ICustomerInvoiceSearchResultDTO>
  implements OnInit, OnDestroy
{
  @Input() rows!: BehaviorSubject<ICustomerInvoiceSearchResultDTO[]>;
  @Input() originType!: number;
  @Output() changeSelection = new EventEmitter<void>();
  @Output() changeFilter = new EventEmitter<void>();
  @Output() rowDoubleClicked = new EventEmitter<void>();
  flowHandler = inject(FlowHandlerService);
  private rowDoubleClickedHandler!: (event: any) => void;
  private isDestroyed = false;
  filterDebounce = new Subject<string>();

  ngOnInit(): void {
    this.flowHandler.execute({
      skipInitialLoad: true,
      setupGrid: this.setupGrid.bind(this),
    });

    this.filterDebounce
      .pipe(
        debounceTime(500),
        tap((value: any) => this.changeFilter.emit(value))
      )
      .subscribe();

    this.rows.subscribe(rows => {
      if (rows && rows.length > 0) {
        this.setFirstRowSelected();
      }
    });
  }

  ngOnDestroy(): void {
    this.grid.onRowDoubleClicked = () => {};
    this.filterDebounce.complete();
    this.isDestroyed = true;
  }

  setupGrid(grid: GridComponent<ICustomerInvoiceSearchResultDTO>) {
    if (this.isDestroyed) return;
    super.setupGrid(grid, 'common.dialogs.searchcustomerinvoice');
    this.translate
      .get([
        'common.number',
        'billing.invoices.externalinvoicenr',
        'billing.import.edi.customernr',
        'common.customer.customer.customername',
        'common.customer.invoices.internaltext',
        'common.customer.invoices.projectnr',
        'common.report.selection.projectname',
        'common.customer.invoices.duedate',
        'common.customer.invoices.payableamount',
        'common.currency',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        //TODO: Select 1st row by default
        this.grid.enableRowSelection(undefined, true);
        this.grid.context.suppressGridMenu = true;

        this.grid.addColumnText('number', terms['common.number'], {
          flex: 1,
        });

        if (this.originType === SoeOriginType.CustomerInvoice) {
          this.grid.addColumnText(
            'externalNr',
            terms['billing.invoices.externalinvoicenr'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'customerNr',
            terms['billing.import.edi.customernr'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'customerName',
            terms['common.customer.customer.customername'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnDate(
            'dueDate',
            terms['common.customer.invoices.duedate'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnNumber(
            'balance',
            terms['common.customer.invoices.payableamount'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText('currencyCode', terms['common.currency'], {
            flex: 1,
          });
        } else {
          this.grid.addColumnText(
            'customerNr',
            terms['billing.import.edi.customernr'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'customerName',
            terms['common.customer.customer.customername'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'internalText',
            terms['common.customer.invoices.internaltext'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'projectNr',
            terms['common.customer.invoices.projectnr'],
            {
              flex: 1,
            }
          );
          this.grid.addColumnText(
            'projectName',
            terms['common.report.selection.projectname'],
            {
              flex: 1,
            }
          );
        }
        this.grid.setRowSelection('singleRow');
        this.rowDoubleClickedHandler = event => {
          this.rowDoubleClicked.emit(event.data);
        };
        this.grid.onRowDoubleClicked = this.rowDoubleClickedHandler;
        super.finalizeInitGrid();
      });
  }

  filterChange(value: any) {
    this.filterDebounce.next(value);
  }

  selectionChange(selectedRows: any) {
    this.changeSelection.emit(selectedRows);
  }

  setFirstRowSelected() {
    setTimeout(() => {
      const firstRow = this.grid.api.getDisplayedRowAtIndex(0);
      if (firstRow) {
        firstRow.setSelected(true);
      }
    }, 0);
  }
}
