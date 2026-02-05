import { Component } from '@angular/core';
import { StaffingNeedsRulesForm } from '../../models/staffing-needs-rules-form.model';
import { StaffingNeedsRulesGridComponent } from '../../components/staffing-needs-rules-grid/staffing-needs-rules-grid.component';
import { StaffingNeedsRulesEditComponent } from '../../components/staffing-needs-rules-edit/staffing-needs-rules-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'staffing-needs-rules',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class StaffingNeedsRulesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StaffingNeedsRulesGridComponent,
      editComponent: StaffingNeedsRulesEditComponent,
      FormClass: StaffingNeedsRulesForm,
      gridTabLabel: 'time.schedule.staffingneedsrule.staffingneedsrules',
      editTabLabel: 'time.schedule.staffingneedsrule.staffingneedsrule',
      createTabLabel: 'time.schedule.staffingneedsrule.new',
    },
  ];
}
