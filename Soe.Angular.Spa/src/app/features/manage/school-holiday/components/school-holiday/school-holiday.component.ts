import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SchoolHolidayForm } from '../../models/school-holiday-form.model';
import { SchoolHolidayEditComponent } from '../school-holiday-edit/school-holiday-edit.component';
import { SchoolHolidayGridComponent } from '../school-holiday-grid/school-holiday-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SchoolHolidayComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SchoolHolidayGridComponent,
      editComponent: SchoolHolidayEditComponent,
      FormClass: SchoolHolidayForm,
      gridTabLabel: 'manage.calendar.schoolholiday.schoolholiday',
      editTabLabel: 'manage.calendar.schoolholiday.schoolholiday',
      createTabLabel: 'manage.calendar.schoolholiday.new_schoolholiday',
    },
  ];
}
