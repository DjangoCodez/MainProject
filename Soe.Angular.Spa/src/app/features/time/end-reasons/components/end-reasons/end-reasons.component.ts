import { Component } from '@angular/core';
import { EndReasonsForm } from '../../models/end-reasons-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EndReasonsEditComponent } from '../end-reasons-edit/end-reasons-edit.component';
import { EndReasonsGridComponent } from '../end-reasons-grid/end-reasons-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class EndReasonsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EndReasonsGridComponent,
      editComponent: EndReasonsEditComponent,
      FormClass: EndReasonsForm,
      gridTabLabel: 'time.employee.endreason.endreasons',
      editTabLabel: 'time.employee.endreason.endreason',
      createTabLabel: 'time.employee.endreason.new_endreason',
    },
  ];
}
