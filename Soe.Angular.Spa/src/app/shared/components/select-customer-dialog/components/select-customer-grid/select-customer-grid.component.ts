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
import { ICustomerSearchModel } from '@shared/models/generated-interfaces/CoreModels';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { debounce } from 'lodash';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-select-customer-grid',
  templateUrl: './select-customer-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SelectCustomerGridComponent
  extends GridBaseDirective<ICustomerSearchModel>
  implements OnInit, OnDestroy
{
  @Input() rows!: BehaviorSubject<ICustomerSearchModel[]>;
  @Output() changeSelection = new EventEmitter<void>();
  @Output() changeFilter = new EventEmitter<void>();
  @Output() rowDoubleClicked = new EventEmitter<void>();

  flowHandler = inject(FlowHandlerService);
  private rowDoubleClickedHandler!: (event: any) => void;
  private isDestroyed = false;

  ngOnInit(): void {
    this.flowHandler.execute({
      skipInitialLoad: true,
      setupGrid: this.setupGrid.bind(this),
    });

    this.rows.subscribe(rows => {
      if (rows && rows.length > 0) {
        this.setFirstRowSelected();
      }
    });
  }

  ngOnDestroy(): void {
    this.grid.onRowDoubleClicked = () => {};
    this.debouncedFilterChange.cancel();
    this.isDestroyed = true;
  }

  setupGrid(grid: GridComponent<ICustomerSearchModel>) {
    if (this.isDestroyed) return;

    super.setupGrid(grid, 'common.dialogs.searchcustomer');
    this.translate
      .get([
        'common.number',
        'common.name',
        'common.contactaddresses.addressmenu.billing',
        'common.customer.invoices.deliveryaddress',
        'common.note',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(undefined, true);
        this.grid.context.suppressGridMenu = true;

        this.grid.addColumnText('customerNr', terms['common.number'], {
          flex: 1,
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText(
          'billingAddress',
          terms['common.contactaddresses.addressmenu.billing'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'deliveryAddress',
          terms['common.customer.invoices.deliveryaddress'],
          { flex: 1 }
        );
        this.grid.addColumnText('note', terms['common.note'], { flex: 1 });

        this.grid.setRowSelection('singleRow');

        this.rowDoubleClickedHandler = event => {
          this.rowDoubleClicked.emit(event.data);
        };

        this.grid.onRowDoubleClicked = this.rowDoubleClickedHandler;

        super.finalizeInitGrid();
      });
  }

  debouncedFilterChange = debounce((value: any) => {
    if (!this.isDestroyed) {
      this.changeFilter.emit(value);
    }
  }, 1000);

  filterChange(value: any) {
    this.debouncedFilterChange(value);
  }

  selectionChange(selectedRows: any) {
    if (!this.isDestroyed) this.changeSelection.emit(selectedRows);
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
