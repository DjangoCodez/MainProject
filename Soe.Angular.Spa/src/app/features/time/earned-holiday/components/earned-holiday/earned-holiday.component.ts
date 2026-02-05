import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EarnedHolidayGridComponent } from '../earned-holiday-grid/earned-holiday-grid.component';
import { EarnedHolidayForm } from '../../models/earned-holiday-form.model';

@Component({
  templateUrl: 'earned-holiday.component.html',
  standalone: false,
})
export class EarnedHolidayComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EarnedHolidayGridComponent,
      FormClass: EarnedHolidayForm,
      gridTabLabel: 'time.time.timeearnedholiday.timeearnedholiday',
      hideForCreateTabMenu: true,
      createTabLabel: 'time.time.timeearnedholiday.timeearnedholiday',
    },
  ];
}
