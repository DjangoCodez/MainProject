import { Component } from '@angular/core';
import { ImportConnectGridComponent } from '../import-connect-grid/import-connect-grid.component';
import { ImportConnectEditComponent } from '../import-connect-edit/import-connect-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ImportConnectFormModel } from '../../models/import-connect-form.model';

@Component({
  selector: 'soe-import-connect',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ImportConnectComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ImportConnectGridComponent,
      editComponent: ImportConnectEditComponent,
      FormClass: ImportConnectFormModel,
      exportFilenameKey: 'common.connect.imports',
      gridTabLabel: 'common.connect.imports',
      editTabLabel: 'common.connect.import',
      createTabLabel: 'common.connect.new_import',
    },
  ];
}
