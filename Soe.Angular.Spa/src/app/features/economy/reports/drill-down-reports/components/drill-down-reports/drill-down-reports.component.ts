import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { DrillDownReportsGridComponent } from '../drill-down-reports-grid/drill-down-reports-grid.component';
import { DrillDownReportForm } from '../../models/drill-down-reports-form.model';

@Component({
  templateUrl: './drill-down-reports.component.html',
  standalone: false,
})
export class DrillDownReportsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DrillDownReportsGridComponent,
      FormClass: DrillDownReportForm,
      hideForCreateTabMenu: true,
      exportFilenameKey: 'common.report.report.report',
      gridTabLabel: 'common.report.report.report',
    },
  ];
}
