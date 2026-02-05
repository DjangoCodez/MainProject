import { Component, EventEmitter, inject, Output } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ConnectImporterGridFilterDTO } from '../../../models/connect-importer.model';
import { ConnectImporterGridFilterForm } from '../../../models/connect-importer-grid-filter-form.model';
import { ValidationHandler } from '@shared/handlers';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';

@Component({
    selector: 'soe-connect-importer-grid-filter',
    templateUrl: './connect-importer-grid-filter.component.html',
    providers: [FlowHandlerService],
    standalone: false
})
export class ConnectImporterGridFilterComponent {
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  flowHandlerService = inject(FlowHandlerService);
  performLoad = new Perform<any>(this.progressService);

  iOImportHeadTypes: ISmallGenericType[] = [];
  dateSelections: ISmallGenericType[] = [];

  @Output() filterChange = new EventEmitter<ConnectImporterGridFilterDTO>();

  formFilter: ConnectImporterGridFilterForm = new ConnectImporterGridFilterForm(
    {
      validationHandler: this.validationHandler,
      element: new ConnectImporterGridFilterDTO(),
    }
  );

  constructor() {
    this.flowHandlerService.execute({
      permission: Feature.Economy_Import_XEConnect,
      lookups: [this.loadIOImportHeadTypes(), this.loadDateSelections()],
      onFinished: this.finished.bind(this),
    });
  }

  finished() {
    this.emitFilterOnChange();
  }

  onFilter() {
    this.emitFilterOnChange();
  }

  emitFilterOnChange() {
    const filterDto = this.formFilter.value as ConnectImporterGridFilterDTO;
    this.filterChange.emit(filterDto);
  }

  //#region Data Loading

  loadIOImportHeadTypes() {
    return this.performLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.IOImportHeadType, false, false)
        .pipe(
          tap(data => {
            this.iOImportHeadTypes = data;
          })
        )
    );
  }

  loadDateSelections() {
    return this.performLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true)
        .pipe(
          tap(data => {
            this.dateSelections = data;
          })
        )
    );
  }

  //#endregion
}
