import { Component, inject } from '@angular/core';
import { ProjectService } from '../../services/project.service';
import { ProjectGridComponent } from '../project-grid/project-grid.component';
import { ProjectEditComponent } from '../project-edit/project-edit.component';
import { ProjectForm } from '../../models/project-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProjectUrlParamsService } from '../../services/project-url.service';
import { TermGroup_ProjectStatus } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-project',
  // templateUrl: '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  templateUrl: './project.component.html',
  standalone: false,
  providers: [ProjectUrlParamsService],
})
export class ProjectComponent {
  service = inject(ProjectService);
  config: MultiTabConfig[] = [
    {
      gridComponent: ProjectGridComponent,
      editComponent: ProjectEditComponent,
      FormClass: ProjectForm,
      gridTabLabel: 'billing.projects.list.projects',
      editTabLabel: 'billing.projects.list.project',
      createTabLabel: 'billing.projects.list.new_project',
      additionalGridProps: {
        projectStatuses: [TermGroup_ProjectStatus.Active],
        onlyMine: false,
      },
    },
  ];
}
