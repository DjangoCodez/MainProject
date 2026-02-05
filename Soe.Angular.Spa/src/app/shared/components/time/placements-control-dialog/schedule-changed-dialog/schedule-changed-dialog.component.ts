import { Component, OnInit } from '@angular/core';
import { IActivateScheduleControlHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { ScheduleChangedDialogGridComponent } from './schedule-changed-dialog-grid/schedule-changed-dialog-grid/schedule-changed-dialog-grid.component';

export interface IScheduleChangedDialogData extends DialogData {
  scheduleChanges: IActivateScheduleControlHeadDTO;
}

@Component({
  selector: 'soe-schedule-changed-dialog',
  imports: [
    DialogComponent,
    ButtonComponent,
    ScheduleChangedDialogGridComponent,
  ],
  templateUrl: './schedule-changed-dialog.component.html',
  providers: [FlowHandlerService],
})
export class ScheduleChangedDialogComponent
  extends DialogComponent<IScheduleChangedDialogData>
  implements OnInit
{
  // Inputs
  public scheduleChanges = this.data?.scheduleChanges;

  ngOnInit(): void {}

  //EVENTS
  public cancel() {
    this.dialogRef.close();
  }
}
