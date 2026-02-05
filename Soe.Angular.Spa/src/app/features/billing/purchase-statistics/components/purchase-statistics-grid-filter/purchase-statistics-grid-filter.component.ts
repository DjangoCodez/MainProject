import { Component, EventEmitter, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseStatisticsFilterForm } from '../../models/purchase-statistics-filter-form.model';
import { PurchaseStatisticsFilterDTO } from '../../models/purchase-statistics.model';

@Component({
    selector: 'soe-purchase-statistics-grid-filter',
    templateUrl: './purchase-statistics-grid-filter.component.html',
    standalone: false
})
export class PurchaseStatisticsGridFilterComponent {
  @Output() searchClick = new EventEmitter<PurchaseStatisticsFilterDTO>();

  validationHandler = inject(ValidationHandler);

  formFilter: PurchaseStatisticsFilterForm = new PurchaseStatisticsFilterForm({
    validationHandler: this.validationHandler,
    element: new PurchaseStatisticsFilterDTO(),
  });

  search() {
    const searchDto = this.formFilter.value as PurchaseStatisticsFilterDTO;
    this.searchClick.emit({
      fromDate: searchDto.fromDate,
      toDate: new Date(
        searchDto.toDate.getFullYear(),
        searchDto.toDate.getMonth(),
        searchDto.toDate.getDate() + 1
      ),
    });
  }
}
