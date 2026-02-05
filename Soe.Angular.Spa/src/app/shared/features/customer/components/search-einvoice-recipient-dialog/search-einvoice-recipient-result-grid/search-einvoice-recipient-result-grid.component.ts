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
import { EinvoiceRecipientLookupForm } from '@shared/features/customer/models/einvoice-recipient-lookup-form.model';
import { EInvoiceRecipientModelDTO } from '@shared/features/customer/models/search-einvoice-recipient-dialog.model';
import { ValidationHandler } from '@shared/handlers';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IGridFilterModified } from '@ui/grid/interfaces';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { debounce } from 'lodash';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-search-einvoice-recipient-result-grid',
  standalone: false,
  templateUrl: './search-einvoice-recipient-result-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class SearchEinvoiceRecipientResultGridComponent
  extends GridBaseDirective<EInvoiceRecipientModelDTO>
  implements OnInit, OnDestroy
{
  @Input() gridRows!: BehaviorSubject<EInvoiceRecipientModelDTO[]>;
  @Output() changeFilter = new EventEmitter<IGridFilterModified>();
  @Output() changeSelection = new EventEmitter<EInvoiceRecipientModelDTO>();
  validationHandler = inject(ValidationHandler);
  einvoiceLookupForm: EinvoiceRecipientLookupForm =
    new EinvoiceRecipientLookupForm({
      validationHandler: this.validationHandler,
      element: new EInvoiceRecipientModelDTO(),
    });
  private isDestroyed = false;

  ngOnInit(): void {
    super.ngOnInit();
    this.flowHandler.execute({
      skipInitialLoad: true,
      setupGrid: this.setupGrid.bind(this),
    });
  }

  ngOnDestroy(): void {
    this.debouncedFilterChange.cancel();
    this.isDestroyed = true;
  }

  setupGrid(grid: GridComponent<EInvoiceRecipientModelDTO>): void {
    if (this.isDestroyed) return;
    super.setupGrid(grid, 'common.dialogs.searchcustomer');

    this.translate
      .get(['common.name', 'common.orgnr', 'common.customer.customer.vatnr'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(undefined, true);

        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          minWidth: 200,
        });
        this.grid.addColumnText('orgNo', terms['common.orgnr'], {
          flex: 1,
          minWidth: 200,
        });
        this.grid.addColumnText(
          'vatNo',
          terms['common.customer.customer.vatnr'],
          { flex: 1, minWidth: 200 }
        );
        this.grid.addColumnText(
          'gln',
          terms['common.customer.customer.invoicedefaultgln'],
          { flex: 1, minWidth: 200 }
        );

        super.finalizeInitGrid();
      });
  }

  filterChange(value: IGridFilterModified) {
    this.debouncedFilterChange(value);
  }

  debouncedFilterChange = debounce((value: IGridFilterModified) => {
    this.changeFilter.emit(value);
  }, 1000);

  selectionChange(selectedRow: EInvoiceRecipientModelDTO | undefined) {
    if (selectedRow) {
      this.changeSelection.emit(selectedRow);
    } else {
      this.changeSelection.emit(undefined);
    }
  }
}
