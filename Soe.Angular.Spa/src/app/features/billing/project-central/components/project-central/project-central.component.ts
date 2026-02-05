import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProjectCentralMainComponent } from '../project-central-main/project-central-main.component';
import { ProjectCentralForm } from '../../models/project-central-form.model';
import { ProjectCentralSupplierInvoicesGridComponent } from '../project-central-supplier-invoices-grid/project-central-supplier-invoices-grid.component';
import { ProjectCentralDataService } from '../../services/project-central-data.service';
import { ProjectCentralProductRowsComponent } from '../project-central-product-rows/project-central-product-rows.component';
import { ProjectTimeReportGridComponent } from '@features/billing/project-time-report/components/project-time-report-grid/project-time-report-grid.component';
import { TimeProjectContainer } from '@shared/util/Enumerations';

@Component({
  selector: 'soe-project-central',
  templateUrl: './project-central.component.html',
  standalone: false,
})
export class ProjectCentralComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProjectCentralMainComponent,
      FormClass: ProjectCentralForm,
      gridTabLabel: 'billing.projects.list.projects',
    },
    {
      key: 'project-central-supplierinvoices',
      gridComponent: ProjectCentralSupplierInvoicesGridComponent,
      gridTabLabel: 'billing.project.central.supplierinvoices',
      exportFilenameKey: 'billing.project.central.supplierinvoices',
      disabled: true,
    },
    {
      key: 'project-central-timesheet',
      gridComponent: ProjectTimeReportGridComponent,
      gridTabLabel: 'billing.project.timesheet.timesheet',
      exportFilenameKey: 'billing.project.timesheet.timesheet',
      additionalGridProps: {
        projectContainer: TimeProjectContainer.ProjectCentral,
      },
      disabled: true,
    },
    {
      key: 'project-central-productrows',
      gridComponent: ProjectCentralProductRowsComponent,
      gridTabLabel: 'billing.order.productrows',
      exportFilenameKey: 'billing.project.central.productrows',
      disabled: true,
    },
  ];
}
