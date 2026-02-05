import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { IExportGridDTO } from '@shared/models/generated-interfaces/ExportDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ExportService } from '../../services/export.service';
import { UrlHelperService } from '@shared/services/url-params.service';

@Component({
  selector: 'soe-export-grid',
  templateUrl: './export-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExportGridComponent extends GridBaseDirective<
  IExportGridDTO,
  ExportService
> {
  service = inject(ExportService);
  urlHelper = inject(UrlHelperService);

  constructor(
    private translationService: TranslateService,
    public flowHandler: FlowHandlerService
  ) {
    super();

    this.startFlow(Feature.Time_Export_XEConnect, 'Shared.Export.Export');
  }

  override onGridReadyToDefine(grid: GridComponent<IExportGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translationService
      .get(['common.name', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 100 });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number,
    additionalProps?: { module: SoeModule }
  ): Observable<IExportGridDTO[]> {
    return super.loadData(id, { module: this.urlHelper.module });
  }
}
