import { Component } from '@angular/core';
import { ProjectTimeReportGridComponent } from '../project-time-report-grid/project-time-report-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProjectExpensesGridComponent } from '../project-expenses/project-expenses-grid/project-expenses-grid.component';
import { ProjectWeekReportGridComponent } from '../project-week-report/project-week-report-grid/project-week-report-grid.component';

@Component({
  selector: 'soe-project-time-report',
  templateUrl: './project-time-report.component.html',
  standalone: false,
})
export class ProjectTimeReportComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProjectTimeReportGridComponent,
      gridTabLabel: 'billing.project.timesheet.timesheet',
    },
    {
      gridComponent: ProjectWeekReportGridComponent,
      gridTabLabel: 'billing.project.timesheet.weekreport',
    },
    {
      gridComponent: ProjectExpensesGridComponent,
      gridTabLabel: 'billing.order.expenses',
    },
  ];
}
