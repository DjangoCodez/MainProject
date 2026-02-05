import { Component } from '@angular/core';
import { AbsenceRequestsGridComponent } from '../absence-requests-grid/absence-requests-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AbsenceRequestsEditComponent } from '../absence-requests-edit/absence-requests-edit.component';
import { AbsenceRequestsForm } from '../../models/absence-requests-form.model';

@Component({
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
})
export class AbsenceRequestsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AbsenceRequestsGridComponent,
      editComponent: AbsenceRequestsEditComponent,
      FormClass: AbsenceRequestsForm,
      gridTabLabel: 'time.schedule.absencerequests.absencerequests.short',
      editTabLabel: 'time.schedule.absencerequests.absencerequest.short',
      createTabLabel: 'time.schedule.absencerequests.new.short',
    },
  ];
}
