import { Component } from '@angular/core';
import { ExtraFieldForm } from '../../models/extra-fields-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ExtraFieldsEditComponent } from '../extra-fields-edit/extra-fields-edit.component';
import { ExtraFieldsGridComponent } from '../extra-fields-grid/extra-fields-grid.component';
import { ExtraFieldsUrlParamsService } from '../../services/extra-fields-url.service';

@Component({
  selector: 'soe-extra-fields',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
  providers: [ExtraFieldsUrlParamsService],
})
export class ExtraFieldsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ExtraFieldsGridComponent,
      editComponent: ExtraFieldsEditComponent,
      FormClass: ExtraFieldForm,
      gridTabLabel: 'common.extrafields.extrafields',
      createTabLabel: 'common.extrafields.createextrafield',
      editTabLabel: 'common.extrafields.editextrafield',
    },
  ];
}
