import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SelectUsersDialogComponent } from './components/select-users-dialog/select-users-dialog.component';
import { SelectUsersComponent } from './components/select-users/select-users.component';

@NgModule({
  declarations: [SelectUsersComponent, SelectUsersDialogComponent],
  imports: [
    CommonModule,
    GridWrapperComponent,
    ButtonComponent,
    CheckboxComponent,
    DialogComponent,
  ],
  exports: [SelectUsersDialogComponent],
})
export class SelectUsersDialogModule {}
