import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { FollowupTypeForm } from '../../models/employee-followup-types-form.model';
import { EmployeeFollowupTypesEditComponent } from '../employee-followup-types-edit/employee-followup-types-edit.component';
import { EmployeeFollowupTypesGridComponent } from '../employee-followup-types-grid/employee-followup-types-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class EmployeeFollowupTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmployeeFollowupTypesGridComponent,
      editComponent: EmployeeFollowupTypesEditComponent,
      FormClass: FollowupTypeForm,
      gridTabLabel: 'time.employee.followuptype.followuptypes',
      editTabLabel: 'time.employee.followuptype.followuptype',
      createTabLabel: 'time.employee.followuptype.new',
    },
  ];
}
