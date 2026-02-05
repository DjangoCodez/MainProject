import { Component, inject, input, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IActivateScheduleControlRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { SpShiftHistoryDialogComponent } from '@features/time/schedule-planning/dialogs/sp-shift-history-dialog/sp-shift-history-dialog.component';

@Component({
  selector: 'soe-schedule-changed-dialog-grid',
  imports: [GridWrapperComponent],
  providers: [FlowHandlerService, ToolbarService],
  templateUrl:
    '../../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
})
export class ScheduleChangedDialogGridComponent
  extends GridBaseDirective<IActivateScheduleControlRowDTO>
  implements OnInit
{
  // Services
  private dialogService = inject(DialogService);

  // Inputs
  public rows = input.required<IActivateScheduleControlRowDTO[]>();

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_Placement,
      'Time.Schedule.Placements.Schedule.Changed',
      {
        skipInitialLoad: true,
        skipDefaultToolbar: true,
      }
    );

    this.initRows(this.rows());
  }

  private initRows(rows: IActivateScheduleControlRowDTO[]) {
    if (rows.length === 0) return;
    this.rowData.next(rows);
  }

  override loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return super.loadTerms([
      'common.date',
      'time.schedule.activate.schedulestarts',
      'time.schedule.activate.schedulestops',
      'time.schedule.activate.shiftstarts',
      'time.schedule.activate.shiftstops',
      'time.schedule.activate.showhistory',
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<IActivateScheduleControlRowDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.grid.addColumnDate('date', this.terms['common.date'], {
      flex: 20,
      suppressFilter: true,
      suppressFloatingFilter: true,
    });
    this.grid.addColumnTime(
      'scheduleStart',
      this.terms['time.schedule.activate.schedulestarts'],
      { flex: 20, suppressFilter: true, suppressFloatingFilter: true }
    );
    this.grid.addColumnTime(
      'scheduleStop',
      this.terms['time.schedule.activate.schedulestops'],
      { flex: 20, suppressFilter: true, suppressFloatingFilter: true }
    );
    this.grid.addColumnTime(
      'start',
      this.terms['time.schedule.activate.shiftstarts'],
      { flex: 20, suppressFilter: true, suppressFloatingFilter: true }
    );
    this.grid.addColumnTime(
      'stop',
      this.terms['time.schedule.activate.shiftstops'],
      { flex: 20, suppressFilter: true, suppressFloatingFilter: true }
    );
    this.grid.addColumnIcon(null, '', {
      iconName: 'info-circle',
      iconClass: 'information-color',
      tooltip: this.terms['time.schedule.activate.showhistory'],
      suppressFilter: true,
      suppressFloatingFilter: true,
      onClick: row => this.onShiftInformationClicked(row),
    });
    this.grid.setNbrOfRowsToShow(1, 3); // Adjusts height based on number of rows
    this.grid.context.suppressFiltering = true;
    super.finalizeInitGrid({ hidden: true });
  }

  private onShiftInformationClicked(row: IActivateScheduleControlRowDTO) {
    this.dialogService.open(SpShiftHistoryDialogComponent, {
      title: this.terms['time.schedule.activate.showhistory'],
      size: 'lg',
      shiftIds: [row.timeScheduleTemplateBlockId],
    });
  }
}
