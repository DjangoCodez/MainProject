import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AdjustTimeStampsGridComponent } from '../adjust-time-stamps-grid/adjust-time-stamps-grid.component';
import { AdjustTimeStampsForm } from '../../models/adjust-time-stamps-form.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'soe-adjust-time-stamps',
  standalone: false,
  templateUrl: './adjust-time-stamps.component.html',
})
export class AdjustTimeStampsComponent {
  config: MultiTabConfig[] = [
    {
      FormClass: AdjustTimeStampsForm,
      gridComponent: AdjustTimeStampsGridComponent,
      gridTabLabel: 'time.time.adjusttimestamps.adjusttimestamps',
      hideForCreateTabMenu: true,
    },
  ];

  constructor(protected translate: TranslateService) {}
}
