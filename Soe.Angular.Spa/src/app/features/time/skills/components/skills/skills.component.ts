import { Component } from '@angular/core';
import { SkillsForm } from '../../models/skill-form.model';
import { SkillsGridComponent } from '../skills-grid/skills-grid.component';
import { SkillsEditComponent } from '../skills-edit/skills-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SkillsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SkillsGridComponent,
      editComponent: SkillsEditComponent,
      FormClass: SkillsForm,
      gridTabLabel: 'time.schedule.skill.skills',
      editTabLabel: 'time.schedule.skill.skill',
      createTabLabel: 'time.schedule.skill.new',
    },
  ];
}
