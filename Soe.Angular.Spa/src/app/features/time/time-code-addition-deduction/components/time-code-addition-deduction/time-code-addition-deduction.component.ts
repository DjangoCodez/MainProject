import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeCodeAdditionDeductionGridComponent } from '../time-code-addition-deduction-grid/time-code-addition-deduction-grid.component';
import { TimeCodeAdditionDeductionEditComponent } from '../time-code-addition-deduction-edit/time-code-addition-deduction-edit.component';
import { TimeCodeAdditionDeductionForm } from '../../models/time-code-addition-deduction-form.model';

@Component({
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
})
export class TimeCodeAdditionDeductionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeCodeAdditionDeductionGridComponent,
      editComponent: TimeCodeAdditionDeductionEditComponent,
      FormClass: TimeCodeAdditionDeductionForm,
      gridTabLabel:
        'time.time.timecodeadditiondeductions.timecodeadditiondeductions',
      editTabLabel:
        'time.time.timecodeadditiondeductions.timecodeadditiondeduction',
      createTabLabel: 'time.time.timecodeadditiondeductions.new',
    },
  ];
}
