import { Component } from '@angular/core';
import { HolidayForm } from '../../models/holidays-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { HolidaysGridComponent } from '../holidays-grid/holidays-grid.component';
import { HolidaysEditComponent } from '../holidays-edit/holidays-edit.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class HolidaysComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: HolidaysGridComponent,
      editComponent: HolidaysEditComponent,
      FormClass: HolidayForm,
      gridTabLabel: 'time.schedule.daytype.holidays',
      editTabLabel: 'time.schedule.daytype.holiday',
      createTabLabel: 'time.schedule.daytype.newholiday',
    },
  ];
}
