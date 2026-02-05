import { Component } from '@angular/core';
import { LeisureCodesGridComponent } from '../leisure-codes-grid/leisure-codes-grid.component';
import { LeisureCodesEditComponent } from '../leisure-codes-edit/leisure-codes-edit.component';
import { LeisureCodesForm } from '../../models/leisure-codes-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class LeisureCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: LeisureCodesGridComponent,
      editComponent: LeisureCodesEditComponent,
      FormClass: LeisureCodesForm,
      gridTabLabel: 'time.schedule.leisurecode.leisurecodes',
      editTabLabel: 'time.schedule.leisurecode.leisurecode',
      createTabLabel: 'time.schedule.leisurecode.leisurecode.new',
    },
  ];
}
