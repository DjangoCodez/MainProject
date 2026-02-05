import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EmployeeCsrExportForm } from '../../models/employee-csr-export-form.model';
import { EmployeeCsrExportGridComponent } from '../employee-csr-export-grid/employee-csr-export-grid.component';

@Component({
  standalone: false,
  templateUrl: './employee-csr-export.component.html',
})
export class EmployeeCsrExportComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmployeeCsrExportGridComponent,
      FormClass: EmployeeCsrExportForm,
      gridTabLabel: 'time.employee.csr.exports',
    },
  ];
}
