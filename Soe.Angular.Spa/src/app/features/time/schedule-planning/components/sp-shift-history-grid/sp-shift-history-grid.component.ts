import { Component, inject, input, model, OnInit, signal } from '@angular/core';
import { ShiftHistoryService } from '@features/time/schedule-planning/services/sp-shift-history.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITrackChangesLogDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';

@Component({
  selector: 'sp-shift-history-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
  standalone: true,
})
export class SpShiftHistoryGridComponent
  extends GridBaseDirective<ITrackChangesLogDTO, ShiftHistoryService>
  implements OnInit
{
  height = model(200);
  shiftIds = input<number[] | undefined>(undefined);
  service = inject(ShiftHistoryService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_SchedulePlanning_Beta, // TODO: Change when released
      'Time.Schedule.ShiftHistory'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITrackChangesLogDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.entitylogviewer.changelog.actionmethod',
        'common.entitylogviewer.changelog.batchnbr',
        'common.entitylogviewer.changelog.columnname',
        'common.from',
        'common.to',
        'common.modified',
        'common.modifiedby',
        'time.schedule.planning.shift',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'recordName',
          terms['time.schedule.planning.shift'],
          {
            flex: 20,
          }
        );
        this.grid.addColumnText(
          'actionMethodText',
          terms['common.entitylogviewer.changelog.actionmethod'],
          {
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'batchNbr',
          terms['common.entitylogviewer.changelog.batchnbr'],
          {
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'columnText',
          terms['common.entitylogviewer.changelog.columnname'],
          {
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('fromValueText', terms['common.from'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('toValueText', terms['common.to'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnDateTime('created', terms['common.modified'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('createdBy', terms['common.modifiedby'], {
          flex: 20,
          enableHiding: true,
        });

        this.grid.height = this.height;
        this.grid.setNbrOfRowsToShow(3, 10);
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<ITrackChangesLogDTO[]> {
    return this.service.getGrid(id, this.shiftIds());
  }
}
