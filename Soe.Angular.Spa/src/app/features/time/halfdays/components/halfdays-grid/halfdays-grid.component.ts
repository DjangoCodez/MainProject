import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeHalfdayGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { HalfdaysService } from '../../services/halfdays.service';

@Component({
  selector: 'soe-halfdays-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HalfdaysGridComponent
  extends GridBaseDirective<ITimeHalfdayGridDTO, HalfdaysService>
  implements OnInit
{
  service = inject(HalfdaysService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_Halfdays,
      'Time.Schedule.HalfDays'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeHalfdayGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.description',
        'common.name',
        'core.edit',
        'time.schedule.daytype.daytype',
        'time.schedule.daytype.halfdaytype',
        'time.schedule.daytype.value',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 20 });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'typeName',
          terms['time.schedule.daytype.halfdaytype'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'value',
          terms['time.schedule.daytype.value'],
          { flex: 10, enableHiding: true }
        );
        this.grid.addColumnText(
          'dayTypeName',
          terms['time.schedule.daytype.daytype'],
          { flex: 15, enableHiding: true }
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
