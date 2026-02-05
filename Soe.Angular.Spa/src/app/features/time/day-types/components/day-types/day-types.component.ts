import { Component } from '@angular/core';
import { DayTypesForm } from '../../models/day-types-form.model';
import { DayTypesGridComponent } from '../day-types-grid/day-types-grid.component';
import { DayTypesEditComponent } from '../day-types-edit/day-types-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class DayTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DayTypesGridComponent,
      editComponent: DayTypesEditComponent,
      FormClass: DayTypesForm,
      gridTabLabel: 'time.schedule.daytype.daytypes',
      editTabLabel: 'time.schedule.daytype.daytype',
      createTabLabel: 'time.schedule.daytype.new',
      passGridDataOnAdd: true,
      additionalGridProps: { test: 'HE test' },
    },
  ];
}
