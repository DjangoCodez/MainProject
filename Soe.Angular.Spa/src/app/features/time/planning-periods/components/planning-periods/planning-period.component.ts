import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { DistributionRuleHeadsForm } from '../../models/distribution-rule-heads-form.model';
import { PlanningPeriodsForm } from '../../models/planning-periods-form.model';
import { DistributionRulesEditComponent } from '../distribution-rules-edit/distribution-rules-edit.component';
import { DistributionRulesGridComponent } from '../distribution-rules-grid/distribution-rules-grid.component';
import { PlanningPeriodsEditComponent } from '../planning-periods-edit/planning-periods-edit.component';
import { PlanningPeriodsGridComponent } from '../planning-periods-grid/planning-periods-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PlanningPeriodsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PlanningPeriodsGridComponent,
      editComponent: PlanningPeriodsEditComponent,
      FormClass: PlanningPeriodsForm,
      gridTabLabel: 'time.time.planningperiod.planningperiods',
      editTabLabel: 'time.time.planningperiod.planningperiod',
      createTabLabel: 'time.time.planningperiod.new',
    },
    {
      gridComponent: DistributionRulesGridComponent,
      editComponent: DistributionRulesEditComponent,
      FormClass: DistributionRuleHeadsForm,
      gridTabLabel:
        'time.time.planningperiod.planningperiods.distribution.rules',
      editTabLabel:
        'time.time.planningperiod.planningperiods.distribution.rule',
      createTabLabel:
        'time.time.planningperiod.planningperiods.distribution.rule.new',
    },
  ];
}
