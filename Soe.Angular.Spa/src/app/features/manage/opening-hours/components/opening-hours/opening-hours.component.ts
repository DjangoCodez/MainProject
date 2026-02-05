import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { OpeningHoursForm } from '../../models/opening-hours-form.model';
import { OpeningHoursEditComponent } from '../opening-hours-edit/opening-hours-edit.component';
import { OpeningHoursGridComponent } from '../opening-hours-grid/opening-hours-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class OpeningHoursComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: OpeningHoursGridComponent,
      editComponent: OpeningHoursEditComponent,
      FormClass: OpeningHoursForm,
      gridTabLabel: 'manage.registry.openinghours.openinghours',
      editTabLabel: 'manage.registry.openinghours.openinghours',
      createTabLabel: 'manage.registry.openinghours.new_openinghours',
    },
  ];
}
