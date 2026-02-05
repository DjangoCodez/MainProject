import { Component } from '@angular/core';
import { PayrollLevelsGridComponent } from '../payroll-levels-grid/payroll-levels-grid.component';
import { PayrollLevelsEditComponent } from '../payroll-levels-edit/payroll-levels-edit.component';
import { PayrollLevelsForm } from '../../models/payroll-levels-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PayrollLevelsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PayrollLevelsGridComponent,
      editComponent: PayrollLevelsEditComponent,
      FormClass: PayrollLevelsForm,
      gridTabLabel: 'time.employee.payrolllevel.payrolllevels',
      editTabLabel: 'time.employee.payrolllevel.payrolllevel',
      createTabLabel: 'time.employee.payrolllevel.new',
    },
  ];
}
