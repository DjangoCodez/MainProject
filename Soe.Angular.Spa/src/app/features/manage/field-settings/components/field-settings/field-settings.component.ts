import { Component, inject } from '@angular/core';
import { FieldSettingsService } from '../../services/field-settings.service';
import { FieldSettingsGridComponent } from '../field-settings-grid/field-settings-grid.component';
import { FieldSettingsEditComponent } from '../field-settings-edit/field-settings-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { FieldSettingsForm } from '../../models/field-settings-form.model';

@Component({
  templateUrl: './field-settings.component.html',
  standalone: false,
})
export class FieldSettingsComponent {
  service = inject(FieldSettingsService);
  config: MultiTabConfig[] = [
    {
      gridComponent: FieldSettingsGridComponent,
      editComponent: FieldSettingsEditComponent,
      FormClass: FieldSettingsForm,
      gridTabLabel: 'manage.preferences.fieldsettings.fieldsettings',
      editTabLabel: 'manage.preferences.fieldsettings.fieldsetting',
    },
  ];
}
