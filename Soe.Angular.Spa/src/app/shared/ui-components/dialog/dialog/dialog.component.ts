import { Component, inject, signal } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { DialogData } from '../models/dialog';
import { Observable, firstValueFrom } from 'rxjs';
import { ButtonComponent } from '@ui/button/button/button.component';
import { IconModule } from '@ui/icon/icon.module';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'soe-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    DragDropModule,
    ButtonComponent,
    IconModule,
    TranslatePipe,
  ],
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.scss'],
})
export class DialogComponent<T extends DialogData> {
  data: T = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef);
  submitting = signal(false);

  constructor() {
    this.data.size = this.data.size || 'md';
    this.dialogRef.addPanelClass(`size-${this.data.size}`);

    if (this.data.hideCloseButton === undefined) {
      this.data.hideCloseButton = false;
    }
  }

  async triggerPrimaryAction() {
    this.submitting.set(true);
    try {
      await this.callAction();
      this.submitting.set(false);
      this.dialogRef.close(true);
    } catch (err) {
      this.submitting.set(false);
    }
  }

  async callAction() {
    if (!this.data.callbackAction) return;
    const cb = this.data.callbackAction();

    if (cb instanceof Observable) {
      await firstValueFrom(cb);
    } else if (cb instanceof Promise) {
      await cb;
    }
  }

  closeDialog(): void {
    this.dialogRef.close(false);
  }
}
