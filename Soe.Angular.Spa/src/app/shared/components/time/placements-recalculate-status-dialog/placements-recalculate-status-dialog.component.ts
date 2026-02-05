import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { PlacementsRecalculateStatusDialogGridComponent } from './placements-recalculate-status-dialog-grid/placements-recalculate-status-dialog-grid.component';

@Component({
  selector: 'soe-placements-recalculate-status-dialog',
  imports: [
    DialogComponent,
    CommonModule,
    PlacementsRecalculateStatusDialogGridComponent,
    ButtonComponent,
  ],
  templateUrl: 'placements-recalculate-status-dialog.component.html',
  providers: [FlowHandlerService],
})
export class PlacementsRecalculateStatusDialogComponent extends DialogComponent<DialogData> {
  // EVENTS
  public cancel() {
    this.dialogRef.close();
  }
}
