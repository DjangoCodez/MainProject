import { Component, inject, input, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  TermGroup_ControlEmployeeSchedulePlacementType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IActivateScheduleControlHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import {
  IScheduleChangedDialogData,
  ScheduleChangedDialogComponent,
} from '../schedule-changed-dialog/schedule-changed-dialog.component';
import { BrowserUtil } from '@shared/util/browser-util';

@Component({
  selector: 'soe-placements-control-dialog-grid',
  imports: [GridWrapperComponent],
  providers: [FlowHandlerService, ToolbarService],
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
})
export class PlacementsControlDialogGridComponent
  extends GridBaseDirective<IActivateScheduleControlHeadDTO>
  implements OnInit
{
  // Services
  private readonly dialogService = inject(DialogService);

  // Inputs
  public heads = input.required<IActivateScheduleControlHeadDTO[]>();

  // Signals
  private modifyAbsenceRequests = signal(false);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_Placement,
      'Time.Schedule.Placements.Control',
      {
        skipDefaultToolbar: true,
        additionalModifyPermissions: [Feature.Time_Schedule_AbsenceRequests],
        skipInitialLoad: true,
      }
    );

    this.initRows(this.heads());
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.modifyAbsenceRequests.set(
      this.flowHandler.hasModifyAccess(Feature.Time_Schedule_AbsenceRequests)
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IActivateScheduleControlHeadDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.grid.addColumnText(
      'employeeNrAndName',
      this.terms['common.employee'],
      { editable: false, flex: 10, enableGrouping: true }
    );
    this.grid.addColumnDate('startDate', this.terms['common.startdate'], {
      editable: false,
      flex: 10,
    });
    this.grid.addColumnDate('stopDate', this.terms['common.stopdate'], {
      editable: false,
      flex: 10,
    });
    this.grid.addColumnText(
      'timeDeviationCauseName',
      this.terms['common.time.timedeviationcause'],
      {
        editable: false,
        flex: 10,
      }
    );
    this.grid.addColumnText('typeName', this.terms['common.type'], {
      editable: false,
      flex: 10,
    });
    this.grid.addColumnText('comment', this.terms['core.info'], {
      editable: false,
      flex: 14,
    });
    this.grid.addColumnText('statusName', this.terms['common.status'], {
      editable: false,
      flex: 10,
    });
    this.grid.addColumnText(
      'resultStatusName',
      this.terms['time.schedule.absencerequests.result'],
      {
        editable: false,
        flex: 10,
      }
    );

    this.grid.addColumnBool(
      'reActivateAbsenceRequest',
      this.terms['time.schedule.activate.reactivate'],
      {
        flex: 6,
        editable: true,
        showCheckbox: row => row?.type === 1 || false,
      }
    );
    if (this.modifyAbsenceRequests()) {
      this.grid.addColumnIcon(null, '', {
        iconName: 'info-circle',
        iconClass: 'information-color',
        tooltip: this.terms['core.info'],
        onClick: row => this.openInformation(row),
        showIcon: row => {
          return (
            row &&
            (row.type ==
              TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest ||
              row.type ==
                TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule)
          );
        },
      });
    }

    this.grid.useGrouping({
      includeFooter: false,
      includeTotalFooter: false,
      stickyGroupTotalRow: undefined,
      stickyGrandTotalRow: undefined,
      keepColumnsAfterGroup: true,
      selectChildren: false,
      groupSelectsFiltered: false,
      hideGroupPanel: true,
      suppressCount: true,
    });

    this.grid.groupRowsByColumn('employeeNrAndName', 'employeeNrAndName', 1);

    super.finalizeInitGrid();
  }

  // LOAD DATA
  override loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return super.loadTerms([
      'common.status',
      'common.startdate',
      'common.employee',
      'common.stopdate',
      'common.type',
      'common.time.timedeviationcause',
      'core.info',
      'time.schedule.activate.reactivate',
      'time.schedule.absencerequests.result',
    ]);
  }

  // EVENTS
  private openInformation(row: IActivateScheduleControlHeadDTO) {
    if (
      row.type ===
      TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest
    )
      this.openAbsenceRequest(row);
    else if (
      row.type ===
      TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule
    )
      this.openScheduleChange(row);
  }

  // HELPER METHODS
  private openAbsenceRequest(row: IActivateScheduleControlHeadDTO) {
    // For now, open in AngularJS.
    // TODO: Open Angular modal
    BrowserUtil.openInNewTab(
      window,
      `/soe/time/schedule/absencerequests/default.aspx?employeeRequestId=${row.employeeRequestId}#!/`
    );
  }

  private openScheduleChange(row: IActivateScheduleControlHeadDTO) {
    const dialogData: IScheduleChangedDialogData = {
      size: 'md',
      title:
        row.employeeNrAndName +
        ' ' +
        DateUtil.localeDateFormat(row.startDate) +
        ' - ' +
        DateUtil.localeDateFormat(row.stopDate),
      scheduleChanges: row,
    };
    this.dialogService.open(ScheduleChangedDialogComponent, dialogData);
  }

  private initRows(rows: IActivateScheduleControlHeadDTO[]) {
    if (rows.length === 0) return;
    this.rowData.next(rows);
  }
}
