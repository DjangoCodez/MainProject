import { Component, EventEmitter, Output, inject } from '@angular/core';
import { InventoryWriteoffsSearchForm } from '../../models/inventory-writeoffs-search-form.model';
import { TransferToAccountDistributionEntryDTO } from '../../models/inventory-writeoffs.model';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';

@Component({
  selector: 'soe-inventory-writeoffs-search',
  templateUrl: './inventory-writeoffs-search.component.html',
  standalone: false,
})
export class InventoryWriteoffsSearchComponent {
  @Output() onSearchClick = new EventEmitter<Date>();

  validationHandler = inject(ValidationHandler);

  formSearch: InventoryWriteoffsSearchForm = new InventoryWriteoffsSearchForm({
    validationHandler: this.validationHandler,
    element: new TransferToAccountDistributionEntryDTO(),
  });

  filterByDate(value?: Date): void {
    if (value) this.onSearchClick.emit(value);
  }
}
