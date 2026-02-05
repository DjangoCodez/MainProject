import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SelectProjectDialogComponent } from './components/select-project-dialog/select-project-dialog.component';
import { SelectProjectComponent } from './components/select-project/select-project.component';

@NgModule({
  declarations: [SelectProjectDialogComponent, SelectProjectComponent],
  imports: [
    CommonModule,
    SharedModule,
    CheckboxComponent,
    ButtonComponent,
    DialogComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
  ],
  exports: [SelectProjectDialogComponent],
})
export class SelectProjectDialogModule {}
