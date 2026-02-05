import { Component, OnInit, Type, effect, inject, signal } from '@angular/core';
import { DialogData } from '../models/dialog';
import { SoeFormGroup } from '@shared/extensions';
import {
  EditBaseDirective,
  IApiService,
} from '@shared/directives/edit-base/edit-base.directive';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { CommonModule } from '@angular/common';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export interface EditComponentDialogData<
  R,
  T extends IApiService<R>,
  FormType extends SoeFormGroup,
  EditParameters = unknown,
> extends DialogData<EditParameters> {
  form: FormType;
  editComponent: Type<EditBaseDirective<R, T, FormType>>;
}

@Component({
  selector: 'soe-edit-component-dialog',
  imports: [CommonModule, DialogComponent],
  templateUrl: './edit-component-dialog.component.html',
  styleUrls: ['./edit-component-dialog.component.scss'],
})
export class EditComponentDialogComponent<
  R,
  T extends IApiService<R>,
  FormType extends SoeFormGroup,
> implements OnInit
{
  data: EditComponentDialogData<R, T, FormType> = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef);
  closeDialogSignal = signal<BackendResponse | undefined>(undefined);

  form = this.data.form;
  editComponent = this.data.editComponent;
  editComponentInputs = {
    form: this.form,
    closeDialogSignal: this.closeDialogSignal,
  };

  constructor() {
    effect(() => {
      // Called from EditBaseDirective
      const closeDialog = this.closeDialogSignal();
      if (!closeDialog) return;

      this.dialogRef.close({ response: closeDialog, value: this.form.value });
    });
  }

  ngOnInit(): void {
    this.data.disableContentScroll = true;
  }
}
