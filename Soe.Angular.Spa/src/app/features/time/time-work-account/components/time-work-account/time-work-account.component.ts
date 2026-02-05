import { Component } from '@angular/core';
import { TimeWorkAccountForm } from '../../models/time-work-account-form.model';
import { TimeWorkAccountGridComponent } from '../time-work-account-grid/time-work-account-grid.component';
import { TimeWorkAccountEditComponent } from '../time-work-account-edit/time-work-account-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeWorkAccountComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeWorkAccountGridComponent,
      editComponent: TimeWorkAccountEditComponent,
      FormClass: TimeWorkAccountForm,
      gridTabLabel: 'time.payroll.worktimeaccount.worktimeaccount',
      editTabLabel: 'time.payroll.worktimeaccount.worktimeaccount',
      createTabLabel: 'time.payroll.worktimeaccount.new',
    },
  ];
}
