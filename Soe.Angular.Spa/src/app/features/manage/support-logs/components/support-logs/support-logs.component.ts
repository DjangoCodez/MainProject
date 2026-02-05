import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ValidationHandler } from '@shared/handlers';
import { SoeLogType } from '@shared/models/generated-interfaces/Enumerations';
import { SupportLogsService } from '../../services/support-logs.service';
import { SupportLogsForm } from '../../models/support-logs-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupportLogsGridComponent } from '../support-logs-grid/support-logs-grid.component';
import { SupportLogsEditComponent } from '../support-logs-edit/support-logs-edit.component';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Component({
  selector: 'soe-support-logs',
  templateUrl: './support-logs.component.html',
  providers: [ValidationHandler],
  standalone: false,
})
export class SupportLogsComponent {
  service = inject(SupportLogsService);

  config: MultiTabConfig[] = [
    {
      gridComponent: SupportLogsGridComponent,
      editComponent: SupportLogsEditComponent,
      FormClass: SupportLogsForm,
      gridTabLabel: 'manage.support.logs',
      editTabLabel: 'manage.support.log',
      createTabLabel: 'manage.support.new',
    },
  ];

  constructor(private activatedRoute: ActivatedRoute) {
    SoeConfigUtil.supportLogType =
      this.activatedRoute.snapshot.queryParamMap.get(
        'type'
      ) as unknown as number as SoeLogType;
    this.updateTabLabel();
  }

  updateTabLabel(): void {
    this.config[0].gridTabLabel = this.service.getLabelTerm(
      SoeConfigUtil.supportLogType
    );
  }
}
