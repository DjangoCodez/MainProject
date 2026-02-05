import { Component, OnInit } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { IconModule } from '@ui/icon/icon.module';
import { ReactiveFormsModule } from '@angular/forms';
import { SpShiftHistoryGridComponent } from '../../components/sp-shift-history-grid/sp-shift-history-grid.component';

export class SpShiftHistoryDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  shiftIds!: number[];
}

export class SpShiftHistoryDialogResult {
  shiftIds!: number[];
}

@Component({
  selector: 'sp-shift-history-dialog',
  styleUrls: ['./sp-shift-history-dialog.component.scss'],
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    DialogComponent,
    IconModule,
    SpShiftHistoryGridComponent,
  ],
  templateUrl: './sp-shift-history-dialog.component.html',
})
export class SpShiftHistoryDialogComponent
  extends DialogComponent<SpShiftHistoryDialogData>
  implements OnInit
{
  ngOnInit(): void {}

  close() {
    this.dialogRef.close({} as SpShiftHistoryDialogResult);
  }
}
