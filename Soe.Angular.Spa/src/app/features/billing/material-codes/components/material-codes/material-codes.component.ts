import { Component } from '@angular/core';
import { TimeCodeMaterialsForm } from '../../models/material-codes-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { MaterialCodesGridComponent } from '../material-codes-grid/material-codes-grid.component';
import { MaterialCodesEditComponent } from '../material-codes-edit/material-codes-edit.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class MaterialCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: MaterialCodesGridComponent,
      editComponent: MaterialCodesEditComponent,
      FormClass: TimeCodeMaterialsForm,
      gridTabLabel: 'time.time.timecodematerials.timecodematerials',
      editTabLabel: ' ',
      createTabLabel: 'time.time.timecodematerials.new',
      exportFilenameKey: 'time.time.timecodematerials.timecodematerials',
    },
  ];
}
