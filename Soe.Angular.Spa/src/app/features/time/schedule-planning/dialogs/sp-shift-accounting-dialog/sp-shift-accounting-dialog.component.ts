import { Component, OnInit } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { IconModule } from '@ui/icon/icon.module';
import { ReactiveFormsModule } from '@angular/forms';
import { SpShiftAccountingGridComponent } from '../../components/sp-shift-accounting-grid/sp-shift-accounting-grid.component';

export class SpShiftAccountingDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  shiftIds!: number[];
}

export class SpShiftAccountingDialogResult {
  shiftIds!: number[];
}

@Component({
  selector: 'sp-shift-accounting-dialog',
  styleUrls: ['./sp-shift-accounting-dialog.component.scss'],
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    DialogComponent,
    IconModule,
    SpShiftAccountingGridComponent,
  ],
  templateUrl: './sp-shift-accounting-dialog.component.html',
})
export class SpShiftAccountingDialogComponent
  extends DialogComponent<SpShiftAccountingDialogData>
  implements OnInit
{
  ngOnInit(): void {}

  close() {
    this.dialogRef.close({} as SpShiftAccountingDialogResult);
  }
}
