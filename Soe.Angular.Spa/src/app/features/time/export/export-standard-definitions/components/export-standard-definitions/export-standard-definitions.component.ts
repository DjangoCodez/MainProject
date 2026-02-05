import { Component } from '@angular/core';
import { ExportStandardDefinitionForm } from '../../models/export-standard-definition-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ExportStandardDefinitionsEditComponent } from '../export-standard-definitions-edit/export-standard-definitions-edit.component';
import { ExportStandardDefinitionsGridComponent } from '../export-standard-definitions-grid/export-standard-definitions-grid.component';

@Component({
  templateUrl:
    '../../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ExportStandardDefinitionsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ExportStandardDefinitionsGridComponent,
      editComponent: ExportStandardDefinitionsEditComponent,
      FormClass: ExportStandardDefinitionForm,
      gridTabLabel: 'time.export.standarddefinitions',
      editTabLabel: 'time.export.standarddefinition',
      createTabLabel: 'time.export.standarddefinition.new',
    },
  ];
}
