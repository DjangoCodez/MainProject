import { Component, inject, OnInit } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ITimeCodeRankingGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeCodeRankingService } from '../../services/time-code-ranking';

@Component({
  selector: 'soe-time-code-ranking-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeCodeRankingGridComponent
  extends GridBaseDirective<
    ITimeCodeRankingGroupGridDTO,
    TimeCodeRankingService
  >
  implements OnInit
{
  service = inject(TimeCodeRankingService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeCodeRanking,
      'Time.Schedule.TimeCodeRanking'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeCodeRankingGroupGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.dateto',
        'common.datefrom',
        'common.description',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnDate('startDate', terms['common.datefrom'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnDate('stopDate', terms['common.dateto'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 50,
          enableHiding: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
          enableHiding: true,
        });
        super.finalizeInitGrid();
      });
  }
}
