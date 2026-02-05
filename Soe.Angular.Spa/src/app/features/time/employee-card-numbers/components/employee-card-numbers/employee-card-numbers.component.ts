import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EmployeeCardNumberForm } from '../../models/employee-card-numbers-form.model';
import { EmployeeCardNumbersGridComponent } from '../employee-card-numbers-grid/employee-card-numbers-grid.component';

@Component({
  templateUrl: './employee-card-numbers.component.html',
  standalone: false,
})
export class EmployeeCardNumbersComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmployeeCardNumbersGridComponent,
      FormClass: EmployeeCardNumberForm,
      gridTabLabel: 'time.employee.cardnumber.cardnumbers',
    },
  ];
}
