import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { IActivationResult } from '@features/time/placements/components/placements-grid/placements-grid-footer/placements-grid-footer.component';
import { PlacementsService } from '@features/time/placements/services/placements.service';
import { TranslateService } from '@ngx-translate/core';
import {
  IActivateScheduleControlDTO,
  IActivateScheduleGridDTO,
  IRecalculateTimeHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Guid } from '@shared/util/string-util';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import {
  InstructionComponent,
  InstructionType,
} from '@ui/instruction/instruction.component';
import { take } from 'rxjs';
import { PlacementsRecalculateStatusDialogService } from '../../placements-recalculate-status-dialog/services/placements-recalculate-status-dialog.service';
import { PlacementsPendingRecalculationDialogGridComponent } from '../placements-pending-recalculation-dialog-grid/placements-pending-recalculation-dialog-grid.component';

export interface IPlacementsPendingRecalculationDialogData extends DialogData {
  control: IActivateScheduleControlDTO;
  rows: IActivateScheduleGridDTO[];
  functionId: number;
  templateHeadId: number;
  templatePeriodId: number;
  startDate: Date;
  stopDate: Date;
  isPreliminary: boolean;
}
@Component({
  selector: 'soe-placements-pending-recalculation-dialog',
  imports: [
    DialogComponent,
    CommonModule,
    ButtonComponent,
    PlacementsPendingRecalculationDialogGridComponent,
    InstructionComponent,
  ],
  templateUrl: './placements-pending-recalculation-dialog.component.html',
  styleUrl: './placements-pending-recalculation-dialog.component.scss',
  providers: [FlowHandlerService],
})
export class PlacementsPendingRecalculationDialogComponent
  extends DialogComponent<IPlacementsPendingRecalculationDialogData>
  implements OnInit, OnDestroy
{
  // Services
  private readonly service = inject(PlacementsRecalculateStatusDialogService);
  private readonly placementService = inject(PlacementsService);
  private readonly translateService = inject(TranslateService);
  private readonly messageboxService = inject(MessageboxService);

  // Dialog Data
  private readonly control = this.data.control;
  private readonly rows = this.data.rows || [];
  private readonly functionId = this.data.functionId;
  private readonly templateHeadId = this.data.templateHeadId;
  private readonly templatePeriodId = this.data.templatePeriodId;
  private readonly startDate = this.data.startDate;
  private readonly stopDate = this.data.stopDate;
  private readonly isPreliminary = this.data.isPreliminary;

  // Signals
  private recalculateTimeHeadId = signal(0);
  private cancelPoll = signal(false);
  private failedEmployees = signal<string[]>([]);
  private activationSuccessful = signal(false);
  public activating = signal(false);
  public currentHead = signal<IRecalculateTimeHeadDTO | null>(null);
  public progressMessageType = signal<InstructionType>('info');
  public progressMessage = signal('');
  public failedEmployeesErrorMessage = signal<string[]>([]);

  // Timeout tracking for cleanup
  private pollTimeoutId?: ReturnType<typeof setTimeout>;
  private headTimeoutId?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.activate(this.data.rows);
  }

  ngOnDestroy(): void {
    this.cancelPoll.set(true);
    if (this.pollTimeoutId) {
      clearTimeout(this.pollTimeoutId);
    }
    if (this.headTimeoutId) {
      clearTimeout(this.headTimeoutId);
    }
  }

  // EVENTS
  public activate(rows: IActivateScheduleGridDTO[]) {
    this.activating.set(true);
    this.getRecalculateTimeHeadId(this.control.key);
    this.translateService
      .get([
        'time.recalculatetimestatus.activated',
        'time.recalculatetimestatus.continueactivating.ask',
        'time.recalculatetimestatus.continueactivating.message',
        'time.recalculatetimestatus.activationfailed',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.placementService
          .activateSchedule(
            this.control,
            rows,
            this.functionId,
            this.templateHeadId,
            this.templatePeriodId,
            this.startDate,
            this.stopDate,
            this.isPreliminary
          )
          .subscribe(result => {
            this.activating.set(false);
            if (result.success) {
              this.activationSuccessful.set(true);
              this.progressMessage.set(
                terms['time.recalculatetimestatus.activated']
              );
              this.progressMessageType.set('success');
              this.service
                .getRecalculateTimeHead(
                  this.recalculateTimeHeadId(),
                  true,
                  true
                )
                .subscribe(x => {
                  this.currentHead.set(x);
                });
            } else {
              this.progressMessage.set('');

              let rowsToActivateNext = rows;

              this.failedEmployeesErrorMessage().unshift(result.errorMessage);

              // Finds failed employee from error message as long as correct format '(123) name'
              // Dependent on error-message from backend. Some don't have this format, in that case whole activation will fail
              let failedEmployee = '';
              if (result.errorMessage.indexOf('(') !== -1) {
                failedEmployee = result.errorMessage.substring(
                  result.errorMessage.indexOf('('),
                  result.errorMessage.length
                );
              }
              if (failedEmployee !== '') {
                this.failedEmployees().unshift(failedEmployee);

                let failedEmployeeIndex = this.rows.findIndex(
                  e => e.employeNrAndName === failedEmployee
                );

                if (failedEmployeeIndex !== -1) {
                  rowsToActivateNext.splice(failedEmployeeIndex, 1);
                }

                if (rowsToActivateNext.length > 0) {
                  this.messageboxService
                    .question(
                      terms[
                        'time.recalculatetimestatus.continueactivating.ask'
                      ],
                      result.errorMessage +
                        '\n' +
                        terms[
                          'time.recalculatetimestatus.continueactivating.message'
                        ]
                    )
                    .afterClosed()
                    .subscribe(r => {
                      if (r.result) this.activate(rowsToActivateNext);
                    });
                } else if (this.activationSuccessful() === false) {
                  // If activation didn't succeed for any employees
                  this.progressMessage.set(
                    terms['time.recalculatetimestatus.activationfailed']
                  );
                  this.progressMessageType.set('warning');
                }
              }
            }
          });
      });
  }

  public cancel() {
    this.cancelPoll.set(true);
    this.dialogRef.close({
      activationSuccessful: this.activationSuccessful(),
    } as IActivationResult);
  }

  // LOAD DATA
  private getRecalculateTimeHeadId(key: Guid) {
    this.service.getRecalculateTimeHeadId(key).subscribe(id => {
      if (id) {
        this.recalculateTimeHeadId.set(id);
        this.getRecalculateTimeHead();
      } else if (!this.cancelPoll()) {
        this.pollTimeoutId = setTimeout(() => {
          this.getRecalculateTimeHeadId(key);
        }, 1000);
      }
    });
  }

  private getRecalculateTimeHead() {
    this.service
      .getRecalculateTimeHead(this.recalculateTimeHeadId(), true, true)
      .subscribe(x => {
        this.translateService
          .get(['time.recalculatetimestatus.activating'])
          .pipe(take(1))
          .subscribe(terms => {
            if (!this.currentHead()) {
              const activatingMessage = terms[
                'time.recalculatetimestatus.activating'
              ] as string;
              this.progressMessage.set(
                activatingMessage
                  ? activatingMessage
                      .replace(/\\n/g, '\n')
                      .replace(/\n\s*\n/g, '\n')
                      .trim()
                  : '' // Fixes linebreaks and collapses multiple newlines with whitespace
              );
              this.progressMessageType.set('info');
            }
            this.currentHead.set(x);

            if (this.activating() && !this.cancelPoll()) {
              this.headTimeoutId = setTimeout(() => {
                this.getRecalculateTimeHead();
              }, 15000);
            }
          });
      });
  }
}
