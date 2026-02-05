import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import {
  ISaftGridSearch,
  SaftGridSearchFormDTO,
} from '../../models/SaftGridSearchDTO.model';
import { SaftGridSearchForm } from '../../models/SaftGridSearchForm.model';

@Component({
    selector: 'soe-saft-grid-search',
    templateUrl: './saft-grid-search.component.html',
    standalone: false
})
export class SaftGridSearchComponent {
  @Output() searchClick = new EventEmitter<ISaftGridSearch>();
  @Input() fromDate!: Date;
  @Input() toDate!: Date;

  validationHander = inject(ValidationHandler);

  formSearch = new SaftGridSearchForm({
    validationHandler: this.validationHander,
    element: new SaftGridSearchFormDTO(this.fromDate, this.toDate),
  });

  filterByDates() {
    this.searchClick.emit(
      new SaftGridSearchFormDTO(this.fromDate, this.toDate)
    );
  }

  updateFromDate(value?: Date) {
    if (!value) return;
    this.fromDate = value;
  }

  updateToDate(value?: Date) {
    if (!value) return;
    this.toDate = value;
  }
}
