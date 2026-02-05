import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EmployeePositionForm } from '../../models/employee-position-form.model';
import { EmployeePositionEditComponent } from '../employee-position-edit/employee-position-edit.component';
import { EmployeePositionGridComponent } from '../employee-position-grid/employee-position-grid.component';
import { EmployeeSystemPositionGridComponent } from '../employee-system-position-grid/employee-system-position-grid.component';

@Component({
  selector: 'soe-employee-position',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class EmployeePositionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmployeePositionGridComponent,
      editComponent: EmployeePositionEditComponent,
      FormClass: EmployeePositionForm,
      gridTabLabel: 'time.employee.position.positions',
      editTabLabel: 'time.employee.position.position',
      createTabLabel: 'time.employee.position.new_position',
    },
    {
      gridComponent: EmployeeSystemPositionGridComponent,
      gridTabLabel: 'time.employee.position.syspositions',
      hideForCreateTabMenu: true,
    },
  ];
}
