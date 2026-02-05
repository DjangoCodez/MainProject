import { Component, effect, input, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup_RecalculateTimeRecordStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IRecalculateTimeRecordDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';

@Component({
  selector: 'soe-placements-pending-recalculation-dialog-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
})
export class PlacementsPendingRecalculationDialogGridComponent
  extends GridBaseDirective<IRecalculateTimeRecordDTO>
  implements OnInit
{
  // Inputs
  public records = input<IRecalculateTimeRecordDTO[]>([]);

  constructor() {
    super();

    effect((): void => {
      const records = this.records();
      this.initRows(records);
    });
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_Placement,
      'Time.Schedule.Placements.Pending.Recalculation',
      {
        skipInitialLoad: true,
        skipDefaultToolbar: true,
      }
    );
  }

  private initRows(rows: IRecalculateTimeRecordDTO[]) {
    this.setRecordProperties(rows);
    if (rows.length === 0) return;
    this.rowData.next(rows);
  }

  override onGridReadyToDefine(
    grid: GridComponent<IRecalculateTimeRecordDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get(['common.status', 'common.from', 'common.to', 'common.employee'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnIcon('detailRowStatusIcon', '', {
          useIconFromField: true,
          showIcon: () => true,
          editable: false,
          enableHiding: false,
          iconClassField: 'detailRowStatusIconClass',
          iconAnimationField: 'detailRowStatusIconAnimation',
        });
        this.grid.addColumnText('statusName', terms['common.status'], {
          flex: 25,
          // filterOptions: ['Schemalagd'],
          suppressFilter: true,
          suppressFloatingFilter: true,
        });
        this.grid.addColumnText('employeeName', terms['common.employee'], {
          flex: 25,
          suppressFilter: true,
          suppressFloatingFilter: true,
        });

        this.grid.addColumnDate('startDate', terms['common.from'], {
          flex: 25,
          suppressFilter: true,
          suppressFloatingFilter: true,
        });
        this.grid.addColumnDate('stopDate', terms['common.to'], {
          flex: 25,
          suppressFilter: true,
          suppressFloatingFilter: true,
        });
      });
    this.grid.setNbrOfRowsToShow(1, 7); // Adjusts height based on number of rows
    this.grid.context.suppressFiltering = true;
    super.finalizeInitGrid({ hidden: true });
  }

  //LOAD DATA
  private setRecordProperties(records: IRecalculateTimeRecordDTO[]) {
    records.forEach(record => {
      this.setRecordStatusTypeIcon(record);
    });
  }

  // HELPER METHODS
  private setRecordStatusTypeIcon(record: IRecalculateTimeRecordDTO) {
    let recordTypeIcon = '';
    let recordTypeIconClass = '';
    let recordTypeIconAnimation = '';
    switch (record.recalculateTimeRecordStatus) {
      case TermGroup_RecalculateTimeRecordStatus.Waiting:
        recordTypeIcon = 'snooze';
        break;
      case TermGroup_RecalculateTimeRecordStatus.Unprocessed:
        recordTypeIcon = 'clock';
        break;
      case TermGroup_RecalculateTimeRecordStatus.UnderProcessing:
        recordTypeIcon = 'spinner';
        recordTypeIconClass = 'icon-spinner';
        recordTypeIconAnimation = 'spin';
        break;
      case TermGroup_RecalculateTimeRecordStatus.Processed:
        recordTypeIcon = 'check';
        break;
      case TermGroup_RecalculateTimeRecordStatus.Error:
        recordTypeIcon = 'exclamation-triangle';
        recordTypeIconClass = 'errorColor';
        break;
      case TermGroup_RecalculateTimeRecordStatus.Cancelled:
        recordTypeIcon = 'undo';
        recordTypeIconClass = 'warningColor';
        break;
    }
    (record as any)['detailRowStatusIcon'] = recordTypeIcon;
    (record as any)['detailRowStatusIconClass'] = recordTypeIconClass;
    (record as any)['detailRowStatusIconAnimation'] = recordTypeIconAnimation;
  }
}
