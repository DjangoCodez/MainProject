import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ConnectImporterService } from '../../services/connect-importer.service';
import { ConnectImporterForm } from '../../models/connect-importer-form.model';
import { ImportBatchDTO } from '../../models/connect-importer.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of } from 'rxjs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-connect-importer-edit',
  templateUrl: './connect-importer-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ConnectImporterEditComponent
  extends EditBaseDirective<
    ImportBatchDTO,
    ConnectImporterService,
    ConnectImporterForm
  >
  implements OnInit
{
  useAccountDistribution = false;
  useAccountDimensions = false;
  importHeadType = 0;
  batchId = '';

  service = inject(ConnectImporterService);
  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Import_XEConnect);

    this.batchId = this.form?.batchId.value;
    this.importHeadType = this.form?.importHeadType.value;
  }

  override loadData(): Observable<void> {
    return of();
  }
}
