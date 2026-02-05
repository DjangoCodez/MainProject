import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeRecalculateTimeHeadAction,
  TermGroup,
  TermGroup_RecalculateTimeHeadStatus,
  TermGroup_RecalculateTimeRecordStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IRecalculateTimeHeadDTO,
  IRecalculateTimeRecordDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { PlacementsRecalculateStatusDialogService } from '../services/placements-recalculate-status-dialog.service';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';

@Component({
  selector: 'soe-placements-recalculate-status-dialog-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
})
export class PlacementsRecalculateStatusDialogGridComponent
  extends GridBaseDirective<
    IRecalculateTimeHeadDTO,
    PlacementsRecalculateStatusDialogService
  >
  implements OnInit
{
  // Services
  readonly service = inject(PlacementsRecalculateStatusDialogService);
  private readonly coreService = inject(CoreService);
  private readonly messageboxService = inject(MessageboxService);

  // Data
  private heads: IRecalculateTimeHeadDTO[] = [];
  private status: SmallGenericType[] = [];

  // Signals
  private showHistory = signal(false);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_Placement,
      'Time.Schedule.Placements.Recalculate.Status',
      {
        lookups: [this.loadStatus()],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('showHistory', {
          labelKey: signal('time.recalculatetimestatus.showhistory'),
          checked: this.showHistory,
          onValueChanged: event =>
            this.showHistoryChanged((event as ToolbarCheckboxAction).value),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IRecalculateTimeHeadDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.status',
        'core.created',
        'core.createdby',
        'common.from',
        'common.to',
        'common.employee',
        'common.employees',
        'time.recalculatetimestatus.showhistory',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnIcon('masterRowStatusIcon', '', {
          useIconFromField: true,
          showIcon: () => true,
          editable: false,
          enableHiding: false,
          iconClassField: 'masterRowStatusIconClass',
          pinned: 'left',
        });
        this.grid.addColumnSelect(
          'status',
          terms['common.status'],
          this.status || [],
          null,
          {
            flex: 18,
          }
        );
        this.grid.addColumnText('length', terms['common.employees'], {
          flex: 10,
        });
        this.grid.addColumnDateTime('created', terms['core.created'], {
          flex: 18,
        });
        this.grid.addColumnText('createdBy', terms['core.createdby'], {
          flex: 18,
        });
        this.grid.addColumnDate('startDate', terms['common.from'], {
          flex: 18,
        });
        this.grid.addColumnDate('stopDate', terms['common.to'], {
          flex: 18,
        });
        if (this.flowHandler.modifyPermission()) {
          this.grid.addColumnIcon('masterRowActionIcon', '', {
            useIconFromField: true,
            showIcon: () => true,
            editable: false,
            enableHiding: false,
            pinned: 'right',
            iconClassField: 'masterRowActionIconClass',
            onClick: (row: IRecalculateTimeHeadDTO) =>
              this.clickMasterRowActionIcon(row),
          });
        }
        this.grid.enableMasterDetail(
          {
            columnDefs: [
              ColumnUtil.createColumnIcon('detailRowStatusIcon', '', {
                useIconFromField: true,
                showIcon: () => true,
                editable: false,
                enableHiding: false,
                pinned: 'left',
                iconClassField: 'detailRowStatusIconClass',
                iconAnimationField: 'detailRowStatusIconAnimation',
              }),
              ColumnUtil.createColumnText(
                'statusName',
                terms['common.status'],
                {
                  flex: 25,
                }
              ),
              ColumnUtil.createColumnText(
                'employeeName',
                terms['common.employee'],
                {
                  flex: 25,
                }
              ),
              ColumnUtil.createColumnDate('startDate', terms['common.from'], {
                flex: 25,
              }),
              ColumnUtil.createColumnDate('stopDate', terms['common.to'], {
                flex: 25,
              }),

              ColumnUtil.createColumnIcon('detailRowActionIcon', '', {
                useIconFromField: true,
                showIcon: () => true,
                editable: false,
                enableHiding: false,
                pinned: 'right',
                iconClassField: 'detailRowActionIconClass',
                onClick: (row: IRecalculateTimeRecordDTO) => {
                  this.clickDetailRowActionIcon(row);
                },
              }),
            ],
          },
          {
            autoHeight: false,
            getDetailRowData: (params: any) => {
              this.loadDetailRows(params);
            },
            suppressFiltering: false,
          }
        );
      });

    super.finalizeInitGrid();
  }

  //LOAD DATA
  private loadStatus(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.RecalculateTimeHeadStatus,
        false,
        true,
        true
      )
      .pipe(
        tap(x => {
          this.status = x;
        })
      );
  }

  private loadDetailRows(params: any) {
    params.successCallback(params.data.records);
  }

  override loadData(
    id?: number,
    additionalProps?: {
      recalculateAction?: SoeRecalculateTimeHeadAction;
      loadRecords?: boolean;
      showHistory?: boolean;
      setExtensionNames?: boolean;
      dateFrom?: Date;
      dateTo?: Date;
      limitNbrOfHeads?: number;
    }
  ): Observable<IRecalculateTimeHeadDTO[]> {
    return this.performLoadData.load$(
      this.service
        .getGrid(id, {
          showHistory: this.showHistory(),
        })
        .pipe(
          tap(x => {
            this.heads = x;
            x.forEach(head => {
              (head as any)['length'] = head.records.length;
              this.setHeadStatusTypeIcon(head);
              this.setHeadActionTypeIcon(head);

              const records: IRecalculateTimeRecordDTO[] = head.records;
              records.forEach(record => {
                this.setRecordStatusTypeIcon(record);
                this.setRecordActionTypeIcon(head, record);
              });
            });
          })
        )
    );
  }

  //EVENTS
  private showHistoryChanged(value: boolean) {
    this.showHistory.set(value);
    this.refreshGrid();
  }

  private clickMasterRowActionIcon(head: IRecalculateTimeHeadDTO): void {
    if (this.canCancelHead(head)) this.cancelHead(head);
    else if (this.canSetHeadToProcessed(head)) this.setHeadToProcessed(head);
  }

  private cancelHead(head: IRecalculateTimeHeadDTO) {
    this.translate
      .get([
        'time.recalculatetimestatus.cancelhead.ask.title',
        'time.recalculatetimestatus.cancelhead.ask.message',
        'time.recalculatetimestatus.cancel.error',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService
          .question(
            terms['time.recalculatetimestatus.cancelhead.ask.title'],
            terms['time.recalculatetimestatus.cancelhead.ask.message']
          )
          .afterClosed()
          .subscribe(q => {
            if (q.result) {
              this.service
                .cancelRecalculateTimeHead(head.recalculateTimeHeadId)
                .subscribe(cancelResult => {
                  if (!cancelResult.success) {
                    this.messageboxService.error(
                      terms['time.recalculatetimestatus.cancel.error'],
                      cancelResult.errorMessage
                    );
                  } else {
                    this.refreshGrid();
                  }
                });
            }
          });
      });
  }

  private setHeadToProcessed(head: IRecalculateTimeHeadDTO) {
    this.translate
      .get([
        'time.recalculatetimestatus.setheadtoprocessed.ask.title',
        'time.recalculatetimestatus.setheadtoprocessed.ask.message',
        'time.recalculatetimestatus.cancel.error',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService
          .question(
            terms['time.recalculatetimestatus.setheadtoprocessed.ask.title'],
            terms['time.recalculatetimestatus.setheadtoprocessed.ask.message']
          )
          .afterClosed()
          .subscribe(questionResult => {
            if (questionResult.result) {
              this.service
                .setRecalculateTimeHeadToProcessed(head.recalculateTimeHeadId)
                .subscribe(recalcResult => {
                  if (!recalcResult.success) {
                    this.messageboxService.error(
                      terms['time.recalculatetimestatus.cancel.error'],
                      recalcResult.errorMessage
                    );
                  } else {
                    this.refreshGrid();
                  }
                });
            }
          });
      });
  }

  private clickDetailRowActionIcon(record: IRecalculateTimeRecordDTO): void {
    const head = this.getHeadFromRecord(record);
    if (this.recordHasWarning(record)) {
      this.showWarningMessage(record);
    } else if (this.recordHasError(record)) {
      this.showErrorMessage(record);
    } else if (
      head &&
      this.canCancel(head, record) &&
      this.flowHandler.modifyPermission()
    ) {
      this.cancelRecord(record);
    }
  }

  private showWarningMessage(record: IRecalculateTimeRecordDTO) {
    this.translate
      .get(['time.recalculatetimestatus.warnings'])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService.warning(
          terms['time.recalculatetimestatus.warnings'],
          record.warningMsg
        );
      });
  }

  private showErrorMessage(record: IRecalculateTimeRecordDTO) {
    this.translate
      .get(['time.recalculatetimestatus.errors'])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService.error(
          terms['time.recalculatetimestatus.errors'],
          record.errorMsg
        );
      });
  }

  private cancelRecord(record: IRecalculateTimeRecordDTO) {
    return this.translate
      .get([
        'time.recalculatetimestatus.cancel',
        'time.recalculatetimestatus.cancel.ask',
        'time.recalculatetimestatus.cancel.error',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService
          .question(
            terms['time.recalculatetimestatus.cancel'],
            terms['time.recalculatetimestatus.cancel.ask']
          )
          .afterClosed()
          .subscribe(questionResult => {
            if (questionResult.result) {
              this.service
                .cancelRecalculateTimeRecord(record.recalculateTimeRecordId)
                .subscribe(cancelRecordResult => {
                  if (!cancelRecordResult.success) {
                    this.messageboxService.error(
                      terms['time.recalculatetimestatus.cancel.error'],
                      cancelRecordResult.errorMessage
                    );
                  } else {
                    this.refreshGrid();
                  }
                });
            }
          });
      });
  }

  //HELPER FUNCTIONS
  private setHeadStatusTypeIcon(head: IRecalculateTimeHeadDTO) {
    let statusIcon = '';
    let statusIconClass = '';

    switch (head.status) {
      case TermGroup_RecalculateTimeHeadStatus.Processed:
        statusIcon = 'check';
        break;
      case TermGroup_RecalculateTimeHeadStatus.Unprocessed:
        statusIcon = 'clock';
        break;
      case TermGroup_RecalculateTimeHeadStatus.Started:
      case TermGroup_RecalculateTimeHeadStatus.UnderProcessing:
        statusIcon = 'spinner';
        statusIconClass = 'icon-spinner';
        break;
      case TermGroup_RecalculateTimeHeadStatus.Error:
        statusIcon = 'exclamation-triangle';
        statusIconClass = 'errorColor';
        break;
      case TermGroup_RecalculateTimeHeadStatus.Cancelled:
        statusIcon = 'undo';
        statusIconClass = 'warningColor';
        break;
    }
    (head as any)['masterRowStatusIcon'] = statusIcon;
    (head as any)['masterRowStatusIconClass'] = statusIconClass;
  }

  private setHeadActionTypeIcon(head: IRecalculateTimeHeadDTO) {
    let actionIcon = '';
    let actionIconClass = '';

    if (this.canCancelHead(head)) {
      actionIconClass = 'icon-delete';
      actionIcon = 'times';
    } else if (this.canSetHeadToProcessed(head)) {
      actionIcon = 'check';
    }

    (head as any)['masterRowActionIcon'] = actionIcon;
    (head as any)['masterRowActionIconClass'] = actionIconClass;
  }

  private canCancelHead(head: IRecalculateTimeHeadDTO) {
    return (
      head.status === TermGroup_RecalculateTimeHeadStatus.Started &&
      head.created?.addDays(1).isBeforeOnDay(DateUtil.getToday()) &&
      head.records.filter(
        r =>
          r.recalculateTimeRecordStatus ===
          TermGroup_RecalculateTimeRecordStatus.Waiting
      ).length > 0
    );
  }

  private canSetHeadToProcessed(head: IRecalculateTimeHeadDTO) {
    return (
      head.status !== TermGroup_RecalculateTimeHeadStatus.Processed &&
      head.records.filter(
        r =>
          r.recalculateTimeRecordStatus !==
            TermGroup_RecalculateTimeRecordStatus.Processed &&
          r.recalculateTimeRecordStatus !==
            TermGroup_RecalculateTimeRecordStatus.Error &&
          r.recalculateTimeRecordStatus !==
            TermGroup_RecalculateTimeRecordStatus.Cancelled
      ).length === 0
    );
  }

  private getHeadFromRecord(
    record: IRecalculateTimeRecordDTO
  ): IRecalculateTimeHeadDTO | null {
    return (
      this.heads.find(
        head => head.recalculateTimeHeadId === record.recalculateTimeHeadId
      ) || null
    );
  }

  private canCancel(
    head: IRecalculateTimeHeadDTO,
    record: IRecalculateTimeRecordDTO
  ) {
    return (
      head &&
      record &&
      record.recalculateTimeRecordStatus ==
        TermGroup_RecalculateTimeRecordStatus.Waiting &&
      (head.status === TermGroup_RecalculateTimeHeadStatus.Processed ||
        head.status === TermGroup_RecalculateTimeHeadStatus.Error)
    );
  }

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

  private setRecordActionTypeIcon(
    head: IRecalculateTimeHeadDTO,
    record: IRecalculateTimeRecordDTO
  ) {
    let recordActionIcon = '';
    let recordActionIconClass = '';

    if (this.recordHasWarning(record)) {
      recordActionIconClass = 'warningColor';
      recordActionIcon = 'exclamation-circle';
    } else if (this.recordHasError(record)) {
      recordActionIconClass = 'errorColor';
      recordActionIcon = 'exclamation-triangle';
    } else if (this.canCancel(head, record)) {
      recordActionIconClass = 'icon-delete';
      recordActionIcon = 'times';
    }

    (record as any)['detailRowActionIcon'] = recordActionIcon;
    (record as any)['detailRowActionIconClass'] = recordActionIconClass;
  }

  private recordHasWarning(record: IRecalculateTimeRecordDTO) {
    return !!record.warningMsg;
  }

  private recordHasError(record: IRecalculateTimeRecordDTO) {
    return !!record.errorMsg;
  }
}
