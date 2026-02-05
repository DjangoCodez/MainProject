import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IHolidayGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { HolidaysService } from '../../services/holidays.service';

@Component({
  selector: 'soe-holidays-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HolidaysGridComponent
  extends GridBaseDirective<IHolidayGridDTO, HolidaysService>
  implements OnInit
{
  service = inject(HolidaysService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_Holidays,
      'Time.Schedule.Holidays'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IHolidayGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'time.schedule.daytype.date',
        'time.schedule.daytype.daytype',
        'common.description',
        'time.schedule.daytype.sysholidaytype',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
          enableHiding: false,
        });
        this.grid.addColumnDate('date', terms['time.schedule.daytype.date'], {
          flex: 10,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'dayTypeName',
          terms['time.schedule.daytype.daytype'],
          { flex: 20, enableHiding: true }
        );
        this.grid.addColumnText(
          'sysHolidayTypeName',
          terms['time.schedule.daytype.sysholidaytype'],
          { flex: 20, enableHiding: true }
        );

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
