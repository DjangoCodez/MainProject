import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AnnualLeaveGroupsForm } from '../../models/annual-leave-groups-form.model';
import { AnnualLeaveGroupsEditComponent } from '../annual-leave-groups-edit/annual-leave-groups-edit.component';
import { AnnualLeaveGroupsGridComponent } from '../annual-leave-groups-grid/annual-leave-groups-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AnnualLeaveGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AnnualLeaveGroupsGridComponent,
      editComponent: AnnualLeaveGroupsEditComponent,
      FormClass: AnnualLeaveGroupsForm,
      gridTabLabel: 'time.employee.annualleavegroups',
      editTabLabel: 'time.employee.annualleavegroup',
      createTabLabel: 'time.employee.annualleavegroup.new',
      passGridDataOnAdd: true,
    },
  ];
}
