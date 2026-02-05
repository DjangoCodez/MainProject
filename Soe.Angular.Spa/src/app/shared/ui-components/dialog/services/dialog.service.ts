import { Injectable, inject } from '@angular/core';
import {
  MatDialog,
  MatDialogConfig,
  MatDialogRef,
} from '@angular/material/dialog';
import { SoeFormGroup } from '@shared/extensions';
import { IApiService } from '@shared/directives/edit-base/edit-base.directive';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import {
  EditComponentDialogComponent,
  EditComponentDialogData,
} from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';

export declare interface ComponentType<T> {
  new (...args: any[]): T;
}

@Injectable({
  providedIn: 'root',
})
export class DialogService {
  dialog = inject(MatDialog);

  confirm({
    title = '',
    content = '',
    primaryText = '',
    secondaryText = '',
    size = 'md',
  }: Partial<DialogData>): MatDialogRef<DialogComponent<DialogData>> {
    return this.dialog.open(DialogComponent, {
      data: {
        title,
        content,
        primaryText,
        secondaryText,
        size,
      },
    });
  }

  open<T, TData extends DialogData = DialogData>(
    comp: ComponentType<T>,
    data: Partial<TData>
  ): MatDialogRef<T> {
    return this.dialog.open(comp, { data, ...this.extractConfig(data) });
  }

  openEditComponent<
    T extends IApiService<R>,
    R,
    FormType extends SoeFormGroup = SoeFormGroup,
  >(
    data: EditComponentDialogData<R, T, FormType>
  ): MatDialogRef<EditComponentDialogComponent<R, T, FormType>> {
    return this.dialog.open(EditComponentDialogComponent<R, T, FormType>, {
      data,
      ...this.extractConfig(data),
    });
  }

  private extractConfig<T extends MatDialogConfig>(
    data: T
  ): Partial<MatDialogConfig> {
    return {
      disableClose: data.disableClose,
      hasBackdrop: data.hasBackdrop ?? true,
    };
  }
}
