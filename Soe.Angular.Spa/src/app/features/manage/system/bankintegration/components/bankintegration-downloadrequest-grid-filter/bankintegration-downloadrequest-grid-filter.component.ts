import { Component, inject, output } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { BankintegrationDownloadRequestGridFilterForm } from '../../models/bankintegration-downloadrequest-grid-filter-form';
import { SoeBankerRequestFilterDTO } from '../../../../models/bankintegration.model';

@Component({
  selector: 'soe-bankintegration-downloadrequest-grid-filter',
  templateUrl: './bankintegration-downloadrequest-grid-filter.component.html',
  standalone: false,
})
export class BankintegrationDownloadRequestGridFilterComponent {
  searchClick = output<SoeBankerRequestFilterDTO>();
  validationHandler = inject(ValidationHandler);
  form: BankintegrationDownloadRequestGridFilterForm =
    new BankintegrationDownloadRequestGridFilterForm({
      validationHandler: this.validationHandler,
      element: new SoeBankerRequestFilterDTO(),
    });

  search() {
    this.searchClick.emit({
      fromDate: this.form.fromDate.value,
      toDate: this.form.toDate.value,
      onlyError: this.form.value.onlyError as boolean,
      statusCodes: this.form.statusCodes,
    });
  }
}
