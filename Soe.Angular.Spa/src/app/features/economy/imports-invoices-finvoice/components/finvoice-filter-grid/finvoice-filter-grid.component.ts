import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { FinvoiveFilterGridForm } from '../../models/finvoice-filter-grid-form.model';
import { ValidationHandler } from '@shared/handlers';
import { FinvoiceGridFilterDTO } from '../../models/imports-invoices-finvoice.model';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';
import { Perform } from '@shared/util/perform.class';

@Component({
    selector: 'soe-finvoice-filter-grid',
    templateUrl: './finvoice-filter-grid.component.html',
    styleUrls: ['./finvoice-filter-grid.component.scss'],
    standalone: false
})
export class FinvoiceFilterGridComponent implements OnInit {
  @Output() filter = new EventEmitter();
  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performLoadSelections = new Perform<SmallGenericType[]>(this.progressService);
  selections: SmallGenericType[] = [];

  form: FinvoiveFilterGridForm = new FinvoiveFilterGridForm({
    validationHandler: this.validationHandler,
    element: new FinvoiceGridFilterDTO(),
  });

  ngOnInit(): void {
    this.getDateSelectedItems().subscribe();
  }

  onChangeGridContoller(): void {
    this.filter.emit(this.form.value);
  }

  getDateSelectedItems() {
    return this.performLoadSelections.load$(
      this.coreService
        .getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true)
        .pipe(
          tap(data => {
            this.selections = data;
          })
        )
    );
  }
}
