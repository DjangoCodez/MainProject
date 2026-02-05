import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EmployeeGroupsGridComponent } from '../employee-groups-grid/employee-groups-grid.component';
import { EmployeeGroupsEditComponent } from '../employee-groups-edit/employee-groups-edit.component';
import { EmployeeGroupsForm } from '../../models/employee-groups-form.model';

@Component({
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
})
export class EmployeeGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmployeeGroupsGridComponent,
      editComponent: EmployeeGroupsEditComponent,
      FormClass: EmployeeGroupsForm,
      gridTabLabel: 'time.employee.employeegroup.employeegroups',
      editTabLabel: 'time.employee.employeegroup.employeegroup',
      createTabLabel: 'time.employee.employeegroup.newemployeegroup',
    },
  ];
}
