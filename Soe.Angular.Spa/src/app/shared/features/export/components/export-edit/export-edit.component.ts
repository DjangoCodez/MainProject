import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ExportDTO } from '../../models/export.model';
import { ExportService } from '../../services/export.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-export-edit',
  templateUrl: './export-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExportEditComponent
  extends EditBaseDirective<ExportDTO, ExportService>
  implements OnInit
{
  service = inject(ExportService);
  exportDefinitions: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Export_XEConnect, {
      lookups: [this.loadExportDefinitions()],
    });
  }

  private loadExportDefinitions(): Observable<SmallGenericType[]> {
    return this.service
      .getDefinitions(true)
      .pipe(tap(x => (this.exportDefinitions = x)));
  }
}
