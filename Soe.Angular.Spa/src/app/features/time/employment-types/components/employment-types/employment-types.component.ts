import { Component } from '@angular/core';
import { EmploymentTypesGridComponent } from '../employment-types-grid/employment-types-grid.component';
import { EmploymentTypesEditComponent } from '../employment-types-edit/employment-types-edit.component';
import { EmploymentTypesForm } from '../../models/employment-types-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class EmploymentTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmploymentTypesGridComponent,
      editComponent: EmploymentTypesEditComponent,
      FormClass: EmploymentTypesForm,
      gridTabLabel: 'time.employee.employmenttype.employmenttypes',
      editTabLabel: 'time.employee.employmenttype.employmenttype',
      createTabLabel: 'time.employee.employmenttype.new_employmenttype',
    },
  ];
}
