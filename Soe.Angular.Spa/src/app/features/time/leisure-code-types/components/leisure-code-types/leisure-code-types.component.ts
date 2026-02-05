import { Component } from '@angular/core';
import { LeisureCodeTypesGridComponent } from '../leisure-code-types-grid/leisure-code-types-grid.component';
import { LeisureCodeTypesEditComponent } from '../leisure-code-types-edit/leisure-code-types-edit.component';
import { LeisureCodeTypesForm } from '../../models/leisure-code-types-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class LeisureCodeTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: LeisureCodeTypesGridComponent,
      editComponent: LeisureCodeTypesEditComponent,
      FormClass: LeisureCodeTypesForm,
      gridTabLabel: 'time.schedule.leisurecode.leisurecodetypes',
      editTabLabel: 'time.schedule.leisurecode.leisurecodetype',
      createTabLabel: 'time.schedule.leisurecode.leisurecodetype.new',
    },
  ];
}
