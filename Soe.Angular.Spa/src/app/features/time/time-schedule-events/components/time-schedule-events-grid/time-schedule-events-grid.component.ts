import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ITimeScheduleEventGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeScheduleEventsService } from '../../services/time-schedule-events.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
@Component({
  selector: 'soe-time-schedule-events-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleEventsGridComponent
  extends GridBaseDirective<
    ITimeScheduleEventGridDTO,
    TimeScheduleEventsService
  >
  implements OnInit
{
  service = inject(TimeScheduleEventsService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Schedule_SchedulePlanning_SalesCalender,
      'Time.Schedule.TimeScheduleEvents'
    );
  }
  override onGridReadyToDefine(grid: GridComponent<ITimeScheduleEventGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.date',
        'time.schedule.timescheduleevent.choosegroups',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });

        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
          enableHiding: true,
        });

        this.grid.addColumnText(
          'recipientGroupNames',
          terms['time.schedule.timescheduleevent.choosegroups'],
          {
            flex: 25,
            tooltipField: 'recipientGroupNames',
          }
        );

        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 15,
          enableHiding: true,
        });

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }
}
