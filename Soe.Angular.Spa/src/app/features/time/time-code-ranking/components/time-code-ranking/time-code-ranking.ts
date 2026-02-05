import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeCodeRankingForm } from '../../models/time-code-ranking-form.model';
import { TimeCodeRankingEdit } from '../time-code-ranking-edit/time-code-ranking-edit';
import { TimeCodeRankingGridComponent } from '../time-code-ranking-grid/time-code-ranking-grid.component';

@Component({
  selector: 'soe-time-code-ranking',
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
})
export class TimeCodeRankingComponent {
  config: MultiTabConfig[] = [
    {
      FormClass: TimeCodeRankingForm,
      gridComponent: TimeCodeRankingGridComponent,
      gridTabLabel: 'time.time.timecode.timecoderanking',
      editComponent: TimeCodeRankingEdit,
      editTabLabel: 'time.time.timecode.timecoderanking',
      createTabLabel: 'time.time.timecode.timecoderanking.new',
    },
  ];

  constructor(protected translate: TranslateService) {}
}
