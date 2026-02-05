import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { LoggedWarningsGridComponent } from '../logged-warnings-grid/logged-warnings-grid.component';

@Component({
  selector: 'soe-logged-warnings',
  templateUrl: './logged-warnings.component.html',
  standalone: false,
})
export class LoggedWarningsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: LoggedWarningsGridComponent,
      gridTabLabel: 'time.schedule.workrulebypass.warnings',
    },
  ];
}
