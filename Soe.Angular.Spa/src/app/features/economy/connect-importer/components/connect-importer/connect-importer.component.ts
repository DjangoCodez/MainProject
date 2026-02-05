import { Component } from '@angular/core';
import { ConnectImporterGridComponent } from '../connect-importer-grid/connect-importer-grid.component';
import { ConnectImporterEditComponent } from '../connect-importer-edit/connect-importer-edit.component';
import { ConnectImporterForm } from '../../models/connect-importer-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: 'connect-importer.component.html',
  standalone: false,
})
export class ConnectImporterComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ConnectImporterGridComponent,
      editComponent: ConnectImporterEditComponent,
      FormClass: ConnectImporterForm,
      exportFilenameKey: 'common.connect.imports',
      gridTabLabel: 'economy.import.batches.batches',
      editTabLabel: 'economy.import.batches.batch',
      createTabLabel: 'economy.import.batches.new_batch',
    },
  ];
}
