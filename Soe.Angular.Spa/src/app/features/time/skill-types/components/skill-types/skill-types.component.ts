import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SkillTypesGridComponent } from '../skill-types-grid/skill-types-grid.component';
import { SkillTypesEditComponent } from '../skill-types-edit/skill-types-edit.component';
import { SkillTypesForm } from '../../models/skill-types-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SkillTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SkillTypesGridComponent,
      editComponent: SkillTypesEditComponent,
      FormClass: SkillTypesForm,
      gridTabLabel: 'time.schedule.skilltype.skilltypes',
      editTabLabel: 'time.schedule.skilltype.skilltype',
      createTabLabel: 'time.schedule.skilltype.new',
    },
  ];
}
