import { Component, EventEmitter, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { ImportPaymentsFilterForm } from '../../models/import-payments-filter-form.model';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { of } from 'rxjs';
import { DefaultDurationSelection } from '../../models/import-payments.model';

@Component({
    selector: 'soe-import-payments-grid-filter',
    templateUrl: './import-payments-grid-filter.component.html',
    styleUrls: ['./import-payments-grid-filter.component.scss'],
    providers: [FlowHandlerService],
    standalone: false
})
export class ImportPaymentsGridFilterComponent {
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  performLoadSelectionTypes = new Perform<any>(this.progressService);
  durationSelection = DefaultDurationSelection.All;

  @Output() filterChange = new EventEmitter<number>();
  @Output() filterReady = new EventEmitter<number>();

  allItemsSelectionDict: SmallGenericType[] = [];

  formFilter: ImportPaymentsFilterForm = new ImportPaymentsFilterForm({
    validationHandler: this.validationHandler,
    element: this.durationSelection,
  });

  constructor(public handler: FlowHandlerService) {
    this.handler.execute({
      lookups: [this.loadSelectionTypes()],
      onFinished: this.onFinished.bind(this),
    });
  }

  onFinished() {
    const filterDto = this.formFilter.durationSelection.value as number;
    this.filterReady.emit(filterDto);
  }

  durationSelectionChanged() {
    this.emitFilterOnChange();
  }

  emitFilterOnChange() {
    const filterDto = this.formFilter.durationSelection.value as number;
    this.filterChange.emit(filterDto);
  }

  loadSelectionTypes() {
    return of(
      this.performLoadSelectionTypes.load(
        this.coreService.getTermGroupContent(
          TermGroup.ChangeStatusGridAllItemsSelection,
          false,
          true,
          true
        ),
        { showDialog: false }
      )
    );
  }
}
