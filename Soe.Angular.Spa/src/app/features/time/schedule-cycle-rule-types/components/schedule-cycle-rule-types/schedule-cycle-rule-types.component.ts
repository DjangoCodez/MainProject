import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ScheduleCycleRuleTypesGridComponent } from '../schedule-cycle-rule-types-grid/schedule-cycle-rule-types-grid.component';
import { ScheduleCycleRuleTypesEditComponent } from '../schedule-cycle-rule-types-edit/schedule-cycle-rule-types-edit.component';
import { ScheduleCycleRuleTypeForm } from '../../models/schedule-cycle-rule-type-form.model';

@Component({
  selector: 'soe-schedule-cycle-rule-types',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ScheduleCycleRuleTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ScheduleCycleRuleTypesGridComponent,
      editComponent: ScheduleCycleRuleTypesEditComponent,
      FormClass: ScheduleCycleRuleTypeForm,
      gridTabLabel:
        'time.schedule.schedulecycleruletype.schedulecycleruletypes',
      editTabLabel: 'time.schedule.schedulecycleruletype.schedulecycleruletype',
      createTabLabel: 'time.schedule.schedulecycleruletype.new',
    },
  ];
}
