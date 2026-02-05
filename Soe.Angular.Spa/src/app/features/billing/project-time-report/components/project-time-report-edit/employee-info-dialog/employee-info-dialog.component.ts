import { Component } from '@angular/core';
import { EmployeeInformationDialogDTO } from '@features/billing/project-time-report/models/project-time-report.model';
import { IEmployeeScheduleTransactionInfoDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';

@Component({
  templateUrl: './employee-info-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class EmployeeInfoDialogComponent extends DialogComponent<EmployeeInformationDialogDTO> {
  infoItem!: IEmployeeScheduleTransactionInfoDTO;

  constructor() {
    super();

    this.infoItem = this.data.data;
  }

  protected close() {
    this.dialogRef.close();
  }
}
