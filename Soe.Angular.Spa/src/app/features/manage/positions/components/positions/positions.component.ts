import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { PositionsForm } from '../../models/positions-form.model';
import { PositionsEditComponent } from '../positions-edit/positions-edit.component';
import { PositionsGridComponent } from '../positions-grid/positions-grid.component';

@Component({
  selector: 'soe-positions',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PositionsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PositionsGridComponent,
      editComponent: PositionsEditComponent,
      FormClass: PositionsForm,
      gridTabLabel: 'manage.registry.sysposition.syspositions',
      editTabLabel: 'manage.registry.sysposition.sysposition',
      createTabLabel: 'manage.registry.sysposition.new',
    },
  ];
}
