import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { HalfdaysGridComponent } from '../halfdays-grid/halfdays-grid.component';
import { HalfdaysEditComponent } from '../halfdays-edit/halfdays-edit.component';
import { HalfdayForm } from '../../models/halfday-form.model';

@Component({
  selector: 'soe-halfdays',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class HalfdaysComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: HalfdaysGridComponent,
      editComponent: HalfdaysEditComponent,
      FormClass: HalfdayForm,
      gridTabLabel: 'time.schedule.daytype.halfdays',
      editTabLabel: 'time.schedule.daytype.halfday',
      createTabLabel: 'time.schedule.daytype.newhalfday',
    },
  ];
}
