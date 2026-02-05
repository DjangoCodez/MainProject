import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { ValidationHandler } from '@shared/handlers';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeScheduleTaskGeneratedNeedDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogFooterComponent } from '@ui/footer/dialog-footer/dialog-footer.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import {
  GeneratedNeedsDialogData,
  GeneratedNeedsForm,
} from '../../models/generated-needs-form.model';
import { TimeScheduleTasksService } from '../../services/time-schedule-tasks.service';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-generated-needs',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DialogComponent,
    GridComponent,
    DialogFooterComponent,
  ],
  templateUrl: './generated-needs.component.html',
  styleUrl: './generated-needs.component.scss',
  providers: [FlowHandlerService, DialogService],
})
export class GeneratedNeedsComponent
  extends DialogComponent<GeneratedNeedsDialogData>
  implements OnInit
{
  service = inject(TimeScheduleTasksService);
  translate = inject(TranslateService);
  messageboxService = inject(MessageboxService);
  flowHandler = inject(FlowHandlerService);
  validationHandler = inject(ValidationHandler);

  form: GeneratedNeedsForm = new GeneratedNeedsForm({
    validationHandler: this.validationHandler,
  });

  subGrid!: GridComponent<ITimeScheduleTaskGeneratedNeedDTO>;
  subData = new BehaviorSubject<ITimeScheduleTaskGeneratedNeedDTO[]>([]);
  deleteEnabled = signal(false);

  ngOnInit(): void {
    this.flowHandler.options = {
      permission: Feature.Time_Schedule_StaffingNeeds,
      data: this.loadGeneratedNeeds(),
      onGridReadyToDefine: this.setupGrid.bind(this),
    };
    this.flowHandler.executeForGrid();
  }

  private loadGeneratedNeeds(): Observable<
    ITimeScheduleTaskGeneratedNeedDTO[]
  > {
    return this.service
      .getTimeScheduleTaskGeneratedNeeds(this.data.timeScheduleTaskId)
      .pipe(
        tap(x => {
          this.subData.next(x);
        })
      );
  }

  private setupGrid(grid: GridComponent<ITimeScheduleTaskGeneratedNeedDTO>) {
    this.subGrid = grid;
    this.translate
      .get([
        'common.type',
        'time.schedule.timescheduletask.generatedneed.occurs',
        'time.schedule.timescheduletask.generatedneed.rowid',
        'time.schedule.timescheduletask.starttime',
        'time.schedule.timescheduletask.stoptime',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.subGrid.addColumnNumber(
          'staffingNeedsRowId',
          terms['time.schedule.timescheduletask.generatedneed.rowid'],
          { flex: 20 }
        );
        this.subGrid.addColumnText('type', terms['common.type'], {
          flex: 20,
        });
        this.subGrid.addColumnText(
          'occurs',
          terms['time.schedule.timescheduletask.generatedneed.occurs'],
          {
            flex: 20,
          }
        );
        this.subGrid.addColumnTime(
          'startTime',
          terms['time.schedule.timescheduletask.starttime'],
          { flex: 20 }
        );
        this.subGrid.addColumnTime(
          'stopTime',
          terms['time.schedule.timescheduletask.stoptime'],
          { flex: 20 }
        );

        this.subGrid.context.suppressGridMenu = true;

        if (this.flowHandler.modifyPermission()) {
          this.subGrid.enableRowSelection();
          this.subGrid.onSelectionChanged = this.selectionChanged.bind(this);
        }

        this.subGrid.finalizeInitGrid();
      });
  }

  selectionChanged() {
    this.deleteEnabled.set(this.subGrid.getSelectedRows().length > 0);
  }

  cancel() {
    this.closeDialog();
  }

  initDelete() {
    this.translate
      .get([
        'time.schedule.timescheduletask.generatedneed.deleterows',
        'time.schedule.timescheduletask.generatedneed.deleterows.warning',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.messageboxService
          .question(
            terms['time.schedule.timescheduletask.generatedneed.deleterows'],
            terms[
              'time.schedule.timescheduletask.generatedneed.deleterows.warning'
            ].format(this.subGrid.getSelectedCount()),
            {
              type: 'warning',
              buttons: 'okCancel',
            }
          )
          .afterClosed()
          .subscribe(res => {
            if (res.result) {
              this.delete();
            }
          });
      });
  }

  private delete() {
    this.service
      .deleteGeneratedNeeds(
        this.subGrid.getSelectedIds('staffingNeedsRowPeriodId')
      )
      .subscribe(res => {
        if (res.success) {
          this.loadGeneratedNeeds().subscribe();
        } else {
          this.messageboxService.error(
            this.translate.instant(
              'time.schedule.timescheduletask.generatedneed.deleterows'
            ),
            ResponseUtil.getErrorMessage(res) ?? ''
          );
        }
      });
  }
}
