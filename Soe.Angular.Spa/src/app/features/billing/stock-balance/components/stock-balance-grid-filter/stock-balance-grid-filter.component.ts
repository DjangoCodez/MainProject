import { Component, EventEmitter, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { StockBalanceFilterForm } from '../../models/stock-balance-grid-filter-form';

@Component({
    selector: 'soe-stock-balance-grid-filter',
    templateUrl: './stock-balance-grid-filter.component.html',
    standalone: false
})
export class StockBalanceGridFilterComponent {
  @Output() filter = new EventEmitter<boolean>();
  validationHandler = inject(ValidationHandler);
  form: StockBalanceFilterForm = new StockBalanceFilterForm({
    validationHandler: this.validationHandler,
    element: false,
  });

  onFilter(): void {
    const showInactives = this.form.value.showInactive as boolean;
    this.filter.emit(showInactives);
  }
}
