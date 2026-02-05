import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ExportForm } from '../../models/export-form.model';
import { ExportEditComponent } from '../export-edit/export-edit.component';
import { ExportGridComponent } from '../export-grid/export-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ExportComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ExportGridComponent,
      editComponent: ExportEditComponent,
      FormClass: ExportForm,
      gridTabLabel: 'common.export.export.exports',
      editTabLabel: 'common.export.export.export',
      createTabLabel: 'common.export.export.new',
      exportFilenameKey: 'common.export.export.exports',
    },
  ];
}
